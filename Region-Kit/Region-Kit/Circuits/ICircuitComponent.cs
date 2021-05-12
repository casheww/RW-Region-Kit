
namespace RegionKit.Circuits
{
    /// <summary>
    /// An interface for creating Circuits components.
    /// If you can inherit from <see cref="BaseComponent"/>, that is recommended.
    /// Otherwise, use BaseComponent as a guide for steps you should take for standard component mechanics.
    /// </summary>
    public interface ICircuitComponent
    {
        /// <summary>
        /// Whether or not the component is supplying power to a circuit or is being powered by a circuit.
        /// To avoid components storing activity state that persists through room reloading, 
        /// this should be set to false in your component's constructor.
        /// See <see cref="BaseComponent(PlacedObject, Room, CompType, InputType)"/>.
        /// </summary>
        bool Activated { get; set; }

        /// <summary>
        /// The type of component, i.e.: Input or Output.
        /// I know it's a little sacreligious, but logic gates and flipflops are Input
        /// components that are treated with some extra steps by the <see cref="CircuitController"/>.
        /// </summary>
        CompType Type { get; set; }

        /// <summary>
        /// The kind of input this component provides. This is used by the <see cref="CircuitController"/>
        /// to determine whether extra processing steps are required
        /// (like in the case of logic gates).
        /// Output components should have an InType of NotAnInput.
        /// See <see cref="BaseComponent.InType"/> for an implementation example.
        /// </summary>
        InputType InType { get; }

        /// <summary>
        /// ID of the circuit that the component currently belongs to.
        /// See <see cref="BaseComponent.CurrentCircuitID"/> for the recommended implementation.
        /// </summary>
        string CurrentCircuitID { get; }

        /// <summary>
        /// ID of the circuit that the component belonged to last update.
        /// You shouldn't touch this - this is for the controller to deal with.
        /// </summary>
        string LastCircuitID { get; set; }

        /// <summary>
        /// ManagedData from henpemaz's managed object framework.
        /// See <see cref="BaseComponent.Data"/> the recommended implementation.
        /// </summary>
        ManagedPlacedObjects.PlacedObjectsManager.ManagedData Data { get; }

    }
}
