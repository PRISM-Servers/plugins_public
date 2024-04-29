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
using System.Globalization;

namespace Oxide.Plugins
{
    [Info("Legendary Events", "Shady", "0.0.9")]
    internal class LegendaryEvents : RustPlugin
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
        public static LegendaryEvents Instance = null;

        //need to announce, add map marker (done)
        //add legendary items to crates on ship - done, but not for non-locked crates

        //rust plus notif for spawn of legendary ship (done)

        [PluginReference]
        private readonly Plugin Luck;

        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        [PluginReference]
        private readonly Plugin RustPlusExtended;

        private const int LUMBERJACK_SUIT_ID = 861513346;
        private const int SCRUBS_ITEM_ID = -1785231475;
        private const int ARCTIC_SCIENTIST_SUIT_ID = 1107575710;
        private const int HEAVY_SCIENTIST_SUIT_ID = -1772746857;
        private const int ARCTIC_SUIT_ID = -470439097;
        private const int GREEN_SCIENTIST_SUIT_ID = -1958316066;
        private const int NOMAD_SUIT_ID = 491263800;
        private const int ABYSS_DIVERS_SUIT_ID = -797592358;

        private const string FX_MISSING = "assets/bundled/prefabs/fx/missing.prefab";

        private readonly List<int> _suitIds = new(8) { LUMBERJACK_SUIT_ID, SCRUBS_ITEM_ID, ARCTIC_SCIENTIST_SUIT_ID, HEAVY_SCIENTIST_SUIT_ID, ARCTIC_SUIT_ID, GREEN_SCIENTIST_SUIT_ID, NOMAD_SUIT_ID, ABYSS_DIVERS_SUIT_ID };

        private readonly HashSet<int> _ignoreItems = new() { 878301596, -2027988285, -44066823, -1364246987, 62577426, 1776460938, -17123659, 999690781, -810326667, 996757362, 1015352446, 1414245162, -1449152644, -187031121, 1768112091, 1770744540, -602717596, 1223900335, 1036321299, 204970153, 236677901, -399173933, 1561022037, 1046904719, 1394042569, -561148628, 861513346, -1366326648, -1884328185, 1878053256, 550753330, 696029452, 755224797, 1230691307, 1364514421, -455286320, 1762167092, -282193997, 180752235, -1386082991, 70102328, 22947882, 81423963, 1409529282, -1779180711, -277057363, -544317637, 1113514903 };
        private const ulong LEGENDARY_OWNER_ID = 5284911221;

