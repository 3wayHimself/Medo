//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2007-10-28: Inital release.
//2007-11-06: Added fix for OleDb DateTime.
//2008-02-20: CreateCommand works with format strings.
//            Obsoleted old CreateCommand methods.
//            Obsoleted GetNew*Connection methods.
//2008-04-23: Removed obsolete CreateCommand() methods.
//2008-05-20: Small adjustments to debug output.
//2010-08-29: All IDbConnection members are internal to make usage from another assembly more clearer.
//2010-11-12: Added ProviderName property.
//            Open and Close are now public.


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Globalization;
using System.Text;

namespace Medo.Data {

    /// <summary>
    /// Represents an connection to a generic database.
    /// </summary>
    public class CommonConnection : IDbConnection, IDisposable {

        private IDbConnection _baseConnection;
        private DbProviderFactory _providerFactory;


        #region Constructors

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="providerInvariantName">Invariant name of a provider.</param>
        /// <exception cref="System.ArgumentException">Unable to find the requested .Net Framework Data Provider.  It may not be installed.</exception>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public CommonConnection(string providerInvariantName) {
#if DEBUG
			DataTable table = DbProviderFactories.GetFactoryClasses();
			StringBuilder sbProviders = new StringBuilder();
			for (int i = 0; i < table.Rows.Count; i++) {
				System.Data.DataRow row = table.Rows[i];
				if (sbProviders.Length > 0) { sbProviders.Append(", "); }
				sbProviders.Append(row[2]);
			}
            System.Diagnostics.Debug.WriteLine("I: Available providers: " + sbProviders.ToString() + ".   {{Medo.Data.CommonConnection}}");
#endif
            this.ProviderName = providerInvariantName;
            this._providerFactory = DbProviderFactories.GetFactory(providerInvariantName);
            this._baseConnection = this._providerFactory.CreateConnection();
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="providerInvariantName">Invariant name of a provider.</param>
        /// <param name="connectionString">A string containing connection settings.</param>
        /// <exception cref="System.ArgumentException">Unable to find the requested .Net Framework Data Provider.  It may not be installed.</exception>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public CommonConnection(string providerInvariantName, string connectionString)
            : this(providerInvariantName) {
            this._baseConnection.ConnectionString = connectionString;
        }


        #region CreateSqlClientConnection

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        public static CommonConnection CreateSqlClientConnection() {
            return new CommonConnection("System.Data.SqlClient");
        }

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        /// <param name="connectionString">A string containing connection settings.</param>
        public static CommonConnection CreateSqlClientConnection(string connectionString) {
            return new CommonConnection("System.Data.SqlClient", connectionString);
        }

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        /// <param name="server">SQL server to which connection is to be made. Eg. SERVER\SQLEXPRESS.</param>
        /// <param name="database">Database to be opened.</param>
        /// <param name="forceIntegratedSspiSecurity">If true, integrated security will be set to SSPI.</param>
        /// <param name="forceMultipleActiveResultSets">If true, MARS will be forced.</param>
        /// <param name="otherSettings">Other settings.</param>
        public static CommonConnection CreateSqlClientConnection(string server, string database, bool forceIntegratedSspiSecurity, bool forceMultipleActiveResultSets, params string[] otherSettings) {
            StringBuilder sbCS = new StringBuilder();
            if (!string.IsNullOrEmpty(server)) {
                sbCS.Append("Server=" + server);
                sbCS.Append(";");
            }
            if (!string.IsNullOrEmpty(database)) {
                sbCS.Append("Database=" + database);
                sbCS.Append(";");
            }
            if (forceIntegratedSspiSecurity) {
                sbCS.Append("Integrated Security=SSPI");
                sbCS.Append(";");
            }
            if (forceMultipleActiveResultSets) {
                sbCS.Append("MultipleActiveResultSets=true");
                sbCS.Append(";");
            }
            if (otherSettings != null) {
                for (int i = 0; i < otherSettings.Length; i++) {
                    string s = otherSettings[i];
                    if (s != null) {
                        sbCS.Append(s);
                        sbCS.Append(";");
                    }
                }
            }

            return new CommonConnection("System.Data.SqlClient", sbCS.ToString());
        }

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        [System.Obsolete("This method is obsoleted. Use CreateSqlClientConnection instead.")]
        public static CommonConnection GetNewSqlClientConnection() {
            return CreateSqlClientConnection();
        }

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        /// <param name="connectionString">A string containing connection settings.</param>
        [System.Obsolete("This method is obsoleted. Use CreateSqlClientConnection instead.")]
        public static CommonConnection GetNewSqlClientConnection(string connectionString) {
            return CreateSqlClientConnection(connectionString);
        }

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        /// <param name="server">SQL server to which connection is to be made. Eg. SERVER\SQLEXPRESS.</param>
        /// <param name="database">Database to be opened.</param>
        /// <param name="forceIntegratedSspiSecurity">If true, integrated security will be set to SSPI.</param>
        /// <param name="forceMultipleActiveResultSets">If true, MARS will be forced.</param>
        /// <param name="otherSettings">Other settings.</param>
        [System.Obsolete("This method is obsoleted. Use CreateSqlClientConnection instead.")]
        public static CommonConnection GetNewSqlClientConnection(string server, string database, bool forceIntegratedSspiSecurity, bool forceMultipleActiveResultSets, params string[] otherSettings) {
            return CreateSqlClientConnection(server, database, forceIntegratedSspiSecurity, forceMultipleActiveResultSets, otherSettings);
        }

