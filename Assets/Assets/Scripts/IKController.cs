using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKController : MonoBehaviour
{


    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;


    public Transform leftFootIKTarget; 
    public Transform rightFootIKTarget; 

    public float leftHandIKWeight = 1.0f;
    public float rightHandIKWeight = 1.0f;


    public float leftFootIKWeight = 1.0f;
    public float rightFootIKWeight = 1.0f;

    public Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    //called every frame after the animator updates. WILL NEED TO OPTIMISE THIS.
    void OnAnimatorIK(int layerIndex)
    {
        //If animator and each individual IK target aren't null, apply the IK to each part individually.
        if (animator != null)
        {

            Debug.Log("Not Null");
            //if (leftHandIKTarget != null)
            //{
            //    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            //    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            //    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
            //    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
            //}

            //if (rightHandIKTarget != null)
            //{
            //    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
            //    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
            //    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
            //    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
            //}
            //if (leftFootIKTarget != null)
            //{
            //    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootIKWeight);
            //    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootIKWeight);
            //    animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKTarget.position);
            //    animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootIKTarget.rotation);
            //}

            //if (rightFootIKTarget != null)
            //{
            //    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootIKWeight);
            //    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootIKWeight);
            //    animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKTarget.position);
            //    animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootIKTarget.rotation);
            //}

        }

    }
}
