namespace Nookly.Application.Media;

public sealed class DuplicateMediaException(string message) : InvalidOperationException(message);
