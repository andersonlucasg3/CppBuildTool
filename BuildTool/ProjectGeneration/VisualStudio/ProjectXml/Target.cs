namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

public class Target(string InCommandName, string? InCommand = null, Parameter[]? InExtraParameters = null) : TTagGroup<Tag>
{
    protected override Parameter[] Parameters => [
        new Parameter("Name", InCommandName),
        .. InExtraParameters ?? [],
    ];

    protected override Tag[] Contents
    {
        get
        {
            List<Tag> TagList = [new Message($"{InCommandName} $(ProjectName) $(Platform) $(Configuration)")];
            if (!string.IsNullOrEmpty(InCommand))
            {
                TagList.Add(new Exec(InCommand));
            }
            return [.. TagList];
        }
    }
}

public class Message(string InText) : Tag
{
    protected override Parameter[] Parameters => [
        new Parameter("Text", InText),
        new Parameter("Importance", "high"),
    ];
}

public class Exec(string InCommand) : Tag
{
    protected override Parameter[] Parameters => [
        new Parameter("Command", InCommand),
        new Parameter("WorkingDirectory", "$(SolutionDir)"),
    ];
}
