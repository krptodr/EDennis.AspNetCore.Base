# EDennis.AspNetCore.Base
*A library that facilities creating testable multi-tier applications in ASP.NET Core*

## Motivation
ASP.NET Core MVC and Entity Framework Core collectively provide a great framework for development of enterprise applications.  That said, developing integration-testable three-tier applications with these technologies can be a little challenging at times.  It is relatively easy to create a web application on one server that communicates with web API on another server, which in turn communicates with a database via Entity Framework; however, to set up proper, repeatable integration tests -- those that completely reset the database (whether in-memory or real) after each test -- is not straightforward, especially when dealing with three-tier solutions.  Furthermore, to completely reset the database after spot testing via Swagger UI requires some extra set-up work.  Collectively, these challenges motivated development of a library to facilitate testable three-tier application development. 

## Features
The current library has a number of features that assist with application development:
- **Parallel Architecture** for projects that access databases and projects that only access HTTP endpoints.  The repository pattern is used for the former type of project.  The typed client pattern is used for the latter type of project.  Both patterns provide a useful abstraction layer.  Both patterns use dependency injection to give controllers access to service objects -- repos and api clients. 
- **Base Repository Classes** that simplify basic Entity Framework operations but also include support for querying by Dynamic Linq Expressions, Stored Procedures, and raw SQL.  Two writeable repo classes are provided that, in conjunction with database management interceptors (see below), can be used with in-memory testing.
- **Base Controller Classes** that simplify web API construction and provide numerous capabilities.  A WriteableController base class provides Swagger-friendly endpoints for regular CRUD operations, Dynamic Linq, OData, and DevExtreme Load operations.  A ReadonlyController class provides Swagger-friendly endpoints for regular Dynamic Linq, OData, and DevExtreme Load operations, as well as Postman-friendly endpoints for executing parameterized stored procedures.
- **Database Management Interceptors** (middleware) that look for special header values in HTTP requests in order to create, select, or discard a specific in-memory database.  This middleware makes it possible to perform multiple operations on the same in-memory database accessed indirectly through multiple layers of APIs.  The interceptors work with both Swagger spot-testing and automated integration testing.    
- **Xunit Base Classes** and associated factory classes that greatly simplify setup of unit and integration tests of projects that use in-memory databases.  These classes support the following scenarios:
  - *Repo Tests* - direct tests of methods in writeable and readonly repositories, where the writeable repo tests, through configuration only, spin up an in-memory database on the fly
  - *Controller Tests* - direct tests of controller methods, where no web server is involved, but where (through configuration only) an in-memory database may be spun up on the fly
  - *Endpoint Tests* - tests of controller methods where one or more web servers are spun up on the fly, each optionally (through configuration only) with its own in-memory or regular SQL database
  - *ApiClient Tests* - direct tests of methods in ApiClient and SecureApiClient classes, which require one or more web servers to be spun up on the fly, each optionally (through configuration only) with its own in-memory or regular SQL database
- **Security Utilities** that perform the following functions:
  - *AutologinMiddleware*, which allows configuration of users that can be automatically logged on during different launch configurations
  - *MockClientAuthorizationMiddleware* for IdentityServer, which allows configuration of OAuth clients for which access tokens are automatically generated during different launch configurations (especially helpful if you are spot-testing with Swagger)
  - *AddDefaultAuthorizationPolicyConvention*, which automatically adds Authorize policies to controllers and action methods (obviating the need for the Authorize attribute)
  - *AddClientAuthenticationAndAuthorizationWithDefaultPolicies*, which is an IServiceCollection extension method that uses reflection to automatically assign default, scope-defined policies to controllers and action methods.
- **Temporal Entities** -- entity-framework-driven support for history tables.  This support was inspired by SQL Server temporal tables; however, the EF implementation makes it possible to do time-travel queries using entity framework.  Several base repository methods are provided to support such queries.  In addition, the EF implementation makes it possible to record the user who deleted a record -- something that requires a little more work with SQL Server temporal tables. 
- **ApiLauncher** -- a main class (and supporting classes) that uses configuration settings to identify, start, and stop one or more web api projects; assign available random port numbers; ensure that the same project api is started only once; and update the BaseAddress of all associated HttpClients (following the typed client pattern).  Importantly, the ApiLauncher launches all APIs within the same execution context as the main project, making it possible to debug into the Apis, even during automated integration testing.  This utility class is especially helpful when you have multiple APIs that each use IdentityServer.
- Numerous **IServiceCollection Extension Methods** that simplify the setup of Dependency Injection for
  - Typed HttpClients
  - DbContexts
  - Repository classes
- Numerous **HttpClient Extension methods** that simplify basic Http operations such as posting, putting, deleting, or getting any object of type T

## Associated Projects
- This project has classes that replace all of the functionality provided by *EDennis.EFBase*
- This project is designed to work with **EDennis.MigrationsExtensions**, which provides assistance in performing Entity Framework migrations.  In particular, the library provides a MigrationBuilder extension method called SaveMappings, which saves Table/Class and Column/Property mappings to SQL Server extended properties.
- This project is designed to work with **EDennis.NetCoreTestingUtilities**, which provides many helpful classes for performing unit/integration tests.
- The **EDennis.DataScaffolder** project provides a WinForms application that generates static data for the "HasData" method.  When the EDennis.MigrationsExtensions SaveMappings extension method is used (see above), the static data have correct class names and property names.
