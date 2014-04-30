using NSubstitute;
using NSubstitute.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit.Fakes
{
    public class DataAccessFakeFactory
    {
        public static T Create<T>()
            where T : class
        {
            T value;

            if (ImplementsInterface<DbProviderFactory, T>())
            {
                value = CreateDbProviderFactory() as T;
            }
            else if (ImplementsInterface<DbConnection, T>())
            {
                value = CreateDbConnection() as T;
            }
            else if (ImplementsInterface<DbCommand, T>())
            {
                value = CreateDbCommand() as T;
            }
            else throw new Exception("Unexpected type : " + typeof(T).FullName);

            return value;
        }

        private static bool ImplementsInterface<TInterface, TObject>()
        {
            return typeof(TInterface).IsAssignableFrom(typeof(TObject));
        }

        private static DbParameter CreateParameter()
        {
            DbParameter newParam = Substitute.For<DbParameter>();

            parameters.Add(newParam);

            return newParam;
        }

        private static DbConnection CreateDbConnection()
        {
            DbConnection conn = Substitute.For<DbConnection>();

            return conn;
        }

        private static DbCommand CreateDbCommand()
        {
            DbCommand newCommand = Substitute.For<DbCommand>();
            commands.Add(newCommand);

            DbConnection conn = Create<DbConnection>();
            newCommand.Connection.Returns(conn);
            newCommand.CreateParameter().Returns(args => CreateParameter());

            ParamterCollection paramters = new ParamterCollection();
            newCommand.Parameters.Returns(paramters);

            newCommand.ExecuteReader().Returns(
                args =>
                {
                    DataAccessFakeFactory.MarkExecuted(newCommand);
                    return Substitute.For<DbDataReader>();
                });
            newCommand.ExecuteNonQuery().Returns(
                args =>
                {
                    int result = 0;
                    
                    if (0 < executeNonQuery.Count())
                    {
                        Func<int> command = executeNonQuery.First();
                        executeNonQuery = executeNonQuery.Skip(1).Take(executeNonQuery.Count() - 1).ToList();
                        result = command();
                    }

                    MarkExecuted(newCommand);

                    return result;
                });

            newCommand.ExecuteScalar().Returns(
                args =>
                {
                    DataAccessFakeFactory.MarkExecuted(newCommand);
                    return new object();
                });

            return newCommand;
        }

        private static void MarkExecuted(DbCommand cmd)
        {
            commandsInExecutionOrder.Add(cmd);
        }

        private static DbProviderFactory CreateDbProviderFactory()
        {
            DbProviderFactory factory = Substitute.For<DbProviderFactory>();

            factory.CreateCommand().Returns( args => CreateDbCommand() );
            factory.CreateConnection().Returns( args => CreateDbConnection() );
            factory.CreateParameter().Returns( args => CreateParameter() );

            return factory;
        }

        public static IList<DbParameter> parameters = new List<DbParameter>();
        public static IList<DbCommand> commands = new List<DbCommand>();

        public static ICollection<Func<int>> executeNonQuery = new List<Func<int>>();
        public static IList<DbCommand> commandsInExecutionOrder = new List<DbCommand>();

        internal static void CleanUp()
        {
            commands.Clear();
            parameters.Clear();
            commandsInExecutionOrder.Clear();
        }

        private class ParamterCollection : DbParameterCollection
        {
            public List<DbParameter> paramters = new List<DbParameter>();

            public override int Add(object value)
            {
                paramters.Add((DbParameter)value);

                return 1;
            }

            public override void AddRange(Array values)
            {
                throw new NotImplementedException();
            }

            public override void Clear()
            {
                throw new NotImplementedException();
            }

            public override bool Contains(string value)
            {
                throw new NotImplementedException();
            }

            public override bool Contains(object value)
            {
                throw new NotImplementedException();
            }

            public override void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public override int Count
            {
                get { return paramters.Count; }
            }

            public override System.Collections.IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }

            protected override DbParameter GetParameter(string parameterName)
            {
                throw new NotImplementedException();
            }

            protected override DbParameter GetParameter(int index)
            {
                return paramters.ElementAt(index);
            }

            public override int IndexOf(string parameterName)
            {
                throw new NotImplementedException();
            }

            public override int IndexOf(object value)
            {
                throw new NotImplementedException();
            }

            public override void Insert(int index, object value)
            {
                throw new NotImplementedException();
            }

            public override bool IsFixedSize
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public override bool IsSynchronized
            {
                get { throw new NotImplementedException(); }
            }

            public override void Remove(object value)
            {
                throw new NotImplementedException();
            }

            public override void RemoveAt(string parameterName)
            {
                throw new NotImplementedException();
            }

            public override void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            protected override void SetParameter(string parameterName, DbParameter value)
            {
                throw new NotImplementedException();
            }

            protected override void SetParameter(int index, DbParameter value)
            {
                throw new NotImplementedException();
            }

            public override object SyncRoot
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
