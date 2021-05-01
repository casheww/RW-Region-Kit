using System.Text.RegularExpressions;

namespace RegionKit.Circuits
{
    class InputComponentData : BaseComponentData
    {
        public InputComponentData(PlacedObject owner) : base(owner)
        {
            activated = false;
        }

        public bool activated;
    }
}
