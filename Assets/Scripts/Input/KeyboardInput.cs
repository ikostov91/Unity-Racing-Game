using UnityEngine;
using Input = UnityEngine.Input;

namespace Assets.Scripts.PlayerInput
{
    public class KeyboardInput : MonoBehaviour, IInput
    {
        public float Throttle { get => this._throttle; }
        public float Brake { get => this._brake; }
        public float Clutch { get => this._clutch; }
        public float Steering { get => this._steering; }
        public bool GearUp { get => this._gearUp; }
        public bool GearDown { get => this._gearDown; }
        public bool Handbrake { get => this._handbrake; }
        public bool Boost { get => this._boost; }

        private float _throttle = 0f;
        private float _brake = 0f;
        private float _clutch = 0f;
        private float _steering = 0f;
        private bool _gearUp = false;
        private bool _gearDown = false;
        private bool _handbrake = false;
        private bool _boost = false;
        AnimationCurve _turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

        void Update()
        {
            this.ProcessThrottle();
            this.ProcessBrake();
            this.ProcessSteering();
            this.ProcessGearUp();
            this.ProcessGearDown();
            this.ProcessHandbrake();
            this.ProcessClutch();
            this.ProcessBoost();
        }

        private void ProcessThrottle()
        {
            bool input = Input.GetKey(KeyCode.UpArrow);
            this._throttle = input == true ? 1 : 0;
        }

        private void ProcessBrake()
        {
            bool input = Input.GetKey(KeyCode.DownArrow);
            this._brake = input == true ? 1 : 0;
        }

        private void ProcessSteering()
        {
            float input = Input.GetAxis("Horizontal");
            this._steering = this._turnInputCurve.Evaluate(input);
        }

        private void ProcessGearUp()
        {
            this._gearUp = Input.GetKeyDown(KeyCode.S);
        }

        private void ProcessGearDown()
        {
            this._gearDown = Input.GetKeyDown(KeyCode.X);
        }

        private void ProcessHandbrake()
        {
            this._handbrake = Input.GetKey(KeyCode.Space);
        }

        private void ProcessClutch()
        {
            bool input = Input.GetKey(KeyCode.Z);
            this._clutch = input == true ? 0f : 1f;
        }

        private void ProcessBoost()
        {
            this._boost = Input.GetKey(KeyCode.A);
        }
    }
}
