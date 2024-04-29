using Oxide.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Pool = Facepunch.Pool;
using Oxide.Core.Libraries.Covalence;
using System.Text;
using System.Text.RegularExpressions;
using Facepunch;

namespace Oxide.Plugins
{
    [Info("Rad Rain", "Shady", "0.0.3")]
    class RadRain : RustPlugin
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
        private readonly HashSet<Coroutine> _coroutines = new();

        private readonly HashSet<LootContainer> _lootCrates = new();

        private Coroutine _crateCoroutine = null;

        private readonly HashSet<string> _forbiddenTags = new()
        { "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>" };


        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private const string CHAT_STEAM_ID = "76561198865536053";

        private const int PLAYER_LAYER_MASK = 131072;

        private const uint CRATE_UNDERWATER_BASIC_PREFAB_ID = 3852690109;
        private const uint CRATE_ELITE_PREFAB_ID = 3286607235;
        private const uint CRATE_UNDERWATER_ELITE_PREFAB_ID = 96231181;
        private const uint CRATE_HELI_PREFAB_ID = 1314849795;
        private const uint CRATE_NORMAL_PREFAB_ID = 2857304752;
        private const uint CRATE_NORMAL_2_PREFAB_ID = 1546200557;
        private const uint CRATE_TOOLS_PREFAB_ID = 1892026534;
        private const uint CRATE_UNDERWATER_ADVANCED_PREFAB_ID = 2803512399;
        private const uint CRATE_UNDERWATER_NORMAL_PREFAB_ID = 1009499252;
        private const uint CRATE_UNDERWATER_NORMAL_2_PREFAB_ID = 2276830067;
        private const uint CRATE_UNDERWATER_TOOLS_PREFAB_ID = 3027334492;
        private const uint CRATE_BRADLEY_ID = 1737870479;
        private const uint CRATE_LOCKED_CHINOOK = 209286362;
        private const uint CRATE_LOCKED_OILRIG = 2043434947;
        private const uint CRATE_AMMO_UNDERGROUND_PREFAB_ID = 2439530480;

        private const uint CINELIGHT_POINT_GREEN_PREFAB_ID = 2423685124;

        private readonly HashSet<uint> _applicableCrateReplacements = new()
        {
        CRATE_UNDERWATER_BASIC_PREFAB_ID,
        CRATE_ELITE_PREFAB_ID,
        CRATE_UNDERWATER_ELITE_PREFAB_ID,
        CRATE_HELI_PREFAB_ID,
        CRATE_NORMAL_PREFAB_ID,
        CRATE_NORMAL_2_PREFAB_ID,
        CRATE_TOOLS_PREFAB_ID,
        CRATE_UNDERWATER_ADVANCED_PREFAB_ID,
        CRATE_UNDERWATER_NORMAL_PREFAB_ID,
        CRATE_UNDERWATER_NORMAL_2_PREFAB_ID,
        CRATE_UNDERWATER_TOOLS_PREFAB_ID,
        CRATE_BRADLEY_ID,
        CRATE_AMMO_UNDERGROUND_PREFAB_ID
    };

        private EnvSync _envSync = null;
        public EnvSync EnvSync
        {
            get
            {
                if (_envSync == null)
                {
                    foreach (var entity in BaseNetworkable.serverEntities)
                    {
                        var sync = entity as EnvSync;
                        if (sync != null)
                        {
                            _envSync = sync;
                            break;
                        }
                    }
                }
                return _envSync;
            }
        }

        [ChatCommand("rr")]
        private void cmdRadRain(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            ToggleRadStorm(!IsRadStorm);

            SendReply(player, "is now: " + IsRadStorm);
        }

        private void Init()
        {
        }

        private void OnServerInitialized()
        {
            foreach (var entity in BaseNetworkable.serverEntities)
                if (entity is LootContainer loot)
                    _lootCrates.Add(loot);
            
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity is not LootContainer loot)
                return;

