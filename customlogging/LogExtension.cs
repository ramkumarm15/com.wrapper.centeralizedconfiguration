using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;

namespace customlogging
{
    public static class LogExtension
    {
        public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Logging.ClearProviders();

            var serilogOptions = configuration.GetSection("CustomLogging").Get<CustomLoggingOptions>();

            GetSqlServerSinkOptions(serilogOptions.TableName, out MSSqlServerSinkOptions sinkOptions);
            GetColumnOptions(out ColumnOptions columnOptions);

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: serilogOptions.ConnectionString,
                    sinkOptions: sinkOptions,
                    columnOptions: columnOptions
                )
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.WithProperty("Log_AppName", serilogOptions.ApplicationName)
                .Enrich.WithProperty("Log_ServiceName", serilogOptions.ServiceName)
                .Enrich.WithProperty("Log_MachineName", Environment.MachineName)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog(logger);

            return builder;
        }

        public static MSSqlServerSinkOptions GetSqlServerSinkOptions(string tableName, out MSSqlServerSinkOptions sinkOptions)
        {
            sinkOptions = new MSSqlServerSinkOptions();
            sinkOptions.TableName = tableName;
            sinkOptions.AutoCreateSqlDatabase = true;
            sinkOptions.AutoCreateSqlTable = true;

            return sinkOptions;
        }

        public static ColumnOptions GetColumnOptions(out ColumnOptions columnOptions)
        {
            columnOptions = new ColumnOptions();
            columnOptions.Id.PropertyName = "Log_Id";
            columnOptions.Id.ColumnName = "Log_Id";
            columnOptions.Message.PropertyName = "Log_Message";
            columnOptions.Message.ColumnName = "Log_Message";
            columnOptions.Level.PropertyName = "Log_Level";
            columnOptions.Level.ColumnName = "Log_Level";
            columnOptions.Exception.PropertyName = "Log_Exception";
            columnOptions.Exception.ColumnName = "Log_Exception";
            columnOptions.TimeStamp.PropertyName = "Log_Timestamp";
            columnOptions.TimeStamp.ColumnName = "Log_Timestamp";



            columnOptions.Store.Remove(StandardColumn.MessageTemplate);
            columnOptions.Store.Remove(StandardColumn.Properties);


            columnOptions.AdditionalColumns = new Collection<SqlColumn>()
            {
                new SqlColumn
                {
                    ColumnName = "Log_AppName",
                    DataType = SqlDbType.VarChar,
                    DataLength = 255,
                    AllowNull = true,
                },
                new SqlColumn
                {
                    ColumnName = "Log_ServiceName",
                    DataType = SqlDbType.VarChar,
                    DataLength = 255,
                    AllowNull = true,
                },
                new SqlColumn
                {
                    ColumnName = "Log_CorrelationId",
                    DataType = SqlDbType.VarChar,
                    DataLength = 255,
                    AllowNull = true,
                },
                new SqlColumn
                {
                    ColumnName = "Log_RequestMethod",
                    DataType = SqlDbType.VarChar,
                    DataLength = 255,
                    AllowNull = true,
                },
                new SqlColumn
                {
                    ColumnName = "Log_OperationName",
                    DataType = SqlDbType.VarChar,
                    DataLength = 255,
                    AllowNull = true,
                },
                new SqlColumn
                {
                    ColumnName = "Log_MachineName",
                    DataType = SqlDbType.VarChar,
                    DataLength = 255,
                    AllowNull = true,
                }
            };

            return columnOptions;
        }

        public static string GetCorrelationId(this HttpContext context)
        {
            StringValues values = new StringValues();
            context.Request?.Headers.TryGetValue("CorrelationId", out values);
            return !string.IsNullOrWhiteSpace(values.FirstOrDefault()) ? values.FirstOrDefault() : Guid.NewGuid().ToString();
        }

        public static string GetActionName(this HttpContext context)
        {
            return context.Request?.RouteValues?["action"]?.ToString();
        }

        public static string GetOperationType(this HttpContext context)
        {
            return context.Request?.Method?.ToString();
        }

        public static IApplicationBuilder UseLogContext(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<LogContextMiddleware>();

            return builder;
        }
    }
}
