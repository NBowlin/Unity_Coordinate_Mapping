using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;

public class Airplane : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public Vector3 direction;

    [HideInInspector] public Transform from;
    [HideInInspector] public Transform to;
    [HideInInspector] public float flightTime;

    [HideInInspector] public Transform planet;

    private float startTime;
    private float maxOffset = 0.1f;

    private void Start() {
        startTime = Time.time;
        planet = GameObject.Find("Earth").transform; //TODO: Don't like this but quick and dirty

        Invoke("Despawn", flightTime + 1f);
    }

    private void Despawn() {
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        float fracComplete = (Time.time - startTime) / flightTime;
        if(fracComplete > 1) {
            transform.position = to.position;
            return;
        }
        var pos = Vector3.Lerp(from.position, to.position, fracComplete);
        var ePoint = PlanetUtility.LineFromOriginToSurface(planet, pos, LayerMask.GetMask("Planet"));

        if (ePoint.HasValue) {
            var offsetFrac = fracComplete > 0.5f ? ((1f - fracComplete) * 2f) : fracComplete * 2f;
            transform.position = ePoint.Value.point + pos * (maxOffset * offsetFrac);
        }
    }
}
