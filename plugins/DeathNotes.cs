using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using Newtonsoft.Json;
using System.Linq;
using Oxide.Core;
using System;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Death Notes", "LaserHydra (largely edited by Shady)", "5.2.98", ResourceId = 819)]
    [Description("Broadcast deaths with many details")]
    internal class DeathNotes : RustPlugin
    {
        #region Global Declaration

        private readonly bool debug = false;
        private readonly bool killReproducing = false;

        [PluginReference]
        private readonly Plugin Clans;

        [PluginReference]
        private readonly Plugin StreamerAPI;

        [PluginReference]
        private readonly Plugin SkinsAPI;

        /*/
        /// <summary>
        /// Returns a player's display name. If a target player is supplied, it will check if that target player has streamer mode. If the player does, it will return the streamer-friendly name of the source player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetDisplayName(BasePlayer player, BasePlayer target = null)
        {
            if (player == null) return string.Empty;
            if (dn?.StreamerAPI == null || !(dn?.StreamerAPI?.IsLoaded ?? false)) return player.displayName;
            if (target != null && target.IsConnected)
            {
                var isStreamer = dn?.StreamerAPI?.Call<bool>("IsStreamerMode", target) ?? false;
                if (isStreamer) return dn?.StreamerAPI?.Call<string>("Get", player.userID) ?? player.displayName;
            }
            return player.displayName;
        }/*/

        private readonly Dictionary<ulong, HitInfo> LastWounded = new Dictionary<ulong, HitInfo>();
        private Dictionary<string, string> reproduceableKills = new Dictionary<string, string>();
        private readonly Dictionary<BasePlayer, Timer> timers = new Dictionary<BasePlayer, Timer>();
        private Dictionary<ulong, PlayerSettings> playerSettings = new Dictionary<ulong, PlayerSettings>();
        private Plugin PopupNotifications;
        private static DeathNotes dn;

        #region Cached Variables

        private readonly UIColor deathNoticeShadowColor = new UIColor(0.1, 0.1, 0.1, 0.8);
        private readonly UIColor deathNoticeColor = new UIColor(0.85, 0.85, 0.85, 0.1);
        private readonly List<string> selfInflictedDeaths = new List<string> { "Cold", "Drowned", "Heat", "Suicide", "Generic", "Posion", "Radiation", "Thirst", "Hunger", "Fall" };
        private readonly List<DeathReason> SleepingDeaths = new List<DeathReason>
        {
            DeathReason.Animal,
            DeathReason.Blunt,
            DeathReason.Bullet,
            DeathReason.Explosion,
            DeathReason.Generic,
            DeathReason.Helicopter,
            DeathReason.Slash,
            DeathReason.Stab,
            DeathReason.Unknown
        };
        private readonly List<Regex> regexTags = new List<Regex>
        {
            new Regex(@"<color=.+?>", RegexOptions.Compiled),
            new Regex(@"<size=.+?>", RegexOptions.Compiled)
        };
        private readonly List<string> tags = new List<string>
        {
            "</color>",
            "</size>",
            "<i>",
            "</i>",
            "<b>",
            "</b>"
        };

        // ------------------->  Config Values

        // ------->   General

        //  Needs Permission to see messages?
        private bool NeedsPermission;

        //  Chat Icon (Steam Profile - SteamID)
        private string ChatIcon;

        //  Message Radius
        private bool MessageRadiusEnabled;
        private float MessageRadius;

        //  Where Should the message appear?
        private bool DoLogToFile;
        private bool WriteToConsole;
        private bool WriteToChat;
        private bool UsePopupNotifications;
        private bool UseSimpleUI;

        //  Attachments
        private string AttachmentSplit;
        private string AttachmentFormatting;

        //  Other
        private string ChatTitle;
        private string ChatFormatting;
        private string ConsoleFormatting;

        // ------->   Colors

        private string TitleColor;
        private string VictimColor;
        private string AttackerColor;
        private string WeaponColor;
        private string AttachmentColor;
        private string DistanceColor;
        private string BodypartColor;
        private string MessageColor;
        private readonly string HealthColor;

        // ------->   Localization

        private Dictionary<string, object> Names;
        private Dictionary<string, object> Bodyparts;
        private Dictionary<string, object> Weapons;
        private Dictionary<string, object> Attachments;

        // ------->   Messages

        private Dictionary<string, List<string>> Messages;

        // ------->   Simple UI

        //  Other
        private bool SimpleUI_StripColors;

        //  Scaling & Positioning
        private int SimpleUI_FontSize;
        private float SimpleUI_Top;
        private float SimpleUI_Left;
        private float SimpleUI_MaxWidth;
        private float SimpleUI_MaxHeight;

        //  Timer
        private float SimpleUI_HideTimer;

        // ----------------------------------------------------

        #endregion

        #endregion

        #region Classes

        private class UIColor
        {
            private readonly string color;

            public UIColor(double red, double green, double blue, double alpha)
            {
                color = $"{red} {green} {blue} {alpha}";
            }

            public override string ToString() => color;
        }

        private class UIObject
        {
            private readonly List<object> ui = new List<object>();
            private readonly List<string> objectList = new List<string>();

            public UIObject()
            {
            }

            private string RandomString()
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                List<char> charList = chars.ToList();

                string random = "";

                for (int i = 0; i <= UnityEngine.Random.Range(5, 10); i++)
                    random += charList[UnityEngine.Random.Range(0, charList.Count - 1)];

                return random;
            }

            public void Draw(BasePlayer player)
            {
             //   CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connection = player.net.connection }, null, "AddUI", new Facepunch.ObjectList(JsonConvert.SerializeObject(ui).Replace("{NEWLINE}", Environment.NewLine)));
            }

            public void Destroy(BasePlayer player)
            {
                for(int i = 0; i < objectList.Count; i++)
                {
                    var uiName = objectList[i];
                //    CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connection = player.net.connection }, null, "DestroyUI", new Facepunch.ObjectList(uiName));
                }
                    
            }

            public string AddText(string name, double left, double top, double width, double height, UIColor color, string text, int textsize = 15, string parent = "Hud.Under", int alignmode = 0, float fadeIn = 0f, float fadeOut = 0f)
            {
                //name = name + RandomString();
                text = text.Replace("\n", "{NEWLINE}");
                string align = "";

                switch (alignmode)
                {
                    case 0: { align = "LowerCenter"; break; };
                    case 1: { align = "LowerLeft"; break; };
                    case 2: { align = "LowerRight"; break; };
                    case 3: { align = "MiddleCenter"; break; };
                    case 4: { align = "MiddleLeft"; break; };
                    case 5: { align = "MiddleRight"; break; };
                    case 6: { align = "UpperCenter"; break; };
                    case 7: { align = "UpperLeft"; break; };
                    case 8: { align = "UpperRight"; break; };
                }

                ui.Add(new Dictionary<string, object> {
                    {"name", name},
                    {"parent", parent},
                    {"fadeOut", fadeOut.ToString()},
                    {"components",
                        new List<object> {
                            new Dictionary<string, string> {
                                {"type", "UnityEngine.UI.Text"},
                                {"text", text},
                                {"fontSize", textsize.ToString()},
                                {"color", color.ToString()},
                                {"align", align},
                                {"fadeIn", fadeIn.ToString()}
                            },
                            new Dictionary<string, string> {
                                {"type", "RectTransform"},
                                {"anchormin", $"{left} {((1 - top) - height)}"},
                                {"anchormax", $"{(left + width)} {(1 - top)}"}
                            }
                        }
                    }
                });

                objectList.Add(name);
                return name;
            }
        }

        private class PlayerSettings
        {
            public bool ui = false;
            public bool chat = true;
            public float MaxDistance = 0f;

            public PlayerSettings()
            {
            }

            internal PlayerSettings(DeathNotes deathnotes)
            {
                ui = dn.UseSimpleUI;
                chat = dn.WriteToChat;
            }
        }

        private class Attacker
        {
            public string name = string.Empty;
            [JsonIgnore]
            public BaseEntity entity;
       //     public BaseCombatEntity entity;
            public AttackerType type = AttackerType.Invalid;
            public float healthLeft;

            public string TryGetName()
            {
                try
                {
                    if (entity == null)
                    {
                        //Interface.Oxide.LogWarning("Entity was null on Attacker.TryGetName()");
                        return "Unknown Attacker";
                    }
                    var entName = entity?.name ?? string.Empty;

                    if (type == AttackerType.Player)
                    {
                        if (entName.Contains("murder")) return "Murderer";
                        if (entName.Contains("scare")) return "Scarecrow";

                        var atkObj = entity?.ToPlayer() ?? null;
                        if (atkObj == null)
                        {
                            var atkParent = entity?.GetParentEntity() as BasePlayer;
                            if (atkParent != null) atkObj = atkParent;
                            else return "Unknown Attacker";
                        }

                        if (atkObj.ShortPrefabName.Contains("scientist"))
                        {
                            if (atkObj.ShortPrefabName.Contains("heavy"))
                            {
                                return "Heavy Scientist";
                            }
                            return "Scientist";
                        }

                        if (atkObj.ShortPrefabName.Contains("bandit"))
                        {
                            return "Bandit";
                        }

                        if (atkObj is NPCPlayer)
                        {
                            Interface.Oxide.LogWarning("atkObj is NPC but not scientist or bandit: " + atkObj?.ShortPrefabName);
                            return string.Empty;
                        }

                        var displayName = atkObj?.displayName ?? string.Empty;
                        if (string.IsNullOrEmpty(displayName)) return dn.FirstUpper(atkObj.ShortPrefabName);
                        
                        var clanTag = dn?.Clans?.Call<string>("GetClanOf", atkObj.UserIDString) ?? string.Empty;
                        if (!string.IsNullOrEmpty(clanTag)) displayName = "[" + clanTag + "] " + displayName;
                        return displayName;
                    }

                    if (type == AttackerType.Helicopter)
                        return "Patrol Helicopter";
                    if (type == AttackerType.Turret) return entName.Contains("guntrap") ? "Shotgun Trap" : entName.Contains("sentry") ? "Sentry Turret" : "Auto Turret";
                    if (type == AttackerType.APC || entName.Contains("bradley")) return "Bradley";
                    if (type == AttackerType.CargoShip || entName.Contains("cargo")) return "Cargo Ship";
                    if (type == AttackerType.SamSite || entName.Contains("sam_site")) return "Sam Site";
                    if (type == AttackerType.Minicopter || entName.Contains("Minicopter")) return "Minicopter";
                    if (type == AttackerType.ScrapHeli || entName.Contains("scraptrans")) return "Scrap Transport Helicopter";
                    if (type == AttackerType.TeslaCoil || entName.Contains("tesla")) return "Tesla Coil";
                    if (type == AttackerType.Self)
                        return "himself";
                    if (type == AttackerType.Animal)
                    {
                        if (entName.Contains("boar"))
                            return "Boar";
                        if (entName.Contains("horse"))
                            return "Horse";
                        if (entName.Contains("wolf"))
                            return "Wolf";
                        if (entName.Contains("stag"))
                            return "Stag";
                        if (entName.Contains("chicken"))
                            return "Chicken";
                        if (entName.Contains("bear"))
                            return "Bear";
                        if (entName.Contains("shark"))
                            return "Shark";
                    }
                    else if (type == AttackerType.Structure)
                    {
                        if (entName.Contains("barricade.wood"))
                            return "Wooden Barricade";
                        if (entName.Contains("barricade.woodwire"))
                            return "Barbed Wooden Barricade";
                        if (entName.Contains("barricade.metal"))
                            return "Metal Barricade";
                        if (entName.Contains("wall.external.high.wood"))
                            return "High External Wooden Wall";
                        if (entName.Contains("wall.external.high.stone"))
                            return "High External Stone Wall";
                        if (entName.Contains("gates.external.high.wood"))
                            return "High External Wooden Gate";
                        if (entName.Contains("gates.external.high.wood"))
                            return "High External Stone Gate";
                    }
                    else if (type == AttackerType.Trap)
                    {
                        if (entName.Contains("beartrap"))
                            return "Snap Trap";
                        if (entName.Contains("landmine"))
                            return "Land Mine";
                        if (entName.Contains("spikes.floor"))
                            return "Wooden Floor Spikes";
                        if (entName.Contains("spikes_static"))
                            return "Cave Spikes";
                        if (entName.Contains("cactus"))
                            return "Cactus";
                    }
                    var getName = dn?.GetItemDefFromPrefabName((entity?.ShortPrefabName ?? string.Empty))?.displayName?.english ?? string.Empty;
                    if (!string.IsNullOrEmpty(getName)) return getName;
                    
                    dn?.PrintWarning("Returning unknown attacker (" + entName + ", " + type + ")");
                    return "Unknown Attacker";
                }
                catch(Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^Failed AttackerType.TryGetName()^");
                    return "Unknown Attacker";
                }
            }

            public AttackerType TryGetType()
            {
                if (entity == null) return AttackerType.Invalid;
                try
                {
                    var entName = entity?.name ?? string.Empty;
                    if (entity is BasePlayer)
                        return AttackerType.Player;
                    if (entity is PatrolHelicopter || entName.Contains("patrolheli"))
                        return AttackerType.Helicopter;
                    if (entity is BradleyAPC)
                        return AttackerType.APC;
                    if (entity is CargoShip)
                        return AttackerType.CargoShip;
                    if (entity is SamSite)
                        return AttackerType.SamSite;
                    if (entity is Minicopter) return AttackerType.Minicopter;
                    if (entity is ScrapTransportHelicopter) return AttackerType.ScrapHeli;
                    if (entity is TeslaCoil) return AttackerType.TeslaCoil;
                    if (entity is SimpleShark || entName.Contains("ai/") || entity is BaseNpc)
                        return AttackerType.Animal;
                    if (entName.Contains("barricades/") || entName.Contains("wall.external.high") || (entity is SimpleBuildingBlock || entity is DecorDeployable || entity is BaseOven))
                        return AttackerType.Structure;
                    if (entName.Contains("beartrap") || entName.Contains("landmine") || entName.Contains("spikes") || entName.Contains("cactus"))
                        return AttackerType.Trap;
                    if (entName.Contains("machete"))
                        return AttackerType.Player;
                    if (entName.Contains("autoturret") || entName.Contains("guntrap") || entName.Contains("sentry"))
                        return AttackerType.Turret;
                }
                catch (Exception ex) { Interface.Oxide.LogWarning(ex.ToString()); }
                return AttackerType.Invalid;
            }
        }

        private class Victim
        {
            public string name = string.Empty;
            [JsonIgnore]
            public BaseCombatEntity entity;
            public bool sleeping;
            public VictimType type = VictimType.Invalid;

            public string TryGetName()
            {
                if (entity == null)
                {
                    Interface.Oxide.LogError("Entity was null on Victim.TryGetName()");
                    return "Unknown Victim";
                }
                try
                {
                    if (entity.ShortPrefabName.Contains("scientist") || entity.ShortPrefabName.Contains("bandit")) return string.Empty;

                    if (type == VictimType.Player)
                    {
                        if (entity.ShortPrefabName != "player") return string.Empty;
                        var atkObj = entity?.ToPlayer() ?? null;
                        if (atkObj == null) return "Unknown Victim";
                        var displayName = entity?.ToPlayer()?.displayName ?? string.Empty;
                        if (string.IsNullOrEmpty(displayName)) return displayName;
                        if ((atkObj?.net?.connection == null || !atkObj.IsConnected) && !atkObj.IsSleeping()) return string.Empty;
                        var clanTag = dn?.Clans?.Call<string>("GetClanOf", atkObj.UserIDString) ?? string.Empty;
                        if (!string.IsNullOrEmpty(clanTag)) displayName = "[" + clanTag + "] " + displayName;
                        return displayName;
                    }
                

                    if (type == VictimType.Helicopter)
                        return "Patrol Helicopter";
                    if (type == VictimType.APC) return "Bradley";
                    if (type == VictimType.Animal)
                    {
                        if (entity.name.Contains("boar"))
                            return "Boar";
                        if (entity.name.Contains("horse"))
                            return "Horse";
                        if (entity.name.Contains("wolf"))
                            return "Wolf";
                        if (entity.name.Contains("stag"))
                            return "Stag";
                        if (entity.name.Contains("chicken"))
                            return "Chicken";
                        if (entity.name.Contains("bear"))
                            return "Bear";
                    }
                }
                catch(Exception ex) { Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^Failed to complete VictimType.TryGetName()^"); }
                return "Unknown Victim";
            }

            public VictimType TryGetType()
            {
                if (entity == null) return VictimType.Invalid;
                try
                {
                    if (entity is BasePlayer)
                        return VictimType.Player;
                    if (entity is PatrolHelicopter)
                        return VictimType.Helicopter;
                    if (entity is BradleyAPC)
                        return VictimType.APC;
                    if ((bool)entity?.name?.Contains("ai/") || entity is BaseNpc)
                        return VictimType.Animal;
                }
                catch(Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                return VictimType.Invalid;
            }
        }

        private class DeathData
        {
            public Victim victim = new Victim();
            public Attacker attacker = new Attacker();
            public DeathReason reason = DeathReason.Unknown;
            public string damageType = string.Empty;
            public string weapon = string.Empty;
            public string customWeaponName = string.Empty;
            public List<string> attachments = new List<string>();
            public string bodypart = string.Empty;
            public Rust.DamageType dmgType = Rust.DamageType.Generic;
            internal float _distance = -1f;

            public float distance
            {
                get
                {
                    if (_distance != -1) return _distance;
                    if (dn?.selfInflictedDeaths != null && dn.selfInflictedDeaths.Count > 0)
                    {
                        for (int i = 0; i < dn.selfInflictedDeaths.Count; i++)
                        {
                            var death = dn.selfInflictedDeaths[i];
                            if (death == null) continue;
                            if (reason == GetDeathReason(death)) attacker.entity = victim.entity;
                        }
                    }
                    if (attacker?.entity == null || attacker.entity.IsDestroyed || attacker.entity.gameObject == null || victim?.entity == null || victim.entity.IsDestroyed || victim.entity.gameObject == null) return -1f;
                    return Vector3.Distance(attacker?.entity?.transform?.position ?? Vector3.zero, victim?.entity?.transform?.position ?? Vector3.zero);
                }
            }

            public DeathReason TryGetReason()
            {
                if (victim.type == VictimType.Helicopter)
                    return DeathReason.HelicopterDeath;
                else if (attacker.type == AttackerType.Helicopter)
                    return DeathReason.Helicopter;
                else if (attacker.type == AttackerType.APC)
                    return DeathReason.APC;
                else if (attacker.type == AttackerType.CargoShip)
                    return DeathReason.CargoShip;
                else if (attacker.type == AttackerType.SamSite)
                    return DeathReason.SamSite;
                else if (victim.type == VictimType.APC)
                    return DeathReason.APCDeath;
                else if (attacker.type == AttackerType.Turret)
                    return DeathReason.Turret;
                else if (attacker.type == AttackerType.Trap)
                    return DeathReason.Trap;
                else if (attacker.type == AttackerType.Structure)
                    return DeathReason.Structure;
                else if (attacker.type == AttackerType.Animal)
                    return DeathReason.Animal;
                else if (victim.type == VictimType.Animal)
                    return DeathReason.AnimalDeath;
                else if (weapon == "F1 Grenade" || weapon == "Survey Charge")
                    return DeathReason.Explosion;
                else if (weapon == "Flamethrower")
                    return DeathReason.Flamethrower;
                else if (victim.type == VictimType.Player)
                    return GetDeathReason(damageType);

                return DeathReason.Unknown;
            }

            public DeathReason GetDeathReason(string damage)
            {
                if (string.IsNullOrEmpty(damage)) return DeathReason.Unknown;
                List<DeathReason> Reason = (from DeathReason current in Enum.GetValues(typeof(DeathReason)) where current.ToString() == damage select current).ToList();

                if (Reason.Count < 1) return DeathReason.Unknown;
                else return Reason[0];
            }

            [JsonIgnore]
            internal string JSON { get { return JsonConvert.SerializeObject(this, Formatting.Indented); } }
            
            
            internal static DeathData Get(object obj)
            {
                if (obj == null) return null;
                JObject jobj = (JObject) obj;
                DeathData data = new DeathData
                {
                    bodypart = jobj["bodypart"].ToString(),
                    weapon = jobj["weapon"].ToString(),
                    attachments = (from attachment in jobj["attachments"] select attachment.ToString()).ToList(),
                    _distance = Convert.ToSingle(jobj["distance"])
                };

                /// Victim
                data.victim.name = jobj["victim"]["name"].ToString();

                List<VictimType> victypes = (from VictimType current in Enum.GetValues(typeof(VictimType)) where current.GetHashCode().ToString() == jobj["victim"]["type"].ToString() select current).ToList();

                if (victypes.Count != 0)
                    data.victim.type = victypes[0];

                /// Attacker
                data.attacker.name = jobj["attacker"]["name"].ToString();

                List<AttackerType> attackertypes = (from AttackerType current in Enum.GetValues(typeof(AttackerType)) where current.GetHashCode().ToString() == jobj["attacker"]["type"].ToString() select current).ToList();

                if (attackertypes.Count != 0)
                    data.attacker.type = attackertypes[0];
                
                /// Reason
                List<DeathReason> reasons = (from DeathReason current in Enum.GetValues(typeof(DeathReason)) where current.GetHashCode().ToString() == jobj["reason"].ToString() select current).ToList();
                if (reasons.Count != 0)
                    data.reason = reasons[0];

                return data;
            }
        }

        #endregion

        #region Enums / Types

        private enum VictimType
        {
            APC,
            Player,
            Helicopter,
            Animal,
            Invalid
        }

        private enum AttackerType
        {
            APC,
            CargoShip,
            Player,
            Helicopter,
            Animal,
            Turret,
            Structure,
            Trap,
            Self,
            SamSite,
            Minicopter,
            ScrapHeli,
            TeslaCoil,
            Invalid
        }

        private enum DeathReason
        {
            APC,
            CargoShip,
            APCDeath,
            CargoShipDeath,
            Turret,
            Helicopter,
            HelicopterDeath,
            Structure,
            Trap,
            Animal,
            AnimalDeath,
            Generic,
            Hunger,
            Thirst,
            Cold,
            Drowned,
            Heat,
            Bleeding,
            Poison,
            Suicide,
            Bullet,
            Arrow,
            Flamethrower,
            Slash,
            Blunt,
            Fall,
            Radiation,
            Stab,
            SamSite,
            Minicopter,
            ScrapHeli,
            TeslaCoil,
            Explosion,
            Unknown
        }

        #endregion

        #region Player Settings

        private List<string> playerSettingFields
        {
            get
            {
                return (from field in typeof(PlayerSettings).GetFields() select field.Name).ToList();
            }
        }

        private List<string> GetSettingValues(BasePlayer player) => (from field in typeof(PlayerSettings).GetFields() select $"{field.Name} : {field.GetValue(playerSettings[player.userID]).ToString().ToLower()}").ToList();

        private void SetSettingField<T>(BasePlayer player, string field, T value)
        {
            var fields = typeof(PlayerSettings).GetFields();
            for(int i = 0; i < fields.Length; i++)
            {
                var curr = fields[i];
                if (curr.Name == field)
                    curr.SetValue(playerSettings[player.userID], value);
            }
        }

        #endregion

        #region General Plugin Hooks

        private void Loaded()
        {
            dn = this;

            if (killReproducing)
                RegisterPerm("reproduce");

            RegisterPerm("customize");
            RegisterPerm("see");

            LoadConf();
            LoadData();
            LoadMessages();
            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                PlayerSettings settingsOut;
                if (!playerSettings.TryGetValue(player.userID, out settingsOut))
                {
                    playerSettings[player.userID] = new PlayerSettings(this);
                    SaveData();
                }
            }

               

            PopupNotifications = (Plugin)plugins.Find("PopupNotifications");

            if (PopupNotifications == null && UsePopupNotifications)
                PrintWarning("You have set 'Use Popup Notifications' to true, but the Popup Notifications plugin is not installed. Popups will not work without it. Get it here: http://oxidemod.org/plugins/1252/");
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Generating new config file...");
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            PlayerSettings settingsOut;
            if (!playerSettings.TryGetValue(player.userID, out settingsOut))
            {
                playerSettings[player.userID] = new PlayerSettings(this);
                SaveData();
            }
        }

        private void OnPluginLoaded(object plugin)
        {
            if (plugin is Plugin && ((Plugin)plugin).Title == "Popup Notifications")
                PopupNotifications = (Plugin)plugin;
        }

        private void Unload() => SaveData();

        #endregion

        #region Loading

        private void LoadData()
        {
            //canRead = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, bool>>("DeathNotes");

            playerSettings = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, PlayerSettings>>("DeathNotes/PlayerSettings");

            if (killReproducing)
                reproduceableKills = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, string>>("DeathNotes/KillReproducing");
        }

        private void SaveData()
        {
            //Interface.Oxide.DataFileSystem.WriteObject("DeathNotes", canRead);

            Interface.Oxide.DataFileSystem.WriteObject("DeathNotes/PlayerSettings", playerSettings);

            if (killReproducing)
                Interface.Oxide.DataFileSystem.WriteObject("DeathNotes_KillReproducing", reproduceableKills);
        }

        private void LoadConf()
        {
            SetConfig("Settings", "Chat Icon (SteamID)", "76561198077847390");

            SetConfig("Settings", "Message Radius Enabled", false);
            SetConfig("Settings", "Message Radius", 300f);

            SetConfig("Settings", "Log to File", false);
            SetConfig("Settings", "Write to Console", true);
            SetConfig("Settings", "Write to Chat", true);
            SetConfig("Settings", "Use Popup Notifications", false);
            SetConfig("Settings", "Use Simple UI", false);
            SetConfig("Settings", "Strip Colors from Simple UI", false);
            SetConfig("Settings", "Simple UI - Font Size", 20);
            SetConfig("Settings", "Simple UI - Top", 0.1f);
            SetConfig("Settings", "Simple UI - Left", 0.1f);
            SetConfig("Settings", "Simple UI - Max Width", 0.8f);
            SetConfig("Settings", "Simple UI - Max Height", 0.05f);

            SetConfig("Settings", "Simple UI Hide Timer", 5f);

            SetConfig("Settings", "Needs Permission", false);

            SetConfig("Settings", "Title", "Death Notes");
            SetConfig("Settings", "Formatting", "[{Title}]: {Message}");
            SetConfig("Settings", "Console Formatting", "{Message}");

            SetConfig("Settings", "Attachments Split", " | ");
            SetConfig("Settings", "Attachments Formatting", " ({attachments})");

            SetConfig("Settings", "Title Color", "#80D000");
            SetConfig("Settings", "Victim Color", "#C4FF00");
            SetConfig("Settings", "Attacker Color", "#C4FF00");
            SetConfig("Settings", "Weapon Color", "#C4FF00");
            SetConfig("Settings", "Attachments Color", "#C4FF00");
            SetConfig("Settings", "Distance Color", "#C4FF00");
            SetConfig("Settings", "Bodypart Color", "#C4FF00");
            SetConfig("Settings", "Message Color", "#696969");
            SetConfig("Settings", "Health Color", "#C4FF00");

            SetConfig("Names", new Dictionary<string, object> { });
            SetConfig("Bodyparts", new Dictionary<string, object> { });
            SetConfig("Weapons", new Dictionary<string, object> { });
            SetConfig("Attachments", new Dictionary<string, object> { });

            SetConfig("Messages", "Bleeding", new List<object> { "{victim} bled out." });
            SetConfig("Messages", "Blunt", new List<object> { "{attacker} used a {weapon} to knock {victim} out." });
            SetConfig("Messages", "Bullet", new List<object> { "{victim} was shot in the {bodypart} by {attacker} with a {weapon}{attachments} from {distance}m." });
            SetConfig("Messages", "Flamethrower", new List<object> { "{victim} was burned to ashes by {attacker} using a {weapon}." });
            SetConfig("Messages", "Cold", new List<object> { "{victim} became an iceblock." });
            SetConfig("Messages", "Drowned", new List<object> { "{victim} tried to swim." });
            SetConfig("Messages", "Explosion", new List<object> { "{victim} was shredded by {attacker}'s {weapon}" });
            SetConfig("Messages", "Fall", new List<object> { "{victim} did a header into the ground." });
            SetConfig("Messages", "Generic", new List<object> { "The death took {victim} with him." });
            SetConfig("Messages", "Heat", new List<object> { "{victim} burned to ashes." });
            SetConfig("Messages", "Helicopter", new List<object> { "{victim} was shot to pieces by a {attacker}." });
            SetConfig("Messages", "HelicopterDeath", new List<object> { "The {victim} was taken down." });
            SetConfig("Messages", "Animal", new List<object> { "A {attacker} followed {victim} until it finally caught him." });
            SetConfig("Messages", "AnimalDeath", new List<object> { "{attacker} killed a {victim} with a {weapon}{attachments} from {distance}m." });
            SetConfig("Messages", "Hunger", new List<object> { "{victim} forgot to eat." });
            SetConfig("Messages", "Poison", new List<object> { "{victim} died after being poisoned." });
            SetConfig("Messages", "Radiation", new List<object> { "{victim} became a bit too radioactive." });
            SetConfig("Messages", "Slash", new List<object> { "{attacker} slashed {victim} in half." });
            SetConfig("Messages", "Stab", new List<object> { "{victim} was stabbed to death by {attacker} using a {weapon}." });
            SetConfig("Messages", "Structure", new List<object> { "A {attacker} impaled {victim}." });
            SetConfig("Messages", "Suicide", new List<object> { "{victim} had enough of life." });
            SetConfig("Messages", "Thirst", new List<object> { "{victim} dried internally." });
            SetConfig("Messages", "Trap", new List<object> { "{victim} ran into a {attacker}" });
            SetConfig("Messages", "Turret", new List<object> { "A {attacker} defended its home against {victim}." });
            SetConfig("Messages", "Unknown", new List<object> { "{victim} died. Nobody knows why, it just happened." });
            
            SetConfig("Messages", "Blunt Sleeping", new List<object> { "{attacker} used a {weapon} to turn {victim}'s dream into a nightmare." });
            SetConfig("Messages", "Bullet Sleeping", new List<object> { "Sleeping {victim} was shot in the {bodypart} by {attacker} with a {weapon}{attachments} from {distance}m." });
            SetConfig("Messages", "Flamethrower Sleeping", new List<object> { "{victim} was burned to ashes by sleeping by {attacker} using a {weapon}." });
            SetConfig("Messages", "Explosion Sleeping", new List<object> { "{victim} was shredded by {attacker}'s {weapon} while sleeping." });
            SetConfig("Messages", "Generic Sleeping", new List<object> { "The death took sleeping {victim} with him." });
            SetConfig("Messages", "Helicopter Sleeping", new List<object> { "{victim} was sleeping when he was shot to pieces by a {attacker}." });
            SetConfig("Messages", "Animal Sleeping", new List<object> { "{victim} was killed by a {attacker} while having a sleep." });
            SetConfig("Messages", "Slash Sleeping", new List<object> { "{attacker} slashed sleeping {victim} in half." });
            SetConfig("Messages", "Stab Sleeping", new List<object> { "{victim} was stabbed to death by {attacker} using a {weapon} before he could even awake." });
            SetConfig("Messages", "Unknown Sleeping", new List<object> { "{victim} was sleeping when he died. Nobody knows why, it just happened." });

            SaveConfig();

            //  Cache Config Variables
            ChatIcon = GetConfig("76561198077847390", "Settings", "Chat Icon (SteamID)");

            MessageRadiusEnabled = GetConfig(false, "Settings", "Message Radius Enabled");
            MessageRadius = GetConfig(300f, "Settings", "Message Radius");

            DoLogToFile = GetConfig(false, "Settings", "Log to File");
            WriteToConsole = GetConfig(true, "Settings", "Write to Console");
            WriteToChat = GetConfig(true, "Settings", "Write to Chat");
            UsePopupNotifications = GetConfig(false, "Settings", "Use Popup Notifications");
            UseSimpleUI = GetConfig(false, "Settings", "Use Simple UI");
            SimpleUI_StripColors = GetConfig(false, "Settings", "Strip Colors from Simple UI");
            SimpleUI_FontSize = GetConfig(20, "Settings", "Simple UI - Font Size");
            SimpleUI_Top = GetConfig(0.1f, "Settings", "Simple UI - Top");
            SimpleUI_Left = GetConfig(0.1f, "Settings", "Simple UI - Left");
            SimpleUI_MaxWidth = GetConfig(0.8f, "Settings", "Simple UI - Max Width");
            SimpleUI_MaxHeight = GetConfig(0.05f, "Settings", "Simple UI - Max Height");

            SimpleUI_HideTimer = GetConfig(5f, "Settings", "Simple UI Hide Timer");

            NeedsPermission = GetConfig(false, "Settings", "Needs Permission");

            ChatTitle = GetConfig("Death Notes", "Settings", "Title");
            ChatFormatting = GetConfig("[{Title}]: {Message}", "Settings", "Formatting");
            ConsoleFormatting = GetConfig("{Message}", "Settings", "Console Formatting");

            AttachmentSplit = GetConfig(" | ", "Settings", "Attachments Split");
            AttachmentFormatting = GetConfig(" ({attachments})", "Settings", "Attachments Formatting");

            TitleColor = GetConfig("#80D000", "Settings", "Title Color");
            VictimColor = GetConfig("#C4FF00", "Settings", "Victim Color");
            AttackerColor = GetConfig("#C4FF00", "Settings", "Attacker Color");
            WeaponColor = GetConfig("#C4FF00", "Settings", "Weapon Color");
            AttachmentColor = GetConfig("#C4FF00", "Settings", "Attachments Color");
            DistanceColor = GetConfig("#C4FF00", "Settings", "Distance Color");
            BodypartColor = GetConfig("#C4FF00", "Settings", "Bodypart Color");
            MessageColor = GetConfig("#696969", "Settings", "Message Color");

            Names = GetConfig(new Dictionary<string, object> { }, "Names");
            Bodyparts = GetConfig(new Dictionary<string, object> { }, "Bodyparts");
            Weapons = GetConfig(new Dictionary<string, object> { }, "Weapons");
            Attachments = GetConfig(new Dictionary<string, object> { }, "Attachments");

            Messages = GetConfig(new Dictionary<string, object>
            {
                //  Normal
                { "Bleeding", new List<object> { "{victim} bled out." }},
                { "Blunt", new List<object> { "{attacker} used a {weapon} to knock {victim} out." }},
                { "Bullet", new List<object> { "{victim} was shot in the {bodypart} by {attacker} with a {weapon}{attachments} from {distance}m." }},
                { "Flamethrower", new List<object> { "{victim} was burned to ashes by {attacker} using a {weapon}." }},
                { "Cold", new List<object> { "{victim} became an iceblock." }},
                { "Drowned", new List<object> { "{victim} tried to swim." }},
                { "Explosion", new List<object> { "{victim} was shredded by {attacker}'s {weapon}" }},
                { "Fall", new List<object> { "{victim} did a header into the ground." }},
                { "Generic", new List<object> { "The death took {victim} with him." }},
                { "Heat", new List<object> { "{victim} burned to ashes." }},
                { "Helicopter", new List<object> { "{victim} was shot to pieces by a {attacker}." }},
                { "HelicopterDeath", new List<object> { "The {victim} was taken down." }},
                { "Animal", new List<object> { "A {attacker} followed {victim} until it finally caught him." }},
                { "AnimalDeath", new List<object> { "{attacker} killed a {victim} with a {weapon}{attachments} from {distance}m." }},
                { "Hunger", new List<object> { "{victim} forgot to eat." }},
                { "Poison", new List<object> { "{victim} died after being poisoned." }},
                { "Radiation", new List<object> { "{victim} became a bit too radioactive." }},
                { "Slash", new List<object> { "{attacker} slashed {victim} in half." }},
                { "Stab", new List<object> { "{victim} was stabbed to death by {attacker} using a {weapon}." }},
                { "Structure", new List<object> { "A {attacker} impaled {victim}." }},
                { "Suicide", new List<object> { "{victim} had enough of life." }},
                { "Thirst", new List<object> { "{victim} dried internally." }},
                { "Trap", new List<object> { "{victim} ran into a {attacker}" }},
                { "Turret", new List<object> { "A {attacker} defended its home against {victim}." }},
                { "Unknown", new List<object> { "{victim} died. Nobody knows why, it just happened." }},

                //  Sleeping
                { "Blunt Sleeping", new List<object> { "{attacker} used a {weapon} to turn {victim}'s dream into a nightmare." }},
                { "Bullet Sleeping", new List<object> { "Sleeping {victim} was shot in the {bodypart} by {attacker} with a {weapon}{attachments} from {distance}m." }},
                { "Flamethrower Sleeping", new List<object> { "{victim} was burned to ashes while sleeping by {attacker} using a {weapon}." }},
                { "Explosion Sleeping", new List<object> { "{victim} was shredded by {attacker}'s {weapon} while sleeping." }},
                { "Generic Sleeping", new List<object> { "The death took sleeping {victim} with him." }},
                { "Helicopter Sleeping", new List<object> { "{victim} was sleeping when he was shot to pieces by a {attacker}." }},
                { "Animal Sleeping", new List<object> { "{victim} was killed by a {attacker} while having a sleep." }},
                { "Slash Sleeping", new List<object> { "{attacker} slashed sleeping {victim} in half." }},
                { "Stab Sleeping", new List<object> { "{victim} was stabbed to death by {attacker} using a {weapon} before he could even awake." }},
                { "Unknown Sleeping", new List<object> { "{victim} was sleeping when he died. Nobody knows why, it just happened." }}
            }, "Messages").ToDictionary(l => l.Key, l => ((List<object>)l.Value).ConvertAll(m => m.ToString()));
        }

        private void LoadMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"No Permission", "You don't have permission to use this command."},
                {"Hidden", "You do no longer see death messages."},
                {"Unhidden", "You will now see death messages."},
                {"Field Not Found", "The field could not be found!"},
                {"True Or False", "{arg} must be 'true' or 'false'!"},
                {"Field Set", "Field '{field}' set to '{value}'"}
            }, this);
        }

        #endregion

        #region Commands

        [ChatCommand("deaths")]
        private void cmdDeaths(BasePlayer player, string cmd, string[] args)
        {
            if(!HasPerm(player.userID, "customize"))
            {
                SendMessage(player, string.Empty, GetMsg("No Permission", player.userID));
                return;
            }

            PlayerSettings settings;
            if (!playerSettings.TryGetValue(player.userID, out settings))
            {
                SendReply(player, "No settings!");
                return;
            }

            if (args.Length == 0)
            {
                // SendMessage(player, string.Empty, "/deaths set <field> <value> - set a value");
                //   SendMessage(player, string.Empty, "Fields: " + Environment.NewLine + ListToString(GetSettingValues(player), 0, Environment.NewLine));
                SendReply(player, "Death messages are currently <color=" + (settings.chat ? "lime>enabled" : "red>disabled") + "</color>. To toggle, type: <color=#8aff47>/" + cmd + " chat</color>" + Environment.NewLine + Environment.NewLine + "Maximum distance to show death messages is: <color=orange>" + (settings.MaxDistance <= 0f ? "0 (always)" : (settings.MaxDistance + "m")) + "</color>" + Environment.NewLine + "To change this, here's an example: <color=orange>/" + cmd + " 330</color> You would now only see death messages if they happened within <color=orange>330</color> meters of your position. You can set this to any value, and you can make it global by using <color=orange>0</color>.");
                return;
            }
            var arg0 = args[0];
            if (arg0.Equals("chat", StringComparison.OrdinalIgnoreCase))
            {
                settings.chat = !settings.chat;
                SendReply(player, "Chat death messages are now: " + (settings.chat ? "enabled" : "disabled") + ".");
                return;
            }
            else
            {
                float dist;
                if (!float.TryParse(arg0, out dist))
                {
                    SendReply(player, "Not a valid distance or argument: " + arg0);
                    return;
                }
                else
                {
                    settings.MaxDistance = dist;
                    SendReply(player, "Maximum distance to show death messages from is now: " + dist + "m.");
                }
            }
           
            /*/
            switch(args[0].ToLower())
            {
                case "set":
                    if(args.Length != 3)
                    {
                        SendMessage(player, string.Empty, "Syntax: /deaths set <field> <value>");
                        return;
                    }

                    if(!playerSettingFields.Contains(args[1].ToLower()))
                    {
                        SendMessage(player, string.Empty, GetMsg("Field Not Found", player.userID));
                        return;
                    }
                    
                    bool value = false;

                    try
                    {
                        value = Convert.ToBoolean(args[2]);
                    }
                    catch(FormatException)
                    {
                        SendMessage(player, string.Empty, GetMsg("True Or False", player.userID).Replace("{arg}", "<value>"));
                        return;
                    }

                    SetSettingField(player, args[1].ToLower(), value);

                    SendMessage(player, string.Empty, GetMsg("Field Set", player.userID).Replace("{value}", value.ToString().ToLower()).Replace("{field}", args[1].ToLower()));

                    SaveData();

                    break;

                default:
                    SendMessage(player, string.Empty, "/deaths set <field> <value> - set a value");
                    SendMessage(player, string.Empty, "Fields", Environment.NewLine + ListToString(GetSettingValues(player), 0, Environment.NewLine));
                    break;
            }/*/
        }

        [ChatCommand("deathnotes")]
        private void cmdGetInfo(BasePlayer player) => GetInfo(player);

        [ConsoleCommand("reproducekill")]
        private void ccmdReproduceKill(ConsoleSystem.Arg arg)
        {
            bool hasPerm = false;

            if (arg?.Connection == null)
                hasPerm = true;
            else
            {
                if((BasePlayer)arg.Connection.player != null)
                {
                    if (HasPerm(arg.Connection.userid, "reproduce"))
                        hasPerm = true;
                }
            }
            
            if (hasPerm)
            {
                if (arg.Args == null || arg.Args.Length != 1)
                {
                    arg.ReplyWith("Syntax: reproducekill <datetime>");
                    return;
                }
                
                if(reproduceableKills.ContainsKey(arg.Args[0]))
                {
                    DeathData data = DeathData.Get(JsonConvert.DeserializeObject(reproduceableKills[arg.Args[0]]));
                    PrintWarning("Reproduced Kill: " + Environment.NewLine + data.JSON);

                    if (data == null)
                        return;

                    NoticeDeath(data, true);
                    arg.ReplyWith("Death reproduced!");
                }
                else
                    arg.ReplyWith("No saved kill at that time found!");
            }
        }

        #endregion

        #region DeathNotes Information

        private void GetInfo(BasePlayer player)
        {
            if (player == null) return;
            webrequest.Enqueue("http://oxidemod.org/plugins/819/", null, (code, response) => {
                if(code != 200)
                {
                    PrintWarning("Failed to get information!");
                    return;
                }

                string version_published = "0.0.0";
                string version_installed = this.Version.ToString();

                Match version = new Regex(@"<h3>Version (\d{1,2}(\.\d{1,2})+?)<\/h3>").Match(response);
                if(version.Success)
                {
                    version_published = version.Groups[1].ToString();
                }

                SendMessage(player, string.Empty, $"<size=25><color=#C4FF00>DeathNotes</color></size> <size=20><color=#696969>by LaserHydra</color>{Environment.NewLine}<color=#696969>Latest <color=#C4FF00>{version_published}</color>{Environment.NewLine}Installed <color=#C4FF00>{version_installed}</color></color></size>");
            }, this);
        }

        #endregion

        #region Death Related

        private HitInfo TryGetLastWounded(ulong uid, HitInfo info)
        {
            HitInfo outInfo;
            if (LastWounded.TryGetValue(uid, out outInfo))
            {
                LastWounded.Remove(uid);
                return outInfo;
            }
            return info;
        }

        private void OnEntityTakeDamage(BasePlayer victim, HitInfo info)
        {
            if (victim == null || info == null) return;
            var pVictim = victim as BasePlayer;
            var pAttacker = info?.Initiator as BasePlayer; 
            if (pVictim != null && pAttacker != null)
            {
                NextTick(() =>
                {
                    try { if ((pVictim?.IsWounded() ?? false)) LastWounded[pVictim.userID] = info; }
                    catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Failed to complete OnEntityTakeDamage: " + ex.Message); }
                });
            }
        }

        private void OnEntityDeath(BaseCombatEntity victim, HitInfo info)
        {
            if (victim == null || info == null) return;
            try
            {
                var pVictim = victim as BasePlayer;
                var pAttacker = info?.Initiator as BasePlayer;
                var vicSleep = pVictim?.IsSleeping() ?? false;
                if (pVictim != null && pVictim.IsWounded()) info = TryGetLastWounded(pVictim.userID, info);

                if (pAttacker == null && (victim?.name?.Contains("autospawn") ?? false))
                {
                    PrintWarning("Autospawn death!");
                    return;
                }

                DeathData data = new DeathData
                {
                    dmgType = (info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic)
                };
                data.victim.entity = victim;
                data.victim.type = data?.victim?.TryGetType() ?? VictimType.Invalid;
                data.victim.sleeping = vicSleep;

                if (data.victim.type == VictimType.Invalid) return;

                data.victim.name = StripTags(data.victim.TryGetName());
                if (string.IsNullOrEmpty(data.victim.name))
                {
                    return;
                }

                data.attacker.entity = info?.Initiator;

                if (data?.attacker?.entity == null)
                {
                    var weaponPrefab = info?.Weapon ?? info?.WeaponPrefab ?? null;
                    if ((weaponPrefab?.net?.ID.Value ?? 0) > 0)
                    {
                        var weaponProj = weaponPrefab as BaseProjectile;
                        if (weaponProj != null)
                        {
                            data.attacker.entity = weaponProj?.GetComponentInChildren<AutoTurret>() ?? weaponProj?.GetComponentInParent<AutoTurret>() ?? null;
                        }

                        if (data?.attacker?.entity == null)
                        {
                            foreach (var entity in BaseNetworkable.serverEntities) //never confirmed if this works lol
                            {
                                var turret = entity as AutoTurret;
                                if (turret?.AttachedWeapon?.net != null && turret.AttachedWeapon.net.ID == weaponPrefab?.net?.ID)
                                {
                                    data.attacker.entity = turret;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (data?.attacker?.entity == null) data.attacker.entity = victim?.lastAttacker ?? null;
              
               
                data.attacker.healthLeft = data.attacker?.entity?.Health() ?? 0f;

                data.attacker.type = data.attacker.TryGetType();
                
                data.attacker.name = StripTags(data.attacker.TryGetName());
                
                if (string.IsNullOrEmpty(data?.attacker?.name))
                {
                   // PrintWarning("null/empty data.attacker.name");
                    return;
                }

                //isadmin debug
                //if (pVictim != null && pVictim.IsAdmin) PrintWarning("attacker ent: " + data.attacker.entity + ", type: " + data.attacker.type + ", name: " + data.attacker.name);

                var itemName = (info?.Weapon != null) ? (info?.Weapon?.GetItem()?.info?.displayName?.english ?? FormatThrownWeapon(info?.Weapon?.name ?? info?.Weapon?.ShortPrefabName ?? string.Empty)) : (info?.WeaponPrefab != null) ? (info?.WeaponPrefab?.GetItem()?.info?.displayName?.english ?? FormatThrownWeapon(info?.WeaponPrefab?.name ?? info?.WeaponPrefab?.ShortPrefabName ?? string.Empty)) : data.dmgType.ToString();

                if (string.IsNullOrEmpty(itemName)) PrintWarning("item name is null");

                var weaponItem = info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem() ?? null;
                var custName = (info?.Weapon == null) ? string.Empty : (info?.Weapon?.GetItem()?.name ?? string.Empty);

                data.weapon = itemName;
                if (!string.IsNullOrEmpty(custName)) data.customWeaponName = itemName + " " + custName;

                /*/ - temp(?) disable of appending skin name
                if (weaponItem != null && weaponItem.skin != 0 && SkinsAPI != null)
                {
                    var skinName = (SkinsAPI?.Call<string>("GetSkinNameFromID", weaponItem.skin.ToString()) ?? string.Empty).Replace(weaponItem.info.displayName.english, string.Empty, StringComparison.OrdinalIgnoreCase).TrimEnd();
                    if (!string.IsNullOrEmpty(skinName))
                    {
                        if (string.IsNullOrEmpty(data.customWeaponName)) data.customWeaponName += itemName;
                        data.customWeaponName += " <i><color=#2CAFB8>(" + skinName + ")</color></i>";
                    }
                }/*/

                data.attachments = GetAttachments(info);
                data.damageType = FirstUpper((victim?.lastDamage ?? Rust.DamageType.Generic).ToString());

                if (data.weapon.Equals("Heli Rocket", StringComparison.OrdinalIgnoreCase))
                {
                    data.attacker.name = "Patrol Helicopter";
                    data.reason = DeathReason.Helicopter;
                }

 
                var hitBone = info?.HitBone ?? 0;
                data.bodypart = FirstUpper(GetBoneName(victim, hitBone) ?? "Unknown Bone");


                data.reason = data.TryGetReason();
                var odnObj = Interface.CallHook("OnDeathNotice", JObject.FromObject(data));
                var odnBool = false;
                if (odnObj != null && bool.TryParse(odnObj.ToString(), out odnBool) && !odnBool) return;

                NoticeDeath(data);
            }
            catch(Exception ex)
            {
                PrintError(ex.ToString() + Environment.NewLine + "Failed to complete OnEntityDeath: " + ex.Message);
            }
        }

        private void NoticeDeath(DeathData data, bool reproduced = false)
        {
            if (data == null) return;
            if ((data?.attacker?.type ?? AttackerType.Invalid) == AttackerType.Animal && (data?.victim?.type ?? VictimType.Invalid) == VictimType.Animal) return;
            try
            {
                DeathData newData = UpdateData(data);
                if (newData == null)
                {
                    PrintWarning("newData is null after updatedata!");
                    return;
                }
                var deathMsg = GetDeathMessage(newData, false);
                var deathMsgCons = GetDeathMessage(newData, true);
                if (string.IsNullOrEmpty(deathMsg) || string.IsNullOrEmpty(deathMsgCons))
                {
                    var isPlayerVic = (newData?.victim?.entity?.ToPlayer() ?? null) != null;
                    if (isPlayerVic)
                    {
                        var dataStr = newData?.JSON ?? data?.JSON ?? data.ToString() ?? string.Empty;
                        PrintWarning("Death Data (deathMsg) string is empty, here's dataStr:\n" + dataStr);
                        var atk = newData?.attacker ?? data?.attacker ?? null;
                        var vic = newData?.victim ?? data?.victim ?? null;
                        if (atk == null) PrintWarning("Atk IS null");
                        if (vic == null) PrintWarning("Vic IS null");
                        if (atk != null) PrintWarning("atk: " + (atk?.name ?? "Unknown"));
                        if (vic != null) PrintWarning("vic: " + (vic?.name ?? "Unknown"));
                    }
                    return;
                }

                if (newData.distance > 500f)
                {
                    Puts("Kill with distance > 500f, not broadcasting!");
                    Puts(StripTags(deathMsgCons));
                    return;
                }

                var atkName = newData?.attacker?.name ?? string.Empty;
                var vicName = newData?.victim?.name ?? string.Empty;
                var atkUID = (newData?.attacker?.entity as BasePlayer)?.userID ?? 0;
                var vicUID = (newData?.victim?.entity as BasePlayer)?.userID ?? 0;
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null) continue;
                    var canSendDeathMsgObj = Interface.CallHook("CanSendDeathNotice", player, data?.attacker?.entity ?? null, data?.victim?.entity ?? null);
                    if (canSendDeathMsgObj != null && (canSendDeathMsgObj is bool) && !(bool)canSendDeathMsgObj) continue;
                    if (!InRadius(player, data.attacker.entity)) continue;
                    var isStreamer = StreamerAPI?.Call<bool>("IsStreamerMode", player) ?? false;
                    if (isStreamer)
                    {
                        var atkStreamerName = FirstUpper((StreamerAPI?.Call<string>("Get", atkUID) ?? string.Empty));
                        var vicStreamerName = FirstUpper((StreamerAPI?.Call<string>("Get", vicUID) ?? string.Empty));
                        if (!string.IsNullOrEmpty(atkStreamerName) || !string.IsNullOrEmpty(vicStreamerName))
                        {
                            var deathSB = Facepunch.Pool.Get<StringBuilder>();
                            try { deathMsg = deathSB.Clear().Append(deathMsg).Replace(atkName, atkStreamerName).Replace(vicName, vicStreamerName).ToString(); }
                            finally { Facepunch.Pool.Free(ref deathSB); }
                        }
                        
                    }
                    if (CanSee(player, "chat")) SendMessage(player, string.Empty, deathMsg, ChatIcon);

                    if (CanSee(player, "ui")) UIMessage(player, SimpleUI_StripColors ? StripTags(deathMsgCons) : deathMsgCons);
                }

                if (WriteToConsole)
                    Puts(StripTags(deathMsgCons));

                if (DoLogToFile)
               //     ConVar.Server.Log("oxide/logs/Kills.txt", StripTags(deathMsgCons));

                if (UsePopupNotifications)
                    PopupMessage(deathMsg);

                if (debug)
                {
                    PrintWarning("DATA: " + Environment.NewLine + data.JSON);
                    PrintWarning("UPDATED DATA: " + Environment.NewLine + newData.JSON);
                }

                if (killReproducing && !reproduced)
                {
                    reproduceableKills[DateTime.Now.ToString()] = data.JSON.Replace(Environment.NewLine, "");
                    SaveData();
                }
            }
            catch(Exception ex)
            {
                PrintError(ex.ToString() + Environment.NewLine + "Failed to complete NoticeDeath: " + ex.Message);
            }
            
        }

        #endregion

        #region Formatting

        private string FormatThrownWeapon(string unformatted)
        {
            if (string.IsNullOrEmpty(unformatted)) return unformatted;

            var splitUnform = unformatted.Split('/');
            var formatted = new StringBuilder(FirstUpper(splitUnform[splitUnform.Length - 1])).Replace(".prefab", string.Empty).Replace(".entity", string.Empty).Replace(".weapon", string.Empty).Replace(".deployed", string.Empty).Replace("_", " ").Replace(".", string.Empty).ToString();

            if (formatted == "Stonehatchet")
                formatted = "Stone Hatchet";
            else if (formatted == "Knife Bone")
                formatted = "Bone Knife";
            else if (formatted == "Spear Wooden")
                formatted = "Wooden Spear";
            else if (formatted == "Spear Stone")
                formatted = "Stone Spear";
            else if (formatted == "Icepick Salvaged")
                formatted = "Salvaged Icepick";
            else if (formatted == "Axe Salvaged")
                formatted = "Salvaged Axe";
            else if (formatted == "Hammer Salvaged")
                formatted = "Salvaged Hammer";
            else if (formatted == "Grenadef1")
                formatted = "F1 Grenade";
            else if (formatted == "Grenadebeancan")
                formatted = "Beancan Grenade";
            else if (formatted == "Explosivetimed")
                formatted = "Timed Explosive";

            return formatted;
        }

        private string StripTags(string original)
        {
            if (string.IsNullOrEmpty(original)) return original;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try 
            {
                sb.Clear().Append(original);
                for (int i = 0; i < tags.Count; i++) { sb.Replace(tags[i], string.Empty); }

                var final = sb.ToString();
                for (int i = 0; i < regexTags.Count; i++) { final = regexTags[i].Replace(final, string.Empty); }

                return final;
            }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private string FirstUpper(string original)
        {
            if (string.IsNullOrEmpty(original)) return original;


            List<string> output = new List<string>();
            var split = original.Split(' ');
            for(int i = 0; i < split.Length; i++)
            {
                var word = split[i];
                output.Add(word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1));
            }
                

            return ListToString(output, 0, " ");
        }

        #endregion

        #region Shady Util 
        //things i specifically put here and wanted in #util
        private ItemDefinition FindItemByPartialName(string engOrShortName)
        {
            if (string.IsNullOrEmpty(engOrShortName)) throw new ArgumentNullException(nameof(engOrShortName));


            ItemDefinition def = null;

            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                if (item.displayName.english.Equals(engOrShortName, StringComparison.OrdinalIgnoreCase) || item.shortname.Equals(engOrShortName, StringComparison.OrdinalIgnoreCase)) return item;

                var engName = item?.displayName?.english;

                if (engName.IndexOf(engOrShortName, StringComparison.OrdinalIgnoreCase) >= 0 || item.shortname.IndexOf(engOrShortName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (def != null)
                    {
                        return null; //multiple matches found!!!
                    }

                    def = item;
                }
            }

            return def;
        }
        #endregion

        #region Death Variables Methods

        private List<string> GetMessages(string reason)
        {
            var listOut = new List<string>();
            if (!Messages.TryGetValue(reason, out listOut)) listOut = new List<string>();
            return listOut;
        }

        private List<string> GetAttachments(HitInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var attachments = new List<string>();
            if (info?.Weapon != null)
            {
                var itemList = info?.Weapon?.GetItem()?.contents?.itemList ?? null;
                if (itemList != null && itemList.Count > 0)
                {
                    for (int i = 0; i < itemList.Count; i++) attachments.Add((itemList[i]?.info?.displayName?.english ?? string.Empty));
                }
            }
            return attachments;
           // var attachments = info?.Weapon?.GetItem()?.contents?.itemList?.Select(p => (p?.info?.displayName?.english ?? string.Empty))?.ToList() ?? null;
           // return (attachments != null) ? attachments : new List<string>();
        }

        private string GetBoneName(BaseCombatEntity entity, uint boneId) => entity?.skeletonProperties?.FindBone(boneId)?.name?.english ?? "Body";

        private ItemDefinition GetItemDefFromPrefabName(string shortprefabName)
        {
            if (string.IsNullOrEmpty(shortprefabName)) throw new ArgumentNullException();
            var adjustedNameSB = new StringBuilder(shortprefabName).Replace("_deployed", "").Replace(".deployed", "").Replace("_", "").Replace(".entity", "");
            var def = ItemManager.FindItemDefinition(adjustedNameSB.ToString()) ?? null;
            if (def == null) def = ItemManager.FindItemDefinition(adjustedNameSB.Replace("_", ".").ToString());
            return def;
        }

        private bool InRadius(BasePlayer player, BaseCombatEntity attacker)
        {
            if (player == null || attacker == null || player.gameObject == null || attacker.gameObject == null || player?.transform == null || attacker?.transform == null) return false;
            try
            {
                var dist = player.Distance(attacker);
                PlayerSettings settings;
                if (playerSettings.TryGetValue(player.userID, out settings) && settings.MaxDistance > 0 && dist > settings.MaxDistance) return false;
                return !MessageRadiusEnabled || (dist <= MessageRadius);
            }
            catch (Exception ex)
            {
                PrintError("Failed to complete InRadius (BCE): " + Environment.NewLine + ex.ToString());
                return false;
            }
        }

        private bool InRadius(BasePlayer player, BaseEntity attacker)
        {
            if (player == null || attacker == null || player.gameObject == null || attacker.gameObject == null || player?.transform == null || attacker?.transform == null) return false;
            try
            {
                var dist = player.Distance(attacker);
                PlayerSettings settings;
                if (playerSettings.TryGetValue(player.userID, out settings) && settings.MaxDistance > 0 && dist > settings.MaxDistance) return false;
                return !MessageRadiusEnabled || (dist <= MessageRadius);
            }
            catch (Exception ex)
            {
                PrintError("Failed to complete InRadius (BE): " + Environment.NewLine + ex.ToString());
                return false;
            }
        }

        private string GetDeathMessage(DeathData data, bool console)
        {
            if (data == null)
            {
                PrintWarning("GetDeathMessage called on null DeathData!");
                return string.Empty;
            }

            var messageSB = Facepunch.Pool.Get<StringBuilder>();
            try 
            {
                messageSB.Clear();

                try
                {
                    var isSleep = data?.victim?.sleeping ?? data?.victim?.entity?.ToPlayer()?.IsSleeping() ?? false;
                    var reason = (isSleep && SleepingDeaths.Contains(data.reason)) ? (data.reason + " Sleeping") : data?.reason.ToString();
                    var messages = GetMessages(reason);
                    if (messages == null || messages.Count < 1)
                    {
                        return string.Empty;
                    }


                    string attachmentsString = data.attachments.Count == 0 ? string.Empty : AttachmentFormatting.Replace("{attachments}", ListToString(data.attachments, 0, AttachmentSplit));

                    if (console)
                        messageSB.Append(ConsoleFormatting.Replace("{Title}", $"<color={TitleColor}>{ChatTitle}</color>").Replace("{Message}", $"<color={MessageColor}>{messages.GetRandom()}</color>"));
                    else
                        messageSB.Append(ChatFormatting.Replace("{Title}", $"<color={TitleColor}>{ChatTitle}</color>").Replace("{Message}", $"<color={MessageColor}>{messages.GetRandom()}</color>"));

                    messageSB.Replace("{health}", $"<color={HealthColor}>{data.attacker.healthLeft.ToString("0.0")}</color>");
                    messageSB.Replace("{attacker}", $"<color={AttackerColor}>{data.attacker.name}</color>");
                    messageSB.Replace("{victim}", $"<color={VictimColor}>{data.victim.name}</color>");
                    messageSB.Replace("{distance}", $"<color={DistanceColor}>{data.distance.ToString("N0")}</color>");

                    var findByWeaponName = data?.weapon ?? data?.customWeaponName ?? string.Empty;


                    var weaponItem = string.IsNullOrWhiteSpace(findByWeaponName) ? null : FindItemByPartialName(findByWeaponName);


                    if (weaponItem == null)
                    {
                        PrintWarning("failed to find weaponItem for weapon: " + findByWeaponName);
                        messageSB.Replace("{weapon}", $"<color={WeaponColor}>{(!string.IsNullOrEmpty(data.customWeaponName) ? data.customWeaponName : data.weapon)}</color>");
                    }
                    else messageSB.Replace("{weapon}", ":" + weaponItem.shortname + ":"); //bad! need SB.

                    

                    messageSB.Replace("{bodypart}", $"<color={BodypartColor}>{data.bodypart}</color>");
                    messageSB.Replace("{attachments}", $"<color={AttachmentColor}>{attachmentsString}</color>");
                }
                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Failed to complete GetDeathMessage: " + ex.Message); }

                return messageSB.ToString();
            }
            finally { Facepunch.Pool.Free(ref messageSB); }
        }

        private DeathData UpdateData(DeathData data)
        {
            if (data == null)
            {
                Interface.Oxide.LogWarning("UpdateData called with null data!");
                return null;
            }
            try
            {
                bool configUpdated = false;

                if (data?.victim != null && (data?.victim?.type ?? VictimType.Invalid) != VictimType.Player)
                {
                    if (Config.Get("Names", data.victim.name) == null)
                    {
                        SetConfig("Names", data.victim.name, data.victim.name);
                        configUpdated = true;
                    }
                    else
                        data.victim.name = GetConfig(data.victim.name, "Names", data.victim.name);
                }

                if (data?.attacker != null && (data?.attacker?.type ?? AttackerType.Invalid) != AttackerType.Player)
                {
                    if (Config.Get("Names", data.attacker.name) == null)
                    {
                        SetConfig("Names", data.attacker.name, data.attacker.name);
                        configUpdated = true;
                    }
                    else
                        data.attacker.name = GetConfig(data.attacker.name, "Names", data.attacker.name);
                }

                if (Config.Get("Bodyparts", data.bodypart) == null)
                {
                    SetConfig("Bodyparts", data.bodypart, data.bodypart);
                    configUpdated = true;
                }
                else
                    data.bodypart = GetConfig(data.bodypart, "Bodyparts", data.bodypart);

                if (Config.Get("Weapons", data.weapon) == null)
                {
                    SetConfig("Weapons", data.weapon, data.weapon);
                    configUpdated = true;
                }
                else
                    data.weapon = GetConfig(data.weapon, "Weapons", data.weapon);

                if (data?.attachments != null && data.attachments.Count > 0)
                {
                    string[] attachmentsCopy = new string[data.attachments.Count];
                    data.attachments.CopyTo(attachmentsCopy);
                    for (int i = 0; i < attachmentsCopy.Length; i++)
                    {
                        var attachment = attachmentsCopy[i];
                        if (Config.Get("Attachments", attachment) == null)
                        {
                            SetConfig("Attachments", attachment, attachment);
                            configUpdated = true;
                        }
                        else
                        {
                            data.attachments.Remove(attachment);
                            data.attachments.Add(GetConfig(attachment, "Attachments", attachment));
                        }
                    }
                }
                

                if (configUpdated)
                    SaveConfig();
            }
            catch(Exception ex)
            {
                PrintError(ex.ToString() + Environment.NewLine + "Failed to complete UpdateData: " + ex.Message);
            }
            return data;
        }

        private bool CanSee(BasePlayer player, string type)
        {
            if (player == null || !(player?.IsConnected ?? false)) return false;
            try
            {
                PlayerSettings plySettings;
                playerSettings.TryGetValue(player.userID, out plySettings);
                if (!NeedsPermission)
                {
                    if (type == "ui") { return HasPerm(player.userID, "customize") ? (plySettings?.ui ?? UseSimpleUI) : UseSimpleUI; }
                    return HasPerm(player.userID, "customize") ? (plySettings?.chat ?? WriteToChat) : WriteToChat;
                }
                else
                {
                    if (HasPerm(player.userID, "see"))
                    {
                        if (type == "ui") { return HasPerm(player.userID, "customize") ? (plySettings?.ui ?? UseSimpleUI) : UseSimpleUI; }
                        return HasPerm(player.userID, "customize") ? (plySettings?.chat ?? WriteToChat) : WriteToChat;
                    }
                }
            }
            catch(Exception ex)
            {
                PrintError(ex.ToString() + Environment.NewLine + "Failed to complete CanSee: " + ex.Message);
            }
            return false;
        }

        #endregion

        #region Converting

        private string ListToString(List<string> list, int first, string seperator) => string.Join(seperator, list.Skip(first).ToArray());

        #endregion

        #region Config and Message Handling

        private void SetConfig(params object[] args)
        {
            List<string> stringArgs = (from arg in args select arg.ToString()).ToList<string>();
            stringArgs.RemoveAt(args.Length - 1);

            if (Config.Get(stringArgs.ToArray()) == null) Config.Set(args);
        }

        private T GetConfig<T>(T defaultVal, params object[] args)
        {
            List<string> stringArgs = (from arg in args select arg.ToString()).ToList<string>();
            if (Config.Get(stringArgs.ToArray()) == null)
            {
                PrintError($"The plugin failed to read something from the config: {ListToString(stringArgs, 0, "/")}{Environment.NewLine}Please reload the plugin and see if this message is still showing. If so, please post this into the support thread of this plugin.");
                return defaultVal;
            }

            return (T)Convert.ChangeType(Config.Get(stringArgs.ToArray()), typeof(T));
        }

        private string GetMsg(string key, object userID = null)
        {
            return lang.GetMessage(key, this, userID.ToString());
        }

        #endregion

        #region Permission Handling

        private void RegisterPerm(params string[] permArray)
        {
            string perm = ListToString(permArray.ToList(), 0, ".");

            permission.RegisterPermission($"{PermissionPrefix}.{perm}", this);
        }

        private bool HasPerm(object uid, params string[] permArray)
        {
            uid = uid.ToString();
            string perm = ListToString(permArray.ToList(), 0, ".");

            return permission.UserHasPermission(uid.ToString(), $"{PermissionPrefix}.{perm}");
        }

        private string PermissionPrefix
        {
            get
            {
                return this.Title.Replace(" ", "").ToLower();
            }
        }

        #endregion

        #region Messages

        private void SendMessage(BasePlayer player, string prefix, string message, string chatUserID = "0")
        {
            if (player == null || !player.IsConnected) return;
            var msg = (!string.IsNullOrEmpty(prefix)) ? prefix + ": " + message : message;
            player.SendConsoleCommand("chat.add", string.Empty, chatUserID, msg);
        }

        private void BroadcastMessage(string prefix, string message, string chatUserID = "0")
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null) continue;

                var msg = (!string.IsNullOrEmpty(prefix)) ? prefix + ": " + message : message;
                player.SendConsoleCommand("chat.add", string.Empty, chatUserID, msg);
            }
        }

        private void PopupMessage(string message) => PopupNotifications?.Call("CreatePopupNotification", message);

        private void UIMessage(BasePlayer player, string message)
        {
            if (player == null || !(player?.IsConnected ?? false) || string.IsNullOrEmpty(message)) return;
            bool replaced = false;
            float fadeIn = 0.2f;

            Timer playerTimer;

            timers.TryGetValue(player, out playerTimer);

            if (playerTimer != null && !playerTimer.Destroyed)
            {
                playerTimer.Destroy();
                fadeIn = 0.1f;

                replaced = true;
            }

            UIObject ui = new UIObject();

            ui.AddText("DeathNotice_DropShadow", SimpleUI_Left + 0.001, SimpleUI_Top + 0.001, SimpleUI_MaxWidth, SimpleUI_MaxHeight, deathNoticeShadowColor, StripTags(message), SimpleUI_FontSize, "Hud.Under", 3, fadeIn, 0.2f);
            ui.AddText("DeathNotice", SimpleUI_Left, SimpleUI_Top, SimpleUI_MaxWidth, SimpleUI_MaxHeight, deathNoticeColor, message, SimpleUI_FontSize, "Hud.Under", 3, fadeIn, 0.2f);

            ui.Destroy(player);

            if(replaced)
            {
                timer.Once(0.1f, () =>
                {
                    ui.Draw(player);

                    timers[player] = timer.Once(SimpleUI_HideTimer, () => ui.Destroy(player));
                });
            }
            else
            {
                ui.Draw(player);

                timers[player] = timer.Once(SimpleUI_HideTimer, () => ui.Destroy(player));
            }
        }

        #endregion
    }
}
