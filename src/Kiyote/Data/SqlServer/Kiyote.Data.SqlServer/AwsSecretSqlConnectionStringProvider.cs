using System.Text.Json;
using Amazon.SecretsManager.Model;
using Kiyote.AWS.SecretsManager;

namespace Kiyote.Data.SqlServer;

internal sealed class AwsSecretSqlConnectionStringProvider<T> : ISqlConnectionStringProvider<T> where T : SqlServerContextOptions {

	private sealed record ConnectionStringSecret( string ConnectionString );

	private readonly IAmazonSecretsManager<T> _secretsManager;
	private readonly string _connectionStringSecretName;
	private string _connectionString;
	private string _masterConnectionString;

	public AwsSecretSqlConnectionStringProvider(
		IAmazonSecretsManager<T> secretsManager,
		T options
	) {
		_secretsManager = secretsManager;
		if( !string.IsNullOrWhiteSpace( options.ConnectionStringProvider )
			&& options.ConnectionStringProvider != SqlServerContextOptions.AwsSecretConnectionStringProvider
		) {
			throw new ArgumentException( $"{options.ConnectionStringProvider} is not configured for this provider.", nameof( options ) );
		}
		_connectionStringSecretName = options.ConnectionStringSecretName ?? "";
		_connectionString = "";
		_masterConnectionString = "";
	}

	async Task<string> ISqlConnectionStringProvider<T>.GetConnectionStringAsync(
		CancellationToken cancellationToken
	) {
		if( string.IsNullOrWhiteSpace( _connectionString ) ) {

			GetSecretValueRequest request = new GetSecretValueRequest {
				SecretId = _connectionStringSecretName
			};
			GetSecretValueResponse response = await _secretsManager
				.GetSecretValueAsync( request, cancellationToken );
			if( string.IsNullOrWhiteSpace( response.SecretString ) ) {
				throw new InvalidOperationException();
			}
			// Deserialize cannot return null as the SecretString property requires
			// a minimum length string, which ends up creating a ConnectionSecretString
			// but with nothing in it
			ConnectionStringSecret conn = JsonSerializer
				.Deserialize<ConnectionStringSecret>(
					response.SecretString
				)!;

			if( string.IsNullOrWhiteSpace( conn.ConnectionString ) ) {
				throw new InvalidOperationException();
			}

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder( conn.ConnectionString ) {
				Encrypt = true,
				TrustServerCertificate = true
			};

			_connectionString = builder.ConnectionString;
		}
		return _connectionString;
	}

	async Task<string> ISqlConnectionStringProvider<T>.GetMasterConnectionStringAsync(
		CancellationToken cancellationToken
	) {
		if( string.IsNullOrWhiteSpace( _masterConnectionString ) ) {
			_ = await ( this as ISqlConnectionStringProvider<T> ).GetConnectionStringAsync( cancellationToken );

			int catalogStart = _connectionString.IndexOf( "Initial Catalog", StringComparison.OrdinalIgnoreCase );
			int catalogEnd = _connectionString.IndexOf( ';', catalogStart );
			_masterConnectionString = _connectionString.Remove( catalogStart, ( catalogEnd - catalogStart + 1 ) );
		}

		return _masterConnectionString;
	}

	Task ISqlConnectionStringProvider<T>.RefreshConnectionStringAsync(
		CancellationToken cancellationToken
	) {
		( this as ISqlConnectionStringProvider<T> ).RefreshConnectionString();
		return Task.CompletedTask;
	}

	string ISqlConnectionStringProvider<T>.GetConnectionString() {
		return ( this as ISqlConnectionStringProvider<T> )
			.GetConnectionStringAsync( CancellationToken.None )
			.ConfigureAwait( false )
			.GetAwaiter()
			.GetResult();
	}

	string ISqlConnectionStringProvider<T>.GetMasterConnectionString() {
		return ( this as ISqlConnectionStringProvider<T> )
			.GetMasterConnectionStringAsync( CancellationToken.None )
			.ConfigureAwait( false )
			.GetAwaiter()
			.GetResult();
	}

	void ISqlConnectionStringProvider<T>.RefreshConnectionString() {
		_connectionString = "";
		_masterConnectionString = "";
	}
}
