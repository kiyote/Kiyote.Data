namespace Kiyote.Data.SqlServer;

internal sealed class IntegratedSecuritySqlConnectionStringProvider<T> : ISqlConnectionStringProvider<T> where T : SqlServerContextOptions {

	private readonly string _dataSource;
	private readonly string _initialCatalog;
	private string _connectionString;
	private string _masterConnectionString;

	public IntegratedSecuritySqlConnectionStringProvider(
		T options
	) {
		if( !string.IsNullOrWhiteSpace( options.ConnectionStringProvider )
			&& options.ConnectionStringProvider != SqlServerContextOptions.IntegratedSecurityConnectionStringProvider
		) {
			throw new ArgumentException( $"{options.ConnectionStringProvider} is not configured for this provider.", nameof( options ) );
		}
		_dataSource = options.DataSource ?? throw new ArgumentException( $"{options.DataSource} must not be empty.", nameof( options ) );
		_initialCatalog = options.InitialCatalog ?? throw new ArgumentException( $"{options.InitialCatalog} must not be empty.", nameof( options ) );
		_connectionString = "";
		_masterConnectionString = "";
	}

	string ISqlConnectionStringProvider<T>.GetConnectionString() {
		if( string.IsNullOrWhiteSpace( _connectionString ) ) {
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {
				DataSource = _dataSource,
				IntegratedSecurity = true,
				InitialCatalog = _initialCatalog,
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
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {
				DataSource = _dataSource,
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

