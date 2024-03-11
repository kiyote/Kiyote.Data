namespace Kiyote.Data.SqlServer.UnitTests;

[TestFixture]
public sealed class BuilderSqlConnectionStringProviderTests {

	private ISqlConnectionStringProvider<TestSqlServerContextOptions>? _provider;

	[Test]
	public void GetConnectionString_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string userId = "user_id";
		string password = "password";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			userId,
			password,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			userId,
			password,
			catalog
		);
		string actual = _provider!.GetConnectionString();

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public async Task GetConnectionStringAsync_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string userId = "user_id";
		string password = "password";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			userId,
			password,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			userId,
			password,
			catalog
		);
		string actual = await _provider!.GetConnectionStringAsync( CancellationToken.None );

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public void GetMasterConnectionString_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string userId = "user_id";
		string password = "password";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			userId,
			password,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			userId,
			password,
			""
		);
		string actual = _provider!.GetMasterConnectionString();

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public async Task GetMasterConnectionStringAsync_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string userId = "user_id";
		string password = "password";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			userId,
			password,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			userId,
			password,
			""
		);
		string actual = await _provider!.GetMasterConnectionStringAsync( CancellationToken.None );

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public void RefreshConnectionString_ValidProvider_DoesNotFail() {
		Assert.DoesNotThrow( () => _provider!.RefreshConnectionString() );
	}

	[Test]
	public void RefreshConnectionStringAsync_ValidProvider_DoesNotFail() {
		Assert.DoesNotThrowAsync( async () => await _provider!.RefreshConnectionStringAsync( CancellationToken.None ) );
	}

	private void SetupOptions(
		string dataSource,
		string userId,
		string password,
		string catalog
	) {
		var options = new TestSqlServerContextOptions {
			ConnectionStringSecretName = SqlServerContextOptions.BuilderConnectionStringProvider,
			DataSource = dataSource,
			UserId = userId,
			Password = password,
			InitialCatalog = catalog
		};
		_provider = new BuilderSqlConnectionStringProvider<TestSqlServerContextOptions>(
			options
		);
	}

	private static string MakeConnectionString(
		string dataSource,
		string userId,
		string password,
		string initialCatalog
	) {
		SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
			DataSource = dataSource,
			UserID = userId,
			Password = password,
			Encrypt = true,
			TrustServerCertificate = true
		};
		if( !string.IsNullOrWhiteSpace( initialCatalog ) ) {
			builder.InitialCatalog = initialCatalog;
		}

		return builder.ConnectionString;
	}
}
