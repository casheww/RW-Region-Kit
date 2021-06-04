using ManagedPlacedObjects;

namespace RegionKit.Circuits
{
    /// <summary>
    /// Interface for the physical, realised part of a component.<br/>
    /// If you have no reason to inherit from another class, inherit from <see cref="RealBaseComponent"/>
    /// instead as it handles a lot of boring stuff for you. <br/>
    /// Otherwise, any other <see cref="UpdatableAndDeletable"/> is okay if you implement this interface.
    /// <br/><br/>
    /// If you are still here, it's highly recommended that you use <see cref="RealBaseComponent"/> as a guide.
    /// Heed its class summary.
    /// </summary>
    interface ICircuitComponent
    {
        /// <summary>
        /// The <see cref="PlacedObject"/> corresponding to our object. 
        /// Can be used to track position with <see cref="PlacedObject.pos"/>.
        /// </summary>
        PlacedObject PObj { get; }

        /// <summary>
        /// Managed data from henpemaz's glorious framework - <seealso cref="ManagedPlacedObjects"/>.
        /// </summary>
        PlacedObjectsManager.ManagedData Data { get; }

        /// <summary>
        /// The <see cref="AbstractBaseComponent"/> that handles the component logic.
        /// </summary>
        AbstractBaseComponent AbstractComp { get; set; }

        /// <summary>
        /// * Take a look at <see cref="RealBaseComponent.Update(bool)"/> and <see cref="RealBaseComponent.Activated"/>
        /// for the recommended implementation.
        /// </summary>
        bool Activated { get; set; }

        string CurrentCircuitID { get; }

    }
}
