using System;
using System.Collections.Generic;
using System.Management;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Managers
{
    /// <summary>
    /// Provides utility functions for managing and identifying connected USB devices.
    /// </summary>
    public static class USB_Devices_Functions
    {
        /// <summary>
        /// Retrieves a list of USB devices connected to the system.
        /// </summary>
        /// <returns>A list of USBDeviceInfo objects representing the connected USB devices.</returns>
        public static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> PluggedInDevices = new List<USBDeviceInfo>();

            try
            {
                // Querying the system for USB devices using WMI
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity WHERE DeviceID LIKE '%VID_2833%'"))
                {
                    var devices = searcher.Get();
                    PluggedInDevices.AddRange(ProcessDevices(devices));
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error retrieving USB devices via WMI.");
            }

            return PluggedInDevices;
        }

        /// <summary>
        /// Processes the ManagementObjectCollection to extract USB device information.
        /// </summary>
        /// <param name="devices">The collection of ManagementObjects representing USB devices.</param>
        /// <returns>A list of USBDeviceInfo objects.</returns>
        private static List<USBDeviceInfo> ProcessDevices(ManagementObjectCollection devices)
        {
            // Dictionary to map device IDs to human-readable names
            Dictionary<string, string> DeviceIDs = new Dictionary<string, string>
            {
                { "VID_2833&PID_2031", "Rift CV1" },
                { "VID_2833&PID_4001", "Quest 3" },
                { "VID_2833&PID_4002", "Quest 3S" },
                { "VID_2833&PID_5001", "Quest Pro" }
                // Add other devices here...
            };

            List<USBDeviceInfo> PluggedInDevices = new List<USBDeviceInfo>();

            foreach (ManagementObject device in devices)
            {
                try
                {
                    // Inline fallback to ensure no null conversion warnings
                    string deviceID = TryGetProperty(device, "DeviceID") ?? "Unknown";
                    string deviceCaption = TryGetProperty(device, "Caption") ?? "Unknown";

                    string[] data = deviceID.Split('\\');
                    string type = "Unknown";
                    string serial = string.Empty;
                    string maskedSerial = string.Empty;

                    if (data.Length == 3)
                    {
                        serial = data[2];
                        if (serial.Contains("&"))
                            serial = string.Empty;

                        if (!DeviceIDs.TryGetValue(data[1], out type))
                            type = $"Unknown - {data[1]}";

                        if (deviceCaption.StartsWith("USB Comp"))
                            deviceCaption = type;

                        if (!string.IsNullOrEmpty(deviceCaption))
                            type = deviceCaption;

                        maskedSerial = serial.Length > 5
                            ? new string('*', serial.Length - 5) + serial[^5..]
                            : serial;

                        PluggedInDevices.Add(new USBDeviceInfo(deviceID, type, maskedSerial, serial));
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, "Error processing USB device.");
                }
            }

            return PluggedInDevices;
        }

        /// <summary>
        /// Attempts to retrieve a property value from a ManagementObject.
        /// </summary>
        /// <param name="wmiObj">The ManagementObject.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property, or "Unknown" if null or an error occurs.</returns>
        private static string TryGetProperty(ManagementObject wmiObj, string propertyName)
        {
            try
            {
                // Ensure we explicitly convert to string and handle null at this level
                object propertyValue = wmiObj.GetPropertyValue(propertyName);
                return propertyValue?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                // Log the error and return a safe fallback value
                ErrorLogger.LogError(ex, $"Failed to retrieve property: {propertyName}");
                return "Unknown";
            }
        }
    }

    /// <summary>
    /// Represents information about a USB device.
    /// </summary>
    public class USBDeviceInfo
    {
        /// <summary>
        /// Initializes a new instance of the USBDeviceInfo class.
        /// </summary>
        /// <param name="deviceID">The device ID.</param>
        /// <param name="type">The type of device.</param>
        /// <param name="maskedSerial">The masked serial number.</param>
        /// <param name="fullSerial">The full serial number.</param>
        public USBDeviceInfo(string deviceID, string type, string maskedSerial, string fullSerial)
        {
            DeviceID = deviceID;
            Type = type;
            MaskedSerial = maskedSerial;
            FullSerial = fullSerial;
        }

        public string DeviceID { get; }
        public string Type { get; }
        public string MaskedSerial { get; }
        public string FullSerial { get; }
    }
}
