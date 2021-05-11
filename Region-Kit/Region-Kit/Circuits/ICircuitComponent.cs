
namespace RegionKit.Circuits
{
    interface ICircuitComponent
    {
        bool Activated { get; set; }

        CompType Type { get; set; }
        InputType InType { get; }
    }
}
