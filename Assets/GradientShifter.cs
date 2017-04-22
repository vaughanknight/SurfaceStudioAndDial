using UnityEngine;

/// <summary>
/// Shifts the gradient through various hue's
/// </summary>
public class GradientShifter : MonoBehaviour {
    public ParticlesArea _ParticlesArea;
    public SpriteRenderer _SpriteRenderer;
    public float HueShiftTime = 30f;
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
        // Uses iTween for the hue shift timer for ease
        // Hue will shift by 0 to 1, over 30 seconds
        // NOTE: iTween is not the most efficient when you have
        // multiple iTweens at the same time, so if you plan on 
        // extending this, or having 10+ lerps simultaneously, 
        // I advise using something else.
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", HueShiftTime, "onupdate", "OnValueUpdate", "oncomplete", "OnValueComplete", "easeType", iTween.EaseType.easeInOutSine));
    }

    private int _gradientIndex = 0;

    public void OnValueUpdate(float offset)
    {
        // We can not update the current gradient, 
        // but in fact have to create a new one
        var newGradient = new Gradient();
        newGradient.alphaKeys = _startGradient.alphaKeys;
        newGradient.mode = GradientMode.Blend;

        // Let's get a copy as a starting point
        // for the colour keys
        var newColorKeys = _startGradient.colorKeys;

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
}
