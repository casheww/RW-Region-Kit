using UnityEngine;

namespace RegionKit.Circuits
{
    class ComponentRepresentation : DevInterface.PlacedObjectRepresentation
    {
        public ComponentRepresentation(DevInterface.DevUI owner, string IDstring,
                DevInterface.DevUINode parentNode, PlacedObject pObj, string name, bool isNewObject)
                : base(owner, IDstring, parentNode, pObj, name)
        {
            subNodes.Add(new ComponentControlPanel(
                owner, name+"_Panel", this, new Vector2(0, 40), pObj, name));

            if (isNewObject)
            {
                switch (name)
                {
                    case "ImpactButton":
                        component = new ImpactButton(pObj, owner.room);
                        break;
                    case "BasicCircuitLight":
                        component = new BasicLight(pObj, owner.room);
                        break;
                    case "CircuitSwitch":
                        component = new Switch(pObj, owner.room);
                        break;
                }
                owner.room.AddObject(component);
            }
        }

        BaseComponent component;


        class ComponentControlPanel : DevInterface.Panel
        {
            public ComponentControlPanel(DevInterface.DevUI owner, string IDstring,
                    DevInterface.DevUINode parentNode, Vector2 pos, PlacedObject pObj, string name)
                    : base(owner, IDstring, parentNode, pos, new Vector2(205, 20), name)
            {
                subNodes.Add(new CircuitIntegerControl(
                    owner, "Circuit_Number", this, new Vector2(5, 5), "Circuit: ", pObj));

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

        class CircuitIntegerControl : DevInterface.IntegerControl
        {
            public CircuitIntegerControl(DevInterface.DevUI owner, string IDstring,
                    DevInterface.DevUINode parentNode, Vector2 pos, string title,
                    PlacedObject pObj)
                    : base(owner, IDstring, parentNode, pos, title)
            {
                this.pObj = pObj;
            }

            readonly PlacedObject pObj;

            public override void Refresh()
            {
                NumberLabelText = (pObj.data as BaseComponentData).circuitNumber.ToString();
                base.Refresh();
            }

            public override void Increment(int change)
            {
                base.Increment(change);
                BaseComponentData data = (pObj.data as BaseComponentData);
                
                data.circuitNumber += change;
                if (data.circuitNumber < CircuitController.minCircuit)
                {
                    data.circuitNumber = CircuitController.maxCircuit;
                }
                else if (data.circuitNumber > CircuitController.maxCircuit)
                {
                    data.circuitNumber = CircuitController.minCircuit;
                }
            }
        }

        class ComponentSlider : DevInterface.SSLightRodRepresentation.SSLightRodControlPanel.DepthControlSlider
        {
            public ComponentSlider(DevInterface.DevUI owner, string IDstring,
                    DevInterface.DevUINode parentNode, Vector2 pos, string title)
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
