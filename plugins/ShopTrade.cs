using System.Collections.Generic;
using Oxide.Core.Plugins;
using System;
using UnityEngine;
using Network;
using Oxide.Core;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Facepunch;

namespace Oxide.Plugins
{
    [Info("ShopTrade", "Shady", "1.1.16", ResourceId = 0000)]
    internal class ShopTrade : RustPlugin
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
        public const float COOLDOWN_SECONDS = 60f;

        private readonly Vector3 _hackySetPos = new Vector3(0, 10, 0);

        private readonly FieldInfo _curList = typeof(InvokeHandler).GetField("curList", BindingFlags.Instance | BindingFlags.NonPublic);

        [PluginReference]
        private readonly Plugin NoEscape;

        private class TradeTimer
        {
            public Timer Timer { get; set; }
            public BasePlayer Player { get; set; }
            public BasePlayer Target { get; set; }
        }

        private readonly Dictionary<string, TradeTimer> tradeTimers = new Dictionary<string, TradeTimer>();
        private readonly Dictionary<BasePlayer, ShopFront> acceptDictionary = new Dictionary<BasePlayer, ShopFront>();
        private readonly Dictionary<string, string> tradingPlayers = new Dictionary<string, string>();
        private readonly Dictionary<string, float> lastTradeTime = new Dictionary<string, float>();

        private readonly HashSet<NetworkableId> ignoreStability = new HashSet<NetworkableId>();
        private readonly HashSet<string> accepting = new HashSet<string>();

