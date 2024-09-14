using UnityEngine;

[CreateAssetMenu(fileName = "Personality", menuName = "Personality/OCEANPersonality", order = 1)]
public class OCEAN_SO : ScriptableObject
{
    [TextArea(10, 100)]
    public string description; 

    [Header("Personality Traits")]
    [Range(-1f, 1f)] public float openness;
    [Range(-1f, 1f)] public float conscientiousness;
    [Range(-1f, 1f)] public float extraversion;
    [Range(-1f, 1f)] public float agreeableness;
    [Range(-1f, 1f)] public float neuroticism;



}
