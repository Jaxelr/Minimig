using System;

namespace Minimig;

public class MigrationResult
{
    public bool Success { get; internal set; }
    public int Attempted { get; internal set; }
    public int Ran { get; internal set; }
    public int Skipped { get; internal set; }
    public int Renamed { get; internal set; }
    public int Forced { get; internal set; }
    public Exception Exception { get; internal set; }
}
