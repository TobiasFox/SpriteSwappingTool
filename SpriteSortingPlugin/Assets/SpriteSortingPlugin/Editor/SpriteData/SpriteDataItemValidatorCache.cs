using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SpriteDataItemValidatorCache
    {
        private static SpriteDataItemValidatorCache instance;

        private readonly Dictionary<int, SpriteDataItemValidator> validationDictionary =
            new Dictionary<int, SpriteDataItemValidator>();

        private SpriteData spriteData;

        private SpriteDataItemValidatorCache()
        {
        }

        public static SpriteDataItemValidatorCache GetInstance()
        {
            return instance ?? (instance = new SpriteDataItemValidatorCache());
        }

        public void UpdateSpriteData(SpriteData spriteData)
        {
            this.spriteData = spriteData;
        }

        public SpriteDataItemValidator GetOrCreateValidator(SpriteRenderer spriteRenderer)
        {
            var containsValidator =
                validationDictionary.TryGetValue(spriteRenderer.sprite.GetInstanceID(), out var validator);

            if (containsValidator)
            {
                return validator;
            }

            validator = new SpriteDataItemValidator();
            validator.Validate(spriteRenderer, spriteData);
            validationDictionary.Add(spriteRenderer.sprite.GetInstanceID(), validator);
            return validator;
        }

        public void Clear()
        {
            validationDictionary.Clear();
        }
    }
}