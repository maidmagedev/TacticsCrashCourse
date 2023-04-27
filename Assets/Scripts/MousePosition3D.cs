using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I'm pretty sure this script was either made by or heavily based on a script made by YT@CodeMonkey.
// Gets the position of the mouse by shooting a raycast from the camera, then checking where it collides on a certain layermask.
// This allows us to get a physical mouse position in the 3D game world. You don't even need to do this in 2D, the extra axis complicates things just a tiny bit.
//
// Also handles some editor tools for level editing and mouse input in general.
public class MousePosition3D : MonoBehaviour
{
    public Camera activeCam;
    [SerializeField] private LayerMask layerMask; // Only Target this Layer.

    public GridCellCollider occupiedCollider; // collider currently in contact with

    public bool gridCellTypeModifierTool; // used for editing &or debugging. Not intended for in-game use.
                                          // when active, pressing a grid cell will toggle it's traversability.

    public bool selectTileMode; // used for moving, attacking, etc

    [SerializeField] TileGrid tileGrid;

    // Update is called once per frame
    void Update()
    {
        // Update Position
        Ray ray = activeCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            transform.position = raycastHit.point;
        }

        // Get Mouse Input
        if (Input.GetMouseButtonDown(0)) {
            if (occupiedCollider != null) {
                // return;
                if (gridCellTypeModifierTool) {
                    ModifyCell();
                }
                if (selectTileMode) {
                    Move();
                }
            }
        }
        
    }

    // Swaps the current selected cell between traversable and non traversable.
    // Intended as an editor level creator tool, not for actual game use.
    void ModifyCell() {
        switch(occupiedCollider.myCell.cellType) {

            case GridCell.Type.notTraversable:
                occupiedCollider.myCell.blockedIndicator.SetActive(false);
                occupiedCollider.myCell.cellType = GridCell.Type.traversable;
                break;
            case GridCell.Type.traversable:
                occupiedCollider.myCell.blockedIndicator.SetActive(true);
                occupiedCollider.myCell.cellType = GridCell.Type.notTraversable;
                break;
        }
    }

    void Move() {
        if (!tileGrid.currentlyRunning) {
            StartCoroutine(tileGrid.GetPath(occupiedCollider.myCell));
        }
    }
}
