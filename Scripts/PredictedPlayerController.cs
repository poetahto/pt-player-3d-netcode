using DefaultNamespace;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using pt_player_3d.Scripts;
using pt_player_3d.Scripts.Movement;
using pt_player_3d.Scripts.Rotation;
using UnityEngine;
using UnityEngine.Serialization;

namespace FishNet.Example.Prediction.CharacterControllers
{
    // The data sent from a client to the server, informing it about some actions it will take.
    public struct PlayerPredictionState : IReplicateData
    {
        public float Horizontal;
        public float Vertical;
        public float Yaw;
        public bool IsJumpQueued;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    // The data sent back to the client from the server, correcting any mistakes in the prediction.
    public struct PlayerReconcileState : IReconcileData
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public PlayerReconcileState(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    /// <summary>
    /// Intercepts input events, and applies client-side prediction to them through FishNet.
    /// </summary>
    public class PredictedPlayerController : NetworkBehaviour, IJumpingSystem, IMovementSystem, IRotationSystem
    {
        [SerializeField]
        private float moveRate = 5f;

        [SerializeField]
        private float acceleration = 25;

        [SerializeField]
        private float deceleration = 7;

        [SerializeField]
        private float jumpSpeed = 5f;

        [SerializeField]
        private float gravity = 1;

        [SerializeField]
        private Camera localCamera;

        [SerializeField]
        private CharacterControllerWrapper character;

        private float _pitch;
        private float _yaw;
        private float _horizontal;
        private float _vertical;
        private bool _isJumpQueued;

        protected override void OnValidate()
        {
            base.OnValidate();

            // This script aggressively controls the update timing for most things.
            if (character != null) character.autoUpdate = false;
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
                Yaw = _yaw,
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

        [Replicate]
        private void Move(PlayerPredictionState state, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
        {
            float deltaTime = (float)TimeManager.TickDelta;
            Vector3 direction = new Vector3(state.Horizontal, 0f, state.Vertical).normalized;
            direction = Quaternion.Euler(0, state.Yaw, 0) * direction;
            Vector3 targetVelocity = direction * moveRate;
            Vector3 currentVelocity = character.Velocity;
            float originalY = currentVelocity.y;

            if (state.Horizontal != 0 && state.Vertical != 0)
            {
                // Acceleration
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * deltaTime);
            }
            else
            {
                // Deceleration
                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deceleration * deltaTime);
            }

            currentVelocity.y = originalY; // Ensure we dont accelerate upward / downwards, that is only for jumping / gravity

            // Jumping
            if (state.IsJumpQueued)
                currentVelocity.y = jumpSpeed;

            // Gravity
            currentVelocity += new Vector3(0f, gravity, 0f);

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

        public Vector3 Rotation
        {
            get => new Vector3(_pitch, _yaw, 0);
            set
            {
                if (IsOwner)
                {
                    _yaw = value.y;
                    _pitch = value.x;
                    _pitch = Mathf.Clamp(_pitch, -90, 90);
                    localCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
                }
            }
        }

        public void ApplyRotationInput(Vector2 delta)
        {
            Rotation += new Vector3(delta.y, delta.x, 0);
        }

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
