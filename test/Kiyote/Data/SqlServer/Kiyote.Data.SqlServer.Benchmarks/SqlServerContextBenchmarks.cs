using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Data.SqlServer.Benchmarks;

[MemoryDiagnoser( true )]
public class SqlServerContextBenchmarks {

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

	[Benchmark]
	public async Task QueryAsync_1000() {
		string sql = "SELECT * FROM BENCHMARK1000";

		IAsyncEnumerable<int> result = _context!.QueryAsync(
			sql,
			Array.Empty<SqlParameter>(),
			Converter,
			DoQueryAsync,
			CancellationToken.None
		);
		List<int> values = new List<int>();
		await foreach( int id in result) {
			values.Add( id );
		}
	}

	[Benchmark]
	public async Task QueryAsync_10000() {
		string sql = "SELECT * FROM BENCHMARK10000";

		IAsyncEnumerable<int> result = _context!.QueryAsync(
			sql,
			Array.Empty<SqlParameter>(),
			Converter,
			DoQueryAsync,
			CancellationToken.None
		);
		List<int> values = new List<int>();
		await foreach( int id in result ) {
			values.Add( id );
		}
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
	public void Query_1000() {
		string sql = "SELECT * FROM BENCHMARK1000";

		IEnumerable<int> result = _context!.Query(
			sql,
			Array.Empty<SqlParameter>(),
			Converter,
			DoQuery
		).ToList();
	}

	[Benchmark]
	public void Query_10000() {
		string sql = "SELECT * FROM BENCHMARK10000";

		IEnumerable<int> result = _context!.Query(
			sql,
			Array.Empty<SqlParameter>(),
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

	/*
	[Benchmark]
	public void PerformAsync_NoDatabaseActivity_Delegate() {
		bool result = _context!.PerformAsync(
			( conn, token ) => {
				return Task.FromResult( true );
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_NoDatabaseActivity_Method() {
		bool result = _context!.PerformAsync(
			DoPerformAsync_NoDatabaseActivity_Method,
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	private static Task<bool> DoPerformAsync_NoDatabaseActivity_Method(
		ISqlConnection conn,
		CancellationToken token
	) {
		return Task.FromResult( true );
	}
	[ Benchmark]
	public void PerformAsync_Insert_AsyncReader_1000() {
		string sql = "INSERT INTO BENCHMARK1000 ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new Microsoft.Data.SqlClient.SqlParameter( "@ValueColumn", "VALUE" ) );
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( await reader.ReadAsync( token ) ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_Insert_AsyncReader_10000() {
		string sql = "INSERT INTO BENCHMARK10000 ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new Microsoft.Data.SqlClient.SqlParameter( "@ValueColumn", "VALUE" ) );
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( await reader.ReadAsync( token ) ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_Insert_SyncReader_1000() {
		string sql = "INSERT INTO BENCHMARK1000 ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new Microsoft.Data.SqlClient.SqlParameter( "@ValueColumn", "VALUE" ) );
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( reader.Read() ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_Insert_SyncReader_10000() {
		string sql = "INSERT INTO BENCHMARK10000 ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new Microsoft.Data.SqlClient.SqlParameter( "@ValueColumn", "VALUE" ) );
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( reader.Read() ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_Read_SyncReader_1000() {
		string sql = "SELECT * FROM BENCHMARK1000";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( reader.Read() ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_Read_SyncReader_10000() {
		string sql = "SELECT * FROM BENCHMARK10000";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( reader.Read() ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_Read_AsyncReader_1000() {
		string sql = "SELECT * FROM BENCHMARK1000";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( await reader.ReadAsync( token ) ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}

	[Benchmark]
	public void PerformAsync_Read_AsyncReader_10000() {
		string sql = "SELECT * FROM BENCHMARK10000";

		int result = _context!.PerformAsync(
			async ( conn, token ) => {
				await using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				await using ISqlDataReader reader = await command.ExecuteReaderAsync( token );
				int counter = 0;
				while( await reader.ReadAsync( token ) ) {
					_ = reader.GetInt32( 0 );
					counter++;
				}
				return counter;
			},
			CancellationToken.None
		).GetAwaiter().GetResult();
	}
	*/

	private static int Converter(
		ISqlDataReader reader
	) {
		return reader.GetInt32( 0 );
	}


	private void CreateDatabase() {
		_ = _context!.PerformMaster(
			$@"CREATE DATABASE [{_catalog}];",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();

			});

		_ = _context!.Perform(
			$@"CREATE TABLE BENCHMARK1000 ( KeyColumn int IDENTITY(1,1) NOT NULL, ValueColumn varchar(255) NOT NULL, CONSTRAINT PK_BENCHMARK1000 PRIMARY KEY CLUSTERED (KeyColumn) )",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();
			});

		_ = _context!.Perform(
			$@"CREATE TABLE BENCHMARK10000 ( KeyColumn int IDENTITY(1,1) NOT NULL, ValueColumn varchar(255) NOT NULL, CONSTRAINT PK_BENCHMARK10000 PRIMARY KEY CLUSTERED (KeyColumn) )",
			Array.Empty<SqlParameter>(),
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
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				_ = cmd.ExecuteNonQuery();
				return true;
			});
	}

	private void FillDatabase() {
		string sql = "INSERT INTO BENCHMARK1000 ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		for (int i = 0; i < 1000; i++) {
			int result = _context!.Perform(
				sql,
				new SqlParameter[] {
					new Microsoft.Data.SqlClient.SqlParameter( "@ValueColumn", "VALUE" )
				},
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

		sql = "INSERT INTO BENCHMARK10000 ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		for( int i = 0; i < 10000; i++ ) {
			int result = _context!.Perform(
				sql,
				new SqlParameter[] {
					new Microsoft.Data.SqlClient.SqlParameter( "@ValueColumn", "VALUE" )
				},
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
				});
		}
	}
}
