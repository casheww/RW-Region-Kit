using System.Text.RegularExpressions;
using UnityEngine;

namespace RegionKit.Circuits
{
    class ColorComponentData : BaseComponentData
    {
        public ColorComponentData(PlacedObject owner) : base(owner)
        {
            color = Color.white;
            strength = 0;
        }

        public Color color;
        public float strength;
        public float flicker;

        public override void FromString(string s)
        {
            base.FromString(s);
            string[] array = Regex.Split(s, "~");
            if (array.Length >= 6)
            {
                float r = float.Parse(array[1]);
                float g = float.Parse(array[2]);
                float b = float.Parse(array[3]);
                color = new Color(r, g, b);

                strength = float.Parse(array[4]);
                flicker = float.Parse(array[5]);
            }
        }

        public override string ToString()
        {
            string s = base.ToString();
            return string.Concat(new object[]
            {
                s,
                "~",
                color.r,
                "~",
                color.g,
                "~",
                color.b,
                "~",
                strength,
                "~",
                flicker
            });
        }

    }
}
