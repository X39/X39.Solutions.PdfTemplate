namespace X39.Solutions.PdfTemplate.Services.PropertyAccessCache;

/// <summary>
/// Interface for accessing arbitrary properties on types.
/// </summary>
public interface IPropertyAccessCache
{
    /// <summary>
    /// Gets the value of a property on an instance.
    /// </summary>
    /// <param name="instance">The instance to get the value from.</param>
    /// <param name="propertyName">The name of the property to get.</param>
    /// <param name="value">Out parameter for the value of the property.</param>
    /// <returns><see langword="true"/> if the property was found, <see langword="false"/> otherwise.</returns>
    bool Get(object instance, string propertyName, out object? value);

    /// <summary>
    /// Sets the value of a property on an instance.
    /// </summary>
    /// <param name="instance">The instance to set the value on.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value to set.</param>
    /// <returns><see langword="true"/> if the property was found, <see langword="false"/> otherwise.</returns>
    bool Set(object instance, string propertyName, object? value);

    /// <summary>
    /// Maps the given type to the cache.
    /// </summary>
    /// <remarks>
    /// This method is to prepopulate the cache with types that are known to be used.
    /// </remarks>
    /// <param name="type">The type to map.</param>
    void Map(Type type);

    /// <summary>
    /// Clears the cache.
    /// </summary>
    void Clear();
}