using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "Scriptable Objects/FishData")]
public class FishData : ScriptableObject
{
    [Header("Visual")]
    public Sprite fishSprite; // Add this line
    public float imageScalar;

    [Header("Speed Control")]
    public float baseVelocity;
    public float maxVelocity;
    public float minVelocity;
    public float pullSpeed;
    public float leaveSpeed;
    public float rotationSpeed;

    [Header("Timing")]
    public float flipFrequency;
    public float burstDuration;
    public float decelerationStartTime;
    public float flipRange;
    public float leaveBufferTime;
    public float captureTimer;

    [Header("Rates")]
    public float accelerationRate;
    public float decelerationRate;

    [Header("RNG")]
    public float directionChangeChance; // 0.x% chance to turn 1-0.x% chance to not

    [Header("Misc")]
    public float captureRadius;
    public float spacePullAmount;
    public float capturePullSpeed;

    [Header("Health")]
    public int captureAttempts;
    public float resetAmount;
}
