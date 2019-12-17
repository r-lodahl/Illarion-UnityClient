using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Illarion.Client.Unity.Common;
using Illarion.Client.Unity.Map;
using Illarion.Client.Common;
using Illarion.Client.Map;

namespace Illarion.Client.Unity.Scene.Ingame
{
    /// <summary>
    /// Manager for determine which game data is currently displayed on the unity tilemap object
    /// </summary>
    public class IsometricLayeredTilemap : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap = null; 
        [SerializeField] private SpriteRenderer spritePrefab = null;

        private int referenceLayer = 0;
        private Tile[] tiles;
        private Sprite[] sprites;
        
        private SpritePool spritePool;

        private Dictionary<int, MapObjectBase> itemBases;
        private Dictionary<Chunk, DynamicChunk> loadedChunks;
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

        public void RegisterItemBases(Dictionary<int, MapObjectBase> itemBases) => this.itemBases = itemBases;

        public void RegisterChunkLoader(ChunkLoader chunkLoader)
        {
            chunkLoader.ChunkLoaded += OnChunkLoaded;
            chunkLoader.ChunkUnloading += OnChunkUnloading;
        }

        /// <summary>
        /// If a new chunk was loaded (and the chunk contains acutal data) this chunk
        /// will be embedded into a new dynamic chunk. After this, for all relevant layers,
        /// there tiles and items will be loaded and set to be displayed. For all elements
        /// zDepth will be calculated relative to the current reference layer.
        /// </summary>
        /// <param name="sender">the object that informed about the event</param>
        /// <param name="chunk">the new chunk that was loaded</param>
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

                    if (!chunk.Items.TryGetValue(tilePosition, out var items)) continue;
                    
                    LoadItemStack(items, new Vector3((xPos-yPos) * (38f/76f) - (1f/76f),
                        (xPos+yPos) * (19f/76f) + 0.25f, zDepth - ((y-x) / 20000f) - 0.3f),
                        tilePosition, dynamicChunk);
                }
            }
        }

        /// <summary>
        /// Loads an array of items to be displayed.
        /// Will apply offset, initialize animated or variant items,
        /// scale and height-level. Loaded items will be in the order of the array.
        /// </summary>
        /// <param name="items">The array of items</param>
        /// <param name="screenPosition">The origin screenposition to display the items</param>
        /// <param name="tilePosition">The corresponding tileposition to the screenpostion</param>
        /// <param name="chunk">The dynamic chunk in charge of the item sprites</param>
        private void LoadItemStack(MapObject[] items, Vector3 screenPosition, Vector3i tilePosition, DynamicChunk chunk)
        {
            foreach (var item in items)
            {
                var unitySprite = spritePool.Get();
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

                    var spriteId = SetupMultiFrameItem(variantBase, tilePosition.x, tilePosition.y, unitySprite);

                    sprite = sprites[spriteId];
                    offset = variantBase.GetOffset(spriteId);
                }

                unitySprite.sprite = sprite;

                unitySprite.transform.position = new Vector3(
                    screenPosition.x - sprite.bounds.extents.x + offset[0],
                    screenPosition.y + offset[1] + chunk.GetHeightLevel(tilePosition),
                    screenPosition.z
                );
                
                SetupItemScale(tilePosition.x, tilePosition.y, itemBase.SizeVariance, unitySprite);

                unitySprite.gameObject.SetActive(true);

                if (itemBase.Height > 0f) chunk.IncreaseHeightLevel(tilePosition, itemBase.Height);
                chunk.RegisterItem(unitySprite);

                screenPosition.z -= 0.000001f;
            }
        }

        /// <summary>
        /// Changes item scale if item has size variance
        /// </summary>
        /// <param name="tileX">X Position of the item</param>
        /// <param name="tileY">Y Position of the item</param>
        /// <param name="sizeVariance">Allowed size variance of the item</param>
        /// <param name="unitySprite">Sprite which size will be changed</param>
        private void SetupItemScale(int tileX, int tileY, float sizeVariance, SpriteRenderer unitySprite)
        {
            if (sizeVariance == 0f) return;

            float scale = MapVariance.GetItemScaleVariance(tileX, tileY, sizeVariance);
            unitySprite.transform.localScale = new Vector3(scale, scale, 1f);  
        }

        /// <summary>
        /// Gets item a random frame id if item is a multiframe item
        /// Registers an animation runnter for the item, if it is animated
        /// </summary>
        /// <param name="variantBase">The base item of the specific item</param>
        /// <param name="tileX">X Position of the item</param>
        /// <param name="tileY">Y Position of the item</param>
        /// <param name="unitySprite">Sprite which is animated or multiframed</param>
        /// <returns>Current frame id to be used by the sprite</returns>
        private int SetupMultiFrameItem(VariantObjectBase variantBase, int tileX, int tileY, SpriteRenderer unitySprite) 
        {
            if (!variantBase.IsAnimated) return variantBase.GetFrameId(MapVariance.GetItemFrameVariance(tileX, tileY, variantBase.FrameCount));
            
            if (!animationRunners.TryGetValue(variantBase, out var animationRunner)) 
            {
                animationRunner = new AnimationRunner(variantBase, variantBase.InitialId);
                animationRunners.Add(variantBase, animationRunner);
            }

            animationRunner.RegisterAnimatedSprite(unitySprite);

            return variantBase.InitialId;
        }

        // TODO: Remove dynamic chunk aswell, release loaded sprites as well
        /// <summary>
        /// If a chunk unloads (and the chunk contains data) all tiles will be cleared,
        /// chunk data will be unloaded and removed
        /// </summary>
        /// <param name="sender">the object that fired the event</param>
        /// <param name="chunk">the chunk that will be unloaded</param>
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

        /// <summary>
        /// Gets all relevant layers for the current rendering.
        /// A layer is relevant that is visible (referenceLayer +- Constants.Map.VisibleLayers) and
        /// that is at least contained in one of the currently loaded chunks
        /// </summary>
        /// <param name="chunkLayers">Ordered layers available in the current chunks</param>
        /// <returns></returns>
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