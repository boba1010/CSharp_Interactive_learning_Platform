using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.API.Services;
using CSharp_Interactive_Learning_App.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AppDb"));

builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<IBattleService, BattleService>();

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

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

List<Battle> chapter1_battles =
[
    new()
    {
        Name = "What is a variable?",
        Content = "A variable in programming is a named, symbolic container used to store, retrieve, and update data in a computer’s memory during program execution.\nThink of it as a labeled box that holds information (numbers, text, etc.), allowing developers to manipulate data using descriptive names rather than raw memory addresses.",
        Instructions = "Your mission is to declare two variables using two of the following datatypes: 'int' it's values are integers, 'double' it's values are decimals, 'float' same as 'double' but with lower precision, 'bool' it's values are true/false, 'char' it's value is a single character inside '' (single quotes), 'string' it's value is any text inside \"\" (double quotes)\n\nSYNTAX: 'datatype' 'variable name' = 'value'; \nEXAMPLE: int i = 0;",
        EnemiesNumber = 2,
        HealthPerEnemy = 5,
        DmgMultiplier = 1,
        AllowedTypes = [.. Enum.GetValues<DataType>()],
        Points = 700,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.VariableDeclaration,
                Count = 2
            },
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
        Points = 1800,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.VariableDeclaration,
                Count = 4
            }
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()],
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
        Points = 2000,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.VariableDeclaration,
                Count = 5
            }
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()],
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
5. Declare 4 variables, then reassign each variable using another variable's value",
        EnemiesNumber = 3,
        HealthPerEnemy = 10,
        DmgMultiplier = 1,
        Points = 1200,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.VariableDeclaration,
                Count = 4
            },
            new()
            {
                AllowedStatementType = StatementType.VariableAssignment,
                Count = 5
            },
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()],
    },
    new()
    {
        Name = "Math Operations (Math expressions)",
        Content =
@"Math expressions allow you to perform calculations using variables, values, and mathematical operators.

The basic arithmetic operators in C# are:
+ Addition
- Subtraction
* Multiplication
/ Division
% Remainder

You can combine multiple operators in the same expression:

int result = 10 + 5 * 2;

C# follows the normal mathematical order of operations, so multiplication and division are performed before addition and subtraction.

You can use parentheses to control the order:

int result = (10 + 5) * 2;

You can also use variables inside expressions:

int a = 10;
int b = 5;
int result = a + b;

Remember that integer division produces an integer result:

int result = 7 / 2;

The result is 3 because both values are integers.",
        Instructions =
@"Follow these rules to survive the battle:
1. Declare the required variables first
2. Use mathematical operators to create expressions
3. Use +, -, *, /, and % correctly
4. You may combine multiple operators in one expression
5. Use parentheses when necessary
6. End every statement with ;
7. Create 2 numeric variables then assign them with a math expression",
        Points = 800,
        EnemiesNumber = 5,
        HealthPerEnemy = 10,
        DmgMultiplier = 2.5,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.VariableDeclaration,
                Count = 2
            },
            new()
            {
                AllowedStatementType = StatementType.VariableAssignment,
                Count = 2
            },
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()],
        AllowedOperations = [.. Enum.GetValues<OperationType>()]
    },
    new()
    {
        Name = "Increments And Shortcuts",
        Content =
@"C# provides shorthand operators for modifying existing variables.

The increment operator ++ increases a variable by 1:

int score = 10;
score++;

The decrement operator -- decreases a variable by 1:

int lives = 3;
lives--;

You can also use compound assignment operators:

score += 10;
score -= 5;
score *= 2;
score /= 2;
score %= 2;

These are shortcuts for writing:

score = score + 10;
score = score - 5;
score = score * 2;
score = score / 2;
score = score % 2;

These operators are especially useful when you need to repeatedly update the value of a variable.",
        Instructions =
@"Follow these rules to survive the battle:
1. Declare the required variables first
2. Use ++ to increase a variable by 1
3. Use -- to decrease a variable by 1
4. Use +=, -=, *=, /=, or %= when appropriate
5. Only modify variables that have already been declared
6. End every statement with ;
7. Perform the required variable updates",
        Points = 500,
        EnemiesNumber = 3,
        HealthPerEnemy = 8,
        DmgMultiplier = 1,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.VariableDeclaration,
                Count = 2
            },
            new()
            {
                AllowedStatementType = StatementType.VariableAssignment,
                Count = 2
            },
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()],
        AllowedOperations = [.. Enum.GetValues<OperationType>()]
    },
    new()
    {
        Name = "Output And Input",
        Content =
@"Programs often need to communicate with the user.

You can display text in the console using Console.WriteLine():

Console.WriteLine(""Hello World!"");

Output: Hello World!

You can also display the value of a variable:

int score = 100;
Console.WriteLine(score);

Output: 100

Console.Write() works similarly, but it does not automatically move to a new line:

Console.Write(""Hello "");

Console.Write(""World!"");

Output: Hello World!

You can receive input from the user using Console.ReadLine():

string name = Console.ReadLine();

Console.ReadLine() returns the text entered by the user.

You can combine input with variables:

Console.WriteLine(""What is your name?"");
string name = Console.ReadLine();

Console.WriteLine(name);

Output: The value entered by the user

Remember that Console.ReadLine() returns a string, so additional conversion is required when you want to use the input as another datatype.

You can test yourself in the Playground",
        Instructions = "Type anything to complete this battle.",
        Points = 10,
        EnemiesNumber = 0,
        HealthPerEnemy = 0,
        DmgMultiplier = 1,
        RequiredStatements = []
    },
    new()
    {
        Name = "Concatenation",
        Content =
@"Concatenation means combining multiple pieces of text into one string.

In C#, strings can be concatenated using the + operator:

string firstName = ""John"";
string lastName = ""Doe"";
string fullName = firstName + "" "" + lastName;

The + operator can also combine strings with other values:

int score = 100;
string message = ""Score: "" + score;

You can use concatenation with Console.WriteLine():

string name = ""John"";
Console.WriteLine(""Hello "" + name);

When a string is combined with another value using +, C# converts the value into text as part of the resulting string.

You can test and see the result in the Playground",
        Instructions =
@"Follow these rules to survive the battle:
1. Declare the required variables first
2. Use the + operator to concatenate strings
3. Combine variables and text when required
4. Use quotation marks correctly around text
5. You may concatenate multiple values together
6. End every statement with ;
7. Create the required concatenated strings",
        Points = 200,
        EnemiesNumber = 2,
        HealthPerEnemy = 10,
        DmgMultiplier = 1,
        RequiredStatements =
        [
            new()
            {
                AllowedStatementType = StatementType.VariableDeclaration,
                Count = 2
            },
            new()
            {
                AllowedStatementType = StatementType.VariableAssignment,
                Count = 2
            },
        ],
        AllowedTypes = [.. Enum.GetValues<DataType>()],
        AllowedOperations = [.. Enum.GetValues<OperationType>()]
    },
    new()
    {
        Name = "Final Boss",
        Content =
@"You have reached the Final Boss.

This battle combines everything you have learned so far.

You will need to use:
- Variables
- Data types
- Variable declarations
- Variable assignments
- Mathematical expressions
- Arithmetic operators
- Increment and decrement operators
- Compound assignment operators
- String concatenation

There is no new syntax to learn here. The challenge is to combine the concepts you have already mastered into one working program.

Think carefully about the order of your statements.

Declare your variables before using them.
Assign values when necessary.
Perform the required calculations.
Use the results in your output.
Combine text and values using concatenation.

The Final Boss will test whether you can actually use C# rather than simply remember individual pieces of syntax.",
        Instructions =
@"FINAL BOSS RULES:

1. Declare all required variables correctly
2. Use the required datatypes
3. Assign and reassign variables when necessary
4. Perform the required mathematical calculations
5. Use arithmetic operators correctly
6. Use increment, decrement, or compound assignment operators when required
7. Use string concatenation when constructing output
8. End every statement with ;
9. Follow the required statement counts
10. Your code must combine multiple concepts from the previous lessons

Defeat the Final Boss by putting everything you have learned together.",
        Points = 3000,
        EnemiesNumber = 1,
        HealthPerEnemy = 1000,
        DmgMultiplier = 5,
    },
];

