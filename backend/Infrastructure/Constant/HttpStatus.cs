namespace Infrastructure.Constant
{
    public class HttpStatus
    {
        /// <summary>
        /// Operation successful
        /// </summary>
        public static readonly int SUCCESS = 200;
        /// <summary>
        /// Object created successfully
        /// </summary>
        public static readonly int CREATED = 201;

        /// <summary>
        /// Request has been accepted
        /// </summary>
        public static readonly int ACCEPTED = 202;

        /// <summary>
        /// Operation executed successfully, but no data returned
        /// </summary>
        public static readonly int NO_CONTENT = 204;

        /// <summary>
        /// Resource has been removed
        /// </summary>
        public static readonly int MOVED_PERM = 301;

        /// <summary>
        /// Redirect
        /// </summary>
        public static readonly int SEE_OTHER = 303;

        /// <summary>
        /// Resource has not been modified
        /// </summary>
        public static readonly int NOT_MODIFIED = 304;

        /// <summary>
        /// Parameter list error (missing, format mismatch)
        /// </summary>
        public static readonly int BAD_REQUEST = 400;

        /// <summary>
        /// Unauthorized
        /// </summary>
        public static readonly int UNAUTHORIZED = 401;

        /// <summary>
        /// Access restricted, authorization expired
        /// </summary>
        public static readonly int FORBIDDEN = 403;

        /// <summary>
        /// Resource, service not found
        /// </summary>
        public static readonly int NOT_FOUND = 404;

        /// <summary>
        /// HTTP method not allowed
        /// </summary>
        public static readonly int BAD_METHOD = 405;

        /// <summary>
        /// Resource conflict, or resource is locked
        /// </summary>
        public static readonly int CONFLICT = 409;

        /// <summary>
        /// Unsupported data, media type
        /// </summary>
        public static readonly int UNSUPPORTED_TYPE = 415;

        /// <summary>
        /// System internal error
        /// </summary>
        public static readonly int ERROR = 500;

        /// <summary>
        /// Interface not implemented
        /// </summary>
        public static readonly int NOT_IMPLEMENTED = 501;
    }
}

