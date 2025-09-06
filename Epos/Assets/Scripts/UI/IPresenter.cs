using UnityEngine;

using Cysharp.Threading.Tasks;

namespace UI
{
    public interface IPresenter<in T> where T : UI.View.IView
    {
        UniTask InitializeAsync(T view);
    }
}


