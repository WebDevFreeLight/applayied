using Infrastructure;
using Infrastructure.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using SqlSugar.IOC;
using ZR.Common;
using ZR.Model.System;

namespace ZR.ServiceCore.SqlSugar
{
    public static class SqlsugarSetup
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initialize database
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environment"></param>
        /// <param name="configuration"></param>
        public static void AddDb(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration = null)
        {
            // ✅ Use App.OptionsSetting if available (after Build), otherwise get from configuration
            // This allows AddDb to be called both BEFORE and AFTER builder.Build()
            OptionsSetting options;
            if (App.OptionsSetting != null)
            {
                options = App.OptionsSetting;
            }
            else if (configuration != null)
            {
                options = configuration.Get<OptionsSetting>();
            }
            else
            {
                throw new InvalidOperationException("Cannot initialize database: Configuration is not available");
            }

            List<DbConfigs> dbConfigs = options.DbConfigs;




            var iocList = new List<IocConfig>();
            foreach (var item in dbConfigs)
            {

                Console.WriteLine();
                Console.WriteLine($"   ConfigId: {item.ConfigId ?? "NULL"}");
                Console.WriteLine($"   DbType: {item.DbType}");
                Console.WriteLine($"   Connection: {item.Conn}");
                Console.WriteLine($"   AutoClose: {item.IsAutoCloseConnection}");



                iocList.Add(new IocConfig()
                {
                    ConfigId = item.ConfigId,
                    ConnectionString = item.Conn,
                    DbType = (IocDbType)item.DbType,
                    IsAutoCloseConnection = item.IsAutoCloseConnection,
                    SlaveConnectionConfigs =new()
                    {
                        new  ()
                        {
                             DbType =  (IocDbType)item.DbType, 
                             IsAutoCloseConnection= item.IsAutoCloseConnection,
                             ConnectionString= item.Conn,
                             ConfigId= item.ConfigId,
                            SlaveConnectionConfigs=  new ()
                            {
                                    new  ()
                                {
                                    ConnectionString = item.Conn,
                                     
                                    DbType = (IocDbType)item.DbType,
                                    IsAutoCloseConnection = item.IsAutoCloseConnection,
                                    ConfigId = item.ConfigId,
                                }
                            }
                        }
                    }
                });
            }
            SugarIocServices.AddSqlSugar(iocList);

            // ✅ Register ISqlSugarClient in ASP.NET Core DI container
            // This is required for constructor injection to work
            services.AddScoped<ISqlSugarClient>(provider =>
            {
                return DbScoped.SugarScope;
            });

            ICacheService cache;
            if (options.RedisServer.DbCache)
            {
                cache = new SqlSugarRedisCache();
            }
            else
            {
                cache = new SqlSugarCache();
            }
            SugarIocServices.ConfigurationSugar(db =>
            {
                // Global soft-delete filter: automatically exclude soft-deleted records
                db.QueryFilter.AddTableFilter<ZR.Model.ISoftDelete>(it => it.IsDeleted == false);

                var user = App.User;
                if (user != null)
                {
                    DataPermi.FilterData("0");
                }

                iocList.ForEach(iocConfig =>
                {
                    SetSugarAop(db, iocConfig, cache);



                });
            });

            if (environment.IsDevelopment())
            {
                //InitTable.InitDb(options.InitDb);

                //InitTable.InitNewTb();
            }
        }

