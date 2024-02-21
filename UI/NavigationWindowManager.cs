using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Mineant
{

    public class NavigationWindowManager : MonoBehaviour
    {
        public enum ButtonHideModes { None, Interactable, SetActive }

        [System.Serializable]
        public class NavWindow
        {
            public GameObject Window;
            public GameObject Interactor;
        }

        public bool HideWithCanvasGroup;
        public bool SetFirstMenuOnEnable;
        public List<NavWindow> DefaultNavWindows;
        public List<NavWindow> CurrentNavWindows;

        [Header("Pages")]
        public ButtonHideModes ButtonHideMode;
        public Button NextPageButton;
        public Button PrevPageButton;
        public TMP_Text NavPageText;

        [Header("Text")]
        public bool ChangeTextColor;
        public Color EnabledColor = Color.white;
        public Color DisabledColor = Color.white;

        public Action OnNavWindowChanged;

        public int CurrentPage { get; protected set; }
        // protected GameObject _selectedMenu;

        void Awake()
        {
            foreach (var navWindow in DefaultNavWindows)
            {
                AddNavigationWindow(navWindow);
            }

            OpenDefaultNavWindow();
            if (NextPageButton) NextPageButton.onClick.AddListener(NextPage);
            if (PrevPageButton) PrevPageButton.onClick.AddListener(PrevPage);
        }

        public void AddNavigationWindow(NavWindow navWindow)
        {
            if (navWindow.Interactor != null)
            {
                if (navWindow.Interactor.TryGetComponent<Button>(out var button))
                {
                    button.onClick.AddListener(() => { OpenNav(navWindow); });
                }
                else if (navWindow.Interactor.TryGetComponent<Toggle>(out var toggle))
                {
                    toggle.onValueChanged.AddListener((value) =>
                    {
                        if (value)
                            OpenNav(navWindow);
                    });
                }
            }

            CurrentNavWindows.Add(navWindow);
            UpdateChangePageButtons();
        }

        private void PrevPage()
        {
            ChangePage(-1);
        }

        private void NextPage()
        {
            ChangePage(1);
        }


        private void ChangePage(int change)
        {
            if (!CurrentNavWindows.IndexWithinRange(CurrentPage + change)) return;

            OpenNav(CurrentPage + change);
        }

        public void OpenNav(int index)
        {
            if (!CurrentNavWindows.IndexWithinRange(index))
            {
                Debug.LogWarning("Index is not within range, will just open the first menu");
                index = 0;
            }

            OpenNav(CurrentNavWindows[index]);
        }

        private void OpenNav(NavWindow targetNav)
        {
            // if (targetNav.Menu != null) _selectedMenu = targetNav.Menu;
            CurrentPage = CurrentNavWindows.IndexOf(targetNav);

            if (targetNav.Interactor != null)
            {
                if (targetNav.Interactor.TryGetComponent<Button>(out var button))
                {

                }
                else if (targetNav.Interactor.TryGetComponent<Toggle>(out var toggle))
                {
                    if (toggle.isOn == false)
                        toggle.isOn = true;
                }
            }


            foreach (var nav in CurrentNavWindows)
            {
                bool enable = targetNav == nav;
                if (nav.Window != null)
                {
                    if (HideWithCanvasGroup)
                        nav.Window.GetComponent<CanvasGroup>().EnableCanvasGroup(enable);
                    else
                        nav.Window.gameObject.SetActive(enable);
                }

                if (ChangeTextColor)
                {
                    nav.Interactor.GetComponentInChildren<TMP_Text>().color = enable ? EnabledColor : DisabledColor;
                }
            }

            UpdateChangePageButtons();
            if (NavPageText) NavPageText.text = $"{CurrentPage + 1} / {CurrentNavWindows.Count}";
            if (OnNavWindowChanged != null) OnNavWindowChanged.Invoke();
        }

        private void UpdateChangePageButtons()
        {
            if (NextPageButton) SetButton(NextPageButton, CurrentPage < CurrentNavWindows.Count - 1);
            if (PrevPageButton) SetButton(PrevPageButton, CurrentPage > 0);
        }

        public void OpenDefaultNavWindow()
        {
            if (CurrentNavWindows.Count == 0) return;

            OpenNav(CurrentNavWindows[0]);
        }

        protected void SetButton(Button button, bool active)
        {
            switch (ButtonHideMode)
            {
                case (ButtonHideModes.Interactable):
                    button.interactable = active;
                    break;
                case (ButtonHideModes.SetActive):
                    button.gameObject.SetActive(active);
                    break;
            }
        }

        void OnEnable()
        {
            if (SetFirstMenuOnEnable) OpenDefaultNavWindow();
            // if (_selectedMenu == null) OpenNav(Navs[0]);
        }


    }


}