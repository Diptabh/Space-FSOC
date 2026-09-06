using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Tracking Target")]
    public Transform target;

    [Header("Pan Settings")]
    public float panSpeed = 2f;
    public float minPan = -180f;
    public float maxPan = 180f;

    [Header("Tilt Settings")]
    public float tiltSpeed = 2f;
    public float minTilt = -60f;
    public float maxTilt = 60f;

    [Header("Smoothing")]
    public float smoothTime = 0.1f;

    [Header("Turbulence")]
    public bool turbulenceEnabled = false;
    [Range(0f, 10f)]
    public float turbulenceStrength = 1f;

    [Range(0.1f, 10f)]
    public float turbulenceFrequency = 2f;

    // Current pan and tilt
    private float currentPan;
    private float currentTilt;

    // Desired pan and tilt from tracking
    private float targetPan;
    private float targetTilt;

    // Smooth movement velocities
    private float panVelocity;
    private float tiltVelocity;

    // Random offsets for Perlin noise
    private float noiseOffsetX;
    private float noiseOffsetY;

    private void Start()
    {
        // Start from the camera's current rotation
        Vector3 angles = transform.localEulerAngles;

        currentPan = angles.y;
        currentTilt = angles.x;

        // Convert Unity's 0-360 representation to -180 to 180
        if (currentPan > 180f)
            currentPan -= 360f;

        if (currentTilt > 180f)
            currentTilt -= 360f;

        targetPan = currentPan;
        targetTilt = currentTilt;

        // Different noise starting points for X and Y
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        // Smoothly move toward the desired tracking position
        currentPan = Mathf.SmoothDamp(
            currentPan,
            targetPan,
            ref panVelocity,
            smoothTime
        );

        currentTilt = Mathf.SmoothDamp(
            currentTilt,
            targetTilt,
            ref tiltVelocity,
            smoothTime
        );

        // Calculate turbulence
        float turbulencePan = 0f;
        float turbulenceTilt = 0f;

        if (turbulenceEnabled)
        {
            float time = Time.time * turbulenceFrequency;

            // Perlin Noise gives smooth movement instead of random jitter
            float noiseX = Mathf.PerlinNoise(noiseOffsetX, time);
            float noiseY = Mathf.PerlinNoise(noiseOffsetY, time);

            // Convert 0-1 to -1 to +1
            noiseX = (noiseX * 2f) - 1f;
            noiseY = (noiseY * 2f) - 1f;

            turbulencePan = noiseX * turbulenceStrength;
            turbulenceTilt = noiseY * turbulenceStrength;
        }

        // Apply final camera rotation
        float finalPan = currentPan + turbulencePan;
        float finalTilt = currentTilt + turbulenceTilt;

        // Keep camera within tilt limits
        finalTilt = Mathf.Clamp(finalTilt, minTilt, maxTilt);

        transform.localRotation = Quaternion.Euler(
            finalTilt,
            finalPan,
            0f
        );
    }

    // ---------------------------------------------------------
    // SET TARGET ANGLES
    // ---------------------------------------------------------

    public void SetPanTilt(float pan, float tilt)
    {
        targetPan = Mathf.Clamp(pan, minPan, maxPan);
        targetTilt = Mathf.Clamp(tilt, minTilt, maxTilt);
    }

    // ---------------------------------------------------------
    // DIRECT PAN CONTROL
    // ---------------------------------------------------------

    public void AddPan(float amount)
    {
        targetPan += amount * panSpeed * Time.deltaTime;
        targetPan = Mathf.Clamp(targetPan, minPan, maxPan);
    }

    // ---------------------------------------------------------
    // DIRECT TILT CONTROL
    // ---------------------------------------------------------

    public void AddTilt(float amount)
    {
        targetTilt += amount * tiltSpeed * Time.deltaTime;
        targetTilt = Mathf.Clamp(targetTilt, minTilt, maxTilt);
    }

    // ---------------------------------------------------------
    // TURBULENCE CONTROL
    // ---------------------------------------------------------

    public void SetTurbulence(bool enabled)
    {
        turbulenceEnabled = enabled;
    }

    public void SetTurbulenceStrength(float strength)
    {
        turbulenceStrength = strength;
    }

    public void SetTurbulenceFrequency(float frequency)
    {
        turbulenceFrequency = frequency;
    }
}