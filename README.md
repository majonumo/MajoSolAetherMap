# Majo & Sol Aether Map

<p align="center">
  <img src="assets/majo-and-sol.png" alt="Majo & Sol under the moonlight" width="100%">
</p>

> **Same party, always.**

A bilingual Dalamud plugin for tracking real Aether Current progress, exploration locations and quest NPCs throughout Final Fantasy XIV.

Developed by **Majo & Sol** · Created November 18, 2026 · Dalamud API 15 · `/aethermap`

**Human-led, AI-assisted development.** Product direction, implementation decisions, in-game validation and publication approval are human-led; AI tools assisted with research, implementation and documentation.

Majo & Sol Aether Map is an unofficial community project. It is not affiliated with, endorsed by, or sponsored by Square Enix, XIVLauncher or Dalamud.

## English

### Features

- Detects the character's current zone and real flight-unlock progress.
- Browse every supported Aether Current zone without travelling there.
- Lists pending and completed exploration currents and quests.
- Resolves real quest names, issuer NPCs, territories, maps and coordinates from Lumina.
- Places native FFXIV map flags for exploration currents and quest NPCs.
- Draws an optional passive multi-marker overlay over the native map.
- Differentiates exploration currents, side-quest NPCs and main-scenario NPCs.
- Includes persistent marker filters, size, opacity and debug settings.
- Spanish and English interface.
- Uses the game's own localized data; it does not ship extracted game assets.

### Screenshots

The official project cover is shown above. Additional in-game screenshots will be added only after privacy review; the capture checklist is documented in [`assets/screenshots/README.md`](assets/screenshots/README.md). No unvalidated or fabricated UI screenshots are presented.

### Installation through the Custom Plugin Repository

Until the plugin is accepted into the official Dalamud repository:

1. Open `/xlsettings` in game.
2. Go to **Experimental > Custom Plugin Repositories**.
3. Add this URL and save the settings:

   ```text
   https://github.com/majonumo/MajoSolAetherMap/releases/latest/download/pluginmaster.json
   ```

4. Open `/xlplugins` and search for **Majo & Sol Aether Map**.
5. Select **Install**, then use `/aethermap` to open the plugin.

Dalamud will use the repository manifest to install compatible releases and offer updates when their `AssemblyVersion` increases. This is a third-party repository and has not been reviewed or endorsed by the Dalamud team.

### Manual fallback

