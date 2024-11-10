using AxpoChallengeZHY.Domain;
using AxpoChallengeZHY.Domain.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;


namespace AxpoChallengeZHY.Infraestructure;

public class ReportRepository : IReportRepository
{
    private readonly CsvConfiguration csvConfiguration = new(CultureInfo.InvariantCulture) { Delimiter = "|", MemberTypes = MemberTypes.Fields };

    /// <inheritdoc/>
    public async Task SaveReportCsvAsync(ReportDto reportDto, string publishPath)
    {

        using var writer = new StreamWriter(publishPath!);
        using var csvWriter = new CsvWriter(writer, csvConfiguration);

        csvWriter.Context.RegisterClassMap<ReportTupleMap>();
        await csvWriter.WriteRecordsAsync(reportDto.PowerPeriods);
    }
}

