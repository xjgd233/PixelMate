namespace PixelMate.Tests;

public sealed class AppIdentityTests
{
    [Fact]
    public void GitHubUrl_MatchesVerifiedAuthorProfile()
    {
        Assert.Equal("https://github.com/xjgd233", AppIdentity.GitHubUrl);
    }
}
