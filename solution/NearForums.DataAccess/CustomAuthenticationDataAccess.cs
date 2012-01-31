﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace NearForums.DataAccess
{
	/// <summary>
	/// Represents a set of method to access the custom authentication providers database.
	/// </summary>
	public class CustomAuthenticationDataAccess : BaseDataAccess
	{
		public CustomAuthenticationDataAccess()
		{
			if (!Config.AuthenticationProviders.Custom.IsDefined)
			{
				throw new ConfigurationErrorsException("Custom Authentication Provider is not defined in the site configuration.");
			}
			Factory = DbProviderFactories.GetFactory(Config.AuthenticationProviders.Custom.ConnectionString.ProviderName);
		}

		public override DbConnection GetConnection()
		{
			var conn = this.Factory.CreateConnection();
			conn.ConnectionString = Config.AuthenticationProviders.Custom.ConnectionString.ConnectionString;
			return conn;
		}

		/// <summary>
		/// Gets the user data from the custom authentication provider
		/// </summary>
		/// <returns>An instance of User type, with only the fields id, name, email filled in.</returns>
		public User GetUser(string userName, string password)
		{
			User user = null;
			DbCommand comm = GetCommand(Config.AuthenticationProviders.Custom.StoredProcedure);
			comm.AddParameter<string>(this.Factory, "username", userName);
			comm.AddParameter<string>(this.Factory, "password", password);

			var dr = GetFirstRow(comm);
			if (dr != null)
			{
				user = new User();
				user.Id = dr.Get<int>("userid");
				user.UserName = dr.GetString("username");
				user.Email = dr.GetString("useremail");
			}
			return user;
		}
	}
}