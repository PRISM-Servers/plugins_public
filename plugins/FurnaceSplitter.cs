using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("Furnace Splitter", "Skipcast (edited by Shady & MBR)", "2.2.2", ResourceId = 2406)]
    [Description("Splits up resources in furnaces automatically and shows useful furnace information")]
    public class FurnaceSplitter : RustPlugin
    {
        private class OvenSlot
        {
            /// <summary>The item in this slot. May be null.</summary>
            public Item Item;

            /// <summary>The slot position</summary>
            public int? Position;

            /// <summary>The slot's index in the itemList list.</summary>
            public int Index;

            /// <summary>How much should be added/removed from stack</summary>
            public int DeltaAmount;
        }

        public enum MoveResult
        {
            Ok,
            SlotsFilled,
            NotEnoughSlots
        }

        public class StoredData 
        {
            public List<string> Disabled = new List<string>();
            public Dictionary<string, double> WoodPercentages = new Dictionary<string, double>();
        }
        
        StoredData data;

        private Dictionary<string, object> _stackOptions = new Dictionary<string, object>(); //workaround for config issue
        private Dictionary<string, int> StackOptions = new Dictionary<string, int>();

        //These are the default values used by the config if no values have been set yet.
        private Dictionary<string, object> StackOptionsDefault = new Dictionary<string, object>
            {
                {"furnace", 3},
                {"bbq.deployed", 9},
                {"campfire", 2},
                {"fireplace.deployed", 2},
                {"furnace.large", 15},
                {"hobobarrel_static", 2},
                {"refinery_small_deployed", 3},
                {"skull_fire_pit", 2}
            };

        private int AddWoodAmount;

        private string ToggleCommandName;
        private string RatioCommandName;

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        private const int WOOD_ITEM_ID = -151838493;

        private const string permUse = "furnacesplitter.use";

        private Dictionary<string, double> ratios = new Dictionary<string, double>
        {
            { "1/1", 1 },
            { "1/2", 0.5 },
            { "1/3", 1d / 3 },
            { "2/3", 2d / 3 },
            { "1/4", 0.25 },
            { "3/4", 0.75 },
            { "1/5", 0.2 },
            { "2/5", 0.4 },
            { "3/5", 0.6 },
            { "4/5", 0.8 }
        };

        private Dictionary<double, string> rRatios; //set in init
        private string ratiosStr; //set in init

        private readonly string[] compatibleOvens =
        {
            "bbq.deployed",
            "campfire",
            "fireplace.deployed",
            "furnace",
            "furnace.large",
            "hobobarrel_static",
            "refinery_small_deployed",
            "skull_fire_pit",
            "electricfurnace.deployed"
        };
        

        private void OnServerInitialized()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                // English
                {"enabled", "Enabled auto sorting." },
                {"disabled", "Disabled auto sorting." },
                { "nopermission", "You don't have permission to use this." }
            }, this);
        }

        protected override void LoadDefaultConfig()
        {
            Config["Slots to fill"] = _stackOptions = GetConfig("Slots to fill", StackOptionsDefault);
            Config["Amount of wood to add"] = AddWoodAmount = GetConfig("Amount of wood to add", 2000);
            Config["Toggle command name"] = ToggleCommandName = GetConfig("Toggle command name", "autosort");
            Config["Ratio command name"] = RatioCommandName = GetConfig("Ratio command name", "fuelratio");
            int val;
            foreach(var kvp in _stackOptions)
            {
                if (!int.TryParse(kvp.Value?.ToString(), out val)) continue;
                StackOptions[kvp.Key] = val;
            }
            SaveConfig();
        }

        private void Init()
        {
            rRatios = ratios.ToDictionary(x => x.Value, x => x.Key);

            permission.RegisterPermission(permUse, this);
            LoadDefaultConfig();
            data = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("FurnaceSplitter") ?? new StoredData();
            AddCovalenceCommand(ToggleCommandName, nameof(cmdAutoSortToggle));
            AddCovalenceCommand(RatioCommandName, nameof(cmdRatio));

            //needs to be set here because of cfg
            ratiosStr = "<color=green>0</color>: <color=orange>None</color> (1000 ore => " + AddWoodAmount + " wood)\n" + string.Join("\n", ratios.Keys.Select(x => "<color=green>" + (ratios.Keys.ToList().IndexOf(x) + 1) + "</color>: <color=orange>" + x + "</color> (1000 ore => " + (ratios[x] * 1000).ToString("N2") + " wood)"));
        }

        private void Unload() => Interface.Oxide.DataFileSystem.WriteObject<StoredData>("FurnaceSplitter", data);


        private bool IsSlotCompatible(Item item, BaseOven oven, ItemDefinition itemDefinition)
        {
            ItemModCookable cookable = item.info.GetComponent<ItemModCookable>();

            if (item.amount < item.info.stackable && item.info == itemDefinition)
                return true;

            if (oven.allowByproductCreation && oven.fuelType.GetComponent<ItemModBurnable>().byproductItem == item.info)
                return true;

            if (cookable == null || cookable.becomeOnCooked == itemDefinition)
                return true;

            if (CanCook(cookable, oven))
                return true;

            return false;
        }


        private bool IsEnabled(BasePlayer player)
        {
            if (player == null) return false;

            if (data?.Disabled == null || data.Disabled.Count < 1) return true;

            for(int i = 0; i < data.Disabled.Count; i++) { if (data.Disabled[i] == player.UserIDString) return false; }

            return true;
        }

        void OnItemRemovedFromContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null) return;
            var player = container?.GetOwnerPlayer() ?? item?.GetOwnerPlayer() ?? item?.parent?.GetOwnerPlayer() ?? null;
            if (player == null || !HasPermission(player) || !IsEnabled(player)) return;
            var cookable = item?.info?.GetComponent<ItemModCookable>() ?? null;
            if (cookable == null) return;
            ItemContainer originalContainer = item.GetRootContainer();

            var oldAmount = item.amount;

            NextTick(() =>
            {
                BaseOven oven = item?.parent?.entityOwner as BaseOven;
                if (oven == null || !compatibleOvens.Contains(oven.ShortPrefabName)) return;



                Func<object> splitFunc = () =>
                {
                    if (player == null) return null;


                    if (container == null || container == item.GetRootContainer()) return null;

                    if (oven == null || cookable == null) return null;

                    var totalSlots = oven.inventorySlots;

                    //int totalSlots = 2 + (oven.allowByproductCreation ? 1 : 0);
                   // int otherSlots;
                   // if (StackOptions.TryGetValue(oven.ShortPrefabName, out otherSlots) && otherSlots >= 0) totalSlots = otherSlots;



                    if (cookable.lowTemp > oven.cookingTemperature || cookable.highTemp < oven.cookingTemperature) return null;

                    var allowedSlots = oven?.GetAllowedSlots(item);
                    if (!allowedSlots.HasValue) return false;


                    var inputSlots = oven?.inputSlots;


                    var splitAmount = (int)(item.amount / inputSlots);

                    if (splitAmount < 1)
                        return false;

                    var oldUid = item.uid;


                    for (int min = allowedSlots.Value.Min; min <= allowedSlots.Value.Max; ++min)
                    {
                        Item slot = oven.inventory.GetSlot(min);
                        if (slot != null)
                            continue;

                        var newItem = item.amount <= splitAmount ? item : ItemManager.Create(item.info, splitAmount);
                        if (newItem.uid != oldUid) item.amount -= splitAmount;

                        if (!newItem.MoveToContainer(oven.inventory, min))
                        {

                            if (!newItem.MoveToContainer(oven.inventory))
                            {
                                RemoveFromWorld(newItem);
                                PrintWarning("couldn't move at all!!!");
                            }

                        }
                    }




                    return true;
                };

                object returnValue = splitFunc();

                //this could be made to look nicer, but the code works :^)
                if (oven?.inventory?.itemList != null && returnValue is bool && (bool)returnValue)
                {
                    Item findWood = null;
                    var cookableCount = 0;

                    for (int i = 0; i < oven.inventory.itemList.Count; i++)
                    {
                        var slotItem = oven.inventory.itemList[i];
                        if (slotItem?.info?.itemid == WOOD_ITEM_ID)
                        {
                            findWood = slotItem;
                            break;
                        }
                    }

                    for (int i = 0; i < oven.inventory.itemList.Count; i++)
                    {
                        var slotItem = oven.inventory.itemList[i];
                        var comp = slotItem.info?.GetComponent<ItemModCookable>();
                        if (comp != null && CanCook(comp, oven)) cookableCount += slotItem.amount;
                    }

                    var _amount = -1d;
                    var amount = AddWoodAmount;
                    if (data.WoodPercentages.TryGetValue(player.UserIDString, out _amount)) 
                    {
                        _amount = Math.Ceiling(_amount * cookableCount);
                        //if (oven.prefabID == 1374462671) _amount /= 5;
                        amount = (int)_amount;
                    }

                    if (findWood == null || findWood.amount < amount)
                    {
                        if ((player?.inventory?.GetAmount(WOOD_ITEM_ID) ?? 0) < amount) return;

                        var takeAmount = amount;
                        if (findWood != null)
                        {
                            var diff = amount - findWood.amount;
                            findWood.amount += diff;
                            findWood.MarkDirty();
                            takeAmount = diff;
                        }
                        else
                        {
                            var firstSlot = oven?.inventory?.GetSlot(0) ?? null;
                            var removedFirstSlot = false;
                            if (firstSlot != null && firstSlot.info?.itemid != WOOD_ITEM_ID)
                            {
                                firstSlot.RemoveFromContainer();
                                removedFirstSlot = true;
                            }

                            var newWood = ItemManager.CreateByItemID(WOOD_ITEM_ID, amount);
                            if (!newWood.MoveToContainer(oven.inventory, 0) && !newWood.MoveToContainer(oven.inventory))
                            {
                                //try to move it back, if we fail both - we remove the wood
                                if (!newWood.MoveToContainer(player.inventory.containerMain) && !newWood.MoveToContainer(player.inventory.containerBelt) && !newWood.Drop(player.GetDropPosition(), player.GetDropVelocity()))
                                {
                                    RemoveFromWorld(newWood);
                                }
                            }
                            var lastSlot = GetFirstFreeSlot(oven.inventory);
                            if (removedFirstSlot && !firstSlot.MoveToContainer(oven.inventory, lastSlot) && !firstSlot.MoveToContainer(oven.inventory)) firstSlot.Drop(oven.GetDropPosition(), oven.GetDropVelocity());
                        }
                        player.inventory.Take(null, WOOD_ITEM_ID, takeAmount);

                    }
                }
            });

        }

   

        private int FindMatchingSlotIndex(ItemContainer container, out Item existingItem, ItemDefinition itemType, List<int> indexBlacklist)
        {
            existingItem = null;
            int firstIndex = -1;
            Dictionary<int, Item> existingItems = new Dictionary<int, Item>();

            for (int i = 0; i < container.capacity; ++i)
            {
                if (indexBlacklist.Contains(i))
                    continue;

                Item itemSlot = container.GetSlot(i);
                if (itemSlot == null || itemType != null && itemSlot.info == itemType)
                {
                    if (itemSlot != null)
                        existingItems.Add(i, itemSlot);

                    if (firstIndex == -1)
                    {
                        existingItem = itemSlot;
                        firstIndex = i;
                    }
                }
            }

            if (existingItems.Count <= 0 && firstIndex != -1)
            {
                return firstIndex;
            }
            else if (existingItems.Count > 0)
            {
                var largestStackItem = new KeyValuePair<int, Item>(); //= existingItems.OrderByDescending(kv => kv.Value.amount).First();
                var max = 0;
                
                var last = -1;
                foreach(var p in existingItems)
                {
                    last = p.Value?.amount ?? 0;
                    if (last > max)
                    {
                        max = last;
                        largestStackItem = p;
                    }
                }
                existingItem = largestStackItem.Value;
                return largestStackItem.Key;
            }

            existingItem = null;
            return -1;
        }

        int GetFirstFreeSlot(ItemContainer container)
        {
            if (container == null || container.itemList == null || container.itemList.Count < 1) return -1;
            for (int i = 0; i < container.capacity; i++)
            {
                var getSlot = container?.GetSlot(i) ?? null;
                if (getSlot == null) return i;
            }
            return -1;
        }

        private bool CanCook(ItemModCookable cookable, BaseOven oven) { return oven.cookingTemperature >= cookable.lowTemp && oven.cookingTemperature <= cookable.highTemp; }

        private bool HasPermission(BasePlayer player) { return permission.UserHasPermission(player.UserIDString, permUse); }
        private bool HasPermission(string userID) { return permission.UserHasPermission(userID, permUse); }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        #region Command

        private void cmdRatio(IPlayer player, string command, string[] args)
        {
            if (!HasPermission(player.Id))
            {
                player.Message(lang.GetMessage("nopermission", this, player.Id));
                return;
            }

            var val = -1d;
            data.WoodPercentages.TryGetValue(player.Id, out val);

            var val2 = "";
            if (!rRatios.TryGetValue(val, out val2)) val2 = "None (" + AddWoodAmount + " wood)";

            var msg = "---<color=orange><size=20>Furnace Fuel Ratio</size></color>---\nCurrent ratio: <color=orange>" + val2 + "</color>\nAvailable\n------------------------\n" + ratiosStr + "\nUsage: /" + command + " 0 - " + ratios.Keys.Count;

            int num = -1;
            if (args.Length < 1 || !int.TryParse(args[0], out num) || num < 0 || num > ratios.Keys.Count)
            {
                player.Reply(msg);
                return;
            }

            if (num == 0)
            {
                if (data.WoodPercentages.TryGetValue(player.Id, out val))
                {
                    data.WoodPercentages.Remove(player.Id);
                }
                val = -1d;
            }
            else
            {
                num--;
                data.WoodPercentages[player.Id] = ratios.Values.ToList()[num];
                val = ratios.Values.ToList()[num];
            }

            if (!rRatios.TryGetValue(val, out val2)) val2 = "None (" + AddWoodAmount + " wood)";

            player.Reply("Set ratio to <color=green>" + val2 + "</color>");
        }

        private void cmdAutoSortToggle(IPlayer player, string command, string[] args)
        {
            if (!HasPermission(player.Id))
            {
                player.Message(lang.GetMessage("nopermission", this, player.Id));
                return;
            }
            if (!data.Disabled.Contains(player.Id))
            {
                player.Message(lang.GetMessage("disabled", this, player.Id));
                data.Disabled.Add(player.Id);
            }
            else
            {
                player.Message(lang.GetMessage("enabled", this, player.Id));
                data.Disabled.Remove(player.Id);
            }
        }
        #endregion

    }
}
