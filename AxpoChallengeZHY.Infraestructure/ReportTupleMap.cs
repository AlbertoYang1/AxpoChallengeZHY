using CsvHelper.Configuration;

namespace AxpoChallengeZHY.Infraestructure;

internal class ReportTupleMap : ClassMap<ValueTuple<DateTime, double>>
{
    internal ReportTupleMap()
    {
        Map(m => m.Item1).Name("DateTime");
        Map(m => m.Item2).Name("Volume");
    }
}

