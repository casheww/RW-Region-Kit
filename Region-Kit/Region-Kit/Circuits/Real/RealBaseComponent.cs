using ManagedPlacedObjects;
using RegionKit.Circuits.Abstract;
using System;
using System.Text.RegularExpressions;

namespace RegionKit.Circuits.Real
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

            bool foundMatch = CircuitController.Instance.TryPassRealCompToAbstractComp(this);

            // if a matching abstract component isn't found, we need to generate a new one
            if (!foundMatch)
            {
                // first, we get a copy of the managed object setup stuff 
                MObjSetup? managed = Setup.GetManagedObjSetupCopy(pObj.type.ToString());
                if (managed == null)
                {
                    Setup.Log($"{GetType()} couldn't find matching managed object entry", true);
                    return;
                }

                // then we use that setup struct and some other data to make an array of args for the ctor
                object[] args =
                {
                    pObj.type.ToString(),
                    room.world.region.name,
                    (MObjSetup)managed
                };

                // then we instantiate the abstract component ...
                AbstractBaseComponent comp = (AbstractBaseComponent)Activator.CreateInstance(((MObjSetup)managed).AbstractType, args);
                CircuitController.Instance.AddComponent(CurrentCircuitID, comp);

                // ... and pass `this` to it so that it can see any changing settings on the realised object
                comp.Realised = true;
                comp.RealisedObj = this;
            }
        }

        public PlacedObject PObj => _pObj;
        private readonly PlacedObject _pObj;

        public PlacedObjectsManager.ManagedData Data => _data;
        private readonly PlacedObjectsManager.ManagedData _data;

        public virtual bool Activated
        {
            get => Data.GetValue<bool>(MKeys.activated);
            set => Data.SetValue(MKeys.activated, value);
        }

        public string CurrentCircuitID => Data.GetValue<string>(MKeys.circuitID);

    }
}
