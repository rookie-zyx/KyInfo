using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Contracts.Auth;
using KyInfo.Contracts.Discussions;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;
using KyInfo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Tests;

public sealed class DiscussionsIntegrationTests : IClassFixture<KyInfoApiFactory>
{
    private readonly KyInfoApiFactory _factory;

    public DiscussionsIntegrationTests(KyInfoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_discussion_and_comment_flow_succeeds()
    {
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Users.Add(new User
            {
                UserName = "forum-user",
                Email = "forum-user@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = "forum-user",
            Password = "Test123!"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var stickers = await client.GetFromJsonAsync<List<StickerDto>>("/api/discussions/stickers");
        Assert.NotNull(stickers);
        Assert.NotEmpty(stickers!);

        var create = await client.PostAsJsonAsync("/api/discussions", new DiscussionCreateDto
        {
            Title = "初试经验分享",
            Content = "加油复习 [s:grin_open]"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var discussionId = await create.Content.ReadFromJsonAsync<int>();
        Assert.True(discussionId > 0);

        var comment = await client.PostAsJsonAsync($"/api/discussions/{discussionId}/comments", new DiscussionCommentCreateDto
        {
            Content = "很有帮助 [s:smile]"
        });
        Assert.Equal(HttpStatusCode.OK, comment.StatusCode);

        var list = await client.GetFromJsonAsync<DiscussionListResponseDto>("/api/discussions?page=1&pageSize=10");
        Assert.NotNull(list);
        Assert.Contains(list!.Items, x => x.Id == discussionId && x.CommentCount == 1);

        var detail = await client.GetFromJsonAsync<DiscussionDetailDto>($"/api/discussions/{discussionId}");
        Assert.NotNull(detail);
        Assert.Single(detail!.Comments);
        Assert.Contains("[s:grin_open]", detail.Content);
    }

    [Fact]
    public async Task Discussions_list_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/discussions");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_delete_another_users_comment()
    {
        var client = _factory.CreateClient();
        int discussionId;
        int commentId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var author = new User
            {
                UserName = "post-author",
                Email = "author@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };
            var admin = new User
            {
                UserName = "forum-admin",
                Email = "forum-admin@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.AddRange(author, admin);
            await db.SaveChangesAsync();

            var discussion = new Discussion
            {
                Title = "待审核帖",
                Content = "内容",
                AuthorUserId = author.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Discussions.Add(discussion);
            await db.SaveChangesAsync();

            var comment = new DiscussionComment
            {
                DiscussionId = discussion.Id,
                AuthorUserId = author.Id,
                Content = "评论",
                CreatedAt = DateTime.UtcNow
            };
            db.DiscussionComments.Add(comment);
            await db.SaveChangesAsync();

            discussionId = discussion.Id;
            commentId = comment.Id;
        }

        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = "forum-admin",
            Password = "Test123!"
        });
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var delete = await client.DeleteAsync($"/api/discussions/comments/{commentId}");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        var detail = await client.GetFromJsonAsync<DiscussionDetailDto>($"/api/discussions/{discussionId}");
        Assert.NotNull(detail);
        Assert.Empty(detail!.Comments);
    }

    [Fact]
    public async Task Discussions_list_can_sort_by_comment_count()
    {
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var author = new User
            {
                UserName = "sort-comments-user",
                Email = "sort-comments@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(author);
            await db.SaveChangesAsync();

            var low = new Discussion
            {
                Title = "sort-comments-low",
                Content = "low",
                AuthorUserId = author.Id,
                CreatedAt = new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc)
            };
            var high = new Discussion
            {
                Title = "sort-comments-high",
                Content = "high",
                AuthorUserId = author.Id,
                CreatedAt = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc)
            };
            db.Discussions.AddRange(low, high);
            await db.SaveChangesAsync();

            db.DiscussionComments.Add(new DiscussionComment
            {
                DiscussionId = low.Id,
                AuthorUserId = author.Id,
                Content = "c1",
                CreatedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc)
            });
            db.DiscussionComments.AddRange(
                new DiscussionComment
                {
                    DiscussionId = high.Id,
                    AuthorUserId = author.Id,
                    Content = "c2",
                    CreatedAt = new DateTime(2026, 5, 1, 11, 0, 0, DateTimeKind.Utc)
                },
                new DiscussionComment
                {
                    DiscussionId = high.Id,
                    AuthorUserId = author.Id,
                    Content = "c3",
                    CreatedAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc)
                });
            await db.SaveChangesAsync();
        }

        var login = await EnsureAuthenticatedAsync(client, "sort-comments-user", "sort-comments@local.test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        var list = await client.GetFromJsonAsync<DiscussionListResponseDto>("/api/discussions?page=1&pageSize=10&keyword=sort-comments&sort=mostCommented");
        Assert.NotNull(list);
        Assert.True(list!.Items.Count >= 2);
        Assert.Equal("sort-comments-high", list.Items[0].Title);
    }

    [Fact]
    public async Task Discussions_list_can_sort_by_recent_activity()
    {
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var author = new User
            {
                UserName = "sort-active-user",
                Email = "sort-active@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(author);
            await db.SaveChangesAsync();

            var newer = new Discussion
            {
                Title = "sort-active-newer",
                Content = "newer",
                AuthorUserId = author.Id,
                CreatedAt = new DateTime(2026, 5, 2, 12, 0, 0, DateTimeKind.Utc)
            };
            var revived = new Discussion
            {
                Title = "sort-active-revived",
                Content = "revived",
                AuthorUserId = author.Id,
                CreatedAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc)
            };
            db.Discussions.AddRange(newer, revived);
            await db.SaveChangesAsync();

            db.DiscussionComments.Add(new DiscussionComment
            {
                DiscussionId = revived.Id,
                AuthorUserId = author.Id,
                Content = "late-comment",
                CreatedAt = new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc)
            });
            await db.SaveChangesAsync();
        }

        var login = await EnsureAuthenticatedAsync(client, "sort-active-user", "sort-active@local.test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        var list = await client.GetFromJsonAsync<DiscussionListResponseDto>("/api/discussions?page=1&pageSize=10&keyword=sort-active&sort=recentlyActive");
        Assert.NotNull(list);
        Assert.True(list!.Items.Count >= 2);
        Assert.Equal("sort-active-revived", list.Items[0].Title);
    }

    [Fact]
    public async Task Discussions_list_can_filter_by_author()
    {
        var client = _factory.CreateClient();
        int targetAuthorId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var targetAuthor = new User
            {
                UserName = "filter-author-user",
                Email = "filter-author@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };
            var otherAuthor = new User
            {
                UserName = "filter-author-other",
                Email = "filter-author-other@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.AddRange(targetAuthor, otherAuthor);
            await db.SaveChangesAsync();
            targetAuthorId = targetAuthor.Id;

            db.Discussions.AddRange(
                new Discussion
                {
                    Title = "filter-author-mine",
                    Content = "mine",
                    AuthorUserId = targetAuthor.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Discussion
                {
                    Title = "filter-author-other-post",
                    Content = "other",
                    AuthorUserId = otherAuthor.Id,
                    CreatedAt = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var login = await EnsureAuthenticatedAsync(client, "filter-author-user", "filter-author@local.test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        var list = await client.GetFromJsonAsync<DiscussionListResponseDto>($"/api/discussions?page=1&pageSize=10&keyword=filter-author&authorUserId={targetAuthorId}");
        Assert.NotNull(list);
        Assert.Single(list!.Items);
        Assert.Equal("filter-author-mine", list.Items[0].Title);
    }

    private async Task<AuthResponse> EnsureAuthenticatedAsync(HttpClient client, string userName, string email)
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Users.Any(x => x.UserName == userName))
            {
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                db.Users.Add(new User
                {
                    UserName = userName,
                    Email = email,
                    PasswordHash = hasher.HashPassword("Test123!"),
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        }

        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = userName,
            Password = "Test123!"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
    }
}
