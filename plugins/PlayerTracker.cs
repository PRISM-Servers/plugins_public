using System.Collections.Generic;
using System;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System.Diagnostics;

namespace Oxide.Plugins
{
    [Info("PlayerTracker", "Shady", "1.0.5", ResourceId = 0)]
    internal class PlayerTracker : CovalencePlugin
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
        private StoredData storedData;


        private class StoredData
        {
            public Dictionary<DateTime, int> playerCounts = new Dictionary<DateTime, int>();

            public void Set(DateTime time, int count)
            {
                if (time <= DateTime.MinValue) throw new ArgumentOutOfRangeException(nameof(time));
                if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
                playerCounts[time] = count;
            }

            public void Set(string time, int count)
            {
                Set(DateTime.Parse(time), count);
            }

            public void Remove(DateTime time)
            {
                if (time <= DateTime.MinValue) throw new ArgumentOutOfRangeException(nameof(time));
                playerCounts.Remove(time);
            }

            public void Remove(string time)
            {
                Remove(DateTime.Parse(time));
            }

            public StoredData() { }
        }

        private Timer checkTimer = null;
        private Timer saveTimer = null;

        private void Init()
        {
            storedData = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("PlayerTracker") ?? new StoredData();


            AddCovalenceCommand("avgp", nameof(cmdAverage));
        }

