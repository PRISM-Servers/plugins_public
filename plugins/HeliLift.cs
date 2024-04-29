using UnityEngine;

namespace Oxide.Plugins
{
    [Info("HeliLift", "MBR", "1.0.1")]
    class HeliLift : RustPlugin
    {
        [ChatCommand("lift")]
        private void LiftCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You don't have permission to use this command.");
                return;
            }

            var ent = GetLookAtEntity(player) as BaseHelicopter;
            if (ent == null)
            {
                SendReply(player, "No heli");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "Current lift: " + ent.liftFraction);
                return;
            }

            float val = 0;
            if (!float.TryParse(args[0], out val))
            {
                SendReply(player, args[0] + " is not a valid number");
                return;
            }

            ent.liftFraction = val;
            SendReply(player, "set lift to " + val);
        }

        [ChatCommand("thrust")]
        private void ThrustCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You don't have permission to use this command.");
                return;
            }

            var ent = GetLookAtEntity(player) as BaseHelicopter;
            if (ent == null)
            {
                SendReply(player, "No heli");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "Current trust: " + ent.engineThrustMax);
                return;
            }

            float val = 0;
            if (!float.TryParse(args[0], out val))
            {
                SendReply(player, args[0] + " is not a valid number");
                return;
            }

            ent.engineThrustMax = val;
            SendReply(player, "set trust to " + val);
        }

        [ChatCommand("liftspeed")]
        private void LiftSpeedCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You don't have permission to use this command.");
                return;
            }

            var ent = GetLookAtEntity(player) as Elevator;
            if (ent == null)
            {
                SendReply(player, "No heli");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "Current lift speed: " + ent.LiftSpeedPerMetre);
                return;
            }

            float val = 0;
            if (!float.TryParse(args[0], out val))
            {
                SendReply(player, args[0] + " is not a valid number");
                return;
            }

            ent.LiftSpeedPerMetre = val;
            SendReply(player, "set lift to " + val);
        }

        BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250)
        {
            if (player == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (UnityEngine.Physics.Raycast(ray, out hit, maxDist))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }
    }
}
