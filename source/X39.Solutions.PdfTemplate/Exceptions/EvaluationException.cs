﻿using System.Runtime.Serialization;

namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Base class for all exceptions that occur during evaluation.
/// </summary>
public abstract class EvaluationException : Exception
{
    /// <inheritdoc />
    protected EvaluationException()
    {
    }

    /// <inheritdoc />
    [Obsolete("Obsolete")]
    protected EvaluationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc />
    protected EvaluationException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    protected EvaluationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}