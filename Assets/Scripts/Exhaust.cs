using UnityEngine;

public class Exhaust : MonoBehaviour
{
    [SerializeField] private ParticleSystem _exhaustLeft;
    [SerializeField] private ParticleSystem _exhaustRight;
    [SerializeField] private float _exhaustRateMin = 100;
    [SerializeField] private float _exhaustRateMax = 500;

    private VehicleController _vehicleController;

    // Start is called before the first frame update
    void Start()
    {
        this._vehicleController = GetComponent<VehicleController>();
    }

    // Update is called once per frame
    void Update()
    {
        this.EmitExhaust();
    }

    private void EmitExhaust()
    {
        float currentEngineRpm = this._vehicleController.EngineRpm;
        float interpolatedRpm = Mathf.InverseLerp(
                this._vehicleController.Engine.MinimumRpm,
                this._vehicleController.Engine.MaximumRmp,
                currentEngineRpm
            );
        float interpolatedEmissionRate = Mathf.Lerp(this._exhaustRateMin, this._exhaustRateMax, interpolatedRpm);

        var emissionLeft = this._exhaustLeft.emission;
        var emissionRight = this._exhaustRight.emission;
        emissionLeft.rateOverTime = interpolatedEmissionRate;
        emissionRight.rateOverTime = interpolatedEmissionRate;
    }
}
