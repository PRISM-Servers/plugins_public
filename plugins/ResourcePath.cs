using System;
using System.Diagnostics;
using System.Text;

namespace Oxide.Plugins
{
    [Info("ResourcePath", "Shady", "0.0.2")] //the command is from nivex - I made large adjustments to it
    internal class ResourcePath : RustPlugin
    {
        [ConsoleCommand("findprefab")]
        private void ccmdFindPrefab(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin) return;

            var watch = Facepunch.Pool.Get<Stopwatch>();
            try 
            {
                watch.Restart();

                if (!arg.HasArgs())
                {
                    SendReply(arg, "PrefabFinder Syntax: findprefab ladder.wooden.wall|2150203378");
                    return;
                }

                var argStr = string.Join(" ", arg.Args);

                uint prefabID;
                if (uint.TryParse(argStr, out prefabID))
                {
                    var prefabName = StringPool.Get(prefabID);

                    if (string.IsNullOrEmpty(prefabName))
                    {
                        SendReply(arg, "PrefabFinder: {0} prefab not found.", prefabID);
                    }
                    else SendReply(arg, "\nPrefabFinder:\n" + prefabID.ToString() + "\n" + prefabName);
                }
                else
                {
                    var resultSB = Facepunch.Pool.Get<StringBuilder>();
                    try 
                    {
                        resultSB.Clear();
                        var count = 0;
                        foreach (var val in StringPool.toNumber)
                        {
                            var key = val.Key;
                            if (key.IndexOf(argStr, StringComparison.OrdinalIgnoreCase) >= 0 && key.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                            {
                                count++;
                                resultSB.Append("[").Append(key).Append(", ").Append(val.Value).Append("]").Append(Environment.NewLine);
                            }
                        }

                        if (resultSB.Length > 1)
                            resultSB.Length--; //trim newline.

                        if (count < 1) SendReply(arg, "PrefabFinder: {0} prefab not found.", argStr);
                        else if (count > 200) SendReply(arg, "PrefabFinder: {0} contains {1} matches. Please narrow your search.", argStr, count);
                        else SendReply(arg, count.ToString("N0") + " results for: " + argStr + ": " + Environment.NewLine + resultSB.ToString());

                    }
                    finally { Facepunch.Pool.Free(ref resultSB); }
                }

                if (watch.ElapsedMilliseconds > 50) PrintWarning("PrefabFinder took: " + watch.ElapsedMilliseconds.ToString("0.##") + "ms");
            }
            finally { Facepunch.Pool.Free(ref watch); }
           
        }

    }
}