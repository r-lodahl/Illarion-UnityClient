using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Illarion.Client.Unity.Common;
using Illarion.Client.Common;
using Illarion.Client.Map;

namespace Illarion.Client.Unity.Scene.Ingame
{
    public class IsometricLayeredTilemap : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap = null;
        
        private int referenceLayer;
        private Tile[] tiles;
        private Sprite[] sprites;
        
        private HashSet<Chunk> loadedChunks;

        private SpritePool spritePool;

        private void Awake()
        {
            loadedChunks = new HashSet<Chunk>();

            tiles = Resources.LoadAll<Tile>(Constants.UserData.TilesetPath);

            sprites = Resources.LoadAll<Sprite>(Constants.UserData.ItemsetPath);
        }

        public void RegisterChunkLoader(ChunkLoader chunkLoader)
        {
            chunkLoader.ChunkLoaded += OnChunkLoaded;
            chunkLoader.ChunkUnloading += OnChunkUnloading;
        }

        private void OnChunkLoaded(object sender, Chunk chunk)
        {
            if (chunk == null) return;

            loadedChunks.Add(chunk);

            var applicableLayerIndices = GetApplicableLayerIndices(chunk.Layers);

            for (int xy = 0; xy < chunk.Map.Length; xy++)
            {
                int x = xy / Constants.Map.Chunksize + chunk.Origin[0];
                int y = xy % Constants.Map.Chunksize + chunk.Origin[1];

                foreach (int layerIndex in applicableLayerIndices)
                {
                    UncompressTileId(chunk.Map[xy][layerIndex], out var tileId, out var overlayId);

                    int layer = chunk.Layers[layerIndex];

                    if (tileId > 0) tilemap.SetTile(new Vector3Int(x, -y, layer * Constants.Map.LayerDrawingFactor), tiles[tileId]);
                    
                    if (overlayId > 0) tilemap.SetTile(new Vector3Int(x - Constants.Map.OverlayCellMinus, 
                        -y - Constants.Map.OverlayCellMinus, layer * Constants.Map.LayerDrawingFactor
                        + Constants.Map.OverlayDrawingAdd), tiles[overlayId]);
                }
            }
        }

        private void OnChunkUnloading(object sender, Chunk chunk)
        {
            if (chunk == null) return;

            var applicableLayerIndices = GetApplicableLayerIndices(chunk.Layers);

            for (int xy = 0; xy < chunk.Map.Length; xy++)
            {
                int x = xy / Constants.Map.Chunksize + chunk.Origin[0];
                int y = xy % Constants.Map.Chunksize + chunk.Origin[1];

                foreach (int layerIndex in applicableLayerIndices)
                {
                    UncompressTileId(chunk.Map[xy][layerIndex], out var tileId, out var overlayId);

                    int layer = chunk.Layers[layerIndex];

                    if (tileId > 0) tilemap.SetTile(new Vector3Int(x, -y, layer * Constants.Map.LayerDrawingFactor), null);
                    
                    if (overlayId > 0) tilemap.SetTile(new Vector3Int(x - Constants.Map.OverlayCellMinus, 
                        -y - Constants.Map.OverlayCellMinus, layer * Constants.Map.LayerDrawingFactor
                        + Constants.Map.OverlayDrawingAdd), null);
                }
            }

            loadedChunks.Remove(chunk);
        }

        private int[] GetApplicableLayerIndices(int[] chunkLayers) 
        {
            List<int> usedLayersIndices = new List<int>(20);
            for (int layerIndex = 0; layerIndex < chunkLayers.Length; layerIndex++)
            {
                int layer = chunkLayers[layerIndex];

                if (layer > referenceLayer + Constants.Map.VisibleLayers 
                || layer < referenceLayer - Constants.Map.VisibleLayers) continue;

                usedLayersIndices.Add(layerIndex);
            }
            return usedLayersIndices.ToArray();
        }

        private void UncompressTileId(int compressedId, out int tileId, out int overlayId)
        {
            tileId = compressedId % Constants.Tile.OverlayFactor;
            overlayId = compressedId / Constants.Tile.OverlayFactor;
        }
    }
}