using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Fade : MonoBehaviour
{
    private const float Duration = 3f;
    
    [SerializeField] 
    private UnityEngine.UI.Image progressImg = null;
    
    public void Out(System.Action completeAction)
    {
        progressImg?.DOFade(0, 0);
        DoFade(1f, completeAction);
    }
    
    public void In(System.Action completeAction)
    {
        progressImg?.DOFade(1f, 0);
        DoFade(0, completeAction);
    }

    private void DoFade(float endValue, System.Action completeAction)
    {
        var sequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(progressImg?.DOFade(0, Duration))
            .OnComplete(() =>
            {
                completeAction?.Invoke();
            });
        sequence.Restart();
    }
}
