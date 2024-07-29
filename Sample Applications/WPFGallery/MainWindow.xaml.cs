﻿using Microsoft.Win32;
using System.Windows.Shell;
using System.Windows.Navigation;
using WPFGallery.Navigation;
using WPFGallery.ViewModels;
using WPFGallery.Models;
using WPFGallery.Views;

namespace WPFGallery;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();

        Toggle_TitleButtonVisibility();

        _navigationService = navigationService;
        _navigationService.Navigating += OnNavigating;
        _navigationService.SetFrame(this.RootContentFrame);
        _navigationService.Navigate(ControlsInfoDataSource.Instance.GetControlInfo("Home"));

        WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = 50,
                CornerRadius = default,
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                UseAeroCaptionButtons = true
            }
        );

        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        this.StateChanged += MainWindow_StateChanged;
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (SystemParameters.HighContrast)
            {
                MinimizeButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Visible;
                CloseButton.Visibility = Visibility.Visible;
            }
            else
            {
                MinimizeButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Collapsed;
            }
        });
    }

    private void MainWindow_StateChanged(object sender, EventArgs e)
    {
        MainGrid.Margin = this.WindowState == WindowState.Maximized ? new Thickness(8) : default;
    }

    private readonly INavigationService _navigationService;

    public MainWindowViewModel ViewModel { get; }

    private void ControlsList_SelectedItemChanged()
    {
        if (ControlsList.SelectedItem is ControlInfoDataItem navItem)
        {
            _navigationService.Navigate(navItem);
            var tvi = ControlsList.ItemContainerGenerator.ContainerFromItem(navItem) as TreeViewItem;
            if(tvi != null)
            {
                tvi.IsExpanded = true;
                tvi.BringIntoView();
            }
        }
    }

    private void Toggle_TitleButtonVisibility()
    {
        var appContextBackdropData = AppContext.GetData("Switch.System.Windows.Appearance.DisableFluentThemeWindowBackdrop");
        bool disableFluentThemeWindowBackdrop = false;

        if (appContextBackdropData != null)
        {
            disableFluentThemeWindowBackdrop = bool.Parse(Convert.ToString(appContextBackdropData));
        }


        if (!disableFluentThemeWindowBackdrop)
        {
            foreach (ResourceDictionary mergedDictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (mergedDictionary.Source != null && mergedDictionary.Source.ToString().EndsWith("Fluent.xaml"))
                {
                    if (SystemParameters.HighContrast == true)
                    {
                        MinimizeButton.Visibility = Visibility.Visible;
                        MaximizeButton.Visibility = Visibility.Visible;
                        CloseButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MinimizeButton.Visibility = Visibility.Collapsed;
                        MaximizeButton.Visibility = Visibility.Collapsed;
                        CloseButton.Visibility = Visibility.Collapsed;
                    }

                    break;
                }
            }
        }
    }

    private void MinimizeWindow(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow(object sender, RoutedEventArgs e)
    {
        if(this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
            MaximizeIcon.Text = "\uE922";
        }
        else
        {
            this.WindowState = WindowState.Maximized;
            MaximizeIcon.Text = "\uE923";
        }
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void OnNavigating(object? sender, NavigatingEventArgs e)
    {
        List<ControlInfoDataItem> list = ViewModel.GetNavigationItemHierarchyFromPageType(e?.DataItem);
        
        if (list.Count > 0)
        {
            TreeViewItem selectedTreeViewItem = null;
            ItemsControl itemsControl = ControlsList;
            foreach(ControlInfoDataItem item in list)
            {
                var tvi = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if(tvi != null)
                {
                    tvi.IsExpanded = true;
                    tvi.UpdateLayout();
                    itemsControl = tvi;
                    selectedTreeViewItem = tvi;
                }
            }

            if(selectedTreeViewItem != null)
            {
                selectedTreeViewItem.IsSelected = true;
                ControlsList_SelectedItemChanged();
            }
        }
    }

    private void RootContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        ViewModel.UpdateCanNavigateBack();
    }

    private void ControlsList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) 
        {
            ControlsList_SelectedItemChanged();
        }
    }

    private void ControlsList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ControlsList_SelectedItemChanged();
    }
}