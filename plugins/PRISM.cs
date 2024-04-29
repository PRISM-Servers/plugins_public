// Reference: 0Harmony
using ConVar;
using HarmonyLib;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony = HarmonyLib.Harmony;

namespace Oxide.Plugins
{
    [Info("PRISM", "MBR", "1.0.6")]
    [Description("")]
    class PRISM : RustPlugin
    {
        #region Fields
        private Harmony _harmony;
        #endregion
        #region Hooks
        private void Init() => DoHarmonyPatches(_harmony = new Harmony(GetType().Name));

        #endregion
        #region Custom Harmony Methods
        private void DoHarmonyPatches(Harmony instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var c = 0;

            var watch = Facepunch.Pool.Get<Stopwatch>();
            try { c = PatchAllHarmonyAttributes(GetType(), instance); }
            finally
            {
                var elapsedMs = watch.ElapsedMilliseconds;
                Facepunch.Pool.Free(ref watch);

                var sb = Facepunch.Pool.Get<StringBuilder>();
                try { PrintWarning(sb.Clear().Append("Took: ").Append(elapsedMs.ToString("0.00").Replace(".00", string.Empty)).Append("ms to apply ").Append(c.ToString("N0")).Append(" patch").Append(c > 1 ? "es" : string.Empty).ToString()); }
                finally { Facepunch.Pool.Free(ref sb); }
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
                                PrintWarning("originalMethod is null!! patch.info.methodName: " + patch.info.methodName + Environment.NewLine + "patch.info.declaringType: " + patch.info.declaringType.FullName);
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

        #region Patches
        [HarmonyPatch(typeof(BasePlayer), "StopDemoRecording")]
        private class DemoRecordingPatch
        {
            private static bool Prefix(BasePlayer __instance)
            {
                if (__instance.net == null || __instance.net.connection == null || !__instance.net.connection.IsRecording || Interface.CallHook("OnDemoRecordingStop", __instance.net.connection.RecordFilename, __instance) != null)
                    return false;
                __instance.net.connection.StopRecording();
                __instance.CancelInvoke(new Action(__instance.MonitorDemoRecording));
                Interface.CallHook("OnDemoRecordingStopped", __instance.net.connection.RecordFilename, __instance); //recordfilename (getter) returns exactly the same as the oriignal
                return false;
            }
        }

        #endregion
        #region Variables

        private List<OverwatchItem> recording = new List<OverwatchItem>();

        private const double SECONDS = 2 * 60;

        #endregion

        #region OverwatchItem

        class OverwatchItem
        {
            public BasePlayer player;
            public string prev;
            public string current;
            public DateTime start;
            public DateTime end = DateTime.MinValue;
            public string pending = null;

            public double elapsedSeconds
            {
                get
                {
                    return (end == DateTime.MinValue ? DateTime.UtcNow : end).Subtract(start).TotalSeconds;
                }
            }

            public bool recording
            {
                get
                {
                    return start != DateTime.MinValue;
                }
            }

            public OverwatchItem(BasePlayer player, string fn = "")
            {
                this.player = player;
                current = fn;
                prev = null;
                start = DateTime.UtcNow;
            }

            public void shift(bool startNew)
            {
                if (prev != null)
                {
                    if (File.Exists(prev))
                    {
                        File.Delete(prev);
                    }
                }

                prev = current;
                current = null;

                if (startNew)
                {
                    start = DateTime.UtcNow;
                    end = DateTime.MinValue;
                    StartRecordingPlayer(player);
                }
                else
                {
                    start = DateTime.MinValue;
                    end = DateTime.UtcNow;
                }
            }
        }

        #endregion

        #region Hooks

        private void OnServerInitialized()
        {
            //check all files in demos
            if (!Directory.Exists("demos"))
            {
                Directory.CreateDirectory("demos");
            }
            
            var files = Directory.GetFiles("demos");

            foreach (var file in files.Where(x => x.EndsWith(".tmp")))
            {
                //file is already path to the actual file, mashallah
                File.Delete(file);
            }


            foreach (var player in BasePlayer.activePlayerList)
            {
                StartRecordingPlayer(player);
            }

            timer.Every(5f, () =>
            {
                try
                {
                    for(int i = 0; i < recording.Count; i++)
                    {
                        var item = recording[i];
                        if (item.elapsedSeconds >= SECONDS) item.player.StopDemoRecording();
                    }
                }
                catch (Exception e)
                {
                    PrintError("Error in Overwatch timer: " + e.Message + "\n" + e.StackTrace);
                }
            });

        }

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                //StopDemoRecording(player);
                player.StopDemoRecording();
            }
        }

