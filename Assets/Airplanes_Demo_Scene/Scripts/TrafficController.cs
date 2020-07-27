using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    private List<GameObject> airports;

    [SerializeField] private float flightCooldown = 1f;
    [SerializeField] private GameObject airplanePrefab;

    private System.Random rng = new System.Random();

    public void StoreAirports(List<GameObject> airports) {
        this.airports = airports;
        StartCoroutine(SpawnAirplane());
    }

    private IEnumerator SpawnAirplane() {
        var from = airports[rng.Next(airports.Count - 1)];
        var to = airports[rng.Next(airports.Count - 1)];
        //TODO: Could get the same airport here

        var airplane = Instantiate(airplanePrefab, from.transform.position, Quaternion.identity);
        var planeScript = airplane.GetComponent<Airplane>();
        planeScript.speed = 0.2f;
        planeScript.from = from.transform;
        planeScript.to = to.transform;
        planeScript.flightTime = 5f;

        yield return new WaitForSeconds(flightCooldown);
        StartCoroutine(SpawnAirplane());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