        private void OnServerInitialized()
        {
            checkTimer = timer.Every(600f, () => LogCount());
            
            saveTimer = timer.Every(1200f, () =>
            {
                var watch = Facepunch.Pool.Get<Stopwatch>();
                try { SaveData(); }
                finally
                {
                    try 
                    {
                        watch.Stop();
                        if (watch.ElapsedMilliseconds >= 125) PrintWarning("saveTimer took: " + watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms");
                    }
                    finally { Facepunch.Pool.Free(ref watch); }
                }
            });
            
            LogCount();
        }

        private void Unload()
        {
            saveTimer.Destroy();
            saveTimer = null;

            checkTimer.Destroy();
            checkTimer = null;

            SaveData();
        }


        private void SaveData()
        {

            Interface.Oxide.DataFileSystem.WriteObject("PlayerTracker", storedData);
        }

        private void SendReply(IPlayer player, string msg) => player?.Reply(msg);

        private void LogCount()
        {
            var pCount = BasePlayer.activePlayerList?.Count ?? 0;

            var time = DateTime.UtcNow;

            if (storedData != null) storedData.Set(time, pCount);
            else PrintWarning("storedData is null!?!?");

            PrintWarning("Player count is: " + pCount + " at: " + time);
        }

        private void cmdAverage(IPlayer player, string command, string[] args)
        {
            SendReply(player, "Average players: " + GetAveragePlayers().ToString("N0") + ", for this month: " + GetAveragePlayersMonth(DateTime.Now.Month).ToString("N0") + ", tracked since: " + GetFirstTrack() + " (UTC)");
            var highest = GetHighestPlayers();
            var peak = GetPeakHour();
            var peakLow = GetPeakLow();
            SendReply(player, "Peak average hour: " + peak.Key + ":00" + ", value: " + peak.Value.ToString("N0") + Environment.NewLine + "Max players ever: " + highest.Value.ToString("N0") + " at " + highest.Key + " (UTC)");
            SendReply(player, "Peak low average hour: " + peakLow.Key + ":00" + ", val: " + peakLow.Value.ToString("N0"));
        }

        private double GetAveragePlayers() 
        {
            if (storedData?.playerCounts == null) return -1d;
            var count = 0d;
            var sum = 0d;
            foreach(var kvp in storedData.playerCounts)
            {
                count++;
                sum += kvp.Value;
            }
            return sum / count;
        }

        private double GetAveragePlayersMonth(Month month) { return GetAveragePlayersMonth((int)month); }

        private double GetAveragePlayersMonth(int month)
        {
            if (month < 0 || month > 12) return -1;
            if (storedData?.playerCounts == null || storedData.playerCounts.Count < 1) return -1d;
            var year = DateTime.UtcNow.Year;
            var count = 0d;
            var sum = 0d;
            foreach(var kvp in storedData.playerCounts)
            {
                var key = kvp.Key;
                if (key.Year == year && key.Month == month)
                {
                    sum += kvp.Value;
                    count++;
                }
            }
            return sum / count;
         //   return storedData?.playerCounts?.Where(p => p.Key.Year == year && p.Key.Month == month)?.Select(p => p.Value)?.Average() ?? -1d;
        }
        /*/
        private double GetAveragePlayers(int hour)
        {
            if (hour < 0) return -1;
            return storedData?.playerCounts?.Where(p => p.Key.Hour == hour)?.Select(p => p.Value)?.Average() ?? -1d;
        }/*/

            /*/
        private double GetPeakAverage()
        {
            var hourTimes = new Dictionary<int, double>();
            var hours = storedData?.playerCounts?.Select(p => p.Key.Hour) ?? null;
            foreach (var hour in hours) hourTimes[hour] = storedData?.playerCounts?.Where(p => p.Key.Hour == hour)?.Average(p => p.Value) ?? 0f;
            var maxVal = hourTimes?.Values?.Max() ?? 0f;
            return hourTimes?.Where(p => p.Value == maxVal)?.FirstOrDefault().Value ?? -1d;
        }/*/

        private double GetAverageDayPlayers(string day)
        {
            if (string.IsNullOrEmpty(day)) throw new ArgumentNullException(nameof(day));

            var sum = 0;
            var count = 0;
            foreach (var kvp in storedData.playerCounts)
            {
                if (kvp.Key.ToString("d").Equals(day, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    sum += kvp.Value;
                }
            }
            return sum / count;
        }

        private KeyValuePair<int, double> GetPeakHour()
        {
            if (storedData?.playerCounts == null)
            {
                PrintWarning("playerCounts == null!!");
                return new KeyValuePair<int, double>();
            }
            var times = new Dictionary<int, double>();
            double outDouble;
            for(int i = 0; i < 24; i++)
            {
                if (times.TryGetValue(i, out outDouble)) continue;
                else
                {
                    var timesAtHour = new Dictionary<DateTime, int>();
                    foreach(var kvp in storedData.playerCounts)
                    {
                        var key = kvp.Key;
                        if (key.Hour == i)
                        {
                            timesAtHour[key] = kvp.Value;
                        }
                    }
                    var sum = 0d;
                    var count = 0d;
                    foreach (var kvp in timesAtHour)
                    {
                        count++;
                        sum += kvp.Value;
                       
                    }
                    times[i] = sum / count;
                }
            }
            if (times.Count < 1)
            {
                PrintWarning("times dictionary is empty in: " + nameof(GetPeakHour));
                throw new InvalidOperationException(nameof(times));
            }
            var maxVal = 0d;
            var maxHour = 0;
            foreach(var kvp in times)
            {
                var val = kvp.Value;
                if (val > maxVal)
                {
                    maxVal = val;
                    maxHour = kvp.Key;
                }
               
            }
            return new KeyValuePair<int, double>(maxHour, maxVal);
        }

        private KeyValuePair<int, double> GetPeakLow()
        {
            if (storedData?.playerCounts == null) return new KeyValuePair<int, double>();
            var times = new Dictionary<int, double>();
            double outDouble;
            for (int i = 0; i < 24; i++)
            {
                if (times.TryGetValue(i, out outDouble)) continue;
                else
                {
                    var timesAtHour = new Dictionary<DateTime, int>();
                    foreach (var kvp in storedData.playerCounts)
                    {
                        var key = kvp.Key;
                        if (key.Hour == i)
                        {
                            timesAtHour[key] = kvp.Value;
                        }
                    }
                    var sum = 0d;
                    var count = 0d;
                    foreach (var kvp in timesAtHour)
                    {
                        count++;
                        sum += kvp.Value;

                    }
                    times[i] = sum / count;
                }
            }
            if (times.Count < 1)
            {
                PrintWarning("times dictionary is empty in: " + nameof(GetPeakLow));
                throw new InvalidOperationException(nameof(times));
            }
            var minVal = -1d;
            var minHour = 0;
            foreach (var kvp in times)
            {
                var val = kvp.Value;
                if (minVal == -1 || val < minVal)
                {
                    minVal = val;
                    minHour = kvp.Key;
                }

            }

            return new KeyValuePair<int, double>(minHour, minVal);
        }

        private KeyValuePair<DateTime, int> GetHighestPlayers()
        {
            if (storedData?.playerCounts == null) return new KeyValuePair<DateTime, int>();
            var highestCount = 0;
            var date = DateTime.MinValue;
            foreach(var kvp in storedData.playerCounts)
            {
                var val = kvp.Value;
                if (val > highestCount)
                {
                    highestCount = val;
                    date = kvp.Key;
                }
            }
            return new KeyValuePair<DateTime, int>(date, highestCount);
        }

        private DateTime GetFirstTrack()
        {
            if (storedData?.playerCounts == null || storedData.playerCounts.Count < 1) return DateTime.MinValue;
            foreach(var kvp in storedData.playerCounts)
            {
                return kvp.Key;
            }
            return DateTime.MinValue;
        }

        public enum Month
        {
            NotSet = 0,
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }
    }
}
 