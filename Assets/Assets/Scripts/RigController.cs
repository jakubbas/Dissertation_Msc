using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static MotionParameter;

public class RigController : MonoBehaviour
{

    public MotionParameter motionCalculator;
    public Animator animator;


    [System.Serializable]
    public class ArmRig
    {
        public Rig rig;
        public TwoBoneIKConstraint ikConstraint;
        public Transform targetTransform;
        public Transform hintTransform;
        [HideInInspector] public Vector3 initialTargetPos;
        [HideInInspector] public Vector3 initialHintPos;
    }

    [System.Serializable]
    public class LegRig
    {
        public Rig rig;
        public TwoBoneIKConstraint ikConstraint;
        public Transform targetTransform;
        public Transform hintTransform;
        [HideInInspector] public Vector3 initialTargetPos;
        [HideInInspector] public Vector3 initialHintPos;
    }

    [System.Serializable]
    public class TorsoRig
    {
        public Rig rig;
        public MultiAimConstraint aimConstraint;
        public Transform targetTransform;
        [HideInInspector] public Vector3 initialTargetPos;
        [HideInInspector] public Vector3 neutralTargetPos;
    }

    [System.Serializable]
    public class HeadRig
    {
        public Rig rig;
        public MultiAimConstraint aimConstraint;
        public Transform targetTransform;
        [HideInInspector] public Vector3 initialTargetPos;
        [HideInInspector] public Vector3 neutralTargetPos;
    }

    public ArmRig leftArm, rightArm;
    public LegRig leftLeg, rightLeg;
    public TorsoRig torso;
    public HeadRig head;

    private float timeSinceStart = 0f;

    [System.Serializable]
    public class GeneralSettings
    {
        [Range(0.1f, 2f)]
        public float speedMultiplier = 1f;
        [Range(0.01f, 0.5f)]
        public float movementIntensity = 0.1f;
    }

    [System.Serializable]
    public class ArmSettings
    {
        [Range(0.1f, 0.5f)]
        public float armSwingAmount = 0.2f;
        [Range(0.5f, 2f)]
        public float armSwingFrequency = 1f;
        [Range(0.1f, 0.5f)]
        public float minArmSpread = 0.3f;
        [Range(0.5f, 1f)]
        public float maxArmSpread = 0.7f;
        [Range(0f, 2f)]
        public float wristBendInfluence = 0.02f;
        [Range(0f, 2f)]
        public float wristTwistInfluence = 0.02f;
        [Range(0f, 2f)]
        public float elbowTwistInfluence = 0.02f;
        [Range(0f, 2f)]
        public float elbowDisplacementInfluence = 0.02f;
    }

    [System.Serializable]
    public class LegSettings
    {
        [Range(0f, 0.2f)]
        public float sinkingRisingInfluence = 0.05f;
        [Range(0f, 0.1f)]
        public float anticipationOvershootInfluence = 0.025f;
        [Range(0f, 0.2f)]
        public float timeExponentInfluence = 0.05f;
    }

    [System.Serializable]
    public class TorsoSettings
    {
        [Range(0f, 0.5f)]
        public float torsoRotationInfluence = 0.1f;
        [Range(0f, 0.5f)]
        public float enclosingSpreadingInfluence = 0.1f;
        [Range(0f, 0.5f)]
        public float retreatingAdvancingInfluence = 0.1f;
        [Range(0f, 0.2f)]
        public float anticipationOvershootInfluence = 0.05f;
    }

    [System.Serializable]
    public class HeadSettings
    {
        [Range(0f, 0.5f)]
        public float headRotationInfluence = 0.1f;
        [Range(0f, 0.2f)]
        public float anticipationOvershootInfluence = 0.05f;
    }

    [System.Serializable]
    public class EffortInfluence
    {
        [Range(0.8f, 1.2f)]
        public float flowMin = 0.9f;
        [Range(0.8f, 1.2f)]
        public float flowMax = 1.1f;
        [Range(0.8f, 1.2f)]
        public float spaceMin = 0.95f;
        [Range(0.8f, 1.2f)]
        public float spaceMax = 1.05f;
        [Range(0.8f, 1.2f)]
        public float weightMin = 0.9f;
        [Range(0.8f, 1.2f)]
        public float weightMax = 1.1f;
        [Range(0.8f, 1.2f)]
        public float timeMin = 0.9f;
        [Range(0.8f, 1.2f)]
        public float timeMax = 1.1f;
    }

    [Header("Rig Settings")]
    public GeneralSettings generalSettings;
    public ArmSettings armSettings;
    public LegSettings legSettings;
    public TorsoSettings torsoSettings;
    public HeadSettings headSettings;
    public EffortInfluence effortInfluence;


    [SerializeField] private float movementIntensity = 0.1f; 
    void Start()
    {
        InitializeRigs();
    }

