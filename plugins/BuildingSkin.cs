using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Building Skin", "Shady", "0.0.5")]
    public class BuildingSkin : RustPlugin
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
        private const string CHAT_STEAM_ID = "76561198865536053";

        private readonly HashSet<string> _forbiddenTags = new(6) { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private readonly HashSet<string> _adobeEnabled = new();

        private readonly HashSet<string> _shippingEnabled = new();

        private readonly HashSet<BuildingGrade> _buildingGrades = new();

        private readonly HashSet<Coroutine> _coroutines = new();

        private class PlayerGradeSkin
        {
            public readonly Dictionary<BuildingGrade.Enum, ulong> GradeShouldUseSkin = new();
        }

        private readonly Dictionary<string, PlayerGradeSkin> _playerGradeSkin = new();

        private const ulong ADOBE_SKIN_ID = 10220;

        private const ulong SHIPPING_CONTAINER_SKIN_ID = 10221;

        private const string ADOBE_COLOR = "#C99D75";
        private const string STONE_COLOR = "#B8AEAC";
        private const string BRICK_COLOR = "#A88273";

        private const string METAL_COLOR = "#C7A585";

        private const string SHIPPING_COLOR = "#3B3645";


        private const int HAMMER_ITEM_ID = 200773292;

        #region Hooks
        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item item)
        {
            if ((oldItem?.info?.itemid ?? 0) == HAMMER_ITEM_ID)
                StopEGUI(player);
        }

        private void GUIBuildingEButton(BasePlayer player)
        {
            if (player == null || !player.IsConnected)
                return;

            var imageAnchorMin = "0.489 0.363";
            var imageAnchorMax = "0.509 0.398";

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = false
            }, "Overlay", "BGUI_E");

            GUISkinElement.Add(new CuiElement
            {
                Name = "ImgUIImg",
                Components =
                {
                    new CuiRawImageComponent
                    {
                        Url = @"https://cdn.prismrust.com/arrow_right.jpg",
                        FadeIn = 0.0f
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = imageAnchorMin,
                        AnchorMax = imageAnchorMax
                    }
                },
                Parent = GUISkin,

            });


            CuiHelper.DestroyUi(player, "BGUI_E");


            CuiHelper.AddUi(player, GUISkinElement);
        }

        private void StopEGUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            CuiHelper.DestroyUi(player, "BGUI_E");
            _hasGui.Remove(player.userID);
        }

        IEnumerator StopGUIWhenNoLongerLooking(BasePlayer player)
        {
            if (player == null || !player.IsConnected)
                yield break;

            PrintWarning(nameof(StopGUIWhenNoLongerLooking));

            var safety = 0;

            if (!_startGuiViewingAngles.TryGetValue(player, out Vector3 viewAngles))
            {
                PrintWarning("couldn't get view angles!!!");
                yield break;
            }

            while (player != null && player.IsConnected && player.IsAlive() && _hasGui.Contains(player.userID) && (player?.eyes?.rotation.eulerAngles ?? Vector3.zero) == viewAngles)
            {
                if (safety >= 500)
                    yield break;

                safety++;

                yield return CoroutineEx.waitForSecondsRealtime(0.025f);
            }

            StopEGUI(player);

            _guiCheckRoutine.Remove(player);
        }

        private readonly Dictionary<BasePlayer, Coroutine> _guiCheckRoutine = new();
        private readonly HashSet<ulong> _hasGui = new();

        private readonly Dictionary<BasePlayer, Vector3> _startGuiViewingAngles = new();

        private void OnPlayerInput(BasePlayer player, InputState input)
        {

            if (player == null || input == null)
                return;

            if (!player.IsAdmin)
                return; //temp!


            /*/if (player.IsAdmin && input.IsDown(BUTTON.USE) || input.WasJustPressed(BUTTON.USE) || input.WasJustReleased(BUTTON.USE))
                PrintWarning("use down/pressed/rel");
            /*/

            var item = player?.GetActiveItem();

            if (item == null || item.info.itemid != HAMMER_ITEM_ID) //check for gmod tool too later on
                return;

        

            if  (_hasGui.Contains(player.userID))
                return;

            if (input.WasJustReleased(BUTTON.FIRE_SECONDARY) && input.WasDown(BUTTON.FIRE_SECONDARY))
            {

                player.Invoke(() =>
                {
                    var preAngles = player?.eyes?.rotation.eulerAngles ?? Vector3.zero;

                    player?.Invoke(() =>
                    {
                        var newAngles = player?.eyes?.rotation.eulerAngles ?? Vector3.zero;

                        if (newAngles != preAngles && Vector3.Distance(newAngles, preAngles) > 0.125f)
                            return;


                        _startGuiViewingAngles[player] = newAngles;

                        GUIBuildingEButton(player);
                        _hasGui.Add(player.userID);

                        _guiCheckRoutine.Remove(player);

                        _guiCheckRoutine[player] = StartCoroutine(StopGUIWhenNoLongerLooking(player));                  


                    }, 0.05f);

                }, 0.05f);
            }

            //turns out when you hold right click with hammer on a wall, you immediately "release" it as far as server input is concerned. this is part of how we can confirm they're upgrading.

            
        }

        private void Init()
        {
            var bSkinAliases = new string[] { "bskin", "bs", "adobe", "adb", "sc" };

            AddCovalenceCommand(bSkinAliases, nameof(cmdBuildingSkin));

        }

        private void Unload()
        {
            if (ServerMgr.Instance != null)
            {
                foreach (var routine in _coroutines)
                {
                    try { ServerMgr.Instance.StopCoroutine(routine); } //DO NOT use StopCoroutine() (in this plugin) here.
                    catch (Exception ex) { PrintError(ex.ToString()); }
                }
            }
           
        }

        private void OnServerInitialized()
        {
            Puts(nameof(OnServerInitialized) + " setting up " + nameof(_buildingGrades));

            var findAll = PrefabAttribute.server.FindAll<ConstructionGrade>(2194854973U);

            //fp code:
            // var source = ((IEnumerable<ConstructionGrade>).Select<ConstructionGrade, BuildingGrade>((Func<ConstructionGrade, BuildingGrade>)(x => x.gradeBase));

            for (int i = 0; i < findAll.Length; i++)
                _buildingGrades.Add(findAll[i].gradeBase);
            

            Puts("Finished setting up " + nameof(_buildingGrades));
        }



        private void OnStructureUpgrade(BuildingBlock block, BasePlayer player, BuildingGrade.Enum grade, ulong skin)
        {
            if (block == null || player == null || block.grade == grade || skin != 0) //is upgrading to, for example, stone, from stone, which means they want to un-skin it.
                return;

            if (!_playerGradeSkin.TryGetValue(player.UserIDString, out PlayerGradeSkin pg))
                return;

            if (!pg.GradeShouldUseSkin.TryGetValue(grade, out ulong desiredSkin))
                return;

            if (desiredSkin == block.skinID)
                return;

            block.Invoke(() =>
            {
                if (block == null || block.IsDestroyed || block.grade != grade || block.skinID == desiredSkin)
                    return;

                block.ClientRPC(null, "DoUpgradeEffect", (int)grade, desiredSkin);

                block.playerCustomColourToApply = player.LastBlockColourChangeId;
                block.ChangeGradeAndSkin(grade, desiredSkin, true);

            }, 0.01f);
        }

        #endregion

        private ulong GetBuildingSkinIdByName(string skinNameFullOrPartial, BuildingGrade.Enum requiredGrade = BuildingGrade.Enum.None)
        {
            if (string.IsNullOrWhiteSpace(skinNameFullOrPartial))
                throw new ArgumentNullException(nameof(skinNameFullOrPartial));

            foreach (var grade in _buildingGrades)
            {
                if (grade.name.IndexOf(skinNameFullOrPartial, StringComparison.OrdinalIgnoreCase) >= 0 && (requiredGrade == BuildingGrade.Enum.None || grade.type == requiredGrade))
                    return grade.skin;
            }

            return 0;
        }

        private BuildingGrade GetBuildingGradeByName(string skinNameFullOrPartial, BuildingGrade.Enum requiredGrade = BuildingGrade.Enum.None)
        {
            if (string.IsNullOrWhiteSpace(skinNameFullOrPartial))
                throw new ArgumentNullException(nameof(skinNameFullOrPartial));

            foreach (var grade in _buildingGrades)
            {
                if (grade.name.IndexOf(skinNameFullOrPartial, StringComparison.OrdinalIgnoreCase) >= 0 && (requiredGrade == BuildingGrade.Enum.None || grade.type == requiredGrade))
                    return grade;
            }

            return null;
        }

        #region Commands

        private string GetOpeningColorTag(string hexColor)
        {
            var sb = Pool.Get<StringBuilder>();

            try { return sb.Clear().Append("<color=").Append(hexColor).Append(">").ToString(); }
            finally { Pool.Free(ref sb); }
        }

        private void cmdBuildingSkin(IPlayer player, string command, string[] args)
        {
            var sb = Pool.Get<StringBuilder>();

            try
            {
                sb.Clear();

                if (args.Length < 1)
                {

                    var brickColor = GetOpeningColorTag(BRICK_COLOR);

                    var adobeColor = GetOpeningColorTag(ADOBE_COLOR);

                    var stoneColor = GetOpeningColorTag(STONE_COLOR);

                    var cmdColor = GetOpeningColorTag("#9E88D1");

                    var coloredCmdTxt = sb.Clear().Append("/").Append(cmdColor).Append(command).Append("</color>").ToString();

                    var msg = sb.Clear().Append("Use ").Append(coloredCmdTxt).Append(" like this, for example:\n").Append(coloredCmdTxt).Append(brickColor).Append(" Brick</color>\nOR:\n").Append(coloredCmdTxt).Append(adobeColor).Append(" Adobe</color>\nTo disable, type the regular grade name, for example, ").Append(coloredCmdTxt).Append(stoneColor).Append(" Stone</color>\nFor a list of skin options, type ").Append(coloredCmdTxt).Append(" list").ToString();

                    SendReply(player, msg);
                    return;
                }

                if (args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
                {



                    var isAdmin = player.IsAdmin;

                    sb.Clear().Append("List of applicable skins:").Append(Environment.NewLine);

                    foreach (var grade in _buildingGrades)
                    {
                        if (!isAdmin && grade.skin > 10230)
                            continue; //temp brutalist thing; broken skin ID.

                        //this sb is a work of art and pure divine inspiration
                        sb.Append(FirstUpper(grade.name)).Append(" (").Append(grade.type).Append(")").Append(!isAdmin ? string.Empty : " ({skinId})").Replace("{skinId}", grade.skin.ToString()).Append(Environment.NewLine);
                    }

                    if (sb.Length > 1)
                        sb.Length--; //trim last newline

                    SendReply(player, sb.ToString());


                    return;
                }

                var joinStr = string.Join(" ", args);
                var desiredSkin = GetBuildingGradeByName(joinStr);

                if (desiredSkin == null)
                {
                    SendReply(player, "No skin found by name: " + desiredSkin);
                    return;
                }

                SetDesiredGradeSkin(player.Id, desiredSkin.type, desiredSkin.skin);

                //use dictionary for grades.
                SendReply(player, "Now using skin: " + FirstUpper(desiredSkin.name) + ". This skin will be used only for its applicable grade " + desiredSkin.type + ".");
            }
            finally { Pool.Free(ref sb); }
        }

        private void SetDesiredGradeSkin(string userId, BuildingGrade.Enum grade, ulong desiredSkin)
        {
            if (!_playerGradeSkin.TryGetValue(userId, out PlayerGradeSkin pg))
                _playerGradeSkin[userId] = pg = new PlayerGradeSkin();

            pg.GradeShouldUseSkin[grade] = desiredSkin;
        }

        private ulong GetDesiredGradeSkin(string userId, BuildingGrade.Enum grade)
        {
            if (!_playerGradeSkin.TryGetValue(userId, out PlayerGradeSkin pg) || !pg.GradeShouldUseSkin.TryGetValue(grade, out ulong skin))
                return 0;

            return skin;
        }

        #endregion

        #region Util
        private bool IsDownOrJustPressedAnyMovementKey(InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return input.IsDown(BUTTON.FORWARD) || input.IsDown(BUTTON.BACKWARD) || input.IsDown(BUTTON.LEFT) || input.IsDown(BUTTON.RIGHT) || input.IsDown(BUTTON.JUMP) || input.WasJustPressed(BUTTON.FORWARD) || input.WasJustPressed(BUTTON.BACKWARD) || input.WasJustPressed(BUTTON.LEFT) || input.WasJustPressed(BUTTON.RIGHT) || input.WasJustPressed(BUTTON.JUMP);
        }

        private Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var coroutine = ServerMgr.Instance.StartCoroutine(routine);

            _coroutines.Add(coroutine);

            return coroutine;
        }

        private void StopCoroutine(Coroutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            ServerMgr.Instance.StopCoroutine(routine);

            _coroutines.Remove(routine);
        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, float radius = 0f, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;

            RaycastHit hit;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, player.eyes.rotation * Vector3.forward);

            if (radius <= 0)
            {
                if (Physics.Raycast(ray, out hit, maxDist, coll))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }
            else
            {
                if (Physics.SphereCast(ray, radius, out hit, maxDist, coll))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }


            return null;
        }

        private string FirstUpper(string original)
        {
            if (string.IsNullOrWhiteSpace(original))
                throw new ArgumentNullException(nameof(original));

            var array = original.ToCharArray();
            array[0] = char.ToUpper(array[0], CultureInfo.CurrentCulture);
            return new string(array);
        }

        private void SendReply(IPlayer player, string msg, string userId = CHAT_STEAM_ID, bool keepTagsConsole = false)
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

        private void SendReply(BasePlayer player, string msg, string userId = CHAT_STEAM_ID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;


            //	Replace Color Tags
            phrase = _colorRegex.Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = _sizeRegex.Replace(phrase, string.Empty);

            var phraseSB = Pool.Get<StringBuilder>();
            try
            {
                phraseSB.Clear().Append(phrase);

                foreach (var tag in _forbiddenTags)
                    phraseSB.Replace(tag, string.Empty);

                return phraseSB.ToString();
            }
            finally { Pool.Free(ref phraseSB); }
        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;

            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, maxDist, coll))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }

            if (coll == -1)
            {
                if (UnityEngine.Physics.Raycast(ray, out hit, maxDist, Rust.Layers.Construction))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }

            return null;
        }

        #endregion

    }
}
