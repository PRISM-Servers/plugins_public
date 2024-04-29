using ConVar;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Time = UnityEngine.Time;
using Physics = UnityEngine.Physics;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("InteliLife", "Shady", "0.0.3", ResourceId = 0)]
    internal class InteliLife : RustPlugin
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
        /*--------------------------------------------------------------------------------//
		//                        PRISM InteliLife ARTIFICAL INTELLIGENCE                          //
		// THE CORE PLUGIN FOR ARTIFICAL LIFE SYSTEMS DEVELOPED FOR PRISM'S RUST SERVERS - INTENDED TO BE LIGHTWEIGHT AND BUILD UPON EXISTING RUST AI //
		//-----------------------------------------------------------------------------------*/

        /*--------------------------------------------------------------------------//
		//	PRIVATE PLUGIN - Developed by Shady - Not intended for public release  //
		//-------------------------------------------------------------------------*/

        //moving custom map marker at some point for convoys

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        public static InteliLife Instance = null;

        private class InteliMove : MonoBehaviour
        {
            public ScientistNPC Scientist { get; private set; }
            public BaseAIBrain Brain
            {
                get;
                private set;
            }

            public bool Paused { get; set; }

            public Vector3 Destination { get; set; } = Vector3.zero;
            
            private Vector3 _lastSetDestination = Vector3.zero;

            private float _lastSetDestinationTime = -1f;

            public float SecondsSinceDestinationSet()
            {
                return _lastSetDestinationTime == -1f ? -1f : UnityEngine.Time.realtimeSinceStartup - _lastSetDestinationTime;
            }

            public BaseNavigator.NavigationSpeed NavSpeed { get; set; } = BaseNavigator.NavigationSpeed.Normal;


            private void Awake()
            {
                Scientist = GetComponent<ScientistNPC>();
                if (Scientist == null)
                {
                    Interface.Oxide.LogError(nameof(InteliMove) + "." + nameof(Awake) + "() called on a non-ScientistNPC type!");
                    DoDestroy();
                    return;
                }

                Brain = Scientist.GetComponent<BaseAIBrain>();

                Interface.Oxide.LogWarning("Fixing brain");

                Brain.Navigator.Agent.agentTypeID = -1372625422;

                Brain.Navigator.SetNavMeshEnabled(false);
                Brain.Navigator.DefaultArea = "Walkable";

                Brain.Navigator.Init(Scientist, Brain.Navigator.Agent);
                Brain.Navigator.SetNavMeshEnabled(true);

                Interface.Oxide.LogWarning("Fixed brain");

            }


            public void DoDestroy() => Destroy(this);

            private void Update()
            {

                if (Scientist == null || Scientist?.gameObject == null || Scientist.IsDestroyed || Scientist.IsDead())
                {
                    Interface.Oxide.LogWarning("Scientist has died! Calling DoDestroy()");
                    DoDestroy();
                    return;
                }

                if (Paused || Destination == Vector3.zero) return;

                var curPos = Scientist?.transform?.position ?? Vector3.zero;

                if (_lastSetDestination != Vector3.zero && Vector3.Distance(_lastSetDestination, Destination) <= 0.02 && SecondsSinceDestinationSet() < 1f)
                    return;

                Brain.Navigator.SetDestination(Destination, NavSpeed);
                _lastSetDestination = Destination;
                _lastSetDestinationTime = UnityEngine.Time.realtimeSinceStartup;

            }

        }

        private class MercenaryAI : MonoBehaviour
        {
            public RoadAI RoadAI { get; private set; }

            public ScientistNPC Scientist
            {
                get
                {
                    return RoadAI?.Scientist;
                }
            }

            public InteliMove Movement { get; private set; }

            public bool HasReachedJunkpile { get; set; } = false;

            private JunkPile _targetJunkpile = null;
            public JunkPile TargetJunkpile
            {
                get
                {
                    return _targetJunkpile;
                }
                set
                {
                    _targetJunkpile = value;
                    HasReachedJunkpile = false;
                }
            }

            public bool HasReachedBarrel { get; set; } = false;

            private LootContainer _targetBarrel = null;
            public LootContainer TargetBarrel
            {
                get
                {
                    return _targetBarrel;
                }
                set
                {
                    _targetBarrel = value;
                    HasReachedBarrel = false;
                }
            }

            private List<NetworkableId> _lootedPiles = new List<NetworkableId>();


            private void Awake()
            {
                Interface.Oxide.LogWarning("Awake() on MercenaryAI");

                RoadAI = GetComponent<RoadAI>();
                if (RoadAI == null)
                {
                    Interface.Oxide.LogError(nameof(MercenaryAI) + " Awake() but RoadAI does not yet exist!");
                    return;
                }


                if (Scientist == null)
                {
                    Interface.Oxide.LogError(nameof(MercenaryAI) + "." + nameof(Awake) + "() called on but Scientist is null!");
                    DoDestroy();
                    return;
                }
                else Interface.Oxide.LogWarning("Scientist is not null on MercenaryAI.Awake()!");

                Movement = GetComponent<InteliMove>();
                if (Movement == null) Movement = Scientist.gameObject.AddComponent<InteliMove>();
                else Interface.Oxide.LogWarning("Movement already existed, strangely (Awake)");

                BeginJunkpileInvoke();
            }

            private void BeginJunkpileInvoke(bool clearTarget = true)
            {
                Interface.Oxide.LogWarning(nameof(BeginJunkpileInvoke));

                if (clearTarget) TargetJunkpile = null;

                CancelJunkpileInvoke();

                InvokeHandler.InvokeRepeating(Scientist, TryMoveToNearestJunkpile, 1f, 0.25f);
            }

            private void CancelJunkpileInvoke()
            {
                InvokeHandler.CancelInvoke(Scientist, TryMoveToNearestJunkpile);
            }

            private void FixedUpdate()
            {

                Vector3 pos;

                if (TargetJunkpile != null && !TargetJunkpile.IsDestroyed && !HasReachedJunkpile)
                {
                    pos = Scientist?.transform?.position ?? Vector3.zero;

                    var junkPileAdjustedPosition = TargetJunkpile?.transform?.position ?? Vector3.zero;
                    junkPileAdjustedPosition.y = pos.y;

                    if (Vector3.Distance(pos, junkPileAdjustedPosition) < 6.75f)
                    {
                        OnReachedTargetJunkpile();
                    }
                   // else Interface.Oxide.LogWarning("scientist pos and targetjunk pos are not close enough");
                }
                
                /*/
                if (TargetBarrel != null && !TargetBarrel.IsDestroyed && !HasReachedBarrel)
                {
                    pos = Scientist?.transform?.position ?? Vector3.zero;
                    if (Vector3.Distance(pos, TargetBarrel.transform.position) <= 1f)
                    {
                        OnReachedTargetBarrel();
                    }
                    else Interface.Oxide.LogWarning("scientist pos and targetbarrel pos are not close enough");
                }/*/

                var shady = BasePlayer.FindByID(76561198028248023);
                if (shady != null && shady.IsAdmin && shady.IsConnected)
                {
                    var scientistEyes = Scientist.eyes.position;
                    
                    var currentRot = Quaternion.Euler(Scientist.viewAngles) * Vector3.forward;

                    RaycastHit hit;

                    if (Physics.Raycast(new Ray(scientistEyes, currentRot), out hit, 25f))
                    {
                        var lookingPos = hit.point;

                        shady.SendConsoleCommand("ddraw.arrow", 0.1f, Color.yellow, scientistEyes, lookingPos, 0.35f);
                    }

                }

            }

            private Coroutine _attackingCoroutine = null;

            System.Collections.IEnumerator MeleeAttackEntity(BaseEntity target)
            {
                if (target == null)
                    throw new ArgumentNullException(nameof(target));

                var wasTargetBarrelAlive = !(TargetBarrel?.IsDestroyed ?? true);

                var startHp = target?.Health() ?? 0f;

                var lastAttackTime = -1f;

                while (Scientist != null && !Scientist.IsDestroyed && target != null && !target.IsDestroyed && target.Health() > 0f)
                {

                    Scientist.Brain.Navigator.SetFacingDirectionEntity(target);
                    Scientist.Brain.Events.Memory.Entity.Set(target, 0);
                    Scientist.SetDucked(true);
                    
                    Scientist.EquipWeapon();
                    yield return CoroutineEx.waitForEndOfFrame;

                    Scientist.MeleeAttack();


                    if (lastAttackTime != -1f && (UnityEngine.Time.realtimeSinceStartup - lastAttackTime) >= 2 && (target?.Health() ?? 0f) == startHp)
                    {
                        Interface.Oxide.LogWarning("We haven't hurt this thing yet and plenty of time has passed, killing if lootcontainer/applicable");
                        if (target is LootContainer) target.Kill(BaseNetworkable.DestroyMode.Gib);
                       // Movement.Destination = Scientist.transform.position + Vector3.forward * UnityEngine.Random.Range(0.15f, 1f);
                    }

                    lastAttackTime = Time.realtimeSinceStartup;

                    yield return CoroutineEx.waitForSeconds(0.1f);
                }
                
                Scientist.SetDucked(false);

                Interface.Oxide.LogWarning("this thing has died (either the victim entity or scientist), setting attack coroutine to null");

                ServerMgr.Instance.StopCoroutine(_attackingCoroutine);
                _attackingCoroutine = null;

                if (wasTargetBarrelAlive && (TargetBarrel == null || TargetBarrel.IsDestroyed))
                    OnTargetBarrelDied();

             
            }

            private void OnTargetBarrelDied()
            {
                Interface.Oxide.LogWarning(nameof(OnTargetBarrelDied));
                
                var nextBarrel = FindNearestBarrel();

                if (nextBarrel != null)
                {
                    Interface.Oxide.LogWarning("nextBarrel is not null!");

                    TargetBarrel = nextBarrel;

                    if (!InvokeHandler.IsInvoking(Scientist, TryMoveToTargetBarrel)) 
                        InvokeHandler.InvokeRepeating(Scientist, TryMoveToTargetBarrel, 0.01f, 0.05f);

                    Interface.Oxide.LogWarning("set TargetBarrel to nextBarrel!");
                    return;
                }
                else Interface.Oxide.LogWarning("nextBarrel was null!");

                if (RoadAI.Paused)
                {
                    Interface.Oxide.LogWarning("Was paused!");
                    Interface.Oxide.LogWarning("Continuing RoadAI!");
                    ResumeRoadAI();
                }
                else Interface.Oxide.LogWarning("Was not paused!");


                EquipGun();
                Scientist.EquipWeapon();

                TargetJunkpile = null;

                BeginJunkpileInvoke();



            }

            private void EquipGun()
            {
                Interface.Oxide.LogWarning(nameof(EquipGun));
                
                var beltItems = Scientist?.inventory?.containerBelt?.itemList;

                if (beltItems == null || beltItems.Count < 1)
                    return;


                Item gun = null;

                for (int i = 0; i < beltItems.Count; i++)
                {
                    var item = beltItems[i];
                    if ((item?.GetHeldEntity() as BaseProjectile) != null)
                    {
                        gun = item;
                        break;
                    }
                }

                if (gun == null)
                {
                    Interface.Oxide.LogWarning(nameof(EquipGun) + " could not find any gun!");
                    return;
                }

                var itemPos0 = Scientist.inventory.containerBelt.GetSlot(0);
                if (itemPos0 == gun) Interface.Oxide.LogWarning("Gun already equipped!");
                else
                {
                    itemPos0.RemoveFromContainer();
                    
                    gun.RemoveFromContainer();

                    if (!gun.MoveToContainer(Scientist.inventory.containerBelt))
                        Interface.Oxide.LogWarning("Couldn't move gun to belt!!");

                    if (!itemPos0.MoveToContainer(Scientist.inventory.containerBelt))
                    {
                        Interface.Oxide.LogWarning("Couldn't move original itempos0 back to belt!!");
                    }
                    else Interface.Oxide.LogWarning("Moved original itempos0 back to belt!");

                }

                Scientist.EquipWeapon();

            }

            private void ResumeRoadAI()
            {
                RoadAI.Unpause();
                Movement.Paused = true;
            }

            private void PauseRoadAI()
            {
                RoadAI.Paused = true;
                Movement.Paused = false;
            }

            private void OnReachedTargetBarrel()
            {
                Interface.Oxide.LogWarning(nameof(OnReachedTargetBarrel));

                if (TargetBarrel == null || TargetBarrel.IsDestroyed)
                {
                    Interface.Oxide.LogWarning(nameof(OnReachedTargetBarrel) + "() called but TargetBarrel is null or destroyed!");
                    return;
                }

                HasReachedBarrel = true;

                Interface.Oxide.LogWarning("Has reached barrel! Starting coroutine");

                if (_attackingCoroutine != null)
                {
                    Interface.Oxide.LogWarning("Attacking coroutine already exists, cancelling");
                    ServerMgr.Instance.StopCoroutine(_attackingCoroutine);
                }

                var itemInSlot0 = Scientist.inventory.containerBelt.GetSlot(0);

                if (itemInSlot0.info.shortname != "hatchet")
                {
                    Interface.Oxide.LogWarning("trying to equip hatchet manually");

                    itemInSlot0.position = Scientist.inventory.containerBelt.capacity - 1;
                    itemInSlot0.RemoveFromContainer();
              
                    
                    Item axe = null;

                    for (int i = 0; i < Scientist.inventory.containerBelt.itemList.Count; i++)
                    {
                        var beltItem = Scientist.inventory.containerBelt.itemList[i];
                        if (beltItem?.info?.shortname == "hatchet")
                        {
                            axe = beltItem;
                            Interface.Oxide.LogWarning("found preexisting hatchet item!");
                            break;
                        }
                    }

                    if (axe == null) axe = ItemManager.CreateByName("hatchet");
                    else axe.RemoveFromContainer();

                    if (!axe.MoveToContainer(Scientist.inventory.containerBelt))
                    {
                        Interface.Oxide.LogWarning("Couldn't move axe?!");
                    }
                    else Interface.Oxide.LogWarning("Moved axe!, pos: " + axe.position);


                    if (!itemInSlot0.MoveToContainer(Scientist.inventory.containerBelt))
                    {
                        Interface.Oxide.LogWarning("Couldn't move original item back?!");
                    }
                    else Interface.Oxide.LogWarning("moved original item back after moving axe");

                }

                Scientist.EquipWeapon();

                _attackingCoroutine = ServerMgr.Instance.StartCoroutine(MeleeAttackEntity(TargetBarrel));

                Interface.Oxide.LogWarning("Attacking coroutine was started!");

            }

            private void TryMoveToTargetBarrel()
            {
                if (TargetBarrel == null || TargetBarrel.IsDestroyed)
                {
                    Interface.Oxide.LogWarning("TargetBarrel is null or destroyed on TryMoveToNearestBarrel!!");
                    InvokeHandler.CancelInvoke(Scientist, TryMoveToTargetBarrel);
                    BeginJunkpileInvoke();
                    return;
                }


                var curPos = Scientist?.transform?.position ?? Vector3.zero;
                var barrelPos = TargetBarrel?.transform?.position ?? Vector3.zero;

                if (Vector3.Distance(curPos, barrelPos) <= 8f)
                {
                    Scientist.Brain.Navigator.SetFacingDirectionEntity(TargetBarrel);
                }

                var doSetDestination = true;

    

                if (Vector3.Distance(curPos, barrelPos) < 1.5f)
                {
                    Interface.Oxide.LogWarning("Barrel within < 1.5f!");
                    var scientistEyes = Scientist.eyes.position;

                    var currentRot = Quaternion.Euler(Scientist.viewAngles) * Vector3.forward;

                    RaycastHit hit;

                    if (Physics.Raycast(new Ray(scientistEyes, currentRot), out hit, 1.75f))
                    {
                        var hitEnt = hit.GetEntity();

                        if (hitEnt != TargetBarrel)
                        {
                            Interface.Oxide.LogWarning("Scientist is not looking at barrel!!, hitEnt is: " + hitEnt);
                        }
                        else
                        {
                            Interface.Oxide.LogWarning("Scientist is looking at barrel (ray hit has ent), moving to barrel");
                            
                            if (Scientist != null && !Scientist.IsDestroyed)
                            {
                                Interface.Oxide.LogWarning("Canceling invoke for try move to barrel because reached dest");
                                InvokeHandler.CancelInvoke(Scientist, TryMoveToTargetBarrel);
                                Interface.Oxide.LogWarning("Cancled invoke for try move to barrel!");
                            }
                            else Interface.Oxide.LogWarning("Scientist is null or destroyed on TryMoveToNearestBarrel()");

                            HasReachedBarrel = true;
                            OnReachedTargetBarrel();

                            TargetBarrel.transform.position = new Vector3(TargetBarrel.transform.position.x, TargetBarrel.transform.position.y + 0.175f, TargetBarrel.transform.position.z);
                            TargetBarrel.SendNetworkUpdate();

                            doSetDestination = false;
                            
                            Interface.Oxide.LogWarning("set doSetDestination to false!!");

                            Movement.Destination = hit.point;
                            Interface.Oxide.LogWarning("set destination to ray hit pos: " + hit.point + " !");


                        }
                    }
                    else Interface.Oxide.LogWarning("no hit!!");


                }

                if (doSetDestination) Movement.Destination = (TargetBarrel?.transform?.position ?? Vector3.zero);

            }

            private LootContainer FindNearestBarrel()
            {
                var barrels = Facepunch.Pool.GetList<LootContainer>();


                LootContainer barrel = null;

                try
                {
                    var curPos = Scientist?.transform?.position ?? Vector3.zero;

                    Vis.Entities(curPos, 8f, barrels);

                    if (barrels.Count > 0)
                    {
                        Interface.Oxide.LogWarning("'barrels' is count > 0: " + barrels.Count);

                        var nearestVisibleBarrel = barrels?.Where(p => p.ShortPrefabName.Contains("barrel"))?.OrderBy(p => Vector3.Distance(curPos, p.transform.position))?.FirstOrDefault();


                        if (nearestVisibleBarrel?.gameObject == null || nearestVisibleBarrel.IsDestroyed)
                        {
                            Interface.Oxide.LogWarning("Barrel destroyed/gameObject null!");
                            return null;
                        }

                        barrel = nearestVisibleBarrel;


                    }
                    else Interface.Oxide.LogWarning("no barrels found!");
                }
                finally { Facepunch.Pool.FreeList(ref barrels); }

                return barrel;
            }

            private void OnReachedTargetJunkpile()
            {
                Interface.Oxide.LogWarning(nameof(OnReachedTargetJunkpile));



                if (TargetJunkpile == null || TargetJunkpile.IsDestroyed)
                {
                    Interface.Oxide.LogWarning("TargetJunkpile is actually null/destroyed, somehow!!");
                    return;
                }

                CancelJunkpileInvoke();

                _lootedPiles.Add(TargetJunkpile.net.ID);

                HasReachedJunkpile = true;



                var curPos = Scientist?.transform?.position ?? Vector3.zero;

                var nearestVisibleBarrel = FindNearestBarrel();

                if (nearestVisibleBarrel?.gameObject == null || nearestVisibleBarrel.IsDestroyed)
                {
                    Interface.Oxide.LogWarning("Barrel null or destroyed/gameObject null!");
                    Interface.Oxide.LogWarning("no barrels found!");
                    if (RoadAI != null)
                    {
                        Interface.Oxide.LogWarning("Resuming RoadAI as no barrels found!");

                        ResumeRoadAI();
                        BeginJunkpileInvoke();
                        Interface.Oxide.LogWarning("Unpaused!");
                    }
                    return;
                }

                Interface.Oxide.LogWarning("TargetBarrel is now becoming nearestVisibleBarrel");

                TargetBarrel = nearestVisibleBarrel;

                InvokeHandler.InvokeRepeating(Scientist, TryMoveToTargetBarrel, 0.01f, 0.05f);


                Interface.Oxide.LogWarning("started invoke TryMoveToNearestBarrel");
            }

            private void TryMoveToNearestJunkpile()
            {

                if (TargetJunkpile != null && !TargetJunkpile.IsDestroyed)
                {
                    //   Interface.Oxide.LogWarning(nameof(TryMoveToNearestJunkpile) + " but junkpile is not null or destroyed!");
                    return;
                }

                //  Interface.Oxide.LogWarning(nameof(TryMoveToNearestJunkpile) + " no target junkpile yet!");


                var junkpiles = Facepunch.Pool.GetList<JunkPile>();
                try
                {
                    var currentPos = Scientist?.transform?.position ?? Vector3.zero;

                    Vis.Entities<JunkPile>(currentPos, 20f, junkpiles);

                    if (junkpiles.Count > 0)
                    {
                      //  Interface.Oxide.LogWarning("Junkpiles within 20 radius! Count: " + junkpiles.Count);
                        var nearestVisiblePile = junkpiles?.Where(p => !_lootedPiles.Contains(p.net.ID))?.OrderBy(p => Vector3.Distance(p.transform.position, currentPos))?.FirstOrDefault(); //disgusting. also temporary

                        if (nearestVisiblePile == null)
                        {
                         //   Interface.Oxide.LogWarning("no near visible pile");
                            return;
                        }

                        TargetJunkpile = nearestVisiblePile;

                        Interface.Oxide.LogWarning("Pausing RoadAI as we found a junkpile!");
                        PauseRoadAI();


                        Movement.Destination = nearestVisiblePile.transform.position;


                    }
                }
                finally { Facepunch.Pool.FreeList(ref junkpiles); }
            }

            private void DoDestroy()
            {
                try { CancelJunkpileInvoke(); }
                finally { Destroy(this); }
            }
        }

        private class RoadAI : MonoBehaviour
        {
            public ScientistNPC Scientist { get; private set; }

            private PathList _currentRoad = null;

            public PathList CurrentRoad
            {
                get
                {
                    return _currentRoad;
                }
                set
                {
                    _currentRoad = value;
                    OnRoadChanged();
                }
            }

            public PathInterpolator CurrentRoadPath
            {
                get
                {
                    return _currentRoad?.Path;
                }
            }

            public BaseAIBrain Brain
            {
                get;
                private set;
            }

            public bool PausedBySelf { get; private set; }

            public bool Paused { get; set; }

            //this should be used to determine how far left, right, back, or forward the scientist is from the main point on the road
            public Vector3 DestinationOffset { get; set; } = Vector3.zero;

            public BaseNavigator.NavigationSpeed NavSpeed { get; set; } = BaseNavigator.NavigationSpeed.Normal;

            private float _lastSetDestTime = -1f;

            private Vector3 _currentPointOnRoad = Vector3.zero;
            private Vector3 _nextPointOnRoad = Vector3.zero;
            private int _currentPathIndex = 0;

            private void Awake()
            {
                Scientist = GetComponent<ScientistNPC>();
                if (Scientist == null)
                {
                    Interface.Oxide.LogError(nameof(RoadAI) + "." + nameof(Awake) + "() called on a non-ScientistNPC type!");
                    DoDestroy();
                    return;
                }

                Brain = Scientist.GetComponent<BaseAIBrain>();

                Interface.Oxide.LogWarning("Fixing brain");

                Brain.Navigator.Agent.agentTypeID = -1372625422;

                Brain.Navigator.SetNavMeshEnabled(false);
                Brain.Navigator.DefaultArea = "Walkable";

                Brain.Navigator.Init(Scientist, Brain.Navigator.Agent);
                Brain.Navigator.SetNavMeshEnabled(true);

                Interface.Oxide.LogWarning("Fixed brain");


                CurrentRoad = FindNearestRoad();

                BeginMoveToRoadInvoke();

                //_currentPointOnRoad = GetFirstPointOnRoad();
                //  Interface.Oxide.LogWarning("Heading toward this position to start (should be start of nearest road): " + _currentPointOnRoad);

                //_nextPointOnRoad = GetNextPointOnRoad();
                //  Interface.Oxide.LogWarning("nextpoint: " + _nextPointOnRoad);
            }

            public PathList FindNearestRoad()
            {

                PathList nearest = null;

                var currentPos = Scientist?.transform?.position ?? Vector3.zero;

                var lastDist = -1f;
                var nearestDist = -1f;

                for (int i = 0; i < TerrainMeta.Path.Roads.Count; i++)
                {
                    var road = TerrainMeta.Path.Roads[i];

                    var dist = Vector3.Distance(road.Path.GetStartPoint(), currentPos);

                    if (nearestDist <= -1f)
                    {
                        nearestDist = dist;
                        nearest = road;
                    }

                    if (dist < nearestDist)
                    {
                        Interface.Oxide.LogWarning("lastDist: " + lastDist + ", this is either -1 or dist (" + dist + ") < lastDist: " + lastDist + ", had nearest already?: " + (nearest != null));

                    }

                    lastDist = dist;

                }

                Interface.Oxide.LogWarning("nearest road has start dist of: " + nearestDist);

                return nearest;
            }

            private void OnRoadChanged()
            {

                _currentPathIndex = 0;
                // _currentPointOnRoad = 
            }

            public Vector3 GetFirstPointOnRoad()
            {
                return _currentRoad.Path.GetStartPoint();
            }

            public Vector3 GetEndPointOnRoad()
            {
                return _currentRoad.Path.GetEndPoint();
            }

            public Vector3 GetNextPointOnRoad()
            {
                if (CurrentRoadPath == null)
                {
                    Interface.Oxide.LogWarning("GetNextPointOnRoad() called while roadpath is null!");
                    return Vector3.zero;
                }

                //if (_currentPathIndex == 0) 
                // return GetFirstPointOnRoad();

                if (_currentPathIndex >= CurrentRoadPath.Points.Length)
                    return GetEndPointOnRoad();

                return (_currentPathIndex + 1) <= (CurrentRoadPath.Points.Length - 1) ? CurrentRoadPath.Points[_currentPathIndex + 1] : GetEndPointOnRoad();
            }

            public Vector3 GetNextPointOnRoad(int skipAhead)
            {
                if (CurrentRoadPath == null)
                {
                    Interface.Oxide.LogWarning("GetNextPointOnRoad() called while roadpath is null!");
                    return Vector3.zero;
                }

                //if (_currentPathIndex == 0) 
                // return GetFirstPointOnRoad();

                if (_currentPathIndex >= CurrentRoadPath.Points.Length)
                    return GetEndPointOnRoad();

                return (_currentPathIndex + (1 + skipAhead)) <= (CurrentRoadPath.Points.Length - (1 + skipAhead)) ? CurrentRoadPath.Points[_currentPathIndex + (1 + skipAhead)] : GetEndPointOnRoad();
            }

            public Vector3 GetCurrentPointOnRoad()
            {
                return _currentPointOnRoad;
            }

            public Vector3 GetNearestPointOnRoad()
            {
                if (CurrentRoadPath == null)
                {
                    Interface.Oxide.LogWarning("GetNearestPointOnRoad() called while roadpath is null!");
                    return Vector3.zero;
                }

                var currentPos = Scientist?.transform?.position ?? Vector3.zero;

                var nearestDist = -1f;
                var nearestPoint = Vector3.zero;

                for (int i = 0; i < CurrentRoadPath.Points.Length; i++)
                {
                    var point = CurrentRoadPath.Points[i];

                    var dist = Vector3.Distance(point, currentPos);

                    if (nearestDist <= -1f || dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestPoint = point;
                    }
                }

                return nearestPoint;
            }

            public int GetIndexByPoint(Vector3 point)
            {
                if (CurrentRoadPath == null)
                {
                    Interface.Oxide.LogWarning("GetIndexByPoint() called while roadpath is null!");
                    return -1;
                }

                for (int i = 0; i < CurrentRoadPath.Points.Length; i++)
                {
                    var thisPoint = CurrentRoadPath.Points[i];

                    if (thisPoint == point)
                        return i;
                }

                return -1;
            }

            private float SecondsSinceLastDestinationSet()
            {
                if (_lastSetDestTime == -1)
                    return -1;

                return UnityEngine.Time.realtimeSinceStartup - _lastSetDestTime;
            }

            public void BeginMoveToRoadInvoke()
            {
                if (!InvokeHandler.IsInvoking(Scientist, TryMoveToRoad))
                    InvokeHandler.InvokeRepeating(Scientist, TryMoveToRoad, 1f, 0.05f);
            }

            public void CancelMoveToRoadInvoke() { InvokeHandler.CancelInvoke(Scientist, TryMoveToRoad); }

            public void DoDestroy()
            {
                try { CancelMoveToRoadInvoke(); }
                finally { Destroy(this); }
            }

            private void EnsureFeetAreGrounded()
            {
                int layerMask = 10551296;
                RaycastHit hitInfo;

                if (!UnityEngine.Physics.Raycast(Scientist.transform.position + Vector3.up * 0.5f, Vector3.down, out hitInfo, 1000f, layerMask))
                {
                    Interface.Oxide.LogWarning("no ground from ray hit!");
                    return;
                }

                var adjustedPoint = hitInfo.point;
                adjustedPoint.y -= 0.05f;

                Scientist.ServerPosition = adjustedPoint;
            }

            public void Unpause()
            {
                Paused = false;

                Interface.Oxide.LogWarning("Resuming from paused start, current point: " + _currentPointOnRoad + ", index: " + _currentPathIndex);

                _nextPointOnRoad = GetNearestPointOnRoad();
                _currentPathIndex = GetIndexByPoint(_nextPointOnRoad);

                Brain.Navigator.ClearFacingDirectionOverride();

                Interface.Oxide.LogWarning("Resumed from paused start, got nearest point: " + _nextPointOnRoad + ", adjusted index to: " + _currentPathIndex);
            }

            private void TryMoveToRoad()
            {
                if (CurrentRoad == null) return;

                if (Scientist == null || Scientist?.gameObject == null || Scientist.IsDestroyed || Scientist.IsDead())
                {
                    Interface.Oxide.LogWarning("Scientist has died! Calling DoDestroy()");
                    DoDestroy();
                    return;
                }

                if (Paused)
                    return;

                if ((Scientist.lastAttackedTime >= 0f && Scientist.SecondsSinceAttacked <= 30f))
                {
                    PausedBySelf = true;
                    //Interface.Oxide.LogWarning("attacked in last 30 seconds!: " + Scientist.SecondsSinceAttacked + ", last attacked time: " + Scientist.lastAttackedTime);
                    return; //attacked in last 30 seconds
                }

                var nearbyScientists = Facepunch.Pool.GetList<ScientistNPC>();
                try 
                {
                    Vis.Entities(Scientist.transform.position, 30f, nearbyScientists, 131072);

                    for (int i = 0; i < nearbyScientists.Count; i++)
                    {
                        var sci = nearbyScientists[i];

                        if ((sci.lastAttackedTime >= 0f && sci.SecondsSinceAttacked <= 30f))
                        {
                            PausedBySelf = true;
                            //Interface.Oxide.LogWarning("nearby scientist attacked in last 30 seconds!: " + sci.SecondsSinceAttacked + ", last attacked time: " + sci.lastAttackedTime);
                            return; //attacked in last 30 seconds
                        }

                    }

                }
                finally { Facepunch.Pool.FreeList(ref nearbyScientists); }

              

                //Interface.Oxide.LogWarning("Update (after a few conditional checks) on convoyai");

                //  EnsureFeetAreGrounded();

                if (PausedBySelf)
                {
                    PausedBySelf = false;

                    Unpause();

                }
                else
                {
                    _nextPointOnRoad = GetNextPointOnRoad(); //skip ahead by 3?
                }


                if (DestinationOffset != Vector3.zero) _nextPointOnRoad += DestinationOffset;

                if (_currentPointOnRoad == Vector3.zero)
                {
                    _currentPointOnRoad = GetFirstPointOnRoad();

                    if (!Brain.Navigator.SetDestination(_nextPointOnRoad, BaseNavigator.NavigationSpeed.Fast))
                        Interface.Oxide.LogWarning("SetDestination returned false!!");
                    else Interface.Oxide.LogWarning("set initial next point to: " + _nextPointOnRoad);
                }
                else
                {

                    if (!Brain.Navigator.SetDestination(_nextPointOnRoad, NavSpeed)) //not setting this as often as we can seems to make them fall out of sync no matter what
                        Interface.Oxide.LogWarning("SetDestination returned false!! (nextpoint1): " + _nextPointOnRoad);


                    /*/
                    var secs = SecondsSinceLastDestinationSet()
                        
                    if (Vector3.Distance(Brain.Navigator.Destination, _nextPointOnRoad) > 2 ||)
                    {
                        Interface.Oxide.LogWarning("Current navigator destination has dist between _nextPoint as > 2, setting pos");

                    }/*/


                    /*/
                    var secs = SecondsSinceLastDestinationSet();
                    if (secs == -1f || secs >= 3)
                    {
                       
                        else
                        {
                            Interface.Oxide.LogWarning("Set dest returned true, setting time");
                            _lastSetDestTime = UnityEngine.Time.realtimeSinceStartup;

                        }
                    }/*/
                }

                var currentPos = Scientist?.transform?.position ?? Vector3.zero;

                var nextPointYNormalized = _nextPointOnRoad;
                nextPointYNormalized.y = currentPos.y;

                var distCheck = 1f;

                NavSpeed = Vector3.Distance(currentPos, nextPointYNormalized) >= 25 ? BaseNavigator.NavigationSpeed.Fast : BaseNavigator.NavigationSpeed.Normal;

                if (Vector3.Distance(currentPos, nextPointYNormalized) <= distCheck) //lower numbers better?
                {
                    Interface.Oxide.LogWarning("dist <= " + distCheck + "! changing");

                    _nextPointOnRoad = GetNextPointOnRoad(2);

                    _currentPathIndex = GetIndexByPoint(_nextPointOnRoad);
                    _currentPointOnRoad = nextPointYNormalized;


                    if (DestinationOffset != Vector3.zero) _nextPointOnRoad += DestinationOffset;

                    if (!Brain.Navigator.SetDestination(_nextPointOnRoad, NavSpeed))
                        Interface.Oxide.LogWarning("SetDestination returned false!! (nextpoint): " + _nextPointOnRoad);
                }



            }
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var scarecrow = entity as ScarecrowNPC;
            if (scarecrow != null)
            {

                scarecrow.Invoke(() =>
                {
                    PrintWarning("scarecrow invoke");

                    if (scarecrow?.Brain == null)
                    {
                        PrintWarning("braein null!");
                        return;
                    }

                    if (scarecrow?.Brain?.Navigator == null)
                    {
                        PrintWarning("navigator is null lol");
                        return;
                    }

                    PrintWarning("agent typ: " + scarecrow?.Brain?.Navigator?.Agent?.agentTypeID);

                    PrintWarning("defaultarea: " + (scarecrow?.Brain?.Navigator?.DefaultArea ?? "null string lol"));

                    PrintWarning("canusenav: " + scarecrow?.Brain?.Navigator?.CanUseNavMesh);

                }, 5f);

                PrintWarning("area: " + scarecrow?.Brain?.Navigator?.DefaultArea + ", can use nav?: " + scarecrow?.Brain?.Navigator?.CanUseNavMesh);
            }
        }

        /*/
         *  
        /*/

        [ChatCommand("roadtest")]
        private void cmdRoadPathingTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var sb = new StringBuilder();

            for (int i = 0; i < TerrainMeta.Path.Roads.Count; i++)
            {
                var road = TerrainMeta.Path.Roads[i];
                sb.Append("start point: ").Append(road?.Path?.GetStartPoint()).Append(", end: ").Append(road?.Path?.GetEndPoint()).Append("\n");

                var pointsLength = road.Path.Points.Length;
                for (int j = 0; j < pointsLength; j++)
                {

                    if ((j + 1) <= (pointsLength - 1))
                    {
                        var point = road.Path.Points[j];
                        var nextPoint = road.Path.Points[j + 1];

                        var adjPoint = point;
                        adjPoint.y += 2;

                        var adjNextPoint = nextPoint;
                        adjNextPoint.y += 2;

                        player.SendConsoleCommand("ddraw.arrow", 30f, Color.yellow, adjPoint, adjNextPoint, 0.35f);
                    }

                }

            }

            if (sb.Length > 1) sb.Length -= 1;

            PrintWarning(sb.ToString());
        }

        private List<ItemAmount> GetArmorForConvoyNPC()
        {
            var items = new List<ItemAmount>();

            items.Add(new ItemAmount(ItemManager.FindItemDefinition("metal.facemask"), 1));
            items.Add(new ItemAmount(ItemManager.FindItemDefinition("hoodie"), 1));
            items.Add(new ItemAmount(ItemManager.FindItemDefinition("pants"), 1));
            items.Add(new ItemAmount(ItemManager.FindItemDefinition("shoes.boots"), 1));
            items.Add(new ItemAmount(ItemManager.FindItemDefinition("metal.plate.torso"), 1));
            items.Add(new ItemAmount(ItemManager.FindItemDefinition("roadsign.kilt"), 1));


            return items;
        }

        [ChatCommand("convoytest")]
        private void cmdConvoyTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            int count;
            if (args.Length < 1 || !int.TryParse(args[0], out count)) count = 1;

            var armorAmounts = GetArmorForConvoyNPC();


            for (int i = 0; i < count; i++)
            {
                var spawnPoint = TerrainMeta.Path.Roads[0].Path.GetStartPoint();

                var offset = i == 0 ? Vector3.zero : i == 1 ? (Vector3.left * 3f) : i == 2 ? (Vector3.right * 3f) : i == 3 ? (Vector3.left * 3f + Vector3.forward * 2.75f) : Vector3.zero;

                if (offset != Vector3.zero)
                    spawnPoint += offset;



                var npc = GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roam.prefab", spawnPoint) as ScientistNPC;
                npc.Spawn();

                PrintWarning("spawned npc at: " + spawnPoint + ", adding its road AI in 1 second");

                npc.Invoke(() =>
                {
                    var ai = npc.gameObject.AddComponent<RoadAI>();

                    ai.DestinationOffset = offset;
                    //  ai.nearest
                }, 1f);

                for (int j = 0; j < npc.inventory.containerWear.itemList.Count; j++)
                {
                    var wearItem = npc.inventory.containerWear.itemList[j];
                    RemoveFromWorld(wearItem);
                }

                for (int j = 0; j < armorAmounts.Count; j++)
                {
                    var armorAmt = armorAmounts[j];

                    var armorItem = ItemManager.Create(armorAmt.itemDef, (int)armorAmt.amount);

                    if (!armorItem.MoveToContainer(npc.inventory.containerWear)) RemoveFromWorld(armorItem);
                }

            }

            SendReply(player, "Spawned: " + count + " in convoy!");

            SimpleBroadcast("<color=#e86209>A convoy of soldiers has been spotted roaming the roads</color>! <color=#f74d22>They were last seen near the abandoned military base, but are rapidly mobilizing</color>.");

        }

        [ChatCommand("roadlifetest")]
        private void cmdRoadPathingTestAI(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            int count;
            if (args.Length < 1 || !int.TryParse(args[0], out count)) count = 1;



            for (int i = 0; i < count; i++)
            {
                var spawnPoint = TerrainMeta.Path.Roads[0].Path.GetStartPoint();

                var offset = i == 0 ? Vector3.zero : i == 1 ? (Vector3.left * 3f) : i == 2 ? (Vector3.right * 3f) : i == 3 ? (Vector3.left * 3f + Vector3.forward * 2.75f) : Vector3.zero;

                if (offset != Vector3.zero)
                    spawnPoint += offset;



                var npc = GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roam.prefab", spawnPoint) as ScientistNPC;
                npc.Spawn();

                PrintWarning("spawned npc at: " + spawnPoint + ", adding its road AI in 1 second");

                npc.Invoke(() =>
                {
                    var ai = npc.gameObject.AddComponent<RoadAI>();

                    ai.DestinationOffset = offset;
                    //  ai.nearest
                }, 1f);
            }
        }

        [ChatCommand("merctest")]
        private void cmdMercTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            int count;
            if (args.Length < 1 || !int.TryParse(args[0], out count)) count = 1;



            for (int i = 0; i < count; i++)
            {
                var spawnPoint = TerrainMeta.Path.Roads[0].Path.GetStartPoint();

                var offset = i == 0 ? Vector3.zero : i == 1 ? (Vector3.left * 3f) : i == 2 ? (Vector3.right * 3f) : i == 3 ? (Vector3.left * 3f + Vector3.forward * 2.75f) : Vector3.zero;

                if (offset != Vector3.zero)
                    spawnPoint += offset;



                var npc = GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roam.prefab", spawnPoint) as ScientistNPC;
                npc.Spawn();

                PrintWarning("spawned npc at: " + spawnPoint + ", adding its road AI in 1 second");

                npc.Invoke(() =>
                {
                    var roadAi = npc.gameObject.AddComponent<RoadAI>();

                    roadAi.DestinationOffset = offset;

                    npc.gameObject.AddComponent<MercenaryAI>();
                    //  ai.nearest
                }, 1f);
            }




        }

        [ChatCommand("lifetest")]
        private void cmdLifeTest(BasePlayer player, string command, string[] args)
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

            if (!Physics.Raycast(ray, out hitt, 10f, layer))
            {
                SendReply(player, "No hit");
                return;
            }




            var npc = GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roam.prefab", hitt.point) as ScientistNPC;
            npc.Spawn();


            var brain = npc.GetComponent<BaseAIBrain>();

            if (brain != null)
            {
                PrintWarning("scientist brain is not null!");

                brain.Invoke(() =>
                {


                    PrintWarning("agent type id after init: " + brain.Navigator.Agent.agentTypeID);

                    //try use scarecrow id -1372625422

                    brain.Navigator.Agent.agentTypeID = -1372625422;

                    brain.Navigator.SetNavMeshEnabled(false);
                    brain.Navigator.DefaultArea = "Walkable";

                    brain.Navigator.Init(npc, brain.Navigator.Agent);
                    brain.Navigator.SetNavMeshEnabled(true);



                    brain.Navigator.SetDestination(player?.transform?.position ?? Vector3.zero, BaseNavigator.NavigationSpeed.Fast);

                    /*/
                    brain.Navigator.SetNavMeshEnabled(false);
                   
                    brain.Navigator.SetNavMeshEnabled(true);

                    SetValue(brain.Navigator, "defaultAreaMask", UnityEngine.AI.NavMesh.GetAreaFromName(brain.Navigator.DefaultArea));
                    
                    PrintWarning("Changed nav to walkable");/*/
                    // PrintWarning("trying to disable nav mesh");
                    //brain.Navigator.SetNavMeshEnabled(false);
                    //  brain.Navigator.
                    // PrintWarning("called set nav false");
                }, 3f);





                brain.InvokeRepeating(() =>
                {
                    PrintWarning("current brain state: " + brain.CurrentState);

                    brain.Navigator.ForceToGround();



                    brain.Navigator.SetDestination(player?.transform?.position ?? Vector3.zero, BaseNavigator.NavigationSpeed.Fast);


                }, 5f, 3f);

                /*/
                brain.Invoke(() =>
                {
                    PrintWarning("try switch to chase");

                    brain.SwitchToState(AIState.Chase);

                    PrintWarning("called switchtostate, canleave btw: " + brain.CurrentState.CanLeave());
                }, 15f);/*/
            }

            /*/
                  var npc = GameManager.server.CreateEntity("assets/prefabs/npc/scarecrow/scarecrow.prefab", hitt.point) as ScarecrowNPC;
            npc.Spawn();

            RemoveFromWorld(npc.inventory.containerBelt.itemList[0]);

            var newGun = ItemManager.CreateByName("pistol.m92");
            if (!newGun.MoveToContainer(npc.inventory.containerBelt))
            {
                PrintWarning("Could not move gun to npc container belt!!!");
                RemoveFromWorld(newGun);
            }

            var ammo = ItemManager.CreateByName("ammo.pistol");
            if (!ammo.MoveToContainer(npc.inventory.containerMain))
            {
                PrintWarning("Could not move ammo for gun!!");
                RemoveFromWorld(ammo);
            }

            npc.EquipWeapon();
            npc.AttackDamageType = Rust.DamageType.Bullet;
            
            if (npc?.Brain != null)
            {
                PrintWarning("brain is NOT null");

                if (npc?.Brain?.states != null)
                {
                    PrintWarning("states not null, count: " + npc.Brain.states.Count);

                    if (npc.Brain.states.Remove(AIState.Chase)) PrintWarning("Removed chase state!");
                    else PrintWarning("Could not remove chase state!");
                }
                else PrintWarning("states are null!");

                npc.Invoke(() =>
                {
                    PrintWarning("states still null?: " + (npc?.Brain?.states == null));

                    if (npc?.Brain?.states != null)
                    {
                        PrintWarning("not null anymore, trying to remove chase");

                        PrintWarning("current state: " + npc.Brain.CurrentState);

                        PrintWarning("trying to leave current state");

                        npc.Brain.CurrentState.StateLeave();

                        PrintWarning("called stateleave, going to try to enter stateattack instead of chase");
                        npc.Brain.CurrentState = npc.Brain.states[AIState.Attack];
                        npc.Brain.CurrentState.StateEnter();

                        npc.Brain.Navigator.

                      //  npc.Brain.SwitchToState(AIState.Idle);

                        if (npc.Brain.states.Remove(AIState.Chase)) PrintWarning("Removed chase state!");
                        else PrintWarning("Could not remove chase state!");

                        if (npc.Brain.states.Remove(AIState.Attack)) PrintWarning("removed attack satte!");
                        else PrintWarning("Could not remove attack state!");
                    }

                }, 5f);

            }


            
            npc.InvokeRepeating(() =>
            {

                PrintWarning("Current state: " + npc.Brain.CurrentState);

                var players = Facepunch.Pool.GetList<BasePlayer>();
                var nearest = -1f;

                try 
                {
                    Vis.Entities<BasePlayer>(npc.transform.position, UnityEngine.Random.Range(12f, 24f), players, 131072);

                    for (int i = 0; i < players.Count; i++)
                    {
                        var ply = players[i];

                        if (ply == null || !ply.IsConnected) continue;

                        var dist = Vector3.Distance(ply.transform.position, npc.transform.position);
                        if (nearest == -1f || dist < nearest) nearest = dist;
                    }

                }
                finally { Facepunch.Pool.FreeList(ref players); }
                
                if (nearest >= 0f) npc.ShotTest(nearest);
            }, 4f, 1.25f);
            /*/
            /*/

            var npc = GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roam.prefab", hitt.point) as ScientistNPC;
            npc.Spawn();

            
            var brain = npc.GetComponent<BaseAIBrain<HumanNPC>>();

            if (brain == null)
            {
                PrintWarning("brain is null?!");
                //brain = npc.gameObject.AddComponent<BaseAIBrain<ScientistNPC>>();
            }
            else
            {
                PrintWarning("Try set pathfinder");
                brain.PathFinder = new HumanPathFinder();
                PrintWarning("set pathfinder, try init");
                (brain.PathFinder as HumanPathFinder).Init(npc);

             //   AIThinkManager.Add((IThinker)npc);

                npc.InvokeRepeating(() =>
                {
                    npc.ServerThink(0f);
                    brain.DoThink();
                }, 0.25f, 0.25f);

             

                PrintWarning("init pathfinder");
            }
            /*/


            /*/
            PrintWarning("Spawned NPC: " + ", legacy: " + npc.LegacyNavigation);

            npc.LegacyNavigation = true;
            //npc.SetDestination(player.transform.position);


            
            //   var fracJourney = (UnityEngine.Time.time - startTime) * speed / journeyLength;
               // entity.transform.position = Vector3.Lerp(startPos, endPos, fracJourney);


            var startTime = UnityEngine.Time.time;
            var startPos = npc.transform.position;
            var endPos = player.transform.position;
            var journeyLength = Vector3.Distance(startPos, endPos);
            var speed = args.Length > 1 ? float.Parse(args[1]) : 0.5f;

            var originalEndPos = endPos;

            npc.InvokeRepeating(() =>
            {
                SendReply(player, "old pos: " + npc.ServerPosition);

            //    startPos = npc?.transform?.position ?? Vector3.zero;
                endPos = player?.transform?.position ?? Vector3.zero;
                if (Vector3.Distance(endPos, originalEndPos) >= 50)
                {
                    originalEndPos = endPos;
                    startTime = UnityEngine.Time.time;
                }

                journeyLength = Vector3.Distance(startPos, endPos);

              //  speed = player?.estimatedSpeed ?? 0f;
              //  if (speed < 2) speed = 2;
                
                var fracJourney = (UnityEngine.Time.time - startTime) * speed / journeyLength;
                npc.ServerPosition = Vector3.Lerp(startPos, endPos, fracJourney);
            SendReply(player, "new pos: " + npc.ServerPosition + Environment.NewLine + "lerp: " + Vector3.Lerp(startPos, endPos, fracJourney) + ", your speed: " + speed);
            }, 0.1f, 0.05f);
         //   npc.ServerPosition = player.transform.position;

          //  npc.
          /*/
            SendReply(player, "Tried to mount npc");

        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private void SetValue(object inputObject, string propertyName, object propertyVal)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) return;
            try
            {
                //find out the type
                var type = inputObject.GetType();
                if (type == null)
                {
                    PrintWarning("inputObject has type of null?!");
                    return;
                }

                //get the property information based on the type
                var propertyInfo = type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    PrintWarning("propertyInfo is null on GetField for type: " + type + ", propertyName: " + propertyName);
                    return;
                }

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
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintWarning("SetValue had an exception, arguments: " + inputObject + " : " + propertyName + " : " + propertyVal);
            }
        }

        private object GetValue(object inputObject, string propertyName)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) return null;
            try
            {
                //find out the type
                var type = inputObject.GetType();
                if (type == null) return null;

                //get the property information based on the type
                return type?.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(inputObject) ?? null;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return null;
        }

        private bool IsNullableType(Type type) { return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)); }

        private void SimpleBroadcast(string msg, ulong userID = 0)
        {
            if (string.IsNullOrEmpty(msg)) return;
            Chat.ChatEntry chatEntry = new Chat.ChatEntry()
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
    }
}