    private void InitializeRigs()
    {
        Vector3 characterForward = transform.forward;
        Vector3 characterUp = transform.up;

        head.initialTargetPos = head.targetTransform.localPosition;
        head.neutralTargetPos = characterForward * 2f + characterUp * 1.7f; 

        leftArm.initialTargetPos = leftArm.targetTransform.localPosition;
        leftArm.initialHintPos = leftArm.hintTransform.localPosition;
        rightArm.initialTargetPos = rightArm.targetTransform.localPosition;
        rightArm.initialHintPos = rightArm.hintTransform.localPosition;

        leftLeg.initialTargetPos = leftLeg.targetTransform.localPosition;
        leftLeg.initialHintPos = leftLeg.hintTransform.localPosition;
        rightLeg.initialTargetPos = rightLeg.targetTransform.localPosition;
        rightLeg.initialHintPos = rightLeg.hintTransform.localPosition;

        torso.initialTargetPos = torso.targetTransform.localPosition;
        head.initialTargetPos = head.targetTransform.localPosition;
    }

    void Update()
    {
        timeSinceStart += Time.deltaTime;
        UpdateRigs();
    }


    private void UpdateRigs()
    {
        var mp = motionCalculator.motionParameters;
        var sq = motionCalculator.shapeQualities;

        animator.speed = Mathf.Lerp(1f, mp.animationSpeed, 0.2f) * generalSettings.speedMultiplier;

        UpdateArmRig(leftArm, mp, 1);
        UpdateArmRig(rightArm, mp, -1);

        UpdateLegRig(leftLeg, mp, sq, 1);
        UpdateLegRig(rightLeg, mp, sq, -1);

        UpdateTorsoRig(mp, sq);
        UpdateHeadRig(mp);

        UpdateRigWeights();
    }

    private Vector3 CalculateArmPosition(float phase, float side)
    {
        float swing = Mathf.Sin(phase) * armSettings.armSwingAmount;

        float vertical = Mathf.Sin(phase * 2) * armSettings.armSwingAmount * 0.02f; 

        float lateral = Mathf.Cos(phase) * armSettings.armSwingAmount * 0.03f;

        return new Vector3(lateral * side, vertical, swing * side);
    }

    private void UpdateArmRig(ArmRig arm, MotionParameters mp, float side)
    {
        float walkCycle = timeSinceStart * armSettings.armSwingFrequency * Mathf.PI * 2 * animator.speed;

        // fixed shoulder pos
        Vector3 shoulderPos = arm.ikConstraint.data.root.position;

        Vector3 handOffset = CalculateArmPosition(walkCycle, side);

        var personality = motionCalculator.effortConverter.personalityTraits;
        var efforts = motionCalculator.effortConverter.GetEffortComponents();

        float armWeight = CalculateArmWeight(personality, efforts);

        handOffset.y -= armSettings.armSwingAmount * 1.2f; 
        handOffset.x += side * armSettings.armSwingAmount * armWeight; 
        Vector3 targetHandPos = shoulderPos + handOffset;

        float elbowPhase = walkCycle + (side * Mathf.PI * 0.1f); 
        Vector3 elbowOffset = CalculateArmPosition(elbowPhase, side) * 0.2f; 
        elbowOffset.x += side * armSettings.armSwingAmount * 0.6f * armWeight; 
        elbowOffset.y -= armSettings.armSwingAmount * 0.6f; //elbow lowered HERE!!!
        Vector3 targetElbowPos = Vector3.Lerp(shoulderPos, targetHandPos, 0.5f) + elbowOffset;

        float personalityEffect = generalSettings.movementIntensity * 0.1f;
        targetHandPos.y += SafeFloat(mp.wristBend * armSettings.wristBendInfluence * side * personalityEffect);
        targetHandPos.x += SafeFloat(mp.wristTwist * armSettings.wristTwistInfluence * personalityEffect);
        targetElbowPos.x += SafeFloat(mp.elbowTwist * armSettings.elbowTwistInfluence * side * personalityEffect);
        targetElbowPos.y += SafeFloat(mp.elbowDisplacement * armSettings.elbowDisplacementInfluence * personalityEffect);

        //update positions
        arm.targetTransform.position = Vector3.Lerp(arm.targetTransform.position, targetHandPos, Time.deltaTime * 10f);
        arm.hintTransform.position = Vector3.Lerp(arm.hintTransform.position, targetElbowPos, Time.deltaTime * 10f);
    }

    private float ClampArmSpread(float spread)
    {
        return Mathf.Clamp(spread, armSettings.minArmSpread, armSettings.maxArmSpread);
    }

