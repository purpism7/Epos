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
        void CreatePresenter(IObjectResolver iResolver);
    }

    public abstract class BaseView<T> : Common.Component<T> where T : Common.Param
    {
        [Inject] protected UIManager _uiManager = null;

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public abstract void CreatePresenter(IObjectResolver iResolver);

        protected V RegisterPresenter<V>(IObjectResolver iResolver)
        {
            using var scope = iResolver?.CreateScope(
                (builder) =>
                {
                    builder.Register<V>(VContainer.Lifetime.Scoped);
                });

            return scope.Resolve<V>();
        }
    }
}


