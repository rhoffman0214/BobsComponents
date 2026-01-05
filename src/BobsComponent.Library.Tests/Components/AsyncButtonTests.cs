using Bunit;
using BobsComponent.Library.Components;
using BobsComponent.Library.Enums;
using Microsoft.AspNetCore.Components;
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

    [Theory]
    [InlineData(ButtonSize.Small, "btn-small")]
    [InlineData(ButtonSize.Medium, "btn-medium")]
    [InlineData(ButtonSize.Large, "btn-large")]
    public void AsyncButton_AppliesCorrectSizeClass(ButtonSize size, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.Size, size));

        // Assert
        var button = cut.Find("button");
        Assert.Contains(expectedClass, button.ClassName);
    }

    [Fact]
    public void AsyncButton_RendersWithId()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.Id, "my-button-id"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("my-button-id", button.Id);
    }

    [Fact]
    public void AsyncButton_RendersWithName()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.Name, "submit-button"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("submit-button", button.GetAttribute("name"));
    }

    [Theory]
    [InlineData("button")]
    [InlineData("submit")]
    [InlineData("reset")]
    public void AsyncButton_RendersWithCorrectType(string buttonType)
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.ButtonType, buttonType));

        // Assert
        var button = cut.Find("button");
        Assert.Equal(buttonType, button.GetAttribute("type"));
    }

    [Fact]
    public void AsyncButton_RendersWithTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.Title, "Click to submit"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("Click to submit", button.GetAttribute("title"));
    }

    [Fact]
    public void AsyncButton_RendersWithAriaLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.AriaLabel, "Submit form"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("Submit form", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void AsyncButton_SetsAriaBusyWhenLoading()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.OnClickAsync, () => tcs.Task));

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.Equal("true", button.GetAttribute("aria-busy"));

        // Cleanup
        tcs.SetResult(true);
    }

    [Fact]
    public void AsyncButton_PassesAdditionalAttributes()
    {
        // Arrange & Act
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .AddUnmatched("data-test-id", "my-test-button")
            .AddUnmatched("data-analytics", "click-tracking"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("my-test-button", button.GetAttribute("data-test-id"));
        Assert.Equal("click-tracking", button.GetAttribute("data-analytics"));
    }

    [Fact]
    public async Task AsyncButton_OnClickEventFires()
    {
        // Arrange
        var clickCount = 0;
        var cut = RenderComponent<AsyncButton>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                this, () => clickCount++))
            .Add(p => p.OnClickAsync, () => Task.CompletedTask));

        // Act
        var button = cut.Find("button");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.Equal(1, clickCount);
    }
}
