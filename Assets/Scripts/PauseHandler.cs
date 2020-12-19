using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private Canvas _pauseCanvas;
    private LevelLoader _levelLoader;

    // Start is called before the first frame update
    void Start()
    {
        this._levelLoader = FindObjectOfType<LevelLoader>();
        this._pauseCanvas.enabled = false;    
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("UPDATE");
        this.PauseGame();
    }

    private void PauseGame()
    {
        Debug.Log("PAUSE GAME");
        if (this._levelLoader.IsCurrentSceneGameplay())
        {
            Debug.Log("PAUSE GAME");
            Debug.Log(this._levelLoader.GetCurrentSceneName());
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Time.timeScale == 1f)
                {
                    Time.timeScale = 0f;
                    this._pauseCanvas.enabled = true;
                }
                else
                {
                    Time.timeScale = 1f;
                    this._pauseCanvas.enabled = false;
                }
            }
        }
    }
}
