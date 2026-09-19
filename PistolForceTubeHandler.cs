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
                // Route to the correct pistol channel based on which hand holds this gun,
                // so dual-wielded pistols can drive separate physical devices.
                // Convention (matches the Crisis VRigade 2 ForceTube mod):
                //   ForceTubeVRChannel.pistol1 = 4  -> RIGHT hand
                //   ForceTubeVRChannel.pistol2 = 5  -> LEFT hand
                // Default to 4 (right) if the hand can't be resolved, so behaviour never
                // regresses below "something fires".
                byte pistolChannel = 4;

                var handController = pistolGun.HandHeldIn; // inherited from base Pickup
                if (handController != null && handController.HandType == InputGlobal.Hand.Left)
                {
                    pistolChannel = 5; // pistol2 = left hand
                }

                // Strong kick power (similar to rifles)
                byte kickPower = 200;

                // Moderate rumble
                byte rumblePower = 150;

                // Short rumble duration
                float rumbleDuration = 0.05f;

                // Call the ForceTube Shot method with PISTOL channel
                ForceTubeVRInterface.Shoot(kickPower, rumblePower, rumbleDuration, (ForceTubeVRChannel)pistolChannel);

                if (Plugin.DeviceConfiguration != null && Plugin.DeviceConfiguration.VerboseLogging.Value)
                {
                    string handName = handController == null
                        ? "unknown (defaulted RIGHT)"
                        : handController.HandType.ToString();
                    Plugin.Log.LogInfo($"PistolForceTubeHandler: Pistol fired by {handName} hand -> channel {pistolChannel}");
                }
                else
                {
                    Plugin.Log.LogInfo($"PistolForceTubeHandler: Sent haptic feedback on PISTOL channel {pistolChannel}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"PistolForceTubeHandler firing callback error: {ex}");
            }
        }
    }
}
