using UnityEngine;

public class DrawMouseLine : MonoBehaviour
{
    private LineRenderer lr;
    public Transform[] points;
    public Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(0, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        lr.SetPosition(1, transform.position);
    }
}
