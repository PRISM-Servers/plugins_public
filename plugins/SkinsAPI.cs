using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Diagnostics;

namespace Oxide.Plugins
{
    [Info("SkinsAPI", "Shady", "1.1.1", ResourceId = 0000)]
    internal class SkinsAPI : RustPlugin
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
        private readonly Dictionary<int, Dictionary<int, ulong>> itemSkins = new Dictionary<int, Dictionary<int, ulong>>();
        private readonly Dictionary<string, ulong> _nameToSkinID = new Dictionary<string, ulong>();
        private readonly List<Rust.Workshop.ApprovedSkinInfo> workshopSkins = new List<Rust.Workshop.ApprovedSkinInfo>();
        private readonly HashSet<ItemDefinition> ItemsWithSkins = new HashSet<ItemDefinition>();
        private readonly Dictionary<string, string> shortToEng = new Dictionary<string, string>();


        private readonly System.Random _rngSkins = new System.Random();

        private readonly Dictionary<string, string> _skinIDToName = new Dictionary<string, string>();


        private void OnServerInitialized()
        {
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try 
            {
                watch.Restart();

                foreach (var kvp in Rust.Workshop.Approved.All)
                    workshopSkins.Add(kvp.Value);


                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];

                    var skins = ItemSkinDirectory.ForItem(item);

                    if (skins.Length > 0) ItemsWithSkins.Add(item);

                    for (int j = 0; j < skins.Length; j++)
                    {
                        var skin = skins[j];
                        var idStr = skin.id.ToString();
                        var name = GetSkinNameFromID(idStr);
                        _nameToSkinID[name] = Convert.ToUInt64(idStr);
                        _skinIDToName[idStr] = name;
                    }

                    shortToEng[item.shortname] = item.displayName.english;
                }

                for (int i = 0; i < workshopSkins.Count; i++)
                {
                    var skin = workshopSkins[i];
                    _nameToSkinID[skin.Name] = skin.WorkshopdId;
                    _skinIDToName[skin.WorkshopdId.ToString()] = skin.Name;

                    var def = ItemManager.FindItemDefinition((skin.Skinnable.ItemName == "lr300.item") ? "rifle.lr300" : skin.Skinnable.ItemName);
                    if (def == null)
                    {
                        PrintWarning("Def is null for: " + skin.Skinnable.ItemName);
                        continue;
                    }

                    ItemsWithSkins.Add(def);
                }


