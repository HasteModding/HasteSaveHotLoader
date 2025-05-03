using System.Text;
using Landfall.Modding;
using NativeFileBrowser;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using Zorro.Localization;
using Button = UnityEngine.UI.Button;

namespace HasteSaveHotLoader;

internal static class Extensions {
    internal static GameObject FindObjectByName(string name) {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < objs.Length; i++) {
            if (objs[i].name == name) {
                return objs[i].gameObject;
            }
        }
        return null!;
    }
}

[LandfallPlugin]
public static class HSHProgram {
    private static GameObject _button;

    static HSHProgram() {
        _button = null!;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static bool HotLoadSave(string filename, int slot = 4200) {
        try {
            Debug.Log("Save file downloaded successfully.");
            byte[] data = File.ReadAllBytes(filename);
            string json = Encoding.ASCII.GetString(data);
            HasteSave save = JsonUtility.FromJson<HasteSave>(json);
            SaveSystem.CurrentSlot = slot;
            SaveSystem.SaveSlots[SaveSystem.CurrentSlot] = save;
            Debug.Log("Save file loaded successfully.");
            return true;
        }
        catch {
            Debug.LogError("Save file not loaded, failed...");
            return false;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "MainMenu") {
            Debug.Log("MainMenu scene loaded");
            // Find the Continue button in the Main Menu scene
            GameObject continueButton = Extensions.FindObjectByName("ContinueButton");
            if (continueButton != null) {
                CreateButtonFromTemplate(continueButton);
            }
            else {
                Debug.LogError("Could not find a button to instantiate the HotLoad button from.");
            }
        }
    }

    private static void CreateButtonFromTemplate(GameObject template) {
        // Instantiate the button and set its properties
        _button = GameObject.Instantiate(template, template.transform.parent);
        _button.name = "HSHButton";
        _button.transform.SetAsFirstSibling();

        LocalizeUIText localizeUIText = _button.GetComponentInChildren<LocalizeUIText>();
        localizeUIText.String = new LocalizedString("HasteSaveHotLoader", "MainMenuButton");

        Button button = _button.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnHotLoadClicked);
    }

    private static void OnHotLoadClicked() {
        // Show the menu when the button is clicked
        Debug.Log("HotLoad button clicked");

        string filename = FileBrowser.PickFile();

        Debug.Log($"Selected file: {filename}");

        // Load the save file
        bool loaded = HotLoadSave(filename);
        if (!loaded) {
            Debug.LogError("Failed to load save file.");
            return;
        }

        GM_API.OnMainMenuPlayButton();
        MonoFunctions.DelayCall(new Action(GameHandler.PlayFromMenu), 0.5f);

        Debug.Log("Save file loaded successfully.");
    }
}
