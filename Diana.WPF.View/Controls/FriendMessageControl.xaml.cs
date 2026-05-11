using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Diana.WPF.View.Controls
{
    /// <summary>
    /// A WPF UserControl that renders a friend-message bubble.
    /// Target framework: net10.0-windows
    /// </summary>
    public partial class FriendMessageControl : UserControl
    {
        // ──────────────────────────────────────────────────────────────────
        /// <summary>
        /// Creates a friend-message bubble.
        /// </summary>
        /// <param name="base64Image">
        ///   Base-64 encoded PNG/JPG/BMP avatar.
        ///   Pass <c>null</c> or empty to show coloured initials instead.
        /// </param>
        /// <param name="username">Display name shown in bold above the message.</param>
        /// <param name="messageContent">The message body text.</param>
        public FriendMessageControl(string? base64Image, string username, string messageContent)
        {
            InitializeComponent();
            HorizontalAlignment = HorizontalAlignment.Left;  // ADD THIS
            MaxWidth = 420;
            SetUsername(username);
            SetMessage(messageContent);
            SetTime(DateTime.Now.ToString("H:mm"));
            SetAvatar(base64Image);
        }

        // ──────────────────────────────────────────────────────────────────
        // Public update API
        // ──────────────────────────────────────────────────────────────────

        /// <summary>Replaces the avatar with a new base-64 image (or reverts to initials).</summary>
        public void SetAvatar(string? base64Image)
        {
            if (!string.IsNullOrWhiteSpace(base64Image))
            {
                try
                {
                    byte[]       bytes  = Convert.FromBase64String(base64Image);
                    using var    ms     = new MemoryStream(bytes);
                    var          bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption  = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();                     // safe to use across threads

                    AvatarBrush.ImageSource = bitmap;
                    AvatarImage.Visibility  = Visibility.Visible;
                    AvatarFallback.Visibility = Visibility.Collapsed;
                    InitialsText.Visibility   = Visibility.Collapsed;
                    return;
                }
                catch { /* fall through to initials */ }
            }

            // Show initials fallback
            AvatarImage.Visibility    = Visibility.Collapsed;
            AvatarFallback.Visibility = Visibility.Visible;
            InitialsText.Visibility   = Visibility.Visible;
            InitialsText.Text         = GetInitials(UsernameText.Text);
        }

        /// <summary>Updates the displayed username.</summary>
        public void SetUsername(string username)
        {
            UsernameText.Text = username ?? string.Empty;
            // refresh initials if we're in fallback mode
            if (AvatarImage.Visibility != Visibility.Visible)
                InitialsText.Text = GetInitials(UsernameText.Text);
        }

        /// <summary>Updates the message body.</summary>
        public void SetMessage(string message) =>
            MessageText.Text = message ?? string.Empty;

        /// <summary>Updates the timestamp label (e.g. "17:50").</summary>
        public void SetTime(string time) =>
            TimeText.Text = time ?? string.Empty;

        // ──────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}"
                : char.ToUpper(parts[0][0]).ToString();
        }
    }
}
