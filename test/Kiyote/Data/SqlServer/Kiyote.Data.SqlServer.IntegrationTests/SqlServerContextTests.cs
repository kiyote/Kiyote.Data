using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Data.SqlServer.IntegrationTests;

[TestFixture]
public sealed class SqlServerContextTests {

	private string? _catalog;
	private IServiceScope _scope;
	private ISqlServerContext<TestSqlServerContextOptions> _context;

	[SetUp]
	public void Setup() {
		_catalog = Guid.NewGuid().ToString( "N" );

		IServiceCollection serviceCollection = new ServiceCollection();
		IConfiguration configuration = BuildConfiguration( _catalog );

		_ = serviceCollection.AddSqlServer<TestSqlServerContextOptions>(
			(opts) => {
				configuration.Bind( "Kiyote:Data:SqlServer", opts );
			}
		);

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

		Assert.That( result, Is.Not.EqualTo( 0 ) );
	}

	private static IConfigurationRoot BuildConfiguration(
		string catalog
	) {
		IConfigurationRoot configuration = new ConfigurationBuilder()
			// Or set the values in an testsettings.json file
			.AddInMemoryCollection( new Dictionary<string, string?>() {
				[ "Kiyote:Data:SqlServer:InitialCatalog" ] = catalog,
				[ "Kiyote:Data:SqlServer:DataSource" ] = "localhost",
				[ "Kiyote:Data:SqlServer:ConnectionStringProvider" ] = SqlServerContextOptions.IntegratedSecurityConnectionStringProvider
			} )
			// This is used by the CI system to provide credentials during the run
			.AddJsonFile( "testsettings.json", true )
			.AddEnvironmentVariables()
			.Build();

		return configuration;
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
