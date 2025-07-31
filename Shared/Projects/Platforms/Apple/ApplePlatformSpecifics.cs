namespace Shared.Projects.Platforms.Apple;

public class ApplePlatformSpecifics
{
    private readonly HashSet<string> _frameworkDependencies = ["CoreFoundation", "Foundation"];

    public IReadOnlySet<string> FrameworkDependencies => _frameworkDependencies;

    public void AddFrameworkDependencies(params string[] InFrameworkNames)
    {
        foreach (string FrameworkName in InFrameworkNames)
        {
            _frameworkDependencies.Add(FrameworkName);
        }
    }
}