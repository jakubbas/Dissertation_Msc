using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ShapeQuality : MonoBehaviour
{
    public RigBuilder rigBuilder;
    public MotionParameter motionParameter;
    public MotionParameterApplier motionParameterApplier;

    [System.Serializable]
    public class ShapeQualitySettings
    {
        [Range(-1f, 1f)] public float enclosingSpreadingFactor = 0f;
        [Range(-1f, 1f)] public float sinkingRisingFactor = 0f;
        [Range(-1f, 1f)] public float retreatingAdvancingFactor = 0f;
    }

    [System.Serializable]
    public class MovementRange
    {
        public float baseValue;
        public float minValue;
        public float maxValue;

        public MovementRange(float baseVal, float min, float max)
        {
            baseValue = baseVal;
            minValue = min;
            maxValue = max;
        }
    }

    [Header("Shape Quality Settings")]
    public ShapeQualitySettings shapeQualities;

    [Header("Targets")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform leftFootTarget;
    public Transform rightFootTarget;
    public Transform hipsTarget;
    public Transform headTarget;
    public Transform leftShoulderTarget;
    public Transform rightShoulderTarget;
    public Transform chestTarget;

    [Header("Movement Settings")]
    public float footRotationRange = 20f;
    public MovementRange handMovementRange = new MovementRange(0.25f, 0.1f, 0.5f);
    public MovementRange footMovementRange = new MovementRange(0.1f, 0.05f, 0.25f);
    public MovementRange hipMovementRange = new MovementRange(0.2f, 0.1f, 0.3f);
    public MovementRange toeRotationRange = new MovementRange(30f, 15f, 45f);
    public MovementRange shoulderRotationRange = new MovementRange(0.5f, 1f, 3f);
    public MovementRange chestMovementRange = new MovementRange(10f, 2f, 8f);
    public MovementRange headMovementRange = new MovementRange(2.5f, 0.05f, 0.15f);

    private Vector3 initialLeftHandPosition;
    private Vector3 initialRightHandPosition;
    private Vector3 initialLeftFootPosition;
    private Vector3 initialRightFootPosition;
    private Vector3 initialHipsPosition;
    private Vector3 initialHeadTargetPosition;
    private Vector3 initialChestTargetPosition;
    private Vector3 initialLeftShoulderTargetPosition;
    private Vector3 initialRightShoulderTargetPosition;
    private Quaternion initialLeftFootRotation;
    private Quaternion initialRightFootRotation;


    private void Start()
    {
        InitializePositionsAndRotations();
    }

    private void InitializePositionsAndRotations()
    {
        initialLeftHandPosition = leftHandTarget.position;
        initialRightHandPosition = rightHandTarget.position;
        initialLeftFootPosition = leftFootTarget.position;
        initialRightFootPosition = rightFootTarget.position;
        initialHipsPosition = hipsTarget.position;
        initialLeftFootRotation = leftFootTarget.rotation;
        initialRightFootRotation = rightFootTarget.rotation;

        initialHeadTargetPosition = headTarget.position;
        initialChestTargetPosition = chestTarget.position;
        initialLeftShoulderTargetPosition = leftShoulderTarget.position;
        initialRightShoulderTargetPosition = rightShoulderTarget.position;
        motionParameterApplier.InitializeRigs2(initialLeftHandPosition, initialRightHandPosition, initialLeftFootPosition, initialRightFootPosition);
    }

    private void LateUpdate()
    {
        ApplyShapeQualities();
        motionParameterApplier.ApplyMotionParams();

    }


    private void ApplyShapeQualities()
    {
        if (motionParameter == null || motionParameter.shapeQualities == null)
        {
            Debug.LogWarning("MotionParameter or ShapeQualities is null!");
            return;
        }

        Vector3 leftHandPos = initialLeftHandPosition;
        Vector3 rightHandPos = initialRightHandPosition;
        Vector3 leftFootPos = initialLeftFootPosition;
        Vector3 rightFootPos = initialRightFootPosition;
        Vector3 hipsPos = initialHipsPosition;
        Vector3 headTargetPos = initialHeadTargetPosition;
        Vector3 chestTargetPos = initialChestTargetPosition;
        Vector3 leftShoulderPos = initialLeftShoulderTargetPosition;
        Vector3 rightShoulderPos = initialRightShoulderTargetPosition;
        Quaternion leftFootRot = initialLeftFootRotation;
        Quaternion rightFootRot = initialRightFootRotation;

        //Enclosing/Spreading
        float es = motionParameter.shapeQualities.enclosingSpreadingFactor;
        float handES = ClampedMovement(handMovementRange, es);
        float footES = ClampedMovement(footMovementRange, es);
        float footRotES = ClampedMovement(new MovementRange(footRotationRange, 10f, 30f), es);
        float shoulderRotES = ClampedMovement(shoulderRotationRange, es);

        leftHandPos += Vector3.right * -handES;
        rightHandPos += Vector3.right * handES;
        leftFootPos += Vector3.right * -footES;
        rightFootPos += Vector3.right * footES;
        leftFootRot *= Quaternion.Euler(0, footRotES, 0);
        rightFootRot *= Quaternion.Euler(0, footRotES, 0);

        leftShoulderPos += transform.right * shoulderRotES;
        rightShoulderPos += transform.right * -shoulderRotES;

        //Sinking/Rising
        float sr = motionParameter.shapeQualities.sinkingRisingFactor;
        float handSR = ClampedMovement(handMovementRange, sr);
        float hipSR = ClampedMovement(hipMovementRange, sr);
        float headSR = ClampedMovement(headMovementRange, sr);
        float chestSR = ClampedMovement(chestMovementRange, sr);
        float toeSR = ClampedMovement(toeRotationRange, sr);

        leftHandPos += Vector3.up * handSR;
        rightHandPos += Vector3.up * handSR;
        hipsPos += Vector3.up * hipSR;
        headTargetPos += Vector3.up * headSR;
        chestTargetPos += Vector3.up * chestSR;

        if (sr > 0)
        {
            leftFootRot *= Quaternion.Euler(-toeSR, 0, 0);
            rightFootRot *= Quaternion.Euler(-toeSR, 0, 0);
        }

        //Retreating/Advancing
        float ra = motionParameter.shapeQualities.retreatingAdvancingFactor;
        float handRA = ClampedMovement(handMovementRange, ra);
        float footRA = ClampedMovement(footMovementRange, ra);

        leftHandPos += Vector3.forward * handRA;
        rightHandPos += Vector3.forward * handRA;
        leftFootPos += Vector3.forward * -footRA;

        //Apply all changes
        leftHandTarget.position = leftHandPos;
        rightHandTarget.position = rightHandPos;
        leftFootTarget.position = leftFootPos;
        rightFootTarget.position = rightFootPos;
        hipsTarget.position = hipsPos;
        headTarget.position = headTargetPos;
        chestTarget.position = chestTargetPos;
        leftFootTarget.rotation = leftFootRot;
        rightFootTarget.rotation = rightFootRot;
        leftShoulderTarget.position = leftShoulderPos;
        rightShoulderTarget.position = rightShoulderPos;
    }
    private float ClampedMovement(MovementRange range, float factor)
    {
        float movement = range.baseValue * factor;
        return Mathf.Clamp(movement, range.minValue, range.maxValue);
    }
}