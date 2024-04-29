using Facepunch;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Rust;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Oxide.Plugins
{
    [Info("Luck", "Shady", "1.9.2", ResourceId = 0000)]
    [Description("Lets players level up as they harvest different resources and when crafting. -- extra GUI work by Shady, DO NOT REMOVE")] //extra everything work lmao
    public class Luck : RustPlugin
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

        //delivery jobs broke _banditConversationalist or will likely break it. please keep this in mind. FIX SOON.

        //use cl_punch for whirlwind fx? wonder if it can move in directions other than upper-right (june 2023 recommendation)

        //note feb 2024: has an old system for backpacks predating FP's backpacks.

        //A SORT OF TODO THING I NEED SOMEWHERE TO WRITE STUFF DOWN FOR NOW LOL
        //skt gui not refreshing (it used to??) - fixed
        //example: at woodcutting/mining level 50, get access to a luck skill specific to woodcutting/mining - but only accessible if lumberjack/miner, respectively - done for miner (level 25: map ores)
        //reminders that skills can be used again when off cooldown, like after hitting a tree a few times or something (done for wc/mining)

        //xp calculation method swapped over (done)
        //this has already largely been done, but Luck will move to a new XP calculation. ZLevels simply gets far less XP

        //note that bone frags require botanist (done)

        //research for loners?!?! duh (done, sort of) - needs ownerid fix

        //finish adding farmer legendary (overalls)

        //need to add checks for equipping legendary weapons (done)

        //ToA can be bought with a press button at class vm store? (done)

        //whirlwind end should undo the storm effects slower so it's not jarring and not accidentally triggered (done)

        //ww/wotw etc should probably require level 25 before any points can be spent on it? (like 25 wc) or something

        //legendary items probably need to be introduced better/implemented in a more user-accessible friendly way that sort of 'advertises' them. some sort of GUI, ala set items?

        //packrats should find guns with better durability (done? - needs testing, also, should be noted in GUI/info)

        //25 lvl cap for no class (done)

        //~20 minute grace period to change class again? (done, 45m)

        //class VMs need to be actually created (done)

        //solo classes need to actually be restricted to solo still (done)

        //skp price adjustments/level reqs (done, but idk if final)

        //fx when placing sleeping bag and chat notification for nomad
        //^ this is done

        //class set items, such as lumberjack clothing to get WC bonuses. think D2/D3 style

        //interesting fx: assets/bundled/prefabs/fx/item_unlock.prefab

        //soldier (mercenary class now) should also be able to use keycards more often?
        //maybe, but for now, packrats have this ability ^^ (done)

        //WORSE beds for nomads? (sleeping bags only good option)

        //lower TC costs may require harmony and it's questionable if that will even reflect upon the GUI as it should

        //class achievements/general achievements

        //switching class in grace period needs to reset stats!! (done)

        //charm gambler has his own spawn point, separate from everyone else. wears shady ( :^) ) clothing.

        private bool _initFailed = false;

        private readonly PropertyInfo _itemModWearable = typeof(ItemDefinition).GetProperty("ItemModWearable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private readonly FieldInfo _strainTimer = typeof(BaseFishingRod).GetField("strainTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private const string DISABLE_LUCK_CHAT_MESSAGES_PERM = nameof(Luck) + ".nochatmessages";
        private const string DISABLE_CLASS_NAME_IN_CHAT = nameof(Luck) + ".nochattitle";

        private const string PRISM_RAINBOW_TXT = "<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color>";

        private const string EGG_EXPLOSION_EFFECT = "assets/prefabs/misc/halloween/pumpkin_bucket/effects/eggexplosion.prefab";

        private const string CHAT_STEAM_ID = "76561198865536053";
        //private const string WIKI_LINK_TXT = "<color=#b9f542>For more information on the Luck skill, check out our wiki!</color> <color=#c35cff>prismatic.fandom.com</color>\n<color=#a5ff5c>You can also find a link to this wiki on our Discord. For more info on that, type <color=#ff5e97>/discord</color>!";

        private const string F1_HELP_MESSAGE = "<size=72><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color></size>\r\n\r\n<size=21>Welcome! There's a lot to unwrap with our custom ARPG experience.\r\nHere's a sort of 'quick-start' guide:</size>\r\n\r\n<color=#FF0030>1</color>: <color=#FF0030>To choose your own class you should use the command</color> \"/class gui\".\r\n<size=16>\u2022</size> <color=#FF0030>Be sure to <i>read</i> the various descriptions to choose the class that fits your style!</color>\r\n<size=16>\u2022</size> <color=#FF0030>If you make a mistake, you can change your class <i>ONCE</i> within 45 minutes of choosing it </color><size=12>(<color=#ff0066>you can access this menu at any time to read the descriptions</color>).</size>\r\n<size=16>\u2022</size> <color=#e60000>After that time, you'll only be able to choose a new class after forced wipe, or you can use</color> <i><color=#a300cc>the sacred way...</color></i>\r\n\r\n<color=#66ccff>2</color>: <color=#66ccff>As with any RPG experience, classes come with skills which you can upgrade</color>.\r\n<size=16>\u2022</size> <color=#66ccff>The command</color> \"<color=#ff66bf>/skt gui</color>\" <color=#66ccff>allows you to upgrade countless skills so you can make a character build of your own</color>.\r\n<size=16>\u2022</size> <color=#66ccff>Every class also has their own</color> <i><color=#ff66bf>special ultimate skill</color>.</i>\r\n<size=16>\u2022</size> <color=#66ccff>Class ultimates include <color=#0080ff>Whirlwind</color> <i>(<color=#e65c00>Lumberjack</color>)</i>, <color=#00cc00>Weight of the World</color> <i>(<color=#008ae6>Miner</color>)</i>, and more. To upgrade skills you need skill points, and are gained by leveling up your luck. More on that later</color>!\r\n\r\n<color=#BE3455>3</color>: <color=#BE3455>Now we get to the <color=#ff80bf><i><size=14>juicy</size></i></color> part, <color=#824C1A>Legendary</color> items</color>! <color=#BE3455>These very rare items can be incredibly powerful in the right hands</color>.\r\n<size=16>\u2022</size> <color=#BE3455>They can be found most commonly on scientists</color> <size=12><i>(<color=#BE3455>or players, whichever you prefer</color>).</i></size>\r\n<size=16>\u2022</size> <color=#BE3455>They offer unique, custom effects & boosts to certain skills</color>.\r\n<size=16>\u2022</size> <color=#BE3455>Some of them can be used by anyone, others are limited to specific classes</color>.\r\n<size=16>\u2022</size> <color=#BE3455>The command</color> \"<color=#BE3455>/leg</color>\" <color=#BE3455>allows you to view all currently added legendaries & their descriptions</color>.\r\n\r\n<color=#6667AB>4</color>: <color=#6667AB>Class shops</color>.\r\n<size=16>\u2022</size> <color=#6667AB>Every class has its own vending machine shop outside of Outpost</color>.\r\n<size=16>\u2022</size> <color=#6667AB>Players can trade class specific items & resources for batteries - PRISM's currency - or vice versa</color>.\r\n<size=16>\u2022</size> <color=#6667AB>It is also where you can use the <color=#a300cc><i>sacred way</i></color> to reset your class in the middle of the wipe</color>...\r\n\r\n<color=#FBDB65>5</color>: <color=#FBDB65>Resetting your class after the initial 45 minutes will require some work from you</color>.\r\n<size=16>\u2022</size> <color=#FBDB65>3 pure ore teas, 3 pure wood teas, 3 pure scrap teas & 5 skulls of the fiercest wolves should grant you the passage to freedom, once you press the button in the class shop & provide the required items, that is</color>.\r\n<size=16>\u2022</size> <color=#FBDB65>Aside from the cost of items, this freedom will also cost you knowledge</color>.\r\n<size=16>\u2022</size> <color=#FBDB65>Your class specific stats (Woodcutting, Mining, etc) & your class specific skill (Whirlwind, etc) will be reset, & the skill points you spent on it will be refunded</color>.\r\n\r\n<color=#307FE2>6</color>: <color=#307FE2>Leveling up your skills</color>.\r\n<size=16>\u2022</size> <color=#307FE2>As mentioned before, you need to gain skill points first</color>.\r\n<size=16>\u2022</size> <color=#307FE2>You gain them by leveling up your Luck, & in order to do that you have to pick up stuff from the ground or do activities specific to your own class (Woodcutting, etc)</color>.\r\n<size=16>\u2022</size> <color=#307FE2>As your level gets higher you will unlock more ways to level Luck, such as using the very same skills you just upgraded</color>.\r\n<size=16>\u2022</size> <color=#307FE2>Other activities, such as Woodcutting, Farming & Survival in general will also boost your gather rate as you become more proficient in those areas</color>.\r\n\r\n<color=#FABBCB>7</color>: <color=#FABBCB>Battery shops</color>.\r\n<size=14>\u2022</size> <color=#FABBCB>Everyone can access these!</color>\r\n<size=16>\u2022</size> <color=#FABBCB>They are spread around the monuments randomly during a wipe, and class-specific shops can be found outside Outpost</color>.\r\n<size=16>\u2022</size> <color=#FABBCB>Trade in your batteries for components or various other items</color>!\r\n<color=#FB637E><size=16>If anything else is unclear, feel free to ask us about it! discord.gg/DUCnZhZ\r\nYou can also type <i>\"<color=#F2F0A1>help</color>\"</i> here in the console for more info!</size></color>";

        private const string PRISM_BLUE_ICON_DISCORD = @"https://cdn.prismrust.com/PRISM_BLUE_LOGO_CROP_SOLID_SQUARED_UP_11_C3_test.jpg";

        //can we move these to some sort of faster image host? maybe.
        //   private const string PRISM_CLASS_SELECTS_URL = @"51.77.126.132/public/class_selects/jpg/"; //@"https://cdn.prismrust.com/class_selects/jpg/";
        //using discord as it is much faster

        private const string MUST_USE_WORKBENCH_REFILL_TOAST = "<color=#FF884C>You must be near a <color=#B7AF8B>Workbench</color> to refill!</color>";


        private const bool ERA_CORRUPTION = true;

        private Dictionary<string, string> ZLevelsColors = new();
        private Dictionary<string, object> ZLevelsMessages = new();

        private readonly HashSet<int> _ignoreItems = new() { 878301596, -2027988285, -44066823, -1364246987, 62577426, 1776460938, -17123659, 999690781, -810326667, 996757362, 1015352446, 1414245162, -1449152644, -187031121, 1768112091, 1770744540, -602717596, 1223900335, 1036321299, 204970153, 236677901, -399173933, 1561022037, 1046904719, 1394042569, -561148628, 861513346, -1366326648, -1884328185, 1878053256, 550753330, 696029452, 755224797, 1230691307, 1364514421, -455286320, 1762167092, -282193997, 180752235, -1386082991, 70102328, 22947882, 81423963, 1409529282, -1779180711, -277057363, -544317637, 1113514903 };

        private readonly HashSet<Coroutine> _coroutines = new();

        private static readonly System.Random _rarityRng = new();

        private static readonly System.Random xpRandom = new();
        private static readonly System.Random itemRandom = new();
        private static readonly System.Random scavRandom = new();

        private static readonly System.Random _charmTypeRng = new(); 

        //ww optimizations
        private readonly HitInfo _whirlwindImpactInfo = new();
        private readonly HitInfo _fellingHitInfo = new();
        private readonly HitInfo _whirlwindHitInfo = new();

        //lumberjack grenade optimization:

        private readonly HitInfo _lumberJackGrenadeHitInfo = new();

        private readonly DamageTypeEntry _whirlwindStabDmg = new() { type = DamageType.Stab };
        private readonly DamageTypeEntry _whirlwindSlashDmg = new() { type = DamageType.Slash };
        private readonly DamageTypeEntry _whirlwindBluntDmg = new() { type = DamageType.Blunt };

        private readonly List<DamageTypeEntry> _whirlwindDmgTypes = new(3);

        private readonly DamageTypeEntry _blunderbussBluntDmg = new() { type = DamageType.Blunt };


        private readonly HashSet<DroppedItem> allDropped = new();
        private readonly HashSet<LootContainer> allContainers = new();

        //fishes
        private readonly HashSet<ItemDefinition> _fishItems = new();
        private readonly HashSet<ItemId> _ignoreFish = new();

        private readonly HashSet<NetworkableId> _secondWindSleepingBags = new();
        private readonly HashSet<ulong> _secondWindThornsBonusActive = new();

        private readonly HashSet<int> _wolfHoundProjectileIDs = new();

        private readonly HashSet<int> _packratsBaneProjectileIDs = new();

        private readonly HashSet<int> _blunderBussProjectileIDs = new();

        private readonly HashSet<NetworkableId> _oreFireballs = new();

        private readonly HashSet<ItemId> _anglersBoonBaitItems = new();

        private Dictionary<string, ClassType> _userClass; //= new Dictionary<string, ClassType>(); //eventually we save this into a data file

        private LegendaryItemData _legendaryData;

        private NPCGearSetData _npcGearData;

        private List<StatInfo> _statInfos;

        private readonly HashSet<BasePlayer> _hasStormWeatherVars = new();

        private Dictionary<ulong, ulong> _itemUIDToBackPackContainerID;

        private readonly Dictionary<string, float> _lastMiningMarkerTime = new();

        private readonly Dictionary<string, int> _consecutiveGroundHits = new();

        private readonly Dictionary<string, List<OreResourceEntity>> _wotwPlayerOres = new();

        private readonly Dictionary<string, Vector3> _lastWotwHitPos = new();

        private readonly Dictionary<string, float> _lastWwReminder = new();
        private readonly Dictionary<string, float> _lastWotwReminder = new();

        private readonly Dictionary<string, BaseCombatEntity> _userBackpack = new();

        private readonly Dictionary<ItemId, NPCHitTarget.Tiers> _tagToTier = new();

        private readonly Dictionary<uint, HashSet<ulong>> _legendaryMapMarkerPlayers = new();

        private readonly Dictionary<string, float> _lastOverDriveReminder = new();

        private readonly Dictionary<string, float> _lastLegendaryNotice = new();

        private readonly Dictionary<string, float> _lastGuiUpdate = new();

        private readonly HashSet<NetworkableId> _cratesBeingRerolled = new();

        private readonly Dictionary<string, MetabolismSnapshot> _metabolicSnapshot = new();

        private readonly Dictionary<BasePlayer, BaseFishingRod> _playerToRodCache = new();

        private readonly Dictionary<string, Coroutine> _forgesCoroutine = new();

        private readonly Dictionary<string, TriggerTemperature> _forgesTrigger = new();

        private readonly HashSet<ulong> _overDrivePlayers = new();
        private readonly HashSet<ulong> _hasHitGui = new();

        private VehicleVendor _banditConversationalist = null;

        private static readonly System.Random charmLootRng = new();

        private static readonly System.Random charmXpRng = new();

        private const string TOKEN_INACTIVE_NAME = "Token of Absolution (Inactive)";
        private const string TOKEN_ACTIVE_NAME = "Token of Absolution (Active)";

        private const string BACKPACK_EQUIP_NOTE = "NOTE: You must equip your backpack to deposit items.";

        private const ulong TOKEN_INACTIVE_SKIN_ID = 2816921100;
        private const ulong TOKEN_ACTIVE_SKIN_ID = 2816925964;

        private const int WRAPPED_GIFT_ITEM_ID = 204970153;

        private const int SILENCER_ITEM_ID = -1850571427;

        private const int GAS_MASK_ITEM_ID = 1659114910;

        private const int BONE_FRAGMENTS_ITEM_ID = 1719978075;

        private const int ABYSS_ASSAULT_RIFLE_ITEM_ID = -139037392;

        private const ulong GENIE_LAMP_SKIN_ID_LUCK = 2939991120;

        private const ulong CORRUPTION_GLOVES_SKIN_ID = 2833922634;

        private const ulong LUMBERJACK_GRENADE_SKIN_ID = 836745325;
        private const ulong CLUSTER_GRENADE_SKIN_ID = 1163186435;

        private const ulong MOUSTACHE_COOKIE_SKIN_ID = 3043290470;

        private const ulong AESIRS_JACKET_SKIN_ID = 2843424058;

        private const ulong RUBY_STONE_HATCHET_SKIN_ID = 2138883683;

        private const ulong SMALL_CHARM_SKIN_ID_1 = 2829841447;
        private const ulong SMALL_CHARM_SKIN_ID_2 = 2829841540;
        private const ulong SMALL_CHARM_SKIN_ID_3 = 2829841593;

        private const ulong LARGE_CHARM_SKIN_ID_1 = 2829840753;
        private const ulong LARGE_CHARM_SKIN_ID_2 = 2829840858;
        private const ulong LARGE_CHARM_SKIN_ID_3 = 2829841032;


        private const ulong GRAND_CHARM_SKIN_ID_1 = 2829841251;
        private const ulong GRAND_CHARM_SKIN_ID_2 = 2829841319;
        private const ulong GRAND_CHARM_SKIN_ID_3 = 2829841370;

        private const ulong AMBER_HATCHET_SKIN_ID = 2637157054;

        private const ulong MINE_CAP_SKIN_ID = 788260164;

        private const ulong HUNTING_RIFLE_SKIN_ID = 2530302842;

        private const ulong GRISWOLDS_BLUNDERBUSS_SKIN_ID = 1209128595;

        private const ulong YESTERYEAR_SKIN_ID = 2296501936;

        private const ulong BURNING_FORGES_SKIN_ID = 796733487;

        private const ulong WINDFORCE_BOW_SKIN_ID = 1734427277;

        private const ulong TWITCHY_TOMMY_SKIN_ID = 2779119246;

        private const ulong GAMBLERS_PYTHON_SKIN_ID = 1258109891;

        private const ulong FANG_SAP_SKIN_ID = 1167255900;

        private const ulong ELECTRIC_GLOVES_SKIN_ID = 2714916868;

        private const ulong ANGLERS_BOON_SKIN_ID = 587812040;

        private const ulong ANGLERS_ANGST_SKIN_ID = 2557702256;

        private const ulong LONERS_BACKPACK_SKIN_ID = 2836989718;
        private const ulong LARGE_BACKPACK_SKIN_ID = 3016459319;
        private const ulong MEDIUM_BACKPACK_SKIN_ID = 855084816;
        private const ulong SMALL_BACKPACK_SKIN_ID = 2837964483;

        private const ulong CRAZY_EARL_SKIN_ID = 2786115544; //must also be a const as we want to be able to easily find this item repeatedly; it is not dynamic. it has an ability that must be coded in

        private const string SHOCK_EFFECT = "assets/prefabs/locks/keypad/effects/lock.code.shock.prefab";
        private const string LUCK_SHORTNAME = "LK";
        private const string CLASS_LOG_FILE_NAME = "Luck_class_log";

        private const int TREE_LAYER_MASK = 1073741824;
        private const int PLAYER_LAYER_MASK = 131072;

        private const int PYTHON_REVOLVER_ID = 1373971859;
        private const int REVOLVER_ID = 649912614;
        private const int PROTO_17_PISTOL_ID = 1914691295;
        private const int SEMI_AUTO_PISTOL_ID = 818877484;
        private const int CUSTOM_SMG_ID = 1796682209;
        private const int EOKA_PISTOL_ID = -75944661;
        private const int M92_PISTOL_ID = -852563019;

        private const int CHAINSAW_ID = 1104520648;
        private const int HATCHET_ID = -1252059217;
        private const int SALVAGED_AXE_ID = -262590403;
        private const int STONE_AXE_ID = -1583967946;
        private const int PROTO_AXE_ID = -399173933;
        private const int CONCRETE_AXE_ID = 1176355476;
        private const int ABYSS_AXE_ID = 1046904719;

        private const int BRONZE_EGG_ID = 844440409;
        private const int SILVER_EGG_ID = 1757265204;
        private const int GOLD_EGG_ID = -1002156085;

        private const int PURE_WOOD_TEA_ID = -557539629;
        private const int PURE_ORE_TEA_ID = 1729374708;
        private const int PURE_SCRAP_TEA_ID = 2024467711;

        private const int WOLF_SKULL_ID = 2048317869;

        private const int NOMAD_SUIT_ID = 491263800;
        private const ulong NOMAD_SUIT_SKIN_ID = 10201; //skin for nomad suit

        private const ulong BOOTS_OF_CLOUDWALKER_SKIN_ID = 2932448101;

        private const ulong LUCKY_GREASE_GUN_SKIN_ID = 2799637726;

        private const ulong BURLAP_TROUSERS_OVERALLS_SKIN_ID = 2039988322;
        private const ulong BURLAP_SHIRT_OVERALLS_SKIN_ID = 2039984110;
        private const ulong BOONIE_STRAW_HAT_SKIN_ID = 2037650796;

        private const int BURLAP_SHIRT_ITEM_ID = 602741290;
        private const int BURLAP_TROUSERS_ITEM_ID = 1992974553;
        private const int BOONIE_HAT_ITEM_ID = -23994173;

        private const int LUMBERJACK_HOODIE_ID = -763071910;
        private const int LUMBERJACK_SUIT_ID = 861513346;

        //LUMBERJACK SET (lower tier legendaries, really, but a set!):

        private const ulong LUMBERJACK_BEANIE_SKIN_ID = 10016;
        private const ulong LUMBERJACK_SHIRT_SKIN_ID = 802073199;
        private const ulong LUMBERJACK_PANTS_SKIN_ID = 1432967312;
        private const ulong LUMBERJACK_BOOTS_SKIN_ID = 1368418893;

        //Blacksmithing shirt:

        private const ulong STEELHEART_SKIN_ID = 1127407306;

        private const int MINING_HAT_ID = -1539025626;

        private const int DRACULA_CAPE_ID = -258574361;

        private const ulong NIMBLESTRIDERS_SKIN_ID = 1915955573;

        private const ulong SHOCK_ABSORBER_GLOVES_SKIN_ID = 2963716003;

        private const ulong MUDSLINGERS_SKIN_ID = 920390242;

        private const ulong UDDER_GLOVES_SKIN_ID = 2075536045;

        private const ulong PACKRATS_BANE_MUZZLE_BOOST_SKIN_ID = 2985792619; //1221

        private const ulong WOLFHOUND_SUPPRESSOR_SKIN_ID = 2938220613;

        private const int PICKAXE_ID = -1302129395;
        private const int STONE_PICKAXE_ID = 171931394;
        private const int ICE_PICK_ID = -1780802565;
        private const int JACKHAMMER_ID = 1488979457;
        private const int PROTO_PICK_ID = 236677901;
        private const int CONCRETE_PICK_ID = -1360171080;
        private const int ABYSS_PICK_ID = 1561022037;

        private const int GEIGER_COUNTER_ITEM_ID = 999690781;

        private const int RABBIT_MASK_ITEM_ID = -986782031; //todo: implement chance to gain a ds spin at any time while wearing

        private const int BATTERY_ID = 609049394;

        private const int WOOD_ITEM_ID = -151838493;
        private const int CHARCOAL_ITEM_ID = -1938052175;

        private const int SCRAP_ITEM_ID = -932201673;

        private const int SULFUR_ORE_ITEM_ID = -1157596551;
        private const int METAL_ORE_ITEM_ID = -4031221;
        private const int HQ_ORE_ITEM_ID = -1982036270;
        private const int STONES_ITEM_ID = -2099697608;

        private const int SULFUR_COOKED_ITEM_ID = -1581843485;
        private const int METAL_COOKED_ITEM_ID = 69511070;
        private const int HQ_COOKED_ITEM_ID = 317398316;

        private const uint FISH_TRAP_PREFAB_ID = 3119617183;

        private const uint OIL_BARREL_PREFAB_ID = 3438187947;

        private const uint CRATE_UNDERWATER_BASIC_PREFAB_ID = 3852690109;

        private const uint CRATE_ELITE_PREFAB_ID = 3286607235;
        private const uint CRATE_UNDERWATER_ELITE_PREFAB_ID = 96231181;
        private const uint CRATE_HELI_PREFAB_ID = 1314849795;
        private const uint CRATE_NORMAL_PREFAB_ID = 2857304752;
        private const uint CRATE_NORMAL_2_PREFAB_ID = 1546200557;
        private const uint CRATE_TOOLS_PREFAB_ID = 1892026534;
        private const uint CRATE_UNDERWATER_ADVANCED_PREFAB_ID = 2803512399;
        private const uint CRATE_UNDERWATER_NORMAL_PREFAB_ID = 1009499252;
        private const uint CRATE_UNDERWATER_NORMAL_2_PREFAB_ID = 2276830067;
        private const uint CRATE_UNDERWATER_TOOLS_PREFAB_ID = 3027334492;
        private const uint CRATE_BRADLEY_ID = 1737870479;
        private const uint CRATE_LOCKED_CHINOOK = 209286362;
        private const uint CRATE_LOCKED_OILRIG = 2043434947;


        private readonly ItemAmount _cachedSulfurItemAmount = new(ItemManager.FindItemDefinition(SULFUR_ORE_ITEM_ID), 1);
        private readonly ItemAmount _cachedMetalItemAmount = new(ItemManager.FindItemDefinition(METAL_ORE_ITEM_ID), 1);
        private readonly ItemAmount _cachedStonesItemAmount = new(ItemManager.FindItemDefinition(STONES_ITEM_ID), 1);
        private readonly ItemAmount _cachedHQMAmount = new(ItemManager.FindItemDefinition(HQ_ORE_ITEM_ID), 1);

        private readonly HashSet<ulong> _wokenPlayers = new();

        private readonly Dictionary<BasePlayer, Item> _lastActiveItem = new();

        private readonly Dictionary<string, float> _classGuiOnWakeTime = new();
        private readonly Dictionary<string, float> _classGuiStartTime = new();
        private readonly Dictionary<string, Action> _classGuiAction = new();
        private readonly Dictionary<string, ClassType> _classGuiLastClass = new();

        private readonly Dictionary<string, Coroutine> _transmuteCoroutine = new();

        private readonly Dictionary<string, HitInfo> _overdriveHitInfo = new();

        private readonly Dictionary<string, string> _currentSkillRotation = new();
        private readonly Dictionary<string, ClassType> _lastRotationType = new();

        private readonly Dictionary<NetworkableId, string> _corpseToAlivePrefabName = new();

        private readonly Dictionary<string, Action> _magpieCooldownStartAction = new();

        private readonly Dictionary<string, Action> _hitGuiAction = new();

        private Coroutine _saveCoroutine = null;

        private Timer _f1HelpTimer = null;

        private readonly HashSet<string> _forbiddenTags = new()
        { "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private bool _resetClassesOnInit = false;

        private Action _hitGenerateAction = null;

        private ItemDefinition _genericVehicleModuleDef = null;

        public ItemDefinition GenericVehicleModuleDef
        {
            get
            {
                if (_genericVehicleModuleDef == null)
                    _genericVehicleModuleDef = ItemManager.FindItemDefinition(878301596);

                return _genericVehicleModuleDef;
            }
        }

        private MonumentInfo _largeOilRig = null;
        public MonumentInfo LargeOilRig
        {
            get
            {
                if (_largeOilRig == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if (mon.displayPhrase.english.Contains("large oil rig", CompareOptions.OrdinalIgnoreCase))
                        {
                            _largeOilRig = mon;
                            break;
                        }
                    }
                }
                return _largeOilRig;
            }
        }

        private MonumentInfo _oilRig = null;
        public MonumentInfo SmallOilRig
        {
            get
            {
                if (_oilRig == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if (mon != LargeOilRig && mon.displayPhrase.english.Contains("oil rig", CompareOptions.OrdinalIgnoreCase))
                        {
                            _oilRig = mon;
                            break;
                        }
                    }
                }
                return _oilRig;
            }
        }

        [PluginReference]
        private readonly Plugin Compilation; //temp

        [PluginReference]
        private readonly Plugin RadRain;

        [PluginReference]
        private readonly Plugin ZLevelsRemastered;

        [PluginReference]
        private readonly Plugin BuildLimiter;

        [PluginReference]
        private readonly Plugin SkinsAPI;

        // [PluginReference]
        // private readonly Plugin NoobBias;

        [PluginReference]
        private readonly Plugin ZZTaxChest;

        [PluginReference]
        private readonly Plugin LocalStatsAPI;

        [PluginReference]
        private readonly Plugin LevelsGUI;

        [PluginReference]
        private readonly Plugin PRISMAchievements;

        [PluginReference]
        private readonly Plugin DiscordAPI2;

        [PluginReference]
        private readonly Plugin Deathmatch;

        [PluginReference]
        private readonly Plugin FindGround;


        private ItemDefinition _woodDefinition = null;
        private ItemDefinition WoodDefinition
        {
            get
            {
                if (_woodDefinition == null) _woodDefinition = ItemManager.FindItemDefinition(-151838493);
                return _woodDefinition;
            }
        }

        private ItemDefinition _noteDefinition = null;
        private ItemDefinition NoteDefinition
        {
            get
            {
                if (_noteDefinition == null) _noteDefinition = ItemManager.FindItemDefinition(1414245162);
                return _noteDefinition;
            }
        }

        private EnvSync _envSync = null;

        public EnvSync EnvSync
        {
            get
            {
                if (_envSync == null)
                {
                    foreach (var entity in BaseNetworkable.serverEntities)
                    {
                        var sync = entity as EnvSync;
                        if (sync != null)
                        {
                            _envSync = sync;
                            break;
                        }
                    }
                }

                return _envSync;
            }
        }

        private TimeSpan SaveSpan { get { return DateTime.UtcNow - SaveRestore.SaveCreatedTime; } }

        public enum SkillTree { Undefined, Resource, NonCombat, Combat, Classes};

        private bool init = false;

        private void TryLoadZLevelsColors() => ZLevelsColors = ZLevelsRemastered?.Call<Dictionary<string, string>>("getColors") ?? null;
        private void TryLoadZLevelsMessages() => ZLevelsMessages = ZLevelsRemastered?.Call<Dictionary<string, object>>("getMessages") ?? null;

        public const string LUCK_COLOR = "#5DC540";
        public const string LUCK_COLOR_FORMATTED = "<color=#5DC540>";

        private string GetLuckColor() { return LUCK_COLOR; }

        private MetabolismSnapshot GetMetabolicSnapshot(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            return _metabolicSnapshot.TryGetValue(userId, out MetabolismSnapshot snap) ? snap : null;
        }

        //if performance becomes an issue, only take snapshots of players wearing the yesteryear legendary.
        private IEnumerator TakeMetabolicSnapshotsOfAllPlayers(TimeSpan timeInBetweenSnapshots)
        {
            PrintWarning(nameof(TakeMetabolicSnapshotsOfAllPlayers) + " initiated with TimeSpan: " + timeInBetweenSnapshots);

            while(true)
            {
                var players = Pool.Get<HashSet<BasePlayer>>();

                try
                {
                    players.Clear();

                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                        players.Add(BasePlayer.activePlayerList[i]);

                    foreach (var p in players)
                    {
                        if (p == null || p.IsDestroyed || !p.IsConnected)
                            continue;

                        var existingSnap = GetMetabolicSnapshot(p.UserIDString);

                        if (existingSnap != null) 
                            MetabolismSnapshot.UpdateSnapshot(p, existingSnap); 
                        else 
                            _metabolicSnapshot[p.UserIDString] = MetabolismSnapshot.TakeSnapshot(p);
                        

                        yield return CoroutineEx.waitForSecondsRealtime(0.05f);
                    }

                }
                finally { Pool.Free(ref players); }

                yield return CoroutineEx.waitForSecondsRealtime((float)timeInBetweenSnapshots.TotalSeconds);
            }
        }

        private class MetabolismSnapshot //for use in legendary yesteryear
        {
            public float Health { get; set; }
            public float Calories { get; set; }
            public float Hydration { get; set; }
            public float RadiationLevel { get; set; }
            public float RadiationPoisoning { get; set; }
            public float Bleeding { get; set; }
            public float Oxygen { get; set; }
            public float Wetness { get; set; }
            public bool Wounded { get; set; }

            public DateTime SnapshotTime { get; set; }

            public TimeSpan TimeSinceTaken
            {
                get
                {
                    return DateTime.UtcNow - SnapshotTime;
                }
            }

            public bool HasBeenApplied { get; set; } = false;

            public DateTime LastApplied { get; set; } = DateTime.MinValue;

            public MetabolismSnapshot()
            {
                SnapshotTime = DateTime.UtcNow;
            }

            public MetabolismSnapshot(BasePlayer player)
            {
                if (player?.metabolism == null)
                    throw new ArgumentNullException(nameof(player.metabolism));

                SnapshotTime = DateTime.UtcNow;

                var metabolism = player.metabolism;

                Health = player.Health();
                Calories = metabolism.calories.value;
                Hydration = metabolism.hydration.value;
                Bleeding = metabolism.bleeding.value;
                RadiationLevel = metabolism.radiation_level.value;
                RadiationPoisoning = metabolism.radiation_poison.value;
                Wetness = metabolism.wetness.value;
                Oxygen = metabolism.oxygen.value;
                Wounded = player.IsWounded();
            }

            public static MetabolismSnapshot TakeSnapshot(BasePlayer player)
            {
                return new MetabolismSnapshot(player);
            }

            public static MetabolismSnapshot UpdateSnapshot(BasePlayer player, MetabolismSnapshot snap)
            {
                if (player?.metabolism == null)
                    throw new ArgumentNullException(nameof(player.metabolism));

                if (snap == null)
                    throw new ArgumentNullException(nameof(snap));


                snap.SnapshotTime = DateTime.UtcNow;
                snap.HasBeenApplied = false;
                snap.LastApplied = DateTime.MinValue;

                var metabolism = player.metabolism;

                snap.Health = player.Health();
                snap.Calories = metabolism.calories.value;
                snap.Hydration = metabolism.hydration.value;
                snap.Bleeding = metabolism.bleeding.value;
                snap.RadiationLevel = metabolism.radiation_level.value;
                snap.RadiationPoisoning = metabolism.radiation_poison.value;
                snap.Wetness = metabolism.wetness.value;
                snap.Oxygen = metabolism.oxygen.value;
                snap.Wounded = player.IsWounded();

                return snap;
            }

            public static void ApplyMetabolism(BasePlayer player, MetabolismSnapshot snap, bool forcePlayerUpdate = true)
            {
                if (player?.metabolism == null)
                    throw new ArgumentNullException(nameof(player.metabolism));

                if (snap.HasBeenApplied)
                    Interface.Oxide.LogWarning(nameof(ApplyMetabolism) + " called with snapshot that has HasBeenApplied!");

                snap.HasBeenApplied = true;
                snap.LastApplied = DateTime.UtcNow;

                if (snap.Health <= 0f)
                {
                    Interface.Oxide.LogWarning(nameof(ApplyMetabolism) + " had 0 or lower HP at metabolic snapshot - killing player!");

                    player.Hurt(9999f, DamageType.Suicide, player, false);

                    return;
                }

                if (snap.Wounded)
                    player.BecomeWounded(new HitInfo(player, player, DamageType.Generic, 999f));
                else player.StopWounded();

                player.SetHealth(Mathf.Clamp(snap.Health, 0.1f, player.MaxHealth()));

                var metabolism = player.metabolism;

                metabolism.calories.value = snap.Calories;
                metabolism.hydration.value = snap.Hydration;
                metabolism.radiation_level.value = snap.RadiationLevel;
                metabolism.radiation_poison.value = snap.RadiationPoisoning;
                metabolism.bleeding.value = snap.Bleeding;
                metabolism.oxygen.value = snap.Oxygen;
                metabolism.wetness.value = snap.Wetness;

                if (forcePlayerUpdate)
                { 
                    metabolism.SendChangesToClient();
                    player.SendNetworkUpdate();
                }
            }

        }

        private class WildTrapExt : FacepunchBehaviour
        {
            private WildlifeTrap _trap;
            public WildlifeTrap Trap
            {
                get
                {
                    if (_trap == null || (_trap?.IsDestroyed ?? true))
                    {
                        DoDestroy();
                        return null;
                    }
                    else return _trap;
                }
                set { _trap = value; }
            }
            public float TickRate
            {
                get { return Trap?.tickRate ?? -1; }
                set
                {
                    if (Trap == null) return;
                    Trap.tickRate = value;
                    ResetInvoke();
                }
            }

            public float DecayScale = 0.1f;

            private void Awake()
            {
                Trap = GetComponent<WildlifeTrap>();
                if (Trap == null)
                {
                    Interface.Oxide.LogError("WildTrapExt.Awake() called with null WildlifeTrap!!");
                    DoDestroy();
                    return;
                }
                Trap?.CancelInvoke(Trap.TrapThink);
                ResetInvoke();
            }

            private void ResetInvoke()
            {
                Trap.CancelInvoke(TrapThink);
                Trap.InvokeRepeating(TrapThink, (float)(Trap.tickRate * 0.800000011920929 + Trap.tickRate * (double)Random.Range(0.0f, 0.4f)), Trap.tickRate);
            }

            public void TrapThink()
            {
                if (Trap == null) return;
                var baitCalories = Trap?.GetBaitCalories() ?? 0;
                if (baitCalories < 1) return;
                var randomWildlife = Trap?.GetRandomWildlife() ?? null;
                if (baitCalories < randomWildlife.caloriesForInterest || Random.Range(0.0f, 1f) > (double)randomWildlife.successRate) return;

                Trap?.UseBaitCalories(randomWildlife.caloriesForInterest);

                if (Random.Range(0.0f, 1f) > (double)Trap?.trapSuccessRate) return;
                TrapWildlife(randomWildlife);
            }

            public void TrapWildlife(TrappableWildlife trapped)
            {
                if (trapped == null || Trap == null) return;
                var obj = ItemManager.Create(trapped.inventoryObject, Random.Range(trapped.minToCatch, trapped.maxToCatch + 1), 0UL);
                if (!obj.MoveToContainer(Trap.inventory, -1, true) && !obj.Drop(Trap.GetDropPosition(), Trap.GetDropVelocity()))
                {
                    Interface.Oxide.LogWarning("Couldn't move obj to inventory OR drop!");
                    obj.Remove(0.0f);
                }
                else Trap.SetFlag(BaseEntity.Flags.Reserved1, true, false);
                Trap.SetTrapActive(false);
                Trap.Hurt(Trap.StartMaxHealth() * DecayScale, DamageType.Decay, null, false);
            }

            public void DoDestroy()
            {
                _trap?.CancelInvoke(TrapThink);
                Destroy(this);
            }
        }

        public static Luck Instance;

        public class StatInfo
        {
            public string ShortName { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;

            public string ShortDescription { get; set; } = string.Empty;
            public string LongDescription { get; set; } = string.Empty;

            public bool Hide { get; set; } = false;

            public SkillTree AppropriateTree { get; set; } = SkillTree.Undefined;

            public ClassType AppropriateClass { get; set; } = ClassType.Undefined;

            public bool MustMatchClass { get; set; } = false;

            public int MaxLevel { get; set; } = 0;

            public Dictionary<int, int> UpgradeCost { get; set; } = new();

            public Dictionary<int, int> RequiredLevelToUpgrade { get; set; } = new();

            public Dictionary<string, Dictionary<int, int>> RequiredStatsToUpgrade { get; set; } = new();

            [JsonIgnore]
            private static readonly Dictionary<string, StatInfo> _shortStatNameToStatInfo = new();

            public static void UpdateAllShortNames(List<StatInfo> infos)
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];
                    _shortStatNameToStatInfo[info.ShortName] = info;
                }
            }

            public static StatInfo GetStatInfoFromShortName(string shortName)
            {

                if (!_shortStatNameToStatInfo.TryGetValue(shortName, out StatInfo stat))
                {
                    Interface.Oxide.LogWarning(nameof(GetStatInfoFromShortName) + " returned false on dictionary check for " + shortName); //i want to try and optimize all things to get the correct name, and I think they already do, but this is to make sure (even though it is good to have the failsafe below)

                    foreach (var kvp in _shortStatNameToStatInfo)
                    {
                        if (kvp.Key.Equals(shortName, StringComparison.OrdinalIgnoreCase))
                        {
                            stat = kvp.Value;
                            break;
                        }
                    }

                    if (stat == null)
                    {
                        Interface.Oxide.LogWarning(nameof(GetStatInfoFromShortName) + " is about to return null for shortName: " + shortName);
                        return null;
                    }
                }

                return stat;
            }

            public int GetRequiredLevelToUpgrade(int currentLevel)
            {
                if (currentLevel >= MaxLevel || RequiredLevelToUpgrade == null || RequiredLevelToUpgrade.Count < 1) return -1;

                foreach (var kvp in RequiredLevelToUpgrade)
                {
                    if (currentLevel < kvp.Key)
                        return kvp.Value;
                }

                foreach (var kvp in RequiredLevelToUpgrade) //basically a default
                    return kvp.Value;

                //side note: it's too bad the compiler can't realize that we're checking to ensure requiredleveltoupgrade is not null and has at least one entry, because if it knew, it'd know we are ALWAYS returning a value and the return below isn't actually necessary
                //the only case in which it may be necessary (?) would be if this was threaded, I suppose?

                return -1;
            }

            public int GetRequiredStatLevelToUpgrade(string statName, int currentLvl)
            {
                if (string.IsNullOrWhiteSpace(statName)) throw new ArgumentNullException(nameof(statName));

                Interface.Oxide.LogWarning(nameof(GetRequiredStatLevelToUpgrade) + " for: " + statName + " currentLvl " + currentLvl);


                if (!RequiredStatsToUpgrade.TryGetValue(statName, out Dictionary<int, int> desiredDictionary))
                {
                    foreach (var kvp in RequiredStatsToUpgrade)
                    {
                        Interface.Oxide.LogWarning(nameof(RequiredStatsToUpgrade) + " iterator: " + kvp.Key + " : " + kvp.Value);

                        var key = kvp.Key;

                        if (key.Equals(statName, StringComparison.OrdinalIgnoreCase))
                        {
                            desiredDictionary = kvp.Value;
                            break;
                        }
                    }
                }



                if (desiredDictionary == null || desiredDictionary.Count < 1)
                {
                    Interface.Oxide.LogWarning(nameof(desiredDictionary) + " is null!!");
                    return -1;
                }

                var correctCostLvlKey = -1;

                foreach (var kvp in desiredDictionary)
                {
                    Interface.Oxide.LogWarning("(2) key: " + kvp.Key + ", val: " + kvp.Value + ", currentLvl >= key?: " + (currentLvl >= kvp.Key));
                    if (currentLvl >= kvp.Key) correctCostLvlKey = kvp.Key;
                }

                if (correctCostLvlKey < 1)
                {
                    Interface.Oxide.LogWarning(nameof(correctCostLvlKey) + " was < 1, so we set it to 1. the user was probably at level 0 himself (2)");
                    correctCostLvlKey = 1;
                }


                if (!desiredDictionary.TryGetValue(correctCostLvlKey, out int requiredLvl)) Interface.Oxide.LogWarning("(2) couldn't get upgrade cost from key: " + correctCostLvlKey);
                else Interface.Oxide.LogWarning("(2) for lvl " + currentLvl + " we got key: " + correctCostLvlKey + " which has val: " + requiredLvl);

                return requiredLvl;
            }

            public StatInfo() { }

        }


        private class LuckInfo
        {

            public Dictionary<string, int> StatLevels { get; set; } = new Dictionary<string, int>();

            public void PopulateAllStatLevels()
            {
                var stats = Instance?._statInfos;
                if (stats == null || stats.Count < 1)
                {
                    Interface.Oxide.LogWarning(nameof(PopulateAllStatLevels) + " has stats null/count < 1");
                    return;
                }


                for (int i = 0; i < stats.Count; i++)
                {
                    var stat = stats[i];
                    if (!StatLevels.TryGetValue(stat.ShortName, out int val)) StatLevels[stat.ShortName] = 0;
                }

            }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool BackpackAutoDeposit { get; set; } = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool HasUsedFreeReset { get; set; } = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(1L)]
            public long Level { get; set; } = 1;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0f)]
            public float XP { get; set; } = 0;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public int SkillPoints { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public int Spent { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public long LastConnectionTime { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(-1L)]
            public long GracePeriodStartTime { get; set; } = -1L;

            /// <summary>
            /// Reduces skill points by the amount and increases spent by the same amount. Will return false if skill points is less than amount.
            /// </summary>
            /// <param name="amount"></param>
            public bool SpendPoints(int amount, bool noGuiUpdate = false)
            {
                if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));
                if (SkillPoints < amount) return false;

                Spent += amount;
                SkillPoints -= amount;

                if (!noGuiUpdate && Player != null && Player.IsConnected)
                    Player?.Invoke(() => Instance?.GUIUpdate(Player), 0.01f);


                return true;
            }

            public void ResetAllClassStats()
            {
                var stats = Pool.GetList<StatInfo>();
                try
                {
                    foreach (var kvp in StatLevels)
                    {

                        var stat = GetStatInfoByName(kvp.Key);
                        if (stat != null) stats.Add(stat);
                    }

                    for (int i = 0; i < stats.Count; i++)
                    {
                        var stat = stats[i];

                        if (stat.AppropriateClass != ClassType.Undefined && stat.MustMatchClass) StatLevels[stat.ShortName] = 0;
                    }

                }
                finally { Pool.FreeList(ref stats); }
            }

            public void ResetAllStats()
            {
                var stats = Pool.GetList<StatInfo>();
                try
                {
                    foreach (var kvp in StatLevels)
                    {

                        var stat = GetStatInfoByName(kvp.Key);
                        if (stat != null) stats.Add(stat);
                    }

                    for (int i = 0; i < stats.Count; i++)
                        StatLevels[stats[i].ShortName] = 0;


                }
                finally { Pool.FreeList(ref stats); }
            }


            [JsonIgnore]
            private BasePlayer _player = null;

            [JsonIgnore]
            public BasePlayer Player
            {
                get
                {
                    if (_player == null) _player = RelationshipManager.FindByID(UserID);

                    return _player;
                }
            }

            //changing everything to use this will take decades lmao lesss goooo
            //it was worth it :)
            public int GetStatLevelByName(string statName, bool callHook = true)
            {
                if (string.IsNullOrEmpty(statName))
                    throw new ArgumentNullException(nameof(statName));


                var statInfo = GetStatInfoByName(statName);

                if (statInfo == null)
                {
                    Interface.Oxide.LogWarning("statInfo null on " + nameof(GetStatLevelByName) + " with name: " + statName + "!!");
                    return -1;
                }

                return GetStatLevel(statInfo, callHook);
            }

            public void SetStatLevel(StatInfo statInfo, int desiredLevel)
            {
                if (statInfo == null)
                    throw new ArgumentNullException(nameof(statInfo));

                SetStatLevelByName(statInfo.ShortName, desiredLevel);
            }


            public void SetStatLevelByName(string statName, int desiredLevel)
            {
                if (string.IsNullOrEmpty(statName))
                    throw new ArgumentNullException(nameof(statName));

                StatLevels[statName] = desiredLevel;
            }

            public int AddStatLevel(StatInfo statInfo, int lvlsToAdd)
            {
                if (statInfo == null)
                    throw new ArgumentNullException(nameof(statInfo));

                return AddStatLevelByName(statInfo.ShortName, lvlsToAdd);
            }

            public int AddStatLevelByName(string statName, int lvlsToAdd)
            {
                if (string.IsNullOrEmpty(statName))
                    throw new ArgumentNullException(nameof(statName));
                if (!StatLevels.TryGetValue(statName, out _)) StatLevels[statName] = lvlsToAdd;
                else StatLevels[statName] += 1;

                return StatLevels[statName];
            }

            public int GetStatLevel(StatInfo statInfo, bool callHook = true)
            {
                if (statInfo == null)
                    throw new ArgumentNullException(nameof(statInfo));

                var val = -1;

                if (!StatLevels.TryGetValue(statInfo.ShortName, out val))
                {
                    Interface.Oxide.LogWarning(nameof(GetStatLevel) + "could NOT get StatLevels from ShortName!!: " + statInfo.ShortName);

                    foreach (var kvp in StatLevels)
                    {
                        var stat = kvp.Key;
                        if (stat.Equals(statInfo.ShortName, StringComparison.OrdinalIgnoreCase))
                        {
                            val = kvp.Value;
                            break;
                        }
                    }
                }

               

                if (callHook)
                {
                    var hookVal = Interface.Oxide.CallHook("OnGetLuckStat", UserID, statInfo, val);
                    if (hookVal != null)
                        val = (int)hookVal;
                }


                return val;
            }

            public int GetStatUpgradeCostByName(string statName, int desiredLevelToUpgrade)
            {
                if (string.IsNullOrEmpty(statName))
                    throw new ArgumentNullException(nameof(statName));

                return GetStatUpgradeCost(GetStatInfoByName(statName), desiredLevelToUpgrade);
            }

            public int GetStatUpgradeCost(StatInfo stat, int desiredLevelToUpgrade)
            {
                if (stat?.UpgradeCost == null || stat.UpgradeCost.Count < 1)
                {
                    Interface.Oxide.LogError("stat has no upgrade cost!!!");
                    return -1;
                }


                if (stat.UpgradeCost.TryGetValue(desiredLevelToUpgrade, out int cost))
                    return cost;

                var currentLvl = GetStatLevel(stat, false);


                var correctCostLvlKey = -1;

                foreach (var kvp in stat.UpgradeCost)
                    if (currentLvl >= kvp.Key) correctCostLvlKey = kvp.Key;


                return stat.UpgradeCost[correctCostLvlKey < 1 ? 1 : correctCostLvlKey];
            }

            public StatInfo GetStatInfoByName(string shortName)
            {
                if (string.IsNullOrWhiteSpace(shortName))
                    throw new ArgumentNullException(nameof(shortName));

                return StatInfo.GetStatInfoFromShortName(shortName);
            }

            public static void GetAllStatNamesNoAlloc(ref List<string> list)
            {
                if (list == null)
                    throw new ArgumentNullException(nameof(list));

                list.Clear();

                var stats = Instance?._statInfos;
                if (stats == null || stats.Count < 1)
                {
                    Interface.Oxide.LogWarning("statInfos is null/count < 1!");
                    return;
                }

                for (int i = 0; i < stats.Count; i++)
                    list.Add(stats[i].ShortName);

            }

            public static List<string> GetAllStatNames()
            {
                var list = new List<string>();

                GetAllStatNamesNoAlloc(ref list);

                return list;
            }

            public int GetPointsSpentOnClassStats()
            {
                var total = 0;
                foreach (var stat in StatLevels)
                {
                    var statName = stat.Key;
                    var curLvl = stat.Value;

                    if (curLvl < 1) continue;

                    var info = GetStatInfoByName(statName);

                    if (info == null)
                    {
                        Interface.Oxide.LogWarning("stat info was null for statName: " + statName);
                        continue;
                    }

                    if (!info.MustMatchClass) continue;



                    for (int i = 0; i < curLvl; i++)
                        total += GetStatUpgradeCost(info, i + 1);
                }

                Interface.Oxide.LogWarning(nameof(GetPointsSpentOnClassStats) + " got final total: " + total);


                return total;
            }

            public int GetPointsSpentOnStat(StatInfo statInfo)
            {
                return GetPointsSpentByLevel(statInfo, GetStatLevel(statInfo, false));
            }

            public int GetPointsSpentByLevel(StatInfo statInfo, int level)
            {
                var total = 0;

                var curLvl = GetStatLevel(statInfo, false);

                for (int i = 0; i < level; i++)
                    total += GetStatUpgradeCost(statInfo, i + 1);


                Interface.Oxide.LogWarning(nameof(GetPointsSpentByLevel) + " (" + statInfo + ") got final total: " + total);

                return total;
            }

            public ulong UserID { get; set; }

            public LuckInfo()
            {
                PopulateAllStatLevels();
            }

            public LuckInfo(BasePlayer newPlayer)
            {
                UserID = newPlayer?.userID ?? 0;
                PopulateAllStatLevels();
            }

            public LuckInfo(string userID)
            {
                if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
                if (ulong.TryParse(userID, out ulong newUID)) UserID = newUID;

                PopulateAllStatLevels();
            }

            public LuckInfo(ulong userID)
            {
                UserID = userID;
                PopulateAllStatLevels();
            }

        }

        private LuckInfo GetLuckInfo(BasePlayer player) { return GetLuckInfo(player?.UserIDString ?? string.Empty); }

        private LuckInfo GetLuckInfo(ulong userID) { return GetLuckInfo(userID.ToString()); }

        private LuckInfo GetLuckInfo(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return null;
            if (storedData3?.luckInfos != null && storedData3.luckInfos.TryGetValue(userID, out LuckInfo luckInfo)) return luckInfo;
            else return null;
        }

        private void SetLuckInfo(string userId, LuckInfo info)
        {
            if (string.IsNullOrEmpty(userId)) return;
            if (storedData3?.luckInfos != null) storedData3.luckInfos[userId] = info;
        }

        private void SetLuckInfo(ulong userId, LuckInfo info) => SetLuckInfo(userId.ToString(), info);


        private CharmsData charmsData;
        private readonly Dictionary<Charm, Item> _charmToItemCache = new();
        private class CharmsData
        {
            public Dictionary<ulong, Charm> charms = new();


            public Charm GetCharm(ulong itemID)
            {
                return charms.TryGetValue(itemID, out Charm charm) ? charm : null;
            }

            public Charm GetCharm(Item item) { return GetCharm(item?.uid.Value ?? 0); }

            public CharmsData() { }
        }

        private class Charm
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0.0f)]
            public float xpBonus = 0.0f;


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0.0f)]
            public float itemBonus = 0.0f;


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0.0f)]
            public float chanceBonus = 0.0f;


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0.0f)]
            public float lootBonus = 0.0f;


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0.0f)]
            public float craftBonus = 0.0f;


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public CharmSkill Skill = CharmSkill.Undefined;


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public CharmType Type = CharmType.Undefined;


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public Rarity lootRarity = Rarity.None;

            //public Item GetItem() { return (luck == null || itemID == 0) ? null : luck.FindItemByID(itemID.ToString()); }

            public Charm(CharmSkill skill, CharmType cType, float xp = 0.0f, float gather = 0.0f, float chance = 0.0f, float craft = 0.0f, float loot = 0.0f, Rarity rarity = Rarity.None)
            {
                if (skill <= CharmSkill.Undefined)
                    throw new ArgumentOutOfRangeException(nameof(skill));

                Skill = skill;
                xpBonus = xp;
                itemBonus = gather;
                chanceBonus = chance;
                craftBonus = craft;
                lootBonus = loot;
                lootRarity = rarity;
                Type = cType;
            }

            public Charm() { }

            public override string ToString()
            {
                var sb = Pool.Get<StringBuilder>();
                try { return sb.Clear().Append(Skill).Append("/").Append(xpBonus).Append("/").Append(itemBonus).Append("/").Append(chanceBonus).Append("/").Append(lootRarity).Append("/").Append(Type).ToString(); }
                finally { Pool.Free(ref sb); }
            }
        }

        public enum CharmType { Undefined, Small, Large, Grand };

        public enum CharmSkill { Undefined, Woodcutting, Mining, Survival, Luck, Loot, Crafting };

        private Item CreateCharm(CharmSkill skill, CharmType type, float xpBonus = 0f, float itemBonus = 0f, float chanceBonus = 0f, float craftBonus = 0.0f, float lootBonus = 0f, Rarity rarity = Rarity.None)
        {
            if (skill <= CharmSkill.Undefined)
                throw new ArgumentOutOfRangeException(nameof(skill));

            if (xpBonus == 0.0f && itemBonus == 0.0f && chanceBonus == 0.0f && craftBonus == 0.0f && lootBonus == 0.0f) throw new ArgumentOutOfRangeException();

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var rng = Random.Range(0, 3);
                var skin = type == CharmType.Grand ? (rng == 0 ? GRAND_CHARM_SKIN_ID_1 : rng == 1 ? GRAND_CHARM_SKIN_ID_2 : GRAND_CHARM_SKIN_ID_3) : type == CharmType.Large ? (rng == 0 ? LARGE_CHARM_SKIN_ID_1 : rng == 1 ? LARGE_CHARM_SKIN_ID_2 : LARGE_CHARM_SKIN_ID_2) : (rng == 0 ? SMALL_CHARM_SKIN_ID_1 : rng == 1 ? SMALL_CHARM_SKIN_ID_2 : SMALL_CHARM_SKIN_ID_3);

                var newItem = ItemManager.Create(NoteDefinition, 1, skin);
                newItem.name = sb.Clear().Append(type).Append(" Charm of ").Append(skill).ToString();


                //   if (type == CharmType.Large) newItem.name = "<color=#5E5197>" + newItem.name + "</color>";
                //     if (type == CharmType.Grand) newItem.name = "<color=#C6D182>" + newItem.name + "</color>"; //BROKEN IN RUST

                if (xpBonus != 0.0f)
                {
                    if (sb.Length > 0) sb.Append(Environment.NewLine);
                    sb.Append((xpBonus > 0f) ? "Increases" : "Decreases").Append(" ").Append(skill).Append(" XP by ").Append(xpBonus.ToString("N1").Replace(".0", string.Empty)).Append("%");
                }
                if (itemBonus != 0.0f)
                {
                    if (sb.Length > 0) sb.Append(Environment.NewLine);
                    sb.Append((itemBonus > 0f) ? "Increases" : "Decreases").Append(" ").Append(skill).Append(" item gathering by ").Append(itemBonus.ToString("N1").Replace(".0", string.Empty)).Append("%");
                }
                if (chanceBonus != 0.0f)
                {
                    if (sb.Length > 0) sb.Append(Environment.NewLine);
                    sb.Append((chanceBonus > 0f) ? "Increases" : "Decreases").Append(" ").Append(skill).Append(" chance by ").Append(chanceBonus.ToString("N1").Replace(".0", string.Empty)).Append("%");
                }
                if (lootBonus != 0.0f)
                {
                    if (sb.Length > 0) sb.Append(Environment.NewLine);
                    sb.Append((lootBonus > 0.0f) ? "Increases" : "Decreases").Append(" chance to find ").Append(rarity == Rarity.VeryRare ? "Very Rare" : rarity.ToString()).Append(" loot by ").Append(lootBonus.ToString("N1").Replace(".0", string.Empty)).Append("%");
                }
                if (craftBonus != 0.0f)
                {
                    if (sb.Length > 0) sb.Append(Environment.NewLine);
                    sb.Append(craftBonus > 0.0f ? "Increases" : "Decreases").Append(" crafting speed by ").Append(craftBonus.ToString("N1").Replace(".0", string.Empty)).Append("%");
                }
                sb.Append(Environment.NewLine).Append("Keep in inventory to gain bonus");

                var slotCount = type == CharmType.Large ? 3 : type == CharmType.Grand ? 4 : 0;
                if (slotCount > 0) sb.Append(Environment.NewLine).Append("Requires an additional ").Append(slotCount).Append(" empty slots");

                newItem.text = sb.ToString();

                var newCharm = new Charm(skill, type, xpBonus, itemBonus, chanceBonus, craftBonus, lootBonus, rarity);

                if (charmsData?.charms != null) charmsData.charms[newItem.uid.Value] = newCharm;

                _charmToItemCache[newCharm] = newItem;

                return newItem;
            }
            finally { Pool.Free(ref sb); }


        }

        private int _classEnumCount = -1;
        public int ClassEnumCount
        {
            get
            {
                if (_classEnumCount == -1)
                    _classEnumCount = Enum.GetValues(typeof(ClassType)).Length;

                return _classEnumCount;
            }
        }
        public enum ClassType { Undefined, Nomad, Miner, Lumberjack, Loner, Farmer, Packrat, Mercenary };
        public enum XPReason { Generic, Loot, Ore, Tree, HarvestPlant, PvEKill, PvEHit, Fish, Pickup };

        private class WhirlwindCooldownHandler : PlayerCooldownHandler
        {

            public override float GetCooldownSeconds()
            {
                if (Player == null || !Player.IsConnected || Player.IsDead() || Player.IsSleeping()) return -1;

                var reduction = 0f;

                var whirlwind = Instance?.GetLuckInfo(Player)?.GetStatLevelByName("Whirlwind") ?? 0; //Whirlwind ?? 0;
                if (whirlwind > 1) reduction = 3f * (whirlwind - 1);

                return 120 - reduction;
            }

            public override void Awake()
            {
                base.Awake();

                Interface.Oxide.LogWarning(nameof(WhirlwindCooldownHandler) + "." + nameof(Awake) + ", GetCooldownSeconds: " + GetCooldownSeconds());
                Interface.Oxide.LogWarning("Awake for ww handler (I hope the original awake is called too)!");

                MessageWhenCooldownStarts = "<color=#50CBDD>Whirlwind</color> <color=#D7B444>has ended</color>. <color=#D7B444>You may use it again in <color=#50CBDD>{cooldown}</color> seconds</color>.";
                MessageWhenUsable = "<color=#50CBDD>Whirlwind</color> <color=#D7B444>can be used again</color>!";

                CooldownStarted += WhirlwindCooldownHandler_CooldownStarted;
            }

            private void WhirlwindCooldownHandler_CooldownStarted()
            {
                Interface.Oxide.LogWarning(nameof(WhirlwindCooldownHandler_CooldownStarted));
            }
        }

        private class OverdriveCooldownHandler : PlayerCooldownHandler
        {

            public override float GetCooldownSeconds()
            {
                if (Player == null || !Player.IsConnected || Player.IsDead() || Player.IsSleeping()) return -1;

                var reduction = 0f;

                var overdrive = Instance?.GetLuckInfo(Player)?.GetStatLevelByName("Overdrive") ?? 0;
                if (overdrive > 1) reduction = 3f * (overdrive - 1);

                return 120 - reduction;
            }

            public override void Awake()
            {
                base.Awake();

                MessageWhenCooldownStarts = "<color=#222B30><color=#F5393A>Overdrive</color> <color=#89FE1A>has ended</color>. <color=#89FE1A>You may use it again in <color=#F5393A>{cooldown}</color> seconds</color>.</color>";
                MessageWhenUsable = "<color=#222B30><color=#F5393A>Overdrive</color> <color=#89FE1A>can be used again</color>!</color>";

                CooldownStarted += OverdriveCooldownHandler_CooldownStarted;
            }

            private void OverdriveCooldownHandler_CooldownStarted()
            {
                Interface.Oxide.LogWarning(nameof(OverdriveCooldownHandler_CooldownStarted));
            }
        }

        private class WeightOfTheWorldCooldownHandler : PlayerCooldownHandler
        {

            public override float GetCooldownSeconds()
            {
                if (Player == null || !Player.IsConnected || Player.IsDead() || Player.IsSleeping()) return -1;

                var reduction = 0f;

                var wotw = Instance?.GetLuckInfo(Player)?.GetStatLevelByName("WeightOfTheWorld") ?? 0;
                if (wotw > 1) reduction = 3f * (wotw - 1);


                return 120 - reduction;
            }

            public override void Awake()
            {
                base.Awake();

                Interface.Oxide.LogWarning(nameof(WeightOfTheWorldCooldownHandler) + "." + nameof(Awake) + ", GetCooldownSeconds: " + GetCooldownSeconds());
                Interface.Oxide.LogWarning("Awake for ww handler (I hope the original awake is called too)!");

                MessageWhenCooldownStarts = "<color=#C68826>Weight of the World</color> <color=#9E4122>has ended</color>. <color=#9E4122>You may use it again in <color=#C68826>{cooldown}</color> seconds</color><color=#C68826>.</color>";
                MessageWhenUsable = "<color=#C68826>Weight of the World</color> <color=#9E4122>can be used again</color><color=#C68826>!</color>";

                CooldownStarted += WeightOfTheWorldCooldownHandler_CooldownStarted;
            }

            private void WeightOfTheWorldCooldownHandler_CooldownStarted()
            {
                Interface.Oxide.LogWarning(nameof(WeightOfTheWorldCooldownHandler_CooldownStarted));
            }
        }

        //change proposal: playercooldownhandler also has a skill name or is assigned to a StatInfo (class), so we don't have to hardcode this text quite as much
        //this may also make these way easier in general and barely require these overrides
        private class SecondWindCooldownHandler : PlayerCooldownHandler
        {

            public override float GetCooldownSeconds()
            {
                return 1800;
            }

            public override void Awake()
            {
                base.Awake();

                MessageWhenCooldownStarts = "<color=#B7AF8B>Second Wind</color> <color=#FF884C>has ended</color>. <color=#FF884C>You may use it again in <color=#B7AF8B>{cooldown}</color> seconds</color><color=#B7AF8B>.</color>";
                MessageWhenUsable = "<color=#B7AF8B>Second Wind</color> <color=#FF884C>can be used again</color><color=#B7AF8B>!</color>";

            }
        }

        private class MagpieCooldownHandler : PlayerCooldownHandler
        {

            public override float GetCooldownSeconds()
            {
                if (Player == null || !Player.IsConnected || Player.IsDead() || Player.IsSleeping()) return -1;

                var reduction = 0f;

                var magpie = Instance?.GetLuckInfo(Player)?.GetStatLevelByName("Magpie") ?? 0;
                if (magpie > 1) reduction = 5f * (magpie - 1);


                return 120 - reduction;

                //may revisit later:

                //var reducedVal = 120 - reduction;

                //  var hookVal = Interface.Oxide.CallHook("OnGetCooldownSecondsMagpie", this, reducedVal);

                //return hookVal != null ? (float)hookVal : reducedVal;
            }

            public override void Awake()
            {
                base.Awake();

                Interface.Oxide.LogWarning(nameof(MagpieCooldownHandler) + "." + nameof(Awake) + ", GetCooldownSeconds: " + GetCooldownSeconds());
                Interface.Oxide.LogWarning("Awake for magpie handler (I hope the original awake is called too)!");

                MessageWhenCooldownStarts = "<color=#055E8E>Magpie</color> <color=#DAE5F8>has ended</color>. <color=#DAE5F8>You may use it again in <color=#055E8E>{cooldown}</color> seconds</color>.";
                MessageWhenUsable = "<color=#055E8E>Magpie</color> <color=#DAE5F8>can be used again</color>!";

                CooldownStarted += MagpieCooldownHandler_CooldownStarted;
            }

            private void MagpieCooldownHandler_CooldownStarted()
            {
                Interface.Oxide.LogWarning(nameof(MagpieCooldownHandler_CooldownStarted));
            }
        }

        private class BotanistCooldownHandler : PlayerCooldownHandler
        {

            public override float GetCooldownSeconds()
            {
                if (Player == null || !Player.IsConnected || Player.IsDead() || Player.IsSleeping()) return -1;

                var reduction = 0f;

                var botanist = Instance?.GetLuckInfo(Player)?.GetStatLevelByName("Botanist") ?? 0;
                if (botanist > 1) reduction = 3f * (botanist - 1);


                return 60 - reduction;
            }

            public override void Awake()
            {
                base.Awake();

                Interface.Oxide.LogWarning(nameof(BotanistCooldownHandler) + "." + nameof(Awake) + ", GetCooldownSeconds: " + GetCooldownSeconds());
                Interface.Oxide.LogWarning("Awake for botanist handler (I hope the original awake is called too)!");

                MessageWhenCooldownStarts = "<color=#63CE25>Botanist</color> <color=#C6B763>has ended</color>. <color=#C6B763>You may use it again in <color=#63CE25>{cooldown}</color> seconds</color><color=#63CE25>.</color>";
                MessageWhenUsable = "<color=#63CE25>Botanist</color> <color=#C6B763>can be used again</color><color=#63CE25>!</color>";

                CooldownStarted += BotanistCooldownHandler_CooldownStarted;
            }

            private void BotanistCooldownHandler_CooldownStarted()
            {
                Interface.Oxide.LogWarning(nameof(BotanistCooldownHandler_CooldownStarted));
            }
        }

        public class NPCHitTargetAlly : FacepunchBehaviour //applied to NPCs who are nearby the hit target
        {
            private NPCHitTarget _target;
            public NPCHitTarget Target
            {
                get { return _target; }
                set
                {
                    _target = value;

                }
            }

            public float TakenDamageScalar { get; set; } = 1.0f;
            public float DamageScalar { get; set; } = 1.0f;

            public void AdjustDamageScalars()
            {

                var tier = Target?.Tier ?? NPCHitTarget.Tiers.Low;

                var minTakenDamage = Mathf.Clamp(1f - ((int)tier * 0.12f), 0.05f, 1f); //THESE ARE NOT REVERSED BECAUSE MINUS ( - )
                var maxTakenDamage = Mathf.Clamp(1f - ((int)tier * 0.033f), 0.05f, 1f); //^^

                TakenDamageScalar = Random.Range(minTakenDamage, maxTakenDamage);

                var minDealtDamage = 1f + ((int)tier * 0.0125f); //these were reversed at one point, but no longer
                var maxDealtDamage = 1f + ((int)tier * 0.064f);

                DamageScalar = Random.Range(minDealtDamage, maxDealtDamage);

                Interface.Oxide.LogWarning("TakenDmgScalar: " + TakenDamageScalar + " for Tier: " + tier + ", DamageScalar: " + DamageScalar + " (ally)");

            }

            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(NPCHitTargetAlly) + " awaiting Target assignment");
            }
        }

        public class NPCHitTarget : FacepunchBehaviour //applied to NPCs who are targets of a hit
        {
            public enum Tiers { Undefined, Low, Medium, High, Elite, Legendary };

            private Tiers _tier = Tiers.Undefined;

            public Tiers Tier
            {
                get
                {
                    return _tier;
                }
                set
                {
                    _tier = value;
                    OnTierChanged();
                }
            }

            public float DamageScalar { get; set; } = 1f;
            public float TakenDamageScalar { get; set; } = 1f;

            public string FlavorRealName { get; set; } = string.Empty;
            public string FlavorNickName { get; set; } = string.Empty;

            public MercenaryHitHandler MercInfo { get; set; } = null;

            public HumanNPC NPC { get; set; } = null;

            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(NPCHitTarget) + ".Awake()");

                NPC = GetComponent<HumanNPC>();
                if (NPC == null || NPC.IsDestroyed || NPC.gameObject == null)
                {
                    Interface.Oxide.LogWarning(nameof(NPCHitTarget) + "." + nameof(Awake) + " called on null NPC!!!");
                    DoDestroy();
                    return;
                }

                FlavorRealName = GenerateFlavorRealName();
                FlavorNickName = GenerateFlavorNickName();

            }

            public void DoDestroy()
            {
                Destroy(this);
            }

            public void RandomlyAdjustDamageScalarsAccordingToTier() //needs a good formula lol
            {


                var minTakenDamage = Mathf.Clamp(1f - ((int)Tier * 0.12f), 0.05f, 1f); //THESE ARE NOT REVERSED BECAUSE MINUS ( - )
                var maxTakenDamage = Mathf.Clamp(1f - ((int)Tier * 0.033f), 0.05f, 1f); //^^

                TakenDamageScalar = Random.Range(minTakenDamage, maxTakenDamage);

                var minDealtDamage = 1f + ((int)Tier * 0.0125f); //these were reversed at one point, but no longer
                var maxDealtDamage = 1f + ((int)Tier * 0.064f);

                DamageScalar = Random.Range(minDealtDamage, maxDealtDamage);

                Interface.Oxide.LogWarning("TakenDmgScalar: " + TakenDamageScalar + " for Tier: " + Tier + ", DamageScalar: " + DamageScalar + " (actual HIT npc)");

                /*/
                var minTakenDamage = Mathf.Clamp(1f - ((int)Tier * 0.214f), 0.05f, 1f);
                var maxTakenDamage = Mathf.Clamp(1f - ((int)Tier * 0.128f), 0.05f, 1f);

                TakenDamageScalar = UnityEngine.Random.Range(minTakenDamage, maxTakenDamage);

                var minDealtDamage = 1f + ((int)Tier * 0.214f);
                var maxDealtDamage = 1f + ((int)Tier * 0.128f);

                DamageScalar = UnityEngine.Random.Range(minDealtDamage, maxDealtDamage);

                Interface.Oxide.LogWarning("TakenDmgScalar: " + TakenDamageScalar + " for Tier: " + Tier + ", DamageScalar: " + DamageScalar);/*/

            }

            public void AddAllNearbyAllies()
            {
                Interface.Oxide.LogWarning(nameof(AddAllNearbyAllies));

                if (NPC == null || NPC.gameObject == null || NPC.IsDestroyed || NPC.transform == null)
                {
                    Interface.Oxide.LogError(nameof(AddAllNearbyAllies) + " called with null NPC!");
                    return;
                }

                var nearScientists = Pool.GetList<HumanNPC>();

                try
                {
                    Vis.Entities(NPC.transform.position, 110f, nearScientists, PLAYER_LAYER_MASK);

                    //Interface.Oxide.LogWarning("nearby scientists: " + nearScientists.Count);

                    for (int i = 0; i < nearScientists.Count; i++)
                    {
                        var sci = nearScientists[i];
                        if (sci == null || sci.gameObject == null || sci.IsDestroyed || sci == NPC) continue;

                        var ally = sci?.GetComponent<NPCHitTargetAlly>() ?? sci?.gameObject?.AddComponent<NPCHitTargetAlly>();

                        ally.Target = this;
                        ally.AdjustDamageScalars();
                       // Interface.Oxide.LogWarning("set Ally target to this (NPCHitTarget)");

                    }

                  //  Interface.Oxide.LogWarning("added all " + nearScientists.Count + " allies");
                }
                finally { Pool.FreeList(ref nearScientists); }
            }

            public void EquipClothingGear(ref List<Item> items)
            {
                if (items == null) 
                    throw new ArgumentNullException(nameof(items));

                if (NPC == null || NPC.IsDestroyed || NPC.gameObject == null)
                {
                    Interface.Oxide.LogWarning("NPC is null/dead?!");
                    return;
                }

                var wear = NPC?.inventory?.containerWear;
                var wearItems = wear?.itemList;

                if (wearItems == null)
                {
                    Interface.Oxide.LogError(nameof(wearItems) + " is null?!?!");
                    return;
                }

              //  Interface.Oxide.LogWarning("looping through wearItems");

                for (int i = 0; i < wearItems.Count; i++)
                    Instance?.RemoveFromWorld(wearItems[i]);


              //  Interface.Oxide.LogWarning("looping through items");
                for(int i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    if (item == null)
                    {
                        Interface.Oxide.LogWarning("item null?!");
                        continue;
                    }

                   // var wearable = item?.info?.GetComponent<ItemModWearable>();
                  //perhaps do wearable/canexistwith check here later 

                    if (!item.MoveToContainer(wear))
                    {
                        Interface.Oxide.LogWarning("Failed to move: " + item);
                        Instance?.RemoveFromWorld(item);
                    }
                }
            }

            public void EquipGun(Item item)
            {
                var weaponSlot = NPC?.inventory?.containerBelt?.GetSlot(0);
                if (weaponSlot != null) Instance?.RemoveFromWorld(weaponSlot);

                if (!item.MoveToContainer(NPC.inventory.containerBelt, 0))
                    Instance?.RemoveFromWorld(item);

                var scientist = NPC as HumanNPC;
                scientist?.EquipWeapon();

            }

            public void EquipLegendaryPsycho()
            {
                var clothes = new string[] { "hat.gas.mask", "heavy.plate.pants", "roadsign.gloves", "boots" };

                var items = Pool.GetList<Item>();
                try 
                {
                    for (int i = 0; i < clothes.Length; i++)
                    {
                        items.Add(ItemManager.CreateByName(clothes[i]));

                    }

                    Interface.Oxide.LogWarning(nameof(EquipLegendaryPsycho) + " created items: " + clothes.Length + " (" + items.Count + ")");

                    EquipClothingGear(ref items);
                }
                finally { Pool.FreeList(ref items); }

                var weaponOptions = new string[] { "hmlmg", "rifle.ak", "rifle.semiauto" };

                var weaponItem = ItemManager.CreateByName(weaponOptions[Random.Range(0, weaponOptions.Length)]);
                EquipGun(weaponItem);


            }

            //NPC Idea: a crazy psycho type guy with a chest piece but no clothes underneath it (no hoodie, for example)
            //psycho in a gas mask with no shirt or chest piece?
            //implements skins - weary
            //we need to move this to a datafile that has a list of applicable options, i.e list of applicable head options for Elite tier
            public void EquipRandomGearAccordingToTier() //as this exists right now, this isn't going to work. this is awful. how do we make it good?
            {
                Interface.Oxide.LogWarning(nameof(EquipRandomGearAccordingToTier));

                if (!Instance._npcGearData.TierGears.TryGetValue(Tier, out NPCGearSet gearSet))
                {
                    Interface.Oxide.LogWarning("Tier has no gearset!!: " + Tier);
                    return;
                }

                if (gearSet.ClothingOptions.Count < 1)
                {
                    Interface.Oxide.LogWarning("Tier: " + Tier + " has no clothing options!!");
                }

                if (gearSet.WeaponOptions.Count < 1)
                {
                    Interface.Oxide.LogWarning("Tier: " + Tier + " has no weapon options!!");
                }

                var itemsToWear = Pool.GetList<Item>();
                try
                {
                    for (int i = 0; i < Enum.GetValues(typeof(NPCGearSet.WearType)).Length; i++)
                    {
                        var type = (NPCGearSet.WearType)i;

                        if (!gearSet.ClothingOptions.TryGetValue(type, out List<WeightedItemInfo> items) || gearSet.ClothingOptions.Count < 1) continue;

                        // Interface.Oxide.LogWarning("found weighted items: " + items.Count + " for type: " + type);

                        var listToShuffle = items.ToList();

                        var rngItem = GetRandomInfoFromList(ref listToShuffle, true);

                       // Interface.Oxide.LogWarning("returned random clothing item: " + rngItem.ItemID);

                        var skin = rngItem.SkinID == 0 ? (Instance?.GetRandomSkinID(rngItem.ItemID.ToString()) ?? 0) : rngItem.SkinID;

                        var itemToCreate = ItemManager.CreateByItemID(rngItem.ItemID, 1, skin);

                        if (itemToCreate == null) Interface.Oxide.LogError("itemToCreate null!!!: " + rngItem.ItemID);
                        else itemsToWear.Add(itemToCreate);

                    }

                    if (itemsToWear.Count > 0)
                        EquipClothingGear(ref itemsToWear);

                }
                finally { Pool.FreeList(ref itemsToWear); }
               

                if (gearSet.WeaponOptions.Count > 0)
                {
                    var weaponListToShuffle = gearSet.WeaponOptions.ToList();
                    var rngWeapon = GetRandomInfoFromList(ref weaponListToShuffle, true);

                   // Interface.Oxide.LogWarning("returned random weapon: " + rngWeapon.ItemID);

                    var weaponItem = ItemManager.CreateByItemID(rngWeapon.ItemID, 1, rngWeapon.SkinID);

                    if (weaponItem == null) Interface.Oxide.LogError("weaponItem null!!!!: " + rngWeapon.ItemID);
                    else EquipGun(weaponItem);

                }
               

            }

            private WeightedItemInfo GetRandomInfoFromList(ref List<WeightedItemInfo> items, bool shuffleList = false)
            {
                if (items == null) throw new ArgumentNullException(nameof(items));

                if (shuffleList) 
                    items.Shuffle((uint)Random.Range(0, uint.MaxValue));

                var minDiff = -1f;
                WeightedItemInfo item = null;

                for (int i = 0; i < items.Count; i++)
                {
                    var weightItem = items[i];

                    var rng = Random.Range(0.0f, 1.0f);
                    var itemProbability = weightItem.Weight;

                    var diff = rng >= itemProbability ? (rng - itemProbability) : (itemProbability - rng);

                    if (minDiff == -1f || diff < minDiff)
                    {
                     //   Interface.Oxide.LogWarning("minDiff is -1f or diff < minDiff, diff: " + diff + ", minnDiff: " + minDiff + ", rng: " + rng + ", leg prob: " + itemProbability);
                        minDiff = diff;
                        item = weightItem;
                    }

                }

                return item;
            }


            public void OnTierChanged()
            {
                EquipRandomGearAccordingToTier();
                RandomlyAdjustDamageScalarsAccordingToTier();
                AddAllNearbyAllies();
            }

            public void SpawnDogTagsIntoCorpse(NPCPlayerCorpse corpse)
            {
                if (corpse == null || corpse.IsDestroyed) return;

                var nickName = FlavorNickName;

                corpse.playerName = FlavorRealName;

                var merc = MercInfo;

                var tier = Tier;

                var dogTagShortName = tier >= Tiers.Elite ? "reddogtags" : tier == Tiers.High ? "bluedogtags" : "dogtagneutral";

                Interface.Oxide.LogWarning("about to do corpse invoke");

                corpse.Invoke(() =>
                {
                    Interface.Oxide.LogWarning("corpse ivnoke");
                    if (corpse == null || corpse?.containers == null)
                    {
                        Interface.Oxide.LogWarning("corpse containers null on 0.25f invoke!!");
                        return;
                    }

                    var dogTag = ItemManager.CreateByName(dogTagShortName, 1); //maybe use skins in the future? :)

                    if (dogTag == null)
                    {
                        Interface.Oxide.LogError("dogTag null!!!");
                        return;
                    }

                    if (!dogTag.MoveToContainer(corpse.containers[0]) && !dogTag.Drop(corpse.GetDropPosition(), corpse.GetDropVelocity()))
                    {
                        Interface.Oxide.LogError("failed to move dog tag and failed to drop!!!");
                        Instance?.RemoveFromWorld(dogTag);
                    }
                    else
                    {
                        Interface.Oxide.LogWarning("set _tagToTier[" + dogTag.uid + "] = " + tier);

                        if (Instance != null) Instance._tagToTier[dogTag.uid] = tier;

                        dogTag.name = corpse.playerName + " (\"" + nickName + "'') - dog tags";
                    }
                }, 0.25f);
            }

            public void SpawnTieredLootIntoCorpse(NPCPlayerCorpse corpse)
            {
                if (corpse == null || corpse.IsDestroyed) return;

                //consider spinforitems style of loot spawning
                /*/
                var maxItems = 2 * (int)Tier;
                var minItems = 1 * (int)Tier;

                var numItemsToSpawn = UnityEngine.Random.Range(minItems, maxItems);

                for (int i = 0; i < numItemsToSpawn; i++)
                {
                   
                }/*/


            }

            public void PopulateCorpse(NPCPlayerCorpse corpse)
            {
                if (corpse == null || corpse.IsDestroyed) return;

                SpawnDogTagsIntoCorpse(corpse);
                SpawnTieredLootIntoCorpse(corpse);
            }

            //very temp code - we'll use our own name list at some point probably
            public string GenerateFlavorRealName()
            {
                var array = RandomUsernames.Get(Random.Range(0, 10000)).ToCharArray();
                array[0] = char.ToUpper(array[0], CultureInfo.CurrentCulture);

                var firstName = new string(array);

                array = RandomUsernames.Get(Random.Range(0, 100000)).ToCharArray();
                array[0] = char.ToUpper(array[0], CultureInfo.CurrentCulture);

                return firstName + " " + new string(array);
            }

            public string GenerateFlavorNickName()
            {
                var array = RandomUsernames.Get(Random.Range(0, 1000000)).ToCharArray();
                array[0] = char.ToUpper(array[0], CultureInfo.CurrentCulture);

                return new string(array);
            }

        }

        public class MercenaryHitHandler : FacepunchBehaviour //needs to destroy itself when player disconnected
        {
            public BasePlayer Player { get; private set; }

            private NPCPlayer _npcTarget = null;

            public NPCPlayer NPCTarget
            {
                get
                {
                    return _npcTarget;
                }
                private set
                {
                    try
                    {
                        if (_targetInfo != null)
                        {
                            _targetInfo.DoDestroy();
                            _targetInfo = null;
                        }
                    }
                    finally
                    {
                        _npcTarget = value;
                        if (ShowTargetOnMap) StartMarkerUpdate();
                    }
                }
            }


            private NPCHitTarget _targetInfo = null;
            public NPCHitTarget TargetInfo
            {
                get
                {
                    if (_targetInfo == null && NPCTarget != null && !NPCTarget.IsDestroyed)
                    {
                        _targetInfo = NPCTarget?.GetComponent<NPCHitTarget>() ?? NPCTarget?.gameObject?.AddComponent<NPCHitTarget>();

                        _targetInfo.MercInfo = this; //this really is a mess but i guess it works (for now?) lol
                    }

                    return _targetInfo;
                }
            }

            private bool _showTargetOnMap = true;

            public bool ShowTargetOnMap
            {
                get
                {
                    return _showTargetOnMap;
                }
                set
                {
                    _showTargetOnMap = value;

                    if (!_showTargetOnMap)
                    {
                        InvokeHandler.CancelInvoke(this, UpdateTargetPositionOnMap);

                        if (MapMarker != null && !MapMarker.IsDestroyed) 
                            MapMarker.Kill();
                    }
                }
            }

            private float _mapInterval = 30f;

            public float TargetMapUpdateInterval
            {
                get
                {
                    return _mapInterval;
                }
                set
                {
                    if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));


                    InvokeHandler.CancelInvoke(this, UpdateTargetPositionOnMap);
                    _mapInterval = value;

                    InvokeHandler.InvokeRepeating(this, UpdateTargetPositionOnMap, 1f, _mapInterval);
                }
            }

            public MapMarkerGenericRadius MapMarker { get; private set; } = null;

            public void SetNPCTarget(NPCPlayer target)
            {
                ClearTarget();

                NPCTarget = target ?? throw new ArgumentException(nameof(target));
            }

            public void ClearTarget()
            {
                try { NPCTarget = null; }
                finally
                {
                    KillMapMarker(true);
                }
            }

            public NPCPlayer FindNewTarget()
            {
                var npcs = Pool.GetList<NPCPlayer>();

                try 
                {
                    foreach (var entity in BaseNetworkable.serverEntities)
                    {

                        var scientist = entity as NPCPlayer;
                        if (scientist != null && (entity is HumanNPC) && !scientist.ShortPrefabName.Contains("peacekeeper")) //outpost
                            npcs.Add(scientist);
                    }

                    if (npcs.Count < 1)
                    {
                        Interface.Oxide.LogWarning("no applicable npcs found!");
                        return null;
                    }

                    return npcs[Random.Range(0, npcs.Count)];
                }
                finally { Pool.FreeList(ref npcs); }
            }

            public NPCPlayer FindAndSetNewTarget()
            {
                var target = FindNewTarget();

                if (target == null)
                {
                    Interface.Oxide.LogWarning("Failed to find new target on " + nameof(FindAndSetNewTarget));
                    return null;
                }

                SetNPCTarget(target);

                return target;
            }

            private void Awake()
            {
                Player = GetComponent<BasePlayer>();

                if (Player == null || !Player.IsConnected)
                {
                    Interface.Oxide.LogError("BasePlayer is null or disconnected on Awake()!!!!");
                    DoDestroy();
                    return;
                }

                if (ShowTargetOnMap && NPCTarget != null && NPCTarget.IsAlive())
                    InvokeHandler.InvokeRepeating(this, UpdateTargetPositionOnMap, 1f, TargetMapUpdateInterval);


            }

            private bool _wasNpcTargetDead = true;

            private void FixedUpdate()
            {
                if (Player == null || Player.gameObject == null || !Player.IsConnected)
                {
                    Interface.Oxide.LogWarning(nameof(FixedUpdate) + " found null/disconnected player - destroying " + nameof(MercenaryHitHandler));
                    DoDestroy();
                    return;
                }

                if (!_wasNpcTargetDead && (NPCTarget == null || NPCTarget.IsDestroyed || NPCTarget.IsDead()))
                {
                    _wasNpcTargetDead = true;
                    OnNPCTargetDeath();
                }
                else if (_wasNpcTargetDead && NPCTarget != null && !NPCTarget.IsDestroyed && NPCTarget.IsAlive())
                {
                    _wasNpcTargetDead = false;
                }

            }

            private void OnNPCTargetDeath()
            {
                try { Interface.Oxide.CallHook("OnHitNPCDeath", this); }
                finally { ClearTarget(); }
            }

            public void KillMapMarker(bool stopUpdate = false)
            {
                if (stopUpdate) 
                    StopMarkerUpdate();

                if (MapMarker != null && !MapMarker.IsDestroyed)
                    MapMarker.Kill();
            }

            public void StopMarkerUpdate()
            {
                InvokeHandler.CancelInvoke(this, UpdateTargetPositionOnMap);
            }

            public void StartMarkerUpdate()
            {
                StopMarkerUpdate();

                if (TargetMapUpdateInterval > 0) InvokeHandler.InvokeRepeating(this, UpdateTargetPositionOnMap, 1, TargetMapUpdateInterval);
            }

            public void UpdateTargetPositionOnMap()
            {
                if (Player == null || Player?.gameObject == null || Player.IsDestroyed || Player.IsDead())
                {
                    Interface.Oxide.LogWarning(nameof(UpdateTargetPositionOnMap) + " but player is null/dead!");
                    return;
                }

                if (!ShowTargetOnMap)
                {
                    Interface.Oxide.LogWarning(nameof(UpdateTargetPositionOnMap) + " but !showtargetonmap, so cancel invoke in case!");
                    InvokeHandler.CancelInvoke(this, UpdateTargetPositionOnMap);
                }

                if (NPCTarget == null || NPCTarget.IsDestroyed || NPCTarget.IsDead())
                {
                    Interface.Oxide.LogWarning(nameof(UpdateTargetPositionOnMap) + " but NPCTarget is null/dead");
                    return;
                }

                var targetPos = NPCTarget?.transform?.position ?? Vector3.zero;

                var hadToCreate = false;

                if (MapMarker == null || MapMarker.IsDestroyed)
                {
                    hadToCreate = true;
                    Interface.Oxide.LogWarning("Did not find existing, usable MapMarker. creating.");

                    MapMarker = (MapMarkerGenericRadius)GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", targetPos);
                }

                var plyPos = Player?.transform?.position ?? Vector3.zero;

                var dist = Vector3.Distance(plyPos, targetPos);

                MapMarker.radius = Mathf.Clamp(0.05f * (dist * 0.033f), 0.175f, 0.667f); //0.375f;
                Interface.Oxide.LogWarning("marker radius: " + MapMarker.radius + ", dist: " + dist + " (reminder: radius is clamped if necessary)");


                MapMarker.OwnerID = 1996200121;

                var mainColor = targetPos.y < 3 ? new Color(1, 0.25f, 0.25f) : new Color(0.33f, 1, 1); //temp
                //red mainColor for underground for now

                MapMarker.color1 = mainColor;
                MapMarker.color2 = new Color(0f, 0f, 0f, 0f); //outline color
                MapMarker.alpha = 1f;

                if (hadToCreate)
                    MapMarker.Spawn();
                else MapMarker.transform.position = targetPos;

                MapMarker.SendUpdate();
                MapMarker.SendNetworkUpdateImmediate(true);

            }

            public void DoDestroy()
            {
                try
                {
                    try { TargetInfo?.DoDestroy(); }
                    finally { KillMapMarker(true); }
                }
                finally { Destroy(this); }
            }

        }

        public void GenerateRewardsBasedOnTierNoAlloc(NPCHitTarget.Tiers tier, ref List<Item> items) //maybe this needs moved
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            // items.Clear();

            var tierInt = (int)tier;
            
            var minItemsToSpawn = tier == NPCHitTarget.Tiers.Legendary ? 15 : tier == NPCHitTarget.Tiers.Elite ? 11 : tier == NPCHitTarget.Tiers.High ? 7 : tier == NPCHitTarget.Tiers.Medium ? 4 : 3;
            var maxItemsToSpawn = tier == NPCHitTarget.Tiers.Legendary ? 23 : tier == NPCHitTarget.Tiers.Elite ? 19 : tier == NPCHitTarget.Tiers.High ? 11 : tier == NPCHitTarget.Tiers.Medium ? 9 : 7;

            var itemsToSpawn = Random.Range(minItemsToSpawn, maxItemsToSpawn);

            for (int i = 0; i < itemsToSpawn; i++)
            {
                var itemCategory = GetRandomCategory(ItemCategory.All, ItemCategory.Favourite, ItemCategory.Common, ItemCategory.Search);

                Rust.Rarity itemRarity;
                itemRarity = Random.Range(0, 101) <= 50 ? GetRandomRarityOrDefault(itemCategory, false) : GetRandomRarityOrDefault(itemCategory, false, Random.Range(0, 101) <= 66 ? Rarity.Common : Rarity.Uncommon);

                var rngItem = GetRandomItem(itemRarity, itemCategory, false, false);

                if (rngItem == null)
                {
                    PrintWarning("rngItem null?!: " + itemCategory + ", " + itemRarity);
                    continue;
                }

                rngItem.amount = Mathf.Clamp((int)(rngItem.MaxStackable() * Random.Range(0.085f, 0.375f)), 1, 3000);

                items.Add(rngItem);


            }

            var compsToSpawn = Random.Range(1 * tierInt, 3 * tierInt);

            for (int i = 0; i < compsToSpawn; i++)
            {
                var rngComp = GetRandomComponent();

                if (rngComp == null)
                {
                    PrintWarning("rng comp null!?!?!?");
                    continue;
                }

                rngComp.amount = Mathf.Clamp((int)(rngComp.MaxStackable() * Random.Range(0.1f, 0.33f)), 1, 75);

                items.Add(rngComp);

            }

            if (Random.Range(0, 101) <= 50)
            {
                var batteryAmt = Random.Range(10 * tierInt , 50 * tierInt);

                var batteryItem = ItemManager.CreateByName("battery.small", batteryAmt);

                items.Add(batteryItem);
            }


            var legsToSpawnMin = tier == NPCHitTarget.Tiers.Legendary ? 2 : tier == NPCHitTarget.Tiers.Elite ? 1 : 0;
            var legsToSpawnMax = tier == NPCHitTarget.Tiers.Legendary ? 5 : tier == NPCHitTarget.Tiers.Elite ? 4 : 0;

            if (legsToSpawnMin < 1 || legsToSpawnMax < 1)
                return;

            var legsToSpawn = Random.Range(legsToSpawnMin, legsToSpawnMax); //max is inclusive!!!

            for (int i = 0; i < legsToSpawn; i++)
                items.Add(CreateRandomLegendaryItem());
            

        }

        private class PlayerCooldownHandler : FacepunchBehaviour
        {

            private bool? _lastCooldownState = null;

            private bool _hasCooldownStarted = false;

            public float CooldownStartTime { get; private set; }

            public BasePlayer Player { get; private set; }

            public float CooldownSeconds { get; set; }

            public virtual float GetCooldownSeconds() { return CooldownSeconds; }

            public float CooldownRemaining
            {
                get
                {
                    return GetCooldownSeconds() - (Time.realtimeSinceStartup - CooldownStartTime);
                }
            }

            public bool IsOnCooldown
            {
                get
                {
                    var secs = GetCooldownSeconds();
                    return secs <= 0 || (Time.realtimeSinceStartup - CooldownStartTime) <= secs;
                }
            }

            private string _messageWhenCooldownStarts = string.Empty;

            public bool ShowMessageWhenUsable { get; set; } = true;
            public bool ShowMessageWhenCooldownStarts { get; set; } = true;
            public string MessageWhenUsable { get; set; } = string.Empty;
            /// <summary>
            /// {cooldown} in this string is replaced with current cooldown seconds
            /// </summary>
            public string MessageWhenCooldownStarts
            {
                get
                {
                    var sb = Pool.Get<StringBuilder>();

                    try { return _messageWhenCooldownStarts.Replace("{cooldown}", GetCooldownSeconds().ToString("N0")).ToString(); }
                    finally { Pool.Free(ref sb); }

                }
                set
                {
                    _messageWhenCooldownStarts = value;
                }
            }

            public virtual void Awake()
            {
                Interface.Oxide.LogWarning(nameof(PlayerCooldownHandler) + "." + nameof(Awake));

                if (Player != null)
                {
                    Interface.Oxide.LogError("Awake called twice?!?!? player was NOT null on Awake()");
                    return;
                }

                Player = GetComponent<BasePlayer>();
                if (Player == null || Player.IsDestroyed || !Player.IsConnected)
                {
                    Interface.Oxide.LogWarning(nameof(Awake) + " called on an invalid player (null/destroyed/disconnected!");
                    return;
                }

                CooldownEnded += OnCooldownEnded;
                CooldownStarted += OnCooldownStarted;

                InvokeHandler.InvokeRepeating(this, CheckCooldown, 0.9f, 0.9f);
            }

            public delegate void CooldownEndCallback();
            public delegate void CooldownStartCallback();

            private CooldownEndCallback _endCallbackHandler;
            private CooldownStartCallback _startCallbackHandler;

            public event CooldownEndCallback CooldownEnded
            {
                add { _endCallbackHandler = (CooldownEndCallback)Delegate.Combine(_endCallbackHandler, value); }
                remove { _endCallbackHandler = (CooldownEndCallback)Delegate.Remove(_endCallbackHandler, value); }
            }

            public event CooldownStartCallback CooldownStarted
            {
                add { _startCallbackHandler = (CooldownStartCallback)Delegate.Combine(_startCallbackHandler, value); }
                remove { _startCallbackHandler = (CooldownStartCallback)Delegate.Remove(_startCallbackHandler, value); }
            }


            private void OnCooldownEnded()
            {
                Interface.Oxide.LogWarning(nameof(OnCooldownEnded) + " " + Player + " IsOnCooldown:" + IsOnCooldown);

                _hasCooldownStarted = false;
                if (ShowMessageWhenUsable && !string.IsNullOrEmpty(MessageWhenUsable))
                {
                    var hookVal = Interface.Oxide.CallHook("CanSendCooldownEndedMessage", this, MessageWhenUsable);

                    if ((hookVal == null || (bool)hookVal) && (Player?.IsConnected ?? false))
                        Player.ChatMessage(MessageWhenUsable);

                }
            }

            private void OnCooldownStarted()
            {
                Interface.Oxide.LogWarning(nameof(OnCooldownStarted) + " " + Player + " IsOnCooldown:" + IsOnCooldown);

                _hasCooldownStarted = true;
                if (ShowMessageWhenCooldownStarts && !string.IsNullOrEmpty(MessageWhenCooldownStarts))
                {
                    var hookVal = Interface.Oxide.CallHook("CanSendCooldownStartedMessage", this, MessageWhenCooldownStarts);

                    if ((hookVal == null || (bool)hookVal) && (Player?.IsConnected ?? false))
                        Player.ChatMessage(MessageWhenCooldownStarts);
                }
            }

            private void CheckCooldown()
            {
                if (!_hasCooldownStarted) return; //has never started, no point in checking yet

                var isOn = IsOnCooldown;
                if (_lastCooldownState == null || _lastCooldownState != isOn)
                {
                    if (!isOn) _endCallbackHandler?.Invoke();
                    else _startCallbackHandler?.Invoke();
                }

                _lastCooldownState = isOn;
            }

            public void StartCooldown()
            {
                _hasCooldownStarted = true;

                CooldownStartTime = Time.realtimeSinceStartup;
            }

            public void EndCooldown()
            {
                CooldownStartTime = Time.realtimeSinceStartup - GetCooldownSeconds();
            }

            public void DoDestroy()
            {
                try { InvokeHandler.CancelInvoke(this, CheckCooldown); }
                finally { Destroy(this); }
            }

        }

        private class ARPGClass
        {
            public string DisplayName { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;
            public string ShortDescription { get; set; } = string.Empty;

            public string PrimaryColor { get; set; } = string.Empty;
            public string SecondaryColor { get; set; } = string.Empty;

            public bool Hide { get; set; } = false;

            public ClassType Type { get; set; }

            public static List<ARPGClass> Classes { get; private set; } = new List<ARPGClass>();
            public static Dictionary<ClassType, ARPGClass> TypeToClass { get; private set; } = new Dictionary<ClassType, ARPGClass>();

            public static ARPGClass FindByType(ClassType type)
            {

                if (!TypeToClass.TryGetValue(type, out ARPGClass _class))
                {
                    if (type != ClassType.Undefined) Interface.Oxide.LogError("typetoclass does not have this name!: " + type);
                    return null;
                }
                else return _class;
            }

            public static ARPGClass FindByDisplayName(string displayName)
            {
                if (string.IsNullOrWhiteSpace(displayName))
                    throw new ArgumentNullException(nameof(displayName));

                for (int i = 0; i < Classes.Count; i++)
                {
                    var rpgClass = Classes[i];

                    if (rpgClass.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        return rpgClass;

                }

                return null;
            }

            public static ARPGClass FindByTypeName(string typeName)
            {
                if (string.IsNullOrWhiteSpace(typeName))
                    throw new ArgumentNullException(nameof(typeName));

                for (int i = 0; i < Classes.Count; i++)
                {
                    var rpgClass = Classes[i];

                    if (rpgClass.Type.ToString().Equals(typeName, StringComparison.OrdinalIgnoreCase))
                        return rpgClass;

                }

                return null;
            }

            public ARPGClass(ClassType type, string displayName = "", string shortDescription = "", string description = "", string color1 = "", string color2 = "", bool hide = false)
            {

                DisplayName = displayName;
                Description = description;
                ShortDescription = shortDescription;
                Type = type;

                PrimaryColor = color1;
                SecondaryColor = color2;

                Hide = hide;

                Classes.Add(this);
                TypeToClass[Type] = this;
            }

            public ARPGClass()
            {
                Interface.Oxide.LogWarning("arpgclass()");


                Classes.Add(this);
                TypeToClass[Type] = this;
            }

            public override string ToString()
            {
                return Type + "_" + DisplayName + "_" + Description;
            }

        }


        private ARPGClass GetUserClass(string userId)
        {
            if (!_userClass.TryGetValue(userId, out ClassType val))
                return null;


            return ARPGClass.FindByType(val);
        }

        private string GetUserClassName(string userId)
        {
            return GetUserClass(userId)?.DisplayName ?? string.Empty;
        }

        private string GetUserClassPrimaryColor(string userId)
        {
            return GetUserClass(userId)?.PrimaryColor ?? string.Empty;
        }

        private string GetUserClassSecondaryColor(string userId)
        {
            return GetUserClass(userId)?.SecondaryColor ?? string.Empty;
        }

        private ClassType GetUserClassType(string userId)
        {
            if (!_userClass.TryGetValue(userId, out ClassType val))
                return ClassType.Undefined;


            return val;
        }

        private void SetUserClass(string userId, ClassType type)
        {
            _userClass[userId] = type;
        }

        private void ResetUserClass(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (!_userClass.TryGetValue(userId, out ClassType userClass)) userClass = ClassType.Undefined;

            if (userClass == ClassType.Undefined)
                PrintWarning(nameof(ResetUserClass) + " but: " + userId + " has class is undefined");

            _userClass.Remove(userId);

            try
            {
                var luckInfo = GetLuckInfo(userId);
                if (luckInfo != null)
                {
                    var spent = luckInfo.GetPointsSpentOnClassStats();

                    luckInfo.ResetAllClassStats();
                    PrintWarning("called luckInfo.ResetAllClassStats() for: " + userId + ", points spent was: " + spent);

                    luckInfo.SkillPoints += spent;

                    PrintWarning("they've been refunded " + spent + " skpoints");

                }
                else PrintWarning("null luckInfo on resetuserclass?! for: " + userId);
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try
            {
                if (userClass != ClassType.Undefined)
                {
                    var mainSkillToReset = userClass == ClassType.Lumberjack ? "WC" : userClass == ClassType.Miner ? "M" : "S"; //packrats, nomads, loners, mercs and farmers all get survival xp

                    ZLevelsRemastered?.Call("ResetLvl", userId, mainSkillToReset);

                    PrintWarning("called resetlvl for: " + userId + " (" + mainSkillToReset + ")");

                }
                else PrintWarning("no zlvl call because class is undefined");
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try
            {
                if (userClass != ClassType.Undefined && (DiscordAPI2?.IsLoaded ?? false))
                {
                    PrintWarning("Removing role via DiscordAPI2 call for reset user class");
                    DiscordAPI2?.Call("RemoveRoleWithColor", userId, userClass.ToString(), ARPGClass.FindByType(userClass).PrimaryColor);
                }
            }
            catch (Exception ex) { PrintError(ex.ToString()); }


            PrintWarning("reset user class for: " + userId);

        }

        private float DistanceFromOilRig(Vector3 pos, bool largeOil = false, bool normalizeY = true)
        {
            var desiredRig = largeOil ? LargeOilRig : SmallOilRig;

            if (normalizeY)
                pos = new Vector3(pos.x, desiredRig.transform.position.y, pos.z);
            

            return Vector3.Distance(desiredRig.transform.position, pos);
        }

        private bool HasWOTWActive(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            return ActiveWeightOfTheWorldCoroutine.TryGetValue(player, out Coroutine routine) && routine != null;
        }
        
        private bool IsBackpackSkin(ulong skin)
        {
            return skin switch
            {
                LONERS_BACKPACK_SKIN_ID or LARGE_BACKPACK_SKIN_ID or MEDIUM_BACKPACK_SKIN_ID or SMALL_BACKPACK_SKIN_ID => true,
                _ => false,
            };
        }

        private bool IsBackpackItem(Item item)
        {
            return IsBackpackSkin(item?.skin ?? 0);
        }
        /// <summary>
        /// Iterates through containerWear to find any item that has a backpack skin
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private Item GetBackpackItem(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.IsDestroyed || player.IsDead()) 
                return null;

            var wornItems = player?.inventory?.containerWear?.itemList;

            if (wornItems == null)
                return null;

            for (int i = 0; i < wornItems.Count; i++)
            {
                var wearable = wornItems[i];

                if (IsBackpackItem(wearable))
                    return wearable;
            }

            return null;
        }

        private void UnequipItemsMissingRequirements(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            //optimize this stuff later. save drop pos, velocity elsewhere; rot too? idk if this matters when it comes to compiler. really need an answer lol


            var beltItems = player?.inventory?.containerBelt?.itemList;

            if (beltItems != null)
            {

                for (int i = 0; i < beltItems.Count; i++)
                {
                    var item = beltItems[i];
                    if (!IsLegendaryItem(item))
                        continue;

                    var canWearObj = CanWearItem(player.inventory, item, -1, true);
                    if (canWearObj != null && !(bool)canWearObj)
                    {
                        if (!item.MoveToContainer(player.inventory.containerMain))
                            item.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation);
                    }

                }

             
            }

            var wearItems = player?.inventory?.containerWear?.itemList;

            if (wearItems == null) return;


            for (int i = 0; i < wearItems.Count; i++)
            {
                var item = wearItems[i];

                if (!IsLegendaryItem(item))
                    continue;

                var canWearObj = CanWearItem(player.inventory, item, -1, true);
                if (canWearObj != null && !(bool)canWearObj)
                {
                    if (!item.MoveToContainer(player.inventory.containerMain))
                        item.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation);
                }

            }

        }

        /// <summary>
        /// Iterates through the container's itemList to find any item definition matching itemId
        /// </summary>
        /// <param name="container"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private Item FindItemInContainer(ItemContainer container, int itemId)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (container?.itemList == null)
                throw new ArgumentNullException(nameof(container.itemList));


            for (int i = 0; i < container.itemList.Count; i++)
            {
                var item = container.itemList[i];

                if (item?.info?.itemid == itemId)
                    return item;

            }

            return null;
        }

        private bool HasItemInContainer(ItemContainer container, int itemId)
        { 
            return FindItemInContainer(container, itemId) != null; 
        }

        private Item FindItemByIDU(ulong uID)
        {
            if (uID == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(uID));
            }

            var itemId = new ItemId(uID);

            var allItems = Pool.GetList<Item>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead() || !player.IsConnected) continue;

                    player.inventory.AllItemsNoAlloc(ref allItems);

                    for (int j = 0; j < allItems.Count; j++)
                    {
                        if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;

                        var item = allItems?.Count >= j ? allItems[j] : null;
                        if (item?.uid == itemId) 
                            return item; //else: find backpack items or other contents item (an item WITHIN an item's inventory)
                        else if (item?.contents?.itemList is not null)
                        {

                            var itemContents = item?.contents;

                            for (int k = 0; k < itemContents.itemList.Count; k++)
                            {
                                var bpItem = itemContents.itemList[k];

                                if (bpItem?.uid == itemId) //found our target item within another item!
                                    return item;

                            }

                        }
                    }

                    try
                    {
                        var lootContainers = player?.inventory?.loot?.containers ?? null;
                        if (lootContainers == null || lootContainers.Count < 1) continue;

                        var containerList = new List<ItemContainer>(lootContainers);

                        for (int j = 0; j < containerList.Count; j++)
                        {
                            if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;
                            var itemList = containerList?.Count >= j ? containerList[j]?.itemList ?? null : null;
                            if (itemList == null || itemList.Count < 1) continue;
                            var loopList = new List<Item>(itemList);
                            for (int k = 0; k < loopList.Count; k++)
                            {
                                if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;
                                var item = loopList?.Count >= k ? loopList[k] : null;
                                if (item?.uid == itemId) return item;
                                else if (item?.contents?.itemList is not null)
                                {

                                    var itemContents = item?.contents;

                                    for (int l = 0; l < itemContents.itemList.Count; l++)
                                    {
                                        var bpItem = itemContents.itemList[l];

                                        if (bpItem?.uid == itemId) //found our target item within another item!
                                            return item;

                                    }

                                }
                            }
                        }

                    }
                    catch (Exception ex) { PrintError("Failed on containerList loops (activePlayerList): " + ex.ToString()); }

                }
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var player = BasePlayer.sleepingPlayerList[i];
                    if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) continue;

                    player.inventory.AllItemsNoAlloc(ref allItems);

                    for (int j = 0; j < allItems.Count; j++)
                    {
                        if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;
                        var item = allItems?.Count >= j ? allItems[j] : null;
                        if (item?.uid == itemId)
                            return item; //else: find backpack items or other contents item (an item WITHIN an item's inventory)
                        else if (item?.contents?.itemList is not null)
                        {

                            var itemContents = item?.contents;

                            for (int k = 0; k < itemContents.itemList.Count; k++)
                            {
                                var bpItem = itemContents.itemList[k];

                                if (bpItem?.uid == itemId) //found our target item within another item!
                                    return item;

                            }

                        }
                    }
                }
            }
            finally { Pool.FreeList(ref allItems); }

            foreach (var drop in allDropped) { if (drop?.item?.uid == itemId) return drop.item; }

            foreach (var lootCont in allContainers)
            {
                if (lootCont == null || lootCont?.inventory?.itemList == null || lootCont.inventory.itemList.Count < 1) continue;

                for (int i = 0; i < lootCont.inventory.itemList.Count; i++)
                {
                    if (lootCont == null || lootCont?.inventory?.itemList == null || lootCont.inventory.itemList.Count < 1) break;
                    var item = lootCont.inventory.itemList[i];
                    if (item?.uid == itemId) return item;
                }
            }

            foreach (var entry in BaseNetworkable.serverEntities)
            {
                if (entry == null) continue;
                var storage = entry as StorageContainer;

                Item item;
                if ((storage?.inventory?.itemList?.Count ?? 0) > 0)
                {
                    for (int i = 0; i < storage.inventory.itemList.Count; i++)
                    {
                        item = storage.inventory.itemList[i];
                        if (item?.uid == itemId)
                            return item;
                        else if (item?.contents?.itemList != null)
                        {
                            var itemContents = item?.contents;

                            for (int k = 0; k < itemContents.itemList.Count; k++)
                            {
                                var bpItem = itemContents.itemList[k];

                                if (bpItem?.uid == itemId) //found our target item within another item!
                                    return item;

                            }
                        }
                    }
                }

                var corpse = entry as LootableCorpse;
                if ((corpse?.containers?.Length ?? 0) > 0)
                {
                    for (int i = 0; i < corpse.containers.Length; i++)
                    {
                        var container = corpse.containers[i];
                        if (container?.itemList == null || container.itemList.Count < 1) continue;

                        for (int j = 0; j < container.itemList.Count; j++)
                        {
                            if (container?.itemList == null || container.itemList.Count < 1) break;
                            item = container.itemList[j];
                            if (item?.uid == itemId) return item;
                        }
                    }
                }


                item = (entry as DroppedItemContainer)?.inventory?.FindItemByUID(itemId);

                if (item != null)
                    return item;
            }

            foreach (var bp in _userBackpack)
            {
                var backPackEntity = bp.Value as DroppedItemContainer;

                if (backPackEntity?.inventory == null || backPackEntity.IsDestroyed) continue;

                var item = backPackEntity?.inventory?.FindItemByUID(itemId);
                if (item != null)
                    return item;
            }

            return null;
        }

        private Item FindItemByID(ItemId uID)
        {
            return FindItemByIDU(uID.Value);
        }

        private Item FindItemByID(string ID)
        {
            if (ulong.TryParse(ID, out ulong uID)) return FindItemByIDU(uID);
            else return null;
        }

        private readonly Dictionary<string, Dictionary<int, Dictionary<ulong, TimeCachedValue<bool>>>> _isWearingItemCache = new();

        /// <summary>
        /// Iterates player's containerWear item list to find specified item. If skin is specified, item ID and skin must match to return true. If no skin is provided (0), only item ID must match.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemId"></param>
        /// <param name="skin"></param>
        /// <returns></returns>
        private bool IsWearingItem(BasePlayer player, int itemId, ulong skin = 0, bool skipCache = false) //if you NEEDED the item id and skin to be 0, this is a problem, I suppose.
        {
            try
            {
                if (!IsValidPlayer(player) || player?.inventory?.containerWear?.itemList == null || player.IsDead())
                    return false;

                if (string.IsNullOrWhiteSpace(player?.UserIDString))
                    PrintError(nameof(IsWearingItem) + " has player with userid null/empty?!");

                if (!_isWearingItemCache.TryGetValue(player.UserIDString, out _))
                    _isWearingItemCache[player.UserIDString] = new Dictionary<int, Dictionary<ulong, TimeCachedValue<bool>>>();

                if (!_isWearingItemCache[player.UserIDString].TryGetValue(itemId, out _))
                    _isWearingItemCache[player.UserIDString][itemId] = new Dictionary<ulong, TimeCachedValue<bool>>();

                if (!_isWearingItemCache[player.UserIDString][itemId].TryGetValue(skin, out _))
                    _isWearingItemCache[player.UserIDString][itemId][skin] = new TimeCachedValue<bool>()
                    {
                        refreshCooldown = 3f,
                        refreshRandomRange = 0.5f,
                        updateValue = new Func<bool>(() =>
                        {
                            try
                            {
                                if (player?.inventory?.containerWear?.itemList == null)
                                {
                                    PrintWarning("itemlist is null on updateValue!!!");
                                    return false;
                                }

                                if (player?.inventory?.containerWear?.itemList?.Count < 1)
                                    return false;

                                for (int i = 0; i < player.inventory.containerWear.itemList.Count; i++)
                                {
                                    var item = player.inventory.containerWear.itemList[i];

                                    if (item?.info == null)
                                        continue;

                                    if (item.info.itemid == itemId && (skin == 0 || item.skin == skin))
                                        return true;
                                }
                            }
                            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^^updateValue exception^^"); }


                            return false;
                        })
                    };

                return _isWearingItemCache[player.UserIDString][itemId][skin].Get(skipCache);
            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            return false;
        }

        /// <summary>
        /// Checks if a player's class matches the item type and should have a higher stack size.
        /// </summary>
        /// <param name="userClass"></param>
        /// <param name="item"></param>
        /// <returns><c>True</c> if <paramref name="userClass"/> should stack <paramref name="item"/> higher than other classes.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool ShouldApplyStackMultiplierForClass(ClassType userClass, Item item)
        {
            if (userClass < ClassType.Undefined)
                throw new ArgumentOutOfRangeException(nameof(userClass));

            if (userClass == ClassType.Undefined)
                return false;

            var isCookable = item.info.GetComponent<ItemModCookable>() != null;

            var category = item.info.category;

            var itemId = item.info.itemid;

            return userClass switch
            {
                ClassType.Mercenary => category == ItemCategory.Weapon || category == ItemCategory.Ammunition || category == ItemCategory.Medical,
                ClassType.Packrat => category == ItemCategory.Component || category == ItemCategory.Items || itemId == BATTERY_ID,
                ClassType.Farmer => category == ItemCategory.Food && !isCookable,
                ClassType.Nomad => true,
                ClassType.Miner => itemId == HQ_COOKED_ITEM_ID || itemId == METAL_COOKED_ITEM_ID || itemId == SULFUR_COOKED_ITEM_ID || itemId == STONES_ITEM_ID || itemId == METAL_ORE_ITEM_ID || itemId == SULFUR_ORE_ITEM_ID || itemId == HQ_ORE_ITEM_ID,
                ClassType.Lumberjack => itemId == WOOD_ITEM_ID || itemId == CHARCOAL_ITEM_ID,
                _ => false,
            };
        }

        /// <summary>
        /// Iterates player's containerWear item list to find ANY item matching specified skin. If skin is found on any item, returns true.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skin"></param>
        /// <returns></returns>
        private bool IsWearingSkin(BasePlayer player, ulong skin, bool skipCache = false)
        {
            return !player.IsDead() && ContainerHasSkin(player.inventory.containerWear, skin, skipCache);
        }


        private readonly Dictionary<BasePlayer, TimeCachedValue<Item>> _activeItemCache = new();
        private Item GetActiveItem(BasePlayer player, bool skipCache = false)
        {
            if (!_activeItemCache.TryGetValue(player, out _))
                _activeItemCache[player] = new TimeCachedValue<Item>()
                {
                    refreshCooldown = 1.33f,
                    refreshRandomRange = 0.16f,
                    updateValue = new Func<Item>(() => 
                    {
                        return player?.GetActiveItem();
                    })
                };

            return _activeItemCache[player].Get(skipCache);
        }

        private readonly Dictionary<ItemContainerId, Dictionary<ulong, TimeCachedValue<bool>>> _containerHasSkinTimeCache = new();


        /// <summary>
        /// Iterates through the specified ItemContainer to find ANY item matching specified skin. If skin is found on any item, returns true. Value is cached unless skip cache is true.
        /// </summary>
        /// <param name="inv"></param>
        /// <param name="skin"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private bool ContainerHasSkin(ItemContainer inv, ulong skin, bool skipCache = false)
        {
            if (!_containerHasSkinTimeCache.TryGetValue(inv.uid, out _))
                _containerHasSkinTimeCache[inv.uid] = new Dictionary<ulong, TimeCachedValue<bool>>();

            if (!_containerHasSkinTimeCache[inv.uid].TryGetValue(skin, out _))
            {
                _containerHasSkinTimeCache[inv.uid][skin] = new TimeCachedValue<bool>()
                {
                    refreshCooldown = 3f,
                    refreshRandomRange = 0.5f,
                    updateValue = new Func<bool>(() =>
                    {
                        for (int i = 0; i < inv.itemList.Count; i++)
                            if (inv.itemList[i].skin == skin)
                                return true;

                        return false;
                    })
                };
            }


            return _containerHasSkinTimeCache[inv.uid][skin].Get(skipCache);
        }

        private bool IsCharm(Item item)
        {
            if (item?.info != NoteDefinition) return false;
            return charmsData?.charms != null && charmsData.charms.TryGetValue(item?.uid.Value ?? 0, out _);
        }

        private void AdjustTickRate(WildlifeTrap trap, ulong userID)
        {
            if (trap == null || userID == 0) return;

            var trapExt = trap?.GetComponent<WildTrapExt>() ?? null;
            if (trapExt == null) return;

            var fishLvl = GetLuckInfo(userID)?.GetStatLevelByName("Fishing") ?? 0;

            if (fishLvl < 1)
                return;

            var tickRate = 60f;

            var percReduction = Mathf.Clamp(15 * fishLvl, 0, 95);
            tickRate -= tickRate * percReduction / 100;

            trapExt.TickRate = tickRate;
        }

        private readonly Dictionary<uint, Action> checkAction = new();
        private Coroutine CharmPurgeCoroutine = null;
        private bool _purgeOnInit = false;
        private void OnTerrainInitialized()
        {
            _purgeOnInit = true;

        }

        private void OnNewSave()
        {
            PrintWarning(nameof(OnNewSave) + " for " + nameof(Luck));
            PrintWarning("temp debug/not setting classes to reset on init");
            // _resetClassesOnInit = true;
        }

        private void OnServerInitialized()
        {
            try
            {
                Interface.Oxide.LogWarning(nameof(OnServerInitialized));
                Subscribe(nameof(CanWearItem));
                Subscribe(nameof(CanEquipItem));

                try
                {
                    if (_purgeOnInit)
                    {
                        PrintWarning(nameof(_purgeOnInit));
                        CharmPurgeCoroutine = StartCoroutine(CharmPurge());
                    }
                    
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try 
                {

                    _f1HelpTimer = timer.Every(300f, () =>
                    {
                        try
                        {
                            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                            {
                                var p = BasePlayer.activePlayerList[i];
                                if (p != null && p.IsConnected) SendF1HelpMessage(p);
                            }
                        }
                        catch(Exception ex) { PrintError(ex.ToString()); }
                    });
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try 
                {
                    for (int i = 0; i < ItemManager.itemList.Count; i++)
                    {
                        var itemDef = ItemManager.itemList[i];

                        if (itemDef?.GetComponent<ItemModFishable>() != null)
                            _fishItems.Add(itemDef);

                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try 
                {
                    foreach(var ent in BaseNetworkable.serverEntities)
                    {
                        if (ent.prefabID == 251735616)
                        {
                            _banditConversationalist = (VehicleVendor)ent;
                            break;
                        }
                    }

                    if (_banditConversationalist == null)
                    {
                        PrintError("bandit conversationalist does not exist or prefab ID changed!!!!");
                    }

                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try 
                {
                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var p = BasePlayer.activePlayerList[i];
                        loadUser(p);

                        if (ShouldHaveCharmComponent(p)) 
                            EnsureCharmComponent(p);

                        SendF1HelpMessage(p);

                        var backpackItem = GetBackpackItem(p);
                        if (backpackItem != null)
                            CreateBackpackEntityForPlayer(p, backpackItem);

                    }

                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];

                        var backpackItem = GetBackpackItem(p);
                        if (backpackItem != null)
                            CreateBackpackEntityForPlayer(p, backpackItem);

                    }

                }
                catch(Exception ex) { PrintError(ex.ToString()); }
                

                try
                {

                    new ARPGClass(ClassType.Nomad, "Nomad", "Take less & deal more dmg, fast bags, tiny bases, craft T2 always|--M/WC XP|++combat Luck chance|++Luck XP", "<color=#B7AF8B>Nomads</color> <color=#FF884C>are known for roaming around; not staying in one place for too long. They are able to <color=#B7AF8B>carry more items</color>, their fast-paced ability makes them able to <color=#B7AF8B>craft T2 items anywhere</color> & have <color=#B7AF8B>much faster sleeping bags</color>. They deal more damage & take less. They're limited to tiny bases, since they are always moving. They gain significantly less Mining and Woodcutting XP, but gain more Luck & normal Survival XP. They're more proficient in combat, giving them higher Luck chances for those abilities.\nThey gain 200% Luck XP.\nWearing the <color=#B7AF8B>Nomad suit</color> <i>grants access to craft <color=#B7AF8B>T3</color> items <color=#B7AF8B>anywhere</color></i> & can type <color=#B7AF8B>/rb</color> to open a repair bench <i>(lower quality)</i>. In the <color=#B7AF8B>desert</color>, they suffer a smaller penalty to their gather rate & XP.\nTheir cars take less damage.\nThey have access to a class specific Vending Machine found near Outpost.</color>", "#B7AF8B", "#FF884C");
                    new ARPGClass(ClassType.Lumberjack, "Lumberjack", "Luck XP when cutting, ++charcoal production|--M/S XP|++proc chance|++wood/charcoal carry", "<color=#F4D927>Lumberjacks</color> <color=#E3AB3C>are masters of the woods. Known for their unmatched cutting abilities, they not only gain Luck XP from cutting trees, but are also able to <color=#F4D927>carry more wood and charcoal</color>. They have a <color=#F4D927>higher chance to proc all resource abilities when chopping trees</color>. They also have an active ability called ''<color=#50CBDD>Whirlwind</color>'', which, when the <color=#F4D927>middle mouse is held down</color>, summons a storm and <color=#F4D927>attacks all nearby trees with dozens of hatchets at once for a few seconds</color>. This ability can be upgraded <color=#F4D927>10</color> times, each time decreasing the cooldown (by 3s), and increasing the radius & number of hatchets/hits\nWhen wearing the <color=#723134>Lumberjack Hoodie</color>, they gain +3 to <i><color=#723134>Two Fang</color></i> and +1 to <i><color=#F4D927>Felling</color></i> & <i><color=#50CBDD>Whirlwind</color></i>\nThey can hold more fuel in their chainsaws.\nThey have access to a class specific Vending Machine found at the Outpost.</color>", "#F4D927", "#E3AB3C");
                    new ARPGClass(ClassType.Miner, "Miner", "Luck XP when mining, faster smelting, --WC/S XP|++proc chance|++ore carry|ores marked on map @ lvl 25", "<color=#0288F6>Miners</color> <color=#DC3230>are renowned for their ability to dig up every inch of the earth, wasting nothing. They are able to <color=#0288F6>carry more ore</color>, and they have a <color=#0288F6>higher chance to proc all resource abilities when mining</color>. Being this proficient has also allowed them to <color=#0288F6>smelt ore more quickly</color>.\nThey have an upgradeable ability named ''<color=#0288F6>Weight of the World</color>'' which can be used by <color=#0288F6>striking the ground below them 3 times with </color>a <color=#0288F6>Pickaxe</color> or <color=#0288F6>Jackhammer</color>. Once triggered, this ability <color=#0288F6>drags all the ores in the nearby area to their position and crushes them in epic fashion</color>.\nUpon reaching Mining level <color=#0288F6>25</color>, miners will unlock a passive ability that <color=#0288F6>highlights nearby ores on their Map</color>. The radius increases each Mining level & increased by 1.5x when wearing a Miner's Hat\nThey have access to a class specific Vending Machine found at the Outpost.</color>", "#0288F6", "#DC3230");
                    new ARPGClass(ClassType.Loner, "Loner", "1.66x gather|+Luck chance, cheap research, steal from VMs, ++bandit prices|--TPRs|++Home TPs", "<color=#CBD01D>Loners</color> <color=#3999CE>are known for their desire to stick to themselves and their proficiency at doing just that. While they don't group up with other people, they've learned to become highly self-sufficient. They <color=#CBD01D>gather at an increased rate</color> (<color=#CBD01D>1</color>.<color=#CBD01D>66x</color>), they have a <color=#CBD01D>5%</color> higher chance of getting Lucky, <color=#CBD01D>25%</color> of all research costs are refunded, and they have an upgradable ability that <color=#CBD01D>allows them to occasionally steal items from Vending Machines when making a purchase</color>.\n<color=#CBD01D>Loners</color> have found somewhat of a friendship with the bandits, and they are refunded <color=#CBD01D>25%</color> when purchasing from the bandit camp</color>.", "#CBD01D", "#3999CE");
                    new ARPGClass(ClassType.Farmer, "Farmer", "Luck XP from plants, --WC/M XP|++proc chance|++food/seed carry|auto seeds|bone frags to mature", "<color=#C6B763><color=#63CE25>Farmers</color> are the caretakers of the <color=#965736>earth</color>. Not seeking to destroy it, they have learned to grow it and protect it from the elements. While earning Luck XP from all plant-related activities, they also have a <color=#63CE25>higher chance to proc <color=#965736>all</color> resource abilities when farming</color>, and they are able to <color=#63CE25>carry more food</color>. They are able to <color=#63CE25>automatically extract seeds from plants when harvesting</color>, and with <color=#63CE25>''Botanist''</color> they can <color=#63CE25>use bone fragments to mature plants instantly</color>.\nTheir active ability, <color=#63CE25>''Botanist''</color>, allows them to splash their plants with water from a <color=#965736>bucket</color>, <color=#965736>jug</color>, or <color=#965736>bota bag</color>, <color=#63CE25>resulting in all nearby Ripe plants being automatically harvested with the items being placed into their inventory.</color> The radius is dependent on the amount of water in the container.\nThey have access to a class specific Vending Machine found at the Outpost.</color>", "#63CE25", "#C6B763");
                    new ARPGClass(ClassType.Packrat, "Packrat", "Luck XP when looting, --WC/M XP|++proc chance|++loot carry|re-roll crates|better keycards", "<color=#9AE90A>Packrats</color> <color=#EA1977>greatest ambition in life is simply ''more''. They are known for their tenacity to loot everything they see, and as a result <color=#9AE90A>have a higher chance to proc Luck abilities when looting</color>. When finding weapons in crates, they'll have higher durability for <color=#9AE90A>Packrats</color>. They are also able to <color=#9AE90A>carry more components and general items</color>. Their active ability, '<color=#9AE90A>Magpie</color>', allows them to <color=#9AE90A>re-roll crates</color>, and they've learned how to best maximize their keycards, <color=#9AE90A>resulting in less damage per swipe</color>.\nThey have access to a class specific Vending Machine found at the Outpost.</color>", "#9AE90A", "#EA1977");
                    new ARPGClass(ClassType.Mercenary, "Mercenary", "Luck XP from PvE, --WC/M XP|++S XP|++ammo/med carry|PvE bonus|better keycards|++outpost prices", "<color=#D2A688>Mercs are what you'd expect. Pay 'em enough, nobody's life is meaningful to them. Mercs can take hits from <color=#9A99A9>Airwolf</color> @ <color=#9A99A9>Bandit Camp</color>. These hits range in difficulty. The harder the hit, the more significant the reward. Take the hit, find the enemy, return dogtags to <color=#9A99A9>Airwolf</color> after you kill them. Mercs must select the <i>''I'm just browsing''</i> option at Airwolf to get access to the Merc hit menu & to claim rewards. <i>Only mercs can take the hits</i>, but <color=#9A99A9>all</color> players can claim a hit for themselves - just need the dogtags. Mercs have the ''<color=#89FE1A>Overdrive</color>'' ability, it must be purchased via <color=#89FE1A>/skt</color> & can be activated by pressing RMB & MMB at the same time. Bending reality to their will, they have unlimited ammo and guaranteed headshots against NPCs for a few seconds. Mercs earn XP from PvE events, such as taking down the <color=#89FE1A>Helicopter</color> & <color=#9A99A9>Bradley</color>. Mercs deal additional PvE damage.</color>", "#9A99A9", "#D2A688");

                    // new ARPGClass(ClassType.Engineer, "Engineer", "Luck XP when building, reduced TC costs, free repair chance, faster crafting, less XP from others", "long_description");

                    //   new ARPGClass(ClassType.Hunter, "Hunter", "Luck XP when hunting, less XP from others, higher chance to proc, carry more animal items", "long_description");


                    for (int i = 0; i < Enum.GetValues(typeof(ClassType)).Length; i++)
                    {
                        var type = (ClassType)i;

                        if (type == ClassType.Undefined) continue;

                        var findClassByType = ARPGClass.FindByType(type);
                        if (findClassByType == null)
                        {
                            PrintWarning("type: " + type + " does not have an ARPG class yet, so it wasn't actually created. we're going to create a placeholder now");
                            new ARPGClass(type, "t_" + type + "_" + i, "tshortdesc_" + type, "tdesc_" + type);
                        }
                    }

                }
                catch (Exception ex)
                {
                    PrintError(ex.ToString());
                }


                TryLoadZLevelsColors();
                TryLoadZLevelsMessages();
                var colCount = ZLevelsColors?.Count ?? 0;
                var msgCount = ZLevelsMessages?.Count ?? 0;
                if (colCount < 1)
                {
                    timer.Repeat(2f, 5, () =>
                    {
                        if (colCount < 1)
                            TryLoadZLevelsColors();
                    });
                }
                if (msgCount < 1)
                {
                    timer.Repeat(2f, 5, () =>
                    {
                        if (msgCount < 1)
                            TryLoadZLevelsMessages();
                    });
                }


                foreach (var entity in BaseNetworkable.serverEntities)
                {
                    if (entity == null) continue;

                    var lootCont = entity as LootContainer;
                    var dropped = entity as DroppedItem;

                    if (lootCont != null) allContainers.Add(lootCont);
                    if (dropped != null) allDropped.Add(dropped);

                    var droppedItemContainer = entity as DroppedItemContainer;

                    if (droppedItemContainer != null && !droppedItemContainer.IsDestroyed && IsBackpackSkin(droppedItemContainer.skinID)) 
                        droppedItemContainer.CancelInvoke(new Action(droppedItemContainer.RemoveMe));

                }

                StartCoroutine(TrapInit());

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p != null && p.IsConnected) UpdateGUI(p);
                }

                var hasAchievements = PRISMAchievements?.IsLoaded ?? false;
                if (hasAchievements)
                {

                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        for (int i = 0; i < _statInfos.Count; i++)
                        {
                            var stat = _statInfos[i];

                            var score = 320 * stat.MaxLevel; //3200 for class stats (max level 10)

                            var achievementShortName = sb.Clear().Append(stat.ShortName).Append("_mastery").ToString();
                            var achievementDisplayName = sb.Clear().Append(stat.DisplayName).Append(" Mastery").ToString();
                            var achievementDescription = sb.Clear().Append("Reach skill level ").Append(stat.MaxLevel.ToString("N0")).Append(" in ").Append(stat.DisplayName).ToString();

                            PRISMAchievements.Call("RegisterAchievementFromPlugin", achievementShortName, achievementDisplayName, this, achievementDescription, 6, score);

                        }
                    }
                    finally { Pool.Free(ref sb); }

                    /*/
                    PRISMAchievements.Call("RegisterAchievementFromPlugin", "whirlwind_mastery", "Whirlwind Mastery", this, "Reach skill level 10 in Whirlwind", 6, 3200);
                    PRISMAchievements.Call("RegisterAchievementFromPlugin", "wotw_mastery", "Weight of the World Mastery", this, "Reach skill level 10 in Weight of the World", 6, 3200);
                    PRISMAchievements.Call("RegisterAchievementFromPlugin", "botanist_mastery", "Botanist Mastery", this, "Reach skill level 10 in Botanist", 6, 3200);
                    PRISMAchievements.Call("RegisterAchievementFromPlugin", "thief_mastery", "Thief Mastery", this, "Reach skill level 10 in Thief", 6, 3200);
                    PRISMAchievements.Call("RegisterAchievementFromPlugin", "magpie_mastery", "Magpie Mastery", this, "Reach skill level 10 in Magpie", 6, 3200);/*/
                }

                if (_resetClassesOnInit && _userClass != null && _userClass.Count > 0)
                {
                    PrintWarning(nameof(_resetClassesOnInit) + " is true, resetting all users classes. Count before: " + _userClass?.Count);

                    if (DiscordAPI2?.IsLoaded ?? false)
                    {
                        //clear all user roles from discord
                        foreach (var kvp in _userClass)
                            DiscordAPI2.Call("RemoveRoleWithColor", kvp.Key, kvp.Value.ToString(), ARPGClass.FindByType(kvp.Value).PrimaryColor);

                    }

                    _userClass.Clear();

                    PrintWarning("reset all user classes!");

                }


                PrintWarning("Starting SaveAllData() coroutine");

                _saveCoroutine = StartCoroutine(SaveAllData(1800, true)); //30 minutes is default interval

                PrintWarning("SaveAllData() coroutine started & saved to be canceled on unload");

                _hitGenerateAction = new Action(() =>
                {
                    GenerateHitTargets();
                });

                ServerMgr.Instance.InvokeRepeating(_hitGenerateAction, 1f, 1800f);

                StartCoroutine(TakeMetabolicSnapshotsOfAllPlayers(TimeSpan.FromMinutes(5)));

            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintError("Failed to complete OnServerInitialized()");
            }
            init = true;
        }

     

        private IEnumerator TrapInit()
        {

            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                var traps = Pool.Get<HashSet<WildlifeTrap>>();

                try 
                {
                    traps.Clear();

                    foreach(var entity in BaseEntity.saveList)
                    {
                        var trap = entity as WildlifeTrap;

                        if (trap != null)
                            traps.Add(trap);
                    }

                    var count = 0;

                    foreach (var trap in traps)
                    {
                        count++;
                        var ownerID = trap?.OwnerID ?? 0;

                        var trapExt = trap?.GetComponent<WildTrapExt>() ?? trap?.gameObject?.AddComponent<WildTrapExt>();

                        if (ownerID != 0) AdjustTickRate(trap, ownerID);

                        if (count >= 5)
                        {
                            count = 0;
                            yield return null;
                        }
                    }

                }
                finally 
                {
                    traps?.Clear();
                    Pool.Free(ref traps); 
                }

            }
            finally
            {
                try { if (watch.ElapsedMilliseconds > 50) PrintWarning(nameof(TrapInit) + " took: " + watch.ElapsedMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }
        }

        private IEnumerator CharmPurge()
        {
            try 
            {
                var watch = Stopwatch.StartNew();
                var remCount = 0;
                var totalCount = 0;
                var charms = charmsData?.charms?.ToDictionary(p => p.Key, p => p.Value) ?? null;
                var loopCount = charms?.Count ?? 0;

                if (loopCount < 1)
                {
                    PrintWarning("No charms to purge on " + nameof(charmPurge));
                    yield break;
                }

                var charmSB = new StringBuilder();

                var toRemove = Pool.GetList<ulong>();

                try
                {
                    var lastpercent = 0;
                    var fpsAvgBefore = Performance.report.frameRateAverage;
                    var i = 0;
                    foreach (var charmData in charms)
                    {
                        i++;

                        var charm = charmData.Value;

                        var charmID = charmData.Key;

                        var percent = (int)(i / (float)loopCount * 100);

                        if (percent != lastpercent)
                        {
                            if (percent % 10 == 0) Puts("Charm Purge: " + percent.ToString("N0") + "% done. Total time taken: " + watch.Elapsed.TotalMilliseconds.ToString("0.##") + "ms");
                            lastpercent = percent;
                        }

                        totalCount++;

                        var findCharm = FindItemByIDU(charmID);
                        if (findCharm == null)
                        {
                            toRemove.Add(charmID);
                            charmSB.AppendLine("Skill: " + charm.Skill + ", type: " + charm.Type + ", item bonus: " + charm.itemBonus + ", xp bonus: " + charm.xpBonus + ", item id: " + charmID);
                            remCount++;
                        }
                        else
                            _charmToItemCache[charm] = findCharm;
                        

                        if (Performance.report.frameRate < 70 && fpsAvgBefore >= 70) 
                            yield return CoroutineEx.waitForSeconds(1.25f);

                        if (totalCount >= 250)
                        {
                            totalCount = 0;
                            yield return CoroutineEx.waitForSeconds(0.175f);
                        }
                    }

                    if (toRemove.Count > 0) { for (i = 0; i < toRemove.Count; i++) charmsData.charms.Remove(toRemove[i]); }
                }
                finally { Pool.FreeList(ref toRemove); }

                if (charmSB.Length > 0) PrintWarning("Removed charms info: " + Environment.NewLine + charmSB.ToString().TrimEnd());

                PrintWarning("Removed " + remCount.ToString("N0") + " charms without an item in: " + watch.Elapsed.TotalMilliseconds + "ms. Total charm count (before removal) was: " + loopCount.ToString("N0"));
            }
            finally
            {
                CharmPurgeCoroutine = null;
            }
        }

        private void OnPluginLoaded(Plugin plugin)
        {

            if ((plugin?.Name ?? string.Empty).Equals(nameof(ZLevelsRemastered), StringComparison.OrdinalIgnoreCase))
            {
                Puts("Detected zlevels load/reload");
                TryLoadZLevelsColors();
                TryLoadZLevelsMessages();
            }
        }


        public void SetPointsAndLevel(string userID, float points, long level, float xpTime = 2.5f)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentOutOfRangeException(nameof(userID));
            if (points < 0) throw new ArgumentOutOfRangeException(nameof(points));
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (xpTime < 0) throw new ArgumentOutOfRangeException(nameof(xpTime));


            var luckInfo = GetLuckInfo(userID);
            if (luckInfo == null)
            {
                luckInfo = new LuckInfo(userID);
                storedData3.luckInfos[userID] = luckInfo;
            }

            var startXP = luckInfo?.XP ?? 0;
            var player = FindPlayerByIdS(userID);
            luckInfo.XP = points;
            luckInfo.Level = level;

            if (player == null || !player.IsConnected || points == startXP)
                return;

            var xpGain = points - startXP;

            var xpHit = ZLevelsRemastered?.Call<bool>("HasXPHit", userID) ?? false;

            if (xpHit)
            {
                UpdateGUI(player, (int)xpGain); //todo: xp - ??? can this comment be removed? idk.

                player?.Invoke(() => UpdateGUI(player), xpTime + 0.1f);
            }
            else UpdateGUI(player);
        }


        private void UpdateGUI(BasePlayer player, int addedPoints = 0)
        {
            if (player == null || !player.IsConnected) 
                return;

            var now = Time.realtimeSinceStartup;
            if (_lastGuiUpdate.TryGetValue(player.UserIDString, out float lastTime) && now - lastTime < 0.2)
                return;

            if (LevelsGUI == null || !LevelsGUI.IsLoaded)
            {
                PrintWarning("no ui handler");
                return;
            }

            var info = GetLuckInfo(player);
            if (info == null) return;

            _lastGuiUpdate[player.UserIDString] = now;

            LevelsGUI.Call("GUIUpdateSkill", player, "Luck", info.Level, (float)GetExperiencePercent(player), addedPoints, "0.365 0.776 0.25 1");
        }

        private void OnLevelsGUICreated(BasePlayer player)
        {
            if (player == null)
            {
                PrintWarning("OnLevelsGUICreated called with null player!!");
                return;
            }
            UpdateGUI(player);
        }

        public void AddPoints(ulong userID, float points)
        {
            AddPointsS(userID.ToString(), points);
        }

        public void AddPointsS(string userID, float points)
        {
            if (string.IsNullOrEmpty(userID)) 
                throw new ArgumentNullException(nameof(userID));

            SetAndHandlePoints(userID, getPointsS(userID) + points);
        }

        private void AddXP(string userId, float points, XPReason reason = XPReason.Generic)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var userClass = GetUserClassType(userId);

            var adjustedPoints = points;
            var scalar = reason switch
            {
                XPReason.Loot => userClass == ClassType.Packrat ? 1f : (userClass == ClassType.Loner || userClass == ClassType.Nomad) ? 0.33f : 0.15f,
                XPReason.Ore => userClass == ClassType.Miner ? 1f : (userClass == ClassType.Loner || userClass == ClassType.Nomad) ? 0.33f : 0.15f,
                XPReason.Tree => userClass == ClassType.Lumberjack ? 1f : (userClass == ClassType.Loner || userClass == ClassType.Nomad) ? 0.33f : 0.15f,
                XPReason.HarvestPlant => userClass == ClassType.Farmer ? 1f : (userClass == ClassType.Loner || userClass == ClassType.Nomad) ? 0.33f : 0.15f,
                XPReason.PvEKill or XPReason.PvEHit => (userClass == ClassType.Mercenary || userClass == ClassType.Loner || userClass == ClassType.Nomad) ? 1f : 0.15f,
                XPReason.Fish => (userClass == ClassType.Loner || userClass == ClassType.Nomad || userClass == ClassType.Farmer) ? 1f : 0.15f,
                _ => 1f,
            };
            adjustedPoints *= scalar;

            AddPointsS(userId, adjustedPoints);
        }

        public void setLastConnectionTime(ulong userID)
        {
            var luckInfo = GetLuckInfo(userID);
            if (luckInfo == null) return;
            luckInfo.LastConnectionTime = DateTime.Now.Ticks;
        }

        public void loadUser(BasePlayer player)
        {
            if (player == null) return;
            var luckInfo = GetLuckInfo(player);

            var LKLevel = luckInfo?.Level ?? 1;

            if (LKLevel < 1)
            {
                PrintWarning("luckInfo.Level < 1!!! - adjusting");
                LKLevel = 1;
            }

            var startingPoints = GetPointsForLevel(1);

            var LKpoints = luckInfo?.XP ?? startingPoints;
            if (LKpoints < startingPoints && luckInfo != null) luckInfo.XP = startingPoints;

            SetPointsAndLevel(player.UserIDString, LKpoints, LKLevel);

            if (player.IsConnected) UpdateGUI(player);
        }


        //WHIRLWIND COLLECTIONS
        private readonly Dictionary<string, float> lastWhirlwindAttack = new();
        private readonly Dictionary<string, float> lastWhirlwindInit = new();
        private readonly Dictionary<string, List<BaseEntity>> _playerWWHatchets = new();

        private readonly HashSet<string> whirlwindPlayers = new();
        private readonly HashSet<string> _activeWhirlwind = new();

        private readonly Dictionary<string, float> lastCapCheck = new();

        private void OnWhirlwindStart(BasePlayer player)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;

            var whirlwind = GetLuckInfo(player)?.GetStatLevelByName("Whirlwind") ?? 0;

            if (whirlwind < 1)
            {
                PrintWarning(nameof(OnWhirlwindStart) + " called with no whirlwind!!");
                return;
            }

            if (HasFusionSkill(player, "Whirlwind"))
            {
                PrintWarning("is fusion ww!: " + player);

                var fusionRng = Random.Range(0, 101);

                var fusionLvl = GetLuckInfo(player)?.GetStatLevelByName("Fusion") ?? 0;

                var neededRng = fusionLvl == 1 ? 33 : fusionLvl == 2 ? 16 : 8;

                if (fusionRng <= neededRng || player.IsAdmin)
                {
                    PrintWarning("ww hit fusion rng ON start OR is admin (temp).");

                    Effect.server.Run("assets/bundled/prefabs/fx/player/beartrap_scream.prefab", player, 0, Vector3.zero, Vector3.zero);

                    var oldHp = player.Health();

                    var healthScalar = fusionLvl == 1 ? 0.375f : fusionLvl == 2 ? 0.48f : 0.5f;

                    player.SetHealth(oldHp * healthScalar);
                    player.metabolism.pending_health.Add(oldHp * 0.66f);

                    var bleedRng = 40 / fusionLvl;

                    if (Random.Range(0, 101) <= bleedRng)
                        player.metabolism.bleeding.Add(Random.Range(8f, 30f));

                    SendLuckNotifications(player, "<color=red>CORRUPTION. <color=yellow>FUSION.</color></color>");
                }

            }

            var vars = Pool.GetList<ClientVarWithValue>();

            try
            {
                GetStormWeatherVarsNoAlloc(ref vars);

                var orgPos = player?.transform?.position ?? Vector3.zero;

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || p.IsDestroyed || p.IsDead() || !p.IsConnected) continue;

                    var pPos = p?.transform?.position ?? Vector3.zero;

                    if (Vector3.Distance(pPos, orgPos) < 40)
                    {
                        _hasStormWeatherVars.Add(p);
                        SendClientVars(p?.Connection, vars);
                    }
                }


            }
            finally { Pool.FreeList(ref vars); }

            var now = Time.realtimeSinceStartup;

            lastWhirlwindAttack[player.UserIDString] = now;

            var endTime = 12f + (whirlwind - 1);

            player.Invoke(() =>
            {
                if (player == null) return;



                if (whirlwindPlayers.Remove(player.UserIDString))
                {
                    OnWhirlwindEnd(player);

                    var wwCooldownHandler = player?.GetComponent<WhirlwindCooldownHandler>() ?? player?.gameObject?.AddComponent<WhirlwindCooldownHandler>();

                    wwCooldownHandler.StartCooldown();
                }
                else PrintWarning("couldn't remove from whirlwind players on invoke!!!! already ended?");

            }, endTime);


            PrintWarning(nameof(OnWhirlwindStart) + " - going to spawn hatchets!!");

            if (!_playerWWHatchets.TryGetValue(player.UserIDString, out List<BaseEntity> hatchets))
            {
                _playerWWHatchets[player.UserIDString] = hatchets = new List<BaseEntity>();
            }

            var hatchetCount = 12 + (int)(1.5 * (whirlwind - 1));

            PrintWarning("hatchetCount: " + hatchetCount);

            for (int i = 0; i < hatchetCount; i++)
            {
                if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected)
                {
                    PrintWarning("player null/dead on hatchet i: " + i);
                    return;
                }

                var hatchet = GameManager.server.CreateEntity("assets/prefabs/weapons/hatchet/hatchet.entity.prefab", SpreadVector(player.transform.position, Random.Range(0.8f, 1.1f)));



                hatchet.Spawn();
                hatchets.Add(hatchet);

                hatchet.InvokeRepeating(() =>
                {
                    if (hatchet == null || hatchet.IsDestroyed || player == null || player.IsDestroyed) return;

                    var plyHeadPos = player?.eyes?.transform?.position ?? Vector3.zero;
                    hatchet.transform.position = SpreadVector(plyHeadPos, Random.Range(0.33f, 1f));
                    hatchet.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                    hatchet.SendNetworkUpdateImmediate();


                }, 0.1f, 0.05f);

                hatchet.InvokeRepeating(() =>
                {
                    if (hatchet == null || hatchet.IsDestroyed) return;

                    if (Random.Range(0, 101) <= (hatchetCount > 20 ? 12 : hatchetCount > 15 ? 25 : 35))
                        Effect.server.Run("assets/bundled/prefabs/fx/player/swing_weapon.prefab", hatchet.transform.position, Vector3.zero);

                }, 0.1f, Random.Range(0.33f, 0.44f));

            }

            _playerWWHatchets[player.UserIDString] = hatchets;

        }

        private void OnWhirlwindEnd(BasePlayer player, bool sendEnvSyncUpdateToPlayer = true)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;

            if (sendEnvSyncUpdateToPlayer)
            {

                player.Invoke(() =>
                {
                    if (player == null || player.IsDestroyed || player.gameObject == null || !player.IsConnected) return;

                    if (_activeWhirlwind.Contains(player.UserIDString))
                    {
                        PrintWarning("player is whirlwinding again? activeWhirlwind contains on storm invoke - so not clearing storm");
                        return;
                    }

                    foreach (var p in _hasStormWeatherVars)
                    {
                        if (p == null || !p.IsConnected || p.IsDead()) continue;

                        var pos = p?.transform?.position ?? Vector3.zero;

                        var listVariables = Pool.GetList<ClientVarWithValue>();
                        try
                        {
                            GetCurrentWeatherVarsNoAlloc(ref listVariables, pos);

                            SendClientVars(p.Connection, listVariables);
                        }
                        finally { Pool.FreeList(ref listVariables); }

                    }



                }, 1.75f);
            }

            _activeWhirlwind.Remove(player.UserIDString);

            if (!_playerWWHatchets.TryGetValue(player.UserIDString, out List<BaseEntity> hatchets) || hatchets.Count < 1)
                return;


            for (int i = 0; i < hatchets.Count; i++)
            {
                var hatchet = hatchets[i];
                if (hatchet != null && !hatchet.IsDestroyed) hatchet.Kill();
            }
        }

        private object OnActiveItemChange(BasePlayer player, Item oldItem, ItemId itemId)
        {
            if (player == null || !itemId.IsValid || !IsValidPlayer(player)) 
                return null;

            var findItem = player.inventory.containerBelt.FindItemByUID(itemId);

            if (findItem == null)
                return null;      

            if (findItem.info.itemid != GEIGER_COUNTER_ITEM_ID)
                return null;

            var radiationTrigger = GetMaxRadiationTrigger(player);

            if (radiationTrigger != null) //is in radiation. allow geiger.
                return null;

            var oldPos = findItem?.position ?? -1;

            var oldParent = findItem?.parent;

            findItem.RemoveFromContainer();

            var dropPos = player?.GetDropPosition() ?? Vector3.zero;
            var dropVel = player?.GetDropVelocity() ?? Vector3.zero;

            player?.Invoke(() =>
            {
                if (findItem == null)
                    return;

                if (!findItem.MoveToContainer(oldParent, oldPos) && !findItem.Drop(dropPos, dropVel))
                    RemoveFromWorld(findItem);


            }, 0.05f);

            SendCooldownMessage(player, "This can only be equipped while in a radioactive area!", 2f, "GEIGER_COUNTER_UNAVAILABLE");

            return true; //don't allow set active item (this doesn't affect view model which is why we forcefully remove it above)
        }

        private TriggerTemperature EnsureForgesTrigger(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));


            if (_forgesTrigger.TryGetValue(player.UserIDString, out TriggerTemperature trigger))
            {
                if (!player.triggers.Contains(trigger))
                    PrintWarning(nameof(EnsureForgesTrigger) + " for " + player + " does not have temp trigger!");

                return trigger;
            }

            var tempTrigger = player.gameObject.AddComponent<TriggerTemperature>();

            player.EnterTrigger(tempTrigger);

            _forgesTrigger[player.UserIDString] = tempTrigger;

            return tempTrigger;
        }

        private TriggerTemperature GetForgesTrigger(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            return _forgesTrigger.TryGetValue(player.UserIDString, out TriggerTemperature trigger) ? trigger : null;
        }

        private void StopForgesTrigger(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!_forgesTrigger.TryGetValue(player.UserIDString, out TriggerTemperature trigger))
                return;

            try
            {
                player.LeaveTrigger(trigger);

                _forgesTrigger.Remove(player.UserIDString);
            }
            finally { UnityEngine.Object.Destroy(trigger); }
           

            
        }

        private IEnumerator BurningForgesCoroutine(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var trigger = EnsureForgesTrigger(player);

            trigger.Temperature = 3f;

            trigger.triggerSize = 0.75f;

            while (player != null && !player.IsDestroyed && player.IsAlive() && (GetActiveItem(player)?.skin ?? 0) == BURNING_FORGES_SKIN_ID)
            {
                trigger.Temperature += 1f; //damage scale is capped instead of this.

                if (player.metabolism.temperature.value < 40f)
                {
                    var sb = Pool.Get<StringBuilder>();
                    try { ShowToast(player, sb.Clear().Append(player.metabolism.temperature.value.ToString("0.00").Replace(".00", string.Empty)).Append("\u00B0c").ToString()); }
                    finally { Pool.Free(ref sb); }
                }

                yield return CoroutineEx.waitForSecondsRealtime(1f);
            }

            Interface.Oxide.LogWarning("while failed, yield break!");

            if (player != null)
                StopForgesTrigger(player);

            yield break;
        }

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item item)
        {
            if (!IsValidPlayer(player))
                return;

            if (_activeItemCache.TryGetValue(player, out var itemCache))
                itemCache.ForceNextRun();

            if ((oldItem == null && item == null) || !IsValidPlayer(player)) return;

         
            

            if (_activeWhirlwind.Contains(player.UserIDString))
                return;

            var itemId = item?.info?.itemid ?? 0;
            if (!IsPickaxeItemID(itemId) && _userMiningMapMarkers.TryGetValue(player.UserIDString, out HashSet<MapMarkerGenericRadius> markers))
            {
                foreach (var marker in markers)
                {
                    if (marker == null || marker.IsDestroyed)
                        continue;

                    marker.OwnerID = 404;
                    marker.alpha = 0f;
                    marker.SendUpdate();

                }
            }

            if (oldItem?.info?.itemid == ABYSS_PICK_ID || item?.info?.itemid == ABYSS_PICK_ID)
            {
                _wotwPlayerOres.Remove(player.UserIDString);
                _consecutiveGroundHits.Remove(player.UserIDString);
            }

            if ((oldItem?.skin ?? 0) == BURNING_FORGES_SKIN_ID || (item?.skin ?? 0) != BURNING_FORGES_SKIN_ID)
            {
                StopForgesTrigger(player);


                if (_forgesCoroutine.TryGetValue(player.UserIDString, out Coroutine routine))
                {
                    StopCoroutine(routine);
                    _forgesCoroutine.Remove(player.UserIDString);
                }

            }
            else if ((item?.skin ?? 0) == BURNING_FORGES_SKIN_ID)
            {
                if (_forgesCoroutine.TryGetValue(player.UserIDString, out _))
                    PrintWarning("already had active coroutine when switched to burning forges?!?!?!");
                else
                    _forgesCoroutine[player.UserIDString] = StartCoroutine(BurningForgesCoroutine(player));
            }

            if (item == null) //no new item, just unequipped old
                return;

            var heldEntity = item?.GetHeldEntity();

            var rod = heldEntity as BaseFishingRod;

            if (rod != null)
                _playerToRodCache[player] = rod;
            else _playerToRodCache.Remove(player);

            _lastActiveItem[player] = item;

            var now = Time.realtimeSinceStartup;

            if (heldEntity != null && heldEntity is BaseProjectile && GetLuckInfo(player).GetStatLevelByName("Overdrive") > 0 && (!_lastOverDriveReminder.TryGetValue(player.UserIDString, out float lastOdRemind) || (now - lastOdRemind) >= 90) && !(player?.GetComponent<OverdriveCooldownHandler>()?.IsOnCooldown ?? false))
            {
                _lastOverDriveReminder[player.UserIDString] = now;

                var primaryColor = "#F5393A";
                var secondaryColor = "#89FE1A";

                var odReminderMsg = "<color=" + secondaryColor + ">Don't forget about <color=" + primaryColor + "><i><size=16>Overdrive!</size></i></color>\nTo activate your <color=" + primaryColor + "><i>Overdrive</i></color> ability, aim down the sights of your <i><color=" + primaryColor + ">" + (item?.info?.displayName?.english ?? "gun") + "</color> and press <color=" + primaryColor + ">Middle Mouse Button!</color></color>";
                var odReminderShortMsg = "<color=" + primaryColor + "><size=16><i>Overdrive!</i></size></color>\n<color=" + secondaryColor + ">To activate <color=" + primaryColor + ">Overdrive</color>, ADS and press <color=" + primaryColor + ">Right Mouse Button</color>.</color>";

                SendReply(player, odReminderMsg);
                ShowToast(player, odReminderShortMsg);
            }

            var leg = LegendaryItemInfo.GetLegendaryInfoFromItem(item);

            var shouldSendLegInfo = false;


            if (!_lastLegendaryNotice.TryGetValue(player.UserIDString, out float lastLegNotice) || (now - lastLegNotice) >= 45)
                shouldSendLegInfo = true;

            var legSb = Pool.Get<StringBuilder>();
            try 
            {
                legSb.Clear();

                if (leg != null)
                {

                    if (item.skin == WINDFORCE_BOW_SKIN_ID)
                    {


                        var bow = heldEntity as BowWeapon;
                        if (bow != null && bow.primaryMagazine.capacity != 4)
                        {
                            bow.primaryMagazine.capacity = 4;
                            bow.SendNetworkUpdate();
                        }
                    }
                    else if (item.skin == TWITCHY_TOMMY_SKIN_ID || item.skin == GRISWOLDS_BLUNDERBUSS_SKIN_ID)
                    {

                        var desiredCap = item.skin == TWITCHY_TOMMY_SKIN_ID ? 30 : 2;

                        var gun = heldEntity as BaseProjectile;
                        if (gun != null && gun.primaryMagazine.capacity < desiredCap)
                        {
                            gun.primaryMagazine.capacity = desiredCap;
                            gun.SendNetworkUpdate();
                        }
                    }
               





                    //needs sb

                    if (shouldSendLegInfo)
                    {
                        _lastLegendaryNotice[player.UserIDString] = now;

                        var modStr = leg.GetModifiersString();
                        legSb.Append("<color=#F37B00>").Append(leg.DisplayName).Append("</color>:").Append(!string.IsNullOrWhiteSpace(modStr) ? "\n" : "").Append(modStr).Append((!string.IsNullOrEmpty(leg.BonusDescription) ? "\n" : "")).Append(leg.BonusDescription).Append(Environment.NewLine); //final \n is trimmed, this is only in case of leg item + leg attachment(s)

                        //SendLuckNotifications(player, "t", true, 2f);
                    }

                }

                if (shouldSendLegInfo && item?.contents?.itemList != null && item.contents.itemList.Count > 0)
                {
                    for (int i = 0; i < item.contents.itemList.Count; i++)
                    {
                        var contentLeg = LegendaryItemInfo.GetLegendaryInfoFromItem(item.contents.itemList[i]);

                        if (contentLeg == null)
                            continue;

                        _lastLegendaryNotice[player.UserIDString] = now;

                        var modStr = contentLeg.GetModifiersString();
                        legSb.Append("<color=#F37B00>").Append(contentLeg.DisplayName).Append(" (attached to your weapon)</color>:").Append(!string.IsNullOrWhiteSpace(modStr) ? "\n" : "").Append(modStr).Append((!string.IsNullOrEmpty(contentLeg.BonusDescription) ? "\n" : "")).Append(contentLeg.BonusDescription);


                    }
                }

                if (legSb.Length > 0) //yay for sb optimizations!
                    SendLuckNotifications(player, legSb.ToString(), true, 3.5f);

            }
            finally { Pool.Free(ref legSb); }

           

          


            if (item.info.itemid != CHAINSAW_ID) return;

            var chainsaw = item?.GetHeldEntity() as Chainsaw;
            if (chainsaw == null)
            {
                PrintWarning("chainsaw null despite item id being chainsaw?!!?");
                return;
            }

            var userClass = GetUserClassType(player.UserIDString);

            var isLumberjack = userClass == ClassType.Lumberjack;

            if (!isLumberjack && chainsaw.maxAmmo != 50)
            {
                if (chainsaw.ammo > 50)
                {
                    NoteItemByDef(player, chainsaw.fuelType, chainsaw.ammo - 50);

                    var lgf = ItemManager.Create(chainsaw.fuelType, chainsaw.ammo - 50);

                    player.GiveItem(lgf);

                    chainsaw.ammo = 50;
                }

                chainsaw.maxAmmo = 50;
                chainsaw.SendNetworkUpdate();
            }
            else if (isLumberjack && chainsaw.maxAmmo != 400)
            {
                chainsaw.maxAmmo = 400;
                chainsaw.SendNetworkUpdate();
            }

        }

        private void OnWaterThrown(BasePlayer player, BaseLiquidVessel vessel, Vector3 pos, Vector3 velocity, int waterThrownAmount)
        {
            if (player == null || vessel == null) return;

            var botanist = GetLuckInfo(player)?.GetStatLevelByName("Botanist") ?? 0;
            if (botanist < 1) return;

            var cooldownHandler = player?.GetComponent<BotanistCooldownHandler>() ?? player?.gameObject?.AddComponent<BotanistCooldownHandler>();

            if (cooldownHandler.IsOnCooldown)
            {
                PrintWarning("Is on cooldown!! (botanist)");
                return;
            }

            var plyPos = player?.transform?.position ?? Vector3.zero;

            var nearPlants = Pool.GetList<GrowableEntity>();
            try
            {
                var dist = 3 * (botanist * 1.25f) * Mathf.Clamp(waterThrownAmount * 0.0025f, 0.1f, 2f);

                PrintWarning("dist: " + dist + " (waterThrownAmount = " + waterThrownAmount + "), * 0.025f: " + (waterThrownAmount * 0.025f));

                Vis.Entities(pos, dist, nearPlants, 524288, QueryTriggerInteraction.Ignore);

                var picked = 0;

                for (int i = 0; i < nearPlants.Count; i++)
                {
                    var plant = nearPlants[i];

                    if (plant == null || plant.IsDestroyed || plant.State != PlantProperties.State.Ripe) continue;

                    if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected)
                    {
                        PrintWarning("player null/dead/destroyed/disconnected!! breaking plant loop");
                        break;
                    }

                    if (!plant.IsVisible(plyPos, dist + 5)) continue;

                    var plantPos = plant?.transform?.position ?? Vector3.zero;

                    plant.PickFruit(player);
                    picked++;

                    //these rng effect chances are sort of for performance with huge farms
                    if (nearPlants.Count < 20 || Random.Range(0, 101) <= 90) Effect.server.Run("assets/prefabs/weapons/waterbucket/effects/waterimpact_explosion.prefab", plantPos);

                    if (Random.Range(0, 101) <= 85) Effect.server.Run("assets/prefabs/food/bota bag/effects/bota-bag-slosh.prefab", plantPos);
                    if (Random.Range(0, 101) <= 75) Effect.server.Run("assets/prefabs/misc/halloween/pumpkin_bucket/effects/add_egg.prefab", plantPos);
                    if (Random.Range(0, 101) <= 50) Effect.server.Run("assets/bundled/prefabs/fx/collect/collect mushroom.prefab", plantPos);

                }


                if (picked > 0)
                {
                    var baseXpAmount = 128 * picked;
                    var addXpAmount = Math.Log((baseXpAmount + 250.0) / 250.0) / Math.Log(2.0) * 250; //a degree of diminishing returns. unfortunately, at 128 * 1, this results in xp of 149.1 but that's okay 

                    var finalXp = DoCatchup(player.userID, DoMultipliers(player.userID, addXpAmount));

                    AddXP(player.UserIDString, (float)finalXp, XPReason.HarvestPlant);
                    PrintWarning("Added xp amount from botanist pickings: " + finalXp);

                    var sb = Pool.Get<StringBuilder>();
                    try
                    {


                        SendLuckNotifications(player, sb.Clear().Append("Your expertise in being a <color=").Append(LUCK_COLOR).Append(">Farmer</color> has allowed you to instantly pick <color=").Append(LUCK_COLOR).Append(">").Append(picked.ToString("N0")).Append("</color> of your nearby plants!").ToString());
                    }
                    finally { Pool.Free(ref sb); }
                }

            }
            finally
            {
                try
                {
                    Pool.FreeList(ref nearPlants);
                }
                finally { cooldownHandler.StartCooldown(); }
            }



        }

        private readonly HashSet<NetworkableId> _bonemealedPlants = new();

        private object OnGrowableGetGrowthBonus(GrowableEntity plant, float overallQuality)
        {
            if (plant?.net == null) return null;

            if (!_bonemealedPlants.Contains(plant.net.ID)) return null;
            else return 10000; //used to instantly mature plants (bonemeal)
        }


        /*/
        private class DistanceFromCaching : FacepunchBehaviour
        {
            public BaseNetworkable Entity { get; private set; }

            public BaseNetworkable Entity2 { get; private set; }

            public TimeCachedValue<float> Distance { get; private set; } = new TimeCachedValue<float>();


            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(DistanceFromCaching) + "." + nameof(Awake));

                Entity = GetComponent<BaseNetworkable>();

                if (Entity == null)
                {
                    throw new ArgumentNullException(nameof(Entity));
                }

                if (Entity.IsDestroyed || Entity.gameObject == null)
                {
                    Interface.Oxide.LogWarning(nameof(DistanceFromCaching) + " has destroyed ent!");

                    DoDestroy();
                    return;
                }

                Distance.refreshRandomRange = 5f;
                Distance.refreshCooldown = 10f;
                Distance.updateValue = new Func<float>(UpdateDistance);


            }

            public float UpdateDistance()
            {
                if (Entity == null || Entity.IsDestroyed || Entity.gameObject == null || Entity2 == null || Entity2.IsDestroyed || Entity2.gameObject == null)
                    return -1f;

                return Vector3.Distance(Entity.transform.position, Entity2.transform.position);

            }

            public void DoDestroy()
            {
                GameObject.Destroy(this);
            }

        }/*/


        private readonly Dictionary<ulong, TimeCachedValue<float>> _furnacePlayerDistanceCache = new Dictionary<ulong, TimeCachedValue<float>>();
        private void OnOvenCook(BaseOven oven, Item burnable)
        {
            if (oven == null)
                return;

            var owner = FindPlayerByID(oven.OwnerID);

            if (owner?.gameObject == null || !owner.IsConnected || owner.IsDead())
                return;

            if (!IsWearingSkin(owner, STEELHEART_SKIN_ID))
                return;

            var netId = oven.net.ID.Value;


            if (!_furnacePlayerDistanceCache.TryGetValue(oven.net.ID.Value, out _))
            {
                _furnacePlayerDistanceCache[oven.net.ID.Value] = new TimeCachedValue<float>()
                {
                    refreshRandomRange = 2.5f,
                    refreshCooldown = 10f,
                    updateValue = new Func<float>(() =>
                    {
                        if (owner == null || owner.IsDestroyed || owner.gameObject == null || oven == null || oven.IsDestroyed || oven.gameObject == null)
                            return -1f;

                        return Vector3.Distance(owner.transform.position, oven.transform.position);
                    })
                };


            }

           var dist = _furnacePlayerDistanceCache[oven.net.ID.Value].Get(false);

            PrintWarning("cache returned dist: " + dist);

            if (dist > 20)
                return;

            for (int i = 0; i < oven.inventory.itemList.Count; i++)
            {
                var slotItem = oven.inventory.itemList[i];

                if (slotItem == null)
                    continue;

                if (!oven.IsOutputItem(slotItem))
                    continue;

                var oldAmt = slotItem?.amount ?? 0;

                oven?.Invoke(() =>
                {
                    var diff = (slotItem?.amount ?? 0) - oldAmt;

                    if (diff <= 0)
                        return;


                    var itemScalar = slotItem.info.itemid switch
                    {
                        SULFUR_COOKED_ITEM_ID => 2.75d,
                        HQ_COOKED_ITEM_ID => 7d,
                        METAL_COOKED_ITEM_ID => 1.75d,
                        _ => 1.0d,
                    };

                    var itemDiffScalar = Mathf.Clamp(1.0f * (diff * 0.05f), 1.0f, 10f);

                    PrintWarning("slotItem: " + slotItem + ", diff: " + diff + ", itemScalar: " + itemScalar + " diff scalar: " + itemDiffScalar);

                    //figure out XP formula later. small amount of XP, but higher for more avluable things like slufur & hqm.
                    var baseXpAmount = 1.5d * (itemDiffScalar + itemScalar);
                    var addXpAmount = baseXpAmount; //Math.Log((baseXpAmount + 250.0) / 250.0) / Math.Log(2.0) * 250; //a degree of diminishing returns. unfortunately, at 128 * 1, this results in xp of 149.1 but that's okay 

                    var finalXp = (long)DoCatchup(owner.userID, DoMultipliers(owner.userID, addXpAmount));

                    ZLevelsRemastered?.Call("AddPointsBySkillName", owner.UserIDString, "M", finalXp, 0.667f);

                }, 0.01f);


            }

        }

        private void OnOvenCooked(BaseOven oven, Item item, BaseEntity slot)
        {
            if (oven is ElectricOven)
                return;

           


        }

        private object OnQuickSmeltConsumptionSet(BaseOven oven, Item fuel, ItemModBurnable burnable, float consumptionMult)
        {
            if (oven == null || fuel == null || burnable == null) return null;

            if (oven.OwnerID == 0 || GetUserClassType(oven.OwnerID.ToString()) != ClassType.Miner) return null;

            return consumptionMult *= 5;
        }

        private object OnQuickSmeltByproductMultiplier(BaseOven oven, Item fuel, ItemModBurnable burnable, float byproductMultiplier)
        {
            if (oven == null || fuel == null || burnable == null) return null;

            if (oven.OwnerID == 0 || GetUserClassType(oven.OwnerID.ToString()) != ClassType.Lumberjack) return null;

            return byproductMultiplier *= 2.5f;
        }

        private object OnNpcTarget(BaseEntity npc, BasePlayer player)
        {
            if (npc == null || !IsValidPlayer(player)) return null;

            var bce = npc as BaseCombatEntity;
            if (bce != null && _cannotTargetPlayers.Contains(bce))
            {
                PrintWarning("is bce, cannot target contained, so return true");
                return true; //do not allow targeting.
            }

            var animal = npc as BaseAnimalNPC;

            if (animal == null)
                return null;

            if (IsWearingItem(player, LUMBERJACK_SUIT_ID) && (animal?.lastAttacker != player))
                return true; //true to override targeting - wearing bunyans/lumberjack suit

            var hunterLvl = GetLuckInfo(player)?.GetStatLevelByName("Hunter") ?? 0;

            if (hunterLvl < 1)
                return null;

            var requiredDistToTarget = 24f * (1.0f - (hunterLvl * 0.133f));

            var dist = Vector3.Distance(npc?.transform?.position ?? Vector3.zero, player?.transform?.position ?? Vector3.zero);

            if (dist > requiredDistToTarget)
                return true;
            
       
            return null;
        }

        private readonly Dictionary<string, HashSet<MapMarkerGenericRadius>> _userMiningMapMarkers = new();
        

        private object CanNetworkTo(BaseNetworkable entity, BasePlayer target)
        {
            if (entity == null || target == null) return null;


            //minecap buff, prefabid is fireball_small
            if (entity.prefabID == 2086405370 && (entity as FireBall)?.OwnerID == 1221 && !IsWearingSkin(target, MINE_CAP_SKIN_ID))
                return false;
            

            var marker = entity as MapMarkerGenericRadius;

            if (marker == null) return null;

            if (marker.OwnerID == 404)
                return false;

            if (marker.OwnerID == 1996200123) //legendary items
            {
                var targetPos = target?.transform?.position ?? Vector3.zero;

                var normalizedYVector3Pos = marker?.transform?.position ?? Vector3.zero;
                normalizedYVector3Pos.y = targetPos.y;

                if (Vector3.Distance(normalizedYVector3Pos, targetPos) > 35)
                    return false;
                
            }
            else if (marker.OwnerID == 1996200121) //merc hits obviously
            {
                var hitComp = target?.GetComponent<MercenaryHitHandler>();
                if (hitComp == null || hitComp.MapMarker != marker) return false; //only network to correct person
            }

           

            if (marker.OwnerID != 19962001) return null;


            if (!_userMiningMapMarkers.TryGetValue(target.UserIDString, out HashSet<MapMarkerGenericRadius> markers) || !markers.Contains(marker))
                return false;


            return null;
        }

        private object CanShowSkinboxSkin(BasePlayer player, ulong skin, int itemId, Item originalItem = null)
        {
            if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected) return null;


            var originalSkin = originalItem?.skin ?? 0;
            var originalItemId = originalItem?.info?.itemid ?? 0;

            if (IsBlockedSkin(originalSkin) || IsBlockedItem(originalItemId) || IsBlockedSkin(skin) || IsBlockedItem(itemId))
                return false;

            return null;
        }


        private object CanSkinCycleSelectSkin(BasePlayer player, Item item, ulong nextSkinID)
        {
            return CanShowSkinboxSkin(player, nextSkinID, item.info.itemid, item);
        }

        private object CanSkinCycleChangeSkin(BasePlayer player, Item item)
        {
            return CanShowSkinboxSkin(player, item.skin, item.info.itemid, item);
        }

        private object CanSendCooldownStartedMessage(PlayerCooldownHandler handler, string message)
        {
            if (handler == null || string.IsNullOrWhiteSpace(message)) return null;

            SendLuckNotifications(handler.Player, message);

            return false;
        }

        private object CanSendCooldownEndedMessage(PlayerCooldownHandler handler, string message)
        {
            if (handler == null || string.IsNullOrWhiteSpace(message)) return null;

            SendLuckNotifications(handler.Player, message);

            return false;
        }

        private object IsBarrelExplodable(HitInfo info)
        {
            if (info?.Initiator as BasePlayer == null)
                return null;

            var wepItem = ((info?.Weapon ?? info?.WeaponPrefab) as BaseProjectile)?.GetItem();

            if (wepItem?.contents != null && ContainerHasSkin(wepItem.contents, PACKRATS_BANE_MUZZLE_BOOST_SKIN_ID))
                return true;

            return null;
        }

        private object CanBarrelExplode(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null)
                return null;

            var isExplodableObj = IsBarrelExplodable(info);
            if (isExplodableObj == null)
                return null;

            return (bool)isExplodableObj && (entity.prefabID == OIL_BARREL_PREFAB_ID || Random.Range(0, 101) <= 33);
        }

        private bool IsBlockedSkin(ulong skin)
        {
            return LegendaryItemInfo.GetLegendaryInfoFromSkinID(skin) != null;
        }

        private bool IsBlockedItem(int itemId)
        {
            var legendary = LegendaryItemInfo.GetLegendaryInfoFromItemID(itemId);

            return legendary != null && legendary.SkinID == 0;
        }

        private bool IsLegendaryItem(Item item)
        {
            if (item == null) 
                throw new ArgumentNullException(nameof(item));

            return LegendaryItemInfo.GetLegendaryInfoFromItem(item) != null;
        }

        private bool IsLightweightGunID(int itemId)
        {
            return itemId switch
            {
                PYTHON_REVOLVER_ID or REVOLVER_ID or PROTO_17_PISTOL_ID or SEMI_AUTO_PISTOL_ID or EOKA_PISTOL_ID or M92_PISTOL_ID or CUSTOM_SMG_ID => true,
                _ => false,
            };
        }

        private bool IsLightweightGun(Item item)
        {
            return IsLightweightGunID(item.info.itemid);
        }

        private bool IsHatchetItem(Item item)
        {
            return IsHatchetItemID(item.info.itemid);
        }

        private bool IsHatchetItemID(int itemId)
        {
            return itemId switch
            {
                CHAINSAW_ID or SALVAGED_AXE_ID or HATCHET_ID or STONE_AXE_ID or PROTO_AXE_ID or CONCRETE_AXE_ID or ABYSS_AXE_ID => true,
                _ => false,
            };
        }

        private bool IsPickaxeItem(Item item)
        {
            return item == null ? throw new ArgumentNullException(nameof(item)) : IsPickaxeItemID(item.info.itemid);
        }

        private bool IsPickaxeItemID(int itemId)
        {
            return itemId switch
            {
                PICKAXE_ID or ICE_PICK_ID or JACKHAMMER_ID or STONE_PICKAXE_ID or PROTO_PICK_ID or CONCRETE_PICK_ID or ABYSS_PICK_ID => true,
                _ => false,
            };
        }

        private object OnKevlarGetSkin(Item item, ulong skin)
        {
            if (item != null && IsLegendaryItem(item))
                return item.skin;
            else
                return null;
        }

        private object CanCraft(ItemCrafter crafter, ItemBlueprint blueprint, int amount, bool free)
        {
            if (crafter == null || blueprint == null) return null;


            var targetItemId = blueprint?.targetItem?.itemid ?? 0;
            if (targetItemId == 0) return null;

            if (IsBlockedItem(targetItemId))
            {
                PrintWarning("returning false on cancraft! itemid: " + targetItemId);
                return false;
            }


            return null;
        }

        private object OnItemSkinChange(int skinId, Item item, RepairBench bench, BasePlayer player)
        {
            if (IsLegendaryItem(item) || LegendaryItemInfo.GetLegendaryInfoFromSkinID((ulong)skinId) != null)
                return true;



            return null;
        }

        private object OnItemResearched(ResearchTable table, int amountToConsume)
        {
            if (table == null || amountToConsume <= 0) return null;

            var userId = table?.user?.UserIDString ?? (table?.OwnerID ?? 0).ToString();

            if (!string.IsNullOrEmpty(userId) && GetUserClassType(userId) == ClassType.Loner)
            {
                PrintWarning("is loner on research! returning amountToConsume * 0.75f: " + (amountToConsume * 0.75f));

                return (int)Math.Round(amountToConsume * 0.75f, MidpointRounding.AwayFromZero);
            }

            return null;
        }

        private object OnItemCraftFinishedItem(ItemCraftTask task, ulong skinId, ItemCrafter crafter)
        {
            if (LegendaryItemInfo.GetLegendaryInfoFromSkinID(skinId) != null)
                return false;


            return null;
        }

        private void AttachMarkerToAllNearbyOres(BasePlayer player, float radius)
        {
            if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected) return;

            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));

            if (!_userMiningMapMarkers.TryGetValue(player.UserIDString, out HashSet<MapMarkerGenericRadius> markers)) _userMiningMapMarkers[player.UserIDString] = new HashSet<MapMarkerGenericRadius>();
            else
            {
                var toRemove = Pool.Get<HashSet<MapMarkerGenericRadius>>();
                try
                {
                    toRemove.Clear();

                    foreach (var marker in markers)
                    {

                        var parent = marker?.GetParentEntity();
                        if (parent == null || parent.IsDestroyed)
                            toRemove.Add(marker);

                    }

                    foreach (var marker in toRemove)
                    {
                        markers.Remove(marker);

                        if (marker != null && !marker.IsDestroyed)
                            marker.Kill();
                    }

                }
                finally { Pool.Free(ref toRemove); }


            }

            var plyPos = player?.transform?.position ?? Vector3.zero;

            var nearbyOres = Pool.GetList<OreResourceEntity>();
            try
            {
                Vis.Entities(plyPos, radius, nearbyOres);

                var userIDString = player?.UserIDString ?? string.Empty;



                for (int i = 0; i < nearbyOres.Count; i++)
                {
                    var ore = nearbyOres[i];

                    if (ore == null || ore.IsDestroyed || ore.gameObject == null)
                        continue;

                    if (player?.gameObject == null)
                    {
                        PrintError("player null on mining loop!!");
                        break;
                    }

                    var alreadyHasMarker = false;

                    var children = ore?.children;
                    if (children != null)
                    {
                        for (int j = 0; j < children.Count; j++)
                        {
                            var markerChild = children[j] as MapMarkerGenericRadius;
                            if (markerChild == null)
                                continue;

                            if (markerChild != null && markers.Contains(markerChild))
                            {
                                if (markerChild.OwnerID == 404)
                                {

                                    markerChild.OwnerID = 19962001;

                                    markerChild.alpha = 1f;
                                    markerChild.SendUpdate();
                                   
                                }

                                alreadyHasMarker = true;
                                break;
                            }

 
                        }
                    }

                    if (alreadyHasMarker) //we no longer force respawn these, for perf reasons.
                        continue;

                    var newMap = (MapMarkerGenericRadius)GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", ore.transform.position);

                    _userMiningMapMarkers[userIDString].Add(newMap);

                    newMap.radius = 0.0375f;

                    var isSulfur = false;
                    var isMetal = false;

                    switch(ore.prefabID)
                    {
                        case 3058967796:
                        case 2204178116:
                        case 1227527004:
                            {
                                isSulfur = true;
                                break;
                            }
                        case 3774647716:
                        case 4225479497:
                        case 3345228353:
                            {
                                isMetal = true;
                                break;
                            }
                    }

                    //if not metal or sulfur, it'll use stone colors

                    newMap.OwnerID = 19962001;

                    var mainColor = new Color(isSulfur ? 0.733f : isMetal ? 0.376f : 0.788f, isSulfur ? 0.725f : isMetal ? 0.427f : 0.733f, isSulfur ? 0.419f : isMetal ? 0.568f : 0.713f);

                    newMap.color1 = mainColor;
                    newMap.color2 = new Color(0f, 0f, 0f, 0f); //outline color
                    newMap.alpha = 1f;

                    newMap.Spawn();

                    newMap.SetParent(ore, true);

                    newMap.SendUpdate();
                    newMap.SendNetworkUpdateImmediate(true);

                  

                    newMap.InvokeRepeating(() =>
                    {
                        if (ore == null || ore.IsDestroyed || player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected || Vector3.Distance(player?.transform?.position ?? Vector3.zero, ore?.transform?.position ?? Vector3.zero) > radius) 
                            newMap?.Kill();

                    }, Random.Range(3.5f, 5f), Random.Range(3.5f, 5f)); //perf?

                }

            }
            finally { Pool.FreeList(ref nearbyOres); }


        }


        private object OnGetRadiationProtection(BasePlayer player, float val)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return null;

            if (IsWearingItem(player, GAS_MASK_ITEM_ID)) //gas mask
                return 99999f;

            return null;
        }

        private IEnumerator BatteryRainCoroutine(Vector3 startPos, int batteryDrops, int batteriesPerDrop = 1, BasePlayer player = null)
        {
            if (batteryDrops < 1)
                throw new ArgumentOutOfRangeException(nameof(batteryDrops));

            if (batteriesPerDrop < 1) 
                throw new ArgumentOutOfRangeException(nameof(batteriesPerDrop));

            for (int i = 0; i < batteryDrops; i++)
            {
                var desiredPos = SpreadVector2(new Vector3(startPos.x, startPos.y + Random.Range(1.75f, 5f), startPos.z), Random.Range(0.85f, 1.25f));

                var batteryItem = ItemManager.CreateByItemID(BATTERY_ID, batteriesPerDrop);

                if (!batteryItem.Drop(desiredPos, Vector3.down * Random.Range(2.5f, 9f), Quaternion.identity))
                {
                    PrintError("failed to drop a battery during battery rain!!!");

                    RemoveFromWorld(batteryItem);

                    break;
                }

                if (player != null && player.IsConnected)
                    PlayCodeLockUpdate(player);

                yield return CoroutineEx.waitForSecondsRealtime(0.1f);

            }

         
        }

        //this code is a giant piece of shit largely because the cobweb prefab is actually terrible. darn.
        private IEnumerator CobWebWhirlingFX(BaseCombatEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.IsDestroyed || target.IsDead()) //temp target contains check
                yield break;

            PrintWarning(nameof(CobWebWhirlingFX));

            var webs = new HashSet<BaseEntity>();

            for (int i = 0; i < 4; i++)
            {
                var adjPos = i switch
                {
                    0 => Vector3.left * 1.25f,
                    1 => Vector3.forward * 1.25f,
                    2 => Vector3.back * 1.25f,
                    _ => Vector3.right * 1.25f,
                };

                var web = GameManager.server.CreateEntity("assets/prefabs/misc/halloween/spiderweb/spiderweba.prefab", target.transform.position + adjPos);

                web.Spawn();

                web.SetParent(target, true, true);

                webs.Add(web);

            }

            var startTime = DateTime.UtcNow;

            var timesRan = 0;

            while (true)
            {
                if ((DateTime.UtcNow - startTime).TotalSeconds > 8 || timesRan > 50)
                {
                    PrintWarning("break! times ran > 50 or total sec > 8");
                    break;
                }

               // var i = 0;

                var flipDir = IsDivisble(timesRan, 2);

                PrintWarning("flip dir: " + flipDir);

                foreach (var web in webs)
                {
                    if (web == null || web.IsDestroyed || web.gameObject == null)
                        continue;

                    PrintWarning("ADJUSTING WEB");

                    web.transform.eulerAngles = SpreadVector2(web.transform.eulerAngles, Random.Range(0.1f, 1.1f));
                    web.transform.localEulerAngles = SpreadVector2(web.transform.localEulerAngles, Random.Range(0.1f, 1.1f));

                    web.SendNetworkUpdateImmediate();

                    PrintWarning("ADJUSTED");

                    yield return CoroutineEx.waitForSecondsRealtime(0.01f);
                }

                timesRan++;

            }

            yield return null;
        }

        private IEnumerator StingingVenomFX(BaseCombatEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.IsDestroyed || target.IsDead() || _stingingVenomTargets.Contains(target)) //temp target contains check
                yield break;

            StartCoroutine(CobWebWhirlingFX(target));

            DoorManipulator invisEntity = null;

            for (int i = 0; i < target.children.Count; i++)
            {
                var doorChild = target.children[i] as DoorManipulator;
                if (doorChild != null)
                {
                    invisEntity = doorChild;
                    break;
                }
            }

            if (invisEntity == null)
            {
                invisEntity = GameManager.server.CreateEntity("assets/prefabs/io/electric/switches/doormanipulator.invisible.prefab", target.transform.position) as DoorManipulator;
                invisEntity.Spawn();
                invisEntity.SetParent(target, true, true);
            }

            invisEntity.EnableSaving(false);

          

           

            var startTime = Time.realtimeSinceStartup;

            var isHuman = target is BasePlayer;

            while (target != null && !target.IsDestroyed && !target.IsDead() && invisEntity != null && !invisEntity.IsDestroyed && Time.realtimeSinceStartup - startTime < 10)
            {
                Effect.server.Run("assets/bundled/prefabs/fx/dig_effect.prefab", invisEntity, StringPool.Get("head"), Vector3.zero, Vector3.zero);

                if (Random.Range(0, 101) <= 60)
                    Effect.server.Run("assets/bundled/prefabs/fx/player/beartrap_blood.prefab", target, 0, Vector3.zero, Vector3.zero);

                yield return CoroutineEx.waitForSeconds(Random.Range(0.33f, 0.5f));

                if (isHuman)
                {
                    Effect.server.Run("assets/bundled/prefabs/fx/gestures/drink_vomit.prefab", invisEntity, 0, Vector3.zero, Vector3.zero);

                    yield return CoroutineEx.waitForSecondsRealtime(0.1f);
                }
            }

            if (invisEntity != null && !invisEntity.IsDestroyed)
                invisEntity.Kill();

        }

        //temp:
        private readonly HashSet<BaseCombatEntity> _stingingVenomTargets = new();

        //grant damage bonus if using rustic/low tier weapons?
        private IEnumerator StingingVenomDamage(BaseCombatEntity target, BasePlayer attacker = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.IsDestroyed || target.IsDead() || !_stingingVenomTargets.Add(target))
                yield break;

            try
            {
              

                var startTime = Time.realtimeSinceStartup;

                while (target != null && !target.IsDestroyed && !target.IsDead())
                {
                    if (Time.realtimeSinceStartup - startTime > 10)
                    {
                        PrintWarning("> 10 - break!");
                        break;
                    }




                    //damage based on % of health remaining?

                    var dmgToDeal = Mathf.Max(target.Health() * 0.05f, 2f);

                    target.Hurt(dmgToDeal, DamageType.Poison, attacker);

                    var humanNpc = target as HumanNPC;

                    if (humanNpc != null)
                    {
                        var switched = humanNpc.Brain.SwitchToState(AIState.Flee, 0); //try to run from the locust/fly/dirt/sand swarm!

                        if (!switched)
                            PrintWarning("failed to switch humanNpc to flee?");

                        //i've yet to determine diff between .add and .increase
                        humanNpc.metabolism.bleeding.Add(1f); //does bleeding do anything with NPC players? I don't know yet.
                    }



                    yield return CoroutineEx.waitForSecondsRealtime(Random.Range(0.15f, 0.2f));

                }
            }
            finally { _stingingVenomTargets.Remove(target); }

           

            

        }

        private void OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return;

            var bce = info?.HitEntity as BaseCombatEntity;

            var bceMaxHealth = bce?.MaxHealth() ?? 0f;

            if (IsValidPlayer(player) && bce != player && IsWearingItem(player, DRACULA_CAPE_ID))
                player?.Invoke(() =>
                {
                    if (player == null || player.gameObject == null || player.IsDestroyed || player.IsDead())
                        return;

                    var dmgTaken = info?.damageTypes?.Total() ?? 0f;

                    if (dmgTaken <= 0f)
                        return;

                    var healAmt = Mathf.Min(bceMaxHealth, dmgTaken) * 0.025f;
                    if (healAmt <= 0f)
                        return;

                    player.Heal(healAmt);

                    if (healAmt >= 0.75f)
                    {
                        var sb = Pool.Get<StringBuilder>();
                        try
                        {
                            ShowToast(player, sb.Clear().Append("<color=red>Dracula</color>: ").Append("+").Append(healAmt.ToString("0.00")).Replace(".00", string.Empty).Append(" HP").ToString());
                        }
                        finally { Pool.Free(ref sb); }
                    }

                }, 0.01f);

            if ((info?.Weapon?.GetCachedItem() ?? info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem())?.info?.itemid == ABYSS_ASSAULT_RIFLE_ITEM_ID)
            {
                if ((info?.HitEntity is BasePlayer victim) && IsValidPlayer(victim) && victim?.metabolism != null)
                    victim.metabolism.hydration.Subtract(15f);
                else if (info?.HitEntity is HumanNPC || info?.HitEntity is BaseNpc || info?.HitEntity is BaseAnimalNPC) //bradley was being counted, possibly other things we don't want to be counted lol
                    bce?.Hurt(1.5f, DamageType.Thirst, player, true);
                
                

                var players = Pool.GetList<BasePlayer>();

                try 
                {
                    Vis.Entities(info.HitPositionWorld, 2.65f, players, PLAYER_LAYER_MASK);

                    for (int i = 0; i < players.Count; i++)
                    {
                        var p = players[i];

                        if (!IsValidPlayer(p) || p?.metabolism == null)
                            continue;

                        p.metabolism.wetness.Add(0.025f); //0.025f here is 2.5% in-game. remember, this is a radius and per shot. don't let it be too OP.                           
                    }

                }
                finally { Pool.FreeList(ref players); }

                Effect.server.Run("assets/content/vehicles/boats/effects/splash.prefab", info.HitPositionWorld);

            }

            if (_overDrivePlayers.Contains(player.userID))
            {
                if (_overdriveHitInfo.TryGetValue(player.UserIDString, out HitInfo odHit) && odHit.HitEntity != null && odHit.HitEntity == info.HitEntity)
                {
                    PrintWarning("found od hit! return");
                    CancelDamage(info);
                    return;
                }

                if ((info?.damageTypes?.GetMajorityDamageType() == DamageType.Explosion) && (player?.GetHeldEntity() is BaseProjectile) && (info?.HitEntity is DecayEntity))
                {
                    CancelDamage(info);
                    return;
                }
            }
           

            //temp
            if (IsPlayerShady(player) && GetUserClassType(player.UserIDString) == ClassType.Nomad)
            {

                //we use an invoke to ensure the damage itself did not kill the target, so no need to run fx if target was killed
                bce?.Invoke(() =>
                {
                    if (bce == null || player == null || bce.IsDestroyed || player.IsDestroyed || bce.IsDead() || player.IsDead()) return;

                    SendReply(player, "Stinging Venom!");
                    StartCoroutine(StingingVenomFX(bce));
                    StartCoroutine(StingingVenomDamage(bce, player));
                }, 0.01f);


            }

            var hitBce = info?.HitEntity as BaseCombatEntity;
            var projectileId = info?.ProjectileID ?? 0;

            if (_blunderBussProjectileIDs.Remove(projectileId))
            {
                PrintWarning(nameof(_blunderBussProjectileIDs) + " removed: " + projectileId);

                info?.damageTypes?.ScaleAll(1.25f);

                var adjPos = info?.HitPositionWorld ?? Vector3.zero;
                adjPos.y += Random.Range(2.75f, 3.15f);

                Effect.server.Run("assets/bundled/prefabs/fx/bucket_drop_debris.prefab", SpreadVector2(adjPos, Random.Range(0.05f, 0.125f)), Vector3.zero);

                var oreBreakFxPos = hitBce?.transform?.position ?? Vector3.zero;
                if (oreBreakFxPos != Vector3.zero)
                    Effect.server.Run("assets/bundled/prefabs/fx/ore_break.prefab", SpreadVector2(oreBreakFxPos, Random.Range(0.1f, 0.33f)), Vector3.zero);


                player?.Invoke(() =>
                {
                    var totalDmg = info?.damageTypes?.Total() ?? 0f;

               

                    var damages = Pool.GetList<DamageTypeEntry>();

                    try
                    {

                        _blunderbussBluntDmg.amount = (totalDmg > 0 ? totalDmg : Random.Range(7, 22)) * Random.Range(0.72f, 1f);
                        damages.Add(_blunderbussBluntDmg);

                        DamageUtil.RadiusDamage(player, info?.WeaponPrefab, info.HitPositionWorld, 1f, 2f, damages, 1084427009, true);

                    }
                    finally { Pool.FreeList(ref damages); }

                 
                }, 0.025f);

               

            }


            if (_wolfHoundProjectileIDs.Remove(projectileId)) //absolutely based code to keep the hashset clear
            {
         

                if (hitBce != null && !hitBce.IsDestroyed && hitBce.IsAlive()) hitBce.Hurt(Random.Range(0.5f, 2.5f), DamageType.Heat, player);

                var hitPoint = info?.HitPositionWorld ?? hitBce?.transform?.position ?? Vector3.zero;

                for (int i = 0; i < Random.Range(2, 6); i++)
                {

                    var fireballNew = (FireBall)GameManager.server.CreateEntity("assets/bundled/prefabs/napalm.prefab", hitPoint);
                    fireballNew.OwnerID = player?.userID ?? 1337;
                    fireballNew.creatorEntity = info?.Weapon ?? info?.WeaponPrefab;
                    fireballNew.damagePerSecond = 2;
                    fireballNew.lifeTimeMax = 8;
                    fireballNew.lifeTimeMin = 4;
                    fireballNew.generation = 1;
                    fireballNew.tickRate = 0.25f;
                    fireballNew.radius = 1f;
                    fireballNew.waterToExtinguish = 100;
                    fireballNew.AttackLayers = 1084427009; //flamer layers
                    fireballNew.Spawn();
                }
               
            }

            var luckInfo = GetLuckInfo(player);
            if (luckInfo == null) return;


            var weaponItem = info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem();



          

            if (info?.HitEntity != null && weaponItem != null && bce != null && !bce.IsDestroyed && !bce.IsDead())
            {

                if (IsLightweightGun(weaponItem))
                {
                    var lightGunLvl = luckInfo.GetStatLevelByName("SwiftGunner");
                    if (lightGunLvl > 0)
                        info.damageTypes.ScaleAll(1.18f + ((lightGunLvl - 1) * 0.18f)); //1.18x + 18%/0.18x per level after that
                    
                }

            
                if (weaponItem.skin == LUCKY_GREASE_GUN_SKIN_ID && Random.Range(0, 101) < 3 && (info?.HitEntity as BaseCorpse == null && info?.HitEntity as PlayerCorpse == null))
                {
                    var worldHitPos = info?.HitPositionWorld ?? Vector3.zero;


                    player.Invoke(() =>
                    {
                        if (player == null || (info?.damageTypes?.Total() ?? 0f) <= 0)
                            return;

                        StartCoroutine(BatteryRainCoroutine(worldHitPos, Random.Range(8, 33), 1, player));

                        //needs SB
                        SendLuckNotifications(player, "<color=" + LUCK_COLOR + ">Battery Rain!</color>");

                    }, 0.01f);
                    

                }

                var hitNpc = info?.HitEntity as HumanNPC;
                if (hitNpc != null && !IsInAttackState(hitNpc) && weaponItem?.contents != null)
                {
                    var foeLvl = luckInfo?.GetStatLevelByName("UnsuspectingFoe") ?? 0;

                    if (foeLvl > 0 && FindItemInContainer(weaponItem?.contents, SILENCER_ITEM_ID) != null)
                    {
                        var dmgScalar = 2 + ((foeLvl - 1) * 0.5f); //2x + 0.5 per level after that
                        info.damageTypes.ScaleAll(dmgScalar);

                        //needs sb
                        SendLuckNotifications(player, "<color=" + LUCK_COLOR + ">Lucky!</color> Unsuspecting Foe for " + dmgScalar.ToString("0.00").Replace(".00", string.Empty) + "x damage!");

                    }
                }

                if (weaponItem.skin == WINDFORCE_BOW_SKIN_ID)
                {
                    bce.Invoke(() =>
                    {
                        if (bce == null || bce.IsDestroyed || bce.IsDead()) return;

                        var totalDmg = info?.damageTypes?.Total() ?? 0f;

                        if (totalDmg <= 0f) return;

                        var c = 1;
                        for (int i = 0; i < 3; i++)
                        {
                            if (bce == null || bce.IsDestroyed || bce.IsDead()) break;

                            bce.Hurt(totalDmg * (1 - (c * 0.33f)), DamageType.Arrow, player);
                            c++;
                        }

                    }, 0.05f);
                }
                else if (weaponItem.skin == GAMBLERS_PYTHON_SKIN_ID)
                {
                    if (Random.Range(0, 101) <= 50)
                    {
                        var dmgVal = Random.Range(0.5f, 2.5f);

                        var fx = dmgVal >= 2f ? "assets/prefabs/misc/casino/slotmachine/effects/payout_jackpot.prefab" : dmgVal > 1f ? "assets/prefabs/misc/casino/slotmachine/effects/payout.prefab" : "assets/prefabs/misc/easter/easter basket/effects/eggexplosion.prefab";

                        Effect.server.Run(fx, info?.HitEntity?.transform?.position ?? Vector3.zero, Vector3.zero);

                        info.damageTypes.ScaleAll(dmgVal);
                    }
                }
                else if (weaponItem.skin == FANG_SAP_SKIN_ID)
                {

                    var twoFang = luckInfo?.GetStatLevelByName("TwoFang") ?? 0;
                    if (twoFang > 0)
                    {

                        var rngNeed = 6 * twoFang;

                        if (Random.Range(0, 101) <= rngNeed)
                        {
                            var scalar = 1f - (0.5f - (0.04f * twoFang));

                            bce.Hurt(info.damageTypes.Total() * scalar, DamageType.Bullet, player);

                            var sb = Pool.Get<StringBuilder>();
                            try
                            {
                                SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> <color=red>Two Fang</color> for <color=").Append(LUCK_COLOR).Append(">").Append(scalar.ToString("0.00").Replace(".00", string.Empty)).Append("</color>x damage!</color>").ToString());
                            }
                            finally { Pool.Free(ref sb); }
                        }

                    }

                }
            }

            var wotw = (info?.Weapon is BaseMelee || info?.WeaponPrefab is BaseMelee) ? luckInfo.GetStatLevelByName("WeightOfTheWorld") : 0;

            //not null. we want a ground hit for wotw checks. not many things should be a non-entity hit, so this should be fine.
            if (info.HitEntity != null || wotw < 1) return;

            //we need to check to make sure the player is actually using a pickaxe/jackhammer/ice pick lol (done, check below)
            //also they may not be hitting teh ground but something at a rad town, so maybe some checks for stone/dirt only

            if (weaponItem == null)
            {
                PrintWarning("wotw is checking for a weapon item and somehow got a null waepon item on OnPlayerAttack with a null hitentity lol");
                return;
            }

            var itemId = weaponItem.info.itemid;
            if (!IsPickaxeItemID(itemId))
                return;

            if (HasWOTWActive(player))
            {
                PrintWarning("wotw found active coroutine! return");
                return;
            }

            var hasAbyss = itemId == ABYSS_PICK_ID;

            //abyss pickaxe allows chaining two wotw together
            if (!hasAbyss && _lastWotwHitPos.TryGetValue(player.UserIDString, out Vector3 lastHitPos) && Vector3.Distance(lastHitPos, info.HitPositionWorld) > 1.25f)
                _consecutiveGroundHits[player.UserIDString] = 0;


            _lastWotwHitPos[player.UserIDString] = info.HitPositionWorld;

            if (!_consecutiveGroundHits.TryGetValue(player.UserIDString, out int hits)) _consecutiveGroundHits[player.UserIDString] = 0;



            var eyesPos = player?.eyes?.transform?.position ?? Vector3.zero;

            var radius = 50f + (8.33f * (wotw - 1));

            var ores = Pool.GetList<OreResourceEntity>();

            List<OreResourceEntity> playerOres = null;

            var needsFreed = true;

            try
            {
                Vis.Entities(eyesPos, radius, ores, 1, QueryTriggerInteraction.Ignore);

                if (hasAbyss)
                {


                    if (!_wotwPlayerOres.TryGetValue(player.UserIDString, out playerOres))
                        _wotwPlayerOres[player.UserIDString] = new List<OreResourceEntity>();
                    

                    playerOres = _wotwPlayerOres[player.UserIDString];

                    for (int i = 0; i < ores.Count; i++)
                    {
                        var ore = ores[i];
                        if (!playerOres.Contains(ore))
                            playerOres.Add(ore);
                    }
                }

                List<OreResourceEntity> oresToUse;

                var shouldFree = true;
                if (playerOres != null && playerOres.Count > 0)
                {
                    oresToUse = playerOres;
                    needsFreed = false;
                    shouldFree = false;
                }
                else oresToUse = ores; //pooling related

                var oreCount = oresToUse?.Count ?? 0;

                //this skill has an effect where every 5 ores, you get an extra 2 added out of thin air

                var extraOresToSpawn = GetNumberOfDivisions(oreCount, 5);

                extraOresToSpawn *= 2; //2 for every 5

                if (hits > 0 && extraOresToSpawn > 0) //needs to be second hit
                {

                    for (int i = 0; i < extraOresToSpawn; i++)
                    {
                        var rngPrefab = oresToUse[Random.Range(0, oresToUse.Count)]?.PrefabName ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(rngPrefab))
                        {
                            PrintWarning("rngPrefab is null!!!!");
                            continue;
                        }

                        var newOre = GameManager.server.CreateEntity(rngPrefab, Vector3.zero) as OreResourceEntity;

                        newOre.Spawn();

                        newOre?.Invoke(() =>
                        {
                            if (newOre != null && !newOre.IsDestroyed)
                                newOre?.Kill();
                        }, 120f);

                        oresToUse.Add(newOre);

                    }
                }



                oreCount = oresToUse?.Count ?? 0;

                var displayedOreCount = (oresToUse?.Count ?? 0) + (hits < 1 ? extraOresToSpawn : 0);

                //look, all of this is a bit messy. you could have something in the coroutine method that is like "extra ores to spawn", and spawn them there, and just use this for display. this would help spawning ores when a player may not evne go through with wotw and cause a proc.

                if (hits < 2)
                {
                    _consecutiveGroundHits[player.UserIDString] = hits += 1;

                    var size = hits == 1 ? "16" : "20";

                    //needs sb

                    var sb = Pool.Get<StringBuilder>();

                    try 
                    {
                        SendLuckNotifications(player, sb.Clear().Append("<color=#9E4122><size=").Append(size).Append(">Keep hitting the ground").Append((playerOres != null ? string.Empty : " nearby")).Append(" to channel <color=#C68826>Weight of the World</color> and summon <color=#C68826><i>").Append(displayedOreCount.ToString("N0")).Append("</i></color> ores to you!</size>").ToString());
                    }
                    finally { Pool.Free(ref sb); }

                    return;
                }

                _consecutiveGroundHits[player.UserIDString] = 0;

                //ensure using pickaxe/mining weapon

                var wotwCooldown = player?.GetComponent<WeightOfTheWorldCooldownHandler>() ?? player?.gameObject?.AddComponent<WeightOfTheWorldCooldownHandler>();

                if (!wotwCooldown.IsOnCooldown || IsWearingSkin(player, AESIRS_JACKET_SKIN_ID))
                {

               
                    


                    ActiveWeightOfTheWorldCoroutine[player] = StartCoroutine(WeightOfTheWorldCoroutine(player, oresToUse, shouldFree));
                    needsFreed = false;
                }
                else
                {
                    var sb = Pool.Get<StringBuilder>();

                    try { SendLuckNotifications(player, sb.Clear().Append("<color=#9E4122>You must wait an additional <color=#C68826>").Append(wotwCooldown.CooldownRemaining.ToString("N0")).Append("</color> seconds before using <color=#C68826>Weight of the World</color> again</color><color=#C68826>.</color>").ToString()); }
                    finally { Pool.Free(ref sb); }

                }

                _wotwPlayerOres.Remove(player.UserIDString);

            }
            finally { if (needsFreed) Pool.FreeList(ref ores); }

        }

        //temp location:

        private readonly HashSet<BaseCombatEntity> _cannotTargetPlayers = new();

        private IEnumerator GetOverHere(BasePlayer player, BaseMelee weapon)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));


            PrintWarning(nameof(GetOverHere));

            var target = GetOverdriveTarget(player, 90f, 3f, PLAYER_LAYER_MASK) ?? GetOverdriveTarget(player, 90f, 3f, 2048);

            if (target == null)
                yield break;

            var oldPos = target?.transform?.position ?? Vector3.zero;

            if (oldPos == Vector3.zero)
            {
                PrintError(nameof(oldPos) + " is vector3 zero!");
                yield break;
            }

            SendReply(player, "<size=20><i><color=red>GET OVER HERE!</color></i></size>");

            try
            {
                _cannotTargetPlayers.Add(target);

                var animalNpc = target as BaseAnimalNPC;

                if (animalNpc != null)
                {
                    PrintWarning("pausing animal npc");
                    animalNpc.brain.SwitchToState(AIState.Idle, 0);
                    animalNpc.StopMoving();
                    animalNpc.Pause();
                    animalNpc.brain?.Navigator?.Pause();

                    PrintWarning("paused animal?");
                }

                var humanNpc = target as HumanNPC;
                if (humanNpc != null)
                {
                    PrintWarning("pause human npc");

                    humanNpc.Brain.SwitchToState(AIState.Idle, 0);
                    humanNpc.Brain.Navigator.Pause();

                    PrintWarning("paused human?");
                }

                var distScalar = 8f;


                //messy temp code for decently smooth forced movement
                while (Vector3.Distance(target.transform.position, player.transform.position) > 2.5f)
                {
                    var desiredPos = player.eyes.transform.position + player.eyes.BodyForward() * distScalar;

                    desiredPos = new Vector3(desiredPos.x, player.transform.position.y, desiredPos.z);

                    target.transform.position = desiredPos;

                    target.SendNetworkUpdateImmediate();

                    distScalar -= 0.25f;

                    yield return CoroutineEx.waitForSecondsRealtime(0.05f);
                }

                //todo: smoothly drag enemy toward player. use SmoothMovement class from Compilation(?) or create something similar - update: the above is actually decently smooth, for what it is.
                //todo: freeze target. prevent from moving with ore attached to head.
                //todo: force AI to look at player
                //todo: force player to look at AI (as close to AI's eyes pos as possible). use RPC force view angles for non-NPC players; setaimdest for other should work fine.


                yield return CoroutineEx.waitForSecondsRealtime(0.33f);

                //spawn explosive ore:
                //use grenade explosive fx. metal explosion. gruesome.

                var oreSpawnPos = (target as BasePlayer)?.eyes?.position ?? target?.CenterPoint() ?? Vector3.zero;

                var ore = GameManager.server.CreateEntity("assets/bundled/prefabs/autospawn/resource/ores/metal-ore.prefab", oreSpawnPos);
                ore.Spawn();

                PrintWarning("ore spawned");

                //safety invoke
                ore.Invoke(() =>
                {
                    if (ore != null && !ore.IsDestroyed)
                        ore.Kill();
                }, 10f);

                ore.SetParent(target, true, true);

                PrintWarning("waiting 1.25f");

                yield return CoroutineEx.waitForSeconds(1.25f);


                PrintWarning("waited 1.25f");

                //explode ore. deal damage.
                //damage ore until it is gone, so that the ore goes through stages? would be added effects.
                //could also have staged explosions.
                //run bleeding effect as soon as ore is attached?

                for (int i = 0; i < 5; i++)
                {
                    Effect.server.Run("assets/prefabs/weapons/f1 grenade/effects/f1grenade_explosion.prefab", SpreadVector2(oreSpawnPos, Random.Range(0.25f, 0.33f)));
                }




                target.Hurt(200f, DamageType.Explosion, player, false);

                PrintWarning("dealt 200f damage");

                if (ore != null && !ore.IsDestroyed) ore.Kill();

                yield return CoroutineEx.waitForSecondsRealtime(0.1f);

                if (!(target?.IsDead() ?? true))
                {
                    PrintWarning("target not dead. force TP back to old pos!");

                    target.transform.position = oldPos;
                    target.SendNetworkUpdateImmediate();

                    PrintWarning("TP'd, time to re-stun!");

                    //move this code somewhere, same with the one above (this is copy & paste):

                    if (animalNpc != null)
                    {
                        PrintWarning("repausing animal npc");
                        animalNpc.brain.SwitchToState(AIState.Idle, 0);
                        animalNpc.StopMoving();
                        animalNpc.Pause();
                        animalNpc.brain?.Navigator?.Pause();
                      

                        PrintWarning("repaused animal?");
                    }

                    if (humanNpc != null)
                    {
                        PrintWarning("repause human npc");

                       
                        humanNpc.Brain.SwitchToState(AIState.Idle, 0);
                        humanNpc.Brain.Navigator.Pause();

                        PrintWarning("repaused human?");
                    }

                    yield return CoroutineEx.waitForSecondsRealtime(8f); //really only affects the finally below (at this time)
                }
              
            }
            finally { _cannotTargetPlayers.Remove(target); }
        }

        private bool HasFusionSkill(BasePlayer player, string skillName)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(skillName))
                throw new ArgumentNullException(nameof(skillName));

            var findStat = FindStatInfoByName(skillName);

            return findStat != null && findStat.MustMatchClass && GetUserClassType(player.UserIDString) != findStat.AppropriateClass && (GetLuckInfo(player)?.GetStatLevelByName("Fusion") ?? 0) > 0;
        }

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            var watch = Pool.Get<Stopwatch>();

            try 
            {
                watch.Restart();

                if (player == null || input == null) return;
                if (player.IdleTime >= 10f && !player.IsAlive() || !player.IsConnected || player.IsSleeping() || player.IsWounded()) return;

                if (player.IsAdmin && input.WasJustPressed(BUTTON.FIRE_PRIMARY) && player.CanAttack() && IsPickaxeItem(GetActiveItem(player)))
                    StartCoroutine(GetOverHere(player, player?.GetHeldEntity() as BaseMelee));
                

                if (input.WasJustPressed(BUTTON.JUMP))
                {
                    if (StopSKTGUI(player))
                        SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");
                }

                if (player?.inventory != null && player.inventory.containerMain.capacity <= 24)
                {
                    if (!lastCapCheck.TryGetValue(player.UserIDString, out float lastCap) || (Time.realtimeSinceStartup - lastCap) >= 3f)
                    {
                        var largeCharms = 0;
                        var grandCharms = 0;
                        var mainItems = player?.inventory?.containerMain?.itemList ?? null;
                        if (mainItems != null && mainItems.Count > 0)
                        {
                            for (int i = 0; i < mainItems.Count; i++)
                            {
                                var item = mainItems[i];

                                var charmType = charmsData?.GetCharm(item)?.Type ?? CharmType.Undefined;

                                if (charmType == CharmType.Large) largeCharms++;
                                else if (charmType == CharmType.Grand) grandCharms++;
                            }
                        }



                        var desiredCapacity = 24 - ((largeCharms * 3) + (grandCharms * 4));

                        if (player.inventory.containerMain.capacity != desiredCapacity)
                        {
                            PrintWarning("cur cap != capacity, " + player.inventory.containerMain.capacity + " != " + desiredCapacity);
                            player.inventory.containerMain.capacity = Mathf.Clamp(desiredCapacity, 0, 24);
                            player.inventory.ServerUpdate(0.01f);
                        }
                        lastCapCheck[player.UserIDString] = Time.realtimeSinceStartup;
                    }
                }

                if (IsDownOrJustPressedAnyMovementKey(input) && (!_lastMiningMarkerTime.TryGetValue(player.UserIDString, out float lastMarkerTime) || Time.realtimeSinceStartup - lastMarkerTime >= 5) && GetUserClassType(player.UserIDString) == ClassType.Miner)
                {
                    _lastMiningMarkerTime[player.UserIDString] = Time.realtimeSinceStartup;

                    var activeItemId = GetActiveItem(player)?.info?.itemid ?? 0;
                    if (IsPickaxeItemID(activeItemId))
                    {
                        var miningLvl = ZLevelsRemastered?.Call<int>("GetLevel", player.UserIDString, "M") ?? -1;
                        if (miningLvl >= 25)
                        {
                            var radius = 40f + (miningLvl - 25);

                            var hasMiningHat = false;

                            var wearItems = player?.inventory?.containerWear?.itemList;

                            for (int i = 0; i < wearItems.Count; i++)
                            {
                                var wornItem = wearItems[i];
                                if (wornItem?.info?.itemid == MINING_HAT_ID && (!IsNight() || wornItem.IsOn()))
                                {
                                    hasMiningHat = true;
                                    break;
                                }
                            }

                            if (hasMiningHat) radius *= 1.5f;

                            AttachMarkerToAllNearbyOres(player, radius);
                            _lastMiningMarkerTime[player.UserIDString] = Time.realtimeSinceStartup;
                        }

                    }
                }

                if ((input.WasJustPressed(BUTTON.FIRE_THIRD) || input.WasJustReleased(BUTTON.FIRE_THIRD)) && input.IsDown(BUTTON.FIRE_SECONDARY) && player?.GetHeldEntity() is BaseProjectile)
                {
                    var overDrive = GetLuckInfo(player)?.GetStatLevelByName("Overdrive") ?? 0;

                    if (overDrive > 0)
                    {

                        var odCooldownHandler = player?.GetComponent<OverdriveCooldownHandler>() ?? player?.gameObject?.AddComponent<OverdriveCooldownHandler>();

                        if (!odCooldownHandler.IsOnCooldown)
                        {
                            var hookVal = Interface.Oxide.CallHook("CanUseOverdrive", player);

                            if ((hookVal == null || (bool)hookVal) && _overDrivePlayers.Add(player.userID))
                            {
                                Compilation?.Call("RenderGlueUI", player, "1 0.075 0.05 0.4");

                                Effect.server.Run("assets/bundled/prefabs/fx/player/beartrap_scream.prefab", player, 0, Vector3.zero, Vector3.zero);

                                var odMsg = "<i><size=20><color=#F5393A>Overdrive</color><color=#89FE1A>!</color></i>";
                                ShowToast(player, odMsg, 1);
                                SendReply(player, odMsg);

                                var userId = player.userID;

                                var lengthOfOverDrive = 20f + (overDrive * 1.5f);

                                player.Invoke(() =>
                                {
                                    _overDrivePlayers.Remove(userId);

                                    CuiHelper.DestroyUi(player, "GlueUI");
                                    CuiHelper.DestroyUi(player, "GlueUI2");

                                    odCooldownHandler.StartCooldown();
                                }, 20f);
                            }
                        }
                    }
                }


                //anglers boon legendary - set strain timer to 0f
                if ((input.IsDown(BUTTON.FIRE_PRIMARY) || input.IsDown(BUTTON.BACKWARD)) && _playerToRodCache.TryGetValue(player, out BaseFishingRod rod) && rod != null && IsWearingSkin(player, ANGLERS_BOON_SKIN_ID))
                    _strainTimer.SetValue(rod, 0f);



                if (input.WasJustReleased(BUTTON.FIRE_SECONDARY))
                {
                    var botanist = GetLuckInfo(player)?.GetStatLevelByName("Botanist") ?? 0;

                    if (botanist > 0)
                    {
                        var activeItem = GetActiveItem(player);



                        if ((activeItem?.info?.itemid ?? 0) == BONE_FRAGMENTS_ITEM_ID)
                        {
 
                            //consider me puzzled. layers.mask.player_model_rendering doesn't exist according to compiler
                            var growable = GetLookAtEntity(player, 2.2f, 0.33f, 524288) as GrowableEntity;

                            if (growable != null && ((int)growable.State < 6))
                            {

                                var neededAmount = Mathf.Clamp((int)Math.Round(0.1 * growable.currentStage.lifeLengthSeconds, MidpointRounding.AwayFromZero), 75, 10000);

                                var setPieces = OverallsSetPieces(player);

                                if (setPieces >= 2) neededAmount = (int)(neededAmount * 0.75);

                                if (activeItem.amount < neededAmount)
                                {
                                    SendLuckNotifications(player, "You don't have enough bone fragments to mature this plant!\nYou need: " + neededAmount.ToString("N0"));
                                    return;
                                }

                                NoteItemByDef(player, activeItem.info, -neededAmount);

                                if (activeItem.amount <= neededAmount) RemoveFromWorld(activeItem);
                                else
                                {
                                    activeItem.amount -= neededAmount;
                                    activeItem.MarkDirty();
                                }


                                var netId = growable.net.ID;

                                if (_bonemealedPlants.Add(netId))
                                {
                                    try
                                    {
                                        growable.RunUpdate(); //we call RunUpdate, which will naturally check if the plant should be aged. Because we handle this in a hook (hence the add above), it will instantly age it to the next stage. This provides a 'natural' age, granting the right yield amount
                                    }
                                    finally
                                    {
                                        _bonemealedPlants.Remove(netId);
                                        growable.RunUpdate();
                                    }
                                }
                                else PrintWarning("fail to add bonemealed plants");


                                var growPos = growable?.transform?.position ?? Vector3.zero;

                                Effect.server.Run("assets/prefabs/food/bota bag/effects/bota-bag-slosh.prefab", growPos);
                                Effect.server.Run("assets/prefabs/misc/halloween/pumpkin_bucket/effects/add_egg.prefab", growPos);
                                Effect.server.Run("assets/bundled/prefabs/fx/collect/collect mushroom.prefab", growPos);

                            }
                        }
                    }
                }

                if (input.IsDown(BUTTON.FIRE_THIRD) && player.IsRunning()) //add impact info perf so we don't create a new hitinfo each time (bottom of whirlwind code below)
                {
                    var whirlwind = GetLuckInfo(player)?.GetStatLevelByName("Whirlwind") ?? 0;
                    if (whirlwind < 1) return;

                    var heldItem = GetActiveItem(player);
                    if (heldItem == null) return;


                    var isChainsaw = heldItem.info.itemid == CHAINSAW_ID;

                    if (!IsHatchetItem(heldItem))
                        return;

                    var isFusion = HasFusionSkill(player, "Whirlwind");

                    var isAmber = heldItem.skin == AMBER_HATCHET_SKIN_ID;

                    var heldEnt = heldItem?.GetHeldEntity() as HeldEntity;

                    var weapon = heldEnt as AttackEntity;

                    var adjustedWwRepeatDelay = (weapon?.repeatDelay ?? 0.166f) * (isChainsaw ? 0.5f : 0.38f);




                    var netCon = player?.net?.connection ?? null;

                    if (!lastWhirlwindAttack.TryGetValue(player.UserIDString, out float lastWhirl) || (Time.realtimeSinceStartup - lastWhirl) >= adjustedWwRepeatDelay)
                    {
                        var wwCooldownHandler = player?.GetComponent<WhirlwindCooldownHandler>();

                        if (!whirlwindPlayers.Contains(player.UserIDString) && (wwCooldownHandler?.IsOnCooldown ?? false))
                            return;


                        if (_activeWhirlwind.Add(player.UserIDString) && whirlwindPlayers.Add(player.UserIDString))
                            OnWhirlwindStart(player);


                        if (!isChainsaw)
                        {
                            if (Random.Range(0, 101) <= 10) //net-performance heavy for clients
                            {
                                player.UpdateActiveItem(new ItemId(0)); //continually makes player re-equip weapon, adds to the overall effect, but without a net update (below) it won't do anything animation wise

                                player.SendEntityUpdate();
                            }
                          
                        }
                        else
                        {
                            var chainsaw = !isChainsaw ? null : weapon as Chainsaw;
                            if (chainsaw != null)
                            {
                                if (!chainsaw.EngineOn() || !input.IsDown(BUTTON.FIRE_PRIMARY)) return;
                                chainsaw.ReduceAmmo(chainsaw.fuelPerSec * 0.85f);
                            }
                        }




                        var fxTimes = Random.Range(2, 4);

                        var eyesPos = player?.eyes?.transform?.position ?? Vector3.zero;

                        var forwardPos = eyesPos + ((player?.eyes?.BodyForward() ?? Vector3.zero) * 1.8f);

                        forwardPos.y = player?.eyes?.position.y ?? (forwardPos.y += 0.33f);

                        if (Random.Range(0, 101) <= 92) //all rng for effects is just for client-side perf
                        {
                            for (int i = 0; i < Random.Range(3, 8); i++)
                            {
                                var goodPos = SpreadVector(forwardPos, 0.33f);


                                Effect.server.Run("assets/bundled/prefabs/fx/player/frosty_breath.prefab", goodPos, Vector3.zero);
                            }
                        }
                        

                        if (Random.Range(0, 101) <= 60) Effect.server.Run(SHOCK_EFFECT, SpreadVector(forwardPos, 0.67f));


                        var sb = Pool.Get<StringBuilder>();
                        try
                        {
                            for (int i = 0; i < fxTimes; i++)
                            {
                                var tightCenter = SpreadVector(forwardPos, Random.Range(0.2f, 0.6f));

                                if (Random.Range(0, 101) <= 35) Effect.server.Run("assets/prefabs/misc/orebonus/effects/hotspot_death.prefab", tightCenter, Vector3.zero);
                                else Effect.server.Run("assets/prefabs/misc/orebonus/effects/bonus_hit.prefab", tightCenter, Vector3.zero);

                                var rngEffect = Random.Range(1, 4);
                                var swordFX = sb.Clear().Append("assets/prefabs/weapons/sword/effects/attack-").Append(rngEffect).Append(".prefab").ToString();


                                using var swordEffect = new Effect(swordFX, player, 0, eyesPos, Vector3.zero);
                                EffectNetwork.Send(swordEffect, netCon);

                            }
                        }
                        finally { Pool.Free(ref sb); }


                        if (isAmber && Random.Range(0, 101) <= 20) 
                            Effect.server.Run("assets/prefabs/ammo/arrow/fire/fireexplosion.prefab", SpreadVector(player.transform.position, Random.Range(0.4f, 1.5f)), Vector3.zero);

                        _whirlwindSlashDmg.amount = Random.Range(7f, 14f);
                        _whirlwindStabDmg.amount = Random.Range(4f, 6f);
                        _whirlwindBluntDmg.amount = Random.Range(2f, 6f);

                        //1084427009 <-- use this layer for a decent shit, ignores animal -- is flamer layers (still better than shitty c4 layer)
                        //-1 layer seems to work to detect almost everything but probably has bad perf
                        //1073741824 <-- tree mask

                        //should probably just do the fire fx and have it grant a bonus to radius and hit one or two extra times or something lol


                        var nearEnts = Pool.GetList<ResourceEntity>();
                        try
                        {

                            var radius = 1.32f + (0.39f * (whirlwind - 1));

                            if (isAmber)
                                radius *= Random.Range(1.25f, 2.5f);

                            //messy amber/legendary code. needs lots of improvement, but this is baseline. also, fireballs seem to take a really long time to actually appear after spawning? or they just seem invisible sometimes? I don't quite get it


                            nearEnts.Clear();

                            Vis.Entities(eyesPos, radius, nearEnts, TREE_LAYER_MASK);

                            _whirlwindHitInfo.Initiator = player;
                            _whirlwindHitInfo.Weapon = weapon;
                            _whirlwindHitInfo.WeaponPrefab = weapon;
                            _whirlwindHitInfo.DidGather = false;
                            _whirlwindHitInfo.CanGather = true;

                            for (int i = 0; i < nearEnts.Count; i++)
                            {
                                var ent = nearEnts[i];
                                if (ent == null || ent.IsDestroyed) continue;

                                if (ent.IsVisible(eyesPos))
                                {
                                    _whirlwindHitInfo.HitEntity = ent;
                                    _whirlwindHitInfo.damageTypes.Clear();
                                    _whirlwindHitInfo.HitPositionWorld = ent?.transform?.position ?? Vector3.zero;

                                    for (int j = 0; j < _whirlwindDmgTypes.Count; j++)
                                    {
                                        var dmgType = _whirlwindDmgTypes[j];
                                        if (ent == null || (ent?.IsDestroyed ?? true)) continue;

                                        _whirlwindHitInfo.DidGather = false;
                                        _whirlwindHitInfo.CanGather = true;
                                        _whirlwindHitInfo.HitEntity = ent;
                                       
                                        _whirlwindHitInfo.damageTypes.Set(dmgType.type, 5f);

                                        ent.OnAttacked(_whirlwindHitInfo);
                                        Interface.Oxide.CallHook("OnMeleeAttack", player, _whirlwindHitInfo); //above doesn't auto call this - to my surprise

                                        if (isAmber)
                                        {
                                            if (Random.Range(0, 101) <= 25) Effect.server.Run("assets/prefabs/ammo/arrow/fire/fireexplosion.prefab", SpreadVector(ent.transform.position, Random.Range(0.4f, 1.5f)), Vector3.zero);

                                            for (int k = 0; k < Random.Range(1, 3); k++)
                                                ent.OnAttacked(_whirlwindHitInfo);

                                        }

                                    }
                                }

                                var collider = ent?.GetComponentInChildren<Collider>() ?? ent?.GetComponentInParent<Collider>() ?? null;

                                if (collider == null)
                                {
                                    PrintWarning("collider null: " + ent.ShortPrefabName);
                                    continue;
                                }

                                var hitMat = ((collider == null) ? string.Empty : collider?.material?.name ?? collider?.sharedMaterial?.name ?? string.Empty).Replace("(Instance)", string.Empty, StringComparison.OrdinalIgnoreCase).TrimEnd().ToLower();
                                var hitCol = (hitMat != "default") ? StringPool.Get(hitMat) : 0;

                                var spreadEntPos = SpreadVector(ent.ClosestPoint(eyesPos), Random.Range(0.1f, 0.9f));
                                if (hitCol == 0) continue;

                                _whirlwindImpactInfo.HitPositionWorld = spreadEntPos;
                                _whirlwindImpactInfo.HitPositionLocal = Vector3.zero;
                                _whirlwindImpactInfo.HitMaterial = hitCol;

                                Effect.server.ImpactEffect(_whirlwindImpactInfo);

                            }
                            lastWhirlwindAttack[player.UserIDString] = Time.realtimeSinceStartup;
                        }
                        finally { Pool.FreeList(ref nearEnts); }


                    }

                }
                else if (_activeWhirlwind.Remove(player.UserIDString))
                {
                    OnWhirlwindEnd(player);
                }
            }
            finally 
            {
                try 
                {
                    watch.Stop();
                    if (watch.Elapsed.TotalMilliseconds > 3) PrintWarning(nameof(OnPlayerInput) + " for " + player?.displayName + " took: " + watch.Elapsed.TotalMilliseconds + "ms");
                }
                finally { Pool.Free(ref watch); }
            }
        }

        private readonly Dictionary<string, float> _lastForcedProjectileFxTime = new();

        private void OnWorldProjectileCreate(HitInfo info, Item item)
        {
            if (info == null || item == null) return;

            if ((info?.Weapon?.GetItem()?.skin ?? 0) != WINDFORCE_BOW_SKIN_ID)
            {
                return;
            }

            var ply = info?.InitiatorPlayer;
            if (ply == null) return;
            var projectileVelocity = info?.ProjectileVelocity ?? Vector3.zero;


            for (int i = 0; i < 3; i++)
            {
                var newItem = ItemManager.Create(item.info, 1);

                var hitPosWorldSpread = SpreadVector2(info.HitPositionWorld, Random.Range(0.08f, 0.16f));
                var hitPosLocalSpread = SpreadVector2(info.HitPositionLocal, Random.Range(0.04f, 0.1f));

                (!(info.HitEntity == null) ? ((int)info.HitBone != 0 ? newItem.CreateWorldObject(hitPosLocalSpread, Quaternion.LookRotation(info.HitNormalLocal * -1f), info.HitEntity, info.HitBone) : item.CreateWorldObject(hitPosLocalSpread, Quaternion.LookRotation(info.HitEntity.transform.InverseTransformDirection(projectileVelocity.normalized)), info.HitEntity, 0U)) : newItem.CreateWorldObject(hitPosWorldSpread, Quaternion.LookRotation(projectileVelocity.normalized), null, 0U)).GetComponent<Rigidbody>().isKinematic = true;


            }


        }

        private object OnClientProjectileEffectCreate(Connection sourceConnection, BaseProjectile weapon, string prefabName, Vector3 pos, Vector3 velocity, int seed, bool silenced = false, bool forceClientsideEffects = false)
        {
            var ply = weapon?.GetOwnerPlayer();

            if (ply == null) return null;

            var item = weapon?.GetItem() ?? GetActiveItem(ply);
            if (item == null) return null;

            if (item.skin == WINDFORCE_BOW_SKIN_ID)
            {

                if (!_lastForcedProjectileFxTime.TryGetValue(ply.UserIDString, out float lastTime) || (Time.realtimeSinceStartup - lastTime) > 1)
                {
                    _lastForcedProjectileFxTime[ply.UserIDString] = Time.realtimeSinceStartup;

                    weapon.CreateProjectileEffectClientside(prefabName, SpreadVector2(ply.eyes.position, 0.22f), velocity, seed, null, false, true);
                    weapon.CreateProjectileEffectClientside(prefabName, SpreadVector2(ply.eyes.position, 0.33f), velocity, seed, null, false, true);
                    weapon.CreateProjectileEffectClientside(prefabName, SpreadVector2(ply.eyes.position, 0.44f), velocity, seed, null, false, true);
                }
            }

            if (prefabName.Contains("arrow"))
                return null;


            if (item.skin == HUNTING_RIFLE_SKIN_ID)
                weapon.CreateProjectileEffectClientside("assets/prefabs/ammo/arrow/arrow_hv.prefab", ply.eyes.position, ply.eyes.BodyForward() * 150, Random.Range(0, 10000), null, false, true);



            return true;
        }

        private void OnExplosiveThrown(BasePlayer player, BaseEntity entity, ThrownWeapon weapon)
        {
            if (player == null || entity == null || weapon == null)
                return;

            var item = weapon?.GetCachedItem() ?? weapon?.GetItem();

            if ((item?.skin ?? 0) != LUMBERJACK_GRENADE_SKIN_ID)
                return;



            ShowToast(player, "Lumberjack Grenade!", 1);
        }

        IEnumerator ClusterGrenadeExplosion(TimedExplosive grenade, float fuseLength)
        {
            if (grenade == null)
                throw new ArgumentNullException(nameof(grenade));

            if (fuseLength <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(fuseLength));

            var player = grenade?.creatorEntity as BasePlayer;

            if (player == null)
            {
                PrintWarning("player null on grenade explosion!!!");
                yield break;
            }

            //temporarily hardcoded around 5 sec fuse

            var waitTime = 4.25f;

            PrintWarning("waiting: " + waitTime);

            yield return CoroutineEx.waitForSecondsRealtime(waitTime);

            PrintWarning("waited: " + waitTime);

            grenade.SetVelocity(Vector3.up * 5f);

            yield return CoroutineEx.waitForSecondsRealtime(0.68f);

            var pos = grenade?.transform?.position ?? Vector3.zero;

            if (pos == Vector3.zero)
            {
                PrintWarning(nameof(ClusterGrenadeExplosion) + " has vec3 zero pos!!!");

                yield break;
            }

            var grenadePrefab = grenade?.PrefabName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(grenadePrefab))
            {
                PrintError("Somehow grenade prefab is null/empty!!!");
                yield break;
            }

            var ownerId = grenade?.OwnerID ?? (grenade?.creatorEntity as BasePlayer)?.userID ?? 0;
            var entityOwner = grenade?.creatorEntity;

            for (int i = 0; i < 4; i++)
            {
                var adjPos = i switch
                {
                    0 => Vector3.left * 7.25f,
                    1 => Vector3.forward * 7.25f,
                    2 => Vector3.back * 7.25f,
                    _ => Vector3.right * 7.25f,
                };
                var newGrenade = (TimedExplosive)GameManager.server.CreateEntity(grenadePrefab, pos);

                newGrenade.skinID = 1337; //used in fuse set hook.

                newGrenade.OwnerID = ownerId;
                newGrenade.SetCreatorEntity(entityOwner);

                newGrenade.Spawn();

                newGrenade.SetVelocity(adjPos);

                yield return CoroutineEx.waitForEndOfFrame;
            }

            yield return CoroutineEx.waitForSecondsRealtime(0.2f);

            DropClusterGrenadeAsItem(pos, Vector3.up * 5f);

        }

        private Item _grenadeHatchet = null;
        private Item GetLumberJackGrenadeHatchet()
        {

            if (_grenadeHatchet != null && _grenadeHatchet.condition < _grenadeHatchet.maxCondition)
                _grenadeHatchet.RepairCondition(9999f);

            var held = _grenadeHatchet?.GetHeldEntity();
            if (held?.gameObject == null || held.IsDestroyed)
            {
                PrintWarning("null held!!!!");

                RemoveFromWorld(_grenadeHatchet);

                PrintWarning("removed from world (safety), creating...");

                _grenadeHatchet = ItemManager.CreateByName("hatchet", 1); //optimize later

                PrintWarning("recreated.");
            }

            return _grenadeHatchet;
        }

        private AttackEntity GetLumberJackGrenadeAttackEntity()
        {
            return GetLumberJackGrenadeHatchet()?.GetHeldEntity() as AttackEntity;
        }

        IEnumerator LumberjackGrenadeExplosion(TimedExplosive grenade, float fuseLength)
        {
            if (grenade == null)
                throw new ArgumentNullException(nameof(grenade));

            if (fuseLength <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(fuseLength));

            var player = grenade?.creatorEntity as BasePlayer;

            if (player == null)
            {
                PrintWarning("player null on grenade explosion!!!");
                yield break;
            }

            //temporarily hardcoded around 5 sec fuse

            var waitTime = 4.25f;

            yield return CoroutineEx.waitForSecondsRealtime(waitTime);

            grenade.SetVelocity(Vector3.up * 14f);

            yield return CoroutineEx.waitForSecondsRealtime(0.68f);

            var pos = grenade?.transform?.position ?? Vector3.zero;

            if (pos == Vector3.zero)
            {
                PrintWarning(nameof(LumberjackGrenadeExplosion) + " has vec3 zero pos!!!");

                yield break;
            }

            var nearTrees = Pool.GetList<TreeEntity>();

            try 
            {
                Vis.Entities(pos, 10f, nearTrees, TREE_LAYER_MASK);

                if (nearTrees.Count < 1)
                {
                    PrintWarning("nearTrees < 1!");

                    try
                    {
                        DropLumberjackGrenadeAsItem(pos, Vector3.up * 2.5f);
                    }
                    catch (Exception ex) { PrintError(ex.ToString()); }

                    yield break;
                }

                //major bug: hatchet item will break!!!

                var attackEnt = GetLumberJackGrenadeAttackEntity();
                var attackItem = GetLumberJackGrenadeHatchet();

                _lumberJackGrenadeHitInfo.Initiator = player;
                _lumberJackGrenadeHitInfo.Weapon = attackEnt;

                _lumberJackGrenadeHitInfo.WeaponPrefab = _lumberJackGrenadeHitInfo.Weapon;

                _lumberJackGrenadeHitInfo.DidGather = false;
                _lumberJackGrenadeHitInfo.CanGather = true;

                _lumberJackGrenadeHitInfo.damageTypes.Set(DamageType.Stab, 5f);

                attackEnt.SetParent(player, true, false);

                //crucial trys below so we remove this no matter what.
                //hacky setparent and itemlist are to avoid errors (parent) and assert failures (itemlist).

                try
                {
                    player.inventory.containerBelt.itemList.Add(attackItem);
                }
                catch (Exception ex) { PrintError(ex.ToString()); }


                try
                {
                    for (int i = 0; i < nearTrees.Count; i++)
                    {
                        var tree = nearTrees[i];

                        if (tree == null || tree.IsDestroyed)
                            continue;

                        StartCoroutine(LumberjackGrenadeKillTree(tree, _lumberJackGrenadeHitInfo));

                        var fxTimes = Random.Range(4, 12);

                        for (int j = 0; j < fxTimes; j++)
                        {
                            if (tree?.xMarker != null && !tree.xMarker.IsDestroyed)
                                Effect.server.Run("assets/bundled/prefabs/fx/impacts/blunt/wood/wood1.prefab", tree.xMarker.transform.position);
                        }
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                while(true)
                {
                    var timeToBreak = true;

                    for (int i = 0; i < nearTrees.Count; i++) //check if some trees are still alive
                    {
                        var tree = nearTrees[i];
                        if (tree != null && !tree.IsDestroyed && tree.health > 0f)
                        {
                            timeToBreak = false;
                            break;
                        }    
                    }

                    if (timeToBreak)
                    {
                        PrintWarning("all trees dead. break!");
                        break;
                    }

                    yield return CoroutineEx.waitForSecondsRealtime(0.1f);
                }

                PrintWarning("trying remove from itemlist, setparent null");

                try
                {
                    if (player?.inventory?.containerBelt?.itemList != null)
                        player.inventory.containerBelt.itemList.Remove(attackItem);
                }
                finally { attackEnt.SetParent(null, true); }


                PrintWarning("all done with undoing. time to spawn f1 grenade.");

                try
                {
                    DropLumberjackGrenadeAsItem(pos, Vector3.up * 2.5f);
                }
                catch(Exception ex) { PrintError(ex.ToString()); }


            }
            finally { Pool.FreeList(ref nearTrees); }

        }

        private void DropLumberjackGrenadeAsItem(Vector3 pos, Vector3 vel)
        {
            var nade = CreateLegendaryItemFromID(LUMBERJACK_GRENADE_SKIN_ID);

            if (!nade.Drop(pos, vel))
                RemoveFromWorld(nade);
        }

        private void DropClusterGrenadeAsItem(Vector3 pos, Vector3 vel)
        {
            var nade = CreateLegendaryItemFromID(CLUSTER_GRENADE_SKIN_ID);

            if (!nade.Drop(pos, vel))
                RemoveFromWorld(nade);
        }

        IEnumerator LumberjackGrenadeKillTree(TreeEntity tree, HitInfo info)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            if (info == null)
                throw new ArgumentNullException(nameof(info));

            if (!tree.IsDestroyed && tree.health <= 0f)
                tree.Kill();

            while (tree != null && tree.gameObject != null && tree.health > 0f)
            {
                _lumberJackGrenadeHitInfo.HitEntity = tree;
                _lumberJackGrenadeHitInfo.HitPositionWorld = tree?.transform?.position ?? Vector3.zero;
                _lumberJackGrenadeHitInfo.DidGather = false;
                _lumberJackGrenadeHitInfo.CanGather = true;

                tree.OnAttacked(info);

                yield return CoroutineEx.waitForEndOfFrame;
            }


        }

        private object OnExplosiveFuseSet(TimedExplosive explosive, float fuseLength)
        {
            if (explosive == null)
                return null;

            if (explosive.skinID == LUMBERJACK_GRENADE_SKIN_ID)
            {
                var newFuseLength = 5f;

                StartCoroutine(LumberjackGrenadeExplosion(explosive, newFuseLength));

                return newFuseLength;
            }
            else if (explosive.skinID == CLUSTER_GRENADE_SKIN_ID)
            {
                PrintWarning("cluster grenade!");
                StartCoroutine(ClusterGrenadeExplosion(explosive, 5f));
                return 5f;
            }
            else if (explosive.skinID == 1337)
                return 0.75f; //cluster grenade!

            return null;
        }

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (projectile == null || player == null || mod == null || projectiles == null) return;

            var projectileList = projectiles?.projectiles ?? null;
            if (projectileList == null || projectileList.Count < 1)
            {
                PrintWarning(nameof(OnWeaponFired) + " called with no projectiles!");

                return; //may occur during antihack?
            }

            var skinId = projectile?.skinID ?? 0;

            if (skinId == WINDFORCE_BOW_SKIN_ID)
            {
                projectile.primaryMagazine.contents = 0;
                projectile.SendNetworkUpdate();
            }
            else if (skinId == FANG_SAP_SKIN_ID)
            {
                var forwardPos = (player?.eyes?.position ?? Vector3.zero) + ((player?.eyes?.BodyForward() ?? Vector3.zero) * Random.Range(2f, 2.15f)) + ((player?.eyes?.BodyUp() ?? Vector3.zero) * Random.Range(0.05f, 0.075f));

                //maybe change to a parented effect later
                Effect.server.Run("assets/prefabs/npc/m2bradley/effects/sidegun_muzzleflash.prefab", forwardPos);
            }
            else if (skinId == GRISWOLDS_BLUNDERBUSS_SKIN_ID)
            {
                for (int i = 0; i < projectileList.Count; i++)
                {
                    var pj = projectileList[i];

                    if (pj.projectileID != 0)
                    {
                        _blunderBussProjectileIDs.Add(pj.projectileID);

                        //todo: more fx/better fx.
                        Effect.server.Run("assets/bundled/prefabs/fx/collect/collect stone.prefab", projectile.transform.position, Vector3.zero);

                    }
                }
            }

            var hasWolfhound = false;

            var hasPackratsBane = false;

            var items = projectile?.GetItem()?.contents?.itemList;
            
            if (items != null && items.Count > 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.skin == WOLFHOUND_SUPPRESSOR_SKIN_ID)
                    {
                        hasWolfhound = true;
                        break; //cannot use silencer and boost so break is safe
                    }
                    else if (item.skin == PACKRATS_BANE_MUZZLE_BOOST_SKIN_ID)
                    {
                        hasPackratsBane = true;
                        break;
                    }
                    
                }
            }

            if (hasPackratsBane)
            {
                for (int i = 0; i < projectileList.Count; i++)
                {
                    var pj = projectileList[i];

                    if (pj.projectileID != 0)
                        _packratsBaneProjectileIDs.Add(pj.projectileID);
                }

                //assets/prefabs/misc/halloween/candies/candyexplosion.prefab

                Vector3 usePos;

                if (Physics.SphereCast(new Ray(player.eyes.position, player.eyes.rotation * Vector3.forward), 0.5f, out RaycastHit hit, 2.5f)) usePos = hit.point;
                else
                {
                    var lowEyePos = player.eyes.position;
                    lowEyePos.y -= 0.125f;

                    usePos = lowEyePos + player.eyes.HeadForward() * (!player.serverInput.IsDown(BUTTON.FORWARD) ? 2.4f : 3.15f);
                }

                if (usePos != Vector3.zero)
                {
                    for (int i = 0; i < Random.Range(7, 11); i++)
                    {
                        var spreadPos = i == 0 ? usePos : SpreadVector2(usePos, Random.Range(0.25f, 1.25f));

                        Effect.server.Run("assets/prefabs/misc/halloween/candies/candyexplosion.prefab", spreadPos, Vector3.zero);
                    }
                }
                else PrintWarning("vec 3 zero usePos for suppressor?!?!?!");

            }


            if (hasWolfhound && Random.Range(0, 101) <= 10)
            {
                
                for (int i = 0; i < projectileList.Count; i++)
                {
                    var pj = projectileList[i];

                    if (pj.projectileID != 0) 
                        _wolfHoundProjectileIDs.Add(pj.projectileID);
                }

                Vector3 usePos;

                if (Physics.SphereCast(new Ray(player.eyes.position, player.eyes.rotation * Vector3.forward), 0.5f, out RaycastHit hit, 2.5f)) usePos = hit.point;
                else
                {
                    var lowEyePos = player.eyes.position;
                    lowEyePos.y -= 0.125f;

                    usePos = lowEyePos + player.eyes.HeadForward() * (!player.serverInput.IsDown(BUTTON.FORWARD) ? 3f : 3.67f);
                }

                if (usePos != Vector3.zero)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var spreadPos = i == 0 ? usePos : SpreadVector2(usePos, Random.Range(0.25f, 1.25f));

                        Effect.server.Run("assets/prefabs/ammo/arrow/fire/fireexplosion.prefab", spreadPos, Vector3.zero);
                    }
                }
                else PrintWarning("vec 3 zero usePos for suppressor?!?!?!");
            }

            if (!_overDrivePlayers.Contains(player.userID)) 
                return;

            var targetEnemy = GetOverdriveTarget(player, 90f, 1.25f, PLAYER_LAYER_MASK) ?? GetOverdriveTarget(player, 90f, 1.25f, 2048);

            if (targetEnemy == null || targetEnemy == player)
                return;

            if (targetEnemy.prefabID == 2421623959) //horse check - mostly to avoid hitting own horse.
                return;

            var targetPlayer = targetEnemy as BasePlayer;
            if (targetPlayer != null && (targetPlayer.IsConnected || targetPlayer.IsDead() || targetPlayer.IsSleeping())) return;

        

            var proj = mod?.projectileObject?.Get()?.GetComponent<Projectile>();

            var hitInfo = new HitInfo(player, targetEnemy, DamageType.Bullet, 40)
            {
                Weapon = projectile,
                WeaponPrefab = projectile,
                HitBone = StringPool.Get("head"),
                DoHitEffects = true,
                IsPredicting = false,
                UseProtection = true
            };

            proj.CalculateDamage(hitInfo, projectile.GetProjectileModifier(), 1f);

            mod.ServerProjectileHit(hitInfo);

            targetEnemy.OnAttacked(hitInfo);
            SendLocalEffect(player, "assets/bundled/prefabs/fx/headshot_2d.prefab");
            targetEnemy.ClientRPCPlayer(null, player, "HitNotify", false);

            /*/    var targetHeadPos = targetEnemy?.FindBone("head")?.transform?.position ?? Vector3.zero;
                if (targetHeadPos != Vector3.zero)
                    targetHeadPos = new Vector3(targetHeadPos.x, targetHeadPos.y - 0.25f, targetHeadPos.z);
               else targetHeadPos = targetEnemy?.transform?.position ?? Vector3.zero; /*/

            // var lookPos = Vector3Ex.Direction2D(targetEnemy.FindBone("head").transform.position, player.eyes.position);

            //temp isshady check since this seems broken now.
            if (IsShady(player.userID))
            {
                var newAim = Vector3Ex.Direction2D(targetEnemy.transform.position, player.transform.position);

                PrintWarning("newAim: " + newAim);

                var lookRot = Quaternion.LookRotation(newAim);
                player.eyes.rotation = Quaternion.LookRotation(newAim, Vector3.up);
                player.viewAngles = player.eyes.rotation.eulerAngles;
                var lookPos = lookRot.eulerAngles;

                //   this.SetAimDirection(Vector3Ex.Direction2D(basePlayer1.eyes.position, this.eyes.position));

                PrintWarning("lookPos: " + lookPos);

                player.ClientRPCPlayer(null, player, "ForceViewAnglesTo", newAim);
            }

   

            projectile.primaryMagazine.contents = Mathf.Clamp(projectile.primaryMagazine.contents + 3, projectile.primaryMagazine.contents, projectile.primaryMagazine.capacity);
            projectile.SendNetworkUpdate();

            _overdriveHitInfo[player.UserIDString] = hitInfo;

        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;

            try 
            {
                var backpack = GetBackpackItem(player);

                if (backpack != null)
                    CreateBackpackEntityForPlayer(player, backpack);

            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            try { SendF1HelpMessage(player); }
            catch(Exception ex) { PrintError(ex.ToString()); }

            try
            {
                if (ShouldHaveCharmComponent(player)) 
                    EnsureCharmComponent(player);
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try { loadUser(player); }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try { setLastConnectionTime(player.userID); }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try
            {
                var luckInfo = GetLuckInfo(player);

                if (luckInfo != null)
                {
                    for (int i = 0; i < _statInfos.Count; i++)
                    {
                        var stat = _statInfos[i];
                        if (stat == null) continue;

                        var lvlInStat = luckInfo.GetStatLevel(stat, false);

                        if (lvlInStat >= stat.MaxLevel)
                            GrantMasteryAchievement(player, stat);
                    }
                }
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try
            {
                var userClass = GetUserClass(player.UserIDString);
                if (userClass != null && userClass.Type != ClassType.Undefined)
                    DiscordAPI2?.Call("GiveRoleWithColor", player.UserIDString, userClass.DisplayName, userClass.PrimaryColor);
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null || !player.IsConnected || player.IsDestroyed || player.gameObject == null) return;

            if (!_wokenPlayers.Add(player.userID)) 
                return; //has already woken up

            if (GetUserClassType(player.UserIDString) != ClassType.Undefined)
                return;

            _classGuiOnWakeTime[player.UserIDString] = Time.realtimeSinceStartup;

            GrantImmunity(player, 30f);
            ClassGUINew(player);
            SendGeneralClassText(player?.IPlayer);

            SendLocalEffect(player, "assets/prefabs/missions/effects/mission_victory.prefab");
        }

        private SleepingBag SpawnSleepingBagOnPointAndAssign(BasePlayer player, Vector3 pos, float cooldownSecs = 300f, string bagName = "")
        {
            var oldParent = player?.GetParentEntity();

            var bag = GameManager.server.CreateEntity("assets/prefabs/deployable/sleeping bag/sleepingbag_leather_deployed.prefab", pos) as SleepingBag;



            bag.OwnerID = player.userID;
            bag.deployerUserID = player.userID;

            bag.Invoke(() =>
            {
                bag.SetUnlockTime(Time.realtimeSinceStartup + cooldownSecs);
            }, 1f);


            if (!string.IsNullOrWhiteSpace(bagName))
                bag.niceName = bagName;

            bag.Spawn();

            if (oldParent != null && !oldParent.IsDestroyed)
                bag.SetParent(oldParent, true, true);




            return bag;
        }

        private object OnPlayerWound(BasePlayer player, HitInfo info)
        {
            if (player == null) 
                return null;

            if (info?.InitiatorPlayer != null && (info?.InitiatorPlayer == player || IsValidPlayer(info?.InitiatorPlayer))) //don't allow pvp or self harm
                return null;

            if (!IsWearingSkin(player, YESTERYEAR_SKIN_ID))
                return null;

            var snap = GetMetabolicSnapshot(player.UserIDString);

            if (snap == null)
            {
                PrintWarning("had yesteryear, but snapshot null?!");
                return null;
            }

            if (snap.TimeSinceTaken.TotalMinutes < 5)
            {
                PrintWarning(nameof(snap.TimeSinceTaken) + " is not at least 5 minutes old!: " + snap.TimeSinceTaken.TotalSeconds + " seconds");
            }

            if (snap.HasBeenApplied)
            {
                PrintWarning("Snap has already been applied! no yesteryear until next snap.");
                return null;
            }

            PrintWarning("yesteryear!");

            SendReply(player, "Yesteryear.");
            ShowToast(player, "Yesteryear.");

            //nice, calm fx. a nice, soothing, calm FX. like Yesteryear from RuneScape.

            MetabolismSnapshot.ApplyMetabolism(player, snap);

            return true;
        }

        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (!IsValidPlayer(player)) 
                return;

            var luckInfo = GetLuckInfo(player);

            var swLvl = luckInfo?.GetStatLevelByName("SecondWind") ?? 0;

            var attackerPlayer = info?.InitiatorPlayer;

            if (swLvl > 0 && (attackerPlayer == null || (!IsValidPlayer(attackerPlayer) && attackerPlayer != player))) //checks to ensure is not real/connected player, but is npc or otherwise attackerPlayer is null.. also checks to ensure didn't kill self
            {
                var swCdHandler = player?.GetComponent<SecondWindCooldownHandler>() ?? player?.gameObject?.AddComponent<SecondWindCooldownHandler>();

                if (!swCdHandler.IsOnCooldown)
                {

                    var userId = player?.userID ?? 0;

                    swCdHandler.StartCooldown();

                    var deathPos = player?.transform?.position ?? Vector3.zero;


                    var respawnTimer = 300 - (swLvl * 60);

                    var bag = SpawnSleepingBagOnPointAndAssign(player, deathPos, respawnTimer, "Second Wind Sleeping Bag"); //second wind max level not 5, but 10 - reversed? not 10, but 5, no?

                    bag.Invoke(() =>
                    {
                        if (!(bag?.IsDestroyed ?? true))
                            bag.Kill();

                    }, respawnTimer + 90f);

                    _secondWindSleepingBags.Add(bag.net.ID);



                }
                else SendLuckNotifications(player, "Second Wind is on cooldown, so no bag has been dropped.");
            }


            if (!_userBackpack.TryGetValue(player.UserIDString, out BaseCombatEntity backpack))
                return;

            backpack.Invoke(() =>
            {
                if (backpack != null && !backpack.IsDestroyed)
                {
                    backpack.SetParent(null);
                    backpack.transform.position = new Vector3(20, -10, 20);
                }
            }, 1.5f);

            _userBackpack.Remove(player.UserIDString);

        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;

            try
            {
                try { OnWhirlwindEnd(player); }
                finally { RemoveCharmComponent(player); }
            }
            finally { saveUser(player.UserIDString); }
        }

        public void SaveUsers()
        {

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || !player.IsConnected) continue;

                saveUser(player.UserIDString);
            }

            SaveData();
        }

        public void saveUser(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));

            var lvl = getLevelS(userID);
            if (lvl < 1)
            {
                PrintWarning(nameof(getLevelS) + " on " + nameof(saveUser) + " returned level < 1 for userId: " + userID + " !! adjusting");
                lvl = 1;
            }

            var points = getPointsS(userID);
            var lvl1Points = GetPointsForLevel(1);
            if (points < lvl1Points) points = lvl1Points;

            SetPointsAndLevel(userID, points, lvl);
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("LuckData2", storedData3);
        private void SaveCharmData() => Interface.Oxide.DataFileSystem.WriteObject("Charms", charmsData);

        private void SaveClassData() => Interface.Oxide.DataFileSystem.WriteObject("LuckUserClasses", _userClass);

        private void SaveLegendaryData() => Interface.Oxide.DataFileSystem.WriteObject("LuckLegendaries", _legendaryData);
        private void SaveNPCGearSetData() => Interface.Oxide.DataFileSystem.WriteObject("LuckNPCGearSets", _npcGearData);
        private void SaveLuckStatInfos() => Interface.Oxide.DataFileSystem.WriteObject("LuckStats", _statInfos);

        private void SaveBackpackData() => Interface.Oxide.DataFileSystem.WriteObject("LuckBackpacks", _itemUIDToBackPackContainerID);


        [ConsoleCommand("luck.deletelootcharms")]
        private void cmdDeleteLootCharms(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin) return;

            arg.ReplyWith("deleting...");

            var c = 0;

            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var lootCont = entity as LootContainer;

                if (lootCont == null)
                    continue;

                var itemList = lootCont.inventory.itemList;

                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList[i];

                    if (IsCharm(item))
                    {
                        
                        RemoveFromWorld(item);
                        c++;
                    }

                }

            }

            arg.ReplyWith("deleted " + c.ToString("N0") + " charms from loot containers");
        }

        [ConsoleCommand("luck.setlvl")]
        private void lklvlCommand(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin) 
                return;

            var args = arg?.Args ?? null;
            if (args == null || args.Length != 2)
            {
                SendReply(arg, "Please supply a target name and lvl, PlayerName Level");
                return;
            }

            if (!ulong.TryParse(args[0], out ulong targetID)) 
                targetID = FindPlayerByPartialName(args[0], true)?.userID ?? 0;

            if (targetID <= 0)
            {
                SendReply(arg, "Invalid target supplied/not found");
                return;
            }

            if (!long.TryParse(args[1], out long lvl))
            {
                SendReply(arg, "Not a number: " + args[1]);
                return;
            }

            var xp = GetPointsForLevel(lvl);
            Puts("XP is: " + xp);

            var idStr = targetID.ToString();

            SetPointsAndLevel(idStr, xp, lvl);

            saveUser(idStr);

            var target = FindPlayerByID(targetID);
            if (target != null) 
            {
                UpdateGUI(target);
                SendReply(target, "Your Luck level was set to: " + lvl + " by an administrator!");
            }
         
           
            Puts("Set luck level to: " + lvl + " for player: " + args[0]);

        }


        [ConsoleCommand("luck.charmtest")]
        private void lkCharmTest(ConsoleSystem.Arg arg)
        {
            var watch = new Stopwatch();
            var selWatch = new Stopwatch();
            var loopWatch = new Stopwatch();
            watch.Start();
            if (arg?.Connection != null) return;
            if (arg?.Args == null || arg.Args.Length < 1)
            {
                SendReply(arg, "Supply a number of times to run test (max 10,000)");
                return;
            }
            if (!int.TryParse(arg.Args[0], out int times))
            {
                SendReply(arg, "Not a number: " + arg.Args[0]);
                return;
            }
            if (times < 1)
            {
                SendReply(arg, "Times must be > 0");
                return;
            }
            if (times > 100000)
            {
                SendReply(arg, "Times must be < 100,000");
                return;
            }
            var charms = new List<Charm>();
            loopWatch.Start();
            for (int i = 0; i < times; i++)
            {
                var charm = CreateRandomCharm();
                if (charm == null) continue;
                var getCharm = charmsData.GetCharm(charm);
                RemoveFromWorld(charm);
                charms.Add(getCharm);
            }
            loopWatch.Stop();

            selWatch.Start();
            var maxXP = charms?.Select(p => p.xpBonus)?.Max() ?? 0f;
            var maxItem = charms?.Select(p => p.itemBonus)?.Max() ?? 0f;
            var maxXPCharm = charms?.Where(p => p.xpBonus == maxXP)?.FirstOrDefault() ?? null;
            var maxItemCharm = charms?.Where(p => p.itemBonus == maxItem)?.FirstOrDefault() ?? null;
            selWatch.Stop();
            PrintWarning("max xp: " + maxXP + " (" + maxXPCharm.Skill + ", item bonus: " + maxXPCharm.itemBonus + "), maxItem: " + maxItem + " (" + maxItemCharm.Skill + ", xp bonus: " + maxItemCharm.xpBonus + ")");



            watch.Stop();
            PrintWarning(times.ToString("N0") + " charms took: " + watch.Elapsed.TotalMilliseconds + "ms, selWatch: " + selWatch.Elapsed.TotalMilliseconds + "ms, loopWatch: " + loopWatch.Elapsed.TotalMilliseconds + "ms");

        }

        [ConsoleCommand("luck.charmmax")]
        private void lkCharmMax(ConsoleSystem.Arg arg)
        {
            var watch = new Stopwatch();
            var selWatch = new Stopwatch();
            var loopWatch = new Stopwatch();
            watch.Start();
            if (arg?.Connection != null) return;


            var charms = new List<Charm>();
            loopWatch.Start();
            var foundMaxCharm = false;
            var count = 0;
            var max = 100000; //100,000
            while (!foundMaxCharm)
            {
                count++;
                if (count >= max)
                {
                    PrintWarning("safety hit " + count + "!");
                    break;
                }
                var charm = CreateRandomCharm();
                if (charm == null) continue;
                var getCharm = charmsData.GetCharm(charm);
                RemoveFromWorld(charm);
                charms.Add(getCharm);
                if (getCharm.itemBonus > 100 && getCharm.xpBonus > 100)
                {
                    PrintWarning("FOUND > 100 for both after: " + count.ToString("N0") + ", charm: " + getCharm);
                    break;
                }
            }
            loopWatch.Stop();

            selWatch.Start();
            var maxXP = charms?.Select(p => p.xpBonus)?.Max() ?? 0f;
            var maxItem = charms?.Select(p => p.itemBonus)?.Max() ?? 0f;
            var maxXPCharm = charms?.Where(p => p.xpBonus == maxXP)?.FirstOrDefault() ?? null;
            var maxItemCharm = charms?.Where(p => p.itemBonus == maxItem)?.FirstOrDefault() ?? null;
            selWatch.Stop();
            PrintWarning("max xp: " + maxXP + " (" + maxXPCharm.Skill + ", item bonus: " + maxXPCharm.itemBonus + "), maxItem: " + maxItem + " (" + maxItemCharm.Skill + ", xp bonus: " + maxItemCharm.xpBonus + ")");
            for (int i = 0; i < charms.Count; i++)
            {
                var charm = charms[i];
                foreach (var kvp in charmsData.charms.ToDictionary(p => p.Key, p => p.Value))
                {
                    if (kvp.Value == charm)
                    {
                        charmsData.charms.Remove(kvp.Key);
                    }
                }
            }


            watch.Stop();
            PrintWarning(count.ToString("N0") + " charms took: " + watch.Elapsed.TotalMilliseconds + "ms, selWatch: " + selWatch.Elapsed.TotalMilliseconds + "ms, loopWatch: " + loopWatch.Elapsed.TotalMilliseconds + "ms");

        }

        private class StoredData3
        {
            public Dictionary<string, LuckInfo> luckInfos = new();
            public StoredData3() { }
        }

        private StoredData3 storedData3;

        #region Stats
       
        private object OnServerCommand(ConsoleSystem.Arg arg)
        {
            if (arg == null) return null;
            var args = arg?.Args ?? null;
            var command = arg?.cmd?.FullName ?? string.Empty;

            if (string.IsNullOrEmpty(command) || args == null) return null;
            if (args != null && args.Length > 1 && command.Equals("note.update", StringComparison.OrdinalIgnoreCase))
            {
                var note = FindItemByID(args[0]);
                if (note == null || !IsCharm(note)) return null;
                var oldText = note?.text ?? string.Empty;
                NextTick(() => { if (note != null) note.text = oldText; });
            }

            return null;
        }

        [ChatCommand("restest")]
        private void cmdTestRes(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            
            var amts = new List<ItemAmount>();


            var amt2 = new ItemAmount(ItemManager.FindItemDefinition(STONES_ITEM_ID), 69);

            var amt1 = new ItemAmount(ItemManager.FindItemDefinition(BOONIE_HAT_ITEM_ID), 2);

            amts.Add(amt2);
            amts.Add(amt1);

            SendReply(player, "sending");

            OnRepairFailedResources(player, amts);

            SendReply(player, "sent");
        }

        [ChatCommand("genhit")]
        private void cmdGenHit(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            SendReply(player, "Generating");
            GenerateHitTargets();
            SendReply(player, "Generated");
        }

        [ChatCommand("chancetest")]
        private void cmdScaleTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 1 || !double.TryParse(args[0], out double num)) return;

            SendReply(player, "chance scaled " + num + " is: " + GetChanceScaledDouble(player.userID, num, num, 99));
           
        }

        [ChatCommand("bpsavetest")]
        private void cmdBpSaveTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (!_userBackpack.TryGetValue(player.UserIDString, out BaseCombatEntity backpack))
            {
                SendReply(player, "no backpack found!!");
                return;

            }

            SendReply(player, "in savelist?: " + BaseEntity.saveList.Contains(backpack) + ", enableSaving?: " + backpack.enableSaving);

        }

        private void EchoMessage(BasePlayer player, string msg)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (!player.IsConnected) return;

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                player.SendConsoleCommand(sb.Clear().Append("echo ").Append(msg).ToString());
            }
            finally { Pool.Free(ref sb); }

        }

        private void SendF1HelpMessage(BasePlayer player)
        {
            if (player == null) 
                throw new ArgumentNullException(nameof(player));

            EchoMessage(player, F1_HELP_MESSAGE);
        }

        [ChatCommand("f1")]
        private void cmdF1(BasePlayer player, string command, string[] args)
        {
            var msg = args.Length > 0 ? string.Join(" ", args) : F1_HELP_MESSAGE;
            EchoMessage(player, msg);
        }

        private readonly Dictionary<string, NPCHitTarget.Tiers> _selectedGearTier = new();

        [ChatCommand("gs")]
        private void cmdGearSet(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            int numTier;

            if (!_selectedGearTier.TryGetValue(player.UserIDString, out NPCHitTarget.Tiers tier))
            {
                SendReply(player, "You need to first select a tier. Try /" + command + " 2, to select " + ((NPCHitTarget.Tiers)2));

                if (args.Length < 1)
                    return;
                

                if (!int.TryParse(args[0], out numTier) || numTier > 6)
                {
                    SendReply(player, "You MUST specify an int and it must be within range!");
                    return;
                }
                else
                {
                    _selectedGearTier[player.UserIDString] = (NPCHitTarget.Tiers)numTier;
                    SendReply(player, "Selected tier: " + (NPCHitTarget.Tiers)numTier + " proceed using /" + command);
                    return;
                }
            }

            
            if (args.Length > 0 && int.TryParse(args[0], out numTier))
            {
                if (numTier > 6)
                {
                    SendReply(player, "out of range!");
                    return;
                }
                _selectedGearTier[player.UserIDString] = (NPCHitTarget.Tiers)numTier;
                SendReply(player, "Selected tier: " + (NPCHitTarget.Tiers)numTier + " proceed using /" + command);
                return;
            }

            if (!_npcGearData.TierGears.TryGetValue(tier, out NPCGearSet gearSet))
            {
                SendReply(player, "Gear set for this tier did not exist yet, so we created it (Tier: " + tier + ")");
                _npcGearData.TierGears[tier] = new NPCGearSet();
            }

            if (args.Length < 1)
            {
                SendReply(player, "You must specify arguments, such as:\n\n" + "/" + command + "\nlist Head\nlist Chest\nlist Pants\nadd Head ItemName/ID\nadd Chest ItemName/ID\nremove Head/Chest/Pants/Etc <index>");
                return;
            }

            var arg0 = args[0];

            if (arg0.Equals("copy", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length < 2)
                {
                    SendReply(player, "Your second argument should be the tier to copy this tier (" + tier + ") over to!");
                    return;
                }

                if (!int.TryParse(args[1], out int copyToTier) || copyToTier > 6)
                {
                    SendReply(player, "You have specified an invalid number: " + args[1]);
                    return;
                }

                var desiredTier = (NPCHitTarget.Tiers)copyToTier;

                if (desiredTier == tier)
                {
                    SendReply(player, "Cannot copy to self!");
                    return;
                }

                PrintWarning("desiredTier: " + desiredTier);


                if (!_npcGearData.TierGears.TryGetValue(desiredTier, out NPCGearSet gearTarget))
                {
                    PrintWarning("had to create new gear set data");
                    gearTarget = new NPCGearSet();
                    _npcGearData.TierGears[desiredTier] = gearTarget;
                }
                else PrintWarning("gear set data already existed");

                SendReply(player, "Counts prior to AddRange: " + gearTarget.ClothingOptions.Count + ", " + gearTarget.WeaponOptions.Count);


                gearTarget.ClothingOptions = gearSet.ClothingOptions.ToDictionary(p => p.Key, p => p.Value);
                gearTarget.WeaponOptions.AddRange(gearSet.WeaponOptions);

                SendReply(player, "New clothing options count: " + gearTarget.ClothingOptions.Count + "\nNew weapon options count: " + gearTarget.WeaponOptions.Count);
                SendReply(player, "Copied");
            }

            if (arg0.Equals("list"))
            {
                NPCGearSet.WearType itemType;

                if (args.Length < 2)
                {
                    SendReply(player, "You must specify the type, such as Head. Use ints.");
                    return;
                }

                if (!int.TryParse(args[1], out int typeNum) || typeNum > 6)
                {
                    SendReply(player, "Not a number or out of range: " + args[1]);
                    return;
                }

                itemType = (NPCGearSet.WearType)typeNum;

                if (!gearSet.ClothingOptions.TryGetValue(itemType, out List<WeightedItemInfo> items) || items.Count < 1)
                {
                    SendReply(player, "No items for wear type: " + itemType);
                    return;
                }

                var sb = Pool.Get<StringBuilder>();
                try 
                {
                    sb.Clear();

                    for (int i = 0; i < items.Count; i++)
                    {
                        var clothingItem = items[i];

                        var itemName = ItemManager.FindItemDefinition(clothingItem.ItemID)?.displayName?.english ?? string.Empty;

                        sb.Append(itemName).Append(" (").Append(clothingItem.ItemID).Append(", ").Append(clothingItem.SkinID).Append(")").Append(Environment.NewLine);

                    }

                    if (sb.Length > 1) sb.Length -= 1; //trim \n

                    SendReply(player, sb.ToString());

                }
                finally { Pool.Free(ref sb); }

                return;
            }

            if (arg0.Equals("add"))
            {
                if (args.Length < 2)
                {
                    SendReply(player, "You must be holding an item and specify the weight (0 [no chance] - 1 [100%]). Example: /" + command + " add 0.125");
                    return;
                }

                if (!float.TryParse(args[1], out float weight))
                {
                    SendReply(player, "Not a float: " + args[1]);
                    return;
                }

                var heldItem = player?.GetActiveItem();
                var heldEntity = player?.GetHeldEntity();

                if (heldItem == null)
                {
                    SendReply(player, "You must be holding an item to add!");
                    return;
                }

                NPCGearSet.WearType wearType = NPCGearSet.WearType.Undefined;

                var modWearable = heldItem?.info?.GetComponent<ItemModWearable>();
                if (modWearable != null)
                {
                    SendReply(player, "Detected clothing");

                    var wearable = modWearable?.entityPrefab?.Get()?.GetComponent<Wearable>();
                    if (wearable == null)
                    {
                        SendReply(player, "Wearable null!!");
                        return;
                    }
                    else SendReply(player, "Got wearable");

                    var chest = (wearable.occupationUnder & (Wearable.OccupationSlots.TorsoBack | Wearable.OccupationSlots.TorsoFront | Wearable.OccupationSlots.LeftArm | Wearable.OccupationSlots.RightArm)) != 0 || (wearable.occupationOver & (Wearable.OccupationSlots.TorsoBack | Wearable.OccupationSlots.TorsoFront | Wearable.OccupationSlots.LeftArm | Wearable.OccupationSlots.RightArm)) != 0;
                    var head = (wearable.occupationUnder & (Wearable.OccupationSlots.Face | Wearable.OccupationSlots.Eyes | Wearable.OccupationSlots.HeadBack | Wearable.OccupationSlots.HeadTop | Wearable.OccupationSlots.Mouth)) != 0 || (wearable.occupationOver & (Wearable.OccupationSlots.Face | Wearable.OccupationSlots.Eyes | Wearable.OccupationSlots.HeadBack | Wearable.OccupationSlots.HeadTop | Wearable.OccupationSlots.Mouth)) != 0;
                    var legs = (wearable.occupationUnder & (Wearable.OccupationSlots.Bum | Wearable.OccupationSlots.LeftKnee | Wearable.OccupationSlots.LeftLeg | Wearable.OccupationSlots.RightKnee | Wearable.OccupationSlots.RightLeg | Wearable.OccupationSlots.Groin)) != 0 || (wearable.occupationOver & (Wearable.OccupationSlots.Bum | Wearable.OccupationSlots.LeftKnee | Wearable.OccupationSlots.LeftLeg | Wearable.OccupationSlots.RightKnee | Wearable.OccupationSlots.RightLeg | Wearable.OccupationSlots.Groin)) != 0;
                    var feet = (wearable.occupationUnder & (Wearable.OccupationSlots.LeftFoot | Wearable.OccupationSlots.RightFoot)) != 0 || (wearable.occupationOver & (Wearable.OccupationSlots.LeftFoot | Wearable.OccupationSlots.RightFoot)) != 0;

                    wearType = chest ? NPCGearSet.WearType.Chest : head ? NPCGearSet.WearType.Head : legs ? NPCGearSet.WearType.Legs : feet ? NPCGearSet.WearType.Feet : NPCGearSet.WearType.Undefined;

                    if (wearType == NPCGearSet.WearType.Undefined)
                    {
                        SendReply(player, "Undefined type, can't continue!");
                        return;
                    }

                    SendReply(player, "Detected wearType: " + wearType);

                    gearSet.AddClothingOption(wearType, new WeightedItemInfo(heldItem.info.itemid, weight));

                    SendReply(player, "Added: " + heldItem.info.displayName.english + " with weight: " + weight);

                    return;
                }

                var baseProj = heldEntity?.GetComponent<BaseProjectile>();

                if (baseProj == null)
                {
                    SendReply(player, "Item is unsupported! Not a weapon/clothing?!: " + heldItem?.info?.shortname);
                    return;
                }

                gearSet.AddWeaponOption(new WeightedItemInfo(heldItem.info.itemid, weight));
                SendReply(player, "Added weapon: " + heldItem.info.displayName.english + " weight: " + weight);

            }

        }

        [ChatCommand("rjt")]
        private void cmdRejackToggle(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (_forceRejack.Add(player.userID)) SendReply(player, "Forcing rejack");
            else
            {
                _forceRejack.Remove(player.userID);
                SendReply(player, "No longer forcing");
            }
        }

        [ChatCommand("bpt2")]
        private void cmdBpTest2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 3) return;

            if (_testBackpack == null)
            {
                SendReply(player, "no bp");
                return;
            }


            if (!float.TryParse(args[0], out float x)) return;


            if (!float.TryParse(args[1], out float y)) return;



            if (!float.TryParse(args[2], out float z)) return;


            _testBackpack.transform.localPosition = new Vector3(x, y, z);
            _testBackpack.SendNetworkUpdate();

            SendReply(player, "set pos");
        }

        [ChatCommand("bpt")]
        private void cmdBpTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 4) return;

            if (_testBackpack == null)
            {
                SendReply(player, "no bp");
                return;
            }


            if (!float.TryParse(args[0], out float x)) return;


            if (!float.TryParse(args[1], out float y)) return;



            if (!float.TryParse(args[2], out float z)) return;



            if (!float.TryParse(args[3], out float w)) return;

            _testBackpack.transform.localRotation = new Quaternion(x, y, z, w);
            _testBackpack.SendNetworkUpdate();

            SendReply(player, "set");
        }

        [ChatCommand("bptspawn")]
        private void cmdBpTestSpawn(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 1) return;

            var prefab = args[0];

            var entity = GameManager.server.CreateEntity(prefab, player.transform.position);
            if (entity == null || entity.IsDestroyed) return;

            entity.enableSaving = false;

            entity.Spawn();

            entity.EnableSaving(false);

            entity.SetParent(player, "spine1", true, true);

            var rigid = entity?.GetComponent<Rigidbody>() ?? entity?.GetComponentInParent<Rigidbody>() ?? entity?.GetComponentInChildren<Rigidbody>() ?? null;
            if (rigid != null)
            {
                rigid.isKinematic = true;
                rigid.useGravity = false;
            }

            _testBackpack = entity as BaseCombatEntity;

            SendReply(player, "Spawned on spine1");
        }

        [ChatCommand("ovd")]
        private void cmdOverdriveTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (_overDrivePlayers.Add(player.userID)) SendReply(player, "Forcing overdrive");
            else
            {
                _overDrivePlayers.Remove(player.userID);
                SendReply(player, "No longer forcing overdrive");
            }
        }

        private void OnLegendarySpawned(Item item, ItemContainer container)
        {
            PrintWarning(nameof(OnLegendarySpawned) + " : " + item + " : " + container);

            if (item == null || container == null) return;

            var entityOwner = container?.entityOwner;
            if (entityOwner != null)
            {
                PrintWarning("Legendary spawned in entityOwner: " + entityOwner.ShortPrefabName);

                if (entityOwner is BasePlayer)
                {
                    PrintWarning("return because we spwaned directly into baseplayer inventory");
                    return;
                }
            }
            else
            {
                PrintWarning("legendary spawn but entity owner is null!?!?");
                return;
            }

            var player = FindLooterFromEntity(entityOwner);
            if (player == null)
            {
                PrintWarning("legendary spawn but couldn't find player!");
                return;
            }

            var leg = LegendaryItemInfo.GetLegendaryInfoFromItem(item);

            if (leg == null)
            {
                PrintError("leg null on leg spawn!!!!: " + item);
                return;
            }

            CelebrateLegendarySpawn(player, leg);
     
        }

        private void CelebrateEffectsFireworks(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || player.transform == null) return;

            var eyePos = player?.eyes?.transform?.position ?? Vector3.zero;
            var eyeForward = player?.eyes?.HeadForward() ?? Vector3.zero;

            for (int j = 0; j < Random.Range(2, 6); j++)
            {
                Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", SpreadVector2(eyePos + eyeForward * Random.Range(1.5f, 1.9f), Random.Range(1.35f, 2.2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/bonus_hit.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + Random.Range(0.5f, 1f), eyePos.z) + eyeForward * Random.Range(0.6f, 1.2f), Random.Range(0.8f, 2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/hotspot_death.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + 0.6f, eyePos.z) + eyeForward * Random.Range(0.6f, 1.2f), Random.Range(1.1f, 3f)), Vector3.zero);
            }

            var fireworkPos = player.transform.position;
            fireworkPos.y -= 1.1f;

            var ogYPos = fireworkPos.y;

            for (int i = 0; i < 7; i++)
            {

                var usePos = SpreadVector2(fireworkPos, Random.Range(0.75f, 1.75f));
                usePos.y = ogYPos;

                var firework1 = (BaseFirework)GameManager.server.CreateEntity("assets/prefabs/deployable/fireworks/volcanofirework-violet.prefab", usePos);

                firework1.Spawn();

                firework1.SetParent(player, true, true);
                firework1.transform.localPosition = new Vector3(firework1.transform.localPosition.x, firework1.transform.localPosition.y - 1.1f, firework1.transform.localPosition.z);

                PrintWarning("f1 spawn: " + fireworkPos);

                firework1.TryLightFuse();
                firework1.Begin();

                firework1.Invoke(() =>
                {
                    if (firework1 != null && !firework1.IsDestroyed) firework1.Kill();
                }, 14f);
            }

            fireworkPos.y -= 25f;
            ogYPos = fireworkPos.y;

            for (int i = 0; i < 7; i++)
            {
                var usePos = SpreadVector2(fireworkPos, Random.Range(0.75f, 1.75f));
                usePos.y = ogYPos;

                var firework2 = (MortarFirework)GameManager.server.CreateEntity("assets/prefabs/deployable/fireworks/mortarviolet.prefab", usePos);

                firework2.Spawn();

                firework2.SetParent(player, true, true);
                firework2.transform.localPosition = new Vector3(firework2.transform.localPosition.x, firework2.transform.localPosition.y - 1.1f, firework2.transform.localPosition.z);

                PrintWarning("f1 spawn: " + fireworkPos);

                firework2.TryLightFuse();
                firework2.Begin();

                firework2.Invoke(() =>
                {
                    firework2.SendFire();
                }, 2f);

                firework2.Invoke(() =>
                {
                    if (firework2 != null && !firework2.IsDestroyed) firework2.Kill();
                }, 7f);
            }

            var time = 0.33f;
            for (int i = 0; i < 10; i++)
            {
                if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead()) break;

                timer.Once(time, () =>
                {
                    if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead()) return;

                    Effect.server.Run("assets/prefabs/misc/xmas/advent_calendar/effects/open_advent.prefab", player?.eyes?.position ?? Vector3.zero);
                });

                PrintWarning("sent invoke with time: " + time);

                time += 0.33f - (i * 0.033f);
            }



            SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");
            player.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
        }

        private void CelebrateEffectsNoFireworks(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || player.transform == null) return;

            var eyePos = player?.eyes?.transform?.position ?? Vector3.zero;
            var eyeForward = player?.eyes?.HeadForward() ?? Vector3.zero;

            for (int j = 0; j < Random.Range(2, 6); j++)
            {
                Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", SpreadVector2(eyePos + eyeForward * Random.Range(1.5f, 1.9f), Random.Range(1.35f, 2.2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/bonus_hit.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + Random.Range(0.5f, 1f), eyePos.z) + eyeForward * Random.Range(0.6f, 1.2f), Random.Range(0.8f, 2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/hotspot_death.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + 0.6f, eyePos.z) + eyeForward * Random.Range(0.6f, 1.2f), Random.Range(1.1f, 3f)), Vector3.zero);
            }

            SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");
            player.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
        }


        private void CelebrateLegendarySpawn(BasePlayer player, LegendaryItemInfo legendary)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (legendary == null)
                throw new ArgumentNullException(nameof(legendary));

            var fxToRun = "assets/prefabs/food/bota bag/effects/bota-bag-cork-squeak.prefab";

            SendLocalEffect(player, "assets/prefabs/food/bota bag/effects/bota-bag-slosh.prefab");
            SendLocalEffect(player, fxToRun);
            SendLocalEffect(player, "assets/bundled/prefabs/fx/kill_notify.prefab");
            SendLocalEffect(player, "assets/prefabs/misc/xmas/advent_calendar/effects/open_advent.prefab");
            //play celebration fx for local player at least

            var modifiersString = legendary.GetModifiersString(true); //colorize = true

            var sb = Pool.Get<StringBuilder>();
            try 
            {


                var legClass = legendary.RequiredClass <= ClassType.Undefined ? null : ARPGClass.FindByType(legendary.RequiredClass);


                var rustBroadcastSb = sb.Clear().Append("<size=20><color=#FF4F78>").Append(player.displayName).Append("</color></size><size=18> <color=#FFA8E2>has looted <i><color=#F37B00>").Append(legendary.DisplayName).Append("</color>");

                if (legClass != null)
                {
                    rustBroadcastSb.Append(" <color=").Append(legClass.SecondaryColor).Append(">(<color=").Append(legClass.PrimaryColor).Append(">").Append(legClass.DisplayName).Append("</color>)</color>");
                    if (ERA_CORRUPTION)
                        rustBroadcastSb.Append(" (<color=red>CORRUPTION</color>: NO CLASS REQUIREMENT)");
                }

                rustBroadcastSb.Append("!</color></size>");

                if (!string.IsNullOrWhiteSpace(modifiersString)) 
                    rustBroadcastSb.Append("\n<size=17><color=#ff0065>BONUSES</color><color=#33ff0a>:</color></size>\n").Append(modifiersString);

                if (!string.IsNullOrWhiteSpace(legendary.BonusDescription)) 
                    rustBroadcastSb.Append("\n").Append(legendary.BonusDescription);

                rustBroadcastSb.Append("</i>");

                var rustBroadcastMsg = rustBroadcastSb.ToString();

                SimpleBroadcast(rustBroadcastMsg, CHAT_STEAM_ID);

                var discordSb = sb.Clear().Append("**").Append(player.displayName).Append("** looted ***").Append(legendary.DisplayName).Append("!***\n");
                if (!string.IsNullOrWhiteSpace(modifiersString)) 
                    discordSb.Append("BONUSES:\n**").Append(RemoveTags(modifiersString)).Append("**");

                if (!string.IsNullOrWhiteSpace(legendary.BonusDescription)) 
                    discordSb.Append("\n***").Append(legendary.BonusDescription).Append("***");

                DiscordAPI2?.Call("SendChatMessageByName", discordSb.ToString(), "PRISM | BLUE", PRISM_BLUE_ICON_DISCORD); //must supply a URL image link at end, not ID

                //needs sb
                SendLuckNotifications(player, sb.Clear().Append("<color=#F37B00>").Append(legendary.DisplayName).Append("</color> | Bonuses:").Append(!string.IsNullOrWhiteSpace(modifiersString) ? "\n" : "").Append(modifiersString).Append(!string.IsNullOrWhiteSpace(legendary.BonusDescription) ? "\n" : "").Append(legendary.BonusDescription).ToString());

            }
            finally { Pool.Free(ref sb); }

     
        }

        private void CelebrateLegendarySpawnFromItem(BasePlayer player, Item item)
        {
            if (player == null || item == null) return;

            CelebrateLegendarySpawn(player, LegendaryItemInfo.GetLegendaryInfoFromItem(item));

        }

        [ChatCommand("giveleg")]
        private void cmdGiveLeg(BasePlayer player, string command, string[] args)
        {

            var searchTerm = string.Join(" ", args);

            PrintWarning("args length: " + args.Length + ", " + nameof(searchTerm) + ": " + searchTerm);

            var leg = FindLegendaryItemInfoByNameOrID(searchTerm);

            var target = player;

            if (leg == null)
            {

                PrintWarning("leg null, try find target by name");

                target = FindPlayerByPartialName(args[0], true);

                if (target == null)
                {
                    SendReply(player, "No player found by this name?!: " + args[0]);
                    return;
                }
                else
                {
                    PrintWarning("target not null on searchTerm/args 0, re-search leg");

                    searchTerm = string.Join(" ", args, 1, args.Length - 1);
                    leg = FindLegendaryItemInfoByNameOrID(searchTerm);

                }

            }

            if (leg == null)
            {

                SendReply(player, "Could not find leg by search: " + searchTerm);
                return;
            }

         

            var item = leg.SpawnItem();

            player.GiveItem(item);

            SendReply(player, "Spawned " + leg.DisplayName + "/" + leg.ItemID + " (" + leg.SkinID + "), ran player.GiveItem()");

        }

        [ChatCommand("legtest")]
        private void cmdLegendaryTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length > 0 && args[0].Equals("sr"))
            {
                SendReply(player, "forced guaranteed spawn rate/super high pity");
                _consecutiveLegendaryChancesMissed[player.UserIDString] = 9999;
                return;
            }

            for (int i = 0; i < _legendaryData.Infos.Count; i++)
            {
                var leg = _legendaryData.Infos[i];

                var item = leg.SpawnItem();

                player.GiveItem(item);
                
            }

            SendReply(player, "spawned legendaries into inv or dropped");
        }

        private bool IsShady(ulong userId)
        {
            return userId == 76561198028248023;
        }

        private bool IsShadyS(string userId)
        {
            return userId == "76561198028248023";
        }

        private bool IsPlayerShady(BasePlayer player)
        {
            return IsShady(player?.userID ?? 0);
        }

        private bool IsInAttackState(HumanNPC npc)
        {
            var curState = npc?.Brain?.CurrentState?.StateType ?? AIState.None;
            return curState switch
            {
                AIState.Attack or AIState.Combat or AIState.CombatStationary or AIState.Chase => true,
                _ => false,
            };
        }

        private bool HasRequiredItemsForTransmutation(ItemContainer inventory)
        {
            if (inventory?.itemList == null) return false;

            var skullCount = 0;

            for (int i = 0; i < inventory.itemList.Count; i++)
            {
                var item = inventory.itemList[i];
                if (item.info.itemid == WOLF_SKULL_ID && string.IsNullOrEmpty(item?.name)) skullCount += item.amount;
            }

            return inventory.GetAmount(PURE_ORE_TEA_ID, false) >= 3 && inventory.GetAmount(PURE_WOOD_TEA_ID, false) >= 3 && inventory.GetAmount(PURE_SCRAP_TEA_ID, false) >= 3 && skullCount >= 5;
        }


        private void cmdPurchaseToken(IPlayer iPlayer, string command, string[] args) //change this to open something like skin box. you have to put all your items in and click 'transmute' or whatever. button only appears correctly if you meet the needed items
        {
            if (iPlayer?.Object == null || args.Length < 1 || args[0] != "buy_now_bcmd") return;

            var player = iPlayer?.Object as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead()) return;

            if (player.IsAdmin && args[0].Equals("spawn"))
            {
                player.GiveItem(CreateInactiveTokenOfAbsolution(args.Length > 1 ? int.Parse(args[1]) : 1));
                return;
            }

            var initialPosition = player?.transform?.position ?? Vector3.zero;

            var box = (StorageContainer)GameManager.server.CreateEntity("assets/prefabs/deployable/woodenbox/woodbox_deployed.prefab", new Vector3(initialPosition.x, -40f, initialPosition.z));
            if (box == null)
            {
                PrintWarning("box is null!!!");
                return;
            }

            box.OwnerID = 528491;
            box.enableSaving = false;

            box.Spawn();
            box.EnableSaving(false);

            SendLocalEffect(player, "assets/bundled/prefabs/fx/kill_notify.prefab");

            var isChatCmd = args.Length > 1 && args[1].Equals("debug", StringComparison.OrdinalIgnoreCase);

            var action = new Action(() =>
            {
                StartLootingContainer(player, box, false);

                if (player.inventory.loot.entitySource == box)
                {


                    PrintWarning("found box after start loot!");
                    TransmuteGUI(player);

                    ShowTextAboveInventory(player, "<color=#ec0202>TOKEN OF ABSOLUTION</color>:\nA <color=#ec0202>Token of Absolution</color> may be used to reset your class. It will also reset your main skill to Level 1.\nPlace 3 Pure Wood, Ore, & Scrap teas along with 5 wolf skulls into the container, then click 'Transmute' to create a Token of Absolution.");

                }
            });

            if (isChatCmd)
                player.Invoke(action, 0.125f);
            else action.Invoke();
        }

        private readonly Dictionary<BasePlayer, LegendaryItemInfo> _activeEditLegendary = new();

        private void cmdCreateLegendary(IPlayer iPlayer, string command, string[] args)
        {

            var player = iPlayer?.Object as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead())
            {
                SendReply(player, "This command must be run in-game.");
                return;
            }

            var heldItem = player?.GetActiveItem();

            if (heldItem == null)
            {
                SendReply(player, "Must be holding desired legendary item with desired skin.");
                return;
            }

            //todo: prevent creating the same legendary (same itemid / skin)

            var newLeg = new LegendaryItemInfo(heldItem.info.itemid, heldItem.skin);

            _legendaryData.Infos.Add(newLeg);

            _activeEditLegendary[player] = newLeg;

            SendReply(player, "Set new legendary item, which is now being edited. Use /legedit to continue or /legdel to remove legendary.");

        }

        private void cmdDeleteLegendary(IPlayer iPlayer, string command, string[] args)
        {

            var player = iPlayer?.Object as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead())
            {
                SendReply(player, "This command must be run in-game.");
                return;
            }

            var heldItem = player?.GetActiveItem();

            if (!_activeEditLegendary.TryGetValue(player, out LegendaryItemInfo legendary) && heldItem == null)
            {
                if (heldItem == null)
                {
                    SendReply(player, "Must be holding desired legendary item with desired skin or select it via /legedit.");
                    return;
                }
                else legendary = LegendaryItemInfo.GetLegendaryInfoFromItem(heldItem);
            }



            if (legendary == null)
            {
                SendReply(player, "No legendary found!");
                return;
            }

            _activeEditLegendary.Remove(player);
            _legendaryData.Infos.Remove(legendary);

            SendReply(player, "Deleted legendary.");
        }

        private void cmdEditLegendary(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin)
                return;

            var player = iPlayer?.Object as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead())
            {
                SendReply(player, "This command must be run in-game.");
                return;
            }

            if (args.Length > 0 && args[0].Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                _activeEditLegendary.Remove(player);
                SendReply(player, "Stopped editing");
                return;
            }

            //commit test (ignore this comment)

            if (args.Length > 0 && args[0].Equals("delete", StringComparison.OrdinalIgnoreCase) && _activeEditLegendary.TryGetValue(player, out var delLeg))
            {
                var didRem1 = _legendaryData.Infos.Remove(delLeg);

                var didRem2 = _activeEditLegendary.Remove(player);
                SendReply(player, "Deleted legendary?: " + didRem1 + ", " + didRem2);

                return;
            }

            var infos = _legendaryData?.Infos;

            if (infos == null)
            {
                SendReply(player, "Legendary infos is null?!?!");
                return;
            }

            var gotFromDic = false;

            if (!_activeEditLegendary.TryGetValue(player, out LegendaryItemInfo leg) || leg == null)
            {
                if (args.Length < 1)
                {
                    SendReply(player, "Select legendary to edit via /" + command + " <legendary item name or list index> or create new one with /legcreate");
                    return;
                }
                else SendReply(player, "No edit selected... finding by args");

            }
            else gotFromDic = true;

            var arg0 = args.Length > 0 ? args[0] : string.Empty;

            if (args.Length > 0 && infos.Count > 0)
            {


                if (int.TryParse(arg0, out int index))
                {
                    if (index > (infos.Count - 1) || index < 0)
                    {
                        SendReply(player, "Bad index specified. infos.Count: " + infos.Count);
                        return;
                    }

                    gotFromDic = false;

                    leg = _legendaryData.Infos[index];

                    if (leg == null)
                        SendReply(player, "Found legendary item by index, but is null?!: " + arg0);

                }
                else if (leg == null)
                {
                    for (int i = 0; i < _legendaryData.Infos.Count; i++)
                    {
                        var legInfo = _legendaryData.Infos[i];

                        if (legInfo.DisplayName.Equals(arg0, StringComparison.OrdinalIgnoreCase))
                        {
                            leg = legInfo;

                            SendReply(player, "Found legendary item matching name " + legInfo.DisplayName);

                            break;
                        }

                    }
                }
            }


            if (leg == null)
            {
                SendReply(player, "Failed to find legendary item. Please run command again and find legendary you want to edit or create a new one.");
                return;
            }

       

            if (!gotFromDic)
            {
                _activeEditLegendary[player] = leg;
                SendReply(player, "Now editing legendary item:\n" + leg.DisplayName + " (" + leg.ItemID + ", " + leg.SkinID + ")" + "\nrun again to edit or view properties.");
                return;
            }
               

            var legProperties = leg.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (args.Length < 1)
            {
                var propSb = Pool.Get<StringBuilder>();

                try
                {
                    propSb.Clear().Append("Properties:").Append(Environment.NewLine);


                    for (int i = 0; i < legProperties.Length; i++)
                    {
                        var legProp = legProperties[i];

                        var isReadOnly = !legProp.CanWrite;
                        var isPrivate = isReadOnly || (legProp.GetSetMethod(true)?.IsPrivate ?? false);

                        var val = legProp.GetValue(leg);

                        //todo: more appealing # colors
                        propSb.Append("<color=orange>").Append(legProp.Name).Append("</color> (").Append(legProp.PropertyType).Replace("System.", string.Empty).Append("): <color=yellow>").Append(val).Append("</color>").Append(isReadOnly ? " (Read Only)" : string.Empty).Append(isPrivate ? " (Private Set)" : string.Empty).Append(Environment.NewLine);
                    }

                    propSb.Length--;

                    SendReply(player, propSb.ToString());

                }
                finally { Pool.Free(ref propSb); }

                return;
            }

            PropertyInfo prop = null;

            for (int i = 0; i < legProperties.Length; i++)
            {
                var legProp = legProperties[i];

                if (legProp.Name.IndexOf(arg0, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (prop != null)
                    {
                        SendReply(player, "Found multiple matches for: " + arg0);
                        return;
                    }

                    prop = legProp;
                }

            }
            
            if (prop == null)
            {
                SendReply(player, "No property found by: " + arg0);
                return;
            }

            var propertyValue = prop.GetValue(leg);

            var isDict = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>);

            if (isDict)
            {
                //optimize below?
                /*/
                _ = prop.PropertyType.GetGenericArguments()[0];
                _ = prop.PropertyType.GetGenericArguments()[1];
                /*/
                var sb = Pool.Get<StringBuilder>();

                try 
                {

                    var itemProp = prop.PropertyType.GetProperty("Item");
                    var keys = (IEnumerable<object>)prop.PropertyType.GetProperty("Keys").GetValue(prop.GetValue(leg));

                    sb.Clear().Append(prop.Name).Append(" contents:").Append(Environment.NewLine);

                    foreach (var key in keys)
                    {
                        var value = itemProp.GetValue(propertyValue, new[] { key });

                        sb.Append(key).Append(" (").Append(key.GetType()).Append("): ").Append(value).Append(" (").Append(value.GetType()).Append(")").Append(Environment.NewLine);
                    }
                    

                    if (sb.Length > 1)
                        sb.Length--;

                    SendReply(player, sb.ToString());
                    PrintWarning(sb.ToString());
                   

                    //  var values = prop.PropertyType.GetProperty("Values").GetValue(prop.GetValue(leg));


                }
                finally { Pool.Free(ref sb); }

            }

            if (args.Length < 2)
            {
                SendReply(player, "No edit arg specified. Property is: " + prop.Name + " value: " + prop.GetValue(leg));
                return;
            }

            if (isDict)
            {
                //handle
                return;
            }


            var arg1 = args[1];
            object convertedType;
            try
            {
                if (int.TryParse(arg1, out int argInt) && prop.PropertyType.IsEnum)
                    convertedType = Enum.ToObject(prop.PropertyType, argInt);
                else 
                    convertedType = Convert.ChangeType(arg1, prop.PropertyType);
            }
            catch (Exception ex)
            {
                SendReply(player, "Exception: " + ex.Message + " (" + ex.GetType() + ")");
                SendReply(player, "Couldn't convert " + arg1 + " to type: " + prop.PropertyType);
                throw ex;
            }

            prop.SetValue(leg, convertedType);

            SendReply(player, "Set " + prop.Name + " to: " + convertedType);

        }

        private void cmdLegendaryInfo(IPlayer iPlayer, string command, string[] args)
        {
            if (iPlayer?.Object == null) return;

            var player = iPlayer?.Object as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead()) return;

            if (args.Length < 1) _curPage[player.userID] = 0;

            if (!_curPage.TryGetValue(player.userID, out int curPage)) _curPage[player.userID] = 0;

            LegendaryGUI(player, curPage, args.Length > 0 ? string.Join(" ", args) : string.Empty);

        }

        private void cmdBackpacks(IPlayer iPlayer, string command, string[] args)
        {
            SendReply(iPlayer, "--<size=30>BACKPACKS</size>--\n\n<color=orange>Backpacks</color> work a little bit differently here on PRISM.\nOn PRISM, you have to loot and find a <color=orange>backpack</color>. Once you've found one, you can equip it on your person, and then open it and use it to store loot.\nIt's a real item which you wear, just like a shirt! This means you drop it on death, too. Other players can steal your backpack, and all its loot!\n\nGet out there and find a backpack, and keep it safe (it's valuable)!");
        }


        private void SendGeneralClassText(IPlayer player)
        {
            if (player == null)
                return;

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                sb.Clear().Append("<size=26><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color></size>\n-<size=24>ARPG CLASSES</size>-\n\n<color=orange>Classes come with their own perks and abilities, and make you valuable to your group <color=yellow>(or provide unique solo styles)</color> in special ways</color>!\n<color=#00b386>Available classes</color>:\n");

                var userClassType = GetUserClassType(player.Id);

                for (int i = 0; i < Enum.GetValues(typeof(ClassType)).Length; i++)
                {
                    var classType = (ClassType)i;
                    if (classType == ClassType.Undefined) continue; //no undefined

                    var rpgClass = ARPGClass.FindByType(classType);

                    var stringClass = classType.ToString();
                    //this sb append below needs some fixing
                    sb.Append("<color=yellow>-</color><color=").Append(rpgClass.PrimaryColor).Append(">").Append(stringClass).Append("</color>").Append(classType == ClassType.Loner || classType == ClassType.Nomad ? " <color=" + rpgClass.SecondaryColor + ">(SOLO)</color>" : string.Empty).Append("</color>").Append(classType == userClassType ? "<size=20> <color=" + rpgClass.PrimaryColor + ">*</color></size>" : string.Empty).Append(Environment.NewLine);
                }

                sb.Length -= 1;

                //sb.Append(Environment.NewLine).Append("<color=yellow>View more info about these classes and select one by typing <color=#00b386><size=20>/class <color=orange>GUI</color>.</size></color>");

                SendReply(player, sb.ToString());
            }
            finally { Pool.Free(ref sb); }
           
        }

        //make this command less of a mess?
        private void cmdClass(IPlayer iPlayer, string command, string[] args)
        {
            if (iPlayer == null) return;


            var basePlayer = iPlayer?.Object as BasePlayer;

            var arg0 = args.Length > 0 ? args[0] : string.Empty;

            var findClassByArg0 = string.IsNullOrWhiteSpace(arg0) ? null : ARPGClass.FindByDisplayName(arg0) ?? ARPGClass.FindByTypeName(arg0);

            StringBuilder sb = null;

            if (findClassByArg0 != null && basePlayer != null)
            {
                //button click fx:
                SendLocalEffect(basePlayer, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");

                sb = Pool.Get<StringBuilder>();
                try
                {
                    ClassGUILegacy(basePlayer, sb.Clear().Append(findClassByArg0.DisplayName).Append("\n").Append(findClassByArg0.Description).ToString()); //environment.newline can't be used because it leaves a trailing (visible in-game) \r
                }
                finally { Pool.Free(ref sb); }


                return;
            }

            var targetPlayer = !string.IsNullOrEmpty(arg0) ? FindConnectedPlayer(arg0, true) : null;

            if (targetPlayer != null)
            {
                var targetClass = GetUserClass(targetPlayer.Id);

                if (targetClass == null)
                {
                    SendReply(iPlayer, targetPlayer.Name + " does not have a class selectd!");
                    return;
                }

                SendReply(iPlayer, targetPlayer.Name + " is a <color=" + targetClass.PrimaryColor + ">" + targetClass.DisplayName + "</color>.");
                return;
            }


            sb = Pool.Get<StringBuilder>();
            try 
            {
                

                if (arg0.Equals("stop"))
                {
                    var msgToReopenLong = PRISM_RAINBOW_TXT + ": To reopen the class selection menu, type <color=#FF0030>/</color><color=#FE950A>" + command + "</color>.";

                    var msgToReopenShort = PRISM_RAINBOW_TXT + ": To reopen, type <color=#FF0030>/</color><color=#FE950A>" + command + "</color>.";

                    SendReply(iPlayer, msgToReopenLong);

                    ShowToast(basePlayer, msgToReopenShort);

                    SendLocalEffect(basePlayer, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");

                    CuiHelper.DestroyUi(basePlayer, "ClassGUINew");
                    return;
                }
                else if (arg0.Equals("new"))
                {

                    var desiredClass = GetUserClassType(iPlayer.Id);

                    if (desiredClass == ClassType.Undefined)
                        desiredClass = ClassType.Nomad;

                    if (args.Length > 1)
                    {

                        if (!_classGuiLastClass.TryGetValue(iPlayer.Id, out ClassType lastClass))
                            lastClass = ClassType.Nomad;

                        if (args[1].Equals("next", StringComparison.OrdinalIgnoreCase))
                        {
                            desiredClass = (ClassType)((int)lastClass + 1);
                            if ((int)desiredClass > (ClassEnumCount - 1)) desiredClass = ClassType.Nomad;
                        }

                        if (args[1].Equals("prev", StringComparison.OrdinalIgnoreCase))
                        {
                            desiredClass = (ClassType)((int)lastClass - 1);
                            if ((int)desiredClass < 1) desiredClass = (ClassType)(ClassEnumCount - 1);// ClassType.Mercenary;
                        }
                    }

                    SendLocalEffect(basePlayer, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");

                    ClassGUINew(basePlayer, desiredClass);
                    return;
                }

                if (arg0.Equals("title", StringComparison.OrdinalIgnoreCase) || arg0.Equals("tag", StringComparison.OrdinalIgnoreCase))
                {
                    if (!permission.UserHasPermission(iPlayer.Id, DISABLE_CLASS_NAME_IN_CHAT)) permission.GrantUserPermission(iPlayer.Id, DISABLE_CLASS_NAME_IN_CHAT, null);
                    else permission.RevokeUserPermission(iPlayer.Id, DISABLE_CLASS_NAME_IN_CHAT);

                    SendReply(iPlayer, "Class chat title status: " + !permission.UserHasPermission(iPlayer.Id, DISABLE_CLASS_NAME_IN_CHAT));
                    return;
                }

                BasePlayer player;

                if (arg0.Equals("reset_") && iPlayer.IsAdmin)
                {
                    PrintWarning("arg0 is reseT_ and player is admin");
                    //DEBUG ONLY

                    var target = args.Length < 2 ? iPlayer : FindConnectedPlayer(args[1], true);

                    PrintWarning("target null?: " + (target == null));

                    if (target == null)
                    {
                        SendReply(iPlayer, "No target found by arg: " + args[1]);
                        return;
                    }

                    PrintWarning("target not null: " + target);



                    ResetUserClass(target.Id);

                    var info = GetLuckInfo(target.Id);
                    info.GracePeriodStartTime = -1;

                    SendReply(iPlayer, "reset class to none for: " + target.Name);

                    if (target.Id != iPlayer.Id && target.IsConnected) SendReply(target, "An admin has reset your class to none");

                    PrintWarning("finished reset_ code");

                    return;
                }


                player = FindPlayerByIdS(iPlayer.Id);

                if (player == null)
                {
                    PrintError("Somehow player is null on cmdClass!!!!");
                    return;
                }


                var luckInfo = GetLuckInfo(player);
                if (luckInfo == null)
                {
                    PrintError("luck info somehow null on cmdClass!!!!");
                    return;
                }

                var userClass = GetUserClass(player.UserIDString);
                var userClassType = GetUserClassType(player.UserIDString);


                if (string.IsNullOrEmpty(arg0))
                {
                    var desiredClass = GetUserClassType(iPlayer.Id);

                    if (desiredClass == ClassType.Undefined)
                        desiredClass = ClassType.Nomad;

                    ClassGUINew(player, desiredClass);

                    SendGeneralClassText(iPlayer);
                    return;
                }

                //consider revisiting set GUI
                if (arg0.Equals("setgui"))
                {
                    ClassSetGUI(player, args.Length > 1 ? args[1] : "<color=green>Lumberjack Hoodie</color>\n<color=red>Lumberjack Pants</color>\n<color=green>Lumberjack Boots</color>");
                    return;
                }

                if (arg0.Equals("setguistop"))
                {
                    CuiHelper.DestroyUi(player, "GUISets");
                    return;
                }





                if (arg0.Equals("gui", StringComparison.OrdinalIgnoreCase) || arg0.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    ClassGUINew(player);
                    SendGeneralClassText(iPlayer);
                    return;
                }

                if (arg0.Equals("gui_legacy", StringComparison.OrdinalIgnoreCase))
                {
                    ClassGUILegacy(player);
                    return;
                }

            


           

                if (!arg0.Equals("select", StringComparison.OrdinalIgnoreCase)) return;


                if (userClassType != ClassType.Undefined && luckInfo.GracePeriodStartTime != -1) //fix if nesting
                {
                    PrintWarning("not undefined!");
                    if (luckInfo.GracePeriodStartTime == 0f || (TimeSpan.FromTicks(DateTime.UtcNow.Ticks) - TimeSpan.FromTicks(luckInfo.GracePeriodStartTime)).TotalSeconds >= 2700)
                    {
                        //needs sb
                        SendReply(player, "<color=" + userClass.SecondaryColor + ">Your class is <color=" + userClass.PrimaryColor + ">" + userClassType + "</color>! This can only be chosen once per wipe, unless you use a Token of Absolution</color>.");
                        SendLocalEffect(player, "assets/prefabs/missions/effects/mission_failed.prefab");
                        return;
                    }
                    else PrintWarning("either couldn't get out for grace period time or >= 2700, now ticks: " + DateTime.UtcNow.Ticks + ", grace start ticks: " + luckInfo.GracePeriodStartTime);
                }

                var arg1 = args[1];

                var findClassByArg = ARPGClass.FindByDisplayName(arg1) ?? ARPGClass.FindByTypeName(arg1);
                if (findClassByArg == null)
                {
                    SendReply(player, "Could not find a class by name: " + arg1);
                    return;
                }

                if (findClassByArg.Type == userClassType)
                {
                    SendReply(player, "You have already selected this class!");
                    return;
                }

                if (findClassByArg.Type == ClassType.Nomad)
                {
                    //GetBlockCountForUser
                    var blockCount = BuildLimiter?.Call<int>("GetBlockCountForUser", player.userID, GLOBAL_NOMAD_BUILD_LIMIT);

                    if (blockCount > GLOBAL_NOMAD_BUILD_LIMIT)
                    {
                        //needs SB
                        SendReply(player, "You cannot select the " + findClassByArg.DisplayName + " class because you are over its global build limit (" + GLOBAL_NOMAD_BUILD_LIMIT.ToString("N0") + " blocks!)");
                        return;
                    }

                    foreach (var kvp in BuildingManager.server.buildingDictionary)
                    {
                        var building = kvp.Value;

                        if (GetMajorityOwnerFromBuilding(building) != player.userID)
                            continue; //ensure this is a building the player in question owns.

                        if (building.buildingBlocks.Count > INDIVIDUAL_NOMAD_BUILD_LIMIT)
                        {
                            SendReply(player, "You cannot select the " + findClassByArg.DisplayName + " class because you are over its individual building block limit (one of your buildings has too many blocks, limit is " + INDIVIDUAL_NOMAD_BUILD_LIMIT.ToString("N0") + " blocks!)");
                            return;
                        }

                    }

                }


                if (luckInfo.GracePeriodStartTime == -1f) luckInfo.GracePeriodStartTime = DateTime.UtcNow.Ticks;
                else
                {
                    //it's permanent now!

                    luckInfo.GracePeriodStartTime = 0;

                    ResetUserClass(player.UserIDString);
                }

                SetUserClass(player.UserIDString, findClassByArg.Type);

                IOnUserSelectedClass(player, findClassByArg);

                var classMsgSb = sb.Clear().Append("You have selected the <color=").Append(findClassByArg.PrimaryColor).Append(">").Append(findClassByArg.DisplayName).Append("</color> class!");
                if (luckInfo.GracePeriodStartTime != 0) classMsgSb.Append(" You will have <i><color=" + findClassByArg.SecondaryColor + ">45 minutes</color></i> to change your class before it is permanent! If you want to change, type <color=#FF0030>/class</color> again.");

                var classMsg = classMsgSb.ToString();

                SendReply(player, classMsg);

                var toastMsg = "<color=" + findClassByArg.SecondaryColor + ">Selected</color> <color=" + findClassByArg.PrimaryColor + "><i>" + findClassByArg.DisplayName + "</i></color>. You will have <i><color=" + findClassByArg.SecondaryColor + ">45</color></i> minutes to change it again (<color=" + findClassByArg.PrimaryColor + ">/class</color>).";
                ShowToast(player, toastMsg, 1);

                player.Invoke(() =>
                {
                    if (player?.IsConnected ?? false) ShowToast(player, toastMsg, 1);
                }, 1.5f); //make it sort of "last longer"

                StopClassGUI(player);

                SendLocalEffect(player, "assets/prefabs/missions/effects/mission_accept.prefab");


            }
            finally { Pool.Free(ref sb); }
        }

        private object OnTPRGetLimit(BasePlayer player, int limit)
        {
            if (player == null || limit < 1) return null;

            if (GetUserClassType(player.UserIDString) == ClassType.Loner)
                return (int)Mathf.Clamp(limit * 0.5f, 1, limit);
            

            return null;
        }

        private object OnHomeTPGetLimit(BasePlayer player, int limit)
        {
            if (player == null || limit < 1) return null;

            if (GetUserClassType(player.UserIDString) == ClassType.Loner)
            {
                PrintWarning("user is loner class on " + nameof(OnHomeTPGetLimit) + ", limit is * 1.25: " + (limit * 1.25));
                return (int)Mathf.Clamp(limit * 1.25f, 1, limit);
            }

            return null;
        }

        //in the style of an interal hook ("I")
        private void IOnUserSelectedClass(BasePlayer player, ARPGClass userClass)
        {
            if (player == null || userClass == null) return;

            Interface.Oxide.CallHook("OnUserSelectedClass", player, (int)userClass.Type);

            BroadcastClassSelection(player, userClass);

            var logSb = Pool.Get<StringBuilder>();
            try
            {
                logSb.Clear();

                if (userClass.Type == ClassType.Nomad || userClass.Type == ClassType.Loner)
                {

                    if (player?.Team != null)
                    {
                        logSb.Append(player.displayName).Append(" (").Append(player.UserIDString).Append(") has been forced to leave their team because they selected class ").Append(userClass.DisplayName).Append(" Team: ").Append(player.Team.teamID);
                        ClearTeam(player);
                    }
                    else logSb.Append(player.displayName).Append(" (").Append(player.UserIDString).Append(") did not have a team on selecting class ").Append(userClass.DisplayName);
                }

                logSb.Append(player.displayName).Append(" (").Append(player.UserIDString).Append(") selected class: ").Append(userClass.DisplayName);

            }
            finally
            {
                try
                {
                    if (logSb?.Length > 0) LogToFile(CLASS_LOG_FILE_NAME, logSb.ToString(), this);
                    else PrintWarning("logsb length < 1 for " + nameof(IOnUserSelectedClass));
                }
                finally { Pool.Free(ref logSb); }
            }

            var userId = player?.UserIDString;

            for (int i = 0; i < Enum.GetValues(typeof(ClassType)).Length; i++)
            {
                try
                {
                    var classType = (ClassType)i;
                    if (classType == ClassType.Undefined || classType == userClass.Type) continue;

                    DiscordAPI2.Call("RemoveRoleWithColor", userId, classType.ToString(), ARPGClass.FindByType(classType).PrimaryColor);
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
            }

            try { DiscordAPI2?.Call("GiveRoleWithColor", player.UserIDString, userClass.DisplayName, userClass.PrimaryColor); }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try { UnequipItemsMissingRequirements(player); }
            catch (Exception ex) { PrintError(ex.ToString()); }
            
        }

        private object OnTeamChatGetPlayerNameColor(BasePlayer player, string color)
        {
            if (player == null) return null;

            var userClass = GetUserClass(player.UserIDString);
            if (userClass == null || userClass.Type == ClassType.Undefined) return null;

            return userClass.PrimaryColor;
        }

        private object OnBetterChatGetTitle(string userId, string chatTitle)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));


            var userClass = GetUserClass(userId);
            if (userClass == null || userClass.Type == ClassType.Undefined || userClass.Hide || permission.UserHasPermission(userId, DISABLE_CLASS_NAME_IN_CHAT)) return null;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                return sb.Clear().Append(chatTitle).Append(!string.IsNullOrWhiteSpace(chatTitle) ? " " : string.Empty).Append("[<color=").Append(userClass.PrimaryColor).Append(">").Append(userClass.DisplayName).Append("</color>]").ToString();
            }
            finally { Pool.Free(ref sb); }
        }

        private const int GLOBAL_NOMAD_BUILD_LIMIT = 576;
        private const int INDIVIDUAL_NOMAD_BUILD_LIMIT = 96; //move me! pls.

        private void OnGetGlobalBuildLimit(BasePlayer player, string clanTag, ref int limit)
        {
            if (player == null) return;

            if (GetUserClassType(player.UserIDString) == ClassType.Nomad)
                limit = GLOBAL_NOMAD_BUILD_LIMIT;
        }

        private void OnGetBuildLimit(BasePlayer player, BuildingBlock block, ref int limit)
        {
            if (player == null) return;

            if (GetUserClassType(player.UserIDString) == ClassType.Nomad)
                limit = INDIVIDUAL_NOMAD_BUILD_LIMIT;
        }

        private object OnGetCatchupMultiplier(string userId, string skillName, double catchup, long level, List<long> topLevels)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(skillName)) return null;


            var userClass = GetUserClassType(userId);

            var useSkillName = skillName.Length > 1 ? skillName[0].ToString() : skillName;
            if (useSkillName.Equals("W", StringComparison.OrdinalIgnoreCase)) useSkillName = "WC"; //hacky workaround lol

            if (SkillMatchesClass(useSkillName, userClass)) return null;

            var newCatchup = catchup * 0.33;
            if (newCatchup < 0) newCatchup = 0;

            return newCatchup;

        }

        private object OnGetLevelCap(string skill, int cap, string userId = "")
        {
            if (string.IsNullOrEmpty(skill) || string.IsNullOrEmpty(userId) || GetUserClassType(userId) != ClassType.Undefined) return null; //user has class, no need to force cap to 25

            return 25; //return 25 if user has no class
        }

        private object OnGetLuckStat(ulong userId, StatInfo stat, int val)
        {
            var newVal = val;

            var ply = FindPlayerByID(userId);

            if (ply == null || !ply.IsConnected) return null;

            if (stat.ShortName.Equals("Thorns", StringComparison.OrdinalIgnoreCase) && _secondWindThornsBonusActive.Contains(ply.userID))
                newVal += 4;

            if (_currentSkillRotation.TryGetValue(userId.ToString(), out string skillRot) && stat.ShortName.Equals(skillRot, StringComparison.OrdinalIgnoreCase) || stat.DisplayName.Equals(skillRot, StringComparison.OrdinalIgnoreCase))
                newVal += 3;

            var statName = stat?.ShortName;

            var wearItems = ply?.inventory?.containerWear?.itemList;

            //check through all wearitems for legendary, if we find a legendary, add whatever stat modifiers from its dictionary

            var hasUdders = false;

            for (int i = 0; i < wearItems.Count; i++)
            {
                var item = wearItems[i];

                var legendary = LegendaryItemInfo.GetLegendaryInfoFromItem(item);
                if (legendary == null) continue;

                if (!hasUdders && legendary.SkinID == UDDER_GLOVES_SKIN_ID)
                    hasUdders = true;

                if (legendary.TryGetModifierBonusesByName(statName, out int modifier))
                    newVal += modifier;
            }

            var activeItemIsLegendary = false;

            var activeItem = GetActiveItem(ply);
            if (activeItem != null)
            {
                var leg = LegendaryItemInfo.GetLegendaryInfoFromItem(activeItem);

                activeItemIsLegendary = leg != null;

                if (leg != null && leg.TryGetModifierBonusesByName(statName, out int modifier))
                    newVal += modifier;

                var activeItemContents = activeItem?.contents?.itemList;

                if (activeItemContents != null)
                {

                    //surely the compiler optimizes this and doesn't try to run this code if count == 0? can the compiler know? how does this work? is it a performance issue? nobody knows - i just wanted to avoid && count > 0
                    for (int i = 0; i < activeItemContents.Count; i++)
                    {
                        var contentItem = activeItemContents[i];

                        var contentLeg = LegendaryItemInfo.GetLegendaryInfoFromItem(contentItem);

                        if (contentLeg != null && contentLeg.TryGetModifierBonusesByName(statName, out modifier))
                            newVal += modifier;

                    }

                }

            }
          
            if (!activeItemIsLegendary)
            {
                var beltItems = ply?.inventory?.containerBelt?.itemList;

                LegendaryItemInfo legInfo;

                for (int i = 0; i < beltItems.Count; i++)
                {
                    var item = beltItems[i];

                    if (item.info.category != ItemCategory.Attire && item.info.category != ItemCategory.Weapon && (legInfo = LegendaryItemInfo.GetLegendaryInfoFromItem(item)) != null)
                    {
                        if (legInfo.TryGetModifierBonusesByName(statName, out int modifier))
                            newVal += modifier;

                        break;
                    }
                    

                }

               
            }

            return (hasUdders && !stat.ShortName.Equals("chanceLvl", StringComparison.OrdinalIgnoreCase)) && newVal > 1 ? 1 : newVal != val ? newVal : null;
        }

        private object OnTeamCreate(BasePlayer player)
        {

            var userClass = GetUserClassType(player.UserIDString);

            if (userClass == ClassType.Nomad)
            {
                SendLuckNotifications(player, "<color=#B7AF8B>Nomad</color>: <color=#FF884C>Nomads, being known for not settling down in any one spot, are unable to create teams</color>.");
                return true;
            }
            else if (userClass == ClassType.Loner)
            {
                SendLuckNotifications(player, "<color=#B7AF8B>Loner</color>: <color=#FF884C>Loners, generally sticking to themselves, are unable to create teams</color>.");
                return true;
            }

            return null;
        }

        private object OnTeamInvite(BasePlayer inviter, BasePlayer target)
        {
            var userClass = GetUserClassType(target.UserIDString);

            if (userClass == ClassType.Nomad)
            {
                //needs sb
                SendReply(inviter, "<color=#FF884C>" + target.displayName + " is a <color=#B7AF8B>Nomad</color>, and cannot be invited to any team</color>.");
                return true;
            }
            else if (userClass == ClassType.Loner)
            {
                //needs sb
                SendReply(inviter, "<color=#FF884C>" + target.displayName + " is a <color=#B7AF8B>Loner</color>, and cannot be invited to any team</color>.");
                return true;
            }

            return null;
        }

        private object OnClanCreate(IPlayer player, string tag, string desc)
        {

            return OnTeamCreate(FindPlayerByIdS(player.Id));
        }

        private object OnClanInvite(IPlayer player, IPlayer target, string tag)
        {

            return OnTeamInvite(FindPlayerByIdS(player.Id), FindPlayerByIdS(target.Id));
        }

        private object OnTurretAuthorize(AutoTurret turret, BasePlayer player)
        {
            if (turret == null || player == null) return null;

            var authCount = 0;

            foreach (var ply in turret.authorizedPlayers)
            {
                var userIdStr = ply.userid.ToString();
                if (userIdStr == player.UserIDString) continue;

                if (!(covalence.Players?.FindPlayerById(userIdStr)?.IsAdmin ?? false))
                {
                    var plyClass = GetUserClassType(userIdStr);

                    if (plyClass == ClassType.Loner || plyClass == ClassType.Nomad)
                    {
                        SendReply(player, "You're not able to authorize yourself to this turret because a " + plyClass + " is authorized to it.");
                        ShowToast(player, "This belongs to a " + plyClass + ", blocking you from authorizing!", 1);
                        return true;
                    }

                    authCount++;
                }
            }

            var authUserClass = GetUserClassType(player.UserIDString);

            if (authCount > 0 && (turret.OwnerID != player.userID) && (authUserClass == ClassType.Loner || authUserClass == ClassType.Nomad))
            {
                //needs sb
                SendReply(player, "You are a " + authUserClass + " - which inhibits you from authorizing to this turret, because at least one other player is authorized to it.");
                ShowToast(player, "This belongs to a " + authUserClass + ", blocking you from authorizing!", 1);

                return true;
            }

            return null;
        }

        private object OnCupboardAuthorize(BuildingPrivlidge privilege, BasePlayer player)
        {
            if (privilege == null || player == null) return null;

            var authCount = 0;

            foreach (var ply in privilege.authorizedPlayers)
            {

                var userIdStr = ply.userid.ToString();
                if (userIdStr == player.UserIDString) continue;

                if (!(covalence.Players?.FindPlayerById(userIdStr)?.IsAdmin ?? false))
                {
                    var plyClass = GetUserClassType(userIdStr);

                    if (plyClass == ClassType.Loner || plyClass == ClassType.Nomad)
                    {
                        SendReply(player, "You're not able to authorize yourself to this cupboard because a " + plyClass + " is authorized to it.");
                        ShowToast(player, "This belongs to a " + plyClass + ", blocking you from authorizing!", 1);

                        return true;
                    }

                    authCount++;
                }
            }

            var authUserClass = GetUserClassType(player.UserIDString);

            var inhibitTarget = false;

            if ((privilege.OwnerID != player.userID) && (authUserClass == ClassType.Loner || authUserClass == ClassType.Nomad))
                inhibitTarget = true;



            if (authCount > 0 && inhibitTarget)
            {
                //needs sb
                SendReply(player, "You are a " + authUserClass + " - which inhibits you from authorizing to this cupboard, because at least one other player is authorized to it.");
                ShowToast(player, "This belongs to a " + authUserClass + ", blocking you from authorizing!", 1);


                return true;
            }

            return null;
        }

        // private object OnCodeEntered(CodeLock codeLock, BasePlayer player, string code)
        //on code entered may be an issue because what if a loner/nomad is raiding, or somehow got access to a code? they should be able to enter it.

        private void OnPlayerRespawned(BasePlayer player)
        {
            //clear all player second wind sleeping bags upon respawn
            var toRemove = Pool.Get<HashSet<NetworkableId>>();
            try
            {
                foreach (var id in _secondWindSleepingBags)
                {
                    var bag = BaseNetworkable.serverEntities.Find(id) as SleepingBag;
                    if (bag == null || bag.IsDestroyed || bag.deployerUserID != player.userID)
                        continue;

                    bag.Kill();

                    toRemove.Add(id);

                }

                foreach (var id in toRemove)
                    _secondWindSleepingBags.Remove(id);

            }
            finally
            {
                try { toRemove?.Clear(); }
                finally { Pool.Free(ref toRemove); }
            }
        }

        private void OnBedMade(SleepingBag bag, BasePlayer player)
        {
            AdjustSleepingBagForNomad(player, bag);
        }

        private void OnPlayerRespawn(BasePlayer player, SleepingBagCamper bagCamper)
        {
            OnPlayerRespawn(player, bagCamper as SleepingBag);
        }

        private void OnPlayerRespawn(BasePlayer player, SleepingBag bag)
        {
            if (player == null || bag == null) return;

            if (_secondWindSleepingBags.Remove(bag.net.ID))
            {
                if (!bag.IsDestroyed)
                    bag.Kill();

                PrintWarning("found second wind bag & removed!");

                var userId = player?.userID ?? 0;

                _secondWindThornsBonusActive.Add(userId);

                ServerMgr.Instance.Invoke(() =>
                {
                    _secondWindThornsBonusActive.Remove(userId);
                }, 10f);

                var pNetId = player.net.ID;

                if (_npcImmuneEntities.Add(pNetId))
                {
                    ServerMgr.Instance.Invoke(() =>
                    {
                        _npcImmuneEntities.Remove(pNetId);
                    }, 20f);
                }

                GrantImmunity(player, 5); //full immunity for 5 seconds. 20 second immunity above for NPCs.

             

                var handler = player?.GetComponent<SecondWindCooldownHandler>();
              
                var sb = Pool.Get<StringBuilder>();
                try 
                {
                    sb.Clear().Append("Second Wind!");

                    if (handler != null)
                        sb.Append(" Cooldown: ").Append(handler.GetCooldownSeconds().ToString("N0")).Append("s");

                    ShowToast(player, sb.ToString(), 1);

                }
                finally { Pool.Free(ref sb); }
             
                return;
            }

            var userClass = GetUserClassType(player.UserIDString);
            if (userClass != ClassType.Nomad)
                return;


            bag?.Invoke(() =>
            {
                if (bag == null || bag.IsDestroyed) return;

                var oldTime = bag.secondsBetweenReuses;
                var newTime = oldTime * ((bag.prefabID == 159326486 || bag.prefabID == 3003382652 || bag.prefabID == 2568114225) ? 0.125f : 1.5f); //prefab IDs are sleeping bag, beach towel, camper sleeping bag, respectively.

                PrintWarning("invoke! secondsbetween resues normal: " + bag.secondsBetweenReuses + " new: " + newTime);
                bag.SetUnlockTime(Time.realtimeSinceStartup + newTime);
            }, 2f);

        }

        private void OnVendingTransaction(VendingMachine shop, BasePlayer player, int sellOrderId, int numberOfTransactions) //goodness gracious this is messy but I promise it works
        {
            var userClass = GetUserClassType(player.UserIDString);

            var sellOrder = shop.sellOrders.sellOrders[sellOrderId];
            if (sellOrder == null)
            {
                PrintWarning("sellOrder null?!");
                return;
            }

            var currencyItem = ItemManager.FindItemDefinition(sellOrder.currencyID);
            var currencyAmount = sellOrder.currencyAmountPerItem * numberOfTransactions;

            if (userClass == ClassType.Loner && shop.prefabID == 858853278) //shopkeeper_vm_invis
            {
                PrintWarning("invis vm!! - presumably bandit/vm, get partial refund");

                player.Server_CancelGesture();

                player.Server_StartGesture(StringToGesture(player, "wave"));

                shop.Invoke(() =>
                {
                    if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead()) return;

                    var refundAmount = (int)Math.Round(currencyAmount * 0.25, MidpointRounding.AwayFromZero);

                    if (refundAmount < 1)
                    {
                        PrintWarning("refundAmount < 1!!!");
                        return;
                    }

                    Item foundStackable = null;

                    var userItems = Pool.GetList<Item>();

                    try
                    {
                        player.inventory.AllItemsNoAlloc(ref userItems);

                        for (int i = 0; i < userItems.Count; i++)
                        {
                            var userItem = userItems[i];

                            if (userItem?.info?.itemid == currencyItem?.itemid && (userItem.amount + refundAmount) <= userItem.MaxStackable())
                            {
                                foundStackable = userItem;
                                break;
                            }
                        }
                    }
                    finally { Pool.FreeList(ref userItems); }

                    PrintWarning("spent currency should be: " + currencyItem.displayName.english + " x" + currencyAmount);

                    NoteItemByDef(player, currencyItem, currencyAmount);

                    if (foundStackable != null)
                    {
                        foundStackable.amount += refundAmount;
                        foundStackable.MarkDirty();
                    }
                    else
                    {
                        var newItem = ItemManager.Create(currencyItem, refundAmount);

                        player.GiveItem(newItem);
                    }

                    var sb = Pool.Get<StringBuilder>();

                    try { SendLuckNotifications(player, sb.Clear().Append("<color=#623453>Because of your friendship with the bandits, they've returned <color=#7F6B54>").Append(refundAmount.ToString("N0")).Append("</color> of your <color=#7F6B54>").Append(currencyItem.displayName.english).Append("</color>!").ToString()); }
                    finally { Pool.Free(ref sb); }

                    player.Server_CancelGesture();

                    player.Server_StartGesture(StringToGesture(player, Random.Range(0, 101) <= 33 ? "victory" : "ok"));

                }, 1.25f);

                return;
            }

            var thief = GetLuckInfo(player)?.GetStatLevelByName("Thief") ?? 0;

            if (thief < 1)
                return;


            var chance = 2.5 * thief;

            if (Random.Range(0, 101) > GetChanceScaledDouble(player.userID, chance, chance, 99))
                return; //no rng hit

            PrintWarning("0, 101 <= chance (scaled of this): " + chance);


            shop.Invoke(() =>
            {

                if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected) return;



                var plyPos = player?.transform?.position ?? Vector3.zero;

                player.Server_CancelGesture();
                player.Server_StartGesture(StringToGesture(player, "hurry"));

                player.Invoke(() =>
                {
                    player.Server_CancelGesture();
                }, 0.8f);

                Effect.server.Run("assets/prefabs/tools/keycard/effects/deploy.prefab", plyPos, Vector3.zero);
                Effect.server.Run("assets/prefabs/tools/keycard/effects/swipe.prefab", plyPos, Vector3.zero);


                //"swipe" visual effect is done by hurry gesture which is cancelled before it finishes





                var refundAmount = Mathf.Clamp((int)Math.Round(currencyAmount * (0.075d * thief), MidpointRounding.AwayFromZero), 1, currencyAmount);


                Item foundStackable = null;

                var userItems = Pool.GetList<Item>();

                try
                {
                    player.inventory.AllItemsNoAlloc(ref userItems);

                    for (int i = 0; i < userItems.Count; i++)
                    {
                        var userItem = userItems[i];

                        if (userItem?.info?.itemid == currencyItem?.itemid && (userItem.amount + refundAmount) <= userItem.MaxStackable())
                        {
                            foundStackable = userItem;
                            break;
                        }
                    }
                }
                finally { Pool.FreeList(ref userItems); }

                PrintWarning("spent currency should be: " + currencyItem.displayName.english + " x" + currencyAmount);

                NoteItemByDef(player, currencyItem, currencyAmount);

                if (foundStackable != null)
                {
                    foundStackable.amount += refundAmount;
                    foundStackable.MarkDirty();
                }
                else
                {
                    
                    var newItem = ItemManager.Create(currencyItem, refundAmount);

                    player.GiveItem(newItem);
                }

                var sb = Pool.Get<StringBuilder>();

                try { SendLuckNotifications(player, sb.Clear().Append("<color=#623453>Your cunning thievery has allowed you to steal <color=#7F6B54>").Append(refundAmount.ToString("N0")).Append("</color> of your <color=#7F6B54>").Append(currencyItem.displayName.english).Append("</color> back!").ToString()); }
                finally { Pool.Free(ref sb); }

            }, 0.05f);


        }

        private object OnVendingShopOpen(VendingMachine shop, BasePlayer player)
        {
            if (shop == null || player == null || string.IsNullOrWhiteSpace(shop.shopName)) return null;

            var userClass = GetUserClassType(player.UserIDString);

            for (int i = 0; i < Enum.GetValues(typeof(ClassType)).Length; i++)
            {
                var classType = (ClassType)i;

                var stringClass = classType.ToString();

                if (!shop.shopName.Equals(stringClass, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (userClass == classType) 
                    return null;

                var shopClass = ARPGClass.FindByType(classType);
                if (shopClass == null)
                {
                    PrintWarning("shopClass null?! ARPGClass.FindByType returned null for: " + stringClass);
                    continue;
                }

                //deny sfx?
                var sb = Pool.Get<StringBuilder>();
                try
                {
                    var strMsg = sb.Clear().Append("<color=").Append(shopClass.SecondaryColor).Append(">You cannot open this <i>Vending Machine!</i>\nIt is specific to the <i><color=").Append(shopClass.PrimaryColor).Append(">").Append(shopClass.DisplayName).Append("</i></color> class!</color>").ToString();

                    ShowToast(player, strMsg, 1);
                    SendReply(player, strMsg);
                }
                finally { Pool.Free(ref sb); }


                return false;
            }

            return null;
        }

        private object OnGetCraftLevel(BasePlayer player)
        {
            if (player == null) return null;

            if (player.cachedCraftLevel > 2 && player.nextCheckTime > Time.realtimeSinceStartup)
                return null; //already 3 or higher (?). no need to run checks
            

            var rpgClass = GetUserClassType(player.UserIDString);
            if (rpgClass != ClassType.Nomad && !ERA_CORRUPTION) return null;

            var hasNomadSuit = IsWearingItem(player, NOMAD_SUIT_ID);

            if (!hasNomadSuit && rpgClass != ClassType.Nomad)
                return null;

            var desiredWorkbench = hasNomadSuit ? 3 : 2;

            if (player?.triggers != null)
            {
                for (int i = 0; i < player.triggers.Count; i++)
                {
                    var benchTrig = player.triggers[i] as TriggerWorkbench;
                    if (benchTrig != null && benchTrig.WorkbenchLevel() >= desiredWorkbench)
                        return null;
                }
            }

            player.cachedCraftLevel = desiredWorkbench;
            player.nextCheckTime = Time.realtimeSinceStartup + Random.Range(0.4f, 0.5f);

            return desiredWorkbench;
        }

        /*/
        [ChatCommand("charm")]
        private void cmdCharm(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var xpBonus = UnityEngine.Random.Range(-70f, 200f);
            var itemBonus = UnityEngine.Random.Range(-50f, 75f);
            var skill = (args.Length > 0) ? args[0].ToLower() : "Woodcutting";
            var type = (xpBonus < 40f && itemBonus < 20) ? CharmType.Small : (xpBonus < 65 && itemBonus < 30) ? CharmType.Large : CharmType.Grand;
            var newCharm = CreateCharm(skill, type, xpBonus, itemBonus);
            var charmData = charmsData.GetCharm(newCharm);
            SendReply(player, "Charm xp: " + charmData.xpBonus + ", charm item bonus: " + charmData.itemBonus + ", skill: " + charmData.skill);
            if (!newCharm.MoveToContainer(player.inventory.containerBelt) && !newCharm.MoveToContainer(player.inventory.containerMain)) RemoveFromWorld(newCharm);
        }/*/

        private object CanEquipItem(PlayerInventory inventory, Item item, int targetPos) //only called when putting in belt slots
        {
            return item?.info?.category == ItemCategory.Attire ? null : CanWearItem(inventory, item, targetPos, targetPos < 0);
        }

        private object CanWearItem(PlayerInventory inventory, Item item, int targetSlot, bool forceSilent = false)
        {

            BasePlayer player = null;

            try 
            {
                player = inventory?._baseEntity;
                if (player == null || player.gameObject == null || player.IsDestroyed || player.IsNpc || player.IsDead() || (!player.IsSleeping() && !player.IsConnected)) return null;


                if (IsBackpackItem(item) && targetSlot != 7)
                {
                    if (!item.MoveToContainer(inventory.containerWear, 7))
                        ShowToast(inventory._baseEntity, "No backpack slot!", 1);

                    return false;
                }

                //should note what it WOULD grant if player could equip (it does...? [later note])

                var leg = LegendaryItemInfo.GetLegendaryInfoFromItem(item);

                if (leg == null)
                    return null;

                var hasCorruptionGloves = IsWearingSkin(player, CORRUPTION_GLOVES_SKIN_ID);

                var userClass = GetUserClassType(player.UserIDString);

                var canEquip = true;
                var equipSb = Pool.Get<StringBuilder>();
                try
                {

                    equipSb.Clear();

                    var cantEquipRequires = equipSb.Clear().Append("<color=#FEDD00>You cannot equip <i><color=#fef8c9>").Append(leg.DisplayName).Append("</color>!</i>\nIt requires you to be ").ToString();

                    equipSb.Clear();

                    if (userClass != leg.RequiredClass && leg.RequiredClass != ClassType.Undefined)
                    {
                        var reqClass = ARPGClass.FindByType(leg.RequiredClass);
                        var userClassArpg = ARPGClass.FindByType(userClass);

                        var userClassColor = userClassArpg?.PrimaryColor ?? LUCK_COLOR;

                        if (ERA_CORRUPTION)
                            equipSb.Append("<color=red>As all order breaks down and PRISM is corrupted, you feel more powerful.</color>\n<color=").Append(userClassColor).Append(">You</color> <color=orange>are able to equip this item despite it being made for <color=").Append(reqClass.PrimaryColor).Append(">another class</color>.</color>").Append(Environment.NewLine);
                        else
                        {
                            canEquip = false;


                            equipSb.Append("<color=").Append(reqClass.SecondaryColor).Append(">You cannot equip <color=").Append(reqClass.PrimaryColor).Append("><i>").Append(leg.DisplayName).Append("</color>!</i>\nIt is specific to the <color=").Append(reqClass.PrimaryColor).Append("><i>").Append(leg.RequiredClass).Append("</i></color> class!</color>").Append(Environment.NewLine);
                        }

                    }



                    var survivalLvl = ZLevelsRemastered?.Call<int>("GetLevel", player.UserIDString, "S") ?? -1;

                    var reqSurvLvl = leg.RequiredSurvivalLevel;

                    var reqWcLvl = leg.RequiredWoodcuttingLevel;

                    var reqMiningLvl = leg.RequiredMiningLevel;

                    var reqLuckLvl = leg.RequiredLuckLevel;


                    if (hasCorruptionGloves)
                    {
                        if (reqSurvLvl > 0)
                            reqSurvLvl = (int)(reqSurvLvl * 0.5);

                        if (reqWcLvl > 0)
                            reqWcLvl = (int)(reqWcLvl * 0.5);

                        if (reqMiningLvl > 0)
                            reqMiningLvl = (int)(reqMiningLvl * 0.5);

                        if (reqLuckLvl > 0)
                            reqLuckLvl = (int)(reqLuckLvl * 0.5);

                    }




                    if (getLevelS(player.UserIDString) < reqLuckLvl)
                    {
                        canEquip = false;
                        equipSb.Append(cantEquipRequires).Append("<color=").Append(LUCK_COLOR).Append(">").Append(reqLuckLvl.ToString("N0")).Append("</color> or higher in <color=").Append(LUCK_COLOR).Append(">").Append("<i>Luck</color>!</i>").Append(Environment.NewLine);
                    }

                    if (survivalLvl < reqSurvLvl)
                    {
                        canEquip = false;
                        equipSb.Append(cantEquipRequires).Append("<color=#7F7A66>").Append(reqSurvLvl.ToString("N0")).Append("</color> or higher in <color=#7F7A66><i>Survival</color>!</i>").Append(Environment.NewLine);
                    }


                    var woodcuttingLvl = ZLevelsRemastered?.Call<int>("GetLevel", player.UserIDString, "WC") ?? -1;

                    if (woodcuttingLvl < reqWcLvl)
                    {
                        canEquip = false;
                        equipSb.Append(cantEquipRequires).Append("<color=#977C46>").Append(reqWcLvl.ToString("N0")).Append("</color> or higher in <color=#977C46><i>Woodcutting</color>!</i>").Append(Environment.NewLine);
                    }

                    var miningLvl = ZLevelsRemastered?.Call<int>("GetLevel", player.UserIDString, "M") ?? -1;

                    if (miningLvl < reqMiningLvl)
                    {
                        canEquip = false;
                        equipSb.Append(cantEquipRequires).Append("<color=#6EA6BF>").Append(reqMiningLvl.ToString("N0")).Append("</color> or higher in <color=#6EA6BF><i>Mining</color>!</i>").Append(Environment.NewLine);
                    }

                    if (equipSb.Length > 1)
                        equipSb.Length -= 1;

                    var equipStrShort = equipSb.ToString();

                    if (!canEquip)
                    {
                        equipSb.Append("\n\n<color=#fef8c9><i>If</i> equipped, <i><color=#FEDD00>").Append(leg.DisplayName).Append("</color></i> would grant the bonuses:</color>\n");
                        var mods = leg.GetModifiersString(true);

                        if (!string.IsNullOrWhiteSpace(mods))
                            equipSb.Append("<color=#FEDD00>").Append(mods).Append("</color>\n");

                        if (!string.IsNullOrWhiteSpace(leg.BonusDescription))
                            equipSb.Append("<color=#FCEC85><i>").Append(leg.BonusDescription).Append("</i></color>\n");
                    }
                   



                    if (canEquip)
                        player.nextCheckTime = Time.realtimeSinceStartup; //reset check time so it detects nomad suit & sets to w3 sooner rather than later

                    if (!forceSilent && equipSb.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(equipStrShort))
                            ShowToast(player, equipStrShort, 1);

                        if (equipSb.Length > 1)
                            equipSb.Length -= 1; //trim newline

                        SendCooldownMessage(player, equipSb.ToString(), 0.5f, "CANNOT_EQUIP_LEGENDARY");

                    }

                    if (canEquip && targetSlot == 7 && IsBackpackItem(item))
                        return true;

                    if (canEquip && leg.SkinID == CORRUPTION_GLOVES_SKIN_ID)
                    {
                        player.Invoke(() =>
                        {
                            if (!IsWearingSkin(player, CORRUPTION_GLOVES_SKIN_ID, true))
                            {
                                PrintWarning("player no longer wearing corruption gloves on invoke despite canequip??");
                                return;
                            }

                            player.SetMaxHealth(50);

                        }, 0.5f);
                    }

                    return canEquip ? null : false;
                }
                finally { Pool.Free(ref equipSb); }
            }
            catch(Exception ex)
            {

                try
                {
                    if (IsLegendaryItem(item))
                    {
                        if (player != null)
                            SendReply(player, "An exception was thrown, so you can't equip this item. Please *immediately* report this to an admin.");

                        return false;
                    }
                }
                finally { PrintError(ex.ToString()); }
                

                return null;
            }
         
           
        }
        

        private object CanAcceptItem(ItemContainer container, Item item, int targetPos)
        {
            if (container == null || item == null) return null;

            var containerEntityOwner = container?.GetEntityOwner(true);

            var containerPlayer = container?.playerOwner ?? container?.GetOwnerPlayer();

            var player = containerPlayer ?? item?.GetOwnerPlayer();

            if (player != null && (container != player?.inventory?.containerBelt && container != player?.inventory?.containerMain && container != player?.inventory?.containerWear) && containerEntityOwner is AttackEntity && IsLegendaryItem(item))
            {
                var canWear = CanWearItem(player.inventory, item, targetPos);

                if (canWear != null && !(bool)canWear)
                    return ItemContainer.CanAcceptResult.CannotAccept;
            }

            if (container == player?.inventory?.containerWear && IsBackpackItem(item))
            {
                var wearItems = player?.inventory?.containerWear?.itemList;

                for (int i = 0; i < wearItems.Count; i++)
                    if (IsBackpackItem(wearItems[i]))
                        return ItemContainer.CanAcceptResult.CannotAccept; //already wearing a backpack
            }

           
            if (containerEntityOwner?.skinID == TWITCHY_TOMMY_SKIN_ID && item.info.itemid == 2005491391)
            {
                if (player != null)
                    SendCooldownMessage(player, "This item is embued with a mystical force, preventing you from attaching this.", 0.5f, "TWITCHY_TOMMY_EXT_MAG_NOTICE");

                return ItemContainer.CanAcceptResult.CannotAccept; //legendary item has 30 rnd mag already
            }

            var entity = container?.entityOwner;

            if (entity != null)
            {
                BasePlayer owner = null;

                var isBackpack = false;

                foreach (var kvp in _userBackpack)
                {
                    if (kvp.Value == null)
                    {
                        PrintWarning("found null kvp.value!!");
                        continue;
                    }

                    if (kvp.Value == entity)
                    {
                        owner = FindPlayerByIdS(kvp.Key);
                        isBackpack = true;
                        break;
                    }
                }

                if (!isBackpack)
                    isBackpack = IsBackpackSkin(entity.skinID);

                if (isBackpack)
                {
                    if (IsBackpackItem(item))
                    {
                        PrintWarning("isBackpack, and also isbackpackitem, so deny (tried move backpack into backpack)");

                        return ItemContainer.CanAcceptResult.CannotAccept;
                    }
                    else if (owner != null && player != owner)
                    {
                        PrintWarning("another player likely tried to deposit item, so deny. owner: " + owner + ", player: " + player);

                        return ItemContainer.CanAcceptResult.CannotAccept;
                    }
                    else
                    {
                        var parent = entity?.GetParentEntity();
                        if (parent == null || parent != owner || parent != player)
                        {
                            if (owner != null || player != null)
                            {
                                SendLocalEffect(owner ?? player, "assets/prefabs/npc/autoturret/effects/targetlost.prefab");

                                var str = "You must be wearing a backpack in order to deposit items!";
                                SendCooldownMessage(owner ?? player, str, 0.5f, "BACKPACK_DEPOSIT_MSG");
                                ShowPopup(owner ?? player, str, 3);
                            }



                            return ItemContainer.CanAcceptResult.CannotAccept;
                        }
                    }

                }

            }


            if (player == null) return null;

            var entitySource = container?.entityOwner ?? player?.inventory?.loot?.entitySource;
            if (entitySource != null && entitySource.OwnerID == 528491 && entitySource.prefabID == 1560881570)
            {
                var itemId = item?.info?.itemid ?? 0;

                if (itemId != PURE_WOOD_TEA_ID && itemId != PURE_ORE_TEA_ID && itemId != PURE_SCRAP_TEA_ID && itemId != WOLF_SKULL_ID && itemId != WRAPPED_GIFT_ITEM_ID)
                    return ItemContainer.CanAcceptResult.CannotAccept;
            }

            if (containerPlayer == null || !IsCharm(item)) return null;

            player?.Invoke(() =>
            {
                if (ShouldHaveCharmComponent(player)) EnsureCharmComponent(player);
            }, 0.05f);


            if (container == player.inventory.containerBelt || container == player.inventory.containerWear)
            {
                SendCooldownMessage(player, "Charms cannot go in these slots! Try moving them somewhere else.", 0.5f, "CHARM_INVALID_CONTAINER_MSG");
                ShowToast(player, "Charms can't go here!", 1);
                return ItemContainer.CanAcceptResult.CannotAccept;
            }

            if (container == item?.parent || container != player.inventory.containerMain)
            {
                return null;
            }

            var pCharms = Pool.GetList<Charm>();
            try
            {
                GetPlayerCharmsNoAlloc(player, ref pCharms);

                var freeSlots = 24 - player.inventory.containerMain.itemList.Count;
                if (pCharms != null && pCharms.Count > 0)
                {
                    var largeCount = 0;
                    var grandCount = 0;

                    for (int i = 0; i < pCharms.Count; i++)
                    {
                        var charmType = pCharms[i].Type;
                        if (charmType == CharmType.Large) largeCount++;
                        else if (charmType == CharmType.Grand) grandCount++;
                    }

                    if (largeCount > 0) freeSlots -= largeCount * 3;
                    if (grandCount > 0) freeSlots -= grandCount * 4;
                }




                var getCharm = charmsData?.GetCharm(item) ?? null;
                if (getCharm == null || getCharm.Type == CharmType.Small || getCharm.Type == CharmType.Undefined) return null;

                if (freeSlots < 4 && getCharm.Type == CharmType.Large)
                {

                    SendCooldownMessage(player, "You must have at least 3 additional empty inventory slots to hold a Large charm!", 0.5f, "NO_SPACE_CHARM_LARGE");

                    ShowToast(player, "Not enough inventory space!", 1);

                    return ItemContainer.CanAcceptResult.CannotAccept;
                }

                if (freeSlots < 5 && getCharm.Type == CharmType.Grand)
                {
                    SendCooldownMessage(player, "You must have at least 4 additional empty inventory slots to hold a Grand charm!", 0.5f, "NO_SPACE_CHARM_GRAND");
                    ShowToast(player, "Not enough inventory space!", 1);

                    return ItemContainer.CanAcceptResult.CannotAccept;
                }
            }
            finally { Pool.FreeList(ref pCharms); }

            

            return null;
        }


        private Item CreateRandomCharm(bool forceLootTest = false, bool forceCraftTest = false, bool forceXpSuper = false, bool forceItemSuper = false, float charmTypeScalar = 1f)
        {
            //var skillRng = UnityEngine.Random.Range(0, 101) <= 33 ? UnityEngine.Random.Range(0, 4) : _charmTypeRng.Next(0, 4); //there are claims that survival charms are somehow way more common. lets do some weird rng to see if there's any truth

            var rng = Random.Range(1, 5); //wc, mining, survival, luck (inclusive, so 4) //Enum.GetValues(typeof(CharmSkill)).Length);

            var skill = (CharmSkill)rng;//(CharmSkill)UnityEngine.Random.Range(1, Enum.GetValues(typeof(ClassType)).Length); //0 == undefined

            if (skill == CharmSkill.Luck && Random.Range(0, 101) <= 33)
                skill = CharmSkill.Crafting;

            /*/
             *         private Item CreateRandomCharm(bool forceLootTest = false, bool forceCraftTest = false, bool forceXpSuper = false, bool forceItemSuper = false, float charmTypeScalar = 1f)
        {
            var skillRng = UnityEngine.Random.Range(0, 101) <= 33 ? UnityEngine.Random.Range(0, 4) : _charmTypeRng.Next(0, 4); //there are claims that survival charms are somehow way more common. lets do some weird rng to see if there's any truth
            var skill = skillRng == 0 ? "Woodcutting" : skillRng == 1 ? "Mining" : skillRng == 2 ? "Survival" : "Luck";
            if (skill == "Luck" && UnityEngine.Random.Range(0, 101) <= 33) skill = "Crafting";

             */

            Rarity typeLoot = Rarity.None;
            if (forceLootTest)
            {
                typeLoot = (Rarity)Random.Range(1, Random.Range(0, 101) <= 45 ? 5 : 3);
                PrintWarning("typeLoot: " + typeLoot);
                skill = CharmSkill.Loot;
            }
            

            if (forceCraftTest)
            {
                skill = CharmSkill.Crafting;
            }
            var typeRng = Random.Range(0, 101 * charmTypeScalar);

            CharmType type;

            var grandNeeded = 8;
            var largeNeeded = 24;

            var isRad = IsRadStormActive();

            if (isRad)
            {
                grandNeeded = 18;
                largeNeeded = 36;
            }

            if (typeRng < grandNeeded) type = CharmType.Grand;
            else if (typeRng < largeNeeded) type = CharmType.Large;
            else type = CharmType.Small;

            if (type == CharmType.Undefined)
            {
                Interface.Oxide.LogError("!! UNDEFINED CHARM !!");
                return null;
            }

            if (forceItemSuper || forceXpSuper) type = CharmType.Grand;

            var bonusRng = Random.Range(0, 101);
            var bothRng = forceItemSuper || forceXpSuper || Random.Range(0, 101) < Random.Range(type > CharmType.Large ? 8 : 4, type > CharmType.Large ? 17 : 11) && type > CharmType.Small;
            var xpBonus = 0.0f;
            var itemBonus = 0.0f;
            var chanceBonus = 0.0f;
            var lootBonus = 0.0f;

            var superXpCharm = false;
            var superItemCharm = false;




            if (skill != CharmSkill.Crafting && skill != CharmSkill.Loot)
            {
                if (bonusRng <= 66 || bothRng)
                {
                    superXpCharm = forceXpSuper || type == CharmType.Grand && Random.Range(0, 101) <= 14 && charmXpRng.Next(0, 101) <= 14;




                    var lowStart = bothRng ? -32f : 5f;
                    if (type == CharmType.Large)
                    {
                        lowStart += Random.Range(14f, 24f);
                        if (!bothRng) lowStart += Random.Range(9f, 21f);
                    }

                    if (type == CharmType.Grand)
                    {
                        lowStart += Random.Range(18f, 34f);
                        if (!bothRng) lowStart += Random.Range(14f, 29f);
                    }

                    var highStart = bothRng ? 7f : 10f;

                    if (type == CharmType.Large)
                    {
                        lowStart += Random.Range(12f, 35f);

                        highStart += Random.Range(32f, 60f);
                        if (!bothRng) highStart += Random.Range(10f, 23f);
                    }

                    if (type == CharmType.Grand)
                    {

                        lowStart += Random.Range(25f, 64f);

                        highStart += Random.Range(35f, 96f);
                        if (!bothRng) highStart += Random.Range(20f, 31f);

                        if (superXpCharm)
                        {
                            lowStart += Random.Range(120, 240);
                            highStart += Random.Range(300, 481);
                        }
                    }


                    var xpLowRange = Random.Range(lowStart, lowStart + Random.Range(3, 7));
                    var highEnd = (type == CharmType.Small ? 15f : type == CharmType.Large ? 105f : 210f) + (bothRng ? 60 : 0); //== small ? 25 check here should be redundant, as bothRng requires > small

                    if (superXpCharm) highEnd += Random.Range(320, Random.Range(480, 641));

                    if (highStart > highEnd)
                    {
                        PrintWarning("highStart (" + highStart + ") is greater than highEnd (" + highEnd + ")");
                        highEnd += highStart;
                        PrintWarning("set highEnd to: " + highEnd);
                    }

                    var xpHighRange = Random.Range(highStart, highEnd);
                    xpBonus = Random.Range(xpLowRange, xpHighRange);

                    if (superXpCharm) PrintWarning("!!! super Xp charm spawn, xp bonus: " + xpBonus + " for: " + skill + ", type: " + type + ", highStart was: " + highStart + ", highEnd: " + highEnd + ", lowStart: " + lowStart);

                }

                if (bonusRng > 66 || bothRng)
                {
                    superItemCharm = forceItemSuper || type == CharmType.Grand && Random.Range(0, 101) <= 8 && charmXpRng.Next(0, 101) <= 8;

                    var lowStart = bothRng ? -20f : 4f;

                    if (type == CharmType.Large) lowStart += bothRng ? Random.Range(11f, 20f) : Random.Range(14f, 25f);
                    if (type == CharmType.Grand) lowStart += bothRng ? Random.Range(12f, 24f) : Random.Range(17f, 29f);

                    var highStart = bothRng ? 12f : 7f;

                    if (type == CharmType.Large) highStart += bothRng ? Random.Range(14f, 22f) : Random.Range(19f, 27f);
                    if (type == CharmType.Grand) highStart += bothRng ? Random.Range(20f, 25f) : Random.Range(22f, 30f);

                    var highEnd = (type == CharmType.Small ? 14f : type == CharmType.Large ? 60f : 160f) + (bothRng ? 80f : 0f);

                    if (superItemCharm)
                    {
                        highEnd += Random.Range(240, Random.Range(380, 541));

                        lowStart += Random.Range(65, 116);
                        highStart += Random.Range(150, 371);
                    }

                    var itemLowRange = Random.Range(lowStart, lowStart + (bothRng ? 8f : 2.25f));




                    if (highStart > highEnd)
                    {
                        PrintWarning("highStart (" + highStart + ") is greater than highEnd (" + highEnd + ")");
                        highEnd += highStart;
                        PrintWarning("set highEnd to: " + highEnd);
                    }

                    var itemHighRange = Random.Range(highStart, highEnd);
                    itemBonus = Random.Range(itemLowRange, itemHighRange);

                    if (superItemCharm) PrintWarning("!!! super item charm spawn, item bonus: " + itemBonus + " for: " + skill + ", type: " + type);

                }
            }


            if (skill == CharmSkill.Luck)
            {
                var chanceRng = Random.Range(0, 101);
                var needRngMin = type == CharmType.Small ? 32 : type == CharmType.Large ? 18 : 7;
                var needRngMax = type == CharmType.Small ? 45 : type == CharmType.Large ? 25 : 12;
                if (chanceRng <= Random.Range(needRngMin, needRngMax))
                {
                    var rangeMin = type == CharmType.Small ? 0.4f : type == CharmType.Large ? 3.2f : 4.5f;
                    var rangeMax = type == CharmType.Small ? 3.2f : type == CharmType.Large ? 7.75f : 16f;
                    var range = Random.Range(rangeMin, rangeMax);
                    chanceBonus = range;
                    itemBonus = 0f;
                    xpBonus = 0f;
                    //   PrintWarning("Creating chance Luck charm, type: " + type + " range: " + range + ", min: " + rangeMin + ", max: " + rangeMax);
                }
            }

            var craftBonus = 0.0f;
            if (skill == CharmSkill.Loot && forceLootTest)
            {
                //   var needRngMin = type == CharmType.Small ? 32 : type == CharmType.Large ? 18 : 7;
                //  var needRngMax = type == CharmType.Small ? 45 : type == CharmType.Large ? 25 : 12;
                var rangeMin = type == CharmType.Small ? 0.2f : type == CharmType.Large ? 1f : 1.9f;
                var rangeMax = type == CharmType.Small ? 2.5f : type == CharmType.Large ? 5.5f : 11;
                var range = Random.Range(rangeMin, rangeMax);
                chanceBonus = 0f;
                itemBonus = 0f;
                xpBonus = 0f;
                lootBonus = range;
                //  PrintWarning("Creating loot chance charm, type: " + type + " range: " + range + ", min: " + rangeMin + ", max: " + rangeMax);
            }

            if (skill == CharmSkill.Crafting)
            {

                //    var needRngMin = type == CharmType.Small ? 32 : type == CharmType.Large ? 18 : 7;
                //   var needRngMax = type == CharmType.Small ? 45 : type == CharmType.Large ? 25 : 12;
                var rangeMin = type == CharmType.Small ? 3.2f : type == CharmType.Large ? 21.7f : 32f;
                var rangeMax = type == CharmType.Small ? 9.5f : type == CharmType.Large ? 49f : 65f;
                var range = Random.Range(rangeMin, rangeMax);
                chanceBonus = 0f;
                itemBonus = 0f;
                xpBonus = 0f;
                craftBonus = range;
                //   PrintWarning("Creating craft charm, type: " + type + " range: " + range + ", min: " + rangeMin + ", max: " + rangeMax);
            }

            if (superXpCharm) PrintWarning("SUPER XP CHARM: " + xpBonus + ", " + itemBonus + ", " + type);
            if (superItemCharm) PrintWarning("SUPER ITEM CHARM: " + itemBonus + ", " + xpBonus + ", " + type);

            return CreateCharm(skill, type, xpBonus, itemBonus, chanceBonus, craftBonus, lootBonus, typeLoot);
        }

        /*        /
         *        
        private bool IsBlockedSkin(ulong skin)
        {
            return skin == NOMAD_SUIT_SKIN_ID || skin == BURLAP_SHIRT_OVERALLS_SKIN_ID || skin == BURLAP_TROUSERS_OVERALLS_SKIN_ID || skin == BOONIE_STRAW_HAT_SKIN_ID;
        }

        private bool IsBlockedItem(int itemId)
        {
            return itemId == LUMBERJACK_HOODIE_ID || itemId == NOMAD_SUIT_ID;
        }
/*/

        private Item CreateRandomLegendaryItem()
        {
            return LegendaryItemInfo.GetRandomLegendary().SpawnItem();
        }

        private Item CreateLegendaryItemFromID(ulong skinId, int itemId = 0)
        {
            if (skinId == 0 && itemId == 0) return null;

            for (int i = 0; i < _legendaryData.Infos.Count; i++)
            {
                var leg = _legendaryData.Infos[i];

                if ((leg.SkinID == 0 && leg.ItemID != 0 && itemId == leg.ItemID) || leg.SkinID == skinId && (itemId == 0 || leg.ItemID == itemId))
                    return leg.SpawnItem();
            }

            return null;
        }

        private Item CreateRandomGenieLamp()
        {
            var item = ItemManager.CreateByItemID(WRAPPED_GIFT_ITEM_ID, 1, GENIE_LAMP_SKIN_ID_LUCK);
            item.name = "Luck Lamp";



            return item;
        }

        private Item CreateInactiveTokenOfAbsolution(int amount = 1)
        {
            if (amount < 1) 
                throw new ArgumentOutOfRangeException(nameof(amount));

            var newToken = ItemManager.CreateByItemID(WRAPPED_GIFT_ITEM_ID, amount, TOKEN_INACTIVE_SKIN_ID);
            newToken.name = TOKEN_INACTIVE_NAME;

            return newToken;
        }

        private Item CreateActiveTokenOfAbsolution(int amount = 1)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var newToken = ItemManager.CreateByItemID(WRAPPED_GIFT_ITEM_ID, amount, TOKEN_ACTIVE_SKIN_ID);
            newToken.name = TOKEN_ACTIVE_NAME;

            return newToken;
        }

        [ChatCommand("rndcharm")]
        private void cmdRndCharm(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var force = false;
            if (args.Length > 0) bool.TryParse(args[0], out force);

            var force2 = false;
            if (args.Length > 1) bool.TryParse(args[1], out force2);

            var forceXpSuper = false;
            var forceItemSuper = false;
            if (args.Length > 2) bool.TryParse(args[2], out forceXpSuper);
            if (args.Length > 3) bool.TryParse(args[3], out forceItemSuper);


            var newCharm = CreateRandomCharm(force, force2, forceXpSuper, forceItemSuper);
            var charmData = charmsData.GetCharm(newCharm);
            SendReply(player, "Charm xp: " + charmData.xpBonus + ", charm item bonus: " + charmData.itemBonus + ", skill: " + charmData.Skill);
            if (!newCharm.MoveToContainer(player.inventory.containerMain)) RemoveFromWorld(newCharm);
        }

        [ChatCommand("scharm")]
        private void cmdSCharm(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 7) return;
            if (!int.TryParse(args[0], out int intType) || intType < 1 || intType > 3)
            {
                SendReply(player, "Bad type: " + intType);
                return;
            }

            var charmType = (CharmType)intType;

            if (!int.TryParse(args[1], out intType) || intType > 5 || intType < 0)
            {
                SendReply(player, "Bad skill type: " + intType);
                return;
            }
            if (!float.TryParse(args[2], out float xpbonus) || !float.TryParse(args[3], out float itembonus) || !float.TryParse(args[4], out float chanceBonus) || !float.TryParse(args[5], out float lootBonus) || !float.TryParse(args[6], out float craftBonus))
            {
                SendReply(player, "No parse item, xp or chance bonus");
                return;
            }
            var skillType = (CharmSkill)intType; // intType == 0 ? "Woodcutting" : intType == 1 ? "Mining" : intType == 2 ? "Survival" : intType == 3 ? "Luck" : intType == 4 ? "Loot" : "Crafting";
            var newCharm = CreateCharm(skillType, charmType, xpbonus, itembonus, chanceBonus, craftBonus, lootBonus);
            var charmData = charmsData.GetCharm(newCharm);
            SendReply(player, "Charm xp: " + charmData.xpBonus + ", charm item bonus: " + charmData.itemBonus + " chance bonus: " + charmData.chanceBonus + ", skill: " + charmData.Skill);
            if (!newCharm.MoveToContainer(player.inventory.containerBelt) && !newCharm.MoveToContainer(player.inventory.containerMain)) RemoveFromWorld(newCharm);
        }

        private readonly Dictionary<BasePlayer, Dictionary<CharmSkill, TimeCachedValue<float>>> _playerCharmXpCache = new();
        private readonly Dictionary<BasePlayer, Dictionary<CharmSkill, TimeCachedValue<float>>> _playerCharmGatherCache = new();


        private float GetCharmXPs(BasePlayer player, CharmSkill skill, bool skipCache = false)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.IsDead() || player?.inventory == null) 
                return 0.0f;

            if (!_playerCharmXpCache.TryGetValue(player, out _))
                _playerCharmXpCache[player] = new Dictionary<CharmSkill, TimeCachedValue<float>>();

            if (!_playerCharmXpCache[player].TryGetValue(skill, out _))
                _playerCharmXpCache[player][skill] = new TimeCachedValue<float> 
                {
                    refreshCooldown = 5f,
                    refreshRandomRange = 1.5f,
                    updateValue = new Func<float>(() =>
                    {
                        var items = Pool.GetList<Item>();
                        try
                        {
                            player?.inventory?.AllItemsNoAlloc(ref items);

                            var sum = 0f;
                            for (int i = 0; i < items.Count; i++)
                            {
                                var item = items[i];
                                if (item == null) continue;

                                var charm = charmsData?.GetCharm(item) ?? null;

                                if (charm != null && charm.Skill == skill)
                                    sum += charm.xpBonus;
                            }
                            return sum;
                        }
                        finally { Pool.FreeList(ref items); }
                    })
                };

            return _playerCharmXpCache[player][skill].Get(skipCache);
        }

        private float GetCharmGathers(BasePlayer player, CharmSkill skill, bool skipCache = false)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.IsDead() || player?.inventory == null)
                return 0.0f;

            if (!_playerCharmGatherCache.TryGetValue(player, out _))
                _playerCharmGatherCache[player] = new Dictionary<CharmSkill, TimeCachedValue<float>>();

            if (!_playerCharmGatherCache[player].TryGetValue(skill, out _))
                _playerCharmGatherCache[player][skill] = new TimeCachedValue<float>()
                {
                    refreshCooldown = 5f,
                    refreshRandomRange = 1.5f,
                    updateValue = new Func<float>(() =>
                    {
                        var items = Pool.GetList<Item>();
                        try
                        {
                            player?.inventory?.AllItemsNoAlloc(ref items);

                            var sum = 0f;

                            for (int i = 0; i < items.Count; i++)
                            {
                                var item = items[i];
                                if (item == null) continue;

                                var charm = charmsData?.GetCharm(item) ?? null;
                                if (charm != null && charm.Skill == skill) sum += charm.itemBonus;
                            }

                            return sum;
                        }
                        finally { Pool.FreeList(ref items); }
                    })
                };

            return _playerCharmGatherCache[player][skill].Get(skipCache);
        }

        /// <summary>
        /// Returns the sum of all chance bonuses, from the Chance skill level, to charms, to the bonus from each level after level 10
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private float GetChanceBonuses(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentOutOfRangeException(nameof(userId));

            var player = FindPlayerByIdS(userId);

            var sum = 0f;

            if (player?.inventory != null)
            {
                var items = Pool.GetList<Item>();
                try 
                {
                    player?.inventory?.AllItemsNoAlloc(ref items);

                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (item == null) continue;

                        var charm = charmsData?.GetCharm(item) ?? null;
                        if (charm != null && charm.Skill == CharmSkill.Luck)
                            sum += charm.chanceBonus;
                    }
                }
                finally { Pool.FreeList(ref items); }
            }

            if (IsVIP(userId)) sum += 5f;

            if (GetUserClassType(userId) == ClassType.Loner) sum += 5f;

            var lkInfo = GetLuckInfo(userId);

            if (lkInfo != null)
            {
                var luckLvl = lkInfo.Level;

                sum += (luckLvl < 10 ? 0 : (int)(luckLvl / 10)) + lkInfo?.GetStatLevelByName("chanceLvl") ?? 0;
            }

            return sum;
        }

        private float GetCharmLootBonuses(BasePlayer player, Rarity rarity)
        {
            if (player == null || player.IsDead() || player?.inventory == null) return 0.0f;
            var items = player?.inventory?.AllItems() ?? null;
            if (items == null || items.Length < 1) return 0.0f;
            var sum = 0f;
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null) continue;
                if (IsCharm(item))
                {
                    var charm = charmsData?.GetCharm(item) ?? null;
                    if (charm == null || charm.lootRarity != rarity) continue;
                    sum += charmsData?.GetCharm(item)?.lootBonus ?? 0f;
                }
            }
            return sum;
        }

        private float GetCraftingBonus(BasePlayer player, float max = -1f)
        {
            return GetCharmCraftingBonuses(player, max) + (7 * (GetLuckInfo(player)?.GetStatLevelByName("Craftsman") ?? 0));
        }

        private float GetCharmCraftingBonuses(BasePlayer player, float max = -1f)
        {
            if (player == null || player.gameObject == null || player.IsDead() || player?.inventory == null) return 0.0f;
            var items = Pool.GetList<Item>();
            try
            {
                player?.inventory?.AllItemsNoAlloc(ref items);

                var sum = 0f;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item == null) continue;

                    var charm = charmsData?.GetCharm(item) ?? null;
                    if (charm != null && charm.craftBonus != 0.0f) sum += charm.craftBonus;
                }

                if (max > 0 && sum > max) sum = max;

                return sum;
            }
            finally { Pool.FreeList(ref items); }


            /*/
            if (diminish)
            {
                PrintWarning("sum pre diminish: " + sum);
                sum = sum * (sum / (sum + 22.5f));

                PrintWarning("sum post diminish: " + sum);
             //   sum = (sum + 100) / (sum * 33) + 1; //f(x) = (x+b) / (x*a)*100
            }/*/

        }

        /*/
        private List<Charm> GetLootCharms(BasePlayer player)
        {
            if (player == null || player.IsDead() || player.inventory == null) return null;
            var items = player?.inventory?.AllItems() ?? null;
            if (items == null || items.Length < 1) return null;
            var charms = new List<Charm>();
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var charm = charmsData?.GetCharm(item) ?? null;
                if (charm != null)
                {
                    if (player.IsAdmin) PrintWarning("charm rarity: " + charm?.lootRarity + ", lootbonus: " + charm?.lootBonus + ", text: " + item?.text);
                    if (charm == null || charm.lootRarity == Rarity.None || charm.lootBonus == 0f) continue;
                    charms.Add(charm);
                }
            }
            return charms;
        }/*/

        private List<Charm> GetPlayerCharms(BasePlayer player)
        {
            if (player == null || player.IsDead() || player.inventory == null) return null;


            var items = Pool.GetList<Item>();
            try
            {
                player.inventory.AllItemsNoAlloc(ref items);

                var charms = new List<Charm>();
                for (int i = 0; i < items.Count; i++)
                {
                    var getCharm = charmsData?.GetCharm(items[i]) ?? null;
                    if (getCharm != null) charms.Add(getCharm);
                }

                return charms;
            }
            finally { Pool.FreeList(ref items); }
        }

        private void GetPlayerCharmsNoAlloc(BasePlayer player, ref List<Charm> charms)
        {
            if (player == null || player.IsDead() || player.inventory == null) 
                return;

            if (charms == null)
                throw new ArgumentNullException(nameof(charms));

            charms.Clear();

            var items = Pool.GetList<Item>();
            try
            {
                player.inventory.AllItemsNoAlloc(ref items);

                for (int i = 0; i < items.Count; i++)
                {
                    var getCharm = charmsData?.GetCharm(items[i]) ?? null;
                    if (getCharm != null) charms.Add(getCharm);
                }

            }
            finally { Pool.FreeList(ref items); }
        }

        private void GenerateHitTargets()
        {
            var c = 0;
            var max = 9;

            foreach (var entity in BaseNetworkable.serverEntities)
            {
                if (c >= max)
                {
                    break;
                }

                var npc = entity as HumanNPC;
                if (npc == null || !npc.ShortPrefabName.Contains("scientist") || npc.ShortPrefabName.Contains("peacekeeper")) continue;

                var skipPerc = 15;

                if (DistanceFromOilRig(npc.transform.position) <= 100 || DistanceFromOilRig(npc.transform.position, true) <= 100)
                    skipPerc += 60;

                if (Random.Range(0, 101) <= skipPerc)
                    continue; //skip this one. base 15% chance of just skipping

                

                var comp = npc?.GetComponent<NPCHitTarget>();
                if (comp == null)
                {
                    comp = npc.gameObject.AddComponent<NPCHitTarget>();

                    var tierRng = Random.Range(0, 101);

                    comp.Tier = tierRng <= 1 ? NPCHitTarget.Tiers.Legendary : tierRng <= 12 ? NPCHitTarget.Tiers.Elite : tierRng <= 20 ? NPCHitTarget.Tiers.High : tierRng <= 40 ? NPCHitTarget.Tiers.Medium : NPCHitTarget.Tiers.Low; 

                    /*/
                    if (comp.Tier == NPCHitTarget.Tiers.Legendary)
                    {
                        Interface.Oxide.LogWarning("psycho legendary!");
                        comp.EquipLegendaryPsycho();

                    }/*/
                }

                if (comp != null) c++;
            }
        }

        private List<NPCHitTarget> GetHitTargets(bool unassignedOnly = false)
        {

            //optimize later

            var list = new List<NPCHitTarget>();

            foreach(var entity in BaseNetworkable.serverEntities)
            {
                var npc = entity as HumanNPC;
                if (npc == null) continue;

                var comp = npc?.GetComponent<NPCHitTarget>();
                if (comp != null && (!unassignedOnly || comp.MercInfo == null))
                    list.Add(comp);
                
            }

            return list;
        }

        [ChatCommand("heldcharm")]
        private void cmdHeldCharm(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var item = player?.GetActiveItem() ?? null;
            if (item == null || !IsCharm(item))
            {
                SendReply(player, "Not a charm");
                return;
            }
            var charmData = charmsData.GetCharm(item);
            if (charmData == null)
            {
                SendReply(player, "No charm data");
                return;
            }
            SendReply(player, charmData.Type + ", " + charmData.Skill + ", " + charmData.xpBonus + ", " + charmData.itemBonus);
        }

        private object CanUpgradeStat(BasePlayer player, LuckInfo luckInfo, StatInfo stat)
        {
            if (player == null || luckInfo == null || stat == null) return null;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                sb.Clear();

                var curStatLvl = luckInfo.GetStatLevel(stat, false);

                if (curStatLvl >= stat.MaxLevel)
                    sb.Append(string.Format(GetMessage("upgradeMaximum", player.UserIDString), stat.MaxLevel.ToString("N0"))).Append("\n");


                var curSKP = luckInfo.SkillPoints;
                var curLvl = luckInfo.Level;

                var userClass = GetUserClassType(player.UserIDString);

                var lvlToUpgrade = stat.GetRequiredLevelToUpgrade(curStatLvl);

                if (curLvl < lvlToUpgrade)
                    sb.Append(string.Format(GetMessage("upgradeBadLevel", player.UserIDString), lvlToUpgrade.ToString("N0"))).Append("\n");


                var upgradeCostNextLvl = luckInfo.GetStatUpgradeCost(stat, curStatLvl + 1);

                if (upgradeCostNextLvl < 1)
                    PrintError(nameof(upgradeCostNextLvl) + " < 1: " + upgradeCostNextLvl + " for stat: " + stat.ShortName + ", curStatLvl : " + curStatLvl + " upgrading to: " + (curStatLvl + 1));



                if (curSKP < upgradeCostNextLvl || upgradeCostNextLvl < 1)
                    sb.Append(string.Format(GetMessage("upgradeNotEnough", player.UserIDString), upgradeCostNextLvl.ToString("N0"), curSKP)).Append("\n");


                if (stat.MustMatchClass && userClass != stat.AppropriateClass && luckInfo.GetStatLevelByName("Fusion") < 1)
                    sb.Append("This skill belongs only to the ").Append(stat.AppropriateClass).Append(" class.\n");



                foreach (var kvp in stat.RequiredStatsToUpgrade)
                {
                    var statByName = FindStatInfoByName(kvp.Key);
                    var lvlInStat = luckInfo.GetStatLevel(statByName, false);

                    var reqLvl = stat.GetRequiredStatLevelToUpgrade(statByName.ShortName, curStatLvl);

                    if (lvlInStat < reqLvl) //this DOES NOT HANDLE '0' as an either/or as of yet. i'm looking into how to do that
                        sb.Append(string.Format(GetMessage("upgradeBadLevel", player.UserIDString), reqLvl.ToString("N0") + " (" + statByName.DisplayName + ")")).Append("\n");


                }

                if (sb.Length > 1) 
                    sb.Length -= 1; //remove final \n

                return sb.Length > 0 ? sb.ToString() : null;
            }
            finally { Pool.Free(ref sb); }
        }

        private void cmdSkillTrees(IPlayer iPlayer, string command, string[] args)
        {
            if (iPlayer == null || !iPlayer.IsConnected) return;

            var player = iPlayer?.Object as BasePlayer;
            if (player == null) return;

            var input = player?.serverInput ?? null;
            if (input != null && (input.WasJustPressed(BUTTON.JUMP) || input.WasJustReleased(BUTTON.JUMP))) return;

            var luckInfo = GetLuckInfo(player);
            if (luckInfo == null)
            {
                SendReply(player, "Unable to find any luck data on your player... Please contact an administrator immediately.");
                PrintError("NO LUCK INFO!: " + player.displayName);
                return;
            }

            var wasCorrectArg = false;

            var pointsSpent = luckInfo.Spent;


            var skillCost = 0;

            foreach (var kvp in luckInfo.StatLevels)
                skillCost += kvp.Value;
            

            var money_required = !(luckInfo?.HasUsedFreeReset ?? true) ? 0 : (int)Math.Round(skillCost * 12d, MidpointRounding.AwayFromZero);
            var pointsSpentAdj = (int)Math.Round(pointsSpent * 0.925, MidpointRounding.AwayFromZero);
            var batCount = player?.inventory?.GetAmount(BATTERY_ID) ?? 0;

            var arg0Lower = (args.Length > 0) ? args[0].ToLower() : string.Empty;


            if (args.Length < 1 || arg0Lower == "gui" || arg0Lower == "giu" || arg0Lower == "ui" || arg0Lower == "igu" || arg0Lower == "iug") //i love helping those who make typos, i really do
            {
                var sb = Pool.Get<StringBuilder>();
                try
                {
                    var text = string.Empty;

                    if (args.Length > 1)
                    {
                        sb.Clear();

                        for (int i = 1; i < args.Length; i++) sb.Append(args[i]).Append(" ");

                        if (sb.Length > 1) sb.Length -= 1;

                        sb.Replace("{n}", "\n");

                        text = sb.ToString();
                    }

                    if (!_curTree.TryGetValue(player.userID, out SkillTree prevTree) || prevTree == SkillTree.Undefined) prevTree = SkillTree.Resource;

                    SkillGUI(player, prevTree, text);
                    return;
                }
                finally { Pool.Free(ref sb); }
            }

            if (args.Length > 0)
            {
                try
                {

                   

                    if (arg0Lower == "help" || arg0Lower == "guide")
                    {
                        var sktHelp = "This is the skill tree. You gain points to spend on skills from gaining levels in the <color=" + LUCK_COLOR + ">Luck</color> skill.\n\nYou can level up <color=" + LUCK_COLOR + ">Luck</color> by picking up collectibles off the ground, such as stones, sulfur, metal, wood and natural (not planted) pumpkins, corn and hemp.";
                        SendReply(player, sktHelp);
                        return;
                    }
                    if (arg0Lower == "trees" || arg0Lower == "res" || arg0Lower == "res2" || arg0Lower == "nc" || arg0Lower == "combat")
                    {
                        SendReply(player, "These arguments have been retired. Please try <color=" + LUCK_COLOR + ">/skt gui</color> to see the skills!");
                        return;
                    }



                    if (arg0Lower == "respec" && args.Length < 2)
                    {
                        wasCorrectArg = true;

                        if (pointsSpent < 1 || money_required < 0)
                        {
                            SendReply(player, "You do not appear to have invested any skill points into anything!");
                            return;
                        }

                        var respecStr = "<size=20><color=orange>RESPEC:</color></size>\nTo respec means to reset all of your skills (this is not the same as resetting your class!) and be refunded (some of) the skill points you spent so that you can invest them differently.\n\nYou have <color=#ff912b>" + batCount + "</color>/<color=#ff912b>" + money_required + "</color> batteries, you can respec by typing: \"" + "<color=yellow>/</color><color=#4da6ff>skt respec <color=red>now</color></color>\".\nIf you respec, you will receive: <color=#ff912b>" + pointsSpentAdj + "</color>/<color=#ff912b>" + pointsSpent + "</color> points that you have spent.\n\nPlease make sure you're okay with the above before respecing, as we are unlikely to give any refunds on points spent/lost/respeced!";

                        if (!(luckInfo?.HasUsedFreeReset ?? true)) 
                            respecStr += "\nYou have <color=#ff912b>1</color> <color=#8aff47>free</color> reset you can use, but after this if you wish to reset again you'll have to pay!";

                        SendReply(player, respecStr);
                    }
                    else if (args.Length >= 2)
                    {
                        if (arg0Lower == "respec" && args[1].ToLower() == "now")
                        {
                            if (pointsSpent < 1 || pointsSpentAdj < 1)
                            {
                                SendReply(player, "You have not invested any points!");
                                return;
                            }
                            if (luckInfo?.HasUsedFreeReset ?? true)
                            {
                                if (batCount < money_required)
                                {
                                    SendReply(player, "You do not have enough batteries for this, you need: <color=#ff912b>" + money_required + "</color>. You only have: <color=#ff912b>" + batCount + "</color>");
                                    return;
                                }

                                player.inventory.Take(null, BATTERY_ID, money_required);

                                NoteItemByID(player, BATTERY_ID, -money_required);
                            }
                            else luckInfo.HasUsedFreeReset = true;


                            luckInfo.ResetAllStats();

                            luckInfo.Spent = 0;
                           
                            luckInfo.SkillPoints += pointsSpentAdj;



                            saveUser(player.UserIDString);
                            SendReply(player, "You have reset your skills! " + "You now have: " + getLKSKP(player.userID) + " points!");
                            wasCorrectArg = true;
                        }
                    }

                    if (wasCorrectArg) return;
                    //else: it wasn't correct arg, so we try to find a stat with a name matching the arg & upgrade it

                    var stat = FindStatInfoByName(arg0Lower);
                    if (stat == null)
                    {
                        SendReply(player, "Invalid argument(s) supplied: \"" + string.Join(" ", args) + "\" Try: /<color=" + LUCK_COLOR + ">skt gui</color>");
                        return;
                    }

                    wasCorrectArg = true;

                    var canUpgradeStr = CanUpgradeStat(player, luckInfo, stat)?.ToString() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(canUpgradeStr))
                    {
                        SendReply(player, canUpgradeStr);
                        return;
                    }

                    var curStatLvl = luckInfo.GetStatLevel(stat, false);

                    var upgradeCostNextLvl = luckInfo.GetStatUpgradeCost(stat, curStatLvl + 1);


                    luckInfo.SpendPoints(upgradeCostNextLvl);
                    luckInfo.AddStatLevel(stat, 1);

                    //move this elsewhere at some point, probably (mastery achievement giving):

                    if ((curStatLvl + 1) >= stat.MaxLevel)
                        GrantMasteryAchievement(player, stat);
                    


                    SendReply(player, string.Format(GetMessage("upgradeSuccess", player.UserIDString), stat.DisplayName, (curStatLvl + 1).ToString("N0"), stat.MaxLevel.ToString("N0")));
                    SendReply(player, string.Format(GetMessage("pointsSpent", player.UserIDString), upgradeCostNextLvl.ToString("N0"), luckInfo.SkillPoints));

                }
                finally
                {
                    if (wasCorrectArg && _curTree.TryGetValue(player.userID, out SkillTree current) && current != SkillTree.Undefined)
                        SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");
                }


            }
        }

        private StatInfo FindStatInfoByName(string shortOrDisplayName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(shortOrDisplayName)) throw new ArgumentNullException(nameof(shortOrDisplayName));

            var matchingStats = Pool.GetList<StatInfo>();
            try
            {
                for (int i = 0; i < _statInfos.Count; i++)
                {
                    var info = _statInfos[i];

                    if (info.ShortName.Equals(shortOrDisplayName, comparison) || info.DisplayName.Equals(shortOrDisplayName, comparison))
                        return info;
                    else if (info.ShortName.IndexOf(shortOrDisplayName, comparison) >= 0 || info.DisplayName.IndexOf(shortOrDisplayName, comparison) >= 0)
                        matchingStats.Add(info);

                }

                if (matchingStats.Count > 1) PrintWarning("found more than one stat matching name: " + shortOrDisplayName);
                else if (matchingStats.Count < 1) PrintWarning("no stat found matching short/display name: " + shortOrDisplayName);

                return matchingStats.Count == 1 ? matchingStats[0] : null;
            }
            finally { Pool.FreeList(ref matchingStats); }

        }
        /// <summary>
        /// Uses RelationshipManager's FindByID method (which has a cache) to find player.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByID(ulong userID)
        {
            return RelationshipManager.FindByID(userID); //relationshipmanager has a cache lol
        }

        private BasePlayer FindPlayerByIdS(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) 
                throw new ArgumentNullException(nameof(userId));


            return ulong.TryParse(userId, out ulong ulUserId) ? FindPlayerByID(ulUserId) : null;
        }

        private void SendTopLuckTextS(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            SendTopLuckText(covalence.Players.FindPlayerById(userId));
        }

        private void SendTopLuckText(IPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var top5 = GetTop5S();
            var topLuckSB = new StringBuilder();
            for (int i = 0; i < top5.Count; i++)
            {
                var topId = top5[i];
                topLuckSB.AppendLine("<color=" + LUCK_COLOR + ">" + GetDisplayNameFromID(topId) + "</color>: " + getLevelS(topId).ToString("N0") + " (XP: " + getPointsS(topId).ToString("N0") + ")");
            }
            SendReply(player, "-<color=" + LUCK_COLOR + "><size=19>Luck</size></color>-" + Environment.NewLine + Environment.NewLine + topLuckSB.ToString().TrimEnd());
        }

        private void cmdTopLuck(IPlayer player, string command, string[] args) => SendTopLuckText(player);


        private TimeCachedValue<List<ulong>> _top5LevelIds = null;
        private TimeCachedValue<List<string>> _top5LevelIdsS = null;

        private List<ulong> GetTop5(bool skipCache = false)
        {

            _top5LevelIds ??= new TimeCachedValue<List<ulong>>()
                {
                    refreshCooldown = 90f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<List<ulong>>(() =>
                    {
                        if (storedData3?.luckInfos == null || storedData3.luckInfos.Count < 1) 
                            return null;

                        var xps = Pool.Get<Dictionary<ulong, float>>();
                        try
                        {
                            xps.Clear();

                            foreach (var kvp in storedData3.luckInfos)
                            {
                                if (!ulong.TryParse(kvp.Key, out ulong UID)) continue;
                                var XP = getPoints(UID);
                                if (XP > 0.0f) xps[UID] = XP;
                            }
                            if (xps.Count < 1) return null;

                            var list = new List<ulong>(5);
                            var descendingTop5 = xps.OrderByDescending(p => p.Value).Take(xps.Count > 4 ? 5 : xps.Count);
                            foreach (var kvp in descendingTop5) list.Add(kvp.Key);

                            return list;
                        }
                        finally { Pool.Free(ref xps); }
                    })
                };

            return _top5LevelIds.Get(skipCache);
        }

        private List<string> GetTop5S(params string[] ignoreIds)
        {
            if (storedData3?.luckInfos == null || storedData3.luckInfos.Count < 1) return null;
            var xps = Pool.Get<Dictionary<string, float>>();
            try
            {
                xps.Clear();

                foreach (var kvp in storedData3.luckInfos)
                {
                    var XP = getPointsS(kvp.Key);
                    if (XP > 0.0f && !ignoreIds.Contains(kvp.Key)) xps[kvp.Key] = XP;
                }
                if (xps.Count < 1) return null;

                var list = new List<string>(5);
                var descendingTop5 = xps.OrderByDescending(p => p.Value).Take(xps.Count > 4 ? 5 : xps.Count);
                foreach (var kvp in descendingTop5) list.Add(kvp.Key);

                return list;
            }
            finally { Pool.Free(ref xps); }
        }

        private List<ulong> GetTop5(params ulong[] ignoreIds)
        {
            if (storedData3?.luckInfos == null || storedData3.luckInfos.Count < 1) return null;
            var xps = Pool.Get<Dictionary<ulong, float>>();
            try
            {
                xps.Clear();

                foreach (var kvp in storedData3.luckInfos)
                {
                    if (!ulong.TryParse(kvp.Key, out ulong UID)) continue;
                    var XP = getPoints(UID);
                    if (XP > 0.0f && !ignoreIds.Contains(UID)) xps[UID] = XP;
                }
                if (xps.Count < 1) return null;

                var list = new List<ulong>(5);
                var descendingTop5 = xps.OrderByDescending(p => p.Value).Take(xps.Count > 4 ? 5 : xps.Count);
                foreach (var kvp in descendingTop5) list.Add(kvp.Key);

                return list;
            }
            finally { Pool.Free(ref xps); }
        }

        private void GetTop5NoAlloc(ref List<ulong> list, params ulong[] ignoreIds)
        {
            var xps = Pool.Get<Dictionary<ulong, float>>();
            try
            {
                xps.Clear();

                foreach (var kvp in storedData3.luckInfos)
                {
                    if (!ulong.TryParse(kvp.Key, out ulong UID)) continue;
                    var XP = getPoints(UID);
                    if (XP > 0.0f && !ignoreIds.Contains(UID)) xps[UID] = XP;
                }
                if (xps.Count < 1) return;

                var descendingTop5 = xps.OrderByDescending(p => p.Value).Take(xps.Count > 4 ? 5 : xps.Count);
                foreach (var kvp in descendingTop5) list.Add(kvp.Key);


            }
            finally { Pool.Free(ref xps); }
        }

        private List<string> GetTop5S()
        {
            if (storedData3?.luckInfos == null || storedData3.luckInfos.Count < 1) return null;
            var xps = Pool.Get<Dictionary<string, float>>();
            try
            {
                xps.Clear();

                foreach (var kvp in storedData3.luckInfos)
                {
                    var XP = getPointsS(kvp.Key);
                    if (XP > 0.0f) xps[kvp.Key] = XP;
                }
                if (xps.Count < 1) return null;

                var list = new List<string>(5);
                var descendingTop5 = xps.OrderByDescending(p => p.Value).Take(xps.Count > 4 ? 5 : xps.Count);
                foreach (var kvp in descendingTop5) list.Add(kvp.Key);

                return list;
            }
            finally { Pool.Free(ref xps); }
        }

        private string GetDisplayNameFromID(string userID) { return string.IsNullOrEmpty(userID) ? string.Empty : covalence.Players?.FindPlayerById(userID)?.Name ?? string.Empty; }


        [ConsoleCommand("luck.transfer")]
        private void cmdLuckTrasnfer(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin)
            {
                SendReply(arg, "No permission!");
                return;
            }
            var args = arg?.Args ?? null;
            if (args.Length < 2 || args == null)
            {
                SendReply(arg, "You must supply a steam ID to trasnfer from and to.");
                return;
            }

            if (!ulong.TryParse(args[0], out ulong sourceID))
            {
                SendReply(arg, "Source ID is not a valid ulong: " + args[0]);
                return;
            }

            if (!ulong.TryParse(args[1], out ulong targetID))
            {
                SendReply(arg, "Target ID is not a valid ulong: " + args[1]);
                return;
            }
            if (sourceID == targetID)
            {
                SendReply(arg, "You cannot transfer from and to the same account!");
                return;
            }
            var info = GetLuckInfo(sourceID);
            if (info == null)
            {
                SendReply(arg, "Source ID has no luck info!");
                return;
            }

            SetLuckInfo(targetID, info);
            var getInfo = GetLuckInfo(targetID);
            if (getInfo != null) getInfo.UserID = targetID;
            SendReply(arg, "Set target's info to source info");
        }

        [ConsoleCommand("luck.getlvl")]
        private void lkGetLevel(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin)
            {
                SendReply(arg, "No permission!");
                return;
            }
            var args = arg?.Args ?? null;
            if (args.Length < 1 || args == null)
            {
                SendReply(arg, "You must supply a valid target name, id or IP!");
                return;
            }
            var targetC = covalence.Players.FindPlayer(args[0]);
            if (targetC == null)
            {
                SendReply(arg, "Failed to find a player with the name, id or IP: " + args[0]);
                return;
            }
            if (!ulong.TryParse(targetC.Id, out ulong uID))
            {
                SendReply(arg, "Failed to parse: " + targetC.Id);
                return;
            }
            var level = getLevel(uID);
            SendReply(arg, GetDisplayNameFromID(targetC.Id) + "'s luck is: " + level);
        }

        [ConsoleCommand("luck.reset")]
        private void lkResetLevel(ConsoleSystem.Arg arg)
        {
            var userId = arg?.Player()?.userID ?? 0;

            if (arg.Args != null && arg.Args.Length > 0 && !ulong.TryParse(arg.Args[0], out userId))
            {
                SendReply(arg, "Not an ID!: " + arg.Args[0]);
                return;
            }

            if (userId == 0)
            {
                SendReply(arg, "No userId!!!");
                return;
            }

            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin && !arg.IsAdmin)
            {
                SendReply(arg, "No permission!");
                return;
            }

            storedData3.luckInfos[userId.ToString()] = new LuckInfo(player);
            UpdateGUI(player);
        }

        [ConsoleCommand("luck.stopsktgui")]
        private void lkStopSktGUI(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null)
                return;

            StopSKTGUI(player);
            SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");
        }

        [ConsoleCommand("luck.backpacktoggle")]
        private void CmdToggleBackpackAutoDeposit(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player();

            if (player == null)
                return;

            var state = ToggleBackpackAutoDeposit(player.UserIDString);

            BackpackDepositGUI(player);

            SendBackpackDepositMessage(player, state);

        }

        [ConsoleCommand("luck.takehit")]
        private void cmdLuckTakeHit(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead()) return;

            if (arg?.Args == null || arg.Args.Length < 1) return;

            NPCHitTarget target = null;

            if (!ulong.TryParse(arg.Args[0], out ulong netID))
            {
                PrintWarning("not netID: " + arg.Args[0]);
                return;
            }

            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var npc = entity as HumanNPC;
                if (npc?.net == null || npc.net.ID.Value != netID) continue;

                var hit = npc.GetComponent<NPCHitTarget>();
                if (hit != null)
                {
                    target = hit;
                    break;
                }

            }


            StopHitGUI(player);
            SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");

            var comp = player?.GetComponent<MercenaryHitHandler>() ?? player?.gameObject?.AddComponent<MercenaryHitHandler>();

            comp.SetNPCTarget(target.NPC);

            SendLocalEffect(player, "assets/prefabs/missions/effects/mission_victory.prefab");
            SendReply(player, "Alright, friend. Your target goes by the name " + comp.TargetInfo.FlavorNickName + " - now, I don't know if that's their real name. Quite frankly, I don't care.\nGo find the person. Kill them. Bring the tags back to me. Got it? Good.\nI pinged their last known location on your map. If the circle's red, that means they were seen underground. Blue? Above ground.\nNow get out of here.");
        }

        [ConsoleCommand("luck.stoplegendarygui")]
        private void lkStopLegGUI(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null)
            {
                StopLegendaryGUI(player);
                SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");
            }

        }

        [ConsoleCommand("luck.stophitgui")]
        private void lkStopHitGUI(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null)
            {
                StopHitGUI(player);
                SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");
            }

        }

        [ConsoleCommand("luck.stopclassgui")]
        private void lkStopClassGUI(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player();
            if (player == null || !player.IsConnected) return;

            var userClass = GetUserClassType(player.UserIDString);

            float timeDiff;
            if (!player.IsAdmin && userClass == ClassType.Undefined && _classGuiOnWakeTime.TryGetValue(player.UserIDString, out float time) && (timeDiff = Time.realtimeSinceStartup - time) < 15)
            {
                PrintWarning("has been open for less than 15 seconds, return (wake)");
                SendLocalEffect(player, "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab");

                //placeholder colors i guess lol
                ClassGUILegacy(player, "<color=red>You must wait <color=green>15</color> seconds before closing this</color>. <color=orange>Please consider picking a class now</color>.\nYou've waited: <color=blue>" + timeDiff.ToString("N0") + "</color>/15 seconds");

                var classAction = new Action(() => //lawsuit? lol
                {
                    if (player == null || !player.IsConnected || player.IsDead()) return;

                    ClassGUILegacy(player);
                });

                if (_classGuiAction.TryGetValue(player.UserIDString, out Action oldAction)) player.CancelInvoke(oldAction);

                player.Invoke(classAction, 3.5f);

                _classGuiAction[player.UserIDString] = classAction;

                return;
            }

            SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab"); //click fx

            if (_classGuiStartTime.TryGetValue(player.UserIDString, out time) && (Time.realtimeSinceStartup - time) < 1.5)
            {
                PrintWarning("has been open for less than 1.5 seconds, return (generic)");
                return;
            }

            StopClassGUI(player);
        }

        [ConsoleCommand("luck.stoptokengui")]
        private void lkStopTokenGUI(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null) StopTokenGUI(player);
        }

        [ConsoleCommand("luck.transmute")]
        private void lkTransmute(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null) return;


            PrintWarning("got player from command for luck.transmute");

            var lootable = (StorageContainer)player?.inventory?.loot?.entitySource;

            if (lootable == null)
            {
                PrintWarning("Lootable null from command (transmute)!!!");
                return;
            }

            if (_transmuteCoroutine.TryGetValue(player.UserIDString, out _))
            {
                SendReply(player, "Transmutation already in progress!");
                return;
            }

            if (!HasRequiredItemsForTransmutation(lootable.inventory))
            {
                SendReply(player, "The container does not have the required items to transmute!");
                return;
            }

            PrintWarning("starting coroutine (transmute)");

            StopTokenGUI(player);

            var routine = StartCoroutine(TokenTransmutation(player, lootable));
            _transmuteCoroutine[player.UserIDString] = routine;
        }

        //move this:
        private readonly Dictionary<NetworkableId, List<ItemAmount>> _temporaryTransmuteItems = new();

        private IEnumerator TokenTransmutation(BasePlayer player, BaseEntity lootable)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (lootable == null)
                throw new ArgumentNullException(nameof(lootable));

            var storageContainer = lootable as StorageContainer;

            var inventory = storageContainer?.inventory;

            if (inventory == null)
            {
                PrintError(nameof(inventory) + " is null!!! on " + nameof(TokenTransmutation));
                yield break;
            }

            if (!HasRequiredItemsForTransmutation(inventory))
            {
                PrintWarning(nameof(TokenTransmutation) + " does not have required items!!");
                yield break;
            }

            if (!_temporaryTransmuteItems.TryGetValue(lootable.net.ID, out List<ItemAmount> tempItems))
            {
                _temporaryTransmuteItems[lootable.net.ID] = tempItems = new List<ItemAmount>();

                for (int i = 0; i < inventory.itemList.Count; i++)
                {
                    var item = inventory.itemList[i];

                    var itemAmount = new ItemAmount(item.info, item.amount);

                    _temporaryTransmuteItems[lootable.net.ID].Add(itemAmount);
                }
            }
            else PrintWarning("had temporary transmute items for this lootable already?!?!?");

            inventory.SetLocked(true);

            ShowTextAboveInventory(player, "Transmuting...", 28);

            var itemList = inventory?.itemList;

            //Effect.server.Run("assets/prefabs/deployable/dropbox/effects/dropbox-deploy.prefab", player.transform.position, Vector3.zero);

            player.Server_CancelGesture();
            player.Server_StartGesture(StringToGesture(player, "shrug"));


            yield return CoroutineEx.waitForSeconds(1.5f);

            SendLocalEffect(player, "assets/bundled/prefabs/fx/player/beartrap_scream.prefab");

            PrintWarning("start itemList loop");


            var count = itemList.Count - 1;

            var takenAmounts = new Dictionary<int, int>();

            //what a mess below lol

            while (count >= 0)
            {
                try
                {
                    var item = itemList[count];

                    var takenAmount = 0;
                    var takenDictionaryAmt = 0;
                    var originalAmount = item.amount;

                    while (item.amount > 1)
                    {
                        if (!takenAmounts.TryGetValue(item.info.itemid, out takenDictionaryAmt)) takenAmounts[item.info.itemid] = 1;
                        else
                        {
                            if (takenDictionaryAmt >= 3)
                            {
                                PrintWarning("already took at least 3");
                                break;
                            }

                            takenAmounts[item.info.itemid] = takenDictionaryAmt += 1;
                        }
                        

                        PrintWarning("item.amount: " + item.amount + " for " + item.info.shortname + " takenAmount: " + takenAmount);


                        takenAmount++;

                        SendLocalEffect(player, "assets/prefabs/food/bota bag/effects/bota-bag-deploy.prefab");

                        yield return CoroutineEx.waitForSecondsRealtime(0.1f);

                        item.amount--;
                        item.MarkDirty();


                        var fxToRun = "assets/prefabs/food/bota bag/effects/bota-bag-cork-squeak.prefab";
                        SendLocalEffect(player, fxToRun);

                        yield return CoroutineEx.waitForSecondsRealtime(0.33f);
                    }

                    if (originalAmount <= 3)
                    {
                        PrintWarning("item amount no longer > 1: " + item.amount + " time to remove!");

                        PrintWarning("removing: " + item.info.shortname);

                        RemoveFromWorld(item);
                        SendLocalEffect(player, "assets/bundled/prefabs/fx/item_break.prefab");
                    }




                    yield return CoroutineEx.waitForSecondsRealtime(0.75f);

                }
                finally { count--; }
            }


            _temporaryTransmuteItems[lootable.net.ID].Clear();

            if (!_temporaryTransmuteItems.Remove(lootable.net.ID)) PrintWarning("Couldn't remove from temporary transmute items!!!");
            else PrintWarning("removed from temporary transmute items");


            try
            {
                //Effect.server.Run("assets/prefabs/deployable/dropbox/effects/submit_items.prefab", cratePos);

                player.GiveItem(CreateInactiveTokenOfAbsolution());

                SendLocalEffect(player, "assets/bundled/prefabs/fx/item_unlock.prefab");
                ShowTextAboveInventory(player, "You've created a Token of Absolution!\nClick 'Unwrap' to activate it. Unwrap it again to reset your class.\n*IT WILL RESET ALL CLASS-SPECIFIC SKILLS TO LEVEL 1: MINING IF YOU'RE A MINER, WOODCUTTING IF YOU'RE A LUMBERJACK, ETC.\nALSO RESETS CLASS ABILITIES, SUCH AS WHIRLWIND.*", 20);

            }
            finally
            {
                PrintWarning("finally");
                // if (player?.inventory?.loot?.entitySource == lootable) //could have switched to another container where it will now show cooldown on magpie instead - no need to destroy
                //     StopTokenGUI(player);
            }

            player.Server_CancelGesture();
            player.Server_StartGesture(StringToGesture(player, "victory"));

            yield return CoroutineEx.waitForSecondsRealtime(1f);

            inventory.SetLocked(false);

            yield return null;
        }

        [ConsoleCommand("luck.stopmagpiegui")]
        private void lkStopMagpieGUI(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null) StopMagpieGUI(player);
        }

        [ConsoleCommand("luck.magpiereroll")]
        private void lkMagpieReroll(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null) return;

            var cooldown = player?.GetComponent<MagpieCooldownHandler>();
            if (cooldown == null)
            {
                PrintWarning("No cooldown handler found for player: " + player);
                return;
            }

            if (cooldown.IsOnCooldown)
            {
                MagpieGUI(player, GetMagpieGUIText(player));
                return;
            }

            //MAGPIE LEVEL CHECK!!!

            PrintWarning("got player from command");

            var lootable = player?.inventory?.loot?.entitySource;

            if (lootable == null)
            {
                PrintWarning("Lootable null from command (magpie)!!!");
                return;
            }

            if (_lootableItemsRemoved.Contains(lootable.net.ID))
            {
                PrintWarning("already looted!!! return magpie reroll");
                StopMagpieGUI(player);
                return;
            }

            AddMagpieLooted(player.userID, lootable.net.ID);

            /*/
            * assets/prefabs/deployable/dropbox/effects/dropbox-deploy.prefab
assets/prefabs/deployable/dropbox/effects/submit_items.prefab
             */


            PrintWarning("starting coroutine");

            StopMagpieGUI(player);

            StartCoroutine(MagpieReroll(player, lootable));

            PrintWarning("starting magpie cooldown action");

            if (_magpieCooldownStartAction.TryGetValue(player.UserIDString, out Action cooldownStartAction))
            {
                PrintWarning("got cooldown start action already?");

                player.CancelInvoke(cooldownStartAction);

                _magpieCooldownStartAction.Remove(player.UserIDString);

                PrintWarning("canceled & removed");
            }
        }

        private IEnumerator MagpieReroll(BasePlayer player, BaseEntity lootable)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (lootable == null)
                throw new ArgumentNullException(nameof(lootable));

            if (lootable?.net == null)
                throw new ArgumentNullException(nameof(lootable.net));

            if (lootable.IsDestroyed)
            {
                Interface.Oxide.LogError(nameof(lootable) + " is destroyed!");
                yield break;
            }

            var magpieLevel = GetLuckInfo(player)?.GetStatLevelByName("Magpie");
            if (magpieLevel < 1)
            {
                PrintWarning("magpie level < 1 on magpiereroll!!!");
                yield break;
            }


            var inventory = (lootable as LootContainer)?.inventory ?? (lootable as DroppedItemContainer)?.inventory ?? (lootable as NPCPlayerCorpse)?.containers[0];

            if (inventory == null)
            {
                PrintError(nameof(inventory) + " is null!!! on " + nameof(MagpieReroll) + " " + player + " " + lootable);
                yield break;
            }

            if (inventory?.itemList == null)
            {
                PrintError(nameof(inventory) + " itemList is null!!!");
                yield break;
            }

            if (inventory.itemList.Count < 1)
            {
                PrintError(nameof(inventory) + " itemList count < 1!!!!");
                yield break;
            }

            var crateNetId = lootable.net.ID;

            try 
            {
                if (!_cratesBeingRerolled.Add(crateNetId))
                {
                    PrintError(nameof(_cratesBeingRerolled) + " already had " + nameof(crateNetId) + "!!!");
                    yield break;
                }

                var userId = player?.UserIDString;

                inventory.SetLocked(true);

                var itemList = inventory?.itemList;

                var cratePos = lootable?.transform?.position ?? Vector3.zero;

                Effect.server.Run("assets/prefabs/deployable/dropbox/effects/dropbox-deploy.prefab", cratePos, Vector3.zero);

                player.Server_CancelGesture();
                player.Server_StartGesture(StringToGesture(player, "thumbsdown"));

                PrintWarning("start itemList loop");

                var count = itemList.Count - 1;

                while (count >= 0)
                {
                    try
                    {
                        try
                        {
                            if (player?.inventory?.loot?.entitySource == lootable)
                            {
                                var fxToRun = Random.Range(0, 101) <= 50 ? "assets/bundled/prefabs/fx/notice/loot.copy.fx.prefab" : "assets/bundled/prefabs/fx/notice/item.select.fx.prefab";
                                SendLocalEffect(player, fxToRun);
                            }

                            RemoveFromWorld(itemList[count]);
                        }
                        catch(Exception ex) { PrintError(ex.ToString()); }

                        yield return CoroutineEx.waitForSecondsRealtime(0.5f);
                    }
                    finally { count--; }
                }

                PrintWarning("end itemList loop");

                //trying to track down mysterious nre
                if (inventory?.itemList == null)
                {
                    Interface.Oxide.LogError(nameof(inventory.itemList) + " is now null!");
                    yield break;
                }

                try
                {
                    Effect.server.Run("assets/prefabs/deployable/dropbox/effects/submit_items.prefab", cratePos);

                    PrintWarning("run SpawnLoot/check NPC");

                    if (lootable is LootContainer lootContainer)
                    {
                        lootContainer.SpawnLoot();

                        _lootedEnts.Remove(lootContainer.net.ID);

                        if (_playerLootedEnts.TryGetValue(player.userID, out _))
                            _playerLootedEnts[player.userID].Remove(lootContainer.net.ID);

                        if (player?.inventory?.loot?.entitySource == lootContainer)
                            OnLootEntity(player, lootContainer);
                    }
                    else
                    {
                        HumanNPC randomNpc = null;

                        //some hacky workarounds are required to re-spawn npc body loot

                        PrintWarning("npc body loot attempt respawn.");

                        var npcCorpse = lootable as NPCPlayerCorpse;

                        var dropContainer = lootable as DroppedItemContainer;

                        if ((npcCorpse != null && _corpseToAlivePrefabName.TryGetValue(npcCorpse.net.ID, out var aliveNpcName)) || (dropContainer != null && _scientistSteamIdToPrefabName.TryGetValue(dropContainer.playerSteamID, out aliveNpcName)))
                        {
                            var dummyScientist = GameManager.server.CreateEntity(aliveNpcName, Vector3.zero);
                            if (dummyScientist == null) PrintError("dummyScientist is null!!! name: " + aliveNpcName);
                            else
                            {
                                dummyScientist.Spawn();

                                randomNpc = dummyScientist as HumanNPC;
                                if (randomNpc?.LootSpawnSlots == null || randomNpc.LootSpawnSlots.Length < 1) randomNpc = null;

                                dummyScientist?.Invoke(() =>
                                {
                                    if (dummyScientist != null && !dummyScientist.IsDestroyed) dummyScientist.Kill();
                                    else PrintWarning("couldn't kill!!");
                                }, 0.5f);
                            }
                        }


                        if (randomNpc == null)
                        {
                            PrintWarning("null after corpse prefab check! finding random corpse");

                            foreach (var entity in BaseNetworkable.serverEntities)
                            {
                                if (entity is not HumanNPC npc || npc.IsDestroyed)
                                    continue;

                                if (npc?.LootSpawnSlots?.Length > 0)
                                {
                                    randomNpc = npc;
                                    break;
                                }

                            }
                        }


                        if (randomNpc?.LootSpawnSlots == null || randomNpc.LootSpawnSlots.Length < 1)
                        {
                            PrintError("no npc found to respawn corpse!!!!!");
                            yield break;
                        }

                        var lootSpawn = randomNpc.LootSpawnSlots;

                        for (int i = 0; i < lootSpawn.Length; i++)
                        {
                            var spawn = lootSpawn[i];

                            for (int k = 0; k < spawn.numberToSpawn; k++)
                                if (Random.Range(0.0f, 1f) <= spawn.probability)
                                    spawn.definition.SpawnIntoContainer(npcCorpse != null ? npcCorpse.containers[0] : dropContainer.inventory);
                        }
                    }
                    

                    var magpieCharmRng = 1.5 * magpieLevel;

                    if (inventory.itemList.Count < inventory.capacity && Random.Range(0, 101) <= magpieCharmRng)
                    {
                        PrintWarning("magpie hit rng for charm spawn!");
                        var charm = CreateRandomCharm(charmTypeScalar: 0.75f);
                        if (!charm.MoveToContainer(inventory))
                        {
                            PrintWarning("created charm but can't move!!!");
                            RemoveFromWorld(charm);
                        }
                        else SendLuckNotifications(player, "Your <i>Magpie</i> ability has caused a Charm to spawn in the loot container!");
                    }
                }
                finally
                {
                    if (player?.inventory?.loot?.entitySource == lootable || player?.inventory?.loot?.entitySource == null) //could have switched to another container where it will now show cooldown on magpie instead - no need to destroy - could also no longer be looting, in which case, we need to destroy lol
                        StopMagpieGUI(player);
                }

                PrintWarning("gesturing");

                try 
                {
                    player.Server_CancelGesture();
                    player.Server_StartGesture(StringToGesture(player, "thumbsup"));
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
                

                yield return CoroutineEx.waitForSecondsRealtime(1f);

                inventory?.SetLocked(false);

                var cooldown = player?.GetComponent<MagpieCooldownHandler>();
                if (cooldown == null)
                {
                    PrintWarning("No cooldown handler found for player on Magpie coroutine: " + player);
                    yield return null;
                }

                Action cooldownStartAction = null;

                try 
                {
                    _magpieCooldownStartAction[userId] = cooldownStartAction = new Action(() =>
                    {
                        try
                        {
                            _magpieCooldownStartAction.Remove(userId);

                            if (player == null || player.IsDestroyed || player.gameObject == null || cooldown == null) 
                                return;

                            cooldown.StartCooldown();
                        }
                        catch(Exception ex) { PrintError(ex.ToString()); }
                    });
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
                

                try
                {
                    if (player?.inventory?.loot?.entitySource == lootable && lootable != null)
                    {
                        SendReply(player, "<color=yellow>Keep looting</color><color=red>!</color> You have <color=red>9</color> seconds to loot another container and re-roll it before your cooldown starts.This chain-looting can be <color=orange><i>stacked infinitely</i></color>, so <size=18><color=red>keep re-rolling</color><color=yellow>!</color></size>");
                        MagpieGUI(player, "<color=yellow>Chain-loot!</color> <color=orange>Re-roll more crates</color><color=red>!</color>"); //placeholder/temp colors?
                    }


                    player.Invoke(cooldownStartAction, 9f);
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

            }
            finally { _cratesBeingRerolled.Remove(crateNetId); }

            yield return null;
        }

        private void OnSendPunch(HeldEntity entity, ref Vector3 amount, ref float duration)
        {
            var player = entity?.GetOwnerPlayer();
            if (player == null || player.IsDestroyed || player.IsDead()) return;

            if (HasWOTWActive(player) && Random.Range(0, 101) <= 85)
            {
                duration = 0f;
                amount = Vector3.zero;
            }
        }

        private void OnRunPlayerMetabolism(PlayerMetabolism metabolism, BasePlayer player, float delta)
        {

            if (player.IsRunning() && IsWearingSkin(player, NIMBLESTRIDERS_SKIN_ID))
            {
                metabolism.calories.Add(Random.Range(0.265f, 0.465f));
                metabolism.hydration.Add(Random.Range(0.14f, 0.26f));
            }

            if (metabolism.radiation_poison.value < 100)
                return;
            

            var hasGeiger = (GetActiveItem(player)?.info?.itemid ?? 0) == GEIGER_COUNTER_ITEM_ID;
            if (!hasGeiger)
                return;
            

            metabolism.Invoke(() =>
            {
                player?.SetPlayerFlag(BasePlayer.PlayerFlags.NoSprint, false);
            }, 0.01f);
        }

        private readonly Dictionary<BasePlayer, TimeCachedValue<TriggerRadiation>> _maxRadTriggerCache = new();

        private TriggerRadiation GetMaxRadiationTrigger(BasePlayer player, bool skipCache = false)
        {
            if (player == null || player.IsDestroyed || player.IsDead())
                return null;


            if (!_maxRadTriggerCache.TryGetValue(player, out var maxRad))
                _maxRadTriggerCache[player] = new TimeCachedValue<TriggerRadiation>()
                {
                    refreshCooldown = 5f,
                    refreshRandomRange = 2.5f,
                    updateValue = new Func<TriggerRadiation>(() =>
                    {
                        if (player?.triggers == null || player.triggers.Count < 1)
                            return null;

                        TriggerRadiation maxRad = null;

                        for (int i = 0; i < player.triggers.Count; i++)
                        {
                            var radTrig = player.triggers[i] as TriggerRadiation;

                            if (radTrig != null && (maxRad == null || radTrig.radiationTier > maxRad.radiationTier))
                                maxRad = radTrig;
                        }

                        return maxRad;
                    })
                };



            return _maxRadTriggerCache[player].Get(skipCache);
        }

        private void OnMovementSpeedBonus(BasePlayer player, InputState input, ref float moveScale)
        {

            var watch = Pool.Get<Stopwatch>();

            try 
            {
                watch.Restart();

                if (IsWearingSkin(player, NIMBLESTRIDERS_SKIN_ID))
                    moveScale += 1.08f;

                if (whirlwindPlayers.Contains(player.UserIDString) && IsWearingSkin(player, MUDSLINGERS_SKIN_ID) && player.IsRunning())
                    moveScale += 2.2f + (0.1f * (GetLuckInfo(player).GetStatLevelByName("Whirlwind") - 1));

                var held = GetActiveItem(player); //optimize using dic cache
                if (held?.info == null)
                    return;

                if (IsLightweightGun(held))
                {
                    var info = GetLuckInfo(player);

                    var lightStat = info?.GetStatInfoByName("SwiftGunner");
                    if (info != null && lightStat != null && info.GetStatLevel(lightStat) >= lightStat.MaxLevel)
                        moveScale += player.IsAiming ? 2.2f : 1.5f;
                }

                if (held.info.itemid != GEIGER_COUNTER_ITEM_ID)
                    return;

                if (player?.triggers == null || player.triggers.Count < 1)
                    return;

                //below can be optimized as well, maybe dic cache?

                var maxRad = GetMaxRadiationTrigger(player);
                if (maxRad == null)
                    return;

                var scale = 0.75f + player.metabolism.radiation_poison.value * 0.01986f; //(maxRad.radiationTier <= TriggerRadiation.RadiationTier.MINIMAL ? 0 : ((int)maxRad.radiationTier * 0.2f));

                //TEMP FOR DEBUG:
                //var scale = 10f;

                if (scale <= 0f)
                    return;

                if (player.IsDucked() || input.IsDown(BUTTON.DUCK))
                    scale *= 0.33f;
                else if (!player.IsRunning())
                    scale *= 0.4f;

                moveScale += scale;

            }
            finally
            {
                try { if (watch.Elapsed.TotalMilliseconds > 1) PrintWarning(nameof(OnMovementSpeedBonus) + " took: " + watch.Elapsed.TotalMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }

            
        }

        private IEnumerator WeightOfTheWorldCoroutine(BasePlayer player, List<OreResourceEntity> ores, bool freeList = false)
        {
            try
            {
                if (player == null) 
                    throw new ArgumentNullException(nameof(player));


                var wotwCooldown = player?.GetComponent<WeightOfTheWorldCooldownHandler>();

                var eyesPos = player?.eyes?.transform?.position ?? Vector3.zero;

                var plyPos = player?.transform?.position ?? Vector3.zero;

                var weapon = player?.GetHeldEntity() as AttackEntity;
                if (weapon == null)
                {
                    PrintWarning("weapon null on wotw!!!");
                    yield break;
                }

                var originalHeldItem = player?.GetActiveItem();

                var hadNearEnts = false;

                var hasAesir = IsWearingSkin(player, AESIRS_JACKET_SKIN_ID);

                try
                {

                    if (ores.Count < 1)
                    {
                        Interface.Oxide.LogWarning("no near ents!! yield break test");
                        yield break;
                    }
                    else hadNearEnts = true;


                    player.SendConsoleCommand("gametip.hidegametip");

                    SendReply(player, "<color=#9E4122><size=26>You begin to channel the spirit of the ancients...</color>");

                    if (HasFusionSkill(player, "WeightOfTheWorld"))
                    {
                        PrintWarning("is fusion wotw!: " + player);

                        var fusionRng = Random.Range(0, 101);

                        var fusionLvl = GetLuckInfo(player)?.GetStatLevelByName("Fusion") ?? 0;

                        var neededRng = fusionLvl == 1 ? 33 : fusionLvl == 2 ? 16 : 8;

                        if (fusionRng <= neededRng || player.IsAdmin)
                        {
                            var fusionItems = Pool.GetList<Item>();
                            try 
                            {

                                var scalar = Mathf.Clamp(0.33f + (0.166f * fusionLvl), 0.1f, 0.95f);

                                var hqTaken = player.inventory.Take(fusionItems, HQ_ORE_ITEM_ID, (int)(player.inventory.GetAmount(HQ_ORE_ITEM_ID) * scalar));
                                var metalTaken = player.inventory.Take(fusionItems, METAL_ORE_ITEM_ID, (int)(player.inventory.GetAmount(METAL_ORE_ITEM_ID) * scalar));
                                var sulfurTaken = player.inventory.Take(fusionItems, SULFUR_ORE_ITEM_ID, (int)(player.inventory.GetAmount(SULFUR_ORE_ITEM_ID) * scalar));

                                var stonesTaken = player.inventory.Take(fusionItems, STONES_ITEM_ID, (int)(player.inventory.GetAmount(STONES_ITEM_ID) * scalar));

                                if (fusionItems.Count > 0)
                                {
                                    var sb = Pool.Get<StringBuilder>();

                                    sb.Clear().Append("<color=red>CORRUPTION. <color=yellow>FUSION.</color></color>").Append(Environment.NewLine);

                                    
  
                                    try
                                    {
                                        for (int i = 0; i < fusionItems.Count; i++)
                                        {
                                            var oreItem = fusionItems[i];

                                            sb.Append("<color=yellow>FUSION:</color> <color=red>-</color>").Append(oreItem.amount.ToString("N0")).Append(" <color=yellow>").Append(oreItem.info.displayName.english).Append("</color>").Append(Environment.NewLine);

                                            NoteInvByItem(player, oreItem, true);

                                            RemoveFromWorld(oreItem);

                                            try
                                            {
                                                Effect.server.Run("assets/bundled/prefabs/fx/gas_explosion_small.prefab", SpreadVector(eyesPos, 1.5f), Vector3.zero);
                                            }
                                            catch (Exception ex) { PrintError(ex.ToString()); }

                                            if (i < fusionItems.Count)
                                                CoroutineEx.waitForSecondsRealtime(0.05f);
                                        }


                                        //trim sb new line.
                                        sb.Length--;

                                        SendLuckNotifications(player, sb.ToString());

                                    }
                                    finally { Pool.Free(ref sb); }
                                }

                              

                               

                            }
                            finally { Pool.FreeList(ref fusionItems); }



                           
                        }
                    }

                    player.Server_CancelGesture();

                    yield return CoroutineEx.waitForSecondsRealtime(0.125f);

                    player.Server_StartGesture(StringToGesture(player, "friendly"));

                    Effect.server.Run("assets/prefabs/npc/bear/sound/breathe.prefab", player, 0, Vector3.zero, Vector3.zero);

                    yield return CoroutineEx.waitForSecondsRealtime(0.25f);

                    Effect.server.Run("assets/rust.ai/agents/bear/sound/breathe.prefab", player, 0, Vector3.zero, Vector3.zero);

                    yield return CoroutineEx.waitForSecondsRealtime(1.75f);

                    player.Server_StartGesture(StringToGesture(player, "friendly"));

                    yield return CoroutineEx.waitForSecondsRealtime(0.175f);

                    //move them first

                    var newEnts = new HashSet<OreResourceEntity>();

                    PrintWarning("loop nearEnts");

                    var increasingSpeedInterval = 0.0033f;
                    var oreMoveWaitTime = 0.375f;

                    for (int i = 0; i < ores.Count; i++)
                    {
                        var ent = ores[i];
                        if (ent == null || ent.IsDestroyed) continue;

                        if (player?.GetActiveItem() == null)
                        {
                            PrintWarning("activeitem is now null, yield break!");
                            yield break;
                        }

                        oreMoveWaitTime = Mathf.Clamp(oreMoveWaitTime - increasingSpeedInterval, 0.025f, oreMoveWaitTime);

                        var oldPos = ent?.transform?.position ?? Vector3.zero;
                        var prefabName = ent?.PrefabName;

                        Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/explosion.prefab", oldPos, Vector3.zero);

                        yield return CoroutineEx.waitForSecondsRealtime(0.1f);

                        Effect.server.Run("assets/bundled/prefabs/fx/ore_break.prefab", oldPos, Vector3.zero);

                        ent.Kill(); //kill the old ent to spawn in a new pos, since ores can't be moved properly it seems? (they go invisible/flicker)


                        var newSpawnPos = SpreadVector(eyesPos, Random.Range(1.5f, 2.4f));
                        newSpawnPos.y = plyPos.y += 0.133f;

                        var newEnt = GameManager.server.CreateEntity(prefabName, newSpawnPos) as OreResourceEntity;


                        newEnt.Spawn();
                        newEnts.Add(newEnt);

                        newEnt.Invoke(() =>
                        {
                            if (newEnt != null && !newEnt.IsDestroyed) newEnt.Kill(); //safety kill in case the coroutine can't complete, we don't want to keep floating ores
                        }, 120f);

                        Effect.server.Run("assets/bundled/prefabs/fx/ore_break.prefab", newSpawnPos, Vector3.zero);


                        yield return CoroutineEx.waitForSeconds(oreMoveWaitTime);
                    }


                    yield return CoroutineEx.waitForSecondsRealtime(0.175f);

                    Effect.server.Run("assets/prefabs/npc/bear/sound/death.prefab", player, 0, Vector3.zero, Vector3.zero);

                    yield return CoroutineEx.waitForSecondsRealtime(0.125f);

                    Effect.server.Run("assets/rust.ai/agents/bear/sound/death.prefab", player, 0, Vector3.zero, Vector3.zero);

                    player.Server_CancelGesture();

                    yield return CoroutineEx.waitForEndOfFrame;

                    player.Server_StartGesture(StringToGesture(player, "victory"));

                    if (hasAesir)
                    {
                        yield return CoroutineEx.waitForSecondsRealtime(0.125f);

                        ShowToast(player, "<color=red>Aesir's Rage!</color>");

                        player.Hurt(20f, DamageType.Bleeding, player, false);

                        player.metabolism.bleeding.Add(15f);
                    }
                    

                    yield return CoroutineEx.waitForSecondsRealtime(!hasAesir ? 2f : 1.875f);

                    SendReply(player, "<color=#D72B29><i><size=32>Lay waste to the earth!</color></i></size>");

                    player.Server_StartGesture(StringToGesture(player, "shrug"));

                    foreach (var newEnt in newEnts)
                    {
                        if (newEnt == null || newEnt.IsDestroyed) continue;

                        var activeItem = GetActiveItem(player);

                        if (activeItem == null || originalHeldItem != activeItem || activeItem.isBroken)
                        {
                            PrintWarning("activeitem is now null, broken or not the same item, yield break!");
                            yield break;
                        }

                        var newEntPos = newEnt?.transform?.position ?? Vector3.zero;

                        var newInfo = new HitInfo(player, newEnt, DamageType.Blunt, 5f)
                        {
                            Weapon = weapon,
                            WeaponPrefab = weapon,
                            DidGather = false,
                            CanGather = true,
                            HitPositionWorld = newEntPos,
                            HitPositionLocal = Vector3.zero
                        };

                    

                        var collider = newEnt?.GetComponentInChildren<Collider>() ?? newEnt?.GetComponentInParent<Collider>() ?? null;
                        if (collider == null)
                        {
                            PrintWarning("collider null: " + newEnt.ShortPrefabName);
                            continue;
                        }

                        var hitMat = ((collider == null) ? string.Empty : collider?.material?.name ?? collider?.sharedMaterial?.name ?? string.Empty).Replace("(Instance)", string.Empty, StringComparison.OrdinalIgnoreCase).TrimEnd().ToLower();
                        var hitCol = (hitMat != "default") ? StringPool.Get(hitMat) : 0;

                        while (newEnt != null && !newEnt.IsDestroyed)
                        {

                            if (player == null || !player.IsConnected || player.IsDead() || GetActiveItem(player) == null)
                            {
                                PrintWarning("while loop cannot continue!! player null, dead, disconnected or not holding any item");
                                break;
                            }


                            newInfo.DidGather = false;
                            newInfo.CanGather = true;
                            newInfo.DidHit = false;

                            if (newEnt._hotSpot != null && !newEnt._hotSpot.IsDestroyed)
                                newInfo.HitPositionWorld = newEnt._hotSpot.transform.position;
                            

                            newEnt.OnAttacked(newInfo);
                            Interface.Oxide.CallHook("OnMeleeAttack", player, newInfo); //again, this hook does not call itself

                            var adjustedFxPoint = new Vector3(eyesPos.x, eyesPos.y + Random.Range(1.6f, 2f), eyesPos.z);
                            adjustedFxPoint = SpreadVector(adjustedFxPoint, Random.Range(0.33f, 1.1f));

                            for (int j = 0; j < 4; j++) Effect.server.Run("assets/bundled/prefabs/fx/bucket_drop_debris.prefab", adjustedFxPoint, Vector3.zero);

                            if (hitCol != 0)
                            {
                                var spreadEntPos = SpreadVector(newEnt.ClosestPoint(eyesPos), Random.Range(0.1f, 0.9f));

                                //todo: cache this hitinfo
                                Effect.server.ImpactEffect(new HitInfo()
                                {
                                    HitPositionWorld = spreadEntPos,
                                    HitNormalWorld = Vector3.zero,
                                    HitMaterial = hitCol
                                });
                            }


                            Effect.server.Run("assets/bundled/prefabs/fx/survey_explosion.prefab", newEntPos);

                            var waitTime = IsWearingSkin(player, SHOCK_ABSORBER_GLOVES_SKIN_ID) ? 0.01875f : 0.0375f;

                            yield return CoroutineEx.waitForSeconds(waitTime);
                        }

                        yield return CoroutineEx.waitForEndOfFrame;
                    }

                }
                finally
                {
                    try { if (player != null) ActiveWeightOfTheWorldCoroutine.Remove(player); }
                    finally { if (hadNearEnts && !hasAesir) wotwCooldown.StartCooldown(); } //don't start cooldown if there weren't any ores - update: or if has aesir
                }
            }
            finally { if (freeList) Pool.FreeList(ref ores); }
          

            yield return null;
        }

        [ConsoleCommand("luck.startgui")]
        private void lkStartGUINew(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null) return;

            SkillGUI(player, SkillTree.Resource);
        }

        [ConsoleCommand("luck.tree")]
        private void lkTree(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null) return;

            if (!_curTree.TryGetValue(player.userID, out SkillTree outCur)) outCur = SkillTree.Undefined;

            var isPrev = arg.Args != null && arg.Args.Length > 0 && arg.Args[0].Equals("prev", StringComparison.OrdinalIgnoreCase);

            var intTree = (int)outCur;

            var max = Enum.GetValues(typeof(SkillTree)).Length - 1; //undefined is 0

            var desiredInt = isPrev ? (intTree - 1) : (intTree + 1);

            desiredInt = desiredInt < 1 ? max : desiredInt > max ? 1 : desiredInt;

            var desiredTree = (SkillTree)Mathf.Clamp(desiredInt, 1, max);

            SkillGUI(player, desiredTree);

            SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");
        }

        [ConsoleCommand("luck.respecbutton")]
        private void lkRespecButton(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null) return;
            
            cmdSkillTrees(player.IPlayer, "skt", new string[] { "respec" });
            StopSKTGUI(player);

        }

        [ConsoleCommand("legendary.nextpage")]
        private void legendaryNextPage(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null) return;
            if (!_curPage.TryGetValue(player.userID, out _))
                _curPage[player.userID] = 0;


            _curPage[player.userID] = GetNextLegendaryGUIPage(player.userID);

            LegendaryGUI(player, _curPage[player.userID]);

            SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/item.select.fx.prefab");
        }

        [ConsoleCommand("charm.purge")]
        private void charmPurge(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && !arg.IsAdmin) return;

            SendReply(arg, "Starting charm purge coroutine");

            CharmPurgeCoroutine = StartCoroutine(CharmPurge());

            SendReply(arg, "Purge routine started");
        }

        [ConsoleCommand("luck.purge")]
        private void lkPurge(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var newDict = storedData3?.luckInfos?.ToDictionary(p => p.Key, p => p.Value) ?? null;
            if (newDict == null || newDict.Count < 1)
            {
                SendReply(arg, "No luck infos!");
                return;
            }

            var removeSB = new StringBuilder();
            var removeCount = 0;

            var doRemove = false;
            foreach (var kvp in newDict)
            {


                if (kvp.Value.Level <= 1 || kvp.Value.XP <= 10)
                {
                    removeSB.AppendLine("Removing: " + GetDisplayNameFromID(kvp.Key) + ", because they have level <= 1");
                    doRemove = true;
                }

                if (doRemove && storedData3.luckInfos.Remove(kvp.Key)) removeCount++;
            }
            SaveData();
            SendReply(arg, "Removed " + removeCount + " entries:\n" + removeSB.ToString().TrimEnd());
        }


        [ConsoleCommand("lkskp")]
        private void lkSkp(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
                return;

            if (arg.Args == null || arg.Args.Length != 2)
            {
                Puts("wrong");
                return;
            }
            var player = FindPlayerByPartialName(arg.Args[0], true);
            var luckInfo = GetLuckInfo(player);
            if (luckInfo == null)
            {
                if (ulong.TryParse(arg.Args[0], out ulong id)) luckInfo = GetLuckInfo(id);
            }
            if (luckInfo == null)
            {
                SendReply(arg, "Failed to get luckinfo/!");
                return;
            }
            if (!int.TryParse(arg.Args[1], out int points))
            {
                SendReply(arg, "Not an int: " + arg.Args[1]);
                return;
            }
            luckInfo.SkillPoints = points;
            SendReply(arg, "Set LKSKP for: " + (player?.displayName ?? arg.Args[0]) + ", to: " + points);
        }



        private BaseCombatEntity GetBackpackContainerByID(ItemId itemUID)
        {

            if (!_itemUIDToBackPackContainerID.TryGetValue(itemUID.Value, out ulong netId))
                return null;

            var findId = BaseNetworkable.serverEntities.Find(new NetworkableId(netId)) as BaseCombatEntity;
            if (findId == null)
            {
                PrintWarning("failed to get findId entity from netId!!!! netId: " + netId + " itemUID: " + itemUID);
            }

            return findId;
        }

        private RepairBench SpawnNomadRepairBench(Vector3 initialPosition)
        {
            var bench = (RepairBench)GameManager.server.CreateEntity("assets/bundled/prefabs/static/repairbench_static.prefab", new Vector3(initialPosition.x, -40f, initialPosition.z));
            if (bench == null)
            {
                PrintWarning("bench is null!!!");
                return null;
            }

            bench.enableSaving = false;

            bench.OwnerID = 528491;
            bench.Spawn();

            bench.EnableSaving(false);


            return bench;
        }

        private bool IsInAridBiome(BasePlayer player)
        {
            return player != null && !player.IsDestroyed && !player.IsDead() && TerrainMeta.BiomeMap.GetBiomeMaxType(player?.transform?.position ?? Vector3.zero, -1) == TerrainBiome.ARID;
        }

        [ChatCommand("resetcd")]
        private void cmdResetCooldowns(BasePlayer player, string command, string[] args)
        {

            if (!player.IsAdmin) return;

            var cooldownHandlers = player?.GetComponents<PlayerCooldownHandler>();

            if (cooldownHandlers == null || cooldownHandlers.Length < 1)
            {
                SendReply(player, "You have no cooldown handlers.");
                return;
            }

            var sb = Pool.Get<StringBuilder>();
            try 
            {

                sb.Clear();

                for (int i = 0; i < cooldownHandlers.Length; i++)
                {
                    var handler = cooldownHandlers[i];
                    handler.EndCooldown();

                    sb.Append("Ended cooldown for ").Append(handler.GetType().Name).Append(Environment.NewLine);

                }

                if (sb.Length > 1)
                    sb.Length -= 1;

                SendReply(player, sb.ToString());
            }
            finally { Pool.Free(ref sb); }

           


        }

        [ChatCommand("biometest")]
        private void cmdBiomeTest(BasePlayer player, string command, string[] args)
        {

            if (!player.IsAdmin) return;

            var biome = TerrainMeta.BiomeMap.GetBiomeMaxType(player.transform.position, -1);
            var biomeType2 = TerrainMeta.BiomeMap.GetBiomeMaxType(player.transform.position, ~biome);

            var biome3 = TerrainMeta.BiomeMap.GetBiome(player.transform.position, biomeType2);

            SendReply(player, "biome: " + biome + ", biomeType2: " + biomeType2 + ", biome3: " + biome3);

            SendReply(player, "In arid/desert: " + IsInAridBiome(player));
        }

        [ChatCommand("rb")]
        private void cmdNomadRepairBench(BasePlayer player, string command, string[] args)
        {

            if (!IsWearingItem(player, NOMAD_SUIT_ID))
            {
                SendReply(player, "You must be a Nomad wearing a Nomad Suit to use this command! If you are a Nomad wearing a Nomad Suit, this command will open a repair bench for you to repair your items, although they won't be repaired quite as well as a typical repair bench and will result in more permanent damage.");
                return;
            }

            var bench = SpawnNomadRepairBench(player.transform.position);

            player.Invoke(() =>
            {
                if (player == null || player.IsDead() || !player.IsConnected || bench == null || bench.IsDestroyed) return;

                StartLootingContainer(player, bench, false);

            }, 0.15f);

        }

        private void StartLootingContainer(BasePlayer player, BaseEntity entity, bool posChecks = true, string overridePanelName = "")
        {
            if (player == null || player.IsDead() || !player.IsConnected) return;

            var inventory = (entity as StorageContainer)?.inventory ?? (entity as LootContainer)?.inventory ?? (entity as DroppedItemContainer)?.inventory ?? (entity as PlayerCorpse)?.containers[0];

            if (inventory == null)
            {
                PrintError(nameof(inventory) + " is null for entity!!!: " + entity?.ShortPrefabName);
                return;
            }

            var panelName = (entity as StorageContainer)?.panelName ?? (entity as LootContainer)?.panelName ?? (entity as DroppedItemContainer)?.lootPanelName ?? (entity as PlayerCorpse)?.lootPanelName ?? string.Empty;

            player.inventory.loot.containers.Clear();
            player.inventory.loot.entitySource = null;
            player.inventory.loot.itemSource = null;
            player.inventory.loot.MarkDirty();
            player.inventory.loot.PositionChecks = posChecks;

            player.inventory.loot.StartLootingEntity(entity, false);
            player.inventory.loot.AddContainer(inventory);
            player.inventory.loot.SendImmediate();

            player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", string.IsNullOrWhiteSpace(overridePanelName) ? panelName : panelName);
        }

        [ChatCommand("luck")]
        private void StatInfoCommand(BasePlayer player, string command, string[] args)
        {

            if (args.Length > 1 && args[0].Equals("test"))
            {
                var points = long.Parse(args[1]);
                SendReply(player, nameof(GetLevelFromPoints) + " : " + GetLevelFromPoints(points));
                return;
            }

            if (args.Length > 0 && args[0].Equals("msg", StringComparison.OrdinalIgnoreCase))
            {
                var hasPerm = permission.UserHasPermission(player.UserIDString, DISABLE_LUCK_CHAT_MESSAGES_PERM);
                if (!hasPerm)
                {
                    permission.GrantUserPermission(player.UserIDString, DISABLE_LUCK_CHAT_MESSAGES_PERM, null);
                    hasPerm = true;
                }
                else
                {
                    permission.RevokeUserPermission(player.UserIDString, DISABLE_LUCK_CHAT_MESSAGES_PERM);
                    hasPerm = false;
                }

                var msg = !hasPerm ? "<color=#40c6ff>You will now see notifications in chat whenever you get <color=" + LUCK_COLOR + ">Lucky</color> with a skill!</color>" : "<color=#ff4545>You'll no longer see notifications in chat when getting <color=" + LUCK_COLOR + ">Lucky</color> with a skill!</color>";

                SendReply(player, msg);
                return;
            }

            var pointsPickup = 0f;
            if (!TryParseFloat(pointsPerHit[LUCK_SHORTNAME], ref pointsPickup))
            {
                SendReply(player, "Failed to get points pickup, report to an Admin!");
                PrintWarning("Failed to get points pickup for player: " + player.displayName);
                return;
            }

            var lvlLk = getLevel(player.userID);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var messageSb = sb.Clear().Append(LUCK_COLOR_FORMATTED).Append("Luck</color>\n").Append("XP per pickup: ").Append(LUCK_COLOR_FORMATTED).Append(pointsPickup.ToString("N0")).Append("</color>\nBonus materials per level: ").Append(LUCK_COLOR_FORMATTED).Append(((GetGatherMultiplier(2) - 1) * 100).ToString("0.##")).Append("</color>%\nCurrent Luck Level: ").Append(LUCK_COLOR_FORMATTED).Append(lvlLk).Append("</color>\nCurrent Points: ").Append(LUCK_COLOR_FORMATTED).Append(getPoints(player.userID).ToString("N0")).Append("</color>/").Append(LUCK_COLOR_FORMATTED).Append(GetPointsForLevel(getLevel(player.userID) + 1).ToString("N0")).Append("</color>\nYour XP rate is: ").Append(LUCK_COLOR_FORMATTED).Append(GetXPMultiplier(player.UserIDString).ToString("0.00").Replace(".00", string.Empty)).Append("</color>%\nYour Item rate is: ").Append(LUCK_COLOR_FORMATTED).Append(GetItemMultiplier(player.UserIDString).ToString("0.00").Replace(".00", string.Empty)).Append("</color>%\nCatchup multiplier: ").Append(LUCK_COLOR_FORMATTED).Append(getCatchupMultiplier(player.userID).ToString("N0") + "</color>%\nLuck chance multiplier: ").Append(LUCK_COLOR_FORMATTED).Append(GetChanceBonuses(player.UserIDString).ToString("N1").Replace(".0", string.Empty)).Append("</color>%\nType ").Append(LUCK_COLOR_FORMATTED).Append("/skt</color> to see info about perks that can be unlocked at certain levels.");
                    
                messageSb.Append(Environment.NewLine).Append("If you wish to disable or toggle the showing of Luck skill notifications in chat (eruption, gather, etc), type ").Append(LUCK_COLOR_FORMATTED).Append("/").Append(command).Append(" msg</color>!");


                SendReply(player, messageSb.ToString());

               // SendReply(player, msgSB.ToString().TrimEnd() + "\n\n" + "If you wish to disable or toggle the showing of Luck skill notifications in chat (eruption, gather, etc), type <color=" + LUCK_COLOR + ">/" + command + " msg</color>!");
            }
            finally { Pool.Free(ref sb); }

           
            var charms = GetPlayerCharms(player);
            if (charms == null || charms.Count < 1) return;

            var charmSB = Pool.Get<StringBuilder>();
            try 
            {
                charmSB.Clear().Append("Bonuses granted from Charms:\n");

                Dictionary<CharmSkill, float> charmXPs = new();
                Dictionary<CharmSkill, float> charmGathers = new();
                Dictionary<CharmSkill, float> charmChances = new();

                float craftingSpeed = 0f;
                for (int i = 0; i < charms.Count; i++)
                {
                    var charm = charms[i];
                    if (charm == null) continue;
                    if (charm.xpBonus != 0.0f)
                    {
                        if (!charmXPs.TryGetValue(charm.Skill, out float charmXP)) charmXPs[charm.Skill] = charm.xpBonus;
                        else charmXPs[charm.Skill] += charm.xpBonus;
                    }
                    if (charm.itemBonus != 0.0f)
                    {
                        if (!charmGathers.TryGetValue(charm.Skill, out float charmGather)) charmGathers[charm.Skill] = charm.itemBonus;
                        else charmGathers[charm.Skill] += charm.itemBonus;
                    }
                    if (charm.chanceBonus != 0.0f)
                    {
                        if (!charmChances.TryGetValue(charm.Skill, out float charmChance)) charmChances[charm.Skill] = charm.chanceBonus;
                        else charmChances[charm.Skill] += charm.chanceBonus;
                    }
                    if (charm.craftBonus != 0) craftingSpeed += charm.craftBonus;
                }
                foreach (var key in charmXPs.Keys)
                {
                    var skillColor = string.Empty;

                    var getColorKey = key == CharmSkill.Woodcutting ? "WC" : key == CharmSkill.Mining ? "M" : key == CharmSkill.Survival ? "S" : string.Empty;

                    if (string.IsNullOrWhiteSpace(getColorKey))
                        skillColor = LUCK_COLOR;

                    if (string.IsNullOrEmpty(skillColor) && !ZLevelsColors.TryGetValue(getColorKey, out skillColor))
                    {
                        skillColor = string.Empty;
                        PrintWarning("NO GET ZLEVEL COLOR: " + getColorKey);
                    }

                    var strAppend = "<color=" + skillColor + ">" + key + "</color> XP: <color=" + skillColor + ">" + charmXPs[key].ToString("N1").Replace(".0", string.Empty) + "</color>%";
                    if (string.IsNullOrEmpty(skillColor)) strAppend = RemoveTags(strAppend);
                    charmSB.AppendLine(strAppend);
                }

                foreach (var key in charmGathers.Keys)
                {
                    var skillColor = string.Empty;

                    var getColorKey = key == CharmSkill.Woodcutting ? "WC" : key == CharmSkill.Mining ? "M" : key == CharmSkill.Survival ? "S" : string.Empty;

                    if (string.IsNullOrWhiteSpace(getColorKey))
                        skillColor = LUCK_COLOR;

                    if (string.IsNullOrEmpty(skillColor) && !ZLevelsColors.TryGetValue(getColorKey, out skillColor))
                    {
                        skillColor = string.Empty;
                        PrintWarning("NO GET ZLEVEL COLOR: " + getColorKey);
                    }

                    var strAppend = "<color=" + skillColor + ">" + key + "</color> Item Bonus: <color=" + skillColor + ">" + charmGathers[key].ToString("N1").Replace(".0", string.Empty) + "</color>%";
                    if (string.IsNullOrEmpty(skillColor)) strAppend = RemoveTags(strAppend);
                    charmSB.AppendLine(strAppend);
                }

                foreach (var key in charmChances.Keys)
                {
                    var skillColor = string.Empty;

                    var getColorKey = key == CharmSkill.Woodcutting ? "WC" : key == CharmSkill.Mining ? "M" : key == CharmSkill.Survival ? "S" : string.Empty;

                    if (string.IsNullOrWhiteSpace(getColorKey))
                        skillColor = LUCK_COLOR;

                    if (string.IsNullOrEmpty(skillColor) && !ZLevelsColors.TryGetValue(getColorKey, out skillColor))
                    {
                        skillColor = string.Empty;
                        PrintWarning("NO GET ZLEVEL COLOR: " + getColorKey);
                    }

                    var strAppend = "<color=" + skillColor + ">" + key + "</color> Chance Bonus: <color=" + skillColor + ">" + charmChances[key].ToString("N1").Replace(".0", string.Empty) + "</color>%";
                    if (string.IsNullOrEmpty(skillColor)) strAppend = RemoveTags(strAppend);
                    charmSB.AppendLine(strAppend);
                }
                if (craftingSpeed != 0.0f) charmSB.AppendLine("<color=green>Crafting speed:</color> <color=#8aff47>" + craftingSpeed.ToString("N1").Replace(".0", string.Empty) + "</color>%");
                SendReply(player, charmSB.ToString().TrimEnd());

            }
            finally { Pool.Free(ref charmSB); }

           

        }

        private void cmdCharmInfo(IPlayer player, string command, string[] args)
        {
            var charmMsg = "<color=#5E5197>---<size=18><color=#C6D182>CHARMS</color></size>---</color>\nCharms are items (notes) that, while in your inventory, grant various bonuses. Some grant an XP bonus to a specific skill, some grant a bonus to item gather amount. Some grant both!\n\n-There's a chance to find Charms in any loot crate in or around monuments.\n\n-There are three types of Charms:\nSmall, Large, and Grand. Small ones take up no space, but have the smallest bonuses and no chance for both XP and item bonuses.\n<color=#5E5197>Large</color> charms take <color=#5E5197>3</color> inventory slots away, but have a larger maximum bonus.\n<color=#C6D182>Grand</color> charms have the largest bonuses, but take up <color=#C6D182>4</color> slots.";
            var charmMsg2 = "<color=#fffcb5>Charm bonuses stack for skills.</color> All Charms on your person add up to your total bonus in that skill.\nCharms have <i><color=#4860db>no hard cap</color></i>, their bonuses are limited only by your inventory space.\n<color=#2481d6>You can view your total charm bonus by typing <size=16><color=" + LUCK_COLOR + ">/luck</color></size></color>.";
            SendReply(player, charmMsg + Environment.NewLine + Environment.NewLine + charmMsg2); //sb in future?
        //    SendReply(player, charmMsg2);
            //SendReply(player, WIKI_LINK_TXT);
        }

        private void PlayCodeLockUpdate(BasePlayer player, int timesToRun = 1)
        {
            if (player == null || (player?.IsDead() ?? true) || timesToRun < 1) return;
            var netCon = player?.net?.connection ?? null;
            var eyePos = player?.eyes?.position ?? Vector3.zero;
            var fx = "assets/prefabs/locks/keypad/effects/lock.code.updated.prefab";
            for (int i = 0; i < timesToRun; i++)
            {
                if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? false)) break;
                using var luckEffect = new Effect(fx, eyePos, Vector3.zero);
                EffectNetwork.Send(luckEffect, netCon);
            }
        }

        private void PlayBatteryEffect(BasePlayer player, int timesToRun = 1)
        {
            if (player == null || (player?.IsDead() ?? true) || timesToRun < 1) return;

            var netCon = player?.net?.connection ?? null;
            var eyePos = player?.eyes?.position ?? Vector3.zero;
            var fx = "assets/prefabs/clothes/night.vision.goggles/sound/battery_change.prefab";
            for (int i = 0; i < timesToRun; i++)
            {
                if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? false)) break;
                using var luckEffect = new Effect(fx, eyePos, Vector3.zero);
                EffectNetwork.Send(luckEffect, netCon);
            }

        }

        private void PlayScrapEffect(BasePlayer player, int timesToRun = 1)
        {
            if (player == null || player.IsDead() || player?.transform == null || timesToRun < 1) return;
            var netCon = player?.net?.connection ?? null;
            var eyePos = player?.eyes?.position ?? Vector3.zero;
            var fx1 = "assets/prefabs/building/wall.frame.fence/effects/chain-link-fence-deploy.prefab";
            var fx2 = "assets/prefabs/building/wall.frame.fence/effects/chain-link-impact.prefab";
            var fx3 = "assets/prefabs/deployable/research table/effects/research-table-deploy.prefab";
            for (int i = 0; i < timesToRun; i++)
            {
                if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? false)) break;

                using (var effect1 = new Effect(fx1, eyePos, Vector3.zero)) { EffectNetwork.Send(effect1, netCon); }
                using (var effect2 = new Effect(fx2, eyePos, Vector3.zero)) { InvokeHandler.Invoke(player, () => EffectNetwork.Send(effect2, netCon), Random.Range(0.1f, 0.25f)); }
                using var effect3 = new Effect(fx3, eyePos, Vector3.zero);
                InvokeHandler.Invoke(player, () => EffectNetwork.Send(effect3, netCon), Random.Range(0.25f, 0.35f));
            }
        }

        private void OnLootEntityEnd(BasePlayer player, BaseEntity entity)
        {
            if (entity == null) return;

            if (IsBackpackSkin(entity.skinID))
            {

                CuiHelper.DestroyUi(player, "GUI_Backpack");

                var dropped = entity as DroppedItemContainer;
                dropped?.Invoke(() =>
                {
                    if (dropped != null && !dropped.IsDestroyed) dropped.CancelInvoke(new Action(dropped.RemoveMe));
                }, 0.1f);
            }

            if (entity.OwnerID == 528491 && (entity is RepairBench || entity.transform.position.y <= -40))
            {

                entity.Invoke(() =>
                {
                    if (entity == null || entity.IsDestroyed) return;



                    entity.Kill();
                }, 0.01f);

                if (entity.prefabID == 1560881570)
                {
                    StopTokenGUI(player);
                    StopTextAboveInventory(player);

                    var container = entity as StorageContainer;

                    if (_transmuteCoroutine.TryGetValue(player.UserIDString, out Coroutine activeRoutine))
                    {
                        StopCoroutine(activeRoutine);

                        _transmuteCoroutine.Remove(player.UserIDString);

                        PrintWarning("stopped routine on loot end!!");

                        SendLocalEffect(player, "assets/prefabs/npc/autoturret/effects/targetlost.prefab");

                    }


                    if (!_temporaryTransmuteItems.TryGetValue(entity.net.ID, out List<ItemAmount> itemAmounts) || itemAmounts.Count < 1)
                    {
                        PrintWarning("no item amounts gotten!");
                    }
                    else PrintWarning("got temporary itemAmounts: " + itemAmounts.Count);

                    var inv = container?.inventory;

                    if ((itemAmounts?.Count ?? 0) > 1)
                    {
                        PrintWarning("found itemAmounts!");

                        for (int i = 0; i < itemAmounts.Count; i++)
                        {
                            var itemAmount = itemAmounts[i];

                            if (inv != null)
                            {
                                var val = inv.Take(null, itemAmount.itemDef.itemid, (int)itemAmount.amount);

                                PrintWarning("ran inv.take as a precaution, val: " + val);
                            }

                            var item = ItemManager.Create(itemAmount.itemDef, (int)itemAmount.amount);

                            player.GiveItem(item);

                        }
                    }

                    var itemList = container?.inventory?.itemList?.ToList(); //temporary. apparently moving the items out of the container removes them from itemlist during iteration

                    var count = itemList.Count;

                    PrintWarning("itemlist is count: " + count);

                    for (int i = 0; i < count; i++)
                    {
                        var item = itemList[i];

                        PrintWarning("item: " + item.info.shortname + " x" + item.amount);

                        player.GiveItem(item);
                    }



                }

            }

            StopMagpieGUI(player);

            var fishTrap = entity as WildlifeTrap;
            if (fishTrap != null && fishTrap.OwnerID != 0)
            {
                var fishLvl = GetLuckInfo(entity.OwnerID)?.GetStatLevelByName("Fishing") ?? 0;

                if (fishLvl > 0)
                {
                    fishTrap?.Invoke(() =>
                    {
                        CancelInvoke("TrapThink", fishTrap);

                        var ext = fishTrap?.GetComponent<WildTrapExt>() ?? fishTrap.gameObject.AddComponent<WildTrapExt>();
                        if (ext == null)
                        {
                            PrintError("Ext null after add?!?!?!");
                            return;
                        }

                        AdjustTickRate(fishTrap, entity.OwnerID);

                    }, 0.01f);
                }
            }


        }

        private readonly Dictionary<ulong, HashSet<NetworkableId>> _playerLootedEnts = new();
        private readonly HashSet<NetworkableId> _lootedEnts = new();
        private readonly Dictionary<ulong, HashSet<ulong>> _lootedCorpses = new();

        private readonly Dictionary<ulong, HashSet<NetworkableId>> _magpieLootedEnts = new();

        private bool HasMagpieLooted(ulong userId, NetworkableId entId)
        {


            return _magpieLootedEnts.TryGetValue(userId, out HashSet<NetworkableId> magpieLooted) && magpieLooted.Contains(entId);
        }

        private bool AddMagpieLooted(ulong userId, NetworkableId entId)
        {
            if (!_magpieLootedEnts.TryGetValue(userId, out _))
                _magpieLootedEnts[userId] = new HashSet<NetworkableId>();

            return _magpieLootedEnts[userId].Add(entId);
        }

        //shitty(?) pity system
        private readonly Dictionary<string, int> _consecutiveLegendaryChancesMissed = new(); 

        private float GetPityScaledLegendaryChance(string userId, float baseChance)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (baseChance <= 0f) throw new ArgumentOutOfRangeException(nameof(baseChance));

            var chance = baseChance;

            if (!_consecutiveLegendaryChancesMissed.TryGetValue(userId, out int missed) || missed < 5)
                return chance;


            chance += missed * 0.00125f;

            return Mathf.Clamp(chance, baseChance, 1f);
        }

        private float GetBonusScaledLegendaryChance(BasePlayer player, float baseChance)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (baseChance <= 0f) 
                throw new ArgumentOutOfRangeException(nameof(baseChance));

            var chance = baseChance;

            var hasGeiger = HasItemInContainer(player.inventory.containerBelt, GEIGER_COUNTER_ITEM_ID);

            if (hasGeiger)
            {
                var baseAdd = 0.0033f;

                PrintWarning("player radiation level: " + player.metabolism.radiation_level.value + ", poison: " + player.metabolism.radiation_poison.value);

                var radiationAdd = (player?.metabolism?.radiation_poison?.value ?? 0f) * 0.0001f;

                chance += baseAdd + radiationAdd;
            }


            if (chance != baseChance)
                PrintWarning("chance after additional scale: " + chance);

            return Mathf.Clamp(chance, baseChance, 1f);
        }

        private bool ToggleBackpackAutoDeposit(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var info = GetLuckInfo(userId);

            return (info.BackpackAutoDeposit = !info.BackpackAutoDeposit);
        }

        private void SendBackpackDepositMessage(BasePlayer player, bool newState, bool showToast = true)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                var stateColorTag = GetOpeningColorTag(newState ? "#53D559" : "#FF0030"); //PRISM green, red, respectively


                var toastMsg = sb.Clear().Append("Auto Deposit is now ").Append(stateColorTag).Append(newState ? "ON" : "OFF").Append("</color>.").ToString();
                var fullMsg = sb.Clear().Append("Auto Deposit is now ").Append(stateColorTag).Append(newState ? "on" : "off").Append("</color>. Any resources harvested while wearing your backpack will ").Append(newState ? "now go into your backpack if space is available." : "no longer go into your backpack.").ToString();

                SendReply(player, fullMsg);
                ShowToast(player, toastMsg, newState ? 0 : 1); //1 == alert!

            }
            finally { Pool.Free(ref sb); }

        }

        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (player == null || entity == null) return;

            if (entity is Stocking) return; //lol


            if (IsBackpackSkin(entity.skinID))
            {
                var fx1 = "assets/prefabs/weapons/hacksaw/effects/hit.prefab";
                var fx2 = "assets/prefabs/deployable/locker/sound/equip_zipper.prefab";

                SendLocalEffect(player, Random.Range(0, 101) <= 60 ? fx2 : fx1);

                BackpackDepositGUI(player);

                return;
            }



            (entity as SurvivalFishTrap)?.Invoke(() =>
            {
                if (entity == null || entity.IsDestroyed) return;
                CancelInvoke("TrapThink", entity);
            }, 0.1f);

            var prefabId = entity?.prefabID ?? 0;

            var corpse = entity as NPCPlayerCorpse;

            var crateCanSpawnLegendaries = entity is NPCPlayerCorpse || entity is HackableLockedCrate || prefabId == 1737870479 /*/bradley crate/*/ || prefabId == 1314849795 /*/ heli crate/*/ || prefabId == 3286607235 || prefabId == 96231181; //elite crate and underwater elite, respectively

            HashSet<ulong> lootedCorpses = null;

            if (corpse != null && corpse.playerSteamID != 0)
            {
                if (!_lootedCorpses.TryGetValue(player.userID, out lootedCorpses)) lootedCorpses = _lootedCorpses[player.userID] = new HashSet<ulong>();

                if (!lootedCorpses.Add(corpse.playerSteamID))
                    return;

            }



            var itemBackpack = entity as DroppedItemContainer;
            if (itemBackpack != null)
            {

                if (itemBackpack.playerSteamID == 0) //not a scientist drop pack, and certainly not a real player
                    return;

                if (_lootedCorpses.TryGetValue(player.userID, out lootedCorpses) && lootedCorpses.Contains(itemBackpack.playerSteamID)) //already looted the corpse prior to it being a bag
                    return;

                var isRealPly = (itemBackpack?.playerSteamID ?? 0).IsSteamId(); //is this a real player, or an AI/NPC bag?

                if (!isRealPly)
                    crateCanSpawnLegendaries = true;
                else return; //return. don't do anything. this is a player's real bag.
            }

            var lootInventory = (entity as LootContainer)?.inventory ?? corpse?.containers[0] ?? (entity as DroppedItemContainer)?.inventory;
            if (lootInventory == null) return;


            if (!_playerLootedEnts.TryGetValue(player.userID, out HashSet<NetworkableId> lootedOut)) lootedOut = _playerLootedEnts[player.userID] = new HashSet<NetworkableId>();

            var entID = entity.net.ID;

            var playerAlreadyLooted = !lootedOut.Add(entID);

            var alreadyLooted = !_lootedEnts.Add(entID);

            if (!alreadyLooted)
            {
                for (int i = 0; i < lootInventory.itemList.Count; i++)
                {
                    var item = lootInventory.itemList[i];

                    if (!IsLegendaryItem(item))
                        continue;

                    CelebrateLegendarySpawnFromItem(player, item);

                }
            }


            var luckInfo = GetLuckInfo(player);
            if (luckInfo == null) return;

            int xpMin;
            int xpMax;
            float rndXP;

            if (luckInfo.GetStatLevelByName("Magpie") > 0 && !HasMagpieLooted(player.userID, entID) && !_lootableItemsRemoved.Contains(entID))
            {
                var cooldown = player?.GetComponent<MagpieCooldownHandler>() ?? player?.gameObject?.AddComponent<MagpieCooldownHandler>();

                if (!cooldown.IsOnCooldown && _magpieCooldownStartAction.TryGetValue(player.UserIDString, out Action cooldownStartAction))
                {
                    PrintWarning("cancel cooldown!");
                    SendLuckNotifications(player, "Magpie chain loot! - Cooldown delayed for another 9 seconds. Keep looting to keep the chain active!");

                    player.CancelInvoke(cooldownStartAction);

                    player.Invoke(cooldownStartAction, 9f);

                }


                MagpieGUI(player, GetMagpieGUIText(player));
            }



            if (playerAlreadyLooted) // already looted!
                return;
            


            var userClassType = GetUserClassType(player.UserIDString);

            if (IsWearingSkin(player, CRAZY_EARL_SKIN_ID) && GetLastSkillRotationType(player.UserIDString) != ClassType.Packrat)
            {
                GrantRandomBonusToSkill(player);
                SetLastSkillRotationType(player.UserIDString, ClassType.Packrat);
            }

            //use new spawning from legendary class


            if (crateCanSpawnLegendaries)
            {
                PrintWarning(nameof(crateCanSpawnLegendaries) + " for: " + entity);
                var spawnChance = GetBonusScaledLegendaryChance(player, GetPityScaledLegendaryChance(player.UserIDString, _legendaryData.BaseLegendarySpawnChance));

                if (IsRadStormActive())
                    spawnChance *= 1.5f;

                if (Random.Range(0.0f, 1.0f) <= spawnChance)
                {
                    var rngLeg = LegendaryItemInfo.GetRandomLegendaryWithClassBias(userClassType);

                    if (rngLeg == null) PrintError("rngLeg is null!!!");
                    else if (!rngLeg.SpawnIntoInventory(lootInventory))
                    {
                        PrintError("failed to spawn legendary into inventory!!!");

                        var legItem = rngLeg.SpawnItem();

                        if (legItem == null) PrintError("leg item null?!?!?!?");
                        else if (!legItem.Drop(lootInventory.dropPosition, lootInventory.dropVelocity + Vector3.up * 1.2f))
                        {
                            PrintError("couldn't drop legItem?!?!?: " + legItem);
                            RemoveFromWorld(legItem);
                        }
                        else CelebrateLegendarySpawnFromItem(player, legItem);

                    }

                    _consecutiveLegendaryChancesMissed[player.UserIDString] = 0;

                }
                else
                {
                    if (!_consecutiveLegendaryChancesMissed.TryGetValue(player.UserIDString, out int missed))
                        _consecutiveLegendaryChancesMissed[player.UserIDString] = 1;
                    else _consecutiveLegendaryChancesMissed[player.UserIDString]++;
                }
            }



            var scalar = userClassType == ClassType.Packrat ? 1.25f : (userClassType == ClassType.Undefined || userClassType == ClassType.Loner) ? 1f : 0.5f; //this is all loot so we can just have one scalar since only packrats have extra loot chance

            if (userClassType == ClassType.Packrat)
            {
                player.Invoke(() =>
                {
                    if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead() || player.gameObject == null || lootInventory == null) return;

                    var baseXp = 48f;

                    var useXp = baseXp;

                    var multiplier = 1f;

                    var itemList = lootInventory?.itemList;
                    if (itemList != null && itemList.Count > 0)
                    {

                        var magpieLvl = luckInfo.GetStatLevelByName("Magpie");
                        var magpieCondScalar = 0.66f * magpieLvl;

                        for (int i = 0; i < itemList.Count; i++)
                        {
                            var item = itemList[i];

                            var rarity = item.info.rarity;

                            multiplier += rarity == Rarity.Rare ? 1f : rarity == Rarity.VeryRare ? 2f : rarity == Rarity.None ? 0.5f : 0;

                            if (item.info.category == ItemCategory.Weapon && item.hasCondition && item.conditionNormalized < 1f) //packrats find weapons with higher durability
                            {
                                item.conditionNormalized *= 1 + magpieLvl;

                                item.MarkDirty();
                            }

                        }
                    }

                    useXp *= multiplier;

                    useXp = DoCatchup(player.userID, DoMultipliers(player.userID, useXp));

                    PrintWarning("useXp: " + useXp + ", base: " + baseXp + ", mult: " + multiplier);

                    AddXP(player.UserIDString, useXp, XPReason.Loot);

                }, 0.125f);



            }

            /*/
            var lootCharms = GetLootCharms(player);
            if (lootCharms != null && lootCharms.Count > 0)
            {
                PrintWarning("Loot charms count: " + lootCharms.Count + ": " + player?.displayName);
                var sums = new Dictionary<Rarity, float>();
                float outTemp;
                for(int i = 0; i < lootCharms.Count; i++)
                {
                    var charm = lootCharms[i];
                    if (charm == null || charm.lootBonus <= 0 || charm.lootRarity == Rarity.None) continue;
                    if (!sums.TryGetValue(charm.lootRarity, out outTemp)) sums[charm.lootRarity] = charm.lootBonus;
                    else sums[charm.lootRarity] += charm.lootBonus;
                  
                }
                if (sums.Count < 1) PrintWarning("sums.count < 1");
                foreach(var kvp in sums)
                {
                    var rngNeed = 20;
                    var addVal = (int)Math.Round((rngNeed * kvp.Value) / 100, MidpointRounding.AwayFromZero);
                    rngNeed += addVal;
                    PrintWarning("rngNeed: " + rngNeed + ", add val: " + addVal + ", kvp.value: " + kvp.Value);
                    if (charmLootRng.Next(0, 101) <= rngNeed)
                    {
                        var rngDef = ItemManager.itemList?.Where(p => !p.hidden && p.rarity == kvp.Key)?.ToList()?.GetRandom() ?? null;
                        if (rngDef != null)
                        {
                            var rngItem = ItemManager.Create(rngDef, 1);
                            if (rngItem != null)
                            {
                                if (!rngItem.MoveToContainer(lootInventory) && !rngItem.Drop(player.GetDropPosition(), player.GetDropVelocity())) RemoveFromWorld(rngItem);
                            }
                        }
                    }
                   
                }
            }
            else
            {
                if (player.IsAdmin) PrintWarning("no loot charms!");
            }/*/

            //hween!!
            
           if (ConVar.Halloween.enabled && Random.Range(0, 101) <= Random.Range(2, 9))
           {
               var isAngry = Random.Range(0, 101) <= 66;
               var jackol = "jackolantern." + (isAngry ? "angry" : "happy");
               var jackItem = ItemManager.CreateByName(jackol, 1);

               if (jackItem == null)
               {
                   PrintWarning("jackItem null?!: " + jackol);
                   return;
               }

               if (!jackItem.MoveToContainer(lootInventory)) jackItem.Drop(entity.GetDropPosition(), entity.GetDropVelocity());


               var rngMinXP = Random.Range(isAngry ? 128 : 512, isAngry ? 1280 : 968);
               var rngMaxXP = Random.Range(isAngry ? 768 : 896, isAngry ? 3864 : 2560);
               var rngXP = DoMultipliers(player.userID, DoCatchup(player.userID, Random.Range(rngMinXP, rngMaxXP)));

               AddXP(player.UserIDString, rngXP);

               var hweenMsg = "<color=" + LUCK_COLOR + ">Lucky!</color> <color=#e6620a><color=#96467a>" + jackItem.info.displayName.english + "</color> for <color=#96467a>" + rngXP.ToString("N0") + "</color> XP!</color>";
               SendReply(player, hweenMsg);
               ShowPopup(player, hweenMsg);
               var fxPath = isAngry ? "assets/prefabs/npc/bear/sound/death.prefab" : "assets/prefabs/npc/bear/sound/breathe.prefab";
               var netCon = player?.net?.connection ?? null;
               var eyePos = player?.eyes?.position ?? Vector3.zero;

               for(int i = 0; i < Random.Range(2, 4); i++)
               {
                   SendLocalEffect(player, fxPath, eyePos);
               }

           }

            if (corpse != null)
            {
                var scavenger = luckInfo.GetStatLevelByName("Scavenger");

                if (scavenger > 0) // NO this can't be a return
                {
                    var scavVal = scavenger * (userClassType == ClassType.Packrat ? 8 : 6);

                    //packrat scaling needs to be added?!?!?!? - i just added it

                    var scavNeed = GetChanceScaledInteger(player.userID, scavVal, scavVal, 99, scalar);

                    var sb = Pool.Get<StringBuilder>(); //even if the player doesn't get lucky & the sb isn't used, it seems more performant to grab it once instead of grabbing it in the loop possibly multiple times

                    try
                    {
                        var itemList = lootInventory?.itemList;

                        for (int i = 0; i < itemList.Count; i++)
                        {
                            var item = itemList[i];

                            var itemId = item?.info?.itemid ?? 0;
                            var isScrap = itemId == SCRAP_ITEM_ID;

                            if ((!isScrap && (item?.info?.category ?? ItemCategory.All) != ItemCategory.Component) || scavRandom.Next(0, 101) > scavNeed)
                                continue; //does not match or rng didn't hit.

                            var oldAmt = item.amount;

                            var rndMax = 5;

                            var rndPerc = (scavenger == 1) ? 0 : scavenger * (isScrap ? 100 : 8); //5% each lvl, 100% each lvl for scrap

                            var rndPercF = (rndPerc > 0) ? (double)(rndMax * rndPerc / 100) : 0;

                            rndMax = (int)Math.Round(rndMax + rndPercF, MidpointRounding.AwayFromZero);

                            var newAmt = Mathf.Clamp(Random.Range(1, rndMax), 1, item.MaxStackable());

                            if (item.amount + newAmt <= item.MaxStackable())
                            {
                                item.amount += newAmt;
                                item.MarkDirty();
                            }
                            else
                            {
                                var newItem = ItemManager.Create(item.info, item.amount, item.skin);
                                if (!newItem.Drop(entity.GetDropPosition(), Vector3.up * 2))
                                {
                                    PrintWarning("no drop item!!");
                                    RemoveFromWorld(newItem);
                                }
                            }


                            SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> <color=#4A7FAA>Scavenger:</color>\nYou found <color=").Append(LUCK_COLOR).Append(">").Append(newAmt).Append("</color> extra <color=").Append(LUCK_COLOR).Append(">").Append(item?.info?.displayName?.english ?? "Unknown").Append("</color> while looting.").ToString());


                            xpMin = 112;
                            xpMax = 385;

                            rndXP = DoCatchup(player.userID, DoMultipliers(player.userID, Random.Range(xpMin, xpMax) * (isScrap ? (newAmt * 0.225f) : newAmt)));

                            AddXP(player.UserIDString, rndXP, XPReason.Loot);

                            PlayCodeLockUpdate(player, 2);
                        }
                    }
                    finally { Pool.Free(ref sb); }
                }
            }



            var scrapper = luckInfo.GetStatLevelByName("Scrapper");

            if (scrapper > 0)
            {
                var isWaterCrate = entity.ShortPrefabName.Contains("underwater");
                var isAdvancedWater = entity.ShortPrefabName.Contains("underwater_advanced");

                var rng = Random.Range(0f, 101f);
                var scrapPerc = (corpse != null ? 3.125f : 0f) + (1.375f * scrapper); // + UnityEngine.Random.Range(0f, Mathf.Clamp(scrapper / UnityEngine.Random.Range(1.1f, 2f), 1f, 5f)); //03/16/2022 update: wtf was this? I commented it out. really weird. why would I do this?
                if (isWaterCrate) scrapPerc += !isAdvancedWater ? 5f : 8f;


                var rngNeed = GetChanceScaledDouble(player.userID, GetScaledScrapNumber(player.userID, scrapPerc, scrapPerc, 100), 1f, 100f, scalar);


                if (rng <= rngNeed)
                {
                    var minAdd = scrapper == 1 ? Random.Range(1, 2) : scrapper == 2 ? Random.Range(1, 3) : scrapper == 3 ? Random.Range(2, 4) : scrapper == 4 ? Random.Range(3, 5) : (scrapper - 1);
                    var scrapMin = Random.Range(0, 2) + minAdd;
                    var scrapMax = Random.Range(1, 4) + minAdd;
                    if (scrapMin > scrapMax) scrapMax = scrapMin;
                    var scrapAdd = Random.Range(scrapMin, scrapMax);

                    var findScrap = FindItemInContainer(lootInventory, SCRAP_ITEM_ID);


                    if (findScrap != null)
                    {
                        findScrap.amount += scrapAdd;
                        findScrap.MarkDirty();
                    }
                    else
                    {
                        var newScrap = ItemManager.CreateByItemID(SCRAP_ITEM_ID, scrapAdd);
                        if (!newScrap.MoveToContainer(lootInventory) && !newScrap.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation))
                        {
                            PrintWarning("no move or drop scrap!!");
                            RemoveFromWorld(newScrap);
                        }
                    }

                    PlayScrapEffect(player, Random.Range(1, 3));

                    var addXP = DoMultipliers(player.userID, DoCatchup(player.userID, Random.Range(32, 64) * scrapAdd));

                    AddXP(player.UserIDString, addXP, XPReason.Loot);

                    var sb = Pool.Get<StringBuilder>();
                    try 
                    {
                        SendReply(player, sb.Clear().Append(LUCK_COLOR_FORMATTED).Append("Lucky!</color> You found an extra ").Append(LUCK_COLOR_FORMATTED).Append(scrapAdd.ToString("N0")).Append("</color> Scrap!").ToString());

                    }
                    finally { Pool.Free(ref sb); }
                }
            }

            var harvestLvl = luckInfo.GetStatLevelByName("NotesHarvest");
            if (harvestLvl < 1) return;
         
            var batRng = Random.Range(0f, 101f);

            var batPerc = (corpse != null ? 8 : 6.5) * harvestLvl;

            batPerc = GetChanceScaledDouble(player.userID, batPerc, batPerc, 99, scalar);

            if (Random.Range(0, 101f) > batPerc)
                return;

            //these 5 lines below brought to you by the fellas at ChatGPT
            var scaleFactor = harvestLvl / luckInfo.GetStatInfoByName("NotesHarvest").MaxLevel;
            var minRange = Mathf.RoundToInt(1 + scaleFactor * 3);
            var maxRange = Mathf.RoundToInt(3 + scaleFactor * 3);

            var batAmountMin = Random.Range(minRange, minRange + 2);
            var batAmountMax = Random.Range(maxRange, maxRange + 2);

            var batAmount = Random.Range(batAmountMin, batAmountMax);

            AddBatteries(lootInventory, batAmount);

            var batMsg = "<color=" + LUCK_COLOR + ">Lucky!</color> You found batteries while looting.";
            SendLuckNotifications(player, batMsg);

            xpMin = 64;
            xpMax = 129;

            rndXP = DoCatchup(player.userID, DoMultipliers(player.userID, Random.Range(xpMin, xpMax) * batAmount));

            AddXP(player.UserIDString, rndXP, XPReason.Loot);

            PlayBatteryEffect(player, 2);

        }

        //todo: make legendary map marker last as long as item does, also, only network to nearby players
        private void OnItemDropped(Item item, BaseEntity entity)
        {
            if (item == null || entity == null) return;

            var droppedItem = entity as DroppedItem;
            if (droppedItem == null)
                return;


            allDropped.Add(droppedItem);

            var leg = LegendaryItemInfo.GetLegendaryInfoFromItem(item);

            if (leg == null)
                return;

            //add a notification for the player who dropped. may not be possible in *this* hook

            //assets/prefabs/weapons/arms/effects/drop_item.prefab
            //assets/bundled/prefabs/fx/armor_break.prefab

            var newMap = (MapMarkerGenericRadius)GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", droppedItem.CenterPoint());

            newMap.radius = 0.075f;


            newMap.OwnerID = 1996200123;

            var mainColor = new Color(0.5098f, 0.298f, 0.1019f, 1f);

            newMap.color1 = mainColor;
            newMap.color2 = new Color(0.862f, 0.623f, 0.286f, 0.75f); //outline color
            newMap.alpha = 1f;

            newMap.Spawn();
            newMap.SendUpdate();
            newMap.SendNetworkUpdateImmediate(true);

            newMap.InvokeRepeating(() =>
            {
                if (newMap == null || newMap.IsDestroyed) return;

                if (droppedItem == null || droppedItem.IsDestroyed)
                    newMap.Kill();
                

            }, 5f, 5f);
        }

        private object CanExistWith(ItemModWearable original, ItemModWearable target)
        {
            if (original == null || target == null) return null;

            try
            {
                if (original?.targetWearable == null || target?.targetWearable == null) return true;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
            }



            return null;
        }

        public double CalculateLampXP(long skillXP, int lampSize)
        {
            return 0;
        }

        private object OnItemRefill(Item item, BasePlayer player) //why does this feel like such a mess
        {
            var reqLvl = item?.info?.GetComponent<ItemModRepair>()?.workbenchLvlRequired ?? 0;

            if (reqLvl < 1)
                return null;
            else if (reqLvl > 0 && player?.triggers == null)
            {
                ShowToast(player, MUST_USE_WORKBENCH_REFILL_TOAST, 1);
                return true; //true to not allow
            }
            

            var hasReq = false;
          
            for (int i = 0; i < player.triggers.Count; i++)
            {
                var benchTrig = player.triggers[i] as TriggerWorkbench;

                if (benchTrig != null && benchTrig.WorkbenchLevel() >= reqLvl)
                {
                    hasReq = true;
                    break;
                }

            }

            if (!hasReq)
            {
                ShowToast(player, MUST_USE_WORKBENCH_REFILL_TOAST, 1);
                return true;
            }

            return null;
        }

        private object OnItemAction(Item item, string action, BasePlayer player)
        {
            if (item?.skin == MOUSTACHE_COOKIE_SKIN_ID && player != null && action.Equals("consume", StringComparison.OrdinalIgnoreCase))
            {
                

                AddXPModifier(player.UserIDString, "LK", new XPModifier { ExpirationTime = Time.realtimeSinceStartup + 10, Multiplier = 0.1, Reason = "hi lol" });
                SendReply(player, "Yum! Enjoy an additional 10% Luck XP for 5 minutes.\nTotal XP rate now: " + GetXPMultiplier(player.UserIDString) + "%");
                ShowToast(player, "Yum! 10% more Luck XP for 5 minutes.\nTotal XP rate now: " + GetXPMultiplier(player.UserIDString) + "%");

                return null;
            }

            //also has large present
            if (!(item?.info?.itemid == WRAPPED_GIFT_ITEM_ID || item?.info?.itemid == 479292118 || item?.info?.itemid == -1622660759) || !(action.Equals("open", StringComparison.OrdinalIgnoreCase) || action.Equals("unwrap", StringComparison.OrdinalIgnoreCase))) return null; //only want wrapped gift being unwrapped or large loot bag being unwrapped

            if (item.skin == GENIE_LAMP_SKIN_ID_LUCK)
            {

                try
                {
                    var xp = (float)CalculateLampXP((long)getPointsS(player.UserIDString), 4);

                    PrintWarning("xp is: " + xp);

                    AddXP(player.UserIDString, xp);

                    CelebrateEffectsNoFireworks(player);
                }
                finally
                {
                    if (item.amount > 1)
                    {
                        item.amount--;
                        item.MarkDirty();
                    }
                    else RemoveFromWorld(item);
                }

                return true;
            }

            var itemName = item?.name ?? string.Empty;

            if (IsBackpackItem(item))
            {

                var containerEntity = GetBackpackContainerByID(item.uid);

                if (containerEntity == null)
                {
                    PrintWarning("initial container entity was null, creating anew");

                    containerEntity = SpawnBackpack(player.transform.position, item.skin);
                    _itemUIDToBackPackContainerID[item.uid.Value] = containerEntity.net.ID.Value;
                }

                var storage = containerEntity as StorageContainer;
                var dropped = containerEntity as DroppedItemContainer;


                if (containerEntity == null || containerEntity.IsDestroyed)
                {
                    PrintWarning("stash null/destroyed on unwrap!!!");
                    return true;
                }

                var wasLootingBackpack = player?.inventory?.loot?.entitySource == containerEntity;

                player.EndLooting();


                if (wasLootingBackpack)
                    return true;

                StartLootingContainer(player, containerEntity, false);


                if (containerEntity?.GetParentEntity() != player)
                {
                    SendReply(player, BACKPACK_EQUIP_NOTE);
                    ShowToast(player, BACKPACK_EQUIP_NOTE, 1);
                }

                return true;
            }

            var isActiveToken = itemName.Equals(TOKEN_ACTIVE_NAME, StringComparison.OrdinalIgnoreCase);

            if (!isActiveToken && !itemName.Equals(TOKEN_INACTIVE_NAME, StringComparison.OrdinalIgnoreCase)) return null;

            var userClass = GetUserClassType(player.UserIDString);
            if (userClass == ClassType.Undefined)
            {
                SendReply(player, "You cannot use this! You do not currently have a class selected.");
                return null;
            }

            if (isActiveToken)
            {
                PrintWarning("is active token!!!");

                try
                {
                    ResetUserClass(player.UserIDString);

                    var announceMsg = "<size=25><color=#E5823B>" + player?.displayName + "</color></size> has just <i>reset their class</i> using a <size=20><color=#FF3730><i>Token</color> of <color=#FF3730>Absolution</color>!</i></size>";
                    SimpleBroadcast(announceMsg, CHAT_STEAM_ID);

                    SendLocalEffect(player, "assets/prefabs/missions/effects/mission_victory.prefab");

                    DiscordAPI2?.Call("SendChatMessageByName", RemoveTags(announceMsg), "PRISM | BLUE", PRISM_BLUE_ICON_DISCORD); //must supply a URL image link at end, not ID

                    //sound fx, text

                    return true;
                }
                finally { RemoveFromWorld(item); }

            }
            else
            {
                item.name = TOKEN_ACTIVE_NAME;
                item.skin = TOKEN_ACTIVE_SKIN_ID;
                item.MarkDirty();

                SendLocalEffect(player, "assets/prefabs/missions/effects/mission_objective_complete.prefab"); //temp? idk yet.

                //sound fx, text improvements

                SendReply(player, "Click '<i>unwrap</i>' on your <i>Token of Absolution</i> again to confirm your decision! After clicking it again, you cannot go back.\n<size=20>This is irreversible.</size> <size=26>An admin will not help you undo this.</size>");
                ShowToast(player, "Click <i>unwrap</i> on your Token to confirm your decision. This is irreversible!", 1);

                return true;
            }
        }

        private void OnLoseCondition(Item item, ref float amount) //the way this hook works is so stupid. go look at IOnLoseCondition in Oxide
        {

            if (item != null && item == _grenadeHatchet)
            {
                amount = 0f;
                return;
            }

            var ownerPlayer = item?.GetOwnerPlayer();

            if (IsLegendaryItem(item))
            {
                amount *= 0.0667f;

                //perf opt:

                if (ownerPlayer != null && ownerPlayer.IsConnected && !_activeWhirlwind.Contains(ownerPlayer.UserIDString))
                    ownerPlayer.Invoke(() => item?.MarkDirty(), 0.01f);
            }

            if (ownerPlayer == null || !ownerPlayer.IsConnected)
                return;
            

            if (item.info.category == ItemCategory.Weapon)
            {
                if (_overDrivePlayers.Contains(ownerPlayer.userID))
                    amount *= 0.01f; //minimal damage loss
                else if (GetUserClassType(ownerPlayer.UserIDString) == ClassType.Mercenary)
                    amount *= 0.33f;

            }

            var itemId = item?.info?.itemid ?? 0;

            var isGreenKeycard = itemId == 37122747;
            var isBlueKeycard = itemId == -484206264;
            var isRedKeycard = itemId == -1880870149;

            if (!isGreenKeycard && !isBlueKeycard && !isRedKeycard) return;

            var userClassType = GetUserClassType(ownerPlayer.UserIDString);
            if (userClassType != ClassType.Packrat && userClassType != ClassType.Mercenary) return;

            var userClass = GetUserClass(ownerPlayer.UserIDString);


            var oldAmount = amount;

            var scalar = isGreenKeycard ? 0.25f : isBlueKeycard ? 0.5f : 0.75f;

            scalar *= userClassType == ClassType.Mercenary ? 1.15f : 1f;

            amount *= scalar;

            ownerPlayer.Invoke(() =>
            {
                if (ownerPlayer == null || !ownerPlayer.IsConnected) return;

                item.MarkDirty();

            }, 0.01f);

            var useColor = isRedKeycard ? "#754038" : isBlueKeycard ? "#2A3D76" : "#447152";

            var sb = Pool.Get<StringBuilder>();
            try
            {
                
                SendLuckNotifications(ownerPlayer, sb.Clear().Append("<color=").Append(userClass.PrimaryColor).Append(">").Append(userClass.DisplayName).Append("</color>: <color=").Append(userClass.SecondaryColor).Append(">Your <color=").Append(useColor).Append(">").Append(item.info.displayName.english).Append("</color> took ").Append(scalar).Append("x damage!").ToString());
            }
            finally { Pool.Free(ref sb); }
        }

        private object OnOverrideMaxStackableForCupboard(Item item, int maxStack)
        {
            var maxStackableObj = OnMaxStackable(item);

            if (maxStackableObj == null) return null;

            var maxStackableInt = (int)maxStackableObj;
            if (maxStackableInt > maxStack) return maxStackableInt;

            return null;
        }

        //todo: fix backpack stacks being able to be dragged out
        private object OnMaxStackable(Item item)
        {
            if (item == null) return null;

            if (IsBackpackItem(item))
                return 1;

            var player = item?.GetOwnerPlayer();

            if (player != null && (Deathmatch?.Call<bool>("InAnyDM", player.UserIDString) ?? false)) 
                return null;

            var entityOwner = item?.parent?.GetEntityOwner();

            var ownerPrefabId = entityOwner?.prefabID ?? 0;

            var userId = player?.UserIDString ?? (entityOwner?.OwnerID ?? 0).ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userId)) return null;

            var maxStack = item?.info?.stackable ?? 0;

            if (maxStack > 1000000000 || maxStack < 2)
                return null; // 1 billion or not stackable

            var userClass = GetUserClassType(userId);

            var stackMultiplier = ShouldApplyStackMultiplierForClass(userClass, item) ? 2.0d : 1.0d;


            //todo: check different types of backpack via entity owner skinid

            if (ownerPrefabId == 1519640547 || ownerPrefabId == 545786656) //item_drop_back, item_drop, respectively
            {
                var toAdd = entityOwner.skinID switch
                {
                    LONERS_BACKPACK_SKIN_ID => 2.0d,
                    LARGE_BACKPACK_SKIN_ID => 1.5d,
                    MEDIUM_BACKPACK_SKIN_ID => 1.25d,
                    _ => 1.0d,
                };
                stackMultiplier += toAdd;
            }


            if (stackMultiplier != 1.0d)
            {
                var multipliedStack = maxStack * stackMultiplier;

                if (multipliedStack < 0 || (multipliedStack < 0 && stackMultiplier >= 1.0d)) 
                    return null; //int overflow or some other sort of weird issue. this was happening with water, the water stack size is apparently int.maxvalue (2 billion), and the code tried to double it, resulting in issues (kicks)

                return (int)Math.Round(multipliedStack, MidpointRounding.AwayFromZero);
            }

            return null;
        }

        private object CanStackItem(Item item, Item targetItem)
        {
            if (item == null || targetItem == null) return null;

            //prevent an anglers boon bait item from stacking to an existing item - CanAcceptItem handles the majority of this.
            if (_anglersBoonBaitItems.Contains(item.uid) || _anglersBoonBaitItems.Contains(targetItem.uid))
                return false;

            if (item.info != targetItem.info || !IsFish(item)) return null;

            var parent = item?.parent?.entityOwner ?? null;
            if (parent == null || parent.prefabID != FISH_TRAP_PREFAB_ID || parent.OwnerID == 0 || HasAnyLootersInCrate(parent)) return null;

            HandleFishing(parent.OwnerID, targetItem, parent);

            return null;
        }


        private BaseCombatEntity _testBackpack = null;

        private object CanRemoveDroppedItemContainer(DroppedItemContainer container)
        {
            if (container?.GetParentEntity() is BasePlayer)
                return false;
            

            if (_itemUIDToBackPackContainerID == null)
            {
                PrintError(nameof(_itemUIDToBackPackContainerID) + " is null!!");
                return null;
            }

            foreach (var kvp in _itemUIDToBackPackContainerID)
            {
                var findId = BaseNetworkable.serverEntities.Find(new NetworkableId(kvp.Value)) as BaseCombatEntity;

                if (findId != null && findId == container)
                {
                    PrintWarning("it tried to kill a dropped item container, but we found it in _itemuid!");

                    return false;
                }

            }

            return null;
        }

        private object CanKillDroppedItemContainer(DroppedItemContainer container, BasePlayer player)
        {
            if (container == null || player == null) return null;


            if ((_userBackpack.TryGetValue(player.UserIDString, out BaseCombatEntity pack) && pack == container) || container?.GetParentEntity() is BasePlayer)
                return false;

            foreach (var kvp in _itemUIDToBackPackContainerID)
            {
                var findId = BaseNetworkable.serverEntities.Find(new NetworkableId(kvp.Value)) as BaseCombatEntity;

                if (findId != null && findId == container)
                    return false;
            }



            return null;
        }

        private BaseCombatEntity SpawnBackpack(Vector3 position, ulong skinId, ulong ownerId = 0)
        {
            var spawnPos = new Vector3(position.x, -10f, position.z);

            var prefabName = (skinId == LONERS_BACKPACK_SKIN_ID || skinId == LARGE_BACKPACK_SKIN_ID) ? "assets/prefabs/misc/item drop/item_drop_backpack.prefab" : "assets/prefabs/misc/item drop/item_drop.prefab";

            var baseBackpack = (BaseCombatEntity)GameManager.server.CreateEntity(prefabName, spawnPos);
            baseBackpack.skinID = skinId;

            baseBackpack.Spawn();

            var droppedContainer = baseBackpack as DroppedItemContainer;
            if (droppedContainer != null)
            {

                var capacity = skinId == LONERS_BACKPACK_SKIN_ID ? 26 : skinId == LARGE_BACKPACK_SKIN_ID ? 19 : skinId == MEDIUM_BACKPACK_SKIN_ID ? 14 : 6;

                droppedContainer.inventory = new ItemContainer();
                droppedContainer.inventory.ServerInitialize(null, capacity); //large backpack -- uh, this thing <-- over there, before the --, idk what that comment means. but i wrote it.
                droppedContainer.inventory.GiveUID();   
                droppedContainer.inventory.entityOwner = baseBackpack;

                droppedContainer.Invoke(() =>
                {
                    droppedContainer.CancelInvoke(new Action(droppedContainer.RemoveMe));
                }, 1f);
            }

            baseBackpack.EnableSaving(false);

            baseBackpack.Invoke(() =>
            {
                if (baseBackpack != null && !baseBackpack.IsDestroyed) baseBackpack.EnableSaving(true);
            }, 1f);

            var rigid = baseBackpack?.GetComponent<Rigidbody>() ?? baseBackpack?.GetComponentInParent<Rigidbody>() ?? baseBackpack?.GetComponentInChildren<Rigidbody>() ?? null;
            if (rigid != null)
            {
                rigid.isKinematic = true;
                rigid.useGravity = false;
            }

            return baseBackpack;

        }

        private BaseCombatEntity CreateBackpackEntityForPlayer(BasePlayer player, Item item)
        {
            if (player == null || item == null) return null;

            var wearable = item?.info?.GetComponent<ItemModWearable>() ?? item?.info?.gameObject?.AddComponent<ItemModWearable>();

            if (wearable != null && item.info.ItemModWearable == null)
                _itemModWearable.SetValue(item.info, wearable);


            var containerEntity = GetBackpackContainerByID(item.uid);

            if (!_userBackpack.TryGetValue(player.UserIDString, out BaseCombatEntity baseBackpack) || (baseBackpack?.IsDestroyed ?? true))
            {
                if (containerEntity != null) //messy attempt at fix for backpacks somehow sticking to original owner after another player takes it
                {
                    var toRemove = string.Empty;

                    foreach (var kvp in _userBackpack)
                    {
                        if (kvp.Value == containerEntity)
                        {
                            toRemove = kvp.Key;
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(toRemove))
                    {
                        PrintWarning("got toRemove: " + toRemove);

                        var didRemove = _userBackpack.Remove(toRemove);

                        PrintWarning("successful?: " + didRemove);
                    }

                }

                baseBackpack = containerEntity;

                if (baseBackpack == null)
                {
                    PrintWarning("backpack null or destroyed! must create again (userBackpack couldn't get or was destroyed");

                    baseBackpack = SpawnBackpack(player.transform.position, item.skin);
                }

            }

            var isMedium = item.skin == MEDIUM_BACKPACK_SKIN_ID;
            var isSmall = item.skin == SMALL_BACKPACK_SKIN_ID;

            _userBackpack[player.UserIDString] = baseBackpack;
            _itemUIDToBackPackContainerID[item.uid.Value] = baseBackpack.net.ID.Value;


            baseBackpack.SetParent(null, false, false);

            baseBackpack.transform.position = player.transform.position;

            baseBackpack.SetParent(player, "spine1", true, true);

            //use -1f if using item_drop for backpack for localrot x (otherwise it is upside down)

            baseBackpack.transform.localRotation = new Quaternion((isMedium || isSmall) ? -1f : 1f, 0f, -1f, -0.05f);
            baseBackpack.transform.localPosition = new Vector3(-0.16f, -0.15f, 0.0125f); // -0.11 -0.0635 0.01

            ///bpt2 -0.21 -0.15 -0.0125
            ///bpt2 -0.18 -0.15 -0.0125

            baseBackpack.OwnerID = player.userID;

            var inventory = (baseBackpack as StorageContainer)?.inventory ?? (baseBackpack as DroppedItemContainer)?.inventory;

            if (inventory != null) 
                inventory.playerOwner = player;

            var rigid = baseBackpack?.GetComponent<Rigidbody>() ?? baseBackpack?.GetComponentInParent<Rigidbody>() ?? baseBackpack?.GetComponentInChildren<Rigidbody>() ?? null;
            if (rigid != null)
            {
                rigid.isKinematic = true;
                rigid.useGravity = false;
            }

            return baseBackpack;
        }

        private ItemContainer GetInventoryFromBackpack(BaseCombatEntity backpack)
        {
            return (backpack as StorageContainer)?.inventory ?? (backpack as DroppedItemContainer)?.inventory;
        }

        private void ResetParentedPositionAndRotationForBackpack(BaseCombatEntity baseBackpack)
        {
            if (baseBackpack == null) return;

            //use -1f if using item_drop for backpack for localrot x (otherwise it is upside down)

            var isMedium = baseBackpack.skinID == MEDIUM_BACKPACK_SKIN_ID;
            var isSmall = baseBackpack.skinID == SMALL_BACKPACK_SKIN_ID;

            baseBackpack.transform.localRotation = new Quaternion((isMedium || isSmall) ? -1f : 1f, 0f, -1f, -0.05f);
            baseBackpack.transform.localPosition = new Vector3(-0.16f, -0.15f, 0.0125f); // -0.11 -0.0635 0.01
        }

        private void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null) return;
            if (container?.entityOwner == null && container?.playerOwner == null) return;

            var player = container?.playerOwner ?? container?.GetOwnerPlayer() ?? FindPlayerByContainer(container, true) ?? null;


            if (container == player?.inventory?.containerWear && item.skin == MINE_CAP_SKIN_ID) //player just equipped mine cap legendary
            {
                foreach (var fbId in _oreFireballs)
                {
                    if (player == null || !player.IsConnected || player.IsDead()) break;

                    var fb = BaseNetworkable.serverEntities.Find(fbId);
                    if (fb == null || fb.IsDestroyed) continue;


                    SendNetworkUpdate(fb, BasePlayer.NetworkQueue.Update, player);

                }
            }

            if (IsBackpackItem(item))
            {
                var wearable = item?.info?.GetComponent<ItemModWearable>() ?? item?.info?.gameObject?.AddComponent<ItemModWearable>();

                if (wearable != null && item.info.ItemModWearable == null)
                    _itemModWearable.SetValue(item.info, wearable);

                if (player != null && !player.IsSleeping() && player.IsAlive() && container == player.inventory.containerWear)
                {
                    
                    var ent = CreateBackpackEntityForPlayer(player, item);


                    var inventory = (ent as DroppedItemContainer)?.inventory ?? (ent as StorageContainer)?.inventory;

                    inventory?.SetOnlyAllowedItem(null);

                }

            }


            var legendary = LegendaryItemInfo.GetLegendaryInfoFromItem(item);
            if (legendary != null && player != null && container == player.inventory.containerWear)
            {
                NextTick(() =>
                {
                    if (legendary == null || player == null || player.IsDestroyed || !player.IsConnected || player.IsDead() || item?.parent != container) return;

                    var sb = Pool.Get<StringBuilder>();
                    try 
                    {
                        var modStr = legendary.GetModifiersString();
                        SendLuckNotifications(player, sb.Clear().Append("<color=#F37B00>").Append(legendary.DisplayName).Append("</color>:").Append(!string.IsNullOrWhiteSpace(modStr) ? "\n" : "").Append(modStr).Append(!string.IsNullOrWhiteSpace(legendary.BonusDescription) ? "\n" : "").Append(legendary.BonusDescription).ToString(), true, 3f);
                    }
                    finally { Pool.Free(ref sb); }

                });
            }

            if (IsCharm(item))
            {
                var getCharm = charmsData?.GetCharm(item) ?? null;
                if (getCharm == null || getCharm.Type == CharmType.Small) return;


                if (player == null) return;

                if (ShouldHaveCharmComponent(player)) 
                    EnsureCharmComponent(player);

                if (container != player.inventory.containerMain) 
                    return;

                var pCharms = Pool.GetList<Charm>(); //GetPlayerCharms(player);

                var largeCount = 0;
                var grandCount = 0;

                try 
                {
                    GetPlayerCharmsNoAlloc(player, ref pCharms);

                    if (pCharms.Count > 0)
                    {
                        for (int i = 0; i < pCharms.Count; i++)
                        {
                            var charmType = pCharms[i].Type;
                            if (charmType == CharmType.Large) largeCount++;
                            else if (charmType == CharmType.Grand) grandCount++;
                        }
                    }

                }
                finally { Pool.FreeList(ref pCharms); }

                var cap = Mathf.Clamp(24 - ((grandCount * 4) + (largeCount * 3)), 0, 24);
          
                if (container.capacity == cap)
                    return;
                
                player.inventory.containerMain.capacity = cap;
                player.inventory.ServerUpdate(0.01f);

                var lostItems = Pool.GetList<Item>();

                try
                {
                    for (int i = 0; i < player.inventory.containerMain.itemList.Count; i++)
                    {
                        var mainItem = player.inventory.containerMain.itemList[i];
                        if (mainItem != null && mainItem.position >= cap) lostItems.Add(mainItem);
                    }

                    for (int i = 0; i < lostItems.Count; i++)
                    {
                        var lostItem = lostItems[i];
                        if (lostItem == null)
                            continue;



                        var lastSpot = GetLastFreePosition(container);
                        if (lastSpot == -1)
                        {
                            PrintWarning("GetLastFreePosition == -1, GetFreePosition == " + (lastSpot = GetFreePosition(container)) + " item: " + lostItem.info.shortname + " x" + lostItem.amount);
                            lastSpot = GetFreePosition(container);
                        }

                        var didMove = false;
                        if ((lastSpot > 0 && !(didMove = lostItem.MoveToContainer(container, lastSpot))) || lastSpot < 0)
                        {
                            if (!lostItem.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.transform.rotation))
                                RemoveFromWorld(lostItem);
                            else PrintWarning("did drop lost item: " + lostItem.info.shortname + " x" + lostItem.amount);
                        }
                    }
                }
                finally { Pool.FreeList(ref lostItems); }
            }

            if (!init || container?.entityOwner == null || container.entityOwner.prefabID != FISH_TRAP_PREFAB_ID || _ignoreFish.Contains(item.uid) || !IsFish(item)) return;

            var ownerID = container?.entityOwner?.OwnerID ?? 0;

            if (ownerID != 0 && !HasAnyLootersInCrate(container.entityOwner))
                HandleFishing(ownerID, item, container.entityOwner);
        }

        private void OnFishCatch(Item item, BaseFishingRod rod, BasePlayer player) //only for fishing rods - not traps
        {
            if (item == null || rod == null || player == null) return;

            HandleFishingRod(player, item);

            if (!IsWearingSkin(player, ANGLERS_BOON_SKIN_ID))
                return;

            var rodItem = rod?.GetItem();

            if (rodItem == null)
            {
                PrintError("rod item null?!?!?!");
                return;
            }

            if (rodItem.contents.GetSlot(0) != null)
                return; //already had bait!

            var newBait = ItemManager.Create(item.info, 1);
            if (!newBait.MoveToContainer(rodItem.contents))
            {
                RemoveFromWorld(newBait);

                PrintError("Couldn't move new bait?!?!?!?");
                return;
            }
            else if (_anglersBoonBaitItems.Add(newBait.uid))
            {
                var sb = Pool.Get<StringBuilder>();

                try 
                {
                    SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Anglers' Boon!</color> 1 ").Append(item.info.displayName.english).Append(" has been added to your rod's bait!").ToString());
                }
                finally { Pool.Free(ref sb); }

            }

        }

        private void HandleFishingRod(BasePlayer player, Item item)
        {
            if (player == null || item == null) return;

            var fishLvl = GetLuckInfo(player.UserIDString)?.GetStatLevelByName("Fishing") ?? 0;
            if (fishLvl < 1) return;

            var userId = player?.userID ?? 0;

            var amtMax = 1.5f * fishLvl;
            var amtMin = Mathf.Clamp(amtMax * 0.66f, 1, amtMax > 1 ? (amtMax - 1) : amtMax);

            var amtExtra = (amtMax == 1) ? 1 : Random.Range(amtMin, amtMax);

            var fishNeededRng = 9.25d * fishLvl;
            fishNeededRng = GetChanceScaledDouble(userId, fishNeededRng, fishNeededRng, 100);

            var fishRng = Random.Range(0, 101);

            if (fishRng > fishNeededRng)
                return;

            //rng hit!

            var rndXP = Random.Range(amtExtra * 12, amtExtra * 24);

            var finalXp = DoCatchup(userId, DoMultipliers(userId, rndXP));

            AddXP(player.UserIDString, finalXp, XPReason.Fish);

            var finalAddedItemAmount = (int)Mathf.Clamp((float)Math.Round(amtExtra, MidpointRounding.AwayFromZero), 1, amtExtra);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var fishMsg = sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> You caught an extra <color=").Append(LUCK_COLOR).Append(">").Append(finalAddedItemAmount).Append("</color> <color=#739895>").Append(item.info.displayName.english).Append("</color>!").ToString();
                SendLuckNotifications(player, fishMsg);
            }
            finally { Pool.Free(ref sb); }

            item.amount += finalAddedItemAmount;
            item.MarkDirty();

        }

        private void HandleFishing(ulong userID, Item item, BaseEntity entity)
        {
            if (userID == 0 || item == null || entity == null) throw new ArgumentNullException();

            var fishLvl = GetLuckInfo(userID)?.GetStatLevelByName("Fishing") ?? 0;
            if (fishLvl < 1) return;

            var player = FindPlayerByID(userID);
            var distance = (player == null || player?.transform == null || entity?.transform == null) ? -1f : Vector3.Distance(player.transform.position, entity.transform.position);

            var amtMax = 1 * fishLvl;
            var amtMin = (int)Mathf.Clamp(amtMax * 0.66f, 1, amtMax > 1 ? (amtMax - 1) : amtMax);

            var amtExtra = (amtMax == 1) ? 1 : Random.Range(amtMin, amtMax);

            var fishNeededRng = 7 * fishLvl;
            fishNeededRng = GetChanceScaledInteger(userID, fishNeededRng, fishNeededRng, 100);

            var fishRng = Random.Range(0, 101);

            if (fishRng > fishNeededRng)
                return;

            //rng hit!

            if (player != null && distance != -1f && distance < 26f)
            {

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    var fishMsg = sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> You caught an extra <color=").Append(LUCK_COLOR).Append(">").Append(amtExtra).Append("</color> <color=#739895>").Append(item.info.displayName.english).Append("</color>!").ToString();
                    SendLuckNotifications(player, fishMsg);
                }
                finally { Pool.Free(ref sb); }



                var rndXP = Random.Range(amtExtra * 12, amtExtra * 24);

                var finalXp = DoCatchup(userID, DoMultipliers(userID, rndXP));

                AddXP(player.UserIDString, finalXp, XPReason.Fish);
            }

            item.amount += amtExtra;
            item.MarkDirty();
        }

        private readonly HashSet<NetworkableId> _lootableItemsRemoved = new();

        private void OnItemStacked(Item newItem, Item oldItem, ItemContainer newContainer)
        {

            var player = newContainer?.playerOwner ?? newContainer?.GetOwnerPlayer() ?? FindLooterFromEntity(newContainer?.entityOwner);

            var playerLoot = player?.inventory?.loot?.entitySource;

            if (playerLoot != null)
            {

                var netId = playerLoot.net.ID;

                _lootableItemsRemoved.Add(netId);


                StopMagpieGUI(player);
            }
        }

        private void OnItemRemovedFromContainer(ItemContainer container, Item item)
        {
            if (item == null) return;


            var player = container?.playerOwner ?? container?.GetOwnerPlayer() ?? item?.GetOwnerPlayer() ?? FindLooterFromEntity(container.entityOwner);

            if (container == player?.inventory?.containerWear && item.skin == CORRUPTION_GLOVES_SKIN_ID)
            {
                PrintWarning("uneqip corruption gloves, reset hp to 100!");

                player.SetMaxHealth(100f);

            }

            if (_anglersBoonBaitItems.Remove(item.uid))
            {
                RemoveFromWorld(item);

                if (player != null) SendReply(player, "Oops! While trying to remove the bait, you fumble the " + item.info.displayName.english + " and drop it back into the water."); //what if player isn't in water? lol xd
                else PrintWarning("player was null!!");
            }

            if (container != null && container == player?.inventory?.containerWear && item.skin == MINE_CAP_SKIN_ID)
            {
                foreach (var fbId in _oreFireballs)
                {
                    if (player == null || !player.IsConnected || player.IsDead()) break;

                    var fb = BaseNetworkable.serverEntities.Find(fbId);
                    if (fb == null || fb.IsDestroyed) continue;



                    DestroyClientEntity(player, fb);
                }
            }

            var isBackpack = IsBackpackItem(item);

            if (isBackpack && player != null && container == player.inventory.containerWear)
            {
                var wasLooting = (player?.inventory?.loot?.entitySource?.skinID ?? 0) == item.skin;

                if (wasLooting)
                {
                    player.Invoke(() =>
                    {

                        if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead())
                            return;

                        if (item?.parent != player?.inventory?.containerMain && item?.parent != player?.inventory?.containerWear && item?.parent != player?.inventory?.containerBelt)
                            player.EndLooting();

                    }, 0.1f);
                }



                if (_userBackpack.TryGetValue(player.UserIDString, out BaseCombatEntity backpack))
                {

                    if (backpack != null && !backpack.IsDestroyed)
                    {

                        var inventory = (backpack as DroppedItemContainer)?.inventory ?? (backpack as StorageContainer)?.inventory;
                        inventory?.SetOnlyAllowedItem(GenericVehicleModuleDef); //generic vehicle module - nonsense, just used to deny items

                        backpack.SetParent(null);
                        backpack.transform.position = new Vector3(20, -10, 20);
                    }
                    else PrintWarning("backpack null/destroyed for user backpack?!?!");

                    _userBackpack.Remove(player.UserIDString);
                }
            }

            /*/
            if (isBackpack)
            {
                foreach (var kvp in _userBackpack)
                {
                    
                }
            }/*/

            var lootSource = player?.inventory?.loot?.entitySource;
            if (lootSource != null)
            {
                var netId = lootSource.net.ID;

                _lootableItemsRemoved.Add(netId);


                StopMagpieGUI(player);
            }


            if (IsCharm(item) && player != null)
            {
                if (!ShouldHaveCharmComponent(player)) RemoveCharmComponent(player);

                var getCharm = charmsData?.GetCharm(item) ?? null;
                if (getCharm == null || getCharm.Type == CharmType.Small || getCharm.Type == CharmType.Undefined) return;
                var slotCount = getCharm.Type == CharmType.Large ? 3 : 4;
                player.inventory.containerMain.capacity = Mathf.Clamp(player.inventory.containerMain.capacity + slotCount, 0, 24);
            }

            if (IsFish(item) && !_ignoreFish.Contains(item.uid)) _ignoreFish.Add(item.uid);
        }

        private void OnPlayerBiomeChanged(BasePlayer player, int lastBiome, int biome)
        {
            if (player == null) return;

            if (GetUserClassType(player.UserIDString) != ClassType.Nomad || (lastBiome != TerrainBiome.ARID && biome != TerrainBiome.ARID))
                return;

            

            var rpgNomadClass = ARPGClass.FindByType(ClassType.Nomad);

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                var nomadPrefix = sb.Clear().Append("<i><color=").Append(rpgNomadClass.PrimaryColor).Append(">").Append(rpgNomadClass.DisplayName).Append("</color>:</i>").ToString();
                var enterStr = sb.Clear().Append(nomadPrefix).Append(" As you enter the desert and feel its breeze, you're slowly reminded of your childhood days. A comfort and relief washes over for you, almost providing a sense of 'home'.\nYou gain 35% more Mining/Woodcutting XP & your gather rates are increased by 30% while in the desert.").ToString();

                var leftStr = sb.Clear().Append(nomadPrefix).Append(" As you leave the desert, you feel as though you're leaving something behind. You gain less XP and have a lower gather rate while outside the desert.").ToString();

                var left = lastBiome == TerrainBiome.ARID && biome != TerrainBiome.ARID;

                SendCooldownMessage(player, left ? leftStr : enterStr, 20f, left ? "NOMAD_LEAVE_DESERT" : "NOMAD_ENTER_DESERT");
            }
            finally { Pool.Free(ref sb); }

        }

        private void OnEntityEnter(TriggerBase trigger, BasePlayer player)
        {
            if (trigger == null || player == null) return;

            if (_userBackpack.TryGetValue(player.UserIDString, out BaseCombatEntity backpack) && backpack != null && !backpack.IsDestroyed)
            {
                player.Invoke(() =>
                {
                    if (backpack == null || backpack.IsDestroyed) return;

                    backpack.SetParent(player, "spine1", true, true);
                    ResetParentedPositionAndRotationForBackpack(backpack);
                }, 0.125f);
            }
        }

        private void OnEntityLeave(TriggerBase trigger, BasePlayer player)
        {
            if (trigger == null || player == null) return;

            if (_userBackpack.TryGetValue(player.UserIDString, out BaseCombatEntity backpack) && backpack != null && !backpack.IsDestroyed)
            {
                player.Invoke(() =>
                {
                    if (backpack == null || backpack.IsDestroyed) return;

                    backpack.SetParent(player, "spine1", true, true);
                    ResetParentedPositionAndRotationForBackpack(backpack);
                }, 0.125f);
            }

            if (trigger is TriggerRadiation radTrigger && (GetActiveItem(player)?.info?.itemid ?? 0) == GEIGER_COUNTER_ITEM_ID)
            {
                player?.Invoke(() =>
                {
                    var activeItem = GetActiveItem(player);

                    if ((activeItem?.info?.itemid ?? 0) != GEIGER_COUNTER_ITEM_ID || GetMaxRadiationTrigger(player) != null)
                        return;

                    //still has geiger

                    var oldParent = activeItem?.parent;
                    var oldPos = activeItem?.position ?? -1;

                    activeItem.RemoveFromContainer();

                    player.UpdateActiveItem(new ItemId(0));
                    player.SendEntityUpdate();

                    player?.Invoke(() =>
                    {
                        if (activeItem == null || !activeItem.IsValid())
                            return;

                        var dropPos = player?.GetDropPosition() ?? Vector3.zero;
                        var dropVel = player?.GetDropVelocity() ?? Vector3.zero;
                        if (!activeItem.MoveToContainer(oldParent, oldPos) && !activeItem.Drop(dropPos, dropVel))
                            RemoveFromWorld(activeItem);

                    }, 0.075f);

                }, 0.01f);
            }

        }

        private void AdjustSleepingBagForNomad(BasePlayer player, SleepingBag bag)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (bag == null)
                throw new ArgumentNullException(nameof(bag));
            

            if (GetUserClassType(player.UserIDString) != ClassType.Nomad)
            {
                PrintWarning(nameof(AdjustSleepingBagForNomad) + " is being called for a non-nomad (returning, not allowing)!: " + player);
                return;
            }
            

            var userClass = GetUserClass(player.UserIDString);

            if (userClass == null)
            {
                PrintWarning("userClass is null!!! " + nameof(AdjustSleepingBagForNomad));
                return;
            }   
            

            var isBag = bag.prefabID == 159326486 || bag.prefabID == 3003382652 || bag.prefabID == 2568114225;

            var newSecs = bag.secondsBetweenReuses * (isBag ? 0.125f : 1.5f);

            var primaryColor = userClass?.PrimaryColor ?? "white";
            var secondaryColor = userClass?.SecondaryColor ?? "yellow";


            var sb = Pool.Get<StringBuilder>();
            try
            {
                SendLuckNotifications(player, sb.Clear().Append(GetOpeningColorTag(primaryColor)).Append("Your ").Append(GetOpeningColorTag(secondaryColor)).Append((isBag ? "sleeping bag" : "bed")).Append("</color> has had its cooldown time changed to <color=#FF884C>").Append(newSecs.ToString("N0")).Append("</color> seconds.</color>").ToString());
            }
            finally { Pool.Free(ref sb); }

            Effect.server.Run("assets/bundled/prefabs/fx/entities/pumpkin/gib.prefab", bag.transform.position);



            bag?.Invoke(() =>
            {
                if (bag == null || bag.IsDestroyed || bag.unlockTime <= Time.realtimeSinceStartup) 
                    return;

                bag.SetUnlockTime(Time.realtimeSinceStartup + newSecs);
            }, 2f);
        }

        private void OnEntityBuilt(Planner plan, GameObject objectBlock)
        {
            var player = plan?.GetOwnerPlayer();
            if (player == null) return;

            var baseEntity = objectBlock?.ToBaseEntity();
            if (baseEntity == null) return;


            var userClassType = GetUserClassType(player.UserIDString);

            if (userClassType == ClassType.Nomad && baseEntity is SleepingBag sleepBag)
                AdjustSleepingBagForNomad(player, sleepBag);
     
            //farmer grant xp on planting a plant code below
            if (userClassType != ClassType.Farmer)
                return;

            var grow = baseEntity as GrowableEntity;
            if (grow == null)
                return;

            if (grow.Genes == null || grow.Genes.Genes == null || grow.Genes.Genes.Length < 1)
            {
                PrintError("grow: " + grow + " has no genes!!!");
                return;
            }

            var isHemp = grow.prefabID == 3587624038;

            var baseXp = isHemp ? 24f : 36f;

            var xpPerYGene = isHemp ? 72 : 80;
            var xpPerGGene = isHemp ? 40 : 48;
            var xpPerHGene = isHemp ? 4 : 6;

         

            var xpToRemovePerW = isHemp ? 64 : 56;
            var xpToRemovePerX = isHemp ? 48 : 40;

            //8 minimum xp per clamp
            var xpToGive = DoCatchup(player.userID, DoMultipliers(player.userID, Mathf.Clamp(baseXp + (xpPerYGene * grow.Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Yield)) + (xpPerGGene * grow.Genes.GetGeneTypeCount(GrowableGenetics.GeneType.GrowthSpeed)) + (xpPerHGene * grow.Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Hardiness)) - (xpToRemovePerW * grow.Genes.GetGeneTypeCount(GrowableGenetics.GeneType.WaterRequirement)) - (xpToRemovePerX * grow.Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Empty)), 8, int.MaxValue)));

            xpToGive += grow.Genes.GetNegativeGeneCount() < 1 ? 128 : 0; //no negative genes? grant lotsa extra xp!

            if (grow.Genes.GetGeneTypeCount(GrowableGenetics.GeneType.GrowthSpeed) == 3 && grow.Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Yield) == 3)
            {
                PrintWarning("somebody just place da pltn with 3x g and 3x y lol lets give extra xp");
                xpToGive += 384;
            }    

            if (player.IsAdmin)
                PrintWarning("plant xpToGive after all scalars: " + xpToGive);

            AddXP(player.UserIDString, xpToGive, XPReason.HarvestPlant);

            /*/
            var block = baseEntity as BuildingBlock;
            if (block == null) return;



            if (!player.IsAdmin) return;

            PrintWarning("player is admin");


            userClass = GetUserClassType(player.UserIDString);

            if (userClass != ClassType.Engineer)
            {
                PrintWarning("userclass isn't builder!! no xp or anything");
                return;
            }

            PrintWarning("builder built, give xp accordingly");

            var baseXp = block.ShortPrefabName == "foundation" ? 96 : block.ShortPrefabName == "wall" ? 72 : block.ShortPrefabName == "wall.low" ? 52 : 64;

            var addXp = DoCatchup(player.userID, DoMultipliers(player.userID, baseXp));

            AddPointsS(player.UserIDString, addXp);

            PrintWarning("added addXp: " + addXp + ", base: " + baseXp);/*/

        }

        private object OnEntityVisibilityCheck(BaseEntity entity, BasePlayer player, uint id, string debugName)
        {
            if (entity == null || player == null) return null;

            if (entity.OwnerID == 528491 && entity is RepairBench) return true;

            if (IsBackpackSkin(entity.skinID) && entity?.GetParentEntity() == player) //makes backpack lootable via alt-look
                return true;
            

            return null;
        }

        private void OnItemRepair(BasePlayer player, Item item) //this is disgusting, but guess what? it works lmaooooooooooooooooo
        {
            if (player == null || item == null) return;

            var removeFromMaxCond = -1f;
            var removeFromCondition = -1f;

            var repairBench = player?.inventory?.loot?.entitySource as RepairBench;
            var isNomadBench = (repairBench?.OwnerID ?? 0) == 528491;

            try
            {
                if (IsLegendaryItem(item))
                {
                    PrintWarning("OnItemRepair for legendary item!!");

                    removeFromMaxCond = item.maxCondition * (isNomadBench ? 0.075f : 0.035f);
                }
                else if (isNomadBench) removeFromMaxCond = item.maxCondition * 0.1f;
            }
            finally
            {
                if (removeFromMaxCond >= 0 || removeFromCondition >= 0)
                {
                    var oldMaxCond = item.maxCondition;
                    var oldCond = item.condition;

                    player.Invoke(() =>
                    {
                        if (item == null) return;

                        if (removeFromMaxCond >= 0)
                        {
                            PrintWarning("remove from max cond is: " + removeFromMaxCond + ", oldmaxcond: " + oldMaxCond + ", current max cond before change: " + item.maxCondition);
                            item.maxCondition = oldMaxCond - removeFromMaxCond;
                        }

                        if (removeFromCondition >= 0)
                        {
                            PrintWarning("remove from cond is: " + removeFromCondition + ", old cond is: " + oldCond + ", current m");
                            item.condition = oldCond - removeFromCondition;
                        }

                        item.condition = item.maxCondition;


                        if (isNomadBench)
                        {
                            PrintWarning("condition normalized: " + item.conditionNormalized);

                            if (item.conditionNormalized < 0 || item.conditionNormalized > 1)
                            {
                                PrintWarning("conditionNormalized is seriously fucked");
                                return;
                            }

                            var basePoints = 112 * item.conditionNormalized;

                            PrintWarning("base points (72 * item.condition): " + basePoints + " (" + item.conditionNormalized + " cond <-- )");

                            var addXp = DoCatchup(player.userID, DoMultipliers(player.userID, basePoints));

                            AddXP(player.UserIDString, addXp);

                            //needs luck sb
                            SendLuckNotifications(player, "<color=" + LUCK_COLOR + ">You've gained </color>" + addXp.ToString("N0") + "<color=" + LUCK_COLOR + "> XP for repairing an item with your Nomad Suit!</color>");
                        }

                    }, 0.01f);
                }
            }





            /*/ //craftsman isn't implemented yet (future class engineer?)
            var craftsman = GetLuckInfo(player)?.Craftsman ?? 0;
            if (craftsman < 1) return;

            var oldMaxCond = item.maxCondition;

            player.Invoke(() =>
            {
                if (item == null)
                {
                    PrintWarning("item null on 0.1 invoke");
                    return;
                }

                var newMaxCond = item?.maxCondition ?? 0f;
                if (newMaxCond < oldMaxCond)
                {
                    var diff = oldMaxCond - newMaxCond;
                    PrintWarning("newMaxCond < oldMaxCond: " + newMaxCond + " < " + oldMaxCond + ", diff: " + diff);

                    var newDiff = diff - (craftsman / 2.5f);
                    PrintWarning("newDiff: " + newDiff);
                    item.maxCondition = oldMaxCond - newDiff;

                    item.condition = item.maxCondition;


                    //  item.maxCondition = Mathf.Clamp(newMaxCond + scaleHeal, newMaxCond, oldMaxCond);
                }

                //PrintWarning("oldmaxcond: " + oldMaxCond + ", now: " + item.maxConditionNormalized);
            }, 0.1f);/*/

        }

        private object OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null) return null;

            if (entity?.net == null)
                return null;
            

            if (_cratesBeingRerolled.Contains(entity.net.ID))
            {
                entity?.Invoke(() =>
                {
                    if (entity == null || entity.IsDestroyed || _cratesBeingRerolled.Contains(entity.net.ID))
                        return;

                    entity.Kill();

                }, 20f); //try kill again in a little bit

                return true;
            }

            if (_oreFireballs.Contains(entity.net.ID))
                return true;
            

            var oreChildren = (entity as OreResourceEntity)?.children;
            if (oreChildren != null)
            {
                for (int i = 0; i < oreChildren.Count; i++)
                {
                    var child = oreChildren[i];

                    if (child == null)
                        continue;

                    if (child.OwnerID == 1221)
                    {
                        _oreFireballs.Remove(child.net.ID);
                        if (!child.IsDestroyed) child.Kill();
                    }

                }
            }


           
           

            var dropped = entity as DroppedItemContainer;
            if (dropped != null && dropped.gameObject != null && IsBackpackSkin(dropped.skinID) && dropped.transform.position.y <= -400)
            {
                dropped.transform.position = new Vector3(20, 5, 20);

                var rigid = dropped?.GetComponent<Rigidbody>() ?? dropped?.GetComponentInParent<Rigidbody>() ?? dropped?.GetComponentInChildren<Rigidbody>() ?? null;
                if (rigid != null)
                {
                    rigid.isKinematic = true;
                    rigid.useGravity = false;
                }

                return true;
            }

            var drop = entity as DroppedItem;
            if (drop != null) 
                allDropped.Remove(drop);

            var loot = entity as LootContainer;

            if (loot == null)
                return null;

            allContainers.Remove(loot);

            //charm recycle!!!
            var watch = Pool.Get<Stopwatch>();

            try 
            {
                watch.Restart();
                if (charmsData?.charms != null && loot?.inventory?.itemList != null && loot.inventory.itemList.Count > 0)
                {
                    var itemList = loot?.inventory?.itemList;
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var item = itemList[i];

                        if (item == null || !IsCharm(item))
                            continue;

                        //try to find a suitable lootcontainer to move this to before destroying it
                        var didMove = false;
                        foreach (var container in allContainers)
                        {
                            if (container == null || container.IsDestroyed || container?.net?.ID == loot?.net?.ID || !IsApplicableContainerForCharm(container.prefabID)) continue;

                            var newList = container?.inventory?.itemList;
                            if (newList == null || newList.Count < 1) continue;

                            var hasCharm = false;
                            for (int j = 0; j < newList.Count; j++)
                            {
                                var item2 = newList[j];
                                if (IsCharm(item2))
                                {
                                    hasCharm = true;
                                    break;
                                }
                            }

                            if (hasCharm) continue;

                            if (item.MoveToContainer(container.inventory))
                            {
                                didMove = true;
                                break;
                            }
                        }

                        if (!didMove)
                        {
                            PrintWarning("Found ZERO applicable containers to move a charm to after its container was killed. Removing charm!");
                            charmsData.charms.Remove(item.uid.Value);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    if (watch.ElapsedMilliseconds > 12) PrintWarning("Took: " + watch.ElapsedMilliseconds + "ms to purge a charm after OnEntityKill!");
                }
                finally { Pool.Free(ref watch); }
            }

            return null;
        }

        private void OnEntityDeath(OreResourceEntity entity, HitInfo info) //unfortunately, TreeEntity does not call this hook - it has an override
        {
            if (entity == null || info == null) return;

            var player = info?.Initiator as BasePlayer;
            if (player == null) return;

            var adjustedChance = _legendaryData.BaseLegendarySpawnChance * 0.10187; //um killa

            if (Random.Range(0.0f, 1.0f) > adjustedChance) 
                return; //no rng hit!

            PrintWarning("rng hit, spawn leg after resource (ore) death!");
            var rngLeg = LegendaryItemInfo.GetRandomLegendaryWithClassBias(GetUserClassType(player.UserIDString));

            if (rngLeg == null) PrintError("rngLeg is null!!!");
            else if (!rngLeg.SpawnIntoInventory(player.inventory.containerMain))
            {
                var item = rngLeg.SpawnItem();

                if (!item.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation))
                {
                    PrintWarning("couldn't drop legendary item after creation!!!");
                    RemoveFromWorld(item);
                }
                else PrintWarning("dropped legendary item after creation :sunglasses:");
            }

            CelebrateLegendarySpawn(player, rngLeg);
        }

        private void OnEntityDeath(LootContainer lootCont, HitInfo info) //(barrels only loot container that dies, I believe?)
        {
            if (lootCont == null || info == null)
                return;

            var attacker = info?.Initiator as BasePlayer;

            if (attacker == null || !IsValidPlayer(attacker))
                return;

            var name = lootCont?.ShortPrefabName ?? string.Empty;

            var luckInfo = GetLuckInfo(attacker);

            if (luckInfo == null)
                return;

            var batteryColLvl = luckInfo?.GetStatLevelByName("NotesHarvest") ?? 0;

            var userClassType = GetUserClassType(attacker.UserIDString);

            var scalar = userClassType == ClassType.Packrat ? 1.25f : (userClassType == ClassType.Undefined || userClassType == ClassType.Loner) ? 1f : 0.5f;


            if (lootCont != null && batteryColLvl > 0)
            {

                var batAmountMin = batteryColLvl == 1 ? 1 : batteryColLvl == 2 ? 2 : (batteryColLvl > 2 && batteryColLvl < 4) ? Random.Range(2, 4) : Random.Range(3, 5);
                var batAmountMax = batteryColLvl == 1 ? 3 : batteryColLvl == 2 ? 4 : (batteryColLvl > 2 && batteryColLvl < 4) ? Random.Range(5, 7) : Random.Range(6, 8);

                var batAmount = Random.Range(batAmountMin, batAmountMax);
                var batRng = Random.Range(0f, 101f);
                var batPerc = 3.88 * batteryColLvl;
                batPerc = GetChanceScaledDouble(attacker.userID, batPerc, batPerc, 99, scalar);

                if (batRng <= batPerc)
                {
                    AddBatteries(lootCont.inventory, batAmount);
                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        SendLuckNotifications(attacker, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> You found <color=").Append(LUCK_COLOR).Append(">").Append(batAmount).Append("</color> batteries!").ToString());
                    }
                    finally { Pool.Free(ref sb); }


                    var xpMin = 80;
                    var xpMax = 145;

                    var rndXP = DoCatchup(attacker.userID, DoMultipliers(attacker.userID, Random.Range(xpMin, xpMax) * batAmount));

                    AddXP(attacker.UserIDString, rndXP, XPReason.Loot);

                    PlayBatteryEffect(attacker, 3);
                }
            }

            if (!name.Contains("barrel") && !name.Contains("trash") && !name.Contains("foodbox") || attacker == null || !attacker.IsConnected) return;

            //hween!!
            if (ConVar.Halloween.enabled && Random.Range(0, 101) <= Random.Range(2, 8))
            {
                var isAngry = Random.Range(0, 101) <= 66;
                var jackol = "jackolantern." + (isAngry ? "angry" : "happy");
                var jackItem = ItemManager.CreateByName(jackol, 1);
                if (jackItem == null)
                {
                    PrintWarning("jackItem null?!: " + jackol);
                    return;
                }

                if (!jackItem.MoveToContainer(lootCont.inventory)) jackItem.Drop(lootCont.GetDropPosition(), lootCont.GetDropVelocity());


                var rngMinXP = Random.Range(isAngry ? 128 : 512, isAngry ? 1280 : 968);
                var rngMaxXP = Random.Range(isAngry ? 768 : 896, isAngry ? 3864 : 2560);
                var rngXP = DoMultipliers(attacker.userID, DoCatchup(attacker.userID, Random.Range(rngMinXP, rngMaxXP)));

                AddXP(attacker.UserIDString, rngXP);

              
                var hweenMsg = "<color=" + LUCK_COLOR + ">Lucky!</color> <color=#e6620a><color=#96467a>" + jackItem.info.displayName.english + "</color> for <color=#96467a>" + rngXP.ToString("N0") + "</color> XP!</color>";
                SendReply(attacker, hweenMsg);
                ShowPopup(attacker, hweenMsg);
                var fxPath = isAngry ? "assets/prefabs/npc/bear/sound/death.prefab" : "assets/prefabs/npc/bear/sound/breathe.prefab";
                var eyePos = attacker?.eyes?.position ?? Vector3.zero;
                for (int i = 0; i < Random.Range(2, 4); i++)
                {
                    SendLocalEffect(attacker, fxPath, eyePos);
                }

            }

            var scrapper = luckInfo?.GetStatLevelByName("Scrapper") ?? 0;
            if (scrapper > 0)
            {

                var rng = Random.Range(0f, 101f);
                var scrapPerc = 3 + (2 * scrapper) + Random.Range(0f, Mathf.Clamp(scrapper / Random.Range(1.1f, 2f), 1f, 5f));
                var rngNeed = GetScaledScrapNumber(attacker.userID, GetChanceScaledDouble(attacker.userID, scrapPerc, scrapPerc, 100, scalar), 1f, 100f);

                var lootInventory = lootCont?.inventory ?? null;
                if (rng <= rngNeed)
                {
                    var minAdd = scrapper == 1 ? Random.Range(0, 2) : scrapper == 2 ? Random.Range(1, 3) : scrapper == 3 ? Random.Range(2, 4) : scrapper == 4 ? Random.Range(3, 5) : (scrapper - 1);
                    var scrapMin = Random.Range(0, 2) + minAdd;
                    var scrapMax = Random.Range(1, 4) + minAdd;
                    if (scrapMin > scrapMax) scrapMax = scrapMin;
                    var scrapAdd = Mathf.Clamp(Random.Range(scrapMin, scrapMax), 1, 999);

                    Item findScrap = null;
                    for (int i = 0; i < lootInventory.itemList.Count; i++)
                    {
                        var item = lootInventory.itemList[i];
                        if (item?.info?.itemid == SCRAP_ITEM_ID)
                        {
                            findScrap = item;
                            break;
                        }
                    }
                    if (findScrap != null)
                    {
                        findScrap.amount += scrapAdd;
                        findScrap.MarkDirty();
                    }
                    else
                    {
                        var newScrap = ItemManager.CreateByItemID(SCRAP_ITEM_ID, scrapAdd);
                        if (!newScrap.MoveToContainer(lootInventory) && !newScrap.Drop(attacker.GetDropPosition(), attacker.GetDropVelocity(), attacker.ServerRotation))
                        {
                            PrintWarning("barrel no move or drop scrap!!");
                            RemoveFromWorld(newScrap);
                        }
                    }
                    PlayScrapEffect(attacker, Random.Range(2, 4));
                    var addXP = DoMultipliers(attacker.userID, DoCatchup(attacker.userID, Random.Range(16, 56) * scrapAdd));

                    AddXP(attacker.UserIDString, addXP, XPReason.Loot);

                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        SendLuckNotifications(attacker, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> You found an extra <color=").Append(LUCK_COLOR).Append(">").Append(scrapAdd.ToString("N0")).Append("</color> Scrap!").ToString());
                    }
                    finally { Pool.Free(ref sb); }
                }
            }

            var scavenger = luckInfo.GetStatLevelByName("Scavenger");

            //removed scavenger return, because we now loop through items in the dead lootcontainer for charm bonuses

            var scavVal = scavenger * (userClassType == ClassType.Packrat ? 6 : 4);

            //hello?? where packrat scale???
            //obviously it was resolved, but this coment was made because it was previously missing lol



            var scavNeed = scavenger < 1 ? -1 : GetChanceScaledInteger(attacker.userID, scavVal, scavVal, 99, scalar); //messy!

            var itemList = lootCont?.inventory?.itemList ?? null;
            if (itemList != null && itemList.Count > 0)
            {
                var sb = Pool.Get<StringBuilder>(); //even if the player doesn't get lucky & the sb isn't used, it seems more performant to grab it once instead of grabbing it in the loop possibly multiple times

                try
                {
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var item = itemList[i];
                        var itemName = item?.info?.shortname ?? string.Empty;

                        if (item.info.itemid != SCRAP_ITEM_ID && (item?.info?.category ?? ItemCategory.All) != ItemCategory.Component)
                            continue;

                        if (item.info.itemid != SCRAP_ITEM_ID) //the modifier bonus itself handles the scrap bonus!! we don't want to bonus it twice, so ensure not scrap item!!!
                        {
                            var survGather = GetCharmGathers(attacker, CharmSkill.Survival);

                            var preCharmAmount = item.amount;

                            if (survGather < 0.0f) item.amount -= (int)Math.Round(item.amount * Math.Abs(survGather) / 100, MidpointRounding.AwayFromZero);
                            if (survGather > 0.0f) item.amount += (int)Math.Round(item.amount * survGather / 100, MidpointRounding.AwayFromZero);

                            if (attacker != null && attacker.IsAdmin) PrintWarning("pre-charm amount for: " + item.info.displayName.english + ": " + preCharmAmount + ", now: " + item.amount + " bonus: " + (item.amount - preCharmAmount));

                        } 

                        if (scavRandom.Next(0, 101) > scavNeed)
                            continue;
                        var rndMax = 5;

                        var rndPerc = (scavenger == 1) ? 0 : scavenger * (item.info.itemid == SCRAP_ITEM_ID ? 33 : 8); //5% (8%?) each lvl, ~~100%~~ 33% each lvl for scrap
                        var rndPercF = (rndPerc > 0) ? (double)(rndMax * rndPerc / 100) : 0;

                        rndMax = (int)Math.Round(rndMax + rndPercF, MidpointRounding.AwayFromZero);

                        var newAmt = Mathf.Clamp(Random.Range(1, rndMax), 1, item.MaxStackable());

                        if (item.amount + newAmt <= item.MaxStackable())
                        {
                            item.amount += newAmt;
                            item.MarkDirty();
                        }
                        else
                        {
                            var newItem = ItemManager.Create(item.info, item.amount, item.skin);
                            if (!newItem.Drop(lootCont.dropPosition, Vector3.up * 2))
                            {
                                PrintWarning("no drop item!!");
                                RemoveFromWorld(newItem);
                            }
                        }


                        SendLuckNotifications(attacker, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> <color=#4A7FAA>Scavenger:</color>\nYou found <color=").Append(LUCK_COLOR).Append(">").Append(newAmt).Append("</color> extra <color=").Append(LUCK_COLOR).Append(">").Append(item?.info?.displayName?.english ?? "Unknown").Append("</color> while looting.").ToString());


                        var xpMin = 56;
                        var xpMax = 129;

                        var rndXP = DoCatchup(attacker.userID, DoMultipliers(attacker.userID, Random.Range(xpMin, xpMax) * (itemName == "scrap" ? (newAmt * 0.225f) : newAmt)));

                        AddXP(attacker.UserIDString, rndXP, XPReason.Loot);

                        PlayCodeLockUpdate(attacker, 2);

                    }
                }
                finally { Pool.Free(ref sb); }
            }
            else
            {
                PrintWarning("bad itemlist! loot container looted with no items: " + lootCont.ShortPrefabName + " @ " + lootCont.transform.position);
            }

            if (lootCont.prefabID == 3438187947 && scavenger > 0) //oil barrel prefab id
            {
                var oilNeed = (userClassType == ClassType.Packrat ? 7d : 5d) * scavenger;

                oilNeed = GetChanceScaledDouble(attacker.userID, oilNeed, oilNeed, 99, scalar);

                if (Random.Range(0, 101) <= oilNeed)
                {
                    var oilMin = 3 * scavenger;
                    var oilMax = 7 * scavenger;
                    var oilAmount = Random.Range(oilMin, oilMax);

                    Item findOil = null;

                    for (int i = 0; i < lootCont.inventory.itemList.Count; i++)
                    {
                        var item = lootCont.inventory.itemList[i];

                        if (item.info.itemid == -321733511) //crude oil item id
                        {
                            findOil = item;
                            break;
                        }

                    }

                    if (findOil != null)
                    {
                        findOil.amount += oilAmount;
                        findOil.MarkDirty();
                    }
                    else
                    {
                        var item = ItemManager.CreateByItemID(-321733511, oilAmount);

                        if (item.MoveToContainer(lootCont.inventory) || item.Drop(lootCont.GetDropPosition(), lootCont.GetDropVelocity()))
                        {

                            var sb = Pool.Get<StringBuilder>();
                            try
                            {
                                SendLuckNotifications(attacker, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> You found <color=").Append(LUCK_COLOR).Append(">").Append(oilAmount.ToString("N0")).Append("</color> extra oil.").ToString());
                            }
                            finally { Pool.Free(ref sb); }

                            var rndXP = DoCatchup(attacker.userID, DoMultipliers(attacker.userID, 24 * oilAmount));

                            AddXP(attacker.UserIDString, rndXP, XPReason.Loot);

                            PlayCodeLockUpdate(attacker);
                        }
                    }

                }

            }

        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || entity == null || !init) return;

            try
            {

                var attacker = info?.Initiator as BasePlayer;

                var weaponSkin = (info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem())?.skin ?? 0;

                var isBlunderbuss = weaponSkin == GRISWOLDS_BLUNDERBUSS_SKIN_ID; //(info?.Weapon?.skinID ?? 0) == GRISWOLDS_BLUNDERBUSS_SKIN_ID || (info?.WeaponPrefab?.skinID ?? 0) == GRISWOLDS_BLUNDERBUSS_SKIN_ID;

                if (isBlunderbuss)
                {
                    var deathPos = entity?.transform?.position ?? Vector3.zero;

                    var adjustedDeathPos = new Vector3(deathPos.x, deathPos.y += 0.1f, deathPos.z);
                    adjustedDeathPos = GetGroundPosition(adjustedDeathPos, 50);

                    var graveStone = GameManager.server.CreateEntity("assets/prefabs/misc/halloween/deployablegravestone/gravestone.stone.deployed.prefab", adjustedDeathPos);
                    graveStone.Spawn();

                    graveStone?.Invoke(() =>
                    {
                        if (!(graveStone?.IsDestroyed ?? true))
                            graveStone.Kill();
                    }, 15f);
                }

        

                var npc = entity as NPCPlayer;

                if (npc == null)
                    return;

                var mercPlayer = npc?.GetComponent<NPCHitTarget>()?.MercInfo?.Player;

                if (mercPlayer == null)
                    return;

                if (attacker != null && attacker == mercPlayer)
                {
                    PrintWarning("NPC was killed by intended player; not other player");
                    SendReply(mercPlayer, "You've completed your hit! Loot the corpse and take the dog tags to Airwolf at the Bandit Camp for your reward (select \"I'm just browsing\")!");
                }
                else
                {
                    SendReply(attacker, "You've killed somebody else's hit target! Loot the corpse and take their dog tags back to Airwolf at the Bandit Camp for a reward (select \"I'm just browsing\")!");
                    SendReply(mercPlayer, "Someone else has killed your hit! You can still take their dog tags from them before they return them to Airwolf at the Bandit Camp for a reward (select \"I'm just browsing\")!");
                }

            }
            catch (Exception ex) { PrintError(ex.ToString()); }
        }

        private bool IsValidPlayer(BasePlayer player) { return player != null && player.gameObject != null && !player.IsDestroyed && !player.IsNpc && player.prefabID == 4108440852 && player is not HumanNPC; }

        private bool IsMeleeDamageType(DamageType dmgType) { return dmgType == DamageType.Blunt || dmgType == DamageType.Slash || dmgType == DamageType.Stab; }


        public bool TryParseFloat(object obj, ref float value)
        {
            if (obj == null) return false;
            return TryParseFloat(obj?.ToString() ?? string.Empty, ref value);
        }

        public bool TryParseFloat(string text, ref float value)
        {
            if (float.TryParse(text, out float tmp))
            {
                value = tmp;
                return true;
            }
            else return false;
        }

        public bool TryParseBool(object obj, ref bool value)
        {
            if (obj == null) return false;
            return TryParseBool(obj?.ToString() ?? string.Empty, ref value);
        }

        public bool TryParseBool(string text, ref bool value)
        {
            if (string.IsNullOrEmpty(text)) return false;
            if (bool.TryParse(text, out bool tmp))
            {
                value = tmp;
                return true;
            }
            else return false;
        }

        private void DoLootCharm(LootContainer loot, float rngScalar = 1f)
        {
            if (loot == null || loot.IsDestroyed || loot?.inventory == null || loot?.transform == null) return;


            var maxVal = rngScalar != 1.0f ? (int)Math.Round(101 * Mathf.Clamp(rngScalar, 0f, float.MaxValue)) : 101;

            var charmRng = Random.Range(0, maxVal);

            var rngNeed = loot.ShortPrefabName.Contains("elite") ? 14 : 1;

            if (charmRng > rngNeed)
                return;


            var newCharm = CreateRandomCharm();

            if (!newCharm.MoveToContainer(loot.inventory))
            {
                PrintError("CHARM SPAWN BUT COULDN'T MOVE!!!");
                RemoveFromWorld(newCharm);
            }
        }

        private void OnLootSpawn(LootContainer container) //only handles charm spawns & fixes legendary items spawning normally.
        {
            if (container == null || container?.transform == null) return;

            if (container.LootSpawnSlots?.Length > 0)
            {
                var incompatibleItems = Pool.GetList<ItemDefinition>();
                var compatibleItemRange = Pool.GetList<ItemAmountRanged>();
                try
                {


                    for (int i = 0; i < container.LootSpawnSlots.Length; i++)
                    {
                        var spawnSlot = container.LootSpawnSlots[i];

                        if (spawnSlot.definition?.items?.Length < 1)
                            continue;

                        incompatibleItems.Clear();

                        for (int j = 0; j < spawnSlot.definition.items.Length; j++)
                        {
                            var spawnItem = spawnSlot.definition.items[j];

                            if (IsBlockedItem(spawnItem.itemid)) incompatibleItems.Add(spawnItem.itemDef);
                            else compatibleItemRange.Add(spawnItem);

                        }

                        if (incompatibleItems.Count >= spawnSlot.definition.items.Length)
                        {
                            PrintWarning(nameof(incompatibleItems) + " Count >= " + nameof(spawnSlot.definition.items) + " Length (" + incompatibleItems.Count + " >= " + spawnSlot.definition.items.Length + ")");
                            spawnSlot.probability = 0f;
                            PrintWarning("forced probability to 0f. i hope this doesn't break things lol");
                        }
                        else if (incompatibleItems.Count > 0)
                        {
                            PrintWarning(nameof(incompatibleItems) + ".Count > 0: " + incompatibleItems.Count);

                            var newCount = compatibleItemRange.Count;

                            PrintWarning(nameof(spawnSlot.definition.items) + " old count: " + spawnSlot.definition.items.Length + " new: " + newCount);

                            spawnSlot.definition.items = new ItemAmountRanged[newCount];

                            for (int j = 0; j < compatibleItemRange.Count; j++)
                            {
                                var desiredItem = compatibleItemRange[j];

                                spawnSlot.definition.items[i] = desiredItem;

                            }



                        }
                        


                    }
                }
                finally
                {
                    try { Pool.FreeList(ref incompatibleItems); }
                    finally { Pool.FreeList(ref compatibleItemRange); }
                }
            }
            else
            {
                //handle single item def
            }
      
         

            if (!IsApplicableContainerForCharm(container.prefabID))
                return;

            var lootPos = container?.transform?.position ?? Vector3.zero;

            var isHacked = container is HackableLockedCrate;

            var isUnderWater = container.ShortPrefabName.Contains("underwater");
            var isAdvancedUnderWater = container.ShortPrefabName.Contains("underwater_adv");

            var nearMon = isHacked || isUnderWater || isAdvancedUnderWater; // || WaterLevel.GetWaterDepth(container.transform.position) >= 4;
            if (!nearMon)
            {
                for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                {
                    var info = TerrainMeta.Path.Monuments[i];
                    if (info == null || info.transform == null) continue;
                    if (info.shouldDisplayOnMap && !info.displayPhrase.english.Contains("Mining", CompareOptions.OrdinalIgnoreCase) && !info.displayPhrase.english.Contains("Quarry", CompareOptions.OrdinalIgnoreCase) && !info.displayPhrase.english.Contains("Lighthouse", CompareOptions.OrdinalIgnoreCase) && Vector3.Distance(new Vector3(info.transform.position.x, lootPos.y, info.transform.position.z), lootPos) <= 300)
                    {
                        nearMon = true;
                        break;
                    }
                }
            }



            if (nearMon)
            {
                var scalar = SaveSpan.TotalDays < 1 ? 3.25f : SaveSpan.TotalDays < 2 ? 1.7f : SaveSpan.TotalDays < 2.5 ? 1.35f : SaveSpan.TotalDays < 3 ? 1.1f : 1f;
                scalar += Random.Range(0.125f, 0.33f);
                //  var scalar = UnityEngine.Random.Range(1.45f, 1.7f);
                if (isHacked) scalar -= Random.Range(0.725f, 0.995f);


                if (isUnderWater)
                {
                    if (isAdvancedUnderWater) scalar -= Random.Range(0.517f, 0.925f);
                    else scalar -= Random.Range(0.2f, 0.5f);
                }

                if (IsRadStormActive())
                    scalar -= 0.33f;

                DoLootCharm(container, scalar);
            }

        }

        private readonly Dictionary<ulong, string> _scientistSteamIdToPrefabName = new();

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null) return;

            if (entity is NPCPlayer npcPlayer)
                _scientistSteamIdToPrefabName[npcPlayer.userID] = npcPlayer.PrefabName;

            var ore = entity as OreResourceEntity;

            ore?.Invoke(() =>
            {
                if (ore == null)
                {
                    PrintError("ore/hotspot null");
                    return;
                }

                var centerPoint = ore?.CenterPoint() ?? Vector3.zero;
                var spawnPos = SpreadVector2(centerPoint, 0.25f);

                spawnPos.y = centerPoint.y + 1.25f;



                var fBall = (FireBall)GameManager.server.CreateEntity("assets/bundled/prefabs/fireball_small.prefab", spawnPos);

                fBall.OwnerID = 1221;

                fBall.lifeTimeMax = 99999f;
                fBall.lifeTimeMin = 99998f;

                fBall.canMerge = false;
                fBall.generation = 0f;
             

                fBall.Spawn();

                //fBall.EnableGlobalBroadcast(true);

                _oreFireballs.Add(fBall.net.ID);

                fBall.SetParent(ore, true, true);

                fBall.SetGeneration(0);
                fBall.radius = 0.01f;
                fBall.damagePerSecond = 0f;

                fBall.lifeTimeMax = 999999f;
                fBall.lifeTimeMin = 999998f;

                fBall.AddLife(999999f);
                fBall.waterToExtinguish = int.MaxValue;
                fBall.canMerge = false;

                fBall.Invoke(() =>
                {
                    var rigid = fBall?.GetComponent<Rigidbody>();

                    if (rigid == null)
                        return;

                    rigid.isKinematic = true;
                    rigid.useGravity = false;

                }, 0.01f);


            }, 0.5f);

            var dropped = entity as DroppedItemContainer;
            if (dropped != null && IsBackpackSkin(dropped.skinID)) dropped.Invoke(() =>
            {
                PrintWarning(nameof(OnEntitySpawned) + " for backpack: " + entity?.net?.ID);
                if (dropped != null && !dropped.IsDestroyed) dropped.CancelInvoke(new Action(dropped.RemoveMe));
            }, 0.5f);

            var loot = entity as LootContainer;
            if (loot != null) allContainers.Add(loot);

            /*/
            if (loot != null && loot?.transform != null && loot.ShortPrefabName.Contains("crate") && !loot.ShortPrefabName.Contains("heli") && !loot.ShortPrefabName.Contains("bradley") && !loot.ShortPrefabName.Contains("_mine") && SaveSpan.TotalHours > 8)
            {
                var lootPos = loot?.transform?.position ?? Vector3.zero;
                var isHacked = loot.ShortPrefabName.Contains("hackable");
                var nearMon = isHacked;
                if (!nearMon)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var info = TerrainMeta.Path.Monuments[i];
                        if (info == null || info.transform == null) continue;
                        if (info.shouldDisplayOnMap && !info.displayPhrase.english.Contains("Mining") && !info.displayPhrase.english.Contains("Quarry") && !info.displayPhrase.english.Contains("Lighthouse") && Vector3.Distance(new Vector3(info.transform.position.x, lootPos.y, info.transform.position.z), lootPos) <= 300)
                        {
                            nearMon = true;
                            break;
                        }
                    }
                }

                if (nearMon)
                {
                    var scalar = SaveSpan.TotalDays < 1 ? 1.57f : SaveSpan.TotalDays < 2 ? 1.28f : SaveSpan.TotalDays < 3 ? 1.17f : SaveSpan.TotalDays < 4.2 ? 1.1f : 1f;
                    if (isHacked) scalar -= UnityEngine.Random.Range(0.45f, 0.6f);
                    NextTick(() => DoLootCharm(loot, scalar));
                }

            
            }/*/

            var trap = entity as WildlifeTrap;
            if (trap == null) return;

            var ownerID = trap?.OwnerID ?? 0;

            var trapExt = trap?.GetComponent<WildTrapExt>() ?? trap.gameObject.AddComponent<WildTrapExt>();

            if (trapExt == null)
            {
                PrintWarning("trapExt is null after add?!");
                return;
            }

            if (ownerID != 0) 
                AdjustTickRate(trap, ownerID);
        }

        class ItemWithPos
        {
            public Item item;
            public int pos;

            public ItemWithPos() { }
            public ItemWithPos(Item Item, int position = -1)
            {
                item = Item;
                pos = position;
            }
        }

        readonly Dictionary<BasePlayer, ItemWithPos> lastDroppedActiveItem = new();
        private void OnPlayerDropActiveItem(BasePlayer player, Item item)
        {
            if (player == null || item == null) return;

            var rejack = GetLuckInfo(player)?.GetStatLevelByName("Rejack") ?? 0;
            if (rejack > 0) lastDroppedActiveItem[player] = new ItemWithPos(item, item?.position ?? -1);
        }

        private object CanDropActiveItem(BasePlayer player)
        {
            if (player == null) return null;

            var item = GetActiveItem(player) ?? GetActiveItem(player, true);

            if (item?.info?.itemid == 1079279582)
            {
                var rejack = GetLuckInfo(player)?.GetStatLevelByName("Rejack") ?? 0;
                if (rejack > 0) return false;
            }

            return null;
        }

        private readonly Dictionary<ulong, float> lastDodgeFXTime = new();
        private readonly Dictionary<HitInfo, float> _preDodgeDamageAmount = new();

        //could be optiomized by using baseplayer version where applicable
        private object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitInfo)
        {
            if (entity == null) return null;
            try
            {

                var attackerPlayer = hitInfo?.InitiatorPlayer ?? ((hitInfo?.Initiator) as BasePlayer);

                var atkBCE = hitInfo?.Initiator as BaseCombatEntity;
                var victim = entity as BasePlayer;
                var prefabName = entity?.ShortPrefabName ?? string.Empty;

                var dmgType = hitInfo?.damageTypes?.GetMajorityDamageType() ?? DamageType.Generic;
                var isBleeding = dmgType == DamageType.Bleeding;
                var vicInfo = GetLuckInfo(victim);
                var atkInfo = GetLuckInfo(attackerPlayer);

                BaseProjectile weaponProj = null;
                try
                {
                    weaponProj = (hitInfo?.Weapon?.gameObject) != null ? (hitInfo?.Weapon?.GetComponent<BaseProjectile>() ?? null) : (hitInfo?.WeaponPrefab?.gameObject != null) ? (hitInfo?.WeaponPrefab?.GetComponent<BaseProjectile>() ?? null) : null;
                }
                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + " exception occurred on weapon"); }

                var isAuto = weaponProj?.automatic ?? false;

                if (entity?.net != null && (immuneEntities.Contains(entity.net.ID) || (_npcImmuneEntities.Contains(entity.net.ID) && !IsValidPlayer(attackerPlayer))))
                {
                    PrintWarning("has immunity: " + entity);
                    CancelDamage(hitInfo);
                    return true;
                }

                //forge stuff

                if (attackerPlayer != null && !IsValidPlayer(victim))
                {
                    var forgeTrigger = GetForgesTrigger(attackerPlayer);
                    if ((forgeTrigger?.Temperature ?? 0f) >= 3)
                    {
                        var divisions = GetNumberOfDivisions(Mathf.FloorToInt(forgeTrigger.Temperature), 3);

                        var scalar = 0.33f * divisions;

                        var finalScalar = Mathf.Clamp(1.0f + scalar, 1.0f, 5.0f);

                        hitInfo.damageTypes.ScaleAll(finalScalar);
                    }
                }

               
                

                //must be dmg type of heat, or inflicted from another entity (self inflicted dmg is still dealt normally), and not another real player (non-NPC).
                if ((hitInfo?.InitiatorPlayer == victim || !IsValidPlayer(hitInfo?.InitiatorPlayer)) && (dmgType == DamageType.Heat || (hitInfo?.Initiator != null && hitInfo?.Initiator != entity)) && victim?.metabolism != null && (GetActiveItem(victim)?.skin ?? 0) == BURNING_FORGES_SKIN_ID)
                {
                    var takenDmgScalar = dmgType == DamageType.Heat ? 0f : 1.0f - Mathf.FloorToInt(victim.metabolism.temperature.value) * 0.0075f; //0.75% less damage taken for every temperature point

                    hitInfo.damageTypes.ScaleAll(takenDmgScalar);
                }

                //end forge

                var carMod = entity as BaseVehicleModule;

                if (carMod?.Vehicle != null)
                {
                    foreach (var mount in carMod.Vehicle.allMountPoints)
                    {
                        var mountedPly = mount?.mountable?.GetMounted();


                        if (mountedPly != null)
                        {
                            if (GetUserClassType(mountedPly.UserIDString) == ClassType.Nomad)
                                hitInfo.damageTypes.ScaleAll(0.33f);


                            break;
                        }
                    }

                    return null;
                }


                try
                {
                    if (entity?.gameObject != null && !entity.IsDestroyed)
                    {

                        var takenDamageScalar = entity?.GetComponent<NPCHitTarget>()?.TakenDamageScalar ?? entity?.GetComponent<NPCHitTargetAlly>()?.TakenDamageScalar ?? 1.0f;

                        if (takenDamageScalar != 1.0f)
                        {
                           // PrintWarning("npcHitVic is not null, TakenDamageScalar is: " + takenDamageScalar);

                            hitInfo.damageTypes.ScaleAll(takenDamageScalar);

                            if (takenDamageScalar < 1.0f && (attackerPlayer?.IsConnected ?? false))
                            {
                                SendCooldownMessage(attackerPlayer, "This person looks to be equipped with Kevlar-lining and my attacks aren't nearly as effective, I better watch out!\nThis guy must be a hit target or one of the \"guards\"...", 25f, "HIT_MERC_ALLY_REMINDER");
                                ShowToast(attackerPlayer, "Watch out! This enemy is tougher.", 1);
                                //cooldown toast too?
                            }


                        }
                    }


                    if (hitInfo?.Initiator != null && hitInfo?.Initiator?.gameObject != null && !hitInfo.Initiator.IsDestroyed)
                    {
                        var damageScalar = hitInfo?.Initiator?.GetComponent<NPCHitTarget>()?.DamageScalar ?? hitInfo?.Initiator?.GetComponent<NPCHitTargetAlly>()?.DamageScalar ?? 1.0f;
                        if (damageScalar != 1.0f)
                        {
                           // PrintWarning("npcHitAtk is not null (it is initiator), DamageScalar is: " + damageScalar);

                            if (damageScalar != 1.0f) hitInfo.damageTypes.ScaleAll(damageScalar); //show msg & toast warning for taking dmg?
                        }
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString() + Environment.NewLine + " exception occurred on entity GetComponents"); }
               

                //NOMAD:

                var victimClass = victim == null ? ClassType.Undefined : GetUserClassType(victim.UserIDString);

                var attackerClass = attackerPlayer == null ? ClassType.Undefined : GetUserClassType(attackerPlayer.UserIDString);

                if (victimClass == ClassType.Nomad && attackerClass != ClassType.Nomad)
                {
                    var hookVal = Interface.Oxide.CallHook("CanScaleNomadVictimDamage", victim, hitInfo);

                    if (hookVal == null || (bool)hookVal)
                        hitInfo.damageTypes.ScaleAll(0.92f);


                }


                if (attackerClass == ClassType.Nomad && victimClass != ClassType.Nomad) //don't scale damage to self, also if both are nomads we just cancel it out
                {
                    var hookVal = Interface.Oxide.CallHook("CanScaleNomadAttackerDamage", attackerPlayer, hitInfo);

                    if (hookVal == null || (bool)hookVal)
                        hitInfo.damageTypes.ScaleAll(1.05f);
                }


                //Mercenary


                if (attackerClass == ClassType.Mercenary)
                {

                    var scalar = entity is PatrolHelicopter ? 1.25f : entity is BradleyAPC ? 1.75f : entity is HumanNPC ? 1.12f : entity is CH47HelicopterAIController ? 2f : 1f;

                    var overdriveLvl = atkInfo?.GetStatLevelByName("Overdrive") ?? 0;

                    if (scalar != 1f)
                    {
                        scalar += Mathf.Clamp(overdriveLvl < 1 ? 0 : overdriveLvl * 0.125f, 0f, 100f);
                        hitInfo.damageTypes.ScaleAll(scalar);
                    }

                    var hitHeli = entity is PatrolHelicopter;
                    var hitApc = entity is BradleyAPC;

                    if (hitHeli || hitApc)
                    {
                        attackerPlayer.Invoke(() =>
                        {
                            if (attackerPlayer == null || attackerPlayer?.gameObject == null || attackerPlayer.IsDestroyed || attackerPlayer.IsDead() || !attackerPlayer.IsConnected) return;
                            var dmg = hitInfo?.damageTypes?.Total() ?? 0f;

                            if (dmg <= 0) return; //no dmg, no xp!

                            var basePoints = hitApc ? 224 : 48; //rockets are used for bradley/APC so more XP reward

                            var scaledPoints = Mathf.Clamp(basePoints * (dmg * 0.0115f), basePoints * 0.5f, int.MaxValue);

                            var finalXp = DoCatchup(attackerPlayer.userID, DoMultipliers(attackerPlayer.userID, scaledPoints));

                            AddXP(attackerPlayer.UserIDString, finalXp, XPReason.PvEHit);
                        }, 0.05f);
                    }
                }


                if (entity is WildlifeTrap && (dmgType == DamageType.Decay || dmgType == DamageType.Generic))
                {
                    var ownerID = entity?.OwnerID ?? 0;
                    var fishLvl = GetLuckInfo(ownerID)?.GetStatLevelByName("Fishing") ?? 0;
                    if (fishLvl > 0)
                    {
                        var dmgScalar = (fishLvl == 1) ? 0.75f : (fishLvl == 2) ? 0.5f : (fishLvl == 3) ? 0.25f : (fishLvl == 4) ? 0.1f : 0.0f;
                        hitInfo?.damageTypes?.ScaleAll(dmgScalar);
                    }

                    return null;
                }

                var thorns = vicInfo?.GetStatLevelByName("Thorns") ?? 0;

                if (victim != null && vicInfo != null)
                {

                    var isNPC = (hitInfo?.Initiator as BaseNpc != null) || ((hitInfo?.Initiator as BasePlayer)?.IsNpc ?? false);
                    var isHeli = hitInfo?.Initiator is PatrolHelicopter;
                    var isBlock = hitInfo?.Initiator is BuildingBlock || hitInfo?.Initiator is SimpleBuildingBlock;


                    if (thorns > 0 && !isBlock && (isNPC || isHeli))
                    {

                        var atkBcePos = (atkBCE != null && atkBCE.gameObject != null && !atkBCE.IsDestroyed) ? (atkBCE?.transform?.position ?? Vector3.zero) : Vector3.zero;

                        NextTick(() =>
                        {
                            try
                            {
                                if (victim == null || !(victim?.IsConnected ?? true) || (hitInfo?.IsProjectile() ?? false)) return;

                                if (!_preDodgeDamageAmount.TryGetValue(hitInfo, out float dmg)) dmg = hitInfo?.damageTypes?.Total() ?? 0f;
                                else _preDodgeDamageAmount.Remove(hitInfo);


                                var perc = ((attackerPlayer != null && attackerPlayer.IsConnected) ? 25 : 100) * thorns;
                                //var perc = ((attackerPlayer != null && attackerPlayer.IsConnected) ? 6.25f : 25) * thorns;

                                if (perc <= 0) return;

                                var dmgPerc = dmg * perc / 100;
                                if (!isNPC)
                                {
                                    var weaponMelee = (hitInfo?.Weapon?.GetItem()?.GetHeldEntity() ?? hitInfo?.WeaponPrefab?.GetItem()?.GetHeldEntity() ?? hitInfo?.WeaponPrefab ?? null) as BaseMelee;
                                    if (weaponMelee == null && IsMeleeDamageType(dmgType)) return;
                                }

                                var dmgToDeal = dmgPerc;
                                if (dmgToDeal < 1f)
                                    return;

                                try 
                                {
                                    var canThornsObj = Interface.CallHook("CanThorns", atkBCE, entity, hitInfo);
                                    var canThorns = true;
                                    if (TryParseBool(canThornsObj, ref canThorns) && !canThorns)
                                    {
                                        PrintWarning("hook returned !canThorns, return!");
                                        return;
                                    }
                                }
                                catch(Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Error on CanThorns hook^"); }



                                HitInfo newHitInfo = null;

                                try 
                                {
                                    newHitInfo = new HitInfo
                                    {
                                        Initiator = victim,
                                        HitEntity = atkBCE ?? hitInfo?.Initiator,
                                        Weapon = hitInfo?.Weapon ?? null,
                                        WeaponPrefab = hitInfo?.WeaponPrefab ?? null,
                                        PointStart = (victim != null && victim.gameObject != null) ? (victim?.transform?.position ?? Vector3.zero) : Vector3.zero,
                                        HitPositionLocal = Vector3.zero
                                    };
                                    newHitInfo.PointEnd = atkBcePos != Vector3.zero ? atkBcePos : newHitInfo.PointStart;
                                    newHitInfo.HitPositionWorld = newHitInfo.PointEnd;
                                    newHitInfo.damageTypes.Add(DamageType.Slash, dmgToDeal);
                                }
                                catch(Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Error on setting HitInfo for Thorns"); }

                                try
                                {
                                    if (atkBCE != null && !(atkBCE?.IsDead() ?? true) && !(atkBCE?.IsDestroyed ?? true))
                                    {
                                        atkBCE.Hurt(newHitInfo);

                                        Interface.Oxide.CallHook("OnPlayerAttack", victim, newHitInfo); //doesn't get called naturally i guess

                                        SendLuckNotifications(victim, "<color=orange>Thorns: " + dmgToDeal.ToString("N0") + " damage (" + perc + "%)" + "!</color>");
                                    }
                                    else PrintWarning("atkBCE is null or dead!!");
                                }
                                catch(Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Error on trying to cause Thorns damage"); }

                            }
                            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete Thorns callback^"); }
                        });
                    }
                    if (isBleeding)
                    {
                        var thickSkin = vicInfo?.GetStatLevelByName("ThickSkin") ?? 0;

                        if (thickSkin > 0)
                        {
                            var canThickSkin = true;
                            if (!TryParseBool(Interface.CallHook("CanThickSkin", atkBCE, entity, hitInfo), ref canThickSkin)) canThickSkin = true;

                            if (canThickSkin)
                            {
                                var bleedScale = 1f - (thickSkin * 0.15f); //0.0225f);

                                hitInfo?.damageTypes?.Scale(DamageType.Bleeding, bleedScale);
                            }
                        }

                    }

                    var rejack = vicInfo?.GetStatLevelByName("Rejack") ?? 0;
                    if (rejack > 0)
                    {
                        if (!victim.IsWounded() && !(dmgType == DamageType.Hunger || dmgType == DamageType.Bleeding || dmgType == DamageType.Thirst || dmgType == DamageType.RadiationExposure || dmgType == DamageType.Radiation || dmgType == DamageType.Cold || dmgType == DamageType.ColdExposure || dmgType == DamageType.Poison || dmgType == DamageType.Suicide))
                        {
                            NextTick(() =>
                            {
                                if (victim == null || entity == null || hitInfo == null || victim?.gameObject == null || victim.IsDestroyed || victim.IsDead()) return;
                                try
                                {
                                    var dmgAfter = entity?.health ?? 0;


                                    if (victim.IsWounded())
                                    {
                                        if (rejack > 0)
                                        {
                                            var syrng = 1079279582;
                                            var hasSyringe = false;
                                            var beltItems = victim?.inventory?.containerBelt?.itemList ?? null;
                                            if (beltItems != null && beltItems.Count > 0)
                                            {
                                                for (int i = 0; i < beltItems.Count; i++)
                                                {
                                                    if (beltItems[i]?.info?.itemid == syrng)
                                                    {
                                                        hasSyringe = true;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (!hasSyringe) return;

                                            var rejackPercScalar = victimClass == ClassType.Mercenary ? 1.02f : victimClass == ClassType.Nomad ? 0.75f : 0.425f;

                                            var needRng = 7 * rejack;
                                            needRng = GetChanceScaledInteger(victim.userID, needRng, needRng, 100, rejackPercScalar);


                                            var length = Random.Range(3.65f, 7.2f);
                                            var now = Time.realtimeSinceStartup;

                                            if (!lastRejack.TryGetValue(victim.userID, out float time)) time = lastRejack[victim.userID] = Time.realtimeSinceStartup - 120;
                                            else time = now - time;

                                            var moveToHealthAfter = Random.Range(20, 30);
                                            var immediateHealthAfter = 0;
                                            Puts(time + " <-- time, maj dmg type: " + dmgType);

                                            if (rejack == 2)
                                            {
                                                length = Random.Range(2.6f, 6.7f);
                                                moveToHealthAfter = Random.Range(18, 24);
                                            }
                                            if (rejack == 3)
                                            {
                                                length = Random.Range(1.9f, 4.65f);
                                                moveToHealthAfter = Random.Range(24, 29);
                                                immediateHealthAfter = Random.Range(14, 17);
                                            }
                                            if (rejack == 4)
                                            {
                                                length = Random.Range(1.5f, 4.125f);
                                                moveToHealthAfter = Random.Range(27, 36);
                                                immediateHealthAfter = Random.Range(16, 19);
                                            }
                                            if (time < 180f)
                                            {
                                                Puts(time + " <-- time rejack (cooldown?)");
                                                return;
                                            }
                                            var canRejack = false;
                                            if (!TryParseBool(Interface.CallHook("CanRejack", atkBCE, victim, hitInfo), ref canRejack)) canRejack = true;

                                            var rng = Random.Range(0, 101);
                                            if (_forceRejack.Contains(victim.userID) || (canRejack && rng <= needRng))
                                            {
                                                lastRejack[victim.userID] = Time.realtimeSinceStartup;
                                                // Puts("canRejack, time >= 120, and rng <= needRng: " + time + ", " + canRejack + ", " + rng + " <= " + needRng);
                                                if (attackerPlayer != null && attackerPlayer.IsConnected)
                                                {
                                                    SendLuckNotifications(attackerPlayer, "<color=orange>REJACK!</color> <color=red>KEEP SHOOTING YOUR TARGET!</color>", false, 3.75f);
                                                    SendLuckNotifications(attackerPlayer, "<color=#7300e6><size=21>Rejack!</size></color> - <size=19>This player will <color=orange>automatically use a syringe and get up</color>, </size><color=red><size=28>KEEP SHOOTING!</size>", true, 0f);
                                                }

                                                InvokeHandler.Invoke(victim, () =>
                                                {
                                                    if (victim == null || victim?.gameObject == null || victim.IsDestroyed || victim.IsDead() || !victim.IsConnected || !victim.IsWounded()) return;

                                                    victim.inventory.Take(null, syrng, 1);

                                                    var vicPos = victim.transform.position;

                                                    var posAdjustedH = victim.transform.position;
                                                    posAdjustedH.y += 1.7f;
                                                    var posAdjustedX = victim.transform.position;
                                                    posAdjustedX.x += 1.2f;
                                                    posAdjustedX.y += 1.6f;
                                                    Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/explosion.prefab", victim.transform.position, victim.transform.position);

                                                    var eyePos = victim?.eyes?.position ?? posAdjustedH;

                                                    for (int i = 0; i < Random.Range(25, 35); i++)
                                                    {
                                                        Effect.server.Run("assets/bundled/prefabs/fx/player/frosty_breath.prefab", SpreadVector(eyePos, Random.Range(0.22f, 0.4f)), Vector3.zero);
                                                        Effect.server.Run("assets/prefabs/deployable/reactive target/effects/tire_smokepuff.prefab", SpreadVector(eyePos, Random.Range(0.15f, 0.55f)), Vector3.zero);
                                                    }

                                                    for (int i = 0; i < 4; i++)
                                                    {
                                                        Effect.server.Run("assets/prefabs/deployable/chinooklockedcrate/effects/landing.prefab", vicPos);
                                                    }



                                                    //Effect.server.Run("assets/bundled/prefabs/fx/victim/gutshot_scream.prefab", victim.transform.position, victim.transform.position);
                                                    InvokeHandler.Invoke(victim, () =>
                                                    {
                                                        Effect.server.Run("assets/prefabs/npc/patrol helicopter/effects/rocket_fire.prefab", victim.transform.position, victim.transform.position);
                                                    }, 0.175f);
                                                    InvokeHandler.Invoke(victim, () =>
                                                    {
                                                        Effect.server.Run("assets/prefabs/npc/patrol helicopter/effects/rocket_fire.prefab", posAdjustedH, posAdjustedH);
                                                    }, 0.685f);
                                                    InvokeHandler.Invoke(victim, () =>
                                                    {
                                                        Effect.server.Run("assets/prefabs/npc/patrol helicopter/effects/rocket_fire.prefab", posAdjustedH, posAdjustedH);
                                                    }, 1.25f);

                                                    SendLocalEffect(victim, "assets/prefabs/tools/medical syringe/effects/pop_cap.prefab");
                                                    SendLocalEffect(victim, "assets/prefabs/weapons/bandage/effects/deploy.prefab");

                                                    victim.Invoke(() =>
                                                    {
                                                        SendLocalEffect(victim, "assets/prefabs/weapons/bandage/effects/wraparm.prefab");
                                                    }, 0.125f);



                                                    victim.Invoke(() =>
                                                    {
                                                        SendLocalEffect(victim, "assets/prefabs/tools/medical syringe/effects/pop_button_cap.prefab");
                                                    }, 0.35f);

                                                    victim.Invoke(() =>
                                                    {
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            SendLocalEffect(victim, "assets/prefabs/tools/medical syringe/effects/inject_self.prefab");
                                                        }
                                                    }, 0.5f);

                                                    if (lastDroppedActiveItem.TryGetValue(victim, out ItemWithPos lastActive) && lastActive?.item != null && lastActive?.item?.parent == null) //ensure the item wasn't picked up by someone else before trying to move it
                                                    {
                                                        var lastItem = lastActive.item;
                                                        if (!lastItem.MoveToContainer(victim.inventory.containerBelt, lastActive.pos) && !lastItem.MoveToContainer(victim.inventory.containerBelt))
                                                        {
                                                            PrintWarning("Couldn't move last active back to player inv!!");
                                                        }
                                                    }

                                                    victim.health += immediateHealthAfter;
                                                    victim.StopWounded();
                                                    victim.metabolism.bleeding.value = 0;
                                                    victim.metabolism.pending_health.value = moveToHealthAfter;
                                                    victim.metabolism.SendChangesToClient();
                                                    victim.SendNetworkUpdateImmediate();

                                                    NoteItemByID(victim, syrng, -1);

                                                    SendLuckNotifications(victim, "<color=#7300e6><size=20>Rejack!</size></color>", true, 2.85f);

                                                }, length);
                                            }
                                            else PrintWarning("couldn't rejack, rng: " + rng + ", needed rng <=: " + needRng + ", time: " + time + ", can rejack: " + canRejack);

                                        }
                                    }
                                }
                                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete Rejack callback^"); }
                            });
                        }
                    }
                }

                if (prefabName.Contains("barrel") || prefabName.Contains("trash"))
                {



                    var atkScav = atkInfo?.GetStatLevelByName("Scavenger") ?? 0;
                    if (atkScav > 0)
                    {
                        var scaleAmount = 1 + (0.33f * atkScav);
                        hitInfo?.damageTypes?.ScaleAll(scaleAmount);
                    }
                }

                //     var initgetType = hitInfo?.Initiator?.GetType()?.ToString()?.ToLower() ?? string.Empty;


                var bpBCE = atkBCE as BasePlayer;
                if (victim != null && (hitInfo?.damageTypes?.Total() ?? 0f) > 0.0f && (bpBCE == null || bpBCE.IsNpc) && (dmgType == DamageType.Bullet || dmgType == DamageType.Arrow || dmgType == DamageType.Blunt || dmgType == DamageType.Bite || dmgType == DamageType.Explosion || dmgType == DamageType.Stab || dmgType == DamageType.Slash || dmgType == DamageType.Heat))
                {
                    var dodge = vicInfo?.GetStatLevelByName("Dodge") ?? 0;

                    if (dodge > 0)
                    {


                        var dodgePerc = 2.5 + (2.5 * dodge);

                        var dodgeScaler = Mathf.Clamp(1f - (0.15f * dodge), 0.05f, 1f);

                        //var dodgePerc = 0.625 + (0.625 * dodge);

                        //  var dodgescaler = Mathf.Clamp(1f - (0.0375f * dodge), 0.001f, 1f);

                        var dodgePercScalar = victimClass == ClassType.Mercenary ? 1.25f : victimClass == ClassType.Nomad ? 1.05f : 0.5f;

                        var scaleDodgePerc = GetChanceScaledDouble(victim.userID, dodgePerc, dodgePerc, 40f, dodgePercScalar);
                        if (Random.Range(0, 101) <= dodgePerc)
                        {
                            var canDodge = true;
                            if (!TryParseBool(Interface.CallHook("CanLuckDodge", victim, atkBCE, dodgeScaler), ref canDodge)) canDodge = true;
                            if (canDodge)
                            {
                                hitInfo?.damageTypes?.ScaleAll(dodgeScaler);

                                NextTick(() =>
                                {
                                    if (victim == null || !(victim?.IsConnected ?? false) || (victim?.IsDead() ?? true)) return;

                                    var dmgTotal = hitInfo?.damageTypes?.Total() ?? 0f;

                                    if (dmgTotal <= 0.0f)
                                        return;

                                    if (thorns > 0) _preDodgeDamageAmount[hitInfo] = dmgTotal / dodgeScaler;

                                    var eyePos = victim?.eyes?.position ?? Vector3.zero;

                                    var sb = Pool.Get<StringBuilder>();
                                    try
                                    {
                                        Effect.server.Run("assets/prefabs/weapons/arms/effects/drop_item.prefab", eyePos);
                                        if (!(victim.IsAiming || victim.modelState.aiming) && (!lastDodgeFXTime.TryGetValue(victim.userID, out float lastDodge) || (Time.realtimeSinceStartup - lastDodge) >= 3))
                                        {
                                            var rngEffect = Random.Range(1, 4);
                                            var swordFX = sb.Clear().Append("assets/prefabs/weapons/sword/effects/attack-").Append(rngEffect).Append(".prefab").ToString();
                                            SendLocalEffect(victim, swordFX);

                                            SendLocalEffect(victim, "assets/prefabs/weapons/arms/effects/shove.prefab");

                                            for (int i = 0; i < Random.Range(2, 4); i++)
                                                Effect.server.Run("assets/bundled/prefabs/fx/player/swing_weapon.prefab", eyePos, Vector3.zero);

                                            lastDodgeFXTime[victim.userID] = Time.realtimeSinceStartup;
                                        }

                                        SendLuckNotifications(victim, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> You dodged an attack. You took: <color=").Append(LUCK_COLOR).Append(">").Append(dodgeScaler.ToString("0.00").Replace(".00", string.Empty)).Append("</color>x damage.").ToString());
                                    }
                                    finally { Pool.Free(ref sb); }
                                });

                            }
                        }
                    }
                }

                if (attackerPlayer == null || !(attackerPlayer?.IsConnected ?? false)) return null;

                if (entity is BaseNpc)
                {
                    var hunter = atkInfo?.GetStatLevelByName("Hunter") ?? 0;
                    if (hunter > 0)
                    {
                        var scalePerc = 20 * hunter;

                        if (!isAuto) scalePerc += 5 * hunter;
                        var scaleAmount = 1f + (1f * scalePerc / 100);

                        hitInfo?.damageTypes?.ScaleAll(scaleAmount);
                    }
                }

                var critc = atkInfo?.GetStatLevelByName("CritChance") ?? 0;
                var critd = atkInfo?.GetStatLevelByName("CritDamage") ?? 0;
                var weapon = hitInfo?.Weapon?.GetItem()?.GetHeldEntity() as BaseProjectile ?? null;
                var weaponName = hitInfo?.Weapon?.GetItem()?.info?.shortname ?? "Unknown";
                var ammoType = weapon?.primaryMagazine?.ammoType?.displayName?.english ?? "Melee";


                
                var isValidBlock = true;

                var isNonLootDecay = entity is DecayEntity && entity is not LootContainer;

                var isNpc = (entity as BasePlayer)?.IsNpc ?? false;

                if (isNonLootDecay || entity is BuildingBlock || entity is Barricade || entity is BaseCorpse || (entity is BasePlayer && !isNpc) || entity is Door || entity is HelicopterDebris) isValidBlock = false;


                if (isValidBlock && critc > 0 && (dmgType != DamageType.Bleeding))
                {
                    if (victim != null && victim == attackerPlayer) return null;

                    var critRng = Random.Range(0, 101);

                    var scalar = 2f + (0.2f * (critd - 1));
                    var needRnd = 8 + (2 * critc);

                    var critPercScalar = attackerClass == ClassType.Mercenary ? 1.05f : attackerClass == ClassType.Nomad ? 0.8f : 0.5f;

                    needRnd = GetChanceScaledInteger(attackerPlayer.userID, needRnd, needRnd, 100, critPercScalar);


                    if (critRng <= needRnd)
                    {
                        var canCrit = true;
                        if (TryParseBool(Interface.CallHook("CanCrit", attackerPlayer, hitInfo), ref canCrit) && !canCrit) return null;

                        var effectPos = hitInfo?.HitEntity?.CenterPoint() ?? Vector3.zero;

                        hitInfo?.damageTypes?.ScaleAll(scalar);
                        NextTick(() =>
                        {
                            var damageD = hitInfo?.damageTypes?.Total() ?? 0f;
                            if (damageD >= 100f && victim != null && (victim?.IsAlive() ?? false)) victim.Die(hitInfo);
                            if (damageD > 0.0f)
                            {
                                if (damageD >= 10 && effectPos != Vector3.zero)
                                    for (int i = 0; i < Random.Range(3, 6); i++)
                                        Effect.server.Run("assets/prefabs/misc/orebonus/effects/bonus_finish.prefab", effectPos, Vector3.zero);


                                for (int i = 0; i < 15; i++)
                                    SendLocalEffect(attackerPlayer, "assets/content/structures/excavator/prefabs/effects/rockvibration.prefab");

                                var sb = Pool.Get<StringBuilder>();
                                try
                                {
                                    SendLuckNotifications(attackerPlayer, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> Critical Hit for <color=").Append(LUCK_COLOR).Append(">").Append(scalar).Append("</color>x (<color=").Append(LUCK_COLOR).Append(">").Append(damageD.ToString("N1").Replace(".0", string.Empty)).Append("</color> total) Damage!").ToString());
                                }
                                finally { Pool.Free(ref sb); }

                                PlayCodeLockUpdate(attackerPlayer, 3);
                            }
                        });
                    }
                }
            }
            catch (Exception ex) { PrintError(ex.ToString() + "^Failed to complete OnEntityTakeDamage^"); }

            return null;
        }

        private readonly Dictionary<BasePlayer, Coroutine> ActiveCraftCoroutine = new();
        private readonly Dictionary<BasePlayer, Coroutine> ActiveWeightOfTheWorldCoroutine = new();

        private IEnumerator CraftHack(BasePlayer crafter, float scalePerc)
        {
            if (crafter == null) yield return null;

            if (crafter.IsAdmin) PrintWarning("CraftHack start for " + crafter + " perc: " + scalePerc);

            // PrintWarning("Craft hack start");
            var perc = (int)Math.Round(scalePerc, MidpointRounding.AwayFromZero);

            if (crafter.IsAdmin) PrintWarning("rounded perc: " + perc);

            var itemCrafter = crafter?.inventory?.crafting ?? null;
            var max2 = 50000;
            var count2 = 0;

            if (itemCrafter?.queue == null || itemCrafter.queue.Count < 1) PrintWarning("itemCrafter has no queue or is null!!");

            while (itemCrafter?.queue != null && itemCrafter.queue.Count > 0)
            {
                count2++;
                if (count2 >= max2)
                {
                    PrintWarning("hit max on while loop (top), " + count2 + " >= " + max2);
                    break;
                }

                if (crafter == null || crafter.IsDestroyed || crafter.gameObject == null || !crafter.IsConnected || crafter.IsDead())
                {
                    PrintWarning("crafter is null on CraftHack, count2: " + count2);
                    break;
                }

                var task = itemCrafter.queue.First.Value;
                if (task == null || task.cancelled) yield return CoroutineEx.waitForSeconds(1f);

                if (task.endTime <= 0f)
                {
                    var max = 500;
                    var count = 0;

                    while (task != null && !task.cancelled && task.endTime == 0.0f)
                    {
                        count++;
                        if (count >= max)
                        {
                            PrintWarning("HIT MAX ON WHILE LOOP (bottom)!");
                            break;
                        }

                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                }



                var nowTime = Time.realtimeSinceStartup;

                var endSeconds = task.endTime - nowTime;


                var remSeconds = endSeconds * perc / 100;
                var newTime = task.endTime - remSeconds;
                var newSeconds = newTime - nowTime;

                task.endTime = newTime;
              //  task.requir= newSeconds; ??workSecondsRequired no longer exists - no longer necessary?

                if (crafter.IsConnected)
                {
                    crafter.SendConsoleCommand("note.craft_start", task.taskUID, newSeconds, task.amount);
                }
                else PrintWarning("crafter isn't connected?!!!");

                if (newSeconds <= 0)
                {
                    var removeTime = 100 * perc / 100;
                    var waitTime = (100 - removeTime) * 0.0015f;

                    if (waitTime <= 0) yield return CoroutineEx.waitForEndOfFrame;
                    else yield return CoroutineEx.waitForSeconds(waitTime);

                    itemCrafter.ServerUpdate(0.0f);

                }

                yield return CoroutineEx.waitForSeconds(newSeconds + 0.01f);
            }

            //      PrintWarning("Craft hack finish ALL");
            if (ActiveCraftCoroutine.TryGetValue(crafter, out Coroutine routine))
            {
                StopCoroutine(routine);
                ActiveCraftCoroutine.Remove(crafter);
            }
            yield return null;
        }

        private object OnItemCraft(ItemCraftTask task, BasePlayer crafter)
        {
            if (task == null || crafter == null) return null;

            var craftBonus = GetCraftingBonus(crafter); //GetCharmCraftingBonuses(crafter);
            if (craftBonus == 0.0) return null;

            if (ActiveCraftCoroutine.TryGetValue(crafter, out Coroutine routine)) return null;

            timer.Once(0.1f, () =>
            {
                if (crafter == null || crafter?.gameObject == null || crafter.IsDestroyed || crafter?.inventory == null || !crafter.IsConnected || crafter.IsDead()) return;
                if (task == null) return;
                if (ServerMgr.Instance != null)
                {
                    routine = StartCoroutine(CraftHack(crafter, craftBonus));
                    ActiveCraftCoroutine[crafter] = routine;
                }
            });

            return null;
        }

        private void OnItemCraftCancelled(ItemCraftTask task, BasePlayer owner, Item fromTempBlueprint)
        {
            if (task == null || owner == null) return;


            if (ActiveCraftCoroutine.TryGetValue(owner, out Coroutine routine) && ServerMgr.Instance != null)
            {
                StopCoroutine(routine);
                ActiveCraftCoroutine.Remove(owner);
                PrintWarning("canceled coroutine for: " + owner.displayName);

                timer.Once(0.3f, () =>
                {
                    var crafter = owner?.inventory?.crafting ?? null;
                    if (crafter != null && crafter.queue != null && crafter.queue.Count > 0)
                    {
                        var craftBonus = GetCraftingBonus(owner);
                        if (craftBonus == 0.0f)
                        {
                            PrintWarning("Craft bonus is 0.0f on 0.3f timer after cancel (queue)");
                            return;
                        }
                        routine = StartCoroutine(CraftHack(owner, craftBonus));
                        ActiveCraftCoroutine[owner] = routine;
                        PrintWarning("craft queue was > 0, started new craft hack after canceling old (to reset time)");
                    }
                });

            }
        }

        private object OnPlayerLand(BasePlayer player, float dmg)
        {
            if (player == null || dmg <= 0f) return null;

            var realDmg = dmg * 500f;
            if (realDmg < 4f) return null; //wtf? this was return true

            var fallDmgLvl = GetLuckInfo(player)?.GetStatLevelByName("FallDMG") ?? 0;
            if (fallDmgLvl < 1) return null;

            var dmgScalar = 1f - (0.2f * fallDmgLvl); //*100. 20% less dmg per rank

            var bleedScalar = 1f - (0.25f * fallDmgLvl); //*100. 25% less bleeding per rank

            var needRng = 12d * fallDmgLvl;

            needRng = GetChanceScaledDouble(player.userID, needRng, needRng, 100);

            if (Random.Range(0, 101) <= needRng)
            {
                var oldHP = player?.Health() ?? 0f;
                player.Hurt(realDmg * dmgScalar, DamageType.Fall, null, true);


                NextTick(() =>
                {
                    if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected || player.IsSleeping()) return;
                    var newHP = player?.Health() ?? 0f;
                    var realDMG = oldHP - newHP;
                    if (realDMG > 0)
                    {
                        player.metabolism.bleeding.Add(dmg * bleedScalar);

                        var sb = Pool.Get<StringBuilder>();
                        try
                        {
                            SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> You took <color=").Append(LUCK_COLOR).Append(">").Append((dmgScalar * 100).ToString("0.00").Replace(".00", string.Empty)).Append("</color>% fall damage!").ToString());
                        }
                        finally { Pool.Free(ref sb); }

                        if (realDmg > 5) Effect.server.Run(player.fallDamageEffect.resourcePath, player.transform.position, Vector3.zero, null, false);
                    }
                });

                return true;
            }

            return null;
        }


        #endregion
        #region Main/Other

        private string MiningColor
        {
            get
            {
                var color = string.Empty;
                if (ZLevelsColors != null && ZLevelsColors.Count > 0) ZLevelsColors.TryGetValue("M", out color);
                return color;
            }
        }

        private double DoCatchup(ulong userID, double number)
        {
            if (userID == 0) throw new ArgumentOutOfRangeException(nameof(userID));
            if (number <= 0.0) return number;
            var catchup = getCatchupMultiplier(userID);
            if (catchup > 0) number += number * (catchup / 100);
            return number;
        }

        private double DoMultipliers(ulong userID, double number)
        {
            if (userID == 0) throw new ArgumentOutOfRangeException(nameof(userID));

            if (number <= 0.0) 
                return number;

            var xpMult = GetXPMultiplier(userID.ToString());
            var level = getLevel(userID);

            var lvlReductionPerc = (level >= 35 && level < 40) ? 20 : (level >= 40 && level < 60) ? 32 : (level >= 60 && level < 80) ? 37.5 : (level >= 80 && level < 100) ? 40 : (level >= 100 && level < 120) ? 55 : level >= 120 ? 80 : 0;

            var player = FindPlayerByID(userID);

            if (player != null)
            {
                var charmXP = GetCharmXPs(player, CharmSkill.Luck);
                if (charmXP < 0.0f) number -= number * Math.Abs(charmXP) / 100;
                if (charmXP > 0.0f) number += number * charmXP / 100;
            }

            if (xpMult > 0) number *= xpMult / 100;
            if (lvlReductionPerc > 0) number -= number * lvlReductionPerc / 100;
            return number;
        }

        private float DoCatchup(ulong userID, float number) { return (float)DoCatchup(userID, (double)number); }

        private float DoMultipliers(ulong userID, float number) { return (float)DoMultipliers(userID, (double)number); }

        private int DoCatchup(ulong userID, int number) { return (int)DoCatchup(userID, (double)number); }

        private int DoMultipliers(ulong userID, int number) { return (int)DoMultipliers(userID, (double)number); }

        private int TimesBeforeMax(int val, int toAdd, int max)
        {
            if (toAdd <= 0)
                throw new ArgumentNullException(nameof(toAdd));

            var timesAdded = 0;
            while (val < max)
            {
                val += toAdd;
                timesAdded++;
            }

            return timesAdded;
        }

        private int TimesBeforeMin(int val, int toRemove, int min)
        {
            if (toRemove <= 0)
                throw new ArgumentOutOfRangeException(nameof(toRemove));

            var timesAdded = 0;
            while (val > min)
            {
                val -= toRemove;
                timesAdded++;
            }

            return timesAdded;
        }

        private float TimesBeforeMin(float val, float toRemove, float min)
        {
            if (toRemove <= 0)
                throw new ArgumentOutOfRangeException(nameof(toRemove));

            var timesAdded = 0;
            while (val > min)
            {
                val -= toRemove;
                timesAdded++;
            }

            return timesAdded;
        }

        private readonly Dictionary<ItemId, int> preItemMultAmt = new();


        private readonly Dictionary<string, Vector3> lastResourceWorldHitPosition = new();

        //move all this shit later
        private readonly Dictionary<string, float> _lastChainLightningFX = new();
        private readonly Dictionary<string, int> _layerMaskCache = new();
        private readonly Dictionary<string, float> _lastChainLightning = new();

        private void OnMeleeAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return;

            var hitEntity = info?.HitEntity;
            if (hitEntity == null)
                return;

          

            if (info?.HitEntity is TreeEntity)
            {

                if (IsWearingSkin(player, CRAZY_EARL_SKIN_ID) && GetLastSkillRotationType(player.UserIDString) != ClassType.Lumberjack)
                {
                    GrantRandomBonusToSkill(player);
                    SetLastSkillRotationType(player.UserIDString, ClassType.Lumberjack);
                }

                if ((GetLuckInfo(player)?.GetStatLevelByName("Whirlwind") ?? 0) > 0)
                {
                    if (!_lastWwReminder.TryGetValue(player.UserIDString, out float lastReminderTime) || (Time.realtimeSinceStartup - lastReminderTime) >= 30f)
                    {
                        var wwCooldown = player?.GetComponent<WhirlwindCooldownHandler>();
                        if (wwCooldown == null || !wwCooldown.IsOnCooldown)
                        {
                            SendLuckNotifications(player, "<color=#50CBDD>Whirlwind</color> <color=#D7B444>can be used again<color=#50CBDD>!</color> Hold down middle mouse while sprinting and holding a hatchet/axe or chainsaw to activate it</color><color=#50CBDD>.</color>");
                            _lastWwReminder[player.UserIDString] = Time.realtimeSinceStartup;
                        }
                    }
                }

                var adjustedChance = _legendaryData.BaseLegendarySpawnChance * 0.00665f; //0.00665f

                var rngVal = Random.Range(0.0f, 1.0f);

                if (rngVal <= adjustedChance)
                {
                    PrintWarning("rng hit, spawn leg after tree hit");
                    var rngLeg = LegendaryItemInfo.GetRandomLegendaryWithClassBias(GetUserClassType(player.UserIDString));

                    if (rngLeg == null) PrintError("rngLeg is null!!!");
                    else if (!rngLeg.SpawnIntoInventory(player.inventory.containerMain))
                    {
                        var item = rngLeg.SpawnItem();

                        if (!item.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation))
                        {
                            PrintWarning("couldn't drop legendary item after creation!!!");
                            RemoveFromWorld(item);
                        }
                        else PrintWarning("dropped legendary item after creation :sunglasses:");
                    }

                    CelebrateLegendarySpawn(player, rngLeg);
                }
            }

            var dispenser = info?.HitEntity?.GetComponent<ResourceDispenser>() ?? null;

            if (dispenser != null)
            {
                var hitPos = info?.HitPositionWorld ?? Vector3.zero;
                if (hitPos != Vector3.zero) lastResourceWorldHitPosition[player.UserIDString] = hitPos;
            }



            var toolEnt = (info?.Weapon ?? info?.WeaponPrefab ?? player?.GetHeldEntity()) as BaseMelee;
            if (toolEnt == null)
                return;
            
            if (IsWearingSkin(player, MINE_CAP_SKIN_ID))
            {
                var newPos = (info?.HitEntity as OreResourceEntity)?._hotSpot?.transform?.position ?? (info?.HitEntity as TreeEntity)?.xMarker?.transform?.position ?? Vector3.zero;
                if (newPos != Vector3.zero) info.HitPositionWorld = newPos;
            }

            if (IsWearingSkin(player, ELECTRIC_GLOVES_SKIN_ID) && (!_lastChainLightning.TryGetValue(player.UserIDString, out float lastLightning) || (Time.realtimeSinceStartup - lastLightning) > 0.125f))
            {
                //messy, but works
                //this isn't actually chain lightning yet. can we make it chain somehow? probably. will we? maybe eventually.
                var chance = 1.33f * (toolEnt?.repeatDelay ?? 1f);

                if (Random.Range(0, 101) <= GetChanceScaledDouble(player.userID, chance, chance, 99))
                    hitEntity.Invoke(() =>
                    {
                        var watch = Pool.Get<Stopwatch>();
                        try
                        {
                            _lastChainLightning[player.UserIDString] = Time.realtimeSinceStartup;

                            info.DidGather = false;
                            info.CanGather = true;

                            var oldY = info.HitPositionWorld.y;


                            try
                            {

                                var nearEnts = Pool.GetList<BaseEntity>();
                                try
                                {


                                    var layerName = LayerMask.LayerToName(hitEntity.gameObject.layer);

                                    if (!_layerMaskCache.TryGetValue(layerName, out int cachedLayer))
                                        _layerMaskCache[layerName] = cachedLayer = LayerMask.GetMask(layerName);


                                    Vis.Entities(hitEntity.transform.position, 12f, nearEnts, cachedLayer);

                                    var now = Time.realtimeSinceStartup;

                                    var canRunFx = !_lastChainLightningFX.TryGetValue(player.UserIDString, out float lastFxTime) || Time.realtimeSinceStartup - lastFxTime > 0.33f;

                                    if (canRunFx)
                                        _lastChainLightningFX[player.UserIDString] = now;

                                    for (int j = 0; j < nearEnts.Count && j < 8; j++) //cap at 8
                                    {
                                        var entToHit = nearEnts[j];

                                        var toHitDispenser = entToHit?.GetComponent<ResourceDispenser>();

                                        if (toHitDispenser == null)
                                        {
                                            PrintWarning("toHitDispenser is null?!");
                                            continue;
                                        }

                                        info.HitPositionWorld = entToHit.transform.position;

                                        var hitPosAdj = info?.HitPositionWorld ?? player?.eyes?.position ?? Vector3.zero;

                                        hitPosAdj.y = oldY;

                                        for (int k = 0; k < Random.Range(1, 4); k++)
                                        {
                                            info.gatherScale = Mathf.Clamp(info.gatherScale - 0.0667f, 0.033f, info.gatherScale);

                                            try
                                            {
                                                if (canRunFx)
                                                    Effect.server.Run(SHOCK_EFFECT, hitPosAdj, Vector3.zero);
                                            }
                                            finally
                                            {
                                                try
                                                {
                                                    toHitDispenser.OnAttacked(info);
                                                    Interface.Oxide.CallHook("OnMeleeAttack", player, info); //again, this hook does not call itself - p.s to past self, we're calling OnMeleeAttack in OnMeleeAttack, this has some unfortunate issues. a dictionary fixed everything though
                                                }
                                                finally
                                                {
                                                    info.DidGather = false;
                                                    info.CanGather = true;
                                                }
                                            }

                                        }

                                    }

                                }
                                finally { Pool.FreeList(ref nearEnts); }


                            }
                            finally
                            {
                                info.DidGather = true;
                                info.CanGather = false;
                            }
                        }
                        finally
                        {
                            try { if (watch.Elapsed.TotalMilliseconds > 45) PrintWarning("hitEntity.invoke for chain lightning gloves took: " + watch.Elapsed.TotalMilliseconds + "ms"); }
                            finally { Pool.Free(ref watch); }
                        }

                    }, 0.01f);
            }

            var weaponItem = info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem();


            var wotw = (info?.Weapon is BaseMelee || info?.WeaponPrefab is BaseMelee) ? (GetLuckInfo(player)?.GetStatLevelByName("WeightOfTheWorld") ?? 0) : 0;

            if (info?.HitEntity is OreResourceEntity)
            {

                if (IsWearingSkin(player, CRAZY_EARL_SKIN_ID) && GetLastSkillRotationType(player.UserIDString) != ClassType.Miner)
                {
                    GrantRandomBonusToSkill(player);
                    SetLastSkillRotationType(player.UserIDString, ClassType.Miner);
                }

                if (wotw > 0 && !HasWOTWActive(player))
                {
                    if (!_lastWotwReminder.TryGetValue(player.UserIDString, out float lastReminderTime) || (Time.realtimeSinceStartup - lastReminderTime) >= 60f)
                    {
                        var cooldown = player?.GetComponent<WeightOfTheWorldCooldownHandler>();
                        if (cooldown == null || !cooldown.IsOnCooldown)
                        {
                            SendLuckNotifications(player, "<color=#C68826>Weight of the World</color> <color=#9E4122>can be used again<color=#C68826>!</color> Attack the ground with a pickaxe/jackhammer to activate it</color><color=#C68826>.</color>");
                            _lastWotwReminder[player.UserIDString] = Time.realtimeSinceStartup;
                        }
                    }
                }

                if (weaponItem?.skin == 1100085862 && Random.Range(0, 101) < 10)
                {

                    var hitEnt = info.HitEntity;
                    PrintWarning("should ore explosionf x");

                    Effect.server.Run("assets/bundled/prefabs/fx/ore_break.prefab", info.HitEntity.transform.position, Vector3.zero);

                    Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/explosion.prefab", info.HitEntity.transform.position, Vector3.zero);

                    Effect.server.Run("assets/bundled/prefabs/fx/bucket_drop_debris.prefab", info.HitEntity.transform.position, Vector3.zero);

                    //needs much better fx, also hurt player

                    PrintWarning("KILL!");

                    player.Invoke(() =>
                    {
                        if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead()) return;
                        player.Hurt(Random.Range(4, 12), DamageType.Explosion, hitEnt);

                    }, 0.075f);

                    hitEnt.Invoke(() =>
                    {
                        if (hitEnt == null || hitEnt.IsDestroyed) return;

                        hitEnt.Kill();
                    }, 0.2f);
                }


            }

            var hitBonusMarker = false;

            if (info?.HitEntity is TreeEntity tree && GetActiveItem(player)?.skin == RUBY_STONE_HATCHET_SKIN_ID)
            {
                var repeatDelay = toolEnt?.repeatDelay ?? 0f;

                hitBonusMarker = (info?.HitEntity as TreeEntity)?.DidHitMarker(info) ?? false;

                var chance = hitBonusMarker ? 2.5f : 1 * repeatDelay;

                if (Random.Range(0, 101) <= chance)
                {
                    //"assets/prefabs/misc/orebonus/effects/bonus_fail.prefab"
                    //assets/prefabs/misc/orebonus/effects/ore_finish.prefab

                    var isHqm = Random.Range(0, 101) <= 15;


                    //effect not prominent enough.
                    Effect.server.Run(!isHqm ? "assets/prefabs/misc/orebonus/effects/bonus_fail.prefab" : "assets /prefabs/misc/orebonus/effects/ore_finish.prefab", info.HitPositionWorld);

                    var rngItemAmount = isHqm ? Random.Range(3, 9) : Random.Range(75, 126);

                    var metalItem = ItemManager.CreateByItemID(!isHqm ? METAL_ORE_ITEM_ID : HQ_ORE_ITEM_ID, rngItemAmount);

                    player.GiveItem(metalItem, BaseEntity.GiveItemReason.ResourceHarvested);

                }
            }

            var fangLvl = GetLuckInfo(player)?.GetStatLevelByName("TwoFang") ?? 0;
            if (fangLvl < 1)
                return;
            
            hitBonusMarker = (info?.HitEntity as TreeEntity)?.DidHitMarker(info) ?? false;

            if (!hitBonusMarker)
            {
                var hotSpot = (info?.HitEntity as OreResourceEntity)?._hotSpot;

                hitBonusMarker = hotSpot != null && Vector3.Distance(info.HitPositionWorld, hotSpot.transform.position) <= hotSpot.GetComponent<SphereCollider>().radius * 1.5;

            }

            var rngNeed = 3 * fangLvl * Mathf.Clamp(toolEnt.repeatDelay, 0.1f, 1f) * (hitBonusMarker ? 1.75 : 1);
            //var rngNeed = 0.9 * fangLvl * Mathf.Clamp(toolEnt.repeatDelay, 0.1f, 1f) * (hitBonusMarker ? 1.75 : 1);

            rngNeed = GetChanceScaledDouble(player.userID, rngNeed, rngNeed, 99, 1);


            if (Random.Range(0f, 101f) > rngNeed) return;

            hitEntity.Invoke(() =>
            {
                try
                {
                    if (info == null)
                    {
                        PrintWarning("info is null 0.01 later!!");
                        return;
                    }

                    if (hitEntity == null || hitEntity.IsDestroyed)
                    {
                        PrintWarning("hitEntity is null/destroyed 0.01 later!!");
                        return;
                    }

                    if (player == null || player.gameObject == null || player.IsDestroyed || player.IsDead() || !player.IsConnected)
                    {
                        PrintWarning("player is no longer valid on double attack invoke");
                        return;
                    }

                    if (dispenser != null)
                    {
                        info.DidGather = false;
                        info.CanGather = true;
                    }


                    try
                    {

                        var hammer = info?.Weapon as Hammer;
                        if (hammer != null && dispenser == null) hammer.ServerUse_OnHit(info);
                        else if (hammer == null)
                        {
                            if (dispenser != null)
                            {
                                dispenser.OnAttacked(info);
                                Interface.Oxide.CallHook("OnMeleeAttack", player, info); //above doesn't auto call this - to my surprise
                            }
                            else hitEntity.OnAttacked(info);
                        }

                        hitEntity?.Invoke(() =>
                        {
                            var materialName = StringPool.Get(info.HitMaterial);

                            var hitEffectPath = info?.HitEntity?.GetImpactEffect(info)?.resourcePath ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(hitEffectPath))
                                Effect.server.Run(hitEffectPath, info.HitPositionWorld, Vector3.zero);


                            var strikeEffectPath = (info?.Weapon as BaseMelee)?.GetStrikeEffectPath(materialName);

                            if (string.IsNullOrWhiteSpace(strikeEffectPath))
                                return;

                            if (info.HitEntity.IsValid())
                                Effect.client.Run(strikeEffectPath, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal);
                            else
                                Effect.client.Run(strikeEffectPath, info.HitPositionWorld, info.HitNormalWorld, new Vector3(), Effect.Type.Generic);

                        }, 0.15f);


                        SendLocalEffect(player, "assets/prefabs/weapons/arms/effects/shove.prefab");


                        var sb = Pool.Get<StringBuilder>();
                        try
                        {
                            var swordFX = sb.Clear().Append("assets/prefabs/weapons/sword/effects/attack-").Append(Random.Range(1, 4)).Append(".prefab").ToString();

                            SendLocalEffect(player, swordFX);

                            //  SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LuckColor).Append(">Lucky!</color> <color=red>Two Fang!</color>").ToString());
                        }
                        finally { Pool.Free(ref sb); }


                    }
                    finally
                    {
                        if (dispenser != null)
                        {
                            info.DidGather = true;
                            info.CanGather = false;
                        }
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }
            }, 0.01f);

        }


        private object OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (dispenser == null || entity == null || item == null) 
                return null;

            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                var entName = entity?.ShortPrefabName ?? string.Empty;
                var itemName = item?.info?.shortname ?? string.Empty;
                var itemAmount = item?.amount ?? -1;
                var dispName = dispenser?.name ?? string.Empty;

                var player = entity as BasePlayer;
                if (player == null || dispenser == null) 
                    return null;

                var luckInfo = GetLuckInfo(player);
                if (luckInfo == null) 
                    return null;

                var originalItemAmount = item?.amount ?? 0;

                var toolItem = GetActiveItem(player);


                if (dispenser.gatherType == ResourceDispenser.GatherType.Tree)
                {

                   

                    var felling = luckInfo.GetStatLevelByName("Felling");

                    if ((DateTime.Now - SaveRestore.SaveCreatedTime).TotalDays > 2)
                    {
                        var hasLumberjack = IsWearingItem(player, LUMBERJACK_SUIT_ID);

                        var eggChance = Random.Range(0.325f, 0.85f);

                        var woodcuttingZLvl = ZLevelsRemastered?.Call<int>("GetLevel", player.UserIDString, "WC") ?? -1;

                        if (woodcuttingZLvl >= 99) eggChance *= 2;

                        if (hasLumberjack) eggChance *= Random.Range(1.25f, 2f);


                        if (toolItem?.info?.itemid == CHAINSAW_ID) eggChance *= 0.11f;

                        var rng = Random.Range(0f, 101f);
                        if (rng <= eggChance)
                        {
                            var eggTypeRng = Random.Range(0, 101);

                            var eggType = eggTypeRng <= 1 ? GOLD_EGG_ID : eggTypeRng <= 10 ? SILVER_EGG_ID : BRONZE_EGG_ID;

                            var eggItem = ItemManager.CreateByItemID(eggType, 1);

                            var treeCenter = dispenser?.GetComponent<BaseEntity>()?.CenterPoint() ?? Vector3.zero;

                            var rngRot = new Vector3(Random.Range(0, 200), Random.Range(0, 200), Random.Range(0, 200));

                            Effect.server.Run(EGG_EXPLOSION_EFFECT, player?.eyes?.position ?? treeCenter, Vector3.zero);


                            if (hasLumberjack && (eggItem.MoveToContainer(player.inventory.containerMain) || eggItem.MoveToContainer(player.inventory.containerBelt)))
                            {
                                NoteInvByItem(player, eggItem);
                            }
                            else
                            {
                                if (!eggItem.Drop(SpreadVector2(treeCenter, Random.Range(0.5f, 1.7f)), Vector3.up * Random.Range(0.8f, 1.8f), Quaternion.Euler(rngRot))) RemoveFromWorld(eggItem);
                            }
                        }
                    }

                    if (felling > 0)
                    {
                        var fellWatch = Stopwatch.StartNew();

                        var resEnt = dispenser?.GetComponent<ResourceEntity>() ?? null;

                        var tree = resEnt as TreeEntity;
                        if (tree != null)
                        {
                            var rngFelling = Random.Range(0f, 101f);

                            var perc = (toolItem?.info?.itemid == CHAINSAW_ID ? 0.214 : 1.5) * felling;
                            //var perc = (toolItem?.info?.itemid == CHAINSAW_ID ? 0.0535 : 0.375) * felling;

                            var userClass = GetUserClassType(player.UserIDString);

                            var scalar = userClass == ClassType.Lumberjack ? 1.25f : (userClass == ClassType.Undefined || userClass == ClassType.Loner) ? 1f : 0.5f;
                            perc = GetChanceScaledDouble(player.userID, perc, perc, 99, scalar);

                            if (rngFelling <= perc)
                            {

                                var treeCenter = tree?.CenterPoint() ?? Vector3.zero;

                                var hasAP = (luckInfo?.GetStatLevelByName("autopickup") ?? 0) > 0;
                                _fellingHitInfo.PointStart = player.eyes.position;
                                _fellingHitInfo.PointEnd = treeCenter;

                                var treeHealth = tree?.Health() ?? 0f;
                                NextTick(() =>
                                {
                                    if (tree == null || tree.IsDestroyed) return;
                                    var newHealth = tree?.Health() ?? 0f;

                                    if (newHealth <= 0f)
                                    {
                                        PrintWarning("newHealth <= 0f for tree, return!");
                                        return;
                                    }

                                    var dmg = treeHealth - newHealth;
                                    if (dmg <= 0)
                                    {
                                        PrintWarning("dmg <= 0 for tree, return!");
                                        return;
                                    }

                                    var toDmg = TimesBeforeMin(newHealth, dmg, 0);

                                    var newAmt = 0;
                                    if (item != null && !preItemMultAmt.TryGetValue(item.uid, out newAmt)) newAmt = originalItemAmount;

                                    if (newAmt < 1)
                                    {
                                        PrintWarning("newAmt < 1!!!!!! for felling!!!");
                                        return;
                                    }


                                    //could potentially optimize by creating 1 wood item instead and splitting/dropping only if necessary?
                                    for (int i = 0; i < toDmg; i++)
                                    {
                                        var newWood = ItemManager.Create(WoodDefinition, newAmt);

                                        if (newWood == null)
                                            continue; //why did we have 5 different lines of if checks for newWood???

                                        if (ZLevelsRemastered?.IsLoaded ?? false)
                                            ZLevelsRemastered.Call("OnDispenserGather", dispenser, entity, newWood);

                                        if (ZZTaxChest?.IsLoaded ?? false)
                                            ZZTaxChest.Call("DoTax", newWood);

                                        if (LocalStatsAPI?.IsLoaded ?? false)
                                            LocalStatsAPI.Call("AddGatherItem", player.UserIDString, newWood);

                                        if (hasAP && player?.inventory != null)
                                            player.GiveItem(newWood);
                                        else
                                        {
                                            var rngRot = new Vector3(Random.Range(0, 200), Random.Range(0, 200), Random.Range(0, 200));
                                            if (newWood != null && !newWood.Drop(SpreadVector2(treeCenter, Random.Range(0.5f, 1.7f)), Vector3.up * Random.Range(0.8f, 1.8f), Quaternion.Euler(rngRot))) RemoveFromWorld(newWood);
                                        }


                                    }

                                    tree.OnKilled(_fellingHitInfo);

                                    var xpAmt = DoCatchup(player.userID, DoMultipliers(player.userID, newAmt * 0.0325f * toDmg));

                                    AddXP(player.UserIDString, xpAmt, XPReason.Tree);
                                });
                            }

                            fellWatch.Stop();
                            if (fellWatch.Elapsed.TotalMilliseconds >= 2) PrintWarning("Felling watch took: " + fellWatch.Elapsed.TotalMilliseconds + "ms");
                        }

                       
                    }

                }


                int eruption;
                if (dispenser.gatherType == ResourceDispenser.GatherType.Ore && (eruption = luckInfo?.GetStatLevelByName("eruption") ?? 0) > 0)
                {

                    var eruptionRng = 2d * eruption; //0.4 * eruption;

                    var userClass = GetUserClassType(player.UserIDString);

                    var scalar = userClass == ClassType.Miner ? 1.25f : (userClass == ClassType.Undefined || userClass == ClassType.Loner) ? 1f : 0.5f;

                    eruptionRng = GetChanceScaledDouble(player.userID, eruptionRng, eruptionRng, 99, scalar);


                    if (Random.Range(0, 101) <= eruptionRng)
                    {
                        var ent = dispenser?.GetComponent<BaseEntity>() ?? null;
                        if (ent == null)
                        {
                            PrintWarning("no entity on eruption!!");
                            return null;
                        }

                        if (!lastResourceWorldHitPosition.TryGetValue(player.UserIDString, out Vector3 dropPos))
                        {
                            PrintWarning("failed to get last resource world hit pos");
                            dropPos = ent?.CenterPoint() ?? Vector3.zero;
                        }

                        if (dropPos == Vector3.zero)
                        {
                            PrintWarning("dropPos is vector3 zero for eruption!!");
                            return null;
                        }

                        if (!HasWOTWActive(player)) //perf/spam reduc by not sending erup messages during a wotw
                        {
                            var sb = Pool.Get<StringBuilder>();
                            try
                            {
                                SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> <color=").Append(MiningColor).Append(">Eruption!</color>").ToString());
                            }
                            finally { Pool.Free(ref sb); }
                        }


                        var adjPos = dropPos;
                        adjPos.y += 2.5f;

                        for (int i = 0; i < 3; i++) 
                            Effect.server.Run("assets/bundled/prefabs/fx/bucket_drop_debris.prefab", adjPos, Vector3.zero);

                        var oreLower = 28 + (14 * eruption);
                        var oreHigher = 40 + (45 * eruption);
                        var sulfurScalar = dispenser.name.Contains("sulfur") ? 0.6f : 0.25f;
                        var metalScalar = dispenser.name.Contains("metal") ? 1.2f : 0.9f;
                        var stoneScalar = dispenser.name.Contains("stone") ? 1.4f : 1f;

                        var rngSulfur = (sulfurScalar == 1f) ? Random.Range(oreLower, oreHigher) : (int)Math.Round(Random.Range(oreLower, oreHigher) * sulfurScalar, MidpointRounding.AwayFromZero);
                        var rngMetal = (int)Math.Round(Random.Range(oreLower, oreHigher) * metalScalar, MidpointRounding.AwayFromZero);
                        var rngStone = (int)Math.Round(Random.Range(oreLower, oreHigher) * stoneScalar, MidpointRounding.AwayFromZero);
                        var rngHQM = (int)Math.Round(Mathf.Clamp(Random.Range(oreLower * 0.025f, oreHigher * 0.025f), Random.Range(1, eruption + 1), 200), MidpointRounding.AwayFromZero);


                        var items = Pool.GetList<ItemAmount>();
                        try
                        {

                            _cachedSulfurItemAmount.amount = rngSulfur;
                            _cachedMetalItemAmount.amount = rngMetal;
                            _cachedStonesItemAmount.amount = rngStone;
                            _cachedHQMAmount.amount = rngHQM;


                            items.Add(_cachedSulfurItemAmount);
                            items.Add(_cachedMetalItemAmount);
                            items.Add(_cachedStonesItemAmount);
                            items.Add(_cachedHQMAmount);


                            var xpAmt = DoCatchup(player.userID, DoMultipliers(player.userID, (rngSulfur + rngMetal + rngStone + rngHQM) * 0.1f));

                            var onEruption = Interface.Oxide.CallHook("OnLuckEruption", player, xpAmt, items);

                            if (onEruption != null)
                                xpAmt = (float)onEruption;


                            AddXP(player.UserIDString, xpAmt, XPReason.Ore);

                            var charmItem = GetCharmGathers(player, CharmSkill.Mining);

                            var hasAutoPickup = luckInfo.GetStatLevelByName("autopickup") > 0;

                            for (int i = 0; i < items.Count; i++)
                            {
                                var oreInfo = items[i];

                                if (oreInfo == null)
                                    throw new ArgumentNullException(nameof(oreInfo));
                                

                                Item ore = null;
                                var dontCreate = false;

                                if (charmItem < 0.0f) oreInfo.amount -= oreInfo.amount * Math.Abs(charmItem) / 100;
                                if (charmItem > 0.0f) oreInfo.amount += oreInfo.amount * charmItem / 100;

                                var finalAmount = Mathf.Clamp((int)Math.Round(oreInfo.amount, MidpointRounding.AwayFromZero), 1, oreInfo.itemDef.stackable/*/ore.MaxStackable()/*/);

                                LocalStatsAPI?.Call("AddGatherDef", player.UserIDString, oreInfo.itemDef, finalAmount);

                                var taxDeduction = 0;

                                taxDeduction = ZZTaxChest?.Call<int>("DoTaxDef", oreInfo.itemDef, finalAmount) ?? 0;

                                finalAmount = Mathf.Clamp(finalAmount - taxDeduction, 1, oreInfo.itemDef.stackable);

                                if (hasAutoPickup && player?.inventory != null)
                                {
                                    for (int j = 0; j < player.inventory.containerMain.itemList.Count; j++)
                                    {
                                        var pItem = player.inventory.containerMain.itemList[j];
                                        if (pItem?.info == oreInfo?.itemDef && ((pItem.amount + oreInfo.amount) <= pItem.MaxStackable()))
                                        {
                                            ore = pItem;
                                            break;
                                        }
                                    }

                                    for (int j = 0; j < player.inventory.containerBelt.itemList.Count; j++)
                                    {
                                        var pItem = player.inventory.containerBelt.itemList[j];
                                        if (pItem?.info == oreInfo?.itemDef && ((pItem.amount + oreInfo.amount) <= pItem.MaxStackable()))
                                        {
                                            ore = pItem;
                                            break;
                                        }
                                    }
                                }

                                if (ore == null) ore = ItemManager.Create(oreInfo.itemDef);
                                else dontCreate = true;

                                if (ore.amount + finalAmount <= ore.MaxStackable())
                                {
                                    ore.amount += finalAmount;
                                    ore.MarkDirty();
                                }
                                else
                                {
                                    if (dontCreate)
                                    {
                                        dontCreate = false;
                                        ore = ItemManager.Create(oreInfo.itemDef, finalAmount);
                                    }
                                }

                                if (dontCreate)
                                {
                                    NoteItemByID(player, ore.info.itemid, finalAmount);
                                    continue;
                                }
                                else finalAmount = ore.amount;

                                if (hasAutoPickup && player?.inventory != null)
                                {
                                    NoteItemByID(player, ore.info.itemid, finalAmount);


                                    if (!ore.MoveToContainer(player.inventory.containerMain) && !ore.MoveToContainer(player.inventory.containerBelt))
                                    {
                                        NoteItemByID(player, ore.info.itemid, -finalAmount);
                                    }
                                    else continue;
                                }

                                var spreadPos = dropPos;
                                spreadPos.y += Random.Range(0.175f, 0.25f);

                                var oldY = spreadPos.y;
                                spreadPos = SpreadVector2(spreadPos, 0.28f);
                                if (spreadPos.y < oldY) spreadPos.y = oldY;


                                if (!ore.Drop(spreadPos, Vector3.up * Random.Range(5f, 7f)))
                                {
                                    PrintError("failed to drop ore!?!?!??!?!");
                                    RemoveFromWorld(ore);
                                }
                                else
                                {
                                    var oreDrop = ore?.GetWorldEntity() as DroppedItem;
                                    oreDrop?.Invoke(() => { if (!oreDrop?.IsDestroyed ?? true) RemoveFromWorld(ore); }, 90f);
                                }
                            }
                        }
                        finally
                        {
                            Pool.FreeList(ref items);
                        }
                    }
                }

                if (luckInfo.BackpackAutoDeposit && _userBackpack.TryGetValue(player.UserIDString, out var backpackBce) && backpackBce is DroppedItemContainer backpack && backpack?.inventory != null)
                {

                    var amtPreMove = item?.amount ?? 0;
                    var itemId = item?.info?.itemid ?? 0;

                    if (!item.MoveToContainer(backpack.inventory, allowSwap: false))
                    {
                        PrintWarning("coudln't move to backpack with allowSwap false. return null.");

                        return null;
                    }

                    NoteItemByID(player, itemId, amtPreMove, (item?.name ?? string.Empty), BaseEntity.GiveItemReason.ResourceHarvested);

                    return true; //stops from giving item 'naturally'.
                }

                watch.Stop();
                if (watch.Elapsed.TotalMilliseconds > 3) PrintWarning("OnDispenserGather took: " + watch.Elapsed.TotalMilliseconds + "ms, disp name: " + dispName + " ent: " + entName + ", item: " + itemName + " x" + itemAmount);
            }
            finally { Pool.Free(ref watch); }

            return null;
        }

        private Vector3 SpreadVector(Vector3 vec, float rocketSpread = 1.5f) { return Quaternion.Euler(Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f)) * vec; }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + Random.insideUnitSphere * spread; }

        private readonly Hash<ulong, float> lastRejack = new();
        private readonly HashSet<ulong> _forceRejack = new();

        private ItemAmount _growableCachedAmount = new ItemAmount();

        private void OnGrowableGathered(GrowableEntity plant, Item item, BasePlayer player)
        {
            if (player == null || item == null || player == null) return;



            //a legendary that increases legendary drop chance from crops based on # of the 'good genes' of your crop?

            var adjustedChance = _legendaryData.BaseLegendarySpawnChance * 0.039f; //0.00665f

            var rngVal = Random.Range(0.0f, 1.0f);

            if (rngVal <= adjustedChance)
            {
                PrintWarning("rng hit, spawn leg after crop harvest");

                var rngLeg = LegendaryItemInfo.GetRandomLegendaryWithClassBias(GetUserClassType(player.UserIDString));

                if (rngLeg == null) PrintError("rngLeg is null!!!");
                else if (!rngLeg.SpawnIntoInventory(player.inventory.containerMain))
                {
                    var legItem = rngLeg.SpawnItem();

                    if (!legItem.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation))
                    {
                        PrintWarning("couldn't drop legendary item after creation!!!");
                        RemoveFromWorld(legItem);
                    }
                    else PrintWarning("dropped legendary item after creation :sunglasses:");
                }

                CelebrateLegendarySpawn(player, rngLeg);
            }

            var userClass = GetUserClassType(player.UserIDString);
            if (userClass != ClassType.Farmer) return;

            if (player.IsAdmin) PrintWarning(nameof(OnGrowableGathered) + " for farmer class: " + player + ", plant: " + plant + ", item: " + item.info.shortname + " x" + item.amount);


            _growableCachedAmount.itemDef = item.info;
            _growableCachedAmount.amount = item.amount;

            HandleLuckItem(player, _growableCachedAmount);

            if (_growableCachedAmount.itemDef == item.info)
                item.amount = (int)Mathf.Clamp((float)Math.Round(_growableCachedAmount.amount, MidpointRounding.AwayFromZero), 1, int.MaxValue);


            var consume = item?.info?.GetComponent<ItemModConsume>();
            var itemAmount = consume?.product != null && consume.product.Length > 0 ? consume?.product[0] : null;
            if (itemAmount?.itemDef != null)
            {
                PrintWarning("got itemAmount, give seeds now!");

                var seedItem = ItemManager.Create(itemAmount.itemDef, Mathf.Clamp(itemAmount.RandomAmount(), 1, itemAmount.itemDef.stackable));

                player.GiveItem(seedItem);
            }
        }

        //there should be a chance of getting legendary items, especially for elites, as a reward for hits
        private void OnNpcConversationRespond(NPCTalking npc, BasePlayer player, ConversationData convFor, ConversationData.ResponseNode response)
        {
            if (npc == null || player == null || convFor == null || response == null) return;

            if (!(response?.responseText ?? string.Empty).Equals("I'm just browsing.", StringComparison.OrdinalIgnoreCase)) return;

            var userClassType = GetUserClassType(player.UserIDString);

            List<Item> rewardItems = null;

            var pItems = Pool.GetList<Item>();
            try
            {
                player.inventory.AllItemsNoAlloc(ref pItems);


                for (int i = 0; i < pItems.Count; i++)
                {
                    var pItem = pItems[i];

                    if (pItem == null || string.IsNullOrWhiteSpace(pItem?.name)) continue; //dog tags must be named

                    var itemId = pItem?.info?.itemid ?? 0;

                    if (itemId != -602717596 && itemId != 1036321299 && itemId != 1223900335) continue; //red, blue, neutral

                    if (!_tagToTier.TryGetValue(pItem.uid, out NPCHitTarget.Tiers dogTagTier))
                    {
                        dogTagTier = itemId == -602717596 ? NPCHitTarget.Tiers.Elite : itemId == 1036321299 ? NPCHitTarget.Tiers.High : (Random.Range(0, 101) <= 33 ? NPCHitTarget.Tiers.Medium : NPCHitTarget.Tiers.Low);

                        PrintWarning("no tag to tier found! generated: " + dogTagTier + " for itemid: " + itemId);

                    }

                    var dogTagName = pItem?.name;

                    RemoveFromWorld(pItem);

                    rewardItems ??= new List<Item>();

                    GenerateRewardsBasedOnTierNoAlloc(dogTagTier, ref rewardItems);

                    var luckXpToAddMin = 960 * (int)dogTagTier;
                    var luckXpToAddMax = 2700 * (int)dogTagTier;

                    var luckXpToAdd = DoCatchup(player.userID, DoMultipliers(player.userID, Random.Range(luckXpToAddMin, luckXpToAddMax)));

                    var survivalXpToAddMin = 720L * (int)dogTagTier;
                    var survivalXpToAddMax = 1440L * (int)dogTagTier;

                    var survivalXpToAdd = Random.Range(survivalXpToAddMin, survivalXpToAddMax); //this needs catchup/multiplier handling from zlevels

                    var zlvlXpMult = ZLevelsRemastered?.Call<double>("GetXPMultiplier", player.UserIDString) ?? 100d;
                    var zlvlCatchupMult = ZLevelsRemastered?.Call<double>("GetCatchupMultiplier", player.UserIDString, "S") ?? 100d;

                    survivalXpToAdd *= (long)((zlvlXpMult + zlvlCatchupMult) / 100f);


                    ZLevelsRemastered?.Call("AddPointsBySkillName", player.UserIDString, "S", survivalXpToAdd);

                    PrintWarning("luckXpToAdd after catchup & mults: " + luckXpToAdd + " for tier: " + dogTagTier + ", survivalXp: " + survivalXpToAdd);

                    AddXP(player.UserIDString, luckXpToAdd);

                    SendReply(player, "Claimed " + dogTagName + " - Enjoy your reward!");

                }

            }
            finally { Pool.FreeList(ref pItems); }


            if (rewardItems != null)
            {
                PrintWarning("rewardItems total count: " + rewardItems.Count + " for: " + player);

                for (int i = 0; i < rewardItems.Count; i++)
                {
                    var item = rewardItems[i];

                    player.GiveItem(rewardItems[i]);

                    if (IsLegendaryItem(item))
                        CelebrateLegendarySpawnFromItem(player, item);

                }


            }

            if (userClassType != ClassType.Mercenary) return;

            if (GetHitTargets(true).Count < 1)
            {
                SendReply(player, "I don't have any hits right now. Come back and see me in a few minutes. I'm getting phone calls now and should have some targets soon.");
                return;
            }

            if (player?.GetComponent<MercenaryHitHandler>() == null)
                player?.gameObject?.AddComponent<MercenaryHitHandler>();

            HitGUI(player);
        }

        private void OnSpawnIntoContainer(LootSpawn spawn, ItemContainer container)
        {
            StripLootSpawnOfLegendaryItems(spawn);
        }

        private int _subSpawnSafety = 0;
        private int StripLootSpawnOfLegendaryItems(LootSpawn spawn, bool checkSubSpawn = false)
        {
            if (spawn == null)
                throw new ArgumentNullException(nameof(spawn));

            if (checkSubSpawn && _subSpawnSafety >= 50000)
            {
                PrintWarning(nameof(StripLootSpawnOfLegendaryItems) + " STOPPED for safety, hit 50k!!!");
                return 0;
            }


            var incompatibleItems = Pool.Get<HashSet<ItemDefinition>>();
            var compatibleItemRange = Pool.Get<HashSet<ItemAmountRanged>>();

            try 
            {
                incompatibleItems.Clear();
                compatibleItemRange.Clear();

                for (int i = 0; i < spawn.items.Length; i++)
                {
                    var spawnItem = spawn.items[i];
                    if (spawnItem?.itemDef == null)
                        continue;

                    if (spawnItem.itemDef.shortname.Contains("surgeon", CompareOptions.OrdinalIgnoreCase)) PrintWarning("SURGEON FOUND!!");

                    if (IsBlockedItem(spawnItem.itemid)) incompatibleItems.Add(spawnItem.itemDef);
                    else compatibleItemRange.Add(spawnItem);

                }



                var diff = spawn.items.Length - incompatibleItems.Count;

                if (incompatibleItems.Count > 0 && diff >= spawn.items.Length)
                {
                    PrintWarning("diff is >= all items!!: " + diff + " >= " + spawn.items.Length);
                }

                if (checkSubSpawn && spawn?.subSpawn != null)
                {
                    _subSpawnSafety++;

                    for (int i = 0; i < spawn.subSpawn.Length; i++)
                        StripLootSpawnOfLegendaryItems(spawn.subSpawn[i].category, true);
                }

                if (spawn.items.Length == compatibleItemRange.Count)
                    return compatibleItemRange.Count;
                

                PrintWarning("setting to compat count: " + compatibleItemRange.Count);
                spawn.items = new ItemAmountRanged[compatibleItemRange.Count];

                var j = 0;
                foreach (var compatible in compatibleItemRange)
                {
                    spawn.items[j] = compatible;
                    j++;
                }

                if (incompatibleItems.Count > 0) 
                    PrintWarning("cleared of " + incompatibleItems.Count + " leggy options.");

              

                return compatibleItemRange.Count;
            }
            finally
            {
                try { Pool.Free(ref incompatibleItems); }
                finally { Pool.Free(ref compatibleItemRange); }
            }

        }

        private void OnCorpsePopulate(ScarecrowNPC npc, NPCPlayerCorpse corpse)
        {
            if (npc == null || corpse == null || corpse?.net == null) return;

            //used for magpie
            if (!_corpseToAlivePrefabName.TryGetValue(corpse.net.ID, out _)) _corpseToAlivePrefabName[corpse.net.ID] = npc.PrefabName;

        }

        private void OnCorpsePopulate(HumanNPC npc, NPCPlayerCorpse corpse)
        {
            if (npc == null || corpse == null || corpse?.net == null) return;

            //used for magpie
            if (!_corpseToAlivePrefabName.TryGetValue(corpse.net.ID, out _)) _corpseToAlivePrefabName[corpse.net.ID] = npc.PrefabName;

            //merc hit stuff            
            npc?.GetComponent<NPCHitTarget>()?.PopulateCorpse(corpse);
            

        }

        private void OnCollectiblePickup(CollectibleEntity entity, BasePlayer player)
        {
            if (player == null || entity == null) return;

            var items = entity.itemList;
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item.itemDef.shortname.Contains("seed")) continue;

                var canPickupItem = Interface.Oxide.CallHook("CanHandleLuckPickup", player, item);
                var canPickupBool = true;
                if (TryParseBool(canPickupItem, ref canPickupBool) && !canPickupBool) continue;

                HandleLuckItem(player, item);
            }

          
        }

        private object OnLuckEruption(BasePlayer player, float points, List<ItemAmount> items)
        {
            if (player == null || points <= 0 || items == null || items.Count < 1) return null;

            var gathMult = GetLuckInfo(player)?.GetStatLevelByName("Gather") ?? 0;
            if (gathMult < 1) return null;


            var itemMultiplier = (gathMult <= 0 ? 0 : (2f + (0.415f * (gathMult - 1)))) * 0.55f; //2x + 0.415 for each level after first level. 3.66 at max. bonus multiplier is only 55% of the original, since this is eruption
            var itemNeed = 2d * (gathMult > 0 ? gathMult : 1);

            var originalMultiplier = itemMultiplier;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || item.amount <= 0) continue;

                if (Random.Range(0, 101) > itemNeed)
                    continue;

                //SULFUR CHECK
                //WE REMOVE 0.36 TO MAKE BASE SCALAR MATCH
                itemMultiplier = item.itemid == SULFUR_ORE_ITEM_ID ? (originalMultiplier - 0.3685f) : originalMultiplier; //originalMultiplier * (item.itemid == -1157596551 ? 0.45f : 1f);

                item.amount *= itemMultiplier; //(int)Math.Round(item.amount * itemMultiplier, MidpointRounding.AwayFromZero); //SULFUR CHECK - ALSO THIS IS ROUNDED IN THE FINAL HANDLING ANYWAY, NO NEED TO ROUND TWICE (PERF)

                var xpAmt = DoCatchup(player.userID, DoMultipliers(player.userID, Mathf.Clamp(item.amount * 0.25f, 1.0f, 1000f)));

                AddXP(player.UserIDString, xpAmt, XPReason.Ore);

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    SendItemMultipliedNotification(player, item.itemDef, itemMultiplier, (int)item.amount, HasWOTWActive(player) ? 3000f : 500f);
                }
                finally { Pool.Free(ref sb); }

            }
            return null;
        }

        private bool SkillMatchesClass(string skill, ClassType classType)
        {
            if (classType == ClassType.Undefined || classType == ClassType.Loner) return true;

            if (classType == ClassType.Miner && skill != "M") return false;
            else if (classType == ClassType.Lumberjack && skill != "WC") return false;
            else if ((classType == ClassType.Nomad || classType == ClassType.Farmer || classType == ClassType.Packrat || classType == ClassType.Mercenary) && skill != "S") return false;

            return true;
        }

        private bool IsApplicableContainerForCharm(uint prefabId)
        {
            return prefabId switch
            {
                CRATE_ELITE_PREFAB_ID or CRATE_UNDERWATER_ELITE_PREFAB_ID or CRATE_HELI_PREFAB_ID or CRATE_NORMAL_PREFAB_ID or CRATE_NORMAL_2_PREFAB_ID or CRATE_TOOLS_PREFAB_ID or CRATE_UNDERWATER_ADVANCED_PREFAB_ID or CRATE_UNDERWATER_BASIC_PREFAB_ID or CRATE_UNDERWATER_NORMAL_PREFAB_ID or CRATE_UNDERWATER_NORMAL_2_PREFAB_ID or CRATE_UNDERWATER_TOOLS_PREFAB_ID or CRATE_BRADLEY_ID or CRATE_LOCKED_CHINOOK or CRATE_LOCKED_OILRIG => true,
                _ => false,
            };
        }

        //later, necessary idea: in json, store a "set ID" variable so that we know what set that legendary belongs to. then, count pieces based on that.

        private readonly Dictionary<BasePlayer, TimeCachedValue<int>> _lumberjackSetCache = new();

        private int LumberjackSetPieces(BasePlayer player, bool skipCache = false)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.IsDestroyed || player.IsDead()) 
                return 0;

            if (!_lumberjackSetCache.TryGetValue(player, out _))
                _lumberjackSetCache[player] = new TimeCachedValue<int>()
                {
                    refreshCooldown = 3f,
                    refreshRandomRange = 0.5f,
                    updateValue = new Func<int>(() =>
                    {
                        var wearItems = player?.inventory?.containerWear?.itemList;
                        if (wearItems == null || wearItems.Count < 1)
                            return 0;

                        var setPieces = 0;

                        for (int i = 0; i < wearItems.Count; i++)
                        {
                            var item = wearItems[i];

                            if (setPieces > 2) break;

                            switch (item.skin)
                            {
                                case LUMBERJACK_BEANIE_SKIN_ID:
                                case LUMBERJACK_SHIRT_SKIN_ID:
                                case LUMBERJACK_PANTS_SKIN_ID:
                                case LUMBERJACK_BOOTS_SKIN_ID:
                                    {
                                        setPieces++;
                                        break;
                                    }
                            }
                        }

                        return setPieces;
                    })
                };

            return _lumberjackSetCache[player].Get(skipCache);
        }

        private int OverallsSetPieces(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || player.IsDead()) return 0;

            var wearItems = player?.inventory?.containerWear?.itemList;
            if (wearItems == null || wearItems.Count < 1) return 0;

            var setPieces = 0;

            for (int i = 0; i < wearItems.Count; i++)
            {
                var item = wearItems[i];

                if (setPieces > 2) break;

                switch (item.skin)
                {
                    case BOONIE_STRAW_HAT_SKIN_ID:
                    case BURLAP_SHIRT_OVERALLS_SKIN_ID:
                    case BURLAP_TROUSERS_OVERALLS_SKIN_ID:
                        {
                            setPieces++;
                            break;
                        }
                }
            }

            return setPieces;
        }

        private object OnEarnZXP(BasePlayer player, Item item, string skill, double points)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrWhiteSpace(skill)) throw new ArgumentNullException(nameof(skill));

            if (points <= 0.0) return null;

            var luckInfo = GetLuckInfo(player);
            if (luckInfo == null) return null;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var toolItem = player?.GetActiveItem();

                var newPoints = points;

                var userClass = GetUserClassType(player.UserIDString);

                var skillMatchesClass = SkillMatchesClass(skill, userClass);

                var classItemMult = skillMatchesClass ? 1.50f : 1.0f;

                var noClassMatchXPMult = 0.25f;

                if (userClass == ClassType.Nomad && IsInAridBiome(player))
                {
                    noClassMatchXPMult = 0.6f;

                    classItemMult = 1.30f;
                }

                if (item != null && classItemMult != 1.0f)
                    item.amount = (int)Math.Round(Mathf.Clamp(item.amount * classItemMult, 1, int.MaxValue), MidpointRounding.AwayFromZero);


                if (!skillMatchesClass)
                {
                    newPoints *= noClassMatchXPMult;
                    if (newPoints < 1) newPoints = 1; //clamp

                   

                }

                if (skill.Equals("WC", StringComparison.OrdinalIgnoreCase))
                {
                    var pieces = LumberjackSetPieces(player);

                    if (pieces == 4)
                        newPoints *= 1.3;
                    else if (pieces > 0) newPoints *= 1 + (0.075 * pieces);

                }

                if (userClass == ClassType.Loner && item != null)
                    item.amount = (int)Math.Round(Mathf.Clamp(item.amount * 1.66f, 1, int.MaxValue), MidpointRounding.AwayFromZero);

                if (skillMatchesClass && userClass == ClassType.Farmer && item != null)
                {
                    if (item.info.category == ItemCategory.Food && item.info.GetComponent<ItemModCookable>() == null)
                    {
                        PrintWarning("this should be a seed or is otherwise uncookable for a farmer: " + item.info.displayName.english + " x" + item.amount + " we will scale it to 2x for them");
                        item.amount *= 2;
                    }

                    if ((item.info.category == ItemCategory.Food || item.info.category == ItemCategory.Resources) && OverallsSetPieces(player) >= 3)
                    {
                        PrintWarning("has 3 set piece! pre item amount (Pre 1.25x scalar): " + item.amount + " for item: " + item.info.displayName.english);
                        item.amount *= 3; //200% bonus (x3)
                        PrintWarning("post item amount: " + item.amount);
                    }


                }




                if (userClass == ClassType.Undefined)
                {
                    var zLvl = ZLevelsRemastered?.Call<int>("GetLevel", player.UserIDString, skill) ?? -1;

                    if (zLvl >= 25)
                        newPoints = 0;//no XP gain if capped to 25 (which they are if no class), because otherwise they could simply gain XP and still pick a class at a later date, effectively destroying the purpose of a level cap since they get that level as soon as they switch to a class and hit a tree/rock (XP is still gained, so it just needs an update letting it know it can go past the old cap now)
                }


                var isChainsaw = toolItem?.info?.itemid == CHAINSAW_ID;
                var isJackhammer = toolItem?.info?.itemid == JACKHAMMER_ID;

                var chanceScalar = isJackhammer ? 0.66f : isChainsaw ? 0.45f : 1f; //this is for the luck skill chance, not the "chance" in general

                if (!skillMatchesClass) chanceScalar *= 0.5f;

                var gathMult = luckInfo?.GetStatLevelByName("Gather") ?? 0;
                var shortName = item?.info?.shortname ?? string.Empty;
                var itemId = item?.info?.itemid ?? 0;


                var xpRnd = xpRandom.Next(0, 101);
                var itemRnd = itemRandom.Next(0, 101);

                var xpNeed = 3.33 * (gathMult > 0 ? gathMult : 1);
                var itemNeed = 2d * (gathMult > 0 ? gathMult : 1);


                if (isChainsaw)
                {
                    xpNeed *= 0.45;
                    itemNeed *= 0.25;
                }
                else if (isJackhammer)
                {
                    xpNeed *= 0.6;
                    itemNeed *= 0.4;
                }


                var displaySkillName = string.Empty;

                if (ZLevelsMessages == null) PrintWarning("ZLevelsMessages null!!!");
                else
                {
                    if (ZLevelsMessages.TryGetValue(sb.Clear().Append(skill).Append("Skill").ToString(), out object skillOut)) displaySkillName = skillOut.ToString();
                    else PrintWarning("No ZLevelsMessages for: " + skill + "Skill");
                }



                var engName = item?.info?.displayName?.english ?? "Unknown";
                var charmSkill = skill switch
                {
                    "WC" => CharmSkill.Woodcutting,
                    "M" => CharmSkill.Mining,
                    "S" => CharmSkill.Survival,
                    _ => CharmSkill.Undefined,
                };
                if (charmSkill == CharmSkill.Undefined)
                    PrintError(nameof(charmSkill) + " is undefined!!!");
                var xpType = charmSkill switch
                {
                    CharmSkill.Mining => XPReason.Ore,
                    CharmSkill.Woodcutting => XPReason.Tree,
                    CharmSkill.Survival => XPReason.HarvestPlant,
                    _ => XPReason.Generic,
                };
                var charmXP = GetCharmXPs(player, charmSkill);

                if (charmXP < 0.0f) newPoints -= newPoints * Math.Abs(charmXP) / 100;
                if (charmXP > 0.0f) newPoints += newPoints * charmXP / 100;


                if (shortName.Contains("metal.fragment") || shortName.Contains("coal") || (shortName.Contains("refined") && !shortName.Contains("ore")))
                {
                    var heliMulti = Random.Range(3f, 9f);
                    newPoints = Math.Round(newPoints * heliMulti, MidpointRounding.AwayFromZero);
                }

                var wasGatherLucky = false;
                var wasXpLucky = false;
                if (gathMult > 0)
                {


                    //check class to see if class matches the type of item gathered. if it does, we need to have better RNG (Luck chances)
                    if (xpRnd <= GetChanceScaledDouble(player.userID, xpNeed, xpNeed, 99, chanceScalar))
                    {
                        var xpMultiplier = gathMult <= 0 ? 0 : (2.5f + (0.375f * (gathMult - 1)));
                        wasXpLucky = true;

                        newPoints = Math.Round(newPoints * xpMultiplier, MidpointRounding.AwayFromZero);
                        if (!ZLevelsColors.TryGetValue(skill, out string color))
                        {
                            PrintWarning("failed to get skill color from ZLevelsColors");
                            color = "white";
                        }

                        //wotw test
                        SendXPMultipliedNotification(player, displaySkillName, xpMultiplier, newPoints, HasWOTWActive(player) ? 3000f : 0f, color);
                        //SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LuckColor).Append(">Lucky!</color> <color=").Append(color).Append(">").Append(skillName).Append("</color> XP multiplied by: <color=").Append(LuckColor).Append(">").Append(xpMultiplier.ToString("0.#").Replace(".0", string.Empty)).Append("</color> (XP earned: <color=").Append(LuckColor).Append(">").Append(newPoints.ToString("N0")).Append("</color>)").ToString());


                    }

                    if (item != null)
                    {
                        //check class to see if class matches the type of item gathered. if it does, we need to have better RNG (Luck chances)

                        if (itemRnd <= GetChanceScaledDouble(player.userID, itemNeed, itemNeed, 99, chanceScalar))
                        {
                            var baseScalar = itemId == SULFUR_ORE_ITEM_ID ? 1.33f : 2f;

                            var itemMultiplier = gathMult <= 0 ? 0 : (baseScalar + (0.415f * (gathMult - 1)));


                            wasGatherLucky = true;
                            // PrintWarning("wasGatherLucky!");

                            preItemMultAmt[item.uid] = item.amount;
                            item.amount = (int)Math.Round(item.amount * itemMultiplier, MidpointRounding.AwayFromZero);

                            //wotw test
                            SendItemMultipliedNotification(player, item.info, itemMultiplier, item.amount, HasWOTWActive(player) ? 3000f : 0f);
                            // PrintWarning("attempted to send item mult notification");
                            //SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LuckColor).Append(">Lucky!</color> ").Append(engName).Append(" multiplied by: <color=").Append(LuckColor).Append(">").Append(itemMultiplier.ToString("0.#")).Append("</color>").Append(" (Total: <color=").Append(LuckColor).Append(">").Append(item.amount.ToString("N0")).Append("</color>)").ToString());

                            var xpAmt = DoCatchup(player.userID, DoMultipliers(player.userID, Mathf.Clamp(item.amount * 0.33f, 1.0f, 1000f)));

                            AddXP(player.UserIDString, xpAmt, xpType);

                        }
                    }
                }

                var luckXpPointsToAdd = 0f;

                if (newPoints != points && (wasGatherLucky || wasXpLucky))
                {
                    var pointsAddBase = Mathf.Clamp((float)newPoints, (float)newPoints, 750);
                    var rndXP = pointsAddBase * (wasXpLucky ? 0.1f : 0.066f);
                    rndXP = DoCatchup(player.userID, DoMultipliers(player.userID, rndXP));
                    luckXpPointsToAdd += rndXP; //add some amount of points to luck!

                }

                //??!??!?! take a look at below. is this not always adding luck xp because "class matches" even though there's no class match check?!?!
                //just added a conditional check to see if skill matches class. omg. how was this not there lol

                if (skillMatchesClass)
                {
                    var classXp = DoCatchup(player.userID, DoMultipliers(player.userID, (float)newPoints * 0.25f));  //add luck xp because skill matches class

                    luckXpPointsToAdd += classXp;
                }

                

                if (luckXpPointsToAdd > 0)
                    AddXP(player.UserIDString, luckXpPointsToAdd, xpType);

                if (newPoints != points)
                    return newPoints;

                return null;
            }
            finally { Pool.Free(ref sb); }
        }

        private object OnEarnLuckXP(BasePlayer player, Item item, float points)
        {
            if (player == null || item == null || points <= 0.0f) 
                return null;

            var luckInfo = GetLuckInfo(player);
            if (luckInfo == null) 
                return null;

            var gathMult = luckInfo?.GetStatLevelByName("Gather") ?? 0;
            if (gathMult < 1) 
                return null;

            var needrnd = 6.25f * gathMult;

            var multiamountxp = 1.7f + (0.33f * (gathMult - 1));

            var newPoints = points;
            // check if farmer? then give extra rng/luck chance
            if (Random.Range(0, 101f) <= GetChanceScaledDouble(player.userID, needrnd, needrnd, 99))
            {
                newPoints = (float)Math.Round(newPoints * multiamountxp, MidpointRounding.AwayFromZero);

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    SendXPMultipliedNotification(player, "Luck", multiamountxp, newPoints);
                }
                finally { Pool.Free(ref sb); }



            }


            return newPoints != points ? newPoints : null;
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private void NoteItemByID(BasePlayer player, int itemID, int amount, string itemName = "", BaseEntity.GiveItemReason reason = BaseEntity.GiveItemReason.Generic)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                player.Command(sb.Clear().Append("note.inv ").Append(itemID).Append(" ").Append(amount).Append(" \"").Append(itemName).Append("\" ").Append((int)reason).ToString()); 
            } //if the amount is - it will be included automatically, so don't need to add it in the " " sb append
            finally { Pool.Free(ref sb); }
        
        }

        private void NoteInvByItem(BasePlayer player, Item item, bool negative = false)
        {
            NoteItemByID(player, item.info.itemid, negative ? -item.amount : item.amount, item.GetName());
        }

        private void NoteItemByDef(BasePlayer player, ItemDefinition def, int amount)
        {
            NoteItemByID(player, def.itemid, amount);
        }

        private void NoteItemByAmount(BasePlayer player, ItemAmount amount)
        {
            NoteItemByID(player, amount.itemDef.itemid, (int)amount.amount);
        }

        private bool IsFishDef(ItemDefinition def)
        {
            if (def == null) 
                throw new ArgumentNullException(nameof(def));

            return _fishItems.Contains(def);
        }

        private bool IsFish(Item item)
        {
            return IsFishDef(item?.info);
        }

        private readonly Dictionary<string, TimeCachedValue<bool>> _isMvpCache = new();


        private bool IsMVP(string userID, bool skipCache = false)
        {
            if (string.IsNullOrEmpty(userID))
                throw new ArgumentNullException(nameof(userID));

            if (!_isMvpCache.TryGetValue(userID, out _))
                _isMvpCache[userID] = new TimeCachedValue<bool>()
                {
                    refreshCooldown = 60f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<bool>(() =>
                    {
                        var perms = permission.GetUserPermissions(userID);
                        if (perms.Length < 1)
                            return false;

                        for (int i = 0; i < perms.Length; i++)
                            if (perms[i].Contains("mvp", CompareOptions.OrdinalIgnoreCase)) return true;

                        return false;
                    })
                };

           return _isMvpCache[userID].Get(skipCache);
        }

        private readonly Dictionary<string, TimeCachedValue<bool>> _isVipCache = new();

        private bool IsVIP(string userID, bool skipCache = false)
        {
            if (string.IsNullOrEmpty(userID))
                throw new ArgumentNullException(nameof(userID));


            if (!_isVipCache.TryGetValue(userID, out _))
                _isVipCache[userID] = new TimeCachedValue<bool>()
                {
                    refreshCooldown = 60f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<bool>(() =>
                    {
                        var perms = permission.GetUserPermissions(userID);
                        if (perms.Length < 1)
                            return false;

                        for (int i = 0; i < perms.Length; i++)
                            if (perms[i].Contains("vip", CompareOptions.OrdinalIgnoreCase)) return true;

                        return false;
                    })
                };


            return _isVipCache[userID].Get(skipCache);
        }

        private double getCatchupMultiplier(ulong userID)
        {
            if (userID == 0) return -1;
            var getLvl = getLevel(userID);
            if (getLvl < 1) return -1;
            var avgTop = Math.Round(getAverageTop5(userID), MidpointRounding.AwayFromZero);
            if (avgTop < 1) return -1;
            if (avgTop < 40) return 0;
            return (avgTop > getLvl) ? avgTop - getLvl + (10 * (avgTop - getLvl)) : 0;
        }

        private double getAverageTop5()
        {
            var top5 = GetTop5();
            if (top5 == null || top5.Count < 1) return -1;
            var sum = 0L;
            for (int i = 0; i < top5.Count; i++)
            {
                var topID = top5[i];
                var topLvl = getLevel(topID);
                if (topLvl > 0) sum += topLvl;
            }
            return sum / top5.Count;
        }

        private double GetAverageTop5S(params string[] ignoreIds)
        {
            var top5 = GetTop5S(ignoreIds);
            if (top5 == null || top5.Count < 1) return -1;
            var sum = 0L;
            for (int i = 0; i < top5.Count; i++)
            {
                var topID = top5[i];
                var topLvl = getLevelS(topID);
                if (topLvl > 0) sum += topLvl;
            }
            return sum / top5.Count;
        }

        private double getAverageTop5(params ulong[] ignoreIds)
        {
            var top5 = Pool.GetList<ulong>();
            try
            {
                GetTop5NoAlloc(ref top5, ignoreIds);

                if (top5?.Count < 1) return 0;

                var sum = 0L;
                for (int i = 0; i < top5.Count; i++)
                {
                    var topID = top5[i];
                    var topLvl = getLevel(topID);
                    if (topLvl > 0) sum += topLvl;
                }
                return sum / top5.Count;
            }
            finally { Pool.FreeList(ref top5); }


        }

        private int GetChanceScaledInteger(ulong userID, int number, int clampMin = -1, int clampMax = -1, float scalar = 1.0f, MidpointRounding rounding = MidpointRounding.AwayFromZero)
        {
            return (int)Math.Round(GetChanceScaledDouble(userID, number, clampMin, clampMax, scalar), rounding);
        }

        /*/
        int getScaledNumber(ulong userID, int number, int clampMin = -1, int clampMax = -1, float scalar = 1.0f, MidpointRounding rounding = MidpointRounding.AwayFromZero)
        {
            if (userID == 0) throw new ArgumentNullException();
            if (number == 0) return number;
            var chanceLvl = GetLuckInfo(userID)?.chanceLvl ?? 0;
            var bias = NoobBias?.Call<float>("GetNoobBiasTotal", userID) ?? 0f;
            var ply = FindPlayerByID(userID);
            var charmChance = 0f;
            if (ply != null) charmChance = GetCharmChances(ply);

            if (chanceLvl < 1 && charmChance <= 0.0f && bias <= 0.0f) return number;
            var chanceMult = (long)Math.Round((chanceLvl + charmChance) * scalar, MidpointRounding.AwayFromZero);
            var val = chanceMult < 1 ? number : (int)Math.Round((double)(number + ((number * chanceMult) / 100)), MidpointRounding.AwayFromZero);
            if (bias > 0.0f) val = (int)Math.Round(val * (1 + (bias / 4)), MidpointRounding.AwayFromZero);
            return (clampMin != -1 && clampMax != -1) ? val = Mathf.Clamp(val, clampMin, clampMax) : val;
        }/*/

        private double GetChanceScaledDouble(ulong userID, double number, double clampMin = -1, double clampMax = -1, float scalar = 1.0f)
        {
            if (userID == 0) throw new ArgumentOutOfRangeException(nameof(userID));
            if (number == 0) return number;

            var chanceBonuses = GetChanceBonuses(userID.ToString());

            var chanceMult = (long)Math.Round(chanceBonuses * scalar, MidpointRounding.AwayFromZero);


            var val = chanceMult < 1 ? number : (number + (number * chanceMult / 100));

            if (clampMax != -1 && val > clampMax) val = clampMax;
            if (clampMin != -1 && val < clampMin) val = clampMin;

            return val;
        }

        private double GetScaledScrapNumber(ulong userID, double number, double clampMin = -1, double clampMax = -1, float scalar = 1.0f)
        {
            if (userID == 0) throw new ArgumentOutOfRangeException(nameof(userID));
            if (number == 0) return number;

            var player = FindPlayerByID(userID);

            var val = number;

            var teamMembers = player?.Team?.members;

            if (teamMembers == null || teamMembers.Count < 1)
            {
                val *= Random.Range(1.59f, 2f); //solo buff!

                if (player != null && player?.blueprints != null)
                {
                    var bpItems = Pool.GetList<ItemDefinition>();
                    var allItems = player?.inventory?.AllItems() ?? null;
                    if (allItems != null && allItems.Length > 0)
                    {
                        for (int i = 0; i < allItems.Length; i++)
                        {
                            var item = allItems[i];
                            if (item?.info != null && item.IsBlueprint()) bpItems.Add(item.info);
                        }
                    }

                    var unlockedItems = new List<ItemDefinition>();
                    for (int i = 0; i < ItemManager.itemList.Count; i++)
                    {
                        var info = ItemManager.itemList[i];
                        if (info != null && info?.Blueprint != null && info.Blueprint.isResearchable && !info.Blueprint.defaultBlueprint && (player.blueprints.IsUnlocked(info) || bpItems.Contains(info)))
                        {
                            unlockedItems.Add(info);
                        }
                    }
                    if (unlockedItems == null || unlockedItems.Count < 1)
                    {
                        PrintWarning("no BPs for: " + userID);
                        val *= Random.Range(2.2f, 2.7f);
                    }
                    else
                    {
                        val -= Mathf.Clamp(unlockedItems.Count / 3.5f, 1f, 100f);
                        for (int j = 0; j < unlockedItems.Count; j++)
                        {
                            var item = unlockedItems[j];
                            if (item == null) continue;
                            var rarity = item.rarity;
                            var rarityDivide = rarity == Rarity.None ? 1f : rarity == Rarity.Common ? 1.009f : rarity == Rarity.Uncommon ? 1.012f : rarity == Rarity.Rare ? 1.017f : rarity == Rarity.VeryRare ? 1.0385f : 1f;
                            if (rarityDivide <= 1f) continue;
                            val = Mathf.Clamp((float)(val / rarityDivide), 1f, (float)number);
                        }
                    }
                    Pool.FreeList(ref bpItems);
                }
            }
            else
            {
                val = Mathf.Clamp((float)(val - (teamMembers.Count - 1)), 1, (float)number); //reduce chance by number of clan members (excluding self)
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    var memberId = teamMembers[i];
                    if (memberId == 0) continue;

                    var member = FindPlayerByID(memberId);
                    if (member?.blueprints == null) continue;

                    var unlockedItems = new List<ItemDefinition>();
                    for (int j = 0; j < ItemManager.itemList.Count; j++)
                    {
                        var info = ItemManager.itemList[j];
                        if (info != null && info?.Blueprint != null && info.Blueprint.isResearchable && !info.Blueprint.defaultBlueprint && member.blueprints.IsUnlocked(info))
                        {
                            unlockedItems.Add(info);
                        }
                    }
                    if (unlockedItems == null || unlockedItems.Count < 1) continue;

                    for (int j = 0; j < unlockedItems.Count; j++)
                    {
                        var item = unlockedItems[j];
                        if (item == null) continue;
                        var rarity = item.rarity;
                        var rarityDivide = rarity == Rarity.None ? 1f : rarity == Rarity.Common ? 1.005f : rarity == Rarity.Uncommon ? 1.008f : rarity == Rarity.Rare ? 1.012f : rarity == Rarity.VeryRare ? 1.0185f : 1f;
                        if (rarityDivide == 1f) continue;
                        //   PrintWarning("rarityDivide: " + rarityDivide + ", val pre: " + val + " item: " + item.shortname + " (" + rarity + ")");
                        val = Mathf.Clamp((float)(val / rarityDivide), 1f, (float)number);
                        //       PrintWarning("val after divide: " + val + " item: " + item.shortname + " (" + rarity + ")");
                    }
                }
            }
            if (clampMax != -1 && val > clampMax) val = clampMax;
            if (clampMin != -1 && val < clampMin) val = clampMin;
            return val;
        }

        private readonly Dictionary<string, TimeCachedValue<double>> _playerXpMultiplier = new();
        private readonly Dictionary<string, TimeCachedValue<double>> _playerItemMultiplier = new();

        private double GetXPMultiplier(string userID, bool skipCache = false)
        {
            if (string.IsNullOrWhiteSpace(userID))
                throw new ArgumentNullException(nameof(userID));

            if (!_playerXpMultiplier.TryGetValue(userID, out _))
                _playerXpMultiplier[userID] = new TimeCachedValue<double>()
                {
                    refreshCooldown = 10f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<double>(() =>
                    {
                        if (!ulong.TryParse(userID, out _))
                            return 100;

                        var baseMult = 100d;

                        var bonusXPPerc = IsVIP(userID) ? 30d : IsMVP(userID) ? 15d : 0d;

                        if (permission.UserHasPermission(userID, "zlevelsremastered.2xxp")) 
                            bonusXPPerc += 100;

                        var userClass = GetUserClassType(userID);
                        if (userClass == ClassType.Nomad)
                            bonusXPPerc += 200;

                        var xpMod = GetTotalXPModifier(userID, "LK");
                        if (xpMod == 0)
                            xpMod = 1;

                        if (IsRadStormActive())
                            bonusXPPerc += 200;

                        return (baseMult + bonusXPPerc) * xpMod;
                    })
                };

           return _playerXpMultiplier[userID].Get(skipCache);
        }

        private double GetItemMultiplier(string userID, bool skipCache = false)
        {
            if (string.IsNullOrEmpty(userID))
                return 100;


            if (!_playerItemMultiplier.TryGetValue(userID, out _))
                _playerItemMultiplier[userID] = new TimeCachedValue<double>()
                {
                    refreshCooldown = 10f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<double>(() =>
                    {
                        var baseMult = 100d;

                        var bonusMult = IsVIP(userID) ? 25d : IsMVP(userID) ? 10d : 0d;

                        bonusMult += GetUserClassType(userID) == ClassType.Loner ? 66d : 0d;

                        if (IsRadStormActive())
                            bonusMult += 50d;

                        //somehow this below gave 0 every time?!:
                        //var bonusMult = (IsVIP(userID) ? 25 : IsMVP(userID) ? 10 : 0) + GetUserClassType(userID) == ClassType.Loner ? 66 : 0;

                        PrintWarning(nameof(GetItemMultiplier) + " : " + userID + " bonusMult: " + bonusMult + ", baseMult: " + baseMult + ", isVIP: " + IsVIP(userID));

                        return baseMult + bonusMult;
                    })
                };

            return _playerItemMultiplier[userID].Get(skipCache);
        }

        private void SetAndHandlePoints(string userID, float points)
        {
            if (string.IsNullOrWhiteSpace(userID))
                throw new ArgumentNullException(nameof(userID));



            var info = GetLuckInfo(userID);
            if (info == null)
            {
                PrintWarning("HandlePoints called on userId that has no LuckInfo! userId: " + userID);
                return;
            }

            var player = FindPlayerByIdS(userID);
            if (player == null || !player.IsConnected)
            {
                PrintWarning("HandlePoints called with null player or disconnected player!");
                return;
            }

            var level = info.Level;
            var oldLevel = level;

            if (points >= GetPointsForLevel(level + 1)) //lvl up!
            {

                level = GetLevelFromPoints(points);

                var sb = Pool.Get<StringBuilder>();
                try
                {

                    var lvlUpStr = sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">").Append(messages["LevelUpText"]).Append("</color>").Replace("{0}", (string)messages["LKSkill"]).Replace("{1}", level.ToString()).Replace("{2}", points.ToString("N0")).Replace("{3}", GetPointsForLevel(level + 1).ToString("N0")).Replace("{4}", ((GetGatherMultiplier(level) - 1) * 100).ToString("0.##")).ToString();

                    SendReply(player, lvlUpStr);

                    var addPoints = Mathf.Clamp(level - oldLevel, 0, int.MaxValue);
                    info.SkillPoints += (int)addPoints;

                    var skillPtTip = sb.Clear().Append(LUCK_COLOR_FORMATTED).Append("+<color=#ff912b>").Append(addPoints.ToString("N0")).Append("</color> Luck Skill point").Append(addPoints > 1 ? "s" : string.Empty).Append("!\nType </color><color=#ff912b>/skt</color><color=#5DC540> to use your skill point!\nTotal Skill Points: </color><color=#ff912b>").Append(info.SkillPoints.ToString("N0")).Append("</color>").ToString();

                    SendLuckNotifications(player, skillPtTip, popupDuration: 8.25f);

                    SendLocalEffect(player, "assets/prefabs/misc/xmas/advent_calendar/effects/open_advent.prefab");
                }
                finally { Pool.Free(ref sb); }

            }

            SetPointsAndLevel(userID, points, level);
        }

        private void HandleLuckItem(BasePlayer player, ItemAmount item, float xpScalar = 1f)
        {
            if (player == null || item == null) return;

            var canEarn = Interface.CallHook("CanHandleLuckItem", player, item);
            var canEarnBool = true;
            if (TryParseBool(canEarn, ref canEarnBool) && !canEarnBool) return;

            long Level = getLevel(player.userID);

            var finalItemAmount = (int)Math.Round(item.amount * GetGatherMultiplier(Level), MidpointRounding.AwayFromZero);

            var pointsToGet = 0f;
            if (!TryParseFloat(pointsPerHit[LUCK_SHORTNAME], ref pointsToGet))
            {
                PrintWarning("Failed to parse: " + pointsPerHit[LUCK_SHORTNAME] + " as float for player: " + player.displayName + " with item: " + (item?.itemDef?.shortname ?? "Unknown"));
                return;
            }


            if (IsVIP(player.UserIDString)) finalItemAmount += finalItemAmount * 20 / 100;
            else if (IsMVP(player.UserIDString)) finalItemAmount += finalItemAmount * 10 / 100;

            var charmItem = GetCharmGathers(player, CharmSkill.Luck);

            if (charmItem < 0.0f) finalItemAmount = (int)Math.Round(finalItemAmount - (finalItemAmount * Math.Abs(charmItem) / 100), MidpointRounding.AwayFromZero);
            else if (charmItem > 0.0f) finalItemAmount = (int)Math.Round(finalItemAmount + (finalItemAmount * charmItem / 100), MidpointRounding.AwayFromZero);

            pointsToGet = DoCatchup(player.userID, DoMultipliers(player.userID, pointsToGet));


            item.amount = finalItemAmount;
            var onEarn = Interface.CallHook(nameof(OnEarnLuckXP), player, item, pointsToGet);

            if (onEarn != null && !TryParseFloat(onEarn, ref pointsToGet)) PrintWarning("Failed to parse float!!! onEarn to pointsToGet");

            AddXP(player.UserIDString, pointsToGet);
        }
        #endregion
        #region Utility

        private bool IsRadStormActive() => (RadRain?.Call<bool>("IsRadStormActive") ?? false);


        public bool IsDivisble(int x, int n) { return (x % n) == 0; }

        private ulong GetMajorityOwnerFromBuilding(BuildingManager.Building building)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                var ownedBlockCounts = Pool.Get<Dictionary<ulong, int>>();

                try
                {
                    ownedBlockCounts.Clear();


                    int count;
                    foreach (var block in building.buildingBlocks)
                    {
                        if (block == null || block.OwnerID == 0) continue;

                        if (!ownedBlockCounts.TryGetValue(block.OwnerID, out count)) ownedBlockCounts[block.OwnerID] = 1;
                        else ownedBlockCounts[block.OwnerID]++;
                    }

                    if (ownedBlockCounts.Count < 1) return 0;


                    var lastBlockCount = 0;
                    var lastUserId = 0UL;

                    foreach (var kvp in ownedBlockCounts)
                    {
                        if (kvp.Value > lastBlockCount)
                        {
                            lastBlockCount = kvp.Value;
                            lastUserId = kvp.Key;
                        }
                    }

                    return lastUserId;

                   

                }
                finally
                {
                    ownedBlockCounts?.Clear();
                    Pool.Free(ref ownedBlockCounts);
                }

            }
            finally
            {
                try
                {
                    if (watch.Elapsed.TotalMilliseconds > 1) PrintWarning(nameof(GetMajorityOwnerFromBuilding) + " took: " + watch.Elapsed.TotalMilliseconds + "ms");
                }
                finally { Pool.Free(ref watch); }
            }
        }
        private string GetOpeningColorTag(string hexColor)
        {
            var sb = Pool.Get<StringBuilder>();

            try { return sb.Clear().Append("<color=").Append(hexColor).Append(">").ToString(); }
            finally { Pool.Free(ref sb); }
        }

        //weirdga ass code for real bro
        private Vector3 GetGroundPosition(Vector3 pos, int maxTries = 20)
        {
            if (pos == Vector3.zero || FindGround == null || !FindGround.IsLoaded) return pos;
            var newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
            maxTries = maxTries < 1 ? 1 : maxTries > 100 ? 100 : maxTries;
            if (newPosObj == null)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    if (newPosObj != null) break;
                    newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
                }
                if (newPosObj == null)
                {
                    PrintWarning("null pos after " + maxTries.ToString("N0") + " tries");
                    return Vector3.zero;
                }
            }
            return (Vector3)newPosObj;
        }

        private Dictionary<string, GestureConfig> _stringToGesture = null;

        private GestureConfig StringToGesture(BasePlayer player, string gestureName)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(gestureName))
                throw new ArgumentNullException(nameof(gestureName));

            if (_stringToGesture == null)
            {
                _stringToGesture = new Dictionary<string, GestureConfig>();

                for (int i = 0; i < player.gestureList.AllGestures.Length; i++)
                {
                    var gesture = player.gestureList.AllGestures[i];
                    _stringToGesture[gesture.convarName] = gesture;
                }

            }

            if (!_stringToGesture.TryGetValue(gestureName, out GestureConfig config))
            {
                PrintWarning("failed to get from " + nameof(_stringToGesture) + ", looking for: " + gestureName);

                foreach (var kvp in _stringToGesture)
                {
                    if (kvp.Key.Equals(gestureName, StringComparison.OrdinalIgnoreCase))
                    {
                        config = kvp.Value;
                        break;
                    }
                }

                if (config == null)
                    PrintError("still failed to get. gesture name: " + gestureName);
                else
                {
                    PrintWarning("setting string to gesture. convarName: " + config.convarName + " for config: " + config);
                    _stringToGesture[config.convarName] = config;
                }

            }



            return config;

        }

        //thanks ChatGPT! (though I could have figured this out myself, it seems ChatGPT does the same thing as I would have, for the most part.)
        public int GetNumberOfDivisions(int dividend, int divisor)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException("Divisor cannot be zero.");
            }

            var count = 0;
            while (dividend >= divisor)
            {
                dividend -= divisor;
                count++;
            }

            return count;
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

        private bool IsDownOrJustPressedAnyMovementKey(InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return input.IsDown(BUTTON.FORWARD) || input.IsDown(BUTTON.BACKWARD) || input.IsDown(BUTTON.LEFT) || input.IsDown(BUTTON.RIGHT) || input.IsDown(BUTTON.JUMP) || input.WasJustPressed(BUTTON.FORWARD) || input.WasJustPressed(BUTTON.BACKWARD) || input.WasJustPressed(BUTTON.LEFT) || input.WasJustPressed(BUTTON.RIGHT) || input.WasJustPressed(BUTTON.JUMP);
        }

        private Item GetRandomComponent(int amount = 1, bool enforceStackLimits = true)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var components = Pool.GetList<ItemDefinition>();

            try
            {

                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];

                    if (item.category == ItemCategory.Component && !_ignoreItems.Contains(item.itemid) && !item.shortname.Contains("vehicle.") && item.shortname != "glue" && item.shortname != "bleach" && item.shortname != "ducttape" && item.shortname != "sticks")
                        components.Add(item);
                }

                var itemInfo = components[Random.Range(0, components.Count)];
                var component = ItemManager.Create(itemInfo, 1);

                if (enforceStackLimits)
                    component.amount = Mathf.Clamp(amount, 1, component.MaxStackable());

                return component;
            }
            finally { Pool.FreeList(ref components); }
        }

        private static readonly System.Random _categoryRng = new();

        private ItemCategory GetRandomCategory() { return (ItemCategory)_categoryRng.Next(0, 18); }

        private ItemCategory GetRandomCategory(params ItemCategory[] ignoreCategories)
        {
            var category = (ItemCategory)_categoryRng.Next(0, 18);
            if (!ignoreCategories.Any(p => p == category)) return category;

            var max = 400;
            var count = 0;

            while (ignoreCategories.Any(p => p == category))
            {
                count++;
                if (count >= max) break;
                category = (ItemCategory)_categoryRng.Next(0, 18);
            }

            return category;
        }

        private Rust.Rarity GetRandomRarity(ItemCategory category, bool onlyResearchable = false, params Rust.Rarity[] ignoreRarity)
        {
            var rarity = Rarity.None;
            var rarities = Pool.GetList<Rust.Rarity>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item == null) continue;

                    if (item.category == category)
                    {
                        if (rarities.Contains(item.rarity)) continue;
                        if (!onlyResearchable || ((item?.Blueprint?.isResearchable ?? false) && !(item?.Blueprint?.defaultBlueprint ?? false))) if (ignoreRarity == null || ignoreRarity.Length < 1 || !ignoreRarity.Any(p => p == item.rarity)) rarities.Add(item.rarity);
                    }
                }

                if (rarities != null && rarities.Count > 0)
                {
                    var rngRarity = rarities[_rarityRng.Next(0, rarities.Count)];
                    //   PrintWarning("rng rarity: " + rngRarity + ", has: " + HasRarity(rngRarity, category, true, true));
                    rarity = rngRarity;
                }

                return rarity;
            }
            finally { Pool.FreeList(ref rarities); }
        }

        private Rust.Rarity GetRandomRarityOrDefault(ItemCategory category, bool onlyResearchable = false, params Rust.Rarity[] ignoreRarity)
        {
            var rarity = Rarity.None;
            var rarities = Pool.GetList<Rust.Rarity>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item == null) continue;

                    if (item.category == category)
                    {
                        if (rarities.Contains(item.rarity)) continue;
                        if (!onlyResearchable || ((item?.Blueprint?.isResearchable ?? false) && !(item?.Blueprint?.defaultBlueprint ?? false))) if (ignoreRarity == null || ignoreRarity.Length < 1 || !ignoreRarity.Any(p => p == item.rarity)) rarities.Add(item.rarity);
                    }
                }
                if (rarities.Count > 0)
                {
                    var rngRarity = rarities[_rarityRng.Next(0, rarities.Count)];
                    //   PrintWarning("rng rarity: " + rngRarity + ", has: " + HasRarity(rngRarity, category, true, true));
                    rarity = rngRarity;
                }
                else
                {
                    var itemCats = Pool.GetList<ItemDefinition>();
                    for (int i = 0; i < ItemManager.itemList.Count; i++)
                    {
                        var item = ItemManager.itemList[i];
                        if (item.category == category) itemCats.Add(item);
                    }
                    if (itemCats.Count > 0)
                    {
                        rarity = itemCats?.Select(p => p.rarity)?.Min() ?? Rarity.None;
                    }

                    Pool.FreeList(ref itemCats);
                }

                return rarity;
            }
            finally { Pool.FreeList(ref rarities); }
        }

        private Item GetRandomItem(Rust.Rarity rarity, ItemCategory category = ItemCategory.Items, bool craftableOnly = true, bool researchableOnly = true)
        {
            Item item = null;

            var applicableItems = Pool.GetList<ItemDefinition>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var itemDefs = ItemManager.itemList[i];
                    if (itemDefs == null) continue;

                    if (_ignoreItems.Contains(itemDefs.itemid) || itemDefs.GetComponent<Rust.Modular.ItemModVehicleChassis>() != null)
                        continue;


                    applicableItems.Add(itemDefs);

                }

                var itemDef = applicableItems?.Where(p => p != null && p.rarity == rarity && p.category == category && (!craftableOnly || craftableOnly && (p?.Blueprint?.userCraftable ?? false)) && (!researchableOnly || (p?.Blueprint?.isResearchable ?? false) || (p?.Blueprint?.defaultBlueprint ?? false)))?.ToList()?.GetRandom() ?? null;
                if (itemDef != null) item = ItemManager.Create(itemDef, 1);

                return item;
            }
            finally { Pool.FreeList(ref applicableItems); }
        }

        private Item GetRandomBP(Rust.Rarity rarity, ItemCategory category = ItemCategory.Items)
        {
            var item = ItemManager.CreateByName("blueprintbase");

            item.blueprintTarget = ItemManager.itemList?.Where(p => p != null && p.rarity == rarity && p.category == category && !(p?.Blueprint?.defaultBlueprint ?? false) && (p?.Blueprint?.userCraftable ?? false) && (p?.Blueprint?.isResearchable ?? false))?.ToList()?.GetRandom()?.itemid ?? 0;

            if (item.blueprintTarget == 0)
            {
                PrintWarning("item.blueprint target 0!!!!");
                RemoveFromWorld(item);
                return null;
            }

            return item.blueprintTarget == 0 ? null : item;
        }

        private static readonly System.Random bpRng = new();

        private Item GetUnlearnedBP(Rust.Rarity rarity, BasePlayer player, ItemCategory category = ItemCategory.Items)
        {
            if (player == null || player.blueprints == null) return null;

            var list = ItemManager.itemList?.Where(p => p.rarity == rarity && p.category == category && !(p?.Blueprint?.defaultBlueprint ?? false) && (p?.Blueprint?.isResearchable ?? false) && (p?.Blueprint?.userCraftable ?? false) && !(player?.blueprints?.HasUnlocked(p) ?? false)) ?? null;
            if (list == null || !list.Any())
            {
                return null;
            }

            var item = ItemManager.CreateByName("blueprintbase");
            item.blueprintTarget = 0;



            var listCount = list?.Count() ?? 0;
            item.blueprintTarget = list.ElementAtOrDefault(bpRng.Next(0, listCount))?.itemid ?? 0;


            if (item.blueprintTarget == 0)
            {
                PrintWarning("item.blueprint target 0!!!!");
                RemoveFromWorld(item);
                return null;
            }

            return item.blueprintTarget == 0 ? null : item;
        }

        private bool HasRarity(Rust.Rarity rarity, ItemCategory category, bool onlyResearchable = false, bool ignoreDefault = false)
        {
            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                if (item == null) continue;
                if (item.category == category && item.rarity == rarity)
                {
                    if (!onlyResearchable || (item?.Blueprint?.isResearchable ?? false)) if (!ignoreDefault || !(item?.Blueprint?.defaultBlueprint ?? false)) return true;
                }
            }
            return false;
        }

        private Rust.Rarity GetHighestRarity(ItemCategory category, bool onlyResearchable = false)
        {
            try
            {
                if (onlyResearchable && !(ItemManager.itemList?.Any(p => p != null && p.category == category && (p?.Blueprint?.isResearchable ?? false)) ?? false)) return Rarity.None;
                else if (!onlyResearchable) if (!(ItemManager.itemList?.Any(p => p != null && p.category == category) ?? false)) return Rarity.None;
                return (!onlyResearchable ? ItemManager.itemList?.Where(p => p.category == category) : ItemManager.itemList?.Where(p => p.category == category && (p?.Blueprint?.isResearchable ?? false) && !(p?.Blueprint?.defaultBlueprint ?? false)))?.Select(p => p.rarity)?.Max() ?? Rarity.None;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintError("^GetHighestRarity^");
                return Rarity.None;
            }
        }

        private readonly Dictionary<string, Dictionary<string, float>> _lastCooldownMsgTime = new();

        private void SendCooldownMessage(BasePlayer player, string msg, float mustHaveWaited, string identifier = "")
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(msg))
                throw new ArgumentNullException(nameof(msg));

            if (mustHaveWaited < 0f)
                throw new ArgumentOutOfRangeException(nameof(mustHaveWaited));

            if (!player.IsConnected)
                return;

            var now = Time.realtimeSinceStartup;

            var findVal = !string.IsNullOrWhiteSpace(identifier) ? identifier : msg;



            if (!_lastCooldownMsgTime.TryGetValue(player.UserIDString, out Dictionary<string, float> lastTimeDictionary))
                _lastCooldownMsgTime[player.UserIDString] = lastTimeDictionary = new Dictionary<string, float>();

            if (lastTimeDictionary.TryGetValue(findVal, out float lastTime) && (now - lastTime) < mustHaveWaited)
                return;


            lastTimeDictionary[findVal] = now;

            _lastCooldownMsgTime[player.UserIDString] = lastTimeDictionary;

            SendReply(player, msg);

        }
        public void OnRepairFailedResources(BasePlayer player, List<ItemAmount> requirements)
        {
            using ProtoBuf.ItemAmountList itemAmountList = ItemAmount.SerialiseList(requirements);
            player.ClientRPCPlayer(null, player, "Client_OnRepairFailedResources", itemAmountList);
        }

        private void DestroyClientEntity(BasePlayer player, BaseNetworkable entity)
        {
            if (player == null || !player.IsConnected || player?.net == null || player?.net?.connection == null || entity == null || entity?.net == null || entity.IsDestroyed) return;

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.EntityDestroy);
            netWrite.EntityID(entity.net.ID);
            netWrite.UInt8((byte)BaseNetworkable.DestroyMode.None);
            netWrite.Send(new SendInfo(player.net.connection)
            {
                priority = Priority.Immediate
            });
        }

        private void ShowToast(BasePlayer player, string message, int type = 0)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.IsConnected)
                return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                player.SendConsoleCommand(sb.Clear().Append("gametip.showtoast ").Append(type).Append(" \"").Append(message).Append("\"").ToString());
            }
            finally { Pool.Free(ref sb); }
        }

        private readonly HashSet<NetworkableId> immuneEntities = new();
        private readonly Dictionary<NetworkableId, Timer> immuneTimers = new();

        private readonly HashSet<NetworkableId> _npcImmuneEntities = new();

        private void GrantImmunity(BaseEntity entity, float duration = -1f)
        {
            if (entity != null && entity?.net != null && !entity.IsDestroyed) GrantImmunity(entity.net.ID, duration);
        }

        private void GrantImmunity(NetworkableId netID, float duration = -1f)
        {
            if (!immuneEntities.Add(netID))
            {
                PrintWarning("!immuneEntities.Add: " + netID);
                return;
            }

            if (duration > 0f)
            {
                Timer newTimer;
                newTimer = timer.Once(duration, () =>
                {
                    if (immuneTimers.TryGetValue(netID, out Timer getTimer)) immuneTimers.Remove(netID);
                    immuneEntities.Remove(netID);
                });
                immuneTimers[netID] = newTimer;
            }
        }

        private void RemoveImmunity(NetworkableId netID)
        {
            immuneEntities.Remove(netID);

            if (immuneTimers.TryGetValue(netID, out Timer getTimer))
            {
                getTimer.Destroy();
                immuneTimers.Remove(netID);
            }
        }

        private void GrantMasteryAchievement(BasePlayer player, StatInfo skill)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (skill == null)
                throw new ArgumentNullException(nameof(skill));

            if (!(PRISMAchievements?.IsLoaded ?? false))
            {
                PrintWarning(nameof(GrantMasteryAchievement) + " called but PA is null!!");
                return;
            }

            var sb = Pool.Get<StringBuilder>();
            try
            {
                PRISMAchievements.Call("GiveAchievementByName", player, sb.Clear().Append(Name).Append("_").Append(skill.ShortName).Append("_mastery").ToString());
            }
            finally { Pool.Free(ref sb); }
        }

        private ulong GetRandomSkinID(string itemidStr)
        {
            if (string.IsNullOrEmpty(itemidStr)) return 0;
            return SkinsAPI?.Call<ulong>("GetRandomSkinID", itemidStr) ?? 0;
        }

        private void CancelDamage(HitInfo hitinfo)
        {
            if (hitinfo == null) return;
            hitinfo.damageTypes.Clear();
            hitinfo.PointStart = Vector3.zero;
            hitinfo.HitEntity = null;
            hitinfo.DoHitEffects = false;
            hitinfo.HitMaterial = 0;
        }

        private void BroadcastClassSelection(BasePlayer player, ARPGClass userClass)
        {
            if (player == null) 
                throw new ArgumentNullException(nameof(player));

            if (userClass == null)
                throw new ArgumentNullException(nameof(userClass));

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                var str = sb.Clear().Append("<color=").Append(userClass.SecondaryColor).Append(">").Append(player.displayName).Append("</color> has selected the <size=22><i><color=").Append(userClass.PrimaryColor).Append(">").Append(userClass.DisplayName).Append("</color></i></size> class!").ToString();

                SimpleBroadcast(str, CHAT_STEAM_ID);

                var cleanStr = RemoveTags(str);

                DiscordAPI2?.Call("SendChatMessageByName", cleanStr, "PRISM | BLUE", PRISM_BLUE_ICON_DISCORD); //must supply a URL image link at end, not ID

            }
            finally { Pool.Free(ref sb); }

        }

        private void SimpleBroadcast(string msg, string userId = "")
        {
            if (string.IsNullOrEmpty(msg)) return;

            var chatEntry = new ConVar.Chat.ChatEntry()
            {
                Channel = ConVar.Chat.ChatChannel.Server,
                Message = RemoveTags(msg),
                UserId = userId,
                Time = Facepunch.Math.Epoch.Current
            };

            RCon.Broadcast(RCon.LogType.Chat, chatEntry);

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || !player.IsConnected) continue;

                if (!string.IsNullOrWhiteSpace(userId)) player.SendConsoleCommand("chat.add", string.Empty, userId, msg);
                else SendReply(player, msg);
            }
        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp))
                throw new ArgumentNullException(nameof(nameOrIdOrIp));

            var ply = covalence.Players.FindPlayerById(nameOrIdOrIp);
            if (ply != null) if ((!ply.IsConnected && tryFindOfflineIfNoOnline) || ply.IsConnected) return ply;

            IPlayer player = null;
            foreach (var p in covalence.Players.Connected)
            {

                if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Address == nameOrIdOrIp || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (player != null) return null;
                    else player = p;
                }
            }

            if (tryFindOfflineIfNoOnline && player == null)
            {
                foreach (var p in covalence.Players.All)
                {
                    if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (player != null) return null;
                        else player = p;
                    }
                }
            }

            return player;
        }

        private bool IsNight() { return TOD_Sky.Instance.Cycle.Hour > TOD_Sky.Instance.SunsetTime || TOD_Sky.Instance.Cycle.Hour < TOD_Sky.Instance.SunriseTime; }

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

        private BaseCombatEntity GetOverdriveTarget(BasePlayer player, float maxDist = 90f, float radius = 1.25f, int layerMask = -1)
        {
            var currentRot = player.eyes.rotation * Vector3.forward;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);

            var hits = Physics.SphereCastAll(ray, radius, maxDist, layerMask);

            if (hits == null || hits.Length < 1)
                return null;
            
           
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];

                var hitEnt = hit.GetEntity();

                if (hitEnt == null || hitEnt.IsDestroyed || hitEnt == player)
                    continue;
                

                var combatEnt = hitEnt as BaseCombatEntity;
                if (combatEnt == null)
                    continue;
                

                var ply = combatEnt as BasePlayer;
                if (ply != null && (ply.IsConnected || ply.IsSleeping()))
                    continue;
                

                return combatEnt;
            }


            return null;
        }

        private void EnsureCharmComponent(BasePlayer player)
        {
            if (player != null && player.gameObject != null && !player.IsDestroyed && player.IsConnected)
            {
                if (player?.GetComponent<PlayerCharmModifiers>() == null)
                    player.gameObject.AddComponent<PlayerCharmModifiers>();

            }
        }

        private void RemoveCharmComponent(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.gameObject != null && !player.IsDestroyed)
                player?.GetComponent<PlayerCharmModifiers>()?.DoDestroy();
            
        }

        private bool ShouldHaveCharmComponent(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || !player.IsConnected) return false;

            var charms = GetPlayerCharms(player);
            if (charms == null || charms.Count < 1) return false;


            for (int i = 0; i < charms.Count; i++)
            {
                var charm = charms[i];
                if (charm.itemBonus > 0 && (charm.Skill == CharmSkill.Woodcutting || charm.Skill == CharmSkill.Survival || charm.Skill == CharmSkill.Mining)) return true;
            }

            return false;
        }

        private readonly Dictionary<string, Dictionary<string, HashSet<XPModifier>>> _xpMods = new();

        private HashSet<XPModifier> GetXPModifiers(string userId, string skill)
        {
            if (_xpMods.TryGetValue(userId, out var kvp) && kvp.TryGetValue(skill, out var mod))
                return mod;
            

            return null;
        }

        private void AddXPModifier(string userId, string skill, XPModifier mod)
        {
            if (!_xpMods.TryGetValue(userId, out _))
                _xpMods[userId] = new Dictionary<string, HashSet<XPModifier>>();

            if (!_xpMods[userId].TryGetValue(skill, out _))
                _xpMods[userId][skill] = new HashSet<XPModifier>();

            _xpMods[userId][skill].Add(mod);
        }
        
        private double GetTotalXPModifier(string userId, string skill)
        {
            var mods = GetXPModifiers(userId, skill);
            if (mods == null || mods.Count < 1)
                return 0.0d;

            var sum = 0.0d;

            var now = Time.realtimeSinceStartup;

            foreach (var mod in mods)
            {
                if (mod.ExpirationTime > now)
                    sum += mod.Multiplier;
            }

            return sum;
        }

        private struct XPModifier
        {
            public double Multiplier;
            public float ExpirationTime;
            public string Reason;
            public int ID { get; private set; } = Random.Range(0, 9999);

            public XPModifier()
            {

            }

        }

        //MASTAPIECE)))))))))
        private class PlayerModifiers : FacepunchBehaviour
        {
            public BasePlayer Player { get; set; }

            public HashSet<Modifier> CustomModifiers
            {
                get;
                private set;
            } = new HashSet<Modifier>();

            private readonly object[] _modObjects = new object[1];

            //can you believe this is STILL private (may, 2023)?
            private readonly MethodInfo _removeModifier = typeof(BaseModifiers<BasePlayer>).GetMethod("Remove", BindingFlags.Instance | BindingFlags.NonPublic);

            public virtual void Awake()
            {
                Interface.Oxide.LogWarning(nameof(PlayerModifiers) + "." + nameof(Awake));

                Player = GetComponent<BasePlayer>();
                if (Player == null || Player.gameObject == null || Player.IsDestroyed)
                {
                    Interface.Oxide.LogError(nameof(PlayerCharmModifiers) + "." + nameof(Awake) + " called with null BasePlayer!");
                    DoDestroy();
                    return;
                }

                Interface.Oxide.LogWarning(nameof(PlayerModifiers) + "." + nameof(Awake) + " completed for: " + Player);

            }

            //we should avoid calling this regularly since we want this to be usable by multiple different things (i.e not just charms)
            public void ClearAllModifiers()
            {

                Interface.Oxide.LogWarning(nameof(PlayerModifiers) + "." + nameof(ClearAllModifiers) + " for " + Player);

                var modifiersAsBase = Player?.modifiers as BaseModifiers<BasePlayer>;
                if (modifiersAsBase == null)
                {
                    Interface.Oxide.LogError("BaseModifiers<BasePlayer> is null!!");
                    return;
                }

                foreach (var mod in CustomModifiers)
                {
                    _modObjects[0] = mod;
                   _removeModifier.Invoke(modifiersAsBase, _modObjects);
                }

                CustomModifiers.Clear();
            }

            public void ClearModifiers(ICollection<Modifier> mods)
            {
                foreach (var mod in mods)
                    RemoveModifier(mod);
            }

            public Modifier AddModifier(ModifierDefintion modDef)
            {
                if (modDef == null)
                    throw new ArgumentNullException(nameof(modDef));

                if (Player?.modifiers == null)
                {
                    Interface.Oxide.LogError("Player modifiers null!!!");
                    return null;
                }

                var newModifiers = Pool.GetList<ModifierDefintion>();

                try 
                {
                    newModifiers.Add(modDef);

                    Player.modifiers.Add(newModifiers);

                    var allModifiers = Player.modifiers.All;

                    for (int i = 0; i < allModifiers.Count; i++)
                    {
                        var mod = allModifiers[i];

                        if (mod == null)
                        {
                            Interface.Oxide.LogError("Found null mod?!?!");
                            continue;
                        }

                        for (int j = 0; j < newModifiers.Count; j++)
                        {
                            var def = newModifiers[j];

                            if (def.value == mod.Value && def.duration == mod.Duration && def.source == mod.Source && def.type == mod.Type)
                            {

                                CustomModifiers.Add(mod);
                                return mod;
                            }
                        }
                    }
                }
                finally { Pool.FreeList(ref newModifiers); }

               

                Interface.Oxide.LogError(nameof(PlayerModifiers) + " this shouldn't happen!! having to return null on " + nameof(AddModifier));

                return null;
            }

            public bool RemoveModifier(Modifier mod)
            {
                var modifiersAsBase = Player?.modifiers as BaseModifiers<BasePlayer>;

                _modObjects[0] = mod ?? throw new ArgumentNullException(nameof(mod));
                _removeModifier.Invoke(modifiersAsBase, _modObjects);

                return CustomModifiers.Remove(mod);
            }

            public virtual void DoDestroy()
            {
                try
                {
                    // if (Player != null && Player.gameObject != null && !Player.IsDestroyed) CancelInvoke(UpdateModifiers);

                    Interface.Oxide.LogWarning(nameof(PlayerModifiers) + "." + nameof(DoDestroy) + " called, clear all modifiers");
                    ClearAllModifiers();
                }
                finally { Destroy(this); }
            }

        }

        //note to past self: comment your fucking code
        private class PlayerCharmModifiers : PlayerModifiers
        {

            public HashSet<Modifier> CharmModifiers
            {
                get;
                private set;
            } = new HashSet<Modifier>();

            public override void Awake()
            {
                base.Awake();
                InvokeRepeating(UpdateCharmModifiers, 0.1f, 5f);
            }

            public override void DoDestroy()
            {
                try
                {
                    Interface.Oxide.LogWarning(nameof(PlayerCharmModifiers) + "." + nameof(DoDestroy));

                    CancelInvoke(UpdateCharmModifiers);
                    CharmModifiers.Clear();
                }
                finally
                {
                    try { base.DoDestroy(); }
                    finally { Destroy(this); }
                }
               
           
            }

            public void UpdateCharmModifiers()
            {
                if (Instance == null || !Instance.IsLoaded)
                {
                    Interface.Oxide.LogError("Instance is null on: " + nameof(UpdateCharmModifiers));
                    DoDestroy();
                    return;
                }

                var watch = Pool.Get<Stopwatch>();
                try
                {
                    watch.Restart();
                    if (Player == null || !Player.IsConnected)
                    {
                        DoDestroy();
                        return;
                    }

                    if (Player.gameObject == null || Player.IsDestroyed || Player.IsDead()) return;

                    //in the future, i suppose we could find existing modifiers, and if they're the exact modifier we need, just update the time instead of clearing and setting over and over again (optimization)

                    ClearModifiers(CharmModifiers); //removes the bonuses from player
                    CharmModifiers.Clear(); //clears actual hashset

                    var charms = Instance?.GetPlayerCharms(Player);

                    if (charms == null || charms.Count < 1)
                        return;

                    var modDef = Pool.Get<ModifierDefintion>();

                    try
                    {
                        for (int i = 0; i < charms.Count; i++)
                        {
                            var charm = charms[i];
                            if (charm == null || charm.itemBonus <= 0) continue;

                            var isMining = charm.Skill == CharmSkill.Mining;
                            var isWoodcutting = charm.Skill == CharmSkill.Woodcutting;
                            var isSurvival = charm.Skill == CharmSkill.Survival;

                            if (!isMining && !isWoodcutting && !isSurvival) continue;

                            modDef.source = (Modifier.ModifierSource)2; //invalid source to allow stacking
                            modDef.duration = 3599; //longest maximum time - changed from 3600 to 3599 as it seems at exactly 3600 it caused it to read "0s" in-game.
                            modDef.type = isMining ? Modifier.ModifierType.Ore_Yield : isWoodcutting ? Modifier.ModifierType.Wood_Yield : Modifier.ModifierType.Scrap_Yield;
                            modDef.value = (float)Math.Round(charm.itemBonus, MidpointRounding.AwayFromZero) * 0.01f + Random.Range(0.00001f, 0.005f); //rng is a hacky bug fix for duplicating charms? if same value and same type, they'd add each other infinitely...? revisit and try to fix properly

                            //rounding is what "caused" the issue, so we just unrounded by using rng omggg that's dmb pls

                            CharmModifiers.Add(AddModifier(modDef));
                        }
                    }
                    finally { Pool.Free(ref modDef); }


                }
                finally
                {
                    try { if (watch.Elapsed.TotalMilliseconds > 1) Interface.Oxide.LogWarning("updatecharm took: " + watch.Elapsed.TotalMilliseconds + "ms"); }
                    finally { Pool.Free(ref watch); }
                }
            }
        }

        private class LegendaryItemData
        {
            //this is the chance that we'll try to spawn any legendary at all
            public float BaseLegendarySpawnChance { get; set; } = 0.0195f;

            public List<LegendaryItemInfo> Infos { get; set; } = new List<LegendaryItemInfo>();

            [JsonIgnore]
            private TimeCachedValue<List<LegendaryItemInfo>> _cachedShuffled = null;


            [JsonIgnore]
            private List<LegendaryItemInfo> _shuffledInfos = null;


            public List<LegendaryItemInfo> GetShuffledInfos(bool forceSkipCache = false)
            {
                _cachedShuffled ??= new TimeCachedValue<List<LegendaryItemInfo>>
                {
                    refreshCooldown = 180f,
                    refreshRandomRange = 30f,
                    updateValue = new Func<List<LegendaryItemInfo>>(() =>
                    {
                        _shuffledInfos ??= new List<LegendaryItemInfo>();

                        _shuffledInfos.Clear();

                        for (int i = 0; i < Infos.Count; i++)
                            _shuffledInfos.Add(Infos[i]);

                        _shuffledInfos.Shuffle((uint)DateTime.UtcNow.Millisecond);

                        return _shuffledInfos;
                    })
                };

                return _cachedShuffled.Get(forceSkipCache || _shuffledInfos == null || _shuffledInfos.Count != Infos.Count);
            }
        }

        public class WeightedItemInfo
        {
            public float Weight { get; set; } = 0.125f;

            public int ItemID { get; set; }

            public ulong SkinID { get; set; } = 0;

            public WeightedItemInfo() { }
            public WeightedItemInfo(int itemId, float weight, ulong skinId = 0)
            {
                ItemID = itemId;
                Weight = weight;
                SkinID = skinId;
            }
        }

        public class NPCGearSet
        {
            public enum WearType { Undefined, Head, Chest, Legs, Feet, Hands, Fullbody };

            public void AddWeaponOption(WeightedItemInfo info)
            {
                WeaponOptions.Add(info);
            }

            public void AddClothingOption(WearType type, WeightedItemInfo info)
            {
                if (!ClothingOptions.TryGetValue(type, out _)) ClothingOptions[type] = new List<WeightedItemInfo>();

                ClothingOptions[type].Add(info);
            }

            public Dictionary<WearType, List<WeightedItemInfo>> ClothingOptions = new();
            public List<WeightedItemInfo> WeaponOptions = new();
        }

        public class NPCGearSetData
        {
            public Dictionary<NPCHitTarget.Tiers, NPCGearSet> TierGears = new();
        }

        private class LegendaryItemInfo
        {

            [JsonIgnore]
            private ItemDefinition _def = null;

            [JsonIgnore]
            public ItemDefinition ItemDef
            {
                get
                {

                    if (_def == null && ItemID != 0)
                    {
                        _def = ItemManager.FindItemDefinition(ItemID);
                    }

                    return _def;
                }
            }


            [JsonRequired]
            private int _itemId = 0;

            [JsonRequired]
            private ulong _skinId = 0;

            [JsonIgnore]
            public int ItemID
            {
                get
                {
                    return _itemId;
                }
                private set
                {
                    _itemIdToLegendary.Remove(_itemId);

                    _itemId = value;

                    if (SkinID == 0) _itemIdToLegendary[_itemId] = this;
                }
            }

            [JsonIgnore]
            public ulong SkinID
            {
                get
                {
                    return _skinId;
                }
                private set
                {
                    _skinIdToLegendary.Remove(_skinId);

                    _skinId = value;

                    if (value != 0) _skinIdToLegendary[_skinId] = this;
                }
            }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(ClassType.Undefined)]
            public ClassType RequiredClass { get; set; } = ClassType.Undefined;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public int RequiredSurvivalLevel { get; set; } = 0;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public int RequiredMiningLevel { get; set; } = 0;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public int RequiredWoodcuttingLevel { get; set; } = 0;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public int RequiredLuckLevel { get; set; } = 0;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0.0f)]
            public float SpawnProbability { get; set; } = 0f;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string FlavorText { get; set; } = string.Empty;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string BonusDescription { get; set; } = string.Empty;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(1)]
            public int MinAmountToSpawn { get; set; } = 1;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(1)]
            public int MaxAmountToSpawn { get; set; } = 1;

            public float GetSpawnProbability(ClassType classType, float penaltyScalar = 0.5f)
            {
                return RequiredClass == classType || classType == ClassType.Undefined ? SpawnProbability : SpawnProbability * penaltyScalar;
            }

            public static readonly Dictionary<int, LegendaryItemInfo> _itemIdToLegendary = new();

            public static readonly Dictionary<ulong, LegendaryItemInfo> _skinIdToLegendary = new();

            public Dictionary<string, int> SkillModifiers { get; set; } = new Dictionary<string, int>();

            [JsonProperty(PropertyName = "_legendaryName", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            private string _legendaryName = string.Empty;

            [JsonIgnore]
            public string DisplayName
            {
                get
                {
                    if (string.IsNullOrEmpty(_legendaryName)) return ItemDef?.displayName?.english ?? string.Empty;

                    return _legendaryName;
                }
                set
                {
                    _legendaryName = value;
                }
            }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool AppendLegendaryToName { get; set; } = true;

            public bool IsCustomName()
            {
                return !string.IsNullOrEmpty(DisplayName) && !DisplayName.Equals(ItemDef?.displayName?.english ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }


            public bool SpawnIntoInventory(ItemContainer inventory, int overrideAmount = -1)
            {
                if (inventory == null)
                    throw new ArgumentNullException(nameof(inventory));

                var item = SpawnItem(overrideAmount);

                var hookVal = Interface.Oxide.CallHook("OnLegendarySpawn", item, inventory);
                if (hookVal != null && !(bool)hookVal)
                    return false;

                if (!item.MoveToContainer(inventory))
                {
                    Interface.Oxide.LogWarning(nameof(SpawnIntoInventory) + " failed to move legendary item: " + item + " into inventory!!!");
                    RemoveFromWorld(item);
                    return false;
                }
                else
                {
                    Interface.Oxide.CallHook("OnLegendarySpawned", item, inventory);
                    return true;
                }
            }

            /*/
            public bool SpawnAndDrop(Vector3 dropPos, Vector3 dropVel, Quaternion dropRotation = default(Quaternion))
            {
                var item = SpawnItem();


                if (!item.Drop(dropPos, dropVel, dropRotation))
                {
                    Interface.Oxide.LogWarning(nameof(SpawnIntoInventory) + " failed to drop legendary item: " + item + "!!!");
                    RemoveFromWorld(item);
                    return false;
                }
                else return true;
                
            }/*/

            private void RemoveFromWorld(Item item)
            {
                if (item == null) return;
                if (item?.parent != null) item.RemoveFromContainer();
                item.Remove();
            }

            public Item SpawnItem(int overrideAmount = -1)
            {
                var hookVal = Interface.Oxide.CallHook("OnLegendaryItemCreate", this);

                if (hookVal != null)
                    return null;

                var amtToSpawn = Mathf.Clamp(overrideAmount > 0 ? overrideAmount : MinAmountToSpawn == MaxAmountToSpawn ? MinAmountToSpawn : Random.Range(MinAmountToSpawn, MaxAmountToSpawn), 1, int.MaxValue);

                var item = ItemManager.Create(ItemDef, amtToSpawn, SkinID);

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    item.name = sb.Clear().Append(DisplayName).Append(AppendLegendaryToName ? " (Legendary)" : string.Empty).ToString();
                }
                finally { Pool.Free(ref sb); }

                Interface.Oxide.CallHook("OnLegendaryItemCreated", this, item);

                return item;
            }

            public LegendaryItemInfo()
            {
                if (ItemID != 0) _itemIdToLegendary[ItemID] = this;
                if (SkinID != 0) _skinIdToLegendary[SkinID] = this;
            }

            public LegendaryItemInfo(int itemId, ulong skinId = 0)
            {
                ItemID = itemId;
                SkinID = skinId;

                Interface.Oxide.LogWarning(nameof(LegendaryItemInfo) + " itemid: " + ItemID + " skinid " + SkinID);
            }

            public static LegendaryItemInfo GetLegendaryInfoFromItemID(int itemId)
            {
                if (_itemIdToLegendary.TryGetValue(itemId, out LegendaryItemInfo info)) return info;

                return null;
            }

            public static LegendaryItemInfo GetLegendaryInfoFromSkinID(ulong skinId)
            {
                if (_skinIdToLegendary.TryGetValue(skinId, out LegendaryItemInfo info)) return info;

                return null;
            }

            public static LegendaryItemInfo GetLegendaryInfoFromItem(Item item)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));

                var skinLegendary = GetLegendaryInfoFromSkinID(item.skin);
              
                if (skinLegendary != null) return skinLegendary;

                var itemLegendary = GetLegendaryInfoFromItemID(item.info.itemid);

                if (itemLegendary != null)
                {
                    if (itemLegendary.SkinID != 0 && item.skin != itemLegendary.SkinID) return null;
                    else return itemLegendary;
                }

                return null;
            }

            public override string ToString()
            {
                return ItemID + "/" + SkinID + "/" + DisplayName + "/" + RequiredClass + "/" + RequiredLuckLevel + "/" + RequiredMiningLevel + "/" + RequiredWoodcuttingLevel + "/" + RequiredSurvivalLevel;
            }

            public static void UpdateAllCaches(List<LegendaryItemInfo> infos)
            {
                if (infos == null) throw new ArgumentNullException(nameof(infos));

                if (infos.Count < 1) return;

                for (int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];

                    if (info.SkinID != 0) _skinIdToLegendary[info.SkinID] = info;
                    if (info.ItemID != 0 && info.SkinID == 0) _itemIdToLegendary[info.ItemID] = info;
                }

            }

            public static LegendaryItemInfo GetRandomLegendary()
            {
                if (Instance?._legendaryData == null) return null;

                var legs = Instance?._legendaryData?.GetShuffledInfos();


                var minDiff = -1f;
                LegendaryItemInfo item = null;

                for (int i = 0; i < legs.Count; i++)
                {
                    var leg = legs[i];

                    var rng = Random.Range(0.0f, 1.0f);
                    var legProbability = leg.SpawnProbability;

                    var diff = rng >= legProbability ? (rng - legProbability) : (legProbability - rng);

                    if (minDiff == -1f || diff < minDiff)
                    {
                        Interface.Oxide.LogWarning("minDiff is -1f or diff < minDiff, diff: " + diff + ", minnDiff: " + minDiff + ", rng: " + rng + ", leg prob: " + legProbability);
                        minDiff = diff;
                        item = leg;
                    }



                }

                return item;
            }

            public static LegendaryItemInfo GetRandomLegendaryWithClassBias(ClassType userClass)
            {
                if (Instance?._legendaryData == null) return null;

                var legs = Instance?._legendaryData?.GetShuffledInfos();

                if (legs == null || legs.Count < 1)
                {

                    Interface.Oxide.LogError(nameof(legs) + " null/empty!");
                    return null;
                }


                var minDiff = -1f;
                LegendaryItemInfo item = null;

                var countOfLegendaryItemsByClass = 0;

                if (userClass != ClassType.Undefined)
                {
                    for (int i = 0; i < legs.Count; i++)
                    {
                        var leg = legs[i];
                        if (leg.RequiredClass == userClass)
                            countOfLegendaryItemsByClass++;
                    }
                }
               

                Interface.Oxide.LogWarning(nameof(GetRandomLegendaryWithClassBias) + " " + nameof(userClass) + ": " + userClass + ", " + nameof(countOfLegendaryItemsByClass) + ": " + countOfLegendaryItemsByClass);

                var penaltyScalar = userClass == ClassType.Undefined ? 0 : Mathf.Clamp(0.62f - (countOfLegendaryItemsByClass * 0.0211f), 0.125f, 999f);

                Interface.Oxide.LogWarning(nameof(penaltyScalar) + ": " + penaltyScalar);

                for (int i = 0; i < legs.Count; i++)
                {
                    var leg = legs[i];

                    var rng = Random.Range(0.0f, 1.0f);
                    var legProbability = leg.GetSpawnProbability(userClass, penaltyScalar);

                    var diff = rng >= legProbability ? (rng - legProbability) : (legProbability - rng);

                    if (minDiff == -1f || diff < minDiff)
                    {
                        Interface.Oxide.LogWarning("minDiff is -1f or diff < minDiff, diff: " + diff + ", minnDiff: " + minDiff + ", rng: " + rng + ", leg prob: " + legProbability);
                        minDiff = diff;
                        item = leg;
                    }



                }

                return item;
            }

            public string GetModifiersString(bool colorize = false)
            {
                var sb = new StringBuilder();

                foreach (var bonus in SkillModifiers)
                {

                    var stat = StatInfo.GetStatInfoFromShortName(bonus.Key);
                    var statName = stat?.DisplayName ?? bonus.Key;

                    var primaryColor = "white";
                    var secondaryColor = "white";

                    if (colorize && RequiredClass != ClassType.Undefined)
                    {
                        var findClass = ARPGClass.FindByType(RequiredClass);

                        primaryColor = findClass.PrimaryColor;
                        secondaryColor = findClass.SecondaryColor;
                    }

                    sb.Append("\u2022 <color=").Append(primaryColor).Append(">").Append(statName).Append("</color> <color=").Append(secondaryColor).Append(">").Append(bonus.Value < 1 ? string.Empty : "+").Append("</color><color=").Append(primaryColor).Append(">").Append(bonus.Value.ToString("N0")).Append("</color>\n");
                }

                if (sb.Length > 1) sb.Length -= 1;

                return sb.ToString();
            }

            public string GetRequirementsString()
            {
                var sb = new StringBuilder();

                if (RequiredLuckLevel > 0) sb.Append("Luck: ").Append(RequiredLuckLevel.ToString("N0")).Append("\n");
                if (RequiredMiningLevel > 0) sb.Append("Mining: ").Append(RequiredMiningLevel.ToString("N0")).Append("\n");
                if (RequiredWoodcuttingLevel > 0) sb.Append("Woodcutting: ").Append(RequiredWoodcuttingLevel.ToString("N0")).Append("\n");
                if (RequiredSurvivalLevel > 0) sb.Append("Survival: ").Append(RequiredSurvivalLevel.ToString("N0")).Append("\n");

                if (sb.Length > 1) sb.Length -= 1;

                return sb.ToString();
            }

            public bool TryGetModifierBonusesByName(string statName, out int mod)
            {
                if (SkillModifiers.TryGetValue(statName, out mod))
                    return true;
                

                foreach (var kvp in SkillModifiers)
                {
                    if (kvp.Key.Equals(statName, StringComparison.OrdinalIgnoreCase))
                    {
                        mod = kvp.Value;
                        return true;
                    }
                }

                return false;

            }

        }

        private class MessageBufferInfo
        {
            public float LastTime { get; set; } = 0f;

            public float SecondsSinceLastMessageAttempt()
            {
                return Time.realtimeSinceStartup - LastTime;
            }

            private Dictionary<ItemDefinition, int> _totalItemAmount = null;

            private Dictionary<string, double> _totalXpAmount = null;

            public int GetTotalForItem(ItemDefinition info)
            {
                if (info == null)
                    throw new ArgumentNullException(nameof(info));

                if (_totalItemAmount == null) return -1;

                if (_totalItemAmount.TryGetValue(info, out int total)) return total;

                return 0;
            }

            public void AddItemAmount(ItemDefinition info, int amount)
            {
                if (info == null)
                    throw new ArgumentNullException(nameof(info));

                _totalItemAmount ??= new Dictionary<ItemDefinition, int>();

                _totalItemAmount[info] = Mathf.Clamp(GetTotalForItem(info) + amount, 0, int.MaxValue);
            }

            public double GetTotalForSkill(string skillName)
            {
                if (string.IsNullOrWhiteSpace(skillName))
                    throw new ArgumentNullException(nameof(skillName));

                if (_totalXpAmount == null) return -1;

                if (_totalXpAmount.TryGetValue(skillName, out double total)) return total;

                return 0;
            }

            public void AddXpAmount(string skillName, double amount)
            {
                if (string.IsNullOrWhiteSpace(skillName))
                    throw new ArgumentNullException(nameof(skillName));

                _totalXpAmount ??= new Dictionary<string, double>();

                _totalXpAmount[skillName] = Mathf.Clamp((float)GetTotalForSkill(skillName) + (float)amount, 0, float.MaxValue);
            }

            public MessageBufferInfo() { }
            public MessageBufferInfo(float lastTime) { LastTime = lastTime; }

        }

        public class LuckyMessageActionHandler
        {
            public LuckyMessageActionHandler() { }

            private Dictionary<ItemDefinition, Action> _itemAction = null;

            private Dictionary<string, Action> _xpAction = null;

            public Action GetActionForItem(ItemDefinition info)
            {
                if (info == null)
                    throw new ArgumentNullException(nameof(info));


                if (_itemAction != null && _itemAction.TryGetValue(info, out Action act))
                    return act;

                return null;
            }

            public void SetActionForItem(ItemDefinition info, Action action)
            {
                if (info == null)
                    throw new ArgumentNullException(nameof(info));
                _itemAction ??= new Dictionary<ItemDefinition, Action>();

                _itemAction[info] = action ?? throw new ArgumentNullException(nameof(action));
            }

            public Action GetActionForXp(string skillName)
            {
                if (string.IsNullOrWhiteSpace(skillName))
                    throw new ArgumentNullException(nameof(skillName));


                if (_xpAction != null && _xpAction.TryGetValue(skillName, out Action act))
                    return act;

                return null;
            }

            public void SetActionForXp(string skillName, Action action)
            {
                if (string.IsNullOrWhiteSpace(skillName))
                    throw new ArgumentNullException(nameof(skillName));
                _xpAction ??= new Dictionary<string, Action>();

                _xpAction[skillName] = action ?? throw new ArgumentNullException(nameof(action));
            }
        }

        private readonly Dictionary<string, MessageBufferInfo> _userItemMultipliedBuffer = new();
        private readonly Dictionary<string, MessageBufferInfo> _userXpMultipliedBuffer = new();
        private readonly Dictionary<string, LuckyMessageActionHandler> _sendLuckyInvoke = new();

        private void SendItemMultipliedNotification(BasePlayer player, ItemDefinition itemInfo, float itemMultiplier, int newAmount, float desiredBufferTimeMs = 0f)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (itemInfo == null)
                throw new ArgumentNullException(nameof(itemInfo));

            if (itemMultiplier <= 0f)
                throw new ArgumentOutOfRangeException(nameof(itemMultiplier));

            if (!player.IsConnected)
            {
                PrintWarning("player is not connected on notification!!");
                return;
            }



            if (!_userItemMultipliedBuffer.TryGetValue(player.UserIDString, out MessageBufferInfo bufferInfo))
            {
                _userItemMultipliedBuffer[player.UserIDString] = bufferInfo = new MessageBufferInfo();
            }

            bufferInfo.AddItemAmount(itemInfo, newAmount);



            if (_sendLuckyInvoke.TryGetValue(player.UserIDString, out LuckyMessageActionHandler handler))
            {
                var itemAction = handler.GetActionForItem(itemInfo);

                if (itemAction != null) player.CancelInvoke(itemAction);
            }
            else _sendLuckyInvoke[player.UserIDString] = new LuckyMessageActionHandler();

            var sendAction = new Action(() =>
            {
                if (player == null || !player.IsConnected || player.IsDead()) return;

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> ").Append(itemInfo?.displayName?.english ?? "Unknown Item").Append(" multiplied by: <color=").Append(LUCK_COLOR).Append(">").Append(itemMultiplier.ToString("0.00")).Append("</color>").Append(" (Total: <color=").Append(LUCK_COLOR).Append(">").Append(bufferInfo.GetTotalForItem(itemInfo).ToString("N0")).Append("</color>)").ToString());

                    // SendLuckNotifications(player, "bro you got lucky with item multiplier lmao here's the total now: " + bufferInfo.GetTotalForItem(itemInfo) + ", mult: " + itemMultiplier);
                }
                finally
                {
                    try { Pool.Free(ref sb); }
                    finally
                    {
                        try
                        {
                            if (!_userItemMultipliedBuffer.Remove(player.UserIDString)) PrintWarning("could NOT remove item buffer for: " + player);
                        }
                        catch (Exception ex) { PrintError(ex.ToString()); }
                    }
                }
            });

            _sendLuckyInvoke[player.UserIDString].SetActionForItem(itemInfo, sendAction);

            var bufferTime = (desiredBufferTimeMs > 0f ? desiredBufferTimeMs : ((Performance.current.frameTime * 3) + 300)) / 1000;

            player.Invoke(sendAction, bufferTime);

        }

        //SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LuckColor).Append(">Lucky!</color> <color=").Append(color).Append(">").Append(skillName).Append("</color> XP multiplied by: <color=").Append(LuckColor).Append(">").Append(xpMultiplier.ToString("0.#").Replace(".0", string.Empty)).Append("</color> (XP earned: <color=").Append(LuckColor).Append(">").Append(newPoints.ToString("N0")).Append("</color>)").ToString());
        private void SendXPMultipliedNotification(BasePlayer player, string skillName, float xpMultiplier, double points, float desiredBufferTimeMs = 0f, string color = "")
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(skillName))
                throw new ArgumentNullException(nameof(skillName));

            if (xpMultiplier <= 0f)
                throw new ArgumentOutOfRangeException(nameof(xpMultiplier));

            if (!player.IsConnected)
            {
                PrintWarning("player is not connected on notification!!");
                return;
            }



            if (!_userXpMultipliedBuffer.TryGetValue(player.UserIDString, out MessageBufferInfo bufferInfo))
            {
                _userXpMultipliedBuffer[player.UserIDString] = bufferInfo = new MessageBufferInfo();
            }

            bufferInfo.AddXpAmount(skillName, points);



            if (_sendLuckyInvoke.TryGetValue(player.UserIDString, out LuckyMessageActionHandler handler))
            {
                var xpAction = handler.GetActionForXp(skillName);

                if (xpAction != null) player.CancelInvoke(xpAction);
            }
            else _sendLuckyInvoke[player.UserIDString] = new LuckyMessageActionHandler();

            var sendAction = new Action(() =>
            {
                if (player == null || !player.IsConnected || player.IsDead()) return;

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LUCK_COLOR).Append(">Lucky!</color> <color=").Append(color).Append(">").Append(skillName).Append("</color> XP multiplied by: <color=").Append(LUCK_COLOR).Append(">").Append(xpMultiplier.ToString("N1").Replace(".0", string.Empty).Replace(".0", string.Empty)).Append("</color> (XP earned: <color=").Append(LUCK_COLOR).Append(">").Append(bufferInfo.GetTotalForSkill(skillName).ToString("N0")).Append("</color>)").ToString());

                    // SendLuckNotifications(player, sb.Clear().Append("<color=").Append(LuckColor).Append(">Lucky!</color> ").Append((itemInfo?.displayName?.english ?? "Unknown Item")).Append(" multiplied by: <color=").Append(LuckColor).Append(">").Append(itemMultiplier.ToString("0.#")).Append("</color>").Append(" (Total: <color=").Append(LuckColor).Append(">").Append(newAmount.ToString("N0")).Append("</color>)").ToString());

                    // SendLuckNotifications(player, "bro you got lucky with item multiplier lmao here's the total now: " + bufferInfo.GetTotalForItem(itemInfo) + ", mult: " + itemMultiplier);
                }
                finally
                {
                    try { Pool.Free(ref sb); }
                    finally
                    {
                        try
                        {
                            if (!_userXpMultipliedBuffer.Remove(player.UserIDString)) PrintWarning("could NOT remove xp buffer for: " + player);
                        }
                        catch (Exception ex) { PrintError(ex.ToString()); }
                    }
                }
            });

            _sendLuckyInvoke[player.UserIDString].SetActionForXp(skillName, sendAction);

            var bufferTime = (desiredBufferTimeMs > 0f ? desiredBufferTimeMs : ((Performance.current.frameTime * 3) + 300)) / 1000;

            player.Invoke(sendAction, bufferTime);

        }

        private void SendLuckNotifications(BasePlayer player, string msg, bool sendChatMsg = true, float popupDuration = 5f)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(msg)) throw new ArgumentNullException(nameof(msg));

            if (!player.IsConnected)
            {
                PrintWarning(nameof(SendLuckNotifications) + " called on disconnected player: " + player?.userID + "/" + player?.displayName);
                return;
            }

            if (popupDuration > 0f) ShowPopup(player, msg, popupDuration);

            if (sendChatMsg && !permission.UserHasPermission(player.UserIDString, DISABLE_LUCK_CHAT_MESSAGES_PERM)) SendReply(player, msg);
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

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;

            using var fx = new Effect(effect, pos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;

            using var fx = new Effect(effect, player, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private BasePlayer FindPlayerByContainer(ItemContainer container, bool checkSleepers = false)
        {
            if (container == null) return null;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null || p.IsDead() || p?.inventory == null) continue;
                if (p.inventory.containerMain == container || p.inventory.containerWear == container || p.inventory.containerBelt == container) return p;
            }
            if (checkSleepers)
            {
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var p = BasePlayer.sleepingPlayerList[i];
                    if (p == null || p.IsDead() || p?.inventory == null) continue;
                    if (p.inventory.containerMain == container || p.inventory.containerWear == container || p.inventory.containerBelt == container) return p;
                }
            }
            return null;
        }

        private bool AddBatteries(ItemContainer container, int amount)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));

            Item findBattery = null;
            if (container?.itemList != null && container.itemList.Count > 0)
            {
                for (int i = 0; i < container.itemList.Count; i++)
                {
                    var item = container.itemList[i];
                    if (item == null) continue;
                    if (item?.info?.itemid == BATTERY_ID && (item.amount + amount) <= item.MaxStackable())
                    {
                        findBattery = item;
                        break;
                    }
                }
            }

            if (findBattery != null)
            {
                findBattery.amount += amount;
                findBattery.MarkDirty();
                return true;
            }
            else
            {
                var batteries = ItemManager.CreateByItemID(BATTERY_ID, amount);
                if (!batteries.MoveToContainer(container))
                {
                    var lootPlayer = FindLooterFromEntity(container?.entityOwner);
                    var dropPos = lootPlayer == null ? container?.entityOwner?.GetDropPosition() ?? Vector3.zero : lootPlayer?.GetDropPosition() ?? Vector3.zero;
                    var dropRotation = lootPlayer == null ? Quaternion.identity : lootPlayer?.ServerRotation ?? Quaternion.identity;
                    var dropVelocity = lootPlayer == null ? container?.entityOwner?.GetDropVelocity() ?? Vector3.zero : lootPlayer?.GetDropVelocity() ?? Vector3.zero;
                    if (dropPos == Vector3.zero)
                    {
                        PrintWarning("dropPos was vec 3 zero after batteries failed to move to container!!");
                        RemoveFromWorld(batteries);
                        return false;
                    }
                    else return batteries.Drop(dropPos, dropVelocity, dropRotation);
                }
                else return true;
            }
        }

        private int GetFreePosition(ItemContainer container, int startPos = 0)
        {
            if (container == null || startPos < 0 || startPos > (container?.capacity ?? 0)) return -1;
            for (int i = startPos; i < container.capacity; i++)
            {
                if (container?.GetSlot(i) == null) return i;
            }
            return -1;
        }

        private int GetLastFreePosition(ItemContainer container)
        {
            if (container == null) return -1;
            for (int i = container.capacity - 1; i > 0; i--)
            {
                if (container?.GetSlot(i) == null) return i;
            }
            return -1;
        }

        private string GetRandomSkillNameForRotation()
        {

            var skills = Pool.GetList<string>();
            try
            {
                LuckInfo.GetAllStatNamesNoAlloc(ref skills);

                if (skills.Count < 1)
                {
                    PrintError("skills.count < 1!!!");
                    return string.Empty;
                }

                return skills[Random.Range(0, skills.Count)];
            }
            finally { Pool.FreeList(ref skills); }

        }


        private readonly Dictionary<string, Action> _earlAction = new();

        private void GrantRandomBonusToSkill(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || !player.IsConnected) return;

            //temp:
            if (!player.IsAdmin) return;

            if (_earlAction.TryGetValue(player.UserIDString, out Action currentAction))
            {
                InvokeHandler.CancelInvoke(player, currentAction);

                _earlAction.Remove(player.UserIDString);
            }

            var userId = player?.UserIDString;

            currentAction = new Action(() =>
            {
                _currentSkillRotation.Remove(userId);
                _lastRotationType.Remove(userId);

                if (player == null || player.IsDestroyed) return;


                SendReply(player, "<color=" + LUCK_COLOR + ">Crazy Earl's effects have worn off</color>.");
            });

            _earlAction[player.UserIDString] = currentAction;

            player.Invoke(currentAction, 35f);

            string skill;
            _currentSkillRotation[player.UserIDString] = skill = GetRandomSkillNameForRotation();


            var skillDisplayName = FindStatInfoByName(skill)?.DisplayName ?? skill;

            //give this an sb
            SendReply(player, "<color=" + LUCK_COLOR + ">Crazy Earl's is now granting you a <color=green>+3</color> bonus to the <color=orange>" + skillDisplayName + "</color> skill</color>!");
        }

        private ClassType GetLastSkillRotationType(string userId)
        {
            if (!_lastRotationType.TryGetValue(userId, out ClassType type)) type = ClassType.Undefined;

            return type;
        }

        private void SetLastSkillRotationType(string userId, ClassType classType)
        {
            _lastRotationType[userId] = classType;
        }

        private string FirstUpper(string original)
        {
            if (string.IsNullOrEmpty(original)) throw new ArgumentNullException(nameof(original));

            var array = original.ToCharArray();
            array[0] = char.ToUpper(array[0], CultureInfo.CurrentCulture);
            return new string(array);
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

        private BasePlayer FindLooterFromEntity(BaseEntity crate)
        {
            if (crate == null) return null;
            var looters = Pool.GetList<BasePlayer>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || player.IsDead() || !player.IsConnected) continue;

                    var lootSource = player?.inventory?.loot?.entitySource ?? null;
                    if (lootSource == crate)
                    {
                        looters.Add(player);
                        continue;
                    }

                    /*/
                    var heldInv = player?.GetActiveItem()?.contents;

                    if (heldInv != null && player?.GetHeldEntity() == crate)
                        looters.Add(player);/*/
                    

                }

                return looters.Count == 1 ? looters[0] : null;
            }
            finally { Pool.FreeList(ref looters); }
        }

        private bool HasAnyLootersInCrate(BaseEntity crate)
        {
            if (crate == null) return false;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected) continue;
                if (player?.inventory?.loot?.entitySource == crate) return true;
            }
            return false;
        }

        private readonly Dictionary<string, Timer> popupTimer = new();

        private void ShowPopup(BasePlayer player, string msg, float duration = 5f)
        {
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));


            player.SendConsoleCommand("gametip.showgametip", msg);

            if (popupTimer.TryGetValue(player.UserIDString, out Timer endTimer)) endTimer.Destroy();

            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.SendConsoleCommand("gametip.hidegametip");
            });

            popupTimer[player.UserIDString] = endTimer;
        }

        private void GUIUpdate(BasePlayer player)
        {
            if (player == null || !player.IsConnected)
            {
                PrintWarning(nameof(GUIUpdate) + " called with null or disconnected player!!!");
                return;
            }

            if (!_curTree.TryGetValue(player.userID, out SkillTree outTree))
                return;


            if (outTree == SkillTree.Undefined)
            {
                PrintWarning("outTree is undefined!");
                return;
            }

            // if (CuiHelper.DestroyUi(player, "SKTGUI")) SkillGUI(player, outTree);
            // else PrintWarning("CuiHelper returned false, but this is called anyway...?");

            SkillGUI(player, outTree);
        }

        private bool StopSKTGUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return false;

            CuiHelper.DestroyUi(player, "SKTGUI");

            if (!_curTree.TryGetValue(player.userID, out SkillTree current) || current == SkillTree.Undefined) return false;

            _curTree[player.userID] = SkillTree.Undefined;

            return true;
        }

        private bool StopLegendaryGUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return false;

            CuiHelper.DestroyUi(player, "LEGGUI");


            return true;
        }

        private bool StopHitGUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return false;

            CuiHelper.DestroyUi(player, "HitGUI");
            _hasHitGui.Remove(player.userID);

            if (_hitGuiAction.TryGetValue(player.UserIDString, out Action checkAction))
                player.CancelInvoke(checkAction);


            return true;
        }

        private void StopClassGUI(BasePlayer player) //destroys legacy and new
        {
            if (player == null || !player.IsConnected) return;
            CuiHelper.DestroyUi(player, "ClassGUI");
            CuiHelper.DestroyUi(player, "ClassGUINew");
        }

        private void StopMagpieGUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;
            CuiHelper.DestroyUi(player, "GUIMagpie");
        }

        private void StopTokenGUI(BasePlayer player)
        {
            if (player != null & player.IsConnected) CuiHelper.DestroyUi(player, "GUITransmute");
        }

        private readonly Dictionary<ulong, SkillTree> _curTree = new();
        private readonly Dictionary<ulong, int> _curPage = new();

        private void SkillGUI(BasePlayer player, SkillTree tree = SkillTree.Undefined, string additionalText = "")
        {
            if (player == null || !player.IsConnected) return;

            var luckInfo = GetLuckInfo(player);

            _curTree[player.userID] = tree;

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.92"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = true
            }, "Overlay", "SKTGUI");

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.tree prev",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Previous Tree",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.1",
                    AnchorMax = "0.6 0.15"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.stopsktgui",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Close",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0",
                    AnchorMax = "0.6 0.05"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.tree",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Next Tree",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.2",
                    AnchorMax = "0.6 0.25"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.respecbutton",
                        Color = "0.8 0.24 0.45 0.425"
                    },
                Text =
                {
                    Text = "<color=yellow>Respec (<color=#4da6ff>won't reset stats</color>)</color>",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "0.156 0.056"
                }
            }, GUISkin);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                GUISkinElement.Add(new CuiLabel
                {
                    Text =
                {
                    Text = sb.Clear().Append("<color=").Append(LUCK_COLOR).Append("><color=#ff80bf>").Append(tree).Append("</color> Skill tree\nCurrent Points: </color>").Append(luckInfo.SkillPoints.ToString("N0")).Append("\n<color=").Append(LUCK_COLOR).Append(">Luck XP:</color> ").Append(luckInfo.XP.ToString("N0")).Append("<color=").Append(LUCK_COLOR).Append(">/</color>").Append(GetPointsForLevel(luckInfo.Level + 1).ToString("N0")).Append(" <color=").Append(LUCK_COLOR).Append(">(</color>").Append(GetExperiencePercentString(player)).Append("<color=").Append(LUCK_COLOR).Append(">)\nCatchup multiplier:</color> ").Append(getCatchupMultiplier(player.userID).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">XP Rate:</color> ").Append(GetXPMultiplier(player.UserIDString).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">Item Rate:</color> ").Append(GetItemMultiplier(player.UserIDString).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">Chance multiplier:</color> ").Append(GetChanceBonuses(player.UserIDString).ToString("0.00").Replace(".00", string.Empty)).Append("%\n<color=#ff9933>Read <color=").Append(LUCK_COLOR).Append(">/skt help</color> before using this.</color>").Append(!string.IsNullOrEmpty(additionalText) ? "\n" : string.Empty).Append(additionalText).ToString(),
                    FontSize = 20,
                    Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                    RectTransform =
                {
                    AnchorMin = "0.705 0.65",
                    AnchorMax = "1 1"
                }
                }, GUISkin);
            }
            finally { Pool.Free(ref sb); }


            var buttonYMin = 0.958;
            var buttonYMax = 1d;

            var button2YMin = 0.917;
            var button2YMax = 0.958;

            var labelXMin = 0.2;
            var labelXMax = 0.475;

            var labelYMin = 0.961;
            var labelYMax = 0.983;


            sb = Pool.Get<StringBuilder>();
            try
            {
                //must sort into stat info lists by resource tree

                for (int i = 0; i < _statInfos.Count; i++)
                {
                    var stat = _statInfos[i];
                    if (stat == null || stat.AppropriateTree != tree || (stat.Hide && !player.IsAdmin))
                        continue;


                    var cmdText = sb.Clear().Append("skt ").Append(stat.ShortName).ToString();

                    var statClass = ARPGClass.FindByType(stat.AppropriateClass);

                    var primaryColor = statClass?.PrimaryColor ?? string.Empty;
                    var secondaryColor = statClass?.SecondaryColor ?? string.Empty;

                    var primaryColorTag = sb.Clear().Append("<color=").Append(primaryColor).Append(">").ToString();
                    var secondaryColorTag = sb.Clear().Append("<color=").Append(secondaryColor).Append(">").ToString();

                    var labelText = sb.Clear().Append(primaryColorTag).Append(stat.DisplayName).Append("</color> (").Append(primaryColorTag).Append(luckInfo.GetStatLevel(stat)).Append("</color>/").Append(primaryColorTag).Append(stat.MaxLevel).Append("</color>): ").Append(secondaryColorTag).Append(stat.ShortDescription).Append("</color>").ToString();

                    //slight SB issues below (buttonSkillName)

                    var buttonSkillNameSb = sb.Clear().Append(secondaryColorTag).Append(stat.DisplayName).Append("</color>");//.Append(stat.MustMatchClass ? (" (" + primaryColorTag + (statClass?.DisplayName ?? stat?.AppropriateClass.ToString() ?? string.Empty) + "</color>)") : string.Empty).ToString();
                    var buttonSkillName = buttonSkillNameSb.ToString();
                    var buttonSkillNameClass = !stat.MustMatchClass ? buttonSkillName : buttonSkillNameSb.Append(" (").Append(primaryColorTag).Append(stat.AppropriateClass).Append("</color>)").ToString();

                    var moreInfoText = stat.LongDescription;

                    if (string.IsNullOrWhiteSpace(moreInfoText))
                        moreInfoText = labelText;


                    var sktGuiCmd = sb.Clear().Append("skt gui ").Append(moreInfoText).ToString();


                    GUISkinElement.Add(new CuiLabel
                    {
                        Text =
                {
                    Text = sb.Clear().Append("<-- ").Append(labelText).ToString(),
                    Align = TextAnchor.MiddleLeft,
                    FontSize = 14,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append(labelXMin).Append(" ").Append(labelYMin).ToString(),
                    AnchorMax = sb.Clear().Append(labelXMax).Append(" ").Append(labelYMax).ToString()
                }
                    }, GUISkin);



                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = cmdText,
                        Color = "0.2 0.4 0.65 0.75"
                    },
                        Text =
                {
                    Text = sb.Clear().Append("<color=#F6D1BE>Upgrade </color>").Append(buttonSkillNameClass).ToString(),
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.0 ").Append(buttonYMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.195 ").Append(buttonYMax).ToString()
                }
                    }, GUISkin);

                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = sktGuiCmd,
                        Color = "0.15 0.35 0.72 0.75"
                    },
                        Text =
                {
                    Text = sb.Clear().Append("<size=16><color=#99d6ff>More <i>").Append(buttonSkillName).Append("</i> info</color> </size>(<color=#33adff>See top right</color>)").ToString(),
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1",
                    VerticalOverflow = VerticalWrapMode.Overflow
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.0 ").Append(button2YMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.195 ").Append(button2YMax).ToString()
                }
                    }, GUISkin);

                    buttonYMin -= 0.1;
                    buttonYMax -= 0.1;

                    button2YMin -= 0.1;
                    button2YMax -= 0.1;

                    labelYMin -= 0.1;
                    labelYMax -= 0.1;

                }
            }
            finally { Pool.Free(ref sb); }

           

            CuiHelper.DestroyUi(player, "SKTGUI"); //destroy only after it's ready to be added
            CuiHelper.AddUi(player, GUISkinElement);
        }


        private int GetNextLegendaryGUIPage(ulong userId)
        {
            var legendaries = _legendaryData.Infos;
            if (legendaries.Count < 1)
            {
                PrintWarning("legendaries count < 1!! (getnext)");
                return -1;
            }

            var itemsPerPage = 8;

            if (!_curPage.TryGetValue(userId, out int page)) page = 0;

            var index = page * itemsPerPage;


            if (!(index + itemsPerPage + 1 < legendaries.Count))
                return 0;


            return page + 1;
        }

        private void LegendaryGUI(BasePlayer player, int page, string additionalText = "") //TODO: switch gui 'style' over to new skt gui
        {
            if (player == null || !player.IsConnected) return;

            var legendaries = _legendaryData.Infos;
            if (legendaries.Count < 1)
            {
                PrintWarning("legendaries count < 1!!");
                return;
            }

            var itemsPerPage = 8;

            var index = page * itemsPerPage;

            PrintWarning("legendaries count: " + legendaries.Count + ", items per page: " + itemsPerPage + ", index: " + index);

            var count = legendaries.Count - index;

            var listOfLegendaries = legendaries.GetRange(index, Mathf.Min(itemsPerPage, count));


            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.92"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = true
            }, "Overlay", "LEGGUI"); //legggoooo

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.stoplegendarygui",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Close",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.1",
                    AnchorMax = "0.6 0.15"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "legendary.nextpage",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Next Page",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.2",
                    AnchorMax = "0.6 0.25"
                }
            }, GUISkin);

            var sb = Pool.Get<StringBuilder>();
            try
            {
                GUISkinElement.Add(new CuiLabel
                {
                    Text =
                {
                    Text = "<color=yellow>LEGENDARY ITEMS</color>:\n" + additionalText, //sb.Clear().Append("<color=").Append(LUCK_COLOR).Append("><color=#ff80bf>").Append("tree").Append("</color> Skill tree (WIP)\nCurrent Points: </color>").Append(luckInfo.SkillPoints.ToString("N0")).Append("\n<color=").Append(LUCK_COLOR).Append(">Luck XP:</color> ").Append(luckInfo.XP.ToString("N0")).Append("<color=").Append(LUCK_COLOR).Append(">/</color>").Append(getPointsNeededForNextLevel(luckInfo.Level).ToString("N0")).Append(" <color=").Append(LUCK_COLOR).Append(">(</color>").Append(GetExperiencePercentString(player)).Append("<color=").Append(LUCK_COLOR).Append(">)\nCatchup multiplier:</color> ").Append(getCatchupMultiplier(player.userID).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">XP Rate:</color> ").Append(getXPMultiplier(player.UserIDString).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">Item Rate:</color> ").Append(getItemMultiplier(player.UserIDString).ToString("N0")).Append("%\n<color=").Append(LUCK_COLOR).Append(">Chance multiplier:</color> ").Append(GetLuckChancePercentage(player.userID).ToString("0.00").Replace(".00", string.Empty)).Append("%\n<color=#ff9933>Read <color=").Append(LUCK_COLOR).Append(">/skt help</color> before using this.</color>").Append(!string.IsNullOrEmpty(additionalText) ? " " : string.Empty).Append(additionalText).ToString(),
                    FontSize = 20,
                    Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0 0.074",
                    AnchorMax = "0.234 1"
                }
                }, GUISkin);
            }
            finally { Pool.Free(ref sb); }


            var buttonYMin = 0.25;
            var buttonYMax = 0.285;

            var button2YMin = 0.215;

            var button2YMax = 0.25;

            var labelMin = 0.65d;
            var labelMax = 1d;

            for (int i = 0; i < listOfLegendaries.Count; i++)
            {

                var item = listOfLegendaries[i];

                var useLabelMin = labelMin;
                var useLabelMax = labelMax;
                //  var cmdText = "leg ";

                var legName = item.DisplayName;

                var labelText = legName;
                if (item.RequiredClass != ClassType.Undefined) labelText += " (" + item.RequiredClass + ")";

                //var moreInfoText = string.Empty;


                var moreInfoText = "<color=" + LUCK_COLOR + ">" + item.GetModifiersString() + "</color>\n\n<color=orange>" + item.FlavorText + "</color>\n" + item.BonusDescription + "\n\n\nRequirements: " + item.GetRequirementsString();

                if (string.IsNullOrEmpty(moreInfoText))
                    moreInfoText = labelText;


                //this label code is ridiculous but it works so don't question it

                var noTag = RemoveTags(labelText);
                var reducLabelAmount = noTag.Length > 140 ? 0.00195 /*/
        tree == SkillTree.Classes ? 0.00195/*/ : 0.00310;
                if (noTag.Length > 22)
                {
                    var addCount = noTag.Length - 22;

                    for (int j = 0; j < addCount; j++)
                    {
                        useLabelMin -= reducLabelAmount;
                        useLabelMax -= reducLabelAmount;
                    }

                }
                GUISkinElement.Add(new CuiLabel
                {
                    Text =
                {
                    Text = labelText + " ->",
                    Align = TextAnchor.MiddleLeft,
                    FontSize = 11,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = useLabelMin + " " + buttonYMin,
                    AnchorMax = useLabelMax + " " + buttonYMax
                }
                }, GUISkin);


                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "leg " + moreInfoText,
                        Color = "0.15 0.35 0.72 0.75"
                    },
                    Text =
                {
                    Text = "<size=16><color=#99d6ff><i>" + legName + "</i> info</color> </size>(<color=#33adff>Top left</color>)",
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.75 " + button2YMin,
                    AnchorMax = "0.95 " + button2YMax
                }
                }, GUISkin);

                buttonYMin += 0.1;
                buttonYMax += 0.1;

                button2YMin += 0.1;
                button2YMax += 0.1;
            }

            CuiHelper.DestroyUi(player, "LEGGUI"); //destroy only after it's ready to be added
            CuiHelper.AddUi(player, GUISkinElement);
        }

        private void ClassGUILegacy(BasePlayer player, string forceMainText = "")
        {
            if (player == null || !player.IsConnected) return;

            _classGuiStartTime[player.UserIDString] = Time.realtimeSinceStartup;

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.95"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = true
            }, "Overlay", "ClassGUI");

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.stopclassgui",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Close (<color=orange>/class</color> to select later)",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.1",
                    AnchorMax = "0.6 0.15"
                }
            }, GUISkin);


            var genericClassText = "<color=#ffa31a>WELCOME TO OUR ACTION RPG (ARPG) EXPERIENCE!</color>\n\n<color=#88ff4d>Think of <color=#ff1a1a>Diablo</color> & <color=#BDAB75>Runescape</color> meeting <color=#CE412B>Rust</color> in a cataclysmic clash of worlds!</color>\n\nClasses all have various unique attributes & abilities. All of them come with their own upsides and downsides, and it will make for <color=#FF54CE>a unique experience <color=#FF0263>each wipe</color> as you play through the different classes</color>!\n<color=#54E8FF>More info for each class can be seen by clicking the More Info button</color>.\n<color=#D9FF00>Please choose wisely, as you can't switch until next wipe</color>!\n\n<color=#ff4da6><i>YOU CAN SELECT YOUR CLASS LATER.</i> YOU ARE NOT REQUIRED TO SELECT A CLASS. TYPE <color=#66ccff>/CLASS</color> AT ANY TIME. <i>NO CLASS CAPS YOU TO LEVEL <color=#66ccff>25</color>.</i>\n<color=#EA6500>Engineer</color> class coming soon<color=#66ccff>!</color> Type <color=" + LUCK_COLOR + ">/skt gui</color> for skills</color><color=#66ccff>!</color>";

            var sb = Pool.Get<StringBuilder>();
            try
            {
                GUISkinElement.Add(new CuiLabel
                {
                    Text =
                {
                    Text = sb.Clear().Append("<size=20><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color></size> ARPG CLASSES\n").Append(!string.IsNullOrEmpty(forceMainText) ? forceMainText : genericClassText).ToString(),
                    FontSize = 20,
                    Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0 0.074",
                    AnchorMax = "0.234 1"
                }
                }, GUISkin);

                var buttonYMin = 0.25;
                var buttonYMax = 0.285;

                var button2YMin = 0.215;

                var button2YMax = 0.25;

                var labelMin = 0.65d;
                var labelMax = 1d;

                var length = Enum.GetValues(typeof(ClassType)).Length; //number of classes


                for (int i = 0; i < length; i++)
                {
                    var useLabelMin = labelMin;
                    var useLabelMax = labelMax;


                    var type = (ClassType)i;
                    var typeClass = ARPGClass.FindByType(type);
                    if (typeClass == null)
                    {
                        if (type != ClassType.Undefined) PrintWarning("typeClass null!! type: " + type);

                        continue;
                    }

                    if (typeClass.Hide && !player.IsAdmin)
                    {
                        PrintWarning("hiding: " + typeClass + " from non-admin: " + player);
                        continue;
                    }

                    var classNameColor = typeClass.PrimaryColor;
                    var classDescColor = typeClass.SecondaryColor;

                    var colorcode = classNameColor.TrimStart('#');


                    var classNameRedRgb = (float)int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber);
                    var classNameGreenRgb = (float)int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber);
                    var classNameBlueRgb = (float)int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber);

                    var labelText = sb.Clear().Append("<color=").Append(classNameColor).Append(">").Append(typeClass.DisplayName).Append((type == ClassType.Nomad || type == ClassType.Loner) ? " <i>(CANNOT JOIN CLANS)</i>" : string.Empty).Append("</color>\n<color=").Append(classDescColor).Append(">").Append(typeClass.ShortDescription).Append("</color>").ToString();
                    var cmdText = sb.Clear().Append("class select ").Append(type).ToString();
                    var cmdTextInfo = sb.Clear().Append("class ").Append(type).ToString();

                    var labelTextNoClassName = labelText.Replace(sb.Clear().Append(typeClass.DisplayName).Append("\n").ToString(), string.Empty);

                    //  PrintWarning("labelText: " + labelText + "\n\n" + labelTextNoClassName);

                    var noTag = RemoveTags(labelTextNoClassName);
                    if (noTag.Length > 22)
                    {
                        var addCount = noTag.Length - 22;

                        for (int j = 0; j < addCount; j++)
                        {
                            useLabelMin -= 0.0035;
                            useLabelMax -= 0.0035;
                        }

                    }

                    GUISkinElement.Add(new CuiLabel
                    {
                        Text =
                {
                    Text = sb.Clear().Append(labelText).Append(" ->").ToString(),
                    Align = TextAnchor.MiddleRight,
                    FontSize = 10,
                    Color = "1 1 1 1"
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append(useLabelMin).Append(" ").Append(buttonYMin).ToString(),
                    AnchorMax = sb.Clear().Append(useLabelMax).Append(" ").Append(buttonYMax).ToString()
                }
                    }, GUISkin);



                    var classDisplayNameSize = typeClass.DisplayName.Length > 7 ? 17 : 21;


                    var selectButtonText = sb.Clear().Append("<size=18>Select <size=").Append(classDisplayNameSize).Append("><i>").Append(typeClass.DisplayName).Append("</i></size>").ToString();

                    var cleanText = RemoveTags(selectButtonText);

                    var spaceToInsert = cleanText.Length < 17 ? (17 - cleanText.Length) : 0;

                    sb.Clear();

                    for (int j = 0; j < spaceToInsert; j++) sb.Append(" ");

                    var permaText = sb.Append(" <size=14>(<color=#FF145A><size=22>PERMANENT</size></color>)</size></size>").ToString();

                    selectButtonText += permaText;

                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = cmdText,
                        Color = sb.Clear().Append(classNameRedRgb / 255).Append(" ").Append(classNameGreenRgb / 255).Append(" ").Append(classNameBlueRgb / 255).Append(" 0.925").ToString()
                    },
                        Text =
                {
                    Text = selectButtonText,
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.75 ").Append(buttonYMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.95 ").Append(buttonYMax).ToString()
                }
                    }, GUISkin);

                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = cmdTextInfo,
                        Color = "0.15 0.35 0.72 0.75"
                    },
                        Text =
                {
                    Text = sb.Clear().Append("<size=16><color=#99d6ff>More <i>").Append(typeClass.DisplayName).Append("</i> info</color> </size>(<color=#33adff>See top left</color>)").ToString(),
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.75 ").Append(button2YMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.95 ").Append(button2YMax).ToString()
                }
                    }, GUISkin);


                    buttonYMin += 0.1;
                    buttonYMax += 0.1;

                    button2YMin += 0.1;

                    button2YMax += 0.1;
                }

            }
            finally { Pool.Free(ref sb); }

            CuiHelper.DestroyUi(player, "ClassGUINew"); //if "show more info" from new GUI is clicked, destroy old GUI.
            CuiHelper.DestroyUi(player, "ClassGUI"); //destroy only after it's ready to be added
            CuiHelper.AddUi(player, GUISkinElement);
        }

        private void ClassGUINew(BasePlayer player, ClassType userClass = ClassType.Nomad)
        {
            if (player == null || !player.IsConnected) return;

            _classGuiLastClass[player.UserIDString] = userClass;
            _classGuiStartTime[player.UserIDString] = Time.realtimeSinceStartup;



            var imageAnchorMin = "0.432 0.319";
            var imageAnchorMax = "0.549 0.736";

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                var mainUrl = @"https://cdn.prismrust.com/class_stuff_2/";
                var idUrl = string.Empty;

                /*switch (userClass)
                {
                    case ClassType.Nomad:
                        idUrl = @"1139888112655409192/";
                        break;
                    case ClassType.Miner:
                        idUrl = @"1139888115138428928/";
                        break;
                    case ClassType.Lumberjack:
                        idUrl = @"1139888113292935208/";
                        break;
                    case ClassType.Loner:
                        idUrl = @"1139888114316353578/";
                        break;
                    case ClassType.Farmer:
                        idUrl = @"1139888114870009856/";
                        break;
                    case ClassType.Packrat:
                        idUrl = @"1139888112403742770/";
                        break;
                    case ClassType.Mercenary:
                        idUrl = @"1139888112944824361/";
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrWhiteSpace(idUrl))
                    PrintError(nameof(idUrl) + " is null/empty!!!! class: " + userClass);*/

                var url = sb.Clear().Append(mainUrl).Append(idUrl).Append(userClass.ToString().ToLower()).Append(".jpg").ToString(); //sb.Clear().Append(PRISM_CLASS_SELECTS_URL).Append(userClass).Append(".jpg").ToString();

                var typeClass = ARPGClass.FindByType(userClass);

                if (typeClass.Hide && !player.IsAdmin)
                    PrintError("we're being asked to show a hidden class to a non-admin!!!!");

                

                var classNameColor = typeClass.PrimaryColor;

                var colorcode = classNameColor.TrimStart('#');


                var classNameRedRgb = (float)int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber);
                var classNameGreenRgb = (float)int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber);
                var classNameBlueRgb = (float)int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber);

                var buttonColors = sb.Clear().Append(classNameRedRgb / 255).Append(" ").Append(classNameGreenRgb / 255).Append(" ").Append(classNameBlueRgb / 255).Append(" 0.925").ToString();

                var selectCmd = sb.Clear().Append("class select ").Append(userClass).ToString();

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
                    CursorEnabled = true
                }, "Overlay", "ClassGUINew");

                GUISkinElement.Add(new CuiElement
                {
                    Name = "ImgUIImg",
                    Components =
                {
                    new CuiRawImageComponent
                    {
                     //   Color = config.ImageColor,
                        Url = url,
                        FadeIn = 0.4f
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = imageAnchorMin,
                        AnchorMax = imageAnchorMax
                    }
                },
                    Parent = GUISkin,

                });

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class gui_legacy",
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "Show More Info",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.432 0.25",
                    AnchorMax = "0.549 0.285"
                }
                }, GUISkin);

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class stop",
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "Close",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.432 0.215",
                    AnchorMax = "0.549 0.25"
                }
                }, GUISkin);

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = selectCmd,
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "Select",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.432 0.285",
                    AnchorMax = "0.549 0.319"
                }
                }, GUISkin);

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class new next",
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "-->",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.549 0.319",
                    AnchorMax = "0.58 0.347"
                }
                }, GUISkin);

                GUISkinElement.Add(new CuiButton
                {
                    Button =
                    {
                        Command = "class new prev",
                        Color = buttonColors
                    },
                    Text =
                {
                    Text = "<--",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0.401 0.319",
                    AnchorMax = "0.432 0.347"
                }
                }, GUISkin);

                CuiHelper.DestroyUi(player, "ClassGUINew");


                CuiHelper.AddUi(player, GUISkinElement);

            }
            finally { Pool.Free(ref sb); }

            ShowToast(player, "No images? Use 'Show More Info' for text version.");
           
        }

        private void HitGUI(BasePlayer player, string forceMainText = "")
        {
            if (player == null || !player.IsConnected) return;

            var hits = GetHitTargets(true);

            if (hits.Count < 1)
            {
                SendReply(player, "No hits!");
                return;
            }


            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.95"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = true
            }, "Overlay", "HitGUI");

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.stophitgui",
                        Color = "0.6 0.34 0.75 0.425"
                    },
                Text =
                {
                    Text = "Close",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.1",
                    AnchorMax = "0.6 0.15"
                }
            }, GUISkin);


            var genericClassText = "Info about hit";

            var sb = Pool.Get<StringBuilder>();
            try
            {
                GUISkinElement.Add(new CuiLabel
                {
                    Text =
                {
                    Text = sb.Clear().Append("<size=20><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color></size> MERC HITS\n").Append(!string.IsNullOrEmpty(forceMainText) ? forceMainText : genericClassText).ToString(),
                    FontSize = 20,
                    Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1"
                },
                    RectTransform =
                {
                    AnchorMin = "0 0.074",
                    AnchorMax = "0.234 1"
                }
                }, GUISkin);

                var buttonYMin = 0.25;
                var buttonYMax = 0.285;

                var button2YMin = 0.215;

                var button2YMax = 0.25;

                var labelMin = 0.65d;
                var labelMax = 1d;

             
                var length = hits.Count;


                for (int i = 0; i < length; i++)
                {
                    var hit = hits[i];

                    var useLabelMin = labelMin;
                    var useLabelMax = labelMax;

                    GUISkinElement.Add(new CuiLabel
                    {
                        Text =
                {
                    Text = hit.FlavorNickName + " (" + hit.Tier + " tier)",
                    Align = TextAnchor.MiddleLeft,
                    FontSize = 11,
                    Color = "1 1 1 1"
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append(useLabelMin).Append(" ").Append(buttonYMin).ToString(),
                    AnchorMax = sb.Clear().Append(useLabelMax).Append(" ").Append(buttonYMax).ToString()
                }
                    }, GUISkin);



                    var selectButtonText = "Take hit"; //sb.Clear().Append("<size=18>Select <size=").Append(classDisplayNameSize).Append("><i>").Append(typeClass.DisplayName).Append("</i></size>").ToString();

                    var cleanText = RemoveTags(selectButtonText);

                    var spaceToInsert = cleanText.Length < 17 ? (17 - cleanText.Length) : 0;

                    sb.Clear();

                    for (int j = 0; j < spaceToInsert; j++) sb.Append(" ");


                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = "luck.takehit " + hit.NPC.net.ID,
                        Color = "1 1 1 1"
                    },
                        Text =
                {
                    Text = selectButtonText,
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "0 1 1 1"
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.75 ").Append(buttonYMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.95 ").Append(buttonYMax).ToString()
                }
                    }, GUISkin);

                    GUISkinElement.Add(new CuiButton
                    {
                        Button =
                    {
                        Command = "cmdTextInfo",
                        Color = "0.15 0.35 0.72 0.75"
                    },
                        Text =
                {
                    Text = "Show info about hit",
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                        RectTransform =
                {
                    AnchorMin = sb.Clear().Append("0.75 ").Append(button2YMin).ToString(),
                    AnchorMax = sb.Clear().Append("0.95 ").Append(button2YMax).ToString()
                }
                    }, GUISkin);


                    buttonYMin += 0.1;
                    buttonYMax += 0.1;

                    button2YMin += 0.1;

                    button2YMax += 0.1;
                }

            }
            finally { Pool.Free(ref sb); }


            StopHitGUI(player);
            //CuiHelper.DestroyUi(player, "HitGUI"); //destroy only after it's ready to be added
            CuiHelper.AddUi(player, GUISkinElement);

            var checkAction = new Action(() => CheckHitGUI(player));

            _hitGuiAction[player.UserIDString] = checkAction;

            player.InvokeRepeating(checkAction, 0.5f, 0.5f);

            _hasHitGui.Add(player.userID);

        }


        private void CheckHitGUI(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || !player.IsConnected) return;

            var npcPos = _banditConversationalist?.transform?.position ?? Vector3.zero;

            var plyPos = player?.transform?.position ?? Vector3.zero;
            
            if (Vector3.Distance(npcPos, plyPos) >= 4)
            {
                PrintWarning("pos >= 4 on " + nameof(CheckHitGUI) + " - destroying!");
                StopHitGUI(player);
            }

        }

        private string GetMagpieGUIText(BasePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var cooldown = player?.GetComponent<MagpieCooldownHandler>();
            if (cooldown == null)
                return string.Empty;


            var sb = Pool.Get<StringBuilder>();
            try
            {
                return cooldown.IsOnCooldown ? sb.Clear().Append("<color=#0577A0><i><size=11>Magpie</size>\n<size=16>").Append(cooldown.CooldownRemaining.ToString("N0")).Append("s</size></i></color>").ToString() : "<color=#0577A0><i><size=16>Magpie</size></i></color>";
            }
            finally { Pool.Free(ref sb); }
        }

        private void MagpieGUI(BasePlayer player, string text = "")
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;


            var guiName = "GUIMagpie";

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.6"
                },
                RectTransform =
                {
                    AnchorMin = "0.77 0.67",
                    AnchorMax = "0.815 0.75"
                },
                CursorEnabled = true
            }, "Overlay", guiName);

            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = text,
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "0.7 0.60 0.67 1.0"
                },
                RectTransform =
                {
                    AnchorMin = "0.035 0.2",
                    AnchorMax = "0.9 1.3"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.magpiereroll",
                        Color = "0.43 0.6 0.85 0.75"
                    },
                Text =
                {
                    Text = "Re-roll",
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.05 0.1",
                    AnchorMax = "0.9 0.4"
                }
            }, GUISkin);


            CuiHelper.DestroyUi(player, guiName);
            CuiHelper.AddUi(player, GUISkinElement);
        }

        private void TransmuteGUI(BasePlayer player, string text = "")
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;


            var guiName = "GUITransmute";

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.6"
                },
                RectTransform =
                {
                    AnchorMin = "0.67 0.12",
                    AnchorMax = "0.715 0.20"
                },
                CursorEnabled = true
            }, "Overlay", guiName);

            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = text,
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "0.7 0.60 0.67 1.0"
                },
                RectTransform =
                {
                    AnchorMin = "0.035 0.2",
                    AnchorMax = "0.9 1.3"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.transmute",
                        Color = "0.43 0.6 0.85 0.75"
                    },
                Text =
                {
                    Text = "Transmute",
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.05 0.1",
                    AnchorMax = "0.9 0.4"
                }
            }, GUISkin);


            CuiHelper.DestroyUi(player, guiName);
            CuiHelper.AddUi(player, GUISkinElement);
        }



        private void ShowTextAboveInventory(BasePlayer player, string text, int fontSize = 16)
        {
            if (player == null || !player.IsConnected) return;


            var elements = new CuiElementContainer();
            elements.Add(new CuiLabel
            {
                Text =
                {
                    Text = text,
                    Color = "1 1 1 0.85",
                    FontSize = fontSize,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0 0.8",
                    AnchorMax = "1 0.9"
                }
            }, "Overlay", "topMidText");


            CuiHelper.DestroyUi(player, "topMidText");
            CuiHelper.AddUi(player, elements);

        }

        private void StopTextAboveInventory(BasePlayer player)
        {
            if (player != null && player.IsConnected) CuiHelper.DestroyUi(player, "topMidText");
        }

        private void ClassSetGUI(BasePlayer player, string text = "")
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;


            var guiName = "GUISets";

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.125 0.1 0.1 0.25"
                },
                RectTransform =
                {
                    AnchorMin = "0.319 0.09",
                    AnchorMax = "0.362 0.167"
                },
                CursorEnabled = false
            }, "Under", guiName);

            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = text,
                    FontSize = 9,
                    Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "1 1"
                }
            }, GUISkin);

            CuiHelper.DestroyUi(player, guiName);
            CuiHelper.AddUi(player, GUISkinElement);
        }

        private void BackpackDepositGUI(BasePlayer player, string textOverride = "")
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;

            var guiName = "GUI_Backpack";

            var sb = Pool.Get<StringBuilder>();

            var text = textOverride;

            if (string.IsNullOrEmpty(text))
            {
                try
                {
                    var isOn = GetLuckInfo(player).BackpackAutoDeposit;

                    var stateStr = isOn ? "ON" : "OFF";

                    var stateColorTag = GetOpeningColorTag(isOn ? "#53D559" : "#FF0030"); //PRISM green, red, respectively

                    text = sb.Clear().Append("Auto Deposit (").Append(stateColorTag).Append(stateStr).Append("</color>)").ToString();
                }
                finally { Pool.Free(ref sb); }
            }

     

        

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.725"
                },
                RectTransform =
                {
                    AnchorMin = "0.67 0.0675",
                    AnchorMax = "0.715 0.1475"
                },
                CursorEnabled = true
            }, "Overlay", guiName);

            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = text,
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.035 0.2",
                    AnchorMax = "0.9 1.3"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "luck.backpacktoggle",
                        Color = "0.43 0.6 0.85 0.75"
                    },
                Text =
                {
                    Text = "Toggle",
                    FontSize = 10,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.05 0.1",
                    AnchorMax = "0.9 0.4"
                }
            }, GUISkin);


            CuiHelper.DestroyUi(player, guiName);
            CuiHelper.AddUi(player, GUISkinElement);
        }

        private readonly FieldInfo curList = typeof(InvokeHandler).GetField("curList", BindingFlags.Instance | BindingFlags.NonPublic);
        private ListDictionary<InvokeAction, float> _invokeList = null;

        private ListDictionary<InvokeAction, float> InvokeList
        {
            get
            {
                if (_invokeList == null && InvokeHandler.Instance != null) _invokeList = curList.GetValue(InvokeHandler.Instance) as ListDictionary<InvokeAction, float>;
                return _invokeList;
            }
        }

        private void CancelInvoke(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null).ToString() == targetName && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private void CancelInvoke(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName) || obj == null) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null) == obj && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private bool IsInvoking(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null) == obj) return true;
                }
            }
            return false;
        }
        private bool IsInvoking(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null).ToString() == targetName) return true;
                }
            }
            return false;
        }
        
        private int GetStatLevelByName(string userId, string statName, bool callHook = true)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            

            if (string.IsNullOrWhiteSpace(statName))
                throw new ArgumentNullException(nameof(statName));

            return GetLuckInfo(userId)?.GetStatLevelByName(statName, callHook) ?? -1;
        }

        private long GetPointsForLevel(long level) { return (long)(110 * level * level * level * 0.05); }

        private long GetLevelFromPoints(float points, int maxLvlsToIterate = 2000) //i can't do math so i did this instead :)
        {
            var watch = Pool.Get<Stopwatch>();

            try
            {
                watch.Restart();

                if (points < GetPointsForLevel(1)) //less points than level 1, so return level 1!
                    return 1;

                var useLevel = -1;


                var count = 0;
                var loopLvl = 1;

                while (true)
                {
                    try
                    {
                        if (count >= maxLvlsToIterate)
                        {
                            PrintWarning("safety break!");
                            break;
                        }

                        var pointsLvl = GetPointsForLevel(loopLvl);

                        if (points == pointsLvl || (points < GetPointsForLevel(loopLvl + 1) && points >= pointsLvl))
                        {
                            useLevel = loopLvl;
                            break;
                        }

                        loopLvl++;
                    }
                    finally { count++; }
                }

                //var lvlCap = (int)levelCaps[LUCK_SHORTNAME];

                return useLevel < 1 ? 1 : useLevel;
            }
            finally
            {
                try { if (watch.Elapsed.TotalMilliseconds >= 1) PrintWarning(watch.Elapsed.TotalMilliseconds + "ms passed for: " + nameof(GetLevelFromPoints) + " (" + points + ", " + maxLvlsToIterate + ")"); }
                finally { Pool.Free(ref watch); }
            }
        }


        private double? _resourceMultiplier = null;

        private double ResourceMultiplier
        {
            get
            {
                if (_resourceMultiplier == null || !_resourceMultiplier.HasValue)
                    _resourceMultiplier = Convert.ToDouble(resourceMultipliers[LUCK_SHORTNAME]);


                return _resourceMultiplier.Value;
            }
            set
            {
                _resourceMultiplier = value;
            }
        }

        private double GetGatherMultiplier(long skillLevel) { return 1 + ResourceMultiplier * 0.1 * (skillLevel - 1); }

        #endregion
        #region Saving

        private IEnumerator SaveAllData(float interval = 1800, bool init = false)
        {
            PrintWarning(nameof(Luck) + "." + nameof(SaveAllData) + " called, this will create a while loop that will effectively continue as a timer until it is cancelled (coroutine)");

            if (init)
            {
                PrintWarning("init true, so waiting interval time before saving first time");
                yield return CoroutineEx.waitForSecondsRealtime(interval);
            }

            while (true)
            {
                var watch = Pool.Get<Stopwatch>();
                try
                {
                    yield return CoroutineEx.waitForSecondsRealtime(3f);

                    PrintWarning("Saving users data...");

                    watch.Restart();

                    SaveUsers();

                    PrintWarning("Saved users data in: " + watch.Elapsed.TotalMilliseconds + "ms");

                    yield return CoroutineEx.waitForSecondsRealtime(2f);

                    PrintWarning("Saving charm data...");

                    watch.Restart();

                    SaveCharmData();

                    PrintWarning("Saved charm data in " + watch.Elapsed.TotalMilliseconds + "ms");

                    yield return CoroutineEx.waitForSecondsRealtime(4f);

                    PrintWarning("Saving class data...");

                    watch.Restart();

                    SaveClassData();

                    watch.Stop();

                    PrintWarning("Saved class data in " + watch.Elapsed.TotalMilliseconds + "ms");

                    yield return CoroutineEx.waitForSecondsRealtime(4f);

                    PrintWarning("Saving backpack data...");

                    watch.Restart();

                    SaveBackpackData();

                    PrintWarning("Saved backpack data in " + watch.Elapsed.TotalMilliseconds + "ms");

                }
                finally { Pool.Free(ref watch); }

                PrintWarning("Finished all data saving, now waiting: " + interval + " until we save again");
                yield return CoroutineEx.waitForSecondsRealtime(interval);
            }
        }

        private int RemoveAllCooldownHandlers(BasePlayer player)
        {
            if (player == null || player.IsDestroyed) return -1;

            var cooldownHandlers = player?.GetComponents<PlayerCooldownHandler>() ?? null;

            var length = cooldownHandlers?.Length ?? 0;

            if (length > 0)
                for (int i = 0; i < length; i++)
                    cooldownHandlers[i]?.DoDestroy();

            return length;
        }

        private void Unload()
        {
            try
            {
                try
                {

                    foreach (var routine in _coroutines)
                    {
                        if (routine == null)
                            continue;

                        ServerMgr.Instance.StopCoroutine(routine);
                    }

                    _coroutines?.Clear();

                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try { RemoveFromWorld(_grenadeHatchet); }
                catch(Exception ex) { PrintError(ex.ToString()); }
         
                try { if (_hitGenerateAction != null) ServerMgr.Instance.CancelInvoke(_hitGenerateAction); }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try 
                {
                    if (_f1HelpTimer != null)
                    {
                        _f1HelpTimer.Destroy();
                        _f1HelpTimer = null; //necessary? idk
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }


                try 
                {

                    foreach (var fb in _oreFireballs)
                    {
                        var ent = BaseNetworkable.serverEntities.Find(fb) as FireBall;
                        if (ent != null && !ent.IsDestroyed)
                        {
                            ent.SetParent(null);
                            ent.Kill();
                        }
                    }

                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try 
                {
                    SaveNPCGearSetData();
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    foreach (var kvp in _userBackpack)
                    {
                        var backpack = kvp.Value;

                        if (backpack == null || backpack.IsDestroyed) continue;

                        var adjustedPos = backpack.transform.localPosition;
                        adjustedPos.y -= 300f;

                        backpack.transform.localPosition = adjustedPos;

                        backpack.EnableSaving(true);
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    if (_saveCoroutine != null)
                    {
                        StopCoroutine(_saveCoroutine);
                        PrintWarning("Canceled _saveCoroutine");
                    }

                    if (CharmPurgeCoroutine != null)
                    {
                        StopCoroutine(CharmPurgeCoroutine);
                        PrintWarning("PurgeRoutine was running on Unload(), canceled!");
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    foreach (var kvp in _userMiningMapMarkers)
                    {
                        if (kvp.Value == null) continue;

                        foreach (var marker in kvp.Value)
                        {
                            if (marker != null && !marker.IsDestroyed) marker.Kill();
                        }

                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var p = BasePlayer.activePlayerList[i];

                        if (p == null || p.gameObject == null || p.IsDestroyed) continue;

                        try { StopForgesTrigger(p); }
                        catch(Exception ex) { PrintError(ex.ToString()); }

                        try
                        {
                            var mercInfo = p?.GetComponent<MercenaryHitHandler>();
                            if (mercInfo != null)
                            {
                                try { mercInfo.TargetInfo?.DoDestroy(); }
                                finally { mercInfo.DoDestroy(); }
                            }
                        }
                        finally
                        {
                            try
                            {
                                StopSKTGUI(p);
                                StopClassGUI(p);
                                StopMagpieGUI(p);
                            }
                            finally
                            {
                                try { RemoveAllCooldownHandlers(p); }
                                finally
                                {
                                    try { OnWhirlwindEnd(p); }
                                    finally { RemoveCharmComponent(p); }
                                }
                            }

                        }
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    if (_charmToItemCache?.Count > 0)
                    {
                        foreach (var kvp in _charmToItemCache)
                        {
                            var val = kvp.Value;
                            if (val == null || (val.parent == null && ((val?.GetWorldEntity() as DroppedItem)?.IsDestroyed ?? true)))
                            {
                                charmsData.charms.Remove(val.uid.Value);
                            }
                        }
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                //it's important that if there's an error writing any of these, we still try to write some of them

                if (!_initFailed)
                {
                    try { SaveCharmData(); }
                    finally
                    {
                        try { SaveUsers(); }
                        finally
                        {
                            try { SaveClassData(); }
                            finally
                            {
                                try { SaveLegendaryData(); }
                                finally
                                {
                                    try { SaveBackpackData(); }
                                    finally { SaveLuckStatInfos(); }
                                }
                            }
                        }
                    }
                }
                else PrintWarning("init failed!!! we aren't going to save data");

                try
                {
                    foreach (var entity in BaseNetworkable.serverEntities)
                    {

                        var npc = entity as HumanNPC;
                        if (npc != null)
                        {
                            var comp = npc.GetComponent<NPCHitTarget>();
                            comp?.DoDestroy();
                        }

                        var baseEntity = entity as BaseEntity;
                        if (baseEntity.OwnerID == 528491 && (entity is RepairBench || entity.prefabID == 1560881570))
                        {
                            baseEntity.Invoke(() =>
                            {
                                if (baseEntity == null || baseEntity.IsDestroyed) return;
                                baseEntity.Kill();
                            }, 0.1f);
                        }

                        var trap = entity as WildlifeTrap;
                        if (trap == null) continue;

                        var trapExt = trap?.GetComponent<WildTrapExt>() ?? null;
                        trapExt?.DoDestroy();

                        trap.tickRate = 60f;
                        trap.SetTrapActive(trap.HasBait());
                        // trap.trapSuccessRate = 0.5f;
                        //   trap.trappedEffectRepeatRate = 30f;
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    foreach (var kvp in ActiveCraftCoroutine)
                        StopCoroutine(kvp.Value);
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
              

                try
                {
                    if (LevelsGUI != null && LevelsGUI.IsLoaded)
                        LevelsGUI.Call("RemoveSkill", "LK");
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    if (PRISMAchievements?.IsLoaded ?? false)
                    {
                        PrintWarning("hasAchievements was true, unregistering achievements");
                        PRISMAchievements.Call("UnregisterAllPluginAchievements", Name);
                        PrintWarning("Called unregister achievements");
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }
              
            }
            finally { Instance = null; }

        }
        #endregion
        #region Config
        private Dictionary<string, object> resourceMultipliers;
        private Dictionary<string, object> pointsPerHit;
        private Dictionary<string, object> messages;

        private readonly float PointsPerPickup;
        protected override void LoadDefaultConfig() { }


        protected override void LoadDefaultMessages()
        {
            var messages = new Dictionary<string, string>
            {
                //DO NOT EDIT LANGUAGE FILES HERE!
                {"upgradeSuccess", "You've successfully upgraded {0} to {1}/{2}!"},
                {"upgradeMaximum", "You've already reached the maximum level for this skill! ({0}/{0})" },
                {"upgradeNotEnough", "You do not have enough points to upgrade this! You need {0}, you have only {1}." },
                {"upgradeBadLevel", "You must be Luck level {0} or higher to upgrade this!" },
                {"upgradeRequirementNotMet", "You must have {0} upgraded to at least level {1} in order to upgrade {2}." },
                {"pointsSpent", "You spent {0} point(s), you now have {1} point(s)."},
            };
            lang.RegisterMessages(messages, this);
        }

        private string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);

        private T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        private void Init()
        {
            try
            {
                Instance = this;

                Unsubscribe(nameof(CanWearItem));
                Unsubscribe(nameof(CanEquipItem));

                permission.RegisterPermission(DISABLE_LUCK_CHAT_MESSAGES_PERM, this);
                permission.RegisterPermission(DISABLE_CLASS_NAME_IN_CHAT, this);

                _whirlwindDmgTypes.Add(_whirlwindStabDmg);
                _whirlwindDmgTypes.Add(_whirlwindSlashDmg);
                _whirlwindDmgTypes.Add(_whirlwindBluntDmg);

                string[] charmCmds = { "charm", "charms" };
                string[] topLuckCmds = { "topluck", "lucktop", "highestluck", "luckmax", "maxluck" };
                string[] classCmds = { "class", "classes", "clases", "classs", "job", "jobs", "jobe", "jobes", "role", "rol", "rols", "roles", "rolez", "jobz", "jobez", "classez", "clasz", "clazz" };

                string[] sktCmds = { "skt", "skp", "skilltrees", "skill", "st", "skilltree", "sktgui", "stkgui", "stk", "strgui" };

                string[] legendaryCmds = { "leggui", "legendarygui", "legendaryui", "leg", "legs", "leggs", "legens" };

                string[] backpackCmds = { "bp", "backpack", "bakpak", "bakpack", "bacpac", "bacpak", "backpacks", "bacpaks", "bakpacs", "bacpacs", "bps", "b" };

                AddCovalenceCommand(backpackCmds, nameof(cmdBackpacks));
                AddCovalenceCommand(charmCmds, nameof(cmdCharmInfo));
                AddCovalenceCommand(topLuckCmds, nameof(cmdTopLuck));
                AddCovalenceCommand(classCmds, nameof(cmdClass));
                AddCovalenceCommand(sktCmds, nameof(cmdSkillTrees));
                AddCovalenceCommand("toa", nameof(cmdPurchaseToken));
                AddCovalenceCommand(legendaryCmds, nameof(cmdLegendaryInfo));
                AddCovalenceCommand("legedit", nameof(cmdEditLegendary));
                AddCovalenceCommand("legcreate", nameof(cmdCreateLegendary));

                try
                {
                    if ((_statInfos = Interface.Oxide.DataFileSystem.ReadObject<List<StatInfo>>("LuckStats")) == null)
                    {
                        PrintWarning("null stats data!!");
                        _statInfos = new List<StatInfo>();
                        Interface.Oxide.DataFileSystem.WriteObject("LuckStats", _statInfos); //temp test stuff
                    }
                    else PrintWarning("loaded luckstats without issue: " + _statInfos.Count);

                    StatInfo.UpdateAllShortNames(_statInfos);

                    PrintWarning("updated all shortnames");

                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    if ((storedData3 = Interface.Oxide.DataFileSystem.ReadObject<StoredData3>("LuckData2")) == null) storedData3 = new StoredData3();

                    foreach (var kvp in storedData3.luckInfos)
                        kvp.Value.PopulateAllStatLevels();
                    
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    if ((charmsData = Interface.Oxide.DataFileSystem.ReadObject<CharmsData>("Charms")) == null)
                    {
                        PrintWarning("no charms data!!");
                        charmsData = new CharmsData();
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    if ((_userClass = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, ClassType>>("LuckUserClasses")) == null)
                    {
                        PrintWarning("null user classes data!!");
                        _userClass = new Dictionary<string, ClassType>();
                        Interface.Oxide.DataFileSystem.WriteObject("LuckUserClasses", _userClass); //temp test stuff
                    }

                }
                catch (Exception ex) { PrintError(ex.ToString() + " on loading user classes"); }

                try
                {
                    if ((_legendaryData = Interface.Oxide.DataFileSystem.ReadObject<LegendaryItemData>("LuckLegendaries")) == null)
                    {
                        PrintWarning("null legendaries data!!");
                        _legendaryData = new LegendaryItemData();
                        Interface.Oxide.DataFileSystem.WriteObject("LuckLegendaries", _legendaryData); //temp test stuff
                    }

                    if (_legendaryData.Infos.Count < 1)
                    {
                        _legendaryData.Infos.Add(new LegendaryItemInfo(0, 0));
                        SaveLegendaryData();
                        PrintWarning("added placeholder legendary!");
                    }
                    else
                        LegendaryItemInfo.UpdateAllCaches(_legendaryData.Infos); // _legendaries);
                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {

                    if ((_npcGearData = Interface.Oxide.DataFileSystem.ReadObject<NPCGearSetData>("LuckNPCGearSets")) == null)
                    {
                        PrintWarning("npc gear sets null! generating");
                        _npcGearData = new NPCGearSetData();
                        Interface.Oxide.DataFileSystem.WriteObject("LuckNPCGearSets", _npcGearData);
                    }

                    var shouldSave = false;

                    if (_npcGearData.TierGears.Count < 1)
                    {
                        shouldSave = true;
                        _npcGearData.TierGears[NPCHitTarget.Tiers.Elite] = new NPCGearSet();

 
                    }

                    var eliteData = _npcGearData.TierGears[NPCHitTarget.Tiers.Elite];


                    if (eliteData.ClothingOptions.Count < 1)
                    {
                        Interface.Oxide.LogWarning("adding example clothing options to elite tier");
                        eliteData.AddClothingOption(NPCGearSet.WearType.Chest, new WeightedItemInfo(1751045826, 0.125f));
                        eliteData.AddClothingOption(NPCGearSet.WearType.Legs, new WeightedItemInfo(237239288, 0.125f));

                    }

                    if (eliteData.WeaponOptions.Count < 1)
                    {
                        Interface.Oxide.LogWarning("adding example weapon options to elite tier");
                        eliteData.WeaponOptions.Add(new WeightedItemInfo(1545779598, 0.125f));
                        eliteData.WeaponOptions.Add(new WeightedItemInfo(-1214542497, 0.125f));
                    }

                    if (shouldSave)
                        SaveNPCGearSetData();

                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                try
                {
                    if ((_itemUIDToBackPackContainerID = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, ulong>>("LuckBackpacks")) == null)
                    {
                        PrintWarning("null backpack data!");
                        _itemUIDToBackPackContainerID = new Dictionary<ulong, ulong>();

                        Interface.Oxide.DataFileSystem.WriteObject("LuckBackpacks", _itemUIDToBackPackContainerID);
                    }
                    else PrintWarning("Loaded backpack data (temp debug)!");
                }
                catch (Exception ex) { PrintError(ex.ToString()); }


                resourceMultipliers = checkCfg("ResourcePerLevelMultiplier", new Dictionary<string, object>{
                {LUCK_SHORTNAME, 2.0d}
            });

                pointsPerHit = checkCfg("PointsPerHit", new Dictionary<string, object>{
                {LUCK_SHORTNAME, 50f}
            });


                messages = checkCfg("Messages", new Dictionary<string, object>{
                {"StatsHeadline", "Level stats (/statinfo [statname] - To get more information about skill)"},
                {"StatsText",   "-{0}"+
                            "\nLevel: {1} (+{4}% bonus) \nXP: {2}/{3} [{5}].\n<color=red>-{6} XP lost on death.</color>"},
                {"LevelUpText", "{0} Level up"+
                            "\nLevel: {1} (+{4}% bonus) \nXP: {2}/{3}"},
                {"LKSkill", "Luck"}
            });
                SaveConfig();
                LoadDefaultMessages();
            }
            catch (Exception ex)
            {
                _initFailed = true;
                PrintError(ex.ToString());
                PrintError("Failed to complete Init()");
                PrintError("!!!!!!!!INIT FAILED\nINIT FAILED\nINIT FAILED\nDATA WILL NOT BE SAVED ON UNLOAD\nDATA WILL NOT BE SAVED ON UNLOAD!!!!!!");
            }

        }

        private T checkCfg<T>(string conf, T def)
        {
            if (Config[conf] != null) { return (T)Config[conf]; }
            else
            {
                Config[conf] = def;
                return def;
            }
        }
        #endregion
        #region Gets&Sets
        private long getLevel(ulong userID) { return GetLuckInfo(userID)?.Level ?? 0; }

        private long getLevelS(string userID) { return GetLuckInfo(userID)?.Level ?? 0; }

        private float getPoints(ulong userID) { return GetLuckInfo(userID)?.XP ?? 0f; }

        private float getPointsS(string userID) { return GetLuckInfo(userID)?.XP ?? 0f; }

        private int getLKSKP(ulong userID) { return GetLuckInfo(userID)?.SkillPoints ?? 0; }

        #endregion
        #region New stuff

        private long getPointsNeededForNextLevel(long level) { return GetPointsForLevel(level + 1) - GetPointsForLevel(level); }

        private double GetExperiencePercent(BasePlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            long Level = getLevel(player.userID);
            long startingPoints = GetPointsForLevel(Level);
            long nextLevelPoints = GetPointsForLevel(Level + 1) - startingPoints;
            var Points = getPoints(player.userID) - startingPoints;
            return Mathf.Clamp(Points / nextLevelPoints * 100, 0.5f, 99.9f);
        }

        private string GetExperiencePercentString(BasePlayer player, string format = "N0")
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            var sb = Pool.Get<StringBuilder>();
            try { return sb.Clear().Append(GetExperiencePercent(player).ToString(format)).Append("%").ToString(); }
            finally { Pool.Free(ref sb); }
        }

        private LegendaryItemInfo FindLegendaryItemInfoByNameOrID(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));


            LegendaryItemInfo foundInfo = null;

            for (int i = 0; i < _legendaryData.Infos.Count; i++)
            {
                var info = _legendaryData.Infos[i];

                if (info == null)
                    continue;



                if (info.ItemID.ToString().Equals(name) || info.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase) || info.DisplayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (foundInfo != null)
                    {
                        PrintWarning("multiple matches, foundInfo wasn't null!!!");
                        return null;
                    }

                    foundInfo = info;
                }
            }

            return foundInfo;
        }

        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            BasePlayer player = null;
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                return null;
            }
            return player;
        }

        private void ClearTeam(BasePlayer player)
        {
            if (player == null || player.currentTeam == 0) return;

            var team = RelationshipManager.ServerInstance?.FindTeam(player.currentTeam) ?? null;

            if (team == null || Interface.Oxide.CallHook("OnTeamLeave", team, player) != null) return;

            team.RemovePlayer(player.userID);

            player.ClearTeam();
        }


        #endregion

        #region Custom Networking
        public void SendNetworkUpdate(BaseNetworkable entity, BasePlayer.NetworkQueue queue = BasePlayer.NetworkQueue.Update, BasePlayer target = null)
        {
            if (entity == null || entity.IsDestroyed || entity.net == null || !entity.isSpawned) return;
            if (target != null && (target?.net?.connection == null || !target.IsConnected)) return;

            entity.LogEntry(BaseMonoBehaviour.LogEntryType.Network, 2, "SendNetworkUpdate");
            entity.InvalidateNetworkCache();

            if (target == null)
            {
                var subscribers = entity?.net?.group?.subscribers ?? null;
                if (subscribers != null && subscribers.Count > 0)
                {
                    for (int i = 0; i < subscribers.Count; i++)
                    {
                        var ply = subscribers[i]?.player as BasePlayer;
                        if (ply != null && entity.ShouldNetworkTo(ply)) ply.QueueUpdate(queue, entity);
                    }
                }
            }
            else target.QueueUpdate(queue, entity);

            if (entity?.gameObject != null) entity.gameObject.SendOnSendNetworkUpdate(entity as BaseEntity);
        }

        /// <summary>
        /// Forcefully send entity updates to specified connections. For performance reasons, this does NOT run the CanNetworkTo check. Handle this yourself or don't use this.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queue"></param>
        /// <param name="connections"></param>
        public void SendNetworkUpdate(BaseNetworkable entity, BasePlayer.NetworkQueue queue = BasePlayer.NetworkQueue.Update, List<Connection> connections = null)
        {
            if (entity == null || entity.IsDestroyed || entity.net == null || !entity.isSpawned) return;
            entity.LogEntry(BaseMonoBehaviour.LogEntryType.Network, 2, "SendNetworkUpdate");
            entity.InvalidateNetworkCache();
            var subscribers = (connections != null && connections.Count > 0) ? connections : entity?.net?.group?.subscribers ?? null;
            if (subscribers != null && subscribers.Count > 0)
            {
                for (int i = 0; i < subscribers.Count; i++)
                {
                    var ply = subscribers[i]?.player as BasePlayer;
                    if (ply != null && ply?.net?.connection != null) ply.QueueUpdate(queue, entity);
                }
            }
            if (entity?.gameObject != null) entity.gameObject.SendOnSendNetworkUpdate(entity as BaseEntity);
        }
        #endregion

        #region Weather Manipulation
        private void GetClearWeatherVarsNoAlloc(ref List<ClientVarWithValue> vars)
        {
            if (vars == null) throw new ArgumentNullException(nameof(vars));

            var rainVar = ClientVarWithValue.GetOrCreateClientVar("weather.rain", "0");// //new ClientVarWithValue("weather.rain", "0");
            var fogVar = ClientVarWithValue.GetOrCreateClientVar("weather.fog", "0");
            var cloudsVar = ClientVarWithValue.GetOrCreateClientVar("weather.cloud_coverage", "0");
            var thunderVar = ClientVarWithValue.GetOrCreateClientVar("weather.thunder", "0");
            var windVar = ClientVarWithValue.GetOrCreateClientVar("weather.wind", "0");
            var atmosphereContrastVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", "1.2");
            var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_brightness", "1");
            var atmosphereDirectionalityVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_directionality", "0.9");
            var atmosphereMieVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_mie", "1");
            var atmosphereRayleighVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_rayleigh", "1");
            //   var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", "1.2");
            var rainbowVar = ClientVarWithValue.GetOrCreateClientVar("weather.rainbow", "0");


            vars.Add(rainVar);
            vars.Add(fogVar);
            vars.Add(cloudsVar);
            vars.Add(thunderVar);
            vars.Add(windVar);
            vars.Add(atmosphereContrastVar);
            vars.Add(rainbowVar);
            vars.Add(atmosphereBrightnessVar);
            vars.Add(atmosphereDirectionalityVar);
            vars.Add(atmosphereMieVar);
            vars.Add(atmosphereRayleighVar);
        }

        private void GetStormWeatherVarsNoAlloc(ref List<ClientVarWithValue> vars)
        {
            if (vars == null) throw new ArgumentNullException(nameof(vars));

            var rainVar = ClientVarWithValue.GetOrCreateClientVar("weather.rain", "1");// //new ClientVarWithValue("weather.rain", "0");
            var fogVar = ClientVarWithValue.GetOrCreateClientVar("weather.fog", "1");
            var cloudsVar = ClientVarWithValue.GetOrCreateClientVar("weather.cloud_coverage", "1");
            var thunderVar = ClientVarWithValue.GetOrCreateClientVar("weather.thunder", "1");
            var windVar = ClientVarWithValue.GetOrCreateClientVar("weather.wind", "1");
            var atmosphereContrastVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", "1.67");
            var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_brightness", "0.3");
            var atmosphereDirectionalityVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_directionality", "0.9");
            var atmosphereMieVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_mie", "1");
            var atmosphereRayleighVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_rayleigh", "1");
            //   var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", "1.2");
            var rainbowVar = ClientVarWithValue.GetOrCreateClientVar("weather.rainbow", "0");


            vars.Add(rainVar);
            vars.Add(fogVar);
            vars.Add(cloudsVar);
            vars.Add(thunderVar);
            vars.Add(windVar);
            vars.Add(atmosphereContrastVar);
            vars.Add(rainbowVar);
            vars.Add(atmosphereBrightnessVar);
            vars.Add(atmosphereDirectionalityVar);
            vars.Add(atmosphereMieVar);
            vars.Add(atmosphereRayleighVar);
        }

        private void GetCurrentWeatherVarsNoAlloc(ref List<ClientVarWithValue> vars, Vector3 pos = default(Vector3))
        {
            if (vars == null) throw new ArgumentNullException(nameof(vars));

            var rainVar = ClientVarWithValue.GetOrCreateClientVar("weather.rain", Climate.GetRain(pos).ToString());// //new ClientVarWithValue("weather.rain", "0");
            var fogVar = ClientVarWithValue.GetOrCreateClientVar("weather.fog", Climate.GetFog(pos).ToString());
            var cloudsVar = ClientVarWithValue.GetOrCreateClientVar("weather.cloud_coverage", Climate.GetClouds(pos).ToString());
            var thunderVar = ClientVarWithValue.GetOrCreateClientVar("weather.thunder", Climate.GetThunder(pos).ToString());
            var windVar = ClientVarWithValue.GetOrCreateClientVar("weather.wind", Climate.GetWind(pos).ToString());
            var atmosphereContrastVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", Climate.Instance.WeatherState.Atmosphere.Contrast.ToString());
            var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_brightness", Climate.Instance.WeatherState.Atmosphere.Brightness.ToString());
            var atmosphereDirectionalityVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_directionality", Climate.Instance.WeatherState.Atmosphere.Directionality.ToString());
            var atmosphereMieVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_mie", Climate.Instance.WeatherState.Atmosphere.MieMultiplier.ToString());
            var atmosphereRayleighVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_rayleigh", Climate.Instance.WeatherState.Atmosphere.RayleighMultiplier.ToString());
            //   var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", "1.2");
            var rainbowVar = ClientVarWithValue.GetOrCreateClientVar("weather.rainbow", Climate.GetRainbow(pos).ToString());


            vars.Add(rainVar);
            vars.Add(fogVar);
            vars.Add(cloudsVar);
            vars.Add(thunderVar);
            vars.Add(windVar);
            vars.Add(atmosphereContrastVar);
            vars.Add(rainbowVar);
            vars.Add(atmosphereBrightnessVar);
            vars.Add(atmosphereDirectionalityVar);
            vars.Add(atmosphereMieVar);
            vars.Add(atmosphereRayleighVar);
        }

        private class ClientVarWithValue
        {
            public string FullCommandName { get; set; }
            public string VariableValue { get; set; } = string.Empty;

            public static List<ClientVarWithValue> All { get; set; } = new List<ClientVarWithValue>();

            public ClientVarWithValue()
            {
                All.Add(this);
            }
            public ClientVarWithValue(string commandName, string variableValue)
            {
                if (string.IsNullOrEmpty(commandName)) throw new ArgumentNullException(nameof(commandName));

                FullCommandName = commandName;
                VariableValue = variableValue;

                All.Add(this);
            }

            public static ClientVarWithValue GetOrCreateClientVar(string fullCommandName, string setVariableValue = "")
            {
                if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

                for (int i = 0; i < All.Count; i++)
                {
                    var var = All[i];
                    if (var.FullCommandName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(setVariableValue)) var.VariableValue = setVariableValue;
                        return var;

                    }
                }

                return new ClientVarWithValue(fullCommandName, setVariableValue);
            }
        }

        private void SendReplicatedVarsToConnection(Connection connection, string filter = "")
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var cmds = Pool.GetList<ConsoleSystem.Command>();

            try
            {
                for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
                {
                    var cmd = ConsoleSystem.Index.Server.Replicated[i];
                    if (string.IsNullOrEmpty(filter) || cmd.FullName.StartsWith(filter)) cmds.Add(cmd);

                }

                if (cmds.Count < 1)
                {
                    PrintWarning("No replicated commands found with filter: " + filter);
                    return;
                }

                var netWrite = Net.sv.StartWrite();

                netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
                netWrite.Int32(cmds.Count);

                for (int i = 0; i < cmds.Count; i++)
                {
                    var var = cmds[i];
                    netWrite.String(var.FullName);
                    netWrite.String(var.String);
                }


                netWrite.Send(new SendInfo(connection));

            }
            finally { Pool.FreeList(ref cmds); }


        }

        private void SendClientsVar(List<Connection> connections, List<ClientVarWithValue> vars)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));

            if (connections.Count < 1) throw new ArgumentOutOfRangeException(nameof(connections));

            if (vars == null) throw new ArgumentNullException(nameof(vars));
            if (vars.Count < 1) throw new ArgumentOutOfRangeException(nameof(vars));

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(vars.Count);

            for (int i = 0; i < vars.Count; i++)
            {
                var var = vars[i];
                netWrite.String(var.FullCommandName);
                netWrite.String(var.VariableValue);
            }


            netWrite.Send(new SendInfo(connections));

        }

        private void SendClientVars(Connection connection, List<ClientVarWithValue> vars)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (vars == null) throw new ArgumentNullException(nameof(vars));
            if (vars.Count < 1) throw new ArgumentOutOfRangeException(nameof(vars));


            var netWrite = Net.sv.StartWrite();


            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(vars.Count);

            for (int i = 0; i < vars.Count; i++)
            {
                var var = vars[i];
                netWrite.String(var.FullCommandName);
                netWrite.String(var.VariableValue);
            }


            netWrite.Send(new SendInfo(connection));

        }

        private void SendClientVar(Connection connection, string fullCommandName, string varValue)
        {
            if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

            var foundCmd = false;
            for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
            {
                var cmd = ConsoleSystem.Index.Server.Replicated[i];
                if (cmd.FullName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    foundCmd = true;
                    fullCommandName = cmd.FullName;
                    break;
                }
            }

            if (!foundCmd)
            {
                PrintWarning(nameof(SendClientVar) + " failed to find replicated var!");
                return;
            }

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(1);

            netWrite.String(fullCommandName);
            netWrite.String(varValue);

            netWrite.Send(new SendInfo(connection));

        }

        private void SendClientsVar(List<Connection> connections, string fullCommandName, string varValue)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));

            if (connections.Count < 1) throw new ArgumentOutOfRangeException(nameof(connections));

            if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

            var foundCmd = false;
            for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
            {
                var cmd = ConsoleSystem.Index.Server.Replicated[i];
                if (cmd.FullName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    foundCmd = true;
                    fullCommandName = cmd.FullName;
                    break;
                }
            }

            if (!foundCmd)
            {
                PrintWarning(nameof(SendClientsVar) + " failed to find replicated var!");
                return;
            }

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(1);

            netWrite.String(fullCommandName);
            netWrite.String(varValue);

            netWrite.Send(new SendInfo(connections));
        }
        #endregion


    }
}
