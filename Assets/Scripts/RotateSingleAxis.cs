using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSingleAxis : MonoBehaviour
{
    [SerializeField] private Transform _anchor;
    [SerializeField] private Transform _lookTarget;

    void Update()
    {
        var viewDirection = _lookTarget.position - transform.position;
        var rotation = Quaternion.LookRotation(viewDirection).eulerAngles;
        //transform.rotation = _anchor.rotation *

        Quaternion r = Quaternion.Euler(0f, rotation.y, 0f);

        transform.localRotation = r;
    }
}