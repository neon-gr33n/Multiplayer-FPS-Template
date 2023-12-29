using UnityEngine;

namespace AlterunaFPS
{
    public partial class PlayerController
    {
        [Header("Sliding")]
        [Tooltip("The reduced height of the characters collider")]
        public float ReducedHeight;

        [Tooltip("The speed of sliding as m/s")]
        public float SlideSpeed;


        [Tooltip("The duration of sliding in m/s")]
        public float SlideTimer = 0.75f;

        [Tooltip("Is the player currently sliding?")]
        public bool IsSliding;

        private Rigidbody _rb; 
        private void Slide()
        {
            _controller.height = ReducedHeight;

            float horizontal = _horizontal;
            float vertical = _vertical;

            Vector2 inputDirection = new Vector2(horizontal, vertical);

            _rb.AddForce(inputDirection.normalized * MoveSpeed * 15f, ForceMode.Force);
            _rb.AddForce(Vector3.forward * 5f, ForceMode.Impulse);
            IsSliding = true;
        }

        private void Revert()
        {
            _controller.height = _initialHeight;
            IsSliding = false;
        }

    }
}
