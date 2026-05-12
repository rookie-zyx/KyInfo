namespace KyInfo.Domain.Enums;

public enum RecommendationLevel
{
    /// <summary>超冲：分差过大，通常不建议。</summary>
    Oversprint = 0,

    /// <summary>冲刺：略低于分数线。</summary>
    Sprint = 1,

    /// <summary>匹配：接近分数线。</summary>
    Match = 2,

    /// <summary>保底：明显高于分数线。</summary>
    CoverBase = 3
}

