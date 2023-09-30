using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Represents the style information of a control.
/// </summary>
public class XmlStyleInformation
{
    private readonly Dictionary<(string controlName, string controlNamespace), IReadOnlyDictionary<string, string>> _contents;

    /// <summary>
    /// Creates a new instance of <see cref="XmlStyleInformation"/>.
    /// </summary>
    public XmlStyleInformation()
    {
        _contents = new();
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="XmlStyleInformation"/> with the given style information.
    /// </summary>
    /// <param name="styles">The style information.</param>
    public XmlStyleInformation(Dictionary<(string controlName, string controlNamespace), IReadOnlyDictionary<string, string>> styles)
    {
        _contents = styles;
    }

    /// <summary>
    /// Adds a style definition to this instance.
    /// </summary>
    /// <param name="line">The line of the control.</param>
    /// <param name="column">The column of the control.</param>
    /// <param name="controlName">The name of the control.</param>
    /// <param name="controlNamespace">The namespace of the control.</param>
    /// <param name="attributes">The attributes of the control.</param>
    /// <exception cref="XmlException">Thrown when a style definition for the given control name and namespace already exists.</exception>
    public void Set(int line, int column, string controlName, string controlNamespace, IReadOnlyDictionary<string,string> attributes)
    {
        var key = (controlName, controlNamespace);
        _contents[key] = attributes;
    }
    
    /// <summary>
    /// Returns all style information stored in this instance.
    /// </summary>
    /// <returns>The style information.</returns>
    public IReadOnlyDictionary<(string controlName, string controlNamespace), IReadOnlyDictionary<string, string>> GetAll() => _contents.AsReadOnly();

    /// <summary>
    /// Returns the style information for the given control name and namespace.
    /// </summary>
    /// <param name="nodeName">The name of the control.</param>
    /// <param name="nodeNamespace">The namespace of the control.</param>
    /// <param name="attributes">Optional attributes to override the style information.</param>
    /// <returns>A dictionary containing the style information.</returns>
    public Dictionary<string, string> Of(string nodeName, string nodeNamespace, IReadOnlyDictionary<string,string>? attributes = null)
    {
        var result = new Dictionary<string, string>();
        if (_contents.TryGetValue((nodeName, nodeNamespace), out var attributeDictionary))
        {
            foreach (var (key, value) in attributeDictionary)
            {
                result.Add(key, value);
            }
        }

        if (attributes is null)
            return result;
        foreach (var (key, value) in attributes)
        {
            result[key] = value;
        }

        return result;
    }
}