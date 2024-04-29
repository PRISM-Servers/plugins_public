using Oxide.Core.Libraries.Covalence;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using Pool = Facepunch.Pool;
using System.Reflection;
using Oxide.Core;
using System;
using Rust;
using ProtoBuf;

namespace Oxide.Plugins
{
    [Info("Dungeons", "Shady", "0.0.1")]
    internal class Dungeons : RustPlugin
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

        //potential ideas:
        //enemies are (occasionally?) named after people on your friends list
        //mixture of melee & ranged mobs



        private const string MINE_DUNGEON_PREFAB = "assets/prefabs/missions/portal/minedungeon.prefab";

        private const string MINE_DUNGEON_ENTRY_PORTAL_PREFAB = "assets/prefabs/missions/portal/halloweenportalentry.prefab";

        private const string PORTAL_APPEAR_FX = "assets/prefabs/missions/portal/proceduraldungeon/effects/appear.prefab";

        private const string PORTAL_DISAPPEAR_FX = "assets/prefabs/missions/portal/proceduraldungeon/effects/disappear.prefab";

        private const string CHAT_STEAM_ID = "76561198865536053";

        private BasePortal _hackPortal = null;

        public static Dungeons Instance;

        private readonly HashSet<ProceduralDynamicDungeon> _dungeons = new HashSet<ProceduralDynamicDungeon>();

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private readonly FieldInfo _curList = typeof(InvokeHandler).GetField("curList", BindingFlags.Instance | BindingFlags.NonPublic);

        //todo: detect when dungeon is 'completed' by all enemies being killed; give warning to players that they'll be transported to a new dungeon automatically within a few seconds (give them time to loot boxes/corpses/whatever)
        //this repeats until death or the player chooses to leave
        //difficulty should continually go up with each "round".
        //alternative idea to first one (todo:) : when dungeon is "complete", allow them to use the exit door to go to next dungeon; i prefer this idea.
        private class DungeonHandler : FacepunchBehaviour
        {
            public ProceduralDynamicDungeon Dungeon { get; private set; } = null;

            public HashSet<BasePlayer> Players { get; set; } = new();

            public int DifficultyStep { get; set; } = 0;

            public bool Cleared { get; private set; } = false;

            private Bounds? _bounds = null;
            public Bounds Bounds
            {
                get
                {
                    if (_bounds == null)
                        _bounds = new Bounds(Dungeon.transform.position, new Vector3(Dungeon.gridResolution * Dungeon.gridSpacing, 20f, Dungeon.gridResolution * Dungeon.gridSpacing));


                    return _bounds.Value;
                }
            }

            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(DungeonHandler) + "." + nameof(Awake));

                Dungeon = GetComponent<ProceduralDynamicDungeon>();

                if (Dungeon == null)
                {
                    Interface.Oxide.LogWarning("Awake called with null dungeon!!!!");
                    DoDestroy();
                    return;
                }

                InitializeMoreEnemies();

