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
