using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Fade : MonoBehaviour
{
    private const float Duration = 2f;
    
    [SerializeField] 
    private UnityEngine.UI.Image progressImg = null;
    
    public void Out(System.Action completeAction)
    {
        progressImg?.DOFade(0, 0);
            
        var sequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(progressImg?.DOFade(1f, Duration))
            // .AppendInterval(Duration)
            .OnComplete(() =>
            {
                completeAction?.Invoke();
            });
        sequence.Restart();
    }
    
    public void In(System.Action completeAction)
    {
        progressImg?.DOFade(1f, 0);
            
        var sequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(progressImg?.DOFade(0, Duration))
            // .AppendInterval(Duration)
            .OnComplete(() =>
            {
                completeAction?.Invoke();
            });
        sequence.Restart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
