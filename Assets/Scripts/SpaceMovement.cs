using UnityEngine;

public class SpaceMovement : MonoBehaviour
{
    [Header("Forward Movement")]
    public float speed = 0.5f;

    [Header("Arc Movement")]
    public bool useArcMovement = true;
    public float arcStrength = 0.15f;

    [Header("Satellite Rotation")]
    public bool enableRotation = false;
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 2f;
    public float minRotationAngle = -20f;
    public float maxRotationAngle = 20f;

    private float currentAngle = 0f;
    private int rotationDirection = 1;

    private void Start()
    {
        currentAngle = 0f;
    }

    private void Update()
    {
        MoveForward();

        if (enableRotation)
        {
            RotateBackAndForth();
        }
    }

    void MoveForward()
    {
        Vector3 forwardMovement = transform.forward * speed;

        Vector3 arcMovement = Vector3.zero;

        if (useArcMovement)
        {
            arcMovement = transform.right * arcStrength;
        }

        transform.position +=
            (forwardMovement + arcMovement) * Time.deltaTime;
    }

    void RotateBackAndForth()
    {
        float rotationAmount =
            rotationDirection * rotationSpeed * Time.deltaTime;

        currentAngle += rotationAmount;

        if (currentAngle >= maxRotationAngle)
        {
            currentAngle = maxRotationAngle;
            rotationDirection = -1;
        }
        else if (currentAngle <= minRotationAngle)
        {
            currentAngle = minRotationAngle;
            rotationDirection = 1;
        }

        Quaternion rotation =
            Quaternion.AngleAxis(currentAngle, rotationAxis);

        transform.localRotation = rotation;
    }
}