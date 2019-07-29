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
        
        [SerializeField] private SpriteRenderer spritePrefab = null;

        private int referenceLayer;
        private Tile[] tiles;
        private Sprite[] sprites;
        
        private HashSet<Chunk> loadedChunks;

        private SpritePool spritePool;

        private Dictionary<int, MapObjectBase> itemBases;

        private void Awake()
        {
            loadedChunks = new HashSet<Chunk>();

            tiles = Resources.LoadAll<Tile>(Constants.UserData.TilesetPath);

            sprites = Resources.LoadAll<Sprite>(Constants.UserData.ItemsetPath);
            
            spritePool = new SpritePool(spritePrefab, 2000);

            Debug.Log(tilemap.CellToLocal(new Vector3Int(0,0,0)));
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

                    if (chunk.Items.TryGetValue(new Vector3i(x, y, layer), out var items)) 
                    {
                        foreach (var item in items)
                        {
                            var spriteItem = spritePool.Get();
                            var itemBase = itemBases[item.ObjectId];

                            var sprite = sprites[item.ObjectId];

                            var position = tilemap.CellToLocal(new Vector3Int(x, -y, 0));


                            position.x += itemBase.OffsetX;// + itemBase.OffsetX;// - 0.5f - itemBase.OffsetY; //itemBase.OffsetX
                            position.y += 0.25f + sprite.bounds.extents.y + itemBase.OffsetY; // - itemBase.OffsetY;// - itemBase.OffsetX; //-itemBase.OffsetY

                            spriteItem.transform.position = position;
                            spriteItem.sortingOrder = layer * 4 + 1;
                            spriteItem.sprite = sprite;
                            spriteItem.gameObject.SetActive(true);
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