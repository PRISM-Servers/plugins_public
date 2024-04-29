using Facepunch.Extend;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("HotAirDrops", "MBR", "1.1.1")]
    class HotAirDrops : RustPlugin
    {
        #region Constants

        private const int CRATE_UNTOUCHED = 69;
        private const int CRATE_LOOTED = 70;

        #endregion

        #region Hooks

        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (entity is HackableLockedCrate && entity.OwnerID == CRATE_UNTOUCHED)
            {
                Server.Broadcast("<size=18>The crate dropped by the <color=orange>hot air balloon</color> is now being <color=#FF7930>looted!</color></size>");
                entity.OwnerID = CRATE_LOOTED;
            }
        }

        private object CanSamSiteShoot(SamSite sam)
        {
            if (sam == null || sam.IsDestroyed || sam.currentTarget == null) return null;

            var balloon = sam.currentTarget as HotAirBalloon;
            if (balloon == null) return null;

            if (balloon.GetComponent<CustomHotAirBalloon>() != null) return false;

            return null;
        }

        #endregion

        #region Classes

        private class CustomCargoPlane : MonoBehaviour
        {
            private enum Progress
            {
                Vibing,
                LeftBank,
                LeftCircle,
                RightBank
            }

            private const float rotationRadius = 100f;
            private const float rotationSpeed = 40f;
            private const float bankValue = 10f;
            private readonly Vector3 rotationAxis = new Vector3(0f, 1f, 0f);
            private readonly float delta = rotationSpeed * Time.fixedDeltaTime;
            private readonly List<Progress> flightPlan = new List<Progress> { Progress.Vibing, Progress.LeftBank, Progress.LeftCircle, Progress.RightBank, Progress.Vibing };

            private float secondsTaken = 0f, secondsToTake = 0f;
            private float bankTimeToTake = 1f, bankTimeTaken = 0f;
            private float totalDegrees = 0f;

            private float bank
            {
                get
                {
                    return bankValue * (state == Progress.RightBank ? -1 : 1);
                }
            }

            private CargoPlane plane;
            private Progress state = Progress.Vibing;
            private bool startedCircling = false;

            private Vector3 startPos, endPos;
            private Vector3 rotationVector = Vector3.zero;

            public BoxStorage lootSource;

            private void Awake()
            {
                plane = GetComponent<CargoPlane>();
                plane.enabled = false;

                startPos = plane.transform.position;
                endPos = plane.endPos;
                startPos.y = 500f;
                endPos.y = 500f;

                secondsToTake = Vector3.Distance(startPos, endPos) / 50f;
                secondsToTake *= UnityEngine.Random.Range(0.95f, 1.05f);
                plane.transform.position = startPos;
                plane.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            }

            private void FixedUpdate()
            {
                if (plane == null || plane.IsDestroyed) return;

                try
                {
                    TickBankProcess();

                    if (state == Progress.LeftCircle)
                    {
                        //drop the crate at half way through the circle
                        if (totalDegrees == 180) DropHAB(plane.transform.position, lootSource);

                        //exit circle
                        if (totalDegrees == 350 && state == Progress.LeftCircle)
                        {
                            state = Progress.RightBank;
                        }
                        
                        plane.transform.RotateAround(rotationVector, rotationAxis, -delta);
                        totalDegrees += delta;
                        return;
                    }

                    if (state == Progress.Vibing || state == Progress.LeftBank || state == Progress.RightBank)
                    {
                        secondsTaken += Time.deltaTime;
                        float single = Mathf.InverseLerp(0f, secondsToTake, secondsTaken);

                        plane.transform.position = Vector3.Lerp(startPos, endPos, single);
                        if (state == Progress.Vibing) plane.transform.rotation = Quaternion.LookRotation(endPos - startPos);

                        //start circling at half way
                        if (!startedCircling && single >= 0.5f)
                        {
                            startedCircling = true;
                            state = Progress.LeftBank;
                            //set the point around which the plane will circle
                            rotationVector = plane.transform.position + plane.transform.right * -rotationRadius;
                        }

                        if (single >= 1f) plane.Kill(BaseNetworkable.DestroyMode.None);
                    }
                }

                catch (Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString());
                    Interface.Oxide.LogWarning("FixedUpdate errored, calling DoDestroy()!");
                    GameObject.Destroy(this);
                }
            }

            private void TickBankProcess()
            {
                if (state != Progress.LeftBank && state != Progress.RightBank) return;

                bankTimeTaken += Time.deltaTime;
                float single = Mathf.InverseLerp(0f, bankTimeToTake, bankTimeTaken);
                plane.transform.Rotate(new Vector3(0f, 0f, bank), single);

                if (single == 1f)
                {
                    state = flightPlan[flightPlan.IndexOf(state) + 1];
                    bankTimeTaken = 0f;
                }
            }
        }

        private class CustomHotAirBalloon : MonoBehaviour
        {
            private HotAirBalloon HAB = null;
            private HackableLockedCrate HLC = null;

            public BoxStorage lootSource;

            private void Awake()
            {
                HAB = GetComponent<HotAirBalloon>();
                HLC = GetComponentInChildren<HackableLockedCrate>();

                HAB.fuelSystem.AdminAddFuel();
                HAB.inflationLevel = 1f;
                HAB.liftAmount = 485f;
                HAB.SetFlag(BaseEntity.Flags.On, true);
                HAB.SendNetworkUpdateImmediate();

                InvokeHandler.InvokeRepeating(this, LandCheck, 1f, 1f);

                HLC.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                HLC.SendNetworkUpdate();
                HLC.GetComponent<Rigidbody>().isKinematic = true;
                HLC.GetComponent<Rigidbody>().detectCollisions = false;
                HLC.OwnerID = CRATE_UNTOUCHED;
                HLC.enableSaving = false;
                HLC.SetWasDropped();
                HLC.hackSeconds = HackableLockedCrate.requiredHackSeconds - 1f;
                HLC.inventory.capacity = 36;

                InvokeHandler.Invoke(this, () =>
                {
                    foreach (var item in HLC.inventory.itemList.ToList())
                    {
                        item.RemoveFromContainer();
                        item.RemoveFromWorld();
                    }

                    if (lootSource != null && lootSource.inventory.itemList.Count > 0)
                    {
                        for (int i = 0; i < lootSource.inventory.itemList.Count; i++)
                        {
                            var item = lootSource.inventory.itemList[i];
                            if (item != null) item.MoveToContainer(HLC.inventory);
                        }
                    }

                    InvokeHandler.Invoke(this, () =>
                    {
                        lootSource.KillMessage();
                    }, 1f);
                }, 1f);
            }

            private void LandCheck()
            {
                var ray = new Ray(transform.position + (Vector3.up * 0.5f), Vector3.down);

                if (Physics.Raycast(ray, 5f, 1084293377)) Drop();
            }

            public void Drop()
            {
                InvokeHandler.CancelInvoke(this, LandCheck);

                InvokeHandler.Invoke(this, () =>
                {
                    HAB.liftAmount = 1000f;
                    HAB.SetFlag(BaseEntity.Flags.On, true);
                    HAB.SendNetworkUpdateImmediate();

                    InvokeHandler.Invoke(this, () =>
                    {
                        if (HAB != null && !HAB.IsDestroyed) HAB.KillMessage();
                    }, 50f);
                }, 1f);
                
                HLC.SetParent(null, true, false);
                var y = Mathf.Max(HLC.transform.position.y - 2, TerrainMeta.HeightMap.GetHeight(HLC.transform.position));
                HLC.transform.position = new Vector3(HLC.transform.position.x, y, HLC.transform.position.z);
                HLC.SendNetworkUpdate();

                HLC.GetComponent<Rigidbody>().isKinematic = false;
                HLC.GetComponent<Rigidbody>().detectCollisions = true;
            }
        }

        #endregion

        #region Commands

        [ChatCommand("hotairdrop")]
        private void SpawnHotAirBaloonCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You don't have permission to use this command.");
                return;
            }

            var ent = GetLookAtEntity(player, 5f) as BoxStorage;
            if (ent == null)
            {
                SendReply(player, "You are not looking at a box!");
                return;
            }

            if (ent.inventory.itemList.Count < 1)
            {
                SendReply(player, "The box is empty!");
                return;
            }

            var plane = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab", Vector3.zero) as CargoPlane;
            plane.Spawn();

            var component = plane.gameObject.AddComponent<CustomCargoPlane>();
            component.lootSource = ent;

            Server.Broadcast("<size=18>A <color=orange>hot air balloon</color> carrying <color=#FF7930>loot taken from cheaters</color> will be dropping soon! Be sure to watch the map for a moving locked crate.</size>");

            ent.transform.position = Vector3.zero;
            ent.SendNetworkUpdateImmediate();
        }

        #endregion

        #region Helpers

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250)
        {
            if (player == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (Physics.Raycast(ray, out hit, maxDist))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }

        private static void DropHAB(Vector3 position, BoxStorage lootSource)
        {
            var HAB = "assets/prefabs/deployable/hot air balloon/hotairballoon.prefab";
            var HLC = "assets/prefabs/deployable/chinooklockedcrate/codelockedhackablecrate.prefab";

            try
            {
                var hotair = GameManager.server.CreateEntity(HAB, position) as HotAirBalloon;
                hotair.Spawn();

                var box = GameManager.server.CreateEntity(HLC, hotair.transform.position) as HackableLockedCrate;
                box.Spawn();

                box.SetParent(hotair, true, false);

                var component = hotair.gameObject.AddComponent<CustomHotAirBalloon>();
                component.lootSource = lootSource;
            }

            catch (Exception) { }
        }

        #endregion
    }
}