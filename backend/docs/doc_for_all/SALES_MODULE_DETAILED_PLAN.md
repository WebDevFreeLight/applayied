# Sales Module - Detailed Implementation Plan

## Overview
This document provides a comprehensive implementation plan for the Sales module, covering CRM functionality, sales pipeline management, lead tracking, opportunity management, and sales analytics dashboard capabilities.

## Sales Module Architecture

### Core Components
1. **Customer Relationship Management (CRM)**
2. **Sales Pipeline Management**
3. **Lead Management**
4. **Opportunity Tracking**
5. **Contact Management**
6. **Sales Forecasting**
7. **Sales Analytics Dashboard**
8. **Quote & Proposal Management**
9. **Sales Team Performance**
10. **Sales Reporting**

## 1. Backend Implementation

### Controllers

**SalesDashboardController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Sales,SalesManager,Admin,SuperAdmin")]
public class SalesDashboardController : BaseController
{
    private readonly ISalesDashboardService _salesDashboardService;
    private readonly ISalesAnalyticsService _analyticsService;
    private readonly ISalesForecastingService _forecastingService;
    
    public SalesDashboardController(
        ISalesDashboardService salesDashboardService,
        ISalesAnalyticsService analyticsService,
        ISalesForecastingService forecastingService)
    {
        _salesDashboardService = salesDashboardService;
        _analyticsService = analyticsService;
        _forecastingService = forecastingService;
    }
    
    [HttpGet("overview")]
    public async Task<IActionResult> GetDashboardOverview([FromQuery] string period = "month")
    {
        var overview = await _salesDashboardService.GetDashboardOverviewAsync(period);
        return Ok(overview);
    }
    
    [HttpGet("sales-metrics")]
    public async Task<IActionResult> GetSalesMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var metrics = await _salesDashboardService.GetSalesMetricsAsync(startDate, endDate);
        return Ok(metrics);
    }
    
    [HttpGet("pipeline-overview")]
    public async Task<IActionResult> GetPipelineOverview([FromQuery] long? salesPersonId = null)
    {
        var pipeline = await _salesDashboardService.GetPipelineOverviewAsync(salesPersonId);
        return Ok(pipeline);
    }
    
    [HttpGet("conversion-funnel")]
    public async Task<IActionResult> GetConversionFunnel([FromQuery] string period = "month")
    {
        var funnel = await _analyticsService.GetConversionFunnelAsync(period);
        return Ok(funnel);
    }
    
    [HttpGet("sales-trends")]
    public async Task<IActionResult> GetSalesTrends([FromQuery] int months = 12)
    {
        var trends = await _analyticsService.GetSalesTrendsAsync(months);
        return Ok(trends);
    }
    
    [HttpGet("team-performance")]
    public async Task<IActionResult> GetTeamPerformance([FromQuery] string period = "month")
    {
        var performance = await _analyticsService.GetTeamPerformanceAsync(period);
        return Ok(performance);
    }
    
    [HttpGet("sales-forecast")]
    public async Task<IActionResult> GetSalesForecast([FromQuery] int months = 6)
    {
        var forecast = await _forecastingService.GetSalesForecastAsync(months);
        return Ok(forecast);
    }
    
    [HttpGet("top-opportunities")]
    public async Task<IActionResult> GetTopOpportunities([FromQuery] int count = 10)
    {
        var opportunities = await _salesDashboardService.GetTopOpportunitiesAsync(count);
        return Ok(opportunities);
    }
    
    [HttpGet("recent-activities")]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 20)
    {
        var activities = await _salesDashboardService.GetRecentActivitiesAsync(count);
        return Ok(activities);
    }
}
```

**LeadController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Sales,SalesManager,Admin")]
public class LeadController : BaseController
{
    private readonly ILeadService _leadService;
    private readonly ILeadScoringService _leadScoringService;
    private readonly ILeadConversionService _conversionService;
    
    public LeadController(
        ILeadService leadService,
        ILeadScoringService leadScoringService,
        ILeadConversionService conversionService)
    {
        _leadService = leadService;
        _leadScoringService = leadScoringService;
        _conversionService = conversionService;
    }
    
    [HttpGet("leads")]
    public async Task<IActionResult> GetLeads([FromQuery] LeadQueryDto query)
    {
        var leads = await _leadService.GetLeadsAsync(query);
        return Ok(leads);
    }
    
    [HttpGet("leads/{id}")]
    public async Task<IActionResult> GetLead(long id)
    {
        var lead = await _leadService.GetLeadByIdAsync(id);
        return Ok(lead);
    }
    
    [HttpPost("leads")]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadDto lead)
    {
        var result = await _leadService.CreateLeadAsync(lead);
        return Ok(result);
    }
    
    [HttpPut("leads/{id}")]
    public async Task<IActionResult> UpdateLead(long id, [FromBody] UpdateLeadDto lead)
    {
        await _leadService.UpdateLeadAsync(id, lead);
        return Ok(new { message = "Lead updated successfully" });
    }
    
    [HttpPost("leads/{id}/assign")]
    [Authorize(Roles = "SalesManager,Admin")]
    public async Task<IActionResult> AssignLead(long id, [FromBody] AssignLeadDto assignment)
    {
        await _leadService.AssignLeadAsync(id, assignment);
        return Ok(new { message = "Lead assigned successfully" });
    }
    
    [HttpPost("leads/{id}/qualify")]
    public async Task<IActionResult> QualifyLead(long id, [FromBody] QualifyLeadDto qualification)
    {
        await _leadService.QualifyLeadAsync(id, qualification);
        return Ok(new { message = "Lead qualified successfully" });
    }
    
    [HttpPost("leads/{id}/convert")]
    public async Task<IActionResult> ConvertLead(long id, [FromBody] ConvertLeadDto conversion)
    {
        var result = await _conversionService.ConvertLeadAsync(id, conversion);
        return Ok(result);
    }
    
    [HttpGet("leads/{id}/score")]
    public async Task<IActionResult> GetLeadScore(long id)
    {
        var score = await _leadScoringService.CalculateLeadScoreAsync(id);
        return Ok(score);
    }
    
    [HttpGet("lead-sources")]
    public async Task<IActionResult> GetLeadSources()
    {
        var sources = await _leadService.GetLeadSourcesAsync();
        return Ok(sources);
    }
    
    [HttpPost("leads/bulk-import")]
    [Authorize(Roles = "SalesManager,Admin")]
    public async Task<IActionResult> BulkImportLeads([FromBody] BulkImportLeadsDto import)
    {
        var result = await _leadService.BulkImportLeadsAsync(import);
        return Ok(result);
    }
}
```

**OpportunityController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Sales,SalesManager,Admin")]
public class OpportunityController : BaseController
{
    private readonly IOpportunityService _opportunityService;
    private readonly ISalesPipelineService _pipelineService;
    private readonly ISalesStageService _stageService;
    
