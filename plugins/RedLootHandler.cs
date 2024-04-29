using Facepunch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Project Red | Loot Handler", "Shady", "0.0.8")]
    class RedLootHandler : RustPlugin
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
        private DateTime SaveTime { get { return SaveRestore.SaveCreatedTime; } }

        private DateTime LocalSaveTime { get { return SaveRestore.SaveCreatedTime.ToLocalTime(); } }
        private TimeSpan SaveSpan { get { return DateTime.UtcNow - SaveRestore.SaveCreatedTime; } }
        private TimeSpan LocalSaveSpan { get { return DateTime.Now - SaveRestore.SaveCreatedTime.ToLocalTime(); } }

        private static readonly System.Random replaceRnd = new System.Random();

        private const int CAR_PARTS_CRATE_UNDERWATER_PREFAB_ID = 971684836;
        private const int CAR_PARTS_CRATE_PREFAB_ID = 356724277;

        private void ReorderContainer(ItemContainer container)
        {
            if (container == null || container?.itemList == null) return;

            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                for (int i = 0; i < container.itemList.Count; i++)
                {
                    var item = container.itemList[i];
                    item.position = i;
                    item.MarkDirty();
                }

            }
            finally
            {
                try { if (watch.Elapsed.TotalMilliseconds >= 2) PrintWarning(nameof(ReorderContainer) + " took " + watch.Elapsed.TotalMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }

            //is it really as easy as this? ^^
            //it was. it doesn't necessarily keep the order, but this doesn't matter. players won't know the order of a lootcontainer lol
        }

        private void OnLootSpawn(LootContainer container)
        {
            if (container == null)
            {
                PrintWarning("Container called null for OnLootSpawn");
                return;
            }

            var prefabName = container?.ShortPrefabName ?? string.Empty;
            if (prefabName.StartsWith("dm "))
                return;

            var prefabId = container?.prefabID ?? 0;

            var pos = container?.transform?.position ?? Vector3.zero;
            container.Invoke(() =>
            {
                if (container == null || container.IsDestroyed || container.gameObject == null || container.inventory == null)
                {
                    PrintWarning("Container null on timer!!");
                    return;
                }

                var watch = Pool.Get<Stopwatch>();
                try
                {
                    watch.Restart();

                    var lootEnt = container;
                    Item findScrap = null;

                    //    var searchForComponent = UnityEngine.Random.Range(0, 101) <= 60;

                  

                    /*/
                    if (randomComponent != null)
                    {
                        //PrintWarning("found and removing all of or part of: " + randomComponent?.info?.shortname + " x" + randomComponent.amount + " rarity: " + randomComponent?.info?.rarity);

                        if (UnityEngine.Random.Range(0, 101) <= 33 || randomComponent.amount < 2)
                        {
                            RemoveFromWorld(randomComponent);
                          //  PrintWarning("removed all of component");
                        }
                        else
                        {
                          //  PrintWarning("removing amount. original: " + randomComponent.amount);
                            randomComponent.amount = Mathf.Clamp((int)Math.Round(randomComponent.amount / 2d, MidpointRounding.AwayFromZero), 1, randomComponent.amount);
                            randomComponent.MarkDirty();
                         //   PrintWarning("new amount: " + randomComponent.amount);
                        }
                    }/*/

                    if (findScrap != null && findScrap.amount > 1 && UnityEngine.Random.Range(0, 101) <= 66)
                    {
                        findScrap.amount = Mathf.Clamp((int)(findScrap.amount * 0.6f), 1, findScrap.amount); //(int)Mathf.Clamp((int)findScrap.amount / 2), 1, findScrap.amount); // Mathf.Clamp((int)Math.Round(findScrap.amount / 2d, MidpointRounding.AwayFromZero), 1, findScrap.amount);
                        findScrap.MarkDirty();
                    }



                    var isCrateTools = prefabId == 1892026534;

                    var isCarPartsCrate = prefabId == CAR_PARTS_CRATE_PREFAB_ID || prefabId == CAR_PARTS_CRATE_UNDERWATER_PREFAB_ID;
                    if (isCarPartsCrate && UnityEngine.Random.Range(0, 101) <= 10)
                    {
                        Item foundCarPart = null;

                        for (int i = 0; i < lootEnt.inventory.itemList.Count; i++)
                        {
                            var item = lootEnt.inventory.itemList[i];
                            if (item.info.shortname.EndsWith("1"))
                            {
                                foundCarPart = item;
                                break;
                            }

                        }

                        if (foundCarPart == null) PrintWarning("car box spawned with no car parts!!");
                        else
                        {


                            var oldAmount = foundCarPart.amount;
                            var oldCondition = foundCarPart.conditionNormalized;

                            var nameNoNumber = foundCarPart.info.shortname.Replace("1", string.Empty);
                            RemoveFromWorld(foundCarPart);

                            var newQualityNumber = UnityEngine.Random.Range(0, 101) <= 66 ? "2" : "3";

                            var newItem = ItemManager.CreateByName(nameNoNumber + newQualityNumber, oldAmount);
                            newItem.conditionNormalized = oldCondition;

                            if (!newItem.MoveToContainer(lootEnt.inventory))
                            {
                                PrintWarning("failed to move new item!!!: " + newItem);
                                RemoveFromWorld(newItem);
                            }

                        }

                    }


                    var isShipHackCrate = container is HackableLockedCrate && (container?.GetParentEntity()?.prefabID ?? 0) == 3234960997; //cargoship ID

                    var isNonShipHackCrate = container is HackableLockedCrate && !isShipHackCrate;


                    var isHeliCrate = container.prefabID == 1314849795;
                    var isBradleyCrate = container.prefabID == 1737870479;


                    var chanceToSpawnBP = isHeliCrate ? 90 : isBradleyCrate ? 50 : isShipHackCrate ? 20 : isNonShipHackCrate ? 12 : isCrateTools ? (UnityEngine.Random.Range(0, 101) <= 33 ? 1 : -1) : (UnityEngine.Random.Range(0, 101) <= 50 ? 0 : -1); //now 1% - joke ruined //2% (milk) for ANY crate!

                    if (UnityEngine.Random.Range(0, 101) <= chanceToSpawnBP)
                    {
                        var rareRng = UnityEngine.Random.Range(0, 101);

                        var rarity = rareRng <= (isHeliCrate || isBradleyCrate || isShipHackCrate ? 20 : 2) ? Rust.Rarity.VeryRare : rareRng <= (isHeliCrate || isBradleyCrate || isShipHackCrate ? 33 : 16) ? Rust.Rarity.Rare : rareRng <= 66 ? Rust.Rarity.Uncommon : Rust.Rarity.Common;

                        var catRNG = UnityEngine.Random.Range(0, 101);

                        var category = catRNG <= 10 ? ItemCategory.Weapon : catRNG <= 40 ? ItemCategory.Ammunition : catRNG <= 50 ? ItemCategory.Medical : catRNG <= 80 ? ItemCategory.Attire : ItemCategory.Component;

                        //PrintWarning("trying to find BP matching category: " + category + " rarity: " + rarity);

                        var bp = GetRandomBP(rarity, category);

                        if (bp == null)
                        {
                            PrintWarning("BP NULL FOR RARITY/CATEGORY: " + rarity + ", category: " + category);

                            var startRarityAsInt = (int)rarity;
                            if (startRarityAsInt > 0)
                            {
                                var useRarity = startRarityAsInt;
                                while (bp == null && useRarity > 0)
                                {
                                    try { bp = GetRandomBP((Rust.Rarity)useRarity, category); }
                                    finally { useRarity--; }
                                }

                                if (bp != null) PrintWarning("finally got BP with rarity: " + (Rust.Rarity)useRarity + " (" + useRarity + "), " + bp?.blueprintTargetDef?.shortname);
                                else
                                {
                                    PrintWarning("never got BP by downstepping rarity. lets try upstepping");

                                    for (int i = 0; i < (int)Rust.Rarity.VeryRare; i++)
                                    {
                                        if (bp != null) 
                                            break;
                                        bp = GetRandomBP((Rust.Rarity)i, category);
                                    }

                                    PrintWarning("finally got BP with rarity after upstepping: " + bp?.blueprintTargetDef?.rarity + " " + bp?.blueprintTargetDef?.shortname);

                                }

                            }
                            else PrintWarning("startRarity < 1?!!");

                        }

                      


                        if (bp != null && !bp.MoveToContainer(container.inventory))
                        {
                            PrintWarning("couldn't move bp: " + bp?.blueprintTargetDef?.shortname + " to: " + container?.ShortPrefabName);
                            RemoveFromWorld(bp);
                        }
                       // else PrintWarning("moved random bp: " + bp?.blueprintTargetDef?.displayName?.english + " to: " + container?.ShortPrefabName);
                        
                    }

                    if (isHeliCrate)
                    {
                        if (UnityEngine.Random.Range(0, 101) <= 66)
                        {
                            var rifleAmmo = ItemManager.CreateByName("ammo.rifle", UnityEngine.Random.Range(10, 27));
                            if (rifleAmmo != null && !rifleAmmo.MoveToContainer(container.inventory))
                            {
                                PrintWarning("couldn't move rifleAmmo to heli_crate!");
                                RemoveFromWorld(rifleAmmo);
                            }
                        }

                        if (UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(17.5f, 34))
                        {
                            var syringes = ItemManager.CreateByName("syringe.medical", UnityEngine.Random.Range(1, 4));
                            if (syringes != null && !syringes.MoveToContainer(container.inventory))
                            {
                                PrintWarning("couldn't move syringes to heli_crate!");
                                RemoveFromWorld(syringes);
                            }
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 95)
                        {
                            var scrap = ItemManager.CreateByName("scrap", UnityEngine.Random.Range(30, 101));
                            if (scrap != null && !scrap.MoveToContainer(container.inventory))
                            {
                                PrintWarning("couldn't move scrap to heli_crate!");
                                RemoveFromWorld(scrap);
                            }
                        }

                    }





                    /*/
                    //if (IsEasyServer()) return; //maybe adjust later!
                    ReplaceRiflebody(lootEnt, SaveSpan);
                    //     ReplaceHazmat(lootEnt);
                    if (!isEasy || UnityEngine.Random.Range(0, 101) <= 33) ReplaceHQM(lootEnt, SaveSpan);
                    ReplaceC4(lootEnt, SaveSpan);
                    ReplaceHazmat(lootEnt);/*/

                    //  var satchPerc = SaveSpan.TotalHours < ScaleHours(72) ? (isEasy ? 70 : 85f) : SaveSpan.TotalHours < ScaleHours(96) ? (isEasy ? 50.5f : 67.5f) : SaveSpan.TotalHours < ScaleHours(120) ? (isEasy ? 30f : 42.5f) : SaveSpan.TotalHours < ScaleHours(144) ? (isEasy ? 15 : 25f) : (isEasy ? 7 : 14f);


                    var gunPerc = SaveSpan.TotalHours < 72 ? 98f : SaveSpan.TotalHours < 96 ? 94f : SaveSpan.TotalHours < 120 ? 91f : SaveSpan.TotalHours < 144 ? 86f : SaveSpan.TotalHours < 168 ? 83f : SaveSpan.TotalHours < 192 ? 78f : SaveSpan.TotalHours < 216 ? 72 : 60f;

                    var satchPerc = gunPerc * 0.66f;


                    var rocketRep = gunPerc * 1.02f;

                    var replacements = Pool.Get<HashSet<Item>>();
                    try 
                    {
                        replacements.Clear();

                        ReplaceItems("explosive.satchel", CreateItemAmount("grenade.beancan"), container.inventory, satchPerc, ref replacements);
                        ReplaceItems("smg.thompson", CreateItemAmount("scrap", 5), container.inventory, gunPerc - 3, ref replacements);
                        ReplaceItems("pistol.python", CreateItemAmount("scrap", 2), container.inventory, gunPerc - 7, ref replacements);
                        ReplaceItems("smg.mp5", CreateItemAmount("scrap", 8), container.inventory, gunPerc + 2, ref replacements);
                        ReplaceItems("pistol.m92", CreateItemAmount("scrap", 8), container.inventory, gunPerc, ref replacements);
                        ReplaceItems("rifle.lr300", CreateItemAmount("scrap", 20), container.inventory, gunPerc + 10, ref replacements);
                        ReplaceItems("lmg.m249", CreateItemAmount("scrap", 32), container.inventory, gunPerc * 0.85f, ref replacements);
                        ReplaceItems("rifle.m39", CreateItemAmount("scrap", 15), container.inventory, gunPerc, ref replacements);
                        ReplaceItems("multiplegrenadelauncher", CreateItemAmount("scrap", 35), container.inventory, gunPerc + 7f, ref replacements);
                        ReplaceItems("rifle.l96", CreateItemAmount("scrap", 50), container.inventory, gunPerc + 20f, ref replacements);
                        ReplaceItems("rifle.bolt", CreateItemAmount("scrap", 8), container.inventory, gunPerc, ref replacements);
                        ReplaceItems("rifle.ak", CreateItemAmount("scrap", 12), container.inventory, gunPerc + 7, ref replacements);

                        ReplaceItems("icepick.salvaged", CreateItemAmount("stone.pickaxe", 1), container.inventory, 35, ref replacements);
                        ReplaceItems("axe.salvaged", CreateItemAmount("stonehatchet", 1), container.inventory, 28, ref replacements);
                        ReplaceItems("chainsaw", CreateItemAmount("axe.salvaged", 1), container.inventory, 65, ref replacements);
                        ReplaceItems("jackhammer", CreateItemAmount("icepick.salvaged", 1), container.inventory, 82, ref replacements);
                        ReplaceItems("metal.plate.torso", CreateItemAmount("scrap", 20), container.inventory, 85, ref replacements);
                        ReplaceItems("metal.facemask", CreateItemAmount("scrap", 20), container.inventory, 90, ref replacements);

                        ReplaceItems("door.double.hinged.toptier", CreateItemAmount("wall.frame.garagedoor", 1), container.inventory, 92, ref replacements);
                        ReplaceItems("door.hinged.toptier", CreateItemAmount("scrap", 5), container.inventory, 90, ref replacements);
                        ReplaceItems("ammo.rocket.mlrs", CreateItemAmount("scrap", 4), container.inventory, 92, ref replacements);


                        ReplaceItems("rocket.launcher", CreateItemAmount("scrap", UnityEngine.Random.Range(7, 22)), container.inventory, rocketRep, ref replacements);

                        if (container.prefabID == 2857304752)
                        {
                            if (replacements.Count > 0)
                                PrintWarning("replacements: " + string.Join(", ", replacements));
                        }

                        for (int i = 0; i < lootEnt.inventory.itemList.Count; i++)
                        {
                            var item = lootEnt.inventory.itemList[i];

                            if (replacements.Contains(item))
                                continue;

                            if (findScrap == null && item?.info?.itemid == -932201673) findScrap = item;
                            else if (item?.info?.category == ItemCategory.Component || item?.info?.category == ItemCategory.Resources)
                            {



                                var rarity = item.info.rarity;
                                //    PrintWarning("comp spawn!: " + item?.info?.shortname + " x" + item.amount + " rarity: " + rarity);
                                var chanceToNerf = rarity == Rust.Rarity.Common ? 20 : rarity == Rust.Rarity.Uncommon ? 25 : rarity == Rust.Rarity.Rare ? 45 : rarity == Rust.Rarity.VeryRare ? 70 : 18;

                                if (UnityEngine.Random.Range(0, 101) <= chanceToNerf)
                                {

                                    var chanceToDestroy = rarity == Rust.Rarity.Rare ? 25 : rarity == Rust.Rarity.VeryRare ? 40 : 15;

                                    if (UnityEngine.Random.Range(0, 101) <= chanceToDestroy || (item.amount < 2 && UnityEngine.Random.Range(0, 101) <= 40))
                                    {
                                     //   if (container.prefabID == 2857304752) 
                                       //     PrintWarning("Full remove: " + item?.info?.shortname + " x" + item.amount + " " + rarity);

                                        ReplaceItem(item, CreateItemAmount("scrap", UnityEngine.Random.Range(2, 11)), container.inventory, 100f);
                                    }
                                    else
                                    {
                                        if (container.prefabID == 2857304752) 
                                            PrintWarning("Item amount nerf: " + item?.info?.shortname + " x" + item.amount + ", now: " + (item.amount * 0.5) + " " + rarity);

                                        item.amount = (int)Mathf.Clamp(item.amount * 0.5f, 1, item.amount);
                                        item.MarkDirty();
                                    }
                                }

                            }
                            /*/
                            else if (searchForComponent && randomComponent == null)
                            {
                                if (item?.info?.category == ItemCategory.Component && item.info.rarity > Rust.Rarity.Common)
                                {
                                    randomComponent = item;
                                }
                            }/*/

                        }

                    }
                    finally { Pool.Free(ref replacements); }

                    

                

                }
                finally
                {
                    try
                    {
                        if (container?.inventory != null) ReorderContainer(container.inventory);
                    }
                    finally
                    {
                        try
                        {
                            if (watch.ElapsedMilliseconds >= 4)
                            {
                                var sb = Facepunch.Pool.Get<StringBuilder>();
                                try { PrintWarning(sb.Clear().Append(nameof(OnLootSpawn)).Append(" took: ").Append(watch.ElapsedMilliseconds.ToString("0.##")).Append("ms").ToString()); }
                                finally { Facepunch.Pool.Free(ref sb); }
                            }
                        }
                        finally { Facepunch.Pool.Free(ref watch); }
                    }
                   
                }

            }, 0.66f);

        }

        private Item GetRandomBP(Rust.Rarity rarity, ItemCategory category = ItemCategory.Items, bool useNoneAsWildCard = true)
        {
            var item = ItemManager.CreateByItemID(-996920608); //blueprintbase
            item.blueprintTarget = ItemManager.itemList?.Where(p => p != null && (p.rarity == rarity || (rarity == Rust.Rarity.None && useNoneAsWildCard)) && p.category == category && !(p?.Blueprint?.defaultBlueprint ?? false) && (p?.Blueprint?.userCraftable ?? false) && (p?.Blueprint?.isResearchable ?? false))?.ToList()?.GetRandom()?.itemid ?? -1;
            return item.blueprintTarget == -1 ? null : item;
        }

        private ItemAmount CreateItemAmount(string shortName, int amount = 1)
        {
            if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException(nameof(shortName));
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));

            var def = ItemManager.FindItemDefinition(shortName);
            if (def == null) throw new NullReferenceException(nameof(def));


            return new ItemAmount(def, amount)
            {
                amount = amount,
                startAmount = amount
            };
        }

        private Item FindStackableInContainer(ItemContainer container, ItemDefinition itemWantsToStack, int amount)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            Item stackable = null;

            for (int i = 0; i < container.itemList.Count; i++)
            {
                var item = container.itemList[i];
                if (item == null) continue;

                if (item?.info == itemWantsToStack && item.amount + amount <= item.MaxStackable())
                {
                    stackable = item;
                    break;
                }

            }

            return stackable;
        }

        private void ReplaceItems<T>(T itemToReplace, ItemAmount replacement, ItemContainer container, float perc, ref HashSet<Item> list)
        {
            if (itemToReplace == null) throw new ArgumentNullException(nameof(itemToReplace));
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            if (replacement.amount < 1) throw new ArgumentOutOfRangeException(nameof(replacement.amount));
            if (container == null) throw new ArgumentNullException(nameof(container));

            if (perc > 100) perc = 100;
            if (perc <= 0) throw new ArgumentOutOfRangeException(nameof(perc));

            ItemDefinition def = null;
            var replaceId = -1;
            var replaceStr = string.Empty;

            if (itemToReplace is int) replaceId = Convert.ToInt32(itemToReplace);
            else if ((def = itemToReplace as ItemDefinition) == null)
            {
                replaceStr = itemToReplace?.ToString();
                if (string.IsNullOrEmpty(replaceStr))
                {
                    PrintWarning(nameof(ReplaceItem) + " was called with a type that is not int, def, or string!!!");
                    return;
                }
            }


            for (int i = 0; i < container.itemList.Count; i++)
            {
                var item = container.itemList[i];

                if (item?.info == null)
                    continue;

                var rng = replaceRnd.Next(0, 101);


                if (rng > perc) //rng not met. why even bother checking items? I ASSUME checking this before seeing if we found our item is more performant even if it is ran more often
                    continue;


                if (def != null && item.info != def)
                    continue;

                if (replaceId != -1 && item.info.itemid != replaceId)
                    continue;


                if (def == null && replaceId == -1 && (!replaceStr.Equals(item.info.shortname, StringComparison.OrdinalIgnoreCase) && !replaceStr.Equals(item.info.displayName.english, StringComparison.OrdinalIgnoreCase)))
                    continue;


                var oldPos = item?.position ?? -1;

                RemoveFromWorld(item);

                var intAmount = (int)Math.Round(replacement.amount, MidpointRounding.AwayFromZero);

                var foundStackable = FindStackableInContainer(container, replacement.itemDef, intAmount);

                Item replacementItem = null;

                if (foundStackable != null)
                {
                    foundStackable.amount += intAmount;
                    foundStackable.MarkDirty();
                    replacementItem = foundStackable;
                }
                else
                {
                    var replaceItem = ItemManager.Create(replacement.itemDef, (int)Math.Round(replacement.amount, MidpointRounding.AwayFromZero));
                    if (replaceItem == null)
                    {
                        PrintWarning("replaceItem somehow null!!");
                        continue;
                    }

                    if (!replaceItem.MoveToContainer(container, oldPos) && !replaceItem.MoveToContainer(container))
                    {
                        PrintWarning("Failed to move replaceItem!: " + replaceItem);
                        RemoveFromWorld(replaceItem);
                    }

                    replacementItem = replaceItem;

                }

                if (replacementItem != null && list != null)
                    list.Add(replacementItem);

            }

        }

        /// <summary>
        /// Replaces an item in a container with your desired replacement. Returns the newly created item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemToReplace"></param>
        /// <param name="replacement"></param>
        /// <param name="container"></param>
        /// <param name="perc"></param>
        /// <param name="removeMultiple"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private Item ReplaceItem<T>(T itemToReplace, ItemAmount replacement, ItemContainer container, float perc, bool removeMultiple = false)
        {
            if (itemToReplace == null) throw new ArgumentNullException(nameof(itemToReplace));
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            if (replacement.amount < 1) throw new ArgumentOutOfRangeException(nameof(replacement.amount));
            if (container == null) throw new ArgumentNullException(nameof(container));

            if (perc > 100) perc = 100;
            if (perc <= 0) throw new ArgumentOutOfRangeException(nameof(perc));

            ItemDefinition def = null;
            var replaceId = -1;
            var replaceStr = string.Empty;

            if (itemToReplace is int) replaceId = Convert.ToInt32(itemToReplace);
            else if ((def = itemToReplace as ItemDefinition) == null)
            {
                replaceStr = itemToReplace?.ToString();
                if (string.IsNullOrEmpty(replaceStr))
                {
                    PrintWarning(nameof(ReplaceItem) + " was called with a type that is not int, def, or string!!!");
                    throw new ArgumentException(nameof(itemToReplace));
                }
            }


            for (int i = 0; i < container.itemList.Count; i++)
            {
                var item = container.itemList[i];

                if (item?.info == null)
                    continue;

                var rng = replaceRnd.Next(0, 101);


                if (rng > perc) //rng not met. why even bother checking items? I ASSUME checking this before seeing if we found our item is more performant even if it is ran more often
                    continue;


                if (def != null && item.info != def)
                    continue;

                if (replaceId != -1 && item.info.itemid != replaceId)
                    continue;


                if (def == null && replaceId == -1 && (!replaceStr.Equals(item.info.shortname, StringComparison.OrdinalIgnoreCase) && !replaceStr.Equals(item.info.displayName.english, StringComparison.OrdinalIgnoreCase)))
                    continue;


                var oldPos = item?.position ?? -1;

                RemoveFromWorld(item);

                var intAmount = (int)Math.Round(replacement.amount, MidpointRounding.AwayFromZero);

                var foundStackable = FindStackableInContainer(container, replacement.itemDef, intAmount);

                Item replacementItem = null;

                if (foundStackable != null)
                {
                    foundStackable.amount += intAmount;
                    foundStackable.MarkDirty();
                    replacementItem = foundStackable;
                }
                else
                {
                    var replaceItem = ItemManager.Create(replacement.itemDef, (int)Math.Round(replacement.amount, MidpointRounding.AwayFromZero));
                    if (replaceItem == null)
                    {
                        PrintWarning("replaceItem somehow null!!");
                        continue;
                    }

                    if (!replaceItem.MoveToContainer(container, oldPos) && !replaceItem.MoveToContainer(container))
                    {
                        PrintWarning("Failed to move replaceItem!: " + replaceItem);
                        RemoveFromWorld(replaceItem);
                    }

                    replacementItem = replaceItem;

                }

                return replacementItem; //remove only one item! use ReplaceItems for multiple.
            }

            return null;

        }



        private void RemoveFromWorld(Item item, bool reorderParent = false)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var parent = item?.parent;

            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();

            if (reorderParent && parent != null)
                ReorderContainer(parent);


        }

    }
}
