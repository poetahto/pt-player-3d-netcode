using FishNet.Object.Prediction;
using UnityEngine;

namespace FishNet.Example.Prediction.CharacterControllers
{
    // The data sent back to the client from the server, correcting any mistakes in the prediction.
    public struct PlayerReconcileState : IReconcileData
    {
        public Vector3 Position;
        public Vector3 Velocity;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
}
