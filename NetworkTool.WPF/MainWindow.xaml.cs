﻿using Microsoft.Extensions.DependencyInjection;
using NetworkTool.WPF.ViewModels;
using System.Windows;

namespace NetworkTool.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainViewModel();
        InitializeComponent();
    }
}