    public OpportunityController(
        IOpportunityService opportunityService,
        ISalesPipelineService pipelineService,
        ISalesStageService stageService)
    {
        _opportunityService = opportunityService;
        _pipelineService = pipelineService;
        _stageService = stageService;
    }
    
    [HttpGet("opportunities")]
    public async Task<IActionResult> GetOpportunities([FromQuery] OpportunityQueryDto query)
    {
        var opportunities = await _opportunityService.GetOpportunitiesAsync(query);
        return Ok(opportunities);
    }
    
    [HttpGet("opportunities/{id}")]
    public async Task<IActionResult> GetOpportunity(long id)
    {
        var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
        return Ok(opportunity);
    }
    
    [HttpPost("opportunities")]
    public async Task<IActionResult> CreateOpportunity([FromBody] CreateOpportunityDto opportunity)
    {
        var result = await _opportunityService.CreateOpportunityAsync(opportunity);
        return Ok(result);
    }
    
    [HttpPut("opportunities/{id}")]
    public async Task<IActionResult> UpdateOpportunity(long id, [FromBody] UpdateOpportunityDto opportunity)
    {
        await _opportunityService.UpdateOpportunityAsync(id, opportunity);
        return Ok(new { message = "Opportunity updated successfully" });
    }
    
    [HttpPost("opportunities/{id}/move-stage")]
    public async Task<IActionResult> MoveOpportunityStage(long id, [FromBody] MoveStageDto moveStage)
    {
        await _pipelineService.MoveOpportunityStageAsync(id, moveStage);
        return Ok(new { message = "Opportunity moved to new stage" });
    }
    
    [HttpPost("opportunities/{id}/close-won")]
    public async Task<IActionResult> CloseOpportunityWon(long id, [FromBody] CloseOpportunityDto closeData)
    {
        await _opportunityService.CloseOpportunityWonAsync(id, closeData);
        return Ok(new { message = "Opportunity closed as won" });
    }
    
    [HttpPost("opportunities/{id}/close-lost")]
    public async Task<IActionResult> CloseOpportunityLost(long id, [FromBody] CloseOpportunityDto closeData)
    {
        await _opportunityService.CloseOpportunityLostAsync(id, closeData);
        return Ok(new { message = "Opportunity closed as lost" });
    }
    
    [HttpGet("opportunities/{id}/activities")]
    public async Task<IActionResult> GetOpportunityActivities(long id)
    {
        var activities = await _opportunityService.GetOpportunityActivitiesAsync(id);
        return Ok(activities);
    }
    
    [HttpPost("opportunities/{id}/activities")]
    public async Task<IActionResult> AddOpportunityActivity(long id, [FromBody] CreateActivityDto activity)
    {
        var result = await _opportunityService.AddOpportunityActivityAsync(id, activity);
        return Ok(result);
    }
    
    [HttpGet("sales-stages")]
    public async Task<IActionResult> GetSalesStages()
    {
        var stages = await _stageService.GetSalesStagesAsync();
        return Ok(stages);
    }
    
    [HttpGet("pipeline-view")]
    public async Task<IActionResult> GetPipelineView([FromQuery] long? salesPersonId = null)
    {
        var pipeline = await _pipelineService.GetPipelineViewAsync(salesPersonId);
        return Ok(pipeline);
    }
}
```

**CustomerController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Sales,Customer Service,Admin")]
public class CustomerController : BaseController
{
    private readonly ICustomerService _customerService;
    private readonly IContactService _contactService;
    private readonly ICustomerInteractionService _interactionService;
    
    public CustomerController(
        ICustomerService customerService,
        IContactService contactService,
        ICustomerInteractionService interactionService)
    {
        _customerService = customerService;
        _contactService = contactService;
        _interactionService = interactionService;
    }
    
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerQueryDto query)
    {
        var customers = await _customerService.GetCustomersAsync(query);
        return Ok(customers);
    }
    
    [HttpGet("customers/{id}")]
    public async Task<IActionResult> GetCustomer(long id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        return Ok(customer);
    }
    
    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto customer)
    {
        var result = await _customerService.CreateCustomerAsync(customer);
        return Ok(result);
    }
    
    [HttpPut("customers/{id}")]
    public async Task<IActionResult> UpdateCustomer(long id, [FromBody] UpdateCustomerDto customer)
    {
        await _customerService.UpdateCustomerAsync(id, customer);
        return Ok(new { message = "Customer updated successfully" });
    }
    
    [HttpGet("customers/{id}/contacts")]
    public async Task<IActionResult> GetCustomerContacts(long id)
    {
        var contacts = await _contactService.GetCustomerContactsAsync(id);
        return Ok(contacts);
    }
    
    [HttpPost("customers/{id}/contacts")]
    public async Task<IActionResult> AddCustomerContact(long id, [FromBody] CreateContactDto contact)
    {
        var result = await _contactService.AddCustomerContactAsync(id, contact);
        return Ok(result);
    }
    
    [HttpGet("customers/{id}/interactions")]
    public async Task<IActionResult> GetCustomerInteractions(long id)
    {
        var interactions = await _interactionService.GetCustomerInteractionsAsync(id);
        return Ok(interactions);
    }
    
    [HttpPost("customers/{id}/interactions")]
    public async Task<IActionResult> AddCustomerInteraction(long id, [FromBody] CreateInteractionDto interaction)
    {
        var result = await _interactionService.AddCustomerInteractionAsync(id, interaction);
        return Ok(result);
    }
    
    [HttpGet("customers/{id}/opportunities")]
    public async Task<IActionResult> GetCustomerOpportunities(long id)
    {
        var opportunities = await _customerService.GetCustomerOpportunitiesAsync(id);
        return Ok(opportunities);
    }
    
    [HttpGet("customers/{id}/sales-history")]
    public async Task<IActionResult> GetCustomerSalesHistory(long id)
    {
        var history = await _customerService.GetCustomerSalesHistoryAsync(id);
        return Ok(history);
    }
}
```

**QuoteController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Sales,SalesManager,Admin")]
public class QuoteController : BaseController
{
    private readonly IQuoteService _quoteService;
    private readonly IProposalService _proposalService;
    private readonly IPricingService _pricingService;
    
    public QuoteController(
        IQuoteService quoteService,
        IProposalService proposalService,
        IPricingService pricingService)
    {
        _quoteService = quoteService;
        _proposalService = proposalService;
        _pricingService = pricingService;
    }
    
    [HttpGet("quotes")]
    public async Task<IActionResult> GetQuotes([FromQuery] QuoteQueryDto query)
    {
        var quotes = await _quoteService.GetQuotesAsync(query);
        return Ok(quotes);
    }
    
