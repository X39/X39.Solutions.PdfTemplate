namespace X39.Solutions.PdfTemplate.Test;

internal class VoidStream : Stream
{
    public override void Flush()
    {
        /* empty */
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return 0;
    }

    public override void SetLength(long value)
    {
        /* empty */
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        /* empty */
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;
    public override long Length => long.MaxValue;

    public override long Position
    {
        get => 0;
        set
        {
            /* empty */
        }
    }
}