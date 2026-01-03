using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using System;

namespace OnwardForceTubePistolPlugin
{
    // Custom ForceTube handler for pistols that uses pistol channels instead of rifle
    public class PistolForceTubeHandler : MonoBehaviour
    {
        private Pickup_Gun pistolGun;
        private bool isSubscribed = false;
        private System.Action firingCallback;

        // Il2Cpp constructor
        public PistolForceTubeHandler(IntPtr ptr) : base(ptr) { }

        private void OnEnable()
        {
            try
            {
                // Get reference to the Pickup_Gun component
                pistolGun = gameObject.GetComponent<Pickup_Gun>();
                if (pistolGun == null)
                {
                    Plugin.Log.LogError("PistolForceTubeHandler: No Pickup_Gun component found!");
                    return;
                }

                // Create callback and subscribe to firing event
                if (!isSubscribed)
                {
                    firingCallback = new System.Action(OnPistolFiring);
                    pistolGun.Firing += firingCallback;
                    isSubscribed = true;
                    Plugin.Log.LogInfo($"PistolForceTubeHandler: Subscribed to firing events for {gameObject.name}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"PistolForceTubeHandler OnEnable error: {ex}");
            }
        }

        private void OnDisable()
        {
            try
            {
                // Unsubscribe from firing event
                if (isSubscribed && pistolGun != null && firingCallback != null)
                {
                    pistolGun.Firing -= firingCallback;
                    isSubscribed = false;
                    Plugin.Log.LogInfo($"PistolForceTubeHandler: Unsubscribed from firing events");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"PistolForceTubeHandler OnDisable error: {ex}");
            }
        }

        private void OnPistolFiring()
        {
            try
            {
                // Send haptic feedback to PISTOL channel
                // ForceTubeVRChannel.pistol1 = 4
                byte pistolChannel = 4;

                // Strong kick power (similar to rifles)
                byte kickPower = 200;

                // Moderate rumble
                byte rumblePower = 150;

                // Short rumble duration
                float rumbleDuration = 0.05f;

                // Call the ForceTube Shot method with PISTOL channel
                ForceTubeVRInterface.Shoot(kickPower, rumblePower, rumbleDuration, (ForceTubeVRChannel)pistolChannel);

                Plugin.Log.LogInfo($"PistolForceTubeHandler: Sent haptic feedback on PISTOL channel");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"PistolForceTubeHandler firing callback error: {ex}");
            }
        }
    }
}
