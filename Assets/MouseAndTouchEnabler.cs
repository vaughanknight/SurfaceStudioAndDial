using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use mouse when in the editor, touch when not in the editor.  This 
/// is because Unity doesn't support 10 point multitouch so we want the mouse, 
/// where as we do not want mouse support when in UWP due to surface dial 
/// and 10 point touch working.
/// </summary>
public class MouseAndTouchEnabler : MonoBehaviour
{
    public MouseManipulator _MouseManipulator;
    public TouchManipulator _TouchManipulator;
    
    void Start()
    {
#if !UNITY_EDITOR
        _MouseManipulator.enabled = false;
#else
        _TouchManipulator.enabled = false;
#endif
        // Job done, self destruct to clean up the object tree
        // as this object is not needed any longer
        //GameObject.Destroy(gameObject);
    }

}
