using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using ConVar;
using Pool = Facepunch.Pool;
using Oxide.Core;
using System.Diagnostics;
using Oxide.Game.Rust.Cui;
using Random = UnityEngine.Random;
using ProtoBuf;
using Environment = System.Environment;
using System.Collections;
using Newtonsoft.Json;
using Physics = UnityEngine.Physics;
using Oxide.Core.Libraries.Covalence;
using System.Reflection;

namespace Oxide.Plugins
{
    [Info("Delivery Jobs", "Shady", "0.0.1")]
    internal class DeliveryJobs : RustPlugin
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
        //todo: check todos in other areas of code.

        //todo: remove delivery/job items placed by job in cars (i.e if a job is taken & the car is loaded with 10k fuel, remove it on unload/when job is canceled) (mostly done?)

        //todo: make it so you cannot take fuel OUT of company truck if spawns with fuel.

        //todo: send message/alert when dismounting car with ActiveDeliveryJob

        //todo: time remaining shows "less than a second" with no time limit jobs; partly not an issue.

        //todo: send a friendly job time remaining reminder every 0.25 of fraction? (0.75, 0.5, 0.25)

        //todo: fail if car is fully submerged in water

        //todo: more damage (especially to module?) = less pay (implemented, can be refactored).

        //minor issue: water can be drank

        //todo: send map markers to passengers? - done, needs testing.

        //todo: parking marker is global - fix. only network to people in car and owner.

        //todo: use talk gestures for quality

        //radio? boombox.static inside a car?

        //todo: proper battery rewards (currency) for completion (done)

        //use fx when selecting job?: assets/bundled/prefabs/fx/item_unlock.prefab

        //todo: company truck needs to be locked so only person who spawned it can take it

        //todo: company trucks.
        //todo: all company trucks should have specific identifier - perhaps OwnerID?

        //NOTE:
        //code for getting intended rotations seems to result in negatives in the xyz values whereas their actual rotation is unchanged. i don't understand this, but it works.
        //for example: you can do /lookat on an object to see its rotation. if you do /monrottest, it'll give you that same rotation, but with negative values. however, setting it to this does not change anything - it is still rotated correctly.

        //todo:
        //make a JSON similar to Deathmatch structure where you can add multiple delivery spots (limited to monuments)
        //we can use local pos inside the monument to always get exact same pos with every wipe (except if a mounment change is made to game)
        //allow setting min-max rng tier options for each delivery "spawn point"?
        //needs a /dm_edit like command. /dj_edit? /dj?

        //company cars should always have low quality comps, perhaps except jobs where you *must* use a company car.

        private const uint TANKER_MODULE_PREFAB_ID = 4177081746;

        private const uint TANKER_STORAGE_CONTAINER_PREFAB_ID = 1803881164; //find equiv for storage if ever necessary

        private const uint STORAGE_MODULE_PREFAB_ID = 2578160356;

        private const uint BANDIT_CONVERSATIONALIST_PREFAB_ID = 251735616;

        private const int WATER_ITEM_ID = -1779180711;
        private const int SALT_WATER_ITEM_ID = -277057363;
        private const int LGF_ITEM_ID = -946369541;
        private const int CRUDE_OIL_ITEM_ID = -321733511;
        private const int DIESEL_ITEM_ID = 1568388703;

        private const int BATTERY_SMALL_ITEM_ID = 609049394;

        private const string CHAT_STEAM_ID = "76561198865536053";

        private const string MISSION_ACCEPT_FX = "assets/prefabs/missions/effects/mission_accept.prefab";
        private const string MISSION_VICTORY_FX = "assets/prefabs/missions/effects/mission_victory.prefab";
        private const string MISSION_OBJECT_COMPLETE_FX = "assets/prefabs/missions/effects/mission_objective_complete.prefab";
        private const string MISSION_FAILED_FX = "assets/prefabs/missions/effects/mission_failed.prefab";

        private const string CODE_LOCK_DENIED_FX = "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab";

        public static DeliveryJobs Instance { get; set; } = null;

        private readonly HashSet<Coroutine> _coroutines = new();

        [PluginReference]
        private readonly Plugin Luck;

        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        [PluginReference]
        private readonly Plugin RustPlusExtended;

        private const string FX_MISSING = "assets/bundled/prefabs/fx/missing.prefab";

        private readonly HashSet<int> _ignoreItems = new() { 878301596, -2027988285, -44066823, -1364246987, 62577426, 1776460938, -17123659, 999690781, -810326667, 996757362, 1015352446, 1414245162, -1449152644, -187031121, 1768112091, 1770744540, -602717596, 1223900335, 1036321299, 204970153, 236677901, -399173933, 1561022037, 1046904719, 1394042569, -561148628, 861513346, -1366326648, -1884328185, 1878053256, 550753330, 696029452, 755224797, 1230691307, 1364514421, -455286320, 1762167092, -282193997, 180752235, -1386082991, 70102328, 22947882, 81423963, 1409529282, -1779180711, -277057363, -544317637, 1113514903 };

        private static readonly System.Random _rarityRng = new();

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private readonly HashSet<string> _forbiddenTags = new(6) { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private MonumentInfo _harbor2;
        private MonumentInfo Harbor2
        {
            get
            {
                return _harbor2 ??= FindMonumentInfo("harbor_2");
            }
        }


        //NEW, USE:
        private class MonumentDeliveryLocations
        {
            public Dictionary<string, List<DeliveryLocation>> _monumentNameDeliveryLocations = new();

            public MonumentDeliveryLocations() { }


            public void OnLoaded()
            {
                foreach (var kvp in _monumentNameDeliveryLocations)
                    for (int i = 0; i < kvp.Value.Count; i++)
                        kvp.Value[i].SetMonument(FindMonumentInfo(kvp.Key));
            }


            public List<DeliveryLocation> GetMonumentDeliveriesByName(string monumentName) => _monumentNameDeliveryLocations.TryGetValue(monumentName, out var deliveryLocations) ? deliveryLocations : null;
            

            public void AddDeliveryLocationByMonument(MonumentInfo monument, Vector3 localPos)
            {
                if (monument == null)
                    throw new ArgumentNullException(nameof(monument));

                var shortName = GetMonumentShortPrefabName(monument.name);

                AddDeliveryLocation(shortName, localPos);
            }

            public void AddDeliveryLocation(string shortName, Vector3 localPos)
            {
                if (string.IsNullOrWhiteSpace(shortName))
                {
                    Interface.Oxide.LogError(nameof(shortName) + " is null/empty!!!");
                    return;
                }

                if (!_monumentNameDeliveryLocations.TryGetValue(shortName, out _))
                    _monumentNameDeliveryLocations[shortName] = new List<DeliveryLocation>();


                var newLoc = new DeliveryLocation(shortName, localPos);
                _monumentNameDeliveryLocations[shortName].Add(newLoc);
            }

            public bool RemoveDeliveryLocation(string shortName, DeliveryLocation loc)
            {
                if (string.IsNullOrWhiteSpace(shortName))
                    throw new ArgumentNullException(nameof(shortName));

                if (!_monumentNameDeliveryLocations.TryGetValue(shortName, out var deliveryLocations)) 
                    return false;

                return deliveryLocations.Remove(loc);
            }

        }

        private MonumentDeliveryLocations _deliveryData;

      

        private static Dictionary<string, MonumentInfo> _monumentCache = new();

        private static string GetMonumentShortPrefabName(string monName)
        {
            if (string.IsNullOrWhiteSpace(monName))
                return monName;

            var sb = Pool.Get<StringBuilder>();

            try
            {
                var lastIndex = monName.LastIndexOf("/");

                return sb.Clear().Append(lastIndex == -1 ? monName : monName.Substring(lastIndex + 1)).Replace(".prefab", string.Empty).ToString();

            }
            finally { Pool.Free(ref sb); }
        }

        private static MonumentInfo FindMonumentInfo(string partialOrFullName)
        {
            if (string.IsNullOrWhiteSpace(partialOrFullName))
                throw new ArgumentNullException(nameof(partialOrFullName));

            if (_monumentCache == null) _monumentCache = new();
            else if (_monumentCache.TryGetValue(partialOrFullName, out var monumentInfo))
                return monumentInfo;


            MonumentInfo info = null;            

            for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
            {
                var monument = TerrainMeta.Path.Monuments[i];
                var monDisplayName = monument?.displayPhrase?.english ?? string.Empty;

                var monName = monument?.name ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(monName))
                    monName = GetMonumentShortPrefabName(monName);
                else
                    continue;

                if (monName.Equals(partialOrFullName, StringComparison.OrdinalIgnoreCase) || monDisplayName.Equals(partialOrFullName, StringComparison.OrdinalIgnoreCase) || monDisplayName.IndexOf(partialOrFullName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (info != null)
                    {
                        Interface.Oxide.LogWarning("found more than 1 monument by this name and will return null!: " + partialOrFullName);
                        return null;
                    }

                    info = monument;
                }
            }

            if (info != null)
                _monumentCache[partialOrFullName] = info;

            return info;
        }

        public static bool IsCargoModule(uint prefabId)
        {
            return prefabId switch
            {
                TANKER_MODULE_PREFAB_ID or STORAGE_MODULE_PREFAB_ID => true,
                _ => false,
            };
        }

        private void Init()
        {
            Instance = this;

            Unsubscribe(nameof(OnEntityKill));

            AddCovalenceCommand("djedit", nameof(CmdDeliveryJobEdit));
            AddCovalenceCommand("djadd", nameof(CmdDeliveryJobAdd));
         
        }

        private void Unload()
        {
            try
            {
                try 
                {
                    foreach (var coroutine in _coroutines)
                    {
                        try 
                        {
                            if (coroutine == null)
                                continue;

                            ServerMgr.Instance?.StopCoroutine(coroutine); 
                        }
                        catch(Exception ex) { PrintError(ex.ToString()); }
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                foreach (var car in ModularCar.allCarsList)
                {
                    try
                    {
                        var job = car?.GetComponent<ActiveDeliveryJob>();

                        if ((job?.IsCompanyCar() ?? false))
                            job?.KillCar();

                        job?.DoDestroy();
                    }
                    catch (Exception ex) { PrintError(ex.ToString()); }
                }

                try
                {
                    _monumentCache?.Clear();
                    _monumentCache = null;
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                        StopJobGUI(BasePlayer.activePlayerList[i]);
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    foreach (var userId in _invLocked)
                    {
                        var p = RelationshipManager.FindByID(userId);
                        if (p == null)
                            continue;

                        try
                        {
                            if (p.IsConnected)
                                p.EndLooting();
                        }
                        finally { LockMainAndBeltContainers(p, false); }
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                foreach (var ent in _temporaryEntities)
                {
                    try
                    {
                        if (!(ent?.IsDestroyed ?? true))
                            ent.Kill();
                    }
                    catch (Exception ex) { PrintError(ex.ToString()); }
                }

                try { SaveDeliveryData(); }
                catch (Exception ex) { PrintError(ex.ToString()); }


            }
            finally { Instance = null; }

        }

        private void OnEntityTakeDamage(BaseVehicleModule module, HitInfo info)
        {
            if (module?.GetParentEntity() is not ModularCar car || !IsCarOnJob(car, out var job))
                return;

            module?.Invoke(() =>
            {
                var dmgTotal = info?.damageTypes?.Total() ?? 0f;
                if (dmgTotal > 0f)
                    job.OnModuleDamaged(module, dmgTotal);

            }, 0.01f);

           

        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null) return;

        }

        private void OnEntityMounted(BaseMountable entity, BasePlayer player)
        {
            if (entity is not ModularCarSeat carSeat)
                return;

            var car = carSeat?.associatedSeatingModule?.Car;

            if (car == null || !IsCarOnJob(car, out var job))
                return;

            if (job.JobOwner == player)
            {
                //colorize?

                var sb = Pool.Get<StringBuilder>();
                try 
                {
                    var timeRem = sb.Clear().Append("Delivery Time Remaining:\n").Append(GetTimeSpanFromMs(job.TimeRemaining.TotalMilliseconds, true)).ToString();

                    var alertFrac = 0.2;

                    ShowToast(player, timeRem, job.TimeRemainingFraction <= alertFrac ? 1 : 0); //1 alert if <= 0.2 (20%).
                }
                finally { Pool.Free(ref sb); }
                


             
            }

            Interface.Oxide.LogWarning("Running ensure");
            job.EnsureAllMountedHaveMapMarker();
            Interface.Oxide.LogWarning("Ran ensure");

        }

        private void OnEntityDismounted(BaseMountable entity, BasePlayer player)
        {
            if (entity is not ModularCarSeat carSeat)
                return;

            var car = carSeat?.associatedSeatingModule?.Car;

            if (car == null || !IsCarOnJob(car, out var job))
                return;
            
            if (job.JobOwner == player)
            {
                PrintWarning(nameof(OnEntityDismounted) + " JobOwner is player dismounted, so NOT removing.");
                return;
            }


            Interface.Oxide.LogWarning("Running remove");

            job.RemoveMarker(player, job.ParkingDestinationMarker);

            Interface.Oxide.LogWarning("Ran Remove");

        }

        private readonly HashSet<ulong> _invLocked = new();

        private void LockMainAndBeltContainers(BasePlayer player, bool desiredState)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player?.inventory == null)
                return;


            player.inventory.containerBelt.SetLocked(desiredState);
            player.inventory.containerMain.SetLocked(desiredState);
        }

        private void OnLootEntityEnd(BasePlayer player, BaseEntity entity)
        {
            if (_invLocked.Remove(player.userID))
                LockMainAndBeltContainers(player, false);
        }

        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (_invLocked.Contains(player.userID) || (entity?.GetParentEntity() is not VehicleModuleStorage modStorage || !IsCarOnJob(modStorage?.Car)))
                return;


            _invLocked.Add(player.userID);


            LockMainAndBeltContainers(player, true);
        }

        private object CanLootEntity(BasePlayer player, ContainerIOEntity ioContainer)
        {
            //todo: also deny if company truck; regardless of job status.

            if (ioContainer is ModularCarGarage carGarage && IsCarOnJob(carGarage?.carOccupant))
            {
                SendLocalEffectOnPlayer(player, CODE_LOCK_DENIED_FX, boneID: StringPool.Get("head"));
                ShowToast(player, "Trucks can't be modified while a job is active. Please finish or cancel your job.\nIf needed, repair your truck with a hammer.", 1);
                return false;
            }


            return null;
        }

        //custom harmony hook via DropItemsPatch.cs
        private object OnDropItems(IItemContainerEntity containerEntity, BaseEntity initiator = null)
        {
            //getparent because we get the actual storage container and not the module at first.
            if (containerEntity?.inventory?.entityOwner?.GetParentEntity() is not VehicleModuleStorage modStorage)
                return null;

            //true to override. null to let it be.
            //todo: if company truck, even if somehow ActiveDeliveryJob is null, deny droppage.
            return IsCarOnJob(modStorage?.Car) ? true : null;
        }

        private bool IsCarOnJob(ModularCar car) => car?.net != null && car.net.ID.IsValid && ActiveDeliveryJob.DeliveryCars.Contains(car.net.ID);

        private bool IsCarOnJob(ModularCar car, out ActiveDeliveryJob job)
        {
            if (car?.net != null && car.net.ID.IsValid && ActiveDeliveryJob.DeliveryCars.Contains(car.net.ID))
            {
                job = car?.GetComponent<ActiveDeliveryJob>();
                return job != null;
            }

            job = null;
            return false;
        }

        private void OnEntityKill(ModularCar car)
        {
            car?.GetComponent<ActiveDeliveryJob>()?.DoDestroy();
        }

        private void OnEntityDeath(ModularCar car, HitInfo info)
        {
            car?.GetComponent<ActiveDeliveryJob>()?.DoDestroy();
        }

        //likely unnecessary (car cannot be used on lift while on job):
        private object OnVehicleModuleMove(BaseVehicleModule module, BaseModularVehicle vehicle, BasePlayer player)
        {
            if (vehicle is ModularCar car && IsCarOnJob(car))
            {
                ShowToast(player, "Cannot be modified at this time!", 1);
                return true; //override! do not allow.
            }

            return null;
        }


        private readonly Dictionary<NPCTalking, TimeCachedValue<bool>> _npcTalkingInBounds = new();

        private object OnNpcConversationStart(NPCTalking npc, BasePlayer player, ConversationData convFor)
        {
            if (npc == null || player == null || convFor == null || npc.prefabID != BANDIT_CONVERSATIONALIST_PREFAB_ID)
                return null;

           

            if (!_npcTalkingInBounds.TryGetValue(npc, out _))
                _npcTalkingInBounds[npc] = new TimeCachedValue<bool>()
                {
                    refreshCooldown = 20f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<bool>(() =>
                    {
                        return Harbor2?.IsInBounds((npc?.transform?.position ?? Vector3.zero)) ?? false;
                    })
                };

            if (!_npcTalkingInBounds[npc].Get(false))
                return null;


            var toRemove = Pool.Get<HashSet<DeliveryJob>>();
            try
            {
                toRemove.Clear();

                foreach (var job in _availableJobs)
                {
                    if (job.Started)
                        toRemove.Add(job);
                }

                foreach (var job in toRemove)
                    _availableJobs.Remove(job);

            }
            finally { Pool.Free(ref toRemove); }

            var _allJobs = Pool.Get<HashSet<DeliveryJob>>();

            try 
            {
                _allJobs.Clear();

                if (_availableJobs.Count < 1)
                    GenerateRandomDeliveryJobsNoAlloc(ref _allJobs, Random.Range(3, 13)); //yes, we intentionally ref _allJobs. check code immediately below
                


                foreach (var job in _allJobs)
                {
                    if (!job.Started)
                        _availableJobs.Add(job);
                }

                DeliveryJobsGUI_New(player);
            //    JobGUI(player);

            }
            finally { Pool.Free(ref _allJobs); }


            return true;
        }

        private const string JOB_CUI_NAME = "DJ_GUI";

        [ConsoleCommand("deliveryjobs.stopgui")]
        private void cmdStopJobsGUI(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null)
                return;

            StopJobGUI(player);

        }

