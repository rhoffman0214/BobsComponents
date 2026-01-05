using Bunit;
using BobsComponent.Library.Components;
using BobsComponent.Library.Enums;
using Xunit;

namespace BobsComponent.Library.Tests.Components;

public class AsyncButtonTests : TestContext
{
    [Fact]
    public void AsyncButton_Renders_WithDefaultText()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>();

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Click me", button.TextContent);
    }

    [Fact]
    public void AsyncButton_Renders_WithCustomText()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.Text, "Custom Button"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Custom Button", button.TextContent);
    }

    [Fact]
    public void AsyncButton_HasCorrectStyleClass()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.Style, ButtonStyle.Primary));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("btn-primary", button.ClassName);
    }

    [Fact]
    public void AsyncButton_IsDisabled_WhenIsDisabledIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.IsDisabled, true));

        // Assert
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void AsyncButton_IsNotDisabled_ByDefault()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>();

        // Assert
        var button = cut.Find("button");
        Assert.False(button.HasAttribute("disabled"));
    }

    [Fact]
    public async Task AsyncButton_ExecutesOnClickAsync_WhenClicked()
    {
        // Arrange
        var wasClicked = false;
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.OnClickAsync, () =>
            {
                wasClicked = true;
                return Task.CompletedTask;
            }));

        // Act
        var button = cut.Find("button");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.True(wasClicked);
    }

    [Fact]
    public async Task AsyncButton_ShowsLoadingState_DuringAsyncOperation()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.OnClickAsync, () => tcs.Task));

        // Act
        var button = cut.Find("button");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert - button should be disabled during loading
        Assert.True(button.HasAttribute("disabled"));
        Assert.Contains("state-loading", button.ClassName);

        // Complete the task
        tcs.SetResult(true);
        await Task.Delay(100); // Give time for state update

        // Assert - button should be enabled again after completion
        cut.WaitForAssertion(() =>
        {
            var btn = cut.Find("button");
            Assert.Contains("state-success", btn.ClassName);
        });
    }

    [Fact]
    public async Task AsyncButton_TriggerClickAsync_ExecutesClick()
    {
        // Arrange
        var wasClicked = false;
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.OnClickAsync, () =>
            {
                wasClicked = true;
                return Task.CompletedTask;
            }));

        // Act
        await cut.Instance.TriggerClickAsync();

        // Assert
        Assert.True(wasClicked);
    }

    [Fact]
    public async Task AsyncButton_ResetAsync_ResetsToIdleState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.OnClickAsync, () => tcs.Task)
            .Add(p => p.AutoResetDelay, 0)); // Disable auto reset

        // Act - click button to start loading
        var button = cut.Find("button");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Complete the task
        tcs.SetResult(true);
        await Task.Delay(100);

        // Reset the button
        await cut.Instance.ResetAsync();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var btn = cut.Find("button");
            Assert.Contains("state-idle", btn.ClassName);
        });
    }

    [Theory]
    [InlineData(ButtonStyle.Primary, "btn-primary")]
    [InlineData(ButtonStyle.Secondary, "btn-secondary")]
    [InlineData(ButtonStyle.Tertiary, "btn-tertiary")]
    [InlineData(ButtonStyle.Success, "btn-success")]
    [InlineData(ButtonStyle.Warning, "btn-warning")]
    [InlineData(ButtonStyle.Error, "btn-error")]
    public void AsyncButton_AppliesCorrectStyleClass(ButtonStyle style, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.Style, style));

        // Assert
        var button = cut.Find("button");
        Assert.Contains(expectedClass, button.ClassName);
    }
}
