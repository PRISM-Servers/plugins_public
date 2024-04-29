using Facepunch;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GiftSystem", "Shady", "2.0.23", ResourceId = 0)]
    internal class GiftSystem : RustPlugin
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
        private const string CHAT_STEAM_ID = "76561198865536053";

        private const string PRISM_RAINBOW_TXT = "<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color>";

        private const uint FOOD_BOX_CRATE_PREFAB_ID = 2896170989;

        private const uint FOOD_CRATE_PREFAB_ID_1 = 808303766;
        private const uint FOOD_CRATE_PREFAB_ID_2 = 3249643118;

        private const uint VEHICLE_PARTS_PREFAB_ID = 356724277;

        private const uint BARREL_1_PREFAB_ID = 3364121927;
        private const uint BARREL_2_PREFAB_ID = 3269883781;
        private const uint BARREL_3_PREFAB_ID = 966676416;
        private const uint BARREL_4_PREFAB_ID = 555882409;
        private const uint OIL_BARREL_PREFAB_ID = 3438187947;

        private const uint TRASH_PILE_1_PREFAB_ID = 615147957;

        private const uint MINECART_PREFAB_ID = 1768976424;
        private const uint CRATE_MINE_PREFAB_ID = 1071933290;


        private const uint CRATE_NORMAL_2_FOOD_PREFAB_ID = 2066926276;

        private const uint CRATE_UNDERWATER_BASIC_PREFAB_ID = 3852690109;

        [PluginReference]
        private readonly Plugin Deathmatch;

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly List<LootContainer> _lootContainers = new List<LootContainer>();

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));

            AddCovalenceCommand("gift", nameof(cmdGiftQueue));

            timer.Every(825f, () => DoGifts());
        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));
            Subscribe(nameof(OnEntityKill));

            foreach(var entity in BaseNetworkable.serverEntities)
            {
                var loot = entity as LootContainer;
                if (loot != null) _lootContainers.Add(loot);
            }
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var loot = entity as LootContainer;
            if (loot != null) _lootContainers.Add(loot);
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            var loot = entity as LootContainer;
            if (loot != null) _lootContainers.Remove(loot);
        }

        private readonly Dictionary<string, List<ItemAmount>> _giftQueue = new Dictionary<string, List<ItemAmount>>();


        [ConsoleCommand("gift.force")]
        private void consoleGiftForce(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null)
            {
                SendReply(arg, "You do not have permission to use this command!");
                return;
            }
            DoGifts();
            SendReply(arg, "Did gifts");
        }

        [ChatCommand("giftall")] //formerly "gift", replaced with gift queue system
        private void CmdGiveGift(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length == 0 || args.Length == 1)
            {
                SendReply(player, "Invalid syntax, example: /gift itemname amount");
                return;
            }

            int giveamount;
            if (!int.TryParse(args[1], out giveamount) || giveamount < 1)
            {
                SendReply(player, "You must give 1 or more of this item.");
                return;
            }

            if (!GiveGiftAllByName(args[0].ToLower(), giveamount))
            {
                SendReply(player, "Unable to create item to give to all players. Did you type the shortname incorrectly?");
                return;
            }

        }


        private void cmdGiftQueue(IPlayer player, string command, string[] args)
        {
            if (player == null || player.IsServer) return;
            var pObj = player?.Object as BasePlayer;
            if (pObj == null) return;

            var queue = GetGiftQueue(player.Id);
            if (player.IsAdmin && args != null && args.Length > 0)
            {
                if (args[0].Equals("test", StringComparison.OrdinalIgnoreCase))
                {
                    if (queue == null)
                    {
                        queue = new List<ItemAmount>();
                        SetGiftQueue(player.Id, queue);
                    }

                    SendReply(player, "Added 1k wood to queue");

                    var woodAmount = new ItemAmount(ItemManager.FindItemDefinition("wood"))
                    {
                        amount = 1000,
                        startAmount = 1000
                    };

                    queue.Add(woodAmount);

                    return;
                }
                else if (args[0].Equals("test2", StringComparison.OrdinalIgnoreCase))
                {
                    var rngAmount = GetRandomDynamicGift();

                    var item = ItemManager.Create(rngAmount.itemDef, (int)rngAmount.amount);
                    player.Command("note.inv " + item.info.itemid + " " + item.amount);

                    var inventory = pObj?.inventory;

                    if (!item.MoveToContainer(inventory.containerMain) && !item.MoveToContainer(inventory.containerBelt))
                    {
                        player.Command("note.inv " + item.info.itemid + " -" + item.amount);
                        RemoveFromWorld(item);
                    }

                    return;
                }
               
             
            }

          

            if (queue == null || queue.Count < 1)
            {
                SendReply(player, "You do not currently have any items in your gift queue. Items will only go to this queue if your inventory was full when it was given. <color=yellow>We promise we'll gift you something soon!</color> <color=red><3</color>");
                return;
            }

            var dropPos = pObj?.GetDropPosition() ?? Vector3.zero;
            var dropVel = pObj?.GetDropVelocity() ?? Vector3.zero;
            var dropRot = pObj?.ServerRotation ?? Quaternion.identity;

            var queueCount = queue.Count;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try 
            {
                sb.Clear();

                if (args == null || args.Length < 1)
                {
                    SendReply(player, "Current gift queue:");
                    for (int i = 0; i < queueCount; i++)
                    {
                        var item = queue[i];
                        if (item == null || item.amount < 1)
                        {
                            PrintWarning("null item or item with amount < 1 in gift queue!!!");
                            continue;
                        }

                        sb.Append(i + 1).Append(": <color=#75ff7e>").Append((item?.itemDef?.displayName?.english ?? "Unknown item name")).Append(" </color>x<color=#ff61dd>").Append(item.amount.ToString("N0")).Append("</color>").Append(Environment.NewLine);
                    }

                    sb.Length -= 1; //trim newline

                    SendReply(player, sb.ToString());
                    SendReply(player, "You can claim any of these gifts by using the corresponding number, like so: <color=orange>/" + command + " 3</color> - This would claim the 3rd gift you have queued.\nAlternatively, you may claim all gifts with <color=orange>/" + command + " all</color>. Make sure you have plenty of free space, or it will drop!");
                    return;
                }

                var newQueue = new List<ItemAmount>(queue);
                if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
                {

                    for (int i = 0; i < queueCount; i++)
                    {
                        var item = queue[i];
                        if (item == null || item.amount < 1)
                        {
                            PrintWarning("null item or item with amount < 1 in gift queue!!!");
                            continue;
                        }
                        newQueue.Remove(item);
                        player.Command(sb.Clear().Append("note.inv ").Append(item.itemDef.itemid).Append(" ").Append((int)item.amount).ToString());

                        var actualItem = ItemManager.Create(item.itemDef, (int)item.amount);
                        if (!actualItem.MoveToContainer(pObj.inventory.containerMain) && !actualItem.MoveToContainer(pObj.inventory.containerBelt))
                        {
                            var drop = actualItem.Drop(dropPos, dropVel, dropRot);
                            if (drop == null)
                            {
                                RemoveFromWorld(actualItem);
                            }
                            else player.Command(sb.Clear().Append("note.inv ").Append(item.itemDef.itemid).Append(" -").Append((int)item.amount).ToString());
                        }
                    }
                    SendReply(player, sb.Clear().Append("Claimed all ").Append(queueCount.ToString("N0")).Append(" gifts!").ToString());
                }
                else
                {
                    int target;
                    if (!int.TryParse(args[0], out target))
                    {
                        SendReply(player, "Not an integer: " + args[0] + " - Type <color=orange>/" + command + "</color> for more info.");
                        return;
                    }
                    if (target < 1)
                    {
                        SendReply(player, "Invalid number specified. Number should be greater than zero.");
                        return;
                    }
                    if (target > queueCount)
                    {
                        SendReply(player, "Invalid number specified. Number should be lesser than or equal to " + queueCount.ToString("N0") + " (your total gifts)");
                        return;
                    }
                    var indexItem = queue[target - 1];
                    if (indexItem == null || indexItem.amount < 1)
                    {
                        PrintWarning("indexItem was null or amount < 1 !!!");
                        SendReply(player, "Something went terribly wrong with this item. Please report this to an administrator!");
                        return;
                    }
                    newQueue.Remove(indexItem);


                    player.Command(sb.Clear().Append("note.inv ").Append(indexItem.itemDef.itemid).Append(" ").Append((int)indexItem.amount).ToString());

                    var actualItem = ItemManager.Create(indexItem.itemDef, (int)indexItem.amount);
                    if (!actualItem.MoveToContainer(pObj.inventory.containerMain) && !actualItem.MoveToContainer(pObj.inventory.containerBelt))
                    {
                        var drop = actualItem.Drop(dropPos, dropVel, dropRot);
                        if (drop == null)
                        {
                            RemoveFromWorld(actualItem);
                        }
                        else
                        {
                            player.Command(sb.Clear().Append("note.inv ").Append(indexItem.itemDef.itemid).Append(" -").Append((int)indexItem.amount).ToString());
                            SendReply(player, "Your gift was dropped because your inventory was full.");
                        }
                    }


                }

                SetGiftQueue(player.Id, newQueue);
            }
            finally { Facepunch.Pool.Free(ref sb); }

          
        }

        private void BroadcastMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                player.SendConsoleCommand("chat.add", string.Empty, 0, msg);
            }
        }

        private bool GiveGiftAll(ItemAmount itemAmount)
        {
            if (itemAmount == null) throw new ArgumentNullException(nameof(itemAmount));
            if (itemAmount.amount <= 0) throw new ArgumentOutOfRangeException(nameof(itemAmount));

            var sb = Pool.Get<StringBuilder>();

            try 
            {

                var itemName = itemAmount?.itemDef?.displayName?.english ?? string.Empty;
                var intAmt = (int)itemAmount.amount;

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || player?.inventory == null || player.IsDead() || !player.IsConnected) continue;


                   

                    if (TryGiveGiftItemAmount(player, itemAmount))
                    {
                        player.SendConsoleCommand(sb.Clear().Append("note.inv ").Append(itemAmount.itemDef.itemid).Append(" ").Append(intAmt).ToString());

                        var giftMsg = sb.Clear().Append(PRISM_RAINBOW_TXT).Append(": You've received a random gift (").Append(itemName).Append(" x").Append(intAmt.ToString("N0")).Append("). Thanks for playing!").ToString();
                        SendReply(player, giftMsg);
                    }
                    else
                    {
                        AddGiftQueue(player.UserIDString, itemAmount);

                        var giftQueueMsg = sb.Clear().Append(PRISM_RAINBOW_TXT).Append(":You've received a random gift (").Append(itemName).Append(" x").Append(intAmt.ToString("N0")).Append("), it has been added to your gift queue! (<color=yellow>/gift</color>). Thanks for playing!").ToString();

                        SendReply(player, giftQueueMsg);

                        //itemGive.Drop(dropPos, dropVelocity, dropRot);
                        //player.SendConsoleCommand("chat.add 0 " + "\"" + "<color=#42dfff>SERVER</color>:<color=red> ♥ </color>You've received a random gift (" + itemName + " x" + giveamount.ToString("N0") + "), it was dropped on the ground because your inventory is full! -- Thanks for playing." + "\"");
                    }


                    ShowPopup(player.UserIDString, sb.Clear().Append("<color=#E9388C>").Append(itemName).Append("</color> x").Append(intAmt.ToString("N0")).ToString(), 8f);
                }
            }
            finally { Pool.Free(ref sb); }

          

            return true;
        }


        private bool GiveGiftAllByName(string shortName, int amount)
        {
            if (string.IsNullOrEmpty(shortName))
            {
                PrintWarning("giveitem was null or empty, amount: " + amount);
               // BroadcastMessage("<color=#42dfff>SERVER</color>:<color=red> ♥ </color>Thanks for playing! -- <color=orange>prismrust.enjin.com</color> - <color=#42dfff>kngsgaming.network</color> - <color=yellow>crackedrustservers.enjin.com</color>");
                return false;
            }
            if (amount < 1)
            {
                PrintWarning("GIVE AMOUNT < 1!");
                return false;
            }

            var emptyItemDef = ItemManager.FindItemDefinition(shortName);
            if (emptyItemDef == null)
            {
                PrintWarning("ITEM DEF IS NULL FOR: " + shortName + "!");
                return false;
            }

            var itemGive = new ItemAmount(emptyItemDef)
            {
                amount = amount,
                startAmount = amount
            };

            return GiveGiftAll(itemGive);
            

        
        }

        private int _repeatNullAmounts = 0;
        //here's the idea: we find a random loot crate on the map and we grab a random item out of it (that isn't scrap!) and we gift it to all the players. 
        private ItemAmount GetRandomDynamicGift()
        {
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                if (_repeatNullAmounts >= 500)
                {
                    PrintWarning("_repeatNullAmounts >= 500!!!");
                    return null;
                }

                Item rngItem = null;
                LootContainer rngLoot = null;

                var lootContainers = Facepunch.Pool.GetList<LootContainer>();
                try
                {
                    for (int i = 0; i < _lootContainers.Count; i++)
                    {
                        var loot = _lootContainers[i];
                        if (loot == null || !loot.isLootable || loot.prefabID == VEHICLE_PARTS_PREFAB_ID || loot.prefabID == FOOD_CRATE_PREFAB_ID_1 || loot.prefabID == FOOD_CRATE_PREFAB_ID_2 || loot.prefabID == FOOD_BOX_CRATE_PREFAB_ID || loot.prefabID == BARREL_1_PREFAB_ID || loot.prefabID == BARREL_2_PREFAB_ID || loot.prefabID == BARREL_3_PREFAB_ID || loot.prefabID == BARREL_4_PREFAB_ID || loot.prefabID == OIL_BARREL_PREFAB_ID || loot.prefabID == TRASH_PILE_1_PREFAB_ID || loot.prefabID == CRATE_UNDERWATER_BASIC_PREFAB_ID || loot.prefabID == MINECART_PREFAB_ID || loot.prefabID == CRATE_MINE_PREFAB_ID || loot.prefabID == CRATE_NORMAL_2_FOOD_PREFAB_ID || HasAnyLooters(loot)) continue;

                        if ((loot?.inventory?.itemList?.Count ?? 0) < 1) continue;

                        lootContainers.Add(loot);

                    }

                    if (lootContainers.Count < 1)
                    {
                        PrintWarning("NO VIABLE LOOT CONTAINERS?!");
                        return null;
                    }

                    rngLoot = lootContainers[UnityEngine.Random.Range(0, lootContainers.Count)];

                    var itemList = rngLoot?.inventory?.itemList;
                    var itemCount = itemList?.Count ?? 0;

                    var rng = itemCount == 1 ? itemList[0] : itemList[UnityEngine.Random.Range(0, itemCount)];

                    var whileCount = 0;

                    while (!(rng?.info != null && rng.info.shortname != "scrap" && rng.info.category != ItemCategory.Weapon && rng.blueprintTarget == 0 && rng.info.shortname != "note"))
                    {
                        rng = null;
                        if (itemCount < 2)
                        {
                            PrintWarning("item count < 2 on while - immediate break");
                            break;
                        }

                        if (whileCount > 25)
                        {
                            //   PrintWarning("whileCount for item reached 5! rngLoot: " + rngLoot?.ShortPrefabName + " @ " + rngLoot.transform.position);
                            break;
                        }

                        whileCount++;

                        rng = itemList[UnityEngine.Random.Range(0, itemCount)];

                    }

                    rngItem = rng;
                }
                finally { Facepunch.Pool.FreeList(ref lootContainers); }

                if (rngItem == null)
                {
                    _repeatNullAmounts++;
                    return GetRandomDynamicGift();
                }
                else
                {
                    rngLoot?.Invoke(() =>
                    {
                        if (rngLoot == null || rngLoot.IsDestroyed || rngLoot.gameObject == null) return;
                        rngLoot.PopulateLoot();
                    }, 5f);
                }

                var itemAmount = new ItemAmount(rngItem.info)
                {
                    amount = rngItem.amount,
                    startAmount = rngItem.amount
                };

                RemoveFromWorld(rngItem);

                _repeatNullAmounts = 0;

                return itemAmount;
            }
            finally 
            {
                try { if (watch.ElapsedMilliseconds > 2) PrintWarning(nameof(GetRandomDynamicGift) + " took: " + watch.ElapsedMilliseconds + "ms"); }
                finally { Facepunch.Pool.Free(ref watch); }
            }
        }

        private void DoGifts()
        {
            if ((DateTime.UtcNow - SaveRestore.SaveCreatedTime).TotalDays < 0.55) return;

            if (UnityEngine.Random.Range(0, 101) <= 33)
            {
                GiveGiftAll(GetRandomDynamicGift());
                return;
            }

            var rnd1 = UnityEngine.Random.Range(1, 26);
            var rnd3 = UnityEngine.Random.Range(1, 66);
            var rnd4 = UnityEngine.Random.Range(0, 130);

            var giveitem = string.Empty;
            var giveamount = 0;

            if (rnd1 == 7 || rnd1 == 11)
            {
                giveitem = "wood";
                giveamount = UnityEngine.Random.Range(1200, 2573);
            }
            if (rnd1 == 3 || rnd1 == 2)
            {
                giveitem = "stones";
                giveamount = UnityEngine.Random.Range(1000, 3500);
            }
            if (rnd1 == 14)
            {
                giveitem = "fat.animal";
                giveamount = UnityEngine.Random.Range(60, 100);
            }
            if (rnd1 == 20)
            {
                giveitem = "bandage";
                giveamount = UnityEngine.Random.Range(1, 4);
            }
            if (rnd1 == 6)
            {
                giveitem = "blueberries";
                giveamount = UnityEngine.Random.Range(3, 9);
            }
            if (rnd3 == 44)
            {
                giveitem = "hq.metal.ore";
                giveamount = UnityEngine.Random.Range(10, 25);
            }
            if (rnd1 == 25)
            {
                giveitem = "scrap";
                giveamount = UnityEngine.Random.Range(11, 24);
            }
            if (rnd4 >= 80 && rnd4 < 90)
            {
                giveitem = "scrap";
                giveamount = UnityEngine.Random.Range(30, 90);
            }

            //CHRISTMAS GIFTS
            if (ConVar.XMas.enabled)
            {
                if (rnd1 > 20 && rnd1 < 23)
                {
                    giveitem = "candycaneclub";
                    giveamount = UnityEngine.Random.Range(2, 8);
                }
                if (rnd4 <= 5)
                {
                    giveitem = "santahat";
                    giveamount = 1;
                }
                if (rnd4 >= 80 && rnd4 < 90)
                {
                    giveitem = "stocking.small";
                    giveamount = 1;
                }
                if (rnd4 > 11 && rnd4 <= 16)
                {
                    giveitem = "pookie.bear";
                    giveamount = 1;
                }
            }

            if (string.IsNullOrWhiteSpace(giveitem))
            {
                PrintWarning("no give item found!! force dynamic");

                GiveGiftAll(GetRandomDynamicGift());
            }
            else GiveGiftAllByName(giveitem, giveamount);

        }

        private bool TryGiveGift(BasePlayer player, Item item)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead() || item == null || item.amount < 1) return false;
            var inDM = Deathmatch?.Call<bool>("InAnyDM", player.UserIDString) ?? false;
            if (inDM) return false;
            return item.MoveToContainer(player.inventory.containerMain) || item.MoveToContainer(player.inventory.containerBelt);
        }

        private bool TryGiveGiftItemAmount(BasePlayer player, ItemAmount amount)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead() || amount == null || amount.amount < 1) return false;
            var inDM = Deathmatch?.Call<bool>("InAnyDM", player.UserIDString) ?? false;
            if (inDM) return false;

            var item = ItemManager.Create(amount.itemDef, (int)amount.amount);
            if (!TryGiveGift(player, item))
            {
                RemoveFromWorld(item);
                return false;
            }
            else return true;
        }

        private void AddGiftQueue(string userId, ItemAmount item)
        {
            if (string.IsNullOrEmpty(userId) || item == null || item.amount < 1) return;

            var queue = GetGiftQueue(userId) ?? new List<ItemAmount>();

            queue.Add(item);

            SetGiftQueue(userId, queue);
        }

        private List<ItemAmount> GetGiftQueue(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            List<ItemAmount> queue;
            if (_giftQueue.TryGetValue(userId, out queue)) return queue;

            return null;
        }

        private void SetGiftQueue(string userId, List<ItemAmount> queue)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            _giftQueue[userId] = queue;
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private bool HasAnyLooters(BaseEntity crate)
        {
            if (crate == null) return false;

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected) continue;

                if (player?.inventory?.loot?.entitySource == crate) return true;
            }

            return false;
        }

        private void SendReply(IPlayer player, string msg, string userId = CHAT_STEAM_ID, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : keepTagsConsole ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else
            {
#if RUST
                player.Command("chat.add", string.Empty, userId, msg);
#else
                player.Reply(msg);
#endif
            }
        }

        private void SendReply(BasePlayer player, string msg, string userId = CHAT_STEAM_ID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;


            //	Replace Color Tags
            phrase = _colorRegex.Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = _sizeRegex.Replace(phrase, string.Empty);

            var phraseSB = Pool.Get<StringBuilder>();
            try
            {
                phraseSB.Clear().Append(phrase);

                foreach (var tag in _forbiddenTags)
                    phraseSB.Replace(tag, string.Empty);

                return phraseSB.ToString();
            }
            finally { Pool.Free(ref phraseSB); }
        }

        private readonly Dictionary<string, Timer> popupTimer = new Dictionary<string, Timer>();

        private void ShowPopup(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException();
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException();
            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;
            player.Command("gametip.showgametip", msg);
            Timer endTimer;
            if (popupTimer.TryGetValue(Id, out endTimer)) endTimer.Destroy();
            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });
            popupTimer[Id] = endTimer;
        }
    }
}
