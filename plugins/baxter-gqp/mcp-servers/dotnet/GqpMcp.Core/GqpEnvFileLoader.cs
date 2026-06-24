namespace GqpMcp.Core;

public static class GqpEnvFileLoader
{
    public static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config",
        "gqp-mcp.env");

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

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    public static bool ConfigFileExists() => File.Exists(ConfigPath);
}
