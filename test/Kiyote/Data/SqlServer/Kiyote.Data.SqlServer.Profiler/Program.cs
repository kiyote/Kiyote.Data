using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Data.SqlServer.Profiler;

public static class Program {

	public const string TableName = "PROFILER";
	public const int RowCount = 10000;
	public const int IterationCount = 100000;
	private static string? _catalog;
	private static ISqlServerContext<ProfilerSqlServerContextOptions>? _context;
	private static IServiceScope? _scope;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
	public static void Main(
		string[] __
	) {
		Console.WriteLine( "Initializing." );
		_catalog = Guid.NewGuid().ToString( "N" );
		IServiceCollection serviceCollection = new ServiceCollection();
		_ = serviceCollection.AddOptions();
		_ = serviceCollection.Configure<ProfilerSqlServerContextOptions>(
			( opts ) => {
				opts.ConnectionStringProvider = SqlServerContextOptions.IntegratedSecurityConnectionStringProvider;
				opts.DataSource = "localhost";
				opts.InitialCatalog = _catalog;
			}
		);
		_ = serviceCollection.AddIntegratedSecuritySqlServer<ProfilerSqlServerContextOptions>();
		IServiceProvider services = serviceCollection.BuildServiceProvider();

		_scope = services.CreateAsyncScope();
		_context = _scope.ServiceProvider.GetRequiredService<ISqlServerContext<ProfilerSqlServerContextOptions>>();
		CreateDatabase();
		FillDatabase();

		string sql = $"SELECT KeyColumn FROM {TableName} WHERE KeyColumn = @Id";

		Console.Write( "Start collection and press any key..." );
		_ = Console.ReadKey();
		Console.WriteLine();
		Console.WriteLine( "Executing." );

		for( int i = 0; i < IterationCount; i++ ) {

			int result = _context!.Perform(
				sql,
				[
					new SqlParameter( "@Id", RowCount >> 1 )
				],
				DoPerform
			);

		}
		Console.Write( "Stop collection and press any key..." );
		_ = Console.ReadKey();
		Console.WriteLine();
		Console.WriteLine( "Finalizing." );
		DeleteDatabase();
	}
#pragma warning restore CA1303 // Do not pass literals as localized parameters

	private static int DoPerform(
		ISqlConnection conn,
		string sql,
		SqlParameter[] parameters
	) {
		using ISqlCommand command = conn.CreateCommand();
		command.CommandType = System.Data.CommandType.Text;
		command.CommandText = sql;
		foreach( SqlParameter parameter in parameters ) {
			command.Parameters.Add( parameter );
		}
		return command.ExecuteScalar<int>();
	}

	private static void CreateDatabase() {
		_ = _context!.PerformMaster(
			$@"CREATE DATABASE [{_catalog}];",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();

			} );

		_ = _context!.Perform(
			$@"CREATE TABLE {TableName} ( KeyColumn int IDENTITY(1,1) NOT NULL, ValueColumn varchar(255) NOT NULL, CONSTRAINT PK_{TableName} PRIMARY KEY CLUSTERED (KeyColumn) )",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();
			} );
	}

	public static void DeleteDatabase() {
		_ = _context!.PerformMaster(
			$@"
				ALTER DATABASE [{_catalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
				DROP DATABASE [{_catalog}];
			",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				_ = cmd.ExecuteNonQuery();
				return true;
			} );
	}

	private static void FillDatabase() {
		string sql = $"INSERT INTO {TableName} ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		for( int i = 0; i < RowCount; i++ ) {
			int result = _context!.Perform(
				sql,
				[
					new SqlParameter( "@ValueColumn", "VALUE" )
				],
				( conn, sql, parameters ) => {
					using ISqlCommand command = conn.CreateCommand();
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = sql;
					foreach( SqlParameter parameter in parameters ) {
						command.Parameters.Add( parameter );
					}
					using ISqlDataReader reader = command.ExecuteReader();
					if( reader.Read() ) {
						return reader.GetInt32( 0 );
					}
					throw new InvalidOperationException();
				} );
		}
	}
}
