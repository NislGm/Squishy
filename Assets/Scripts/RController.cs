using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nisl
{
    public class RController : MonoBehaviour
    {
        [SerializeField] private Transform playerInputSpace = default;
        [SerializeField] private RLeg[] _rLegs;

        [SerializeField, Range(0f, 20f)] private float maxSpeed = 10f;
        [SerializeField, Range(0f, 20f)] private float maxAcceleration = 10f;

        [SerializeField] private float _minMoveInterval = 0.5f;
        [SerializeField] private float _maxMoveInterval = 1f;
        [SerializeField] private float _minMoveDuration = 0.5f;
        [SerializeField] private float _maxMoveDuration = 1f;

        [HideInInspector] public Vector3 velocity, delta;

        private Vector3 desiredVelocity, desiredAngularVelocity;
        private Vector3 _lastPosition;
        private float _timer = 0f;
        private int lastLegIndex = 0;
        private int legIndie = 4;

        private Rigidbody body;
        private Transform _transform;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            _lastPosition = body.position;

            _transform = transform.GetChild(0);
        }

        void Start()
        {
            for (var i = 0; i < _rLegs.Length; i++)
            {
                _rLegs[i].Initialize(this, gameObject.transform);
            }
        }

        void Update()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
                return; // No gamepad connected.

            Vector2 move = gamepad.leftStick.ReadValue();

            if (playerInputSpace)
            {
                Vector3 forward = playerInputSpace.forward;
                forward.y = 0f;
                forward.Normalize();
                Vector3 right = playerInputSpace.right;
                right.y = 0f;
                right.Normalize();
                desiredVelocity =
                    (forward * move.y + right * move.x) * maxSpeed;
            }
            else
            {
                desiredVelocity.x = move.x * maxSpeed;
                desiredVelocity.y = move.y * maxSpeed;
            }

            delta = _lastPosition - body.position;
            _lastPosition = body.position;

            float ratio = Mathf.Clamp01(move.magnitude);

            _transform.rotation = Quaternion.identity * Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(velocity.normalized, _transform.up), Time.deltaTime * 5);

            _timer += Time.deltaTime / Mathf.Lerp(_maxMoveInterval, _minMoveInterval, ratio);
            if (_timer > 1f)
            {
                _timer -= 1f;

                for (var i = 0; i < _rLegs.Length; i++)
                {
                    if ((lastLegIndex + i) % legIndie == 0)
                    {
                        _rLegs[i].MoveLeg(Mathf.Lerp(_maxMoveDuration, _minMoveDuration, ratio));
                    }
                }

                lastLegIndex++;
                if (lastLegIndex > legIndie - 1) lastLegIndex = 0;
            }
        }

        void FixedUpdate()
        {
            velocity = body.velocity;
            float maxSpeedChange = maxAcceleration * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            body.velocity = velocity;
        }
    }
}