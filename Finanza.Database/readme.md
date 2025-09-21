
dotnet ef dbcontext scaffold "Host=192.170.170.153;Port=5435;Database=finanze;Username=myuser;Password=mypassword" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Models --context-dir Context --context FinanzeContext --no-pluralize --use-database-names  --force

{
  "ConnectionStrings": {
    "DefaultConnectionFinanze":  "Host=192.170.170.153;Port=5435;Database=finanze;Username=myuser;Password=mypassword",
  }
}




    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
        string? conn = configuration.GetConnectionString("DefaultConnectionFinanze");
        if (conn == null)
        {
            throw new Exception("Not found connection string DefaultConnectionFinanze inside appsettings.json");
        }
        optionsBuilder.UseNpgsql(conn);
    }


        <PackageReference Include="Npgsql" Version="9.0.3" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
	  <PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="8.0.0" />
	  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
	  <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
	  <PackageReference Include="Dapper" Version="2.1.66" />