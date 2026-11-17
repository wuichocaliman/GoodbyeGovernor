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
| `Despacho/Ventana_Tranquilo` / `_Nublado` / `_Humo` / `_Crisis` | Vista de la ventana (el cristal, 44x42, con los parteluces): crisis si la media < 40; humo si algún indicador < 35; tranquilo si la media >= 60; si no, nublado | Cielo oscurecido y manchas de humo |
| `Despacho/Bandeja_Expedientes` | Bandeja de la mesa: de aquí se coge el expediente; el número va sobre el círculo rojo dibujado | Rectángulo de madera |
| `Sellos/Sello_Aprobado` / `_Denegado` | Sellos que se arrastran (derecha = sí, izquierda = no) | Sello generado por código |
| `Sellos/Tampon_Verde` / `_Rojo` | Tampones bajo cada sello | Nada |
| `Sellos/Marca_Aprobado` / `_Denegado` | Marca que deja el sello en el papel (solo si la etiqueta es APROBADO/DENEGADO; las cartas con etiquetas propias usan la marca de texto) | Marco con texto |
| `Despacho/Ficha_Indicador` | Tarjetas de Estabilidad/Seguridad/Prosperidad del corcho (38x28) | Tarjeta crema |
| `Despacho/Recorte_Prensa` | Recorte con el último titular (52x22); el texto va bajo la regla roja | Papel liso |
| `Despacho/Informe_Confidencial` | Informe con los países que esperan audiencia (56x23); el texto evita el lomo y el clip | Papel liso |
| `Despacho/Mapa_Clavado` | Mapa del corcho que abre el mapa del mundo (40x38) | Mapa dibujado por código |
| `Despacho/Chincheta` | Chinchetas de todos los papeles del corcho y la nota (6x5) | Círculo rojo |
| `Despacho/Reloj_Esfera` + `Reloj_Marco` | Reloj del mandato (30x30); los cuartos del año se pintan entre los dos | Reloj por código |
| `Despacho/Nota_Asesor` | Nota del asesor (63x66) | Papel amarillo |
| `Despacho/Placa_Nombre` | Placa «Presidencia de Aurelia» (56x13); el texto va encima | Placa por código |
| `Despacho/Papel_Expediente` | Papel del expediente abierto y cerrado (99x86); el contenido va dentro del doble marco | Papel liso |
| `UI/Boton_Laton` | Botón de cerrar el año y DOBLAR EL PERIÓDICO (58x11) | Botón dorado |
| `UI/Gaceta_Abierta` | Periódico abierto (136x160) | Papel liso |
| `UI/Gaceta_Ilustracion_Economia` / `_Exterior` / `_Politica` | Foto de la noticia principal según el tema (40x28) | Recuadro vacío |
| `UI/Sello_Nuevo` | Marca «NUEVO» en las noticias sin leer (26x9) | Texto rojo |

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
