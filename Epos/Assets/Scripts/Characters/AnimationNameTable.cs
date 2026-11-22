using System;

namespace Characters
{
    /// <summary>
    /// (애니메이션 키, 앞뒤) → 스파인 애니메이션 이름 표. 유니티/스파인에 기대지 않는 순수 클래스.
    /// 만들 때 한 번 다 풀어 두고, 이후 조회는 배열 읽기뿐이다(매 프레임 문자열 생성 없음).
    ///
    /// 키 하나·앞뒤 하나마다 아래 순서로 '정확히' 있는 첫 이름을 쓴다.
    ///   1. 그 방향 이름   : "Front/F_" + 기본 이름 / "Back/B_" + 기본 이름   (SD 그림)
    ///   2. 기본 이름      : "Idle", "Run", "Skill_01", "Death"            (옛 Knight_01, 방향 없는 SD "Death")
    ///   3. 반대 방향 이름 : 한쪽 방향만 그린 그림                               → 대체(IsFallback)
    ///   4. 아무것도 없으면 기본 이름을 그대로 넘긴다                             → 대체(IsFallback)
    ///      공용 Extensions.PlayAnimation 의 기존 규칙(정확 일치 → 첫 부분 일치)이 그대로 적용된다.
    /// </summary>
    public sealed class AnimationNameTable
    {
        public static readonly int KeyCount = Enum.GetValues(typeof(AnimationKey)).Length;

        private const string FrontPrefix = "Front/F_";
        private const string BackPrefix = "Back/B_";
        private const int FacingCount = 2;

        private readonly string[] _names = null;
        private readonly bool[] _isFallback = null;

        /// <param name="hasAnimation">그 이름의 애니메이션이 정확히 있는지. null 이면 아무것도 없다고 본다.</param>
        public AnimationNameTable(Func<string, bool> hasAnimation)
        {
            hasAnimation ??= _ => false;

            _names = new string[KeyCount * FacingCount];
            _isFallback = new bool[KeyCount * FacingCount];

            for (int keyIndex = 0; keyIndex < KeyCount; keyIndex++)
            {
                var key = (AnimationKey)keyIndex;

                Resolve(key, VerticalFacing.Front, hasAnimation);
                Resolve(key, VerticalFacing.Back, hasAnimation);
            }
        }

        /// <summary>재생할 이름. 잘못된 값이면 빈 문자열(재생 실패로 이어진다).</summary>
        public string GetName(AnimationKey key, VerticalFacing vertical)
        {
            int index = ToIndex(key, vertical);
            return index < 0 ? string.Empty : _names[index];
        }

        /// <summary>정확한 이름(그 방향 이름·기본 이름)이 없어 대신한 이름(3·4번)이면 true.</summary>
        public bool IsFallback(AnimationKey key, VerticalFacing vertical)
        {
            int index = ToIndex(key, vertical);
            return index >= 0 && _isFallback[index];
        }

        private void Resolve(AnimationKey key, VerticalFacing vertical, Func<string, bool> hasAnimation)
        {
            int index = ToIndex(key, vertical);
            if (index < 0)
                return;

            string baseName = GetBaseName(key);
            if (string.IsNullOrEmpty(baseName))
            {
                // 키를 더하고 기본 이름을 빠뜨린 경우. 재생은 실패하고 경고로 드러난다.
                _names[index] = string.Empty;
                _isFallback[index] = true;
                return;
            }

            string ownName = ComposeDirectionalName(baseName, vertical);
            if (hasAnimation(ownName))
            {
                _names[index] = ownName;
                return;
            }

            if (hasAnimation(baseName))
            {
                _names[index] = baseName;
                return;
            }

            string oppositeName = ComposeDirectionalName(baseName, Opposite(vertical));
            _names[index] = hasAnimation(oppositeName) ? oppositeName : baseName;
            _isFallback[index] = true;
        }

        private static string GetBaseName(AnimationKey key) => key switch
        {
            AnimationKey.Idle => "Idle",
            AnimationKey.Run => "Run",
            AnimationKey.Skill01 => "Skill_01",
            AnimationKey.Death => "Death",
            _ => string.Empty,
        };

        private static string ComposeDirectionalName(string baseName, VerticalFacing vertical)
        {
            return (vertical == VerticalFacing.Back ? BackPrefix : FrontPrefix) + baseName;
        }

        private static VerticalFacing Opposite(VerticalFacing vertical)
        {
            return vertical == VerticalFacing.Back ? VerticalFacing.Front : VerticalFacing.Back;
        }

        private static int ToIndex(AnimationKey key, VerticalFacing vertical)
        {
            if ((uint)key >= (uint)KeyCount || (uint)vertical >= FacingCount)
                return -1;

            return (int)key * FacingCount + (int)vertical;
        }
    }
}
