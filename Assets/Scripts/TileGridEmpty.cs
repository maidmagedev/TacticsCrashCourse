using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This script spawns a 2D grid of game objects.
public class TileGridEmpty : MonoBehaviour
{
    [Header("Settings")]
    public GameObject tilePrefab; // the tile to be created
    public int rows;
    public int columns;
    public double heuristicWeight = 11; // only used for A_Star. I've found that 11 works the best.
    public float autoStepTimer = 0.1f; // delay between each iteration of the pathfinding algorithm
    public bool manualStepping = false; // toggles manual gridcell by gridcell stepping during pathfinding.
    public KeyCode manualStepKey = KeyCode.Space;
    public float travelTime = 0.1f; // time to move the Unit's model physically per tile.

    [Header("Operational Variables....")]
    public GameObject gridContainer; // empty game object that acts like a folder. This will contain all the individual cells.

    public GridCell[,] grid;
    // public int maxDistance; // unused
    public bool currentlyRunning;

    [Header("References")]
    public GameManager gameManager;

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

    // This function creates a Path using either Dijsktra's or A* pathfinding algorithms and is highly customizable.
    // Most parameters can be edited using this files local variables via the script or the inspect window.
    // Heuristic values can be easily tweaked using a heuristic weight multiplier or changing the heuristic function entirely.
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
        }

        // some versions of pathfinding algorithms store these instead in a map/dict in this script but personally I like this more... dont know why.
        start.finalCost = 0;
        pseudoQueue.Add(start);

        while (pseudoQueue.Count > 0 && lookingForPath) {
        }
        yield return null;
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
            float duration = travelTime;
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
