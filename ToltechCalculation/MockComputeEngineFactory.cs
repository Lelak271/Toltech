#if !REAL_SOLVER

using Toltech.Solver.Mock;
using Toltech.Solver.Contracts;

namespace Toltech.App.ToltechCalculation;

internal static class MockComputeEngineFactory
{
    public static IComputeEngine Create()
    {
        return new Solver.Mock.MockComputeEngine();
    }
}

#endif