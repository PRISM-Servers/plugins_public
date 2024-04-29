using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("SteamChecks", "MBR", "2.0.0")]
    [Description("Checks for steam bans")]
    class SteamChecks : CovalencePlugin
    {
        private readonly int ban_threshold = 17;
        private readonly int ban_count_threshold = 1;
        private readonly int account_age_threshold = 2;

        private readonly string age_bypass = "steamchecks.agebypass";
        private readonly string ban_bypass = "steamchecks.banbypass";
        private readonly string twitter_bypass = "steamchecks.twitterbypass";

        [PluginReference]
        Plugin DiscordAPI2, PRISMID;

        private void Init()
        {
            permission.RegisterPermission(age_bypass, this);
            permission.RegisterPermission(ban_bypass, this);
            permission.RegisterPermission(twitter_bypass, this);
        }

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
            CheckBans(player);
        }

        private void CheckBans(IPlayer player)
        {
            if (player == null) return;

            var steamid = player.Id;

            GetSteamAccountInfo(steamid, userinfo =>
            {
                if (userinfo.ban_count != -1 && userinfo.ban_count > ban_count_threshold && !permission.UserHasPermission(steamid, ban_bypass))
                {
                    //2 Steam bans > 1
                    Puts("Banning " + steamid + " because of steam ban count (" + userinfo.ban_count + ")");
                    server.Command("ban", steamid, userinfo.ban_count + " Steam bans > " + ban_count_threshold);
                    timer.Once(1f, () =>
                    {
                        if (player.IsConnected) player.Kick("You are banned");
                    });
                    return;
                }

                if (userinfo.days_since_last_ban != -1 && userinfo.ban_count > 0 && userinfo.days_since_last_ban < ban_threshold && !permission.UserHasPermission(steamid, ban_bypass))
                {   //25 Days since last Steam ban < 60
                    Puts("Banning " + steamid + " because of recent steam ban(s) (" + userinfo.days_since_last_ban + " day(s) old ban(s))");
                    server.Command("ban", steamid, userinfo.days_since_last_ban + " Days since last Steam ban < " + ban_threshold);
                    timer.Once(1f, () =>
                    {
                        if (player.IsConnected) player.Kick("You are banned");
                    });
                    return;
                }

                if (userinfo.account_age_days != -1 && userinfo.account_age_days < account_age_threshold && !permission.UserHasPermission(steamid, age_bypass))
                {
                    var kick_message = "<color=#FF0030>Your Steam account is too young! You can wait or get approval from a moderator on Discord:</color> <color=#2BACE2>discord.gg/DUCnZhZ</color>";

                    Puts("Kicking " + steamid + " because of account age (" + userinfo.account_age_days + " days)");
                    player.Kick(kick_message);

                    timer.Once(1f, () =>
                    {
                        if (player.IsConnected) player.Kick(kick_message);
                    });
                    return;
                }
            });
        }

        private void GetSteamAccountInfo(string steamid, Action<UserInfo> cb)
        {
            var token = DiscordAPI2.Call<string>("GenerateToken", "", "steamcheck_prism_" + ServerId.ToLower());

            var headers = new Dictionary<string, string>
            {
                { "User-Agent",  this.Name + " " + ServerId + "/" + this.Version },
                { "Authorization", token }
            };

            webrequest.Enqueue("https://prismrust.com/api/steam/user/" + steamid + "?fields=bans,user", null, (code, response) =>
            {
                var rv = new UserInfo();

                if (code != 200)
                {
                    PrintWarning("Got a " + code + " when fetching steam account info for " + steamid + "\n" + response);
                    timer.Once(5f, () => GetSteamAccountInfo(steamid, cb));
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    PrintWarning("Response was empty when fetching steam account info for " + steamid);
                    timer.Once(5f, () => GetSteamAccountInfo(steamid, cb));
                    return;
                }

                try
                {
                    Response json = Utility.ConvertFromJson<Response>(response);

                    if (json.bans != null)
                    {
                        rv.ban_count = json.bans.NumberOfGameBans + json.bans.NumberOfVACBans;
                        rv.days_since_last_ban = json.bans.DaysSinceLastBan;
                    }

                    if (json.user != null && json.user.timecreated.HasValue)
                    {
                        rv.account_age_days = DateTime.UtcNow.Subtract(json.user.timecreated.Value).TotalDays;
                    }

                    cb.Invoke(rv);
                }

                catch (Exception e)
                {
                    PrintError("Exception while fetching steam account info for " + steamid + ": " + e.Message + "\n" + e.StackTrace + "\n" + response);
                    timer.Once(5f, () => GetSteamAccountInfo(steamid, cb));
                }
            }, this, Core.Libraries.RequestMethod.GET, headers, 10);
        }

        public class Bans
        {
            public int NumberOfVACBans { get; set; } = 0;
            public int DaysSinceLastBan { get; set; } = 0;
            public int NumberOfGameBans { get; set; } = 0;
        }

        public class User
        {
            public string steamid { get; set; }
            public int communityvisibilitystate { get; set; }
            public int profilestate { get; set; }
            public string personaname { get; set; }
            public int commentpermission { get; set; }
            public string profileurl { get; set; }
            public string avatar { get; set; }
            public string avatarmedium { get; set; }
            public string avatarfull { get; set; }
            public string avatarhash { get; set; }
            public int lastlogoff { get; set; }
            public int personastate { get; set; }
            public string realname { get; set; }
            public string primaryclanid { get; set; }
            public DateTime? timecreated { get; set; }
            public int personastateflags { get; set; }
        }

        public class Response
        {
            public Bans bans = null;
            public User user = null;
        }

        class UserInfo
        {
            public int ban_count = -1;
            public int days_since_last_ban = -1;
            public double account_age_days = -1;
        }

        [Command("steamchecks.test")]
        void TestCMD(IPlayer player, string command, string[] args)
        {
            if (!player.IsServer || args.Length < 1) return;

            GetSteamAccountInfo(args[0], i =>
            {
                player.Reply("Account is " + i.account_age_days + " days old, has " + i.ban_count + " bans and last one " + i.days_since_last_ban + " days ago");
            });
        }
    }
}
