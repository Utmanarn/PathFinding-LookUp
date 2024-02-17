using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : MonoBehaviour
{
    [SerializeField, Tooltip("Default Cost of Movement")] int defaultCost = 1;
    [SerializeField, Tooltip("Allowed movement in one turn")] int allowedMovement = 5;
    [SerializeField, Tooltip("Enable if you want to manually step through the process")] bool waitForPlayerInput = true;
    [SerializeField, Tooltip("Insert FenceMarker prefab here")] GameObject fenceMarker;

    List<Tile> goalTiles;
    PriorityQueue<Tile> frontierQueue;
    Dictionary<Tile, GameObject> markers;
    Dictionary<Tile, Tile> previousTile;
    Dictionary<Tile, int> gValueOfTile;

    Board board;
    Tile startTile;
    Tile currentTile;
    Tile newTile;
    bool done = false;

    readonly List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int(0, -1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)
    };
    void Start() //initialisation and sets up the startTile
    {
        board = GetComponent<Board>();
        done = false;
        previousTile = new Dictionary<Tile, Tile>();
        gValueOfTile = new Dictionary<Tile, int>();
        frontierQueue = new PriorityQueue<Tile>();
        markers = new Dictionary<Tile, GameObject>();
        goalTiles = new List<Tile>();
        frontierQueue.Enqueue(0, startTile);
        previousTile[startTile] = null;
        gValueOfTile[startTile] = 0;
    }

    void Update()
    {
        if (!done)
        {
            //Depending on if the player has enable waitForPlayerInput it either autoupdates or waits for the player to press "P"
            if (Input.GetKeyDown(KeyCode.P) || !waitForPlayerInput)
            {
                //if the entire board has been checked it sets "done" to true
                //else it continues exploring the board
                if (frontierQueue.QueueIsEmpty())
                {
                    done = true;
                }
                else
                {
                    //Current tile is set to the current tile in the frontierQueue
                    currentTile = frontierQueue.GetCurrentTile();
                    //If the current tile is a goal tile then it adds it to the goalTile list
                    if (currentTile.IsCheckPoint) SetGoalTile(currentTile);
                    //Changes the material to show that the tile has been closed
                    ChangeTileMaterial(currentTile, currentTile.closedMaterial);
                    //For each allowed movement direction
                    foreach (Vector2Int direction in directions)
                    {
                        //Gets the new tile from the coordinate of the current tile and the current direction
                        board.TryGetTile(currentTile.coordinate + direction, out newTile);
                        if (newTile == null) continue;
                        if (newTile.IsBlocked) continue;
                        //If true then the tile has already been explored and has a g value
                        if (gValueOfTile.ContainsKey(newTile) == false)
                        {
                            //calculates the g value for the newTile with the g value of the current tile and adds it to the gValueOfTile dictionary
                            newTile.IsObstacle(out int penalty);
                            int newGValue = gValueOfTile[currentTile] + defaultCost + penalty; //Gets newGvalue from the g value fo the previous tile+ default cost + penalty(if their is a penalty)
                            gValueOfTile[newTile] = newGValue; //Updates G Value
                            //Add the newTile to the frontierQueue with the newGValue as its priority
                            frontierQueue.Enqueue(newGValue, newTile); //Adds the newTile to the frontierQueue with the newGvalue as priority
                            newTile.gValue = newGValue; //Updates G value
                            ChangeTileMaterial(newTile, newTile.openMaterial);
                            previousTile[newTile] = currentTile; // Sets the currentTile as the previous tile of the newTile
                        }
                        else //If the tile has already been explored
                        {
                            newTile.IsObstacle(out int penalty);
                            int newGValue = gValueOfTile[currentTile] + defaultCost + penalty; //Creates a g value buffer
                            if (newGValue < gValueOfTile[newTile]) //Checks if the g value buffer is lower than the old 
                            {
                                gValueOfTile[newTile] = newGValue;//Updates the g value to the g value buffer
                                frontierQueue.Enqueue(newGValue, newTile); //Opens the tile again so its neighbours g values can be recalculated
                                newTile.gValue = newGValue;
                                ChangeTileMaterial(newTile, newTile.openMaterial);
                                previousTile[newTile] = currentTile; //Updates the newTile previous tile to the current tile
                            }
                        }
                    }
                    if (currentTile.IsPortal(out Vector2Int target)) //If the current tile is a portal
                    {
                        board.TryGetTile(target, out newTile); //Gets the tile on the portal Desination location
                        //Below works the same as above but uses the portal desination tile as the newTile
                        if (newTile == null) return;
                        if (newTile.IsBlocked) return;
                        if (gValueOfTile.ContainsKey(newTile) == false)                         {
                            newTile.IsObstacle(out int penalty);
                            int newGValue = gValueOfTile[currentTile] + defaultCost + penalty;
                            gValueOfTile[newTile] = newGValue;
                            frontierQueue.Enqueue(newGValue, newTile);
                            newTile.gValue = newGValue;
                            ChangeTileMaterial(newTile, newTile.openMaterial);
                            previousTile[newTile] = currentTile;
                        }
                        else
                        {
                            newTile.IsObstacle(out int penalty);
                            int newGValue = gValueOfTile[currentTile] + defaultCost + penalty;
                            if (newGValue < gValueOfTile[newTile])
                            {
                                gValueOfTile[newTile] = newGValue;
                                frontierQueue.Enqueue(newGValue, newTile);
                                newTile.gValue = newGValue;
                                ChangeTileMaterial(newTile, newTile.openMaterial);
                                previousTile[newTile] = currentTile;
                            }
                        }
                    }
                }
                if (done)
                {
                    //Resets all tile materials by iterating over each tile
                    foreach (Tile tile in board.Tiles)
                    {
                        //If the tile isn't a special tile then set it to the regular tile material
                        if (!tile.IsBlocked && !tile.IsStartPoint && !tile.IsCheckPoint && !tile.IsPortal(out Vector2Int x) && !tile.IsObstacle(out int y) && !previousTile[tile].IsPortal(out x))
                        {
                            ChangeTileMaterial(tile, tile.regularMaterial);
                        }
                        else if (tile.IsObstacle(out y)) ChangeTileMaterial(tile, tile.obstacleMaterial); //If tile is an obstacle then set its material to the obstacleMaterial
                        else if (tile.IsPortal(out x)) //If tile is a portal then set the portal tile to the portalMaterial and the desination tile to the portalExitMaterial
                        {
                            board.TryGetTile(x, out Tile portalExit);
                            ChangeTileMaterial(portalExit, tile.portalExitMaterial);
                            ChangeTileMaterial(tile, tile.portalMaterial);
                        }
                        if (!tile.IsBlocked && !tile.IsStartPoint)
                        {
                            if (gValueOfTile[tile] <= allowedMovement) //If the g value of the tile is less or equal to the allowed movement then instantiate a marker on that tile and add it to the marker dictionary
                            {
                                markers.Add(tile, Instantiate(fenceMarker, new Vector3(tile.transform.position.x, tile.transform.position.y + 0.05f, tile.transform.position.z - 0.5f), Quaternion.identity));
                            }
                        }//Same as above but for startTile it is not added to the gvalue dictionary
                        else if (tile.IsStartPoint) markers.Add(tile, Instantiate(fenceMarker, new Vector3(tile.transform.position.x, tile.transform.position.y + 0.05f, tile.transform.position.z - 0.5f), Quaternion.identity));
                    }

                    foreach (Tile goalTile in goalTiles)
                    {
                        print(gValueOfTile[goalTile]);

                        currentTile = goalTile; //Sets the currentTile to the current goalTile
                        ChangeTileMaterial(currentTile, currentTile.goalMaterial);
                        Material tempMaterial = new Material(currentTile.openMaterial); //Creates a temporary material to not change the original materials
                        tempMaterial.color = Random.ColorHSV(0, 1);//Randomizes the material colour, this allows each goal path to have a different marker colour
                        if (markers.ContainsKey(currentTile)) //If the currentTile has a marker then change the material colour of the markers four cube children 
                        {
                            foreach (Transform child in markers[currentTile].transform)
                            {
                                child.GetComponent<MeshRenderer>().material = tempMaterial;
                            }
                        }
                        currentTile = previousTile[currentTile]; //Sets the current tile to the goalTile's previous node
                        while (previousTile[currentTile] != null) //Since the startNode's previous node is null then this will loop until it reaches the start tile
                        {
                            if (markers.ContainsKey(currentTile)) //If the tile has a marker update it's material colour
                            {
                                foreach (Transform child in markers[currentTile].transform)
                                {
                                    child.GetComponent<MeshRenderer>().material = tempMaterial;
                                }
                            }
                            currentTile = previousTile[currentTile];
                        }
                        foreach (Transform child in markers[currentTile].transform)//Updates the Start marker colour
                        {
                            child.GetComponent<MeshRenderer>().material = tempMaterial;
                        }
                        ChangeTileMaterial(currentTile, currentTile.startMaterial);
                    }
                }
                else frontierQueue.Dequeue(); //Removes the tile with highest priority from the queue
            }
        }
    }

    public void ChangeTileMaterial(Tile tile, Material material) //Changes input Tile to input material
    {
        tile.SetTileMaterial(material);
    }

    public void SetStartTile(Tile tile) //Sets the start tile to input
    {
        startTile = tile;
    }

    public void SetGoalTile(Tile tile) //Adds input to goalTile List if it does not already contain the input
    {
        if (!goalTiles.Contains(tile)) goalTiles.Add(tile);
    }
}