using Assets.Scripts.Enums;
using Constants;
using UnityEngine;
using UnityEngine.UI;

public class FuelSelector : MonoBehaviour
{
    [SerializeField] private int[] _allFuelOptions = new int[] { (int)FuelUsage.Real, (int)FuelUsage.X2, (int)FuelUsage.Off };
    [SerializeField] private int _initialOption = (int)FuelUsage.Real;

    [SerializeField] private Dropdown _fuelSelectDropdown;

    private Global _global;

    void Start()
    {
        this._global = FindObjectOfType<Global>();

        this.SelectInitialFuelSetting();

        this._fuelSelectDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(this._fuelSelectDropdown);
        });
    }

    private void SelectInitialFuelSetting()
    {
        this._global.SetFuelSetting(this._initialOption);
    }

    void DropdownValueChanged(Dropdown change)
    {
        int fuelIndex = change.value;
        this._global.SetFuelSetting(this._allFuelOptions[fuelIndex]);
    }
}
