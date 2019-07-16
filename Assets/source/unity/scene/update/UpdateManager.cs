using UnityEngine;
using Illarion.Client.Common;
using Illarion.Client.Unity.Common;
using Illarion.Client.Update;

using Logger = Illarion.Client.Unity.Common.Logger;


namespace Illarion.Client.Unity.Scene.Update
{
    public class UpdateManager : MonoBehaviour
    {
        private async void Awake()
        {
            Game.Initialize(
                new FileSystem(Application.persistentDataPath),
                new Logger(),
                new Config()
            );

            var updater = new ClientUpdater();
            await updater.Update();
        }
    }
}