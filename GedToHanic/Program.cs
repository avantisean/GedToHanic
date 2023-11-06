using GedToHanic;
using Serilog;
using Microsoft.Extensions.Configuration;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"{AppContext.BaseDirectory}/logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
    //var x = Host.CreateApplicationBuilder().Logging(Log)
    var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>();
        services.AddLogging(builder => builder.AddSerilog(Log.Logger));
    })
    .UseWindowsService()
    .Build();
host.Run();

