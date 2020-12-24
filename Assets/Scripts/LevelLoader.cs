using Constants;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private string _currentScene = LevelNameConstants.StartMenu;
    private string[] _gameplayScenes = { LevelNameConstants.RaceTrack, LevelNameConstants.OvalTrack };

    private Global _global;

    public string ActiveScene => this._currentScene;

    void Start()
    {
        this._global = FindObjectOfType<Global>();
        this._currentScene = SceneManager.GetActiveScene().name;
    }

    public void LoadGameStart()
    {
        this._currentScene = this._global.SelectedTrack;
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
        string activeScene = SceneManager.GetActiveScene().name;
        return _gameplayScenes.Any(x => x == activeScene);
    }
}
