using Facepunch;
using Oxide.Core.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Auto Vending Machine", "Shady", "0.0.1")]
    internal class AutoVM : RustPlugin
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
        private readonly Dictionary<string, MonumentInfo> _monumentCache = new();
        private readonly Dictionary<string, PasteData> _localMonumentVMPosition = new();

        private class PasteData
        {
            public Vector3 LocalPosition { get; set; } = Vector3.zero;
            public float HeightAdjustment { get; set; } = 0f;

            public PasteData() { }
            public PasteData(Vector3 localPos, float heightAdj = 0f)
            {
                LocalPosition = localPos;
                HeightAdjustment = heightAdj;
            }

            public PasteData(float x, float y, float z, float heightAdj = 0f)
            {
                LocalPosition = new Vector3(x, y, z);
                HeightAdjustment = heightAdj;
            }

        }

        [PluginReference]
        private readonly Plugin CopyPaste;

        private float _pluginInitTime;

        private void Init()
        {
            _pluginInitTime = Time.time;
        }

        private IEnumerator PasteCoroutine(float yieldTime = 0f)
        {
            PrintWarning(nameof(PasteCoroutine) + " yielding " + yieldTime + " seconds.");

            if (yieldTime > 0f)
                yield return CoroutineEx.waitForSecondsRealtime(yieldTime);

            if (CopyPaste == null || !CopyPaste.IsLoaded)
            {
                PrintError(nameof(CopyPaste) + " is missing/unloaded for : " + nameof(PasteCoroutine) + "!");
                yield break;
            }

            PrintWarning(nameof(PasteCoroutine) + " yielded for " + yieldTime + " seconds.");

            var monuments = Pool.GetList<MonumentInfo>();
            var blocks = Pool.GetList<BuildingBlock>();
            try
            {

                foreach (var kvp in _localMonumentVMPosition)
                {
                    var monName = kvp.Key;
                    var data = kvp.Value;

                    monuments.Clear();

                    FindMonuments(ref monuments, monName, false);

                    for (int i = 0; i < monuments.Count; i++)
                    {
                        var mon = monuments[i];
                        if (mon?.transform == null)
                            continue;

                        var alreadyPlaced = false;

                        var worldPos = TransformPoint(mon.transform, data.LocalPosition);

                        blocks.Clear();

                        Vis.Entities(worldPos, 2f, blocks, 2097152); //2097152 is construction mask

                        if (blocks.Count > 0)
                            PrintWarning("found blocks at world pos with radius of 2f, block count: " + blocks.Count + " unless they have ownerid 528491 we will still paste.");

                        for (int j = 0; j < blocks.Count; j++)
                        {
                            if ((blocks[j]?.OwnerID ?? 0) == 528491)
                            {
                                alreadyPlaced = true;
                                break;
                            }
                        }

                        if (alreadyPlaced)
                        {
                            PrintWarning(nameof(alreadyPlaced) + " was true, so skipping this mon: " + mon?.name + " @ " + (mon?.transform?.position ?? Vector3.zero));
                            break;
                        }
                        else PrintWarning("not already placed, attempting paste!");

                        var fakeArgs = new string[4];

                        fakeArgs[0] = "oid";
                        fakeArgs[1] = "528491";
                        fakeArgs[2] = "height";
                        fakeArgs[3] = data.HeightAdjustment.ToString();

                        var resultObj = CopyPaste?.Call("TryPaste", worldPos, "vmfinal3_8", string.Empty, 0f, fakeArgs, true, null);
                        PrintWarning("resultObj: " + resultObj + " for worldPos: " + worldPos);

                    }

                }

            }
            finally
            {
                try { Pool.FreeList(ref monuments); }
                finally { Pool.FreeList(ref blocks); }

            }

        }

        private void OnServerInitialized()
        {


            _localMonumentVMPosition["gas_station_1"] = new PasteData(-17.95f, 3.03f, 8.12f, 1.5f);
            _localMonumentVMPosition["airfield_1"] = new PasteData(8.40f, 0.26f, -27.57f, 1.5f);
            _localMonumentVMPosition["supermarket_1"] = new PasteData(19.93f, 0.02f, -8.24f, 1.5f);
            _localMonumentVMPosition["bandit_town"] = new PasteData(-18.69f, 1.75f, 5.07f, 1.5f);
            _localMonumentVMPosition["harbor_2"] = new PasteData(-6.12f, 5.00f, 5.54f, 7f);
            _localMonumentVMPosition["sphere_tank"] = new PasteData(-3.68f, 5.68f, -40f, 1.5f);

            var wasReload = (Time.time - _pluginInitTime) < 10f;

            PrintWarning(nameof(OnServerInitialized) + " " + nameof(wasReload) + ": " + wasReload);

            _pasteRoutine = ServerMgr.Instance?.StartCoroutine(PasteCoroutine(wasReload ? 0.25f : 75f));

        }

        private Coroutine _pasteRoutine;

        private void Unload()
        {
            if (_pasteRoutine != null)
                ServerMgr.Instance?.StopCoroutine(_pasteRoutine);
        }

        [ConsoleCommand("vm.test")]
        private void CmdVmDbg(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin)
                return;

            if (CopyPaste == null || !CopyPaste.IsLoaded)
            {
                PrintError(nameof(CopyPaste) + " is missing/unloaded!");
                return;
            }

            //assets/bundled/prefabs/autospawn/monument/roadside/gas_station_1.prefab

            var userId = arg?.Player()?.UserIDString ?? string.Empty;

            var oxums = Pool.GetList<MonumentInfo>();

            try 
            { 
                FindMonuments(ref oxums, "gas_station_1");

                if (oxums.Count < 1)
                {
                    SendReply(arg, "No oxums found!!");
                    return;
                }
                else SendReply(arg, "Found oxums: " + oxums.Count);

                for (int i = 0; i < oxums.Count; i++)
                {
                    var oxum = oxums[i];

                    var localPos = new Vector3(-17.95f, 3.03f, 8.12f);

                    var transformedPoint = TransformPoint(oxum.transform, localPos);

                    PrintWarning("transformedPoint is: " + transformedPoint);

                    transformedPoint.y += 0.05f;

                    var fakeArgs = new string[4];

                    fakeArgs[0] = "oid";
                    fakeArgs[1] = "528491";
                    fakeArgs[2] = "height";
                    fakeArgs[3] = "2.5";

                    var resultObj = CopyPaste?.Call("TryPaste", transformedPoint, "vmfinal3_8", userId, 0f, fakeArgs, true, null);

                    if (resultObj is string str) SendReply(arg, "Result is string: " + str);
                    else SendReply(arg, "Result is obj: " + resultObj);

                    // private object TryPaste(Vector3 startPos, string filename, string userID, float RotationCorrection, string[] args, bool autoHeight = true, Action callback = null)
                    // var resultObj = CopyPaste?.Call("TryPaste", )
                }

            }
            finally { Pool.FreeList(ref oxums); }


       

        }

        [ConsoleCommand("vm.del")]
        private void CmdVmDel(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin)
                return;

            var killed = 0;

            var monuments = Pool.GetList<MonumentInfo>();
            var blocks = Pool.GetList<BuildingBlock>();
            try
            {

                foreach (var kvp in _localMonumentVMPosition)
                {
                    var monName = kvp.Key;
                    var data = kvp.Value;

                    monuments.Clear();

                    FindMonuments(ref monuments, monName, false);

                    for (int i = 0; i < monuments.Count; i++)
                    {
                        var mon = monuments[i];
                        if (mon?.transform == null)
                            continue;

                        var worldPos = TransformPoint(mon.transform, data.LocalPosition);

                        blocks.Clear();

                        Vis.Entities(worldPos, 2f, blocks, 2097152); //2097152 is construction mask


                        for (int j = 0; j < blocks.Count; j++)
                        {
                            var block = blocks[j];
                            if (block != null && !block.IsDestroyed)
                            {
                                block.Kill();
                                killed++;
                            }
                        }

                    }

                }

            }
            finally
            {
                try { Pool.FreeList(ref monuments); }
                finally { Pool.FreeList(ref blocks); }

            }

            SendReply(arg, "Killed: " + killed.ToString("N0"));

        }

        private void FindMonuments(ref List<MonumentInfo> monuments, string displayOrPrefabName, bool allowPartialMatch = true)
        {
            if (monuments == null)
                throw new ArgumentNullException(nameof(monuments));

            if (string.IsNullOrWhiteSpace(displayOrPrefabName))
                throw new ArgumentNullException(nameof(displayOrPrefabName));


            for(int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
            {
                var monument = TerrainMeta.Path.Monuments[i];

                var monDisplayName = monument?.displayPhrase?.english ?? string.Empty;

                var monName = monument?.name ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(monName))

                {
                    var slashIndex = monName.LastIndexOf("/");
                    var baseName = (slashIndex == -1) ? monName : monName.Substring(slashIndex + 1);
                    monName = baseName.Replace(".prefab", "");
                }

                if (string.IsNullOrWhiteSpace(monName))
                    continue;

                if (monName.Equals(displayOrPrefabName, StringComparison.OrdinalIgnoreCase) || monDisplayName.Equals(displayOrPrefabName, StringComparison.OrdinalIgnoreCase) || (allowPartialMatch && monDisplayName.IndexOf(displayOrPrefabName, StringComparison.OrdinalIgnoreCase) >= 0))
                    monuments.Add(monument);


            }

        }

        private MonumentInfo FindMonumentInfo(string partialOrFullName)
        {
            if (string.IsNullOrWhiteSpace(partialOrFullName))
                return null;

            if (_monumentCache.TryGetValue(partialOrFullName, out var monumentInfo))
                return monumentInfo;



            MonumentInfo info = null;

            foreach (var monument in TerrainMeta.Path.Monuments)
            {


                var monDisplayName = monument?.displayPhrase?.english ?? string.Empty;

                var monName = monument?.name ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(monName))

                {
                    var slashIndex = monName.LastIndexOf("/");
                    var baseName = (slashIndex == -1) ? monName : monName.Substring(slashIndex + 1);
                    monName = baseName.Replace(".prefab", "");
                }

                if (string.IsNullOrWhiteSpace(monName))
                    continue;

                if (monName.Equals(partialOrFullName, StringComparison.OrdinalIgnoreCase) || monDisplayName.Equals(partialOrFullName, StringComparison.OrdinalIgnoreCase) || monDisplayName.IndexOf(partialOrFullName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (info != null)
                    {
                        PrintWarning("found more than 1 monument by this name and will return null!: " + partialOrFullName);
                        return null;
                    }

                    info = monument;
                }


            }

            if (info != null)
                _monumentCache[partialOrFullName] = info;

            return info;
        }

        private Vector3 TransformPoint(Transform transform, Vector3 localPosition)
        {
            return transform.position + transform.rotation * localPosition;
        }

        private Vector3 InverseTransformPoint(Transform transform, Vector3 worldPosition)
        {
            return Quaternion.Inverse(transform.rotation) * (worldPosition - transform.position);
        }

    }
}
