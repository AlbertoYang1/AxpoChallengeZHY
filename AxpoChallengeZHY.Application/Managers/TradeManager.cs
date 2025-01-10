using Axpo;
using AxpoChallengeZHY.Domain.CustomError;
using AxpoChallengeZHY.Domain.Interfaces;
using AxpoChallengeZHY.Domain.PowerReport;
using Microsoft.Extensions.Configuration;

namespace AxpoChallengeZHY.Application.Managers;

public class TradeManager(IPowerService powerService, IConfiguration configuration) : ITradeManager
{
    private readonly IPowerService _powerService = powerService ?? throw new ArgumentNullException(nameof(powerService));
    private readonly string _timeZoneId = configuration.GetSection("LocalTimeZone").Value ?? throw new ArgumentNullException("No local timeZone set on appsettings");

    private const string iso8601Format = "o";

    /// <inheritdoc/>
    // This method could have more logging
    public async Task<ReportDto> GetTradeReportAsync(DateTime nextDay)
    {
        var trades = await _powerService.GetTradesAsync(nextDay);
        var tradesList = trades.ToList();

        // Throw custom exception if there's no data, actions may vary on business needs 
        if (tradesList is null || tradesList.Count == 0)
            throw new NoTradesException("No data was retrieved from PowerService in TradeManager");

        // We assume every period has the same Length an date
        var periodCount = tradesList.First().Periods.Length;
        var powerTradeDay = tradesList.First().Date.Date;

        // We can separate this validation in a separate method and for each validation
        if (tradesList.Any(t => t.Periods.Length != periodCount || t.Date.Date != powerTradeDay))
            throw new ArgumentException("Invalid data retrieved from PowerService");

        var powerTradeDayUtc = GetLocalDateTime(powerTradeDay);
        var powerPeriods = new List<(string dateTimes, double volumes)>();

        for (int i = 0; i < periodCount; i++)
        {
            var powerTradeUtcIso = powerTradeDayUtc.ToString(iso8601Format);
            powerPeriods.Add((powerTradeUtcIso, tradesList.Select(t => t.Periods).Sum(p => p[i].Volume)));
            powerTradeDayUtc = powerTradeDayUtc.AddHours(1);
        }

        return new() { PowerPeriods = powerPeriods };
    }

    /// <summary>
    /// Transforms a powerTrade Date to a local time zone specified in appsettings and then to UTC
    /// </summary>
    /// <param name="powerPeriodDate"></param>
    /// <returns></returns>
    // This method could be moved to a utils file
    private DateTime GetLocalDateTime(DateTime powerPeriodDate)
    {
        // ToUniversalTime() already takes care of Daylight saving time 
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);

        // If the DateTime is not explicitly set as Unspecified, convert it to Unspecified
        if (powerPeriodDate.Kind != DateTimeKind.Unspecified)
        {
            powerPeriodDate = DateTime.SpecifyKind(powerPeriodDate, DateTimeKind.Unspecified);
        }

        var localDateTime = TimeZoneInfo.ConvertTimeToUtc(powerPeriodDate, timeZoneInfo);

        return localDateTime;
    }

}
