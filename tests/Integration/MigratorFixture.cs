using System.Linq;
using Minimig;
using Xunit;

namespace MinimigTests.Integration
{
    public class MigratorFixture
    {
        private readonly int migrationCount = 5;

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
        [InlineData(".", "master", "customTableB", "..\\..\\..\\..\\sampleMigrations\\SqlServer")]
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
                Assert.Equal(migrationCount, outstanding);
                Assert.Equal(migrationCount, mig.Migrations.Count());
            }

            //Cleanup
            connection.Open();
            connection.DropMigrationsTable();
            connection.Dispose();
        }

        [Theory]
        [InlineData(".", "master", "missingSchema", "customTableB", "..\\..\\..\\..\\sampleMigrations\\SqlServer")]
        public void Migrator_instantiation_with_migrations_and_missing_schema(string server, string database, string schema, string table, string migrationsFolder)
        {
            //Arrange
            var option = new Options()
            {
                Server = server,
                Database = database,
                MigrationsTableSchema = schema,
                MigrationsTable = table,
                MigrationsFolder = migrationsFolder
            };
            var connection = new ConnectionContext(option);

            //Act
            using (var mig = new Migrator(option))
            {
                //Assert
                Assert.Equal(migrationCount, mig.Migrations.Count());
            }

            //Cleanup
            connection.Open();
            connection.Dispose();
        }

        [Theory]
        [InlineData(".", "master", "customTableC", "..\\..\\..\\..\\sampleMigrations\\SqlServer")]
        public void Migrator_instantiation_with_migrations_and_run_outstanding_migrations(string server, string database, string table, string migrationsFolder)
        {
            //Arrange
            var option = new Options() { Server = server, Database = database, MigrationsTable = table, MigrationsFolder = migrationsFolder };
            var connection = new ConnectionContext(option);
            var result = Migrator.RunOutstandingMigrations(option);

            //Act
            using (var mig = new Migrator(option))
            {
                //Assert
                Assert.Equal(migrationCount, result.Attempted);
                Assert.Equal(migrationCount, result.Ran);
                Assert.True(result.Success);
                Assert.Equal(migrationCount, mig.Migrations.Count());
            }

            //Cleanup
            connection.Open();
            connection.DropMigrationsTable();
            connection.Dispose();
        }

        [Theory]
        [InlineData(".", "master", "customTableD", "..\\..\\..\\..\\sampleMigrations\\SqlServer")]
        public void Migrator_instantiation_with_migrations_and_run_outstanding_migrations_single_transaction(string server, string database, string table, string migrationsFolder)
        {
            //Arrange
            var option = new Options() { Server = server, Database = database, MigrationsTable = table, MigrationsFolder = migrationsFolder, UseGlobalTransaction = true };
            var connection = new ConnectionContext(option);
            var result = Migrator.RunOutstandingMigrations(option);

            //Act
            using (var mig = new Migrator(option))
            {
                //Assert
                Assert.Equal(migrationCount, result.Attempted);
                Assert.Equal(migrationCount, result.Ran);
                Assert.True(result.Success);
                Assert.Equal(migrationCount, mig.Migrations.Count());
            }

            //Cleanup
            connection.Open();
            connection.DropMigrationsTable();
            connection.Dispose();
        }

        [Theory]
        [InlineData(".", "master", "customTableE", "..\\..\\..\\..\\sampleMigrations\\SqlServer")]
        public void Migrator_instantiation_with_migrations_and_run_outstanding_migrations_twice(string server, string database, string table, string migrationsFolder)
        {
            //Arrange
            var option = new Options() { Server = server, Database = database, MigrationsTable = table, MigrationsFolder = migrationsFolder };
            var connection = new ConnectionContext(option);
            var result = Migrator.RunOutstandingMigrations(option);
            var result2 = Migrator.RunOutstandingMigrations(option);

            //Act
            using (var mig = new Migrator(option))
            {
                //Assert
                Assert.Equal(migrationCount, result.Attempted);
                Assert.Equal(migrationCount, result2.Attempted);
                Assert.Equal(migrationCount, result.Ran);
                Assert.Equal(migrationCount, result2.Skipped);
                Assert.True(result.Success);
                Assert.True(result2.Success);
                Assert.Equal(migrationCount, mig.Migrations.Count());
            }

            //Cleanup
            connection.Open();
            connection.DropMigrationsTable();
            connection.Dispose();
        }
    }
}
