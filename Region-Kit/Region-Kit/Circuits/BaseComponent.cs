using ManagedPlacedObjects;
using UnityEngine;

namespace RegionKit.Circuits
{
    public abstract class BaseComponent : UpdatableAndDeletable, ICircuitComponent
    {
        public BaseComponent(PlacedObject pObj, Room room, CompType type, InputType inType = InputType.NotAnInput)
        {
            this.pObj = pObj;
            this.room = room;
            Type = type;

            if (type == CompType.Input && inType == InputType.NotAnInput)
                throw new System.ArgumentException("Input component can't have input type NotAnInput");
            _inType = inType;

            _data = pObj.data as PlacedObjectsManager.ManagedData;

            // some components should always start deactivated
            if (type == CompType.Output)
            {
                _data.SetValue(MKeys.activated, false);
            }

            Debug.Log($"created circuits component ({GetType()})");
        }

        public readonly PlacedObject pObj;
        protected readonly PlacedObjectsManager.ManagedData _data;
        public PlacedObjectsManager.ManagedData Data => _data;

        private readonly InputType _inType;
        public CompType Type { get; set; }
        public InputType InType => Type == CompType.Input ? _inType : InputType.NotAnInput;

        public virtual bool Activated
        {
            get => Data.GetValue<bool>(MKeys.activated);
            set => Data.SetValue(MKeys.activated, value);
        }

        public string CurrentCircuitID
        {
            get
            {
                return this is LogicGate || this is FlipFlop ?
                    Data.GetValue<string>(MKeys.output) : Data.GetValue<string>(MKeys.circuitID);
            }
        }
        public string LastCircuitID { get; set; }

    }
}
