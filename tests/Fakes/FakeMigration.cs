using System.Text.RegularExpressions;

namespace MinimigTests.Fakes;

public class FakeMigration(string filePath) : Migration(filePath, new("\r\n|\n\r|\n|\r", RegexOptions.Compiled))
{
}
