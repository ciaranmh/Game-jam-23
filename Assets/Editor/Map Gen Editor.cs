using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var mapGen = (MapGenerator) target;

            if (DrawDefaultInspector())
                if (mapGen.autoUpdate)
                    mapGen.DrawMapInEditor();

            if (GUILayout.Button("Generate"))
                mapGen.DrawMapInEditor();
        }
    }
}