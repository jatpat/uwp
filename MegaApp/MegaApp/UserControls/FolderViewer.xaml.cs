﻿using System;
using Windows.UI.Xaml;
using MegaApp.ViewModels;
using MegaApp.ViewModels.UserControls;
using Microsoft.Xaml.Interactivity;
using System.Linq;
using Windows.UI.Xaml.Controls;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.Enums;

namespace MegaApp.UserControls
{
    // Helper class to define the viewmodel of this view
    // XAML cannot use generics in it's declaration.
    public class BaseFolderViewer : UserControlEx<FolderViewerViewModel> { }

    public sealed partial class FolderViewer : BaseFolderViewer
    {
        public FolderViewer()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Folder.
        /// </summary>
        public FolderViewModel Folder
        {
            get { return (FolderViewModel)GetValue(FolderProperty); }
            set { SetValue(FolderProperty, value); }
        }

        /// <summary>
        /// Identifier for the<see cref="Folder" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.Register(
                nameof(Folder),
                typeof(FolderViewModel),
                typeof(FolderViewer),
                new PropertyMetadata(null, FolderChangedCallback));

        private static void FolderChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as FolderViewer;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                control.OnFolderChanged((FolderViewModel)dpc.NewValue);
            }
        }

        private void OnFolderChanged(FolderViewModel folder)
        {
            if(this.ViewModel?.Folder != null)
            {
                this.ViewModel.Folder.ItemCollection.MultiSelectEnabled -= OnMultiSelectEnabled;
                this.ViewModel.Folder.ItemCollection.MultiSelectDisabled -= OnMultiSelectDisabled;
                this.ViewModel.Folder.ItemCollection.AllSelected -= OnAllSelected;
            }

            this.ViewModel.Folder = folder;

            if (this.ViewModel?.Folder != null)
            {
                this.ViewModel.Folder.ItemCollection.MultiSelectEnabled += OnMultiSelectEnabled;
                this.ViewModel.Folder.ItemCollection.MultiSelectDisabled += OnMultiSelectDisabled;
                this.ViewModel.Folder.ItemCollection.AllSelected += OnAllSelected;
            }
        }

        private void OnMultiSelectEnabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // First save the current selected items to restore them after enable the multi select
            var selectedItems = this.ViewModel.Folder.ItemCollection.SelectedItems.ToList();

            this.ListView.SelectionMode = ListViewSelectionMode.Multiple;
            this.GridView.SelectionMode = ListViewSelectionMode.Multiple;

            // Update the selected items
            foreach (var item in selectedItems)
            {
                this.ListView.SelectedItems.Add(item);
                this.GridView.SelectedItems.Add(item);
            }

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        private void OnMultiSelectDisabled(object sender, EventArgs e)
        {
            // Needed to avoid extrange behaviors during the view update
            DisableViewsBehaviors();

            // If there is only one selected item save it to restore it after disable the multi select mode
            IMegaNode selectedItem = null;
            if (this.ViewModel.Folder.ItemCollection.OnlyOneSelectedItem)
                selectedItem = this.ViewModel.Folder.ItemCollection.SelectedItems.First();

            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
            {
                this.ListView.SelectionMode = ListViewSelectionMode.Extended;
                this.GridView.SelectionMode = ListViewSelectionMode.Extended;
            }
            else
            {
                this.ListView.SelectionMode = ListViewSelectionMode.Single;
                this.GridView.SelectionMode = ListViewSelectionMode.Single;
            }

            // Restore the selected item
            this.ListView.SelectedItem = this.GridView.SelectedItem = 
                this.ViewModel.Folder.ItemCollection.FocusedItem = selectedItem;

            // Restore the view behaviors again
            EnableViewsBehaviors();
        }

        /// <summary>
        /// Enable the behaviors of the active view
        /// </summary>
        private void EnableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListView).Attach(this.ListView);
            Interaction.GetBehaviors(this.GridView).Attach(this.GridView);
        }

        /// <summary>
        /// Disable the behaviors of the current active view
        /// </summary>
        private void DisableViewsBehaviors()
        {
            Interaction.GetBehaviors(this.ListView).Detach();
            Interaction.GetBehaviors(this.GridView).Detach();
        }

        private void OnAllSelected(object sender, bool value)
        {
            if (value)
            {
                this.ListView?.SelectAll();
                this.GridView?.SelectAll();
            }
            else
            {
                this.ListView?.SelectedItems.Clear();
                this.GridView?.SelectedItems.Clear();
            }
        }
    }
}
