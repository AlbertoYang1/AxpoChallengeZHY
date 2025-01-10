namespace AxpoChallengeZHY.Domain.Interfaces;

public interface IReportManager
{
    /// <summary>
    /// It generates and save a CSV with PowerTrades of the next day 
    /// </summary>
    /// <param name="reportDate">Date of the request</param>
    /// <param name="identifier">Identifier of the request</param>
    /// <returns></returns>
    Task GenerateReportAsync(DateTime reportDate, Guid identifier);
}
