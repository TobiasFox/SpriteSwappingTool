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

using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CameraDistance : MonoBehaviour
    {
        public Camera camera;
        public SpriteRenderer ownRenderer;
        public bool isDisplayingDistanceToCamera = true;
        public Vector3 offset;
        public Color color= Color.blue;
        public Color textColor=Color.white;
        public int fontSize = 0;
        
        private void OnDrawGizmos()
        {
            if (!isDisplayingDistanceToCamera || camera == null)
            {
                return;
            }

            if (ownRenderer == null)
            {
                ownRenderer = GetComponent<SpriteRenderer>();
            }

            Handles.BeginGUI();
            Handles.color = color;
            var style = new GUIStyle {normal = {textColor = textColor}, fontSize = fontSize};
            
            if (camera.orthographic)
            {
                var positionOnCameraPlane =
                    new Vector3(transform.position.x, transform.position.y, camera.transform.position.z);
                var distance = transform.position - positionOnCameraPlane;
                var dist = distance.magnitude;
                distance = distance * -0.5f + transform.position + offset;

                Handles.Label(distance, dist.ToString("#.00"), style);
                Handles.DrawLine(transform.position, positionOnCameraPlane);
                Handles.DrawLine(transform.position, new Vector3(positionOnCameraPlane.x, positionOnCameraPlane.y, positionOnCameraPlane.z+0.005f));
            }
            else
            {
                var distance = transform.position - camera.transform.position;
                var dist = distance.magnitude;
                distance = distance * -0.5f + transform.position + offset;
                Handles.Label(distance, dist.ToString("#.00"), style);
                Handles.DrawLine(transform.position, camera.transform.position);
                Handles.DrawLine(transform.position, new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z+0.005f));
            }

            Handles.EndGUI();
        }
    }
}