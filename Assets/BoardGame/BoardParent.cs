using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BoardGame {
    [ExecuteInEditMode]
    public abstract class BoardParent : MonoBehaviour {
        
        [SerializeField, HideInInspector] private Vector2Int boardSize;
        [SerializeField, HideInInspector] private GameObject tilePrefab;
        
        private readonly Dictionary<Vector2Int, Tile> tilesByCoordinate;

        protected BoardParent() {
            tilesByCoordinate = new Dictionary<Vector2Int, Tile>();
        }
        
        /// <summary>
        /// The number of tiles horizontally and vertically on the board.
        /// </summary>
        public Vector2Int BoardSize => boardSize;

        /// <summary>
        /// The prefab used to represent a single tile.
        /// </summary>
        public GameObject TilePrefab => tilePrefab;

        /// <summary>
        /// Enumeration of all the tiles on the board in no particular order.
        /// </summary>
        public IEnumerable<Tile> Tiles => tilesByCoordinate.Values;

        /// <summary>
        /// Returns true if a square exists with the specified coordinate, giving
        /// access to the square through the output parameter.
        /// </summary>
        /// <param name="coordinate">the (discrete) coordinate to check</param>
        /// <param name="square">the square if found, otherwise unspecified</param>
        /// <returns>true if found, otherwise false</returns>
        public bool TryGetTile(Vector2Int coordinate, out Tile square) {
            return tilesByCoordinate.TryGetValue(coordinate, out square);
        }

        /// <summary>
        /// This function is called once when all tiles has been generated or
        /// when something has changed in one of the tiles.
        /// </summary>
        public abstract void SetupBoard();

        /// <summary>
        /// Clears all children to this game object and generates new ones up to
        /// the specified size.
        /// </summary>  
        public void RegenerateBoard(GameObject prefab, Vector2Int size, bool undo = false) {
            if (undo) Undo.RecordObject(this, "Regenerate board");
            
            tilePrefab = prefab;
            boardSize = size;
            
            if (tilePrefab == null) {
                Debug.LogError("Can't regenerate board since the 'Square Prefab' property is set to null.");
                return;
            }
            
            var existingChildren = new List<Transform>();
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                existingChildren.Add(child);
            }
            
            var existingTilesByName = new Dictionary<string, TileParent>();
            foreach (var tile in GetComponentsInChildren<TileParent>()) {
                if (!existingTilesByName.ContainsKey(tile.name)) {
                    existingTilesByName.Add(tile.name, tile);
                }
            }

            tilesByCoordinate.Clear();
            for (var i = 0; i < boardSize.x; i++) {
                for (var j = 0; j < boardSize.y; j++) {
                    var key = $"Tile ({i},{j})";
                    var obj = Instantiate(tilePrefab, transform);
                    #if UNITY_EDITOR
                    if (undo) Undo.RegisterCreatedObjectUndo(obj, "Created tile");
                    #endif
                    obj.name = key;
                    obj.transform.localPosition = new Vector3(i, 0, j);
                    if (!obj.TryGetComponent<Tile>(out var generatedTile)) {
                        #if UNITY_EDITOR
                        if (Application.isEditor) {
                            generatedTile = ObjectFactory.AddComponent<Tile>(obj);
                        } else {
                        #endif
                            generatedTile = obj.AddComponent<Tile>();
                        #if UNITY_EDITOR
                        }
                        #endif
                    }
                    
                    #if UNITY_EDITOR
                    if (undo) Undo.RecordObject(generatedTile, "Set tile coordinate");
                    #endif
                    
                    var coord = new Vector2Int(i, j);
                    if (existingTilesByName.TryGetValue(key, out var existingSquare)) {
                        generatedTile.CopyFrom(existingSquare);
                    }
                    
                    generatedTile.coordinate = new Vector2Int(i, j);
                    tilesByCoordinate.Add(coord, generatedTile);
                    
                    #if UNITY_EDITOR
                    if (Application.isEditor) {
                        EditorUtility.SetDirty(obj);
                    }
                    #endif
                }
            }

            foreach (var child in existingChildren) {
                #if UNITY_EDITOR
                if (Application.isEditor) {
                    Undo.DestroyObjectImmediate(child.gameObject);
                } else {
                #endif
                    Destroy(child.gameObject);
                #if UNITY_EDITOR
                }
                #endif
            }

            SetupBoard();
            
            foreach (var tile in Tiles) {
                tile.OnSetup((Board) this);
            }
        }
        
        #if UNITY_EDITOR
        private void OnEnable() {
        #else
        private void Start() {
        #endif
            RegenerateBoard(tilePrefab, boardSize);
        }

        private void Update() {
            foreach (var tile in Tiles) {
                tile.OnUpdate((Board) this);
            }
        }
    }
}