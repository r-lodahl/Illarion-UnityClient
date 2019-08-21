using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Illarion.Client.Unity.Common;
using Illarion.Client.Unity.Map;
using Illarion.Client.Common;
using Illarion.Client.Map;

namespace Illarion.Client.Unity.Scene.Ingame
{
    public class IsometricLayeredTilemap : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap = null;
        
        [SerializeField] private SpriteRenderer spritePrefab = null;

        private int referenceLayer = 0;
        private Tile[] tiles;
        private Sprite[] sprites;
        
        private Dictionary<Chunk, DynamicChunk> loadedChunks;

        private SpritePool spritePool;

        private Dictionary<int, MapObjectBase> itemBases;

        private void Awake()
        {
            loadedChunks = new Dictionary<Chunk, DynamicChunk>(9);

            tiles = Resources.LoadAll<Tile>(Constants.UserData.TilesetPath);
            sprites = Resources.LoadAll<Sprite>(Constants.UserData.ItemsetPath);
            
            spritePool = new SpritePool(spritePrefab, 2000);
        }

        public void RegisterItemBases(Dictionary<int, MapObjectBase> itemBases)
        {
            this.itemBases = itemBases;
        }

        public void RegisterChunkLoader(ChunkLoader chunkLoader)
        {
            chunkLoader.ChunkLoaded += OnChunkLoaded;
            chunkLoader.ChunkUnloading += OnChunkUnloading;
        }

        private void OnChunkLoaded(object sender, Chunk chunk)
        {
            if (chunk == null) return;

            var dynamicChunk = new DynamicChunk(spritePool);
            loadedChunks.Add(chunk, dynamicChunk);

            var applicableLayerIndices = GetApplicableLayerIndices(chunk.Layers);

            for (int xy = 0; xy < chunk.Map.Length; xy++)
            {
                int x = xy / Constants.Map.Chunksize + chunk.Origin[0];
                int y = xy % Constants.Map.Chunksize + chunk.Origin[1];

                foreach (int layerIndex in applicableLayerIndices)
                {
                    UncompressTileId(chunk.Map[xy][layerIndex], out var tileId, out var overlayId);

                    int layer = chunk.Layers[layerIndex];

                    int zValue = (referenceLayer-layer) * 2;

                    if (tileId > 0) tilemap.SetTile(new Vector3Int(
                        x - layer * 2,
                        -y + layer * 2,
                        zValue), tiles[tileId]);
                    
                    if (overlayId > 0) tilemap.SetTile(new Vector3Int(
                        x - layer * 2, 
                        -y + layer * 2,
                        zValue - 1), tiles[overlayId]);

                    var tilePosition = new Vector3i(x, y, layer);
                    if (chunk.Items.TryGetValue(tilePosition, out var items)) 
                    {
                        float n = 0f;
                        foreach (var item in items)
                        {
                            var spriteItem = spritePool.Get();
                            var itemBase = itemBases[item.ObjectId];
                            var sprite = sprites[item.ObjectId];

                            float[] offset;
                            if (itemBase is SimpleObjectBase simpleBase)
                            {
                                offset = simpleBase.Offset;
                            }
                            else
                            {
                                var variantBase = (VariantObjectBase) itemBase;
                                offset = variantBase.GetOffset(item.ObjectId);
                            }

                            var position = new Vector3(
                                (x + y) * (38f/76f) - (1f/76f) - sprite.bounds.extents.x + offset[0],
                                (x - y) * (19f/76f) + 0.25f + offset[1] + dynamicChunk.GetHeightLevel(tilePosition),
                                zValue - 1 - ((y-x) / 20000f) - 0.3f - n
                            );

                            spriteItem.transform.position = position;
                            spriteItem.sprite = sprite;
                            spriteItem.gameObject.SetActive(true);

                            if (itemBase.Height != 0.0f) dynamicChunk.IncreaseHeightLevel(tilePosition, itemBase.Height);
                            dynamicChunk.RegisterItem(spriteItem);

                            n += 0.000001f;
                        }
                    }
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

            loadedChunks[chunk].Unload();
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