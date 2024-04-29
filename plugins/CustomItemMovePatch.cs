// Reference: 0Harmony
using Facepunch;
using HarmonyLib;
using Oxide.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Custom Item Move Patch", "Shady", "1.0.0")]
    class CustomItemMovePatch : RustPlugin
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
        #region Fields
        private Harmony _harmony;
        #endregion
        #region Hooks
        private void Init() => DoHarmonyPatches(_harmony = new Harmony(GetType().Name));

        private void Unload() => _harmony?.UnpatchAll(GetType().Name);
        #endregion
        #region Custom Harmony Methods
        private void DoHarmonyPatches(Harmony instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var c = 0;

            var watch = Pool.Get<Stopwatch>();
            try { c = PatchAllHarmonyAttributes(GetType(), instance); }
            finally
            {
                var elapsedMs = watch.ElapsedMilliseconds;
                Pool.Free(ref watch);

                var sb = Pool.Get<StringBuilder>();
                try { PrintWarning(sb.Clear().Append("Took: ").Append(elapsedMs.ToString("0.00").Replace(".00", string.Empty)).Append("ms to apply ").Append(c.ToString("N0")).Append(" patch").Append(c > 1 ? "es" : string.Empty).ToString()); }
                finally { Pool.Free(ref sb); }
            }

        }

        private int PatchAllHarmonyAttributes(Type type, Harmony harmony, BindingFlags? flags = null)
        {
            var patched = 0;


            var types = Assembly.GetExecutingAssembly().GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];

                if (t != type && t.IsClass && t.FullName.Contains(type.FullName))
                {
                    var attributes = Attribute.GetCustomAttributes(t);
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        try
                        {
                            var patch = attributes[j] as HarmonyPatch;
                            if (patch == null) continue;

                            if (string.IsNullOrEmpty(patch?.info?.methodName))
                            {
                                PrintWarning("patch.info.methodName is null/empty!!");
                                continue;
                            }

                            if (patch?.info?.declaringType == null)
                            {
                                PrintWarning("declaringType is null?!: " + ", info: " + (patch?.info?.ToString() ?? string.Empty) + ", declaringType: " + (patch?.info?.declaringType?.ToString() ?? string.Empty));
                                continue;
                            }

                            var originalMethod = patch.info.declaringType.GetMethod(patch.info.methodName, (flags != null && flags.HasValue) ? (BindingFlags)flags : BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                            if (originalMethod == null)
                            {
                                PrintWarning("originalMethod is null!! patch.info.methodName: " + patch.info.methodName + Environment.NewLine + "patch.info.declaringType: " + patch.info.declaringType.FullName);
                                continue;
                            }

                            HarmonyMethod prefix = null;
                            HarmonyMethod postfix = null;

                            var prefixMethod = t.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                            var postFixMethod = t.GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                            if (prefixMethod != null) prefix = new HarmonyMethod(prefixMethod);
                            if (postFixMethod != null) postfix = new HarmonyMethod(postFixMethod);

                            harmony.Patch(originalMethod, prefix, postfix);

                            patched++;
                        }
                        catch (Exception ex) { PrintError(ex.ToString()); }

                    }
                }

            }

            return patched;
        }
        #endregion
        #region CanExistWith Patch
        [HarmonyPatch(typeof(ItemModWearable), "CanExistWith")]
        private class CanExistWithPatch
        {
            private static bool Prefix(ItemModWearable __instance, ItemModWearable wearable, ref bool __result)
            {
                if (__instance == null)
                    throw new ArgumentNullException(nameof(__instance));

                if (wearable == null)
                    throw new ArgumentNullException(nameof(wearable));

                var hookVal = Interface.Oxide.CallHook("CanExistWith", __instance, wearable);
                if (hookVal != null)
                {
                    __result = (bool)hookVal;
                    return false;
                }

                return true;
            }
        }
        #endregion
        #region MoveToContainer Patch
        [HarmonyPatch(typeof(Item), "MoveToContainer")]
        private class MoveToContainerPatch
        {

            private static void RemoveConflictingSlots(Item item, ItemContainer container, BaseEntity entityOwner, BasePlayer sourcePlayer)
            {
                if (container.availableSlots == null || container.availableSlots.Count < 1)
                    return;

                var list = Pool.GetList<Item>();
                try
                {
                    list.AddRange(container.itemList);

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i];

                        if (!obj.DoItemSlotsConflict(item)) continue;

                        obj.RemoveFromContainer();

                        var basePlayer = entityOwner as BasePlayer;

                        if (basePlayer != null) basePlayer.GiveItem(obj, BaseEntity.GiveItemReason.Generic);
                        else
                        {
                            var itemContainerEntity = entityOwner as IItemContainerEntity;

                            if (itemContainerEntity != null)
                                obj.MoveToContainer(itemContainerEntity.inventory, -1, true, false, sourcePlayer, true);
                        }
                    }

                }
                finally { Pool.FreeList(ref list); }
            }


            private static bool Prefix(Item __instance, ItemContainer newcontainer, ref bool __result, int iTargetPos = -1, bool allowStack = true, bool ignoreStackLimit = false, BasePlayer sourcePlayer = null, bool allowSwap = true)
            {
                if (__instance == null)
                    throw new ArgumentNullException(nameof(__instance));

                var flag1 = iTargetPos == -1;

                ItemContainer parent1 = __instance.parent;
                IItemContainerEntity entityOwner1 = null;

                try
                {
                    if (iTargetPos == -1 && allowStack && __instance.MaxStackable() > 1)
                    {
                        var itemList = newcontainer?.itemList;

                        for (int i = 0; i < itemList.Count; i++)
                        {
                            var itemInList = itemList[i];
                            if (itemInList != null && itemInList.info.itemid == __instance.info.itemid && itemInList.CanStack(__instance) && (ignoreStackLimit || itemInList.amount < itemInList.MaxStackable()))
                            {
                                iTargetPos = itemInList.position;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("1: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    if (iTargetPos == -1)
                    {
                        entityOwner1 = newcontainer.GetEntityOwner(true) as IItemContainerEntity;

                        if (entityOwner1 != null)
                        {
                            iTargetPos = entityOwner1.GetIdealSlot(sourcePlayer, newcontainer, __instance);

                            if (iTargetPos == int.MinValue)
                            {
                                Interface.Oxide.LogWarning("iTargetPos was == minvalue!!!");

                                __result = false;
                                return false;
                            }


                        }

                    }
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("2: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    if (iTargetPos == -1)
                    {
                        if (newcontainer == __instance.parent)
                        {
                            __result = false;
                            return false;
                        }
                        else
                        {
                            var wearable = newcontainer.HasFlag(ItemContainer.Flag.Clothing) && __instance.info.isWearable;
                            var modWearable = __instance.info.ItemModWearable;

                            for (int i = 0; i < newcontainer.capacity; i++)
                            {
                                var slot1 = newcontainer.GetSlot(i);
                                if (slot1 == null)
                                {
                                    if (__instance.CanMoveTo(newcontainer, i))
                                    {
                                        iTargetPos = i;
                                        break;
                                    }
                                }
                                else
                                {

                                    if (wearable && slot1 != null && !slot1.info.ItemModWearable.CanExistWith(modWearable))
                                    {
                                        iTargetPos = i;
                                        break;
                                    }

                                    if (newcontainer.availableSlots != null && newcontainer.availableSlots.Count > 0 && __instance.DoItemSlotsConflict(slot1))
                                    {
                                        iTargetPos = i;
                                        break;
                                    }
                                }

                            }

                            if (wearable && iTargetPos == -1)
                                iTargetPos = newcontainer.capacity - 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("3: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    if (iTargetPos == -1 || !__instance.CanMoveTo(newcontainer, iTargetPos))
                    {
                        __result = false;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("4: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    if (iTargetPos >= 0 && newcontainer.SlotTaken(__instance, iTargetPos))
                    {
                        Item slot = newcontainer.GetSlot(iTargetPos);
                        if (slot == __instance)
                        {
                            __result = false;
                            return false;
                        }

                        Item actualDesiredSlot = null;

                        if (slot != null && slot.info.itemid == __instance.info.itemid && !string.IsNullOrWhiteSpace(__instance?.name) && __instance?.name != slot?.name && slot.MaxStackable() > 1)
                        {

                            for (int i = 0; i < newcontainer.itemList.Count; i++)
                            {
                                var itemFind = newcontainer.itemList[i];
                                var hasSameName = itemFind?.name == __instance?.name;
                                if (itemFind.uid != __instance.uid && itemFind.info.itemid == __instance.info.itemid && hasSameName)
                                {
                                    actualDesiredSlot = itemFind;
                                    break;
                                }
                            }
                        }


                        if (actualDesiredSlot != null)
                        {
                            Interface.Oxide.LogWarning(nameof(actualDesiredSlot) + " was not null, slot was: " + slot + ", set to: " + actualDesiredSlot);
                            slot = actualDesiredSlot;
                        }


                        if (allowStack && slot != null)
                        {
                            int num1 = slot.MaxStackable();
                            if (slot.CanStack(__instance))
                            {
                                if (ignoreStackLimit)
                                    num1 = int.MaxValue;

                                if (slot.amount >= num1)
                                {
                                    __result = false;
                                    return false;
                                }

                                int num2 = Mathf.Min(num1 - slot.amount, __instance.amount);
                                slot.amount += num2;
                                __instance.amount -= num2;
                                slot.MarkDirty();
                                __instance.MarkDirty();
                                Interface.CallHook("OnItemStacked", (object)slot, (object)__instance, (object)newcontainer, (object)num2);
                                if (__instance.amount <= 0)
                                {
                                    __instance.RemoveFromWorld();
                                    __instance.RemoveFromContainer();
                                    __instance.Remove(0.0f);

                                    __result = true;
                                    return false;
                                }
                                if (flag1)
                                {
                                    var move = __instance.MoveToContainer(newcontainer, -1, allowStack, ignoreStackLimit, sourcePlayer, true);

                                    __result = move;

                                    return false;
                                }

                                __result = false;

                                return false;
                            }
                        }
                        if (!(__instance.parent != null & allowSwap) || slot == null)
                        {
                            __result = false;
                            return false;
                        }

                        ItemContainer parent2 = __instance.parent;
                        int position1 = __instance.position;
                        ItemContainer parent3 = slot.parent;
                        int position2 = slot.position;

                        if (!slot.CanMoveTo(parent2, position1))
                        {
                            __result = false;
                            return false;
                        }

                        BaseEntity entityOwner2 = __instance.GetEntityOwner();
                        BaseEntity entityOwner3 = slot.GetEntityOwner();
                        __instance.RemoveFromContainer();
                        slot.RemoveFromContainer();

                        RemoveConflictingSlots(__instance, newcontainer, entityOwner2, sourcePlayer);
                        RemoveConflictingSlots(slot, parent2, entityOwner3, sourcePlayer);


                        if (slot.MoveToContainer(parent2, position1, true, false, sourcePlayer, true) && __instance.MoveToContainer(newcontainer, iTargetPos, true, false, sourcePlayer, true))
                        {

                            __result = true;

                            return false;
                        }

                        __instance.RemoveFromContainer();
                        slot.RemoveFromContainer();


                        __instance.SetParent(parent2);
                        __instance.position = position1;


                        slot.SetParent(parent3);
                        slot.position = position2;

                        __result = true;

                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("5: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    if (__instance.parent == newcontainer)
                    {
                        if (iTargetPos < 0 || iTargetPos == __instance.position || __instance.parent.SlotTaken(__instance, iTargetPos))
                        {

                            __result = false;
                            return false;
                        }

                        __instance.position = iTargetPos;
                        __instance.MarkDirty();


                        __result = true;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("6: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    if (newcontainer.maxStackSize > 0 && newcontainer.maxStackSize < __instance.amount)
                    {
                        Item obj1 = __instance.SplitItem(newcontainer.maxStackSize);
                        if (obj1 != null && !obj1.MoveToContainer(newcontainer, iTargetPos, false, false, sourcePlayer, true) && (parent1 == null || !obj1.MoveToContainer(parent1, -1, true, false, sourcePlayer, true)))
                        {
                            Item obj2 = obj1;
                            Vector3 dropPosition = newcontainer.dropPosition;
                            Vector3 dropVelocity = newcontainer.dropVelocity;
                            Quaternion rotation = new Quaternion();
                            Interface.CallHook("OnItemStacked", (object)entityOwner1, __instance, (object)newcontainer);
                            obj2.Drop(dropPosition, dropVelocity, rotation);
                        }

                        __result = true;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("7: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    if (!newcontainer.CanAccept(__instance))
                    {
                        __result = false;
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    Interface.Oxide.LogError("8: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

                try
                {
                    BaseEntity entityOwner4 = __instance.GetEntityOwner();

                    __instance.RemoveFromContainer();
                    __instance.RemoveFromWorld();
                    RemoveConflictingSlots(__instance, newcontainer, entityOwner4, sourcePlayer);
                    __instance.position = iTargetPos;
                    __instance.SetParent(newcontainer);


                    __result = true;
                    return false;
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError("9: " + Environment.NewLine + ex.ToString());
                    throw ex;
                }

              
            }
        }
        #endregion

    }
}
