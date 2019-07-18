using Illarion.Client.Map;
using UnityEngine;

namespace Illarion.Client.Unity.Scene.Ingame
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Player player;

        private ChunkLoader chunkLoader;
        private IsometricLayeredTilemap tilemap;

        private void Awake()
        {
            chunkLoader = new ChunkLoader(0, 0, player);
            tilemap.RegisterChunkLoader(chunkLoader);
        }
    }
}