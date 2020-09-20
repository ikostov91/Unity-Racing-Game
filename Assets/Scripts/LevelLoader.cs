using Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(LevelNameConstants.RaceTrack);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToStartMenu()
    {
        SceneManager.LoadScene(LevelNameConstants.StartMenu);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToOptionsMenu()
    {
        SceneManager.LoadScene(LevelNameConstants.OptionsMenu);
    }
}
