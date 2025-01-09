using AxpoChallengeZHY.Domain.Interfaces;
using Coravel.Invocable;

namespace AxpoChallengeZHY;

public class PowerReportService(ILogger<PowerReportService> logger, IReportManager reportManager) : IInvocable
{
    private readonly ILogger<PowerReportService> _logger = logger;
    private readonly IReportManager _reportManager = reportManager;

    /// <summary>
    /// Service invoked by the scheduler, 
    /// it calls the method to generate the CSV with all the information
    /// </summary>
    /// <returns></returns>
    public async Task Invoke()
    {
        // DateTime.Now has a local timeZone from the machine it can be changed to offset
        var reportDate = DateTime.Now;
        var identifier = Guid.NewGuid().ToString();

        // In production it can be done in another way. Readme logging
        _logger.LogInformation("Start report generation for Date: {reportDate} and id: {identifier}", reportDate, identifier);

        try
        {
            await _reportManager.GenerateReportCsvAsync(reportDate, identifier);

            _logger.LogInformation("End report generation for Date: {reportDate} and id: {identifier}", reportDate, identifier);
        }
        catch (Exception ex)
        {
            // LogCritical when one report fail to generate
            _logger.LogCritical(ex, "Error on generating report for Date: {reportDate} and id: {identifier} ErrorMessage: {message}"
                , reportDate, identifier, ex.Message);
        }
    }
}
