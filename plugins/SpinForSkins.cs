using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SpinForSkins", "Shady", "0.0.2", ResourceId = 0)]
    internal class SpinForSkins : RustPlugin
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
        private const string DATA_FILE_NAME = "UserSpinsData";


        private Dictionary<string, int> _spinCounts;

        private readonly Dictionary<string, Timer> _popupTimer = new Dictionary<string, Timer>();

        private readonly string[] _allowedColors = { "", "#75ff47", "#459b20", "#8800cc", "#3385ff", "#e6e600", "#669900", "#4d4dff", "#cc0000", "#33ffbb", "#ff00ff", "#725142", "#595959", "#00ffff", "#ffb3cc", "#5c8a8a", "#e6004c", "#db890f", "#ce7235", "#e64d00", "#2c528c", "#42f4e8", "#494f4e", "#b5c40f", "#681144", "#562c2c", "#356656", "#57776d", "#255143", "#1e7258", "#3c3051", "#512f8e", "#2c1d7c", "#DD0000", "#8877ad", "#736b84", "#ff72ad", "#b70b53", "#eaa8d3", "#770750", "#3144a0", "#55a4c1", "#4332fc", "#1d5b77", "#1281C6", "#3E3E3E", "#661B77", "#FACD78", "#E4DCA4", "#4F4533", "#ffe6e6", "#89dc93", "#2B5116", "#BE2227", "#c5a787", "#FF0030", "#FE950A", "#FEF506", "#53D559", "#2BACE2", "#9E88D2", "#58767D", "#525354", "#95A26D", "#552E84", "#939296", "#9B5B5D", "#cd2a2a", "#8D1028", "#C40C23", "#D95A19", "#ECA013", "#6B3421", "#733053", "#FBFF93", "#F9D0DD", "#ffe14a", "#ffabe9", "#ffe0f3", "#c7fcff", "#45f5ff", "#ccf4ff", "#fcffcc", "#f2ff26", "#ff8e52", "#fdebff", "#f494ff", "#c3a1ff", "#7b30ff", "#ff91e7", "#ff00c7", "#ffbff1", "#ff82e4", "#cfff82", "#b5ffd6", "#a0ff52", "#ffdd94", "#ff8fa6", "#2f1cff", "#1c20ff", "#7376ff", "#999bff", "#1d20cc", "#b09419", "#12628a", "#198791", "#37b38e", "#a32a5d", "#992c90", "#2e992c", "#55ab33", "#8aba3d", "#cee64c", "#faaf64", "#ffa587", "#ff9587", "#ff7070", "#8fdbff", "#532696", "#96267e", "#177578", "#006366", "#00521d", "#474000", "#402800", "#421300", "#6e2102", "#8f0c00", "#1b00b3", "#69111e", "#380000", "#000000" };


        [PluginReference]
        private readonly Plugin SkinsAPI;

        [PluginReference]
        private readonly Plugin UserSkins;

        #endregion

        #region Hooks
        private void Init()
        {
            AddCovalenceCommand("setspins", nameof(cmdSetSpins));
            AddCovalenceCommand("addspins", nameof(cmdAddSpins));

            _spinCounts = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<string, int>>(DATA_FILE_NAME) ?? null;
        }

        private void Unload() => SaveSpinsData();

     
        #endregion
        #region Commands
      

        [ChatCommand("skins")]
        private void cmdRngskin(BasePlayer player, string command, string[] args)
        {
            try
            {

                var pSkins = Facepunch.Pool.GetList<ulong>();
                try 
                {
                    UserSkins?.Call("GetAllUserSkinsNoAlloc", player.UserIDString, pSkins);


                    PrintWarning("pSkins.Count: " + pSkins.Count.ToString("N0") + " for: " + player.displayName + "/" + player.UserIDString);

                    var spins = GetSkinSpins(player.UserIDString);
                    if (args.Length < 1)
                    {
                        SendReply(player, "<color=#F24998>You have <color=#33ff33>" + spins.ToString("N0") + "</color> available rolls.</color>\n<color=#80ffff>You can use these by typing: <color=#42dfff>/" + command + " roll</color></color>" + Environment.NewLine + "<color=#8aff47>You can view all <color=#eb3dff>" + (pSkins?.Count ?? 0).ToString("N0") + "</color> of your unlocked skins by typing <color=#eb3dff>/" + command + " list</color>.\nYou can get rolls to use by <color=#eb3dff>voting!</color> Type <color=#42dfff>/voteclaim</color> for more info.</color>\n\n<color=#eb3dff>You can use your skins by typing <i><color=#80ffff>/skin</color></i> (without the last 'S'!)</color>");
                        return;
                    }
                    var arg0Lower = args[0].ToLower();
                    if (arg0Lower != "roll" && arg0Lower != "list")
                    {
                        SendReply(player, "<color=#ff3300>Invalid argument:</color> " + args[0] + Environment.NewLine + "<color=#99ff33>Type <color=#42dfff>/" + command + "</color> to see more information.</color>");
                        return;
                    }

                    if (arg0Lower == "list")
                    {
                        if (pSkins == null || pSkins.Count < 1)
                        {
                            SendReply(player, "You do not currently have any skins!");
                            return;
                        }

                        var skinSB = new StringBuilder();

                        var SBs = Facepunch.Pool.GetList<StringBuilder>();

                        try
                        {
                            for (int i = 0; i < pSkins.Count; i++)
                            {
                                var skinId = pSkins[i];
                                var name = SkinsAPI?.Call<string>("GetSkinNameFromID", skinId.ToString()) ?? string.Empty;

                                if (string.IsNullOrEmpty(name)) continue;

                                if ((skinSB.Length + name.Length) > 800)
                                {
                                    SBs.Add(skinSB);
                                    skinSB = new StringBuilder();
                                }
                                var col = IsDivisble(i, 2) ? "#BE2227" : "#569E2C";
                                skinSB.Append("<color=").Append(col).Append(">").Append(name).Append("</color>, ");
                            }

                            if (SBs.Count > 0) for (int i = 0; i < SBs.Count; i++) SendReply(player, SBs[i].ToString().TrimEnd(", ".ToCharArray()));
                            if (skinSB.Length > 0) SendReply(player, skinSB.ToString().TrimEnd(", ".ToCharArray()));
                            if (SBs.Count > 0 || skinSB.Length > 0) SendReply(player, "<color=#eb3dff>Showing <color=#8aff47>" + pSkins.Count.ToString("N0") + "</color> unlocked skins.</color>");
                        }
                        finally { Facepunch.Pool.FreeList(ref SBs); }

                        return;
                    }
                    //else "rolls"
                    if (spins < 1)
                    {
                        SendReply(player, "<color=#ff1a1a>You do not have any rolls left!</color>");
                        if (!player.IsAdmin) return;
                        else SendReply(player, "Overriding spins because you are an admin.");
                    }

                    var rngSkin = SkinsAPI?.Call<ulong>("GetRandomSkinExcluding", pSkins) ?? 0ul;
                    if (rngSkin == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            rngSkin = SkinsAPI?.Call<ulong>("GetRandomSkinExcluding", pSkins) ?? 0ul;
                            if (rngSkin != 0) break;
                        }
                    }

                    var admStr = "Try again or report this issue to an administrator.";
                    if (rngSkin == 0)
                    {
                        SendReply(player, "Couldn't get a random skin! " + admStr);
                        return;
                    }

                    var skinName = SkinsAPI?.Call<string>("GetSkinNameFromID", rngSkin.ToString()) ?? string.Empty;

                    if (string.IsNullOrEmpty(skinName))
                    {
                        SendReply(player, "Skin name is null/empty for: " + rngSkin + Environment.NewLine + admStr);
                        return;
                    }

                    var itemId = SkinsAPI?.Call<int>("GetItemIDFromSkin", rngSkin) ?? 0;
                    if (itemId == 0)
                    {
                        SendReply(player, "Couldn't get item from skin: " + rngSkin + "! " + admStr);
                        return;
                    }


                    ItemDefinition itemDef = null;
                    for(int i = 0; i < ItemManager.itemList.Count; i++)
                    {
                        var def = ItemManager.itemList[i];
                        if (def?.itemid == itemId)
                        {
                            itemDef = def;
                            break;
                        }
                    }

                    if (itemDef == null)
                    {
                        SendReply(player, "Item is null for ID: " + itemId + Environment.NewLine + admStr);
                        return;
                    }

                    PrintWarning(player?.displayName + " (" + player?.UserIDString + ") is going to get skin: " + skinName + " from /" + command);
                    if (!player.IsAdmin) SetSkinSpins(player.UserIDString, spins - 1);



                    //ADD THE SKIN

                    UserSkins?.Call("AddAccessToSkin", player.UserIDString, rngSkin);


                    ClearPlayerChat(player);
                    ShowPopup(player.UserIDString, "Rolling for a skin...", 3.5f);
                    for (int i = 0; i < UnityEngine.Random.Range(2, 6); i++) SendLocalEffect(player, "assets/prefabs/deployable/spinner_wheel/effects/spinner-wheel-deploy.prefab");
                    //  var effectTime = 0.1f;
                    var periodCount = 1;
                    var time = 0.066f;
                    var max = 48;
                    var timerI = 0;
                    var useColors = _allowedColors.Where(p => !string.IsNullOrEmpty(p));
                    timer.Once(0.1f, () =>
                    {

                        for (int i = 0; i < max; i++)
                        {

                            timer.Once(time, () =>
                            {
                                SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/loot.copy.fx.prefab");
                                timerI++;
                                if (periodCount > 3) periodCount = 1;
                                var dotStr = "";
                                for (int j = 0; j < periodCount; j++) dotStr += ".";
                                var rollStr = "<size=" + UnityEngine.Random.Range(17, 21) + "><color=" + RandomElement(useColors, new System.Random()) + ">Rolling for random skin" + dotStr + "</color></size>";
                                SendReply(player, rollStr);
                                ShowPopup(player.UserIDString, rollStr, time / 1.5f);
                                periodCount++;
                                if (timerI == max)
                                {
                                    timer.Once(0.42f, () =>
                                    {
                                        ClearPlayerChat(player);
                                        var skinItemStr = skinName + " (" + (itemDef?.displayName?.english ?? "Unknown Item") + ")";
                                        SendReply(player, "<size=21><color=" + RandomElement(useColors, new System.Random()) + ">You've unlocked <color=" + RandomElement(useColors, new System.Random()) + ">" + skinName + "</color> (<color=" + RandomElement(useColors, new System.Random()) + ">" + (itemDef?.displayName?.english ?? "Unknown Item") + "</color>)!</color></size>");
                                        ShowPopup(player.UserIDString, "<color=" + RandomElement(useColors, new System.Random()) + ">" + skinItemStr + "</color>!");
                                        for (int j = 0; j < UnityEngine.Random.Range(2, 6); j++)
                                        {
                                            Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", player.eyes.transform.position + player.eyes.HeadForward() * 2.2f, Vector3.zero);
                                        }
                                        SendLocalEffect(player, "assets/prefabs/deployable/repair bench/effects/skinchange_spraypaint.prefab");
                                        timer.Once(0.125f, () =>
                                        {
                                            for (int j = 0; j < UnityEngine.Random.Range(1, 3); j++) SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");
                                        });
                                        player.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
                                    });

                                }
                            });


                            time += 0.045f + (0.00555f * (i / 1.5f)) + ((i >= 44) ? 0.0525f : 0f);


                        }

                    });

                }
                finally { Facepunch.Pool.FreeList(ref pSkins); }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                SendReply(player, "Failed to complete command because an error happened. Please report this to an administrator immediately!");
            }
        }

        private void cmdAddSpins(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args == null || args.Length < 2) return;
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                player.Message("No player found: " + args[0]);
                return;
            }
            int count;
            if (!int.TryParse(args[1], out count))
            {
                player.Message("Not an int: " + args[1]);
                return;
            }
            AddSkinSpins(target.Id, count);
            player.Message("Added spins: " + count + " (total now: " + GetSkinSpins(target.Id) + ") for: " + target?.Name);
        }

        private void cmdSetSpins(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args == null || args.Length < 2) return;
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                player.Message("No player found: " + args[0]);
                return;
            }
            int count;
            if (!int.TryParse(args[1], out count))
            {
                player.Message("Not an int: " + args[1]);
                return;
            }
            SetSkinSpins(target.Id, count);
            player.Message("Set spins to: " + count + " for: " + target?.Name);
        }
        #endregion
        #region Util

        public bool IsDivisble(int x, int n) { return (x % n) == 0; }

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

        private void ClearPlayerChat(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                sb.Clear();
                for (int i = 0; i < 100; i++) sb.AppendLine("\n");
                for (int i = 0; i < 10; i++) SendReply(player, sb.ToString());
            }
            finally { Facepunch.Pool.Free(ref sb); }

        }

        public T RandomElement<T>(IEnumerable<T> source, System.Random rng)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
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

        private int GetSkinSpins(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return 0;
            var count = 0;
            if (_spinCounts != null) _spinCounts.TryGetValue(userId, out count);
            return count;
        }

        private void SetSkinSpins(string userId, int amount)
        {
            _spinCounts[userId] = amount;
        }

        private void AddSkinSpins(string userId, int add)
        {
            SetSkinSpins(userId, GetSkinSpins(userId) + add);
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

        private void SaveSpinsData() => Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _spinCounts);

        private void SendInitMessage(BasePlayer player, string msg)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            var isSnap = player?.IsReceivingSnapshot ?? true;
            if (isSnap) timer.Once(1.5f, () => SendInitMessage(player, msg));
            else SendReply(player, msg);
        }
        #endregion
    }
}