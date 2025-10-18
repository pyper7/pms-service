namespace PMS.Application.Models.AppraisalSettings;

public class ScoringWeightDto
{
    public long Id { get; set; }
    public string Section { get; set; } = string.Empty;
    public int Weight { get; set; }
    public string? Description { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateScoringWeightsRequest
{
    public List<ScoringWeightItem> Weights { get; set; } = new();
}

public class ScoringWeightItem
{
    public string Section { get; set; } = string.Empty;
    public int Weight { get; set; }
}

public class ScoringWeightsResponse
{
    public List<ScoringWeightDto> Weights { get; set; } = new();
    public int TotalWeight { get; set; }
    public bool IsValid { get; set; }
}
