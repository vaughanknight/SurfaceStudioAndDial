using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAndTouchEnabler : MonoBehaviour
{
    public MouseManipulator _MouseManipulator;
    public TouchManipulator _TouchManipulator;

    // Use this for initialization
    void Start()
    {
#if !UNITY_EDITOR
        _MouseManipulator.enabled = false;
#else
        _TouchManipulator.enabled = false;
#endif
        //if (Application.isEditor)
        //{
        //    Debug.Log("Disabling Mouse Manipulator.");
        //    _MouseManipulator.enabled = false;
        //}
        //else
        //{
        //    Debug.Log("Disabling Touch Manipulator.");
        //    _TouchManipulator.enabled = false;
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }
}
