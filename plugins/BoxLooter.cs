using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("BoxLooter", "MBR", "1.0.1")]
    class BoxLooter : CovalencePlugin
    {
        private string Permission = "boxlooter.use";

        #region DataFile

        private class StoredData
        {
            public Dictionary<ulong, Dictionary<ulong, LootingInfo>> LootInfo = new Dictionary<ulong, Dictionary<ulong, LootingInfo>>();

            public class LootingInfo
            {
                public DateTime firstLoot;
                public DateTime lastLoot;
            }
        }

        private StoredData storedData;

        private void SaveDataFile() => Interface.Oxide.DataFileSystem.WriteObject(this.Name, this.storedData);

        #endregion

        #region Hooks

        private void Init()
        {
            permission.RegisterPermission(Permission, this);
            this.storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(this.Name);
            timer.Every(60 * 15f, SaveDataFile);
        }

        private void Unload()
        {
            SaveDataFile();
        }

        void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (player == null || entity == null || entity.net == null) return;

            if (storedData.LootInfo.ContainsKey(entity.net.ID.Value))
            {
                if (storedData.LootInfo[entity.net.ID.Value].ContainsKey(player.userID))
                {
                    if (storedData.LootInfo[entity.net.ID.Value][player.userID].firstLoot == null)
                    {
                        PrintWarning("Somehow, firstLoot was null for " + player.userID + " for box " + entity.net.ID.Value);
                        storedData.LootInfo[entity.net.ID.Value][player.userID].firstLoot = DateTime.UtcNow;
                    }
                    storedData.LootInfo[entity.net.ID.Value][player.userID].lastLoot = DateTime.UtcNow;
                }

                else
                {
                    storedData.LootInfo[entity.net.ID.Value].Add(player.userID, new StoredData.LootingInfo { firstLoot = DateTime.UtcNow, lastLoot = DateTime.UtcNow });
                }

                return;
            }

            storedData.LootInfo.Add(entity.net.ID.Value, new Dictionary<ulong, StoredData.LootingInfo>
            {
                { player.userID, new StoredData.LootingInfo{firstLoot = DateTime.UtcNow, lastLoot = DateTime.UtcNow} }
            });
        }

        void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null || entity.net == null) return;

            if (storedData.LootInfo.ContainsKey(entity.net.ID.Value)) storedData.LootInfo.Remove(entity.net.ID.Value);
        }

        private void OnNewSave(string filename)
        {
            PrintWarning("Detected wipe, wiping data file!");
            this.storedData.LootInfo.Clear();
            SaveDataFile();
        }

        #endregion

        #region Commands

        [Command("box")]
        private void BoxCMD(IPlayer player, string command, string[] args)
        {
            if (!(player.IsAdmin || player.HasPermission(Permission)))
            {
                player.Reply("You don't have permission to use this command!");
                return;
            }

            var entity = GetLookAtEntity(player, 10f);

            if (entity == null)
            {
                player.Reply("You are not looking at a storage container!");
                return;
            }

            if (entity.GetComponentInParent<StorageContainer>() == null && entity.GetComponentInChildren<StorageContainer>() == null && entity.GetComponent<StorageContainer>() == null)
            {
                player.Reply("Entity is not a storage container!");
                return;
            }

            Dictionary<ulong, StoredData.LootingInfo> item = null;

            if (!storedData.LootInfo.TryGetValue(entity.net.ID.Value, out item))
            {
                player.Reply("No logs found for this entity!");
                return;
            }

            if (item == null)
            {
                player.Reply("No logs found for this entity!");
                return;
            }

            player.Reply("List of looters for this \"" + entity.ShortPrefabName + "\"");

            var stringBuilder = new StringBuilder();
            var sbList = new List<StringBuilder>();
            var count = 0;

            foreach (var looter in item)
            {
                var lootPlayer = covalence.Players.FindPlayerById(looter.Key.ToString());
                var lootPlayerName = (lootPlayer == null ? "Unknown player" : "\"" + lootPlayer.Name + "\"") + " (" + looter.Key + ")";
                var Final = "<color=#F5D400>" + (++count) + " - </color><color=#4F9BFF>" + lootPlayerName + "</color>\nF: <color=#F80>" + looter.Value.firstLoot + " (" + this.ReadableTimeSpan(DateTime.UtcNow.Subtract(looter.Value.firstLoot)) + " ago)</color>\nL: <color=#F80>" + looter.Value.lastLoot + " (" + this.ReadableTimeSpan(DateTime.UtcNow.Subtract(looter.Value.lastLoot)) + " ago)</color>";
                if (stringBuilder.Length + Final.Length >= 768)
                {
                    sbList.Add(stringBuilder);
                    stringBuilder = new StringBuilder();
                }
                stringBuilder.Append(Final + "\n");
            }

            if (sbList.Count > 0) for (int i = 0; i < sbList.Count; i++) player.Reply(sbList[i].ToString().TrimEnd(", ".ToCharArray()));
            if (stringBuilder.Length > 0) player.Reply(stringBuilder.ToString().TrimEnd(", ".ToCharArray()));
            sbList.Clear();
        }

        #endregion

        #region Helpers

        BaseEntity GetLookAtEntity(IPlayer player, float maxDist = 250) => GetLookAtEntity(player.Object as BasePlayer, maxDist);

        BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250)
        {
            if (player == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (Physics.Raycast(ray, out hit, maxDist))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }

        string ReadableTimeSpan(TimeSpan span, string stringFormat = "N0")
        {
            if (span == TimeSpan.MinValue) return string.Empty;
            var str = string.Empty;
            if (span.TotalHours >= 24) str = (int)span.TotalDays + " day" + (span.TotalDays >= 2 ? "s" : string.Empty) + " " + (span.TotalHours - ((int)span.TotalDays * 24)).ToString(stringFormat) + " hour(s)";
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : string.Empty) + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = (span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : string.Empty)) + (span.Seconds < 1 ? string.Empty : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : string.Empty));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? (span.TotalDays.ToString(stringFormat)) + " day" + (span.TotalDays >= 1.5 ? "s" : string.Empty) : (span.TotalHours >= 1.0) ? (span.TotalHours.ToString(stringFormat)) + " hour" + (span.TotalHours >= 1.5 ? "s" : string.Empty) : (span.TotalMinutes >= 1.0) ? (span.TotalMinutes.ToString(stringFormat)) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : string.Empty) : (span.TotalSeconds >= 1.0) ? (span.TotalSeconds.ToString(stringFormat)) + " second" + (span.TotalSeconds >= 1.5 ? "s" : string.Empty) : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : string.Empty);
        }

        #endregion
    }
}
