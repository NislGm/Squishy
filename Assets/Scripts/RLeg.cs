using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nisl
{
    public class RLeg : MonoBehaviour
    {
        private RController _rController = null;
        private Transform _bodyRef = null;

        private Transform _socket, _bone, _contract, _ballJoint, _toNormal, _tip, _volume;

        private IEnumerator _routine = null;
        private RaycastHit _tipRaycast;
        private RaycastHit _socketRaycast;

        
        private Quaternion _boneRotation = Quaternion.identity;
        private Quaternion _contractRotation = Quaternion.identity;
        private Quaternion _contractRotationOffset = Quaternion.identity;
        private Quaternion _volumeRotation = Quaternion.identity;
        private Quaternion _ballJointRotation = Quaternion.identity;
        private Quaternion _toNormalRotation = Quaternion.identity;
        private Quaternion _toTipRotation = Quaternion.identity;
        
        private Vector3 _contractPos = Vector3.zero;
        private Vector3 _socketTarget = Vector3.zero;
        private Vector3 _fromPosition = Vector3.zero;
        private Vector3 _targetPosition = Vector3.zero;
        private Vector3 _restPosition = Vector3.zero;
        private Vector3 _currentWorld = Vector3.zero;
        private Vector3 _tipTarget = Vector3.zero;
        private Vector3 _tipNormal = Vector3.zero;
        private Vector3 _rayOffset = new Vector3(0f, 0.6f, 0f);
        private Vector3 _lastPosition;
        

        private Transform _rootTarget = null;

        private float _contractRestDist;
        private static readonly float PI = Mathf.PI;
        private static readonly float DOUBLE_PI = 6.28f;

        private int _groundMask = 1 << 8;

        void Awake()
        {
            _socket = transform.GetChild(0);
            _bone = _socket.GetChild(0);
            _contract = _bone.GetChild(0);
            _ballJoint = _contract.GetChild(0);
            _toNormal = _ballJoint.GetChild(0);
            _tip = _toNormal.GetChild(0);
            _volume = _bone.GetChild(0); 
        }

        public void Initialize(RController rController, Transform t)
        {
            _rController = rController;
            _bodyRef = t;

            _lastPosition = _socket.position;
            _restPosition = _socket.InverseTransformPoint(_tip.position);
            _tipTarget = _restPosition;
            _currentWorld = _tip.position;
            _contractRestDist = (_tip.position - _contract.position).magnitude;  

            StartCoroutine(DoLegMove(0f));
        }

        public void MoveLeg(float duration)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = DoLegMove(duration);
            StartCoroutine(_routine);
        }

        private IEnumerator DoLegMove(float duration)
        {
            float t = 0f;
            float sineHalf;

            _fromPosition =  _tipTarget;
            _targetPosition = _socket.TransformPoint(_restPosition);

            if((_fromPosition - _targetPosition).magnitude < 0.2f)
                yield break;

            Vector3 stepOffset = _rController.delta.normalized * -0.5f;
            stepOffset.y = 0f;
            if (Physics.Raycast(_targetPosition + _rayOffset + stepOffset, -_bodyRef.up, out _tipRaycast, 2f, _groundMask))
            {
                _tipTarget = _tipRaycast.point;
                _tipNormal = _tipRaycast.normal;
            }

            Vector3 delta = _socket.position;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                if (t > 1f)
                    t = 1f;
                sineHalf = Mathf.Sin(t * PI);
                
                _currentWorld = Vector3.Lerp(_fromPosition, _tipTarget + _socket.position - delta, t);
                _contractRotationOffset = Quaternion.AngleAxis(sineHalf * -145, Vector3.right);
                _toNormalRotation = Quaternion.LookRotation(_tipNormal, Vector3.up);

                yield return null;
            }
        }

        private void LateUpdate()
        {
            // measure if too far away

            if (Physics.Raycast(_socket.position, -_socket.up, out _socketRaycast, 2f, _groundMask))
            {
                _socketTarget = _socketRaycast.point;
            }

            _boneRotation = Quaternion.LookRotation(_currentWorld - _socket.position, _socket.up);
            _contractPos = (_currentWorld - _socketTarget).normalized * _contractRestDist;

            _toTipRotation = Quaternion.LookRotation(_contractPos, _socket.up);
            _contractRotation = _toTipRotation * _contractRotationOffset;
            _ballJointRotation = _socket.rotation * _contractRotationOffset;
            _volumeRotation = Quaternion.Slerp(_toTipRotation, _contractRotation, 0.33f);
            
            
            _bone.rotation = _boneRotation;
            _contract.position = _currentWorld - _contractPos;
            _contract.rotation = _contractRotation;
            _ballJoint.rotation = _ballJointRotation;
            _toNormal.rotation = _toNormalRotation;
            _volume.rotation = _volumeRotation;

            Debug.DrawLine(_socket.position, _currentWorld, Color.red);
            Debug.DrawLine(_currentWorld, _contract.position, Color.blue);
            Debug.DrawLine(_contract.position, _socket.position, Color.yellow);
        }
    }
}