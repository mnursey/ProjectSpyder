using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

public class BulletTrailController : MonoBehaviour
{
    [Header("General Bullet Settings")]
    public GameObject particleHitPrefab;
    public float bulletSpeed = 10.0f;
    public float objDestroyDelayAfterHit = 3.0f;

    [Header("General Particle Settings")]
    public float pInitialSize = 0.2f;
    public float pLifetime = 3.0f;
    public float pEmissionRateOverDistance = 25;
    public float pEmissionRadius = 0.01f;
    public bool pAlignRotationToDirection = false;

    [Header("A Bunch More Particle Settings")]
    public Gradient pColorOverLifetime;
    public ParticleSystem.MinMaxCurve pSizeOverLifetime;
    public ParticleSystem.MinMaxCurve pForwardVelocityOverLifetime;
    public bool pNoiseEnabled = true;
    public float pNoiseFrequency = 0.5f;
    public float pNoiseStrength = 0.1f;
    public ParticleSystemNoiseQuality pNoiseQuality = ParticleSystemNoiseQuality.High;
    public bool pTrailsEnabled = false;
    public ParticleSystemTrailMode pTrailMode = ParticleSystemTrailMode.Ribbon;
    public Material particleMaterial;
    public Material trailMaterial;
    public ParticleSystemRenderMode pRenderMode = ParticleSystemRenderMode.Billboard;
    public Mesh pRenderMesh;
    public ShadowCastingMode pShadowCastingMode = ShadowCastingMode.Off;

    // Private particle stuff
    private ParticleSystem particles;
    private ParticleSystemRenderer pRenderer;
    private bool bHaveSystem = false;

    // Data about trajectory
    Vector3 beginPos;
    Vector3 endPos;
    float iterationsToHit;
    int currentIteration = 1;
    bool hit = false;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        if(particles != null)
        {
            bHaveSystem = true;
            // Init main settings
            var pMainSettings = particles.main;
            pMainSettings.startLifetime = pLifetime;
            pMainSettings.startSize = pInitialSize;

            // Init emitter settings
            var pEmissionSettings = particles.emission;
            pEmissionSettings.rateOverDistance = pEmissionRateOverDistance;

            // Init shape settings
            var pShapeSettings = particles.shape;
            pShapeSettings.radius = pEmissionRadius;
            pShapeSettings.alignToDirection = pAlignRotationToDirection;

            // Init velocity over lifetime
            var pVelocityOverLifetime = particles.velocityOverLifetime;
            pVelocityOverLifetime.z = pForwardVelocityOverLifetime;

            // Init color settings
            var pColorOverLifetimeSettings = particles.colorOverLifetime;
            pColorOverLifetimeSettings.color = pColorOverLifetime;

            // Init size over lifetime
            var pSizeOverLifetimeSettings = particles.sizeOverLifetime;
            pSizeOverLifetimeSettings.z = pSizeOverLifetime;

            // Init noise settings
            var pNoiseSettings = particles.noise;
            pNoiseSettings.enabled = pNoiseEnabled;
            pNoiseSettings.frequency = pNoiseFrequency;
            pNoiseSettings.strength = pNoiseStrength;
            pNoiseSettings.quality = pNoiseQuality;

            // Init trails settings
            var pTrailSettings = particles.trails;
            pTrailSettings.enabled = pTrailsEnabled;
            pTrailSettings.mode = pTrailMode;

            // Init render settings
            pRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
            pRenderer.renderMode = pRenderMode;
            pRenderer.material = particleMaterial;
            pRenderer.trailMaterial = trailMaterial;
            pRenderer.shadowCastingMode = pShadowCastingMode;
        }
    }


    // Figure out when to stop the moving the object
    public void Init(Vector3 begin, Vector3 end)
    {
        beginPos = begin;
        endPos = end;
        float distance = Vector3.Distance(begin, end);
        iterationsToHit = distance / bulletSpeed;
    }


    private void FixedUpdate()
    {
        if (currentIteration < iterationsToHit)
        {
            Vector3 forward = transform.forward;
            Vector3 pos = transform.position;
            pos += forward * bulletSpeed;
            transform.position = pos;
        }
        else if(currentIteration >= iterationsToHit)
        {
            if (!hit)
            {
                Instantiate(particleHitPrefab, endPos, Quaternion.identity);
                transform.position = endPos;
                hit = true;
                Destroy(gameObject, objDestroyDelayAfterHit);
            }
        }
        currentIteration++;
    }
}