            _lootCrates.Add(loot);

        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity is not LootContainer loot)
                return;

            _lootCrates.Remove(loot);
        }

        private void Unload()
        {
            try 
            {
                foreach (var routine in _coroutines)
                {
                    if (routine == null)
                        continue;

                    try { ServerMgr.Instance.StopCoroutine(routine); }
                    catch(Exception ex) { PrintError(ex.ToString()); }
                }

                _coroutines.Clear();
            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            foreach (var kvp in _playerRadTrigger)
            {
                try
                {
                    if (kvp.Value == null)
                        continue;

                    if (kvp.Key?.triggers != null)
                        kvp.Key.LeaveTrigger(kvp.Value);
                }
                finally { UnityEngine.Object.Destroy(kvp.Value); }
            }

        }

        public bool IsRadStorm { get; private set; } = false;

        private bool IsRadStormActive() => IsRadStorm;

        private void ToggleRadStorm(bool radStorm, bool callHook = true)
        {
            IsRadStorm = radStorm;

            if (radStorm)
                Subscribe(nameof(OnRunPlayerMetabolism));
            else Unsubscribe(nameof(OnRunPlayerMetabolism));

            if (callHook)
                Interface.Oxide.CallHook(radStorm ? "OnRadStormStarted" : "OnRadStormEnded");

            if (radStorm)
                IOnRadStormStarted();
            else IOnRadStormEnded();

        }

        private void IOnRadStormStarted()
        {
            if (_crateCoroutine != null)
                StopCoroutine(_crateCoroutine);

            _crateCoroutine = StartCoroutine(MutateCrates());

            SimpleBroadcast("A radiation storm has begun!\nStay inside, or acquire rads. Odd things may happen outside!", CHAT_STEAM_ID);

            BroadcastToast("Rad storm inbound!", 1);

        }

        private void IOnRadStormEnded()
        {
            if (_crateCoroutine != null)
                StopCoroutine(_crateCoroutine);

            SimpleBroadcast("A radiation storm has ended.");
            BroadcastToast("Rad storm ending...");
        }

        private readonly Dictionary<BasePlayer, TriggerRadiation> _playerRadTrigger = new();

        private TriggerRadiation GetOrAddTriggerRad(BasePlayer player)
        {
            if (!_playerRadTrigger.TryGetValue(player, out var trigger))
                _playerRadTrigger[player] = trigger = player.gameObject.AddComponent<TriggerRadiation>();

            if (player.triggers == null || !player.triggers.Contains(trigger))
                player.EnterTrigger(trigger);

            var sphereC = trigger?.GetComponent<SphereCollider>() ?? trigger.gameObject.AddComponent<SphereCollider>();

            sphereC.radius = 1f;

            return _playerRadTrigger[player];
        }


        private void RemoveRadTrigger(BasePlayer player)
        {
            if (!_playerRadTrigger.TryGetValue(player, out var trigger))
                return;

            try
            {
                player.LeaveTrigger(trigger);

                UnityEngine.Object.Destroy(trigger);

                _playerRadTrigger.Remove(player);
            }
            catch(Exception ex) { PrintError(ex.ToString()); }

          
        }

        private void OnRunPlayerMetabolism(PlayerMetabolism metabolism, BasePlayer player, float delta)
        {
            if (!IsRadStorm)
                return;

            if (player.IsOutside(player.eyes.position))
            {
                var f2 = Climate.GetRain(player.eyes.position) * 1.25f; //GetRain * 1.25f gives you a max radiation poisoning value in-game of about 10 (radiation_level of 0.625) (this comment could be wrong; investigate).
                var f3 = Climate.GetSnow(player.eyes.position) * 1f; //a Climate.Get value of "1" is full rain/snow (100%).

                var finalVal = Mathf.Max(f2, f3);

                var trig = GetOrAddTriggerRad(player);

                trig.radiationTier = TriggerRadiation.RadiationTier.NONE;
                trig.RadiationAmountOverride = finalVal;

                //sendcooldownmessage about rad rain and getting a roof over your head.

            }
            else RemoveRadTrigger(player); //maybe optimize by setting radiation value to 0 instead of destroying?

        }

        private void OnStartThunderstorm()
        {
            PrintWarning("A thunderstorm has begun!");

            ToggleRadStorm(true);
        }

        private void OnEndThunderstorm()
        {
            PrintWarning("A thunderstorm has ended!");

            ToggleRadStorm(false);
        }

        #region Util
        private Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var coroutine = ServerMgr.Instance.StartCoroutine(routine);

            _coroutines.Add(coroutine);

            return coroutine;
        }

        private void StopCoroutine(Coroutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            ServerMgr.Instance.StopCoroutine(routine);

            _coroutines.Remove(routine);
        }


        private IEnumerator MutateCrates()
        {
            
            PrintWarning(nameof(MutateCrates));

            var replaceRng = new System.Random();


            while (true)
            {
                var toKill = Pool.Get<HashSet<LootContainer>>();

                var copyCat = Pool.Get<HashSet<LootContainer>>();

                try
                {
                    toKill.Clear();

                    copyCat.Clear();

                    foreach (var crate in _lootCrates)
                        copyCat.Add(crate);

                    foreach (var crate in copyCat)
                    {
                        if (crate == null || crate.IsDestroyed)
                            continue;

                        if (Random.Range(0, 701) > 1)
                            continue;

                        var replacementId = RandomElement(_applicableCrateReplacements, replaceRng);

                        var safety = 0;
                        while (replacementId == crate.prefabID) //try not to replace with same crate type.
                        {
                            if (safety > 10)
                                break;


                            replacementId = RandomElement(_applicableCrateReplacements, replaceRng);

                            safety++;
                        }

                        var prefabName = StringPool.Get(replacementId);

                        if (string.IsNullOrWhiteSpace(prefabName))
                        {
                            PrintError(nameof(prefabName) + " is empty!!! - stringpool.get failed!");
                            continue;
                        }

                        var newCrate = GameManager.server.CreateEntity(prefabName, crate.transform.position, crate.transform.rotation) as LootContainer;

                        newCrate.Spawn();

                        newCrate.EnableSaving(false);

                        newCrate.inventory.SetLocked(true);
                        newCrate.Invoke(() =>
                        {
                            newCrate?.inventory?.SetLocked(false);
                        }, 2f);

                        var oldParent = crate?.GetParentEntity();
                        if (oldParent != null)
                            newCrate.SetParent(oldParent, true, false);
                        else
                        {
                            JunkPile junkPile = null;

                            if (Physics.Raycast(newCrate.transform.position, Vector3.down, out var hitInfo, 4f, 1) || Physics.Raycast(newCrate.transform.position + Vector3.up * 5f, Vector3.down, out hitInfo, 4f, 1) || Physics.Raycast(newCrate.transform.position + Vector3.down * 1.25f, Vector3.down, out hitInfo, 4f, 1))
                                junkPile = GameObjectEx.ToBaseEntity(hitInfo.collider) as JunkPile;


                            if (junkPile == null)
                            {
                                var junkPiles = Pool.GetList<JunkPile>();

                                try
                                {
                                    Vis.Entities(newCrate.transform.position, 6f, junkPiles, 1);

                                    if (junkPiles.Count > 0)
                                        junkPile = junkPiles[0];
                                }
                                finally { Pool.FreeList(ref junkPiles); }
                            }

                            if (junkPile != null)
                            {
                                newCrate.SetParent(junkPile, true, false);
                                PrintWarning("set junkpile parent: " + junkPile.ShortPrefabName + " @ " + junkPile.transform.position);
                            }
                        }

                        AttachMutatedLightToCrate(newCrate);

                        toKill.Add(crate);

                        SendRadiusMessage(crate.transform.position, 12f, "A nearby crate has mutated!", 1);
                        //play sfx.


                    }

                    foreach (var crate in toKill)
                    {
                        if (crate == null)
                            continue;

                        _lootCrates.Remove(crate);

                        if (!crate.IsDestroyed)
                            crate.Kill();

                    }

                }
                finally
                {
                    try
                    {
                        Pool.Free(ref toKill);
                    }
                    finally { Pool.Free(ref copyCat); }
                }

             

                yield return CoroutineEx.waitForSecondsRealtime(15f);
            }
        }

        private void SendRadiusMessage(Vector3 pos, float radius, string message, int showTipStyle = -1)
        {
            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            var nearPlayers = Pool.GetList<BasePlayer>();

            try
            {
                Vis.Entities(pos, radius, nearPlayers, PLAYER_LAYER_MASK);

                for (int i = 0; i < nearPlayers.Count; i++)
                {
                    var player = nearPlayers[i];

                    if (player == null || !player.IsConnected || player.IsDead())
                        continue;

                    SendReply(player, message);

                    if (showTipStyle > -1)
                        ShowToast(player, message, showTipStyle);

                }

            }
            finally { Pool.FreeList(ref nearPlayers); }

        }

        private void BroadcastToast(string message, int type = 0)
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                ShowToast(BasePlayer.activePlayerList[i], message, type);
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

        public T RandomElement<T>(IEnumerable<T> source, System.Random rng)
        {
            T current = default;
            var count = 0;

            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                    current = element;
            }

            if (count == 0)
                throw new InvalidOperationException("Sequence was empty");
            
            return current;
        }

        private CinematicEntity AttachMutatedLightToCrate(LootContainer crate)
        {
            if (crate == null)
                throw new ArgumentNullException(nameof(crate));

            if (crate.IsDestroyed)
                return null;

            var greenLightPrefab = StringPool.Get(CINELIGHT_POINT_GREEN_PREFAB_ID);

            if (string.IsNullOrWhiteSpace(greenLightPrefab))
                return null;

            var adjPos = crate.transform.position;
            adjPos.y -= 0.033f;

            var light = GameManager.server.CreateEntity(greenLightPrefab, adjPos, crate.transform.rotation);

            light.Spawn();
            light.SetParent(crate, true, false);

            light.InvokeRepeating(() =>
            {
                if (light == null || light.IsDestroyed)
                    return;

                if (crate == null || crate.IsDestroyed)
                    light.Kill();
            }, 5f, 5f);

            return light as CinematicEntity;
        }

        private bool IsNight() { return TOD_Sky.Instance.Cycle.Hour > TOD_Sky.Instance.SunsetTime || TOD_Sky.Instance.Cycle.Hour < TOD_Sky.Instance.SunriseTime; }
        private bool IsNight(float offset) { return TOD_Sky.Instance.Cycle.Hour > (TOD_Sky.Instance.SunsetTime + offset) || TOD_Sky.Instance.Cycle.Hour < TOD_Sky.Instance.SunriseTime; }
        private bool IsAM() { return TOD_Sky.Instance.Cycle.Hour > 0 && TOD_Sky.Instance.Cycle.Hour < 12; }
        private bool IsAMAndPastSunrise() { return TOD_Sky.Instance.Cycle.Hour >= TOD_Sky.Instance.SunriseTime && TOD_Sky.Instance.Cycle.Hour < 12; }

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

        private void SimpleBroadcast(string msg, string userId = "")
        {
            if (string.IsNullOrEmpty(msg))
                return;

            var chatEntry = new ConVar.Chat.ChatEntry()
            {
                Channel = ConVar.Chat.ChatChannel.Server,
                Message = RemoveTags(msg),
                UserId = userId,
                Time = Facepunch.Math.Epoch.Current
            };

            RCon.Broadcast(RCon.LogType.Chat, chatEntry);

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || !player.IsConnected) continue;

                if (!string.IsNullOrWhiteSpace(userId)) player.SendConsoleCommand("chat.add", string.Empty, userId, msg);
                else SendReply(player, msg);
            }
        }

        #endregion

    }
}
