using System.Collections.Generic;
using UnityEngine;
using Illarion.Client.Common;
using Illarion.Client.Unity.Common;

namespace Illarion.Client.Unity.Map
{
    public class DynamicChunk
    {
        private List<SpriteRenderer> loadedItems;
        private Dictionary<Vector3i, int> itemHeightLevel;
        private SpritePool spritePool;

        public DynamicChunk(SpritePool pool) 
        {
            spritePool = pool;
            loadedItems = new List<SpriteRenderer>();
            itemHeightLevel = new Dictionary<Vector3i, int>();
        }

        public void RegisterItem(SpriteRenderer item) => loadedItems.Add(item);

        public int GetHeightLevel(Vector3i position) => itemHeightLevel.TryGetValue(position, out var level) ? level : 0;

        public void Unload() 
        {
            foreach(var item in loadedItems) spritePool.Put(item);
        }
    }
}