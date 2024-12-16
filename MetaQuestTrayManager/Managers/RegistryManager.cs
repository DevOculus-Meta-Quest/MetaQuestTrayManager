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
        public static RegistryKey GetRegistryKey(RegistryKeyType type, string keyLocation)
        {
            try
            {
                var registryKey = type switch
                {
                    RegistryKeyType.ClassRoot => Registry.ClassesRoot.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.CurrentUser => Registry.CurrentUser.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.LocalMachine => Registry.LocalMachine.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.Users => Registry.Users.OpenSubKey(keyLocation, writable: true),
                    RegistryKeyType.CurrentConfig => Registry.CurrentConfig.OpenSubKey(keyLocation, writable: true),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };

                return registryKey;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to open registry key at {keyLocation}");
                return null;
            }
        }

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

        public static string GetKeyValueString(RegistryKeyType type, string keyLocation, string keyName)
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
    }
}
