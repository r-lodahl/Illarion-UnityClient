using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Illarion.Client.Unity.Common
{
    /// <summary>
    /// Concurrent sprite object pool
    /// </summary>
    public class SpritePool
    {
        private SpriteRenderer _spritePrefab;
        private int _minimalCapacity;

        private ConcurrentBag<SpriteRenderer> sprites;

        private Thread cleaningThread = null;

        public SpritePool(SpriteRenderer spritePrefab, int minimalCapacity)
        {
            _spritePrefab = spritePrefab;
            _minimalCapacity = minimalCapacity;

            sprites = new ConcurrentBag<SpriteRenderer>();

            for (int i = 0; i < minimalCapacity; i++)
            {
                Put(GameObject.Instantiate(spritePrefab));
            }
            
        }

        public SpriteRenderer Get() 
        {
            if (sprites.TryTake(out var sprite)) return sprite;
            return GameObject.Instantiate(_spritePrefab);
        }

        public void Put(SpriteRenderer sprite)
        {
            sprite.gameObject.SetActive(false);
            sprites.Add(sprite);

            if (sprites.Count > 2 * _minimalCapacity && cleaningThread != null && !cleaningThread.IsAlive)
            {
                cleaningThread = new Thread(CleanupBag);
                cleaningThread.Start();
            }
        }

        private void CleanupBag()
        {
            while (sprites.Count > 1.5f * _minimalCapacity)
            {
                if (sprites.TryTake(out var sprite)) GameObject.Destroy(sprite);
            }
        }
    }
}