using ConVar;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Nightly Fire Sales", "Shady", "0.0.1")]
    internal class NightlyFireSales : RustPlugin
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
        #region Constants

        private const bool YOU_MATTER = true;

        private const string CHAT_STEAM_ID = "76561198865536053";

        #endregion

        private readonly HashSet<Coroutine> _coroutines = new();

        private readonly HashSet<string> _forbiddenTags = new() { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        #endregion
        #region Commands
        [ChatCommand("fss")]
        private void cmdFireTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            BeginFireSale();
            SendReply(player, "Started fire sale!");

        }

        [ChatCommand("fse")]
        private void cmdFireTestEnd(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            EndFireSale();
            SendReply(player, "Ended fire sale!");

        }
        #endregion
        #region Hooks

        private readonly HashSet<NPCVendingMachine> _vendingMachines = new();

        private void OnStartNight()
        {
            PrintWarning(nameof(OnStartNight));

            BeginFireSale();
            AnnounceFireSaleStarted();

        }

        private void OnStartMorning()
        {
            PrintWarning(nameof(OnStartMorning));
            EndFireSale();
            AnnounceFireSaleEnded();
        }

        private readonly Dictionary<uint, Dictionary<ProtoBuf.VendingMachine.SellOrder, int>> _preSaleCurrencyAmountPerItem = new();


        public bool IsFireSaleActive { get; private set; } = false;

        private void BeginFireSale()
        {
            

            try
            {
                if (IsFireSaleActive)
                    EndFireSale();

                PrintWarning("fire sale starting!");

                _preSaleCurrencyAmountPerItem.Clear();

                foreach (var vm in _vendingMachines)
                {
                    if (vm == null || vm.IsDestroyed)
                        continue;

                    if (!_preSaleCurrencyAmountPerItem.TryGetValue(vm.prefabID, out _))
                        _preSaleCurrencyAmountPerItem[vm.prefabID] = new Dictionary<ProtoBuf.VendingMachine.SellOrder, int>();
                    else PrintWarning("duplicate key found (prefabID) (this may not be an issue unless they have the same orders, with separate prices, for same prefab ID, but that seems unlikely): " + vm.prefabID);

                    for (int i = 0; i < vm.sellOrders.sellOrders.Count; i++)
                    {
                        var order = vm.sellOrders.sellOrders[i];

                        if (!_preSaleCurrencyAmountPerItem[vm.prefabID].TryGetValue(order, out _))
                            _preSaleCurrencyAmountPerItem[vm.prefabID][order] = order.currencyAmountPerItem;


                        order.currencyAmountPerItem = Mathf.Clamp((int)Math.Round(order.currencyAmountPerItem * 0.5d, MidpointRounding.AwayFromZero), 1, int.MaxValue);
                    }


                    vm.RefreshSellOrderStockLevel();
                    vm.RefreshAndSendNetworkUpdate();
                    vm.FullUpdate();

                }

                PrintWarning("fire sale started!");
            }
            finally { IsFireSaleActive = true; }
        }

        private void EndFireSale()
        {

            try
            {
                PrintWarning("fire sale ending!");

                foreach (var vm in _vendingMachines)
                {
                    if (vm == null || vm.IsDestroyed)
                        continue;

                    if (!_preSaleCurrencyAmountPerItem.TryGetValue(vm.prefabID, out var amounts))
                        continue;

                    for (int i = 0; i < vm.sellOrders.sellOrders.Count; i++)
                    {
                        var order = vm.sellOrders.sellOrders[i];

                        if (!amounts.TryGetValue(order, out var curAmount))
                            continue;

                        order.currencyAmountPerItem = curAmount;
                    }


                    vm.RefreshSellOrderStockLevel();
                    vm.RefreshAndSendNetworkUpdate();
                    vm.FullUpdate();

                }

                PrintWarning("fire sale ended!");
            }
            finally
            {
                IsFireSaleActive = false;
            }
         
        }

        private void OnServerInitialized(bool init)
        {
           
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                if (entity is not NPCVendingMachine vm || entity is NPCTalking)
                    continue;

                _vendingMachines.Add(vm);

            }

            PrintWarning(nameof(OnServerInitialized) + " found " + _vendingMachines.Count.ToString("N0") + " NPC vending machines.");
           
        }

        private void Unload()
        {
            EndFireSale();
        }

        private void AnnounceFireSaleStarted()
        {
            SimpleBroadcast("<size=25><color=#870da5><i>Nightly</i></color> <color=red>FIRE SALE!</color></size></color>\nAll NPC Vendors prices have been <color=orange>SLASHED</color> in <i>HALF.</i>", CHAT_STEAM_ID);
        }

        private void AnnounceFireSaleEnded()
        {
            SimpleBroadcast("Fire sale over! See you again at <color=#870da5><i>night!</i></color>", CHAT_STEAM_ID);
        }


        /*/
        private object OnBuyVendingItem(VendingMachine vending, BasePlayer player, int sellOrder, int numberOfTransactions)
        {
            if (_order == vending.sellOrders.sellOrders[sellOrder])
            {

                var freeMainSlots = 0;

                var mainCap = player?.inventory?.containerMain?.capacity ?? 0;

                for (var i = 0; i < mainCap; i++)
                {
                    var findItem = player?.inventory?.containerMain?.GetSlot(i);
                    if (findItem == null)
                    {
                        freeMainSlots++;
                    }
                }


                if (freeMainSlots < 5)
                {
                    SendReply(player, "<color=red>You do not have enough inventory space to complete this transaction!</color>");
                    return true;
                }
            }
            return null;
        }

        private object OnNpcGiveSoldItem(NPCVendingMachine vending, Item item, BasePlayer player)
        {
            if (vending == null || item == null || player == null) return null;

            if (item?.info?.itemid == NOTE_ITEM_ID && vending.sellOrders.sellOrders.Contains(_order))
            {
                if (Luck == null || !Luck.IsLoaded)
                {
                    PrintError("Failed to find Luck on OnNpcGiveSoldItem!");
                    return null;
                }
                var charm = Luck?.Call<Item>("CreateRandomCharm", false, false, false, false, UnityEngine.Random.Range(1.75f, 2.971221f)) ?? null;
                if (charm == null)
                {
                    PrintError("GOT NULL CHARM FROM CreateRandomCharm!!!");
                    return null;
                }

                RemoveFromWorld(item);

                if (!charm.MoveToContainer(player.inventory.containerMain)) RemoveFromWorld(charm);
                else CelebrateEffects(player);
                    
                return true;
            }

            return null;
        }
        /*/
        #endregion
        #region Util

        private Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var coroutine = ServerMgr.Instance.StartCoroutine(routine);

            _coroutines.Add(coroutine);

            return coroutine;
        }

        private void StopCoroutine(Coroutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            ServerMgr.Instance.StopCoroutine(routine);

            _coroutines.Remove(routine);
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;


            //	Replace Color Tags
            phrase = _colorRegex.Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = _sizeRegex.Replace(phrase, string.Empty);

            var phraseSB = Pool.Get<StringBuilder>();
            try
            {
                phraseSB.Clear().Append(phrase);

                foreach (var tag in _forbiddenTags)
                    phraseSB.Replace(tag, string.Empty);

                return phraseSB.ToString();
            }
            finally { Pool.Free(ref phraseSB); }
        }

        private void SimpleBroadcast(string msg, string userID = "")
        {
            if (string.IsNullOrEmpty(msg))
                return;

            Chat.ChatEntry chatEntry = new()
            {
                Channel = Chat.ChatChannel.Server,
                Message = RemoveTags(msg),
                UserId = userID.ToString(),
                Time = Facepunch.Math.Epoch.Current
            };

            Facepunch.RCon.Broadcast(Facepunch.RCon.LogType.Chat, chatEntry);

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player == null || !player.IsConnected) continue;

                if (!string.IsNullOrWhiteSpace(userID)) player.SendConsoleCommand("chat.add", string.Empty, userID, msg);
                else SendReply(player, msg);
            }

        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using var fx = new Effect(effect, pos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using var fx = new Effect(effect, player, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void CelebrateEffects(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || player.transform == null) return;

            var eyePos = player?.eyes?.transform?.position ?? Vector3.zero;
            var eyeForward = player?.eyes?.HeadForward() ?? Vector3.zero;

            for (var j = 0; j < UnityEngine.Random.Range(4, 10); j++)
            {
                Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", SpreadVector2(eyePos + eyeForward * UnityEngine.Random.Range(1.5f, 1.9f), UnityEngine.Random.Range(1.35f, 2.2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/bonus_hit.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + UnityEngine.Random.Range(0.5f, 1f), eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(0.8f, 2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/hotspot_death.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + 0.6f, eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(1.1f, 3f)), Vector3.zero);
            }

            SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");

            player.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
        }

        private void SendReply(IPlayer player, string msg, string userId = CHAT_STEAM_ID, bool keepTagsConsole = false)
        {
            if (player == null) return;

            msg = !player.IsServer ? msg : keepTagsConsole ? msg : RemoveTags(msg);

            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else
            {
#if RUST
                player.Command("chat.add", string.Empty, userId, msg);
#else
                player.Reply(msg);
#endif
            }
        }

        private void SendReply(BasePlayer player, string msg, string userId = CHAT_STEAM_ID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
        }

        #endregion
    }
}