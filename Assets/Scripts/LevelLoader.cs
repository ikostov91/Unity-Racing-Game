using Constants;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private string _currentScene = LevelNameConstants.StartMenu;
    
    private Global _global;
    private PauseScript _pauseScript;

    void Awake()
    {
        int levelLoaderCount = FindObjectsOfType<LevelLoader>().Length;
        if (levelLoaderCount > 1)
        {
            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        this._global = FindObjectOfType<Global>();
    }

    public void StartGame()
    {
        this._currentScene = this._global.GetCurrentTrack;
        SceneManager.LoadScene(this._currentScene);
    }

    public string GetCurrentSceneName()
    {
        return this._currentScene;
    }

    public void LoadScene(string sceneName)
    {
        this._currentScene = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    public void GoToStartMenu()
    {
        this._currentScene = LevelNameConstants.StartMenu;
        SceneManager.LoadScene(LevelNameConstants.StartMenu);
    }

    public void GoToGameplayOptions()
    {
        this._currentScene = LevelNameConstants.GameplayOptions;
        SceneManager.LoadScene(LevelNameConstants.GameplayOptions);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToOptionsMenu()
    {
        this._currentScene = LevelNameConstants.OptionsMenu;
        SceneManager.LoadScene(LevelNameConstants.OptionsMenu);
    }

    public bool IsCurrentSceneGameplay()
    {
        string[] gameplayScenes = new string[] { LevelNameConstants.ParkingLot, LevelNameConstants.RaceTrack };
        return gameplayScenes.Any(x => x == this._currentScene);
    }
}
