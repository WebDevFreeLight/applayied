using Infrastructure;
using Infrastructure.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ZR.CodeGenerator
{
    /// <summary>
    /// ?????????
    /// </summary>
    public class DbProvider
    {
        protected static SqlSugarClient CodeDb;

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="dbName">????</param>
        /// <returns></returns>
        public SqlSugarClient GetSugarDbContext(string dbName = "")
        {
            DbConfigs configs = AppSettings.Get<DbConfigs>("CodeGenDbConfig");

            string connStr = configs.Conn;

            if (!string.IsNullOrEmpty(dbName))
            {
                configs.DbName = dbName;
            }
            connStr = connStr.Replace("{dbName}", configs.DbName, StringComparison.OrdinalIgnoreCase);






            var db = new SqlSugarClient(new List<ConnectionConfig>()
            {
                new ConnectionConfig(){
                    ConnectionString = connStr,
                    DbType = (DbType)configs.DbType,
                      IsAutoCloseConnection = configs.IsAutoCloseConnection,
                      ConfigId=configs.ConfigId,
                      //MoreSettings = new ConnMoreSettings()
                      //{
                      //    //IsWithNoLockQuery = true,
                      //    DatabaseModel= (DbType)configs.DbType,

                      //    //DisableMillisecond = false,

                      //    //ClickHouseEnableFinal = true,
                      //    //IsAutoRemoveDataCache = true,

                      //},

                      //SlaveConnectionConfigs = new()
                      //{
                      //    new SlaveConnectionConfig()
                      //    {
                      //        HitRate=50,
                              
                      //        //ConnectionString=configs.SlaveConn
                      //    }

                      //},


                    InitKeyType = InitKeyType.Attribute,//?????????????
                },
            });

            CodeDb = db;
            return db;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="str">???</param>
        /// <param name="s">??</param>
        /// <param name="e">??</param>
        /// <returns></returns> 
        public static string GetValue(string str, string s, string e)
        {
            Regex rg = new("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }
    }
}

