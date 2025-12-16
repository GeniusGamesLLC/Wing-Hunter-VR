using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor utility to set up the Start Pedestal with proper XR interaction.
/// Run this once from the menu to create the pedestal structure.
/// </summary>
public class StartPedestalSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Wing Hunter/Setup Start Pedestal")]
    public static void SetupStartPedestal()
    {
        // Find or create the UI parent
        GameObject uiParent = GameObject.Find("--- UI ---");
        if (uiParent == null)
        {
            uiParent = new GameObject("--- UI ---");
        }
        
        // Remove existing StartPedestal if present
        GameObject existingPedestal = GameObject.Find("StartPedestal");
        if (existingPedestal != null)
        {
            DestroyImmediate(existingPedestal);
        }
        
        // Create the main pedestal container
        // Position it in front of the player (XR Origin is at z=-23)
        GameObject pedestal = new GameObject("StartPedestal");
        pedestal.transform.SetParent(uiParent.transform);
        pedestal.transform.position = new Vector3(0, 0, -20);
        
        // Create pedestal base (table)
        GameObject pedestalBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestalBase.name = "PedestalBase";
        pedestalBase.transform.SetParent(pedestal.transform);
        pedestalBase.transform.localPosition = new Vector3(0, 0.5f, 0);
        pedestalBase.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
        
        // Try to load and instantiate the XRI Push Button prefab
        string pushButtonPath = "Assets/Samples/XR Interaction Toolkit/3.3.0/Starter Assets/DemoSceneAssets/Prefabs/Interactables/Push Button.prefab";
        GameObject pushButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pushButtonPath);
        
        GameObject startButton;
        XRSimpleInteractable interactable;

        
        if (pushButtonPrefab != null)
        {
            // Instantiate the XRI Push Button prefab
            startButton = (GameObject)PrefabUtility.InstantiatePrefab(pushButtonPrefab);
            startButton.name = "StartButton";
            startButton.transform.SetParent(pedestal.transform);
            startButton.transform.localPosition = new Vector3(0, 1.0f, 0);
            startButton.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
            interactable = startButton.GetComponent<XRSimpleInteractable>();
            Debug.Log("StartPedestalSetup: Using XRI Push Button prefab");
        }
        else
        {
            Debug.LogWarning("StartPedestalSetup: XRI Push Button prefab not found, creating simple button");
            startButton = CreateSimpleButton(pedestal.transform);
            interactable = startButton.GetComponent<XRSimpleInteractable>();
        }
        
        // Add AudioSource for button press sound
        AudioSource audioSource = startButton.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        AudioClip clickSound = FindClickSound();
        if (clickSound != null)
        {
            audioSource.clip = clickSound;
        }
        
        // Add 3D text label on top of button
        GameObject buttonLabel = new GameObject("ButtonLabel");
        buttonLabel.transform.SetParent(startButton.transform);
        buttonLabel.transform.localPosition = new Vector3(0, 0.05f, 0);
        buttonLabel.transform.localRotation = Quaternion.Euler(90, 0, 0);
        buttonLabel.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        
        TextMeshPro labelTMP = buttonLabel.AddComponent<TextMeshPro>();
        labelTMP.text = "START";
        labelTMP.fontSize = 36;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color = Color.white;
        
        // Add particle system for button press effect
        GameObject particlesObj = new GameObject("ButtonParticles");
        particlesObj.transform.SetParent(startButton.transform);
        particlesObj.transform.localPosition = new Vector3(0, 0.05f, 0);
        particlesObj.transform.localScale = Vector3.one;
        
        ParticleSystem particles = particlesObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.5f;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.05f;
        main.startColor = new Color(1f, 0.8f, 0.2f); // Gold color
        
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
        
        // Create world-space canvas for UI text - positioned above the button
        // Canvas faces +Z by default, text renders on -Z side (toward player at z=-23)
        GameObject canvasObj = new GameObject("PedestalCanvas");
        canvasObj.transform.SetParent(pedestal.transform);
        // Position: above the button (y=1.4), slightly in front (z=-0.2, closer to player)
        canvasObj.transform.localPosition = new Vector3(0, 1.4f, -0.2f);
        // No rotation needed - canvas text faces -Z (toward player) by default
        canvasObj.transform.localRotation = Quaternion.identity;
        // Scale: 0.001 means 1 unit in canvas = 0.001 meters = 1mm
        canvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        // 600x400 pixels at 0.001 scale = 0.6m x 0.4m real size
        canvasRect.sizeDelta = new Vector2(600, 400);
        
        // Create text elements with adjusted positions for larger canvas
        GameObject statusTextObj = CreateTextElement(canvasObj.transform, "StatusText", 
            "READY TO PLAY", 72, Color.white, new Vector2(0, 100));
        TextMeshProUGUI statusText = statusTextObj.GetComponent<TextMeshProUGUI>();
        
        GameObject highScoreObj = CreateTextElement(canvasObj.transform, "HighScoreText", 
            "High Score: 0", 48, new Color(1f, 0.8f, 0.2f), new Vector2(0, 0));
        TextMeshProUGUI highScoreText = highScoreObj.GetComponent<TextMeshProUGUI>();
        
        GameObject instructionObj = CreateTextElement(canvasObj.transform, "InstructionText", 
            "Press Button to Start", 36, new Color(0.7f, 0.7f, 0.7f), new Vector2(0, -80));
        TextMeshProUGUI instructionText = instructionObj.GetComponent<TextMeshProUGUI>();
        
        // Add StartPedestalController
        StartPedestalController controller = pedestal.AddComponent<StartPedestalController>();
        
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("statusText").objectReferenceValue = statusText;
        serializedController.FindProperty("highScoreText").objectReferenceValue = highScoreText;
        serializedController.FindProperty("instructionText").objectReferenceValue = instructionText;
        serializedController.ApplyModifiedProperties();
        
        // Add event wirer
        ButtonEventWirer wirer = startButton.AddComponent<ButtonEventWirer>();
        SerializedObject serializedWirer = new SerializedObject(wirer);
        serializedWirer.FindProperty("pedestalController").objectReferenceValue = controller;
        serializedWirer.FindProperty("audioSource").objectReferenceValue = audioSource;
        serializedWirer.FindProperty("buttonParticles").objectReferenceValue = particles;
        serializedWirer.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(pedestal);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("StartPedestalSetup: Start pedestal created successfully!");
        Debug.Log("StartPedestalSetup: Remember to disable VRGameAutoStart to use the pedestal button.");
        
        Selection.activeGameObject = pedestal;
    }

    
    private static GameObject CreateSimpleButton(Transform parent)
    {
        GameObject buttonBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        buttonBase.name = "ButtonBase";
        buttonBase.transform.SetParent(parent);
        buttonBase.transform.localPosition = new Vector3(0, 1.02f, 0);
        buttonBase.transform.localScale = new Vector3(0.2f, 0.02f, 0.2f);
        
        GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        button.name = "StartButton";
        button.transform.SetParent(parent);
        button.transform.localPosition = new Vector3(0, 1.06f, 0);
        button.transform.localScale = new Vector3(0.15f, 0.04f, 0.15f);
        
        Renderer buttonRenderer = button.GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            Material buttonMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            buttonMat.color = new Color(0.8f, 0.1f, 0.1f);
            buttonRenderer.material = buttonMat;
        }
        
        button.AddComponent<XRSimpleInteractable>();
        return button;
    }
    
    private static GameObject CreateTextElement(Transform parent, string name, string text, 
        int fontSize, Color color, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(580, 100);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.enableAutoSizing = false;
        
        return textObj;
    }
    
    private static AudioClip FindClickSound()
    {
        string[] searchPaths = new string[]
        {
            "Assets/Audio",
            "Assets/MRTemplateAssets/Audio",
            "Assets/Samples/XR Interaction Toolkit"
        };
        
        foreach (string path in searchPaths)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { path });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower();
                if (fileName.Contains("click") || fileName.Contains("button") || fileName.Contains("press"))
                {
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                }
            }
        }
        return null;
    }
#endif
}
