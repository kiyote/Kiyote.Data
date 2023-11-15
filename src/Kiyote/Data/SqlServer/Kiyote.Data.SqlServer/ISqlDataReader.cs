namespace Kiyote.Data.SqlServer;

public interface ISqlDataReader : IDisposable, IAsyncDisposable {

	Task<bool> ReadAsync(
		CancellationToken cancellationToken
	);

	bool Read();

	int GetInt32(
		int column
	);

	string GetString(
		int column
	);

	bool GetBoolean(
		int column
	);

	double GetDouble(
		int column
	);

	DateTime GetDateTime(
		int column
	);

	bool IsDBNull(
		int column
	);
}
