using KyInfo.Contracts.Admin;
using KyInfo.Contracts.ExamScores;

namespace KyInfo.Application.Services.Admin;

public interface IAdminAppService
{
    Task<IReadOnlyList<AdminUserListItemDto>> GetUsersAsync(CancellationToken cancellationToken);

    Task CreateUserAsync(AdminCreateUserRequest request, int actorUserId, CancellationToken cancellationToken);

    Task DeleteUserAsync(int id, int actorUserId, CancellationToken cancellationToken);

    Task UpdateUserAsync(int id, int actorUserId, AdminUpdateUserRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminUserListItemDto>> GetAdministratorUsersAsync(int actorUserId, CancellationToken cancellationToken);

    Task CreateAdministratorAsync(int actorUserId, AdminCreateAdministratorRequest request, CancellationToken cancellationToken);

    Task RemoveAdministratorAsync(int actorUserId, int targetAdministratorId, CancellationToken cancellationToken);

    Task<AuditLogListResponseDto> GetAuditLogsAsync(int actorUserId, int page, int pageSize, CancellationToken cancellationToken);

    byte[] GetExamScoreImportTemplate();

    Task<ExcelImportResultDto> ImportExamScoresFromExcelAsync(Stream excelStream, int actorUserId, CancellationToken cancellationToken);
}
