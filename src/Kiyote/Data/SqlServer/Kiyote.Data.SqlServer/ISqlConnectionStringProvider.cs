namespace Kiyote.Data.SqlServer;

public interface ISqlConnectionStringProvider<T> where T : SqlServerContextOptions {

	string GetConnectionString();

	string GetMasterConnectionString();

	void RefreshConnectionString();

	Task<string> GetConnectionStringAsync( CancellationToken cancellationToken );

	Task<string> GetMasterConnectionStringAsync( CancellationToken cancellationToken );

	Task RefreshConnectionStringAsync( CancellationToken cancellationToken );

}
