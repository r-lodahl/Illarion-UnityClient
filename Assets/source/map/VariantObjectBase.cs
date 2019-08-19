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

        public int getNextId(int id)
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

        public float[] getOffset(int id)
        {
            int idIndex = Array.IndexOf(ids, id);
            
            if (idIndex == -1)
            {
                Game.Logger.Debug($"Did not find next Id for item id {id}");
                return new float[] {offsetX[0], offsetY[0]};
            }

            return new float[] {offsetX[idIndex], offsetY[idIndex]};
        }

        public VariantObjectBase(int[] ids, float[] offsetX, float[] offsetY, float red, float green, float blue, float alpha, float sizeVariance, int encodedLight) : base(red, green, blue, alpha, sizeVariance, encodedLight)
        {
            this.ids = ids;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }
    }
}