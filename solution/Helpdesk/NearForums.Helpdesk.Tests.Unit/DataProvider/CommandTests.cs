using NearForums.Helpdesk.DataProvider;
using NearForums.Helpdesk.Tests.Unit.Fakes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit.DataProvider
{
    public class CommandTests
    {
        [SetUp]
        public void SetUp()
        {
            Fakes.DataAccessFakeFactory.CleanUp();
        }

        [Test]
        public void ShouldFailIfSchemaEmpty()
        {
            CommandForTests cmd;
            
            Assert.Throws<Command.InvalidCommandException>(() => cmd = new CommandForTests(string.Empty));
        }

        [Test]
        public void ExecuteNonQueryShouldSetShemaFirst()
        {
            ShouldExecuteSetSchemaBeforeEveryCommandExecution(cmd => cmd.ExecuteNonQuery());
        }

        [Test]
        public void ExecuteScalarShouldSetSchemaFirst()
        {
            ShouldExecuteSetSchemaBeforeEveryCommandExecution(cmd => cmd.ExecuteScalar());
        }

        [Test]
        public void ExecuteReaderShouldSetSchemaFirst()
        {
            ShouldExecuteSetSchemaBeforeEveryCommandExecution(cmd => cmd.ExecuteReader());
        }

        private TResult ShouldExecuteSetSchemaBeforeEveryCommandExecution<TResult>(Func<DbCommand, TResult> action)
        {
            CommandForTests cmd = new CommandForTests(TEST_SCHEMA);
            Assert.AreEqual(TEST_SCHEMA, cmd.Schema);

            TResult result = action(cmd);

            Assert.AreEqual(2, DataAccessFakeFactory.commandsInExecutionOrder.Count());
            AssertIsCorrectSetSchemaCommand(DataAccessFakeFactory.commandsInExecutionOrder.First(), TEST_SCHEMA);

            return result;
        }

        private void AssertIsCorrectSetSchemaCommand(DbCommand cmd, string schemaName)
        {
            string expectedSetSchemaQuery = string.Format("SET search_path TO {0};", schemaName);
            Assert.AreEqual(expectedSetSchemaQuery, cmd.CommandText);
            Assert.AreEqual(0, cmd.Parameters.Count);

            cmd.Received(1).ExecuteNonQuery();
        }

        private class CommandForTests : Command
        {
            public CommandForTests(string schemaName)
                : this(Fakes.DataAccessFakeFactory.Create<DbCommand>(), schemaName)
            {

            }

            public CommandForTests(DbCommand baseCommand, string schemaName)
                :base(baseCommand, schemaName)
            {
                base.factory = Fakes.DataAccessFakeFactory.Create<DbProviderFactory>();
                base.Connection = new Npgsql.NpgsqlConnection();
            }

           
        }

        private const string TEST_SCHEMA = "TEST_SCHEMA";
    }
}
