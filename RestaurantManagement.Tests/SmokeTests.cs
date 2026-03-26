using FluentAssertions;
using Xunit;

namespace RestaurantManagement.Tests;

public class SmokeTests
{
    [Fact]
    public void Test_project_should_run()
    {
        true.Should().BeTrue();
    }
}
