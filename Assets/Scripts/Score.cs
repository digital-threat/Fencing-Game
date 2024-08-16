using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Score : NetworkBehaviour
{
    private TMP_Text tmpText;
    
    private NetworkVariable<Vector2> position = new (writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<int> score = new (writePerm: NetworkVariableWritePermission.Server);

    private void Awake()
    {
        tmpText = GetComponentInChildren<TMP_Text>();
    }

    public override void OnNetworkSpawn()
    {
        score.OnValueChanged += OnScoreChanged;
        score.Value = 0;
        
        position.OnValueChanged += OnPositionChanged;
        tmpText.GetComponent<RectTransform>().anchoredPosition = position.Value;
    }

    private void OnScoreChanged(int previousvalue, int newvalue)
    {
        tmpText.text = score.Value.ToString("00");
    }

    private void OnPositionChanged(Vector2 previousvalue, Vector2 newvalue)
    {
        tmpText.GetComponent<RectTransform>().anchoredPosition = position.Value;
    }

    public void IncrementScore()
    {
        score.Value++;
        score.Value = Mathf.Clamp(score.Value, 0, 99);
    }
    
    public void DecrementScore()
    {
        score.Value--;
        score.Value = Mathf.Clamp(score.Value, 0, 99);
    }

    public void SetPosition(Vector2 position)
    {
        this.position.Value = position;
    }
}
