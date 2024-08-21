using System.Text.RegularExpressions;
using Minimig;

namespace MinimigTests.Fakes;

public class FakeMigration(string filePath) : Migration(filePath, new("\r\n|\n\r|\n|\r", RegexOptions.Compiled))
{
}
