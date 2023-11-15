namespace Kiyote.Data.SqlServer;

public interface ISqlConnectionFactory {
	ISqlConnection Create(
		string connectionString
	);

}
