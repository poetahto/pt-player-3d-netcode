using FishNet;
using FishNet.Managing.Timing;
using poetools.Core.Abstraction;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(CharacterController))]
    public class FishNetCharacterControllerWrapper : PhysicsComponent
    {
        private CharacterController _character;
        private TimeManager _timeManager;

        private void Start()
        {
            _character = GetComponent<CharacterController>();
            _timeManager = InstanceFinder.TimeManager;
        }

        private void Update()
        {
            _character.Move(Velocity * (float) _timeManager.TickDelta);

            if (Mathf.Round(_character.velocity.sqrMagnitude) < Mathf.Round(Velocity.sqrMagnitude))
                Velocity = _character.velocity;

            Physics.SyncTransforms();
        }

        public override Vector3 Velocity { get; set; }
    }
}
