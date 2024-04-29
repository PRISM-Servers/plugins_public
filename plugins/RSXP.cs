using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("RSXP", "Shady", "0.0.2", ResourceId = 0000)]
    class RSXP : RustPlugin
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
        [ChatCommand("rsxp")]
        private void cmdRSXP(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            long lvl;
            if (!long.TryParse(args[0], out lvl))
            {
                SendReply(player, "Not int: " + args[0]);
                return;
            }
            SendReply(player, "XP for level " + lvl + " is: " + GetExperience(lvl).ToString("N0"));
        }

        [ChatCommand("rslvl")]
        private void cmdRSLvl(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            long lvl;
            if (!long.TryParse(args[0], out lvl))
            {
                SendReply(player, "Not int: " + args[0]);
                return;
            }
            SendReply(player, "level for XP " + lvl + " is: " + GetLevel(lvl).ToString("N0"));
        }

     

        [ChatCommand("rsxp2")]
        private void cmdRSXP2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 3) return;
            long lvl;
            if (!long.TryParse(args[0], out lvl))
            {
                SendReply(player, "Not int: " + args[0]);
                return;
            }

            int val1;

            int val2;

            if (!int.TryParse(args[1], out val1))
            {
                SendReply(player, "Not int: " + args[1]);
                return;
            }

            if (!int.TryParse(args[2], out val2))
            {
                SendReply(player, "Not int: " + args[2]);
                return;
            }

            SendReply(player, "(2) XP for level " + lvl + " is: " + GetExperience2(lvl, val1, val2).ToString("N0"));
        }

        [ChatCommand("rsxp3")]
        private void cmdRSXP3(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            
            long lvl;
            if (!long.TryParse(args[0], out lvl))
            {
                SendReply(player, "Not int: " + args[0]);
                return;
            }


            SendReply(player, "(2) XP for level " + lvl + " is: " + GetExperience3(lvl).ToString("N0"));
        }

        [ChatCommand("xp4")]
        private void cmdXP4(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 3) return;

            long lvl;
            if (!long.TryParse(args[0], out lvl))
            {
                SendReply(player, "Not int: " + args[0]);
                return;
            }

            int val1;

            double val2;

            if (!int.TryParse(args[1], out val1))
            {
                SendReply(player, "Not int: " + args[1]);
                return;
            }

            if (!double.TryParse(args[2], out val2))
            {
                SendReply(player, "Not double: " + args[2]);
                return;
            }
            
            SendReply(player, "(2) XP for level " + lvl + " is: " + getLevelPoints(lvl, val1, val2).ToString("N0"));
        }
        
        private long getLevelPoints(long level, int val1, double val2) { return val1 * level * level * ((long)Math.Round(level * val2, MidpointRounding.AwayFromZero)); }

        public long GetExperience2(long lvl, int val1, int val2)
        {
            if (lvl <= 0) return 0;


            /*/
             *      var a = 0L;

            for (int x = 1; x < lvl; x++) a += (long)(x + 300 * Math.Pow(2, x / (double)7));

            return (long)(a / (double)4); //experience
             /*/

            var a = 0L;


            for (int x = 1; x < lvl; x++) a += (long)(x + val1 * Math.Pow(2, x / (double)val2)); //900 (val1) / 8 (val2) is a pretty decent formula that ups the low end and keeps a relatively similar top end (level 99), but does make levels 100-120 easier

            return (long)(a / (double)4); //experience
        }

        public long GetExperience3(long lvl)
        {
            if (lvl <= 0) return 0;


            var a = 0L;

            for (int x = 1; x < lvl; x++) a += (long)(Math.Pow(x, Math.Log(10, Math.E)));


            return (long)(a / (double)4); //experience
        }

        public long GetExperience(long lvl)
        {
            if (lvl <= 0) return 0;

            var a = 0L;

            for (int x = 1; x < lvl; x++) a += (long)(x + 300 * Math.Pow(2, x / (double)7));

            return (long)(a / (double)4); //experience
        }

        public long GetLevel(long experience, int max = 1000) //this is terrible but please make it better somehow for me thanks
        {
            var count = 0;
            var start = 1; //level can't be 0, so start at 1
            var lvl = -1;
            
            var dic = new Dictionary<int, long>();
           
            
            while(true)
            {
                count++;
                if (count >= max) break; //safety break
                
                var xpLvl = GetExperience(start);
                if (xpLvl == experience)
                {
                    lvl = start;
                    break;
                }

                var xpNext = GetExperience(start + 1);
                var diff = (xpLvl > experience) ? (xpLvl - experience) : (xpLvl < experience) ? (experience - xpLvl) : 0;
                dic[start] = diff;
                
                if (experience < xpNext) break;
                start++;
            }
            
            if (lvl == -1)
            {
                var lowestDiff = dic?.Where(p => p.Value == dic.Values.Min())?.FirstOrDefault() ?? null;
                if (lowestDiff.HasValue) lvl = lowestDiff.Value.Key;
            }
            
            return lvl;
        }

    }
}