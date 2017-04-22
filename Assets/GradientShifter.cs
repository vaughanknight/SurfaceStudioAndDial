using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public class GradientShifter : MonoBehaviour {
    public enum Foo
    {
        a, b, c
    }

    public ParticlesArea _ParticlesArea;
    public SpriteRenderer _SpriteRenderer;

    private float _hueCounter = 0;
    private Gradient _startGradient;

    // Use this for initialization
    void Start ()
    {
        _ParticlesArea.m_updateGradient = true;
        _startGradient = _ParticlesArea.m_colourGradient;
        
        StartGradientTransition();
    }
    
    private void StartGradientTransition()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 30f, "onupdate", "OnValueUpdate", "oncomplete", "OnValueComplete", "easeType", iTween.EaseType.easeInOutSine));
    }

    private int _gradientIndex = 0;

    public void OnValueUpdate(float offset)
    {
        var newColorKeys = new GradientColorKey[_startGradient.colorKeys.Length];

        // Create the gradient
        var newGradient = new Gradient();
        newGradient.alphaKeys = _startGradient.alphaKeys;
        newGradient.mode = GradientMode.Blend;

        // Let's get a copy as a starting point
        // for the colour keys
        newColorKeys = _startGradient.colorKeys;

        // Shift the hue of the colour keys
        ShiftColourKeysByHueOffset(offset, newColorKeys);

        // Assign to the array, assigning invidividual elements
        // doesn't update the gradient
        newGradient.colorKeys = newColorKeys;

        // Assign the gradient
        _ParticlesArea.m_colourGradient = newGradient;
    }

    /// <summary>
    /// Shifts all colours by the offset
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="newColorKeys"></param>
    private void ShiftColourKeysByHueOffset(float offset, GradientColorKey[] newColorKeys)
    {
        for (var i = 0; i < newColorKeys.Length; i++)
        {
            var rgb = newColorKeys[i].color;
            var hsv = ColorSpace.RGBtoHSV(rgb);
            hsv.h = (hsv.h + offset) % 1f;
            var c = ColorSpace.HSVtoRGB(hsv);
            newColorKeys[i].color = c;
            newColorKeys[i].color.a = rgb.a;

            // Set the background to the hue 
            // of the last colour so it 
            // fades out into the background
            if (i == newColorKeys.Length - 1)
            {
                _SpriteRenderer.color = c;
            }
        }
    }


    /// <summary>
    /// Loop forever on complete
    /// </summary>
    public void OnValueComplete()
    {
        StartGradientTransition();
    }

    

    // Update is called once per frame
    void Update () {
        //var i = (int)(Time.timeSinceLevelLoad % _Gradients.Count);
        //_ParticlesArea.m_updateGradient = true;
        //_ParticlesArea.m_colourGradient = _Gradients[i];

       

        //var speed = 6f;
        
        //var t = Mathf.Sin(Time.timeSinceLevelLoad * Mathf.PI) / speed + 0.5f;
        //var index1 = ((int)(Time.timeSinceLevelLoad * Mathf.PI)) % 3;
        //var index2 = (index1 + 1) % 3;
        
      
    }
}
