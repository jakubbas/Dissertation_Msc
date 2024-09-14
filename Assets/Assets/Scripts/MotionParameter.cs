using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionParameter : MonoBehaviour
{

    public ShapeQualities shapeQualities;

    [System.Serializable]
    public class ShapeQualities
    {
        public float enclosingSpreadingFactor;  
        public float sinkingRisingFactor;       
        public float retreatingAdvancingFactor; 
    }
    [System.Serializable]
    public struct MotionParameters
    {
        public float animationSpeed;
        public float anticipationVelocity;
        public float overshootVelocity;
        public float anticipationTime;
        public float overshootTime;
        public float timeExponent;
        public float wristBend;
        public float wristTwist;
        public float wristFrequency;
        public float elbowTwist;
        public float elbowDisplacement;
        public float elbowFrequency;
        public float torsoRotationMagnitude;
        public float torsoRotationFrequency;
        public float headRotationMagnitude;
        public float headRotationFrequency;
    }


    public PersonalityToEffort effortConverter;
    public MotionParameters motionParameters;


    private readonly float[,] motionCoefficients = new float[,]
    {
        //  Intercept, Space, Weight, Time, Flow
        { 0.558f, -0.000f,  0.001f,  0.470f,  0.001f }, //Animation Speed
        { 0.223f, -0.011f,  0.297f,  0.000f, -0.029f }, //Anticipation Velocity
        { 0.344f, -0.042f, -0.042f,  0.000f, -0.458f }, //Overshoot Velocity
        { 0.031f, -0.002f,  0.041f,  0.008f, -0.002f }, //Anticipation Time
        { 0.930f,  0.015f,  0.018f, -0.015f,  0.092f }, //Overshoot Time
        { 1.043f,  0.015f,  0.008f,  0.072f,  0.060f }, //Time Exponent
        { 0.191f, -0.008f, -0.238f,  0.000f, -0.025f }, //Wrist Bend
        { 0.160f, -0.010f, -0.053f,  0.010f, -0.196f }, //Wrist Twist
        { 0.848f, -0.040f, -0.760f, -0.150f, -0.381f }, //Wrist Frequency
        { 0.281f, -0.009f,  0.039f, -0.005f, -0.313f }, //Elbow Twist
        { 0.164f, -0.016f, -0.017f,  0.035f, -0.161f }, //Elbow Displacement
        { 0.735f,  0.015f,  0.041f,  0.020f, -0.809f }, //Elbow Frequency
        { 0.290f, -0.043f,  0.040f,  0.010f, -0.331f }, //Torso Rotation Magnitude
        { 1.283f, -0.179f,  0.223f,  0.067f, -1.410f }, //Torso Rotation Frequency
        { 1.210f, -0.804f,  0.008f,  0.004f, -0.178f }, //Head Rotation Magnitude
        { 1.078f, -1.225f,  0.104f, -0.017f,  0.184f }  //Head Rotation Frequency
    };

    //Calculates each motion paramter for each present effort. Pretty costly calculation, as it is not linear.
    public void CalculateMotionParameters()
    {
        PersonalityToEffort.EffortComponents efforts = effortConverter.GetEffortComponents();

        motionParameters.animationSpeed = CalculateParameter(0, efforts);
        motionParameters.anticipationVelocity = CalculateParameter(1, efforts);
        motionParameters.overshootVelocity = CalculateParameter(2, efforts);
        motionParameters.anticipationTime = CalculateParameter(3, efforts);
        motionParameters.overshootTime = CalculateParameter(4, efforts);
        motionParameters.timeExponent = CalculateParameter(5, efforts);
        motionParameters.wristBend = CalculateParameter(6, efforts);
        motionParameters.wristTwist = CalculateParameter(7, efforts);
        motionParameters.wristFrequency = CalculateParameter(8, efforts);
        motionParameters.elbowTwist = CalculateParameter(9, efforts);
        motionParameters.elbowDisplacement = CalculateParameter(10, efforts);
        motionParameters.elbowFrequency = CalculateParameter(11, efforts);
        motionParameters.torsoRotationMagnitude = CalculateParameter(12, efforts);
        motionParameters.torsoRotationFrequency = CalculateParameter(13, efforts);
        motionParameters.headRotationMagnitude = CalculateParameter(14, efforts);
        motionParameters.headRotationFrequency = CalculateParameter(15, efforts);

        CalculateShapeQualities(efforts);
    }
    //Math for calculating motion params.
    private float CalculateParameter(int index, PersonalityToEffort.EffortComponents efforts)
    {
        return motionCoefficients[index, 0] +
               motionCoefficients[index, 1] * efforts.space +
               motionCoefficients[index, 2] * efforts.weight +
               motionCoefficients[index, 3] * efforts.time +
               motionCoefficients[index, 4] * efforts.flow;
    }

    //TODO
    public void CalculateShapeQualities(PersonalityToEffort.EffortComponents effortComponents)
    {
        shapeQualities = new ShapeQualities();

        //Enclosing/Spreading (affected by Space - Indirect/Direct)
        shapeQualities.enclosingSpreadingFactor = -effortComponents.space;

        //Sinking/Rising (affected by Weight - Strong/Light)
        shapeQualities.sinkingRisingFactor = -effortComponents.weight;

        //Retreating/Advancing (affected by Time - Sudden/Sustained)
        shapeQualities.retreatingAdvancingFactor = -effortComponents.time;

        ApplyAdditionalInfluences(effortComponents);
    }
    //fFor more nuanced definition between shape qualities.
    private void ApplyAdditionalInfluences(PersonalityToEffort.EffortComponents effortComponents)
    {
        float influenceStrength = 0.2f;

        //Existing influences
        shapeQualities.enclosingSpreadingFactor += effortComponents.flow * influenceStrength;
        shapeQualities.sinkingRisingFactor -= effortComponents.flow * influenceStrength;

        shapeQualities.retreatingAdvancingFactor += effortComponents.weight * 0.1f;
        shapeQualities.enclosingSpreadingFactor -= effortComponents.time * 0.1f;

        float neuroticismInfluence = effortComponents.flow * 0.15f;
        shapeQualities.enclosingSpreadingFactor += neuroticismInfluence;
        shapeQualities.sinkingRisingFactor += neuroticismInfluence;
        shapeQualities.retreatingAdvancingFactor -= neuroticismInfluence;

        //final values
        shapeQualities.enclosingSpreadingFactor = Mathf.Clamp(shapeQualities.enclosingSpreadingFactor, -1f, 1f);
        shapeQualities.sinkingRisingFactor = Mathf.Clamp(shapeQualities.sinkingRisingFactor, -1f, 1f);
        shapeQualities.retreatingAdvancingFactor = Mathf.Clamp(shapeQualities.retreatingAdvancingFactor, -1f, 1f);
    }

    public void PrintMotionParametersAndShapeQualities()
    {
        Debug.Log("Motion Parameters:");
        Debug.Log($"Animation Speed: {motionParameters.animationSpeed}");
        Debug.Log($"Anticipation Velocity: {motionParameters.anticipationVelocity}");
        Debug.Log($"Overshoot Velocity: {motionParameters.overshootVelocity}");
        Debug.Log($"Anticipation Time: {motionParameters.anticipationTime}");
        Debug.Log($"Overshoot Time: {motionParameters.overshootTime}");
        Debug.Log($"Time Exponent: {motionParameters.timeExponent}");
        Debug.Log($"Wrist Bend: {motionParameters.wristBend}");
        Debug.Log($"Wrist Twist: {motionParameters.wristTwist}");
        Debug.Log($"Wrist Frequency: {motionParameters.wristFrequency}");
        Debug.Log($"Elbow Twist: {motionParameters.elbowTwist}");
        Debug.Log($"Elbow Displacement: {motionParameters.elbowDisplacement}");
        Debug.Log($"Elbow Frequency: {motionParameters.elbowFrequency}");
        Debug.Log($"Torso Rotation Magnitude: {motionParameters.torsoRotationMagnitude}");
        Debug.Log($"Torso Rotation Frequency: {motionParameters.torsoRotationFrequency}");
        Debug.Log($"Head Rotation Magnitude: {motionParameters.headRotationMagnitude}");
        Debug.Log($"Head Rotation Frequency: {motionParameters.headRotationFrequency}");
    }

    public void PrintEffortComponents()
    {
        var efforts = effortConverter.GetEffortComponents();
        Debug.Log("\nEffort Components:");
        Debug.Log($"Space: {efforts.space}");
        Debug.Log($"Weight: {efforts.weight}");
        Debug.Log($"Time: {efforts.time}");
        Debug.Log($"Flow: {efforts.flow}");
    }

    public void PrintOCEAN()
    {
        Debug.Log("\nOCEAN:");
        Debug.Log($"Openness: {effortConverter.personalityTraits.openness}");
        Debug.Log($"Conscientiousness: {effortConverter.personalityTraits.conscientiousness}");
        Debug.Log($"Extraversion: {effortConverter.personalityTraits.extraversion}");
        Debug.Log($"Agreeableness: {effortConverter.personalityTraits.agreeableness}");
        Debug.Log($"Neuroticism: {effortConverter.personalityTraits.neuroticism}");
    }

}
