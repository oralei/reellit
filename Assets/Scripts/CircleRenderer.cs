using UnityEngine;

public class CircleRenderer : MonoBehaviour
{
    [SerializeField] private FishMovement fishMovement;
    private LineRenderer lr;
    public GameObject centre;
    public float maxRadius = 4f;
    public Camera mainCamera;

    public bool useMouseRadius;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lr = GetComponent<LineRenderer>();

        if (!useMouseRadius)
        {
            maxRadius = fishMovement.fishData.captureRadius;
            DrawCircle(100, fishMovement.fishData.captureRadius);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (useMouseRadius)
            DrawCircle(100, GetDistanceFromCentre());
    }

    void DrawCircle(int steps, float radius)
    {
        lr.positionCount = steps + 1;
        float clampedRadius = Mathf.Min(maxRadius, radius);

        for (int i = 0; i <= steps; i++)
        {
            float circumferenceProgress = (float)i / steps;

            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);

            float x = xScaled * clampedRadius;
            float y = yScaled * clampedRadius;

            Vector3 currentPosition = new Vector3 (x, y, 0);
            lr.SetPosition(i, currentPosition);
        }
    }

    public float GetDistanceFromCentre()
    {
        Vector3 centreV = centre.transform.position;
        Vector3 mouseV = new Vector3(mainCamera.ScreenToWorldPoint(Input.mousePosition).x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y, 0);
        float distance = Vector3.Distance(mouseV, centreV);

        return distance;
    }
}
