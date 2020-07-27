using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airplane : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public Vector3 direction;

    [HideInInspector] public Transform from;
    [HideInInspector] public Transform to;
    [HideInInspector] public float flightTime;

    public float startTime;

    private void Start() {
        startTime = Time.time;
        Invoke("Despawn", flightTime + 1f);
    }

    private void Despawn() {
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        var dir = to.position - from.position;
        float fracComplete = (Time.time - startTime) / flightTime;
        transform.position = Vector3.Lerp(from.position, to.position, fracComplete);
    }
}
