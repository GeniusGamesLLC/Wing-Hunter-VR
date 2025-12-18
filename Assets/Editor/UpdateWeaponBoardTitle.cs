using UnityEngine;
using UnityEditor;

public static class UpdateWeaponBoardTitle
{
    [MenuItem("Tools/Update Weapon Board Title Style")]
    public static void UpdateTitleStyle()
    {
        var rack = GameObject.Find("GunDisplayRack");
        if (rack == null)
        {
            Debug.LogError("GunDisplayRack not found!");
            return;
        }

        // Try both names - it may have been renamed to "266612" by MCP
        var titleTransform = rack.transform.Find("TitleText");
        if (titleTransform == null)
        {
            titleTransform = rack.transform.Find("266612");
        }
        if (titleTransform == null)
        {
            Debug.LogError("TitleText not found on GunDisplayRack!");
            return;
        }
        
        // Rename back to TitleText if needed
        if (titleTransform.name != "TitleText")
        {
            titleTransform.name = "TitleText";
        }

        var tmp = titleTransform.GetComponent<TMPro.TextMeshPro>();
        if (tmp == null)
        {
            Debug.LogError("TextMeshPro not found on TitleText!");
            return;
        }

        // Match AnnouncementBoard title style:
        // - Dark brown color (0.3, 0.2, 0.1)
        // - Font size 0.2
        // - Normal style (not bold)
        tmp.color = new Color(0.3f, 0.2f, 0.1f, 1f);
        tmp.fontSize = 0.2f;
        tmp.fontStyle = TMPro.FontStyles.Normal;
        
        // Position above the board
        titleTransform.localPosition = new Vector3(0, 0.38f, -0.01f);
        
        // Adjust rect size
        var rect = titleTransform.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(1f, 0.15f);
        }

        EditorUtility.SetDirty(tmp);
        EditorUtility.SetDirty(rack);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Weapon board title style updated to match announcement board!");
    }
}
