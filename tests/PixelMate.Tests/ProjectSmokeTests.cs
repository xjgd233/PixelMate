using System.Runtime.InteropServices;

namespace PixelMate.Tests;

public class ProjectSmokeTests
{
    [Fact]
    public void DevelopmentHost_IsAppleSiliconArm64()
    {
        Assert.Equal(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
    }
}
