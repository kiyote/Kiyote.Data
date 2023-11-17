using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Data.SqlServer.Benchmarks;

[MemoryDiagnoser( true )]
public class SqlServerContextBenchmarks {

	private const string TableName = "QUERY_BENCHMARK";
	private AsyncServiceScope _scope;
	private ISqlServerContext<BenchmarkSqlServerContextOptions>? _context;
	private string? _catalog;

	[GlobalSetup]
	public void GlobalSetup() {
		_catalog = Guid.NewGuid().ToString( "N" );
		IServiceCollection serviceCollection = new ServiceCollection();
		_ = serviceCollection.AddOptions();
		_ = serviceCollection.Configure<BenchmarkSqlServerContextOptions>( ( opts ) => {
			opts.ConnectionStringProvider = SqlServerContextOptions.IntegratedSecurityConnectionStringProvider;
			opts.DataSource = "localhost";
			opts.InitialCatalog = _catalog;
		}
		);
		_ = serviceCollection.AddIntegratedSecuritySqlServer<BenchmarkSqlServerContextOptions>();
		IServiceProvider services = serviceCollection.BuildServiceProvider();

		_scope = services.CreateAsyncScope();
		_context = services.GetRequiredService<ISqlServerContext<BenchmarkSqlServerContextOptions>>();
		CreateDatabase();
		FillDatabase();
	}

	[GlobalCleanup]
	public void GlobalCleanup() {
		DeleteDatabase();
		_scope.Dispose();
	}

	[Params( 100, 1000, 10000 )]
	public int RowCount { get; set; }

	[Benchmark]
	public async Task QueryAsync() {
		string sql = $"SELECT * FROM {TableName}";

		IAsyncEnumerable<int> result = _context!.QueryAsync(
			sql,
			[],
			Converter,
			DoQueryAsync,
			CancellationToken.None
		);
#pragma warning disable IDE0028 // Simplify collection initialization
// Disabled because their autofix does not work with IAsyncEnumerable
		List<int> values = new List<int>();
		await foreach( int id in result ) {
			values.Add( id );
		}
#pragma warning restore IDE0028 // Simplify collection initialization
	}

	private static async IAsyncEnumerable<int> DoQueryAsync(
		ISqlConnection conn,
		Func<ISqlDataReader, int> converter,
		string sql,
		SqlParameter[] parameters,
		[EnumeratorCancellation]
		CancellationToken cancellationToken
	) {
		using ISqlCommand command = conn.CreateCommand();
		command.CommandType = System.Data.CommandType.Text;
		command.CommandText = sql;
		await using ISqlDataReader reader = await command.ExecuteReaderAsync( cancellationToken );
		while( await reader.ReadAsync( cancellationToken ) ) {
			yield return converter( reader );
		}
	}

	[Benchmark]
	public void Query() {
		string sql = $"SELECT * FROM {TableName}";

		IEnumerable<int> result = _context!.Query(
			sql,
			[],
			Converter,
			DoQuery
		).ToList();
	}

	private IEnumerable<int> DoQuery(
		ISqlConnection conn,
		Func<ISqlDataReader, int> converter,
		string sql,
		SqlParameter[] parameters
	) {
		using ISqlCommand command = conn.CreateCommand();
		command.CommandType = System.Data.CommandType.Text;
		command.CommandText = sql;
		using ISqlDataReader reader = command.ExecuteReader();
		while( reader.Read() ) {
			yield return converter( reader );
		}
	}

	[Benchmark]
	public void Perform() {
		string sql = $"SELECT KeyColumn FROM {TableName} WHERE KeyColumn = @Id";

		int result = _context!.Perform(
			sql,
			[
				new SqlParameter( "@Id", RowCount >> 1 )
			],
			DoPerform
		);
	}

	private int DoPerform(
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

	[Benchmark]
	public void PerformAsync() {
		string sql = $"SELECT KeyColumn FROM {TableName} WHERE KeyColumn = @Id";

		int result = _context!.Perform(
			sql,
			[
				new SqlParameter( "@Id", RowCount >> 1 )
			],
			DoPerformAsync
		);
	}

	private int DoPerformAsync(
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

	private static int Converter(
		ISqlDataReader reader
	) {
		return reader.GetInt32( 0 );
	}


	private void CreateDatabase() {
		_ = _context!.PerformMaster(
			$@"CREATE DATABASE [{_catalog}];",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();

			});

		_ = _context!.Perform(
			$@"CREATE TABLE {TableName} ( KeyColumn int IDENTITY(1,1) NOT NULL, ValueColumn varchar(255) NOT NULL, CONSTRAINT PK_{TableName} PRIMARY KEY CLUSTERED (KeyColumn) )",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();
			});
	}

	public void DeleteDatabase() {
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
			});
	}

	private void FillDatabase() {
		string sql = $"INSERT INTO {TableName} ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		for (int i = 0; i < RowCount; i++) {
			int result = _context!.Perform(
				sql,
				[
					new SqlParameter( "@ValueColumn", "VALUE" )
				],
				( conn, sql, parameters ) => {
					using ISqlCommand command = conn.CreateCommand();
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = sql;
					foreach (SqlParameter parameter in parameters) {
						command.Parameters.Add( parameter );
					}
					using ISqlDataReader reader = command.ExecuteReader();
					if( reader.Read() ) {
						return reader.GetInt32( 0 );
					}
					throw new InvalidOperationException();
				});
		}
	}
}
