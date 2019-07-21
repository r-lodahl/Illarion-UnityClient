using System;
using System.Collections.Generic;
using System.IO;
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

        public void CreateItemBaseFile(Dictionary<int, int[]> itemServerIdToLocalIds)
        {
            var tableFile = Resources.Load<TextAsset>(Constants.Update.ItemTablePath);
            if (tableFile == null) throw new FileNotFoundException($"Failed opening intern tile table at {Constants.Update.ItemTablePath}!");

            Dictionary<int, MapObjectBase> localIdToItemBase = new Dictionary<int, MapObjectBase>(itemServerIdToLocalIds.Count);

            using (var lineReader = new StringReader(tableFile.text))
            {
                string line;
                while ((line = lineReader.ReadLine()) != null)
                {
                    if (line.Equals("")) break;
                    if (line.StartsWith("#") || line.StartsWith("/")) continue;

                    string[] rowValues = line.Split(new char[] {','}, StringSplitOptions.None);

                    int serverId = int.Parse(rowValues[Constants.Update.ItemIdColumn]);

                    // One Unity unit is not measured in pixels but in tileSizeX -> 1 unit = TileSizeX; 1px = 1/TileSizeX;
                    float offsetX = int.Parse(rowValues[Constants.Update.ItemOffsetXColumn]) / (float)Constants.Tile.SizeX; 
                    float offsetY = int.Parse(rowValues[Constants.Update.ItemOffsetYColumn]) / (float)Constants.Tile.SizeX;

                    var mapObjectBase = new MapObjectBase(offsetX, offsetY);

                    int[] localIds = itemServerIdToLocalIds[serverId];

                    foreach (var localId in localIds)
                    {
                        localIdToItemBase.Add(localId, mapObjectBase);
                    }
                }
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo fileInfo = new FileInfo(String.Concat(Game.FileSystem.UserDirectory, Constants.Update.ItemBaseFileName));

            using (var file = fileInfo.Create())
            {
                binaryFormatter.Serialize(file, localIdToItemBase);
                file.Flush();
            }
        }

        /* Using the provided Tileset this function will create 
        * a mapping Dictionary from the Server Table Tile Ids
        * to the Tileset Tile Ids. 
        *
        * This mapping will be return and saved to disk
        */
        private Dictionary<int,int[]> CreateMapping(string tablePath, int nameColumn, int idColumn, string fileName, Dictionary<string, int> nameToIndex) {
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
                string name = Path.GetFileName(rowValues[nameColumn].Substring(1, rowValues[nameColumn].Length - 2));

                int[] localIds;
                int localId = LocalIdFromName(name, nameToIndex);

                if (localId == -1)
                {
                    int variantId = 0;
                    List<int> ids = new List<int>();
                    localId = LocalIdFromName(name + "-" + variantId, nameToIndex);
                    
                    while (localId != -1) {
                        ids.Add(localId);
                        variantId++;
                        localId = LocalIdFromName(name + "-" + variantId, nameToIndex);
                    }

                    localIds = ids.ToArray();
                }
                else 
                {
                    localIds = new int[] {localId};
                }

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

        private int LocalIdFromName(string name, Dictionary<string, int> nameToIndex)
        {
            if (!nameToIndex.ContainsKey(name)) return -1;
            else return nameToIndex[name];
        }

    }
}