        private void OnDemoRecordingStopped(string filename, BasePlayer player)
        {
            var item = GetRecording(player);
            if (item == null)
            {
                PrintWarning("No recording on demo record stop!");
                return;
            }

            timer.Once(0.1f, () =>
            {
                if (item.pending != null)
                {
                    SaveFile(item.current, item.pending);
                    item.pending = null;
                    item.current = null;
                }

                item.shift(player.IsConnected);
            });
        }

        private void OnDemoRecordingStarted2(string filename, BasePlayer player)
        {
            var existing = GetRecording(player);
            if (existing != null) existing.current = filename;
            else recording.Add(new OverwatchItem(player, filename));
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            StartRecordingPlayer(player);
        }

        #endregion

        #region Methods

        private void StopDemoRecording(BasePlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.net == null || player.net.connection == null || !player.net.connection.IsRecording || Interface.CallHook("OnDemoRecordingStop", player.net.connection.RecordFilename, player) != null)
                return;

            player.net.connection.StopRecording();
            player.CancelInvoke(new Action(player.MonitorDemoRecording));
            Interface.CallHook("OnDemoRecordingStopped", player.net.connection.RecordFilename, player);
        }

        private static void SaveFile(string file, string path)
        {
            if (File.Exists(file))
            {
                Interface.Oxide.LogDebug("Moving " + file + " to " + path);
                if (File.Exists(path)) File.Delete(path);
                File.Move(file, path);
            }
        }

        private List<string> Save(BasePlayer player, string filename)
        {
            var rv = new List<string>();

            var item = GetRecording(player);
            if (item == null) return rv;

            filename = filename.Replace(".dem", "");
            filename = "demos/" + filename + ".dem";

            if (item.elapsedSeconds <= 10 && item.prev != null)
            {
                Puts("Not enough seconds, saving previous too");

                if (item.elapsedSeconds < 2)
                {
                    rv.Add(filename);
                    SaveFile(item.prev, filename);
                    item.prev = null;
                    return rv;
                }
                else
                {
                    var temp = filename.Replace(".dem", "_prev.dem");
                    rv.Add(temp);
                    SaveFile(item.prev, temp);
                    item.prev = null;
                }
            }

            if (item.recording)
            {
                item.pending = filename;
                rv.Add(filename);
                //StopDemoRecording(player);
                player.StopDemoRecording();
            }
            else
            {
                rv.Add(item.current ?? item.prev);
                SaveFile(item.current ?? item.prev, filename);
                item.current = null;
            }

            return rv;
        }

        private OverwatchItem GetRecording(BasePlayer player)
        {
            foreach (var item in recording)
            {
                if (item.player.UserIDString == player.UserIDString)
                {
                    return item;
                }
            }

            return null;
        }

        private static void StartRecordingPlayer(BasePlayer player)
        {
            var now = DateTime.UtcNow;
            StartDemoRecording(player, "OW_" + player.UserIDString + "_" + now.Year + "_" + now.Month + "_" + now.Day + "_" + now.Hour + "_" + now.Minute + "_" + now.Second + "_UTC");
        }

        private static void StartDemoRecording(BasePlayer player, string filename)
        {
            if (player.net == null || player.net.connection == null || player.net.connection.IsRecording) return;

            string path = "demos/" + filename + ".tmp";

            player.net.connection.StartRecording(path, new Demo.Header()
            {
                version = Demo.Version,
                level = UnityEngine.Application.loadedLevelName,
                levelSeed = World.Seed,
                levelSize = World.Size,
                checksum = World.Checksum,
                localclient = player.userID,
                position = player.eyes.position,
                rotation = player.eyes.HeadForward(),
                levelUrl = World.Url,
                recordedTime = DateTime.Now.ToBinary()
            });

            player.SendNetworkUpdateImmediate(false);
            player.SendGlobalSnapshot();
            player.SendFullSnapshot();
            ServerMgr.SendReplicatedVars(player.net.connection);

            Interface.Oxide.CallHook("OnDemoRecordingStarted2", path, player);
        }

        #endregion

        #region Commands

        [ConsoleCommand("overwatch.save")]
        private void SaveCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null || arg.Args == null || arg.Args.Length < 1) return;

            var id = arg.Args[0];
            var player = BasePlayer.FindAwakeOrSleeping(id);

            if (player == null)
            {
                arg.ReplyWith("player not found!");
                return;
            }

            Save(player, "test_" + Facepunch.Math.Epoch.Current + "_" + player.UserIDString);
        }

        #endregion
    }
}
