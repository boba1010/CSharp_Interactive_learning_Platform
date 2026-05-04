using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AppDb"));

builder.Services.AddSingleton<AuthService>();

builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/admin";
        options.AccessDeniedPath = "/admin";
    });

builder.Services.AddAuthentication("UserCookie")
    .AddCookie("UserCookie", options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
    });

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

var app = builder.Build();

app.UseStaticFiles();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

List<Unit> units =
[
    new()
    {
        Id = 1,
        LessonIds = [1, 2, 3, 4, 5, 6, 7, 8, 9],
        Name = "Variables"
    },
    new()
    {
        Id = 2,
        LessonIds = [10, 11, 12, 13, 14, 15, 16, 17, 18],
        Name = "Conditions"
    },
];

List<Battle> battles =
[
    // unit 1
    new() 
    { 
        Name = "What is a variable?",
        Content = "A variable in programming is a named, symbolic container used to store, retrieve, and update data in a computer’s memory during program execution.\nThink of it as a labeled box that holds information (numbers, text, etc.), allowing developers to manipulate data using descriptive names rather than raw memory addresses.\nSYNTAX: 'datatype' 'variable name' = 'value'; \nEXAMPLE: int i = 0;", 
        Instructions = "Type code to win the battle.\nCorrect code will deal a specific amount of damage to the enemies.\nCompiler errors will decrease your health points.\nYour mission is to declare two variables using two of the following datatypes: 'int' it's values are integers, 'double' it's values are decimals, 'float' same as 'double', 'bool' it's values are true/false, 'char' it's value is a single character inside '' (single quotes), 'string' it's value is any text inside \"\" (double quotes)", 
        Id = 1, 
        EnemiesNumber = 2,
        HealthPerEnemy = 5, 
        DmgMultiplier = 1
    },
    new() 
    { 
        Name = "Data types", 
        Content = "DataTypes are used to specifiy the variable type. The basic DataTypes are int (integer), float (decimal), double (percise decimal), bool (a true or false value), char (character) you can only store a single character in a variable of datatype char and it has to be inside '' (quotes), string (any text) you can store anything inside a string but it has to be inside '\"\"' (double quotes).", 
        Instructions = "Type code to win the battle. Correct code will deal a specific amount of damage to the enemies. Compiler, syntax, logic errors will decrease your health points. Your mission is to declare five variables using any of the following datatypes: 'int' it's values are integers, 'double' it's values are decimals, 'float' same as 'double', 'bool' it's values are true/false, 'char' it's value is a single character inside '' (single quotes), 'string' it's value is any text inside \"\" (double quotes)", 
        Id = 2, 
        EnemiesNumber = 5, 
        HealthPerEnemy = 4, 
        DmgMultiplier = 0.8
    },
    new() 
    { 
        Name = "Variable Declaration Syntax Rules", 
        Content = "", 
        Instructions = "",
        Id = 3, 
        EnemiesNumber = 4, 
        HealthPerEnemy = 5, 
        DmgMultiplier = 1
    },
    new() 
    { 
        Name = "Variable Assignment Syntax Rules", 
        Content = "", 
        Instructions = "", 
        Id = 4, 
        EnemiesNumber = 3, 
        HealthPerEnemy = 2,
    },
    new() 
    { 
        Name = "Math Operations (Math expressions)", 
        Content = "", 
        Instructions = "", 
        Id = 5, 
    },
    new() 
    { 
        Name = "Increments And Shortcuts", 
        Content = "", 
        Instructions = "", 
        Id = 6, 
    },
    new() 
    {
        Name = "Output And Input",
        Content = "",
        Instructions = "",
        Id = 7, 
    },
    new() 
    { 
        Name = "Concatenation",
        Content = "",
        Instructions = "",
        Id = 8, 
    },
    new()
    { 
        Name = "Final Boss (Review1)", 
        Content = "", 
        Instructions = "", 
        Id = 9, 
    },
    // unit 2
    new() 
    { 
        Name = "What is a condition?",
        Content = "", 
        Instructions = "", 
        Id = 10, 
    },
    new() 
    { 
        Name = "Binary Expressions", 
        Content = "", 
        Instructions = "", 
        Id = 11, 
    },
    new() 
    { 
        Name = "Unary Expressions", 
        Content = "", 
        Instructions = "", 
        Id = 12, 
    },
    new() 
    { 
        Name = "Expression Syntax Rules", 
        Content = "", 
        Instructions = "", 
        Id = 13, 
    },
    new() 
    { 
        Name = "'if', 'else if' And 'else' keywords", 
        Content = "", 
        Instructions = "", 
        Id = 14, 
    },
    new() 
    { 
        Name = "'switch' and 'break' keywords", 
        Content = "", 
        Instructions = "", 
        Id = 15, 
    },
    new() 
    { 
        Name = "What is an algorithm?",
        Content = "",
        Instructions = "", 
        Id = 16, 
    },
    new() 
    { 
        Name = "Implementing a mini algorithm!", 
        Content = "",
        Instructions = "", 
        Id = 17, 
    },
    new() 
    { 
        Name = "Final Boss (Review2)", 
        Content = "", 
        Instructions = "", 
        Id = 18, 
    },
];

foreach (Unit unit in units)
    context.Units.Add(unit);
foreach (Battle battle in battles)
    context.Battles.Add(battle);
await context.SaveChangesAsync();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
