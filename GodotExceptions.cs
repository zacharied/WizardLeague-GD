using System;

// ReSharper disable once CheckNamespace
namespace wizardballz.exceptions;

public class ExportFieldUnsetException : Exception
{
    public ExportFieldUnsetException()
    {
    }

    public ExportFieldUnsetException(string message) : base(message)
    {
    }

    public ExportFieldUnsetException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class MissingChildException : Exception
{
    public MissingChildException()
    {
    }

    public MissingChildException(string message) : base(message)
    {
    }

    public MissingChildException(string message, Exception inner) : base(message, inner)
    {
    }
}
