namespace MinimigTests.Fakes;

internal class FakeAlreadyRan(MigrationRow row) : AlreadyRan([row])
{
}
