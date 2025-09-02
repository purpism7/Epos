using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

namespace UI
{
    public interface IView
    {
        void CreatePresenter(IObjectResolver iResolver);
    }

    public abstract class BaseView<T> : Common.Component<T> where T : Common.Param
    {
        public abstract void CreatePresenter(IObjectResolver iResolver);
    }
}


