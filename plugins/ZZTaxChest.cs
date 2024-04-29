using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("ZZTaxChest", "Shady", "1.0.5")]
    internal class ZZTaxChest : RustPlugin
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
        private readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        private readonly int deployColl = LayerMask.GetMask(new string[] { "Deployable", "Deployed" });

        private const int MAX_STACK = 2147450880;
        private readonly Dictionary<string, float> _itemBuffers = new Dictionary<string, float>();

        private float GetBuffer(string shortName)
        {
            if (string.IsNullOrEmpty(shortName)) return -1f;

            float val;

            if (!_itemBuffers.TryGetValue(shortName, out val)) return 0f;
            else return val;
        }

        private void SetBuffer(string shortName, float val)
        {
            if (string.IsNullOrEmpty(shortName)) return;
            _itemBuffers[shortName] = val;
        }

        private void AddBuffer(string shortName, float val)
        {
            if (string.IsNullOrEmpty(shortName)) return;

            float outVal;

            if (!_itemBuffers.TryGetValue(shortName, out outVal)) _itemBuffers[shortName] = val;
            else _itemBuffers[shortName] += val;
        }

        private void SubtractBuffer(string shortName, float val) => AddBuffer(shortName, -val);

        private class StoredData
        {
            public ulong TaxChestID { get; set; }
            public float TaxRate { get; set; } = 0.01f;
            public StoredData() { }
        }

        private StoredData data;
        public ulong TaxChestID
        {
            get { return data?.TaxChestID ?? 0; }
            set { if (data != null) data.TaxChestID = value; }
        }
        public float TaxRate
        {
            get { return data?.TaxRate ?? 0f; }
            set { if (data != null) data.TaxRate = value; }
        }

        public DateTime LastTaxChestSearch = DateTime.MinValue;

        private StorageContainer _taxChest;
        public StorageContainer ChestEntity
        {
            get
            {
                if (TaxChestID != 0 && _taxChest == null)
                {
                    var now = DateTime.UtcNow;
                    if (LastTaxChestSearch <= DateTime.MinValue || (now - LastTaxChestSearch).TotalSeconds >= 45)
                    {
                        foreach (var entity in BaseEntity.saveList)
                        {
                            if (entity?.net?.ID.Value == TaxChestID)
                            {
                                _taxChest = entity as StorageContainer;
                                break;
                            }
                        }
                        LastTaxChestSearch = now;
                    }

                }

                ToggleAllHooks(_taxChest != null);

                return _taxChest;
            }
            set
            {
                _taxChest = value;
                TaxChestID = value?.net?.ID.Value ?? 0;

                ToggleAllHooks(_taxChest != null);
            }
        }

        private int DoTaxDef(ItemDefinition def, int amount)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));
            var watch = Stopwatch.StartNew();
            var chest = ChestEntity;
            if (chest == null || chest.IsDestroyed || chest?.inventory?.itemList == null) return 0;


            int newAmount;

            var newAmountD = amount * TaxRate;
            newAmount = (int)(amount * TaxRate); //rounding would be creating MORE of the item than there actually is. we don't want to cheat on our taxes, now do we?
            if (newAmount < 1 && newAmountD > 0) AddBuffer(def.shortname, newAmountD);


            var addAmount = newAmount;
            var getBuffer = GetBuffer(def.shortname);
            if (getBuffer >= 1)
            {
                var intBuffer = (int)getBuffer;
                if (addAmount > 0) addAmount += intBuffer; //again, rounding would be creating MORE of the item than there actually is. we're not tax thieves!
                SubtractBuffer(def.shortname, intBuffer);
            }
            if (addAmount < 1) return 0;



            var itemId = def?.itemid ?? 0;
            Item findItem = null;
            if ((chest?.inventory?.itemList?.Count ?? 0) > 0)
            {
                var itemList = chest.inventory.itemList;
                for (int i = 0; i < itemList.Count; i++)
                {
                    var item2 = itemList[i];
                    if ((item2?.info?.itemid ?? 0) == itemId)
                    {
                        findItem = item2;
                        break;
                    }
                }
            }
            if (findItem != null)
            {
                findItem.amount += addAmount;
                findItem.MarkDirty();
            }
            else
            {
                var newItem = ItemManager.CreateByItemID(itemId, addAmount);
                if (newItem != null && !newItem.MoveToContainer(chest.inventory)) RemoveFromWorld(newItem);
            }


            watch.Stop();
            if (watch.Elapsed.TotalMilliseconds >= 3) PrintWarning("DoTax took: " + watch.Elapsed.TotalMilliseconds + "ms for: " + def?.shortname + " x" + amount);
            return addAmount; // newAmount;
        }

        private void ToggleAllHooks(bool desiredState)
        {
            if (desiredState)
            {
                Subscribe(nameof(OnDispenserGather));
                Subscribe(nameof(OnDispenserBonus));
            }
            else
            {
                Unsubscribe(nameof(OnDispenserGather));
                Unsubscribe(nameof(OnDispenserBonus));
            }
        }

        private void DoTax(Item item, bool takeAll = false)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.amount < 1)
            {
                PrintWarning("item amount < 1!!!! (DoTax)" + item);

                throw new ArgumentOutOfRangeException(nameof(item));
            }

            var watch = Stopwatch.StartNew();
            var chest = ChestEntity;
            if (chest == null || chest.IsDestroyed || chest?.inventory?.itemList == null) return;

            var amount = item?.amount ?? 0;
            if (amount < 1) return;

            int newAmount;

            if (takeAll) newAmount = amount;
            else
            {
                var newAmountD = amount * TaxRate;
                newAmount = (int)(amount * TaxRate); //rounding would be creating MORE of the item than there actually is. we don't want to cheat on our taxes, now do we?
                if (newAmount < 1 && newAmountD > 0) AddBuffer(item.info.shortname, newAmountD);
            }


            var addAmount = newAmount;
            var getBuffer = GetBuffer(item.info.shortname);
            if (getBuffer >= 1)
            {
                var intBuffer = (int)getBuffer;
                if (addAmount > 0) addAmount += intBuffer; //again, rounding would be creating MORE of the item than there actually is. we're not tax thieves!
                SubtractBuffer(item.info.shortname, intBuffer);
            }
            if (addAmount < 1) return;

            if (!takeAll && (item.amount >= newAmount && newAmount > 0))
            {
                item.amount -= newAmount;
                item.MarkDirty();
            }

            var itemId = item?.info?.itemid ?? 0;
            if (!takeAll)
            {
                Item findItem = null;
                if ((chest?.inventory?.itemList?.Count ?? 0) > 0)
                {
                    var itemList = chest.inventory.itemList;
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var item2 = itemList[i];
                        if ((item2?.info?.itemid ?? 0) == itemId)
                        {
                            findItem = item2;
                            break;
                        }
                    }
                }
                if (findItem != null)
                {
                    findItem.amount += addAmount;
                    findItem.MarkDirty();
                }
                else
                {
                    var newItem = ItemManager.CreateByItemID(itemId, newAmount);
                    if (newItem != null && !newItem.MoveToContainer(chest.inventory)) RemoveFromWorld(newItem);
                }
                if (addAmount >= item.amount) RemoveFromWorld(item);
            }
            else
            {
                if (!item.MoveToContainer(chest.inventory))
                {
                    RemoveFromWorld(item);
                    PrintWarning("Couldn't move full tax item (takeAll true) to tax chest!");
                }
            }

            watch.Stop();
            if (watch.Elapsed.TotalMilliseconds >= 3) PrintWarning("DoTax took: " + watch.Elapsed.TotalMilliseconds + "ms for: " + item?.info?.shortname + " x" + amount);
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (dispenser == null) return;
            var player = entity as BasePlayer;
            if (player == null) return;

            if (item.amount < 1)
            {
                PrintWarning("item has amount < 1!!!: " + item + " @ " + dispenser?._baseEntity?.ShortPrefabName);
                return;
            }

            DoTax(item);
        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            if (dispenser == null || player == null || item == null) return;

            if (item.amount < 1)
            {
                PrintWarning("item has amount < 1!!! (bonus): " + item + " @ " + dispenser?._baseEntity?.ShortPrefabName);
                return;
            }

            DoTax(item);
        }

        private void OnVendingTransaction(VendingMachine vending, BasePlayer player, int sellOrderId, int numberOfTransactions)
        {
            if (!(vending is NPCVendingMachine)) return; //only tax NPC vending machines!!
            if (vending == null || player == null)
            {
                PrintWarning("Bad vending/player!");
                return;
            }
            if (vending?.sellOrders?.sellOrders == null || vending.sellOrders.sellOrders.Count < 1) return;

            var getOrder = vending.sellOrders.sellOrders[sellOrderId];
            if (getOrder == null)
            {
                PrintWarning("getOrder is null for sellOrderID!!! " + sellOrderId);
                return;
            }
            var currencyDef = ItemManager.FindItemDefinition(getOrder.currencyID);
            if (currencyDef == null)
            {
                PrintWarning("Couldn't get currency item definition from ID!!! " + getOrder.currencyID);
                return;
            }
            var realCost = (getOrder?.currencyAmountPerItem ?? -1) * numberOfTransactions;
            if (realCost < 1)
            {
                PrintWarning("realCost < 1 for currency item!!! " + currencyDef?.shortname);
                return;
            }
            var taxItem = ItemManager.Create(currencyDef, realCost);
            DoTax(taxItem, true);
        }

        private object CanStackItem(Item item, Item targetItem)
        {
            if (item == null || targetItem == null) return null;
            var cont1 = item?.parent?.entityOwner ?? targetItem?.parent?.entityOwner ?? null;
            var cont2 = targetItem?.parent?.entityOwner ?? item?.parent?.entityOwner ?? null;
            if (cont1 == null || cont2 == null) return null;
            if (ChestEntity != null && !ChestEntity.IsDestroyed && (ChestEntity == cont1 || ChestEntity == cont2))
            {
                var canStack = targetItem != item && item.info.stackable > 1 && (targetItem.info.stackable > 1 && targetItem.info.itemid == item.info.itemid) && (item.IsValid()) && (!item.IsBlueprint() || item.blueprintTarget == item.blueprintTarget);
                if (canStack) return true;
            }
            return null;
        }

        private object OnMaxStackable(Item item)
        {
            if (item == null) return null;
            var cont1 = item?.parent?.entityOwner ?? null;
            if (cont1 != null && cont1 == ChestEntity) return MAX_STACK;
            return null;
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null) return;
            if (entity?.net?.ID.Value == TaxChestID)
            {
                _taxChest = null;
                PrintWarning("Tax chest has been killed!");
            }
        }

        private void Init()
        {
            data = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("TaxChest") ?? new StoredData();
        }

        private void OnServerInitialized()
        {
            if (TaxChestID != 0 && (ChestEntity == null || ChestEntity.IsDestroyed))
            {
                PrintWarning("TaxChestID was: " + TaxChestID + ", but chest no longer exists!");
                TaxChestID = 0;
            }
        }

        private void OnServerSave() => SaveData();

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("TaxChest", data);
        private void Unload() => SaveData();

        [ChatCommand("taxc")]
        private void cmdTaxC(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var arg0Lower = args.Length > 0 ? args[0].ToLower() : string.Empty;
            if (string.IsNullOrEmpty(arg0Lower))
            {
                var chest = ChestEntity;
                if (chest == null || chest.IsDestroyed) SendReply(player, "No tax chest has been set yet, or it was destroyed. You can set one by looking at a chest and typing <color=#8aff47>/taxc set</color>.");
                else SendReply(player, "Tax chest exists at: " + (chest?.transform?.position ?? Vector3.zero) + ", net ID: " + (chest?.net?.ID.Value ?? 0) + Environment.NewLine + "You can change the tax chest by looking at a chest and typing <color=#8aff47>/taxc set</color>.");
                SendReply(player, "Current tax rate: <color=#8aff47>" + TaxRate + " (" + (TaxRate * 100f) + "%)</color>. You can set it by typing: <color=#8aff47>/taxc rate <value></color>.");
                return;
            }
            if (arg0Lower != "set" && arg0Lower != "rate" && arg0Lower != "tp")
            {
                SendReply(player, "Invalid argument: " + args[0]);
                return;
            }
            if (arg0Lower == "set")
            {
                var lookAt = GetLookAtEntity(player, 10f, deployColl) as StorageContainer;
                if (lookAt == null || lookAt.IsDestroyed)
                {
                    SendReply(player, "Must be looking at a storage container");
                    return;
                }
                ChestEntity = lookAt;
                SendReply(player, "Set tax chest to: " + (ChestEntity.net.ID.Value));
            }
            else if (arg0Lower == "tp")
            {
                TeleportPlayer(player, ChestEntity?.transform?.position ?? Vector3.zero);
                SendReply(player, "Teleported to tax chest.");
            }
            else
            {
                if (args.Length < 1)
                {
                    SendReply(player, "You must supply a value for the tax rate!");
                    return;
                }
                float tax;
                if (!float.TryParse(args[1], out tax))
                {
                    SendReply(player, "Not a float: " + args[1]);
                    return;
                }
                SendReply(player, "Set tax rate to: <color=#8aff47>" + (TaxRate = tax) + "</color>.");
            }

        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (Physics.Raycast(ray, out hit, maxDist, (coll != -1) ? coll : constructionColl))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true, bool respawnIfDead = false)
        {
            try
            {
                if (player == null || player?.transform == null) return false;
                var playerPos = player?.transform?.position ?? Vector3.zero;
                var isConnected = player?.IsConnected ?? false;
                if (respawnIfDead && player.IsDead())
                {
                    player.RespawnAt(dest, Quaternion.identity);
                    return true;
                }
                var distFrom = Vector3.Distance(playerPos, dest);

                if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
                if (doSleep && isConnected && !player.IsSleeping()) player.StartSleeping();
                player.MovePosition(dest);
                if (isConnected)
                {
                    if (doSleep) player.inventory.crafting.CancelAll(true);
                    player.ClientRPCPlayer(null, player, "ForcePositionTo", dest);
                    if (distFrom >= 250 && distChecks) player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
                    player.UpdateNetworkGroup();
                    player.SendNetworkUpdate();
                    if (distFrom >= 50)
                    {
                        try { player?.ClearEntityQueue(null); } catch { }
                        player.SendFullSnapshot();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete TeleportPlayer^");
                return false;
            }
        }


    }
}