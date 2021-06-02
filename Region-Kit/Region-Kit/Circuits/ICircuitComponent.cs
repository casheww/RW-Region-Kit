
namespace RegionKit.Circuits
{
    /// <summary>
    /// An interface that all Circuits components should implement.<br/>
    /// Circuit components are not the same as devtools-placeable room objects 
    /// - they own a devtools-placeable room object, and the component behaviour is handled in an abstract way
    /// by classes that implement this interface.<br/><br/>
    /// If you don't need to inherit from another class, 
    /// it's recommended to inherit from <see cref="BaseComponent"/> rather than implementing this interface.
    /// </summary>
    public interface ICircuitComponent
    {
        /// <summary>
        /// Basic component type: input or output.<br/>
        /// Logic gates and flipflops are treated as inputs (sorry) 
        ///     - their role as the output of a circuit is handled by the <see cref="CircuitController_old"/>.
        /// </summary>
        CompType Type { get; set; }


        /// <summary>
        /// The type of input, like a button or switch.<br/>
        /// Output components should have this set to <see cref="InputType.NotAnInput"/>.<br/>
        /// See <see cref="BaseComponent.InType"/> for the recommended implementation.
        /// </summary>
        InputType InType { get; }


        /// <summary>
        /// Whether or not the component is supplying power to a circuit or is being powered by a circuit.
        /// To avoid components storing activity state that persists through room reloading, 
        /// this should be set to false in your component's constructor.
        /// See <see cref="BaseComponent(PlacedObject, Room, CompType, InputType)"/>.
        /// </summary>
        bool Activated { get; set; }


        /// <summary>
        /// ID of the circuit that the component currently belongs to.<br/>
        /// See <see cref="BaseComponent.CurrentCircuitID"/> for the recommended implementation.
        /// </summary>
        string CurrentCircuitID { get; }


        /// <summary>
        /// ID of the circuit that the component belonged to last update.<br/>
        /// You shouldn't touch this - this is for the controller to deal with.
        /// </summary>
        string LastCircuitID { get; set; }


        /// <summary>
        /// ManagedData from henpemaz's managed object framework.<br/>
        /// See <see cref="BaseComponent.Data"/> the recommended implementation.
        /// </summary>
        ManagedPlacedObjects.PlacedObjectsManager.ManagedData Data { get; }


        /// <summary>
        /// For component serialisation - must be undone by <see cref="FromString"/>
        /// </summary>
        string ToString();

        /// <summary>
        /// For component deserialisation - must be undone by <see cref="ToString"/>
        /// </summary>
        /// <returns></returns>
        object FromString(string s);

    }

}
