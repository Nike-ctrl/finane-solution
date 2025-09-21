
dotnet ef dbcontext scaffold "Host=192.170.170.153;Port=5435;Database=finanze;Username=myuser;Password=mypassword" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Models --context-dir Context --context FinanzeContext --no-pluralize --use-database-names  --force

{
  "ConnectionStrings": {
    "DefaultConnectionFinanze":  "Host=192.170.170.153;Port=5435;Database=finanze;Username=myuser;Password=mypassword",
  }
}



```
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
```


## backup 

cd 'C:\Program Files\pgAdmin 4\runtime'
.\pg_dump.exe -U myuser -h serverhome -p 5435 -d finanze -n public  -F c -b -v -f "C:\Users\gagli\Desktop\dati_finanze.dump"


## restore 
cd 'C:\Program Files\pgAdmin 4\runtime'
.\pg_restore.exe -U myuser -h serverhome -p 5436 -d mydatabase -v "C:\Users\gagli\Desktop\dati_finanze.dump"