        #endregion

        #region CreateOleDbConnection

        /// <summary>
        /// Returns new connection based on System.Data.OleDb data provider.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "This casing is used also in System.Data.OleDb.")]
        public static CommonConnection CreateOleDbConnection() {
            return new CommonConnection("System.Data.OleDb");
        }

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        /// <param name="provider">Provider to be used for connection. Eg. Microsoft.Jet.OleDb.4.0.</param>
        /// <param name="dataSource">Data source to use.</param>
        /// <param name="otherSettings">Other settings.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "This casing is used also in System.Data.OleDb.")]
        public static CommonConnection CreateOleDbConnection(string provider, string dataSource, params string[] otherSettings) {
            StringBuilder sbCS = new StringBuilder();
            if (!string.IsNullOrEmpty(provider)) {
                sbCS.Append("Provider=" + provider);
                sbCS.Append(";");
            }
            if (!string.IsNullOrEmpty(dataSource)) {
                sbCS.Append("Data Source=" + dataSource);
                sbCS.Append(";");
            }
            if (otherSettings != null) {
                for (int i = 0; i < otherSettings.Length; i++) {
                    string s = otherSettings[i];
                    if (s != null) {
                        sbCS.Append(s);
                        sbCS.Append(";");
                    }
                }
            }

            return new CommonConnection("System.Data.OleDb", sbCS.ToString());
        }

        /// <summary>
        /// Returns new connection based on System.Data.OleDb data provider.
        /// </summary>
        /// <param name="connectionString">A string containing connection settings.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "This casing is used also in System.Data.OleDb.")]
        public static CommonConnection CreateOleDbConnection(string connectionString) {
            return new CommonConnection("System.Data.OleDb", connectionString);
        }

        /// <summary>
        /// Returns new connection based on System.Data.OleDb data provider.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "This casing is used also in System.Data.OleDb.")]
        [System.Obsolete("This method is obsoleted. Use CreateOleDbConnection instead.")]
        public static CommonConnection GetNewOleDbConnection() {
            return CreateOleDbConnection();
        }

        /// <summary>
        /// Returns new connection based on System.Data.SqlClient data provider.
        /// </summary>
        /// <param name="provider">Provider to be used for connection. Eg. Microsoft.Jet.OleDb.4.0.</param>
        /// <param name="dataSource">Data source to use.</param>
        /// <param name="otherSettings">Other settings.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "This casing is used also in System.Data.OleDb.")]
        [System.Obsolete("This method is obsoleted. Use CreateOleDbConnection instead.")]
        public static CommonConnection GetNewOleDbConnection(string provider, string dataSource, params string[] otherSettings) {
            return CreateOleDbConnection(provider, dataSource, otherSettings);
        }

