using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json")); //Default content type of all action methods is application/json (response body)
    options.Filters.Add(new ConsumesAttribute("application/json")); //(Request body)

    //Authorization policy
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    options.Filters.Add(new AuthorizeFilter(policy));

})
    .AddXmlSerializerFormatters(); //Enables XML serialization for the particular action methods [Produces("application/xml")] is mentioned 


builder.Services.AddTransient<IJwtService, JwtService>();


builder.Services.AddApiVersioning(config =>
{
    config.ApiVersionReader = new UrlSegmentApiVersionReader(); 
    //This version reader enables asp.net core to identify the current working version of the API as per the request URL

    //config.ApiVersionReader = new QueryStringApiVersionReader();
    //Reads version number from request query string called "api-version"

    //config.ApiVersionReader = new HeaderApiVersionReader("api-version"); //Reads version number from request header called "api-version". Eg: api-version: 1.0

    config.DefaultApiVersion = new ApiVersion(1, 0); //Defining default Api version
    config.AssumeDefaultVersionWhenUnspecified= true; //Sets default API version when it is not specified by user
});

//Adding DbContext as a service
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")); //Getting connection string from appsetting (CS name is "Default")
});


//builder.Services.AddEndpointsApiExplorer(); //Generates description for all endpoints


builder.Services.AddEndpointsApiExplorer(); //It enables swagger to read metadata (HTTP method, URL, attributes etc. ) of endpoints (Web Api action methods)
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory /*Solution/Project*/ , "api.xml"));

    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version= "1.0" });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "2.0" });

}); //It configures 


builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; //v1 - will be recognized by swagger
    options.SubstituteApiVersionInUrl = true;
});


//Enables relationship between different domain origins. For example: localhost:4200 can send requests to localhost:7224
//The exact meaning of course policy is the course policy contains the essential information on the server what request headers what HTTP methods and what client URLs the server can accept or reject
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder => //Policy contains set of rules that you want to accept or reject request
    {
        policyBuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()) //Url of Angular application
        .WithHeaders("Authorization", "origin", "accept", "content-type") //Server allows ang application to send following headers
        .WithMethods("GET", "POST", "PUT", "DELETE"); //Server allows following methods for angular application
    });



    //4100Client is the custom client so that we restrict certain requested methods and headers from exactly that client
    options.AddPolicy("4100Client", policyBuilder => //Policy contains set of rules that you want to accept or reject request
    {
        policyBuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins2").Get<string[]>()) //Url of Angular application
        .WithHeaders("Authorization", "origin", "accept") //Server allows ang application to send following headers
        .WithMethods("GET"); //Server allows following methods for angular application
    });
});


// This is where you add Identity to your application. Identity is the membership system in ASP.NET Core that adds user sign in functionality.
// In this case, you're specifying that you want to use your ApplicationUser class to represent users, and your ApplicationRole class to represent roles.
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 5;

    options.Password.RequireNonAlphanumeric = false;

    options.Password.RequireUppercase = false;

    options.Password.RequireLowercase = true;

    options.Password.RequireDigit = true;
})
    // This line is where you specify the Entity Framework store to use for Identity.
    // This sets up Identity to use your ApplicationDbContext to interact with the database.
    .AddEntityFrameworkStores<ApplicationDbContext>()

    // Adds a provider for generating tokens for reset passwords, change email and change telephone number operations.
    .AddDefaultTokenProviders()

    // AddUserStore specifies the user store class to use. A user store is what Identity uses to retrieve user information.
    // Here, you're specifying that Identity should use the UserStore class, and you're providing the types it needs.
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()

    // Similar to AddUserStore, AddRoleStore specifies the role store class to use.
    // A role store is what Identity uses to retrieve role information.
    // Here, you're specifying that Identity should use the RoleStore class, and you're providing the types it needs.
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();



//JWT
// Set up authentication within the application.
builder.Services.AddAuthentication(options =>
{
    // The DefaultAuthenticateScheme sets JWT (JSON Web Token) as the default authentication scheme.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // The DefaultChallengeScheme sets Cookies as the default challenge scheme. When an anonymous user tries to access a protected resource, the application issues a challenge to the user using this scheme to sign in.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            // If ValidateAudience is true, the audience (aud) claim of the incoming token is validated against ValidAudience.
            ValidateAudience = true,

            // This is the expected audience value in the token. It should match the 'aud' claim in the JWT.
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // If ValidateIssuer is true, the issuer (iss) claim of the incoming token is validated against ValidIssuer.
            ValidateIssuer = true,

            // This is the expected issuer value in the token. It should match the 'iss' claim in the JWT.
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            // If ValidateLifetime is true, the lifetime of the incoming token is checked. The token must not be expired and its 'nbf' claim (not before) should be in the past.
            ValidateLifetime = true,

            // If ValidateIssuerSigningKey is true, the incoming token signature is validated with IssuerSigningKey.
            ValidateIssuerSigningKey = true,

            // This is the key used to validate the token signature. It must match the key used by the issuer to sign the JWT.
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Adds the authorization middleware to the dependency injection container, which evaluates the Authorization policy applied on controllers/actions.
builder.Services.AddAuthorization();





var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();   

app.UseSwagger(); //creates endpoint for swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0"); //Enables endpoints for each version
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0"); //Enables endpoints for each version

}); //creates swagger UI for testing all Web APi endpoints / action methods 


app.UseRouting();

app.UseCors(); //Enables asp.net core to include that response header called Access Control Allow Origin version automatically with the value called HTTP localhost 4200

app.UseAuthentication();

app.UseAuthorization();


app.UseAuthorization();

app.MapControllers();

app.Run();
