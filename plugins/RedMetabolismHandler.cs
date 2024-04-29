

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Project Red | Metabolism Handler", "Shady", "0.0.4")]
    class RedMetabolismHandler : RustPlugin
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
        private const string MISSED_HIT_FX = "assets/prefabs/misc/orebonus/effects/bonus_fail.prefab";

        private readonly Dictionary<string, Timer> _popupTimer = new Dictionary<string, Timer>();

        private readonly Dictionary<string, HitInfo> _lastInaccurateHit = new Dictionary<string, HitInfo>();

        private const int M249_ITEM_ID = -2069578888;
        private const int LR300_ITEM_ID = -1812555177;
        private const int M39_ITEM_ID = 28201841;
        private const int AK_ITEM_ID = 1545779598;
        private const int L96_ITEM_ID = -778367295;
        private const int SEMI_AUTO_RIFLE_ID = -904863145;

        private void ShowPopup(BasePlayer player, string msg, float duration = 5f)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));
         
            player.SendConsoleCommand("gametip.showgametip", msg);
            Timer endTimer;
            if (_popupTimer.TryGetValue(player.UserIDString, out endTimer)) endTimer.Destroy();

            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.SendConsoleCommand("gametip.hidegametip");
            });

            _popupTimer[player.UserIDString] = endTimer;
        }

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (projectile == null || player == null) return;

            var weaponItem = projectile?.GetItem();
            if (weaponItem == null)
            {
                PrintWarning("weaponItem null!!");
                return;
            }

            var weaponId = weaponItem?.info?.itemid ?? 0;

            var modifier = weaponId == M249_ITEM_ID ? 4f : weaponId == L96_ITEM_ID ? 5f : weaponId == LR300_ITEM_ID ? 2.2f : weaponId == AK_ITEM_ID ? 2.8f : weaponId == M39_ITEM_ID ? 2f : weaponId == SEMI_AUTO_RIFLE_ID ? 1.8f : 1f;
         //   var hydMod = 1f;

            var calTake = 0.04f * modifier;
            var hydTake = 0.08f * modifier;

            var metabolism = player.metabolism;

            metabolism.calories.Subtract(calTake);
            metabolism.hydration.Subtract(hydTake);

            PrintWarning("modifier: " + modifier + ", take: " + calTake + ", hydTake: " + hydTake);

            if (weaponId == M249_ITEM_ID || weaponId == L96_ITEM_ID)
            {
                metabolism.temperature.Add(weaponId == M249_ITEM_ID ? 1.25f : 12f);
                if (weaponId == M249_ITEM_ID && UnityEngine.Random.Range(0, 101) <= 3 || weaponId == L96_ITEM_ID) metabolism.oxygen.Subtract(20f); //this immediately goes away if you're above water but creates a nice effect
            }

        }

        private void OnRunPlayerMetabolism(PlayerMetabolism metabolism, BaseCombatEntity ownerEntity)
        {
            if (metabolism == null) return;

            var player = ownerEntity as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead()) return;




            if (metabolism.calories.value > 0.0 || metabolism.hydration.value > 0.0)
            {
                var reduc = UnityEngine.Random.Range(0.00975f, 0.0375f);

                var waterReduc = reduc * UnityEngine.Random.Range(1.45f, 2.75f);


                var swimming = player?.IsSwimming() ?? false;

                if (player.IsRunning() && !swimming)
                {
                    reduc *= UnityEngine.Random.Range(4.25f, 7.25f);
                    waterReduc *= UnityEngine.Random.Range(3.75f, 7.95f);
                }

                if (!player.IsOnGround() && !swimming)
                {
                    reduc *= UnityEngine.Random.Range(3.7f, 4.5f);
                    waterReduc *= UnityEngine.Random.Range(8f, 12.25f);
                }

                if (swimming)
                {
                    reduc *= UnityEngine.Random.Range(10f, 17.2f);
                    waterReduc *= UnityEngine.Random.Range(18f, 24f);
                }

                var idleTime = player?.IdleTime ?? 0f;
                if (idleTime > 420)
                {
                    var idleMult = Mathf.Clamp(idleTime / UnityEngine.Random.Range(90, 170f), 2.2f, 10f);
                    reduc *= idleMult;
                    waterReduc *= idleMult;
                }

                var tempMult = Mathf.Clamp(player.metabolism.temperature.value / 25, 1, 10); ;
                reduc *= tempMult;
                waterReduc *= tempMult;


                if (metabolism.hydration.value > 0.0) metabolism.hydration.Subtract(waterReduc);
                if (metabolism.calories.value > 0.0) metabolism.calories.Subtract(reduc);
            }
        }

        private object OnMeleeAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return null;

            var metabolism = player?.metabolism;


            if (info?.Weapon?.prefabID != 3940068399 || UnityEngine.Random.Range(0, 101) <= 25) //reduced chance of calorie hit if using a rock (noob friendly)
            {
                metabolism.calories.Subtract(Mathf.Clamp(UnityEngine.Random.Range(0.125f, 0.325f) * (info?.Weapon as BaseMelee)?.repeatDelay ?? 1f, 0f, 500f));
                metabolism.hydration.Subtract(Mathf.Clamp(UnityEngine.Random.Range(0.175f, 0.75f) * (info?.Weapon as BaseMelee)?.repeatDelay ?? 1f, 0f, 500f));
            }



            var calFrac = metabolism.calories.Fraction();

            var waterFrac = metabolism.hydration.Fraction();

            var repeatDelay = info?.Weapon?.repeatDelay ?? 1f;

            if (calFrac <= 0.16 && UnityEngine.Random.Range(0, 101) <= (16 / (1 + calFrac)) * repeatDelay || waterFrac <= 0.2 && UnityEngine.Random.Range(0, 101) <= (25 / (1 + waterFrac)) * repeatDelay)
            {
                for (int i = 0; i < 5; i++) SendLocalEffect(player, MISSED_HIT_FX);

                ShowPopup(player, "You're beginning to feel weak and miss on your attack.", 3.25f);

                return true;
            }

            if (calFrac <= 0.25 && UnityEngine.Random.Range(0, 101) <= (5 / (1 + calFrac)) * repeatDelay || waterFrac <= 0.33 && UnityEngine.Random.Range(0, 101) <= (10 / (1 + waterFrac)) * repeatDelay)
            {
                for (int i = 0; i < 5; i++) SendLocalEffect(player, MISSED_HIT_FX);

                ShowPopup(player, "Because you begin to feel weak, you inaccurately swing your weapon and deal less damage!", 3.25f);
                info.damageTypes.ScaleAll(0.325f);
                _lastInaccurateHit[player.UserIDString] = info;
            }


            return null;
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            if (dispenser == null || player == null || item == null) return;

            //   PrintWarning("OnDispenserGather");

            HitInfo info;
            if (!_lastInaccurateHit.TryGetValue(player.UserIDString, out info))
            {
                //  PrintWarning("no last inaccurate hit");
                return;
            }



            var dispEnt = dispenser?._baseEntity ?? dispenser?.GetComponent<BaseEntity>();
            if (dispEnt == null) return;

            if (info?.HitEntity?.net?.ID != dispEnt?.net?.ID)
            {
                _lastInaccurateHit.Remove(player.UserIDString);
               // PrintWarning("last inaccurate hit does not match current ent!!!!: " + info?.HitEntity + " != " + dispEnt);
                return;
            }

            PrintWarning("inaccurate hit dispenser!!!");

            PrintWarning("pre: " + item.amount);
            item.amount = Mathf.Clamp((int)Math.Round(item.amount * 0.325, MidpointRounding.AwayFromZero), 1, item.amount);
            PrintWarning("post: " + item.amount);

            _lastInaccurateHit.Remove(player.UserIDString);

        }

        private void OnHealingItemUse(MedicalTool tool, BasePlayer player)
        {
            if (tool == null || player == null) return;

            var item = tool?.GetItem();
            if (item == null) return;

            PrintWarning("Itemname?: " + item?.info?.displayName?.english);


            // var dehydrateAmount = 0f;

            var dehydrateAmount = item?.info?.itemid == 1079279582 ? 20f : 0f; //syringes only. need to handle medkits elsewhere (onitemuse?): 254522515 <-- medkit id

            if (dehydrateAmount > 0)
            {
                Action dehydrateAct = null;

                var repeated = 0;

                dehydrateAct = new Action(() =>
                {
                    if (player == null || player.IsDestroyed) return;// || player.IsDead() || !player.IsConnected) return;

                    if (repeated >= 30 || player.IsDead() || !player.IsConnected)
                    {
                        PrintWarning("CANCEL");
                        player.CancelInvoke(dehydrateAct);
                        return;
                    }

                    player.metabolism.hydration.MoveTowards(player.metabolism.hydration.value - dehydrateAmount, 1f);


                    //maybe we should make them hungry too?
                    player.metabolism.calories.MoveTowards(player.metabolism.calories.value - (dehydrateAmount / 2), 0.5f);


                    repeated++;

                });

                player.InvokeRepeating(dehydrateAct, 0.01f, 0.025f);

               
            }

        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            if (player?.metabolism == null || !player.IsConnected) return;

            PrintWarning("OnPlayerRespawned!");


            player.metabolism.calories.value = player.metabolism.calories.max * 0.66f;
            player.metabolism.hydration.value = player.metabolism.hydration.max * 0.85f;
            player.metabolism.SendChangesToClient();



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
