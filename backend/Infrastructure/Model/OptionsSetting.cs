using System.Collections.Generic;

namespace Infrastructure.Model
{
    /// <summary>
    /// ??????POCO???
    /// </summary>
    public class OptionsSetting
    {
        /// <summary>
        /// ???????
        /// </summary>
        public bool SingleLogin { get; set; }
        /// <summary>
        /// ??????
        /// </summary>
        public bool DemoMode { get; set; }
        /// <summary>
        /// ???db
        /// </summary>
        public bool InitDb { get; set; }
        public string[] InitTables { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public List<MailOptions> MailOptions { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public Upload Upload { get; set; }
        /// <summary>
        /// ???oss
        /// </summary>
        public ALIYUN_OSS ALIYUN_OSS { get; set; }
        public JwtSettings JwtSettings { get; set; }
        /// <summary>
        /// ??????
        /// </summary>
        public CodeGen CodeGen { get; set; }
        /// <summary>
        /// ?????
        /// </summary>
        public List<DbConfigs> DbConfigs { get; set; }
        /// <summary>
        /// ?????????
        /// </summary>
        public DbConfigs CodeGenDbConfig { get; set; }
        /// <summary>
        /// Reids??
        /// </summary>
        public RedisServerConfig RedisServer { get; set; }
    }
    /// <summary>
    /// ????????
    /// </summary>
    public class MailOptions
    {
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public string Smtp { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string Signature { get; set; }
    }
    /// <summary>
    /// ??
    /// </summary>
    public class Upload
    {
        public string UploadUrl { get; set; }
        public string LocalSavePath { get; set; }
        public int MaxSize { get; set; }
        public string[] NotAllowedExt { get; set; } = new string[0];
    }
    /// <summary>
    /// ?????
    /// </summary>
    public class ALIYUN_OSS
    {
        public string REGIONID { get; set; }
        public string KEY { get; set; }
        public string SECRET { get; set; }
        public string BucketName { get; set; }
        public string DomainUrl { get; set; }
        public int MaxSize { get; set; } = 100;
    }

    /// <summary>
    /// Jwt
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// token?????
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// token??????????
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// ???key(SecretKey????16?,???,??????)
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// token??(?)
        /// </summary>
        public int Expire { get; set; } = 1440;
        /// <summary>
        /// ??token??
        /// </summary>
        public int RefreshTokenTime { get; set; }
        /// <summary>
        /// token??
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }

    public class CodeGen
    {
        /// <summary>
        /// ???????????
        /// </summary>
        public bool ShowApp { get; set; }
        /// <summary>
        /// ????????
        /// </summary>
        public bool AutoPre { get; set; }
        /// <summary>
        /// vue??????
        /// </summary>
        public string VuePath { get; set; }
        /// <summary>
        /// ??
        /// </summary>
        public string Author { get; set; }
        public string TablePrefix { get; set; }
        /// <summary>
        /// ???,???:business
        /// </summary>
        public string ModuleName { get; set; }
        public int FrontTpl { get; set; }
        /// <summary>
        /// unipap vue??????2/3
        /// </summary>
        public int UniappVersion { get; set; } = 2;
        /// <summary>
        /// unipap??????
        /// </summary>
        public string UniappPath { get; set; }
        public CsharpTypeArr CsharpTypeArr { get; set; }
    }

    public class DbConfigs
    {
        public string Conn { get; set; }
        public int DbType { get; set; }
        public string ConfigId { get; set; }
        public bool IsAutoCloseConnection { get; set; }
        public string DbName { get; set; }
    }

    public class CsharpTypeArr
    {
        public string[] String { get; set; }
        public string[] Int { get; set; }
        public string[] Long { get; set; }
        public string[] DateTime { get; set; }
        public string[] Float { get; set; }
        public string[] Decimal { get; set; }
        public string[] Bool { get; set; }
    }

    public class RedisServerConfig
    {
        public int Open { get; set; }
        public bool DbCache { get; set; }
    }
}

