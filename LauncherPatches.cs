using HarmonyLib;
using RenpyLauncher;
using UnityEngine.UI;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class LauncherPatches
    {
        // Rate limiting for confirmation dialog to prevent spam
        private static string lastConfirmationButtonText = "";
        private static UnityEngine.GameObject lastConfirmationButtonObject = null;

        // Patch button selection for navigation announcements
        [HarmonyPatch(
            typeof(DesktopApp),
            "SelectButton",
            new System.Type[] { typeof(StartMenuButton) }
        )]
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
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopApp_SelectButton_Postfix: {ex.Message}"
                );
            }
        }

        // Confirmation dialog accessibility
        [HarmonyPatch(typeof(DesktopConfirmationWindow), "ShowMessageDialog")]
        [HarmonyPostfix]
        public static void DesktopConfirmationWindow_ShowMessageDialog_Postfix(
            DesktopConfirmationWindow __instance,
            string message,
            string header
        )
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    ClipboardUtils.OutputGameText("", header, TextType.SystemMessage);
                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopConfirmationWindow_ShowMessageDialog_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(DesktopConfirmationWindow), "UpdateSelection")]
        [HarmonyPostfix]
        public static void DesktopConfirmationWindow_UpdateSelection_Postfix(
            DesktopConfirmationWindow __instance
        )
        {
            try
            {
                var currentSelected = UnityEngine
                    .EventSystems
                    .EventSystem
                    .current
                    ?.currentSelectedGameObject;
                if (currentSelected != null)
                {
                    string buttonText = "";
                    if (
                        currentSelected == __instance.ConfirmButton?.gameObject
                        && __instance.confirmText?.text != null
                    )
                    {
                        buttonText = __instance.confirmText.text;
                    }
                    else if (
                        currentSelected == __instance.CancelButton?.gameObject
                        && __instance.backText?.text != null
                    )
                    {
                        buttonText = __instance.backText.text;
                    }
                    else if (
                        currentSelected == __instance.OKButton?.gameObject
                        && __instance.okText?.text != null
                    )
                    {
                        buttonText = __instance.okText.text;
                    }

                    if (!string.IsNullOrWhiteSpace(buttonText))
                    {
                        // Only announce if this is a different button than last time
                        if (currentSelected != lastConfirmationButtonObject)
                        {
                            lastConfirmationButtonText = buttonText;
                            lastConfirmationButtonObject = currentSelected;

                            if (ModConfig.Instance.EnableVerboseLogging)
                                ScreenReaderMod.Logger?.Msg(
                                    $"Confirmation button focused: {buttonText}"
                                );
                            ClipboardUtils.OutputGameText("", buttonText, TextType.Menu);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopConfirmationWindow_UpdateSelection_Postfix: {ex.Message}"
                );
            }
        }
    }
}
