using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.U2D;
using Unity.VisualScripting;

public class FishMovement : MonoBehaviour
{
    [SerializeField] public FishData fishData;
    private SpriteRenderer fishSpriteRenderer;
    private float minScale = 0.25f; // Smallest size when radius = 0
    private float maxScale = 0.35f; // Largest size when radius = maxRadius

    public HealthHeartBar healthHeartBar;
    public int currentHealth;

    // Movement States:
    private float currentSpeed;
    private float targetSpeed;

    private bool movingClockwise = true;
    private float directionChangeTimer;
    private bool decisionMade;
    private bool isChangingDirection = false;
    private float decellerationTimer;

    private bool inCaptureZone;
    private bool isFishCaught;

    private bool inCone;
    //private bool isPulling = true;

    // Position:
    public float maxRadius;
    public float radius;
    public float currentAngle;      // in degrees
    public Transform centre;

    public TextMeshProUGUI currentAngleText;
    public TextMeshProUGUI fishCaughtText;
    public TextMeshProUGUI gameOverText;
    public float fishDirection;
    public GameObject fishSprite;

    public int numFishCaught;

    void Start()
    {
        // Get the SpriteRenderer component from the child object
        fishSpriteRenderer = fishSprite.GetComponent<SpriteRenderer>();

        // Set the sprite from FishData
        if (fishData.fishSprite != null)
        {
            fishSpriteRenderer.sprite = fishData.fishSprite;

        }

        SetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
        UpdateDirectionChangeTimer();

        inCone = MouseMovement.Instance.isFishInCone;
        CheckCaptureState();
        PullFish();
        RotateFish();
        ScaleFishByRadius();
    }

    private void Awake()
    {
        currentHealth = fishData.captureAttempts;
    }

    void SetPosition()
    {
        float radians = currentAngle * Mathf.Deg2Rad;

        float x = radius * Mathf.Cos(radians);
        float y = radius * Mathf.Sin(radians);

        Vector2 offset = new Vector2(x, y);
        
        transform.position = (Vector2)centre.position + offset;
        currentAngleText.text = "Object angle from centre: " + currentAngle;

        float fishDirection = currentAngle - 180f;
        fishSprite.transform.rotation = Quaternion.Euler(0f, 0f, fishDirection);
    }

    void UpdateMovement()
    {
        // Calculate how much to move this frame

        // Choose which rate to use based on current situation
        float rateToUse = (targetSpeed > currentSpeed) ? fishData.accelerationRate : fishData.decelerationRate;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rateToUse * Time.deltaTime);

        // Use current speed instead of fishData.speed directly
        float angleChange = currentSpeed * Time.deltaTime;

        // Apply direction
        if (movingClockwise)
            currentAngle += angleChange;
        else
            currentAngle -= angleChange;

        // Convert angle to position (same as SetPosition)
        float radians = currentAngle * Mathf.Deg2Rad;
        float x = radius * Mathf.Cos(radians);
        float y = radius * Mathf.Sin(radians);

