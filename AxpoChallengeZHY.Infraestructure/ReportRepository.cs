using AxpoChallengeZHY.Domain.Interfaces;
using AxpoChallengeZHY.Domain.PowerReport;
using AxpoChallengeZHY.Infraestructure.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;


namespace AxpoChallengeZHY.Infraestructure;

public class ReportRepository : IReportRepository
{
    private const string csvDelimiter = ";";
    private readonly CsvConfiguration _csvConfiguration = new(CultureInfo.InvariantCulture) { Delimiter = csvDelimiter, MemberTypes = MemberTypes.Fields };

    /// <inheritdoc/>
    public async Task SaveReportCsvAsync(ReportDto reportDto, string publishPath)
    {

        using var writer = new StreamWriter(publishPath!);
        using var csvWriter = new CsvWriter(writer, _csvConfiguration);

        csvWriter.Context.RegisterClassMap<ReportTupleMap>();
        await csvWriter.WriteRecordsAsync(reportDto.PowerPeriods);
    }
}

