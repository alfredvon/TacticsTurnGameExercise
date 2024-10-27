using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class RayAbility : Ability
{
    public string rayPrefabName;
    public string hitPrefabName;

    public Vector3 launchPositionModify;

    private AsyncOperationHandle<GameObject> handleRay;
    private AsyncOperationHandle<GameObject> handleHit;

    private GameObject prefabRay;
    private GameObject prefabHit;

    private GameObject hitRay;
    private Character target;
    private float waitHitTime = 1f;

    public override void OnDestroy()
    {
        base.OnDestroy();
        prefabRay = null;
        prefabHit = null;
        //if (prefabRay != null)
        //    GameObject.DestroyImmediate(prefabRay);
        //if (prefabHit != null)
        //    GameObject.DestroyImmediate(prefabHit);
        if (handleRay.IsValid())
            handleRay.Release();
        if (handleHit.IsValid())
            handleHit.Release();
    }

    protected override void OnApply(Tile target_tile)
    {
        LoadPrefab();

        owner.StartCoroutine(ApplyRayAbiltyCoroutine(target_tile));
    }

    private void LoadPrefab()
    {
        if (prefabRay == null)
        {
            handleRay = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Ability/" + rayPrefabName + ".prefab");
            handleRay.Completed += HandleRay_Completed;
        }
        if (prefabHit == null)
        {
            handleHit = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Effect/Hit/" + hitPrefabName + ".prefab");
            handleHit.Completed += HandleHit_Completed;
        }
    }

    private void HandleRay_Completed(AsyncOperationHandle<GameObject> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            prefabRay = operation.Result;
        }
        else
        {
            Debug.LogError($"Asset for {rayPrefabName} failed to load.");
        }
    }

    private void HandleHit_Completed(AsyncOperationHandle<GameObject> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            prefabHit = operation.Result;
        }
        else
        {
            Debug.LogError($"Asset for {hitPrefabName} failed to load.");
        }
    }

    IEnumerator ApplyRayAbiltyCoroutine(Tile target_tile)
    {
        target = null;
        //face direction
        owner.ParentUnit.RotateUnit(target_tile.WorldPosition);
        //play anim
        owner.ParentUnit.PlayCharacterAnimation(abilityAnimation, true);
        //wait apply effect time
        yield return new WaitForSeconds(startEffectInterval);
        if (effectRangeType == AbilityEffectRangeType.Single)
        {
            Vector3 rayStartPos = owner.ParentUnit.rayStartPoint.position + launchPositionModify;
            hitRay = GameObject.Instantiate(prefabRay, rayStartPos, owner.ParentUnit.rayStartPoint.rotation, GameManager.Instance.FXTransform);
            ParticleSystem particleSystem = hitRay.GetComponent<ParticleSystem>();
            //ray check 
            Ray ray = new Ray(rayStartPos, owner.ParentUnit.rayStartPoint.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                // 如果射线击中了某个物体，输出该物体的名字
                Debug.Log("Hit object: " + hit.collider.gameObject.name);

                waitHitTime = GetRayDuringTime(rayStartPos, hit.collider.gameObject.transform.position);
                var mainModule = particleSystem.main;
                mainModule.startLifetime = waitHitTime;
                //击中棋子
                if (hit.collider.gameObject.CompareTag("Pawn"))
                {
                    target = hit.collider.gameObject.GetComponentInParent<Character>();
                }
            }

            particleSystem.Play();
        }
        yield return new WaitForSeconds(waitHitTime);

        //GameObject.Destroy(hitRay);

        if (target != null && effectType == AbilityEffectType.Damage && IsTarget(target))
        {
            //roll 命中
            int hitRate = GameManager.Instance.DiceManager.RollDice(DiceManager.DiceType.dice20, 1);
            Debug.Log("roll 命中：" + hitRate + " 对方AC:" + target.AC);
            if (hitRate >= target.AC)
            {
                //roll damage
                //target.ParentUnit.TakeDamage(owner.CalculateAbilityDamage(), prefabHit);
                target.ParentUnit.TakeDamage(owner.CalculateAbilityDamage());
            }
            else
            {
                GameManager.Instance.UIManager.CreateFloatingMessage(target_tile.WorldPosition, "Miss");
            }
        }

        //stop anim
        owner.ParentUnit.PlayCharacterAnimation(abilityAnimation, false);
        //perform done
        if (useCouts < 1)
            owner.OnAbilityPerformDone();
    }

    private float GetRayDuringTime(Vector3 start, Vector3 end)
    {
        float ret = 1f;
        float dis = Vector3.Distance(start, new Vector3(end.x, start.y, end.z));
        ret = dis / 60f;    //射线默认速度
        return ret;
    }
}
