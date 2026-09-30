using UnityEngine;
using UnityEngine.UI;

public class CloseButton : MonoBehaviour
{
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(onClick);
    }

    private void onClick()
    {
        var closable = GetComponentInParent<IClosable>();

        if(closable == null)
        {
            Debug.LogError("No IClosable found in parent hierarchy.");
            return;
        }

        closable.Close();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(onClick);
    }
}
