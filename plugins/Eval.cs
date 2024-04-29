using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Eval", "MBR", "1.1.2")]
    class Eval : CovalencePlugin
    {
        // TODO
        // looking at (player eyes)
        // 

        private void Init()
        {
            AddCovalenceCommand("eval", "EvalCmd");
            AddCovalenceCommand("evallook", "EvalCmd");
        }

        private void EvalCmd(IPlayer player, string command, string[] args)
        {
            var usage = "<class> <'method'|'field'|'property'> <name> [arguments for calling|newvalue]? (if not provided shows current value/info)\nUse /evallook to eval on looking entity";
            if (player == null || !player.IsAdmin)
            {
                player.Reply("You do not have permission to use this command.");
                return;
            }

            if (args.Length < 3)
            {
                player.Reply("Usage: /" + command + " " + usage);
                return;
            }

            var cls = args[0];
            var type = args[1];
            var name = args[2];
            var newval = args.Length > 3 ? args[3] : null;
            var args2 = args.Length > 3 ? args.Skip(3).ToArray() : null;

            var ent = command == "evallook" ? GetLookAtEntity(player.Object as BasePlayer, 20f) : null;
            if (command == "evallook" && ent == null)
            {
                player.Reply("You are not looking at an entity!");
                return;
            }

            Type target = ent?.GetType() ?? null;

            if (target == null)
            {
                var types = typeof(BasePlayer).Assembly.GetTypes();

                foreach (var t in types)
                {
                    if (t.IsClass && t.Name == cls)
                    {
                        target = t;
                        break;
                    }
                }
            }

            if (target == null)
            {
                player.Reply("Class not found!");
                return;
            }

            var flags = BindingFlags.NonPublic | BindingFlags.Public;
            if (command == "evallook")
            {
                flags |= BindingFlags.Instance;
            }
            else
            {
                flags |= BindingFlags.Static;
            }

            if (type == "method")
            {
                var method = target.GetMethod(name, flags);

                if (method == null)
                {
                    player.Reply("Method " + name + " not found in class " + target.Name);
                    return;
                }
                
                var rt = method.ReturnType;
                var methodArgs = method.GetParameters().Select(p => p.ParameterType).ToList();

                Puts("A exst: " + (args2 != null));

                if ((args2?.Length == 1 && args2[0] == "." && methodArgs.Count == 0) || (args2?.Length ?? 0) == methodArgs.Count)
                {
                    //if ()
                    //{
                        if (args2.Length == 1 && args2[0] == ".") {
                            args2 = new string[] {};
                        }

                        var args3 = args2.Select(a => Convert.ChangeType(a, methodArgs[0])).ToArray();
                        var result = method.Invoke(ent, args3);
                        
                        player.Reply("Result: " + result + GetVariableTypeName(result));
                        return;
                    //}
                }

                var strParam = methodArgs.Count < 1 ? "None" : string.Join(", ", methodArgs.Select(p => p.Name));

                player.Reply("Return type: " + rt.Name + " parameters: " + strParam);
            }
            else if (type == "field")
            {
                var field = target.GetField(name, flags);

                if (field == null)
                {
                    player.Reply("Field " + name + " not found in class " + target.Name);
                    return;
                }

                if (newval != null) {
                    object t = null;
                    try
                    {
                        t = ParseArg(newval);
                    }
                    catch (Exception e)
                    {
                        player.Reply("Arg exception: " + e.Message);
                        return;
                    }
                    
                    // TODO instance here
                    try
                    {
                        field.SetValue(ent, t);
                        player.Reply("Set field " + field.Name + " to " + t + GetVariableTypeName(t));
                    }
                    catch (Exception e)
                    {
                        player.Reply("Exception: " + e.Message);
                    }
                    return;
                }

                var result = field.GetValue(ent);
                if (result != null)
                    player.Reply("Current value: " + result.ToString() + GetVariableTypeName(result));
            }
            else if (type == "property")
            {
                var property = target.GetProperty(name, flags);

                if (property == null)
                {
                    player.Reply("Property " + name + " not found in class " + target.Name);
                    return;
                }

                if (newval != null)
                {
                    if (!property.CanWrite)
                    {
                        player.Reply("Property " + name + " is read-only");
                        return;
                    }

                    object t = null;
                    try
                    {
                        t = ParseArg(newval);
                    }
                    catch (Exception e)
                    {
                        player.Reply("Arg exception: " + e.Message);
                        return;
                    }

                    // TODO instance here
                    try
                    {
                        property.SetValue(ent, t);
                        player.Reply("Set field " + property.Name + " to " + t + GetVariableTypeName(t));
                    }
                    catch (Exception e)
                    {
                        player.Reply("Exception: " + e.Message);
                    }
                    return;
                }

                if (!property.CanRead)
                {
                    player.Reply("Property " + name + " is write-only");
                    return;
                }

                Puts("pls " + (property == null));

                var result = property.GetValue(null);
                Puts("pls2");
                if (result != null)
                    player.Reply("Current value: " + result.ToString() + GetVariableTypeName(result));
            }
            else
            {
                player.Reply("Invalid type!\nUsage: /" + command + " " + usage);
            }
        }

        private object ParseArg(string arg)
        {
            var lower = arg.ToLower();
            if (lower == "true") return true;
            if (lower == "false") return false;
            if (lower == "null") return null;

            if (arg.StartsWith("\"") && arg.EndsWith("\""))
                return arg.Substring(1, arg.Length - 2);

            if (arg.StartsWith("'") && arg.EndsWith("'"))
            {
                var s = arg.Substring(1, arg.Length - 2);
                if (s.Length == 1)
                    return s[0];
                return s;
            }

            if (arg.EndsWith("d")) {
                double temp;
                if (double.TryParse(arg.Substring(0, arg.Length - 1), out temp))
                {
                    return temp;
                }
                else
                {
                    throw new FormatException("Invalid format for double!");   
                }
            }

            if (arg.EndsWith("f")) {
                float temp;
                if (float.TryParse(arg.Substring(0, arg.Length - 1), out temp))
                {
                    return temp;
                }
                else
                {
                    throw new FormatException("Invalid format for float!");
                }
            }

            if (lower.EndsWith("l")) {
                if (!lower.Contains("u")) {
                    long temp;
                    if (long.TryParse(arg.Substring(0, arg.Length - 2), out temp))
                    {
                        return temp;
                    }
                    else
                    {
                        throw new FormatException("Invalid format for long!");
                    }
                }
                else
                {
                    ulong temp;
                    if (ulong.TryParse(arg.Substring(0, arg.Length - 1), out temp))
                    {
                        return temp;
                    }
                    else
                    {
                        throw new FormatException("Invalid format for ulong!");
                    }
                }
            }

            if (lower.EndsWith("ui")) {
                uint temp;
                if (uint.TryParse(arg.Substring(0, arg.Length - 2), out temp))
                {
                    return temp;
                }
                else
                {
                    throw new FormatException("Invalid format for uint!");
                }
            }

            if (!Regex.IsMatch(arg, @"^\d+$"))
            {
                throw new Exception("Could not detect type (use ' for strings, d/f/L/UL/UI for numbers)");
            }

            int itemp;
            if (int.TryParse(arg, out itemp))
            {
                return itemp;
            }
            else
            {
                throw new FormatException("Invalid format for int!");
            }
        }

        private string GetVariableTypeName(object obj)
        {
            return GetTypeNameNice(obj.GetType());
        }

        private string GetTypeNameNice(Type type)
        {
            return " (" + (type.Name == "Single" ? "Float" : type.Name) + ")";
        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250)
        {
            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new Ray(player.eyes.position, currentRot);
            RaycastHit hitt;

            if (Physics.Raycast(ray, out hitt, 10f, -1)) {
                return hitt.GetEntity();
            }

            return null;
        }
    }
}