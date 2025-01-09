using CsvHelper.Configuration;

namespace AxpoChallengeZHY.Infraestructure.Utils;

internal class ReportTupleMap : ClassMap<ValueTuple<string, double>>
{
    internal ReportTupleMap()
    {
        Map(m => m.Item1).Name("DateTime");
        Map(m => m.Item2).Name("Volume");
    }
}