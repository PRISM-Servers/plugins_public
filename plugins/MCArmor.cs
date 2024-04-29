namespace Oxide.Plugins
{
    [Info("MCArmor", "MBR", "1.0.3")]
    [Description("Minecraft-style armor equipping (left click while holding an item)")]
    class MCArmor : RustPlugin
    {
        void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player == null || input == null) return;

            if (input.WasJustPressed(BUTTON.FIRE_SECONDARY))
            {
                var item = player.GetActiveItem();
                if (item == null || item.info == null) return;

                var t = item.info.GetComponent<ItemModWearable>();
                if (t == null) return;

                var container = player.inventory.containerWear;
                if (container.itemList.Count == container.capacity) return;

                foreach (var i in container.itemList.ToArray())
                {
                    var c = i.info.GetComponent<ItemModWearable>();
                    if (!t.CanExistWith(c)) return;
                }

                if (item.MoveToContainer(player.inventory.containerWear))
                {
                    var fx = "assets/prefabs/deployable/locker/sound/equip_zipper.prefab";
                    EffectNetwork.Send(new Effect(fx, player.transform.position, UnityEngine.Vector3.zero), player.net.connection);
                }
            }
        }
    }
}