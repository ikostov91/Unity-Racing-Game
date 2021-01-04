using Assets.Scripts.PlayerInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.PlayerInput
{
    public class XboxGamepadInput : MonoBehaviour, IInput
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

        void Update()
        {
            if (!PauseScript.GamePaused)
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
        }

        private void ProcessThrottle()
        {
            float input = Gamepad.current.rightTrigger.ReadValue();
            this._throttle = input;
        }

        private void ProcessBrake()
        {
            float input = Gamepad.current.leftTrigger.ReadValue();
            this._brake = input;
        }

        private void ProcessSteering()
        {
            float input = Gamepad.current.leftStick.x.ReadValue();
            this._steering = input;
        }

        private void ProcessGearUp()
        {
            bool input = Gamepad.current.buttonSouth.wasPressedThisFrame;
            this._gearUp = input;
        }

        private void ProcessGearDown()
        {
            bool input = Gamepad.current.buttonWest.wasPressedThisFrame;
            this._gearDown = input;
        }

        private void ProcessHandbrake()
        {
            bool input = Gamepad.current.buttonEast.wasPressedThisFrame;
            this._handbrake = input;
        }

        private void ProcessClutch()
        {
            bool input = Gamepad.current.dpad.down.wasPressedThisFrame;
            this._clutch = input == true ? 0f : 1f;
        }

        private void ProcessBoost()
        {
            bool input = Gamepad.current.buttonNorth.isPressed;
            this._boost = input;
        }
    }
}
