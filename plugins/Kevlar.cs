using Facepunch;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Kevlar", "Shady", "0.0.2")]

    public class Kevlar : RustPlugin
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
        //  private readonly HashSet<uint> _noNetworks = new HashSet<uint>();

        private void Init()
        {
            Unsubscribe(nameof(OnEntityTakeDamage));
        }

        private void OnServerInitialized() 
        { 
            Subscribe(nameof(OnEntityTakeDamage)); 
        }

        private void OnPlayerLootEnd(PlayerLoot inventory)
        {
            if (inventory == null) return;

            var player = inventory.GetComponent<BasePlayer>();
            if (player == null) return;

            var ent = inventory?.entitySource as LootContainer;


            if (inventory == null || inventory.entitySource == null || (inventory?.entitySource?.IsDestroyed ?? true)) return;

            var entSourceName = inventory?.entitySource?.ShortPrefabName ?? string.Empty;
            if (entSourceName.Contains("lantern"))
            {
                try 
                {
                    PrintWarning("contains lantern!");

                    var pos = inventory?.entitySource?.transform?.position ?? Vector3.zero;

                    PrintWarning("Got pos: " + pos);

                    if (pos.y == -40)
                    {
                        PrintWarning("pos.y == -40");
                        Item itemSlot = null;

                        try
                        {
                            PrintWarning("try get itemslot");
                            itemSlot = inventory?.entitySource?.GetComponent<StorageContainer>()?.inventory?.GetSlot(0) ?? null;
                        }
                        catch (Exception ex) 
                        {
                            PrintError(ex.ToString());
                            PrintWarning("Exception occurred when trying to get itemSlot");
                        }

                        if (itemSlot != null)
                        {
                            PrintWarning("itemSlot is not null");
                            if (!TryMoveItemToContainer(itemSlot, player.inventory.containerBelt, false)) if (!TryMoveItemToContainer(itemSlot, player.inventory.containerMain, false)) TryMoveItemToContainer(itemSlot, player.inventory.containerWear);
                        }

                        PrintWarning("pre ent kill");
                        if (ent != null && !ent.IsDestroyed) ent.Kill();
                        PrintWarning("killed lantern ent after loot end (if necessary)");
                    }
                }
                finally
                {
                    CuiHelper.DestroyUi(player, "GUIKevlar");
                }
               

                
            }
        }

        private object OnEntityTakeDamage(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null || !IsValidPlayer(player)) return null;

            var victim = player;



            var attacker = info?.Initiator as BasePlayer;

            var boneID = info?.HitBone ?? 0;
            var boneName = (boneID == 0) ? "Body" : GetBoneName(player, boneID) ?? "Body";

            var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            var boneNameLower = boneName.ToLower().TrimEnd(' ');
            var dmgType = majDmg;


            if (IsValidPlayer(victim))
            {

                if (majDmg == Rust.DamageType.Bullet || majDmg == Rust.DamageType.Bite)
                {
                    var kevScalar = 1f;
                    for (int i = 0; i < victim.inventory.containerWear.itemList.Count; i++)
                    {
                        var item = victim.inventory.containerWear.itemList[i];
                        if (item == null) continue;
                        var lowerName = (item?.name ?? string.Empty).ToLower();
                        if (string.IsNullOrEmpty(lowerName) || !lowerName.Contains("kevlar")) continue;

                        if (boneName.Equals("Head", StringComparison.OrdinalIgnoreCase) && lowerName.Contains("balaclava")) kevScalar -= 0.166f;

                        if (item.info.shortname.Contains("hoodie") && (boneNameLower == "body" || boneNameLower == "chest" || boneNameLower.EndsWith("arm") || boneNameLower == "stomach" || boneNameLower == "lower spine")) kevScalar -= 0.166f;
                        if (item.info.shortname.Contains("pants") && (boneNameLower.Contains("knee") || boneNameLower == "pelvis" || boneNameLower.Contains("hip") || boneNameLower == "groin")) kevScalar -= 0.166f;
                        if (item.info.shortname.Contains("boots") && (boneNameLower.Contains("foot") || boneNameLower.Contains("toe"))) kevScalar -= 0.166f;
                        if (item.info.shortname.Contains("glove") && boneNameLower.Contains("hand") || boneNameLower.Contains("finger")) kevScalar -= 0.166f;

                        if (attacker == null) kevScalar -= 0.133f;
                    }

                    if (kevScalar != 1f)
                    {
                        info?.damageTypes?.ScaleAll(kevScalar);
                        if (victim.IsAdmin) PrintWarning("KEV SCALAR: " + kevScalar + ", " + boneNameLower);
                    }
                }

                if (dmgType == Rust.DamageType.Cold || dmgType == Rust.DamageType.ColdExposure)
                {
                    var kevCount = 0;

                    if ((victim?.inventory?.containerWear?.itemList?.Count ?? 0) > 0)
                    {
                        for (int i = 0; i < victim.inventory.containerWear.itemList.Count; i++)
                        {
                            if (victim?.inventory?.containerWear?.itemList[i]?.name?.Contains("kevlar", CompareOptions.OrdinalIgnoreCase) ?? false) kevCount++;
                        }
                    }

                    if (kevCount > 0)
                    {
                        var coldScalar = 1f;
                        for (int i = 0; i < kevCount; i++) coldScalar -= 0.133f;
                        info?.damageTypes?.ScaleAll(coldScalar);
                    }
                }

            }



            return null;
        }

        [ChatCommand("kevlar")]
        private void cmdKev(BasePlayer player, string command, string[] args)
        {
            var costItems = new Dictionary<string, int>
            {
                ["ducttape"] = 4,
                ["glue"] = 3,
                ["leather"] = 70,
                ["metal.refined"] = 4
            };


            if (args.Length < 1 || args[0].ToLower() != "upgrade")
            {
                var costSB = Pool.Get<StringBuilder>();

                try
                {
                    costSB.Clear();

                    foreach (var kvp in costItems)
                    {
                        var itemDef = ItemManager.FindItemDefinition(kvp.Key);
                        if (itemDef == null) continue;
                        var itemAmt = player?.inventory?.GetAmount(itemDef.itemid) ?? 0;
                        costSB.Append(itemDef.displayName.english).Append(" x").Append(kvp.Value.ToString("N0")).Append(" (Have: ").Append(itemAmt.ToString("N0")).Append(")").Append(Environment.NewLine);
                    }

                    if (costSB.Length > 1)
                        costSB.Length--;

                    SendReply(player, "<size=20><color=#ff912b>Kevlar Armor</color></size>\n<color=#ff912b>-</color>Kevlar armor is an 'upgraded' version of hoodies, gloves, pants, boots and a balaclava.\n\n<color=#ff912b>-</color>Kevlar armor provides a <color=#ff912b>16%</color> damage reduction to the body part hit if the item covers it. For example, if you have Kevlar-lined gloves and you get hit in the hand, you take <color=#ff912b>16%</color> less damage (does not stack).\n\n<color=#ff912b>-</color>You can upgrade any of these items for:\n" + costSB.ToString().TrimEnd());
                    SendReply(player, "<color=#ff912b>-</color>To upgrade, type <color=yellow>/kevlar upgrade</color> - place the item in the inventory slot that appears and press the button, then");
                }
                finally { Pool.Free(ref costSB); }

                return;
            }

            var adjustPos = new Vector3(player.transform.position.x, -40f, player.transform.position.z);
            var playerRot = player?.transform?.rotation ?? default(Quaternion);
            var lantern = GameManager.server.CreateEntity("assets/prefabs/deployable/lantern/lantern.deployed.prefab", adjustPos, playerRot);
            if (lantern == null) return;

            lantern.enableSaving = false;
            lantern.OwnerID = player.userID;
            lantern.Spawn();


            var lanternLoot = lantern?.GetComponent<StorageContainer>() ?? null;

            lantern?.Invoke(() =>
            {
                if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? true)) return;
                lanternLoot.allowedItem = null;
                lanternLoot.allowedItem2 = null; //????
                lanternLoot.inventory.onlyAllowedItems = null;
                lanternLoot.inventory.canAcceptItem = null;


                player.inventory.loot.containers.Clear();
                player.inventory.loot.entitySource = null;
                player.inventory.loot.itemSource = null;
                player.inventory.loot.MarkDirty();
                player.inventory.loot.PositionChecks = false;
                var lanternFuel = lanternLoot?.inventory?.GetSlot(0) ?? null;
                if (lanternFuel != null) RemoveFromWorld(lanternFuel);

                player.inventory.loot.StartLootingEntity(lantern, false);
                player.inventory.loot.AddContainer(lanternLoot.inventory);
                player.inventory.loot.SendImmediate();

                player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", "lantern");
                LanternGUI(player, "Insert an item.");
            }, 0.1f);
        }

        private readonly Dictionary<string, Timer> kevlarGUITimer = new Dictionary<string, Timer>();

        private void LanternGUI(BasePlayer player, string text = "", float delayTime = 0f)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;


            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.6"
                },
                RectTransform =
                {
                    AnchorMin = "0.67 0.12",
                    AnchorMax = "0.715 0.20"
                },
                CursorEnabled = true
            }, "Overlay", "GUIKevlar");

            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = text,
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "0.7 0.60 0.67 1.0"
                },
                RectTransform =
                {
                    AnchorMin = "0.035 0.2",
                    AnchorMax = "0.9 1.3"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "kevlar.upgrade",
                        Color = "0.43 0.6 0.85 0.75"
                    },
                Text =
                {
                    Text = "Upgrade Now",
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.05 0.1",
                    AnchorMax = "0.9 0.4"
                }
            }, GUISkin);

            if (delayTime > 0f)
            {
                Timer kevTimer = null;
                if (kevlarGUITimer.TryGetValue(player.UserIDString, out kevTimer))
                {
                    kevTimer.Destroy();
                    kevTimer = null;
                    kevlarGUITimer.Remove(player.UserIDString);
                }
                kevTimer = timer.Once(delayTime, () =>
                {
                    if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? false) || !IsLooting(player)) return;
                    CuiHelper.DestroyUi(player, "GUIKevlar");
                    CuiHelper.AddUi(player, GUISkinElement);
                });
                kevlarGUITimer[player.UserIDString] = kevTimer;
            }
            else
            {
                CuiHelper.DestroyUi(player, "GUIKevlar");
                CuiHelper.AddUi(player, GUISkinElement);
            }
        }

        [ConsoleCommand("kevlar.upgrade")]
        private void consoleSkinChange(ConsoleSystem.Arg arg)
        {
            var errorSB = new StringBuilder();
            try
            {
                var player = arg?.Player() ?? null;
                if (player == null)
                {
                    arg.ReplyWith("This can only be ran as a player!");
                    return;
                }

                if (!IsLooting(player) || (player?.IsSleeping() ?? false) || (player?.IsWounded() ?? false))
                {
                    arg.ReplyWith("You must be looting to use this!");
                    CuiHelper.DestroyUi(player, "GUISkin");
                    return;
                }

                errorSB.AppendLine("Got past looting and player check");

                var lootingContainer = player?.inventory?.loot?.entitySource ?? null;
                var isLantern = lootingContainer.ShortPrefabName.Contains("lantern"); //prefabid check please

                if (!isLantern)
                {
                    arg.ReplyWith("Invalid container!");
                    return;
                }

                var lantern = lootingContainer?.GetComponent<BaseOven>() ?? null;
                if (lantern?.allowedItem != null)
                {
                    PrintWarning("lantern is not null allowed item");
                    return;
                }

                errorSB.AppendLine("Got past lantern check");

                var item = lantern?.inventory?.GetSlot(0) ?? null;
                var containerSlot = lantern?.inventory ?? null;

                if (item == null || containerSlot == null)
                {
                    PrintWarning("item is null/slot");
                    return;
                }

                if ((item?.name ?? string.Empty).Contains("kevlar", CompareOptions.OrdinalIgnoreCase))
                {
                    LanternGUI(player, "Already upgraded!");
                    LanternGUI(player, "", 2f);
                    return;
                }

                var skinUse = 0UL;

              


                if (item.info.shortname.Contains("hoodie")) skinUse = 897890977;
                if (item.info.shortname.Contains("pants")) skinUse = 888360095;
                if (item.info.shortname.Contains("gloves")) skinUse = 874488180;
                if (item.info.shortname.Contains("boots")) skinUse = 869090082;
                if (skinUse == 0 && !item.info.shortname.Contains("balaclava"))
                {
                    LanternGUI(player, "Unsupported item!");
                    LanternGUI(player, "", 2f);
                    SendReply(player, "Invalid item! Try upgrading hoodie, pants, gloves, boots or a balaclava.");
                    return;
                }

                var onHook = Interface.Oxide.CallHook("OnKevlarGetSkin", item, skinUse);
                if (onHook != null)
                    skinUse = (ulong)onHook;
                

                var costItems = new Dictionary<string, int>
                {
                    ["ducttape"] = 4,
                    ["glue"] = 3,
                    ["leather"] = 70,
                    ["metal.refined"] = 4
                };

                var hasEnough = true;

                foreach (var kvp in costItems)
                {
                    var itemDef = ItemManager.FindItemDefinition(kvp.Key);
                    if (itemDef == null) continue;
                    var itemAmt = player?.inventory?.GetAmount(itemDef.itemid) ?? 0;
                    if (itemAmt < kvp.Value)
                    {
                        hasEnough = false;
                        break;
                    }
                }

                if (!hasEnough)
                {
                    LanternGUI(player, "Not enough!");
                    LanternGUI(player, "", 2f);
                    return;
                }

                foreach (var kvp in costItems)
                {
                    var itemDef = ItemManager.FindItemDefinition(kvp.Key);
                    if (itemDef == null) continue;
                    player.inventory.Take(null, itemDef.itemid, kvp.Value);
                    player.SendConsoleCommand("note.inv", itemDef.itemid, "-" + kvp.Value);
                }

                item.skin = skinUse;
                item.name = item.name + (string.IsNullOrWhiteSpace(item.name) ? (item?.info?.displayName?.english ?? "Unknown") : string.Empty) + " (Kevlar)";
                item.MarkDirty();
                LanternGUI(player, "Upgraded!");
                LanternGUI(player, "", 2f);
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintWarning(errorSB.ToString().TrimEnd());
                arg.ReplyWith("<color=red>Failed to process kevlar!</color> Please report this to an administrator.");
            }
        }

        #region Util
        private bool IsValidPlayer(BasePlayer player) { return player != null && player.gameObject != null && !player.IsDestroyed && !player.IsNpc && player.prefabID == 4108440852; }

        private string GetBoneName(BaseCombatEntity entity, uint boneId) => entity?.skeletonProperties?.FindBone(boneId)?.name?.english ?? "Body";

        private bool TryMoveItemToContainer(Item item, ItemContainer container, bool TryDropIfFailed = true)
        {
            if (item == null || container == null) return false;
            try 
            {
                if (!item.MoveToContainer(container))
                {
                    var pos = item?.parent?.entityOwner?.transform?.position ?? item?.parent?.playerOwner?.transform?.position ?? Vector3.zero;
                    var velocity = item?.parent?.playerOwner?.GetDropVelocity() ?? item?.parent?.entityOwner?.GetDropVelocity() ?? Vector3.zero;
                    var rotation = item?.parent?.entityOwner?.transform?.rotation ?? item?.parent?.playerOwner?.transform?.rotation ?? default(Quaternion);
                    if (TryDropIfFailed && pos != Vector3.zero) item.Drop(pos, velocity, rotation);
                    return false;
                }
                else return true;
            }
            catch(Exception ex) 
            { 
                PrintError(ex.ToString());
                PrintWarning(nameof(TryMoveItemToContainer) + " threw an exception");

                throw ex;
            }
        }

        private bool IsLooting(BasePlayer player) { return player?.inventory?.loot?.entitySource != null; }

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