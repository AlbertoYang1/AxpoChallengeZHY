namespace AxpoChallengeZHY.Domain.Interfaces;

public interface IReportRepository
{
    /// <summary>
    /// Saves PowerPosition data into a CSV file
    /// </summary>
    /// <param name="reportDto">CSV data</param>
    /// <param name="publishPath">Path to publish the CSV</param>
    /// <returns></returns>
    Task SaveReportCsvAsync(ReportDto reportDto, string publishPath);
}
