using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using Illarion.Client.Common;
using Illarion.Client.Map;

namespace Illarion.Client.Update 
{
    public class MapChunkBuilder {

        private Dictionary<int, List<RawMap>> worldMapInLayers;
        private Dictionary<int, int[]> baseIdToLocalId;
        private Dictionary<int, int[]> overlayIdToLocalId;
        private Random random;

        public MapChunkBuilder(Dictionary<int,int[]> baseIdToLocalId, Dictionary<int,int[]> overlayIdToLocalId)
        {
            this.baseIdToLocalId = baseIdToLocalId;
            this.overlayIdToLocalId = overlayIdToLocalId;
            this.worldMapInLayers = new Dictionary<int, List<RawMap>>();

            this.random = new Random();
        }

        /* Using the given Mapping Dictionaries this function will
        * split the Mapfiles provided in the Server Map File Format
        * into Binary Map Files using directly the Tileset Tile and
        * Overlay Ids. The Map Files will be equally sized Chunks of 
        * the complete Map. These Chunks are better to stream while
        * gameplay and do not need too much resources on the disk.
        *
        * This function will save each chunk to the user disk.
        */
        public void Create() 
        {
            string[] mapFiles = System.IO.Directory.GetFiles(
                String.Concat(Game.FileSystem.UserDirectory, Constants.UserData.ServerMapPath),
                "*.tiles.txt",
                SearchOption.AllDirectories);
                
            int worldMinX = int.MaxValue;
            int worldMinY = int.MaxValue;
            int worldMaxX = int.MinValue;
            int worldMaxY = int.MinValue;

            foreach (var mapFile in mapFiles)
            {
                RawMap map = LoadSingleMap(mapFile);

                if (!worldMapInLayers.ContainsKey(map.Layer)) 
                {
                    worldMapInLayers.Add(map.Layer, new List<RawMap>());
                }

                worldMapInLayers[map.Layer].Add(map);

                if (map.StartX < worldMinX) worldMinX = map.StartX;
                if (map.StartY < worldMinY) worldMinY = map.StartY;
                if (map.StartX+map.Width > worldMaxX) worldMaxX = map.StartX + map.Width;
                if (map.StartY+map.Height > worldMaxY) worldMaxY = map.StartY + map.Height;
            }

            for (int baseX = worldMinX; baseX < worldMaxX; baseX += Constants.Map.Chunksize) 
            {
                for (int baseY = worldMinY; baseY < worldMaxY; baseY += Constants.Map.Chunksize)
                {
                    CreateSingleChunk(baseX, baseY);
                }
            }
        }

