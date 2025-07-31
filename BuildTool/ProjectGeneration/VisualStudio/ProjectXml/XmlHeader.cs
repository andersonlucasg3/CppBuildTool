using Shared.Misc;

namespace BuildTool.ProjectGeneration.VisualStudio.ProjectXml;

public static class XmlHeader
{
    public const string XmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

    public static void Build(IndentedStringBuilder InStringBuilder)
    {
        InStringBuilder.AppendLine($"""<?xml version="1.0" encoding="utf-8"?>""");
    }
}
