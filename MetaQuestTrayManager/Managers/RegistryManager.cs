using Microsoft.Win32;
using System;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Managers
{
    public enum RegistryKeyType
    {
        ClassRoot,
        CurrentUser,
        LocalMachine,
        Users,
        CurrentConfig
    }

    public static class RegistryManager
    {
        /// <summary>
        /// Gets a registry key based on the type and key location. Returns null if the key cannot be opened.
        /// </summary>
        public static RegistryKey? GetRegistryKey(RegistryKeyType type, string keyLocation)
        {
            try
            {
                return type switch
                {
                    RegistryKeyType.ClassRoot => Registry.ClassesRoot.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.CurrentUser => Registry.CurrentUser.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.LocalMachine => Registry.LocalMachine.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.Users => Registry.Users.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.CurrentConfig => Registry.CurrentConfig.OpenSubKey(keyLocation, writable: true),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to open registry key at {keyLocation}");
                return null;
            }
        }

        /// <summary>
        /// Sets a registry key value. Returns false if an error occurs.
        /// </summary>
        public static bool SetKeyValue(RegistryKey key, string keyName, object value, RegistryValueKind valueKind)
        {
            try
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                key.SetValue(keyName, value, valueKind);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to set {keyName} to {value}");
                return false;
            }
        }

        /// <summary>
        /// Gets the value of a registry key as a string. Returns null if the key or value does not exist.
        /// </summary>
        public static string? GetKeyValueString(RegistryKeyType type, string keyLocation, string keyName)
        {
            try
            {
                using var key = GetRegistryKey(type, keyLocation);
                return key?.GetValue(keyName)?.ToString();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to get value {keyName} from {keyLocation}");
                return null;
            }
        }

        /// <summary>
        /// Writes a value to the registry. Returns false if an error occurs.
        /// </summary>
        public static bool WriteRegistryValue(RegistryKeyType type, string keyPath, string valueName, object value, RegistryValueKind valueKind)
        {
            try
            {
                var baseKey = type switch
                {
                    RegistryKeyType.ClassRoot => Registry.ClassesRoot,
                    RegistryKeyType.CurrentUser => Registry.CurrentUser,
                    RegistryKeyType.LocalMachine => Registry.LocalMachine,
                    RegistryKeyType.Users => Registry.Users,
                    RegistryKeyType.CurrentConfig => Registry.CurrentConfig,
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };

                using var key = baseKey.CreateSubKey(keyPath, writable: true);
                if (key == null)
                {
                    throw new InvalidOperationException($"Unable to open or create registry key at {keyPath}");
                }

                key.SetValue(valueName, value, valueKind);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to write registry value {valueName} to {keyPath}");
                return false;
            }
        }

        /// <summary>
        /// Reads a registry key value as an object. Returns null if the key or value does not exist.
        /// </summary>
        public static object? ReadRegistryValue(RegistryKeyType type, string keyPath, string valueName)
        {
            try
            {
                using var key = GetRegistryKey(type, keyPath);
                return key?.GetValue(valueName);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to read value {valueName} from {keyPath}");
                return null;
            }
        }
    }
}
