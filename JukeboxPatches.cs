using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class JukeboxPatches
    {
        [HarmonyPatch(typeof(JukeboxApp), "OnWindowActive")]
        [HarmonyPostfix]
        public static void JukeboxApp_OnWindowActive_Postfix(JukeboxApp __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Jukebox app opened");
                ClipboardUtils.OutputGameText("", "Jukebox opened", TextType.Jukebox);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_OnWindowActive_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "OnAppClose")]
        [HarmonyPostfix]
        public static void JukeboxApp_OnAppClose_Postfix(JukeboxApp __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Jukebox app closed");
                ClipboardUtils.OutputGameText("", "Jukebox closed", TextType.Jukebox);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_OnAppClose_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "ShowAllTracks")]
        [HarmonyPostfix]
        public static void JukeboxApp_ShowAllTracks_Postfix(JukeboxApp __instance)
        {
            try
            {
                if (__instance.JukeboxPlayer == null)
                    return;

                var playlist = __instance.JukeboxPlayer.GetPlaylist(
                    JukeboxPlayer.Playlist.AllTracks
                );
                int unlockedCount = 0;
                foreach (var track in playlist)
                {
                    if (track.Unlocked)
                        unlockedCount++;
                }

                string announcement = $"All tracks playlist, {unlockedCount} tracks available";
                ScreenReaderMod.Logger?.Msg($"Switched to all tracks playlist: {announcement}");
                ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_ShowAllTracks_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "ShowCustomPlaylist")]
        [HarmonyPostfix]
        public static void JukeboxApp_ShowCustomPlaylist_Postfix(JukeboxApp __instance)
        {
            try
            {
                if (__instance.JukeboxPlayer == null)
                    return;

                var playlist = __instance.JukeboxPlayer.GetPlaylist(
                    JukeboxPlayer.Playlist.CustomPlaylist
                );
                string announcement = $"Custom playlist, {playlist.Count} tracks";
                ScreenReaderMod.Logger?.Msg($"Switched to custom playlist: {announcement}");
                ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_ShowCustomPlaylist_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(TrackListingEntry), "OnSelect")]
        [HarmonyPostfix]
        public static void TrackListingEntry_OnSelect_Postfix(
            TrackListingEntry __instance,
            BaseEventData eventData
        )
        {
            try
            {
                if (__instance.TrackInfo == null)
                    return;

                var trackInfo = __instance.TrackInfo;
                string trackNumber = __instance.TrackNumber.text;
                string trackName = trackInfo.TrackName;
                string trackArtist = trackInfo.TrackArtist;

                int minutes = (int)System.Math.Floor(trackInfo.Length / 60f);
                int seconds = (int)System.Math.Floor(trackInfo.Length - (float)minutes * 60f);
                string duration = $"{minutes}:{seconds:D2}";

                string announcement =
                    $"Track {trackNumber}: {trackName} by {trackArtist}, {duration}";

                ScreenReaderMod.Logger?.Msg($"Track selected: {announcement}");
                ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in TrackListingEntry_OnSelect_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(TrackListingEntry), "OnClicked")]
        [HarmonyPostfix]
        public static void TrackListingEntry_OnClicked_Postfix(TrackListingEntry __instance)
        {
            try
            {
                if (__instance.TrackInfo == null)
                    return;

                string trackName = __instance.TrackInfo.TrackName;
                string announcement;

                if (__instance.PlayingObject.activeSelf)
                {
                    announcement = $"Toggling playback for {trackName}";
                }
                else
                {
                    announcement = $"Playing {trackName}";
                }

                ScreenReaderMod.Logger?.Msg($"Track clicked: {announcement}");
                ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in TrackListingEntry_OnClicked_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "Play", new System.Type[] { })]
        [HarmonyPrefix]
        public static void JukeboxApp_Play_NoArgs_Prefix(JukeboxApp __instance)
        {
            try
            {
                if (__instance.JukeboxPlayer == null)
                    return;

                var currentTrack = __instance.JukeboxPlayer.GetCurrentTrack();
                if (currentTrack.CurrentTrackValid())
                {
                    var trackInfo = __instance.JukeboxPlayer.GetCurrentTrackEntry();
                    if (trackInfo != null)
                    {
                        var playerState = __instance.JukeboxPlayer.GetPlayerState();
                        string announcement =
                            playerState == JukeboxPlayer.PlayerState.Playing
                                ? $"Stopping {trackInfo.TrackName}"
                                : $"Playing {trackInfo.TrackName}";

                        ScreenReaderMod.Logger?.Msg($"Play button pressed: {announcement}");
                        ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                    }
                }
                else
                {
                    ScreenReaderMod.Logger?.Msg("Play pressed with no current track");
                    ClipboardUtils.OutputGameText("", "Starting playback", TextType.Jukebox);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_Play_NoArgs_Prefix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "SkipForward")]
        [HarmonyPostfix]
        public static void JukeboxApp_SkipForward_Postfix(JukeboxApp __instance)
        {
            try
            {
                if (__instance.JukeboxPlayer == null)
                    return;

                var trackInfo = __instance.JukeboxPlayer.GetCurrentTrackEntry();
                if (trackInfo != null)
                {
                    string announcement = $"Skipped to {trackInfo.TrackName}";
                    ScreenReaderMod.Logger?.Msg($"Skip forward: {announcement}");
                    ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                }
                else
                {
                    ScreenReaderMod.Logger?.Msg("Skip forward pressed");
                    ClipboardUtils.OutputGameText("", "Skipped forward", TextType.Jukebox);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_SkipForward_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "SkipBackward")]
        [HarmonyPostfix]
        public static void JukeboxApp_SkipBackward_Postfix(JukeboxApp __instance)
        {
            try
            {
                if (__instance.JukeboxPlayer == null)
                    return;

                var trackInfo = __instance.JukeboxPlayer.GetCurrentTrackEntry();
                if (trackInfo != null)
                {
                    string announcement = $"Skipped to {trackInfo.TrackName}";
                    ScreenReaderMod.Logger?.Msg($"Skip backward: {announcement}");
                    ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                }
                else
                {
                    ScreenReaderMod.Logger?.Msg("Skip backward pressed");
                    ClipboardUtils.OutputGameText("", "Skipped backward", TextType.Jukebox);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_SkipBackward_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "ToggleLoop")]
        [HarmonyPostfix]
        public static void JukeboxApp_ToggleLoop_Postfix(JukeboxApp __instance, Toggle toggle)
        {
            try
            {
                string state = toggle.isOn ? "enabled" : "disabled";
                string announcement = $"Loop {state}";
                ScreenReaderMod.Logger?.Msg($"Loop toggled: {announcement}");
                ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_ToggleLoop_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(JukeboxApp), "AddToCustomPlaylist")]
        [HarmonyPostfix]
        public static void JukeboxApp_AddToCustomPlaylist_Postfix(
            JukeboxApp __instance,
            TrackListingEntry entry
        )
        {
            try
            {
                if (entry?.TrackInfo != null)
                {
                    string announcement = $"Added {entry.TrackInfo.TrackName} to custom playlist";
                    ScreenReaderMod.Logger?.Msg($"Track added to custom playlist: {announcement}");
                    ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_AddToCustomPlaylist_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(
            typeof(JukeboxApp),
            "RemoveFromCustomPlaylist",
            new System.Type[] { typeof(TrackListingEntry) }
        )]
        [HarmonyPostfix]
        public static void JukeboxApp_RemoveFromCustomPlaylist_Postfix(
            JukeboxApp __instance,
            TrackListingEntry entry
        )
        {
            try
            {
                if (entry?.TrackInfo != null)
                {
                    string announcement =
                        $"Removed {entry.TrackInfo.TrackName} from custom playlist";
                    ScreenReaderMod.Logger?.Msg(
                        $"Track removed from custom playlist: {announcement}"
                    );
                    ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxApp_RemoveFromCustomPlaylist_Postfix: {ex.Message}"
                );
            }
        }

        private static Button GetSkipBackButton(JukeboxInfoPanelComponent component)
        {
            return component?.SkipBack;
        }

        private static Button GetSkipForwardButton(JukeboxInfoPanelComponent component)
        {
            return component?.SkipForward;
        }

        private static Button GetPlayPauseButton(JukeboxInfoPanelComponent component)
        {
            return component?.PlayPause;
        }

        private static Toggle GetLoopToggle(JukeboxInfoPanelComponent component)
        {
            return component?.Loop;
        }

        [HarmonyPatch(typeof(JukeboxInfoPanelComponent), "OnEventTrigger")]
        [HarmonyPostfix]
        public static void JukeboxInfoPanelComponent_OnEventTrigger_Postfix(
            JukeboxInfoPanelComponent __instance,
            BaseEventData eventData
        )
        {
            try
            {
                var jukeboxApp = __instance.GetComponentInParent<JukeboxApp>();
                if (jukeboxApp == null || jukeboxApp.JukeboxPlayer == null)
                    return;

                GameObject selectedObject = eventData?.selectedObject;
                if (selectedObject == null)
                    return;

                string announcement = "";

                if (selectedObject == __instance.SkipBack?.gameObject)
                {
                    announcement = "Skip backward button";
                }
                else if (selectedObject == __instance.SkipForward?.gameObject)
                {
                    announcement = "Skip forward button";
                }
                else if (selectedObject == __instance.PlayPause?.gameObject)
                {
                    var playerState = jukeboxApp.JukeboxPlayer.GetPlayerState();
                    var currentTrack = jukeboxApp.JukeboxPlayer.GetCurrentTrackEntry();

                    if (playerState == JukeboxPlayer.PlayerState.Playing && currentTrack != null)
                    {
                        announcement = $"Stop button, currently playing {currentTrack.TrackName}";
                    }
                    else if (
                        playerState == JukeboxPlayer.PlayerState.Stopped
                        && currentTrack != null
                    )
                    {
                        announcement = $"Play button, stopped on {currentTrack.TrackName}";
                    }
                    else
                    {
                        announcement = "Play button";
                    }
                }
                else if (selectedObject == __instance.Loop?.gameObject)
                {
                    string state = __instance.Loop.isOn ? "enabled" : "disabled";
                    announcement = $"Loop toggle, currently {state}";
                }

                if (!string.IsNullOrEmpty(announcement))
                {
                    ScreenReaderMod.Logger?.Msg($"Jukebox control focused: {announcement}");
                    ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in JukeboxInfoPanelComponent_OnEventTrigger_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(Selectable), "OnSelect")]
        [HarmonyPostfix]
        public static void Selectable_OnSelect_Postfix(
            Selectable __instance,
            BaseEventData eventData
        )
        {
            try
            {
                var jukeboxApp = __instance.GetComponentInParent<JukeboxApp>();
                if (jukeboxApp == null)
                    return;

                // Handle tab navigation
                if (__instance == jukeboxApp.AllSongsToggle)
                {
                    // Check if the All Songs playlist panel is currently active
                    bool isCurrentlySelected = jukeboxApp.PlayListParents[0].activeSelf;
                    string announcement = isCurrentlySelected
                        ? "All Songs tab, selected"
                        : "All Songs tab";
                    ScreenReaderMod.Logger?.Msg($"All Songs tab focused: {announcement}");
                    ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                }
                else if (__instance == jukeboxApp.CustomSongsToggle)
                {
                    // Check if the Custom playlist panel is currently active
                    bool isCurrentlySelected = jukeboxApp.PlayListParents[1].activeSelf;
                    string announcement = isCurrentlySelected
                        ? "Custom Songs tab, selected"
                        : "Custom Songs tab";
                    ScreenReaderMod.Logger?.Msg($"Custom Songs tab focused: {announcement}");
                    ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                }

                // Handle add/remove playlist buttons on track entries
                var trackEntry = __instance.GetComponentInParent<TrackListingEntry>();
                if (trackEntry != null)
                {
                    var toggle = __instance as Toggle;
                    if (toggle != null)
                    {
                        if (
                            toggle == trackEntry.AddToPlaylistButton
                            && trackEntry.AddToPlaylistButton.gameObject.activeSelf
                        )
                        {
                            string buttonState = toggle.interactable ? "" : ", disabled";
                            string announcement =
                                $"Add {trackEntry.TrackInfo.TrackName} to playlist{buttonState}";
                            ScreenReaderMod.Logger?.Msg(
                                $"Add to playlist button focused: {announcement}"
                            );
                            ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                        }
                        else if (
                            toggle == trackEntry.RemoveFromPlaylistButton
                            && trackEntry.RemoveFromPlaylistButton.gameObject.activeSelf
                        )
                        {
                            string announcement =
                                $"Remove {trackEntry.TrackInfo.TrackName} from playlist";
                            ScreenReaderMod.Logger?.Msg(
                                $"Remove from playlist button focused: {announcement}"
                            );
                            ClipboardUtils.OutputGameText("", announcement, TextType.Jukebox);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in Selectable_OnSelect_Postfix: {ex.Message}"
                );
            }
        }
    }
}
