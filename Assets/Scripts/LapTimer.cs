using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapTimer : MonoBehaviour
{
    private float _startTime;
    private float _elapsedTime;
    private bool _timerStarted = false;
    private List<float> _lapTimes;
    private bool _isLapValid = true;

    [SerializeField] private Text _lapTimeDisplay;

    private void Start()
    {
        this._lapTimes = new List<float>();    
    }

    void Update()
    {
        this.SetElapsedTime();
        this.UpdateUITimer();
    }

    private void SetElapsedTime()
    {
        if (this._timerStarted && this._isLapValid)
        {
            this._elapsedTime = Time.time - this._startTime;
        }
        else
        {
            this._elapsedTime = 0f;
        }
    }

    private void UpdateUITimer()
    {
        string lapTimeText;
        if (this._isLapValid)
        {
            lapTimeText = Math.Round(this._elapsedTime, 3).ToString();
        }
        else
        {
            lapTimeText = "Invalid";
        }
        this._lapTimeDisplay.text = $"Lap: {lapTimeText}";
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        VehicleController _vehicle = otherCollider.gameObject.GetComponentInParent<VehicleController>();
        if (_vehicle != null)
        {
            if (this._isLapValid)
            {
                this._lapTimes.Add(this._elapsedTime);
            }
            
            this._timerStarted = true;
            this._startTime = Time.time;
            this._isLapValid = true;
        }
    }

    public void InvalidateLap()
    {
        this._isLapValid = false;
    }
}
