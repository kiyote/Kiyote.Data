using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Data.SqlServer;

[ExcludeFromCodeCoverage( Justification = "Only performs object allocations." )]
internal sealed class SqlConnectionFactory : ISqlConnectionFactory {

	ISqlConnection ISqlConnectionFactory.Create(
		string connectionString
	) {
		return new SqlConnectionAdapter( new SqlConnection( connectionString ) );
	}
}
