using AxpoChallengeZHY.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace AxpoChallengeZHY.Application.Managers;

public class ReportManager : IReportManager
{

    private readonly ITradeManager _tradeManager;
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<ReportManager> _logger;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ResiliencePipeline _pipeline;
    private readonly string basePath;

    public ReportManager(ITradeManager tradeManager, IConfiguration configuration, IReportRepository reportRepository, ILogger<ReportManager> logger, ResiliencePipelineProvider<string> pipelineProvider)
    {
        basePath = configuration.GetSection("CsvHelperConfig:PublishPath").Value ?? throw new ArgumentNullException(nameof(ReportManager), "Null configuration section");
        _tradeManager = tradeManager;
        _reportRepository = reportRepository;
        _logger = logger;
        _pipelineProvider = pipelineProvider;
        _pipeline = _pipelineProvider.GetPipeline("retryPipeline");
    }

    /// <inheritdoc/>
    public async Task GenerateReportCsvAsync(DateTime reportDate, string identifier)
    {
        var fileName = GenerateNameCsv(reportDate);
        var publishPath = string.Concat([basePath, fileName]);

        // pipeline which handle the retries, configured in program.cs
        var reportDto = await _pipeline.ExecuteAsync(async ct =>
        {
            return await _tradeManager.GetTradeReportAsync(reportDate); ;
        });

        await _reportRepository.SaveReportCsvAsync(reportDto, publishPath);
        _logger.LogInformation("Identifier: {identifier} at reportDate: {reportDate}. Published report {fileName} in {path}"
            , identifier, reportDate, fileName, publishPath);
    }

    /// <summary>
    /// Generates a name for CSV file with specified format
    /// </summary>
    /// <param name="reportDate">Date of the request</param>
    /// <returns>csv file name for the power report</returns>
    private static string GenerateNameCsv(DateTime reportDate)
    {
        return $"PowerPosition_{reportDate:yyyyMMdd}_{reportDate:yyyyMMddhhmm}.csv";
    }
}
