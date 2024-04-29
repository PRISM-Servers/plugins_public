using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("DonateDropBox", "Shady", "1.0.5", ResourceId = 0)]
    [Description("Adds donation drop boxes")]

    class DonateDropBox : RustPlugin
    {
        /*
         * Copyright 2024 PRISM
         *
         * This file is part of PRISM's plugins.
         *
         * PRISM's plugins is free software: you can redistribute it and/or modify
         * it under the terms of the GNU General Public License as published by
         * the Free Software Foundation, either version 3 of the License, or
         * (at your option) any later version.
         *
         * PRISM's plugins is distributed in the hope that it will be useful,
         * but WITHOUT ANY WARRANTY; without even the implied warranty of
         * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
         * GNU General Public License for more details.
         *
         * You should have received a copy of the GNU General Public License
         * along with PRISM's plugins. If not, see <https://www.gnu.org/licenses/>.
         */

        /*
         * The below is not a license (that can be found above), but simply a notice to the reader of this code:
         * The development of PRISM has been gradual and started in 2014.
         * Some of the code provided in this repository may be outdated and may not be the best representation of the author's current coding practices.
         * Some of the code provided in this repository may have been initially developed in 2014.
         * Files not containing the above license are not uniquely part of PRISM's plugins and are subject to the license of the original plugin, where applicable.
         * No copyright infringement is ever intended and the author of PRISM's plugins will comply with any requests to remove code.
         * PRISM's plugins are provided as is and no functionality is guaranteed.
         * Many of these plugins will not work without significant changes as there are numerous instances of hard-coded functions and may rely on external API calls that are not a part of this project itself.
         * 
         * 
         * With those notices out of the way, I, Shady, the author of this plugin, would like you to read and acknowledge the following:
         * These plugins are part of a decade long passion project where we, PRISM, created a unique experience to all players on our Rust servers.
         * This was a significant part of my life. I dedicated a decade to developing and maintaining these plugins and the servers themselves.
         * I implore you that, should you use any of the code or plugins provided in this repository, where it belongs solely to PRISM, that you respect the time and effort put into it.
         * Please remember that I, a single human, invested a significant amount of time and effort into these plugins and the servers they were used on.
         * Please feel free to critique my code as it is written; I am aware it is not perfect. Much of it is old, and I'm capable of doing far better now.
         * Moreover, I ask, sincerely, that you do not use these plugins or code without acknowledging that someone dedicated themselves to this passion project.
         * In asking that, it is my simple desire that you do not let these plugins be used in a way that is inconsistent with my own desires and love.
         * Please do not use them and claim them as your own. I wish for you to use them on your server and make that server something of your own.
         * Something unique and beautiful. However, I ask that you do not use these plugins and claim them as your own work.
         * Should you make changes to them, you are, of course, the author of those changes. But, if your code is based on one of PRISM's original plugins, please do not claim it as your own.
         * I also request that you do not use the plugins in a way that would be detrimental to the Rust community or the community of server owners.
         * In doing so, I ask that you please do not monetize the functions within these plugins with any changes you should make.
         * Should you use any of PRISM's plugins or its code, please do not lock anything behind a paywall or in a way that is unfair to the community of Rust.
         * My origin is that of one where I saw, daily, very young players who only wished to enjoy a few hours of their life on a video game and in many cases had little or no expendable income.
         * For many people, even purchasing the game is significantly difficult. It is, in my eyes, unfair to create fun, custom content, and suggest that you care about the community while also charging for it or creating an unfair environment where players who pay are significantly advantaged compared to those who cannot afford to do so.
         * I cannot control what you do with this code, but I ask that you respect the spirit in which it was created:
         * That is one of sincere love, care, and appreciation for the people who played on PRISM's servers.
         * Our community, much like yours, is your entire server. Your people matter, just as you matter. Please remember that *everyone* deserves a fair, fun environment.
         * I ask that should you use these plugins, you do so with the same love and care that I put into them. I ask that you do so with the same respect, love, care, and appreciation for the people who will play on your server.
         *
         * One of my greatest helps over the years was MBR. He has been a wonderful person, friend, and developer; I am beyond grateful for his contributions to us, but moreover, his friendship.
         * Some of these plugins were authored solely by him, or with his help. Where this has occurred, it should be noted in the author field at the top of the plugin.
         * Worthy of note is that he is solely responsible for our top-tier Discord integration.
         * If you so wish to see his GitHub page, here it is linked below:
         * https://github.com/MBR-0001
         * 
         * 
         * Should you wish to support the work that I have put into this project of the past decade, please consider doing so here:
         * https://www.buymeacoffee.com/shady757
         * 
         * I neither expect nor anticipate any such donations. You are by no means obligated, it is truly a donation. It should be done out of the kindness of your heart, if you so choose.
         * 
         * PRISM, as a community, has not ceased and continues to exist. We have no plans of ceasing our community. We have, however, ceased our Rust servers and thus made these plugins open source.
         * If you ever wish to contact me, or simply join our community and become one of our beloved members, please join us here:
         * https://discord.gg/DUCnZhZ
         * 
         * To quote Rush: "There is magic at your fingers".
         * With love,
         * Shady and all of PRISM. Thank you for everything, always.
         * 
         */
        public readonly int deployColl = LayerMask.GetMask(new string[] { "Deployable", "Deployed" });
        private readonly string adminPerm = "donatedropbox.admin";

        private bool? _wasEasy;
        private bool IsEasyServer()
        {
            if (_wasEasy.HasValue) return (bool)_wasEasy;
            var val = ConVar.Server.hostname.Contains("#2") || ConVar.Server.hostname.Contains("#3");
            _wasEasy = val;
            return val;
        }
        private bool IsTrash(ItemDefinition def)
        {
            if (def == null) return true;
            if (def.category == ItemCategory.Food) return true;
            if (def.shortname.Contains("can.") || def.shortname == "rock" || def.shortname == "torch" || def.shortname.Contains("skull") || def.shortname == "bone.fragments" || def.shortname.Contains("burlap") || def.shortname.Contains("diving") || def.shortname.Contains("water") || def.shortname.Contains("arrow") || def.shortname.Contains("handmade") || def.shortname.Contains("wood.armor") || def.shortname.Contains("tuna") || def.shortname.Contains("lantern") || def.shortname.Contains("sign.") || def.shortname.Contains("stone.") || def.shortname == "stonehatchet" || def.shortname.Contains("electric.")) return true;
            return false;
        }
        class Dropboxes
        {
            public List<DropBoxInfo> dropboxes = new List<DropBoxInfo>();
            public Dropboxes() { }
        }

        class DropBoxLog
        {
            public int itemID = 0;
            public ulong userID = 0;
            public int amount = 0;
            [JsonRequired]
            private string _createTime = string.Empty;

            [JsonIgnore]
            public DateTime CreationTime
            {
                get
                {
                    DateTime time;
                    if (string.IsNullOrEmpty(_createTime) || !DateTime.TryParse(_createTime, out time)) time = DateTime.MinValue;
                    return time;
                }
                private set { _createTime = value.ToString(); }
            }

            public DropBoxLog() { CreationTime = DateTime.UtcNow; }
        }

        class DropBoxInfo
        {
            public NetworkableId netID;
            public bool refund = true;
            public bool InfiniteStack { get; set; } = true;
            public List<DropBoxLog> Logs = new List<DropBoxLog>();
            public DropBoxInfo(NetworkableId netId, bool doRefund = true)
            {
                netID = netId;
                refund = doRefund;
            }
            public void AddLog(ulong userID, Item item)
            {
                if (userID == 0 || item == null || item.amount < 1) return;
                var newLog = new DropBoxLog();
                newLog.amount = item.amount;
                newLog.itemID = item.info.itemid;
                newLog.userID = userID;
                Logs.Add(newLog);
            }
            public void AddLog(ulong userID, int itemId, int amount)
            {
                if (userID == 0 || itemId == 0 || amount < 1) return;
                var newLog = new DropBoxLog();
                newLog.amount = amount;
                newLog.itemID = itemId;
                newLog.userID = userID;
                Logs.Add(newLog);
            }

            //public DropBox GetDropbox() { return (netID == 0) ? null : ((BaseNetworkable.serverEntities?.Where(p => p != null && p.net != null && p.net.ID == netID)?.FirstOrDefault() ?? null) as DropBox); }
        }

        Dropboxes dropBoxes;

        void Init()
        {
            Unsubscribe(nameof(OnItemAddedToContainer));
            permission.RegisterPermission(adminPerm, this);
            if ((dropBoxes = Interface.Oxide.DataFileSystem.ReadObject<Dropboxes>("DonateDropBoxes")) == null) dropBoxes = new Dropboxes();
        }

        void OnServerInitialized()
        {
            Subscribe(nameof(OnItemAddedToContainer));
        }

        void Unload() => SaveData();
        void OnServerSave() => SaveData();

        void SaveData() => Interface.GetMod().DataFileSystem.WriteObject("DonateDropBoxes", dropBoxes);

        [ChatCommand("ddbox")]
        private void cmdDonateBox(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, adminPerm))
            {
                SendReply(player, "No permission");
                return;
            }
            if (args.Length < 1)
            {
                SendReply(player, "No args");
                return;
            }
            var arg0Lower = args[0].ToLower();
            if (arg0Lower != "remove" && arg0Lower != "add" && arg0Lower != "refund" && arg0Lower != "log" && arg0Lower != "stack")
            {
                SendReply(player, "Unknown arg: " + args[0]);
                return;
            }
            var box = GetLookAtEntity(player, 250, deployColl) as DropBox;
            if (box == null || box.IsDestroyed)
            {
                SendReply(player, "You must be looking at a Drop Box to use this command!");
                return;
            }

            var netID = box.net.ID;

            var isDonationBox = IsDonationBox(netID);
            if (arg0Lower == "add")
            {
                if (!isDonationBox)
                {
                    AddDonationBox(netID);
                    SendReply(player, "Drop box is now a donation box");
                }
                else SendReply(player, "Drop box is already a donation box");
            }
            else if(arg0Lower == "remove")
            {
                if (isDonationBox)
                {
                    RemoveDonationBox(netID);
                    SendReply(player, "Drop box is no longer a donation box");
                }
                else SendReply(player, "Drop box is not a donation box");
            }
            else if (arg0Lower == "refund")
            {
                if (!isDonationBox)
                {
                    SendReply(player, "Not a donation drop box");
                    return;
                }

                var boxInfo = GetDropBox(box.net.ID);
                if (boxInfo == null)
                {
                    SendReply(player, "Box info is null");
                    return;
                }
                else SendReply(player, "Set refund to: " + (boxInfo.refund = !boxInfo.refund));
            }
            else if (arg0Lower == "log")
            {
                if (!isDonationBox)
                {
                    SendReply(player, "Not a donation drop box");
                    return;
                }
                var boxInfo = GetDropBox(box.net.ID);
                if (boxInfo == null)
                {
                    SendReply(player, "Box info is null");
                    return;
                }
                var showLogs = boxInfo?.Logs?.Where(p => p != null && (DateTime.UtcNow - p.CreationTime).TotalHours < 48) ?? null;
                if (showLogs == null || !showLogs.Any())
                {
                    SendReply(player, "No logs!");
                    return;
                }
                var logSB = new StringBuilder();
                var logSBs = new List<StringBuilder>();
                foreach(var log in showLogs)
                {
                    if (log == null) continue;
                    var itemDef = ItemManager.FindItemDefinition(log.itemID);
                    if (itemDef == null) continue;
                    var amountStr = log.amount > 1 ? (" x" + log.amount.ToString("N0")) : string.Empty;
                    var logStr = GetDisplayNameFromID(log.userID.ToString()) + ": " + itemDef.displayName.english + amountStr + " at: " + log.CreationTime + " (" + (DateTime.UtcNow - log.CreationTime).TotalHours.ToString("0.00") + " hours ago)";
                    if ((logSB.Length + logStr.Length) >= 896)
                    {
                        logSBs.Add(logSB);
                        logSB = new StringBuilder();
                    }
                    logSB.AppendLine(logStr);
                }
             
                if (logSBs.Count > 0) for (int i = 0; i < logSBs.Count; i++) SendReply(player, logSBs[i].ToString().TrimEnd());
                if (logSB.Length > 0) SendReply(player, logSB.ToString().TrimEnd());
                SendReply(player, "All times are UTC.");
            }
            else if (arg0Lower == "stack")
            {
                if (!isDonationBox)
                {
                    SendReply(player, "Not a donation drop box");
                    return;
                }
                var boxInfo = GetDropBox(box.net.ID);
                if (boxInfo == null)
                {
                    SendReply(player, "Box info is null");
                    return;
                }
                else SendReply(player, "Set infinite stack to: " + (boxInfo.InfiniteStack = !boxInfo.InfiniteStack));
            }
        }

        List<ItemAmount> GetRefunds(Item item)
        {
            if (item == null) return null;
            var amounts = new List<ItemAmount>();
            if (item?.info?.Blueprint == null || item.info.Blueprint.ingredients == null) return amounts;
            var effec = 0.6f;
            if (item.info.Blueprint.scrapFromRecycle > 0)
            {
                var scrap = new ItemAmount();
                scrap.itemDef = ItemManager.FindItemDefinition("scrap");
                scrap.amount = (float)Math.Round((scrap.startAmount = (item.info.Blueprint.scrapFromRecycle * 1.2f) * item.amount), MidpointRounding.AwayFromZero);
                amounts.Add(scrap);
            }
            if (item.hasCondition) effec = Mathf.Clamp01(effec * Mathf.Clamp(item.conditionNormalized * item.maxConditionNormalized, 0.1f, 1f));
            for (int i = 0; i < item.info.Blueprint.ingredients.Count; i++)
            {
                var ingredient = item.info.Blueprint.ingredients[i];
                if (ingredient == null || ingredient.itemDef.shortname == "scrap") continue;
                var num2 = ingredient.amount / (float)item.info.Blueprint.amountToCreate;
                var num3 = 0;
                if (num2 <= 1.0)
                {
                    for (int j = 0; j < item.amount; j++)
                    {
                        if (UnityEngine.Random.Range(0.0f, 1f) <= effec) num3++;
                    }
                }
                else num3 = Mathf.CeilToInt(Mathf.Clamp(num2 * effec * UnityEngine.Random.Range(1f, 1f), 0.0f, ingredient.amount) * item.amount);
                if (num3 > 0)
                {
                    int num4 = Mathf.CeilToInt((float)num3 / (float)ingredient.itemDef.stackable);
                    for (int index = 0; index < num4; ++index)
                    {
                        int iAmount = num3 <= ingredient.itemDef.stackable ? num3 : ingredient.itemDef.stackable;
                        var newAmount = new ItemAmount();
                        newAmount.itemDef = ingredient.itemDef;
                        newAmount.amount = iAmount;
                        newAmount.startAmount = iAmount;
                        amounts.Add(newAmount);
                        num3 -= iAmount;
                        if (num3 <= 0)
                            break;
                    }
                }
            }
            return amounts;
        }

        string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty;
        }

        void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null) return;
            var box = container?.entityOwner as DropBox;
            if (box == null || box.OwnerID == 0) return;
            if (!IsDonationBox(box.net.ID)) return;
            var looter = FindLooterFromCrate(box);
            var boxPos = box?.transform?.position ?? Vector3.zero;
            if (item.position == box.mailInputSlot) return;
            if (!permission.UserHasPermission(box.OwnerID.ToString(), adminPerm)) return;
            var dismissItem = false;
            if (IsTrash(item?.info)) dismissItem = true;
            if (item.isBroken || item.conditionNormalized <= 0.1f)
            {
                RemoveFromWorld(item);
                return;
            }
            if (item.conditionNormalized < 0.95f) dismissItem = true;
            var dropPos = looter?.GetDropPosition() ?? box?.GetDropPosition() ?? Vector3.zero;
            var dropVel = looter?.GetDropVelocity() ?? box?.GetDropVelocity() ?? Vector3.zero;
            var dropRotation = looter?.ServerRotation ?? Quaternion.identity;
            if (!dismissItem)
            {
                if (!item.Drop(dropPos, dropVel, dropRotation)) return; //attempt to drop, if we can't, return (shouldn't ever happen). Removing the item from container or trying to move it in general without dropping the item results in the item getting stuck in the original dropbox container, no idea why, but it seems to have 100% success rate when it's dropped first.
            }
           
            var chests = new List<StorageContainer>();
            Vis.Entities(box.transform.position, 2.35f, chests, deployColl);
            //       chests = chests?.Where(p => p != null && p.OwnerID == box.OwnerID && !p.ShortPrefabName.Contains("dropbox")) ?? null;

            var chestsOrdered = chests?.Where(p => p != null && p.OwnerID == box.OwnerID && !p.ShortPrefabName.Contains("dropbox"))?.OrderBy(p => Vector3.Distance(boxPos, p?.transform?.position ?? Vector3.zero)) ?? null;
            StorageContainer chest = null;
            if (chestsOrdered != null)
            {
                foreach(var ch in chestsOrdered)
                {
                    if (ch == null || ch.IsDestroyed) continue;
                    var isFullSlots = ch.inventory.itemList.Count >= ch.inventory.capacity;
                    var canStack = false;
                    if (!dismissItem)
                    {
                        for (int i = 0; i < ch.inventory.itemList.Count; i++)
                        {
                            var loopItem = ch.inventory.itemList[i];
                            if (loopItem?.info == item?.info && (loopItem.amount + item.amount) <= loopItem.MaxStackable())
                            {
                                canStack = true;
                                break;
                            }
                        }
                    }
                   
                   // var canStack = ch?.inventory?.itemList?.Any(p => p != null && p.info == item.info && (p.amount + item.amount) <= p.MaxStackable()) ?? false;
                    //   PrintWarning("isFullSlots: " + isFullSlots + ", can stack: " + canStack);
                    if (isFullSlots && !canStack && !dismissItem) continue;
                    else
                    {
                        //   PrintWarning("Got chest, full: " + isFullSlots + ", stack: " + canStack);
                        chest = ch;
                        break;
                    }
                }
            }

           // var chest = chestsOrdered?.Where(p => p != null && p.inventory.itemList.Count < p.inventory.capacity)?.FirstOrDefault() ?? chestsOrdered?.FirstOrDefault() ?? null;
            if (chest == null || chest.IsDestroyed)
            {
                PrintWarning("no chest for dropbox!! chests count: " + (chestsOrdered != null ? chestsOrdered.Count() : 0));
                return;
            }
            if (chest.OwnerID != box.OwnerID)
            {
                PrintWarning("chest ownerid != entity ownerid!! " + chest.OwnerID + " != " + box.OwnerID);
                return;
            }
         //   PrintWarning("using chest: " + chest.ShortPrefabName + " (" + chest.net.ID + ", owner: " + chest.OwnerID + ", at: " + chest.transform.position);
            var rarity = item?.info?.rarity ?? Rust.Rarity.None;
            
            var rarityMult = Mathf.Clamp((int)rarity, 1, 4);
            var preDropAmount = item?.amount ?? 0;
            NextTick(() =>
            {
                if (item == null || chest?.inventory == null || looter == null || looter.IsDead()) return;
                var refunds = GetRefunds(item); //get refunds *prior* to moving to prevent a bug where item combines with an existing stack of an item, causing the item amount to possibly be lower or higher than what they actually donated
                var didMove = false;
                Item findItem = null;
                if (!dismissItem)
                {
                    for (int i = 0; i < chest.inventory.itemList.Count; i++)
                    {
                        var loopItem = chest.inventory.itemList[i];
                        if (loopItem?.info?.itemid == item?.info?.itemid && loopItem?.text == item?.text && loopItem?.name == item?.name)
                        {
                            findItem = loopItem;
                            break;
                        }
                    }
                }
                
                //var findItem = chest?.inventory?.itemList?.Where(p => p?.info?.itemid == item?.info?.itemid && p?.text == item?.text && p?.name == item?.name)?.FirstOrDefault() ?? null;
                if (!dismissItem)
                {
                    if (findItem == null) didMove = item.MoveToContainer(chest.inventory);
                    else
                    {
                        findItem.amount += item.amount;
                        findItem.MarkDirty();
                        didMove = true;
                        NextTick(() =>
                        {
                            RemoveFromWorld(item);
                        });
                    }
                }
                else RemoveFromWorld(item);
              
               // var didMove = item.MoveToContainer(chest.inventory);
                var fxPath = "assets/prefabs/locks/keypad/effects/lock.code." + ((didMove || dismissItem) ? "updated" : "denied") + ".prefab";
                Effect.server.Run(fxPath, boxPos, Vector3.zero);
                if (!didMove && !dismissItem)
                {
                    PrintWarning("Did not move!");
                    return;
                }
                if (item != null && item.info.shortname == "note")
                {
                    item.text = item.text + Environment.NewLine + (looter?.displayName ?? "Unknown") + " (" + (looter?.UserIDString ?? string.Empty) + ")";
                    item.MarkDirty();
                }
                PrintWarning(looter.displayName + " donated: " + item.info.shortname + " x" + preDropAmount + " trash: " + dismissItem);
                var boxInfo = GetDropBox(box.net.ID);
                if (boxInfo == null)
                {
                    PrintError("Boxinfo null!!");
                    return;
                }
                //boxInfo.AddLog(looter.userID, item);
                if (!boxInfo.refund)
                {
                    PrintWarning("No box info refunds");
                    return;
                }
             
                if (refunds == null || refunds.Count < 1)
                {
                    PrintWarning("No refunds for: " + item.info.shortname);
                    if (refunds == null) refunds = new List<ItemAmount>();
                    if (item.info.shortname != "scrap")
                    {
                        var genericAmount = IsEasyServer() && (item.info.category != ItemCategory.Resources) ? 0 : (int)Math.Round(item.info.shortname.Contains("sulfur") ? (0.01 * preDropAmount) : item.info.shortname.Contains("stones") ? (0.00375 * preDropAmount) : (item.info.shortname == "metal.ore" || item.info.shortname == "metal.fragments") ? (0.004 * preDropAmount) : (item.info.shortname == "hq.metal.ore" || item.info.shortname == "metal.refined") ? (0.175 * preDropAmount) : item.info.shortname == "wood" ? (0.0015 * preDropAmount) : item.info.shortname == "cloth" ? (0.008 * preDropAmount) : item.info.shortname == "leather" ? (0.045 * preDropAmount) : item.info.shortname == "crude.oil" ? (0.15 * preDropAmount) : item.info.shortname == "fat.animal" ? (0.035 * preDropAmount) : item.info.shortname == "battery.small" ? (0.275 * preDropAmount) : 0, MidpointRounding.AwayFromZero);
                        if (genericAmount < 1) return;
                        genericAmount = Mathf.Clamp(genericAmount, 1, 480);
                        var amount = new ItemAmount(ItemManager.FindItemDefinition("scrap"), genericAmount);
                        PrintWarning("added refund: " + amount.amount + " " + amount.startAmount + " " + amount.itemDef.shortname + " for: " + item.info.shortname + " x" + preDropAmount);
                        refunds.Add(amount);
                    }
                   
                }
                for(int i = 0; i < refunds.Count; i++)
                {
                    var refund = refunds[i];
                    if (refund == null) continue;
                    if (refund.amount < 1 || refund.startAmount < 1) continue;
                    var refundItem = ItemManager.Create(refund.itemDef, (int)Math.Round(refund.amount, MidpointRounding.AwayFromZero));
                    if (refundItem != null && !refundItem.MoveToContainer(looter.inventory.containerMain) && !refundItem.MoveToContainer(looter.inventory.containerBelt) && !refundItem.Drop(dropPos, dropVel, dropRotation)) RemoveFromWorld(refundItem);
                    else { if (looter.IsConnected) looter.SendConsoleCommand("note.inv " + refund.itemid + " " + refundItem.amount); }
                }
            });
        }

        BasePlayer FindLooterFromCrate(BaseEntity crate)
        {
            if (crate == null) return null;
            var looters = new List<BasePlayer>();
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || player.IsDead() || !player.IsConnected) continue;
                var lootSource = player?.inventory?.loot?.entitySource ?? null;
                if (lootSource != null && lootSource == crate) looters.Add(player);
            }
            return looters.Count == 1 ? looters[0] : null;
        }
        BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (UnityEngine.Physics.Raycast(ray, out hit, maxDist, ((coll != -1) ? coll : Rust.Layers.Construction)))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }
        bool IsDonationBox(NetworkableId netID)
        {
            if (dropBoxes == null || dropBoxes.dropboxes == null || dropBoxes.dropboxes.Count < 1) return false;
            for(int i = 0; i < dropBoxes.dropboxes.Count; i++)
            {
                if (dropBoxes?.dropboxes[i]?.netID == netID) return true;
            }
            return false;
            //return dropBoxes.dropboxes.Any(p => p.netID == netID);
        }
        void AddDonationBox(NetworkableId netID, bool refund = true)
        {
            if (dropBoxes?.dropboxes != null && !IsDonationBox(netID)) dropBoxes.dropboxes.Add(new DropBoxInfo(netID, refund));
        }
        void RemoveDonationBox(NetworkableId netID)
        {
            if (IsDonationBox(netID))
            {
                for (int i = 0; i < dropBoxes.dropboxes.Count; i++)
                {
                    var dbox = dropBoxes.dropboxes[i];
                    if (dbox?.netID == netID)
                    {
                        dropBoxes.dropboxes.Remove(dbox);
                        break;
                    }
                }
            }
        }
        DropBoxInfo GetDropBox(NetworkableId netID)
        {
            for (int i = 0; i < dropBoxes.dropboxes.Count; i++)
            {
                var dbox = dropBoxes.dropboxes[i];
                if (dbox?.netID == netID) return dbox;
            }
            return null;
            //return (netID == 0) ? null : dropBoxes?.dropboxes?.Where(p => p.netID == netID)?.FirstOrDefault() ?? null;
        }
        
        void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            if (item?.parent != null) item.RemoveFromContainer();
            item.Remove();
        }
    }
}
