using UnityEngine;
using System;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("Give", "Shady", "1.0.2", ResourceId = 0)]
    internal class Give : RustPlugin
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
        private const string GIVE_PERM_SELF = "give.giveself";
        private const string GIVE_PERM_OTHER = "give.giveother";

        private void Init()
        {
            permission.RegisterPermission(GIVE_PERM_SELF, this);
            permission.RegisterPermission(GIVE_PERM_OTHER, this);

            string[] giveCmds = { "give", "g.givec" };
            string[] giveBpCmds = { "givebp", "g.givebpc" };

            AddCovalenceCommand(giveCmds, nameof(cmdGive));
            AddCovalenceCommand(giveBpCmds, nameof(cmdGiveBP));

            AddCovalenceCommand("giveme", nameof(cmdGiveMe));

        }

        /*----------------------------------------------------------------------------------------------------------------------------//
        //													HOOKS																	  //
        //----------------------------------------------------------------------------------------------------------------------------*/

        private bool GiveItem(BasePlayer target, Item item)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (target?.inventory == null || target.IsDead())
                return false;

            item.text = "Spawned item";

            return item.MoveToContainer(target.inventory.containerMain) || item.MoveToContainer(target.inventory.containerBelt);
        }

        private bool GiveItem(BasePlayer target, ItemDefinition itemDef, int amount = 1, ulong skin = 0)
        {
            if (target == null || target.IsDead() || itemDef == null || amount < 1) return false;


            var item = ItemManager.CreateByItemID(itemDef.itemid, amount, skin);

            item.text = "Spawned item";

            return GiveItem(target, item);
        }

        private void SendItemNotice(BasePlayer target, int itemId, int plusOrMinusAmount = 1) { SendItemNotice(target?.UserIDString ?? string.Empty, itemId.ToString(), plusOrMinusAmount.ToString()); }

        private void SendItemNotice(string userId, string itemId, string plusOrMinusAmount = "1")
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(itemId)) return;
            var target = FindPlayerById(userId);
            if (target == null || target.IsDead() || !target.IsConnected) return;
            target.SendConsoleCommand("note.inv " + itemId + " " + plusOrMinusAmount);
        }

        private void cmdGive(IPlayer player, string command, string[] args)
        {
            if (!CanGiveOther(player.Id) && !CanGiveSelf(player.Id) && !player.IsServer)
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }
            if (args.Length < 2)
            {
                SendReply(player, "Invalid Syntax, try /give PlayerName Item (Amount)");
                return;
            }
            var target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                if (ulong.TryParse(args[0], out ulong targetId)) target = BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId);
            }
            if (target == null)
            {
                SendReply(player, "Failed to find a player with the name or ID: " + args[0]);
                return;
            }
            if (!CanGiveOther(player.Id) && target.UserIDString != player.Id && !player.IsServer)
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }
            var newItemDef = FindItemByPartialName(args[1]);
            if (newItemDef == null)
            {
                SendReply(player, "Failed to find an item with the name: " + args[1]);
                return;
            }
            var itemName = newItemDef?.displayName?.english ?? "Unknown";
            var amount = 1;
            if (args.Length > 2)
            {
                if (!int.TryParse(args[2], out amount))
                {
                    SendReply(player, "Failed to parse amount: " + args[2]);
                    return;
                }
            }
            var skin = 0ul;
            if (args.Length > 3)
            {
                if (!ulong.TryParse(args[3], out skin))
                {
                    SendReply(player, "Failed to parse: " + args[3] + " as skin!");
                    return;
                }
            }

            if (GiveItem(target, newItemDef, amount, skin))
            {
                SendReply(player, "Gave: " + target.displayName + " " + itemName + " x" + amount.ToString("N0"));
                SendItemNotice(target, newItemDef.itemid, amount);
            }
            else SendReply(player, "Failed to give item to " + target.displayName + "!");  
        }

        private void cmdGiveBP(IPlayer player, string command, string[] args)
        {
            if (!CanGiveOther(player.Id) && !CanGiveSelf(player.Id) && !player.IsServer)
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }
            if (args.Length < 2)
            {
                SendReply(player, "Invalid Syntax, try /give PlayerName Item (Amount)");
                return;
            }
            var target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, "Failed to find a player with the name: " + args[0]);
                return;
            }
            if (!CanGiveOther(player.Id) && target.UserIDString != player.Id)
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }
            var newItemDef = FindItemByPartialName(args[1]);
            if (newItemDef == null)
            {
                SendReply(player, "Failed to find an item with the name: " + args[1]);
                return;
            }
            var itemName = newItemDef?.displayName?.english ?? "Unknown";
            var amount = 1;
            if (args.Length > 2)
            {
                if (!int.TryParse(args[2], out amount))
                {
                    SendReply(player, "Failed to parse amount: " + args[2]);
                    return;
                }
            }
            var skin = 0ul;
            if (args.Length > 3)
            {
                if (!ulong.TryParse(args[3], out skin))
                {
                    SendReply(player, "Failed to parse: " + args[3] + " as skin!");
                    return;
                }
            }

            var bpItem = ItemManager.CreateByName("blueprintbase", 1);
            bpItem.blueprintTarget = newItemDef.itemid;
            bpItem.amount = amount;
            if (GiveItem(target, bpItem))
            {
                SendReply(player, "Gave: " + target.displayName + " " + itemName + " x" + amount.ToString("N0"));
                SendItemNotice(target, newItemDef.itemid, amount);
            }
            else SendReply(player, "Failed to give item to " + target.displayName + "!");
        }


        private void cmdGiveMe(IPlayer player, string command, string[] args)
        {
            if (player.IsServer)
            {
                SendReply(player, "This must be ran as a player!");
                return;
            }

            if (!CanGiveSelf(player.Id))
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "Invalid Syntax, try /give PlayerName Item (Amount)");
                return;
            }

            var newItemDef = FindItemByPartialName(args[0]);

            if (newItemDef == null)
            {
                SendReply(player, "Failed to find an item with the name: " + args[0]);
                return;
            }

            var amount = args.Length > 1 ? ParseAmount(args[1]) : 1;

            if (amount <= 0)
            {
                SendReply(player, "Failed to parse amount: " + args[1]);
                return;
            }

            var playerObj = player?.Object as BasePlayer;

            if (playerObj?.inventory == null)
            {
                SendReply(player, "This must be ran as a player!");
                return;
            }

            if (playerObj.IsDead())
            {
                SendReply(player, "This command cannot be used while dead!");
                return;
            }

            var newItem = ItemManager.CreateByItemID(newItemDef.itemid, amount);
            newItem.text = "Spawned item";

            

            playerObj.GiveItem(newItem);

        }

        private int ParseAmount(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(nameof(str));

            if (int.TryParse(str, out int newInt))
                return newInt;

            if (!str.EndsWith("k", StringComparison.OrdinalIgnoreCase))
                return -1;

            var newStr = str.Replace("k", string.Empty, StringComparison.OrdinalIgnoreCase);

            if (!double.TryParse(newStr, out double newDouble))
                return -1;

            var roundedVal = newDouble > int.MaxValue ? int.MaxValue : Math.Round(newDouble * 1000, MidpointRounding.AwayFromZero);

            return (int)roundedVal;
        }

        private bool CanGiveSelf(string userID) { return permission.UserHasPermission(userID, GIVE_PERM_SELF); }

        private bool CanGiveOther(string userID) { return permission.UserHasPermission(userID, GIVE_PERM_OTHER); }

        private void SendReply(IPlayer player, string message) { if (player != null && player.IsConnected) player.Message(message); }

        private BasePlayer FindPlayerById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            return ulong.TryParse(userId, out ulong uid) ? RelationshipManager.FindByID(uid) : null;
        }

        private ItemDefinition FindItemByPartialName(string engOrShortName)
        {
            if (string.IsNullOrEmpty(engOrShortName)) throw new ArgumentNullException(nameof(engOrShortName));


            var matches = Facepunch.Pool.GetList<ItemDefinition>();
            try 
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item.displayName.english.Equals(engOrShortName, StringComparison.OrdinalIgnoreCase) || item.shortname.Equals(engOrShortName, StringComparison.OrdinalIgnoreCase)) return item;

                    var engName = item?.displayName?.english;

                    if (engName.IndexOf(engOrShortName, StringComparison.OrdinalIgnoreCase) >= 0 || item.shortname.IndexOf(engOrShortName, StringComparison.OrdinalIgnoreCase) >= 0) 
                        matches.Add(item);
                }

                return matches.Count != 1 ? null : matches[0];
            }
            finally { Facepunch.Pool.FreeList(ref matches); }
        }

        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            BasePlayer player = null;
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                return null;
            }
            return player;
        }
    }
}