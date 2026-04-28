using System.Net;
using System.Net.Http.Json;

namespace AiEducation.Api.Tests;

public sealed class AdminAuthorizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Admin_endpoints_require_admin_role()
    {
        var anonymous = factory.CreateClient();
        var anonymousResponse = await anonymous.GetAsync("/api/v1/admin/content");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var learner = factory.CreateClient();
        var learnerToken = await ApiFactory.RegisterAndGetTokenAsync(learner, "learner-admin-check@example.com");
        ApiFactory.Authorize(learner, learnerToken);
        var learnerResponse = await learner.GetAsync("/api/v1/admin/content");
        Assert.Equal(HttpStatusCode.Forbidden, learnerResponse.StatusCode);

        var admin = factory.CreateClient();
        var adminToken = await factory.RegisterAdminAndGetTokenAsync(admin, "admin-auth-check@example.com");
        ApiFactory.Authorize(admin, adminToken);
        var adminResponse = await admin.GetAsync("/api/v1/admin/content");
        adminResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Development_seed_endpoints_are_not_available_in_testing_like_production()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/dev/reset-database", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
