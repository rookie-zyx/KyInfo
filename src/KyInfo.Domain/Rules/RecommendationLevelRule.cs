using KyInfo.Domain.Enums;

namespace KyInfo.Domain.Rules;

/// <summary>
/// 基于“用户总分 - 分数线”的分差，计算推荐档位。
/// 规则来源：当前 Controller 中的分档逻辑（冲刺/匹配/保底/超冲）。
/// </summary>
public static class RecommendationLevelRule
{
    public const string LevelOversprint = "超冲";
    public const string LevelSprint = "冲刺";
    public const string LevelMatch = "匹配";
    public const string LevelCoverBase = "保底";

    public static RecommendationLevel FromScoreDiff(int scoreDiff)
    {
        // diff >= 20  => 保底
        // diff >= -10 => 匹配
        // diff >= -30 => 冲刺
        // else        => 超冲
        return scoreDiff >= 20
            ? RecommendationLevel.CoverBase
            : scoreDiff >= -10
                ? RecommendationLevel.Match
                : scoreDiff >= -30
                    ? RecommendationLevel.Sprint
                    : RecommendationLevel.Oversprint;
    }

    public static string ToDisplayString(RecommendationLevel level)
    {
        return level switch
        {
            RecommendationLevel.Sprint => LevelSprint,
            RecommendationLevel.Match => LevelMatch,
            RecommendationLevel.CoverBase => LevelCoverBase,
            _ => LevelOversprint
        };
    }

    public static bool ShouldInclude(RecommendationLevel level) => level != RecommendationLevel.Oversprint;

    public static int SortOrder(RecommendationLevel level)
    {
        // 与当前 Controller 的排序保持一致：冲刺 -> 匹配 -> 保底 -> 超冲
        return level switch
        {
            RecommendationLevel.Sprint => 0,
            RecommendationLevel.Match => 1,
            RecommendationLevel.CoverBase => 2,
            _ => 3
        };
    }
}

