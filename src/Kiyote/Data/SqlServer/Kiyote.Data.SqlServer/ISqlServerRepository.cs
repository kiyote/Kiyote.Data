namespace Kiyote.Data.SqlServer;

public interface ISqlServerRepository<T> where T : SqlServerContextOptions {

	IEnumerable<TModel> Query<TModel>(
		string sql,
		Func<ISqlDataReader, TModel> converter,
		params SqlParameter[] parameters
	);

	IAsyncEnumerable<TModel> QueryAsync<TModel>(
		string sql,
		Func<ISqlDataReader, TModel> converter,
		CancellationToken cancellationToken,
		params SqlParameter[] parameters
	);

	int ExecuteNonQuery(
		string sql,
		params SqlParameter[] parameters
	);

	Task<int> ExecuteNonQueryAsync(
		string sql,
		CancellationToken cancellationToken,
		params SqlParameter[] parameters
	);

	TModel ExecuteScalar<TModel>(
		string sql,
		params SqlParameter[] parameters
	);

	Task<TModel> ExecuteScalarAsync<TModel>(
		string sql,
		CancellationToken cancellationToken,
		params SqlParameter[] parameters
	);
}
