using UnityEngine;
using UnityEngine.UI;

public class CaptureUIManager : MonoBehaviour
{
    [SerializeField] private GameObject capturePanel;
    [SerializeField] private CanvasGroup captureCanvasGroup;
    [SerializeField] private float fadeSpeed = 10f;

    public Image targetImage;        // The UI Image to change
    public Sprite normalSprite;      // Default sprite
    public Sprite pressedSprite;     // Pressed-down sprite


    private bool shouldShow = false;

    void OnEnable()
    {
        FishMovement.OnCaptureStateChanged += HandleCaptureStateChanged;
    }

    void OnDisable()
    {
        FishMovement.OnCaptureStateChanged -= HandleCaptureStateChanged;
    }

    void HandleCaptureStateChanged(bool inCaptureTime)
    {
        shouldShow = inCaptureTime;
        capturePanel.SetActive(true); // Keep active for fading
    }

    void Update()
    {
        if (captureCanvasGroup != null)
        {
            float targetAlpha = shouldShow ? 1f : 0f;
            captureCanvasGroup.alpha = Mathf.MoveTowards(captureCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            // Disable panel when fully faded out
            if (!shouldShow && captureCanvasGroup.alpha <= 0f)
            {
                capturePanel.SetActive(false);
            }
        }

        // Detects when the space bar is pressed down
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetImage.sprite = pressedSprite;
        }

        // Detects when the space bar is released
        if (Input.GetKeyUp(KeyCode.Space))
        {
            targetImage.sprite = normalSprite;
        }
    }
}
