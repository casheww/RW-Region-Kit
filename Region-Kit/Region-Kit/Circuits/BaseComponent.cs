using ManagedPlacedObjects;
using UnityEngine;

namespace RegionKit.Circuits
{
    abstract class BaseComponent : UpdatableAndDeletable, IDrawable
    {
        public BaseComponent(PlacedObject pObj, Room room)
        {
            this.pObj = pObj;
            this.room = room;
            activated = false;
            Debug.Log($"created circuits component (type:{this.GetType()})");
        }

        public PlacedObject pObj;
        public Type type;
        protected InputType _inType;

        public InputType InType => type == Type.Input ? _inType : InputType.NotAnInput;

        protected bool activated;

        public virtual void Activate() { activated = true; }
        public virtual void Deactivate() { activated = false; }

        public override void Update(bool eu)
        {
            base.Update(eu);

            PlacedObjectsManager.ManagedData data = pObj.data as PlacedObjectsManager.ManagedData;

            if (firstUpdate)
            {
                if (type == Type.Input)
                {
                    // load saved activation status
                    activated = data.GetValue<bool>(MKeys.activated);
                }
                firstUpdate = false;
            }

            // update circuit ID based on dev input
            string currentCircuitID = this is LogicGate ? data.GetValue<string>(MKeys.output) : data.GetValue<string>(MKeys.circuitID);
            if (lastCircuitID != currentCircuitID)
            {
                CircuitController.Instance.MigrateComponent(lastCircuitID, currentCircuitID, this);
                lastCircuitID = currentCircuitID;
            }
        }
        bool firstUpdate = true;

        string lastCircuitID = null;

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Items");
            }
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (sLeaser.sprites == null) return;

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.color = palette.blackColor;
            }
        }

        // placeholder
        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].x = pObj.pos.x - camPos.x;
            sLeaser.sprites[0].y = pObj.pos.y - camPos.y;
            sLeaser.sprites[0].color = Color.red;
            sLeaser.sprites[0].scaleX = 20;
            sLeaser.sprites[0].scaleY = 15;
            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        // placeholder
        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].scaleX = 4;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            AddToContainer(sLeaser, rCam, null);
        }


        public enum Type
        {
            Input,
            Output,
        }

        public enum InputType
        {
            NotAnInput,
            Button,
            Switch,
            LogicGate
        }

    }
}
