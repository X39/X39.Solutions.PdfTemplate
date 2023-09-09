namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Contains the default colors.
/// </summary>
[PublicAPI]
public static class Colors
{
    internal static readonly TypeExpressionMapCache<Color> AccessCache = new(typeof(Colors));
    
    
    /// <summary>The color black with the RGBA value of (0, 0, 0, 255).</summary>
    public static Color Black { get; } = new(0, 0, 0);
    
    /// <summary>The color white with the RGBA value of (255, 255, 255, 255).</summary>
    public static Color White { get; } = new(255, 255, 255);
    
    /// <summary>The color red with the RGBA value of (255, 0, 0, 255).</summary>
    public static Color Red { get; } = new(255, 0, 0);
    
    /// <summary>The color green with the RGBA value of (0, 255, 0, 255).</summary>
    public static Color Green { get; } = new(0, 255, 0);
    
    /// <summary>The color blue with the RGBA value of (0, 0, 255, 255).</summary>
    public static Color Blue { get; } = new(0, 0, 255);
    
    /// <summary>The color yellow with the RGBA value of (255, 255, 0, 255).</summary>
    public static Color Yellow { get; } = new(255, 255, 0);
    
    /// <summary>The color cyan with the RGBA value of (0, 255, 255, 255).</summary>
    public static Color Cyan { get; } = new(0, 255, 255);
    
    /// <summary>The color magenta with the RGBA value of (255, 0, 255, 255).</summary>
    public static Color Magenta { get; } = new(255, 0, 255);
    
    /// <summary>The color transparent with the RGBA value of (0, 0, 0, 0).</summary>
    public static Color Transparent { get; } = new(0, 0, 0, 0);
    
    /// <summary>The color light gray with the RGBA value of (211, 211, 211, 255).</summary>
    public static Color LightGray { get; } = new(211, 211, 211);
    
    /// <summary>The color dark gray with the RGBA value of (169, 169, 169, 255).</summary>
    public static Color DarkGray { get; } = new(169, 169, 169);
    
    /// <summary>The color gray with the RGBA value of (128, 128, 128, 255).</summary>
    public static Color Gray { get; } = new(128, 128, 128);
    
    /// <summary>The color orange with the RGBA value of (255, 165, 0, 255).</summary>
    public static Color Orange { get; } = new(255, 165, 0);
    
    /// <summary>The color brown with the RGBA value of (165, 42, 42, 255).</summary>
    public static Color Brown { get; } = new(165, 42, 42);
    
    /// <summary>The color pink with the RGBA value of (255, 192, 203, 255).</summary>
    public static Color Pink { get; } = new(255, 192, 203);
    
    /// <summary>The color purple with the RGBA value of (128, 0, 128, 255).</summary>
    public static Color Purple { get; } = new(128, 0, 128);
    
}