using System.Collections.Generic;
using Oxide.Core;
using System;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("CompoundOptions", "rever (edited by Shady)", "1.2.0", ResourceId = 2834)]
    [Description("Compound monument options")]
    class CompoundOptions : RustPlugin
    {
        #region Save data classes

        private class StorageData
        {
            public Dictionary<string, Order[]> VendingMachinesOrders { get; set; }
        }

        private class Order
        {
            public string _comment;
            public int sellId;
            public int sellAmount;
            public bool sellAsBP;
            public int currencyId;
            public int currencyAmount;
            public bool currencyAsBP;
        }

        #endregion

        #region Config and data

        private StorageData data;
        private static bool disallowCompoundNPC;
        private static bool disableCompoundTurrets;
        private static bool disableCompoundTrigger;
        private static bool disableCompoundVendingMachines;
        private static bool allowCustomCompoundVendingMachines = true;

        protected override void LoadDefaultConfig()
        {
            Config.Clear();
            LoadVariables();
            SaveConfig();
        }

        public void LoadVariables() {
            CheckCfg("Disallow Compound NPC", ref disallowCompoundNPC);
            CheckCfg("Disable Compound Turrets", ref disableCompoundTurrets);
            CheckCfg("Disable Compound SafeZone trigger", ref disableCompoundTrigger);
            CheckCfg("Disable Compound Vending Machines", ref disableCompoundVendingMachines);
            CheckCfg("Allow custom sell list for Compound vending machines (see in data)", ref allowCustomCompoundVendingMachines);
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Title, data);


        void Init()
        {
            data = Interface.Oxide?.DataFileSystem?.ReadObject<StorageData>(Title) ?? new StorageData();
            LoadVariables();
        }
        

        private void CheckCfg<T>(string Key, ref T var)
        {
            if (Config[Key] is T) var = (T)Config[Key];
            else Config[Key] = var;
        }

        #endregion

        #region Implementation
        /*/
        void KillNPC(NPCPlayer npc)
        {
            if (npc == null) return;


     
            var npcLocationType = npc?.AiContext?.AiLocationManager?.LocationType;
            if (npcLocationType == null) return;

            if (npcLocationType == AiLocationSpawner.SquadSpawnerLocation.Compound) npc.Kill(BaseNetworkable.DestroyMode.Gib);
            
        }/*/

        void FlushNPCVending(NPCVendingMachine vending, bool emptyInv = false)
        {
            if (vending == null) return;
            if (vending?.vendingOrders == null)
            {
                PrintWarning("flush vending: vending.vendingOrders == null!! for: " + vending?.ShortPrefabName + " @ " + vending?.transform?.position);
                return;
            }
            vending.ClearSellOrders();
            if (emptyInv)
            {
                if (vending?.inventory?.itemList != null && vending.inventory.itemList.Count > 0)
                {
                    for (int i = 0; i < vending.inventory.itemList.Count; i++)
                    {
                        var item = vending.inventory.itemList[i];
                        if (item != null) RemoveFromWorld(item);
                    }
                }
            }
        }

        void StockNPCVending(NPCVendingMachine vending)
        {
            if (vending == null) return;
            if (data?.VendingMachinesOrders == null || data?.VendingMachinesOrders?.Count < 1)
            {
                PrintWarning("no data vending machine orders on StockNPCVending!");
                return;
            }
            if (vending?.vendingOrders == null)
            {
                PrintWarning("vending.vendingOrders == null!! for: " + vending?.ShortPrefabName + " @ " + vending?.transform?.position);
                return;
            }

            Order[] orders;
            if (!data.VendingMachinesOrders.TryGetValue(vending.vendingOrders.name, out orders)) return;

            var valSet = 2000000;
            for (int i = 0; i < orders.Length; i++)
            {
                try 
                {
                    var order = orders[i];
                    if (order == null) continue;

                    vending.AddItemForSale(order.sellId, order.sellAmount, order.currencyId, order.currencyAmount, vending.GetBPState(order.sellAsBP, order.currencyAsBP));
                    if (vending?.sellOrders?.sellOrders != null)
                    {
                        for (int j = 0; j < vending.sellOrders.sellOrders.Count; j++)
                        {
                            var order2 = vending.sellOrders.sellOrders[j];

                            order2.inStock = valSet;
                        }
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
                
            }
            vending.RefreshSellOrderStockLevel();
        }

        void ProcessNPCTurrert(NPCAutoTurret npcTurret)
        {
            if (npcTurret == null) return;
            npcTurret.SetFlag(NPCAutoTurret.Flags.On, !disableCompoundTurrets, !disableCompoundTurrets);
            npcTurret.UpdateNetworkGroup();
            npcTurret.SendNetworkUpdateImmediate();
        }

        void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            if (item?.parent != null) item.RemoveFromContainer();
            item.Remove();
        }

        #endregion

        #region Oxide hooks

        void OnServerInitialized()
        {
            if (data.VendingMachinesOrders == null)
            {
                data.VendingMachinesOrders = new Dictionary<string, Order[]>();

                PrintWarning("Had to create VMOrders dictionary");
            }

            //can you believe this order updating code fucking works? wild.
            var ordersUpdated = false;
            foreach (var ent in BaseNetworkable.serverEntities)
            {
                var vending = ent as NPCVendingMachine;
                if (vending == null) continue;

              

                var vendingName = vending?.vendingOrders?.name ?? string.Empty;
                if (string.IsNullOrEmpty(vendingName))
                {
                    PrintWarning("empty/null vendingName for VM @ : " + vending?.transform?.position);
                    continue;
                }

                Order[] oldOrders = null;
                data.VendingMachinesOrders.TryGetValue(vending.vendingOrders.name, out oldOrders);


                var orders = new List<Order>();
                for (int i = 0; i < vending.vendingOrders.orders.Length; i++)
                {
                    var order = vending.vendingOrders.orders[i];

                    var skip = false;

                    foreach(var kvp in data.VendingMachinesOrders)
                    {
                        if (skip) 
                            break;

                        for (int j = 0; j < kvp.Value.Length; j++)
                        {
                            var val = kvp.Value[j];
                            if (val.sellId == order.sellItem.itemid && val.currencyId == order.currencyItem.itemid)
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (skip)
                        continue;

                    var saveOrder = new Order
                    {
                        sellId = order.sellItem.itemid,
                        sellAmount = order.sellItemAmount,
                        sellAsBP = order.sellItemAsBP,
                        currencyId = order.currencyItem.itemid,
                        currencyAmount = order.currencyAmount,
                        currencyAsBP = order.currencyAsBP,
                        _comment = $"Sell {order.sellItem.displayName.english} x {order.sellItemAmount} for {order.currencyItem.displayName.english} x {order.currencyAmount}"
                    };
                    orders.Add(saveOrder);
                    ordersUpdated = true;
                }

                Order[] outOrders;
                if (data.VendingMachinesOrders.TryGetValue(vending.vendingOrders.name, out outOrders))
                {
                    var newOrders = outOrders.ToList();

                    for (int i = 0; i < orders.Count; i++)
                        newOrders.Add(orders[i]);

                    data.VendingMachinesOrders[vending.vendingOrders.name] = newOrders.ToArray();

                }
                else data.VendingMachinesOrders[vending.vendingOrders.name] = orders.ToArray();

              //  ordersUpdated = true;
            }

            if (ordersUpdated)
            {
                Puts("Default VendingMachinesOrders updated!");
                SaveData();
            }

            PrintWarning("Found " + (data?.VendingMachinesOrders?.Count ?? 0) + " vending machine orders");

            /*/
            if (disallowCompoundNPC)
            {
                var npcPlayers = UnityEngine.Object.FindObjectsOfType<NPCPlayer>();
                for(int i = 0; i < npcPlayers.Length; i++)
                {
                    var npc = npcPlayers[i];
                    if (npc != null && !npc.IsDestroyed) KillNPC(npc);
                }
            }/*/

            var turrets = UnityEngine.Object.FindObjectsOfType<NPCAutoTurret>();
            if (turrets != null && turrets.Length > 0) for (int i = 0; i < turrets.Length; i++) ProcessNPCTurrert(turrets[i]);
            if (disableCompoundVendingMachines || allowCustomCompoundVendingMachines)
            {
                InvokeHandler.Invoke(ServerMgr.Instance, () =>
                {
                    foreach(var ent in BaseNetworkable.serverEntities)
                    {
                        var vending = ent as NPCVendingMachine;
                        if (vending == null) continue;
                        FlushNPCVending(vending, true);
                        if (disableCompoundVendingMachines) continue;
                        StockNPCVending(vending);
                    }
                }, 2f); //defer loading for 2 seconds
            }
            
        }

        object OnRotateVendingMachine(VendingMachine machine, BasePlayer player)
        {
            if (machine == null || player == null) return null;
            if ((machine as NPCVendingMachine) != null) return false;
            return null;
        }

        void OnEntityEnter(TriggerBase trigger, BaseEntity entity)
        {
            if (!(trigger is TriggerSafeZone) && !(entity is BasePlayer)) return;
            var safeZone = trigger as TriggerSafeZone;
            if (safeZone == null) return;

            safeZone.enabled = !disableCompoundTrigger;
        }

        void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null) return;

            if (disableCompoundVendingMachines)
            {
                var vending = entity as NPCVendingMachine;
                if (vending != null) FlushNPCVending(vending);
            }

            /*/
            if (disallowCompoundNPC)
            {
                var npc = entity as NPCPlayer;
                if (npc != null) KillNPC(npc);
            }/*/

            var turret = entity as NPCAutoTurret;
            if (turret != null) ProcessNPCTurrert(turret);
        }

        #endregion
    }
}
