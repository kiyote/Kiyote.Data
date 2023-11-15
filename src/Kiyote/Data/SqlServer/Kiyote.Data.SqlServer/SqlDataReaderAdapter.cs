using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Data.SqlServer;

[ExcludeFromCodeCoverage( Justification = "Pass-through adapter class." )]
internal sealed class SqlDataReaderAdapter : ISqlDataReader {

	private readonly SqlDataReader _reader;

	public SqlDataReaderAdapter(
		SqlDataReader reader
	) {
		_reader = reader;
	}

	void IDisposable.Dispose() {
		_reader.Dispose();
	}

	ValueTask IAsyncDisposable.DisposeAsync() {
		return _reader.DisposeAsync();
	}

	bool ISqlDataReader.GetBoolean(
		int column
	) {
		return _reader.GetBoolean( column );
	}

	DateTime ISqlDataReader.GetDateTime(
		int column
	) {
		return _reader.GetDateTime( column );
	}

	double ISqlDataReader.GetDouble(
		int column
	) {
		return _reader.GetDouble( column );
	}

	int ISqlDataReader.GetInt32(
		int column
	) {
		return _reader.GetInt32( column );
	}

	string ISqlDataReader.GetString(
		int column
	) {
		return _reader.GetString( column );
	}

	bool ISqlDataReader.IsDBNull(
		int column
	) {
		return _reader.IsDBNull( column );
	}

	bool ISqlDataReader.Read() {
		return _reader.Read();
	}

	Task<bool> ISqlDataReader.ReadAsync(
		CancellationToken cancellationToken
	) {
		return _reader.ReadAsync( cancellationToken );
	}
}
