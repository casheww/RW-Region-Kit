using DevInterface;
using UnityEngine;
using RWCustom;

namespace RegionKit {

    public static class EnumExt_Objects {
        // PW light rod
        public static PlacedObject.Type PWLightrod;

        // circuit components
        public static PlacedObject.Type ImpactButton;
        public static PlacedObject.Type BasicCircuitLight;
        public static PlacedObject.Type CircuitSwitch;
    }

    class RoomLoader {
        public static void Patch() {
            On.Room.Loaded += Room_Loaded;
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
        }

        public static void Disable() {
            On.Room.Loaded -= Room_Loaded;
            On.PlacedObject.GenerateEmptyData -= PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep -= ObjectsPage_CreateObjRep;
        }

        public static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            bool isNewObject = false;
            PlacedObjectRepresentation rep = null;

            if (tp == EnumExt_Objects.PWLightrod) {
                if (pObj == null) {
                    isNewObject = true;
                    pObj = new PlacedObject(tp, null) {
                        pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    self.RoomSettings.placedObjects.Add(pObj);
                    self.owner.room.AddObject(new PWLightRod(pObj, self.owner.room));
                }
                rep = new PWLightRodRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString(), isNewObject);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }

            else if (pObj.data is Circuits.BaseComponentData componentData)
            {
                Circuits.BaseComponent component = null;

                if (pObj == null)
                {
                    pObj = new PlacedObject(tp, null);
                    self.RoomSettings.placedObjects.Add(pObj);
                    pObj.pos = self.owner.room.game.cameras[0].pos +
                        Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) +
                        Custom.DegToVec(Random.value * 360f) * 0.2f;

                    if (tp == EnumExt_Objects.ImpactButton)
                    {
                        component = new Circuits.ImpactButton(pObj, self.owner.room);
                    }
                    else if (tp == EnumExt_Objects.BasicCircuitLight)
                    {
                        component = new Circuits.BasicLight(pObj, self.owner.room);
                    }
                    else if (tp == EnumExt_Objects.CircuitSwitch)
                    {
                        component = new Circuits.Switch(pObj, self.owner.room);
                    }
                    else
                    {
                        Debug.Log($"u wot? invalid component {tp}");
                    }
                }

                rep = new Circuits.ComponentRepresentation(
                        self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());

                if (component != null)
                {
                    self.owner.room.AddObject(component);
                }
            }

            else
            {
                orig(self, tp, pObj);
            }

            if (rep != null)
            {
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }

        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self) {
            orig(self);
            // PW light rod
            if (self.type == EnumExt_Objects.PWLightrod)
            {
                self.data = new PWLightRodData(self);
            }

            // add other stuff BEFORE circuits else

            // circuit components
            else
            {
                GenerateCircuitComponentData(self);
            }
            
        }

        static void GenerateCircuitComponentData(PlacedObject pObj)
        {
            if (pObj.type == EnumExt_Objects.ImpactButton ||
                pObj.type == EnumExt_Objects.CircuitSwitch)
            {
                pObj.data = new Circuits.InputComponentData(pObj);
            }
            else if (pObj.type == EnumExt_Objects.BasicCircuitLight)
            {
                pObj.data = new Circuits.ColorComponentData(pObj);
            }
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self) {
            orig(self);
            //ManyMoreFixes Patch
            if (self.game == null) { return; }
            //Load Objects
            for (int l = 0; l < self.roomSettings.placedObjects.Count; ++l) {
                var pObj = self.roomSettings.placedObjects[l];
                if (pObj != null && pObj.active)
                {
                    UpdatableAndDeletable uad = null;
                    if (pObj.type == EnumExt_Objects.PWLightrod)
                    {
                        uad = new PWLightRod(pObj, self);
                    }
                    else if (pObj.type == EnumExt_Objects.ImpactButton)
                    {
                        uad = new Circuits.ImpactButton(pObj, self);
                    }
                    else if (pObj.type == EnumExt_Objects.BasicCircuitLight)
                    {
                        uad = new Circuits.BasicLight(pObj, self);
                    }
                    else if (pObj.type == EnumExt_Objects.CircuitSwitch)
                    {
                        uad = new Circuits.Switch(pObj, self);
                    }

                    if (uad != null)
                    {
                        self.AddObject(uad);
                    }
                }
            }
        }


    }
}
