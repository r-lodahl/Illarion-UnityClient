using System;
using Illarion.Client.Common;

namespace Illarion.Client.Map
{
    [Serializable]
    public class VariantObjectBase : MapObjectBase
    {
        private int[] ids;
        private float[] offsetX;
        private float[] offsetY;

        public float AnimationSpeed {get;}
        public bool IsAnimated { get { return !(AnimationSpeed < 0.01f); } }
        public int InitialId { get { return ids[0]; } } 

        public int GetNextId(int id)
        {
            int idIndex = Array.IndexOf(ids, id);
            
            if (idIndex == -1)
            {
                Game.Logger.Debug($"Did not find next Id for item id {id}");
                return ids[0];
            }

            idIndex++;
            if (idIndex < ids.Length) return ids[idIndex];
            return ids[0];
        }

        public float[] GetOffset(int id)
        {
            int idIndex = Array.IndexOf(ids, id);
            
            if (idIndex == -1)
            {
                Game.Logger.Debug($"Did not find next Id for item id {id}");
                return new float[] {offsetX[0], offsetY[0]};
            }

            return new float[] {offsetX[idIndex], offsetY[idIndex]};
        }

        public VariantObjectBase(int[] ids, float[] offsetX, float[] offsetY, float animationSpeed, float red, float green, float blue, float alpha, float sizeVariance, int encodedLight, float height) : base(red, green, blue, alpha, sizeVariance, encodedLight, height)
        {
            this.ids = ids;
            this.offsetX = offsetX;
            this.offsetY = offsetY;

            AnimationSpeed = animationSpeed;
        }
    }
}