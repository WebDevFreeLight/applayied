using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using PuppeteerSharp;
using System.Text;
using PuppeteerSharp.Media;
using ZR.Pdf.Core;
using ZR.Infrastructure.Extensions.Qrcode;
using ZR.Service.EventManagement.IEventManagementService;
using Mapster;
using System.IO;
using ZR.Pdf.Mvc;
using ZR.Service.EventManagement;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    /// <summary>
    /// Event Management PDF Export Controller
    /// </summary>
    [Route("event/pdf")]
    public class EventPdfExportController : BaseController
    {
        private readonly IPdfRenderer _pdfRenderer;
        private readonly IEventService _eventService;
        private readonly IEventTicketService _ticketService;
        private readonly IEventVisitorService _visitorService;
        private readonly IEventZoneService _zoneService;
        private readonly IEventPermitService _permitService;
        private readonly IRazorViewRender _razorViewRender;

        public EventPdfExportController(
            IPdfRenderer pdfRenderer,
            IEventService eventService,
            IEventTicketService ticketService,
            IEventVisitorService visitorService,
            IEventZoneService zoneService,
            IEventPermitService permitService,
            IRazorViewRender razorViewRender)
        {
            _pdfRenderer = pdfRenderer;
            _eventService = eventService;
            _ticketService = ticketService;
            _visitorService = visitorService;
            _zoneService = zoneService;
            _permitService = permitService;
            _razorViewRender = razorViewRender;
        }

        /// <summary>
        /// Export Event Details to PDF
        /// </summary>
        [HttpGet("event/{eventId}")]
        //[ActionPermissionFilter(Permission = "event:export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportEventPdf(long eventId)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportEventPdf - Starting export for eventId: {eventId}");

                var eventData = _eventService.GetInfo(eventId); //
                if (eventData == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportEventPdf - Event not found, using sample data for eventId: {eventId}");

                    eventData = new Event()
                    {
                        Eventid = 0,
                        Eventcode = "EVT" + DateTime.Now.ToString("yyyyMMdd"),
                        Eventname = "New Event",
                        Eventnamear = "فعالية جديدة",
                        Description = "Event description",
                        Descriptionar = "وصف الفعالية",
                        Eventtype = "conference",
                        Status = "planned",
                        Startdate = DateTime.Now,
                        Enddate = DateTime.Now.AddDays(1),
                        Registrationstartdate = DateTime.Now,
                        Registrationenddate = DateTime.Now.AddDays(1),
                        Locationid  = 500,
                        Locationname = "Main Hall",
                        Address = "Event Address",
                        City = "Riyadh",
                        Latitude = 50,
                        Longitude = 980,
                        Mapimageurl = "",
                        Expectedvisitors = 100,
                        Maxcapacity = 200,
                        Currentoccupancy = 0,
                        Organizer = "Event Organizer",
                        Contactemail = "info@event.com",
                        Contactphone = "+966500000000",
                        Website = "https://event.com",
                        Logourl = "",
                        Bannerimageurl = "",
                        Ispublic = "Y",
                        Requiresregistration = "N",
                        Ispaidevent = "N",
                        Ticketprice = 9500,
                        Currency = "SAR",
                        Eventmanagerid = 0,
                        Tags = "",
                        Isactive = "Y",
                        Actualcost = 0,
                        Budget = 0,
                        Totalareasqm = 0,
                        Expectedattendance = 100,
                        Country = "Saudi Arabia",
                        Venue = "Conference Center"
                    };
                    //return ToResponse(ResultCode.FAIL, "Event not found"); s
                }

                var html = await _razorViewRender.RenderViewToStringAsync("EventDetails", eventData, new ()
                {
                    "/Views/EventPdfExport/EventDetails.cshtml"
                });
                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions { Top = "20px", Bottom = "20px", Left = "20px", Right = "20px" }
                };

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportEventPdf - Successfully generated PDF for eventId: {eventId}");

                return File(pdfData, "application/pdf", $"Event_{eventData.Eventcode}___{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportEventPdf - eventId: {eventId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate event PDF", message = ex.Message });
            }
        }

        /// <summary>
        /// Export Event Ticket to PDF
        /// </summary>
        [HttpGet("ticket/{ticketId}")]
        //[ActionPermissionFilter(Permission = "event:ticket:export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportTicketPdf(long ticketId)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Starting export for ticketId: {ticketId}");

                EventTicket ticket = _ticketService.GetInfo(ticketId);
                
                if (ticket == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Ticket not found for ticketId: {ticketId}");
                    return ToResponse(ResultCode.FAIL, "Ticket not found");
                }

                // Validate that ticket has a visitor ID
                if (ticket.VisitorId <= 0)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Ticket {ticketId} does not have a valid VisitorId");
                    return ToResponse(ResultCode.FAIL, "Ticket must have a visitor assigned");
                }

                // Fetch visitor information - REQUIRED for ticket printing
                EventVisitor visitor = null;
                string visitorName = null;

                try
                {
                    visitor = _visitorService.GetInfo(ticket.VisitorId);
                    
                    if (visitor == null)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Visitor not found for VisitorId: {ticket.VisitorId}");
                        return ToResponse(ResultCode.FAIL, $"Visitor not found for ticket. Visitor ID: {ticket.VisitorId}");
                    }

                    // Build visitor name from FirstName and LastName
                    var firstName = visitor.FirstName ?? "";
                    var lastName = visitor.LastName ?? "";
                    visitorName = $"{firstName} {lastName}".Trim();

                    // Validate that visitor has a name
                    if (string.IsNullOrWhiteSpace(visitorName))
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Visitor {ticket.VisitorId} does not have a name");
                        return ToResponse(ResultCode.FAIL, "Visitor name is required but not found. Please update visitor information.");
                    }

                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Found visitor: {visitorName} (ID: {visitor.VisitorId})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Error fetching visitor: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    return ToResponse(ResultCode.FAIL, $"Failed to retrieve visitor information: {ex.Message}");
                }

                // Final validation - visitor name must be present
                if (string.IsNullOrWhiteSpace(visitorName))
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Visitor name is missing for ticket {ticketId}");
                    return ToResponse(ResultCode.FAIL, "Visitor name is required for ticket printing. Please ensure the visitor has a valid name.");
                }

                // Create view model with ticket and visitor name (guaranteed to be non-null at this point)
                var viewModel = new TicketViewModel
                {
                    Ticket = ticket,
                    VisitorName = visitorName
                };

                var html = await _razorViewRender.RenderViewToStringAsync("Ticket", viewModel, new ()
                {
                    "/Views/EventPdfExport/Ticket.cshtml"
                });
                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions { Top = "10px", Bottom = "10px", Left = "10px", Right = "10px" }
                };

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportTicketPdf - Successfully generated PDF for ticketId: {ticketId}");

                return File(pdfData, "application/pdf", $"Ticket_{ticket.TicketNumber}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportTicketPdf - ticketId: {ticketId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate ticket PDF", message = ex.Message });
            }
        }


        /// <summary>
        /// Export Visitor Badge to PDF
        /// </summary>
        /// <param name="visitorId">Visitor ID</param>
        /// <param name="version">Badge template version (default, v1, v2)</param>
        [HttpGet("visitor/badge/{visitorId}")]
        //[ActionPermissionFilter(Permission = "event:visitor:export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportVisitorBadgePdf(long visitorId=1, [FromQuery] string version = "default")
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportVisitorBadgePdf - Starting export for visitorId: {visitorId}, version: {version}");

                EventVisitor visitor = _visitorService.GetInfo(visitorId);
                if (visitor == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportVisitorBadgePdf - Visitor not found, using sample data for visitorId: {visitorId}");

                    // Real data example for EventVisitor object (updated to match new schema)
                    visitor = new EventVisitor
                    {
                        VisitorId = 60025,
                        VisitorCode = "VIS-2024-00125",
                        FirstName = "Ahmed",
                        LastName = "Alotaibi",
                        FirstNameAr = "أحمد",
                        LastNameAr = "العتيبي",
                        Email = "ahmed.alotaibi@techcompany.com",
                        PhoneNumber = "+966551234567",
                        NationalId = "1122334455",
                        PassportNumber = "1122334455",
                        IdentificationNumber = "1122334455",
                        IdentificationType = "national_id",
                        Nationality = "Saudi",
                        NationalityCode = "SA",
                        Gender = "Male",
                        DateOfBirth = new DateTime(1985, 5, 15),
                        VisitorType = "vip",
                        PhotoUrl = "/uploads/visitors/ahmed_alotaibi.jpg",
                        VerificationStatus = "verified",
                        Notes = "زائر VIP - مدعو من إدارة المعرض"
                    };

                    //return ToResponse(ResultCode.FAIL, "Visitor not found");
                }

                // Determine which template to use based on version parameter
                string templateName;
                string templatePath;
                PdfOptions pdfOptions;

                switch (version.ToLower())
                {
                    case "v1":
                        templateName = "VisitorBadgeV1";
                        templatePath = "/Views/EventPdfExport/VisitorBadgeV1.cshtml";
                        pdfOptions = new PdfOptions
                        {
                            PrintBackground = true,
                            Width = "720px",
                            Height = "960px",
                            MarginOptions = new MarginOptions { Top = "0px", Bottom = "0px", Left = "0px", Right = "0px" }
                        };
                        break;

                    case "v2":
                        templateName = "VisitorBadgeV2";
                        templatePath = "/Views/EventPdfExport/VisitorBadgeV2.cshtml";
                        pdfOptions = new PdfOptions
                        {
                            PrintBackground = true,
                            Width = "4in",
                            Height = "6in",
                            MarginOptions = new MarginOptions { Top = "0px", Bottom = "0px", Left = "0px", Right = "0px" }
                        };
                        break;
                    default:
                        templateName = "VisitorBadge";
                        templatePath = "/Views/EventPdfExport/VisitorBadge.cshtml";
                        pdfOptions = new PdfOptions
                        {
                            PrintBackground = true,
                            Width = "4in",
                            Height = "6in",
                            MarginOptions = new MarginOptions { Top = "10px", Bottom = "10px", Left = "10px", Right = "10px" }
                        };
                        break;
                }

                var html = await _razorViewRender.RenderViewToStringAsync(templateName, visitor, new ()
                {
                    templatePath
                });

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportVisitorBadgePdf - Successfully generated PDF for visitorId: {visitorId} with version: {version}");

                return File(pdfData, "application/pdf", $"Badge_{visitor.VisitorId}_{version}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportVisitorBadgePdf - visitorId: {visitorId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate visitor badge PDF", message = ex.Message });
            }
        }

        /// <summary>
        /// Export Event Zone Map to PDF
        /// </summary>
        [HttpGet("zone/map/{eventId}")]
        //[ActionPermissionFilter(Permission = "event:zone:export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportZoneMapPdf(long eventId)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportZoneMapPdf - Starting export for eventId: {eventId}");

                Event eventData = _eventService.GetInfo(eventId);
                if (eventData == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportZoneMapPdf - Event not found, using sample data for eventId: {eventId}");

                    // Real data example for Event object
                    eventData = new Event
                    {
                        Eventid = 5001,
                        Eventcode = "TECH-EXPO-2024",
                        Eventname = "Saudi Technology Expo 2024",
                        Eventnamear = "المعرض التقني السعودي 2024",
                        Description = "The largest technology exhibition in Saudi Arabia showcasing the latest innovations in AI, IoT, and digital transformation. Featuring keynote speakers, workshops, and networking opportunities with industry leaders.",
                        Descriptionar = "أكبر معرض تقني في المملكة العربية السعودية يعرض أحدث الابتكارات في الذكاء الاصطناعي وإنترنت الأشياء والتحول الرقمي. يشمل متحدثين رئيسيين وورش عمل وفرص للتواصل مع قادة الصناعة.",
                        Eventtype = "exhibition",
                        Status = "active",
                        Startdate = new DateTime(2024, 3, 1),
                        Enddate = new DateTime(2024, 3, 5),
                        Registrationstartdate = new DateTime(2024, 1, 1),
                        Registrationenddate = new DateTime(2024, 2, 28),
                        Locationid = 1001,
                        Locationname = "Riyadh International Convention & Exhibition Center",
                        Address = "King Abdullah Road, Al Murabba",
                        City = "Riyadh",
                        Latitude = 24.7136m,
                        Longitude = 46.6753m,
                        Mapimageurl = "/uploads/events/tech-expo-2024-map.jpg",
                        Expectedvisitors = 15000,
                        Maxcapacity = 20000,
                        Currentoccupancy = 12450,
                        Organizer = "Saudi Technology Commission",
                        Contactemail = "info@tech-expo-saudi.com",
                        Contactphone = "+966112345678",
                        Website = "https://www.tech-expo-saudi.com",
                        Logourl = "/uploads/events/tech-expo-2024-logo.png",
                        Bannerimageurl = "/uploads/events/tech-expo-2024-banner.jpg",
                        Ispublic = "Y",
                        Requiresregistration = "Y",
                        Ispaidevent = "Y",
                        Ticketprice = 150.00m,
                        Currency = "SAR",
                        Eventmanagerid = 1001,
                        Tags = "technology,ai,innovation,digital transformation,saudi vision 2030",
                        Isactive = "Y",
                        Actualcost = 2850000.00m,
                        Budget = 3000000.00m,
                        Totalareasqm = 25000.00m,
                        Expectedattendance = 15000.00m,
                        Country = "Saudi Arabia",
                        Venue = "مركز الرياض للمؤتمرات والمعارض"
                    };

                    //return ToResponse(ResultCode.FAIL, "Event not found");//
                }

                // Get all zones for this event
                var zones = _zoneService.GetList(new EventZoneQueryDto { EventId = eventId });

                List<EventZone> zoneList;

                // If zones or Result is null, provide sample data
                if (zones == null || zones.Result == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportZoneMapPdf - Zones not found, generating sample data for eventId: {eventId}");
                    zoneList = GenerateSampleZoneData(eventId);
                }
                else
                {
                    zoneList = zones.Result.Adapt<List<EventZone>>();
                }

                var viewModel = new EventZoneMapViewModel
                {
                    EventData = eventData,
                    Zones = zoneList
                };

                var html = await _razorViewRender.RenderViewToStringAsync("ZoneMap", viewModel, new ()
                {
                    "/Views/EventPdfExport/ZoneMap.cshtml"
                });

                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.A3,
                    Landscape = true,
                    MarginOptions = new MarginOptions { Top = "10px", Bottom = "10px", Left = "10px", Right = "10px" }
                };

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportZoneMapPdf - Successfully generated PDF for eventId: {eventId}");

                return File(pdfData, "application/pdf", $"ZoneMap_{eventData.Eventcode}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportZoneMapPdf - eventId: {eventId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate zone map PDF", message = ex.Message });
            }
        }

        private List<EventZone> GenerateSampleZoneData(long eventId)
        {


            return new List<EventZone>
    {
        new EventZone
        {
            ZoneId = 7001,
            EventId = eventId,
            ZoneCode = "MAIN-HALL",
            ZoneName = "Main Exhibition Hall",
            ZoneNameAr = "القاعة الرئيسية للمعرض",
            Description = "Primary exhibition area featuring major technology companies and innovations",
            ZoneType = "exhibition",
            ParentZoneId = 150,
            MaxCapacity = 5000,
            CurrentOccupancy = 4200,
            AreaSqm = 8000.00m,
            FloorNumber = 1,
            BuildingName = "Main Convention Center",
            MapCoordinates = @"[{""x"": 100, ""y"": 150}, {""x"": 400, ""y"": 150}, {""x"": 400, ""y"": 450}, {""x"": 100, ""y"": 450}]",
            ColorCode = "#3498db",
            AccessLevel = "public",
            ZoneManagerId = 1002,
            Status = "active",
            DisplayOrder = 1,
            IsActive = true
        },
        new EventZone
        {
            ZoneId = 7002,
            EventId = eventId,
            ZoneCode = "VIP-LOUNGE",
            ZoneName = "VIP Lounge",
            ZoneNameAr = "صالة كبار الشخصيات",
            Description = "Exclusive area for VIP guests with premium amenities",
            ZoneType = "vip",
            ParentZoneId = 500,
            MaxCapacity = 200,
            CurrentOccupancy = 150,
            AreaSqm = 500.00m,
            FloorNumber = 1,
            BuildingName = "Main Convention Center",
            MapCoordinates = @"[{""x"": 450, ""y"": 100}, {""x"": 600, ""y"": 100}, {""x"": 600, ""y"": 250}, {""x"": 450, ""y"": 250}]",
            ColorCode = "#e74c3c",
            AccessLevel = "vip",
            ZoneManagerId = 1003,
            Status = "active",
            DisplayOrder = 2,
            IsActive = true
        },
        new EventZone
        {
            ZoneId = 7003,
            EventId = eventId,
            ZoneCode = "FOOD-COURT",
            ZoneName = "Food Court",
            ZoneNameAr = "ساحة المطاعم",
            Description = "Dining area with various food and beverage options",
            ZoneType = "food",
            ParentZoneId = 950,
            MaxCapacity = 800,
            CurrentOccupancy = 350,
            AreaSqm = 1200.00m,
            FloorNumber = 1,
            BuildingName = "Main Convention Center",
            MapCoordinates = @"[{""x"": 50, ""y"": 500}, {""x"": 300, ""y"": 500}, {""x"": 300, ""y"": 650}, {""x"": 50, ""y"": 650}]",
            ColorCode = "#2ecc71",
            AccessLevel = "public",
            ZoneManagerId = 1004,
            Status = "active",
            DisplayOrder = 3,
            IsActive = true
        },
        new EventZone
        {
            ZoneId = 7004,
            EventId = eventId,
            ZoneCode = "AI-PAVILION",
            ZoneName = "AI & Robotics Pavilion",
            ZoneNameAr = "جناح الذكاء الاصطناعي والروبوتات",
            Description = "Dedicated area for artificial intelligence and robotics demonstrations",
            ZoneType = "exhibition",
            ParentZoneId = 7001,
            MaxCapacity = 300,
            CurrentOccupancy = 280,
            AreaSqm = 600.00m,
            FloorNumber = 1,
            BuildingName = "Main Convention Center",
            MapCoordinates = @"[{""x"": 150, ""y"": 200}, {""x"": 300, ""y"": 200}, {""x"": 300, ""y"": 350}, {""x"": 150, ""y"": 350}]",
            ColorCode = "#9b59b6",
            AccessLevel = "public",
            ZoneManagerId = 1005,
            Status = "active",
            DisplayOrder = 4,
            IsActive = true
        },
        new EventZone
        {
            ZoneId = 7005,
            EventId = eventId,
            ZoneCode = "STARTUP-ZONE",
            ZoneName = "Startup Innovation Zone",
            ZoneNameAr = "منطقة الابتكار للشركات الناشئة",
            Description = "Area dedicated to startups and innovative projects",
            ZoneType = "exhibition",
            ParentZoneId = 7001,
            MaxCapacity = 150,
            CurrentOccupancy = 120,
            AreaSqm = 400.00m,
            FloorNumber = 1,
            BuildingName = "Main Convention Center",
            MapCoordinates = @"[{""x"": 320, ""y"": 200}, {""x"": 450, ""y"": 200}, {""x"": 450, ""y"": 300}, {""x"": 320, ""y"": 300}]",
            ColorCode = "#f39c12",
            AccessLevel = "public",
            ZoneManagerId = 1006,
            Status = "active",
            DisplayOrder = 5,
            IsActive = true
        },
        new EventZone
        {
            ZoneId = 7006,
            EventId = eventId,
            ZoneCode = "CONFERENCE-A",
            ZoneName = "Conference Room A",
            ZoneNameAr = "قاعة المؤتمرات أ",
            Description = "Main conference room for workshops and presentations",
            ZoneType = "conference",
            ParentZoneId = null,
            MaxCapacity = 500,
            CurrentOccupancy = 320,
            AreaSqm = 800.00m,
            FloorNumber = 2,
            BuildingName = "Conference Building",
            MapCoordinates = @"[{""x"": 500, ""y"": 300}, {""x"": 700, ""y"": 300}, {""x"": 700, ""y"": 500}, {""x"": 500, ""y"": 500}]",
            ColorCode = "#34495e",
            AccessLevel = "public",
            ZoneManagerId = 1007,
            Status = "active",
            DisplayOrder = 6,
            IsActive = true
        },
        new EventZone
        {
            ZoneId = 7007,
            EventId = eventId,
            ZoneCode = "MEDIA-CENTER",
            ZoneName = "Media Center",
            ZoneNameAr = "مركز الإعلام",
            Description = "Designated area for media and press coverage",
            ZoneType = "media",
            ParentZoneId = null,
            MaxCapacity = 100,
            CurrentOccupancy = 45,
            AreaSqm = 300.00m,
            FloorNumber = 1,
            BuildingName = "Main Convention Center",
            MapCoordinates = @"[{""x"": 650, ""y"": 50}, {""x"": 800, ""y"": 50}, {""x"": 800, ""y"": 150}, {""x"": 650, ""y"": 150}]",
            ColorCode = "#95a5a6",
            AccessLevel = "restricted",
            ZoneManagerId = 1008,
            Status = "active",
            DisplayOrder = 7,
            IsActive = true
        },
        new EventZone
        {
            ZoneId = 7008,
            EventId = eventId,
            ZoneCode = "NETWORKING",
            ZoneName = "Networking Area",
            ZoneNameAr = "منطقة التواصل",
            Description = "Open space for networking and business meetings",
            ZoneType = "networking",
            ParentZoneId = null,
            MaxCapacity = 300,
            CurrentOccupancy = 180,
            AreaSqm = 600.00m,
            FloorNumber = 1,
            BuildingName = "Main Convention Center",
            MapCoordinates = @"[{""x"": 350, ""y"": 500}, {""x"": 550, ""y"": 500}, {""x"": 550, ""y"": 650}, {""x"": 350, ""y"": 650}]",
            ColorCode = "#1abc9c",
            AccessLevel = "public",
            ZoneManagerId = 1009,
            Status = "active",
            DisplayOrder = 8,
            IsActive = true
        }
    };

        }


        /// <summary>
        /// Export Event Permit to PDF
        /// </summary>
        [HttpGet("permit/{permitId}")]
        //[ActionPermissionFilter(Permission = "event:permit:export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPermitPdf(long permitId)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportPermitPdf - Starting export for permitId: {permitId}");

                EventPermit permit = _permitService.GetInfo(permitId);
                if (permit == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportPermitPdf - Permit not found, using sample data for permitId: {permitId}");

                    // Sample data for EventPermit
                    permit = new EventPermit
                    {
                        PermitId = 1001,
                        EventId = 5001,
                        EmployeeId = "[1,2,3]",
                        VisitorId = "[101,102]",
                        PermitNumber = "PER-2024-001001",
                        PermitType = "vip",
                        PermitCategory = "premium",
                        ApplicantName = "Ahmed Mohammed",
                        ApplicantNameAr = "أحمد محمد",
                        CompanyName = "Tech Solutions Ltd",
                        NationalId = "1122334455",
                        ContactPhone = "+966551234567",
                        Email = "ahmed@techsolutions.com",
                        ValidFrom = DateTime.Now,
                        ValidUntil = DateTime.Now.AddDays(30),
                        IssueDate = DateTime.Now,
                        IssuedBy = "Admin",
                        QRCode = GenerateQRCodeBase64($"PER-2024-001001"),
                        ZoneAccess = "[1,2,3,4]",
                        GateAccess = "[1,2]",
                        AccessLevel = "full",
                        Purpose = "VIP Access for Exhibition",
                        VehiclePlateNumber = "ABC-1234",
                        VehicleType = "Car",
                        PhotoUrl = "/uploads/permits/photo.jpg",
                        AttachmentUrl = "/uploads/permits/document.pdf",
                        Status = "approved",
                        ApprovedBy = 1,
                        ApprovalDate = DateTime.Now.AddDays(-1),
                        RejectionReason = "",
                        Notes = "VIP permit with full access to all zones",
                        ApprovedDate = DateTime.Now.AddDays(-1),
                        HolderName = "Ahmed Mohammed"
                    };
                }

                // Generate QR code if not exists
                if (string.IsNullOrEmpty(permit.QRCode))
                {
                    permit.QRCode = GenerateQRCodeBase64(permit.PermitNumber);
                }

                // Convert photo URL to Base64 if exists
                if (!string.IsNullOrEmpty(permit.PhotoUrl))
                {
                    permit.PhotoUrl = await ConvertImageToBase64(permit.PhotoUrl);
                }

                var html = await _razorViewRender.RenderViewToStringAsync("Permit", permit, new ()
                {
                    "/Views/EventPdfExport/Permit.cshtml"
                });

                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions { Top = "10px", Bottom = "10px", Left = "10px", Right = "10px" }
                };

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportPermitPdf - Successfully generated PDF for permitId: {permitId}");

                return File(pdfData, "application/pdf", $"Permit_{permit.PermitNumber}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportPermitPdf - permitId: {permitId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate permit PDF", message = ex.Message });
            }
        }

        /// <summary>
        /// Generate QR Code as Base64 string
        /// </summary>
        private string GenerateQRCodeBase64(string content)
        {
            try
            {

                if (string.IsNullOrEmpty(content))
                {
                    content = "1230";
                }
                var qrCodeBytes = content.GenerateQRCodeBase64();//50
                return $"{qrCodeBytes}";
                return $"data:image/png;base64,{qrCodeBytes}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating QR code: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert image URL to Base64 string
        /// </summary>
        private async Task<string> ConvertImageToBase64(string imageUrl)
        {
            try
            {
                // Check if already Base64
                if (imageUrl.StartsWith("data:image"))
                {
                    return imageUrl;
                }

                // Handle local file path
                if (imageUrl.StartsWith("/"))
                {
                    var basePath = Directory.GetCurrentDirectory();
                    var fullPath = Path.Combine(basePath, "wwwroot", imageUrl.TrimStart('/'));

                    if (global::System.IO.File.Exists(fullPath))
                    {
                        var bytes = await global::System.IO.File.ReadAllBytesAsync(fullPath);
                        var extension = Path.GetExtension(fullPath).ToLower().Replace(".", "");
                        if (extension == "jpg") extension = "jpeg";
                        return $"data:image/{extension};base64,{Convert.ToBase64String(bytes)}";
                    }
                }

                // Handle HTTP/HTTPS URLs
                if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var bytes = await httpClient.GetByteArrayAsync(imageUrl);
                        var extension = Path.GetExtension(imageUrl).ToLower().Replace(".", "");
                        if (extension == "jpg") extension = "jpeg";
                        if (string.IsNullOrEmpty(extension)) extension = "png";
                        return $"data:image/{extension};base64,{Convert.ToBase64String(bytes)}";
                    }
                }

                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting image to Base64: {ex.Message}");
                return imageUrl;
            }
        }
    }
}

