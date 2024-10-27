using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageCameraController : BaseController
{
    [SerializeField] float keyboardInputSensitivity = 10f;
    [SerializeField] float mouseInputSensitivity = 10f;
    [SerializeField] float xDir = 1f;   //1 or -1
    [SerializeField] float zDir = 1f;   //1 or -1
    [SerializeField] bool xzInverse = false;
    [SerializeField] bool mouseInputContinious = false;
    [SerializeField] bool mouseInputInverse = false;
    [SerializeField] Float2ValMin xRange;
    [SerializeField] Float2ValMin zRange;

    public float speed = 3f;
    public float rotateSpeed = 300f;
    public float threshold = 0.1f;
    public Transform follow;
    [HideInInspector] public Vector3 rotateTowards;

    Vector3 input;
    Vector3 pointOfOrigin;

    Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    public override void Tick()
    {
        AutoMove();
        //有跟随目标时不让自由移动摄像机
        if (follow)
            return;
        ResetInput();
        MoveCameraInput();
        MoveCamera();
    }

    private void AutoMove()
    {
        if (follow)
        {
            Vector2 camXZ = new Vector2(_transform.position.x, _transform.position.z);
            Vector2 followXZ = new Vector2(_transform.position.x, _transform.position.z);
            Vector2 xz = Vector2.Lerp(camXZ, followXZ, speed * Time.deltaTime);
            _transform.position = new Vector3(xz.x, _transform.position.y, xz.y);
            if (Vector2.Distance(camXZ, followXZ) < threshold)
            {
                follow = null;
            }
        }

        if (rotateTowards != transform.rotation.eulerAngles)
        {
            float step = rotateSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotateTowards), step);
        }
    }

    private void ResetInput()
    {
        input = Vector3.zero;
    }

    private void MoveCamera()
    {
        Vector3 pos = _transform.position;
        pos += input * Time.deltaTime;
        
        pos.x = Mathf.Clamp(pos.x, xRange.min, xRange.max);
        pos.z = Mathf.Clamp(pos.z, zRange.min, zRange.max);
      
        _transform.position = pos;
    }

    private void MoveCameraInput()
    {
        AxisInput();
        MouseInput();
    }

    private void AxisInput()
    {
        if (xzInverse)
        {
            input.z += Input.GetAxisRaw("Horizontal") * keyboardInputSensitivity * xDir;
            input.x += Input.GetAxisRaw("Vertical") * keyboardInputSensitivity * zDir;
        }
        else
        {
            input.x += Input.GetAxisRaw("Horizontal") * keyboardInputSensitivity * xDir;
            input.z += Input.GetAxisRaw("Vertical") * keyboardInputSensitivity * zDir;
        }
        
    }

    private void MouseInput()
    { 
        if (Input.GetMouseButtonDown(0))
            pointOfOrigin = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 mouseInput = Input.mousePosition;
            float inverseScale = mouseInputInverse ? -1f : 1f;
            if (xzInverse)
            {
                input.z += (mouseInput.x - pointOfOrigin.x) * mouseInputSensitivity * inverseScale * xDir;
                input.x += (mouseInput.y - pointOfOrigin.y) * mouseInputSensitivity * inverseScale * zDir;
            }
            else
            {
                input.x += (mouseInput.x - pointOfOrigin.x) * mouseInputSensitivity * inverseScale * xDir;
                input.z += (mouseInput.y - pointOfOrigin.y) * mouseInputSensitivity * inverseScale * zDir;
            }

            if (mouseInputContinious == false)
                pointOfOrigin = mouseInput;
        }

    }
}
