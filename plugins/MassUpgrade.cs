using Facepunch;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("MassUpgrade", "Shady", "1.0.6")]
    internal class MassUpgrade : RustPlugin
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
        //must be holding hammer to use /up? (suggestion by me, unsure if we want this for sure)

        private const string CHAT_STEAM_ID = "76561198865536053";

        private readonly HashSet<string> _forbiddenTags = new() { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };
        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private readonly MethodInfo _skinChanged = typeof(BuildingBlock).GetMethod("OnSkinChanged", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly Dictionary<string, float> _lastAdvertTime = new();

        [PluginReference]
        private readonly Plugin Friends;

        [PluginReference]
        private readonly Plugin NoEscape;

        [PluginReference]
        private readonly Plugin BuildingSkin;

        private readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        //  private readonly MethodInfo isUpgradeBlocked = typeof(BuildingBlock).GetMethod("IsUpgradeBlocked", BindingFlags.NonPublic | BindingFlags.Instance);

        /*/
                private Vector3 GetTopOfBlock(BuildingBlock block)
                {
                    var targets2 = UnityEngine.Physics.SphereCastAll(new Ray(block.transform.position + Vector3.up * 2f, Vector3.down), 0.775f, 20f, constructionColl);
                    for (int i = 0; i < targets2.Length; i++)
                    {
                        var target = targets2[i];
                        var targEnt = target.GetEntity();
                        if (targEnt == block)
                        {
                            return target.point;
                        }
                    }
                    return Vector3.zero;
                }/*/

        private void OnStructureUpgrade(BaseCombatEntity entity, BasePlayer player, BuildingGrade.Enum grade)
        {
            if (player == null) return;

            if (_lastAdvertTime.TryGetValue(player.UserIDString, out float lastTime) || (Time.realtimeSinceStartup - lastTime) < 240)
                return;

            _lastAdvertTime[player.UserIDString] = Time.realtimeSinceStartup;

            //double space since italics pushes it close to the 'to'
            var msg = "<color=#33ccff>Try <size=18><i><color=#ff66cc>/</color><color=#ff4dff>up</color></i></size>  to mass-upgrade your base!</color>";

            player.Invoke(() =>
            {
                ShowToast(player, msg); //helps it stay up longer, weird workaroud i thought of to not being able to specify duration
            }, 0.8f);

            ShowToast(player, msg);
            SendReply(player, msg);

        }

        private readonly Dictionary<string, Coroutine> _currentUpgradeCoroutine = new();

        private System.Collections.IEnumerator StartMassUpgrade(HashSet<BuildingBlock> blocks, BuildingGrade.Enum grade, float timeBetweenBlocks = 0.25f, BasePlayer player = null)
        {
            if (blocks == null) throw new ArgumentNullException(nameof(blocks));
            if (blocks.Count < 1) throw new ArgumentOutOfRangeException(nameof(blocks));

            var userId = player?.UserIDString ?? string.Empty;

            var totalCount = blocks.Count;
            var index = 0;
            var lastPerc = -1f;
            foreach (var block in blocks)
            {
                index++;
                if (block == null || block.IsDestroyed || block.gameObject == null || block.IsDead()) continue;
                if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead() || player.inventory == null)
                {
                    PrintWarning("Player IsDestroyed, not connected, IsDead or inventory is null on StartMassUpgrade coroutine. Canceling!");
                    break;
                }

                if (block.grade > grade) //don't downgrade!
                    continue;

                var desiredSkin = BuildingSkin?.Call<ulong>("GetDesiredGradeSkin", userId, grade) ?? 0;

                if (block.grade == grade && (block.skinID == desiredSkin) && ((player?.LastBlockColourChangeId ?? 0) == block.playerCustomColourToApply))
                    continue;

                var blocked = player.IsBuildingBlocked(block.transform.position, block.transform.rotation, block.bounds);
                if (blocked)
                {
                    var buildingBlockMsg = "A block failed to be upgraded because you do not have permission to build where it's located.";

                    SendReply(player, buildingBlockMsg);
                    ShowToast(player, buildingBlockMsg);
                    continue;
                }

                var upgradeTestFailed = DeployVolume.Check(block.transform.position, block.transform.rotation, PrefabAttribute.server.FindAll<DeployVolume>(block.prefabID), ~(1 << block.gameObject.layer));
                if (upgradeTestFailed)
                {
                    var blockMsg = "A block failed to be upgraded because it's blocked by an object or player.";

                    SendReply(player, blockMsg);
                    ShowToast(player, blockMsg);
                    continue;
                }

                if (Interface.Oxide.CallHook("OnStructureUpgrade", block, player, grade) != null)
                    continue;

                var specifiedGradeCost = block.blockDefinition.GetGrade(grade, 0).CostToBuild(block.grade); //block?.blockDefinition?.grades[(int)grade]?.CostToBuild(block.grade) ?? null;
                if (specifiedGradeCost == null)
                {
                    PrintWarning("specifiedGradeCost is null?! grade: " + grade);
                    continue;
                }

                var canAfford = true;
                if (block.grade != grade) //don't charge for changing skin
                {
                    for (int i = 0; i < specifiedGradeCost.Count; i++)
                    {
                        var cost = specifiedGradeCost[i];
                        if ((player?.inventory?.GetAmount(cost.itemid) ?? 0) < (int)cost.amount)
                        {
                            canAfford = false;
                            break;
                        }
                    }

                    if (!canAfford)
                    {
                        if (player != null && player.IsConnected) SendReply(player, "Upgrade canceled, cannot afford!");
                        break;
                    }

                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        //take payment
                        for (int i = 0; i < specifiedGradeCost.Count; i++)
                        {
                            var cost = specifiedGradeCost[i];

                            player.inventory.Take(null, cost.itemid, (int)cost.amount);

                            //note in inv (bottom right)
                            player.SendConsoleCommand(sb.Clear().Append("note.inv ").Append(cost.itemid).Append(" -").Append(cost.amount).ToString());
                        }

                        //player null check is done above
                        var percent = index / (float)totalCount * 100;
                        if (lastPerc < 0 || (percent - lastPerc) >= 25)
                        {
                            lastPerc = percent;
                            SendReply(player, sb.Clear().Append("Upgrade <color=#92ff45>").Append(percent.ToString("0.0").Replace(".0", string.Empty)).Append("%</color> done.").ToString());
                        }
                    }
                    finally { Pool.Free(ref sb); }
                }

                DoUpgrade(block, grade, desiredSkin, (player?.LastBlockColourChangeId ?? 0));

                if (timeBetweenBlocks > 0) yield return CoroutineEx.waitForSeconds(timeBetweenBlocks);
                else yield return CoroutineEx.waitForEndOfFrame;
            }

            if (player?.IsConnected ?? false) 
                SendReply(player, "Upgrade finished!");


            _currentUpgradeCoroutine.Remove(userId);
        }

        private void DoUpgrade(BuildingBlock block, BuildingGrade.Enum grade, ulong desiredSkin = 0, uint desiredColor = 0)
        {
            if (block.grade == grade && desiredSkin == block.skinID && desiredColor == block.playerCustomColourToApply)
                return;

            block.playerCustomColourToApply = desiredColor;

            block.ClientRPC(null, "DoUpgradeEffect", (int)grade, desiredSkin);

            if (block.grade == grade) //hack! for some reason you HAVE to call "ChangeGradeAndSkin" for it to work correctly, but if the grade is already equal, it messes things up
                block.ChangeGradeAndSkin(BuildingGrade.Enum.TopTier, 0, false, true);
           
            block.ChangeGradeAndSkin(grade, desiredSkin, true, true);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                Effect.server.Run(sb.Clear().Append("assets/bundled/prefabs/fx/build/promote_").Append(grade.ToString().ToLower()).Append(".prefab").ToString(), block, 0U, Vector3.zero, Vector3.zero, null, false);
            }
            finally { Pool.Free(ref sb); }
        }

        private BaseCombatEntity GetLookAtEntity(BasePlayer player, float maxDist = 90f, float radius = 1.25f, int layerMask = -1)
        {
            var currentRot = player.eyes.rotation * Vector3.forward;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);

            var hits = Physics.SphereCastAll(ray, radius, maxDist, layerMask);

            if (hits == null || hits.Length < 1)
                return null;


            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];

                var hitEnt = hit.GetEntity();

                if (hitEnt == null || hitEnt.IsDestroyed || hitEnt == player)
                    continue;


                var combatEnt = hitEnt as BaseCombatEntity;
                if (combatEnt == null)
                    continue;


                var ply = combatEnt as BasePlayer;
                if (ply != null && (ply.IsConnected || ply.IsSleeping()))
                    continue;


                return combatEnt;
            }


            return null;
        }

        private bool IsMVP(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            var perms = permission.GetUserPermissions(userID);
            if (perms == null || perms.Length < 1) return false;
            for (int i = 0; i < perms.Length; i++)
            {
                if (perms[i].Contains(".mvp", System.Globalization.CompareOptions.OrdinalIgnoreCase)) { return true; }
            }
            return false;
        }

        private bool IsVIP(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            var perms = permission.GetUserPermissions(userID);
            if (perms == null || perms.Length < 1) return false;
            for (int i = 0; i < perms.Length; i++)
            {
                if (perms[i].Contains(".vip", System.Globalization.CompareOptions.OrdinalIgnoreCase)) { return true; }
            }
            return false;
        }

        private HashSet<T> ToHashSet<T>(HashSet<T> hash) { return new HashSet<T>(hash); }

        private HashSet<T> ToHashSet<T>(List<T> list) { return new HashSet<T>(list); }

        private HashSet<T> ToHashSet<T>(IEnumerable<T> enumerable) { return new HashSet<T>(enumerable); }

        private ListHashSet<BuildingBlock> GetBlocksFromID(uint buildingId)
        {

            var building = BuildingManager.server.GetBuilding(buildingId);
            if (building == null)
            {
                PrintWarning("no building found from buildingID!!!: " + buildingId);
                return null;
            }

            return building.buildingBlocks;
        }

        private void Init()
        {
            string[] upCmds = { "up", "upg", "upgradeall", "upall", "upbuild", "upbuilding" };
            AddCovalenceCommand(upCmds, nameof(cmdUp));
        }


        private void cmdUp(IPlayer player, string command, string[] args)
        {
            var pObj = player?.Object as BasePlayer;
            if (pObj == null)
            {
                SendReply(player, "This command must be ran as a player!");
                return;
            }

            if (_currentUpgradeCoroutine.TryGetValue(pObj.UserIDString, out Coroutine upgradeCoroutine))
            {
                if (args == null || args.Length < 1 || !args[0].Equals("cancel", StringComparison.OrdinalIgnoreCase)) SendReply(player, "<color=#ffb44a>An upgrade is currently in progress. You can cancel with <color=red>/" + command + " cancel</color>.</color>");
                else
                {
                    ServerMgr.Instance.StopCoroutine(upgradeCoroutine);
                    _currentUpgradeCoroutine.Remove(pObj.UserIDString);
                    SendReply(player, "Canceled upgrade!");
                }
                return;
            }

            var escapeBlocked = NoEscape?.Call<bool>("IsEscapeBlockedS", player.Id) ?? false;
            if (escapeBlocked)
            {
                SendReply(player, "You cannot use this while raid or combat blocked!");
                return;
            }

            var vip = IsVIP(pObj.UserIDString);
            var mvp = IsMVP(pObj.UserIDString);
            var lookAt = GetLookAtEntity(pObj, 5f, 0.133f, constructionColl) as BuildingBlock;
            if (lookAt == null)
            {
                SendReply(player, "<color=#ffb44a>You must be looking at a building block (<color=yellow>wall, foundation, floor, etc</color>) to use this!</color>");
                return;
            }

            var getBlocks = GetBlocksFromID(lookAt.buildingID);
            if (getBlocks == null || getBlocks.Count < 1)
            {
                SendReply(player, "<color=red>No blocks found from building ID, you may want to report this issue to an administrator!</color>");
                return;
            }

            var upgradeBlocks = new HashSet<BuildingBlock>();

            if (!ulong.TryParse(player.Id, out ulong userId))
            {
                PrintWarning("fucked up not id: " + player.Id);
                return;
            }

            var cupboard = lookAt?.GetBuildingPrivilege() ?? null;
            if (cupboard != null)
            {
                if (!cupboard.IsAuthed(pObj))
                {
                    SendReply(player, "<color=red>You are not authorized to this building's cupboard!</color>");
                    return;
                }
            }

            var authPlayers = cupboard?.authorizedPlayers ?? null;
            var blockCount = getBlocks.Count;
            var ownedCount = 0;

            foreach (var ent in getBlocks)
            {
                if (ent == null || ent.IsDestroyed) continue;
                if (ent.OwnerID == 0) continue;

                if (ent.OwnerID == userId)
                {
                    ownedCount++;
                    continue;
                }

                var isFriend = Friends?.Call<bool>("HasFriend", ent.OwnerID, userId) ?? false;
                if (isFriend) ownedCount++;
            }

            var ownedPerc = ownedCount * 100d / blockCount;
            var doesOwn = ownedPerc > 50;
            if (!doesOwn)
            {
                SendReply(player, "<color=orange>You or a <color=#8aff47>friend</color> (use <color=#2aaced>/friend</color>) must own the majority of this base before using <color=#ff912b>/" + command + "</color>.</color>");
                return;
            }

            foreach (var block in getBlocks)
            {
                if (block.OwnerID == userId)
                {
                    upgradeBlocks.Add(block);
                }
                else
                {
                    var isFriend = Friends?.Call<bool>("HasFriend", block.OwnerID, userId) ?? false;
                    if (isFriend) upgradeBlocks.Add(block);
                }
            }

            var costs = new Dictionary<int, double>();
            var gradeCosts = new Dictionary<BuildingGrade.Enum, Dictionary<int, double>>();
            if (args == null || args.Length < 1)
            {
                var gradeEnumLength = Enum.GetValues(typeof(BuildingGrade.Enum)).Length;

                foreach (var bloc in upgradeBlocks)
                {
                    //if ((int)bloc.grade >= gradeEnumLength)
                    //  continue;

                    if (authPlayers != null && authPlayers.Count > 0)
                    {
                        var hasPriv = false;

                        foreach (var ap in authPlayers)
                        {
                            if (ap.userid == bloc.OwnerID)
                            {
                                hasPriv = true;
                                break;
                            }
                        }

                        if (!hasPriv) continue;
                    }

                    for (int i = (int)bloc.grade + 1; i < gradeEnumLength; i++)
                    {

                        var intToGrade = (BuildingGrade.Enum)i;


                        if (intToGrade > BuildingGrade.Enum.TopTier)
                            break;

                        var grade = bloc.blockDefinition.GetGrade(intToGrade, 0);

                        var specifiedGradeCost = grade.CostToBuild(bloc.grade);
                        if (specifiedGradeCost == null) continue;

                        for (int j = 0; j < specifiedGradeCost.Count; j++)
                        {
                            var cost = specifiedGradeCost[j];
                            if (!gradeCosts.TryGetValue(intToGrade, out Dictionary<int, double> tempCost)) gradeCosts[intToGrade] = new Dictionary<int, double>();
                            if (!gradeCosts[intToGrade].TryGetValue(cost.itemid, out double temp)) gradeCosts[intToGrade][cost.itemid] = cost.amount;
                            else gradeCosts[intToGrade][cost.itemid] += cost.amount;
                            if (!costs.TryGetValue(cost.itemid, out double outAmount)) costs[cost.itemid] = cost.amount;
                            else costs[cost.itemid] += cost.amount;
                        }
                    }

                }
                var costSB2 = new StringBuilder();
                foreach (var kvp in gradeCosts)
                {
                    var gradeName = kvp.Key == BuildingGrade.Enum.TopTier ? "Armored" : kvp.Key.ToString();
                    costSB2.AppendLine((costSB2.Length > 0 ? Environment.NewLine : string.Empty) + "<color=#f9ff42>" + gradeName + " (" + (int)kvp.Key + ")</color> <color=#ff6ee7>costs</color>:");
                    foreach (var kvp2 in kvp.Value)
                    {
                        var engName = ItemManager.FindItemDefinition(kvp2.Key)?.displayName?.english ?? "Unknown item name";
                        costSB2.AppendLine("<color=#ffb44a>" + engName + " </color>x<color=#9f63ff>" + kvp2.Value.ToString("N0") + "</color>");
                    }
                }

                var costStr = costSB2.ToString().TrimEnd();
                SendReply(player, "<color=#ffb44a>Please specify a tier from 1-4. </color><color=#59c4ff>1 for Wood, 2 for Stone, 3 for Metal, 4 for Armored</color>." + Environment.NewLine + "Example: <color=yellow>/" + command + " 2</color>" + Environment.NewLine + Environment.NewLine + (string.IsNullOrEmpty(costStr) ? "<color=#9430ff>This building is already fully upgraded </color><color=#e882ff>:^)</color>" : ("--<color=#ff6ee7><size=20>GRADE COSTS</size></color>--" + Environment.NewLine + costStr)));
                return;
            }
            if (!int.TryParse(args[0], out int tierInt))
            {
                SendReply(player, "<color=#ffb44a>Not a number: <color=yellow>" + args[0] + "</color>, please specify a tier from 1-4. </color><color=#59c4ff>1 for Wood, 2 for Stone, 3 for Metal, 4 for Armored</color>.");
                return;
            }
            if (tierInt < 1 || tierInt > 4)
            {
                SendReply(player, "<color=#ffb44a>Invalid number: " + tierInt + " specified. Try a number 1-4</color>");
                return;
            }



            var toUpgrade = new HashSet<BuildingBlock>();
            foreach (var bloc in upgradeBlocks)
            {

                var desiredGrade = (BuildingGrade.Enum)tierInt;
                var specifiedGradeCost = bloc.blockDefinition.GetGrade(desiredGrade, 0).CostToBuild(bloc.grade);
                if (specifiedGradeCost == null) continue;


                var cantAffordAnyMore = false;

                if (bloc.grade < desiredGrade)
                {
                    for (int i = 0; i < specifiedGradeCost.Count; i++)
                    {
                        var cost = specifiedGradeCost[i];
                        var pAmount = pObj?.inventory?.GetAmount(cost.itemid) ?? 0;

                        if (costs.TryGetValue(cost.itemid, out double outAmount))
                        {
                            costs[cost.itemid] += cost.amount;

                            if ((outAmount + cost.amount) > pAmount)
                                cantAffordAnyMore = true;
                        }
                        else costs[cost.itemid] = cost.amount;

                        if (cost.amount > pAmount)
                            cantAffordAnyMore = true;
                    }
                }

                if (!cantAffordAnyMore)
                    toUpgrade.Add(bloc);
            }

            if (toUpgrade.Count < 1)
            {
                /*/
                var itemAmounts = Pool.GetList<ItemAmount>();
                try 
                {
                    foreach(var kvp in costs)
                        itemAmounts.Add(new ItemAmount(ItemManager.FindItemDefinition(kvp.Key), (float)kvp.Value));

                   // OnRepairFailedResources(pObj, itemAmounts);
                }
                finally { Pool.FreeList(ref itemAmounts); }/*/ //sad!

                SendReply(player, "There's nothing to upgrade or you cannot afford to upgrade to this tier!"); //improve since we can't use above
                return;
            }
            /*/
            if (costs == null || costs.Count < 1)
            {
                SendReply(player, "<color=#ffb44a>Failed to get costs!</color> <color=yellow>This may happen if you tried to downgrade the building (selected lower grade)</color>.");
                return;
            }/*/
            SendReply(player, "You can afford to upgrade <color=#59c4ff>" + toUpgrade.Count.ToString("N0") + "</color> out of <color=#9f63ff>" + upgradeBlocks.Count.ToString("N0") + "</color> total blocks");


            if (toUpgrade == null || toUpgrade.Count < 1)
            {
                SendReply(player, "Nothing to upgrade!");
                PrintError("toUpgrade null or count < 1 ?!?!?!"); //shouldn't be possible
                return;
            }

            SendReply(player, "<color=yellow>Upgrade started!</color>");
            ServerMgr.Instance.StartCoroutine(StartMassUpgrade(toUpgrade, (BuildingGrade.Enum)tierInt, vip ? 0f : mvp ? 0.1f : 0.25f, pObj));
        }



        #region Util

        public void OnRepairFailedResources(BasePlayer player, List<ItemAmount> requirements) //borderline useless - you must be holding a hammer (client-side rpc)
        {
            if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead()) return;

            var oldBeltCap = player?.inventory?.containerBelt?.capacity ?? 0;


            var activeItem = player?.GetActiveItem();

            Item hammerItem = null;

            if (activeItem == null || activeItem.info.itemid != 200773292)
            {

                if (oldBeltCap > 0)
                {
                    player.inventory.containerBelt.capacity += 1;

                    hammerItem = ItemManager.CreateByItemID(200773292, 1);

                    if (!hammerItem.MoveToContainer(player.inventory.containerBelt, 6))
                    {
                        RemoveFromWorld(hammerItem);
                        hammerItem = null;
                    }
                    else
                    {
                        PrintWarning("moved hammer item!! set actiev to uid: " + hammerItem.uid);

                        player.UpdateActiveItem(hammerItem.uid);
                        player.SendNetworkUpdateImmediate();
                    }

                }

            }

            player.Invoke(() =>
            {
                try
                {
                    using (ProtoBuf.ItemAmountList itemAmountList = ItemAmount.SerialiseList(requirements))
                        player.ClientRPCPlayer(null, player, "Client_OnRepairFailedResources", itemAmountList);
                }
                finally
                {
                    if (player?.inventory?.containerBelt != null) 
                        player.inventory.containerBelt.capacity = oldBeltCap;

                    if (hammerItem != null)
                    {
                        PrintWarning("hammer item not null, remove!");
                        RemoveFromWorld(hammerItem);
                    }
                }
            }, 0.25f);

          

           
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

        private void SendReply(BasePlayer player, string msg, string userId = CHAT_STEAM_ID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
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

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }
        #endregion

    }
}