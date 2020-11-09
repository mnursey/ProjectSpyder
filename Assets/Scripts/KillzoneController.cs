using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KillzoneController : MonoBehaviour
{
    float height = 100.0f;
    float radius = 0.0f;
    float minRadius = 0.0f;
    float damagePerSecond = 0.0f;
    float zoneDecreaseRate = 0.0f;
    MeshFilter mesh;
    MeshRenderer meshRenderer;

    /* For testing
    private void Start()
    {
        Init_Client(10.0f, 0.0f, 0.1f, 0.1f);
    }
    private void Update()
    {
        DecreaseZone_Client();
    }
    */

    public void Init_Server(float radius, float minRadius, float damagePerSecond, float zoneDecreaseRate)
    {
        this.radius = radius;
        this.minRadius = minRadius;
        this.damagePerSecond = damagePerSecond;
        this.zoneDecreaseRate = zoneDecreaseRate;
    }

    public void Init_Client(float radius, float minRadius, float damagePerSecond, float zoneDecreaseRate)
    {
        Init_Server(radius, minRadius, damagePerSecond, zoneDecreaseRate);

        // Make sure we have mesh and renderer components
        mesh = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if(mesh == null)
        {
            mesh = gameObject.AddComponent<MeshFilter>();
        }
        if(meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Load our mesh and materials
        mesh.mesh = Resources.Load<Mesh>(@"KillzoneMesh");
        meshRenderer.sharedMaterial = Resources.Load<Material>(@"Mat_KillZone");
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        if (mesh.mesh == null || meshRenderer.sharedMaterial == null)
        {
            Debug.LogError("KillzoneController - unable to load killzone mesh or material!");
        }

        // Center the position and set size based on radius
        transform.position = Vector3.zero;
        transform.localScale = new Vector3(radius, height, radius);
    }

    // I don't actually know how passing by ref works in c#. Does this make any sense at all?
    void ApplyDamage(ref List<IEntity> entities)
    {
        foreach(IEntity entity in entities)
        {
            Vector3 entityPos = entity.gameObject.transform.position;
            if(entityPos.magnitude >= radius)
            {
                //entity.ApplyDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    void DecreaseZone_Server()
    {
        if(radius > minRadius)
        {
            radius -= zoneDecreaseRate * Time.deltaTime;
        }
    }

    void DecreaseZone_Client()
    {
        DecreaseZone_Server();
        transform.localScale = new Vector3(radius, height, radius);
    }

    float GetRadius()
    {
        return radius;
    }

    // Returns true if the killzone is at its smallest size
    bool IsKillzoneClosed()
    {
        if(radius <= minRadius)
        {
            return true;
        }
        return false;
    }
}
