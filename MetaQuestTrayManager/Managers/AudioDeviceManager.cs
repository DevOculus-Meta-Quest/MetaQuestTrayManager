using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;

namespace MetaQuestTrayManager.Managers
{
    public static class AudioDeviceManager
    {
        /// <summary>
        /// Retrieves a list of playback devices (output).
        /// </summary>
        public static List<AudioDeviceInfo> GetPlaybackDevices()
        {
            var devices = new List<AudioDeviceInfo>();
            using var enumerator = new MMDeviceEnumerator();

            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                devices.Add(new AudioDeviceInfo
                {
                    DeviceId = device.ID ?? string.Empty, // Handle possible null
                    DeviceName = device.FriendlyName ?? "Unknown Device" // Handle possible null
                });
            }

            return devices;
        }

        /// <summary>
        /// Retrieves a list of recording devices (input).
        /// </summary>
        public static List<AudioDeviceInfo> GetRecordingDevices()
        {
            var devices = new List<AudioDeviceInfo>();
            using var enumerator = new MMDeviceEnumerator();

            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                devices.Add(new AudioDeviceInfo
                {
                    DeviceId = device.ID ?? string.Empty, // Handle possible null
                    DeviceName = device.FriendlyName ?? "Unknown Device" // Handle possible null
                });
            }

            return devices;
        }

        /// <summary>
        /// Gets the default playback device.
        /// </summary>
        public static AudioDeviceInfo? GetDefaultPlaybackDevice()
        {
            using var enumerator = new MMDeviceEnumerator();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return defaultDevice != null
                ? new AudioDeviceInfo
                {
                    DeviceId = defaultDevice.ID ?? string.Empty,
                    DeviceName = defaultDevice.FriendlyName ?? "Unknown Device"
                }
                : null; // Handle null case
        }

        /// <summary>
        /// Gets the default recording device.
        /// </summary>
        public static AudioDeviceInfo? GetDefaultRecordingDevice()
        {
            using var enumerator = new MMDeviceEnumerator();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            return defaultDevice != null
                ? new AudioDeviceInfo
                {
                    DeviceId = defaultDevice.ID ?? string.Empty,
                    DeviceName = defaultDevice.FriendlyName ?? "Unknown Device"
                }
                : null; // Handle null case
        }
    }

    public class AudioDeviceInfo
    {
        public string DeviceId { get; set; } = string.Empty; // Default to avoid CS8618
        public string DeviceName { get; set; } = "Unknown Device"; // Default to avoid CS8618
    }
}
