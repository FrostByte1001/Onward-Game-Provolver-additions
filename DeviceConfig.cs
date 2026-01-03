using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OnwardForceTubePistolPlugin
{
    /// <summary>
    /// Configuration for ForceTube device channel assignments
    /// </summary>
    public class DeviceConfig
    {
        // General settings
        public ConfigEntry<bool> EnableAutoConfig;
        public ConfigEntry<bool> VerboseLogging;
        public ConfigEntry<bool> TestModeOnStartup;
        public ConfigEntry<float> TestRumbleDuration;

        // Device 1
        public ConfigEntry<string> Device1_ID;
        public ConfigEntry<string> Device1_Channels;
        public ConfigEntry<string> Device1_FriendlyName;

        // Device 2
        public ConfigEntry<string> Device2_ID;
        public ConfigEntry<string> Device2_Channels;
        public ConfigEntry<string> Device2_FriendlyName;

        // Device 3
        public ConfigEntry<string> Device3_ID;
        public ConfigEntry<string> Device3_Channels;
        public ConfigEntry<string> Device3_FriendlyName;

        public DeviceConfig(ConfigFile config)
        {
            // General settings
            EnableAutoConfig = config.Bind(
                "General",
                "EnableAutoConfig",
                true,
                "Enable automatic device detection and channel configuration"
            );

            VerboseLogging = config.Bind(
                "Advanced",
                "VerboseLogging",
                true,
                "Log detailed device detection and channel assignment information"
            );

            TestModeOnStartup = config.Bind(
                "Advanced",
                "TestModeOnStartup",
                false,
                "Test mode: Rumble each configured device on game startup to help identify them"
            );

            TestRumbleDuration = config.Bind(
                "Advanced",
                "TestRumbleDuration",
                2.0f,
                new ConfigDescription(
                    "How long to rumble each device in test mode (seconds)",
                    new AcceptableValueRange<float>(0.5f, 5.0f)
                )
            );

            // Device 1
            Device1_ID = config.Bind(
                "Device_1",
                "DeviceID",
                "",
                "Device ID (auto-detected from connected ForceTube devices). Leave empty to disable this slot."
            );

            Device1_Channels = config.Bind(
                "Device_1",
                "Channels",
                "2,3",
                new ConfigDescription(
                    "Channel assignment for this device (comma-separated channel numbers)\n" +
                    "Channel meanings:\n" +
                    "  2 = RifleButt   (rifle stock position)\n" +
                    "  3 = RifleBolt   (rifle bolt position)\n" +
                    "  4 = Pistol1     (left-hand pistol)\n" +
                    "  5 = Pistol2     (right-hand pistol)\n" +
                    "\n" +
                    "Common configurations:\n" +
                    "  2,3     = Rifle device (both rifle channels)\n" +
                    "  4,5     = Pistol device for both hands\n" +
                    "  4       = Left-hand pistol only\n" +
                    "  5       = Right-hand pistol only\n" +
                    "  2,3,4,5 = All weapons trigger this device\n" +
                    "  (empty) = Disabled"
                )
            );

            Device1_FriendlyName = config.Bind(
                "Device_1",
                "FriendlyName",
                "",
                "Friendly name for this device (optional, for your reference)"
            );

            // Device 2
            Device2_ID = config.Bind(
                "Device_2",
                "DeviceID",
                "",
                "Device ID (auto-detected from connected ForceTube devices). Leave empty to disable this slot."
            );

            Device2_Channels = config.Bind(
                "Device_2",
                "Channels",
                "4,5",
                "Channel assignment - see Device_1 for explanation"
            );

            Device2_FriendlyName = config.Bind(
                "Device_2",
                "FriendlyName",
                "",
                "Friendly name for this device (optional, for your reference)"
            );

            // Device 3
            Device3_ID = config.Bind(
                "Device_3",
                "DeviceID",
                "",
                "Device ID (auto-detected from connected ForceTube devices). Leave empty to disable this slot."
            );

            Device3_Channels = config.Bind(
                "Device_3",
                "Channels",
                "",
                "Channel assignment - see Device_1 for explanation"
            );

            Device3_FriendlyName = config.Bind(
                "Device_3",
                "FriendlyName",
                "",
                "Friendly name for this device (optional, for your reference)"
            );
        }

        /// <summary>
        /// Get all configured devices as a list
        /// </summary>
        public List<DeviceEntry> GetDevices()
        {
            var devices = new List<DeviceEntry>();

            if (!string.IsNullOrWhiteSpace(Device1_ID.Value))
            {
                devices.Add(new DeviceEntry
                {
                    DeviceID = Device1_ID.Value.Trim(),
                    Channels = ParseChannels(Device1_Channels.Value),
                    FriendlyName = Device1_FriendlyName.Value
                });
            }

            if (!string.IsNullOrWhiteSpace(Device2_ID.Value))
            {
                devices.Add(new DeviceEntry
                {
                    DeviceID = Device2_ID.Value.Trim(),
                    Channels = ParseChannels(Device2_Channels.Value),
                    FriendlyName = Device2_FriendlyName.Value
                });
            }

            if (!string.IsNullOrWhiteSpace(Device3_ID.Value))
            {
                devices.Add(new DeviceEntry
                {
                    DeviceID = Device3_ID.Value.Trim(),
                    Channels = ParseChannels(Device3_Channels.Value),
                    FriendlyName = Device3_FriendlyName.Value
                });
            }

            return devices;
        }

        /// <summary>
        /// Parse channel string like "2,3,4" into list of ints
        /// </summary>
        private List<int> ParseChannels(string channelString)
        {
            var channels = new List<int>();

            if (string.IsNullOrWhiteSpace(channelString))
                return channels;

            foreach (string part in channelString.Split(','))
            {
                if (int.TryParse(part.Trim(), out int channel))
                {
                    // Valid channels are 2-7 (rifleButt, rifleBolt, pistol1, pistol2, other, vest)
                    if (channel >= 2 && channel <= 7)
                    {
                        channels.Add(channel);
                    }
                }
            }

            return channels;
        }

        /// <summary>
        /// Update config with newly detected devices
        /// </summary>
        public void UpdateWithDetectedDevices(List<string> detectedDevices)
        {
            if (detectedDevices == null || detectedDevices.Count == 0)
                return;

            // Get list of already configured device IDs
            var configuredIDs = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(Device1_ID.Value)) configuredIDs.Add(Device1_ID.Value.Trim());
            if (!string.IsNullOrWhiteSpace(Device2_ID.Value)) configuredIDs.Add(Device2_ID.Value.Trim());
            if (!string.IsNullOrWhiteSpace(Device3_ID.Value)) configuredIDs.Add(Device3_ID.Value.Trim());

            // Find new devices not in config
            var newDevices = detectedDevices.Where(d => !configuredIDs.Contains(d)).ToList();

            if (newDevices.Count == 0)
                return;

            Plugin.Log.LogInfo($"Found {newDevices.Count} new device(s) not in config, adding...");

            // Add new devices to first empty slots
            int deviceIndex = 0;
            foreach (string newDevice in newDevices)
            {
                if (deviceIndex >= 3)
                {
                    Plugin.Log.LogWarning($"Cannot add device {newDevice} - all 3 config slots are full!");
                    break;
                }

                if (string.IsNullOrWhiteSpace(Device1_ID.Value))
                {
                    Device1_ID.Value = newDevice;
                    Plugin.Log.LogInfo($"Added {newDevice} to Device_1 config slot");
                    deviceIndex++;
                }
                else if (string.IsNullOrWhiteSpace(Device2_ID.Value))
                {
                    Device2_ID.Value = newDevice;
                    Plugin.Log.LogInfo($"Added {newDevice} to Device_2 config slot");
                    deviceIndex++;
                }
                else if (string.IsNullOrWhiteSpace(Device3_ID.Value))
                {
                    Device3_ID.Value = newDevice;
                    Device3_Channels.Value = ""; // Default to disabled for 3rd device
                    Plugin.Log.LogInfo($"Added {newDevice} to Device_3 config slot (DISABLED by default - edit config to enable)");
                    deviceIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Represents a configured device entry
    /// </summary>
    public class DeviceEntry
    {
        public string DeviceID { get; set; }
        public List<int> Channels { get; set; }
        public string FriendlyName { get; set; }

        public override string ToString()
        {
            string name = string.IsNullOrWhiteSpace(FriendlyName) ? DeviceID : $"{FriendlyName} ({DeviceID})";
            string channelStr = Channels.Count > 0 ? string.Join(",", Channels) : "disabled";
            return $"{name} -> Channels: {channelStr}";
        }
    }
}