        private MonumentInfo _outpost = null;
        public MonumentInfo Outpost
        {
            get
            {
                if (_outpost == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if ((mon?.displayPhrase?.english ?? string.Empty).Contains("outpost", CompareOptions.OrdinalIgnoreCase))
                        {
                            _outpost = mon;
                            break;
                        }
                    }
                }
                return _outpost;
            }
        }

        private MonumentInfo _bandit = null;
        public MonumentInfo BanditCamp
        {
            get
            {
                if (_bandit == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if ((mon?.displayPhrase?.english ?? string.Empty).Contains("bandit", CompareOptions.OrdinalIgnoreCase))
                        {
                            _bandit = mon;
                            break;
                        }
                    }
                }
                return _bandit;
            }
        }
        #region Commands
        [ChatCommand("trade")]
        private void cmdTrade(BasePlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                var tradeMsg = "----<color=#85bb65><size=22>TRADE</size></color>----\n\n-<color=#bee855>Starting a trade:</color> You can start a trade with another player by typing: <color=#8aff47>/trade <color=orange>PlayerName</color></color>.\n\n-<color=#f95ee7>Accepting and denying a trade request:</color> To accept a trade request from someone else, you must type <color=#8aff47>/tac</color>.\nTo deny a trade request, type <color=red>/tdc</color>\n\n-<color=#5edff9>Agreeing to a trade:</color> Once you've placed your items up and ready to agree to finish the trade, you must press and hold <size=18><color=#55b2e8>R</color></size> <size=10>(Reload key)</size> for a few seconds.\nOnce you've accepted, you can cancel by tapping <size=18><color=#55b2e8>R</color></size> once. When both players hold R and confirm the trade, the items will be traded.";
                SendReply(player, tradeMsg);
                return;
            }
            var lowerArg0 = args[0].ToLower();
            if (lowerArg0 == "accept")
            {
                cmdTradeAccept(player, "tac", args);
                return;
            }
            if (lowerArg0 == "decline" || lowerArg0 == "deny" || lowerArg0 == "cancel")
            {
                cmdTradeDecline(player, "tdc", args);
                return;
            }
            var target = FindPlayerByPartialName(args[0], false);
            if (target == null || target.IsDead())
            {
                SendReply(player, "No target or more than one found with: " + args[0]);
                return;
            }
            if (target == player)
            {
                SendReply(player, "You cannot trade yourself!");
                return;
            }
            if (HasTradeTimer(player.UserIDString))
            {
                SendReply(player, "Someone is already requesting a trade with you! You can cancel this with <color=red>/tdc</color>.");
                return;
            }
            if (HasTradeTimer(target.UserIDString))
            {
                SendReply(player, target.displayName + " is already requesting a trade with someone!");
                return;
            }
            var cooldownSec = GetTradeCooldownSeconds(player);
            var cooldownNeed = GetCoolDown(player.UserIDString);
            if (cooldownSec > 0.0 && cooldownSec < cooldownNeed)
            {
                var diff = cooldownNeed - cooldownSec;
                SendReply(player, "You cannot trade yet. Please wait another: " + diff.ToString(diff >= 1 ? "N0" : "0.0").Replace(".0", string.Empty) + " seconds");
                return;
            }
            var reason = string.Empty;
            if (!CanTrade(player, out reason))
            {
                if (!string.IsNullOrEmpty(reason)) SendReply(player, reason);
                else SendReply(player, "Couldn't trade, reason is unknown.");
                return;
            }
            var targName = target.displayName;
            var targID = target.UserIDString;
            var orgName = player.displayName;
            SendReply(player, "Sent trade request to: " + targName + System.Environment.NewLine + "You can cancel this request with <color=red>/tdc</color>");
            SendReply(target, orgName + " has requested to trade. Type <color=#8aff47>/tac</color> to accept & open a trade window, or <color=red>/tdc</color> to decline & cancel this request.");
            if (target.IsConnected) SendLocalEffect(target, "assets/bundled/prefabs/fx/invite_notice.prefab", target.CenterPoint());

            tradeTimers[target.UserIDString] = new TradeTimer
            {
                Player = player,
                Target = target,
                Timer = timer.Once(30f, () =>
                {
                    if (player != null && player.IsConnected) SendReply(player, targName + " did not accept your request in time!");
                    if (target != null && target.IsConnected) SendReply(target, "You did not accept " + orgName + "'s trade request in time!");
                    TradeTimer outTimer;
                    if (tradeTimers.TryGetValue(targID, out outTimer)) tradeTimers.Remove(targID);
                })
            };
        }


        [ChatCommand("tdc")]
        private void cmdTradeDecline(BasePlayer player, string command, string[] args)
        {
            var canceled = CancelAllTrades(player);
            if (canceled < 1) SendReply(player, "No trade has been requested.");
            else SendReply(player, "You <color=red>canceled</color> the trade.");
        }

        [ChatCommand("tac")]
        private void cmdTradeAccept(BasePlayer player, string command, string[] args)
        {
            TradeTimer outTimer;
            if (!tradeTimers.TryGetValue(player.UserIDString, out outTimer))
            {
                SendReply(player, "No one has requested to trade with you.");
                return;
            }
            var reason = string.Empty;
            if (!CanTrade(player, out reason))
            {
                if (!string.IsNullOrEmpty(reason)) SendReply(player, reason);
                else SendReply(player, "Couldn't trade, reason is unknown.");
                return;
            }
            if (outTimer.Timer != null)
            {
                outTimer.Timer.Destroy();
                outTimer.Timer = null;
            }
            var orgPlayer = outTimer.Player;
            if (orgPlayer == null || !orgPlayer.IsConnected)
            {
                SendReply(player, "The player who requsted this trade has disconnected.");
                return;
            }
            if (!CanTrade(orgPlayer, out reason))
            {
                if (!string.IsNullOrEmpty(reason))
                {
                    SendReply(orgPlayer, reason);
                    SendReply(player, orgPlayer.displayName + " couldn't trade. Reason: " + reason);
                }
                else
                {
                    SendReply(orgPlayer, "Couldn't trade, reason is unknown.");
                    SendReply(player, "Couldn't trade, reason is unknown.");
                }
                return;
            }
            if (accepting.Contains(player.UserIDString))
            {
                SendReply(player, "You're already accepting a trade! You can cancel this with <color=red>/tdc</color>.");
                return;
            }
            var cooldownSec = GetTradeCooldownSeconds(player);
            var cooldownNeed = GetCoolDown(player.UserIDString);
            if (cooldownSec > 0.0 && cooldownSec < cooldownNeed)
            {
                SendReply(player, "You cannot trade yet. Please wait another: " + (cooldownNeed - cooldownSec).ToString("N0") + " seconds");
                return;
            }
            var tradeStartStr = "Starting trade in <color=#8aff47>4</color> seconds... <color=orange>Close your chat window!</color>\nYou cancel this request with <color=red>/tdc</color>.";

            var closeChatToastMsg = "<size=20>Close your <i>chat window</i> or else the trade will</size> <size=22><i>fail!</i></size>";

            ShowToast(player, closeChatToastMsg, 1);
            ShowToast(orgPlayer, closeChatToastMsg, 1);

            SendReply(player, tradeStartStr);
            SendReply(orgPlayer, tradeStartStr);
            accepting.Add(player.UserIDString);
            var orgName = orgPlayer.displayName;
            var plyID = player.UserIDString;

            timer.Once(4f, () =>
            {
                if (accepting.Contains(plyID)) accepting.Remove(plyID);
                if (orgPlayer == null || !orgPlayer.IsConnected || orgPlayer.IsDead())
                {
                    if (player != null && player.IsConnected) SendReply(player, "The player who requested this trade has disconnected or died.");
                    return;
                }
                if (player == null || !player.IsConnected || player.IsDead())
                {
                    if (orgPlayer != null && orgPlayer.IsConnected) SendReply(orgPlayer, "The player who requested to trade with you has disconnected or died.");
                    return;
                }
                if (!tradeTimers.TryGetValue(player.UserIDString, out outTimer))
                {
                    //  SendReply(orgPlayer, plyName + " <color=red>canceled</color> the trade request.");
                    PrintWarning("no tradeTimer for: " + player.displayName + " (" + player.UserIDString + ") after 4 sec timer!");
                    return;
                }
                else tradeTimers.Remove(player.UserIDString);
                if (!CanTrade(orgPlayer, out reason))
                {
                    CancelTimer(outTimer);
                    SendReply(orgPlayer, reason);
                    SendReply(player, orgPlayer.displayName + " couldn't trade. Reason: " + reason);
                    return;
                }
                if (!CanTrade(player, out reason))
                {
                    CancelTimer(outTimer);
                    SendReply(player, reason);
                    SendReply(orgPlayer, player.displayName + " couldn't trade. Reason: " + reason);
                    return;
                }
                StartTrade(orgPlayer, player);
            });

        }
        #endregion
        #region Hooks
        private void OnShopCompleteTrade(ShopFront shop)
        {
            if (shop == null || shop.OwnerID != 757) return;
            var fx = shop?.transactionCompleteEffect?.resourcePath ?? string.Empty;

            var vendor = shop?.vendorPlayer;
            var customer = shop?.customerPlayer;
            
            if (!string.IsNullOrEmpty(fx))
            {
                if (vendor != null && vendor.IsConnected) SendLocalEffect(vendor, fx, vendor.CenterPoint());
                if (customer != null && customer.IsConnected) SendLocalEffect(customer, fx, customer.CenterPoint());
            }
          
            var now = Time.realtimeSinceStartup;

            if (vendor != null)
                lastTradeTime[vendor.UserIDString] = now;
            
            
            if (customer != null)
                lastTradeTime[customer.UserIDString] = now;
            

            if (vendor != null && customer != null && vendor.IsConnected && customer.IsConnected)
            {
                TradeHack(vendor, customer, customer.transform.position);
                TradeHack(customer, vendor, vendor.transform.position);
            }

            shop.Invoke(() =>
            {
                KillTradeShop(shop);

                vendor?.EndLooting();
                customer?.EndLooting();
            }, 0.125f);
        }

        private object OnEntityGroundMissing(BaseEntity entity)
        {
            if (entity == null || entity?.net == null) return null;
            if (ignoreStability.Contains(entity.net.ID)) return false;
            return null;
        }

        private object OnEntityVisibilityCheck(BaseEntity entity, BasePlayer player, uint id, string debugName, float maxDist)
        {
            if ((id == 1159607245U || id == 3168107540U) && entity is ShopFront && IsTrading(player))
                return true;

            return null;
        }

 
        private void OnPlayerLootEnd(PlayerLoot inventory)
        {
            if (inventory == null) return;

            var player = inventory.GetComponent<BasePlayer>();
            if (player == null) return;

            var plyName = player?.displayName ?? "Unknown";
            var userId = player?.UserIDString ?? string.Empty;

            var shop = inventory?.entitySource as ShopFront;
            if (shop == null || shop.IsDestroyed) return;

            var findPly = shop.vendorPlayer == player ? shop.customerPlayer : shop.vendorPlayer;
           

            NextTick(() =>
            {
                if (shop == null || shop.IsDestroyed || shop.OwnerID != 757) return;

                if (findPly != null && findPly.IsConnected)
                    SendReply(findPly, "Trade canceled by <color=orange>" + plyName + "</color>.");
                
                


                string tradeTarget;
                if (tradingPlayers.TryGetValue(userId, out tradeTarget)) tradingPlayers.Remove(userId);
                else if (findPly != null && tradingPlayers.TryGetValue(findPly.UserIDString, out tradeTarget)) tradingPlayers.Remove(findPly.UserIDString);

                if (findPly != null && player != null && findPly.IsConnected && player.IsConnected)
                {
                    VanishAndReappear(findPly);
                    VanishAndReappear(player);
                    PrintWarning("trade was cancel, running tradehacks");
                }
                else PrintWarning("couldn't run trade hacks on a cancel?!");

                KillTradeShop(shop);
            });
        }

        private void Unload()
        {
            foreach(var entity in BaseNetworkable.serverEntities)
            {
                var shop = entity as ShopFront;
                if (shop?.OwnerID == 757) KillTradeShop(shop);
            }
        }
        #endregion
        #region Util
        private void ShowToast(BasePlayer player, string message, int type = 0)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.IsConnected)
                return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                player.SendConsoleCommand(sb.Clear().Append("gametip.showtoast ").Append(type).Append(" \"").Append(message).Append("\"").ToString());
            }
            finally { Pool.Free(ref sb); }
        }

        private void VanishAndReappear(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null || !player.IsConnected) return;

            PrintWarning(nameof(VanishAndReappear) + " : " + player);

            var connections = Facepunch.Pool.GetList<Connection>();
            try
            {

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var target = BasePlayer.activePlayerList[i];
                    if (target == null || target == player || !target.IsConnected) continue;

                    connections.Add(target.net.connection);


                }

                var netWrite = Net.sv.StartWrite();

                netWrite.PacketID(Network.Message.Type.EntityDestroy);
                netWrite.EntityID(player.net.ID);
                netWrite.UInt8((byte)BaseNetworkable.DestroyMode.None);
                netWrite.Send(new SendInfo(connections));
            }
            finally { Facepunch.Pool.FreeList(ref connections); }

            player.Invoke(() =>
            {
                if (player != null && player.IsConnected) player.SendNetworkUpdate();
            }, 0.5f);
        }

        public float GetCoolDown(string userID = "")
        {
            return COOLDOWN_SECONDS * (IsVIP(userID) ? 0.33f : IsMVP(userID) ? 0.66f : 1f);
        }

        private TradeTimer GetTradeTimer(BasePlayer player)
        {
            if (player == null) return null;
            TradeTimer outTimer;
            if (tradeTimers.TryGetValue(player.UserIDString, out outTimer) && outTimer != null) return outTimer;
            foreach(var kvp in tradeTimers)
            {
                if (kvp.Value.Player == player) return kvp.Value;
            }
            return null;
        }

        private List<TradeTimer> GetTradeTimers(BasePlayer player)
        {
            if (player == null) return null;
            TradeTimer outTimer;
            var trades = new List<TradeTimer>();
            if (tradeTimers.TryGetValue(player.UserIDString, out outTimer)) trades.Add(outTimer);
            foreach(var kvp in tradeTimers)
            {
                if (kvp.Value.Player == player) trades.Add(kvp.Value);
            }
            return trades;
        }
        
        private void GetTradeTimersNoAlloc(BasePlayer player, ref List<TradeTimer> list)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (list == null) throw new ArgumentNullException(nameof(list));

            TradeTimer outTimer;
            if (tradeTimers.TryGetValue(player.UserIDString, out outTimer)) list.Add(outTimer);

            foreach (var kvp in tradeTimers)
            {
                if (kvp.Value.Player == player) list.Add(kvp.Value);
            }
            
        }

        private int CancelAllTrades(BasePlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var trades = Facepunch.Pool.GetList<TradeTimer>();
            try 
            {
                GetTradeTimersNoAlloc(player, ref trades);
                for (int i = 0; i < trades.Count; i++) CancelTimer(trades[i], player);
                return trades.Count;
            }
            finally { Facepunch.Pool.FreeList(ref trades); }
        }

        private bool IsTrading(BasePlayer player)
        {
            if (player == null) return false;
            string targetId;
          
            var uid = player?.UserIDString ?? string.Empty;
            if (tradingPlayers.TryGetValue(uid, out targetId)) return true;

            foreach (var kvp in tradingPlayers)
            {
                if (kvp.Key == uid || kvp.Value == uid) return true;
            }

            return false;
        }

        private bool IsMVP(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            var perms = permission.GetUserPermissions(userID);
            return perms.Length < 1 ? false : string.Join(" ", perms).Contains(".mvp");
        }

        private bool IsVIP(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            var perms = permission.GetUserPermissions(userID);
            if (perms.Length < 1) return false;
            return perms.Length < 1 ? false : string.Join(" ", perms).Contains(".vip");
        }

        private float GetTradeCooldownSeconds(BasePlayer player)
        {
            if (player == null) return -1f;
            float time;
            if (!lastTradeTime.TryGetValue(player.UserIDString, out time)) return -1f;
            return Time.realtimeSinceStartup - time;
        }
        private bool IsFalling(BasePlayer player)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return false;
            var input = player?.serverInput ?? null;
            if (input == null) return false;
            if (!player.IsOnGround())
            {
                if (input.WasJustPressed(BUTTON.JUMP) || input.WasJustReleased(BUTTON.JUMP)) return false;
                return true;
            }
            return false;
        }
        private bool IsHostile(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || !player.IsHostile()) return false;
            var pos = player?.transform?.position ?? Vector3.zero;
            if (pos == Vector3.zero) return false;
            if (BanditCamp?.gameObject != null && Vector3.Distance(pos, BanditCamp.transform.position) <= 125) return true;
            if (Outpost?.gameObject != null && Vector3.Distance(pos, Outpost.transform.position) <= 125) return true;
            return false;
        }
        private bool CanTrade(BasePlayer player, out string reason)
        {
            reason = string.Empty;
            if (player == null || !player.IsConnected || player.IsDead()) return false;
            var canTrade = Interface.Oxide.CallHook("CanPlayerTrade", player);
            if (canTrade != null)
            {
                var str = canTrade as string;
                if (!string.IsNullOrEmpty(str)) reason = str;
                return false;
            }
         
            if ((player?.metabolism?.radiation_level?.value ?? 0) > 1.5 || (player?.metabolism?.radiation_poison?.value ?? 0) > 1.5)
            {
                reason = "Radiation is too high to trade!";
                return false;
            }
            if ((player?.metabolism?.bleeding?.value ?? 0) > 0)
            {
                reason = "Cannot trade while bleeding!";
                return false;
            }
            if (player?.metabolism == null)
            {
                reason = "No metabolism!";
                return false;
            }
            if (IsEscapeBlocked(player.UserIDString))
            {
                reason = "Cannot trade while combat/raid blocked!";
                return false;
            }
            if (!player.CanBuild())
            {
                reason = "Cannot trade while building blocked!";
                return false;
            }
            if (player.isMounted)
            {
                reason = "Cannot trade while sitting!";
                return false;
            }
            if (IsCrafting(player))
            {
                reason = "Cannot trade while crafting!";
                return false;
            }
            if (player.IsWounded())
            {
                reason = "Cannot trade while wounded!";
                return false;
            }
            if (player.IsSwimming() || player.IsHeadUnderwater())
            {
                reason = "Cannot trade while swimming!";
                return false;
            }
            if (IsFalling(player) && player.IsAdmin && !player.IsFlying)
            {
                reason = "Cannot trade while falling!";
                return false;
            }
            if (player.IsSpectating())
            {
                reason = "Cannot trade while spectating!";
                return false;
            }
            if (player.IsSleeping())
            {
                reason = "Cannot trade while sleeping!";
                return false;
            }
            if (player.OnLadder())
            {
                reason = "Cannot trade while on a ladder!";
                return false;
            }
            if (player.metabolism.temperature.value < 0)
            {
                reason = "Cannot trade while cold!";
                return false;
            }
            if (player.metabolism.temperature.value > 45)
            {
                reason = "Cannot trade while hot!";
                return false;
            }
            if (player.metabolism.calories.value <= 5)
            {
                reason = "Cannot trade while starving!";
                return false;
            }
            if (player.metabolism.hydration.value <= 5)
            {
                reason = "Cannot trade while dehydrated!";
                return false;
            }
            var parent = player?.GetParentEntity();
            if (parent != null)
            {
                reason = parent?.prefabID == 3234960997 ? "Cannot trade while on cargoship!" : "Cannot trade while parented!";
                return false;
            }
            if (IsHostile(player))
            {
                reason = "Cannot trade while hostile!";
                return false;
            }
            var lastAtk = player?.lastAttackedTime ?? -1f;
            var atkCooldown = 30;
            var lastDmg = player?.lastDamage ?? Rust.DamageType.Generic;
            if (((Time.realtimeSinceStartup - lastAtk) < atkCooldown) && lastDmg != Rust.DamageType.Generic && lastDmg != Rust.DamageType.Cold && lastDmg != Rust.DamageType.ColdExposure)
            {
                var sb = Facepunch.Pool.Get<StringBuilder>();
                try 
                {
                    reason = sb.Append("Damage taken within the last").Append(atkCooldown.ToString("N0")).Append(" seconds! Please wait.").ToString();
                    return false;
                }
                finally { Facepunch.Pool.Free(ref sb); }
            }
            var heliIns = PatrolHelicopterAI.heliInstance;
            if (heliIns != null && heliIns?.helicopterBase != null && !heliIns.isDead && !heliIns.helicopterBase.IsDead() && !heliIns.helicopterBase.IsDestroyed)
            {
                var isTarget = false;
                var targetList = PatrolHelicopterAI.heliInstance?._targetList ?? null;
                if (targetList != null && targetList.Count > 0)
                {
                    var uidStr = player.UserIDString;
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        var target = targetList[i];
                        if (target.ply != null && target.ply.UserIDString.Equals(uidStr, StringComparison.OrdinalIgnoreCase))
                        {
                            isTarget = true;
                            break;
                        }
                    }
                }

                if (isTarget)
                {
                    reason = "Cannot trade while being targeted by a helicopter.";
                    return false;
                }
            }
            return true;
        }

        private void StartTrade(BasePlayer player, BasePlayer target)
        {
            if (player == null || target == null || player.IsDead() || target.IsDead()) return;

            tradeTimers.Remove(player.UserIDString);
            tradeTimers.Remove(target.UserIDString);

            var shop = (ShopFront)GameManager.server.CreateEntity("assets/bundled/prefabs/static/wall.frame.shopfront.metal.static.prefab", new Vector3(5, 2, 5));
            if (shop == null) return;

            var parent1 = player.GetParentEntity();
            var parent2 = target.GetParentEntity();

            var parent1Needed = parent1 != null && !parent1.IsDestroyed;
            var parent2Needed = parent2 != null && !parent2.IsDestroyed;


            var stab = shop?.GetComponent<StabilityEntity>() ?? null;
            if (stab != null) stab.grounded = true;

            shop.globalBroadcast = true;
            shop.enableSaving = false;
            shop.Spawn();
            shop.enableSaving = false;

            shop.globalBroadcast = true;
            shop.OwnerID = 757;

            ignoreStability.Add(shop.net.ID);
           
            var targID = target.UserIDString;
            var orgID = player.UserIDString;
            tradingPlayers[player.UserIDString] = target.UserIDString;
            tradingPlayers[target.UserIDString] = player.UserIDString;

            var targPos = target?.transform?.position ?? Vector3.zero;
            var orgPos = player?.transform?.position ?? Vector3.zero;
            var dist = Vector3.Distance(orgPos, targPos);
            if (dist >= 300)
            {
                TradeHack(player, target, _hackySetPos);
                TradeHack(target, player, _hackySetPos);
                StopHostile(player);
                StopHostile(target);
            }


            timer.Once(0.25f, () =>
            {
                StartLootingShop(shop, player, false);
                StartLootingShop(shop, target, true);


                if (dist >= 300)
                {
                    timer.Once(0.1f, () =>
                    {
                        TeleportPlayer(target, targPos, false, false, false, false);
                        TeleportPlayer(player, orgPos, false, false, false, false);
                        NextTick(() =>
                        {
                            if (parent1Needed)
                            {
                                if (parent1 != null && !parent1.IsDestroyed)
                                {
                                    player.SetParent(parent1, true, true);
                                    PrintWarning("re-set parent1 after trade teleport");
                                }
                                else PrintWarning("parent1 is null or destroyed now!");
                            }

                            if (parent2Needed)
                            {
                                if (parent2 != null && !parent2.IsDestroyed)
                                {
                                    target.SetParent(parent2, true, true);
                                    PrintWarning("re-set parent2 after trade teleport");
                                }
                                else PrintWarning("parent2 is null or destroyed now!");
                            }
                             
                            StopHostile(player);
                            StopHostile(target);
                        });
                    });
                }
                StopHostile(player);
                StopHostile(target);
            });
        }

        private void CancelTimer(TradeTimer timer, BasePlayer cancel = null)
        {
            if (timer == null) return;
            var findKey = string.Empty;
            foreach(var kvp in tradeTimers)
            {
                if (kvp.Value == timer)
                {
                    findKey = kvp.Key;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(findKey)) tradeTimers.Remove(findKey);
            var sendPly = (cancel == null) ? null : timer.Player == cancel ? timer.Target : timer.Target == cancel ? timer.Player : null;
            if (sendPly != null && sendPly.IsConnected) SendReply(sendPly, cancel.displayName + " <color=red>canceled</color> the trade request.");
            if (timer.Timer != null)
            {
                timer.Timer.Destroy();
                timer.Timer = null;
            }
        }
        private bool HasTradeTimer(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            TradeTimer outTimer;
            return tradeTimers.TryGetValue(userID, out outTimer);
        }

        /*/
        private bool? _wasEasy;
        private bool IsEasyServer()
        {
            if (_wasEasy.HasValue) return (bool)_wasEasy;
            var val = ConVar.Server.hostname.Contains("#2") || ConVar.Server.hostname.Contains("#3");
            _wasEasy = val;
            return val;
        }/*/
        private bool IsCrafting(BasePlayer player) { return (player?.inventory?.crafting?.queue?.Count ?? 0) > 0; }

        private bool IsEscapeBlocked(string userID) { return string.IsNullOrEmpty(userID) ? false : NoEscape?.Call<bool>("IsEscapeBlockedS", userID) ?? false; }

        private void StopHostile(BasePlayer player)
        {
            if (player == null) return;
            player.State.unHostileTimestamp = TimeEx.currentTimestamp;
            player.ClientRPCPlayer(null, player, "SetHostileLength", 0);
        }

        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true, bool respawnIfDead = false, bool endLooting = true)
        {
            if (player == null || player?.transform == null) return false;

            var playerPos = player?.transform?.position ?? Vector3.zero;
            var isConnected = player?.IsConnected ?? false;

            if (respawnIfDead && player.IsDead())
            {
                player.RespawnAt(dest, Quaternion.identity);
                return true;
            }

            if (endLooting) player.EndLooting(); //redundant?
            player.EnsureDismounted();
            player.UpdateActiveItem(new ItemId(0));
            player.Server_CancelGesture();

            if (player.HasParent()) player.SetParent(null, true); //if check redundant?

            var distFrom = Vector3.Distance(playerPos, dest);

            if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
            if (doSleep && isConnected) player.StartSleeping();

            player.RemoveFromTriggers();
            player.MovePosition(dest);

            if (isConnected)
            {
                if (doSleep)
                {
                    player.inventory.crafting.CancelAll(true);
                    CancelInvoke("InventoryUpdate", player); //this is safe to call without checking IsInvoking
                }

                player.ClientRPCPlayer(null, player, "ForcePositionTo", dest);

                if (distFrom >= 250 && distChecks) player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);

                player.UpdateNetworkGroup();
                player.SendEntityUpdate();
                player.SendNetworkUpdateImmediate();

                if (distFrom > 100)
                {
                    player?.ClearEntityQueue(null);
                    player.SendFullSnapshot();
                }
            }

            player.ForceUpdateTriggers();

            return true;
        }
        
        /*/
        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true, bool respawnIfDead = false)
        {
            if (player == null || player?.transform == null) return false;
            var playerPos = player?.transform?.position ?? Vector3.zero;
            var isConnected = player?.IsConnected ?? false;
            if (respawnIfDead && player.IsDead())
            {
                player.RespawnAt(dest, Quaternion.identity);
                return true;
            }
          //  player.EndLooting(); //redundant? - DO NOT USE FOR TRADE
            player.EnsureDismounted();
            player.SetParent(null, true);
            var distFrom = Vector3.Distance(playerPos, dest);

            if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
            if (doSleep && isConnected) player.StartSleeping();
            player.MovePosition(dest);
            if (isConnected)
            {
                if (doSleep)
                {
                    player.inventory.crafting.CancelAll(true);
                    CancelInvoke("InventoryUpdate", player); //this is safe to call without checking IsInvoking
                }
                player.ClientRPCPlayer(null, player, "ForcePositionTo", dest);
                if (distFrom >= 250 && distChecks) player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
                player.UpdateNetworkGroup();
                player.SendEntityUpdate();
                player.SendNetworkUpdateImmediate();
                if (distFrom > 100)
                {
                    player?.ClearEntityQueue(null);
                    player.SendFullSnapshot();
                }
            }
            return true;
        }/*/

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

        private object TradeHack(BasePlayer target, BasePlayer player, Vector3 setPos = default(Vector3))
        {
            if (target == null || player == null) return null;
            try
            {
                var connection = target?.net?.connection ?? null;
                if (connection != null)
                {
                    var netWrite = Net.sv.StartWrite();
                    connection.validate.entityUpdates += 1;
                    BaseNetworkable.SaveInfo saveInfo = new BaseNetworkable.SaveInfo
                    {
                        forConnection = connection,
                        forDisk = false
                    };
                    netWrite.PacketID(Message.Type.Entities);
                    netWrite.UInt32(target.net.connection.validate.entityUpdates);
                    using (saveInfo.msg = Facepunch.Pool.Get<ProtoBuf.Entity>())
                    {
                        player.Save(saveInfo);
                        if (saveInfo.msg.baseEntity == null) PrintError(Name + ": ToStream - no BaseEntity!?");
                        if (saveInfo.msg.baseNetworkable == null) PrintError(Name + ": ToStream - no baseNetworkable!?");
                        saveInfo.msg.baseEntity.pos = (setPos != Vector3.zero) ? setPos : (target?.transform?.position ?? Vector3.zero);

                        saveInfo.msg.ToProto(netWrite);
                        player.PostSave(saveInfo);
                        netWrite.Send(new SendInfo(connection));
                    }
                    return true;
                }
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return null;
        }

        private void StartLootingShop(ShopFront shop, BasePlayer player, bool customer)
        {
            if (shop == null || shop.IsDestroyed || player == null || player.IsDead() || !player.IsConnected || player.IsSleeping()) return;
            if (player?.inventory?.loot == null) return;
            player.inventory.loot.StartLootingEntity(shop, false);
            player.inventory.loot.AddContainer(shop.vendorInventory);
            player.inventory.loot.AddContainer(shop.customerInventory);
            player.inventory.loot.SendImmediate();

            if (customer) shop.customerPlayer = player;
            else shop.vendorPlayer = player;

            player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", "shopfront");
            shop.ResetTrade();
            shop.UpdatePlayers();
            acceptDictionary[player] = shop;
        }

        private void KillTradeShop(ShopFront shop)
        {
            if (shop == null || shop.IsDestroyed) return;

            //these looters will only ever exist if they never had EndLooting called, which a normal trade completion should


            BasePlayer firstPlayer = null;
            BasePlayer secondPlayer = null;

            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var ply = BasePlayer.activePlayerList[i];

                if (ply?.inventory?.loot?.entitySource == shop)
                {
                    if (firstPlayer == null) firstPlayer = ply;
                    else if (secondPlayer == null) secondPlayer = ply;
                    
                    shop.PlayerStoppedLooting(ply);
                }
            }


            if (firstPlayer != null && secondPlayer != null && firstPlayer.IsConnected && secondPlayer.IsConnected)
            {
                PrintWarning("reappear players after trade to fix map position");
                VanishAndReappear(firstPlayer);
                VanishAndReappear(secondPlayer);
            }
            else PrintWarning("trade hacks couldn't be called as first or second player is null/disconnected (if this was a cancel, this is not an issue as this is handled elsewhere)");

            shop.Kill();
        }
    
        private object CanMoveItem(Item item, PlayerInventory inv, ItemContainerId num, int num2, int num3)
        {
            if (item == null || inv == null) return null;

            var player = inv?.GetComponent<BasePlayer>() ?? null;
            if (player == null) return null;

            var shop = (player?.inventory?.loot?.entitySource ?? null) as ShopFront;
            if (shop == null) return null;

            var itemContainer = inv.FindContainer(num);
            if (itemContainer == null) return null;

            if (shop?.vendorPlayer != player && shop?.customerPlayer != player)
            {
                PrintWarning("Player tried to put item in shopfront but is not customer or vendor?!!! Player: " + player?.displayName + "(" + player?.UserIDString + ")");
                return null;
            }

            var pContainer = shop?.vendorPlayer == player ? shop.vendorInventory : shop?.customerPlayer == player ? shop.customerInventory : null;

            if (pContainer != null)
            {

                if (pContainer != itemContainer && itemContainer != player?.inventory?.containerMain && itemContainer != player?.inventory?.containerBelt&& itemContainer != player?.inventory?.containerWear)
                {
                    //PrintWarning("tried to put item into container that isn't theirs in shop front, returning false!");
                    var cancelFX = "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab";
                    SendLocalEffect(player, cancelFX, player?.eyes?.position ?? Vector3.zero);
                    return false;
                }
            }
            else PrintWarning("pContainer is null on ShopFront?! player: " + player?.displayName);

            return null;
        }

        public const string VALID_CHARACTERS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=[]{}\|\'\"",./`*!@#$%^&*()_+<>?~;: "; //extra space at end is to allow space as a valid character

        public string[] GetValidCharactersString()
        {
            var length = VALID_CHARACTERS.Length;
            var array = new string[length];
            for (int i = 0; i < length; i++) array[i] = VALID_CHARACTERS[i].ToString();
            return array;
        }

        public char[] GetValidCharacters() { return VALID_CHARACTERS.ToCharArray(); }

        private int Fitness(string individual, string target)
        {
            return Enumerable.Range(0, Math.Min(individual.Length, target.Length)).Count(i => individual[i] == target[i]);
        }

        private int ExactMatch(string comp1, string comp2, StringComparison options = StringComparison.CurrentCulture)
        {
            if (string.IsNullOrEmpty(comp1) || string.IsNullOrEmpty(comp2)) return 0;
            var val = 0;


            if (comp1.Length > 0 && comp2.Length > 0)
            {
                for (int i = 0; i < comp1.Length; i++)
                {
                    if ((comp2.Length - 1) >= i && comp2[i].ToString().Equals(comp1[i].ToString(), options)) val++;
                }
            }

            return val;
        }

        private string CleanPlayerName(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            var strSB = new StringBuilder();
            var valid = GetValidCharactersString();
            for (int i = 0; i < str.Length; i++)
            {
                var chrStr = str[i].ToString();
                var skip = true;
                for (int j = 0; j < valid.Length; j++)
                {
                    var v = valid[j];
                    if (v.Equals(chrStr, StringComparison.OrdinalIgnoreCase))
                    {
                        skip = false;
                        break;
                    }
                }
                if (!skip) strSB.Append(chrStr);
            }
            return strSB.ToString().TrimStart().TrimEnd();
        }

        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            BasePlayer player = null;
            var matches = Facepunch.Pool.GetList<BasePlayer>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }

                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(p);
                    }
                }
                if (sleepers)
                {

                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matches.Add(p);
                        }
                    }
                }
                var topMatch = matches?.OrderByDescending(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name, StringComparison.OrdinalIgnoreCase)) ?? null;
                if (topMatch != null && topMatch.Any())
                {
                    var exactMatches = matches?.Select(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name, StringComparison.OrdinalIgnoreCase))?.OrderByDescending(p => p) ?? null;
                    if (exactMatches.All(p => p == 0))
                    {
                        topMatch = matches?.OrderByDescending(p => Fitness(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                    }
                }
                player = topMatch?.FirstOrDefault() ?? null;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            Facepunch.Pool.FreeList(ref matches);
            return player;
        }

        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false, params BasePlayer[] ignore)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            BasePlayer player = null;
            var matches = Facepunch.Pool.GetList<BasePlayer>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || (ignore != null && ignore.Length > 0 && ignore.Contains(p))) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }

                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null || (ignore != null && ignore.Length > 0 && ignore.Contains(p))) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || ignore.Contains(p)) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(p);
                    }
                }
                if (sleepers)
                {

                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null || ignore.Contains(p)) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matches.Add(p);
                        }
                    }
                }
                var topMatch = matches?.OrderByDescending(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                if (topMatch != null && topMatch.Any())
                {
                    var exactMatches = matches?.Select(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name))?.OrderByDescending(p => p) ?? null;
                    if (exactMatches.All(p => p == 0))
                    {
                        topMatch = matches?.OrderByDescending(p => Fitness(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                    }
                }
                player = topMatch?.FirstOrDefault() ?? null;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            Facepunch.Pool.FreeList(ref matches);
            return player;
        }

        private ListDictionary<InvokeAction, float> _invokeList = null;

        private ListDictionary<InvokeAction, float> InvokeList
        {
            get
            {
                if (_invokeList == null && InvokeHandler.Instance != null) _invokeList = _curList.GetValue(InvokeHandler.Instance) as ListDictionary<InvokeAction, float>;
                return _invokeList;
            }
        }

        private void CancelInvoke(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null).ToString() == targetName && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private void CancelInvoke(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName) || obj == null) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null) == obj && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private bool IsInvoking(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null) == obj) return true;
                }
            }
            return false;
        }
        private bool IsInvoking(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null).ToString() == targetName) return true;
                }
            }
            return false;
        }

        #endregion
    }
}