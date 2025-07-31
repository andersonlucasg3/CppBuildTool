namespace Shared.Projects;

using IO;
using Exceptions;
using Shared.Platforms;

public abstract class ProjectDefinition : Definition
{
    private bool _bIsConfigured = false;

    private readonly Dictionary<ETargetPlatform, HashSet<ProjectDefinition>> _dependencyProjectsPerPlatform = [];
    private readonly Dictionary<ETargetPlatform, Dictionary<string, ModuleDefinition>> _modulesPerPlatform = [];

    public IReadOnlyDictionary<string, ModuleDefinition> GetModules(ETargetPlatform InTargetPlatform)
    {
        if (!_modulesPerPlatform.TryGetValue(InTargetPlatform, out Dictionary<string, ModuleDefinition>? ModuleMap))
        {
            return new Dictionary<string, ModuleDefinition>();
        }

        return ModuleMap;
    }

    public IReadOnlySet<ProjectDefinition> GetDependencyProjects(ETargetPlatform InTargetPlatform)
    {
        if (!_dependencyProjectsPerPlatform.TryGetValue(InTargetPlatform, out HashSet<ProjectDefinition>? ProjectSet))
        {
            return new HashSet<ProjectDefinition>();
        }

        return ProjectSet;
    }

    protected abstract void Configure();

    protected void AddProjectDependency<TProject>(ETargetPlatform InTargetPlatform = ETargetPlatform.Any)
        where TProject : ProjectDefinition
    {
        TProject Project = ProjectFinder.FindProject<TProject>();

        if (!_dependencyProjectsPerPlatform.TryGetValue(InTargetPlatform, out HashSet<ProjectDefinition>? ProjectSet))
        {
            ProjectSet = [];
            _dependencyProjectsPerPlatform.Add(InTargetPlatform, ProjectSet);
        }

        ProjectSet.Add(Project);

        foreach (KeyValuePair<ETargetPlatform, Dictionary<string, ModuleDefinition>> Pair in Project._modulesPerPlatform)
        {
            _modulesPerPlatform.Add(Pair.Key, Pair.Value);
        }
    }

    protected void AddModule<TModule>(ETargetPlatform InTargetPlatform = ETargetPlatform.Any) 
        where TModule : ModuleDefinition, new()
    {
        TModule Module = new();
        
        if (!_modulesPerPlatform.TryGetValue(InTargetPlatform, out Dictionary<string, ModuleDefinition>? ModuleMap))
        {
            ModuleMap = [];
            _modulesPerPlatform.Add(InTargetPlatform, ModuleMap);
        }

        ModuleMap.Add(Module.Name, Module);
    }

    internal void Configure(DirectoryReference InRootDirectory)
    {
        if (_bIsConfigured) return;

        _bIsConfigured = true;

        RootDirectory = InRootDirectory.Combine(Name);
        SourcesDirectory = RootDirectory.Combine(SourcesRoot);

        Configure();

        foreach (Dictionary<string, ModuleDefinition> Dict in _modulesPerPlatform.Values)
        {
            foreach (ModuleDefinition Module in Dict.Values)
            {
                Module.Configure(this, SourcesDirectory);
            }
        }
    }
}

public class ModuleAlreadyDefinedException(string InModuleName) 
    : BaseException($"Module '{InModuleName}' already exists in the project definition.");