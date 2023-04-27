using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script takes collisions from the Cursor (MousePosition3D.cs) and then updates the indicator visibility.
public class GridCellCollider : MonoBehaviour
{
    [SerializeField] GameObject hoverIndicator;
    public GridCell myCell;
    [SerializeField] MousePosition3D cursor;

    void Start() {
        if (cursor == null) {
            cursor = FindObjectOfType<MousePosition3D>();
        }
    }

    void OnTriggerEnter(Collider other) {
        hoverIndicator.SetActive(true);
        if (cursor == null) {
            Debug.Log("null cursor");
            return;
        }
        cursor.occupiedCollider = this;
    }

    // void OnTriggerStay(Collider other) {
    //     // Mouse button down 0 is Left Click.
    //     if (Input.GetMouseButtonDown(0)) {
    //         myCell.blockedIndicator.SetActive(true);
    //         Debug.Log("hello!");
    //     }
    // }

    void OnTriggerExit(Collider other) {
        hoverIndicator.SetActive(false);
    }
}
