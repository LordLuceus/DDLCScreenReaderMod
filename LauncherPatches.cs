using HarmonyLib;
using RenpyLauncher;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class LauncherPatches
    {
        // Patch button selection for navigation announcements
        [HarmonyPatch(typeof(DesktopApp), "SelectButton", new System.Type[] { typeof(StartMenuButton) })]
        [HarmonyPostfix]
        public static void DesktopApp_SelectButton_Postfix(DesktopApp __instance, StartMenuButton b)
        {
            try
            {
                if (b != null && b.Text != null && !string.IsNullOrWhiteSpace(b.Text.text))
                {
                    string buttonText = b.Text.text;
                    if (ModConfig.Instance.EnableVerboseLogging)
                        ScreenReaderMod.Logger?.Msg($"Launcher button selected: {buttonText}");
                    ClipboardUtils.OutputGameText("", buttonText, TextType.Menu);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in DesktopApp_SelectButton_Postfix: {ex.Message}");
            }
        }
    }
}