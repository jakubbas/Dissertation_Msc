using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RecordAnim : MonoBehaviour
{
    public float recordingDuration = 5f;
    public float frameRate = 30f;
    public string animationName = "RecordedAnimation";
    public bool recordOneCycleOnly = false;
    public float cycleDuration = 1f;

    public Transform rootBone; 
    public bool recordRootMotion = true;

    private Vector3 initialRootPosition;
    private Quaternion initialRootRotation;



    public List<Transform> bonestoRecord; 

    private float recordingTimer;
    private bool isRecording = false;
    private AnimationClip recordedClip;
    private Dictionary<Transform, List<TransformKeyframe>> keyframes;

    [System.Serializable]
    private class TransformKeyframe
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public void StartRecording()
    {
        recordingTimer = 0f;
        isRecording = true;
        recordedClip = new AnimationClip();
        keyframes = new Dictionary<Transform, List<TransformKeyframe>>();

        Debug.Log("Recording started...");
    }

    private void Update()
    {
        if (isRecording)
        {
            recordingTimer += Time.deltaTime;

            if ((recordOneCycleOnly && recordingTimer >= cycleDuration) ||
                (!recordOneCycleOnly && recordingTimer >= recordingDuration))
            {
                StopRecording();
            }
            else
            {
                RecordFrame();
            }
        }
    }

    private void RecordFrame()
    {
        if (recordRootMotion && rootBone != null)
        {
            RecordRootMotion();
        }

        foreach (Transform bone in bonestoRecord)
        {
            if (bone != null)
            {
                RecordTransform(bone);
            }
        }
    }

    private void RecordRootMotion()
    {
        if (!keyframes.ContainsKey(rootBone))
        {
            keyframes[rootBone] = new List<TransformKeyframe>();
        }

        keyframes[rootBone].Add(new TransformKeyframe
        {
            time = recordingTimer,
            position = rootBone.position - initialRootPosition,
            rotation = Quaternion.Inverse(initialRootRotation) * rootBone.rotation,
            scale = rootBone.localScale
        });
    }

    private void RecordTransform(Transform target)
    {
        if (!keyframes.ContainsKey(target))
        {
            keyframes[target] = new List<TransformKeyframe>();
        }

        keyframes[target].Add(new TransformKeyframe
        {
            time = recordingTimer,
            position = target.position,
            rotation = target.rotation,
        });
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        Debug.Log("Recording stopped. Creating animation clip...");

        foreach (var kvp in keyframes)
        {
            Transform target = kvp.Key;
            List<TransformKeyframe> transformKeyframes = kvp.Value;

            string relativePath = AnimationUtility.CalculateTransformPath(target, transform);

            AnimationCurve posX = new AnimationCurve();
            AnimationCurve posY = new AnimationCurve();
            AnimationCurve posZ = new AnimationCurve();
            AnimationCurve rotX = new AnimationCurve();
            AnimationCurve rotY = new AnimationCurve();
            AnimationCurve rotZ = new AnimationCurve();
            AnimationCurve rotW = new AnimationCurve();

            bool hasMovement = false;
            Vector3 initialPos = transformKeyframes[0].position;
            Quaternion initialRot = transformKeyframes[0].rotation;

            foreach (var keyframe in transformKeyframes)
            {
                posX.AddKey(keyframe.time, keyframe.position.x);
                posY.AddKey(keyframe.time, keyframe.position.y);
                posZ.AddKey(keyframe.time, keyframe.position.z);
                rotX.AddKey(keyframe.time, keyframe.rotation.x);
                rotY.AddKey(keyframe.time, keyframe.rotation.y);
                rotZ.AddKey(keyframe.time, keyframe.rotation.z);
                rotW.AddKey(keyframe.time, keyframe.rotation.w);
                if (!hasMovement && (keyframe.position != initialPos || keyframe.rotation != initialRot))
                {
                    hasMovement = true;
                }
            }

            if (target == rootBone && recordRootMotion)
            {
                recordedClip.SetCurve("", typeof(Animator), "RootT.x", posX);
                recordedClip.SetCurve("", typeof(Animator), "RootT.y", posY);
                recordedClip.SetCurve("", typeof(Animator), "RootT.z", posZ);
                recordedClip.SetCurve("", typeof(Animator), "RootQ.x", rotX);
                recordedClip.SetCurve("", typeof(Animator), "RootQ.y", rotY);
                recordedClip.SetCurve("", typeof(Animator), "RootQ.z", rotZ);
                recordedClip.SetCurve("", typeof(Animator), "RootQ.w", rotW);
            }
            else
            {
                recordedClip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
                recordedClip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
                recordedClip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);
                recordedClip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
                recordedClip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
                recordedClip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
                recordedClip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);
            }

        }

        recordedClip.frameRate = frameRate;

        SaveAnimationClip();
    }

    private void SaveAnimationClip()
    {
        string path = $"Assets/Anims/{animationName}.anim";
        AssetDatabase.CreateAsset(recordedClip, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"Animation clip saved at: {path}");
    }
}
