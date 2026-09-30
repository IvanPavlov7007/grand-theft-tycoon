using Pixelplacement;
using System.Collections;
using UnityEngine;

public class TutorialSystem : MonoBehaviour
{
    [SerializeField]
    GameObject tutorialFinger;
    IEnumerator Start()
    {
        if (!G.runIntialized)
            yield break;
        yield return new WaitUntil(() => G.VehicleController != null && G.VehicleController.CurrentVehicleEntity != null);
        var position = G.VehicleController.CurrentVehicleEntity.Position;
        var initVec3 = position.WorldPosition;
        var direction = position.GetCurrentTangent();
        var obj = Instantiate(tutorialFinger, initVec3, Quaternion.identity);
        Tween.Position(obj.transform, initVec3 + direction * 1f, 1f, 0f, Tween.EaseInOut, Tween.LoopType.Loop);
        yield return new WaitUntil(() =>
        {
            var distance = Vector3.Distance(G.VehicleController.CurrentVehicleEntity.Position.WorldPosition, initVec3);
            return distance > 0.7f;
        });
        Destroy(obj);
    }
}