    private void UpdateLegRig(LegRig leg, MotionParameters mp, ShapeQualities sq, float side)
    {
        Vector3 newTargetPos = leg.initialTargetPos;
        Vector3 newHintPos = leg.initialHintPos;

        //sinking/rising factor with lspess intensity
        newTargetPos.y += SafeFloat(sq.sinkingRisingFactor * 0.05f * movementIntensity);

        float anticipationFactor = SafeFloat(Mathf.Sin(timeSinceStart * 2f * Mathf.PI / SafeFloat(mp.anticipationTime, 0.01f)) * mp.anticipationVelocity * 0.25f * movementIntensity);
        float overshootFactor = SafeFloat(Mathf.Sin(timeSinceStart * 2f * Mathf.PI / SafeFloat(mp.overshootTime, 0.01f)) * mp.overshootVelocity * 0.25f * movementIntensity);
        newTargetPos += SafeVector3((Vector3.forward * anticipationFactor + Vector3.back * overshootFactor) * 0.025f);

        float timeEffect = SafeFloat(Mathf.Pow(Mathf.Sin(timeSinceStart * Mathf.PI), SafeFloat(mp.timeExponent, 1f)));
        newTargetPos.z += SafeFloat(timeEffect * 0.05f * side * movementIntensity);

        leg.targetTransform.localPosition = newTargetPos;
        leg.hintTransform.localPosition = newHintPos;
    }

    private void UpdateTorsoRig(MotionParameters mp, ShapeQualities sq)
    {
        Vector3 newPos = torso.initialTargetPos;

        float torsoRotation = Mathf.Sin(timeSinceStart * mp.torsoRotationFrequency) * mp.torsoRotationMagnitude * torsoSettings.torsoRotationInfluence;
        newPos.x += torsoRotation * generalSettings.movementIntensity;

        newPos.x += sq.enclosingSpreadingFactor * torsoSettings.enclosingSpreadingInfluence * generalSettings.movementIntensity;
        newPos.z += sq.retreatingAdvancingFactor * torsoSettings.retreatingAdvancingInfluence * generalSettings.movementIntensity;

        float anticipationFactor = Mathf.Sin(timeSinceStart * 2f * Mathf.PI / mp.anticipationTime) * mp.anticipationVelocity;
        float overshootFactor = Mathf.Sin(timeSinceStart * 2f * Mathf.PI / mp.overshootTime) * mp.overshootVelocity;
        Vector3 anticipationOvershoot = (Vector3.forward * anticipationFactor + Vector3.back * overshootFactor) * torsoSettings.anticipationOvershootInfluence * generalSettings.movementIntensity;
        newPos += anticipationOvershoot;

        torso.targetTransform.localPosition = Vector3.Lerp(torso.targetTransform.localPosition, newPos, Time.deltaTime * 5f);

        Quaternion targetRotation = Quaternion.Euler(0, torsoRotation * 30f, 0); // Convert torso rotation to degrees
        torso.targetTransform.localRotation = Quaternion.Slerp(torso.targetTransform.localRotation, targetRotation, Time.deltaTime * 5f);
    }

    private void UpdateHeadRig(MotionParameters mp)
    {
    Vector3 newPos = head.initialTargetPos;

    float headRotationX = SafeFloat(Mathf.Sin(timeSinceStart * SafeFloat(mp.headRotationFrequency)) * mp.headRotationMagnitude * movementIntensity * 2f);
    float headRotationY = SafeFloat(Mathf.Cos(timeSinceStart * SafeFloat(mp.headRotationFrequency * 0.7f)) * mp.headRotationMagnitude * 1.4f * movementIntensity);
    newPos.x += SafeFloat(headRotationX * 0.1f);
    newPos.y += SafeFloat(headRotationY * 0.1f);

    float anticipationFactor = SafeFloat(Mathf.Sin(timeSinceStart * 2f * Mathf.PI / SafeFloat(mp.anticipationTime, 0.01f)) * mp.anticipationVelocity * 0.3f * movementIntensity);
    float overshootFactor = SafeFloat(Mathf.Sin(timeSinceStart * 2f * Mathf.PI / SafeFloat(mp.overshootTime, 0.01f)) * mp.overshootVelocity * 0.3f * movementIntensity);
    newPos += SafeVector3((Vector3.forward * anticipationFactor + Vector3.back * overshootFactor) * 0.05f);

    head.targetTransform.localPosition = Vector3.Lerp(head.targetTransform.localPosition, newPos, Time.deltaTime * 5f);
    }

