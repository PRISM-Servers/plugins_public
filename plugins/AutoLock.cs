using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Auto Lock", "Shady", "1.0.0")]
    internal class AutoLock : RustPlugin
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
        #region Fields
        #region Plugin References
        [PluginReference]
        private readonly Plugin NoEscape;
        #endregion

        private const string DATA_FILE_NAME = "AutoLockAds";
        private const string CHAT_STEAM_ID = "76561198865536053";

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private Dictionary<ulong, int> _codeLockAds;
        private readonly Dictionary<ulong, DateTime> _lastAdvertised = new Dictionary<ulong, DateTime>();

        private readonly string[] _lockAds = { "Want to speed up placing locks? With <color=#eb3dff>/autolock</color> you can set a code to <i>automatically</i> use!", "Want your code lock to <i>automatically</i> lock when you place it? Check out <color=orange>/autolock</color>!" };


        private readonly Dictionary<string, string> _autoCode = new Dictionary<string, string>();
        #endregion
        #region Hooks
        private void Init()
        {
            _codeLockAds = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<ulong, int>>(DATA_FILE_NAME) ?? new Dictionary<ulong, int>();

            string[] autoLockCmds = { "autolock", "autocode" };
            AddCovalenceCommand(autoLockCmds, nameof(cmdAutoLock));

        }
        private void Unload() => SaveAdData();

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item item)
        {
            if (player == null || item == null) return;

            if (item?.info?.shortname != "lock.code") return;

            DateTime lastTime;
            int count;
            var now = DateTime.Now;
            if (!_lastAdvertised.TryGetValue(player.userID, out lastTime) || (now - lastTime).TotalSeconds >= 5)
            {
                if (!_codeLockAds.TryGetValue(player.userID, out count)) _codeLockAds[player.userID] = 1;
                if (count > 5) return;
                SendReply(player, _lockAds[UnityEngine.Random.Range(0, 2)]);
                _codeLockAds[player.userID]++;
                _lastAdvertised[player.userID] = now;
            }
        }

        private void OnItemDeployed(Deployer deployer, BaseEntity entity)
        {
            if (deployer == null || entity == null) return;
            var player = deployer?.ToPlayer() ?? deployer?.GetOwnerPlayer() ?? null;
            if (player == null) return;

            var codeLock = entity?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (codeLock == null) return;

            var code = GetAutoCode(player.UserIDString);
            if (string.IsNullOrEmpty(code) || code.Length != 4 || IsEscapeBlocked(player.UserIDString)) return;

            var canChange = Interface.Oxide.CallHook("CanChangeCode", codeLock, player, code, false);
            if (canChange != null && (canChange is bool) && !(bool)canChange) return;

            codeLock.code = code;
            codeLock.SetFlag(BaseEntity.Flags.Locked, true);
            codeLock.whitelistPlayers.Add(player.userID);
            codeLock.SendNetworkUpdate();

            var parent = entity?.GetParentEntity() ?? null;
            var effectPos = (parent != null && parent?.transform != null) ? parent.transform.position : (entity?.transform?.position ?? Vector3.zero);
            if (effectPos != Vector3.zero) Effect.server.Run(codeLock.effectCodeChanged.resourcePath, effectPos, effectPos);
        }
        #endregion
        #region Command
        private void cmdAutoLock(IPlayer player, string command, string[] args)
        {
            if (player == null || player.IsServer) return;
            string curCode;
            _autoCode.TryGetValue(player.Id, out curCode);
            if (args.Length < 1)
            {

                SendReply(player, "<color=#ff912b>----<color=orange>Auto Lock</color>----</color>\n\n-You can have your locks automatically be placed with a specific code and locked by doing <color=#eb3dff>/autolock CodeToUse</color>\n\n-You can disable this using <color=#eb3dff>/autolock 0</color>\n\n-Your current auto lock code is set to: <color=orange>" + (string.IsNullOrEmpty(curCode) ? "None" : curCode) + "</color>");
                return;
            }
            var arg0Lower = args[0].ToLower();
            if (arg0Lower == "0")
            {
                SendReply(player, "Autolock disabled");
                if (!string.IsNullOrEmpty(curCode)) _autoCode.Remove(player.Id);
                return;
            }
            if (arg0Lower.Length < 4)
            {
                SendReply(player, "Invalid code supplied (too short)");
                return;
            }
            if (arg0Lower.Length > 4)
            {
                SendReply(player, "Invalid code supplied (too long)");
                return;
            }
            int code;
            if (!int.TryParse(args[0], out code))
            {
                SendReply(player, "Invalid code supplied: " + args[0] + " (not a number)");
                return;
            }
            _autoCode[player.Id] = args[0];
            SendReply(player, "Set auto lock to: " + args[0]);
        }
        #endregion


        #region Util
        private void SaveAdData() => Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _codeLockAds);

        private string GetAutoCode(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;

            string code;
            return _autoCode.TryGetValue(userID, out code) ? code : string.Empty;
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

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var p = BasePlayer.sleepingPlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty; //BasePlayer.activePlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? BasePlayer.sleepingPlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? string.Empty;
        }

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

        private bool IsEscapeBlocked(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            return NoEscape?.Call<bool>("IsEscapeBlockedS", userID) ?? false;
        }
        #endregion
    }
}