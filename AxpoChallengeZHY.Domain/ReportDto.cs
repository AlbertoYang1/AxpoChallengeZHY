namespace AxpoChallengeZHY.Domain;

public sealed record ReportDto
{
    public IEnumerable<(DateTime dateTime, double volumes)> PowerPeriods { get; init; } = [];
}

