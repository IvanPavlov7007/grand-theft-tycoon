using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicalProductViewBuilder", menuName = "Configs/View/PhysicalProductViewBuilder")]
public class PhysicalProductGameObjectBuilder : WarehouseProductGameObjectBuilder
{
    [Header("Physics settings")]
    public float carMass = 1f;
    public float wheelMass = 0.5f;
    public bool enableWheelMotor = false;
    public float wheelMotorSpeed = 0f;
    public float wheelMotorMaxTorque = 0f;
    public float wheelDampingRatio = 0.5f;
    public float wheelFrequency = 5f;
    public PhysicsMaterial2D wheelPhysicsMaterial;

    [Header("Layer settings")]
    [Tooltip("Layer index used for the assembled car (body + wheels).")]
    public int carLayer = 0; // set this in the inspector to e.g. a 'Car' layer

    [Header("Spring motor settings")] public bool enableSpringMotor = false;
    public float springMotorStiffness = 5f;
    public float springMotorMaxEnergy = 50f;
    public float springMotorDischargeRate = 10f;
    public float springMotorMaxTorque = 50f;
    [Tooltip("Layer mask for what counts as ground.")]
    public LayerMask groundLayerMask;
    [Tooltip("Distance to raycast below the wheel center to detect ground.")]
    public float groundCheckDistance = 0.25f;


    public override GameObject BuildCar(Car car)
    {
        // Root for the physical car
        var carGO = new GameObject(car.Name);
        carGO.layer = carLayer;

        var carRb = carGO.AddComponent<Rigidbody2D>();
        carRb.mass = carMass;

        // FRAME (visual)
        var frameGO = car.CarFrame.GetRepresentation(carPartViewBuilder);
        frameGO.transform.SetParent(carGO.transform, worldPositionStays: false);
        frameGO.transform.localPosition = Vector3.zero;
        frameGO.transform.localRotation = Quaternion.identity;
        SetLayerRecursively(frameGO, carLayer);

        // cache all colliders on the car body hierarchy for later ignore-collision setup
        var carColliders = new List<Collider2D>();
        carColliders.AddRange(carGO.GetComponentsInChildren<Collider2D>());

        // BUILD PHYSICAL PARTS
        foreach (var partLocation in car.carParts.Keys)
        {
            AttachPartInternal(carGO.transform, carRb, carColliders, partLocation);
        }

        // Attach warehouse-level view/drag logic to the whole car (not to individual parts)
        var view = productViewComponentBuilder.BuildViewComponent(carGO, car) as WarehouseCarView;
        // Make sure the view knows which builder created it
        view?.SetBuilder(this);

        return carGO;
    }

    public override GameObject BuildCarFrame(CarFrame carFrame)
    {
        Debug.LogError("PhysicalProductGameObjectBuilder does not support building standalone CarFrame views. " + carFrame.UniqueName);
        return null;
    }

    /// <summary>
    /// Called by WarehouseCarView when a part is attached at runtime.
    /// Ensures behavior matches BuildCar (physics, no extra views on parts, etc.).
    /// </summary>
    public void AttachPartToCarView(WarehouseCarView carView, Car.CarPartLocation partLocation)
    {
        if (carView == null || carView.car == null) return;

        var carGO = carView.gameObject;
        var carRb = carGO.GetComponent<Rigidbody2D>();
        if (carRb == null)
        {
            Debug.LogError("Physical car view has no Rigidbody2D on root.");
            return;
        }

        // collect all existing colliders for proper ignore-collision setup
        var carColliders = new List<Collider2D>();
        carColliders.AddRange(carGO.GetComponentsInChildren<Collider2D>());

        AttachPartInternal(carGO.transform, carRb, carColliders, partLocation);
    }

