using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMainController : BaseController
{
    [HideInInspector] public StageCameraController cameraController;
    [HideInInspector] public GridController gridController;
    public override void OnEnter()
    {
        base.OnEnter();
        //find camera controller
        if (cameraController == null)
            cameraController = StageManager.Instance.cameraRig.GetComponent<StageCameraController>();
        if (gridController == null)
            gridController = gameObject.GetComponent<GridController>();
    }

    public override void Tick()
    {
        cameraController.Tick();
        if (InputLock == true)
            return;
        gridController.Tick();
    }
}
