using SqlSugar;

namespace VibeVoice.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string Section = "Database";

    /// <summary>Sqlite | PostgreSQL | SqlServer | MySql | Oracle</summary>
    public string Provider { get; set; } = "Sqlite";

    public string ConnectionString { get; set; } = "Data Source=storage/vibevoice.db";

    /// <summary>Insert demo user + sample notes/memories when the database is empty.</summary>
    public bool Seed { get; set; }

    /// <summary>Log generated SQL (Debug level).</summary>
    public bool LogSql { get; set; }
}

/// <summary>The single place where the configured provider name becomes a SqlSugar <see cref="DbType"/>.</summary>
public static class DbProviderFactory
{
    public static DbType ToDbType(string provider) => provider.Trim().ToLowerInvariant() switch
    {
        "sqlite" => DbType.Sqlite,
        "postgresql" or "postgres" or "npgsql" => DbType.PostgreSQL,
        "sqlserver" or "mssql" => DbType.SqlServer,
        "mysql" or "mariadb" => DbType.MySql,
        "oracle" => DbType.Oracle,
        _ => throw new InvalidOperationException(
            $"Unsupported Database:Provider '{provider}'. Use Sqlite, PostgreSQL, SqlServer, MySql or Oracle."),
    };

    /// <summary>
    /// Hosts like Neon, Render and Heroku hand out <c>postgres://user:pass@host:port/db?sslmode=require</c>;
    /// Npgsql wants key=value pairs. Anything that is not a URL is returned unchanged.
    /// </summary>
    public static string FromPostgresUrl(string value)
    {
        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return value;

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var sslMode = query["sslmode"] switch
        {
            null or "" => "Prefer",
            var m => char.ToUpperInvariant(m[0]) + m[1..].Replace("-full", "Full").Replace("-ca", "CA"),
        };

        var parts = new List<string>
        {
            $"Host={uri.Host}",
            $"Port={(uri.Port > 0 ? uri.Port : 5432)}",
            $"Database={Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'))}",
            $"Username={Uri.UnescapeDataString(userInfo[0])}",
        };
        if (userInfo.Length > 1) parts.Add($"Password={Uri.UnescapeDataString(userInfo[1])}");
        parts.Add($"SSL Mode={sslMode}");
        // Serverless Postgres (Neon) closes idle connections; keep the pool small and recycle quickly.
        parts.Add("Maximum Pool Size=10");
        parts.Add("Connection Idle Lifetime=60");
        return string.Join(';', parts);
    }

    public static ConnectionConfig CreateConfig(DatabaseOptions options, string contentRoot)
    {
        var dbType = ToDbType(options.Provider);
        var connectionString = options.ConnectionString;

        if (dbType == DbType.Sqlite)
        {
            // Resolve "Data Source=storage/x.db" against the app's content root and make sure the folder exists.
            var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
            if (builder.TryGetValue("Data Source", out var ds) && ds is string file && file != ":memory:")
            {
                var full = Path.IsPathRooted(file) ? file : Path.GetFullPath(Path.Combine(contentRoot, file));
                Directory.CreateDirectory(Path.GetDirectoryName(full)!);
                builder["Data Source"] = full;
                connectionString = builder.ConnectionString;
            }
        }
        else if (dbType == DbType.PostgreSQL)
        {
            connectionString = FromPostgresUrl(connectionString);
        }

        return new ConnectionConfig
        {
            DbType = dbType,
            ConnectionString = connectionString,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            MoreSettings = new ConnMoreSettings
            {
                // Keep identifiers lower-case on PostgreSQL so they never need quoting.
                PgSqlIsAutoToLower = true,
                PgSqlIsAutoToLowerCodeFirst = true,
            },
        };
    }
}
