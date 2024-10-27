using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DiceManager : MonoBehaviour
{


    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        ThrowDice(DiceType.dice20, 1);
    //    }

    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        ThrowDice(DiceType.dice20, 2);
    //    }
    //}

    public int RollDice(DiceType dice_type, int dice_count)
    {
        int ret = 0;
        for (int i = 0; i < dice_count; i++)
        {
            ret += Random.Range(1, (int)dice_type);
            Debug.Log("roll dice type : " + dice_type + " count:" + i + " value:" + ret);
        }
        Debug.Log("roll dice type:" + dice_type + " count:" + dice_count + " total value:" + ret);
        return ret;
    }

    #region ThrowDice
    public enum DiceType
    {
        dice4 = 4, dice6 = 6, dice8 = 8, dice10 = 10, dice20 = 20
    }

    //[Header("骰子预制体")]
    //[SerializeField] GameObject dice4_Prefab;
    //[SerializeField] GameObject dice6_Prefab;
    //[SerializeField] GameObject dice8_Prefab;
    //[SerializeField] GameObject dice10_Prefab;
    //[SerializeField] GameObject dice12_Prefab;
    //[SerializeField] GameObject dice20_Prefab;
    //[Header("骰子初始位置")]
    //[SerializeField] List<Transform> initPosList;
    //[Header("跳跃力度")]
    //[SerializeField] float forceAmount = 300f;
    //[Header("动画预计持续时间")]
    //[SerializeField] float duration = 3f;

    ///// <summary>
    ///// 生成骰子并投掷，通过回调获取点数结果。
    ///// </summary>
    ///// <param name="diceType">骰子类型</param>
    ///// <param name="diceCount">骰子个数</param>
    ///// <param name="callBack">获取点数结果</param>
    //public void ThrowDice(DiceType diceType, int diceCount, UnityEvent<int> callBack = null)
    //{
    //    GameObject prefab = GetDicePrefab(diceType);
    //    List<DiceStats> diceList = new List<DiceStats>();
    //    for (int i = 0; i < diceCount; i++)
    //    {
    //        GameObject dice = Instantiate(prefab, initPosList[i].position, Quaternion.identity, transform);
    //        Rigidbody rb = dice.GetComponent<Rigidbody>();
    //        rb.AddForce(Vector3.up * forceAmount);
    //        rb.AddTorque(new Vector3(Random.value * forceAmount, Random.value * forceAmount, Random.value * forceAmount));
    //        diceList.Add(dice.GetComponent<DiceStats>());
    //    }
    //    StartCoroutine(GetDicePoint(diceList, callBack));
    //}

    //GameObject GetDicePrefab(DiceType diceType)
    //{
    //    switch (diceType)
    //    {
    //        case DiceType.dice4: return dice4_Prefab;
    //        case DiceType.dice6: return dice6_Prefab;
    //        case DiceType.dice8: return dice8_Prefab;
    //        case DiceType.dice10: return dice10_Prefab;
    //        case DiceType.dice20: return dice20_Prefab;
    //        default: return null;
    //    }
    //}

    //IEnumerator GetDicePoint(List<DiceStats> diceStats, UnityEvent<int> callBack)
    //{
    //    Debug.Log("骰子已经掷下" + duration + "秒后开始结算");
    //    yield return new WaitForSeconds(duration);

    //    //检查骰子是否都停下
    //    bool isMove = true;
    //    while (isMove)
    //    {
    //        for (int i = 0; i < diceStats.Count; i++)
    //        {
    //            if (diceStats[i].gameObject.GetComponent<Rigidbody>().velocity != Vector3.zero)
    //            {
    //                Debug.Log("骰子索引" + i + "没有停下");
    //                break;
    //            }
    //            if (i == diceStats.Count - 1)
    //            {
    //                isMove = false;
    //                Debug.Log("骰子都停下了");
    //            }
    //        }
    //        yield return null;
    //    }


    //    //计算总点数并输出结果
    //    int dicePoint = 0;
    //    for (int i = 0; i < diceStats.Count; i++)
    //    {
    //        dicePoint += diceStats[i].side;
    //        Destroy(diceStats[i].gameObject, 3f);
    //    }
    //    callBack?.Invoke(dicePoint);
    //    Debug.Log("投掷出了" + dicePoint);
    //}
    #endregion
}
