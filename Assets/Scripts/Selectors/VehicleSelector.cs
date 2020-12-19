using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleSelector : MonoBehaviour
{
    [SerializeField] private List<GameObject> _allVehicles = new List<GameObject>();
    [SerializeField] private int _vehicleIndex = 0;

    [SerializeField] private Dropdown _vehicleSelectDropdown;

    private Global _global;

    void Start()
    {
        this._global = FindObjectOfType<Global>();

        this.SelectInitialVehicle();

        this._vehicleSelectDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(this._vehicleSelectDropdown);
        });
    }

    private void SelectInitialVehicle()
    {
        this._global.SetSelectedVehicle(this._allVehicles[this._vehicleIndex]);
    }

    void DropdownValueChanged(Dropdown change)
    {
        int vehicleIndex = change.value;
        this._global.SetSelectedVehicle(this._allVehicles[vehicleIndex]);
    }
}
