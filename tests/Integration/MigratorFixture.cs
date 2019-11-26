using Minimig;
using Xunit;

namespace MinimigTests.Integration
{
    public class MigratorFixture
    {
        [Theory]
        [InlineData(".", "master", false, "customTableA")]
        public void Migrator_instantiation(string server, string database, bool isPreview, string table)
        {
            //Arrange
            var option = new Options() { Server = server, Database = database, IsPreview = isPreview, MigrationsTable = table };
            var connection = new ConnectionContext(option);

            //Act
            using (var mig = new Migrator(option))
            {
                //Assert
                Assert.Empty(mig.Migrations);
            }

            //Cleanup
            connection.Open();
            connection.DropMigrationsTable();
            connection.Dispose();
        }

        [Theory]
        [InlineData(".", "master", "customTableB", "..\\..\\..\\..\\sampleMigrations")]
        public void Migrator_instantiation_with_migrations(string server, string database, string table, string migrationsFolder)
        {
            //Arrange
            var option = new Options() { Server = server, Database = database, MigrationsTable = table, MigrationsFolder = migrationsFolder };
            var connection = new ConnectionContext(option);
            int outstanding = Migrator.GetOutstandingMigrationsCount(option);

            //Act
            using (var mig = new Migrator(option))
            {
                //Assert
                Assert.Equal(5, outstanding);
                Assert.Equal(5, mig.Migrations.Count);
            }

            //Cleanup
            connection.Open();
            connection.DropMigrationsTable();
            connection.Dispose();
        }
    }
}