        /// <summary>
        /// Database AOP settings
        /// </summary>
        /// <param name="db"></param>
        /// <param name="iocConfig"></param>
        /// <param name="cache"></param>
        private static void SetSugarAop(SqlSugarClient db, IocConfig iocConfig, ICacheService cache)
        {
            var config = db.GetConnectionScope(iocConfig.ConfigId).CurrentConnectionConfig;
            var showDbLog = AppSettings.Get<bool>("ShowDbLog");
            string configId = config.ConfigId;
            db.GetConnectionScope(configId).Aop.OnLogExecuting = (sql, pars) =>
            {
                if (showDbLog)
                {
                    string log = $"?db{configId} SQL?{UtilMethods.GetSqlString(config.DbType, sql, pars)}\n";
                    if (sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info(log);
                    }
                    else if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Warn(log);
                    }
                    else if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("TRUNCATE", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Error(log);
                    }
                    else
                    {
                        log = $"?db{configId} SQL Statement?dbo.{sql} {string.Join(", ", pars.Select(x => x.ParameterName + " = " + GetParsValue(x)))};\n";
                        logger.Info(log);
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(sql);
                }
            };
            db.GetConnectionScope(configId).Aop.OnError = (ex) =>
            {
                //var pars = db.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));

                string sql = $"?Error SQL, db={configId}?{UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres)}\r\n";
                logger.Error(ex, $"{sql}\r\n{ex.Message}\r\n{ex.StackTrace}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(sql);
            };



            db.Aop.CheckConnectionExecuted = (x, t) =>
            {
            

          
            };



            db.GetConnectionScope(configId).Aop.DataExecuting = (oldValue, entityInfo) =>
            {
                var user = JwtUtil.GetLoginUser(App.HttpContext);
                if (user == null)
                    return;
                if (
                    entityInfo.PropertyName == "Create_time"
                    && entityInfo.OperationType == DataFilterType.InsertByObject
                )
                {
                    entityInfo.SetValue(DateTime.Now); /*oldValue represeEquivalent to the following writing current field value Equivalent to the following writing*/
                    // var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);//entityInfo.IsAnyAttribute<characteristic>()  //entityInfo.GetAttribute<characteristic>()
                }
                if (
                    entityInfo.PropertyName == "Create_by"
                    && entityInfo.OperationType == DataFilterType.InsertByObject
                )
                {
                    entityInfo.SetValue(user.UserName); /*oldValue represeEquivalent to the following writing current field value Equivalent to the following writing*/
                    // var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);//entityInfo.IsAnyAttribute<characteristic>()  //entityInfo.GetAttribute<characteristic>()
                }




                if (entityInfo.PropertyName == "CreatedIdby"
                    && entityInfo.OperationType == DataFilterType.InsertByObject)
                {
                    entityInfo.SetValue(user.UserId); /*oldValue represeEquivalent to the following writing current field value Equivalent to the following writing*/
                    // var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);//entityInfo.IsAnyAttribute<characteristic>()  //entityInfo.GetAttribute<characteristic>()
                }



                if (
                    entityInfo.PropertyName == "CompanyID"
                    && entityInfo.OperationType == DataFilterType.InsertByObject
                )
                {
                    entityInfo.SetValue(user.CompanyID); /*oldValue represeEquivalent to the following writing current field value Equivalent to the following writing*/
                    // var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);//entityInfo.IsAnyAttribute<characteristic>()  //entityInfo.GetAttribute<characteristic>()
                }

                if (
                    entityInfo.PropertyName == "BranchID"
                    && entityInfo.OperationType == DataFilterType.InsertByObject
                )
                {
                    entityInfo.SetValue(user.BranchID); /*oldValue represeEquivalent to the following writing current field value Equivalent to the following writing*/
                    // var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);//entityInfo.IsAnyAttribute<characteristic>()  //entityInfo.GetAttribute<characteristic>()
                }

                /*** Row level events: a record will only be entered once ***/
                if (entityInfo.EntityColumnInfo.IsPrimarykey)
                {
                    //entityInfo.EntityValue Get a single entity object
                }

                //You can write multiple IFs

                #region on null
                /// on null

                if (
                    entityInfo.PropertyName == "Create_time"
                    && entityInfo.OperationType == DataFilterType.UpdateByObject
                )
                {
                    var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(
                        entityInfo.EntityValue
                    );
                    if (value == null)
                    {
                        entityInfo.SetValue(DateTime.Now);
                    }
                }
                if (entityInfo.PropertyName == "Create_by"
                    && entityInfo.OperationType == DataFilterType.UpdateByObject)
                {
                    var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(
                        entityInfo.EntityValue
                    );
                    if (value == null)
                    {
                        entityInfo.SetValue(user.UserName);
                    }
                }
                if (entityInfo.PropertyName == "CreatedIdby"
                                  && entityInfo.OperationType == DataFilterType.UpdateByObject)
                {
                    var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(
                        entityInfo.EntityValue
                    );
                    if (value == null)
                    {
                        entityInfo.SetValue(user.UserId);
                    }
                }



                if (
                    entityInfo.PropertyName == "CompanyID"
                    && entityInfo.OperationType == DataFilterType.UpdateByObject
                )
                {
                    var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(
                        entityInfo.EntityValue
                    );
                    if (value.IsNullOrZero())
                    {
                        entityInfo.SetValue(user.CompanyID);
                    }
                }

                if (
                    entityInfo.PropertyName == "BranchID"
                    && entityInfo.OperationType == DataFilterType.UpdateByObject
                )
                {
                    var value = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(
                        entityInfo.EntityValue
                    );
                    if (value.IsNullOrZero())
                    {
                        entityInfo.SetValue(user.BranchID);
                    }
                }

                #endregion

                if (
                    entityInfo.PropertyName == "Update_time"
                    && entityInfo.OperationType == DataFilterType.UpdateByObject
                )
                {
                    entityInfo.SetValue(DateTime.Now); //Modify the UpdateTime field
                }


                if (
                    entityInfo.PropertyName == "Update_by"
                    && entityInfo.OperationType == DataFilterType.UpdateByObject
                )
                {
                    entityInfo.SetValue(user.UserName); //Modify the UpdateTime field
                }



                if (entityInfo.PropertyName == "UpdateIdby"
                    && entityInfo.OperationType == DataFilterType.UpdateByObject
                )
                {
                    entityInfo.SetValue(user.UserId); //Modify the UpdateTime field
                }


                /*** Row-level events: update a record only once ***/
                if (entityInfo.EntityColumnInfo.IsPrimarykey)
                {
                    //entityInfo.EntityValue Get a single entity object
                }

                /***  Modify another column based on the current column ***/
                //if(Current column logic==XXX)
                //var properyDate = entityInfo.EntityValue.GetType().GetProperty("Date"); //if(properyDate!=null)
                //properyDate.SetValue(entityInfo.EntityValue,1);

                //You can write multiple IFs
            };



          














            // Difference logging functionality
            db.GetConnectionScope(configId).Aop.OnDiffLogEvent = it =>
            {
                // Pre-operation records include: field description, column name, value, table name, table description
                var editBeforeData = it.BeforeData; // Before is null for inserts, not yet in database
                // Post-operation records include: field description, column name, value, table name, table description
                var editAfterData = it.AfterData;
                var sql = it.Sql;
                var parameter = it.Parameters;
                var data = it.BusinessData; // This will display the object you passed in
                var time = it.Time;
                var diffType = it.DiffType; // enum insert, update and delete  
                string name = App.UserName;

                foreach (var item in editBeforeData)
                {
                    var pars = db.Utilities.SerializeObject(item.Columns.ToDictionary(it => it.ColumnName, it => it.Value));

                    SqlDiffLog log = new()
                    {
                        BeforeData = pars,
                        BusinessData = data?.ToString(),
                        DiffType = diffType.ToString(),
                        Sql = sql,
                        TableName = item.TableName,
                        UserName = name,
                        AddTime = DateTime.Now,
                        ConfigId = configId
                    };
                    if (editAfterData != null)
                    {
                        var afterData = editAfterData?.First(x => x.TableName == item.TableName);
                        var afterPars = db.Utilities.SerializeObject(afterData?.Columns.ToDictionary(it => it.ColumnName, it => it.Value));
                        log.AfterData = afterPars;
                    }
                    //logger.WithProperty("title", data).Info(pars);
                    db.GetConnectionScopeWithAttr<SqlDiffLog>()
                    .Insertable(log)
                    .ExecuteReturnSnowflakeId();
                }
            };
            db.GetConnectionScope(configId).CurrentConnectionConfig.MoreSettings = new ConnMoreSettings()
            {

                IsAutoRemoveDataCache = true,
                //DefaultCacheDurationInSeconds = 60,

                SqliteCodeFirstEnableDefaultValue = true,
                DisableMillisecond=true,
                
                PgSqlIsAutoToLowerCodeFirst = true,
                PgSqlIsAutoToLower = true,
                PgSqlIsAutoToLowerSchema = true,
                
                // Fix for default scalar values - commented out as these properties don't exist in current version
                // IsDefaultNumberConvert = true,
                // DefaultNumberConvertType = typeof(int),

                SqlServerCodeFirstNvarchar = true,
                EnableCodeFirstUpdatePrecision = true,
                //DbMinDate = Convert.ToDateTime("1000-01-01"),
                EnableOracleIdentity = true,
                OracleCodeFirstNvarchar2 = true,
                

                SqliteCodeFirstEnableDropColumn = true,
            };




            db.GetConnectionScope(configId).CurrentConnectionConfig.ConfigureExternalServices = new ConfigureExternalServices()
            {
                DataInfoCacheService = cache,
                
                EntityService = (c, p) =>
                {
                    if (p.IsPrimarykey == true) // Primary key cannot be null
                    {
                        p.IsNullable = false;
                    }
                    else if (p.ExtendedAttribute?.ToString() == ProteryConstant.NOTNULL.ToString())
                    {
                        p.IsNullable = false;
                    }
                    else // Otherwise default to null
                    {
                        p.IsNullable = true;
                    }



                    //p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName);//ToUnderLine????????


                    //if (config.DbType == DbType.PostgreSQL)
                    //{
                    //    if (c.Name == nameof(SysMenu.IsCache) || c.Name == nameof(SysMenu.IsFrame))
                    //    {
                    //        p.DataType = "char(1)";
                    //    }
                    //}




                    if (config.DbType == DbType.SqlServer)
                    {
                        if (p.UnderType == typeof(decimal))
                        {
                            p.DataType = "decimal(38, 8)";
                        }

                        var v = p.EntityName.ToLower();
                        if (!v.EndsWith("dto"))
                        {
                            if (p.UnderType == typeof(string))
                            {
                                p.DataType = " nvarchar(max) ";
                                p.Length = 0;
                                if (p.EntityName == nameof(SysTasks))
                                {
                                    if (p.DbColumnName == nameof(SysTasks.ID))
                                    {
                                        p.DataType = " nvarchar(2500) ";
                                        p.Length = 0;
                                    }
                                }

                                if (p.EntityName == nameof(SysDictData))
                                {
                                    if (p.DbColumnName == nameof(SysDictData.DictValue))
                                    {
                                        p.DataType = " nvarchar(100) ";
                                        p.Length = 0;
                                    }
                                    if (p.DbColumnName == nameof(SysDictData.DictType))
                                    {
                                        p.DataType = " nvarchar(100) ";
                                        p.Length = 0;
                                    }
                                }
                            }
                        }
                    }

                    if (config.DbType == DbType.PostgreSQL)
                    {
                        if (
                            c.Name == nameof(SysMenu.IsCache)
                            || c.Name == nameof(SysMenu.IsFrame)
                        )
                        {
                            p.DataType = "char(1)";
                        }



                        if (p.UnderType == typeof(string))
                        {
                            p.DataType = "TEXT ";
                            p.Length = 0;
                            if (p.EntityName == nameof(SysTasks))
                            {
                                if (p.DbColumnName == nameof(SysTasks.ID))
                                {
                                    p.DataType = "TEXT";
                                    p.Length = 0;
                                }
                            }

                            if (p.EntityName == nameof(SysDictData))
                            {
                                if (p.DbColumnName == nameof(SysDictData.DictValue))
                                {
                                    p.DataType = "TEXT";
                                    p.Length = 0;
                                }
                                if (p.DbColumnName == nameof(SysDictData.DictType))
                                {
                                    p.DataType = "TEXT";
                                    p.Length = 0;
                                }
                            }
                        }










                    }





                    #region SQLite compatibility
                    if (config.DbType == DbType.Sqlite)
                    {
                        if (p.IsPrimarykey && c.PropertyType == typeof(long))
                        {
                            p.DataType = "INTEGER";
                        }
                    }
                    #endregion

                    #region Oracle compatibility
                    if (config.DbType == DbType.Oracle)
                    {
                        if (p.IsIdentity == true)
                        {
                            if (p.EntityName == nameof(SysUser))
                            {
                                p.OracleSequenceName = "SEQ_SYS_USER_USERID";
                            }
                            else if (p.EntityName == nameof(SysRole))
                            {
                                p.OracleSequenceName = "SEQ_SYS_ROLE_ROLEID";
                            }
                            else if (p.EntityName == nameof(SysDept))
                            {
                                p.OracleSequenceName = "SEQ_SYS_DEPT_DEPTID";
                            }
                            else if (p.EntityName == nameof(SysMenu))
                            {
                                p.OracleSequenceName = "SEQ_SYS_MENU_MENUID";
                            }
                            else
                            {
                                p.OracleSequenceName = "SEQ_ID";
                            }
                        }
                    }
                    #endregion
                }
          
            
            



            };
            db.GetConnectionScope(configId).Aop.OnLogExecuted = (sql, pars) =>
            {
                var sqlExecutionTime = AppSettings.Get<int>("sqlExecutionTime");
                if (db.Ado.SqlExecutionTime.TotalSeconds > sqlExecutionTime)
                {
                    // Code CS file name
                    var fileName = db.Ado.SqlStackTrace.FirstFileName;
                    // Code line number
                    var fileLine = db.Ado.SqlStackTrace.FirstLine;
                    // Method name
                    var FirstMethodName = db.Ado.SqlStackTrace.FirstMethodName;
                    var logInfo = $"SQL execution timeout, took {db.Ado.SqlExecutionTime.TotalSeconds} seconds?{sql}?,fileName={fileName},line={fileLine},methodName={FirstMethodName}";
                    WxNoticeHelper.SendMsg("SQL request took too long", logInfo);



                    logger.Warn(logInfo);
                }
            };
        }

        private static object GetParsValue(SugarParameter x)
        {
            if (x.DbType == System.Data.DbType.String || x.DbType == System.Data.DbType.DateTime || x.DbType == System.Data.DbType.String)
            {
                return "'" + x.Value + "'";
            }
            return x.Value;
        }
    }
}

