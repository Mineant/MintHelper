using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MioHelper.AbilitySystem;
using MioHelper.StatSystem;

namespace MioHelper.Samples.CharacterSystem
{
    /// <summary>
    /// A sample character entity built from the MioHelper seams — the reference "how to compose an
    /// entity" for the package. Implements <see cref="IAbilityOwner"/>, <see cref="ISelectable"/> and
    /// <see cref="ITeamMember"/>, hosts a <see cref="MioStatSheet"/>, auto-adds a
    /// <see cref="CharacterAbilityModule"/> and wires up every <see cref="SampleCharacterBehaviour"/>
    /// on the GameObject. Copy-paste and shape to your project (EC's Character is the same idea).
    ///
    /// This sample adds no framework code — every piece it uses is an existing package seam.
    /// </summary>
    [DisallowMultipleComponent]
    public class SampleCharacter : MonoBehaviour, IAbilityOwner, ISelectable, ITeamMember
    {
        [Header("Setup")]
        [Tooltip("If true, Initialize() runs automatically in Awake.")]
        [SerializeField] protected bool _initializeOnAwake = true;

        [Tooltip("Team affiliation used by target selection to filter ally/enemy/self.")]
        [SerializeField] protected int _teamId = 1;

        protected bool _initialized;
        protected HashSet<SampleCharacterBehaviour> _behaviours;

        #region Seams

        // ---- IAbilityOwner ----
        public string ID { get; protected set; }
        public GameObject GameObject => gameObject;

        // ---- ITeamMember ----
        public int TeamId => _teamId;

        /// <summary>Sets the team affiliation used by target selection.</summary>
        public void SetTeam(int teamId) => _teamId = teamId;

        // ---- ISelectable ----
        public bool IsValidTarget => true;
        public Transform Transform => transform;
        public Vector3 GetHitPosition() => transform.position;

        /// <summary>
        /// Returns a component by type — the ability system's seam for reaching stats, health, etc.
        /// SampleCharacterBehaviours are found through the gathered behaviour set; everything else
        /// through GetComponent.
        /// </summary>
        public T GetBehaviour<T>()
        {
            if (typeof(SampleCharacterBehaviour).IsAssignableFrom(typeof(T)))
            {
                if (_behaviours != null)
                {
                    foreach (var behaviour in _behaviours)
                        if (behaviour is T typed) return typed;
                }
                return default;
            }
            return GetComponent<T>();
        }

        #endregion

        #region References

        /// <summary>Stat sheet owned by this character. Created on Initialize unless already assigned.</summary>
        public MioStatSheet StatTable { get; protected set; }

        /// <summary>The health component on this character, if present.</summary>
        public SampleHealth Health { get; protected set; }

        /// <summary>The auto-added ability module.</summary>
        public CharacterAbilityModule AbilityModule { get; protected set; }

        #endregion

        #region Events

        /// <summary>Fires after Initialize() completes.</summary>
        public event Action OnInitialized;

        /// <summary>Fires after Release() completes.</summary>
        public event Action OnReleased;

        #endregion

        #region Initialization & Release

        protected virtual void Awake()
        {
            if (_initializeOnAwake) Initialize();
        }

        protected virtual void OnDestroy()
        {
            Release();
        }

        /// <summary>
        /// Wires the character together: assigns a unique ID, gathers behaviours, caches health,
        /// auto-adds + initializes the ability module, creates the stat sheet, initializes every
        /// behaviour, then fires <see cref="OnInitialized"/>.
        /// </summary>
        public virtual void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            ID = Guid.NewGuid().ToString();
            _behaviours = GetComponents<SampleCharacterBehaviour>().ToHashSet();
            Health = GetComponent<SampleHealth>();

            AbilityModule = GetOrAddComponent<CharacterAbilityModule>();
            AbilityModule.Initialize();

            foreach (var behaviour in _behaviours)
                behaviour.Initialize(this);

            StatTable ??= new MioStatSheet();

            OnInitialized?.Invoke();
        }

        /// <summary>Releases the ability module and behaviours. Safe to call repeatedly.</summary>
        public virtual void Release()
        {
            if (!_initialized) return;
            _initialized = false;

            AbilityModule?.Release();
            AbilityModule = null;

            if (_behaviours != null)
            {
                foreach (var behaviour in _behaviours)
                    behaviour.Release();
                _behaviours.Clear();
            }

            OnReleased?.Invoke();
        }

        /// <summary>Gets an existing component or adds one — used to auto-add the ability module.</summary>
        protected T GetOrAddComponent<T>() where T : Component
            => GetComponent<T>() ?? gameObject.AddComponent<T>();

        #endregion
    }
}