        /// <summary>
        /// Returns new connection based on System.Data.OleDb data provider.
        /// </summary>
        /// <param name="connectionString">A string containing connection settings.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "This casing is used also in System.Data.OleDb.")]
        [System.Obsolete("This method is obsoleted. Use CreateOleDbConnection instead.")]
        public static CommonConnection GetNewOleDbConnection(string connectionString) {
            return CreateOleDbConnection(connectionString);
        }

        #endregion

        #region CreateOdbcConnection

        /// <summary>
        /// Returns new connection based on System.Data.Odbc data provider.
        /// </summary>
        public static CommonConnection CreateOdbcConnection() {
            return new CommonConnection("System.Data.Odbc");
        }

        /// <summary>
        /// Returns new connection based on System.Data.Odbc data provider.
        /// </summary>
        /// <param name="connectionString">A string containing connection settings.</param>
        public static CommonConnection CreateOdbcConnection(string connectionString) {
            return new CommonConnection("System.Data.Odbc", connectionString);
        }

        /// <summary>
        /// Returns new connection based on System.Data.Odbc data provider.
        /// </summary>
        [System.Obsolete("This method is obsoleted. Use CreateOdbcConnection instead.")]
        public static CommonConnection GetNewOdbcConnection() {
            return CreateOdbcConnection();
        }

        /// <summary>
        /// Returns new connection based on System.Data.Odbc data provider.
        /// </summary>
        /// <param name="connectionString">A string containing connection settings.</param>
        [System.Obsolete("This method is obsoleted. Use CreateOdbcConnection instead.")]
        public static CommonConnection GetNewOdbcConnection(string connectionString) {
            return CreateOdbcConnection(connectionString);
        }

        #endregion

        #endregion


        #region Base properties

        /// <summary>
        /// Gets provider name.
        /// </summary>
        protected string ProviderName { get; private set; }

        /// <summary>
        /// Gets underlying connection.
        /// </summary>
        protected IDbConnection BaseConnection {
            get { return this._baseConnection; }
        }

        /// <summary>
        /// Gets underlying connection.
        /// </summary>
        protected System.Data.Common.DbProviderFactory ProviderFactory {
            get { return this._providerFactory; }
        }

        #endregion


        #region CreateParameter

        /// <summary>
        /// Creates and returns new instance of an System.Data.IDbDataParameter object.
        /// </summary>
        /// <param name="parameterName">The name of parameter to map.</param>
        /// <param name="value">Value of parameter.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is intended behaviour.")]
        internal IDbDataParameter CreateParameter(string parameterName, object value) {
            IDbDataParameter param = this._providerFactory.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            if (param.DbType == System.Data.DbType.DateTime) {
                System.Data.OleDb.OleDbParameter odp = param as System.Data.OleDb.OleDbParameter;
                if (odp != null) { odp.OleDbType = System.Data.OleDb.OleDbType.Date; }
            }
            return param;
        }

        /// <summary>
        /// Creates and returns new instance of an System.Data.IDbDataParameter object.
        /// </summary>
        /// <param name="parameterName">The name of parameter to map.</param>
        /// <param name="value">Value of parameter.</param>
        /// <param name="type">Type of parameter.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is intended behaviour.")]
        internal System.Data.IDbDataParameter CreateParameter(string parameterName, object value, System.Data.DbType type) {
            System.Data.IDbDataParameter param = this._providerFactory.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            param.DbType = type;
            if (param.DbType == System.Data.DbType.DateTime) {
                System.Data.OleDb.OleDbParameter odp = param as System.Data.OleDb.OleDbParameter;
                if (odp != null) { odp.OleDbType = System.Data.OleDb.OleDbType.Date; }
            }
            return param;
        }

        #endregion


