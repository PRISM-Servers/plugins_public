using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Cars", "Shady", "1.1.0", ResourceId = 0)]
    class Cars : RustPlugin
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
        public static string PluginNamePrefix { get { return ins?.Name + ":"; } }
        private readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        public static readonly int groundWorldLayer = LayerMask.GetMask("World", "Default", "Terrain");
        FieldInfo curList = typeof(InvokeHandler).GetField("curList", (BindingFlags.Instance | BindingFlags.NonPublic));
        CarData carData;
        public static Cars ins;
        private List<CollectibleEntity> collectibles = new List<CollectibleEntity>();


        public static readonly int MaxFuel = 500;
        public static readonly int MaxSpawnCars = 50;

        #region Init
        private void Init()
        {
            ins = this;
            if ((carData = Interface.GetMod().DataFileSystem.ReadObject<CarData>("CarData")) == null) carData = new CarData();
            string[] carCmds = { "car", "cars" };
            AddCovalenceCommand(carCmds, "cmdCarHelp");
			AddCovalenceCommand("carspawn", "cmdCarSpawn");
            AddCovalenceCommand("fcs", "cmdFCS");
        }

        protected override void LoadDefaultConfig() => SaveConfig();
        void OnServerInitialized()
        {
            var cars = UnityEngine.Object.FindObjectsOfType<BasicCar>();

            if (cars != null && cars.Length > 0)
            {
                var infoSB = new StringBuilder();
                for (int i = 0; i < cars.Length; i++)
                {
                    var car = cars[i];
                    if (car == null) continue;
                    var carSpeed = car?.GetComponent<CarSpeed>() ?? null;
                    if (carSpeed != null) carSpeed.DoDestroy();
                    carSpeed = car.gameObject.AddComponent<CarSpeed>();
                    var carCol = car?.GetComponent<CarCol>() ?? null;
                    if (carCol != null) UnityEngine.Object.Destroy(carCol);
                    carCol = car.gameObject.AddComponent<CarCol>();
                    var carEngine = car?.GetComponent<CarEngine>() ?? null;
                    if (carEngine != null) carEngine.DoDestroy();
                    carEngine = car.gameObject.AddComponent<CarEngine>();
                    var carNetID = car?.net?.ID.Value ?? 0;
                    var carInfo = carData?.carInfos?.Where(p => p.NetID == carNetID)?.FirstOrDefault() ?? null;
                    if (carInfo != null)
                    {
                        carEngine.Fuel = carInfo.Fuel;
                        carEngine.Oil = carInfo.Oil;
                        carEngine.HasBattery = carInfo.HasBattery;
                        if (carInfo.Turbo)
                        {
                            var vel = car?.GetComponent<CarVelocity>() ?? null;
                            if (vel == null) vel = car.gameObject.AddComponent<CarVelocity>();
                        }
                    }
                }
                if (infoSB.Length > 0) PrintWarning(infoSB.ToString().TrimEnd());
            }
            var cols = UnityEngine.Object.FindObjectsOfType<CollectibleEntity>();
            if (cols != null && cols.Length > 0) collectibles = cols?.ToList() ?? null;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null) continue;
                var pCar = GetCarFromSeat(player?.GetMounted());
                if (pCar != null)
                {
                    LockInventory(player);
                    SpeedometerGUI(player, pCar);
                }
            }
        }

        void OnNewSave(string save)
        {
            var saveFile = save;
            var getSave = GetConfig("Save", string.Empty);
            if (getSave != saveFile)
            {
                Config["Save"] = saveFile;
                if (string.IsNullOrEmpty(getSave)) PrintWarning("getSave is null/empty, probably first run.");
                else
                {
                    PrintWarning("getSave: " + getSave + " != " + saveFile + ", updating & deleting car spawns");
                    if (carData?.carInfos != null) carData.carInfos.Clear();
                }
                SaveConfig();
            }
        }

        void Unload()
        {
            try
            {
                var invokes = (InvokeList == null) ? null : InvokeList.Where(p => (p.Key.action?.Target ?? null).ToString() == Object.ToString())?.ToList() ?? null;
                if (invokes != null && invokes.Count > 0)
                {
                    for (int i = 0; i < invokes.Count; i++)
                    {
                        var inv = invokes[i];
                        InvokeHandler.CancelInvoke(inv.Key.sender, inv.Key.action);
                    }
                }
            }
            catch(Exception ex) { PrintError(ex.ToString()); }
          
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null) continue;
                var mount = GetCarFromSeat(player?.GetMounted());
                if (mount != null && (mount is BasicCar))
                {
                    CuiHelper.DestroyUi(player, "Speedometer");
                    UnlockInventory(player);
                }
            }
            SaveCarEngines();
            foreach(var entity in BaseNetworkable.serverEntities)
            {
                if (entity == null || !(entity is BasicCar)) continue;
                var carSpeed = entity?.GetComponent<CarSpeed>() ?? null;
                if (carSpeed != null) carSpeed.DoDestroy();

                var carCol = entity?.GetComponent<CarCol>() ?? null;
                if (carCol != null) UnityEngine.Object.Destroy(carCol);

                var carEngine = entity?.GetComponent<CarEngine>() ?? null;
                if (carEngine != null) carEngine.DoDestroy();
                var carVel = entity?.GetComponent<CarVelocity>() ?? null;
                if (carVel != null) carVel.DoDestroy();
                var pos = entity?.transform?.position ?? Vector3.zero;
                var isSpawnCar = carData?.spawnInfos?.Any(p => Vector3.Distance(pos, p.Position) < 3) ?? false;
                if (isSpawnCar)
                {
                    PrintWarning("Cleaning up spawned car: " + (entity?.net?.ID.Value ?? 0) + ", at pos: " + pos + " (very near spawn point!)");
                    if (!entity.IsDestroyed) entity.Kill();
                }
            }

            SaveCarData();
        }
        #endregion

        #region Commands

        [ChatCommand("carown")]
        void cmdCOwn(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var ent = GetLookAtEntity(player, 10f, 8192) as BasicCar;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }
            SendReply(player, "Car OwnerID: " + ent.OwnerID);
        }

        [ChatCommand("cvel")]
        void cmdCVel(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var ent = GetLookAtEntity(player, 10f, 8192) as BasicCar;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }

            var rigid = ent?.GetComponent<Rigidbody>() ?? null;
            if (rigid != null)
            {
                rigid.velocity = rigid.velocity + GetPlayerRotation(player) * 20; 
                rigid.AddForce(GetPlayerRotation(player) * 400, ForceMode.Force);
            }
            if (ent.GetComponent<CarVelocity>() == null) ent.gameObject.AddComponent<CarVelocity>();

        }

        [ChatCommand("cspeed")]
        void cmdCSpeed(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1F;
            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out layer))
                {
                    SendReply(player, "Bad speed: " + args[0]);
                    return;
                }
            }
            var ent = GetLookAtEntity(player, 10f, 8192) as BasicCar;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }
            var vel = ent?.GetComponent<CarVelocity>() ?? null;
            if (vel == null) vel = ent.gameObject.AddComponent<CarVelocity>();
            var prev = vel.BaseScalar;
            vel.BaseScalar = layer;
            SendReply(player, "Set base scalar to: " + vel.BaseScalar + ", previous: " + prev);

        }


        private void cmdCarHelp(IPlayer player, string command, string[] args)
        {
            SendReply(player, "-<color=orange>You can find cars in various areas of the map.</color>\n\n-<color=cyan>Cars require fuel, you can fuel them up by putting Low Grade Fuel into your belt/hotbar and holding right click while looking up the back (rear) left (driver side) tire.</color>");
            SendReply(player, "-<color=lime>You can repair a car using a hammer, it will require Metal Fragments. It will also require High Quality Metal if the health is below half.</color>");
        }

        private void cmdCarSpawn(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var pObj = player?.Object as BasePlayer;
            if (pObj == null)
            {
                SendReply(player, "This can only be ran as a player!");
                return;
            }
            var badArgs = "Incorrect args, try:\nadd (auto picks number)\nremove <SpawnNumber>\nclear\nspawnnow";
            if (args.Length < 1)
            {
                SendReply(player, badArgs);
                return;
            }
            var arg0Lower = args[0].ToLower();
            if (arg0Lower == "clear")
            {
                carData.spawnInfos.Clear();
                SendReply(player, "Cleared all spawns");
                return;
            }
            if (arg0Lower == "spawnnow")
            {
                SpawnAllCars(true);
                SendReply(player, "Called SpawnAllCars()");
                return;
            }
            if (arg0Lower == "cleanup")
            {
                SendReply(player, "Removing all cars in water...");
                var sedans = BaseNetworkable.serverEntities?.Where(p => p is BasicCar)?.ToList() ?? null;
                var killed = 0;
                if (sedans != null && sedans.Count > 0)
                {
                 
                    for (int i = 0; i < sedans.Count; i++)
                    {
                        var sedan = sedans[i];
                        if (sedan == null || sedan.IsDestroyed || sedan.gameObject == null || sedan.transform == null) continue;
                        var car = sedan as BasicCar;
                        if (car == null) continue;
                        var carCenter = car?.CenterPoint() ?? Vector3.zero;
                        var leftWheelPos = car?.wheels[0]?.wheel?.position ?? Vector3.zero;
                        var rightWheelPos = car?.wheels[1]?.wheel?.position ?? Vector3.zero;

                        var leftDepth = WaterLevel.GetWaterDepth(leftWheelPos, true, true);
                        var rightDepth = WaterLevel.GetWaterDepth(rightWheelPos, true, true);
                        var waterDepth = WaterLevel.GetWaterDepth(carCenter, true, true);
                        if ((leftDepth >= 0.4 && rightDepth >= 0.4) || (rightDepth >= 0.51 || leftDepth >= 0.51) || waterDepth >= 0.78)
                        {
                            car.Kill();
                            killed++;
                        }
                    }
                  
                }
                SendReply(player, "Cleaned up " + killed.ToString("N0") + " cars.");
                return;
            }
           
              
          
            var oldCount = carData?.spawnInfos?.Count ?? 0;
            var pos = pObj?.transform?.position ?? Vector3.zero;
            if (arg0Lower == "add")
            {
                var chance = 100f;
                if (args.Length > 1)
                {
                    if (!float.TryParse(args[1], out chance))
                    {
                        SendReply(player, "Not a float: " + chance);
                        return;
                    }
                }
                var newInfo = new SpawnInfo(pos, chance);
                carData.spawnInfos.Add(newInfo);
                SendReply(player, "Added spawn " + (oldCount + 1).ToString("N0") + " at position: " + pos + " with chance: " + chance + "%");
            }
            if (arg0Lower == "remove")
            {
                if (args.Length < 2)
                {
                    SendReply(player, badArgs);
                    return;
                }
                int spawn;
                if (!int.TryParse(args[1], out spawn))
                {
                    SendReply(player, "Not an integer: " + args[1]);
                    return;
                }
                if (spawn > oldCount || spawn < 1)
                {
                    SendReply(player, "No spawn found with number: " + spawn);
                    return;
                }
                var getSpawn = carData.spawnInfos[spawn];
                SendReply(player, "Removed spawn: " + spawn + ", at: " + getSpawn.Position + " (" + getSpawn.Chance + "%)");
                carData.spawnInfos.Remove(getSpawn);
            }
        }



        private void cmdFCS(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var pObj = player?.Object as BasePlayer;
            if (pObj == null)
            {
                SendReply(player, "This can only be ran as a player!");
                return;
            }
            var plyPos = pObj?.transform?.position ?? Vector3.zero;
            if (plyPos == Vector3.zero) return;
            var sedan = GameManager.server.CreateEntity("assets/content/vehicles/sedan_a/sedantest.entity.prefab", plyPos);
            sedan.Spawn();
            var selfDrive = false;

            if (sedan != null)
            {
                var carEngine = sedan?.GetComponent<CarEngine>() ?? null;
                if (carEngine != null)
                {
                    carEngine.HasBattery = true;
                    carEngine.Fuel = MaxFuel;
                    SendReply(player, "Spawned sedan with battery and max fuel: " + MaxFuel.ToString("N0"));
                }
                else SendReply(player, "Could not get CarEngine");
                var car = sedan.GetComponent<BasicCar>();
                if (car == null)
                {
                    SendReply(player, "BasicCar was null!!");
                    return;
                }
                if (selfDrive) car.motorForceConstant *= 100;
                for (int i = 0; i < car.wheels.Length; i++)
                {
                    var wheel = car.wheels[i];
                    if (wheel != null)
                    {
                        wheel.wheelCollider.motorTorque *= 10;
                        wheel.wheelCollider.attachedRigidbody.maxAngularVelocity *= 5;
                        wheel.wheelCollider.attachedRigidbody.maxDepenetrationVelocity *= 5;
                    }
                }
               // car.myRigidBody.maxDepenetrationVelocity *= 5;
             //   car.myRigidBody.maxAngularVelocity *= 5;
            }
        }

        #endregion

        #region Hooks
        Dictionary<string, string> lastMPS = new Dictionary<string, string>();
        Dictionary<string, float> lastSpeedGUI = new Dictionary<string, float>();
        Dictionary<string, float> lastFuelTime = new Dictionary<string, float>();
        Dictionary<string, float> lastFuelFX = new Dictionary<string, float>();
        Dictionary<string, string> lastFuelGUI = new Dictionary<string, string>();
        Dictionary<string, string> lastHPGUI = new Dictionary<string, string>();
        Dictionary<string, float> lastInvCheck = new Dictionary<string, float>();
        Dictionary<string, Timer> dismountTimer = new Dictionary<string, Timer>();
        void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (input == null || player == null) return;
            if (!player.IsAlive() || !player.IsConnected || player.IsSleeping() || player.IsWounded()) return;
            var activeItem = player?.GetActiveItem() ?? null;
            var playerPos = player?.transform?.position ?? Vector3.zero;
            var eyePos = player?.eyes?.position ?? Vector3.zero;
            var playerRot = GetPlayerRotation(player);
    
            if (activeItem != null && input.IsDown(BUTTON.FIRE_SECONDARY))
            {
                var getCar = GetLookAtEntity(player, 1.85f, 8192) as BasicCar;
                if (getCar == null) return;
                var carEngine = getCar?.GetComponent<CarEngine>() ?? null;
                if (carEngine == null) return;
                if (activeItem.info.shortname == "lowgradefuel")
                {
                    float lastFuel;
                    if (!lastFuelTime.TryGetValue(player.UserIDString, out lastFuel) || (Time.realtimeSinceStartup - lastFuel) >= UnityEngine.Random.Range(0.5f, 0.8f))
                    {
                        if (getCar != null)
                        {
                            var driverRearTire = getCar.wheels[2];
                            var driverRearPos = driverRearTire?.wheel?.position ?? Vector3.zero;

                            RaycastHit info;
                            if (!Physics.Raycast(new Ray(eyePos, playerRot), out info, 1.5f, 8192)) return; //|| Vector3.Distance(info.point, driverRearPos) > 1.1) return;
                            lastFuelTime[player.UserIDString] = Time.realtimeSinceStartup;
                            var carSpeed = getCar?.GetComponent<CarSpeed>() ?? null;
                            if (carEngine != null)
                            {
                                if (carEngine.Fuel >= MaxFuel) SendReply(player, "This car is already full!");
                                else if (carSpeed != null && carSpeed.mps > 0.2f) SendReply(player, "You cannot re-fuel while the car is moving!");
                                else if (player.isMounted) SendReply(player, "You cannot re-fuel right now!");
                                else
                                {
                                    var itemID = activeItem?.info?.itemid ?? 0;
                                    var fuelGive = 5;
                                    var takeStr = (activeItem.amount < 5) ? activeItem.amount.ToString() : "5";
                                    if (activeItem.amount > 5)
                                    {
                                        activeItem.amount -= 5;
                                        activeItem.MarkDirty();
                                    }
                                    else
                                    {
                                        fuelGive = activeItem.amount;
                                        RemoveFromWorld(activeItem);
                                    }
                                    player.SendConsoleCommand("note.inv " + itemID + " -" + takeStr);
                                    carEngine.Fuel += fuelGive;
                                    if (carEngine.Fuel > MaxFuel) carEngine.Fuel = MaxFuel;
                                    SendReply(player, "Car fuel: " + carEngine.Fuel.ToString("N0") + "/" + MaxFuel.ToString("N0"));
                                    float lastFX;
                                    if (!lastFuelFX.TryGetValue(player.UserIDString, out lastFX) || (Time.realtimeSinceStartup - lastFX) > 3.14)
                                    {
                                        if (lastFX <= 0f || (Time.realtimeSinceStartup - lastFX) > 7) Effect.server.Run("assets/prefabs/food/water jug/effects/water-jug-open-cap.prefab", player.CenterPoint(), Vector3.zero);
                                        Effect.server.Run("assets/prefabs/food/water jug/effects/water-jug-fill-container.prefab", player.CenterPoint(), Vector3.zero);
                                        lastFuelFX[player.UserIDString] = Time.realtimeSinceStartup;
                                    }
                                }
                            }
                        }
                    }
                }
                if (activeItem.info.shortname == "battery.small" && (activeItem?.name ?? string.Empty) == "Car Battery")
                {
                    var driverFrontTire = getCar.wheels[0];
                    var passengerFrontTire = getCar.wheels[1];
                    var dfDist = Vector3.Distance((driverFrontTire?.wheel?.position ?? Vector3.zero), playerPos);
                    var pfDist = Vector3.Distance((passengerFrontTire?.wheel?.position ?? Vector3.zero), playerPos);
                    if (pfDist > 2.2 || dfDist > 2.2) return;
                    if (carEngine.HasBattery)
                    {
                        SendReply(player, "This car already has a battery!");
                        return;
                    }

                    player.SendConsoleCommand("note.inv " + activeItem.info.itemid + " -" + activeItem.amount + " \"" + activeItem.name + "\"");
                    RemoveFromWorld(activeItem);
                    carEngine.HasBattery = true;
                    SendReply(player, "Installed car battery!");
                }
            }
            var mountSeat = player?.GetMounted() ?? null;
            var mount = GetCarFromSeat(mountSeat);
            if (mount != null && mount is BasicCar)
            {
                if (mountSeat.ShortPrefabName == "driverseat" && input.WasJustPressed(BUTTON.USE))
                {
                    var pos = mount?.transform?.position ?? Vector3.zero;
                    for (int i = 0; i < UnityEngine.Random.Range(1, 5); i++) Effect.server.Run("assets/prefabs/locks/keypad/effects/lock.code.denied.prefab", pos, Vector3.zero);
                }
                if ((player?.modelState?.waterLevel ?? 0) >= 0.4)
                {
                    Timer dmTimer = null;
                    if (!dismountTimer.TryGetValue(player.UserIDString, out dmTimer) || dmTimer == null)
                    {
                        dmTimer = timer.Once(UnityEngine.Random.Range(2.3f, 3f), () =>
                        {
                            var findTimer = dismountTimer?.Where(p => p.Value == dmTimer)?.FirstOrDefault().Key ?? null;
                            if (!string.IsNullOrEmpty(findTimer)) dismountTimer.Remove(findTimer);

                            if (player == null || mount == null) return;
                            if ((player?.modelState?.waterLevel ?? 0) >= 0.435) mount.DismountAllPlayers();
                            

                        });
                        dismountTimer[player.UserIDString] = dmTimer;
                    }
                }
                float lastSpeed;
                if (!lastSpeedGUI.TryGetValue(player.UserIDString, out lastSpeed) || (Time.realtimeSinceStartup - lastSpeed) > 0.6)
                {
                    var speed = (mount?.GetComponent<CarSpeed>()?.mps ?? 0).ToString("N0");
                    var fuel = (mount?.GetComponent<CarEngine>()?.Fuel ?? 0).ToString("N0");
                    var HP = (mount?.Health() ?? 0f).ToString("N0");
                    var outSpeed = string.Empty;
                    var outFuel = string.Empty;
                    var outHP = string.Empty;
                    if (!lastMPS.TryGetValue(player.UserIDString, out outSpeed) || outSpeed != speed || !lastFuelGUI.TryGetValue(player.UserIDString, out outFuel) || outFuel != fuel || !lastHPGUI.TryGetValue(player.UserIDString, out outHP) || outHP != HP)
                    {
                        SpeedometerGUI(player, mount);
                        lastMPS[player.UserIDString] = speed;
                        lastFuelGUI[player.UserIDString] = fuel;
                        lastHPGUI[player.UserIDString] = HP;
                    }

                    lastSpeedGUI[player.UserIDString] = Time.realtimeSinceStartup;
                }
            }
        }

        object OnHammerHit(BasePlayer player, HitInfo hitInfo)
        {
            if (player == null || hitInfo == null) return null;
            if (hitInfo?.HitEntity != null && hitInfo.HitEntity.ShortPrefabName.Contains("sedan") && (player?.inventory != null))
            {
                var bce = hitInfo?.HitEntity as BaseCombatEntity;
                if (bce == null) return null;
                if (bce.Health() >= bce.MaxHealth()) return null;
                if (bce.SecondsSinceAttacked < 5 && !((bce.lastAttacker is BuildingBlock) || bce.lastAttacker is TreeEntity)) return null;
                var items = new List<ItemAmount>();
                if (bce.healthFraction <= 0.48)
                {
                    var hqm = new ItemAmount();
                    hqm.amount = Mathf.Clamp(4 / bce.healthFraction, 4, 125);
                    hqm.startAmount = hqm.amount;
                    hqm.itemDef = ItemManager.FindItemDefinition("metal.refined");
                    items.Add(hqm);
                }
                var frags = new ItemAmount();
                frags.amount = Mathf.Clamp(25 / bce.healthFraction, 25, 750);
                frags.startAmount = frags.amount;
                frags.itemDef = ItemManager.FindItemDefinition("metal.fragments");
                items.Add(frags);
                if (!items.All(p => player.inventory.GetAmount(p.itemid) >= p.amount))
                {
                    Effect.server.Run("assets/prefabs/deployable/reactive target/effects/bullseye.prefab", hitInfo.HitEntity.CenterPoint(), Vector3.zero);
                    return false;
                }
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    player.inventory.Take(null, item.itemid, (int)item.amount);
                    player.SendConsoleCommand("note.inv " + item.itemid + " -" + (int)item.amount);
                }

                //bce.healthFraction += UnityEngine.Random.Range(0.1f, 0.15f);
                bce.SendNetworkUpdate();
            }
            return null;
        }

        void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null) return;
            var car = entity as BasicCar;
            if (car == null) return;
            var mountPly = GetMounted(car);
            if (mountPly == null) return;

            CuiHelper.DestroyUi(mountPly, "Speedometer");
            UnlockInventory(mountPly);
        }

        void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;
            if (entity is BasicCar)
            {
                var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
                if (majDmg == Rust.DamageType.Generic) info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(2f, 7f));
                if (majDmg == Rust.DamageType.Bullet) info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(230f, 300f));
                if (majDmg == Rust.DamageType.Explosion) info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(1.2f, 2.3f));
            }
        }

        void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || entity == null) return;
            var startTime = Time.realtimeSinceStartup;
            var name = entity?.ShortPrefabName ?? "Unknown";
            var attacker = info?.Initiator as BasePlayer ?? null;
            var victim = entity as BasePlayer;
            var projID = info?.ProjectileID ?? 0;
            var isProjectile = info?.IsProjectile() ?? false;
            var getType = entity?.GetType()?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(getType)) return;
            var userID = victim?.UserIDString ?? string.Empty;
            var vicPos = entity?.transform?.position ?? Vector3.zero;
            var attackerID = attacker?.userID ?? 0;
            var victimID = victim?.userID ?? 0;
            var centerVicPos = entity?.CenterPoint() ?? Vector3.zero;
            if (entity.ShortPrefabName == "landmine")
            {
                var ownerID = entity?.OwnerID ?? 0;
                var atk = (ownerID == 0) ? null : ((BasePlayer.FindByID(ownerID) ?? BasePlayer.FindSleeping(ownerID)) as BaseEntity);
                var nearCars = BaseEntity.saveList?.Where(p => p != null && (p is BasicCar) && p?.transform != null && Vector3.Distance(p.transform.position, vicPos) <= 5)?.ToList() ?? null;
                nearCars = nearCars.Distinct().ToList();
                if (nearCars != null && nearCars.Count > 0)
                {
                    for (int i = 0; i < nearCars.Count; i++)
                    {
                        var car = nearCars[i];
                        if (car == null || car.IsDestroyed || car?.transform == null) continue;
                        var carCombat = (car as BaseCombatEntity);
                        if (carCombat == null) continue;
                        var dist = Vector3.Distance(car.transform.position, vicPos);
                        var dmg = UnityEngine.Random.Range(100f, 250f) / (dist < 1.0f ? 1f : dist);
                        carCombat.Hurt(dmg, Rust.DamageType.Explosion, atk);
                    }
                }
            }
            if (entity is BasicCar && !entity.IsDestroyed)
            {
                var mountPly = GetMounted(entity as BasicCar);
                if (mountPly != null)
                {
                    CuiHelper.DestroyUi(mountPly, "Speedometer");
                    UnlockInventory(mountPly);
                }

                var engine = entity?.GetComponent<CarEngine>() ?? null;
                var fuel = engine?.Fuel ?? 0;
                var fireballs = UnityEngine.Random.Range(5, 11);
                fireballs = Mathf.Clamp((int)(fireballs * (fuel / 12)), fireballs, 25);

                var listDmg = new List<Rust.DamageTypeEntry>();
                var expDmg = new Rust.DamageTypeEntry();
                var expMin = Mathf.Clamp(50f * ((float)fuel / 14f), 50f, 400f);
                var expMax = Mathf.Clamp(90f * ((float)fuel / 8f), 90f, 1200f);
                expDmg.amount = UnityEngine.Random.Range(expMin, expMax);
                expDmg.type = Rust.DamageType.Explosion;
                listDmg.Add(expDmg);
                var atkMount = GetMounted(entity as BasicCar);

                var newList = BasePlayer.activePlayerList.ToList();
                newList.AddRange(BasePlayer.sleepingPlayerList.ToList());
                var hurtPos = entity?.transform?.position ?? Vector3.zero;
                for(int i = 0; i < newList.Count; i++)
                {
                    var ply = newList[i];
                    if (ply == null || ply.IsDead() || ply?.transform == null || ply?.gameObject == null) continue;
                    var dist = Vector3.Distance(ply?.transform?.position ?? Vector3.zero, hurtPos);
                    if (dist <= UnityEngine.Random.Range(5.5f, 12.5f) && ply.IsVisible(hurtPos))
                    {
                        var newInfo = new HitInfo(atkMount ?? entity, ply, Rust.DamageType.Explosion, UnityEngine.Random.Range(expMin, expMax));
                        newInfo.WeaponPrefab = entity;
                        ply.Hurt(newInfo);
                    }
                
                }

                Effect.server.Run("assets/bundled/prefabs/fx/gas_explosion_small.prefab", centerVicPos, Vector3.zero);
                Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/explosion.prefab", centerVicPos, Vector3.zero);
                Effect.server.Run("assets/bundled/prefabs/fx/explosions/explosion_01.prefab", centerVicPos, Vector3.zero);
                Effect.server.Run("assets/prefabs/npc/patrol helicopter/damage_effect_debris.prefab", centerVicPos, Vector3.zero);
            }
        }

        Dictionary<string, float> lastEject = new Dictionary<string, float>();
        void OnVehicleEjectedZone(string zoneID, BasicCar car, Vector3 pos) //compatibility for zone manager's "eject" flag
        {
            var player = GetMounted(car);
            if (player == null) return;
            GrantImmunity(car, 0.7f);
            float lastTime;
            if (!lastEject.TryGetValue(player.UserIDString, out lastTime) || (Time.realtimeSinceStartup - lastTime) >= 1.5)
            {
                SendReply(player, "A mysterious force stops your vehicle from entering this area...");
                lastEject[player.UserIDString] = Time.realtimeSinceStartup;
            }
        }

        void OnCollectiblePickup(Item item, BasePlayer player) //stops collectibles from being picked up while in a car
        {
            if (item == null || player == null) return;
            if (player.isMounted)
            {
                var oldAmt = item.amount;
                NextTick(() =>
                {
                    if (item == null || player == null || item.amount < 1) return;
                    var amt = item.amount;
                    player.inventory.Take(null, item.info.itemid, amt);
                    player.SendConsoleCommand("note.inv " + item.info.itemid + " -" + amt);
                });
            }
        }


        void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null) return;
            if (entity is BasicCar)
            {
                var bce = entity as BaseCombatEntity;
                if (bce != null)
                {
                    bce.ShowHealthInfo = true;
                    bce.sendsHitNotification = true;
                    bce.sendsMeleeHitNotification = true;
                }
                var carSpeed = entity?.GetComponent<CarSpeed>() ?? null;
                if (carSpeed == null) carSpeed = entity.gameObject.AddComponent<CarSpeed>();
                var carEngine = entity?.GetComponent<CarEngine>() ?? null;
                if (carEngine == null) carEngine = entity.gameObject.AddComponent<CarEngine>();
                var col = entity?.GetComponent<CarCol>() ?? null;
                if (col == null) col = entity.gameObject.AddComponent<CarCol>();

                NextTick(() =>
                {
                    var carColliders = GetEntityColliders(entity as BaseEntity);
                    if (collectibles != null && collectibles.Count > 0 && carColliders != null && carColliders.Count > 0)
                    {
                        for (int i = 0; i < collectibles.Count; i++)
                        {
                            var collect = collectibles[i];
                            if (collect == null || collect.IsDestroyed) continue;
                            var collectCols = GetEntityColliders(collect);
                            if (collectCols == null || collectCols.Count < 1) continue;
                            for (int j = 0; j < carColliders.Count; j++) for (int k = 0; k < collectCols.Count; k++) Physics.IgnoreCollision(carColliders[j], collectCols[k]);
                        }
                    }
                });
            }
            if (entity is CollectibleEntity)
            {
                var colbEnt = entity as CollectibleEntity;
                if (colbEnt != null && !collectibles.Contains(colbEnt)) collectibles.Add(colbEnt);
                var colbCollider = colbEnt?.GetComponent<Collider>() ?? null;
                if (colbCollider != null)
                {
                    foreach (var ent in BaseEntity.saveList)
                    {
                        if (ent == null) continue;
                        if (!(ent is BasicCar)) continue;
                        if (ent.IsDestroyed) continue;
                        var carColliders = GetEntityColliders(ent);
                        if (carColliders == null || carColliders.Count < 1) continue;
                        for (int i = 0; i < carColliders.Count; i++) Physics.IgnoreCollision(carColliders[i], colbCollider);
                    }
                }
            }
        }

        object OnEntityDismounted(BaseMountable mount, BasePlayer player)
        {
            if (mount == null || player == null) return null;
            if (GetCarFromSeat(mount) != null)
            {
                UnlockInventory(player);
                CuiHelper.DestroyUi(player, "Speedometer");
            }
            return null;
        }

        void OnEntityMounted(BaseMountable mount, BasePlayer player)
        {
            if (mount == null || player == null) return;
            var car = GetCarFromSeat(mount);
            if (car == null) return;
            if (car.OwnerID == 0) car.OwnerID = player.userID;
            if (mount.ShortPrefabName == "driverseat")
            {
                SpeedometerGUI(player, car);
                LockInventory(player);
            }
            else
            {
                CuiHelper.DestroyUi(player, "Speedometer");
                UnlockInventory(player);
            }
        }


        object CanMountEntity(BasePlayer player, BaseMountable mount)
        {
            if (mount == null || player == null) return null;
            if (mount.ShortPrefabName != "driverseat") return null;
            var car = GetCarFromSeat(mount);
            if (car == null) return null;

            var carCenter = car?.CenterPoint() ?? Vector3.zero;
            var leftWheelPos = car?.wheels[0]?.wheel?.position ?? Vector3.zero;
            var rightWheelPos = car?.wheels[1]?.wheel?.position ?? Vector3.zero;

            var leftDepth = WaterLevel.GetWaterDepth(leftWheelPos, true, true);
            var rightDepth = WaterLevel.GetWaterDepth(rightWheelPos, true, true);
            var waterDepth = WaterLevel.GetWaterDepth(carCenter, true, true);

            if ((leftDepth >= 0.4 && rightDepth >= 0.4) || (rightDepth >= 0.51 || leftDepth >= 0.51) || waterDepth >= 0.78)
            {
                SendReply(player, "This car is flooded...");
                return false;
            }
            if ((player?.modelState?.waterLevel ?? 0) >= 0.35) return false;
            var carEngine = car?.GetComponent<CarEngine>() ?? null;
            if (carEngine == null) return null;
            if (!carEngine.HasBattery)
            {
                SendReply(player, "This car needs a battery!");
                return false;
            }
            if (carEngine.Fuel <= 0)
            {
                SendReply(player, "This car needs fuel.");
                return false;
            }
            return null;
        }
        #endregion
        #region Classes

        public class CarVelocity : MonoBehaviour
        {
            public BasicCar car;
            public float BaseScalar = 0.01f;
            public float SpeedDivide = 7000f;
            public int GroundedWheels { get { return car?.wheels?.Count(p => (p?.wheelCollider?.isGrounded ?? false)) ?? 0; } }
            void Awake() => car = GetComponent<BasicCar>();
            public void Update()
            {
                if (car == null || car.IsDestroyed) return;
                var mount = GetMounted(car);
                if (mount == null || mount?.serverInput == null) return;
                var isForward = mount.serverInput.IsDown(BUTTON.FORWARD) && !mount.serverInput.IsDown(BUTTON.BACKWARD);
                var isBackward = mount.serverInput.IsDown(BUTTON.BACKWARD) && !mount.serverInput.IsDown(BUTTON.FORWARD);
                if (!isForward && !isBackward) return;
                if (GroundedWheels < 3) return;
                var rigid = car?.GetComponent<Rigidbody>() ?? null;
                if (rigid == null) return;
                var dist = Vector3.Distance(rigid.velocity, Vector3.zero);
                if (dist < 1f) return;
                var speed = car?.GetComponent<CarSpeed>()?.mps ?? 0;
                if (speed < 0.5) return;
                var fuel = car?.GetComponent<CarEngine>()?.Fuel ?? 0;
                if (fuel <= 3) return;

                var scalar = BaseScalar;
            
                if (SpeedDivide > 0f) scalar -= Mathf.Clamp(((float)speed / SpeedDivide), 0f, 9999f);
                if (speed >= 32) scalar = scalar / 2f;
                if (speed < UnityEngine.Random.Range(4, 9)) scalar = scalar * UnityEngine.Random.Range(1.5f, 2f);
                if (speed < 1f) scalar = scalar * UnityEngine.Random.Range(2f, 4f);
                if (isBackward) scalar = scalar / 2f;

                var rot = car?.transform?.forward ?? Vector3.zero;
                if (isForward) rigid.velocity = rigid.velocity + rot * scalar;
                else if (isBackward) rigid.velocity = rigid.velocity - rot * scalar;

            }
            public void DoDestroy() => Destroy(this);
            
        }

        class CarCol : MonoBehaviour
        {
            public BasicCar car;

            public float lastCollision = 0f;
            void Awake()
            {
                car = GetComponent<BasicCar>();
                InvokeHandler.InvokeRepeating(this, CheckPlayersAndNPCs, 0.5f, 0.05f);
            }


            Dictionary<ulong, DateTime> lastHit = new();
            public void CheckPlayersAndNPCs()
            {
                if (car == null || car?.wheels == null || car.wheels.Length < 1) return;
                var carSpeed = car?.GetComponent<CarSpeed>() ?? null;
                var pSpeed = (float)(carSpeed?.mps ?? -1);
                if (pSpeed < 4) return;
                var tirePos1 = car.wheels[0]?.wheelCollider?.transform?.position ?? Vector3.zero;
                var tirePos2 = car.wheels[1]?.wheelCollider?.transform?.position ?? Vector3.zero;
                var tirePos3 = car.wheels[2]?.wheelCollider?.transform?.position ?? Vector3.zero;
                var tirePos4 = car.wheels[3]?.wheelCollider?.transform?.position ?? Vector3.zero;
                var spheres = Physics.SphereCastAll(new Ray(tirePos1, tirePos2), 0.57f, 1.15f)?.ToList() ?? null;
                if (spheres == null || spheres.Count < 1) spheres = Physics.SphereCastAll(new Ray(tirePos3, tirePos4), 0.57f, 1.15f)?.ToList() ?? null;
                if (spheres != null) spheres.Distinct();
                if (spheres == null || spheres.Count < 1) return;
                var mountPly = GetMounted(car);
                for (int i = 0; i < spheres.Count; i++)
                {
                    var hit = spheres[i];
                    var hitEnt = hit.GetEntity();
                    if (hitEnt == null) continue;
                    var isValid = hitEnt is BasePlayer || hitEnt is BaseNpc || hitEnt is BaseCorpse;
                    if (!isValid) continue;
                    var hitPly = hitEnt as BasePlayer;
                    var hitBce = hitEnt as BaseCombatEntity;
                    if (hitBce == null || hitBce.IsDead()) continue;
                    var netID = hitBce?.net?.ID.Value ?? 0;
                    if (netID == 0) continue;
                    if (hitPly != null)
                    {
                        if ((hitPly.IsFlying && hitPly.IsAdmin) || mountPly != null && hitPly.UserIDString == mountPly.UserIDString) continue;
                        var hitMount = hitPly?.GetMounted() ?? null;
                        if (hitMount != null && (hitMount == car || hitMount.GetParentEntity() == car)) continue;
                    }
                   

                    DateTime lasthitTime;
                    if (!lastHit.TryGetValue(netID, out lasthitTime) || lasthitTime == DateTime.MinValue || (DateTime.UtcNow - lasthitTime).TotalSeconds > 0.58 && pSpeed >= 1.8)
                    {
                        var hurtDmg = UnityEngine.Random.Range(3.7f, 4.85f) * pSpeed;
                        if (hitPly != null && hitPly.ShortPrefabName.Contains("scientist")) hurtDmg *= UnityEngine.Random.Range(2.6f, 3.1f);
                        if (hitPly == null)
                        {
                            hurtDmg *= UnityEngine.Random.Range(2.67f, 4f);
                            var rigid = hitBce?.GetComponent<Rigidbody>() ?? null;
                            if (rigid != null) rigid.velocity = rigid.velocity + (car?.transform?.forward ?? Vector3.zero) * UnityEngine.Random.Range(8, 12);
                            
                        }
                        lastHit[netID] = DateTime.UtcNow;
                        if (hitPly != null) Effect.server.Run(hitPly.fallDamageEffect.resourcePath, hitPly.transform.position, Vector3.zero);
                        var oldPos = hitBce?.transform?.position ?? Vector3.zero;
                        if (!(hitEnt is BaseCorpse))
                        {
                            hitBce.Hurt(hurtDmg, Rust.DamageType.Blunt, (mountPly ?? hitPly));
                            InvokeHandler.Invoke(this, () =>
                            {
                                if (hitBce != null && !hitBce.IsDead()) return;
                                var findCorpses = new List<BaseCorpse>();
                                Vis.Entities<BaseCorpse>(oldPos, 1f, findCorpses, Rust.Layers.Ragdolls);
                                var findCorpse = findCorpses?.FirstOrDefault() ?? null;
                                if (findCorpse != null)
                                {
                                    var rigid = findCorpse?.GetComponent<Rigidbody>() ?? null;
                                    if (rigid != null) rigid.velocity = rigid.velocity + (car?.transform?.forward ?? Vector3.zero) * (pSpeed * UnityEngine.Random.Range(1.1f, 1.4f));
                                }
                            }, 0.1f);
                        }
                        else
                        {
                            var rigid = (hitEnt as BaseCorpse)?.GetComponent<Rigidbody>() ?? null;
                            if (rigid != null) rigid.velocity = rigid.velocity + (car?.transform?.forward ?? Vector3.zero) * (pSpeed * UnityEngine.Random.Range(1.1f, 1.4f));
                        }
                     
                       
                    }
                }
            }

            public void DoDestroy()
            {
                InvokeHandler.CancelInvoke(this, CheckPlayersAndNPCs);
                Destroy(this);
            }

            void OnCollisionEnter(Collision coll)
            {
                using (TimeWarning.New("CarColOnCollisionEnter"))
                {

                    if (coll == null || IsSaving()) return;
                    var colName = coll?.collider?.name ?? string.Empty;
                    if (string.IsNullOrEmpty(colName)) return;
                    var colMat = (coll?.collider?.material?.name ?? coll?.collider?.sharedMaterial?.name ?? string.Empty).ToLower();

                    var player = GetMounted(car);

                    if (colName.Contains("plant", CompareOptions.OrdinalIgnoreCase)) return;

                    if (lastCollision != 0f && (Time.realtimeSinceStartup - lastCollision) < 0.45f) return;

                    var carPos = car?.transform?.position ?? Vector3.zero;
                    var hitDistances = coll?.contacts?.Select(p => Vector3.Distance(p.point, carPos))?.ToList() ?? null;

                    var hitPos = carPos;
                    if (hitDistances != null && hitDistances.Count > 0)
                    {
                        var minDist = hitDistances.Min();
                        hitPos = coll?.contacts?.Where(p => Vector3.Distance(p.point, carPos) == minDist)?.FirstOrDefault().point ?? Vector3.zero;
                    }
                    var hitTire = false;
                    var collider = coll?.collider ?? null;
                    
                    if (collider != null)
                    {
                        RaycastHit info;
                        for (int i = 0; i < car.wheels.Length; i++)
                        {
                            var wheel = car.wheels[i];
                            if (wheel == null) continue;
                            var wheelPos = wheel?.wheel?.transform?.position ?? Vector3.zero;
                            if (wheelPos == Vector3.zero) continue;
                            if (Physics.Raycast(new Ray(wheelPos, Vector3.down), out info, 0.4f))
                            {
                                var getCol = info.GetCollider() ?? info.collider;
                                if (getCol == null) getCol = info.collider;
                                if (getCol != null && getCol == collider)
                                {
                                    hitTire = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (hitTire) return;

                    var carSpeed = car?.GetComponent<CarSpeed>() ?? null;
                    var pSpeed = (float)(carSpeed?.mps ?? -1);
                    if (pSpeed < 0.5) return;
                    if (pSpeed > 1)
                    {
                        if (colName.Contains("driftwood") || colName.Contains("log") || colMat.Contains("tree") || colName.Contains("tree", CompareOptions.OrdinalIgnoreCase) || colName.Contains("pine") || colName.Contains("maple") || colName.Contains("v2_"))
                        {
                            var rngMax = (pSpeed > 1.5) ? 6 : 2;
                            for (int i = 0; i < UnityEngine.Random.Range(1, rngMax); i++) Effect.server.Run("assets/bundled/prefabs/fx/entities/tree/tree-impact.prefab", hitPos, Vector3.zero);
                            if (pSpeed >= 12)
                            {
                                var rngTree = UnityEngine.Random.Range(0, 101);
                                if (rngTree <= 50 || pSpeed > 19)
                                {
                                    TreeEntity treeEnt = null;
                                    try { treeEnt = (coll?.GetEntity() ?? coll?.gameObject?.ToBaseEntity() ?? null) as TreeEntity; }
                                    catch (Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                                    if (treeEnt != null && !treeEnt.IsDestroyed)
                                    {
                                        var newHit = new HitInfo();
                                        newHit.PointStart = hitPos;
                                        newHit.PointEnd = hitPos + Vector3.forward * 2f;
                                        treeEnt.OnKilled(newHit);
                                    }

                                }
                            }
                        }
                        else if (colMat.Contains("wood", CompareOptions.OrdinalIgnoreCase) && pSpeed > 1.5) Effect.server.Run("assets/bundled/prefabs/fx/impacts/blunt/wood/wood1.prefab", hitPos, Vector3.zero);
                    }

                    if (pSpeed > UnityEngine.Random.Range(1.2f, 1.6f))
                    {
                        if (colMat.Contains("grass", CompareOptions.OrdinalIgnoreCase)) Effect.server.Run("assets/bundled/prefabs/fx/impacts/blunt/grass/grass1.prefab", hitPos, Vector3.zero);
                    }



                    if (pSpeed > 1.2)
                    {
                        if (colMat.Contains("rock") || colMat.Contains("stone") || colMat.Contains("concrete") || colName.Contains("rock", CompareOptions.OrdinalIgnoreCase) || colName.Contains("stone", CompareOptions.OrdinalIgnoreCase) || colName.Contains("concrete", CompareOptions.OrdinalIgnoreCase) || colName.Contains("pavement") || colName.Contains("road"))
                        {
                            var concFX = (UnityEngine.Random.Range(0, 5) < 3) ? "assets/bundled/prefabs/fx/impacts/stab/concrete/concrete1.prefab" : "assets/bundled/prefabs/fx/impacts/blunt/concrete/concrete1.prefab";
                            for (int i = 0; i < 2; i++) Effect.server.Run(concFX, hitPos, Vector3.zero);
                            var fx = (UnityEngine.Random.Range(0, 5) < 3) ? "assets/bundled/prefabs/fx/impacts/stab/metal/metal1.prefab" : "assets/bundled/prefabs/fx/impacts/blunt/metal/metal1.prefab";
                            if (pSpeed >= 9) Effect.server.Run(fx, hitPos, Vector3.zero);
                        }
                        if (colMat.Contains("ice") || colMat.Contains("snow"))
                        {
                            var rngSnow = UnityEngine.Random.Range(0, 4);
                            var snowFX = rngSnow == 0 ? "assets/bundled/prefabs/fx/impacts/bullet/snow/snow1.prefab" : rngSnow == 1 ? "assets/bundled/prefabs/fx/impacts/blunt/snow/snow.prefab" : rngSnow == 2 ? "assets/bundled/prefabs/fx/impacts/stab/snow/snow.prefab" : "assets/bundled/prefabs/fx/impacts/slash/snow/snow.prefab";
                            var runTimes = UnityEngine.Random.Range(0, (pSpeed >= 9) ? 9 : 5);
                            if (runTimes > 0) for (int i = 0; i < runTimes; i++) Effect.server.Run(snowFX, hitPos);
                        }
                    }

                    if (pSpeed > 0.9)
                    {
                        if (colMat.Contains("metal") || colName.Contains("metal", CompareOptions.OrdinalIgnoreCase))
                        {
                            var fx = (UnityEngine.Random.Range(0, 5) < 3) ? "assets/bundled/prefabs/fx/impacts/stab/metal/metal1.prefab" : "assets/bundled/prefabs/fx/impacts/blunt/metal/metal1.prefab";
                            Effect.server.Run(fx, hitPos, Vector3.zero);
                        }
                    }
                    lastCollision = Time.realtimeSinceStartup;
                    if (colMat.Contains("zero friction", CompareOptions.OrdinalIgnoreCase)) return;
                    if (pSpeed <= 0.0f) return;
                    if (colName.Contains("Terrain", CompareOptions.OrdinalIgnoreCase))
                    {
                        if(pSpeed >= UnityEngine.Random.Range(3f, 7f))
                        {
                            var dirtRng = UnityEngine.Random.Range(0, 3);
                            var dirtFX = dirtRng == 0 ? "assets/bundled/prefabs/fx/impacts/blunt/dirt/dirt1.prefab" : dirtRng == 1 ? "assets/bundled/prefabs/fx/impacts/slash/dirt/dirt1.prefab" : "assets/bundled/prefabs/fx/impacts/stab/dirt/dirt1.prefab";
                            var rngTimes = UnityEngine.Random.Range(0, 5);
                            if (rngTimes > 0) for (int i = 0; i < rngTimes; i++) Effect.server.Run(dirtFX, hitPos, Vector3.zero);
                        }
                      
                            
                        
                        if (pSpeed < 10) return;
                    }
                    if (colName.Contains("Terrain", CompareOptions.OrdinalIgnoreCase) && pSpeed < 10) return;
                    var hurtDmg = pSpeed * (Mathf.Clamp((pSpeed > 0.0f ? pSpeed : 1f), 1f, 11f) / UnityEngine.Random.Range(1.62f, 2.3f));
                    if (pSpeed >= UnityEngine.Random.Range(22.7f, 27))
                    {
                        hurtDmg = hurtDmg * (pSpeed / UnityEngine.Random.Range(16f, 24f));
                        if (colName.Contains("Terrain", CompareOptions.OrdinalIgnoreCase)) hurtDmg = hurtDmg / UnityEngine.Random.Range(1.6f, 2f);
                    }
                    if (colName.Contains("road", CompareOptions.OrdinalIgnoreCase) || colName.Contains("pavement", CompareOptions.OrdinalIgnoreCase)) hurtDmg = Mathf.Clamp((hurtDmg / UnityEngine.Random.Range(9f, 17f)), 0, 100);
                    var colEnt = coll?.GetEntity() ?? null;
                    if (colName == "lootbarrel")
                    {
                        if (colEnt != null && !colEnt.IsDestroyed)
                        {
                            var dmg = UnityEngine.Random.Range(2.7f, 6f) * Mathf.Clamp(pSpeed, 1, 999);
                            var bce = colEnt?.GetComponent<BaseCombatEntity>() ?? null;
                            if (bce != null) bce.Hurt(dmg, Rust.DamageType.Blunt, player);
                        }
                    }
                    var groundedCount = car?.wheels?.Count(p => (p?.wheelCollider?.isGrounded ?? false)) ?? 0;
                    if (groundedCount < 1) hurtDmg = hurtDmg * 2.1f;
                    else if (groundedCount < 2) hurtDmg = hurtDmg * 1.8f;
                    else if (groundedCount < 3) hurtDmg = hurtDmg * 1.45f;
                    else if (groundedCount < 4) hurtDmg = hurtDmg * 1.15f;

                    if (hurtDmg > 0 && !car.IsDestroyed)
                    {
                        var oldHP = car?.Health() ?? 0f;
                        car.Hurt(hurtDmg, Rust.DamageType.Generic, colEnt, true);                        
                    }
                }
            }
        }
        
        public static void ForceSlowCar(BasicCar car, bool dismountOnStop = true, float slowSpeed = 1.025f, float multiplier = 0.0035f, int maxSlowTimes = 200)
        {
            if (car == null || car.IsDestroyed || car.gameObject == null) return;
            var rigid = car?.GetComponent<Rigidbody>() ?? null;
            var mount = GetMounted(car);
            var speed = car?.GetComponent<CarSpeed>() ?? null;
            var mps = speed?.mps ?? -1;
            if (rigid != null && mount != null)
            {
                var startVel = rigid.velocity;
                var velTimes = 0;
                Action act = null;
                act = new Action(() =>
                {
                    velTimes++;
                    mps = speed?.mps ?? -1;
                    if (car == null || car.IsDestroyed || rigid == null || rigid.velocity == Vector3.zero || Vector3.Distance(rigid.velocity, Vector3.zero) < 1.7 || mps < 2 || (maxSlowTimes > 0 && velTimes > maxSlowTimes) || mps > 40)
                    {
                        InvokeHandler.CancelInvoke(car, act);
                        if (dismountOnStop && car != null) car?.DismountAllPlayers();
                        if (rigid != null) rigid.velocity = Vector3.zero;
                        return;
                    }

                    rigid.velocity = rigid.velocity / (slowSpeed + (velTimes * multiplier));
                });
                InvokeHandler.InvokeRepeating(car, act, 0.01f, 0.15f);
            }
        }

        class CarSpeed : MonoBehaviour
        {
            BasicCar car;
            public double mps = -1;
            public Vector3 lastPos;
            void Awake()
            {
                car = GetComponent<BasicCar>();
                lastPos = car?.transform?.position ?? Vector3.zero;
                InvokeHandler.InvokeRepeating(this, CheckDistance, 0.25f, 0.25f);
            }
            void CheckDistance()
            {
                if (car?.transform == null || IsSaving()) return;
                var pos = car?.transform?.position ?? Vector3.zero;
                mps = Vector3.Distance(lastPos, car.transform.position) * 4;
                lastPos = pos;
            }
            public void DoDestroy()
            {
                InvokeHandler.CancelInvoke(this, CheckDistance);
                Destroy(this);
            }
        }

        public class SpawnInfo
        {
            [JsonProperty(Required = Required.AllowNull)]
            private float _chance = 0f;
            [JsonIgnore]
            public float Chance
            {
                get { return  Mathf.Clamp(_chance, 0, 100f); }
                set { _chance = Mathf.Clamp(value, 0, 100f); }
            }
            [JsonProperty(Required = Required.AllowNull)]
            private string _pos = string.Empty;
            [JsonIgnore]
            private Vector3 _cachePos = Vector3.zero;
            public Vector3 Position
            {
                get
                {
                    if (_cachePos != Vector3.zero) return _cachePos;
                    var newPos = GetVector3FromString(_pos);
                    return (_cachePos = newPos);
                }
                set
                {
                    _pos = value.ToString();
                    _cachePos = value;
                }
            }
            public SpawnInfo() { }
            public SpawnInfo(Vector3 position, float chance)
            {
                Chance = chance;
                Position = position;
            }
        }

        public static Vector3 GetVector3FromString(string vectorStr)
        {
            var vector = Vector3.zero;
            if (string.IsNullOrEmpty(vectorStr)) return vector;
            var vecStr = vectorStr;
            if (vecStr.StartsWith("(") && vecStr.EndsWith(")")) vecStr = vecStr.Substring(1, vecStr.Length - 2);
            var split = vecStr.Split(',');
            return (split == null || split.Length < 3) ? vector : new Vector3(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]));
        }

        class CarData
        {
            public List<CarInfo> carInfos = new List<CarInfo>();
            public List<SpawnInfo> spawnInfos = new List<SpawnInfo>();
            
            public CarData() { }
        }
        class CarInfo
        {
            public double Fuel = 0d;
            public double Oil = 0d;
            public bool HasBattery = false;
            public ulong NetID = 0;
            public bool Turbo = false;

            public CarInfo() { }

            public CarInfo(BasicCar car)
            {
                if (car == null) return;
                NetID = car?.net?.ID.Value ?? 0;
                var carEngine = car?.GetComponent<CarEngine>() ?? null;
                if (carEngine != null)
                {
                    Fuel = carEngine.Fuel;
                    Oil = carEngine.Oil;
                }
            }
        }

        class CarEngine : MonoBehaviour
        {
            BasicCar car;
            public double Oil = 0;
            public double Fuel = 0;
            public bool HasBattery = false;
            public bool Turbo = false;
            public int GroundedWheels
            {
                get
                {
                    var count = 0;
                    for(int i = 0; i < car.wheels.Length; i++)
                    {
                        if ((car.wheels[i]?.wheelCollider?.isGrounded ?? false)) count++;
                    }
                    return count;
                }
            }

            void Awake()
            {
                car = GetComponent<BasicCar>();
                if (car == null || car.IsDestroyed)
                {
                    Interface.Oxide.LogError("CarEngine.Awake() called on null or destroyed car!!");
                    DoDestroy();
                    return;
                }
                InvokeHandler.InvokeRepeating(this, CheckEngine, 1f, 1f);
                InvokeHandler.InvokeRandomized(this, UseFuel, 8f, 5f, 7f);
            }

            public void CheckEngine()
            {
                if (car == null || car.IsDestroyed || !car.IsMounted()) return;
                if (GroundedWheels < 2) return;
                if (car.healthFraction <= 0.35f)
                {
                    if (UnityEngine.Random.Range(0, 101) <= 5)
                    {
                        var mount = GetMounted(car);
                        if (mount != null && mount.IsConnected)
                        {
                            mount.ChatMessage("Your car engine appears to be having issues...");
                            ForceSlowCar(car, false, 1.015f, 0.0025f, 275);
                        }
                    }
                }

                if (Fuel <= 0) ForceSlowCar(car);
            }

            public void UseFuel()
            {
                if (Fuel <= 0 || !car.IsMounted()) return;
                var mps = car?.GetComponent<CarSpeed>()?.mps ?? 0;
                var fuelUse = UnityEngine.Random.Range(0.5f, 1.75f);
                if (mps < 1) fuelUse = fuelUse / UnityEngine.Random.Range(1.8f, 6f);
                if (mps > 11) fuelUse = fuelUse * UnityEngine.Random.Range(1.2f, Mathf.Clamp(((float)mps / 5f), 1.2f, 999));
                if (GroundedWheels < 2) fuelUse = fuelUse / UnityEngine.Random.Range(3f, 8f);
                
                var oldFuel = Fuel;
                Fuel = Mathf.Clamp((float)Fuel - fuelUse, 0, MaxFuel);
            }


            public void DoDestroy()
            {
                InvokeHandler.CancelInvoke(this, CheckEngine);
                InvokeHandler.CancelInvoke(this, UseFuel);
                Destroy(this);
            }
        }
        #endregion
        #region Saving
        void SaveCarEngines()
        {
            foreach (var entity in BaseEntity.saveList)
            {
                if (entity == null || !(entity is BasicCar)) continue;
                var engine = entity?.GetComponent<CarEngine>() ?? null;
                if (engine == null) continue;
                var carNetID = entity?.net?.ID.Value ?? 0;
                var carInfo = (carData?.carInfos == null || carData.carInfos.Count < 1) ? null : carData?.carInfos?.Where(p => p.NetID == carNetID)?.FirstOrDefault() ?? null;
                if (carInfo == null)
                {
                    carInfo = new CarInfo();
                    if (carData?.carInfos != null) carData.carInfos.Add(carInfo);
                }
                carInfo.NetID = carNetID;
                carInfo.Fuel = engine.Fuel;
                carInfo.Oil = engine.Oil;
                carInfo.HasBattery = engine.HasBattery;
            }
        }
        private void SaveCarData()
        {
            carData?.carInfos?.RemoveAll(p => p == null || !BaseEntity.saveList.Any(x => (x?.net?.ID.Value ?? 0) == p.NetID));
            Interface.Oxide.DataFileSystem.WriteObject("CarData", carData);
        }
        static System.Random spawnRng = new System.Random();
        System.Collections.IEnumerator SpawnCars(List<SpawnInfo> spawns)
        {
            var infoSB = new StringBuilder();
            var watch = Stopwatch.StartNew();
            var count = 0;
            var max = 10;
            for(int i = 0; i < spawns.Count; i++)
            {
                var info = spawns[i];
                if (info == null) continue;
                var pos = info.Position;
                if (pos == Vector3.zero) continue;
                count++;
                if (count >= max)
                {
                    count = 0;
                    yield return new WaitForSeconds(0.25f);
                }
                var nearBreak = false;
                foreach (var ent in BaseNetworkable.serverEntities)
                {
                    if (ent == null || ent?.gameObject == null || ent.IsDestroyed || ent?.transform == null) continue;
                    var dist = Vector3.Distance(ent?.transform?.position ?? Vector3.zero, pos);
                    if ((ent is BasePlayer && dist <= 30) || (ent is BasicCar && dist <= 20) || (dist < 4))
                    {
                        nearBreak = true;
                        break;
                    }
                }
                if (nearBreak) continue;
                var rng = spawnRng.Next(0, 101);
                if (rng > info.Chance)
                {
                    infoSB.AppendLine("rng > info.chance: " + rng + " > " + info.Chance);
                    continue;
                }
                var car = (BasicCar)GameManager.server.CreateEntity("assets/content/vehicles/sedan_a/sedantest.entity.prefab", pos);
                if (car != null)
                {
                    car.Spawn();
                    NextTick(() =>
                    {
                        var engine = car?.GetComponent<CarEngine>() ?? null;
                        if (engine != null)
                        {
                            engine.Fuel = Mathf.Clamp(UnityEngine.Random.Range(0, (MaxFuel / 2) + 1), 0, MaxFuel);
                            engine.HasBattery = true;
                            var turboRng = UnityEngine.Random.Range(0, 101) <= 7;
                            if (turboRng)
                            {
                                car.gameObject.AddComponent<CarVelocity>();
                                engine.Turbo = true;
                                infoSB.AppendLine("Turbo car spawn at: " + pos);
                            }
                            infoSB.AppendLine("rng spawn with Fuel amount: " + engine.Fuel + ", has battery: " + engine.HasBattery);
                        }

                    });

                }
            }
            watch.Stop();
            infoSB.AppendLine("SpawnCars (coroutine) took: " + watch.Elapsed.TotalMilliseconds + "ms");
            PrintWarning(infoSB.ToString().TrimEnd());
        }
        void SpawnAllCars(bool force = false)
        {
            if (carData?.spawnInfos != null && carData.spawnInfos.Count > 0)
            {
                var carCount = BaseNetworkable.serverEntities?.Count(p => p is BasicCar) ?? 0;
              
                if (!force && carCount > MaxSpawnCars)
                {
                    PrintWarning("carCount " + carCount + " > " + MaxSpawnCars + " max spawn");
                    return;
                }
                var watch = Stopwatch.StartNew();
                var infoSB = new StringBuilder();
                if (ServerMgr.Instance != null) ServerMgr.Instance.StartCoroutine(SpawnCars(carData.spawnInfos));
                watch.Stop();
                infoSB.AppendLine("Car spawns (" + carData.spawnInfos.Count + " count) took: " + watch.Elapsed.TotalMilliseconds + "ms");
                PrintWarning(infoSB.ToString().TrimEnd());
            }
        }

        public void SetTurboCar(BasicCar car, bool val)
        {
            if (car == null) return;
            var findTurbo = car?.GetComponent<CarVelocity>() ?? null;
            
            if (!val)
            {
                if (findTurbo != null) findTurbo.DoDestroy();
            }
            else
            {
                if (findTurbo == null) car.gameObject.AddComponent<CarVelocity>();
            }
            var engine = car?.GetComponent<CarEngine>() ?? null;
            if (engine != null) engine.Turbo = val;
        }
        public static DateTime LastServerSave { get; set; }
        void OnServerSave()
        {
            LastServerSave = DateTime.UtcNow;
            timer.Once(1f, () =>
            {
                SaveCarEngines();
                timer.Once(1f, () => SaveCarData());
            });
            if (ServerMgr.Instance != null && UnityEngine.Random.Range(0, 101) < 33) ServerMgr.Instance.Invoke(() => SpawnAllCars(false), 8f); //start spawning cars 8 seconds *AFTER* save for performance reasons
        }
        #endregion
        #region Util
        T GetConfig<T>(string name, T defaultValue) { return (Config[name] == null) ? defaultValue : (T)Convert.ChangeType(Config[name], typeof(T)); }
        void SendReply(IPlayer player, string msg, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : (keepTagsConsole) ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else player.Reply(msg);
        }
        string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;
            //	Forbidden formatting tags
            var forbiddenTags = new List<string>{
                "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>"
            };

            //	Replace Color Tags
            phrase = new Regex("(<color=.+?>)").Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = new Regex("(<size=.+?>)").Replace(phrase, string.Empty);
            var phraseSB = new StringBuilder(phrase);
            for (int i = 0; i < forbiddenTags.Count; i++)
            {
                var tag = forbiddenTags[i];
                phraseSB.Replace(tag, string.Empty);
            }

            return phraseSB.ToString();
        }

        BasicCar GetCarFromSeat(BaseMountable mountable)
        {
            if (mountable == null || !(mountable is BaseVehicleSeat)) return null;
            return ((mountable as BaseVehicleSeat)?.VehicleParent() ?? null) as BasicCar;
        }

        public static  bool IsSaving()
        {
            if (SaveRestore.IsSaving) return true;
            return (LastServerSave != DateTime.MinValue) ? ((DateTime.UtcNow - LastServerSave).TotalSeconds < 3) : false;
        }

        public static bool IsLowFPS() { return Performance.current.frameRateAverage < Mathf.Clamp(100, 100, ConVar.FPS.limit) || Performance.current.frameRate < Mathf.Clamp(50, 50, ConVar.FPS.limit); }


        private ListDictionary<InvokeAction, float> InvokeList { get { return InvokeHandler.Instance == null ? null : (ListDictionary<InvokeAction, float>)curList.GetValue(InvokeHandler.Instance); } }

        private void CancelInvoke(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName) || obj == null) return;
            if (!IsInvoking(methodName, obj)) return;
            var action = InvokeList.Where(p => (p.Key.action?.Target ?? null) == obj && (p.Key.action?.Method?.Name ?? string.Empty) == methodName).FirstOrDefault().Key;
            if (action != null) InvokeHandler.CancelInvoke(action.sender, action.action);
        }

        private bool IsInvoking(string methodName, object obj) { return InvokeList?.Any(p => (p.Key.action?.Method?.Name ?? string.Empty) == methodName && (p.Key.action?.Target ?? null) == obj) ?? false; }

        private bool IsNullableType(Type type) { return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)); }

        void SetValue(object inputObject, string propertyName, object propertyVal)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException();
            //find out the type
            var type = inputObject.GetType();

            //get the property information based on the type
            var propertyInfo = type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

            //find the property type
            var propertyType = propertyInfo.FieldType;

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal);
        }

        object GetValue(object inputObject, string propertyName)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException();
            //find out the type
            var type = inputObject.GetType();

            //get the property information based on the type
            return type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(inputObject);
        }

        void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            if (item?.parent != null) item.RemoveFromContainer();
            item.Remove();
        }

        Vector3 SpreadVector(Vector3 vec, float rocketSpread = 1.5f) { return Quaternion.Euler(UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f)) * vec; }
        Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

        List<Collider> GetEntityColliders(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed) return null;
            var colliders = new List<Collider>();
            var cols = entity?.transform?.GetComponentsInChildren<Collider>() ?? null;
            var col2 = entity?.transform?.GetComponentInChildrenIncludeDisabled<Collider>() ?? null;
            if (cols != null) for (int i = 0; i < cols.Length; i++) colliders.Add(cols[i]);
            if (col2 != null && !colliders.Contains(col2)) colliders.Add(col2);
            return colliders;
        }

        BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;
            if (maxDist > 1000) maxDist = 1000;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (Physics.Raycast(ray, out hit, maxDist, ((coll != -1) ? coll : constructionColl)))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }

        void SpeedometerGUI(BasePlayer player, BasicCar car)
        {
            if (player == null || player.IsDead() || GetCarFromSeat(player?.GetMounted() ?? null) != car || car == null || car.IsDestroyed || !player.IsConnected || player?.net?.connection == null) return;
            var speed = (car?.GetComponent<CarSpeed>()?.mps ?? -1).ToString("N0");

            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.25 0.25 0.25 0.7"
                },
                RectTransform =
                {
                    AnchorMin = "0.476 0.114",
                    AnchorMax = "0.539 0.197"
                }
            }, "Under", "Speedometer");

            var carHP = car?.Health() ?? 0f;
            var carHPMax = car?.MaxHealth() ?? 0f;
            var color = "<color=" + ((carHP > 250) ? "lime" : (carHP > 175) ? "orange" : (carHP > 75) ? "yellow" : "red") + ">";
            var HPText = "HP: " + color + carHP.ToString("N0") + "/" + carHPMax.ToString("N0") + "</color>";
            var fuel = car?.GetComponent<CarEngine>()?.Fuel ?? 0;
            var fuelText = "Fuel: " + fuel.ToString("N0") + "/" + MaxFuel.ToString("N0");
            if (fuel <= 25) fuelText = "<color=red>" + fuelText + "</color>";
            var isTurbo = car?.GetComponent<CarVelocity>() != null;
            var innerPingUIText = new CuiElement
            {
                Name = CuiHelper.GetGuid(),
                Parent = "Speedometer",
                Components =
                        {
                            new CuiTextComponent { Color = "0.9 0.5 0.85 1", Text = speed + " mps " + (isTurbo ? "<color=orange>(T)</color>" : "") + "\n" + HPText + "\n" + fuelText, FontSize = 14, Align = TextAnchor.MiddleCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0 0", AnchorMax = "1 1" }
                        }
            };
            elements.Add(innerPingUIText);
            CuiHelper.DestroyUi(player, "Speedometer");
            CuiHelper.AddUi(player, elements);
        }

        public static BasePlayer GetMounted(BaseMountable mount)
        {
            if (mount == null) return null;
            var mountPly = mount.GetMounted();
            if (mountPly != null) return mountPly;
            mountPly = mount?.VehicleParent()?.GetMounted() ?? null;
            if (mountPly != null) return mountPly;
            var mountPoints = (mount as BaseVehicle)?.mountPoints ?? null;
            if (mountPoints != null && mountPoints.Count > 0) return mountPoints[0]?.mountable?.GetMounted() ?? null;
            return null;
        }

        List<ulong> immuneEntities = new();
        Dictionary<ulong, Timer> immuneTimers = new();

        void GrantImmunity(BaseEntity entity, float duration = -1f) { if (entity != null && entity?.net != null && !entity.IsDestroyed) GrantImmunity(entity.net.ID.Value, duration); }

        void GrantImmunity(ulong netID, float duration = -1f)
        {
            if (immuneEntities.Contains(netID)) return;
            immuneEntities.Add(netID);
            if (duration != -1f)
            {
                Timer newTimer;
                newTimer = timer.Once(duration, () =>
                {
                    Timer getTimer;
                    if (immuneTimers.TryGetValue(netID, out getTimer)) immuneTimers.Remove(netID);
                    if (immuneEntities.Contains(netID)) immuneEntities.Remove(netID);
                });
                immuneTimers[netID] = newTimer;
            }
        }

        void RemoveImmunity(uint netID)
        {
            if (immuneEntities.Contains(netID)) immuneEntities.Remove(netID);
            Timer getTimer;
            if (immuneTimers.TryGetValue(netID, out getTimer)) immuneTimers.Remove(netID);
        }

        static void CancelDamage(HitInfo hitinfo)
        {
            if (hitinfo == null) return;
            hitinfo.damageTypes = new Rust.DamageTypeList();
            hitinfo.HitEntity = null;
        }

        void UnlockInventory(BasePlayer player)
        {
            if (player == null || player?.inventory == null) return;
            if (player.inventory.containerMain.IsLocked()) player.inventory.containerMain.SetLocked(false);
            if (player.inventory.containerBelt.IsLocked()) player.inventory.containerBelt.SetLocked(false);
            if (player.inventory.containerWear.IsLocked()) player.inventory.containerWear.SetLocked(false);
        }

        void LockInventory(BasePlayer player)
        {
            if (player == null || player?.inventory == null) return;
            if (!player.inventory.containerMain.IsLocked()) player.inventory.containerMain.SetLocked(true);
            if (!player.inventory.containerBelt.IsLocked()) player.inventory.containerBelt.SetLocked(true);
            if (!player.inventory.containerWear.IsLocked()) player.inventory.containerWear.SetLocked(true);
        }

        Vector3 GetPlayerRotation(BasePlayer player) { return player == null ? Vector3.zero : Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward; }
        #endregion
    }
}
