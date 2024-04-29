using Oxide.Core;
using System.Linq;
using System.Reflection;

namespace Oxide.Plugins
{
    [Info("TimerKiller", "MBR", 1.0)]
    [Description("Because stuck timers never happen")]
    class TimerKiller : RustPlugin
    {
        [ConsoleCommand("killtimer")]
        private void KillTimerCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            var args = arg.Args;
            if (args == null || args.Length < 1) return;

            KillTimers(args[0]);
        }

        private void KillTimers(string TargetPlugin)
        {
            Puts("Unloading target plugin");
            Interface.Oxide.UnloadPlugin(TargetPlugin);

            Puts("Fetching timer stuff thru reflection which will be disabled but that's ok since timers can't get stuck");
            var lib = Interface.Oxide.GetLibrary<Core.Libraries.Timer>(null);

            FieldInfo f = typeof(Core.Libraries.Timer).GetField("timeSlots", BindingFlags.Instance | BindingFlags.NonPublic);

            var val = f.GetValue(lib) as Core.Libraries.Timer.TimeSlot[];
            var list = val.Where(x => x?.FirstInstance?.Owner?.Name == TargetPlugin);

            Puts("Found " + list.Count() + " timers, about to try to kill them properly");
            if (list.Count() < 1) return;

            Puts("Looping through timers");
            foreach (var timer in list)
            {
                Puts("About to kill timer with delay: " + timer.FirstInstance.Delay + " rep:" + timer.FirstInstance.Repetitions);
                timer.FirstInstance.Destroy();
            }
        }
    }
}