using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PersonalityToEffort;

public class PersonalityToEffort : MonoBehaviour
{
    [System.Serializable]

    //the structure for personaity traits
    public struct PersonalityTraits
    {
        [Range(-1f, 1f)] public float openness;
        [Range(-1f, 1f)] public float conscientiousness;
        [Range(-1f, 1f)] public float extraversion;
        [Range(-1f, 1f)] public float agreeableness;
        [Range(-1f, 1f)] public float neuroticism;
    }

    //the structure for effortss
    [System.Serializable]
    public struct EffortComponents
    {
        public float space;
        public float weight;
        public float time; 
        public float flow;
    }

    public PersonalityTraits personalityTraits;
    public EffortComponents effortComponents;

    //Th matrix which allows to take a series of personalities and get their combined effort value
    private readonly float[][] personalityToEffortMatrix = new float[][]
    {
        new float[] {-0.921f,  0.928f, -0.894f,  0.000f, -1.000f}, // Space
        new float[] { 0.000f,  0.000f,  0.000f, -1.000f,  0.000f}, // Weight
        new float[] { 0.000f, -0.857f,  0.990f, -1.000f,  0.970f}, // Time
        new float[] {-0.931f,  0.938f, -1.000f,  0.000f, -0.762f}  // Flow
    };

    //Clamps to -1 or 1 no matter what.
    public void UpdatePersonality(float o, float c, float e, float a, float n)
    {
        personalityTraits.openness = Mathf.Clamp(o, -1f, 1f);
        personalityTraits.conscientiousness = Mathf.Clamp(c, -1f, 1f);
        personalityTraits.extraversion = Mathf.Clamp(e, -1f, 1f);
        personalityTraits.agreeableness = Mathf.Clamp(a, -1f, 1f);
        personalityTraits.neuroticism = Mathf.Clamp(n, -1f, 1f);

        ConvertPersonalityToEffort();
    }

    //Convert each personality type into its corresponding effort with the use of the matrix. Uses CalculateEffortComponent for the actual math.
    private void ConvertPersonalityToEffort()
    {
        float[] personalityValues = new float[]
        {
            personalityTraits.openness,
            personalityTraits.conscientiousness,
            personalityTraits.extraversion,
            personalityTraits.agreeableness,
            personalityTraits.neuroticism
        };

        effortComponents.space = CalculateEffortComponent(personalityToEffortMatrix[0], personalityValues);
        effortComponents.weight = CalculateEffortComponent(personalityToEffortMatrix[1], personalityValues);
        effortComponents.time = CalculateEffortComponent(personalityToEffortMatrix[2], personalityValues);
        effortComponents.flow = CalculateEffortComponent(personalityToEffortMatrix[3], personalityValues);
    }

    //Math for calculation of each component
    private float CalculateEffortComponent(float[] coefficients, float[] personalityValues)
    {
        float maxPositive = 0f;
        float minNegative = 0f;

        for (int i = 0; i < 5; i++)
        {
            float value = coefficients[i] * personalityValues[i];
            if (value > 0)
                maxPositive = Mathf.Max(maxPositive, value);
            else
                minNegative = Mathf.Min(minNegative, value);
        }

        return maxPositive + minNegative;
    }

    public EffortComponents GetEffortComponents()
    {
        return effortComponents;
    }


}
