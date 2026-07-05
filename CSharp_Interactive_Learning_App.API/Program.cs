using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AppDb"));

builder.Services.AddSingleton<AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasmPolicy", policy =>
    {
        policy.WithOrigins("https://csharp-gamified-learning-platform-blazor.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Convert.FromBase64String(Environment.GetEnvironmentVariable("JWT_KEY")!)
        ),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"FAIL: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5000);
//    options.ListenAnyIP(5001, listenOptions =>
//    {
//        listenOptions.UseHttps(); // Automatically loads the dotnet dev-cert
//    });
//});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

List<Battle> chapter1_battles = 
[
    new()
    {
        Name = "What is a variable?",
        Content = "A variable in programming is a named, symbolic container used to store, retrieve, and update data in a computer’s memory during program execution.\nThink of it as a labeled box that holds information (numbers, text, etc.), allowing developers to manipulate data using descriptive names rather than raw memory addresses.",
        Instructions = "Your mission is to declare two variables using two of the following datatypes: 'int' it's values are integers, 'double' it's values are decimals, 'float' same as 'double', 'bool' it's values are true/false, 'char' it's value is a single character inside '' (single quotes), 'string' it's value is any text inside \"\" (double quotes)\n\nSYNTAX: 'datatype' 'variable name' = 'value'; \nEXAMPLE: int i = 0;",
        EnemiesNumber = 2,
        HealthPerEnemy = 5,
        DmgMultiplier = 1,
        AllowedTypes = [.. Enum.GetValues<DataType>()
            .Select(t => new BattleAllowedTypes
            {
                AllowedType = t
            })],
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.Variable,
                Count = 2
            }
        ],
    },
    new()
    {
        Name = "Data types",
        Content =
@"DataTypes are used to specifiy the variable type. 
The basic DataTypes are int (integer), float (decimal), double (percise decimal), bool (a true or false value), 
char (character) you can only store a single character in a variable of datatype char and it has to be inside '' (quotes), 
string (any text) you can store anything inside a string but it has to be inside '""' (double quotes).",
        Instructions =
@"Follow these rules to survive the battle:
1. Declare variables using: type name = value;
2. Use correct datatype (int, string, bool, etc.)
3. End every line with ;
4. No invalid names allowed
5. Declare 4 variables",
        EnemiesNumber = 5,
        HealthPerEnemy = 4,
        DmgMultiplier = 0.8,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.Variable,
                Count = 4
            }
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()
            .Select(t => new BattleAllowedTypes
            {
                AllowedType = t
            })],
    },
    new()
    {
        Name = "Variable Declaration Syntax Rules",
        Content =
@"A variable declaration in C# follows a strict structure:

'datatype' 'variableName' = 'value';

Rules:
1. You must write the datatype first (int, string, bool, etc.)
2. Then the variable name
3. Then '=' followed by the value
4. Every statement must end with a semicolon ';'
5. Variable names cannot start with numbers
6. Variable names cannot contain spaces
7. Each variable must be declared on its own line",
        Instructions =
@"Follow these rules to survive the battle:
1. Declare variables using: type name = value;
2. Use correct datatype (int, string, bool, etc.)
3. End every line with ;
4. No invalid names allowed
5. Declare 5 variables",
        EnemiesNumber = 5,
        HealthPerEnemy = 5,
        DmgMultiplier = 1,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.Variable,
                Count = 5
            }
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()
            .Select(t => new BattleAllowedTypes
            {
                AllowedType = t
            })],
    },
    new()
    {
        Name = "Variable Assignment Syntax Rules",
        Content =
@"A variable assignment in C# follows a strict structure:

'variableName' = 'value';

Rules:
1. The variable must be declared in an outer or same scope
2. the variable name
3. Then '=' followed by the value
4. Every statement must end with a semicolon ';'",
        Instructions =
@"Follow these rules to survive the battle:
1. Declare variables using: type name = value;
2. Use correct datatype (int, string, bool, etc.)
3. End every line with ;
4. No invalid names allowed
5. declare 3 variables then reassign them",
        EnemiesNumber = 3,
        HealthPerEnemy = 10,
        DmgMultiplier = 1,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.Variable,
                Count = 3
            }
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()
            .Select(t => new BattleAllowedTypes
            {
                AllowedType = t
            })],
    },
    new()
    {
        Name = "Math Operations (Math expressions)",
        Content = "",
        Instructions = "",
    },
    new()
    {
        Name = "Increments And Shortcuts",
        Content = "",
        Instructions = "",
    },
    new()
    {
        Name = "Output And Input",
        Content = "",
        Instructions = "",
    },
    new()
    {
        Name = "Concatenation",
        Content = "",
        Instructions = "",
    },
    new()
    {
        Name = "Final Boss",
        Content = "",
        Instructions = "",
    },
];

//List<Battle> chapter2_battles =
//[
//    // unit 2
//    new()
//    {
//        Name = "What is a condition?",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "Binary Expressions",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "Unary Expressions",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "Expression Syntax Rules",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "'if', 'else if' And 'else' keywords",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "'switch' and 'break' keywords",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "What is an algorithm?",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "Implementing a mini algorithm!",
//        Content = "",
//        Instructions = "",
//    },
//    new()
//    {
//        Name = "Final Boss",
//        Content = "",
//        Instructions = "",
//    },
//];

List<Chapter> chapters =
[
    new()
    {
        Id = 1,
        Battles = chapter1_battles,
        Name = "Variables",
    },
    //new()
    //{
    //    Id = 2,
    //    Battles = chapter2_battles,
    //    Name = "Conditions"
    //},
];


foreach (Chapter chapter in chapters)
    context.Chapters.Add(chapter);
await context.SaveChangesAsync();

// Configure the HTTP request pipeline.

app.UseRouting();

app.UseCors("BlazorWasmPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
