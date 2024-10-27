using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
    public bool InputLock { get; set; } = true;


    public virtual void OnEnter()
    { 
        //do nothing
    }

    public virtual void OnExit()
    {
        //do nothing
    }


    public abstract void Tick();
}
