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
        private Dictionary<VariantObjectBase, AnimationRunner> animationRunners;

        private void Awake()
        {
            loadedChunks = new Dictionary<Chunk, DynamicChunk>(9);

            tiles = Resources.LoadAll<Tile>(Constants.UserData.TilesetPath);
            sprites = Resources.LoadAll<Sprite>(Constants.UserData.ItemsetPath);
            
            spritePool = new SpritePool(spritePrefab, 2000);

            AnimationRunner.ItemSprites = sprites;
            animationRunners = new Dictionary<VariantObjectBase, AnimationRunner>();
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

                    int xPos = x + layer * 3;
                    int yPos = -y + layer * 3;
                    int zDepth = (referenceLayer-layer) * 2;

                    if (tileId > 0) tilemap.SetTile(new Vector3Int(
                        xPos,
                        yPos,
                        zDepth), tiles[tileId]);
                    
                    zDepth--;
                    if (overlayId > 0) tilemap.SetTile(new Vector3Int(
                        xPos,
                        yPos,
                        zDepth), tiles[overlayId]);

                    var tilePosition = new Vector3i(x, y, layer);
                    if (chunk.Items.TryGetValue(tilePosition, out var items)) 
                    {
                        float xPosObject = (xPos-yPos) * (38f/76f) - (1f/76f);
                        float yPosObject = (xPos+yPos) * (19f/76f) + 0.25f;
                        float zDepthObject = zDepth - ((y-x) / 20000f) - 0.3f;
                        foreach (var item in items)
                        {
                            var spriteItem = spritePool.Get();
                            var itemBase = itemBases[item.BaseId];
                            
                            Sprite sprite;
                            float[] offset;
                            if (itemBase is SimpleObjectBase simpleBase)
                            {
                                sprite = sprites[simpleBase.SpriteId];
                                offset = simpleBase.Offset;
                            }
                            else
                            {
                                var variantBase = (VariantObjectBase) itemBase;

                                int itemId;;
                                if (variantBase.IsAnimated)
                                {
                                    if (!animationRunners.TryGetValue(variantBase, out var animationRunner)) 
                                    {
                                        animationRunner = new AnimationRunner(variantBase, variantBase.InitialId);
                                        animationRunners.Add(variantBase, animationRunner);
                                    }

                                    animationRunner.RegisterAnimatedSprite(spriteItem);

                                    itemId = variantBase.InitialId;
                                }
                                else
                                {
                                    itemId = variantBase.GetFrameId(MapVariance.GetItemFrameVariance(x, y, variantBase.FrameCount));
                                }

                                sprite = sprites[itemId];
                                offset = variantBase.GetOffset(itemId);
                            }

                            var position = new Vector3(
                                xPosObject - sprite.bounds.extents.x + offset[0],
                                yPosObject + offset[1] + dynamicChunk.GetHeightLevel(tilePosition),
                                zDepthObject
                            );

                            spriteItem.sprite = sprite;
                            spriteItem.transform.position = position;
                            
                            var scale = itemBase.SizeVariance;
                            if (scale > 0f)
                            {
                                scale = MapVariance.GetItemScaleVariance(x, y, scale);
                                spriteItem.transform.localScale = new Vector3(scale, scale, 1f);                            
                            }

                            spriteItem.gameObject.SetActive(true);

                            if (itemBase.Height != 0.0f) dynamicChunk.IncreaseHeightLevel(tilePosition, itemBase.Height);
                            dynamicChunk.RegisterItem(spriteItem);

                            zDepthObject -= 0.000001f;
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

        private void Update()
        {
            foreach(var animationRunner in animationRunners) animationRunner.Value.Tick(Time.deltaTime);
        }
    }
}