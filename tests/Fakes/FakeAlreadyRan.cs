using Minimig;

namespace MinimigTests.Fakes;

internal class FakeAlreadyRan : AlreadyRan
{
    public FakeAlreadyRan(MigrationRow row) : base(new MigrationRow[1] { row })
    {
    }
}
