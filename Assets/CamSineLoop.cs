using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSineLoop : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    
    public float range = 2f;
    public float distance = 2f;
    public float viewOffsetScale = 1f;
    public Vector3 offset = Vector3.one;

    private Transform _transform;
    private Vector3 pos = Vector3.zero;

    
    
    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        pos.x = Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * speed)) * range;
        pos.y = 0f;
        pos.z = Mathf.Abs(Mathf.Cos(Time.time * Mathf.PI * speed)) * range;
        
        _transform.position = pos + offset;
        
        Vector3 targetDirection = target.position - _transform.position + offset * viewOffsetScale;
        _transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
    }
}