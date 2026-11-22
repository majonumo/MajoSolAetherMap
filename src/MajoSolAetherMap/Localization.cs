using System;
using System.Collections.Generic;
using Dalamud.Game;

namespace MajoSolAetherMap;

public enum UiText
{
    About, Advanced, AllObjectivesComplete, AmbiguousLocation, Appearance, Automatic,
    CardBackgroundOpacity, Close, Completed, CompletedObtained, CreatedOn, CurrentZone,
    CurrentsCompleted, DebugMode, EnableOverlay, Exploration, FlightUnlocked,
    InterfaceDescription, Language, License, LocateNpc, LocationUnavailable, MainScenario,
    MapOverlay, MarkerOpacity, MarkerSize, NpcUnavailable, NoCurrents, NoObjectives,
    Objectives, OpenSettings, Pending, PendingObjectives, PlaceFlag, PluginDescription,
    Quests, RemoveMarker, ResetAppearance, Settings, ShowObtainedCurrents,
    ShowPendingCurrents, ShowQuestNpcs, SideQuest, TechnicalFooter, Unavailable,
    VisibleMap, VisibleMapUnresolved, WaitingForCharacter, WaitingForCharacterHelp,
    WindowBackgroundOpacity, DevelopedBy, VersionLabel,
}

public static class Localization
{
    public static readonly IReadOnlyList<InterfaceLanguage> SupportedLanguages =
    [
        InterfaceLanguage.English,
        InterfaceLanguage.Spanish,
        InterfaceLanguage.BrazilianPortuguese,
        InterfaceLanguage.French,
        InterfaceLanguage.German,
        InterfaceLanguage.Japanese,
    ];

    private static readonly IReadOnlyDictionary<UiText, string> English = new Dictionary<UiText, string>
    {
        [UiText.About] = "About",
        [UiText.Advanced] = "ADVANCED",
        [UiText.AllObjectivesComplete] = "All objectives are complete",
        [UiText.AmbiguousLocation] = "Ambiguous location",
        [UiText.Appearance] = "APPEARANCE",
        [UiText.Automatic] = "Automatic (FFXIV client)",
        [UiText.CardBackgroundOpacity] = "Card background opacity",
        [UiText.Close] = "Close",
        [UiText.Completed] = "COMPLETED",
        [UiText.CompletedObtained] = "Completed / obtained",
        [UiText.CreatedOn] = "Created on November 18, 2026",
        [UiText.CurrentZone] = "Current zone",
        [UiText.CurrentsCompleted] = "Currents completed",
        [UiText.DebugMode] = "Debug mode",
        [UiText.EnableOverlay] = "Enable overlay",
        [UiText.Exploration] = "EXPLORATION",
        [UiText.FlightUnlocked] = "Flight unlocked",
        [UiText.InterfaceDescription] = "Persistent map and interface preferences.",
        [UiText.Language] = "LANGUAGE",
        [UiText.License] = "Original code licensed under the MIT License.",
        [UiText.LocateNpc] = "Locate NPC",
        [UiText.LocationUnavailable] = "Location unavailable",
        [UiText.MainScenario] = "Main scenario",
        [UiText.MapOverlay] = "MAP OVERLAY",
        [UiText.MarkerOpacity] = "Opacity",
        [UiText.MarkerSize] = "Size",
        [UiText.NpcUnavailable] = "NPC unavailable",
        [UiText.NoCurrents] = "No currents",
        [UiText.NoObjectives] = "There are no objectives in this zone.",
        [UiText.Objectives] = "OBJECTIVES",
        [UiText.OpenSettings] = "Open settings",
        [UiText.Pending] = "PENDING",
        [UiText.PendingObjectives] = "Pending objectives",
        [UiText.PlaceFlag] = "Place flag",
        [UiText.PluginDescription] = "A compact guide for finding Aether Currents, tracking quests and unlocking flight.",
        [UiText.Quests] = "QUESTS",
        [UiText.RemoveMarker] = "Remove marker",
        [UiText.ResetAppearance] = "Reset appearance",
        [UiText.Settings] = "Settings",
        [UiText.ShowObtainedCurrents] = "Show obtained currents",
        [UiText.ShowPendingCurrents] = "Show pending currents",
        [UiText.ShowQuestNpcs] = "Show quest NPCs",
        [UiText.SideQuest] = "Side quest",
        [UiText.TechnicalFooter] = "Lumina · Dalamud API 15 · Non-invasive reading",
        [UiText.Unavailable] = "Unavailable",
        [UiText.VisibleMap] = "Visible map",
        [UiText.VisibleMapUnresolved] = "Visible map unresolved",
        [UiText.WaitingForCharacter] = "Waiting for character",
        [UiText.WaitingForCharacterHelp] = "Log in with a character to view Aether Current progress.",
        [UiText.WindowBackgroundOpacity] = "Window background opacity",
        [UiText.DevelopedBy] = "Developed by Majo & Sol",
        [UiText.VersionLabel] = "Version",
    };

