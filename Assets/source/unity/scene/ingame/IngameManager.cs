using Illarion.Client.Map;
using Illarion.Client.Unity.Map;
using UnityEngine;

namespace Illarion.Client.Unity.Scene.Ingame
{
    public class IngameManager : MonoBehaviour
    {
        [SerializeField] private Player player = null;

        private ChunkLoader chunkLoader;
        private IsometricLayeredTilemap tilemap;

        private void Start()
        {
            var binaryLoader = new BinaryLoader();

            var itemBaseDictionary = binaryLoader.LoadItemBaseDicitionary();

            tilemap = GetComponent<IsometricLayeredTilemap>();
            chunkLoader = new ChunkLoader(354, 275, player); 

            tilemap.RegisterItemBases(itemBaseDictionary);
            tilemap.RegisterChunkLoader(chunkLoader);

            chunkLoader.ReloadChunks();
        }
    }
}