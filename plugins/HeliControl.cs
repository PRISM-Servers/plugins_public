using Oxide.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core.Plugins;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Linq;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("HeliControl", "Shady", "1.4.6", ResourceId = 1348)]
    [Description("Tweak various settings of helicopters.")]
    internal class HeliControl : RustPlugin
    {
        //Soli Deo gloria
        #region Constants
        private const uint ROCKET_PREFAB_ID = 129320027;
        private const uint AIRBURST_ROCKET_PREFAB_ID = 3253859536;
        private const uint NAPALM_ROCKET_PREFAB_ID = 200672762;
        private const uint HELI_CRATE_PREFAB_ID = 1314849795;
        private const uint NAPALM_FIREBALL_PREFAB_ID = 184893264;
        private const uint OIL_FIREBALL_PREFAB_ID = 3550347674;
        private const uint CHINOOK_EVENT_PREFAB_ID = 1514383717;
        private const uint HELI_EVENT_PREFAB_ID = 3029415845;

        private const uint CHINOOK_SCIENTISTS_PREFAB_ID = 1514383717;

        private const string HELI_PREFAB = "assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab";
        private const string CHINOOK_PREFAB = "assets/prefabs/npc/ch47/ch47scientists.entity.prefab";
        #endregion
        #region Fields
        #region Plugin References
        [PluginReference]
        private readonly Plugin Vanish;
        #endregion

        private PatrolHelicopterAI HeliInstance
        {
            get { return PatrolHelicopterAI.heliInstance; }
            set { PatrolHelicopterAI.heliInstance = value; }
        }

        private CH47HelicopterAIController CH47Instance { get; set; }

        private float _lastSpawnTimer;
        private float _lastSpawnTimerCH47;
        private DateTime _lastTimerStart;
        private DateTime _lastTimerStartCH47;
        private Timer _callTimer;
        private Timer _callTimerCH47;

        private Timer CallTimer
        {
            get { return _callTimer; }
            set
            {
                _callTimer?.Destroy();
                _lastTimerStart = DateTime.UtcNow;
                _lastSpawnTimer = value.Delay;
                _callTimer = value;
            }
        }

        private Timer CallTimerCH47
        {
            get { return _callTimerCH47; }
            set
            {
                _callTimerCH47?.Destroy();
                _lastTimerStartCH47 = DateTime.UtcNow;
                _lastSpawnTimerCH47 = value.Delay;
                _callTimerCH47 = value;
            }
        }

        private TriggeredEventPrefab[] _eventPrefabs = null;
        public TriggeredEventPrefab[] EventPrefabs
        {
            get
            {
                _eventPrefabs ??= UnityEngine.Object.FindObjectsOfType<TriggeredEventPrefab>();

                return _eventPrefabs;
            }
        }

        private NetworkableId _timerHeliId;
        private CH47HelicopterAIController _timerCH47;

        private bool _configChanged = false;
        private bool _terrainHookCalled = false;
        private bool _useNapalm = false; //not a config option!
        private bool _init;


        private StoredData _lootData = new();
        private StoredData2 _weaponsData = new();
        private StoredData3 _cooldownData = new();
        private StoredData4 _spawnsData = new();

        private float _boundary;

        private readonly HashSet<PatrolHelicopter> _PatrolHelicopters = new();
        private readonly HashSet<CH47HelicopterAIController> _chinooks = new();
        private readonly HashSet<HelicopterDebris> _gibs = new();
        private readonly HashSet<FireBall> _fireBalls = new();
        private readonly HashSet<PatrolHelicopter> _forceCalled = new();
        private readonly HashSet<CH47HelicopterAIController> _forceCalledCh = new();
        private readonly HashSet<LockedByEntCrate> _lockedCrates = new();
        private readonly HashSet<HackableLockedCrate> _hackLockedCrates = new();

        private readonly Dictionary<PatrolHelicopter, int> _strafeCount = new();


        private static readonly System.Random _rng = new(); //used for loot crates

        private readonly int _groundLayer = LayerMask.GetMask("Terrain", "World", "Default");

        #endregion
        #region Config Fields


        private bool DisableHeli;
        private bool DisableDefaultHeliSpawns;
        private bool DisableDefaultChinookSpawns;
        private bool UseCustomLoot;
        private bool DisableGibs;
        private bool DisableNapalm;
        private bool AutoCallIfExists;
        private bool AutoCallIfExistsCH47;
        private bool DisableCratesDeath;
        private bool HelicopterCanShootWhileDying;
        private bool UseCustomHeliSpawns;
        private bool UseOldSpawning;
        private bool UseOldSpawningCH47;
        private bool SpawnHeliOnRestart;
        private bool SpawnChinookOnRestart;
        private bool SpawnHeliOnTarget;
        private float GlobalDamageMultiplier;
        private float HeliBulletDamageAmount;
        private float MainRotorHealth;
        private float TailRotorHealth;
        private float BaseHealth;
        private float BaseChinookHealth;
        private float HeliSpeed;
        private float HeliStartSpeed;
        private float HeliStartLength;
        private float HeliAccuracy;
        private float TimeBeforeUnlocking;
        private float TimeBeforeUnlockingHack;
        private float TurretFireRate;
        private float TurretBurstLength;
        private float TurretTimeBetweenBursts;
        private float TurretMaxRange;
        private float GibsTooHotLength;
        private float GibsHealth;
        private float TimeBetweenRockets;
        private float MinSpawnTime;
        private float MinSpawnTimeCH47;
        private float MaxSpawnTime;
        private float MaxSpawnTimeCH47;
        private float RocketDamageBlunt;
        private float RocketDamageExplosion;
        private float RocketExplosionRadius;
        private int MaxLootCrates;
        private int MaxHeliRockets;
        private int BulletSpeed;
        private int LifeTimeMinutes;
        private int LifeTimeMinutesCH47;
        private int MaxActiveHelicopters;
        private int HelicoptersToSpawn;
        private int ChinooksToSpawn;

        private Dictionary<string, float> Cds;

        private Dictionary<string, int> Limits;
        #endregion




        #region Config
        protected override void LoadDefaultConfig()
        {

            //this cooldown and limit garbage is to fix an issue that was causing duplicate entries in the config.
            //I hate it, but it was necessary unless I wanted to ask people to reset or clean up their own configs.
            var _coolDowns = GetConfig("Cooldowns", new Dictionary<string, object>());

            var _limits = GetConfig("Limits", new Dictionary<string, object>());

            Cds = new Dictionary<string, float>();
            foreach (var kvp in _coolDowns) Cds[kvp.Key.ToLower()] = Convert.ToSingle(kvp.Value);

            Limits = new Dictionary<string, int>();
            foreach (var kvp in _limits) Limits[kvp.Key.ToLower()] = Convert.ToInt32(kvp.Value);

            if (Limits.Count < _limits.Count || Cds.Count < _coolDowns.Count) _configChanged = true;

            var sb = Pool.Get<StringBuilder>();

            try 
            {
                for (int i = 0; i < 10; i++)
                {

                    var cd = sb.Clear().Append("cooldown.").Append(i).ToString();
                    var limit = sb.Clear().Append("limit.").Append(i).ToString();
                    var cdCh47 = sb.Clear().Append("cooldown.ch47.").Append(i).ToString();
                    var limitCh47 = sb.Clear().Append("limit.ch47.").Append(i).ToString();

                    if (!Cds.TryGetValue(cd, out _))
                        Cds[cd] = 86400f;

                    if (!Limits.TryGetValue(limit, out _))
                            Limits[limit] = 5;

                    if (!Cds.TryGetValue(cdCh47, out _))
                        Cds[cdCh47] = 86400f;

                    if (!Limits.TryGetValue(limitCh47, out _))
                        Limits[limitCh47] = 5;
                }
            }
            finally { Pool.Free(ref sb); }

          

            Config["Cooldowns"] = Cds;
            Config["Limits"] = Limits;


            DisableHeli = GetConfig("Spawning - Disable Helicopter", false);
            DisableDefaultHeliSpawns = GetConfig("Spawning - Disable Rust's default spawns", false);
            DisableDefaultChinookSpawns = GetConfig("Spawning - Disable CH47 default spawns", false);
            UseCustomLoot = GetConfig("Loot - Use Custom loot spawns", false);
            GlobalDamageMultiplier = GetConfig("Damage - Global damage multiplier", 1f);
            HeliBulletDamageAmount = GetConfig("Turrets - Helicopter bullet damage", 20f);
            HelicopterCanShootWhileDying = GetConfig("Misc - Helicopter can shoot while dying", true);
            MainRotorHealth = GetConfig("Health - Main rotor health", 750f);
            TailRotorHealth = GetConfig("Health - Tail rotor health", 375f);
            BaseHealth = GetConfig("Health - Base Helicopter health", 10000f);
            BaseChinookHealth = GetConfig("Health - Base CH47 health", 4000f);
            MaxLootCrates = GetConfig("Loot - Max Crates to drop", 4);
            HeliSpeed = GetConfig("Misc - Helicopter speed", 25f);
            HeliAccuracy = GetConfig("Turrets - Helicopter bullet accuracy", 2f);
            MaxHeliRockets = GetConfig("Rockets - Max helicopter rockets", 12);
            DisableGibs = GetConfig("Spawning - Disable helicopter gibs", false);
            DisableNapalm = GetConfig("Spawning - Disable helicopter napalm", false);
            BulletSpeed = GetConfig("Turrets - Helicopter bullet speed", 250);
            TimeBeforeUnlocking = GetConfig("Loot - Time before unlocking crates", -1f);
            TimeBeforeUnlockingHack = GetConfig("Loot - Time before unlocking CH47 crates", -1f);
            LifeTimeMinutes = GetConfig("Misc - Maximum helicopter life time in minutes", 15);
            LifeTimeMinutesCH47 = GetConfig("Misc - Maximum helicopter life time in minutes", 15);
            TimeBetweenRockets = GetConfig("Rockets - Time between each rocket in seconds", 0.2f);
            TurretFireRate = GetConfig("Turrets - Turret fire rate in seconds", 0.125f);
            TurretBurstLength = GetConfig("Turrets - Turret burst length in seconds", 3f);
            TurretTimeBetweenBursts = GetConfig("Turrets - Time between turret bursts in seconds", 3f);
            TurretMaxRange = GetConfig("Turrets - Max range", 300f);
            RocketDamageBlunt = GetConfig("Rockets - Blunt damage to deal", 175f);
            RocketDamageExplosion = GetConfig("Rockets - Explosion damage to deal", 100f);
            RocketExplosionRadius = GetConfig("Rockets - Explosion radius", 6f);
            GibsTooHotLength = GetConfig("Gibs - Time until gibs can be harvested in seconds", 480f);
            GibsHealth = GetConfig("Gibs - Health of gibs", 500f);
            MinSpawnTime = GetConfig("Spawning - Automatically call helicopter between min seconds", 0f);
            MaxSpawnTime = GetConfig("Spawning - Automatically call helicopter between max seconds", 0f);
            MinSpawnTimeCH47 = GetConfig("Spawning - Automatically call CH47 between min seconds", 0f);
            MaxSpawnTimeCH47 = GetConfig("Spawning - Automatically call CH47 between max seconds", 0f);
            UseOldSpawning = GetConfig("Spawning - Use static spawning", false);
            UseOldSpawningCH47 = GetConfig("Spawning - Use static spawning for CH47", false);
            SpawnHeliOnRestart = GetConfig("Spawning - Spawn helicopter after restart", false);
            SpawnChinookOnRestart = GetConfig("Spawning - Spawn CH47 after restart", false);
            AutoCallIfExists = GetConfig("Spawning - Automatically call helicopter if one is already flying", false);
            SpawnHeliOnTarget = GetConfig("Spawning - Spawn helicopters directly on target", true);
            AutoCallIfExistsCH47 = GetConfig("Spawning - Automatically call CH47 if one is already flying", false);
            HelicoptersToSpawn = GetConfig("Spawning - Helicopters to spawn", 1);
            ChinooksToSpawn = GetConfig("Spawning - Chinooks to spawn", 1);
            UseCustomHeliSpawns = GetConfig("Spawning - Use custom helicopter spawns", false);
            HeliStartSpeed = GetConfig("Misc - Helicopter startup speed", 25f);
            HeliStartLength = GetConfig("Misc - Helicopter startup length in seconds", 0f);
            DisableCratesDeath = GetConfig("Misc - Prevent crates from spawning when forcefully killing helicopter", true);
            MaxActiveHelicopters = GetConfig("Spawning - Max active helicopters", -1);
        }
        protected override void LoadDefaultMessages()
        {
            var messages = new Dictionary<string, string>
            {
                //DO NOT EDIT LANGUAGE FILES HERE! Navigate to oxide\lang
                {"noPerms", "You do not have permission to use this command!"},
                {"invalidSyntax", "Invalid Syntax, usage example: {0} {1}"},
                {"invalidSyntaxMultiple", "Invalid Syntax, usage example: {0} {1} or {2} {3}"},
                {"heliCalled", "Helicopter Inbound!"},
                {"helisCalledPlayer", "{0} Helicopter(s) called on: {1}"},
                {"entityDestroyed", "{0} {1}(s) were annihilated!"},
                {"helisForceDestroyed", "{0} Helicopter(s) were forcefully destroyed!"},
                {"heliAutoDestroyed", "Helicopter auto-destroyed because config has it disabled!" },
                {"playerNotFound", "Could not find player: {0}"},
                {"noHelisFound", "No active helicopters were found!"},
                {"cannotBeCalled", "This can only be called on a single Helicopter, there are: {0} active."},
                {"strafingOtherPosition", "Helicopter is now strafing {0}'s position."},
                {"destinationOtherPosition", "Helicopter's destination has been set to {0}'s position."},
                {"IDnotFound", "Could not find player by ID: {0}" },
                {"updatedHelis", "{0} helicopters were updated successfully!" },
                {"callheliCooldown", "You must wait before using this again! You've waited: {0}/{1}" },
                {"invalidCoordinate", "Incorrect argument supplied for {0} coordinate!" },
                {"coordinatesOutOfBoundaries", "Coordinates are out of map boundaries!" },
                {"callheliLimit", "You've used your daily limit of {0} heli calls!" },
                {"unlockedAllCrates", "Unlocked all Helicopter crates!" },
                {"teleportedToHeli", "You've been teleported to the ground below the active Helicopter!" },
                {"removeAddSpawn", "To remove a Spawn, type: /helispawn remove SpawnName\n\nTo add a Spawn, type: /helispawn add SpawnName -- This will add the spawn on your current position." },
                {"addedSpawn", "Added helicopter spawn {0} with the position of: {1}" },
                {"spawnExists", "A spawn point with this name already exists!" },
                {"noSpawnsExist", "No Helicopter spawns have been created!" },
                {"removedSpawn", "Removed Helicopter spawn point: {0}: {1}" },
                {"noSpawnFound", "No spawn could be found with that name!" },
                {"onlyCallSelf", "You can only call a Helicopter on yourself, try: /callheli {0}" },
                {"spawnCommandLiner", "<color=orange>----</color>Spawns<color=orange>----</color>\n" },
                {"spawnCommandBottom", "\n<color=orange>----------------</color>" },
                {"cantCallTargetOrSelf", "You do not have the permission to call a Helicopter on a target! Try: /callheli" },
                {"maxHelis", "Killing helicopter because the maximum active helicopters has been reached" },
                {"cmdError", "An error happened while using this command. Please report this to your server administrator." },
                {"ch47AlreadyDropped", "This CH47 has already dropped a crate!" },
                {"ch47DroppedCrate", "Dropped crate!" },
                {"noTimeFound", "No spawn time found for helicopter." },
                {"noTimeFoundCH47", "No spawn time found for CH47." },
                {"nextHeliSpawn", "Next helicopter spawn: {0}" },
                {"nextCH47Spawn", "Next CH47 spawn: {0}" },
                {"nextAlreadyActive", "A helicopter is already active." },
                {"tooManyActiveHelis", "The maximum amount of Helicopters active has been reached. Please try again later." },
                {"itemNotFound", "Item not found!" },
            };
            lang.RegisterMessages(messages, this);
        }
        private string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);
        #endregion

        #region Hooks
        #region Init
        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));

            _cooldownData = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData3>("HeliControlCooldowns") ?? new StoredData3();

            if (!_configChanged) LoadDefaultConfig(); //don't call load again if it was already called because config file didn't exist. tiny optimization

            if (_configChanged) SaveConfig(); //config could have been changed after a manual call of loaddefaultconfig, so we check again

            LoadDefaultMessages();


            string[] perms = { "callheli", "callheliself", "callhelitarget", "callch47", "callch47self", "callch47target", "killch47", "killheli", "strafe", "update", "destination", "dropcrate", "killnapalm", "killgibs", "unlockcrates", "admin", "ignorecooldown", "ignorelimits", "tpheli", "tpch47", "helispawn", "callmultiple", "callmultiplech47", "dropcrates", "nextheli" };


            for (int j = 0; j < perms.Length; j++) permission.RegisterPermission("helicontrol." + perms[j], this);
            foreach (var limit in Limits.Keys) permission.RegisterPermission("helicontrol." + limit, this);
            foreach (var cd in Cds.Keys) permission.RegisterPermission("helicontrol." + cd, this);

            if (HelicopterCanShootWhileDying)
            {
                Unsubscribe(nameof(CanBeTargeted));
                Unsubscribe(nameof(OnHelicopterTarget));
                Unsubscribe(nameof(CanHelicopterStrafeTarget));
                Unsubscribe(nameof(CanHelicopterStrafe));
                Unsubscribe(nameof(CanHelicopterTarget));
            }

            if (!DisableNapalm) Unsubscribe(nameof(CanHelicopterUseNapalm));

            AddCovalenceCommand("unlockcrates", nameof(cmdUnlockCrates));
            AddCovalenceCommand("tpheli", nameof(cmdTeleportHeli));
            AddCovalenceCommand("killheli", nameof(cmdKillHeli));
            AddCovalenceCommand("killch47", nameof(cmdKillCH47));
            AddCovalenceCommand("dropcrate", nameof(cmdDropCH47Crate));
            AddCovalenceCommand("updatehelis", nameof(cmdUpdateHelicopters));
            AddCovalenceCommand("strafe", nameof(cmdStrafeHeli));
            AddCovalenceCommand("helidest", nameof(cmdDestChangeHeli));
            AddCovalenceCommand("killnapalm", nameof(cmdKillFB));
            AddCovalenceCommand("killgibs", nameof(cmdKillGibs));
            AddCovalenceCommand("nextheli", nameof(cmdNextHeli));

            AddCovalenceCommand("hcdebug", nameof(cmdDebug));

        }


        private void OnTerrainInitialized()
        {
            try
            {
                if (DisableDefaultHeliSpawns) ToggleHeliEvent(false); //OnServerInit may be too late to disable the event
                if (DisableDefaultChinookSpawns) ToggleCH47Event(false);
            }
            finally { _terrainHookCalled = true; }
        }

        private void OnServerInitialized()
        {
            try
            {
                _boundary = TerrainMeta.Size.x * 0.75f;
                Subscribe(nameof(OnEntitySpawned));

                ServerMgr.Instance?.InvokeRepeating(CheckHelicopter, 10f, 10f); //ServerMgr.Instance should never be null after server init, but we check anyway

                foreach (var entity in BaseNetworkable.serverEntities)
                {
                    if (entity == null) continue;
                    var heli = entity as PatrolHelicopter;
                    var ch47 = entity as CH47HelicopterAIController;
                    var crate = entity as LockedByEntCrate;
                    var hackCrate = entity as HackableLockedCrate;
                    var debris = entity as HelicopterDebris;
                    var fireball = entity as FireBall;
                    if (heli != null)
                    {
                        _PatrolHelicopters.Add(heli);
                        UpdateHeli(heli);
                    }

                    if (ch47?.prefabID == CHINOOK_SCIENTISTS_PREFAB_ID)
                    {
                        _chinooks.Add(ch47);
                        UpdateChinook(ch47);
                    }

                    if (crate != null) _lockedCrates.Add(crate);
                    if (hackCrate != null) _hackLockedCrates.Add(hackCrate);
                    if (debris != null) _gibs.Add(debris);
                    if (fireball != null && (fireball.prefabID == NAPALM_FIREBALL_PREFAB_ID || fireball.prefabID == OIL_FIREBALL_PREFAB_ID)) _fireBalls.Add(fireball);
                }


                ConVar.PatrolHelicopter.bulletAccuracy = HeliAccuracy;
                ConVar.PatrolHelicopter.lifetimeMinutes = LifeTimeMinutes;
                if (TimeBeforeUnlockingHack > 0f) HackableLockedCrate.requiredHackSeconds = TimeBeforeUnlockingHack;

                if (UseCustomLoot) LoadLootData();
                LoadHeliSpawns();
                LoadWeaponData();




                foreach (var ch47 in _chinooks)
                {
                    CH47Instance = ch47; //I feel like this is better than LINQ's FirstOrDefault for a hashset. maybe hashsets shouldn't have been used at all. who knows
                    break;
                }


                var randomHeliSpawnTime = GetRandomSpawnTime();
                var randomChinookSpawnTime = GetRandomSpawnTime(true);

                if (randomHeliSpawnTime > 0)
                {
                    var heliSpawnTime = UseOldSpawning ? GetRandomSpawnTime() : GetAdjustedSecondsToSpawn(GetRandomSpawnTime());

                    CallTimer = GetHeliSpawnTimer(heliSpawnTime);
                }

                if (randomChinookSpawnTime > 0)
                {
                    var chinookSpawnTime = UseOldSpawningCH47 ? GetRandomSpawnTime(true) : GetAdjustedSecondsToSpawn(GetRandomSpawnTime(true));

                    CallTimerCH47 = GetChinookSpawnTimer(chinookSpawnTime);
                }

                if (_terrainHookCalled)
                {
                    if (SpawnHeliOnRestart) callHelis(HelicoptersToSpawn, forced: false);
                    if (SpawnChinookOnRestart) callChinooks(ChinooksToSpawn, forced: false);
                }
                else
                {
                    if (DisableDefaultHeliSpawns) ToggleHeliEvent(false); //OnServerInit may be too late to disable the event
                    if (DisableDefaultChinookSpawns) ToggleCH47Event(false);
                }
            }
            finally { _init = true; }
        }
        #endregion

        private void Unload()
        {
            ServerMgr.Instance?.CancelInvoke(CheckHelicopter);

            ToggleHeliEvent(true);
            ToggleCH47Event(true);

            SaveCooldownData();
            SaveSpawnData();
        }

        /*/
        private void OnServerSave()
        {
            var saveAction = new Action(() =>
            {
                SaveCooldownData();
                SaveSpawnData();
            });
            if (ServerMgr.Instance != null) ServerMgr.Instance.Invoke(saveAction, 4f); //ServerMgr.Instance should never be null on a server save, but we'll check anyway. We want to delay saving a few seconds because a lot of things are often called on OnServerSave
            else saveAction.Invoke();
        }/*/

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null || entity.IsDestroyed || entity.gameObject == null) return; //entity should never be destroyed on spawn, but lets do a check anyway
            var prefabname = entity?.ShortPrefabName ?? string.Empty;
            var longprefabname = entity?.PrefabName ?? string.Empty;
            if (string.IsNullOrEmpty(prefabname) || string.IsNullOrEmpty(longprefabname)) return; //another likely impossibility, but lets just check EVERYTHING



            var ownerID = (entity as BaseEntity)?.OwnerID ?? 0;

            var lockedEntCrate = entity as LockedByEntCrate;
            if (lockedEntCrate != null) _lockedCrates.Add(lockedEntCrate);

            var hackableCrate = entity as HackableLockedCrate;
            if (hackableCrate != null) _hackLockedCrates.Add(hackableCrate);

            var ch = entity as CH47HelicopterAIController;
            if (ch?.prefabID == CHINOOK_SCIENTISTS_PREFAB_ID && !ch.ShouldLand()) //ShouldLand will be true for oil rig chinooks. we want to ignore those; we don't control them
            {
                ch.Invoke(() =>
                {
                    if (!ch.IsDestroyed) ch.Kill();
                }, LifeTimeMinutesCH47 * 60);

                _chinooks.Add(ch);
                CH47Instance = ch;

                if (UseOldSpawningCH47) CallTimerCH47 = GetChinookSpawnTimer(GetRandomSpawnTime(true));

                UpdateChinook(ch, true);
            }

            var fireBall = entity as FireBall;
            if (fireBall != null && (fireBall.prefabID == NAPALM_FIREBALL_PREFAB_ID || fireBall.prefabID == OIL_FIREBALL_PREFAB_ID)) _fireBalls.Add(fireBall);

            if (entity.prefabID == ROCKET_PREFAB_ID || entity.prefabID == NAPALM_ROCKET_PREFAB_ID || entity.prefabID == AIRBURST_ROCKET_PREFAB_ID)
            {
                var explosion = entity as TimedExplosive;
                if (explosion == null || explosion.IsDestroyed || explosion.gameObject == null) return; //super ultra extra safe null checking


                _useNapalm = explosion.prefabID == NAPALM_ROCKET_PREFAB_ID && explosion.OwnerID != 1337; //is a napalm rocket & was not spawned by plugin (1337 owner id indicates that)


                if (MaxHeliRockets < 1) explosion.Kill();
                else
                {
                    explosion.explosionRadius = RocketExplosionRadius;
                    if (MaxHeliRockets > 12 && ownerID == 0)
                    {
                        PatrolHelicopter strafeHeli = null;
                        foreach (var heli in _PatrolHelicopters) //loop through all active helis to find one that's currently strafing
                        {
                            if (heli == null || heli.IsDestroyed || heli.gameObject == null || heli.IsDead()) continue; //super ultra extra safe null checking
                            var state = heli?.GetComponent<PatrolHelicopterAI>()?._currentState ?? PatrolHelicopterAI.aiState.IDLE;
                            if (state == PatrolHelicopterAI.aiState.STRAFE)
                            {
                                strafeHeli = heli;
                                break;
                            }
                        }

                        if (strafeHeli == null || strafeHeli.IsDestroyed || strafeHeli.gameObject == null || strafeHeli.IsDead()) return; //super ultra extra safe null checking
                        if (!_strafeCount.TryGetValue(strafeHeli, out int curCount)) curCount = _strafeCount[strafeHeli] = 1;
                        else curCount = _strafeCount[strafeHeli] += 1;
                        if (curCount >= 12)
                        {
                            var heliAI = strafeHeli?.GetComponent<PatrolHelicopterAI>() ?? null;
                            if (heliAI == null || heliAI.gameObject == null) return; //extra null checking
                            var actCount = 0;
                            Action fireAct = null;
                            fireAct = new Action(() =>
                            {
                                if (heliAI == null || heliAI.gameObject == null || actCount >= (MaxHeliRockets - 12))
                                {
                                    InvokeHandler.CancelInvoke(heliAI, fireAct);
                                    return;
                                }
                                actCount++;
                                FireRocket(heliAI);
                            });
                            InvokeHandler.InvokeRepeating(heliAI, fireAct, TimeBetweenRockets, TimeBetweenRockets);
                            _strafeCount[strafeHeli] = 0;
                        }
                    }
                    else if (MaxHeliRockets < 12 && HeliInstance != null && HeliInstance.gameObject != null && HeliInstance.ClipRocketsLeft() > MaxHeliRockets)
                    {
                        explosion.Kill();
                        return;
                    }


                    var dmgTypes = explosion?.damageTypes ?? null;

                    if (dmgTypes != null && dmgTypes.Count > 0)
                    {
                        for (int i = 0; i < dmgTypes.Count; i++)
                        {
                            var dmg = dmgTypes[i];
                            if (dmg == null) continue; //impossible? who knows. but we're gonna null check it anyway cause that's what you do
                            if (dmg.type == Rust.DamageType.Blunt) dmg.amount = RocketDamageBlunt;
                            if (dmg.type == Rust.DamageType.Explosion) dmg.amount = RocketDamageExplosion;
                        }
                    }
                }
            }

            if (entity.prefabID == HELI_CRATE_PREFAB_ID)
            {
                if (UseCustomLoot && _lootData?.HeliInventoryLists != null && _lootData.HeliInventoryLists.Count > 0)
                {
                    var heli_crate = entity as LootContainer;
                    if (heli_crate == null || heli_crate?.inventory == null) return; //possible that the inventory is somehow null? not sure

                    var index = _rng.Next(_lootData.HeliInventoryLists.Count);
                    var inv = _lootData.HeliInventoryLists[index];
                    if (inv?.lootBoxContents != null && inv.lootBoxContents.Count > 0)
                    {
                        if (heli_crate?.inventory?.itemList != null && heli_crate.inventory.itemList.Count > 0)
                        {
                            var itemList = new List<Item>(heli_crate.inventory.itemList);
                            if (itemList != null && itemList.Count > 0) for (int i = 0; i < itemList.Count; i++) RemoveFromWorld(itemList[i]); //completely remove all existing items in crate
                        }

                        for (int i = 0; i < inv.lootBoxContents.Count; i++)
                        {
                            var itemDef = inv.lootBoxContents[i];
                            if (itemDef == null) continue;

                            var amount = (itemDef.amountMin > 0 && itemDef.amountMax > 0) ? UnityEngine.Random.Range(itemDef.amountMin, itemDef.amountMax) : itemDef.amount;

                            var def = ItemManager.FindItemDefinition(itemDef.name);
                            if (def != null)
                            {
                                var item = ItemManager.Create(def, amount, itemDef.skinID);
                                if (item != null && !item.MoveToContainer(heli_crate.inventory)) RemoveFromWorld(item); //ensure the item is completely removed if we can't move it, so we're not causing issues
                            }
                        }

                        heli_crate.inventory.MarkDirty();
                    }
                }

                if (TimeBeforeUnlocking >= 0f)
                {
                    var crate2 = entity as LockedByEntCrate;
                    if (crate2 != null)
                    {
                        if (TimeBeforeUnlocking <= 0f) UnlockCrate(crate2);
                        else crate2.Invoke(() =>
                        {
                            if (entity == null || entity.IsDestroyed || crate2 == null) return;
                            UnlockCrate(crate2);
                        }, TimeBeforeUnlocking);
                    }
                }
            }

            var debris = entity as HelicopterDebris;
            if (debris != null)
            {
                if (DisableGibs || GibsHealth <= 0)
                {
                    NextTick(() => { if (!(entity?.IsDestroyed ?? true)) entity.Kill(); });
                    return;
                }

                if (GibsHealth != 500f)
                {
                    debris.InitializeHealth(GibsHealth, GibsHealth);
                    debris.SendNetworkUpdate();
                }

                _gibs.Add(debris);
                if (GibsTooHotLength != 480f) debris.tooHotUntil = Time.realtimeSinceStartup + GibsTooHotLength;
            }

            var BaseHeli = entity as PatrolHelicopter;
            if (BaseHeli != null)
            {
                var isMax = HeliCount >= MaxActiveHelicopters && MaxActiveHelicopters != -1;
                if (DisableHeli || isMax)
                {
                    NextTick(() => { if (!(entity?.IsDestroyed ?? true)) entity.Kill(); });
                }
                if (DisableHeli)
                {
                    Puts(GetMessage("heliAutoDestroyed"));
                    return;
                }
                else if (isMax)
                {
                    Puts(GetMessage("maxHelis"));
                    return;
                }

                var AIHeli = entity?.GetComponent<PatrolHelicopterAI>() ?? null;
                if (AIHeli == null) return;

                if (UseOldSpawning) CallTimer = GetHeliSpawnTimer(GetRandomSpawnTime());


                _PatrolHelicopters.Add(BaseHeli);
                UpdateHeli(BaseHeli, true);

                if (UseCustomHeliSpawns && _spawnsData?.HelicopterSpawns != null && _spawnsData.HelicopterSpawns.Count > 0 && !_forceCalled.Contains(BaseHeli))
                {
                    var valCount = _spawnsData.HelicopterSpawns.Count;
                    var rng = UnityEngine.Random.Range(0, valCount);
                    var pos = _spawnsData.HelicopterSpawns[rng].Position;
                    BaseHeli.transform.position = pos;
                    AIHeli.transform.position = pos;
                }

                if (HeliStartLength > 0.0f && HeliStartSpeed != HeliSpeed)
                {
                    AIHeli.maxSpeed = HeliStartSpeed;
                    AIHeli.Invoke(() =>
                    {
                        if (AIHeli == null || AIHeli.gameObject == null || BaseHeli == null || BaseHeli.IsDestroyed || BaseHeli.gameObject == null || BaseHeli.IsDead()) return;
                        AIHeli.maxSpeed = HeliSpeed;
                    }, HeliStartLength);
                }
            }
        }

        private object CanBeTargeted(BaseCombatEntity entity, MonoBehaviour monoTurret) //this hook is unsubscribed if the config option isn't enabled
        {
            if (!_init || entity == null || (entity?.IsDestroyed ?? true) || monoTurret == null) return null;
            var aiHeli = (monoTurret as HelicopterTurret)?._heliAI ?? null;
            if (aiHeli == null) return null;
            var player = entity as BasePlayer;
            if (player != null && !player.IsDestroyed && player.gameObject != null && !player.IsSleeping() && Vanish != null && (Vanish?.Call<bool>("IsInvisible", player) ?? false)) return null;
            if ((aiHeli?._currentState ?? PatrolHelicopterAI.aiState.DEATH) == PatrolHelicopterAI.aiState.DEATH) return false;
            return null;
        }

        private object OnHelicopterTarget(HelicopterTurret turret, BaseCombatEntity entity) //this hook is unsubscribed if the config option isn't enabled
        {
            if (turret == null || entity == null) return null;
            if ((turret?._heliAI?._currentState ?? PatrolHelicopterAI.aiState.DEATH) == PatrolHelicopterAI.aiState.DEATH) return false;
            return null;
        }

        private object CanHelicopterStrafeTarget(PatrolHelicopterAI entity, BasePlayer target) //this hook is unsubscribed if the config option isn't enabled
        {
            if (entity == null || target == null) return null;
            if ((entity?._currentState ?? PatrolHelicopterAI.aiState.DEATH) == PatrolHelicopterAI.aiState.DEATH) return false;
            return null;
        }

        private object CanHelicopterStrafe(PatrolHelicopterAI entity) //this hook is unsubscribed if the config option isn't enabled
        {
            if (entity == null) return null;
            if ((entity?._currentState ?? PatrolHelicopterAI.aiState.DEATH) == PatrolHelicopterAI.aiState.DEATH) return false;
            return null;
        }

        private object CanHelicopterTarget(PatrolHelicopterAI entity, BasePlayer player) //this hook is unsubscribed if the config option isn't enabled
        {
            if ((entity?._currentState ?? PatrolHelicopterAI.aiState.DEATH) == PatrolHelicopterAI.aiState.DEATH) return false;
            return null;
        }

        private object CanHelicopterUseNapalm(PatrolHelicopterAI entity) //this hook is unsubscribed if the config option isn't enabled
        {
            return false;
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity?.net == null) //edge cases?
                return;

            var crate = entity as LockedByEntCrate;
            var CH47 = entity as CH47HelicopterAIController;
            var baseHeli = entity as PatrolHelicopter;

            if (crate != null) 
                _lockedCrates.Remove(crate);

            if (baseHeli != null)
            {
                var heliId = baseHeli.net.ID;
                _PatrolHelicopters.Remove(baseHeli);
                _forceCalled.Remove(baseHeli);

                if (!UseOldSpawning && (CallTimer == null || CallTimer.Destroyed) && (_timerHeliId.Value == 0 || heliId == _timerHeliId)) //this weird timer heli ID stuff is a workaround for an issue I just could not fix or figure out. sometimes the timer heli would not be equal after a server restart, so the timer would never ever restart.
                {
                    var rngTime = GetRandomSpawnTime();
                    if (rngTime > 0) CallTimer = GetHeliSpawnTimer(rngTime);
                } //otherwise, a timer is already firing (or heli is not a timer heli or old spawning is enabled)
            }

            if (CH47?.prefabID == CHINOOK_SCIENTISTS_PREFAB_ID && !CH47.ShouldLand()) //ShouldLand will be true for oil rig chinooks. we want to ignore those; we don't control them
            {
                _chinooks.Remove(CH47);
                _forceCalledCh.Remove(CH47);

                if (!UseOldSpawningCH47 && (CallTimerCH47 == null || CallTimerCH47.Destroyed) && CH47 == _timerCH47)
                {
                    var rngTime = GetRandomSpawnTime(true);
                    if (rngTime > 0) CallTimerCH47 = GetChinookSpawnTimer(rngTime);
                } //otherwise, a timer is already firing (or heli is not a timer heli or old spawning is enabled)
            }


            var fireball = entity as FireBall;
            if (fireball != null)
                _fireBalls.Remove(fireball);

            var debris = entity as HelicopterDebris;
            if (debris != null)
                _gibs.Remove(debris);
        }

        private void OnPlayerAttack(BasePlayer attacker, HitInfo hitInfo)
        {
            if (attacker == null || hitInfo?.HitEntity == null) return;

            if (hitInfo?.HitEntity is PatrolHelicopter)
            {
                if (GlobalDamageMultiplier != 1f && GlobalDamageMultiplier >= 0)
                {
                    hitInfo?.damageTypes?.ScaleAll(GlobalDamageMultiplier);
                    return;
                }
                var shortName = hitInfo?.Weapon?.GetItem()?.info?.shortname ?? string.Empty;
                var displayName = hitInfo?.Weapon?.GetItem()?.info?.displayName?.english ?? string.Empty;

                if (_weaponsData.WeaponList.TryGetValue(shortName, out float weaponConfig) || _weaponsData.WeaponList.TryGetValue(displayName, out weaponConfig))
                {
                    if (weaponConfig != 0.0f && weaponConfig != 1.0f) hitInfo?.damageTypes?.ScaleAll(weaponConfig);
                }
            }
        }
        #endregion
        #region Main
        private void UpdateHeli(PatrolHelicopter heli, bool justCreated = false)
        {
            if (heli == null || heli.IsDestroyed || heli.IsDead()) return;

            heli.startHealth = BaseHealth;
            if (justCreated) heli.InitializeHealth(BaseHealth, BaseHealth);

            heli.maxCratesToSpawn = MaxLootCrates;
            heli.bulletDamage = HeliBulletDamageAmount;
            heli.bulletSpeed = BulletSpeed;

            var weakspots = heli.weakspots;
            if (weakspots != null && weakspots.Length > 1) //not even sure if this is needed, but may fix some very strange NRE
            {
                if (justCreated)
                {
                    weakspots[0].health = MainRotorHealth;
                    weakspots[1].health = TailRotorHealth;
                }
                weakspots[0].maxHealth = MainRotorHealth;
                weakspots[1].maxHealth = TailRotorHealth;
            }

            var heliAI = heli?.GetComponent<PatrolHelicopterAI>() ?? null;
            if (heliAI == null) return;

            heliAI.maxSpeed = Mathf.Clamp(HeliSpeed, 0.1f, 125);
            heliAI.timeBetweenRockets = Mathf.Clamp(TimeBetweenRockets, 0.1f, 1f);
            heliAI.numRocketsLeft = Mathf.Clamp(MaxHeliRockets, 0, 48);
            UpdateTurrets(heliAI);
            heli.SendNetworkUpdateImmediate(justCreated);
        }

        private void UpdateChinook(CH47HelicopterAIController chinook, bool justCreated = false)
        {
            if (chinook == null || chinook.IsDestroyed || chinook.gameObject == null) return;

            chinook.startHealth = BaseChinookHealth;
            chinook._maxHealth = BaseChinookHealth;

            if (justCreated)
            {
                chinook.InitializeHealth(BaseChinookHealth, BaseChinookHealth);
            }
        }

        //nearly exact code used by Rust to fire helicopter rockets
        private void FireRocket(PatrolHelicopterAI heliAI)
        {
            if (heliAI == null || !(heliAI?.IsAlive() ?? false)) return;


            var strafeTarget = heliAI.strafe_target_position;
            if (strafeTarget == Vector3.zero) return;

            var num1 = 4f;

            var vector3 = heliAI.transform.position + heliAI.transform.forward * 1f;
            var direction = (strafeTarget - vector3).normalized;
            if (num1 > 0.0) direction = Quaternion.Euler(UnityEngine.Random.Range((float)(-num1 * 0.5), num1 * 0.5f), UnityEngine.Random.Range((float)(-num1 * 0.5), num1 * 0.5f), UnityEngine.Random.Range((float)(-num1 * 0.5), num1 * 0.5f)) * direction;

            var leftTubeLast = heliAI.leftTubeFiredLast;
            heliAI.leftTubeFiredLast = !leftTubeLast;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                Effect.server.Run(heliAI.helicopterBase.rocket_fire_effect.resourcePath, heliAI.helicopterBase, StringPool.Get(sb.Clear().Append("rocket_tube_").Append((!leftTubeLast ? "right" : "left")).ToString()), Vector3.zero, Vector3.forward, null, true);
            }
            finally { Pool.Free(ref sb); }


            var entity = GameManager.server.CreateEntity(!(_useNapalm && heliAI.CanUseNapalm()) ? heliAI.rocketProjectile.resourcePath : heliAI.rocketProjectile_Napalm.resourcePath, vector3, new Quaternion(), true);
            if (entity == null)
            {
                PrintWarning("Failed to create entity on " + nameof(FireRocket) + " !!");
                return;
            }

            var projectile = entity.GetComponent<ServerProjectile>();
            projectile?.InitializeVelocity(direction * projectile.speed);

            entity.OwnerID = 1337; //assign ownerID so it doesn't infinitely loop on OnEntitySpawned
            entity.Spawn();
        }

        private PatrolHelicopter callHeli(Vector3 coordinates = new Vector3(), bool forced = true, bool setPositionAfterSpawn = true)
        {
            var heli = (PatrolHelicopter)GameManager.server.CreateEntity(HELI_PREFAB, new Vector3(), new Quaternion(), true);
            if (heli == null)
            {
                PrintWarning("Failed to create heli prefab on " + nameof(callHeli));
                return null;
            }

            var heliAI = heli?.GetComponent<PatrolHelicopterAI>() ?? null;
            if (heliAI == null)
            {
                PrintWarning("Failed to get helicopter AI on " + nameof(callHeli));
                return null;
            }
            
            if (coordinates != Vector3.zero)
            {
                if (coordinates.y < 225) coordinates.y = 225;
                heliAI.SetInitialDestination(coordinates, 0.25f);
                if (setPositionAfterSpawn) heli.transform.position = heliAI.transform.position = coordinates;
            }

            if (forced) _forceCalled.Add(heliAI.helicopterBase);
            
            heli.Spawn();
            
            return heli;
        }

        //chinook position setting code is based off of the same code used by patrolhelicopter in order to (try to) get it to spawn out of the map and fly in
        private CH47HelicopterAIController callChinook(Vector3 coordinates = new Vector3(), bool forced = true)
        {
            var heli = (CH47HelicopterAIController)GameManager.server.CreateEntity(CHINOOK_PREFAB, new Vector3(0, 100, 0), new Quaternion(), true);
            if (heli == null) return null;
            float x = TerrainMeta.Size.x;
            float num = coordinates.y + 50f;
            var mapScaleDistance = 0.8f; //high scale for further out distances/positions
            var vector3_1 = Vector3Ex.Range(-1f, 1f);
            vector3_1.y = 0.0f;
            vector3_1.Normalize();
            var vector3_2 = vector3_1 * (x * mapScaleDistance);
            vector3_2.y = num;
            heli.transform.position = vector3_2;
            if (forced) _forceCalledCh.Add(heli);
            heli.Spawn();
            if (coordinates != Vector3.zero)
            {
                heli.Invoke(() =>
                {
                    if (heli != null && !heli.IsDestroyed) SetDestination(heli, coordinates + new Vector3(0f, 10f, 0)); //worth noting that null checks inside of invokes are probably unnecessary, because the invoke should never happen if the object turned null... but we check anyway.
                }, 1f);
            }
            return heli;
        }

        private List<PatrolHelicopter> callHelis(int amount, Vector3 coordinates = new Vector3(), bool forced = true, bool setPositionAfterSpawn = true)
        {
            if (amount < 1) return null;
            var listHelis = new List<PatrolHelicopter>(amount);
            for (int i = 0; i < amount; i++) listHelis.Add(callHeli(coordinates, forced, setPositionAfterSpawn));
            return listHelis;
        }

        private List<CH47HelicopterAIController> callChinooks(int amount, Vector3 coordinates = new Vector3(), bool forced = true)
        {
            if (amount < 1) return null;
            var listHelis = new List<CH47HelicopterAIController>(amount);
            for (int i = 0; i < amount; i++) listHelis.Add(callChinook(coordinates, forced));
            return listHelis;
        }

        private PatrolHelicopter callCoordinates(Vector3 coordinates)
        {
            var heli = (PatrolHelicopter)GameManager.server.CreateEntity(HELI_PREFAB, new Vector3(), new Quaternion(), true);
            if (heli == null) return null;
            var heliAI = heli?.GetComponent<PatrolHelicopterAI>() ?? null;
            if (heliAI == null) return null;
            heliAI.SetInitialDestination(coordinates + new Vector3(0f, 10f, 0f), 0.25f);
            _forceCalled.Add(heliAI.helicopterBase);
            heli.Spawn();
            return heli;
        }

        private void UpdateTurrets(PatrolHelicopterAI helicopter)
        {
            if (helicopter == null || helicopter.leftGun == null || helicopter.rightGun == null) return;
            helicopter.leftGun.fireRate = helicopter.rightGun.fireRate = TurretFireRate;
            helicopter.leftGun.timeBetweenBursts = helicopter.rightGun.timeBetweenBursts = TurretTimeBetweenBursts;
            helicopter.leftGun.burstLength = helicopter.rightGun.burstLength = TurretBurstLength;
            helicopter.leftGun.maxTargetRange = helicopter.rightGun.maxTargetRange = TurretMaxRange;
        }

        private int KillAllHelis(bool isForced = false)
        {
            CheckHelicopter();
            var count = 0;
            if (_PatrolHelicopters.Count < 1) return count;

            var helis = new List<PatrolHelicopter>(_PatrolHelicopters);
            for (int i = 0; i < helis.Count; i++)
            {
                var helicopter = helis[i];
                if (helicopter != null && !helicopter.IsDead())
                {
                    if (DisableCratesDeath) helicopter.maxCratesToSpawn = 0;

                    if (isForced) helicopter.Kill(); //network kill if forced, else die 'naturally' with explosion
                    else helicopter.DieInstantly();

                    count++;
                }
            }

            CheckHelicopter();
            return count;
        }

        private int KillAllChinooks(bool isForced = false)
        {
            CheckHelicopter();
            var count = 0;
            if (_chinooks.Count < 1) return count;

            var chinooks = new List<CH47HelicopterAIController>(_chinooks);
            for (int i = 0; i < chinooks.Count; i++)
            {
                var ch47 = chinooks[i];
                if (ch47 != null && !ch47.IsDestroyed)
                {
                    if (isForced) ch47.Kill(); //network kill if forced, else die 'naturally' with explosion
                    else ch47.DieInstantly();

                    count++;
                }
            }

            CheckHelicopter();
            return count;
        }
        #endregion
        #region Commands
        [ChatCommand("helispawn")]
        private void cmdHeliSpawns(BasePlayer player, string command, string[] args)
        {
            if (!HasPerms(player.UserIDString, "helispawn"))
            {
                SendNoPerms(player);
                return;
            }
            if (args.Length < 1)
            {
                var msgSB = new StringBuilder();
                for (int i = 0; i < _spawnsData.HelicopterSpawns.Count; i++)
                {
                    var sp = _spawnsData.HelicopterSpawns[i];
                    msgSB.Append(sp.Name).Append(": ").Append(sp.Position).Append(", ");
                }

                if (msgSB.Length > 2) msgSB.Length -= 2;
                var msg = msgSB.ToString();

                if (!string.IsNullOrEmpty(msg)) SendReply(player, GetMessage("spawnCommandLiner", player.UserIDString) + msgSB + GetMessage("spawnCommandBottom", player.UserIDString));
                SendReply(player, GetMessage("removeAddSpawn"), player.UserIDString); //this isn't combined with a new line with the above because there is a strange character limitation per-message, so we send two messages
                return;
            }
            var arg0 = args[0];
            var spawn = args.Length > 1 ? FindSpawn(args[1]) : null;
            if (arg0.Equals("add", StringComparison.OrdinalIgnoreCase) && args.Length > 1)
            {
                if (spawn == null)
                {
                    var pos = player?.transform?.position ?? Vector3.zero;
                    if (pos == Vector3.zero) return;
                    _spawnsData.HelicopterSpawns.Add(new HelicopterSpawn { Position = pos, Name = args[1] });
                    SendReply(player, string.Format(GetMessage("addedSpawn", player.UserIDString), args[1], pos));
                }
                else SendReply(player, GetMessage("spawnExists", player.UserIDString));
            }
            else if (arg0.Equals("remove", StringComparison.OrdinalIgnoreCase) && args.Length > 1)
            {
                if (_spawnsData?.HelicopterSpawns == null || _spawnsData.HelicopterSpawns.Count < 1)
                {
                    SendReply(player, GetMessage("noSpawnsExist", player.UserIDString));
                    return;
                }
                if (spawn != null)
                {
                    var value = spawn.Position;
                    _spawnsData.HelicopterSpawns.Remove(spawn);
                    SendReply(player, string.Format(GetMessage("removedSpawn", player.UserIDString), args[1], value));
                }
                else SendReply(player, GetMessage("noSpawnFound", player.UserIDString));
            }
            else SendReply(player, string.Format(GetMessage("invalidSyntaxMultiple", player.UserIDString), "/helispawn add", "SpawnName", "/helispawn remove", "SpawnName"));
        }

        private void cmdUnlockCrates(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "unlockcrates"))
            {
                SendNoPerms(player);
                return;
            }
            var chCrate = args.Length > 0 ? args[0].Equals("ch47", StringComparison.OrdinalIgnoreCase) : false;
            var bothCrates = args.Length > 0 ? args[0].Equals("all", StringComparison.OrdinalIgnoreCase) : false;

            if (bothCrates || !chCrate) foreach (var crate in _lockedCrates) UnlockCrate(crate);

            if (bothCrates || chCrate) foreach (var crate in _hackLockedCrates) UnlockCrate(crate);

            player.Message(GetMessage("unlockedAllCrates", player.Id));
        }

        private void cmdDebug(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var testTimer = GetHeliSpawnTimer(GetRandomSpawnTime());
            player.Reply("got test timer: " + testTimer.Delay);
        }

        private void cmdNextHeli(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "nextheli"))
            {
                SendNoPerms(player);
                return;
            }

            var nextHeli = GetNextHeliTime();
            var nextCh47 = GetNextCH47Time();

            if (nextHeli <= TimeSpan.Zero) player.Message(GetMessage("noTimeFound", player.Id));
            else
                player.Message(string.Format(GetMessage("nextHeliSpawn", player.Id), !(HeliInstance?.isDead ?? true) && !(HeliInstance?.helicopterBase?.IsDestroyed ?? true) ? GetMessage("nextAlreadyActive", player.Id) : ReadableTimeSpan(nextHeli)));
            
            if (nextCh47 <= TimeSpan.Zero) player.Message(GetMessage("noTimeFoundCH47", player.Id));
            else 
                player.Message(string.Format(GetMessage("nextCH47Spawn", player.Id), CH47Instance != null && !CH47Instance.IsDestroyed ? GetMessage("nextAlreadyActive", player.Id) : ReadableTimeSpan(nextCh47)));
        }

        private void cmdDropCH47Crate(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "dropcrate"))
            {
                SendNoPerms(player);
                return;
            }
            if (CH47Instance == null || CH47Instance.IsDestroyed || CH47Instance.IsDead())
            {
                player.Message(GetMessage("noHelisFound", player.Id));
                return;
            }
            var all = args.Length > 0 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase);
            if (CH47Count > 1 && !all)
            {
                player.Message(string.Format(GetMessage("cannotBeCalled", player.Id), HeliCount.ToString("N0")));
                return;
            }
            if (all) foreach (var ch47 in _chinooks) { if (ch47.CanDropCrate()) ch47.DropCrate(); }
            else
            {
                if (!CH47Instance.CanDropCrate())
                {
                    player.Message(GetMessage("ch47AlreadyDropped", player.Id));
                    return;
                }
                CH47Instance.DropCrate();
            }
            player.Message(GetMessage("ch47DroppedCrate", player.Id));
        }


        private void cmdTeleportHeli(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;

            var ply = player?.Object as BasePlayer;
            if (ply == null || ply.IsDestroyed || ply.IsDead() || !ply.IsConnected || ply.IsSleeping()) return;
            _ = Vector3.zero;
            var tpCh47 = args.Length > 0 ? args[0].Equals("ch47", StringComparison.OrdinalIgnoreCase) : false;
            Vector3 tpPos;
            if (!tpCh47)
            {
                if (!HasPerms(player.Id, "tpheli"))
                {
                    SendNoPerms(player);
                    return;
                }
                if (HeliInstance == null || HeliInstance?.transform == null)
                {
                    player.Message(GetMessage("noHelisFound", player.Id));
                    return;
                }
                if (HeliCount > 1)
                {
                    player.Message(string.Format(GetMessage("cannotBeCalled", player.Id), HeliCount.ToString("N0")));
                    return;
                }
                tpPos = HeliInstance.transform.position;
            }
            else
            {
                if (!HasPerms(player.Id, "tpch47"))
                {
                    SendNoPerms(player);
                    return;
                }
                if (CH47Instance == null || CH47Instance?.transform == null)
                {
                    player.Message(GetMessage("noHelisFound", player.Id));
                    return;
                }
                if (HeliCount > 1)
                {
                    player.Message(string.Format(GetMessage("cannotBeCalled", player.Id), HeliCount.ToString("N0")));
                    return;
                }
                tpPos = CH47Instance.transform.position;
            }

            if (tpPos == Vector3.zero)
            {
                player.Message(GetMessage("noHelisFound", player.Id));
                return;
            }
            TeleportPlayer(ply, GetGround(tpPos));
            player.Message(GetMessage("teleportedToHeli", player.Id));
        }

        private object CanPlayerCallHeli(BasePlayer player, bool ch47 = false)
        {
            if (player == null) return null;
            var permMsg = GetNoPerms(player.UserIDString);
            var callPerm = ch47 ? "callch47" : "callheli";
            var callMult = ch47 ? "callmultiplech47" : "callmultiple";
            var cooldownTime = GetLowestCooldown(player, ch47);
            var limit = GetHighestLimit(player, ch47);
            var now = DateTime.Now;
            var today = now.ToString("d");
            var cdd = GetCooldownInfo(player.userID);
            if (cdd == null)
            {
                cdd = new CooldownInfo(player);
                if (_cooldownData?.cooldownList != null) _cooldownData.cooldownList.Add(cdd);
            }
            var timesCalled = ch47 ? cdd.TimesCalledCH47 : cdd.TimesCalled;
            var lastCall = ch47 ? cdd.LastCallDayCH47 : cdd.LastCallDay;
            var coolTime = ch47 ? cdd.CooldownTimeCH47 : cdd.CooldownTime;
            if (limit < 1 && !IgnoreLimits(player) && !HasPerms(player.UserIDString, callPerm)) return permMsg;
            if (!IgnoreLimits(player) && limit > 0)
            {
                if (timesCalled >= limit && today == lastCall) return string.Format(GetMessage("callheliLimit", player.UserIDString), limit);
                else if (today != lastCall)
                {
                    if (ch47) cdd.TimesCalledCH47 = 0;
                    else cdd.TimesCalled = 0;
                }
            }
            if (!IgnoreCooldown(player) && cooldownTime > 0.0f && !string.IsNullOrEmpty(coolTime))
            {
                if (!DateTime.TryParse(coolTime, out DateTime cooldownDT))
                {
                    PrintWarning("An error has happened while trying to parse date time ''" + coolTime + "''! Report this issue on plugin thread.");
                    return GetMessage("cmdError", player.UserIDString);
                }
                var diff = now - cooldownDT;
                if (diff.TotalSeconds < cooldownTime)
                {
                    var cooldownDiff = TimeSpan.FromSeconds(cooldownTime);
                    var waitedString = ReadableTimeSpan(diff);
                    var timeToWait = ReadableTimeSpan(cooldownDiff);
                    return string.Format(GetMessage("callheliCooldown", player.UserIDString), waitedString, timeToWait);
                }
            }
            if ((ch47 ? CH47Count : HeliCount) > 0 && !HasPerms(player.UserIDString, callMult)) return string.Format(GetMessage("cannotBeCalled", player.UserIDString), ch47 ? CH47Count : HeliCount);
            if (!ch47 && MaxActiveHelicopters >= 0 && (_PatrolHelicopters.Count + 1) > MaxActiveHelicopters) return string.Format(GetMessage("tooManyActiveHelis", player.UserIDString));

            return null;
        }


        [ChatCommand("callheli")]
        private void cmdCallToPlayer(BasePlayer player, string command, string[] args)
        {
            var argsStr = args.Length > 0 ? string.Join(" ", args) : string.Empty;
            try
            {
                var canCall = CanPlayerCallHeli(player) as string;
                if (!string.IsNullOrEmpty(canCall))
                {
                    SendReply(player, canCall);
                    return;
                }

                var now = DateTime.Now;
                var cdd = GetCooldownInfo(player.userID);
                if (cdd == null)
                {
                    cdd = new CooldownInfo(player);
                    _cooldownData.cooldownList.Add(cdd);
                }

                if (args.Length == 0)
                {
                    if (!HasPerms(player.UserIDString, "helicontrol.callheli"))
                    {
                        SendReply(player, GetNoPerms(player.UserIDString));
                        return;
                    }
                    var newHeli = callHeli();
                    if (!HasPerms(player.UserIDString, "helicontrol.dropcrates")) newHeli.maxCratesToSpawn = 0;
                    SendReply(player, GetMessage("heliCalled", player.UserIDString));
                    cdd.CooldownTime = now.ToString();
                    cdd.LastCallDay = now.ToString("d");
                    cdd.TimesCalled += 1;
                    return;
                }
                var target = ulong.TryParse(args[0], out ulong ID) ? FindPlayerByID(ID) : FindPlayerByPartialName(args[0]);
                if (target == null)
                {
                    SendReply(player, string.Format(GetMessage("playerNotFound", player.UserIDString), args[0]));
                    return;
                }

                if (target != null && HasPerms(player.UserIDString, "callheliself") && !HasPerms(player.UserIDString, "callhelitarget") && target != player)
                {
                    SendReply(player, string.Format(GetMessage("onlyCallSelf", player.UserIDString), player.displayName));
                    return;
                }
                if (target != null && !HasPerms(player.UserIDString, "callheliself") && !HasPerms(player.UserIDString, "callhelitarget"))
                {
                    SendReply(player, GetMessage("cantCallTargetOrSelf", player.UserIDString));
                    return;
                }

                var num = 1;
                if (args.Length == 2 && HasPerms(player.UserIDString, "callheli") && !int.TryParse(args[1], out num)) num = 1;

                var newHelis = callHelis(num, target.transform.position, setPositionAfterSpawn: SpawnHeliOnTarget);
                if (newHelis.Count > 0 && !permission.UserHasPermission(player.UserIDString, "helicontrol.dropcrates")) for (int i = 0; i < newHelis.Count; i++) newHelis[i].maxCratesToSpawn = 0;
                SendReply(player, string.Format(GetMessage("helisCalledPlayer", player.UserIDString), num, target.displayName));
                cdd.CooldownTime = now.ToString();
                cdd.TimesCalled += 1;
                cdd.LastCallDay = now.ToString("d");
            }
            catch (Exception ex)
            {
                var errorMsg = GetMessage("cmdError", player.UserIDString);
                if (!string.IsNullOrEmpty(errorMsg)) SendReply(player, errorMsg);
                PrintError("Error while using /callheli with args: " + argsStr + System.Environment.NewLine + ex.ToString());
            }
        }

        [ChatCommand("callch47")]
        private void cmdCallCH47(BasePlayer player, string command, string[] args)
        {
            var canCall = CanPlayerCallHeli(player, true) as string;
            if (!string.IsNullOrEmpty(canCall))
            {
                SendReply(player, canCall);
                return;
            }
            var now = DateTime.Now;
            var cdd = GetCooldownInfo(player.userID);
            if (cdd == null)
            {
                cdd = new CooldownInfo(player);
                _cooldownData.cooldownList.Add(cdd);
            }

            if (args.Length < 1)
            {
                callChinook();
                SendReply(player, GetMessage("heliCalled", player.UserIDString));
                cdd.CooldownTimeCH47 = now.ToString();
                cdd.LastCallDayCH47 = now.ToString("d");
                cdd.TimesCalledCH47 += 1;
                return;
            }
            var target = ulong.TryParse(args[0], out ulong ID) ? FindPlayerByID(ID) : FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, string.Format(GetMessage("playerNotFound", player.UserIDString), args[0]));
                return;
            }
            if (target != null && HasPerms(player.UserIDString, "callch47self") && !HasPerms(player.UserIDString, "callch47target") && target != player)
            {
                SendReply(player, string.Format(GetMessage("onlyCallSelf", player.UserIDString), player.displayName));
                return;
            }
            if (target != null && !HasPerms(player.UserIDString, "callch47self") && !HasPerms(player.UserIDString, "callch47target"))
            {
                SendReply(player, GetMessage("cantCallTargetOrSelf", player.UserIDString));
                return;
            }
            var num = 1;
            if (args.Length == 2 && HasPerms(player.UserIDString, "callch47") && !int.TryParse(args[1], out num)) num = 1;

            callChinooks(num, target.transform.position);
            SendReply(player, string.Format(GetMessage("helisCalledPlayer", player.UserIDString), num, target.displayName));
            cdd.CooldownTimeCH47 = now.ToString();
            cdd.TimesCalledCH47 += 1;
            cdd.LastCallDayCH47 = now.ToString("d");
        }


        private void cmdKillHeli(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "killheli"))
            {
                SendNoPerms(player);
                return;
            }
            var forced = args.Length > 0 ? args[0].Equals("forced", StringComparison.OrdinalIgnoreCase) : false;
            var numKilled = KillAllHelis(forced);
            player.Message(string.Format(GetMessage(forced ? "helisForceDestroyed" : "entityDestroyed", player.Id), numKilled.ToString("N0"), "helicopter"));
        }

        private void cmdKillCH47(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "killch47"))
            {
                SendNoPerms(player);
                return;
            }
            var forced = args.Length > 0 ? args[0].Equals("forced", StringComparison.OrdinalIgnoreCase) : false;
            var numKilled = KillAllChinooks(forced);
            player.Message(string.Format(GetMessage(forced ? "helisForceDestroyed" : "entityDestroyed", player.Id), numKilled.ToString("N0"), "helicopter"));
        }

        private void cmdUpdateHelicopters(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "update"))
            {
                SendNoPerms(player);
                return;
            }
            CheckHelicopter();
            if (HeliCount < 1 && CH47Count < 1)
            {
                player.Message(GetMessage("noHelisFound", player.Id));
                return;
            }
            var count = 0;

            foreach (var helicopter in _PatrolHelicopters)
            {
                if (helicopter == null) continue;
                UpdateHeli(helicopter, false);
                count++;
            }

            foreach (var ch47 in _chinooks)
            {
                if (ch47 == null) continue;
                UpdateChinook(ch47);
                count++;
            }

            player.Message(string.Format(GetMessage("updatedHelis", player.Id), count));
        }


        private void cmdStrafeHeli(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "strafe"))
            {
                SendNoPerms(player);
                return;
            }
            if (HeliCount < 1)
            {
                player.Message(GetMessage("noHelisFound", player.Id));
                return;
            }
            var isAll = args.Length > 0 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase);
            if (HeliCount > 1 && !isAll)
            {
                player.Message(string.Format(GetMessage("cannotBeCalled", player.Id), HeliCount));
                return;
            }
            if (args.Length < (isAll ? 2 : 1))
            {
                player.Message(string.Format(GetMessage("invalidSyntax", player.Id), "/strafe", "<player name>"));
                return;
            }

            var findArg = isAll ? args[1] : args[0];
            var target = FindPlayerByPartialName(findArg);
            if (ulong.TryParse(findArg, out ulong ID)) target = FindPlayerByID(ID);
            if (target == null)
            {
                player.Message(string.Format(GetMessage("playerNotFound", player.Id), findArg));
                return;
            }
            var targPos = target?.transform?.position ?? Vector3.zero;
            if (isAll)
            {
                foreach (var heli in _PatrolHelicopters)
                {
                    var ai = heli?.GetComponent<PatrolHelicopterAI>() ?? null;
                    if (ai != null) StartStrafe(ai, targPos, ai.CanUseNapalm());
                }
            }
            else StartStrafe(HeliInstance, targPos, HeliInstance.CanUseNapalm());
            player.Message(string.Format(GetMessage("strafingOtherPosition", player.Id), target.displayName));
        }


        private void cmdDestChangeHeli(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "destination") && !HasPerms(player.Id, "ch47destination"))
            {
                SendNoPerms(player);
                return;
            }
            if (args.Length < 1)
            {
                player.Message(string.Format(GetMessage("invalidSyntax", player.Id), "/strafe", "<player name>"));
                return;
            }
            var isCh47 = args.Last().Equals("ch47", StringComparison.OrdinalIgnoreCase);
            if ((isCh47 && !HasPerms(player.Id, "ch47destination")) || (!isCh47 && !HasPerms(player.Id, "destination")))
            {
                SendNoPerms(player);
                return;
            }
            if (isCh47 && CH47Count < 1)
            {
                player.Message(GetMessage("noHelisFound", player.Id));
                return;
            }
            else if (!isCh47 && HeliCount < 1)
            {
                player.Message(GetMessage("noHelisFound", player.Id));
                return;
            }

            var isAll = args.Length > 0 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase);
            if (isAll && args.Length < 2)
            {
                player.Message(string.Format(GetMessage("invalidSyntax", player.Id), "/strafe", "<player name>"));
                return;
            }

            if ((isCh47 && CH47Count > 1 || !isCh47 && HeliCount > 1) && !isAll)
            {
                player.Message(string.Format(GetMessage("cannotBeCalled", player.Id), isCh47 ? CH47Count : HeliCount));
                return;
            }

            var findArg = isAll ? args[1] : args[0];
            var target = FindPlayerByPartialName(findArg);
            if (ulong.TryParse(findArg, out ulong ID)) target = FindPlayerByID(ID);
            if (target == null)
            {
                player.Message(string.Format(GetMessage("playerNotFound", player.Id), findArg));
                return;
            }
            var targPos = target?.transform?.position ?? Vector3.zero;
            var newY = GetGround(targPos).y + 10f;
            if (newY > targPos.y) targPos.y = newY;
            if (isAll)
            {
                if (!isCh47)
                {
                    foreach (var heli in _PatrolHelicopters)
                    {
                        var ai = heli?.GetComponent<PatrolHelicopterAI>() ?? null;
                        if (ai != null) SetDestination(ai, targPos);
                    }
                }
                else foreach (var ch47 in _chinooks) SetDestination(ch47, targPos);
            }
            else
            {
                if (!isCh47) SetDestination(HeliInstance, targPos);
                else SetDestination(CH47Instance, targPos);
            }
            player.Message(string.Format(GetMessage("destinationOtherPosition", player.Id), target.displayName));
        }


        private void cmdKillFB(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "killnapalm"))
            {
                SendNoPerms(player);
                return;
            }
            player.Message(string.Format(GetMessage("entityDestroyed", player.Id), KillAllFireballs().ToString("N0"), "fireball"));
        }


        private void cmdKillGibs(IPlayer player, string command, string[] args)
        {
            if (!HasPerms(player.Id, "killgibs"))
            {
                SendNoPerms(player);
                return;
            }
            player.Message(string.Format(GetMessage("entityDestroyed", player.Id), KillAllGibs().ToString("N0"), "helicopter gib"));
        }


        [ConsoleCommand("callheli")]
        private void consoleCallHeli(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null && !HasPerms(player.UserIDString, "callheli"))
            {
                SendNoPerms(player);
                return;
            }
            var userIDString = player?.UserIDString ?? string.Empty;
            var noDrop = (player != null) ? !HasPerms(player.UserIDString, "helicontrol.dropcrates") : false;
            List<PatrolHelicopter> newHelis;

            if (arg.Args == null || arg?.Args?.Length < 1)
            {
                var newHeli = callHeli();
                if (newHeli != null && noDrop) newHeli.maxCratesToSpawn = 0;
                SendReply(arg, GetMessage("heliCalled", userIDString));
                return;
            }

            var isPos = arg.Args[0].Equals("pos", StringComparison.OrdinalIgnoreCase);
            if (isPos && arg.Args.Length < 4)
            {
                SendReply(arg, "You must supply 3 args for coordinates!");
                return;
            }

            if (isPos)
            {
                var coords = default(Vector3);
                var callNum = 1;
                if (!float.TryParse(arg.Args[1], out coords.x))
                {
                    SendReply(arg, string.Format(GetMessage("invalidCoordinate", userIDString), "X"));
                    return;
                }
                if (!float.TryParse(arg.Args[2], out coords.y))
                {
                    SendReply(arg, string.Format(GetMessage("invalidCoordinate", userIDString), "Y"));
                    return;
                }
                if (!float.TryParse(arg.Args[3], out coords.z))
                {
                    SendReply(arg, string.Format(GetMessage("invalidCoordinate", userIDString), "Z"));
                    return;
                }
                if (!CheckBoundaries(coords.x, coords.y, coords.z))
                {
                    SendReply(arg, GetMessage("coordinatesOutOfBoundaries", userIDString));
                    return;
                }
                if (arg.Args.Length > 4) if (!int.TryParse(arg.Args[4], out callNum)) callNum = 1;
                newHelis = callHelis(callNum, coords, setPositionAfterSpawn: SpawnHeliOnTarget);
                if (newHelis.Count > 0 && noDrop) for (int i = 0; i < newHelis.Count; i++) newHelis[i].maxCratesToSpawn = 0;
                SendReply(arg, string.Format(GetMessage("helisCalledPlayer", userIDString), callNum, coords));
                return;
            }

            var target = ulong.TryParse(arg.Args[0], out ulong ID) ? FindPlayerByID(ID) : FindPlayerByPartialName(arg.Args[0]);

            if (target == null)
            {
                SendReply(arg, string.Format(GetMessage("playerNotFound", userIDString), arg.Args[0]));
                return;
            }

            var num = 1;
            if (arg.Args.Length == 2 && !int.TryParse(arg.Args[1], out num)) num = 1;
            newHelis = callHelis(num, target?.transform?.position ?? Vector3.zero, setPositionAfterSpawn: SpawnHeliOnTarget);
            if (newHelis.Count > 0 && noDrop) for (int i = 0; i < newHelis.Count; i++) newHelis[i].maxCratesToSpawn = 0;
            SendReply(arg, string.Format(GetMessage("helisCalledPlayer", userIDString), num, target.displayName));
        }

        [ConsoleCommand("callch47")]
        private void consoleCallCH47(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null && !HasPerms(player.UserIDString, "callch47"))
            {
                SendNoPerms(player);
                return;
            }
            var userIDString = player?.UserIDString ?? string.Empty;

            if (arg.Args == null || arg.Args.Length < 1)
            {
                callChinook();
                SendReply(arg, GetMessage("heliCalled", userIDString));
                return;
            }
            var isPos = arg.Args[0].Equals("pos", StringComparison.OrdinalIgnoreCase);
            if (isPos && arg.Args.Length < 4)
            {
                SendReply(arg, "You must supply 3 args for coordinates!");
                return;
            }

            if (isPos)
            {
                var coords = default(Vector3);
                var callNum = 1;
                if (!float.TryParse(arg.Args[1], out coords.x))
                {
                    SendReply(arg, string.Format(GetMessage("invalidCoordinate", userIDString), "X"));
                    return;
                }
                if (!float.TryParse(arg.Args[2], out coords.y))
                {
                    SendReply(arg, string.Format(GetMessage("invalidCoordinate", userIDString), "Y"));
                    return;
                }
                if (!float.TryParse(arg.Args[3], out coords.z))
                {
                    SendReply(arg, string.Format(GetMessage("invalidCoordinate", userIDString), "Z"));
                    return;
                }
                if (!CheckBoundaries(coords.x, coords.y, coords.z))
                {
                    SendReply(arg, GetMessage("coordinatesOutOfBoundaries", userIDString));
                    return;
                }
                if (arg.Args.Length > 4) if (!int.TryParse(arg.Args[4], out callNum)) callNum = 1;
                callChinooks(callNum, coords);
                SendReply(arg, string.Format(GetMessage("helisCalledPlayer", userIDString), callNum, coords));
                return;
            }

            var target = ulong.TryParse(arg.Args[0], out ulong ID) ? FindPlayerByID(ID) : FindPlayerByPartialName(arg.Args[0]);

            if (target == null)
            {
                SendReply(arg, string.Format(GetMessage("playerNotFound", userIDString), arg.Args[0]));
                return;
            }

            var num = 1;
            if (arg.Args.Length == 2 && !int.TryParse(arg.Args[1], out num)) num = 1;
            callChinooks(num, target?.transform?.position ?? Vector3.zero);
            SendReply(arg, string.Format(GetMessage("helisCalledPlayer", userIDString), num, target.displayName));
        }

        #endregion
        #region Util

        private TimeSpan GetNextHeliTime()
        {
            if (_lastTimerStart <= DateTime.MinValue || _lastSpawnTimer <= 0f)
                return TimeSpan.Zero;

            return _lastTimerStart.AddSeconds(_lastSpawnTimer) - DateTime.UtcNow;
        }

        private TimeSpan GetNextCH47Time()
        {
            if (_lastTimerStartCH47 <= DateTime.MinValue || _lastSpawnTimerCH47 <= 0f)
                return TimeSpan.Zero;
            

           return _lastTimerStartCH47.AddSeconds(_lastSpawnTimerCH47) - DateTime.UtcNow;
        }

        private Timer GetHeliSpawnTimer(float heliSpawnTime)
        {
            if (heliSpawnTime <= 0.0f) throw new ArgumentOutOfRangeException(nameof(heliSpawnTime));

            Timer newTimer = null;
            newTimer = timer.Once(heliSpawnTime, () =>
            {
                if (HeliCount < 1 || AutoCallIfExists)
                {
                    var helis = callHelis(HelicoptersToSpawn, forced: false);
                    if (helis != null && helis.Count > 0) _timerHeliId = helis[0]?.net?.ID ?? default(NetworkableId); //possible fix? i don't care much for it
                }

            });

            return newTimer;
        }

        private Timer GetChinookSpawnTimer(float chinookSpawnTime)
        {
            if (chinookSpawnTime <= 0.0f) throw new ArgumentOutOfRangeException(nameof(chinookSpawnTime));

            Timer newTimer = null;
            newTimer = timer.Once(chinookSpawnTime, () =>
            {
                if (CH47Count < 1 || AutoCallIfExistsCH47)
                {
                    var chinooks = callChinooks(ChinooksToSpawn, forced: false);
                    if (chinooks != null && chinooks.Count > 0) _timerCH47 = chinooks[0];
                }

            });

            return newTimer;
        }

        /// <summary>
        /// Adjusts the given amount of time (in seconds) to be in accord with when the server (re)started
        /// </summary>
        /// <param name="timeSecs"></param>
        /// <returns></returns>
        private float GetAdjustedSecondsToSpawn(float timeSecs)
        {
            if (timeSecs <= 0f) throw new ArgumentOutOfRangeException(nameof(timeSecs));

            var now = DateTime.UtcNow;

            var startTime = Facepunch.Math.Epoch.ToDateTime((long)(Facepunch.Math.Epoch.FromDateTime(now) - Time.realtimeSinceStartup));
            var useTime = startTime;

            var cur = 0;
            var max = 100;
            while (useTime < now)
            {
                useTime = useTime.AddSeconds(timeSecs);

                cur++;
                if (cur >= max)
                {
                    break;
                } //this should never happen, but in case, we don't want a stuck loop
            }


            return (float)(useTime - now).TotalSeconds; //t
        }

        private string ReadableTimeSpan(TimeSpan span, string stringFormat = "N0") //I'm sure some of you uMod code snobs absolutely LOVE this one
        {
            if (span == TimeSpan.MinValue) return string.Empty;
            var str = string.Empty;
            var repStr = stringFormat.StartsWith("0.0", StringComparison.CurrentCultureIgnoreCase) ? ("." + stringFormat.Replace("0.", string.Empty)) : "WORKAROUNDGARBAGETEXTTHATCANNEVERBEFOUNDINASTRINGTHISISTOPREVENTREPLACINGANEMPTYSTRINGFOROLDVALUEANDCAUSINGANEXCEPTION"; //this removes unnecessary values, for example for ToString("0.00"), 80.00 will show as 80 instead

            if (span.TotalHours >= 24)
            {

                var totalHoursWereGoingToShowToTheUserAsAString = (span.TotalHours - ((int)span.TotalDays * 24)).ToString(stringFormat).Replace(repStr, string.Empty);
                var totalHoursToShowAsNumber = (int)Math.Round(double.Parse(totalHoursWereGoingToShowToTheUserAsAString), MidpointRounding.AwayFromZero);
                var showHours = totalHoursToShowAsNumber < 24 && totalHoursToShowAsNumber > 0;

                str = (int)span.TotalDays + (!showHours && span.TotalHours > 24 ? 1 : 0) + " day" + (span.TotalDays >= 1.5 ? "s" : "") + (showHours ? (" " + totalHoursWereGoingToShowToTheUserAsAString + " hour(s)") : "");
            }
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true)
        {
            try
            {
                if (player == null || player?.transform == null) return false;
                var playerPos = player?.transform?.position ?? Vector3.zero;
                var isConnected = player?.IsConnected ?? false;
                var distFrom = Vector3.Distance(playerPos, dest);
                player.SetParent(null, false, false);

                if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
                if (doSleep && isConnected && !player.IsSleeping()) player.StartSleeping();
                player.MovePosition(dest);
                if (isConnected)
                {
                    player.ClientRPCPlayer(null, player, "ForcePositionTo", dest);
                    if (distFrom >= 250 && distChecks) player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
                    player.UpdateNetworkGroup();
                    player.SendNetworkUpdate();
                    if (distFrom >= 50)
                    {
                        player.ClearEntityQueue();
                        player.SendFullSnapshot();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                return false;
            }
        }

        private float GetRandomSpawnTime(bool ch47 = false)
        {
            if (!ch47) return (MinSpawnTime > 0 && MaxSpawnTime > 0 && MaxSpawnTime >= MinSpawnTime) ? UnityEngine.Random.Range(MinSpawnTime, MaxSpawnTime) : -1f;
            else return (MinSpawnTimeCH47 > 0 && MaxSpawnTimeCH47 > 0 && MaxSpawnTimeCH47 >= MinSpawnTimeCH47) ? UnityEngine.Random.Range(MinSpawnTimeCH47, MaxSpawnTimeCH47) : -1f;
        }

        private void StartStrafe(PatrolHelicopterAI heli, Vector3 target, bool useNapalm)
        {
            if (heli == null || !(heli?.IsAlive() ?? false) || (heli?._currentState ?? PatrolHelicopterAI.aiState.DEATH) == PatrolHelicopterAI.aiState.DEATH || target == Vector3.zero) return;
            heli.interestZoneOrigin = target;
            heli.ExitCurrentState();
            heli.State_Strafe_Enter(target, useNapalm);
        }

        private void SetDestination(PatrolHelicopterAI heli, Vector3 target)
        {
            if (heli == null || !heli.IsAlive() || (heli?._currentState ?? PatrolHelicopterAI.aiState.DEATH) == PatrolHelicopterAI.aiState.DEATH || target == Vector3.zero) return;
            heli.interestZoneOrigin = target;
            heli.ExitCurrentState();
            heli.State_Move_Enter(target);
        }

        private void SetDestination(CH47HelicopterAIController heli, Vector3 target)
        {
            if (heli == null || heli.IsDestroyed || heli.gameObject == null || heli.IsDead() || target == Vector3.zero) return;
            heli.SetMoveTarget(target);

            var brain = heli?.GetComponent<CH47AIBrain>() ?? null;
            if (brain != null) brain.mainInterestPoint = target;
        }

        private void ToggleCH47Event(bool value)
        {
            var eventPrefabs = EventPrefabs;
            for (int i = 0; i < eventPrefabs.Length; i++)
            {
                var eve = eventPrefabs[i];
                if (eve?.targetPrefab?.resourceID == CHINOOK_EVENT_PREFAB_ID)
                {
                    eve.gameObject.SetActive(value);
                    break;
                }
            }
        }

        private void ToggleHeliEvent(bool value)
        {
            var eventPrefabs = EventPrefabs;
            for (int i = 0; i < eventPrefabs.Length; i++)
            {
                var eve = eventPrefabs[i];
                if (eve?.targetPrefab?.resourceID == HELI_EVENT_PREFAB_ID)
                {
                    eve.gameObject.SetActive(value);
                    break;
                }
            }
        }

        private Action _checkHelicopter = null;
        private Action CheckHelicopter
        {
            get
            {
                _checkHelicopter ??= new Action(() =>
                    {
                        _PatrolHelicopters.RemoveWhere(p => p?.IsDestroyed ?? true);
                        _chinooks.RemoveWhere(p => p?.IsDestroyed ?? true);
                        _gibs.RemoveWhere(p => p?.IsDestroyed ?? true);
                        _fireBalls.RemoveWhere(p => p?.IsDestroyed ?? true);
                        _forceCalled.RemoveWhere(p => p?.IsDestroyed ?? true);
                        _forceCalledCh.RemoveWhere(p => p?.IsDestroyed ?? true);
                        _lockedCrates.RemoveWhere(p => p?.IsDestroyed ?? true);
                    });

                return _checkHelicopter;
            }
        }

        private void UnlockCrate(LockedByEntCrate crate)
        {
            if (crate == null || crate.IsDestroyed || crate.gameObject == null) return;
            var lockingEnt = (crate?.lockingEnt != null) ? crate.lockingEnt.GetComponent<FireBall>() : null;
            if (lockingEnt != null && !lockingEnt.IsDestroyed)
            {
                lockingEnt.Extinguish();
                //lockingEnt.CancelInvoke(lockingEnt.Extinguish);
               // lockingEnt.Invoke(lockingEnt.Extinguish, 30f);
            }

            crate.CancelInvoke(crate.Think);
            crate.SetLocked(false);
            crate.lockingEnt = null;
        }

        private void UnlockCrate(HackableLockedCrate crate)
        {
            if (crate == null || crate.IsDestroyed) return;
            crate.SetFlag(BaseEntity.Flags.Reserved1, true);
            crate.SetFlag(BaseEntity.Flags.Reserved2, true);
            crate.isLootable = true;
            crate.CancelInvoke(new Action(crate.HackProgress));
        }

        private int HeliCount { get { return _PatrolHelicopters?.Count ?? 0; } }

        private int CH47Count { get { return _chinooks?.Count ?? 0; } }

        private CooldownInfo GetCooldownInfo(ulong userId) //iterating here isn't optimal for performance, but it's old code
        {
            if (_cooldownData?.cooldownList != null)
            {
                for (int i = 0; i < _cooldownData.cooldownList.Count; i++)
                {
                    var cd = _cooldownData.cooldownList[i];
                    if (cd?.UserID == userId) return cd;
                }
            }
            return null;
        }

        private void SendNoPerms(IPlayer player) => player?.Message(GetMessage("noPerms", player.Id));
        private void SendNoPerms(BasePlayer player) { if (player != null && player.IsConnected) player.ChatMessage(GetMessage("noPerms", player.UserIDString)); }
        private string GetNoPerms(string userID = "") { return GetMessage("noPerms", userID); }

        //**Borrowed from Nogrod's NTeleportation, with permission**//
        private Vector3 GetGround(Vector3 sourcePos)
        {
            var oldPos = sourcePos;
            sourcePos.y = TerrainMeta.HeightMap.GetHeight(sourcePos);
            if (Physics.SphereCast(oldPos, .1f, Vector3.down, out RaycastHit hitinfo, 300f, _groundLayer)) sourcePos.y = hitinfo.point.y;
            return sourcePos;
        }

        public static Vector3 GetVector3FromString(string vectorStr)
        {
            if (string.IsNullOrEmpty(vectorStr)) return Vector3.zero;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var split = sb.Clear().Append(vectorStr).Replace("(", string.Empty).Replace(")", string.Empty).ToString().Split(',');
                return new Vector3(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]));
            }
            finally { Pool.Free(ref sb); }
        }

        private int KillAllFireballs()
        {
            CheckHelicopter();
            var countfb = 0;
            if (_fireBalls.Count < 1) return countfb;

            var fbs = new List<FireBall>(_fireBalls);
            for (int i = 0; i < fbs.Count; i++)
            {
                var fb = fbs[i];
                if (fb != null && !fb.IsDestroyed)
                {
                    fb.Kill();
                    countfb++;
                }
            }

            CheckHelicopter();
            return countfb;
        }

        private int KillAllGibs()
        {
            CheckHelicopter();
            var countgib = 0;
            if (_gibs.Count < 1) return countgib;

            var gibs = new List<HelicopterDebris>(_gibs);
            for (int i = 0; i < gibs.Count; i++)
            {
                var gib = gibs[i];
                if (gib != null && !gib.IsDestroyed)
                {
                    gib.Kill();
                    countgib++;
                }
            }

            CheckHelicopter();
            return countgib;
        }

        private T GetConfig<T>(string name, T defaultValue)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            if (Config[name] == null)
            {
                SetConfig(name, defaultValue);

                return defaultValue;
            }

            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        private void SetConfig<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Config[name] = value;

            _configChanged = true;
        }

        private bool HasPerms(string userId, string perm)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(perm)) throw new ArgumentNullException(nameof(perm));

            if (userId.Equals("server_console", StringComparison.OrdinalIgnoreCase) || permission.UserHasPermission(userId, "helicontrol.admin")) return true;

            var sb = Pool.Get<StringBuilder>();
            try { return permission.UserHasPermission(userId, !perm.StartsWith("helicontrol") ? sb.Clear().Append("helicontrol.").Append(perm).ToString() : perm); }
            finally { Pool.Free(ref sb); }
        }

        private int Fitness(string individual, string target)
        {
            var count = 0;
            var range = Enumerable.Range(0, Math.Min(individual.Length, target.Length));
            foreach (var i in range)
            {
                if (individual[i] == target[i]) count++;
            }
            return count;
        }

        private int ExactMatch(string comp1, string comp2, StringComparison options = StringComparison.CurrentCulture)
        {
            if (string.IsNullOrEmpty(comp1) || string.IsNullOrEmpty(comp2)) return 0;
            var val = 0;


            if (comp1.Length > 0 && comp2.Length > 0)
            {
                for (int i = 0; i < comp1.Length; i++)
                {
                    if ((comp2.Length - 1) >= i)
                    {
                        if (comp2[i].ToString().Equals(comp1[i].ToString(), options)) val++;
                    }
                }
            }

            return val;
        }

        public const string VALID_CHARACTERS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=[]{}\|\'\"",./`*!@#$%^&*()_+<>?~;: "; //extra space at end is to allow space as a valid character

        public string[] GetValidCharactersString()
        {
            var length = VALID_CHARACTERS.Length;
            var array = new string[length];
            for (int i = 0; i < length; i++) array[i] = VALID_CHARACTERS[i].ToString();
            return array;
        }

        private string CleanPlayerName(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            var strSB = new StringBuilder();
            var valid = GetValidCharactersString();
            for (int i = 0; i < str.Length; i++)
            {
                var chrStr = str[i].ToString();
                var skip = true;
                for (int j = 0; j < valid.Length; j++)
                {
                    var v = valid[j];
                    if (v.Equals(chrStr, StringComparison.OrdinalIgnoreCase))
                    {
                        skip = false;
                        break;
                    }
                }
                if (!skip) strSB.Append(chrStr);
            }
            return strSB.ToString().TrimStart().TrimEnd();
        }

        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            var matches = Pool.GetList<BasePlayer>();
            try
            {
                BasePlayer player = null;
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
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
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(p);
                    }
                }
                if (sleepers)
                {

                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matches.Add(p);
                        }
                    }
                }
                var topMatch = matches?.OrderByDescending(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name, StringComparison.OrdinalIgnoreCase)) ?? null;
                if (topMatch != null && topMatch.Any())
                {
                    var exactMatches = matches?.Select(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name, StringComparison.OrdinalIgnoreCase))?.OrderByDescending(p => p) ?? null;
                    if (exactMatches.All(p => p == 0))
                    {
                        topMatch = matches?.OrderByDescending(p => Fitness(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                    }
                }
                player = topMatch?.FirstOrDefault() ?? null;
                return player;
            }
            finally { Pool.FreeList(ref matches); }
        }

        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false, params BasePlayer[] ignore)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            var matches = Pool.GetList<BasePlayer>();
            try
            {
                BasePlayer player = null;

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || (ignore != null && ignore.Length > 0 && ignore.Contains(p))) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
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
                        if (p == null || (ignore != null && ignore.Length > 0 && ignore.Contains(p))) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || ignore.Contains(p)) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(p);
                    }
                }

                if (sleepers)
                {

                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null || ignore.Contains(p)) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matches.Add(p);
                        }
                    }
                }

                var topMatch = matches?.OrderByDescending(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                if (topMatch != null && topMatch.Any())
                {
                    var exactMatches = matches?.Select(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name))?.OrderByDescending(p => p) ?? null;
                    if (exactMatches.All(p => p == 0))
                    {
                        topMatch = matches?.OrderByDescending(p => Fitness(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                    }
                }

                return topMatch?.FirstOrDefault() ?? null;
            }
            finally { Pool.FreeList(ref matches); }
        }

        private BasePlayer FindPlayerByID(ulong userID) { return BasePlayer.FindByID(userID) ?? BasePlayer.FindSleeping(userID) ?? null; }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        //CheckBoundaries taken from Nogrod's NTeleportation, with permission
        private bool CheckBoundaries(float x, float y, float z) { return x <= _boundary && x >= -_boundary && y < 2000 && y >= -100 && z <= _boundary && z >= -_boundary; }

        private float GetLowestCooldown(BasePlayer player, bool ch47 = false) //TO DO: CLEANUP THIS AND THE LIMITS METHOD BELOW IT
        {
            var perms = Pool.GetList<string>();
            var time = -1f;
            try
            {
                var getPerms = permission.GetUserPermissions(player.UserIDString);
                if (getPerms != null && getPerms.Length > 0)
                {
                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        for (int i = 0; i < getPerms.Length; i++)
                        {
                            var perm = getPerms[i];

                            if ((!ch47 && perm.IndexOf("helicontrol.cooldown.", StringComparison.OrdinalIgnoreCase) >= 0) || (ch47 && perm.IndexOf("helicontrol.cooldown.ch47", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                perms.Add(sb.Clear().Append(perm).Replace("helicontrol.", string.Empty).ToString());
                            }
                        }
                    }
                    finally { Pool.Free(ref sb); }
                }

                if (perms.Count > 0)
                {
                    var nums = Pool.GetList<float>();
                    try
                    {
                        for (int i = 0; i < perms.Count; i++)
                        {
                            var perm = perms[i];

                            if (!Cds.TryGetValue(perm, out float tempTime))
                            {
                                PrintWarning("Cooldowns dictionary does not contain: " + perm);
                                continue;
                            }

                            nums.Add(tempTime);
                        }

                        if (nums.Count > 0)
                        {
                            var lowest = -1f;
                            var last = 0f;
                            foreach (var num in nums)
                            {
                                last = num;
                                if (lowest < 0f || last < lowest) lowest = last;
                            }
                            time = lowest;
                        }
                    }
                    finally { Pool.FreeList(ref nums); }
                }

                return time;
            }
            finally { Pool.FreeList(ref perms); }
        }

        private int GetHighestLimit(BasePlayer player, bool ch47 = false)
        {
            var perms = Pool.GetList<string>();
            var limit = -1;
            try
            {
                var getPerms = permission.GetUserPermissions(player.UserIDString);
                if (getPerms != null && getPerms.Length > 0)
                {
                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        for (int i = 0; i < getPerms.Length; i++)
                        {
                            var perm = getPerms[i];

                            if ((!ch47 && perm.IndexOf("helicontrol.limit.", StringComparison.OrdinalIgnoreCase) >= 0) || (ch47 && perm.IndexOf("helicontrol.limit.ch47", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                perms.Add(sb.Clear().Append(perm).Replace("helicontrol.", string.Empty).ToString());
                            }
                        }
                    }
                    finally { Pool.Free(ref sb); }
                }

                if (perms.Count > 0)
                {
                    var nums = Pool.GetList<int>();
                    try
                    {
                        for (int i = 0; i < perms.Count; i++)
                        {
                            var perm = perms[i];

                            if (!Limits.TryGetValue(perm, out int tempInt))
                            {
                                PrintWarning("Limits dictionary does not contain: " + perm);
                                continue;
                            }

                            nums.Add(tempInt);
                        }

                        if (nums.Count > 0)
                        {
                            var highest = 0;
                            var last = 0;
                            foreach (var num in nums)
                            {
                                last = num;
                                if (last > highest) highest = last;
                            }
                            limit = highest;
                        }
                        return limit;
                    }
                    finally { Pool.FreeList(ref nums); }
                }

                return limit;
            }
            finally { Pool.FreeList(ref perms); }
        }

        private bool IgnoreCooldown(BasePlayer player) { return HasPerms(player.UserIDString, "helicontrol.ignorecooldown"); }

        private bool IgnoreLimits(BasePlayer player) { return HasPerms(player.UserIDString, "helicontrol.ignorelimits"); }


        #endregion
        #region Classes

        private class StoredData
        {
            public List<BoxInventory> HeliInventoryLists = new();
            public StoredData() { }
        }

        private class StoredData2
        {
            public Dictionary<string, float> WeaponList = new();
            public StoredData2() { }
        }

        private class StoredData3
        {
            public List<CooldownInfo> cooldownList = new();
            public StoredData3() { }
        }

        public class HelicopterSpawn
        {
            public enum HeliType { Patrol, Chinook };
            public HeliType Type = HeliType.Patrol;

            [JsonRequired]
            private string _pos = string.Empty;

            [JsonIgnore]
            public Vector3 Position
            {
                get { return string.IsNullOrEmpty(_pos) ? Vector3.zero : GetVector3FromString(_pos); }
                set { _pos = value.ToString(); }
            }

            public string Name { get; set; } = string.Empty;

            public HelicopterSpawn() { }
            public HelicopterSpawn(Vector3 position, HeliType type)
            {
                Position = position;
                Type = type;
            }

        }

        private class StoredData4 //unnecessary class, but changing would likely break existing configurations
        {
            public List<HelicopterSpawn> HelicopterSpawns = new();
            public StoredData4() { }
        }

        public HelicopterSpawn FindSpawn(string name, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!string.IsNullOrEmpty(name) && _spawnsData?.HelicopterSpawns != null)
            {
                for (int i = 0; i < _spawnsData.HelicopterSpawns.Count; i++)
                {
                    var spawn = _spawnsData.HelicopterSpawns[i];
                    if (spawn?.Name?.Equals(name, comparison) ?? false) return spawn;
                }
            }
            return null;
        }

        private class CooldownInfo
        {
            public string LastCallDay { get; set; }

            public string CooldownTime { get; set; }

            public int TimesCalled { get; set; }

            public string LastCallDayCH47 { get; set; } = string.Empty;

            public string CooldownTimeCH47 { get; set; } = string.Empty;

            public int TimesCalledCH47 { get; set; }

            public ulong UserID { get; set; }

            public CooldownInfo() { }

            public CooldownInfo(BasePlayer newPlayer) { UserID = newPlayer?.userID ?? 0; }

            public CooldownInfo(string userID)
            {
                if (ulong.TryParse(userID, out ulong newUID)) UserID = newUID;
            }

            public CooldownInfo(ulong userID) { UserID = userID; }

        }

        private class BoxInventory
        {
            public List<ItemDef> lootBoxContents = new();

            public BoxInventory() { }

            public BoxInventory(List<ItemDef> list) { lootBoxContents = list; }

            public BoxInventory(List<Item> list)
            {
                if (list == null || list.Count < 1) return;
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item == null) continue;

                    lootBoxContents.Add(new ItemDef(item.info.shortname, item.amount, item.skin));
                }
            }

            public BoxInventory(string name, int amount, int amountMin = 0, int amountMax = 0, ulong skinID = 0)
            {
                if (amountMin > 0 && amountMax > 0) amount = UnityEngine.Random.Range(amountMin, amountMax);
                lootBoxContents.Add(new ItemDef(name, amount, skinID));
            }

        }

        private class ItemDef
        {
            public string name;
            public int amountMin;
            public int amountMax;
            public int amount;
            public ulong skinID;

            public ItemDef() { }

            public ItemDef(string name, int amount, ulong skinID = 0)
            {
                this.name = name;
                this.amount = amount;
                this.skinID = skinID;
            }
        }
        #endregion
        #region Data
        private void LoadLootData()
        {
            _lootData = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("HeliControlData") ?? null;
            var count = _lootData?.HeliInventoryLists?.Count ?? 0;
            //Create a default data file if there was none:
            if (_lootData == null || _lootData.HeliInventoryLists == null || count < 1)
            {
                Puts("No Lootdrop Data found, creating new file...");
                _lootData = new StoredData();
                BoxInventory inv;
                inv = new BoxInventory("rifle.ak", 1);
                inv.lootBoxContents.Add(new ItemDef("ammo.rifle.hv", 128));
                _lootData.HeliInventoryLists.Add(inv);

                inv = new BoxInventory("rifle.bolt", 1);
                inv.lootBoxContents.Add(new ItemDef("ammo.rifle.hv", 128));
                _lootData.HeliInventoryLists.Add(inv);

                inv = new BoxInventory("explosive.timed", 3);
                inv.lootBoxContents.Add(new ItemDef("ammo.rocket.hv", 3));
                _lootData.HeliInventoryLists.Add(inv);

                inv = new BoxInventory("lmg.m249", 1);
                inv.lootBoxContents.Add(new ItemDef("ammo.rifle", 100));
                _lootData.HeliInventoryLists.Add(inv);

                inv = new BoxInventory("rifle.lr300", 1);
                inv.lootBoxContents.Add(new ItemDef("ammo.rifle", 100));
                _lootData.HeliInventoryLists.Add(inv);

                SaveLootData();
            }
            else
            {
                var invalidSB = new StringBuilder();
                for (int i = 0; i < _lootData.HeliInventoryLists.Count; i++)
                {
                    var inv = _lootData.HeliInventoryLists[i];
                    if (inv == null || inv?.lootBoxContents == null || inv.lootBoxContents.Count < 1) continue;
                    for (int j = 0; j < inv.lootBoxContents.Count; j++)
                    {
                        var content = inv.lootBoxContents[j];
                        if (content == null) continue;
                        var findDef = ItemManager.FindItemDefinition(content.name);
                        if (findDef == null) invalidSB.AppendLine("Invalid item name in loot table: ").Append(content.name);
                    }
                }
                if (invalidSB.Length > 0) PrintWarning(invalidSB.ToString());
            }
        }

        private void LoadWeaponData()
        {
            _weaponsData = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData2>("HeliControlWeapons") ?? null;
            var count = _weaponsData?.WeaponList?.Count ?? 0;
            if (_weaponsData == null || _weaponsData.WeaponList == null || count < 1)
            {
                Puts("No weapons data found, creating new file...");
                _weaponsData = new StoredData2();
                var itemDefs = ItemManager.itemList;
                if (itemDefs != null && itemDefs.Count > 0)
                {
                    for (int i = 0; i < itemDefs.Count; i++)
                    {
                        var itemdef = itemDefs[i];
                        if (itemdef == null) continue;
                        var category = itemdef.category;
                        if (category != ItemCategory.Weapon) continue;
                        var shortName = itemdef.shortname;
                        var englishName = itemdef.displayName?.english ?? shortName;
                        if (!shortName.Contains("weapon.mod")) _weaponsData.WeaponList[englishName] = 1f;
                    }
                }
                SaveWeaponData();
            }
        }

        private void LoadHeliSpawns() => _spawnsData = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData4>("HeliControlSpawns") ?? new StoredData4();


        private void SaveLootData() => Interface.Oxide.DataFileSystem.WriteObject("HeliControlData", _lootData);
        private void SaveWeaponData() => Interface.Oxide.DataFileSystem.WriteObject("HeliControlWeapons", _weaponsData);
        private void SaveCooldownData() => Interface.Oxide.DataFileSystem.WriteObject("HeliControlCooldowns", _cooldownData);
        private void SaveSpawnData() => Interface.Oxide.DataFileSystem.WriteObject("HeliControlSpawns", _spawnsData);
    }
    #endregion
}
