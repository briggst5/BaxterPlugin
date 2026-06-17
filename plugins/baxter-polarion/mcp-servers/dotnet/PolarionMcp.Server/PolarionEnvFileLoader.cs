namespace PolarionMcp.Server;

internal static class PolarionEnvFileLoader
{
    internal static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config",
        "polarion-mcp.env"
    );

    public static void LoadEnvironmentVariables()
    {
        if (!File.Exists(ConfigPath))
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(ConfigPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (key.Length == 0)
            {
                continue;
            }

            // Keep deterministic behavior: env file values override process env.
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
