using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimControlTest : MonoBehaviour
{
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("StartWalk");
        }
    }
}
