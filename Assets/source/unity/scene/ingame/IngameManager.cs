using Illarion.Client.Map;
using Illarion.Client.Unity.Map;
using UnityEngine;

namespace Illarion.Client.Unity.Scene.Ingame
{
    /// <summary>
    /// Main unity connection for the map scene
    /// </summary>
    public class IngameManager : MonoBehaviour
    {
        [SerializeField] private Player player = null;

        private ChunkLoader chunkLoader;
        private IsometricLayeredTilemap tilemap;

        /// <summary>
        /// Initializes objects for data loading: BinaryLoader, ChunkLoader
        /// Loads initial chunks
        /// Connects chunkLoader and tilemap
        /// </summary>
        private void Start()
        {
            var binaryLoader = new BinaryLoader();

            var itemBaseDictionary = binaryLoader.LoadItemBaseDicitionary();

            tilemap = GetComponent<IsometricLayeredTilemap>();
            chunkLoader = new ChunkLoader(376, 271, player); 

            tilemap.RegisterItemBases(itemBaseDictionary);
            tilemap.RegisterChunkLoader(chunkLoader);

            chunkLoader.ReloadChunks();
        }
    }
}