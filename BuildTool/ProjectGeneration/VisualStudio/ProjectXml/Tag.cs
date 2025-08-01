﻿using Shared.Misc;

namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

public abstract class Tag : IIndentedStringBuildable
{
    protected virtual string TagName { get; }
    protected readonly string? RawContent;

    protected virtual Parameter[] Parameters { get; } = [];

    public Tag(string? InRawContent = null)
    {
        TagName = GetType().Name;
        RawContent = InRawContent;
    }

    public void Build(IndentedStringBuilder InStringBuilder)
    {
        string ParametersString = Parameters.Length > 0 ? $" {string.Join(' ', (IEnumerable<Parameter>)Parameters)}" : "";
        if (RawContent != null)
        {
            InStringBuilder.AppendLine($"<{TagName}{ParametersString}>{RawContent}</{TagName}>");

            return;
        }

        InStringBuilder.AppendLine($"<{TagName}{ParametersString} />");
    }
}