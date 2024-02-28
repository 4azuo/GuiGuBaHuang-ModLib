namespace ModLib.Enum
{
    /// <summary>
    /// 1-ChangeStats
    /// 2-LockedStats
    /// 3-SpecialFunction
    /// 
    /// 101-对自己使用效果
    /// 102-对鼠标位置使用武技
    /// 201-战斗外使用（增加属性）
    /// 301-突破用的丹药
    /// 302-逃跑用的丹药
    /// 401-兴趣道具
    /// 501-战斗战败喂丹
    /// </summary>
    public enum EffectTypeEnum
    {
        ChangeStats = 1,
        LockedStats = 2,
        SpecialFunction = 3,

        UseItem = 101,
        CastItem = 102,
        NormalItem = 201,
        BreakthroughItem = 301,
        EscapeItem = 302,
        HobbyItem = 401,
        BattleDebuffItem = 501,
    }
}
