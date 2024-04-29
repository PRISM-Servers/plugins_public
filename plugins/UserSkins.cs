using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxide.Plugins
{
    [Info("UserSkins", "Shady", "0.0.4", ResourceId = 0)]
    internal class UserSkins : RustPlugin
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
        private const string DATA_FILE_NAME = "UserSkinData";

        private const string STEAM_INVENTORY_URL = "https://steamcommunity.com/inventory/{0}/252490/2?l=english&count=5000";
        //private const string STEAM_INVENTORY_URL = "https://steamcommunity.com/profiles/{0}/inventory/json/252490/2";

        private Dictionary<string, UserSkinInfo> _playerSkins;

        [PluginReference]
        private readonly Plugin SkinsAPI;

        #endregion
        #region Classes
        private class UserSkinInfo
        {
            public List<ulong> Skins { get; set; } = new List<ulong>();

            public bool AddSkin(ulong skinId)
            {
                if (!Skins.Contains(skinId))
                {
                    Skins.Add(skinId);
                    return true;
                }

                return false;
            }

            public bool HasSkin(ulong skinId)
            {
                return Skins != null && Skins.Contains(skinId);
            }
            
        }
        #endregion

        #region Hooks
        private void Init()
        {
            _playerSkins = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<string, UserSkinInfo>>(DATA_FILE_NAME) ?? null;

            AddCovalenceCommand("skint", nameof(cmdTest));
        }

        private void cmdTest(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected) return;

            var pObj = player?.Object as BasePlayer;

            if (pObj != null) OnPlayerConnected(pObj);

            player.Reply("Attempted to send onplayerconnected");
        }

        private void Unload() => SaveSkinData();

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;

            var userId = player?.UserIDString ?? string.Empty;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try 
            {
                var url = sb.Clear().Append(STEAM_INVENTORY_URL).Replace("{0}", userId).ToString();
                PrintWarning("Url is: " + url + " for: " + userId);
               
                
                webrequest.Enqueue(url, null, (code, response) =>
                {
                    try
                    {
                        if (code != 200 || string.IsNullOrEmpty(response))
                        {
                            PrintWarning("Got non 200 (" + code + ") or null/empty response for: " + player.displayName + " with URL: " + url);
                            SendInitMessage(player, "<color=red>We were unable to pull your Steam skins!</color> <color=orange>Your inventory may be private. Because of this, /skinbox will not show the skins you own on Steam.</color>");
                            SendInitMessage(player, "<color=orange>You can use your Steam skins using</color><color=#ff912b>/skinbox</color> <color=orange><i>if</i> you make your Steam inventory public!</color>");
                            return;
                        }

                      //  if ((!response.Contains("private") && !response.Contains("rgDesc")) || response.Contains(@"</div>")) return;
                        
                        if (response.Contains("private"))
                        {
                            PrintWarning("response contained private!!!: " + url);
                            SendInitMessage(player, "<color=orange>You can use your Steam skins using</color><color=#ff912b>/skinbox</color> <color=orange><i>if</i> you make your Steam inventory public!</color>");
                            return;
                        }
                        if (!response.Contains("descriptions"))
                        {
                            Puts(response);
                            return;
                        }
                        var jsonsettings = new JsonSerializerSettings();
                        jsonsettings.Converters.Add(new KeyValuesConverter());

                        var jsonresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                        if ((jsonresponse?.Count ?? 0) < 1)
                        {
                            PrintWarning("Bad json response");
                            return;
                        }
                        else PrintWarning("got jsonresponse");

                        var skinNames = Facepunch.Pool.GetList<string>();
                        try 
                        {
                            foreach (var kvp in jsonresponse)
                            {
                                var value2 = (kvp.Value != null && kvp.Value is Dictionary<string, object>) ? (kvp.Value as Dictionary<string, object>) : null;
                                if ((value2?.Count ?? 0) < 1)
                                {
                                    PrintWarning("value2 count < 1!!: is null? " + (value2 == null) + ", kvp.value?: " + kvp.Value + ", " + kvp.Value?.GetType());
                                    continue;
                                }
                                else PrintWarning("value2 count NOT < 1: " + value2.Count);

                                if (!kvp.Key.Equals("descriptions", StringComparison.OrdinalIgnoreCase))
                                {
                                    PrintWarning("no descriptions key: " + value2);
                                    continue;
                                }
                                else PrintWarning("found descriptions key: " + kvp.Key);

                                foreach (var kvp2 in value2)
                                {
                                    var value3 = (kvp2.Value != null && kvp2.Value is Dictionary<string, object>) ? (kvp2.Value as Dictionary<string, object>) : null;
                                    if ((value3?.Count ?? 0) < 1)
                                    {
                                        PrintWarning("value3 count < 1!!!");
                                        continue;
                                    }

                                    foreach (var kvp3 in value3)
                                    {
                                        var nameVal = kvp3.Value.ToString();
                                        if (kvp3.Key.Equals("name", StringComparison.OrdinalIgnoreCase) && !skinNames.Contains(nameVal))
                                        {
                                            skinNames.Add(nameVal);
                                        }
                                    }

                                }

                            }

                            if (skinNames.Count > 0)
                            {
                                PrintWarning("skinNames count: " + skinNames.Count);

                                UserSkinInfo info;
                                if (!_playerSkins.TryGetValue(userId, out info))
                                {
                                    info = new UserSkinInfo();
                                    _playerSkins[userId] = info;
                                }

                                for (int i = 0; i < skinNames.Count; i++)
                                {
                                    var skinName = skinNames[i];

                                    var skinId = SkinsAPI?.Call<ulong>("GetSkinIDFromName", skinName) ?? 0;
                                    if (skinId == 0)
                                    {
                                        PrintWarning("no skinId from name: " + skinName);
                                    }
                                    else
                                    {
                                        if (info.AddSkin(skinId)) PrintWarning("added new skin: " + skinId + " (" + skinName + ")");
                                       // else PrintWarning("did not add new skin because it already existed");
                                    }


                                }

                            }
                            else PrintWarning("skinNames.count < 1");
                        }
                        finally { Facepunch.Pool.FreeList(ref skinNames); }

                      

                    }
                    catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete inventory web request^"); }
                }, this);

            }
            finally { Facepunch.Pool.Free(ref sb); }
            
        }
        #endregion
        #region Util
        private void SaveSkinData() => Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _playerSkins);

        private void SendInitMessage(BasePlayer player, string msg)
        {
            if (player == null) throw new ArgumentNullException(nameof(player)); // || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            if (string.IsNullOrEmpty(msg)) throw new ArgumentNullException(nameof(msg));

            if (!player.IsConnected) return;

            var isSnap = player?.IsReceivingSnapshot ?? true;
            if (isSnap) timer.Once(1.5f, () => SendInitMessage(player, msg));
            else SendReply(player, msg);
        }
        #endregion
        #region Exposed API
        private bool AddAccessToSkin(string userId, ulong skinId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            UserSkinInfo info;
            if (!_playerSkins.TryGetValue(userId, out info))
            {
                _playerSkins[userId] = (info = new UserSkinInfo());
            }

            return info.AddSkin(skinId);
        }
        private bool HasAccessToSkin(string userId, ulong skinId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            UserSkinInfo info;
            if (!_playerSkins.TryGetValue(userId, out info)) return false;

            return info.HasSkin(skinId);
        }
        private List<ulong> GetAllUserSkins(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            UserSkinInfo info;
            if (!_playerSkins.TryGetValue(userId, out info)) return null;

            return new List<ulong>(info.Skins);
        }
        private void GetAllUserSkinsNoAlloc(string userId, ref List<ulong> list)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            UserSkinInfo info;
            if (!_playerSkins.TryGetValue(userId, out info)) return;

            for(int i = 0; i < info.Skins.Count; i++) list.Add(info.Skins[i]);
        }
        #endregion
    }
}