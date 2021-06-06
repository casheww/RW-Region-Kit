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
            const string savesDir = "Mods/RegionKit/Circuits";
            // saves are in Mods/RegionKit/Circuits/<region-code>.txt

            public static void LoadComponentConfig(CircuitController cController, RainWorldGame game)
            {
                if (!Directory.Exists(savesDir))
                {
                    Directory.CreateDirectory(savesDir);
                    return;     // nothing to load
                }

                if (game.IsArenaSession) return;        // skipping arena compat for now TODO

                foreach (string fp in Directory.GetFiles(savesDir))
                {
                    string[] raw = File.ReadAllLines(fp);
                    Dictionary<string, Circuit> regionCircuits = DeserialiseComponentConfig(game, raw);

                    foreach (Circuit c in regionCircuits.Values)
                    {
                        foreach (AbstractBaseComponent comp in c.AllComponents)
                        {
                            cController.AddComponent(c.id, comp);
                        }
                    }
                }

            }


            /* Custom circuit save format involves fields separated by `~`.
             *   `~` in string fields is replaced by `\~`.
             * Basic structure:
             *   0: type name (Button)
             *   1: placed object type name (Circuit_Button)
             *   2: region code
             *   3 onwards: determined by the setup registered with henpemaz's framework
             */

            static Dictionary<string, Circuit> DeserialiseComponentConfig(RainWorldGame game, string[] raw)
            {
                Dictionary<string, Circuit> dict = new Dictionary<string, Circuit>();

                foreach (string l in raw)
                {
                    if (l == "") continue;

                    string[] lSplits = Regex.Split(l, @"(?<!\\)~");       // split at ~ but not \~

                    // check that we have at least the class name and the pObj entry
                    if (lSplits.Length < 2)
                    {
                        Setup.Log($"too few fields found in {l}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    // get the type - will be used to instantiate
                    Type type = Assembly.GetCallingAssembly().GetType(lSplits[0]);
                    if (type == null)
                    {
                        Setup.Log($"error parsing component type {lSplits[0]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    // to get the managed field types and whatnot
                    MObjSetup? nullableManagedSetup = Setup.GetManagedObjSetupCopy(lSplits[1]);
                    if (nullableManagedSetup == null)
                    {
                        Setup.Log($"error parsing component managed object type {lSplits[1]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    // check that region code corresponds to a region loaded by the game/CRS
                    if (!game.overWorld.regions.Any(r => r.name == lSplits[2]))
                    {
                        Setup.Log($"region {lSplits[2]} for component {lSplits[1]} is not loaded", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    int fieldOffset = 3;
                    MObjSetup managedSetup = (MObjSetup)nullableManagedSetup;

                    // check that the number of fields in save matches the number of managed fields
                    if (lSplits.Length - fieldOffset != managedSetup.Fields.Length)
                    {
                        Setup.Log($"incorrect number of fields in {l} for {lSplits[0]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    // parse the saved field values
                    if (!TryParseFields(managedSetup, lSplits, fieldOffset))
                    {
                        Setup.Log($"error parsing values in {l} for {lSplits[0]}", true, MethodBase.GetCurrentMethod());
                        continue;
                    }

                    // instantiate
                    object[] args = new object[]
                    {
                        lSplits[1], lSplits[2], managedSetup
                    };
                    AbstractBaseComponent newComponent = (AbstractBaseComponent)Activator.CreateInstance(type, args);

                    // add the component to its circuit
                    if (!dict.ContainsKey(newComponent.CurrentCircuitID))
                    {
                        dict[newComponent.CurrentCircuitID] = new Circuit(newComponent.CurrentCircuitID);
                    }

                    if (newComponent.CompType == CompType.Input)
                    {
                        dict[newComponent.CurrentCircuitID].inputComponents.Add(newComponent);
                    }
                    else
                    {
                        dict[newComponent.CurrentCircuitID].outputComponents.Add(newComponent);
                    }
                    
                }

                return dict;
            }

            static bool TryParseFields(MObjSetup managedSetup, string[] lSplits, int fieldOffset)
            {
                PlacedObjectsManager.ManagedField[] fields = managedSetup.Fields;

                bool parseSuccess = true;

                for (int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i - fieldOffset];

                    try
                    {
                        object value = f.FromString(lSplits[i]);
                        if (value is string str)
                        {
                            value = Regex.Replace(str, @"\\~", @"~");
                        }
                        managedSetup.SetValue(f.key, value);
                    }
                    catch
                    {
                        parseSuccess = false;
                    }

                    if (!parseSuccess) return false;
                }

                return true;
            }

            public static void SaveComponentConfig(CircuitController cController, RainWorldGame game)
            {
                if (game.IsArenaSession) return; // TODO ? arena support

                if (!Directory.Exists(savesDir)) Directory.CreateDirectory(savesDir);

                Dictionary<string, List<AbstractBaseComponent>> compByRegionName = new Dictionary<string, List<AbstractBaseComponent>>();
                foreach (Circuit c in cController.circuits.Values)
                {
                    foreach (AbstractBaseComponent comp in c.AllComponents)
                    {
                        if (!compByRegionName.ContainsKey(comp.Region))
                        {
                            compByRegionName[comp.Region] = new List<AbstractBaseComponent>();
                        }

                        compByRegionName[comp.Region].Add(comp);
                    }
                }

                foreach (var pair in compByRegionName)
                {
                    string[] saveData = new string[pair.Value.Count];
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        saveData[i] = SerialiseComponentConfig(pair.Value[i]);
                    }

                    File.WriteAllLines(Path.Combine(savesDir, pair.Key + ".txt"), saveData);
                }
            }

            public static string SerialiseComponentConfig(AbstractBaseComponent comp)
            {
                List<string> data = new List<string>();

                data.Add(comp.GetType().ToString());
                data.Add(comp.PObjTypeStr);
                data.Add(comp.Region);

                MObjSetup? nullableMObjSetup = Setup.GetManagedObjSetupCopy(comp.PObjTypeStr);
                if (nullableMObjSetup == null)
                {
                    Setup.Log($"error finding managed object setup for {comp.PObjTypeStr}", true, MethodBase.GetCurrentMethod());
                    return "";
                }
                MObjSetup mObjSetup = (MObjSetup)nullableMObjSetup;

                foreach (string key in mObjSetup.FieldsByKey.Keys)
                {
                    mObjSetup.TryGetFieldAndValue(key, out PlacedObjectsManager.ManagedField f, out object v);
                    data.Add(f.ToString(v));
                }

                data.ForEach(s => Regex.Replace(s, @"~", @"\~"));
                return string.Join("~", data.ToArray());

            }


            public static void LoadComponentStates(CircuitController cController, RainWorldGame game)
            {
                if (game.IsArenaSession) return;    // TODO?

                // placeholder
                foreach (Circuit c in cController.circuits.Values)
                {
                    c.AllComponents.ForEach(comp => comp.Activated = false);
                }

            }

        }
    }
}