    private static readonly IReadOnlyDictionary<InterfaceLanguage, IReadOnlyDictionary<UiText, string>> Translations =
        new Dictionary<InterfaceLanguage, IReadOnlyDictionary<UiText, string>>
        {
            [InterfaceLanguage.English] = English,
            [InterfaceLanguage.Spanish] = Create(
                "Acerca de", "AVANZADO", "Todos los objetivos están completados", "Ubicación ambigua",
                "APARIENCIA", "Automático (cliente de FFXIV)", "Opacidad del fondo de las tarjetas", "Cerrar",
                "COMPLETADOS", "Completada / obtenida", "Creado el 18 de noviembre de 2026", "Zona actual",
                "Corrientes completadas", "Modo Debug", "Activar overlay", "EXPLORACIÓN", "Vuelo desbloqueado",
                "Preferencias persistentes del mapa y la interfaz.", "IDIOMA", "Código original bajo licencia MIT.",
                "Localizar NPC", "Ubicación no disponible", "Historia principal", "OVERLAY DEL MAPA", "Opacidad",
                "Tamaño", "NPC no disponible", "Sin corrientes", "No hay objetivos para mostrar en esta zona.",
                "OBJETIVOS", "Abrir configuración", "PENDIENTES", "Objetivos pendientes", "Colocar bandera",
                "Una guía compacta para encontrar Aether Currents, seguir misiones y desbloquear el vuelo.",
                "MISIONES", "Eliminar marcador", "Restablecer apariencia", "Configuración",
                "Mostrar corrientes obtenidas", "Mostrar corrientes pendientes", "Mostrar NPC de misiones",
                "Misión secundaria", "Lumina · Dalamud API 15 · Lectura no invasiva", "No disponible",
                "Mapa visible", "Mapa visible sin resolver", "Esperando al personaje",
                "Ingresá con un personaje para consultar el progreso de Aether Currents.",
                "Opacidad del fondo de la ventana", "Desarrollado por Majo & Sol", "Versión"),
            [InterfaceLanguage.BrazilianPortuguese] = Create(
                "Sobre", "AVANÇADO", "Todos os objetivos foram concluídos", "Localização ambígua",
                "APARÊNCIA", "Automático (cliente do FFXIV)", "Opacidade do fundo dos cartões", "Fechar",
                "CONCLUÍDOS", "Concluída / obtida", "Criado em 18 de novembro de 2026", "Área atual",
                "Correntes concluídas", "Modo de depuração", "Ativar sobreposição", "EXPLORAÇÃO", "Voo desbloqueado",
                "Preferências persistentes do mapa e da interface.", "IDIOMA", "Código original sob a licença MIT.",
                "Localizar NPC", "Localização indisponível", "Cenário principal", "SOBREPOSIÇÃO DO MAPA", "Opacidade",
                "Tamanho", "NPC indisponível", "Sem correntes", "Não há objetivos nesta área.", "OBJETIVOS",
                "Abrir configurações", "PENDENTES", "Objetivos pendentes", "Colocar bandeira",
                "Um guia compacto para encontrar Aether Currents, acompanhar missões e desbloquear o voo.",
                "MISSÕES", "Remover marcador", "Redefinir aparência", "Configurações", "Mostrar correntes obtidas",
                "Mostrar correntes pendentes", "Mostrar NPCs de missões", "Missão secundária",
                "Lumina · Dalamud API 15 · Leitura não invasiva", "Indisponível", "Mapa visível",
                "Mapa visível não resolvido", "Aguardando personagem",
                "Entre com um personagem para ver o progresso das Aether Currents.", "Opacidade do fundo da janela",
                "Desenvolvido por Majo & Sol", "Versão"),
            [InterfaceLanguage.French] = Create(
                "À propos", "AVANCÉ", "Tous les objectifs sont terminés", "Emplacement ambigu", "APPARENCE",
                "Automatique (client FFXIV)", "Opacité du fond des cartes", "Fermer", "TERMINÉS",
                "Terminée / obtenue", "Créé le 18 novembre 2026", "Zone actuelle", "Courants terminés",
                "Mode débogage", "Activer la superposition", "EXPLORATION", "Vol débloqué",
                "Préférences persistantes de la carte et de l’interface.", "LANGUE",
                "Code original sous licence MIT.", "Localiser le PNJ", "Emplacement indisponible",
                "Épopée", "SUPERPOSITION DE CARTE", "Opacité", "Taille", "PNJ indisponible", "Aucun courant",
                "Aucun objectif à afficher dans cette zone.", "OBJECTIFS", "Ouvrir les paramètres", "EN ATTENTE",
                "Objectifs en attente", "Placer un drapeau",
                "Un guide compact pour trouver les Aether Currents, suivre les quêtes et débloquer le vol.",
                "QUÊTES", "Supprimer le marqueur", "Réinitialiser l’apparence", "Paramètres",
                "Afficher les courants obtenus", "Afficher les courants en attente", "Afficher les PNJ de quête",
                "Quête secondaire", "Lumina · Dalamud API 15 · Lecture non invasive", "Indisponible",
                "Carte visible", "Carte visible non résolue", "En attente du personnage",
                "Connectez-vous avec un personnage pour voir la progression des Aether Currents.",
                "Opacité du fond de la fenêtre", "Développé par Majo & Sol", "Version"),
            [InterfaceLanguage.German] = Create(
                "Über", "ERWEITERT", "Alle Ziele sind abgeschlossen", "Mehrdeutiger Ort", "ERSCHEINUNGSBILD",
                "Automatisch (FFXIV-Client)", "Deckkraft des Kartenhintergrunds", "Schließen", "ABGESCHLOSSEN",
                "Abgeschlossen / erhalten", "Erstellt am 18. November 2026", "Aktuelles Gebiet",
                "Windätherquellen abgeschlossen", "Debug-Modus", "Overlay aktivieren", "ERKUNDUNG",
                "Fliegen freigeschaltet", "Dauerhafte Karten- und Oberflächeneinstellungen.", "SPRACHE",
                "Originalcode unter MIT-Lizenz.", "NPC lokalisieren", "Ort nicht verfügbar", "Hauptszenario",
                "KARTEN-OVERLAY", "Deckkraft", "Größe", "NPC nicht verfügbar", "Keine Quellen",
                "In diesem Gebiet gibt es keine Ziele.", "ZIELE", "Einstellungen öffnen", "AUSSTEHEND",
                "Ausstehende Ziele", "Flagge setzen",
                "Ein kompakter Leitfaden zum Finden von Aether Currents, Verfolgen von Aufträgen und Freischalten des Fliegens.",
                "AUFTRÄGE", "Markierung entfernen", "Erscheinungsbild zurücksetzen", "Einstellungen",
                "Erhaltene Quellen anzeigen", "Ausstehende Quellen anzeigen", "Auftrags-NPCs anzeigen",
                "Nebenauftrag", "Lumina · Dalamud API 15 · Nichtinvasives Lesen", "Nicht verfügbar",
                "Sichtbare Karte", "Sichtbare Karte nicht erkannt", "Warte auf Charakter",
                "Melde dich mit einem Charakter an, um den Fortschritt der Aether Currents zu sehen.",
                "Deckkraft des Fensterhintergrunds", "Entwickelt von Majo & Sol", "Version"),
            [InterfaceLanguage.Japanese] = Create(
                "このプラグインについて", "詳細設定", "すべての目標を達成しました", "場所を特定できません", "外観",
                "自動（FFXIVクライアント）", "カード背景の不透明度", "閉じる", "完了済み", "完了／取得済み",
                "2026年11月18日作成", "現在のエリア", "風脈コンプリート", "デバッグモード", "オーバーレイを有効化",
                "探索", "フライングマウント開放済み", "マップとインターフェースの設定を保存します。", "言語",
                "オリジナルコードはMITライセンスです。", "NPCの場所を表示", "場所を取得できません", "メインクエスト",
                "マップオーバーレイ", "不透明度", "サイズ", "NPC情報なし", "風脈なし",
                "このエリアに表示する目標はありません。", "目標", "設定を開く", "未完了", "未完了の目標",
                "フラッグを立てる", "Aether Currentsを探し、クエストを追跡して、飛行を開放するためのコンパクトなガイドです。",
                "クエスト", "マーカーを削除", "外観をリセット", "設定", "取得済みの風脈を表示",
                "未取得の風脈を表示", "クエストNPCを表示", "サブクエスト",
                "Lumina · Dalamud API 15 · 読み取り専用", "利用不可", "表示中のマップ",
                "表示中のマップを特定できません", "キャラクターを待っています",
                "キャラクターでログインするとAether Currentの進行状況を確認できます。", "ウィンドウ背景の不透明度",
                "開発: Majo & Sol", "バージョン"),
        };

