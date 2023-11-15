namespace Kiyote.Data.SqlServer;


public interface ISqlServerContext<T> where T : SqlServerContextOptions {

	IEnumerable<TModel> Query<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlDataReader, TModel> converter,
		Func<ISqlConnection, Func<ISqlDataReader, TModel>, string, SqlParameter[], IEnumerable<TModel>> action
	);

	IAsyncEnumerable<TModel> QueryAsync<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlDataReader, TModel> converter,
		Func<ISqlConnection, Func<ISqlDataReader, TModel>, string, SqlParameter[], CancellationToken, IAsyncEnumerable<TModel>> action,
		CancellationToken cancellationToken
	);

	TModel Perform<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], TModel> action
	);

	Task<TModel> PerformAsync<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<TModel>> action,
		CancellationToken cancellationToken
	);

	TModel PerformMaster<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], TModel> action
	);

	Task<TModel> PerformMasterAsync<TModel>(
		string sql,
		SqlParameter[] parameters,
		Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<TModel>> action,
		CancellationToken cancellationToken
	);
}
