using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public static bool GamePaused = false;

    private const float PAUSED_TIME_SCALE = 0.0000000001f;
    private const float NORMAL_TIME_SCALE = 1f;

    [SerializeField] private Canvas _pauseCanvas;
    private LevelLoader _levelLoader;

    void Start()
    {
        this._pauseCanvas.enabled = false;
        this._levelLoader = FindObjectOfType<LevelLoader>();
    }

    void Update()
    {
        this.PauseGame();    
    }

    private void PauseGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GamePaused = !GamePaused;
            this.SetPauseState();
        }
    }

    public void ExitGame()
    {
        GamePaused = false;
        this.SetPauseState();
        this._levelLoader.GoToGameplayOptions();
    }

    public void ResumeGame()
    {
        GamePaused = false;
        this.SetPauseState();
    }

    public void RestartGame()
    {
        GamePaused = false;
        this.SetPauseState();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private void SetPauseState()
    {
        Time.timeScale = GamePaused ? PAUSED_TIME_SCALE : NORMAL_TIME_SCALE;
        this._pauseCanvas.enabled = GamePaused;
    }
}
