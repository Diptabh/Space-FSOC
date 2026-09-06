using UnityEngine;

public class FloatingSatellite : MonoBehaviour
{
    [Header("Orbit")]
    [Tooltip("Object that the satellite will orbit around.")]
    public Transform orbitTarget;

    [Tooltip("Distance between the satellite and the orbit target.")]
    public float orbitRadius = 10f;

    [Tooltip("Orbit speed in degrees per second.")]
    public float orbitSpeed = 5f;

    [Tooltip("1 = one direction, -1 = opposite direction.")]
    public int orbitDirection = 1;

    [Tooltip("Axis around which the satellite orbits.")]
    public Vector3 orbitAxis = Vector3.up;


    [Header("Controlled Rotation")]
    [Tooltip("Local axis the satellite rotates around.")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Rotation speed in degrees per second.")]
    public float rotationSpeed = 5f;

    [Tooltip("Minimum rotation angle.")]
    public float minRotationAngle = -20f;

    [Tooltip("Maximum rotation angle.")]
    public float maxRotationAngle = 20f;


    private float orbitAngle;
    private float currentRotation;
    private int rotationDirection = 1;

    private Quaternion initialRotation;


    void Start()
    {
        orbitAxis = orbitAxis.normalized;
        rotationAxis = rotationAxis.normalized;

        // Remember starting rotation
        initialRotation = transform.localRotation;

        // Determine the starting orbit angle from the satellite's position
        if (orbitTarget != null)
        {
            Vector3 offset = transform.position - orbitTarget.position;

            // Project the offset onto the orbit plane
            offset -= Vector3.Project(offset, orbitAxis);

            if (offset.sqrMagnitude > 0.001f)
            {
                orbitAngle = Vector3.SignedAngle(
                    Vector3.right,
                    offset.normalized,
                    orbitAxis
                );
            }
        }

        currentRotation = 0f;
    }


    void Update()
    {
        if (orbitTarget == null)
            return;


        // -------------------------
        // ORBIT MOVEMENT
        // -------------------------

        orbitAngle +=
            orbitSpeed *
            orbitDirection *
            Time.deltaTime;

        if (orbitAngle > 360f)
            orbitAngle -= 360f;

        if (orbitAngle < 0f)
            orbitAngle += 360f;


        // Calculate position on the orbit
        Quaternion orbitRotation =
            Quaternion.AngleAxis(
                orbitAngle,
                orbitAxis
            );

        Vector3 orbitOffset =
            orbitRotation * Vector3.right * orbitRadius;

        transform.position =
            orbitTarget.position + orbitOffset;


        // -------------------------
        // CONTROLLED ROTATION
        // -------------------------

        currentRotation +=
            rotationSpeed *
            rotationDirection *
            Time.deltaTime;


        // Reverse direction at rotation limits
        if (currentRotation >= maxRotationAngle)
        {
            currentRotation = maxRotationAngle;
            rotationDirection = -1;
        }
        else if (currentRotation <= minRotationAngle)
        {
            currentRotation = minRotationAngle;
            rotationDirection = 1;
        }


        // Apply rotation relative to starting rotation
        transform.localRotation =
            initialRotation *
            Quaternion.AngleAxis(
                currentRotation,
                rotationAxis
            );
    }
}
