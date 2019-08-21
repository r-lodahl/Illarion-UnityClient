using UnityEngine;
using System.Collections.Generic;
using Illarion.Client.Map;

namespace Illarion.Client.Unity.Map
{
    public class AnimationRunner
    {
        public static Sprite[] ItemSprites {get;set;}

        private float currentTime;
        private int currentId;

        private VariantObjectBase animationInfo;
        private List<SpriteRenderer> registeredSprites;

        public AnimationRunner(VariantObjectBase animationBase, int animationId)
        {
            registeredSprites = new List<SpriteRenderer>();
            currentId = animationId;
            animationInfo = animationBase;

            Debug.Log($"Registered animation with speed of {animationInfo.AnimationSpeed}");
        }

        public void RegisterAnimatedSprite(SpriteRenderer sprite)
        {
            registeredSprites.Add(sprite);
        }

        public void Tick(float deltaTime)
        {
            currentTime += deltaTime;
            if (currentTime < animationInfo.AnimationSpeed) return;
            currentTime = 0f;

            currentId = animationInfo.GetNextId(currentId);
            var nextSprite = ItemSprites[currentId];

            foreach(var renderer in registeredSprites) renderer.sprite = nextSprite;
        }
    }
}