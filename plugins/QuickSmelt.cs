using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;

namespace Oxide.Plugins
{
    [Info("QuickSmelt", "Shady", "1.3.7", ResourceId = 0/*/1067/*/)]
    [Description("Increases the speed of the furnace smelting and oil refineries")]
    internal class QuickSmelt : RustPlugin
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
        //i made my own method instead, for perf
        //private readonly MethodInfo _increaseCookTime = typeof(BaseOven).GetMethod("IncreaseCookTime", BindingFlags.NonPublic | BindingFlags.Instance);

        private const int SULFUR_ORE_ITEM_ID = -1157596551;
        private const int METAL_ORE_ITEM_ID = -4031221;
        private const int HQ_ORE_ITEM_ID = -1982036270;

        private const uint FURNACE_PREFAB_ID = 2931042549;
        private const uint LARGE_FURNACE_PREFAB_ID = 1374462671;
        private const uint REFINERY_PREFAB_ID = 1057236622;
        private const uint REFINERY_STATIC_PREFAB_ID = 919097516;
        private const uint ELECTRIC_FURNACE_PREFAB_ID = 3808299817;

        private bool? _wasEasy;
        private bool IsEasyServer()
        {
            if (_wasEasy.HasValue) return (bool)_wasEasy;
            var val = ConVar.Server.hostname.Contains("#2") || ConVar.Server.hostname.Contains("#3");
            _wasEasy = val;
            return val;
        }

        private enum ServerType
        {
            None,
            x3,
            x10,
            BF
        }

        private ServerType serverType = ServerType.None;

        #region Initialization

        private const string permAllow = "quicksmelt.allow";
        private float byproductModifier;
        private int byproductPercent;
        private float cookedModifier;
        private int cookedPercent;
        private int fuelUsageModifier;
        private int fuelUsageModifierLarge;
        private bool overcookMeat;
        private bool usePermissions;

        protected override void LoadDefaultConfig()
        {
            // Default is *roughly* x2 production rate
            Config["ByproductModifier"] = byproductModifier = GetConfig("ByproductModifier", 1f);
            Config["ByproductPercent"] = byproductPercent = GetConfig("ByproductPercent", 50);
            Config["FuelUsageModifier"] = fuelUsageModifier = GetConfig("FuelUsageModifier", 1);
            Config["FuelUsageModifierLarge"] = fuelUsageModifierLarge = GetConfig("FuelUsageModifierLarge", 1);
            Config["CookedModifier"] = cookedModifier = GetConfig("CookedModifier", 1f);
            Config["CookedPercent"] = cookedPercent = GetConfig("CookedPercent", 100);
            Config["OvercookMeat"] = overcookMeat = GetConfig("OvercookMeat", false);
            Config["UsePermissions"] = usePermissions = GetConfig("UsePermissions", false);

            // Remove old config entries
            Config.Remove("ChancePerConsumption");
            Config.Remove("CharcoalChance");
            Config.Remove("CharcoalChanceModifier");
            Config.Remove("CharcoalProductionModifier");
            Config.Remove("DontOvercookMeat");
            Config.Remove("ProductionModifier");

            SaveConfig();
        }

        private void Init()
        {
            LoadDefaultConfig();
            permission.RegisterPermission(permAllow, this);
        }

