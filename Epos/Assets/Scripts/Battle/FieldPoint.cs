using System;
using UnityEngine;
using VContainer;

namespace Battle
{
    public interface IFieldPoint
    {
        void Initialize(FieldPointParam param);
    }

    public class FieldPointParam : Common.Param
    {
        public IFieldPointListener IFieldPointListener { get; private set; }

        public FieldPointParam WithIFieldPointListener( IFieldPointListener iListener)
        {
            IFieldPointListener = iListener;
            return this;
        }
    }

    public interface IFieldPointListener
    {

    }

    public abstract class FieldPoint<T> : Common.Component<T>, IFieldPoint where T : FieldPointParam
    {
        [Inject] protected IObjectResolver _iResolver = null;

        protected IFieldPointListener _iFieldPointListener = null;

        void IFieldPoint.Initialize(FieldPointParam param)
        {
            base.InitializeAsync(param as T);
            //if (param is T t) 
            //    Initialize(t);
            //else throw new ArgumentException($"Expected {typeof(T).Name}, got {param?.GetType().Name}");

            _iFieldPointListener = param?.IFieldPointListener;

            Initialize(param as T);
        }

        // �Ļ� Ŭ������ �������̵��� ����
        //public override void Initialize(T param)
        //{
        //    base.Initialize(param);

        //    _iFieldPointListener = param?.IFieldPointListener;
        //}

        protected abstract new void Initialize(T param);

        //public override void Initialize(T param)
        //{
        //    base.Initialize(param);

        //    _iFieldPointListener = param?.IFieldPointListener;
        //}

        //public override void Initialize(FieldPointParam param)
        //{

        //}

        //public virtual void Initialize(T t)
        //{
        //    base.Initialize(t);
        //}
    }
}

