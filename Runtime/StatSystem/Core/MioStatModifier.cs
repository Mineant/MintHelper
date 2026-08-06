using System;

namespace MioHelper.StatSystem
{
    /// <summary>
    /// Inspector/authoring DTO for a single stat modifier. Stat and group names are plain strings
    /// (there is no Identifier/Definition layer). <see cref="Source"/> is a runtime-only reference
    /// field — Unity's serializer ignores it, which is expected.
    /// </summary>
    [Serializable]
    public class MioStatModifier
    {
        public string Stat;
        public float Value;
        public MioStatModType Type;
        public object Source;

        public StatModifier GetStatModifier() => new StatModifier(Value, Type, Source);
    }
}
