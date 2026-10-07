using System.Collections;
using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class GPS : MonoBehaviour
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public float AccuracyMeters { get; private set; }
    public bool HasLocation { get; private set; }

    private IEnumerator Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            bool answered = false;
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => answered = true;
            callbacks.PermissionDenied += _ => answered = true;
            callbacks.PermissionDeniedAndDontAskAgain += _ => answered = true;
            callbacks.PermissionRequestDismissed += _ => answered = true;

            Permission.RequestUserPermission(Permission.FineLocation, callbacks);
            yield return new WaitUntil(() => answered);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            yield break;
#endif

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("기기의 위치 서비스가 꺼져 있습니다.");
            yield break;
        }

        Input.location.Start(10f, 5f); // 목표 정확도 10m, 이동 5m마다 갱신

        int waitSeconds = 20;
        while (Input.location.status == LocationServiceStatus.Initializing
               && waitSeconds-- > 0)
            yield return new WaitForSeconds(1f);

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("GPS 위치를 가져오지 못했습니다.");
            yield break;
        }

        while (Input.location.status == LocationServiceStatus.Running)
        {
            var location = Input.location.lastData;
            Latitude = location.latitude;
            Longitude = location.longitude;
            AccuracyMeters = location.horizontalAccuracy;
            HasLocation = true;

            Debug.Log($"GPS: {Latitude}, {Longitude} / 오차 약 {AccuracyMeters}m");
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDisable()
    {
        if (Input.location.status == LocationServiceStatus.Running)
            Input.location.Stop();
    }
}