using System;
using System.Runtime.InteropServices;

namespace OnwardForceTubePistolPlugin
{
    /// <summary>
    /// Extended ForceTube API functions from the newer DLL
    /// These functions exist in the updated ForceTubeVR_API_x64.dll
    /// </summary>
    public static class ForceTubeExtendedAPI
    {
        private const string DllName = "ForceTubeVR_API_x64";

        // List all connected ForceTube devices as JSON string
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ListConnectedForceTube();

        // List all channels with their assigned devices as JSON string
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ListChannels();

        // Add a device to a specific channel
        // channel: 2=rifleButt, 3=rifleBolt, 4=pistol1, 5=pistol2, 6=other, 7=vest
        // deviceName: Device name from ListConnectedForceTube (e.g., "ForceTubeVR XXXXXXXXXXXXX")
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool AddToChannel(int channel, [MarshalAs(UnmanagedType.LPStr)] string deviceName);

        // Remove a device from a specific channel
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RemoveFromChannel(int channel, [MarshalAs(UnmanagedType.LPStr)] string deviceName);

        // Clear all devices from a channel
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ClearChannel(int channel);

        /// <summary>
        /// Get JSON string of all connected ForceTube devices
        /// Returns: {"Connected": ["ForceTubeVR XXXXXXXXXXXXX", "ForceTubeVR YYYYYYYYYYYYY"]}
        /// </summary>
        public static string GetConnectedDevices()
        {
            try
            {
                IntPtr ptr = ListConnectedForceTube();
                if (ptr == IntPtr.Zero)
                {
                    return null;
                }
                return Marshal.PtrToStringAnsi(ptr);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"GetConnectedDevices error: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Get JSON string of all channels and their device assignments
        /// </summary>
        public static string GetChannelAssignments()
        {
            try
            {
                IntPtr ptr = ListChannels();
                if (ptr == IntPtr.Zero)
                {
                    return null;
                }
                return Marshal.PtrToStringAnsi(ptr);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"GetChannelAssignments error: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Add a device to a specific channel
        /// </summary>
        public static bool AddDeviceToChannel(int channel, string deviceName)
        {
            try
            {
                bool result = AddToChannel(channel, deviceName);
                Plugin.Log.LogInfo($"AddToChannel({channel}, {deviceName}): {result}");
                return result;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"AddDeviceToChannel error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Remove a device from a specific channel
        /// </summary>
        public static bool RemoveDeviceFromChannel(int channel, string deviceName)
        {
            try
            {
                bool result = RemoveFromChannel(channel, deviceName);
                Plugin.Log.LogInfo($"RemoveFromChannel({channel}, {deviceName}): {result}");
                return result;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"RemoveDeviceFromChannel error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Clear all devices from a channel
        /// </summary>
        public static void ClearChannelDevices(int channel)
        {
            try
            {
                ClearChannel(channel);
                Plugin.Log.LogInfo($"ClearChannel({channel})");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"ClearChannelDevices error: {ex}");
            }
        }
    }
}
