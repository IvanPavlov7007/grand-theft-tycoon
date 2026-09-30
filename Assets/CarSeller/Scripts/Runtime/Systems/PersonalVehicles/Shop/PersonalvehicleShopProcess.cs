using System.Collections;
using UnityEngine;

public class PersonalVehicleShopProcess : IProcess
{
    PersonalVehicleShop Shop;

    public PersonalVehicleShopProcess(PersonalVehicleShop shop)
    {
        Shop = shop;
    }

    public IEnumerator Run()
    {
        bool finished = false;
        bool refreshRequested = false;

        PersonalVehicleShopController controller = new PersonalVehicleShopController(Shop, () =>
        {
            refreshRequested = true;
        });

        while (!finished)
        {
            refreshRequested = false;

            PopUpContextMenu menu = G.FixedContextMenuManager.CreateContextMenu(controller.GenerateMenuUI());
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