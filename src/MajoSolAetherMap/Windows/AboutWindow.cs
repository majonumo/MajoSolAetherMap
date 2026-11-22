using System;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace MajoSolAetherMap.Windows;

public sealed class AboutWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly ISharedImmediateTexture? authorsImage;
    private IDisposable? themeScope;

    public AboutWindow(Configuration configuration, ITextureProvider textureProvider)
        : base("About · Majo & Sol Aether Map##MajoSolAetherMapAbout")
    {
        this.configuration = configuration;
        Size = new Vector2(420, 520);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(340, 420),
            MaximumSize = new Vector2(620, 800),
        };

        var assemblyDirectory = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
        var imagePath = assemblyDirectory is null
            ? null
            : Path.Combine(assemblyDirectory, "assets", "majo-and-sol.png");
        if (imagePath is not null && File.Exists(imagePath))
            authorsImage = textureProvider.GetFromFile(imagePath);
    }

    public void Dispose()
    {
    }

    public override void PreDraw()
    {
        WindowName = $"{T(UiText.About)} · Majo & Sol Aether Map##MajoSolAetherMapAbout";
        themeScope = AetherMapTheme.PushWindowStyle(configuration.WindowBackgroundOpacity);
    }

    public override void PostDraw()
    {
        themeScope?.Dispose();
        themeScope = null;
    }

    public override void Draw()
    {
        DrawAuthorsImage();
        ImGui.Spacing();
        AetherMapTheme.IconInline(FontAwesomeIcon.Compass, AetherMapTheme.Turquoise);
        ImGui.TextColored(AetherMapTheme.Text, "MAJO & SOL AETHER MAP");
        ImGui.TextColored(AetherMapTheme.Gold, "Same party, always.");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextColored(AetherMapTheme.Text, T(UiText.DevelopedBy));
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            T(UiText.CreatedOn));
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            $"{T(UiText.VersionLabel)} {typeof(Plugin).Assembly.GetName().Version?.ToString(3) ?? "0.7.0"}");
        ImGui.Spacing();
        ImGui.TextWrapped(T(UiText.PluginDescription));
        ImGui.Spacing();
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            T(UiText.License));
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            "Final Fantasy XIV © SQUARE ENIX CO., LTD.");
    }

    private void DrawAuthorsImage()
    {
        if (authorsImage is not null)
        {
            var wrap = authorsImage.GetWrapOrEmpty();
            if (wrap.Width > 1 && wrap.Height > 1)
            {
                var maxWidth = MathF.Min(ImGui.GetContentRegionAvail().X, 360f * AetherMapTheme.Scale);
                var height = maxWidth * wrap.Height / wrap.Width;
                var x = ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X - maxWidth) * 0.5f;
                ImGui.SetCursorPosX(x);
                ImGui.Image(wrap.Handle, new Vector2(maxWidth, height));
                return;
            }
        }

        using (AetherMapTheme.BeginCard("AuthorsFallback", 150,
                   backgroundOpacity: configuration.CardBackgroundOpacity))
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 38f * AetherMapTheme.Scale);
            AetherMapTheme.IconInline(FontAwesomeIcon.Compass, AetherMapTheme.Turquoise);
            ImGui.TextColored(AetherMapTheme.Text, "MAJO  &  SOL");
        }
    }

    private string T(UiText key) => Localization.Text(configuration, key);
}