                InvokeRepeating(CheckIfDungeonCleared, 2f, 1f);

            }

            private void InitializeMoreEnemies()
            {
                //spawn more fellas
            }

            private void OnDungeonCleared()
            {
                Interface.Oxide.LogWarning(nameof(OnDungeonCleared));

                if (Cleared)
                {
                    Interface.Oxide.LogWarning(nameof(OnDungeonCleared) + " called when dungeon is already Cleared?!");
                    return;
                }

                if (Interface.Oxide.CallHook("OnDungeonClear", Dungeon, this) != null)
                    return;

                Cleared = true;

                Interface.Oxide.LogWarning("swapping in 5!");

                BroadcastToDungeon("New Dungeon in 5 secs!");

                Invoke(SwapAllToNextDungeon, 5f);
                Invoke(new Action(() =>
                {
                    BroadcastToast("New dungeon!", 1);
                    BroadcastMessage("New dungeon!");
                }), 5.1f);                

            }

            public void BroadcastToDungeon(string msg, string userId = "")
            {
                //implement userid later
                foreach (var p in Players)
                    p?.ChatMessage(msg);
            }

            public void BroadcastToast(string msg, int type = 0)
            {
                foreach (var p in Players)
                    Instance?.ShowToast(p, msg, type);
            }

            private void CheckIfDungeonCleared()
            {

                if (!AnyNPCsAlive())
                {
                    if (!Cleared)
                        OnDungeonCleared();
                }
                else Interface.Oxide.LogWarning("NPCs still alive");

                //if (isCleared): callhook "ondungeoncleared" or whatever.
            }

            public void GetAllEnemiesNoAlloc(ref List<ScarecrowNPC> list)
            {
                if (list == null)
                    throw new ArgumentNullException(nameof(list));

                list.Clear();

                var enemies = Pool.GetList<ScarecrowNPC>();

                Vis.Entities(Dungeon.transform.position, 125f, enemies, 131072);

                Interface.Oxide.LogWarning("enemies count lol: " + enemies.Count);

                try
                {
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        var enemy = enemies[i];

                        if (enemy == null || enemy.IsDestroyed || enemy.IsDead())
                            continue;

                        if (Bounds.Contains(enemy.transform.position))
                            list.Add(enemy);
                    }

                }
                finally { Pool.FreeList(ref enemies); }

            }

            public bool AnyNPCsAlive()
            {
                var enemies = Pool.GetList<ScarecrowNPC>();

                try 
                {
                    GetAllEnemiesNoAlloc(ref enemies);
                    return enemies.Count > 0;
                }
                finally { Pool.FreeList(ref enemies); }
            }

            public bool AnyPlayersAlive()
            {
                return Dungeon.ContainsAnyPlayers();
            }

            public void KickPlayers(bool useExitPortal = true)
            {
                var copyTo = Pool.Get<HashSet<BasePlayer>>();
                try 
                {
                    copyTo.Clear();

                    foreach (var p in Players)
                        copyTo.Add(p);

                    foreach (var p in copyTo)
                        KickPlayer(p, useExitPortal);

                }
                finally { Pool.Free(ref copyTo); }
            }

            public BasePortal GetEntryPortal()
            {
                var hackPortal = Instance?._hackPortal;

                hackPortal.targetPortal = Dungeon.GetExitPortal(true) as BasePortal;

                return hackPortal;
            }

            public BasePortal GetExitPortal()
            {
                return Dungeon?.GetExitPortal(true) as BasePortal;
            }

            public bool AddPlayer(BasePlayer player, bool useEntryPortal = true)
            {
                if (player == null)
                    throw new ArgumentNullException(nameof(player));


                if (!Players.Add(player))
                {
                    Interface.Oxide.LogWarning(nameof(AddPlayer) + " called with player already in hashset!: " + player);
                    return false;
                }

                if (useEntryPortal)
                    GetEntryPortal()?.UsePortal(player);

                return true;
            }

            public void KickPlayer(BasePlayer player, bool useExitPortal = true)
            {
                if (player == null)
                    throw new ArgumentNullException(nameof(player));


                if (!Players.Remove(player))
                {
                    Interface.Oxide.LogWarning(nameof(AddPlayer) + " called without player in hashset!: " + player);
                    return;
                }

                if (useExitPortal)
                    GetExitPortal()?.UsePortal(player);
            }

            public void DoDestroy()
            {
                try 
                {
                    Interface.Oxide.LogWarning(nameof(DoDestroy));

                    try { KickPlayers(); }
                    catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                    
                    if (Dungeon != null && !Dungeon.IsDestroyed)
                        Dungeon.Kill();



                }
                finally { GameObject.Destroy(this); }
              
            }

            public ProceduralDynamicDungeon GetNextDungeon()
            {
                //temp Instance reference...

                var desiredPos = Dungeon.transform.position;
                desiredPos.y += UnityEngine.Random.Range(-40f, 90f);

                return Instance?.SpawnDungeon(desiredPos, true);
            }

            public void SwapAllToNextDungeon()
            {
                var nextDung = GetNextDungeon();

                var comp = nextDung.GetComponent<DungeonHandler>();

                comp.DifficultyStep++;

                foreach (var p in Players)
                    comp.AddPlayer(p);

                KickPlayers(false);

                Invoke(() =>
                {
                    DoDestroy();
                }, 1f);

            }

        }

        public ProceduralDynamicDungeon SpawnDungeon(Vector3 pos, bool addComponent = true)
        {
            var dung = GameManager.server.CreateEntity(MINE_DUNGEON_PREFAB, pos) as ProceduralDynamicDungeon;

            _dungeons.Add(dung);

            dung.Spawn();

            if (addComponent)
                dung.gameObject.AddComponent<DungeonHandler>();

            dung.EnableSaving(false);

            return dung;
        }

        #region Commands
        private void cmdDungeon(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin || iPlayer?.Object == null)
                return;

            SendReply(iPlayer, nameof(cmdDungeon) + " " + command);

            var player = iPlayer?.Object as BasePlayer;

            if (player == null)
            {
                SendReply(iPlayer, "no baseplayer!");
                return;
            }

            var dung = SpawnDungeon(new Vector3(1500, 500, 1500));

            SendReply(player, "Spawned dungeon");

            var dungHandler = dung?.GetComponent<DungeonHandler>();

            SendReply(player, "dung handler?: " + (dungHandler != null));

            dungHandler?.AddPlayer(player);


        }
        #endregion

        #region Hooks

        private void Init()
        {
            Instance = this;

            AddCovalenceCommand("dungeon", nameof(cmdDungeon));
        }

        private void OnServerInitialized()
        {
           _hackPortal = GameManager.server.CreateEntity(MINE_DUNGEON_ENTRY_PORTAL_PREFAB, new Vector3(2000, -200, 2000)) as BasePortal;

        }

        private void Unload()
        {
            try 
            {

                if (!(_hackPortal?.IsDestroyed ?? true))
                    _hackPortal.Kill();

            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            try 
            {
                foreach (var dungeon in _dungeons)
                {
                    try
                    {
                        if (dungeon == null || dungeon.IsDestroyed)
                            continue;

                        dungeon?.GetComponent<DungeonHandler>()?.DoDestroy();

                        dungeon?.Invoke(() =>
                        {
                            if (dungeon != null && !dungeon.IsDestroyed)
                                dungeon?.Kill();
                        }, 0.15f);
                    }
                    catch(Exception ex) { PrintError(ex.ToString()); }
                  

                }

            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            Instance = null;
        }



        #endregion
        #region Util

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

        private readonly Dictionary<string, Vector3> preTpPosition = new Dictionary<string, Vector3>();
        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true, bool respawnIfDead = false)
        {
            if (player == null || player?.transform == null) return false;

            preTpPosition[player.UserIDString] = player?.transform?.position ?? Vector3.zero;

            var playerPos = player?.transform?.position ?? Vector3.zero;
            var isConnected = player?.IsConnected ?? false;

            if (respawnIfDead && player.IsDead())
            {
                player.RespawnAt(dest, Quaternion.identity);
                return true;
            }

            player.EndLooting(); //redundant?
            player.EnsureDismounted();
            player.UpdateActiveItem(new ItemId(0));
            player.Server_CancelGesture();

            if (player.HasParent()) player.SetParent(null, true); //if check redundant?

            var distFrom = Vector3.Distance(playerPos, dest);

            if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
            if (doSleep && isConnected) player.StartSleeping();

            player.RemoveFromTriggers();
            player.MovePosition(dest);

            if (isConnected)
            {
                if (doSleep)
                {
                    player.inventory.crafting.CancelAll(true);
                    CancelInvoke("InventoryUpdate", player); //this is safe to call without checking IsInvoking
                }

                player.ClientRPCPlayer(null, player, "ForcePositionTo", dest);

                if (distFrom >= 250 && distChecks) player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);

                player.UpdateNetworkGroup();
                player.SendEntityUpdate();
                player.SendNetworkUpdateImmediate();

                if (distFrom > 100)
                {
                    player?.ClearEntityQueue(null);
                    player.SendFullSnapshot();
                }
            }

            player.ForceUpdateTriggers();

            return true;
        }
        private ListDictionary<InvokeAction, float> _invokeList = null;

        private ListDictionary<InvokeAction, float> InvokeList
        {
            get
            {
                if (_invokeList == null && InvokeHandler.Instance != null) _invokeList = _curList.GetValue(InvokeHandler.Instance) as ListDictionary<InvokeAction, float>;
                return _invokeList;
            }
        }

        private void CancelInvoke(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null).ToString() == targetName && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private void CancelInvoke(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName) || obj == null) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null) == obj && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
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