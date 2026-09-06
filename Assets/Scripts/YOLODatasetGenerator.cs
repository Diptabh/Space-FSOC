using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class YOLODatasetGenerator : MonoBehaviour
{
    [Header("References")]
    public Camera captureCamera;
    public Transform satellite;
    public Transform beacon;

    [Header("Dataset Settings")]
    public int totalImages = 1000;
    public int imageWidth = 640;
    public int imageHeight = 640;

    [Header("Camera Distance")]
    public float minDistance = 15f;
    public float maxDistance = 25f;

    [Header("Satellite Front Direction")]
    public FrontAxis satelliteFrontAxis = FrontAxis.PositiveZ;

    public enum FrontAxis
    {
        PositiveZ,
        NegativeZ,
        PositiveX,
        NegativeX
    }

    [Header("Camera Dome")]
    [Tooltip("How far left/right the camera can move from the front.")]
    [Range(0f, 180f)]
    public float horizontalAngleRange = 90f;

    [Tooltip("How far above/below the satellite the camera can move.")]
    [Range(0f, 90f)]
    public float verticalAngleRange = 60f;

    [Header("Camera Aim Offset")]
    [Tooltip("Maximum horizontal offset from the beacon.")]
    public float horizontalAimOffset = 8f;

    [Tooltip("Maximum vertical offset from the beacon.")]
    public float verticalAimOffset = 6f;

    [Header("Camera Impairments")]
    public Material impairmentMaterial;

    public float noiseIntensity = 0.15f;
    public float blurStrength = 0.7f;

    [Header("Dataset Folder")]
    public string datasetFolderName = "YOLO_Dataset";


    private string imageFolder;
    private string labelFolder;


    void Start()
    {
        if (captureCamera == null)
            captureCamera = GetComponent<Camera>();

        if (impairmentMaterial == null)
        {
            Debug.LogError("Impairment Material is not assigned!");
            return;
        }

        if (satellite == null)
        {
            Debug.LogError("Satellite is not assigned!");
            return;
        }

        if (beacon == null)
        {
            Debug.LogError("Beacon is not assigned!");
            return;
        }

        SetupFolders();

        Debug.Log("YOLO Dataset Generator Ready.");
        Debug.Log("Press SPACE to start dataset generation.");
    }


    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GenerateDataset();
        }
    }


    void SetupFolders()
    {
        string parentFolder =
            Directory.GetParent(Application.dataPath).FullName;

        string datasetFolder =
            Path.Combine(
                parentFolder,
                datasetFolderName
            );

        imageFolder =
            Path.Combine(
                datasetFolder,
                "images"
            );

        labelFolder =
            Path.Combine(
                datasetFolder,
                "labels"
            );

        Directory.CreateDirectory(imageFolder);
        Directory.CreateDirectory(labelFolder);

        Debug.Log("Dataset folder:");
        Debug.Log(datasetFolder);
    }


    void GenerateDataset()
    {
        Debug.Log("Starting dataset generation...");

        Vector3 originalCameraPosition =
            captureCamera.transform.position;

        Quaternion originalCameraRotation =
            captureCamera.transform.rotation;

        Vector3 originalSatellitePosition =
            satellite.position;

        Quaternion originalSatelliteRotation =
            satellite.rotation;


        // Satellite remains completely fixed.
        // We restore it here just in case another script changed it.

        satellite.position =
            originalSatellitePosition;

        satellite.rotation =
            originalSatelliteRotation;


        for (int i = 0; i < totalImages; i++)
        {
            GenerateRandomCameraPosition();

            ConfigureImpairment(i);

            CaptureImage(i);

            if ((i + 1) % 50 == 0)
            {
                Debug.Log(
                    "Generated " +
                    (i + 1) +
                    " / " +
                    totalImages
                );
            }
        }


        // Restore original camera
        captureCamera.transform.position =
            originalCameraPosition;

        captureCamera.transform.rotation =
            originalCameraRotation;


        // Restore satellite
        satellite.position =
            originalSatellitePosition;

        satellite.rotation =
            originalSatelliteRotation;


        // Turn impairments off
        impairmentMaterial.SetFloat(
            "_NoiseIntensity",
            0f
        );

        impairmentMaterial.SetFloat(
            "_BlurStrength",
            0f
        );


        Debug.Log("=================================");
        Debug.Log("DATASET GENERATION COMPLETE!");
        Debug.Log("Images: " + imageFolder);
        Debug.Log("Labels: " + labelFolder);
        Debug.Log("=================================");
    }


    // ======================================================
    // CAMERA SEMI-DOME
    // ======================================================

    void GenerateRandomCameraPosition()
    {
        // --------------------------------------------------
        // Horizontal angle around the FRONT of satellite
        // --------------------------------------------------

        float horizontalAngle =
            Random.Range(
                -horizontalAngleRange,
                horizontalAngleRange
            );


        // --------------------------------------------------
        // Vertical angle
        // --------------------------------------------------

        float verticalAngle =
            Random.Range(
                -verticalAngleRange,
                verticalAngleRange
            );


        // --------------------------------------------------
        // Distance
        // --------------------------------------------------

        float distance =
            Random.Range(
                minDistance,
                maxDistance
            );


        // --------------------------------------------------
        // Get satellite front direction
        // --------------------------------------------------

        Vector3 frontDirection =
            GetSatelliteFrontDirection();


        // --------------------------------------------------
        // Horizontal rotation
        // --------------------------------------------------

        Quaternion horizontalRotation =
            Quaternion.AngleAxis(
                horizontalAngle,
                Vector3.up
            );


        Vector3 horizontalDirection =
            horizontalRotation *
            frontDirection;


        // --------------------------------------------------
        // Vertical rotation
        // --------------------------------------------------

        Vector3 rightAxis =
            Vector3.Cross(
                Vector3.up,
                horizontalDirection
            ).normalized;


        Quaternion verticalRotation =
            Quaternion.AngleAxis(
                verticalAngle,
                rightAxis
            );


        Vector3 finalDirection =
            verticalRotation *
            horizontalDirection;


        finalDirection.Normalize();


        // --------------------------------------------------
        // Position camera
        // --------------------------------------------------

        captureCamera.transform.position =
            satellite.position +
            finalDirection * distance;


        // --------------------------------------------------
        // FIRST: point directly at beacon
        // --------------------------------------------------

        Vector3 directionToBeacon =
            beacon.position -
            captureCamera.transform.position;


        Quaternion lookRotation =
            Quaternion.LookRotation(
                directionToBeacon.normalized
            );


        // --------------------------------------------------
        // SECOND: introduce random AIM OFFSET
        // --------------------------------------------------

        float horizontalOffset =
            Random.Range(
                -horizontalAimOffset,
                horizontalAimOffset
            );


        float verticalOffset =
            Random.Range(
                -verticalAimOffset,
                verticalAimOffset
            );


        // Create an offset in camera-local space
        Vector3 offset =
            new Vector3(
                horizontalOffset,
                verticalOffset,
                0f
            );


        // Convert offset from degrees into rotation
        Quaternion offsetRotation =
            Quaternion.Euler(
                -verticalOffset,
                horizontalOffset,
                0f
            );


        // --------------------------------------------------
        // FINAL CAMERA ROTATION
        // --------------------------------------------------

        captureCamera.transform.rotation =
            lookRotation * offsetRotation;
    }

    // ======================================================
    // SATELLITE FRONT AXIS
    // ======================================================

    Vector3 GetSatelliteFrontDirection()
    {
        switch (satelliteFrontAxis)
        {
            case FrontAxis.PositiveZ:
                return satellite.forward;

            case FrontAxis.NegativeZ:
                return -satellite.forward;

            case FrontAxis.PositiveX:
                return satellite.right;

            case FrontAxis.NegativeX:
                return -satellite.right;
        }

        return satellite.forward;
    }


    // ======================================================
    // IMPAIRMENTS
    // ======================================================

    void ConfigureImpairment(int index)
    {
        if (impairmentMaterial == null)
            return;


        switch (index % 4)
        {
            case 0:

                // CLEAN

                impairmentMaterial.SetFloat(
                    "_NoiseIntensity",
                    0f
                );

                impairmentMaterial.SetFloat(
                    "_BlurStrength",
                    0f
                );

                break;


            case 1:

                // NOISE

                impairmentMaterial.SetFloat(
                    "_NoiseIntensity",
                    noiseIntensity
                );

                impairmentMaterial.SetFloat(
                    "_BlurStrength",
                    0f
                );

                break;


            case 2:

                // BLUR

                impairmentMaterial.SetFloat(
                    "_NoiseIntensity",
                    0f
                );

                impairmentMaterial.SetFloat(
                    "_BlurStrength",
                    blurStrength
                );

                break;


            case 3:

                // NOISE + BLUR

                impairmentMaterial.SetFloat(
                    "_NoiseIntensity",
                    noiseIntensity
                );

                impairmentMaterial.SetFloat(
                    "_BlurStrength",
                    blurStrength
                );

                break;
        }
    }


    // ======================================================
    // CAPTURE IMAGE
    // ======================================================

    void CaptureImage(int index)
    {
        RenderTexture renderTexture =
            new RenderTexture(
                imageWidth,
                imageHeight,
                24
            );


        captureCamera.targetTexture =
            renderTexture;

        RenderTexture.active =
            renderTexture;


        captureCamera.Render();


        Texture2D image =
            new Texture2D(
                imageWidth,
                imageHeight,
                TextureFormat.RGB24,
                false
            );


        image.ReadPixels(
            new Rect(
                0,
                0,
                imageWidth,
                imageHeight
            ),
            0,
            0
        );


        image.Apply();


        // --------------------------------------------------
        // SAVE IMAGE
        // --------------------------------------------------

        byte[] bytes =
            image.EncodeToPNG();


        string imagePath =
            Path.Combine(
                imageFolder,
                "image_" +
                index.ToString("D5") +
                ".png"
            );


        File.WriteAllBytes(
            imagePath,
            bytes
        );


        // --------------------------------------------------
        // GENERATE LABEL
        // --------------------------------------------------

        GenerateYOLOLabel(index);


        // --------------------------------------------------
        // CLEANUP
        // --------------------------------------------------

        captureCamera.targetTexture = null;

        RenderTexture.active = null;

        DestroyImmediate(renderTexture);

        DestroyImmediate(image);
    }


    // ======================================================
    // YOLO LABEL
    // ======================================================

    void GenerateYOLOLabel(int index)
    {
        Renderer beaconRenderer =
            beacon.GetComponent<Renderer>();


        if (beaconRenderer == null)
        {
            Debug.LogError(
                "Beacon does not have a Renderer!"
            );

            return;
        }


        Bounds bounds =
            beaconRenderer.bounds;


        Vector3[] corners =
            new Vector3[8];


        corners[0] =
            new Vector3(
                bounds.min.x,
                bounds.min.y,
                bounds.min.z
            );

        corners[1] =
            new Vector3(
                bounds.min.x,
                bounds.min.y,
                bounds.max.z
            );

        corners[2] =
            new Vector3(
                bounds.min.x,
                bounds.max.y,
                bounds.min.z
            );

        corners[3] =
            new Vector3(
                bounds.min.x,
                bounds.max.y,
                bounds.max.z
            );

        corners[4] =
            new Vector3(
                bounds.max.x,
                bounds.min.y,
                bounds.min.z
            );

        corners[5] =
            new Vector3(
                bounds.max.x,
                bounds.min.y,
                bounds.max.z
            );

        corners[6] =
            new Vector3(
                bounds.max.x,
                bounds.max.y,
                bounds.min.z
            );

        corners[7] =
            new Vector3(
                bounds.max.x,
                bounds.max.y,
                bounds.max.z
            );


        float minX = 1f;
        float maxX = 0f;
        float minY = 1f;
        float maxY = 0f;


        bool visible = false;


        foreach (Vector3 corner in corners)
        {
            Vector3 screenPoint =
                captureCamera.WorldToViewportPoint(
                    corner
                );


            if (screenPoint.z > 0)
            {
                visible = true;


                minX =
                    Mathf.Min(
                        minX,
                        screenPoint.x
                    );


                maxX =
                    Mathf.Max(
                        maxX,
                        screenPoint.x
                    );


                minY =
                    Mathf.Min(
                        minY,
                        screenPoint.y
                    );


                maxY =
                    Mathf.Max(
                        maxY,
                        screenPoint.y
                    );
            }
        }


        string labelPath =
            Path.Combine(
                labelFolder,
                "image_" +
                index.ToString("D5") +
                ".txt"
            );


        if (!visible)
        {
            File.WriteAllText(
                labelPath,
                ""
            );

            return;
        }


        minX = Mathf.Clamp01(minX);
        maxX = Mathf.Clamp01(maxX);

        minY = Mathf.Clamp01(minY);
        maxY = Mathf.Clamp01(maxY);


        float centerX =
            (minX + maxX) / 2f;


        float centerY =
            1f -
            ((minY + maxY) / 2f);


        float width =
            maxX - minX;


        float height =
            maxY - minY;


        string label =
            "0 " +
            centerX.ToString("F6") +
            " " +
            centerY.ToString("F6") +
            " " +
            width.ToString("F6") +
            " " +
            height.ToString("F6");


        File.WriteAllText(
            labelPath,
            label
        );
    }
}