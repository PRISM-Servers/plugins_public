namespace Oxide.Plugins
{
    [Info("Project Red | NPC Handler", "Shady", "0.0.3")]
    class RedNpcHandler : RustPlugin
    {
        /*/
        private void OnEntityTakeDamage(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return;

            var attacker = info?.Initiator;

            var attackerPlayer = attacker as BasePlayer;

            if (attackerPlayer == null || attackerPlayer.IsConnected) return;


            info.damageTypes.ScaleAll(UnityEngine.Random.Range(1.275f, 1.7f));

        }/*/
    }
}
