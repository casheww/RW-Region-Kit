using DevInterface;
using UnityEngine;

namespace RegionKit.Circuits
{
    class ComponentRepresentation : PlacedObjectRepresentation
    {
        public ComponentRepresentation(DevUI owner, string IDstring, DevUINode parentNode,
                PlacedObject pObj, string name)
                : base(owner, IDstring, parentNode, pObj, name)
        {
            subNodes.Add(new ComponentControlPanel(
                owner, name+"_Panel", this, new Vector2(0, 40), pObj, name));
        }

        class ComponentControlPanel : Panel
        {
            public ComponentControlPanel(DevUI owner, string IDstring,
                    DevUINode parentNode, Vector2 pos, PlacedObject pObj, string name)
                    : base(owner, IDstring, parentNode, pos, new Vector2(205, 20), name)
            {
                subNodes.Add(new CircuitStringControl(
                        owner, "Component_Key", this, new Vector2(5, 5), "ID: ", pObj));

                if (pObj.data is ColorComponentData)
                {
                    size = new Vector2(250, 120);
                    subNodes.Add(new ComponentSlider(owner, "Flicker_Slider", this, new Vector2(5, 105), "Flicker: "));
                    subNodes.Add(new ComponentSlider(owner, "Strength_Slider", this, new Vector2(5, 85), "Strength: "));
                    subNodes.Add(new ComponentSlider(owner, "ColorR_Slider", this, new Vector2(5, 65), "Color R: "));
                    subNodes.Add(new ComponentSlider(owner, "ColorG_Slider", this, new Vector2(5, 45), "Color G: "));
                    subNodes.Add(new ComponentSlider(owner, "ColorB_Slider", this, new Vector2(5, 25), "Color B: "));
                }
            }

        }

        class CircuitStringControl : CustomDevInterface.StringControl
        {
            public CircuitStringControl(DevUI owner, string IDstring, DevUINode parentNode,
                    Vector2 pos, string title, PlacedObject pObj)
                    : base(owner, IDstring, parentNode, pos, title, (pObj.data as BaseComponentData).CircuitID)
            {
                this.pObj = pObj;
            }

            readonly PlacedObject pObj;

            public override void SetValue(string newKey)
            {
                (pObj.data as BaseComponentData).CircuitID = newKey;
            }

        }

        class ComponentSlider : SSLightRodRepresentation.SSLightRodControlPanel.DepthControlSlider
        {
            public ComponentSlider(DevUI owner, string IDstring, DevUINode parentNode,
                    Vector2 pos, string title)
                    : base(owner, IDstring, parentNode, pos, title)
            { }

            public override void Refresh()
            {
                base.Refresh();
                float num = 0;
                string idstring = IDstring;
                ColorComponentData data = ((parentNode.parentNode as ComponentRepresentation).pObj.data as ColorComponentData);

                switch (idstring)
                {
                    case "ColorR_Slider":
                        num = data.color.r;
                        NumberText = ((int)(num * 255)).ToString();
                        break;
                    case "ColorG_Slider":
                        num = data.color.g;
                        NumberText = ((int)(num * 255)).ToString();
                        break;
                    case "ColorB_Slider":
                        num = data.color.b;
                        NumberText = ((int)(num * 255)).ToString();
                        break;

                    case "Strength_Slider":
                        num = data.strength;
                        NumberText = num.ToString();
                        break;
                    case "Flicker_Slider":
                        num = data.flicker;
                        NumberText = num.ToString();
                        break;
                }
                RefreshNubPos(num);
            }

            public override void NubDragged(float nubPos)
            {
                ColorComponentData data = (parentNode.parentNode as ComponentRepresentation).pObj.data as ColorComponentData;
                switch (IDstring)
                {
                    case "ColorR_Slider":
                        data.color.r = nubPos;
                        break;
                    case "ColorG_Slider":
                        data.color.g = nubPos;
                        break;
                    case "ColorB_Slider":
                        data.color.b = nubPos;
                        break;

                    case "Strength_Slider":
                        data.strength = nubPos;
                        break;
                    case "Flicker_Slider":
                        data.flicker = nubPos;
                        break;
                }
                base.NubDragged(nubPos);
            }

        }

    }

}
