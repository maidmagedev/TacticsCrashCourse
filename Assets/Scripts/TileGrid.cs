using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script spawns a 2D grid of game objects.
public class TileGrid : MonoBehaviour
{

    [Header("Setup")]
    public GameObject tilePrefab;
    public int rows;
    public int columns;

    [Header("Operational Variables....")]
    public GridCell[,] grid;
    public GameObject gridContainer; // empty game object that acts like a folder. This will contain all the individual cells.
    public int maxDistance;

    [Header("References")]
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        InstantiateGrid();
        SetupNeighbors();
    }

    // Creates our Grid using settings from our local variables. Hooks up all the references, and creates the physical game world tiles.
    private void InstantiateGrid() {
        // Create a 2 Dimensional Array of GridCells.
        if (grid == null) {
            grid = new GridCell[columns, rows];
        }
        // We're using Y to represent the Z axis here. It's a misnomer. 
        // Technically it's Z, but since we're working with a 2D plane it's easier to think of it as Y.
        for (int y = 0; y < rows; y++) 
        {
            for (int x = 0; x < columns; x++) { 
                // Create a new Game Object in the game world using the Instantiate method, using the for loops as position points.
                // - Instantiate has the format: Instantiate(GameObject objectTemplate, Vector3 position, Quaternion rotation);
                // Keep in mind that even though it's labeled Y, we put it in the Z component of the Vector3. 
                // - This is because in 3D, Y is vertical, but we're top down and ignoring verticality for this.
                // We can just directly use X and Y for the Vector3 Position since each cell is a 1 unit wide cube.
                GameObject currObj = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                GridCell currCell = currObj.GetComponent<GridCell>(); // GetComponent<>() fetches a script attached to the object.
                currCell.coordinates = (x, y);

                // Update the text to display the coordinate information.
                //currCell.textElem.text = "(" + x + ", " + y + ")";

                // Update our grid to have a reference to this new cell.
                grid[x, y] = currCell;

                // Make this object a child of the gridContainer object. This keeps our hierarchy clean.
                currObj.transform.SetParent(gridContainer.transform);
            }
        }
    }

    // Second pass of the grid that sets up neighbor connections.
    // Connections are North/South/East/West if they are within bounds- no diagonals
    private void SetupNeighbors() {
        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < columns; x++) {
                GridCell currCell = grid[x, y];
                // north
                if (y + 1 < rows) {
                    currCell.neighbors.Add(grid[x, y + 1]);
                }
                // west
                if (x - 1 > 0) {
                    currCell.neighbors.Add(grid[x - 1, y]);
                }
                // east
                if (x + 1 < columns) {
                    currCell.neighbors.Add(grid[x + 1, y]);
                }
                // south
                if (y - 1 > 0) {
                    currCell.neighbors.Add(grid[x, y - 1]);
                }
            }
        }
    }

    public void GetPath((int, int) endCoordinates) {
        // Get the starting GridCell from our Grid, using the current selected unit's coordinates.
        GridCell start = grid[gameManager.currentSelectedUnit.coordinates.Item1, gameManager.currentSelectedUnit.coordinates.Item2];

        List<GridCell> pseudoQueue = new List<GridCell>();

        // Reset each grid cell's states to ensure the path is generated correctly.
        foreach (GridCell currCell in grid) {
            currCell.dist = int.MaxValue; // set to infinity
            currCell.previous = null;

            //pseudoQueue.Add(currCell);
        }

        // some versions of pathfinding algorithms store these instead in a map/dict in this script but personally I like this more... dont know why.
        start.dist = 0;
        pseudoQueue.Add(start);
        int i = 0;
        while (pseudoQueue.Count > 0) {
            Debug.Log("loop: " + i);
            pseudoQueue.Sort((a, b) => a.dist.CompareTo(b.dist)); // Sort the queue.
            GridCell current = pseudoQueue[0];
            pseudoQueue.Remove(current);
            current.textElem.text = i.ToString();// current.dist.ToString();

            if (current.coordinates == endCoordinates) {
                Debug.Log("End!");
                // found the end!
                while (current.previous != null) {
                    current.textElem.text = "path";
                    current = current.previous;
                }
                return;
            }

            // update neighbor distances.
            foreach(GridCell neighbor in current.neighbors) {
                Debug.Log(" + neighbor");
                // Get the distance from this cell.
                double distFromCurr = current.dist + (neighbor.cost * neighbor.costModifier);

                if (pseudoQueue.Contains(neighbor)) {
                    // If the new distance from this cell is less than the stored distance, update the stored dist.
                    if (distFromCurr < neighbor.dist) {
                        neighbor.dist = distFromCurr;
                        neighbor.previous = current;
                    }
                } else {
                    pseudoQueue.Add(neighbor);
                    neighbor.dist = distFromCurr;
                    neighbor.previous = current;
                    
                }
            }
            i++;
        }



    }

}
