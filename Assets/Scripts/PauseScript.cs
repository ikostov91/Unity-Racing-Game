using UnityEngine;

public class PauseScript : MonoBehaviour
{
    private void Start()
    {
        // this.gameObject.SetActive(false);    
    }

    public void OnResumeClick()
    {
        Debug.Log("Resume");
    }

    public void OnExitClick()
    {
        Debug.Log("Cancel");

    }
}