    [HttpGet("quotes/{id}")]
    public async Task<IActionResult> GetQuote(long id)
    {
        var quote = await _quoteService.GetQuoteByIdAsync(id);
        return Ok(quote);
    }
    
    [HttpPost("quotes")]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteDto quote)
    {
        var result = await _quoteService.CreateQuoteAsync(quote);
        return Ok(result);
    }
    
    [HttpPut("quotes/{id}")]
    public async Task<IActionResult> UpdateQuote(long id, [FromBody] UpdateQuoteDto quote)
    {
        await _quoteService.UpdateQuoteAsync(id, quote);
        return Ok(new { message = "Quote updated successfully" });
    }
    
    [HttpPost("quotes/{id}/send")]
    public async Task<IActionResult> SendQuote(long id, [FromBody] SendQuoteDto sendData)
    {
        await _quoteService.SendQuoteAsync(id, sendData);
        return Ok(new { message = "Quote sent successfully" });
    }
    
    [HttpPost("quotes/{id}/accept")]
    public async Task<IActionResult> AcceptQuote(long id)
    {
        await _quoteService.AcceptQuoteAsync(id);
        return Ok(new { message = "Quote accepted" });
    }
    
    [HttpPost("quotes/{id}/reject")]
    public async Task<IActionResult> RejectQuote(long id, [FromBody] RejectQuoteDto rejection)
    {
        await _quoteService.RejectQuoteAsync(id, rejection);
        return Ok(new { message = "Quote rejected" });
    }
    
    [HttpGet("quotes/{id}/pdf")]
    public async Task<IActionResult> GenerateQuotePdf(long id)
    {
        var pdf = await _quoteService.GenerateQuotePdfAsync(id);
        return File(pdf, "application/pdf", $"quote-{id}.pdf");
    }
    
    [HttpGet("pricing-templates")]
    public async Task<IActionResult> GetPricingTemplates()
    {
        var templates = await _pricingService.GetPricingTemplatesAsync();
        return Ok(templates);
    }
}
```

### Services

**ISalesDashboardService.cs**
```csharp
public interface ISalesDashboardService
{
    Task<SalesDashboardOverviewDto> GetDashboardOverviewAsync(string period);
    Task<SalesMetricsDto> GetSalesMetricsAsync(DateTime? startDate, DateTime? endDate);
    Task<PipelineOverviewDto> GetPipelineOverviewAsync(long? salesPersonId);
    Task<List<OpportunityDto>> GetTopOpportunitiesAsync(int count);
    Task<List<SalesActivityDto>> GetRecentActivitiesAsync(int count);
    Task<List<SalesKpiDto>> GetSalesKpisAsync(string period);
}
```

**SalesDashboardService.cs**
```csharp
[AppService(ServiceLifetime = LifeTime.Transient)]
public class SalesDashboardService : BaseService<Lead>, ISalesDashboardService
{
    private readonly ILeadService _leadService;
    private readonly IOpportunityService _opportunityService;
    private readonly ICustomerService _customerService;
    private readonly ISalesAnalyticsService _analyticsService;
    
    public SalesDashboardService(
        ILeadService leadService,
        IOpportunityService opportunityService,
        ICustomerService customerService,
        ISalesAnalyticsService analyticsService)
    {
        _leadService = leadService;
        _opportunityService = opportunityService;
        _customerService = customerService;
        _analyticsService = analyticsService;
    }
    
    public async Task<SalesDashboardOverviewDto> GetDashboardOverviewAsync(string period)
    {
        var startDate = GetStartDateForPeriod(period);
        var endDate = DateTime.UtcNow;
        
        var totalLeads = await _leadService.GetLeadsCountAsync(startDate, endDate);
        var qualifiedLeads = await _leadService.GetQualifiedLeadsCountAsync(startDate, endDate);
        var totalOpportunities = await _opportunityService.GetOpportunitiesCountAsync(startDate, endDate);
        var wonOpportunities = await _opportunityService.GetWonOpportunitiesCountAsync(startDate, endDate);
        var totalRevenue = await _opportunityService.GetTotalRevenueAsync(startDate, endDate);
        var pipelineValue = await _opportunityService.GetPipelineValueAsync();
        var conversionRate = totalLeads > 0 ? (decimal)wonOpportunities / totalLeads * 100 : 0;
        var averageDealSize = wonOpportunities > 0 ? totalRevenue / wonOpportunities : 0;
        
        return new SalesDashboardOverviewDto
        {
            TotalLeads = totalLeads,
            QualifiedLeads = qualifiedLeads,
            TotalOpportunities = totalOpportunities,
            WonOpportunities = wonOpportunities,
            TotalRevenue = totalRevenue,
            PipelineValue = pipelineValue,
            ConversionRate = conversionRate,
            AverageDealSize = averageDealSize,
            Period = period,
            StartDate = startDate,
            EndDate = endDate,
            LastUpdated = DateTime.UtcNow
        };
    }
    
    public async Task<SalesMetricsDto> GetSalesMetricsAsync(DateTime? startDate, DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;
        
        var salesByStage = await _opportunityService.GetSalesByStageAsync(start, end);
        var salesBySource = await _leadService.GetSalesBySourceAsync(start, end);
        var salesTrends = await _analyticsService.GetSalesTrendsAsync(start, end);
        var topPerformers = await _analyticsService.GetTopPerformersAsync(start, end, 10);
        
        return new SalesMetricsDto
        {
            SalesByStage = salesByStage,
            SalesBySource = salesBySource,
            SalesTrends = salesTrends,
            TopPerformers = topPerformers,
            StartDate = start,
            EndDate = end
        };
    }
    
    public async Task<PipelineOverviewDto> GetPipelineOverviewAsync(long? salesPersonId)
    {
        var pipelineStages = await _opportunityService.GetPipelineStagesAsync(salesPersonId);
        var stageConversion = await _analyticsService.GetStageConversionRatesAsync(salesPersonId);
        var averageSalesCycle = await _analyticsService.GetAverageSalesCycleAsync(salesPersonId);
        
        return new PipelineOverviewDto
        {
            PipelineStages = pipelineStages,
            StageConversionRates = stageConversion,
            AverageSalesCycle = averageSalesCycle,
            SalesPersonId = salesPersonId
        };
    }
    
