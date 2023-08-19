using DefaultNamespace;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using pt_player_3d.Scripts;
using pt_player_3d.Scripts.Movement;
using pt_player_3d.Scripts.Rotation;
using UnityEngine;

namespace FishNet.Example.Prediction.CharacterControllers
{
    /// <summary>
    /// Intercepts input events, and applies client-side prediction to them through FishNet.
    /// Movement and jumping are entirely predicted + reconciled.
    /// Rotation is not reconciled, but is synced for correctly predicting movement directions and such.
    /// </summary>
    public class PredictedPlayerController : NetworkBehaviour, IJumpingSystem, IMovementSystem
    {
        [SerializeField]
        private StandardMovementSystem movementSystem;

        [SerializeField]
        private JumpingSystem jumpingSystem;

        [SerializeField]
        private RotationSystem rotationSystem;

        [SerializeField]
        private CharacterControllerWrapper character;

        [SerializeField]
        private CharacterControllerGroundCheck groundCheck;

        [SerializeField]
        private Camera localCamera;

        // private float _pitch;
        // private float _yaw;
        private float _horizontal;
        private float _vertical;
        private bool _isJumpQueued;

        protected override void OnValidate()
        {
            base.OnValidate();

            // This script aggressively controls the update timing for most things.
            if (character != null) character.autoUpdate = false;
            if (groundCheck != null) groundCheck.autoUpdate = false;
            if (movementSystem != null) movementSystem.autoUpdate = false;
            if (jumpingSystem != null) jumpingSystem.autoUpdate = false;
        }

        private void Awake()
        {
            InstanceFinder.TimeManager.OnTick += OnTick;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
                InstanceFinder.TimeManager.OnTick -= OnTick;
        }

        public override void OnStartClient()
        {
            localCamera.enabled = IsOwner;
            character.enabled = IsServer || IsOwner;
        }

        private void OnTick()
        {
            if (IsOwner)
            {
                Reconciliation(default, false);
                PlayerPredictionState prediction = GetPredictionState();
                Move(prediction, false);
            }
            if (IsServer)
            {
                Move(default, true);
                PlayerReconcileState trueState = GetReconcileState();
                Reconciliation(trueState, true);
            }
        }

        private PlayerPredictionState GetPredictionState()
        {
            var result = new PlayerPredictionState
            {
                Horizontal = _horizontal,
                Vertical = _vertical,
                Yaw = rotationSystem.Yaw,
                IsJumpQueued = _isJumpQueued,
            };

            _isJumpQueued = false;

            return result;
        }

        private PlayerReconcileState GetReconcileState()
        {
            var result = new PlayerReconcileState
            {
                Position = transform.position,
                Velocity = character.Velocity,
            };

            return result;
        }

        // === Movement Sequencing ===
        [Replicate]
        private void Move(PlayerPredictionState state, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
        {
            groundCheck.Tick();
            float deltaTime = (float)TimeManager.TickDelta;

            // Movement
            movementSystem.ApplyMovementInput(state.ReadInputDirection(), state.Yaw);
            movementSystem.Tick(deltaTime);

            // Jumping
            jumpingSystem.IsJumpHeld = state.IsJumpQueued;
            jumpingSystem.Tick(deltaTime);

            // Gravity
            Vector3 currentVelocity = character.Velocity;
            currentVelocity += new Vector3(0f, Physics.gravity.y * deltaTime, 0f);

            // I don't think we need to sync yet, and it could get really slow during replays.
            character.Velocity = currentVelocity;
            character.Tick(deltaTime, shouldSyncPhysics: false);
        }

        [Reconcile]
        private void Reconciliation(PlayerReconcileState state, bool asServer, Channel channel = Channel.Unreliable)
        {
            transform.position = state.Position;
            character.Velocity = state.Velocity;
        }

        // === Input Handlers ===
        public bool IsJumpHeld
        {
            set
            {
                if (IsOwner)
                    _isJumpQueued |= value;
            }
        }

        // public Vector3 Rotation
        // {
        //     get => new Vector3(_pitch, _yaw, 0);
        //     set
        //     {
        //         if (IsOwner)
        //         {
        //             _yaw = value.y;
        //             _pitch = value.x;
        //             _pitch = Mathf.Clamp(_pitch, -90, 90);
        //             localCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
        //         }
        //     }
        // }
        //
        // public void ApplyRotationInput(Vector2 delta)
        // {
        //     Rotation += new Vector3(delta.y, delta.x, 0);
        // }

        public void ApplyMovementInput(Vector3 direction)
        {
            if (IsOwner)
            {
                _vertical = direction.z;
                _horizontal = direction.x;
            }
        }
    }
}
