using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingMessage : MonoBehaviour
{
    public string message;
    public float duration = 1f;

    public Color32 color = Color.white;
    public int fontSize = 18;
    public Vector3 originPos;

    [SerializeField] Text text;

    [Header("Animation Curve")]
    [SerializeField] AnimationCurve opacityCurve;
    [SerializeField] AnimationCurve scaleCurve;
    [SerializeField] AnimationCurve heightCurve;

    private float timer;
    

    private void Awake()
    {
        timer = 0;
        originPos = transform.position;
        text.fontSize = fontSize;
    }

    private void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0f)
        {
            Destroy(gameObject);
        }

        if (!text)
        {
            Debug.LogError("Missing component: Text");
            return;
        }

        if (!string.IsNullOrEmpty(message))
        {
            text.text = message;
        }
        
        //animation curve effect
        //Debug.Log("opacity:"+ opacityCurve.Evaluate(timer));
        text.color = new Color(color.r, color.g, color.b, opacityCurve.Evaluate(timer));
        transform.localScale = Vector3.one * scaleCurve.Evaluate(timer);
        transform.position = originPos + new Vector3(0, 1 + heightCurve.Evaluate(timer), 0);
        timer += Time.deltaTime;

        
    }


}
