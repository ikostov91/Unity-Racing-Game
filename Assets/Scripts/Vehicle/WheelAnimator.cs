using UnityEngine;

public class WheelAnimator : MonoBehaviour
{
    [SerializeField] private float _yRotation = 0f;
    private WheelCollider _wheelCollider;

    void Start()
    {
        this._wheelCollider = this.GetComponent<WheelCollider>();
    }

    void Update()
    {
        this.ApplyLocalPositionToWheel();
    }

    private void ApplyLocalPositionToWheel()
    {
        if (this._wheelCollider.transform.childCount == 0)
            return;

        Transform visualWheel = this._wheelCollider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        this._wheelCollider.GetWorldPose(out position, out rotation);

        Quaternion newRotation = rotation * Quaternion.Euler(new Vector3(0, this._yRotation, 0));
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = newRotation;
    }
}
