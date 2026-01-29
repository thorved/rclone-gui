namespace RcloneGui.Features.Ftp.Models;

/// <summary>
/// TLS/SSL mode for FTP connections.
/// </summary>
public enum FtpTlsMode
{
    /// <summary>
    /// No encryption (plain FTP).
    /// </summary>
    None,

    /// <summary>
    /// Implicit TLS on port 990 (FTPS).
    /// </summary>
    Implicit,

    /// <summary>
    /// Explicit TLS via AUTH TLS command.
    /// </summary>
    Explicit
}
