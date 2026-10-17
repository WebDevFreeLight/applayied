using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ZR.ServiceCore.Localization
{
    /// <summary>
    /// Translates PMO business/validation messages (thrown in English by the services) into the
    /// request language at the single exception → response boundary. Keyed by the stable English
    /// text, so the services stay untouched. Unknown strings pass through unchanged (graceful
    /// English fallback). Interpolated messages are matched by regex that preserves the embedded value.
    /// </summary>
    public static class PmoMessageLocalizer
    {
        // Exact-match table for the fixed (non-interpolated) messages — EN → AR.
        private static readonly Dictionary<string, string> Ar = new()
        {
            // access / RBAC (surfaced by the authorization-denial acceptance test)
            ["You do not have access to this project"] = "ليس لديك صلاحية الوصول إلى هذا المشروع",
            ["You do not have access to this task"] = "ليس لديك صلاحية الوصول إلى هذه المهمة",
            ["You do not have access to this risk"] = "ليس لديك صلاحية الوصول إلى هذا الخطر",
            ["You can only update your own assigned tasks"] = "يمكنك فقط تحديث المهام المسندة إليك",
            ["You cannot review or decide your own change request"] = "لا يمكنك مراجعة أو البتّ في طلب التغيير الخاص بك",
            ["Only the project owner or an administrator may edit this project"] = "يمكن فقط لمالك المشروع أو المسؤول تعديل هذا المشروع",
            ["Only the requester may edit this change request"] = "يمكن فقط لمقدّم الطلب تعديل طلب التغيير هذا",
            ["Only the requester may submit this change request"] = "يمكن فقط لمقدّم الطلب إرسال طلب التغيير هذا",

            // finance (surfaced by finance-posting tests)
            ["Amount must be greater than zero"] = "يجب أن يكون المبلغ أكبر من صفر",
            ["A posted transaction cannot be edited (void/reverse instead)"] = "لا يمكن تعديل معاملة مُرحّلة (استخدم الإلغاء/العكس بدلاً من ذلك)",
            ["Transaction is already posted"] = "تم ترحيل المعاملة بالفعل",
            ["Transaction not found"] = "المعاملة غير موجودة",
            ["Initial approved budget cannot be negative"] = "لا يمكن أن تكون الميزانية المعتمدة الأولية سالبة",
            ["Cost category not found"] = "فئة التكلفة غير موجودة",
            ["Category not found"] = "الفئة غير موجودة",
            ["Revision not found"] = "المراجعة غير موجودة",

            // workflow / change requests
            ["A decision reason is required to reject or return"] = "يلزم إدخال سبب القرار للرفض أو الإرجاع",
            ["A rationale is required"] = "يلزم إدخال مبرّر",
            ["Change request not found"] = "طلب التغيير غير موجود",

            // delivery
            ["Due date cannot be before the start date"] = "لا يمكن أن يكون تاريخ الاستحقاق قبل تاريخ البدء",
            ["Planned end date cannot be before the start date"] = "لا يمكن أن يكون تاريخ الانتهاء المخطّط قبل تاريخ البدء",
            ["Progress must be between 0 and 100"] = "يجب أن تكون نسبة الإنجاز بين 0 و100",
            ["Task title is required"] = "عنوان المهمة مطلوب",
            ["Task not found"] = "المهمة غير موجودة",
            ["Milestone name is required"] = "اسم المعلم الرئيسي مطلوب",
            ["Milestone not found"] = "المعلم الرئيسي غير موجود",
            ["Status must be Pending, Achieved or Missed"] = "يجب أن تكون الحالة: قيد الانتظار أو مُنجز أو فائت",
            ["This task was changed by someone else — reload and retry."] = "تم تعديل هذه المهمة بواسطة شخص آخر — أعد التحميل وحاول مرة أخرى.",

            // risks
            ["Risk not found"] = "الخطر غير موجود",
            ["Probability must be between 1 and 5"] = "يجب أن يكون الاحتمال بين 1 و5",
            ["Impact must be between 1 and 5"] = "يجب أن يكون الأثر بين 1 و5",
            ["Kind must be 'Risk' or 'Issue'"] = "يجب أن يكون النوع 'خطر' أو 'مشكلة'",
            ["Status must be 'Open', 'Mitigating' or 'Closed'"] = "يجب أن تكون الحالة 'مفتوح' أو 'قيد المعالجة' أو 'مغلق'",

            // projects / generic
            ["Project not found"] = "المشروع غير موجود",
            ["Title is required"] = "العنوان مطلوب",

            // RBAC route filter (authorization-denial responses)
            ["Unauthorized - please sign in"] = "غير مُصرّح — يرجى تسجيل الدخول",
        };

        // Regex rules for the interpolated messages — $1/$2 keep the concrete value from the thrown text.
        private static readonly (Regex Pattern, string Replacement)[] ArRules =
        {
            (new Regex(@"^A change request in '(.+)' cannot be edited$"), "لا يمكن تعديل طلب تغيير في الحالة '$1'"),
            (new Regex(@"^Category code '(.+)' already exists$"), "رمز الفئة '$1' موجود مسبقاً"),
            (new Regex(@"^Project code '(.+)' already exists$"), "رمز المشروع '$1' موجود مسبقاً"),
            (new Regex(@"^Revision already (.+)$"), "حالة المراجعة بالفعل '$1'"),
            (new Regex(@"^Transition (.+) → (.+) is not permitted$"), "الانتقال من $1 إلى $2 غير مسموح به"),
            (new Regex(@"^Unknown action '(.+)'$"), "إجراء غير معروف '$1'"),
            (new Regex(@"^Forbidden - this action requires the '(.+)' role or higher$"),
                "ممنوع — يتطلب هذا الإجراء دور '$1' أو أعلى"),
        };

        /// <summary>Derive the primary language code (e.g. "ar") from a raw Accept-Language header.</summary>
        public static string LanguageFromHeader(string acceptLanguage)
        {
            if (string.IsNullOrEmpty(acceptLanguage)) return "en";
            return acceptLanguage.Split(',')[0].Split('-')[0].Trim().ToLowerInvariant();
        }

        /// <summary>Return <paramref name="message"/> localized to <paramref name="language"/> (currently Arabic), else unchanged.</summary>
        public static string Translate(string message, string language)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(language)) return message;
            if (!language.StartsWith("ar", System.StringComparison.OrdinalIgnoreCase)) return message;

            if (Ar.TryGetValue(message, out var exact)) return exact;
            foreach (var (pattern, replacement) in ArRules)
            {
                if (pattern.IsMatch(message)) return pattern.Replace(message, replacement);
            }
            return message; // unknown → English fallback
        }
    }
}
