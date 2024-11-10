using Axpo;
using AxpoChallengeZHY;
using AxpoChallengeZHY.Application.Managers;
using AxpoChallengeZHY.Domain.CustomError;
using AxpoChallengeZHY.Domain.Interfaces;
using AxpoChallengeZHY.Infraestructure;
using Coravel;
using Polly;
using Polly.Retry;
using Serilog;

var builder = Host.CreateApplicationBuilder();

const string pipelineKey = "retryPipeline";

// Add DI
builder.Services.AddScheduler();
builder.Services.AddScoped<PowerReportService>();
builder.Services.AddScoped<IReportManager, ReportManager>();
builder.Services.AddScoped<ITradeManager, TradeManager>();
builder.Services.AddScoped<IPowerService, PowerService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Add Serilog
builder.Services.AddSerilog(config => config
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
#if DEBUG    
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
#endif
    .WriteTo.File(Path.Join(builder.Environment.ContentRootPath, "\\logs\\axpoChallenge.log")));

// Add coravel pipeline configuration for retries
builder.Services.AddResiliencePipeline(pipelineKey, x =>
{
    x.AddRetry(new RetryStrategyOptions
    {
        // Exceptions that triggers retries, default do not retry
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: PowerServiceException } => PredicateResult.True(),
            { Exception: NoTradesError } => PredicateResult.True(),
            _ => PredicateResult.False(),
        },
        Delay = TimeSpan.FromSeconds(3),
        MaxRetryAttempts = 5,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
    });
});

var app = builder.Build();

// Config the scheduler
app.Services.UseScheduler(scheduler =>
{
    // Run every X minute confire in appsettings
    var minuteInterval = builder.Configuration.GetSection("CoravelPipelineCofig:MinuteInterval").Value;
    scheduler.Schedule<PowerReportService>()
       .Cron($"{DateTime.Now.Minute}-59/{minuteInterval} * * * *")
       .RunOnceAtStart()
       .PreventOverlapping(Guid.NewGuid().ToString());
});

app.Run();