    private void AttachPartInternal(
        Transform carTransform,
        Rigidbody2D carRb,
        List<Collider2D> carColliders,
        Car.CarPartLocation partLocation)
    {
        var slotData = partLocation.PartSlotRuntimeConfig.partSlotData;

        // If not occupied, skip
        if (slotData.Hidden || partLocation.Occupant == null)
            return;

        if (partLocation.Occupant is Wheel wheelProduct)
        {
            // First build & place the wheel using the shared helper
            var wheelGO = CarPartViewPlacementHelper.BuildCarPartAtPosition(
                partLocation,
                carTransform,
                carPartViewBuilder);

            if (wheelGO != null)
            {
                SetLayerRecursively(wheelGO, carLayer);
                SetupPhysicalWheel(wheelGO, wheelProduct, partLocation, carRb, carColliders);
            }
        }
        else
        {
            // Non-wheel parts use the same helper for placement
            var partGO = CarPartViewPlacementHelper.BuildCarPartAtPosition(
                partLocation,
                carTransform,
                carPartViewBuilder);

            if (partGO != null)
            {
                SetLayerRecursively(partGO, carLayer);
                carColliders.AddRange(partGO.GetComponentsInChildren<Collider2D>());
            }
        }
    }

    /// <summary>
    /// Adds physics and joints to an already built wheel GameObject.
    /// Also makes the wheel ignore collisions with other colliders in the same car.
    /// </summary>
    private void SetupPhysicalWheel(
        GameObject wheelGO,
        Wheel wheel,
        Car.CarPartLocation partLocation,
        Rigidbody2D carRb,
        List<Collider2D> carColliders)
    {
        var slotData = partLocation.PartSlotRuntimeConfig.partSlotData;

        // Ensure rigidbody on wheel
        var wheelRb = wheelGO.GetComponent<Rigidbody2D>();
        if (wheelRb == null)
        {
            wheelRb = wheelGO.AddComponent<Rigidbody2D>();
        }
        wheelRb.mass = wheelMass;
        wheelRb.sharedMaterial = wheelPhysicsMaterial;

        // Attach wheel to car with a WheelJoint2D
        var joint = wheelGO.AddComponent<WheelJoint2D>();
        joint.connectedBody = carRb;
        joint.autoConfigureConnectedAnchor = false;

        // Anchors: wheel's own center as localAnchor; car-local anchor same as slot local position
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = slotData.LocalPosition;

        var suspension = joint.suspension;
        suspension.dampingRatio = wheelDampingRatio;
        suspension.frequency = wheelFrequency;
        joint.suspension = suspension;

        // Disable built-in motor if using spring motor
        if (enableSpringMotor)
        {
            joint.useMotor = false;
        }
        else
        {
            joint.useMotor = enableWheelMotor;
            if (enableWheelMotor)
            {
                var motor = joint.motor;
                motor.motorSpeed = wheelMotorSpeed;
                motor.maxMotorTorque = wheelMotorMaxTorque;
                joint.motor = motor;
            }
        }

        // ----- COLLISION FILTERING -----
        var wheelColliders = wheelGO.GetComponentsInChildren<Collider2D>();
        foreach (var wCol in wheelColliders)
        {
            if (wCol == null) continue;

            foreach (var cCol in carColliders)
            {
                if (cCol == null || cCol == wCol) continue;
                Physics2D.IgnoreCollision(wCol, cCol, true);
            }

            if (!carColliders.Contains(wCol))
                carColliders.Add(wCol);
        }

        // Attach spring motor component if enabled
        if (enableSpringMotor)
        {
            var springMotor = wheelGO.GetComponent<WheelSpringMotor2D>();
            if (springMotor == null)
            {
                springMotor = wheelGO.AddComponent<WheelSpringMotor2D>();
            }

            springMotor.springStiffness = springMotorStiffness;
            springMotor.maxEnergy = springMotorMaxEnergy;
            springMotor.dischargeRate = springMotorDischargeRate;
            springMotor.maxTorque = springMotorMaxTorque;
            springMotor.groundLayerMask = groundLayerMask;
            springMotor.groundCheckDistance = groundCheckDistance;
        }

        // IMPORTANT: no productViewComponentBuilder.BuildViewComponent here,
        // wheels visually/semantically belong to the car in the warehouse.
    }


    private void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}