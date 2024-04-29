using System.Collections.Generic;
using UnityEngine;
using Facepunch;
using Rust;
using System;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("CH47 Hijack", "Shady", "1.0.5")]
    internal class CH47Hijack : RustPlugin
    {
        #region Fields

        private const uint CHINOOK_SCIENTISTS_PREFAB_ID = 1514383717;
        private const uint CHINOOK_CONTROLLABLE_PREFAB_ID = 1675349834;
        private const uint CHINOOK_PILOT_SEAT_PREFAB_ID = 952100854;

        private const string CHINOOK_CONTROLLABLE_PREFAB = "assets/prefabs/npc/ch47/ch47.entity.prefab";

        private readonly Dictionary<string, float> _lastUsePress = new Dictionary<string, float>();

        private readonly List<float> _oldProtection = new List<float>();

        private ProtectionProperties _properties;

        private bool _configChanged = false;

        #region Plugin References
        [PluginReference]
        private readonly Plugin SAMSiteAuth;
        #endregion

        #endregion
        #region Config

        #region Config Fields

        private float _bulletDamageScale;
        private float _explosiveDamagescale;

        private float _sphereRadius;
        private float _sphereDistance;

        private bool _enableSaving;
        private bool _controlWithScientists;
        private bool _targetedBySams;

        #endregion

        protected override void LoadDefaultConfig()
        {
            _bulletDamageScale = GetConfig("Bullet Damage Scale", 0.8f);
            _explosiveDamagescale = GetConfig("Explosive Damage Scale", 1f);

            _sphereRadius = GetConfig("Sphere Radius", 0.75f);
            _sphereDistance = GetConfig("Sphere Distance", 1.5f);

            _enableSaving = GetConfig("Enable Saving", false);
            _controlWithScientists = GetConfig("Control with scientists", false);
            _targetedBySams = GetConfig("Targeted by SAMs", true);

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
        #endregion
        #region Hooks
        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
            if (!_configChanged) LoadDefaultConfig(); //don't call load again if it was already called because config file didn't exist. tiny optimization

            if (_configChanged) SaveConfig(); //config could have been changed after a manual call of loaddefaultconfig, so we check again

        }

        private void Unload()
        {
            if (_properties == null)
                return;
            

            for (int i = 0; i < _oldProtection.Count; i++)
            {
                var prot = _oldProtection[i];
                _properties.amounts[i] = prot;
            }

        }

        private void OnServerInitialized()
        {
            if (_targetedBySams) Subscribe(nameof(OnSamSiteTarget));
            else Unsubscribe(nameof(OnSamSiteTarget));

            if (!_controlWithScientists) Unsubscribe(nameof(CanMountEntity));
            else Subscribe(nameof(CanMountEntity));

            Subscribe(nameof(OnEntitySpawned));
        }

        private object OnSamSiteTarget(SamSite sam, BaseCombatEntity entity)
        {
            if (entity == null) return null;

            var ch47 = entity as CH47Helicopter;
            if (ch47 == null) return null;

            var pilot = ch47?.mountPoints[0]?.mountable?._mounted;
            if (pilot != null && pilot.IsNpc)
            {

                //before returning, lets do a plugin check to make sure we aren't conflicting
                if (SAMSiteAuth?.IsLoaded ?? false)
                {
                    var onTarget = SAMSiteAuth?.Call<object>(nameof(OnSamSiteTarget)) ?? null;
                    if (onTarget != null) return null; //got a non-null value from SAMSiteAuth, so we'll let it handle the hook.
                }

                return true; //returning true overrides default behavior, meaning we won't let it target!
            }

            return null;
        }

        private object CanMountEntity(BasePlayer player, BaseMountable mountable)
        {
            return (player != null && mountable?.prefabID == CHINOOK_CONTROLLABLE_PREFAB_ID && player is NPCPlayer && mountable?.GetComponentInParent<CH47Helicopter>()?.prefabID == CHINOOK_CONTROLLABLE_PREFAB_ID) ? (object)false : null;
        }


        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player == null || input == null || player.IsDead() || player.isMounted || !input.WasJustPressed(BUTTON.USE)) return;

            var now = Time.realtimeSinceStartup;
            float lastTime;

            var userId = player?.UserIDString ?? string.Empty;

            if (_lastUsePress.TryGetValue(userId, out lastTime) && (now - lastTime) < 0.5) return;

            try
            {

                var ray = new Ray(player?.eyes?.position ?? Vector3.zero, player.eyes.rotation * Vector3.forward);


                CH47HelicopterAIController lookEnt = null;

                RaycastHit hit;

                if (Physics.SphereCast(ray, _sphereRadius, out hit, _sphereDistance, Layers.Server.VehiclesSimple))
                    lookEnt = hit.GetEntity() as CH47HelicopterAIController;


                if (lookEnt?.prefabID != CHINOOK_SCIENTISTS_PREFAB_ID) return; //not scientists ch47



                for (int i = 0; i < lookEnt.mountPoints.Count; i++)
                {
                    var mountable = lookEnt.mountPoints[i]?.mountable;
                    if (mountable == null || mountable._mounted != null) continue;

                    player.EnsureDismounted();
                    mountable.MountPlayer(player);

                    break;
                }

            }
            finally { _lastUsePress[userId] = now; }

        }

        private void OnSamSiteTargetScan(SamSite sam, List<SamSite.ISamSiteTarget> list)
        {
            if (sam == null || list == null) return;

            var ch47Near = Pool.GetList<CH47Helicopter>();
            try 
            {
                Vis.Entities(sam.transform.position, 100f, ch47Near, Layers.Server.VehiclesSimple);

                for (int i = 0; i < ch47Near.Count; i++)
                {
                    var ch47 = ch47Near[i];
                    if (ch47 == null || ch47.prefabID != CHINOOK_CONTROLLABLE_PREFAB_ID) continue;

                    var comp = ch47.GetComponent<SAMComponent>();

                    if (comp != null && !list.Contains(comp)) list.Add(comp);
                }

            }
            finally { Pool.FreeList(ref ch47Near); }
        }

        private void OnEntityMounted(BaseMountable mountable, BasePlayer player)
        {
            if (mountable == null || player == null || player.gameObject == null || player.IsDestroyed || player.IsNpc || !player.IsConnected) return;

            var ch47 = mountable?.GetComponentInParent<CH47HelicopterAIController>();
            if (ch47?.mountPoints == null) return;


            if (!_controlWithScientists)
            {
                for (int i = 0; i < ch47.mountPoints.Count; i++)
                {
                    var point = ch47.mountPoints[i];
                    var mountedPlayer = point?.mountable?._mounted;
                    if (mountedPlayer == null) continue;

                    if (mountedPlayer is NPCPlayer) return;
                }
            }


            var playersToRemount = Pool.GetList<BasePlayer>();

            try
            {
                for (int i = 0; i < ch47.mountPoints.Count; i++)
                {
                    var point = ch47.mountPoints[i];
                    var mountedPlayer = point?.mountable?._mounted;

                    if (mountedPlayer == null) continue;

                    var aiPlayer = mountedPlayer as NPCPlayer;

                    if ((!_controlWithScientists && aiPlayer != null) || i == 0 && aiPlayer != null && !aiPlayer.IsDead())
                    {
                        //no control with scientists *OR* pilot is alive & AI
                        return;
                    }


                    if (aiPlayer != null) aiPlayer.EnsureDismounted();
                    else mountedPlayer.EnsureDismounted();

                    playersToRemount.Add(mountedPlayer);
                }


                var oldPos = ch47?.transform?.position ?? Vector3.zero;
                var oldRot = ch47?.transform?.rotation ?? Quaternion.identity;

                var mapMarker = ch47?.GetComponent<MapMarkerCH47>();
                if (mapMarker != null) mapMarker.Kill();


                if (_controlWithScientists) ch47.Kill();
                else ch47.DelayedKill();

                var newCh47 = GameManager.server.CreateEntity(CHINOOK_CONTROLLABLE_PREFAB, oldPos, oldRot) as CH47Helicopter;
                newCh47.Spawn();
                newCh47.OwnerID = player?.userID ?? 0;

                newCh47.EnableSaving(_enableSaving);

                if (_targetedBySams) newCh47.gameObject.AddComponent<SAMComponent>();

                if (_properties == null)
                {
                    _properties = newCh47.baseProtection;

                    for (int i = 0; i < _properties.amounts.Length; i++)
                    {
                        var prot = _properties.amounts[i];

                        _oldProtection.Add(prot);
                    }
                }

                newCh47.baseProtection.amounts[(int)DamageType.Bullet] = _bulletDamageScale;
                newCh47.baseProtection.amounts[(int)DamageType.Explosion] = _explosiveDamagescale;

                for (int i = 0; i < playersToRemount.Count; i++)
                {
                    var p = playersToRemount[i];
                    if (p == null || p.IsDead() || (!p.IsNpc && !p.IsConnected) || p.IsSleeping()) continue;



                    var aiPlayer = p as NPCPlayer;

                    if (aiPlayer != null) aiPlayer.EnsureDismounted();
                    else p.EnsureDismounted();

                    for (int j = 0; j < newCh47.mountPoints.Count; j++)
                    {
                        var _mountable = newCh47.mountPoints[j]?.mountable;
                        if (_mountable == null) continue;

                        if (!_enableSaving) _mountable.EnableSaving(false); //savelist bug fix

                        if (_mountable?._mounted == null)
                        {

                            if (aiPlayer != null) aiPlayer.MountObject(_mountable);
                            else _mountable.MountPlayer(p);

                            break;
                        }
                    }
                }

            }
            finally { Pool.FreeList(ref playersToRemount); }
            
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var ch47 = entity as CH47HelicopterAIController;

            if (ch47?.baseProtection == null) 
                return;

            for (int i = 0; i < _oldProtection.Count; i++)
            {
                var prot = _oldProtection[i];
                ch47.baseProtection.amounts[i] = prot;
            }

        }

        private class SAMComponent : FacepunchBehaviour, SamSite.ISamSiteTarget
        {

            private CH47Helicopter _heli;

            private void Awake()
            {
                _heli = GetComponent<CH47Helicopter>();

                if (_heli == null)
                {
                    DoDestroy();
                    return;
                }
            }

            public void DoDestroy() => Destroy(this);
            

            public SamSite.SamTargetType SAMTargetType
            {
                get { return SamSite.targetTypeVehicle; }
            }

            public bool isClient => false;

            public Vector3 CenterPoint()
            {
                return _heli?.CenterPoint() ?? Vector3.zero;
            }

            public Vector3 GetWorldVelocity()
            {
                return _heli?.GetWorldVelocity() ?? Vector3.zero;
            }

            public bool IsValidSAMTarget(bool staticRespawn) { return true; }
            

            public bool IsVisible(Vector3 position, float maxDistance = float.PositiveInfinity)
            {
                return _heli?.IsVisible(position, maxDistance) ?? false;
            }
        }

        #endregion
    }
}