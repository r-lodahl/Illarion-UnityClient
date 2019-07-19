using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Illarion.Client.Common;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Illarion.Client.Update
{
    public class TileTableReader {

        private Dictionary<string, int> tileNameToIndex;
        
        public TileTableReader(Dictionary<string, int> tileNameToIndex)
        {
            this.tileNameToIndex = tileNameToIndex;
        }

        public Dictionary<int,int[]> CreateTileMapping()
        {
            return CreateTileMapping(
                Constants.Update.TileTablePath,
                Constants.Update.TileNameColumn,
                Constants.Update.TileIdColumn,
                Constants.Update.TileFileName
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
            return CreateTileMapping(
                Constants.Update.OverlayTablePath,
                Constants.Update.OverlayNameColumn,
                Constants.Update.OverlayIdColumn,
                Constants.Update.OverlayFileName
            );
        }

        /* Using the provided Tileset this function will create 
        * a mapping Dictionary from the Server Table Tile Ids
        * to the Tileset Tile Ids. 
        *
        * This mapping will be return and saved to disk
        */
        private Dictionary<int,int[]> CreateTileMapping(string tablePath, int nameColumn, int idColumn, string fileName) {
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
                string tileName = rowValues[nameColumn].Substring(1, rowValues[nameColumn].Length-2);

                int[] localIds = new int[1];
                int localId = LocalIdFromTileName(tileName);

                if (localId == -1)
                {
                    int variantId = 0;
                    List<int> ids = new List<int>();
                    localId = LocalIdFromTileName(tileName+"-"+variantId);
                    while(localId != -1) {
                        ids.Add(localId);
                        variantId++;
                        localId = LocalIdFromTileName(tileName+"-"+variantId);
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
            FileInfo tileFileInfo = new FileInfo(String.Concat(Game.FileSystem.UserDirectory, fileName));

            using (var file = tileFileInfo.Create())
            {
                binaryFormatter.Serialize(file, resultDic);
                file.Flush();
            }

            return resultDic;
        }

        private int LocalIdFromTileName(string tileName)
        {
            if (!tileNameToIndex.ContainsKey(tileName)) return -1;
            else return tileNameToIndex[tileName];
        }

    }
}
