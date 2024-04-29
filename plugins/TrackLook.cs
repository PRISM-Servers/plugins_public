using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Oxide.Plugins
{
    [Info("TrackLook", "Shady", "0.0.1")]
    internal class TrackLook : RustPlugin
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
        private readonly FieldInfo _storageUnitInstance = typeof(VehicleModuleStorage).GetField("storageUnitInstance", BindingFlags.Instance | BindingFlags.NonPublic);

        #region Fields

        private const string TRACKLOOK_PERMISSION_DEFAULT = "tracklook.use";
        private const string LOOKAT_PERMISSION_DEFAULT = "tracklook.lookat";

        private bool _configChanged = false;


        //t




        private readonly Dictionary<string, BaseEntity> _trackEntity = new Dictionary<string, BaseEntity>();
        private readonly int _constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });

        #endregion

        #region Config

        #region Config Fields
        private string _useTrackLookPermission;
        private string _lookAtPermission;


        #endregion

        protected override void LoadDefaultConfig()
        {

            _useTrackLookPermission = GetConfig("Tracklook Permission", TRACKLOOK_PERMISSION_DEFAULT);
            _lookAtPermission = GetConfig("Lookat Permission", LOOKAT_PERMISSION_DEFAULT);

        }
        protected override void LoadDefaultMessages()
        {
            var messages = new Dictionary<string, string>
            {
                //DO NOT EDIT LANGUAGE FILES HERE! Navigate to oxide\lang
                {"noPerms", "You do not have permission to use this command!"},
                {"tracklookDisabled", "Disabled tracklook"},
                {"notBool", "Value must be true or false"},
                {"noEntityFound", "No entity found!" }
            };
            lang.RegisterMessages(messages, this);
        }
        private string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);

        private T GetConfig<T>(string name, T defaultValue)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            if (Config[name] == null)
            {
                SetConfig(name, defaultValue);

                return defaultValue;
            }

            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        private void SetConfig<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Config[name] = value;

            _configChanged = true;
        }
        #endregion

        #region Hooks
        private void Init()
        {
            Unsubscribe(nameof(OnPlayerInput));

            if (!_configChanged) LoadDefaultConfig(); //don't call load again if it was already called because config file didn't exist. tiny optimization

            if (_configChanged) SaveConfig(); //config could have been changed after a manual call of loaddefaultconfig, so we check again

            LoadDefaultMessages();

            permission.RegisterPermission(_useTrackLookPermission, this);
            permission.RegisterPermission(_lookAtPermission, this);

            AddCovalenceCommand("tracklook", nameof(cmdTrackLook));
            AddCovalenceCommand("tracklook2", nameof(cmdTrackLookAuto));

        }
        #endregion


       
        private void cmdTrackLook(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;
            if (!HasPerms(player.Id, _useTrackLookPermission))
            {
                SendNoPerms(player);
                return;
            }

            BaseEntity outEntity;
            BaseNpc npc = null;
            TrackMovement getTrack = null;
            if (_trackEntity.TryGetValue(player.Id, out outEntity))
            {

                player.Message(GetMessage("tracklookDisabled", player.Id));
                _trackEntity.Remove(player.Id);
                if (outEntity != null)
                {
                    getTrack = outEntity?.GetComponent<TrackMovement>() ?? null;
                    if (getTrack != null) getTrack.DoDestroy();

                    npc = outEntity as BaseNpc;
                    if (npc != null) npc.Resume();

                }

                return;
            }

            int coll;
            if (args.Length < 1 || !int.TryParse(args[0], out coll)) coll = -1;// if (!int.TryParse(args[0], out coll)) coll = -1;

            var noRigid = false;
            if (args.Length > 1 && !bool.TryParse(args[1], out noRigid))
            {
                player.Message(GetMessage("notBool", player.Id));
                return;
            }

            var pObj = player?.Object as BasePlayer;
            if (pObj == null) return;

            var entity = GetLookAtEntity(pObj, 100, coll);
            if (entity == null || entity.gameObject == null || entity.IsDestroyed)
            {
                player.Message(GetMessage("noEntityFound", player.Id));
                return;
            }
            

            npc = entity as BaseNpc;

            if (npc != null) npc.Pause();


            _trackEntity[player.Id] = entity;
             Subscribe(nameof(OnPlayerInput));

            player.Message("Track ent: " + ((entity == null) ? "disabled" : "enabled") + ", ent: " + entity);

            if (noRigid)
            {
                entity.Invoke(() =>
                {
                    var trackNow = entity?.GetComponent<TrackMovement>() ?? null;
                    if (trackNow != null) trackNow.ToggleRigid(false);
                }, 0.1f);
            }

        }

       
        private void cmdTrackLookAuto(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;

            if (!HasPerms(player.Id, _useTrackLookPermission))
            {
                SendNoPerms(player);
                return;
            }


            BaseEntity outEntity;
            BaseNpc npc = null;
            TrackMovement getTrack = null;

            if (_trackEntity.TryGetValue(player.Id, out outEntity))
            {

                _trackEntity.Remove(player.Id);

                if (outEntity != null)
                {
                    getTrack = outEntity?.GetComponent<TrackMovement>() ?? null;
                    if (getTrack != null) getTrack.DoDestroy();

                    npc = outEntity as BaseNpc;
                    if (npc != null) npc.Resume();

                }

                player.Message(GetMessage("tracklookDisabled", player.Id));

                return;
            }

            var noRigid = false;
            if (args.Length > 0 && !bool.TryParse(args[0], out noRigid))
            {
                player.Message(GetMessage("notBool", player.Id));
                return;
            }

            var max = 500;
            var startColl = 0;
            var curColl = startColl;
            BaseEntity entity = null;


            var pObj = player?.Object as BasePlayer;

            for (int i = 0; i < max; i++)
            {
                entity = GetLookAtEntity(pObj, 100, curColl);

                if (entity != null) break;
                

                if (curColl == 0) curColl = 1;
                else
                {
                    if (curColl >= int.MaxValue) break;

                    var newVal = ((float)curColl) * 2;

                    curColl = newVal >= int.MaxValue ? int.MaxValue : (int)newVal;

                }
            }


            curColl = 0;


            if (entity == null) entity = GetLookAtEntity(pObj, 100, 1269916417);
            

            if (entity == null || entity.gameObject == null || entity.IsDestroyed)
            {
                player.Message(GetMessage("noEntityFound", player.Id));
                return;
            }


            npc = entity as BaseNpc;
            if (npc != null) npc.Pause();


            Subscribe(nameof(OnPlayerInput));
            _trackEntity[player.Id] = entity;
          

            player.Message("Track ent: " + ((entity == null) ? "disabled" : "enabled") + ", ent: " + entity);

            if (noRigid)
            {
                entity.Invoke(() =>
                {
                    var trackNow = entity?.GetComponent<TrackMovement>() ?? null;
                    if (trackNow != null) trackNow.ToggleRigid(false);
                }, 0.1f);
            }

        }

        [ChatCommand("lookat")]
        private void cmdLookAt(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var layer = -1;
            if (args.Length > 0 && !int.TryParse(args[0], out layer))
            {
                SendReply(player, "Layer is not an int: " + args[0]);
                return;
            }

            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new Ray(player.eyes.position, currentRot);
            RaycastHit hitt;

            if (Physics.Raycast(ray, out hitt, 10f, layer))
            {
                var getEnt = hitt.GetEntity();

                var type = hitt.GetEntity()?.GetType()?.ToString() ?? hitt.GetCollider()?.GetType()?.ToString() ?? "Unknown Type";
                var prefabName = hitt.GetEntity()?.PrefabName ?? hitt.GetCollider()?.GetComponent<MeshCollider>()?.sharedMesh?.name ?? hitt.GetCollider()?.name ?? "Unknown Prefab";
                var shortPrefab = hitt.GetEntity()?.ShortPrefabName ?? "Unknown";
                var pos = (getEnt?.transform?.position ?? hitt.collider?.transform?.position ?? Vector3.zero);
                var rot = getEnt?.transform != null ? getEnt.transform.rotation : Quaternion.identity;
                var rot2 = getEnt?.transform != null ? getEnt.transform.eulerAngles : Vector3.zero;
                var owner = GetDisplayNameFromID(hitt.GetEntity()?.OwnerID ?? 0);
                var health = hitt.GetEntity()?.Health() ?? 0f;
                var maxHealth = hitt.GetEntity()?.MaxHealth() ?? 0f;
                var hpFrac = getEnt?.GetComponent<BaseCombatEntity>()?.healthFraction ?? 0f;
                var dmgSB = new StringBuilder();
                var baseProt = hitt.GetEntity()?.GetComponent<BaseCombatEntity>()?.baseProtection ?? null;
                var slot = hitt.GetEntity()?.GetSlot(BaseEntity.Slot.Lock)?.GetComponent<CodeLock>() ?? null;
                if (baseProt != null && baseProt.amounts.Length > 0)
                {
                    for (int i = 0; i < baseProt.amounts.Length; i++) dmgSB.Append(baseProt.amounts[i] + "f, ");
                }
                if (string.IsNullOrEmpty(owner)) owner = "Unknown/None";
                SendReply(player, prefabName + " (" + shortPrefab + ")\nType: " + type + "\nPos: " + pos + " (Point: " + hitt.point + ")\nRotation (quaternion and euler): " + rot + "   " + rot2 + "\nOwner: " + owner + "\nHealth: " + health + "/" + maxHealth + " (Frac: " + hpFrac + ") " + dmgSB.ToString().TrimEnd() + "\n" + "Code Lock (" + (slot?.PrefabName ?? string.Empty) + ")\nSkin ID: " + (getEnt?.skinID ?? 0) + "\nIsDestroyed: " + (getEnt?.IsDestroyed ?? true) + "\nBuilding ID: " + ((getEnt as BuildingBlock)?.buildingID ?? 0) + "\nGlobal broadcast: " + (getEnt?.globalBroadcast ?? false));
                if (getEnt != null && !(getEnt?.IsDestroyed ?? true)) SendReply(player, "Saving enabled: " + (getEnt?.enableSaving ?? false) + ", is in savelist?: " + BaseEntity.saveList.Contains(getEnt));
                else PrintWarning("NULL: " + (getEnt?.IsDestroyed ?? true));
                if (dmgSB.Length > 0) PrintWarning(dmgSB.ToString().TrimEnd());
                if (getEnt != null) SendReply(player, "Prefab ID: " + getEnt?.prefabID + Environment.NewLine + "Layer: " + getEnt.gameObject.layer + ", " + LayerMask.LayerToName(getEnt.gameObject.layer) + ", mask: " + LayerMask.GetMask(LayerMask.LayerToName(getEnt.gameObject.layer)) + Environment.NewLine + "Parent: " + (getEnt?.GetParentEntity()?.ShortPrefabName ?? "Not Parented"));
                if (getEnt is Signage)
                {
                    var sign = getEnt as Signage;
                    if (sign != null) SendReply(player, "Texture ID(s): " + string.Join(", ", sign.textureIDs));
                }
                if (getEnt is SleepingBag)
                {
                    var bag = getEnt as SleepingBag;
                    if (bag != null) 
                    {
                        var dpl = BasePlayer.FindAwakeOrSleeping(bag.deployerUserID.ToString());
                        SendReply(player, "Respawn point of " + bag.deployerUserID + (dpl != null ? " (" + dpl.displayName + ")" : ""));
                    }
                    
                }

                var lootEnt = getEnt as LootContainer;
                if (lootEnt != null) SendReply(player, "panelName: " + lootEnt.panelName);

                var npc = getEnt as NPCPlayer;
                if (npc != null)
                {
                    var plyPos = player?.transform?.position ?? Vector3.zero;
                    var facingDir = Vector3Ex.Direction2D(plyPos, npc.transform.position);

                    npc.SetAimDirection(facingDir);

                    npc.Invoke(() =>
                    {

                        SendReply(player, "should look");
                        npc.eyes.rotation = player.eyes.rotation;
                        
                        npc.finalDestination = player.transform.position;

                        npc.SendNetworkUpdate_Position();
                        npc.MovementUpdate(0.0f);

                        var scientist = npc as ScientistNPC;
                        if (scientist != null)
                        {
                            scientist.Brain.Navigator.SetFacingDirectionEntity(player);
                            scientist.EquipWeapon();
                            SendReply(player, "set facing dir entity and called equipweapon!");
                        }
                    }, 2f);
                    

                    SendReply(player, "NPC AI info: Nav running: " + npc.IsNavRunning() + ", HasPath: " + npc.HasPath + ", OnNavMeshLink: " + npc.IsOnNavMeshLink);
                }

                var campFire = getEnt as BaseOven;
                if (campFire != null)
                {
                    SendReply(player, "oven!");

                    var trig = campFire.GetComponent<TriggerTemperature>();
                    if (trig != null) SendReply(player, "trigger temperature!: " + trig.Temperature);

                    var comfTrig = campFire.GetComponent<TriggerComfort>();
                    if (comfTrig != null) SendReply(player, "comfort trigger: " + comfTrig.baseComfort);
                }

                var quarry = getEnt as MiningQuarry;
                if (quarry != null)
                {
                    var quarrySb = new StringBuilder();
                    foreach(var res in quarry._linkedDeposit._resources)
                    {
                        quarrySb.Append(res.isLiquid + ", " + res.type.displayName.english);   
                    }

                    SendReply(player, quarrySb.ToString());

                }

                var dropBack = getEnt as DroppedItemContainer;

                if (dropBack != null) SendReply(player, dropBack._name + ", " + dropBack.lootPanelName + ", " + dropBack.playerName + ", steamid: " + dropBack.playerSteamID);

                if (getEnt is VehicleModuleStorage modStorage)
                {
                    var storageRef = (EntityRef)_storageUnitInstance.GetValue(modStorage);

                    var get = storageRef.Get(true);

                    SendReply(player, "modStorage: " + get.ShortPrefabName + " type: " + get.GetType() + ", parent?: " + get?.GetParentEntity());

                    var liquid = get as LiquidContainer;

                    if (liquid != null)
                    {
                        PrintWarning("liquid! default: " + liquid.defaultLiquid.shortname);

                        for (int i = 0; i < liquid.ValidItems.Length; i++)
                        {
                            var item = liquid.ValidItems[i];
                            PrintWarning(item.shortname + " (" + item.itemid + ")");
                        }

                        for (int i = 0; i < liquid.inventory.itemList.Count; i++)
                        {
                            var item = liquid.inventory.itemList[i];
                            if (item != null)
                                RemoveFromWorld(item);
                                
                        }

                        liquid.inventory.canAcceptItem = null;
                        liquid.inventory.SetOnlyAllowedItems(liquid.ValidItems);
                        liquid.inventory.allowedContents = ItemContainer.ContentsType.Generic;
                        liquid.inventory.MarkDirty();

                        var fuelTest = ItemManager.CreateByName("lowgradefuel", UnityEngine.Random.Range(1, 1000));
                        if (!fuelTest.MoveToContainer(liquid.inventory))
                        {
                            PrintWarning("failed to move!!");
                            RemoveFromWorld(fuelTest);
                        }
                        else PrintWarning("moved!");
                    
                    }

                }

            }
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        [ChatCommand("lookat2")]
        private void cmdLookAt2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1;
            if (args.Length > 0 && !int.TryParse(args[0], out layer))
            {
                SendReply(player, "Layer is not an int: " + args[0]);
                return;
            }
            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new Ray(player.eyes.position, currentRot);
            //     RaycastHit hitt;
            var water = LayerMask.GetMask("Water");
            var hits = UnityEngine.Physics.SphereCastAll(ray, 1f, 10f, layer);
            if (hits != null && hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    var hitt = hits[i];
                    var getEnt = hitt.GetEntity();
                    if (getEnt != null && getEnt == player) continue;

                    var type = hitt.GetEntity()?.GetType()?.ToString() ?? hitt.GetCollider()?.GetType()?.ToString() ?? "Unknown Type";
                    var prefabName = hitt.GetEntity()?.PrefabName ?? hitt.GetCollider()?.GetComponent<MeshCollider>()?.sharedMesh?.name ?? hitt.GetCollider()?.name ?? "Unknown Prefab";
                    var shortPrefab = hitt.GetEntity()?.ShortPrefabName ?? "Unknown";
                    var pos = getEnt?.transform != null ? getEnt.transform.position : hitt.point;
                    var rot = getEnt?.transform != null ? getEnt.transform.rotation : Quaternion.identity;
                    var rot2 = getEnt?.transform != null ? getEnt.transform.eulerAngles : Vector3.zero;
                    var owner = GetDisplayNameFromID(hitt.GetEntity()?.OwnerID ?? 0);
                    var health = hitt.GetEntity()?.Health() ?? 0f;
                    var maxHealth = hitt.GetEntity()?.MaxHealth() ?? 0f;
                    var hpFrac = getEnt?.GetComponent<BaseCombatEntity>()?.healthFraction ?? 0f;
                    var dmgSB = new StringBuilder();
                    var baseProt = hitt.GetEntity()?.GetComponent<BaseCombatEntity>()?.baseProtection ?? null;
                    var slot = hitt.GetEntity()?.GetSlot(BaseEntity.Slot.Lock)?.GetComponent<CodeLock>() ?? null;
                    if (baseProt != null && baseProt.amounts.Length > 0)
                    {
                        for (int j = 0; j < baseProt.amounts.Length; j++) dmgSB.Append(baseProt.amounts[j] + "f, ");
                    }
                    if (string.IsNullOrEmpty(owner)) owner = "Unknown/None";
                    SendReply(player, prefabName + " (" + shortPrefab + ")\nType: " + type + "\nPos: " + pos + "\nRotation (quaternion and euler): " + rot + "   " + rot2 + "\nOwner: " + owner + "\nHealth: " + health + "/" + maxHealth + " (Frac: " + hpFrac + ") " + dmgSB.ToString().TrimEnd() + "\n" + "Code Lock (" + (slot?.PrefabName ?? string.Empty) + ")\nSkin ID: " + (hitt.GetEntity()?.skinID ?? 0) + "\nIsDestroyed: " + (getEnt?.IsDestroyed ?? true) + "\nBuilding ID: " + ((getEnt as BuildingBlock)?.buildingID ?? 0) + "\nGlobal broadcast: " + (getEnt?.globalBroadcast ?? false));
                    if (getEnt != null && !(getEnt?.IsDestroyed ?? true)) SendReply(player, "Saving enabled: " + (getEnt?.enableSaving ?? false) + ", is in savelist?: " + BaseEntity.saveList.Contains(getEnt));
                    else PrintWarning("NULL: " + (getEnt?.IsDestroyed ?? true));
                    if (dmgSB.Length > 0) PrintWarning(dmgSB.ToString().TrimEnd());
                    if (getEnt != null) SendReply(player, "Layer: " + getEnt.gameObject.layer + ", " + LayerMask.LayerToName(getEnt.gameObject.layer) + ", mask: " + LayerMask.GetMask(LayerMask.LayerToName(getEnt.gameObject.layer)));
                    if (getEnt != null && getEnt is BradleyAPC)
                    {
                        var brad = getEnt as BradleyAPC;
                        SendReply(player, brad.mainCannonMuzzleFlash.resourcePath + "   --  " + brad.mainCannonProjectile.resourcePath);
                    }
                    if (getEnt != null && getEnt is FlameTurret)
                    {
                        var flamer = getEnt as FlameTurret;
                        SendReply(player, flamer.triggeredEffect.resourcePath + " - " + flamer.explosionEffect.resourcePath + " - " + flamer.fireballPrefab.resourcePath);
                    }
                    if (getEnt is Signage)
                    {
                        var sign = getEnt as Signage;
                        if (sign != null) SendReply(player, "Texture ID(s): " + string.Join(", ", sign.textureIDs));
                    }

                    if (getEnt != null) getEnt.SendNetworkUpdate();
                }
            }

        }

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player == null || input == null) return;

            if (_trackEntity?.Count < 1)
            {
                Unsubscribe(nameof(OnPlayerInput));
                PrintWarning("Unsubscribe automatically due to no players tracklooking");
                return;
            }

            BaseEntity trackEnt;
            if (_trackEntity.TryGetValue(player.UserIDString, out trackEnt) && !(trackEnt?.IsDestroyed ?? true))
            {
                var getTrack = trackEnt?.GetComponent<TrackMovement>() ?? null;
                if (getTrack == null)
                {
                    getTrack = trackEnt.gameObject.AddComponent<TrackMovement>();
                    getTrack.Parent = player;
                }

                if (trackEnt is SearchLight)
                {
                    var searchLight = trackEnt as SearchLight;
                    searchLight.SetTargetAimpoint(searchLight.eyePoint.transform.position + player.eyes.BodyForward() * 100f); //sets the light to point at where the player is looking
                }

                trackEnt.enableSaving = false;
                var speed = (input.IsDown(BUTTON.SPRINT) && input.IsDown(BUTTON.DUCK) && input.IsDown(BUTTON.RELOAD)) ? 128 : (input.IsDown(BUTTON.RELOAD) && !input.IsDown(BUTTON.DUCK)) ? 20 : player.IsRunning() ? 12 : (input.IsDown(BUTTON.DUCK) && input.IsDown(BUTTON.RELOAD)) ? 1.5f : 5;
                getTrack.Speed = speed;
                getTrack.ChangeRotation = input.IsDown(BUTTON.USE);
                if (input.WasJustPressed(BUTTON.FIRE_THIRD) || input.IsDown(BUTTON.FIRE_THIRD))
                {
                    var oldTrack = getTrack.DistInFront;
                    getTrack.DistInFront += input.IsDown(BUTTON.FIRE_SECONDARY) ? -0.25f : 0.25f;
                    SendReply(player, "Dist in front: " + getTrack.DistInFront + " (was: " + oldTrack + ")");
                }
            }

        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);
            if (Physics.Raycast(ray, out hit, maxDist, (coll != -1) ? coll : _constructionColl))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }

        private string GetDisplayNameFromID(ulong userID) { return GetDisplayNameFromID(userID.ToString()); }

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return covalence?.Players?.FindPlayerById(userID)?.Name ?? string.Empty;
        }

        private class TrackMovement : MonoBehaviour
        {
            public BaseEntity Entity { get; private set; }
            public BasePlayer Parent { get; set; }

            private BasePlayer _playerEntity;
            public BasePlayer PlayerEntity
            {
                get
                {
                    if (_playerEntity != null && Entity != null && _playerEntity != Entity) _playerEntity = null;

                    if (_playerEntity == null && Entity != null && !Entity.IsDestroyed && Entity.gameObject != null)
                    {
                        _playerEntity = Entity as BasePlayer;
                    }

                    return _playerEntity;
                }
            }


            public float Speed = 5;
            public float DistInFront = 3.5f;

            public bool ChangeRotation { get; set; } = false;
            public bool HasRigid { get; set; } = true;
            public bool UseEyes { get; set; } = true;

            private void Awake()
            {
                Entity = GetComponent<BaseEntity>();
                if (Entity == null || Entity.IsDestroyed)
                {
                    DoDestroy();
                    return;
                }
            }

            public void ToggleRigid(bool val)
            {
                if (Entity == null || Entity.IsDestroyed) return;
                var rigid = Entity?.GetComponent<Rigidbody>() ?? Entity?.GetComponentInParent<Rigidbody>() ?? Entity?.GetComponentInChildren<Rigidbody>() ?? null;
                if (rigid != null)
                {
                    rigid.isKinematic = !val;
                    rigid.useGravity = val;
                    HasRigid = val;
                }
            }

            public void DoDestroy()
            {
                try { if (!HasRigid) ToggleRigid(true); }
                finally { Destroy(this); }
            }

            private void Update()
            {
                if (Entity == null || Entity?.transform == null || Parent == null || Parent?.transform == null || Parent.IsDestroyed) return;
                var lerpPos = Vector3.Lerp(Entity.transform.position, (UseEyes ? Parent.eyes.position : Parent.transform.position) + Parent.eyes.HeadRay().direction * DistInFront, Time.deltaTime * Speed);
                Entity.transform.position = lerpPos;

                var entPlayer = PlayerEntity;
                if (entPlayer?.net != null && entPlayer.IsConnected) entPlayer.ClientRPCPlayer(null, entPlayer, "ForcePositionTo", lerpPos);


                if (ChangeRotation)
                {
                    var aimAngles = Parent?.serverInput?.current?.aimAngles ?? Vector3.zero;

                    Assert.IsTrue(aimAngles != Vector3.zero, "Parent aimAngles is Vector3.zero!");

                    Entity.transform.rotation = Quaternion.Euler(Parent?.serverInput?.current?.aimAngles ?? Vector3.zero);  //rotEuler;
                }

                Entity.OnPositionalNetworkUpdate();
                Entity.UpdateNetworkGroup();
                Entity.SendNetworkUpdateImmediate();
            }

        }

        #region Util

        private bool HasPerms(string userId, string perm)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(perm)) throw new ArgumentNullException(nameof(perm));

            if (userId.Equals("server_console", StringComparison.OrdinalIgnoreCase)) return true;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try { return permission.UserHasPermission(userId, !perm.StartsWith("tracklook") ? sb.Clear().Append("tracklook.").Append(perm).ToString() : perm); }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private void SendNoPerms(IPlayer player) => player?.Message(GetMessage("noPerms", player.Id));
        private void SendNoPerms(BasePlayer player) { if (player != null && player.IsConnected) player.ChatMessage(GetMessage("noPerms", player.UserIDString)); }


        #endregion

    }
}