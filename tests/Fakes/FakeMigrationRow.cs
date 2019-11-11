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
    }
}
