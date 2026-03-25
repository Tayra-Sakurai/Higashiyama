using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Higashiyama
{
    /// <summary>
    /// A license agreement comfirming dialog.
    /// </summary>
    public sealed partial class LicenseAgreementDialog : ContentDialog
    {
        public LicenseAgreementDialog()
        {
            InitializeComponent();

            Opened += LicenseAgreementDialog_Opened;
            AgreeBox.Checked += AgreeBox_Checked;
            AgreeBox.Unchecked += AgreeBox_Unchecked;
        }

        /// <summary>
        /// Disables the primary button.
        /// </summary>
        /// <param name="sender">The check box.</param>
        /// <param name="e">The event argument.</param>
        private void AgreeBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = false;
        }

        /// <summary>
        /// Enables the primary button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AgreeBox_Checked(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = true;
        }

        /// <summary>
        /// Sets up the primary access dialog.
        /// </summary>
        /// <param name="sender">The <see cref="ContentDialog"/> instance which sends the event.</param>
        /// <param name="args">The event arguments.</param>
        private async void LicenseAgreementDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            // The license file.
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new("ms-appx:///Assets/LICENSES/LICENSE.rtf"));

            // The license file stream.
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);

            // Loads the license file.

            SuperEditBox.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.FormatRtf, stream);

            // Unchecks the "agree" checkbox to make user check it.

            AgreeBox.IsChecked = false;
        }
    }
}
