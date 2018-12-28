using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimatorParameter : MonoBehaviour
{
    public Animator anim;
    public string nameOfParameter;

#if UNITY_EDITOR
    private void Reset()
    {
        anim = GetComponent<Animator>();
    }
#endif

    void Start()
    {
        anim.SetFloat(nameOfParameter, Random.value);
    }
}