    // Additional method implementations...
}
```

### Models and DTOs

**SalesDashboardOverviewDto.cs**
```csharp
public class SalesDashboardOverviewDto
{
    public int TotalLeads { get; set; }
    public int QualifiedLeads { get; set; }
    public int TotalOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal AverageDealSize { get; set; }
    public string Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

**Lead.cs**
```csharp
[SugarTable("sales_leads")]
public class Lead : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long LeadId { get; set; }
    
    [SugarColumn(Length = 200)]
    public string FirstName { get; set; }
    
    [SugarColumn(Length = 200)]
    public string LastName { get; set; }
    
    [SugarColumn(Length = 400)]
    public string FullName { get; set; }
    
    [SugarColumn(Length = 200)]
    public string Email { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Phone { get; set; }
    
    [SugarColumn(Length = 200)]
    public string Company { get; set; }
    
    [SugarColumn(Length = 100)]
    public string JobTitle { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Industry { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Source { get; set; } // Website, Referral, Social Media, etc.
    
    [SugarColumn(Length = 20)]
    public string Status { get; set; } = "New"; // New, Contacted, Qualified, Unqualified, Converted
    
    public int Score { get; set; } = 0;
    
    [SugarColumn(Length = 20)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Hot
    
    public long? AssignedTo { get; set; }
    
    public DateTime? LastContactDate { get; set; }
    
    public DateTime? NextFollowUpDate { get; set; }
    
    [SugarColumn(Length = 1000)]
    public string Notes { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal? EstimatedValue { get; set; }
    
    public DateTime? ConvertedDate { get; set; }
    
    public long? ConvertedOpportunityId { get; set; }
    
    [SugarColumn(Length = 200)]
    public string Address { get; set; }
    
    [SugarColumn(Length = 100)]
    public string City { get; set; }
    
    [SugarColumn(Length = 100)]
    public string State { get; set; }
    
    [SugarColumn(Length = 20)]
    public string PostalCode { get; set; }
    
    [SugarColumn(Length = 100)]
    public string Country { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(LeadActivity.LeadId))]
    public List<LeadActivity> Activities { get; set; }
}
```

**Opportunity.cs**
```csharp
[SugarTable("sales_opportunities")]
public class Opportunity : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long OpportunityId { get; set; }
    
    [SugarColumn(Length = 200)]
    public string OpportunityName { get; set; }
    
    [SugarColumn(Length = 500)]
    public string Description { get; set; }
    
    public long CustomerId { get; set; }
    
    public long? LeadId { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal Amount { get; set; }
    
    [SugarColumn(Length = 10)]
    public string Currency { get; set; } = "USD";
    
    public long SalesStageId { get; set; }
    
    [SugarColumn(Length = 20)]
    public string Status { get; set; } = "Open"; // Open, Won, Lost, Cancelled
    
    public decimal Probability { get; set; } = 50;
    
    public DateTime? ExpectedCloseDate { get; set; }
    
    public DateTime? ActualCloseDate { get; set; }
    
    public long AssignedTo { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Source { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Type { get; set; } // New Business, Existing Business, Renewal
    
    [SugarColumn(Length = 1000)]
    public string Notes { get; set; }
    
    [SugarColumn(Length = 500)]
    public string LostReason { get; set; }
    
    public long? CompetitorId { get; set; }
    
    public DateTime? LastActivityDate { get; set; }
    
    public DateTime? NextActivityDate { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(CustomerId))]
    public Customer Customer { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(SalesStageId))]
    public SalesStage SalesStage { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(OpportunityActivity.OpportunityId))]
    public List<OpportunityActivity> Activities { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(Quote.OpportunityId))]
    public List<Quote> Quotes { get; set; }
}
```

**Customer.cs**
```csharp
[SugarTable("sales_customers")]
public class Customer : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long CustomerId { get; set; }
    
    [SugarColumn(Length = 200)]
    public string CustomerName { get; set; }
    
    [SugarColumn(Length = 50)]
    public string CustomerType { get; set; } // Individual, Business
    
    [SugarColumn(Length = 200)]
    public string Email { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Phone { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Industry { get; set; }
    
    [SugarColumn(Length = 200)]
    public string Website { get; set; }
    
    [SugarColumn(Length = 20)]
    public string Status { get; set; } = "Active"; // Active, Inactive, Prospect
    
    public long? AssignedTo { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal LifetimeValue { get; set; } = 0;
    
    public DateTime? LastPurchaseDate { get; set; }
    
    [SugarColumn(Length = 1000)]
    public string Notes { get; set; }
    
    // Address Information
    [SugarColumn(Length = 200)]
    public string BillingAddress { get; set; }
    
    [SugarColumn(Length = 100)]
    public string BillingCity { get; set; }
    
    [SugarColumn(Length = 100)]
    public string BillingState { get; set; }
    
    [SugarColumn(Length = 20)]
    public string BillingPostalCode { get; set; }
    
    [SugarColumn(Length = 100)]
    public string BillingCountry { get; set; }
    
    [SugarColumn(Length = 200)]
    public string ShippingAddress { get; set; }
    
    [SugarColumn(Length = 100)]
    public string ShippingCity { get; set; }
    
    [SugarColumn(Length = 100)]
    public string ShippingState { get; set; }
    
    [SugarColumn(Length = 20)]
    public string ShippingPostalCode { get; set; }
    
    [SugarColumn(Length = 100)]
    public string ShippingCountry { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(Contact.CustomerId))]
    public List<Contact> Contacts { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(Opportunity.CustomerId))]
    public List<Opportunity> Opportunities { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(CustomerInteraction.CustomerId))]
    public List<CustomerInteraction> Interactions { get; set; }
}
```

**SalesStage.cs**
```csharp
[SugarTable("sales_stages")]
public class SalesStage : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long SalesStageId { get; set; }
    
    [SugarColumn(Length = 100)]
    public string StageName { get; set; }
    
    [SugarColumn(Length = 500)]
    public string Description { get; set; }
    
    public int SortOrder { get; set; }
    
    public decimal DefaultProbability { get; set; }
    
    [SugarColumn(Length = 20)]
    public string Color { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsClosedStage { get; set; } = false;
    
    public bool IsWonStage { get; set; } = false;
    
    [Navigate(NavigateType.OneToMany, nameof(Opportunity.SalesStageId))]
    public List<Opportunity> Opportunities { get; set; }
}
```

## 2. Frontend Implementation

### Sales Dashboard Vue Component

**views/sales/Dashboard.vue**
```vue
<template>
  <div class="sales-dashboard">
    <!-- Dashboard Header -->
    <div class="dashboard-header">
      <h1>{{ $t('sales.dashboard.title') }}</h1>
      <div class="header-actions">
        <el-select v-model="selectedPeriod" @change="changePeriod">
          <el-option
            v-for="period in periods"
            :key="period.value"
            :label="period.label"
            :value="period.value"
          />
        </el-select>
        <el-select v-model="selectedSalesPerson" @change="changeSalesPerson" clearable>
          <el-option
            v-for="person in salesTeam"
            :key="person.id"
            :label="person.name"
            :value="person.id"
          />
        </el-select>
        <el-button type="primary" @click="refreshDashboard">
          <i class="el-icon-refresh"></i>
          {{ $t('common.refresh') }}
        </el-button>
      </div>
    </div>

    <!-- Sales KPIs -->
    <div class="kpi-grid">
      <SalesKpiCard
        v-for="kpi in salesKpis"
        :key="kpi.key"
        :title="kpi.title"
        :value="kpi.value"
        :change="kpi.change"
        :trend="kpi.trend"
        :icon="kpi.icon"
        :color="kpi.color"
        :format="kpi.format"
      />
    </div>

    <!-- Sales Pipeline -->
    <div class="pipeline-section">
      <div class="section-header">
        <h3>{{ $t('sales.dashboard.salesPipeline') }}</h3>
        <div class="pipeline-controls">
          <el-button-group size="small">
            <el-button 
              :type="pipelineView === 'kanban' ? 'primary' : 'default'"
              @click="pipelineView = 'kanban'"
            >
              {{ $t('sales.views.kanban') }}
            </el-button>
            <el-button 
              :type="pipelineView === 'chart' ? 'primary' : 'default'"
              @click="pipelineView = 'chart'"
            >
              {{ $t('sales.views.chart') }}
            </el-button>
          </el-button-group>
        </div>
      </div>
      
      <SalesPipelineKanban 
        v-if="pipelineView === 'kanban'"
        :pipeline-data="pipelineData"
        @opportunity-moved="handleOpportunityMoved"
      />
      <SalesPipelineChart 
        v-else
        :pipeline-data="pipelineData"
      />
    </div>

    <!-- Charts Section -->
    <div class="charts-section">
      <el-row :gutter="20">
        <el-col :span="12">
          <div class="chart-container">
            <div class="chart-header">
              <h3>{{ $t('sales.dashboard.salesTrends') }}</h3>
              <el-button-group size="small">
                <el-button 
                  v-for="view in chartViews" 
                  :key="view.value"
                  :type="salesTrendView === view.value ? 'primary' : 'default'"
                  @click="salesTrendView = view.value"
                >
                  {{ view.label }}
                </el-button>
              </el-button-group>
            </div>
            <SalesTrendChart 
              :data="salesTrendData" 
              :view="salesTrendView"
            />
          </div>
        </el-col>
        <el-col :span="12">
          <div class="chart-container">
            <h3>{{ $t('sales.dashboard.conversionFunnel') }}</h3>
            <ConversionFunnelChart :data="conversionFunnelData" />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="20" style="margin-top: 20px;">
        <el-col :span="8">
          <div class="chart-container">
            <h3>{{ $t('sales.dashboard.leadSources') }}</h3>
            <LeadSourceChart :data="leadSourceData" />
          </div>
        </el-col>
        <el-col :span="8">
          <div class="chart-container">
            <h3>{{ $t('sales.dashboard.salesByStage') }}</h3>
            <SalesByStageChart :data="salesByStageData" />
          </div>
        </el-col>
        <el-col :span="8">
          <div class="chart-container">
            <h3>{{ $t('sales.dashboard.teamPerformance') }}</h3>
            <TeamPerformanceChart :data="teamPerformanceData" />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- Sales Forecast -->
    <div class="forecast-section">
      <h3>{{ $t('sales.dashboard.salesForecast') }}</h3>
      <SalesForecastChart :forecast-data="forecastData" />
    </div>

    <!-- Quick Actions -->
    <div class="quick-actions">
      <h3>{{ $t('sales.dashboard.quickActions') }}</h3>
      <div class="actions-grid">
        <SalesActionCard
          v-for="action in quickActions"
          :key="action.key"
          :title="action.title"
          :description="action.description"
          :icon="action.icon"
          :route="action.route"
          :permission="action.permission"
        />
      </div>
    </div>

    <!-- Top Opportunities -->
    <div class="top-opportunities">
      <div class="section-header">
        <h3>{{ $t('sales.dashboard.topOpportunities') }}</h3>
        <el-button type="text" @click="viewAllOpportunities">
          {{ $t('common.viewAll') }}
        </el-button>
      </div>
      <TopOpportunitiesList :opportunities="topOpportunities" />
    </div>

    <!-- Recent Activities -->
    <div class="recent-activities">
      <div class="section-header">
        <h3>{{ $t('sales.dashboard.recentActivities') }}</h3>
        <el-button type="text" @click="viewAllActivities">
          {{ $t('common.viewAll') }}
        </el-button>
      </div>
      <RecentActivitiesList :activities="recentActivities" />
    </div>
  </div>
</template>

<script>
import { ref, reactive, onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { 
  getDashboardOverview, 
  getSalesMetrics,
  getPipelineOverview,
  getConversionFunnel,
  getSalesTrends,
  getTeamPerformance,
  getSalesForecast,
  getTopOpportunities,
  getRecentActivities
} from '@/api/sales/dashboard'
import SalesKpiCard from '@/components/Sales/SalesKpiCard.vue'
import SalesPipelineKanban from '@/components/Sales/SalesPipelineKanban.vue'
import SalesPipelineChart from '@/components/Sales/SalesPipelineChart.vue'
import SalesTrendChart from '@/components/Sales/SalesTrendChart.vue'
import ConversionFunnelChart from '@/components/Sales/ConversionFunnelChart.vue'
import LeadSourceChart from '@/components/Sales/LeadSourceChart.vue'
import SalesByStageChart from '@/components/Sales/SalesByStageChart.vue'
import TeamPerformanceChart from '@/components/Sales/TeamPerformanceChart.vue'
import SalesForecastChart from '@/components/Sales/SalesForecastChart.vue'
import SalesActionCard from '@/components/Sales/SalesActionCard.vue'
import TopOpportunitiesList from '@/components/Sales/TopOpportunitiesList.vue'
import RecentActivitiesList from '@/components/Sales/RecentActivitiesList.vue'

export default {
  name: 'SalesDashboard',
  components: {
    SalesKpiCard,
    SalesPipelineKanban,
    SalesPipelineChart,
    SalesTrendChart,
    ConversionFunnelChart,
    LeadSourceChart,
    SalesByStageChart,
    TeamPerformanceChart,
    SalesForecastChart,
    SalesActionCard,
    TopOpportunitiesList,
    RecentActivitiesList
  },
  setup() {
    const { t } = useI18n()
    const router = useRouter()
    
    const loading = ref(false)
    const selectedPeriod = ref('month')
    const selectedSalesPerson = ref(null)
    const pipelineView = ref('kanban')
    const salesTrendView = ref('monthly')
    
    const periods = [
      { label: t('common.week'), value: 'week' },
      { label: t('common.month'), value: 'month' },
      { label: t('common.quarter'), value: 'quarter' },
      { label: t('common.year'), value: 'year' }
    ]
    
    const chartViews = [
      { label: t('sales.views.monthly'), value: 'monthly' },
      { label: t('sales.views.quarterly'), value: 'quarterly' },
      { label: t('sales.views.yearly'), value: 'yearly' }
    ]
    
    const salesTeam = ref([])
    
    const dashboardData = reactive({
      overview: {},
      salesMetrics: {},
      pipelineOverview: {},
      conversionFunnel: {},
      salesTrends: {},
      teamPerformance: {},
      forecast: {},
      topOpportunities: [],
      recentActivities: []
    })
    
    const salesKpis = computed(() => [
      {
        key: 'totalLeads',
        title: t('sales.kpis.totalLeads'),
        value: dashboardData.overview.totalLeads || 0,
        change: '+15.2%',
        trend: 'up',
        icon: 'el-icon-user-solid',
        color: 'primary',
        format: 'number'
      },
      {
        key: 'qualifiedLeads',
        title: t('sales.kpis.qualifiedLeads'),
        value: dashboardData.overview.qualifiedLeads || 0,
        change: '+8.7%',
        trend: 'up',
        icon: 'el-icon-check',
        color: 'success',
        format: 'number'
      },
      {
        key: 'totalRevenue',
        title: t('sales.kpis.totalRevenue'),
        value: dashboardData.overview.totalRevenue || 0,
        change: '+22.1%',
        trend: 'up',
        icon: 'el-icon-money',
        color: 'success',
        format: 'currency'
      },
      {
        key: 'pipelineValue',
        title: t('sales.kpis.pipelineValue'),
        value: dashboardData.overview.pipelineValue || 0,
        change: '+12.5%',
        trend: 'up',
        icon: 'el-icon-data-line',
        color: 'warning',
        format: 'currency'
      },
      {
        key: 'conversionRate',
        title: t('sales.kpis.conversionRate'),
        value: dashboardData.overview.conversionRate || 0,
        change: '+3.2%',
        trend: 'up',
        icon: 'el-icon-pie-chart',
        color: 'info',
        format: 'percentage'
      },
      {
        key: 'averageDealSize',
        title: t('sales.kpis.averageDealSize'),
        value: dashboardData.overview.averageDealSize || 0,
        change: '+7.8%',
        trend: 'up',
        icon: 'el-icon-trophy',
        color: 'primary',
        format: 'currency'
      }
    ])
    
    const quickActions = computed(() => [
      {
        key: 'addLead',
        title: t('sales.actions.addLead'),
        description: t('sales.actions.addLeadDesc'),
        icon: 'el-icon-plus',
        route: '/sales/leads/create',
        permission: 'sales:lead:create'
      },
      {
        key: 'createOpportunity',
        title: t('sales.actions.createOpportunity'),
        description: t('sales.actions.createOpportunityDesc'),
        icon: 'el-icon-opportunity',
        route: '/sales/opportunities/create',
        permission: 'sales:opportunity:create'
      },
      {
        key: 'addCustomer',
        title: t('sales.actions.addCustomer'),
        description: t('sales.actions.addCustomerDesc'),
        icon: 'el-icon-user',
        route: '/sales/customers/create',
        permission: 'sales:customer:create'
      },
      {
        key: 'createQuote',
        title: t('sales.actions.createQuote'),
        description: t('sales.actions.createQuoteDesc'),
        icon: 'el-icon-document',
        route: '/sales/quotes/create',
        permission: 'sales:quote:create'
      },
      {
        key: 'salesReports',
        title: t('sales.actions.salesReports'),
        description: t('sales.actions.salesReportsDesc'),
        icon: 'el-icon-data-analysis',
        route: '/sales/reports',
        permission: 'sales:reports:view'
      },
      {
        key: 'teamManagement',
        title: t('sales.actions.teamManagement'),
        description: t('sales.actions.teamManagementDesc'),
        icon: 'el-icon-s-custom',
        route: '/sales/team',
        permission: 'sales:team:manage'
      }
    ])
    
    const loadDashboardData = async () => {
      loading.value = true
      try {
        const [overview, metrics, pipeline, funnel, trends, performance, forecast, opportunities, activities] = await Promise.all([
          getDashboardOverview(selectedPeriod.value),
          getSalesMetrics(),
          getPipelineOverview(selectedSalesPerson.value),
          getConversionFunnel(selectedPeriod.value),
          getSalesTrends(12),
          getTeamPerformance(selectedPeriod.value),
          getSalesForecast(6),
          getTopOpportunities(10),
          getRecentActivities(20)
        ])
        
        dashboardData.overview = overview.data
        dashboardData.salesMetrics = metrics.data
        dashboardData.pipelineOverview = pipeline.data
        dashboardData.conversionFunnel = funnel.data
        dashboardData.salesTrends = trends.data
        dashboardData.teamPerformance = performance.data
        dashboardData.forecast = forecast.data
        dashboardData.topOpportunities = opportunities.data
        dashboardData.recentActivities = activities.data
        
      } catch (error) {
        ElMessage.error(t('common.loadError'))
        console.error('Sales dashboard load error:', error)
      } finally {
        loading.value = false
      }
    }
    
    const refreshDashboard = () => {
      loadDashboardData()
    }
    
    const changePeriod = (period) => {
      selectedPeriod.value = period
      loadDashboardData()
    }
    
    const changeSalesPerson = (personId) => {
      selectedSalesPerson.value = personId
      loadDashboardData()
    }
    
    const handleOpportunityMoved = (opportunityId, newStageId) => {
      // Handle opportunity stage change
      console.log('Opportunity moved:', opportunityId, newStageId)
      loadDashboardData()
    }
    
    const viewAllOpportunities = () => {
      router.push('/sales/opportunities')
    }
    
    const viewAllActivities = () => {
      router.push('/sales/activities')
    }
    
    onMounted(() => {
      loadDashboardData()
      // Set up auto-refresh every 10 minutes
      setInterval(loadDashboardData, 600000)
    })
    
    return {
      loading,
      selectedPeriod,
      selectedSalesPerson,
      pipelineView,
      salesTrendView,
      periods,
      chartViews,
      salesTeam,
      salesKpis,
      quickActions,
      pipelineData: computed(() => dashboardData.pipelineOverview || {}),
      salesTrendData: computed(() => dashboardData.salesTrends || []),
      conversionFunnelData: computed(() => dashboardData.conversionFunnel || []),
      leadSourceData: computed(() => dashboardData.salesMetrics.salesBySource || []),
      salesByStageData: computed(() => dashboardData.salesMetrics.salesByStage || []),
      teamPerformanceData: computed(() => dashboardData.teamPerformance || []),
      forecastData: computed(() => dashboardData.forecast || []),
      topOpportunities: computed(() => dashboardData.topOpportunities || []),
      recentActivities: computed(() => dashboardData.recentActivities || []),
      refreshDashboard,
      changePeriod,
      changeSalesPerson,
      handleOpportunityMoved,
      viewAllOpportunities,
      viewAllActivities
    }
  }
}
</script>

<style scoped>
.sales-dashboard {
  padding: 20px;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.header-actions {
  display: flex;
  gap: 15px;
  align-items: center;
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.pipeline-section {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 30px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.pipeline-controls {
  display: flex;
  gap: 10px;
}

.charts-section {
  margin-bottom: 30px;
}

.chart-container {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  height: 400px;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.chart-container h3 {
  margin-bottom: 20px;
  color: #333;
  font-size: 16px;
  font-weight: 600;
}

.forecast-section {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 30px;
}

.quick-actions {
  margin-bottom: 30px;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 15px;
  margin-top: 15px;
}

.top-opportunities,
.recent-activities {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}
</style>
```

## 3. Database Schema

```sql
-- Leads
CREATE TABLE sales_leads (
    lead_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    first_name VARCHAR(200),
    last_name VARCHAR(200),
    full_name VARCHAR(400),
    email VARCHAR(200),
    phone VARCHAR(50),
    company VARCHAR(200),
    job_title VARCHAR(100),
    industry VARCHAR(50),
    source VARCHAR(50),
    status VARCHAR(20) DEFAULT 'New',
    score INTEGER DEFAULT 0,
    priority VARCHAR(20) DEFAULT 'Medium',
    assigned_to BIGINT,
    last_contact_date TIMESTAMP,
    next_follow_up_date TIMESTAMP,
    notes TEXT,
    estimated_value DECIMAL(15,2),
    converted_date TIMESTAMP,
    converted_opportunity_id BIGINT,
    address VARCHAR(200),
    city VARCHAR(100),
    state VARCHAR(100),
    postal_code VARCHAR(20),
    country VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT
);

-- Customers
CREATE TABLE sales_customers (
    customer_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    customer_name VARCHAR(200) NOT NULL,
    customer_type VARCHAR(50) DEFAULT 'Business',
    email VARCHAR(200),
    phone VARCHAR(50),
    industry VARCHAR(50),
    website VARCHAR(200),
    status VARCHAR(20) DEFAULT 'Active',
    assigned_to BIGINT,
    lifetime_value DECIMAL(15,2) DEFAULT 0,
    last_purchase_date TIMESTAMP,
    notes TEXT,
    billing_address VARCHAR(200),
    billing_city VARCHAR(100),
    billing_state VARCHAR(100),
    billing_postal_code VARCHAR(20),
    billing_country VARCHAR(100),
    shipping_address VARCHAR(200),
    shipping_city VARCHAR(100),
    shipping_state VARCHAR(100),
    shipping_postal_code VARCHAR(20),
    shipping_country VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT
);

-- Opportunities
CREATE TABLE sales_opportunities (
    opportunity_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    opportunity_name VARCHAR(200) NOT NULL,
    description TEXT,
    customer_id BIGINT NOT NULL,
    lead_id BIGINT,
    amount DECIMAL(15,2) NOT NULL,
    currency VARCHAR(10) DEFAULT 'USD',
    sales_stage_id BIGINT NOT NULL,
    status VARCHAR(20) DEFAULT 'Open',
    probability DECIMAL(5,2) DEFAULT 50,
    expected_close_date DATE,
    actual_close_date DATE,
    assigned_to BIGINT NOT NULL,
    source VARCHAR(50),
    type VARCHAR(50),
    notes TEXT,
    lost_reason TEXT,
    competitor_id BIGINT,
    last_activity_date TIMESTAMP,
    next_activity_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT,
    FOREIGN KEY (customer_id) REFERENCES sales_customers(customer_id),
    FOREIGN KEY (lead_id) REFERENCES sales_leads(lead_id),
    FOREIGN KEY (sales_stage_id) REFERENCES sales_stages(sales_stage_id)
);

-- Sales Stages
CREATE TABLE sales_stages (
    sales_stage_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    stage_name VARCHAR(100) NOT NULL,
    description TEXT,
    sort_order INTEGER NOT NULL,
    default_probability DECIMAL(5,2) DEFAULT 50,
    color VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    is_closed_stage BOOLEAN DEFAULT FALSE,
    is_won_stage BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Contacts
CREATE TABLE sales_contacts (
    contact_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    customer_id BIGINT NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    full_name VARCHAR(200),
    email VARCHAR(200),
    phone VARCHAR(50),
    job_title VARCHAR(100),
    department VARCHAR(100),
    is_primary BOOLEAN DEFAULT FALSE,
    is_decision_maker BOOLEAN DEFAULT FALSE,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES sales_customers(customer_id)
);

-- Quotes
CREATE TABLE sales_quotes (
    quote_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    quote_number VARCHAR(50) UNIQUE NOT NULL,
    opportunity_id BIGINT,
    customer_id BIGINT NOT NULL,
    quote_name VARCHAR(200),
    description TEXT,
    subtotal DECIMAL(15,2) DEFAULT 0,
    tax_amount DECIMAL(15,2) DEFAULT 0,
    discount_amount DECIMAL(15,2) DEFAULT 0,
    total_amount DECIMAL(15,2) DEFAULT 0,
    currency VARCHAR(10) DEFAULT 'USD',
    status VARCHAR(20) DEFAULT 'Draft',
    valid_until DATE,
    sent_date TIMESTAMP,
    accepted_date TIMESTAMP,
    rejected_date TIMESTAMP,
    rejection_reason TEXT,
    terms_conditions TEXT,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT,
    FOREIGN KEY (opportunity_id) REFERENCES sales_opportunities(opportunity_id),
    FOREIGN KEY (customer_id) REFERENCES sales_customers(customer_id)
);

-- Activities
CREATE TABLE sales_activities (
    activity_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    activity_type VARCHAR(50) NOT NULL,
    subject VARCHAR(200),
    description TEXT,
    lead_id BIGINT,
    opportunity_id BIGINT,
    customer_id BIGINT,
    contact_id BIGINT,
    assigned_to BIGINT,
    status VARCHAR(20) DEFAULT 'Planned',
    priority VARCHAR(20) DEFAULT 'Medium',
    due_date TIMESTAMP,
    completed_date TIMESTAMP,
    duration_minutes INTEGER,
    outcome TEXT,
    follow_up_required BOOLEAN DEFAULT FALSE,
    follow_up_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT
);

-- Customer Interactions
CREATE TABLE sales_customer_interactions (
    interaction_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    customer_id BIGINT NOT NULL,
    contact_id BIGINT,
    interaction_type VARCHAR(50) NOT NULL,
    channel VARCHAR(50),
    subject VARCHAR(200),
    description TEXT,
    interaction_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    duration_minutes INTEGER,
    outcome VARCHAR(100),
    sentiment VARCHAR(20),
    follow_up_required BOOLEAN DEFAULT FALSE,
    follow_up_date TIMESTAMP,
    created_by BIGINT,
    FOREIGN KEY (customer_id) REFERENCES sales_customers(customer_id),
    FOREIGN KEY (contact_id) REFERENCES sales_contacts(contact_id)
);
```

## 4. API Integration

### Sales Dashboard API
```javascript
// api/sales/dashboard.js
import request from '@/utils/request'

export function getDashboardOverview(period = 'month') {
  return request({
    url: '/api/SalesDashboard/overview',
    method: 'get',
    params: { period }
  })
}

export function getSalesMetrics(startDate, endDate) {
  return request({
    url: '/api/SalesDashboard/sales-metrics',
    method: 'get',
    params: { startDate, endDate }
  })
}

export function getPipelineOverview(salesPersonId) {
  return request({
    url: '/api/SalesDashboard/pipeline-overview',
    method: 'get',
    params: { salesPersonId }
  })
}

export function getConversionFunnel(period = 'month') {
  return request({
    url: '/api/SalesDashboard/conversion-funnel',
    method: 'get',
    params: { period }
  })
}

export function getSalesTrends(months = 12) {
  return request({
    url: '/api/SalesDashboard/sales-trends',
    method: 'get',
    params: { months }
  })
}

export function getTeamPerformance(period = 'month') {
  return request({
    url: '/api/SalesDashboard/team-performance',
    method: 'get',
    params: { period }
  })
}

export function getSalesForecast(months = 6) {
  return request({
    url: '/api/SalesDashboard/sales-forecast',
    method: 'get',
    params: { months }
  })
}

export function getTopOpportunities(count = 10) {
  return request({
    url: '/api/SalesDashboard/top-opportunities',
    method: 'get',
    params: { count }
  })
}

export function getRecentActivities(count = 20) {
  return request({
    url: '/api/SalesDashboard/recent-activities',
    method: 'get',
    params: { count }
  })
}
```

### Lead Management API
```javascript
// api/sales/lead.js
import request from '@/utils/request'

export function getLeads(query) {
  return request({
    url: '/api/Lead/leads',
    method: 'get',
    params: query
  })
}

export function getLead(id) {
  return request({
    url: `/api/Lead/leads/${id}`,
    method: 'get'
  })
}

export function createLead(data) {
  return request({
    url: '/api/Lead/leads',
    method: 'post',
    data
  })
}

export function updateLead(id, data) {
  return request({
    url: `/api/Lead/leads/${id}`,
    method: 'put',
    data
  })
}

export function assignLead(id, assignment) {
  return request({
    url: `/api/Lead/leads/${id}/assign`,
    method: 'post',
    data: assignment
  })
}

export function qualifyLead(id, qualification) {
  return request({
    url: `/api/Lead/leads/${id}/qualify`,
    method: 'post',
    data: qualification
  })
}

export function convertLead(id, conversion) {
  return request({
    url: `/api/Lead/leads/${id}/convert`,
    method: 'post',
    data: conversion
  })
}

export function getLeadScore(id) {
  return request({
    url: `/api/Lead/leads/${id}/score`,
    method: 'get'
  })
}

export function getLeadSources() {
  return request({
    url: '/api/Lead/lead-sources',
    method: 'get'
  })
}

export function bulkImportLeads(data) {
  return request({
    url: '/api/Lead/leads/bulk-import',
    method: 'post',
    data
  })
}
```

## 5. Implementation Timeline

### Phase 1: Foundation (Weeks 1-2)
- [ ] Database schema creation and migration
- [ ] Basic models and entities setup
- [ ] Core service interfaces definition
- [ ] Authentication and authorization setup

### Phase 2: Core CRM Features (Weeks 3-5)
- [ ] Lead management implementation
- [ ] Customer management implementation
- [ ] Contact management implementation
- [ ] Basic CRUD operations for all entities

### Phase 3: Sales Pipeline (Weeks 6-8)
- [ ] Opportunity management implementation
- [ ] Sales stages configuration
- [ ] Pipeline visualization
- [ ] Stage transition workflows

### Phase 4: Dashboard & Analytics (Weeks 9-11)
- [ ] Sales dashboard implementation
- [ ] KPI calculations and metrics
- [ ] Charts and visualizations
- [ ] Real-time data updates

### Phase 5: Advanced Features (Weeks 12-14)
- [ ] Quote and proposal management
- [ ] Sales forecasting
- [ ] Lead scoring algorithms
- [ ] Activity tracking and management

### Phase 6: Integration & Testing (Weeks 15-16)
- [ ] API integration testing
- [ ] Frontend-backend integration
- [ ] Performance optimization
- [ ] User acceptance testing

## 6. Key Features

### Sales Dashboard Features
- **Real-time KPI Monitoring**: Track key sales metrics in real-time
- **Interactive Pipeline Visualization**: Kanban-style pipeline management
- **Advanced Analytics**: Sales trends, conversion funnels, team performance
- **Forecasting**: AI-powered sales forecasting and predictions
- **Quick Actions**: Fast access to common sales tasks
- **Customizable Views**: Personalized dashboard layouts

### CRM Features
- **360° Customer View**: Complete customer information and history
- **Lead Scoring**: Automated lead qualification and scoring
- **Activity Tracking**: Comprehensive interaction history
- **Contact Management**: Multiple contacts per customer
- **Integration Ready**: API-first design for third-party integrations

### Sales Pipeline Features
- **Visual Pipeline Management**: Drag-and-drop opportunity management
- **Customizable Stages**: Configurable sales process stages
- **Probability Tracking**: Stage-based probability calculations
- **Automated Workflows**: Stage transition automation
- **Performance Analytics**: Stage conversion analysis

### Reporting & Analytics
- **Sales Performance Reports**: Comprehensive sales analytics
- **Team Performance Tracking**: Individual and team metrics
- **Revenue Forecasting**: Predictive revenue analysis
- **Custom Report Builder**: Flexible reporting capabilities
- **Export Capabilities**: Multiple export formats support

## 7. Technical Considerations

### Performance Optimization
- Database indexing for frequently queried fields
- Caching strategies for dashboard data
- Pagination for large datasets
- Lazy loading for complex relationships

### Security Features
- Role-based access control
- Data encryption for sensitive information
- Audit trails for all sales activities
- API rate limiting and security

### Scalability
- Microservices architecture support
- Database partitioning strategies
- Horizontal scaling capabilities
- Cloud deployment readiness

### Integration Capabilities
- RESTful API design
- Webhook support for real-time updates
- Third-party CRM integration
- Email marketing platform integration
- Calendar and scheduling integration

This comprehensive Sales module will provide a complete CRM and sales pipeline management solution with advanced analytics and forecasting capabilities.