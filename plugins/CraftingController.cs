﻿// Reference: 0Harmony
using Facepunch;
using HarmonyLib;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{

    [Info("Crafting Controller", "Mughisi (edited by Shady)", "2.4.91", ResourceId = 695)]
    internal class CraftingController : RustPlugin
    {

        public static CraftingController ins;

        #region Configuration Data
        // Do not modify these values, to configure this plugin edit
        // 'CraftingController.json' in your server's config folder.
        // <drive>:\...\server\<server identity>\oxide\config\

        private bool configChanged;

        // Plugin settings
        private const string DefaultChatPrefix = "Crafting Controller";
        private const string DefaultChatPrefixColor = "#008000ff";

        public string ChatPrefix { get; private set; }
        public string ChatPrefixColor { get; private set; }

        // Plugin options
        private const float DefaultCraftingRate = 100;
        private const float DefaultCraftingExperience = 100;
        private const bool DefaultAdminInstantCraft = true;
        private const bool DefaultModeratorInstantCraft = false;
        private const bool DefaultCompleteCurrentCraftingOnShutdown = false;

        public float CraftingRate { get; private set; }
        public float CraftingExperience { get; private set; }
        public bool AdminInstantCraft { get; private set; }
        public bool ModeratorInstantCraft { get; private set; }
        public bool CompleteCurrentCrafting { get; private set; }

        // Plugin options - blocked items
        private static readonly List<object> DefaultBlockedItems = new List<object>();
        private static readonly Dictionary<string, object> DefaultIndividualRates = new Dictionary<string, object>();

        public List<string> BlockedItems { get; private set; }
        public Dictionary<string, float> IndividualRates { get; private set; }

        // Plugin messages
        private const string DefaultCurrentCraftingRate = "The crafting rate is set to {0}%.";
        private const string DefaultModifyCraftingRate = "The crafting rate is now set to {0}%.";
        private const string DefaultModifyCraftingRateItem = "The crafting rate for {0} is now set to {1}%.";
        private const string DefaultModifyError = "The new crafting rate must be a number. 0 is instant craft, 100 is normal and 200 is double!";
        private const string DefaultCraftBlockedItem = "{0} is blocked and can not be crafted!";
        private const string DefaultNoItemSpecified = "You need to specify an item for this command.";
        private const string DefaultNoItemRate = "You need to specify an item and a new crafting rate for this command.";
        private const string DefaultInvalidItem = "{0} is not a valid item. Please use the name of the item as it appears in the item list. Ex: Camp Fire";
        private const string DefaultBlockedItem = "{0} has already been blocked!";
        private const string DefaultBlockSucces = "{0} has been blocked from crafting.";
        private const string DefaultUnblockItem = "{0} is not blocked!";
        private const string DefaultUnblockSucces = "{0} is no longer blocked from crafting.";
        private const string DefaultNoPermission = "You don't have permission to use this command.";
        private const string DefaultShowBlockedItems = "The following items are blocked: ";
        private const string DefaultNoBlockedItems = "No items have been blocked.";

        public string CurrentCraftingRate { get; private set; }
        public string ModifyCraftingRate { get; private set; }
        public string ModifyCraftingRateItem { get; private set; }
        public string ModifyError { get; private set; }
        public string CraftBlockedItem { get; private set; }
        public string NoItemSpecified { get; private set; }
        public string NoItemRate { get; private set; }
        public string InvalidItem { get; private set; }
        public string BlockedItem { get; private set; }
        public string BlockSucces { get; private set; }
        public string UnblockItem { get; private set; }
        public string UnblockSucces { get; private set; }
        public string NoPermission { get; private set; }
        public string ShowBlockedItems { get; private set; }
        public string NoBlockedItems { get; private set; }

        #endregion

        private List<ItemBlueprint> blueprintDefinitions = new List<ItemBlueprint>();

        public Dictionary<string, float> Blueprints { get; set; } = new Dictionary<string, float>();

        private List<ItemDefinition> itemDefinitions = new List<ItemDefinition>();

        public List<string> Items { get; } = new List<string>();

        private readonly MethodInfo finishCraftingTask = typeof(ItemCrafter).GetMethod("FinishCrafting", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Loaded() => LoadConfigValues();

        private void OnServerInitialized()
        {
            blueprintDefinitions = ItemManager.bpList;
            foreach (var bp in blueprintDefinitions)
                Blueprints[bp.targetItem.shortname] = bp.time;

            itemDefinitions = ItemManager.itemList;
            foreach (var itemdef in itemDefinitions)
                Items.Add(itemdef.displayName.english);

            UpdateCraftingRate();
        }

      

        protected override void LoadDefaultConfig() => PrintWarning("New configuration file created.");

        private void LoadConfigValues()
        {
            // Plugin settings
            ChatPrefix = GetConfigValue("Settings", "ChatPrefix", DefaultChatPrefix);
            ChatPrefixColor = GetConfigValue("Settings", "ChatPrefixColor", DefaultChatPrefixColor);

            // Plugin options
            AdminInstantCraft = GetConfigValue("Options", "InstantCraftForAdmins", DefaultAdminInstantCraft);
            ModeratorInstantCraft = GetConfigValue("Options", "InstantCraftForModerators", DefaultModeratorInstantCraft);
            CraftingRate = GetConfigValue("Options", "CraftingRate", DefaultCraftingRate);
            CraftingExperience = GetConfigValue("Options", "CraftingExperienceRate", DefaultCraftingExperience);
            CompleteCurrentCrafting = GetConfigValue("Options", "CompleteCurrentCraftingOnShutdown", DefaultCompleteCurrentCraftingOnShutdown);


            // Plugin options - blocked items
            var list = GetConfigValue("Options", "BlockedItems", DefaultBlockedItems);
            var dict = GetConfigValue("Options", "IndividualCraftingRates", DefaultIndividualRates);

            BlockedItems = new List<string>();
            foreach (var item in list)
                BlockedItems.Add(item.ToString());

            IndividualRates = new Dictionary<string, float>();
            foreach (var entry in dict)
            {
                float rate;
                if (!float.TryParse(entry.Value.ToString(), out rate)) continue;
                IndividualRates.Add(entry.Key, rate);
            }

            // Plugin messages
            CurrentCraftingRate = GetConfigValue("Messages", "CurrentCraftingRate", DefaultCurrentCraftingRate);
            ModifyCraftingRate = GetConfigValue("Messages", "ModifyCraftingRate", DefaultModifyCraftingRate);
            ModifyCraftingRateItem = GetConfigValue("Messages", "ModifyCraftingRateItem", DefaultModifyCraftingRateItem);
            ModifyError = GetConfigValue("Messages", "ModifyCraftingRateError", DefaultModifyError);
            CraftBlockedItem = GetConfigValue("Messages", "CraftBlockedItem", DefaultCraftBlockedItem);
            NoItemSpecified = GetConfigValue("Messages", "NoItemSpecified", DefaultNoItemSpecified);
            NoItemRate = GetConfigValue("Messages", "NoItemRate", DefaultNoItemRate);
            InvalidItem = GetConfigValue("Messages", "InvalidItem", DefaultInvalidItem);
            BlockedItem = GetConfigValue("Messages", "BlockedItem", DefaultBlockedItem);
            BlockSucces = GetConfigValue("Messages", "BlockSucces", DefaultBlockSucces);
            UnblockItem = GetConfigValue("Messages", "UnblockItem", DefaultUnblockItem);
            UnblockSucces = GetConfigValue("Messages", "UnblockSucces", DefaultUnblockSucces);
            NoPermission = GetConfigValue("Messages", "NoPermission", DefaultNoPermission);
            ShowBlockedItems = GetConfigValue("Messages", "ShowBlockedItems", DefaultShowBlockedItems);
            NoBlockedItems = GetConfigValue("Messages", "NoBlockedItems", DefaultNoBlockedItems);

            if (!configChanged) return;
            Puts("Configuration file updated.");
            SaveConfig();
        }

        #region Chat/Console command to check/alter the crafting rate.

        [ChatCommand("rate")]
        private void CraftCommandChat(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                SendChatMessage(player, CurrentCraftingRate, CraftingRate);
                return;
            }

            if (!player.IsAdmin)
            {
                SendChatMessage(player, NoPermission);
                return;
            }

            float rate;
            if (!float.TryParse(args[0], out rate))
            {
                SendChatMessage(player, ModifyError);
                return;
            }

            CraftingRate = rate;
            SetConfigValue("Options", "CraftingRate", rate);
            UpdateCraftingRate();
            SendChatMessage(player, ModifyCraftingRate, CraftingRate);
        }

        [ConsoleCommand("crafting.rate")]
        private void CraftCommandConsole(ConsoleSystem.Arg arg)
        {
            if (!arg.HasArgs())
            {
                arg.ReplyWith(string.Format(CurrentCraftingRate, CraftingRate));
                return;
            }

            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NoPermission);
                return;
            }

            var rate = arg.GetFloat(0, -1f);
            if (rate == -1f)
            {
                arg.ReplyWith(ModifyError);
                return;
            }

            CraftingRate = rate;
            SetConfigValue("Options", "CraftingRate", rate);
            UpdateCraftingRate();
            arg.ReplyWith(string.Format(ModifyCraftingRate, CraftingRate));
        }

        private readonly DataFileSystem dataSystem = Interface.Oxide.DataFileSystem;

        [ConsoleCommand("crafting.load")]
        private void CraftLoad(ConsoleSystem.Arg arg)
        {
            if (!arg.HasArgs())
            {
                arg.ReplyWith("No args");
                return;
            }

            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NoPermission);
                return;
            }
            if (arg?.Args == null || arg.Args.Length < 1)
            {
                arg.ReplyWith("No args");
                return;
            }
            var path = arg.Args[0];
            var data = dataSystem.GetDatafile(path);
            if (data == null || !data.Any())
            {
                arg.ReplyWith("No data in path: " + path);
                return;
            }
            var newData = dataSystem.ReadObject<Dictionary<string, float>>(path);
            if (newData == null)
            {
                arg.ReplyWith("New data is null: " + path);
                return;
            }
            var kvpSB = new StringBuilder();
            foreach (var kvp in data) kvpSB.AppendLine(kvp.Key + ", " + kvp.Value);
            arg.ReplyWith(kvpSB.ToString().TrimEnd());
            Blueprints = newData;
            UpdateCraftingRate();
        }

        [ConsoleCommand("crafting.export")]
        private void CraftExport(ConsoleSystem.Arg arg)
        {
            if (!arg.HasArgs())
            {
                arg.ReplyWith("No args");
                return;
            }

            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NoPermission);
                return;
            }
            if (arg?.Args == null || arg.Args.Length < 1)
            {
                arg.ReplyWith("No args");
                return;
            }
            var path = arg.Args[0];
            dataSystem.WriteObject<Dictionary<string, float>>(path, Blueprints);
            arg.ReplyWith("Attempted to export crafting values to: " + path);
        }

        #endregion

        #region Chat/Console command to alter the crafting rate of a single item.

        [ChatCommand("itemrate")]
        private void CraftItemCommandChat(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendChatMessage(player, NoPermission);
                return;
            }

            if (args.Length == 0 || args.Length < 2)
            {
                SendChatMessage(player, NoItemRate);
                return;
            }

            float rate;
            if (!float.TryParse(args[args.Length - 1], out rate))
            {
                SendChatMessage(player, ModifyError);
                return;
            }

            var item = string.Empty;
            for (var i = 0; i < args.Length - 1; i++)
                item += args[i] + " ";
            item = item.Trim();

            if (!Items.Contains(item))
            {
                SendChatMessage(player, InvalidItem, item);
                return;
            }


            if (IndividualRates.ContainsKey(item))
                IndividualRates[item] = rate;
            else
                IndividualRates.Add(item, rate);

            SetConfigValue("Options", "IndividualCraftingRates", IndividualRates);
            SendChatMessage(player, ModifyCraftingRateItem, item, rate);
            UpdateCraftingRate();
        }

        [ConsoleCommand("crafting.itemrate")]
        private void CraftItemCommandConsole(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NoPermission);
                return;
            }

            if (!arg.HasArgs(2))
            {
                arg.ReplyWith(NoItemRate);
                return;
            }

            var rate = arg.GetFloat(arg.Args.Length - 1, -1f);
            if (rate == -1f)
            {
                arg.ReplyWith(ModifyError);
                return;
            }

            var item = string.Empty;
            for (var i = 0; i < arg.Args.Length - 1; i++)
                item += arg.Args[i] + " ";
            item = item.Trim();

            if (!Items.Contains(item))
            {
                arg.ReplyWith(string.Format(InvalidItem, item, rate));
                return;
            }

            if (IndividualRates.ContainsKey(item))
                IndividualRates[item] = rate;
            else
                IndividualRates.Add(item, rate);

            SetConfigValue("Options", "IndividualCraftingRates", IndividualRates);
            arg.ReplyWith(string.Format(ModifyCraftingRateItem, item, rate));
            UpdateCraftingRate();
        }

        #endregion

        #region Chat/Console command to block an item from being crafted.

        [ChatCommand("block")]
        private void BlockCommandChat(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendChatMessage(player, NoPermission);
                return;
            }

            if (args.Length == 0)
            {
                SendChatMessage(player, NoItemSpecified);
                return;
            }

            var item = string.Join(" ", args);
            if (!Items.Contains(item))
            {
                SendChatMessage(player, InvalidItem, item);
                return;
            }

            if (BlockedItems.Contains(item))
            {
                SendChatMessage(player, BlockedItem, item);
                return;
            }

            BlockedItems.Add(item);
            SetConfigValue("Options", "BlockedItems", BlockedItems);
            SendChatMessage(player, BlockSucces, item);
        }

        [ConsoleCommand("crafting.block")]
        private void BlockCommandConsole(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NoPermission);
                return;
            }

            if (!arg.HasArgs(1))
            {
                arg.ReplyWith(NoItemSpecified);
                return;
            }

            var item = string.Join(" ", arg.Args);
            if (!Items.Contains(item))
            {
                arg.ReplyWith(string.Format(InvalidItem, item));
                return;
            }

            if (BlockedItems.Contains(item))
            {
                arg.ReplyWith(string.Format(BlockedItem, item));
                return;
            }

            BlockedItems.Add(item);
            SetConfigValue("Options", "BlockedItems", BlockedItems);
            arg.ReplyWith(string.Format(BlockSucces, item));
        }

        #endregion

        #region Chat/Console command to unblock an item from being crafted.

        [ChatCommand("unblock")]
        private void UnblockCommandChat(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendChatMessage(player, NoPermission);
                return;
            }

            if (args.Length == 0)
            {
                SendChatMessage(player, NoItemSpecified);
                return;
            }

            var item = string.Join(" ", args);
            if (item != "*")
            {
                if (!Items.Contains(item))
                {
                    SendChatMessage(player, InvalidItem, item);
                    return;
                }

                if (!BlockedItems.Contains(item))
                {
                    SendChatMessage(player, UnblockItem, item);
                    return;
                }

                BlockedItems.Remove(item);
            }
            else
                BlockedItems = new List<string>();

            SetConfigValue("Options", "BlockedItems", BlockedItems);
            SendChatMessage(player, UnblockSucces, item);
        }

        [ConsoleCommand("crafting.unblock")]
        private void UnblockCommandConsole(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith(NoPermission);
                return;
            }

            if (!arg.HasArgs())
            {
                arg.ReplyWith(NoItemSpecified);
                return;
            }

            var item = string.Join(" ", arg.Args);
            if (item != "*")
            {
                if (!Items.Contains(item))
                {
                    arg.ReplyWith(string.Format(InvalidItem, item));
                    return;
                }

                if (!BlockedItems.Contains(item))
                {
                    arg.ReplyWith(string.Format(UnblockItem, item));
                    return;
                }

                BlockedItems.Remove(item);
            }
            else
                BlockedItems = new List<string>();

            SetConfigValue("Options", "BlockedItems", BlockedItems);
            arg.ReplyWith(string.Format(UnblockSucces, item));
        }

        #endregion

        [ChatCommand("blocked")]
        private void BlockedItemsList(BasePlayer player, string command, string[] args)
        {
            if (BlockedItems.Count == 0)
                SendChatMessage(player, NoBlockedItems);
            else
            {
                SendChatMessage(player, ShowBlockedItems);
                foreach (var item in BlockedItems)
                    SendChatMessage(player, item);
            }
        }

        private void SendHelpText(BasePlayer player) => SendChatMessage(player, CurrentCraftingRate, CraftingRate);

        private void OnServerQuit()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (CompleteCurrentCrafting)
                    CompleteCrafting(player);

                CancelAllCrafting(player);
            }
        }

        private void CompleteCrafting(BasePlayer player)
        {
            var crafter = player.inventory.crafting;
            if (crafter.queue.Count == 0) return;
            var task = crafter.queue.First<ItemCraftTask>();
            finishCraftingTask.Invoke(crafter, new object[] { task });
           // crafter.queue.
           // crafter.queue.Dequeue();
        }

        private static void CancelAllCrafting(BasePlayer player)
        {
            var crafter = player.inventory.crafting;
            foreach (var task in crafter.queue)
                crafter.CancelTask(task.taskUID, true);
        }

        private void UpdateCraftingRate()
        {
            float rate;

            foreach (var bp in blueprintDefinitions)
            {
                if (IndividualRates.TryGetValue(bp.targetItem.displayName.english, out rate)) bp.time = Blueprints[bp.targetItem.shortname] * rate / 100;
                else bp.time = Blueprints[bp.targetItem.shortname] * CraftingRate / 100;
            }
        }

        private object OnItemCraft(ItemCraftTask task, BasePlayer owner)
        {
            var itemname = task.blueprint.targetItem.displayName.english;

            if (AdminInstantCraft && owner.net.connection.authLevel == 2) task.endTime = 1f;
            if (ModeratorInstantCraft && owner.net.connection.authLevel == 1) task.endTime = 1f;

            if (!BlockedItems.Contains(itemname)) return null;

            task.cancelled = true;

            SendChatMessage(owner, CraftBlockedItem, itemname);

            foreach (var amount in task.blueprint.ingredients) //disgusting foreach. fix.
                owner.inventory.GiveItem(ItemManager.CreateByItemID(amount.itemid, (int)amount.amount * task.amount));

            return false;
        }

        private static bool InstantAdminBulkCraft(BasePlayer player, ItemCraftTask task)
        {
            var crafter = player.inventory.crafting;
            var amount = task.amount;

            for (var i = 1; i <= amount; i++)
            {
                crafter.taskUID++;
                var item = new ItemCraftTask
                {
                    blueprint = task.blueprint,
                    endTime = 1f,
                    taskUID = crafter.taskUID,
                    instanceData = null
                };

               // crafter.queue.Enqueue(item);
                player?.Command("note.craft_add", item.taskUID, item.blueprint.targetItem.itemid);
            }

            return false;
        }

        private void OnItemCraftFinished(ItemCraftTask task, Item item, ItemCrafter crafter)
        {
            if (task == null || crafter == null || crafter?.owner == null || crafter.owner.Connection.authLevel < 1) 
                return;

            crafter.queue.First().endTime = 1f;
        }

        #region Fields
        private Harmony _harmony;
        #endregion
        #region Hooks
        private void Init()
        {
            ins = this;
           
            DoHarmonyPatches((_harmony = new Harmony(GetType().Name)));

        }

        private void Unload()
        {
            try
            {
                for (int i = 0; i < blueprintDefinitions.Count; i++)
                {
                    var bp = blueprintDefinitions[i];
                    bp.time = Blueprints[bp.targetItem.shortname];
                }

                _harmony?.UnpatchAll(GetType().Name);
            }
            finally { ins = null; }

        }

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
                                PrintWarning("originalMethod is null!! patch.info.methodName: " + patch.info.methodName + System.Environment.NewLine + "patch.info.declaringType: " + patch.info.declaringType.FullName);
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
        #region Patch
        [HarmonyPatch(typeof(MixingTable), "StartMixing", new[] { typeof(BasePlayer) })]
        private class HookPatch
        {
            private static readonly MethodInfo _returnExcessItems = typeof(MixingTable).GetMethod("ReturnExcessItems", BindingFlags.Instance | BindingFlags.NonPublic);

            private static bool Prefix(BasePlayer player, MixingTable __instance)
            {
                if (__instance.IsOn())
                    return false;

                bool itemsAreContiguous;
                var orderedContainerItems = __instance.GetOrderedContainerItems(__instance.inventory, out itemsAreContiguous);

                int quantity;

                __instance.currentRecipe = RecipeDictionary.GetMatchingRecipeAndQuantity(__instance.Recipes, orderedContainerItems, out quantity);

                __instance.currentQuantity = quantity;

                if (__instance.currentRecipe == null || __instance.currentRecipe.RequiresBlueprint && !player.blueprints.HasUnlocked(__instance.currentRecipe.ProducedItem) || !itemsAreContiguous)
                    return false;

             
                __instance.lastTickTimestamp = Time.realtimeSinceStartup;


                var newTime = __instance.currentRecipe.MixingDuration * __instance.currentQuantity;

              //  Interface.Oxide.LogWarning("initial newTime: " + newTime);

                float rate;

                if (ins.IndividualRates.TryGetValue(__instance.currentRecipe.ProducedItem.displayName.english, out rate))
                {
                 //   Interface.Oxide.LogWarning("individual rate: " + rate);
                    newTime *= Mathf.Clamp(rate, 10f, float.MaxValue) / 100;
                }
                else
                {
                   // Interface.Oxide.LogWarning("NO individual rate. crafting rate: " + ins.CraftingRate + " ( / 100: " + (ins.CraftingRate / 100) + ")");
                    newTime *= ins.CraftingRate / 100;
                }

               // Interface.Oxide.LogWarning("TotalMixTime was: " + __instance.TotalMixTime + ", newTime: " + newTime);

                __instance.RemainingMixTime = newTime;
                __instance.TotalMixTime = newTime;

                _returnExcessItems.Invoke(__instance, new object[] { orderedContainerItems, player });


                if (__instance.RemainingMixTime <= 0.0) __instance.ProduceItem(__instance.currentRecipe, __instance.currentQuantity);
                else
                {
                    __instance.InvokeRepeating(new Action(__instance.TickMix), 1f, 1f);
                    __instance.SetFlag(BaseEntity.Flags.On, true, false, true);
                    __instance.SendNetworkUpdateImmediate(false);
                }

                return false;
            }
        }
        #endregion


        #region Helper methods

        private void SendChatMessage(BasePlayer player, string message, params object[] args) => player?.SendConsoleCommand("chat.add", string.Empty, -1, string.Format($"<color={ChatPrefixColor}>{ChatPrefix}</color>: {message}", args), 1.0);

        private T GetConfigValue<T>(string category, string setting, T defaultValue)
        {
            var data = Config[category] as Dictionary<string, object>;
            object value;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[category] = data;
                configChanged = true;
            }
            if (data.TryGetValue(setting, out value)) return (T)Convert.ChangeType(value, typeof(T));
            value = defaultValue;
            data[setting] = value;
            configChanged = true;
            return (T)Convert.ChangeType(value, typeof(T));
        }

        private void SetConfigValue<T>(string category, string setting, T newValue)
        {
            var data = Config[category] as Dictionary<string, object>;
            object value;
            if (data != null && data.TryGetValue(setting, out value))
            {
                value = newValue;
                data[setting] = value;
                configChanged = true;
            }
            SaveConfig();
        }

        #endregion
    }

}
