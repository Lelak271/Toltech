using Toltech.Solver.Contracts;

namespace Toltech.App.ToltechCalculation;

public static class ComputeEngineFactory
{
    public static IComputeEngine Create()
    {
#if REAL_SOLVER
        return RealComputeEngineFactory.Create();
#else
        return MockComputeEngineFactory.Create();
#endif
    }
}