        private readonly Dictionary<string, float> _lastTakeJobCmd = new();

        //also incredibly messy at this time
        [ConsoleCommand("deliveryjobs.takejob")]
        private void cmdTakeJob(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null)
                return;

            if (_lastTakeJobCmd.TryGetValue(player.UserIDString, out var time) && (UnityEngine.Time.realtimeSinceStartup - time) < 2)
            {
                PrintWarning(nameof(cmdTakeJob) + " cooldown!!!");
                return;
            }
            else _lastTakeJobCmd[player.UserIDString] = UnityEngine.Time.realtimeSinceStartup;

            if (arg?.Args == null || arg.Args.Length < 1 || !int.TryParse(arg.Args[0], out var jobIndex))
            {
                PrintWarning(nameof(cmdTakeJob) + " with no args or no int arg0");
                return;
            }

            if (jobIndex > _availableJobs.Count)
            {
                PrintWarning(nameof(jobIndex) + " higher than " + nameof(_availableJobs) + "!");
                return;
            }

            DeliveryJob desiredJob = null;

            var i = 0;

            foreach (var job in _availableJobs)
            {
                if (i == jobIndex)
                {
                    desiredJob = job;
                    break;
                }

                i++;
            }

            if (desiredJob == null)
            {
                PrintError(nameof(desiredJob) + " is null!!!! index: " + jobIndex);
                return;
            }

            if (HarborArea == null)
            {
                PrintError(nameof(HarborArea) + " is null!?!?!?");
                return;
            }

            var car = HarborArea?.CarLift?.carOccupant;

            if (car == null)
            {
                SendReply(player, "Please park your truck on the car lift nearby!");
                ShowToast(player, "No truck on lift!", 1);
                return;
            }

            if (car?.OwnerID != player.userID)
            {
                SendReply(player, "Someone else's car is currently parked on the lift. Please wait!");
                ShowToast(player, "Someone else's car is parked on the lift!", 1);
                return;
            }

            if (!(car?.net?.ID.IsValid ?? false))
            {
                SendReply(player, "Car has invalid netID?!");
                return;
            }

            if (ActiveDeliveryJob.DeliveryCars.Contains(car.net.ID))
            {
                SendReply(player, "This car is already on a job!");
                return;
            }

            if (ActiveDeliveryJob.PlayerToJob.TryGetValue(player, out _))
            {
                //future todo(?): reply with info about current job.

                SendReply(player, "You're already on a job! Please cancel your current job first.\nYou can cancel it by removing the map marker on your map.");
                return;
            }

            var hasAppropriateModule = false;

            var desiredModule = desiredJob.GetDesiredCargoModulePrefabID();

            if (car?.AttachedModuleEntities != null)
            {
                for (int j = 0; j < car.AttachedModuleEntities.Count; j++)
                {
                    var mod = car.AttachedModuleEntities[j];

                    if (mod.prefabID == desiredModule)
                    {
                        hasAppropriateModule = true;
                        break;
                    }

                }
            }

            if (!hasAppropriateModule)
            {
                SendReply(player, "Your car does not have the appropriate module for this type of job! Liquid jobs require a tanker. Solids require at least one storage module.");
                return;
            }

            StopJobGUI(player);

            var delJob = car?.gameObject?.AddComponent<ActiveDeliveryJob>();

            if (delJob == null)
            {
                SendReply(player, "Somehow car job is null, report to an admin!!");
                return;
            }

            if (!ActiveDeliveryJob.DeliveryCars.Add(car.net.ID))
                PrintError(nameof(ActiveDeliveryJob.DeliveryCars) + " already contained netID?!");
            else delJob.InitializeJob(desiredJob, player);

