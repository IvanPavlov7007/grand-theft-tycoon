using System.Collections;
using UnityEngine;

public class SelectPrimaryVehicleProcess : IProcess
{
    public SelectPrimaryVehicleProcess()
    {
    }

    public IEnumerator Run()
    {
        bool finished = false;
        bool refreshRequested = false;

        PersonalVehicleSelectorController controller = new PersonalVehicleSelectorController(G.VehicleController.GetPersonalVehiclesList(),
            () =>
        {
            refreshRequested = true;
        });

        while (!finished)
        {
            refreshRequested = false;

            PopUpContextMenu menu = G.FixedContextMenuManager.CreateContextMenu(controller.CreateMenuUIContent());
            menu.Closed += _ =>
            {
                finished = true;
            };

            yield return new WaitUntil(() => finished || refreshRequested);

            if (refreshRequested && !finished)
            {
                menu.Close();
            }
        }
    }
}