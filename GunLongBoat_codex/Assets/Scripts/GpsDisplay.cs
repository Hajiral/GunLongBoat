using System.Globalization;
using TMPro;
using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

/// <summary>Displays the phone's GPS reading on a screen-space UI label.</summary>
public sealed class GpsDisplay : MonoBehaviour
{
    [SerializeField] private GPS gpsReader;
    [SerializeField] private TMP_Text gpsText;
    [SerializeField, Min(0.1f)] private float refreshInterval = 0.5f;

    private float nextRefreshTime;

    private void Awake()
    {
        if (gpsReader == null)
            gpsReader = FindFirstObjectByType<GPS>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
            return;

        Refresh();
    }

    private void Refresh()
    {
        nextRefreshTime = Time.unscaledTime + refreshInterval;
        if (gpsText == null)
            return;

        if (gpsReader == null)
        {
            ShowStatus("GPS: Reader not connected");
            return;
        }

        if (gpsReader.HasLocation && Input.location.status == LocationServiceStatus.Running)
        {
            gpsText.text = string.Format(
                CultureInfo.InvariantCulture,
                "GPS: Connected\nLatitude: {0:F6}\nLongitude: {1:F6}\nAccuracy: +/- {2:F1} m",
                gpsReader.Latitude,
                gpsReader.Longitude,
                gpsReader.AccuracyMeters);
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            ShowStatus("GPS: Location permission required");
            return;
        }
#endif

        if (!Input.location.isEnabledByUser)
        {
            ShowStatus("GPS: Turn on location services");
            return;
        }

        switch (Input.location.status)
        {
            case LocationServiceStatus.Initializing:
                ShowStatus("GPS: Finding your location...");
                break;
            case LocationServiceStatus.Failed:
                ShowStatus("GPS: Could not get location");
                break;
            default:
                ShowStatus("GPS: Waiting for location...");
                break;
        }
    }

    private void ShowStatus(string status)
    {
        gpsText.text = status + "\nLatitude: --\nLongitude: --\nAccuracy: --";
    }
}
