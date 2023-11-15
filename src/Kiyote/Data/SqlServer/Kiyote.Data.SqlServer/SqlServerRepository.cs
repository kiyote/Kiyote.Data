using System.Runtime.CompilerServices;

namespace Kiyote.Data.SqlServer;

internal sealed class SqlServerRepository<T> : ISqlServerRepository<T> where T : SqlServerContextOptions {

	private readonly ISqlServerContext<T> _context;

	public SqlServerRepository(
		ISqlServerContext<T> context
	) {
		_context = context;
	}

	IEnumerable<TModel> ISqlServerRepository<T>.Query<TModel>(
		string sql,
		Func<ISqlDataReader, TModel> converter,
		params SqlParameter[] parameters
	) {
		return _context.Query(
			sql,
			parameters,
			converter,
			DoQuery
		);
	}

	private static IEnumerable<TModel> DoQuery<TModel>(
		ISqlConnection connection,
		Func<ISqlDataReader, TModel> converter,
		string sql,
		SqlParameter[] parameters
	) {
		using ISqlCommand command = PrepareCommand( connection, sql, parameters );
		using ISqlDataReader reader = command.ExecuteReader();
		while( reader.Read() ) {
			TModel item = converter( reader );
			yield return item;
		}
	}

	IAsyncEnumerable<TModel> ISqlServerRepository<T>.QueryAsync<TModel>(
		string sql,
		Func<ISqlDataReader, TModel> converter,
		CancellationToken cancellationToken,
		params SqlParameter[] parameters
	) {
		return _context.QueryAsync(
			sql,
			parameters,
			converter,
			DoQueryAsync,
			cancellationToken
		);
	}

	private static async IAsyncEnumerable<TModel> DoQueryAsync<TModel>(
		ISqlConnection connection,
		Func<ISqlDataReader, TModel> converter,
		string sql,
		SqlParameter[] parameters,
		[EnumeratorCancellation]
		CancellationToken cancellationToken
	) {
		await using ISqlCommand command = PrepareCommand( connection, sql, parameters );
		await using ISqlDataReader reader = await command.ExecuteReaderAsync( cancellationToken );
		while( await reader.ReadAsync( cancellationToken ) ) {
			TModel item = converter( reader );
			yield return item;
		}
	}

	int ISqlServerRepository<T>.ExecuteNonQuery(
		string sql,
		params SqlParameter[] parameters
	) {
		return _context.Perform(
			sql,
			parameters,
			DoExecuteNonQuery
		);
	}

	private static int DoExecuteNonQuery(
		ISqlConnection connection,
		string sql,
		SqlParameter[] parameters
	) {
		using ISqlCommand command = PrepareCommand( connection, sql, parameters );
		return command.ExecuteNonQuery();
	}

	Task<int> ISqlServerRepository<T>.ExecuteNonQueryAsync(
		string sql,
		CancellationToken cancellationToken,
		params SqlParameter[] parameters
	) {
		return _context.PerformAsync(
			sql,
			parameters,
			DoExecuteNonQueryAsync,
			cancellationToken
		);
	}

	private static async Task<int> DoExecuteNonQueryAsync(
		ISqlConnection connection,
		string sql,
		SqlParameter[] parameters,
		CancellationToken cancellationToken
	) {
		await using ISqlCommand command = PrepareCommand( connection, sql, parameters );
		return await command.ExecuteNonQueryAsync( cancellationToken );
	}

	TModel ISqlServerRepository<T>.ExecuteScalar<TModel>(
		string sql,
		params SqlParameter[] parameters
	) {
		return _context.Perform(
			sql,
			parameters,
			DoExecuteScalar<TModel>
		);
	}

	private static TModel DoExecuteScalar<TModel>(
		ISqlConnection connection,
		string sql,
		SqlParameter[] parameters
	) {
		using ISqlCommand command = PrepareCommand( connection, sql, parameters );
		return command.ExecuteScalar<TModel>();
	}

	Task<TModel> ISqlServerRepository<T>.ExecuteScalarAsync<TModel>(
		string sql,
		CancellationToken cancellationToken,
		params SqlParameter[] parameters
	) {
		return _context.PerformAsync(
			sql,
			parameters,
			DoExecuteScalarAsync<TModel>,
			cancellationToken
		);
	}

	private static async Task<TModel> DoExecuteScalarAsync<TModel>(
		ISqlConnection connection,
		string sql,
		SqlParameter[] parameters,
		CancellationToken cancellationToken
	) {
		await using ISqlCommand command = PrepareCommand( connection, sql, parameters );
		return await command.ExecuteScalarAsync<TModel>( cancellationToken );
	}

	private static ISqlCommand PrepareCommand(
		ISqlConnection connection,
		string sql,
		SqlParameter[] parameters
	) {
		ISqlCommand command = connection.CreateCommand();
		command.CommandType = CommandType.Text;
		command.CommandText = sql;
		foreach( SqlParameter parameter in parameters ) {
			command.Parameters.Add( parameter );
		}
		return command;
	}

}