        private void OnServerInitialized()
        {
            if (ConVar.Server.hostname.Contains("#2")) serverType = ServerType.x10;
            else if (ConVar.Server.hostname.Contains("#3")) serverType = ServerType.BF;
            else serverType = ServerType.x3;

            // Reset fuel consumption and byproduct amount - fix for previous versions
            /*/
            var wood = ItemManager.FindItemDefinition("wood");
            var burnable = wood?.GetComponent<ItemModBurnable>();
            if (burnable != null)
            {
                burnable.byproductAmount = 1;
                burnable.byproductChance = 0.5f;
            }/*/

            // Check if meat should be overcooked
            if (overcookMeat) return;

            // Loop through item definitions
            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                if (!item.shortname.Contains(".cooked")) continue;
                var cookable = item?.GetComponent<ItemModCookable>() ?? null;
                if (cookable != null) cookable.highTemp = 150;
            }
        }

        private void Unload()
        {
            // Loop through item definitions
            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                if (!item.shortname.Contains(".cooked")) continue;
                var cookable = item?.GetComponent<ItemModCookable>() ?? null;
                if (cookable != null) cookable.highTemp = 250;
            }
        }

        #endregion

        #region Smelting Magic

        private void OnOvenCook(ElectricOven oven, Item burnable) //change to use ElectricOven
        {
            if (oven == null || oven.prefabID != ELECTRIC_FURNACE_PREFAB_ID) return;

            IncreaseCookTime(oven, 25f * oven.GetSmeltingSpeed()); //easier than ever to make these little shits work faster
        }

        private void OnFuelConsume(BaseOven oven, Item fuel, ItemModBurnable burnable) //doesn't work for electric furnaces lol
        {
            // Check if furnance is usable
            if (oven == null || fuel == null || burnable == null) return;

            if (oven.prefabID != FURNACE_PREFAB_ID && oven.prefabID != LARGE_FURNACE_PREFAB_ID && oven.prefabID != REFINERY_PREFAB_ID && oven.prefabID != REFINERY_STATIC_PREFAB_ID) return;

            var isRefinery = oven.prefabID == REFINERY_PREFAB_ID || oven.prefabID == REFINERY_STATIC_PREFAB_ID;

            // Modify the amount of fuel to use

            var modAmount = (oven.prefabID == LARGE_FURNACE_PREFAB_ID ? fuelUsageModifierLarge : fuelUsageModifier) - 1; //minus one is cause the original amount is only 1 i guess? that's wild bro who knew

            if (fuel.amount <= modAmount) RemoveFromWorld(fuel);
            else fuel.amount -= modAmount;

            // Modify the amount of byproduct to produce
            var isLarge = oven.prefabID == LARGE_FURNACE_PREFAB_ID;

            var useByproductModifier = isLarge ? (byproductModifier * Random.Range(4.45f, 9f)) : (byproductModifier * Random.Range(1.35f, 2.15f));

            var byproductHookVal = Interface.Oxide.CallHook("OnQuickSmeltByproductMultiplier", oven, fuel, burnable, useByproductModifier);
            if (byproductHookVal != null)
                useByproductModifier = Convert.ToSingle(byproductHookVal);

            burnable.byproductAmount = 1 * (int)Math.Round(useByproductModifier, MidpointRounding.AwayFromZero);
            burnable.byproductChance = (100 - byproductPercent) / 100f;

            if (oven?.inventory == null || oven?.inventory?.itemList == null || oven?.inventory?.itemList.Count < 1 || oven.inventorySlots < 1) return;


            var consumptionMult = isLarge ? Random.Range(4.85f, 7.75f) : Random.Range(2f, 4.05f);
            if (serverType == ServerType.BF) consumptionMult *= Random.Range(isLarge ? 2f : 3.1f, isLarge ? 3.4f : 5.25f);

            if (isRefinery) consumptionMult *= 0.33f;

            var hookVal = Interface.Oxide.CallHook("OnQuickSmeltConsumptionSet", oven, fuel, burnable, consumptionMult);
            
            if (hookVal != null) 
                consumptionMult = Convert.ToSingle(hookVal);

            var stacks = Facepunch.Pool.Get<Dictionary<int, int>>();
            try 
            {
                stacks.Clear();

                for (int i = 0; i < oven.inventorySlots; i++)
                {
                    // Check for and ignore invalid items
                    var slotItem = oven.inventory.GetSlot(i);
                    if (slotItem == null) continue;

                    // Check for and ignore non-cookables
                    var cookable = slotItem?.info?.GetComponent<ItemModCookable>() ?? null;
                    if (cookable == null) continue;

                    var itemId = slotItem?.info?.itemid ?? 0;
                    if (!isRefinery && (itemId != METAL_ORE_ITEM_ID && itemId != SULFUR_ORE_ITEM_ID && itemId != HQ_ORE_ITEM_ID)) continue;

                    //  if (!slotItem.info.shortname.Contains("ore")) continue;

                    // Skip already cooked food items
                    //  if (slotItem.info.shortname.EndsWith(".cooked")) continue;


                    // The chance of consumption is going to result in a 1 or 0
                    var consumptionAmount = (int)Math.Ceiling(cookedModifier * (Random.Range(0f, 1f) <= cookedPercent ? 1 : 0));
                    if (consumptionMult != 1.0f) consumptionAmount = (int)Math.Round(consumptionAmount * consumptionMult, MidpointRounding.AwayFromZero);


                    // Check how many are actually in the furnace, before we try removing too many
                    var inFurnaceAmount = slotItem.amount;
                    if (inFurnaceAmount < consumptionAmount) consumptionAmount = inFurnaceAmount;

                    // Set consumption to however many we can pull from this actual stack
                    consumptionAmount = TakeFromInventorySlot(oven.inventory, slotItem.info.itemid, consumptionAmount, i);

                    // If we took nothing, then... we can't create any
                    if (consumptionAmount <= 0) continue;

                    // Create the item(s) that are now cooked
                    var addAmount = cookable.amountOfBecome * consumptionAmount;
                    if (addAmount < 1)
                    {
                        PrintWarning("Got addAmount < 1: " + cookable?.becomeOnCooked?.shortname + " x" + addAmount);
                        continue;
                    }

                    Item findCooked = null;


                    for (int j = 0; j < oven.inventory.itemList.Count; j++)
                    {
                        var item = oven.inventory.itemList[j];
                        if (item?.info != cookable.becomeOnCooked) continue;

                        int stack;
                        if (!stacks.TryGetValue(item.info.itemid, out stack)) stack = stacks[item.info.itemid] = item.MaxStackable();
                        if ((item.amount + addAmount) <= stack)
                        {
                            findCooked = item;
                            break;
                        }
                    }

                    if (findCooked != null)
                    {
                        findCooked.amount += addAmount;
                        findCooked.MarkDirty();
                    }
                    else
                    {
                        var cookedItem = ItemManager.Create(cookable.becomeOnCooked, addAmount);
                        if (cookedItem != null && !cookedItem.MoveToContainer(oven.inventory)) cookedItem.Drop(oven.inventory.dropPosition, oven.inventory.dropVelocity);
                    }

                }
            }
            finally { Facepunch.Pool.Free(ref stacks); }

         
            /*/
        }
        catch(Exception ex)
        {
            if (oven != null && !oven.IsDestroyed) oven.StopCooking();
            PrintError(ex.ToString());
        }/*/
        }

        private int TakeFromInventorySlot(ItemContainer container, int itemId, int amount, int slot)
        {
            if (container == null || itemId == 0 || amount <= 0) return 0;
            var item = container.GetSlot(slot);
            if (item == null || item.info.itemid != itemId) return 0;

            if (item.amount > amount)
            {
                item.amount -= amount;
                item.MarkDirty();
                return amount;
            }

            amount = item.amount;

            RemoveFromWorld(item);
            return amount;
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private void IncreaseCookTime(BaseOven oven, float amount)
        {
            if (oven == null || oven.IsDestroyed) return;

            var c = 0;
            for (int i = 0; i < oven.inventory.itemList.Count; i++)
            {
                if (oven.inventory.itemList[i].HasFlag(global::Item.Flag.Cooking))
                    c++;
            }

            var delta = amount / c;
            for (int i = 0; i < oven.inventory.itemList.Count; i++)
            {
                var item = oven.inventory.itemList[i];
                if (item.HasFlag(global::Item.Flag.Cooking))
                    item.OnCycle(delta);
            }
        }

        #endregion

        private T GetConfig<T>(string name, T value) => Config[name] == null ? value : (T)Convert.ChangeType(Config[name], typeof(T));
    }
}
