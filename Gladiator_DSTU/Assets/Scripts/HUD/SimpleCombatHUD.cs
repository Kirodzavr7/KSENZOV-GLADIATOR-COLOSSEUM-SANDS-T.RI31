using UnityEngine;

/// <summary>Минимальный HUD: здоровье и выносливость обеих сторон.</summary>
public class SimpleCombatHUD : MonoBehaviour
{
    [SerializeField] private SimpleFighter player;
    [SerializeField] private SimpleFighter enemy;

    private GUIStyle _titleStyle;
    private GUIStyle _valueStyle;
    private GUIStyle _barBackgroundStyle;
    private Texture2D _whiteTexture;

    private void Awake()
    {
        _whiteTexture = Texture2D.whiteTexture;
    }

    private void Update()
    {
        if (player == null || enemy == null)
            FindFighters();
    }

    private void FindFighters()
    {
        SimpleFighter[] fighters = FindObjectsByType<SimpleFighter>(FindObjectsSortMode.None);
        foreach (SimpleFighter fighter in fighters)
        {
            if (fighter.Team == FighterTeam.Player && player == null) player = fighter;
            if (fighter.Team == FighterTeam.Enemy && enemy == null) enemy = fighter;
        }
    }

    private void OnGUI()
    {
        if (player == null && enemy == null) return;

        float margin = 18f;
        float panelWidth = Mathf.Min(320f, (Screen.width - margin * 3f) * 0.5f);
        float panelHeight = 100f;
        InitStyles();

        if (player != null)
            DrawFighterPanel(new Rect(margin, margin, panelWidth, panelHeight), "PLAYER", player);
        if (enemy != null)
            DrawFighterPanel(new Rect(Screen.width - panelWidth - margin, margin, panelWidth, panelHeight), "ENEMY", enemy);
    }

    private void InitStyles()
    {
        if (_titleStyle != null) return;
        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        _valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.white }
        };
        _barBackgroundStyle = new GUIStyle(GUI.skin.box);
    }

    private void DrawFighterPanel(Rect rect, string title, SimpleFighter fighter)
    {
        Color previous = GUI.color;
        GUI.color = new Color(0.04f, 0.05f, 0.07f, 0.82f);
        GUI.Box(rect, GUIContent.none, _barBackgroundStyle);
        GUI.color = previous;

        GUI.Label(new Rect(rect.x + 10f, rect.y + 5f, rect.width - 20f, 22f), title, _titleStyle);
        DrawBar(rect, 34f, "HP", fighter.CurrentHealth, fighter.MaxHealth, new Color(0.82f, 0.18f, 0.16f));
        DrawBar(rect, 64f, "STAMINA", fighter.CurrentStamina, fighter.MaxStamina, new Color(0.88f, 0.7f, 0.12f));
    }

    private void DrawBar(Rect panel, float yOffset, string label, float value, float max, Color fillColor)
    {
        float labelWidth = 58f;
        float valueWidth = 52f;
        float barX = panel.x + 10f + labelWidth;
        float barWidth = panel.width - labelWidth - valueWidth - 30f;
        float barY = panel.y + yOffset;
        float barHeight = 15f;

        GUI.color = Color.white;
        GUI.Label(new Rect(panel.x + 10f, barY - 2f, labelWidth, 19f), label, _valueStyle);
        GUI.color = new Color(0.16f, 0.17f, 0.19f, 1f);
        GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight), _whiteTexture);

        float fraction = max > 0f ? Mathf.Clamp01(value / max) : 0f;
        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(barX, barY, barWidth * fraction, barHeight), _whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(panel.x + panel.width - valueWidth - 8f, barY - 2f, valueWidth, 19f),
            $"{Mathf.CeilToInt(value)}/{Mathf.CeilToInt(max)}", _valueStyle);
        GUI.color = Color.white;
    }
}
