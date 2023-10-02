using FishNet.Object;
using UnityEngine;

namespace DefaultNamespace
{
    public class PredictedProjectileLauncher : NetworkBehaviour
    {
        private const float MaxPassedTime = 0.3f;

        [SerializeField]
        private Transform sourceTransform;

        [SerializeField]
        private PredictedProjectile projectilePrefab;

        [Client(RequireOwnership = true)]
        public void ClientFire()
        {
            Vector3 pos = sourceTransform.position;
            Vector3 dir = sourceTransform.forward;

            if (IsClientOnly)
                SpawnProjectile(pos, dir, 0f);

            Rpc_ServerFire(pos, dir, TimeManager.Tick);
        }

        [ServerRpc(RequireOwnership = true)]
        private void Rpc_ServerFire(Vector3 position, Vector3 direction, uint tick)
        {
            // Safety checks.
            direction.Normalize();

            float passedTime = (float) TimeManager.TimePassed(tick);
            passedTime = Mathf.Min(MaxPassedTime / 2f, passedTime);

            SpawnProjectile(position, direction, passedTime);
            Rpc_ObserversFire(position, direction, tick);
        }

        [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
        private void Rpc_ObserversFire(Vector3 position, Vector3 direction, uint tick)
        {
            float passedTime = (float) TimeManager.TimePassed(tick);
            passedTime = Mathf.Min(MaxPassedTime, passedTime);

            SpawnProjectile(position, direction, passedTime);
        }

        private void SpawnProjectile(Vector3 position, Vector3 direction, float passedTime)
        {
            PredictedProjectile projectileInstance = Instantiate(projectilePrefab, position, Quaternion.identity);
            projectileInstance.Initialize(direction, passedTime, NetworkObject);
        }
    }
}
