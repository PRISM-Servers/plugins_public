using Facepunch;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxide.Plugins
{
    [Info("SaveTimer", "MBR/Shady", "1.0.9")]
    internal class SaveTimer : RustPlugin
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
        private int OldInterval = -1;
        private const int NewInterval = 10800;
        private const int SaveActionIntervalMinimum = 900;
        private const int SaveActionIntervalMaximum = 1800;
        private Action SaveAction = null;

        private const int MaxDelay = 240;

        private bool GoodTimeToSave()
        {
            var plyList = BasePlayer.activePlayerList;
            if (plyList == null || plyList.Count < 10) return true; //only do checks if at least 10 players are online
            var now = UnityEngine.Time.realtimeSinceStartup;
            var possiblyInCombat = 0;
            for(int i = 0; i < plyList.Count; i++)
            {
                var p = plyList[i];
                if (p == null || p.IsDestroyed || p.gameObject == null || p.IsDead() || p.IsSleeping() || p.IsReceivingSnapshot) continue;
                var lastAtk = p?.lastAttacker as BasePlayer;
                if (lastAtk != null && !lastAtk.IsDestroyed && lastAtk.gameObject != null && !lastAtk.IsNpc && p.lastAttackedTime > 0f && (now - p.lastAttackedTime) < 60)
                {
                    possiblyInCombat++;
                }
            }
            if (possiblyInCombat > 1) return false;
            return true;
        }

        private void OnServerInitialized()
        {
            OldInterval = ConVar.Server.saveinterval;
            ConVar.Server.saveinterval = NewInterval;
            Puts("Set Server.saveinterval to: " + NewInterval + ", old interval: " + OldInterval);
            SaveAction = new Action(() =>
            {
                if (ServerMgr.Instance == null)
                {
                    PrintWarning("servermgr instance is null on saveaction!!!");
                    return;
                }

                ServerMgr.Instance.Invoke(() =>
                {
                    ShowToastAll("<color=#4fb6ff>SAVING IN <i><size=30>5</size></i> SECONDS!</color>", 1);
                }, 5f);

                ServerMgr.Instance.Invoke(() =>
                {
                  //  Server.Broadcast("<color=#bcf248>Server saving! <color=white>-</color> <color=orange>EXPECT A SHORT LAG SPIKE</color>!</color>");

                    ShowToastAll("<color=#f246c2>SERVER SAVING!</color>", 1);
                    if (ServerMgr.Instance != null) ServerMgr.Instance.Invoke(() => ConVar.Server.save(null), 0.15f);
                    else PrintWarning("servermgr instance is null on saveaction invoke!!!");
                }, 10f);

               // Server.Broadcast("<color=#ea780e>Server save in <color=#8ff968>10</color> seconds<color=#f98416>!</color> <color=#f4d177>Expect a <i>short</i> lag spike</color></color>.");
                ShowToastAll("<color=#4fb6ff>SAVING IN <i><size=22>10</size></i> SECONDS!</color>", 1);

                
            });

            ServerMgr.Instance.Invoke(SaveAction, UnityEngine.Random.Range(SaveActionIntervalMinimum, SaveActionIntervalMaximum));
        }

        private void OnServerSave()
        {
            var rngTime = UnityEngine.Random.Range(SaveActionIntervalMinimum, SaveActionIntervalMaximum);

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try { PrintWarning(sb.Clear().Append("Next server save in: ").Append(rngTime.ToString("N0")).Append(" seconds").ToString()); }
            finally { Facepunch.Pool.Free(ref sb); }
          
            ServerMgr.Instance.Invoke(SaveAction, rngTime);
        }

        private void Unload()
        {
            if (SaveAction != null && ServerMgr.Instance != null)
            {
                PrintWarning("Canceled SaveAction");
                ServerMgr.Instance.CancelInvoke(SaveAction);
            }

            if (ConVar.Server.saveinterval != OldInterval && OldInterval > 0)
            {
                ConVar.Server.saveinterval = OldInterval;
                Puts("Restored save interval to old interval: " + OldInterval);
            }
        }

        private readonly Dictionary<string, Timer> popupTimer = new Dictionary<string, Timer>();

        private void ShowPopupById(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException(nameof(Id));
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));
            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;
            ShowPopup(player, msg, duration);
        }

        private void ShowPopup(IPlayer player, string msg, float duration = 5f)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));

            if (!player.IsConnected) return;

            player.Command("gametip.showgametip", msg);

            Timer endTimer;
            if (popupTimer.TryGetValue(player.Id, out endTimer)) endTimer.Destroy();

            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });

            popupTimer[player.Id] = endTimer;
        }

        private void ShowPopupAll(string msg, float duration = 5f)
        {
            foreach (var ply in covalence.Players.Connected) ShowPopup(ply, msg, duration);
        }

        private void ShowToastAll(string msg, int type = 0)
        {
            foreach (var ply in covalence.Players.Connected)
            {
                var pObj = ply?.Object as BasePlayer;
                if (pObj != null)
                    ShowToast(pObj, msg, type);
            }
        }

        private void ShowToast(BasePlayer player, string message, int type = 0)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.IsConnected)
                return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                player.SendConsoleCommand(sb.Clear().Append("gametip.showtoast ").Append(type).Append(" \"").Append(message).Append("\"").ToString());
            }
            finally { Pool.Free(ref sb); }
        }
    }
}