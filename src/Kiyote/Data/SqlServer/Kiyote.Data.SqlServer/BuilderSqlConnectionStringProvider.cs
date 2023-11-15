namespace Kiyote.Data.SqlServer;

internal sealed class BuilderSqlConnectionStringProvider<T> : ISqlConnectionStringProvider<T> where T : SqlServerContextOptions {

	private readonly IOptions<T> _options;
	private string _connectionString;
	private string _masterConnectionString;

	public BuilderSqlConnectionStringProvider(
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
				UserID = opts.UserId,
				Password = opts.Password,
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
				UserID = opts.UserId,
				Password = opts.Password,
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

	Task ISqlConnectionStringProvider<T>.RefreshConnectionStringAsync(
		CancellationToken cancellationToken
	) {
		( this as ISqlConnectionStringProvider<T> ).RefreshConnectionString();
		return Task.CompletedTask;
	}

	void ISqlConnectionStringProvider<T>.RefreshConnectionString() {
		_connectionString = "";
		_masterConnectionString = "";
	}
}
