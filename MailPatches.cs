using System;
using System.Collections;
using System.Globalization;
using HarmonyLib;
using RenpyLauncher;
using TMPro;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class MailPatches
    {
        // Rate limiting for mail item selections to prevent spam
        private static MailItemComponent lastSelectedMailItem = null;
        private static string lastAnnouncedMailContent = "";

        // Track mail app state for context-aware announcements
        private static bool isInReadingMode = false;
        private static int lastMailCount = 0;

        // Patch mail item selection for navigation announcements
        [HarmonyPatch(typeof(MailItemComponent), "OnSelect")]
        [HarmonyPostfix]
        public static void MailItemComponent_OnSelect_Postfix(MailItemComponent __instance)
        {
            try
            {
                if (__instance != null && __instance.Mail != null)
                {
                    AnnounceMail(__instance, "selected");
                    lastSelectedMailItem = __instance;
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in MailItemComponent_OnSelect_Postfix: {ex.Message}"
                );
            }
        }

        // Patch mail item pointer hover for mouse navigation
        [HarmonyPatch(typeof(MailItemComponent), "OnPointerEnter")]
        [HarmonyPostfix]
        public static void MailItemComponent_OnPointerEnter_Postfix(MailItemComponent __instance)
        {
            try
            {
                if (__instance != null && __instance.Mail != null)
                {
                    // Only announce on hover if it's a different item than last selected
                    if (__instance != lastSelectedMailItem)
                    {
                        AnnounceMail(__instance, "hovered");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in MailItemComponent_OnPointerEnter_Postfix: {ex.Message}"
                );
            }
        }

        // Patch mail app start to announce initial state
        [HarmonyPatch(typeof(MailApp), "PerformAppStart")]
        [HarmonyPostfix]
        public static void MailApp_PerformAppStart_Postfix(MailApp __instance)
        {
            try
            {
                __instance.StartCoroutine(AnnounceMailAppStart(__instance));
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in MailApp_PerformAppStart_Postfix: {ex.Message}"
                );
            }
        }

        private static IEnumerator AnnounceMailAppStart(MailApp mailApp)
        {
            // Wait a frame to ensure mail items are loaded
            yield return null;

            try
            {
                if (mailApp.NoMailText != null && mailApp.NoMailText.gameObject.activeInHierarchy)
                {
                    // No mail available
                    ClipboardUtils.OutputGameText(
                        "",
                        "Mail app opened, no new emails",
                        TextType.Mail
                    );
                    lastMailCount = 0;
                }
                else
                {
                    // Count visible mail items
                    int mailCount = 0;
                    if (mailApp.ContentPanel != null)
                    {
                        foreach (Transform child in mailApp.ContentPanel.transform)
                        {
                            if (child.GetComponent<MailItemComponent>() != null)
                            {
                                mailCount++;
                            }
                        }
                    }

                    if (mailCount > 0)
                    {
                        string mailText = mailCount == 1 ? "email" : "emails";
                        ClipboardUtils.OutputGameText(
                            "",
                            $"Mail app opened, {mailCount} {mailText}",
                            TextType.Mail
                        );
                        lastMailCount = mailCount;
                    }
                }

                isInReadingMode = false;
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in AnnounceMailAppStart: {ex.Message}");
            }
        }

        // Patch mail opening to announce reading mode and content
        [HarmonyPatch(typeof(MailApp), "PerformOpenMail")]
        [HarmonyPostfix]
        public static void MailApp_PerformOpenMail_Postfix(
            MailApp __instance,
            MailItemComponent mic
        )
        {
            try
            {
                if (mic != null && mic.Mail != null)
                {
                    isInReadingMode = true;

                    // Start a coroutine to announce mail content after it's loaded
                    __instance.StartCoroutine(AnnounceMailContent(__instance, mic));
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in MailApp_PerformOpenMail_Postfix: {ex.Message}"
                );
            }
        }

        private static IEnumerator AnnounceMailContent(MailApp mailApp, MailItemComponent mic)
        {
            // Wait a moment for the mail content to be loaded
            yield return new UnityEngine.WaitForSeconds(0.5f);

            if (mic?.Mail != null)
            {
                try
                {
                    // Get sender name (clean up email format if needed)
                    string fromText = mic.Mail.From;
                    if (fromText.IndexOf('<') >= 0)
                    {
                        fromText = fromText.Substring(0, fromText.IndexOf('<')).Trim();
                    }

                    string subject = Renpy.Text.GetLocalisedString(mic.Mail.Subject);

                    // Announce header
                    string headerAnnouncement =
                        $"Reading email from {fromText}, Subject: {subject}";
                    ClipboardUtils.OutputGameText("", headerAnnouncement, TextType.Mail);
                    lastAnnouncedMailContent = headerAnnouncement;

                    ScreenReaderMod.Logger?.Msg($"Mail opened: {headerAnnouncement}");
                }
                catch (System.Exception ex)
                {
                    ScreenReaderMod.Logger?.Error($"Error announcing mail header: {ex.Message}");
                }
            }

            // Wait a moment before announcing content
            yield return new UnityEngine.WaitForSeconds(0.5f);

            if (mic?.Mail != null)
            {
                try
                {
                    // Get and clean the mail body content (without LocalisedStringPostProcessing parameter)
                    string bodyContent = Renpy.Text.GetLocalisedString(mic.Mail.Body);
                    if (!string.IsNullOrWhiteSpace(bodyContent))
                    {
                        // Clean the body text using the existing TextProcessor
                        string cleanedBody = TextProcessor.CleanText(bodyContent);

                        // Announce the mail body content
                        ClipboardUtils.OutputGameText("", cleanedBody, TextType.Mail);
                        ScreenReaderMod.Logger?.Msg(
                            $"Mail body announced: {cleanedBody.Substring(0, Math.Min(100, cleanedBody.Length))}..."
                        );
                    }
                }
                catch (System.Exception ex)
                {
                    ScreenReaderMod.Logger?.Error($"Error announcing mail content: {ex.Message}");
                }
            }
        }

        // Patch returning to browsing mode
        [HarmonyPatch(typeof(MailApp), "ReturnToBrowsingMode")]
        [HarmonyPostfix]
        public static void MailApp_ReturnToBrowsingMode_Postfix(MailApp __instance)
        {
            try
            {
                if (isInReadingMode)
                {
                    isInReadingMode = false;

                    string mailText = lastMailCount == 1 ? "email" : "emails";
                    string announcement = $"Returned to mail list, {lastMailCount} {mailText}";

                    ClipboardUtils.OutputGameText("", announcement, TextType.Mail);
                    ScreenReaderMod.Logger?.Msg(announcement);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in MailApp_ReturnToBrowsingMode_Postfix: {ex.Message}"
                );
            }
        }

        // Helper method to format and announce mail item information
        private static void AnnounceMail(MailItemComponent mailItem, string action)
        {
            try
            {
                if (mailItem?.Mail == null)
                    return;

                var mail = mailItem.Mail;

                // Get sender name (clean up email format if needed)
                string fromText = mail.From;
                if (fromText.IndexOf('<') >= 0)
                {
                    fromText = fromText.Substring(0, fromText.IndexOf('<')).Trim();
                }

                // Get localized subject
                string subject = Renpy.Text.GetLocalisedString(mail.Subject);

                // Format date/time
                string dateTime = "";
                try
                {
                    CultureInfo currentCultureInfo = Renpy.GetCurrentCultureInfo();
                    DateTime dt = DateTime.FromFileTimeUtc(mail.TimeStamp);
                    if (dt.Date == DateTime.UtcNow.Date)
                    {
                        dateTime = dt.ToString(
                            currentCultureInfo.DateTimeFormat.ShortTimePattern,
                            currentCultureInfo
                        );
                    }
                    else
                    {
                        dateTime = dt.ToString(
                            currentCultureInfo.DateTimeFormat.ShortDatePattern,
                            currentCultureInfo
                        );
                    }
                }
                catch
                {
                    dateTime = "Unknown date";
                }

                // Determine read status
                string readStatus = mail.Read ? "read" : "unread";

                // Format announcement with read status first
                string announcement =
                    $"{readStatus} email {action}: From {fromText}, Subject: {subject}, {dateTime}";

                // Only announce if it's different from the last announcement
                if (announcement != lastAnnouncedMailContent)
                {
                    ClipboardUtils.OutputGameText("", announcement, TextType.Mail);
                    lastAnnouncedMailContent = announcement;

                    ScreenReaderMod.Logger?.Msg($"Mail item {action}: {announcement}");
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in AnnounceMail: {ex.Message}");
            }
        }

        // Patch notification icon updates
        [HarmonyPatch(typeof(MailApp), "RefreshNotificationIcon")]
        [HarmonyPostfix]
        public static void MailApp_RefreshNotificationIcon_Postfix(MailApp __instance)
        {
            try
            {
                if (
                    __instance.notificationIcon != null
                    && __instance.notificationIcon.activeInHierarchy
                )
                {
                    // New mail notification became active
                    ClipboardUtils.OutputGameText("", "New mail notification", TextType.Mail);
                    ScreenReaderMod.Logger?.Msg("Mail notification icon activated");
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in MailApp_RefreshNotificationIcon_Postfix: {ex.Message}"
                );
            }
        }
    }
}
