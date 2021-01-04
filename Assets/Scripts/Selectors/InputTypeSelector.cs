using GameInput;
using UnityEngine;
using UnityEngine.UI;

public class InputTypeSelector : MonoBehaviour
{
    [SerializeField] private InputTypes _initialOption = InputTypes.Keyboard;

    [SerializeField] private Dropdown _inputSelectDropdown;

    private Global _global;

    void Start()
    {
        this._global = FindObjectOfType<Global>();

        this.SelectInitialInputType();

        this._inputSelectDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(this._inputSelectDropdown);
        });
    }

    private void SelectInitialInputType()
    {
        InputTypes savedInputType = this._global.InputType;
        this._inputSelectDropdown.value = (int)savedInputType;
    }

    void DropdownValueChanged(Dropdown change)
    {
        int inputIndex = change.value;
        InputTypes newInput = (InputTypes)inputIndex;
        this._global.SetInputType(newInput);
    }
}
