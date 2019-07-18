using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Illarion.Client.Common;
using Illarion.Client.Map;

namespace Illarion.Client.Unity.Scene.Ingame
{
    public class IsometricLayeredTilemap : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        
        private int referenceLayer;
        private Tile[] tiles;
        private HashSet<Chunk> loadedChunks;

        private void Awake()
        {
            loadedChunks = new HashSet<Chunk>();
            tiles = Resources.LoadAll<Tile>(Constants.UserData.TilesetPath);
        }

        public void RegisterChunkLoader(ChunkLoader chunkLoader)
        {
            chunkLoader.ChunkLoaded += OnChunkLoaded;
            chunkLoader.ChunkUnloading += OnChunkUnloading;
        }

        private void OnChunkLoaded(object sender, Chunk chunk)
        {
            loadedChunks.Add(chunk);

            for (int layerIndex = 0; layerIndex < chunk.Map.Length; layerIndex++)
            {
                int layer = chunk.Layers[layerIndex];

                if (layer > referenceLayer + Constants.Map.VisibleLayers 
                || layer < referenceLayer - Constants.Map.VisibleLayers) continue;

                var layerMap = chunk.Map[layerIndex];

                for (int xy = 0; xy < layerMap.Length; xy++)
                {
                    int x = xy / Constants.Map.Chunksize;
                    int y = xy % Constants.Map.Chunksize;

                    int compressedTileId = layerMap[xy];
                    int tileId = compressedTileId % Constants.Tile.OverlayFactor;
                    int overlayId = compressedTileId / Constants.Tile.OverlayFactor;

                    if (tileId > 0)
                    {
                        tilemap.SetTile(
                            new Vector3Int(x, y, layer * Constants.Map.LayerDrawingFactor),
                            tiles[tileId]
                        );
                    }

                    
                    if (overlayId > 0)
                    {
                        tilemap.SetTile(
                            new Vector3Int(
                                x,
                                y,
                                layer * Constants.Map.LayerDrawingFactor
                                + Constants.Map.OverlayDrawingAdd),
                            tiles[overlayId]
                        );
                    }
                }
            }
        }

        private void OnChunkUnloading(object sender, Chunk chunk)
        {
            for (int layerIndex = 0; layerIndex < chunk.Map.Length; layerIndex++)
            {
                int layer = chunk.Layers[layerIndex];

                if (layer > referenceLayer + Constants.Map.VisibleLayers 
                || layer < referenceLayer - Constants.Map.VisibleLayers) continue;

                var layerMap = chunk.Map[layerIndex];

                for (int xy = 0; xy < layerMap.Length; xy++)
                {
                    int x = xy / Constants.Map.Chunksize;
                    int y = xy % Constants.Map.Chunksize;

                    tilemap.SetTile(new Vector3Int(x,y,layer), null);
                }
            }

            loadedChunks.Remove(chunk);
        }
    }
}