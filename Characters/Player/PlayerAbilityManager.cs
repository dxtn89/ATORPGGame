﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : CharacterAbilityManager {

    public System.Action<IAbility> OnPerformAbility = delegate { };

    private Coroutine globalCoolDownCoroutine = null;

    protected override void Awake() {
        //Debug.Log(gameObject.name + ".PlayerAbilityManager.Awake()");
        base.Awake();
        baseCharacter = GetComponent<PlayerCharacter>() as ICharacter;
    }

    protected override void Start() {
        //Debug.Log(gameObject.name + ".PlayerAbilityManager.Start()");
        base.Start();
    }

    public override void CreateEventReferences() {
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        base.CreateEventReferences();
        SystemEventManager.MyInstance.OnEquipmentChanged += OnEquipmentChanged;
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += OnCharacterUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn += OnCharacterUnitDespawn;
        if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.CreateEventReferences() Player is already spawned");
            OnCharacterUnitSpawn();
        }
    }

    public override void CleanupEventReferences() {
        if (!eventReferencesInitialized) {
            return;
        }
        base.CleanupEventReferences();
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnEquipmentChanged -= OnEquipmentChanged;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= OnCharacterUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= OnCharacterUnitDespawn;
        }
    }

    public override void OnDisable() {
        base.OnDisable();
        CleanupEventReferences();
    }

    public void OnEquipmentChanged(Equipment newItem, Equipment oldItem) {
        // can safely be ignored if player is not spawned
        if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
            return;
        }
        if (newItem != null) {
            if (newItem.MyOnEquipAbility != null) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(newItem.MyOnEquipAbility);
            }
            foreach (BaseAbility baseAbility in newItem.MyLearnedAbilities) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.LearnAbility(baseAbility.MyName);
            }
        }
        if (oldItem != null) {
            foreach (BaseAbility baseAbility in oldItem.MyLearnedAbilities) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.UnlearnAbility(baseAbility.MyName);
            }
        }
    }

    public void AbilityLearnedHandler(BaseAbility newAbility) {
        //Debug.Log("PlayerAbilityManager.AbilityLearnedHandler()");
        if (MessageFeedManager.MyInstance != null) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("Learned New Ability: {0}", newAbility.MyName));
        }
    }

    /*
    public void AbilityButtonPressedHandler(int abilityIndex) {
        Debug.Log("Ability Button Pressed Handler received ability index " + abilityIndex.ToString());
        //BeginAbility(abilityManager.abilityList[abilityIndex - 1]);
        BeginAbility(abilityList[abilityIndex - 1]);
    }
    */

    public override void LearnAbility(string abilityName) {
        //Debug.Log("PlayerAbilityManager.LearnAbility()");
        base.LearnAbility(abilityName);
        SystemEventManager.MyInstance.NotifyOnAbilityListChanged(abilityName);

    }

    public void LoadAbility(string abilityName) {
        //Debug.Log("PlayerAbilityManager.LoadAbility(" + abilityName + ")");
        IAbility ability = SystemAbilityManager.MyInstance.GetResource(abilityName) as IAbility;
        string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
        if (!abilityList.ContainsKey(keyName)) {
            //Debug.Log("PlayerAbilityManager.LoadAbility(" + abilityName + "): found it!");
            abilityList[keyName] = ability;
        }

    }

    public override void UpdateAbilityList(int newLevel) {
        //Debug.Log(gameObject.name + ".PlayerAbilitymanager.UpdateAbilityList(). length: " + abilityList.Count);
        base.UpdateAbilityList(newLevel);
        if (PlayerManager.MyInstance.MyCharacter.MyFactionName != null && PlayerManager.MyInstance.MyCharacter.MyFactionName != string.Empty) {
            PlayerManager.MyInstance.MyCharacter.LearnFactionAbilities(PlayerManager.MyInstance.MyCharacter.MyFactionName);
        }
    }

    public override void PerformAbility(IAbility ability, GameObject target, Vector3 groundTarget) {
        //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbility(" + ability.MyName + ")");
        base.PerformAbility(ability, target, groundTarget);
        if (ability.MyCanSimultaneousCast == false && ability.MyIgnoreGlobalCoolDown == false) {
            InitiateGlobalCooldown(ability);
        }
        OnPerformAbility(ability);
        SystemEventManager.MyInstance.NotifyOnAbilityUsed(ability as BaseAbility);
    }

    public void InitiateGlobalCooldown(IAbility ability) {
        Debug.Log(gameObject.name + ".PlayerAbilitymanager.InitiateGlobalCooldown(" + ability.MyName + ")");
        if (globalCoolDownCoroutine == null) {
            globalCoolDownCoroutine = StartCoroutine(BeginGlobalCoolDown());
        } else {
            Debug.Log("INVESTIGATE: GCD COROUTINE WAS NOT NULL");
        }

    }

    public IEnumerator BeginGlobalCoolDown() {
        Debug.Log(gameObject.name + ".PlayerAbilityManager.BeginGlobalCoolDown()");
        remainingGlobalCoolDown = 1f;
        while (remainingGlobalCoolDown > 0f) {
            remainingGlobalCoolDown -= Time.deltaTime;
            //Debug.Log("BaseAbility.BeginAbilityCooldown():" + MyName + ". time: " + remainingCoolDown);
            yield return null;
        }
        globalCoolDownCoroutine = null;
    }

    public override void CleanupCoroutines() {
        // called from base.ondisable
        base.CleanupCoroutines();
        if (globalCoolDownCoroutine != null) {
            StopCoroutine(globalCoolDownCoroutine);
            globalCoolDownCoroutine = null;
        }
    }

}
