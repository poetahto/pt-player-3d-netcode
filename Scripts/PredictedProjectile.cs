using UnityEngine;

namespace DefaultNamespace
{
    public class PredictedProjectile : MonoBehaviour
    {
        [SerializeField]
        private float speed = 5f;

        [SerializeField]
        private bool autoUpdate = true;

        private Vector3 _direction;
        private float _passedTime;

        public void Initialize(Vector3 direction, float passedTime)
        {
            _direction = direction;
            _passedTime = passedTime;

            transform.forward = direction;
        }

        private void Update()
        {
            if (autoUpdate)
                Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            float passedTimeDelta = 0;

            if (_passedTime > 0f)
            {
                float step = _passedTime * 0.08f;
                _passedTime -= step;

                if (_passedTime <= deltaTime / 2f)
                {
                    step += _passedTime;
                    _passedTime = 0f;
                }

                passedTimeDelta = step;
            }

            // Vector3 velocity = _direction * speed;
            transform.position += _direction * (speed * (deltaTime + passedTimeDelta));
        }
    }
}
