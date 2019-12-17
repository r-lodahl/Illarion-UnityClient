using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using Illarion.Client.Common;
using Illarion.Client.Map;
using UnityEngine;

namespace Illarion.Client.Update
{
    public class TableReader {

        private Dictionary<string, int> tileNameToIndex;
        private Dictionary<string, int> itemNameToIndex;
        
        public TableReader(Dictionary<string, int> tileNameToIndex, Dictionary<string, int> itemNameToIndex)
        {
            this.tileNameToIndex = tileNameToIndex;
            this.itemNameToIndex = itemNameToIndex;
        }

        public Dictionary<int,int[]> CreateItemMapping()
        {
            return CreateMapping(
                Constants.Update.ItemTablePath,
                Constants.Update.ItemNameColumn,
                Constants.Update.ItemIdColumn,
                Constants.Update.ItemFileName,
                itemNameToIndex
            );
        }

        public Dictionary<int,int[]> CreateTileMapping()
        {
            return CreateMapping(
                Constants.Update.TileTablePath,
                Constants.Update.TileNameColumn,
                Constants.Update.TileIdColumn,
                Constants.Update.TileFileName,
                tileNameToIndex
            );
        }

        public Dictionary<int,int[]> CreateOverlayMapping()
        {
            return CreateMapping(
                Constants.Update.OverlayTablePath,
                Constants.Update.OverlayNameColumn,
                Constants.Update.OverlayIdColumn,
                Constants.Update.OverlayFileName,
                tileNameToIndex
            );
        }

