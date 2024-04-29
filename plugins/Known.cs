using System;
using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core;
using Newtonsoft.Json;
using System.Text;

namespace Oxide.Plugins
{
    [Info("Known", "Shady", "0.04.", ResourceId = 0)]
    [Description("Keep track of player aliases")]
    internal class Known : CovalencePlugin
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
        //  private const bool CHECK_STEAM = true;

        private class StoredData
        {
            public Dictionary<string, string> lastName = new Dictionary<string, string>();
            public Dictionary<string, List<string>> playerNames = new Dictionary<string, List<string>>();
            public StoredData() { }
        }

        private class Aliases
        {
            public class Player
            {
                [JsonProperty("newname")]
                public string newname;
                [JsonProperty("timechanged")]
                public string timechanged;
            }
        }

        private StoredData storedData;


        private Timer _saveTimer = null;

        private void Init()
        {
            storedData = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("KnownDB") ?? new StoredData();
            
            _saveTimer = timer.Every(18000f, () =>
            {
                PrintWarning(nameof(Known) + " saving data...");

                SaveData();
                PrintWarning("Saved data, will do again in 18000 seconds");
            });
            
        }
        
        private void Unload()
        {
            SaveData();
            
            _saveTimer?.Destroy();
            _saveTimer = null;
        }

        #region Hooks
        private void OnUserConnected(IPlayer player)
        {
            if (player == null) return;

            var ID = player.Id;
            var name = player.Name;
         
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(name)) return;
            storedData.lastName[ID] = name;
            var listNames = new List<string>();
            if (!storedData.playerNames.TryGetValue(player.Id, out listNames)) storedData.playerNames[player.Id] = (listNames = new List<string>());
            if (listNames == null || !listNames.Contains(name))
            {
                listNames.Add(name);
                storedData.playerNames[player.Id] = listNames;
            }

          //  if (!CHECK_STEAM) return;

            var plyAddr = (player != null && player.IsConnected) ? player.Address : string.Empty;

            webrequest.Enqueue("https://steamcommunity.com/profiles/" + ID + "/ajaxaliases", null, (code, response) =>
            {
                if (code != 200 || string.IsNullOrEmpty(response))
                {
                    PrintWarning("Bad response for: " + name + " (" + ID + ", " + plyAddr + ") on aliases");
                    return;
                }

                if (response.IndexOf("DOCTYPE", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    PrintWarning("Ignoring HTML response: " + response);
                    return;
                }

                List<Aliases.Player> aliases = null;
                try { aliases = JsonConvert.DeserializeObject<List<Aliases.Player>>(response); }
                catch(Exception ex)
                {
                    PrintError(ex.ToString());
                    PrintWarning("Exception on JsonConvert.DeserializeObject with response of:" + Environment.NewLine + response);

                    throw;
                }
                
                if (aliases == null || aliases.Count < 0)
                {
                    PrintWarning("Response for aliases OK but aliases is null or empty");
                    return;
                }

                var addSB = new StringBuilder();
                for (int i = 0; i < aliases.Count; i++)
                {
                    var aliasClass = aliases[i];
                    if (string.IsNullOrEmpty(aliasClass.newname))
                    {
                        PrintWarning("Alias empty: " + i);
                        continue;
                    }
                    if (!listNames.Contains(aliasClass.newname))
                    {
                        addSB.Append(aliasClass.newname).Append(" : ").Append(aliasClass.timechanged).Append(Environment.NewLine);
                        storedData.playerNames[player.Id].Add(aliasClass.newname);
                    }
                }

                if (addSB.Length > 0)
                {
                    if (addSB.Length > 1) addSB.Length -= 1;
                    PrintWarning("Added aliases for " + player.Name + " (" + player.Id + "): " + Environment.NewLine + addSB.ToString().TrimEnd());
                }

            }, this);
        }

        #endregion

        #region API
        private List<string> GetAliases(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));

            List<string> listStr;
            if (storedData.playerNames.TryGetValue(userID, out listStr)) return listStr;

            return null;
        }
        #endregion
        #region Helpers
        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("KnownDB", storedData);
        #endregion
    }
}
