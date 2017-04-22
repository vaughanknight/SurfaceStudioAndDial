﻿using RadialControllerHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MenuItem
{
    public string Title;
    public Texture2D Icon;
}

public class RadialControllerBehaviour : MonoBehaviour {

    public List<MenuItem> MenuItems = new List<MenuItem>() { new MenuItem() };
    public float RotationResolution = 10f;
    public bool UseAutomaticHapticFeedback = true;

    private RadialControllerUnityBridge _radialController;

    /// <summary>
    /// Invoke the Initialise in the awake method
    /// TODO: Move the Intialise into the bridge, hide it
    /// from Unity completely
    /// </summary>
    void Awake () {
        _radialController = RadialControllerUnityBridge.Instance;
        _radialController.Initialise();
        _radialController.RotationResolutionInDegrees = RotationResolution;
        _radialController.UseAutomaticHapticFeedback = UseAutomaticHapticFeedback;
        _radialController.ScreenContactStarted += _radialController_ScreenContactStarted;
        _radialController.ScreenContactEnded += _radialController_ScreenContactEnded;
        _radialController.ScreenContactContinued += _radialController_ScreenContactContinued;
    }

    private void _radialController_ScreenContactContinued(object sender, RadialControllerHelper.Events.RadialControllerScreenContactContinuedEventArgs args)
    {
        
    }

    private void _radialController_ScreenContactEnded(object sender, object args)
    {
        
    }

    private void _radialController_ScreenContactStarted(object sender, RadialControllerHelper.Events.RadialControllerScreenContactStartedEventArgs args)
    {
        
    }

    public void Start()
    {
        foreach(var m in MenuItems)
        {
            var data = m.Icon.EncodeToPNG();
            _radialController.AddMenuItem(m.Title, m.Icon);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}