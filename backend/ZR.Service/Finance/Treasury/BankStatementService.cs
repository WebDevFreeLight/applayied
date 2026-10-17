using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.PaymentHub.Enums;
using ZR.Repository.Finance.Treasury;
using ZR.Repository.Finance.PaymentHub;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Service - خدمة كشف الحساب البنكي
    /// Enhanced with MT940/CSV parsing, preview mode, and auto-matching
    /// </summary>
    [AppService(ServiceType = typeof(IBankStatementService), ServiceLifetime = LifeTime.Transient)]
    public class BankStatementService : BaseService<BankStatement>, IBankStatementService
    {
        private readonly BankStatementRepository _repository;
        private readonly BankStatementLineRepository _lineRepository;
        private readonly PaymentHubRepository _paymentRepository;
        private readonly BankParsingProfileRepository _parsingProfileRepository;
        private readonly ChequeRepository _chequeRepository;
        private readonly IBankStatementParserService _parserService;
        private readonly ITreasuryAuditService _auditService;

        public BankStatementService(
            BankStatementRepository repository,
            BankStatementLineRepository lineRepository,
            PaymentHubRepository paymentRepository,
            BankParsingProfileRepository parsingProfileRepository,
            ChequeRepository chequeRepository,
            IBankStatementParserService parserService,
            ITreasuryAuditService auditService)
        {
            _repository = repository;
            _lineRepository = lineRepository;
            _paymentRepository = paymentRepository;
            _parsingProfileRepository = parsingProfileRepository;
            _chequeRepository = chequeRepository;
            _parserService = parserService;
            _auditService = auditService;
        }

        public PagedInfo<BankStatementDetailDto> GetList(BankStatementQueryDto parm)
        {
            var predicate = Expressionable.Create<BankStatement>();

            if (parm.TenantId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == (long)parm.TenantId.Value.GetHashCode());
            }

            if (parm.BankAccountId.HasValue)
            {
                predicate = predicate.And(x => x.BankAccountId == parm.BankAccountId.Value);
            }

            if (parm.StatementDateFrom.HasValue)
            {
                predicate = predicate.And(x => x.StatementDate >= parm.StatementDateFrom.Value);
            }

            if (parm.StatementDateTo.HasValue)
            {
                predicate = predicate.And(x => x.StatementDate <= parm.StatementDateTo.Value);
            }

            if (parm.Imported.HasValue)
            {
                predicate = predicate.And(x => x.Imported == parm.Imported.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);
            return response.Adapt<PagedInfo<BankStatementDetailDto>>();
        }

        public BankStatementDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }
            return entity.Adapt<BankStatementDetailDto>();
        }

        public BankStatement Create(CreateBankStatementDto dto, string userId)
        {
            var entity = dto.Adapt<BankStatement>();
            entity.Id = Guid.NewGuid();
            entity.Imported = false;
            entity.Create_time = DateTime.Now;

            if (!string.IsNullOrEmpty(userId))
            {
                entity.Create_by = userId;
            }

            _repository.Insert(entity);
            return entity;
        }

        public BankStatement ImportBankStatement(UploadBankStatementDto dto, byte[] fileContent, string userId)
        {
            // Get parsing profile if specified
            BankParsingProfile profile = null;
            if (dto.ParsingProfileId.HasValue)
            {
                profile = _parsingProfileRepository.GetById(dto.ParsingProfileId.Value);
            }

            // Parse the file using the parser service
            var parseResult = _parserService.Parse(fileContent, dto.ImportFormat, profile);
            if (!parseResult.Success)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_STATEMENT_PARSE_FAILED)
                    .WithParameter("Error", parseResult.ErrorMessage ?? string.Empty);
            }

            // Validate the parsed result
            var validationResult = _parserService.Validate(parseResult);
            if (!validationResult.IsValid)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_STATEMENT_VALIDATION_FAILED)
                    .WithParameter("Errors", string.Join(", ", validationResult.Errors ?? new List<string>()));
            }

            BankStatement statement;

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                // Convert BankStatementFormat to BankStatementImportSource
                var importSource = dto.ImportFormat switch
                {
                    BankStatementFormat.GenericCSV => BankStatementImportSource.CSV,
                    BankStatementFormat.MT940 => BankStatementImportSource.MT940,
                    BankStatementFormat.CAMT053 => BankStatementImportSource.API, // Use API as closest match
                    _ => BankStatementImportSource.Manual
                };

                // Create the statement header
                statement = new BankStatement
                {
                    Id = Guid.NewGuid(),
                    BankAccountId = dto.BankAccountId,
                    StatementDate = parseResult.ClosingBalanceDate ?? DateTime.Now,
                    StatementReference = parseResult.TransactionReferenceNumber ?? dto.FileName,
                    OpeningBalance = parseResult.OpeningBalance,
                    ClosingBalance = parseResult.ClosingBalance,
                    Imported = true,
                    ImportSource = importSource,
                    Notes = dto.Notes,
                    Create_time = DateTime.Now,
                    Create_by = userId
                };

                _repository.Insert(statement);

                // Create statement lines
                foreach (var parsedLine in parseResult.Lines)
                {
                    var statementLine = new BankStatementLine
                    {
                        Id = Guid.NewGuid(),
                        BankStatementId = statement.Id,
                        LineDate = parsedLine.TransactionDate,
                        ValueDate = parsedLine.ValueDate ?? parsedLine.TransactionDate,
                        Amount = parsedLine.IsCredit ? parsedLine.Amount : -parsedLine.Amount,
                        Description = parsedLine.Description,
                        ReferenceNo = parsedLine.Reference,
                        RunningBalance = parsedLine.RunningBalance ?? 0,
                        Matched = false,
                        Create_time = DateTime.Now,
                        Create_by = userId
                    };

                    _lineRepository.Insert(statementLine);
                }

                // Log the import
                _auditService.LogCreate(statement, TreasuryEntityType.BankStatement,
                    statement.StatementReference, userId, null, null);

                scope.Complete();
            }

            // Auto-match after import
            AutoMatchLines(statement.Id);

            return statement;
        }

        /// <summary>
        /// Preview bank statement import (dry-run) - معاينة استيراد كشف الحساب
        /// </summary>
        public BankStatementPreviewResult PreviewImport(UploadBankStatementDto dto, byte[] fileContent)
        {
            // Get parsing profile if specified
            BankParsingProfile profile = null;
            if (dto.ParsingProfileId.HasValue)
            {
                profile = _parsingProfileRepository.GetById(dto.ParsingProfileId.Value);
            }

            // Get preview from parser
            var preview = _parserService.Preview(fileContent, dto.ImportFormat, profile);

            // Enhance with suggested matches from database
            if (preview.ParseResult != null && preview.ParseResult.Success)
            {
                foreach (var line in preview.ParseResult.Lines)
                {
                    var suggestedMatch = FindBestMatch(dto.BankAccountId, line);
                    if (suggestedMatch != null)
                    {
                        preview.SuggestedMatches.Add(suggestedMatch);
                    }
                }

                // Count potential duplicates
                preview.PotentialDuplicates = CountPotentialDuplicates(dto.BankAccountId, preview.ParseResult.Lines);
                preview.NewTransactions = preview.TotalTransactions - preview.PotentialDuplicates;
            }

            return preview;
        }

        private SuggestedMatch FindBestMatch(Guid bankAccountId, ParsedBankStatementLine line)
        {
            // Try exact match by reference and amount
            var paymentPredicate = Expressionable.Create<Payment>();
            paymentPredicate = paymentPredicate.And(x => x.SourceAccountId == bankAccountId || x.DestinationAccountId == bankAccountId);
            paymentPredicate = paymentPredicate.And(x => x.Status == Model.Finance.PaymentHub.Enums.PaymentStatus.Posted);

            var potentialMatches = _paymentRepository.GetList(paymentPredicate.ToExpression());

            // Score each potential match
            Payment bestMatch = null;
            decimal bestConfidence = 0;
            string matchType = null;

            foreach (var payment in potentialMatches)
            {
                decimal confidence = 0;

                // Exact reference match
                if (!string.IsNullOrEmpty(line.Reference) &&
                    string.Equals(payment.ReferenceNumber, line.Reference, StringComparison.OrdinalIgnoreCase))
                {
                    confidence += 50;
                    matchType = "Reference";
                }

                // Amount match
                if (Math.Abs(payment.Amount - line.Amount) < 0.01m)
                {
                    confidence += 40;
                    matchType = confidence > 50 ? "Exact" : "Amount";
                }

                // Date proximity (within 3 days)
                if (Math.Abs((payment.PaymentDate - line.TransactionDate).TotalDays) <= 3)
                {
                    confidence += 10;
                }

                if (confidence > bestConfidence)
                {
                    bestConfidence = confidence;
                    bestMatch = payment;
                }
            }

            if (bestMatch != null && bestConfidence >= 40)
            {
                return new SuggestedMatch
                {
                    LineNumber = line.LineNumber,
                    MatchedPaymentId = bestMatch.Id,
                    MatchedPaymentNumber = bestMatch.PaymentNumber,
                    Confidence = bestConfidence,
                    MatchType = matchType
                };
            }

            return null;
        }

        private int CountPotentialDuplicates(Guid bankAccountId, List<ParsedBankStatementLine> lines)
        {
            int duplicates = 0;

            foreach (var line in lines)
            {
                // Check if a statement line with same date, amount, and reference already exists
                var existingPredicate = Expressionable.Create<BankStatementLine>();
                existingPredicate = existingPredicate.And(x => x.LineDate.Date == line.TransactionDate.Date);
                existingPredicate = existingPredicate.And(x => Math.Abs(x.Amount - (line.IsCredit ? line.Amount : -line.Amount)) < 0.01m);

                if (!string.IsNullOrEmpty(line.Reference))
                {
                    existingPredicate = existingPredicate.And(x => x.ReferenceNo == line.Reference);
                }

                var existing = _lineRepository.GetFirst(existingPredicate.ToExpression());
                if (existing != null)
                {
                    duplicates++;
                }
            }

            return duplicates;
        }

        public List<BankStatementLineDto> GetStatementLines(Guid statementId)
        {
            var predicate = Expressionable.Create<BankStatementLine>();
            predicate = predicate.And(x => x.BankStatementId == statementId);

            var lines = _lineRepository.GetList(predicate.ToExpression());
            return lines.Adapt<List<BankStatementLineDto>>();
        }

        public List<BankStatementLineDto> GetUnmatchedLines(Guid bankAccountId, long? companyId = null)
        {
            var predicate = Expressionable.Create<BankStatementLine>();
            predicate = predicate.And(x => x.Matched == false);

            // Get statement IDs for this bank account
            var statementPredicate = Expressionable.Create<BankStatement>();
            statementPredicate = statementPredicate.And(x => x.BankAccountId == bankAccountId);
            if (companyId.HasValue)
            {
                statementPredicate = statementPredicate.And(x => x.CompanyID == companyId.Value);
            }

            var statements = _repository.GetList(statementPredicate.ToExpression());
            var statementIds = statements.Select(s => s.Id).ToList();

            predicate = predicate.And(x => statementIds.Contains(x.BankStatementId));

            var lines = _lineRepository.GetList(predicate.ToExpression());
            return lines.Adapt<List<BankStatementLineDto>>();
        }

        public int AutoMatchLines(Guid statementId)
        {
            var lines = GetStatementLines(statementId);
            int matchedCount = 0;

            foreach (var line in lines.Where(l => !l.Matched))
            {
                // Try to match by reference number and amount
                var paymentPredicate = Expressionable.Create<Payment>();
                paymentPredicate = paymentPredicate.And(x => x.ReferenceNumber == line.ReferenceNo);
                paymentPredicate = paymentPredicate.And(x => Math.Abs(x.Amount - Math.Abs(line.Amount)) < 0.01m);
                paymentPredicate = paymentPredicate.And(x => x.Status == Model.Finance.PaymentHub.Enums.PaymentStatus.Posted);
                
                var payments = _paymentRepository.GetList(paymentPredicate.ToExpression());
                var payment = payments.FirstOrDefault();

                if (payment != null)
                {
                    var lineEntity = _lineRepository.GetById(line.Id);
                    if (lineEntity != null)
                    {
                        lineEntity.Matched = true;
                        lineEntity.MatchedTransactionId = payment.Id;
                        _lineRepository.Update(lineEntity);
                        matchedCount++;
                    }
                }
            }

            return matchedCount;
        }

        public int MatchLine(MatchBankStatementLineDto dto, string userId)
        {
            var line = _lineRepository.GetById(dto.BankStatementLineId);
            if (line == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_STATEMENT_LINE_NOT_FOUND)
                    .WithParameter("BankStatementLineId", dto.BankStatementLineId);
            }

            var payment = _paymentRepository.GetById(dto.MatchedTransactionId);
            if (payment == null)
            {
                throw new AppException(ExceptionCode.FINANCE_PAYMENT_NOT_FOUND)
                    .WithParameter("PaymentId", dto.MatchedTransactionId);
            }

            line.Matched = true;
            line.MatchedTransactionId = dto.MatchedTransactionId;

            return _lineRepository.Update(line);
        }
    }
}


