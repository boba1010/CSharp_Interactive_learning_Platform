# Codebase

#### Video Demo: `<URL HERE>`

## Description

> A gamified C# learning platform built with C#, WinUI 3, .NET, ASP.NET Core, and Roslyn.

Codebase is an interactive application designed to teach C# programming through lessons, coding challenges, battles, progression, achievements, and real-time code analysis.

Instead of simply presenting programming concepts and asking users to memorize syntax, Codebase turns C# programming into an interactive gameplay experience.

The player writes actual C# code to complete challenges, deal damage to enemies, progress through battles, earn experience, and unlock achievements.

The core of Codebase is its code-analysis system. Rather than attempting to interpret C# using custom string parsing, Codebase uses the **Microsoft .NET Compiler Platform (Roslyn)** to parse, validate, and analyze the player's code.

---

## Features

### ⚔️ Coding Battles

Coding battles are the primary gameplay mechanic of Codebase.

Players write C# code to deal damage to enemies. The code is analyzed and different programming constructs contribute to the amount of damage dealt.

The battle system currently supports concepts including:

* Variable declarations
* Data types
* Variable assignments
* Arithmetic expressions
* Arithmetic operators
* Increment and decrement operators
* Compound assignment operators
* String concatenation
* Syntax errors
* Semantic errors

Each battle defines its own requirements, allowing the same underlying analysis system to power challenges with different difficulty levels.

The player must write valid C# code while satisfying the requirements of the current battle.

---

## 🧠 Roslyn-Powered Code Analysis

Codebase uses the **Microsoft .NET Compiler Platform (Roslyn)** to analyze submitted C# code.

Instead of manually parsing source code as text, Codebase works directly with Roslyn's syntax trees, semantic models, and compiler diagnostics.

The analysis pipeline can:

1. Parse the submitted C# source code.
2. Retrieve compiler diagnostics.
3. Identify relevant syntax nodes.
4. Perform semantic validation.
5. Remove invalid constructs from normal damage evaluation.
6. Convert supported syntax into Codebase's internal statement model.
7. Calculate damage from recognized programming constructs.
8. Convert selected compiler errors into beginner-friendly feedback.

This allows the battle system to reason about actual C# syntax and semantics instead of relying on a custom approximation of the language.

---

## 🧩 Statement System

Recognized C# constructs are converted into structured Codebase statements.

For example, a variable declaration can become a `VariableDeclarationStatement`, while an assignment can become a `VariableAssignmentStatement`.

This gives the battle engine structured information about the player's code instead of requiring it to work directly with raw source text.

Supported statement types currently include:

```text
VariableDeclaration
VariableAssignment
```

Expressions can represent operators including:

```text
+
-
*
/
%
=
+=
-=
*=
/=
%=
++
--
```

This internal representation also allows the rest of the application to remain independent from the details of Roslyn's syntax nodes.

---

## 💥 Damage System

Programming actions contribute to the player's damage.

For example:

```csharp
int x = 10;
```

can award variable-declaration damage.

Likewise:

```csharp
x = x + 5;
```

can award assignment and mathematical-expression damage.

String concatenation is also recognized:

```csharp
string name = firstName + " " + lastName;
```

Each battle can configure its own damage multiplier, allowing the same programming concepts to have different effects depending on the challenge.

This creates a direct connection between writing code and gameplay:

```text
C# Code
   ↓
Code Analysis
   ↓
Recognized Constructs
   ↓
Damage
   ↓
Battle Result
```

---

## ❌ Error Damage

Incorrect code is not simply rejected.

Selected compiler errors can cause the player to take damage.

Codebase analyzes Roslyn diagnostics and maps supported errors to beginner-friendly explanations.

Examples include errors involving:

* Missing semicolons
* Unknown identifiers
* Invalid assignments
* Invalid operators
* Type conversion problems
* Invalid unary expressions
* Missing braces
* Missing parentheses
* Invalid syntax

For example, a missing semicolon can produce a friendly explanation containing the compiler error code and the relevant source location.

This makes compiler errors part of the gameplay loop while still exposing the player to real C# compiler diagnostics.

---

## 🧹 Semantic Validation

Codebase distinguishes between code that is syntactically valid and code that is actually semantically valid.

For example:

```csharp
string firstName = 100;
```

is valid C# syntax, but the assignment is semantically invalid because an integer cannot be implicitly assigned to a `string`.

Roslyn's semantic diagnostics allow Codebase to detect this distinction.

Invalid constructs are prevented from receiving normal programming damage.

This prevents players from accidentally receiving rewards for code that only *looks* structurally correct.

---

## 🏆 Achievements

Codebase includes an achievement system that rewards player actions and progression.

Achievements can be unlocked through gameplay actions and can provide rewards to the player.

The system is designed around explicit user actions rather than continuously checking every possible achievement condition.

This keeps achievement unlocking connected to actual gameplay events.

---

## 👤 Profile & Progression

Players can track their progression through their profile.

The application includes progression systems such as:

* Experience
* Levels
* Achievements
* Rewards
* Battle progression
* Leaderboards

Developer progression is represented through developer levels:

```text
Absolute Beginner
Beginner
Intermediate Memory User
Memory Manipulator
Memory Expert
```

---

## 🏅 Leaderboard

Codebase includes a leaderboard where players can compare their progression and accumulated points.

The leaderboard adds a competitive layer to the application while keeping programming itself at the center of the gameplay.

---

## 💻 Built-in Playground

Codebase includes a built-in C# playground for experimenting with code outside of battles.

The playground provides an interactive coding environment directly inside the application.

Roslyn is used for compilation and analysis, allowing the application to work with real C# code rather than a simulated programming language.

---

## 👾 The Final Boss

