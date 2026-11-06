using System.Collections;
using System.Collections.Generic;
using BZApp.GUI.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BZApp.Prototyping
{
    public class SplashScreenBehaviour : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private string sceneToLoad = "LoaderScene";
        
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
        
        // Internal references
        private AsyncOperation asyncLevelLoad;

        private void Start() => StartCoroutine(StartBehaviour());

        private IEnumerator StartBehaviour()
        {
            // Initial loading stage
            yield return new WaitForSeconds(startDelayTime);
            asyncLevelLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            asyncLevelLoad.allowSceneActivation = false;
            while (asyncLevelLoad.progress < 0.9f)
                yield return null;
            
            // Initial splash fade out
            Image[] bzAppSplashImages = initialSplashGroup.GetComponentsInChildren<Image>();
            yield return StartCoroutine(FadeSplashImages(bzAppSplashImages, FadeType.FadeOut, initialSplashFadeTime));
            initialSplashGroup.SetActive(false);
            
            // Teams logos fade in/out stage
            yield return new WaitForSeconds(initialSplashWaitAfterFadeTime);
            teamsLogosSplashGroup.SetActive(true);
            Image[] teamsLogosSplashImages = teamsLogosSplashGroup.GetComponentsInChildren<Image>();
            yield return StartCoroutine(FadeSplashImages(teamsLogosSplashImages, FadeType.FadeIn, logosFadeTime));
            yield return new WaitForSeconds(logosHoldTime);
            yield return StartCoroutine(FadeSplashImages(teamsLogosSplashImages, FadeType.FadeOut, logosFadeTime));
            yield return new WaitForSeconds(logosWaitAfterFadeTime);
            teamsLogosSplashGroup.SetActive(false);
            
            asyncLevelLoad.allowSceneActivation = true;
        }

        private IEnumerator FadeSplashImages(IReadOnlyList<Image> splashImages, FadeType fadeType, float fadeTime)
        {
            Color startingColour = fadeType == FadeType.FadeIn ? ExtColour.ClearWhite : Color.white;
            Color endingColour = fadeType == FadeType.FadeIn ? Color.white : ExtColour.ClearWhite;
            float timeElapsed = 0;
            while (timeElapsed < fadeTime)
            {
                timeElapsed += Time.deltaTime;
                foreach (Image bzAppSplashImage in splashImages)
                    bzAppSplashImage.color = Color.Lerp(startingColour, endingColour, timeElapsed / fadeTime);
                yield return null;
            }
        }
    }
}
