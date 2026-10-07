using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class MapCameraFit : MonoBehaviour
{
    [SerializeField] SpriteRenderer background;
    [SerializeField, Range(1f, 1.2f)] float margin = 1.04f;

    Camera mapCamera;
    float previousAspect;

    void Awake()
    {
        mapCamera = GetComponent<Camera>();
        if (background == null)
            background = GameObject.Find("Background")?.GetComponent<SpriteRenderer>();
        Fit();
    }

    void Update()
    {
        if (mapCamera != null && !Mathf.Approximately(mapCamera.aspect, previousAspect))
            Fit();
    }

    void Fit()
    {
        if (background == null || mapCamera.aspect <= 0f) return;

        Bounds bounds = background.bounds;
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = Mathf.Max(bounds.extents.z, bounds.extents.x / mapCamera.aspect) * margin;
        transform.position = new Vector3(bounds.center.x, transform.position.y, bounds.center.z);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        previousAspect = mapCamera.aspect;
    }
}
