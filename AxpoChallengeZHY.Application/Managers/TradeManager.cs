using Axpo;
using AxpoChallengeZHY.Domain;
using AxpoChallengeZHY.Domain.CustomError;
using AxpoChallengeZHY.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AxpoChallengeZHY.Application.Managers;

public class TradeManager(IPowerService powerService, IConfiguration configuration) : ITradeManager
{
    private IPowerService _powerService = powerService;
    private readonly string _timeZoneId = configuration.GetSection("LocalTimeZone").Value ?? throw new ArgumentNullException("No local timeZone set on appsettings");

    /// <inheritdoc/>
    public async Task<ReportDto> GetTradeReportAsync(DateTime dateTime)
    {

        var trades = await _powerService.GetTradesAsync(dateTime.Date.AddDays(1));

        // Throw custom exception if there's no data, actions may vary on business needs 
        if (trades is null || !trades.Any())
            throw new NoTradesException("No data was retrieved from PowerService in TradeManager");

        // We assume every period has the same Length an date
        var periodCount = trades.First().Periods.Length;
        var powerTradeDay = trades.First().Date;

        var nextDayUtc = GetLocalDateTime(powerTradeDay);
        var powerPeriods = new List<(DateTime dateTimes, double volumes)>();

        for (int i = 0; i < periodCount; i++)
        {
            powerPeriods.Add((nextDayUtc, trades.Select(t => t.Periods).Sum(p => p[i].Volume)));
            nextDayUtc = nextDayUtc.AddHours(1);
        }

        return new() { PowerPeriods = powerPeriods };
    }

    /// <summary>
    /// Transforms a powerTrade Date to a local time zone specified in appsettings and then to UTC
    /// </summary>
    /// <param name="powerPeriodDate"></param>
    /// <returns></returns>
    // This method could be moved to a utils file
    private DateTime GetLocalDateTime(DateTime powerPeriodDate) =>
        // ToUniversalTime() already takes care of Daylight saving time 
        TimeZoneInfo.ConvertTime(powerPeriodDate, TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId)).ToUniversalTime();

}
