namespace MajoSolAetherMap;

public static class Localization
{
    public static string Text(InterfaceLanguage language, string spanish, string english) =>
        language == InterfaceLanguage.English ? english : spanish;
}
