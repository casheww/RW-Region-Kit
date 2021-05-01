using UnityEngine;

namespace RegionKit.Circuits
{
    class BaseComponent : UpdatableAndDeletable, IDrawable
    {
        public BaseComponent(PlacedObject pObj, Room room)
        {
            this.pObj = pObj;
            this.room = room;
        }

        public PlacedObject pObj;
        public Type type;
        protected InputType _inType;

        public InputType InType => type == Type.Output ? InputType.NotAnInput : _inType;

        public virtual void Activate() { }
        public virtual void Deactivate() { }

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
            Output
        }

        public enum InputType
        {
            NotAnInput,
            Button,
            Switch
        }

    }

}
