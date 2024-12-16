using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MetaQuestTrayManager.Utils
{
    /// <summary>
    /// Parses VDF (Valve Data Format) files into usable objects.
    /// </summary>
    public class VdfParser
    {
        /// <summary>
        /// Parses a VDF file and returns its structured data.
        /// </summary>
        public Dictionary<string, object> ParseVdf(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    return ReadNextObject(br);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Error parsing VDF file: {filePath}");
                return null;
            }
        }

        /// <summary>
        /// Reads the next VDF object (map, string, or integer).
        /// </summary>
        private Dictionary<string, object> ReadNextObject(BinaryReader br)
        {
            var result = new Dictionary<string, object>();

            while (true)
            {
                var type = br.ReadByte();

                switch (type)
                {
                    case 0x00: // Map
                        var mapKey = ReadString(br);
                        result[mapKey] = ReadNextObject(br);
                        break;

                    case 0x01: // String
                        var stringKey = ReadString(br);
                        result[stringKey] = ReadString(br);
                        break;

                    case 0x02: // Integer
                        var intKey = ReadString(br);
                        result[intKey] = br.ReadInt32();
                        break;

                    case 0x08: // End of a map
                        return result;

                    default:
                        throw new Exception($"Unknown type encountered in VDF file: {type}");
                }
            }
        }

        /// <summary>
        /// Reads a null-terminated UTF8 string.
        /// </summary>
        private string ReadString(BinaryReader br)
        {
            var bytes = new List<byte>();

            while (true)
            {
                var b = br.ReadByte();
                if (b == 0) break;
                bytes.Add(b);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Extracts specific shortcut data from VDF structured data.
        /// </summary>
        public List<ShortcutInfo> ExtractSpecificData(Dictionary<string, object> vdfData)
        {
            var shortcuts = new List<ShortcutInfo>();

            foreach (var entry in vdfData)
            {
                if (entry.Value is Dictionary<string, object> shortcutData)
                {
                    var info = new ShortcutInfo
                    {
                        AppName = shortcutData.TryGetValue("AppName", out var appName) ? appName.ToString() : "Unknown",
                        Exe = shortcutData.TryGetValue("Exe", out var exe) ? exe.ToString() : "Unknown"
                    };

                    shortcuts.Add(info);
                }
            }

            return shortcuts;
        }
    }

    /// <summary>
    /// Represents shortcut information extracted from VDF.
    /// </summary>
    public class ShortcutInfo
    {
        public string AppName { get; set; }
        public string Exe { get; set; }
    }
}
