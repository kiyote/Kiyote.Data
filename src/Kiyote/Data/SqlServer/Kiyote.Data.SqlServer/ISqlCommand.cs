namespace Kiyote.Data.SqlServer;

public interface ISqlCommand : IDisposable, IAsyncDisposable {

	public string CommandText { get; set; }
	public CommandType CommandType { get; set; }

	public ISqlParameterCollection Parameters { get; }

	Task<ISqlDataReader> ExecuteReaderAsync(
		CancellationToken cancellationToken
	);

	ISqlDataReader ExecuteReader();

	Task<object> ExecuteScalarAsync(
		CancellationToken cancellationToken
	);

	int ExecuteNonQuery();

	Task<int> ExecuteNonQueryAsync(
		CancellationToken cancellationToken
	);

	TModel ExecuteScalar<TModel>();

	Task<TModel> ExecuteScalarAsync<TModel>(
		CancellationToken cancellationToken
	);

}
