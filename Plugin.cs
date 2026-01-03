using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Onward.Weapons;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OnwardForceTubePistolPlugin
{
    [BepInPlugin("com.forcetube.onward.pistol", "Onward ForceTube Pistol Support", "2.0.0")]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;
        internal static DeviceConfig DeviceConfiguration;
        private static bool channelsConfigured = false;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo("=== Onward ForceTube Pistol Mod Loading ===");

            // Initialize configuration
            DeviceConfiguration = new DeviceConfig(Config);
            Log.LogInfo($"Configuration loaded from: {Config.ConfigFilePath}");

            // Register our custom PistolForceTubeHandler with Il2Cpp
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<PistolForceTubeHandler>();
                Log.LogInfo("Registered PistolForceTubeHandler with Il2Cpp");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to register PistolForceTubeHandler: {ex}");
            }

            // Apply Harmony patches
            try
            {
                var harmony = new Harmony("com.forcetube.onward.pistol");
                harmony.PatchAll();
                Log.LogInfo("Harmony patches applied successfully!");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to apply Harmony patches: {ex}");
            }

            Log.LogInfo("=== ForceTube Pistol Mod Loaded ===");
            Log.LogInfo("Edit config at: BepInEx/config/com.forcetube.onward.pistol.cfg");
        }

        /// <summary>
        /// Configure ForceTube channels based on user configuration
        /// </summary>
        internal static void ConfigureChannels()
        {
            if (channelsConfigured)
            {
                if (DeviceConfiguration.VerboseLogging.Value)
                    Log.LogInfo("Channels already configured, skipping");
                return;
            }

            try
            {
                Log.LogInfo("=== Configuring ForceTube Channels ===");

                // Check if auto-config is enabled
                if (!DeviceConfiguration.EnableAutoConfig.Value)
                {
                    Log.LogInfo("Auto-config disabled in settings, skipping channel configuration");
                    Log.LogInfo("Channels will use game's default initialization");
                    channelsConfigured = true;
                    return;
                }

                // Initialize ForceTube
                Log.LogInfo("Initializing ForceTube connection...");
                ForceTubeVRInterface.InitAsync(pistolsFirst: false);

                // Small delay to let devices connect
                System.Threading.Thread.Sleep(500);

                // Get list of connected devices
                string devicesJson = ForceTubeExtendedAPI.GetConnectedDevices();

                if (DeviceConfiguration.VerboseLogging.Value)
                    Log.LogInfo($"Connected devices JSON: {devicesJson}");

                if (string.IsNullOrEmpty(devicesJson))
                {
                    Log.LogWarning("No devices found or failed to get device list");
                    channelsConfigured = true;
                    return;
                }

                // Parse device names from JSON
                List<string> detectedDevices = ParseDeviceNames(devicesJson);
                Log.LogInfo($"Detected {detectedDevices.Count} ForceTube device(s): {string.Join(", ", detectedDevices)}");

                if (detectedDevices.Count == 0)
                {
                    Log.LogWarning("No devices to configure");
                    channelsConfigured = true;
                    return;
                }

                // Update config with any newly detected devices
                DeviceConfiguration.UpdateWithDetectedDevices(detectedDevices);

                // Get configured devices from config
                List<DeviceEntry> configuredDevices = DeviceConfiguration.GetDevices();

                if (configuredDevices.Count == 0)
                {
                    Log.LogWarning("No devices configured in config file!");
                    Log.LogInfo("Edit BepInEx/config/com.forcetube.onward.pistol.cfg to configure devices");
                    channelsConfigured = true;
                    return;
                }

                Log.LogInfo($"Configuring {configuredDevices.Count} device(s) from config:");
                foreach (var device in configuredDevices)
                {
                    Log.LogInfo($"  - {device}");
                }

                // Clear all channels first
                if (DeviceConfiguration.VerboseLogging.Value)
                    Log.LogInfo("Clearing all channels...");

                for (int channel = 2; channel <= 7; channel++)
                {
                    ForceTubeExtendedAPI.ClearChannelDevices(channel);
                }

                // Apply channel assignments from config
                foreach (var device in configuredDevices)
                {
                    if (device.Channels.Count == 0)
                    {
                        Log.LogInfo($"Device {device.DeviceID} has no channels assigned (disabled)");
                        continue;
                    }

                    foreach (int channel in device.Channels)
                    {
                        ForceTubeExtendedAPI.AddDeviceToChannel(channel, device.DeviceID);
                    }
                }

                // Log final channel configuration
                if (DeviceConfiguration.VerboseLogging.Value)
                {
                    string channelsJson = ForceTubeExtendedAPI.GetChannelAssignments();
                    Log.LogInfo($"Final channel assignments: {channelsJson}");
                }

                // Run test mode if enabled
                if (DeviceConfiguration.TestModeOnStartup.Value)
                {
                    RunTestMode(configuredDevices);
                }

                channelsConfigured = true;
                Log.LogInfo("=== ForceTube Channels Configured Successfully ===");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to configure channels: {ex}");
            }
        }

        /// <summary>
        /// Test mode: Rumble each configured device to help identify them
        /// </summary>
        private static void RunTestMode(List<DeviceEntry> devices)
        {
            Log.LogInfo("=== TEST MODE: Identifying Devices ===");

            byte rumblePower = 255; // Full power
            float rumbleDuration = DeviceConfiguration.TestRumbleDuration.Value;

            foreach (var device in devices)
            {
                if (device.Channels.Count == 0)
                    continue;

                string name = string.IsNullOrWhiteSpace(device.FriendlyName)
                    ? device.DeviceID
                    : $"{device.FriendlyName} ({device.DeviceID})";

                Log.LogInfo($"TESTING: {name} - Channels: {string.Join(",", device.Channels)} - RUMBLING NOW for {rumbleDuration}s");

                // Rumble on first assigned channel
                int testChannel = device.Channels[0];
                ForceTubeVRInterface.Rumble(rumblePower, rumbleDuration, (ForceTubeVRChannel)testChannel);

                // Wait for rumble to finish plus a bit of gap
                System.Threading.Thread.Sleep((int)((rumbleDuration + 0.5f) * 1000));
            }

            Log.LogInfo("=== TEST MODE: Complete ===");
        }

        /// <summary>
        /// Parse device names from JSON response
        /// Expected format: {"Connected": ["ForceTubeVR XXXXXXXXXXXXX", "ForceTubeVR YYYYYYYYYYYYY"]}
        /// </summary>
        private static List<string> ParseDeviceNames(string json)
        {
            List<string> names = new List<string>();

            try
            {
                // Simple regex to extract device names from JSON array
                // Matches strings like "ForceTubeVR XXXXXXXXXXXXX"
                var matches = Regex.Matches(json, @"""(ForceTubeVR [^""]+)""");
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        names.Add(match.Groups[1].Value);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to parse device names: {ex}");
            }

            return names;
        }
    }

    // Patch EquipLocal to configure channels and add PistolForceTubeHandler
    [HarmonyPatch(typeof(Pickup_Gun), "EquipLocal")]
    public class Pickup_Gun_EquipLocal_Patch
    {
        static void Postfix(Pickup_Gun __instance, HandController handController)
        {
            try
            {
                // Configure channels on first weapon equip
                Plugin.ConfigureChannels();

                // Check if WeaponSO is available
                if (__instance.WeaponSO == null)
                {
                    Plugin.Log.LogWarning($"EquipLocal: {__instance.gameObject.name} has no WeaponSO!");
                    return;
                }

                Plugin.Log.LogInfo($"EquipLocal: {__instance.gameObject.name}, WeaponType: {__instance.WeaponSO.WeaponType}");

                // Add PistolForceTubeHandler to pistols
                if (__instance.WeaponSO.WeaponType == WeaponType.Pistol)
                {
                    // Check if PistolForceTubeHandler already exists
                    var existing = __instance.GetComponent<PistolForceTubeHandler>();
                    if (existing == null)
                    {
                        // Add PistolForceTubeHandler component
                        var handler = __instance.gameObject.AddComponent<PistolForceTubeHandler>();
                        if (handler != null)
                        {
                            Plugin.Log.LogInfo($"Added PistolForceTubeHandler to: {__instance.gameObject.name}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error in EquipLocal patch: {ex}");
            }
        }
    }
}
