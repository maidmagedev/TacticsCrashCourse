using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents a player or enemy or anything really.
public class Unit : MonoBehaviour
{
    public Affiliation affiliation;
    public (int, int) coordinates; // coordinate along the TileGrid.cs

    public enum Affiliation {
        playerControlled,
        neutral,
        enemy
    }

    public void Start() {
        coordinates = (0, 0);
    }


}