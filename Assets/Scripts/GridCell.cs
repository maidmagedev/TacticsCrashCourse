using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This is a Grid Cell, used by a TileGrid script which holds an array of these
public class GridCell : MonoBehaviour
{
    [Header("Settings")]
    public Type cellType;
    public Unit unit; // The Unit on this tile, if there is one.
    public (int, int) coordinates; // coordinates inside the TileGrid.cs Grid.

    [Header("Pathfinding Variables")]
    public double costModifier = 1.0; // might be used for special effects, speed bonuses... dunno
    public double baseCost; // base cost for this tile type. Ex: a mud tile might have a cost of 2 movement points, grass a cost of 1, and a teleporter a cost of 0.
    public double gCost; // distance from the starting node. 
    public double hCost; // heuristic value - predicted distance to the ending node. Ignored by Dijkstra's and used by A*.
    public double finalCost; 
    public GridCell previous;
    public List<GridCell> neighbors = new List<GridCell>(4);

    [Header("References")]
    public TextMeshProUGUI textElem;
    public GameObject blockedIndicator;

    public enum Type {
        traversable,
        notTraversable
    }

    public bool IsOccupied() {
        return unit != null;
    }



}
