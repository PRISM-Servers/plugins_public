using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("ButtonCommand", "Shady", "1.0.1")]
    class ButtonCommand : RustPlugin
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
        //this should use a dictionary instead for perf

        //this code is a DISASTER in general

        private readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        List<ButtonInfo> buttonInfos = new List<ButtonInfo>();

        void Init()
        {
            buttonInfos = Interface.Oxide?.DataFileSystem?.ReadObject<List<ButtonInfo>>("ButtonCommands") ?? new List<ButtonInfo>();
            AddCovalenceCommand("bcmd", "cmdBcmd");
        }

        void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("ButtonCommands", buttonInfos);

        void Unload() => SaveData();


        private class ButtonInfo
        {
            public string ServerCommand { get; set; }
            public string PlayerCommand { get; set; }

            public string LocalFX { get; set; }

            public string GlobalFX { get; set; }

            public NetworkableId ButtonID;

            public ButtonInfo() { }

            public ButtonInfo(NetworkableId netId)
            {
                ButtonID = netId;
            }

            public ButtonInfo(PressButton button)
            {
                if (button == null) throw new ArgumentNullException(nameof(button));
                ButtonID = button.net.ID;
            }
        }

        ButtonInfo GetButtonInfo(NetworkableId netId)
        {
            for(int i = 0; i < buttonInfos.Count; i++)
            {
                var info = buttonInfos[i];
                if (info?.ButtonID == netId) return info;
            }
            return null;
        }

        ButtonInfo GetButtonInfoEntity(PressButton button)
        {
            return GetButtonInfo(button.net.ID);
        }

        private void cmdBcmd(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (player.IsServer || player.Object == null)
            {
                SendReply(player, "This can only be ran as a player!");
                return;
            }
            if (args == null || args.Length < 1)
            {
                SendReply(player, "You must supply arguments!");
                return;
            }
            var button = GetLookAtEntity(player?.Object as BasePlayer, 4f, 65536) as PressButton;
            if (button == null)
            {
                SendReply(player, "No PressButton found! You must be looking at a PressButton to use this command.");
                return;
            }
            var info = GetButtonInfoEntity(button);
            if (args[0].Equals("create", StringComparison.OrdinalIgnoreCase))
            {
               
                if (info != null) SendReply(player, "This button is already a ButtonCommand button!");
                else
                {
                    info = new ButtonInfo(button);
                    buttonInfos.Add(info);
                    SendReply(player, "Created ButtonInfo for this button!");
                }
                return;
            }
            if (args[0].Equals("scmd"))
            {
                if (info == null)
                {
                    SendReply(player, "This PressButton is not a ButtonCommand button! Create it first.");
                    return;
                
                }


                var sCmdSb = Facepunch.Pool.Get<StringBuilder>();
                try
                {
                    sCmdSb.Clear();

                    for (int i = 1; i < args.Length; i++)
                    {
                        sCmdSb.Append(args[i]).AppendLine(" ");
                    }

                    if (sCmdSb.Length > 1) sCmdSb.Length -= 1;

                    
                    info.ServerCommand = sCmdSb.ToString();
                    SendReply(player, "Set ServerCommand to: " + info.ServerCommand);
                    return;
                }
                finally { Facepunch.Pool.Free(ref sCmdSb); }
            }
            
            if (args[0].Equals("pcmd"))
            {
                if (info == null)
                {
                    SendReply(player, "This PressButton is not a ButtonCommand button! Create it first.");
                    return;
                }

                var pCmdSb = Facepunch.Pool.Get<StringBuilder>();
                try 
                {
                    pCmdSb.Clear();
                    
                    for (int i = 1; i < args.Length; i++)
                    {
                        pCmdSb.Append(args[i]).AppendLine(" ");
                    }

                    if (pCmdSb.Length > 1) pCmdSb.Length -= 1;

                    
                    info.PlayerCommand = pCmdSb.ToString();
                    SendReply(player, "Set PlayerCommand to: " + info.PlayerCommand);
                    return;
                }
                finally { Facepunch.Pool.Free(ref pCmdSb); }

               
            }
            if (args[0].Equals("fx"))
            {
                if (info == null)
                {
                    SendReply(player, "This PressButton is not a ButtonCommand button! Create it first.");
                    return;
                }

                var fxSb = Facepunch.Pool.Get<StringBuilder>();
                try
                {
                    fxSb.Clear();

                    for (int i = 1; i < args.Length; i++)
                    {
                        fxSb.Append(args[i]).AppendLine(" ");
                    }

                    if (fxSb.Length > 1) fxSb.Length -= 1;


                    info.LocalFX = fxSb.ToString();
                    SendReply(player, "Set fx to: " + info.PlayerCommand);
                    return;
                }
                finally { Facepunch.Pool.Free(ref fxSb); }
            }
        }

        private void OnButtonPress(PressButton button, BasePlayer player)
        {
            if (button == null || button.IsDestroyed || button.IsOn() || player == null || player.IsDestroyed) return;

            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                var netId = button.net.ID;
                var info = GetButtonInfo(netId);

                if (info == null) return;

                if (!string.IsNullOrEmpty(info.PlayerCommand))
                {
                    var repStr = info.PlayerCommand.Replace("{userid}", player.UserIDString).Replace("''", "\"");
                    if (repStr.Contains(";"))
                    {
                        var split = repStr.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        int nextWait = 0;
                        int lastWait = 0;
                        for (int i = 0; i < split.Length; i++)
                        {
                            var str = split[i];
                            if (str.StartsWith("wait="))
                            {
                                var waitSplit = str.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                int wait;
                                if (waitSplit.Length > 1)
                                {
                                    if (!int.TryParse(waitSplit[1], out wait)) PrintWarning("Couldn't parse waitSplit length as an integer! str: " + str);
                                    else nextWait = lastWait + wait;
                                }
                                else PrintWarning("waitSplit with length < 1 on str: " + str);
                            }
                            else
                            {
                                if (nextWait > 0)
                                {
                                    player.Invoke(() => player.SendConsoleCommand(str), nextWait / 1000);
                                    lastWait = nextWait;
                                    nextWait = 0;
                                }
                                else player.SendConsoleCommand(str);
                            }

                        }
                    }
                    else player.SendConsoleCommand(repStr);
                }

                if (!string.IsNullOrEmpty(info.LocalFX))
                {
                    var userId = player.UserIDString;
                    int nextWait = 0;
                    int lastWait = 0;
                    timer.Once(0.2f, () =>
                    {
                        var fx = info.LocalFX;
                        if (fx.Contains(";"))
                        {
                            var split = fx.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < split.Length; i++)
                            {
                                var str = split[i];
                                if (str.StartsWith("wait="))
                                {
                                    var waitSplit = str.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                    int wait;
                                    if (waitSplit.Length > 1)
                                    {
                                        if (!int.TryParse(waitSplit[1], out wait)) PrintWarning("Couldn't parse waitSplit length as an integer! str: " + str);
                                        else nextWait = lastWait + wait;
                                    }
                                    else PrintWarning("waitSplit with length < 1 on str: " + str);
                                }
                                else
                                {
                                    if (nextWait > 0)
                                    {
                                        ServerMgr.Instance.Invoke(() => SendLocalEffect(player, info.LocalFX), nextWait / 1000);
                                        lastWait = nextWait;
                                        nextWait = 0;
                                    }
                                    else SendLocalEffect(player, info.LocalFX);
                                }

                            }
                        }
                        else SendLocalEffect(player, info.LocalFX);
                    });
                }

                if (!string.IsNullOrEmpty(info.ServerCommand))
                {
                    var userId = player.UserIDString;
                    int nextWait = 0;
                    int lastWait = 0;
                    timer.Once(0.2f, () =>
                    {
                        var repStr = info.ServerCommand.Replace("{userid}", userId).Replace("''", "\"");
                        if (repStr.Contains(";"))
                        {
                            var split = repStr.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < split.Length; i++)
                            {
                                var str = split[i];
                                if (str.StartsWith("wait="))
                                {
                                    var waitSplit = str.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                    int wait;
                                    if (waitSplit.Length > 1)
                                    {
                                        if (!int.TryParse(waitSplit[1], out wait)) PrintWarning("Couldn't parse waitSplit length as an integer! str: " + str);
                                        else nextWait = lastWait + wait;
                                    }
                                    else PrintWarning("waitSplit with length < 1 on str: " + str);
                                }
                                else
                                {
                                    if (nextWait > 0)
                                    {
                                        ServerMgr.Instance.Invoke(() => covalence.Server.Command(str), nextWait / 1000);
                                        lastWait = nextWait;
                                        nextWait = 0;
                                    }
                                    else covalence.Server.Command(str);
                                }

                            }
                        }
                        else covalence.Server.Command(repStr);
                    });

                }
            }
            finally
            {
                try
                {
                    if (watch.Elapsed.TotalMilliseconds > 5) PrintWarning(nameof(OnButtonPress) + " took: " + watch.Elapsed.TotalMilliseconds + "ms");
                }
                finally { Facepunch.Pool.Free(ref watch); }
            }
        }


        string RemoveTags(string phrase)
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
            for (int i = 0; i < forbiddenTags.Count; i++) phraseSB.Replace(forbiddenTags[i], string.Empty);

            return phraseSB.ToString();
        }

        void SendReply(IPlayer player, string msg, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : (keepTagsConsole) ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else player.Reply(msg);
        }

        BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (UnityEngine.Physics.Raycast(ray, out hit, maxDist, coll))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            if (coll == -1)
            {
                if (UnityEngine.Physics.Raycast(ray, out hit, maxDist, constructionColl))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }
            return null;
        }

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;

            using (var fx = new Effect(effect, pos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;

            using (var fx = new Effect(effect, player, boneID, localPos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }
    }
}