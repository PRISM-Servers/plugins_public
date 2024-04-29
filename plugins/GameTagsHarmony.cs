// Reference: 0Harmony
using Facepunch;
using HarmonyLib;
using Oxide.Core;
using Oxide.Core.Plugins;
using Steamworks;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GameTagsHarmony", "Shady", "2.0.4", ResourceId = 0000)]
    internal class GameTagsHarmony : RustPlugin
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
        private readonly MethodInfo _updateServerInformation = typeof(ServerMgr).GetMethod("UpdateServerInformation", BindingFlags.NonPublic | BindingFlags.Instance);

        private Harmony _harmony;

        private static GameTagsHarmony Instance = null;

        [PluginReference]
        private readonly Plugin WipeInfo;

        public int CachedTotalCount = 0;

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

        private void Init()
        {
            Instance = this;
            _harmony = new Harmony(GetType().Name);
            DoHarmonyPatches(_harmony);
        }

        private Action CacheAction { get; set; } = null;

        private static int GetCachedTotalCount()
        {
            return Instance?.CachedTotalCount ?? 0;
        }

        private void OnServerInitialized()
        {
            CacheAction = new Action(() =>
            {
                CachedTotalCount = BasePlayer.activePlayerList?.Count ?? 0;

            });
            CacheAction.Invoke();
            ServerMgr.Instance.InvokeRepeating(CacheAction, 30f, 30f);

            ServerMgr.Instance.Invoke(() =>
            {
                _updateServerInformation.Invoke(ServerMgr.Instance, null);
            }, 0.5f);
        }

        private void Unload()
        {
            try
            {
                if (CacheAction != null && ServerMgr.Instance != null) 
                    ServerMgr.Instance.CancelInvoke(CacheAction);
              
                _harmony?.UnpatchAll(GetType().Name);
            }
            finally { Instance = null; }
        }

        #region ServerMgr.UpdateServerInformation Patch
        [HarmonyPatch(typeof(ServerMgr), "UpdateServerInformation")]
        private class HookPatch
        {
            private static readonly PropertyInfo _assemblyHashGet = typeof(ServerMgr).GetProperty("AssemblyHash", (BindingFlags.Instance | BindingFlags.NonPublic));

            private static string _assemblyHash;
            public static string AssemblyHash
            {
                get
                {
                    try { if (string.IsNullOrEmpty(_assemblyHash) && ServerMgr.Instance != null) _assemblyHash = _assemblyHashGet.GetValue(ServerMgr.Instance).ToString(); }
                    catch (Exception ex) { Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + " Error on AssemblyHash get"); }

                    return _assemblyHash;
                }
            }

            private static int _bornTime = 0;
            public static int BornTime { get { return (_bornTime == 0) ? (_bornTime = Facepunch.Math.Epoch.FromDateTime(SaveRestore.SaveCreatedTime)) : _bornTime; } }

            private static bool Prefix(ServerMgr __instance)
            {

                if (!SteamServer.IsValid)
                {
                    Interface.Oxide.LogError("SteamServer is not valid!");
                    return false;
                }

                SteamServer.ServerName = ConVar.Server.hostname;
                SteamServer.MaxPlayers = ConVar.Server.maxplayers;
                SteamServer.Passworded = false;

                if (ConVar.Server.hostname.Contains("#2")) SteamServer.MapName = "[Ping Verified]";
                else if (ConVar.Server.hostname.Contains("#1")) SteamServer.MapName = "[Hardcore]|{0.5x}";
                else SteamServer.MapName = World.Name;


                var sb = Pool.Get<StringBuilder>();
                try
                {
                    //var localSaveTime = SaveRestore.SaveCreatedTime.ToLocalTime();

                    var wipeDate = Instance?.WipeInfo?.Call<DateTime>("GetWipeDate") ?? DateTime.MinValue;

                    var wipeStr = (DateTime.UtcNow - SaveRestore.SaveCreatedTime).TotalDays < 2 ? "JUST WIPED" : sb.Clear().Append("WIPE ").Append(wipeDate.Day).Append("/").Append(wipeDate.Month).ToString();

                    SteamServer.MapName = sb.Clear().Append(wipeStr).Append("| ").Append(SteamServer.MapName).ToString();

                    var tagsTagStr = string.IsNullOrEmpty(ConVar.Server.tags) ? string.Empty : sb.Clear().Append(",").Append(ConVar.Server.tags.Trim(',')).ToString();

                    SteamServer.GameTags = sb.Clear().Append("mp").Append(ConVar.Server.maxplayers).Append(",cp").Append(GetCachedTotalCount()).Append(",pt").Append(Network.Net.sv.ProtocolId).Append(",qp").Append(__instance.connectionQueue.Queued).Append(",v").Append(Rust.Protocol.network).Append(ConVar.Server.pve ? ",pve" : string.Empty).Append(tagsTagStr).Append(",h").Append(AssemblyHash).Append(",").Append(__instance.Restarting ? "strst" : "stok").Append(",born").Append(BornTime).Append(",gm").Append(ServerMgr.GamemodeName()).Append(",cs").Append(BuildInfo.Current?.Scm?.ChangeId ?? "0").ToString();

                    if (!string.IsNullOrEmpty(ConVar.Server.description) && ConVar.Server.description.Length > 100)
                    {
                        SteamServer.SetKey("description_0", string.Empty); //this *may*(?) be able to be removed in the future - i think our old code simply set it and never un-set it

                        var array = ConVar.Server.description.SplitToChunks(100);
                        var i = 0;

                        foreach(var item in array)
                        {
                            if (i >= 16)
                                break;

                            SteamServer.SetKey(sb.Clear().Append("description_").Append(i.ToString("00")).ToString(), item);

                            i++;
                        }
                    }
                    else
                    {
                        SteamServer.SetKey("description_0", ConVar.Server.description);
                        for (int index = 1; index < 16; ++index)
                            SteamServer.SetKey(sb.Clear().Append("description_").Append(index.ToString("00")).ToString(), string.Empty);
                    }

                    SteamServer.SetKey("hash", AssemblyHash);
                    SteamServer.SetKey("world.seed", World.Seed.ToString());
                    SteamServer.SetKey("world.size", World.Size.ToString());
                    SteamServer.SetKey("pve", ConVar.Server.pve.ToString());
                    SteamServer.SetKey("headerimage", ConVar.Server.headerimage);
                    SteamServer.SetKey("url", ConVar.Server.url);
                    SteamServer.SetKey("gmn", ServerMgr.GamemodeName());
                    SteamServer.SetKey("gmt", ServerMgr.GamemodeTitle());
                    SteamServer.SetKey("gmd", string.Empty);
                    SteamServer.SetKey("gmu", string.Empty);
                    SteamServer.SetKey("uptime", ((int)Time.realtimeSinceStartup).ToString());
                    SteamServer.SetKey("gc_mb", Performance.report.memoryAllocations.ToString());
                    SteamServer.SetKey("gc_cl", Performance.report.memoryCollections.ToString());
                    SteamServer.SetKey("fps", Performance.report.frameRate.ToString());
                    SteamServer.SetKey("fps_avg", Performance.report.frameRateAverage.ToString("0.00"));
                    SteamServer.SetKey("ent_cnt", BaseNetworkable.serverEntities.Count.ToString());
                    SteamServer.SetKey("build", BuildInfo.Current.Scm.ChangeId);

                    Interface.CallHook("OnServerInformationUpdated");

                    return false;
                }
                finally { Pool.Free(ref sb); }

              
            }
        }
        #endregion
    }
}