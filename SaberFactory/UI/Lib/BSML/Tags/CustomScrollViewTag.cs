using System.Collections;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using HMUI;
using IPA.Utilities;
using SaberFactory.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;

namespace SaberFactory.UI.Lib.BSML.Tags
{
    public class CustomScrollViewTag : BSMLTag
    {
        public override string[] Aliases => new[] { CustomComponentHandler.ComponentPrefix + ".scroll-view" };

        private static TextPageScrollView _textPageScrollViewTemplate;
        private static IVRPlatformHelper _platformHelper;

        public override GameObject CreateObject(Transform parent)
        {
            if (_textPageScrollViewTemplate == null)
            {
                _textPageScrollViewTemplate = Resources.FindObjectsOfTypeAll<TextPageScrollView>().FirstOrDefault(x => x.name == "TextPageScrollView");
            }

            if (_platformHelper == null)
            {
                foreach (var sv in Resources.FindObjectsOfTypeAll<ScrollView>())
                {
                    var helper = sv.GetField<IVRPlatformHelper, ScrollView>("_platformHelper");
                    if (helper != null)
                    {
                        _platformHelper = helper;
                        break;
                    }
                }
            }

            if (_textPageScrollViewTemplate == null)
            {
                return new GameObject("CustomScrollView_Fallback");
            }

            var textScrollView = Object.Instantiate(_textPageScrollViewTemplate, parent);
            textScrollView.name = "BSMLScrollView";
            var pageUpButton = textScrollView.GetField<Button, ScrollView>("_pageUpButton");
            var pageDownButton = textScrollView.GetField<Button, ScrollView>("_pageDownButton");
            var verticalScrollIndicator = textScrollView.GetField<VerticalScrollIndicator, ScrollView>("_verticalScrollIndicator");

            var viewport = textScrollView.GetField<RectTransform, ScrollView>("_viewport");
            viewport.gameObject.AddComponent<VRGraphicRaycaster>().SetField("_physicsRaycaster", BeatSaberUI.PhysicsRaycasterWithCache);

            Object.Destroy(textScrollView.GetField<TextMeshProUGUI, TextPageScrollView>("_text").gameObject);
            var gameObject = textScrollView.gameObject;
            Object.Destroy(textScrollView);
            gameObject.SetActive(false);

            var scrollView = gameObject.AddComponent<BSMLScrollView>();
            if (_platformHelper != null)
            {
                scrollView.SetField<ScrollView, IVRPlatformHelper>("_platformHelper", _platformHelper);
            }
            scrollView.SetField<ScrollView, Button>("_pageUpButton", pageUpButton);
            scrollView.SetField<ScrollView, Button>("_pageDownButton", pageDownButton);
            scrollView.SetField<ScrollView, VerticalScrollIndicator>("_verticalScrollIndicator", verticalScrollIndicator);
            scrollView.SetField<ScrollView, RectTransform>("_viewport", viewport);

            viewport.anchorMin = new Vector2(0, 0);
            viewport.anchorMax = new Vector2(1, 1);

            var parentObj = new GameObject();
            parentObj.name = "BSMLScrollViewContent";
            parentObj.transform.SetParent(viewport, false);

            var contentSizeFitter = parentObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var verticalLayout = parentObj.AddComponent<VerticalLayoutGroup>();
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = false;
            verticalLayout.childControlHeight = true;
            verticalLayout.childControlWidth = true;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;

            var rectTransform = parentObj.transform.AsRectTransform();

            parentObj.AddComponent<LayoutElement>();
            var scrollViewContent = parentObj.AddComponent<ScrollViewContent>();
            #if V_1_29_1
            scrollViewContent.scrollView = scrollView;
            #else
            scrollViewContent.ScrollView = scrollView;
            #endif

            var child = new GameObject();
            child.name = "BSMLScrollViewContentContainer";
            child.transform.SetParent(rectTransform, false);

            var layoutGroup = child.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 0.5f;

            parentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            child.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            child.AddComponent<LayoutElement>();
            var externalComponents = child.AddComponent<ExternalComponents>();
            #if V_1_29_1
            externalComponents.components.Add(scrollView);
            externalComponents.components.Add(scrollView.transform);
            #else
            externalComponents.Components.Add(scrollView);
            externalComponents.Components.Add(scrollView.transform);
            #endif
            var childRect = child.transform.AsRectTransform();
            childRect.anchorMin = new Vector2(0, 1);
            childRect.anchorMax = new Vector2(1, 1);
            childRect.pivot = new Vector2(0.5f, 1);
            childRect.sizeDelta = new Vector2(0, 0);

            var rootRect = gameObject.transform.AsRectTransform();
            rootRect.anchorMin = new Vector2(0, 0);
            rootRect.anchorMax = new Vector2(1, 1);
            rootRect.sizeDelta = new Vector2(-4, -4);
            rootRect.anchoredPosition = new Vector2(0, 0);

            scrollView.SetField<ScrollView, RectTransform>("_contentRectTransform", parentObj.transform as RectTransform);

            var runner = new GameObject("SF_ScrollViewRunner");
            runner.AddComponent<CoroutineStarter>().Run(SetupScrollView(gameObject, rectTransform, runner));

            return child;
        }

        private IEnumerator SetupScrollView(GameObject gameObject, RectTransform rectTransform, GameObject runner)
        {
            gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.sizeDelta = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 1);
            Object.Destroy(runner);
        }

        private class CoroutineStarter : MonoBehaviour
        {
            public void Run(IEnumerator routine)
            {
                StartCoroutine(routine);
            }
        }
    }
}