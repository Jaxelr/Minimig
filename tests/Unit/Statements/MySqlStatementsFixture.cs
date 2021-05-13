using System;
using Minimig;
using Xunit;

namespace MinimigTests.Unit
{
    public class MySqlStatementsFixture
    {
        [Fact]
        public void MySql_statements_insert()
        {
            //Arrange
            const string table = "mymigration";
            const string expected = "INSERT";

            //Act
            var s = new MySqlStatements(table);

            //Assert
            Assert.Contains(expected, s.InsertMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.InsertMigration);
        }

        [Fact]
        public void MySql_statements_insert_with_schema()
        {
            //Arrange
            const string table = "mymigration";
            const string schema = "tst";
            const string expected = "INSERT";

            //Act
            var s = new MySqlStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.InsertMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.InsertMigration);
            Assert.Contains(schema, s.InsertMigration);
        }

        [Fact]
        public void MySql_statements_update()
        {
            //Arrange
            const string table = "mymigration";
            const string expected = "UPDATE";

            //Act
            var s = new MySqlStatements(table);

            //Assert
            Assert.Contains(expected, s.UpdateMigrationHash, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.UpdateMigrationHash);
            Assert.Contains(expected, s.RenameMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.RenameMigration);
        }

        [Fact]
        public void MySql_statements_update_with_schema()
        {
            //Arrange
            const string table = "mymigration";
            const string schema = "tst";
            const string expected = "UPDATE";

            //Act
            var s = new MySqlStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.UpdateMigrationHash, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.UpdateMigrationHash);
            Assert.Contains(schema, s.UpdateMigrationHash);
            Assert.Contains(expected, s.RenameMigration, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.RenameMigration);
            Assert.Contains(schema, s.RenameMigration);
        }

        [Fact]
        public void MySql_statements_migration_exists()
        {
            //Arrange
            const string table = "mymigration";
            const string expected = "SELECT";

            //Act
            var s = new MySqlStatements(table);

            //Assert
            Assert.Contains(expected, s.DoesMigrationsTableExist, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.DoesMigrationsTableExist);
            Assert.Contains(expected, s.GetAlreadyRan, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.GetAlreadyRan);
        }

        [Fact]
        public void MySql_statements__migration_exists_with_schema()
        {
            //Arrange
            const string table = "mymigration";
            const string schema = "tst";
            const string expected = "SELECT";

            //Act
            var s = new MySqlStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.DoesMigrationsTableExist, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.DoesMigrationsTableExist);
            Assert.Contains(schema, s.DoesMigrationsTableExist);
            Assert.Contains(expected, s.GetAlreadyRan, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.GetAlreadyRan);
            Assert.Contains(schema, s.GetAlreadyRan);
        }

        [Fact]
        public void MySql_statements_create_migration()
        {
            //Arrange
            const string table = "mymigration";
            const string expected = "CREATE";

            //Act
            var s = new MySqlStatements(table);

            //Assert
            Assert.Contains(expected, s.CreateMigrationsTable, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.CreateMigrationsTable);
        }

        [Fact]
        public void MySql_statements_create_migration_with_schema()
        {
            //Arrange
            const string table = "mymigration";
            const string schema = "tst";
            const string expected = "CREATE";

            //Act
            var s = new MySqlStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.CreateMigrationsTable, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.CreateMigrationsTable);
            Assert.Contains(schema, s.CreateMigrationsTable);
        }

        [Fact]
        public void MySql_statements_drop_migration()
        {
            //Arrange
            const string table = "mymigration";
            const string expected = "DROP";

            //Act
            var s = new MySqlStatements(table);

            //Assert
            Assert.Contains(expected, s.DropMigrationsTable, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.DropMigrationsTable);
        }

        [Fact]
        public void MySql_statements_drop_migration_with_schema()
        {
            //Arrange
            const string table = "mymigration";
            const string schema = "tst";
            const string expected = "DROP";

            //Act
            var s = new MySqlStatements(table, schema);

            //Assert
            Assert.Contains(expected, s.DropMigrationsTable, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains(table, s.DropMigrationsTable);
            Assert.Contains(schema, s.DropMigrationsTable);
        }

        [Fact]
        public void MySql_statements_command_splitter()
        {
            //Arrange
            const string value = ";";
            const string lowerValue = ";";
            const string table = "mymigration";
            const string schema = "tst";

            //Act
            var s = new MySqlStatements(table, schema);

            //Assert
            Assert.True(s.CommandSplitter.Match(value).Success);
            Assert.True(s.CommandSplitter.Match(lowerValue).Success);
        }
    }
}
