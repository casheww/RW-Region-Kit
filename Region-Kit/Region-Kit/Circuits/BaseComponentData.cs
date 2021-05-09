using System.Text.RegularExpressions;

namespace RegionKit.Circuits
{
    class BaseComponentData : PlacedObject.Data
    {
        public BaseComponentData(PlacedObject owner) : base(owner)
        {
            CircuitID = "-";
        }

        public override void FromString(string s)
        {
            base.FromString(s);
            string[] array = Regex.Split(s, "~");
            if (array.Length >= 1)
            {
                CircuitID = array[0];
            }
        }

        public override string ToString()
        {
            return CircuitID;
        }

        public string CircuitID;

    }
}
