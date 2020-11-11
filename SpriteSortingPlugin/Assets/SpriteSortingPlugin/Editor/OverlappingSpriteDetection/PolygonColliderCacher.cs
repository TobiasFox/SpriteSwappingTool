using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin.OverlappingSpriteDetection
{
    public class PolygonColliderCacher
    {
        private static PolygonColliderCacher instance;

        private Dictionary<string, PolygonCollider2D[]> spriteColliderDataDictionary =
            new Dictionary<string, PolygonCollider2D[]>();

        private PolygonColliderCacher()
        {
        }

        public static PolygonColliderCacher GetInstance()
        {
            if (instance == null)
            {
                instance = new PolygonColliderCacher();
            }

            return instance;
        }

        public PolygonCollider2D GetCachedColliderOrCreateNewCollider(string assetGuid,
            SpriteDataItem spriteDataItem, Transform transform)
        {
            var containsColliderArray =
                spriteColliderDataDictionary.TryGetValue(assetGuid, out var polygonColliderArray);
            if (!containsColliderArray)
            {
                polygonColliderArray = new PolygonCollider2D[2];

                var polygonCollider = CreateNewPolygonColliderOnNewGameObject(spriteDataItem);
                SetColliderPointsToCollider(spriteDataItem, transform, ref polygonCollider);

                polygonColliderArray[0] = polygonCollider;
                spriteColliderDataDictionary[assetGuid] = polygonColliderArray;
                return polygonCollider;
            }

            for (var i = 0; i < polygonColliderArray.Length; i++)
            {
                var polygonCollider = polygonColliderArray[i];

                if (polygonCollider == null)
                {
                    polygonCollider = CreateNewPolygonColliderOnNewGameObject(spriteDataItem);
                    SetColliderPointsToCollider(spriteDataItem, transform, ref polygonCollider);
                    polygonColliderArray[i] = polygonCollider;

                    spriteColliderDataDictionary[assetGuid] = polygonColliderArray;
                    return polygonCollider;
                }

                if (polygonCollider.enabled)
                {
                    continue;
                }

                SetColliderPointsToCollider(spriteDataItem, transform, ref polygonCollider);
                polygonCollider.enabled = true;
                return polygonCollider;
            }

            return null;
        }

        private static PolygonCollider2D CreateNewPolygonColliderOnNewGameObject(SpriteDataItem spriteDataItem)
        {
            var polyColliderGameObject = new GameObject("PolygonCollider " + spriteDataItem.AssetName);
            return polyColliderGameObject.AddComponent<PolygonCollider2D>();
        }

        private static void SetColliderPointsToCollider(SpriteDataItem spriteDataItem, Transform transform,
            ref PolygonCollider2D polygonCollider)
        {
            polygonCollider.transform.SetPositionAndRotation(transform.position, transform.rotation);
            polygonCollider.transform.localScale = transform.lossyScale;

            polygonCollider.points = spriteDataItem.outlinePoints.ToArray();
        }

        public void DisableCachedCollider(string assetGuid, int polygonColliderInstanceId)
        {
            var containsColliderArray =
                spriteColliderDataDictionary.TryGetValue(assetGuid, out var polygonColliderArray);
            if (!containsColliderArray)
            {
                return;
            }

            foreach (var polygonCollider in polygonColliderArray)
            {
                if (polygonCollider == null)
                {
                    continue;
                }

                if (polygonCollider.GetInstanceID() != polygonColliderInstanceId)
                {
                    continue;
                }

                polygonCollider.enabled = false;
                break;
            }
        }

        public void CleanUp()
        {
            foreach (var polygonColliders in spriteColliderDataDictionary.Values)
            {
                if (polygonColliders == null)
                {
                    continue;
                }

                foreach (var polygonCollider in polygonColliders)
                {
                    if (polygonCollider == null)
                    {
                        continue;
                    }

                    Object.DestroyImmediate(polygonCollider.gameObject);
                }
            }
        }
    }
}