                timer.Once(2f, () =>
                {
                    Puts("Filling item list!");
                    FillItemList();
                });
              
            }
            finally
            {
                try { PrintWarning("OnServerInitialized() took: " + watch.Elapsed.TotalMilliseconds + "ms"); }
                finally { Facepunch.Pool.Free(ref watch); }
            }
        }

        private void FillItemList()
        {
            foreach (var item in ItemsWithSkins)
            {
                Dictionary<int, ulong> skinsDirTemp;
                if (!itemSkins.TryGetValue(item.itemid, out skinsDirTemp)) itemSkins[item.itemid] = new Dictionary<int, ulong>();
                FillSkinList(item);
            }
        }

        #region Exposed API Calls

        private ulong GetRandomSkinID(string itemidStr)
        {
            int itemid;
            if (!int.TryParse(itemidStr, out itemid)) return 0ul;
            var outDict = new Dictionary<int, ulong>();
            if (!itemSkins.TryGetValue(itemid, out outDict)) return 0ul;
            var rng = _rngSkins.Next(0, itemSkins[itemid].Count);
            ulong nextSkinID;
            if (itemSkins[itemid].TryGetValue(rng, out nextSkinID)) return nextSkinID;
            else return 0ul;
        }

        private ulong GetRandomSkin()
        {
            var rngItem = ItemManager.itemList?.Where(p => HasSkins(p))?.ToList()?.GetRandom() ?? null;
            if (rngItem == null) return 0;
            return GetSkinsForItem(rngItem.itemid)?.GetRandom() ?? 0;
        }

        private ulong GetRandomSkin(params ulong[] ignore)
        {
            var rngItem = ItemManager.itemList?.Where(p => HasSkins(p) && (GetSkinsForItem(p.itemid)?.Any(x => !ignore.Contains(x)) ?? false))?.ToList()?.GetRandom() ?? null;
            if (rngItem == null) return 0;
            return GetSkinsForItem(rngItem.itemid)?.Where(p => !ignore.Contains(p))?.ToList()?.GetRandom() ?? 0;
        }

        private ItemDefinition GetItemDefinitionForRandomSkinExcluding(IEnumerable<ulong> exclusions)
        {
            if (exclusions == null) throw new ArgumentNullException(nameof(exclusions));

            var allItemsCount = ItemsWithSkins.Count;

            var indexToUse = UnityEngine.Random.Range(0, allItemsCount);

            var count = 0;

            ItemDefinition rngItem = null;

            var hasUsableSkin = false;

            foreach (var item in ItemsWithSkins)
            {
                if (count == indexToUse)
                {

                    var allSkins = GetSkinsForItem(item.itemid);
                    for (int i = 0; i < allSkins.Count; i++)
                    {
                        var skin = allSkins[i];
                        if (!exclusions.Contains(skin))
                        {
                            hasUsableSkin = true;
                            break;
                        }
                    }

                    if (hasUsableSkin) rngItem = item;


                    break;
                }

                count++;
            }

            return rngItem;
        }

        private ulong GetRandomSkinExcluding(IEnumerable<ulong> exclusions)
        {
            if (exclusions == null) throw new ArgumentNullException(nameof(exclusions));

            var def = GetItemDefinitionForRandomSkinExcluding(exclusions);
            if (def == null)
            {
                PrintWarning("initial def was null. trying up to 100 times for another random one");
                for (int i = 0; i < 100; i++)
                {
                    def = GetItemDefinitionForRandomSkinExcluding(exclusions);
                    if (def != null)
                    {
                        PrintWarning("found non-null def after " + i + " tries");
                        break;
                    }
                }
            }

            var allSkins = GetSkinsForItem(def.itemid);
            return allSkins[UnityEngine.Random.Range(0, allSkins.Count)];
        }

        private List<ulong> GetSkinsForItem(int itemId)
        {


            Dictionary<int, ulong> dictionaryForItem = null;

            foreach (var kvp in itemSkins)
            {
                if (kvp.Key == itemId)
                {
                    dictionaryForItem = kvp.Value;
                    break;
                }
            }

            if (dictionaryForItem == null)
                return null;

            var list = new List<ulong>();

            foreach (var kvp in dictionaryForItem)
            {
                list.Add(kvp.Value);
            }

            return list;
        }

        private void GetSkinsForItemNoAlloc(int itemId, ref List<ulong> list)
        {
            list.Clear();

            Dictionary<int, ulong> dictionaryForItem = null;

            foreach (var kvp in itemSkins)
            {
                if (kvp.Key == itemId)
                {
                    dictionaryForItem = kvp.Value;
                    break;
                }
            }

            if (dictionaryForItem == null)
                return;


            foreach (var kvp in dictionaryForItem)
            {
                list.Add(kvp.Value);
            }

            return;
        }

        private int GetItemIDFromSkin(ulong skin)
        {
            if (skin == 0) return 0;

            var list = Facepunch.Pool.GetList<ulong>();

            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];

                    GetSkinsForItemNoAlloc(item.itemid, ref list);

                    if (list.Contains(skin))
                        return item.itemid;
                    
                }
            }
            finally { Facepunch.Pool.Free(ref list); }

            return 0;
        }

        private ulong FindSkinID(string skinName)
        {
            if (string.IsNullOrEmpty(skinName)) return 0ul;

            ulong outID;
            if (_nameToSkinID.TryGetValue(skinName, out outID)) return outID;
            return 0ul;
        }

        private Dictionary<int, Dictionary<int, ulong>> GetSkins() { return itemSkins; }

        private List<Rust.Workshop.ApprovedSkinInfo> GetWorkshopSkins() { return workshopSkins; }

        private bool HasSkins(ItemDefinition item)
        {
            return ItemsWithSkins.Contains(item);
        }

        private string GetWorkshopSkinName(string skinID)
        {
            if (string.IsNullOrEmpty(skinID)) throw new ArgumentNullException(nameof(skinID));

            for (int i = 0; i < workshopSkins.Count; i++)
            {
                var skin = workshopSkins[i];

                var WorkshopID = skin?.WorkshopdId.ToString() ?? string.Empty;
                var invID = skin?.InventoryId.ToString() ?? string.Empty;

                if (WorkshopID == skinID || invID == skinID) return skin?.Name ?? string.Empty;
            }

            return string.Empty;
        }

        private string GetSkinName(Item item)
        {
            if (item == null || item.info == null) return string.Empty;

            var skinID = item?.skin ?? 0;

            var skinName = GetWorkshopSkinName(skinID.ToString());

            if (!string.IsNullOrEmpty(skinName)) return skinName;



            var directoryInstance = ItemSkinDirectory.Instance;

            var skinIDStr = skinID.ToString();
            int skinIDI;

            for (int i = 0; i < directoryInstance.skins.Length; i++)
            {
                var skin = directoryInstance.skins[i];
                if (int.TryParse(skinIDStr, out skinIDI) && skin.id == skinIDI) return skin.invItem?.displayName?.english ?? string.Empty;
            }

            return skinName;
        }

        private string GetSkinNameFromID(string ID)
        {
            if (string.IsNullOrEmpty(ID)) throw new ArgumentNullException(nameof(ID));

            string skinName;
            if (_skinIDToName.TryGetValue(ID, out skinName)) return skinName;

            skinName = GetWorkshopSkinName(ID);
            if (!string.IsNullOrEmpty(skinName)) return skinName;

            int skinIDI;
            if (int.TryParse(ID, out skinIDI))
            {
                var directoryInstance = ItemSkinDirectory.Instance;


                for (int i = 0; i < directoryInstance.skins.Length; i++)
                {
                    var skin = directoryInstance.skins[i];
                    if (skin.id == skinIDI) return skin.invItem?.displayName?.english ?? string.Empty;
                }
            }

            return skinName;
        }

        private ulong GetSkinIDFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));


            for (int i = 0; i < workshopSkins.Count; i++)
            {
                var info = workshopSkins[i];

                if (info.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return info.WorkshopdId;
            }

            for (int i = 0; i < ItemSkinDirectory.Instance.skins.Length; i++)
            {
                var globalSkin = ItemSkinDirectory.Instance.skins[i];
                if ((globalSkin.invItem?.displayName?.english ?? string.Empty).Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return (ulong)globalSkin.id;
                }
            }

            return 0;
        }


        #endregion


        private void FillSkinList(ItemDefinition item)
        {
            if (item == null) 
                throw new ArgumentNullException(nameof(item));

            Dictionary<int, ulong> outDict;
            if (!itemSkins.TryGetValue(item.itemid, out outDict)) return;

            int i;
            itemSkins[item.itemid][0] = 0;

            var skins = ItemSkinDirectory.ForItem(item);


            foreach (var skin in workshopSkins)
            {
                var ID = skin?.WorkshopdId ?? 0;
                if ((skin.Skinnable.ItemName == "lr300.item" && item.shortname == "rifle.lr300") || skin.Skinnable.ItemName.Equals(item.shortname, StringComparison.OrdinalIgnoreCase))
                {
                    i = itemSkins[item.itemid].Keys.Max() + 1;
                    itemSkins[item.itemid][i] = ID;
                }
            }

            if (skins != null && skins.Length > 0)
            {
                for (int j = 0; j < skins.Length; j++)
                {
                    var entry = skins[j];
                    ulong skinUL;
                    if (!ulong.TryParse(entry.id.ToString(), out skinUL)) continue;
                    i = itemSkins[item.itemid].Keys.Max() + 1;
                    itemSkins[item.itemid][i] = skinUL;
                }
            }
        }
    }
}