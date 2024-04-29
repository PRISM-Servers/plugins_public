using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SkinBox", "MBR", "1.4.6")]
    class SkinBox : CovalencePlugin
    {
        public class LootBoxHandler : MonoBehaviour
        {
            private StorageContainer container;

            private int page = 0;
            private bool cycling = false, tempAllow = false;
            public bool favoriteMode = false;
            private List<ulong> skins = new List<ulong>();

            public BasePlayer player { get; private set; }
            public Item item { get; private set; }
            private FPBS localFPBS { get; set; }
            public BaseEntity ent { get; private set; }

            private void Awake()
            {
                ent = GetComponent<BaseEntity>();
                container = GetComponent<StorageContainer>();

                InvokeHandler.Invoke(this, () =>
                {
                    container.inventory.maxStackSize = 1;
                    container.SetFlag(BaseEntity.Flags.Open, true, false);
                }, 0.01f);
            }

            public void ForcePlayerToLoot(BasePlayer player)
            {
                this.player = player;
                container.PlayerOpenLoot(player, "generic_resizable", false);
            }

            public void Fill(List<ulong> skins)
            {
                EmptyBox();

                tempAllow = true;

                for (int i = 0; i < skins.Count; i++)
                {
                    var skin = skins[i];

                    var originalId = localFPBS?.originalItemId ?? this.item.info.itemid;

                    var invDef = ItemSkinDirectory.FindByInventoryDefinitionId((int)skin);
                    var targetId = (invDef.invItem as ItemSkin)?.Redirect?.itemid ?? originalId;

                    var hookVal = Interface.Oxide.CallHook("CanShowSkinboxSkin", player, skin, targetId, this.item);
                    if (hookVal != null && !(bool)hookVal) continue;

                    var item = ItemManager.CreateByItemID(originalId, 1, skin);
                    item.condition = this.item.condition;
                    item.maxCondition = this.item.maxCondition;

                    if (this.item.contents?.itemList != null)
                    {
                        foreach (var attachment in this.item.contents.itemList)
                        {
                            var newAttachment = ItemManager.Create(attachment.info, 1, 0);

                            newAttachment.maxCondition = attachment.maxCondition;
                            newAttachment.condition = attachment.condition;

                            newAttachment.skin = attachment.skin;
                            newAttachment.name = attachment.name;

                            newAttachment.MoveToContainer(item.contents);

                           
                        }

                        //Lock attachment slots (prevents from stealing attachments)
                        item.contents.SetLocked(true);
                    }

                    var name = instance.SkinsAPI.Call<string>("GetSkinNameFromID", skin.ToString());
                    item.name = name;

                    var prev = this.item.GetHeldEntity() as BaseProjectile;
                    if (prev != null)
                    {
                        var temp = item.GetHeldEntity() as BaseProjectile;
                        temp.primaryMagazine.contents = prev.primaryMagazine.contents;
                        temp.primaryMagazine.ammoType = prev.primaryMagazine.ammoType;
                    }

                    if (!item.MoveToContainer(container.inventory))
                    {
                        instance.PrintError("Couldn't move " + item.info.shortname);
                    }
                }

                tempAllow = false;
            }

            public void OnItemRemove(Item removed)
            {
                if (item == null || cycling) return;

                var skin = string.IsNullOrEmpty(removed.name) ? "Default skin" : removed.name;

                removed.name = item?.name ?? string.Empty;


                //so, the code below was MBR's. I changed it to the single line above. so far, this works fine - this was necessary to prevent losing original item name if you picked the same skin out
                /*/
                if (!string.IsNullOrEmpty(item.name) && removed.uid != item.uid)
                {
                    removed.name = item.name;
                }
                else removed.name = null;
                /*/

                removed.contents?.SetLocked(false);

                if (removed.uid != item.uid)
                {
                    var effect = new Effect("assets/prefabs/deployable/repair bench/effects/skinchange_spraypaint.prefab", player.transform.position, default(Vector3));
                    EffectNetwork.Send(effect, player.net.connection);

                    if (favoriteMode)
                    {
                        try
                        {
                            if (instance.SkinFavorite?.IsLoaded ?? false)
                            {
                                var riid = removed.info.itemid;
                                var rskin = removed.skin;

                                var f = FPBS.list.FirstOrDefault(x => x.newItemId == riid);
                                if (f != null)
                                {
                                    riid = f.originalItemId;
                                    rskin = f.skinId;
                                }

                                instance.SkinFavorite.Call("SetFavoriteSkin", player.UserIDString, riid, rskin);
                                player.IPlayer.Message("<color=orange>" + skin + "</color> has been set as your favorite skin for " + removed.info.displayName.english);
                            }
                            else throw new Exception("FavoriteSkin plugin is missing/not loaded");
                        }
                        catch (Exception e)
                        {
                            instance.Puts(e.Message + "\n" + e.StackTrace);
                            player.IPlayer.Message("An error occurred while setting your favorite skin!");
                        }
                    }
                }

                item = null;
                skins = new List<ulong>();
                localFPBS = null;

                EmptyBox();

                instance.DestroyCUI(player);
            }

            public void OnItemAdd(Item item)
            {
                if (container.inventory.itemList.Count != 1) return;

                this.item = item;
                if (this.item.position != 0) this.item.position = 0;
                page = 0;

                var full = instance.skins[this.item.info.itemid];
                if (full.Count == 0 && localFPBS != null)
                {
                    if (this.item.info.itemid == localFPBS.newItemId)
                    {
                        full = instance.skins[localFPBS.originalItemId];
                    }
                }

                if (instance.permission.UserHasPermission(player.UserIDString, "skinbox.all") || debug)
                {
                    skins = full;
                }
                else
                {
                    if (instance.UserSkins?.IsLoaded ?? false)
                    {
                        skins = (instance.UserSkins.Call<List<ulong>>("GetAllUserSkins", player.UserIDString) ?? new List<ulong>()).Where(x => full.Contains(x)).ToList();
                    }
                }

                // not sure if this ever happens
                if (!skins.Contains(0))
                {
                    skins.Insert(0, 0);
                }

                skins.Sort();

                if (instance.SkinFavorite?.IsLoaded ?? false)
                {
                    ulong favorite = instance.SkinFavorite.Call<ulong>("GetFavoriteSkin", player.UserIDString, item.info.itemid);
                    if (favorite != 0)
                    {
                        if (skins.Contains(favorite))
                        {
                            skins.Remove(favorite);
                        }
                        skins.Insert(0, favorite);
                    }
                }

                HandleChange(true);
            }

            public void HandleChange(bool instant, bool skipItems = false)
            {
                var index = page * ITEMS_PER_PAGE;

                if (index < 0)
                {
                    Interface.Oxide.LogError("Bad index: " + index + " (page: " + page + ") for " + player);
                    return;
                }

                ButtonType type = ButtonType.Favorite;

                if (index + ITEMS_PER_PAGE + 1 < skins.Count) type |= ButtonType.Next;
                if (page != 0) type |= ButtonType.Previous;

                Draw(instant, type);

                if (!skipItems)
                {
                    var list = skins.Where(x => x != (localFPBS != null ? localFPBS.skinId : item.skin)).ToList();
                    if (list.Count < 1) return;

                    var count = list.Count - index;

                    if (count < 0)
                    {
                        Interface.Oxide.LogError("Bad count: " + count + ", list.Count " + list.Count + ", index: " + index + " for " + player);
                        return;
                    }

                    list = list.GetRange(index, Mathf.Min(ITEMS_PER_PAGE, count));
                    Fill(list);
                }
            }

            public void Draw(bool instant, ButtonType types)
            {
                if ((types & ButtonType.Next) == ButtonType.Next)
                {
                    //rate limit :Pepega:
                    InvokeHandler.Invoke(this, () =>
                    {
                        if (item != null) instance.DrawButton(player, true);
                    }, instant ? 0f : 1f);
                }

                if ((types & ButtonType.Previous) == ButtonType.Previous)
                {
                    //rate limit :Pepega:
                    InvokeHandler.Invoke(this, () =>
                    {
                        if (item != null) instance.DrawButton(player, false);
                    }, instant ? 0f : 1f);
                }

                if ((types & ButtonType.Favorite) == ButtonType.Favorite)
                {
                    //rate limit :Pepega:
                    InvokeHandler.Invoke(this, () =>
                    {
                        if (item != null) instance.DrawFavoriteButton(player, favoriteMode);
                    }, instant ? 0f : 1f);
                }
            }
            
            public void OnLootStart(BasePlayer player)
            {
                if (!instance.boxes.Contains(this) && this.player == player)
                {
                    instance.boxes.Add(this);
                }
            }

            public void OnLootEnd(BasePlayer player)
            {
                if (item != null)
                {
                    if (player.IsDead())
                    {
                        item.Drop(player.transform.position, Vector3.zero);
                    }
                    else player.GiveItem(item, BaseEntity.GiveItemReason.PickedUp);
                }

                if (instance.CUIs.ContainsKey(player))
                {
                    instance.DestroyCUI(player);
                }

                foreach (var item in instance.boxes)
                {
                    if (item == this)
                    {
                        instance.boxes.Remove(item);
                        break;
                    }
                }

                EmptyBox();

                InvokeHandler.Invoke(this, () => ent.KillMessage(), 0.2f);
            }

            private void EmptyBox()
            {
                cycling = true; //prevents onitemremove from fucking it all up
                foreach (var item in container.inventory.itemList.ToArray())
                {
                    if (item.uid == this.item?.uid) continue;
                    CompletelyDestroy(item);
                }
                cycling = false;
            }

            public void UpdatePage(bool next)
            {
                if (next) page += 1;
                else page -= 1;

                var e = new Effect("assets/bundled/prefabs/fx/notice/item.select.fx.prefab", player.transform.position, Vector3.zero);
                EffectNetwork.Send(e, player.net.connection);

                HandleChange(false);
            }

            public void UpdateFavorite(bool value)
            {
                favoriteMode = value;
                HandleChange(true, true);
            }

            public bool CanAcceptItem(Item i)
            {
                if (i?.info == null) return false;

                if (instance.skins[i.info.itemid].Count < 1)
                {
                    if (item == null)
                    {
                        var fpbs = FPBS.list.FirstOrDefault(x => x.newItemId == i.info.itemid);
                        if (fpbs == null)
                        {
                            // We are putting in an item that can't be skinned?
                            return false;
                        }

                        localFPBS = fpbs;
                    }

                    else
                    {
                        if (tempAllow)
                        {
                            // Workaround for facepunchs bullshit

                            var fpbs = FPBS.list.FirstOrDefault(x => x.newItemId == i.info.itemid);

                            if (fpbs.originalItemId == item.info.itemid || fpbs.originalItemId == localFPBS?.originalItemId)
                            {
                                return true;
                            }
                        }

                        instance.LogWarning("Retrunign lfaase");
                        return false;
                    }
                }

                return item == null || tempAllow;
            }
        }

        #region Setup & Variables

        [PluginReference] private readonly Plugin SkinsAPI, UserSkins, SkinFavorite;
        private const int ITEMS_PER_PAGE = 47;
        private const bool debug = false;
        private const int rockId = 963906841;

        private static SkinBox instance;

        private Dictionary<BasePlayer, List<string>> CUIs = new Dictionary<BasePlayer, List<string>>();
        private Dictionary<int, List<ulong>> skins = new Dictionary<int, List<ulong>>();
        private Dictionary<BasePlayer, float> CommandCooldown = new Dictionary<BasePlayer, float>();
        private List<LootBoxHandler> boxes = new List<LootBoxHandler>();

        public enum ButtonType
        {
            Previous = 1 << 3,
            Next = 1 << 2,
            Favorite = 1 << 1
        }

        #endregion

        #region FP bullshit

        // skins that create a new item on creation

        private class FPBS
        {
            public int originalItemId;
            public ulong skinId;
            public int newItemId;

            public static List<FPBS> list = new List<FPBS>();
        }

        #endregion

        #region Hooks

        private void OnDefaultItemsReceived(PlayerInventory inventory)
        {
            if (inventory == null || inventory.baseEntity == null) return;

            ulong favorite = instance.SkinFavorite.Call<ulong>("GetFavoriteSkin", inventory.baseEntity.UserIDString, rockId);
            if (favorite == 0) return;
            
            foreach (var item in inventory.containerBelt.itemList)
            {
                if (item.info.shortname == "rock")
                {
                    var f = FPBS.list.FirstOrDefault(x => x.originalItemId == item.info.itemid && x.skinId == favorite);

                    if (f == null)
                    {
                        item.skin = favorite;
                        item.MarkDirty();

                        BaseEntity heldEntity = item.GetHeldEntity();
                        if (heldEntity != null)
                        {
                            heldEntity.skinID = favorite;
                            heldEntity.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                        }
                    }
                    else
                    {
                        var pos = item.position;
                        var nItem = ItemManager.CreateByItemID(f.newItemId, 1, f.skinId);
                        CompletelyDestroy(item);
                        nItem.MoveToContainer(inventory.containerBelt, pos);
                    }

                    break;
                }
            }
        }

        private void Unload()
        {
            foreach (var changer in boxes.ToArray())
            {
                changer.OnLootEnd(changer.player);
            }

            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyCUI(player);
            }

            instance = null;
        }

        ItemContainer.CanAcceptResult? CanAcceptItem(ItemContainer container, Item item, int targetPos)
        {
            if (container == null || item == null || container.entityOwner == null || container.entityOwner.net == null) return null;

            var handler = GetHandlerFromNetId(container.entityOwner.net.ID.Value);
            if (handler == null) return null;

            if (!handler.CanAcceptItem(item))
            {
                return ItemContainer.CanAcceptResult.CannotAccept;
            }

            return null;
        }

        void OnItemRemovedFromContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null || container.entityOwner == null) return;

            var component = container.entityOwner.GetComponent<LootBoxHandler>();
            component?.OnItemRemove(item);
        }

        void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null || container.entityOwner == null) return;

            var component = container.entityOwner.GetComponent<LootBoxHandler>();
            component?.OnItemAdd(item);
        }

        private void Init()
        {
            permission.RegisterPermission("skinbox.all", this);

            string[] cmdAliases = { "sb", "sbs", "skinbox", "scinbox", "skibox", "skin" };

            AddCovalenceCommand(cmdAliases, nameof(SkinCMD));
        }

        private void OnServerInitialized(bool first)
        {
            instance = this;

            if (!first)
            {
                if (SkinsAPI != null && SkinsAPI.IsLoaded)
                {
                    LoadSkinCache();
                }
            }

            else timer.Once(10f, () =>
            {
                if (SkinsAPI == null)
                {
                    PrintError("SkinsAPI is missing, aborting!");
                    Interface.Oxide.UnloadPlugin(Name);
                    return;
                }

                LoadSkinCache();
            });
        }
        
        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            var component = entity?.GetComponent<LootBoxHandler>();
            component?.OnLootStart(player);
        }

        private void OnLootEntityEnd(BasePlayer player, BaseCombatEntity entity)
        {
            var component = entity?.GetComponent<LootBoxHandler>();
            component?.OnLootEnd(player);
        }

        private object OnItemCraftSkin(ItemCraftTask task, ItemCrafter crafter)
        {
            if (task == null || crafter?.owner == null) return null;

            if (!(SkinFavorite?.IsLoaded ?? false)) 
                return null;

            if (skins[task.blueprint.targetItem.itemid].Count < 1) return null; //necessary?

            ulong favorite = instance.SkinFavorite.Call<ulong>("GetFavoriteSkin", crafter.owner.UserIDString, task.blueprint.targetItem.itemid);

            if (favorite != 0)
                return favorite;

            return null;
        }

        #endregion

        #region Commands

        [Command("skinbox2cuicmd")]
        private void SkinBoxCUICMD(IPlayer player, string command, string[] args)
        {
            var p = player.Object as BasePlayer;
            if (p == null) return;

            if (args.Length < 1) return;

            DestroyCUI(p);

            var box = GetHandlerFromPlayer(p);
            if (box == null) return;

            switch (args[0].ToLower())
            {
                case "next":
                    {
                        box.UpdatePage(true);
                        return;
                    }
                case "previous":
                    {
                        box.UpdatePage(false);
                        return;
                    }
                case "favorite":
                    {
                        box.UpdateFavorite(!box.favoriteMode);
                        return;
                    }
            }
        }

       
        private void SkinCMD(IPlayer player, string command, string[] args)
        {
            if (player.IsServer)
            {
                player.Reply("Server console cannot use this command!");
                return;
            }

            var p = player.Object as BasePlayer;

            if (p.IsDead())
            {
                player.Reply("You cannot run this command while you are dead!");
                return;
            }
            
            if (p.GetParentEntity() != null)
            {
                player.Reply("You cannot use skinbox while being on a vehicle!");
                return;
            }

            if (CommandCooldown.ContainsKey(p))
            {
                if (Time.realtimeSinceStartup < CommandCooldown[p])
                {
                    player.Reply("You are using this command too fast, try again in " + Math.Round((CommandCooldown[p] - Time.realtimeSinceStartup), 1) + "s");
                    return;
                }

                CommandCooldown.Remove(p);
            }

            CommandCooldown.Add(p, Time.realtimeSinceStartup + 4);

            foreach (var changer in boxes)
            {
                if (changer.player.UserIDString == player.Id)
                {
                    player.Reply("You appear to be changing an item skin already, if this is not true <i>contact staff!</i>");
                    return;
                }
            }

            var pos = new Vector3(p.transform.position.x, -200, p.transform.position.z);
            var box = GameManager.server.CreateEntity("assets/prefabs/misc/halloween/coffin/coffinstorage.prefab", pos);
            if (box == null)
            {
                player.Reply("An error occurred while setting up your skinbox");
                return;
            }

            UnityEngine.Object.Destroy(box.GetComponent<DestroyOnGroundMissing>());
            UnityEngine.Object.Destroy(box.GetComponent<GroundWatch>());

            var comp = box.gameObject.AddComponent<LootBoxHandler>();
            if (args.Length > 0 && args[0].ToLower().StartsWith("fav"))
            {
                comp.favoriteMode = true;
            }
            
            box.Spawn();

            timer.Once(0.25f, () =>
            {
                if (Interface.Oxide.CallHook("CanLootEntity", p, box) != null)
                {
                    player.Reply("You are not allowed to use the skinbox at this time");
                    comp.OnLootEnd(p);
                    return;
                }
                comp.ForcePlayerToLoot(p);
            });
        }

        [Command("skinbox.listskins")]
        private void ListSkinsForItem(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply("Usage: skinbox.listskins <item id>");
                return;
            }

            int id;
            if (!int.TryParse(args[0], out id))
            {
                player.Reply("Invalid item id");
                return;
            }

            var skins = new List<ulong>();
            if (!this.skins.TryGetValue(id, out skins))
            {
                player.Reply("No skins found for this item");
                return;
            }

            player.Reply("Skins for item " + id + ": " + string.Join(", ", skins));
        }

        #endregion

        #region Methods

        private void LoadSkinCache()
        {
            var totalSkins = 0;

            foreach (var item in ItemManager.itemList)
            {
                var skins = SkinsAPI.Call<List<ulong>>("GetSkinsForItem", item.itemid) ?? new List<ulong>();

                if (skins.Count > 1)
                {
                    totalSkins += skins.Count - 1;
                }

                this.skins.Add(item.itemid, skins);
            }

            foreach (var item in ItemManager.itemList)
            {
                foreach (var skinId in skins[item.itemid])
                {
                    ItemSkinDirectory.Skin skin = ItemSkinDirectory.FindByInventoryDefinitionId((int)skinId);
                    ItemSkin itemSkin = skin.invItem as ItemSkin;

                    if (itemSkin?.Redirect == null)
                    {
                        continue;
                    }

                    var ns = itemSkin.Redirect.itemid;

                    FPBS.list.Add(new FPBS { originalItemId = item.itemid, newItemId = ns, skinId = skinId });
                }
            }

            Puts("Skin cache loaded, there are " + skins.Count + " items that have skins (" + totalSkins + " skins in total) and " + FPBS.list.Count + " weird skins");
        }

        public void DestroyCUI(BasePlayer player)
        {
            if (!CUIs.ContainsKey(player)) return;

            if (CUIs[player].Count > 0)
            {
                foreach (var id in CUIs[player])
                {
                    CuiHelper.DestroyUi(player, id);
                }

                CUIs[player] = new List<string>();
            }
        }

        public LootBoxHandler GetHandlerFromNetId(ulong id)
        {
            foreach (var handler in boxes)
            {
                if (handler.ent.net.ID.Value == id) return handler;
            }

            return null;
        }

        public LootBoxHandler GetHandlerFromPlayer(BasePlayer player)
        {
            foreach (var handler in boxes)
            {
                if (handler.player?.userID == player.userID) return handler;
            }

            return null;
        }

        public static void CompletelyDestroy(Item item)
        {
            if (item == null) return;

            item.GetWorldEntity()?.KillMessage();
            item.GetHeldEntity()?.KillMessage();

            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private void DrawFavoriteButton(BasePlayer player, bool enabled)
        {
            var menu = new CuiElementContainer();
            var id = menu.Add(new CuiPanel
            {
                Image = { Color = "0.0 0.0 0.0 0.0" },
                RectTransform = { AnchorMin = "0.65 0.005", AnchorMax = "0.90 0.065" },
            }, "Overlay");

            if (!CUIs.ContainsKey(player))
            {
                CUIs.Add(player, new List<string>());
            }

            CUIs[player].Add(id);

            menu.Add(new CuiButton
            {
                Button =
                {
                    Command = "skinbox2cuicmd favorite",
                    Color = enabled ? "0.55 0.70 0.40 0.95" : "0.7 0.1 0.2 0.95",
                },
                RectTransform =
                {
                    AnchorMin = "0.00 0.00",
                    AnchorMax = "1.00 1.00"
                },
                Text =
                {
                    Text = enabled ? "Favorite mode is ON! <size=11>(skin you select will\nbe saved as favorite and displayed first next time)</size>" : "Favorite mode is OFF <size=11>(Running /skinbox fav\nstarts skinbox with favorite mode enabled)</size>",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter
                }
            }, id);
            CuiHelper.AddUi(player, menu);
        }

        private void DrawButton(BasePlayer player, bool next)
        {
            var min = next ? "0.80 0.07" : "0.65 0.07";
            var max = next ? "0.90 0.13" : "0.75 0.13";
            var command = next ? "skinbox2cuicmd next" : "skinbox2cuicmd previous";

            var menu = new CuiElementContainer();
            var id = menu.Add(new CuiPanel
            {
                Image = { Color = "0.0 0.0 0.0 0.0" },
                RectTransform = { AnchorMin = min, AnchorMax = max },
            }, "Overlay");

            if (!CUIs.ContainsKey(player))
            {
                CUIs.Add(player, new List<string>());
            }

            CUIs[player].Add(id);

            menu.Add(new CuiButton
            {
                Button =
                {
                    Command = command,
                    Color = "0.80 0.55 0.15 0.95",
                },
                RectTransform =
                {
                    AnchorMin = "0.00 0.00",
                    AnchorMax = "1.00 1.00"
                },
                Text =
                {
                    Text = next ? "Next Page" : "Previous Page",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter
                }
            }, id);
            CuiHelper.AddUi(player, menu);
        }

        #endregion
    }
}
