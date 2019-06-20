using System;
using Minimig;
using Xunit;

namespace MinimigTests.Unit
{
    public class SqlServerStatementsFixture
    {
        [Fact]
        public void Sql_server_statements_insert()
        {
            //Arrange
            string table = "MyMigration";
            const string expected = "INSERT";

            //Act
            var s = new SqlServerStatements(table);

            //Assert
            Assert.Contains(expected, s.InsertMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.InsertMigration);
        }

        [Fact]
        public void Sql_server_statements_insert_with_schema()
        {
            //Arrange
            string table = "MyMigration";
            string schema = "tst";
            const string expected = "INSERT";

            //Act
            var s = new SqlServerStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.InsertMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.InsertMigration);
            Assert.Contains(schema, s.InsertMigration);
        }

        [Fact]
        public void Sql_server_statements_update()
        {
            //Arrange
            string table = "MyMigration";
            const string expected = "UPDATE";

            //Act
            var s = new SqlServerStatements(table);

            //Assert
            Assert.Contains(expected, s.UpdateMigrationHash, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.UpdateMigrationHash);
            Assert.Contains(expected, s.RenameMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.RenameMigration);
        }

        [Fact]
        public void Sql_server_statements_update_with_schema()
        {
            //Arrange
            string table = "MyMigration";
            string schema = "tst";
            const string expected = "UPDATE";

            //Act
            var s = new SqlServerStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.UpdateMigrationHash, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.UpdateMigrationHash);
            Assert.Contains(schema, s.UpdateMigrationHash);
            Assert.Contains(expected, s.RenameMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.RenameMigration);
            Assert.Contains(schema, s.RenameMigration);
        }

        [Fact]
        public void Sql_server_statements_migration_exists()
        {
            //Arrange
            string table = "MyMigration";
            const string expected = "SELECT";

            //Act
            var s = new SqlServerStatements(table);

            //Assert
            Assert.Contains(expected, s.DoesMigrationsTableExist, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.DoesMigrationsTableExist);
            Assert.Contains(expected, s.GetAlreadyRan, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.GetAlreadyRan);
        }

        [Fact]
        public void Sql_server_statements__migration_exists_with_schema()
        {
            //Arrange
            string table = "MyMigration";
            string schema = "tst";
            const string expected = "SELECT";

            //Act
            var s = new SqlServerStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.DoesMigrationsTableExist, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.DoesMigrationsTableExist);
            Assert.Contains(schema, s.DoesMigrationsTableExist);
            Assert.Contains(expected, s.GetAlreadyRan, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.GetAlreadyRan);
            Assert.Contains(schema, s.GetAlreadyRan);
        }

        [Fact]
        public void Sql_server_statements_create_migration()
        {
            //Arrange
            string table = "MyMigration";
            const string expected = "CREATE";

            //Act
            var s = new SqlServerStatements(table);

            //Assert
            Assert.Contains(expected, s.CreateMigrationsTable, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.CreateMigrationsTable);
        }

        [Fact]
        public void Sql_server_statements_drop_migration()
        {
            //Arrange
            string table = "MyMigration";
            const string expected = "DROP";

            //Act
            var s = new SqlServerStatements(table);

            //Assert
            Assert.Contains(expected, s.DropMigrationsTable, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.DropMigrationsTable);
        }

        [Fact]
        public void Sql_server_statements_create_migration_with_schema()
        {
            //Arrange
            string table = "MyMigration";
            string schema = "tst";
            const string expected = "CREATE";

            //Act
            var s = new SqlServerStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.CreateMigrationsTable, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.CreateMigrationsTable);
            Assert.Contains(schema, s.CreateMigrationsTable);
        }

        [Fact]
        public void Sql_server_statements_sql_server_command_splitter()
        {
            //Arrange
            string value = "GO";
            string lowerValue = "go";
            string table = "MyMigration";
            string schema = "tst";

            //Act
            var s = new SqlServerStatements(table, schema);

            //Assert
            Assert.True(s.CommandSplitter.Match(value).Success);
            Assert.True(s.CommandSplitter.Match(lowerValue).Success);
        }
    }
}
