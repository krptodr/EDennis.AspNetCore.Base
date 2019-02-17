# EDennis.AspNetCore.Base
*A library with many test/sample projects, used as a basis for creating testable three-tier applications*

## Motivation
ASP.NET Core MVC and Entity Framework Core collectively provide a great framework for development of enterprise applications.  That said, developing integration-testable three-tier applications with these technologies can be a little challenging at times.  It is relatively easy to create a web application on one server that communicates with web API on another server, which in turn communicates with a database via Entity Framework; however, to set up proper, repeatable integration tests -- those that completely reset the database (whether in-memory or real) after each test -- is not straightforward, especially when dealing with three-tier solutions.  Furthermore, to completely reset the database after spot testing via Swagger UI requires some extra set-up work.  Collectively, these challenges motivated development of a library to facilitate testable three-tier application development. 

## Features
The current library has a number of features that assist with application development:
- An action filter that looks for special header values in HTTP requests in order to initialize or reset a test connection or in-memory database.  Originally, this action filter fully supported both real databases and in-memory databases; however, only the in-memory databases proved to be reliable with parallel unit/integration testing.  (A testing-transaction alternative to in-memory databases is still available, but it should be used only in situations where database locks are not likely -- e.g., spot testing using Swagger.)
- An ApiLauncher class that uses appsettings.json to identify, start, and stop one or more web applications.  This utility class makes it easier to start and manage a set of micro-services during both automated integration testing and spot-testing (e.g., using Swagger).    
- A base repository class that simplifies basic Entity Framework operations but still allows querying via Linq expressions
- A pair of design-time factory classes that allow code-first migrations without default constructors in the DbContext class. (One of these classes, in combination with the EDennis.MigrationsExtensions NuGet library, supports auto-creation of temporal tables.)
- a MaxPlusOneValueGenerator class that *mostly* emulates a sequence or identity during testing
- SQL scripts that identify and reset all identity values and sequences in a SQL Server database
- Extensions to IServiceCollection that use Reflection to simplify the setup of Dependency Injection for
  - Typed HttpClients
  - DbContextPools
  - Repository classes
- Extensions to HttpClient that simplify the setup of 
  - Basic Http operations such as posting, putting, deleting, or getting any object of type T
  - Consecutive Http operations such as posting and then getting an object of type T -- making it easier to use HttpClient for testing.
- Two types of Controllers
  - RepoController, which is designed to work with one or more repository classes backed by Entity Framework
  - ProxyController, which is designed to work with one or more API classes backed by HttpClient instances
- Default security for users and clients
  - AutologinMiddleware, which allows configuration of users that can be automatically logged on during different launch configurations
  - MockClientAuthorizationMiddleware, which allows configuration of OAuth clients for which access tokens are automatically generated during different launch configurations (especially helpful if you are spot-testing with Swagger)
  - AddDefaultAuthorizationPolicyConvention, which automatically adds Authorize policies to controllers and action methods (obviating the need for the Authorize attribute)
  - AddClientAuthenticationAndAuthorizationWithDefaultPolicies, which is an IServiceCollection extension method that uses reflection to automatically assign default, scope-defined policies to controllers and action methods.

## Constraints
- The ApiLauncher will fail if the target port for an API is unavailable.
- In order for the ApiLauncher to work appropriately with spot-testing a master Api (one that calls other APIs), the master Api must be launched using the project profile (with a port in an acceptable range), rather than using IIS Express.  To reset sequences/identities at the end of a spot-testing session, be sure to suspend the application by pressing CTRL-C in the command window, rather than by closing the browser window/tab.  (Upon restarting spot-testing, identities and sequences will be reset, just in case.)