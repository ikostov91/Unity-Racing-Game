using Constants;
using GameInput;
using System.Linq;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private InputTypes _currentInputType = InputTypes.Keyboard;
    [SerializeField] private GameObject _currentVehicle = null;
    private Vector3 _vehiclePosition = new Vector3(x: 64, y: 0, z: 49);
    private Quaternion _vehicleRotation = new Quaternion(x: 0, y: 180, z: 0, w: 0);
    
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        LoadVehicleForGameplay();
    }

    private void LoadVehicleForGameplay()
    {
        string[] gameplayScenes = new string[] { LevelNameConstants.RaceTrack, LevelNameConstants.ParkingLot };
        LevelLoader levelLoader = GetComponent<LevelLoader>();
        string currentScene = levelLoader.GetCurrentSceneName();
        if (gameplayScenes.Any(x => x == currentScene))
        {
            Instantiate(this._currentVehicle);
        }
    }

    public void SetNewVehicle(GameObject newVehicle)
    {
        this._currentVehicle = newVehicle;
    }

    public void SetNewInputType(InputTypes newInputType)
    {
        this._currentInputType = newInputType;
    }
}
