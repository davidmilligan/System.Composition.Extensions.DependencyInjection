# System.Composition.Extensions.DependencyInjection
An adapter that allows using MEF (System.Composition) with Asp.Net Core's DI Framework

Add `UseMef()` in `Program.cs`:

```
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseMef()
                .UseStartup<Startup>();
```

Configure the MEF container in `Startup.cs`:

```
        public void ConfigureContainer(ContainerConfiguration config)
        {
            config.WithAssembly(Assembly.GetExecutingAssembly());
        }
```

Make sure you use `AddControllersAsServices`:
```
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                 .AddControllersAsServices();
            //...
```
