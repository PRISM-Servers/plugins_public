using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using UnityEngine;
using CompanionServer;

namespace Oxide.Plugins
{
    [Info("Discord Plant Reminder", "Shady", "0.0.3")]
    class DiscordPlantReminder : RustPlugin
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
        //needs to check if has already sent reminder today lol

        //check last sent time for plugin reload compatilbiity

        [PluginReference]
        private readonly Plugin DiscordAPI2;

        private readonly Dictionary<string, HashSet<GrowableEntity>> _playerPlants = new Dictionary<string, HashSet<GrowableEntity>>();

        private Coroutine _reminderCoroutine = null;

        private const string OPT_OUT_PERMISSION_NAME = "discordplantreminder.optout";

        System.Collections.IEnumerator SendPlantReminders()
        {

            PrintWarning(nameof(SendPlantReminders));

            PrintWarning("waiting 10 secs before starting while");

            yield return CoroutineEx.waitForSecondsRealtime(10f);

            PrintWarning("waited");

            while (true)
            {
                PrintWarning("start while");

                foreach (var kvp in _playerPlants)
                {
                    var playerId = kvp.Key;

                    if (permission.UserHasPermission(playerId, OPT_OUT_PERMISSION_NAME))
                    {
                        PrintWarning("playerId: " + playerId + " has opt out perm: " + OPT_OUT_PERMISSION_NAME);
                        continue;
                    }

                    var plants = kvp.Value;

                    ulong userId;
                    if (!ulong.TryParse(playerId, out userId)) continue;

                    var p = BasePlayer.FindByID(userId);
                    if (p != null && p.IsConnected && !p.IsAdmin) continue; //don't send discord messages to online players

                    var ripePlants = 0;

                    foreach (var plant in plants)
                    {
                        if (plant.State == PlantProperties.State.Ripe)
                            ripePlants++;

                    }

                    if (ripePlants > 0)
                    {
                        PrintWarning("ripe plants > 0!! ripe plants: " + ripePlants);

                        var userName = GetDisplayNameFromID(playerId);

                        var msg = ":potted_plant:" + (!string.IsNullOrWhiteSpace(userName) ? (" Hi " + userName + ",") : string.Empty) + " You have " + ripePlants.ToString("N0") + " ripe plants ready & waiting for you! Don't let them get lonely. :farmer:\nTo opt out, type ``/plantmsg`` in-game";

                        var rustPlusMsg = (!string.IsNullOrWhiteSpace(userName) ? (" Hi " + userName + ",") : string.Empty) + " You have " + ripePlants.ToString("N0") + " ripe plants ready & waiting for you! Don't let them get lonely.";

                        SendRustPlusNotification(userId, rustPlusMsg);

                        DiscordAPI2?.Call("DMUser", playerId, msg);

                        PrintWarning("Sent message to: " + userName + " (" + playerId + "), message: " + Environment.NewLine + msg);
                    }
                    else PrintWarning("no ripe plants: " + playerId);

                }

                var waitTime = 14400;

                PrintWarning("Finished reminder, waiting " + waitTime + " before looping this while");

                yield return CoroutineEx.waitForSecondsRealtime(waitTime);
            }

        }

        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));

            permission.RegisterPermission(OPT_OUT_PERMISSION_NAME, this);

            string[] cmds = { "plantreminder", "discordplants", "plantmsg", "plantnotifications", "ripenotifications", "ripemsg", "plantmsgs", "plantdiscord", "discordplant" };
            AddCovalenceCommand(cmds, nameof(cmdNotifications));
        }

        private void Unload()
        {
            if (_reminderCoroutine != null)
            {
                PrintWarning("stopping coroutine on unload");

                ServerMgr.Instance.StopCoroutine(_reminderCoroutine);
                PrintWarning("stopped coroutine!");

            }
            else PrintWarning("reminderCoroutine null on unload");
        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));

            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var plant = entity as GrowableEntity;
                if (plant == null || plant.OwnerID == 0) continue;

                var userId = plant.OwnerID.ToString();

                HashSet<GrowableEntity> growables;

                if (!_playerPlants.TryGetValue(userId, out growables)) _playerPlants[userId] = growables = new HashSet<GrowableEntity>();

                growables.Add(plant);

                _playerPlants[userId] = growables; //unnecessary?

            }

            _reminderCoroutine = ServerMgr.Instance.StartCoroutine(SendPlantReminders());

        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var plant = entity as GrowableEntity;
            if (plant == null || plant.OwnerID == 0) return;

            var userId = plant.OwnerID.ToString();

            HashSet<GrowableEntity> growables;

            if (!_playerPlants.TryGetValue(userId, out growables)) _playerPlants[userId] = growables = new HashSet<GrowableEntity>();

            growables.Add(plant);

            _playerPlants[userId] = growables; //unnecessary?
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            var plant = entity as GrowableEntity;
            if (plant == null || plant.OwnerID == 0) return;

            var userId = plant.OwnerID.ToString();

            HashSet<GrowableEntity> growables;
            if (_playerPlants.TryGetValue(userId, out growables) && growables.Remove(plant))
            {
                //growables.Remove(plant);
                _playerPlants[userId] = growables; //unnecessary?
            }

        }

        private void SendRustPlusNotification(ulong userId, string titleMsg, string bodyMsg = "PRISM", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            var serverPairingData = Util.GetServerPairingData();

            NotificationList.SendNotificationTo(userId, channel, titleMsg, bodyMsg, serverPairingData);
        }

        private void cmdNotifications(IPlayer iPlayer, string command, string[] args)
        {
            if (iPlayer == null || iPlayer.IsServer) return;

            if (args.Length > 0 && args[0].Equals("test", StringComparison.OrdinalIgnoreCase))
            {
                SendRustPlusNotification(ulong.Parse(iPlayer.Id), args.Length > 1 ? args[1] : "test");
                return;
            }

            if (permission.UserHasPermission(iPlayer.Id, OPT_OUT_PERMISSION_NAME)) permission.RevokeUserPermission(iPlayer.Id, OPT_OUT_PERMISSION_NAME);
            else permission.GrantUserPermission(iPlayer.Id, OPT_OUT_PERMISSION_NAME, this);

            var nowEnabled = !permission.UserHasPermission(iPlayer.Id, OPT_OUT_PERMISSION_NAME);

            var enabledStr = nowEnabled ? "now enabled" : "no longer enabled";
            iPlayer.Message("Ripe plant notifications are " + enabledStr + ".");

            PrintWarning("nowEnabled: " + nowEnabled + " (! has permission: " + OPT_OUT_PERMISSION_NAME);
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

    }
}
