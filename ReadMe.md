# Toltech

Toltech is a desktop application for mechanical tolerance analysis and variability calculation.

The project provides tools to model mechanical assemblies, define tolerances and functional requirements, and analyse their influence on assembly behaviour.

## Features

- Mechanical assembly modelling
- Parts and mechanical linkages management
- Geometrical tolerance definition
- Functional requirements definition
- Tolerance and variability calculations
- Graph-based mechanical modelling
- Small-displacement torsor modelling
- Calculation result visualisation
- 3D visualisation
- SQLite-based model storage
- JSON result persistence
- CAD integration
- FreeCAD integration
- Multilingual user interface

## Screenshots

<!-- Add application screenshots here -->

## Architecture

Toltech is organised into several components:

```text
Toltech
│
├── Toltech.App
│   └── WPF application
│
├── Toltech.ComputeEngine.Contracts
│   └── Solver contracts and DTOs
│
├── Toltech.Solver
│   └── Mechanical and mathematical calculations
│
├── Toltech.Cad
│   └── Generic CAD abstraction
│
└── Toltech.FreeCAD
    └── FreeCAD integration
```
The WPF application communicates with the calculation engine through dedicated contracts and data transfer objects.

The solver is designed to remain independent from the WPF user interface.

## Technology

- C#
- .NET 8
- WPF
- MVVM
- SQLite
- JSON
- HelixToolkit.Wpf
- FreeCAD
- CSparse
- MathNet.Numerics

### Solver

The `Toltech.Solver` project targets .NET 8 and provides the calculation engine used by Toltech.

The solver is currently not publicly available. To allow the application to be used and demonstrated without access to the calculation engine, Toltech provides a `FakeSolver` implementation that simulates solver responses with realistic data.

The solver currently uses:

- [CSparse](https://www.nuget.org/packages/CSparse/) 4.2.0
- [MathNet.Numerics](https://www.nuget.org/packages/MathNet.Numerics/) 5.0.0

The solver is also configured to generate a NuGet package when the project is built.

## Getting Started

### Requirements

- Windows
- .NET 8 SDK
- Visual Studio 2022 or compatible .NET development environment
- FreeCAD for CAD-related features

### Build

Clone the repository:

```bash
git clone <repository-url>
cd Toltech
```

Restore the dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

To build the solver independently:

```bash
dotnet build Toltech.Solver
```

The `Toltech.Solver` project is configured to generate a NuGet package when the project is built.

### Run

Start the WPF application from Visual Studio, or use:

```bash
dotnet run --project Toltech.App
```

## Documentation

Technical documentation is available in the project documentation.

<!-- Add documentation link here -->

## Contributing

Contributions, bug reports and suggestions are welcome.

Please ensure that the solution builds successfully before submitting changes.

<!-- Add contribution guidelines here -->

## License

<!-- Add license information here -->

See the `LICENSE` file for details.

## Status

Toltech is currently under development.