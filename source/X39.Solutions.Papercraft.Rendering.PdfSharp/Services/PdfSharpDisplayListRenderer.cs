using PdfSharp.Drawing;
using PdfSharp.Pdf;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal sealed class PdfSharpDisplayListRenderer
{
    public void Render(XGraphics graphics, PdfPage page, DisplayList displayList)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(displayList);

        var state = new RenderState(page.Width.Point, page.Height.Point);
        foreach (var command in displayList.Commands)
        {
            RenderCommand(graphics, page, command, state);
        }
    }

    private static void RenderCommand(
        XGraphics graphics,
        PdfPage page,
        DisplayCommand command,
        RenderState state)
    {
        switch (command)
        {
            case PushStateCommand:
                graphics.Save();
                state.Push();
                break;
            case PopStateCommand:
                if (state.CanPop)
                {
                    graphics.Restore();
                    state.Pop();
                }
                break;
            case TranslateCommand translate:
                state.Translate(translate.Offset.X, translate.Offset.Y);
                break;
            case ClipCommand clip:
                var clipRectangle = TransformRectangle(clip.Rectangle, state);
                graphics.IntersectClip(ToXRect(clipRectangle));
                state.IntersectClip(clipRectangle);
                break;
            case DrawLineCommand line:
                DrawLine(graphics, line, state);
                break;
            case DrawRectangleCommand rectangle:
                DrawRectangle(graphics, rectangle, state);
                break;
            case DrawTextCommand text:
                DrawText(graphics, text, state);
                break;
            case DrawImageCommand image:
                DrawImage(graphics, image, state);
                break;
            case LinkAnnotationCommand link:
                AddLinkAnnotation(page, link, state);
                break;
        }
    }

    private static void DrawLine(
        XGraphics graphics,
        DrawLineCommand line,
        RenderState state)
    {
        var pen = new XPen(ToXColor(line.Color), Math.Max(0D, line.Thickness));
        graphics.DrawLine(
            pen,
            line.StartX + state.TranslateX,
            line.StartY + state.TranslateY,
            line.EndX + state.TranslateX,
            line.EndY + state.TranslateY);
    }

    private static void DrawRectangle(
        XGraphics graphics,
        DrawRectangleCommand rectangle,
        RenderState state)
    {
        if (rectangle.Color.Alpha is 0)
            return;

        var brush = new XSolidBrush(ToXColor(rectangle.Color));
        graphics.DrawRectangle(brush, ToXRect(TransformRectangle(rectangle.Rectangle, state)));
    }

    private static void DrawText(
        XGraphics graphics,
        DrawTextCommand text,
        RenderState state)
    {
        if (string.IsNullOrEmpty(text.Text) || text.TextStyle.Foreground.Alpha is 0)
            return;

        var font = PdfSharpFontHelper.CreateFont(text.TextStyle, text.Dpi);
        var brush = new XSolidBrush(ToXColor(text.TextStyle.Foreground));
        var x = text.X + state.TranslateX;
        var y = text.Y + state.TranslateY;
        if (!IntersectsClip(graphics, font, text, x, y, state.ClipRectangle))
            return;

        if (NeedsTextTransform(text.TextStyle))
        {
            var graphicsState = graphics.Save();
            try
            {
                graphics.TranslateTransform(x, y);
                if (Math.Abs(text.TextStyle.Rotation) > float.Epsilon)
                    graphics.RotateTransform(text.TextStyle.Rotation);
                if (Math.Abs(text.TextStyle.Scale - 1F) > float.Epsilon)
                    graphics.ScaleTransform(text.TextStyle.Scale, 1D);

                graphics.DrawString(text.Text, font, brush, 0D, 0D);
            }
            finally
            {
                graphics.Restore(graphicsState);
            }

            return;
        }

        graphics.DrawString(text.Text, font, brush, x, y);
    }

    private static bool IntersectsClip(
        XGraphics graphics,
        XFont font,
        DrawTextCommand text,
        double x,
        double y,
        DisplayRectangle clipRectangle)
    {
        var width = graphics.MeasureString(text.Text, font).Width;
        if (clipRectangle.Width <= 0F || clipRectangle.Height <= 0F)
            return false;

        var height = font.GetHeight();
        var scale = text.TextStyle.Scale;
        var rotation = text.TextStyle.Rotation * Math.PI / 180D;
        var cosine = Math.Cos(rotation);
        var sine = Math.Sin(rotation);
        Span<XPoint> corners = stackalloc XPoint[]
        {
            TransformTextPoint(0D, -height, scale, cosine, sine, x, y),
            TransformTextPoint(width, -height, scale, cosine, sine, x, y),
            TransformTextPoint(width, height, scale, cosine, sine, x, y),
            TransformTextPoint(0D, height, scale, cosine, sine, x, y),
        };
        var left = corners[0].X;
        var top = corners[0].Y;
        var right = left;
        var bottom = top;
        foreach (var corner in corners[1..])
        {
            left = Math.Min(left, corner.X);
            top = Math.Min(top, corner.Y);
            right = Math.Max(right, corner.X);
            bottom = Math.Max(bottom, corner.Y);
        }

        return left < clipRectangle.Right
               && right > clipRectangle.Left
               && top < clipRectangle.Bottom
               && bottom > clipRectangle.Top;
    }

    private static XPoint TransformTextPoint(
        double localX,
        double localY,
        double scale,
        double cosine,
        double sine,
        double originX,
        double originY)
    {
        localX *= scale;
        return new XPoint(
            originX + localX * cosine - localY * sine,
            originY + localX * sine + localY * cosine);
    }

    private static void DrawImage(
        XGraphics graphics,
        DrawImageCommand image,
        RenderState state)
    {
        if (image.Bytes.Length is 0 || image.Rectangle.Width <= 0F || image.Rectangle.Height <= 0F)
            return;

        using var stream = new MemoryStream(image.Bytes, writable: false);
        try
        {
            using var xImage = XImage.FromStream(stream);
            graphics.DrawImage(xImage, ToXRect(TransformRectangle(image.Rectangle, state)));
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }
    }

    private static void AddLinkAnnotation(PdfPage page, LinkAnnotationCommand link, RenderState state)
    {
        if (string.IsNullOrWhiteSpace(link.Uri)
            || link.Rectangle.Width <= 0F
            || link.Rectangle.Height <= 0F)
        {
            return;
        }

        page.AddWebLink(ToPdfRectangle(link.Rectangle, state), link.Uri);
    }

    private static bool NeedsTextTransform(DisplayTextStyle textStyle)
        => Math.Abs(textStyle.Rotation) > float.Epsilon
           || Math.Abs(textStyle.Scale - 1F) > float.Epsilon;

    private static XRect ToXRect(DisplayRectangle rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

    private static DisplayRectangle TransformRectangle(DisplayRectangle rectangle, RenderState state)
        => new(
            (float)(rectangle.Left + state.TranslateX),
            (float)(rectangle.Top + state.TranslateY),
            rectangle.Width,
            rectangle.Height);

    private static PdfRectangle ToPdfRectangle(DisplayRectangle rectangle, RenderState state)
    {
        var left = rectangle.Left + state.TranslateX;
        var top = rectangle.Top + state.TranslateY;
        var right = left + rectangle.Width;
        var bottom = top + rectangle.Height;
        return new PdfRectangle(
            new XPoint(left, state.PageHeight - bottom),
            new XPoint(right, state.PageHeight - top));
    }

    private static XColor ToXColor(DisplayColor color)
        => XColor.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

    private sealed class RenderState
    {
        private readonly Stack<(double TranslateX, double TranslateY, DisplayRectangle ClipRectangle)> _stack = new();

        public RenderState(double pageWidth, double pageHeight)
        {
            PageHeight = pageHeight;
            ClipRectangle = new DisplayRectangle(0F, 0F, (float) pageWidth, (float) pageHeight);
        }

        public double PageHeight { get; }

        public double TranslateX { get; private set; }

        public double TranslateY { get; private set; }

        public DisplayRectangle ClipRectangle { get; private set; }

        public bool CanPop => _stack.Count > 0;

        public void Push()
            => _stack.Push((TranslateX, TranslateY, ClipRectangle));

        public void Pop()
        {
            var restored = _stack.Pop();
            TranslateX = restored.TranslateX;
            TranslateY = restored.TranslateY;
            ClipRectangle = restored.ClipRectangle;
        }

        public void Translate(double x, double y)
        {
            TranslateX += x;
            TranslateY += y;
        }

        public void IntersectClip(DisplayRectangle rectangle)
        {
            var left = Math.Max(ClipRectangle.Left, rectangle.Left);
            var top = Math.Max(ClipRectangle.Top, rectangle.Top);
            var right = Math.Min(ClipRectangle.Right, rectangle.Right);
            var bottom = Math.Min(ClipRectangle.Bottom, rectangle.Bottom);
            ClipRectangle = new DisplayRectangle(
                left,
                top,
                Math.Max(0F, right - left),
                Math.Max(0F, bottom - top));
        }
    }
}
