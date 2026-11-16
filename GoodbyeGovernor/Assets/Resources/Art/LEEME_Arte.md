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
| `UI/UI_IconoGaceta_Normal` / `_Hover` | La Gaceta en la mesa (abre el periódico; aviso rojo = noticias sin leer) | Papel con texto |
| `Despacho/Fondo_Despacho` | Fondo del despacho (320x180, se ve a x6). **Si falta, se usa el despacho anterior entero** | Despacho generado por código |
| `Despacho/Bandeja_Expedientes` | Bandeja de la mesa: de aquí se coge el expediente; el número va sobre el círculo rojo dibujado | Rectángulo de madera |
| `Sellos/Sello_Aprobado` / `_Denegado` | Sellos que se arrastran (derecha = sí, izquierda = no) | Sello generado por código |
| `Sellos/Tampon_Verde` / `_Rojo` | Tampones bajo cada sello | Nada |
| `Sellos/Marca_Aprobado` / `_Denegado` | Marca que deja el sello en el papel (solo si la etiqueta es APROBADO/DENEGADO; las cartas con etiquetas propias usan la marca de texto) | Marco con texto |

## Importación
`Assets/Editor/ElDespachoArtImporter.cs` aplica solo a esta carpeta: Sprite, filtro Point, sin compresión, sin mipmaps, 100 PPU.
Bordes 9-slice: botones (10, 3, 3, 3), caja de diálogo (72, 10, 16, 14) en píxeles del arte (izq., abajo, der., arriba).
Para forzarlo: Tools > El Despacho > Arte > Reimportar el arte del juego.
Escala en pantalla: `Art.PixelScale = 4` (1 píxel del arte = 4 píxeles a 1920x1080) en las piezas estiradas.
El despacho ilustrado usa x6 (`ArtPx` en `DespachoGame.Office.cs`): el fondo de 320x180 llena 1920x1080 y las piezas
se colocan con las coordenadas en píxeles del fondo (`At(stage, nombre, x, y, ancho, alto)`, origen arriba a la izquierda).

## Mapa del fondo (píxeles del arte, 320x180)
Ventana x15-59 y15-57 (alféizar y58-68) · corcho x73-247 y6-66 · pared derecha x252-318 · borde de la mesa y72-80 ·
vade x72-249 y80-171 · mesa izquierda x0-72 · mesa derecha x249-320. Si el fondo cambia, hay que revisar estas cifras.

## Añadir un retrato
1. Copiar el PNG (64x64) a `Retratos/`.
2. Añadir la línea `{ "id_del_pais", "Nombre_Archivo" }` en el diccionario `portraits` de `Assets/Scripts/ElDespacho/Art.cs`.
Los ids de país están en `CountryDatabase.cs` (korovia, selvana, lindmark...).

## Sellos, tampones y marcas
El tamaño en pantalla sale del propio PNG (x6), así que se pueden redibujar con otra medida sin tocar código.
Los sellos se colocan centrados en su columna del vade con el pie apoyado sobre el tampón; la etiqueta va debajo.
La bandeja lleva el número de pendientes sobre el círculo rojo de la esquina superior derecha (píxeles 51-63, 1-13): si se mueve el círculo, ajustar `PendingBadge` en `DespachoGame.Office.cs`.
