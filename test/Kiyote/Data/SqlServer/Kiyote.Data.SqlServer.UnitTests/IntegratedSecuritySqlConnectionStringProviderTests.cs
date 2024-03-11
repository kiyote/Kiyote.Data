﻿using Microsoft.Extensions.Options;

namespace Kiyote.Data.SqlServer.UnitTests;

[TestFixture]
public sealed class IntegratedSecuritySqlConnectionStringProviderTests {

	private TestSqlServerContextOptions _options;
	private ISqlConnectionStringProvider<TestSqlServerContextOptions> _provider;

	[SetUp]
	public void SetUp() {
		_options = new TestSqlServerContextOptions();
		_provider = new IntegratedSecuritySqlConnectionStringProvider<TestSqlServerContextOptions>(
			_options
		);
	}

	[Test]
	public void GetConnectionString_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			catalog
		);
		string actual = _provider.GetConnectionString();

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public async Task GetConnectionStringAsync_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			catalog
		);
		string actual = await _provider.GetConnectionStringAsync( CancellationToken.None );

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public void GetMasterConnectionString_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			""
		);
		string actual = _provider.GetMasterConnectionString();

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public async Task GetMasterConnectionStringAsync_ValidConnectionString_ConnectionStringReturned() {
		string dataSource = "data_source";
		string catalog = "catalog";

		SetupOptions(
			dataSource,
			catalog
		);
		string expected = MakeConnectionString(
			dataSource,
			""
		);
		string actual = await _provider.GetMasterConnectionStringAsync( CancellationToken.None );

		Assert.That( actual, Is.EqualTo( expected ) );
	}

	[Test]
	public void RefreshConnectionString_ValidProvider_DoesNotFail() {
		Assert.DoesNotThrow( () => _provider.RefreshConnectionString() );
	}

	[Test]
	public void RefreshConnectionStringAsync_ValidProvider_DoesNotFail() {
		Assert.DoesNotThrowAsync( async () => await _provider.RefreshConnectionStringAsync( CancellationToken.None ) );
	}

	private void SetupOptions(
		string dataSource,
		string catalog
	) {
		_options.DataSource = dataSource;
		_options.InitialCatalog = catalog;
	}

	private static string MakeConnectionString(
		string dataSource,
		string initialCatalog
	) {
		SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
			DataSource = dataSource,
			Encrypt = true,
			TrustServerCertificate = true,
			IntegratedSecurity = true
		};
		if( !string.IsNullOrWhiteSpace( initialCatalog ) ) {
			builder.InitialCatalog = initialCatalog;
		}

		return builder.ConnectionString;
	}
}
