using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateY : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;

    void Update()
    {
        transform.Rotate(Vector3.up * _speed, Space.World);
    }
}
