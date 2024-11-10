namespace AxpoChallengeZHY.Domain.Interfaces;

public interface ITradeManager
{
    /// <summary>
    /// Retreives and process data from PowerService
    /// </summary>
    /// <param name="dateTime"></param>
    /// <exception cref="PowerServiceException"></exception>
    /// <returns> A <see cref="ReportDto"/> contains all processed data for csv file</returns>
    Task<ReportDto> GetTradeReportAsync(DateTime dateTime);
}