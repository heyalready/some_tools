using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebRdp.Service.Controllers;
using WebRdp.Service.Services;
using WebRdp.Service.Models;
using log4net;
using log4net.Config;
using System.IO;

var logger = LogManager.GetLogger(typeof(Program));
XmlConfigurator.Configure(new FileInfo("log4net.config"));

logger.Info("WebRDP Service starting...");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRdpSessionManager, RdpSessionManager>();
builder.Services.AddSingleton<IFreeRdpClientFactory, FreeRdpClientFactory>();
builder.Services.Configure<RdpSettings>(builder.Configuration.GetSection("RdpSettings"));
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowElectron", policy => policy
        .WithOrigins("http://localhost:*")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
    options.ListenLocalhost(5001);
});

var app = builder.Build();

app.UseCors("AllowElectron");
app.UseWebSockets();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

logger.Info("WebRDP Service started on http://localhost:5000");
app.Run();