List<Chapter> chapters =
[
    new()
    {
        Id = 1,
        Battles = chapter1_battles,
        Name = "Variables",
    },
];


foreach (Chapter chapter in chapters)
    context.Chapters.Add(chapter);

List<DeveloperLevel> levels =
[
    new(1, 200, CSharp_Interactive_Learning_App.API.Enums.DevLevel.AbsoluteBeginner),
    new(2, 500, CSharp_Interactive_Learning_App.API.Enums.DevLevel.Beginner),
    new(3, 1000, CSharp_Interactive_Learning_App.API.Enums.DevLevel.Intermediate),
    new(4, 2500, CSharp_Interactive_Learning_App.API.Enums.DevLevel.AdvancedIntermediate),
    new(5, 10000, CSharp_Interactive_Learning_App.API.Enums.DevLevel.Advanced)
];

foreach (var level in levels)
    context.DeveloperLevels.Add(level);

List<Achievement> achievements = 
[
    new() { Name = "Welcome to <Codebase/>!", Description = "Unlocked by creating a new account", RewardPoints = 1000 },
    new() { Name = "OFFLINE COOOOOOOOOOOOOOOOOOOOOODING!", Description = "Unlocked by running any code in the Playground (Low Reward Achievement)", RewardPoints = 10 },
    new() { Name = "Offline secret: Low reward achievements", Description = "Unlocked by Unlocking any offline achievements", RewardPoints = 1000 },
    new() { Name = "DATA TYPES!", Description = "Unlocked by surviving the Data types battle", RewardPoints = 200 },
    new() { Name = "IT'S NOT THAT HARD!", Description = "Unlocked by getting defeated in a battle", RewardPoints = 120 },
    new() { Name = "NOW, YOU'RE AN ABSOLUTE BEGINNER!", Description = "Unlocked by reaching lv1 (Absolute beginner)", RewardPoints = 1500 },
    new() { Name = "You don't wanna quit, do you?", Description = "Unlocked by reaching lv2 (Beginner)", RewardPoints = 2000 },
    new() { Name = "Now, we're talking!", Description = "Unlocked by reaching lv3 (Intermediate)", RewardPoints = 5000 },
    new() { Name = "You're so close!", Description = "Unlocked by reaching lv4 (Advanced Intermediate)", RewardPoints = 10_000 },
    new() { Name = "Unknown", Description = "Unlocked by reaching the first boss", RewardPoints = 1000 },
    new() { Name = "Memory manipulator", Description = "Unlocked by completing the variable declaration and assignment battles", RewardPoints = 3800 },
    new() { Name = "Chapter 1 conquerer", Description = "Unlocked by completing chapter 1 completely", RewardPoints = 7000 },
];

foreach (var achievement in achievements)
    context.Achievements.Add(achievement);

await context.SaveChangesAsync();

app.UseRouting();

app.UseCors("BlazorWasmPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
