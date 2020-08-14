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

    [SerializeField] private Transform planeModel = null;

    private float startTime;
    private float maxOffset = 0.1f;

    private void Start() {
        startTime = Time.time;
        planet = GameObject.Find("Earth").transform; //TODO: Don't like this but quick and dirty

        SetInitialHeading();

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
        var planetHit = PlanetUtility.LineFromOriginToSurface(planet, pos, LayerMask.GetMask("Planet"));

        if (planetHit.HasValue) {
            var offsetFrac = fracComplete > 0.5f ? ((1f - fracComplete) * 2f) : fracComplete * 2f;
            transform.position = planetHit.Value.point + pos * (maxOffset * offsetFrac);
            transform.up = planetHit.Value.normal;
        }
    }

    public void SetInitialHeading() {
        planeModel.rotation = Quaternion.LookRotation(to.position - planeModel.position);
    }
}