        private static readonly System.Random _rarityRng = new();

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private readonly HashSet<string> _forbiddenTags = new() { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly HashSet<ScientistNPC> _legendaryNPCs = new();

        public CH47HelicopterAIController CurrentLegendaryCH47 { get; set; }

        private void SpawnLegendariesIntoHackableCrate(ItemContainer container, int min = 2, int max = 5)
        {
            PrintWarning(nameof(SpawnLegendariesIntoHackableCrate) + ", " + min + "-" + max);


            var legCount = UnityEngine.Random.Range(min, max);
            for (int i = 0; i < legCount; i++)
            {
                var legItem = Luck?.Call<Item>("CreateRandomLegendaryItem");

                if (legItem == null)
                {
                    PrintError(nameof(legItem) + " null!!!!!");
                    break;
                }

                if (!legItem.MoveToContainer(container) && !legItem.Drop((container?.entityOwner?.transform?.position ?? Vector3.zero), Vector3.up * 2.5f))
                {
                    PrintError("couldn't move OR drop leg item!!!");
                    RemoveFromWorld(legItem);
                    break;
                }
                else PrintWarning("Moved legItem: " + legItem?.info?.shortname + " to hacked crate: " + container);

            }
        }

        private void Init()
        {
            Instance = this;
        }

        private void Unload()
        {
            Instance = null;
        }

        private void OnEntityTakeDamage(BasePlayer player, HitInfo info)
        {
            if (player == null || info?.Initiator is not ScientistNPC scientist) //I love this
                return;

            if (!IsOnLegendaryCargo(scientist))
                return;

            player?.Invoke(() =>
            {
                if (player?.metabolism?.radiation_poison == null)
                    return;

                var totalDmg = info?.damageTypes?.Total() ?? 0f;

                if (totalDmg <= 0.0f)
                    return;


                var radAmt = totalDmg * 1.08f;

                player.metabolism.radiation_poison.Add(radAmt);
            }, 0.01f);
         
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null) return;

            var hackable = entity as HackableLockedCrate;
            hackable?.Invoke(() =>
            {
                if (hackable.OwnerID == LEGENDARY_OWNER_ID)
                    return; //already leggy!

                var parentCargo = hackable?.GetParentEntity() as CargoShip;

                if (parentCargo == null || parentCargo.IsDestroyed || parentCargo.OwnerID != LEGENDARY_OWNER_ID)
                    return;

                PrintWarning("found cargo hacked crate that was not already leggy even though ship is! fixing");
                PopulateLegendaryHackableCrate(hackable.inventory);

                var emitter = GetOrCreateFXEmitter(hackable);

                emitter.FX = FX_MISSING;
                emitter.FXToRunPerInterval = 3;
                emitter.PosLocal = new Vector3(0, 0.33f, 0);

            }, 1f);


            //find a way to populate the corpses for legendary NPCs that spawn around crate
            var sciCorpse = entity as NPCPlayerCorpse;

            var isLegendaryDeath = false;

            if (sciCorpse != null)
            {


                var steamId = sciCorpse?.playerSteamID ?? 0;

                foreach (var npc in _legendaryNPCs)
                {
                    if (npc == null || npc.userID == 0) continue;

                    if (npc.userID == steamId)
                    {
                        isLegendaryDeath = true;
                        break;
                    }

                }

            }

            sciCorpse?.Invoke(() => 
            {

                if (isLegendaryDeath || (sciCorpse?.GetParentEntity()?.OwnerID ?? 0) == LEGENDARY_OWNER_ID)
                {
                    if (sciCorpse == null || sciCorpse.IsDestroyed || sciCorpse.gameObject == null || sciCorpse.containers == null || sciCorpse.containers.Length < 1)
                    {
                        PrintWarning("sciCorpse is all screwed up on 1 second invoke!");
                        return;
                    }

                    ItemContainer mainContainer = null;
                    for (int i = 0; i < sciCorpse.containers.Length; i++)
                    {
                        var cont = sciCorpse?.containers[i] ?? null;
                        if (cont?.itemList != null && cont.itemList.Count > 0 && cont.itemList.Count < cont.capacity)
                        {
                            mainContainer = cont;
                            break;
                        }
                    }

                    if (mainContainer == null)
                    {
                        mainContainer = sciCorpse.containers.FirstOrDefault();
                        PrintWarning("mainContainer was null, grabbing default!");
                    }
                    if (mainContainer == null)
                    {
                        PrintWarning("mainContainer still null after FirstOrDefault!!");
                        return;
                    }

                    PopulateLegendaryScientistCorpse(mainContainer);
                }
            }, 0.1f);

            var ch47 = entity as CH47HelicopterAIController;

            if (ch47 != null && UnityEngine.Random.Range(0, 101) <= 40)
            {
                PrintWarning("elite ch47 spawn!");

                MakeCH47Legendary(ch47);

            }


            var cargo = entity as CargoShip;
            if (cargo == null) return;

            var isLeg = UnityEngine.Random.Range(0, 101) <= 40;

            if (!isLeg) return;

            cargo.Invoke(() =>
            {
                MakeCargoLegendary(cargo);
                //announce cargo

                BroadcastToast("A <i><color=orange><size=20>Legendary</size></color></i> cargo ship has entered the map!", 1);
                SimpleBroadcast("A <i><color=orange><size=20>Legendary</size></color></i> cargo ship has entered the map!\nTake it if you dare. You will be rewarded with the best loot!");

                RustPlusExtended?.Call("BroadcastRustPlusNotification", "A Legendary Cargo Ship has entered the map! Take it if you dare...", "LEGENDARY CARGO | PRISM", "LEGENDARY_CARGO_SPAWN");
       

            }, 5f);

        }

