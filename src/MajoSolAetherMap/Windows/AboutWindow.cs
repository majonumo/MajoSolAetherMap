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

    public override void PreDraw() => themeScope = AetherMapTheme.PushWindowStyle();

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
        ImGui.TextColored(AetherMapTheme.Text, "Developed by Majo & Sol");
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            T("Creado el 18 de noviembre de 2026", "Created on November 18, 2026"));
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            $"Version {typeof(Plugin).Assembly.GetName().Version?.ToString(3) ?? "0.7.0"}");
        ImGui.Spacing();
        ImGui.TextWrapped(T(
            "Una guía compacta para encontrar Aether Currents, seguir misiones y desbloquear el vuelo.",
            "A compact guide for finding Aether Currents, tracking quests and unlocking flight."));
        ImGui.Spacing();
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            T("Código original bajo licencia MIT.", "Original code licensed under the MIT License."));
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

        using (AetherMapTheme.BeginCard("AuthorsFallback", 150))
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 38f * AetherMapTheme.Scale);
            AetherMapTheme.IconInline(FontAwesomeIcon.Compass, AetherMapTheme.Turquoise);
            ImGui.TextColored(AetherMapTheme.Text, "MAJO  &  SOL");
        }
    }

    private string T(string spanish, string english) =>
        Localization.Text(configuration.Language, spanish, english);
}
