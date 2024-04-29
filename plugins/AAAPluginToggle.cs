// Reference: 0Harmony
using Facepunch;
using HarmonyLib;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oxide.Plugins
{
    [Info("Plugin Toggle", "Shady", "0.0.6", ResourceId = 0)]
    internal class AAAPluginToggle : RustPlugin
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
        private Harmony _harmony;

        private Dictionary<string, bool> _pluginStates;

        public Dictionary<string, bool> PluginStates
        {
            get { return _pluginStates; }
            set
            {
                try { Config["Plugin States"] = value; }
                finally { _pluginStates = value; }
            }
        }
        

        private static AAAPluginToggle _instance = null;
        #endregion
        #region Config
        private bool _configChanged = false;

        private T GetConfig<T>(string name, T defaultValue)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            if (Config[name] == null)
            {
                Config[name] = defaultValue;

                _configChanged = true;

                return defaultValue;
            }

            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        protected override void LoadDefaultConfig()
        {
            PluginStates = GetConfig("Plugin States", new Dictionary<string, object>())?.ToDictionary(p => p.Key, p => Convert.ToBoolean(p.Value)) ?? new Dictionary<string, bool>();

            if (_configChanged) SaveConfig();
        }


        #endregion
        #region Command
        private void cmdPluginToggle(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsAdmin) return;

            if (args.Length < 1)
            {
                player.Message("Must supply a plugin name!");
                return;
            }

            var findPlugin = plugins.Find(args[0]);

            string pluginActualName;

            if (findPlugin == null)
            {
                player.Reply("No plugin found with name: " + args[0] + " - performing manual search");
                pluginActualName = args[0];
            }
            else pluginActualName = findPlugin?.Name;


            bool state;
            if (!PluginStates.TryGetValue(pluginActualName, out state))
            {
                player.Reply("Plugin has no state! - manual search failed for: " + pluginActualName);
                return;
            }
            else if (string.IsNullOrWhiteSpace(pluginActualName)) pluginActualName = args[0];


            PluginStates[pluginActualName] = !state;
            player.Message(pluginActualName + " now has state: " + !state);
            if (!state)
            {
                Interface.Oxide.LoadPlugin(pluginActualName);
            }
            else Interface.Oxide.UnloadPlugin(pluginActualName);
         
        }
        #endregion
        #region Hooks
        private void Init()
        {
            _instance = this;

            LoadDefaultConfig();

            string[] cmdAliases = { "plugin.toggle", };
            AddCovalenceCommand(cmdAliases, nameof(cmdPluginToggle));

            DoHarmonyPatches(_harmony = new Harmony(GetType().Name));   
        }

        private void OnPluginLoaded(Plugin plugin)
        {
            if (plugin == null || plugin.IsCorePlugin) return;

            var pluginName = plugin.Name;

            bool val;
            if (PluginStates.TryGetValue(pluginName, out val) && !val)
            {
                NextTick(() => Interface.Oxide.UnloadPlugin(plugin.Name));
            }
        }

        private void OnTerrainInitialized() => UnloadAllDisabledPlugins();
        

        private void OnServerInitialized()
        {
            var allPlugins = plugins.GetAll();

            bool val;

            var added = 0;

            for(int i = 0; i < allPlugins.Length; i++)
            {
                var p = allPlugins[i];
                if (p == null || p.IsCorePlugin || p.Name == Name) continue;

                if (!PluginStates.TryGetValue(p.Name, out val))
                {
                    PluginStates[p.Name] = false;
                    added++;
                }
                else if (!val) Interface.Oxide.UnloadPlugin(p.Name); //try to unload plugin if found to be disabled in config
            }

            if (added > 0)
            {
                PrintWarning("Added: " + added.ToString("N0") + " new plugins to config");

                SaveConfig();
            }

            Unsubscribe(nameof(OnPluginLoaded));
        }

        private void UnloadAllDisabledPlugins()
        {
            var allPlugins = plugins.GetAll();

            bool val;

            for (int i = 0; i < allPlugins.Length; i++)
            {
                var p = allPlugins[i];
                if (p == null || p.IsCorePlugin || p.Name == Name) continue;

                if (PluginStates.TryGetValue(p.Name, out val) && !val) Interface.Oxide.UnloadPlugin(p.Name);
            }
        }

        private void Unload()
        {
            try { _harmony?.UnpatchAll(GetType().Name); }
            finally
            {
                try { SaveConfig(); }
                finally { _instance = null; }
            }
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
            if (harmony == null) throw new ArgumentNullException(nameof(harmony));

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

                            var originalMethod = patch.info.declaringType.GetMethod(patch.info.methodName, (flags != null && flags.HasValue) ? (BindingFlags)flags : BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, patch.info.argumentTypes, null);
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
        #region Patch
        [HarmonyPatch(typeof(CSharpPluginLoader), "Load", new[] { typeof(CompilablePlugin) })]
        private class PluginLoadPatch
        {
            private static bool Prefix(CompilablePlugin plugin)
            {
                if (string.IsNullOrEmpty(plugin?.Name) || plugin?.Name == _instance?.Name) return true;

                if (_instance == null)
                {
                    Interface.Oxide.LogWarning("Patch found null _instance!!");
                    return true;
                }

                if (_instance?.PluginStates == null)
                {
                    Interface.Oxide.LogWarning("Patch found null _pluginStates!!");
                    return true;
                }

                bool val;

                if (!_instance.PluginStates.TryGetValue(plugin.Name, out val))
                {
                    _instance.PluginStates[plugin.Name] = false;
                }

                if (val) return true;
                else
                {
                    if (Interface.Oxide.UnloadPlugin(plugin.Name)) //call unload in the case of an existing plugin instance (like o.reload)
                    {
                        Interface.Oxide.LogWarning("Unloaded disabled plugin: " + plugin.Name);
                    }


                    Interface.Oxide.LogWarning("Loading was prevented for plugin: " + plugin.Name);
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(CSharpPluginLoader), "Load", new[] { typeof(string), typeof(string) })]
        private class PluginLoadPatch2
        {
            private static bool Prefix(string directory, string name)
            {
                if (string.IsNullOrEmpty(name) || name.Equals(_instance?.Name)) return true;

                if (_instance == null)
                {
                    Interface.Oxide.LogWarning("Patch found null _instance (directory load)!!");
                    return true;
                }

                if (_instance?.PluginStates == null)
                {
                    Interface.Oxide.LogWarning("Patch found null _pluginStates (directory load)!!");
                    return true;
                }

                bool val;

                if (!_instance.PluginStates.TryGetValue(name, out val))
                {
                    _instance.PluginStates[name] = false;
                }

                if (val) return true;
                else
                {
                    if (Interface.Oxide.UnloadPlugin(name)) //call unload in the case of an existing plugin instance (like o.reload)
                    {
                        Interface.Oxide.LogWarning("Unloaded disabled plugin (directory load): " + name);
                    }


                    Interface.Oxide.LogWarning("Loading was prevented for plugin (directory load): " + name);
                    return false;
                }
            }
        }
        #endregion
    }
}