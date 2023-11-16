using Microsoft.Extensions.Configuration;

namespace Kiyote.Data.SqlServer.IntegrationTests;

[TestFixture]
public sealed class SqlServerContextTests {

	private string? _catalog;
	private IServiceScope _scope;
	private ISqlServerContext<TestSqlServerContextOptions> _context;

	[SetUp]
	public void Setup() {
		IServiceCollection serviceCollection = new ServiceCollection();
		_catalog = ConfigureDatabase( serviceCollection, Guid.NewGuid().ToString( "N" ) );
		IServiceProvider services = serviceCollection.BuildServiceProvider();

		_scope = services.CreateScope();
		_context = services.GetRequiredService<ISqlServerContext<TestSqlServerContextOptions>>();

		CreateDatabase( _context, _catalog );
	}

	[TearDown]
	public void TearDown() {
		if( !string.IsNullOrWhiteSpace( _catalog ) ) {
			DeleteDatabase( _context, _catalog );
		}
		_scope.Dispose();
	}

	[Test]
	public void Perform() {
		string sql = "INSERT INTO TESTS ( ValueColumn ) OUTPUT Inserted.KeyColumn VALUES ( @ValueColumn )";

		int result = _context!.Perform(
			sql,
			[],
			( conn, sql, parameters ) => {
				using ISqlCommand command = conn.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new SqlParameter( "@ValueColumn", "VALUE" ) );
				using ISqlDataReader reader = command.ExecuteReader();
				if( reader.Read() ) {
					return reader.GetInt32( 0 );
				}
				Assert.Fail( "Unable to read from database." );
				throw new InvalidOperationException();
			}
		);

		Assert.AreNotEqual( 0, result );
	}

	private static string ConfigureDatabase(
		IServiceCollection services,
		string catalog
	) {
		IConfigurationRoot configuration = new ConfigurationBuilder()
			.AddInMemoryCollection( new Dictionary<string, string?>() {
				[ "Kiyote:Data:SqlServer:InitialCatalog" ] = catalog,
				[ "Kiyote:Data:SqlServer:DataSource" ] = "localhost",
				[ "Kiyote:Data:SqlServer:ConnectionStringProvider" ] = "Integrated"
			} )
			.AddEnvironmentVariables()
			.Build();

		IConfigurationSection config = configuration.GetSection( "Kiyote" ).GetSection( "Data" ).GetSection( "SqlServer" );
		_ = services.Configure<TestSqlServerContextOptions>( config );
		string? connectionStringProvider = config[ nameof( SqlServerContextOptions.ConnectionStringProvider ) ];
		if( connectionStringProvider == SqlServerContextOptions.BuilderConnectionStringProvider ) {
			_ = services.AddBuilderSqlServer<TestSqlServerContextOptions>();
		} else if( connectionStringProvider == SqlServerContextOptions.IntegratedSecurityConnectionStringProvider ) {
			_ = services.AddIntegratedSecuritySqlServer<TestSqlServerContextOptions>();
		} else {
			_ = services.AddAwsSecretSqlServer<TestSqlServerContextOptions>();
		}

		return config[ "InitialCatalog" ] ?? catalog;
	}

	private static void CreateDatabase(
		ISqlServerContext<TestSqlServerContextOptions> context,
		string catalog
	) {
		_ = context.PerformMaster(
			$@"CREATE DATABASE [{catalog}];",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();

			} );

		_ = context.Perform(
			$@"CREATE TABLE TESTS ( KeyColumn int IDENTITY(1,1) NOT NULL, ValueColumn varchar(255) NOT NULL, CONSTRAINT PK_TESTS PRIMARY KEY CLUSTERED (KeyColumn) )",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();
			} );
	}

	private static void DeleteDatabase(
		ISqlServerContext<TestSqlServerContextOptions> context,
		string catalog
	) {
		_ = context.PerformMaster(
			$@"
				ALTER DATABASE [{catalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
				DROP DATABASE [{catalog}];
			",
			[],
			( conn, sql, parameters ) => {
				ISqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				_ = cmd.ExecuteNonQuery();
				return true;
			} );
	}
}
