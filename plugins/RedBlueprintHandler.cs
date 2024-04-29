using Facepunch;
using Oxide.Core;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Project Red | Blueprint & Ammo Handler", "Shady", "0.0.9")]
    internal class RedBlueprintHandler : RustPlugin
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
        public static RedBlueprintHandler Instance = null;

        private const int HANDMADE_SHELLS_ID = 588596902;

        private const int PISTOL_BULLET_ID = 785728077;
        private const int PISTOL_FIRE_BULLET_ID = 51984655;
        private const int PISTOL_HV_BULLET_ID = -1691396643;

        private const ulong PISTOL_EMPTY_SKIN_ID = 2632052406;

        private const int RIFLE_BULLET_ID = -1211166256;
        private const int RIFLE_EXPLOSIVE_BULLET_ID = -1321651331;
        private const int RIFLE_HV_BULLET_ID = 1712070256;
        private const int RIFLE_FIRE_BULLET_ID = 605467368;

        private const ulong RIFLE_EMPTY_SKIN_ID = 2632052674;


        private const int BUCKSHOT_ID = -1685290200;
        private const int SLUG_ID = -727717969;
        private const int SHOTGUN_FIRE_ID = -1036635990;

        private const ulong SHOTGUN_EMPTY_SKIN_ID = 2632052134;

        private const string EMPTY_556_DISPLAY_NAME = "Empty 5.56 Cartridges";

        private const string EMPTY_PISTOL_DISPLAY_NAME = "Empty Pistol Cartridges";

        private const string EMPTY_SHOTGUN_DISPLAY_NAME = "Empty Shotgun Shells";

        private const uint REPAIR_BENCH_PREFAB_ID = 3846783416;

        private const uint REPAIR_BENCH_STATIC_PREFAB_ID = 1123731744;

        private readonly HashSet<int> _556AmmoList = new() { RIFLE_BULLET_ID, RIFLE_EXPLOSIVE_BULLET_ID, RIFLE_FIRE_BULLET_ID, RIFLE_HV_BULLET_ID };
        private readonly HashSet<int> _pistolAmmoList = new() { PISTOL_BULLET_ID, PISTOL_FIRE_BULLET_ID, PISTOL_HV_BULLET_ID };
        private readonly HashSet<int> _shotgunAmmoList = new() { BUCKSHOT_ID, SLUG_ID, SHOTGUN_FIRE_ID };

        public enum EmptyAmmoType { None, Rifle, Pistol, Shotgun };

        private readonly Dictionary<string, Timer> _popupTimer = new();

        [PluginReference]
        private readonly Plugin TimedBPBlock;


        private void Init() => Instance = this;

        private void OnServerInitialized()
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null || !p.IsConnected) continue;

                p.gameObject.AddComponent<BlueprintHandler>();

            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            var handler = player?.GetComponent<BlueprintHandler>();
            if (handler != null)
            {
                PrintError("handler is NOT NULL on player connected. we didn't destroy it properly before and this is very bad. stop that. now.");
                return;
            }

            player.gameObject.AddComponent<BlueprintHandler>();

        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            var handler = player?.GetComponent<BlueprintHandler>();
            handler?.DoDestroy();
        }

        private void Unload()
        {
            try
            {

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;

                    p?.GetComponent<BlueprintHandler>()?.DoDestroy();

                }
            }
            finally { Instance = null; }
        }

        private void OnCollectedIngredients(ItemCrafter crafter, ItemCraftTask task)
        {
            if (crafter == null || task == null) return;

            //PrintWarning("on collected");

            Item findAmmo = null;

            var ingredientIds = Pool.Get<HashSet<int>>();
            try
            {
                ingredientIds.Clear();

                for (int i = 0; i < task.blueprint.ingredients.Count; i++)
                {
                    var ingredient = task.blueprint.ingredients[i];
                    ingredientIds.Add(ingredient.itemDef.itemid);
                }

                for (int i = 0; i < task.takenItems.Count; i++)
                {
                    var takenItem = task.takenItems[i];
                    if (!ingredientIds.Contains(takenItem.info.itemid))
                    {
                        PrintWarning("found item we need to collect: " + takenItem.info.displayName.english);
                        findAmmo = takenItem;
                        break;
                    }
                }

            }
            finally { Pool.Free(ref ingredientIds); }

            if (findAmmo == null)
            {
            //    PrintWarning("findAmmo is null!! we probably didn't call this hook for an ammo item");
                return;
            }

            var ammoCollectAmount = GetNeededCartridgeAmount(task.blueprint.targetItem.itemid);

            if (findAmmo.amount > ammoCollectAmount) findAmmo.amount -= ammoCollectAmount;
            else RemoveFromWorld(findAmmo);

        }

        private bool HasAny556Cartridges(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDead()) return false;

            var itemList = Pool.GetList<Item>();
            try
            {
                if (player.inventory.AllItemsNoAlloc(ref itemList) <= 0) return false;

                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList[i];
                    if (item?.info?.itemid == HANDMADE_SHELLS_ID && item.name == EMPTY_556_DISPLAY_NAME)
                        return true;
                }

            }
            finally { Pool.FreeList(ref itemList); }

            return false;
        }

        private bool HasAnyPistolCartridges(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDead()) return false;

            var itemList = Pool.GetList<Item>();
            try
            {
                if (player.inventory.AllItemsNoAlloc(ref itemList) <= 0) return false;

                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList[i];

                    if (item?.info?.itemid == HANDMADE_SHELLS_ID && item.name == EMPTY_PISTOL_DISPLAY_NAME)
                        return true;
                }

            }
            finally { Pool.FreeList(ref itemList); }

            return false;
        }

        private bool HasAnyShotgunCartridges(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDead()) return false;

            var itemList = Pool.GetList<Item>();
            try
            {
                if (player.inventory.AllItemsNoAlloc(ref itemList) <= 0) return false;

                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList[i];
                    if (item?.info?.itemid == HANDMADE_SHELLS_ID && item.name == EMPTY_SHOTGUN_DISPLAY_NAME)
                    {

                        return true;
                    }
                }

            }
            finally { Pool.FreeList(ref itemList); }

            return false;
        }

        private bool IsRifleAmmoBP(ItemBlueprint bp)
        {
            if (bp == null) return false;

            return bp.targetItem.itemid == RIFLE_BULLET_ID || bp.targetItem.itemid == RIFLE_EXPLOSIVE_BULLET_ID || bp.targetItem.itemid == RIFLE_FIRE_BULLET_ID || bp.targetItem.itemid == RIFLE_HV_BULLET_ID;
        }

        private bool IsPistolAmmoBP(ItemBlueprint bp)
        {
            if (bp == null) return false;

            return bp.targetItem.itemid == PISTOL_BULLET_ID || bp.targetItem.itemid == PISTOL_HV_BULLET_ID || bp.targetItem.itemid == PISTOL_FIRE_BULLET_ID;
        }

        private bool IsShotgunAmmoBP(ItemBlueprint bp)
        {
            if (bp == null) return false;

            return bp.targetItem.itemid == SHOTGUN_FIRE_ID || bp.targetItem.itemid == BUCKSHOT_ID || bp.targetItem.itemid == SLUG_ID;
        }

        private int GetNeededCartridgeAmount(int itemId)
        {
            return itemId == PISTOL_BULLET_ID || itemId == PISTOL_HV_BULLET_ID || itemId == PISTOL_FIRE_BULLET_ID || itemId == RIFLE_BULLET_ID || itemId == RIFLE_HV_BULLET_ID ? 3 : 2;
        }

        private object CanCraft(ItemCrafter itemCrafter, ItemBlueprint bp, int amount)
        {
            var player = itemCrafter?.GetComponent<BasePlayer>();
            if (player == null) return null;

            if (bp.defaultBlueprint) //use this only for handling default BPs (as they always appear in crafting menu)
            {
                var bpMsg = TimedBPBlock?.Call("IsBlueprintBlocked", bp.targetItem.itemid)?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(bpMsg)) //still blocked
                {
                    PrintWarning("cancraft false because not yet unlocked!!");
                    TimedBPBlock?.Call("SendBlueprintBlockedFX", player, bpMsg);
                    return false;
                }
            }

            var itemNameToFind = IsRifleAmmoBP(bp) ? EMPTY_556_DISPLAY_NAME : IsPistolAmmoBP(bp) ? EMPTY_PISTOL_DISPLAY_NAME : IsShotgunAmmoBP(bp) ? EMPTY_SHOTGUN_DISPLAY_NAME : string.Empty;
            if (string.IsNullOrEmpty(itemNameToFind))
            {
                PrintWarning("no item name to find (CanCraft)!! (this probably just means the user is crafting something that isn't ammo)");
                return null;
            }
            else PrintWarning("item name to find (CanCraft): " + itemNameToFind);


            var ammoCollectAmount = GetNeededCartridgeAmount(bp.targetItem.itemid);

            var neededTakeAmount = ammoCollectAmount * amount;
            PrintWarning("neededTakeAmount (cancraft): " + neededTakeAmount);


            var itemList = Pool.GetList<Item>();
            try
            {
                if (player.inventory.AllItemsNoAlloc(ref itemList) <= 0) return null;

                var totalCartridgesCount = 0;

                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList[i];

                    if (item?.info?.itemid == HANDMADE_SHELLS_ID && item.name == itemNameToFind)
                    {
                        totalCartridgesCount += item.amount;
                    }
                }

                if (totalCartridgesCount < neededTakeAmount)
                {
                    PrintWarning("CanCraft: total count < needed: " + totalCartridgesCount + " < " + neededTakeAmount);

                    SendLocalEffect(player, "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab");

                    SendReply(player, "You need: " + neededTakeAmount + " cartridges to craft " + amount + " of bullets. You only have: " + totalCartridgesCount);

                    return false;
                }


            }
            finally { Pool.FreeList(ref itemList); }

            PrintWarning("returning null on cancraft");

            return null;
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using var fx = new Effect(effect, player, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private bool TakeCartridgesForCrafting(ItemBlueprint bp, ItemCraftTask task, BasePlayer player)
        {
            if (bp == null || player == null) return false;

            PrintWarning("task amount: " + task.amount + ", numCrafted: " + task.numCrafted);

            var itemNameToFind = IsRifleAmmoBP(bp) ? EMPTY_556_DISPLAY_NAME : IsPistolAmmoBP(bp) ? EMPTY_PISTOL_DISPLAY_NAME : IsShotgunAmmoBP(bp) ? EMPTY_SHOTGUN_DISPLAY_NAME : string.Empty;
            if (string.IsNullOrEmpty(itemNameToFind))
            {
                PrintWarning("no item name to find!!");
                return false;
            }
            else PrintWarning("item name to find: " + itemNameToFind);


            var itemsToTake = Pool.Get<HashSet<Item>>();//new HashSet<Item>();

            try
            {
                itemsToTake.Clear();

                var neededTakeAmount = GetNeededCartridgeAmount(bp.targetItem.itemid) * task.amount;
                PrintWarning("neededTakeAmount: " + neededTakeAmount);

                var itemList = Pool.GetList<Item>();
                try
                {
                    if (player.inventory.AllItemsNoAlloc(ref itemList) <= 0) return false;

                    var totalCartridgesCount = 0;
                    var cartridgesToTakeFrom = new HashSet<Item>();

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var item = itemList[i];

                        if (item?.info?.itemid == HANDMADE_SHELLS_ID && item.name == itemNameToFind)
                        {
                            totalCartridgesCount += item.amount;
                            cartridgesToTakeFrom.Add(item);
                        }
                    }

                    if (totalCartridgesCount < neededTakeAmount)
                    {
                        PrintWarning("total count < needed: " + totalCartridgesCount + " < " + neededTakeAmount);
                        return false;
                    }



                    foreach (var item in cartridgesToTakeFrom)
                    {
                        if (totalCartridgesCount <= 0) break;


                        var addItem = item;

                        var takeAmount = item.amount > neededTakeAmount ? neededTakeAmount : item.amount;

                        if (takeAmount < item.amount)
                        {
                            addItem = ItemManager.Create(item.info, takeAmount, item.skin);
                            addItem.name = item.name;

                            item.amount -= takeAmount;
                            item.MarkDirty();
                        }

                        totalCartridgesCount -= takeAmount;
                        itemsToTake.Add(addItem);

                    }

                }
                finally { Pool.FreeList(ref itemList); }

                foreach (var item in itemsToTake)
                {
                    item.RemoveFromContainer();

                    item.CollectedForCrafting(player);

                    //removed in aug 3 '23 rust update. please see if any replacement exists/is necessary
                    /*/
                    if (!task.potentialOwners.Contains(player.userID))
                        task.potentialOwners.Add(player.userID);
                    /*/

                    task.takenItems.Add(item);
                }
            }
            finally { Facepunch.Pool.Free(ref itemsToTake); }




            return true;
        }

        private bool IsBlueprintPermanentlyBlocked(int targetItemId)
        {
            return TimedBPBlock?.Call<bool>("IsBlueprintPermanentlyBlocked", targetItemId) ?? false;
        }

        private bool IsBlueprintUnlockedYet(int targetItemId)
        {
            return TimedBPBlock?.Call<bool>("IsBlueprintUnlockedYet", targetItemId) ?? false;
        }


        private void OnPlayerTryStudyBlueprint(BasePlayer player, Item item)
        {
            if (player?.blueprints != null && item?.blueprintTarget != 0 && player.blueprints.IsUnlocked(item.blueprintTargetDef))
                TimedBPBlock?.Call("OnPlayerStudyBlueprint", player, item);
        }

        private object OnItemCraft(ItemCraftTask task, BasePlayer player, Item item)
        {
            var bp = task.blueprint;

            var targetItemId = task?.blueprint?.targetItem?.itemid ?? 0;

            if ((task?.blueprint?.targetItem?.category ?? ItemCategory.All) != ItemCategory.Ammunition)
            {
                if (!IsBlueprintUnlockedYet(targetItemId))
                {

                    var itemList = Pool.GetList<Item>();
                    try
                    {
                        itemList.Clear();

                        if (player.inventory.AllItemsNoAlloc(ref itemList) <= 0)
                        {
                            PrintWarning("player has no items on OnItemCraft?!?!?");
                            return null;
                        }

                        Item findBp = null;

                        for (int i = 0; i < itemList.Count; i++)
                        {
                            var itemInList = itemList[i];
                            PrintWarning("blueprintTarget: " + itemInList?.blueprintTarget + " == targetItemId?: " + targetItemId);

                            if (itemInList?.blueprintTarget == targetItemId)
                            {
                                findBp = itemInList;
                                break;
                            }
                        }

                        if (findBp == null)
                        {
                            player.SendConsoleCommand("craft.cancel");
                            PrintWarning("couldn't take one time BP!!");
                            return true;
                        }
                        else
                        {

                            var takeBp = findBp.amount < 2 ? findBp : ItemManager.Create(findBp.info, 1);

                            if (findBp.amount > 1)
                            {
                                findBp.amount -= 1;
                                findBp.MarkDirty();

                               
                                takeBp.blueprintTarget = findBp.blueprintTarget;
                              
                            }

                            takeBp.RemoveFromContainer();
                            task.takenItems.Add(takeBp);

                            PrintWarning("removed bp for craft!");

                        }

                    }
                    finally { Pool.FreeList(ref itemList); }



                }
            }
            else PrintWarning("not taking BP on craft for ammo item: " + task?.blueprint?.targetItem?.displayName?.english);


            PrintWarning("targetItemId: " + targetItemId);



            if (!IsRifleAmmoBP(bp) && !IsPistolAmmoBP(bp) && !IsShotgunAmmoBP(bp))
            {

                return null;
            }
            else if (!TakeCartridgesForCrafting(task.blueprint, task, player))
            {
                player.SendConsoleCommand("craft.cancel");
                PrintWarning("couldn't take cartridges!!");
                return true;
            }

            return null;
        }


        private object CanUnlockTechTreeNode(BasePlayer player, TechTreeData.NodeInstance node, TechTreeData techTree)
        {
            var str = "The tech tree is, unfortunately, disabled on this server due to incompatibility with our custom Blueprint system. We wish we could fix this, but it's client-side things that we can't touch.";
            SendReply(player, str);
            ShowPopup(player.UserIDString, "Unfortunately, the tech tree must be disabled on this server.");
            SendLocalEffect(player, "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab");
            return false;
        }

        void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (item == null) return;

            if (item.info.itemid != HANDMADE_SHELLS_ID || item.skin == 0) return;

            if (item.skin == RIFLE_EMPTY_SKIN_ID) item.name = EMPTY_556_DISPLAY_NAME;
            else if (item.skin == PISTOL_EMPTY_SKIN_ID) item.name = EMPTY_PISTOL_DISPLAY_NAME;
            else if (item.skin == SHOTGUN_EMPTY_SKIN_ID) item.name = EMPTY_SHOTGUN_DISPLAY_NAME;

        }

        void OnItemRemovedFromContainer(ItemContainer container, Item item)
        {
            if (item == null) return;

            if (item.info.itemid != HANDMADE_SHELLS_ID || item.skin == 0) return;

            if (item.skin == RIFLE_EMPTY_SKIN_ID) item.name = EMPTY_556_DISPLAY_NAME;
            else if (item.skin == PISTOL_EMPTY_SKIN_ID) item.name = EMPTY_PISTOL_DISPLAY_NAME;
            else if (item.skin == SHOTGUN_EMPTY_SKIN_ID) item.name = EMPTY_SHOTGUN_DISPLAY_NAME;
        }

        void OnItemDropped(Item item, BaseEntity entity)
        {
            if (item == null) return;

            if (item.info.itemid != HANDMADE_SHELLS_ID || item.skin == 0) return;

            if (item.skin == RIFLE_EMPTY_SKIN_ID) item.name = EMPTY_556_DISPLAY_NAME;
            else if (item.skin == PISTOL_EMPTY_SKIN_ID) item.name = EMPTY_PISTOL_DISPLAY_NAME;
            else if (item.skin == SHOTGUN_EMPTY_SKIN_ID) item.name = EMPTY_SHOTGUN_DISPLAY_NAME;
        }

        private object CanStackItem(Item item, Item targetItem)
        {
            if (item.info.itemid == HANDMADE_SHELLS_ID && targetItem.info.itemid == HANDMADE_SHELLS_ID)
            {
                //  PrintWarning("canstack called for: " + item.name + " (" + item?.info?.displayName?.english + ") -> " + targetItem.name + " (" + targetItem?.info?.displayName?.english + ")");
                if (!string.IsNullOrEmpty(item.name) || !string.IsNullOrEmpty(targetItem.name))
                {
                    if (item.name != targetItem.name || item.skin != targetItem.skin)
                    {
                        //       PrintWarning("denying a stack. item: " + (item?.name ?? item?.info?.displayName?.english) + " has targetItem: " + (targetItem?.name ?? targetItem?.info?.displayName?.english));

                        return false;
                    }
                }
            }


            return null;
        }


        private object CanCombineDroppedItem(DroppedItem dropped, DroppedItem targetDropped)
        {

            var item = dropped?.item;
            var targetItem = targetDropped?.item;
            if (item == null || targetItem == null || !(item.info.itemid == HANDMADE_SHELLS_ID && targetItem.info.itemid == HANDMADE_SHELLS_ID)) return null;

            if (!string.IsNullOrEmpty(item.name) || !string.IsNullOrEmpty(targetItem.name))
            {
                if (item.name != targetItem.name || item.skin != targetItem.skin) return false;
            }

            return null;
        }


        private class BlueprintHandler : FacepunchBehaviour
        {
            public BasePlayer Player { get; set; }

            public ulong UserID => Player?.userID ?? 0;

            public ProtoBuf.PersistantPlayer GetPlayerInfo()
            {
                return ServerMgr.Instance?.persistance?.GetPlayerInfo(UserID); //weird things may have happened with the 'cached' version below. perf diff is minimal as far as i can tell.
            }

            private void Awake()
            {
                Player = GetComponent<BasePlayer>();

                if (Player == null || !Player.IsConnected)
                {
                    Interface.Oxide.LogError(nameof(BlueprintHandler) + "." + nameof(Awake) + " has failed, player is null or disconnected!!!!");
                    DoDestroy();
                    return;
                }

                InvokeRepeating(CheckBlueprints, 4f, 1f);

                Interface.Oxide.LogWarning("Finished Awake() for BlueprintHandler");
            }

            public void CheckBlueprints()
            {
                var watch = Pool.Get<Stopwatch>();
                try
                {
                    watch.Restart();

                    var playerInfo = GetPlayerInfo();

                    var itemList = Pool.Get<HashSet<Item>>();
                    try
                    {
                        itemList.Clear();

                        for (int i = 0; i < Player.inventory.containerMain.itemList.Count; i++)
                            itemList.Add(Player.inventory.containerMain.itemList[i]);

                        for (int i = 0; i < Player.inventory.containerBelt.itemList.Count; i++)
                            itemList.Add(Player.inventory.containerBelt.itemList[i]);
                        

                        if (itemList.Count < 1)
                            return;

                        var shouldHaveBps = Pool.Get<HashSet<int>>();
                        try
                        {
                            shouldHaveBps.Clear();

                            var hasEmpty556 = false;
                            var hasEmptyPistol = false;
                            var hasEmptyShells = false;

                            var isLootingRepairBench = (Player?.inventory?.loot?.entitySource?.prefabID ?? 0) switch
                            {
                                REPAIR_BENCH_PREFAB_ID or REPAIR_BENCH_STATIC_PREFAB_ID => true,
                                _ => false,
                            };

                            if (isLootingRepairBench) //if looting repair bench, we want players to have all BPs so that they can actually repair their items
                            {
                                for (int i = 0; i < ItemManager.itemList.Count; i++) //could be sped up by storing a hashset of user craftable bps
                                {
                                    var item = ItemManager.itemList[i];

                                    if (item?.Blueprint != null && item.Blueprint.userCraftable)
                                        shouldHaveBps.Add(item.itemid);

                                }
                            }
                            else
                            {
                                foreach (var item in itemList)
                                {
                                    if (item.info.itemid != HANDMADE_SHELLS_ID) continue;

                                    if (item.name == EMPTY_556_DISPLAY_NAME) hasEmpty556 = true;
                                    else if (item.name == EMPTY_PISTOL_DISPLAY_NAME) hasEmptyPistol = true;
                                    else if (item.name == EMPTY_SHOTGUN_DISPLAY_NAME) hasEmptyShells = true;

                                    if (hasEmpty556 && hasEmptyPistol && hasEmptyShells) break; //perf
                                }

                                foreach (var item in itemList)
                                {
                                    if (item.blueprintTarget == 0)
                                        continue;

                                    if (Instance._556AmmoList.Contains(item.blueprintTarget) && !hasEmpty556) continue;
                                    else if (Instance._pistolAmmoList.Contains(item.blueprintTarget) && !hasEmptyPistol) continue;
                                    else if (Instance._shotgunAmmoList.Contains(item.blueprintTarget) && !hasEmptyShells) continue;

                                    shouldHaveBps.Add(item.blueprintTarget);
                                }
                            }


                         

                            //    Interface.Oxide.LogWarning("has 556: " + hasEmpty556 + ", pistol: " + hasEmptyPistol + ", shells: " + hasEmptyShells);


                            //first, we delete all bps the user SHOULDN'T have
                            var shouldUpdate = false;
                            for (int i = 0; i < playerInfo.unlockedItems.Count; i++)
                            {

                                var unlocked = playerInfo.unlockedItems[i];

                                if (shouldHaveBps.Contains(unlocked))
                                    continue;

                                var isUnlocked = Instance?.IsBlueprintUnlockedYet(unlocked) ?? false;
                                if (isUnlocked) 
                                    continue;

                                //check if it's even a perma locked BP or not. if it isn't, we ignore
                                //  var perma = Instance?.IsBlueprintPermanentlyBlocked(unlocked) ?? false;
                                //  var perma = Instance?.TimedBPBlock?.Call<bool>("IsBlueprintPermanentlyBlocked", unlocked) ?? false;

                                //  if (!perma) continue;


                                if (playerInfo.unlockedItems.Remove(unlocked)) 
                                    shouldUpdate = true;
                            }


                            foreach (var item in shouldHaveBps)
                            {
                                if (playerInfo.unlockedItems.Contains(item))
                                    continue;

                                var canBeLearned = Instance?.IsBlueprintUnlockedYet(item) ?? false;
                                if (canBeLearned) //if it can be learned, MAKE THE PLAYER LEARN IT. this is to allow the study button to appear.
                                    continue;

                                //  var perma = Instance?.TimedBPBlock?.Call<bool>("IsBlueprintPermanentlyBlocked", item) ?? false;

                                //     if (!perma) continue;

                                shouldUpdate = true;
                                playerInfo.unlockedItems.Add(item);
                            }

                            if (shouldUpdate)
                            {
                                ServerMgr.Instance.persistance.SetPlayerInfo(Player.userID, playerInfo);
                                Player.PersistantPlayerInfo = playerInfo;
                                Player.ClientRPCPlayer(null, Player, "UnlockedBlueprint", 0);
                                Player.SendNetworkUpdate();
                            }

                        }
                        catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                        finally { Pool.Free(ref shouldHaveBps); }
                    }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                    finally { Pool.Free(ref itemList); }
                }
                catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                finally
                {
                    try
                    {
                        if (watch.Elapsed.TotalMilliseconds > 1) Interface.Oxide.LogWarning(nameof(CheckBlueprints) + " took: " + watch.Elapsed.TotalMilliseconds + "ms");
                    }
                    finally { Pool.Free(ref watch); }

                }

            }

            public void DoDestroy()
            {
                try { InvokeHandler.CancelInvoke(this, CheckBlueprints); }
                finally { Destroy(this); }
            }

        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private Item CreateEmptyAmmoItem(EmptyAmmoType type, int amount = 1)
        {
            if (type == EmptyAmmoType.None) throw new ArgumentOutOfRangeException(nameof(type));
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));

            var item = ItemManager.CreateByItemID(HANDMADE_SHELLS_ID, amount, type == EmptyAmmoType.Rifle ? RIFLE_EMPTY_SKIN_ID : type == EmptyAmmoType.Pistol ? PISTOL_EMPTY_SKIN_ID : SHOTGUN_EMPTY_SKIN_ID);

            item.name = type == EmptyAmmoType.Rifle ? EMPTY_556_DISPLAY_NAME : type == EmptyAmmoType.Pistol ? EMPTY_PISTOL_DISPLAY_NAME : EMPTY_SHOTGUN_DISPLAY_NAME;

            return item;
        }

        private string GetEmptyAmmoDisplayName(EmptyAmmoType type)
        {
            if (type == EmptyAmmoType.None) throw new ArgumentOutOfRangeException(nameof(type));

            return type == EmptyAmmoType.Rifle ? EMPTY_556_DISPLAY_NAME : type == EmptyAmmoType.Pistol ? EMPTY_PISTOL_DISPLAY_NAME : EMPTY_SHOTGUN_DISPLAY_NAME;
        }

        private ulong GetEmptyAmmoSkin(EmptyAmmoType type)
        {
            if (type == EmptyAmmoType.None) throw new ArgumentOutOfRangeException(nameof(type));

            return type == EmptyAmmoType.Rifle ? RIFLE_EMPTY_SKIN_ID : type == EmptyAmmoType.Pistol ? PISTOL_EMPTY_SKIN_ID : SHOTGUN_EMPTY_SKIN_ID;
        }

        [ChatCommand("ammotest")]
        private void cmdAmmoTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var empty556 = CreateEmptyAmmoItem(EmptyAmmoType.Rifle, 5);
            empty556.name = EMPTY_556_DISPLAY_NAME;

            var emptyPistol = CreateEmptyAmmoItem(EmptyAmmoType.Pistol, 5);
            emptyPistol.name = EMPTY_PISTOL_DISPLAY_NAME;

            var emptyShotgun = CreateEmptyAmmoItem(EmptyAmmoType.Shotgun, 5);
            emptyShotgun.name = EMPTY_SHOTGUN_DISPLAY_NAME;

            if (!empty556.MoveToContainer(player.inventory.containerMain))
            {
                RemoveFromWorld(empty556);
            }

            if (!emptyPistol.MoveToContainer(player.inventory.containerMain))
            {
                RemoveFromWorld(emptyPistol);
            }

            if (!emptyShotgun.MoveToContainer(player.inventory.containerMain))
            {
                RemoveFromWorld(emptyShotgun);
            }

        }

        private void OnLootEntityEnd(BasePlayer player, BaseEntity entity)
        {
            if (player == null || entity == null || (entity.prefabID != REPAIR_BENCH_PREFAB_ID && entity.prefabID != REPAIR_BENCH_STATIC_PREFAB_ID))
                return;

            //repair bench. force update of bps to prevent exploit (we must grant BP when repair bench is open so player can repair without having BP actually unlocked)

            player?.GetComponent<BlueprintHandler>()?.CheckBlueprints();
        }

        private void OnLootSpawn(LootContainer container)
        {
            if (container == null)
            {
                PrintWarning("Container called null for OnLootSpawn");
                return;
            }
            var prefabName = container?.ShortPrefabName ?? string.Empty;
            if (prefabName.StartsWith("dm ")) return;

            var prefabId = container?.prefabID ?? 0;

            var pos = container?.transform?.position ?? Vector3.zero;
            timer.Once(0.5f, () =>
            {
                if (container == null || container.IsDestroyed || container.gameObject == null || container.inventory == null) return;
                var watch = Pool.Get<Stopwatch>();
                try
                {
                    watch.Restart();

                    var typeOfAmmo = (EmptyAmmoType)UnityEngine.Random.Range(1, 4);

                    var chanceToSpawn = typeOfAmmo == EmptyAmmoType.Rifle ? 3 : typeOfAmmo == EmptyAmmoType.Pistol ? 6 : 2;
                    if (UnityEngine.Random.Range(0, 101) <= chanceToSpawn)
                    {
                        // PrintWarning("met chance to spawn: " + chanceToSpawn + " for: " + typeOfAmmo);

                        var amountOfAmmoMin = typeOfAmmo == EmptyAmmoType.Rifle ? UnityEngine.Random.Range(2, 4) : typeOfAmmo == EmptyAmmoType.Pistol ? UnityEngine.Random.Range(2, 5) : UnityEngine.Random.Range(1, 4); ;
                        var amountOfAmmoMax = typeOfAmmo == EmptyAmmoType.Rifle ? UnityEngine.Random.Range(3, 13) : typeOfAmmo == EmptyAmmoType.Pistol ? UnityEngine.Random.Range(4, 19) : UnityEngine.Random.Range(3, 9);


                        var ammoAmount = UnityEngine.Random.Range(amountOfAmmoMin, amountOfAmmoMax);

                        var ammoTypeName = GetEmptyAmmoDisplayName(typeOfAmmo);
                        var ammoTypeSkin = GetEmptyAmmoSkin(typeOfAmmo);

                        var lootEnt = container;
                        Item findAmmo = null;
                        for (int i = 0; i < lootEnt.inventory.itemList.Count; i++)
                        {
                            var item = lootEnt.inventory.itemList[i];
                            if (item?.info?.itemid == HANDMADE_SHELLS_ID && item?.name == ammoTypeName)
                            {
                                findAmmo = item;
                                break;
                            }
                        }

                        if (findAmmo == null)
                        {
                            findAmmo = CreateEmptyAmmoItem(typeOfAmmo, ammoAmount);
                            if (!findAmmo.MoveToContainer(container.inventory))
                            {
                                PrintWarning("failed to move to container!!! (ammo: " + typeOfAmmo + " x" + ammoAmount);
                                RemoveFromWorld(findAmmo);
                            }
                        }
                        else
                        {
                            PrintWarning("findAmmo already existed! for gc reasons we just add");
                            findAmmo.amount += ammoAmount;
                            findAmmo.MarkDirty();
                        }
                    }




                }
                finally
                {
                    try
                    {
                        if (watch.ElapsedMilliseconds >= 4)
                        {
                            var sb = Pool.Get<StringBuilder>();
                            try { PrintWarning(sb.Clear().Append(nameof(OnLootSpawn)).Append(" took: ").Append(watch.ElapsedMilliseconds.ToString("0.##")).Append("ms").ToString()); }
                            finally { Pool.Free(ref sb); }
                        }
                    }
                    finally { Pool.Free(ref watch); }
                }

            });
        }

        private void ShowPopup(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException(nameof(Id));
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));

            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;

            player.Command("gametip.showgametip", msg);

            Timer endTimer;
            if (_popupTimer.TryGetValue(Id, out endTimer)) endTimer.Destroy();

            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });

            _popupTimer[Id] = endTimer;
        }

    }
}
