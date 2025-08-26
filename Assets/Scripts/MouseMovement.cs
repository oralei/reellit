using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseMovement : MonoBehaviour
{
    public Vector3 refV = Vector2.right;
    public Transform centre;
    public Camera mainCamera;
    public TextMeshProUGUI mouseAngleText;
    public float mouseAngle;
    public float circleRadius;

    public GameObject centreDot;
    public GameObject leftDot;
    public GameObject rightDot;

    public GameObject fishObj;
    public TextMeshProUGUI inConeText;
    public bool isFishInCone = false;

    public float coneHalfWidth = 5f;

    public static MouseMovement Instance;

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseAngle();
        ClampedObjectPosition();
        ClampedSidesPosition();
        isFishInCone = IsObjectInCone(fishObj);
        inConeText.text = "Is fish in cone?: " + isFishInCone;
    }

    public void GetMouseAngle()
    {
        Vector2 mousePos = new Vector2(mainCamera.ScreenToWorldPoint(Input.mousePosition).x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y);
        Vector2 centreV = new Vector2(centre.position.x, centre.position.y);
        Vector2 mouseV = mousePos - centreV;
        float mouseA = Vector2.SignedAngle(refV, mouseV);

        mouseAngleText.text = "Mouse angle from centre: " + mouseA.ToString();
        mouseAngle = mouseA;
    }

    void ClampedObjectPosition()
    {
        float radians = mouseAngle * Mathf.Deg2Rad;

        float x = circleRadius * Mathf.Cos(radians);
        float y = circleRadius * Mathf.Sin(radians);
        Vector2 offset = new Vector2(x, y);

        centreDot.transform.position = (Vector2)centre.position + offset;

        float tangentDirection = mouseAngle - 180f;
        centreDot.transform.rotation = Quaternion.Euler(0f, 0f, tangentDirection);
    }

    void ClampedSidesPosition()
    {
        SetDotPosition(leftDot, mouseAngle + coneHalfWidth);
        SetDotPosition(rightDot, mouseAngle - coneHalfWidth);
    }

    void SetDotPosition(GameObject dot, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(circleRadius * Mathf.Cos(radians), circleRadius * Mathf.Sin(radians));
        dot.transform.position = (Vector2)centre.position + offset;
        dot.transform.rotation = Quaternion.Euler(0f, 0f, angle - 180f);
    }

    public bool IsObjectInCone(GameObject obj)
    {
        // Get obj's angle from center
        Vector2 objPos = new Vector2(obj.transform.position.x, obj.transform.position.y);
        Vector2 centreV = new Vector2(centre.position.x, centre.position.y);
        Vector2 objV = objPos - centreV;
        float objAngle = Vector2.SignedAngle(refV, objV);

        // Use DeltaAngle to check if object is within the cone width
        float angleFromCenter = Mathf.DeltaAngle(mouseAngle, objAngle);

        // Object is in cone if it's within the half-width on either side
        return Mathf.Abs(angleFromCenter) <= coneHalfWidth;
    }
}
