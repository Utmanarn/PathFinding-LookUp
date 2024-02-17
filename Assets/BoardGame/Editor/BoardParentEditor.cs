using UnityEditor;
using UnityEngine;

namespace BoardGame.Editor {
    [CustomEditor(typeof(Board))]
    public class BoardParentEditor : UnityEditor.Editor{
        public override void OnInspectorGUI() {

            var t = target as BoardParent;
            if (t == null) return;
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Board Settings", EditorStyles.boldLabel);
            var boardSize = EditorGUILayout.Vector2IntField("Size", t.BoardSize);
            if (boardSize.x < 1) boardSize.x = 1;
            if (boardSize.y < 1) boardSize.y = 1;
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Tile Settings", EditorStyles.boldLabel);
            var prefab = (GameObject) EditorGUILayout.ObjectField("Tile Prefab", t.TilePrefab, typeof(GameObject), false);
            
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(t, "Regenerate board");
                t.RegenerateBoard(prefab, boardSize);
                EditorUtility.SetDirty(t);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inherited", EditorStyles.boldLabel);
            
            base.OnInspectorGUI();
        }
    }
}