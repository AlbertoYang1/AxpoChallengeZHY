using Axpo;
using AxpoChallengeZHY.Domain;
using AxpoChallengeZHY.Domain.CustomError;
using AxpoChallengeZHY.Domain.Interfaces;

namespace AxpoChallengeZHY.Application.Managers;

public class TradeManager(IPowerService powerService) : ITradeManager
{
    private IPowerService _powerService = powerService;

    /// <inheritdoc/>
    public async Task<ReportDto> GetTradeReportAsync(DateTime dateTime)
    {
        DateTime nextDay = dateTime.Date.AddDays(1);

        var trades = await _powerService.GetTradesAsync(nextDay);

        // Throw custom exception if there's no data, actions may vary on business needs 
        if (trades is null || !trades.Any())
            throw new NoTradesException("No data was retrieved from PowerService in TradeManager");

        // We assume every period has the same Length
        var periodCount = trades.First().Periods.Length;
        // ToUniversalTime() already takes care of Daylight saving time 
        var nextDayUtc = nextDay.ToUniversalTime();
        var powerPeriods = new List<(DateTime dateTimes, double volumes)>();

        for (int i = 0; i < periodCount; i++)
        {
            powerPeriods.Add((nextDayUtc, trades.Select(t => t.Periods).Sum(p => p[i].Volume)));
            nextDayUtc = nextDayUtc.AddHours(1);
        }

        return new() { PowerPeriods = powerPeriods };
    }

}
