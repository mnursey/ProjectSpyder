
using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    class CameraState
    {
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t)
        {
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation)
        {
            x += translation.x;
            y += translation.y;
            z += translation.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct)
        {
            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t)
        {
            t.position = new Vector3(x, y, z);
        }
    }


    CameraState m_TargetCameraState = new CameraState();
    CameraState m_InterpolatingCameraState = new CameraState();

    [Header("Movement Settings")]
    [Tooltip("Basically controls how fast to move.")]
    public float speedMultiplier = 5.0f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.5f;

    [Tooltip("Target distance to maintain from ground (or other object directly below the camera)")]
    public float targetDistanceFromGround = 50.0f;

    [Tooltip("Slows the vertical movement speed")]
    public float groundDistanceDamping = 0.1f;

    public const float maxDistanceFromGround = 100.0f;

    void OnEnable()
    {
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
    }

    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        return direction;
    }

    // Return the distance from the camera to the first object directly beneath the camera
    float GetDistanceFromGround()
    {
        Vector3 rayBeginPos = this.transform.position;
        RaycastHit hitResult = new RaycastHit();
        bool bHit = Physics.Raycast(rayBeginPos, Vector3.down, out hitResult, Mathf.Infinity);
        if (bHit)
        {
            return hitResult.distance;
        }
        return -1.0f;
    }


    void Update()
    {
        // Get the desired translation based on input and apply the speed multiplier
        Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;
        translation *= speedMultiplier;

        // Maintain distance from ground by altering vertical translation component
        float distanceFromGround = GetDistanceFromGround();
        if(distanceFromGround != -1) 
        {
            float distanceFromGroundError = targetDistanceFromGround - distanceFromGround;
            translation.y += distanceFromGroundError * groundDistanceDamping;
        }

        // Do some nice lerping
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        m_TargetCameraState.Translate(translation);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct);
        m_InterpolatingCameraState.UpdateTransform(transform);
    }
}
