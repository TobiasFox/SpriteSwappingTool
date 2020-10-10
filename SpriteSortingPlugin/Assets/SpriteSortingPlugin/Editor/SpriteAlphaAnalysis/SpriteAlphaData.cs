using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAlphaAnalysis
{
    [CreateAssetMenu(fileName = "Data")]
    public class SpriteAlphaData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<ObjectOrientedBoundingBox> oobbList = new List<ObjectOrientedBoundingBox>();

        [NonSerialized] public Dictionary<string, ObjectOrientedBoundingBox> objectOrientedBoundingBoxDictionary =
            new Dictionary<string, ObjectOrientedBoundingBox>();

        public void OnBeforeSerialize()
        {
            if (objectOrientedBoundingBoxDictionary == null)
            {
                return;
            }

            oobbList = new List<ObjectOrientedBoundingBox>(objectOrientedBoundingBoxDictionary.Values);
        }

        public void OnAfterDeserialize()
        {
            if (oobbList == null)
            {
                return;
            }

            foreach (var objectOrientedBoundingBox in oobbList)
            {
                objectOrientedBoundingBoxDictionary.Add(objectOrientedBoundingBox.assetGuid, objectOrientedBoundingBox);
            }
        }
    }
}