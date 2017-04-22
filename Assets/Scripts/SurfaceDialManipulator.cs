using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RadialControllerHelper;
using RadialControllerHelper.Events;

public class SurfaceDialManipulator : MonoBehaviour {
    
    private ParticlesAreaManipulator m_particles;
    private RadialControllerUnityBridge m_controller;
    private float m_rotationDeltaInDegrees = 0;
    private Transform m_transform;
    private bool m_clicked = false;
    
    public FluidSimulator m_simulator;

    public event RadialControllerButtonClickedEventHandler ButtonClicked;
    private RadialControllerButtonClickedEventArgs _buttonClickedArgs;

    void Awake()
    {
        m_controller = RadialControllerUnityBridge.Instance;
    }

    // Use this for initialization
    void Start () {
        m_particles = GetComponent<ParticlesAreaManipulator>();

        
        m_controller.RotationChanged += _controller_RotationChanged;
        m_controller.ButtonClicked += _controller_ButtonClicked;
        m_controller.ControlAcquired += _controller_ControlAcquired;
        
        m_transform = this.transform;
        
        this.ButtonClicked += SurfaceDialManipulator_ButtonClicked;
    }

    private void SurfaceDialManipulator_ButtonClicked(object sender, RadialControllerButtonClickedEventArgs args)
    {
        m_transform.Rotate(Vector3.back, Random.Range(90f, 270f), Space.World);
    }

    private void _controller_ControlAcquired(object sender, RadialControllerControlAcquiredEventArgs args)
    {
        Debug.LogError("Acquired!");
    }

    private void _controller_ButtonClicked(object sender, RadialControllerButtonClickedEventArgs args)
    {
        m_clicked = true;
        _buttonClickedArgs = args;
    }

    private void _controller_RotationChanged(object sender, RadialControllerRotationChangedEventArgs args)
    {
        m_rotationDeltaInDegrees += args.RotationDeltaInDegrees;
    }
    
    // Update is called once per frame
    void Update () {
        m_transform.Rotate(Vector3.back, m_rotationDeltaInDegrees, Space.World);
        m_rotationDeltaInDegrees = 0;

      
        if(m_clicked)
        {
            ButtonClicked(this, _buttonClickedArgs);
            
            m_clicked = false;
        }
	}


}
