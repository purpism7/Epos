namespace Characters
{
    /// <summary>
    /// 액션이 요청하는 애니메이션 키. 실제 스파인 이름은 캐릭터마다 AnimationNameTable 이 고른다.
    /// 값은 이름 표의 칸 번호로도 쓰이므로 0부터 빈틈없이 이어 붙인다.
    /// 키를 더하면 AnimationNameTable.GetBaseName 에도 기본 이름을 더한다.
    /// </summary>
    public enum AnimationKey
    {
        Idle = 0,
        Run = 1,
        Skill01 = 2,
        Death = 3,
    }
}
