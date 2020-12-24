using Constants;
using UnityEngine;
using UnityEngine.UI;

public class TrackSelector : MonoBehaviour
{
    [SerializeField] private string[] _allTracks = new string[] { LevelNameConstants.RaceTrack, LevelNameConstants.OvalTrack };
    [SerializeField] private string _initialTrack = LevelNameConstants.RaceTrack;

    [SerializeField] private Dropdown _trackSelectDropdown;

    private Global _global;

    void Start()
    {
        this._global = FindObjectOfType<Global>();

        this.SelectInitialTrack();

        this._trackSelectDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(this._trackSelectDropdown);
        });
    }

    private void SelectInitialTrack()
    {
        this._global.SetSelectedTrack(this._initialTrack);
    }

    void DropdownValueChanged(Dropdown change)
    {
        int trackIndex = change.value;
        this._global.SetSelectedTrack(this._allTracks[trackIndex]);
    }
}
