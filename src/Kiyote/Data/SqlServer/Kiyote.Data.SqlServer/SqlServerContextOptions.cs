using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Data.SqlServer;

/// <summary>
/// Provides configuration <see cref="SqlServerContext"/> with all values
/// needed to establish a connection to the SqlServer instance.
/// </summary>
/// <typeparam name="T">
/// Allows for multiple configurations to be defined enabling a context
/// per SqlServer instance connection desired.
/// </typeparam>
/// <remarks>
/// If two different methods of connecting to a SqlServer instance are required,
/// for example, perhaps two different sets of credentials would be needed to
/// access different tables, or perhaps to entirely different SqlServers then
/// two SqlServerContextOptions<T> would need to be defined with the necessary
/// configuration settings supplied in two different instances of T types.
/// </remarks>
[ExcludeFromCodeCoverage( Justification = "POCO" )]
public abstract class SqlServerContextOptions {

	public const string BuilderConnectionStringProvider = "Builder";
	public const string IntegratedSecurityConnectionStringProvider = "Integrated";
	public const string AwsSecretConnectionStringProvider = "AwsSecret";

	public string? ConnectionStringSecretName { get; set; }

	public string? DataSource { get; set; }

	public string? UserId { get; set; }

	public string? Password { get; set; }

	public string? InitialCatalog { get; set; }

	public string? ConnectionStringProvider { get; set; }
}
