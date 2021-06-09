using ManagedPlacedObjects;
using RegionKit.Circuits.Abstract;
using RegionKit.Circuits.Real;
using System.Collections.Generic;
using System.Linq;
namespace RegionKit.Circuits
{
    partial class CircuitController : World.WorldProcess
    {
        public CircuitController(World world) : base(world)
        {
            Instance = this;

            circuits = new Dictionary<string, Circuit>();

            Saver.LoadComponentConfig(this, world.game);
            Saver.LoadComponentStates(this, world.game);
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

            UpdateComponentAllegiance(out bool migrationOccured);
            if (migrationOccured)
                unregisteredMigrationsExist = true;

            // only save every {saveInterval} updates and if components have been moved around
            if (saveCounter == saveInterval)
            {
                saveCounter = 0;
                if (unregisteredMigrationsExist || componentCountLastUpdate != GetTotalComponentCount())
                {
                    Saver.SaveComponentConfig(this, world.game);
                    unregisteredMigrationsExist = false;
                }
            }
            saveCounter++;

            componentCountLastUpdate = GetTotalComponentCount();
        }
        int saveCounter = 0;
        const int saveInterval = 40;
        int componentCountLastUpdate = 0;
        bool unregisteredMigrationsExist = false;

        int GetTotalComponentCount()
        {
            int sum = 0;
            foreach (Circuit c in circuits.Values)
            {
                sum += c.AllComponents.Count;
            }
            return sum;
        }

        #region ComponentManagement

        public void AddComponent(string circuitID, AbstractBaseComponent component)
        {
            if (!circuits.ContainsKey(circuitID))
            {
                circuits[circuitID] = new Circuit(circuitID);
            }

            switch (component.CompType)
            {
                case CompType.Input:
                    circuits[circuitID].inputComponents.Add(component);
                    break;
                case CompType.Output:
                    circuits[circuitID].outputComponents.Add(component);
                    break;
            }

            component.LastCircuitID = circuitID;
            Setup.Log($"added component {component} to {circuitID}");
        }

        public bool RemoveComponent(string circuitID, AbstractBaseComponent component)
        {
            if (!circuits.ContainsKey(circuitID)) return false;

            bool success = false;
            switch (component.CompType)
            {
                case CompType.Input:
                    success = circuits[circuitID].inputComponents.Remove(component);
                    break;
                case CompType.Output:
                    success = circuits[circuitID].outputComponents.Remove(component);
                    break;
            }

            if (success) Setup.Log($"removed component {component} from {circuitID}");
            return success;
        }

        /// <summary>
        /// Checks whether a component's current circuit ID has changed.
        /// If it has changed, then the component is migrated to the new circuit.
        /// </summary>
        void UpdateComponentAllegiance(out bool changesMade)
        {
            List<AbstractBaseComponent> componentsToMigrate = new List<AbstractBaseComponent>();

            foreach (Circuit c in circuits.Values)
            {
                foreach (AbstractBaseComponent comp in c.AllComponents)
                {
                    if (comp.LastCircuitID != comp.CurrentCircuitID)
                    {
                        componentsToMigrate.Add(comp);
                    }
                }
            }

            changesMade = componentsToMigrate.Count > 0;

            foreach (AbstractBaseComponent comp in componentsToMigrate)
            {
                Instance.MigrateComponent(comp);
                comp.LastCircuitID = comp.CurrentCircuitID;
            }
        }

        public void MigrateComponent(AbstractBaseComponent component)
        {
            if (component.LastCircuitID != null)
            {
                RemoveComponent(component.LastCircuitID, component);
            }

            if (component.CompType == CompType.Output)
            {
                component.Activated = false;
            }
            else if (component is AbstractFlipFlop ff)
            {
                ff.Clear();
            }

            AddComponent(component.CurrentCircuitID, component);
        }

        /// <summary>
        /// Finds an existing <see cref="AbstractBaseComponent"/> with the same settings.
        /// </summary>
        /// <returns>Whether the request was successful.</returns>
        public bool TryPassRealCompToAbstractComp(RealBaseComponent real)
        {
            if (!circuits.TryGetValue(real.CurrentCircuitID, out Circuit c)) return false;

            foreach (AbstractBaseComponent abstractComp in c.AllComponents)
            {
                if (CheckAbstractToRealDataMatch(abstractComp, real))
                {
                    abstractComp.Realised = true;
                    abstractComp.RealisedObj = real;
                    return true;
                }
            }

            return false;
        }

        public bool CheckAbstractToRealDataMatch(AbstractBaseComponent _abstract, RealBaseComponent real)
        {
            if (_abstract.PObjTypeStr != real.PObj.type.ToString() && _abstract.Region != real.room.world.region.name)
            {
                return false;
            }

            // compare field values
            foreach (string key in _abstract.Data.FieldsByKey.Keys)
            {
                if (!_abstract.Data.TryGetFieldAndValue(key, out PlacedObjectsManager.ManagedField _, out object absV))
                    return false;

                object realV = real.Data.GetValue<object>(key);

                if (absV != realV) return false;
            }

            // all checks passed
            return true;
        }

        #endregion ComponentManagement


        /// <summary>
        /// Tries to get the Circuit object with the given circuit ID
        /// </summary>
        /// <returns>Whether the get was successful</returns>
        public bool TryGetCircuit(string id, out Circuit circuit)
        {
            if (circuits.TryGetValue(id, out Circuit c))
            {
                circuit = c;
                return true;
            }
            circuit = null;
            return false;
        }

    }
}
