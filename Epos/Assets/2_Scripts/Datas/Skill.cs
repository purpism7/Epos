using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

namespace Datas
{
    // 테이블 데이터 (json) 로 변경 될 예정.
    public class Skill : Base
    {
        public int Id { get; private set; } = 0;
        public float Range { get; private set; } = 0;
        public float Attack { get; private set; } = 0;

        public Skill(int id, float range, float attack)
        {
            Id = id;
            Range = range;
            Attack = attack;
        }
    }
}

