using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    // [CreateAssetMenu(fileName = "Data")]
    public class SpriteData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SpriteDataItem> spriteDataList = new List<SpriteDataItem>();

        [NonSerialized]
        public Dictionary<string, SpriteDataItem> spriteDataDictionary = new Dictionary<string, SpriteDataItem>();

        public void OnBeforeSerialize()
        {
            if (spriteDataDictionary != null)
            {
                spriteDataList = new List<SpriteDataItem>(spriteDataDictionary.Values);
            }
        }

        public void OnAfterDeserialize()
        {
            if (spriteDataList != null)
            {
                spriteDataDictionary.Clear();

                foreach (var spriteData in spriteDataList)
                {
                    spriteDataDictionary.Add(spriteData.AssetGuid, spriteData);
                }
            }
        }

        public bool SpriteDataDictionaryContainsAllGuids(List<string> guidList)
        {
            if (spriteDataDictionary == null || guidList == null)
            {
                return false;
            }

            foreach (var guid in guidList)
            {
                if (!spriteDataDictionary.ContainsKey(guid))
                {
                    return false;
                }
            }

            return true;
        }
    }
}