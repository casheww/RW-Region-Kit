using ManagedPlacedObjects;
using System;

namespace RegionKit.Circuits
{
    /// <summary>
    /// Base class for the physical, realised part of a component.
    /// If you wish to inherit from your own class when making a component, 
    /// implement <see cref="ICircuitComponent"/> and use this as a rough guide.
    /// <br/><br/>
    /// Circuit logic should be handled in a <see cref="AbstractBaseComponent"/>.<br/>
    /// Useful for sprites, collision, etc.
    /// - obviously to add sprites, inherit from this class and implement <see cref="IDrawable"/>.
    /// <br/><br/>
    /// Instantiation is already handled by the <see cref="ManagedPlacedObjects"/> framework.
    /// </summary>
    public class RealBaseComponent : UpdatableAndDeletable, ICircuitComponent
    {
        public RealBaseComponent(PlacedObject pObj, Room room)
        {
            _pObj = pObj;
            this.room = room;

            _data = pObj.data as PlacedObjectsManager.ManagedData;

            bool foundMatch = CircuitController.Instance.RequestMatchingAbstractComp(this, out var abstractComp);
            AbstractComp = abstractComp;

            if (!foundMatch)
            {
                MObjSetup? managed = Setup.GetManagedObjSetupCopy(pObj.type.ToString());
                if (managed == null)
                {
                    Setup.Log($"{this.GetType()} couldn't find matching managed object entry", true);
                    return;
                }

                object[] args =
                {
                    pObj.type.ToString(),
                    room.world.region,
                    (MObjSetup)managed
                };
                AbstractComp = (AbstractBaseComponent)Activator.CreateInstance(((MObjSetup)managed).AbstractType, args);
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (activatedLastUpdate != AbstractComp.Activated)
            {
                Activated = AbstractComp.Activated;
                activatedLastUpdate = AbstractComp.Activated;
            }
        }
        protected bool activatedLastUpdate = false;

        public PlacedObject PObj => _pObj;
        private readonly PlacedObject _pObj;

        public PlacedObjectsManager.ManagedData Data => _data;
        private readonly PlacedObjectsManager.ManagedData _data;

        public AbstractBaseComponent AbstractComp { get; set; }

        /// <summary>
        /// Updated by <see cref="Update(bool)"/> when <see cref="AbstractComp"/>.Activated has changed.<br/>
        /// If overriding, ensure that the base getter/setter is called 
        /// to update the activity in the managed data.
        /// </summary>
        public virtual bool Activated
        {
            get => Data.GetValue<bool>(MKeys.activated);
            set => Data.SetValue(MKeys.activated, value);
        }

        public string CurrentCircuitID => AbstractComp is AbstractLogicGate || AbstractComp is AbstractFlipFlop ?
            Data.GetValue<string>(MKeys.output) : Data.GetValue<string>(MKeys.circuitID);

    }
}
