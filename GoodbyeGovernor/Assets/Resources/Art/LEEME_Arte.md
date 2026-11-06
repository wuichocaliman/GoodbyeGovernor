# Arte del juego (pixel art)

Origen: `Arte y comcepts/Comcepts/GoodbyeGovernor_Arte/` (pack original, con vistas x6 y paleta).
Aquí solo van los sprites a tamaño real que usa el juego. Se cargan por código con `Art.Get("Carpeta/Nombre")`.

| Archivo | Dónde se usa | Si falta |
| --- | --- | --- |
| `Iconos/Icono_TuPais` | Mapa: tu país y tu bloque (antes «B») | Círculo con letra |
| `Iconos/Icono_Visitado` | Mapa: país ya decidido (antes «i») | Círculo con letra |
| `Iconos/Icono_SinVisitar` | Mapa: sin visitar (antes «?») | Círculo con letra |
| `Iconos/Icono_Amenaza` | Mapa: posible amenaza (antes «!») | Círculo con letra |
| `Iconos/Icono_Aliado` | Mapa: posible aliado (antes «+») | Círculo con letra |
| `Retratos/Retrato_Korovia` | Visita a Korovia | Retrato generado por código |
| `Retratos/Retrato_Valdoria` | Visita a Selvana (PROVISIONAL: «Valdoria» no existe en el juego) | Retrato generado por código |
| `UI/UI_CajaDialogo` | Conversación en las visitas; el nombre del líder va en la pestaña | Panel de papel |
| `UI/UI_BotonOpcion_Normal` / `_Seleccionado` | Preguntas de las visitas (seleccionado = ratón encima) | Botón de madera |
| `UI/UI_Dossier` | Portada del expediente cerrado en la pila | Portada de texto |

## Importación
`Assets/Editor/ElDespachoArtImporter.cs` aplica solo a esta carpeta: Sprite, filtro Point, sin compresión, sin mipmaps, 100 PPU.
Bordes 9-slice: botones (10, 3, 3, 3), caja de diálogo (72, 10, 16, 14) en píxeles del arte (izq., abajo, der., arriba).
Para forzarlo: Tools > El Despacho > Arte > Reimportar el arte del juego.
Escala en pantalla: `Art.PixelScale = 4` (1 píxel del arte = 4 píxeles a 1920x1080).

## Añadir un retrato
1. Copiar el PNG (64x64) a `Retratos/`.
2. Añadir la línea `{ "id_del_pais", "Nombre_Archivo" }` en el diccionario `portraits` de `Assets/Scripts/ElDespacho/Art.cs`.
Los ids de país están en `CountryDatabase.cs` (korovia, selvana, lindmark...).
