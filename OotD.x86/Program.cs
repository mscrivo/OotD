namespace OotD;

/// <summary>
///     Thin x86 host process. All application logic lives in OotD.Core; this exe exists only to
///     provide a 32-bit process so that Outlook COM interop matches a 32-bit Outlook installation.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Startup.Run(args);
    }
}
