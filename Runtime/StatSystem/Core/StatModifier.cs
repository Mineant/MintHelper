using System;

namespace MioHelper.StatSystem
{
    /// <summary>
    /// Modifier stacking type. The enum values ARE the default <see cref="StatModifier.Order"/>,
    /// so a modifier sorts among its own type unless a custom Order is given.
    /// </summary>
    public enum MioStatModType
    {
        /// <summary>Flat addition, applied first: final += Value.</summary>
        Flat = 100,

        /// <summary>Additive percentage, accumulated across all percent mods then applied once: final *= (1 + sum).</summary>
        PercentAdd = 200,

        /// <summary>Multiplicative, applied to the running value: final *= (1 + Value).</summary>
        Mult = 300,
    }

    /// <summary>
    /// Runtime stat modifier. Public mutable fields: the fold re-reads them live on every
    /// calculation, so an in-place field write is observed immediately with no cache invalidation.
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public float Value;
        public MioStatModType Type;
        public int Order;
        public object Source;

        public StatModifier()
        {
            Order = (int)MioStatModType.Flat;
        }

        public StatModifier(float value, MioStatModType type, object source = null)
        {
            Value = value;
            Type = type;
            Order = (int)type;
            Source = source;
        }

        public StatModifier(float value, MioStatModType type, int order, object source = null)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }

        /// <summary>
        /// Value/Type/Order equality — Source is deliberately excluded (Source is disambiguated
        /// by callers passing a source to the remove methods).
        /// </summary>
        public override bool Equals(object obj) =>
            obj is StatModifier m &&
            m.Value == Value &&
            m.Type == Type &&
            m.Order == Order;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Value.GetHashCode();
                hash = hash * 31 + (int)Type;
                hash = hash * 31 + Order;
                return hash;
            }
        }
    }
}
