using System;

namespace Infrastructure.Attribute
{
    /// <summary>
    /// Lookup Permission Attribute
    ///
    /// Used to mark endpoints that provide lookup/reference data for dropdowns.
    /// Lookup endpoints return minimal data and can be accessed by all authenticated users (if IsPublicLookup=true)
    /// or only by users with specific lookup permission (if IsPublicLookup=false).
    ///
    /// Example:
    /// [HttpGet("lookup")]
    /// [LookupPermission("hr:employee:lookup", isPublic: true)]
    /// public async Task&lt;IActionResult&gt; GetLookup() { ... }
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LookupPermissionAttribute : System.Attribute
    {
        /// <summary>
        /// The lookup permission required (e.g., "hr:employee:lookup")
        /// </summary>
        public string LookupPermission { get; set; }

        /// <summary>
        /// If true, any authenticated user can access this lookup.
        /// If false, user must have the specific lookup permission.
        /// </summary>
        public bool IsPublicLookup { get; set; }

        /// <summary>
        /// Creates a new lookup permission attribute
        /// </summary>
        /// <param name="lookupPermission">The lookup permission string (e.g., "hr:employee:lookup")</param>
        /// <param name="isPublic">Whether this lookup is public to all authenticated users</param>
        public LookupPermissionAttribute(string lookupPermission, bool isPublic = false)
        {
            LookupPermission = lookupPermission;
            IsPublicLookup = isPublic;
        }
    }
}