        /// <summary>
        /// Extracts item game data from the item server data file and transforms them into map objects.
        /// Saves the mapobjects together with their corresponding server id as a file.
        /// </summary>
        /// <param name="itemNameToLocalIds">Dictionary matching item names to local object ids</param>
        /// <param name="localIdToCorrectionOffsets">Dictionary containing local offsets for local object ids</param>
        public void CreateItemBaseFile(Dictionary<string, int> itemNameToLocalIds, Dictionary<int, int[]> localIdToCorrectionOffsets)
        {
            var tableFile = Resources.Load<TextAsset>(Constants.Update.ItemTablePath);
            if (tableFile == null) throw new FileNotFoundException($"Failed opening intern tile table at {Constants.Update.ItemTablePath}!");

            Dictionary<int, MapObjectBase> serverIdToItemBase = new Dictionary<int, MapObjectBase>();

            using (var lineReader = new StringReader(tableFile.text))
            {
                string line;
                while ((line = lineReader.ReadLine()) != null)
                {
                    if (line.Equals("")) break;
                    if (line.StartsWith("#") || line.StartsWith("/")) continue;

                    string[] rowValues = line.Split(new char[] {','}, StringSplitOptions.None);
                    
                    int serverId = int.Parse(rowValues[Constants.Update.ItemIdColumn]);
                    string itemName = FormatName(rowValues[Constants.Update.ItemNameColumn]);

                    int itemMode = int.Parse(rowValues[Constants.Update.ItemModeColumn]);
                    int itemFrameCount = int.Parse(rowValues[Constants.Update.ItemFrameCountColumn]);

                    float baseOffsetX = int.Parse(rowValues[Constants.Update.ItemOffsetXColumn]);
                    float baseOffsetY = int.Parse(rowValues[Constants.Update.ItemOffsetYColumn]);

                    float scaleVariance = int.Parse(rowValues[Constants.Update.ItemScalingColumn]) / 100f;

                    int emittedLight = int.Parse(rowValues[Constants.Update.ItemLightEmitColumn]);

                    float colorModRed = int.Parse(rowValues[Constants.Update.ItemColorModRedColumn]) / 255f;
                    float colorModGreen = int.Parse(rowValues[Constants.Update.ItemColorModGreenColumn]) / 255f;
                    float colorModBlue = int.Parse(rowValues[Constants.Update.ItemColorModBlueColumn]) / 255f;
                    float colorModAlpha = int.Parse(rowValues[Constants.Update.ItemColorModAlphaColumn]) / 255f;

                    float height = int.Parse(rowValues[Constants.Update.ItemSurfaceLevelColumn]) / (float)Constants.Tile.SizeX;

                    int[] localIds = LocalIdsFromName(itemName, itemNameToLocalIds);

                    MapObjectBase mapObject;
                    if (itemMode == (int)Constants.ItemMode.Simple)
                    {
                        mapObject = new SimpleObjectBase(
                            localIds[0], CorrectBaseOffset(baseOffsetX, baseOffsetY, localIdToCorrectionOffsets, localIds[0]),
                            colorModRed, colorModGreen, colorModBlue, colorModAlpha,
                            scaleVariance, emittedLight, height);
                    }
                    else
                    {
                        float[] offsetX = new float[itemFrameCount];
                        float[] offsetY = new float[itemFrameCount];

                        for (int i = 0; i < itemFrameCount; i++)
                        {
                            var itemVarianceName = itemName + "-" + i;
                            var correctedOffsets = CorrectBaseOffset(baseOffsetX, baseOffsetY, localIdToCorrectionOffsets, localIds[0]);
                            offsetX[i] = correctedOffsets[0];
                            offsetY[i] = correctedOffsets[1];
                        }

                        float animationSpeed = int.Parse(rowValues[Constants.Update.ItemAnimationSpeedColumn]) / 100f;
                        
                        mapObject = new VariantObjectBase(
                            localIds, offsetX, offsetY, animationSpeed,
                            colorModRed, colorModGreen, colorModBlue, colorModAlpha,
                            scaleVariance, emittedLight, height);
                    }

                    if (serverIdToItemBase.ContainsKey(serverId))
                    {
                        Debug.Log("Tried to save a single item serverId twice in the mapping file!");
                        continue;
                    }

                    serverIdToItemBase.Add(serverId, mapObject);
                }
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo fileInfo = new FileInfo(String.Concat(Game.FileSystem.UserDirectory, Constants.Update.ItemBaseFileName));

            using (var file = fileInfo.Create())
            {
                binaryFormatter.Serialize(file, serverIdToItemBase);
                file.Flush();
            }
        }

        /// <summary>
        /// Corrects the server base offset by adding the local offset on top of it
        /// </summary>
        /// <param name="baseOffsetX">the server data offset x</param>
        /// <param name="baseOffsetY">the server data offset y</param>
        /// <param name="localIdToOffsets">the offsets for the local ids</param>
        /// <param name="objectId">the local object id</param>
        /// <returns>the correct base offset</returns>
        private float[] CorrectBaseOffset(float baseOffsetX, float baseOffsetY, Dictionary<int, int[]> localIdToOffsets, int objectId) 
        {
            if (localIdToOffsets.TryGetValue(objectId, out var offsetCorrection))
            {
                return new float[] {
                    (baseOffsetX + (offsetCorrection[0] + offsetCorrection[2]) / 2.0f) / (float)Constants.Tile.SizeX,
                    (baseOffsetY + offsetCorrection[3]) / (float)Constants.Tile.SizeX
                };
            }
            else
            {
                Debug.LogError($"No offset correction found for {objectId}");
                return new float[] {0f, 0f};
            }
        }

        /// <summary>
        /// Given a server table file path and its name and id columns this function  
        /// will map the server id to the local id by using the name as matcher
        /// 
        /// This mapping will be returned and saved to disk using a given fileName
        /// </summary>
        /// <param name="tablePath">file path of the server table file</param>
        /// <param name="nameColumn">name column in the server table file</param>
        /// <param name="idColumn">id column in the server table file</param>
        /// <param name="fileName">fileName of the mapping to be saved</param>
        /// <param name="nameToLocalId">mapping of item names to their local id</param>
        /// <returns></returns>
        private Dictionary<int,int[]> CreateMapping(string tablePath, int nameColumn, int idColumn, string fileName, Dictionary<string, int> nameToLocalId) {
            var tableFile = Resources.Load<TextAsset>(tablePath);

            if (tableFile == null) throw new FileNotFoundException($"Failed opening intern tile table at {tablePath}!");
            
            Dictionary<int,int[]> resultDic = new Dictionary<int, int[]>();
            
            using(var lineReader = new StringReader(tableFile.text))
            {
            string line;
                while ((line = lineReader.ReadLine()) != null)
                {
                    if (line.Equals("")) break;
                    if (line.StartsWith("#") || line.StartsWith("/")) continue;

                    string[] rowValues = line.Split(new char[] {','}, StringSplitOptions.None);
                    string name = FormatName(rowValues[nameColumn]);

                    int[] localIds = LocalIdsFromName(name, nameToLocalId);

                    int serverId = int.Parse(rowValues[idColumn]);

                    if (localIds.Length == 0)
                    {
                        Game.Logger.Warning($"Not found any local id for server id [{serverId}]({name}) @ {tablePath}");
                        continue;
                    }

                    resultDic.Add(serverId, localIds);
                }
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo fileInfo = new FileInfo(String.Concat(Game.FileSystem.UserDirectory, fileName));

            using (var file = fileInfo.Create())
            {
                binaryFormatter.Serialize(file, resultDic);
                file.Flush();
            }

            return resultDic;
        }

        /// <summary>
        /// Returns all local ids matching a given object name
        /// 
        /// For a matching object the number of returned ids is 1,
        /// for animated or variant objects there are more than 1 matches
        /// </summary>
        /// <param name="name">the object name</param>
        /// <param name="nameToIndex">the mapping file between name and local ids</param>
        /// <returns>an array of matching local ids</returns>
        private int[] LocalIdsFromName(string name, Dictionary<string, int> nameToIndex) 
        {
            int localId = LocalIdFromName(name, nameToIndex);

            if (localId != -1) return new int[] {localId};
            
            int variantId = 0;
            List<int> ids = new List<int>();
            localId = LocalIdFromName(name + "-" + variantId, nameToIndex);
            
            while (localId != -1) {
                ids.Add(localId);
                variantId++;
                localId = LocalIdFromName(name + "-" + variantId, nameToIndex);
            }

            return ids.ToArray();
        }

        /// <summary>
        /// Returns the local id from the given matching table for a given name
        /// </summary>
        /// <param name="name">the name to be searched</param>
        /// <param name="nameToIndex">the name to id matching table</param>
        /// <returns>the local id or -1 if name not found</returns>
        private int LocalIdFromName(string name, Dictionary<string, int> nameToIndex)
        {
            if (!nameToIndex.ContainsKey(name)) return -1;
            else return nameToIndex[name];
        }

        /// <summary>
        /// Removes " from a string and replaces / with -
        /// </summary>
        /// <param name="unformattedName">the string to be formatted</param>
        /// <returns>the formatted string</returns>
        private string FormatName(string unformattedName) => unformattedName.Substring(1, unformattedName.Length - 2).Replace("/","-");
    }
}
