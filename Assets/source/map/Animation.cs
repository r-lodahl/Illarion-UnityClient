using UnityEngine;
using System.Collections.Generic;

namespace Illarion.Client.Unity.Map
{
    public class Animation
    {
        int speed;

        float currentTime;
        int currentIndex;

        Sprite[] sprites;
        List<SpriteRenderer> registeredSprites;

        public Animation()
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