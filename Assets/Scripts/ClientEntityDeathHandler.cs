using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientEntityDeathHandler : MonoBehaviour
{
    GameObject clientController;
    ClientGameRunner gameRunner;
    MeshRenderer meshRenderer;
    Rigidbody rb;

    bool isDestroyed = false; // Just so that we only call destroy once

    public float explosionForce = 100.0f;
    public float explosionRadius = 5.0f;
    public List<GameObject> fragmentPrefabs;
    public List<Vector3> fragmentPositionOffsets;
    public GameObject explosionParticlePrefab;
    public GameObject explosionAudioPrefab;

    // Start is called before the first frame update
    void Start()
    {
        clientController = GameObject.Find("ClientController");
        if(clientController != null)
        {
            gameRunner = clientController.GetComponent<ClientGameRunner>();
        }
        rb = GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    void Update()
    {
        //if(gameRunner != null)
        //{
            //if(gameRunner.em.getentity(gameobject).health <= 0)
        if(Time.frameCount > 300 && !isDestroyed)
        {
            isDestroyed = true;
            Destroy();
        }
        //}
    }


    private void Destroy()
    {
        SpawnFragmentPrefabs();
        gameObject.SetActive(false);
    }


    private void SpawnFragmentPrefabs()
    {
        if (fragmentPrefabs.Count == fragmentPositionOffsets.Count)
        {
            for (int i = 0; i < fragmentPrefabs.Count; i++)
            {
                if (rb != null)
                {
                    Vector3 vehiclePosition = GetVehiclePosition();
                    Vector3 prefabSpawnPosition = vehiclePosition + fragmentPositionOffsets[i];
                    Vector3 explosionForceOrigin = vehiclePosition + new Vector3(0.0f, -1.0f, 0.0f);
                    GameObject fragObj = Instantiate(fragmentPrefabs[i], prefabSpawnPosition, transform.rotation);
                    fragObj.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, explosionForceOrigin, explosionRadius);
                    if (explosionParticlePrefab != null)
                    {
                        Instantiate(explosionParticlePrefab, vehiclePosition, Quaternion.identity);
                    }
                    if(explosionAudioPrefab != null)
                    {
                        Instantiate(explosionAudioPrefab, vehiclePosition, Quaternion.identity);
                    }
                }
                else
                {
                    Debug.LogError("Unable to spawn destroyed vehicle because no rigidbody found!");
                }
            }
        }
        else
        {
            Debug.LogError("Unable to spawn destroyed vehicle because not all fragments has position offsets!");
        }
    }


    private Vector3 GetVehiclePosition()
    {
        if(rb == null)
        {
            return Vector3.zero;
        }
        return rb.position + rb.centerOfMass;
    }
}
