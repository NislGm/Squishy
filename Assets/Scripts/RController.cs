using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nisl
{
    public class RController : MonoBehaviour
    {
        [SerializeField] private RLeg[] _rLegs;
        [HideInInspector] public Vector3 _direction = Vector3.zero;

        [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;

        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _moveInterval = 2f;
        [SerializeField] private float _moveDuration = 2f;

        [HideInInspector] public Vector3 velocity, delta;

        private Vector3 desiredVelocity, moveToXZ;
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

            _transform = transform;
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

            // if (gamepad.rightTrigger.wasPressedThisFrame)
            // {
            //     Debug.Log("TRigger");
            // }

            Vector2 move = gamepad.leftStick.ReadValue();
            moveToXZ.x = move.x;
            moveToXZ.z = move.y;
            desiredVelocity = _transform.TransformDirection(moveToXZ) * maxSpeed;

            // velocity.x = move.x * _speed * Time.deltaTime;
            // velocity.z = move.y * _speed * Time.deltaTime;
            //_rgbd.position += _rgbd.TransformDirection(velocity);



            delta = _lastPosition - body.position;
            _lastPosition = body.position;
            //Debug.Log(velocity.sqrMagnitude);
            //_transform.rotation = Quaternion.identity * Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(velocity.normalized, _transform.up), Time.deltaTime * 1);

            _timer += Time.deltaTime / _moveInterval;
            if (_timer > 1f)
            {
                _timer -= 1f;

                for (var i = 0; i < _rLegs.Length; i++)
                {
                    if ((lastLegIndex + i) % legIndie == 0)
                    {
                        _rLegs[i].MoveLeg(_moveDuration);
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