using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("CCTVFix", "MBR", "1.0.2")]
    class CCTVFix : RustPlugin
    {
        private void OnServerInitialized()
        {
            var list = BaseNetworkable.serverEntities.Where(x => x is CCTV_RC rc && rc.isStatic).Select(x => x as CCTV_RC); //mbr you know i hate linq stop generating garbage you eco terrorist — shady
            var names = new List<string>();

            foreach (var item in list)
            {
                var name = item.rcIdentifier;

                if (names.Contains(name))
                {
                    Puts("Found duplicate camera ID: " + name);

                    var last_char = name[name.Length - 1];
                    var new_name_part = last_char >= '0' && last_char <= '9' ? name.TrimEnd(last_char) : name;

                    int start = 1;
                    var temp_str = new_name_part + start;

                    while (names.Contains(temp_str))
                    {
                        start++;
                        temp_str = new_name_part + start;

                        if (start >= 100)
                        {
                            temp_str = "";
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(temp_str))
                    {
                        Puts("Renaming CCTV " + name + " to " + temp_str);
                        item.UpdateIdentifier(temp_str, true);
                        names.Add(temp_str);
                    }
                }
                else names.Add(name);
            }
        }
    
        [ConsoleCommand("cctvs")]
        private void CMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            arg.ReplyWith(string.Join("\n", BaseNetworkable.serverEntities.Where(x => x is CCTV_RC && (x as CCTV_RC).isStatic).Select(x => (x as CCTV_RC).rcIdentifier)));
        }

        [ConsoleCommand("renamecctv")]
        private void RenameCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            if (arg.Args == null || arg.Args.Length < 2) return;

            var before = arg.Args[0];
            var after = arg.Args[1];

            var camera = BaseNetworkable.serverEntities.FirstOrDefault(x => x is CCTV_RC && (x as CCTV_RC).rcIdentifier == before) as CCTV_RC;
            if (!camera)
            {
                Puts("Couldn't find camera " + before);
                return;
            }

            camera.UpdateIdentifier(after.ToUpper());
            Puts("Renamed " + before + " to " + after);
        }
    }
}
