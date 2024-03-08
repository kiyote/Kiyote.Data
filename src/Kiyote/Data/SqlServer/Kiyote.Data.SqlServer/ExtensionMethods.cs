using System.Diagnostics.CodeAnalysis;
using Kiyote.AWS.SecretsManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Data.SqlServer;

[ExcludeFromCodeCoverage( Justification = "Registration-only class." )]
public static class ExtensionMethods {

	public static IServiceCollection AddIntegratedSecuritySqlServer<T>(
		this IServiceCollection services
	) where T : SqlServerContextOptions {
		_ = services.AddSingleton<ISqlConnectionStringProvider<T>, IntegratedSecuritySqlConnectionStringProvider<T>>();
		_ = services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
		_ = services.AddSingleton<ISqlServerContext<T>, SqlServerContext<T>>();

		return services;
	}

	public static IServiceCollection AddBuilderSqlServer<T>(
		this IServiceCollection services
	) where T : SqlServerContextOptions {
		_ = services.AddSingleton<ISqlConnectionStringProvider<T>, BuilderSqlConnectionStringProvider<T>>();
		_ = services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
		_ = services.AddSingleton<ISqlServerContext<T>, SqlServerContext<T>>();

		return services;
	}

	public static IServiceCollection AddAwsSecretSqlServer<T>(
		this IServiceCollection services
	) where T : SqlServerContextOptions {
		_ = services.AddSecretsManager<T>();
		_ = services.AddSingleton<ISqlConnectionStringProvider<T>, AwsSecretSqlConnectionStringProvider<T>>();
		_ = services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
		_ = services.AddSingleton<ISqlServerContext<T>, SqlServerContext<T>>();

		return services;
	}

}
