using ConVar;
using Oxide.Core;
using Oxide.Core.Plugins;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("PRISM H'ween", "Shady", "0.0.1")]
    internal class PRISMWeen : RustPlugin
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
        private readonly HashSet<ulong> _hasHween = new();

        public static PRISMWeen Instance { get; set; } = null;

        [PluginReference]
        private readonly Plugin FindGround;

        private void OnServerInitialized()
        {

            Instance = this;

            if (Halloween.enabled && BasePlayer.activePlayerList != null)
            {
                PrintWarning("Halloween is enabled! Adding any missing HalloweenPlayer components.");

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || p.gameObject == null || !p.IsConnected || p.IsDestroyed) continue;
                    var hween = p?.GetComponent<HalloweenPlayer>() ?? null;
                    if (hween == null) p.gameObject.AddComponent<HalloweenPlayer>();
                }

            }

        }

        private void Unload()
        {
            try 
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || player?.gameObject == null) continue;


                    var hween = player?.GetComponent<HalloweenPlayer>() ?? null;
                    hween?.DoDestroy();
                }
                
            }
            finally { Instance = null; }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (Halloween.enabled && !_hasHween.Contains(player.userID)) 
                player.gameObject.AddComponent<HalloweenPlayer>();
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            var pHalloween = player?.GetComponent<HalloweenPlayer>() ?? null;
            pHalloween?.DoDestroy();

            _hasHween.Remove(player.userID);

        }

        private IEnumerator CoffinZombieCoroutine(BaseEntity coffin)
        {
            if (coffin == null || coffin.IsDestroyed)
                yield break;

            var spawnPos = coffin?.transform?.position ?? Vector3.zero;

            yield return CoroutineEx.waitForSecondsRealtime(0.175f);

            Effect.server.Run("assets/prefabs/building/wall.window.shutter/effects/shutters-wood-open-start.prefab", coffin.transform.position);

            yield return CoroutineEx.waitForSecondsRealtime(0.365f);

            Effect.server.Run("assets/prefabs/building/wall.window.shutter/effects/shutters-wood-open-end.prefab");

            yield return CoroutineEx.waitForSecondsRealtime(0.12f);

   

            coffin.Kill(BaseNetworkable.DestroyMode.Gib);

            yield return CoroutineEx.waitForSecondsRealtime(0.05f);

            var scarecrow = GameManager.server.CreateEntity("assets/prefabs/npc/scarecrow/scarecrow.prefab", spawnPos);

            scarecrow.enableSaving = false;
            scarecrow.Spawn();
            scarecrow.EnableSaving(false);

            //temp below? unnecessary? thre as a test.
            scarecrow.SendNetworkUpdateImmediate();

        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (!Halloween.enabled)
                return;

            var p = entity as BasePlayer;
            if (p == null)
                return;


            var resEnt = dispenser?.GetComponent<ResourceEntity>() ?? null;

            var tree = resEnt as TreeEntity;

            if (tree == null)
                return;

            if (UnityEngine.Random.Range(0, 101) > 10) //rng not met; don't do coffin stuff.
                return;

            tree?.Invoke(() =>
            {
                if (tree == null)
                    return;

                if (tree.Health() > 0f) //tree not dead
                    return;


                var highPos = new Vector3(tree.transform.position.x, tree.transform.position.y + 10f, tree.transform.position.z);
                var groundPos = GetGroundPosition(highPos);

                groundPos.y += 1f;

                PrintWarning("high pos: " + highPos + ", groundPos: " + groundPos);

                var coffin = GameManager.server.CreateEntity("assets/prefabs/misc/halloween/coffin/coffinstorage.prefab", groundPos, Quaternion.Euler(0f, 95f, 273));

                coffin.enableSaving = false;
                coffin.Spawn();


                coffin.EnableSaving(false);

                ServerMgr.Instance.StartCoroutine(CoffinZombieCoroutine(coffin));

            }, 0.125f);





        }

        public class HalloweenPlayer : FacepunchBehaviour
        {
            public BasePlayer player;
            private float _checkRate = UnityEngine.Random.Range(1.9f, 3.1f); //randomized CheckRate to minimize amount of players who are being checked at exact same time (spread out for performance)
            public HashSet<CollectibleEntity> SeenCollectibles = new();
            //  private List<string> Effects = new List<string> { "" };
            private readonly List<string> SoundEffects = new() { "assets/bundled/prefabs/fx/oiljack/pump_up.prefab", "assets/bundled/prefabs/fx/oiljack/pump_down.prefab", "assets/prefabs/npc/bear/sound/breathe.prefab", "assets/content/sound/monuments/nuclearmissilesilo/effects/nuclear-missile-silo-elevator-door-open-end.prefab", "assets/content/structures/office_interiors/effects/vent-door-open.prefab", "assets/prefabs/misc/halloween/skull_door_knocker/effects/door_knock_fx.prefab", "assets/prefabs/weapons/halloween/skull torch/effects/ignite.prefab" };
            public enum CollectibleType { Wood, Stone, Metal, Sulfur, Bone };
            public float CheckRate
            {
                get { return _checkRate; }
                set
                {
                    _checkRate = value;
                    CancelInvoke(DoCheck);
                    InvokeRepeating(DoCheck, _checkRate, _checkRate);
                }
            }
            public string GetPrefab(CollectibleType type)
            {
                var sb = Pool.Get<StringBuilder>();
                try
                {
                    return sb.Clear().Append("assets/bundled/prefabs/autospawn/collectable/stone/halloween/halloween-").Append(type.ToString().ToLower()).Append("-").Append(type == CollectibleType.Sulfur ? "collectible" : "collectable").Append(".prefab").ToString();
                }
                finally { Pool.Free(ref sb); }
            }
            private static readonly System.Random rng = new();

            private void Awake()
            {
                player = GetComponent<BasePlayer>();

                if (player == null || player.IsDestroyed || player.gameObject == null)
                {
                    Interface.Oxide.LogError("HalloweenPlayer called on null/destroyed player!");

                    DoDestroy();

                    return;
                }

                InvokeRepeating(DoCheck, CheckRate, CheckRate);
                InvokeRepeating(DoSounds, UnityEngine.Random.Range(20, 50), UnityEngine.Random.Range(30, 75));

                if (player.IsAdmin)
                    Interface.Oxide.LogWarning("HalloweenPlayer.Awake() for admin player. CheckRate: " + CheckRate);
            }

            public void DoSounds()
            {
                if (UnityEngine.Random.Range(0, 101) > rng.Next(15, 40))
                    return;

                var rngEffect = SoundEffects.GetRandom();
                var runPos = player?.eyes?.position ?? Vector3.zero;
                runPos.y -= UnityEngine.Random.Range(0, 3);
                runPos.x += UnityEngine.Random.Range(0, 2);
                runPos.z += UnityEngine.Random.Range(0, 5);

                for (int i = 0; i < UnityEngine.Random.Range(1, 3); i++) 
                    Instance?.SendLocalEffect(player, rngEffect, runPos);


            }

            public void DoCheck()
            {
                if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead()) return;

                if (!player.IsConnected)
                {
                    DoDestroy();
                    return;
                }

                var eyePos = player?.eyes?.position ?? Vector3.zero;
                var visRange = UnityEngine.Random.Range(10, 16);

                var nearCollectibles = Pool.GetList<CollectibleEntity>();

                try
                {
                    Vis.Entities(eyePos, visRange, nearCollectibles);

                    for (int i = 0; i < nearCollectibles.Count; i++)
                    {
                        var collect = nearCollectibles[i];
                        if (collect == null || !SeenCollectibles.Add(collect)) 
                            continue;

                        if (player == null || player.gameObject == null || player.IsDestroyed || (!collect.ShortPrefabName.Contains("halloween") && !collect.ShortPrefabName.Contains("mushroom")) || !collect.IsVisible(player.eyes.position)) 
                            continue;

                        if (rng.Next(0, 101) <= UnityEngine.Random.Range(7, 15))
                        {
                            var pos = collect?.transform?.position ?? Vector3.zero;
                            var spawnPos = pos;
                            spawnPos.y -= 2.4f;
                            var rot = collect?.transform?.rotation ?? Quaternion.identity;

                            var type = (CollectibleType)UnityEngine.Random.Range(0, 5);
                            var newCollect = GameManager.server.CreateEntity(GetPrefab(type), spawnPos, rot);

                            if (newCollect == null)
                                continue;


                            newCollect.enableSaving = false;

                            newCollect.Spawn();

                            newCollect.EnableSaving(false);

                            newCollect.Invoke(() =>
                            {
                                if (!newCollect.IsDestroyed) newCollect.Kill();
                            }, 45f);

                            var smooth = newCollect.gameObject.AddComponent<SmoothMovement>();
                            smooth.speed = 0.55f;

                            var kcEnt = newCollect;

                            var newDest = new Vector3(kcEnt.transform.position.x, kcEnt.transform.position.y + 2.4f, kcEnt.transform.position.z);

                            smooth.startTime = UnityEngine.Time.time;
                            smooth.StartPos = kcEnt.transform.position;
                            smooth.EndPos = newDest;

                            newCollect.Invoke(() =>
                            {
                                Effect.server.Run("assets/bundled/prefabs/fx/dig_effect.prefab", pos, Vector3.zero);
                                if (UnityEngine.Random.Range(0, 101) <= 10)
                                {
                                    var howlPos = eyePos + Vector3.back * UnityEngine.Random.Range(4, 8);
                                    for (int j = 0; j < 3; j++) Effect.server.Run("assets/bundled/prefabs/fx/player/howl.prefab", howlPos, Vector3.zero);
                                }
                            }, UnityEngine.Random.Range(0.8f, 1.25f));

                            if (collect != null && !collect.IsDestroyed)
                                collect.Kill();
                        }
                    }
                }
                finally
                {
                    Pool.FreeList(ref nearCollectibles);
                }

              
            }

            public void DoDestroy()
            {
                try
                {
                    CancelInvoke(DoSounds);
                    CancelInvoke(DoCheck);
                }
                finally { Destroy(this); }
            }

        }

        private class SmoothMovement : MonoBehaviour
        {
            public BaseEntity entity;
            public Vector3 StartPos { get; set; } = Vector3.zero;

            private Vector3 _endPos = Vector3.zero;

            public Vector3 EndPos
            {
                get { return _endPos; }
                set
                {
                    _endPos = value;
                    journeyLength = Distance;
                }
            }

            public Vector3 EulerAngles = Vector3.zero;
            public float Distance { get { return Vector3.Distance(StartPos, EndPos); } }
            public float startTime = 0f;
            public float speed = 10f;

            private float journeyLength;

            public bool doRotation = false;
            public bool DestroyOnDestinationHit = true;

            private void Awake()
            {
                entity = GetComponent<BaseEntity>();
                if (entity == null || entity.IsDestroyed)
                {
                    DoDestroy();
                    return;
                }
                StartPos = entity.transform.position;
                startTime = UnityEngine.Time.time;
            }

            public void DoDestroy() => Destroy(this);

            private void Update()
            {
                if (entity == null || entity?.transform == null) return;

                if (StartPos == Vector3.zero || EndPos == Vector3.zero) return;

                if (doRotation && EulerAngles != Vector3.zero)
                {
                    var lerpEuler = Vector3.Lerp(entity.transform.eulerAngles, EulerAngles, UnityEngine.Time.deltaTime * speed);
                    var rotEuler = Quaternion.Euler(lerpEuler);
                    entity.transform.rotation = rotEuler;
                }

                var fracJourney = (UnityEngine.Time.time - startTime) * speed / journeyLength;
                entity.transform.position = Vector3.Lerp(StartPos, EndPos, fracJourney);
                entity.SendNetworkUpdateImmediate();

                if (DestroyOnDestinationHit && Vector3.Distance(entity.transform.position, EndPos) <= 0.05) DoDestroy();
            }

        }

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using var fx = new Effect(effect, pos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using var fx = new Effect(effect, player, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var npcCorpse = entity as NPCPlayerCorpse;

            if (npcCorpse == null)
                return;

            if (!Halloween.enabled)
                return;

            if (!npcCorpse.ShortPrefabName.Contains("murderer") && !npcCorpse.ShortPrefabName.Contains("scare"))
                return;


            if (Halloween.enabled && npcCorpse != null && (npcCorpse.ShortPrefabName.Contains("murderer") || npcCorpse.ShortPrefabName.Contains("scare")))
            {
                npcCorpse.SetLootableIn(0.01f);

                npcCorpse?.Invoke(() =>
                {
                    if (npcCorpse?.containers == null || npcCorpse.containers.Length < 1)
                        return;

                    var firstCont = npcCorpse.containers[0];

                    var lootBagRNG = UnityEngine.Random.Range(0, 101);

                    var lootBagCount = lootBagRNG <= 33 ? UnityEngine.Random.Range(1, 8) : lootBagRNG <= 66 ? UnityEngine.Random.Range(2, 5) : UnityEngine.Random.Range(3, 11);
                    var lootBag = ItemManager.CreateByName("halloween.lootbag.small", lootBagCount);

                    if (!lootBag.MoveToContainer(npcCorpse.containers[0]))
                    {
                        PrintWarning("unable to move lootbag");
                        return;
                    }

                    var amtItems = UnityEngine.Random.Range(2, 5);

                    var newItems = Pool.GetList<Item>();

                    try
                    {
                        for (int i = 0; i < amtItems; i++)
                            newItems.Add(GetRandomHalloweenItem());

                        for (int i = 0; i < newItems.Count; i++)
                        {
                            var item = newItems[i];
                            if (npcCorpse == null || npcCorpse?.containers == null || npcCorpse.containers.Length < 1)
                            {
                                PrintWarning("corpse or containers were null during loop");
                                break;
                            }
                            if (item != null && !item.MoveToContainer(firstCont))
                            {
                                PrintWarning("failed to move item to corpse or null item!!");
                                RemoveFromWorld(item);
                            }
                        }
                    }
                    finally { Pool.FreeList(ref newItems); }

                   

                }, 0.01f);


            }
        }

        private Item GetRandomHalloweenItem()
        {
            var rng = UnityEngine.Random.Range(0, 116);

            //down with LINQ
            var rngItems = ItemManager.itemList?.Where(p => p != null && !p.shortname.Contains("surgeon") && (p.shortname.Contains("hween") || p.shortname.Contains("halloween") || p.shortname.Contains("jacko") || p.shortname.Contains("scarecrow") || p.shortname.Contains("skull")))?.Select(p => p.shortname)?.ToList() ?? null;

            var rngItem = rngItems[UnityEngine.Random.Range(0, rngItems.Count)];
            return ItemManager.CreateByName(rngItem);
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private Vector3 GetGroundPosition(Vector3 pos, int maxTries = 20)
        {
            if (pos == Vector3.zero || FindGround == null || !FindGround.IsLoaded) return pos;
            var newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
            maxTries = maxTries < 1 ? 1 : maxTries > 100 ? 100 : maxTries;
            if (newPosObj == null)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    if (newPosObj != null) break;
                    newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
                }
                if (newPosObj == null)
                {
                    PrintWarning("null pos after " + maxTries.ToString("N0") + " tries");
                    return Vector3.zero;
                }
            }
            return (Vector3)newPosObj;
        }

    }
}
