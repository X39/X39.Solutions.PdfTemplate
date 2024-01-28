using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Services.TextService;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// A control that displays text.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class TextControl : TextBaseControl
{

    /// <summary>
    /// Creates a new instance of <see cref="TextControl"/>
    /// </summary>
    /// <param name="textService">The text service to use.</param>#
    [ControlConstructor]
    public TextControl(ITextService textService) : base(textService)
    {
    }

    /// <summary>
    /// The text to display.
    /// </summary>
    [Parameter(IsContent = true)]
    public string Text { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override string GetText() => Text;
}