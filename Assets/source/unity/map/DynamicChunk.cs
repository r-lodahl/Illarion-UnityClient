using System.Collections.Generic;
using UnityEngine;
using Illarion.Client.Common;
using Illarion.Client.Unity.Common;

namespace Illarion.Client.Unity.Map
{
    public class DynamicChunk
    {
        private List<SpriteRenderer> loadedItems;
        private Dictionary<Vector3i, float> itemHeightLevel;
        private SpritePool spritePool;

        public DynamicChunk(SpritePool pool) 
        {
            spritePool = pool;
            loadedItems = new List<SpriteRenderer>();
            itemHeightLevel = new Dictionary<Vector3i, float>();
        }

        public void IncreaseHeightLevel(Vector3i position, float height) => itemHeightLevel[position] = GetHeightLevel(position) + height;

        public void RegisterItem(SpriteRenderer item) => loadedItems.Add(item);

        public float GetHeightLevel(Vector3i position) => itemHeightLevel.TryGetValue(position, out var level) ? level : 0.0f;

        public void Unload() 
        {
            foreach(var item in loadedItems) spritePool.Put(item);
        }
    }
}