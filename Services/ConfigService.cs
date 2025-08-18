using BepInEx.Configuration;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SoVUtilities.Services;

internal static class ConfigService
{
    static readonly Lazy<bool> _DisableBatForm = new(() => GetConfigValue<bool>("DisableBatForm"));
    public static bool DisableBatForm => _DisableBatForm.Value;

    public static class ConfigInitialization
    {
        static readonly Regex _regex = new(@"^\[(.+)\]$");

        public static readonly Dictionary<string, object> FinalConfigValues = [];

        static readonly Lazy<List<string>> _directoryPaths = new(() =>
        {
            return
            [
            Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME),
            ];
        });
        public static List<string> DirectoryPaths => _directoryPaths.Value;

        public static readonly List<string> SectionOrder =
        [
            "General"
        ];
        public class ConfigEntryDefinition(string section, string key, object defaultValue, string description)
        {
            public string Section { get; } = section;
            public string Key { get; } = key;
            public object DefaultValue { get; } = defaultValue;
            public string Description { get; } = description;
        }
        public static readonly List<ConfigEntryDefinition> ConfigEntries =
        [
            new ConfigEntryDefinition("General", "DisableBatForm", true, "Disable Bat Vampire unlock for players, useful for servers that don't want to allow it."),
        ];
        public static void InitializeConfig()
        {
            foreach (string path in DirectoryPaths)
            {
                CreateDirectory(path);
            }

            foreach (ConfigEntryDefinition entry in ConfigEntries)
            {
                // Get the type of DefaultValue
                Type entryType = entry.DefaultValue.GetType();

                // Reflect on the nested ConfigInitialization class within ConfigService
                Type nestedClassType = typeof(ConfigService).GetNestedType("ConfigInitialization", BindingFlags.Static | BindingFlags.Public);

                // Use reflection to call InitConfigEntry with the appropriate type
                MethodInfo method = nestedClassType.GetMethod("InitConfigEntry", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo generic = method.MakeGenericMethod(entryType);

                var configEntry = generic.Invoke(null, [entry.Section, entry.Key, entry.DefaultValue, entry.Description]);
                UpdateConfigProperty(entry.Key, configEntry);

                object valueProp = configEntry.GetType().GetProperty("Value")?.GetValue(configEntry);
                if (valueProp != null)
                {
                    FinalConfigValues[entry.Key] = valueProp;
                }
                else
                {
                    Core.Log.LogError($"Failed to get value property for {entry.Key}");
                }

            }

            var configFile = Path.Combine(BepInEx.Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");
            if (File.Exists(configFile)) OrganizeConfig(configFile);
        }
        static void UpdateConfigProperty(string key, object configEntry)
        {
            PropertyInfo propertyInfo = typeof(ConfigService).GetProperty(key, BindingFlags.Static | BindingFlags.Public);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                object value = configEntry.GetType().GetProperty("Value")?.GetValue(configEntry);

                if (value != null)
                {
                    propertyInfo.SetValue(null, Convert.ChangeType(value, propertyInfo.PropertyType));
                }
                else
                {
                    throw new Exception($"Value property on configEntry is null for key {key}.");
                }
            }
        }
        static ConfigEntry<T> InitConfigEntry<T>(string section, string key, T defaultValue, string description)
        {
            // Bind the configuration entry with the default value in the new section
            var entry = Plugin.Instance.Config.Bind(section, key, defaultValue, description);

            // Define the path to the configuration file
            var configFile = Path.Combine(BepInEx.Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");

            // Ensure the configuration file is only loaded if it exists
            if (File.Exists(configFile))
            {
                string[] configLines = File.ReadAllLines(configFile);
                //Plugin.LogInstance.LogInfo(configLines);
                foreach (var line in configLines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    {
                        continue;
                    }

                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var configKey = keyValue[0].Trim();
                        var configValue = keyValue[1].Trim();

                        // Check if the key matches the provided key
                        if (configKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            // Try to convert the string value to the expected type
                            try
                            {
                                object convertedValue;

                                Type t = typeof(T);

                                if (t == typeof(float))
                                {
                                    convertedValue = float.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(double))
                                {
                                    convertedValue = double.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(decimal))
                                {
                                    convertedValue = decimal.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(int))
                                {
                                    convertedValue = int.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(uint))
                                {
                                    convertedValue = uint.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(long))
                                {
                                    convertedValue = long.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(ulong))
                                {
                                    convertedValue = ulong.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(short))
                                {
                                    convertedValue = short.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(ushort))
                                {
                                    convertedValue = ushort.Parse(configValue, CultureInfo.InvariantCulture);
                                }
                                else if (t == typeof(bool))
                                {
                                    convertedValue = bool.Parse(configValue);
                                }
                                else if (t == typeof(string))
                                {
                                    convertedValue = configValue;
                                }
                                else
                                {
                                    // Handle other types or throw an exception
                                    throw new NotSupportedException($"Type {t} is not supported");
                                }

                                entry.Value = (T)convertedValue;
                            }
                            catch (Exception ex)
                            {
                                Plugin.LogInstance.LogError($"Failed to convert config value for {key}: {ex.Message}");
                            }

                            break;
                        }
                    }
                }
            }

            return entry;
        }
        static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        const string DEFAULT_VALUE_LINE = "# Default value: ";
        static void OrganizeConfig(string configFile)
        {
            try
            {
                Dictionary<string, List<string>> OrderedSections = [];
                string currentSection = "";

                string[] lines = File.ReadAllLines(configFile);
                string[] fileHeader = lines[0..3];

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    var match = _regex.Match(trimmedLine);

                    if (match.Success)
                    {
                        currentSection = match.Groups[1].Value;
                        if (!OrderedSections.ContainsKey(currentSection))
                        {
                            OrderedSections[currentSection] = [];
                        }
                    }
                    else if (SectionOrder.Contains(currentSection))
                    {
                        OrderedSections[currentSection].Add(trimmedLine);
                    }
                }

                using StreamWriter writer = new(configFile, false);

                foreach (var header in fileHeader)
                {
                    writer.WriteLine(header);
                }

                foreach (var section in SectionOrder)
                {
                    if (OrderedSections.ContainsKey(section))
                    {
                        writer.WriteLine($"[{section}]");

                        List<string> sectionLines = OrderedSections[section];

                        // Extract keys from the config file
                        List<string> sectionKeys = [..sectionLines
                            .Where(line => line.Contains('='))
                            .Select(line => line.Split('=')[0].Trim())];

                        // Create a dictionary of default values directly from ConfigEntries
                        Dictionary<string, object> defaultValuesMap = ConfigEntries
                            .Where(entry => entry.Section == section)
                            .ToDictionary(entry => entry.Key, entry => entry.DefaultValue);

                        int keyIndex = 0;
                        bool previousLineSkipped = false; // Track whether the last line was skipped

                        foreach (var line in sectionLines)
                        {
                            if (line.Contains('='))
                            {
                                string key = line.Split('=')[0].Trim();

                                // Skip obsolete keys that are not in ConfigEntries
                                if (!defaultValuesMap.ContainsKey(key))
                                {
                                    Core.Log.LogWarning($"Skipping obsolete config entry: {key}");
                                    previousLineSkipped = true;
                                    continue;
                                }
                            }

                            // Prevent consecutive blank lines by skipping extra ones
                            if (string.IsNullOrWhiteSpace(line) && previousLineSkipped)
                            {
                                continue;
                            }

                            if (line.Contains(DEFAULT_VALUE_LINE))
                            {
                                ConfigEntryDefinition entry = ConfigEntries.FirstOrDefault(e => e.Key == sectionKeys[keyIndex] && e.Section == section);

                                if (entry != null)
                                {
                                    writer.WriteLine(DEFAULT_VALUE_LINE + entry.DefaultValue.ToString());
                                    keyIndex++;
                                }
                                else
                                {
                                    Core.Log.LogWarning($"Config entry for key '{sectionKeys[keyIndex]}' not found in ConfigEntries!");
                                    writer.WriteLine(line);
                                }
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }

                            previousLineSkipped = false; // Reset flag since we wrote a valid line
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.LogError($"Failed to clean and organize config file: {ex.Message}");
            }
        }
    }
    static T GetConfigValue<T>(string key)
    {
        if (ConfigInitialization.FinalConfigValues.TryGetValue(key, out var val))
        {
            return (T)Convert.ChangeType(val, typeof(T));
        }

        var entry = ConfigInitialization.ConfigEntries.FirstOrDefault(e => e.Key == key);
        return entry == null ? throw new InvalidOperationException($"Config entry for key '{key}' not found.") : (T)entry.DefaultValue;
    }
}
