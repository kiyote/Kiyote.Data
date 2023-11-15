using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Kiyote.Data.SqlServer.Benchmarks;

public static class Program {
	public static void Main(
		string[] args
	) {
		ManualConfig config = DefaultConfig.Instance
			.AddJob( Job
				 .MediumRun
				 .WithLaunchCount( 1 )
				 .WithToolchain( InProcessNoEmitToolchain.Instance ) );

		_ = BenchmarkRunner.Run<SqlServerContextBenchmarks>( config, args );
	}
}
