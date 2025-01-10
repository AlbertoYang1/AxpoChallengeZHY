using AxpoChallengeZHY.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace AxpoChallengeZHY.Application.Managers;

public class ReportManager(ITradeManager tradeManager,
    IConfiguration configuration,
    IReportRepository reportRepository,
    ILogger<ReportManager> logger,
    ResiliencePipelineProvider<string> pipelineProvider)
    : IReportManager
{
    private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("retryPipeline")
        ?? throw new ArgumentNullException(nameof(pipelineProvider), "Pipeline provider cannot be null.");

    private readonly string _basePath = configuration.GetSection("CsvHelperConfig:PublishPath").Value 
        ?? throw new ArgumentNullException(nameof(configuration), "Null configuration section");

    /// <inheritdoc/>
    // This method could have more logging
    public async Task GenerateReportAsync(DateTime reportDate, Guid identifier)
    {
        if (reportDate == default)
            throw new ArgumentException("Invalid date provided for reportDate");

        var nextDay = reportDate.AddDays(1);
        var fileName = GenerateNameCsv(reportDate, nextDay);
        var publishPath = Path.Combine(_basePath, fileName);

        // pipeline which handle the retries, configured in program.cs
        var reportDto = await _pipeline.ExecuteAsync(async _ => await tradeManager.GetTradeReportAsync(nextDay));

        // Create directory if it does not exist
        Directory.CreateDirectory(_basePath);

        await reportRepository.SaveReportCsvAsync(reportDto, publishPath);
        logger.LogInformation("Identifier: {Identifier} at reportDate: {ReportDate}. Published report {FileName} in {Path}"
            , identifier, reportDate, fileName, publishPath);
    }

    /// <summary>
    /// Generates a name for CSV file with specified format
    /// </summary>
    /// <param name="reportDate">Date of the request</param>
    /// <returns>csv file name for the power report</returns>
    private static string GenerateNameCsv(DateTime reportDate, DateTime nextDay) =>
        $"PowerPosition_{nextDay:yyyyMMdd}_{reportDate:yyyyMMddHHmm}.csv";
}
