using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;

using Common;
using Creature;
using Parts;
using Battle.RealTime;
using VContainer.Unity;

namespace Entities
{
    public interface IFieldManager : IManager
    {
        void MoveToTarget(Vector3 pos);

        //void Activate();
        //void Deactivate();
        
        IField IField { get; }
        //Hero FieldHero { get; }
    }
    
    public class FieldManager : IFieldManager, ITickable
    {
        //[SerializeField] 
        //private Parts.Field field = null;
        //[SerializeField] 
        //private FieldIndicator fieldIndicator = null;

        [Inject] private IObjectResolver _iResolver = null;

        //private Hero _fieldHero = null;
        // private List<Parts.Field> _fieldList = new();
        public Parts.IField IField { get; private set; } = null;

        // 임시.
        //private List<Datas.Field> _fieldDataList = null;

        #region IGeneric
        async UniTask IGeneric.InitializeAsync()
        {
            //_container = container;
            IField = GameObject.FindFirstObjectByType<Field>();
            _iResolver?.Inject(IField);
            
            IField?.Initialize();
            
            //IField?.GetFieldPoint<IRealTimeFieldPoint>();
            await UniTask.CompletedTask;
        }

        //UniTask IGeneric.InitializeAsync(VContainer.IObjectResolver container)
        //{
        //field?.Initialize();
        //field?.Activate();
        //CurrIField = field;

        //CreateFieldHero();

        //_fieldDataList = new();
        //_fieldDataList?.Clear();

        //_fieldDataList?.Add(new Datas.Field(1));
        //_fieldDataList?.Add(new Datas.Field(2));

        //fieldIndicator?.Deactivate();

        //Activate();

        //return this;
        //}

        #region ITickable
        void ITickable.Tick()
        {
            IField?.ChainUpdate();
        }
        #endregion

        private void Update()
        {
            //if (!IsActivate)
            //    return;
            
            IField?.ChainUpdate();
            //_fieldHero?.ChainUpdate();
        }

        // void IGeneric.ChainUpdate()
        // {
        //     if (!IsActivate)
        //         return;
        //     
        //     CurrIField?.ChainUpdate();
        //     _fieldHero?.ChainUpdate();
        // }

        // void IGeneric.ChainLateUpdate()
        // {
        //     
        // }
        
        void FixedUpdate()
        {
            //_fieldHero?.ChainFixedUpdate();
        }
        #endregion

        //public override void Activate()
        //{
        //    base.Activate();
            
        //    //_fieldHero?.Activate();
        //}

        //public override void Deactivate()
        //{
        //    base.Deactivate();
            
        //    fieldIndicator?.Deactivate();
        //}

        //private void CreateFieldHero()
        //{
        //    _fieldHero = MainManager.Get<ICharacterManager>()?.Create<Hero>(10001, field.transform);
        //    if (_fieldHero == null)
        //        return;
            
        //    _fieldHero.Initialize();
        //    _fieldHero.transform.Initialize();
        //    _fieldHero.EnableNavmeshAgent();
        //    _fieldHero.Activate();
        //}
        
        #region IFieldManager
        void IFieldManager.MoveToTarget(Vector3 pos)
        {
            //if (_fieldHero == null)
            //    return;

            //if (!_fieldHero.IsActivate)
            //    return;
            
            //_fieldHero?.MoveToTarget(pos,
            //    () =>
            //    {
            //        fieldIndicator?.Deactivate();
            //    });
            
            //fieldIndicator?.Activate(
            //    new FieldIndicator.Param
            //    {
            //        TargetPos = pos,
            //    });
        }
        
        //Hero IFieldManager.FieldHero
        //{
        //    get
        //    {
        //        return _fieldHero;
        //    }
        //}

        // Data conatainer 로 변경 할 것.
        // Data.Field IFieldManager.GetFieldData(int fieldPointId)
        // {
        //     if (_fieldDataList == null)
        //         return null;
        //     
        //     foreach (var fieldData in _fieldDataList)
        //     {
        //         if (fieldData == null)
        //             continue;
        //
        //         if (fieldData.FieldPointId == fieldPointId)
        //             return fieldData;
        //     }
        //
        //     // if (_fieldDataList.TryGetValue(id, out Data.Field fieldData))
        //     //     return fieldData;
        //
        //     return null;
        // }
        #endregion
    }
}

