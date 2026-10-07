using UnityEngine;
using UnityEngine.AI;

/// <summary>GPS 위도·경도를 Background 지도의 월드 좌표로 바꿉니다.</summary>
public sealed class GpsMapPosition : MonoBehaviour
{
    [System.Serializable]
    struct Anchor
    {
        public double latitude;
        public double longitude;
        // 원본 지도 이미지에서 왼쪽 위를 (0, 0)으로 한 픽셀 좌표
        public Vector2 pixel;
    }

    [SerializeField] GPS gps;
    [SerializeField] SpriteRenderer background;
    [SerializeField] float maxAccuracyMeters = 30f;
    [SerializeField] Anchor[] anchors =
    {
        new Anchor { latitude = 37.649226872529226, longitude = 127.06385895315638, pixel = new Vector2(197, 208) },
        new Anchor { latitude = 37.64926237874475, longitude = 127.06483633015236, pixel = new Vector2(530, 192) },
        new Anchor { latitude = 37.64773110272582, longitude = 127.0641126272316, pixel = new Vector2(293, 819) }
    };

    const double MetersPerDegree = 111320.0;
    double originLatitude, originLongitude, longitudeToMeters;
    double scale, rotation, offsetX, offsetZ;
    double lastLatitude = double.NaN, lastLongitude = double.NaN;
    bool calibrated;

    void Awake()
    {
        if (gps == null) gps = FindFirstObjectByType<GPS>();
        if (background == null) background = GameObject.Find("Background")?.GetComponent<SpriteRenderer>();
        calibrated = Calibrate();

        // MyPos의 NavMeshAgent가 GPS로 옮긴 Transform을 다시 덮어쓰지 않게 합니다.
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.updatePosition = false;
    }

    void Update()
    {
        if (!calibrated || gps == null || !gps.HasLocation ||
            gps.AccuracyMeters <= 0f || gps.AccuracyMeters > maxAccuracyMeters ||
            (gps.Latitude == lastLatitude && gps.Longitude == lastLongitude)) return;

        transform.position = ToWorld(gps.Latitude, gps.Longitude);
        lastLatitude = gps.Latitude;
        lastLongitude = gps.Longitude;
    }

    bool Calibrate()
    {
        if (background == null || background.sprite == null || anchors == null || anchors.Length < 3)
        {
            Debug.LogError("[GpsMapPosition] Background와 기준점 3개가 필요합니다.", this);
            return false;
        }

        originLatitude = anchors[0].latitude;
        originLongitude = anchors[0].longitude;
        longitudeToMeters = MetersPerDegree * System.Math.Cos(originLatitude * System.Math.PI / 180.0);

        double meanEast = 0, meanNorth = 0, meanX = 0, meanZ = 0;
        for (int i = 0; i < anchors.Length; i++)
        {
            meanEast += East(anchors[i].longitude);
            meanNorth += North(anchors[i].latitude);
            Vector3 point = PixelToWorld(anchors[i].pixel);
            meanX += point.x;
            meanZ += point.z;
        }
        int count = anchors.Length;
        meanEast /= count; meanNorth /= count;
        meanX /= count; meanZ /= count;

        // 기준점 전체로 이동·회전·축척을 맞춥니다. 클릭 오차가 있어도 한 점에만 끌려가지 않습니다.
        double numeratorScale = 0, numeratorRotation = 0, denominator = 0;
        for (int i = 0; i < count; i++)
        {
            double east = East(anchors[i].longitude) - meanEast;
            double north = North(anchors[i].latitude) - meanNorth;
            Vector3 point = PixelToWorld(anchors[i].pixel);
            double x = point.x - meanX, z = point.z - meanZ;
            numeratorScale += east * x + north * z;
            numeratorRotation += east * z - north * x;
            denominator += east * east + north * north;
        }
        if (denominator < 0.001) return false;

        scale = numeratorScale / denominator;
        rotation = numeratorRotation / denominator;
        offsetX = meanX - scale * meanEast + rotation * meanNorth;
        offsetZ = meanZ - rotation * meanEast - scale * meanNorth;
        return true;
    }

    Vector3 PixelToWorld(Vector2 pixel)
    {
        Sprite sprite = background.sprite;
        float x = (pixel.x - sprite.pivot.x) / sprite.pixelsPerUnit;
        float y = (sprite.rect.height - pixel.y - sprite.pivot.y) / sprite.pixelsPerUnit;
        return background.transform.TransformPoint(new Vector3(x, y, 0f));
    }

    double East(double longitude) => (longitude - originLongitude) * longitudeToMeters;
    double North(double latitude) => (latitude - originLatitude) * MetersPerDegree;

    public Vector3 ToWorld(double latitude, double longitude)
    {
        double east = East(longitude), north = North(latitude);
        return new Vector3(
            (float)(offsetX + scale * east - rotation * north),
            transform.position.y,
            (float)(offsetZ + rotation * east + scale * north));
    }
}
