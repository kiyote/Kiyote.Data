using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Data.SqlServer;

[ExcludeFromCodeCoverage( Justification = "Pass-through adapter class." )]
internal sealed class SqlParameterCollectionAdapter : ISqlParameterCollection {

	private readonly SqlParameterCollection _parameters;

	public SqlParameterCollectionAdapter(
		SqlParameterCollection parameters
	) {
		_parameters = parameters;
	}

	void ISqlParameterCollection.Add( SqlParameter parameter ) {
		_ = _parameters.Add( parameter );
	}
}
