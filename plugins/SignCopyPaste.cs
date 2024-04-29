using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SignCopyPaste", "MBR", "1.0.1")]
    class SignCopyPaste : RustPlugin
    {
        private Dictionary<BasePlayer, uint[]> textureDictionary = new Dictionary<BasePlayer, uint[]>();

        [ChatCommand("signcopy")]
        private void SignCopyCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You don't have permission to use this command.");
                return;
            }

            var ent = GetLookAtEntity(player, 10f) as Signage;
            if (ent == null)
            {
                SendReply(player, "You are not looking at a sign");
                return;
            }

            if (textureDictionary.ContainsKey(player)) textureDictionary[player] = ent.textureIDs;
            else textureDictionary.Add(player, ent.textureIDs);
            SendReply(player, "Successfully saved image, use /signpaste to paste the picture onto a sign");
        }

        [ChatCommand("signpaste")]
        private void SignPasteCMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "You don't have permission to use this command.");
                return;
            }

            var ent = GetLookAtEntity(player, 10f) as Signage;
            if (ent == null)
            {
                SendReply(player, "You are not looking at a sign");
                return;
            }

            if (!textureDictionary.ContainsKey(player))
            {
                SendReply(player, "You did not save a sign");
                return;
            }

            ent.textureIDs = textureDictionary[player];
            ent.SendNetworkUpdate();
        }

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
    }
}
