using UnityEngine;

namespace MioHelper.Samples.CharacterSystem
{
    /// <summary>
    /// Pluggable per-character component base, mirroring EC's CharacterBehaviour. Attach subclasses
    /// to a SampleCharacter GameObject; <see cref="SampleCharacter.Initialize"/> gathers and
    /// initializes them in one pass.
    /// </summary>
    public class SampleCharacterBehaviour : MonoBehaviour
    {
        public SampleCharacter Owner { get; protected set; }
        public bool IsInitialized { get; protected set; }

        public virtual void Initialize(SampleCharacter owner)
        {
            IsInitialized = true;
            Owner = owner;
        }

        public virtual void Release()
        {
            IsInitialized = false;
            Owner = null;
        }
    }
}
