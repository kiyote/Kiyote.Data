using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Data.SqlServer;

[ExcludeFromCodeCoverage( Justification = "Pass-through adapter class." )]
internal sealed class SqlCommandAdapter : ISqlCommand {

	private readonly SqlCommand _command;
	private readonly SqlParameterCollectionAdapter _parameters;

	public SqlCommandAdapter(
		SqlCommand command
	) {
		_command = command;
		_command.EnableOptimizedParameterBinding = true;
		_parameters = new SqlParameterCollectionAdapter( _command.Parameters );
	}

#pragma warning disable CA2100
	string ISqlCommand.CommandText {
		get {
			return _command.CommandText;
		}
		set {
			_command.CommandText = value;
		}
	}
#pragma warning restore CA2100

	CommandType ISqlCommand.CommandType {
		get {
			return _command.CommandType;
		}
		set {
			_command.CommandType = value;
		}
	}

	ISqlParameterCollection ISqlCommand.Parameters {
		get {
			return _parameters;
		}
	}

	void IDisposable.Dispose() {
		_command.Dispose();
	}

	ValueTask IAsyncDisposable.DisposeAsync() {
		return _command.DisposeAsync();
	}

	int ISqlCommand.ExecuteNonQuery() {
		return _command.ExecuteNonQuery();
	}

	Task<int> ISqlCommand.ExecuteNonQueryAsync(
		CancellationToken cancellationToken
	) {
		return _command.ExecuteNonQueryAsync( cancellationToken );
	}

	async Task<ISqlDataReader> ISqlCommand.ExecuteReaderAsync(
		CancellationToken cancellationToken
	) {
		return new SqlDataReaderAdapter( await _command.ExecuteReaderAsync( cancellationToken ) );
	}

	ISqlDataReader ISqlCommand.ExecuteReader() {
		return new SqlDataReaderAdapter( _command.ExecuteReader() );
	}

	Task<object> ISqlCommand.ExecuteScalarAsync(
		CancellationToken cancellationToken
	) {
		return _command.ExecuteScalarAsync( cancellationToken );
	}

	TModel ISqlCommand.ExecuteScalar<TModel>() {
		object result = _command.ExecuteScalar();
		return (TModel)result;
	}

	async Task<TModel> ISqlCommand.ExecuteScalarAsync<TModel>(
		CancellationToken cancellationToken
	) {
		object result = await _command.ExecuteScalarAsync( cancellationToken );
		return (TModel)result;
	}
}
