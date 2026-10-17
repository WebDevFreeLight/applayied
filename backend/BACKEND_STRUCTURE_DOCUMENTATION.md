# Backend System Structure Documentation
## Maham ERP Backend - Complete Architecture Overview

---

## 📋 Table of Contents
1. [Solution Overview](#solution-overview)
2. [Project Structure](#project-structure)
3. [Core Layers](#core-layers)
4. [Business Modules](#business-modules)
5. [Infrastructure](#infrastructure)
6. [Database & Data Access](#database--data-access)
7. [API Layer](#api-layer)
8. [Configuration & Deployment](#configuration--deployment)

---

## Solution Overview

**Solution File:** `ZRAdminMahamBackend.sln`

**Technology Stack:**
- .NET 8.0
- ASP.NET Core Web API
- SqlSugar ORM
- Swagger/Scalar API Documentation
- SignalR for Real-time Communication
- Quartz.NET for Scheduled Tasks
- Redis for Caching
- NLog for Logging
- Docker Support

**Architecture Pattern:** Layered Architecture (Repository Pattern + Service Pattern)

---

## Project Structure

```
mahamBackendNET/backend/
├── ZR.Admin.WebApi/          # Main Web API Project (Entry Point)
├── ZR.Model/                 # Entity Models & DTOs
├── ZR.Service/               # Business Logic Services
├── ZR.ServiceCore/           # Core System Services
├── ZR.Repository/            # Data Access Layer
├── ZR.Infrastructure/        # Infrastructure & Utilities
├── ZR.Common/                # Shared Utilities
├── ZR.Tasks/                 # Scheduled Tasks (Quartz.NET)
├── ZR.CodeGenerator/         # Code Generation Tool
├── ZR.Mall/                  # E-Commerce Module
├── ZR.Puppeteer/             # PDF Generation (Puppeteer)
├── ZR.Pdf/                   # PDF Processing
├── Infrastructure/           # Core Infrastructure
└── Documentation/            # Project Documentation
```

---

## Core Layers

### 1. ZR.Admin.WebApi (Entry Point)
**Purpose:** Main Web API application, HTTP endpoints, middleware, startup configuration

**Key Directories:**
```
ZR.Admin.WebApi/
├── Controllers/              # API Controllers (REST endpoints)
│   ├── System/              # System management (users, roles, permissions)
│   ├── HR/                  # Human Resources APIs
│   ├── EventManagement/     # Event Management APIs
│   ├── ERP/                 # ERP Business APIs
│   ├── Business/            # Business Partner APIs
│   ├── Boulevard/           # Boulevard Management APIs
│   ├── Ai/                  # AI Agent APIs
│   ├── Content/             # Content Management APIs
│   └── Public/              # Public APIs
├── Extensions/              # Startup extensions
├── Filters/                 # Action filters, exception handlers
├── Middleware/              # Custom middleware
├── Services/                # Application services
├── Views/                   # Razor views (PDF generation)
├── Database/                # Database scripts & seed data
│   ├── SeedData/           # JSON seed data files
│   └── sys_dict_type/      # System dictionary types
├── wwwroot/                 # Static files
└── Program.cs               # Application entry point
```

**Key Features:**
- RESTful API endpoints
- Swagger/Scalar API documentation
- Global exception handling
- Request validation
- API rate limiting
- SignalR hubs
- Authentication & authorization
- File upload/download
- PDF generation

---

### 2. ZR.Model (Data Models)
**Purpose:** Entity models, DTOs, data transfer objects

**Structure:**
```
ZR.Model/
├── System/                  # System models (users, roles, menus, etc.)
├── HR/                      # HR models
│   ├── Dto/                # HR DTOs
│   ├── HrEmployee.cs
│   ├── HrLeave.cs
│   ├── HrAttendance.cs
│   ├── HrPayroll.cs
│   ├── HrTraining.cs
│   ├── HrLoan.cs
│   └── ...
├── EventManagement/         # Event Management models
│   ├── Dto/                # Event DTOs
│   ├── Event.cs
│   ├── EventTicket.cs
│   ├── EventContract.cs
│   ├── EventPermit.cs
│   └── ...
├── ERP/                     # ERP Business models
│   ├── Sales/              # Sales module
│   ├── Inventory/          # Inventory module
│   ├── Accounts/           # Accounting module
│   ├── Subscriptions/      # Subscription module
│   ├── Treasury/           # Treasury module
│   ├── HR/                 # ERP HR module
│   ├── Marketing/          # Marketing module
│   ├── Suppliers/          # Supplier module
│   ├── Operations/         # Operations module
│   ├── AI/                 # AI Agent models
│   ├── ZATCA/              # ZATCA e-invoicing
│   └── Settings/           # ERP Settings
├── Business/                # Business partner models
├── Boulevard/               # Boulevard management models
├── Businesspartnerdata/     # Business partner data models
├── Partnertype/            # Partner type models
├── Hrtypes/                # HR type models (leave types, holiday types)
├── Hrinsurancetype/        # Insurance type models
├── Content/                # Content management models
├── Public/                 # Public models
├── Social/                 # Social features models
├── Ai/                     # AI models
├── Enum/                   # Enumerations
└── PagedInfo.cs            # Pagination model
```

**Key Attributes:**
- `[SugarTable]` - SqlSugar table mapping
- `[SqlDict]` - Dictionary type mapping
- `[JsonConverter]` - JSON serialization

---

### 3. ZR.Service (Business Logic)
**Purpose:** Business logic implementation, service layer

**Structure:**
```
ZR.Service/
├── HR/                      # HR Services
│   ├── IHrService/         # HR Service interfaces
│   ├── HrEmployeeService.cs
│   ├── HrLeaveService.cs
│   ├── HrAttendanceService.cs
│   ├── HrHolidayService.cs
│   ├── HrTrainingService.cs
│   ├── HrLoanService.cs
│   ├── HrSalaryComplianceService.cs
│   └── ...
├── EventManagement/         # Event Management Services
│   ├── IEventService/      # Event Service interfaces
│   ├── EventService.cs
│   ├── EventTicketService.cs
│   ├── EventContractService.cs
│   ├── EventPermitService.cs
│   ├── TicketCheckinService.cs
│   └── ...
├── ERP/                     # ERP Business Services
│   ├── IErpService/        # ERP Service interfaces
│   ├── ErpCustomerService.cs
│   ├── ErpProductService.cs
│   ├── ErpSalesInvoiceService.cs
│   ├── ErpInventoryTransactionService.cs
│   ├── ErpSubscriptionService.cs
│   ├── ErpAccountService.cs
│   ├── ErpTreasuryService.cs
│   ├── ErpAiService.cs
│   ├── ErpZatcaService.cs
│   ├── Finance/            # Finance services
│   ├── Cash_Bank/          # Cash & Bank services
│   ├── FixedAssets/        # Fixed Assets services
│   ├── Purchase/           # Purchase services
│   └── ProjectManagement/  # Project Management services
├── Business/                # Business services
├── Boulevard/               # Boulevard services
├── Businesspartnerdata/     # Business partner services
├── Partnertype/            # Partner type services
├── Hrtypes/                # HR type services
├── Hrinsurancetype/        # Insurance type services
├── Content/                # Content services
├── Ai/                     # AI services
└── Public/                 # Public services
```

**Service Pattern:**
- Services implement interfaces (`IServiceName`)
- All services inherit from `BaseService<T>`
- Use dependency injection
- Handle business logic and validation
- Call Repository layer for data access

---

### 4. ZR.Repository (Data Access)
**Purpose:** Database operations, repository pattern implementation

**Structure:**
```
ZR.Repository/
├── Base/                    # Base repository classes
├── UnitOfWork/              # Unit of Work pattern
└── ...                      # Repository implementations
```

**Key Features:**
- SqlSugar ORM integration
- Repository pattern
- Unit of Work pattern
- Database abstraction

---

### 5. ZR.ServiceCore (Core Services)
**Purpose:** Core system services, shared business logic

**Structure:**
```
ZR.ServiceCore/
├── Services/                # Core services
│   ├── SeedDataService.cs  # Seed data initialization
│   └── ...
├── Signalr/                 # SignalR hubs
└── SqlSugar/                # SqlSugar extensions
```

**Key Services:**
- Seed data management
- SignalR real-time communication
- Database initialization
- System configuration

---

### 6. Infrastructure (Core Infrastructure)
**Purpose:** Core infrastructure, extensions, utilities

**Structure:**
```
Infrastructure/
├── Attribute/               # Custom attributes
├── Cache/                   # Caching utilities
├── Constant/                # Constants
├── Controllers/             # Base controllers
├── Converter/               # Type converters
├── CustomException/         # Custom exceptions
├── Enums/                   # Enumerations
├── Extensions/              # Extension methods
├── Helper/                  # Helper classes
├── IPTools/                 # IP utilities
├── Model/                   # Infrastructure models
├── WebExtensions/           # Web extensions
├── AppSettings.cs           # App settings
├── GlobalConstant.cs        # Global constants
├── InternalApp.cs           # Internal app configuration
├── JwtUtil.cs               # JWT utilities
└── Log.cs                   # Logging utilities
```

---

### 7. ZR.Common (Shared Utilities)
**Purpose:** Shared utility classes and helpers

**Structure:**
```
ZR.Common/
├── DynamicApiSimple/        # Dynamic API implementation
├── AliyunOssHelper.cs       # Aliyun OSS helper
├── ExcelHelper.cs           # Excel operations
├── MailHelper.cs            # Email sending
├── Tools.cs                 # Common tools
├── WxHelper.cs              # WeChat helper
└── WxNoticeHelper.cs        # WeChat notifications
```

---

### 8. ZR.Tasks (Scheduled Tasks)
**Purpose:** Background tasks using Quartz.NET

**Structure:**
```
ZR.Tasks/
└── ...                      # Task implementations
```

**Features:**
- Scheduled job execution
- Task scheduling
- Job monitoring

---

### 9. ZR.CodeGenerator (Code Generation)
**Purpose:** Automated code generation tool

**Structure:**
```
ZR.CodeGenerator/
├── Model/                   # Generator models
├── Service/                 # Generator services
├── CodeGeneratorTool.cs     # Main generator
├── DbProvider.cs            # Database provider
└── GenConstants.cs          # Generation constants
```

**Features:**
- Generate CRUD controllers
- Generate service classes
- Generate DTOs
- Generate Vue frontend code
- Generate SQL scripts

---

### 10. ZR.Mall (E-Commerce)
**Purpose:** E-commerce module (in development)

**Structure:**
```
ZR.Mall/
└── ...                      # Mall/Shop functionality
```

---

### 11. ZR.Puppeteer & ZR.Pdf (PDF Generation)
**Purpose:** PDF generation and processing

**Structure:**
```
ZR.Puppeteer/                # Puppeteer-based PDF generation
ZR.Pdf/                      # PDF processing utilities
```

---

## Business Modules

### 1. Human Resources (HR) Module
**Controllers:** `Controllers/HR/`
**Services:** `ZR.Service/HR/`
**Models:** `ZR.Model/HR/`

**Features:**
- Employee management
- Leave management
- Attendance tracking
- Payroll processing
- Training programs
- Loan management
- Insurance enrollment
- Performance reviews
- Salary compliance
- Holidays management

**Key Models:**
- `HrEmployee` - Employee information
- `HrLeave` - Leave requests
- `HrAttendance` - Attendance records
- `HrPayroll` - Payroll data
- `HrTraining` - Training programs
- `HrLoan` - Employee loans
- `HrHoliday` - Company holidays
- `HrSalaryCompliance` - Salary compliance rules

---

### 2. Event Management Module
**Controllers:** `Controllers/EventManagement/`
**Services:** `ZR.Service/EventManagement/`
**Models:** `ZR.Model/EventManagement/`

**Features:**
- Event creation and management
- Ticket management
- Contract management
- Permit management
- Visitor management
- Gate access control
- Ticket check-in
- Financial tracking
- Invoice management
- Payment processing

**Key Models:**
- `Event` - Event information
- `EventTicket` - Ticket types and sales
- `EventContract` - Event contracts
- `EventPermit` - Access permits
- `EventVisitor` - Visitor information
- `EventGate` - Gate access points
- `TicketCheckin` - Check-in records
- `EventInvoice` - Event invoices
- `EventPayment` - Payment records

---

### 3. ERP Module
**Controllers:** `Controllers/ERP/`
**Services:** `ZR.Service/ERP/`
**Models:** `ZR.Model/ERP/`

**Sub-modules:**
- **Sales:** Customer management, sales orders, invoices
- **Inventory:** Product management, warehouse management, stock tracking
- **Accounts:** Chart of accounts, journal entries, financial reports
- **Subscriptions:** Subscription packages, members, bookings
- **Treasury:** Cash management, treasury transactions
- **Suppliers:** Supplier management, supplier balances
- **Marketing:** Promotions, marketing campaigns
- **Operations:** Daily operations, shifts
- **AI:** AI agent integration
- **ZATCA:** E-invoicing compliance (Saudi Arabia)

**Key Models:**
- `ErpCustomer` - Customer information
- `ErpProduct` - Product catalog
- `ErpSalesInvoice` - Sales invoices
- `ErpInventoryTransaction` - Inventory movements
- `ErpSubscription` - Subscription packages
- `ErpAccount` - Chart of accounts
- `ErpTreasury` - Treasury management

---

### 4. Boulevard Management Module
**Controllers:** `Controllers/Boulevard/`
**Services:** `ZR.Service/Boulevard/`
**Models:** `ZR.Model/Boulevard/`

**Features:**
- Boulevard seasons
- Shop management
- Zone management
- Customer registration

---

### 5. AI Module
**Controllers:** `Controllers/Ai/`
**Services:** `ZR.Service/Ai/`
**Models:** `ZR.Model/Ai/`, `ZR.Model/ERP/AI/`

**Features:**
- AI agent management
- AI model providers
- AI team modes
- AI tool types
- AI decision making
- AI thinking logs

---

## Database & Data Access

### Database Configuration
**ORM:** SqlSugar
**Database Support:** PostgreSQL, MySQL, SQL Server, Oracle

### Seed Data
**Location:** `ZR.Admin.WebApi/Database/SeedData/`
- JSON seed data files
- Automatic initialization on startup
- Support for HR, Event Management, ERP modules

### System Dictionary
**Location:** `ZR.Admin.WebApi/Database/sys_dict_type/`
- Dictionary type definitions
- SQL scripts for dictionary initialization
- JSON data files

### Database Scripts
**Location:** `sql/`, `Database/`
- Schema creation scripts
- Migration scripts
- Seed data scripts

---

## API Layer

### API Documentation
- **Swagger UI:** `/swagger`
- **Scalar API:** Alternative documentation UI
- **API Versioning:** Supported

### API Structure
```
/api/
├── /system/                 # System APIs
├── /hr/                     # HR APIs
├── /event-management/       # Event Management APIs
├── /erp/                    # ERP APIs
├── /boulevard/              # Boulevard APIs
├── /business/               # Business APIs
├── /ai/                     # AI APIs
└── /public/                 # Public APIs
```

### API Features
- RESTful design
- JWT authentication
- Role-based authorization
- API rate limiting
- Request validation
- Global exception handling
- CORS support

---

## Configuration & Deployment

### Configuration Files
```
ZR.Admin.WebApi/
├── appsettings.json         # Main configuration
├── appsettings.Development.json
├── appsettings.Production.json
├── appsettings.Staging.json
└── appsettings.Docker.json
```

### Docker Support
- `docker-compose.yml` - Docker compose configuration
- `Dockerfile` - Container definition
- Multi-environment support

### Deployment Scripts
- `deploy-script.sh` - Linux deployment
- `deploy-to-synology.bat` - Synology deployment
- `setup-server.sh` - Server setup
- `setup-ssl.sh` - SSL certificate setup

### Environment Configuration
- Development
- Staging
- Production
- Docker

---

## Key Features

### 1. Authentication & Authorization
- JWT token-based authentication
- Role-based access control (RBAC)
- Permission management
- User session management

### 2. Logging
- NLog integration
- Operation logs
- Login logs
- Error logs
- Audit trails

### 3. Caching
- In-memory caching
- Redis caching
- Cache invalidation

### 4. File Management
- File upload/download
- Aliyun OSS integration
- Local file storage

### 5. Email
- Email sending
- Email templates
- Email logging

### 6. Real-time Communication
- SignalR hubs
- Online user tracking
- Real-time notifications

### 7. PDF Generation
- Puppeteer-based PDF generation
- Razor views for PDF templates
- Invoice PDF generation

### 8. Code Generation
- Automated CRUD code generation
- Frontend code generation
- Database script generation

---

## Development Workflow

### 1. Adding New Feature
1. Create model in `ZR.Model/`
2. Create DTO in `ZR.Model/*/Dto/`
3. Create service interface in `ZR.Service/*/IService/`
4. Implement service in `ZR.Service/`
5. Create controller in `ZR.Admin.WebApi/Controllers/`
6. Register services in `Program.cs`
7. Add seed data if needed

### 2. Database Changes
1. Create migration script in `sql/`
2. Update model with `[SugarTable]` attribute
3. Update seed data if needed
4. Test migration

### 3. Code Generation
1. Configure code generator
2. Select database tables
3. Generate code templates
4. Customize generated code

---

## Dependencies

### Main NuGet Packages
- `SqlSugar` - ORM
- `Swashbuckle.AspNetCore` - Swagger
- `SignalR` - Real-time communication
- `Quartz` - Task scheduling
- `StackExchange.Redis` - Redis client
- `NLog` - Logging
- `Newtonsoft.Json` - JSON serialization
- `AutoMapper` - Object mapping

---

## Documentation

**Location:** Root directory and `docs/` folder

**Key Documents:**
- `README.md` - Main readme
- `ERP_DESIGN.md` - ERP system design
- `EVENT_MANAGEMENT_COMPLETE_SUMMARY.md` - Event management docs
- `HR_SEED_DATA_README.md` - HR seed data documentation
- `COMPREHENSIVE_HR_SEED_DATA_README.md` - Comprehensive HR docs
- Various deployment and setup guides

---

## Testing & Quality

### Error Handling
- Global exception filter
- Custom exception types
- Error logging
- User-friendly error messages

### Validation
- Model validation
- Request validation
- Business rule validation

---

## Security

### Features
- JWT authentication
- HTTPS support
- API rate limiting
- SQL injection prevention
- XSS protection
- CSRF protection
- Data encryption

---

## Performance

### Optimizations
- Database query optimization
- Caching strategies
- Lazy loading
- Pagination
- Async/await patterns

---

## Maintenance

### Monitoring
- Application logs
- Error tracking
- Performance monitoring
- Health checks

### Backup
- Database backups
- File backups
- Configuration backups

---

## Future Enhancements

- Enhanced AI integration
- More ERP modules
- Mobile API support
- Microservices migration
- GraphQL API
- Event sourcing

---

**Last Updated:** 2025
**Version:** 1.0.0
**Maintainer:** Maham Development Team

