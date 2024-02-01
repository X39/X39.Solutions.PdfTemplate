using System.ComponentModel;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services.TextService;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Represents a control that displays the current page number in a document.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class PageNumberControl : TextBaseControl
{
    /// <summary>
    /// Enumeration representing the different page number modes.
    /// </summary>
    public enum EPageNumberMode
    {
        /// <summary>
        /// Displays the current page number.
        /// </summary>
        Current,

        /// <summary>
        /// Displays the total page count.
        /// </summary>
        Total,

        /// <summary>
        /// Displays the current page number and the total page count.
        /// </summary>
        CurrentTotal,

        /// <summary>
        /// Displays the current page number and the total page count.
        /// </summary>
        TotalCurrent,
    }

    /// <summary>
    /// Creates a new instance of <see cref="PageNumberControl"/>
    /// </summary>
    /// <param name="textService">The text service to use.</param>#
    [ControlConstructor]
    public PageNumberControl(ITextService textService)
        : base(textService) { }

    /// <summary>
    /// Gets or sets the prefix to display before the page number.
    /// </summary>
    /// <remarks>
    /// The prefix is not including a whitespace. If you want to have a whitespace between the prefix and the page number,
    /// you have to add it to the prefix.
    /// </remarks>
    [Parameter]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delimiter to display between the page number and the total page count (if enabled).
    /// </summary>
    [Parameter]
    public string Delimiter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mode to use for the page number.
    /// </summary>
    [Parameter]
    public EPageNumberMode Mode { get; set; } = EPageNumberMode.Current;

    /// <summary>
    /// Gets or sets the suffix to display after the page number.
    /// </summary>
    /// <remarks>
    /// The suffix is not including a whitespace. If you want to have a whitespace between the page number and the suffix,
    /// you have to add it to the suffix.
    /// </remarks>
    [Parameter]
    public string Suffix { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override string GetText() => GetText(ushort.MaxValue, ushort.MaxValue);

    private string GetText(ushort pageNumber, ushort totalPages)
    {
        return Mode switch
        {
            EPageNumberMode.Current => string.Concat(Prefix, pageNumber.ToString(CultureInfo.InvariantCulture), Suffix),
            EPageNumberMode.Total   => string.Concat(Prefix, totalPages.ToString(CultureInfo.InvariantCulture), Suffix),
            EPageNumberMode.CurrentTotal => string.Concat(
                Prefix,
                pageNumber.ToString(CultureInfo.InvariantCulture),
                Delimiter,
                totalPages.ToString(CultureInfo.InvariantCulture),
                Suffix
            ),
            EPageNumberMode.TotalCurrent => string.Concat(
                Prefix,
                totalPages.ToString(CultureInfo.InvariantCulture),
                Delimiter,
                pageNumber.ToString(CultureInfo.InvariantCulture),
                Suffix
            ),
            _ => throw new InvalidEnumArgumentException(nameof(Mode), (int) Mode, typeof(EPageNumberMode)),
        };
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        canvas.Defer(
            (immediateCanvas) => RenderText(
                immediateCanvas,
                dpi,
                GetText(immediateCanvas.PageNumber, immediateCanvas.TotalPages)
            )
        );
        return Size.Zero;
    }
}
