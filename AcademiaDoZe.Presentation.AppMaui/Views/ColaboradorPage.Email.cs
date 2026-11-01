using System.Text.RegularExpressions;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorPage : ContentPage
{
    private void OnEmailUnfocused(object? sender, FocusEventArgs e)
    {
        var email = EmailEntry?.Text?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            EmailErrorLabel.IsVisible = false;
            return;
        }

        var re = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        if (!re.IsMatch(email))
        {
            EmailErrorLabel.IsVisible = true;
            EmailErrorLabel.Text = "Formato de e-mail inválido. Use nome@dominio.com";
            EmailEntry?.Focus();
        }
        else
        {
            EmailErrorLabel.IsVisible = false;
        }
    }
}
