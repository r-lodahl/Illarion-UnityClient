using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Illarion.Client.Common;
using Illarion.Client.Unity.Common;
using Illarion.Client.Update;

using Logger = Illarion.Client.Unity.Common.Logger;


namespace Illarion.Client.Unity.Scene.Update
{
    /// <summary>
    /// Called by Unity
    /// 
    /// Initializes the Game singleton
    /// Triggers the updating
    /// Opens the map scene
    /// </summary>
    public class UpdateManager : MonoBehaviour
    {
        private ClientUpdater updater;
        
        private async void Awake()
        {
            QualitySettings.vSyncCount = 1;

            Game.Initialize(
                new FileSystem(Application.persistentDataPath),
                new Logger(),
                new Config()
            );

            updater = new ClientUpdater();

            bool mapUpdateSuccess = await MapUpdate(1, 3);

            if (mapUpdateSuccess)
            {
                await SceneManager.LoadSceneAsync(Constants.Scene.Map, LoadSceneMode.Single);
            }
            else
            {
                // Change GUI
                Debug.Log("Fail");
            }
        }

        /// <summary>
        /// Basic async task for updating the game
        /// The function will retry if it fails
        /// </summary>
        /// <param name="attempt">The current number of attempts to update the game</param>
        /// <param name="maxAttempt">The maximal allowed number of attempts to update the game</param>
        /// <returns>true if the game was succesfully updated, false otherwise</returns>
        private async Task<bool> MapUpdate(int attempt, int maxAttempt)
        {
            if (await updater.Update()) return true;
            if (attempt == maxAttempt) return false;
            return await MapUpdate(attempt + 1, maxAttempt);
        }
    }
}