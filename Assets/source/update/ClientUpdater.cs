using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Illarion.Client.Common;
using Newtonsoft.Json;

namespace Illarion.Client.Update
{
    public class ClientUpdater
    {
        private HttpClient http;

        public ClientUpdater() {
            http = new HttpClient();
            http.MaxResponseContentBufferSize = 200000000;
            http.DefaultRequestHeaders.Add("user-agent", "Unity/2019 (illarion)");
        }

        public async Task<bool> Update() 
        {
            string localVersion = GetLocalVersion();
            string serverVersion = await GetServerVersion();
            if (serverVersion == null) return false;
            if (serverVersion.Equals(localVersion)) return true;

            ClearMapFolder();

            if (!await DownloadMapFiles()) return false;

            var tableConverter = new TileTableReader();
            var tileDictionary = tableConverter.CreateTileMapping();
            var overlayDictionary = tableConverter.CreateOverlayMapping();

            var mapChunkBuilder = new MapChunkBuilder(tileDictionary, overlayDictionary);
            mapChunkBuilder.Create();

            UpdateVersion(serverVersion);

            RemoveDownloadFolder();

            return true;
        }

        private string GetLocalVersion()
        {
            string versionPath = string.Concat(
                Game.FileSystem.UserDirectory,
                Constants.UserData.VersionPath);

            if (!File.Exists(versionPath)) return "";

            return File.ReadAllText(versionPath);
        }

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

        private void UpdateVersion(string version)
        {
            File.WriteAllText(String.Concat(Game.FileSystem.UserDirectory, Constants.UserData.VersionPath), version);
        }

        private void ClearMapFolder()
        {
            string mapDataPath = String.Concat(Game.FileSystem.UserDirectory, Constants.UserData.MapPath);

            if (Directory.Exists(mapDataPath)) Directory.Delete(mapDataPath, true);

            Directory.CreateDirectory(mapDataPath);
        }

        private void RemoveDownloadFolder()
        {
            Directory.Delete(String.Concat(Game.FileSystem.UserDirectory, Constants.UserData.ServerMapPath));
        }
    }
}