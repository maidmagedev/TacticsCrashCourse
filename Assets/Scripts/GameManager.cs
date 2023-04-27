using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Unit currentSelectedUnit;
    public int currentUnitIndex = 0;
    public List<Unit> units;
    public TileGrid tileGrid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeUnit() {
        currentUnitIndex++;
        if (currentUnitIndex == units.Count) {
            currentUnitIndex = 0;
        }
        currentSelectedUnit = units[currentUnitIndex];

    }
}
