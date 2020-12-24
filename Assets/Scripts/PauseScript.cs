using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public static bool GamePaused = false;

    private const float PAUSED_TIME_SCALE = 0.0000000001f;
    private const float NORMAL_TIME_SCALE = 1f;

    [SerializeField] private Canvas PauseCanvas;

    void Update()
    {
        this.PauseGame();    
    }

    private void PauseGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GamePaused = !GamePaused;
            Time.timeScale = GamePaused ? PAUSED_TIME_SCALE : NORMAL_TIME_SCALE;
        }
    }
}
