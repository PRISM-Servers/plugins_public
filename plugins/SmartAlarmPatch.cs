// Reference: 0Harmony
using CompanionServer;
using HarmonyLib;
using Oxide.Core;
using System.Reflection;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SmartAlarmPatch", "MBR", "1.0.3", ResourceId = 0)]
    class SmartAlarmPatch : RustPlugin
    {
        private Harmony _harmony = new Harmony(nameof(SmartAlarmPatch));

        #region Hooks
        private void OnServerInitialized()
        {
            if (_harmony != null) _harmony.PatchAll();
        }

        private void Unload()
        {
            if (_harmony != null) _harmony.UnpatchAll(nameof(SmartAlarmPatch));
        }
        #endregion

        #region Patch
        [HarmonyPatch(typeof(SmartAlarm), "IOStateChanged", new[] { typeof(int), typeof(int) })]
        private class HookPatch
        {
            private static bool Prefix(int inputAmount, int inputSlot, SmartAlarm __instance)
            {
                __instance.Value = inputAmount > 0;
                if (__instance.Value == __instance.IsOn()) return false;

                __instance.SetFlag(BaseEntity.Flags.On, __instance.Value, false, true);

                //__instance.BroadcastValueChange(); srsly fp
                MethodInfo BroadcastValueChange = typeof(AppIOEntity).GetMethod("BroadcastValueChange", BindingFlags.Instance | BindingFlags.NonPublic);
                BroadcastValueChange.Invoke(__instance, BindingFlags.Instance | BindingFlags.NonPublic, null, null, null);

                if (__instance.Value && Time.realtimeSinceStartup - __instance._lastSentTime >= Mathf.Max(ConVar.App.alarmcooldown, 15f))
                {
                    BuildingPrivlidge buildingPrivilege = __instance.GetBuildingPrivilege();
                    if (buildingPrivilege != null)
                    {
                        __instance._subscriptions.IntersectWith(buildingPrivilege.authorizedPlayers);
                    }

                    if (Interface.Oxide.CallHook("CanSmartAlarmSend", __instance, __instance._subscriptions) != null) return false;

                    __instance._subscriptions.SendNotification(NotificationChannel.SmartAlarm, __instance._notificationTitle, __instance._notificationBody, string.Empty);
                    __instance._lastSentTime = Time.realtimeSinceStartup;
                }

                return false;
            }
        }
        #endregion
    }
}