        private void CreateSingleChunk(int baseX, int baseY)
        {
            List<int> usedLayers = new List<int>();
            List<RawMap> usedMaps = new List<RawMap>();
            Dictionary<Vector3i, MapObject[]> usedItems = new Dictionary<Vector3i, MapObject[]>();
            Dictionary<Vector3i, Vector3i> usedWarps = new Dictionary<Vector3i, Vector3i>();

            

            foreach (var layerMaps in worldMapInLayers) 
            {
                foreach (var singleMap in layerMaps.Value)
                {
                    bool overlaps = CheckOverlap(singleMap.StartX, singleMap.StartY,
                        singleMap.StartX + singleMap.Width, singleMap.StartY + singleMap.Height,
                        baseX, baseY, baseX + Constants.Map.Chunksize, baseY + Constants.Map.Chunksize);

                    if (overlaps)
                    {
                        if (!usedLayers.Contains(singleMap.Layer)) usedLayers.Add(singleMap.Layer);
                        usedMaps.Add(singleMap);
                    }
                }
            }

            if (usedLayers.Count == 0) return;
            
            usedLayers.Sort();

            int[][] chunkMapData = new int[Constants.Map.Chunksize*Constants.Map.Chunksize][];

            for (int ix = baseX; ix < baseX + Constants.Map.Chunksize; ix++)
            {
                for (int iy = baseY; iy < baseY + Constants.Map.Chunksize; iy++)
                {
                    List<int> tileIds = new List<int>();

                    foreach (int layer in usedLayers) 
                    {
                        int layerValue = 0;

                        foreach (RawMap map in usedMaps) 
                        {
                            if (map.Layer != layer) continue;

                            int x = ix - map.StartX;
                            int y = iy - map.StartY;

                            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) continue;

                            layerValue = map.MapArray[x,y];

                            Vector2i mapPosition = new Vector2i(x,y);

                            if (map.Items.ContainsKey(mapPosition))
                            {
								var absolutePosition = new Vector3i(ix, iy, layer);
								if (usedItems.ContainsKey(absolutePosition))
								{
                                    Game.Logger.Error("Adding an item-array to an tile already having items!");
									var joinedItems = usedItems[absolutePosition].ToList();
									joinedItems.AddRange(map.Items[mapPosition]);
									usedItems[absolutePosition] = joinedItems.ToArray();
								}
								else
								{
                                	usedItems.Add(absolutePosition, map.Items[mapPosition]);
								}
                            }

                            if (map.Warps.ContainsKey(mapPosition))
                            {
                                Vector3i warpTarget = map.Warps[mapPosition];
                                Vector3i absolutePosition = new Vector3i(ix, iy, layer);

                                if (usedWarps.ContainsKey(absolutePosition))
                                {
                                    Game.Logger.Error("Tried adding a warp to a location that already contains a warp!");
                                }
                                else 
                                {
                                    usedWarps.Add(absolutePosition, new Vector3i((int)warpTarget.x, (int)warpTarget.y, (int)warpTarget.z));
                                }
                            }
                        }

                        int[] serverTileIds = DeserializeServerIds(layerValue);

                        int baseId = GetBaseIdFromServerBaseId(serverTileIds[0]);
                        int overlayId = GetOverlayIdFromServerOverlayId(serverTileIds[1], serverTileIds[2]);

                        // Reserialize the id in a more compact way
                        layerValue = overlayId * Constants.Tile.OverlayFactor + baseId;

                        tileIds.Add(layerValue);
                    }

                    chunkMapData[(ix-baseX) * Constants.Map.Chunksize + (iy-baseY)] = tileIds.ToArray();
                }
            }

            Chunk chunk = new Chunk(chunkMapData, usedLayers.ToArray(), new int[]{baseX,baseY}, usedItems, usedWarps);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo chunkFileInfo = new FileInfo(String.Concat(Game.FileSystem.UserDirectory,"/map/chunk_",baseX/Constants.Map.Chunksize,"_",baseY/Constants.Map.Chunksize,".bin"));

            using(var file = chunkFileInfo.Create()) 
            {
                binaryFormatter.Serialize(file, chunk);
                file.Flush();
            }
        }

        private bool CheckOverlap(int topLeftX1, int topLeftY1, int bottomRightX1, int bottomRightY1, int topLeftX2, int topLeftY2, int bottomRightX2, int bottomRightY2) 
        {
            if (topLeftX1 > bottomRightX2 || topLeftX2 > bottomRightX1) return false;
            if (bottomRightY2 < topLeftY1 || bottomRightY1 < topLeftY2) return false;
            return true;
        }

        private int GetBaseIdFromServerBaseId(int serverBaseId)
        {
            if (serverBaseId == 0 || !baseIdToLocalId.ContainsKey(serverBaseId)) return 0;
            
            int[] tileVariantIds = baseIdToLocalId[serverBaseId];
            return tileVariantIds[random.Next(tileVariantIds.Length)];
        }

        private int GetOverlayIdFromServerOverlayId(int serverOverlayId, int serverOverlayShapeId)
        {
            if (serverOverlayId == 0 || !overlayIdToLocalId.ContainsKey(serverOverlayId*Constants.Tile.OverlayFactor)) return 0;
            return overlayIdToLocalId[serverOverlayId*Constants.Tile.OverlayFactor][serverOverlayShapeId-1];
        }

        private int[] DeserializeServerIds(int serializedServerIds) 
        {
            if ((serializedServerIds & Constants.Tile.ShapeIdMask) == 0) 
                return new int[]{serializedServerIds, 0, 0};
            
            return new int[]{serializedServerIds & Constants.Tile.BaseIdMask,
                (serializedServerIds & Constants.Tile.OverlayIdMask) >> 5,
                (serializedServerIds & Constants.Tile.ShapeIdMask) >> 10};
        }

