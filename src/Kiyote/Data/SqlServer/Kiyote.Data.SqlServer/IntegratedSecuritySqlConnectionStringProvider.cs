namespace Kiyote.Data.SqlServer;

internal sealed class IntegratedSecuritySqlConnectionStringProvider<T> : ISqlConnectionStringProvider<T> where T : SqlServerContextOptions {

	private readonly IOptions<T> _options;
	private string _connectionString;
	private string _masterConnectionString;

	public IntegratedSecuritySqlConnectionStringProvider(
		IOptions<T> options
	) {
		_options = options;
		_connectionString = "";
		_masterConnectionString = "";
	}

	string ISqlConnectionStringProvider<T>.GetConnectionString() {
		if( string.IsNullOrWhiteSpace( _connectionString ) ) {
			T opts = _options.Value;
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {
				DataSource = opts.DataSource,
				IntegratedSecurity = true,
				InitialCatalog = opts.InitialCatalog,
				Encrypt = true,
				TrustServerCertificate = true
			};
			_connectionString = builder.ConnectionString;
		}

		return _connectionString;
	}

	Task<string> ISqlConnectionStringProvider<T>.GetConnectionStringAsync(
		CancellationToken cancellationToken
	) {
		return Task.FromResult( ( this as ISqlConnectionStringProvider<T> ).GetConnectionString() );
	}

	string ISqlConnectionStringProvider<T>.GetMasterConnectionString() {
		if( string.IsNullOrWhiteSpace( _connectionString ) ) {
			T opts = _options.Value;
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {
				DataSource = opts.DataSource,
				IntegratedSecurity = true,
				Encrypt = true,
				TrustServerCertificate = true
			};
			_masterConnectionString = builder.ConnectionString;
		}

		return _masterConnectionString;
	}

	Task<string> ISqlConnectionStringProvider<T>.GetMasterConnectionStringAsync(
		CancellationToken cancellationToken
	) {
		return Task.FromResult( ( this as ISqlConnectionStringProvider<T> ).GetMasterConnectionString() );
	}

	void ISqlConnectionStringProvider<T>.RefreshConnectionString() {
		_connectionString = "";
		_masterConnectionString = "";
	}

	Task ISqlConnectionStringProvider<T>.RefreshConnectionStringAsync(
		CancellationToken cancellationToken
	) {
		( this as ISqlConnectionStringProvider<T> ).RefreshConnectionString();
		return Task.CompletedTask;
	}
}

