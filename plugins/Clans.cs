using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Clans", "Shady", "1.9.4", ResourceId = 0/*/842/*/)]
    public class Clans : RustPlugin
    {
        //originally by dcode. huge edits/additions/changes by me (Shady) seem worthy of changing authorship - includes (forced) team system integration
        [PluginReference]
        private readonly Plugin Compilation;

        [PluginReference]
        private readonly Plugin BanSystem;

        [PluginReference]
        private readonly Plugin Vanish;

        [PluginReference]
        private readonly Plugin Deathmatch;

        private readonly Dictionary<string, float> _lastAttemptedInvite = new Dictionary<string, float>();

        #region Rust:IO Bindings

        public class Clan
        {
            public string tag;
            public string description;
            public string owner;
            public List<string> moderators = new List<string>();
            public List<string> members = new List<string>();
            public List<string> invited = new List<string>();

            public static Clan Create(string tag, string description, string ownerId)
            {
                var clan = new Clan() { tag = tag, description = description, owner = ownerId };
                clan.members.Add(ownerId);
                return clan;
            }

            public bool IsOwner(string userId)
            {
                return userId == owner;
            }

            public bool IsModerator(string userId)
            {
                return moderators.Contains(userId);
            }

            public bool IsMember(string userId)
            {
                return members.Contains(userId);
            }

            public bool IsInvited(string userId)
            {
                return invited.Contains(userId);
            }

            public void Broadcast(string message)
            {
                if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
                for (int i = 0; i < members.Count; i++)
                {
                    var memberId = members[i];
                    var player = BasePlayer.FindByID(Convert.ToUInt64(memberId));
                    if (player == null) continue;
                    player.SendConsoleCommand("chat.add", string.Empty, 0, "<color=#8ae6ff><size=10>[<size=12>Clan</size>]</size></color> " + message);
                }
            }

            internal JObject ToJObject()
            {
                var obj = new JObject();
                obj["tag"] = tag;
                obj["description"] = description;
                obj["owner"] = owner;
                var jmoderators = new JArray();
                for (int i = 0; i < moderators.Count; i++) jmoderators.Add(moderators[i]);
                obj["moderators"] = jmoderators;
                var jmembers = new JArray();
                for (int i = 0; i < members.Count; i++) jmembers.Add(members[i]);
                obj["members"] = jmembers;
                var jinvited = new JArray();
                for (int i = 0; i < invited.Count; i++) jinvited.Add(invited[i]);
                obj["invited"] = jinvited;
                return obj;
            }

            internal void OnCreate() => Interface.CallHook("OnClanCreate", tag);
            internal void OnUpdate() => Interface.CallHook("OnClanUpdate", tag);
            internal void OnDestroy() => Interface.CallHook("OnClanDestroy", tag);
        }

        private Library lib;
        private MethodInfo isInstalled;
        private MethodInfo hasFriend;
        private MethodInfo addFriend;
        private MethodInfo deleteFriend;
        internal static Clans Instance = null;

      //  FieldInfo displayName = typeof(BasePlayer).GetField("_displayName", (BindingFlags.Instance | BindingFlags.NonPublic));

        private void ioInitialize()
        {
            lib = Interface.Oxide.GetLibrary<Library>("RustIO");
            if (lib == null || (isInstalled = lib.GetFunction("IsInstalled")) == null || (hasFriend = lib.GetFunction("HasFriend")) == null || (addFriend = lib.GetFunction("AddFriend")) == null || (deleteFriend = lib.GetFunction("DeleteFriend")) == null)
            {
                lib = null;
                Puts("{0}: {1}", Title, "Rust:IO is not present. You need to install Rust:IO first in order to use this plugin!");
            }
        }

        private bool ioIsInstalled()
        {
            if (lib == null) return false;
            return (bool)isInstalled.Invoke(lib, new object[] { });
        }

        private bool ioHasFriend(string playerId, string friendId)
        {
            if (lib == null) return false;
            return (bool)hasFriend.Invoke(lib, new object[] { playerId, friendId });
        }

        private bool ioAddFriend(string playerId, string friendId)
        {
            if (lib == null) return false;
            return (bool)addFriend.Invoke(lib, new object[] { playerId, friendId });
        }

        private bool ioDeleteFriend(string playerId, string friendId)
        {
            if (lib == null) return false;
            return (bool)deleteFriend.Invoke(lib, new object[] { playerId, friendId });
        }

        #endregion

        private readonly Dictionary<string, Clan> clans = new Dictionary<string, Clan>();
        private readonly Dictionary<string, string> originalNames = new Dictionary<string, string>();
        private readonly Regex tagRe = new Regex("^[a-zA-Z0-9]{2,7}$");
        private readonly Dictionary<string, string> messages = new Dictionary<string, string>();
        private readonly Dictionary<string, Clan> lookup = new Dictionary<string, Clan>();
        private bool addClanMatesAsFriends = true;
        private int limitMembers = -1;
        private int limitModerators = -1;

        // Loads the data file
        private void loadData()
        {
            clans.Clear();
            var data = Interface.Oxide.DataFileSystem.GetDatafile("rustio_clans");
            if (data["clans"] != null)
            {
                var clansData = (Dictionary<string, object>)Convert.ChangeType(data["clans"], typeof(Dictionary<string, object>));
                var sb = new StringBuilder();
                foreach (var iclan in clansData)
                {
                    string tag = iclan.Key;
                    var clanData = iclan.Value as Dictionary<string, object>;
                    string description = (string)clanData["description"];
                    string owner = (string)clanData["owner"];
                    List<string> moderators = new List<string>();
                    List<string> members = new List<string>();
                    List<string> invited = new List<string>();
                    var clanDataMods = clanData["moderators"] as List<object>;
                    var clanDataMembers = clanData["members"] as List<object>;
                    var clanDataInvited = clanData["invited"] as List<object>;
                    for (int i = 0; i < clanDataMods.Count; i++)
                    {
                        moderators.Add((string)clanDataMods[i]);
                    }
                    for(int i = 0; i < clanDataMembers.Count; i++)
                    {
                        members.Add((string)clanDataMembers[i]);
                    }
                    for(int i = 0; i < clanDataInvited.Count; i++)
                    {
                        invited.Add((string)clanDataInvited[i]);
                    }
                    if (members?.Count < 1)
                    {
                        sb.AppendLine("Found clan with < 1 member!! tag: " + tag + " - will try to add leader if exists");
                        if (!string.IsNullOrEmpty(owner))
                        {
                           // var added = false;
                            members.Add(owner);
                            sb.AppendLine("Successfully added leader to clan " + tag + " that was lacking any members");
                        }
                        else
                        {
                            sb.AppendLine("Couldn't add anyone to clan withoue members: " + tag + " !!!");
                            continue;
                        }
                    }
                    Clan clan;
                    clans.Add(tag, clan = new Clan()
                    {
                        tag = tag,
                        description = description,
                        owner = owner,
                        moderators = moderators,
                        members = members,
                        invited = invited
                    });
                    lookup[owner] = clan;
                    for (int i = 0; i < members.Count; i++) lookup[members[i]] = clan;
                }
                if (sb.Length > 0) PrintWarning(sb.ToString().TrimEnd());
            }
        }

        // Saves the data file
        private void saveData()
        {
            var data = Interface.Oxide.DataFileSystem.GetDatafile("rustio_clans");
            var clansData = new Dictionary<string, object>();
            foreach (var clan in clans)
            {
                var clanData = new Dictionary<string, object>();
                clanData.Add("tag", clan.Value.tag);
                clanData.Add("description", clan.Value.description);
                clanData.Add("owner", clan.Value.owner);
                var moderators = new List<object>();
                var members = new List<object>();
                var invited = new List<object>();
                for (int i = 0; i < clan.Value.moderators.Count; i++)
                {
                    var imoderator = clan.Value.moderators[i];
                    moderators.Add(imoderator);
                }
                for(int i = 0; i < clan.Value.members.Count; i++)
                {
                    members.Add(clan.Value.members[i]);
                }
                for(int i = 0; i < clan.Value.invited.Count; i++)
                {
                    invited.Add(clan.Value.invited[i]);
                }
                clanData.Add("moderators", moderators);
                clanData.Add("members", members);
                clanData.Add("invited", invited);
                clansData.Add(clan.Value.tag, clanData);
            }
            data["clans"] = clansData;
            Interface.Oxide.DataFileSystem.SaveDatafile("rustio_clans");
        }

        [HookMethod("Unload")]
        private void Unload()
        {
            saveData();
            try
            {
                if (ServerMgr.Instance != null && UpdateAllRoutine != null) ServerMgr.Instance.StopCoroutine(UpdateAllRoutine);
                var disbanded = 0;
                foreach (var kvp in clanToTeam)
                {
                    var team = kvp.Value;
                    if (team != null)
                    {
                        DisbandTeam(team);
                        disbanded++;
                    }
                }
                PrintWarning("Disbanded " + disbanded.ToString("N0") + "/" + clanToTeam.Count.ToString("N0") + " teams on unload");
                // Reset player names to originals
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null) continue;
                    player.displayName = stripTag(player.displayName);
                }
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var player = BasePlayer.sleepingPlayerList[i];
                    if (player == null || (player?.IsConnected ?? false)) continue;
                    player.displayName = stripTag(player.displayName);
                }

                /*/
                foreach (var pair in originalNames)
                {
                    var playerId = Convert.ToUInt64(pair.Key);
                    var player = BasePlayer.FindByID(playerId) ?? BasePlayer.FindSleeping(playerId);
                    var displayNamee = player?.displayName ?? string.Empty;
                    var IDString = player?.UserIDString ?? string.Empty;
                    if (displayNamee == "Shady" && IDString == "76561198028248023") continue;
                    if (player != null)
                    {
                        player.displayName = pair.Value;
                        if (player.IsConnected) player.SendNetworkUpdate();
                    }
                }/*/
            }
            catch (Exception ex)
            {
                error("Unload failed", ex);
            }
        }

        // A list of all translateable texts
        private readonly List<string> texts = new List<string>() {
            "%NAME% has come online!",
            "%NAME% has gone offline.",

            "You are currently not a member of a clan.",
            "You are the owner of:",
            "You are a moderator of:",
            "You are a member of:",
            "Members online:",
            "Pending invites:",
            "To learn more about clans, type: <color=#ffd479>/clan help</color>",

            "Usage: <color=#ffd479>/clan create \"TAG\" \"Description\"</color>",
            "You are already a member of a clan.",
            "Clan tags must be 2 to 6 characters long and may contain standard letters and numbers only",
            "Please provide a short description of your clan.",
            "There is already a clan with this tag.",
            "You are now the owner of your new clan:",
            "To invite new members, type: <color=#ffd479>/clan invite \"Player name\"</color>",

            "Usage: <color=#ffd479>/clan invite \"Player name\"</color>",
            "You need to be a moderator of your clan to use this command.",
            "No such player or player name not unique:",
            "This player is already a member of your clan:",
            "This player is not a member of your clan:",
            "This player has already been invited to your clan:",
            "This player is already a moderator of your clan:",
            "This player is not a moderator of your clan:",
            "%MEMBER% invited %PLAYER% to the clan.",
            "Usage: <color=#ffd479>/clan join \"TAG\"</color>",
            "You have not been invited to join this clan.",
            "%NAME% has joined the clan!",
            "You have been invited to join the clan:",
            "To join, type: <color=#ffd479>/clan join \"%TAG%\"</color>",
            "This clan has already reached the maximum number of members.",
            "This clan has already reached the maximum number of moderators.",

            "Usage: <color=#ffd479>/clan promote \"Player name\"</color>",
            "You need to be the owner of your clan to use this command.",
            "%OWNER% promoted %MEMBER% to moderator.",

            "Usage: <color=#ffd479>/clan demote \"Player name\"</color>",

            "Usage: <color=#ffd479>/clan leave</color>",
            "You have left your current clan.",
            "%NAME% has left the clan.",

            "Usage: <color=#ffd479>/clan kick \"Player name\"</color>",
            "This player is an owner or moderator and cannot be kicked:",
            "%NAME% kicked %MEMBER% from the clan.",

            "Usage: <color=#ffd479>/clan disband forever</color>",
            "Your current clan has been disbanded forever.",

            "Usage: <color=#ffd479>/clan delete \"TAG\"</color>",
            "You need to be a server owner to delete clans.",
            "There is no clan with that tag:",
            "Your clan has been deleted by the server owner.",
            "You have deleted the clan:",

            "Available commands:",
            "<color=#ffd479>/clan</color> - Displays relevant information about your current clan",
            "<color=#ffd479>/c Message...</color> - Sends a message to all online clan members",
            "<color=#ffd479>/clan create \"TAG\" \"Description\"</color> - Creates a new clan you own",
            "<color=#ffd479>/clan join \"TAG\"</color> - Joins a clan you have been invited to",
            "<color=#ffd479>/clan leave</color> - Leaves your current clan",
            "<color=#74c6ff>Moderator</color> commands:",
            "<color=#ffd479>/clan invite \"Player name\"</color> - Invites a player to your clan",
            "<color=#ffd479>/clan kick \"Player name\"</color> - Kicks a member from your clan",
            "<color=#a1ff46>Owner</color> commands:",
            "<color=#ffd479>/clan promote \"Name\"</color> - Promotes a member to moderator",
            "<color=#ffd479>/clan demote \"Name\"</color> - Demotes a moderator to member",
            "<color=#ffd479>/clan disband forever</color> - Disbands your clan (no undo)",
            "<color=#cd422b>Server owner</color> commands:",
            "<color=#ffd479>/clan delete \"TAG\"</color> - Deletes a clan (no undo)",

            "<color=#ffd479>/clan</color> - Displays your current clan status",
            "<color=#ffd479>/clan help</color> - Learn how to create or join a clan"
        };

        // Loads the default configuration
        protected override void LoadDefaultConfig()
        {
            var messages = new Dictionary<string, object>();
            for(int i = 0; i < texts.Count; i++)
            {
                var text = texts[i];
                if (messages.ContainsKey(text))
                    Puts("{0}: {1}", Title, "Duplicate translation string: " + text);
                else
                    messages.Add(text, text);
            }
            Config["messages"] = messages;
            Config.Set("addClanMatesAsFriends", true);
            Config.Set("limit", "members", -1);
            Config.Set("limit", "moderators", -1);
        }

        // Translates a string
        private string _(string text, Dictionary<string, string> replacements = null)
        {
            if (messages.ContainsKey(text) && messages[text] != null)
                text = messages[text];
            if (replacements != null)
                foreach (var replacement in replacements)
                    text = text.Replace("%" + replacement.Key + "%", replacement.Value);
            return text;
        }

        // Finds a clan by tag
        private Clan findClan(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;
            Clan clan;
            if (clans.TryGetValue(tag, out clan) && clan != null) return clan; //try to find by exact key name (case sensitive) first

            foreach(var kvp in clans)
            {
                if (kvp.Value.tag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    clan = kvp.Value;
                    break;
                }
            }
            return clan;
       //     return clans?.Where(p => p.Value.tag.Equals(tag, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault().Value ?? null;
            /*/
            Clan clan;
            if (clans.TryGetValue(tag, out clan))
                return clan;/*/
          //  return null;
        }

        // Finds a user's clan
        private Clan findClanByUser(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            Clan clan;
            return lookup.TryGetValue(userId, out clan) ? clan : null;
        }

        private List<Clan> GetAllClansList()
        {
            if (clans == null || clans.Count < 1) return null;
            var clansList = new List<Clan>();
            foreach (var kvp in clans) clansList.Add(kvp.Value);
            return clansList;
        }

        private IPlayer FindClanPlayer(string nameOrIdOrIp, Clan clan)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));
            if (clan == null) throw new ArgumentNullException(nameof(clan));


            var members = clan?.members ?? null;
            if ((members?.Count ?? 0) < 1) return null;

            var foundMembers = Facepunch.Pool.GetList<IPlayer>();
            try
            {
                for (int i = 0; i < members.Count; i++)
                {
                    var member = members[i];
                    var player = covalence.Players?.FindPlayerById(member) ?? null;
                    if (player == null) continue;

                    var pName = player?.Name ?? string.Empty;
                    if ((player?.IsConnected ?? false) && player.Address == nameOrIdOrIp)
                    {
                       // PrintWarning("connected && adddress == nameidip: " + nameOrIdOrIp);
                        foundMembers.Add(player);
                    }
                    else if (player.Id == nameOrIdOrIp)
                    {
                        //PrintWarning("player.id == nameidip " + nameOrIdOrIp);
                        foundMembers.Add(player);
                    }
                    else if (pName.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase))
                    {
                       // PrintWarning("pname == nameidip: " + nameOrIdOrIp);
                        foundMembers.Add(player);
                    }
                    else if (pName.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        //PrintWarning("pname indexof nameidip: " + nameOrIdOrIp);
                        foundMembers.Add(player);
                    }
                    //if ((player?.IsConnected ?? false && player.Address == nameOrIdOrIp) || player.Id == nameOrIdOrIp || pName.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || pName.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0) foundMembers.Add(player);
                }

                return foundMembers.Count == 1 ? foundMembers[0] : null;
            }
            finally { Facepunch.Pool.FreeList(ref foundMembers); }

        }

        private IPlayer FindClanPlayerByTag(string nameOrIdOrIp, string clan)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));
            if (string.IsNullOrEmpty(clan)) throw new ArgumentNullException(nameof(clan));

            return FindClanPlayer(nameOrIdOrIp, findClan(clan));
        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));

            var p = covalence.Players.FindPlayerById(nameOrIdOrIp);
            if (p != null) if ((!p.IsConnected && tryFindOfflineIfNoOnline) || p.IsConnected) return p;


            var players = Facepunch.Pool.GetList<IPlayer>();
            try
            {
                var connected = covalence.Players.Connected;
                foreach (var player in connected)
                {
                    if (players.Count > 1) break;
                    if (player.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(player);
                    else if (player.Address == nameOrIdOrIp) players.Add(player);
                    else if (player.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0) players.Add(player);
                }
                if (players.Count < 1 && tryFindOfflineIfNoOnline)
                {
                    foreach (var player in covalence.Players.All)
                    {
                        if (player.IsConnected) continue;
                        if (players.Count > 1) break;
                        if (player.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(player);
                        else if (player.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0) players.Add(player);
                    }
                }
                return players.Count == 1 ? players[0] : null;
            }
            finally { Facepunch.Pool.FreeList(ref players); }
        }

        // Strips the tag from a player's name
        private string stripTag(string name, Clan clan = null)
        {
            if (clan == null)
            {
                Clan foundClan;
                foreach(var kvp in clans)
                {
                    var val = kvp.Value;
                    if (name.Contains(val.tag))
                    {
                        foundClan = val;
                        break;
                    }
                }
            }
            if (clan == null) return name;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try 
            {
                var re = new Regex(sb.Clear().Append(@"^\[").Append(clan.tag).Append(@"\]\s").ToString());

                
                while (re.IsMatch(sb.ToString()))
                    name = name.Substring(clan.tag.Length + 3);
                

                return name;
            }
            finally { Facepunch.Pool.Free(ref sb); }
        }
        /*/
        // Sets up a player to use the correct clan tag
        private void setupPlayer(BasePlayer player)
        {
     //      var prevName = player.displayName;
            var playerId = player.UserIDString;
            var clan = findClanByUser(playerId);
       //     if (player.displayName == "Shady" && playerId == "76561198028248023") return;
     //       player.displayName = stripTag(player.displayName, clan);
          //  displayName.SetValue(player, stripTag(player.displayName, clan));
           // var originalName = string.Empty;

       //     if (!originalNames.ContainsKey(playerId)) originalNames.Add(playerId, originalName = player.displayName);
           // else originalName = originalNames[playerId];

            if (clan == null) player.displayName = originalName;
            else
            {
                var tag = "[" + clan.tag + "]" + " ";
                if (!player.displayName.StartsWith(tag)) player.displayName = tag + originalName;
            }
          //  if (player.displayName != prevName) player.SendNetworkUpdate();
        }

        // Sets up all players contained in playerIds
        private void setupPlayers(List<string> playerIds)
        {
            for(int i = 0; i < playerIds.Count; i++)
            {
                var playerId = playerIds[i];
                var uid = Convert.ToUInt64(playerId);
                var player = BasePlayer.FindByID(uid);
                if (player != null)
                    setupPlayer(player);
                else
                {
                    player = BasePlayer.FindSleeping(uid);
                    if (player != null)
                        setupPlayer(player);
                }
            }
        }/*/

        [HookMethod("Init")]
        private void Init()
        {
            Instance = this;
            AddCovalenceCommand(new string[] { "clan", "clans", "klan", "klans" }, nameof(cmdChatClan));

            var clansInfo = plugins.Find("ClansInfo");
            if (clansInfo == null || !clansInfo.IsLoaded)
            {
                covalence.Server.Command("o.load ClansInfo");
            }
        }

        private readonly Dictionary<string, RelationshipManager.PlayerTeam> clanToTeam = new Dictionary<string, RelationshipManager.PlayerTeam>();

        private System.Collections.IEnumerator UpdateAllClanTeams()
        {
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();
                var sb = Facepunch.Pool.Get<StringBuilder>();
                try
                {
                    PrintWarning(sb.Clear().Append("UpdateAllTeams() being called for: ").Append(clans.Count.ToString("N0")).Append(" clans").ToString());

                    foreach (var kvp in clans)
                    {
                        UpdateClanTeam(kvp.Value);
                        if (Performance.report.frameRate < 60 && Performance.report.frameRateAverage >= 60) yield return CoroutineEx.waitForSeconds(0.025f);
                        else yield return CoroutineEx.waitForEndOfFrame;
                    }

                    watch.Stop();

                    PrintWarning(sb.Clear().Append("UpdateAllClanTeams() finished in ").Append(watch.Elapsed.TotalMilliseconds).Append("ms").ToString());
                }
                finally { Facepunch.Pool.Free(ref sb); }
            }
            finally { Facepunch.Pool.Free(ref watch); }
        }


        private void UpdateClanTeam(Clan clan)
        {
            if (clan == null) throw new ArgumentNullException(nameof(clan));
            if ((clan?.members?.Count ?? 0) < 1)
            {
                PrintWarning("UpdateClanTeam called with no members!!! Clan tag: " + clan?.tag);
                return;
            }
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try 
            {
                watch.Restart();
              
                var ownerId = Convert.ToUInt64(clan.owner);
                var ownerPly = RelationshipManager.FindByID(ownerId) ?? BasePlayer.FindByID(ownerId) ?? BasePlayer.FindSleeping(ownerId);



                var lastIndex = RelationshipManager.ServerInstance.lastTeamIndex; //(ulong)lastTeamIndex.GetValue(RelationshipManager.ServerInstance);
                RelationshipManager.PlayerTeam newTeam = null;
                RelationshipManager.PlayerTeam foundTeam = null;
                for (int i = 0; i < clan.members.Count; i++)
                {
                    var c = clan.members[i];
                    var cID = Convert.ToUInt64(c);
                    var clanPly = BasePlayer.FindByID(cID) ?? BasePlayer.FindSleeping(cID);
                    if (clanPly == null)
                    {
                        foreach(var team in RelationshipManager.ServerInstance.teams)
                        {
                            var val = team.Value;
                            if (val == null) continue;
                            if ((val?.members?.Contains(cID) ?? false))
                            {
                                foundTeam = val;

                                //PrintWarning("found existing team from clan leader or a member, so we'll use it & just update (clanPly was null, but manual lookup succeeded)");
                                break;
                            }
                        }
                    }
                    else if (clanPly?.currentTeam != 0)
                    {
                        var findTeam = RelationshipManager.ServerInstance.FindTeam(clanPly.currentTeam);
                        if (findTeam != null)
                        {
                            foundTeam = findTeam;

                            //PrintWarning("found existing team from clan leader or a member, so we'll use it & just update");
                            break;
                        }
                    }
                }
                if (foundTeam != null)
                {
                    newTeam = foundTeam;
                    ClearTeamMembers(foundTeam, true);
                }


                var bumpIndex = lastIndex + 1;
                if (newTeam == null)
                {
                    var findTeamWithIndex = RelationshipManager.ServerInstance.FindTeam(lastIndex);
                    if (findTeamWithIndex == null) findTeamWithIndex = RelationshipManager.ServerInstance.FindTeam(bumpIndex);
                    if (findTeamWithIndex != null)
                    {
                        newTeam = findTeamWithIndex;
                        var oldCount = newTeam.members.Count;
                        ClearTeamMembers(newTeam);
                        var newCount = newTeam.members.Count;
                        PrintWarning("(" + ownerPly + ") FOUND existing team with lastIndex + 1, val: " + (lastIndex + 1) + ", before clear: " + oldCount + ", now: " + newCount);
                    }
                    else
                    {
                        var createTeam = RelationshipManager.ServerInstance.CreateTeam();
                        if (createTeam.members.Count > 0)
                        {
                            PrintWarning("createTeam for teamId: " + createTeam.teamID + " and owner: " + ownerId + " (" + ownerPly + ") had members upon creation, members: " + string.Join(" ", createTeam.members.Select(p => GetDisplayNameFromID(p.ToString()) + " (" + p + "), ")) + ", clearing!");
                            var loopMax = 5000;
                            var loopCount = 0;
                            while ((createTeam = RelationshipManager.ServerInstance.CreateTeam()).members.Count > 0)
                            {
                                loopCount++;
                                if (loopCount >= loopMax)
                                {
                                    PrintWarning("yo dude we just called CreateTeam 5000 times and every time we had members in it already lol");
                                    break;
                                }
                            }
                            if (createTeam.members.Count > 0)
                            {
                                PrintWarning("members count is STILL > 0 after " + loopCount.ToString("N0") + " CreateTeam calls, now with Id: " + createTeam.teamID);
                                ClearTeamMembers(createTeam);
                            }
                            else PrintWarning("members.count was not > 0 after " + loopCount.ToString("N0") + " CreateTeam calls! :^)");


                        }
                        // else PrintWarning("it is truly a miracle. we did NOT have any members already in this team when we first created it. Thanks be to God");
                        newTeam = createTeam;
                    }
                }

                if (newTeam == null)
                {
                    PrintError("somehow newTeam is still null after all that, so something has gone really bad");
                    return;
                }


                newTeam.teamLeader = ownerId;

                if (ownerPly != null)
                {
                   // PrintWarning("ownerPly was not null, so calling AddPlayer for: " + ownerPly + " for teamId: " + newTeam.teamID);
                    if (!newTeam.members.Contains(ownerId) && !newTeam.AddPlayer(ownerPly))
                    {
                        PrintWarning("COULD NOT ADD OWNER PLAYER, DID NOT EXIST IN TEAM MEMBERS BUT ALSO COULD NOT ADD. RETURNED FALSE ON BOOL!!! PLAYER: " + ownerPly + Environment.NewLine + " teamId: " + newTeam.teamID + ", member count: " + newTeam.members.Count + ", player.currentTeam: " + ownerPly.currentTeam + ", members.contains: " + newTeam.members.Contains(ownerPly.userID));
                    }
                }
                else
                {
                    AddPlayerToTeam(newTeam, ownerId);
                }
                if (clan.members.Count > 1)
                {
                   // PrintWarning("clan " + clan.tag + " has members count > 1, which means it isn't just the leader. going to add each team member, count: " + clan.members.Count + " for teamId: " + newTeam.teamID);
                    for (int i = 0; i < clan.members.Count; i++)
                    {
                        var memberStr = clan.members[i];
                        var member = Convert.ToUInt64(memberStr);
                        if (member == ownerId)
                        {
                            //  PrintWarning("member == ownerId for clan: " + clan.tag + " and user: " + GetDisplayNameFromID(ownerId.ToString()) + " (" + ownerId + "), continuing");
                            continue;
                        }
                        var ply = RelationshipManager.FindByID(member) ?? BasePlayer.FindByID(member) ?? BasePlayer.FindSleeping(member);
                        if (ply != null && ply.currentTeam != 0 && ply.currentTeam != newTeam.teamID)
                        {
                            var plyTeam = RelationshipManager.ServerInstance.FindTeam(ply.currentTeam);
                            if (plyTeam != null) RemovePlayerFromTeam(plyTeam, ply.userID); // plyTeam.RemovePlayer(ply.userID);
                            ply.ClearTeam();
                        }
                        // PrintWarning("ADD CLAN MEM: " + GetDisplayNameFromID(memberStr) + " (" + memberStr + "), CLAN " + clan.tag + " TEAM: " + newTeam.teamID);

                        RelationshipManager.PlayerTeam tempTeam;
                        if (RelationshipManager.ServerInstance.playerToTeam.TryGetValue(member, out tempTeam) && tempTeam.teamID != newTeam.teamID)
                        {
                            PrintWarning("playerToTeam on loop found member: " + member + " in another team. forcing to leave or disband");
                            if (tempTeam.members.Count < 2) DisbandTeam(tempTeam);
                            else RemovePlayerFromTeam(tempTeam, member); //tempTeam.RemovePlayer(member);
                        }
                        if (!newTeam.members.Contains(member))
                        {
                            //PrintWarning("newTeam.members did not contain member: " + member + ", adding new member");
                            if (ply != null) newTeam.AddPlayer(ply);
                            else AddPlayerToTeam(newTeam, member); //newTeam.members.Add(member);
                        }
                    }
                }
                newTeam.MarkDirty();
                clanToTeam[clan.tag] = newTeam;
                watch.Stop();
                if (watch.Elapsed.TotalMilliseconds > 10) PrintWarning("UpdateClanTeam took: " + watch.Elapsed.TotalMilliseconds + "ms");
            }
            finally { Facepunch.Pool.Free(ref watch); }
          
        }

        private void OnClanUpdate(string tag)
        {
            if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag));
            var clan = findClan(tag);
            if (clan?.members == null || clan.members.Count < 1)
            {
                PrintWarning("OnClanUpdate has null members or < 1: " + tag);
                return;
            }
            UpdateClanTeam(clan);
        }

        private void OnClanCreate(string tag)
        {
            if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag));
            var clan = findClan(tag);
            if (clan?.members == null || clan.members.Count < 1)
            {
                PrintWarning("OnClanCreate has null members or < 1: " + tag);
                return;
            }
            UpdateClanTeam(clan);
        }

        private void OnClanDestroy(string tag)
        {
            if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag));

            RelationshipManager.PlayerTeam clanTeam;
            if (clanToTeam.TryGetValue(tag, out clanTeam))
            {
                clanToTeam.Remove(tag);
                if (clanTeam != null) DisbandTeam(clanTeam);
            }
            else PrintWarning("no clanToTeam on OnClanDestroy for: " + tag);
        }

        private void OnTeamLeave(RelationshipManager.PlayerTeam team, BasePlayer player)
        {
            if (team == null || player == null) return;
            var clanName = string.Empty;
            foreach (var kvp in clanToTeam)
            {
                if (kvp.Value == team)
                {
                    clanName = kvp.Key;
                    break;
                }
            }
            if (string.IsNullOrEmpty(clanName)) return;
            var clan = findClan(clanName);
            if (clan == null) return;
            var userId = player?.userID ?? 0;
            var userIDStr = player?.UserIDString ?? string.Empty;
            ServerMgr.Instance.Invoke(() =>
            {
              if (team?.members == null || !team.members.Contains(userId))
                {
                    if (clan != null && clan.IsMember(userIDStr))
                    {
                        if (player != null && player.IsConnected) player.SendConsoleCommand("clan leave");
                        else
                        {
                            clan.members.Remove(userIDStr);
                            lookup.Remove(userIDStr);
                            clan.OnUpdate();
                        }
                    }
                }
            }, 0.25f);
        }

        private object OnTeamInvite(BasePlayer player, BasePlayer target)
        {
            if (player == null || target == null) return null;

            var clan = findClanByUser(player.UserIDString) ?? findClan(GetClanOf(player));

            if (clan == null)
            {
                PrintError("team invite attempted with a null clan!!!!! " + player + " " + target);
                return null;
            }

            RelationshipManager.PlayerTeam team;
            if (!clanToTeam.TryGetValue(clan.tag, out team))
            {
                PrintError("clanToTeam null for " + clan.tag + "!!!!");
                return null;
            }

            var isVanish = Vanish?.Call<bool>("IsInvisible", player) ?? false;
            if (isVanish) return null;

            if (clan.IsMember(target.UserIDString)) return null;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                if (team.members.Count >= Mathf.Min(limitMembers, RelationshipManager.maxTeamSize))
                {
                    SendReply(player, "Your team is already full!");
                    return true;
                }
                
                if (!team.invites.Contains(target.userID))
                {
                    //avoid message spam?

                    team.invites.Add(target.userID);
                    SendReply(target, sb.Clear().Append(player.displayName).Append(" invited you to join their team. To join the team, join the ").Append(clan.tag).Append(" clan. You have been invited.").Append(Environment.NewLine).Append("<color=orange>/clan join \"").Append(clan.tag).Append("\"</color>").ToString());
                    SendReply(player, sb.Clear().Append("Sent clan invite to: ").Append(target.displayName).ToString());
                    player.SendConsoleCommand(sb.Clear().Append("clan invite \"").Append(target.displayName).Append("\"").ToString());
                }

                return true;
            }
            finally { Facepunch.Pool.Free(ref sb); }

        }

        private object OnServerCommand(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection == null) return null;

            var command = arg?.cmd?.FullName ?? string.Empty;

            if (command.IndexOf("trycreateteam", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var player = arg?.Player() ?? null;
                if (player != null && player.IsConnected)
                {
                    SendLocalEffect(player, "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab");
                    SendReply(player, "--<size=20><color=#4aff92>TEAMS</color></size>--\n<color=#df5eff>To create a team, you must make a <color=#ecff3d>clan</color></color>, <color=#ff3030>there are no longer teams without clans. <color=#97ff85>Upon creating a clan, you'll automatically be put into a team!</color> <color=#ff5ec4>:)</color></color>\n<color=#ff3da8>Type <color=#ecff3d>/clan</color> for more info</color>.");
                }
                return true;
            }


            return null;
        }

        private void OnTeamKick(RelationshipManager.PlayerTeam team, BasePlayer kicker, ulong targetId)
        {
            if (team == null || kicker == null || targetId == 0) return;
            var clanName = string.Empty;
            foreach (var kvp in clanToTeam)
            {
                if (kvp.Value == team)
                {
                    clanName = kvp.Key;
                    break;
                }
            }
            if (string.IsNullOrEmpty(clanName)) return;
            var clan = findClan(clanName);
            if (clan == null) return;
            var player = BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId);
            ServerMgr.Instance.Invoke(() =>
            {
                if (team?.members == null || !team.members.Contains(targetId))
                {
                    var userIDStr = targetId.ToString();
                    if (clan != null && clan.IsMember(userIDStr))
                    {
                        if (player != null && player.IsConnected) player.SendConsoleCommand("clan leave");
                        else
                        {
                            clan.members.Remove(userIDStr);
                            lookup.Remove(userIDStr);
                            clan.OnUpdate();
                        }
                    }
                }
            }, 0.25f);
        }

        private void OnTeamPromote(RelationshipManager.PlayerTeam team, BasePlayer target)
        {
            if (team == null || target == null) return;

            var clanName = string.Empty;
            foreach (var kvp in clanToTeam)
            {
                if (kvp.Value == team)
                {
                    clanName = kvp.Key;
                    break;
                }
            }

            if (string.IsNullOrEmpty(clanName)) return;
            
            var clan = findClan(clanName);
            if (clan == null) return;

            var oldOwner = clan.owner;

            if (target.UserIDString == oldOwner)
            {
                PrintWarning("Somehow target userid is same as oldOwner on OnTeamPromote!");
                return;
            }

            ServerMgr.Instance.Invoke(() =>
            {
                if (team == null || clan?.members == null || clan.members.Count < 1) return;
                clan.owner = target.UserIDString;
                if (!clan.moderators.Contains(oldOwner)) clan.moderators.Add(oldOwner);
                clan.Broadcast(GetDisplayNameFromID(oldOwner) + " promoted " + target?.displayName + " to clan & team leader.");
            }, 0.1f);
         
        }

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var p = BasePlayer.sleepingPlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty; //BasePlayer.activePlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? BasePlayer.sleepingPlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? string.Empty;
        }
        private Coroutine UpdateAllRoutine = null;
        [HookMethod("OnServerInitialized")]
        private void OnServerInitialized()
        {
            try
            {
                ioInitialize();
                LoadConfig();
                try
                {
                    var customMessages = Config.Get<Dictionary<string, object>>("messages");
                    if (customMessages != null)
                        foreach (var pair in customMessages)
                            messages[pair.Key] = (string)pair.Value;
                    
                }
                catch (Exception ex2)
                {
                    PrintError(ex2.ToString());
                    warn("oxide/config/Clans.json seems to contain an invalid 'messages' structure. Please delete the config file once and reload the plugin.");
                }
                try
                {
                    loadData();
                    UpdateAllRoutine = ServerMgr.Instance.StartCoroutine(UpdateAllClanTeams());
                    //UpdateAllClanTeams();
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
               


                //   for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) setupPlayer(BasePlayer.activePlayerList[i]);
                //     for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++) setupPlayer(BasePlayer.sleepingPlayerList[i]);


                try
                {
                    addClanMatesAsFriends = Config.Get<bool>("addClanMatesAsFriends");
                    limitMembers = Config.Get<int>("limit", "members");
                    limitModerators = Config.Get<int>("limit", "moderators");
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
            }
            catch (Exception ex)
            {
                error("OnServerInitialized failed", ex);
            }
        }

        [HookMethod("OnUserApprove")]
        private void OnUserApprove(Network.Connection connection)
        {
            // Override whatever there is
            originalNames[connection.userid.ToString()] = connection.username;
        }

        [HookMethod("OnPlayerConnected")]
        private void OnPlayerConnected(BasePlayer player)
        {
            //    string originalName;
            //     if (player.displayName == "Shady" && player.UserIDString == "76561198028248023") return;
            //   player.displayName = stripTag(player.displayName);
            //       if (originalNames.TryGetValue(player.UserIDString, out originalName)) player.displayName = originalName;

            RelationshipManager.PlayerTeam realTeam = null;
            foreach(var kvp in RelationshipManager.ServerInstance.teams)
            {
                if (kvp.Value.members.Contains(player.userID))
                {
                    realTeam = kvp.Value;
                    break;
                }
            }
            if (realTeam == null && player.currentTeam != 0)
            {
                PrintWarning("realTeam == null but player.currentTeam was: " + player.currentTeam + ", fixing!");
                player.ClearTeam();
            }
            if (realTeam != null && player.currentTeam != realTeam.teamID)
            {
                PrintWarning("realTeam was not null but player.currentTeam (" + player.currentTeam + ") != realTeam ID: " + realTeam.teamID + ", fixing!");
                player.ClearTeam();
                realTeam.AddPlayer(player);
            }
            try
            {
          //      setupPlayer(player);
                var clan = findClanByUser(player.UserIDString);
                if (clan != null)
                    clan.Broadcast(_("%NAME% has come online!", new Dictionary<string, string>() { { "NAME", stripTag(player.displayName, clan) } }));
            }
            catch (Exception ex)
            {
                error("OnPlayerConnected failed", ex);
            }
        }

        [HookMethod("OnPlayerDisconnected")]
        private void OnPlayerDisconnected(BasePlayer player)
        {
            try
            {
                var clan = findClanByUser(player.UserIDString);
                if (clan != null)
                    clan.Broadcast(_("%NAME% has gone offline.", new Dictionary<string, string>() { { "NAME", stripTag(player.displayName, clan) } }));
            }
            catch (Exception ex)
            {
                error("OnPlayerDisconnected failed", ex);
            }
        }

        private void DisbandTeam(RelationshipManager.PlayerTeam team)
        {
            if (team == null) return;
            if (team?.members?.Count > 0)
            {
                var memList = team?.members?.ToList() ?? null;
                for(int i = 0; i < memList.Count; i++)
                {
                    var member = memList[i];
                    RelationshipManager.ServerInstance.playerToTeam.Remove(member);
                    var ply = RelationshipManager.FindByID(member) ?? BasePlayer.FindByID(member) ?? BasePlayer.FindSleeping(member);
                    if (ply != null) ply.ClearTeam();
                }
                team.members.Clear();
            }
            var teamId = team?.teamID ?? 0;
            RelationshipManager.ServerInstance.teams.Remove(team.teamID);
            team.Disband();
            RelationshipManager.ServerInstance.teams = RelationshipManager.ServerInstance.teams.Where(p => p.Key != teamId && p.Value.teamID != teamId).ToDictionary(p => p.Key, p => p.Value);
            RelationshipManager.ServerInstance.playerToTeam = RelationshipManager.ServerInstance.playerToTeam.Where(p => p.Value.teamID != teamId).ToDictionary(p => p.Key, p => p.Value);
        }

        private void ClearTeamMembers(RelationshipManager.PlayerTeam team, bool keepLeader = false)
        {
            if (team == null) return;
            if (team?.members?.Count > 0)
            {
                var memList = new List<ulong>(team.members);

                for (int i = 0; i < memList.Count; i++)
                {
                    var member = memList[i];
                    if (keepLeader && member == team.teamLeader) continue;
                    if (keepLeader) team.members.Remove(member);

                    RelationshipManager.ServerInstance.playerToTeam.Remove(member);

                    var ply = RelationshipManager.FindByID(member) ?? BasePlayer.FindByID(member) ?? BasePlayer.FindSleeping(member);
                    if (ply != null) ply.ClearTeam();
                }
                if (!keepLeader) team.members.Clear();
            }
        }

        private int RemoveAllTeams()
        {
            var c = 0;

            var teamValues = RelationshipManager.ServerInstance?.teams?.Values?.ToList() ?? null;
            if (teamValues?.Count > 0)
            {
                for(c = 0; c < teamValues.Count; c++) DisbandTeam(teamValues[c]);
            }

            RelationshipManager.ServerInstance.teams = new Dictionary<ulong, RelationshipManager.PlayerTeam>();
            RelationshipManager.ServerInstance.playerToTeam = new Dictionary<ulong, RelationshipManager.PlayerTeam>();

           // lastTeamIndex.SetValue(RelationshipManager.ServerInstance, (ulong)(int.MaxValue - 4000000));
            return c;
        }

        private void AddPlayerToTeam(RelationshipManager.PlayerTeam team, ulong userId)
        {
            if (team == null)
            {
                PrintWarning("team is null on AddPlayerToTeam!");
                return;
            }
            if (userId <= 0)
            {
                PrintWarning("userId <= 0 on AddPlayerToTeam!");
                return;
            }
            if (team.members.Count >= RelationshipManager.maxTeamSize)
            {
              //  PrintWarning("!!! trying to add over limit for team: " + team.teamID + ", teamLeader: " + GetDisplayNameFromID(team.teamLeader.ToString()) + " (" + team.teamLeader + ")" + "team.members.count >= maxTeamSize: " + team.members.Count + " >= " + RelationshipManager.maxTeamSize + ", was trying to add: " + GetDisplayNameFromID(userId.ToString()) + " (" + userId + "), team members: " + string.Join(" ", team.members.Select(p => GetDisplayNameFromID(p.ToString()) + ", ")));
                return;
            }
            RelationshipManager.PlayerTeam tempTeam;
            if (RelationshipManager.ServerInstance.playerToTeam.TryGetValue(userId, out tempTeam)) //&& tempTeam.teamID != team.teamID)
            {
                RelationshipManager.ServerInstance.playerToTeam.Remove(userId);
                if (tempTeam.teamID != team.teamID)
                {
                  //  PrintWarning("AddPlayerToTeam found player with playerToTeam that was not the correct team, tempTeam.teamId != team.TeamID: " + tempTeam.teamID + " != " + team.teamID + " -- now removing from temp (or disbanding if last member) before adding to new team");
                    if (tempTeam.members.Count > 1) RemovePlayerFromTeam(tempTeam, userId);
                    else DisbandTeam(tempTeam);
                }
            }

            if (!team.members.Contains(userId)) team.members.Add(userId);
        //    else PrintWarning("AddPlayerToTeam called when team.members already contained userId: " + userId); //this usually happens if the server crashed (teams should be destroyed on server shutdown and rebuilt, but if the server crashes — they persist)
         
            RelationshipManager.ServerInstance.playerToTeam.Add(userId, team); // = team;
            team.MarkDirty();
        }

        private void RemovePlayerFromTeam(RelationshipManager.PlayerTeam team, ulong userId)
        {
            if (team == null)
            {
                PrintWarning("team is null on RemovePlayerFromTeam!");
                return;
            }
            if (team?.members == null || team.members.Count < 1)
            {
                PrintWarning("team.members is null or count < 1 on RemovePlayerFromTeam!!!");
                return;
            }
            team.members.Remove(userId);
            if (team.teamLeader == userId)
            {
                if (team.members.Count > 0)
                {
                    team.SetTeamLeader(team.members[0]);
                }
                else DisbandTeam(team);
            }
            RelationshipManager.ServerInstance.playerToTeam.Remove(userId);
            var p = RelationshipManager.FindByID(userId) ?? BasePlayer.FindByID(userId) ?? BasePlayer.FindSleeping(userId) ?? null;
            if (p != null) p.ClearTeam();
        }

       

        [HookMethod("SendHelpText")]
        private void SendHelpText(BasePlayer player)
        {
            var sb = new StringBuilder()
               .Append("<size=18>Clans</size> by <color=#ce422b>http://playrust.io</color>\n")
               .Append("  ").Append(_("<color=#ffd479>/clan</color> - Displays your current clan status")).Append("\n")
               .Append("  ").Append(_("<color=#ffd479>/clan help</color> - Learn how to create or join a clan"));
            player.ChatMessage(sb.ToString());
        }

        /*/
        [HookMethod("BuildServerTags")]
        private void BuildServerTags(IList<string> taglist)
        {
            taglist.Add("clans");
        }/*/

        [ConsoleCommand("teams.reset")]
        private void consoleTeamReset(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection != null) return;
            PrintWarning("Resetting all teams!");
            PrintWarning("Removed " + RemoveAllTeams().ToString("N0") + " teams!");
        }

        [ConsoleCommand("teams.generate")]
        private void consoleTeamGenerate(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && !arg.IsAdmin) return;
            if (ServerMgr.Instance == null) return;
            if (UpdateAllRoutine != null) ServerMgr.Instance.StopCoroutine(UpdateAllRoutine);
            UpdateAllRoutine = ServerMgr.Instance.StartCoroutine(UpdateAllClanTeams());
            SendReply(arg, "Started UpdateAllRoutine");
        }

        [ConsoleCommand("team.info")]
        private void consoleTeamInfo(ConsoleSystem.Arg arg)
        {
            if (arg.Connection == null || !arg.IsAdmin) return;
            var player = arg?.Player() ?? null;
            if (player == null) return;
            PrintWarning("player.currentTeam: " + player.currentTeam);
            var team = RelationshipManager.ServerInstance.FindTeam(player.currentTeam);
            if (team == null)
            {
                PrintWarning("team is null for player.currentTeam: " + player.currentTeam + "!!!");
                return;
            }
            var memberStr = string.Join(" ", team.members.Select(p => GetDisplayNameFromID(p.ToString()) + " (" + p + "), "));
            var str = "teamId: " + team.teamID + " team leader: " + team.teamLeader + " (" + GetDisplayNameFromID(team.teamLeader.ToString()) + "), memberStr: " + memberStr;

            SendReply(player, "Team info: " + str);
        }

        [ConsoleCommand("clans.purge")]
        private void consoleClanPurge(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && !arg.IsAdmin) return;
            var watch = Stopwatch.StartNew();
            var clanList = clans?.Select(p => p.Value)?.ToList() ?? null;
            if (clanList == null || clanList.Count < 1)
            {
                SendReply(arg, "No clans found!");
                return;
            }
            var removed = 0;
            var now = DateTime.UtcNow;
            var removeSB = new StringBuilder();
            for(int i = 0; i < clanList.Count; i++)
            {
                var clan = clanList[i];
                if (clan == null) continue;
                var ownerBanned = BanSystem?.Call<bool>("IsBanned", clan.owner) ?? false;
                var daysSinceOwnerPlayed = (now - (Compilation.Call<DateTime>("GetLastConnectionS", clan.owner))).TotalDays;
                if (daysSinceOwnerPlayed < 10) continue;
                var avgTimeDaysSincePlayed = ownerBanned ? -1 : clan?.members?.Average(p => (now - Compilation.Call<DateTime>("GetLastConnectionS", p)).TotalDays);
                if (ownerBanned || avgTimeDaysSincePlayed >= 30)
                {
                    removeSB.AppendLine(clan.tag + " (" + (covalence.Players?.FindPlayerById(clan.owner)?.Name ?? clan.owner) + ")");
                    DisbandClan(clan);
                    removed++;
                }
            }
            watch.Stop();
            SendReply(arg, "Purged: " + removed.ToString("N0") + " clans in: " + watch.Elapsed.TotalMilliseconds.ToString("N0") + "ms" + Environment.NewLine + removeSB.ToString().TrimEnd());
            
        }

        [ConsoleCommand("clans.resetall")]
        private void consoleClansResetAll(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && !arg.IsAdmin) return;
            var watch = Stopwatch.StartNew();
            var clanList = clans?.Select(p => p.Value)?.ToList() ?? null;
            if (clanList == null || clanList.Count < 1)
            {
                SendReply(arg, "No clans found!");
                return;
            }
            var removeSB = new StringBuilder();
            for (int i = 0; i < clanList.Count; i++)
            {
                var clan = clanList[i];
                if (clan == null) continue;

                removeSB.AppendLine(clan.tag + " (" + (covalence.Players?.FindPlayerById(clan.owner)?.Name ?? clan.owner) + ")");
                DisbandClan(clan);
            }
            watch.Stop();
            SendReply(arg, "Removed: " + clanList.Count.ToString("N0") + " clans in: " + watch.Elapsed.TotalMilliseconds.ToString("N0") + "ms" + Environment.NewLine + removeSB.ToString().TrimEnd());

        }

        private void DisbandClan(Clan clan)
        {
            if (clan == null) return;
            clans.Remove(clan.tag);
            for (int i = 0; i < clan.members.Count; i++) lookup.Remove(clan.members[i]);
            clan.Broadcast(_("Your current clan has been disbanded forever."));
            clan.OnDestroy();
        }

        //[ChatCommand("clan")]
        private void cmdChatClan(IPlayer player, string command, string[] args)
        {
            if (player == null) return;
            var userId = player.Id;

            var myClan = findClanByUser(userId);

            var sb = new StringBuilder();

            IPlayer argPlayer = null;

            if (args.Length > 1)
            {
                if (myClan != null) argPlayer = FindClanPlayer(args[1], myClan);
                
                if (argPlayer == null) argPlayer = FindConnectedPlayer(args[1], true);
            }

            // No arguments: List clans and get help how to create one
            if (args.Length == 0)
            {
                //sb.Append("<size=22>Clans</size> " + Version + " by <color=#ce422b>http://playrust.io</color>\n");
                if (myClan == null)
                {
                    sb.Append(_("You are currently not a member of a clan.")).Append("\n");
                }
                else {
                    
                    if (myClan.IsOwner(userId))
                    {
                        sb.Append(_("You are the owner of:"));
                    }
                    else if (myClan.IsModerator(userId))
                        sb.Append(_("You are a moderator of:"));
                    else
                        sb.Append(_("You are a member of:"));
                    sb.Append(" [").Append(myClan.tag).Append("] ").Append(myClan.description).Append("\n");
                    sb.Append(_("Members online:")).Append(" ");
                    List<string> onlineMembers = new List<string>();
                    int n = 0;
                    for(int i = 0; i < myClan.members.Count; i++)
                    {
                        var memberId = myClan.members[i];
                        var member = Convert.ToUInt64(memberId);
                        var p = RelationshipManager.FindByID(member) ?? BasePlayer.FindByID(member) ?? BasePlayer.FindSleeping(member);
                        if (p != null)
                        {
                            if (n > 0) sb.Append(", ");
                            if (myClan.IsOwner(memberId))
                            {
                                sb.Append("<color=#a1ff46>").Append(stripTag(p.displayName, myClan)).Append("</color>");
                            }
                            else if (myClan.IsModerator(memberId))
                            {
                                sb.Append("<color=#74c6ff>").Append(stripTag(p.displayName, myClan)).Append("</color>");
                            }
                            else
                            {
                                sb.Append(p.displayName);
                            }
                            ++n;
                        }
                    }
                    sb.Append("\n");
                    if ((myClan.IsOwner(userId) || myClan.IsModerator(userId)) && myClan.invited.Count > 0)
                    {
                        sb.Append(_("Pending invites:")).Append(" ");
                        int m = 0;
                        for(int i = 0; i < myClan.invited.Count; i++)
                        {
                            var inviteId = myClan.invited[i];
                            var p = BasePlayer.FindByID(Convert.ToUInt64(inviteId));
                            if (p != null)
                            {
                                if (m > 0) sb.Append(", ");
                                sb.Append(p.displayName);
                                ++m;
                            }
                        }
                        sb.Append("\n");
                    }
                }
                sb.Append(_("To learn more about clans, type: <color=#ffd479>/clan help</color>"));
                player.Message(sb.ToString().TrimEnd());
                //SendReply(player.Object as BasePlayer, "{0}", sb.ToString());
                return;
            }
            switch (args[0].ToLower())
            {
                case "create":
                    if (args.Length < 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan create \"TAG\" \"Description\"</color>"));
                        break;
                    }
                    if (myClan != null)
                    {
                        sb.Append(_("You are already a member of a clan."));
                        break;
                    }
                    if (!tagRe.IsMatch(args[1]))
                    {
                        sb.Append(_("Clan tags must be 2 to 6 characters long and may contain standard letters and numbers only"));
                        break;
                    }
                    var clanDesc = args.Length > 2 ? string.Join(" ", args, 2, args.Length - 2) : string.Empty;
                   // var clanDesc = string.Join(" ", args.Where(p => p != args[0] && p != args[1]).ToArray()).Trim();
                    //args[2] = args[2].Trim();
                    /*/
                    if (clanDesc.Length < 2 || clanDesc.Length > 30)
                    {
                        sb.Append(_("Please provide a short description of your clan."));
                        break;
                    }/*/
                    Clan tempClan;
                    if (clans.TryGetValue(args[1], out tempClan) || clans.Any(p => p.Key.Equals(args[1], StringComparison.OrdinalIgnoreCase)))
                    {
                        sb.Append(_("There is already a clan with this tag."));
                        break;
                    }

                    if (Interface.CallHook("OnClanCreate", player, args[1], clanDesc) != null)
                    {
                        //sb.Append(_("Clan creation was cancelled by a plugin."));
                        break;
                    }

                    myClan = Clan.Create(args[1], clanDesc, userId);
                    clans.Add(myClan.tag, myClan);
                    saveData();
                    lookup[userId] = myClan;
               //     setupPlayer(player); // Add clan tag
                    sb.Append(_("You are now the owner of your new clan:")).Append(" ");
                    sb.Append("[").Append(myClan.tag).Append("] ").Append(myClan.description).Append("\n");
                    sb.Append(_("To invite new members, type: <color=#ffd479>/clan invite \"Player name\"</color>"));
                    myClan.OnCreate();
                    break;
                case "invite":
                    if (args.Length != 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan invite \"Player name\"</color>"));
                        break;
                    }
                    if (myClan == null)
                    {
                        sb.Append(_("You are currently not a member of a clan."));
                        break;
                    }
                  
                    /*/
                    if (!myClan.IsOwner(userId) && !myClan.IsModerator(userId) && !player.IsAdmin)
                    {
                        sb.Append(_("You need to be a moderator of your clan to use this command."));
                        break;
                    }/*/

                    var invPlayer = argPlayer;
                    if (invPlayer == null)
                    {
                        sb.Append(_("No such player or player name not unique:")).Append(" ").Append(args[1]);
                        break;
                    }

                    var inDM = (Deathmatch?.Call<bool>("InAnyDM", player.Id) ?? false) || (Deathmatch?.Call<bool>("InAnyDM", invPlayer.Id) ?? false);
                    if (inDM) break;

                    var invUserId = invPlayer?.Id ?? string.Empty;
                    
                    if (Interface.Oxide.CallHook("OnClanInvite", player, invPlayer, myClan.tag) != null) break; //call this hook for Luck (Nomads)

                    if (myClan.members.Contains(invUserId))
                    {
                        sb.Append(_("This player is already a member of your clan:")).Append(" ").Append(invPlayer.Name);
                        break;
                    }
                    if (myClan.invited.Contains(invUserId))
                    {
                        sb.Append(_("This player has already been invited to your clan:")).Append(" ").Append(invPlayer.Name);
                        break;
                    }
                    myClan.invited.Add(invUserId);
                    saveData();
                    myClan.Broadcast(_("%MEMBER% invited %PLAYER% to the clan.", new Dictionary<string, string>() { { "MEMBER", stripTag(player.Name, myClan) }, { "PLAYER", invPlayer.Name } }));
                    if (invPlayer.IsConnected) invPlayer.Reply(_("You have been invited to join the clan:") + " [" + myClan.tag + "] " + myClan.description + "\n" +
                        _("To join, type: <color=#ffd479>/clan join \"%TAG%\"</color>", new Dictionary<string, string>() { { "TAG", myClan.tag } }));
                    myClan.OnUpdate();
                    break;
                case "force":
                    if (args.Length != 3)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan force \"TAG\" UserName</color>"));
                        break;
                    }
                    if (myClan != null)
                    {
                        sb.Append(_("You are already a member of a clan."));
                        break;
                    }
                    myClan = findClan(args[1]);
                    if (myClan == null)
                    {
                        sb.Append(_("No clan found by this name!"));
                        break;
                    }
                    var forcePlayer = FindConnectedPlayer(args[2], true);
                    userId = forcePlayer?.Id ?? string.Empty;
                    if (string.IsNullOrEmpty(userId))
                    {
                        sb.Append(_("No such player or player name not unique:") + " " + args[2]);
                        break;
                    }
                    if (!myClan.IsInvited(userId) && !player.IsAdmin)
                    {
                        sb.Append(_("You have not been invited to join this clan."));
                        break;
                    }
                    if (limitMembers >= 0 && myClan.members.Count >= limitMembers && !player.IsAdmin)
                    {
                        sb.Append(_("This clan has already reached the maximum number of members."));
                        break;
                    }
                    myClan.invited.Remove(userId);
                    myClan.members.Add(userId);
                    saveData();
                    lookup[userId] = myClan;
                    //     setupPlayer(player);
                    myClan.Broadcast(_("%NAME% has joined the clan!", new Dictionary<string, string>() { { "NAME", stripTag(forcePlayer.Name, myClan) } }));
                    for (int i = 0; i < myClan.members.Count; i++)
                    {
                        var memberId = myClan.members[i];
                        if (memberId != userId && ioIsInstalled() && addClanMatesAsFriends)
                        {
                            ioAddFriend(memberId, userId);
                            ioAddFriend(userId, memberId);
                        }
                    }
                    if (player.IsAdmin) myClan.moderators.Add(userId);
                    myClan.OnUpdate();
                    break;
                case "join":
                    if (args.Length != 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan join \"TAG\"</color>"));
                        break;
                    }
                    if (myClan != null)
                    {
                        sb.Append(_("You are already a member of a clan."));
                        break;
                    }
                    myClan = findClan(args[1]);
                    if (myClan == null)
                    {
                        sb.Append(_("No clan found by this name!"));
                        break;
                    }
                    if (!myClan.IsInvited(userId) && !player.IsAdmin)
                    {
                        sb.Append(_("You have not been invited to join this clan."));
                        break;
                    }
                    if (limitMembers >= 0 && myClan.members.Count >= limitMembers && !player.IsAdmin)
                    {
                        sb.Append(_("This clan has already reached the maximum number of members."));
                        break;
                    }
                    if ((Deathmatch?.Call<bool>("InAnyDM", player.Id) ?? false)) break;
                    myClan.invited.Remove(userId);
                    myClan.members.Add(userId);
                    saveData();
                    lookup[userId] = myClan;
               //     setupPlayer(player);
                    myClan.Broadcast(_("%NAME% has joined the clan!", new Dictionary<string, string>() { { "NAME", stripTag(player.Name, myClan) } }));

                    if (addClanMatesAsFriends && ioIsInstalled())
                    {
                        for (int i = 0; i < myClan.members.Count; i++)
                        {
                            var memberId = myClan.members[i];
                            if (memberId != userId)
                            {
                                ioAddFriend(memberId, userId);
                                ioAddFriend(userId, memberId);
                            }
                        }
                    }
                 
                    if (player.IsAdmin) myClan.moderators.Add(userId);
                    myClan.OnUpdate();
                    break;
                case "promote":
                    if (args.Length != 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan promote \"Player name\"</color>"));
                        break;
                    }
                    if (myClan == null)
                    {
                        sb.Append(_("You are currently not a member of a clan."));
                        break;
                    }
                    if (!player.IsAdmin && !myClan.IsOwner(userId))
                    {
                        sb.Append(_("You need to be the owner of your clan to use this command."));
                        break;
                    }
                    var promotePlayer = argPlayer;
                    if (promotePlayer == null)
                    {
                        sb.Append(_("No such player or player name not unique:") + " " + args[1]);
                        break;
                    }
                    var promotePlayerUserId = promotePlayer?.Id ?? string.Empty;
                    if (!myClan.IsMember(promotePlayerUserId))
                    {
                        sb.Append(_("This player is not a member of your clan:") + " " + promotePlayer.Name);
                        break;
                    }
                    if (myClan.IsModerator(promotePlayerUserId))
                    {
                        sb.Append(_("This player is already a moderator of your clan:") + " " + promotePlayer.Name);
                        break;
                    }
                    if (limitModerators >= 0 && myClan.moderators.Count >= limitModerators)
                    {
                        sb.Append(_("This clan has already reached the maximum number of moderators."));
                        break;
                    }
                    myClan.moderators.Add(promotePlayerUserId);
                    saveData();
                    myClan.Broadcast(_("%OWNER% promoted %MEMBER% to moderator.", new Dictionary<string, string>() { { "OWNER", stripTag(player.Name, myClan) }, { "MEMBER", stripTag(promotePlayer.Name, myClan) } }));
                    myClan.OnUpdate();
                    break;
                case "demote":
                    if (args.Length != 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan demote \"Player name\"</color>"));
                        break;
                    }
                    if (myClan == null)
                    {
                        sb.Append(_("You are currently not a member of a clan."));
                        break;
                    }
                    if (!player.IsAdmin && !myClan.IsOwner(userId))
                    {
                        sb.Append(_("You need to be the owner of your clan to use this command."));
                        break;
                    }
                    var demotePlayer = argPlayer;
                    if (demotePlayer == null)
                    {
                        sb.Append(_("No such player or player name not unique:") + " " + args[1]);
                        break;
                    }
                    var demotePlayerUserId = demotePlayer?.Id ?? string.Empty;
                    if (!myClan.IsMember(demotePlayerUserId))
                    {
                        sb.Append(_("This player is not a member of your clan:") + " " + demotePlayer.Name);
                        break;
                    }
                    if (!myClan.IsModerator(demotePlayerUserId))
                    {
                        sb.Append(_("This player is not a moderator of your clan:") + " " + demotePlayer.Name);
                        break;
                    }
                    myClan.moderators.Remove(demotePlayerUserId);
                    saveData();
                    myClan.Broadcast(player.Name + " demoted " + demotePlayer.Name + " to a member");
                    myClan.OnUpdate();
                    break;
                case "leave":
                    if (args.Length != 1)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan leave</color>"));
                        break;
                    }
                    if (myClan == null)
                    {
                        sb.Append(_("You are currently not a member of a clan."));
                        break;
                    }
                    var removed = false;
                    if (myClan.members.Count == 1)
                    { // Remove the clan once the last member leaves
                        clans.Remove(myClan.tag);
                        removed = true;
                    }
                    else {
                        myClan.moderators.Remove(userId);
                        myClan.members.Remove(userId);
                        myClan.invited.Remove(userId);
                        if (myClan.IsOwner(userId) && myClan.members.Count > 0)
                        { // Make the first member the new owner
                            myClan.owner = myClan.members[0];
                        }
                    }
                    saveData();
                    lookup.Remove(userId);
                  //  setupPlayer(player); // Remove clan tag
                    sb.Append(_("You have left your current clan."));
                    myClan.Broadcast(_("%NAME% has left the clan.", new Dictionary<string, string>() { { "NAME", player.Name } }));
                    if (!removed) myClan.OnUpdate();
                    else myClan.OnDestroy();
                    break;
                case "kick":
                    if (args.Length != 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan kick \"Player name\"</color>"));
                        break;
                    }
                    if (myClan == null)
                    {
                        sb.Append(_("You are currently not a member of a clan."));
                        break;
                    }
                    if (!myClan.IsOwner(userId) && !myClan.IsModerator(userId) && !player.IsAdmin)
                    {
                        sb.Append(_("You need to be a moderator of your clan to use this command."));
                        break;
                    }
                    var kickPlayer = argPlayer;
                    if (kickPlayer == null)
                    {
                        sb.Append(_("No such player or player name not unique:") + " " + args[1]);
                        break;
                    }
                    var kickPlayerUserId = kickPlayer?.Id ?? string.Empty;
                    if (!myClan.IsMember(kickPlayerUserId) && !myClan.IsInvited(kickPlayerUserId))
                    {
                        sb.Append(_("This player is not a member of your clan:") + " " + kickPlayer.Name);
                        break;
                    }
                    if (!player.IsAdmin && (myClan.IsOwner(kickPlayerUserId) || myClan.IsModerator(kickPlayerUserId)))
                    {
                        sb.Append(_("This player is an owner or moderator and cannot be kicked:") + " " + kickPlayer.Name);
                        break;
                    }
                    var wasMember = myClan.members.Contains(kickPlayerUserId);
                    myClan.members.Remove(kickPlayerUserId);
                    myClan.invited.Remove(kickPlayerUserId);
                    saveData();
                    lookup.Remove(kickPlayerUserId);
                    //   setupPlayer(kickPlayer); // Remove clan tag
                    if (wasMember)
                    {
                        if ((kickPlayer?.IsConnected ?? false)) kickPlayer.Reply(_("You were kicked from [" + myClan.tag + "]!"));
                        myClan.Broadcast(_("%NAME% kicked %MEMBER% from the clan.", new Dictionary<string, string>() { { "NAME", stripTag(player.Name, myClan) }, { "MEMBER", kickPlayer.Name } }));
                    }
                  
                    myClan.OnUpdate();
                    break;
                case "disband":
                    if (args.Length != 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan disband forever</color>"));
                        break;
                    }
                    if (myClan == null)
                    {
                        sb.Append(_("You are currently not a member of a clan."));
                        break;
                    }
                    if (!myClan.IsOwner(userId) && !player.IsAdmin)
                    {
                        sb.Append(_("You need to be the owner of your clan to use this command."));
                        break;
                    }
                    clans.Remove(myClan.tag);
                    saveData();
                    for (int i = 0; i < myClan.members.Count; i++) lookup.Remove(myClan.members[i]);
                    myClan.Broadcast(_("Your current clan has been disbanded forever."));
                 //   setupPlayers(myClan.members); // Remove clan tags
                    myClan.OnDestroy();
                    break;
                case "delete":
                    if (args.Length != 2)
                    {
                        sb.Append(_("Usage: <color=#ffd479>/clan delete \"TAG\"</color>"));
                        break;
                    }
                    if (!player.IsAdmin)
                    {
                        sb.Append(_("You need to be a server owner to delete clans."));
                        break;
                    }
                    Clan clan;
                    if (!clans.TryGetValue(args[1], out clan))
                    {
                        sb.Append(_("There is no clan with that tag:")).Append(" ").Append(args[1]);
                        break;
                    }
                    clan.Broadcast(_("Your clan has been deleted by the server owner."));
                    clans.Remove(args[1]);
                    saveData();
                    for (int i = 0; i < clan.members.Count; i++) lookup.Remove(clan.members[i]);
                 //   setupPlayers(clan.members);
                    sb.Append(_("You have deleted the clan:")).Append(" [").Append(clan.tag).Append("] ").Append(clan.description);
                    myClan.OnDestroy();
                    break;
                default:
                    sb.Append(_("Available commands:")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan</color> - Displays relevant information about your current clan")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/c Message...</color> - Sends a message to all online clan members")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan create \"TAG\" \"Description\"</color> - Creates a new clan you own")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan join \"TAG\"</color> - Joins a clan you have been invited to")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan leave</color> - Leaves your current clan")).Append("\n");
                    sb.Append(_("<color=#74c6ff>Moderator</color> commands:")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan invite \"Player name\"</color> - Invites a player to your clan")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan kick \"Player name\"</color> - Kicks a member from your clan")).Append("\n");
                    sb.Append(_("<color=#a1ff46>Owner</color> commands:")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan promote \"Name\"</color> - Promotes a member to moderator")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan demote \"Name\"</color> - Demotes a moderator to member")).Append("\n");
                    sb.Append("  ").Append(_("<color=#ffd479>/clan disband forever</color> - Disbands your clan (no undo)")).Append("\n");
                    if (player.IsAdmin)
                    {
                        sb.Append(_("<color=#cd422b>Server owner</color> commands:")).Append("\n");
                        sb.Append("  ").Append(_("<color=#ffd479>/clan delete \"TAG\"</color> - Deletes a clan (no undo)")).Append("\n");
                    }
                    break;
            }
            player.Message(sb.ToString().TrimEnd());
            //SendReply(player, "{0}", sb.ToString().TrimEnd());
        }

        [ChatCommand("c")]
        private void cmdChatClanchat(BasePlayer player, string command, string[] args)
        {
            SendReply(player, "<color=orange>/" + command + "</color> has been retired. With the chat open, please press TAB (default key) to switch between Global (public) and Team (clan) chat.");
            /*/
            var playerId = player.UserIDString;
            var myClan = findClanByUser(playerId);
            if (myClan == null)
            {
                SendReply(player, "{0}", _("You are currently not a member of a clan."));
                return;
            }
            var message = string.Join(" ", args);
            if (string.IsNullOrEmpty(message)) return;
            myClan.Broadcast(stripTag(player.displayName, myClan) + ": " + message);
            Puts("[CLANCHAT] {0} - {1}: {2}", myClan.tag, player.displayName, message);/*/
        }

        // Represents a clan
       

        #region Plugin API

        [HookMethod("GetClan")]
        private JObject GetClan(string tag)
        {
            var clan = findClan(tag);
            if (clan == null)
                return null;
            return clan.ToJObject();
        }

        [HookMethod("GetAllClans")]
        private JArray GetAllClans()
        {
            return new JArray(clans.Keys);
        }

        [HookMethod("GetClanOf")]
        private string GetClanOf(object player)
        {
            if (player == null) throw new ArgumentException("player");
            var ply = player as BasePlayer;
            if (ply != null) player = ply.UserIDString;
            else if (player is ulong) player = player.ToString();
            else if (!(player is string)) return null;
            return findClanByUser(player as string)?.tag ?? string.Empty;
        }

        [HookMethod("GetClanMembers")]
        private List<ulong> GetClanMembers(ulong userID)
        {
            var findClanTag = GetClanOf(userID);
            if (string.IsNullOrEmpty(findClanTag)) return null;

            var mems = findClan(findClanTag)?.members ?? null;
            if (mems == null || mems.Count < 1) return null;

            var list = new List<ulong>();

            for(int i = 0; i < mems.Count; i++)
            {
                var mem = mems[i];
                if (string.IsNullOrEmpty(mem)) continue;
                ulong memID;
                if (ulong.TryParse(mem, out memID)) list.Add(memID);
            }

            return list;
        }

        private List<string> GetClanMembersByTag(string clanTag)
        {
            return findClan(clanTag)?.members;
        }

        private HashSet<string> GetAllClanTags()
        {
            var tags = new HashSet<string>();

            foreach (var kvp in clans)
                tags.Add(kvp.Key);

            return tags;
        }

        private void GetAllClanTagsNoAlloc(ref HashSet<string> tags)
        {
            foreach (var kvp in clans)
                tags.Add(kvp.Key);
        }

        // Available hooks
        // --------------------------------------------------------------------
        // OnClanCreate CLANTAG      Called when a new clan has been created
        // OnClanUpdate CLANTAG      Called when clan members or invites change
        // OnClanDestroy CLANTAG     Called when a clan is disbanded or deleted

        #endregion

        #region Utility Methods

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using (var fx = new Effect(effect, pos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using (var fx = new Effect(effect, player, boneID, localPos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        private void log(string message)
        {
            Interface.Oxide.LogInfo("{0}: {1}", Title, message);
        }

        private void warn(string message)
        {
            Interface.Oxide.LogWarning("{0}: {1}", Title, message);
        }

        private void error(string message, Exception ex = null)
        {
            if (ex != null)
                Interface.Oxide.LogException(string.Format("{0}: {1}", Title, message), ex);
            else
                Interface.Oxide.LogError("{0}: {1}", Title, message);
        }

        #endregion
    }
}
