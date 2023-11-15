using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Options;

namespace Kiyote.Data.SqlServer.UnitTests;

[TestFixture]
public sealed class AwsSecretSqlConnectionStringProviderTests {

	private Mock<IOptions<TestSqlServerContextOptions>> _options;
	private Mock<IAmazonSecretsManager> _secretsManager;
	private ISqlConnectionStringProvider<TestSqlServerContextOptions> _provider;

	[SetUp]
	public void SetUp() {
		_options = new Mock<IOptions<TestSqlServerContextOptions>>( MockBehavior.Strict );
		_secretsManager = new Mock<IAmazonSecretsManager>( MockBehavior.Strict );
		_provider = new AwsSecretSqlConnectionStringProvider<TestSqlServerContextOptions>(
			_secretsManager.Object,
			_options.Object
		);
	}

	[TearDown]
	public void TearDown() {
		_options.VerifyAll();
		_secretsManager.VerifyAll();
	}

	[Test]
	public async Task GetConnectionStringAsync_ValidConnectionString_ConnectionStringReturned() {
		string secretName = "secret_name";
		string expected = MakeConnectionString();

		SetupOptions( secretName );

		GetSecretValueResponse response = new GetSecretValueResponse() {
			SecretString = $"{{ \"ConnectionString\": \"{expected}\" }}"
		};
		_ = _secretsManager
			.Setup( sm => sm.GetSecretValueAsync(
				It.Is<GetSecretValueRequest>( ( r ) => r.SecretId == secretName ),
				It.IsAny<CancellationToken>()
			) )
			.ReturnsAsync( response );

		string actual = await _provider.GetConnectionStringAsync( CancellationToken.None );

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public void GetConnectionStringAsync_EmptyConnectionString_ThrowsException() {
		string secretName = "secret_name";
		string expected = "";

		SetupOptions( secretName );

		GetSecretValueResponse response = new GetSecretValueResponse() {
			SecretString = $"{{ \"ConnectionString\": \"{expected}\" }}"
		};
		_ = _secretsManager
			.Setup( sm => sm.GetSecretValueAsync(
				It.Is<GetSecretValueRequest>( ( r ) => r.SecretId == secretName ),
				It.IsAny<CancellationToken>()
			) )
			.ReturnsAsync( response );

		_ = Assert.ThrowsAsync<InvalidOperationException>( () => _provider.GetConnectionStringAsync( CancellationToken.None ) );
	}

	[Test]
	public void GetConnectionStringAsync_EmptySecret_ThrowsException() {
		string secretName = "secret_name";

		SetupOptions( secretName );

		GetSecretValueResponse response = new GetSecretValueResponse() {
			SecretString = " "
		};
		_ = _secretsManager
			.Setup( sm => sm.GetSecretValueAsync(
				It.Is<GetSecretValueRequest>( ( r ) => r.SecretId == secretName ),
				It.IsAny<CancellationToken>()
			) )
			.ReturnsAsync( response );

		_ = Assert.ThrowsAsync<InvalidOperationException>( () => _provider.GetConnectionStringAsync( CancellationToken.None ) );
	}

	[Test]
	public void GetConnectionString_ValidConnectionString_ConnectionStringReturned() {
		string secretName = "secret_name";
		string expected = MakeConnectionString();

		SetupOptions( secretName );

		GetSecretValueResponse response = new GetSecretValueResponse() {
			SecretString = $"{{ \"ConnectionString\": \"{expected}\" }}"
		};
		_ = _secretsManager
			.Setup( sm => sm.GetSecretValueAsync(
				It.Is<GetSecretValueRequest>( ( r ) => r.SecretId == secretName ),
				It.IsAny<CancellationToken>()
			) )
			.ReturnsAsync( response );

		string actual = _provider.GetConnectionString();

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public async Task GetMasterConnectionStringAsync_ValidConnectionString_ConnectionStringReturned() {
		string secretName = "secret_name";
		string expected = MakeConnectionString( false );

		SetupOptions( secretName );

		GetSecretValueResponse response = new GetSecretValueResponse() {
			SecretString = $"{{ \"ConnectionString\": \"{MakeConnectionString( true )}\" }}"
		};
		_ = _secretsManager
			.Setup( sm => sm.GetSecretValueAsync(
				It.Is<GetSecretValueRequest>( ( r ) => r.SecretId == secretName ),
				It.IsAny<CancellationToken>()
			) )
			.ReturnsAsync( response );

		string actual = await _provider.GetMasterConnectionStringAsync( CancellationToken.None );

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public void GetMasterConnectionString_ValidConnectionString_ConnectionStringReturned() {
		string secretName = "secret_name";
		string expected = MakeConnectionString( false );

		SetupOptions( secretName );

		GetSecretValueResponse response = new GetSecretValueResponse() {
			SecretString = $"{{ \"ConnectionString\": \"{MakeConnectionString( true )}\" }}"
		};
		_ = _secretsManager
			.Setup( sm => sm.GetSecretValueAsync(
				It.Is<GetSecretValueRequest>( ( r ) => r.SecretId == secretName ),
				It.IsAny<CancellationToken>()
			) )
			.ReturnsAsync( response );

		string actual = _provider.GetMasterConnectionString();

		Assert.AreEqual( expected, actual );
	}

	[Test]
	public void RefreshConnectionString_ValidProvider_MethodDoesNotFail() {
		Assert.DoesNotThrow( () => _provider.RefreshConnectionString() );
	}

	[Test]
	public void RefreshConnectionStringAsync_ValidProvider_MethodDoesNotFail() {
		Assert.DoesNotThrowAsync( async () => await _provider.RefreshConnectionStringAsync( CancellationToken.None ) );
	}

	private void SetupOptions(
		string secretName
	) {
		TestSqlServerContextOptions options = new TestSqlServerContextOptions() {
			ConnectionStringSecretName = secretName
		};
		_ = _options
			.SetupGet( o => o.Value )
			.Returns( options );
	}

	private static string MakeConnectionString(
		bool includeCatalog = true
	) {
		SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
			DataSource = "localhost",
			Encrypt = true,
			TrustServerCertificate = true,
			IntegratedSecurity = true,
		};
		if( includeCatalog ) {
			builder.InitialCatalog = "catalog";
		}

		return builder.ConnectionString;
	}
}
