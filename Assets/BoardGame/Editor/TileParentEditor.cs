using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BoardGame.Editor {
    [CustomEditor(typeof(Tile)), CanEditMultipleObjects]
    public class TileParentEditor : UnityEditor.Editor {
        private void OnEnable() {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable() {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo() {
            var board = FindObjectOfType<Board>();
            if (board != null) {
                board.SetupBoard();
                foreach (var tile in board.Tiles) {
                    tile.OnSetup(board);
                }
            }
        }

        public override void OnInspectorGUI() {
            if (targets == null || targets.Length == 0) return;

            var tiles = new List<TileParent>();
            foreach (var t in targets) {
                if (t is TileParent tile) {
                    tiles.Add(tile);
                } else {
                    Debug.LogError($"Unexpected target type {t.GetType().Name}.");
                    return;
                }
            }
            
            EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Coordinate");
            EditorGUI.BeginDisabledGroup(true);
            var uniqueCoordinates = tiles.Select(s => s.Coordinate).Distinct().ToList();
            var coordsText = string.Join(", ", uniqueCoordinates.Select(c => $"({c.x},{c.y})"));
            EditorGUILayout.TextField(coordsText);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);
            if (EditModifiers(tiles)) {
                var board = GameObject.FindObjectOfType<Board>();
                if (board != null) {
                    Undo.RegisterCompleteObjectUndo(board, "Edit modifiers");
                    board.SetupBoard();
                    EditorUtility.SetDirty(board);
                    
                    foreach (var tile in board.Tiles) {
                        Undo.RegisterCompleteObjectUndo(tile, "Edit modifiers");
                        tile.OnSetup(board);
                        EditorUtility.SetDirty(tile);
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Inherited", EditorStyles.boldLabel);
            base.OnInspectorGUI();
        }
        
        private bool EditModifiers(List<TileParent> tiles) {
            var hasBoardChanged = false;
            EditModifierBlocked(tiles, ref hasBoardChanged);
            EditModifierObstacle(tiles, ref hasBoardChanged);
            EditModifierPortal(tiles, ref hasBoardChanged);
            EditModifierCheckpoint(tiles, ref hasBoardChanged);
            EditModifierStartPoint(tiles, ref hasBoardChanged);
            return hasBoardChanged;
        }

        private void ReadBool(List<TileParent> tiles,
                Func<TileParent, bool> getter, out bool allAreTrue, out bool ambiguous) {
            allAreTrue = tiles.All(getter);
            var allAreFalse = !tiles.Any(getter);
            ambiguous = !(allAreTrue || allAreFalse);
            if (ambiguous) EditorGUI.showMixedValue = true;
        }

        private bool DrawToggle(string label, bool oldValue) {
            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            var newValue = EditorGUILayout.Toggle(label, oldValue);
            EditorGUIUtility.labelWidth = oldWidth;
            return newValue;
        }

        private int DrawInt(string label, int oldValue) {
            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            var newValue = EditorGUILayout.IntField(label, oldValue);
            EditorGUIUtility.labelWidth = oldWidth;
            return newValue;
        }
        
        private Vector2Int DrawVec2Int(string label, Vector2Int oldValue) {
            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            var newValue = EditorGUILayout.Vector2IntField(label, oldValue);
            EditorGUIUtility.labelWidth = oldWidth;
            return newValue;
        }

        private void EditModifierBlocked(List<TileParent> tiles, ref bool hasBoardChanged) {
            ReadBool(tiles, s => s.IsBlocked, out var allTrue, out _);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            var blocked = DrawToggle("Blocked", allTrue);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RegisterCompleteObjectUndo(
                    tiles.Select(s => s as UnityEngine.Object).ToArray(), 
                    "Set blocked");
                    
                foreach (var square in tiles) {
                    square.SetBlocked(blocked);
                    EditorUtility.SetDirty(square);
                }
                
                hasBoardChanged = true;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUI.showMixedValue = false;
        }

        private void EditModifierObstacle(List<TileParent> tiles, ref bool hasBoardChanged) {
            ReadBool(tiles, s => s.IsObstacle(out _), out var allTrue, out var ambiguous);
            
            EditorGUI.BeginChangeCheck();
            
            var obstacle = DrawToggle("Obstacle", allTrue);
            if (obstacle) {
                var oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                
                const int minPenalty = 2;
                var penalties = tiles
                    .Where(s => s.IsObstacle(out _))
                    .Select(s => {
                        s.IsObstacle(out var penalty);
                        return penalty;
                    })
                    .Where(cost => cost >= minPenalty)
                    .Distinct()
                    .DefaultIfEmpty(minPenalty)
                    .ToArray();
                
                EditorGUI.showMixedValue = ambiguous || penalties.Length > 1;
                var newPenalty = DrawInt("Penalty", penalties[0]);
                
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(
                        tiles.Select(s => s as UnityEngine.Object).ToArray(), 
                        "Add obstacle");
                    
                    foreach (var tile in tiles) {
                        tile.AddObstacle(newPenalty);
                        EditorUtility.SetDirty(tile);
                    }

                    hasBoardChanged = true;
                }

                EditorGUI.indentLevel = oldIndent;
            } else if (EditorGUI.EndChangeCheck()) {
                Undo.RegisterCompleteObjectUndo(
                    tiles.Select(s => s as UnityEngine.Object).ToArray(), 
                    "Remove obstacle");
                    
                foreach (var tile in tiles) {
                    tile.RemoveObstacle();
                    EditorUtility.SetDirty(tile);
                }
                
                hasBoardChanged = true;
            }
            
            //EditorGUILayout.EndHorizontal();
            EditorGUI.showMixedValue = false;
        }

        private void EditModifierPortal(List<TileParent> tiles, ref bool hasBoardChanged) {
            ReadBool(tiles, s => s.IsPortal(out _), out var allTrue, out var ambiguous);
            
            EditorGUI.BeginChangeCheck();
            
            Vector2Int destination = Vector2Int.zero;
            var portal = DrawToggle("Portal", allTrue);
            if (portal) {
                var oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                
                var currentDestinations = 
                    tiles.Where(s => s.IsPortal(out _))
                        .Select(s => {
                            s.IsPortal(out var dest);
                            return dest;
                        })
                        .Distinct()
                        .ToArray();

                if (currentDestinations.Length == 0) {
                    currentDestinations = new[] { Vector2Int.zero };
                }
                
                EditorGUI.showMixedValue = ambiguous || currentDestinations.Length > 1;
                destination = DrawVec2Int("Destination", currentDestinations[0]);
                EnsureInside(ref destination);

                EditorGUI.indentLevel = oldIndent;
            }
            
            if (EditorGUI.EndChangeCheck()) {
                if (portal) {
                    Undo.RegisterCompleteObjectUndo(
                        tiles.Select(s => s as UnityEngine.Object).ToArray(), 
                        "Add portal");

                    foreach (var tile in tiles) {
                        tile.AddPortal(destination);
                        EditorUtility.SetDirty(tile);
                    }
                } else {
                    Undo.RegisterCompleteObjectUndo(
                        tiles.Select(s => s as UnityEngine.Object).ToArray(), 
                        "Remove portal");
                    
                    foreach (var tile in tiles) {
                        tile.RemovePortal();
                        EditorUtility.SetDirty(tile);
                    }
                }

                hasBoardChanged = true;
            }
            
            EditorGUI.showMixedValue = false;
            
            void EnsureInside(ref Vector2Int coordinate) {
                var board = GameObject.FindObjectOfType<BoardParent>();
                if (board != null) {
                    if (coordinate.x >= board.BoardSize.x) coordinate.x = board.BoardSize.x - 1;
                    if (coordinate.y >= board.BoardSize.y) coordinate.y = board.BoardSize.y - 1;
                }
                
                if (coordinate.x < 0) coordinate.x = 0;
                if (coordinate.y < 0) coordinate.y = 0;
            }
        }

        private void EditModifierCheckpoint(List<TileParent> tiles, ref bool hasBoardChanged) {
            ReadBool(tiles, s => s.IsCheckPoint, out var allTrue, out _);
            
            EditorGUI.BeginChangeCheck();
            var checkpoint = DrawToggle("Check Point", allTrue);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RegisterCompleteObjectUndo(
                    tiles.Select(s => s as UnityEngine.Object).ToArray(), 
                    "Edit 'checkpoint'");
                    
                foreach (var square in tiles) {
                    square.SetCheckPoint(checkpoint);
                    EditorUtility.SetDirty(square);
                }

                hasBoardChanged = true;
            }

            EditorGUI.showMixedValue = false;
        }
        
        private void EditModifierStartPoint(List<TileParent> tiles, ref bool hasBoardChanged) {
            ReadBool(tiles, s => s.IsStartPoint, out var allTrue, out _);
            
            EditorGUI.BeginChangeCheck();
            var startPoint = DrawToggle("Start Point", allTrue);
            if (EditorGUI.EndChangeCheck()) {
                if (startPoint) {
                    var board = GameObject.FindObjectOfType<BoardParent>();
                    var allTiles = board.GetComponentsInChildren<TileParent>()
                        .Where(s => s.IsStartPoint).ToList();

                    var lastTile = tiles[tiles.Count - 1];
                    if (allTiles.Count > 0) {
                        allTiles.Add(lastTile);
                        Undo.RegisterCompleteObjectUndo(
                            allTiles.Select(s => s as UnityEngine.Object).ToArray(), 
                            "Move 'start point'");
                    
                        foreach (var tile in allTiles) {
                            tile.SetStartPoint(tile == lastTile);
                            EditorUtility.SetDirty(tile);
                        }
                    } else {
                        Undo.RegisterCompleteObjectUndo(lastTile, "Set 'start point'");
                        lastTile.SetStartPoint(true);
                        EditorUtility.SetDirty(lastTile);
                    }
                } else {
                    Undo.RegisterCompleteObjectUndo(
                        tiles.Select(s => s as UnityEngine.Object).ToArray(), 
                        "Remove 'start point'");
                    foreach (var tile in tiles) {
                        tile.SetStartPoint(false);
                        EditorUtility.SetDirty(tile);
                    }
                }

                hasBoardChanged = true;
            }

            EditorGUI.showMixedValue = false;
        }
    }
}