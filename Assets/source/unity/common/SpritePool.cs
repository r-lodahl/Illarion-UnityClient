using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Illarion.Client.Unity.Common
{
    public class SpritePool
    {
        [SerializeField] private GameObject spritePrefab;
        [SerializeField] private int minimalCapacity;

        private ConcurrentBag<GameObject> sprites;

        private Thread cleaningThread = null;

        public SpritePool()
        {
            sprites = new ConcurrentBag<GameObject>();

            for (int i = 0; i < minimalCapacity; i++)
            {
                Put(GameObject.Instantiate(spritePrefab));
            }
            
        }

        public GameObject Get() 
        {
            if (sprites.TryTake(out var sprite)) return sprite;
            return GameObject.Instantiate(spritePrefab);
        }

        public void Put(GameObject sprite)
        {
            sprite.SetActive(false);
            sprites.Add(sprite);

            if (sprites.Count > 2 * minimalCapacity && cleaningThread != null && !cleaningThread.IsAlive)
            {
                cleaningThread = new Thread(CleanupBag);
                cleaningThread.Start();
            }
        }

        private void CleanupBag()
        {
            while (sprites.Count > 2 * minimalCapacity)
            {
                if (sprites.TryTake(out var sprite)) GameObject.Destroy(sprite);
            }
        }
    }
}