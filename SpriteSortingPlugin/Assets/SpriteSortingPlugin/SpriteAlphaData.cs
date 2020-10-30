using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    // [CreateAssetMenu(fileName = "Data")]
    public class SpriteAlphaData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<ObjectOrientedBoundingBox> oobbList = new List<ObjectOrientedBoundingBox>();
        [SerializeField] private List<SpriteDataItem> spriteDataList = new List<SpriteDataItem>();

        [NonSerialized]
        public Dictionary<string, SpriteDataItem> spriteDataDictionary = new Dictionary<string, SpriteDataItem>();

        [NonSerialized] public Dictionary<string, ObjectOrientedBoundingBox> objectOrientedBoundingBoxDictionary =
            new Dictionary<string, ObjectOrientedBoundingBox>();

        public void OnBeforeSerialize()
        {
            if (objectOrientedBoundingBoxDictionary != null)
            {
                oobbList = new List<ObjectOrientedBoundingBox>(objectOrientedBoundingBoxDictionary.Values);
            }

            if (spriteDataDictionary != null)
            {
                spriteDataList = new List<SpriteDataItem>(spriteDataDictionary.Values);
            }
        }

        public void OnAfterDeserialize()
        {
            if (oobbList != null)
            {
                objectOrientedBoundingBoxDictionary.Clear();

                foreach (var objectOrientedBoundingBox in oobbList)
                {
                    objectOrientedBoundingBoxDictionary.Add(objectOrientedBoundingBox.assetGuid,
                        objectOrientedBoundingBox);
                }
            }

            if (spriteDataList != null)
            {
                spriteDataDictionary.Clear();

                foreach (var spriteData in spriteDataList)
                {
                    spriteDataDictionary.Add(spriteData.assetGuid, spriteData);
                }
            }
        }
    }
}