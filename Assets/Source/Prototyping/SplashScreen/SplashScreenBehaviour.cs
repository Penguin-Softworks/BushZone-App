using System.Collections;
using System.Collections.Generic;
using BZApp.GUI.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BZApp.Prototyping
{
    public class SplashScreenBehaviour : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private string sceneToLoad = "LoaderScene";
        [SerializeField] private InputActionReference[] fadeSkipActions;
        
        [Header("Timing Settings")]
        [SerializeField] private float startDelayTime = 0.5f;
        [SerializeField] private float initialSplashFadeTime = 0.5f;
        [SerializeField] private float initialSplashWaitAfterFadeTime = 0.5f;
        [SerializeField] private float logosFadeTime = 1f;
        [SerializeField] private float logosHoldTime = 3f;
        [SerializeField] private float logosWaitAfterFadeTime = 0.5f;
        
        [Header("Object References")]
        [SerializeField] private GameObject initialSplashGroup;
        [SerializeField] private GameObject teamsLogosSplashGroup;

        // Internal variables
        private enum FadeType { FadeIn, FadeOut }
        private bool skipFade = false;
        
        // Internal references
        private AsyncOperation asyncLevelLoad;
        private Coroutine currentFadeCoroutine;

        private void Start() => StartCoroutine(StartBehaviour());

        private IEnumerator StartBehaviour()
        {
            // Initial loading stage
            yield return new WaitForSeconds(startDelayTime);
            asyncLevelLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            if (asyncLevelLoad == null) yield break;
            asyncLevelLoad.allowSceneActivation = false;
            while (asyncLevelLoad.progress < 0.9f)
                yield return null;
            
            // Initial splash fade out
            Image[] bzAppSplashImages = initialSplashGroup.GetComponentsInChildren<Image>();
            yield return FadeSplashImages(bzAppSplashImages, FadeType.FadeOut, initialSplashFadeTime);
            initialSplashGroup.SetActive(false);
            
            // Teams logos fade in/out stage
            yield return new WaitForSeconds(initialSplashWaitAfterFadeTime);
            teamsLogosSplashGroup.SetActive(true);
            Image[] teamsLogosSplashImages = teamsLogosSplashGroup.GetComponentsInChildren<Image>();
            yield return StartCancellableFade(teamsLogosSplashImages, FadeType.FadeIn, logosFadeTime);
            if (!skipFade) yield return StartCancellableWait(logosHoldTime);
            yield return FadeSplashImages(teamsLogosSplashImages, FadeType.FadeOut, logosFadeTime);
            yield return new WaitForSeconds(logosWaitAfterFadeTime);
            teamsLogosSplashGroup.SetActive(false);
            
            asyncLevelLoad.allowSceneActivation = true;
        }

        private IEnumerator StartCancellableFade(IReadOnlyList<Image> splashImages, FadeType fadeType, float fadeTime)
        {
            skipFade = false;
            currentFadeCoroutine = StartCoroutine(FadeSplashImages(splashImages, fadeType, fadeTime));
            foreach (var action in fadeSkipActions)
                action.action.performed += HandleFadeSkip;
            while (currentFadeCoroutine != null && !skipFade) 
                yield return null;
            foreach (var action in fadeSkipActions)
                action.action.performed -= HandleFadeSkip;
            if (currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);
            foreach (Image img in splashImages)
                img.color = fadeType == FadeType.FadeIn ? Color.white : ExtColour.ClearWhite;
        }

        private IEnumerator FadeSplashImages(IReadOnlyList<Image> splashImages, FadeType fadeType, float fadeTime)
        {
            Color startingColour = fadeType == FadeType.FadeIn ? ExtColour.ClearWhite : Color.white;
            Color endingColour = fadeType == FadeType.FadeIn ? Color.white : ExtColour.ClearWhite;
            float timeElapsed = 0;
            while (timeElapsed < fadeTime)
            {
                timeElapsed += Time.deltaTime;
                foreach (Image img in splashImages)
                    img.color = Color.Lerp(startingColour, endingColour, timeElapsed / fadeTime);
                yield return null;
            }
            currentFadeCoroutine = null;
        }

        private IEnumerator StartCancellableWait(float waitTime)
        {
            skipFade = false;
            foreach (var action in fadeSkipActions)
                action.action.performed += HandleFadeSkip;
            float timeElapsed = 0;
            while (!skipFade && timeElapsed < waitTime)
            {
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            foreach (var action in fadeSkipActions)
                action.action.performed -= HandleFadeSkip;
        }

        private void HandleFadeSkip(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            skipFade = true;
        }
    }
}
