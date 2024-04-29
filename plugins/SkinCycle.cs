using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core.Plugins;
using System;
using UnityEngine;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("SkinCycle", "Shady", "1.0.0", ResourceId = 0000)]
    class SkinCycle : RustPlugin
    {
        [PluginReference]
        readonly Plugin Compilation;

        private readonly Dictionary<ulong, bool> _activePlayers = new Dictionary<ulong, bool>();

        private Dictionary<int, Dictionary<int, ulong>> ItemSkins { get { return SkinsAPI?.Call<Dictionary<int, Dictionary<int, ulong>>>("GetSkins") ?? null; } }
        private readonly List<BasePlayer> activeSelectUI = new List<BasePlayer>();
        private readonly List<BasePlayer> activeInvalidUI = new List<BasePlayer>();
        private readonly List<BasePlayer> activeChangedUI = new List<BasePlayer>();
        readonly Dictionary<string, Timer> timerr = new Dictionary<string, Timer>();
        private List<Rust.Workshop.ApprovedSkinInfo> workshopSkins = new List<Rust.Workshop.ApprovedSkinInfo>();

        [PluginReference]
        readonly Plugin TrakSystem;

        [PluginReference]
        readonly Plugin SkinsAPI;
        readonly bool? lastUpdateState;
        void UpdateHooks(bool state)
        {
            if (lastUpdateState.HasValue && lastUpdateState == state) return;
            if (state) Subscribe(nameof(OnPlayerInput));
            else Unsubscribe(nameof(OnPlayerInput));
        }

        void Init()
        {
            permission.RegisterPermission("skincycle.canuse", this);
            timer.Once(0.5f, () =>
            {
                if (!(SkinsAPI?.IsLoaded ?? false))
                {
                    PrintWarning("SkinsAPI not detected or failed to load. Plugin unloading...");
                    covalence.Server.Command("oxide.unload " + this.Name);
                }
            });

            UpdateHooks(false);
        }

        void Unload()
        {
            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                CuiHelper.DestroyUi(player, "HelpGUI");
                CuiHelper.DestroyUi(player, "InvalidGUI");
                CuiHelper.DestroyUi(player, "ChangedGUI");
            }
        }

        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            CuiHelper.DestroyUi(player, "HelpGUI");
            CuiHelper.DestroyUi(player, "InvalidGUI");
            CuiHelper.DestroyUi(player, "ChangedGUI");
        }

        private object OnHammerHit(BasePlayer player, HitInfo info) //WTF NESTING OMG
        {
            if (player == null || info == null) return null;
            bool isActive;
            if (_activePlayers.TryGetValue(player.userID, out isActive) && isActive)
            {
                var hitEntity = info?.HitEntity ?? null;
                if (hitEntity != null)
                {
                    int findItem;
                    if (PrefabIDToItem.TryGetValue(hitEntity.prefabID, out findItem))
                    {
                        var itemInfo = ItemManager.FindItemDefinition(findItem);
                        if (itemInfo != null && HasSkins(itemInfo))
                        {
                            var currentSkin = hitEntity.skinID;
                            var nextSkinID = 0ul;

                            var listSkins = SkinsAPI?.Call<List<ulong>>("GetSkinsForItem", itemInfo.itemid);
                            if (listSkins == null || listSkins.Count < 1)
                            {
                                PrintWarning("null skins!!");
                                return null;
                            }

                            var currentSkinKey = listSkins.FindIndex(p => p == currentSkin);
                            if (currentSkinKey < 0) currentSkinKey = 0;


                            var nextKey = currentSkinKey + 1;

                            if (nextKey > (listSkins.Count - 1)) nextSkinID = 0ul;
                            else if (nextKey < 0) nextSkinID = 0ul;
                            else nextSkinID = listSkins[nextKey];

                            if (nextSkinID == currentSkin)
                            {
                                if (listSkins.Count >= (nextKey + 1)) nextSkinID = listSkins[nextKey + 1];
                                else nextSkinID = listSkins[0];
                            }

                            if (nextSkinID == currentSkin)
                            {
                                PrintWarning("next skin same as current skin (" + nextSkinID + ")");
                                return false;
                            }
                            hitEntity.skinID = nextSkinID;
                            hitEntity.SendNetworkUpdate();
                            return true;
                        }
                    }
                }

            }
            return null;
        }

        private readonly Dictionary<uint, int> PrefabIDToItem = new Dictionary<uint, int>();
        void OnServerInitialized()
        {
            workshopSkins = SkinsAPI?.Call<List<Rust.Workshop.ApprovedSkinInfo>>("GetWorkshopSkins") ?? null;
            var defs = ItemManager.itemList;
            for (int i = 0; i < defs.Count; i++)
            {
                var item = defs[i];
                if (item == null) continue;
                var itemdeployable = item?.GetComponent<ItemModDeployable>();
                if (itemdeployable == null) continue;
                PrefabIDToItem[itemdeployable.entityPrefab.resourceID] = item.itemid;
            }
        }

        readonly Hash<ulong, float> lastSkinUse = new Hash<ulong, float>();
        void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player == null || input == null || !player.IsAlive() || !player.IsConnected || player.IsDead() || !input.WasJustReleased(BUTTON.USE)) return;
            bool isActive;
            if (_activePlayers.TryGetValue(player.userID, out isActive) && isActive)
            {
                var now = Time.realtimeSinceStartup;
                var repeatDelay = (player?.GetActiveItem()?.GetHeldEntity() as BaseProjectile)?.repeatDelay ?? 0f;
                var lastTime = 0f;
                if (!lastSkinUse.TryGetValue(player.userID, out lastTime)) lastTime = lastSkinUse[player.userID] = (now - 5);
                if ((now - lastTime) < (repeatDelay)) return;
                lastSkinUse[player.userID] = now;
                var item = player?.GetActiveItem() ?? null;
                if (ProcessItem(player, item, input.IsDown(BUTTON.SPRINT) || input.WasJustPressed(BUTTON.SPRINT) || input.WasJustReleased(BUTTON.SPRINT))) SendLocalEffect(player, "assets/prefabs/deployable/repair bench/effects/skinchange_spraypaint.prefab");
            }
        }

        private bool ProcessItem(BasePlayer player, Item item, bool usePrevSkin = false)
        {
            if (player == null || item == null || !player.IsAlive() || !player.IsConnected || player.IsWounded() || player.IsSleeping()) return false;
            var definition = item.info;
            if (definition == null) return false;
            
            if (!HasSkins(definition))
            {
               
                RenderUIInvalid(player);
                activeInvalidUI.Add(player);
                return false;
            }

            var canChangeSkinVal = Interface.Oxide.CallHook("CanSkinCycleChangeSkin", player, item);

            if (canChangeSkinVal != null && !(bool)canChangeSkinVal)
                return false;


            var hasPerm = canExecute(player);
            var pSkins = new List<string>();
            if (!hasPerm)
            {
                pSkins = GetPlayerSkins(player);
                if (pSkins == null || pSkins.Count < 1)
                {
                    PrintWarning("no perm and no skins");
                    return false;
                }
            }

            var listSkins = SkinsAPI?.Call<List<ulong>>("GetSkinsForItem", item.info.itemid);
            if (listSkins == null || listSkins.Count < 1)
            {
                PrintWarning("list skins null!!");
                return false;
            }
            if (!hasPerm)
            {
                listSkins = listSkins.ToList();

                var listEdit = listSkins.ToList();
                for (int i = 0; i < listEdit.Count; i++)
                {
                    var skin1 = listEdit[i];
                    var skinName = GetSkinName(skin1.ToString());
                    if (!string.IsNullOrEmpty(skinName))
                    {
                        var hasSkin = pSkins.Any(p => p.Equals(skinName, StringComparison.OrdinalIgnoreCase));
                        if (!hasSkin) listSkins.Remove(skin1);
                    }
                }
            }
           
         
            if (listSkins == null || listSkins.Count < 1)
            {
                PrintWarning("list skins is null or empty AFTER filter");
                return false;
            }

         


            var newSkinName = string.Empty;
            Item newItem = null;
            var oldItemName = item?.name ?? string.Empty;


            var currentSkin = item.skin;
            var nextSkinID = 0ul;

            var currentSkinKey = listSkins.FindIndex(p => p == currentSkin);
            if (currentSkinKey < 0) currentSkinKey = 0;

         
            var nextKey = (usePrevSkin) ? (currentSkinKey - 1) : (currentSkinKey + 1);

            if (nextKey >= listSkins.Count - 1) nextSkinID = 0ul;
            else if (nextKey < 0) nextSkinID = 0ul;
            else nextSkinID = listSkins[nextKey];

            var hookVal = Interface.Oxide.CallHook("CanSkinCycleSelectSkin", player, item, nextSkinID);
            if (hookVal != null && !(bool)hookVal)
            {
                var hookValBool = (bool)hookVal;

                var c = 0;
                while (!hookValBool)
                {
                    c++;

                    if (c >= 500)
                    {
                        PrintError("500 hit!!!!");
                        break;
                    }

                    nextKey += 1;
                    if (nextKey >= listSkins.Count) nextSkinID = 0ul;
                    else nextSkinID = listSkins[nextKey];

                    hookVal = Interface.Oxide.CallHook("CanSkinCycleSelectSkin", player, item, nextSkinID);

                    if (hookVal != null) hookValBool = (bool)hookVal;
                    else hookValBool = true;

                    if (!hookValBool && nextSkinID == 0)
                        return false;
                    

                }

              
            }

            if (nextSkinID == 0 && currentSkin == 0 && usePrevSkin) nextSkinID = listSkins.LastOrDefault();


            if (nextSkinID == currentSkin)
            {
                //PrintWarning("next skin same as current skin (" + nextSkinID + ")");
                return false;
            }
            var weapon = item?.GetHeldEntity() as BaseProjectile;
            var itemConts = item?.contents?.itemList ?? null;
            var magName = weapon?.primaryMagazine?.ammoType?.shortname ?? string.Empty;
            var magCount = weapon?.primaryMagazine?.contents ?? 0;

            var itemPos = item?.position ?? -1;
            var itemPar = item?.parent ?? null;
            var oldName = item?.name ?? string.Empty;
            var oldID = item.uid;

            newItem = BuildNewItem(item, false);
            newItem.skin = nextSkinID;

            var heldEnt = newItem?.GetHeldEntity();
            if (heldEnt != null) heldEnt.skinID = nextSkinID;

            if (!string.IsNullOrEmpty(oldItemName)) newItem.name = oldItemName;

            
            newSkinName = GetSkinName(newItem);

            if (TrakSystem != null) TrakSystem?.Call("TransferWeaponID", oldID, newItem.uid, player.userID);

            item.RemoveFromContainer();

            if (newItem.MoveToContainer(player.inventory.containerBelt, itemPos) || newItem.MoveToContainer(player.inventory.containerMain)) RemoveFromWorld(item);

            if (string.IsNullOrEmpty(newSkinName)) newSkinName = "No/unknown skin";
            SendReply(player, "<color=#8000ff>Skin Cycle: </color>Changed skin to: <color=#8000ff>" + newSkinName + "</color>");
            return true;
        }

        void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            if (item?.parent != null) item.RemoveFromContainer();
            item.Remove();
        }

        private bool HasSkins(ItemDefinition item) { return SkinsAPI?.Call<bool>("HasSkins", item) ?? false; }
        private string GetSkinName(Item item) { return SkinsAPI?.Call<string>("GetSkinName", item) ?? string.Empty; }
        private string GetSkinName(string Id) { return SkinsAPI?.Call<string>("GetSkinNameFromID", Id) ?? string.Empty; }

        private Item BuildNewItem(Item sourceItem, bool deleteSource = true)
        {
            try 
            {
                if (sourceItem == null) throw new ArgumentNullException(nameof(sourceItem));



                var newItem = ItemManager.Create(sourceItem.info, sourceItem.amount, sourceItem.skin);
                newItem.text = sourceItem.text;
                newItem.name = sourceItem.name;

                if (sourceItem.hasCondition)
                {
                    newItem.maxCondition = newItem._maxCondition = sourceItem._maxCondition;
                    newItem.condition = newItem._condition = sourceItem._condition;
                }


                if (sourceItem?.contents != null && newItem?.contents != null && sourceItem?.contents?.itemList?.Count > 0)
                {
                    var itemList = new List<Item>(sourceItem.contents.itemList);
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var item = itemList[i];
                        if (item != null)
                        {
                            var newContentItem = BuildNewItem(item);
                            if (newContentItem != null && !newContentItem.MoveToContainer(newItem.contents)) RemoveFromWorld(newContentItem);
                        }
                    }
                }

                var baseProj = sourceItem?.GetHeldEntity() as BaseProjectile;

                if (baseProj != null)
                {
                    var newProj = newItem?.GetHeldEntity() as BaseProjectile;
                    if (newProj != null)
                    {
                        newProj.primaryMagazine.capacity = baseProj.primaryMagazine.capacity;
                        newProj.primaryMagazine.contents = baseProj.primaryMagazine.contents;
                        newProj.primaryMagazine.ammoType = baseProj.primaryMagazine.ammoType;
                    }
                    else PrintWarning("baseProj was not null, but newProj is?!");
                }



                if (deleteSource) RemoveFromWorld(sourceItem);
                return newItem;
            }
            catch(Exception ex) 
            {
                PrintWarning("Failed to run BuildNewItem!");
                PrintError(ex.ToString()); 
            }
            return null;
        }

     

        [ChatCommand("skinc")]
        private void cmdSkin(BasePlayer player, string command, string[] args)
        {
            try 
            {
                var hasPerm = canExecute(player);

                var pSkins = GetPlayerSkins(player);
                if (!hasPerm && (pSkins == null || pSkins.Count < 1))
                {
                    return;
                }

                if (args != null && args.Length > 0)
                {
                    if (args[0].ToLower() == "help")
                    {
                        SendReply(player, "Activate the function normally using <color=#8000ff>/skin</color>");
                        SendReply(player, "Place the item you want to change in your hotbar/belt");
                        SendReply(player, "Put the item in your hands (select it) and press the USE button (default is <color=#8000ff>E</color>)");
                        return;
                    }
                }

                var isActive = false;
                if (!_activePlayers.TryGetValue(player.userID, out isActive) || !isActive)
                {
                    if (!hasPerm) SendReply(player, "<color=#89f442>----<size=20>Want instant access to <i>ALL</i> skins?</size>----</color>\n\n<color=#41bbf4>You can donate a small amount to help our server, and get all skins and much more! Type <color=#8aff47>/donate</color> to read about it.</color>");
                    _activePlayers[player.userID] = true;
                    SendReply(player, "You have <color=#8000ff>activated </color>Skin Cycle!");
                    SendReply(player, "For help type <color=#8000ff>/skin help </color>");
                    RenderUI(player);
                    activeSelectUI.Add(player);
                    return;
                }
                else if (isActive)
                {
                    _activePlayers[player.userID] = false;

                    SendReply(player, "You have <color=#8000ff>de-activated </color>Skin Cycle!");
                    CuiHelper.DestroyUi(player, "HelpGUI");
                    activeSelectUI.Remove(player);
                    return;
                }
            }
            finally { UpdateHooks(_activePlayers.Count > 0); }
            
        }

        [ChatCommand("skinname")]
        private void cmdSkinName(BasePlayer player, string command, string[] args)
        {
            try 
            {
                List<string> pSkins = null;
                var hasPerm = canExecute(player);
                if (!hasPerm)
                {
                    pSkins = GetPlayerSkins(player);
                    if (pSkins == null || pSkins.Count < 1)
                    {
                        SendReply(player, "You do not have permission to use this command and/or don't have any skins!");
                        return;
                    }
                }
                if (args.Length < 1)
                {
                    SendReply(player, "No args!");
                    return;
                }
                var heldItem = player?.GetActiveItem() ?? null;
                if (heldItem == null)
                {
                    SendReply(player, "You must be holding an item!");
                    return;
                }
                if (heldItem?.info?.itemid == 1266491000)
                {
                    SendReply(player, "No skins found for this item");
                    return;
                }
                Dictionary<int, ulong> skins;
                if (!ItemSkins.TryGetValue(heldItem.info.itemid, out skins))
                {
                    SendReply(player, "No skins found for this item");
                    return;
                }
                var skinNames = skins.Values?.Select(p => GetSkinName(p.ToString()));
                if (skinNames != null && !hasPerm)
                {
                    var oldCount = skinNames.Count();
                    skinNames = skinNames.Where(p => pSkins.Any(x => x.Equals(p, StringComparison.OrdinalIgnoreCase)));
                    PrintWarning("Skin names before filtering: " + oldCount + ", post filtering: " + skinNames.Count());
                }
                if (skinNames == null || !skinNames.Any())
                {
                    SendReply(player, "No skinNames found. You may have supplied an invalid name, or you don't own this skin.");
                    return;
                }
                var arg0 = args[0];
                var findSkin = skinNames?.Where(p => p.Equals(arg0, StringComparison.OrdinalIgnoreCase) || p.IndexOf(arg0, StringComparison.OrdinalIgnoreCase) >= 0) ?? null;
                if (findSkin == null || !findSkin.Any())
                {
                    SendReply(player, "No skins found with name: " + arg0);
                    return;
                }
                if (findSkin.Count() > 1)
                {
                    SendReply(player, "More than one skin found with: " + arg0);
                    return;
                }
                var findSkinFinal = findSkin?.FirstOrDefault() ?? null;
                if (string.IsNullOrEmpty(findSkinFinal))
                {
                    SendReply(player, "Bad skin (empty)");
                    return;
                }
                var skinID = skins?.Values?.Where(p => GetSkinName(p.ToString()).Equals(findSkinFinal, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault() ?? 0UL;
                if (skinID == 0)
                {
                    SendReply(player, "Got bad ID: " + skinID);
                    return;
                }

                var hookVal = Interface.Oxide.CallHook("CanSkinCycleSelectSkin", player, heldItem, skinID);
                if (hookVal != null && !(bool)hookVal) return;

                var oldParent = heldItem?.parent ?? null;
                var oldPos = heldItem?.position ?? -1;

                var weapon = heldItem?.GetHeldEntity() as BaseProjectile;

                PrintWarning("pre build item!");

                Item newItem = null;
                if (weapon != null)
                {
                    newItem = BuildNewItem(heldItem);
                    if (newItem == null)
                    {
                        PrintWarning("newItem null!!");
                        SendReply(player, "Failed to get new item!");
                        return;
                    }
                    else PrintWarning("got newItem: " + newItem?.info?.shortname + " x" + newItem?.amount);
                }
                else newItem = heldItem;
                
                
                if (newItem == null)
                {
                    SendReply(player, "Could not build new item!");
                    return;
                }

                PrintWarning("Determined not null");

                newItem.skin = skinID;
                var newProj = newItem?.GetHeldEntity() as BaseProjectile;
                if (newProj != null)
                {
                    newProj.skinID = skinID;
                }


                if (heldItem == null || newItem != heldItem)
                {
                    PrintWarning("NOT EQUAL");

                    if (heldItem != null) heldItem.RemoveFromContainer();
                    if (newItem.MoveToContainer(oldParent, oldPos) || newItem.MoveToContainer(oldParent))
                    {
                        RemoveFromWorld(heldItem);
                    }
                    else
                    {
                        RemoveFromWorld(newItem);
                        if (heldItem != null && !heldItem.MoveToContainer(oldParent, oldPos) && !heldItem.MoveToContainer(oldParent))
                        {

                            RemoveFromWorld(heldItem);
                            SendReply(player, "Could not move new OR old item to container!!!");
                            PrintWarning("failed to move new OR old item to container!!! for " + player?.displayName + " (" + player?.UserIDString + ")");
                        }
                    }
                }

                SendLocalEffect(player, "assets/prefabs/deployable/repair bench/effects/skinchange_spraypaint.prefab");
                SendReply(player, "Set " + newItem.info.displayName.english + " skin to: " + findSkinFinal);
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                SendReply(player, "Failed to run command: " + ex?.Message);
            }
        }

        List<string> GetPlayerSkins(BasePlayer player) { return Compilation?.Call<List<string>>("GetPlayerSkins", player.UserIDString) ?? null; }

        bool HasPlayerSkins(BasePlayer player) { return GetPlayerSkins(player)?.Count > 0; }

        private bool canExecute(BasePlayer player)
        {
            if (player == null) return false;
            return permission.UserHasPermission(player.UserIDString, "skincycle.canuse");
        }

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using (var fx = new Effect(effect, pos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using (var fx = new Effect(effect, player, boneID, localPos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        private void RenderUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;
            CuiHelper.DestroyUi(player, "HelpGUI");

            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiLabel
            {
                Text =
                {
                    Text = "Press USE (<color=#8aff47>E</color>) to cycle through skins on the item you have selected in your belt.",
                   Color = "1 1 1 0.85",
                    FontSize = 20,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0 0.7",
                    AnchorMax = "1 0.8"
                }
            }, "Hud", "HelpGUI");

            CuiHelper.AddUi(player, elements);
        }

        private void RenderUIInvalid(BasePlayer player)
        {
            if (player == null) return;
            Timer outTimer;
            CuiHelper.DestroyUi(player, "InvalidGUI");
            CuiHelper.DestroyUi(player, "ChangedGUI");
            if (timerr.TryGetValue(player.UserIDString, out outTimer)) outTimer.Destroy();

            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiLabel
            {
                Text =
                {
                    Text = "This item does not have any skins!",
                   Color = "1 0.25 0.25 1",
                    FontSize = 20,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0 0.6",
                    AnchorMax = "1 0.8"
                }
            }, "Overlay", "InvalidGUI");

            CuiHelper.AddUi(player, elements);
            var UID = player?.UserIDString ?? string.Empty;
            
            timerr[player.UserIDString] = timer.Once(2f, () => 
            {
                if (player == null) return;
                if (player.IsConnected) CuiHelper.DestroyUi(player, "InvalidGUI");
                activeInvalidUI.Remove(player);
               // Timer outTimer;
                if (timerr.TryGetValue(UID, out outTimer))
                {
                    outTimer.Destroy();
                    outTimer = null;
                }
            });
        }


    }
}
