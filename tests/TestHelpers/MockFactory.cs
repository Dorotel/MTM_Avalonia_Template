using NSubstitute;

namespace MTM_Template_Tests.TestHelpers;

/// <summary>
/// Factory for creating NSubstitute mocks with common patterns
/// </summary>
public static class MockFactory
{
    /// <summary>
    /// Creates a substitute for the specified interface type
    /// </summary>
    public static T CreateMock<T>() where T : class
    {
        return Substitute.For<T>();
    }

    /// <summary>
    /// Creates a substitute with pre-configured return values
    /// </summary>
    public static T CreateMockWithDefaults<T>() where T : class
    {
        var mock = Substitute.For<T>();
        ConfigureDefaults(mock);
        return mock;
    }

    private static void ConfigureDefaults<T>(T mock) where T : class
    {
        // Add default configurations here as needed
        // Example: mock.SomeMethod().Returns(defaultValue);
    }

    /// <summary>
    /// Creates a mock with callbacks to verify method calls
    /// </summary>
    public static T CreateMockWithCallbacks<T>(System.Action<T> configureCallbacks) where T : class
    {
        var mock = Substitute.For<T>();
        configureCallbacks(mock);
        return mock;
    }
}
