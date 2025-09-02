using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI
{
    public interface IPresenter<in T> where T : UI.IView
    {
        UniTask InitializeAsync(T view);
    }
}


