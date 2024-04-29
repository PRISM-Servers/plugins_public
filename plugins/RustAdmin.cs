using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("RustAdmin", "MBR", "1.1.0")]
    [Description("Checks for RA bans (bullshit free)")]
    class RustAdmin : RustPlugin
    {
        [PluginReference]
        Plugin DiscordAPI2, PRISMID;

        private readonly int ban_threshold = 2;
        private readonly string perm = "rustadmin.bypass";
        private readonly List<string> keywords = new List<string> { "cheat", "esp", "hack", "aimbot", "recoil" };
        private Dictionary<string, List<string>> cache = new Dictionary<string, List<string>>();

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

        private void Init() => permission.RegisterPermission(perm, this);

        private void OnUserConnected(IPlayer player) => CheckBans(player);

        private void CheckBans(IPlayer player)
        {
            if (player == null) return;

            var steamid = player.Id;

            if (permission.UserHasPermission(steamid, perm)) return;

            GetRustAdminBans(steamid, bans =>
            {
                if (bans == null) return;

                if (bans.Count > ban_threshold)
                {
                    Puts("Banning " + steamid + " beacuse of RA bans (" + bans.Count + ")");
                    Server.Command("ban", steamid, "Player RustAdminDB bans (" + bans.Count + ") > 2");
                    timer.Once(1f, () =>
                    {
                        if (player.IsConnected) player.Kick("You are banned");
                    });
                }
            });
        }

        private void GetRustAdminBans(string steamid, Action<List<string>> cb)
        {
            if (cache.ContainsKey(steamid))
            {
                cb.Invoke(cache[steamid]);
                return;
            }

            var token = DiscordAPI2.Call<string>("GenerateToken", "", "rustadmin_prism_" + ServerId.ToLower());

            var headers = new Dictionary<string, string>
            {
                { "User-Agent",  Name + " " + ServerId + "/" + Version },
                { "Authorization", token }
            };

            webrequest.Enqueue("https://prismrust.com/api/rust/ra/" + steamid + "?simple=reasons", null, (code, response) =>
            {
                if (code != 200)
                {
                    PrintWarning("Got a " + code + " when fetching RA bans for " + steamid + "\n" + response);
                    cb.Invoke(null);
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    PrintWarning("Response was empty when fetching RA bans for " + steamid);
                    cb.Invoke(null);
                    return;
                }

                List<string> bans = new List<string>();

                try { bans = Utility.ConvertFromJson<List<string>>(response); }
                catch (Exception e)
                {
                    PrintError("Exception while fetching RA bans for " + steamid + ": " + e.Message + "\n" + e.StackTrace);
                    cb.Invoke(null);
                    return;
                }

                //loop thru all ban reasons and filter out "good" ones
                foreach (var ban in bans.ToArray())
                {
                    var lwr = ban.ToLower();

                    var should_remove_ban = true;

                    foreach (var item in keywords)
                    {
                        if (lwr.Contains(item))
                        {
                            should_remove_ban = false;
                            break;
                        }
                    }

                    if (should_remove_ban)
                    {
                        Puts("Removing ban \"" + ban + "\" (no keywords)");
                        bans.Remove(ban);
                    }
                }

                cache[steamid] = bans;
                timer.Once(3600f, () =>
                {
                    if (cache.ContainsKey(steamid)) cache.Remove(steamid);
                });
                cb.Invoke(bans);
            }, this, Core.Libraries.RequestMethod.GET, headers, 10);
        }

        [ConsoleCommand("rustadmin.test")]
        private void TestCMD(ConsoleSystem.Arg arg)
        {
            if (arg == null || arg?.Connection != null || arg.Args == null || arg.Args.Length < 1) return;

            var steamid = arg.Args[0];
            if (string.IsNullOrEmpty(steamid) || !steamid.StartsWith("765611"))
            {
                arg.ReplyWith(steamid + " is not a steamid");
                return;
            }

            GetRustAdminBans(steamid, bans =>
            {
                if (bans == null || bans.Count < 1) Puts("No bans for " + steamid);
                else Puts("Bans for " + steamid + ":\n" + string.Join("\n", bans));
            });
        }
    }
}
