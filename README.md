# SkeletonsAdventure

A custom-built 2D RPG and modular game engine developed in C# using MonoGame. This project includes a reusable engine framework, custom tooling, and an in-house editor designed to support scalable RPG development and rapid content iteration.

📌 **Project Overview**

Skeleton’s Adventure is a personal software engineering project focused on building a complete RPG system from the ground up. Rather than using a pre-built engine, the project emphasizes custom architecture, performance-conscious design, and long-term maintainability.

The codebase consists of multiple interconnected projects, including a core engine library, game logic, and development tools. It demonstrates real-world software engineering practices such as modular design, refactoring, and version control.

✨ **Key Features**

Custom 2D game engine built with MonoGame

Modular, object-oriented architecture

Component-based and system-driven design

Integrated level and asset editor

Custom save/load and serialization system

Tile-based world system

Animation and sprite management

Combat and NPC interaction systems

Game-state and scene management

Configuration-driven content loading

Git-based version control and iterative development

🏗️ **Architecture**

The project is organized into multiple layers to promote separation of concerns and reusability:

SkeletonsAdventure/
│
├── Engine/          # Core rendering, input, and system logic
├── Game/            # Gameplay systems and RPG mechanics
├── Editor/          # Custom map and asset editor
├── Libraries/       # Shared utilities and frameworks
├── Content/         # Game assets and configuration files
└── Data/            # Save files and serialized data
**Core Subsystems**

Rendering System

Input Handling

Entity and Component System

State Management

Asset Pipeline

Save/Load System

AI and NPC Logic

Collision and Interaction System

Each subsystem is designed to operate independently while communicating through well-defined interfaces.

🛠️ **Technologies Used**

Language: C#

Framework: MonoGame

IDE: Visual Studio

Version Control: Git / GitHub

Data Formats: JSON / Custom Serialization

Platform: Windows

🚀 **Getting Started**
Prerequisites

Windows 10 or later

.NET SDK

Visual Studio (recommended)

MonoGame Framework

**Installation**

Clone the repository:

git clone https://github.com/felker12/SkeletonsAdventure.git

Open the solution in Visual Studio.

Restore NuGet packages if prompted.

Build the solution.

Run the main game project.

🎮 **Using the Editor**
The project includes a custom-built editor to support rapid development and data creation.

Editor Capabilities:

Create Item Data

Create Entity Data

Create Quest Data

📊 **Performance and Optimization**

This project emphasizes performance-aware design and scalability:

Object pooling for frequently created entities

Efficient sprite batching

Optimized asset loading

Reduced memory allocations

Targeted refactoring of performance-critical systems

Profiling and optimization were applied throughout development to maintain stable frame rates as the project grew.

📚 **Development Practices**

Modular and layered architecture

Separation of engine and game logic

Consistent naming conventions

Regular refactoring

Feature-based branching

Incremental development

Documentation-first approach for major systems

📈 **Learning Objectives**

This project was created to strengthen skills in:

Large-scale C# application development

Engine architecture and system design

Performance optimization

Tool development

Software maintainability

Debugging complex systems

Data-driven design

Version control workflows

🔮 **Future Improvements**

Advanced AI behavior trees

Multithreaded simulation systems

Expanded modding support

Improved UI/UX

Cross-platform deployment

Automated testing framework

📄 **License**

This project is released for educational and portfolio purposes. All rights reserved unless otherwise specified.

🤝 **Contributing**

This is currently a personal portfolio project. Feedback, suggestions, and code reviews are welcome.
