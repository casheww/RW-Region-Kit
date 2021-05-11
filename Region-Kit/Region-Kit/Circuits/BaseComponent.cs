using ManagedPlacedObjects;
using UnityEngine;

namespace RegionKit.Circuits
{
    abstract class BaseComponent : UpdatableAndDeletable, ICircuitComponent
    {
        public BaseComponent(PlacedObject pObj, Room room, CompType type, InputType inType = InputType.NotAnInput)
        {
            this.pObj = pObj;
            this.room = room;
            Type = type;

            if (type == CompType.Input && inType == InputType.NotAnInput)
                throw new System.ArgumentException("Input component can't have input type NotAnInput");
            _inType = inType;

            data = pObj.data as PlacedObjectsManager.ManagedData;

            Debug.Log($"created circuits component ({GetType()})");
        }

        public readonly PlacedObject pObj;
        public readonly PlacedObjectsManager.ManagedData data;
        private readonly InputType _inType;

        public CompType Type { get; set; }
        public InputType InType => Type == CompType.Input ? _inType : InputType.NotAnInput;

        public virtual bool Activated
        {
            get => data.GetValue<bool>(MKeys.activated);
            set => data.SetValue(MKeys.activated, value);
        }


        public override void Update(bool eu)
        {
            base.Update(eu);

            // update circuit ID based on dev input
            string currentCircuitID = this is LogicGate ? data.GetValue<string>(MKeys.output) : data.GetValue<string>(MKeys.circuitID);
            if (lastCircuitID != currentCircuitID)
            {
                CircuitController.Instance.MigrateComponent(lastCircuitID, currentCircuitID, this);
                lastCircuitID = currentCircuitID;
            }
        }
        string lastCircuitID = null;

    }
}
