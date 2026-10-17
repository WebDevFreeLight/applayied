using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Infrastructure.Attribute;
using NLog;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Parser Service - خدمة تحليل كشف الحساب البنكي
    /// Supports CSV and MT940 SWIFT format parsing
    /// </summary>
    [AppService(ServiceType = typeof(IBankStatementParserService), ServiceLifetime = LifeTime.Transient)]
    public class BankStatementParserService : IBankStatementParserService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        #region Public Methods

        public BankStatementParseResult Parse(byte[] fileContent, BankStatementFormat format, BankParsingProfile profile = null)
        {
            return format switch
            {
                BankStatementFormat.MT940 => ParseMT940(fileContent),
                BankStatementFormat.GenericCSV => ParseCSV(fileContent, profile),
                BankStatementFormat.CAMT053 => ParseCAMT053(fileContent),
                _ => new BankStatementParseResult
                {
                    Success = false,
                    ErrorMessage = $"Unsupported format: {format}"
                }
            };
        }

        public BankStatementParseResult ParseCSV(byte[] fileContent, BankParsingProfile profile)
        {
            var result = new BankStatementParseResult();

            try
            {
                var encoding = GetEncoding(profile?.Encoding ?? "UTF-8");
                var content = encoding.GetString(fileContent);
                var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                var delimiter = profile?.CsvDelimiter ?? ",";
                var hasHeader = profile?.HasHeaderRow ?? true;
                var skipRows = profile?.SkipRows ?? 0;
                var dateFormat = profile?.DateFormat ?? "yyyy-MM-dd";

                // Parse column mapping
                var columnMapping = ParseColumnMapping(profile?.ColumnMapping);

                var lineNumber = 0;
                var startLine = skipRows + (hasHeader ? 1 : 0);

                for (int i = startLine; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    lineNumber++;
                    var columns = ParseCSVLine(line, delimiter[0]);

                    try
                    {
                        var parsedLine = ParseCSVLineToStatement(columns, columnMapping, dateFormat, profile, lineNumber);
                        parsedLine.RawData = line;
                        result.Lines.Add(parsedLine);

                        // Update totals
                        if (parsedLine.IsCredit)
                        {
                            result.TotalCredits += parsedLine.Amount;
                            result.CreditCount++;
                        }
                        else
                        {
                            result.TotalDebits += parsedLine.Amount;
                            result.DebitCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Line {lineNumber}: {ex.Message}");
                    }
                }

                result.Success = result.Lines.Count > 0;
                if (!result.Success && result.Warnings.Count > 0)
                {
                    result.ErrorMessage = "Failed to parse any lines from CSV file";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"CSV parsing error: {ex.Message}";
                _logger.Error(ex, "Error parsing CSV bank statement");
            }

            return result;
        }

        public BankStatementParseResult ParseMT940(byte[] fileContent)
        {
            var result = new BankStatementParseResult();

            try
            {
                var content = Encoding.ASCII.GetString(fileContent);

                // MT940 uses specific tags
                // :20: - Transaction Reference Number
                // :25: - Account Identification
                // :28C: - Statement Number/Sequence Number
                // :60F: or :60M: - Opening Balance
                // :61: - Statement Line (transaction)
                // :86: - Information to Account Owner (details)
                // :62F: or :62M: - Closing Balance
                // :64: - Available Balance (optional)

                // Parse header tags
                result.TransactionReferenceNumber = ExtractMT940Tag(content, ":20:");
                result.AccountIdentification = ExtractMT940Tag(content, ":25:");

                var statementSeq = ExtractMT940Tag(content, ":28C:");
                if (!string.IsNullOrEmpty(statementSeq))
                {
                    var parts = statementSeq.Split('/');
                    result.StatementNumber = parts[0];
                    result.SequenceNumber = parts.Length > 1 ? parts[1] : null;
                }

                // Parse opening balance (:60F: or :60M:)
                var openingBalanceRaw = ExtractMT940Tag(content, ":60F:") ?? ExtractMT940Tag(content, ":60M:");
                if (!string.IsNullOrEmpty(openingBalanceRaw))
                {
                    ParseMT940Balance(openingBalanceRaw, out var isCredit, out var date, out var currency, out var amount);
                    result.OpeningBalanceIsCredit = isCredit;
                    result.OpeningBalanceDate = date;
                    result.OpeningBalanceCurrency = currency;
                    result.OpeningBalance = amount;
                }

                // Parse closing balance (:62F: or :62M:)
                var closingBalanceRaw = ExtractMT940Tag(content, ":62F:") ?? ExtractMT940Tag(content, ":62M:");
                if (!string.IsNullOrEmpty(closingBalanceRaw))
                {
                    ParseMT940Balance(closingBalanceRaw, out var isCredit, out var date, out var currency, out var amount);
                    result.ClosingBalanceIsCredit = isCredit;
                    result.ClosingBalanceDate = date;
                    result.ClosingBalanceCurrency = currency;
                    result.ClosingBalance = amount;
                }

                // Parse available balance (:64:)
                var availableBalanceRaw = ExtractMT940Tag(content, ":64:");
                if (!string.IsNullOrEmpty(availableBalanceRaw))
                {
                    ParseMT940Balance(availableBalanceRaw, out _, out var date, out _, out var amount);
                    result.AvailableBalance = amount;
                    result.AvailableBalanceDate = date;
                }

                // Parse transactions (:61: tags followed by :86:)
                var transactions = ExtractMT940Transactions(content);
                int lineNumber = 0;

                foreach (var (tag61, tag86) in transactions)
                {
                    lineNumber++;
                    try
                    {
                        var parsedLine = ParseMT940Transaction(tag61, tag86, lineNumber);
                        parsedLine.Currency = result.OpeningBalanceCurrency;
                        result.Lines.Add(parsedLine);

                        if (parsedLine.IsCredit)
                        {
                            result.TotalCredits += parsedLine.Amount;
                            result.CreditCount++;
                        }
                        else
                        {
                            result.TotalDebits += parsedLine.Amount;
                            result.DebitCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Transaction {lineNumber}: {ex.Message}");
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"MT940 parsing error: {ex.Message}";
                _logger.Error(ex, "Error parsing MT940 bank statement");
            }

            return result;
        }

        public BankStatementValidationResult Validate(BankStatementParseResult result)
        {
            var validation = new BankStatementValidationResult
            {
                IsValid = true
            };

            if (result == null || !result.Success)
            {
                validation.IsValid = false;
                validation.Errors.Add("Parse result is null or failed");
                return validation;
            }

            // Validate balance reconciliation
            if (result.OpeningBalanceDate.HasValue && result.ClosingBalanceDate.HasValue)
            {
                var calculatedClosing = result.OpeningBalance;
                calculatedClosing += result.TotalCredits;
                calculatedClosing -= result.TotalDebits;

                validation.CalculatedClosingBalance = calculatedClosing;
                validation.BalanceDifference = result.ClosingBalance - calculatedClosing;
                validation.BalanceReconciles = Math.Abs(validation.BalanceDifference) < 0.01m;

                if (!validation.BalanceReconciles)
                {
                    validation.Warnings.Add($"Balance does not reconcile. Difference: {validation.BalanceDifference:N2}");
                }

                validation.OpeningBalanceValid = result.OpeningBalance != 0 || result.Lines.Count == 0;
                validation.ClosingBalanceValid = true;
            }

            // Validate individual lines
            foreach (var line in result.Lines)
            {
                bool lineValid = true;

                if (line.TransactionDate == default)
                {
                    validation.Errors.Add($"Line {line.LineNumber}: Missing transaction date");
                    lineValid = false;
                }

                if (line.Amount <= 0)
                {
                    validation.Errors.Add($"Line {line.LineNumber}: Invalid amount ({line.Amount})");
                    lineValid = false;
                }

                if (string.IsNullOrEmpty(line.Description) && string.IsNullOrEmpty(line.Reference))
                {
                    validation.Warnings.Add($"Line {line.LineNumber}: No description or reference");
                }

                if (lineValid)
                {
                    validation.ValidLines++;
                }
                else
                {
                    validation.InvalidLines++;
                    validation.LinesWithErrors.Add(line.LineNumber);
                }
            }

            validation.IsValid = validation.InvalidLines == 0 && validation.Errors.Count == 0;
            return validation;
        }

        public BankStatementPreviewResult Preview(byte[] fileContent, BankStatementFormat format, BankParsingProfile profile = null)
        {
            var preview = new BankStatementPreviewResult();

            // Parse the file
            preview.ParseResult = Parse(fileContent, format, profile);

            if (!preview.ParseResult.Success)
            {
                preview.CanImport = false;
                return preview;
            }

            // Validate
            preview.ValidationResult = Validate(preview.ParseResult);
            preview.CanImport = preview.ValidationResult.IsValid;

            // Build summary
            preview.TotalTransactions = preview.ParseResult.Lines.Count;
            preview.TotalDebits = preview.ParseResult.TotalDebits;
            preview.TotalCredits = preview.ParseResult.TotalCredits;
            preview.Currency = preview.ParseResult.OpeningBalanceCurrency;

            if (preview.ParseResult.Lines.Count > 0)
            {
                preview.StatementStartDate = preview.ParseResult.Lines.Min(l => l.TransactionDate);
                preview.StatementEndDate = preview.ParseResult.Lines.Max(l => l.TransactionDate);
            }

            // Note: SuggestedMatches would be populated by comparing against existing payments in the database
            // This would require injecting PaymentRepository, but keeping parser stateless for now

            return preview;
        }

        public List<BankStatementFormat> GetSupportedFormats()
        {
            return new List<BankStatementFormat>
            {
                BankStatementFormat.GenericCSV,
                BankStatementFormat.MT940,
                BankStatementFormat.CAMT053
            };
        }

        #endregion

        #region MT940 Parsing Helpers

        private string ExtractMT940Tag(string content, string tag)
        {
            var pattern = $@"{Regex.Escape(tag)}([^\r\n:]+)";
            var match = Regex.Match(content, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private void ParseMT940Balance(string balanceData, out bool isCredit, out DateTime date, out string currency, out decimal amount)
        {
            // Format: C/D YYMMDD CURRENCY AMOUNT
            // Example: C230315SAR1234,56
            isCredit = balanceData.StartsWith("C");

            var dateStr = balanceData.Substring(1, 6);
            date = ParseMT940Date(dateStr);

            currency = balanceData.Substring(7, 3);

            var amountStr = balanceData.Substring(10).Replace(",", ".");
            amount = decimal.Parse(amountStr, CultureInfo.InvariantCulture);
        }

        private DateTime ParseMT940Date(string dateStr)
        {
            // Format: YYMMDD
            if (dateStr.Length == 6)
            {
                var year = 2000 + int.Parse(dateStr.Substring(0, 2));
                var month = int.Parse(dateStr.Substring(2, 2));
                var day = int.Parse(dateStr.Substring(4, 2));
                return new DateTime(year, month, day);
            }
            return DateTime.MinValue;
        }

        private List<(string tag61, string tag86)> ExtractMT940Transactions(string content)
        {
            var transactions = new List<(string, string)>();

            // Pattern to match :61: followed optionally by :86:
            var pattern = @":61:([^\r\n]+(?:\r?\n(?!:)[^\r\n]+)*)(?:\r?\n:86:([^\r\n]+(?:\r?\n(?!:)[^\r\n]+)*))?";
            var matches = Regex.Matches(content, pattern);

            foreach (Match match in matches)
            {
                var tag61 = match.Groups[1].Value.Trim();
                var tag86 = match.Groups.Count > 2 ? match.Groups[2].Value.Trim() : null;
                transactions.Add((tag61, tag86));
            }

            return transactions;
        }

        private ParsedBankStatementLine ParseMT940Transaction(string tag61, string tag86, int lineNumber)
        {
            var line = new ParsedBankStatementLine
            {
                LineNumber = lineNumber,
                RawData = $":61:{tag61}\n:86:{tag86}"
            };

            // :61: format: YYMMDD[MMDD]C/DAMOUNT[N]TYPEREFERENCE//BANKREF
            // Example: 2303150315CR1000,00NTRFPAYMENT//BANK123456

            if (tag61.Length < 16)
            {
                line.ParseWarnings.Add("Invalid :61: tag format");
                return line;
            }

            // Parse value date (YYMMDD)
            var valueDateStr = tag61.Substring(0, 6);
            line.ValueDate = ParseMT940Date(valueDateStr);
            line.TransactionDate = line.ValueDate ?? DateTime.MinValue;

            // Check for entry date (optional MMDD after value date)
            int offset = 6;
            if (char.IsDigit(tag61[6]))
            {
                var entryDateStr = valueDateStr.Substring(0, 2) + tag61.Substring(6, 4);
                line.TransactionDate = ParseMT940Date(entryDateStr);
                offset = 10;
            }

            // Credit/Debit indicator
            var dcIndicator = tag61[offset];
            line.IsCredit = dcIndicator == 'C';
            offset++;

            // Skip funds code if present (single letter after C/D)
            if (char.IsLetter(tag61[offset]) && !char.IsDigit(tag61[offset + 1]))
            {
                offset++;
            }

            // Parse amount (until we hit a letter)
            var amountEnd = offset;
            while (amountEnd < tag61.Length && (char.IsDigit(tag61[amountEnd]) || tag61[amountEnd] == ',' || tag61[amountEnd] == '.'))
            {
                amountEnd++;
            }
            var amountStr = tag61.Substring(offset, amountEnd - offset).Replace(",", ".");
            line.Amount = decimal.Parse(amountStr, CultureInfo.InvariantCulture);
            offset = amountEnd;

            // Transaction type (N + 3 chars)
            if (offset < tag61.Length && tag61[offset] == 'N')
            {
                line.SwiftCode = tag61.Substring(offset, Math.Min(4, tag61.Length - offset));
                offset += line.SwiftCode.Length;
            }

            // Parse reference (until //)
            var refEnd = tag61.IndexOf("//", offset);
            if (refEnd > offset)
            {
                line.CustomerReference = tag61.Substring(offset, refEnd - offset).Trim();
                offset = refEnd + 2;
                line.BankReference = tag61.Substring(offset).Trim();
            }
            else if (offset < tag61.Length)
            {
                line.CustomerReference = tag61.Substring(offset).Trim();
            }

            line.Reference = line.CustomerReference ?? line.BankReference;

            // Parse :86: information
            if (!string.IsNullOrEmpty(tag86))
            {
                line.Description = ParseMT940Tag86(tag86, line);
            }

            return line;
        }

        private string ParseMT940Tag86(string tag86, ParsedBankStatementLine line)
        {
            // :86: contains structured or unstructured information
            // Structured format uses subfields like /NAME/, /ADDR/, /REMI/, etc.

            var description = new StringBuilder();

            // Check for structured format (starts with /)
            if (tag86.StartsWith("/"))
            {
                var subfields = Regex.Matches(tag86, @"/([A-Z]+)/([^/]*)");
                foreach (Match sf in subfields)
                {
                    var fieldName = sf.Groups[1].Value;
                    var fieldValue = sf.Groups[2].Value.Trim();

                    switch (fieldName)
                    {
                        case "NAME":
                            line.PayeeName = fieldValue;
                            break;
                        case "REMI":
                        case "REM":
                            description.Append(fieldValue).Append(" ");
                            break;
                        case "EREF":
                            if (string.IsNullOrEmpty(line.Reference))
                                line.Reference = fieldValue;
                            break;
                        case "CHQN":
                            line.ChequeNumber = fieldValue;
                            break;
                        default:
                            description.Append(fieldValue).Append(" ");
                            break;
                    }
                }
            }
            else
            {
                // Unstructured - just use as description
                description.Append(tag86);
            }

            return description.ToString().Trim();
        }

        #endregion

        #region CSV Parsing Helpers

        private BankParsingColumnMapping ParseColumnMapping(string mappingJson)
        {
            if (string.IsNullOrEmpty(mappingJson))
            {
                return new BankParsingColumnMapping();
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<BankParsingColumnMapping>(mappingJson);
            }
            catch
            {
                return new BankParsingColumnMapping();
            }
        }

        private string[] ParseCSVLine(string line, char delimiter)
        {
            var result = new List<string>();
            var inQuotes = false;
            var current = new StringBuilder();

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString().Trim());

            return result.ToArray();
        }

        private ParsedBankStatementLine ParseCSVLineToStatement(
            string[] columns,
            BankParsingColumnMapping mapping,
            string dateFormat,
            BankParsingProfile profile,
            int lineNumber)
        {
            var line = new ParsedBankStatementLine
            {
                LineNumber = lineNumber
            };

            // Parse date
            if (mapping.DateColumn >= 0 && mapping.DateColumn < columns.Length)
            {
                if (DateTime.TryParseExact(columns[mapping.DateColumn], dateFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    line.TransactionDate = date;
                }
                else if (DateTime.TryParse(columns[mapping.DateColumn], out date))
                {
                    line.TransactionDate = date;
                }
            }

            // Parse value date
            if (mapping.ValueDateColumn >= 0 && mapping.ValueDateColumn < columns.Length)
            {
                if (DateTime.TryParseExact(columns[mapping.ValueDateColumn], dateFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var valueDate))
                {
                    line.ValueDate = valueDate;
                }
            }

            // Parse description
            if (mapping.DescriptionColumn >= 0 && mapping.DescriptionColumn < columns.Length)
            {
                line.Description = columns[mapping.DescriptionColumn];
            }

            // Parse amount
            var decimalSeparator = profile?.DecimalSeparator ?? ".";
            var thousandSeparator = profile?.ThousandSeparator ?? ",";

            if (mapping.DebitColumn >= 0 && mapping.CreditColumn >= 0)
            {
                // Separate debit/credit columns
                var debitStr = mapping.DebitColumn < columns.Length ? columns[mapping.DebitColumn] : "";
                var creditStr = mapping.CreditColumn < columns.Length ? columns[mapping.CreditColumn] : "";

                if (!string.IsNullOrWhiteSpace(debitStr))
                {
                    line.Amount = ParseAmount(debitStr, decimalSeparator, thousandSeparator);
                    line.IsCredit = false;
                }
                else if (!string.IsNullOrWhiteSpace(creditStr))
                {
                    line.Amount = ParseAmount(creditStr, decimalSeparator, thousandSeparator);
                    line.IsCredit = true;
                }
            }
            else if (mapping.AmountColumn >= 0 && mapping.AmountColumn < columns.Length)
            {
                var amountStr = columns[mapping.AmountColumn];
                line.Amount = Math.Abs(ParseAmount(amountStr, decimalSeparator, thousandSeparator));

                // Determine credit/debit
                var amountFormat = profile?.AmountFormat ?? "signed";
                if (amountFormat == "signed")
                {
                    line.IsCredit = !amountStr.StartsWith("-");
                }
                else if (amountFormat == "indicator" && mapping.IndicatorColumn >= 0 && mapping.IndicatorColumn < columns.Length)
                {
                    var indicator = columns[mapping.IndicatorColumn].ToUpper();
                    var creditIndicators = (profile?.CreditIndicator ?? "CR,C,+").Split(',');
                    line.IsCredit = creditIndicators.Any(ci => indicator.Contains(ci.Trim()));
                }
            }

            // Parse reference
            if (mapping.ReferenceColumn >= 0 && mapping.ReferenceColumn < columns.Length)
            {
                line.Reference = columns[mapping.ReferenceColumn];
            }

            // Parse cheque number
            if (mapping.ChequeNumberColumn >= 0 && mapping.ChequeNumberColumn < columns.Length)
            {
                line.ChequeNumber = columns[mapping.ChequeNumberColumn];
            }
            else if (!string.IsNullOrEmpty(profile?.ChequeNumberPattern) && !string.IsNullOrEmpty(line.Description))
            {
                var match = Regex.Match(line.Description, profile.ChequeNumberPattern);
                if (match.Success)
                {
                    line.ChequeNumber = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                }
            }

            // Parse payee
            if (mapping.PayeeColumn >= 0 && mapping.PayeeColumn < columns.Length)
            {
                line.PayeeName = columns[mapping.PayeeColumn];
            }

            // Parse running balance
            if (mapping.BalanceColumn >= 0 && mapping.BalanceColumn < columns.Length)
            {
                var balanceStr = columns[mapping.BalanceColumn];
                if (!string.IsNullOrWhiteSpace(balanceStr))
                {
                    line.RunningBalance = ParseAmount(balanceStr, decimalSeparator, thousandSeparator);
                }
            }

            return line;
        }

        private decimal ParseAmount(string amountStr, string decimalSeparator, string thousandSeparator)
        {
            if (string.IsNullOrWhiteSpace(amountStr)) return 0;

            // Remove thousand separators and standardize decimal separator
            amountStr = amountStr.Replace(thousandSeparator, "").Replace(decimalSeparator, ".");

            // Remove any currency symbols or spaces
            amountStr = Regex.Replace(amountStr, @"[^\d.-]", "");

            if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                return amount;
            }

            return 0;
        }

        private Encoding GetEncoding(string encodingName)
        {
            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        #endregion

        #region CAMT.053 Parsing (Placeholder)

        private BankStatementParseResult ParseCAMT053(byte[] fileContent)
        {
            // CAMT.053 is an XML-based format (ISO 20022)
            // This would require XML parsing with the camt.053 schema
            return new BankStatementParseResult
            {
                Success = false,
                ErrorMessage = "CAMT.053 parsing not yet implemented. Use MT940 or CSV format."
            };
        }

        #endregion
    }
}
