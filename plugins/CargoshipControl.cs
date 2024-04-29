using System;
using UnityEngine;
using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using System.Text;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("CargoshipControl", "MBR/Shady", "1.0.94")]
    internal class CargoshipControl : RustPlugin
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

        private readonly HashSet<CargoShip> CargoShipHashSet = new HashSet<CargoShip>();
        private DateTime lastship;
        private DateTime nextship;

        private readonly float SpawnHours = 3f;
        private readonly bool WarnOnRadiation = true;
        private readonly bool AutomaticallyUnlockCrates = true;
        private readonly bool AnnounceSpawn = true;

        private Action SpawnAction = null;

        private const string CargoshipPrefabPath = "assets/content/vehicles/boats/cargoship/cargoshiptest.prefab";

        #region Commands

     
        private void NextShipCMD(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "nextship"))
            {
                player.Reply("You do not have permission to use this command!");
                return;
            }

            var now = DateTime.Now;
            var lastShipTxt = lastship > DateTime.MinValue ? (ReadableTimeSpan(now.Subtract(lastship)) + " ago") : (!AnyCargoActive() ? "There hasn't been one yet" : "Right now");
            var nextShipTxt = (nextship > now) ? ReadableTimeSpan(nextship.Subtract(now)) : "Right now";
            player.Reply("Last Cargoship: " + lastShipTxt + " \nNext Cargoship: " + nextShipTxt);
        }

        private bool AnyCargoActive()
        {
            if (CargoShipHashSet.Count > 0) return true;
            foreach(var entity in BaseNetworkable.serverEntities)
            {
                if (entity is CargoShip) return true;
            }
            return false;
        }

        [ConsoleCommand("killship")]
        private void KillShipCommand(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin && arg.Connection != null) return;
            KillShips();
        }

        [ConsoleCommand("spawnship")]
        private void SpawnShipCommand(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin && arg.Connection != null) return;
            SpawnCargoShip();
            lastship = DateTime.Now;
            Puts("Spawned Cargoship");
        }

        #endregion

        private void SendReply(BasePlayer player, string msg, string userId = CHAT_STEAM_ID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
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

        private string ReadableTimeSpan(TimeSpan span, string stringFormat = "N0") //I'm sure some of you uMod code snobs absolutely LOVE this one
        {
            if (span == TimeSpan.MinValue) return string.Empty;
            var str = string.Empty;
            var repStr = stringFormat.StartsWith("0.0", StringComparison.CurrentCultureIgnoreCase) ? ("." + stringFormat.Replace("0.", string.Empty)) : "WORKAROUNDGARBAGETEXTTHATCANNEVERBEFOUNDINASTRINGTHISISTOPREVENTREPLACINGANEMPTYSTRINGFOROLDVALUEANDCAUSINGANEXCEPTION"; //this removes unnecessary values, for example for ToString("0.00"), 80.00 will show as 80 instead

            if (span.TotalHours >= 24)
            {

                var totalHoursWereGoingToShowToTheUserAsAString = (span.TotalHours - ((int)span.TotalDays * 24)).ToString(stringFormat).Replace(repStr, string.Empty);
                var totalHoursToShowAsNumber = (int)Math.Round(double.Parse(totalHoursWereGoingToShowToTheUserAsAString), MidpointRounding.AwayFromZero);
                var showHours = totalHoursToShowAsNumber < 24 && totalHoursToShowAsNumber > 0;

                str = (int)span.TotalDays + (!showHours && span.TotalHours > 24 ? 1 : 0) + " day" + (span.TotalDays >= 1.5 ? "s" : "") + (showHours ? (" " + totalHoursWereGoingToShowToTheUserAsAString + " hour(s)") : "");
            }
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        private bool HasPerms(string userId, string perm)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(perm)) throw new ArgumentNullException(nameof(perm));

            if (userId.Equals("server_console", StringComparison.OrdinalIgnoreCase) || permission.UserHasPermission(userId, "cargoshipcontrol.admin")) return true;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try { return permission.UserHasPermission(userId, !perm.StartsWith(nameof(CargoshipControl), StringComparison.OrdinalIgnoreCase) ? sb.Clear().Append(nameof(CargoshipControl).ToLower()).Append(".").Append(perm).ToString() : perm); }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private void KillShips()
        {
            var kc = 0;
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var ship = entity as CargoShip;
                if (ship != null && !ship.IsDestroyed)
                {
                    kc++;
                    ship.Kill();
                }
            }
           
            Puts("Killed " + kc.ToString("N0") + " ships");
            CargoShipHashSet.Clear();
        }

        private void Init()
        {
            permission.RegisterPermission("cargoshipcontrol.admin", this);
            permission.RegisterPermission("cargoshipcontrol.nextship", this);
            AddCovalenceCommand(new string[] { "nextship", "nextcargo", "cargonext", "shipnext" }, nameof(NextShipCMD));
        }

        private void OnServerInitialized()
        {

            var events = GameObject.FindObjectsOfType<TriggeredEventPrefab>();
            if (events != null && events.Length > 0)
            {
                TriggeredEventPrefab cargoEvent = null;
                for (int i = 0; i < events.Length; i++)
                {
                    var evnt = events[i];
                    if ((evnt?.targetPrefab?.resourcePath ?? string.Empty).Contains("cargoship"))
                    {
                        cargoEvent = evnt;
                        break;
                    }
                }
                if (cargoEvent != null)
                {
                    GameObject.Destroy(cargoEvent);
                    Puts("Disabled default Cargoship spawning.");
                }
                else Puts("Default cargoship spawning was already disabled");
            }

            //calculate the server-approriate time to spawn this cargoship judging by when the server restarted
            var now = DateTime.Now;
            var startTime = Facepunch.Math.Epoch.ToDateTime((long)(Facepunch.Math.Epoch.FromDateTime(now) - Time.realtimeSinceStartup));
            var useTime = startTime;
            var cur = 0;
            var max = 100;
            while(useTime < now)
            {
                useTime = useTime.AddHours(SpawnHours);

                cur++;
                if (cur >= max)
                {
                    PrintWarning("SAFETY LOOP HIT MAX!");
                    break;
                } //this should never happen, but in case, we don't want a stuck loop
            }

            PrintWarning("Now time is: " + now + ", the apparent server start time was: " + startTime + "(" + ReadableTimeSpan(now - startTime) + ") Got useTime of: " + useTime + ", which is in: " + ReadableTimeSpan(useTime - now));

            nextship = useTime;
           
            SpawnAction = new Action(() =>
            {
                var tNow = DateTime.Now;
                nextship = tNow.AddHours(SpawnHours);
                PrintWarning("Spawning ship, next ship after this will be: " + nextship + ", (tNow.AddHours SpawnHours), " + tNow + " + hours: " + SpawnHours);
                lastship = tNow;
                SpawnCargoShip();
            });
            var t = (float)(nextship - now).TotalSeconds;
            PrintWarning("totalSeconds (when we spawn the cargo) is: " + t + ", now: " + now + ", nextship: " + nextship);
            ServerMgr.Instance.InvokeRepeating(SpawnAction, t, SpawnHours * 3600);
        }

        private void Unload()
        {
            if (ServerMgr.Instance != null && SpawnAction != null)
            {
                ServerMgr.Instance.CancelInvoke(SpawnAction);
                Puts("Canceled SpawnAction");
            }
        }

        private void OnCrateHackEnd(HackableLockedCrate crate)
        {
            if (crate == null) return;

            if (crate.GetParentEntity()?.ShortPrefabName == CargoshipPrefabPath && crate.OwnerID == 13376969)
            {
                Server.Broadcast("<color=orange>The cargoship will be leaving in 5 minutes. Its crates are now unlocked!</color>");
                Unsubscribe(nameof(OnCrateHackEnd));
                timer.In(5f, () => Subscribe(nameof(OnCrateHackEnd)));
            }
        }

        private CargoShip SpawnCargoShip(Vector3 location = new Vector3(), bool silent = false)
        {
            Puts("Spawning a Cargoship");

            float x = TerrainMeta.Size.x;
            float num = location.y + 50f;
            var mapScaleDistance = 0.8f;
            var vector3_1 = Vector3Ex.Range(-1f, 1f);
            vector3_1.y = 0.0f;
            vector3_1.Normalize();
            var vector3_2 = vector3_1 * (x * mapScaleDistance);

            vector3_2.y = num;
            var ship = GameManager.server.CreateEntity(CargoshipPrefabPath, vector3_2, Quaternion.identity, true);
            if (ship == null)
            {
                PrintError("Ship was null when spawning!!");
                return null;
            }
           
            ship.Spawn();
            var cargoShip = ship as CargoShip;
            if (cargoShip == null)
            {
                PrintError("cargoShip was null when spawning!!");
                if (ship != null && !ship.IsDestroyed) ship.Kill();
                return null;
            }

            if (!CargoShipHashSet.Add(cargoShip)) PrintWarning("Failed to add cargoShip to CargoShipHashSet!!");

            if (AnnounceSpawn && !silent) Server.Broadcast("<color=#ff362b>A <color=#5eb9ff>cargo ship</color> is entering the map!</color>");

            if (AutomaticallyUnlockCrates)
            {
                //if it lasts 40 mins and it takes 15 mins to unlock, make sure it's unlocked at least 5 mins before it leaves (40-15-5 = 20 mins)
                var TimeUntilCrateUnlock = (CargoShip.event_duration_minutes * 60f) - HackableLockedCrate.requiredHackSeconds - (60f * 5f);
                Puts("Ship will unlock crates in " + (TimeUntilCrateUnlock / 60f) + " minutes");

                ship.Invoke(() =>
                {
                    Puts("Starting unlock process of ship's crates (time)");

                    int unlocked = 0;
                    foreach(var entity in BaseNetworkable.serverEntities)
                    {
                        var crate = entity as HackableLockedCrate;
                        if (crate != null && !crate.IsBeingHacked() && crate.GetParentEntity() == ship)
                        {
                            crate.OwnerID = 13376969;
                            crate.StartHacking();
                            unlocked++;
                        }
                    }

                    if (unlocked > 0) Server.Broadcast("<color=#ff9c40>The <color=#1fd13a>hacking</color> process of " + unlocked.ToString("N0") + " crate" + (unlocked == 1 ? string.Empty : "s") + " on the <color=#5eb9ff>cargo ship</color> has started, " + (unlocked == 1 ? "it" : "they") + " will be unlocked <i><color=#ff362b>5</color> minutes before the <color=#5eb9ff>cargo ship</color> leaves (be careful)!</color>");
                }, TimeUntilCrateUnlock);
            }


            //--Start anti-stuck code--//
           // var lastPos = Vector3.zero;
         //   var stuckTimes = 0;
            /*/
            ship.InvokeRepeating(() =>
            {
                if (ship == null || ship.IsDestroyed || ship.gameObject == null)
                {
                    PrintWarning("ship is null/destroyed on anti-stuck invoke!");
                    return;
                }
                var currentPos = ship?.transform?.position ?? Vector3.zero;
                if (lastPos != Vector3.zero)
                {
                    if (Vector3.Distance(lastPos, currentPos) < 1.5f)
                    {
                        PrintWarning("Cargo ship has not moved at least 1.5m in 7 seconds, may be stuck (x" + stuckTimes + ")!");
                        stuckTimes++;
                    }
                    else stuckTimes = 0;

                    if (stuckTimes >= 7)
                    {
                        PrintWarning("Cargo ship stuck times was >= 7 (" + stuckTimes + "), destroying & respawning!");
                        ship.Kill();
                        SpawnCargoShip();
                        //var newShip = SpawnCargoShip()
                        //comment here
                        if (newShip != null && !newShip.IsDestroyed)
                        {
                            newShip.Invoke(() =>
                            {
                                newShip.transform.position = lastPos;
                                newShip.SendNetworkUpdateImmediate();
                                PrintWarning("set newShip to lastPos!");
                            }, 3f);
                        }//end it here
                    }
                }
                lastPos = currentPos;
            }, 60f, 7f);/*/
            //--End anti-stuck code--//


            if (WarnOnRadiation)
            {
                bool flag = false;
                Action checkAct = null;
                checkAct = new Action(() =>
                {
                    if (flag)
                    {
                        ship.CancelInvoke(checkAct);
                        PrintWarning("Canceled repeating invoke checkAct for radiation warning on ship because flag was true");
                        return;
                    }

                    if (ship.HasFlag(BaseEntity.Flags.Reserved8))
                    {
                        flag = true;
                        var players = Facepunch.Pool.GetList<BasePlayer>();

                        try 
                        {
                            Vis.Entities(ship.transform.position, 125f, players, 131072);
                            if (players.Count > 0)
                            {
                                var warnMsg = "<color=#6eff6b>Radioactive waste</color> <color=#ff9c40>on the <color=#5eb9ff>ship</color> is <color=#ff362b>very unstable!</color> Leave immediately!</color>";
                                for (int i = 0; i < players.Count; i++)
                                {
                                    var p = players[i];
                                    if (p != null && p.IsConnected) SendReply(p, warnMsg);
                                }
                            }
                        }
                        finally { Facepunch.Pool.FreeList(ref players); }
                    }

                });
                ship.InvokeRepeating(checkAct, (CargoShip.event_duration_minutes * 60f - 10f), 1f);
            }

            lastship = DateTime.Now;

            return cargoShip;
        }

        private DateTime NextShipTime()
        {
            return nextship;
        }

    }
}