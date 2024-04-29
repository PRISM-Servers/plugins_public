using UnityEngine;
using System.Collections.Generic;
using System;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
using System.Text;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("LevelsGUI", "Shady", "0.0.8", ResourceId = 0)]
    [Description("Handles GUI for levels plugins")]
    public class LevelsGUI : RustPlugin
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
        private Dictionary<string, int> guiSkillIndex = new Dictionary<string, int>();


        private HashSet<ulong> _guiOff = new HashSet<ulong>();
        private readonly HashSet<string> _skills = new HashSet<string>();
        private readonly HashSet<ulong> _baseGUI = new HashSet<ulong>();


        private readonly Dictionary<BasePlayer, List<LastGUIInfo>> _guiInfos = new Dictionary<BasePlayer, List<LastGUIInfo>>();


        private class LastGUIInfo
        {
            public long Level { get; set; }
            public string Skill { get; set; }
            public string SkillColor { get; set; }
            public float Percent { get; set; }
        }

        private LastGUIInfo FindInfoBySkill(BasePlayer player, string skill)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            List<LastGUIInfo> infos;
            if (_guiInfos.TryGetValue(player, out infos) && infos?.Count > 0)
            {
                for(int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];
                    if (info.Skill == skill) return info;
                }
            }

            return null;
        }


        #region Hooks
        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;

            _guiOff.Remove(player.userID);
            DestroyGUI(player);
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;

            DestroyGUI(player);
            CreateGUI(player);
        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null) return;

            CreateGUIFromCaches(player);
        }

        private void OnPlayerDeath(BasePlayer victim)
        {
            if (victim != null && victim.IsConnected) DestroyGUI(victim);
        }
        #endregion
        #region GUI
        private void RemoveSkillIndexAndSort(string skill)
        {
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            int index;
            if (!guiSkillIndex.TryGetValue(skill, out index)) return;

            guiSkillIndex.Remove(skill);

            var newIndex = new Dictionary<string, int>();
            var val = 0;
            foreach (var kvp in guiSkillIndex)
            {
                newIndex[kvp.Key] = val;
                val++;
            }

            guiSkillIndex = newIndex;
        }

        private void AddSkillIndexAndSort(string skill, int indexToAdd)
        {
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            int index;
            if (guiSkillIndex.TryGetValue(skill, out index)) return;

            guiSkillIndex.Add(skill, indexToAdd);
            var newIndex = new Dictionary<string, int>();
            var val = 0;
            foreach (var kvp in guiSkillIndex)
            {
                newIndex[kvp.Key] = val;
                val++;
            }

            guiSkillIndex = newIndex;
        }

        private void RemoveSkill(string skill)
        {
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));
            if (_skills.Remove(skill))
            {
                RemoveSkillIndexAndSort(skill);
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || !p.IsConnected) continue;

                    DestroyGUI(p);
                    foreach (var s in _skills)
                    {
                        var findInfo = FindInfoBySkill(p, s);
                        if (findInfo == null) continue;

                        GUIUpdateSkillFromInfo(p, findInfo);
                    }
                }
            }
            else PrintWarning("RemoveSkill returned false with Skills.Remove with skill: " + skill);
        }

        private void AddSkill(string skill)
        {
            if (string.IsNullOrEmpty(skill)) 
                throw new ArgumentNullException(nameof(skill));

            if (!_skills.Add(skill))
                return;

            int index;
            if (guiSkillIndex.TryGetValue(skill, out index))
            {
                PrintWarning("Duplicate skill detected!! " + skill);
                _skills.Remove(skill);
                return;
            }

            var maxIndex = 0;
            var last = 0;
            if (guiSkillIndex.Count > 0)
            {
                foreach (var kvp in guiSkillIndex)
                {
                    var val = kvp.Value;
                    if (val > last) maxIndex = val;
                    last = kvp.Value;
                }
            }

            var useIndex = guiSkillIndex.Count > 0 ? (maxIndex + 1) : 0;
            AddSkillIndexAndSort(skill, useIndex);

            var skillList = Facepunch.Pool.GetList<string>();
            try
            {
                foreach (var s in _skills)
                    skillList.Add(s);

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || !p.IsConnected) continue;

                    DestroyGUI(p);

                    for (int j = 0; j < skillList.Count; j++)
                    {
                        var s = skillList[j];

                        var findInfo = FindInfoBySkill(p, s);
                        if (findInfo == null) continue;

                        GUIUpdateSkillFromInfo(p, findInfo);
                    }
                }
            }
            finally { Facepunch.Pool.FreeList(ref skillList); }


        }

        private void DestroyGUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            CuiHelper.DestroyUi(player, "LevelsGUI");

            _baseGUI.Remove(player.userID);
            _hasGuiIndex.Remove(player.userID);
        }

        private void CreateGUI(BasePlayer player, bool callHook = true, string anchorMax = "")
        {
            if (player == null || !player.IsConnected) return;
            var panelName = "LevelsGUI";

            var mainContainer = new CuiElementContainer()
            {
                {
                    new CuiPanel
                    {
                        Image = {Color = "0 0 0 0"},
                        RectTransform = {AnchorMin = "0.725 0.02", AnchorMax = string.IsNullOrEmpty(anchorMax) ? "0.83 0.1225" : anchorMax},
                        CursorEnabled = false
                    },
                    new CuiElement().Parent = "Under",
                    panelName
                }
            };

            CuiHelper.DestroyUi(player, panelName);
            CuiHelper.AddUi(player, mainContainer);
            _baseGUI.Add(player.userID);

            if (callHook) Interface.Oxide.CallHook("OnLevelsGUICreated", player);
        }

        private void CreateGUIFromCaches(BasePlayer player, bool forceDestroy = true)
        {
            if (player == null || !player.IsConnected) return;
            if (forceDestroy) DestroyGUI(player);
            if (!HasBaseGUI(player.userID) || forceDestroy) CreateGUI(player, false);


            List<LastGUIInfo> infos;
            if (_guiInfos.TryGetValue(player, out infos) && infos?.Count > 0)
            {
                for(int i = 0; i < infos.Count; i++) GUIUpdateSkillFromInfo(player, infos[i]);
            }
        }

        private bool HasBaseGUI(ulong userId)
        {
            return _baseGUI.Contains(userId);
        }


        private void GUIUpdateSkillFromInfo(BasePlayer player, LastGUIInfo info)
        {
            if (info == null) return;
            GUIUpdateSkill(player, info.Skill, info.Level, info.Percent, 0, info.SkillColor);
        }

        /*/
        private object CanUseUI(BasePlayer player, string json)
        {
            if (player == null || !player.IsAdmin) return null;

            PrintWarning(nameof(CanUseUI) + " for " + player + " json: " + Environment.NewLine + json);

            return null;

        }/*/

        //gui updating is broken idk why

        private readonly Dictionary<ulong, HashSet<int>> _hasGuiIndex = new Dictionary<ulong, HashSet<int>>();

        private void GUIUpdateSkill(BasePlayer player, string skill, long level, float percentToNextLvl, int addedXP = 0, string skillColor = "")
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            if (_guiOff.Contains(player.userID)) 
                return;

            if (!_skills.Contains(skill)) 
                AddSkill(skill);

            if (guiSkillIndex == null || guiSkillIndex.Count < 1)
            {
                PrintWarning("guiSkillIndex is null or count < 1");
                return;
            }

       

            var percent = ((int)percentToNextLvl).ToString();

            var maxRows = guiSkillIndex.Count;
            int rowNumber;
            if (!guiSkillIndex.TryGetValue(skill, out rowNumber))
            {
                PrintWarning("guiSkillIndex could not get value for skill: " + skill);
                return;
            }

            /*/
            HashSet<int> statHash;
            var hadGuiAlready = !player.IsAdmin ? false : (_hasGuiIndex.TryGetValue(player.userID, out statHash) && statHash.Contains(rowNumber)); //temp isadmin!!

            if (player.IsAdmin)
            {
                PrintWarning("had gui already: " + hadGuiAlready + " for: " + rowNumber + "( " + skill + ")");
            }/*/

            var hadGuiAlready = false; //it's broken as far as I can tell. :(
        

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                var mainPanel = sb.Clear().Append("LG").Append(skill).ToString();
                var value = 1 / (float)maxRows;
                var positionMin = 1 - (value * rowNumber);
                var positionMax = 2 - (1 - (value * (1 - rowNumber)));


                if (!HasBaseGUI(player.userID)) 
                    CreateGUI(player, true);


                var container = new CuiElementContainer()
            {
                {
                    new CuiPanel
                    {
                        Image = {Color = "0.1 0.1 0.1 0.1"},
                        RectTransform = { AnchorMin = sb.Clear().Append("0 ").Append(positionMin.ToString("0.00")).ToString(), AnchorMax = sb.Clear().Append("1 ").Append(positionMax.ToString("0.00")).ToString() },
                    },
                    new CuiElement().Parent = "LevelsGUI",
                    mainPanel
                }
            };

                var innerXPBar1 = new CuiElement
                {
                    Name = sb.Clear().Append("innerXpBar").Append(skill).ToString(),
                    Parent = mainPanel,
                    Components =
                        {
                            new CuiImageComponent { Color = "0.2 0.2 0.2 0.2" },
                            new CuiRectTransformComponent{ AnchorMin = "0.225 0.03", AnchorMax = "0.8 0.65" }
                        },
                };
                container.Add(innerXPBar1);

                var innerXPBarProgress1 = new CuiElement
                {
                    Name = sb.Clear().Append("innerXpBarProgress1").Append(skill).ToString(),
                    Parent = innerXPBar1.Name,
                    Components =
                        {
                            new CuiImageComponent() { Color = skillColor },
                            new CuiRectTransformComponent{ AnchorMin = "0 0", AnchorMax = sb.Clear().Append(percentToNextLvl / 100f).Append(" 0.95").ToString() }
                        },
                };
                container.Add(innerXPBarProgress1);



                var xpText = addedXP != 0 ? sb.Clear().Append(addedXP < 0 ? "-" : "+").Append(addedXP.ToString("N0")).Append(" XP!").ToString() : skill;


                var innerXPBarTextShadow1 = new CuiElement
                {
                    Name = sb.Clear().Append("innerXpBarTextShadow").Append(skill).ToString(),
                    Parent = innerXPBar1.Name,
                    Components =
                            {
                                new CuiTextComponent { Color = "0.1 0.1 0.1 0.75", Text = xpText, FontSize = 11, Align = TextAnchor.MiddleCenter},
                                new CuiRectTransformComponent{ AnchorMin = "0.035 -0.1", AnchorMax = "1 1" }
                            },
                    Update = hadGuiAlready
                };
                container.Add(innerXPBarTextShadow1);


                var innerXPBarText1 = new CuiElement
                {
                    Name = sb.Clear().Append("innerXpBarText").Append(skill).ToString(),
                    Parent = innerXPBar1.Name,
                    Components =
                        {
                            new CuiTextComponent { Color = "0.74 0.76 0.78 1", Text = xpText, FontSize = 11, Align = TextAnchor.MiddleCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0.05 0", AnchorMax = "1 1" }
                        },
                    Update = hadGuiAlready
                };
                container.Add(innerXPBarText1);

                var lvText = sb.Clear().Append("Lv.").Append(level).ToString();

                var lvShader1 = new CuiElement
                {
                    Name = sb.Clear().Append("lvShader").Append(skill).ToString(),
                    Parent = mainPanel,
                    Components =
                            {
                                new CuiTextComponent { Text = lvText, FontSize = (level > 99) ? 9 : 10, Align = TextAnchor.MiddleLeft, Color = "0.1 0.1 0.1 0.75" },
                                new CuiRectTransformComponent{ AnchorMin = "0.035 -0.2", AnchorMax = "0.5 1" }
                            },
                    Update = hadGuiAlready
                };
                container.Add(lvShader1);


                var lvText1 = new CuiElement
                {
                    Name = sb.Clear().Append("lvText").Append(skill).ToString(),
                    Parent = mainPanel,
                    Components =
                        {
                            new CuiTextComponent { Text = lvText, FontSize = (level > 99) ? 9 : 10 , Align = TextAnchor.MiddleLeft, Color = "0.74 0.76 0.78 1" },
                            new CuiRectTransformComponent{ AnchorMin = "0.025 -0.1", AnchorMax = "0.5 1" }
                        },
                    Update = hadGuiAlready
                };
                container.Add(lvText1);


                var percShader1 = new CuiElement
                {
                    Name = sb.Clear().Append("percShader").Append(skill).ToString(),
                    Parent = mainPanel,
                    Components =
                            {
                                new CuiTextComponent { Text = sb.Clear().Append(percent).Append("%").ToString(), FontSize = 10 , Align = TextAnchor.MiddleRight, Color = "0.1 0.1 0.1 0.75" },
                                new CuiRectTransformComponent{ AnchorMin = "0.5 -0.2", AnchorMax = "0.985 1" }
                            },
                     Update = hadGuiAlready
                };
                container.Add(percShader1);

                var percText1 = new CuiElement
                {
                    Name = sb.Clear().Append("percText").Append(skill).ToString(),
                    Parent = mainPanel,
                    Components =
                        {
                            new CuiTextComponent { Text = sb.Clear().Append(percent).Append("%").ToString(), FontSize = 10 , Align = TextAnchor.MiddleRight, Color = "0.74 0.76 0.78 1" },
                            new CuiRectTransformComponent{ AnchorMin = "0.5 -0.1", AnchorMax = "0.975 1" }
                        },
                    Update = hadGuiAlready
                };

                container.Add(percText1);

                if (!hadGuiAlready)
                    CuiHelper.DestroyUi(player, mainPanel);
                

             
                CuiHelper.AddUi(player, container);

               

                var info = FindInfoBySkill(player, skill);
                if (info == null)
                {
                    info = new LastGUIInfo();

                    List<LastGUIInfo> infos;
                    if (!_guiInfos.TryGetValue(player, out infos)) _guiInfos[player] = new List<LastGUIInfo>();

                    _guiInfos[player].Add(info);
                 
                }

                info.Skill = skill;
                info.SkillColor = skillColor;
                info.Level = level;
                info.Percent = percentToNextLvl;

                /*/
                if (!_hasGuiIndex.TryGetValue(player.userID, out statHash))
                    _hasGuiIndex[player.userID] = new HashSet<int>();

                _hasGuiIndex[player.userID].Add(rowNumber);/*/
           

            }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        #endregion
        #region Utility
        private bool HasGUIStr(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            ulong uID;
            if (ulong.TryParse(userID, out uID)) return HasGUI(uID);
            else return false;
        }

        private bool HasGUI(ulong userId)
        {
            return !_guiOff.Contains(userId);
        }

        private void SendReply(IPlayer player, string msg, string userId = "0", bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : keepTagsConsole ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else
            {
#if RUST
                player.Command("chat.add", string.Empty, userId, msg);
#else
                player.Reply(msg);
#endif
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
            for (int i = 0; i < forbiddenTags.Count; i++) phraseSB.Replace(forbiddenTags[i], string.Empty);


            return phraseSB.ToString();
        }
        #endregion
        #region Saving
        private void SaveGUIData() => Interface.Oxide.DataFileSystem.WriteObject("LevelsGUI_Disabled", _guiOff);

        private void OnServerInitialized()
        {
            //perf stuff, don't ask why it's so weird
            var tempList = Pool.GetList<ulong>();

            try 
            {
                tempList = Interface.Oxide?.DataFileSystem?.ReadObject<List<ulong>>("LevelsGUI_Disabled") ?? new List<ulong>();

                for (int i = 0; i < tempList.Count; i++)
                    _guiOff.Add(tempList[i]);

            }
            finally { Pool.FreeList(ref tempList); }


            ServerMgr.Instance.Invoke(() =>
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || _guiOff.Contains(player.userID)) continue;
                    CreateGUI(player);
                }
            }, 0.25f); //use delay, because some wonky stuff happens if you don't. don't know why. maybe hooks are called too soon
        }

        private void cmdStatsUI(IPlayer player, string command, string[] args)
        {
            if (player == null) return;
            var ply = player?.Object as BasePlayer;
            if (player.IsServer || ply == null)
            {
                SendReply(player, "This can only be ran as a player!");
                return;
            }
            ulong userId;
            if (!ulong.TryParse(player.Id, out userId))
            {
                PrintWarning("Invalid ID: " + player?.Id + " -- not a ulong!");
                SendReply(player, "Invalid ID!");
                return;
            }
            if (_guiOff.Contains(userId))
            {
                _guiOff.Remove(userId);
                CreateGUI(ply);
            }
            else
            {
                DestroyGUI(ply);
                _guiOff.Add(userId);

            }
        }

        private void cmdSkillTest(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsAdmin) return;
            if (args == null || args.Length < 2) return;
            if (args[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                RemoveSkill(args[1]);
                SendReply(player, "Called RemoveSkill for: " + args[1]);
            }
            else if (args[0].Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                AddSkill(args[1]);
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p != null && p.IsConnected)
                    {
                        GUIUpdateSkill(p, args[1], 1, 0);
                    }
                }
                SendReply(player, "Called AddSkill for: " + args[1]);
            }
        }

        private void Init()
        {
            string[] statsUICmds = { "statsui", "uistats", "statsgui", "guistats", "luckui", "luckgui" };
            AddCovalenceCommand(statsUICmds, nameof(cmdStatsUI));
            AddCovalenceCommand("sktest", "cmdSkillTest");
        }

        private void Unload()
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || _guiOff.Contains(player.userID)) 
                    continue;

                DestroyGUI(player);

            }

            SaveGUIData();
        }
        #endregion
    }
}
