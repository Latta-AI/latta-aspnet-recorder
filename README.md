To implement Latta into ASP.NET Core application:

1. Install Latta via NuGet

```
dotnet add package LattaASPNet
```

2. Add your API key to appsettings.json file

```
"LATTA_APIKEY": "YOUR API KEY"
```

3. Add logging provider line AFTER CreateBuilder method in Program.cs file

```
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddProvider(new LattaLogProvider());
```

4. Register middleware BEFORE UseStaticFiles method in Program.cs file

```
app.UseMiddleware<LattaMiddleware>();

app.UseStaticFiles();
```