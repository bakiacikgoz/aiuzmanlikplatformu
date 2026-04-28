using System.Net;
using System.Net.Http.Json;

namespace AiEducation.Api.Tests;

public sealed class AdminContentTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Admin_can_create_edit_publish_preview_and_archive_lesson()
    {
        var admin = factory.CreateClient();
        ApiFactory.Authorize(admin, await factory.RegisterAdminAndGetTokenAsync(admin, "admin-content@example.com"));

        var create = await admin.PostAsJsonAsync("/api/v1/admin/lessons", new
        {
            unitSlug = "ai-okuryazarligi",
            slug = "admin-created-ai-byte",
            title = "Admin Created AI Byte",
            learningObjective = "Tek bir AI ürün kararını açıklamak.",
            miniExplanation = "Bu mikro ders, AI ürünlerinde karar anını ve başarı ölçümünü kısa bir örnekle anlatır.",
            tinyExample = "Örnek: Kullanıcı mesajını sınıflandırıp doğru destek kuyruğuna göndermek.",
            exercisePrompt = "Bir AI ürün kararını tek cümleyle yaz.",
            durationMinutes = 4,
            xpReward = 10,
            sortOrder = 50,
            status = "Draft"
        });
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<CreatedLessonPayload>();

        var student = factory.CreateClient();
        var hiddenDraft = await student.GetAsync("/api/v1/lessons/admin-created-ai-byte");
        Assert.Equal(HttpStatusCode.NotFound, hiddenDraft.StatusCode);

        var poorPublish = await admin.PatchAsJsonAsync($"/api/v1/admin/lessons/{created!.Id}/status", new { status = "Published" });
        Assert.Equal(HttpStatusCode.BadRequest, poorPublish.StatusCode);

        var resource = await admin.PostAsJsonAsync("/api/v1/admin/resources", new
        {
            slug = "admin-created-resource",
            title = "AI Product Quality Note",
            url = "https://example.com/ai-product-quality",
            type = "article",
            summary = "Quality reference"
        });
        resource.EnsureSuccessStatusCode();
        var resourcePayload = await resource.Content.ReadFromJsonAsync<CreatedResourcePayload>();

        var patch = await admin.PatchAsJsonAsync($"/api/v1/admin/lessons/{created.Id}", new
        {
            title = "Admin Updated AI Byte",
            miniExplanation = string.Join(" ", Enumerable.Repeat("Kaliteli mikro ders tek hedef, örnek ve aktif uygulama içerir.", 12)),
            tinyExample = "Örnek: Destek talebini sınıflandırıp güven skoru düşükse insana yönlendir.",
            nextStep = "Bir sonraki AI Byte içinde ölçüm metriğini seç.",
            resourceIds = new[] { resourcePayload!.Id }
        });
        patch.EnsureSuccessStatusCode();

        var ready = await admin.PatchAsJsonAsync($"/api/v1/admin/lessons/{created.Id}/status", new { status = "ReadyForReview" });
        ready.EnsureSuccessStatusCode();
        var publish = await admin.PatchAsJsonAsync($"/api/v1/admin/lessons/{created.Id}/status", new { status = "Published" });
        publish.EnsureSuccessStatusCode();

        var visible = await student.GetAsync("/api/v1/lessons/admin-created-ai-byte");
        visible.EnsureSuccessStatusCode();
        var preview = await admin.GetAsync($"/api/v1/admin/lessons/{created.Id}/preview");
        preview.EnsureSuccessStatusCode();

        var archive = await admin.PatchAsJsonAsync($"/api/v1/admin/lessons/{created.Id}/status", new { status = "Archived" });
        archive.EnsureSuccessStatusCode();
        var hiddenArchived = await student.GetAsync("/api/v1/lessons/admin-created-ai-byte");
        Assert.Equal(HttpStatusCode.NotFound, hiddenArchived.StatusCode);
    }

    private sealed record CreatedLessonPayload(Guid Id, string Slug);

    private sealed record CreatedResourcePayload(Guid Id, string Slug);
}
