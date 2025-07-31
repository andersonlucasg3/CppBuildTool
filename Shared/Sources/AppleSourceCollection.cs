namespace Shared.Sources;

public class AppleSourceCollection : CppSourceCollection
{
    public static readonly string[] ObjCSourceFilesExtensions = [".m", ".mi"];
    public static readonly string[] ObjCppSourceFilesExtensions = [".mm", ".mii"];

    protected override string[] GetSourceFilesExtensions()
    {
        return [
            .. base.GetSourceFilesExtensions(),
            .. ObjCSourceFilesExtensions,
            .. ObjCppSourceFilesExtensions,
        ];
    }
}