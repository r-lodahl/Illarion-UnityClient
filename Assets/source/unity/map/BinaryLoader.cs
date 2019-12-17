using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Illarion.Client.Common;
using Illarion.Client.Map;

namespace Illarion.Client.Unity.Map
{
    /// <summary>
    /// Helper class to deserialize binary data during game time
    /// </summary>
    public class BinaryLoader
    {
        public Dictionary<int, MapObjectBase> LoadItemBaseDicitionary()
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo fileInfo = new FileInfo(string.Concat(Game.FileSystem.UserDirectory, Constants.Update.ItemBaseFileName));

            Dictionary<int, MapObjectBase> itemBaseDictionary = new Dictionary<int, MapObjectBase>();

            using (var file = fileInfo.OpenRead())
            {
                itemBaseDictionary = (Dictionary<int,MapObjectBase>) binaryFormatter.Deserialize(file);
                file.Flush();
            }

            return itemBaseDictionary;
        }
    }
}