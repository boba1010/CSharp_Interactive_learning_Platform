# Codebase

#### Video Demo: <URL HERE>

## Description

> A gamified C# learning platform built with C#, WinUI 3, .NET, and Roslyn.

Codebase is an interactive learning application designed to teach C# programming through a combination of lessons, coding challenges, battles, progression, achievements, and real-time code analysis.

Instead of simply presenting syntax and asking the user to memorize it, Codebase turns programming concepts into interactive gameplay.

---

## Features

### ⚔️ Coding Battles

Codebase's primary learning mechanic is its battle system.

Players write C# code to deal damage to enemies. Different programming constructs contribute to the amount of damage dealt.

The battle system supports concepts including:

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

The player must write valid C# code while satisfying the requirements of each battle.

---

## 🧠 Roslyn-Powered Code Analysis

Codebase uses the **Microsoft Roslyn compiler platform** to analyze the player's code.

Rather than relying on string parsing or manually implemented syntax rules, Codebase parses the submitted C# code into a syntax tree and analyzes it using Roslyn's syntax and semantic APIs.

The analysis pipeline can:

1. Parse the submitted C# source code.
2. Detect compiler diagnostics.
3. Identify relevant syntax nodes.
4. Perform semantic validation.
5. Convert supported syntax into Codebase's internal statement model.
6. Calculate damage based on the detected programming constructs.
7. Convert relevant compiler errors into beginner-friendly feedback.

This allows Codebase to work with actual C# syntax rather than a custom approximation of the language.

---

## 🧩 Statement System

Codebase converts recognized C# constructs into an internal representation.

For example, a variable declaration can become a `VariableDeclarationStatement`, while an assignment can become a `VariableAssignmentStatement`.

This provides the battle engine with structured information instead of requiring it to work directly with raw source code.

Supported statement concepts include:

```text
VariableDeclaration
VariableAssignment
```

Expressions can also represent operators such as:

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

---

## 💥 Damage System

Programming actions contribute to the player's damage.

For example:

```csharp
int x = 10;
```

can award declaration damage.

Likewise:

```csharp
x = x + 5;
```

can award assignment and mathematical-expression damage.

String concatenation is also recognized:

```csharp
string name = firstName + " " + lastName;
```

The final damage is affected by the battle's configured damage multiplier.

This allows different battles to use the same underlying code-analysis system while having completely different difficulty levels.

---

## 👾 The Final Boss

The final battle of Chapter 1 is designed as a programming capstone.

The Final Boss combines every concept introduced throughout the chapter.

The player is required to:

* Declare multiple variables
* Use different data types
* Assign and reassign variables
* Perform mathematical calculations
* Use arithmetic operators
* Use increment/decrement operators
* Use compound assignments
* Use string concatenation
* Write multiple statements
* Produce valid C# code

The Final Boss has:

```text
1000 HP
10× damage multiplier
10 required variable declarations
20 required assignments
```

There is no new syntax introduced in the Final Boss. The challenge is combining previously learned concepts into one working program.

---

## ❌ Error Damage

Incorrect code is not simply rejected.

Relevant compiler errors can cause the player to take damage.

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

This turns compiler errors into another part of the gameplay loop while still exposing the player to real C# compiler feedback.

---

## 🧹 Invalid Syntax Handling

Codebase separates invalid code from valid code during damage calculation.

If a syntax node contains a relevant semantic error, it is not awarded normal programming damage.

For example:

```csharp
string firstName = 100;
```

is syntactically valid C#, but the assignment is semantically invalid.

Roslyn can identify the type mismatch, allowing Codebase to avoid treating the invalid declaration as valid learning progress.

---

## 🏆 Achievements

Codebase includes an achievement system that rewards player actions and progression.

Achievements can be unlocked through gameplay actions and provide rewards to the player.

The system is designed around user actions rather than simply checking arbitrary conditions every frame.

---

## 👤 Profile & Progression

Players can track their progress through their profile.

The application includes progression-related systems such as:

* Experience
* Levels
* Achievements
* Rewards
* Battle progression
* Leaderboards

Developer progression is represented through developer levels such as:

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

This adds a competitive element to the learning experience without changing the core programming-focused gameplay.

---

## 💻 Built-in Playground

Codebase includes a built-in coding playground.

The playground provides an environment where users can experiment with C# without necessarily being inside a battle.

The project uses a code editor and Roslyn-based compilation to provide an interactive coding experience directly inside the application.

---

## 🖥️ Technology Stack

### Application

* **C#**
* **.NET**
* **WinUI 3**
* **Windows App SDK**

### Code Analysis

* **Microsoft.CodeAnalysis (Roslyn)**
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

## 🏗️ Architecture

A simplified representation of the application is:

```text
                    ┌─────────────────┐
                    │     WinUI 3     │
                    │       UI        │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │  Application    │
                    │    Logic        │
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
     │                Error Damage
     │
     ▼
Syntax Validation
     │
     ▼
Invalid Syntax Removal
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

---

## 🎮 Battle Progression

Chapter 1, **The Beginning of Algorithms**, contains nine battles.

The battles gradually introduce and combine programming concepts.

The final battle acts as a cumulative challenge rather than introducing another isolated feature.

The progression follows the general idea:

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

---

## 🛠️ Development

Codebase was developed primarily in C#.

During development, the project went through multiple architectural iterations and was migrated from .NET MAUI to WinUI 3.

The final application focuses specifically on the Windows platform and takes advantage of WinUI's native Windows UI capabilities.

A major part of the project was replacing manually implemented code-analysis logic with Roslyn.

This significantly simplified the implementation of C# syntax analysis while making the evaluator more accurate and extensible.

---

## 📁 Project Structure

A simplified structure looks like:

```text
Codebase/
├── App/
│   ├── Views/
│   ├── ViewModels/
│   └── Services/
│
├── Battles/
│   ├── Models/
│   ├── Evaluators/
│   └── Services/
│
├── DamageCalculator/
│   ├── Models/
│   ├── SyntaxDamageEvaluator.cs
│   ├── ErrorDamageEvaluator.cs
│   └── InvalidSyntaxRemover.cs
│
├── Achievements/
├── Profile/
├── Leaderboard/
├── Playground/
└── Resources/
```

The exact structure may differ depending on the current project version.

---

## 🚀 Running the Project

### Requirements

* Windows
* Visual Studio
* .NET SDK compatible with the project
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
* Compiler technology
* Roslyn syntax analysis
* Semantic analysis
* Game mechanics
* Application architecture
* Data binding
* User progression systems
* Error handling
* UI/UX design

The central idea behind Codebase is simple:

> **Learn C# by actually writing C#.**

---

## 👨‍💻 Author

**Boba**

Built with C#, .NET, WinUI 3, and a lot of compiler errors.