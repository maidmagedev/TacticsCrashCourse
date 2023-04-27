using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script spawns a 2D grid of game objects.
public class TileGrid : MonoBehaviour
{

    [Header("Settings")]
    public GameObject tilePrefab; // the tile to be created
    public int rows;
    public int columns;
    public Algorithm algorithm;
    public double heuristicWeight = 11; // only used for A_Star. I've found that 11 works the best.
    public float autoStepTimer = 0.1f; // delay between each iteration of the pathfinding algorithm
    public bool manualStepping = false; // toggles manual gridcell by gridcell stepping during pathfinding.
    public KeyCode manualStepKey = KeyCode.Space;

    [Header("Operational Variables....")]
    public GridCell[,] grid;
    public GameObject gridContainer; // empty game object that acts like a folder. This will contain all the individual cells.
    public int maxDistance;
    public bool currentlyRunning;

    [Header("References")]
    public GameManager gameManager;

    // Pathfinding algorithm to be used.
    public enum Algorithm {
        Dijkstras,
        A_Star
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InstantiateGrid());
    }

    // Creates our Grid using settings from our local variables. Hooks up all the references, and creates the physical game world tiles.
    private IEnumerator InstantiateGrid() {
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
                yield return new WaitForSeconds(0.01f);
            }
        }
        SetupNeighbors();

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
                if (x - 1 >= 0) {
                    currCell.neighbors.Add(grid[x - 1, y]);
                }
                // east
                if (x + 1 < columns) {
                    currCell.neighbors.Add(grid[x + 1, y]);
                }
                // south
                if (y - 1 >= 0) {
                    currCell.neighbors.Add(grid[x, y - 1]);
                }
            }
        }
    }

    private void ResetText() {
        foreach(GridCell curr in grid) {
            curr.textElem.text = "?";
            curr.textElem.color = Color.white;
        }
    }

    public IEnumerator GetPath(GridCell end) {

        currentlyRunning = true;
        (int, int) endCoordinates = end.coordinates;

        ResetText();
        // Get the starting GridCell from our Grid, using the current selected unit's coordinates.
        GridCell start = grid[gameManager.currentSelectedUnit.coordinates.Item1, gameManager.currentSelectedUnit.coordinates.Item2];

        List<GridCell> pseudoQueue = new List<GridCell>();
        List<GridCell> visited = new List<GridCell>();
        List<GridCell> path = new List<GridCell>();

        bool lookingForPath = true;

        // Reset each grid cell's states to ensure the path is generated correctly.
        foreach (GridCell currCell in grid) {
            currCell.finalCost = int.MaxValue; // set to infinity
            currCell.gCost = 0;
            currCell.hCost = 0;
            currCell.previous = null;

            //pseudoQueue.Add(currCell); // In standard dijsktra's, you might add all cells immediately.
        }

        // some versions of pathfinding algorithms store these instead in a map/dict in this script but personally I like this more... dont know why.
        start.finalCost = 0;
        pseudoQueue.Add(start);
        int i = 0;
        while (pseudoQueue.Count > 0 && lookingForPath) {
            Debug.Log("loop: " + i);
            pseudoQueue.Sort((a, b) => a.finalCost.CompareTo(b.finalCost)); // Sort the queue.

            GridCell current = pseudoQueue[0];
            pseudoQueue.Remove(current);
            visited.Add(current);

            if (current.cellType == GridCell.Type.notTraversable) {
                continue; // cell is not walkable, so ignore this cell.
            }

            current.textElem.text = current.finalCost.ToString();
            current.textElem.color = Color.cyan;

            // Is this the destination cell?
            if (current.coordinates == endCoordinates) {
                lookingForPath = false;

                Debug.Log("Path Found!");
                // End cell is treated differently for color styling. 
                current.textElem.text = "e";
                current.textElem.color = Color.red;
                path.Add(current);

                current = current.previous;

                // found the end!
                while (current != null && current.previous != null || current == start) {
                    path.Add(current);
                    current.textElem.text = ".";
                    current = current.previous;
                }
                start.textElem.text = "s";
                start.textElem.color = Color.green;

                StartCoroutine(PathLerp(path, gameManager.currentSelectedUnit));
                
                gameManager.currentSelectedUnit.coordinates = endCoordinates;
                pseudoQueue.Clear();
                continue; // would normally have a return here or something, but because this is a coroutine some weird stuff happens? 
                // we just continue, the loop ends bc of the above condition and the function is over.
            }

            // update neighbor distances.
            foreach(GridCell neighbor in current.neighbors) {


                Debug.Log(" + neighbor");
                // Get the distance from this cell.

                // Dijsktra's Algorithm
                if (algorithm == Algorithm.Dijkstras) {
                    double distFromCurr;
                    distFromCurr = current.finalCost + (neighbor.baseCost * neighbor.costModifier);
                    if (pseudoQueue.Contains(neighbor)) { // potentially update a neighbor
                        // If the new distance from this cell is less than the stored distance, update the stored dist.
                        if (distFromCurr < neighbor.finalCost) {
                            neighbor.finalCost = distFromCurr;
                            neighbor.previous = current;
                        }
                        // Normally we'd need to remove then add back the item if its updated, but since we sort on every pass I dont think we need to.
                    } else if (!visited.Contains(neighbor)) { // add this unvisited node as a neighbor
                        pseudoQueue.Add(neighbor);
                        neighbor.finalCost = distFromCurr;
                        neighbor.previous = current;
                        neighbor.textElem.color = Color.yellow;
                        neighbor.textElem.text = neighbor.finalCost.ToString();
                    }
                } else if (algorithm == Algorithm.A_Star) {
                    // A* Algorithm using Manhattan Distance.
                    double newGCost = neighbor.baseCost + current.gCost;
                    double newHCost = ManhattanDist(neighbor, end) * heuristicWeight;

                    if (pseudoQueue.Contains(neighbor)) {
                        if (newGCost + newHCost < neighbor.finalCost) {
                            neighbor.gCost = newGCost;
                            neighbor.hCost = newHCost;
                            neighbor.finalCost = neighbor.gCost + neighbor.hCost;    
                            neighbor.previous = current;
                        }
                    } else if (!visited.Contains(neighbor)) {
                        pseudoQueue.Add(neighbor);

                        neighbor.gCost = newGCost;
                        neighbor.hCost = newHCost;
                        neighbor.finalCost = neighbor.gCost + neighbor.hCost;    
                        neighbor.previous = current;

                        neighbor.textElem.color = Color.yellow;
                        neighbor.textElem.text = neighbor.finalCost.ToString();
                    }
                }
            }
            i++;
            // Manualstepping can be set in the inspector window in unity. This allows us to go through each cell one by one every time we press the key (default space bar)
            if (manualStepping) {
                bool wait = true;
                while (wait) {
                    if (Input.GetKeyDown(manualStepKey)) {
                        wait = false;
                    }
                    yield return null;
                }
            } else {
                yield return new WaitForSeconds(autoStepTimer);
            }
        }
        if (lookingForPath) {
            Debug.Log("Destination was unreachable.");
            currentlyRunning = false;
        }
    }

    // hCost - hueristic - used by A* pathfinding.
    // Heuristic cost is the predicted distance to the end.
    public int ManhattanDist(GridCell A, GridCell B) {
        return Mathf.Abs(A.coordinates.Item1 - B.coordinates.Item1) + Mathf.Abs(A.coordinates.Item2 - B.coordinates.Item2);
    }

    public IEnumerator PathLerp(List<GridCell> path, Unit unit) {
        int i = path.Count - 1;
        foreach (GridCell current in path) {
            if (i == 0) {
                continue;
            }
            Vector3 startPosition = path[i].gameObject.transform.position;
            i--;
            Vector3 endPosition = path[i].gameObject.transform.position;
            float duration = 0.2f;
            float timeElapsed = 0.0f;
            while (timeElapsed < duration) {
                timeElapsed += Time.deltaTime;
                unit.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / duration);
                yield return new WaitForEndOfFrame();
            }

        }
        gameManager.ChangeUnit();
        currentlyRunning = false;
    }
}
