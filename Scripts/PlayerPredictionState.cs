using FishNet.Object.Prediction;
using UnityEngine;

namespace FishNet.Example.Prediction.CharacterControllers
{
    // The data sent from a client to the server, informing it about some actions it will take.
    public struct PlayerPredictionState : IReplicateData
    {
        public float Horizontal;
        public float Vertical;
        public float Yaw;
        public bool IsJumpQueued;

        public Vector3 ReadInputDirection()
        {
            return new Vector3(Horizontal, 0, Vertical).normalized;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
}
