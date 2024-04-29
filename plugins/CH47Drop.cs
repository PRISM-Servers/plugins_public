using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("CH47Drop", "Shady", "1.0.94", ResourceId = 0)]
    internal class CH47Drop : RustPlugin
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
        private readonly Dictionary<CH47AIBrain, Action> _brainActions = new Dictionary<CH47AIBrain, Action>();

        private readonly HashSet<CH47HelicopterAIController> _oilrigChinook = new HashSet<CH47HelicopterAIController>();    

        private readonly HashSet<CH47AntiStuck> _antiStuckBehaviors = new HashSet<CH47AntiStuck>();

        #region Config
        private Dictionary<string, object> Monuments => GetConfig("Monuments", new Dictionary<string, object>());


        protected override void LoadDefaultConfig()
        {
            try
            {
                try
                {
                    for (int i = 0; i < MonInfos.Count; i++)
                    {
                        try 
                        {
                            var monument = MonInfos[i];
                            if (monument == null) continue;
                            var monName = monument?.displayPhrase?.english ?? string.Empty;
                            if (string.IsNullOrEmpty(monName)) continue;
                            var dropZone = monument?.GetComponentInChildren<CH47DropZone>() ?? null;
                            object outObj;
                            if (!Monuments.TryGetValue(monName, out outObj)) Monuments[monName] = (dropZone != null);
                        }
                        catch(Exception ex) { PrintError(ex.ToString()); }
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }
                Config["Monuments"] = Monuments;
                SaveConfig();
            }
            catch(Exception ex) { PrintError(ex.ToString()); }
         
        }
        #endregion
        #region Fields
        public List<MonumentInfo> MonInfos { get { return TerrainMeta.Path?.Monuments ?? null; } }
        private readonly HashSet<MonumentInfo> DisabledZones = new HashSet<MonumentInfo>();
        private readonly HashSet<MonumentInfo> AddedZones = new HashSet<MonumentInfo>();

        #endregion
        #region Hooks
        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
            Unsubscribe(nameof(OnEntityKill));
        }

        private void OnServerInitialized()
        {
            try
            {

                Subscribe(nameof(OnEntitySpawned));
                Subscribe(nameof(OnEntityKill));

                LoadDefaultConfig();

                foreach(var ent in BaseNetworkable.serverEntities)
                {
                    var ch47 = ent as CH47HelicopterAIController;

                    if (ch47 != null)
                        _antiStuckBehaviors.Add(ch47.gameObject.AddComponent<CH47AntiStuck>());
                    
                }

                foreach (var kvp in Monuments)
                {
                    if (!(kvp.Value is bool))
                    {
                        PrintError("Config value is not a bool!!: " + kvp.Value);
                        continue;
                    }
                    var val = (bool)kvp.Value;
                    for(int i = 0; i < MonInfos.Count; i++)
                    {
                        var monInfo = MonInfos[i];
                        if (monInfo == null || monInfo?.gameObject == null || string.IsNullOrEmpty(monInfo?.displayPhrase?.english ?? string.Empty) || monInfo?.displayPhrase?.english != kvp.Key) continue;
                        var dropZone = monInfo?.GetComponentInChildren<CH47DropZone>() ?? null;
                        if (dropZone == null && val)
                        {
                            var newZone = monInfo.gameObject.AddComponent<CH47DropZone>();
                            newZone.Awake();
                            AddedZones.Add(monInfo);
                        }
                        if (!val && dropZone != null && dropZone?.gameObject != null)
                        {
                            dropZone.OnDestroy();
                            UnityEngine.Object.Destroy(dropZone);
                            DisabledZones.Add(monInfo);
                        }
                    }
                }                
            }
            catch(Exception ex) { PrintError(ex.ToString()); }
        }

        private void OnSpawnOilrigCH47(CH47HelicopterAIController ch47, CH47ReinforcementListener listener)
        {
            PrintWarning("oil rig ch47 spawn!");
            PrintWarning("oil rig chinook spawned!! adding to list");

            _oilrigChinook.Add(ch47);
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            var ch47 = entity as CH47HelicopterAIController;
            if (ch47 != null) _oilrigChinook.Remove(ch47);
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            //   if (entity?.prefabID != 1675349834) return; //only detect "normal" chinooks

            if (entity?.prefabID != 1514383717) return; //only detect scientists chinooks which drop the crate for goodness sake this plugin has been broken for forever because of above check which ONLY WORKS FOR USER CONTROLLED CHINOOKS?!

            var ch47 = entity as CH47HelicopterAIController;
            if (ch47 == null) return;

            _antiStuckBehaviors.Add(ch47.gameObject.AddComponent<CH47AntiStuck>());

            PrintWarning("ch47 spawn (onentityspawned)");

            if (_oilrigChinook.Contains(ch47))
            {
                PrintWarning("oil rig chinook hashset contained ch47, we will NOT modify this chinook");
                return;
            }
            
            /*/
            var nearestMon = GetNearestMonument((entity?.transform?.position ?? Vector3.zero));

            if (nearestMon == LargeOilRig || nearestMon == SmallOilRig)
            {
                PrintWarning("ch47 is near large/oil rig: " + entity + " @ " + entity.transform.position + ", dist: " + Vector3.Distance(entity.transform.position, SmallOilRig.transform.position) + " large dist: " + Vector3.Distance(entity.transform.position, LargeOilRig.transform.position));
                return;
            }/*/


         

            var zones = Pool.GetList<CH47DropZone>();
            try
            {
                GetDropZonesNoAlloc(ref zones);

                if (zones == null || zones.Count < 1)
                {
                    PrintWarning("no drop zones on OnEntitySpawned for chinook");
                    return;
                }

                var rngZone = zones[UnityEngine.Random.Range(0, zones.Count)];
                if (rngZone == null)
                {
                    PrintWarning("rngZone null!!");
                    return;
                }
                
                var rngZoneName = (rngZone?.GetComponentInParent<MonumentInfo>() ?? rngZone?.GetComponentInChildren<MonumentInfo>() ?? rngZone?.GetComponent<MonumentInfo>() ?? null)?.displayPhrase?.english ?? rngZone?.name ?? string.Empty;
                PrintWarning("Got random zone for chinook: " + rngZoneName);

                timer.Once(1f, () =>
                {
                    if (ch47 == null || ch47.IsDestroyed)
                    {
                        PrintWarning("Chinook is null after 1 second");
                        return;
                    }

                    SetDropDestination(ch47, rngZone);
                    PrintWarning("ch47 is now heading toward (it will visit others first): " + (rngZone?.transform?.position ?? Vector3.zero) + ", name: " + rngZoneName);
                });
            }
            finally { Pool.FreeList(ref zones); }
         
        }

        private void Unload()
        {
            
            foreach(var antiStuck in _antiStuckBehaviors)
                antiStuck?.DoDestroy();
            

            foreach (var kvp in _brainActions)
            {
                InvokeHandler.CancelInvoke(kvp.Key, kvp.Value);
            }
            
            if (DisabledZones != null && DisabledZones.Count > 0)
            {
                foreach (var mon in DisabledZones)
                {
                    if (mon == null)
                    {
                        PrintWarning("monument info is entirely null");
                        continue;
                    }

                    if (mon == null || mon.gameObject == null)
                    {
                        PrintWarning("mon gameobject is null in DisabledZones!!");
                        continue; //this probably shouldn't happen
                    }

                    var zone = mon?.GetComponentInChildren<CH47DropZone>() ?? null;
                    if (zone != null)
                    {
                       // PrintWarning("zone is null in DisabledZones!!");
                        continue; //this probably shouldn't happen either
                    }
                    
                    
                    var drop = mon.gameObject.AddComponent<CH47DropZone>();
                    drop.Awake();
                }
            }

            if (AddedZones != null && AddedZones.Count > 0)
            {
                foreach (var mon in AddedZones)
                {
                    if (mon == null || mon?.gameObject == null) continue;
                    var zone = mon?.GetComponentInChildren<CH47DropZone>() ?? null;
                    if (zone != null && zone?.gameObject != null)
                    {
                        zone.OnDestroy();
                        UnityEngine.Object.Destroy(zone);
                    }
                }
            }
        }
        #endregion
        #region AntiStuck
        private class CH47AntiStuck: FacepunchBehaviour
        {
            public CH47HelicopterAIController Heli { get; set; }

            public CH47AIBrain Brain { get; set; }

            private void Awake()
            {
                Heli = GetComponent<CH47HelicopterAIController>();
                if (Heli == null || Heli.IsDestroyed)
                {
                    Interface.Oxide.LogError(nameof(CH47AntiStuck) + " awake called but null heli!!!!");
                    return;
                }

                Brain = Heli?.GetComponent<CH47AIBrain>();

                InvokeRepeating(StuckCheck, 5f, 1.25f);

            }

            public Vector3 LastPosition { get; set; } = Vector3.zero;
            
            public Vector3 CurrentPosition
            {
                get { return Heli?.transform?.position ?? Vector3.zero; }
            }

            public float CurrentDistanceFromLastPosition()
            {
                var dist = Vector3.Distance(CurrentPosition, LastPosition);

                if (dist < 5) Interface.Oxide.LogWarning(nameof(CurrentDistanceFromLastPosition) + " is: " + dist);

                return dist;
            }

            private int _timesStuck = 0;

            private int _timesUnstuck = 0;

            public void StuckCheck()
            {
                if (LastPosition == Vector3.zero)
                {
                    LastPosition = CurrentPosition;
                    return;
                }

                var timesUntilUnStick = 10;

                if (_timesStuck >= timesUntilUnStick)
                {
                    Interface.Oxide.LogWarning(nameof(_timesStuck) + " >= " + timesUntilUnStick + "!!! - try unstuck");

                    _timesStuck = 0;
                    TryUnstick();

                    return;
                }

                var curDist = CurrentDistanceFromLastPosition();

                if (curDist < 1.33f)
                {
                    _timesStuck++;
                }

                

                LastPosition = CurrentPosition;
            }

            public void TryUnstick()
            {
              
                if (_timesUnstuck > 0)
                {
                    Interface.Oxide.LogWarning(nameof(TryUnstick) + " has already been called once, so we're just force dropping crate if stuck in crate state");

                    if (Brain.CurrentState == Brain.states[AIState.DropCrate])
                    {
                        Interface.Oxide.LogWarning("was current state drop crate, force drop!");
                        Heli.DropCrate();

                        return;
                    }
                    else Interface.Oxide.LogWarning("WAS NOT STATE DROP CRATE!!!! is: " + Brain.CurrentState);

                }

                Interface.Oxide.LogWarning(nameof(TryUnstick));

                Interface.Oxide.LogWarning("evidently we're stuck, so lets get some info:");

                Interface.Oxide.LogWarning("brain info: " + Brain.CurrentState + " (" + Brain.currentStateContainerID + ")");

                Heli.transform.position = new Vector3(CurrentPosition.x + 50, CurrentPosition.y + 15f, CurrentPosition.z + 25);

                var oldInterest = Brain.mainInterestPoint;

                Brain.mainInterestPoint = new Vector3(5000, 175, 5000);

                Brain.SwitchToState(AIState.Roam);

             

                Brain.Invoke(() =>
                {

                    if (Heli.OutOfCrates())
                    {
                        Interface.Oxide.LogWarning("out of crates on unstick!!!");

                        Brain.mainInterestPoint = new Vector3(5000, 175, 5000);

                        Brain.SwitchToState(AIState.Egress);
                   

                    }
                    else
                    {
                        Interface.Oxide.LogWarning("Not out of crates - try drop again!!!");

                        Brain.mainInterestPoint = new Vector3(oldInterest.x, oldInterest.y + 10f, oldInterest.z);

                        Brain.SwitchToState(AIState.DropCrate);

                     
                    }

                }, 8f);


                _timesUnstuck++;
            }

            public void DoDestroy()
            {
                try 
                {
                    CancelInvoke(StuckCheck);
                }
                finally { Destroy(this); }
            }

        }
        #endregion
        #region Util

        private void SetDropDestination(CH47HelicopterAIController heli, CH47DropZone zone, bool visitOthersBeforeDropping = true)
        {
            if (heli == null || heli.IsDestroyed || heli.IsDead() || zone == null)
            {
                PrintWarning("bad destination call args");
                return;
            }

            var zonePos = zone?.transform?.position ?? Vector3.zero;
            var originalY = zonePos.y;
            var hasVisitedAll = false;
            
          

            heli.SetMoveTarget(zonePos);

            var brain = heli?.GetComponent<CH47AIBrain>() ?? null;
            if (brain == null)
            {
                PrintWarning("ch47ai brain is null!!!!");
                return;
            }


       
            brain.SwitchToState(AIState.Patrol);
         
            brain.mainInterestPoint = zonePos;
            brain.mainInterestPoint.y += UnityEngine.Random.Range(80f, 150f);

            //perhaps we make it so that it visits a few locations before dropping the crate/running rng to see if we want to spawn at this one or another one?

            var toVisit = new List<CH47DropZone>();
            var zones = Pool.GetList<CH47DropZone>();
            try 
            {
                GetDropZonesNoAlloc(ref zones);

                CH47DropZone _last = null;
                
                for (int i = 0; i < 3; i++)
                {
                    var zoneToUse = zones.GetRandom();

                    if (zoneToUse == zone) zoneToUse = zones.GetRandom();
                    
                    if (zoneToUse == _last) zoneToUse = zones.GetRandom();
                    if (zoneToUse == _last || zoneToUse == zone) continue;

                    toVisit.Add(zoneToUse);
                    _last = zoneToUse;

                }
                
            }
            finally { Pool.FreeList(ref zones); }


            //smooth flight height variation?


            var nearPosTimes = 0;

            Action brainAction = null;
            brainAction = new Action(() =>
            {
                try
                {

                    if (brain == null || brain?._baseEntity == null || (brain?._baseEntity?.IsDestroyed ?? true))
                    {
                        try { brain.CancelInvoke(brainAction); }
                        catch (Exception ex) { PrintError(ex.ToString()); }
                    }

                    var currentPos = brain?._baseEntity?.transform?.position ?? Vector3.zero;

                    if (toVisit?.Count > 0)
                    {
                        var currentVisit = toVisit[0];
                        var currentVisitPos = currentVisit?.transform?.position ?? Vector3.zero;

                        var pos = SpreadVector(currentVisitPos, UnityEngine.Random.Range(6f, 84f));
                        pos.y = currentVisitPos.y + UnityEngine.Random.Range(20, 70);


                        var compPos = pos;
                        compPos.y = currentPos.y;

                        var dist = Vector3.Distance(compPos, currentPos);

                        if (dist < 160f)
                        {
                            if (nearPosTimes <= 0) brain.SwitchToState(AIState.Orbit);

                            nearPosTimes++;

                            if (nearPosTimes >= 8)
                            {
                                toVisit.Remove(currentVisit);

                                hasVisitedAll = toVisit.Count < 1;

                                nearPosTimes = 0;
                            }


                        }
                        else
                        {
                            brain.SwitchToState(AIState.Patrol);

                            brain.mainInterestPoint = pos;
                            brain.mainInterestPoint.y += UnityEngine.Random.Range(80f, 150f);
                        }

                    }

                    if (!hasVisitedAll)
                    {
                        //PrintWarning("has not yet visited all");
                        return;
                    }




                    BaseAIBrain.BasicAIState _dropState;
                    if (!brain.states.TryGetValue(AIState.DropCrate, out _dropState))
                    {
                        PrintWarning("Could not get drop state with DropCrate!!");
                        return;
                    }

                    if ((brain.CurrentState == _dropState) || (brain.states.TryGetValue(AIState.Egress, out _dropState) && _dropState == brain.CurrentState))
                    {

                        if (heli.OutOfCrates())
                        {

                            brain.SwitchToState(AIState.Egress);
                            brain.mainInterestPoint = new Vector3(5000, 175, 5000);

                        }

                        return;
                    }

                    zonePos = SpreadVector(zonePos, UnityEngine.Random.Range(2f, 12f));
                    zonePos.y = originalY;

                    brain.mainInterestPoint = zonePos;
                    brain.mainInterestPoint.y += UnityEngine.Random.Range(80f, 150f);

                    var adjustedTarget = zonePos;
                    adjustedTarget.y = currentPos.y;

                    var distFromInterest = Vector3.Distance(currentPos, adjustedTarget);
                    if (distFromInterest <= 150)
                        brain.SwitchToState(AIState.DropCrate);

                }
                catch (Exception ex) { PrintError(ex.ToString()); }
                
            });

            _brainActions[brain] = brainAction;
            
            brain.InvokeRepeating(brainAction, 8f, 8f);

        }

        private List<CH47DropZone> GetDropZones()
        {
            var monuments = TerrainMeta.Path?.Monuments;
            if (monuments == null || monuments.Count < 1) return null;
            
            var list = new List<CH47DropZone>();
            
            for (int i = 0; i < monuments.Count; i++)
            {
                var mon = monuments[i];
                if (mon == null || mon?.gameObject == null || mon?.transform == null) continue;
                
                var drop = mon?.GetComponentInChildren<CH47DropZone>() ?? null;
                if (drop != null && drop?.gameObject != null && drop.gameObject.activeSelf) list.Add(drop);
            }
            
            return list;
        }

        private void GetDropZonesNoAlloc(ref List<CH47DropZone> list)
        {
            if (list == null) 
                throw new ArgumentNullException(nameof(list));

            var monuments = TerrainMeta.Path?.Monuments;
            if (monuments == null || monuments.Count < 1)
            {
                PrintWarning("monuments is null on dropzonesnoalloc!");
                return;
            }

            list.Clear();

            var sb = Pool.Get<StringBuilder>();

            try 
            {

                sb.Clear().Append("monuments count: ").Append(monuments?.Count).Append(Environment.NewLine);

                for (int i = 0; i < monuments.Count; i++)
                {
                    var mon = monuments[i];
                    if (mon == null || mon?.gameObject == null || mon?.transform == null)
                    {
                        sb.Append("found null monument, gameobject or transform").Append(Environment.NewLine);
                        continue;
                    }

                    var drop = mon?.GetComponent<CH47DropZone>() ?? mon?.GetComponentInChildren<CH47DropZone>() ?? mon?.GetComponentInParent<CH47DropZone>();
                    if (drop?.gameObject != null && drop.gameObject.activeSelf && !list.Contains(drop))
                    {
                        sb.Append("added to list: ").Append((mon?.displayPhrase?.english ?? mon?.ToString() ?? string.Empty)).Append(Environment.NewLine);
                        list.Add(drop);
                    }

                }

                if (sb.Length > 1)
                    sb.Length -= 1;

                PrintWarning(sb.ToString());

            }
            finally { Pool.Free(ref sb); }

           
        }

        private MonumentInfo GetNearestMonument(Vector3 position)
        {
            var pos = position;
            pos.y = 0;

            var monDist = Pool.Get<Dictionary<MonumentInfo, float>>();//new Dictionary<MonumentInfo, float>();
            try
            {
                if (monDist?.Count > 0)
                {
                    PrintWarning("monDist.Count > 0!!!");
                    monDist.Clear();
                }

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
                return nearest;
            }
            finally { Pool.Free(ref monDist); }
        }

        private Vector3 SpreadVector(Vector3 vec, float spread = 1.5f) { return Quaternion.Euler(UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f), UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f), UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f)) * vec; }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

        private T GetConfig<T>(string name, T defaultValue) { return (Config[name] == null) ? defaultValue : (T)Convert.ChangeType(Config[name], typeof(T)); }
        
        #endregion
    }
}
