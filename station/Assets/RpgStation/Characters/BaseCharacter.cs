﻿using System.Collections.Generic;
using UnityEngine;

namespace Station
{
    
    public class BaseCharacter : MonoBehaviour
    {
        #region FIELDS

        private DbSystem _dbSystem;
        private GameSettingsDb _gameSettingsDb;
        private StationMechanics _mechanics;
       
        
        public CharacterUpdate OnCharacterInitialized;
        public CharacterUpdate OnVitalsUpdated;
        public CharacterUpdate OnStatisticUpdated;
        public CharacterUpdate OnAttributesUpdated;
        public CharacterVitalChange OnDamaged;
        public CharacterVitalChange OnHealed;
        public CharacterVitalChange OnEnergyChange;
        public CharacterUpdate OnDie;
        public CharacterUpdate OnRevived;
        public CharacterTargetUpdated OnTargetChanged;

        public delegate void CharacterTargetUpdated(BaseCharacter target);
        public delegate void CharacterUpdate(BaseCharacter character);
        public delegate void CharacterVitalChange(BaseCharacter character, VitalChangeData data);
       
        [SerializeField] private CharacterInputHandler _inputHandler = null;
        public CharacterInputHandler GetInputHandler => _inputHandler;

        private CharacterControl _control;
        public CharacterControl Control => _control;

        private StatsHandler _stats = null;
        public StatsHandler Stats => _stats;

        private CharacterCalculation _calculatorInstance = null;
        public CharacterCalculation Calculator => _calculatorInstance;

        private ActionHandler _action = null;
        public ActionHandler Action => _action;

        private bool _isDead;

        public bool IsDead
        {
            get 
            { 
                return _isDead;
            }
            set
            {
                if (value && OnDie != null)
                {
                    if (_isDead)
                    {
                        Debug.LogError("died twice");
                    }

                    OnDie.Invoke(this);
                }

                _isDead = value;
            }
        }
        
        private BaseCharacter _target;
        public BaseCharacter Target
        {
            get { return _target; }
            set
            {
                _target = value;
                OnTargetChanged?.Invoke(value);
            }
        }

        private string _raceId;

        public string GetRace()
        {
            return _raceId;
        }

        private string _genderId;

        public string GetGender()
        {
            return _genderId;
        }

        
        private string _factionId;

        public string GetFaction()
        {
            return _factionId;
        }

        private string _name;

        public string GetName()
        {
            return _name;
        }
        
        private Dictionary<string, string> _meta = new Dictionary<string, string>();

        public string GetMeta(string key)
        {
            if (_meta.ContainsKey(key))
            {
                return _meta[key];
            }

            return "";
        }


        [SerializeField]private Renderer _characterVisual;
        #endregion

        private void Awake()
        {
            _dbSystem = RpgStation.GetSystemStatic<DbSystem>();
            _gameSettingsDb = _dbSystem.GetDb<GameSettingsDb>();
            _control = GetComponent<CharacterControl>();
            _mechanics = _gameSettingsDb.Get().Mechanics;
        }

        public void Init(string raceId, string factionId, string genderId, CharacterCalculation instance, string characterName, Dictionary<string, string> meta = null)
        {
           
            
            if (meta != null)
            {
                foreach (var entry in meta)
                {
                    AddMeta(entry.Key, entry.Value);
                }
            }

            _raceId = raceId;
            _factionId = factionId;
            _genderId = genderId;
            _name = characterName;
            
            _calculatorInstance = instance;
            
            _calculatorInstance.Setup(this);
        }

        public void SetRenderer(Renderer cache)
        {
            _characterVisual = cache;
        }

        public void SetupStats(IdIntegerValue health, IdIntegerValue secondaryHealth, IdIntegerValue[] energies)
        {
            _stats = gameObject.AddComponent<StatsHandler>();
            _stats.Setup(this,health, secondaryHealth,energies);
        }
        
        public void AddMeta(string key, string value)
        {
            if (_meta.ContainsKey(key))
            {
                _meta[key] = value;
            }
            else
            {
                _meta.Add(key, value);
            }
        }

        public void SetupAction(AttackData defaultAttack)
        {
           _action = new PlayerActionHandler();
           _action.SetupDefaultAttack(defaultAttack);
           _action.SetAbilities(new List<RuntimeAbility>(), this);
        }

        #region [[ FACTION & TARGETING ]]

        public Stance ResolveStance(BaseCharacter requester)
        {
            var factionHandler = _mechanics.FactionHandler();
  
            var stance = factionHandler.ResolveStance(requester._factionId, _factionId);

            if (stance < 2)
            {
                return Stance.Ally;
            }
            else if (stance == 0)
            {
                return Stance.Neutral;
            }
            else
            {
                return Stance.Enemy;
            }
        }

        #endregion
        
        private void Update()
        {
            _action?.UpdateLoop();
        }
        
        #region effect related

        public Vector3 GetCenter()
        {
            if (_characterVisual == null)
            {
                return transform.position;
            }

            return _characterVisual.bounds.center;
        }
        
        public Vector3 GetTop()
        {
            if (_characterVisual == null)
            {
                return transform.position;
            }

            var bounds = _characterVisual.bounds;
            Vector3 top = bounds.center;
            top.y += bounds.extents.y;
            return top;
        }
        #endregion

    }

    public enum Stance
    {
        Ally,
        Neutral,
        Enemy
    }
}