    private float SafeFloat(float value, float defaultValue = 0f)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return defaultValue;
        return value;
    }

    private Vector3 SafeVector3(Vector3 vector)
    {
        return new Vector3(
            SafeFloat(vector.x),
            SafeFloat(vector.y),
            SafeFloat(vector.z)
        );
    }

    private void UpdateRigWeights()
    {
        var personalityConverter = motionCalculator.effortConverter;
        var personality = personalityConverter.personalityTraits;
        var efforts = personalityConverter.GetEffortComponents();

        float armWeight = CalculateArmWeight(personality, efforts);
        float legWeight = CalculateLegWeight(personality, efforts);
        float torsoWeight = CalculateTorsoWeight(personality, efforts);
        float headWeight = CalculateHeadWeight(personality, efforts);

        leftArm.rig.weight = rightArm.rig.weight = armWeight;
        leftLeg.rig.weight = rightLeg.rig.weight = legWeight;
        torso.rig.weight = torsoWeight;
        head.rig.weight = headWeight;
    }


    private float CalculateArmWeight(PersonalityToEffort.PersonalityTraits personality, PersonalityToEffort.EffortComponents efforts)
    {
        float extraversionInfluence = personality.extraversion; 
        float normalizedExtraversion = (extraversionInfluence + 1f) / 2f; 

        float spread = ClampArmSpread(normalizedExtraversion);

        float weight = spread; 

        weight *= Mathf.Lerp(effortInfluence.flowMin, effortInfluence.flowMax, (efforts.flow + 1f) / 2f);
        weight *= Mathf.Lerp(effortInfluence.spaceMin, effortInfluence.spaceMax, (efforts.space + 1f) / 2f);

        return Mathf.Clamp01(weight);
    }
    private float CalculateLegWeight(PersonalityToEffort.PersonalityTraits personality, PersonalityToEffort.EffortComponents efforts)
    {
        float weight = (personality.conscientiousness + 1f) / 2f;
        weight *= Mathf.Lerp(effortInfluence.weightMin, effortInfluence.weightMax, (efforts.weight + 1f) / 2f);
        weight *= Mathf.Lerp(effortInfluence.timeMax, effortInfluence.timeMin, (efforts.time + 1f) / 2f);
        return Mathf.Clamp01(weight);
    }
    private float CalculateTorsoWeight(PersonalityToEffort.PersonalityTraits personality, PersonalityToEffort.EffortComponents efforts)
    {
        float weight = (personality.openness + 1f) / 2f;
        weight *= Mathf.Lerp(effortInfluence.spaceMin, effortInfluence.spaceMax, (efforts.space + 1f) / 2f);
        weight *= Mathf.Lerp(effortInfluence.flowMax, effortInfluence.flowMin, (efforts.flow + 1f) / 2f);
        return Mathf.Clamp01(weight);
    }
    private float CalculateHeadWeight(PersonalityToEffort.PersonalityTraits personality, PersonalityToEffort.EffortComponents efforts)
    {
        float weight = (personality.extraversion + 1f) / 2f;
        weight *= Mathf.Lerp(0.9f, 1.1f, (personality.neuroticism + 1f) / 2f);
        weight *= Mathf.Lerp(effortInfluence.timeMax, effortInfluence.timeMin, (efforts.time + 1f) / 2f);
        return Mathf.Clamp01(weight);
    }
    private Vector3 SmoothPosition(Vector3 current, Vector3 target, float smoothTime = 0.1f)
    {
        Vector3 velocity = Vector3.zero;
        return Vector3.SmoothDamp(current, target, ref velocity, smoothTime);
    }

    //WAS USED FOR TESTING ORIGINALLY. 
    private void ApplyPersonalityInfluence(PersonalityToEffort.PersonalityTraits personality)
    {
        float subtlety = Mathf.Lerp(0.8f, 1.2f, (personality.agreeableness + 1) / 2);

        leftArm.rig.weight *= subtlety;
        rightArm.rig.weight *= subtlety;
        leftLeg.rig.weight *= subtlety;
        rightLeg.rig.weight *= subtlety;
        torso.rig.weight *= subtlety;
        head.rig.weight *= subtlety;

        float randomness = Mathf.Lerp(0, 0.1f, (personality.neuroticism + 1) / 2);
        leftArm.rig.weight += Random.Range(-randomness, randomness);
        rightArm.rig.weight += Random.Range(-randomness, randomness);
        leftLeg.rig.weight += Random.Range(-randomness, randomness);
        rightLeg.rig.weight += Random.Range(-randomness, randomness);
        torso.rig.weight += Random.Range(-randomness, randomness);
        head.rig.weight += Random.Range(-randomness, randomness);

        leftArm.rig.weight = Mathf.Clamp01(leftArm.rig.weight);
        rightArm.rig.weight = Mathf.Clamp01(rightArm.rig.weight);
        leftLeg.rig.weight = Mathf.Clamp01(leftLeg.rig.weight);
        rightLeg.rig.weight = Mathf.Clamp01(rightLeg.rig.weight);
        torso.rig.weight = Mathf.Clamp01(torso.rig.weight);
        head.rig.weight = Mathf.Clamp01(head.rig.weight);
    }

}
