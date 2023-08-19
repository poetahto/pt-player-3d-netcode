using FishNet.Object;
using UnityEngine;

namespace DefaultNamespace
{
    public class OwnerOnly : NetworkBehaviour
    {
        [SerializeField] private Behaviour[] behaviors;
        [SerializeField] private GameObject[] objects;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            foreach (var behaviour in behaviors)
                behaviour.enabled = Owner.IsLocalClient;

            foreach (var obj in objects)
                obj.SetActive(Owner.IsLocalClient);
        }
    }
}
