namespace Nookly.Api.Authentication;

public static class EnvironmentLoader
{
    public static void Load(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        string? path = null;
        while (directory is not null && path is null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate)) path = candidate;
            directory = directory.Parent;
        }
        if (path is null) return;
        if (!File.Exists(path)) return;
        foreach (var line in File.ReadLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
            var separator = trimmed.IndexOf('=');
            if (separator < 1) continue;
            var key = trimmed[..separator];
            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, trimmed[(separator + 1)..]);
            }
        }
    }
}
