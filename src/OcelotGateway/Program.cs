using Microsoft.AspNetCore.HttpLogging;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using OcelotGateway.Config;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

#region Serilog
var logs = "logs";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogEventLevel.Information)
    .Enrich.FromLogContext()
    //uncomment to write to Azure diagnostics stream
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File($"{logs}/Info-.log", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File($"{logs}/Debug-.log", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File($"{logs}/Warning-.log", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File($"{logs}/Error-.log", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal).WriteTo.File($"{logs}/Fatal-.log", rollingInterval: RollingInterval.Day))
    .WriteTo.File($"{logs}/Verbose-.log", rollingInterval: RollingInterval.Day)
    
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();

Log.Information("Starting up");
#endregion


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var routes = "config-routes";
builder.Configuration.AddOcelotWithSwaggerSupport(options =>
{
    options.Folder = routes;
});

builder.Services.AddOcelot(builder.Configuration).AddPolly();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddOcelot(routes, builder.Environment)
    .AddEnvironmentVariables();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.ResponseHeaders.Add("Remote-Ip");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;

});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
//builder.Services.AddOcelot(builder.Configuration);



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    
}

app.UseHttpLogging();

app.Use(async (context, next) =>
{
    var conn = context.Request.HttpContext.Connection;
    
    context.Response.Headers["Remote-Ip"] = new string[] 
        { conn.LocalIpAddress.ToString(), conn.RemoteIpAddress.ToString(), conn.LocalPort.ToString(), conn.RemotePort.ToString() };

    await next();
});

app.UseAuthorization();

app.UseSwaggerForOcelotUI(options =>
        {
            options.PathToSwaggerGenerator = "/swagger/docs";
            options.ReConfigureUpstreamSwaggerJson = AlterUpstream.AlterUpstreamSwaggerJson;
        })
    .UseOcelot().Wait();

app.MapControllers();

app.Run();
