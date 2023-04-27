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
    public double costModifier = 1.0;
    public double cost;
    public double dist;
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
