using System;
using Minimig;

namespace MinimigTests.Fakes
{
    public class FakeMigrationRow : MigrationRow
    {
        public FakeMigrationRow()
        {
            Id = 1;
            Filename = "c:\\temp\\abc";
            Duration = 0;
            Hash = Guid.NewGuid().ToString();
            ExecutionDate = DateTime.Now;
        }

        public FakeMigrationRow(string filename, string hash)
        {
            Id = 1;
            Filename = filename;
            Duration = 0;
            Hash = hash;
            ExecutionDate = DateTime.Now;
        }
    }
}
