using UnityEngine;
public class ObjectWithPopupMenuTest : MonoBehaviour
{
    [System.Serializable]
    public enum TestButton { Crouch, Jump}
    public TestButton buttonToTest;

    void OnTestButton()
    {
        //GameEvents.Instance.OnObjectWithPopupClicked?.Invoke(this);
    }

    private void OnEnable()
    {
        switch (buttonToTest)
        {
            case TestButton.Crouch:
                G.PlayerInputController.crouched += OnTestButton;
                break;
            case TestButton.Jump:
                G.PlayerInputController.jumped += OnTestButton;
                break;
            default:
                break;
        }
    }


    private void OnDisable()
    {
        switch (buttonToTest)
        {
            case TestButton.Crouch:
                G.PlayerInputController.crouched -= OnTestButton;
                break;
            case TestButton.Jump:
                G.PlayerInputController.jumped -= OnTestButton;
                break;
            default:
                break;
        }
    }
}

