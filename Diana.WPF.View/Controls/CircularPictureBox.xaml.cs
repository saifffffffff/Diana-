using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Diana.WPF.View.Controls;

public partial class CircularPictureBox : UserControl
{
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(BitmapImage),
            typeof(CircularPictureBox),
            new PropertyMetadata(null, OnImageSourceChanged));

    public static readonly DependencyProperty DefaultImageProperty =
        DependencyProperty.Register(
            nameof(DefaultImage),
            typeof(BitmapImage),
            typeof(CircularPictureBox),
            new PropertyMetadata(null, OnDefaultImageChanged));

    public BitmapImage ImageSource
    {
        get => (BitmapImage)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public BitmapImage DefaultImage
    {
        get => (BitmapImage)GetValue(DefaultImageProperty);
        set => SetValue(DefaultImageProperty, value);
    }

    public CircularPictureBox()
    {
        InitializeComponent();
        InitializeDefaultImage();
    }

    private void InitializeDefaultImage()
    {
        // Load default profile picture from Assets
        try
        {
            var defaultImageUri = new Uri("pack://application:,,,/Assets/pfp_.jpg", UriKind.Absolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = defaultImageUri;
            bitmap.EndInit();
            bitmap.Freeze();
            DefaultImage = bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load default image: {ex.Message}");
        }
    }

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (CircularPictureBox)d;
        if (e.NewValue != null)
        {
            control.PictureImageBrush.ImageSource = (BitmapImage)e.NewValue;
            control.PlusIcon.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Show default image if no custom image is set
            control.PictureImageBrush.ImageSource = control.DefaultImage;
            control.PlusIcon.Visibility = control.DefaultImage == null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnDefaultImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (CircularPictureBox)d;
        // Only update if no custom image is set
        if (control.ImageSource == null)
        {
            if (e.NewValue != null)
            {
                control.PictureImageBrush.ImageSource = (BitmapImage)e.NewValue;
                control.PlusIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.PictureImageBrush.ImageSource = null;
                control.PlusIcon.Visibility = Visibility.Visible;
            }
        }
    }

    private void PictureEllipse_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenImageDialog();
    }

    private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenImageDialog();
    }

    private void OpenImageDialog()
    {
        // Ensure file dialog runs on the UI thread
        Dispatcher.Invoke(() =>
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select an Image",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(dlg.FileName, UriKind.Absolute);
                    bmp.EndInit();
                    bmp.Freeze();

                    ImageSource = bmp;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        });
    }
}
