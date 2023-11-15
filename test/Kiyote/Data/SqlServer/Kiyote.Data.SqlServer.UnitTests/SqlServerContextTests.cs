using Microsoft.Data.SqlClient;

namespace Kiyote.Data.SqlServer.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class SqlServerContextTests {

	private Mock<ISqlConnectionStringProvider<TestSqlServerContextOptions>> _connectionStringProvider;
	private Mock<ISqlConnectionFactory> _connectionFactory;
	private Mock<ISqlConnection> _connection;
	private ISqlServerContext<TestSqlServerContextOptions> _context;

	[SetUp]
	public void Setup() {
		_connection = new Mock<ISqlConnection>( MockBehavior.Strict );
		_connectionFactory = new Mock<ISqlConnectionFactory>( MockBehavior.Strict );
		_connectionStringProvider = new Mock<ISqlConnectionStringProvider<TestSqlServerContextOptions>>( MockBehavior.Strict );
		_context = new SqlServerContext<TestSqlServerContextOptions>(
			_connectionStringProvider.Object,
			_connectionFactory.Object
		);
	}

	[TearDown]
	public void TearDown() {
		_connection.VerifyAll();
		_connectionFactory.VerifyAll();
		_connectionStringProvider.VerifyAll();
	}

	[Test]
	public async Task QueryAsync_ValidConnection_ValueReturned() {
		int expected = 1;

		SetupValidConnectionAsync();

		IAsyncEnumerable<int> result = _context.QueryAsync(
			"",
			Array.Empty<SqlParameter>(),
			Converter,
			( conn, converter, sql, parameters, token ) => {
				return new int[] { expected }.ToAsyncEnumerable();
			},
			CancellationToken.None
		);

		// If you don't iterate over the entire list the connection isn't closed
		// by the time we check the expectations in TearDown
		int actual = (await result.ToListAsync()).First();
		Assert.AreEqual( expected, actual );
	}

	[Test]
	public void Query_ValidConnection_ValueReturned() {
		int expected = 1;

		SetupValidConnection();

		IEnumerable<int> result = _context.Query(
			"",
			Array.Empty<SqlParameter>(),
			Converter,
			( conn, converter, sql, parameters ) => {
				return new int[] { expected };
			}
		);

		// If you don't iterate over the entire list the connection isn't closed
		// by the time we check the expectations in TearDown
		int actual = result.ToList().First();
		Assert.AreEqual( expected, actual );
	}

	[Test]
	public async Task PerformAsync_ValidConnection_ValueReturned() {
		int expected = 1;

		SetupValidConnectionAsync();

		int actual = await _context.PerformAsync(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters, token ) => {
				return Task.FromResult( expected );
			},
			CancellationToken.None
		);

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public void Perform_ValidConnection_ValueReturned() {
		int expected = 1;

		SetupValidConnection();

		int actual = _context.Perform(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters ) => {
				return expected;
			}
		);

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public async Task PerformMasterAsync_ValidConnection_ValueReturned() {
		int expected = 1;

		SetupValidConnectionAsync( true );

		int actual = await _context.PerformMasterAsync(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters, token ) => {
				return Task.FromResult( expected );
			},
			CancellationToken.None
		);

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public void PerformMaster_ValidConnection_ValueReturned() {
		int expected = 1;

		SetupValidConnection( true );

		int actual = _context.PerformMaster(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters ) => {
				return expected;
			}
		);

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public void PerformAsync_InvalidConnection_ConnectionStringRefreshed() {
		int expected = 1;
		string connectionString = "invalid_string";

		_ = _connectionStringProvider
			.Setup( csp => csp.GetConnectionStringAsync( It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( connectionString );

		_ = _connectionStringProvider
			.Setup( csp => csp.RefreshConnectionStringAsync( It.IsAny<CancellationToken>() ) )
			.Returns( Task.CompletedTask );

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _connection
			.Setup( c => c.OpenAsync( It.IsAny<CancellationToken>() ) )
			.Throws( new InvalidOperationException() );

		_ = Assert.ThrowsAsync<InvalidOperationException>( async () => await _context.PerformAsync(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters, token ) => {
				return Task.FromResult( expected );
			},
			CancellationToken.None
		) );
	}

	[Test]
	public void QueryAsync_InvalidConnection_ConnectionStringRefreshed() {
		int expected = 1;
		string connectionString = "invalid_string";

		_ = _connectionStringProvider
			.Setup( csp => csp.GetConnectionStringAsync( It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( connectionString );

		_ = _connectionStringProvider
			.Setup( csp => csp.RefreshConnectionStringAsync( It.IsAny<CancellationToken>() ) )
			.Returns( Task.CompletedTask );

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _connection
			.Setup( c => c.OpenAsync( It.IsAny<CancellationToken>() ) )
			.Throws( new InvalidOperationException() );

		IAsyncEnumerable<int> result = _context.QueryAsync(
			"",
			Array.Empty<SqlParameter>(),
			Converter,
			( conn, converter, sql, parameters, token ) => {
				return new int[] { expected }.ToAsyncEnumerable();
			},
			CancellationToken.None
		);

		_ = Assert.ThrowsAsync<InvalidOperationException>( async () => await result.ToListAsync() );
	}

	[Test]
	public void Query_InvalidConnection_ConnectionStringRefreshed() {
		int expected = 1;
		string connectionString = "invalid_string";

		_ = _connectionStringProvider
			.Setup( csp => csp.GetConnectionString() )
			.Returns( connectionString );

		_ = _connectionStringProvider
			.Setup( csp => csp.RefreshConnectionString() );

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.Dispose() );

		_ = _connection
			.Setup( c => c.Open() )
			.Throws( new InvalidOperationException() );

		IEnumerable<int> result = _context.Query(
			"",
			Array.Empty<SqlParameter>(),
			Converter,
			( conn, converter, sql, parameters ) => {
				return new int[] { expected };
			}
		);

		_ = Assert.Throws<InvalidOperationException>( () => result.ToList() );
	}

	[Test]
	public void Perform_InvalidConnection_ConnectionStringRefreshed() {
		int expected = 1;
		string connectionString = "invalid_string";

		_ = _connectionStringProvider
			.Setup( csp => csp.GetConnectionString() )
			.Returns( connectionString );

		_ = _connectionStringProvider
			.Setup( csp => csp.RefreshConnectionString() );

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.Dispose() );

		_ = _connection
			.Setup( c => c.Open() )
			.Throws( new InvalidOperationException() );

		_ = Assert.Throws<InvalidOperationException>( () => _context.Perform(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters ) => {
				return expected;
			}
		) );
	}