If the custom repository is temporarily unavailable, download `latest.zip` from the matching [GitHub Release](https://github.com/majonumo/MajoSolAetherMap/releases), extract it to a permanent folder and add the DLL under **Experimental > Dev Plugin Locations**.

Do not load the DLL directly from inside the ZIP, and do not install builds from untrusted mirrors.

### Commands

| Command | Description |
| --- | --- |
| `/aethermap` | Opens or closes the main Aether Map window. |

### Development installation

1. Install XIVLauncher with Dalamud enabled.
2. Build the project with .NET SDK 10:

   ```powershell
   dotnet build .\MajoSolAetherMap.slnx --configuration Release
   ```

3. Open `/xlsettings` in game.
4. Under **Experimental > Dev Plugin Locations**, add the full path to:

   `src/MajoSolAetherMap/bin/x64/Release/MajoSolAetherMap.dll`

5. Enable **Majo & Sol Aether Map** under `/xlplugins` and open it with `/aethermap`.

For a future official release, install through `/xlplugins` after the plugin has passed Dalamud testing review. Do not install DLLs from untrusted sources.

### Build requirements

- .NET SDK 10
- Dalamud API 15 / `Dalamud.NET.Sdk 15.0.0`
- A local Dalamud installation, or `DALAMUD_HOME` pointing to one
- Final Fantasy XIV game data available to Dalamud/Lumina at runtime

### Safety and limitations

- Dalamud and third-party plugins are unofficial. Use them at your own risk and follow the applicable Square Enix rules and policies.
- The plugin reads game state and Lumina sheets. It does not automate movement, modify game files, write to `AgentMap`, or alter native marker arrays.
- The native-map overlay reads typed `AddonAreaMap` geometry. Game patches may temporarily break map detection or alignment.
- Map identity is resolved conservatively from localized native-addon labels. Ambiguous maps are skipped instead of guessed.
- Quest NPC locations reflect authoritative game data when available. Ambiguous locations are reported and never fabricated.
- Final Fantasy XIV and all related assets are property of SQUARE ENIX CO., LTD. No proprietary game textures are redistributed by this repository.

### AI-Assisted Development

**AI involvement: Copilot**

This project was conceived, directed, and tested by its human developer, with substantial implementation and documentation assistance from ChatGPT and OpenAI Codex.

I defined the features, made design decisions, configured the development environment, tested the plugin in FFXIV, reported bugs, and verified the final build.

The Majo & Sol illustration was also AI-generated.

Developed by Majo & Sol — Same party, always.

## Español

### Funcionalidades

- Detecta la zona actual y el progreso real para desbloquear el vuelo.
- Permite consultar todas las zonas compatibles sin viajar hasta ellas.
- Lista corrientes de exploración y misiones pendientes o completadas.
- Resuelve nombres reales de misiones, NPC, territorios, mapas y coordenadas mediante Lumina.
- Coloca banderas nativas para corrientes y NPC.
- Ofrece un overlay pasivo opcional con múltiples marcadores sobre el mapa original.
- Diferencia corrientes, NPC de misiones secundarias y NPC de historia principal.
- Conserva filtros, tamaño, opacidad y modo Debug.
- Interfaz en español e inglés.
- Usa datos localizados del juego sin distribuir recursos extraídos de FFXIV.

### Capturas

La portada oficial aparece al comienzo del README. Se añadirán capturas dentro del juego únicamente después de revisar su privacidad; la lista está en [`assets/screenshots/README.md`](assets/screenshots/README.md). No se presentan capturas de interfaz inventadas o sin validar.

### Instalación mediante el Custom Plugin Repository

Hasta que el plugin sea aceptado en el repositorio oficial de Dalamud:

1. Abrir `/xlsettings` dentro del juego.
2. Ir a **Experimental > Custom Plugin Repositories**.
3. Agregar esta URL y guardar la configuración:

   ```text
   https://github.com/majonumo/MajoSolAetherMap/releases/latest/download/pluginmaster.json
   ```

4. Abrir `/xlplugins` y buscar **Majo & Sol Aether Map**.
5. Seleccionar **Install** y luego usar `/aethermap` para abrir el plugin.

Dalamud utilizará el manifiesto para instalar versiones compatibles y ofrecer actualizaciones cuando aumente su `AssemblyVersion`. Este es un repositorio de terceros que no fue revisado ni respaldado por el equipo de Dalamud.

### Instalación manual alternativa

Si el repositorio personalizado no estuviera disponible temporalmente, descargar `latest.zip` desde la [GitHub Release](https://github.com/majonumo/MajoSolAetherMap/releases) correspondiente, extraerlo en una carpeta permanente y agregar la DLL desde **Experimental > Dev Plugin Locations**.

No cargues la DLL desde dentro del ZIP ni instales compilaciones de mirrors no confiables.

### Comandos

| Comando | Descripción |
| --- | --- |
| `/aethermap` | Abre o cierra la ventana principal de Aether Map. |

### Instalación para desarrollo

1. Instalar XIVLauncher con Dalamud habilitado.
2. Compilar con .NET SDK 10:

   ```powershell
   dotnet build .\MajoSolAetherMap.slnx --configuration Release
   ```

3. Abrir `/xlsettings`.
4. En **Experimental > Dev Plugin Locations**, agregar la ruta completa de la DLL Release.
5. Habilitar el plugin desde `/xlplugins` y abrirlo mediante `/aethermap`.

Cuando exista una publicación oficial aprobada, la instalación recomendada será mediante `/xlplugins`. No instales DLL de fuentes no confiables.

### Responsabilidad y límites

- Dalamud y sus plugins son herramientas no oficiales. El uso es responsabilidad del jugador.
- El plugin no automatiza el personaje, no modifica archivos del juego y no escribe sobre `AgentMap` ni arrays nativos.
- Los parches del juego pueden afectar temporalmente la detección o alineación del overlay.
- Los mapas ambiguos y ubicaciones no verificables se omiten en lugar de inventarse.
- Final Fantasy XIV y sus recursos pertenecen a SQUARE ENIX CO., LTD.
- Este es un proyecto comunitario no oficial, sin afiliación, respaldo ni patrocinio de Square Enix, XIVLauncher o Dalamud.

### Desarrollo asistido por IA

**Nivel de participación de IA: Copilot**

Este proyecto nació de una idea propia y fue dirigido y probado personalmente por mí, con una participación importante de ChatGPT y OpenAI Codex en la implementación técnica y la documentación.

Me encargué de definir las funcionalidades, tomar decisiones sobre el diseño, configurar el entorno de desarrollo, probar el plugin directamente dentro de Final Fantasy XIV, detectar errores, solicitar correcciones y verificar que la versión final compilara y funcionara correctamente.

La ilustración de Majo y Sol utilizada en el proyecto también fue generada mediante inteligencia artificial.

*Desarrollado por Majo & Sol — Same party, always.*

## Repository and distribution

- Original project code: [MIT License](LICENSE)
- Version history: [CHANGELOG.md](CHANGELOG.md)
- Official-repository preparation: [distribution/README.md](distribution/README.md)
- Author-art asset instructions: [assets/README.md](assets/README.md)

**Developed by Majo & Sol.** Human-led, AI-assisted development. *Same party, always.*