        [ChatCommand("cargotest")]
        private void CmdCargoTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length > 0 && args[0].Equals("rptest"))
            {
                RustPlusExtended?.Call("BroadcastRustPlusNotification", "A Legendary Cargo Ship has entered the map! Take it if you dare...", "LEGENDARY CARGO | PRISM", "LEGENDARY_CARGO_SPAWN");


                SendReply(player, "rptest sent");
                return;
            }

            foreach (var ent in BaseNetworkable.serverEntities)
            {
                var ship = ent as CargoShip;
                if (ship != null)
                {
                    PrintWarning("found ship: " + ship);

                    if (ship.OwnerID == LEGENDARY_OWNER_ID) 
                        ship.OwnerID = 0; //forces to remake legendary

                    MakeCargoLegendary(ship);

                    SendReply(player, "made legendary: " + ship);

                    break;
                }
            }

        }

        [ChatCommand("ch47test")]
        private void CmdCH47Test(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length > 0 && args[0].Equals("rptest"))
            {
                RustPlusExtended?.Call("BroadcastRustPlusNotification", "A Legendary Cargo Ship has entered the map! Take it if you dare...", "LEGENDARY CARGO | PRISM", "LEGENDARY_CARGO_SPAWN");


                SendReply(player, "rptest sent");
                return;
            }

            foreach (var ent in BaseNetworkable.serverEntities)
            {
                var ch47 = ent as CH47HelicopterAIController;

                if (ch47 == null)
                    continue;

                PrintWarning("fuond ch47! " + nameof(CmdCH47Test) + ": " + ch47);


                if (ch47.OwnerID == LEGENDARY_OWNER_ID)
                {
                    PrintWarning("was already leg, remake");
                    SendReply(player, "was already legendary, setting ownerid 0 to remake");
                    ch47.OwnerID = 0; //forces to remake legendary
                }

                if (ch47.numCrates < 1)
                    ch47.numCrates++;

                MakeCH47Legendary(ch47);

                SendReply(player, "made legendary: " + ch47);

                break;
            }

        }

        [ChatCommand("legcrate")]
        private void CmdLegendaryCrate(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            var crate = GetLookAtEntity(player, 15f) as HackableLockedCrate;
            if (crate == null)
            {
                SendReply(player, "no " + nameof(HackableLockedCrate) + " found");
                return;
            }

            SendReply(player, "populating");

            int minItemsToSpawn;
            int maxItemsToSpawn;
            int compsToSpawnMin;
            int compsToSpawnMax;

            if (args.Length <= 0 || !int.TryParse(args[0], out minItemsToSpawn))
                minItemsToSpawn = 8;

            if (args.Length < 1 || !int.TryParse(args[1], out maxItemsToSpawn))
                maxItemsToSpawn = 17;

            if (args.Length < 2 || !int.TryParse(args[2], out compsToSpawnMin))
               compsToSpawnMin = 2;

            if (args.Length < 3 || !int.TryParse(args[3], out compsToSpawnMax))
                compsToSpawnMax = 8;

            PopulateLegendaryHackableCrate(crate.inventory, minItemsToSpawn, maxItemsToSpawn, compsToSpawnMin, compsToSpawnMax);

            var emitter = GetOrCreateFXEmitter(crate);

            emitter.FX = FX_MISSING;
            //emitter.PosLocal = new Vector3(0, 0.33f, 0);

            var comp = crate?.GetComponent<CrateScientistsHandler>();

            if (comp == null)
                crate.gameObject.AddComponent<CrateScientistsHandler>();
            

            UnlockCrate(crate);

            SendReply(player, "populated");
        }

        private void CanSamSiteShoot(SamSite site)
        {
            if (site == null) return;

            if (site.OwnerID != 528491 || site?.GetParentEntity() == null) return;



            site.Invoke(() =>
            {
                if (site == null || site.IsDestroyed) return;


                if (site.currentAimDir == Vector3.zero || site.currentTarget == null) return;

                var tubePos = site.tubes[site.currentTubeIndex].position;

                var oldSpeedMult = site.currentTarget.SAMTargetType.speedMultiplier;


                site.FireProjectile(tubePos, site.currentAimDir, oldSpeedMult * 2.5f);
            }, 0.2f);
          
          

        }

        private void SpawnSAMOnCargo(CargoShip ship, Vector3 pos)
        {
            if (ship == null || ship.IsDestroyed || pos == Vector3.zero) return;

            var samSite = (SamSite)GameManager.server.CreateEntity("assets/prefabs/npc/sam_site_turret/sam_static.prefab", pos);

            samSite.OwnerID = 528491;
            samSite.Spawn();

            samSite.SetParent(ship, true, true);

            samSite.InvokeRepeating(() =>
            {
                samSite?.SelfHeal();
            }, 5f, 1f);

        }

        private void MakeCargoLegendary(CargoShip ship)
        {
            if (ship == null || ship.IsDestroyed)
            {
                PrintWarning(nameof(MakeCargoLegendary) + " ship is null/destroyed!!");
                return;
            }


            if (ship.OwnerID == LEGENDARY_OWNER_ID)
            {
                PrintWarning("cargo ship is already legendary?! has ownerid: " + LEGENDARY_OWNER_ID);

                return;
            }

            ship.InvokeRepeating(() =>
            {

                if (ship == null || ship.IsDestroyed) 
                    return;

                var players = Pool.GetList<BasePlayer>();
                try
                {
                    Vis.Entities(ship.CenterPoint(), 180f, players, 131072);

                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];
                        if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected || player?.GetParentEntity() == ship) continue;

                        //send toast too
                        ShowToast(player, "You are nearing a <i><color=orange>Legendary</color></i> cargo ship. Be careful!", 1);
                        SendReply(player, "You are nearing a <i><color=orange>Legendary</color></i> cargo ship. Take precautions! This ship will be much harder to take than regular ones, but will have far better loot (including Legendary items)!");
                    }

                }
                finally { Pool.FreeList(ref players); }

            }, 5f, 15f); //periodically check for players nearing the ship & send warning message

            ship.OwnerID = LEGENDARY_OWNER_ID;


            var scientistNpcs = Pool.GetList<ScientistNPC>();
            try 
            {
                Vis.Entities(ship.WorldSpaceBounds(), scientistNpcs, 131072);

                PrintWarning("vis.entities got: " + scientistNpcs.Count);

                //i wanted to duplicate spawning of these guys but im not sure how to set their pathing yet, so they just stand still (this is why it only duplicates turret guys for now) - should take a look at intellife, as stuff used there can probably fix this issue

                for (int i = 0; i < scientistNpcs.Count; i++)
                {
                    var npc = scientistNpcs[i];

                    if (!npc.ShortPrefabName.Contains("turret")) 
                        continue;

                    if (!(npc.transform.position.y >= 27 && npc.transform.position.y < 28 || (npc.transform.position.y > 22.5f && npc.transform.position.y < 23f))) 
                        continue;

                    //we spawn sams near turret npcs cause lol

                    var desiredAdjustment = npc.transform.position.y >= 27 ? (Vector3.back * 3f) : npc.transform.position.y >= 22 ? (Vector3.forward * 4.33f) : Vector3.zero;

                    if (desiredAdjustment != Vector3.zero)
                    {
                        var adjustedSamPos = npc.transform.position + desiredAdjustment;

                        if (npc.transform.position.y >= 22 && npc.transform.position.y < 27)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                var newPos = adjustedSamPos;
                                newPos.y = 18.54f;

                                newPos += Vector3.back * (54f + (j <= 0 ? j : j * 4f));

                                SpawnSAMOnCargo(ship, newPos);
                            }
                        }

                        SpawnSAMOnCargo(ship, adjustedSamPos);
                    }


                    if (npc.transform.position.y >= 22 && npc.transform.position.y <= 23f)
                    {
                        var frontSamPos = npc.transform.position + Vector3.forward * 8f;
                        frontSamPos.y = 9.154f;

                        SpawnSAMOnCargo(ship, frontSamPos);

                    }


                    
                    //change to use vector3.back/forward later?
                    var adjustedPos = npc.transform.position;
                    adjustedPos.x += UnityEngine.Random.Range(0.675f, 1.275f);
                    adjustedPos.z += UnityEngine.Random.Range(0.375f, 0.775f);

                    var newNpc = GameManager.server.CreateEntity(npc.PrefabName, adjustedPos, npc.ServerRotation);
                    newNpc.Spawn();
                    newNpc.SetParent(ship, true, true);

                 //   newNpc.transform.localPosition = npc.transform.localPosition;
                }



                //spawn sams near boat
                var escapeBoatPoint = ship?.escapeBoatPoint?.transform?.position ?? Vector3.zero;

                var firstSamBoatPos = escapeBoatPoint + Vector3.forward * 4.75f;
                firstSamBoatPos += Vector3.right * 6.5f;

                var secondSamBoatPos = escapeBoatPoint + Vector3.forward * 4f;
                secondSamBoatPos += Vector3.left * 6.5f;

                SpawnSAMOnCargo(ship, firstSamBoatPos);
                SpawnSAMOnCargo(ship, secondSamBoatPos);


                Vis.Entities(ship.WorldSpaceBounds(), scientistNpcs, 131072);

                for (int i = 0; i < scientistNpcs.Count; i++)
                {
                    var npc = scientistNpcs[i];
                    MakeScientistLegendary(npc);
                }


            }
            finally { Pool.FreeList(ref scientistNpcs); }

            AttachLegendaryMapMarkers(ship);

            ship?.Invoke(() =>
            {
                PrintWarning("child check");

                var children = ship?.children;
                if (children == null || children.Count < 1)
                {
                    PrintWarning("ship has NO children?!");
                    return;
                }

                PrintWarning("ship has " + children.Count + " children");

                var foundCrate = false;

                for (int i = 0; i < children.Count; i++)
                {
                    var crate = children[i] as HackableLockedCrate;

                    if (crate == null) continue;

                    foundCrate = true;

                    if (crate.OwnerID == LEGENDARY_OWNER_ID)
                    {
                        PrintWarning("10f invoke crate already had leggy owner id, do not populate");
                        continue;
                    }

                    crate.OwnerID = LEGENDARY_OWNER_ID;

                 

                    PrintWarning("found hacked crate! " + crate);
                    PopulateLegendaryHackableCrate(crate.inventory);

                    var emitter = GetOrCreateFXEmitter(crate);

                    emitter.FX = FX_MISSING;
                    emitter.FXToRunPerInterval = 3;
                    emitter.PosLocal = new Vector3(0, 0.33f, 0);

                }

                PrintWarning("foundCrate: " + foundCrate);

            }, 10f);

         
        }

        private void MakeCH47Legendary(CH47HelicopterAIController ch47)
        {
            if (CurrentLegendaryCH47 != null && !CurrentLegendaryCH47.IsDestroyed)
            {
                PrintWarning(nameof(MakeCH47Legendary) + " called, but returning, as one is already active!!");
                return;
            }

            if (ch47 == null || ch47.IsDestroyed)
            {
                PrintWarning(nameof(MakeCH47Legendary) + " is null/destroyed!!");
                return;
            }


            if (ch47.OwnerID == LEGENDARY_OWNER_ID)
            {
                PrintWarning("ch47 is already legendary?! has ownerid: " + LEGENDARY_OWNER_ID);

                return;
            }


            //make it legendary!
          
            ch47.OwnerID = LEGENDARY_OWNER_ID;

            CurrentLegendaryCH47 = ch47;


            ch47.InvokeRepeating(() =>
            {

                if (ch47 == null || ch47.IsDestroyed)
                    return;

                var players = Pool.GetList<BasePlayer>();
                try
                {
                    var desiredVisPos = ch47.CenterPoint();

                    var desiredY = desiredVisPos.y > 100 ? (desiredVisPos.y - 80f) : desiredVisPos.y; //try to warn players near ground

                    desiredVisPos = new Vector3(desiredVisPos.x, desiredY, desiredVisPos.z);


                    Vis.Entities(ch47.CenterPoint(), 120f, players, 131072);

                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];
                        if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected || player?.GetParentEntity() == ch47) continue;

                        //send toast too
                        ShowToast(player, "A <i><color=orange>Legendary</color></i> Chinook is nearing you. Be careful!", 1);
                        SendReply(player, "A <i><color=orange>Legendary</color></i> Chinook is nearing you. Take precautions! This CH47 will be much more aggressive, but will have far better loot in its dropped crate (including Legendary items)!");
                    }

                }
                finally { Pool.FreeList(ref players); }

            }, 5f, 25f); //periodically check for players nearing the ship & send warning message



            var scientistNpcs = Pool.GetList<ScientistNPC>();
            try
            {
                Vis.Entities(ch47.WorldSpaceBounds(), scientistNpcs, 131072);

                PrintWarning("vis.entities got: " + scientistNpcs.Count);


                Vis.Entities(ch47.WorldSpaceBounds(), scientistNpcs, 131072);

                for (int i = 0; i < scientistNpcs.Count; i++)
                {
                    var npc = scientistNpcs[i];
                    MakeScientistLegendary(npc);
                }


            }
            finally { Pool.FreeList(ref scientistNpcs); }

            AttachLegendaryMapMarkers(ch47);
            /*/
            var children = ch47?.children;
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    var crate = children[i] as HackableLockedCrate;

                    if (crate == null) continue;

                    PrintWarning("found hacked crate! " + crate);
                    PopulateLegendaryHackableCrate(crate.inventory);

                }
            }/*/ // - this is handled when the crate is dropped for ch47

        }

        //move later:

        private void OnLootSpawn(LootContainer container)
        {
            if (container == null) return;

            var hackable = container as HackableLockedCrate;

            if (hackable == null)
                return;

            var cargo = hackable?.GetParentEntity() as CargoShip;

            if (cargo != null && cargo.OwnerID == LEGENDARY_OWNER_ID)
            {
                PrintWarning(nameof(OnLootSpawn) + " for cargo crate");
                PopulateLegendaryHackableCrate(container.inventory);
            }
            else if (hackable.OwnerID == LEGENDARY_OWNER_ID) PopulateLegendaryHackableCrate(container.inventory, 10, 21, 4, 10); //legendary ch47

        }

        private void OnCrateDropped(HackableLockedCrate crate)
        {
            if (crate == null)
                throw new ArgumentNullException(nameof(crate));

            if (CurrentLegendaryCH47 == null || CurrentLegendaryCH47.IsDestroyed)
                return;

            PrintWarning("trying to determine if leggy heli dropped this crate: " + crate);


            //impecable code for finding out if we have the right heli:

            var oldCrateCount = CurrentLegendaryCH47?.numCrates ?? 0;

            if (oldCrateCount < 1)
                return;

            PrintWarning("oldCrateCount > 0");

            crate.Invoke(() =>
            {
                if ((CurrentLegendaryCH47?.numCrates ?? 0) >= oldCrateCount)
                    return;
                
                

                PrintWarning("legendary crate dropped from ch47!!! - crate count is lower now, marking as legendary");

                crate.OwnerID = LEGENDARY_OWNER_ID;

                PrintWarning("populating");

               
                PopulateLegendaryHackableCrate(crate.inventory, 10, 21, 4, 10);

                var emitter = GetOrCreateFXEmitter(crate);

                emitter.FX = FX_MISSING;
                //emitter.PosLocal = new Vector3(0, 0.33f, 0);

                PrintWarning("populated!");

                crate.gameObject.AddComponent<CrateScientistsHandler>();

                PrintWarning("added scientist handler!");


              

            }, 0.05f);

           

        }

        private void AttachLegendaryMapMarkers(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed) return;

            var scientistNpcs = Pool.GetList<ScientistNPC>();
            try 
            {
                Vis.Entities(entity.WorldSpaceBounds(), scientistNpcs, 131072);

                for (int i = 0; i < scientistNpcs.Count; i++)
                {
                    var usePos = scientistNpcs[i]?.transform?.position ?? entity?.CenterPoint() ?? Vector3.zero;

                    var newMap = (MapMarkerGenericRadius)GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", usePos);

                    newMap.radius = 0.075f;


                    newMap.OwnerID = 13371337;

                    var mainColor = new Color(0.6f, 0, 1f, 1f);

                    newMap.color1 = mainColor;
                    newMap.color2 = new Color(0.862f, 0.623f, 0.286f, 0.75f); //outline color
                    newMap.alpha = 1f;

                    newMap.Spawn();
                    newMap.SendUpdate();
                    newMap.SendNetworkUpdateImmediate(true);
                    newMap.SetParent(entity, true, true);

                    newMap.InvokeRepeating(() =>
                    {
                        newMap.SendUpdate();
                        newMap.SendNetworkUpdate();

                    }, 1f, 0.125f);
                }

            }
            finally { Pool.FreeList(ref scientistNpcs); }
        }

        private void MakeScientistLegendary(ScientistNPC npc)
        {
            if (npc == null) 
                throw new ArgumentNullException(nameof(npc));

            if (npc.IsDestroyed)
            {
                PrintWarning("npc is destroyed on " + nameof(MakeScientistLegendary));
                return;
            }

            if (npc.OwnerID == LEGENDARY_OWNER_ID)
            {
                PrintWarning("npc owner is already legendary!");
                return;
            }

            npc.OwnerID = LEGENDARY_OWNER_ID;

            _legendaryNPCs.Add(npc);

            npc.startHealth = npc._maxHealth *= UnityEngine.Random.Range(1.75f, 2.75f);
            npc.SetHealth(npc._maxHealth);

            EquipWithRandomSuit(npc);

        }

        private void EquipWithRandomSuit(ScientistNPC npc)
        {
            if (npc == null || npc.IsDestroyed) return;


            var rng = UnityEngine.Random.Range(0, 101);

            var rngItemId = rng <= 10 ? SCRUBS_ITEM_ID : rng <= 30 ? HEAVY_SCIENTIST_SUIT_ID : rng <= 50 ? LUMBERJACK_SUIT_ID : rng <= 65 ? NOMAD_SUIT_ID : rng <= 70 ? GREEN_SCIENTIST_SUIT_ID : rng <= 80 ? ARCTIC_SUIT_ID : rng <= 85 ? ARCTIC_SCIENTIST_SUIT_ID : HEAVY_SCIENTIST_SUIT_ID;

            //var rngItemId = _suitIds[UnityEngine.Random.Range(0, _suitIds.Count)];

            var rngItem = ItemManager.CreateByItemID(rngItemId);
            if (rngItem == null)
            {
                PrintWarning("rng item null??!?!?!? ID: " + rngItemId);
                return;
            }

            for (int i = 0; i < npc.inventory.containerWear.itemList.Count; i++)
            {
                var wearItem = npc.inventory.containerWear.itemList[i];
                if (wearItem != null) RemoveFromWorld(wearItem);
            }

            if (!rngItem.MoveToContainer(npc.inventory.containerWear))
            {
                PrintWarning("couldn't move to wear?!: " + rngItem);
                RemoveFromWorld(rngItem);
                return;
            }
        }

        private void PopulateLegendaryHackableCrate(ItemContainer container, int minItemsToSpawn = 8, int maxItemsToSpawn = 17, int compsToSpawnMin = 2, int compsToSpawnMax = 8)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (container.capacity < 36)
                container.capacity = 36;

            SpawnLegendariesIntoHackableCrate(container);

            var itemsToSpawn = UnityEngine.Random.Range(minItemsToSpawn, maxItemsToSpawn);

            for (int i = 0; i < itemsToSpawn; i++)
            {
                var itemCategory = GetRandomCategory(ItemCategory.All, ItemCategory.Favourite, ItemCategory.Common, ItemCategory.Search);

                Rust.Rarity itemRarity;
                itemRarity = UnityEngine.Random.Range(0, 101) <= 50 ? GetRandomRarityOrDefault(itemCategory, false) : GetRandomRarityOrDefault(itemCategory, false, UnityEngine.Random.Range(0, 101) <= 66 ? Rust.Rarity.Common : Rust.Rarity.Uncommon);

                var rngItem = GetRandomItem(itemRarity, itemCategory, false, false);

                if (rngItem == null)
                {
                    PrintWarning("rngItem null?!: " + itemCategory + ", " + itemRarity);
                    continue;
                }

                rngItem.amount = Mathf.Clamp((int)(rngItem.info.stackable * UnityEngine.Random.Range(0.066f, 0.175f)), 1, 400);

                if (!rngItem.MoveToContainer(container))
                {
                    PrintWarning("couldn't move to container!!!: " + rngItem);
                    RemoveFromWorld(rngItem);
                    break;
                }

            }

            var compsToSpawn = UnityEngine.Random.Range(compsToSpawnMin, compsToSpawnMax);

            for (int i = 0; i < compsToSpawn; i++)
            {
                var rngComp = GetRandomComponent();

                if (rngComp == null)
                {
                    PrintError("rng comp null!?!?!?");
                    continue;
                }

                rngComp.amount = Mathf.Clamp((int)(rngComp.info.stackable * UnityEngine.Random.Range(0.075f, 0.285f)), 1, 100);

                if (!rngComp.MoveToContainer(container))
                {
                    PrintWarning("couldn't move to container!!!: " + rngComp);
                    RemoveFromWorld(rngComp);
                    break;
                }

            }

            if (UnityEngine.Random.Range(0, 101) <= 50)
            {
                var batteryAmt = UnityEngine.Random.Range(15, 260);

                var batteryItem = ItemManager.CreateByName("battery.small", batteryAmt);
                if (!batteryItem.MoveToContainer(container))
                {
                    RemoveFromWorld(batteryItem);
                    PrintWarning("couldn't move to container!!!: " + batteryItem);
                }
            }

            if (UnityEngine.Random.Range(0, 101) <= 33)
            {
                var charmsToSpawn = UnityEngine.Random.Range(1, 4);

                for (int i = 0; i < charmsToSpawn; i++)
                {
                    var forceXpSuper = UnityEngine.Random.Range(0, 101) <= 9;
                    var forceItemSuper = UnityEngine.Random.Range(0, 101) <= 7;

                    var charmItem = Luck?.Call<Item>("CreateRandomCharm", false, false, forceXpSuper, forceItemSuper, 0.6f);

                    if (charmItem != null && !charmItem.MoveToContainer(container))
                    {
                        PrintWarning("failed to move charm item to hacked crate!");

                        RemoveFromWorld(charmItem);
                    }
                    else PrintWarning("charmItem not null, forceXp: " + forceXpSuper + ", item: " + forceItemSuper + ", charm text: " + charmItem.text);

                }

            }

            //battery spawns, scrap spawns?

        }

        private void PopulateLegendaryScientistCorpse(ItemContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            var legendaryRng = UnityEngine.Random.Range(0, 101);
            if (legendaryRng <= 5)
            {
                var luckItem = Luck?.Call<Item>("CreateRandomLegendaryItem");
                if (luckItem != null)
                {
                    PrintWarning("luckItem/leg not null!, got: " + luckItem + ", name: " + luckItem.name);


                    if (!luckItem.MoveToContainer(container))
                    {
                        PrintWarning("luckItem couldn't be moved?!");
                        RemoveFromWorld(luckItem);
                    }

                }
            }

            var itemsToSpawn = UnityEngine.Random.Range(7, 12);

            for (int i = 0; i < itemsToSpawn; i++)
            {
                var itemCategory = GetRandomCategory(ItemCategory.All, ItemCategory.Favourite, ItemCategory.Common, ItemCategory.Search);

                Rust.Rarity itemRarity;
                itemRarity = UnityEngine.Random.Range(0, 101) <= 50 ? GetRandomRarityOrDefault(itemCategory, false) : GetRandomRarityOrDefault(itemCategory, false, UnityEngine.Random.Range(0, 101) <= 66 ? Rust.Rarity.Common : Rust.Rarity.Uncommon);

                var rngItem = GetRandomItem(itemRarity, itemCategory, false, false);

                if (rngItem == null)
                {
                    PrintWarning("rngItem null?!: " + itemCategory + ", " + itemRarity);
                    continue;
                }

                rngItem.amount = Mathf.Clamp((int)(rngItem.MaxStackable() * UnityEngine.Random.Range(0.07f, 0.125f)), 1, 200);

                if (!rngItem.MoveToContainer(container))
                {
                    PrintWarning("couldn't move to container!!!: " + rngItem);
                    RemoveFromWorld(rngItem);
                    break;
                }

            }

            var compsToSpawn = UnityEngine.Random.Range(2, 5);

            for (int i = 0; i < compsToSpawn; i++)
            {
                var rngComp = GetRandomComponent();

                if (rngComp == null)
                {
                    PrintWarning("rng comp null!?!?!?");
                    continue;
                }

                rngComp.amount = Mathf.Clamp((int)(rngComp.MaxStackable() * UnityEngine.Random.Range(0.07f, 0.19f)), 1, 300);

                if (!rngComp.MoveToContainer(container))
                {
                    PrintWarning("couldn't move to container!!!: " + rngComp);
                    RemoveFromWorld(rngComp);
                    break;
                }

            }

            if (UnityEngine.Random.Range(0, 101) <= 25)
            {
                var batteryAmt = UnityEngine.Random.Range(20, 220);

                var batteryItem = ItemManager.CreateByName("battery.small", batteryAmt);
                if (!batteryItem.MoveToContainer(container))
                {
                    RemoveFromWorld(batteryItem);
                    PrintWarning("couldn't move to container!!!: " + batteryItem);
                }
            }

            //battery spawns, scrap spawns?

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

                var itemInfo = components[UnityEngine.Random.Range(0, components.Count)];
                var component = ItemManager.Create(itemInfo, 1);

                component.amount = Mathf.Clamp(amount, 1, enforceStackLimits ? component.MaxStackable() : int.MaxValue);

                return component;
            }
            finally { Pool.FreeList(ref components); }
        }

        private Vector3 SpreadVector(Vector3 vec, float spread = 1.5f) { return Quaternion.Euler(UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f), UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f), UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f)) * vec; }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

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

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;

            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);

            if (UnityEngine.Physics.Raycast(ray, out hit, maxDist, coll))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }

            if (coll == -1)
            {
                if (UnityEngine.Physics.Raycast(ray, out hit, maxDist))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }

            return null;
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
                    {
                        rarity = itemCats?.Select(p => p.rarity)?.Min() ?? Rust.Rarity.None;
                    }

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

        private bool IsOnCargo(BaseEntity entity)
        {
            return (entity?.GetParentEntity()?.prefabID ?? 0) == 3234960997;
        }

        private bool IsOnLegendaryCargo(BaseEntity entity)
        {
            var parent = entity?.GetParentEntity();

            return parent?.prefabID == 3234960997 && parent?.OwnerID == LEGENDARY_OWNER_ID;
        }

        private MonumentInfo _largeOilRig = null;
        public MonumentInfo LargeOilRig
        {
            get
            {
                if (_largeOilRig == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if (mon.displayPhrase.english.Contains("large oil rig", CompareOptions.OrdinalIgnoreCase))
                        {
                            _largeOilRig = mon;
                            break;
                        }
                    }
                }

                if (_largeOilRig == null)
                    PrintError(nameof(_largeOilRig) + " could not be found!!!");

                return _largeOilRig;
            }
        }

        private MonumentInfo _oilRig = null;
        public MonumentInfo SmallOilRig
        {
            get
            {
                if (_oilRig == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if (mon != LargeOilRig && mon.displayPhrase.english.Contains("oil rig", CompareOptions.OrdinalIgnoreCase))
                        {
                            _oilRig = mon;
                            break;
                        }
                    }
                }

                if (_oilRig == null)
                    PrintError(nameof(_oilRig) + " could not be found!!!");

                return _oilRig;
            }
        }

        private bool IsNearOilRig(BaseEntity entity) //yes, this code CAN be improved.
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return Vector3.Distance(SmallOilRig.transform.position, entity.transform.position) < 200 || Vector3.Distance(LargeOilRig.transform.position, entity.transform.position) < 200;
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
                    if (!RunAtPosition)
                        Effect.server.Run(FX, Entity, BoneID, PosLocal, NormLocal);
                    else Effect.server.Run(FX, Entity.transform.position, Vector3.zero);
                }
            }

            public void DoDestroy()
            {
                try { CancelInvoke(EmitFX); }
                finally { GameObject.Destroy(this); }
            }
        }

        private class CrateScientistsHandler : FacepunchBehaviour
        {

            public HashSet<ScientistNPC> Scientists { get; set; } = new HashSet<ScientistNPC>();

            public HackableLockedCrate Crate { get; set; }

            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(CrateScientistsHandler) + "." + nameof(Awake));

                Crate = GetComponent<HackableLockedCrate>();

                if (Crate == null)
                {
                    Interface.Oxide.LogError("Crate null on Awake!!!");
                    DoDestroy();
                    return;
                }

                Invoke(SpawnScientists, 5f);

            }

            public void DoDestroy()
            {
                GameObject.Destroy(this);
            }

            public void SpawnScientists() //rough draft for testing
            {
                Interface.Oxide.LogWarning(nameof(SpawnScientists));

                var cratePos = Crate?.transform?.position ?? Vector3.zero;
                if (cratePos == Vector3.zero)
                {
                    Interface.Oxide.LogError(nameof(cratePos) + " is zero!!!");
                    return;
                }

                cratePos = new Vector3(cratePos.x, cratePos.y + 0.35f, cratePos.z);

                var spawnPos1 = cratePos + Vector3.left * 1.75f;
                var spawnPos2 = cratePos + Vector3.right * 1.75f;
                var spawnPos3 = cratePos + Vector3.forward * 1.75f;
                var spawnPos4 = cratePos + Vector3.back * 1.75f;
                var spawnPos5 = cratePos + Vector3.back * 0.5f + Vector3.left * 4f;
                var spawnPos6 = cratePos + Vector3.forward * 0.5f + Vector3.right * 4f;

                Interface.Oxide.LogWarning("spawn positions: " + spawnPos1 + ", " + spawnPos2 + ", " + spawnPos3 + ", " + spawnPos4);

                // var npc = GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roam.prefab", spawnPoint) as ScientistNPC;
                // npc.Spawn();


                //gunner guys don't move!
                //assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_ch47_gunner.prefab


                //i have no idea what tethered actually does, it seems to do nothing special on its own

                var npc1 = (ScientistNPC)GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roamtethered.prefab", spawnPos1);
                var npc2 = (ScientistNPC)GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roamtethered.prefab", spawnPos2);
                var npc3 = (ScientistNPC)GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roamtethered.prefab", spawnPos3);
                var npc4 = (ScientistNPC)GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roamtethered.prefab", spawnPos4);

                var npc5 = (ScientistNPC)GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_ch47_gunner.prefab", spawnPos5);
                var npc6 = (ScientistNPC)GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_ch47_gunner.prefab", spawnPos6);


                npc1.spawnPos = spawnPos1;
                npc2.spawnPos = spawnPos2;

                npc1.Spawn();
                npc2.Spawn();
                npc3.Spawn();
                npc4.Spawn();
                npc5.Spawn();
                npc6.Spawn();

                Scientists.Add(npc1);
                Scientists.Add(npc2);
                Scientists.Add(npc3);
                Scientists.Add(npc4);
                Scientists.Add(npc5);
                Scientists.Add(npc6);

             
                foreach(var scientist in Scientists)
                {
                    Instance?.MakeScientistLegendary(scientist);

                    //do this to prevent them from leaving suddenly
                    scientist.InvokeRepeating(() =>
                    {
                        if (scientist == null || scientist.IsDestroyed || scientist.IsDead() || scientist.Brain == null) return;

                        if (scientist.lastAttackedTime < 0f || scientist.lastAttacker == null)
                            scientist.Brain.SwitchToState(AIState.Idle, 0);


                    }, 1.25f, 1f);

                  

                }

            }

        }

    }
}