using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

public interface IFieldPoint
{
    void Initialize();
    void Activate();
    void Deactivate();
    void ChainUpdate();
}

public class FieldPoint : MonoBehaviour, IFieldPoint
{
    [SerializeField]
    private Transform pointTm = null;
    
    // 임시
    [SerializeField]
    private Monster monster = null;

    private bool _isActivate = false;
    
    #region FieldPoint

    public void Initialize()
    {
        monster?.Initialize();
    }
    
    public void Activate()
    {
        _isActivate = true;
        
        RandomActionAsync().Forget();
    }

    public void Deactivate()
    {
        _isActivate = false;
    }
    
    public void ChainUpdate()
    {
        if (!_isActivate)
            return;
        
        monster?.ChainUpdate();
        
        return;
    }
    #endregion

    private async UniTask RandomActionAsync()
    {
        await UniTask.WaitForSeconds(UnityEngine.Random.Range(5f, 10f));
        if (!_isActivate)
            return;
        
        if (!pointTm)
            return;
        
        if (monster == null)
            return;
        
        float randomX = UnityEngine.Random.Range(-2f, 2f);
        float randomY = UnityEngine.Random.Range(-2f, 2f);
        
        var targetPos = new Vector3(pointTm.position.x + randomX, pointTm.position.y + randomY, 0);
        monster.IActCtr?.MoveToTarget("02_Run", targetPos,
            () =>
            {
                RandomActionAsync().Forget();
            });
    }
}
