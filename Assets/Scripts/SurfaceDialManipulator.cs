using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RadialControllerHelper;
using RadialControllerHelper.Events;

public class SurfaceDialManipulator : MonoBehaviour {
    
    // Fluid Simulator
    public FluidSimulator m_simulator;
    
    // Local transform for performance
    private Transform m_transform;

    // Surface Dial / Radial Controller bits
    private RadialControllerUnityBridge m_controller;
    private bool m_contact = false;
    private float m_rotationDeltaInDegrees = 0;
    private Vector3 m_latestRadialControllerPosition;

    void Awake()
    {
        m_controller = RadialControllerUnityBridge.Instance;
    }

    void Start () {
        // Store a local transform for performance
        m_transform = this.transform;

        // Hooked the events 
        m_controller.RotationChanged += _controller_RotationChanged;
        m_controller.ButtonClicked += _controller_ButtonClicked;
        m_controller.ControlAcquired += _controller_ControlAcquired;
        //m_controller.ScreenContactStarted += _controller_ScreenContactStarted;
        //m_controller.ScreenContactEnded += _controller_ScreenContactEnded;
        //m_controller.ScreenContactContinued += _controller_ScreenContactContinued; 
    }

    #region Event Handlers
    private void _controller_ScreenContactContinued(object sender, RadialControllerScreenContactContinuedEventArgs args)
    {
        m_contact = true;
        UpdatePositionFromFluidCoordinates(args.Contact.Position);
    }

    private void _controller_ScreenContactEnded(object sender, object args)
    {
        m_contact = false;
    }

    private void _controller_ScreenContactStarted(object sender, RadialControllerScreenContactStartedEventArgs args)
    {
        m_contact = true;
        UpdatePositionFromFluidCoordinates(args.Contact.Position);
    }


    private void _controller_ControlAcquired(object sender, RadialControllerControlAcquiredEventArgs args)
    {

    }

    private void _controller_ButtonClicked(object sender, RadialControllerButtonClickedEventArgs args)
    {
        UpdatePositionFromFluidCoordinates(args.Contact.Position);
        m_transform.Rotate(Vector3.back, Random.Range(90f, 270f), Space.World);
    }

    private void _controller_RotationChanged(object sender, RadialControllerRotationChangedEventArgs args)
    {
        m_rotationDeltaInDegrees += args.RotationDeltaInDegrees;
    }

    #endregion 

    private void UpdatePositionFromFluidCoordinates(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(position.x, position.y, 0f));
        RaycastHit hitInfo = new RaycastHit();
        if (m_simulator.GetComponent<Collider>().Raycast(ray, out hitInfo, 100))
        {
            m_latestRadialControllerPosition = hitInfo.point;
        }
    }
    
    void Update ()
    {
        // Rotate rotation
        if(m_rotationDeltaInDegrees != 0)
        { 
            m_transform.Rotate(Vector3.back, m_rotationDeltaInDegrees, Space.World);
            m_rotationDeltaInDegrees = 0;
        }

        // Update the position if the controller has contact with the screen
        if (m_contact)
        {
            m_transform.position = m_latestRadialControllerPosition;
        }
	}


}
