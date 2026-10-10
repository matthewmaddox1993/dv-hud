using System.Collections.Generic;

namespace QuantitiesNet
{
    public class UnitRegistry
    {
        public static UnitRegistry Default { get; } = new UnitRegistry();

        private readonly Dictionary<Dimension, List<Unit>> units = new Dictionary<Dimension, List<Unit>>();
        private readonly Dictionary<Dimension, Unit> preferredUnits = new Dictionary<Dimension, Unit>();

        public bool TryGetUnits(Dimension d, out List<Unit> result) => units.TryGetValue(d, out result);
        public bool TryGetUnits<D>(out List<Unit> result) where D : IDimension, new() =>
            TryGetUnits(Dimension.ForType<D>(), out result);

        public bool TryGetPreferredUnit(Dimension d, out Unit result) => preferredUnits.TryGetValue(d, out result);
        public bool TryGetPreferredUnit<D>(out Unit result) where D : IDimension, new() =>
            TryGetPreferredUnit(Dimension.ForType<D>(), out result);

        public void SetPreferredUnit(Unit unit) => preferredUnits[unit.Dimension] = unit;

        public void Add(Unit unit)
        {
            if (!units.TryGetValue(unit.Dimension, out var list))
                units[unit.Dimension] = list = new List<Unit>();
            if (!list.Contains(unit))
                list.Add(unit);
        }
    }
}

