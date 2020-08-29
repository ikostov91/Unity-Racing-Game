using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera[] _cameras;

    void Start()
    {
        this._cameras.First().gameObject.SetActive(true);
        foreach (Camera camera in this._cameras.Skip(1))
        {
            camera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        this.ToggleCameras();
    }

    private void ToggleCameras()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.SwitchToNextCamera();
        }
    }

    private void SwitchToNextCamera()
    {
        Camera activeCamera = this._cameras.First(x => x.gameObject.activeSelf);

        int cameraIndex = Array.IndexOf(this._cameras, activeCamera);
        int nextCameraIndex = cameraIndex + 1 < this._cameras.Length ? cameraIndex + 1 : 0;

        this._cameras[cameraIndex].gameObject.SetActive(false);
        this._cameras[nextCameraIndex].gameObject.SetActive(true);
    }
}
