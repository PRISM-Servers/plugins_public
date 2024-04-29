// Reference: System.Net.Http
using CompanionServer;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Rust+ Extended", "Shady", "1.0.4")]
    internal class RustPlusExtended : RustPlugin
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
        //internal todo:

        //add chat command to customize notifications - will be controlled by 'notificationName', i.e: LEGENADRY_CARGO_SPAWN -> can be translated to a string in /cmd to control

        private readonly MethodInfo _sendNotificationImpl = typeof(NotificationList).GetMethod("SendNotificationImpl", BindingFlags.NonPublic | BindingFlags.Static);

        private readonly FieldInfo _httpClient = typeof(NotificationList).GetField("Http", BindingFlags.NonPublic | BindingFlags.Static);

        private readonly HashSet<Coroutine> _coroutines = new HashSet<Coroutine>();

        private Dictionary<ulong, List<string>> _disabledNotifications;

        private List<string> _notificationNames;

        private const string DATA_FILE_NAME_DISABLED = "RustPlusExtended_Disabled";

        private const string DATA_FILE_NAME_NOTIFICATION_NAMES = "RustPlusExtended_Notification_Names";

        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        private bool _sendWipeNotificationsOnInit = false;

        #region Commands
        [ConsoleCommand("rustplus.restart")]
        private void consoleTryRestartRustPlusLol(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin) return;

            PrintWarning("Calling companion shutdown...");
            CompanionServer.Server.Shutdown();

            PrintWarning("Called shutdown");

            PrintWarning("waiting 5 seconds before init again");

            ServerMgr.Instance.Invoke(() =>
            {
                PrintWarning("Calling companion init... (this may take a while)");
                CompanionServer.Server.Initialize();
                PrintWarning("Called init");

               // PrintWarning("in 2 secs, will reset httpclient");

                ServerMgr.Instance.Invoke(() =>
                {
                    
                  //  var newClient = new System.Net.Http.HttpClient();

                  //  _httpClient.SetValue(null, newClient);
                }, 2f);

            }, 5f);


        }
        [ConsoleCommand("rustplus.debug")]
        private void consoleDebug(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin) return;

            if (arg.Args == null || arg.Args.Length < 1) return;

            var arg0 = arg.Args[0];

            if (string.IsNullOrWhiteSpace(arg0)) return;


            //lets test a manual thingy real quick

            var client = _httpClient.GetValue(null) as HttpClient;

            if (client == null)
            {
                PrintWarning("http client null get!!!");
                return;
            }
            else PrintWarning("got httpclinet");


            PushRequest pushRequest = Pool.Get<PushRequest>();
            pushRequest.ServerToken = @"";
            pushRequest.Channel = NotificationChannel.SmartAlarm;
            pushRequest.Title = arg0;
            pushRequest.Body = "test";
            pushRequest.Data = Util.GetServerPairingData();

            pushRequest.SteamIds = Pool.GetList<ulong>();
                pushRequest.SteamIds.Add(76561198028248023);

            
            string content = JsonConvert.SerializeObject((object)pushRequest);

            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            PrintWarning(content);
            PrintWarning(stringContent.ToString());
            PrintWarning("sent serialized json for pushrequest");


            var task = Task.Run(() => client.PostAsync("https://companion-rust.facepunch.com/api/push/send", (HttpContent)stringContent));

            task.Wait();

            var httpResponseMessage = task.Result;

            PrintWarning(httpResponseMessage.ToString());

            PrintWarning("Is success code: " + httpResponseMessage.IsSuccessStatusCode);
            PrintWarning("code: " + httpResponseMessage.StatusCode);



            //    BroadcastRustPlusNotification(arg0);
            SendReply(arg, "sent arg0: " + arg0);


        }

        [ConsoleCommand("rustplus.broadcast")]
        private void consoleBroadcast(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin) return;

            if (arg.Args == null || arg.Args.Length < 1) return;

            var arg0 = arg.Args[0];

            if (string.IsNullOrWhiteSpace(arg0)) return;


            BroadcastRustPlusNotification(arg0);
            SendReply(arg, "sent arg0: " + arg0);
        }
        #endregion

        #region Hooks
        private void Init()
        {
            _notificationNames = Interface.Oxide.DataFileSystem.ReadObject<List<string>>(DATA_FILE_NAME_NOTIFICATION_NAMES);
            _disabledNotifications = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, List<string>>>(DATA_FILE_NAME_DISABLED);
        }

        private void Unload()
        {
            SaveData();

            foreach (var coroutine in _coroutines)
            {
                try { ServerMgr.Instance.StopCoroutine(coroutine); }
                catch (Exception ex) { PrintError(ex.ToString()); }
            }
        }

        private void OnFuelConsume(BaseOven oven, Item fuel, ItemModBurnable burnable)
        {
            if (oven == null) return;

            //furnace, large furnace

            if (oven.prefabID != 4192425004 && oven.prefabID != 3151820041) return; //we only want furnaces & large furnaces for now

            var ownerId = oven.OwnerID;
            if (ownerId == 0) return;

            var ply = BasePlayer.FindByID(ownerId);
            if (ply != null && ply.IsConnected && ply.IdleTime < 300) return; //don't send to connected players

            var fuelId = fuel.info.itemid;
            var getWoodAmount = oven.inventory.GetAmount(fuelId, false);

            if (getWoodAmount >= (fuel.MaxStackable() * 0.075)) return; //opt, we can safely assume it isn't going to use more than 5% of its stack size in one consume lol
         
            oven.Invoke(() =>
            {
                if (oven == null || oven.IsDestroyed) return;

                if (!oven.IsOn())
                {
                    getWoodAmount = oven.inventory.GetAmount(fuelId, false);

                    if (getWoodAmount <= 0)
                    {
                        

                        ItemDefinition firstCookableItemInfo = null;

                        for (int i = 0; i < oven.inventory.itemList.Count; i++)
                        {
                            var item = oven.inventory.itemList[i];
                            if (item.info.itemid != fuelId && item.info.GetComponent<ItemModCookable>() != null)
                            {
                                firstCookableItemInfo = item.info;
                                
                                break;
                            }
                        }

                        var itemName = firstCookableItemInfo?.displayName?.english;
                        var itemTotal = oven.inventory.GetAmount((firstCookableItemInfo?.itemid ?? 0), false);

                        var smeltTxt = string.Empty;
                        if (!string.IsNullOrWhiteSpace(itemName)) smeltTxt = "with " + itemTotal.ToString("N0") + " " + itemName + " remaining!";

                        SendRustPlusNotification(ownerId, "Your furnace has run out of fuel" + (!string.IsNullOrWhiteSpace(smeltTxt) ? (" " + smeltTxt) : "!"));
                    }
                }

            }, 2f);

        }

        private void OnServerInitialized()
        {
            PrintWarning(nameof(OnServerInitialized));

            if (!_sendWipeNotificationsOnInit)
            {
                PrintWarning(nameof(_sendWipeNotificationsOnInit) + " was false, return");
                return;
            }

            PrintWarning(nameof(_sendWipeNotificationsOnInit) + " was true, sending notifications after 150 second buffer!");

            ServerMgr.Instance.Invoke(() =>
            {
                PrintWarning("Attempting to send Rust+ notifications for server wipe...");

                BroadcastRustPlusNotification("Server wiped! Hurry and join before they beat you to it!", "SERVER WIPE | PRISM", "SERVER_WIPE");

                PrintWarning("Rust+ notifications for server wipe sent.");
            }, 150f);
        }

        private void OnNewSave(string fileName)
        {
            _sendWipeNotificationsOnInit = true;
            PrintWarning("Setting " + nameof(_sendWipeNotificationsOnInit) + " to true as we've just received a OnNewSave hook call (" + fileName + ")");
        }
        #endregion

        #region Util
        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME_DISABLED, _disabledNotifications);
            Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME_NOTIFICATION_NAMES, _notificationNames);
        }

        private void SendRustPlusNotification(ulong userId, string titleMsg, string bodyMsg = "PRISM", string notificationName = "", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            if (!string.IsNullOrWhiteSpace(notificationName) && !_notificationNames.Contains(notificationName))
                _notificationNames.Add(notificationName);

            if (!CanSendNotificationTo(userId, notificationName))
                return;

            NotificationList.SendNotificationTo(userId, channel, titleMsg, bodyMsg, Util.GetServerPairingData());
        }

        private bool CanSendNotificationTo(ulong userId, string notificationName)
        {
            if (string.IsNullOrWhiteSpace(notificationName)) 
                return true;

            List<string> disabled;

            if (!_disabledNotifications.TryGetValue(userId, out disabled))
                return true; //??!?!? was false? lol

            return disabled.Contains(notificationName);
        }

        private IEnumerator BroadcastRustPlusNotificationCoroutine(string titleMsg, string bodyMsg = "PRISM", string notificationName = "", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            var userIds = Pool.Get<HashSet<ulong>>();

            try
            {
                var actualIdsToSend = new HashSet<ulong>();

                var c = 0;

                PlayersByDatabase?.Call("GetAllPlayerIDsNoAllocUH", userIds);

                PrintWarning(nameof(BroadcastRustPlusNotificationCoroutine) + " userIds: " + userIds.Count);

                var pairingData = Util.GetServerPairingData();

                foreach (var userId in userIds)
                {
                  

                    if (c >= 15) //i'm not sure if 15 is the max, but i confirmed it seems to work with 15. if you send too many IDs in one list, it won't actually send.
                    {
                        var actualIdsI = (ICollection<ulong>)actualIdsToSend;

                        _sendNotificationImpl.Invoke(null, new object[] { actualIdsI, channel, titleMsg, bodyMsg, pairingData });

                        actualIdsToSend.Clear();

                        c = 0;

                        yield return CoroutineEx.waitForSecondsRealtime(0.25f);
                    }

                    if (CanSendNotificationTo(userId, notificationName))
                    {
                        actualIdsToSend.Add(userId);

                        c++;
                    }

                }
            }
            finally
            {
                userIds?.Clear();
                Pool.Free(ref userIds);
            }

            yield return null;
        }

        private void BroadcastRustPlusNotification(string titleMsg, string bodyMsg = "PRISM", string notificationName = "", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            if (!string.IsNullOrWhiteSpace(notificationName) && !_notificationNames.Contains(notificationName))
                _notificationNames.Add(notificationName);

            StartCoroutine(BroadcastRustPlusNotificationCoroutine(titleMsg, bodyMsg, notificationName, channel));
        }

        private void GetAllPlayerIDsNoAlloc(ref List<string> list)
        {
            PlayersByDatabase?.Call("GetAllPlayerIDsNoAlloc", list);
        }

        private void StartCoroutine(IEnumerator routine)
        {
            _coroutines.Add(ServerMgr.Instance.StartCoroutine(routine));
        }

        private void StopCoroutine(Coroutine routine) { _coroutines.Remove(routine); }

        #endregion
    }
}
