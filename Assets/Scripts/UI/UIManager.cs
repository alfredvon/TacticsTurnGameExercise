using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] CommandPanel commandPanel;
    [Header("FloatingMessage")]
    [SerializeField] GameObject messagePrefab;
    [SerializeField] Transform floatingMessageRoot;
    [SerializeField] Vector3 floatingMessagePosDelta;

    private void Start()
    {
        ShowCommandPanel(false);
    }

    public void ShowCommandPanel(bool is_show, in Character character = null)
    {
        if (is_show == true)
        {
            if (character != null)
            {
                commandPanel.UpdateBtnState(character);
            }
            commandPanel.Show();
        }
            
        else
            commandPanel.Hide();
    }

    public CommandPanel GetCommandPanel()
    { 
        return commandPanel;
    }

    public void CreateFloatingMessage(Vector3 position, string message)
    {
        if (messagePrefab == null)
        {
            Debug.LogError("No message prefab.");
            return;
        }

        GameObject instance = Instantiate(messagePrefab, floatingMessageRoot);
        instance.transform.position = position + floatingMessagePosDelta;
        var floatingMessage = instance.GetComponent<FloatingMessage>();
        floatingMessage.originPos = position + floatingMessagePosDelta;
        floatingMessage.message = message;
        floatingMessage.duration = 1.5f;
    }
}
