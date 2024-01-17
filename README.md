![coverage](https://github.com/kiyote/Data/blob/badges/.badges/main/coverage.svg?raw=true)

# Data

# Setup

Prior to running the tests, please be sure a SQLServer instance is running locally.
Additionally, set the environment variables `Kiyote:Data:SqlServer:UserID` and
`Kiyote:Data:SqlServer:Password` to provide the credentials necessary to connect to
the instance.  If using Docker, this is as simple as:
`docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrong(!)Password" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`

# Benchmarks

```
BenchmarkDotNet v0.13.10, Windows 10 (10.0.19044.3570/21H2/November2021Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15
LaunchCount=1  WarmupCount=10
```

| Method       | RowCount | Mean        | Error     | StdDev    | Gen0    | Gen1    | Allocated |
|------------- |--------- |------------:|----------:|----------:|--------:|--------:|----------:|
| QueryAsync   | 100      |   155.99 us |  6.198 us |  5.797 us |  0.9766 |       - |  11.52 KB |
| Query        | 100      |   106.37 us |  0.849 us |  0.752 us |  0.6104 |       - |   4.36 KB |
| Perform      | 100      |    88.29 us |  3.707 us |  3.096 us |  0.4883 |       - |   3.71 KB |
| PerformAsync | 100      |    87.41 us |  3.836 us |  3.400 us |  0.4883 |       - |   3.71 KB |
| QueryAsync   | 1000     |   496.88 us | 11.719 us | 10.388 us |  8.7891 |       - |  57.21 KB |
| Query        | 1000     |   281.26 us |  3.693 us |  3.084 us |  1.4648 |       - |  11.43 KB |
| Perform      | 1000     |    86.46 us |  1.473 us |  1.230 us |  0.4883 |       - |   3.71 KB |
| PerformAsync | 1000     |    89.09 us |  1.955 us |  1.633 us |  0.4883 |       - |   3.71 KB |
| QueryAsync   | 10000    | 2,592.64 us | 31.012 us | 25.896 us | 97.6563 | 19.5313 | 605.09 KB |
| Query        | 10000    | 1,700.15 us |  9.520 us |  7.432 us | 19.5313 |  1.9531 | 131.53 KB |
| Perform      | 10000    |    89.01 us |  1.133 us |  0.946 us |  0.4883 |       - |   3.71 KB |
| PerformAsync | 10000    |   100.42 us | 11.698 us | 10.370 us |  0.4883 |       - |   3.71 KB |

# Notes

Github action failing with permission denied?
```
git update-index --chmod=+x ./db.sh
```