        #region CreateCommand

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more objects to format. Those objects are inserted in Parameters as IDbDataParameter with name of @Px where x is order index.</param>
        /// <returns>A Command object associated with the connection.</returns>
        /// <example>
        /// IDbCommand cmd = CreateCommand("INSERT INTO TT([Text], [Date]) VALUES({0},{1})", "Test", DateTime.Now);
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Injection attack is not possible since all args[] are converted to IDbDataParameter.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="This is intended behaviour.")]
        internal IDbCommand CreateCommand(string format, params object[] args) {
            IDbCommand cmd = this.CreateCommand();

            List<string> argList = new List<string>();
            for (int i = 0; i < args.Length; ++i) {
                if (args[i] == null) {
                    argList.Add("NULL");
                } else {
                    string paramName = string.Format(CultureInfo.InvariantCulture, "@P{0}", i);
                    argList.Add(paramName);
                    IDbDataParameter param = cmd.CreateParameter();
                    param.ParameterName = paramName;
                    param.Value = args[i];
                    if (param.DbType == DbType.DateTime) {
                        OleDbParameter odp = param as OleDbParameter;
                        if (odp != null) { odp.OleDbType = OleDbType.Date; }
                    }
                    cmd.Parameters.Add(param);
                }
            }
            if (argList.Count > 0) {
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, format, argList.ToArray());
            } else {
                cmd.CommandText = format;
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine(cmd.CommandText + "I:     {{Medo.Data.CommonConnection}}");
			for (int i = 0; i < cmd.Parameters.Count; ++i) {
				DbParameter curr = cmd.Parameters[i] as DbParameter;
				if (curr != null) {
                    System.Diagnostics.Debug.WriteLine("I:     " + curr.ParameterName + "=" + "\"" + curr.Value.ToString() + "\"    {{Medo.Data.CommonConnection}}");
				} else {
                    System.Diagnostics.Debug.WriteLine("I:     " + cmd.Parameters[i].ToString() + "    {{Medo.Data.CommonConnection}}");
				}
			}
#endif
            return cmd;
        }

        #endregion


        #region IDbConnection Members

        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il) {
            return this.BeginTransaction(il);
        }

        IDbTransaction IDbConnection.BeginTransaction() {
            return this.BeginTransaction();
        }

        void IDbConnection.ChangeDatabase(string databaseName) {
            this.ChangeDatabase(databaseName);
        }

        void IDbConnection.Close() {
            this.Close();
        }

        string IDbConnection.ConnectionString {
            get { return this.ConnectionString; }
            set { this.ConnectionString = value; }
        }

        int IDbConnection.ConnectionTimeout {
            get { return this.ConnectionTimeout; }
        }

        IDbCommand IDbConnection.CreateCommand() {
            return this.CreateCommand();
        }

        string IDbConnection.Database {
            get { return this.Database; }
        }

        void IDbConnection.Open() {
            this.Open();
        }

        ConnectionState IDbConnection.State {
            get { return this.State; }
        }


        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <param name="il">One of the System.Data.IsolationLevel values.</param>
        /// <returns>An object representing the new transaction.</returns>
        internal protected IDbTransaction BeginTransaction(IsolationLevel il) {
            return this._baseConnection.BeginTransaction(il);
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>An object representing the new transaction.</returns>
        internal protected System.Data.IDbTransaction BeginTransaction() {
            return this._baseConnection.BeginTransaction();
        }


        /// <summary>
        /// Changes the current database for an open Connection object.
        /// </summary>
        /// <param name="databaseName">The name of the database to use in place of the current database.</param>
        internal protected void ChangeDatabase(string databaseName) {
            this._baseConnection.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        public void Close() {
            this._baseConnection.Close();
        }

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        /// <returns>A string containing connection settings.</returns>
        internal protected string ConnectionString {
            get { return this._baseConnection.ConnectionString; }
            set { this._baseConnection.ConnectionString = value; }
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        /// <returns>The time (in seconds) to wait for a connection to open. The default value is 15 seconds.</returns>
        internal protected int ConnectionTimeout {
            get { return this._baseConnection.ConnectionTimeout; }
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <returns>A Command object associated with the connection.</returns>
        internal protected IDbCommand CreateCommand() {
            return this._baseConnection.CreateCommand();
        }

        /// <summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <returns>The name of the current database or the name of the database to be used once a connection is open. The default value is an empty string.</returns>
        internal protected string Database {
            get { return this._baseConnection.Database; }
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString property of the provider-specific Connection object.
        /// </summary>
        public void Open() {
            try {
                this._baseConnection.Open();
            } catch {
                throw;
            }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <returns>One of the System.Data.ConnectionState values.</returns>
        internal protected ConnectionState State {
            get { return this._baseConnection.State; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this._baseConnection.Dispose();
            }
        }

        #endregion

    }

}