	[Test]
	public void PerformMasterAsync_InvalidConnection_ConnectionStringRefreshed() {
		int expected = 1;
		string connectionString = "invalid_string";

		_ = _connectionStringProvider
			.Setup( csp => csp.GetMasterConnectionStringAsync( It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( connectionString );

		_ = _connectionStringProvider
			.Setup( csp => csp.RefreshConnectionStringAsync( It.IsAny<CancellationToken>() ) )
			.Returns( Task.CompletedTask );

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _connection
			.Setup( c => c.OpenAsync( It.IsAny<CancellationToken>() ) )
			.Throws( new InvalidOperationException() );

		_ = Assert.ThrowsAsync<InvalidOperationException>( async () => await _context.PerformMasterAsync(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters, token ) => {
				return Task.FromResult( expected );
			},
			CancellationToken.None
		) );
	}

	[Test]
	public void PerformMaster_InvalidConnection_ConnectionStringRefreshed() {
		int expected = 1;
		string connectionString = "invalid_string";

		_ = _connectionStringProvider
			.Setup( csp => csp.GetMasterConnectionString() )
			.Returns( connectionString );

		_ = _connectionStringProvider
			.Setup( csp => csp.RefreshConnectionString() );

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.Dispose() );

		_ = _connection
			.Setup( c => c.Open() )
			.Throws( new InvalidOperationException() );

		_ = Assert.Throws<InvalidOperationException>( () => _context.PerformMaster(
			"",
			Array.Empty<SqlParameter>(),
			( conn, sql, parameters ) => {
				return expected;
			}
		) );
	}

	private void SetupValidConnectionAsync(
		bool setupMaster = false
	) {
		string connectionString = GetConnectionString();

		if (setupMaster) {
			_ = _connectionStringProvider
				.Setup( csp => csp.GetMasterConnectionStringAsync( It.IsAny<CancellationToken>() ) )
				.ReturnsAsync( connectionString );
		} else {
			_ = _connectionStringProvider
				.Setup( csp => csp.GetConnectionStringAsync( It.IsAny<CancellationToken>() ) )
				.ReturnsAsync( connectionString );
		}

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _connection
			.Setup( c => c.OpenAsync( It.IsAny<CancellationToken>() ) )
			.Returns( Task.CompletedTask );

		_ = _connection
			.Setup( c => c.CloseAsync() )
			.Returns( Task.CompletedTask );
	}

	private void SetupValidConnection(
		bool setupMaster = false
	) {
		string connectionString = GetConnectionString();

		if( setupMaster ) {
			_ = _connectionStringProvider
				.Setup( csp => csp.GetMasterConnectionString() )
				.Returns( connectionString );
		} else {
			_ = _connectionStringProvider
				.Setup( csp => csp.GetConnectionString() )
				.Returns( connectionString );
		}

		_ = _connectionFactory
			.Setup( cf => cf.Create( connectionString ) )
			.Returns( _connection.Object );

		_ = _connection
			.Setup( c => c.Dispose() );

		_ = _connection
			.Setup( c => c.Open() );

		_ = _connection
			.Setup( c => c.Close() );
	}

	private static string GetConnectionString() {
		SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
			DataSource = "localhost",
			Encrypt = true,
			TrustServerCertificate = true,
			IntegratedSecurity = true
		};
		return builder.ConnectionString;
	}

	private static int Converter(
		ISqlDataReader reader
	) {
		return reader.GetInt32( 0 );
	}
}
