using Assets.Scripts.PlayerInput;
using Constants;
using GameInput;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Global : MonoBehaviour
{
    [SerializeField] private GameObject _currentVehicle;
    [SerializeField] private string _currentTrack = LevelNameConstants.RaceTrack;
    [SerializeField] private string _currentGameMode = GameModeConstants.FreeRide;
    private int _fuelMultiplier = 1;
    private InputTypes _inputType = InputTypes.Keyboard;

    private LevelLoader _levelLoader;

    public GameObject SelectedVehicle => this._currentVehicle;
    public string SelectedTrack => this._currentTrack;
    public int FuelMultiplier => this._fuelMultiplier;
    public InputTypes InputType => this._inputType;

    private void Awake()
    {
        int globalCount = FindObjectsOfType<Global>().Length;
        if (globalCount > 1)
        {
            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        this._levelLoader = FindObjectOfType<LevelLoader>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SetSelectedVehicle(GameObject vehicle)
    {
        this._currentVehicle = vehicle;
    }

    public void SetSelectedTrack(string track)
    {
        this._currentTrack = track;
    }

    public void SetFuelSetting(int newFuelSetting)
    {
        this._fuelMultiplier = newFuelSetting;
    }

    public void SetInputType(InputTypes newInputType)
    {
        this._inputType = newInputType;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.InstantiatePlayerVehicle();
    }

    private void InstantiatePlayerVehicle()
    {
        if (this._levelLoader.IsCurrentSceneGameplay())
        {
            GameObject playerVehicle = Instantiate(this._currentVehicle, new Vector3(6, 0, 0), Quaternion.identity);
        }
    }
}
