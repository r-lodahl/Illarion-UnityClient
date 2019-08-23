using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

namespace Illarion.Client.Update
{
    public class OffsetReader
    {
        public Dictionary<int, int[]> AdaptItemOffsets(string offsetFilePath, Dictionary<string, int> itemNameToLocalId)
        {
            Dictionary<int, int[]> localIdToOffset = new Dictionary<int, int[]>(itemNameToLocalId.Count);

            var offsetFile = Resources.Load<TextAsset>(offsetFilePath);
            using(var reader = new StringReader(offsetFile.text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Equals("") || line.StartsWith("#")) continue;

                    var values = line.Split(new char[] {','});

                    if (itemNameToLocalId.TryGetValue(values[0], out var localId))
                    {
                        localIdToOffset.Add(localId, new int[] {int.Parse(values[2]),int.Parse(values[3]),int.Parse(values[4]),int.Parse(values[5])});
                    }
                    else 
                    {
                        Debug.LogError($"{values[0]} no id found");
                        continue;
                    }
                }
            }
            return localIdToOffset;            
        }
    }
}