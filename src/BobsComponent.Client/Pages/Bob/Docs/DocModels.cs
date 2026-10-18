namespace BobsComponent.Client.Pages.Bob.Docs;

public sealed record DocParam(string Name, string Type, string Default, string Description, bool Required = false);

public sealed record DocPart(string Part, string Class, string Description);

public sealed record DocVar(string Name, string Default, string Effect);

public sealed record DocError(string Code, string When);
