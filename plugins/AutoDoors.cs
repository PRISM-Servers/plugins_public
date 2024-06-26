using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("AutoDoors", "Wulf (edited by Shady)", "2.4.2", ResourceId = 800)]
    [Description("Automatically closes doors behind players after X seconds.")]
    internal class AutoDoors : RustPlugin
    {
        // Do NOT edit this file, instead edit AutoDoors.json in oxide/config and AutoDoors.en.json in oxide/lang,
        // or create a language file for another language using the 'en' file as a default.

        private readonly Dictionary<Door, Action> _doorActions = new Dictionary<Door, Action>();

        #region Localization

        protected override void LoadDefaultMessages()
        {
            var messages = new Dictionary<string, string>
            {
                {"ChatCommand", "ad"},
                {"CommandUsage", "Usage:\n /ad to disable automatic doors\n /ad # (a number between 5 and 30)"},
                {"DelayDisabled", "Automatic door closing is now disabled"},
                {"DelaySetTo", "Automatic door closing delay set to {time}s"}
            };
            lang.RegisterMessages(messages, this);
        }

        #endregion

        #region Configuration

        private int DefaultDelay => GetConfig("DefaultDelay", 5);

        private int MaximumDelay => GetConfig("MaximumDelay", 30);

        private int MinimumDelay => GetConfig("MinimumDelay", 5);

        protected override void LoadDefaultConfig()
        {
            Config["DefaultDelay"] = DefaultDelay;
            Config["MaximumDelay"] = MaximumDelay;
            Config["MinimumDelay"] = MinimumDelay;
            SaveConfig();
        }

        #endregion

        #region Initialization

        private readonly DynamicConfigFile dataFile = Interface.Oxide.DataFileSystem.GetFile("AutoDoors");
        private Dictionary<ulong, int> playerPrefs = new Dictionary<ulong, int>();

        private void Init()
        {
            LoadDefaultConfig();
            LoadDefaultMessages();
            playerPrefs = dataFile.ReadObject<Dictionary<ulong, int>>();
            AddCovalenceCommand(new string[] { "ad", "autodoors", "autodoor", "doorauto", "doorautos" }, nameof(AutoDoorChatCmd));
           // cmd.AddChatCommand(GetMessage("ChatCommand"), this, nameof(AutoDoorChatCmd));
        }

        private void Unload()
        {
            foreach (var kvp in _doorActions)
            {
                var door = kvp.Key;

                if (door != null && !door.IsDestroyed)
                {
                    try { door.CancelInvoke(kvp.Value); }
                    finally { door.CloseRequest(); }
                }
            }

            SaveData();
        }

        #endregion

        #region Chat Command

        private void AutoDoorChatCmd(IPlayer player, string command, string[] args)
        {
            if (player == null || player.IsServer) return;

            int time;
            if (args?.Length < 1 || !int.TryParse(args[0], out time) || time > MaximumDelay || (time < MinimumDelay && time != 0))
            {
                player.Reply(string.Format(GetMessage("CommandUsage", player.Id), MinimumDelay.ToString("N0"), MaximumDelay.ToString("N0")));
                return;
            }

            playerPrefs[ulong.Parse(player.Id)] = time;


            player.Reply(time <= 0 ? GetMessage("DelayDisabled", player.Id) : string.Format(GetMessage("DelaySetTo", player.Id), time.ToString("N0")));
        }

        #endregion

        #region Door Closing

        private void OnDoorOpened(Door door, BasePlayer player)
        {
            if (door == null || player == null || !door.IsOpen() || door.ShortPrefabName.Contains("shutter")) return;
            var baseLock = door?.GetSlot(BaseEntity.Slot.Lock) as BaseLock;
            var codeLock = door?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (baseLock == null && codeLock == null) return;

            int time;
            if (!playerPrefs.TryGetValue(player.userID, out time)) time = DefaultDelay;
            if (time < 1) return;

            var action = new Action(() =>
            {
                if (!(door?.IsOpen() ?? false)) return;

                var isLocked = (codeLock != null) ? (codeLock.IsLocked()) : baseLock?.IsLocked() ?? false;

                if (!isLocked) return;
                if (player == null || (player?.IsDead() ?? true)) return;
                if ((player?.IsAlive() ?? false) && (player?.TimeAlive() ?? 0f) <= 120) return;

                try
                {
                    if (Interface.Oxide.CallHook("OnAutoDoorClose", door, player) == null)
                    {
                        door.SetFlag(BaseEntity.Flags.Open, false);
                        door.SendNetworkUpdate();
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
                finally { _doorActions.Remove(door); }

            });

            door.Invoke(action, time);

            _doorActions[door] = action;
        }

        private void OnDoorClosed(Door door, BasePlayer player)
        {
            if (door == null || player == null) return;

            Action doorAct;
            if (_doorActions.TryGetValue(door, out doorAct))
            {
                try { door.CancelInvoke(doorAct); }
                finally { _doorActions.Remove(door); }
            }

        }

        #endregion

        #region Helper Methods

        private void SaveData()
        {
            dataFile.WriteObject(playerPrefs);
        }

        private T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        private string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);

        #endregion
    }
}
