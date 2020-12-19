﻿using Assets.Scripts;
using Assets.Scripts.PlayerInput;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(IInput))]
public class GearboxController : MonoBehaviour
{
    private IInput _input;
    private VehicleController _vehicleController;

    private IEngine Engine;
    private IGearbox Gearbox;

    private int _currentGear = 0;

    [SerializeField] private float _shiftUpTime = 1.0f;
    [SerializeField] private float _shiftDownTime = 0.2f;

    public int Gear => this._currentGear;

    void Start()
    {
        this._input = GetComponent<IInput>();
        this._vehicleController = GetComponent<VehicleController>();

        this.Engine = GetComponent<IEngine>();
        this.Gearbox = GetComponent<IGearbox>();
    }

    void Update()
    {
        this.ChangeGears();
    }

    private void ChangeGears()
    {
        if (this._input.GearUp || this._input.GearDown)
        {
            int nextGear = this._input.GearUp ? this._currentGear + 1 : this._currentGear - 1;
            int nextGearClamped = this._input.GearUp ?
                Mathf.Min(nextGear, this.Gearbox.HighestGear) :
                Mathf.Max(nextGear, this.Gearbox.LowestGear);

            if (!this.CheckIfGearChangeIsPossible(nextGearClamped))
            {
                return;
            }

            float shiftTime = this._input.GearUp ? this._shiftUpTime : this._shiftDownTime;
            this.StartCoroutine(this.ShiftIntoNewGear(nextGearClamped, shiftTime));
        }
    }

    private bool CheckIfGearChangeIsPossible(int nextGear)
    {
        if (this._currentGear == nextGear)
        {
            return false;
        }
        else if (this._currentGear > 0 && nextGear >= 1 && nextGear <= this.Gearbox.HighestGear && this._vehicleController.Speed > this._vehicleController.SpeedThreshold)
        {
            AxleInfo motorAxle = this._vehicleController.MotorAxles.FirstOrDefault();
            float wheelRpm = (motorAxle.RightWheel.rpm + motorAxle.LeftWheel.rpm) / 2;
            float engineRpm = wheelRpm * this.Gearbox.ForwardGearRatios[nextGear] * this.Gearbox.FinalDriveRatio;
            if (engineRpm < this.Engine.MinimumRpm || engineRpm > this.Engine.MaximumRmp)
            {
                return false;
            }
        }
        else if (this._currentGear == -1 && nextGear >= 0 && this._vehicleController.Speed > this._vehicleController.SpeedThreshold)
        {
            return false;
        }
        else if (this._currentGear == 0 && nextGear == -1 && this._vehicleController.Speed > this._vehicleController.SpeedThreshold)
        {
            return false;
        }

        return true;
    }

    private IEnumerator ShiftIntoNewGear(int gear, float shiftTime)
    {
        this._currentGear = 0;
        this._vehicleController.CutTrottle = true;

        yield return new WaitForSeconds(shiftTime);

        this._currentGear = gear;
        this._vehicleController.CutTrottle = false;
    }
}

public class FormulaTempGearbox : MonoBehaviour, IGearbox
{
    public int NumberOfGears => 7;
    public int LowestGear => -1;
    public int HighestGear => 7;
    public int NeutralGear => 0;
    public int ReverseGear => -1;
    public int FirstGear => 1;
    public float KPD => 0.96f;

    public Dictionary<int, float> ForwardGearRatios => new Dictionary<int, float>()
    {
        { -1, 2.90f },
        {  0, 0f    },
        {  1, 2.97f },
        {  2, 2.07f },
        {  3, 1.43f },
        {  4, 1.00f },
        {  5, 0.71f },
        {  6, 0.57f },
        {  7, 0.48f }
    };

    public float FinalDriveRatio => 3.42f;

    public float ReverseRatio => 2.90f;
}