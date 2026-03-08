namespace Skillexa.Engine.Queries.GetTemplateByKeyAndVersion;

public record GetTemplateByKeyAndVersionQuery(
    string TemplateKey,
    int Version) : IQuery<GetTemplateByKeyAndVersionResult?>;
