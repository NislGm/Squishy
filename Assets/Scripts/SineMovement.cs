using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : MonoBehaviour
{
    public float speed = 1f;
    public float range = 2f;
    public float rotSpeed = 10f;
    public float rotRange = 10f;

    void Update()
    {
        Vector3 vec = new Vector3(Mathf.Sin(Time.time * speed) * range, 0f, Mathf.Cos(Time.time * speed) * 2);

        transform.position = vec;
        transform.rotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * rotSpeed) * rotRange, vec * -1);
    }
}
