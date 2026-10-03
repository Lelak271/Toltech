using Toltech.Solver.Contracts;
using Toltech.Solver.Mock;

namespace Toltech.App.ToltechCalculation
{
    /// <summary>
    /// Factory utilisée par l'application pour sélectionner
    /// le moteur de calcul à utiliser.
    /// </summary>
    public static class ComputeEngineFactory
    {
        /// <summary>
        /// Crée le moteur de calcul réel.
        /// </summary>
        /// <returns>
        /// Une implémentation réelle de <see cref="IComputeEngine"/>.
        /// </returns>
        public static IComputeEngine CreateReal()
        {
            // La construction du moteur réel est déléguée
            // à la factory interne du projet Toltech.Solver.
            return Solver.ComputeEngineFactory.Create();
        }

        /// <summary>
        /// Crée le moteur de calcul fictif.
        /// </summary>
        /// <returns>
        /// Une implémentation fictive de <see cref="IComputeEngine"/>.
        /// </returns>
        public static IComputeEngine CreateMock()
        {
            // Le Mock permet à l'application publique de fonctionner
            // sans distribuer le véritable moteur de calcul.
            return new MockComputeEngine();
        }
    }
}