    public static InterfaceLanguage EffectiveLanguage(Configuration configuration) =>
        configuration.FollowClientLanguage
            ? FromClientLanguage(Plugin.ClientState.ClientLanguage)
            : IsSupported(configuration.Language) ? configuration.Language : InterfaceLanguage.English;

    public static string Text(Configuration configuration, UiText key) => Text(EffectiveLanguage(configuration), key);

    public static string Text(InterfaceLanguage language, UiText key)
    {
        if (Translations.TryGetValue(language, out var translation) && translation.TryGetValue(key, out var value))
            return value;
        return English[key];
    }

    public static string LanguageName(InterfaceLanguage language) => language switch
    {
        InterfaceLanguage.English => "English",
        InterfaceLanguage.Spanish => "Español",
        InterfaceLanguage.BrazilianPortuguese => "Português (Brasil)",
        InterfaceLanguage.French => "Français",
        InterfaceLanguage.German => "Deutsch",
        InterfaceLanguage.Japanese => "日本語",
        _ => "English",
    };

    public static bool IsSupported(InterfaceLanguage language)
    {
        foreach (var supportedLanguage in SupportedLanguages)
        {
            if (supportedLanguage == language)
                return true;
        }
        return false;
    }

    public static IReadOnlyList<UiText> MissingKeys(InterfaceLanguage language)
    {
        var missing = new List<UiText>();
        Translations.TryGetValue(language, out var translation);
        foreach (var key in Enum.GetValues<UiText>())
        {
            if (translation is null || !translation.ContainsKey(key))
                missing.Add(key);
        }
        return missing;
    }

    private static InterfaceLanguage FromClientLanguage(ClientLanguage language) => language switch
    {
        ClientLanguage.Japanese => InterfaceLanguage.Japanese,
        ClientLanguage.German => InterfaceLanguage.German,
        ClientLanguage.French => InterfaceLanguage.French,
        _ => InterfaceLanguage.English,
    };

    private static IReadOnlyDictionary<UiText, string> Create(params string[] values)
    {
        var keys = Enum.GetValues<UiText>();
        if (values.Length != keys.Length)
            throw new InvalidOperationException($"Localization catalog has {values.Length} values for {keys.Length} keys.");

        var result = new Dictionary<UiText, string>(keys.Length);
        for (var index = 0; index < keys.Length; index++)
            result[keys[index]] = values[index];
        return result;
    }
}
