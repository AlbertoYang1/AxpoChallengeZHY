namespace AxpoChallengeZHY.Domain.PowerReport;

public sealed record ReportDto
{
    public IEnumerable<(string dateTime, double volumes)> PowerPeriods { get; init; } = [];
}