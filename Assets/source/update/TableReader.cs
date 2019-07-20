using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Illarion.Client.Common;
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

        /* Using the provided Tileset this function will create 
        * a mapping Dictionary from the Server Table Overlay Ids
        * to the Tileset Overlay Ids. 
        *
        * This mapping will be return and saved to disk
        */
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
                string tileName = rowValues[nameColumn].Substring(1, rowValues[nameColumn].Length - 2);

                int[] localIds = new int[1];
                int localId = LocalIdFromName(tileName, nameToIndex);

                if (localId == -1)
                {
                    int variantId = 0;
                    List<int> ids = new List<int>();
                    localId = LocalIdFromName(tileName + "-" + variantId, nameToIndex);
                    
                    while (localId != -1) {
                        ids.Add(localId);
                        variantId++;
                        localId = LocalIdFromName(tileName + "-" + variantId, nameToIndex);
                    }

                    localIds = ids.ToArray();
                }
                else 
                {
                    localIds[0] = localId;
                }

                int serverId = int.Parse(rowValues[idColumn]);

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
