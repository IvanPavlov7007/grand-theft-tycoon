using System;

public class PersonalVehicleSelectorController
{
    public readonly PersonalVehiclesList PersonalVehiclesList;
    public event Action OnRefresh;

    public PersonalVehicleSelectorController(PersonalVehiclesList personalVehiclesList,Action refreshedCallback)
    {
        PersonalVehiclesList = personalVehiclesList;
        OnRefresh += refreshedCallback;
    }
    public Widget CreateMenuUIContent()
    {
        Widget container = new VerticalContentWidget(
            "Select Primary Vehicle");
        populateVehicleSelectionContainer(container, G.VehicleController.CurrentPrimaryVehicle);
        return container;
    }

    void populateVehicleSelectionContainer(Widget container, PersonalVehicle currentVehicle)
    {
        foreach(var vehicle in PersonalVehiclesList.OwnedVehicles)
        {
            if(vehicle == currentVehicle)
            {
                container.Children.Add(selectedVehicleUIContent(vehicle));
            }
            else
            {
                container.Children.Add(availableVehicleUIContent(vehicle));
            }
        }
    }

    Widget selectedVehicleUIContent(PersonalVehicle vehicle)
    {
        return PrimarySelectionCarButtonWidgetCreator.CreateSelected(vehicle.Car, null);
    }

    Widget availableVehicleUIContent(PersonalVehicle vehicle)
    {
        return PrimarySelectionCarButtonWidgetCreator.CreateAvailable(vehicle.Car, () =>
            {
                G.VehicleController.SwapPrimaryVehicle(vehicle);
                OnRefresh?.Invoke();
            });
    }
}