using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpriteSwappingPlugin.Helper
{
    public class EditorPhysicSimulation : MonoBehaviour
    {

        public PolygonCollider2D polygonCollider2D;
        public PolygonCollider2D polygonCollider2D2;
    
        public void TestPhysics()
        {

            Physics2D.autoSimulation = false;
            Physics2D.Simulate(Time.fixedDeltaTime);

            var list = new List<Collider2D>();
            var contactFilter2D = new ContactFilter2D();
            Physics2D.OverlapCollider(polygonCollider2D, contactFilter2D,  list);

            int j = 0;
        
            polygonCollider2D.OverlapCollider(contactFilter2D, list);

            var isTouching= Physics2D.IsTouching(polygonCollider2D, polygonCollider2D2);
        
            int i = 0;
        
            Physics2D.autoSimulation = true;
        }
 
    }

    [CustomEditor(typeof(EditorPhysicSimulation))]
    public class EditorPhysicSimulationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Start Phyiscs amd test"))
            {
                var simulator = (EditorPhysicSimulation) target;
                simulator.TestPhysics();
            }
        }
    }
}