using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MotionParameterApplier : MonoBehaviour
{
    public MotionParameter motionParameter;
    public PersonalityToEffort personalityToEffort;
    public Animator animator;

    [Header("Rig Constraints")]
    public TwoBoneIKConstraint leftArmIK;
    public TwoBoneIKConstraint rightArmIK;
    public TwoBoneIKConstraint leftLegIK;
    public TwoBoneIKConstraint rightLegIK;
    public MultiPositionConstraint hipsIK;
    public MultiAimConstraint headIK;
    public MultiAimConstraint chestIK;


    [Header("Targets")]

    public Transform leftArmTarget;
    public Transform rightArmTarget;
    public Transform leftLegTarget;
    public Transform rightLegTarget;
    public Transform hipsTarget;
    public Transform headTarget;
    public Transform chestTarget;


    [Header("Hints")]

    public Transform leftArmHint;
    public Transform rightArmHint;
    public Transform leftLegHint;
    public Transform rightLegHint;

    private Vector3 leftArmInitialPosition;
    private Vector3 rightArmInitialPosition;
    private Quaternion initialLeftHandRotation;
    private Quaternion initialRightHandRotation;



    [Header("General Settings")]
    public float animSpeedMultiplier = 1f;
    [Range(0.1f, 1f)]
    public float minAnimationSpeed = 0.5f;
    [Range(1f, 2f)]
    public float maxAnimationSpeed = 1.5f;

    [Header("Head Movement")]
    [Range(0f, 1f)] public float headNoiseIntensity = 0.5f;
    public float headMovementAmount = 50f;
    public float headMovementSpeed = 1f;

    private Vector3 initialHeadPosition;
    private Vector3 targetHeadPosition;
    private float headNoiseOffset;


    [Header("Arm Swing")]
    public float armSwingModifier = 0.65f;
    public float armSwingFrequency = 1f;
    public float armSwingAmplitude = 0.2f;
    public float verticalSwingModifier = 0.5f;
    public float verticalSwingAmplitude = 0.1f;
    public float verticalSmoothness = 0.5f; 
    [Range(0.2f, 0.8f)] public float maxBackSwing = 0.3f; 
    [Range(0.5f, 1.3f)] public float maxForwardSwing = 0.2f;  
    public float armFloorHeight = 0.1f; 

    [Header("Wrist Rotation")]
    public Vector3 wristRotationOffset = new Vector3(0, -90, 0);
    public float wristZRotationAmount = 30f;
    public float minWristZRotation = -30f;
    public float maxWristZRotation = 30f;


    [Header("Leg Swing")]
    public float legSwingModifier = 0.5f;
    public float legSwingAmplitude = 0.2f;
    [Range(0.3f, 1f)] public float legLiftModifier = 0.5f;
    public float legLiftAmplitude = 0.1f;
    [Range(0.4f, 1f)] public float maxLegForwardSwing = 0.3f;
    [Range(0.3f, 0.9f)] public float maxLegBackSwing = 0.2f;
    public float legFloorHeight = 0f;
    [Range(0f, 1f)] public float legSmoothness = 0.5f;
    public float liftStartPoint = 0.1f; //

    private float time;
    private Vector3 leftLegInitialPosition;
    private Vector3 rightLegInitialPosition;
    private bool leftLegInBackSwing = false;
    private bool rightLegInBackSwing = false;
    private float peakThreshold = 0.1f;
    private float currentSpeedFactor = 1f;
    public void InitializeRigs2(Vector3 leftArmInit, Vector3 rightArmInit, Vector3 leftFootInit, Vector3 rightFootInit)
    {
        leftArmInitialPosition = leftArmInit;
        rightArmInitialPosition = rightArmInit;
        leftLegInitialPosition = leftFootInit;
        rightLegInitialPosition = rightFootInit;
        initialLeftHandRotation = leftArmTarget.rotation;
        initialRightHandRotation = rightArmTarget.rotation;
        InitializeHeadMovement();
    }

    public void ApplyMotionParams()
    {
        time += Time.deltaTime;
        ApplyArmSwing();
        ApplyLegSwing();
        ApplyHeadMovement();
        ApplyWristRotations();
    }

    private void ApplyWristRotations()
    {
        if (leftArmTarget == null || rightArmTarget == null) return;

        float adjustedTime = time * armSwingFrequency * currentSpeedFactor;
        float swingPhase = Mathf.Sin(adjustedTime * Mathf.PI);

        //Z rotation based on arm swing
        float zRotationRange = maxWristZRotation - minWristZRotation;
        float leftZRotation = maxWristZRotation - (swingPhase * 0.5f + 0.5f) * zRotationRange; 
        float rightZRotation = maxWristZRotation - (swingPhase * 0.5f + 0.5f) * zRotationRange; 
        
        //Rotation offsets
        Quaternion baseRotationOffset = Quaternion.Euler(wristRotationOffset);
        Quaternion leftSwingOffset = Quaternion.Euler(0, 0, leftZRotation);
        Quaternion rightSwingOffset = Quaternion.Euler(0, 0, rightZRotation);
        leftArmTarget.rotation = initialLeftHandRotation * baseRotationOffset * leftSwingOffset;
        rightArmTarget.rotation = initialRightHandRotation * baseRotationOffset * rightSwingOffset;
    }

    private float CalculateArmSwingModifier(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.65f;
        float effortInfluence = motionParams.elbowFrequency * 0.2f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.4f, 0.9f);
    }

    private float CalculateMaxBackSwing(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.3f;
        float effortInfluence = motionParams.elbowDisplacement * 0.5f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.2f, 0.8f);
    }

    private float CalculateMaxForwardSwing(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.2f;
        float effortInfluence = motionParams.animationSpeed * 0.5f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.5f, 1.3f);
    }

    private float CalculateVerticalSwingModifier(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.5f;
        float effortInfluence = motionParams.wristBend * 0.3f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.3f, 0.7f);
    }

    private float CalculateVerticalSmoothness(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.5f;
        float effortInfluence = -motionParams.wristFrequency * 0.3f;
        return Mathf.Clamp(baseValue + effortInfluence, 0f, 1f);
    }

    private float CalculateLegSwingModifier(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.5f;
        float effortInfluence = motionParams.animationSpeed * 0.3f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.3f, 0.7f);
    }

    private float CalculateLegLiftModifier(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.5f;
        float effortInfluence = motionParams.wristBend * 0.4f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.3f, 1f);
    }

    private float CalculateMaxLegForwardSwing(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.3f;
        float effortInfluence = motionParams.torsoRotationMagnitude * 0.4f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.4f, 1f);
    }

    private float CalculateMaxLegBackSwing(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.2f;
        float effortInfluence = motionParams.elbowTwist * 0.4f;
        return Mathf.Clamp(baseValue + effortInfluence, 0.3f, 0.9f);
    }

    private float CalculateLegSmoothness(MotionParameter.MotionParameters motionParams)
    {
        float baseValue = 0.5f;
        float effortInfluence = -motionParams.elbowFrequency * 0.3f;
        return Mathf.Clamp(baseValue + effortInfluence, 0f, 1f);
    }
    public void UpdateMovementParameters()
    {
        PersonalityToEffort.EffortComponents efforts = personalityToEffort.GetEffortComponents();
        MotionParameter.MotionParameters motionParams = motionParameter.motionParameters;

        //arm parameters
        armSwingModifier = CalculateArmSwingModifier(motionParams);
        maxBackSwing = CalculateMaxBackSwing(motionParams);
        maxForwardSwing = CalculateMaxForwardSwing(motionParams);
        verticalSwingModifier = CalculateVerticalSwingModifier(motionParams);
        verticalSmoothness = CalculateVerticalSmoothness(motionParams);

        //leg parameters
        legSwingModifier = CalculateLegSwingModifier(motionParams);
        legLiftModifier = CalculateLegLiftModifier(motionParams);
        maxLegForwardSwing = CalculateMaxLegForwardSwing(motionParams);
        maxLegBackSwing = CalculateMaxLegBackSwing(motionParams);
        legSmoothness = CalculateLegSmoothness(motionParams);
        UpdateHeadMovementParameters(motionParams, efforts);

        float motionParamSpeed = motionParameter.motionParameters.animationSpeed;

        float normalizedSpeed = Mathf.Clamp01(motionParamSpeed);

        float mappedSpeed = Mathf.Lerp(minAnimationSpeed, maxAnimationSpeed, normalizedSpeed);

        currentSpeedFactor = mappedSpeed;

        animator.speed = currentSpeedFactor;

    }

    private void ApplyArmSwing()
    {
        if (leftArmTarget == null || rightArmTarget == null) return;

        float adjustedTime = Time.time * armSwingFrequency * currentSpeedFactor;
        float swingPhase = Mathf.Sin(adjustedTime * Mathf.PI);

        float leftHorizontalSwing = CalculateAsymmetricSwing(swingPhase, maxBackSwing, maxForwardSwing) * armSwingModifier;
        float leftVerticalSwing = -CalculateVerticalSwing(swingPhase);

        //Right Arm reverse
        float rightHorizontalSwing = CalculateAsymmetricSwing(-swingPhase, maxBackSwing, maxForwardSwing) * armSwingModifier;
        float rightVerticalSwing = -CalculateVerticalSwing(-swingPhase);

        //pply swing to left arm
        Vector3 leftArmPosition = leftArmTarget.localPosition;
        leftArmPosition.z = leftArmInitialPosition.z + leftHorizontalSwing;
        leftArmPosition.y = Mathf.Max(leftArmInitialPosition.y - leftVerticalSwing, armFloorHeight);
        leftArmTarget.localPosition = leftArmPosition;

        //Apply swing to right arm
        Vector3 rightArmPosition = rightArmTarget.localPosition;
        rightArmPosition.z = rightArmInitialPosition.z + rightHorizontalSwing;
        rightArmPosition.y = Mathf.Max(rightArmInitialPosition.y - rightVerticalSwing, armFloorHeight);
        rightArmTarget.localPosition = rightArmPosition;
    }

    private float CalculateAsymmetricSwing(float phase, float maxBack, float maxForward)
    {
        if (phase >= 0)
        {
            //Forward
            return Mathf.Lerp(0, maxForward, phase);
        }
        else
        {
            //Back
            return Mathf.Lerp(0, -maxBack, -phase);
        }
    }

    private float CalculateVerticalSwing(float phase)
    {
        if (phase >= 0)
        {
            float smoothedPhase = CustomEase(phase);
            return smoothedPhase * verticalSwingAmplitude * verticalSwingModifier;
        }
        else
        {
            //No vertical movement-back swiing
            return 0f;
        }
    }

    private void ApplyLegSwing()
    {
        if (leftLegTarget == null || rightLegTarget == null) return;

        float adjustedTime = Time.time * armSwingFrequency * currentSpeedFactor;
        float swingPhase = Mathf.Sin(adjustedTime * Mathf.PI);
        float swingDerivative = Mathf.Cos(adjustedTime * Mathf.PI);

        float leftLegHorizontalSwing = CalculateAsymmetricLegSwing(swingPhase, maxLegBackSwing, maxLegForwardSwing) * legSwingModifier;
        float leftLegLift = CalculateLegLift(swingPhase);

        //opposite phase
        float rightLegHorizontalSwing = CalculateAsymmetricLegSwing(-swingPhase, maxLegBackSwing, maxLegForwardSwing) * legSwingModifier;
        float rightLegLift = CalculateLegLift(-swingPhase);

        if (swingDerivative >= peakThreshold && leftLegInBackSwing)
        {
            leftLegInBackSwing = false;
        }
        else if (swingDerivative <= -peakThreshold && !leftLegInBackSwing)
        {
            leftLegInBackSwing = true;
        }

        if (swingDerivative <= -peakThreshold && rightLegInBackSwing)
        {
            rightLegInBackSwing = false;
        }
        else if (swingDerivative >= peakThreshold && !rightLegInBackSwing)
        {
            rightLegInBackSwing = true;
        }

        Vector3 leftLegPosition = leftLegTarget.localPosition;
        leftLegPosition.z = leftLegInitialPosition.z + leftLegHorizontalSwing;
        leftLegPosition.y = leftLegInBackSwing ? legFloorHeight :
                            Mathf.Max(leftLegInitialPosition.y + leftLegLift, legFloorHeight);
        leftLegTarget.localPosition = leftLegPosition;

        Vector3 rightLegPosition = rightLegTarget.localPosition;
        rightLegPosition.z = rightLegInitialPosition.z + rightLegHorizontalSwing;
        rightLegPosition.y = rightLegInBackSwing ? legFloorHeight : Mathf.Max(rightLegInitialPosition.y + rightLegLift, legFloorHeight);

        rightLegTarget.localPosition = rightLegPosition;
    }

    private float CalculateAsymmetricLegSwing(float phase, float maxBack, float maxForward)
    {
        if (phase >= 0)
        {
            //Forward
            return Mathf.Lerp(0, maxForward, CustomEase(phase));
        }
        else
        {
            //Back
            return Mathf.Lerp(0, -maxBack, CustomEase(-phase));
        }
    }

    private float CalculateLegLift(float phase)
    {
        float adjustedPhase = (phase + 1f) % 2f - 1f; 

        if (adjustedPhase > -1f + liftStartPoint && adjustedPhase < 1f)
        {
            float liftPhase = (adjustedPhase + 1f - liftStartPoint) / (2f - liftStartPoint);

            float liftCurve = 1f - (2f * liftPhase - 1f) * (2f * liftPhase - 1f);

            return CustomEase(liftCurve) * legLiftAmplitude * legLiftModifier;
        }
        else
        {
            return 0f;
        }
    }

    private float CustomEase(float t)
    {
        //easing function
        return Mathf.Lerp(t, t * t * (3 - 2 * t), Mathf.Max(verticalSmoothness, legSmoothness));
    }

    private void InitializeHeadMovement()
    {
        if (headTarget != null)
        {
            initialHeadPosition = headTarget.localPosition;
            targetHeadPosition = initialHeadPosition;
            headNoiseOffset = Random.value * 1000f; //perlin noise
        }
    }

    private void UpdateHeadMovementParameters(MotionParameter.MotionParameters motionParams, PersonalityToEffort.EffortComponents efforts)
    {
        // head rotation magnitude and frequency
        headMovementAmount = Mathf.Clamp(motionParams.headRotationMagnitude * 100f, 1f, 30f);
        headMovementSpeed = Mathf.Clamp(motionParams.headRotationFrequency, 0.2f, 1f);

        float spaceInfluence = Mathf.Abs(efforts.space); //More influence for direct and indirect
        float timeInfluence = Mathf.Max(0, efforts.time); //More influence for suddenn less for sustained

        headNoiseIntensity = Mathf.Clamp01((spaceInfluence + timeInfluence) / 2f);
    }

    private void ApplyHeadMovement()
    {
        if (headTarget == null) return;

        float time = Time.time * headMovementSpeed;

        //random offsets
        float offsetX = (Mathf.PerlinNoise(time, headNoiseOffset) - 0.5f) * 2f * headMovementAmount;
        float offsetY = (Mathf.PerlinNoise(time + 100f, headNoiseOffset + 100f) - 0.5f) * 2f * headMovementAmount;
        float offsetZ = (Mathf.PerlinNoise(time + 200f, headNoiseOffset + 200f) - 0.5f) * 2f * headMovementAmount;

        Vector3 noiseOffset = new Vector3(offsetX, offsetY, offsetZ) * headNoiseIntensity;

        //new target position
        targetHeadPosition = initialHeadPosition + noiseOffset;

        //Smoothly move the head to taret
        headTarget.localPosition = Vector3.Lerp(headTarget.localPosition, targetHeadPosition, Time.deltaTime * 5f);
    }

}
