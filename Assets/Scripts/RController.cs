using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nisl
{
    public class RController : MonoBehaviour
    {
        [SerializeField] private RLeg[] _rLegs;

        [SerializeField] private float _inputFreqX = 1f;
        [SerializeField] private float _inputFreqY = 1f;
        [HideInInspector] public Vector3 desiredDirection = Vector3.zero;
        [HideInInspector] public Vector3 _direction = Vector3.zero;

        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _moveInterval = 2f;
        [SerializeField] private float _moveDuration = 2f;

        [HideInInspector] public Vector3 velocity = Vector3.zero;

        private bool _offLeg = false;
        private float _timer = 0f;

        private int lastLegIndex = 0;
        private int legIndie = 4;

        private Transform _transform;

        void Awake()
        {
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
            float inputHorizontal = Mathf.PerlinNoise(Time.time * _inputFreqX, Time.time * _inputFreqX) * 2 - 1;
            float inputVertical = Mathf.PerlinNoise(Time.time * _inputFreqY, Time.time * _inputFreqY);

            desiredDirection.x = inputHorizontal;
            desiredDirection.z = inputVertical;
            desiredDirection = desiredDirection.normalized;

            _direction = Vector3.Lerp(_direction, desiredDirection, Time.deltaTime * 5);

            velocity.x = desiredDirection.x * _speed * Time.deltaTime;
            velocity.z = desiredDirection.z * _speed * Time.deltaTime;

            _transform.position += _transform.TransformDirection(velocity);
            //_transform.rotation = Quaternion.identity * Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(_velocity), Time.deltaTime * 3);
            
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
                if(lastLegIndex > legIndie - 1) lastLegIndex = 0;
            }
        }
    }
}
