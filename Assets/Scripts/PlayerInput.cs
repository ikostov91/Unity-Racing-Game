//using UnityEngine;
//using UnityStandardAssets.CrossPlatformInput;
//using GameInput;

//public class PlayerInput : MonoBehaviour
//{
//    public bool InputActive { get; private set; }
//    // Setup for Xbox controller
//    public PlayerInputTypes InputType { get; private set; } = PlayerInputTypes.Keyboard;
//    public float Throttle { get; private set; }
//    public float Brake { get; private set; }
//    public float Steering { get; private set; }
//    public bool GearUp { get; private set; }
//    public bool GearDown { get; private set; }
//    public bool Handbrake { get; private set; }
//    public float Clutch { get; private set; }
//    public bool HybridBoost { get; private set; }

//    [SerializeField] AnimationCurve _turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

//    void Start()
//    {
//        this.InputActive = false;
//        this.InputType = PlayerInputTypes.Keyboard;
//        this.Throttle = 0f;
//        this.Brake = 0f;
//        this.Steering = 0f;
//        this.GearUp = false;
//        this.GearDown = false;
//        this.Handbrake = false;
//        this.Clutch = 1f;
//        this.HybridBoost = false;
//    }

//    void Update()
//    {
//        this.ThrottleInput();
//        this.BrakesInput();
//        this.SteeringInput();
//        this.GearChangeInput();
//        this.HandbrakeInput();
//        this.ClutchInput();
//        this.HybridBoostInput();
//    }

//    private void ThrottleInput()
//    {
//        if (this.InputType == PlayerInputTypes.Keyboard)
//        {
//            bool input = Input.GetKey(KeyCode.UpArrow);
//            this.Throttle = input == true ? 1 : 0;
//        }
//        else
//        {
//            return;
//            float input = CrossPlatformInputManager.GetAxis("Vertical");
//            this.Throttle = input > 0f ? input : 0f;
//        }
//    }

//    private void BrakesInput()
//    {
//        if (this.InputType == PlayerInputTypes.Keyboard)
//        {

//            bool input = Input.GetKey(KeyCode.DownArrow);
//            this.Brake = input == true ? 1 : 0;
//        }
//        else
//        {
//            return;
//            float input = CrossPlatformInputManager.GetAxis("Vertical");
//            this.Brake = input < 0f ? input : 0f;
//        }
//    }

//    private void SteeringInput()
//    {
//        if (this.InputType == PlayerInputTypes.Keyboard)
//        {
//            float input = Input.GetAxis("Horizontal");
//            this.Steering = this._turnInputCurve.Evaluate(input);
//        }
//        else
//        {
//            return;
//            float input = Input.GetAxisRaw("Horizontal"); // Different Axis
//            this.Steering = input;
//        }
//    }

//    private void GearChangeInput()
//    {
//        if (this.InputType == PlayerInputTypes.Keyboard)
//        {
//            if (Input.GetKeyDown(KeyCode.S))
//            {
//                this.GearUp = true;
//                this.GearDown = false;
//            }
//            else if (Input.GetKeyDown(KeyCode.X))
//            {
//                this.GearUp = false;
//                this.GearDown = true;
//            }
//            else
//            {
//                this.GearUp = false;
//                this.GearDown = false;
//            }
//        }
//        else if (this.InputType == PlayerInputTypes.Gamepad)
//        {
//            return;
//            if (Input.GetButtonDown("gearUp"))
//            {
//                this.GearUp = true;
//                this.GearDown = false;
//            }
//            else if (Input.GetButtonDown("gearDown"))
//            {
//                this.GearUp = false;
//                this.GearDown = true;
//            }
//            else
//            {
//                this.GearUp = false;
//                this.GearDown = false;
//            }
//        }
//    }

//    private void HandbrakeInput()
//    {
//        if (this.InputType == PlayerInputTypes.Keyboard)
//        {
//            bool input = Input.GetKey(KeyCode.Space);
//            this.Handbrake = input;
//        }
//        else
//        {
//            return;
//            bool input = Input.GetButton("handbrakeInput");
//            this.Handbrake = input;
//        }
//    }

//    private void ClutchInput()
//    {
//        if (this.InputType == PlayerInputTypes.Keyboard)
//        {
//            bool input = Input.GetKeyDown(KeyCode.Z);
//            this.Clutch = input ? 0f : 1f;
//        }
//        else
//        {
//            return;
//            bool input = Input.GetButtonDown("clutchButton");
//            this.Clutch = input ? 0f : 1f;
//        }
//    }

//    private void HybridBoostInput()
//    {
//        if (this.InputType == PlayerInputTypes.Keyboard)
//        {
//            bool input = Input.GetKey(KeyCode.A);
//            this.HybridBoost = input;
//        }
//        else
//        {
//            return;
//            bool input = Input.GetButton("boostButton");
//            this.HybridBoost = input;
//        }
//    }
//}
