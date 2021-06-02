using ManagedPlacedObjects;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RegionKit.Circuits
{
    partial class CircuitController : World.WorldProcess
    {
        public CircuitController(World world) : base(world)
        {
            Instance = this;

            circuits = Saver.LoadComponentConfig(world.game);
            LoadComponentStates(circuits);
        }

        public static CircuitController Instance { get; private set; }
        readonly Dictionary<string, Circuit> circuits;

        public override void Update()
        {
            base.Update();

            List<string> idsForPurge = new List<string>();

            foreach (string id in circuits.Keys)
            {
                Circuit c = circuits[id];
                if (c.IsEmpty)
                {
                    idsForPurge.Add(id);
                }
                else
                {
                    c.Update();
                }
            }

            // remove empty circuits
            foreach (string id in idsForPurge)
            {
                circuits.Remove(id);
            }

            UpdateComponentAllegiance(out bool componentsHaveChangedCircuit);

            // only save every {saveInterval} updates and if components have been moved around
            if (componentsHaveChangedCircuit && saveCounter == saveInterval)
            {
                Saver.SaveComponents(circuits);
                saveCounter = 0;
            }
            saveCounter++;
        }
        int saveCounter = 0;
        const int saveInterval = 40;


        #region ComponentManagement

        public void AddComponent(string circuitID, ICircuitComponent component)
        {
            if (!circuits.ContainsKey(circuitID))
            {
                circuits[circuitID] = new Circuit(circuitID);
            }

            switch (component.Type)
            {
                case CompType.Input:
                    circuits[circuitID].inputComponents.Add(component);
                    break;
                case CompType.Output:
                    circuits[circuitID].outputComponents.Add(component);
                    break;
            }

            component.LastCircuitID = circuitID;
            Log($"added component {component} to {circuitID}");
        }

        public bool RemoveComponent(string circuitID, ICircuitComponent component)
        {
            if (!circuits.ContainsKey(circuitID)) return false;

            bool success = false;
            switch (component.Type)
            {
                case CompType.Input:
                    success = circuits[circuitID].inputComponents.Remove(component);
                    break;
                case CompType.Output:
                    success = circuits[circuitID].outputComponents.Remove(component);
                    break;
            }

            if (success) Log($"removed component {component} from {circuitID}");
            return success;
        }

        /// <summary>
        /// Checks whether a component's current circuit ID has changed.
        /// If it has changed, then the component is migrated to the new circuit.
        /// </summary>
        void UpdateComponentAllegiance(out bool changesMade)
        {
            List<ICircuitComponent> componentsToMigrate = new List<ICircuitComponent>();

            foreach (Circuit c in circuits.Values)
            {
                foreach (ICircuitComponent comp in c.AllComponents)
                {
                    if (comp.LastCircuitID != comp.CurrentCircuitID)
                    {
                        componentsToMigrate.Add(comp);
                    }
                }
            }

            changesMade = componentsToMigrate.Count >= 1;
            foreach (ICircuitComponent comp in componentsToMigrate)
            {
                Instance.MigrateComponent(comp);
                comp.LastCircuitID = comp.CurrentCircuitID;
            }
        }

        public void MigrateComponent(ICircuitComponent component)
        {
            if (component.LastCircuitID != null)
            {
                RemoveComponent(component.LastCircuitID, component);
            }

            if (component.Type == CompType.Output)
            {
                component.Activated = false;
            }
            else if (component is FlipFlop ff)
            {
                ff.Clear();
            }

            AddComponent(component.CurrentCircuitID, component);
        }

        #endregion ComponentManagement


        /// <summary>
        /// Tries to get the Circuit object with the given circuit ID
        /// </summary>
        /// <returns>Whether the get was successful</returns>
        bool TryGetCircuit(string id, out Circuit circuit)
        {
            if (circuits.TryGetValue(id, out Circuit c))
            {
                circuit = c;
                return true;
            }
            circuit = null;
            return false;
        }

        static void Log(object message, bool error=false, MethodBase method=null)
        {
            string methodStr = method == null ? "" : $"[{method.Name}]";
            message = $"[Circuits]{methodStr} {message}";

            if (!error) Debug.Log(message);
            else Debug.LogError(message);
        }

    }
}
