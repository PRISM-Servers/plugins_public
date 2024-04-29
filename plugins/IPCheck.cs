using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("IPCheck", "MBR", "1.1.0")]
    class IPCheck : RustPlugin
    {
        [PluginReference]
        Plugin DiscordAPI2, PRISMID;

        private readonly string perm = "ipcheck.bypass";

        private void Init() => permission.RegisterPermission(perm, this);

        private string ServerId
        {
            get
            {
                if (!(PRISMID?.IsLoaded ?? false))
                {
                    throw new Exception("PRISMID is not loaded");
                }

                return PRISMID.Call<string>("GetServerTypeString");
            }
        }

        private void OnUserConnected(IPlayer player)
        {
            if (player == null) return;

            var ip = player.Address;
            if (string.IsNullOrEmpty(ip) || permission.UserHasPermission(player.Id, perm) || permission.UserHasPermission(player.Id, "bansystem.ipbanoverride")) return;

            CheckUser(ip, player.Id, response =>
            {
                if (response == null)
                {
                    PrintWarning("Got response that was NULL");
                    return;
                }

                if (response.Count > 0)
                {
                    if (response.Any(x => x.info == "IP/SteamID match"))
                    {
                        DiscordAPI2.Call<bool>("SendToMothership", "Detected ban evasion for " + FormatSteamID(player.Id) + ", banning!");
                        Puts("Detected ban evasion for " + player.Id + ", banning!");
                        Server.Command("ban", player.Id, response.Find(x => x.info == "IP/SteamID match").reason);
                        return;
                    }

                    DiscordAPI2.Call<bool>("SendToMothership", Format(ip, player.Id, response, true));
                    Puts(Format(ip, player.Id, response));
                }
            });
        }

        private void CheckUser(string ip, string steamid, Action<List<MatchInfo>> cb)
        {
            var rv = new List<MatchInfo>();
            var token = DiscordAPI2.Call<string>("GenerateToken", "", "ipcheck_prism_" + ServerId.ToLower());

            var headers = new Dictionary<string, string>
            {
                { "User-Agent",  this.Name + " " + ServerId + "/" + this.Version },
                { "Content-Type", "application/json" },
                { "Authorization", token }
            };

            var body = Utility.ConvertToJson(new 
            {
                ip = ip,
                steamid = steamid
            });

            webrequest.Enqueue("https://prismrust.com/api/rust/ban-evasion-check", body, (code, response) =>
            {
                if (code != 200)
                {
                    PrintWarning("Failed to check ip " + ip + ": " + response);
                    cb.Invoke(rv);
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    PrintWarning("Response was empty when checking ip " + ip);
                    cb.Invoke(rv);
                    return;
                }

                if (!response.StartsWith("["))
                {
                    PrintWarning("Response was not an array when checking " + ip + "\n" + response);
                    cb.Invoke(rv);
                    return;
                }

                try
                {
                    rv = Utility.ConvertFromJson<List<MatchInfo>>(response);
                    cb.Invoke(rv);
                }

                catch (Exception e)
                {
                    PrintWarning("Exception while parsing JSON for " + ip + ": " + e.Message + "\n" + e.StackTrace);
                    Puts(response);
                    cb.Invoke(rv);
                }
            }, this, Core.Libraries.RequestMethod.POST, headers, 10);
        }

        [ConsoleCommand("ipcheck.test")]
        private void TestCMD(ConsoleSystem.Arg arg)
        {
            if (arg == null || arg?.Connection != null || arg.Args == null || arg.Args.Length < 2) return;

            var ip = arg.Args[0];
            if (string.IsNullOrEmpty(ip) || !Utility.ValidateIPv4(ip))
            {
                arg.ReplyWith(ip + " is not a valid IP");
                return;
            }

            var steamid = arg.Args[1];
            if (string.IsNullOrEmpty(steamid))
            {
                arg.ReplyWith(ip + " is not a valid steamid");
                return;
            }

            CheckUser(ip, steamid, response =>
            {
                if (response.Count > 0) Puts(Format_Simple(response));
                else Puts("No evasion detected");
            });
        }

        private string FormatSteamID(string id) => "[" + id + "](<https://steamcommunity.com/profiles/" + id + ">)";
        private string FormatIP(string ip) => "[" + ip + "](<https://whatismyipaddress.com/ip/" + ip + ">)";

        private string Format(string ip, string steamid, List<MatchInfo> list, bool links = false)
        {
            if (list.Count < 1)
            {
                return "No evasion detected";
            }

            var str = (links ? FormatSteamID(steamid) : steamid) + " (" + (links ? FormatIP(ip) : ip) + ") may be evading!";

            foreach (var item in list)
            {
                str += "\nIP: " + (links ? FormatIP(item.ip) : item.ip) + ", steamid: " + (links ? FormatSteamID(item.id) : item.id) + ", info: " + item.info + (item.mutual_friends.Count < 1 ? "" : (", mutual friends: " + string.Join(", ", item.mutual_friends)));
            }

            return str;
        }

        private string Format_Simple(List<MatchInfo> list)
        {
            if (list.Count < 1)
            {
                return "No evasion detected";
            }

            var str = "Possible ban evasion detected!";

            foreach (var item in list)
            {
                str += "\nIP: " + item.ip + ", steamid: " + item.id + ", info: " + item.info + (item.mutual_friends.Count < 1 ? "" : (", mutual friends: " + string.Join(", ", item.mutual_friends)));
            }

            return str;
        }

        class MatchInfo
        {
            public string id;
            public string ip;
            public string info;
            public string reason;
            public List<string> mutual_friends;
        }
    }
}
