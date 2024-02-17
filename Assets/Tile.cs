using System;
using BoardGame;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class Tile : TileParent
{
    [SerializeField] Dijkstra dijkstra;
    public int gValue;

    // 1. TileParent extends MonoBehavior, so you can add member variables here
    // to store data.
    public Material regularMaterial;
    public Material blockedMaterial;
    public Material openMaterial;
    public Material closedMaterial;
    public Material goalMaterial;
    public Material startMaterial;
    public Material obstacleMaterial;
    public Material portalMaterial;
    public Material portalExitMaterial;

    Material currentMaterial;
    void Start()
    {
        currentMaterial = null;
        gValue = 0;
    }

    // This function is called when something has changed on the board. All 
    // tiles have been created before it is called.
    public override void OnSetup(Board board)
    {
        dijkstra = GetComponentInParent<Dijkstra>();

        // 2. Each tile has a unique 'coordinate'
        Vector2Int key = Coordinate;

        // 3. Tiles can have different modifiers
        if (IsBlocked)
        {
            currentMaterial = blockedMaterial;
        }
        else
        {
            if (currentMaterial != openMaterial //If the currentMaterial isnt any other material then it sets it to the regularMaterial
                && currentMaterial != closedMaterial
                && currentMaterial != startMaterial
                && currentMaterial != goalMaterial
                && currentMaterial != portalMaterial
                && currentMaterial != portalExitMaterial
                && currentMaterial != obstacleMaterial)
                currentMaterial = regularMaterial;
        }
        if (IsObstacle(out int penalty)) //Sets the material to the obstacleMaterial
        {
            SetTileMaterial(obstacleMaterial);
        }

        if (IsCheckPoint) //Sets the material to the checkPointMaterial 
        {
            SetTileMaterial(goalMaterial);
        }

        if (IsStartPoint)//Sets the material to the startMaterial and sets the startTile in the Djikstras script to this tile
        {
            dijkstra.SetStartTile(this);
            SetTileMaterial(startMaterial);
        }

        if (IsPortal(out Vector2Int destination)) //Sets the material of this tile and the desination tile to their acording material
        {
            SetTileMaterial(portalMaterial);
            board.TryGetTile(destination, out Tile square);
            square.SetTileMaterial(portalExitMaterial);
        }

        // 4. Other tiles can be accessed through the 'board' instance
        if (board.TryGetTile(new Vector2Int(2, 1), out Tile otherTile))
        {
        }

        // 5. Change the material color if this tile is blocked
        if (TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            if (currentMaterial == null) meshRenderer.material = regularMaterial;
            else meshRenderer.material = currentMaterial;
        }
    }


    // This function is called during the regular 'Update' step, but also gives
    // you access to the 'board' instance.
    public override void OnUpdate(Board board)
    {
    }

    public void SetTileMaterial(Material material) //Updates the current material to input and sets this tiles material to the input material
    {
        currentMaterial = material;
        TryGetComponent<MeshRenderer>(out var meshRenderer);
        meshRenderer.material = material;
    }

}