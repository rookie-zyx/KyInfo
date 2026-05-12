using ClosedXML.Excel;
using KyInfo.Application.Abstractions.Audit;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Application.Services.ExamScores;
using KyInfo.Contracts.Admin;
using KyInfo.Contracts.ExamScores;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;

namespace KyInfo.Application.Services.Admin;

public class AdminAppService : IAdminAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IExamScoreAppService _examScoreAppService;
    private readonly IAuditLogRepository _auditLogRepository;

    public AdminAppService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IExamScoreAppService examScoreAppService,
        IAuditLogRepository auditLogRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _examScoreAppService = examScoreAppService;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IReadOnlyList<AdminUserListItemDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users
            .Where(u => u.Role == UserRole.User)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToList();
    }

    public async Task CreateUserAsync(AdminCreateUserRequest request, int actorUserId, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("用户名/邮箱/密码不能为空。");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new ArgumentException("角色不合法，请使用 User。");
        }

        if (role == UserRole.Root)
        {
            throw new ArgumentException("不能通过接口创建 Root 账号。");
        }

        // 管理后台“普通用户管理”：仅允许创建普通用户账号。
        if (role != UserRole.User)
        {
            throw new ArgumentException("只能创建普通用户账号。");
        }

        var exists = await _userRepository.ExistsByUsernameOrEmailAsync(request.UserName, request.Email, cancellationToken);
        if (exists)
        {
            throw new ArgumentException("用户名或邮箱已存在。");
        }

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = role
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var actorRole = await GetActorRoleStringAsync(actorUserId, cancellationToken);
        await _auditLogRepository.AddAsync(
            actorUserId,
            actorRole,
            "User.Create",
            nameof(User),
            user.Id,
            $"UserName={user.UserName}; Email={MaskEmail(user.Email)}",
            cancellationToken);
    }

    public async Task DeleteUserAsync(int id, int actorUserId, CancellationToken cancellationToken)
    {
        if (id == actorUserId)
        {
            throw new ArgumentException("不能删除当前登录的管理员账号。");
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("用户不存在。");
        }

        if (user.Role == UserRole.Root)
        {
            throw new ArgumentException("不能删除 Root 账号。");
        }

        // 仅允许删除普通用户账号。
        if (user.Role != UserRole.User)
        {
            throw new ArgumentException("只能删除普通用户账号。");
        }

        await _userRepository.DeleteAsync(id, cancellationToken);

        var actorRole = await GetActorRoleStringAsync(actorUserId, cancellationToken);
        await _auditLogRepository.AddAsync(
            actorUserId,
            actorRole,
            "User.Delete",
            nameof(User),
            id,
            $"Deleted User id={id}",
            cancellationToken);
    }

    public async Task UpdateUserAsync(int id, int actorUserId, AdminUpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("用户名/邮箱不能为空。");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new ArgumentException("角色不合法，请使用 User。");
        }

        // 普通用户管理：目标账号必须是普通用户，且角色固定为 User。
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("用户不存在。");
        }

        if (user.Role == UserRole.Root)
        {
            throw new ArgumentException("不能修改 Root 账号。");
        }

        if (user.Role != UserRole.User || role != UserRole.User)
        {
            throw new ArgumentException("只能修改普通用户账号。");
        }

        var newUserName = request.UserName.Trim();
        var newEmail = request.Email.Trim();

        if (await _userRepository.IsUserNameTakenAsync(id, newUserName, cancellationToken))
        {
            throw new ArgumentException("该用户名已被占用");
        }

        if (await _userRepository.IsEmailTakenAsync(id, newEmail, cancellationToken))
        {
            throw new ArgumentException("该邮箱已被注册");
        }

        user.UserName = newUserName;
        user.Email = newEmail;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        var actorRoleUpdate = await GetActorRoleStringAsync(actorUserId, cancellationToken);
        await _auditLogRepository.AddAsync(
            actorUserId,
            actorRoleUpdate,
            "User.Update",
            nameof(User),
            id,
            $"UserName={user.UserName}; Email={MaskEmail(user.Email)}; PasswordChanged={!string.IsNullOrWhiteSpace(request.Password)}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<AdminUserListItemDto>> GetAdministratorUsersAsync(
        int actorUserId,
        CancellationToken cancellationToken)
    {
        await AssertActorIsRootAsync(actorUserId, cancellationToken);

        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToList();
    }

    public async Task CreateAdministratorAsync(
        int actorUserId,
        AdminCreateAdministratorRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        await AssertActorIsRootAsync(actorUserId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("用户名/邮箱/密码不能为空。");
        }

        var exists = await _userRepository.ExistsByUsernameOrEmailAsync(request.UserName, request.Email, cancellationToken);
        if (exists)
        {
            throw new ArgumentException("用户名或邮箱已存在。");
        }

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.Admin
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var actorRoleAdm = await GetActorRoleStringAsync(actorUserId, cancellationToken);
        await _auditLogRepository.AddAsync(
            actorUserId,
            actorRoleAdm,
            "Admin.Create",
            nameof(User),
            user.Id,
            $"UserName={user.UserName}; Email={MaskEmail(user.Email)}",
            cancellationToken);
    }

    public async Task RemoveAdministratorAsync(
        int actorUserId,
        int targetAdministratorId,
        CancellationToken cancellationToken)
    {
        await AssertActorIsRootAsync(actorUserId, cancellationToken);

        if (targetAdministratorId == actorUserId)
        {
            throw new ArgumentException("不能对当前 Root 账号执行撤权。");
        }

        var user = await _userRepository.GetByIdAsync(targetAdministratorId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("用户不存在。");
        }

        if (user.Role == UserRole.Root)
        {
            throw new ArgumentException("不能撤权 Root 账号。");
        }

        if (user.Role != UserRole.Admin)
        {
            throw new ArgumentException("只能撤权管理员（Admin）账号。");
        }

        user.Role = UserRole.User;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var actorRoleRevoke = await GetActorRoleStringAsync(actorUserId, cancellationToken);
        await _auditLogRepository.AddAsync(
            actorUserId,
            actorRoleRevoke,
            "Admin.Revoke",
            nameof(User),
            targetAdministratorId,
            $"Revoked admin id={targetAdministratorId}",
            cancellationToken);
    }

    public async Task<AuditLogListResponseDto> GetAuditLogsAsync(
        int actorUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await AssertActorIsRootAsync(actorUserId, cancellationToken);
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 20;
        }

        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var skip = (page - 1) * pageSize;
        var (items, total) = await _auditLogRepository.GetPagedAsync(skip, pageSize, cancellationToken);
        return new AuditLogListResponseDto { Items = items, TotalCount = total };
    }

    private async Task AssertActorIsRootAsync(int actorUserId, CancellationToken cancellationToken)
    {
        var actor = await _userRepository.GetByIdAsync(actorUserId, cancellationToken);
        if (actor is null || actor.Role != UserRole.Root)
        {
            throw new ForbiddenException("仅 Root 用户可执行此操作。");
        }
    }

    private async Task<string> GetActorRoleStringAsync(int actorUserId, CancellationToken cancellationToken)
    {
        var actor = await _userRepository.GetByIdAsync(actorUserId, cancellationToken);
        return actor?.Role.ToString() ?? "?";
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0)
        {
            return "***";
        }

        var local = email[..at];
        var domain = email[(at + 1)..];
        var shown = local.Length <= 1 ? "*" : $"{local[0]}***";
        return $"{shown}@{domain}";
    }

    public byte[] GetExamScoreImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("成绩导入");
        var headers = new[]
        {
            "用户Id", "年份", "总分", "政治", "英语", "数学", "专业课", "院校Id", "专业Id"
        };
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        ws.Row(1).Style.Font.Bold = true;
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents(1, 1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ExcelImportResultDto> ImportExamScoresFromExcelAsync(Stream excelStream, int actorUserId, CancellationToken cancellationToken)
    {
        if (excelStream is null || !excelStream.CanRead)
        {
            throw new ArgumentException("无效的 Excel 流。");
        }

        using var workbook = new XLWorkbook(excelStream);
        var ws = workbook.Worksheets.FirstOrDefault();
        if (ws is null)
        {
            throw new ArgumentException("Excel 中没有工作表。");
        }

        var headerMap = BuildHeaderMap(ws.Row(1));
        var colUserId = FindColumn(headerMap, "用户id", "userid", "用户Id");
        var colYear = FindColumn(headerMap, "年份", "year");
        var colTotal = FindColumn(headerMap, "总分", "totalscore");
        if (!colUserId.HasValue || !colYear.HasValue || !colTotal.HasValue)
        {
            throw new ArgumentException(
                "表头必须包含：用户Id（或 UserId）、年份（或 Year）、总分（或 TotalScore）。");
        }

        var colPol = FindColumn(headerMap, "政治", "politicsscore");
        var colEn = FindColumn(headerMap, "英语", "englishscore");
        var colMath = FindColumn(headerMap, "数学", "mathscore");
        var colMajor = FindColumn(headerMap, "专业课", "majorsubjectscore");
        var colSchool = FindColumn(headerMap, "院校id", "schoolid");
        var colMajorId = FindColumn(headerMap, "专业id", "majorid");

        var result = new ExcelImportResultDto();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (var r = 2; r <= lastRow; r++)
        {
            if (IsRowEmpty(ws, r, colUserId.Value, colYear.Value, colTotal.Value))
            {
                continue;
            }

            if (!TryGetInt(ws.Cell(r, colUserId.Value), out var userId))
            {
                result.Errors.Add(new ExcelImportErrorDto { RowNumber = r, Message = "用户Id 无效" });
                continue;
            }

            if (!TryGetInt(ws.Cell(r, colYear.Value), out var year) || year <= 0)
            {
                result.Errors.Add(new ExcelImportErrorDto { RowNumber = r, Message = "年份无效" });
                continue;
            }

            if (!TryGetInt(ws.Cell(r, colTotal.Value), out var total) || total <= 0)
            {
                result.Errors.Add(new ExcelImportErrorDto { RowNumber = r, Message = "总分无效" });
                continue;
            }

            int? politics = TryGetOptionalInt(ws, r, colPol);
            int? english = TryGetOptionalInt(ws, r, colEn);
            int? math = TryGetOptionalInt(ws, r, colMath);
            int? majorSub = TryGetOptionalInt(ws, r, colMajor);
            int? schoolId = TryGetOptionalInt(ws, r, colSchool);
            int? majorId = TryGetOptionalInt(ws, r, colMajorId);

            var dto = new ExamScoreCreateDto
            {
                UserId = userId,
                Year = year,
                TotalScore = total,
                PoliticsScore = politics,
                EnglishScore = english,
                MathScore = math,
                MajorSubjectScore = majorSub,
                SchoolId = schoolId,
                MajorId = majorId
            };

            try
            {
                await _examScoreAppService.CreateAsync(dto, cancellationToken);
                result.SuccessCount++;
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add(new ExcelImportErrorDto { RowNumber = r, Message = ex.Message });
            }
        }

        var actorRoleImport = await GetActorRoleStringAsync(actorUserId, cancellationToken);
        await _auditLogRepository.AddAsync(
            actorUserId,
            actorRoleImport,
            "ExamScore.ImportBatch",
            "ExamScore",
            null,
            $"Success={result.SuccessCount}; Errors={result.Errors.Count}",
            cancellationToken);

        return result;
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastCol = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (var c = 1; c <= lastCol; c++)
        {
            var text = headerRow.Cell(c).GetString().Trim();
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            var key = NormalizeHeader(text);
            if (!map.ContainsKey(key))
            {
                map[key] = c;
            }
        }

        return map;
    }

    private static string NormalizeHeader(string text) =>
        text.Trim().Replace(" ", "").ToLowerInvariant();

    private static int? FindColumn(Dictionary<string, int> map, params string[] aliases)
    {
        foreach (var a in aliases)
        {
            var key = NormalizeHeader(a);
            if (map.TryGetValue(key, out var col))
            {
                return col;
            }
        }

        return null;
    }

    private static bool IsRowEmpty(IXLWorksheet ws, int row, int colUser, int colYear, int colTotal)
    {
        return ws.Cell(row, colUser).IsEmpty()
               && ws.Cell(row, colYear).IsEmpty()
               && ws.Cell(row, colTotal).IsEmpty();
    }

    private static bool TryGetInt(IXLCell cell, out int value)
    {
        if (cell.IsEmpty())
        {
            value = 0;
            return false;
        }

        if (cell.DataType == XLDataType.Number)
        {
            value = (int)Math.Round(cell.GetDouble());
            return true;
        }

        var s = cell.GetString().Trim();
        return int.TryParse(s, out value);
    }

    private static int? TryGetOptionalInt(IXLWorksheet ws, int row, int? col)
    {
        if (!col.HasValue)
        {
            return null;
        }

        var cell = ws.Cell(row, col.Value);
        if (cell.IsEmpty())
        {
            return null;
        }

        return TryGetInt(cell, out var v) ? v : null;
    }
}
