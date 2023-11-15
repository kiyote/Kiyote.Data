using System.Runtime.CompilerServices;

namespace Kiyote.Data.SqlServer;

internal sealed class SqlServerContext<T> : ISqlServerContext<T> where T : SqlServerContextOptions {

	private readonly ISqlConnectionStringProvider<T> _connectionStringProvider;
	private readonly ISqlConnectionFactory _sqlConnectionFactory;

	public SqlServerContext(
		ISqlConnectionStringProvider<T> connectionStringProvider,
		ISqlConnectionFactory sqlConnectionFactory
	) {
		_connectionStringProvider = connectionStringProvider;
		_sqlConnectionFactory = sqlConnectionFactory;
	}

	TModel ISqlServerContext<T>.Perform<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], TModel> action
	) {
		string connectionString = _connectionStringProvider.GetConnectionString();
		using ISqlConnection connection = _sqlConnectionFactory.Create( connectionString );
		try {
			connection.Open();
		} catch( Exception ) {
			_connectionStringProvider.RefreshConnectionString();
			throw;
		}
		TModel result = action.Invoke( connection, sql, parameters );
		connection.Close();
		return result;
	}

	async Task<TModel> ISqlServerContext<T>.PerformAsync<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<TModel>> action,
		CancellationToken cancellationToken
	) {
		string connectionString = await _connectionStringProvider.GetConnectionStringAsync( cancellationToken );
		await using ISqlConnection connection = _sqlConnectionFactory.Create( connectionString );
		try {
			await connection.OpenAsync( cancellationToken );
		} catch( Exception ) {
			await _connectionStringProvider.RefreshConnectionStringAsync( cancellationToken );
			throw;
		}
		TModel result = await action.Invoke( connection, sql, parameters, cancellationToken );
		await connection.CloseAsync();
		return result;
	}

	TModel ISqlServerContext<T>.PerformMaster<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], TModel> action
	) {
		string connectionString = _connectionStringProvider.GetMasterConnectionString();
		using ISqlConnection connection = _sqlConnectionFactory.Create( connectionString );
		try {
			connection.Open();
		} catch( Exception ) {
			_connectionStringProvider.RefreshConnectionString();
			throw;
		}
		TModel result = action.Invoke( connection, sql, parameters );
		connection.Close();
		return result;
	}

	async Task<TModel> ISqlServerContext<T>.PerformMasterAsync<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<TModel>> action,
		CancellationToken cancellationToken
	) {
		string connectionString = await _connectionStringProvider.GetMasterConnectionStringAsync( cancellationToken );
		await using ISqlConnection connection = _sqlConnectionFactory.Create( connectionString );
		try {
			await connection.OpenAsync( cancellationToken );
		} catch( Exception ) {
			await _connectionStringProvider.RefreshConnectionStringAsync( cancellationToken );
			throw;
		}
		TModel result = await action.Invoke( connection, sql, parameters, cancellationToken );
		await connection.CloseAsync();
		return result;
	}

	IEnumerable<TModel> ISqlServerContext<T>.Query<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlDataReader, TModel> converter,
		Func<ISqlConnection, Func<ISqlDataReader, TModel>, string, SqlParameter[], IEnumerable<TModel>> action
	) {
		string connectionString = _connectionStringProvider.GetConnectionString();
		using ISqlConnection connection = _sqlConnectionFactory.Create( connectionString );
		try {
			connection.Open();
		} catch( Exception ) {
			_connectionStringProvider.RefreshConnectionString();
			throw;
		}
		IEnumerable<TModel> items = action.Invoke( connection, converter, sql, parameters );
		foreach( TModel item in items ) {
			yield return item;
		}
		connection.Close();
	}

	async IAsyncEnumerable<TModel> ISqlServerContext<T>.QueryAsync<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlDataReader, TModel> converter,
		Func<ISqlConnection, Func<ISqlDataReader, TModel>, string, SqlParameter[], CancellationToken, IAsyncEnumerable<TModel>> action,
		[EnumeratorCancellation]
		CancellationToken cancellationToken
	) {
		string connectionString = await _connectionStringProvider.GetConnectionStringAsync( cancellationToken );
		await using ISqlConnection connection = _sqlConnectionFactory.Create( connectionString );
		try {
			await connection.OpenAsync( cancellationToken );
		} catch( Exception ) {
			await _connectionStringProvider.RefreshConnectionStringAsync( cancellationToken );
			throw;
		}
		IAsyncEnumerable<TModel> items = action.Invoke( connection, converter, sql, parameters, cancellationToken );
		await foreach( TModel item in items ) {
			yield return item;
		}
		await connection.CloseAsync();
	}
}
