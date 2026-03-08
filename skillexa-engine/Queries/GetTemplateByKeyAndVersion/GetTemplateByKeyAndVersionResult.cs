namespace Skillexa.Engine.Queries.GetTemplateByKeyAndVersion;

public record GetTemplateByKeyAndVersionResult(
    long Id,
    string TemplateKey,
    int Version,
    bool IsActive,
    string Content);
