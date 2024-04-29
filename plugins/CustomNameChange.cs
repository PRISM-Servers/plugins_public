using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Oxide.Plugins
{
    [Info("CustomNameChange", "MBR", "1.0.2")]
    class CustomNameChange : RustPlugin
    {
        private Dictionary<string, int> Changes = new Dictionary<string, int>();

        #region Hooks

        private void Init()
        {
            Changes = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, int>>(this.Name);
        }

        private void Unload() => SaveData();

        #endregion

        #region Commands

        [ChatCommand("itemnamechange")]
        private void ChangeItemnameCMD(BasePlayer player, string command, string[] args)
        {
            int val = 0;
            if (!Changes.TryGetValue(player.UserIDString, out val) || val == 0)
            {
                SendReply(player, "<color=red>You don't have any item name changes left.</color>");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "You currently have <color=orange>" + val + "</color> item name change" + (val == 1 ? "" : "s"));
                return;
            }

            Item helditem = player.GetActiveItem();

            if (helditem == null)
            {
                SendReply(player, "You're not holding an item!");
                return;
            }

            if (helditem.amount != 1)
            {
                SendReply(player, "You can only rename one item!");
                return;
            }

            var text = string.Join(" ", args);

            var filteredText = RemoveTags(text);

            if (filteredText.Length > 20)
            {
                SendReply(player, "Name is too long, please pick a shorter name! (20 letters)");
                return;
            }

            helditem.name = filteredText;
            SendReply(player, "New name is: " + helditem.name);
            helditem.MarkDirty();

            if (Changes[player.UserIDString] == 1) Changes.Remove(player.UserIDString);
            else Changes[player.UserIDString]--;
        }


        [ConsoleCommand("giveitemnamechange")]
        private void AddItemNameChangeCMD(ConsoleSystem.Arg arg)
        {
            if (arg == null || (arg.Connection != null && !arg.IsAdmin)) return;

            var args = arg.Args;
            if (args == null || args.Length < 1)
            {
                Puts("Invalid command! additemnamechange <name|ID>");
                return;
            }

            var player = FindConnectedPlayer(args[0], true);
            if (player == null)
            {
                Puts("Couldn't find player " + args[0]);
                return;
            }

            var count = 1;
            if (args.Length > 1 && !int.TryParse(args[1], out count))
            {
                SendReply(arg, "Not a valid int: " + args[1]);
                return;
            }

            var bp = player.Object as BasePlayer;

            if (bp != null && bp.IsConnected) SendReply(bp, "<color=green>You have been given 1 item name change! <i>/itemnamechange</i></color>");

            if (!Changes.ContainsKey(player.Id)) Changes.Add(player.Id, count);
            else Changes[player.Id] += count;
            Puts("Gave " + count + " item name change" + (count == 1 ? "" : "s") + " to " + player.Id + "/" + player.Name); 
        }

        [ConsoleCommand("inccount")]
        private void CCMD(ConsoleSystem.Arg arg)
        {
            if (arg == null || (arg.Connection != null && !arg.IsAdmin)) return;

            var arr = new List<string>();
            foreach (var item in Changes) arr.Add(item.Key + ": " + item.Value);
            Puts(string.Join("\n", arr));
        }
        
        #endregion

        #region Methods

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(this.Name, Changes);

        IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException();
            try
            {
                var p = covalence.Players.FindPlayer(nameOrIdOrIp);
                if (p != null) if ((!p.IsConnected && tryFindOfflineIfNoOnline) || p.IsConnected) return p;
                var connected = covalence.Players.Connected;
                List<IPlayer> players = new List<IPlayer>();
                foreach (var player in connected)
                {
                    var IP = player?.Address ?? string.Empty;
                    var name = player?.Name ?? string.Empty;
                    var ID = player?.Id ?? string.Empty;
                    if (ID.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || IP == nameOrIdOrIp || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(player);
                }
                if (players.Count <= 0 && tryFindOfflineIfNoOnline)
                {
                    foreach (var offlinePlayer in covalence.Players.All)
                    {
                        if (offlinePlayer.IsConnected) continue;
                        var name = offlinePlayer?.Name ?? string.Empty;
                        var ID = offlinePlayer?.Id ?? string.Empty;
                        if (ID.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(offlinePlayer);
                    }
                }
                if (players.Count > 1 || players.Count <= 0) return null;
                else return players[0];
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString() + " ^ FindConnectedPlayer ^ ");
                return null;
            }
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
            for (int i = 0; i < forbiddenTags.Count; i++)
            {
                var tag = forbiddenTags[i];
                phraseSB.Replace(tag, string.Empty);
            }

            return phraseSB.ToString();
        }
        
        #endregion
    }
}