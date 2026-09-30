using System;
using UnityEngine;

    public class CitySceneMain : MonoBehaviour
    {
        private void Start()
        {
            Enter();
        }

        public void Enter()
        {
            G.GameFlowController.SetCity();
            G.CitySceneManager.SetCurrentProfile((G.GameState));
            G.CitySceneManager.InitializeCity();
            setRootActive(true);

            CameraHelper.SetCurrentPositionAtCar();

            Run.After(0.2f, ()=> G.WarehouseEntryCooldownService.Reset());
        }

        public void Exit()
        {
            setRootActive(false);
        }

        void setRootActive(bool active)
        {
            if (G.CityRoot != null)
            {
                G.CityRoot.SetActive(active);
            }
            else
            {
                Debug.LogWarning("CityRoot is not set");
            }
        }
    }