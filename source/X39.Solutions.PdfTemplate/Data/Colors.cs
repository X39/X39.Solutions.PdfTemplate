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
    
    /// <summary>The color transparent black with the RGBA value of (0, 0, 0, 0).</summary>
    public static Color TransparentBlack { get; } = new(0, 0, 0, 0);
    
    /// <summary>The color transparent white with the RGBA value of (255, 255, 255, 0).</summary>
    public static Color TransparentWhite { get; } = new(255, 255, 255, 0);
    
    /// <summary>The color cornflower blue with the RGBA value of (100, 149, 237, 255).</summary>
    public static Color CornflowerBlue { get; } = new(100, 149, 237);
    
    /// <summary>The color light sky blue with the RGBA value of (135, 206, 250, 255).</summary>
    public static Color LightSkyBlue { get; } = new(135, 206, 250);
    
    /// <summary>The color light steel blue with the RGBA value of (176, 196, 222, 255).</summary>
    public static Color LightSteelBlue { get; } = new(176, 196, 222);
    
    /// <summary>The color royal blue with the RGBA value of (65, 105, 225, 255).</summary>
    public static Color RoyalBlue { get; } = new(65, 105, 225);
    
    /// <summary>The color midnight blue with the RGBA value of (25, 25, 112, 255).</summary>
    public static Color MidnightBlue { get; } = new(25, 25, 112);
    
    /// <summary>The color dark blue with the RGBA value of (0, 0, 139, 255).</summary>
    public static Color DarkBlue { get; } = new(0, 0, 139);
    
    /// <summary>The color medium blue with the RGBA value of (0, 0, 205, 255).</summary>
    public static Color MediumBlue { get; } = new(0, 0, 205);
    
    /// <summary>The color blue violet with the RGBA value of (138, 43, 226, 255).</summary>
    public static Color BlueViolet { get; } = new(138, 43, 226);
    
    /// <summary>The color indigo with the RGBA value of (75, 0, 130, 255).</summary>
    public static Color Indigo { get; } = new(75, 0, 130);
    
    /// <summary>The color dark slate blue with the RGBA value of (72, 61, 139, 255).</summary>
    public static Color DarkSlateBlue { get; } = new(72, 61, 139);
    
    /// <summary>The color slate blue with the RGBA value of (106, 90, 205, 255).</summary>
    public static Color SlateBlue { get; } = new(106, 90, 205);
    
    /// <summary>The color medium slate blue with the RGBA value of (123, 104, 238, 255).</summary>
    public static Color MediumSlateBlue { get; } = new(123, 104, 238);
    
    /// <summary>The color medium purple with the RGBA value of (147, 112, 219, 255).</summary>
    public static Color MediumPurple { get; } = new(147, 112, 219);
    
    /// <summary>The color dark orchid with the RGBA value of (153, 50, 204, 255).</summary>
    public static Color DarkOrchid { get; } = new(153, 50, 204);
    
    /// <summary>The color dark violet with the RGBA value of (148, 0, 211, 255).</summary>
    public static Color DarkViolet { get; } = new(148, 0, 211);
    
    /// <summary>The color dark magenta with the RGBA value of (139, 0, 139, 255).</summary>
    public static Color DarkMagenta { get; } = new(139, 0, 139);
    
    /// <summary>The color medium orchid with the RGBA value of (186, 85, 211, 255).</summary>
    public static Color MediumOrchid { get; } = new(186, 85, 211);
    
    /// <summary>The color thistle with the RGBA value of (216, 191, 216, 255).</summary>
    public static Color Thistle { get; } = new(216, 191, 216);
    
    /// <summary>The color plum with the RGBA value of (221, 160, 221, 255).</summary>
    public static Color Plum { get; } = new(221, 160, 221);
    
    /// <summary>The color violet with the RGBA value of (238, 130, 238, 255).</summary>
    public static Color Violet { get; } = new(238, 130, 238);
    
    /// <summary>The color orchid with the RGBA value of (218, 112, 214, 255).</summary>
    public static Color Orchid { get; } = new(218, 112, 214);
    
    /// <summary>The color medium violet red with the RGBA value of (199, 21, 133, 255).</summary>
    public static Color MediumVioletRed { get; } = new(199, 21, 133);
    
    /// <summary>The color pale violet red with the RGBA value of (219, 112, 147, 255).</summary>
    public static Color PaleVioletRed { get; } = new(219, 112, 147);
    
    /// <summary>The color deep pink with the RGBA value of (255, 20, 147, 255).</summary>
    public static Color DeepPink { get; } = new(255, 20, 147);
    
    /// <summary>The color hot pink with the RGBA value of (255, 105, 180, 255).</summary>
    public static Color HotPink { get; } = new(255, 105, 180);
    
    /// <summary>The color light pink with the RGBA value of (255, 182, 193, 255).</summary>
    public static Color LightPink { get; } = new(255, 182, 193);
    
    /// <summary>The color antique white with the RGBA value of (250, 235, 215, 255).</summary>
    public static Color AntiqueWhite { get; } = new(250, 235, 215);
    
    /// <summary>The color beige with the RGBA value of (245, 245, 220, 255).</summary>
    public static Color Beige { get; } = new(245, 245, 220);
    
    /// <summary>The color bisque with the RGBA value of (255, 228, 196, 255).</summary>
    public static Color Bisque { get; } = new(255, 228, 196);
    
    /// <summary>The color blanched almond with the RGBA value of (255, 235, 205, 255).</summary>
    public static Color BlanchedAlmond { get; } = new(255, 235, 205);
    
    /// <summary>The color wheat with the RGBA value of (245, 222, 179, 255).</summary>
    public static Color Wheat { get; } = new(245, 222, 179);
    
    /// <summary>The color cornsilk with the RGBA value of (255, 248, 220, 255).</summary>
    public static Color Cornsilk { get; } = new(255, 248, 220);
    
    /// <summary>The color lemon chiffon with the RGBA value of (255, 250, 205, 255).</summary>
    public static Color LemonChiffon { get; } = new(255, 250, 205);
    
    /// <summary>The color light goldenrod yellow with the RGBA value of (250, 250, 210, 255).</summary>
    public static Color LightGoldenrodYellow { get; } = new(250, 250, 210);
    
    /// <summary>The color light yellow with the RGBA value of (255, 255, 224, 255).</summary>
    public static Color LightYellow { get; } = new(255, 255, 224);
    
    /// <summary>The color saddle brown with the RGBA value of (139, 69, 19, 255).</summary>
    public static Color SaddleBrown { get; } = new(139, 69, 19);
    
    /// <summary>The color sienna with the RGBA value of (160, 82, 45, 255).</summary>
    public static Color Sienna { get; } = new(160, 82, 45);
    
    /// <summary>The color chocolate with the RGBA value of (210, 105, 30, 255).</summary>
    public static Color Chocolate { get; } = new(210, 105, 30);
    
    /// <summary>The color peru with the RGBA value of (205, 133, 63, 255).</summary>
    public static Color Peru { get; } = new(205, 133, 63);
    
    /// <summary>The color sandy brown with the RGBA value of (244, 164, 96, 255).</summary>
    public static Color SandyBrown { get; } = new(244, 164, 96);
    
    /// <summary>The color burly wood with the RGBA value of (222, 184, 135, 255).</summary>
    public static Color BurlyWood { get; } = new(222, 184, 135);
    
    /// <summary>The color tan with the RGBA value of (210, 180, 140, 255).</summary>
    public static Color Tan { get; } = new(210, 180, 140);
    
    /// <summary>The color rosy brown with the RGBA value of (188, 143, 143, 255).</summary>
    public static Color RosyBrown { get; } = new(188, 143, 143);
    
    /// <summary>The color moccasin with the RGBA value of (255, 228, 181, 255).</summary>
    public static Color Moccasin { get; } = new(255, 228, 181);
    
    /// <summary>The color navajo white with the RGBA value of (255, 222, 173, 255).</summary>
    public static Color NavajoWhite { get; } = new(255, 222, 173);
    
    /// <summary>The color peach puff with the RGBA value of (255, 218, 185, 255).</summary>
    public static Color PeachPuff { get; } = new(255, 218, 185);
    
    /// <summary>The color misty rose with the RGBA value of (255, 228, 225, 255).</summary>
    public static Color MistyRose { get; } = new(255, 228, 225);
    
    /// <summary>The color lavender blush with the RGBA value of (255, 240, 245, 255).</summary>
    public static Color LavenderBlush { get; } = new(255, 240, 245);
    
    /// <summary>The color linen with the RGBA value of (250, 240, 230, 255).</summary>
    public static Color Linen { get; } = new(250, 240, 230);
    
    /// <summary>The color old lace with the RGBA value of (253, 245, 230, 255).</summary>
    public static Color OldLace { get; } = new(253, 245, 230);
    
    /// <summary>The color papaya whip with the RGBA value of (255, 239, 213, 255).</summary>
    public static Color PapayaWhip { get; } = new(255, 239, 213);
    
    /// <summary>The color sea shell with the RGBA value of (255, 245, 238, 255).</summary>
    public static Color SeaShell { get; } = new(255, 245, 238);
    
    /// <summary>The color mint cream with the RGBA value of (245, 255, 250, 255).</summary>
    public static Color MintCream { get; } = new(245, 255, 250);
    
    /// <summary>The color slate gray with the RGBA value of (112, 128, 144, 255).</summary>
    public static Color SlateGray { get; } = new(112, 128, 144);
    
    /// <summary>The color light slate gray with the RGBA value of (119, 136, 153, 255).</summary>
    public static Color LightSlateGray { get; } = new(119, 136, 153);
    
    /// <summary>The color lavender with the RGBA value of (230, 230, 250, 255).</summary>
    public static Color Lavender { get; } = new(230, 230, 250);
    
    /// <summary>The color floral white with the RGBA value of (255, 250, 240, 255).</summary>
    public static Color FloralWhite { get; } = new(255, 250, 240);
    
    /// <summary>The color alice blue with the RGBA value of (240, 248, 255, 255).</summary>
    public static Color AliceBlue { get; } = new(240, 248, 255);
    
    /// <summary>The color ghost white with the RGBA value of (248, 248, 255, 255).</summary>
    public static Color GhostWhite { get; } = new(248, 248, 255);
    
    /// <summary>The color honeydew with the RGBA value of (240, 255, 240, 255).</summary>
    public static Color Honeydew { get; } = new(240, 255, 240);
    
    /// <summary>The color ivory with the RGBA value of (255, 255, 240, 255).</summary>
    public static Color Ivory { get; } = new(255, 255, 240);
    
    /// <summary>The color azure with the RGBA value of (240, 255, 255, 255).</summary>
    public static Color Azure { get; } = new(240, 255, 255);
    
    /// <summary>The color snow with the RGBA value of (255, 250, 250, 255).</summary>
    public static Color Snow { get; } = new(255, 250, 250);
    
    /// <summary>The color dim gray with the RGBA value of (105, 105, 105, 255).</summary>
    public static Color DimGray { get; } = new(105, 105, 105);
    
    /// <summary>The color silver with the RGBA value of (192, 192, 192, 255).</summary>
    public static Color Silver { get; } = new(192, 192, 192);
    
    /// <summary>The color gainsboro with the RGBA value of (220, 220, 220, 255).</summary>
    public static Color Gainsboro { get; } = new(220, 220, 220);
    
    /// <summary>The color white smoke with the RGBA value of (245, 245, 245, 255).</summary>
    public static Color WhiteSmoke { get; } = new(245, 245, 245);
}