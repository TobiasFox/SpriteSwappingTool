using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SpriteDataItemValidator
    {
        private bool isOOBBValid;
        private bool isOutlineValid;
        private string assetGuid;

        public string AssetGuid => assetGuid;

        public void Validate(SpriteRenderer spriteRenderer, SpriteData spriteData)
        {
            if (spriteData == null || spriteRenderer == null)
            {
                return;
            }

            assetGuid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

            var isSpriteDataItemExisting =
                spriteData.spriteDataDictionary.TryGetValue(assetGuid, out var spriteDataItem);

            if (!isSpriteDataItemExisting)
            {
                return;
            }

            isOOBBValid = spriteDataItem.IsValidOOBB();
            isOutlineValid = spriteDataItem.IsValidOutline();
        }

        public OutlinePrecision GetValidOutlinePrecision(OutlinePrecision preferredOutlinePrecision)
        {
            var returnOutlinePrecision = OutlinePrecision.AxisAlignedBoundingBox;
            switch (preferredOutlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    if (isOOBBValid)
                    {
                        returnOutlinePrecision = OutlinePrecision.ObjectOrientedBoundingBox;
                    }

                    break;
                case OutlinePrecision.PixelPerfect:
                    if (isOutlineValid)
                    {
                        returnOutlinePrecision = OutlinePrecision.PixelPerfect;
                    }

                    break;
            }

            return returnOutlinePrecision;
        }
    }
}