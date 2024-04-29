using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("AchievementsExtended", "Shady", "0.0.4")]
    public class AchievementsExtended : RustPlugin
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
        [PluginReference]
        private readonly Plugin PRISMAchievements;

        //private enum AchievementType { Misc, PvP, PvE, Looting, Gathering, Skill, All };

        private const string RAD_SHORTNAME = "max_rads";
        private const string RAD_DISPLAYNAME = "Reactor #4";
        private const string RAD_DESC = "Accumulate enough radiation to be an extension of the Chernobyl Exclusion Zone";
        private const string RAD_FX = "assets/bundled/prefabs/fx/gestures/drink_vomit.prefab";
        private const int RAD_POINTS = 1986;

        private const string SURVIVE_SHORTNAME = "survive_1hp";
        private const string SURVIVE_DISPLAYNAME = "'Tis but a scratch";
        private const string SURVIVE_DESC = "Survive with only 1 HP";
        private const int SURVIVE_POINTS = 400;

        private const string UBER_CRASH_SHORTNAME = "mini_crash_passenger";
        private const string UBER_CRASH_DISPLAYNAME = "Bad Uber Flyer";
        private const string UBER_CRASH_DESC = "Have a horrific flight accident while transporting another person";
        private const int UBER_CRASH_POINTS = 250;


        private readonly Dictionary<ulong, Action> _healthAction = new Dictionary<ulong, Action>();
        
        private void OnServerInitialized()
        {
            if (!(PRISMAchievements?.IsLoaded ?? false))
            {
                PrintWarning("PRISMAchievements is not loaded, AchievementsExtended will not work correctly");
                return;
            }

            PRISMAchievements.Call("RegisterAchievement", RAD_SHORTNAME, RAD_DISPLAYNAME, RAD_DESC, 0, RAD_POINTS, RAD_FX, 15);
            PRISMAchievements.Call("RegisterAchievement", SURVIVE_SHORTNAME, SURVIVE_DISPLAYNAME, SURVIVE_DESC, 0, SURVIVE_POINTS);
            PRISMAchievements.Call("RegisterAchievement", UBER_CRASH_SHORTNAME, UBER_CRASH_DISPLAYNAME, UBER_CRASH_DESC, 0, UBER_CRASH_POINTS);
        }

        private void OnRunPlayerMetabolism(PlayerMetabolism metabolism, BaseCombatEntity ownerEntity)
        {
            if (metabolism == null || ownerEntity == null) return;

            var player = ownerEntity as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead()) return;


            if (metabolism.radiation_poison.value > 498) //max rads
                GiveAchievementByName(player, RAD_SHORTNAME);

            if (player.Health() <= 1f)
            {
                PrintWarning("health <= 1");
                var userId = player.userID;

                Action healthAction = null;

                if (!_healthAction.TryGetValue(userId, out healthAction))
                {
                    _healthAction[userId] = healthAction = new Action(() =>
                    {
                        if (player == null || player.IsDead() || !player.IsConnected || player.IsWounded())
                        {
                            PrintWarning("player has died or IsWounded (so not a real survive!), cancel action!");

                            player.CancelInvoke(healthAction);

                            _healthAction.Remove(userId);
                            return;
                        }

                        if (player.Health() >= 2f)
                        {
                            PrintWarning("health now >= 2 after it was 1! they survived. granting achievement");

                            GiveAchievementByName(player, SURVIVE_SHORTNAME);
                            player.CancelInvoke(healthAction);

                            _healthAction.Remove(userId);
                        }

                    });

                    player.InvokeRepeating(healthAction, 1f, 1f);
                }

            }

        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            var mini = entity as Minicopter;
            if (mini == null) return;

            var pilot = mini?.mountPoints[0]?.mountable?._mounted;
            if (pilot == null || !pilot.IsConnected) return;

            var passenger = mini?.mountPoints[1]?.mountable?._mounted;
            if (passenger == null) return;

            GiveAchievementByName(pilot, UBER_CRASH_SHORTNAME);

            if (passenger.IsConnected)
                GiveAchievementByName(passenger, UBER_CRASH_SHORTNAME);

        }

        private void GiveAchievementByName(BasePlayer player, string achievementName) //easier than calling plugin every time
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(achievementName))
                throw new ArgumentNullException(nameof(player));

            PRISMAchievements?.Call("GiveAchievementByName", player, achievementName);
        }

    }
}
