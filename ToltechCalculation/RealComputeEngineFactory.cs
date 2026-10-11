#if REAL_SOLVER

using Toltech.Solver;
using Toltech.Solver.Contracts;

namespace Toltech.App.ToltechCalculation;

internal static class RealComputeEngineFactory
{
    public static IComputeEngine Create()
    {
        return Solver.ComputeEngineFactory.Create();
    }
}

#endif