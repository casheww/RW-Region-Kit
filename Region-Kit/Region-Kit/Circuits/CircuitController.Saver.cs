using ManagedPlacedObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RegionKit.Circuits
{
    partial class CircuitController
    {
        static class Saver
        {
            const string crsDir = "Mods/CustomResources";
            const string configFile = "circuit_config.txt";

            public static Dictionary<string, Circuit> LoadComponentConfig(RainWorldGame game)
            {
                if (!Directory.Exists(crsDir))
                {
                    Log("./Mods/CustomResources not found! Are you sure you have CRS installed?", true, MethodBase.GetCurrentMethod());
                    return null;
                }

                if (game.IsArenaSession) return null;       // TODO: add arena compat

                foreach (string regionPack in Directory.GetDirectories(crsDir))
                {
                    string p = Path.Combine(Path.Combine(crsDir, regionPack), configFile);
                    if (!File.Exists(p)) continue;

                    string[] raw = File.ReadAllLines(p);
                    Dictionary<string, Circuit> packCircuits = DeserialiseComponentConfig(raw);
                }

                return dict;
            }


            /* Custom circuit save format involves fields separated by `~`.
             *   `~` in string fields is replaced by `\~`.
             * Basic structure:
             *   0: type name (Button)
             *   1: placed object type name (Circuit_Button)
             *   2: position for component identification (as a Vector2)
             *   3 onwards: determined by the setup registered with henpemaz's framework
             */

            static Dictionary<string, Circuit> DeserialiseComponentConfig(string[] raw)
            {
                Dictionary<string, Circuit> dict = new Dictionary<string, Circuit>();

                foreach (string l in raw)
                {
                    string[] lSplits = Regex.Split(l, @"(?<!\\)~");       // split at ~ but not \~

                    if (lSplits.Length < 2)
                    {
                        Log($"too few fields found in {l}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    Type type = Assembly.GetCallingAssembly().GetType("RegionKit.Circuits." + lSplits[0]);
                    if (type == null)
                    {
                        Log($"error parsing component type {lSplits[0]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    Setup.MObjSetup? managedSetup = Setup.GetManagedObjectSetup(lSplits[1]);
                    if (managedSetup == null)
                    {
                        Log($"error parsing component managed object type {lSplits[1]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    if (!TryParsePosition(lSplits[2], out UnityEngine.Vector2 pos))
                    {
                        Log($"error parsing component position {lSplits[2]} for {lSplits[0]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    PlacedObjectsManager.ManagedField[] fields = ((Setup.MObjSetup)managedSetup).fields;
                    if (lSplits.Length - fieldOffset != fields.Length)
                    {
                        Log($"incorrect number of fields in {l} for {lSplits[0]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    if (!TryParseFields(fields, lSplits, out object[] fieldValues))
                    {
                        Log($"error parsing values in {l} for {lSplits[0]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }
                    
                    object newComponent = Activator.CreateInstance(type, fieldValues);  // TODO
                }

                // return dict;
            }
            const int fieldOffset = 2;

            static bool TryParsePosition(string raw, out UnityEngine.Vector2 pos)
            {
                pos = UnityEngine.Vector2.zero;

                string[] posParts = Regex.Split(raw, @"\^");
                if (!float.TryParse(posParts[0], out float x))
                {
                    return false;
                }
                if (!float.TryParse(posParts[1], out float y))
                {
                    return false;
                }

                pos = new UnityEngine.Vector2(x, y);
                return true;
            }

            static bool TryParseFields(PlacedObjectsManager.ManagedField[] fields, string[] lSplits, out object[] fieldValues)
            {
                fieldValues = new object[fields.Length - fieldOffset];
                bool parseSuccess = true;

                for (int i = 0; i < fieldValues.Length; i++)
                {
                    var f = fields[i - fieldOffset];

                    try
                    {
                        object value = f.FromString(lSplits[i]);
                        if (value is string str)
                        {
                            value = Regex.Replace(str, @"\\~", @"~");
                        }
                        fieldValues[i] = value;
                    }
                    catch
                    {
                        parseSuccess = false;
                    }

                    if (!parseSuccess) return false;
                }

                return true;
            }


            public static void SaveComponents(Dictionary<string, Circuit> circuits)
            {
                string saveData = MiniJsonExtensions.toJson(circuits.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value));

                Log($"saving {circuits.Count} things");

                File.WriteAllText(path, saveData);
            }

            public static Dictionary<string, Circuit> LoadComponents()
            {
                Dictionary<string, object> dict = MiniJsonExtensions.dictionaryFromJson(File.ReadAllText(pathFile));
                Log($"loading {dict.Count} things");
                return dict.ToDictionary(kvp => kvp.Key, kvp => (Circuit)kvp.Value);
            }

        }

    }
}
