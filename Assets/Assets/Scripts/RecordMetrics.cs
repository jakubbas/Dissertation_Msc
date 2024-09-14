using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using UnityEngine.Profiling;

public class RecordMetrics : MonoBehaviour
{
    public PersonalityControllerTest personalityController;
    public int testDurationFrames = 300;
    public bool isKeyframeTest = false;

    private List<float> frameTimes = new List<float>();
    private List<float> framerates = new List<float>();
    private List<float> memoryUsages = new List<float>();
    private string personalityData;

    private int frameCount = 0;
    private bool isRecording = false;
    private float fpsUpdateInterval = 0.5f;
    private float fpsAccumulator = 0f;
    private int fpsFrameCount = 0;
    private float currentFps = 0f;

    void Update()
    {
        if (isRecording)
        {
            RecordMetric();
            frameCount++;

            if (frameCount >= testDurationFrames)
            {
                StopRecording();
            }
        }
    }

    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            frameCount = 0;
            ClearPreviousData();
            if (!isKeyframeTest)
            {
                RecordPersonalityData();
            }
            Debug.Log("Started recording metrics");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            SaveResults();
            Debug.Log("Stopped recording metrics and saved results");
        }
    }

    void ClearPreviousData()
    {
        frameTimes.Clear();
        framerates.Clear();
        memoryUsages.Clear();
    }

    void RecordPersonalityData()
    {
        if (personalityController != null)
        {
            personalityData = $"Openness,Conscientiousness,Extraversion,Agreeableness,Neuroticism\n" +
                              $"{personalityController.openness},{personalityController.conscientiousness}," +
                              $"{personalityController.extraversion},{personalityController.agreeableness}," +
                              $"{personalityController.neuroticism}";
        }
        else
        {
            personalityData = "No personality data available for this test.";
        }
    }

    void RecordMetric()
    {
        float frameTime = Time.unscaledDeltaTime;
        frameTimes.Add(frameTime);

        fpsAccumulator += 1f / frameTime;
        fpsFrameCount++;
        if (Time.unscaledTime > fpsUpdateInterval)
        {
            currentFps = fpsAccumulator / fpsFrameCount;
            fpsAccumulator = 0f;
            fpsFrameCount = 0;
        }
        framerates.Add(currentFps);

        float memoryUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f); //Convertds to MB
        memoryUsages.Add(memoryUsage);
    }

    void SaveResults()
    {
        SavePerformanceData();
        if (!isKeyframeTest)
        {
            SavePersonalityData();
        }
    }

    string GetMetricsPath(string fileName)
    {
        string directory = Path.Combine(Application.dataPath, "Metrics");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return Path.Combine(directory, fileName);
    }

    void SavePerformanceData()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Frame,FrameTime,Framerate,MemoryUsage(MB)");
        for (int i = 0; i < frameTimes.Count; i++)
        {
            sb.AppendLine($"{i},{frameTimes[i]},{framerates[i]},{memoryUsages[i]}");
        }
        string fileName = isKeyframeTest ? "KeyframePerformanceData.csv" : "PerformanceData.csv";
        File.WriteAllText(GetMetricsPath(fileName), sb.ToString());
    }

    void SavePersonalityData()
    {
        File.WriteAllText(GetMetricsPath("PersonalityData.csv"), personalityData);
    }

}