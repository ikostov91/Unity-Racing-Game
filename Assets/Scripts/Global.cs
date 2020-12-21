using Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Global : MonoBehaviour
{
    private LevelLoader _levelLoader;

    [SerializeField] private GameObject _currentVehicle;
    [SerializeField] private string _currentTrack = LevelNameConstants.RaceTrack;
    [SerializeField] private string _currentGameMode = GameModeConstants.FreeRide;

    [SerializeField] public GameObject MainGaugesCanvas;

    public GameObject GetCurrentVehicle => this._currentVehicle;
    public string GetCurrentTrack => this._currentTrack;

    private void Awake()
    {
        this._levelLoader = FindObjectOfType<LevelLoader>();

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.InstantiatePlayerVehicle();
    }

    private void InstantiatePlayerVehicle()
    {
        if (this._levelLoader.IsCurrentSceneGameplay())
        {
            Instantiate(this._currentVehicle, new Vector3(6, 0, 0), Quaternion.identity);
            Instantiate(this.MainGaugesCanvas);
        }
    }
}
