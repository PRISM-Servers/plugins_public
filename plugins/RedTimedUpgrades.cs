using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Project Red | Timed Building Upgrades", "Shady", "0.0.5")]
    class RedTimedUpgrades : RustPlugin
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
        private const string REPAIR_FAILED_FX = "assets/bundled/prefabs/fx/build/repair_failed.prefab";

        private const uint FOUNDATION_PREFAB_ID = 72949757;
        private const uint FOUNDATION_TRIANGLE_PREFAB_ID = 3234260181;
        private const uint WALL_LOW_PREFAB_ID = 2441851734;
        private const uint WALL_PREFAB_ID = 2194854973;
        private const uint WALL_DOORWAY_PREFAB_ID = 803699375;
        private const uint WALL_HALF_PREFAB_ID = 3531096400;
        private const uint WALL_WINDOW_PREFAB_ID = 2326657495;
        private const uint WALL_FRAME_PREFAB_ID = 919059809;

        [PluginReference]
        private readonly Plugin BuildingSkin;

        private readonly HashSet<NetworkableId> _noCallBlocks = new();

        private readonly Dictionary<string, Timer> _popupTimer = new();

        private void ShowPopup(BasePlayer player, string msg, float duration = 5f)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));

            player.SendConsoleCommand("gametip.showgametip", msg);
            if (_popupTimer.TryGetValue(player.UserIDString, out Timer endTimer)) endTimer.Destroy();

            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.SendConsoleCommand("gametip.hidegametip");
            });

            _popupTimer[player.UserIDString] = endTimer;
        }

        private void DoUpgrade(BuildingBlock block, BuildingGrade.Enum grade, ulong skin = 0, uint colorToApply = 0)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));

            PrintWarning(nameof(DoUpgrade) + " : " + block + " : " + grade + " : " + skin);


            block.ClientRPC(null, "DoUpgradeEffect", (int)grade, skin);

            block.playerCustomColourToApply = colorToApply;
            block.ChangeGradeAndSkin(grade, skin, true, true);


            var sb = Pool.Get<StringBuilder>();
            try
            {
                Effect.server.Run(sb.Clear().Append("assets/bundled/prefabs/fx/build/promote_").Append(grade.ToString().ToLower()).Append(".prefab").ToString(), block, 0U, Vector3.zero, Vector3.zero, null, false);
            }
            finally { Pool.Free(ref sb); }
           
        }
        
        private object CanChangeGrade(BasePlayer player, BuildingBlock block)
        {
            if (player == null || block?.net == null) return null;

            if (_noCallBlocks.Contains(block.net.ID))
            {
                SendReply(player, "<color=yellow>An upgrade is already in progress!</color>\n<color=green>Please wait for it to finish before attempting any other upgrades</color>.");
                SendLocalEffect(player, REPAIR_FAILED_FX);
                return false;
            }

            return null;
        }

        private object OnStructureUpgrade(BuildingBlock block, BasePlayer player, BuildingGrade.Enum grade, ulong skin)
        {

            if (skin != 0)PrintWarning(nameof(OnStructureUpgrade) + " with skin: " + skin);

            var netId = block.net.ID;

            if (!_noCallBlocks.Add(netId)) //.Contains(entity.net.ID))
            {
                PrintWarning("nocallblocks contained!!");
                return null;
            }

            block.PayForUpgrade(block.blockDefinition.GetGrade(grade, block.skinID), player);


            var prefabId = block.prefabID;

            var initialTimeValue = prefabId == FOUNDATION_PREFAB_ID ? 8.5 : prefabId == FOUNDATION_TRIANGLE_PREFAB_ID ? 6 : prefabId == WALL_PREFAB_ID ? 3.75 : prefabId == WALL_HALF_PREFAB_ID ? 1.75 : prefabId == WALL_WINDOW_PREFAB_ID ? 2 : prefabId == WALL_LOW_PREFAB_ID ? 0.75 : prefabId == WALL_DOORWAY_PREFAB_ID ? 1.33 : prefabId == WALL_FRAME_PREFAB_ID ? 0.425 : 3;

            var timeToTake = Mathf.Clamp((int)Math.Round(initialTimeValue * ((int)grade < 1 ? 1 : ((int)grade * 2.5)), MidpointRounding.AwayFromZero), 1, 1000);

            PrintWarning("timeToTake: " + timeToTake);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                ShowPopup(player, sb.Clear().Append("<color=yellow>Upgrade will finish in</color> <color=orange>").Append(timeToTake.ToString("N0")).Append("</color> <color=yellow>seconds</color><color=white>.</color>").ToString(), 3.25f);
            }
            finally { Pool.Free(ref sb); }

            //THIS SHOULD BE SCALED BASED ON THE SIZE OF THE OBJECT. FOUNDATIONS TAKING THE LONGEST; THEN WALLS, LOW WALLS, ETC.

            Action upgradeFxAction = null;

            upgradeFxAction = new Action(() =>
            {
                if (block == null || block.IsDestroyed) return;

                if (block.grade >= grade)
                {
                    PrintWarning("stop fx cause we hit the grade");
                    block.CancelInvoke(upgradeFxAction);
                    return;
                }

                if (UnityEngine.Random.Range(0, 101) <= 50) Effect.server.Run("assets/bundled/prefabs/fx/build/repair.prefab", block, 0U, Vector3.zero, Vector3.zero, null, false); //rng to reduce spammy noises
                if (UnityEngine.Random.Range(0, 101) <= 80) Effect.server.Run("assets/prefabs/building/door.hinged/effects/door-wood-impact.prefab", block, 0U, Vector3.zero, Vector3.zero, null, false);
            });

            var userId = player?.UserIDString;

            var desiredSkin = skin != 0 ? skin : BuildingSkin?.Call<ulong>("GetDesiredGradeSkin", userId, grade) ?? 0;
            var customColor = player?.LastBlockColourChangeId ?? 0;

            PrintWarning("desired Grade Skin: " + desiredSkin);

            block.InvokeRepeating(upgradeFxAction, 0.01f, UnityEngine.Random.Range(2.5f, 3.5f));

            block.Invoke(() =>
            {

                DoUpgrade(block, grade, desiredSkin);
                _noCallBlocks.Remove(netId);

            }, timeToTake);

         


            return true;

        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;

            using var fx = new Effect(effect, player, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }
    }
}
