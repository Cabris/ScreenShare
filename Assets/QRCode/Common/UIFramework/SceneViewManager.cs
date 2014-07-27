/*============================================================================== 
 * Copyright (c) 2012-2014 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

/// <summary>
/// All Initializations, Draw Calls and Update Calls go through here.
/// </summary>
using UnityEngine;
using System.Collections;

public class SceneViewManager : MonoBehaviour {
    
    public AppManager mAppManager;
    
    void Start () 
    {
        mAppManager.InitManager();
    }
    
    void Update()
    {
        InputController.UpdateInput();  
        mAppManager.UpdateManager();
    }
    
    void OnGUI () 
    {
        mAppManager.Draw();
    }
}
