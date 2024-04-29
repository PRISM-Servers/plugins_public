using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("BattlefieldLoadout", "Shady", "0.0.2")]
    class BattlefieldLoadout : RustPlugin
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
        class ItemInfo
        {

            public int amount = 1;
            [JsonProperty(PropertyName = "sName", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string shortName = string.Empty;
            [JsonProperty(PropertyName = "cont", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string containerName = string.Empty;

            public Dictionary<string, int> mods = new Dictionary<string, int>();

            public string ammoName = string.Empty;
            public int ammoAmount = 0;
            public ulong skinID = 0;
            public int itemPos = -1;

            public ItemInfo(string shortname, int itemAmount = 1, ulong skin = 0)
            {
                shortName = shortname;
                amount = itemAmount;
                skinID = skin;
            }

            public ItemDefinition GetDefinition() => ItemManager.FindItemDefinition(shortName);

            public Item GetItem() => ItemManager.Create(GetDefinition(), amount, skinID);

            public List<ItemDefinition> GetModDefinitions()
            {
                var defs = new List<ItemDefinition>();
                foreach (var kvp in mods)
                {
                    var def = ItemManager.FindItemDefinition(kvp.Key);
                    if (def != null) defs.Add(def);
                }
                return defs;
            }

            public List<Item> GetModItems()
            {
                var defs = GetModDefinitions();
                var items = new List<Item>();
                var amount = 1;
                for (int i = 0; i < defs.Count; i++)
                {
                    var def = defs[i];
                    foreach(var kvp in mods)
                    {
                        if (kvp.Key == def.shortname)
                        {
                            amount = kvp.Value;
                            break;
                        }
                    }
                    if (amount < 1) amount = 1;
                    var newItem = ItemManager.Create(def, amount, 0);
                    if (newItem != null) items.Add(newItem);
                }
                return items;
            }

        }

        class ItemPosition
        {
            [JsonProperty(PropertyName = "cont", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string containerName = string.Empty;
            [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int itemID = 0;
            [JsonIgnore]
            public string shortName { get { return ItemManager.FindItemDefinition(itemID)?.shortname ?? string.Empty; } }
            [JsonProperty(PropertyName = "pos", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int position = -1;

            public ItemPosition() { }

            public ItemPosition(int Id, string containername, int pos = -1)
            {
                itemID = Id;
                containerName = containername;
                position = pos;
            }
        }


        class StoredData
        {
            public List<ItemInfo> DefaultItems = new List<ItemInfo>();
            public StoredData() { }
        }

        StoredData data;
        
        void Init()
        {
            

            data = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("BFLoadout") ?? new StoredData();
        }

        void OnServerInitialized()
        {
            timer.Once(1f, () =>
            {
                if (ConVar.Server.hostname.Contains("#2")) serverType = ServerType.x10;
                else if (ConVar.Server.hostname.Contains("#3")) serverType = ServerType.BF;
                else serverType = ServerType.x3;

                if (serverType != ServerType.BF)
                {
                    PrintWarning(Name + " has been loaded on an unsupported (type: " + serverType + ") server. The plugin will now unload itself.");
                    covalence.Server.Command("o.unload \"" + Name + "\"");
                }
            });
           
        }

        private enum ServerType
        {
            None,
            x3,
            x10,
            BF
        }

        private ServerType serverType = ServerType.None;

        void ClearInventory(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || player.IsDead()) return;

            var items = Facepunch.Pool.GetList<Item>();
            try 
            {
                player.inventory.AllItemsNoAlloc(ref items);
                for (int i = 0; i < items.Count; i++) RemoveFromWorld(items[i]);
            }
            finally { Facepunch.Pool.FreeList(ref items); }
         
            
            player.SetPlayerFlag(BasePlayer.PlayerFlags.DisplaySash, false);
            player.SendNetworkUpdate();
        }

        void GiveLoadout(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || player.IsDead()) return;
            var defItems = data?.DefaultItems ?? null;
            if (defItems == null || defItems.Count < 1)
            {
                PrintWarning("no default items!");
                return;
            }
            var items = GetItemList(defItems);
            if (items == null || items.Count < 1)
            {
                PrintWarning("no Item default items!");
                return;
            }
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;
                ItemInfo itemInf = null;
                for (int j = 0; j < defItems.Count; j++)
                {
                    var info = defItems[j];
                    if (info != null && info.shortName.Equals(item.info.shortname, StringComparison.OrdinalIgnoreCase))
                    {
                        itemInf = info;
                        break;
                    }
                }
                // var itemInf = arena?.GetItemInfo(item) ?? null;
                ItemContainer cont = null;
                var itemPos = -1;
                if (itemInf != null)
                {
                    var contName = itemInf?.containerName ?? string.Empty;
                    cont = (contName == "wear") ? (player?.inventory?.containerWear ?? null) : (contName == "belt" ? (player?.inventory?.containerBelt ?? null) : (player?.inventory?.containerMain ?? null));
                    if (cont == null) PrintWarning("CONT NULL: " + contName);
                    itemPos = itemInf?.itemPos ?? -1;
                    if (cont != null && (itemPos + 1) > cont.capacity)
                    {
                        PrintWarning("Cont capacity lower than item pos: " + cont.capacity + ", item pos: " + itemPos + " (starting at 1: " + (itemPos + 1) + ")");
                        cont.capacity = (itemPos + 1);
                    }
                }
                else PrintWarning("item inf null: " + item.info.shortname);
                if (cont == null)
                {
                    PrintWarning("cont null:" + item.info.shortname);
                    if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt))
                    {
                        PrintWarning("no move!");
                        RemoveFromWorld(item);
                    }
                }
                else
                {
                    //PrintWarning("cont not null: " + item.info.shortname);
                    if (!item.MoveToContainer(cont, itemPos))
                    {
                        PrintWarning("NO move to cont: " + item.info.shortname + ", " + itemPos);
                        RemoveFromWorld(item);
                    }
                }
            }
        }

        void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null) return;
            if (serverType != ServerType.BF) return;
            ClearInventory(player);
            GiveLoadout(player);
        }

        void Unload() => SaveData();

        void OnServerSave() => timer.Once(8f, () => SaveData());

        void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("BFLoadout", data);
        

        static void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        List<Item> GetItemList(List<ItemInfo> infos)
        {
            if (infos == null || infos.Count < 1) return null;
            var newList = new List<Item>();
            for (int i = 0; i < infos.Count; i++)
            {
                var item = infos[i];
                if (item == null || string.IsNullOrEmpty((item?.shortName ?? string.Empty))) continue;
                var newItem = ItemManager.CreateByName(item.shortName, item.amount, item.skinID);
                if (newItem == null) continue;
                var heldProj = newItem?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                if (heldProj != null)
                {
                    if (!string.IsNullOrEmpty((item?.ammoName ?? string.Empty))) heldProj.primaryMagazine.ammoType = ItemManager.FindItemDefinition(item.ammoName) ?? null;
                    heldProj.primaryMagazine.contents = item?.ammoAmount ?? 0;
                }
                if (item.mods != null && item.mods.Count > 0 && newItem?.contents != null)
                {
                    var modItems = item?.GetModItems() ?? null;
                    if (modItems != null && modItems.Count > 0)
                    {
                        for (int j = 0; j < modItems.Count; j++)
                        {
                            var modItem = modItems[j];
                            if (!modItem.MoveToContainer(newItem.contents)) RemoveFromWorld(modItem);
                        }
                    }
                }
                newList.Add(newItem);
            }
            return newList;
        }


        [ChatCommand("bf_edit")]
        private void cmdBFEdit(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args?.Length < 1) return;
            if (args[0].Equals("items", StringComparison.OrdinalIgnoreCase))
            {
                var itemList = player?.inventory?.AllItems()?.ToList() ?? null;
                var itemInfos = new List<ItemInfo>();
                if (itemList != null && itemList.Count > 0)
                {
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var item = itemList[i];
                        if (item == null) continue;
                        var newInfo = new ItemInfo(item.info.shortname, (item?.amount ?? 1), (item?.skin ?? 0));
                        if (item.parent == player.inventory.containerWear) newInfo.containerName = "wear";
                        if (item.parent == player.inventory.containerMain) newInfo.containerName = "main";
                        if (item.parent == player.inventory.containerBelt) newInfo.containerName = "belt";
                        newInfo.itemPos = item?.position ?? -1;
                        var heldEnt = item?.GetHeldEntity() ?? null;
                        var baseProj = item?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                        newInfo.ammoName = baseProj?.primaryMagazine?.ammoType?.shortname ?? string.Empty;
                        newInfo.ammoAmount = baseProj?.primaryMagazine?.contents ?? 0;
                        if (heldEnt != null && heldEnt is Chainsaw)
                        {
                            newInfo.ammoAmount = (heldEnt as Chainsaw)?.ammo ?? 0;
                            newInfo.ammoName = "lowgradefuel";
                        }
                        var contents = item?.contents?.itemList ?? null;
                        if (contents != null && contents.Count > 0)
                        {
                            for (int j = 0; j < contents.Count; j++)
                            {
                                var content = contents[j];
                                if (content == null) continue;
                                newInfo.mods[content.info.shortname] = content.amount;
                            }
                        }
                        itemInfos.Add(newInfo);
                    }
                }
                
                data.DefaultItems = itemInfos;
                SendReply(player, "Set BF items to your inventory.");
            }
            if (args[0].Equals("gitems"))
            {
                var infos = data?.DefaultItems ?? null;
                if (infos == null || infos.Count < 1)
                {
                    SendReply(player, "No infos!");
                    return;
                }
                var itemList = GetItemList(infos);
                if (itemList == null || itemList.Count < 1)
                {
                    SendReply(player, "No items!");
                    return;
                }

                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList[i];
                    if (item == null) continue;
                    ItemInfo itemInf = null;
                    for(int j = 0; j < infos.Count; j++)
                    {
                        var info = infos[j];
                        if (info != null && info.shortName.Equals(item.info.shortname, StringComparison.OrdinalIgnoreCase))
                        {
                            itemInf = info;
                            break;
                        }
                    }
                   // var itemInf = arena?.GetItemInfo(item) ?? null;
                    ItemContainer cont = null;
                    var itemPos = -1;
                    if (itemInf != null)
                    {
                        var contName = itemInf?.containerName ?? string.Empty;
                        cont = (contName == "wear") ? (player?.inventory?.containerWear ?? null) : (contName == "belt" ? (player?.inventory?.containerBelt ?? null) : (player?.inventory?.containerMain ?? null));
                        if (cont == null) PrintWarning("CONT NULL: " + contName);
                        itemPos = itemInf?.itemPos ?? -1;
                        if (cont != null && (itemPos + 1) > cont.capacity)
                        {
                            PrintWarning("Cont capacity lower than item pos: " + cont.capacity + ", item pos: " + itemPos + " (starting at 1: " + (itemPos + 1) + ")");
                            cont.capacity = (itemPos + 1);
                        }
                    }
                    else PrintWarning("item inf null: " + item.info.shortname);
                    if (cont == null)
                    {
                        PrintWarning("cont null:" + item.info.shortname);
                        if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt))
                        {
                            PrintWarning("no move!");
                            RemoveFromWorld(item);
                        }
                    }
                    else
                    {
                        //PrintWarning("cont not null: " + item.info.shortname);
                        if (!item.MoveToContainer(cont, itemPos))
                        {
                            PrintWarning("NO move to cont: " + item.info.shortname + ", " + itemPos);
                            RemoveFromWorld(item);
                        }
                    }
                }
                SendReply(player, "Gave BF items!");
            }
        }


        Dictionary<string, Timer> popupTimer = new Dictionary<string, Timer>();
        void ShowPopup(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException();
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException();
            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;
            player.Command("gametip.showgametip", msg);
            Timer endTimer;
            if (popupTimer.TryGetValue(Id, out endTimer)) endTimer.Destroy();
            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });
            popupTimer[Id] = endTimer;
        }
        void ShowPopupAll(string msg, float duration = 5f)
        {
            foreach (var ply in covalence.Players.Connected) ShowPopup(ply.Id, msg, duration);
        }
    }
}