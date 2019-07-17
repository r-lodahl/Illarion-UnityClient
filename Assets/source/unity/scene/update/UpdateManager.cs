using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Illarion.Client.Common;
using Illarion.Client.Unity.Common;
using Illarion.Client.Update;

using Logger = Illarion.Client.Unity.Common.Logger;


namespace Illarion.Client.Unity.Scene.Update
{
    public class UpdateManager : MonoBehaviour
    {
        private ClientUpdater updater;
        
        private async void Awake()
        {
            Game.Initialize(
                new FileSystem(Application.persistentDataPath),
                new Logger(),
                new Config()
            );

            var updater = new ClientUpdater();

            bool mapUpdateSuccess = await MapUpdate(1, 3);

            if (mapUpdateSuccess)
            {
                await SceneManager.LoadSceneAsync(Constants.Scene.Map, LoadSceneMode.Single);
            }
            else
            {
                // Change GUI
            }
        }

        private async Task<bool> MapUpdate(int attempt, int maxAttempt)
        {
            if (await updater.Update()) return true;
            if (attempt == maxAttempt) return false;
            return await MapUpdate(attempt + 1, maxAttempt);
        }
    }
}