using System.Text;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public static class WorkPermitPdfTemplate
    {
        public static string GenerateHtml(OpsPermitDto permit)
        {
            var sb = new StringBuilder();
            sb.Append(HtmlHead());
            sb.Append("<body>");
            sb.Append(Page1(permit));
            sb.Append(Page2(permit));
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string HtmlHead() => @"<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8""/>
<style>
@page {
    size: A4;
    margin: 1mm;
}
* {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
}
body {
    font-family: Tahoma, Arial, sans-serif;
    font-size: 10px;
    color: #000;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
}
.page2 {
    page-break-before: always;
}
</style>
</head>";

        private static string Cb(bool v) => v ? "☑" : "☐";
        private static string E(string? v) => string.IsNullOrEmpty(v) ? "&nbsp;" : System.Net.WebUtility.HtmlEncode(v);

        private static string Page1(OpsPermitDto p)
        {
            var acc = string.Equals(p.Direction, "access", StringComparison.OrdinalIgnoreCase);
            var ext = string.Equals(p.Direction, "exit", StringComparison.OrdinalIgnoreCase);
            var nrm = !string.Equals(p.WorkCondition, "emergency", StringComparison.OrdinalIgnoreCase);
            var emr = string.Equals(p.WorkCondition, "emergency", StringComparison.OrdinalIgnoreCase);
            var wt = p.WorkTypeNames ?? new List<string>();
            var wc = p.WorkCategoryNames ?? new List<string>();
            var od = p.OpeningDate?.ToString("dd/MM/yyyy") ?? p.ValidFrom.ToString("dd/MM/yyyy");
            var ot = p.OpeningDate?.ToString("h:mm tt") ?? "";
            var cd = p.ClosingDate?.ToString("dd/MM/yyyy") ?? p.ValidUntil.ToString("dd/MM/yyyy");
            var ct = p.ClosingDate?.ToString("h:mm tt") ?? "";
            var dt = p.OpeningDate ?? p.ValidFrom;
            bool wtHas(string k) => wt.Any(x => x.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            bool wcHas(string k) => wc.Any(x => x.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

            var s = new StringBuilder();

            // ─────────────────────────────────────────
            // HEADER: Logo + Doc Reference + Title + Date
            // ─────────────────────────────────────────
            // Logo on its own line
            s.Append($@"
<div style=""margin-bottom:10px;"">
  {SelaLogo(170)}
</div>");

            // Title centered (Arabic on top, English below) + Date on the right
            s.Append($@"
<table style=""width:100%;border-collapse:collapse;border:none;margin-bottom:10px;"">
<tr>
  <td style=""border:none;text-align:center;vertical-align:bottom;"">
    <div style=""font-size:24px;font-weight:bold;"">تصريح عمل</div>
  </td>
  <td rowspan=""2"" style=""width:300px;border:none;text-align:right;vertical-align:bottom;font-size:11px;direction:rtl;padding-bottom:2px;"">
    خُير يوم:
    <span style=""border-bottom:1px solid #000;padding:0 8px;font-weight:bold;"">{E(dt.ToString("dddd", new System.Globalization.CultureInfo("ar-SA")))}</span>
    &nbsp;&nbsp;الموافق:
    <span style=""border:1px solid #000;padding:1px 5px;font-weight:bold;"">{dt.Day:00}</span> /
    <span style=""border:1px solid #000;padding:1px 5px;font-weight:bold;"">{dt.Month:00}</span> /
    <span style=""border:1px solid #000;padding:1px 5px;font-weight:bold;"">{dt.Year}</span>م
  </td>
</tr>
<tr>
  <td style=""border:none;text-align:center;vertical-align:top;"">
    <div style=""font-size:16px;font-weight:bold;"">Work Permit</div>
  </td>
</tr>
</table>");

            // ─────────────────────────────────────────
            // INFORMATION SECTION
            // ─────────────────────────────────────────
            s.Append(@"
<table style=""width:100%;border-collapse:collapse;"">
<tr>
  <td colspan=""6"" style=""border:1px solid #000;background:#c5d9f1;text-align:center;font-weight:bold;font-size:12px;padding:4px;"">
    المعلومات / Information
  </td>
</tr>");

            // Row 1: Labels (Work Area, Gate Number, Project Name)
            s.Append(@"
<tr>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;width:16.66%;"">Work Area:</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;text-align:right;width:16.66%;"">منطقة العمل</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;width:16.66%;"">Gate Number:</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;text-align:right;width:16.66%;"">رقم البوابة</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;width:16.66%;"">Project Name:</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;text-align:right;width:16.66%;"">اسم المشروع</td>
</tr>");

            // Row 1: Values
            s.Append($@"
<tr>
  <td colspan=""2"" style=""border:1px solid #000;text-align:center;font-size:15px;font-weight:bold;padding:7px 4px;"">{E(p.ZoneName)}</td>
  <td colspan=""2"" style=""border:1px solid #000;text-align:center;font-size:15px;font-weight:bold;padding:7px 4px;"">{E(p.GateNumber)}</td>
  <td colspan=""2"" style=""border:1px solid #000;text-align:center;font-size:15px;font-weight:bold;padding:7px 4px;"">{E(p.SeasonName)}</td>
</tr>");

            // Row 2: Labels (Plate Number, Company, Unit Name)
            s.Append(@"
<tr>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;"">Plate Number:</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;text-align:right;"">لوحة السيارة</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;"">Company:</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;text-align:right;"">الشركة</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;"">Unit Name:</td>
  <td style=""border:1px solid #000;padding:3px 4px;font-size:9px;text-align:right;"">اسم الوحدة</td>
</tr>");

            // Row 2: Values
            s.Append($@"
<tr>
  <td colspan=""2"" style=""border:1px solid #000;text-align:center;font-size:15px;font-weight:bold;padding:7px 4px;"">{E(p.VehiclePlateNumber)}</td>
  <td colspan=""2"" style=""border:1px solid #000;text-align:center;font-size:15px;font-weight:bold;padding:7px 4px;"">{E(p.CompanyName ?? p.ApplicantName)}</td>
  <td colspan=""2"" style=""border:1px solid #000;text-align:center;font-size:15px;font-weight:bold;padding:7px 4px;"">{E(p.ShopName)}</td>
</tr>
</table>");

            // ─────────────────────────────────────────
            // PERMIT TYPE + OPENING/CLOSING
            // ─────────────────────────────────────────
            s.Append($@"
<table style=""width:100%;border-collapse:collapse;margin-top:-1px;"">
<tr>
  <td rowspan=""2"" style=""border:1px solid #000;width:13%;vertical-align:top;padding:5px;font-size:10px;"">
    <div><span>Permit Type:</span><span style=""float:right;"">نوع التصريح:</span></div>
    <div style=""margin-top:10px;font-size:12px;"">
      <div><span>{Cb(acc)} Access</span><span style=""float:right;"">دخول</span></div>
      <div style=""margin-top:6px;""><span>{Cb(ext)} Exit</span><span style=""float:right;"">خروج</span></div>
    </div>
  </td>
  <td style=""border:1px solid #000;width:8%;font-size:9px;padding:3px 4px;"">Time:</td>
  <td style=""border:1px solid #000;width:11%;text-align:center;font-size:14px;font-weight:bold;padding:3px;"">{E(ot)}</td>
  <td style=""border:1px solid #000;width:6%;font-size:9px;padding:3px 4px;text-align:right;"">:الساعة</td>
  <td style=""border:1px solid #000;width:5%;font-size:9px;padding:3px 4px;"">Date:</td>
  <td style=""border:1px solid #000;width:11%;text-align:center;font-size:14px;font-weight:bold;padding:3px;"">{E(od)}</td>
  <td style=""border:1px solid #000;width:6%;font-size:9px;padding:3px 4px;text-align:right;"">:التاريخ</td>
  <td style=""border:1px solid #000;width:27%;font-size:9px;padding:3px 6px;text-align:right;"">work permit opening بداية التصريح:</td>
</tr>
<tr>
  <td style=""border:1px solid #000;font-size:9px;padding:3px 4px;"">Time:</td>
  <td style=""border:1px solid #000;text-align:center;font-size:14px;font-weight:bold;padding:3px;"">{E(ct)}</td>
  <td style=""border:1px solid #000;font-size:9px;padding:3px 4px;text-align:right;"">:الساعة</td>
  <td style=""border:1px solid #000;font-size:9px;padding:3px 4px;"">Date:</td>
  <td style=""border:1px solid #000;text-align:center;font-size:14px;font-weight:bold;padding:3px;"">{E(cd)}</td>
  <td style=""border:1px solid #000;font-size:9px;padding:3px 4px;text-align:right;"">:التاريخ</td>
  <td style=""border:1px solid #000;font-size:9px;padding:3px 6px;text-align:right;"">work permit closing نهاية التصريح:</td>
</tr>
</table>");

            // ─────────────────────────────────────────
            // WORK CONDITION | CATEGORY | TYPE OF WORK
            // ─────────────────────────────────────────
            s.Append(@"<table style=""width:100%;border-collapse:collapse;margin-top:-1px;""><tr>");

            // Work Condition
            s.Append($@"
<td style=""border:1px solid #000;width:22%;vertical-align:top;padding:5px;font-size:10px;"">
  <div style=""font-weight:bold;text-align:center;margin-bottom:4px;"">Work Condition / حالة العمل</div>
  <div style=""margin-top:4px;"">{Cb(nrm)} Normal / عادي</div>
  <div style=""font-size:8px;color:#666;padding-left:18px;"">From 6-24 hours / من 6 - 24 ساعة</div>
  <div style=""margin-top:4px;"">{Cb(emr)} Emergency / طارئ</div>
  <div style=""font-size:8px;color:#666;padding-left:18px;"">During 2 hours / خلال ساعتين</div>
</td>");

            // Category of Work
            s.Append($@"
<td style=""border:1px solid #000;width:36%;vertical-align:top;padding:5px;font-size:10px;"">
  <div style=""font-weight:bold;text-align:center;margin-bottom:4px;"">Category of Work / فئة العمل</div>
  <table style=""width:100%;border-collapse:collapse;border:none;"">
  <tr>
    <td style=""border:none;padding:2px;width:50%;"">{Cb(wcHas("Hot"))} Hot Work / أعمال ساخنة</td>
    <td style=""border:none;padding:2px;width:50%;"">{Cb(wcHas("Cold"))} Cold Work / أعمال باردة</td>
  </tr>
  <tr>
    <td style=""border:none;padding:2px;"">{Cb(wcHas("Height"))} Work at Height / أعمال مرتفعه</td>
    <td style=""border:none;padding:2px;"">{Cb(wcHas("Heavy"))} Heavy Maint. / أعمال ثقيلة</td>
  </tr>
  <tr>
    <td style=""border:none;padding:2px;"">{Cb(wcHas("Light"))} Light Maint. / أعمال خفيفة</td>
    <td style=""border:none;padding:2px;"">&nbsp;</td>
  </tr>");
            foreach (var x in wc)
            {
                bool shown = new[] { "Cold", "Hot", "Height", "Heavy", "Light" }.Any(k => x.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!shown) s.Append($@"<tr><td colspan=""2"" style=""border:none;padding:2px;"">☑ {E(x)}</td></tr>");
            }
            s.Append("</table></td>");

            // Type of Work
            s.Append($@"
<td style=""border:1px solid #000;width:42%;vertical-align:top;padding:5px;font-size:10px;"">
  <div style=""font-weight:bold;text-align:center;margin-bottom:4px;"">Type of Work / نوع العمل</div>
  <table style=""width:100%;border-collapse:collapse;border:none;"">
  <tr>
    <td style=""border:none;padding:2px;width:50%;"">{Cb(wtHas("Civil"))} Civil / أعمال مدنية</td>
    <td style=""border:none;padding:2px;width:50%;"">{Cb(wtHas("Mechanical"))} Mechanical / ميكانيكية</td>
  </tr>
  <tr>
    <td style=""border:none;padding:2px;"">{Cb(wtHas("Electrical"))} Electrical / كهربائية</td>
    <td style=""border:none;padding:2px;"">{Cb(wtHas("Plumbing") || wtHas("سباكة"))} Plumbing / سباكة</td>
  </tr>");
            foreach (var x in wt)
            {
                bool shown = new[] { "Civil", "Mechanical", "Electrical", "Plumbing" }.Any(k => x.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!shown) s.Append($@"<tr><td colspan=""2"" style=""border:none;padding:2px;"">☑ {E(x)}</td></tr>");
            }
            s.Append("</table></td>");
            s.Append("</tr></table>");

            // ─────────────────────────────────────────
            // WORK DESCRIPTION
            // ─────────────────────────────────────────
            s.Append($@"
<table style=""width:100%;border-collapse:collapse;margin-top:-1px;"">
<tr>
  <td style=""border:1px solid #000;padding:4px 6px;font-size:10px;font-weight:bold;width:50%;"">Work Description</td>
  <td style=""border:1px solid #000;padding:4px 6px;font-size:10px;font-weight:bold;text-align:right;width:50%;"">وصف العمل</td>
</tr>
<tr>
  <td colspan=""2"" style=""border:1px solid #000;min-height:50px;height:50px;padding:8px;text-align:center;font-size:15px;font-weight:bold;direction:rtl;"">{E(p.WorkDescription)}</td>
</tr>
</table>");

            // ─────────────────────────────────────────
            // WORKERS TABLE
            // ─────────────────────────────────────────
            var ws = p.Workers ?? new List<OpsPermitWorkerDto>();
            s.Append(@"
<table style=""width:100%;border-collapse:collapse;margin-top:-1px;"">
<tr>
  <td colspan=""4"" style=""border:1px solid #000;background:#c5d9f1;text-align:center;font-weight:bold;font-size:11px;padding:4px;"">
    قائمة العاملين المصرح لهم / List of Authorized Workers
  </td>
</tr>
<tr style=""background:#e8e8e8;font-weight:bold;text-align:center;font-size:10px;"">
  <td style=""border:1px solid #000;padding:4px;width:18%;"">رقم التواصل<br/>Contact Number</td>
  <td style=""border:1px solid #000;padding:4px;width:24%;"">رقم الهوية / الإقامة<br/>ID / Iqama Number</td>
  <td style=""border:1px solid #000;padding:4px;width:44%;"">الاسم<br/>Name</td>
  <td style=""border:1px solid #000;padding:4px;width:14%;"">م<br/>NO#</td>
</tr>");
            for (int i = 0; i < Math.Max(ws.Count, 10); i++)
            {
                var w = i < ws.Count ? ws[i] : null;
                s.Append($@"
<tr style=""height:24px;font-size:11px;"">
  <td style=""border:1px solid #000;padding:3px 4px;text-align:center;"">{E(w?.Phone)}</td>
  <td style=""border:1px solid #000;padding:3px 4px;text-align:center;"">{E(w?.NationalId)}</td>
  <td style=""border:1px solid #000;padding:3px 4px;"">{E(w?.WorkerName ?? w?.WorkerNameAr)}</td>
  <td style=""border:1px solid #000;padding:3px 4px;text-align:center;"">{i + 1}</td>
</tr>");
            }
            s.Append("</table>");

            return s.ToString();
        }

        private static string Page2(OpsPermitDto p)
        {
            var sc = p.SafetyChecks ?? new List<OpsPermitSafetyCheckDto>();
            var s = new StringBuilder();

            s.Append(@"<div class=""page2""></div>");

            // Page 2 Header — Logo with margin below
            s.Append($@"
<div style=""margin-bottom:14px;"">
  {SelaLogo(170)}
</div>");

            // Safety title
            s.Append(@"
<table style=""width:100%;border-collapse:collapse;"">
<tr>
  <td colspan=""6"" style=""border:1px solid #000;background:#c5d9f1;text-align:center;font-weight:bold;font-size:13px;padding:6px;"">
    احتياطات السلامة / Safety Precautions
  </td>
</tr>
<tr style=""background:#e8e8e8;font-weight:bold;text-align:center;font-size:11px;"">
  <td style=""border:1px solid #000;padding:5px;width:5%;"">#</td>
  <td style=""border:1px solid #000;padding:5px;width:33%;"">Items</td>
  <td style=""border:1px solid #000;padding:5px;width:7%;"">Yes</td>
  <td style=""border:1px solid #000;padding:5px;width:7%;"">No</td>
  <td style=""border:1px solid #000;padding:5px;width:33%;"">العناصر</td>
  <td style=""border:1px solid #000;padding:5px;width:5%;"">#</td>
</tr>");
            for (int i = 0; i < Math.Max(sc.Count, 12); i++)
            {
                var c = i < sc.Count ? sc[i] : null;
                string yes = c != null && c.IsChecked ? "✓" : "";
                string no = c != null && !c.IsChecked ? "✓" : "";
                s.Append($@"
<tr style=""font-size:11px;height:28px;"">
  <td style=""border:1px solid #000;text-align:center;padding:3px;"">{i + 1}</td>
  <td style=""border:1px solid #000;padding:3px 6px;"">{E(c?.PrecautionDescription)}</td>
  <td style=""border:1px solid #000;text-align:center;padding:3px;font-size:14px;"">{yes}</td>
  <td style=""border:1px solid #000;text-align:center;padding:3px;font-size:14px;"">{no}</td>
  <td style=""border:1px solid #000;padding:3px 6px;text-align:right;direction:rtl;"">{E(c?.PrecautionDescriptionAr)}</td>
  <td style=""border:1px solid #000;text-align:center;padding:3px;"">{i + 1}</td>
</tr>");
            }
            s.Append("</table>");

            // Acknowledgment - no "English:" / "العربية:" labels
            s.Append(@"
<table style=""width:100%;border-collapse:collapse;margin-top:6px;"">
<tr>
  <td style=""border:1px solid #000;background:#c5d9f1;text-align:center;font-weight:bold;font-size:13px;padding:5px;"">
    الإقرار / Acknowledgment
  </td>
</tr>
<tr>
  <td style=""border:1px solid #000;font-size:10px;line-height:1.6;padding:8px;"">
    <p>I, the undersigned, acknowledge that I have read and understood the safety instructions and regulations mentioned in this work permit. I commit to complying with all safety requirements and procedures during the execution of work. I bear full responsibility for any violations or non-compliance with the stated safety conditions.</p>
    <p dir=""rtl"" style=""text-align:right;margin-top:6px;"">أقر أنا الموقع أدناه بأنني قد قرأت وفهمت تعليمات وأنظمة السلامة المذكورة في تصريح العمل هذا. وأتعهد بالالتزام بجميع متطلبات وإجراءات السلامة أثناء تنفيذ العمل. وأتحمل المسؤولية الكاملة عن أي مخالفات أو عدم الامتثال لشروط السلامة المذكورة.</p>
  </td>
</tr>
</table>");

            // Approval / Signatures
            s.Append(@"
<table style=""width:100%;border-collapse:collapse;margin-top:6px;"">
<tr>
  <td colspan=""4"" style=""border:1px solid #000;background:#c5d9f1;text-align:center;font-weight:bold;font-size:12px;padding:4px;"">
    الموافقة / Approval
  </td>
</tr>
<tr style=""font-weight:bold;text-align:center;font-size:10px;"">
  <td style=""border:1px solid #000;padding:4px;width:25%;"">توقيع مقدم الطلب<br/>Requester Signature</td>
  <td style=""border:1px solid #000;padding:4px;width:25%;"">مدير المشروع (صلة)<br/>Project Manager (Sela)</td>
  <td style=""border:1px solid #000;padding:4px;width:25%;"">جهة ذات علاقة<br/>Involved Party</td>
  <td style=""border:1px solid #000;padding:4px;width:25%;"">المستأجر / المقاول<br/>Tenant / Contractor</td>
</tr>
<tr>
  <td style=""border:1px solid #000;height:45px;vertical-align:bottom;padding:4px;"">&nbsp;</td>
  <td style=""border:1px solid #000;height:45px;vertical-align:bottom;padding:4px;"">&nbsp;</td>
  <td style=""border:1px solid #000;height:45px;vertical-align:bottom;padding:4px;"">&nbsp;</td>
  <td style=""border:1px solid #000;height:45px;vertical-align:bottom;padding:4px;"">&nbsp;</td>
</tr>
</table>");

            // Work Permit Instructions — matching target with detailed text
            s.Append(@"
<table style=""width:100%;border-collapse:collapse;margin-top:6px;"">
<tr>
  <td colspan=""2"" style=""border:1px solid #000;background:#c5d9f1;text-align:center;font-weight:bold;font-size:12px;padding:4px;"">
    تعليمات تصريح العمل / Work Permit Instructions
  </td>
</tr>
<tr>
  <td style=""border:1px solid #000;font-size:8px;line-height:1.5;padding:6px;width:50%;vertical-align:top;"">
    <div style=""font-weight:bold;margin-bottom:3px;"">Work Permit Instruction:</div>
    <div>- Fill all the requirements to avoid reject your Permit.</div>
    <div>- All IDs must be valid.</div>
    <div>- This permit is valid as mentioned above.</div>
    <div>- The responsible person/company must take all necessary precautions and safety measures to protect the staff and the promises permanent works, and they will assume full responsibility for any damages that might be caused while doing their work activities.</div>
    <div>- The responsible person/company will be responsible for guaranteeing that their staff and subcontractors comply with legal rules, customs, training requirements and risk prevention when these carry out their work.</div>
    <div>- The responsibilities of the mentioned locations above will be transferred to the responsible person/company.</div>
  </td>
  <td style=""border:1px solid #000;font-size:8px;line-height:1.5;padding:6px;width:50%;vertical-align:top;direction:rtl;text-align:right;"">
    <div style=""font-weight:bold;margin-bottom:3px;"">تعليمات تصريح عمل:</div>
    <div>- تعبئة جميع البيانات وعدم ترك أي حقول فارغة لتجنب رفض الطلب.</div>
    <div>- جميع الإثباتات المستندية يجب أن تكون سارية المفعول.</div>
    <div>- يسري التصريح للمدة المحددة فقط.</div>
    <div>- يجب اتخاذ جميع الاحتياطات الإلزامية وتقديم السلامة الوقائية لحماية العاملين وموقع العمل. ويتحمل الشخص المسؤول / الشركة المسؤولية الكاملة عن أي أضرار قد تحدث خلال العمل.</div>
    <div>- الشخص المسؤول / الشركة يتحملون مسؤولية ضمان امتثال كافة العاملين والمقاولين للقوانين المعمول بها والتدريبات المطلوبة والوقاية من المخاطر خلال أداء أعمالهم.</div>
    <div>- المسؤولية الموقع المذكور مسبقا سوف تنقل للشخص المسؤول / الشركة.</div>
  </td>
</tr>
</table>");

            return s.ToString();
        }

        private static string SelaLogo(int widthPx) => $@"
<div>
<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 488.21 137.94"" style=""width:{widthPx}px;display:block;""><path d=""M140.54,315.45c19.45-.12,38.92.06,58.36-.12,2.34,8.14,4.3,16.37,6.18,24.63a272,272,0,0,0-64.57,27.85C140.63,350.36,140.6,332.91,140.54,315.45Z"" transform=""translate(-139.9 -315.03)""/><path d=""M217.35,336.72c-1.41-7.17-3.16-14.26-4.83-21.35,21.14.15,42.32-.09,63.46.15,0,5,.06,10,.12,15C256.36,330.57,236.71,333,217.35,336.72Z"" transform=""translate(-139.9 -315.03)""/><path d=""M486.89,322.83c-1.14-12.5,14.85-5.89,22.26-7.23,1.87,32.54.29,65.22.79,97.84-.53,12,1.14,24.31-.73,36.25-7.09.26-15.52,2.31-21.82-1.11C486.19,406.74,487.16,364.74,486.89,322.83Z"" transform=""translate(-139.9 -315.03)""/><path d=""M219.43,349.66A243.53,243.53,0,0,1,276,343.8c-.2,26.36-.12,52.71-.06,79.07a258.75,258.75,0,0,1-55.76-5.71C222.92,394.76,223,372,219.43,349.66Z"" transform=""translate(-139.9 -315.03)""/><path d=""M311.24,351.24c17.63-9.69,41.44-5.27,55,9.52a59.61,59.61,0,0,1-15.7,14.47c-8.43-8.9-26.06-12.62-32.62.41C325,389.87,343.72,388,355.4,396.54c15.4,8.2,17.51,30.6,6.44,43.22-5,6.71-13.18,9.25-20.47,12.5-16,1.41-33.56-2.84-44.86-14.88-8.11-7,5.42-13.3,9.46-18.51,8.05,7.44,17.89,14.2,29.58,11.8,8.46.88,16.11-11.07,7.82-16.6-11.71-7.5-27.06-8.14-37.48-18C292.06,384.45,295.49,359.56,311.24,351.24Z"" transform=""translate(-139.9 -315.03)""/><path d=""M394,358.06c18.71-17.45,52-15.61,68.32,4.3,10,12.24,13.88,29.14,11.27,44.63-24.72.26-49.43,0-74.12.12,3,8.81,8.32,17.48,17.19,21.32,12.15,5.33,26.42,1.73,36.43-6.41,4.66,4.25,8.35,9.37,12,14.5-10.78,13.06-29.31,17-45.54,15.76-13.79-4.07-27.47-11.27-34.88-24.13C370.89,406.53,374.76,375.2,394,358.06Zm5.18,30.43c17.69.32,35.35.44,53-.06-3.81-11.27-12.94-22.64-26.06-21.67C412.92,365.82,403.52,377.25,399.21,388.49Z"" transform=""translate(-139.9 -315.03)""/><path d=""M531.52,369.54c11-17.13,33.38-26.8,53.15-20.73,8.14,2.23,13.44,9,18.16,15.52,1.7-5.21,3.37-10.45,5.18-15.64,6.71.26,13.41.59,20.09,1.46v99.33c-7,.53-14,.76-20.94.91q-2.37-6.24-4.66-12.48c-9.2,9.78-22.61,16.84-36.31,14.64-12.15-2.4-23.81-8.67-31.63-18.39C519.9,416.28,518.73,388.73,531.52,369.54Zm30.66,2c-14.85,6.79-20.06,26.06-14.53,40.65,4.48,13.27,19.88,21.5,33.38,17.28,16.4-3.92,25.71-23.52,20.21-39.07C597.44,374.08,577.44,363.69,562.18,371.59Z"" transform=""translate(-139.9 -315.03)""/><path d=""M139.9,383.45a255.72,255.72,0,0,1,67.33-30.78c2.17,20.41,2.66,41,.32,61.44C183.65,407.12,161.13,396.34,139.9,383.45Z"" transform=""translate(-139.9 -315.03)""/><path d=""M140.54,398.86a298.82,298.82,0,0,0,65.39,28.23c-1.79,7.7-3.6,15.4-5.51,23.11-20,0-40,.15-60.06-.09C140.8,433,140.51,415.93,140.54,398.86Z"" transform=""translate(-139.9 -315.03)""/><path d=""M218.2,430c19,4.13,38.42,5.48,57.81,6.3,0,4.63.06,9.25.12,13.88-20.91.18-41.82.06-62.73.06C215.25,443.52,216.89,436.75,218.2,430Z"" transform=""translate(-139.9 -315.03)""/></svg>
<div style=""font-size:9px;color:#666;margin-top:1px;"">PM/All/002/4</div>
</div>";
    }
}
