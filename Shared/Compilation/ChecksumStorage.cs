using System.Security.Cryptography;
using System.Text.Json;
using Shared.IO;
using Shared.Projects;

namespace Shared.Compilation;

public class ChecksumStorage
{
    public static readonly ChecksumStorage Shared = new();

    private Dictionary<string, string> _savedChecksumsMap = [];
    private Dictionary<string, string> _computedChecksumsMap = [];
    private readonly Lock _lock = new();

    public bool ShouldRecompile(CompileAction InAction)
    {
        lock (_lock)
        {
            // if the checksum is not on the map, we need to compile it
            if (!_savedChecksumsMap.TryGetValue(InAction.SourceFile.RelativePath, out string? SavedChecksum))
            {
                return true;
            }

            if (!_computedChecksumsMap.TryGetValue(InAction.SourceFile.RelativePath, out string? ComputedChecksum))
            {
                // if the checksum is not on the map, we need to generate it
                ComputedChecksum = GenerateChecksum(InAction.SourceFile);

                _computedChecksumsMap.Add(InAction.SourceFile.RelativePath, ComputedChecksum);
            }

            // If the source file changed, we don't need to look to the dependencies
            if (ComputedChecksum != SavedChecksum)
            {
                return true;
            }
            
            // then, we check all dependency headers for changes
            bool bShouldRecompile = false;
            // need the nullable here due to shader libraries not having dependency files
            foreach (FileReference HeaderFile in InAction.Dependency?.DependencyHeaderFiles ?? [])
            {
                if (!HeaderFile.bExists)
                {
                    _savedChecksumsMap.Remove(HeaderFile.RelativePath);
                    _computedChecksumsMap.Remove(HeaderFile.RelativePath);
                    continue;
                }

                ComputedChecksum = GenerateChecksum(HeaderFile);

                if (!_savedChecksumsMap.TryGetValue(HeaderFile.RelativePath, out SavedChecksum) || SavedChecksum != ComputedChecksum)
                {
                    bShouldRecompile = true;
                }
                
                _computedChecksumsMap[HeaderFile.RelativePath] = ComputedChecksum;
            }
            return bShouldRecompile;
        }
    }

    public void CompilationSuccess(CompileAction InAction)
    {
        lock (_lock)
        {
            string RelativePath = InAction.SourceFile.RelativePath;

            if (!_computedChecksumsMap.TryGetValue(RelativePath, out string? ComputedChecksum))
            {
                ComputedChecksum = GenerateChecksum(InAction.SourceFile);
                _computedChecksumsMap.Add(RelativePath, ComputedChecksum);
            }
            _savedChecksumsMap[RelativePath] = ComputedChecksum;

            foreach (FileReference HeaderFile in InAction.Dependency?.DependencyHeaderFiles ?? [])
            {
                if (!HeaderFile.bExists)
                {
                    _savedChecksumsMap.Remove(HeaderFile.RelativePath);
                    _computedChecksumsMap.Remove(HeaderFile.RelativePath);
                    continue;
                }

                if (!_computedChecksumsMap.TryGetValue(HeaderFile.RelativePath, out ComputedChecksum))
                    {
                        ComputedChecksum = GenerateChecksum(HeaderFile);
                        _computedChecksumsMap.Add(HeaderFile.RelativePath, ComputedChecksum);
                    }
                _savedChecksumsMap[HeaderFile.RelativePath] = ComputedChecksum;
            }
        }
    }

    public void CompilationFailed(CompileAction InAction)
    {
        lock (_lock)
        {
            _savedChecksumsMap.Remove(InAction.SourceFile.RelativePath);

            foreach (FileReference HeaderFile in InAction.Dependency?.DependencyHeaderFiles ?? [])
            {
                _savedChecksumsMap.Remove(HeaderFile.RelativePath);
            }
        }
    }

    public void LoadChecksums()
    {
        DirectoryReference ChecksumsDirectory = ProjectDirectories.Shared.CreateIntermediateChecksumsDirectory();

        FileReference ChecksumsFile = ChecksumsDirectory.CombineFile("Cached.checksums");

        if (ChecksumsFile.bExists)
        {
            ChecksumsFile.OpenRead(InFileStream =>
            {
                _savedChecksumsMap = JsonSerializer.Deserialize<Dictionary<string, string>>(InFileStream) ?? [];
            });
        }
    }
    
    public void SaveChecksums()
    {
        DirectoryReference ChecksumsDirectory = ProjectDirectories.Shared.CreateIntermediateChecksumsDirectory();

        FileReference ChecksumsFile = ChecksumsDirectory.CombineFile("Cached.checksums");

        if (ChecksumsFile.bExists) ChecksumsFile.Delete();
        
        ChecksumsFile.OpenWrite(InFileStream =>
        {
            JsonSerializer.Serialize(InFileStream, _savedChecksumsMap, new JsonSerializerOptions { WriteIndented = true });
        });
    }
    
    public static string GenerateChecksum(FileReference InFile)
    {
        using SHA256 SHA = SHA256.Create();
        
        bool bGotIt = false;

        string Result = "";
        do
        {
            try
            {
                InFile.OpenRead(FileStream =>
                {
                    Result = Convert.ToHexString(SHA.ComputeHash(FileStream));
                    bGotIt = true;
                });
            }
            catch
            {
                Thread.Sleep(1);
            }
        } 
        while (!bGotIt);
        
        return Result;
    }
}