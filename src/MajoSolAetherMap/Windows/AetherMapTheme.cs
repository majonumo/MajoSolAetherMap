using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace MajoSolAetherMap.Windows;

public static class AetherMapTheme
{
    public static readonly Vector4 Background = Hex(0x15, 0x19, 0x1E);
    public static readonly Vector4 Card = Hex(0x22, 0x2A, 0x30);
    public static readonly Vector4 Turquoise = Hex(0x51, 0xD8, 0xBE);
    public static readonly Vector4 Gold = Hex(0xEA, 0xC3, 0x5B);
    public static readonly Vector4 Text = Hex(0xEE, 0xEE, 0xEE);
    public static readonly Vector4 TextSecondary = Hex(0x9C, 0xA3, 0xAF);
    public static readonly Vector4 Completed = Hex(0x52, 0xCC, 0xA1);
    public static readonly Vector4 Pending = Hex(0x77, 0x81, 0x8C);
    public static readonly Vector4 Border = Hex(0x34, 0x3D, 0x45);
    public static readonly Vector4 Blue = Hex(0x55, 0x8F, 0xD8);

    public static float Scale => ImGuiHelpers.GlobalScale;

    public static IDisposable PushWindowStyle()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Background);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, Card);
        ImGui.PushStyleColor(ImGuiCol.Text, Text);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, TextSecondary);
        ImGui.PushStyleColor(ImGuiCol.Border, Border);
        ImGui.PushStyleColor(ImGuiCol.Separator, Border);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(Card.X, Card.Y, Card.Z, 0.9f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(Turquoise.X, Turquoise.Y, Turquoise.Z, 0.18f));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(Turquoise.X, Turquoise.Y, Turquoise.Z, 0.16f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(Turquoise.X, Turquoise.Y, Turquoise.Z, 0.30f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(Turquoise.X, Turquoise.Y, Turquoise.Z, 0.42f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark, Turquoise);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, Turquoise);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, Completed);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10f * Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 9f * Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f * Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 8f * Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12f, 11f) * Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(7f, 4f) * Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8f, 6f) * Scale);
        return new PopScope(14, 7);
    }

    public static IDisposable BeginCard(
        string id,
        float height,
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoScrollbar,
        float width = 0f)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Card);
        ImGui.PushStyleColor(ImGuiCol.Border, Border);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 9f * Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(11f, 9f) * Scale);
        ImGui.BeginChild(id, new Vector2(width <= 0f ? 0f : width, height * Scale), true, flags);
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(2);
        return new ChildScope();
    }

    public static void Icon(FontAwesomeIcon icon, Vector4 color)
    {
        using var pushedFont = Plugin.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        ImGui.TextColored(color, icon.ToIconString());
    }

    public static void IconInline(FontAwesomeIcon icon, Vector4 color)
    {
        Icon(icon, color);
        ImGui.SameLine(0, 7f * Scale);
    }

    public static bool IconButton(string id, FontAwesomeIcon icon, string tooltip)
    {
        ImGui.PushID(id);
        var clicked = Dalamud.Interface.Components.ImGuiComponents.IconButton(
            icon,
            Turquoise,
            new Vector4(Turquoise.X, Turquoise.Y, Turquoise.Z, 0.20f),
            Text,
            new Vector2(28f, 26f) * Scale);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
        ImGui.PopID();
        return clicked;
    }

    public static void CompactProgress(float progress, float maxWidth = 180f)
    {
        progress = Math.Clamp(progress, 0f, 1f);
        var width = MathF.Min(ImGui.GetContentRegionAvail().X, maxWidth * Scale);
        var height = 5f * Scale;
        var position = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(position, position + new Vector2(width, height), ToU32(Background), height * 0.5f);
        if (progress > 0f)
        {
            var color = progress >= 1f ? Completed : Gold;
            drawList.AddRectFilled(position, position + new Vector2(width * progress, height), ToU32(color), height * 0.5f);
        }
        ImGui.Dummy(new Vector2(width, height));
    }

    public static void CircularProgress(Vector2 center, float radius, float progress)
    {
        progress = Math.Clamp(progress, 0f, 1f);
        var drawList = ImGui.GetWindowDrawList();
        var thickness = 4f * Scale;
        drawList.AddCircle(center, radius, ToU32(Background), 48, thickness);
        if (progress > 0f)
        {
            var endAngle = -MathF.PI * 0.5f + MathF.Tau * progress;
            drawList.PathArcTo(center, radius, -MathF.PI * 0.5f, endAngle, 48);
            drawList.PathStroke(ToU32(progress >= 1f ? Completed : Turquoise), ImDrawFlags.None, thickness);
        }
    }

    public static void TruncatedText(string text, float availableWidth, Vector4? color = null)
    {
        var display = text;
        var truncated = false;
        if (ImGui.CalcTextSize(display).X > availableWidth)
        {
            truncated = true;
            while (display.Length > 1 && ImGui.CalcTextSize(display + "…").X > availableWidth)
                display = display[..^1];
            display += "…";
        }

        if (color is { } textColor)
            ImGui.TextColored(textColor, display);
        else
            ImGui.TextUnformatted(display);

        if (truncated && ImGui.IsItemHovered())
            ImGui.SetTooltip(text);
    }

    public static uint ToU32(Vector4 color) => ImGui.ColorConvertFloat4ToU32(color);

    private static Vector4 Hex(byte red, byte green, byte blue, byte alpha = 255) =>
        new(red / 255f, green / 255f, blue / 255f, alpha / 255f);

    private sealed class PopScope(int colorCount, int styleCount) : IDisposable
    {
        public void Dispose()
        {
            ImGui.PopStyleVar(styleCount);
            ImGui.PopStyleColor(colorCount);
        }
    }

    private sealed class ChildScope : IDisposable
    {
        public void Dispose() => ImGui.EndChild();
    }
}
