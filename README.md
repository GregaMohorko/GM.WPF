# GM.WPF
.NET library with various controls, utilities and base classes for MVVM driven WPF development.

[![Latest release](https://img.shields.io/github/release/GregaMohorko/GM.WPF.svg?style=flat-square)](https://github.com/GregaMohorko/GM.WPF/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GM.WPF.svg?style=flat-square)](https://www.nuget.org/packages/GM.WPF)

## Documentation
You can read the documentation and how-to's under the [Wiki](https://github.com/GregaMohorko/GM.WPF/wiki).

## Similar projects
- [GM.Utility](https://github.com/GregaMohorko/GM.Utility)
- [GM.Tools](https://github.com/GregaMohorko/GM.Tools)
- [GM.Windows.Utility](https://github.com/GregaMohorko/GM.Windows.Utility)
- [GM.Windows.Tools](https://github.com/GregaMohorko/GM.Windows.Tools)

## List of features
*(Check the class documentation comments in the source files for more information!)*

**Controls**:
 - [GMDataGrid](src/GM.WPF/GM.WPF/Controls/GMDataGrid.cs)
 - [GMStackPanel](src/GM.WPF/GM.WPF/Controls/GMStackPanel.cs)
 - [GMWrapPanel](src/GM.WPF/GM.WPF/Controls/GMWrapPanel.cs)
 - [ProgressOverlay](src/GM.WPF/GM.WPF/Controls/ProgressOverlay.xaml.cs)
 - [StartPage](src/GM.WPF/GM.WPF/Controls/StartPage.xaml.cs)
 - [TimeControl](src/GM.WPF/GM.WPF/Controls/TimeControl.xaml.cs)
 - [TimePicker](src/GM.WPF/GM.WPF/Controls/TimePicker.xaml.cs)
 - [WatermarkTextBox](src/GM.WPF/GM.WPF/Controls/WatermarkTextBox.xaml.cs)

**Dialogs**:
 - [DialogPanel](src/GM.WPF/GM.WPF/Controls/Dialogs/DialogPanel.xaml.cs)
 - [ChooseDialog](src/GM.WPF/GM.WPF/Controls/Dialogs/ChooseDialog.xaml.cs)
 - [InputDialog](src/GM.WPF/GM.WPF/Controls/Dialogs/InputDialog.xaml.cs)
 - [MessageDialog](src/GM.WPF/GM.WPF/Controls/Dialogs/MessageDialog.xaml.cs)
 - [ProgressDialog](src/GM.WPF/GM.WPF/Controls/Dialogs/ProgressDialog.xaml.cs)
 - [SearchDialog](src/GM.WPF/GM.WPF/Controls/Dialogs/SearchDialog.xaml.cs)
 - [SelectDialog](src/GM.WPF/GM.WPF/Controls/Dialogs/SelectDialog.xaml.cs)

**Windows**:
 - [ClosingWindow](src/GM.WPF/GM.WPF/Windows/ClosingWindow.cs)
 - [SettingsWindow](src/GM.WPF/GM.WPF/Windows/SettingsWindow.xaml.cs)
 - [SplashWindow](src/GM.WPF/GM.WPF/Windows/SplashWindow.cs)

**Patterns**:
 - [Undo/Redo](src/GM.WPF/GM.WPF/Patterns/UndoRedo/GMWPFUndoRedo.cs)

**Tools**:
 - [AsyncRequestLoader](src/GM.WPF/GM.WPF/AsyncRequestLoader.cs)
 - [ProgressUpdater](src/GM.WPF/GM.WPF/ProgressUpdater.cs)

**Behaviors**:
 - [DataGrid](src/GM.WPF/GM.WPF/Behaviors/DataGridBehavior.cs)
 - [FrameworkElement](src/GM.WPF/GM.WPF/Behaviors/FrameworkElementBehavior.cs)
 - [Panel](src/GM.WPF/GM.WPF/Behaviors/PanelBehavior.cs)
 - [TabItem](src/GM.WPF/GM.WPF/Behaviors/TabItemBehavior.cs)
 - [TabControl](src/GM.WPF/GM.WPF/Behaviors/TabControlBehavior.cs)
 - [TextBlock](src/GM.WPF/GM.WPF/Behaviors/TextBlockBehavior.cs)

**Converters**:
 - [BoolToBool](src/GM.WPF/GM.WPF/Converters/BoolToBoolConverter.cs)
 - [BoolToScrollBarVisibility](src/GM.WPF/GM.WPF/Converters/BoolToScrollBarVisibilityConverter.cs)
 - [BoolToVisibility](src/GM.WPF/GM.WPF/Converters/BoolToVisibilityConverter.cs)
 - [EnumToCollectionConverter](src/GM.WPF/GM.WPF/Converters/EnumToCollectionConverter.cs)
 - [FunctionToString](src/GM.WPF/GM.WPF/Converters/FunctionToStringConverter.cs)
 - [ICollectionToBool](src/GM.WPF/GM.WPF/Converters/ICollectionToBoolConverter.cs)
 - [ICollectionToCountConverter](src/GM.WPF/GM.WPF/Converters/ICollectionToCountConverter.cs)
 - [ICollectionToVisibility](src/GM.WPF/GM.WPF/Converters/ICollectionToVisibilityConverter.cs)
 - [IListToIList](src/GM.WPF/GM.WPF/Converters/IListToIListConverter.cs)
 - [IntToVisibility](src/GM.WPF/GM.WPF/Converters/IntToVisibilityConverter.cs)
 - [ObjectToBool](src/GM.WPF/GM.WPF/Converters/ObjectToBoolConverter.cs)
 - [ObjectToVisibility](src/GM.WPF/GM.WPF/Converters/ObjectToVisibilityConverter.cs)
 - [StringToBool](src/GM.WPF/GM.WPF/Converters/StringToBoolConverter.cs)
 - [StringToString](src/GM.WPF/GM.WPF/Converters/StringToStringConverter.cs)
 - [StringToVisibility](src/GM.WPF/GM.WPF/Converters/StringToVisibilityConverter.cs)

**Utilities (static classes)**:
- [Binding](src/GM.WPF/GM.WPF/Utility/BindingUtility.cs)
- [Brush](src/GM.WPF/GM.WPF/Utility/BrushUtility.cs)
- [DependencyObject](src/GM.WPF/GM.WPF/Utility/DependencyObjectUtility.cs)
- [DependencyProperty](src/GM.WPF/GM.WPF/Utility/DependencyPropertyUtility.cs)
- [FrameworkElement](src/GM.WPF/GM.WPF/Utility/FrameworkElementUtility.cs)
- [Size](src/GM.WPF/GM.WPF/Utility/SizeUtility.cs)
- [TextBlock](src/GM.WPF/GM.WPF/Utility/TextBlockUtility.cs)
- [TreeView](src/GM.WPF/GM.WPF/Utility/TreeViewUtility.cs)
- [Visual](src/GM.WPF/GM.WPF/Utility/VisualUtility.cs)

## Requirements
.NET Framework 4.7.1

## Author and License
Gregor Mohorko ([www.mohorko.info](https://www.mohorko.info))

Copyright (c) 2021 Gregor Mohorko

[MIT License](./LICENSE.md)
