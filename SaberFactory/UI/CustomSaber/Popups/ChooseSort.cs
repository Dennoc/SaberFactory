using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using SaberFactory.UI.CustomSaber.CustomComponents;
using SaberFactory.UI.Lib;
using UnityEngine;

namespace SaberFactory.UI.CustomSaber.Popups
{
    internal class ChooseSort : Popup
    {
        public enum ESortMode
        {
            Name,
            Date,
            Size,
            Author
        }

        [UIComponent("sort-list")] private readonly CustomList _sortList = null;

        public bool ShouldScrollToTop { get; set; } = true;

        private Action<ESortMode, string> _onSelectionChanged;
        
        private ESortMode _sortMode = default;
        [UIValue("SaberSearch")] private string _saberSearch; // Added these values to rebuild the state before exiting last time.


        public async void Show(Action<ESortMode, string> onSelectionChanged, string currentFilter, ESortMode currentSortMode)
        {
            _onSelectionChanged = onSelectionChanged;
            _saberSearch = currentFilter;
            _sortMode = currentSortMode;

            var modes = new List<SortModeItem>();
            foreach (var mode in (ESortMode[])Enum.GetValues(typeof(ESortMode)))
            {
                modes.Add(new SortModeItem(mode));
            }

            _ = Create(true);
            _sortList.OnItemSelected += SortSelected;
            _sortList.SetItems(modes);

            await AnimateIn();
        }

        private void SortSelected(ICustomListItem item)
        {
            _onSelectionChanged?.Invoke(((SortModeItem)item).SortMode, _saberSearch);
            Exit();
        }

        private async void Exit()
        {
            _onSelectionChanged = null;

            _sortList.OnItemSelected -= SortSelected;
            await Hide(true);
        }

        [UIAction("click-cancel")]
        private void ClickSelect()
        {
            Exit();
        }
        
        [UIAction("on-saber-search-change")]
        private void OnSaberSearchChange(string value)
        {
            _onSelectionChanged?.Invoke(_sortMode, value);
            Exit();
        }

        private class SortModeItem : ICustomListItem
        {
            public readonly ESortMode SortMode;

            public SortModeItem(ESortMode sortMode)
            {
                SortMode = sortMode;
            }

            public string ListName => SortMode.ToString();
            public string ListAuthor { get; }
            public Sprite ListCover { get; }
            public bool IsFavorite { get; }
        }
    }
}