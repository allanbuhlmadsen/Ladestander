namespace Ladestander.Api.Entities;

public class ImportLog
{
    public int ImportLogId { get; set; }

    public int BillingPeriodId { get; set; }

    public DateTime ImportedAt { get; set; }

    public int ImportedCount { get; set; }

    public int SkippedCount { get; set; }

    public string? ErrorMessages { get; set; }

    public string? FileName { get; set; }

    public BillingPeriod BillingPeriod { get; set; } = null!;
}