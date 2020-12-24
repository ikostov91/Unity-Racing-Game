using UnityEngine;

public class ExhaustController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _exhaust;
    [SerializeField] private float _exhaustRateMin = 100;
    [SerializeField] private float _exhaustRateMax = 500;

    private EngineController _engineController;

    void Start()
    {
        this._engineController = GetComponent<EngineController>();
    }

    void Update()
    {
        this.EmitExhaust();
    }

    private void EmitExhaust()
    {
        float currentEngineRpm = this._engineController.EngineRpm;
        float interpolatedRpm = Mathf.InverseLerp(
                this._engineController.EngineMinRpm,
                this._engineController.EngineMaxRpm,
                currentEngineRpm
            );
        float interpolatedEmissionRate = Mathf.Lerp(this._exhaustRateMin, this._exhaustRateMax, interpolatedRpm);

        var emission = this._exhaust.emission;
        emission.rateOverTime = interpolatedEmissionRate;
    }
}
