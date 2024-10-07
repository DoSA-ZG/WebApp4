using NLog.Web;
using NLog;
using RPPP_WebApp;
using RPPP_WebApp.Model;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using RPPP_WebApp.ModelsValidation;
using FluentValidation;

//NOTE: Add dependencies/services in StartupExtensions.cs and keep this file as-is

var builder = WebApplication.CreateBuilder(args);
var logger = NLog.LogManager.Setup().GetCurrentClassLogger();
logger.Debug("init main");
try
{
    logger.Debug("init main");
    builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });

    var app = builder.ConfigureServices().ConfigurePipeline();
    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

public partial class Program
{
}