using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Illarion.Client.Common;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Illarion.Client.Update
{
    public class ClientUpdater
    {
        /// <summary>
        /// HttpClient instance handling the network communication for the update.
        /// </summary>
        private HttpClient http;

        /// <summary>
        /// This class downloads the current game data from an Illarion Update Server
        /// if the local data is outdated and converts this data to be usable by the game.
        /// </summary>
        public ClientUpdater() {
            http = new HttpClient();
            http.MaxResponseContentBufferSize = 200000000;
            http.DefaultRequestHeaders.Add("user-agent", "Unity/2019 (illarion)");
        }

        /// <summary>
        /// Updates the gamedata to the newest data available online and converts it.
        /// </summary>
        /// <returns>true if update not needed or successfull, else false</returns>
        public async Task<bool> Update() 
        {
            string localVersion = GetLocalVersion();
            string serverVersion = await GetServerVersion();
            if (serverVersion == null) return false;
            if (serverVersion.Equals(localVersion)) return true;

            ClearMapFolder();

            if (!await DownloadMapFiles()) return false;

            await new WaitForUpdate();

            var tileNameDictionary = GetTileNameDictionary();
            var itemNameDictionary = GetItemNameDictionary();
            var tableReader = new TableReader(tileNameDictionary, itemNameDictionary);

            var tileDictionary = tableReader.CreateTileMapping();
            var overlayDictionary = tableReader.CreateOverlayMapping();
                        
            var offsetReader = new OffsetReader();
            var itemOffsets = offsetReader.AdaptItemOffsets(Constants.Update.ItemOffsetPath, itemNameDictionary);
            tableReader.CreateItemBaseFile(itemNameDictionary, itemOffsets);

            await new WaitForBackgroundThread();

            var mapChunkBuilder = new MapChunkBuilder(tileDictionary, overlayDictionary);
            mapChunkBuilder.Create();

            UpdateVersion(serverVersion);

            RemoveDownloadFolder();

            return true;
        }

        /// <summary>
        /// Gets the current version of gamedata.
        /// </summary>
        /// <returns>the local game version</returns>
        private string GetLocalVersion()
        {
            string versionPath = string.Concat(
                Game.FileSystem.UserDirectory,
                Constants.UserData.VersionPath);

            if (!File.Exists(versionPath)) return "";

            return File.ReadAllText(versionPath);
        }

        /// <summary>
        /// Gets the newest online version of the gamedata.
        /// </summary>
        /// <returns>the server game version</returns>
        private async Task<string> GetServerVersion()
        {
            HttpResponseMessage response = await http.GetAsync(string.Concat(
                Constants.Update.ServerAddress, 
                Constants.Update.MapVersionEndpoint
            ));

            if (!response.IsSuccessStatusCode) return "";

            if (!response.Content.Headers.ContentType.MediaType.Equals("application/json")) return "";

            string jsonString = await response.Content.ReadAsStringAsync();
            
            var versionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

            if (versionDictionary.ContainsKey("version")) return versionDictionary["version"];

            return "";
        }

        /// <summary>
        /// Downloads the newest gamedata available online.
        /// </summary>
        /// <returns>true if the download was successfull, else false.</returns>
        private async Task<bool> DownloadMapFiles()
        {
            HttpResponseMessage response = await http.GetAsync(string.Concat(
                Constants.Update.ServerAddress,
                Constants.Update.MapDataEndpoint
            ));

            if (!response.IsSuccessStatusCode) return false;

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var unzip = new Unzip(stream))
                {
                    unzip.ExtractToDirectory(Game.FileSystem.UserDirectory);
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a dictionary that has the local tile image names as keys
        /// and their index in the sprite array as value. 
        /// </summary>
        /// <returns>The dictionary</returns>
        private Dictionary<string, int> GetTileNameDictionary() 
        {
            var tiles = Resources.LoadAll<Tile>(Constants.UserData.TilesetPath);
            return GetNameDictionary(tiles);
        }

        /// <summary>
        /// Creates a dictionary that has the local item image names as keys
        /// and their index in the sprite array as value. 
        /// </summary>
        /// <returns>The dictionary</returns>
        private Dictionary<string, int> GetItemNameDictionary()
        {
            Sprite[] items = Resources.LoadAll<Sprite>(Constants.UserData.ItemsetPath);
            return GetNameDictionary(items);
        }

        /// <summary>
        /// For a given object array, a function will be created with the object names
        /// as keys and their array index as value.
        /// </summary>
        /// <param name="objects">The named object array</param>
        /// <returns>The name to index dictionary</returns>
        private Dictionary<string, int> GetNameDictionary(UnityEngine.Object[] objects)
        {
            var nameToIndex = new Dictionary<string, int>(objects.Length);

            for (int i = 0; i < objects.Length; i++) nameToIndex.Add(objects[i].name, i);
            
            return nameToIndex;
        }

        /// <summary>
        /// Updates the game version saved in the gamedata to the given version.
        /// </summary>
        /// <param name="version">The new version</param>
        private void UpdateVersion(string version)
        {
            File.WriteAllText(String.Concat(Game.FileSystem.UserDirectory, Constants.UserData.VersionPath), version);
        }

        /// <summary>
        /// Removes the current map chunks from the gamedata.
        /// </summary>
        private void ClearMapFolder()
        {
            string mapDataPath = String.Concat(Game.FileSystem.UserDirectory, Constants.UserData.MapPath);

            if (Directory.Exists(mapDataPath)) Directory.Delete(mapDataPath, true);

            Directory.CreateDirectory(mapDataPath);
        }

        /// <summary>
        /// Removes the folder containing the unconverted map data.
        /// </summary>
        private void RemoveDownloadFolder()
        {
            Directory.Delete(String.Concat(Game.FileSystem.UserDirectory, Constants.UserData.ServerMapPath), true);
        }
    }
}