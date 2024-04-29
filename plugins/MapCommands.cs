using Facepunch;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace Oxide.Plugins
{
    [Info("MapCommands", "MBR", "1.0.4")]
    class MapCommands : RustPlugin
    {
        [PluginReference] Plugin Compilation;
        Dictionary<string, List<Vector3>> monuments = new Dictionary<string, List<Vector3>>();

        private const float GRID = 145;

        private Coroutine _cacheRoutine;

        private void OnServerInitialized()
        {
            foreach (var monument in TerrainMeta.Path.Monuments)
            {
                if (monument == null || !monument.shouldDisplayOnMap) continue;
                
                var name = monument.displayPhrase.english.ToLower().Trim();

                if (monuments.ContainsKey(name)) monuments[name].Add(monument.transform.position);
                else monuments.Add(name, new List<Vector3> { monument.transform.position });
            }

            _cacheRoutine = ServerMgr.Instance?.StartCoroutine(CachePositions(2f));

        }

        private void Unload()
        {
            if (_cacheRoutine != null)
                ServerMgr.Instance?.StopCoroutine(_cacheRoutine);
        }

        private IEnumerator CachePositions(float initialDelaySeconds = 0f)
        {

            var sb = Pool.Get<StringBuilder>();

            try
            {
                PrintWarning(sb.Clear().Append(nameof(CachePositions)).Append(" (").Append(nameof(initialDelaySeconds)).Append(": ").Append(initialDelaySeconds).Append(")").ToString());
            }
            finally { Pool.Free(ref sb); }

            if (initialDelaySeconds > 0f)
                yield return CoroutineEx.waitForSecondsRealtime(initialDelaySeconds);

            var gridChar = 'A';
            var gridNum = 0;

            sb = Pool.Get<StringBuilder>();

            try 
            {
                var gridName = sb.Clear().Append(gridChar).Append(gridNum).ToString();

                Vector3 pos;

                while ((pos = PositionFromGrid(gridName)) != Vector3.zero)
                {


                    if (gridChar > 'Z')
                    {
                        PrintWarning("gridChar > Z: " + gridChar + " and gridNum: " + gridNum);
                        break;
                    }

                    if (gridNum >= 26)
                    {
                        gridChar++;
                        gridNum = 0;
                    }

                    _gridToPosition[gridName] = pos;

                    gridNum++;

                    gridName = sb.Clear().Append(gridChar).Append(gridNum).ToString();

                    yield return CoroutineEx.waitForEndOfFrame;
                }

                PrintWarning("all done! cached grid positions: " + _gridToPosition.Count);
            }
            finally { Pool.Free(ref sb); }

         

        }


        private readonly Dictionary<string, Vector3> _gridToPosition = new();

        private Vector3 PositionFromGrid(string gridName)
        {
            if (string.IsNullOrEmpty(gridName)) 
                return Vector3.zero;

            //gridName = gridName.Trim().ToUpper();
            //^ does it in command instead for perf with cache.

            if (_gridToPosition.TryGetValue(gridName, out var position))
                return position;

    
            var letters = gridName.ToCharArray().ToList();

            if (letters.Count < 2 || letters.Count > 4 || letters.Any(x => !IsValidLetter(x) && !Char.IsDigit(x))) return Vector3.zero;

            int x_offset = IsValidLetter(letters[1]) ? (letters[1] - 'A' + 26) : letters[0] - 'A';
            
            letters.RemoveAt(0);
            if (IsValidLetter(letters[0])) letters.RemoveAt(0);

            if (!Char.IsDigit(letters[0])) return Vector3.zero;

            int z_offset = letters.Count == 2 && Char.IsDigit(letters[1]) ? (((letters[0] - '0') * 10) + letters[1] - '0') : letters[0] - '0';

            //Puts("Log: " + x_offset + ", " + z_offset);

            float map_width = ConVar.Server.worldsize / 2;

            var rv = new Vector3((GRID * x_offset) - map_width + (GRID / 2), 25f, (GRID * -z_offset) + map_width - (GRID / 2) - 50f);

            if (rv.x < -map_width || rv.x > map_width || rv.z < -map_width || rv.z > map_width) return Vector3.zero;

            return rv;
        }

        [ChatCommand("monument")]
        private void MonumentCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You dont have permission to use this command.");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "Invalid syntax! /monument <monument name>");
                return;
            }

            var search_value = args[0].Trim().ToLower();
            var name = this.monuments.Keys.FirstOrDefault(x => x.Contains(search_value));

            if (string.IsNullOrEmpty(name))
            {
                SendReply(player, "No such monument found, available:\n" + string.Join(", ", this.monuments.Keys));
                return;
            }

            var monuments = this.monuments[name];

            if (monuments.Count < 1)
            {
                SendReply(player, "No monuments found!");
                return;
            }

            if (monuments.Count == 1)
            {
                TeleportPlayer(player, monuments[0] + new Vector3(0f, 20f, 0f));
                return;
            }

            Vector3 closest = monuments[0];
            double closest_dist = Vector3.Distance(player.transform.position, closest);

            for (var i = 1; i < monuments.Count; i++)
            {
                var d = Vector3.Distance(player.transform.position, monuments[i]);
                if (d < closest_dist)
                {
                    closest_dist = d;
                    closest = monuments[i];
                }
            }

            var new_list = monuments.Where(x => x != closest).ToList();

            TeleportPlayer(player, new_list[UnityEngine.Random.Range(0, new_list.Count)] + new Vector3(0, 20f, 0));
        }

        [ChatCommand("grid")]
        private void GridCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You dont have permission to use this command.");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "Invalid syntax! /grid <grid>");
                return;
            }

            var pos = PositionFromGrid(args[0].Trim().ToUpper());

            if (pos == Vector3.zero)
            {
                SendReply(player, args[0] + " is not a valid grid!");
                return;
            }

            TeleportPlayer(player, pos);
        }

        private void TeleportPlayer(BasePlayer player, Vector3 pos) => Compilation?.Call("TeleportPlayer", player, pos);
        

        private bool IsValidLetter(char letter)
        {
            return letter >= 'A' && letter <= 'Z';
        }
    }
}
