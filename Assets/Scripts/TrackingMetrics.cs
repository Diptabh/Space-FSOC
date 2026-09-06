using UnityEngine;
using TMPro;

public class TrackingMetrics : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text statusText;
    public TMP_Text acquisitionTimeText;
    public TMP_Text xErrorText;
    public TMP_Text yErrorText;
    public TMP_Text totalErrorText;
    public TMP_Text stabilityText;

    [Header("Acquisition Settings")]
    public float acquiredErrorThreshold = 40f;

    [Header("Stability Settings")]
    public float stableErrorThreshold = 10f;
    public float stabilityTimeRequired = 1f;

    private float acquisitionStartTime;
    private float acquisitionTime;

    private bool acquisitionStarted = false;
    private bool acquired = false;

    private float stableTimer = 0f;

    void Start()
    {
        acquisitionStartTime = Time.time;
        acquisitionStarted = true;

        statusText.text = "Status: ACQUIRING";
        acquisitionTimeText.text = "Acquisition: 0.00 s";
        stabilityText.text = "Stability: UNSTABLE";
    }

    void Update()
    {
        if (acquisitionStarted && !acquired)
        {
            acquisitionTime = Time.time - acquisitionStartTime;

            acquisitionTimeText.text =
                $"Acquisition: {acquisitionTime:F2} s";
        }
    }

    public void UpdateError(float errorX, float errorY)
    {
        float totalError = Mathf.Sqrt(
            errorX * errorX +
            errorY * errorY
        );

        xErrorText.text =
            $"X Error: {errorX:F1} px";

        yErrorText.text =
            $"Y Error: {errorY:F1} px";

        totalErrorText.text =
            $"Total Error: {totalError:F1} px";

        // Check acquisition
        if (!acquired && totalError <= acquiredErrorThreshold)
        {
            SetAcquired();
        }

        UpdateStability(totalError);
    }

    public void SetAcquired()
    {
        if (acquired)
            return;

        acquired = true;

        acquisitionTime =
            Time.time - acquisitionStartTime;

        acquisitionTimeText.text =
            $"Acquisition: {acquisitionTime:F2} s";

        statusText.text = "Status: TRACKING";
    }

    void UpdateStability(float totalError)
    {
        if (!acquired)
        {
            stabilityText.text = "Stability: UNSTABLE";
            return;
        }

        if (totalError <= stableErrorThreshold)
        {
            stableTimer += Time.deltaTime;

            if (stableTimer >= stabilityTimeRequired)
            {
                stabilityText.text = "Stability: STABLE";
            }
            else
            {
                stabilityText.text = "Stability: STABILIZING";
            }
        }
        else
        {
            stableTimer = 0f;
            stabilityText.text = "Stability: UNSTABLE";
        }
    }
}