using System.Collections;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    public class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager Instance { get; private set; }

        private Coroutine _clipboardCoroutine;
        private const float _clipboardProcessInterval = 0.015f; // Interval in seconds

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                StartClipboardProcessor();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartClipboardProcessor()
        {
            if (_clipboardCoroutine == null)
            {
                _clipboardCoroutine = StartCoroutine(ProcessClipboardQueue());
            }
        }

        public void StopClipboardProcessor()
        {
            if (_clipboardCoroutine != null)
            {
                StopCoroutine(_clipboardCoroutine);
                _clipboardCoroutine = null;
            }
        }

        private IEnumerator ProcessClipboardQueue()
        {
            while (true)
            {
                string message = ClipboardUtils.DequeueMessage();
                if (message != null)
                {
                    try
                    {
                        GUIUtility.systemCopyBuffer = message;
                    }
                    catch (System.Exception ex)
                    {
                        ScreenReaderMod.Logger?.Error(
                            $"Failed to set clipboard text: {ex.Message}"
                        );
                    }

                    yield return new WaitForSeconds(_clipboardProcessInterval);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
