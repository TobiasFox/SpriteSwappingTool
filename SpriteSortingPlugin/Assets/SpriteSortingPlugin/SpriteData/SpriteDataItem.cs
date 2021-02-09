#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System;
using SpriteSortingPlugin.OOBB;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SpriteDataItem
    {
        [SerializeField, HideInInspector] private string assetGuid;
        [SerializeField] private string assetName;

        public ObjectOrientedBoundingBox objectOrientedBoundingBox;
        public Vector2[] outlinePoints;
        public SpriteAnalysisData spriteAnalysisData;

        public string AssetGuid => assetGuid;
        public string AssetName => assetName;

        public SpriteDataItem(string assetGuid, string assetName)
        {
            this.assetGuid = assetGuid;
            this.assetName = assetName;
        }

        public bool IsValidOOBB()
        {
            return objectOrientedBoundingBox != null && objectOrientedBoundingBox.IsInitialized;
        }

        public bool IsValidOutline()
        {
            return outlinePoints != null && outlinePoints.Length >= 2;
        }

        //https://answers.unity.com/questions/684909/how-to-calculate-the-surface-area-of-a-irregular-p.html
        public float CalculatePolygonArea(Transform polygonTransform)
        {
            float area = 0;
            if (!IsValidOutline())
            {
                return area;
            }

            for (var i = 0; i < outlinePoints.Length; i++)
            {
                var point1 = polygonTransform.TransformPoint(outlinePoints[i]);
                var point2 = polygonTransform.TransformPoint(outlinePoints[(i + 1) % outlinePoints.Length]);

                var mulA = point1.x * point2.y;
                var mulB = point2.x * point1.y;
                area += (mulA - mulB);
            }

            area *= 0.5f;
            return Mathf.Abs(area);
        }
    }
}