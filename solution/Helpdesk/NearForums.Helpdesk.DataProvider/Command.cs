using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace NearForums.Helpdesk.DataProvider
{
    public class Command : DbCommand
    {
        public Command(DbCommand cmd, string schema)
        {
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidCommandException("Invalid schema name : name epmty");
            }

            this.cmd = cmd;
            this.Schema = schema;
        }

        public override void Cancel()
        {
            this.cmd.Cancel();
        }

        public override string CommandText
        {
            get
            {
                return this.cmd.CommandText;
            }
            set
            {
                this.cmd.CommandText = value;
            }
        }

        public override int CommandTimeout
        {
            get
            {
                return this.cmd.CommandTimeout;
            }
            set
            {
                this.cmd.CommandTimeout = value;
            }
        }

        public override System.Data.CommandType CommandType
        {
            get
            {
                return this.cmd.CommandType;
            }
            set
            {
                this.cmd.CommandType = value;
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                return this.cmd.DesignTimeVisible;
            }
            set
            {
                this.cmd.DesignTimeVisible = value;
            }
        }

        public override int ExecuteNonQuery()
        {
            this.SetSchema();

            return this.cmd.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            this.SetSchema();

            return this.cmd.ExecuteScalar();
        }

        public override void Prepare()
        {
            this.cmd.Prepare();
        }

        public string Schema
        {
            get;
            private set;
        }

        protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
        {
            this.SetSchema();

            return this.cmd.ExecuteReader(behavior);
        }

        protected override DbParameter CreateDbParameter()
        {
            return this.cmd.CreateParameter();
        }

        protected override DbConnection DbConnection
        {
            get
            {
                return this.cmd.Connection;
            }
            set
            {
                this.cmd.Connection = (NpgsqlConnection)value;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return this.cmd.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                return this.cmd.Transaction;
            }
            set
            {
                this.cmd.Transaction = (NpgsqlTransaction)value;
            }
        }

        public override System.Data.UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this.cmd.UpdatedRowSource;
            }
            set
            {
                this.cmd.UpdatedRowSource = value;
            }
        }

        protected DbCommand cmd;

        private void SetSchema()
        {
            DbCommand cmdSetSchema = (factory ?? Npgsql.NpgsqlFactory.Instance).CreateCommand();
            cmdSetSchema.CommandText = string.Format("SET search_path TO {0};", this.Schema);
            cmdSetSchema.Connection = this.cmd.Connection;

            cmdSetSchema.ExecuteNonQuery();
        }

        protected DbProviderFactory factory = null;

        private const string SCHEMA_NAME_PARAMETER = "name";

        public class InvalidCommandException : Exception
        {
            public InvalidCommandException(string msg)
                : base(msg)
            {
            }
        }
    }
}
