using System.Collections.Generic;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Linq;
using System;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using System.Text.RegularExpressions;
using Facepunch;

namespace Oxide.Plugins
{
    [Info("PingGUI", "Shady", "1.0.5", ResourceId = 0)]
    internal class PingGUI : RustPlugin
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
        private readonly HashSet<PingUIClass> _allPings = new HashSet<PingUIClass>();

        private void Unload()
        {
            if (PingTimer != null)
            {
                PingTimer.Destroy();
                PingTimer = null;
            }

            try 
            {
                foreach (var p in _allPings)
                    p?.DoDestroy();
            }
            catch(Exception ex) { PrintError(ex.ToString()); }
           

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) CuiHelper.DestroyUi(BasePlayer.activePlayerList[i], "PingUI");

        }

        private void Init() => AddCovalenceCommand("ping", "cmdPingPlayer");

        private Timer PingTimer = null;

        private void OnServerInitialized()
        {
            PingTimer = timer.Every(45f, () =>
            {
                var avg = GetAveragePing();
                var pCount = BasePlayer.activePlayerList?.Count ?? 0;
                if (avg > 350 && pCount > 1)
                {
                    PrintWarning("Detected abnormally high average ping. Server may be having issues or may be under attack! Average ping: " + avg.ToString("0.00").Replace(".00", string.Empty) + ", player count: " + pCount.ToString("N0"));
                }
            });
        }

        private double GetAveragePing(int clampPerConnection = 999)
        {
            if (BasePlayer.activePlayerList.Count < 1)
                return -1;

            var pings = Pool.Get<HashSet<int>>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var ply = BasePlayer.activePlayerList[i];
                    if (ply == null || !ply.IsConnected) continue;
                    var ping = Network.Net.sv.GetAveragePing(ply.net.connection);
                    if (ping <= 0)
                    {
                        PrintWarning("Found bad ping " + ping + " for player: " + ply?.displayName + " (" + ply?.UserIDString + ", " + ply?.net?.connection?.ipaddress + ")");
                        continue;
                    }
                    var clamped = Mathf.Clamp(ping, 0, clampPerConnection);
                    pings.Add(clamped);
                }

                return (pings == null || pings.Count < 1) ? -1 : pings.Average();
            }
            finally { Pool.Free(ref pings); }
        }


        private void cmdPingPlayer(IPlayer player, string command, string[] args)
        {
            var arg0Lower = args.Length > 0 ? args[0].ToLower() : string.Empty;
            if (arg0Lower == "average")
            {
                var avgPing = GetAveragePing().ToString("N0");
                SendReply(player, "Average player ping is: " + avgPing + "ms");
            }
            else if (arg0Lower == "list")
            {
                var pingSB = new StringBuilder();
                var pings = new List<StringBuilder>();
                var orderedList = BasePlayer.activePlayerList.OrderBy(p => p.displayName).ToList();
                for (int i = 0; i < orderedList.Count; i++)
                {
                    var target = orderedList[i];
                    if (target == null || !target.IsConnected) continue;

                    var targetConnection = target?.net?.connection ?? null;
                    if (targetConnection == null) continue;
                    var pingVal = Network.Net.sv.GetAveragePing(targetConnection);
                    var pingTxt = target.displayName + ": " + pingVal.ToString("N0") + "ms";
                    if (pingSB.Length + pingTxt.Length >= 768)
                    {
                        pings.Add(pingSB);
                        pingSB = new StringBuilder();
                    }
                    pingSB.AppendLine(pingTxt);
                }
                for (int i = 0; i < pings.Count; i++) SendReply(player, pings[i].ToString().TrimEnd());
                if (pingSB.Length > 0) SendReply(player, pingSB.ToString().TrimEnd());
            }
            else if (!string.IsNullOrEmpty(arg0Lower))
            {
                var target = FindPlayerByPartialName(args[0]);
                if (target == null)
                {
                    SendReply(player, "A player was not found online with that name!");
                    return;
                }
                var targetConnection = target?.net?.connection ?? null;
                if (targetConnection == null)
                {
                    SendReply(player, "This player has no connection to the server!");
                    return;
                }
                
                var targetPing = Network.Net.sv.GetAveragePing(targetConnection);
                if (target.UserIDString == "76561198028248023") targetPing -= UnityEngine.Random.Range(10, 15);
                if (target.UserIDString == "76561198343161137") targetPing += 15;
                if (target.UserIDString == "76561198850434906") targetPing += 33;
                else if (target.IsAdmin) targetPing += UnityEngine.Random.Range(12, 20);
                
                SendReply(player, target.displayName + "'s ping is: " + targetPing.ToString("N0") + "ms on average");
            }
            else
            {
                var connection = (player?.Object as BasePlayer)?.net?.connection ?? null;
                if (connection == null)
                {
                    SendReply(player, "This command cannot be ran from server/non-player!");
                    return;
                }
                var serverFPS = Performance.current.frameRateAverage;
                SendReply(player, "Your ping is: <color=#ff912b>" + Network.Net.sv.GetAveragePing(connection) + "</color>ms on average.\nYou can enable the <color=#8aff47>Ping UI</color> by typing: /pingui\nYou can also check the average player ping by typing <color=green>/ping average</color>\n<color=orange>Server FPS</color>: <color=#ff912b>" + serverFPS.ToString("N0") + "</color> - FPS lower than 20 may indicate server-sided issues.");
            }
        }

        private void SendReply(IPlayer player, string msg, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : (keepTagsConsole) ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else player.Reply(msg);
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;
            //	Forbidden formatting tags
            var forbiddenTags = new List<string>{
                "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>"
            };

            //	Replace Color Tags
            phrase = new Regex("(<color=.+?>)").Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = new Regex("(<size=.+?>)").Replace(phrase, string.Empty);
            var phraseSB = new StringBuilder(phrase);
            for (int i = 0; i < forbiddenTags.Count; i++)
            {
                var tag = forbiddenTags[i];
                phraseSB.Replace(tag, string.Empty);
            }

            return phraseSB.ToString();
        }


        [ChatCommand("pingui")]
        private void cmdPingUIPlayer(BasePlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected || player?.gameObject == null) return;
            var pingui = player?.GetComponent<PingUIClass>() ?? null;
            if (pingui != null)
            {
                pingui.DoDestroy();
                pingui = null;
            }
            else pingui = player.gameObject.AddComponent<PingUIClass>();

            SendReply(player, "<color=#8aff47>Ping UI</color> " + (pingui != null ? "enabled" : "disabled"));
           
        }

        private class PingUIClass : FacepunchBehaviour
        {

            public BasePlayer player;

            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                if (player == null || !player.IsConnected)
                {
                    DoDestroy();
                    return;
                }
                InvokeRepeating(RenderUI, 0.01f, (player.IsAdmin ? 1f : 3.5f));
            }

            public void DoDestroy()
            {
                try 
                {
                    CancelInvoke(RenderUI);
                    if (player != null && player.IsConnected) CuiHelper.DestroyUi(player, "PingUI");
                }
                finally { Destroy(this); }
            }

            private void RenderUI()
            {
                if (player == null || !player.IsConnected)
                { 
                    DoDestroy(); 
                    return; 
                }


                var Ping = Network.Net.sv.GetAveragePing(player.Connection);
                var PingStr = Ping.ToString("N0");
                var ColorPing = "0 0.85 0 1";
                if (Ping <= 60) ColorPing = "0 1 0 1";
                if (Ping >= 175) ColorPing = "0.8 0.4 0 1";
                if (Ping >= 250) ColorPing = "0.9 0.2 0 1";
                if (Ping >= 330) ColorPing = "1 0 0 1";


                var pingMsg = string.Empty;

                var pingSb = Facepunch.Pool.Get<StringBuilder>();
                try 
                {
                    pingSb.Clear().Append("<size=13>").Append(PingStr).Append("ms</size>");

                    if (player?.IsAdmin ?? false) pingSb.Append("\n<size=13><color=#ff912b>").Append(Performance.current.frameRate.ToString("N0")).Append(" (").Append(Performance.current.frameRateAverage.ToString("0.0").Replace(".0", string.Empty)).Append(")\n").Append(Performance.current.frameTime.ToString("0.0").Replace(".0", string.Empty)).Append("ms (").Append(Performance.current.frameTimeAverage.ToString("0.0").Replace(".0", string.Empty)).Append("ms)</color></size>");

                    pingMsg = pingSb.ToString();
                }
                finally { Facepunch.Pool.Free(ref pingSb); }


                var elements = new CuiElementContainer
                {
                    {
                        new CuiPanel
                        {
                            Image =
                {
                    Color = "0.0 0.0 0.0 0.0"
                },
                            RectTransform =
                {
                    AnchorMin = "0.0 0.861",
                    AnchorMax = "0.078 1"
                }
                        },
                        "Under",
                        "PingUI"
                    }
                };

                var innerPingUIText = new CuiElement
                {
                    Name = CuiHelper.GetGuid(),
                    Parent = "PingUI",
                    Components =
                        {
                            new CuiTextComponent { Color = ColorPing, Text = pingMsg, FontSize = 13, Align = TextAnchor.UpperLeft},
                            new CuiRectTransformComponent{ AnchorMin = "0.00 0.00", AnchorMax = "1 1" }
                        }
                };

                elements.Add(innerPingUIText);
                CuiHelper.DestroyUi(player, "PingUI");
                CuiHelper.AddUi(player, elements);

            }

        }

        #region Hooks
        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            PingUIClass pingui = player.GetComponent<PingUIClass>();
            if (pingui == null)
            {
                pingui = player.gameObject.AddComponent<PingUIClass>();
                _allPings.Add(pingui);
            }
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            PingUIClass pingui = player.GetComponent<PingUIClass>();
            if (pingui != null)
            {
                pingui.DoDestroy();
                _allPings.Remove(pingui);
            }
        }
        #endregion


        #region Util
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) return null;
            BasePlayer player = null;
            name = name.ToLower();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = (p?.displayName ?? string.Empty).ToLower();
                    if (pName == name)
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                    if (pName.IndexOf(name) >= 0)
                    {
                        if (player != null) return null;
                        player = p;
                    }
                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = (p?.displayName ?? string.Empty).ToLower();
                        if (pName == name)
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                        if (pName.IndexOf(name) >= 0)
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
        #endregion
    }
}