        private RawMap LoadSingleMap(string mapFile)
        {
            StreamReader fileReader = new StreamReader(mapFile);
            
            string line;
            int next;
            bool read = true;
            RawMap map = new RawMap();

            while (read && (next = fileReader.Peek()) != -1) 
            {
                switch (next)
                {
                    case 'L':
                        line = fileReader.ReadLine();
                        map.Layer = int.Parse(line.Substring(3, line.Length-3));
                        break;
                    case 'X':
                        line = fileReader.ReadLine();
                        map.StartX = int.Parse(line.Substring(3, line.Length-3));
                        break;
                    case 'Y':
                        line = fileReader.ReadLine();
                        map.StartY = int.Parse(line.Substring(3, line.Length-3));
                        break;
                    case 'W':
                        line = fileReader.ReadLine();
                        map.Width = int.Parse(line.Substring(3, line.Length-3));
                        break;
                    case 'H':
                        line = fileReader.ReadLine();
                        map.Height = int.Parse(line.Substring(3, line.Length-3));
                        break;
                    case '#':
                        line = fileReader.ReadLine();
                        break;
                    case 'V':
                        line = fileReader.ReadLine();
                        break;
                    default:
                        read = false;
                        break;
                }
            }

            map.MapArray = new int[map.Width, map.Height];

            while ((line = fileReader.ReadLine()) != null) 
            {
                string[] rowValues = line.Split((new string[]{";"}), StringSplitOptions.RemoveEmptyEntries);
                map.MapArray[int.Parse(rowValues[0]), int.Parse(rowValues[1])] = int.Parse(rowValues[2]);
            }

            fileReader.Close();
            
            string itemPath = String.Concat(mapFile.Substring(0, mapFile.Length - 9), "items.txt");
            if (!File.Exists(itemPath)) throw new FileNotFoundException($"{itemPath} not found!");
            fileReader = new StreamReader(itemPath);

            Dictionary<Vector2i, List<MapObject>> itemDic = new Dictionary<Vector2i, List<MapObject>>();
            while ((line = fileReader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.Equals("")) continue;

                string[] rowValues = line.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);

                Vector2i position = new Vector2i(int.Parse(rowValues[0]), int.Parse(rowValues[1]));

                if (!itemDic.ContainsKey(position)) itemDic.Add(position, new List<MapObject>());

                MapObject item = new MapObject();
                item.ObjectId = int.Parse(rowValues[2]);

                string name = null;
                string description = null;
                if (Game.Config.Language == Language.German)
                {
                    name = rowValues.FirstOrDefault(x => x.StartsWith("nameDe"));
                    description = rowValues.FirstOrDefault(x => x.StartsWith("descriptionDe"));
                }
                else 
                {
                    name = rowValues.FirstOrDefault(x => x.StartsWith("nameEn"));
                    description = rowValues.FirstOrDefault(x => x.StartsWith("descriptionEn"));
                }

                if (name != null) item.Name = name.Substring(7);
                if (description != null) item.Description = description.Substring(14);

                itemDic[position].Add(item);
            }

            fileReader.Close();
            
            Dictionary<Vector2i, MapObject[]> arrayItemDic = new Dictionary<Vector2i, MapObject[]>(itemDic.Count);
            foreach(var item in itemDic) arrayItemDic.Add(item.Key, item.Value.ToArray());
            map.Items = arrayItemDic;    

            string warpPath = String.Concat(mapFile.Substring(0, mapFile.Length - 9), "warps.txt");
            if (!File.Exists(warpPath)) throw new FileNotFoundException($"{warpPath} not found!");
            fileReader = new StreamReader(warpPath); 

            Dictionary<Vector2i, Vector3i> warpDic = new Dictionary<Vector2i, Vector3i>();
            while ((line = fileReader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.Equals("")) continue;

                string[] rowValues = line.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);

                warpDic.Add(new Vector2i(
                        int.Parse(rowValues[0]),
                        int.Parse(rowValues[1])
                    ), new Vector3i(
                        int.Parse(rowValues[2]),
                        int.Parse(rowValues[3]),
                        int.Parse(rowValues[4])
                ));
            }

            fileReader.Close();
            map.Warps = warpDic;
            
            return map;
        }
    }
}
