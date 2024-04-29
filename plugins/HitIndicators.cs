using System.Collections.Generic;
using System;
using Oxide.Core;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using System.Globalization;
using System.Text.RegularExpressions;
using Facepunch;

namespace Oxide.Plugins
{
    [Info("HitIndicators", "Shady", "1.0.3", ResourceId = 0)]
    internal class HitIndicators : RustPlugin
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

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private StoredData _storedData;

        private StoredData storedData
        {
            get
            {
                if (_storedData == null)
                {
                    if ((_storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>("HitIndicatorData2")) == null) _storedData = new StoredData();
                }
                return _storedData;
            }
            set { _storedData = value; }
        }

        private List<string> EnabledList
        {
            get { return storedData?.enabledIDs ?? new List<string>(); }
            set
            {
                if (storedData != null) storedData.enabledIDs = value;
            }
        }

        private class StoredData
        {
            public List<string> enabledIDs = new List<string>();
            public StoredData() { }
        }

        private void OnServerInitialized()
        {
            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player != null && !hitIndicators(player.UserIDString)) EnabledList.Add(player.UserIDString);
            }
            Subscribe(nameof(OnEntityTakeDamage));
        }

        private void Unload() => SaveData();

        private void Init()
        {
            Unsubscribe(nameof(OnEntityTakeDamage));
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            if (!hitIndicators(player.UserIDString)) EnabledList.Add(player.UserIDString);
        }

        private readonly Dictionary<string, StringBuilder> sbBuffer = new Dictionary<string, StringBuilder>();

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            var player = ((info?.Initiator ?? null) as BasePlayer) ?? info?.Weapon?.GetItem()?.GetOwnerPlayer() ?? info?.WeaponPrefab?.GetItem()?.GetOwnerPlayer() ?? null;
            if (player == null) return;

            var victim = entity as BasePlayer;

            var wepPrefab = info?.WeaponPrefab?.ShortPrefabName ?? string.Empty;
            var prefabname = entity?.ShortPrefabName?.ToLower() ?? string.Empty;

            var heli = entity as PatrolHelicopter;
            var isNPC = entity is BaseNpc || entity is Horse || entity is BaseRidableAnimal || entity is RidableHorse;
            var isBradley = entity is BradleyAPC;

            if (!player.IsConnected || (victim == null && !isNPC && heli == null && !isBradley) || victim == player) return;

            var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            var boneID = info?.HitBone ?? 0;

            StringBuilder boneSB = null;

            if (heli?.weakspots != null && heli.weakspots.Length > 0)
            {
                for (int i = 0; i < heli.weakspots.Length; i++)
                {
                    var weakspots = heli.weakspots[i];
                    if (weakspots?.bonenames != null && weakspots.bonenames.Length > 0)
                    {
                        for (int j = 0; j < weakspots.bonenames.Length; j++)
                        {
                            var heliBone = weakspots.bonenames[j];
                            var getBone = StringPool.Get(heliBone);
                            if (getBone == boneID)
                            {
                                boneSB = new StringBuilder(heliBone);
                                break;
                            }
                        }
                    }
                }
            }

            if (boneSB == null) boneSB = new StringBuilder(GetBoneName(entity, boneID));

            var boneName = FirstUpper(boneSB.Replace("_col", string.Empty).Replace("_", " ").ToString());
            
            if (hitIndicators(player.UserIDString) && majDmg != Rust.DamageType.Bleeding && majDmg != Rust.DamageType.Suicide)
            {
                NextTick(() =>
                {
                    var damages = info?.damageTypes?.Total() ?? 0f;
                    if (damages <= 0.0f) return;

                    if (majDmg == Rust.DamageType.Stab && ((info?.Weapon?.GetItem()?.GetHeldEntity()?.GetComponent<BaseMelee>() ?? null) == null)) return;

                    //    if (victim != null) Effect.server.Run("assets/bundled/prefabs/fx/player/bloodspurt_wounded_pelvis.prefab", victim, boneID, (info?.HitPositionLocal ?? Vector3.zero), Vector3.zero);

                    var sb = Pool.Get<StringBuilder>();

                    var dmgStr = string.Empty;
                    var msg = string.Empty;

                    try 
                    {
                        dmgStr = sb.Clear().Append(damages.ToString((damages >= 1.0) ? "N0" : "0.0")).Replace(".0", string.Empty).ToString();
                        //new StringBuilder(damages.ToString((damages >= 1.0) ? "N0" : "0.0")).Replace(".0", string.Empty).ToString();


                        msg = sb.Clear().Append("Hit <color=#FF0030>").Append(boneName).Append("</color> for <color=#FF0030>").Append(dmgStr).Append("</color> dmg.").ToString();

                    }
                    finally { Pool.Free(ref sb); }

              
                    StringBuilder outSB;

                    if (!sbBuffer.TryGetValue(player.UserIDString, out outSB)) 
                        sbBuffer[player.UserIDString] = outSB = new StringBuilder();

                    outSB.Append(msg).Append(Environment.NewLine);
                    sbBuffer[player.UserIDString] = outSB;

                    player?.Invoke(() =>
                    {
                        if (player == null || !player.IsConnected) return;

                        StringBuilder nowSB;

                        if (!sbBuffer.TryGetValue(player.UserIDString, out nowSB) || nowSB.Length < 1)
                            return;

                        if (nowSB.Length > 1)
                            nowSB.Length--;

                        SendReply(player, nowSB.ToString());
                        sbBuffer[player.UserIDString].Clear();

                    }, 0.1f);
                });
            }
        }

        [ChatCommand("hit")]
        private void cmdHI(BasePlayer player, string command, string[] args)
        {
            ToggleIndicator(player.UserIDString);
            SendReply(player, "Hit notifications (chat) enabled: <color=#ff912b>" + hitIndicators(player.UserIDString) + "</color>");
        }

        #region Util
        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("HitIndicatorData2", storedData);
        private string GetBoneName(BaseCombatEntity entity, uint boneId) => entity?.skeletonProperties?.FindBone(boneId)?.name?.english ?? "Body";

        private bool hitIndicators(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            return storedData?.enabledIDs?.Contains(userID) ?? false;
        }

        private void EnableIndicator(string userID) { if (!hitIndicators(userID)) EnabledList.Add(userID); }

        private void DisableIndicator(string userID) { if (hitIndicators(userID)) EnabledList.Remove(userID); }

        private void ToggleIndicator(string userID)
        {
            if (hitIndicators(userID)) DisableIndicator(userID);
            else EnableIndicator(userID);
        }

        private string FirstUpper(string original)
        {
            if (string.IsNullOrEmpty(original)) throw new ArgumentNullException(nameof(original));

            var array = original.ToCharArray();
            array[0] = char.ToUpper(array[0], CultureInfo.CurrentCulture);
            return new string(array);
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

        #endregion
    }
}