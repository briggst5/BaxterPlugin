namespace GqpMcp.Core;

public sealed class SearchHit
{
    public string? DocId { get; init; }
    public string? Revision { get; init; }
    public string? Title { get; init; }
    public string? SectionHeading { get; init; }
    public string? Content { get; init; }
    public string? PageNumber { get; init; }
}
