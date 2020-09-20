using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Text _bestLapTimeDisplay;
    [SerializeField] private Text _lastLapTimeDisplay;

    private void Start()
    {
        this._lapTimes = new List<float>();

        if (this._lapTimes.Count == 0)
        {
            this._bestLapTimeDisplay.text = LapTimingConstants.NA;
            this._lastLapTimeDisplay.text = LapTimingConstants.NA;
        }
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
            lapTimeText = LapTimingConstants.Invalid;
        }
        this._lapTimeDisplay.text = $"{lapTimeText}";
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.gameObject.CompareTag(TagConstants.PlayerVehicleTag))
        {
            if (this._isLapValid && this._timerStarted)
            {
                this._lapTimes.Add(this._elapsedTime);

                this._lastLapTimeDisplay.text = $"{Math.Round(this._elapsedTime, 3)}";

                float bestLapTime = this._lapTimes.Min();
                this._bestLapTimeDisplay.text = $"{Math.Round(bestLapTime, 3)}";
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
