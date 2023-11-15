using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Data.SqlServer;

[ExcludeFromCodeCoverage( Justification = "Pass-through adapter class." )]
internal sealed class SqlConnectionAdapter : ISqlConnection {

	private readonly SqlConnection _connection;

	public SqlConnectionAdapter(
		SqlConnection connection
	) {
		_connection = connection;
	}

	void ISqlConnection.Close() {
		_connection.Close();
	}

	Task ISqlConnection.CloseAsync() {
		return _connection.CloseAsync();
	}

	ISqlCommand ISqlConnection.CreateCommand() {
		return new SqlCommandAdapter( _connection.CreateCommand() );
	}

	void IDisposable.Dispose() {
		_connection.Dispose();
	}

	ValueTask IAsyncDisposable.DisposeAsync() {
		return _connection.DisposeAsync();
	}

	void ISqlConnection.Open() {
		_connection.Open();
	}

	Task ISqlConnection.OpenAsync(
		CancellationToken cancellationToken
	) {
		return _connection.OpenAsync( cancellationToken );
	}
}
