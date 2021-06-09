using ManagedPlacedObjects;
using RegionKit.Circuits.Abstract;
using RegionKit.Circuits.Real;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/* Circuits 
 * ... is a framework for custom PlacedObjects that can be used as components in logic circuits.
 * Author: casheww
 */

namespace RegionKit.Circuits
{
    public static class Setup
    {
        public static void Apply()
        {
            On.World.ctor += World_ctor;

            if (!objectsSetUp) SetupObjects();
        }

        public static void UnApply()
        {
            On.World.ctor -= World_ctor;
        }

        private static void World_ctor(On.World.orig_ctor orig, World self, RainWorldGame game,
                Region region, string name, bool singleRoomWorld)
        {
            orig(self, game, region, name, singleRoomWorld);

            self.AddWorldProcess(new CircuitController(self));
        }

        /// <summary>
        /// Handles registration for fully managed managed objects in henpemaz's framework - <see cref="PlacedObjectsManager"/>
        /// </summary>
        private static void SetupObjects()
        {
            List<PlacedObjectsManager.ManagedField> fields;

            // basic components
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.circuitID, "default", "Circuit ID"),
                new ComponentActivityField(MKeys.activated, false)
            };
            RegisterComponent("Button", typeof(AbstractButton), typeof(Button), fields.ToArray());
            RegisterComponent("Switch", typeof(AbstractSwitch), typeof(Switch), fields.ToArray());

            // components with colours and other funky stuff
            fields.AddRange(new PlacedObjectsManager.ManagedField[]
            {
                new PlacedObjectsManager.FloatField(MKeys.sine, 0, 1, 0.4f, 0.05f, displayName: "Sine noise"),
                new PlacedObjectsManager.FloatField(MKeys.flicker, 0, 1, 0.2f, 0.05f, displayName: "Flicker"),
                new PlacedObjectsManager.FloatField(MKeys.strength, 0, 1, 0.7f, 0.05f, displayName: "Strength"),
                new PlacedObjectsManager.IntegerField(MKeys.red, 0, 255, 80,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Red"),
                new PlacedObjectsManager.IntegerField(MKeys.green, 0, 255, 200,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Green"),
                new PlacedObjectsManager.IntegerField(MKeys.blue, 0, 255, 200,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Blue")
            });
            RegisterComponent("Light", typeof(AbstractGenericOutput), typeof(BasicLight), fields.ToArray());

            // logic gates with 2 inputs
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "defaultA", "Input A"),
                new PlacedObjectsManager.StringField(MKeys.inputB, "defaultB", "Input B"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(AbstractLogicGate_2In.Op), AbstractLogicGate_2In.Op.AND,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.circuitID, "defaultOUT", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            RegisterComponent("LogicGate_2", typeof(AbstractLogicGate_2In), typeof(RealBaseComponent), fields.ToArray());

            // logic gates with a single input
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "default", "Input"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(AbstractLogicGate_1In.Op), AbstractLogicGate_1In.Op.NOT,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.circuitID, "defaultOUT", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            RegisterComponent("LogicGate_1", typeof(AbstractLogicGate_1In), typeof(RealBaseComponent), fields.ToArray());

            // clock pulse generator
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.circuitID, "default", "Circuit ID"),
                new ComponentActivityField(MKeys.activated, false),
                new PlacedObjectsManager.IntegerField(MKeys.clockOnMax, 10, 600, 60,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, "On-frames"),
                new PlacedObjectsManager.IntegerField(MKeys.clockOffMax, 10, 600, 60,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, "Off-frames")
            };
            RegisterComponent("Clock", typeof(AbstractClock), typeof(RealBaseComponent), fields.ToArray());

            // flip flops
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "defaultD", "D"),
                new PlacedObjectsManager.StringField(MKeys.inputClock, "clock", "Clock"),
                new PlacedObjectsManager.StringField(MKeys.circuitID, "defaultQ", "Q"),
            };
            RegisterComponent("D_FlipFlop", typeof(AbstractFlipFlop), typeof(RealBaseComponent), fields.ToArray());

            fields[0] = new PlacedObjectsManager.StringField(MKeys.inputA, "defaultT", "T");
            RegisterComponent("T_FlipFlop", typeof(AbstractFlipFlop), typeof(RealBaseComponent), fields.ToArray());

            fields[0] = new PlacedObjectsManager.StringField(MKeys.inputA, "defaultJ", "J");
            fields.Insert(1, new PlacedObjectsManager.StringField(MKeys.inputB, "defaultK", "K"));
            RegisterComponent("JK_FlipFlop", typeof(AbstractFlipFlop), typeof(RealBaseComponent), fields.ToArray());

            foreach (MObjSetup s in mObjSetups)
            {
                PlacedObjectsManager.RegisterFullyManagedObjectType(s.Fields, s.RealisedType, s.Name);
            }

            objectsSetUp = true;
        }
        private static bool objectsSetUp = false;

        /// <summary>
        /// Entrypoint for registering new components with the Circuits... 
        /// well I guess it's basically the Circuits *Framework* now.
        /// </summary>
        /// <param name="name">The name of the new component. This will eventually be prepended by 'Circuit_'.</param>
        /// <param name="abstractType">The type of the abstract object that inherits from <see cref="AbstractBaseComponent"/>.</param>
        /// <param name="realisedType">The type of the realised object that implements or inherits <see cref="ICircuitComponent"/>.</param>
        /// <param name="fields">An array of <see cref="PlacedObjectsManager.ManagedField"/> used in Henpemaz's managed object framework.</param>
        public static void RegisterComponent(string name, Type abstractType, Type realisedType,
                PlacedObjectsManager.ManagedField[] fields)
        {
            if (!abstractType.IsSubclassOf(typeof(AbstractBaseComponent)))
            {
                Log($"component {name} couldn't be registered because {abstractType.Name} doesn't inherit from " +
                    $"Circuits.AbstractBaseComponent", true);
                return;
            }
            if (realisedType.GetInterface("ICircuitComponent") == null)
            {
                Log($"component {name} couldn't be registered because {realisedType.Name} doesn't implement " +
                    $"Circuits.ICircuitComponent", true);
                return;
            }

            string qualifiedName = "Circuit_" + name;
            MObjSetup setup = new MObjSetup(qualifiedName, abstractType, realisedType, fields);
            mObjSetups.Add(setup);

            PlacedObjectsManager.RegisterFullyManagedObjectType(setup.Fields, setup.RealisedType, setup.Name);
        }

        public readonly static List<MObjSetup> mObjSetups = new List<MObjSetup>();
        public static MObjSetup? GetManagedObjSetupCopy(string name)
        {
            foreach (MObjSetup s in mObjSetups)
            {
                if (s.Name == name)
                {
                    // return a copy of our managed object setup
                    return new MObjSetup(s.Name, s.AbstractType, s.RealisedType, s.Fields);
                }
            }

            return null;
        }


        /// <summary>
        /// A Managed bool field for henpemaz's framework that has no representation.
        /// Designed to store component activity for <see cref="IAbstractCircuitComponent.Activated"/>.
        /// </summary>
        public class ComponentActivityField : PlacedObjectsManager.BooleanField
        {
            public ComponentActivityField(string key, bool defaultValue) : base(key, defaultValue) { }

            public override bool NeedsControlPanel => false;    // no representation
        }


        public static void Log(object message, bool error = false, MethodBase method = null)
        {
            string methodStr = method == null ? "" : $"[{method.Name}]";
            message = $"[Circuits]{methodStr} {message}";

            if (!error) Debug.Log(message);
            else Debug.LogError(message);
        }

    }
}
