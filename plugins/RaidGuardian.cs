using CompanionServer;
using Oxide.Core.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Oxide.Core;
using Pool = Facepunch.Pool;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Oxide.Plugins
{

    [Info("Raid Guardian", "Shady", "1.0.5")]
    internal class RaidGuardian : RustPlugin
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
        //needs to be updated to have more support for some non-buildingblock things, such as doors. perhaps check if door is connected to protected building (done)
        //little concern about decor items or furnaces/big walls. maybe can check if in tc range of protd building (done, ^ done - thankfully these things get assigned a building id lol)
        //don't stop all damage types (i.e decay, it currently stops decay) (done)

        //radius is all kinds of fucked for spheres currently at "fuck it" priority (resolved)

        //cancel start coroutine if the building is attacked, so it doesn't get protected mid-raid

        //send rust++ message when raid proteciton starts!!! (great idea imo) ("your building at X grid pos is now protected!") (done, sort of, no grid pos yet)

        //if cupboard time is < protection time, cap/clamp protection time to upkeep minutes left


        //all uses of linq are temporary because linq is BAD!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //update: linq use reduced, but not removed entirely yet = exists only in spheres

        [PluginReference]
        private readonly Plugin Clans;

        [PluginReference]
        private readonly Plugin NoEscape;

        [PluginReference]
        private readonly Plugin WipeInfo;

        private ProtectionData _protectionData = null;

        private readonly int _constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });

        private readonly HashSet<BuildingManager.Building> _protectedBuildings = new();

        private readonly Dictionary<uint, HashSet<SphereEntity>> _buildingIdToSpheres = new();

        private readonly Dictionary<string, float> _lastWarningToast = new();

        private readonly Dictionary<uint, Coroutine> _buildingIdToCountdownCoroutine = new();

        private readonly Dictionary<uint, Coroutine> _buildingIdToEndProtectionCoroutine = new();

        private readonly Dictionary<string, float> _lastPlaytimeOnDisconnect = new();

        private readonly Dictionary<uint, Coroutine> _buildingIdToRaidBlockCoroutine = new();

        private readonly HashSet<Coroutine> _coroutines = new();

        private readonly HashSet<BuildingManager.Building> _raidBlockedBuildings = new();

        private readonly Dictionary<ulong, Action> _rustPlusNotifyAction = new();

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private const string CHAT_STEAM_ID = "76561198865536053";

        private const string BUILD_TOO_CLOSE_PROTECTED_BUILDING = "You are attempting to build too close to a currently <i>Offline Raid Protected</i> building!";
        private const string BUILD_TOO_CLOSE_TOAST = "Too close to an <i>Offline Raid Protected</i> building!";

        private const string PROTECTED_BUILDING_TOAST = "<size=30><color=yellow>THIS BUILDING IS PROTECTED AS ITS OWNER IS OFFLINE.</color></size>";

        private readonly HashSet<string> _forbiddenTags = new(6)
        { "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>" };

        private void SpawnSpheresOnBuilding(BuildingManager.Building building, int sphereCount = 4, bool globalBroadcast = false)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));


            var blocks = building.decayEntities;

            if (blocks == null || blocks.Count < 1)
            {
                PrintWarning(nameof(SpawnSpheresOnBuilding) + " called for a non-buildingblock building: " + building?.ID);
                return;
            }

            var centerPos = Vector3.zero;

            var transforms = Pool.GetList<Transform>();

            try
            {

                for (int i = 0; i < blocks.Count; i++) //only checks foundations
                {
                    var block = blocks[i];

                    if (block == null || block.IsDestroyed || block.gameObject == null)
                        continue;

                    if (!block.ShortPrefabName.Contains("foundation")) //change to check IDs later - foundation check *may* be unnecessary, and not checking foundations may mean we can not adjust y manually? not sure
                        continue;

                    transforms.Add(block.transform);
                }

                if (transforms.Count < 1)
                {
                    PrintError("transforms.count < 1?!?!?!?!? on " + nameof(SpawnSpheresOnBuilding) + " building: " + building?.ID);
                    return;
                }

                centerPos = FindCenterOfTransforms(transforms);
                centerPos.y += 2f;

            }
            finally { Pool.FreeList(ref transforms); }


            //change to not use linq below. the code works, but pls no linq:
            var furthestDistance = blocks.Select(p => Vector3.Distance(p.transform.position, centerPos)).Max();

            var furthestBlock = blocks.Where(p => Vector3.Distance(p.transform.position, centerPos) == furthestDistance).FirstOrDefault();
            var furthestDist = Vector3.Distance(furthestBlock.transform.position, centerPos);
            var sphereRadius = furthestDistance * 2f + 4f; //7.5f - must be a few positive or else it doesn't cover entirely

            PrintWarning("set radius to furthestDistance * 2f + 4f: " + sphereRadius);

            if (_buildingIdToSpheres.TryGetValue(building.ID, out var spheres))
            {
                foreach (var sphere in spheres)
                {
                    if (sphere == null || sphere.IsDestroyed) continue;

                    sphere.Kill();
                }

                _buildingIdToSpheres[building.ID].Clear();
            }
            else _buildingIdToSpheres[building.ID] = new HashSet<SphereEntity>();



            for (int i = 0; i < sphereCount; i++)
            {
                var sphere = (SphereEntity)GameManager.server.CreateEntity("assets/prefabs/visualization/sphere.prefab", centerPos, Quaternion.identity, true);

                if (sphere == null)
                    continue;

                sphere.currentRadius = sphereRadius;
                sphere.lerpRadius = sphereRadius;
                sphere.lerpSpeed = 1000f;

                sphere.globalBroadcast = globalBroadcast;

                sphere.Spawn();
                sphere.EnableSaving(false);


                _buildingIdToSpheres[building.ID].Add(sphere);
            }

        }

        [ChatCommand("bsphere")]
        private void cmdBsphere(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            var initialBlock = GetLookAtEntity(player, 10f) as DecayEntity;
            if (initialBlock == null)
            {
                SendReply(player, "You must be looking at a DecayEntity!");
                return;
            }

            //PrintWarning("initialBlock bounds: " + initialBlock.WorldSpaceBounds() + ", " + initialBlock.WorldSpaceBounds().extents.magnitude + ", " + initialBlock.WorldSpaceBounds().extents.sqrMagnitude);

            var buildingId = initialBlock.buildingID;

            var building = initialBlock.GetBuilding();

            if (_protectedBuildings.Contains(building))
            {
                SendReply(player, "is already protected!!");
                return;
            }

            SpawnSpheresOnBuilding(building);

            SendReply(player, "spawned spheres");

            var prot = RaidProtectBuilding(building);

            SendReply(player, "raid protected building?: " + prot);

        }

        [ChatCommand("protected")]
        private void cmdProtected(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (_protectedBuildings.Count < 1)
            {
                SendReply(player, "No buildings are currently protected!");
                return;
            }

            var sb = Pool.Get<StringBuilder>();

            try 
            {

                sb.Clear().Append("Currently protected buildings:\n");

                foreach(var building in _protectedBuildings)
                {
                    if (BuildingManager.server.GetBuilding(building.ID) == null)
                    {
                        PrintWarning(nameof(_protectedBuildings) + " had building with ID: " + building.ID + " but GetBuilding returned null!!!!");
                    }

                    var protInfo = _protectionData?.GetOrCreateProtectionInfo(building.ID);

                    sb.Append(building.ID).Append(" (").Append(ReadableTimeSpan(protInfo.ProtectionTime)).Append("), ");
                }

                sb.Length -= 2;

                SendReply(player, sb.ToString());

            }
            finally { Pool.Free(ref sb); }

        }

        [ChatCommand("proteccleft")]
        private void cmdHeProteccTimeLeft(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var initialBlock = GetLookAtEntity(player, 40f) as DecayEntity;
            if (initialBlock == null)
            {
                SendReply(player, "You must be looking at a DecayEntity!");
                return;
            }

            var buildingId = initialBlock.buildingID;

            var building = initialBlock.GetBuilding();

            var prot = _protectionData.GetOrCreateProtectionInfo(buildingId);

            SendReply(player, "Building " + buildingId + " has " + prot.ProtectionTime + " left. Started at: " + prot.ProtectionStartTime);

        }

        [ChatCommand("protecc")]
        private void cmdHeProtecc(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var initialBlock = GetLookAtEntity(player, 10f) as DecayEntity;
            if (initialBlock == null)
            {
                SendReply(player, "You must be looking at a DecayEntity!");
                return;
            }

            var buildingId = initialBlock.buildingID;

            var building = initialBlock.GetBuilding();

            var prot = _protectionData.GetOrCreateProtectionInfo(buildingId);

            if (prot.ProtectionTime.TotalHours < 3) 
                prot.ProtectionTime = TimeSpan.FromHours(3);


            SendReply(player, "protecting building: " + buildingId + ", protection time: " + prot.ProtectionTime);

            var routine = StartRaidProtectionCoroutine(building, true, true);

            SendReply(player, "Started routine with init type, so should be instant");

            var protectionTime = prot.ProtectionTime;

            var wipeSpan = TimeUntilWipe();

            if (wipeSpan.TotalHours <= 24)
            {
                PrintWarning("wipeSpan.TotalHours <= 24, no protection!!!");
            }
            else
            {
                var wipeTime = GetWipeDate();

                var desiredTime = wipeTime.AddHours(-24);

                PrintWarning("wipeTime: " + wipeTime + ", 24 hours before wipe is: " + desiredTime);

                if (DateTime.Now.Add(protectionTime) >= desiredTime)
                {
                    PrintWarning("datetime.now + protecitontime is >= desiredTime, so we must clmap to wipe time! was: " + protectionTime);

                    protectionTime = desiredTime - DateTime.Now;

                    PrintWarning("is now (after clamp: " + protectionTime + ")");
                }
                else PrintWarning("didn't have enough protect to be >= desiredTime");

            }

            PrintWarning("final prot time: " + protectionTime);

        }

        [ChatCommand("owncheck")]
        private void cmdOwnCheck(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            if (args.Length < 1 || !ulong.TryParse(args[0], out var ownerId))
            {
                SendReply(player, "supply userID to check");
                return;
            }

            var initialBlock = GetLookAtEntity(player, 10f) as DecayEntity;
            if (initialBlock == null)
            {
                SendReply(player, "You must be looking at a DecayEntity!");
                return;
            }

         

            var buildingId = initialBlock.buildingID;

            var building = initialBlock.GetBuilding();

            if (building == null)
            {
                SendReply(player, "building is null!!!!!");
                return;
            }

            var isOwnedByUserIdOrClan = IsBuildingOwnedByPlayerOrClanMember(ownerId, building);

            var majorityOwn = GetMajorityOwnerFromBuilding(building);


            SendReply(player, nameof(isOwnedByUserIdOrClan) + " (" + ownerId + "): " + isOwnedByUserIdOrClan);
            SendReply(player, "actual majority owner is: " + GetMajorityOwnerFromBuilding(building));

            var clans = GetClanMembersByUserID(majorityOwn.ToString());
            if (clans == null || clans.Count < 1)
            {
                SendReply(player, "no clan members for: " + majorityOwn);
                return;
            }

            SendReply(player, "clan members of majority owner: " + string.Join(", ", clans));

        }

        [ChatCommand("rgcache")]
        private void cmdClanDebug(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length > 1)
            {


                if (ulong.TryParse(args[0], out var id))
                {
                    SendReply(player, args[0] + " is id, trying to find clan by userid");

                    SendReply(player, "members: " + string.Join(", ", GetClanMembersByUserID(args[0])));

                    return;
                }

                var clanMembers = GetClanMembers(args[0]);

                SendReply(player, "tried to get clan members from tag " + args[0] + ", lets see what we got: " + string.Join(", ", clanMembers));


                return;
            }

            SendReply(player, "Updating");

            UpdateClanMembersCache();

            SendReply(player, "Updated");

        }

        [ChatCommand("raidtimetest")]
        private void cmdRaidTimeTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            if (args.Length < 1)
            {
                SendReply(player, "You must specify the seconds to add!");
                return;
            }

            if (!double.TryParse(args[0], out var seconds))
            {
                SendReply(player, "not double: " + args[0]);
                return;
            }

            var initialBlock = GetLookAtEntity(player, 10f) as DecayEntity;
            if (initialBlock == null)
            {
                SendReply(player, "You must be looking at a DecayEntity!");
                return;
            }

            var buildingId = initialBlock.buildingID;

            var prot = _protectionData.GetOrCreateProtectionInfo(buildingId);
            prot.AddTimeToProtection(new TimeSpan(0, 0, (int)seconds));

            SendReply(player, "added seconds. protectiontime now: " + prot.ProtectionTime);

        }

        #region Hooks

        private void OnClanUpdate(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(nameof(tag));

            PrintWarning(nameof(OnClanUpdate) + " for " + tag + " so we are calling " + nameof(UpdateClanMembersCache));


            UpdateClanMembersCache(); //could we have an option to update only a singular clan? of course. will we? probably. but for now, this is so fast it hardly matters.

        }

        private void OnClanCreate(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(nameof(tag));

            PrintWarning(nameof(OnClanCreate) + " for " + tag + " so we are calling " + nameof(UpdateClanMembersCache));


            UpdateClanMembersCache(); //could we have an option to update only a singular clan? of course. will we? probably. but for now, this is so fast it hardly matters.

        }

        private void OnClanDestroy(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(nameof(tag));

            PrintWarning(nameof(OnClanDestroy) + " for " + tag + " so we are calling " + nameof(UpdateClanMembersCache));

            UpdateClanMembersCache(); //could we have an option to update only a singular clan? of course. will we? probably. but for now, this is so fast it hardly matters.

        }

        private void OnServerInitialized()
        {
            UpdateClanMembersCache();

            LoadData();

            PrintWarning("looping through buildings");

            foreach (var kvp in BuildingManager.server.buildingDictionary)
            {
                var building = kvp.Value;
                if (!_protectedBuildings.Contains(building) && IsApplicableForProtection(building))
                {
                    PrintWarning("building: " + kvp.Key + " is applicable, start raid prot countdown coroutine after init");

                    //save coroutine so it can be cancelled upon connection and/or not started for each member

                    StartRaidProtectionCoroutine(building, true, true);
                }
            }

            StartCoroutine(UpdateClanCacheEnumerator());
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;

            PrintWarning(nameof(OnPlayerConnected));

            var userId = player?.userID ?? 0;

            var userIdString = player?.UserIDString ?? string.Empty;

            var members = GetClanMembersByUserID(userIdString);

            Action rustPlusAction;

            if (members != null && members.Count > 0)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    var mem = members[i];

                    if (_rustPlusNotifyAction.TryGetValue(ulong.Parse(mem), out rustPlusAction))
                        ServerMgr.Instance.CancelInvoke(rustPlusAction);

                }
            }
            else
            {
                if (_rustPlusNotifyAction.TryGetValue(userId, out rustPlusAction))
                    ServerMgr.Instance.CancelInvoke(rustPlusAction);
            }

            foreach (var kvp in BuildingManager.server.buildingDictionary)
            {
                var building = kvp.Value;

                if (building == null)
                    continue;

                if (!IsBuildingOwnedByPlayerOrClanMember(player.userID, kvp.Value))
                    continue;

                if (_buildingIdToEndProtectionCoroutine.TryGetValue(building.ID, out var stopRoutine))
                {
                    PrintWarning("got stopRoutine!!! cancelling for: " + building.ID + "/" + player);
                    StopCoroutine(stopRoutine);
                }

                if (_buildingIdToCountdownCoroutine.TryGetValue(building.ID, out var coroutine) && coroutine != null)
                {
                    PrintWarning("found existing coroutine when player connected!! - stopping coroutine instead of starting a cancel one");

                    StopCoroutine(coroutine);

                    PrintWarning("stopped in-progress start coroutine, removing from dictionary");

                    _buildingIdToCountdownCoroutine.Remove(building.ID);

                    PrintWarning("removed from dictionary for countdown coroutine: " + building.ID);
                }
                else if (_protectedBuildings.Contains(building))
                {
                    PrintWarning("starting end protection coroutine as there was not a start coroutine in progress, and building was protected: " + building.ID + ", majority owned by/clan member: " + player.userID);
                    StartCoroutine(StartEndingRaidProtection(building));
                }
            }

        }

        private TimeSpan GetTimeToAddToBuildingsFromDisconnect(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var secondsConnected = player?.net?.connection?.GetSecondsConnected() ?? 0;
            if (secondsConnected <= 0)
                PrintError("failed to get seconds connected!!!!: " + player);
            else
            {


                var totalTime = (int)secondsConnected;

                if (_lastPlaytimeOnDisconnect.TryGetValue(player.UserIDString, out var lastTime))
                    totalTime += (int)lastTime;
                


                return new TimeSpan(totalTime < 3600 ? 3 : totalTime < 7200 ? 12 : 24, 0, 0);
            }

            return TimeSpan.Zero;
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;

            PrintWarning(nameof(OnPlayerDisconnected) + " for: " + player);

            var userId = player?.userID ?? 0;
            var userIdString = player?.UserIDString ?? string.Empty;

            try 
            {
                var secondsConnected = player?.net?.connection?.GetSecondsConnected() ?? 0;
                if (secondsConnected <= 0)
                    PrintError("failed to get seconds connected!!!!: " + player);
                else
                {
                    PrintWarning("had seconds connected! time: " + secondsConnected);


                    var totalTime = (int)secondsConnected;

                    if (_lastPlaytimeOnDisconnect.TryGetValue(player.UserIDString, out var lastTime))
                    {
                        PrintWarning("had a last play time, adding: " + lastTime + " to: " + totalTime);
                        totalTime += (int)lastTime;
                        PrintWarning("added, now: " + totalTime);
                    }

                    _lastPlaytimeOnDisconnect[player.UserIDString] = secondsConnected;

                    if (totalTime >= 3600) //temp isadmin
                    {
                        PrintWarning("total time was >= 3600, so now rewarding");

                        var span = new TimeSpan(totalTime < 7200 ? 12 : 24, 0, 0);
                        PrintWarning("span from totalTime: " + totalTime + " is: " + span);

                        AddTimeToAllBuildings(player.userID, span);
                    }
                    else PrintWarning("totalTime was < 3600: " + totalTime + " - so no reward!");
                }
            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            ServerMgr.Instance.Invoke(() =>
            {
                foreach (var kvp in BuildingManager.server.buildingDictionary)
                {
                    var building = kvp.Value;
                    if (building == null || _protectedBuildings.Contains(building)) continue;

                    if (_buildingIdToCountdownCoroutine.TryGetValue(building.ID, out var routine) && routine != null)
                    {
                        PrintWarning(building.ID + " already had a countdown coroutine!!!!");
                        continue;
                    }

                    if (!IsBuildingOwnedByPlayerOrClanMember(userId, building))
                        continue;

                    var prot = _protectionData?.GetOrCreateProtectionInfo(building.ID);
                    if (prot != null && prot.ProtectionTime.TotalHours < 3)
                        prot.ProtectionTime = TimeSpan.FromHours(3);

                    if (IsApplicableForProtection(building))
                    {
                        PrintWarning("building: " + kvp.Key + " is majority owned by: " + userId + " or one of their clan members and is applicable, start raid prot countdown coroutine for bid: " + building.ID);

                        StartRaidProtectionCoroutine(building);
                    }
                }
            }, 2f);


            if (_rustPlusNotifyAction.TryGetValue(player.userID, out var action))
                ServerMgr.Instance.CancelInvoke(action);



            var members = GetClanMembersByUserID(userIdString);

            var anyOtherPlayersConnected = false;

            if (members != null && members.Count > 1)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    var p = members[i];

                    if (p == userIdString) continue;

                    var bp = BasePlayer.FindByID(ulong.Parse(p));
                    if (bp?.IsConnected ?? false)
                    {
                        anyOtherPlayersConnected = true;
                        break;
                    }
                }
            }

            if (anyOtherPlayersConnected)
            {
                PrintWarning("user disconnected but had other clan members connected: " + player?.displayName + " (" + player?.UserIDString + ")");
                return;
            }

            action = new Action(() =>
            {
                SendRustPlusNotificationsForProtectedBuildings(userId, members);
            });

            ServerMgr.Instance.Invoke(action, 620f); //600 was too close? may have fired off before buildings were actually protected

            _rustPlusNotifyAction[userId] = action;


        }

        private void Unload()
        {
            try
            {

                var copyHash = Pool.Get<HashSet<BuildingManager.Building>>();

                try
                {
                    copyHash.Clear();

                    foreach (var building in _protectedBuildings)
                        copyHash.Add(building);
                    

                    foreach (var building in copyHash)
                        StopRaidProtection(building);
                }
                finally
                {
                    copyHash.Clear();
                    Pool.Free(ref copyHash);
                }

               
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try
            {
                //could maybe use reflection to find all ienumerators and stop coroutines, but whatever. this works for now, and is probably faster

                foreach (var routine in _coroutines)
                {
                    if (routine == null) continue;

                    ServerMgr.Instance.StopCoroutine(routine);
                }
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try 
            {
                foreach (var kvp in _rustPlusNotifyAction)
                {
                    ServerMgr.Instance.CancelInvoke(kvp.Value);
                }
            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            SaveData();
        }

        private void OnLootEntityEnd(BasePlayer player, BuildingPrivlidge cupboard)
        {
            if (player == null || cupboard == null) return;

            if (cupboard.GetProtectedMinutes(true) < 1f)
                return;

            var building = BuildingManager.server.GetBuilding(cupboard.buildingID);
            if (building == null)
            {
                PrintError(cupboard + " had null building on " + nameof(OnLootEntityEnd) + "!!!!");
                return;
            }

            if (!IsBuildingOwnedByPlayerOrClanMember(player.userID, building))
                return;

            var buildingId = building.ID;


            var protectionInfo = _protectionData.GetOrCreateProtectionInfo(buildingId);
            if (protectionInfo == null)
            {
                PrintError("Somehow protectionInfo is null?!");
                return;
            }

            var timeToAdd = GetTimeToAddToBuildingsFromDisconnect(player);


            var timeWouldBe = protectionInfo.ProtectionTime + timeToAdd;

            if (timeWouldBe.TotalDays > 1)
                timeWouldBe = TimeSpan.FromDays(1); //clamp


            var sb = Pool.Get<StringBuilder>();
            try 
            {
                var uptimeMsg = sb.Clear().Append("<color=#ff33ff>If you disconnect now, this building will be <i>offline-raid protected</i> for:</color>\n<size=20><color=#77ff33>").Append(ReadableTimeSpan(timeWouldBe)).Append("</size></color>").ToString();

                ShowToast(player, uptimeMsg);

                player?.Invoke(() =>
                {
                    if (player == null || !player.IsConnected || player.IsDead()) return;

                    if (player?.inventory?.loot?.entitySource != null) return; //looted again

                    var toastWarningMsg = "<color=yellow>NOTE:</color> Protection does not start until <i><color=yellow>10</color> mins</i> after disconnect\nIf a raid starts during this period, the building will <i><color=yellow>NOT</color></i> be protected!";


                    ShowToast(player, toastWarningMsg, 1);

                    player?.Invoke(() =>
                    {
                        ShowToast(player, toastWarningMsg, 1);
                    }, 1.25f);


                }, 5f);

            }
            finally { Pool.Free(ref sb); }

            

            /*/
            var cupboardSpan = TimeSpan.FromSeconds(cupboard.GetProtectedSeconds());

            PrintWarning("protection time pre ")

            protectionInfo.ProtectionStartTime = Mathf.Clamp(protectionInfo.ProtectionTime, protectionInfo.ProtectionStartTime, cupboardSpan);/*/
            //eventually, we should force protection time to be no more than cupboard upkeep time
            //right now, it starts out that way, but if it somehow changes, it is never updated
            //the concern is that we need to restart coroutines, which changing the start time itself does not automatically do

        }

        private void OnRocketLaunched(BasePlayer player, BaseEntity entity)
        {
            if (player == null || entity == null) return;

            if (!IsLookingAtProtectedBuilding(player, 250f, 12.5f)) 
                return;

            ShowProtectedToast(player);

            if (!entity.IsDestroyed) entity.Kill();

            var launcher = player?.GetHeldEntity() as BaseLauncher;

            if (launcher == null) 
                return;

            var item = launcher?.GetItem();
            var oldCond = item?.condition ?? 0f;
            if (oldCond > 0f)
            {
                launcher.Invoke(() =>
                {
                    if (item == null) return;

                    item.condition = oldCond;
                    item.MarkDirty();
                }, 0.1f);
            }

            launcher.primaryMagazine.contents += 1;
            launcher.SendNetworkUpdate();

            NoteItemByID(player, launcher.primaryMagazine.ammoType.itemid, 1);
        }

        private void OnExplosiveThrown(BasePlayer player, BaseEntity entity, ThrownWeapon item)
        {
            StartCoroutine(CheckExplosiveStickCoroutine(entity, player, item?.GetItem()));
        }

        private void OnEntityKill(DecayEntity entity)
        {
            var buildingId = entity?.buildingID ?? 0;

            if (buildingId == 0)
                return;
            
            var building = BuildingManager.server.GetBuilding(buildingId);

            if (building == null)
            {
                PrintWarning("building was null prior to invoke?!: " + buildingId);
                return;
            }


            //save and restart invoke instead of repeatedly invoking? small perf opt, maybe
            ServerMgr.Instance.Invoke(() =>
            {
                var newBuilding = BuildingManager.server.GetBuilding(buildingId);

                if (newBuilding == null)
                {
                    if (building != null) StopRaidProtection(building);
                    else PrintWarning("original building is now null!!");
                }

            }, 1f);

        }

        private object OnSamSiteTarget(SamSite sam, BaseCombatEntity entity)
        {
            if (entity == null) return null;

            var building = sam?.GetBuilding();

            if (building != null && _protectedBuildings.Contains(building))
                return true;
            

            return null;
        }

        private object CanBuild(Planner plan, Construction prefab, Construction.Target target)
        {
            if (plan == null || prefab == null) return null;

            var watch = Pool.Get<Stopwatch>();
            try 
            {

                watch.Restart();

                var player = plan?.GetOwnerPlayer();
                if (player == null) return null;

                var userId = player?.userID ?? 0;

                var pos = player?.transform?.position ?? Vector3.zero;

                var entities = Pool.GetList<DecayEntity>();
                try
                {

                    Vis.Entities(pos, 38f, entities, Rust.Layers.Construction); //this is slow, but it can't be helped. even a hashset containg decay entities is still slow - must be vector3.dist check

                    var buildings = Pool.Get<HashSet<BuildingManager.Building>>();

                    var targetDecay = target.entity as DecayEntity;

                    var targetDecayBuilding = targetDecay?.GetBuilding();

                    try 
                    {
                        buildings.Clear();

                        var cupboardCache = Pool.Get<Dictionary<uint, BuildingPrivlidge>>();

                        try
                        {
                            cupboardCache.Clear();

                            for (int i = 0; i < entities.Count; i++)
                            {
                                var entity = entities[i];

                                var building = entity?.GetBuilding();
                                if (building == null) continue;

                                if (!_protectedBuildings.Contains(building)) 
                                    continue; //speed opt by checking here instead of in buildings itself? it is a hashset after all

                                if (entity.transform.position.y < pos.y && (pos.y - entity.transform.position.y) >= 10) continue;

                                if (targetDecayBuilding == building) continue;



                                if (!cupboardCache.TryGetValue(building.ID, out var cupboard) || (cupboard == null || cupboard.IsDestroyed)) //since we loop through a bunch of entities, we may get a lot from the same building. we don't need to call GetDominating... each time
                                {
                                    cupboard = building?.GetDominatingBuildingPrivilege();

                                    if (cupboard != null) cupboardCache[building.ID] = cupboard;
                                    else cupboardCache.Remove(building.ID);
                                }

                                if (cupboard == null) continue;

                                //cupboard.IsAuthed is slow


                                var isAuthed = false;
                                foreach (var ap in cupboard.authorizedPlayers)
                                {
                                    if (ap.userid == userId)
                                    {
                                        isAuthed = true;
                                        break;
                                    }
                                }

                                if (!isAuthed) continue;


                                buildings.Add(building);
                            }

                        }
                        finally
                        {
                            cupboardCache?.Clear();
                            Pool.Free(ref cupboardCache);
                        }

                        foreach (var building in buildings)
                        {
                            if (IsBuildingOwnedByPlayerOrClanMember(userId, building)) 
                                continue;

                            SendReply(player, BUILD_TOO_CLOSE_PROTECTED_BUILDING);
                            ShowToast(player, BUILD_TOO_CLOSE_TOAST, 1);

                            return false;
                        }

                    }
                    finally 
                    {
                        buildings.Clear();
                        Pool.Free(ref buildings); 
                    }


                }
                finally { Pool.FreeList(ref entities); }

            }
            finally
            {
                try { if (watch.Elapsed.TotalMilliseconds >= 4) PrintWarning(nameof(CanBuild) + " took " + watch.Elapsed.TotalMilliseconds + " ms"); }
                finally { Pool.Free(ref watch); }
            }

            return null;
        }

        private object OnEntityTakeDamage(DecayEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return null;

            var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;

            if (majDmg != Rust.DamageType.Explosion && majDmg != Rust.DamageType.Stab && majDmg != Rust.DamageType.Slash && majDmg != Rust.DamageType.Heat && majDmg != Rust.DamageType.Blunt && majDmg != Rust.DamageType.Bullet) return null;

            var building = entity.GetBuilding();

            if (building == null)
            {

                var ioEnt = entity as IOEntity;

                if (ioEnt != null)
                {
                    PrintWarning("building was null but ioEnt wasn't, trying to grab...");
                    
                    for (int i = 0; i < ioEnt.inputs.Length; i++)
                    {
                        var input = ioEnt.inputs[i];

                        building = ioEnt.inputs[i]?.connectedTo?.Get()?.GetBuilding();

                        if (building != null)
                        {
                            PrintWarning("on io input, we got building: " + building.ID);
                            break;
                        }


                    }

                    if (building == null)
                    {
                        PrintWarning("still null");

                        for (int i = 0; i < ioEnt.outputs.Length; i++)
                        {
                            var input = ioEnt.outputs[i];

                            building = ioEnt.outputs[i]?.connectedTo?.Get()?.GetBuilding();

                            if (building != null)
                            {
                                PrintWarning("on io output, we got building: " + building.ID);
                                break;
                            }
                        }

                        if (building == null) 
                            PrintWarning("building still null, could never get!!");

                    }
                }

            }

            var blockGrade = (entity as BuildingBlock)?.grade ?? BuildingGrade.Enum.None;

            if (_protectedBuildings.Contains(building) && blockGrade != BuildingGrade.Enum.Twigs) //allow dmg twig even if protected
            {
                CancelDamage(info);

                if (majDmg == Rust.DamageType.Explosion || blockGrade <= BuildingGrade.Enum.Wood)
                {
                    var attackerPlayer = info?.Initiator as BasePlayer;

                    if (attackerPlayer != null)
                        ShowProtectedToast(attackerPlayer);
                }
               

                return true;
            }

            if (majDmg != Rust.DamageType.Explosion) 
                return null; //no (raid) blocking of offline protection because it was dmg type that wasn't explosive - don't want a stray bullet to stop building protections

            ServerMgr.Instance.Invoke(() =>
            {

                if (building == null)
                    return;
                

                var dmgAmount = info?.damageTypes?.Total() ?? 0f;

                if (dmgAmount <= 0f)
                    return;
                

                if (!_raidBlockedBuildings.Add(building))
                {
                    if (_buildingIdToRaidBlockCoroutine.TryGetValue(building.ID, out var routine) && routine != null) StopCoroutine(routine);
                    else PrintWarning("Couldn't get coroutine despite building being blocked?!??!?!");
                }
                else
                {
                    PrintWarning("raid blocking building (init): " + building.ID);



                    var raidBlockStartedMsgShort = "A nearby building has become raid blocked!";
                    var raidBlockStartedMsg = string.Empty;

                    var sb = Pool.Get<StringBuilder>();

                    try 
                    {
                        raidBlockStartedMsg = sb.Clear().Append(raidBlockStartedMsgShort).Append("\nOffline Raid Protection will not apply to this building if a player disconnects during a raid.").ToString();
                    }
                    finally { Pool.Free(ref sb); }

                    var centerPos = FindCenterOfBuilding(building);

                    if (centerPos != Vector3.zero)
                    {
                        var players = Pool.GetList<BasePlayer>();

                        try
                        {

                            Vis.Entities(centerPos, 40f, players, 131072);

                            for (int i = 0; i < players.Count; i++)
                            {
                                var p = players[i];

                                if (p == null || !p.IsConnected || p.IsDead()) continue;

                                SendReply(p, raidBlockStartedMsg);
                                ShowToast(p, raidBlockStartedMsgShort, 1);
                            }
                        }
                        finally { Pool.FreeList(ref players); }
                    }

                
                }

                _buildingIdToCountdownCoroutine.Remove(building.ID);

                _buildingIdToRaidBlockCoroutine[building.ID] = StartCoroutine(StopRaidBlockAfterTime(building, TimeSpan.FromMinutes(10)));

            }, 0.1f);

           

            return null;
        }

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item item)
        {
            if (player == null || item == null) return;

            var held = item?.GetHeldEntity();
            if (held != null && (held is ThrownWeapon || held is BaseLauncher))
                SendToastIfLookingAtProtectedBuilding(player, held is ThrownWeapon ? 15f : 115f);
        }

        private void OnPlayerInput(BasePlayer player, InputState input) //todo: clean up OnPlayerInput (bad if nesting)
        {
            if (player == null || input == null) return;

            if (!input.IsDown(BUTTON.FIRE_SECONDARY) && !input.WasJustPressed(BUTTON.FIRE_SECONDARY)) return;

            var heldEntity = player?.GetHeldEntity();

            if (heldEntity is ThrownWeapon || heldEntity is BaseLauncher)
                SendToastIfLookingAtProtectedBuilding(player, heldEntity is ThrownWeapon ? 15f : 115f);
        }

        #endregion
        #region Classes
        private class ProtectionInfo
        {
            public TimeSpan ProtectionTime { get; set; } = TimeSpan.Zero;

            [JsonIgnore]
            public DateTime ProtectionStartTime { get; set; } = DateTime.MinValue;

            public void ProtectionStarted()
            {
                Interface.Oxide.LogWarning(nameof(ProtectionStarted) + " @ " + DateTime.UtcNow);
                ProtectionStartTime = DateTime.UtcNow;
            }

            public void ProtectionEnded()
            {
                if (ProtectionStartTime <= DateTime.MinValue)
                {
                    Interface.Oxide.LogWarning(nameof(ProtectionStartTime) + " <= DateTime.MinValue?!: " + ProtectionStartTime);
                    //likely never started
                    return;
                }

                var timePassed = DateTime.UtcNow - ProtectionStartTime;

                Interface.Oxide.LogWarning(nameof(ProtectionEnded) + ", utcnow - protectionstarttime: " + timePassed);

                Interface.Oxide.LogWarning(nameof(ProtectionEnded) + ", ProtectionTime pre-subtraction: " + ProtectionTime);

                ProtectionTime = ProtectionTime.Subtract(timePassed);

                if (ProtectionTime.TotalSeconds <= 0) ProtectionTime = TimeSpan.Zero;

                Interface.Oxide.LogWarning(nameof(ProtectionEnded) + ", ProtectionTime post-subtraction: " + ProtectionTime);
            }

            //perhaps use later on. the issue with this is that we don't have a way of recalculating based off of *current* TC time left, so it'll only grab it once and use that as the time
            public TimeSpan GetAdjustedProtectionTime(BuildingManager.Building building)
            {
                if (building == null)
                    throw new ArgumentNullException(nameof(building));

                var cupboard = building?.GetDominatingBuildingPrivilege();
                if (cupboard == null)
                    return ProtectionTime;

                var cupboardMinutes = (int)cupboard.GetProtectedMinutes();

                return cupboardMinutes < 1 ? TimeSpan.Zero : cupboardMinutes < ProtectionTime.TotalMinutes ? new TimeSpan(0, cupboardMinutes, 0) : ProtectionTime;
            }

            public TimeSpan AddTimeToProtection(TimeSpan timeToAdd) //this probably all works :)
            {
                if (ProtectionTime.TotalHours >= 24) return ProtectionTime;

                if (timeToAdd.TotalSeconds < 0) Interface.Oxide.LogError("got NEGATIVE timespan: " + timeToAdd);

                var desiredProtectionTime = ProtectionTime + timeToAdd;

                if (desiredProtectionTime.TotalDays > 1) desiredProtectionTime = TimeSpan.FromDays(1);

                ProtectionTime = desiredProtectionTime;

                return ProtectionTime;
            }

            public ProtectionInfo() { }

            public ProtectionInfo(TimeSpan span)
            {
                ProtectionTime = span;
            }

        }

        private class ProtectionData
        {
            public Dictionary<uint, ProtectionInfo> buildingProtections = new();

            public ProtectionInfo GetOrCreateProtectionInfo(uint buildingId)
            {
                if (buildingId == 0)
                    throw new ArgumentOutOfRangeException(nameof(buildingId));

                if (buildingProtections.TryGetValue(buildingId, out var protection))
                    return protection;

                return buildingProtections[buildingId] = new ProtectionInfo();
            }

        }
        #endregion
        #region Util

        private readonly Dictionary<string, List<string>> _clanNameToMembers = new(); //move to top at some point i suppose
        private readonly Dictionary<string, string> _userIdToClanTag = new();

        private void UpdateClanMembersCache() //masterpiece
        {
            var watch = Pool.Get<Stopwatch>();

            try 
            {
                watch.Restart();

                var tags = Pool.Get<HashSet<string>>();
                try
                {

                    tags.Clear();

                    Clans?.Call("GetAllClanTagsNoAlloc", tags);

                    _clanNameToMembers.Clear();
                    _userIdToClanTag.Clear();

                    foreach (var clanTag in tags)
                    {
                        var clanMembers = Clans?.Call<List<string>>("GetClanMembersByTag", clanTag);

                        _clanNameToMembers[clanTag] = clanMembers;

                        for (int i = 0; i < clanMembers.Count; i++)
                        {
                            var mem = clanMembers[i];
                            _userIdToClanTag[mem] = clanTag;
                        }

                    }

                }
                finally
                {
                    tags.Clear();
                    Pool.Free(ref tags);
                }
            }
            finally
            {
                try
                {
                    if (watch.Elapsed.TotalMilliseconds > 3) PrintWarning(nameof(UpdateClanMembersCache) + " took: " + watch.Elapsed.TotalMilliseconds);
                }
                finally { Pool.Free(ref watch); }
            }

        }

        private List<string> GetClanMembers(string clanTag)
        {
            if (string.IsNullOrWhiteSpace(clanTag))
                throw new ArgumentNullException(nameof(clanTag));

            return _clanNameToMembers.TryGetValue(clanTag, out var members) ? members : null;
        }

        private List<string> GetClanMembersByUserID(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) 
                throw new ArgumentNullException(nameof(userId));

            return !_userIdToClanTag.TryGetValue(userId, out var clanTag) ? null : GetClanMembers(clanTag);
        }

        private string GetClanTag(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            return _userIdToClanTag.TryGetValue(userId, out var clanTag) ? clanTag : string.Empty;
        }

        private DateTime GetWipeDate()
        {
            return WipeInfo?.Call<DateTime>("GetWipeDate") ?? DateTime.MinValue;
        }

        private TimeSpan TimeUntilWipe()
        {
            var time = WipeInfo?.Call<TimeSpan>("TimeUntilWipe") ?? TimeSpan.Zero;

            if (time <= TimeSpan.Zero)
                throw new ArgumentNullException(nameof(time));

            return time;
        }

        private bool Within24HoursOfWipe()
        {
            var wipeSpan = TimeUntilWipe();

            return wipeSpan != TimeSpan.Zero && wipeSpan.TotalHours <= 24;
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
            else if (span.TotalMinutes > 60)
            {
                var totalMinutesWithoutHours = (span.TotalMinutes - ((int)span.TotalHours * 60));
                str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + (totalMinutesWithoutHours <= 0 ? string.Empty : " " + totalMinutesWithoutHours.ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)");
            }
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        private void NoteItemByID(BasePlayer player, int itemID, int amount)
        {
            if (player == null || !player.IsConnected) return;

            var sb = Pool.Get<StringBuilder>();
            try { player.SendConsoleCommand(sb.Clear().Append("note.inv ").Append(itemID).Append(" ").Append(" ").Append(" ").Append(amount).ToString()); } //if the amount is - it will be included automatically, so don't need to add it in the " " sb append
            finally { Pool.Free(ref sb); }
        }

        private void LoadData()
        {
            _protectionData = Interface.Oxide?.DataFileSystem?.ReadObject<ProtectionData>(nameof(RaidGuardian)) ?? new ProtectionData();
        }

        private void SaveData()
        {
            if (_protectionData == null)
            {
                PrintWarning(nameof(SaveData) + " called with null _protectionData!!");
                return;
            }

            Interface.Oxide.DataFileSystem.WriteObject(nameof(RaidGuardian), _protectionData);
        }

        private void AddTimeToAllBuildings(ulong userId, TimeSpan timeToAdd)
        {
            PrintWarning(nameof(AddTimeToAllBuildings) + ", " + userId + ", " + timeToAdd);

            foreach(var kvp in BuildingManager.server.buildingDictionary)
            {
                var building = kvp.Value;

                if (IsBuildingOwnedByPlayerOrClanMember(userId, building))
                    AddTimeToBuildingProtection(building, timeToAdd);
                
            }
        }

        private void AddTimeToBuildingProtection(BuildingManager.Building building, TimeSpan timeToAdd)
        {
            PrintWarning(nameof(AddTimeToBuildingProtection) + ", " + building?.ID + ", " + timeToAdd);
            var prot = _protectionData.GetOrCreateProtectionInfo(building.ID);

            PrintWarning("prot.Protectiontime was: " + prot.ProtectionTime);
            prot.AddTimeToProtection(timeToAdd);
            PrintWarning("is now: " + prot.ProtectionTime);


        }

        private IEnumerator UpdateClanCacheEnumerator()
        {
            PrintWarning(nameof(UpdateClanMembersCache) + " wakeup");

            var c = 0;

            while(true)
            {
                c++;

                if (c >= 10000)
                {
                    PrintError("c hit 10000, so something really bad happened");
                    break;
                }

                UpdateClanMembersCache();

                yield return CoroutineEx.waitForSecondsRealtime(480f);

            }
        }

        private IEnumerator StopRaidBlockAfterTime(BuildingManager.Building building, TimeSpan time)
        {
            if (building == null)
                throw new ArgumentOutOfRangeException(nameof(building));

            if (time <= TimeSpan.Zero)
            {
                PrintError(nameof(StopRaidBlockAfterTime) + " called with time <= Zero!!");
                yield break;
            }

            var seconds = (float)time.TotalSeconds;

            yield return CoroutineEx.waitForSecondsRealtime(seconds);

            if (building == null)
            {
                PrintError("RaidBlock stop: building is null after coroutine yield of: " + seconds + " seconds!");
                yield break;
            }

            PrintWarning("RaidBlock stop: yield finished, seconds was: " + seconds + " - now setting to TimeSpan.zero for building: " + building.ID);


            StopRaidBlock(building);
        }

        private IEnumerator StopProtectionAfterTime(BuildingManager.Building building, TimeSpan time)
        {
            if (building == null)
                throw new ArgumentOutOfRangeException(nameof(building));

            if (time <= TimeSpan.Zero)
            {
                PrintError(nameof(StopProtectionAfterTime) + " called with time <= Zero!!");
                yield break;
            }

            var seconds = (float)time.TotalSeconds;

            PrintWarning(nameof(StopProtectionAfterTime) + " (bID: " + building.ID + ") has time: " + time + ", totalseconds with base of UtcNow: " + seconds + " - yielding!");

            yield return CoroutineEx.waitForSecondsRealtime(seconds);

            if (building == null)
            {
                PrintError("building is null after coroutine yield of: " + seconds + " seconds!");
                yield break;
            }

            PrintWarning("yield finished, seconds was: " + seconds + " - now setting to TimeSpan.zero for building: " + building.ID);

            var protInfo = _protectionData?.GetOrCreateProtectionInfo(building.ID);
            if (protInfo != null)
                protInfo.ProtectionTime = TimeSpan.Zero;
            

            StopRaidProtection(building);
        }

        private Coroutine StartCoroutineToEndProtection(BuildingManager.Building building)
        {
            var info = _protectionData.GetOrCreateProtectionInfo(building.ID);

            if (info.ProtectionTime <= TimeSpan.Zero)
            {
                PrintWarning("couldn't start coroutine, got endprotectontime of DateTime.UtcNow or less!!!");
                return null;
            }

            if (_buildingIdToEndProtectionCoroutine.TryGetValue(building.ID, out var existingRoutine))
            {
                PrintWarning(nameof(StartCoroutineToEndProtection) + " for " + building.ID + " already had routine - cancelling and setting new!");
                StopCoroutine(existingRoutine);
            }

            var protectionTime = info.ProtectionTime;

            var wipeSpan = TimeUntilWipe();

            if (wipeSpan.TotalHours <= 24)
            {
                PrintWarning("wipeSpan.TotalHours <= 24, no protection!!!");
                return null;
            }
            else
            {
                var wipeTime = GetWipeDate();

                var desiredTime = wipeTime.AddHours(-24);

                PrintWarning("wipeTime: " + wipeTime + ", 24 hours before wipe is: " + desiredTime);

                if (DateTime.Now.Add(protectionTime) >= desiredTime)
                {
                    PrintWarning("datetime.now + protecitontime is >= desiredTime, so we must clmap to wipe time! was: " + protectionTime);

                    protectionTime = desiredTime - DateTime.Now;

                    PrintWarning("is now (after clamp: " + protectionTime + ")");
                }
                else PrintWarning("didn't have enough protect to be >= desiredTime");

            }


            var routine = StartCoroutine(StopProtectionAfterTime(building, protectionTime));

            _buildingIdToEndProtectionCoroutine[building.ID] = routine;

            return routine;
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

        private void SendRustPlusNotification(ulong userId, string titleMsg, string bodyMsg = "PRISM", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            NotificationList.SendNotificationTo(userId, channel, titleMsg, bodyMsg, Util.GetServerPairingData());
        }

        private bool IsLookingAtProtectedBuilding(BasePlayer player, float maxDist = 115f, float radius = 2.5f)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return false;

            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);

            if (!Physics.SphereCast(ray, radius, out var hit, maxDist, _constructionColl)) return false;

            var decayEntity = hit.GetEntity() as DecayEntity;


            return decayEntity != null && IsBuildingIdProtected(decayEntity.buildingID);
        }

        private void SendToastIfLookingAtProtectedBuilding(BasePlayer player, float radius = 115f, float coolDownSecs = 4f)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));

            if (_lastWarningToast.TryGetValue(player.UserIDString, out var lastTime) && (Time.realtimeSinceStartup - lastTime) < coolDownSecs) return;

            if (!IsLookingAtProtectedBuilding(player)) 
                return;

            _lastWarningToast[player.UserIDString] = Time.realtimeSinceStartup;
            ShowProtectedToast(player);
        }

        private void ShowProtectedToast(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.IsConnected) return;

            ShowToast(player, PROTECTED_BUILDING_TOAST, 1);
        }

        private bool IsEscapeBlocked(string userID)
        {
            if (string.IsNullOrEmpty(userID))
                throw new ArgumentNullException(nameof(userID));

            return NoEscape?.Call<bool>("IsEscapeBlockedS", userID) ?? false;
        }

        private bool IsBuildingIdProtected(uint buildingId)
        {
            return buildingId != 0 && _protectedBuildings.Contains(BuildingManager.server.GetBuilding(buildingId));
        }

        private readonly Dictionary<ulong, Dictionary<BuildingManager.Building, TimeCachedValue<bool>>> _isOwnedByPlayerOrClanCache = new();

        private bool IsBuildingOwnedByPlayerOrClanMember(ulong userId, BuildingManager.Building building, bool skipCache = false)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            if (!_isOwnedByPlayerOrClanCache.TryGetValue(userId, out _))
                _isOwnedByPlayerOrClanCache[userId] = new Dictionary<BuildingManager.Building, TimeCachedValue<bool>>();

            if (!_isOwnedByPlayerOrClanCache[userId].TryGetValue(building, out _))
                _isOwnedByPlayerOrClanCache[userId][building] = new TimeCachedValue<bool>() 
                {
                    refreshCooldown = 15,
                    refreshRandomRange = 2.5f,
                    updateValue = new Func<bool>(() =>
                    {
                        var majorityOwner = GetMajorityOwnerFromBuilding(building);
                        if (majorityOwner == userId)
                        {
                            //   PrintWarning(nameof(IsBuildingOwnedByPlayerOrClanMember) + " for: " + userId + ", building: " + building.ID + ", majority owner == userId: " + majorityOwner);
                            return true;
                        }



                        var members = GetClanMembersByUserID(userId.ToString());

                        if (members == null || members.Count < 1) return false;

                        if (members.Contains(majorityOwner.ToString()))
                        {
                            //PrintWarning("clan member is majorityOwner!!: " + majorityOwner + " majorityOwner of: " + building.ID + ", original userId: " + userId);

                            return true;
                        }

                        var totalCount = building.decayEntities.Count;

                        var clanOwned = 0;

                        for (int i = 0; i < building.decayEntities.Count; i++)
                        {
                            var entity = building.decayEntities[i];
                            if (entity == null || entity.IsDestroyed || entity.OwnerID == 0) continue;

                            if (members.Contains(entity.OwnerID.ToString())) clanOwned++;

                        }

                        if (clanOwned >= (totalCount * 0.5))
                        {
                            PrintWarning("clanOwned >= 50% of totalCount!: " + clanOwned + " >= " + (totalCount * 0.5));
                            return true;
                        }

                        return false;
                    })
                };

            return _isOwnedByPlayerOrClanCache[userId][building].Get(skipCache);
        }

        private readonly Dictionary<BuildingManager.Building, TimeCachedValue<ulong>> _cachedMajorityBuildingOwner = new();

        private ulong GetMajorityOwnerFromBuilding(BuildingManager.Building building, bool skipCache = false)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            if (!_cachedMajorityBuildingOwner.TryGetValue(building, out _))
                _cachedMajorityBuildingOwner[building] = new TimeCachedValue<ulong>
                {
                    refreshCooldown = 20f,
                    refreshRandomRange = 2.5f,
                    updateValue = new Func<ulong>(() =>
                    {
                        var watch = Pool.Get<Stopwatch>();
                        try
                        {
                            watch.Restart();

                            var ownedBlockCounts = Pool.Get<Dictionary<ulong, int>>();

                            try
                            {
                                ownedBlockCounts.Clear();


                                foreach (var block in building.buildingBlocks)
                                {
                                    if (block == null || block.OwnerID == 0) continue;

                                    if (!ownedBlockCounts.TryGetValue(block.OwnerID, out var count)) ownedBlockCounts[block.OwnerID] = 1;
                                    else ownedBlockCounts[block.OwnerID]++;
                                }

                                if (ownedBlockCounts.Count < 1) return 0;


                                var lastBlockCount = 0;
                                var lastUserId = 0UL;

                                foreach (var kvp in ownedBlockCounts)
                                {
                                    if (kvp.Value > lastBlockCount)
                                    {
                                        lastBlockCount = kvp.Value;
                                        lastUserId = kvp.Key;
                                    }
                                }

                                return lastUserId;

                                //temp linq:

                                //return ownedBlockCounts.Where(p => p.Value == ownedBlockCounts.Values.Max()).FirstOrDefault().Key;
                            }
                            finally
                            {
                                ownedBlockCounts?.Clear();
                                Pool.Free(ref ownedBlockCounts);
                            }

                        }
                        finally
                        {
                            try
                            {
                                if (watch.Elapsed.TotalMilliseconds > 1) PrintWarning(nameof(GetMajorityOwnerFromBuilding) + " took: " + watch.Elapsed.TotalMilliseconds + "ms");
                            }
                            finally { Pool.Free(ref watch); }
                        }
                    })
                };

            return _cachedMajorityBuildingOwner[building].Get(skipCache);
        }

        private bool IsApplicableForProtection(BuildingManager.Building building)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            var wipeSpan = TimeUntilWipe();
            if (wipeSpan > TimeSpan.Zero && wipeSpan.TotalHours <= 24)
            {
                PrintWarning("wipeSpan <= 24, is not applicable! return false.");
                return false;
            }

            if (building?.decayEntities == null || building.decayEntities.Count < 1)
            {
                PrintWarning("return false for : " + nameof(IsApplicableForProtection) + " for bid: " + building?.ID + " as it has no decay blocks!!");
                return false;
            }

            if (_raidBlockedBuildings.Contains(building))
            {
                PrintWarning(nameof(IsApplicableForProtection) + " ret false for " + building?.ID + " as it is raid blocked");
                return false;
            }

            var cupboard = building?.GetDominatingBuildingPrivilege();
            if (cupboard == null || cupboard.GetProtectedMinutes() < 1)
            {
                PrintWarning("building has no cupboard or protected minutes < 1: " + building?.ID);
                return false;
            }

            var protectionInfo = _protectionData.GetOrCreateProtectionInfo(building.ID);
            if (protectionInfo.ProtectionTime <= TimeSpan.Zero)
            {
                PrintWarning("building: " + building.ID + " had invalid endprotectiontime (not enough to start): " + protectionInfo.ProtectionTime);
                return false;
            }
         //   else PrintWarning("building: " + building.ID + " passed ProtectionTime check, has enough time to start, span is: " + protectionInfo.ProtectionTime);


            foreach(var p in cupboard.authorizedPlayers)
            {
                var pObj = BasePlayer.FindByID(p.userid);

                if (pObj != null && pObj.IsConnected)
                {
                    PrintWarning("building: " + building.ID + " had a player authed to cupboard (" + pObj.UserIDString + ") who is connected! - return false, not applicable");
                    return false;
                }
            }


            var majorityOwnerId = GetMajorityOwnerFromBuilding(building);

            var majorityOwner = BasePlayer.FindByID(majorityOwnerId);
            if (majorityOwner != null)
            {
                /*/
                if (majorityOwner.IsAdmin)
                {
                    PrintWarning("temp debug " + nameof(IsApplicableForProtection) + " return true for majority owner admin - regardless of online or not");
                    return true;
                }/*/

                if (majorityOwner.IsConnected)
                {
                    PrintWarning("Majority owner is connected, ret false for applicability");
                    return false;
                }
                else if (IsEscapeBlocked(majorityOwner.UserIDString))
                {
                    PrintWarning("Majority owner is escape blocked, ret false for applicability");
                    return false;
                }
            }

            var majorityOwnerClanMembers = GetClanMembersByUserID(majorityOwnerId.ToString());

            if (majorityOwnerClanMembers != null && majorityOwnerClanMembers.Count > 0)
            {
               // PrintWarning("majorityOwner: " + majorityOwnerId + " is in clan with " + majorityOwnerClanMembers.Count + " members");
                for (int i = 0; i < majorityOwnerClanMembers.Count; i++)
                {
                    var clanMem = BasePlayer.FindByID(ulong.Parse(majorityOwnerClanMembers[i]));
                    if (clanMem != null)
                    {
                        if (clanMem.IsConnected)
                        {
                            PrintWarning("building: " + building.ID + " had an online clan member (majority building owner had a clan member who is online still)");
                            return false;
                        }
                        else if (IsEscapeBlocked(clanMem.UserIDString))
                        {
                            PrintWarning("found player who is in clan or is owner and is raid/escape blocked!!");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void StopRaidBlock(BuildingManager.Building building)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            if (!_raidBlockedBuildings.Remove(building))
            {
                //PrintWarning(nameof(StopRaidProtection) + " for: " + building.ID + " returned false because protectedbuildings couldn't remove building (didn't exist)!");
                return;
            }

            var centerPos = FindCenterOfBuilding(building);

            if (centerPos == Vector3.zero)
                return;
            

            var players = Pool.GetList<BasePlayer>();

            try 
            {

                Vis.Entities(centerPos, 40f, players, 131072);

                var raidEndedMsg = "<size=16><color=red>Raid Block</color></size> has <color=green><i>ended</i></color> for a nearby building!";

                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];

                    if (p == null || !p.IsConnected || p.IsDead()) continue;

                    SendReply(p, raidEndedMsg);
                    ShowToast(p, raidEndedMsg, 1);
                }

            }
            finally { Pool.FreeList(ref players); }
        }

        private void StopRaidProtection(BuildingManager.Building building)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            if (!_protectedBuildings.Remove(building))
                return;
            

            try
            {
                var prot = _protectionData?.GetOrCreateProtectionInfo(building.ID);
                if (prot != null)
                {
                    PrintWarning("calling ProtectionEnded for building: " + building.ID);
                    prot.ProtectionEnded();
                }
            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            try
            {
                if (!_buildingIdToSpheres.TryGetValue(building.ID, out var spheres))
                    return;

                foreach (var sphere in spheres)
                {
                    if (sphere != null && !sphere.IsDestroyed) sphere.Kill();
                }
            }
            catch(Exception ex) { PrintError(ex.ToString()); }
        }

        private IEnumerator StartEndingRaidProtection(BuildingManager.Building building)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));


            if (!_protectedBuildings.Contains(building))
            {
                PrintWarning(nameof(StartEndingRaidProtection) + " called on a building that is NOT protected!");
                yield break;
            }

            PrintWarning("building was protected, ending protection...");

            var players = Pool.GetList<BasePlayer>();
            try
            {
                var centerPos = FindCenterOfBuilding(building);

                if (centerPos == Vector3.zero)
                    PrintError(nameof(StartEndingRaidProtection) + " had " + nameof(centerPos) + " as vector3.zero");

                Vis.Entities(centerPos, 30f, players, 131072); //interesting note: if vanished, you aren't picked up by this

                PrintWarning("players in area: " + players.Count);

                var currentTime = 25f;
                var removeAndWaitTime = 5f;

             

                while (currentTime > 0f)
                {
                    var endingMsg = string.Empty;

                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        endingMsg = sb.Clear().Append("This building's raid block is ending in: ").Append(currentTime.ToString("N0")).Append(" seconds!").ToString();
                    }
                    finally { Pool.Free(ref sb); }

                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];
                        if (player != null && player.IsConnected)
                            ShowToast(player, endingMsg, 1);
                        
                    }


                    yield return CoroutineEx.waitForSecondsRealtime(removeAndWaitTime);

                    currentTime -= removeAndWaitTime;
                }

                //actually stop the raid protection
                StopRaidProtection(building);
                PrintWarning("stopped raid protection for building after a player connected");

                for (int i = 0; i < players.Count; i++)
                {
                    var player = players[i];
                    if (player != null && player.IsConnected)
                        ShowToast(player, "Offline Raid protection has ended for this building!", 1);
                }

            }
            finally
            {
                Pool.Free(ref players);
            }

            yield return null;
        }

        private IEnumerator StartRaidProtectionCountdown(BuildingManager.Building building, bool startCoroutineToEndProtection = true, bool init = false)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            if (_buildingIdToCountdownCoroutine.TryGetValue(building.ID, out var coroutine) && coroutine != null)
            {
                PrintWarning(nameof(StartRaidProtectionCountdown) + " was called despite a coroutine in progress: " + building.ID);
                yield break;
            }

            try 
            {
                if (_protectedBuildings.Contains(building))
                {
                    PrintWarning(nameof(StartRaidProtectionCountdown) + " called on a building that is already protected: " + building.ID);
                    yield break;
                }

                if (!IsApplicableForProtection(building))
                {
                    PrintWarning(nameof(StartRaidProtectionCountdown) + " called without ever being applicable: " + building.ID);
                    yield break;
                }

                var timeWaited = 0f;
                var timeToWait = init ? 1f : 600f; //600f!!!

                while (timeWaited < timeToWait)
                {

                    if (!IsApplicableForProtection(building))
                    {
                        PrintWarning("building is no longer applicable, so cancelling");
                        yield break;
                    }

                    timeWaited += 1f;

                    yield return CoroutineEx.waitForSecondsRealtime(1f);
                }

                var info = _protectionData?.GetOrCreateProtectionInfo(building.ID);

                PrintWarning("starting protection for " + building.ID + ", protectiontime is: " + info.ProtectionTime + "  waited: " + timeWaited);

                RaidProtectBuilding(building);

                if ((building?.buildingBlocks?.Count ?? 0) >= 64) //allow smaller bases to remain a bit more hidden
                    SpawnSpheresOnBuilding(building);
                

             


                PrintWarning("protection started! (" + building.ID + ")");

                StartCoroutineToEndProtection(building);
                PrintWarning("started (called method) to end protection after due time");

                yield return null;

            }
            finally
            {
                _buildingIdToCountdownCoroutine.Remove(building.ID);
            }
        }

        private IEnumerator CheckExplosiveStickCoroutine(BaseEntity explosive, BasePlayer player = null, Item item = null)
        {
            if (explosive == null)
                throw new ArgumentNullException(nameof(explosive));

            var parent = explosive.GetParentEntity();

            while (parent == null)
            {
                if (explosive == null || explosive.IsDestroyed)
                    yield break;
                

                parent = explosive.GetParentEntity();

                yield return parent != null ? null : CoroutineEx.waitForSeconds(0.1f);
            }

            var decayEntity = parent as DecayEntity;
            if (decayEntity == null)
            {
                PrintWarning("found entity, but not DecayEntity (" + nameof(CheckExplosiveStickCoroutine) + "!");
                yield break;
            }

            var building = decayEntity.GetBuilding();

            if (building == null)
            {
                PrintWarning("building null!");
                yield break;
            }

            if (!_protectedBuildings.Contains(building))
                yield break;


            if (item != null)
            {
                var dropPos = explosive.transform.position;

                var newItem = ItemManager.Create(item.info, 1, item.skin);
                if (newItem != null && !newItem.Drop(dropPos, Vector3.up * 4.5f, Quaternion.identity))
                {
                    PrintWarning("couldn't drop item!!!");
                    RemoveFromWorld(item);
                }
            }

            explosive.Kill();

            if (player != null && player.IsConnected)
                ShowToast(player, "<size=30><color=yellow>THIS BUILDING IS PROTECTED AS ITS OWNER IS OFFLINE.</color></size>", 1);
        }

        private Coroutine StartRaidProtectionCoroutine(BuildingManager.Building building, bool startCoroutineToEndProtection = true, bool init = false)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));


            if (_buildingIdToCountdownCoroutine.TryGetValue(building.ID, out var routine) && routine != null)
            {
                PrintWarning(nameof(StartRaidProtectionCoroutine) + " called when building ID " + building.ID + " already has one - cancelling!!!");

                _buildingIdToCountdownCoroutine.Remove(building.ID);

                StopCoroutine(routine);
            }

            PrintWarning(nameof(StartRaidProtectionCoroutine) + " for: " + building.ID + ", " + startCoroutineToEndProtection + ", " + init);

            return _buildingIdToCountdownCoroutine[building.ID] = StartCoroutine(StartRaidProtectionCountdown(building, startCoroutineToEndProtection, init));
        }

        private void SendRustPlusNotificationsForProtectedBuildings(ulong ownerId, List<string> players = null) //no idea yet how I really want to handle this - I think maybe a 'buffer' would be best, so wait a half second or so to see if we're about to send another, and if we are, combine them
        {

            PrintWarning(nameof(SendRustPlusNotificationsForProtectedBuildings) + " " + ownerId + " players list count: " + (players?.Count ?? 0));

            var buildingCount = 0;
            var protectionTime = TimeSpan.Zero;

            var sb = Pool.Get<StringBuilder>();
            try
            {

                foreach (var building in _protectedBuildings)
                {
                    var majorityOwner = GetMajorityOwnerFromBuilding(building);

                    if (majorityOwner == 0 || (ownerId != majorityOwner && (players == null || !players.Contains(majorityOwner.ToString()))))
                        continue;
                    

                    var info = _protectionData?.GetOrCreateProtectionInfo(building.ID);
                    if (info == null || info.ProtectionTime <= TimeSpan.Zero)
                        continue;

                    buildingCount++;

                    if (protectionTime <= TimeSpan.Zero) protectionTime = info.ProtectionTime;
                }

                if (buildingCount < 1)
                {
                    PrintWarning(nameof(SendRustPlusNotificationsForProtectedBuildings) + " called, but buildingCount < 1!!: " + ownerId + " (" + (players != null ? string.Join(", ", players) : string.Empty) + ")");
                    return;
                }

                var msgStr = sb.Clear().Append(buildingCount.ToString("N0")).Append(" of your bases ").Append(buildingCount > 1 ? "are" : "is").Append(" now protected by Raid Guardian for ").Append(ReadableTimeSpan(protectionTime)).Append("! (Offline Raid Protection)").ToString(); //english lol

                if (players != null && players.Count > 0)
                {
                    for (int i = 0; i < players.Count; i++)
                        SendRustPlusNotification(ulong.Parse(players[i]), msgStr, "Raid Guardian | PRISM");
                }
                else SendRustPlusNotification(ownerId, msgStr, "Raid Guardian | PRISM");

            }
            finally { Pool.Free(ref sb); }


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

        private bool RaidProtectBuilding(BuildingManager.Building building)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            var prot = _protectionData?.GetOrCreateProtectionInfo(building.ID);
            prot?.ProtectionStarted();
            

            return _protectedBuildings.Add(building);
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private void CancelDamage(HitInfo hitInfo)
        {
            if (hitInfo == null) return;
            hitInfo.damageTypes.Clear();
            hitInfo.PointStart = Vector3.zero;
            hitInfo.HitEntity = null;
            hitInfo.DoHitEffects = false;
            hitInfo.HitMaterial = 0;
        }

        public Vector3 FindCenterOfTransforms(List<Transform> transforms)
        {
            if (transforms == null)
                throw new ArgumentNullException(nameof(transforms));

            if (transforms.Count < 1)
                throw new ArgumentOutOfRangeException(nameof(transforms));
            

            var bound = new Bounds(transforms[0].position, Vector3.zero);

            if (transforms.Count > 1)
            {
                for (int i = 1; i < transforms.Count; i++)
                    bound.Encapsulate(transforms[i].position);
            }
           
            return bound.center;
        }

        public Vector3 FindCenterOfBuilding(BuildingManager.Building building, bool onlyFoundations = false)
        {

            if (building == null)
                throw new ArgumentNullException(nameof(building));

            if (building?.buildingBlocks == null || building.buildingBlocks.Count < 1)
                return Vector3.zero;

            var transforms = Pool.GetList<Transform>();

            try
            {
                foreach (var block in building.buildingBlocks)
                {
                    if (block == null || block.IsDestroyed || (onlyFoundations && !block.ShortPrefabName.Contains("foundation"))) continue;

                    transforms.Add(block.transform);

                }

                if (transforms.Count < 1)
                {
                    PrintError("transforms.count < 1 ?!?!? for " + nameof(FindCenterOfBuilding) + " building: " + building?.ID + " !!!");
                    return Vector3.zero;
                }

                return FindCenterOfTransforms(transforms);
            }
            finally { Pool.FreeList(ref transforms); }
        }

        public float FindRadiusFromTransforms(List<Transform> transforms)
        {
            if (transforms == null)
                throw new ArgumentNullException(nameof(transforms));

            var bound = new Bounds(transforms[0].position, Vector3.zero);

            for (int i = 1; i < transforms.Count; i++)
            {
                bound.Encapsulate(transforms[i].position);
            }

            return bound.extents.magnitude;
        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;

            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);

            if (Physics.Raycast(ray, out var hit, maxDist, coll))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }

            if (coll == -1)
            {
                if (Physics.Raycast(ray, out hit, maxDist, _constructionColl))
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

        #endregion

    }
}