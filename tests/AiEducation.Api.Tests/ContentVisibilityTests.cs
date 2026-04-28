using System.Net;
using System.Net.Http.Json;

namespace AiEducation.Api.Tests;

public sealed class ContentVisibilityTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Student_learning_endpoints_only_show_published_lessons()
    {
        var admin = factory.CreateClient();
        ApiFactory.Authorize(admin, await factory.RegisterAdminAndGetTokenAsync(admin, "admin-visibility@example.com"));

        var lessons = await admin.GetFromJsonAsync<AdminLessonPayload[]>("/api/v1/admin/lessons");
        var first = lessons!.Single(x => x.Slug == "beginner-ai-okuryazarligi-01");

        var archive = await admin.PatchAsJsonAsync($"/api/v1/admin/lessons/{first.Id}/status", new { status = "Archived" });
        archive.EnsureSuccessStatusCode();

        var student = factory.CreateClient();
        var direct = await student.GetAsync("/api/v1/lessons/beginner-ai-okuryazarligi-01");
        Assert.Equal(HttpStatusCode.NotFound, direct.StatusCode);

        var path = await student.GetFromJsonAsync<PathPayload>("/api/v1/learning-paths/beginner");
        Assert.DoesNotContain(path!.Units.SelectMany(x => x.Lessons), lesson => lesson.Slug == "beginner-ai-okuryazarligi-01");
    }

    private sealed record AdminLessonPayload(Guid Id, string Slug, string Status);

    private sealed record PathPayload(UnitPayload[] Units);

    private sealed record UnitPayload(LessonPayload[] Lessons);

    private sealed record LessonPayload(string Slug);
}
