using UnityEngine;

public class PersonalityControllerTest : MonoBehaviour
{
    public MotionParameter motionParameter;
    public ShapeQuality shapeQuality;
    public PersonalityToEffort personalityToEffort;
    public MotionParameterApplier motionParameterApplier;
    public RecordAnim animationRecorder;
    public RecordMetrics animationSystemTester;
    public OCEAN_SO personality;

    public bool useCustom;

    [Header("Personality Traits")]
    [Range(-1f, 1f)] public float openness;
    [Range(-1f, 1f)] public float conscientiousness;
    [Range(-1f, 1f)] public float extraversion;
    [Range(-1f, 1f)] public float agreeableness;
    [Range(-1f, 1f)] public float neuroticism;

    public float hudOffset;

    void UpdatePersonalityAndStartRecording()
    {
        TestPersonality();
        if (animationRecorder != null)
        {
            animationRecorder.StartRecording();
        }
    }

    public void TestPersonality()
    {
        if (useCustom)
        {
            personalityToEffort.UpdatePersonality(openness, conscientiousness, extraversion, agreeableness, neuroticism);
        }
        else
        {
            personalityToEffort.UpdatePersonality(personality.openness, personality.conscientiousness, personality.extraversion, personality.agreeableness, personality.neuroticism);
        }

        motionParameter.CalculateMotionParameters();
        motionParameterApplier.UpdateMovementParameters();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10 + hudOffset, 10, 250, 30), "Update Personality and Start Recording"))
        {
            UpdatePersonalityAndStartRecording();
        }

        if (GUI.Button(new Rect(10 + hudOffset, 50, 150, 30), "UpdatePersonality"))
        {
            TestPersonality();
        }

        if (GUI.Button(new Rect(10 + hudOffset, 90, 150, 30), "Stop Recording"))
        {
            if (animationRecorder != null)
            {
                animationRecorder.StopRecording();
            }
        }

        if (GUI.Button(new Rect(10 + hudOffset, 130, 150, 30), "Start Recording Metrics"))
        {
            if (animationSystemTester != null)
            {
                animationSystemTester.StartRecording();
            }
        }
    }
}
