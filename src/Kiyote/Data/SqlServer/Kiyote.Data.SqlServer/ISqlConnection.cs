namespace Kiyote.Data.SqlServer;

public interface ISqlConnection : IDisposable, IAsyncDisposable {

	void Open();

	Task OpenAsync(
		CancellationToken cancellationToken
	);

	void Close();

	Task CloseAsync();

	ISqlCommand CreateCommand();
}
