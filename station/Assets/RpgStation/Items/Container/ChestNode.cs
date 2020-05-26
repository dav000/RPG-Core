﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Station
{
    public class ChestNode : Interactible
    {
        
        private AreaContainerSystem _containerSystem;
        private ItemsDb _itemDb;
        private ChestNodesDb _chestNodeDb;
        private ItemsSettingsDb _itemsSettingsDb;
        [HideInInspector]public bool StateSaved;
        [HideInInspector]public string SaveId;
        [HideInInspector]public string ChestNodeModelId;
        private UiContainerPopup _cachedContainerPopup;
        
        protected override void Setup()
        {
            GameGlobalEvents.OnSceneLoadObjects.AddListener(OnLoadContainer);
        }
        
        protected override void Dispose()
        {
            GameGlobalEvents.OnSceneLoadObjects.RemoveListener(OnLoadContainer); 
        }

        public void OnLoadContainer()
        {
            var dbSystems = RpgStation.GetSystemStatic<DbSystem>();
            _chestNodeDb = dbSystems.GetDb<ChestNodesDb>();
            if (string.IsNullOrEmpty(ChestNodeModelId))
            {
                return;
            }

            var nodeModel = _chestNodeDb.GetEntry(ChestNodeModelId);
            var defaultItems = LootUtils.GenerateLootStack(nodeModel.Loots);
            InitializeWithDefaultItems(Guid.NewGuid().ToString(), defaultItems, true);
        }

        private void Initialize(string saveId)
        {
            SaveId = saveId;
            _containerSystem = RpgStation.GetSystemStatic<AreaContainerSystem>();
            var dbSystems = RpgStation.GetSystemStatic<DbSystem>();
            _itemDb = dbSystems.GetDb<ItemsDb>();
            _chestNodeDb = dbSystems.GetDb<ChestNodesDb>();
            _itemsSettingsDb = dbSystems.GetDb<ItemsSettingsDb>();

            SetUiName(GetObjectName());
        }

        private void InitializeWithDefaultItems(string id, List<ItemStack> items, bool saved)
        {
            Initialize(id);
            var containerState = new ContainerState(8, items);
            var container = new ItemContainer(id, containerState, _itemDb);
            _containerSystem.AddContainer(container, saved);
        }


        public override void Interact(BaseCharacter user)
        {
            base.Interact(user);
            if (_cachedContainerPopup == null)
            {
                var prefab = _itemsSettingsDb.Get().ContainerSettings.ContainerPopup;
                if (prefab == null)
                {
                    Debug.LogError($" missing prefab container popup");
                    return;
                }
                
                _cachedContainerPopup = UiSystem.GetUniquePopup<UiContainerPopup>(UiContainerPopup.POPUP_KEY, prefab);
            }
        
            CachePopup(_cachedContainerPopup);
            _cachedContainerPopup.Setup(new ContainerReference(SaveId, RpgStation.GetSystemStatic<AreaContainerSystem>()), user);
            _cachedContainerPopup.Show();
            
        }
        
        public override void OnCancelInteraction(BaseCharacter user)
        {
            Debug.Log("cancel");
            UiSystem.HideUniquePopup<UiContainerPopup>(UiContainerPopup.POPUP_KEY);
            _cachedContainerPopup.Hide();
            base.OnCancelInteraction(user);
        }

        public override string GetObjectName()
        {
            var chestModel = _chestNodeDb.GetEntry(ChestNodeModelId);
            return chestModel.Name.GetValue();
        }
    }

}
