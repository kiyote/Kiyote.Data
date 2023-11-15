namespace Kiyote.Data.SqlServer.UnitTests;

[TestFixture]
public sealed class SqlServerRepositoryTests {

	private Mock<ISqlServerContext<TestSqlServerContextOptions>> _context;
	private Mock<ISqlConnection> _connection;
	private Mock<ISqlCommand> _command;
	private Mock<ISqlParameterCollection> _parameters;
	private Mock<ISqlDataReader> _reader;
	private ISqlServerRepository<TestSqlServerContextOptions> _repository;

	[SetUp]
	public void SetUp() {
		_reader = new Mock<ISqlDataReader>( MockBehavior.Strict );
		_parameters = new Mock<ISqlParameterCollection>( MockBehavior.Strict );
		_command = new Mock<ISqlCommand>( MockBehavior.Strict );
		_connection = new Mock<ISqlConnection>( MockBehavior.Strict );
		_context = new Mock<ISqlServerContext<TestSqlServerContextOptions>>( MockBehavior.Strict );
		_repository = new SqlServerRepository<TestSqlServerContextOptions>(
			_context.Object
		);
	}

	[TearDown]
	public void TearDown() {
		_reader.VerifyAll();
		_parameters.VerifyAll();
		_command.VerifyAll();
		_connection.VerifyAll();
		_context.VerifyAll();
	}

	[Test]
	public void Query_OneItem_ReturnsData() {
		string sql = "sql";
		string parameterName = "name";
		int parameterValue = 2;
		SqlParameter parameter = new SqlParameter( parameterName, parameterValue );
		SqlParameter[] parameters = new SqlParameter[] {
			parameter
		};
		int expectedValue = 3;
		int[] expected = new int[] { expectedValue };

		_ = _connection
			.Setup( c => c.CreateCommand() )
			.Returns( _command.Object );

		_ = _command
			.SetupSet( c => c.CommandType = System.Data.CommandType.Text );

		_ = _command
			.SetupSet( c => c.CommandText = sql );

		_ = _command
			.SetupGet( c => c.Parameters )
			.Returns( _parameters.Object );

		_ = _command
			.Setup( c => c.Dispose() );

		_ = _parameters
			.Setup( p => p.Add( parameter ) );

		_ = _command
			.Setup( c => c.ExecuteReader() )
			.Returns( _reader.Object );

		_ = _reader
			.SetupSequence( r => r.Read() )
			.Returns( true )
			.Returns( false );

		_ = _reader
			.Setup( r => r.GetInt32( 0 ) )
			.Returns( expectedValue );

		_ = _reader
			.Setup( r => r.Dispose() );

		_ = _context
			.Setup( c => c.Query(
				It.Is<string>( ( s ) => s == sql ),
				It.Is<SqlParameter[]>( ( p ) => p.SequenceEqual( parameters ) ),
				It.IsAny<Func<ISqlDataReader, int>>(),
				It.IsAny<Func<ISqlConnection, Func<ISqlDataReader, int>, string, SqlParameter[], IEnumerable<int>>>()
			) )
			.Callback<
				string,
				SqlParameter[],
				Func<ISqlDataReader, int>,
				Func<ISqlConnection, Func<ISqlDataReader, int>, string, SqlParameter[], IEnumerable<int>>
			> ( (sql, parameters, converter, action) => {
				List<int> values = action( _connection.Object, converter, sql, parameters ).ToList();
			} )
			.Returns( expected );

		List<int> actual = _repository.Query(
			sql,
			Converter,
			parameters
		).ToList();

		CollectionAssert.AreEquivalent( expected, actual );
	}

