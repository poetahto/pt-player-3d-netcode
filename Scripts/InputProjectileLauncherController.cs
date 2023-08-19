using FishNet.Object;
using UnityEngine;

namespace DefaultNamespace
{
    public class InputProjectileLauncherController : NetworkBehaviour
    {
        public PredictedProjectileLauncher launcher;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1) && IsOwner)
                launcher.ClientFire();
        }
    }
}