using UnityEngine;
using System.Collections.Generic;

namespace Illarion.Client.Unity.Map
{
    public class AnimationRunner
    {
        private int speed = 0;

        private float currentTime;
        private int currentIndex;

        Sprite[] sprites = null;
        List<SpriteRenderer> registeredSprites;

        public AnimationRunner()
        {
            registeredSprites = new List<SpriteRenderer>();
            
        }

        public void RegisterAnimatedSprite(SpriteRenderer sprite)
        {
            registeredSprites.Add(sprite);
        }

        private void AnimationTick(float deltaTime)
        {
            currentTime += deltaTime;

            if (currentTime < speed) return;
            
            currentTime = 0f;

            currentIndex++;
            if (currentIndex == sprites.Length) currentIndex = 0;

            foreach(var renderer in registeredSprites) renderer.sprite = sprites[currentIndex];
        }
    }
}