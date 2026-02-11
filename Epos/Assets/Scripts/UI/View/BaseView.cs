using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;

using GameSystem;

namespace UI.View
{
    public interface IView
    {
        void Configure(IObjectResolver iResolver);
    }

    public abstract class BaseView<T> : Common.Component<T> where T : Common.Param
    {
        [Inject] protected UIManager _uiManager = null;

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public abstract void Configure(IObjectResolver iResolver);

        /// <summary>씬/필드 스코프에 등록된 Presenter를 resolve. (임시 스코프 생성 없이 호출자 resolver와 동일 수명 유지)</summary>
        protected V RegisterPresenter<V>(IObjectResolver iResolver)
        {
            return iResolver != null ? iResolver.Resolve<V>() : default;
        }
    }
}


