using System.IO;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("broken", "MBR", "1.0.0")]
    [Description("plugin for testing random snippets of code")]
    class broken : RustPlugin
    {
        [ChatCommand("testsign")]
        private void TestSignCMD(BasePlayer player, string command, string[] args)
        {
            var ent = GetLookAtEntity(player, 5f) as Signage;
            if (ent == null)
            {
                SendReply(player, "No sign");
                return;
            }

            var bytes = FileStorage.server.Get(ent.textureIDs[0], FileStorage.Type.png, ent.net.ID);

            File.WriteAllBytes("test1.png", bytes);
            SendReply(player, "Saved to file");
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
