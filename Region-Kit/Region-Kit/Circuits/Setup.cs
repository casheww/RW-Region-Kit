using ManagedPlacedObjects;
using System.Collections.Generic;

namespace RegionKit.Circuits
{
    static class Setup
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
            mObjSetups.Add(new MObjSetup("Circuit_Button", typeof(Button), fields.ToArray()));
            mObjSetups.Add(new MObjSetup("Circuit_Switch", typeof(Switch), fields.ToArray()));

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
            mObjSetups.Add(new MObjSetup("Circuit_Light", typeof(BasicLight), fields.ToArray()));

            // logic gates with 2 inputs
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "defaultA", "Input A"),
                new PlacedObjectsManager.StringField(MKeys.inputB, "defaultB", "Input B"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(LogicGate_TwoInputs.Op), LogicGate_TwoInputs.Op.AND,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.output, "defaultOUT", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            mObjSetups.Add(new MObjSetup("Circuit_LogicGate_2", typeof(LogicGate_TwoInputs), fields.ToArray()));

            // logic gates with a single input
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "default", "Input"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(LogicGate_OneInput.Op), LogicGate_OneInput.Op.NOT,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.output, "defaultOUT", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            mObjSetups.Add(new MObjSetup("Circuit_LogicGate_1", typeof(LogicGate_OneInput), fields.ToArray()));


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
            mObjSetups.Add(new MObjSetup("Circuit_Clock", typeof(Clock), fields.ToArray()));


            // flip flops
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "defaultD", "D"),
                new PlacedObjectsManager.StringField(MKeys.inputClock, "clock", "Clock"),
                new PlacedObjectsManager.StringField(MKeys.output, "defaultQ", "Q"),
            };
            mObjSetups.Add(new MObjSetup("Circuit_D_FlipFlop", typeof(FlipFlop), fields.ToArray()));

            fields[0] = new PlacedObjectsManager.StringField(MKeys.inputA, "defaultT", "T");
            mObjSetups.Add(new MObjSetup("Circuit_T_FlipFlop", typeof(FlipFlop), fields.ToArray()));

            fields[0] = new PlacedObjectsManager.StringField(MKeys.inputA, "defaultJ", "J");
            fields.Insert(1, new PlacedObjectsManager.StringField(MKeys.inputB, "defaultK", "K"));
            mObjSetups.Add(new MObjSetup("Circuit_JK_FlipFlop", typeof(FlipFlop), fields.ToArray()));

            foreach (MObjSetup s in mObjSetups)
            {
                PlacedObjectsManager.RegisterFullyManagedObjectType(s.Fields, s.Type, s.Name);
            }

            objectsSetUp = true;
        }
        private static bool objectsSetUp = false;

        public readonly static List<MObjSetup> mObjSetups = new List<MObjSetup>();
        public static MObjSetup? GetManagedObjectSetup(string name)
        {
            foreach (MObjSetup s in mObjSetups)
            {
                if (s.Name == name) return s;
            }
            return null;
        }


        /// <summary>
        /// A Managed bool field for henpemaz's framework that has no representation.
        /// Designed to store component activity for <see cref="IAbstractCircuitComponent.Activated"/>.
        /// </summary>
        private class ComponentActivityField : PlacedObjectsManager.BooleanField
        {
            public ComponentActivityField(string key, bool defaultValue) : base(key, defaultValue) { }

            public override bool NeedsControlPanel => false;    // no representation
        }


    }
}