            SendLocalEffectOnPlayer(player, MISSION_ACCEPT_FX);

        }

        //probably not very useful if not providing ownerid
        private ModularCar GetCarNearby(Vector3 pos, float radius, ulong ownerId = 0)
        {

            var cars = Pool.GetList<ModularCar>();
            try
            {
                Vis.Entities(pos, radius, cars);

                for (int i = 0; i < cars.Count; i++)
                {
                    var car = cars[i];

                    if (ownerId != 0 && car.OwnerID != ownerId)
                        continue;

                    return car;
                }

            }
            finally { Pool.FreeList(ref cars); }


            return null;
        }

        private void JobGUI(BasePlayer player, string additionalText = "")
        {
            if (player == null || !player.IsConnected) return;

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.92"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = true
            }, "Hud", JOB_CUI_NAME);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "deliveryjobs.page prev",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Previous Page",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.1",
                    AnchorMax = "0.6 0.15"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "deliveryjobs.stopgui",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Close",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0",
                    AnchorMax = "0.6 0.05"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "none.none",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Next Page",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.2",
                    AnchorMax = "0.6 0.25"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "none.none",
                        Color = "0.8 0.24 0.45 0.425"
                    },
                Text =
                {
                    Text = "<color=yellow>button</color>",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "0.156 0.056"
                }
            }, GUISkin);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                GUISkinElement.Add(new CuiLabel
                {
                    Text =
                {
                    Text = "Delivery Job GUI",//sb.Clear().Append("<color=").Append(LUCK_COLOR).Append("><color=#ff80bf>").Append(tree).Append("</color> Skill tree\nCurrent Points: </color>").Append(luckInfo.SkillPoints.ToString("N0")).Append("\n<color=").Append(LUCK_COLOR).Append(">Luck XP:</color> ").Append(luckInfo.XP.ToString("N0")).Append("<color=").Append(LUCK_COLOR).Append(">/</color>").Append(GetPointsForLevel(luckInfo.Level + 1).ToString("N0")).Append(" <color=").Append(LUCK_COLOR).Append(">(</color>").Append(GetExperiencePercentString(player)).Append("<color=").Append(LUCK_COLOR).Append(">)\nCatchup multiplier:</color> ").Append(getCatchupMultiplier(player.userID).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">XP Rate:</color> ").Append(GetXPMultiplier(player.UserIDString).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">Item Rate:</color> ").Append(getItemMultiplier(player.UserIDString).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">Chance multiplier:</color> ").Append(GetChanceBonuses(player.UserIDString).ToString("0.00").Replace(".00", string.Empty)).Append("%\n<color=#ff9933>Read <color=").Append(LUCK_COLOR).Append(">/skt help</color> before using this.</color>").Append(!string.IsNullOrEmpty(additionalText) ? "\n" : string.Empty).Append(additionalText).ToString(),
                    FontSize = 20,
                    Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                    RectTransform =
                {
                    AnchorMin = "0.705 0.65",
                    AnchorMax = "1 1"
                }
                }, GUISkin);
            }
            finally { Pool.Free(ref sb); }


            var buttonYMin = 0.958;
            var buttonYMax = 1d;

            var button2YMin = 0.917;
            var button2YMax = 0.958;

            var labelXMin = 0.2;
            var labelXMax = 0.475;

            var labelYMin = 0.961;
            var labelYMax = 0.983;


            sb = Pool.Get<StringBuilder>();
            try
            {

                var i = 0;
                foreach (var job in _availableJobs)
                {


                    var cmdText = sb.Clear().Append("deliveryjobs.takejob ").Append(i).ToString();

                    var nearestMonName = job?.LocationDelivery?.GetMonument()?.displayPhrase?.english ?? job?.LocationDelivery?.MonumentShortPrefabName ?? "Unknown";

                    var labelText = sb.Clear().Append("Delivery to: ").Append(nearestMonName).ToString();

                    GUISkinElement.Add(new CuiLabel
                    {
                        Text =
                {
                    Text = sb.Clear().Append("<-- ").Append(labelText).ToString(),
                    Align = TextAnchor.MiddleLeft,
                    FontSize = 14,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append(labelXMin).Append(" ").Append(labelYMin).ToString(),
                    AnchorMax = sb.Clear().Append(labelXMax).Append(" ").Append(labelYMax).ToString()
                }
                    }, GUISkin);



                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = cmdText,
                        Color = "0.2 0.4 0.65 0.75"
                    },
                        Text =
                {
                    Text = sb.Clear().Append("<color=#F6D1BE>").Append(job.Tier).Append(" delivery of ").Append(job.TypeCargo).Append(" to ").Append(nearestMonName).Append(" (Time Limit: ").Append(job.LocationDelivery.TimeLimit.TotalSeconds > 0 ? job.LocationDelivery.TimeLimit.ToString() : "None").Append(")</color>").ToString(),
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.0 ").Append(buttonYMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.195 ").Append(buttonYMax).ToString()
                }
                    }, GUISkin);

                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = "no command",
                        Color = "0.15 0.35 0.72 0.75"
                    },
                        Text =
                {
                    Text = "show more job info", //sb.Clear().Append("<size=16><color=#99d6ff>More <i>").Append(buttonSkillName).Append("</i> info</color> </size>(<color=#33adff>See top right</color>)").ToString(),
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.0 ").Append(button2YMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.195 ").Append(button2YMax).ToString()
                }
                    }, GUISkin);

                    buttonYMin -= 0.1;
                    buttonYMax -= 0.1;

                    button2YMin -= 0.1;
                    button2YMax -= 0.1;

                    labelYMin -= 0.1;
                    labelYMax -= 0.1;

                    i++;

                }
            }
            finally { Pool.Free(ref sb); }



            CuiHelper.DestroyUi(player, JOB_CUI_NAME); //destroy only after it's ready to be added
            CuiHelper.AddUi(player, GUISkinElement);
        }

        private void StopJobGUI(BasePlayer player) => CuiHelper.DestroyUi(player, JOB_CUI_NAME);

        private readonly HashSet<MapNote> _hasAttempedToRemove = new();

        private object OnMapMarkerRemove(BasePlayer player, MapNote note)
        {
            if (player == null || note == null)
                return null;

            if (note.totalDuration != 1337f)
                return null;

            if (_hasAttempedToRemove.Add(note))
            {
                ShowToast(player, "Try again to confirm remove.\nTHIS WILL CANCEL YOUR JOB.", 1);

                player?.Invoke(() =>
                {
                    _hasAttempedToRemove?.Remove(note);
                }, 4f);

                return true;
            }
            else if (_hasAttempedToRemove.Remove(note))
            {

                //cancel job.

                if (ActiveDeliveryJob.PlayerToJob.TryGetValue(player, out var job))
                {
                    job.CancelJob();
                    ShowToast(player, "Job canceled!", 1);
                }
                else
                {
                    ShowToast(player, "Couldn't cancel job!!!\nPlease report this to an admin.", 1);
                    PrintWarning("Failed to cancel job on map note removal!!! no _playerToJob get");

                    return null;
                }

                return true; //not preventing the original remove means CancelJob() calls it first and causes an RPC error on client.

            }

            return null;
        }

        private readonly HashSet<MapNote> _unremovableMapNotes = new();

        private MapNote AddPermanentMapMarker(BasePlayer player, Vector3 worldPos, string label = "", int icon = 1, int noteType = 1, int colorIndex = -1, bool isPing = false, bool canBeRemoved = true)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (!player.IsConnected || player?.State == null)
                return null;

            var mapN = new MapNote
            {
                colourIndex = colorIndex,
                totalDuration = 99999,
                icon = icon,
                isPing = isPing,
                worldPosition = worldPos,
                label = label,
                timeRemaining = 99999, //unsure if necessary
                noteType = noteType
            };

            if (player.State.pointsOfInterest == null)
                player.State.pointsOfInterest = Pool.GetList<MapNote>();

            player.State.pointsOfInterest.Add(mapN);
            player.DirtyPlayerState();
            player.SendMarkersToClient();
            player.TeamUpdate();

            if (!canBeRemoved)
                _unremovableMapNotes.Add(mapN);

            return mapN;
        }

        private readonly HashSet<BaseEntity> _temporaryEntities = new();

        private BaseEntity CreateTemporaryEntity(string strPrefab, Vector3 pos = default, Quaternion rot = default, bool startActive = true, ulong ownerId = 0)
        {

            var ent = GameManager.server.CreateEntity(strPrefab, pos, rot, startActive);
            ent.OwnerID = ownerId;

            _temporaryEntities.Add(ent);

            return ent;
        }

        public class HarborPickupArea
        {
            public MonumentInfo Harbor2 { get; private set; }

            public ModularCarGarage CarLift { get; set; }

            public HarborPickupArea(MonumentInfo harbor, ModularCarGarage lift = null)
            {
                Harbor2 = harbor;
                CarLift = lift;
            }

        }

        public HarborPickupArea HarborArea { get; set; }

        private void InitializeHarbor2Area()
        {

            if (HarborArea != null)
            {
                Interface.Oxide.LogWarning(nameof(InitializeHarbor2Area) + " despite already being not null!?");
                return;
            }

            var lightSpawnPos = GetHarbor2TruckLightSpawnPoint();

            var light = CreateTemporaryEntity("assets/bundled/prefabs/modding/cinematic/cinelights/cinelight_point_green.prefab", lightSpawnPos, ownerId: 528491) as CinematicEntity;
            light.Spawn();
            light.EnableSaving(false);


            var carLiftSpawnPoint = GetHarbor2TruckSpawnPoint();

            var rot = GetIntendedRotationFromAngles(Harbor2.transform.rotation, new Vector3(1.23f, 90.16f, 0f));

            var lift = CreateTemporaryEntity("assets/bundled/prefabs/static/modularcarlift.static.prefab", carLiftSpawnPoint, rot, ownerId: 528491) as ModularCarGarage;

            lift.Spawn();
            lift.EnableSaving(false);

            HarborArea = new HarborPickupArea(Harbor2, lift);

        }

        private void OnServerInitialized()
        {
            var watch = Pool.Get<Stopwatch>();

            try 
            {
                watch.Restart();

                Subscribe(nameof(OnEntityKill));

                _deliveryData = Interface.Oxide.DataFileSystem.ReadObject<MonumentDeliveryLocations>(nameof(DeliveryJobs));
                _deliveryData.OnLoaded();

                InitializeHarbor2Area();

                InitializeDefaultDeliveryLocations();
            }
            finally
            {
                try { PrintWarning(nameof(OnServerInitialized) + " took: " + watch.Elapsed.TotalMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }

        
        }

        private void InitializeDefaultDeliveryLocations()
        {
            if (_deliveryData == null)
            {
                PrintError(nameof(_deliveryData) + " is null on: " + nameof(InitializeDefaultDeliveryLocations) + "?!");
                return;
            }

            if (_deliveryData._monumentNameDeliveryLocations.Count > 0)
            {
                PrintWarning("count > 0 already, return");
                return;
            }


            PrintWarning("init default placeholder temporary locations!");

            _deliveryData.AddDeliveryLocationByMonument(FindMonumentInfo("airfield_1"), new Vector3(0, 0, 0));
            _deliveryData.AddDeliveryLocationByMonument(FindMonumentInfo("sphere_tank"), new Vector3(5, 2, 0));
            _deliveryData.AddDeliveryLocationByMonument(FindMonumentInfo("sphere_tank"), new Vector3(8, 4, 0));

        }


        private readonly Dictionary<BasePlayer, string> _activeEditMonument = new();
        private readonly Dictionary<BasePlayer, DeliveryLocation> _activeEditLocation = new();

        private void CmdDeliveryJobEdit(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin)
                return;

            var player = iPlayer?.Object as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead())
            {
                SendReply(player, "This command must be run in-game.");
                return;
            }

            var arg0 = args.Length > 0 ? args[0] : string.Empty;

            if (arg0.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                _activeEditMonument.Remove(player);
                _activeEditLocation.Remove(player);
                SendReply(player, "Stopped editing.");
                return;
            }

            if (_deliveryData?._monumentNameDeliveryLocations == null)
            {
                SendReply(player, "_monumentNameDeliveryLocations is null?!?!");

                return;
            }

            if (args.Length > 0 && args[0].Equals("delete", StringComparison.OrdinalIgnoreCase) && _activeEditMonument.TryGetValue(player, out var curMon) && _activeEditLocation.TryGetValue(player, out var curLoc))
            {

                var didRem = _deliveryData.RemoveDeliveryLocation(curMon, curLoc);

                SendReply(player, "didRem: " + didRem);

                return;
            }

            var infoKvp = _deliveryData._monumentNameDeliveryLocations;

            var gotFromKvp = false;

            if (!_activeEditMonument.TryGetValue(player, out string monumentName))
            {
                if (args.Length < 1)
                {
                    SendReply(player, "Select monument to edit via /" + command + " <monument shortname or list index> or create new one with /djadd");
                    return;
                }
                else
                {
                    SendReply(player, "No edit selected... finding by args");
                    monumentName = GetMonumentShortPrefabName(FindMonumentInfo(arg0)?.name);
                }
            }
            else gotFromKvp = true;

            if (string.IsNullOrWhiteSpace(monumentName))
            {
                SendReply(player, "Could not find proper monument short name from: " + arg0);
                return;
            }
            else if (!gotFromKvp)
            {
                SendReply(player, "Found monumentName. It is: " + monumentName);

                _activeEditMonument[player] = monumentName;

                SendReply(player, "Now editing: " + monumentName + "\nRun again to select index, i.e: /" + command + " 2 - to select the 2nd list index.");
                return;
            }

            var monument = FindMonumentInfo(monumentName);
            if (monument == null)
            {
                SendReply(player, "monument somehow null?!?!?! for: " + monumentName);
                return;
            }

            if (!_deliveryData._monumentNameDeliveryLocations.TryGetValue(monumentName, out var deliveryLocations))
            {
                SendReply(player, "Couldn't get delivery locations from monumentName: " + monumentName);
                return;
            }

            if (!_activeEditLocation.TryGetValue(player, out var loc))
            {
                if (!int.TryParse(arg0, out var index))
                {
                    SendReply(player, "You must specify an index to edit in the delivery list.");
                    return;
                }

                if (index > (deliveryLocations.Count - 1) || index < 0)
                {
                    SendReply(player, "Bad index!: " + index);
                    return;
                }


                _activeEditLocation[player] = loc = deliveryLocations[index];

                SendReply(player, "Selected: " + index + " (" + loc + ")");

                return;
            }

            if (loc == null)
            {
                SendReply(player, "Somehow loc is null!!!");
                return;
            }

            var locProp = loc.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (args.Length < 1)
            {
                var propSb = Pool.Get<StringBuilder>();

                try
                {
                    propSb.Clear().Append("Properties:").Append(Environment.NewLine);


                    for (int i = 0; i < locProp.Length; i++)
                    {
                        var locProperty = locProp[i];

                        var isReadOnly = !locProperty.CanWrite;
                        var isPrivate = isReadOnly || (locProperty.GetSetMethod(true)?.IsPrivate ?? false);

                        PrintWarning(locProperty.Name + " setmethod private?: " + (locProperty?.GetSetMethod()?.IsPrivate ?? false));

                        var val = locProperty.GetValue(loc);

                        //todo: more appealing # colors
                        propSb.Append("<color=orange>").Append(locProperty.Name).Append("</color> (").Append(locProperty.PropertyType).Replace("System.", string.Empty).Append("): <color=yellow>").Append(val).Append("</color>").Append(isReadOnly ? " (Read Only)" : string.Empty).Append(isPrivate ? " (Private Set)" : string.Empty).Append(Environment.NewLine);
                    }

                    propSb.Length--;

                    SendReply(player, propSb.ToString());

                }
                finally { Pool.Free(ref propSb); }

                return;
            }

            PropertyInfo prop = null;

            for (int i = 0; i < locProp.Length; i++)
            {
                var legProp = locProp[i];

                if (legProp.Name.IndexOf(arg0, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (prop != null)
                    {
                        SendReply(player, "Found multiple matches for: " + arg0);
                        return;
                    }

                    prop = legProp;
                }

            }

            if (prop == null)
            {
                SendReply(player, "No property found by: " + arg0);
                return;
            }

            var propertyValue = prop.GetValue(loc);

            var isDict = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>);

            if (isDict)
            {
                //optimize below?
                _ = prop.PropertyType.GetGenericArguments()[0];
                _ = prop.PropertyType.GetGenericArguments()[1];

                var sb = Pool.Get<StringBuilder>();

                try
                {

                    var itemProp = prop.PropertyType.GetProperty("Item");
                    var keys = (IEnumerable<object>)prop.PropertyType.GetProperty("Keys").GetValue(prop.GetValue(loc));

                    sb.Clear().Append(prop.Name).Append(" contents:").Append(Environment.NewLine);

                    foreach (var key in keys)
                    {
                        var value = itemProp.GetValue(propertyValue, new[] { key });

                        sb.Append(key).Append(" (").Append(key.GetType()).Append("): ").Append(value).Append(" (").Append(value.GetType()).Append(")").Append(Environment.NewLine);
                    }


                    if (sb.Length > 1)
                        sb.Length--;

                    SendReply(player, sb.ToString());
                    PrintWarning(sb.ToString());


                    //  var values = prop.PropertyType.GetProperty("Values").GetValue(prop.GetValue(leg));


                }
                finally { Pool.Free(ref sb); }

            }

            if (args.Length < 2)
            {
                SendReply(player, "No edit arg specified. Property is: " + prop.Name + " value: " + prop.GetValue(loc));
                return;
            }

            if (isDict)
            {
                SendReply(player, "dict not handled yet");
                throw new NotImplementedException();
            }


            var arg1 = args[1];
            object convertedType;
            try
            {
                if (int.TryParse(arg1, out int argInt) && prop.PropertyType.IsEnum)
                    convertedType = Enum.ToObject(prop.PropertyType, argInt);
                else if (prop.PropertyType == typeof(Vector3))
                    convertedType = arg1.Equals("here", StringComparison.OrdinalIgnoreCase) ? player.transform.position : arg1.Equals("local", StringComparison.OrdinalIgnoreCase) ? InverseTransformPoint(monument.transform, player.transform.position) : GetVector3FromString(arg1);
                else if (prop.PropertyType == typeof(TimeSpan))
                    convertedType = TimeSpan.Parse(arg1);
                else
                    convertedType = Convert.ChangeType(arg1, prop.PropertyType);
            }
            catch (Exception ex)
            {
                SendReply(player, "Exception: " + ex.Message + " (" + ex.GetType() + ")");
                SendReply(player, "Couldn't convert " + arg1 + " to type: " + prop.PropertyType);
                throw ex;
            }

            prop.SetValue(loc, convertedType);

            //we do the thing below to get it to show bitwise'd enums properly in string, instead of, for example, "3".
            var typeStr = prop.PropertyType.IsEnum ? ((Enum)convertedType).ToString() : convertedType.ToString();

            SendReply(player, "Set " + prop.Name + " to: " + typeStr);
        }

        private MonumentInfo GetMonumentInBoundsOf(Vector3 pos)
        {
            for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
            {
                var monument = TerrainMeta.Path.Monuments[i];

                if (monument.IsInBounds(pos))
                    return monument;

            }

            return null;
        }

        private void CmdDeliveryJobAdd(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin)
                return;

            var player = iPlayer?.Object as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead())
            {
                SendReply(player, "This command must be run in-game.");
                return;
            }

            var arg0 = args.Length > 0 ? args[0] : string.Empty;
            var arg1 = args.Length > 1 ? args[1] : string.Empty;

            if (_deliveryData?._monumentNameDeliveryLocations == null)
            {
                SendReply(player, "_monumentNameDeliveryLocations is null?!?!");

                return;
            }

            var monumentInBounds = GetMonumentInBoundsOf(player.transform.position);
            if (monumentInBounds == null)
            {
                SendReply(player, "You are not in bounds of a monument.");
                return;
            }

            var monumentName = GetMonumentShortPrefabName(monumentInBounds.name);

            if (string.IsNullOrWhiteSpace(monumentName))
            {
                SendReply(player, "You were in bounds of a monument, but we couldn't get its short name?!: " + monumentInBounds?.name);
                return;
            }

            if (!_deliveryData._monumentNameDeliveryLocations.TryGetValue(monumentName, out var deliveryLocations))
            {
                deliveryLocations = _deliveryData._monumentNameDeliveryLocations[monumentName] = new List<DeliveryLocation>();

                SendReply(player, "Had to create kvp for " + monumentName);
                //SendReply(player, "Couldn't get delivery locations from monumentName: " + monumentName);
            }
            else SendReply(player, "Already had kvp for " + monumentName);

            var inversePoint = InverseTransformPoint(monumentInBounds.transform, player.transform.position);

            SendReply(player, "Local Position: " + inversePoint);

            var cargoTypes = DeliveryJob.CargoType.Undefined;
            var deliveryTypes = DeliveryJob.DeliveryTier.Undefined;

            if (int.TryParse(arg0, out var result))
                cargoTypes = (DeliveryJob.CargoType)result;

            if (int.TryParse(arg1, out result))
                deliveryTypes = (DeliveryJob.DeliveryTier)result;

            var newLoc = new DeliveryLocation(monumentInBounds, inversePoint, cargoTypes, deliveryTypes);
            deliveryLocations.Add(newLoc);

            SendReply(player, "Added new delivery loc " + newLoc);

            //below unnecessary?
            _deliveryData._monumentNameDeliveryLocations[monumentName] = deliveryLocations;

        }

        public static Vector3 GetVector3FromString(string vectorStr)
        {
            if (string.IsNullOrWhiteSpace(vectorStr))
                throw new ArgumentNullException(nameof(vectorStr));

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var split = sb.Clear().Append(vectorStr).Replace("(", string.Empty).Replace(")", string.Empty).ToString().Split(',');
                return new Vector3(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]));
            }
            finally { Pool.Free(ref sb); }
        }


        [ChatCommand("deljobs")]
        private void CmdDelJobs(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
                return;

            _availableJobs?.Clear();

            SendReply(player, "Cleared available jobs.");

        }


        [ChatCommand("monrottest")]
        private void CmdMonRotTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
                return;


            if (!Harbor2.IsInBounds(player.transform.position))
            {
                SendReply(player, "You must be in bounds of harbor2!");
                return;
            }

            var lookAt = GetLookAtEntity(player, 10f);
            if (lookAt == null)
            {
                SendReply(player, "No look at entity!");
                return;
            }

            var angles = LocalRotation(player.viewAngles, Harbor2.transform.rotation.eulerAngles);

            var intendedPlayerAngles = GetIntendedRotationFromAngles(Harbor2.transform.rotation, angles);

            var inv = InverseRotationAngles(Harbor2.transform, lookAt.transform);
            var fromInv = GetIntendedRotationFromAngles(Harbor2.transform.rotation, inv);
            var intendedRotation = Harbor2.transform.rotation * Quaternion.Euler(inv);

            //inverse rotation angles. first: monument. second: whatever entity you're interacting with i guess

            //RotationAngles = (Quaternion.Inverse(monument.Rotation) * spawnPoint.transform.rotation).eulerAngles,
            //public Quaternion IntendedRotation => Monument.Rotation * Quaternion.Euler(TransformData.RotationAngles);
            //RotationAngles = (Quaternion.Inverse(spawnPointAdapter.Monument.Rotation) * moveEntityTransform.rotation).eulerAngles;

            SendReply(player, nameof(angles) + ": " + angles + " " + nameof(intendedPlayerAngles) + ": " + intendedPlayerAngles + "\n" + nameof(inv) + ": " + inv + ", " + nameof(fromInv) + ": " + fromInv + "\n" + nameof(intendedRotation) + ": " + intendedRotation);
            SendReply(player, "lookAt rotation: " + lookAt.transform.rotation + " would be set to: " + intendedRotation);

            if (args.Length > 0 && args[0].Equals("set"))
            {
                if (args.Length > 1 && args[1].Equals("test"))
                {
                    intendedRotation = Quaternion.identity;
                    SendReply(player, "forced intended to: " + intendedRotation);
                }

                SendReply(player, "found set! trying to set to intended (no change SHOULD happen if this works).");
                lookAt.transform.rotation = intendedRotation;

                lookAt.OnPositionalNetworkUpdate();
                lookAt.UpdateNetworkGroup();
                lookAt.SendNetworkUpdateImmediate();

                SendReply(player, "set.");
            }

        }

        [ChatCommand("monpostest")]
        private void CmdMonPosTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
                return;

            SendReply(player, "Finding harbor_2 and giving you a pos lol");



            MonumentInfo monument = null;

            for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
            {
                var mon = TerrainMeta.Path.Monuments[i];

                if (mon.IsInBounds(player.transform.position))
                {
                    PrintWarning(player + " is in bounds of: " + mon?.name);

                    monument = mon;
                    break;
                }

            }

            if (monument == null)
            {
                SendReply(player, "you're evidently not in the bounds of a monument");
                return;
            }

            var monPos = monument.transform.position;
            var monRot = monument.transform.rotation;

            var localPos = new Vector3(-23, 5, -10);

            var transformPoint = TransformPoint(monument.transform, localPos);  //monPos + monRot * localPos;

            var currentLocalPos = InverseTransformPoint(monument.transform, player.transform.position); //Quaternion.Inverse(monRot) * (player.transform.position - monPos);

            SendReply(player, nameof(transformPoint) + " is: " +  transformPoint + "\n" + "Your current local pos in relation to this harbor is: " + currentLocalPos);

        }

        private static Vector3 TransformPoint(Transform transform, Vector3 localPosition)
        {
            return transform.position + transform.rotation * localPosition;
        }

        private static Vector3 InverseTransformPoint(Transform transform, Vector3 worldPosition)
        {
            return Quaternion.Inverse(transform.rotation) * (worldPosition - transform.position);
        }

        private static float LocalRotationAngle(Vector3 eulerAngles1, Vector3 eulerAngles2)
        {
            return eulerAngles1.y - eulerAngles2.y;
        }

        private static Vector3 LocalRotation(Vector3 eulerAngles1, Vector3 eulerAngles2)
        {
            var localRotationAngle = LocalRotationAngle(eulerAngles1, eulerAngles2);

            return new Vector3(0, (localRotationAngle + 360) % 360, 0);
        }

        private static Vector3 InverseRotationAngles(Transform transform1, Transform transform2)
        {
            return (Quaternion.Inverse(transform1.rotation) * transform2.rotation).eulerAngles;
        }

        public static Quaternion GetIntendedRotationFromAngles(Quaternion rotation, Vector3 rotationAngles)
        {
            return rotation * Quaternion.Euler(rotationAngles);
        }

        private Vector3 _harbor2TruckSpawnLocal = new(-16.32f, 5.00f, -9.95f);

        private Vector3 GetHarbor2TruckSpawnPoint()
        {

            if (Harbor2 == null)
            {
                PrintError(nameof(Harbor2) + " is null!!!");
                return Vector3.zero;
            }


            return TransformPoint(Harbor2.transform, _harbor2TruckSpawnLocal);
        }

        private Vector3 GetHarbor2TruckLightSpawnPoint()
        {
            if (Harbor2 == null)
            {
                PrintError(nameof(Harbor2) + " is null!!!");
                return Vector3.zero;
            }

            var desiredPos = new Vector3(_harbor2TruckSpawnLocal.x, _harbor2TruckSpawnLocal.y + 2.75f, _harbor2TruckSpawnLocal.z);


            return TransformPoint(Harbor2.transform, desiredPos);
        }

        private void GenerateRandomDeliveryJobsNoAlloc(ref HashSet<DeliveryJob> jobs, int numberOfJobs = 1)
        {
            if (jobs == null)
                throw new ArgumentNullException(nameof(jobs));

            if (numberOfJobs < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfJobs));

            for (int i = 0; i < numberOfJobs; i++)
                jobs.Add(GenerateRandomDeliveryJob());


        }

        private readonly HashSet<DeliveryJob> _availableJobs = new();

        public static T GetRandomEnumOption<T>(T combinedValue) where T : Enum
        {

            //todo: cleanup? is it possible?

            var result = Pool.GetList<object>();
            try
            {
                var intVal = Convert.ToInt32(combinedValue);

                foreach (var r in Enum.GetValues(typeof(T)))
                    if ((intVal & (int)r) != 0)
                        result.Add(r);
                

                return result.Count < 1 ? default : (T)result[Random.Range(0, result.Count)];
            }
            finally { Pool.FreeList(ref result); }
        }

        private DeliveryJob GenerateRandomDeliveryJob()
        {
            var keyCount = _deliveryData._monumentNameDeliveryLocations.Keys.Count; //different than .Count? idk.

            var keyRng = Random.Range(0, keyCount);

            var index = 0;

            var desiredKey = string.Empty;

            foreach (var key in _deliveryData._monumentNameDeliveryLocations.Keys)
            {
                if (index == keyRng)
                {
                    desiredKey = key;
                    break;
                }
                
                    index++;
            }

            if (string.IsNullOrWhiteSpace(desiredKey))
            {
                PrintError(nameof(desiredKey) + " couldn't find key on rng!??!?!");
                return null;
            }

            if (!_deliveryData._monumentNameDeliveryLocations.TryGetValue(desiredKey, out var deliveryLocations))
            {
                PrintError("couldn't get list of locations from desiredKey!!!: " + desiredKey);
                return null;
            }

            if (deliveryLocations.Count < 1)
            {
                PrintError(nameof(deliveryLocations) + " has count < 1!!!!");
                return null;
            }

            var rngLoc = deliveryLocations[Random.Range(0, deliveryLocations.Count)];

            return new DeliveryJob(GetRandomEnumOption(rngLoc.DeliveryTiers), GetRandomEnumOption(rngLoc.CargoTypes), rngLoc.WorldPosition, rngLoc);
        }

        public enum MapIcon { Default, DollarSign, Home, Airdrop, Crosshair, Shield, Skull, SleepingPerson, SleepingZZZ, Gun, Ore, Chest };
        public enum MapColor { Default, Yellow, Blue, Green, Red, Purple, Teal };

        private class DeliveryLocation
        {

            /// <summary>
            /// Bitwise OR to support multiple types - will pick randomly.
            /// </summary>
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DeliveryJob.CargoType CargoTypes { get; set; } = DeliveryJob.CargoType.Undefined; //use bitwise to support random types

            /// <summary>
            /// Bitwise OR to support multiple tiers - will pick randomly.
            /// </summary>
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DeliveryJob.DeliveryTier DeliveryTiers { get; set; } = DeliveryJob.DeliveryTier.Undefined; //use bitwise to support random types

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public TimeSpan TimeLimit { get; set; } = default;

            // [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            // public int CurrencyRewardMin { get; set; } = 0;

            //[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            //  public int CurrencyRewardMax { get; set; } = 0;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int CurrencyReward { get; set; } = 0;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public MapIcon MapIcon { get; set; } = MapIcon.DollarSign;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public MapColor MapColor { get; set; } = MapColor.Default;

            /*/
            public int GetRandomCurrencyAmount()
            {
                return Random.Range(CurrencyRewardMin, CurrencyRewardMax);
            }/*/

            private MonumentInfo _monument = null;

            [JsonIgnore]
            public string MonumentShortPrefabName { get; private set; } = string.Empty;


            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public Vector3 LocalPosition { get; set; }

            [JsonIgnore]
            public Vector3 WorldPosition => GetWorldPosition();

            public Vector3 GetWorldPosition()
            {
                var mon = GetMonument();
                if (mon?.transform == null)
                {
                    Interface.Oxide.LogError(nameof(GetWorldPosition) + " is null mon/transform!");
                    return Vector3.zero;
                }

                return TransformPoint(mon.transform, LocalPosition);
            }

            public MonumentInfo GetMonument()
            {
                if (_monument == null && !string.IsNullOrWhiteSpace(MonumentShortPrefabName))
                    _monument = FindMonumentInfo(MonumentShortPrefabName);

                if (_monument == null)
                    Interface.Oxide.LogWarning("ran search for monument, but still null. short prefab: " + MonumentShortPrefabName);

                return _monument;
            }

            public void SetMonument(MonumentInfo monument)
            {
                if (monument == null)
                    throw new ArgumentNullException(nameof(monument));


                MonumentShortPrefabName = GetMonumentShortPrefabName(monument.name);
                _monument = monument;

                if (string.IsNullOrWhiteSpace(MonumentShortPrefabName))
                    Interface.Oxide.LogError(nameof(MonumentShortPrefabName) + " is null/empty after monument set?!:");
            }

            public DeliveryLocation() { }

            public DeliveryLocation(MonumentInfo monument, Vector3 localPos, DeliveryJob.CargoType cargoTypes = DeliveryJob.CargoType.Undefined, DeliveryJob.DeliveryTier deliveryTiers = DeliveryJob.DeliveryTier.Undefined)
            {
                SetMonument(monument ?? throw new ArgumentNullException(nameof(monument)));
                LocalPosition = localPos;
                CargoTypes = cargoTypes;
                DeliveryTiers = deliveryTiers;
            }

            public DeliveryLocation(string monumentShortPrefabName, Vector3 localPos, DeliveryJob.CargoType cargoTypes = DeliveryJob.CargoType.Undefined, DeliveryJob.DeliveryTier deliveryTiers = DeliveryJob.DeliveryTier.Undefined)
            {
                if (string.IsNullOrWhiteSpace(monumentShortPrefabName))
                    throw new ArgumentNullException(nameof(monumentShortPrefabName));

                var findMon = FindMonumentInfo(monumentShortPrefabName);
                

                if (findMon == null)
                    Interface.Oxide.LogError(nameof(DeliveryLocation) + " init with: " + monumentShortPrefabName + " is null monument!!!");
                else SetMonument(findMon);

                LocalPosition = localPos;
                CargoTypes = cargoTypes;
                DeliveryTiers = deliveryTiers;
            }

            public override string ToString()
            {
                return MonumentShortPrefabName + "/" + LocalPosition + "/" + CargoTypes + "/" + DeliveryTiers;
            }

        }

        private class DeliveryJob
        {
            [Flags]
            public enum DeliveryTier
            {
                Undefined = 0,
                Low = 1,
                Medium = 2,
                High = 4,
                Extreme = 8
            }
            public DeliveryTier Tier { get; set; } = DeliveryTier.Undefined;

            [Flags]
            public enum CargoType //this has bitwise support but we don't actually make use of it anywhere else right now, so just keep that in mind.
            {
                Undefined = 0,
                Water = 1,
                SaltWater = 2,
                Gasoline = 4,
                Oil = 8,
                Diesel = 16,
                Solids = 32
            }

            public DeliveryLocation LocationDelivery { get; set; } = null;

            public CargoType TypeCargo { get; set; } = CargoType.Undefined;

            public Vector3 Destination { get; set; } = Vector3.zero;

            public bool Started { get; private set; } = false;

            public void SetStarted(bool state)
            {
                Started = state;
                TimeStarted = state ? DateTime.UtcNow : default;
            }

            public DateTime TimeStarted { get; private set; } = default;

            public static bool IsLiquidType(CargoType type)
            {
                return type switch
                {
                    CargoType.Water or CargoType.SaltWater or CargoType.Gasoline or CargoType.Diesel or CargoType.Oil => true,
                    _ => false,
                };
            }

            public DeliveryJob() { }

            //todo: change to use only deliverylocation
            public DeliveryJob(DeliveryTier tier, CargoType typeCargo, Vector3 destination, DeliveryLocation location)
            {
                Tier = tier;
                TypeCargo = typeCargo;
                Destination = destination;
                LocationDelivery = location;
            }

            public uint GetDesiredCargoModulePrefabID()
            {
                return IsLiquidType(TypeCargo) ? TANKER_MODULE_PREFAB_ID : STORAGE_MODULE_PREFAB_ID;
            }
        }

      


        //important todo: localized effects & map marker - do not send to all players (no effects yet, map marker (not generic) are localized).
        //todo: persistance? copy all fields into json on unload; use on reload if same car still exists
        private class ActiveDeliveryJob : FacepunchBehaviour
        {
            public ModularCar Car { get; private set; } = null;
            public HashSet<VehicleModuleStorage> CargoModules { get; set; } = new();
            public BasePlayer JobOwner { get; private set; } = null;

            public DeliveryJob Job { get; set; }

            public Vector3 CarPosition { get => Car?.transform?.position ?? Vector3.zero; }
            
            public float MaxDistanceFromDestinationToConsiderParked { get; set; } = 3.75f;

            /// <summary>
            /// Used to grab active job from a player; will always be same as JobOwner for that job.
            /// </summary>
            public static Dictionary<BasePlayer, ActiveDeliveryJob> PlayerToJob { get; set; } = new Dictionary<BasePlayer, ActiveDeliveryJob>();

            /// <summary>
            /// A list of all cars by netID that have an ActiveDeliveryJob component.
            /// </summary>
            public static HashSet<NetworkableId> DeliveryCars { get; set; } = new();

            public MapNote ParkingDestinationMarker { get; set; } = null;

            public MapMarkerGenericRadius CarMarker { get; set; } = null;

            public CinematicEntity ParkingLight { get; set; } = null;

            /// <summary>
            /// The amount of time remaining before the job is failed because time ran out.
            /// </summary>
            public TimeSpan TimeRemaining => Job.LocationDelivery.TimeLimit - (DateTime.UtcNow - Job.TimeStarted);

            public double TimeRemainingFraction => TimeRemaining.TotalSeconds <= 0 ? 0 : TimeRemaining.TotalSeconds / Job.LocationDelivery.TimeLimit.TotalSeconds;

            private readonly Dictionary<BaseVehicleModule, float> _moduleDamageTaken = new();

            private TimeCachedValue<float> _distanceFromDestination = null;

            public void OnModuleDamaged(BaseVehicleModule module, float amount)
            {
                Interface.Oxide.LogWarning(nameof(OnModuleDamaged) + ": " + module + " " + nameof(amount) + ": " + amount);

                if (!_moduleDamageTaken.TryGetValue(module, out _))
                    _moduleDamageTaken[module] = amount;
                else _moduleDamageTaken[module] += amount;
            }

            public float GetModuleDamage(BaseVehicleModule module)
            {
                return _moduleDamageTaken.TryGetValue(module, out var result) ? result :0f;
            }

            /// <summary>
            /// A sum of all damage taken by all modules on the car.
            /// </summary>
            public float GetTotalModuleDamage()
            {
                var sum = 0f;

                foreach(var kvp in _moduleDamageTaken)
                    sum += kvp.Value;

                return sum;
            }

            public float GetRewardDamagePenaltyPercentage()
            {
                var storageDmg = 0f;
                var nonStorageDmg = 0f;
                foreach (var kvp in _moduleDamageTaken)
                {
                    if (kvp.Key is VehicleModuleStorage modStorage && CargoModules.Contains(modStorage))
                        storageDmg += kvp.Value;
                    else nonStorageDmg += kvp.Value;
                }


                return storageDmg + (nonStorageDmg * 0.25f) * 0.033f;
            }

            public float GetDistanceFromDestination(bool skipCache = false)
            {
                _distanceFromDestination ??= new TimeCachedValue<float>()
                {
                    refreshCooldown = 2.75f,
                    refreshRandomRange = 0.5f,
                    updateValue = new Func<float>(() =>
                    {
                        return Vector3.Distance(CarPosition, Job.Destination);
                    })
                };

                return _distanceFromDestination.Get(skipCache);
            }

            private void InitializeCargoModule()
            {
                if (CargoModules.Count > 0)
                {
                    Interface.Oxide.LogWarning(nameof(InitializeCargoModule) + " called with CargoModules already populated?!");
                    return;
                }

                var isLiquid = DeliveryJob.IsLiquidType(Job.TypeCargo);

                Interface.Oxide.LogWarning(nameof(InitializeCargoModule) + " IsLiquid?: " + isLiquid);

                if (Car?.AttachedModuleEntities == null || Car.AttachedModuleEntities.Count < 1)
                {
                    Interface.Oxide.LogError("Car has no attached module entities?!?!?");
                    return;
                }

                var desiredModule = Job.GetDesiredCargoModulePrefabID();

                VehicleModuleStorage foundModule = null;

                for (int i = 0; i < Car.AttachedModuleEntities.Count; i++)
                {
                    var module = Car.AttachedModuleEntities[i] as VehicleModuleStorage;

                    if (module?.prefabID == desiredModule)
                    {
                        Interface.Oxide.LogWarning(" found desired cargo mod (" + desiredModule + " for type: " + Job.TypeCargo + ", setting.");

                        foundModule = module;
                        CargoModules.Add(module);

                        Interface.Oxide.LogWarning("set.");
                        break;
                    }

                }

                if (foundModule == null)
                {
                    Interface.Oxide.LogError("found no cargo module!!!! we really shouldn't initialize jobs with cars that do not have the correct type");
                    return;
                }

                if (DeliveryJob.IsLiquidType(Job.TypeCargo))
                    DesiredLiquidAmount = (int)Mathf.Clamp(GetDesiredLiquidItem().stackable * UnityEngine.Random.Range(0.125f, 0.375f), 1f, 200000);

                LoadCargoOntoModules();

                LockAllStorageModules(true);

   
            }

            public HashSet<Item> CargoItems { get; private set; } = new HashSet<Item>();

            private ItemDefinition _desiredLiquid = null;

            public ItemDefinition GetDesiredLiquidItem()
            {

                if (_desiredLiquid == null)
                {
                    var itemId = Job.TypeCargo switch
                    {
                        DeliveryJob.CargoType.Water => WATER_ITEM_ID,
                        DeliveryJob.CargoType.SaltWater => SALT_WATER_ITEM_ID,
                        DeliveryJob.CargoType.Gasoline => LGF_ITEM_ID,
                        DeliveryJob.CargoType.Oil => CRUDE_OIL_ITEM_ID,
                        DeliveryJob.CargoType.Diesel => DIESEL_ITEM_ID,
                        _ => 0,
                    };

                    _desiredLiquid = ItemManager.FindItemDefinition(itemId);
                }

                return _desiredLiquid;
            }

            public int DesiredLiquidAmount { get; set; } = 0;

            public void LoadCargoOntoModules()
            {
                if (CargoModules.Count < 1)
                {
                    Interface.Oxide.LogWarning(nameof(LoadCargoOntoModules) + " has no CargoModules!!!");
                    return;
                }

                var containers = Pool.GetList<IItemContainerEntity>();

                var isLiquid = DeliveryJob.IsLiquidType(Job.TypeCargo);

                try 
                {
                    GetContainersFromModulesNoAlloc(CargoModules, ref containers);

                    if (containers.Count < 1)
                    {
                        Interface.Oxide.LogWarning(nameof(LoadCargoOntoModules) + " has no containers!!!");
                        return;
                    }

                    for (int i = 0; i < containers.Count; i++)
                    {
                        var container = containers[i];

                        if (isLiquid)
                        {
                            if (DesiredLiquidAmount < 1)
                            {
                                Interface.Oxide.LogError(nameof(DesiredLiquidAmount) + " is < 1!!!");
                                return;
                            }

                            var liquid = GetDesiredLiquidItem();

                            if (liquid == null)
                            {
                                Interface.Oxide.LogError(nameof(liquid) + " is null!!!");
                                return;
                            }

                            var liquidItem = ItemManager.Create(liquid, DesiredLiquidAmount);

                            if (!MoveCargoItem(liquidItem, container.inventory))
                            {
                                liquidItem.Remove();


                                Interface.Oxide.LogError("failed to move liquid Item!!!");
                                return;
                            }
                        }
                        else
                        {
                            var cargoItems = Pool.GetList<Item>();
                            try 
                            {
                                GetRandomCargoNoAlloc(ref cargoItems, Job?.Tier ?? DeliveryJob.DeliveryTier.Undefined);

                                for (int j = 0; j < cargoItems.Count; j++)
                                {
                                    var cargoItem = cargoItems[j];

                                    if (cargoItem == null)
                                        continue;

                                    if (!MoveCargoItem(cargoItem, container.inventory))
                                    {
                                        cargoItem.Remove();
                                        Interface.Oxide.LogWarning("failed to move cargo item!: " + cargoItem);
                                    }


                                }

                            }
                            finally { Pool.FreeList(ref cargoItems); }
                        }

                    }

                }
                finally { Pool.FreeList(ref containers); }


               

            }

            public bool MoveCargoItem(Item item, ItemContainer container)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (container == null)
                    throw new ArgumentNullException(nameof(container));


                var val = item.MoveToContainer(container);

                if (val && !CargoItems.Add(item))
                    Interface.Oxide.LogWarning(nameof(MoveCargoItem) + " " + item + " was already a cargo item!! will return movetocontainer result anyway.");

                return val;

            }

            //todo: cargo item setup? a list of appliable cargo item spawn groups per tier. i suppose it should be per tier and the cargo type lol
            //may eventually need to change CargoType solely to "liquid" and "solid" - no more individual lgf, etc.
            //i.e: a field which is like _cargoItemsByTier[tier] = new List<CargoSpawn>();
            // is a list of CargoSpawn which contains the item def, skin to use when spawned, and the min-max amount, and (? ->) the weighted chance of this being picked.



            public void GetRandomCargoNoAlloc(ref List<Item> items, DeliveryJob.DeliveryTier tier = DeliveryJob.DeliveryTier.Undefined)
            {
                if (items == null)
                    throw new ArgumentNullException(nameof(items));

                if (tier == DeliveryJob.DeliveryTier.Undefined)
                    Interface.Oxide.LogWarning(nameof(GetRandomCargoNoAlloc) + " with Undefined tier");



            }

            /// <summary>
            /// Locks every single VehicleModuleStorage on the car, including engine bays.
            /// </summary>
            /// <param name="desiredState"></param>
            public void LockAllStorageModules(bool desiredState)
            {
                var containers = Pool.GetList<IItemContainerEntity>();

                try 
                {
                    GetAllContainersNoAlloc(ref containers);

                    for (int i = 0; i < containers.Count; i++)
                        containers[i]?.inventory?.SetLocked(desiredState);
                }
                finally { Pool.FreeList(ref containers); }
            }

            /// <summary>
            /// Locks only modules in CargoModules.
            /// </summary>
            /// <param name="desiredState"></param>
            public void LockAllCargoModules(bool desiredState)
            {
                foreach (var mod in CargoModules)
                    mod?.GetContainer()?.inventory?.SetLocked(desiredState);
            }

            public void SetJobOwner(BasePlayer jobOwner)
            {
                var prev = jobOwner;

                JobOwner = jobOwner;

                IOnJobOwnerSet(prev);
            }

            private void IOnJobOwnerSet(BasePlayer previousOwner = null)
            {
                Interface.Oxide.LogWarning(nameof(IOnJobOwnerSet));

                if (previousOwner != null && PlayerToJob.TryGetValue(previousOwner, out var job) && job == this)
                    PlayerToJob.Remove(previousOwner);

                if (JobOwner == null || JobOwner.IsDestroyed || !JobOwner.IsConnected)
                    return;

                if (JobOwner.IsDead())
                    Interface.Oxide.LogWarning("JobOwner was set to a dead player. this is acceptable as it won't break functionality, but why did you do it?");

             
                PlayerToJob[JobOwner] = this;

            }

            //todo: change to flat currency reward set by each job; not rng.
            //then, use damage penalty to reduce.
            public void GetRewardItemsNoAlloc(ref HashSet<Item> items)
            {
                var intTier = (int)Job.Tier;



                var penalty = GetRewardDamagePenaltyPercentage();


                var currency = Job.LocationDelivery.CurrencyReward;
                var batteriesAmount = (int)Mathf.Clamp(currency * (1 - penalty / 100f), 1, int.MaxValue);

                Interface.Oxide.LogWarning(nameof(GetRewardItemsNoAlloc) + " " + nameof(batteriesAmount) + ": " + batteriesAmount);


                items.Add(ItemManager.CreateByItemID(BATTERY_SMALL_ITEM_ID, batteriesAmount));

            }

            public void CompleteJob()
            {
                Interface.Oxide.LogWarning(nameof(CompleteJob));
                try 
                {
                    SendJobCompletedMessage(JobOwner);

                    var dmgTotal = GetTotalModuleDamage();
                    var penalty = GetRewardDamagePenaltyPercentage();

                    Interface.Oxide.LogWarning(nameof(dmgTotal) + ": " + dmgTotal + " " + nameof(penalty) + ": " + penalty);

                    if (JobOwner == null || !JobOwner.IsConnected)
                        return;

                    var rewardItems = Pool.Get<HashSet<Item>>();

                    try
                    {
                        rewardItems.Clear();

                        GetRewardItemsNoAlloc(ref rewardItems);

                        foreach (var item in rewardItems)
                            JobOwner.GiveItem(item);

                        //JobCompleteCelebration();

                    }
                    finally { Pool.Free(ref rewardItems); }

                }
                finally { IOnJobFinished(); }
                
             

            
            }

            public void JobCompleteCelebration()
            {
                throw new NotImplementedException();
            }

            public void GetCargoContainersNoAlloc(ref List<IItemContainerEntity> itemContainerEntities)
            {
                if (itemContainerEntities == null)
                    throw new ArgumentNullException(nameof(itemContainerEntities));

                if (Car?.AttachedModuleEntities == null)
                    return;

                for (int i = 0; i < Car.AttachedModuleEntities.Count; i++)
                {
                    var carMod = Car.AttachedModuleEntities[i];

                    if (carMod == null || !IsCargoModule(carMod.prefabID) || carMod is not VehicleModuleStorage modStorage)
                        continue;

                    itemContainerEntities.Add(modStorage.GetContainer());

                }
            }

            /// <summary>
            /// Returns every container (VehicleModuleStorage.GetContainer()) on the modular car, including engine bays.
            /// </summary>
            /// <param name="itemContainerEntities"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public void GetAllContainersNoAlloc(ref List<IItemContainerEntity> itemContainerEntities)
            {
                if (itemContainerEntities == null)
                    throw new ArgumentNullException(nameof(itemContainerEntities));

                if (Car?.AttachedModuleEntities == null)
                    return;

                for (int i = 0; i < Car.AttachedModuleEntities.Count; i++)
                {
                    var carMod = Car.AttachedModuleEntities[i];

                    if (carMod is VehicleModuleStorage modStorage)
                        itemContainerEntities.Add(modStorage.GetContainer());

                }

            }

            /// <summary>
            /// Returns all IItemContainerEntity containers from collection of VehicleModuleStorage
            /// </summary>
            /// <param name="storageModules"></param>
            /// <param name="itemContainerEntities"></param>
            public void GetContainersFromModulesNoAlloc(ICollection<VehicleModuleStorage> storageModules, ref List<IItemContainerEntity> itemContainerEntities)
            {
                foreach (var mod in storageModules)
                    itemContainerEntities.Add(mod.GetContainer());
            }

            public void EmptyCargoModulesLootContainers()
            {
                foreach (var module in CargoModules)
                {
                    var container = module?.GetContainer();
                    if (container == null)
                    {
                        Interface.Oxide.LogWarning(nameof(container) + " is null in loop");
                        continue;
                    }

                    //temp clear code? own clear may be cleaner
                    container.inventory.Clear();
                }
            }

            private void IOnJobFinished()
            {
                Interface.Oxide.LogWarning(nameof(IOnJobFinished));

                Interface.Oxide.LogWarning("job completely finished, internal method calling destroy now");

                DoDestroy();

            }

            public void TimeLimitFail()
            {
                //send player message about failure and what not.

                if (JobOwner != null && JobOwner.IsConnected)
                {
                    Instance?.ShowToast(JobOwner, "FAILED:\nJob could not be completed in time!", 1);
                    Instance?.SendLocalEffectOnPlayer(JobOwner, MISSION_FAILED_FX);
                }

                DoDestroy();
            }

            private IEnumerator CheckTimeLimit()
            {
                Interface.Oxide.LogWarning(nameof(CheckTimeLimit) + " init - will wait 1 second.");

                yield return CoroutineEx.waitForSecondsRealtime(1f);

                Interface.Oxide.LogWarning(nameof(CheckTimeLimit) + " waited 1 second.");

                if (Job?.LocationDelivery?.TimeLimit == default || Job?.LocationDelivery?.TimeLimit <= TimeSpan.Zero)
                {
                    Interface.Oxide.LogWarning(nameof(CheckTimeLimit) + " called with default/0 time, break!");
                    yield break;
                }



                while(true && Job?.LocationDelivery?.TimeLimit != default && Job?.LocationDelivery?.TimeLimit > TimeSpan.Zero)
                {
                    var secs = TimeRemaining.TotalSeconds;

                    if (secs <= 0)
                    {
                        Interface.Oxide.LogWarning(nameof(CheckTimeLimit) + " no time remaining! job failed due to time limit.");
                        TimeLimitFail();

                        yield break;
                    }

                    //CHANGE TO FRAC!!!
                    if (TimeRemainingFraction < 0.16 && JobOwner != null && JobOwner.IsConnected)
                        Instance?.ShowToast(JobOwner, GetTimeSpanFromMs(TimeRemaining.TotalMilliseconds, true) + " remaining!", 1); //int first to intentionally drop decimal first.
                    
                    //CHANGE TO FRAC!!!
                    if (TimeRemainingFraction <= 0.08)
                    {
                        
                        //job owner send sfx - dramatic countdown.
                    }

                    //todo:
                    //for specific "breakpoints", send player a toast to remind them of time remaining?
                    //when entering truck, show time limit each time, obviously especially important to show when first getting in.


                    yield return CoroutineEx.waitForSecondsRealtime(1f);
                }
               
            }

            public void SendJobCompletedMessage(BasePlayer player)
            {
                if (player == null || !player.IsConnected)
                    return;

                player.ChatMessage("Thanks for delivering the goods! Here's your reward:");
                //send fx! (here or via hook/elsewhere?)

            }

            public void SendJobStartedMessage(BasePlayer player)
            {
                if (player == null || !player.IsConnected)
                    return;

                player.ChatMessage("Welcome to your new delivery job! Deliver the goods. Receive reward. Less damage means better reward. Hooray!");
                Instance?.ShowToast(player, "Time to drive!");
                //send fx! (here or via hook/elsewhere?)

            }

            public void InitializeJob(DeliveryJob job, BasePlayer jobOwner = null) //DeliveryJob.CargoType cargoType, Vector3 dest, BasePlayer jobOwner = null
            {
                if (job == null)
                    throw new ArgumentNullException(nameof(job));
                
                Interface.Oxide.LogWarning(nameof(InitializeJob) + " with cargoType: " + job.TypeCargo);

                Job = job;
                job.SetStarted(true);

                InitializeCargoModule();

                SetJobOwner(jobOwner);

                SendJobStartedMessage(jobOwner);

                CreateCarMapMarker();

                SetDestination(job.Destination); //temp lol

                //created when destination is actually set, not here!!!
                //CreateDestinationMapMarker();

                InvokeRepeating(DoDestinationChecks, 2f, 2f);

            }

            public void SetDestination(Vector3 dest)
            {
                Interface.Oxide.LogWarning(nameof(SetDestination) + " " + dest);

                Job.Destination = dest;

                ParkingLight?.Kill();

                SpawnParkingLight(dest);

                if (ParkingDestinationMarker == null)
                    CreateDestinationMapMarker();
                else
                {
                    ParkingDestinationMarker.worldPosition = dest;

                    RefreshMarkersForAllSeated();
                }
             

            
            }

            public void CreateCarMapMarker()
            {
                if (CarMarker != null)
                {
                    Interface.Oxide.LogWarning(nameof(CreateCarMapMarker) + " called with CarMarker already existent?!");
                    return;
                }

                var newMap = (MapMarkerGenericRadius)GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", CarPosition);

                newMap.radius = 0.1f;
                newMap.color1 = new UnityEngine.Color(1f, 0f, 0f);
                newMap.color2 = new UnityEngine.Color(0f, 1f, 0f);
                newMap.alpha = 1f;

                newMap.Spawn();
                newMap.SendUpdate();

                newMap.SetParent(Car, true, true);

                CarMarker = newMap;

            }

            public void CreateDestinationMapMarker()
            {
                if (ParkingDestinationMarker != null)
                {
                    Interface.Oxide.LogWarning(nameof(CreateDestinationMapMarker) + " called with dest marker already existent?!");
                    return;
                }

                if (JobOwner == null || !JobOwner.IsConnected)
                {
                    Interface.Oxide.LogWarning(nameof(CreateDestinationMapMarker) + " called but no JobOwner!!!");
                    return;
                }

                if (JobOwner?.State?.pointsOfInterest == null)
                {
                    Interface.Oxide.LogWarning("pointsOfInterest is null!!!! - creating forcefully");

                    if (JobOwner?.State != null)
                        JobOwner.State.pointsOfInterest = Pool.GetList<MapNote>();
                }

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    try
                    {
                        var mapN = new MapNote
                        {
                            colourIndex = (int)Job.LocationDelivery.MapColor,
                            totalDuration = 1337f,
                            icon = (int)Job.LocationDelivery.MapIcon,
                            isPing = false,
                            worldPosition = Job.Destination,
                            label = sb.Clear().Append(Job.TypeCargo).Append(" Delivery").ToString(),
                            timeRemaining = 1337f, //unsure if necessary
                            noteType = 1
                        };

                        ParkingDestinationMarker = mapN;

                        JobOwner.State.pointsOfInterest.Add(mapN);
                        RefreshMarkersForOwner();
                    }
                    catch(Exception ex) { Interface.Oxide.LogError(nameof(CreateDestinationMapMarker) + ":\n" + ex); }
               
                }
                finally { Pool.Free(ref sb); }

              


            }

            public void EnsureAllMountedHaveMapMarker()
            {
                if (Car == null)
                    return;

                if (ParkingDestinationMarker == null)
                {
                    Interface.Oxide.LogWarning(nameof(EnsureAllMountedHaveMapMarker) + " has null " + nameof(ParkingDestinationMarker));
                    return;
                }
                
                var players = Pool.GetList<BasePlayer>();

                try 
                {
                    Car.GetMountedPlayers(players);

                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];

                        if (player == null || !player.IsConnected)
                            continue;

                        if (player?.State?.pointsOfInterest == null && player?.State != null)
                            player.State.pointsOfInterest = Pool.GetList<MapNote>();

                        if (!player.State.pointsOfInterest.Contains(ParkingDestinationMarker))
                        {
                            player.State.pointsOfInterest.Add(ParkingDestinationMarker);
                            RefreshMarkers(player);
                        }

                    }

                }
                finally { Pool.FreeList(ref players); }

            }


            private void KillDestinationMarker()
            {
                if (ParkingDestinationMarker == null)
                    return;

           

                foreach (var p in BasePlayer.allPlayerList)
                {
                    if (p?.State?.pointsOfInterest == null)
                        continue;

                    RemoveMarker(p, ParkingDestinationMarker);
                }

                if (ParkingDestinationMarker.ShouldPool)
                    ParkingDestinationMarker.Dispose();

                ParkingDestinationMarker = null;
            }

            public void RemoveDestinationMarkerForOwner()
            {
                if (ParkingDestinationMarker == null)
                {
                    Interface.Oxide.LogWarning(nameof(RemoveDestinationMarkerForOwner) + " called with null dest marker!!!");
                    return;
                }


                if (JobOwner?.State?.pointsOfInterest?.Remove(ParkingDestinationMarker) ?? false)
                    RefreshMarkersForOwner();

            }

            public void RemoveMarker(BasePlayer player, MapNote marker)
            {
                if (player == null)
                    throw new ArgumentNullException(nameof(player));
                
                if (marker == null)
                    throw new ArgumentNullException(nameof(marker));

                if (player?.State?.pointsOfInterest == null)
                    return;

                if (player.State.pointsOfInterest.Remove(ParkingDestinationMarker))
                    RefreshMarkers(player);

            }

            public void RefreshMarkersForAllSeated()
            {
                if (Car == null)
                    return;
                
                var players = Pool.GetList<BasePlayer>();
                
                try 
                {
                    Car.GetMountedPlayers(players);

                    for (int i = 0; i < players.Count; i++)
                        RefreshMarkers(players[i]);
                    

                }
                finally { Pool.FreeList(ref players); }
            }

            public void RefreshMarkersForOwner() => RefreshMarkers(JobOwner);

            public void RefreshMarkers(BasePlayer player)
            {
                if (player == null)
                    throw new ArgumentNullException(nameof(player));

                //not sure if this works yet to actually refresh or if new one must be added; test!

                if (!player.IsConnected)
                    return;
                

                player.DirtyPlayerState();
                player.SendMarkersToClient();
                player.TeamUpdate();
            }


            public void SpawnParkingLight(Vector3 position, bool fxEmitter = true)
            {
                if (ParkingLight != null)
                {
                    Interface.Oxide.LogWarning(nameof(SpawnParkingLight) + " called with parking ligth already existent?!");
                    return;
                }

                var light = GameManager.server.CreateEntity("assets/bundled/prefabs/modding/cinematic/cinelights/cinelight_point_red.prefab", position) as CinematicEntity;

                light.enableSaving = false;

                light.Spawn();

                light.EnableSaving(false);

                ParkingLight = light;

                if (JobOwner != null)
                    AddParkingFXEmitter();


            }

            private void AddParkingFXEmitter(bool addJobOwner = true)
            {
                if (GetParkingFXEmitter() != null)
                {
                    Interface.Oxide.LogWarning(nameof(AddParkingFXEmitter) + " called when an emitter already exists?!");
                    return;
                }

                if (addJobOwner && JobOwner == null)
                    Interface.Oxide.LogError(nameof(AddParkingFXEmitter) + " has " + nameof(addJobOwner) + " but job owner is null!!!");


                var emitter = ParkingLight.gameObject.AddComponent<EntityFXEmitter>();

                emitter.FX = FX_MISSING;
                emitter.FXToRunPerInterval = 3;
                emitter.PosLocal = new Vector3(0, 0.33f, 0);


                if (addJobOwner)
                    emitter.EmitToPlayers.Add(JobOwner);
            }

            public EntityFXEmitter GetParkingFXEmitter()
            {
                return ParkingLight?.GetComponent<EntityFXEmitter>();
            }

            private Coroutine _aliveCheckCoroutine = null;

            private Coroutine _timeLimitCoroutine = null;

            private IEnumerator AliveCheck() //this is actually necessary. a regular invoke will be stopped. when the car is killed, it doesn't seem to properly disappear. super strange.
            {           
                while (Car != null && Car?.gameObject != null && !(Car?.IsDestroyed ?? true) && !(Car?.IsDead() ?? true))
                    yield return CoroutineEx.waitForSecondsRealtime(3f);


                Interface.Oxide.LogWarning("car is dead on alive check. destroying self. how were we still alive?");
                DoDestroy();

                yield break;
            }

            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(ActiveDeliveryJob) + "." + nameof(Awake));

                Car = GetComponent<ModularCar>();

                if (Car == null || Car.IsDestroyed)
                {
                    Interface.Oxide.LogWarning("car null/destroyed!!!");

                    DoDestroy();

                    return;
                }

                _aliveCheckCoroutine = Instance?.StartCoroutine(AliveCheck());

                _timeLimitCoroutine = Instance?.StartCoroutine(CheckTimeLimit());

                Interface.Oxide.LogWarning(nameof(ActiveDeliveryJob) + "." + nameof(Awake) + " finished");

            }

            private void DoDestinationChecks()
            {
                var dist = GetDistanceFromDestination();

                Interface.Oxide.LogWarning(nameof(DoDestinationChecks) + " dist: " + dist + ", dets is: " + Job.Destination);

                if (dist > MaxDistanceFromDestinationToConsiderParked)
                    return;

                Interface.Oxide.LogWarning("Reached destination/in range!");

                CompleteJob();



            }

            public void CancelJob()
            {
                try 
                {
                    //sound fx for cancel

                    if (JobOwner != null && JobOwner.IsConnected)
                    {
                        JobOwner.ChatMessage("Job canceled!");
                        Instance.ShowToast(JobOwner, "Job canceled!", 1);

                    }

                }
                finally { IOnCancelJob(); }

             
            }

            private void IOnCancelJob()
            {
                try
                {
                    if (IsCompanyCar())
                        KillCar(10f);
                }
                catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
              


                DoDestroy();

            }

            public bool IsCompanyCar() => Car?.OwnerID == 1337;

            public void KillCar(float timeDelay = 0f)
            {
                if (timeDelay < 0f)
                    throw new ArgumentOutOfRangeException(nameof(timeDelay));

                if (timeDelay == 0f)
                    Car?.Kill();
                else Car?.Invoke(() => Car?.Kill(), timeDelay);
            }

            /// <summary>
            /// Destroys component cleanly and destroys markers, lights, empties cargo loot container, etc. Does not destroy car - use CancelJob instead or kill it yourself.
            /// </summary>
            public void DoDestroy()
            {
                try
                {
                    Interface.Oxide.LogWarning(nameof(ActiveDeliveryJob) + "." + nameof(DoDestroy));

                    try { if (Car?.net != null) DeliveryCars.Remove(Car.net.ID); }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { PlayerToJob.Remove(JobOwner); }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { Instance?.StopCoroutine(_timeLimitCoroutine); }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { Instance?.StopCoroutine(_aliveCheckCoroutine); }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { EmptyCargoModulesLootContainers(); }
                    catch (Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { LockAllStorageModules(false); }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { if (!(CarMarker?.IsDestroyed ?? true)) CarMarker?.Kill(); }
                    catch (Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { KillDestinationMarker(); }
                    catch (Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { if (!(ParkingLight?.IsDestroyed ?? true)) ParkingLight?.Kill(); }
                    catch (Exception ex) { Interface.Oxide.LogError(ex.ToString()); }

                    try { CancelInvoke(DoDestinationChecks); }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                }
                finally { Destroy(this); }

            }

        }

        private object CanAcceptItem(ItemContainer container, Item item, int targetPos)
        {

            
            if (item?.parent == null && container?.GetEntityOwner(true) is BaseLiquidVessel baseLiquidVessel)
            {
                var player = baseLiquidVessel?.GetOwnerPlayer();

                if (player != null && _invLocked.Contains(player.userID))
                {

                    if (player.IsConnected)
                        ShowToast(player, "Sorry, you're on the job!\nYou cannot take that right now.", 1);
;
                    return ItemContainer.CanAcceptResult.CannotAcceptRightNow;
                }

            }

            var owner = item?.parent?.GetEntityOwner();

            return (item?.parent?.IsLocked() ?? false && (owner is VehicleModuleStorage || owner?.GetParentEntity() is VehicleModuleStorage)) ? ItemContainer.CanAcceptResult.CannotAccept : null;
        }


        private Item GetRandomComponent(int amount = 1, bool enforceStackLimits = true)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var components = Pool.GetList<ItemDefinition>();

            try
            {

                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];

                    if (item.category == ItemCategory.Component && !_ignoreItems.Contains(item.itemid) && !item.shortname.Contains("vehicle.") && item.shortname != "glue" && item.shortname != "bleach" && item.shortname != "ducttape" && item.shortname != "sticks")
                        components.Add(item);
                }

                var itemInfo = components[Random.Range(0, components.Count)];
                var component = ItemManager.Create(itemInfo, 1);

                component.amount = Mathf.Clamp(amount, 1, enforceStackLimits ? component.MaxStackable() : int.MaxValue);

                return component;
            }
            finally { Pool.FreeList(ref components); }
        }

        private Vector3 SpreadVector(Vector3 vec, float spread = 1.5f) { return Quaternion.Euler(Random.Range((float)(-spread * 0.2), spread * 0.2f), Random.Range((float)(-spread * 0.2), spread * 0.2f), Random.Range((float)(-spread * 0.2), spread * 0.2f)) * vec; }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + Random.insideUnitSphere * spread; }

        private static readonly System.Random _categoryRng = new();

        private ItemCategory GetRandomCategory() { return (ItemCategory)_categoryRng.Next(0, 18); }

        private ItemCategory GetRandomCategory(params ItemCategory[] ignoreCategories)
        {
            var category = (ItemCategory)_categoryRng.Next(0, 18);
            if (!ignoreCategories.Any(p => p == category)) return category;

            var max = 400;
            var count = 0;

            while (ignoreCategories.Any(p => p == category))
            {
                count++;
                if (count >= max) break;
                category = (ItemCategory)_categoryRng.Next(0, 18);
            }

            return category;
        }

        private Item GetRandomItem(Rust.Rarity rarity, ItemCategory category = ItemCategory.Items, bool craftableOnly = true, bool researchableOnly = true)
        {
            Item item = null;

            var applicableItems = Pool.GetList<ItemDefinition>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var itemDefs = ItemManager.itemList[i];
                    if (itemDefs == null) continue;

                    if (_ignoreItems.Contains(itemDefs.itemid) || itemDefs.GetComponent<Rust.Modular.ItemModVehicleChassis>() != null)
                        continue;


                    applicableItems.Add(itemDefs);

                }

                var itemDef = applicableItems?.Where(p => p != null && p.rarity == rarity && p.category == category && (!craftableOnly || craftableOnly && (p?.Blueprint?.userCraftable ?? false)) && (!researchableOnly || (p?.Blueprint?.isResearchable ?? false) || (p?.Blueprint?.defaultBlueprint ?? false)))?.ToList()?.GetRandom() ?? null;
                if (itemDef != null) item = ItemManager.Create(itemDef, 1);

                return item;
            }
            finally { Pool.FreeList(ref applicableItems); }
        }

        private Item GetRandomBP(Rust.Rarity rarity, ItemCategory category = ItemCategory.Items)
        {
            var item = ItemManager.CreateByName("blueprintbase");

            item.blueprintTarget = ItemManager.itemList?.Where(p => p != null && p.rarity == rarity && p.category == category && !(p?.Blueprint?.defaultBlueprint ?? false) && (p?.Blueprint?.userCraftable ?? false) && (p?.Blueprint?.isResearchable ?? false))?.ToList()?.GetRandom()?.itemid ?? 0;

            if (item.blueprintTarget == 0)
            {
                PrintWarning("item.blueprint target 0!!!!");
                RemoveFromWorld(item);
                return null;
            }

            return item.blueprintTarget == 0 ? null : item;
        }

        private static readonly System.Random bpRng = new();

        private Item GetUnlearnedBP(Rust.Rarity rarity, BasePlayer player, ItemCategory category = ItemCategory.Items)
        {
            if (player == null || player.blueprints == null) return null;

            var list = ItemManager.itemList?.Where(p => p.rarity == rarity && p.category == category && !(p?.Blueprint?.defaultBlueprint ?? false) && (p?.Blueprint?.isResearchable ?? false) && (p?.Blueprint?.userCraftable ?? false) && !(player?.blueprints?.HasUnlocked(p) ?? false)) ?? null;
            if (list == null || !list.Any())
            {
                return null;
            }

            var item = ItemManager.CreateByName("blueprintbase");
            item.blueprintTarget = 0;



            var listCount = list?.Count() ?? 0;
            item.blueprintTarget = list.ElementAtOrDefault(bpRng.Next(0, listCount))?.itemid ?? 0;


            if (item.blueprintTarget == 0)
            {
                PrintWarning("item.blueprint target 0!!!!");
                RemoveFromWorld(item);
                return null;
            }

            return item.blueprintTarget == 0 ? null : item;
        }

        private bool HasRarity(Rust.Rarity rarity, ItemCategory category, bool onlyResearchable = false, bool ignoreDefault = false)
        {
            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                if (item == null) continue;
                if (item.category == category && item.rarity == rarity)
                {
                    if (!onlyResearchable || (item?.Blueprint?.isResearchable ?? false)) if (!ignoreDefault || !(item?.Blueprint?.defaultBlueprint ?? false)) return true;
                }
            }
            return false;
        }

        private Rust.Rarity GetHighestRarity(ItemCategory category, bool onlyResearchable = false)
        {
            try
            {
                if (onlyResearchable && !(ItemManager.itemList?.Any(p => p != null && p.category == category && (p?.Blueprint?.isResearchable ?? false)) ?? false)) return Rust.Rarity.None;
                else if (!onlyResearchable) if (!(ItemManager.itemList?.Any(p => p != null && p.category == category) ?? false)) return Rust.Rarity.None;
                return (!onlyResearchable ? ItemManager.itemList?.Where(p => p.category == category) : ItemManager.itemList?.Where(p => p.category == category && (p?.Blueprint?.isResearchable ?? false) && !(p?.Blueprint?.defaultBlueprint ?? false)))?.Select(p => p.rarity)?.Max() ?? Rust.Rarity.None;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintError("^GetHighestRarity^");
                return Rust.Rarity.None;
            }
        }

        private void SimpleBroadcast(string msg, ulong userID = 0)
        {
            if (string.IsNullOrEmpty(msg)) return;

            Chat.ChatEntry chatEntry = new()
            {
                Channel = Chat.ChatChannel.Server,
                Message = RemoveTags(msg),
                UserId = userID.ToString(),
                Time = Facepunch.Math.Epoch.Current
            };

            Facepunch.RCon.Broadcast(Facepunch.RCon.LogType.Chat, chatEntry);

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || !player.IsConnected) continue;

                if (userID > 0) player.SendConsoleCommand("chat.add", string.Empty, userID, msg);
                else SendReply(player, msg);
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

        
        private Rust.Rarity GetRandomRarity(ItemCategory category, bool onlyResearchable = false, params Rust.Rarity[] ignoreRarity)
        {
            var rarity = Rust.Rarity.None;
            var rarities = Pool.GetList<Rust.Rarity>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item == null) continue;

                    if (item.category == category)
                    {
                        if (rarities.Contains(item.rarity)) continue;
                        if (!onlyResearchable || ((item?.Blueprint?.isResearchable ?? false) && !(item?.Blueprint?.defaultBlueprint ?? false))) if (ignoreRarity == null || ignoreRarity.Length < 1 || !ignoreRarity.Any(p => p == item.rarity)) rarities.Add(item.rarity);
                    }
                }

                if (rarities != null && rarities.Count > 0)
                {
                    var rngRarity = rarities[_rarityRng.Next(0, rarities.Count)];
                    //   PrintWarning("rng rarity: " + rngRarity + ", has: " + HasRarity(rngRarity, category, true, true));
                    rarity = rngRarity;
                }

                return rarity;
            }
            finally { Pool.FreeList(ref rarities); }
        }

        private Rust.Rarity GetRandomRarityOrDefault(ItemCategory category, bool onlyResearchable = false, params Rust.Rarity[] ignoreRarity)
        {
            var rarity = Rust.Rarity.None;
            var rarities = Pool.GetList<Rust.Rarity>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item == null) continue;

                    if (item.category == category)
                    {
                        if (rarities.Contains(item.rarity)) continue;
                        if (!onlyResearchable || ((item?.Blueprint?.isResearchable ?? false) && !(item?.Blueprint?.defaultBlueprint ?? false))) if (ignoreRarity == null || ignoreRarity.Length < 1 || !ignoreRarity.Any(p => p == item.rarity)) rarities.Add(item.rarity);
                    }
                }
                if (rarities.Count > 0)
                {
                    var rngRarity = rarities[_rarityRng.Next(0, rarities.Count)];
                    //   PrintWarning("rng rarity: " + rngRarity + ", has: " + HasRarity(rngRarity, category, true, true));
                    rarity = rngRarity;
                }
                else
                {
                    var itemCats = Pool.GetList<ItemDefinition>();

                    for (int i = 0; i < ItemManager.itemList.Count; i++)
                    {
                        var item = ItemManager.itemList[i];
                        if (item.category == category) itemCats.Add(item);
                    }

                    if (itemCats.Count > 0)
                        rarity = itemCats?.Select(p => p.rarity)?.Min() ?? Rust.Rarity.None;
                    

                    Pool.FreeList(ref itemCats);
                }

                return rarity;
            }
            finally { Pool.FreeList(ref rarities); }
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

        private void BroadcastToast(string message, int type = 0)
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];

                if (p != null && p.IsConnected)
                    ShowToast(p, message, type);
            }
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private void UnlockCrate(HackableLockedCrate crate)
        {
            if (crate == null || crate.IsDestroyed) return;
            crate.SetFlag(BaseEntity.Flags.Reserved1, true);
            crate.SetFlag(BaseEntity.Flags.Reserved2, true);
            crate.isLootable = true;
            crate.CancelInvoke(new Action(crate.HackProgress));
        }

        private EntityFXEmitter GetOrCreateFXEmitter(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed) return null;

            return entity?.GetComponent<EntityFXEmitter>() ?? entity?.gameObject?.AddComponent<EntityFXEmitter>();
        }

        private class EntityFXEmitter : FacepunchBehaviour
        {
            public BaseEntity Entity { get; set; }

            public string FX { get; set; } = string.Empty;

            public Vector3 PosLocal { get; set; } = Vector3.zero;

            public Vector3 NormLocal { get; set; } = Vector3.zero;

            public Vector3 NormWorld { get; set; } = Vector3.zero;

            public uint BoneID { get; set; } = 0;

            public bool RunAtPosition { get; set; } = false;

            public float Interval { get; set; } = 1f;

            public int FXToRunPerInterval { get; set; } = 1;

            /// <summary>
            /// If HashSet contains any players, these are the only players in which the FX will be emitted to.
            /// </summary>
            public HashSet<BasePlayer> EmitToPlayers { get; set; } = new HashSet<BasePlayer>();

            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(EntityFXEmitter) + "." + nameof(Awake));

                Entity = GetComponent<BaseEntity>();

                if (Entity == null)
                {
                    Interface.Oxide.LogError("Crate null on Awake!!!");
                    DoDestroy();
                    return;
                }

                UpdateInvoke();

            }

            public void UpdateInvoke(float initialTime = 1f)
            {
                CancelInvoke(EmitFX);

                InvokeRepeating(EmitFX, initialTime, Interval);
            }

            public void EmitFX()
            {
                if (Entity == null || Entity.IsDestroyed)
                {
                    Interface.Oxide.LogWarning("Crate is null/destroyed, destroying self!!");

                    DoDestroy();

                    return;
                }

                if (string.IsNullOrWhiteSpace(FX))
                    return;

                for (int i = 0; i < FXToRunPerInterval; i++)
                {

                    if (EmitToPlayers.Count > 0)
                    {
                        foreach (var p in EmitToPlayers)
                        {
                            if (RunAtPosition)
                                Instance?.SendLocalEffect(p, FX, Entity.transform.position);
                            else Instance?.SendLocalEffect(p, Entity, FX, 1f, BoneID);
                        }
                    }
                    else
                    {
                        if (!RunAtPosition)
                            Effect.server.Run(FX, Entity, BoneID, PosLocal, NormLocal);
                        else Effect.server.Run(FX, Entity.transform.position, Vector3.zero);
                    }


                }
            }

            public void DoDestroy()
            {
                try { CancelInvoke(EmitFX); }
                finally { Destroy(this); }
            }
        }

        public T RandomElement<T>(IEnumerable<T> source, System.Random rng)
        {
            T current = default;
            int count = 0;
            foreach (T element in source)
            {
                count++;

                if (rng.Next(count) == 0)
                    current = element;
            }

            return count == 0 ? throw new InvalidOperationException("Sequence was empty") : current;
        }

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

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using var fx = new Effect(effect, pos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void SendLocalEffect(BasePlayer player, BaseEntity fxEntity, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using var fx = new Effect(effect, fxEntity, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void SendLocalEffectOnPlayer(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using var fx = new Effect(effect, player, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private bool HasAnyLooters(BaseEntity crate)
        {
            if (crate == null) return false;

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];

                if (player?.inventory?.loot?.entitySource == crate)
                    return true;
            }

            return false;
        }

        private void SaveDeliveryData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(nameof(DeliveryJobs), _deliveryData);
        }

        private readonly Dictionary<Vector3, MonumentInfo> _nearestMonCache = new();

        private MonumentInfo GetNearestMonument(Vector3 position)
        {
            if (_nearestMonCache.TryGetValue(position, out var monInfo))
                return monInfo;
            

            var pos = position;
            pos.y = 0;

            var monDist = Pool.Get<Dictionary<MonumentInfo, float>>();
            try
            {
                monDist.Clear();


                for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                {
                    var mon = TerrainMeta.Path.Monuments[i];
                    if (mon == null || mon.gameObject == null) continue;
                    var monP = mon?.transform?.position ?? Vector3.zero;
                    monP.y = 0;
                    monDist[mon] = Vector3.Distance(monP, pos);
                }

                MonumentInfo nearest = null;
                var lastDist = -1f;
                foreach (var kvp in monDist)
                {
                    var key = kvp.Key;
                    var val = kvp.Value;
                    if (lastDist < 0f || (val < lastDist))
                    {
                        lastDist = val;
                        nearest = key;
                    }
                }

                _nearestMonCache[position] = nearest;

                return nearest;
            }
            finally { Pool.Free(ref monDist); }
        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, float radius = 0f, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;

            RaycastHit hit;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, player.eyes.rotation * Vector3.forward);

            if (radius <= 0)
            {
                if (Physics.Raycast(ray, out hit, maxDist, coll))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }
            else
            {
                if (Physics.SphereCast(ray, radius, out hit, maxDist, coll))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }


            return null;
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

        [ChatCommand("jgt")]
        private void CmdJobGuiTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
                return;

            if (args.Length < 2)
            {
                SendReply(player, "need more args!");
                return;
            }

            var isMin = args[0].Equals("min", StringComparison.OrdinalIgnoreCase);
            var isMax = args[0].Equals("max", StringComparison.OrdinalIgnoreCase);

            if (!isMin && !isMax)
            {
                SendReply(player, "invalid args! use min/max.");
                return;
            }

            if (isMin)
                _debugImageAnchorMin[player.UserIDString] = args[1];
            else if (isMax)
                _debugImageAnchorMax[player.UserIDString] = args[1];

            SendReply(player, "set: " + (isMin ? "min" : "max") + " to: " + args[1]);

        }

        private readonly Dictionary<string, string> _debugImageAnchorMin = new();
        private readonly Dictionary<string, string> _debugImageAnchorMax = new();

        private void DeliveryJobsGUI_New(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;


            if (!_debugImageAnchorMin.TryGetValue(player.UserIDString, out var imageAnchorMin))
                return;

            if (!_debugImageAnchorMax.TryGetValue(player.UserIDString, out var imageAnchorMax))
                return;

            //var imageAnchorMin = "0.328 0.383"; //"0.432 0.319";
            //var imageAnchorMax = "0.543 0.633"; //"0.549 0.736";

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var mainUrl = @"https://cdn.prismrust.com/delivery_jobs/";
                var idUrl = string.Empty;

                var url = mainUrl + "osrs_gui_example.png"; //fix

                var selectCmd = string.Empty; //fix //sb.Clear().Append("class select ").Append(userClass).ToString();

                var buttonColors = "1 1 1 1";

                var GUISkinElement = new CuiElementContainer();

                var GUISkin = GUISkinElement.Add(new CuiPanel
                {
                    Image =
                {
                    Color = "0 0 0 0"
                },
                    RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                    CursorEnabled = true
                }, "Overlay", "ClassGUINew");

                GUISkinElement.Add(new CuiElement
                {
                    Name = "ImgUIImg",
                    Components =
                {
                    new CuiRawImageComponent
                    {
                     //   Color = config.ImageColor,
                        Url = url,
                        FadeIn = 0.4f
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = imageAnchorMin,
                        AnchorMax = imageAnchorMax
                    }
                },
                    Parent = GUISkin,

                });

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class gui_legacy",
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "Show More Info",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.432 0.25",
                    AnchorMax = "0.549 0.285"
                }
                }, GUISkin);

                //invisible button below to blend into image. a test.
                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class stop",
                        Color = "0 0 0 0"
                    },
                    Text =
                {
                    Text = string.Empty,
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "0 0 0 0"
                },
                    RectTransform =
                {
                    AnchorMin = "0.463 0.369",
                    AnchorMax = "0.541 0.406"
                }
                }, GUISkin);

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = selectCmd,
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "Select",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.432 0.285",
                    AnchorMax = "0.549 0.319"
                }
                }, GUISkin);

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class new next",
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "-->",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.549 0.319",
                    AnchorMax = "0.58 0.347"
                }
                }, GUISkin);

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class new prev",
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "<--",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.401 0.319",
                    AnchorMax = "0.432 0.347"
                }
                }, GUISkin);

                CuiHelper.DestroyUi(player, "ClassGUINew");


                CuiHelper.AddUi(player, GUISkinElement);

            }
            finally { Pool.Free(ref sb); }

            ShowToast(player, "No images? Use 'Show More Info' for text version.");

        }

        public static string GetTimeSpanFromMs(double ms, bool isShort = false)
        {
            if (ms < 1000)
                return "Less than a second";

            ms /= 1000;
            int seconds = (int)(ms % 60);
            ms /= 60;
            int minutes = (int)(ms % 60);
            ms /= 60;
            int hours = (int)(ms % 24);
            int days = (int)(ms / 24);
            int years = days / 365;
            days -= years * 365;

            var sb = Pool.Get<StringBuilder>();

            try
            {
                string year_s = sb.Clear().Append(years).Append(" year").Append(years != 1 ? "s" : "").ToString();
                string day_s = sb.Clear().Append(days).Append(" day").Append(days != 1 ? "s" : "").ToString();
                string hour_s = sb.Clear().Append(hours).Append(" hour").Append(hours != 1 ? "s" : "").ToString();
                string mins_s = sb.Clear().Append(minutes).Append(" minute").Append(minutes != 1 ? "s" : "").ToString();
                string sec_s = sb.Clear().Append(seconds).Append(" second").Append(seconds != 1 ? "s" : "").ToString();


                var arr = Pool.GetList<string>();
                try
                {
                    if (years > 0)
                        arr.Add(year_s);
                    if (days > 0)
                        arr.Add(day_s);
                    if (hours > 0)
                        arr.Add(hour_s);
                    if (minutes > 0)
                        arr.Add(mins_s);
                    if (seconds > 0 && (!isShort || arr.Count < 1))
                        arr.Add(sec_s);

                    if (arr.Count < 1)
                        return "Unknown";
                    if (arr.Count < 2)
                        return arr[0];

                    string last = arr[arr.Count - 1];
                    arr.RemoveAt(arr.Count - 1);

                    sb.Clear();

                    for (int i = 0; i < arr.Count; i++)
                    {
                        var ar = arr[i];

                        sb.Append(ar).Append(", ");

                    }

                    if (sb.Length > 2) //trim last 2 to trim ", "
                        sb.Length -= 2;

                    sb.Append(" and ").Append(last);

                    return sb.ToString();
                }
                finally { Pool.FreeList(ref arr); }


            }
            finally { Pool.Free(ref sb); ; }


        }

    }
}