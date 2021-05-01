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

        public static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj) {
            bool isNewObject = false;

            if (tp == EnumExt_Objects.PWLightrod) {
                if (pObj == null) {
                    isNewObject = true;
                    pObj = new PlacedObject(tp, null) {
                        pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    self.RoomSettings.placedObjects.Add(pObj);
                    self.owner.room.AddObject(new PWLightRod(pObj, self.owner.room));
                }
                PlacedObjectRepresentation rep = new PWLightRodRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString(), isNewObject);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }

            else if (tp == EnumExt_Objects.ImpactButton)
            {
                if (pObj == null)
                {
                    isNewObject = true;
                    pObj = new PlacedObject(tp, null)
                    {
                        pos = self.owner.room.game.cameras[0].pos +
                            Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) +
                            Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    self.RoomSettings.placedObjects.Add(pObj);
                    self.owner.room.AddObject(new Circuits.ImpactButton(pObj, self.owner.room));
                }
                PlacedObjectRepresentation rep = new Circuits.ComponentRepresentation(
                    self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString(), isNewObject);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }

            else if (tp == EnumExt_Objects.BasicCircuitLight)
            {
                if (pObj == null)
                {
                    isNewObject = true;
                    pObj = new PlacedObject(tp, null)
                    {
                        pos = self.owner.room.game.cameras[0].pos +
                            Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) +
                            Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    self.RoomSettings.placedObjects.Add(pObj);
                    self.owner.room.AddObject(new Circuits.BasicLight(pObj, self.owner.room));
                }
                PlacedObjectRepresentation rep = new Circuits.ComponentRepresentation(
                    self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString(), isNewObject);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }

            else if (tp == EnumExt_Objects.CircuitSwitch)
            {
                if (pObj == null)
                {
                    isNewObject = true;
                    pObj = new PlacedObject(tp, null)
                    {
                        pos = self.owner.room.game.cameras[0].pos +
                            Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) +
                            Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    self.RoomSettings.placedObjects.Add(pObj);
                    self.owner.room.AddObject(new Circuits.Switch(pObj, self.owner.room));
                }
                PlacedObjectRepresentation rep = new Circuits.ComponentRepresentation(
                    self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString(), isNewObject);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }

            else {
                orig(self, tp, pObj);
            }
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self) {
            orig(self);
            // PW light rod
            if (self.type == EnumExt_Objects.PWLightrod)
            {
                self.data = new PWLightRodData(self);
            }

            // circuit components
            else if (self.type == EnumExt_Objects.ImpactButton
                self.type == EnumExt_Objects.CircuitSwitch)
            {
                self.data = new Circuits.InputComponentData(self);
            }
            else if (self.type == EnumExt_Objects.BasicCircuitLight)
            {
                self.data = new Circuits.ColorComponentData(self);
            }
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self) {
            orig(self);
            //ManyMoreFixes Patch
            if (self.game == null) { return; }
            //Load Objects
            for (int l = 0; l < self.roomSettings.placedObjects.Count; ++l) {
                var obj = self.roomSettings.placedObjects[l];
                if (obj.active)
                {
                    // PW light rod
                    if (obj.type == EnumExt_Objects.PWLightrod)
                    {
                        self.AddObject(new PWLightRod(obj, self));
                    }

                    // Circuit components - they also require additional setup
                    else if (obj.data is Circuits.BaseComponentData componentData)
                    {
                        Circuits.BaseComponent component = null;

                        if (obj.type == EnumExt_Objects.ImpactButton)
                        {
                            component = new Circuits.ImpactButton(obj, self);
                        }
                        else if (obj.type == EnumExt_Objects.BasicCircuitLight)
                        {
                            component = new Circuits.BasicLight(obj, self);
                        }
                        else if (obj.type == EnumExt_Objects.CircuitSwitch)
                        {
                            component = new Circuits.Switch(obj, self);
                        }

                        self.AddObject(component);

                        // ... aforementioned additional setup
                        if (component != null)
                        {
                            Circuits.CircuitController.Instance
                                .AddComponent(componentData.circuitNumber, component);
                        }
                    }
                }
            }
        }


    }
}
