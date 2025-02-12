using UnityEngine;

namespace Kamgam.UGUIWorldImage.Examples
{
    public class LinearMovement : MonoBehaviour
    {
        public Vector3 Velocity = new Vector3(15f, 0f, 0f);
        public float Limit = 20f;

        protected Vector3 _startPos;

        public void Start()
        {
            _startPos = transform.localPosition;
        }

        public void Update()
        {
            var pos = transform.localPosition;
            pos += Velocity * Time.deltaTime;

            if (Mathf.Abs(pos.x - _startPos.x) > Limit)
            {
                Velocity.x *= -1f;
                pos.x = _startPos.x - Limit * Mathf.Sign(Velocity.x);
            }
            if (Mathf.Abs(pos.y - _startPos.y) > Limit)
            {
                Velocity.y *= -1f;
                pos.y = _startPos.y - Limit * Mathf.Sign(Velocity.y);
            }
            if (Mathf.Abs(pos.z - _startPos.z) > Limit)
            {
                Velocity.z *= -1f;
                pos.z = _startPos.z - Limit * Mathf.Sign(Velocity.z);
            }

            pos += Velocity * Time.deltaTime * 0.1f;

            transform.localPosition = pos;
        }

    }
}
