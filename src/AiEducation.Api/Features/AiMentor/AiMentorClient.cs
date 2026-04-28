namespace AiEducation.Api.Features.AiMentor;

public sealed record AiMentorRequest(string Message, string? LessonSlug = null, string? Context = null);

public sealed record AiMentorResponse(string Content, string SafetyNote);

public interface IAiMentorClient
{
    Task<AiMentorResponse> RespondAsync(AiMentorRequest request, CancellationToken cancellationToken = default);
}

public sealed class MockAiMentorClient : IAiMentorClient
{
    public Task<AiMentorResponse> RespondAsync(AiMentorRequest request, CancellationToken cancellationToken = default)
    {
        var lessonContext = string.IsNullOrWhiteSpace(request.LessonSlug)
            ? "aktif ders bağlamına"
            : $"{request.LessonSlug} dersine";
        var content = $"Mock mentor: {lessonContext} göre kısa cevap: {request.Message}. Önce kavramı tek cümlede tanımla, sonra küçük bir örnekle pekiştir.";
        return Task.FromResult(new AiMentorResponse(content, "Bu MVP yanıtı mock sağlayıcıdan gelir; gerçek API anahtarı frontend'e konulmaz."));
    }
}
