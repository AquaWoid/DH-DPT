using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DHM.ViewModels;

namespace DHM.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void OpenFileDialog(object sender, RoutedEventArgs args)
    {
        var toplevel = TopLevel.GetTopLevel(this);

        var files = await toplevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open JSON",
            AllowMultiple = false

        });


        if (files.Count >= 1) {
            
            ((MainViewModel)DataContext!).receiveFileDialog(files[0].Path.LocalPath);
        
        }

    }

}
