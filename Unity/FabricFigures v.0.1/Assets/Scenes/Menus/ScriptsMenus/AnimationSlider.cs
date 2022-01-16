using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AnimationSlider : MonoBehaviour
{


    public Slider slider;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnValueChanged()
    {
        animator.Play("Slider", -1, slider.normalizedValue);  
    }
}
