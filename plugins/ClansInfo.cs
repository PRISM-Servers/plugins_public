// Requires: Clans
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("ClansInfo", "Shady", "1.0.5", ResourceId = 0)]
    internal class ClansInfo : RustPlugin
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
        private bool init = false;
        

        private void Init()
        {

            var clanInfoAliases = new string[] { "claninfo", "claninf", "claninfoo", "ci", "clainfo", "cinfo" };

            var clanListAliases = new string[] { "clanlist", "clanslist", "cl" };

            AddCovalenceCommand(clanInfoAliases, nameof(cmdClanInfo));
            AddCovalenceCommand("clanrename", "cmdClanRename");
            AddCovalenceCommand(clanListAliases, nameof(cmdClanList));
            AddCovalenceCommand("clandesc", "cmdClanDesc");
        }

        [PluginReference]
        private readonly Clans Clans;

        
        //the compiler was completely screwed by having a regular Luck reference and I have no idea why
        private RustPlugin _luck = null;
     
        private RustPlugin Luck
        {
            get
            {
                if (_luck == null || !_luck.IsLoaded)
                {
                    _luck = plugins.Find("Luck") as RustPlugin;
                    PrintWarning("luck was null or unloaded now: " + (_luck == null) + " isloaded: " + (_luck?.IsLoaded ?? false));
                }
                
                return _luck;
            }
        }

        private readonly Regex tagRe = new Regex("^[a-zA-Z0-9]{2,7}$");

        private void OnServerInitialized()
        {
            init = true;
            AddCovalenceCommand("teamlist", "cmdTeamsList");
        }

        
       private void SendReply(IPlayer player, string msg, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : (keepTagsConsole) ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else player.Reply(msg);
        }

        private void cmdClanInfo(IPlayer player, string command, string[] args)
        {
            if (args.Length <= 0)
            {
                SendReply(player, "You must supply arguments! Try: /" + command + " ClanNameGoesHere");
                return;
            }
            var clanName = args[0];
            var clan = Clans?.Call<Clans.Clan>("findClan", clanName) ?? null;
            if (clan == null)
            {
                SendReply(player, "Unable to find clan: " + clanName);
                return;
            }
            var ownerUID = 0ul;
            if (!ulong.TryParse(clan.owner, out ownerUID))
            {
                SendReply(player, "Failed to get clan leader!");
                return;
            }
            var owner = BasePlayer.FindByID(ownerUID)?.displayName ?? BasePlayer.FindSleeping(ownerUID)?.displayName ?? covalence.Players.FindPlayerById(clan.owner)?.Name ?? "Unknown";
            
            SendReply(player, "<color=#ff912b>" + clan.tag + "</color> - <color=orange>" + clan.description + "</color>, <color=orange>Leader: </color><color=#ff912b>" + owner + "</color>");
            
            if (clan.members.Count > 1)
            {
                var membersSB = new StringBuilder();

                var onlineCount = 0;

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p != null && p.IsConnected && clan.members.Contains(p.UserIDString)) onlineCount++;
                }

                membersSB.Append("<color=green>" + onlineCount.ToString("N0") + "</color>/<color=#ff912b>" + clan.members.Count.ToString("N0") + "</color> Online <color=orange>Members:</color>\n");
                
                for (int i = 0; i < clan.members.Count; i++)
                {
                    var member = (covalence.Players?.FindPlayerById(clan.members[i])) ?? null;
                    if (member == null) continue;

                    var userClassName = Luck?.Call<string>("GetUserClassName", member.Id) ?? string.Empty;
                    var userClassColor = Luck?.Call<string>("GetUserClassPrimaryColor", member.Id) ?? string.Empty;

                    var userClassStr = string.IsNullOrEmpty(userClassName) ? string.Empty : ("<color=" + userClassColor + ">" + userClassName + "</color>");

                    membersSB.Append(member.Name).Append(" (<color=").Append((member?.IsConnected ?? false) ? "green>Online" : "red>Offline").Append("</color>)").Append(!string.IsNullOrEmpty(userClassStr) ? " (" : string.Empty).Append(userClassStr).Append(!string.IsNullOrEmpty(userClassStr) ? ")" : string.Empty).Append(", ");
                }

                membersSB.Length -= 2;

                SendReply(player, membersSB.ToString());
            }
        }

        private void cmdTeamsList(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var teamMgr = RelationshipManager.ServerInstance;
            if (teamMgr == null)
            {
                SendReply(player, "RelationshipManager.ServerInstance is null!!");
                return;
            }
            if (teamMgr.teams == null)
            {
                SendReply(player, "teams is null?!");
                return;
            }
            if (teamMgr.teams.Count < 1)
            {
                SendReply(player, "No teams found!");
                return;
            }
            else SendReply(player, "Showing " + teamMgr.teams.Count.ToString("N0") + " teams:");
            var teamSB = new StringBuilder();
            var teamSBs = new List<StringBuilder>();
            var count = 0;
            foreach(var kvp in teamMgr.teams.OrderBy(p => p.Key))
            {
                count++;
                var team = kvp.Value;
                if (team == null) continue;
                var leader = team?.GetLeader() ?? null;
                if (leader == null)
                {
                    PrintWarning("team with null leader?!");
                    continue;
                }
                var leaderName = leader?.displayName ?? "Unknown";
                var leaderId = leader?.userID ?? 0;
                var strSB = new StringBuilder();
                var dashStr = string.Empty;
                for(int i = 0; i < team.members.Count; i++)
                {
                    var member = team.members[i];
                    if (member == leaderId) continue;
                    dashStr += "-";
                    strSB.AppendLine(dashStr + "<color=yellow>" + GetDisplayNameFromID(member) + "</color>");
                }
                var str = count + ": " + team.teamID + ": <color=#eb3dff>" + leaderName + "</color> leads <color=#42dfff>" + (team.members.Count > 1 ? team.members.Count - 1 : team.members.Count).ToString("N0") + "</color>: " + (strSB.Length > 0 ? ("\n" + strSB.ToString().TrimEnd()) : "himself");
                if ((teamSB.Length + str.Length) >= 800)
                {
                    teamSBs.Add(teamSB);
                    teamSB = new StringBuilder();
                }
                teamSB.AppendLine(str);
            }
            if (teamSBs.Count > 0) for (int i = 0; i < teamSBs.Count; i++) SendReply(player, teamSBs[i].ToString().TrimEnd());
            if (teamSB.Length > 0) SendReply(player, teamSB.ToString().TrimEnd());
        }

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return covalence.Players.FindPlayerById(userID)?.Name ?? BasePlayer.activePlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? BasePlayer.sleepingPlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? string.Empty;
        }

        private string GetDisplayNameFromID(ulong userID) { return GetDisplayNameFromID(userID.ToString()); }


        private void cmdClanList(IPlayer player, string command, string[] args)
        {
           // var startTime = UnityEngine.Time.realtimeSinceStartup;
            var clanList = Clans?.Call<JArray>("GetAllClans") ?? null;
            if (clanList == null || clanList.Count < 1)
            {
                SendReply(player, "Unable to find any clans!");
                return;
            }
            var clanSB = new StringBuilder();
            var finishList = new List<StringBuilder>();
            for(int i = 0; i < clanList.Count; i++)
            {
                var clan = clanList[i].ToString();
                if (string.IsNullOrEmpty(clan))
                {
                    PrintWarning("Bad/null clan, index: " + i);
                    continue;
                }
                var getClan = Clans?.Call<Clans.Clan>("findClan", clan) ?? null;
                var memberCount = (getClan?.members?.Count ?? 0).ToString("N0");
                var appendMsg = "<color=#ff912b>" + clan + "</color> <color=#8aff47>(<color=orange>" + memberCount + "</color> mem.)</color>";
                if (clanSB.Length >= 1024 || (clanSB.Length + appendMsg.Length) >= 1024)
                {
              //      PrintWarning(">= 1024 length, adding to finishlist!");
                    finishList.Add(clanSB);
                    clanSB = new StringBuilder();
                }
                clanSB.AppendLine(appendMsg);
            }
            for (int i = 0; i < finishList.Count; i++) SendReply(player, finishList[i].ToString().TrimEnd());
            if (clanSB.Length > 0) SendReply(player, clanSB.ToString().TrimEnd());
            SendReply(player, "Showing " + clanList.Count + " clans");
        }

        private void cmdClanRename(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;
            var clan = Clans?.Call<Clans.Clan>("findClanByUser", player.Id) ?? null;
            if (clan == null)
            {
                SendReply(player, "You are not in a clan!");
                return;
            }
            if (clan.owner != player.Id)
            {
                SendReply(player, "You do not own this clan!");
                return;
            }
            if (args.Length < 1)
            {
                SendReply(player, "You must specify a new name!");
                return;
            }
            var arg0 = args[0];
            if (arg0.Length < 2 || arg0.Length > 7)
            {
                SendReply(player, "Clan name too short or too long!");
                return;
            }
            if (!tagRe.IsMatch(args[0]))
            {
                SendReply(player, "Clan tags must be 2 to 7 characters long and may contain standard letters and numbers only");
                return;
            }
            var findClan = Clans?.Call<Clans.Clan>("findClan", arg0) ?? null;
            if (findClan != null && findClan != clan)
            {
                SendReply(player, "A different clan already exists with this tag.");
                return;
            }
            if (clan.tag == arg0)
            {
                SendReply(player, "The clan already has this name");
                return;
            }

            clan.tag = arg0;
            clan.Broadcast(player.Name + " has renamed the clan to: " + clan.tag);
        }

     
        private void cmdClanDesc(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;
            var clan = Clans?.Call<Clans.Clan>("findClanByUser", player.Id) ?? null;
            if (clan == null)
            {
                SendReply(player, "You are not in a clan!");
                return;
            }
            if (clan.owner != player.Id)
            {
                SendReply(player, "You do not own this clan!");
                return;
            }
            if (args.Length < 1)
            {
                SendReply(player, "You must specify a new description!");
                return;
            }
            var argStr = string.Join(" ", args);
            if (argStr.Length < 1)
            {
                SendReply(player, "Clan description too short!");
                return;
            }


            clan.description = argStr;
            clan.Broadcast(player.Name + " has changed the clan description to: " + clan.description);
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;
            //	Forbidden formatting tags
            var forbiddenTags = new List<string>{
                "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>"
            };

            //	Replace Color Tags
            phrase = new Regex("(<color=.+?>)").Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = new Regex("(<size=.+?>)").Replace(phrase, string.Empty);
            var phraseSB = new StringBuilder(phrase);

            for (int i = 0; i < forbiddenTags.Count; i++) phraseSB.Replace(forbiddenTags[i], string.Empty);
            

            return phraseSB.ToString();
        }
    }
}
