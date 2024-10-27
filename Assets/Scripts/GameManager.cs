using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] StageManager stageManager;
    [SerializeField] UIManager uiManager;
    [SerializeField] DiceManager diceManager;

    public UIManager UIManager => uiManager;
    public DiceManager DiceManager => diceManager;
    public Transform FXTransform;

    private BaseController controller;

    private void Start()
    {
        stageManager.ChangeState(StageState.Start);
    }

    public void ChangeController(BaseController new_controller)
    {
        if (controller != null)
        {
            controller.InputLock = true;
            controller.OnExit();
        }
        controller = new_controller;
        controller.OnEnter();
        controller.InputLock = false;
    }

    public void SetInputLock(bool is_lock)
    {
        if (controller != null)
            controller.InputLock = is_lock;
    }
        
    private void Update()
    {
        controller.Tick();
    }





}