	[Test]
	public async Task QueryAsync_OneItem_ReturnsData() {
		string sql = "sql";
		string parameterName = "name";
		int parameterValue = 2;
		SqlParameter parameter = new SqlParameter( parameterName, parameterValue );
		SqlParameter[] parameters = new SqlParameter[] {
			parameter
		};
		int expectedValue = 3;
		int[] expected = new int[] { expectedValue };

		_ = _connection
			.Setup( c => c.CreateCommand() )
			.Returns( _command.Object );

		_ = _command
			.SetupSet( c => c.CommandType = System.Data.CommandType.Text );

		_ = _command
			.SetupSet( c => c.CommandText = sql );

		_ = _command
			.SetupGet( c => c.Parameters )
			.Returns( _parameters.Object );

		_ = _command
			.Setup( c => c.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _parameters
			.Setup( p => p.Add( parameter ) );

		_ = _command
			.Setup( c => c.ExecuteReaderAsync( It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( _reader.Object );

		_ = _reader
			.SetupSequence( r => r.ReadAsync( It.IsAny<CancellationToken>()) )
			.ReturnsAsync( true )
			.ReturnsAsync( false );

		_ = _reader
			.Setup( r => r.GetInt32( 0 ) )
			.Returns( expectedValue );

		_ = _reader
			.Setup( r => r.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _context
			.Setup( c => c.QueryAsync(
				It.Is<string>( ( s ) => s == sql ),
				It.Is<SqlParameter[]>( ( p ) => p.SequenceEqual( parameters ) ),
				It.IsAny<Func<ISqlDataReader, int>>(),
				It.IsAny<Func<ISqlConnection, Func<ISqlDataReader, int>, string, SqlParameter[], CancellationToken, IAsyncEnumerable<int>>>(),
				It.IsAny<CancellationToken>()
			) )
			.Callback<
				string,
				SqlParameter[],
				Func<ISqlDataReader, int>,
				Func<ISqlConnection, Func<ISqlDataReader, int>, string, SqlParameter[], CancellationToken, IAsyncEnumerable<int>>,
				CancellationToken
			>( async ( sql, parameters, converter, action, token ) => {
				List<int> values = await action( _connection.Object, converter, sql, parameters, token ).ToListAsync( token );
			} )
			.Returns( expected.ToAsyncEnumerable() );

		List<int> actual = await _repository.QueryAsync(
			sql,
			Converter,
			CancellationToken.None,
			parameters			
		).ToListAsync( CancellationToken.None );

		CollectionAssert.AreEquivalent( expected, actual );
	}

	[Test]
	public void ExecuteNonQuery_ValidConnection_ReturnsValue() {
		string sql = "sql";
		string parameterName = "name";
		int parameterValue = 2;
		SqlParameter parameter = new SqlParameter( parameterName, parameterValue );
		SqlParameter[] parameters = new SqlParameter[] {
			parameter
		};
		int expectedValue = 3;
		int[] expected = new int[] { expectedValue };

		_ = _connection
			.Setup( c => c.CreateCommand() )
			.Returns( _command.Object );

		_ = _command
			.SetupSet( c => c.CommandType = System.Data.CommandType.Text );

		_ = _command
			.SetupSet( c => c.CommandText = sql );

		_ = _command
			.SetupGet( c => c.Parameters )
			.Returns( _parameters.Object );

		_ = _command
			.Setup( c => c.Dispose() );

		_ = _parameters
			.Setup( p => p.Add( parameter ) );

		_ = _command
			.Setup( c => c.ExecuteNonQuery() )
			.Returns( expectedValue );

		_ = _context
			.Setup( c => c.Perform(
				It.Is<string>( ( s ) => s == sql ),
				It.Is<SqlParameter[]>( ( p ) => p.SequenceEqual( parameters ) ),
				It.IsAny<Func<ISqlConnection, string, SqlParameter[], int>>()
			) )
			.Callback<
				string,
				SqlParameter[],
				Func<ISqlConnection, string, SqlParameter[], int>
			>( ( sql, parameters, action ) => {
				int value = action( _connection.Object, sql, parameters );
			} )
			.Returns( expectedValue );

		int actual = _repository.ExecuteNonQuery(
			sql,
			parameters
		);

		Assert.AreEqual( expectedValue, actual );
	}

	[Test]
	public async Task ExecuteNonQueryAsync_ValidConnection_ReturnsValue() {
		string sql = "sql";
		string parameterName = "name";
		int parameterValue = 2;
		SqlParameter parameter = new SqlParameter( parameterName, parameterValue );
		SqlParameter[] parameters = new SqlParameter[] {
			parameter
		};
		int expectedValue = 3;
		int[] expected = new int[] { expectedValue };

		_ = _connection
			.Setup( c => c.CreateCommand() )
			.Returns( _command.Object );

		_ = _command
			.SetupSet( c => c.CommandType = System.Data.CommandType.Text );

		_ = _command
			.SetupSet( c => c.CommandText = sql );

		_ = _command
			.SetupGet( c => c.Parameters )
			.Returns( _parameters.Object );

		_ = _command
			.Setup( c => c.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _parameters
			.Setup( p => p.Add( parameter ) );

		_ = _command
			.Setup( c => c.ExecuteNonQueryAsync( It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( expectedValue );

		_ = _context
			.Setup( c => c.PerformAsync<int>(
				It.Is<string>( ( s ) => s == sql ),
				It.Is<SqlParameter[]>( ( p ) => p.SequenceEqual( parameters ) ),
				It.IsAny<Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<int>>>(),
				It.IsAny<CancellationToken>()
			) )
			.Callback<
				string,
				SqlParameter[],
				Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<int>>,
				CancellationToken
			>( async ( sql, parameters, action, token ) => {
				int value = await action( _connection.Object, sql, parameters, token );
			} )
			.ReturnsAsync( expectedValue );

		int actual = await _repository.ExecuteNonQueryAsync(
			sql,
			CancellationToken.None,
			parameters
		);

		Assert.AreEqual( expectedValue, actual );
	}

	[Test]
	public void ExecuteScalar_ValidConnection_ReturnsValue() {
		string sql = "sql";
		string parameterName = "name";
		int parameterValue = 2;
		SqlParameter parameter = new SqlParameter( parameterName, parameterValue );
		SqlParameter[] parameters = new SqlParameter[] {
			parameter
		};
		int expectedValue = 3;
		int[] expected = new int[] { expectedValue };

		_ = _connection
			.Setup( c => c.CreateCommand() )
			.Returns( _command.Object );

		_ = _command
			.SetupSet( c => c.CommandType = System.Data.CommandType.Text );

		_ = _command
			.SetupSet( c => c.CommandText = sql );

		_ = _command
			.SetupGet( c => c.Parameters )
			.Returns( _parameters.Object );

		_ = _command
			.Setup( c => c.Dispose() );

		_ = _parameters
			.Setup( p => p.Add( parameter ) );

		_ = _command
			.Setup( c => c.ExecuteScalar<int>() )
			.Returns( expectedValue );

		_ = _context
			.Setup( c => c.Perform(
				It.Is<string>( ( s ) => s == sql ),
				It.Is<SqlParameter[]>( ( p ) => p.SequenceEqual( parameters ) ),
				It.IsAny<Func<ISqlConnection, string, SqlParameter[], int>>()
			) )
			.Callback<
				string,
				SqlParameter[],
				Func<ISqlConnection, string, SqlParameter[], int>
			>( ( sql, parameters, action ) => {
				int value = action( _connection.Object, sql, parameters );
			} )
			.Returns( expectedValue );

		int actual = _repository.ExecuteScalar<int>(
			sql,
			parameters
		);

		Assert.AreEqual( expectedValue, actual );
	}

	[Test]
	public async Task ExecuteScalarAsync_ValidConnection_ReturnsValue() {
		string sql = "sql";
		string parameterName = "name";
		int parameterValue = 2;
		SqlParameter parameter = new SqlParameter( parameterName, parameterValue );
		SqlParameter[] parameters = new SqlParameter[] {
			parameter
		};
		int expectedValue = 3;
		int[] expected = new int[] { expectedValue };

		_ = _connection
			.Setup( c => c.CreateCommand() )
			.Returns( _command.Object );

		_ = _command
			.SetupSet( c => c.CommandType = System.Data.CommandType.Text );

		_ = _command
			.SetupSet( c => c.CommandText = sql );

		_ = _command
			.SetupGet( c => c.Parameters )
			.Returns( _parameters.Object );

		_ = _command
			.Setup( c => c.DisposeAsync() )
			.Returns( ValueTask.CompletedTask );

		_ = _parameters
			.Setup( p => p.Add( parameter ) );

		_ = _command
			.Setup( c => c.ExecuteScalarAsync<int>( It.IsAny<CancellationToken>() ) )
			.ReturnsAsync( expectedValue );

		_ = _context
			.Setup( c => c.PerformAsync<int>(
				It.Is<string>( ( s ) => s == sql ),
				It.Is<SqlParameter[]>( ( p ) => p.SequenceEqual( parameters ) ),
				It.IsAny<Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<int>>>(),
				It.IsAny<CancellationToken>()
			) )
			.Callback<
				string,
				SqlParameter[],
				Func<ISqlConnection, string, SqlParameter[], CancellationToken, Task<int>>,
				CancellationToken
			>( async ( sql, parameters, action, token ) => {
				int value = await action( _connection.Object, sql, parameters, token );
			} )
			.ReturnsAsync( expectedValue );

		int actual = await _repository.ExecuteScalarAsync<int>(
			sql,
			CancellationToken.None,
			parameters
		);

		Assert.AreEqual( expectedValue, actual );
	}

	private static int Converter(
		ISqlDataReader reader
	) {
		return reader.GetInt32( 0 );
	}
}
