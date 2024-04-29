using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Oxide.Plugins
{
    [Info("RCONTools", "MBR", "1.0.3")]
    class RCONTools : RustPlugin
    {
        [PluginReference] Plugin BanSystem;

        private readonly FieldInfo mapField = typeof(CompanionServer.Handlers.Map).GetField("_imageData", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly FieldInfo sizeField = typeof(CompanionServer.Handlers.Map).GetField("_width", BindingFlags.Static | BindingFlags.NonPublic);
        List<object> items = new List<object>();
        private string image = null;
        private int size = 0;

        private void OnServerInitialized()
        {
            timer.Every(1f, () =>
            {
                if (ServerInfoEx.buffer.Count == 120) ServerInfoEx.buffer.RemoveAt(0);
                ServerInfoEx.buffer.Add(new ServerInfoEx());
            });

            if (GetMap() == null)
            {
                CompanionServer.Handlers.Map.PopulateCache();
            }

            image = Convert.ToBase64String(GetMap());
            size =  (int)sizeField.GetValue(null);
            items = ItemManager.itemList.Select(x => (object)new { id = x.itemid, shortname = x.shortname, name = x.displayName.english }).ToList();
        }

        private byte[] GetMap()
        {
            return mapField.GetValue(null) as byte[];
        }

        class ServerInfoEx
        {
            public int EntityCount;
            public float Framerate;
            public long Memory;
            public int NetworkIn;
            public int NetworkOut;
            public DateTime _Time = DateTime.UtcNow;

            public ServerInfoEx()
            {
                var o = ConVar.Admin.ServerInfo();
                EntityCount = o.EntityCount;
                Framerate = o.Framerate;
                Memory = Performance.report.memoryUsageSystem;
                NetworkIn = o.NetworkIn;
                NetworkOut = o.NetworkOut;
            }

            public static List<ServerInfoEx> buffer = new List<ServerInfoEx>();
        }

        [ConsoleCommand("performance.now")]
        private void PerformanceTimeCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            arg.ReplyWithObject(new ServerInfoEx());
        }

        [ConsoleCommand("performance.tail")]
        private void PerformanceTailCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            arg.ReplyWithObject(ServerInfoEx.buffer);
        }

        [ConsoleCommand("map.data")]
        private void MapDataCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            List<object> monuments = new List<object>();

            foreach (MonumentInfo m in TerrainMeta.Path.Monuments.Where(x => x.shouldDisplayOnMap))
            {
                monuments.Add(new
                {
                    name = m.displayPhrase.english,
                    x = m.transform.position.x,
                    y = m.transform.position.z
                });
            }

            arg.ReplyWithObject(new
            {
                size = size,
                image = image,
                monuments = monuments
            });
        }

        [ConsoleCommand("map.players")]
        private void GetMapImageCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            List<object> rv = new List<object>();

            foreach (var player in BasePlayer.activePlayerList)
            {
                rv.Add(new
                {
                    id = player.UserIDString,
                    name = player.displayName,
                    x = player.transform.position.x,
                    y = player.transform.position.z,
                    r = player?.serverInput?.current?.aimAngles.y ?? 0
                });
            }

            arg.ReplyWithObject(rv);
        }

        [ConsoleCommand("baninfo.json")]
        private void BanInfoCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null || arg.Args == null || arg.Args.Length < 1) return;

            var info = BanSystem?.Call<string>("GetBanInfoJSON", arg.Args[0]) ?? "{}";

            arg.ReplyWith(info);
        }

        [ConsoleCommand("inventory.itemlist")]
        private void ItemListCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            arg.ReplyWith(items);
        }

        [ConsoleCommand("inventory.player")]
        private void PlayerInvCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null || arg.Args == null || arg.Args.Length < 1) return;

            var player = BasePlayer.FindAwakeOrSleeping(arg.Args[0]);
            if (player == null) return;

            var obj = new
            {
                belt = player.inventory.containerBelt.itemList.Select(x => new { id = x.info.itemid, amount = x.amount }),
                main = player.inventory.containerMain.itemList.Select(x => new { id = x.info.itemid, amount = x.amount }),
                wear = player.inventory.containerWear.itemList.Select(x => new { id = x.info.itemid, amount = x.amount }),
            };

            arg.ReplyWith(obj);
        }
    }
}