        transform.position = (Vector2)centre.position + new Vector2(x, y);
    }

    void UpdateDirectionChangeTimer()
    {
        if (forcedLeaving) return; // skip direction changes during forced leaving

        // count down timer
        directionChangeTimer -= Time.deltaTime;

        // start slowing down before direction change
        if (directionChangeTimer <= fishData.decelerationStartTime && !isChangingDirection)
        {
            StartDeceleration();
        }

        if (directionChangeTimer <= 0f)
        {
            CompleteDirectionChange();
            // reset timer with some randomness
            directionChangeTimer = fishData.flipFrequency + UnityEngine.Random.Range(-fishData.flipRange, fishData.flipRange);
        }
    }

    private bool shouldFaceOutward = false;

    void RotateFish()
    {
        // Determine target rotation based on fish state
        float targetDirection;

        if (shouldFaceOutward)
        {
            targetDirection = currentAngle; // Face outward (towards edge)
        }
        else{
            targetDirection = currentAngle - 180f; // Face inward (towards center)
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetDirection);

        // Smoothly rotate towards target
        fishSprite.transform.rotation = Quaternion.Slerp(fishSprite.transform.rotation, targetRotation, fishData.rotationSpeed * Time.deltaTime);
    }

    void ChangeDirection()
    {
        // Switch direction
        if (movingClockwise && directionChangeTimer <= 0f)
        {
            movingClockwise = false;
        }
        else if (!movingClockwise && directionChangeTimer <= 0f)
        {
            movingClockwise = true;
        }
    }

    void StartDeceleration()
    {
        isChangingDirection = true;
        targetSpeed = fishData.minVelocity;

        // Randomly decide what happens after slowing
        decisionMade = UnityEngine.Random.Range(0f, 1f) < fishData.directionChangeChance;
        // true = change direction, false = just burst forward
    }

    void CompleteDirectionChange()
    {
        if (decisionMade)
        {
            ChangeDirection(); // actually turn
        }
        // Either way, do the burst

        targetSpeed = fishData.maxVelocity; // accelerate back up, burst!
        StartCoroutine(ReturnToBaseSpeed());

        isChangingDirection = false;
    }

    IEnumerator ReturnToBaseSpeed()
    {
        yield return new WaitForSeconds(fishData.burstDuration);
        targetSpeed = fishData.baseVelocity; // back to normal
    }

    private float bufferTimer = 0f;
    private float captureTimer = 0f;
    private bool captureTimeExpired = false;
    private bool forcedLeaving = false;

    void PullFish()
    {
        if (!isFishCaught)
        {
            if (forcedLeaving) // fish is forced out no interruptions
            {
                shouldFaceOutward = true;
                float pull = radius += (fishData.leaveSpeed * Time.deltaTime);
                radius = Mathf.Min(resetRadius, pull);


                // stop forced leaving when fish reaches max radius
                if (radius >= maxRadius || radius == resetRadius)
                {
                    forcedLeaving = false;
                    bufferTimer = fishData.leaveBufferTime;
                }
                return; // skip all other logic
            }
            if (inCaptureZone && !captureTimeExpired) // fish is in capture zone and timer
            {
                shouldFaceOutward = false;
                bufferTimer = fishData.leaveBufferTime; // reset buffer so the fish stays in same place
            }
            else if (inCaptureZone && captureTimeExpired) // fish is in capture zone but time expired - force it to leave (no buffer)
            {
                shouldFaceOutward = true;
                float pull = radius += (fishData.leaveSpeed * Time.deltaTime);
                radius = Mathf.Min(maxRadius, pull);
            }
            else if (inCone && !captureTimeExpired) // normal fishing gameplay
            {
                shouldFaceOutward = false;
                bufferTimer = fishData.leaveBufferTime;
                float pull = radius -= (fishData.pullSpeed * Time.deltaTime);
                radius = Mathf.Max(0, pull);
            }
            else if (!captureTimeExpired)
            {
                // Normal leaving with buffer (only if capture time hasn't expired)
                shouldFaceOutward = false;
                bufferTimer -= Time.deltaTime;
                if (bufferTimer <= 0f)
                {
                    shouldFaceOutward = true;
                    float pull = radius += (fishData.leaveSpeed * Time.deltaTime);
                    radius = Mathf.Min(maxRadius, pull);
                }
            }
            else
            {
                // Capture time expired and not in capture zone - fish leaves
                shouldFaceOutward = true;
                float pull = radius += (fishData.leaveSpeed * Time.deltaTime);
                radius = Mathf.Min(maxRadius, pull);
            }
        }
    }

    private float captureTargetRadius;
    private bool isCapturePulling = false;
    private float resetRadius = 0f;

    // EVENT - CaptureUIManager listens to
    public static event System.Action<bool> OnCaptureStateChanged;

    void CheckCaptureState()
    {
        bool wasinCaptureZone = inCaptureZone;

        if (radius <= fishData.captureRadius)
        {
            if (!wasinCaptureZone) // Just entered capture zone
            {
                captureTimer = fishData.captureTimer; // Start the countdown
                captureTimeExpired = false;
            }

            inCaptureZone = true;

            // Count down the capture timer
            if (captureTimer > 0)
            {
                captureTimer -= Time.deltaTime;
            }
            else if (!captureTimeExpired) // Just expired (only trigger once)
            {
                captureTimeExpired = true; // Time's up!
                forcedLeaving = true; // Start forced leaving
                isCapturePulling = false; // Stop any spacebar pulling

                currentHealth--;
                healthHeartBar.DrawHearts();

                // Push fish further out each escape
                resetRadius += fishData.resetAmount * maxRadius;

                if (resetRadius >= maxRadius)
                {
                    Debug.Log("Fish should escape permanently!");
                    resetRadius = maxRadius; // Clamp to max for final escape
                    gameOverText.text = "GAME OVER!!!";
                }
                else
                {
                    Debug.Log($"Fish escapes to radius: {resetRadius}");
                }
            }

            // Spacebar mechanics (only work if time hasn't expired)
            if (!captureTimeExpired && Input.GetKeyDown(KeyCode.Space))
            {
                captureTargetRadius = Mathf.Max(0, radius - fishData.spacePullAmount);
                isCapturePulling = true;
            }

            if (isCapturePulling && !captureTimeExpired)
            {
                radius = Mathf.MoveTowards(radius, captureTargetRadius, fishData.capturePullSpeed * Time.deltaTime);
                if (Mathf.Approximately(radius, captureTargetRadius))
                {
                    isCapturePulling = false;
                }
            }

            if (radius <= 0.05)
            {
                fishCaught();
                OnCaptureStateChanged?.Invoke(false);
            }
        }
        else // check if fish inside capture zone fail
        {
            inCaptureZone = false;
            isCapturePulling = false;
            captureTimeExpired = false; // Reset when fish leaves capture zone
        }

        if (wasinCaptureZone != inCaptureZone)
        {
            OnCaptureStateChanged?.Invoke(inCaptureZone);
        }
    }

    void fishCaught()
    {
        isFishCaught = true;
        radius = maxRadius;
        isFishCaught = false;
        numFishCaught++;
        fishCaughtText.text = "Fish Caught: " + numFishCaught;
    }

    void ScaleFishByRadius()
    {
        // Calculate scale based on radius (0 to maxRadius maps to minScale to maxScale)
        float scaleRatio = radius / maxRadius;
        float currentScale = Mathf.Lerp(minScale, maxScale, scaleRatio);

        fishSpriteRenderer.transform.localScale = Vector3.one * currentScale * fishData.imageScalar;
    }   
}
