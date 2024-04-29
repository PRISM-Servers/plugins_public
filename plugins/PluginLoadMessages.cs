using Oxide.Core.Plugins;
using System.Text;

namespace Oxide.Plugins
{
    [Info("PluginLoadMessages", "MBR", "1.0.1")]
    class PluginLoadMessages : CovalencePlugin
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private void OnPluginLoaded(Plugin plugin)
        {
            
            foreach(var p in covalence.Players.Connected)
            {
                if (p == null || !p.IsAdmin) continue;

                p.Message(_stringBuilder.Clear().Append("Loaded plugin <color=#ef1f4c>").Append(plugin.Name).Append("</color><size=10> v</size>").Append(plugin.Version.ToString()).ToString());
            }
        }
    }
}