The final battle of Chapter 1, **The Beginning of Algorithms**, acts as a cumulative programming challenge.

The Final Boss does not introduce new syntax. Instead, it requires the player to combine the concepts learned throughout the chapter.

The player is required to:

* Declare multiple variables
* Use different data types
* Assign and reassign variables
* Perform mathematical calculations
* Use arithmetic operators
* Use increment and decrement operators
* Use compound assignment operators
* Use string concatenation
* Write multiple statements
* Produce valid C# code

The Final Boss is configured with:

```text
1000 HP
10× damage multiplier
10 required variable declarations
20 required assignments
```

This makes it a practical test of whether the player can combine the individual concepts introduced throughout the chapter.

---

## 🎮 Battle Progression

Chapter 1, **The Beginning of Algorithms**, contains nine battles.

The battles progressively introduce programming concepts and then combine them into increasingly complex challenges.

The progression can be summarized as:

```text
Learn
  ↓
Practice
  ↓
Apply
  ↓
Combine
  ↓
Final Challenge
```

The final battle acts as a capstone rather than introducing another isolated programming concept.

---

## 🔬 Damage Analysis Pipeline

When a player submits code during a battle, Codebase follows a pipeline similar to:

```text
Player Code
     │
     ▼
CSharpSyntaxTree.ParseText()
     │
     ├──────────────► Compiler Diagnostics
     │                       │
     │                       ▼
     │                  Error Damage
     │
     ▼
Syntax Analysis
     │
     ▼
Invalid Construct Filtering
     │
     ▼
Clean Syntax Tree
     │
     ▼
Semantic Model
     │
     ▼
SyntaxDamageEvaluator
     │
     ├── Variable declarations
     ├── Assignments
     ├── Math expressions
     ├── String concatenation
     ├── Increment/decrement
     └── Compound assignments
     │
     ▼
Structured Statements
     │
     ▼
Damage Calculation
     │
     ▼
Battle Result
```

The important distinction is that error analysis and normal damage analysis are handled separately.

This allows Codebase to report compiler errors while still evaluating the valid portions of the submitted program.

---

## 🏗️ Architecture

A simplified view of the application is:

```text
                    ┌─────────────────┐
                    │     WinUI 3     │
                    │       UI        │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │  ASP.NET Core   │
                    │     Backend     │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
       ┌────────────┐ ┌────────────┐ ┌────────────┐
       │   Battles  │ │ Achievements│ │ Progression│
       └──────┬─────┘ └────────────┘ └────────────┘
              │
              ▼
       ┌────────────────┐
       │ Damage Analysis │
       └───────┬────────┘
               │
               ▼
       ┌────────────────┐
       │     Roslyn     │
       │ Syntax/Semantic│
       │    Analysis    │
       └────────────────┘
```

The WinUI application provides the user interface while the ASP.NET Core backend handles application functionality exposed through the backend.

The battle and progression systems build on top of the underlying domain models and services.

Roslyn is used specifically where Codebase needs to understand and evaluate C# source code.

---

## 🖥️ Technology Stack

### Application

* **C#**
* **.NET**
* **WinUI 3**
* **Windows App SDK**

### Backend

* **ASP.NET Core**

### Code Analysis

* **Microsoft.CodeAnalysis**
* C# Syntax Trees
* Semantic Models
* Compiler Diagnostics
* `CSharpCompilation`

### Architecture

* MVVM-style application architecture
* Dependency Injection
* Data binding
* Structured domain models
* Messenger-based UI communication

---

## 📁 Project Structure

The solution is separated into several major projects:

```text
Codebase/
│
├── CSharp_Friendly_Compiler/
│
├── Codebase.API/
│
├── DamageCalculatorV2/
│
└── Codebase.WinUI/
```

### `CSharp_Friendly_Compiler`

Contains the compiler-related functionality used to provide friendlier feedback for selected C# compiler errors.

### `Codebase.API`

Contains the ASP.NET Core backend used by the application.

### `DamageCalculatorV2`

Contains the Roslyn-powered code-analysis and damage-calculation system.

This project is responsible for parsing and analyzing submitted C# code and converting recognized constructs into Codebase's internal representation.

### `Codebase.WinUI`

Contains the main Windows application and its WinUI 3 user interface.

---

## 🛠️ Development

Codebase was developed primarily in C#.

During development, the application went through multiple architectural iterations and was migrated from .NET MAUI to WinUI 3.

The final application focuses specifically on Windows and uses WinUI's native Windows UI capabilities.

One of the major technical changes during development was replacing manually implemented C# analysis logic with Roslyn.

This allowed the project to move from manually interpreting source code toward working directly with the C# language's syntax trees, semantic model, and compiler diagnostics.

The result is a more structured code-analysis pipeline that can be extended as additional C# constructs are introduced.

---

## 🚀 Running the Project

### Requirements

* Windows
* Visual Studio
* A .NET SDK compatible with the solution
* Windows App SDK / WinUI 3 workload
* C# development workload

### Build

Clone the repository and open the solution in Visual Studio.

Restore the NuGet packages, select the appropriate startup project, and build the solution.

Then run the application from Visual Studio.

---

## 🎓 CS50

Codebase was developed as a final project for **CS50**.

The project demonstrates several areas of software development, including:

* C# application development
* WinUI development
* ASP.NET Core development
* Compiler technology
* Roslyn syntax analysis
* Semantic analysis
* Game mechanics
* Application architecture
* Data binding
* User progression systems
* Error handling
* UI/UX design

The project combines these technologies around one central idea:

> **Learn C# by actually writing C#.**

---

## 👨‍💻 Author

**Boba**

Built with C#, .NET, WinUI 3, Roslyn, and a lot of compiler errors.