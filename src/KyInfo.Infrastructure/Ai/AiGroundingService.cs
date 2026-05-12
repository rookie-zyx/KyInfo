using System.Text;
using System.Text.RegularExpressions;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Ai;

public class AiGroundingService
{
    private static readonly Regex YearRegex = new(@"\b(20\d{2})\b", RegexOptions.Compiled);

    private readonly AppDbContext _db;

    public AiGroundingService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 尝试从数据库直接生成“带引用”的答案。
    /// 当前实现优先覆盖：计算机专硕 + 北京 + 211 + 好考/容易。
    /// </summary>
    public async Task<string?> TryAnswerFromDbAsync(string userMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return null;
        }

        userMessage = userMessage.Trim();

        var containsComputer = userMessage.Contains("计算机", StringComparison.Ordinal);
        var containsZhuanShuo = userMessage.Contains("专硕", StringComparison.Ordinal);
        var containsBeijing = userMessage.Contains("北京", StringComparison.Ordinal);
        var contains211 = userMessage.Contains("211", StringComparison.Ordinal);

        if (!(containsComputer && containsZhuanShuo && containsBeijing && contains211))
        {
            return null;
        }

        // “好考/容易”这里只影响排序策略（分数线越低越容易）
        var wantsEasy = userMessage.Contains("好考", StringComparison.Ordinal) ||
                         userMessage.Contains("容易", StringComparison.Ordinal) ||
                         userMessage.Contains("低分", StringComparison.Ordinal);

        int? requestedYear = null;
        var yearMatch = YearRegex.Match(userMessage);
        if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var y))
        {
            requestedYear = y;
        }

        // 目标：城市/省份=北京，LevelTag=211，学位类型=专硕，专业名称包含计算机（例如“计算机技术”/“计算机科学与技术”）
        // 分数线：ScoreLines.IsNational=false，且把“专业线”按指定年份（或最新年份）筛选
        var baseQuery =
            from sl in _db.ScoreLines.AsNoTracking()
            where !sl.IsNational
                  && sl.SchoolId.HasValue
                  && sl.MajorId.HasValue
            join s in _db.Schools.AsNoTracking() on sl.SchoolId!.Value equals s.Id
            join m in _db.Majors.AsNoTracking() on sl.MajorId!.Value equals m.Id
            where s.LevelTag == "211"
                  && (s.City == "北京" || s.Province == "北京")
                  && m.DegreeType == "专硕"
                  && m.Name.Contains("计算机")
            select new
            {
                sl.Year,
                sl.Score,
                sl.SchoolId,
                sl.MajorId,
                SchoolName = s.Name,
                MajorName = m.Name,
                MajorCode = m.Code
            };

        if (!await baseQuery.AnyAsync(cancellationToken))
        {
            return "数据库当前没有满足“计算机专硕 + 北京 + 211”的院校/专业分数线数据。你可以先新增该条件下的 `Schools/Majors/ScoreLines` 数据，或把“211”范围扩大到“985/普通”。";
        }

        var latestYear = await baseQuery
            .MaxAsync(x => x.Year, cancellationToken);

        var year = requestedYear ?? latestYear;

        var rows = await baseQuery
            .Where(x => x.Year == year)
            .OrderBy(x => wantsEasy ? x.Score : -x.Score)
            .Take(8)
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return $"数据库没有找到年份 {year} 的“计算机专硕 + 北京 + 211”专业线数据。你也可以把年份改成其它年份，或补充 `ScoreLines`。";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"我已从数据库 `ScoreLines` 检索最新年份/你指定的年份（{year}）中：");
        sb.AppendLine("筛选条件：北京地区（Schools.City/Province=北京） + 学校层次=211（Schools.LevelTag=211） + 学位类型=专硕（Majors.DegreeType=专硕） + 专业名称包含“计算机”（Majors.Name）。");
        sb.AppendLine();
        sb.AppendLine($"根据“好考”=分数线更低（IsNational=0 的专业线）对结果排序，给出从低到高的候选：");

        for (var i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var rank = i + 1;
            sb.AppendLine(
                $"{rank}) {r.SchoolName} - {r.MajorName}（{r.MajorCode}）：{r.Score} 分。来源：ScoreLines（Year={r.Year}, IsNational=0, SchoolId={r.SchoolId}, MajorId={r.MajorId}）。");
        }

        sb.AppendLine();
        sb.AppendLine("提示：分数线越低通常表示相对竞争压力更小，但不等同于最终录取结果（还受报考人数、调剂、初试科目等影响）。");
        return sb.ToString();
    }
}

