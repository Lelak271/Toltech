namespace Toltech.App.ViewModels
{
    public enum ToleranceKind
    {
        N,
        T1,
        T2,
        Rn,
        RT1,
        RT2
    }

    public enum TolerancePosition
    {
        Origin,
        Intermediate,
        Extremity
    }

    /// <summary>
    /// Regroupe les 3 positions d'une même tolérance (Origin, Intermediate, Extremity).
    /// Pur regroupement, aucune logique propre.
    /// </summary>
    public sealed class ToleranceGroupViewModel
    {
        public ToleranceKind Kind { get; }

        public ToleranceSlotViewModel Extremity { get; }

        public ToleranceSlotViewModel Intermediate { get; }

        public ToleranceSlotViewModel Origin { get; }

        public ToleranceGroupViewModel(
            ToleranceKind kind,
            ToleranceSlotViewModel extremity,
            ToleranceSlotViewModel intermediate,
            ToleranceSlotViewModel origin)
        {
            Kind = kind;
            Extremity = extremity;
            Intermediate = intermediate;
            Origin = origin;
        }

        public IEnumerable<ToleranceSlotViewModel> Slots
        {
            get
            {
                yield return Origin;
                yield return Intermediate;
                yield return Extremity;
            }
        }

        public ToleranceSlotViewModel this[TolerancePosition position] => position switch
        {
            TolerancePosition.Origin => Origin,
            TolerancePosition.Intermediate => Intermediate,
            TolerancePosition.Extremity => Extremity,
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };


    }
}
