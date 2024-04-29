using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Mass Authorize", "Shady", "0.0.1")]
    internal class MassAuthorize : RustPlugin
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
        [PluginReference]
        private readonly Plugin NoEscape;

        private void Init()
        {
            string[] authorizeCmds = { "authorize", "authorise", "auths", "share", "unshare", "a", "au" };
            AddCovalenceCommand(authorizeCmds, nameof(cmdShareRadius));
        }

        private void cmdShareRadius(IPlayer iPlayer, string command, string[] args)
        {
            if (iPlayer == null || iPlayer.IsServer) return;

            var player = iPlayer?.Object as BasePlayer;
            if (player == null)
            {
                iPlayer.Reply("This can only be ran as a player!");
                return;
            }

            if (player.IsDestroyed || player.gameObject == null || player.IsDead()) return;

            if (args == null || args.Length <= 3)
            {
                SendReply(player, "You must specify a player, type and distance/radius. Example: <color=#42dfff>/" + command + " <share/unshare> PlayerName <type> <radius></color>" + Environment.NewLine + Environment.NewLine + "Type can be: <color=orange>box</color>, <color=#ff912b>cupboard</color>, <color=#8aff47>door</color>, <color=red>turret</color>, or <color=#eb3dff>all</color>." + Environment.NewLine + "Radius should be the maximum distance in meters.\nHere's a full example:\n<color=yellow>/" + command + " share UserName all 300</color>. This will grant a user access to <i><color=#fcb13a>everything</color></i> within a 300 meter radius.");
                return;
            }
            if (IsEscapeBlocked(player.UserIDString))
            {
                SendReply(player, "You cannot use this while raid/combat blocked!");
                return;
            }
            if (player.IsWounded())
            {
                SendReply(player, "You cannot use this while wounded!");
                return;
            }
            if (!player.CanBuild())
            {
                SendReply(player, "You cannot use this while building blocked!");
                return;
            }
            if (player.IsSwimming() || player.IsHeadUnderwater())
            {
                SendReply(player, "You cannot use this while swimming!");
                return;
            }

            var target = FindConnectedPlayer(args[1], true)?.Object as BasePlayer;
            if (target == null)
            {
                SendReply(player, "Unable to find a player with name: " + args[1]);
                return;
            }

            var findType = args[2];
            var findTypeLower = findType.ToLower();
            var shareType = args[0].ToLower();

            var boxCount = 0;
            var doorCount = 0;
            var cupboardCount = 0;
            var turretCount = 0;

            if (shareType != "share" && shareType != "unshare")
            {
                SendReply(player, "Invalid share type: " + shareType + ", try <color=#8aff47>share</color> or <color=red>unshare</color>");
                return;
            }

            var doShare = shareType == "share";
            if (findTypeLower != "box" && findTypeLower != "cupboard" && findTypeLower != "door" && findTypeLower != "turret" && findTypeLower != "all")
            {
                SendReply(player, "Invalid type supplied: " + findType + ", try: <color=orange>box</color>, <color=#ff912b>cupboard</color>, <color=#8aff47>door</color>, <color=red>turret</color>, or <color=#eb3dff>all</color>.");
                return;
            }
            float radius;
            if (!float.TryParse(args[3], out radius))
            {
                SendReply(player, "Radius is not a number: " + args[3]);
                return;
            }
            else radius = Mathf.Clamp(radius, 1, 750);

            var entList = new HashSet<BaseEntity>();
            var plyPos = player?.transform?.position ?? Vector3.zero;
            foreach (var entity in BaseEntity.saveList)
            {
                if (entity == null || entity.IsDestroyed || entity.gameObject == null || entity.OwnerID != player.userID) continue;
                var dist = Vector3.Distance(entity.transform.position, plyPos);
                if (dist <= radius) entList.Add(entity);
            }
            if (entList.Count < 1)
            {
                SendReply(player, "Failed to find any entities you own & are authorized to within a " + radius + " meter radius");
                return;
            }
   
            var isAll = findTypeLower == "all";
            SendReply(player, "Iterating through <color=yellow>" + entList.Count.ToString("N0") + "</color> entities...");
            foreach (var ent in entList)
            {
                if (ent == null) continue;
                if (player == null)
                {
                    PrintWarning("player turned null on loop!!");
                    break;
                }


                var door = ent as Door;
                if (findTypeLower == "door" && door == null) continue;

                var cupboard = ent as BuildingPrivlidge;
                if (findTypeLower == "cupboard" && cupboard == null) continue;

                var turret = ent as AutoTurret;

                var box = ent as StorageContainer;
                if (findTypeLower == "box" && box == null) continue;

                if ((findTypeLower == "door" || findTypeLower == "box" || isAll) && (door != null || box != null) && HasCodeAccess(ent, player))
                {
                    if ((doShare && TryGiveCodeAccess(ent, target)) || (!doShare && TryTakeCodeAccess(ent, target)))
                    {
                        if (door != null) doorCount++;
                        else if (box != null) boxCount++;
                    }
                }
                if ((findTypeLower == "cupboard" || isAll) && cupboard != null && HasCodeAccess(ent, player) && HasCupboardAccess(cupboard, player))
                {
                    if ((doShare && TryGiveCodeAccess(ent, target) && TryGiveCupboardAccess(cupboard, target)) || (!doShare && TryTakeCodeAccess(ent, target) && TryTakeCupboardAccess(cupboard, target))) cupboardCount++;
                }
                if ((findTypeLower == "turret" || isAll) && turret != null && turret.IsAuthed(player))
                {
                    if ((doShare && TryGiveTurretAccess(turret, target)) || (!doShare && TryTakeTurretAccess(turret, target))) turretCount++;
                }


            }

            if (boxCount < 1 && doorCount < 1 && cupboardCount < 1 && turretCount < 1)
            {
                SendReply(player, "No entities were changed.");
                return;
            }
            if (doShare)
            {
                SendReply(target, player.displayName + " has <color=green>authorized</color> you on " + boxCount + " boxes, " + doorCount + " doors, " + turretCount + " turrets, and " + cupboardCount + " cupboards");
                SendReply(player, "<color=green>Authorized</color> " + target.displayName + " on " + boxCount + " boxes, " + doorCount + " doors, " + turretCount + " turrets, and " + cupboardCount + " cupboards");
            }
            else
            {
                SendReply(target, player.displayName + " has <color=red>un-authorized</color> you on " + boxCount + " boxes, " + doorCount + " doors, " + turretCount + " turrets, and " + cupboardCount + " cupboards");
                SendReply(player, "<color=red>Un-authorized</color> " + target.displayName + " on " + boxCount + " boxes, " + doorCount + " doors, " + turretCount + " turrets, and " + cupboardCount + " cupboards");
            }
        }

        #region Util
        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));

            var ply = covalence.Players.FindPlayerById(nameOrIdOrIp);
            if (ply != null) if ((!ply.IsConnected && tryFindOfflineIfNoOnline) || ply.IsConnected) return ply;

            IPlayer player = null;
            foreach (var p in covalence.Players.Connected)
            {

                if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Address == nameOrIdOrIp || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (player != null) return null;
                    else player = p;
                }
            }

            if (tryFindOfflineIfNoOnline && player == null)
            {
                foreach (var p in covalence.Players.All)
                {
                    if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (player != null) return null;
                        else player = p;
                    }
                }
            }

            return player;
        }

        private bool HasCupboardAccess(BuildingPrivlidge cupboard, BasePlayer player) { return cupboard?.IsAuthed(player) ?? false; }

        private bool HasCupboardAccess(BuildingPrivlidge cupboard, ulong userID)
        {
            return cupboard?.IsAuthed(userID) ?? false;
        }

        private bool HasCupboardAccess(BuildingPrivlidge cupboard, string userID)
        {
            ulong uID;
            if (!ulong.TryParse(userID, out uID)) return false;
            else return HasCupboardAccess(cupboard, uID);
        }

        private bool TryTakeCupboardAccess(BuildingPrivlidge cupboard, BasePlayer player) { return TryTakeCupboardAccess(cupboard, player?.userID ?? 0); }

        private bool TryTakeCupboardAccess(BuildingPrivlidge cupboard, ulong userID)
        {
            if (cupboard == null || userID == 0) return false;

            var remCount = cupboard.authorizedPlayers.RemoveWhere(p => p.userid == userID);
            if (remCount > 0)
            {
                cupboard.SendNetworkUpdate();
                var player = BasePlayer.FindByID(userID);
                player?.SendNetworkUpdate();
            }

            return true;
        }

        private bool TryGiveCupboardAccess(BuildingPrivlidge cupboard, BasePlayer player) { return TryGiveCupboardAccess(cupboard, player?.userID ?? 0); }

        private bool TryGiveCupboardAccess(BuildingPrivlidge cupboard, ulong userID)
        {
            if (cupboard == null || userID == 0) return false;
            if (HasCupboardAccess(cupboard, userID)) return true;
            var player = BasePlayer.FindByID(userID) ?? BasePlayer.FindSleeping(userID) ?? null;
            var newAuth = new ProtoBuf.PlayerNameID
            {
                userid = userID,
                username = player?.displayName ?? string.Empty
            };
            cupboard.authorizedPlayers.Add(newAuth);
            cupboard.SendNetworkUpdate();
            if (player != null && player.IsConnected) player.SendNetworkUpdate();
            return true;
        }

        private bool TryGiveCupboardAccess(BuildingPrivlidge cupboard, string userID)
        {
            ulong uID;
            if (!ulong.TryParse(userID, out uID)) return false;
            else return TryGiveCupboardAccess(cupboard, uID);
        }

        private bool TryGiveTurretAccess(AutoTurret turret, BasePlayer player)
        {
            if (turret == null || player == null) return false;
            var isAuthed = turret?.IsAuthed(player) ?? false;
            if (isAuthed) return true;
            var newAuth = new ProtoBuf.PlayerNameID
            {
                userid = player.userID,
                username = player.displayName
            };
            turret.authorizedPlayers.Add(newAuth);
            turret.SendNetworkUpdate();
            return true;
        }

        private bool TryTakeTurretAccess(AutoTurret turret, BasePlayer player)
        {
            if (turret == null || player == null) return false;
            turret.authorizedPlayers.RemoveWhere(p => p.userid == player.userID);
            turret.SendNetworkUpdate();
            return true;
        }

        private bool HasCodeLockAccess(CodeLock codeLock, BasePlayer player)
        {
            if (player == null || codeLock == null) return false;
            return codeLock?.whitelistPlayers?.Contains(player.userID) ?? false;
        }

        private bool TryGiveCodeLockAccess(CodeLock codeLock, BasePlayer player)
        {
            if (codeLock == null || player == null) return false;
            var listPlayers = codeLock?.whitelistPlayers ?? null;
            if (listPlayers == null) return false;
            if (!listPlayers.Contains(player.userID)) listPlayers.Add(player.userID);
            return true;
        }

        private bool TryTakeCodeLockAccess(CodeLock codeLock, BasePlayer player)
        {
            if (codeLock == null || player == null) return false;
            var listPlayers = codeLock?.whitelistPlayers ?? null;
            if (listPlayers == null) return false;
            if (listPlayers.Contains(player.userID)) listPlayers.Remove(player.userID);
            return true;
        }

        private bool HasCodeAccess(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || player == null) return false;
            var codeLock = entity?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (codeLock != null)
            {
                if (codeLock.whitelistPlayers.Contains(player.userID)) return true;
                else return false;
            }
            return true;
        }

        private bool TryGiveCodeAccess(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || player == null) return false;
            var codeLock = entity?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (codeLock != null && !codeLock.whitelistPlayers.Contains(player.userID)) codeLock.whitelistPlayers.Add(player.userID);
            return true;
        }

        private bool TryTakeCodeAccess(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || player == null) return false;
            var codeLock = entity?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (codeLock != null && codeLock.whitelistPlayers.Contains(player.userID)) codeLock.whitelistPlayers.Remove(player.userID);
            return true;
        }

        private bool IsEscapeBlocked(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            return NoEscape?.Call<bool>("IsEscapeBlockedS", userID) ?? false;
        }
        #endregion

    }
}