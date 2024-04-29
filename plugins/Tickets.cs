using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    static class Extension
    {
        public static void TicketReply(this IPlayer player, string msg, bool noPrefix = false)
        {
            if (!player.IsServer)
            {
                if (!noPrefix) if (!msg.StartsWith("[Tickets]")) msg = "[Tickets] " + msg;
                msg = msg.Replace("[Tickets]", "<color=#C4FF00>[Tickets]</color>");
                msg = msg.Replace("[OPEN]", "<color=#C4FF00>[OPEN]</color>");
                msg = msg.Replace("[CLOSED]", "<color=red>[CLOSED]</color>");
            }
            else msg = Tickets.instance.RemoveTags(msg);
            player.Reply(msg);
        }
    }

    [Info("Tickets", "MBR", "1.1.0")]
    class Tickets : RustPlugin
    {
        [PluginReference] private readonly Plugin DiscordAPI2, Compilation, PRISMID;

        private const string base_url = "https://prismrust.com";
        public List<Ticket> tickets = new List<Ticket>();
        public static Tickets instance;
        private readonly Dictionary<IPlayer, float> cooldown = new Dictionary<IPlayer, float>();

        private static string ServerId
        {
            get
            {
                if (!(instance.PRISMID?.IsLoaded ?? false))
                {
                    throw new Exception("PRISMID is not loaded");
                }

                return instance.PRISMID.Call<string>("GetServerTypeString");
            }
        }

        private static int ServerIdInt
        {
            get
            {
                if (!(instance.PRISMID?.IsLoaded ?? false))
                {
                    throw new Exception("PRISMID is not loaded");
                }

                return instance.PRISMID.Call<int>("GetServerTypeInt");
            }
        }

        #region Hooks

        private void OnServerInitialized()
        {
            instance = this;

            AddCovalenceCommand(new string[] { "ticket" }, nameof(TicketCMD));
            AddCovalenceCommand(new string[] { "report" }, nameof(ReportCMD));

            timer.Once(2f, Update);
            timer.Every(3600f, Update);
        }

        private void Unload()
        {
            instance = null;
        }

        private void OnUserConnected(IPlayer player)
        {
            if (player.IsAdmin)
            {
                player.TicketReply("Welcome, there are " + tickets.Where(x => x.Open).Count() + " open tickets");
            }
        }

        void OnPlayerReported(BasePlayer reporter, string targetName, string targetId, string subject, string message, string type)
        {
            if (type == "name" || type == "abusive") return;

            var player = reporter.IPlayer;

            if (cooldown.ContainsKey(player))
            {
                if (UnityEngine.Time.realtimeSinceStartup < cooldown[player] && !player.IsAdmin)
                {
                    player.TicketReply("You are creating tickets too fast, try again in " + System.Math.Round((cooldown[player] - UnityEngine.Time.realtimeSinceStartup), 2) + "s");
                    return;
                }

                cooldown.Remove(player);
            }

            cooldown.Add(player, UnityEngine.Time.realtimeSinceStartup + 10);

            var target = covalence.Players.FindPlayerById(targetId) ?? null;

            if (target != null)
            {
                if (target.Id == "76561198865536053" || target.Id == "76561198886960011")
                {
                    Puts("Ignoring report by " + player + " for PRISM account");
                    player.TicketReply("Ticket creation failed: \"PRISM\" cannot be reported");
                    return;
                }
            }

            CreateTicket(player, (subject + "\n\n" + message).Replace("[" + type + "]", "").Trim(), target);
        }

        #endregion

        #region Methods

        private void CloseOnBan(string id)
        {
            if (!id.StartsWith("765611") || id.Length < 15) return;

            var tickets1 = tickets.Where(x => (x.reported?.id ?? "") == id && x.Open);
            var tickets2 = tickets.Where(x => x.creator.id == id && x.Open);

            if (tickets1.Count() > 0)
            {
                if (tickets1.Count() == 1)
                {
                    tickets1.First().Close(Ticket.TicketUser.server, "Reported person was banned");
                    return;
                }

                BulkDelete(tickets1.ToList(), "Reported person was banned", Ticket.TicketUser.server);
            }

            if (tickets2.Count() > 0)
            {
                BulkDelete(tickets2.ToList(), "Ticket creator was banned", Ticket.TicketUser.server);
            }
        }

        private void CreateTicket(IPlayer player, string command, string[] args)
        {
            if (player.IsServer)
            {
                player.TicketReply("Server Console cannot create tickets!");
                return;
            }

            if (args.Length > 0 && args[0].ToLower().Trim() == "create") args = args.Skip(1).ToArray();
            if (args.Length < 1)
            {
                player.TicketReply("Please specify your reason for creating a ticket.");    
                return;
            }

            if (cooldown.ContainsKey(player))
            {
                if (UnityEngine.Time.realtimeSinceStartup < cooldown[player] && !player.IsAdmin)
                {
                    player.TicketReply("You are creating tickets too fast, try again in " + System.Math.Round((cooldown[player] - UnityEngine.Time.realtimeSinceStartup), 2) + "s");
                    return;
                }

                cooldown.Remove(player);
            }

            CreateTicket(player, string.Join(" ", args), (IPlayer)null);
        }

        public Ticket Find(int ID)
        {
            foreach (var ticket in tickets)
            {
                if (ticket.id == ID) return ticket;
            }

            return null;
        }

        public int ReportsForPlayer(string id)
        {
            return tickets.Where(x => x.Open && (x.reported?.id ?? "") == id).Count();
        }

        public void BulkDelete(List<Ticket> tickets, string reason, Ticket.TicketUser issuer)
        {
            BulkDelete(tickets.Select(x => x._id).ToList(), reason, issuer);
        }

        public void BulkDelete(List<string> unique_ids, string reason, Ticket.TicketUser issuer)
        {
            var data = new
            {
                tickets = unique_ids,
                by = issuer,
                reason,
            };

            CreateInternalRequest(base_url + "/api/tickets/bulk-close", Utility.ConvertToJson(data, true), RequestMethod.POST);
        }

        public void Update()
        {
            CreateInternalRequest(base_url + "/api/tickets", null, RequestMethod.GET, (code, response) =>
            {
                if (code >= 400 || string.IsNullOrEmpty(response))
                {
                    SendToMothership("Failed to update ticket list, code: " + code + ", response: " + response);
                    return;
                }

                List<Ticket> list = null;

                try { list = Utility.ConvertFromJson<List<Ticket>>(response); }
                catch (Exception e)
                {
                    instance.PrintWarning("Exception while fetching ticket list!\n" + e);
                    return;
                }

                tickets = list.Where(x => x.server == ServerIdInt).ToList();
            });
        }

        #endregion

        #region Commands

        [ConsoleCommand("ticketlist.json")]
        private void TicketJSONCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            arg.ReplyWith(Utility.ConvertToJson(instance.tickets));
        }

        private void TicketCMD(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                var cases = new List<string> {
                    "/" + command + " create <i>text</i>",
                    "/" + command + " close <i>ticket ID</i>",
                    "/" + command + " reply <i>ticket ID</i> <i>text</i>",
                    "/" + command + " view <i>ticket ID</i>",
                    "/" + command + " list" + (player.IsAdmin ? " <i>all</i>" : ""),
                    player.IsAdmin ? "/" + command + " teleport <i>ticket ID</i>" : "",
                };

                player.TicketReply("Please supply a player name & reason to report them, or otherwise specify your reason for making a report/ticket.");
                player.TicketReply("Invalid syntax!\n" + string.Join("\n", cases));
                return;
            }

            switch (args[0].ToLower())
            {
                case "close":
                    {
                        args = args.Skip(1).ToArray();

                        if (args.Length < 1)
                        {
                            player.TicketReply("You did not provide ticket ID!");
                            return;
                        }

                        var ID_str = args[0];
                        var ID = -1;
                        if (!int.TryParse(ID_str, out ID))
                        {
                            player.TicketReply(ID_str + " is not a number!");
                            return;
                        }

                        if (ID == -1)
                        {
                            player.TicketReply("Invalid ticket ID (" + ID + ")");
                            return;
                        }

                        var ticket = Find(ID);
                        if (ticket == null)
                        {
                            player.TicketReply("Couldn't find a ticket with ID " + ID);
                            return;
                        }

                        if (ticket.creator.id != player.Id && !player.IsAdmin)
                        {
                            player.TicketReply("You can only close your own tickets!");
                            return;
                        }

                        if (!ticket.Open)
                        {
                            player.TicketReply("This ticket is already closed.");
                            return;
                        }

                        if (args.Length < 2) ticket.Close(player);
                        else ticket.Close(player, string.Join(" ", args.Skip(1)));

                        return;
                    }

                case "teleport":
                    {
                        if (!player.IsAdmin) return;

                        if (player.IsServer)
                        {
                            player.Reply("Server Console cannot teleport!");
                            return;
                        }

                        args = args.Skip(1).ToArray();

                        if (args.Length < 1)
                        {
                            player.TicketReply("You did not provide ticket ID!");
                            return;
                        }

                        var ID_str = args[0];
                        var ID = -1;
                        if (!int.TryParse(ID_str, out ID))
                        {
                            player.TicketReply(ID_str + " is not a number!");
                            return;
                        }

                        if (ID == -1)
                        {
                            player.TicketReply("Invalid ticket ID (" + ID + ")");
                            return;
                        }

                        var ticket = Find(ID);
                        if (ticket == null)
                        {
                            player.Reply("Couldn't find a ticket with ID " + ID);
                            return;
                        }

                        TeleportPlayer(player.Object as BasePlayer, new Vector3(ticket.position[0], ticket.position[1], ticket.position[2]));
                        player.Reply("You were teleported to " + ticket.position.ToString());
                        return;
                    }

                case "view":
                    {
                        args = args.Skip(1).ToArray();

                        if (args.Length < 1)
                        {
                            player.TicketReply("You did not provide ticket ID!");
                            return;
                        }

                        var ID_str = args[0];
                        var ID = -1;
                        if (!int.TryParse(ID_str, out ID))
                        {
                            player.TicketReply(ID_str + " is not a number!");
                            return;
                        }

                        if (ID == -1)
                        {
                            player.TicketReply("Invalid ticket ID (" + ID + ")");
                            return;
                        }

                        var ticket = Find(ID);
                        if (ticket == null)
                        {
                            player.Reply("Couldn't find a ticket with ID " + ID);
                            return;
                        }

                        if (ticket.creator.id != player.Id && !player.IsAdmin) return;

                        var replySB = new StringBuilder();
                        var replyList = new List<StringBuilder>();

                        var msg = "[Tickets] [" + (ticket.Open ? "OPEN" : "CLOSED") + "] Viewing Ticket #" + ticket.id + " by " + ticket.creator + "\n" + (ticket.reported == null ? "" : "\nReported person: " + ticket.reported + "\n");

                        var replies = ticket.replies.Where(x => x.content != "Automated Report").Select(x => x.ToString()).ToList();

                        if (!ticket.Open) replies.Add("Closed by <color=#C4FF00>" + ticket.closed.by.name + "</color> at " + ticket.closed.at.ToString() + " UTC" + (string.IsNullOrEmpty(ticket.closed.reason) ? "" : " - " + ticket.closed.reason));

                        if (replies.Count() > 0) msg += string.Join("\n", replies);

                        for (int i = 0; i < msg.Length; i++)
                        {
                            var chr = msg[i];
                            if (replySB.Length >= 768)
                            {
                                replyList.Add(replySB);
                                replySB = new StringBuilder();
                            }
                            replySB.Append(chr);
                        }

                        if (replyList.Count > 0) for (int i = 0; i < replyList.Count; i++) player.TicketReply(replyList[i].ToString());
                        if (replySB.Length > 0) player.TicketReply(replySB.ToString());
                        return;
                    }

                case "list":
                    {
                        bool all = false;
                        if (args.Length > 1 && args[1].ToLower() == "all" && (player.IsAdmin || player.IsServer)) all = true;
                        var list = tickets.Where(x => (all || x.Open) && (player.IsAdmin || x.creator.id == player.Id)).Select(x => "[" + (x.Open ? "OPEN" : "CLOSED") + "] Ticket #" + x.id + " by " + x.creator.name).ToList();
                        var ticketSBF = new StringBuilder();
                        var finishSB = new List<StringBuilder>();
                        var isServer = player.IsServer;

                        for (int i = 0; i < list.Count; i++)
                        {
                            var ticketStr = list[i];
                            if (string.IsNullOrEmpty(ticketStr)) continue;
                            var removeTagsStr = RemoveTags(ticketStr);
                            if (!isServer && (ticketSBF.Length + removeTagsStr.Length) > 700)
                            {
                                finishSB.Add(ticketSBF);
                                ticketSBF = new StringBuilder();
                            }
                            ticketSBF.AppendLine(removeTagsStr);
                        }

                        if (ticketSBF.Length < 1) player.TicketReply("No tickets!");
                        else
                        {
                            player.TicketReply("List of " + (all ? "all" : "open") + " tickets");
                            if (finishSB.Count > 0) for (int i = 0; i < finishSB.Count; i++) player.TicketReply(finishSB[i].ToString().TrimEnd(), true);
                            player.TicketReply(ticketSBF.ToString().TrimEnd(), true);
                        }

                        return;
                    }

                case "reply":
                    {
                        args = args.Skip(1).ToArray();

                        if (args.Length < 2)
                        {
                            player.TicketReply("You did not provide ticket ID/text to reply with!");
                            return;
                        }

                        var ID_str = args[0];
                        var ID = -1;
                        if (!int.TryParse(ID_str, out ID))
                        {
                            player.TicketReply(ID_str + " is not a number!");
                            return;
                        }

                        if (ID == -1)
                        {
                            player.TicketReply("Invalid ticket ID (" + ID + ")");
                            return;
                        }

                        var ticket = Find(ID);
                        if (ticket == null)
                        {
                            player.TicketReply("Couldn't find a ticket with ID " + ID);
                            return;
                        }

                        if (!ticket.Open)
                        {
                            player.TicketReply("This ticket is closed.");
                            return;
                        }

                        if (ticket.creator.id != player.Id && !player.IsAdmin)
                        {
                            player.TicketReply("You cannot reply to a ticket that is not created by you.");
                            return;
                        }

                        args = args.Skip(1).ToArray();
                        var text = string.Join(" ", args);

                        ticket.Reply(player, text);
                        return;
                    }

                case "create":
                default:
                    {
                        CreateTicket(player, command, args);
                        return;
                    }
            }
        }

        private void ReportCMD(IPlayer player, string command, string[] args)
        {
            player.Reply("Please use the report menu <color=green>(F7 key)</color> if you want to report someone, for bugs use the <color=#C4FF00>/ticket</color> command");
        }

        #endregion

        #region Ticket class

        public class Ticket
        {
            public string _id;
            public int id = -1;
            public DateTime created_at = DateTime.UtcNow;
            public TicketUser creator;
            public List<TicketReply> replies = new List<TicketReply>();
            public TicketUser reported;
            public CloseInfo closed;
            public int server;
            public List<int> position;

            public bool Open
            {
                get
                {
                    return closed == null;
                }
            }

            public Ticket() { }

            public Ticket(IPlayer creator, string text, IPlayer reported)
            {
                server = ServerIdInt;
                id = instance.tickets.Count == 0 ? 1 : instance.tickets.Select(x => x.id).Max() + 1;

                this.creator = new TicketUser(creator);
 
                var bp = creator.Object as BasePlayer;

                replies = new List<TicketReply> { new TicketReply(creator, text) };
                this.reported = reported == null ? null : new TicketUser(reported);

                if (bp == null) position = new List<int> { 0, 0, 0 };
                else
                {
                    var temp = Vector3Int.RoundToInt(bp.transform.position);
                    position = new List<int> { temp.x, temp.y, temp.z };
                }

                CreateInternalRequest(base_url + "/api/tickets", Utility.ConvertToJson(this, true), RequestMethod.POST, (code, response) =>
                {
                    if (code < 400) return;

                    if (code == 429)
                    {
                        creator.TicketReply("You already reported this person recently.");
                        return;
                    }

                    instance.Puts("Failed to create ticket #" + id + ": " + code + " (" + response + ")");
                    instance.SendToMothership("Failed to create ticket #" + id + ": " + code + " (" + response + ")");
                });
            }

            public void Reply(IPlayer player, string text)
            {
                if (!player.IsAdmin && !Open)
                {
                    player.TicketReply("You cannot reply to a closed ticket");
                    return;
                }

                var reply = new TicketReply(player, text);

                CreateInternalRequest(base_url + "/api/tickets/" + _id, Utility.ConvertToJson(reply, true), RequestMethod.PATCH);
            }

            public void BroadcastToAdminsAndCreator(string msg)
            {
                //msg = msg.Replace("[Tickets]", "<color=#C4FF00>[Tickets]</color>");
                //msg = msg.Replace("[OPEN]", "<color=#C4FF00>[OPEN]</color>");
                //msg = msg.Replace("[CLOSED]", "<color=red>[CLOSED]</color>");

                //https://stackoverflow.com/a/60103725
                foreach (var player in BasePlayer.activePlayerList.ToArray())
                {
                    if (creator.id == player.UserIDString || player.IsAdmin)
                    {
                        instance.Puts("sending msg to " + player);
                        player.IPlayer.TicketReply(msg);
                    }
                }
            }

            public void Close(TicketUser user, string text = null)
            {
                if (!Open) return;

                CreateInternalRequest(base_url + "/api/tickets/" + _id, Utility.ConvertToJson(new
                {
                    by = user,
                    reason = text
                }, true), RequestMethod.DELETE);
            }

            public void Close(IPlayer player, string text = null)
            {
                if (!Open) return;

                Close(new TicketUser(player), text);
            }

            public class TicketUser
            {
                public static TicketUser server = new TicketUser { id = "0", name = "Server Console" };

                public string id;
                public string name;

                public TicketUser() { }

                public TicketUser(IPlayer player)
                {
                    id = player.Id;
                    name = player.Name;
                }

                public override string ToString()
                {
                    if (id == "0") return name;
                    return name + "/" + id;
                }
            }

            public class CloseInfo
            {
                public DateTime at;
                public TicketUser by;
                public string reason;
            }

            public class TicketReply
            {
                public string content;
                public DateTime date;
                public TicketUser replier;
                

                public TicketReply() { }

                public TicketReply(IPlayer player, string text)
                {
                    replier = new TicketUser(player);
                    content = text;
                }

                public override string ToString()
                {
                    return "<color=#C4FF00>" + this.replier.name + "</color> - " + date.ToString() + " UTC\n" + content;
                }
            }
        }

        #endregion

        #region WS

        private void WSTicketClose(string json)
        {
            var ticket = Utility.ConvertFromJson<Ticket>(json);
            tickets[tickets.IndexOf(tickets.Find(x => x._id == ticket._id))] = ticket;

            var msg = "[Tickets] Ticket #" + ticket.id + " was closed by " + ticket.closed.by.name + (!string.IsNullOrEmpty(ticket.closed.reason) ? ", reason: " + ticket.closed.reason : "");
            msg = RemoveTags(msg);
            Puts(msg);
            ticket.BroadcastToAdminsAndCreator(msg);
        }

        private void WSTicketCreate(string json)
        {
            var ticket = Utility.ConvertFromJson<Ticket>(json);
            tickets.Add(ticket);

            var msg = "[Tickets] New Ticket #" + ticket.id + ":\n" + (ticket.reported == null ? "" : "Reported person: " + ticket.reported + "\n") + ticket.replies.First().ToString();
            msg = RemoveTags(msg);
            Puts(msg);
            ticket.BroadcastToAdminsAndCreator(msg);
        }

        private void WSTicketReply(string json)
        {
            var ticket = Utility.ConvertFromJson<Ticket>(json);
            var e = tickets.Find(x => x._id == ticket._id);

            if (e == null)
            {
                tickets.Add(e);
            }
            else tickets[tickets.IndexOf(e)] = ticket;

            var last_reply = ticket.replies.Last();
            var msg = "[Tickets] New reply to Ticket #" + ticket.id + ":\n" + last_reply.replier.name + " - " + last_reply.date.ToString("HH:mm:ss") + " UTC\n" + last_reply.content;
            msg = RemoveTags(msg);
            Puts(msg);
            ticket.BroadcastToAdminsAndCreator(msg);
        }

        private void WSTicketBulkClose(List<string> ticket_ids, string reason, string id, string name)
        {
            if (ticket_ids == null || ticket_ids.Count < 1)
            {
                PrintWarning("Got a TICKET_BULK_CLOSE with missing ticket list");
                return;
            }

            if (string.IsNullOrEmpty(reason))
            {
                PrintWarning("Got a TICKET_BULK_CLOSE with missing reason");
                return;
            }

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                PrintWarning("Got a TICKET_BULK_CLOSE with missing name/id");
                return;
            }

            var tickets = new List<Ticket>();

            foreach (var ticket in this.tickets)
            {
                if (ticket_ids.Contains(ticket._id))
                {
                    tickets.Add(ticket);
                    ticket.closed = new Ticket.CloseInfo
                    {
                        at = DateTime.UtcNow,
                        by = new Ticket.TicketUser
                        {
                            id = id,
                            name = name
                        },
                        reason = reason
                    };
                }
            }

            if (tickets.Count > 0)
            {
                var message = tickets.Count + " tickets were closed by " + tickets[0].closed.by.name + ", reason: \"" + reason + "\"" + (tickets.Count < 20 ? "\n(" + string.Join(", ", tickets.Select(x => x.id)) + ")" : "");
                Puts(message);
            }
        }

        #endregion

        #region Helpers

        public string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;
            //	Forbidden formatting tags
            var forbiddenTags = new List<string> {
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
            for (int i = 0; i < forbiddenTags.Count; i++)
            {
                var tag = forbiddenTags[i];
                phraseSB.Replace(tag, string.Empty);
            }

            return phraseSB.ToString();
        }

        private void SendToMothership(string text)
        {
            DiscordAPI2.Call<bool>("SendToMothership", text);
        }

        private void TeleportPlayer(BasePlayer player, Vector3 pos)
        {
            if (Compilation == null || !Compilation.IsLoaded) return;
            Compilation.Call("TeleportPlayer", player, pos);
        }

        private static void CreateInternalRequest(string URL, string body, RequestMethod method, Action<int, string> callback = null)
        {
            if (string.IsNullOrEmpty(URL)) return;

            var token = instance.DiscordAPI2.Call<string>("GenerateToken", "", "prism_" + ServerId);

            var headers = new Dictionary<string, string>
            {
                { "User-Agent", "PRISM " + ServerId + (instance.Version.ToString()) },
                { "Authorization", token }
            };

            if (method != RequestMethod.GET && !string.IsNullOrEmpty(body)) headers.Add("Content-Type", "application/json");

            instance.webrequest.Enqueue(URL, body, (c, response) =>
            {
                if (c >= 400) instance.Puts("Failed to send a " + method.ToString() + " request to " + URL + " (" + response + " |" + c + ")");
                if (callback != null) callback.Invoke(c, response);
            }, instance, method, headers, 10);
        }

        private bool CreateTicket(IPlayer player, string text, IPlayer reported)
        {
            if (string.IsNullOrEmpty(text))
            {
                PrintError("CreateTicket called but not text??");
                return false;
            }

            var ticket = new Ticket(player, text, reported);
            return ticket != null;
        }

        #endregion
    }
}
