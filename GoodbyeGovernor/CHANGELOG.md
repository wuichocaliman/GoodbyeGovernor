## v0.9.14-prototipo — Expediente de formación presidencial (2026-11-17)

- Rediseñados el índice de CÓMO JUGAR y la consulta de lecciones con estructura de expediente oficial: título, lección/nota, progreso, ficha de Julián Paz, documento de pergamino y navegación inferior uniforme.
- Conservadas las doce lecciones en dos columnas y las dieciocho notas, su contenido, orden y acciones. ATRÁS/SIGUIENTE se desactivan en los extremos; VOLVER/ESC regresan al índice.
- Fondo del despacho y botones ornamentados reutilizados. Reserva de retrato con monograma JP; no se usa la imagen de otro personaje.
- Tipografía propia sin reducción automática, escalas hasta 175%, alto contraste y scroll separado de los botones inferiores. Foco y activación existentes; desplazamiento del manual disponible en navegación accesible. Evitada la doble aplicación del adaptador genérico de texto.
- 907 comprobaciones nuevas de Unity Play Mode: recorrido de las dieciocho notas en ambos sentidos, 100/150/175% en 1920x1080 y 1280x720, textos y botones dentro de sus áreas, rueda/foco/activación simulados, contenido de narración y conservación completa de campaña. Regresiones de tutorial (54), menús/guardados (58), semántica (116), lector (141) y navegación/scroll (122), con persistencia en proceso nuevo.
- Actualizada una prueba antigua que localizaba botones por el prefijo «1.» y el título anterior; ahora usa identificadores estables y la presentación actual. No se alteró la lógica para satisfacer la prueba.
- TutorialLessons, recursos de notas, activación contextual, progreso, omisión, guardados, cartas y diplomacia sin modificar. Copia previa verificada; ocho archivos reales de preferencias/partidas intactos por SHA-256.
- Pendientes evaluación física/humana, escucha de pronunciación y retrato real de Julián Paz. Este cambio no rediseña la nota contextual superpuesta durante la partida.
- Informe: Documentation/El_Despacho_Tutorial_Presidencial_Rediseno_v0.9.14-prototipo.pdf. Capturas y registros: Documentation/Tutorial_Presidencial_v0914/.

## v0.9.13-prototipo — Accesibilidad durante la partida (2026-11-17)

- Conectados alto contraste y ampliación de texto con la interfaz ilustrada mediante adaptadores uGUI reversibles. Placas de contraste locales, texto completo desplazable sin encogimiento, etiquetas de estado y separación de textos de indicadores. Ilustraciones conservadas.
- Reorganización de la columna del mapa y de preguntas/pestañas/decisiones diplomáticas al ampliar texto. Corregida la selección de referencias a paneles antiguos durante reconstrucciones. Tutorial con bandas independientes de título, cuerpo y controles; sello decorativo de inteligencia separado de su título.
- Ampliación de áreas de botones nativos, preservando callbacks y rectángulos de controles vecinos. Ayudas de herramientas para mapa, preguntas, dossier, inteligencia, comercio y decisiones; sin revelar secretos ni cambiar narrativa.
- Narración de contenido actual y opciones disponibles; eliminación de una segunda lectura redundante del tutorial. Reactivación y F3 consultan contenido actual. Se conservan detener/repetir, voz local, avisos y acceso voluntario F2.
- Conservados definiciones/reversibilidad/persistencia de perfiles, reglas, cartas, cadenas, diplomacia, años, finales y guardados. El menú mantiene tarjetas/scroll/foco v0.9.12; solo se actualizan dos descripciones de alcance.
- Unity Play Mode: 322 comprobaciones nuevas de cobertura; cuatro perfiles en diez pantallas, reversión y conservación de campaña; combinaciones 5/3/12/15; tamaños 75–175%, 1920x1080 y Game View 1280x720. Pruebas de ratón virtual para documento/sello/cancelación/decisión no duplicada, también con Lectura cómoda combinada. Regresiones existentes completadas; persistencia individual y combinada en procesos separados.
- Una expectativa de la prueba combinada se corrigió: Lectura cómoda también aporta áreas ampliadas, que no deben desactivarse al retirar Interacción sencilla. No se cambió la lógica de perfiles.
- Narración automática y aceptación por proceso comprobadas. WAV con Microsoft Helena Desktop es-ES y muestras no nulas; no equivale a escucha humana ni a grabación de salida de Unity.
- Límites explícitos: rótulos pequeños/decorativos y botones estrechos requieren scroll a gran escala; no se certifica adaptación visual completa ni todas las variantes de informe/foto o cada país con cada combinación. Pendientes dispositivos físicos, comprensión con jugadores, escucha/pronunciación y rendimiento en equipos lentos.
- Backup previo verificado. Ocho archivos reales de partidas/preferencias intactos por SHA-256. QualitySettings restaurado tras pruebas.
- Informe y matriz: Documentation/El_Despacho_Accesibilidad_Gameplay_v0.9.13-prototipo.pdf. Evidencias: Documentation/Accesibilidad_Gameplay_v0913/.

## v0.9.12-prototipo — Tarjetas uniformes de Accesibilidad (2026-11-17)

- Corregida la causa de las diferencias: cada tarjeta calculaba su altura por separado y las tarjetas sin pareja ocupaban todo el ancho. Ahora se mide el contenido de cada categoría y se comparten altura, ancho, espacio de cabecera, descripción y zona de control.
- Controles alineados abajo con el mismo margen; separación constante de 20 unidades entre tarjetas. Escala de interfaz y las demás tarjetas sueltas mantienen el ancho de una columna cuando la cuadrícula tiene dos.
- Aplicado a Visión y lectura, Controles, Comprensión y Audio (incluido el audio compartido de Ajustes). PERFILES, encabezados y notas conservan su diseño. No se cambian descripciones, acciones, disponibilidad ni preferencias.
- Las dimensiones comunes se recalculan con el ancho y las escalas actuales; una columna para texto mayor del 125% o espacio reducido. Se conserva texto completo, sin recortarlo ni reducirlo para encajar.
- 117 comprobaciones de Unity Play Mode: igualdad de dimensiones, cabeceras, descripciones, márgenes y alineación inferior; texto 175% e interfaz 120% en Game View real 1280x720, alto contraste, scroll y foco. Regresiones existentes de categorías, perfiles, persistencia, restauración, lector, tutorial, menús y guardados. Entradas simuladas; pruebas físicas y evaluación humana pendientes.
- Backup previo verificado y ocho archivos reales de preferencias/partidas preservados por SHA-256. Sin cambios en el modelo de opciones ni en documentos, diplomacia o campaña.
- Informe: Documentation/El_Despacho_Tarjetas_Uniformes_v0.9.12-prototipo.pdf. Capturas antes/después y registros: Documentation/Tarjetas_Uniformes_v0912/.


## v0.9.11-prototipo — Categorías de Accesibilidad organizadas como PERFILES (2026-11-17)

- Visión y lectura, Controles, Comprensión y Audio y narración usan tarjetas compactas con icono, título, explicación y control/estado, agrupadas con encabezados y separadores dorados. El modelo, orden, disponibilidad y acciones de las opciones no cambian.
- Dos columnas cuando el ancho y el tamaño de texto lo permiten; una columna con texto mayor del 125% o menos de 840 píxeles disponibles. Alturas según contenido, sin reducir letras para encajarlas. Notas informativas y vistas previas compactas fuera de tarjetas de acción.
- Conservados el fondo ilustrado, panel oscuro, botones ornamentados, tipografía, navegación lateral, PERFILES y restauración. El audio compartido de Ajustes utiliza la misma presentación agrupada.
- Sin cambios en preferencias, perfiles, guardados, F2, tutorial, documentos o diplomacia. Conservados AccessScrollRect y el marco de foco hijo del control con su recorte por máscara.
- 57 comprobaciones nuevas de Unity Play Mode: cuatro categorías, columnas, texto y estado combinado al 175%/120% con alto contraste, Game View real 1280x720, conservación de scroll, foco oculto y retorno. Capturas antes/después; regresiones existentes de controles, perfiles, persistencia, restauración, lector, teclado, tutorial y menús. Entradas simuladas; no se declara prueba física ni evaluación humana.
- Backup previo y ocho archivos reales de usuario conservados mediante comparación SHA-256. Tests aislados en directorios temporales. No se amplía el alcance funcional de ajustes ni la disponibilidad de audio existente.
- Informe: Documentation/El_Despacho_Accesibilidad_Visual_Unificada_v0.9.11-prototipo.pdf. Capturas y registros: Documentation/Accesibilidad_Visual_v0911/.


## v0.9.10-prototipo — PERFILES compacto y foco recortado (2026-11-17)

- PERFILES conserva cuatro tarjetas con icono, descripción breve, botón y marca activa. Retirados los paneles Personaliza cómo quieres jugar, Perfiles recomendados y Preferencias personales.
- Opciones activas pasa a resumen compacto; restauración a botón con su confirmación existente. Se conserva aviso de migración en el resumen, prioridad de ajustes personales y lógica de perfiles sin modificaciones.
- Cuadrícula 2 x 2 con ancho suficiente; una columna para texto mayor del 125% o espacio reducido. Botones ornamentados, fondo y recursos actuales conservados.
- Corregido el marco flotante: ahora es hijo del control enfocado y hereda ScrollRect/RectMask2D, en lugar de dibujarse en un Canvas superpuesto independiente. No intercepta eventos ni altera la rueda.
- 20 comprobaciones específicas en Unity Play Mode: geometría, texto hasta 175%, Game View 1280x720, foco con cull efectivo fuera de pantalla y retorno por teclado. Regresiones de perfiles, persistencia, restauración, accesibilidad, lector, scroll y menús ejecutadas. Entradas simuladas; sin prueba física ni evaluación humana.
- Campaña, Audio, otras categorías, lector, tutorial, diplomacia y documentos sin cambios funcionales. Preferencias/partidas reales conservadas por SHA-256; backup previo local.
- Informe: Documentation/El_Despacho_Perfiles_y_Foco_v0.9.10-prototipo.pdf. Evidencias: Documentation/Perfiles_Visual_v0910/.
- Documentada incidencia de tutorial durante recarga de scripts en la sesión previa; fuera de este alcance, no se declara corregida.

## v0.9.9-prototipo — Perfiles aplicados al despacho (2026-11-17)

- Comparado el recorrido de ratón simulado en Unity Play Mode antes/después: los dos clics y sellos simplificados ya funcionaban, pero las instrucciones pedían arrastrar y las áreas ampliadas no alcanzaban los objetos del despacho.
- Añadida superficie de selección ampliada reversible a expedientes cerrados y sellos disponibles. Se reutilizan sus manejadores y consecuencias, sin ampliar la zona válida de confirmación ni cambiar reglas.
- Zona de lectura y notas del tutorial adaptan instrucciones al modo de clics. La nota del asesor muestra el siguiente paso, cancelación, confirmación y continuación.
- Comprensión guiada conecta ayudas, definiciones de indicadores, símbolos y consulta AYUDA/GLOSARIO al despacho ilustrado; conserva tutorial desactivado/completado y preferencias personalizadas. No revela secretos ni cambia cartas o diálogos.
- Pruebas nuevas de entrada real de Unity con ratón virtual, márgenes ampliados, consultas, reversión y reinicio; regresiones de perfiles, lector, documentos, tutorial, diplomacia, guardados y teclado simulado. Sin prueba física ni evaluación humana de comprensión.
- Conservación de ocho archivos reales de preferencias/partidas verificada por SHA-256. Copia previa local; sin migración de datos.
- Informe y comparativas: Documentation/El_Despacho_Perfiles_Jugabilidad_v0.9.9-prototipo.pdf. Evidencia: Documentation/Perfiles_Jugabilidad_v099/.
- Alcance pendiente: evaluación con jugadores nuevos; no se transforma toda la ayuda del mapa/diplomacia ni el despacho alternativo sin arte. El tamaño de texto no se convierte en ajuste global.

## Auditoría funcional de accesibilidad sobre v0.9.8 (2026-11-17)

- Añadida FunctionalEvidenceProbe (Editor): 92 comprobaciones de propiedades renderizadas, reversión de controles, campaña aislada y capturas; persistencia completa comprobada en otro proceso de Unity.
- Reejecutadas regresiones de perfiles, accesibilidad, lector, navegación, tutorial, documentos y menús. Pruebas en Unity Play Mode con eventos simulados; no se afirma uso de periféricos físicos ni escucha humana.
- Documentado alcance parcial: contraste y texto no cambian despacho/mapa/diplomacia ilustrados ni texto del tutorial; Música/Ambiente no disponibles; salida audible y confort requieren prueba manual.
- Sin cambios en lógica de producción ni incremento de versión. Partidas y preferencias reales conservadas, verificadas por SHA-256; copia previa fuera de la documentación compartible.
- Informe: Documentation/El_Despacho_Verificacion_Funcional_Accesibilidad_v0.9.8.pdf. Tabla, resultados y capturas: Documentation/Verificacion_Accesibilidad_v098/.

## v0.9.8-prototipo — Navegación de Ajustes y desplazamiento (2026-11-17)

- Corregida la entrada AJUSTES del menú principal y pausa: abre la portada general (ShowSettings(-1)), con Audio, Pantalla, Jugabilidad, Controles y Accesibilidad. Ya no abre Audio accesible de forma implícita.
- Audio conserva un único panel/modelo, con navegación general cuando se abre desde Ajustes y contexto accesible cuando se abre desde el centro. El contexto no se guarda como preferencia.
- VOLVER y ESC comparten el retorno por niveles. Entrar en Accesibilidad requiere seleccionar su apartado; cerrar y abrir Ajustes no conserva una categoría equivocada.
- Nuevo AccessScrollRect: delta de rueda limitado, 48 unidades por evento completo, pasos fraccionarios, transición breve con tiempo no escalado, límites Clamped y sin inercia prolongada. Reducir movimiento aplica pasos limitados sin transición.
- El refresco ya no revela incondicionalmente el foco antiguo. La navegación/activación explícita por teclado conserva visibilidad; arrastre, barra, paginación y foco cancelan un destino anterior de rueda.
- 122 comprobaciones nuevas en Unity Play Mode: recorridos repetidos, contextos y retorno, rueda simulada, posiciones intermedias, parada, límites, foco, texto al 100/175 % y reducción de movimiento. Regresiones de perfiles, persistencia, accesibilidad, lector, arranque, tutorial, documentos, campaña y guardados ejecutadas; resultados detallados en el informe.
- Conservados fondo, panel oscuro, oro, botones, cuatro categorías, cuatro perfiles y todas las funciones. Sin cambios de preferencias ni formato de guardados. Sin pruebas con rueda o teclado físicos; la comodidad con hardware diverso sigue pendiente.
- Copia previa verificada. Informe: Documentation/El_Despacho_Navegacion_y_Scroll_v0.9.8-prototipo.pdf.

## v0.9.7-prototipo — Simplificación y consolidación de accesibilidad (2026-11-17)

- Cuatro categorías: Visión y lectura, Controles, Comprensión y Audio y narración. Los cuatro perfiles reversibles aparecen primero; estado y restauración se reúnen en su portada. Se retira el botón redundante de guardar: los cambios siguen guardándose al realizarse.
- Ajustes/Audio y Accesibilidad/Audio abren el mismo panel y usan los mismos datos. Contraste conserva una sola propiedad. Se mantiene la estética del escritorio y lámpara, controles ornamentados, oro, enfoque y desplazamiento.
- Texto cambia fuentes del centro/lector; escala cambia espacios, iconos y mínimos de controles sin multiplicar la fuente. Se conservan los porcentajes existentes. Áreas ampliadas sigue siendo un ajuste diferente.
- Glosario independiente de Símbolos. Migración navigationSchema conserva el acceso efectivo anterior y las bases de perfiles; tras migrar puede desactivarse sin apagar Símbolos.
- Lectura opcional pasa a información. Se conserva readingView como recordatorio antiguo; F2 y Abrir ahora siguen siendo explícitos. No se abre automáticamente al iniciar o cargar.
- ATRÁS del manual abre la última nota de la lección anterior. Las 18 notas se recorren en ambos sentidos sin alterar contenido, IDs ni progreso.
- Play Mode: 116 comprobaciones semánticas, 654 de perfiles/controles, 50 UX, 21 presentación, 338 accesibilidad, 141 lector, 22 + 15 arranque, 54 tutorial, 58 menús/guardados y 21 arte; reinicios independientes completados. Menús incluye la regresión de documentos; lector/accesibilidad cubren mapa y visitas. Los recuentos cambian por la retirada de duplicaciones y nuevos contratos.
- Revisión directa con ratón en Unity: cuatro categorías, volumen 80 a 50 %, contraste, texto hasta 175 %, rueda y retorno a Ajustes. Tras adelantar los perfiles, se repitieron presentación y perfiles. Pendientes teclado físico, escucha humana y pantalla en ejecutable.
- Sin cambios en mecánicas de documentos/sellos, diplomacia, indicadores, narrativa, años, finales ni formato de guardado. Música y Ambiente siguen sin pistas; voz Windows y todos sus controles se conservan.
- Backup verificado antes de editar. Informe: Documentation/El_Despacho_Simplificacion_Accesibilidad_v0.9.7-prototipo.pdf.

## v0.9.6-prototipo — Auditoría semántica de accesibilidad (2026-11-17)

- Corregido Brillo por teclado: las flechas respetan el rango real 65–115 %, con pasos del 5 % del rango. Reproducción previa y verificación del efecto visual y reversión en Unity Play Mode.
- Aclaradas descripciones de texto/escala, indicadores, ayudas, símbolos/glosario, preferencia de lectura, mono y restauraciones. Se conserva su funcionamiento y persistencia.
- Inventario de 120 registros con efectos reales, datos compartidos, evidencia, limitaciones y recomendaciones. Propuesta de cuatro grupos y perfiles rápidos, sin reorganización ni eliminación de funciones.
- 92 comprobaciones semánticas nuevas y reinicio independiente; regresiones de perfiles (705), UX (57), estilo (25), accesibilidad (588), lector (141), arranque (22 + 15), tutorial (54), menús/guardados (58) y arte (21), además de persistencia, completadas. Los PASS que reproducen limitaciones no certifican un diseño correcto.
- Detectado ATRÁS del manual: salta a la primera nota de la lección anterior y omite sus notas finales. Pendiente; no se modifica el tutorial protegido por el encargo.
- Pendientes escucha humana, teclado físico y verificación de pantalla en ejecutable/monitor. Música y Ambiente siguen sin recursos; mono no altera clips ya mono; narración no equivale a audiodescripción de arte.
- Copia previa verificada. No cambian campaña, documentos, sellos, indicadores, diplomacia, tutorial, guardados, narrativa, finales, arte ni escenas.
- Informe: Documentation/El_Despacho_Auditoria_Simplificacion_Accesibilidad.pdf. Matriz editable: Documentation/Inventario_Simplificacion_Accesibilidad.csv.

## v0.9.5-prototipo — Perfiles reversibles y auditoría de accesibilidad (2026-11-17)

- Corregidos los cuatro perfiles: ahora alternan Activar/Desactivar, conservan una base personalizada y persisten su estado. Indicador ACTIVO y borde reforzado.
- Capas combinables e independientes del orden: requisitos booleanos por unión y texto de Lectura cómoda con mínimo 150 %, sin reducir una base mayor. Los cambios manuales tienen prioridad y se conservan al retirar capas.
- Compatibilidad con JSON anteriores: se conservan los valores efectivos como base personalizada; se retiran marcas antiguas de perfil porque no incluían valores previos recuperables. Mensaje explicativo en el menú. No se borran preferencias intencionales.
- Corregidos desplazamiento tras recalcular el contenido y enfoque de controles fuera de pantalla, con texto ampliado y alto contraste. Se conserva el arte y los botones ornamentados.
- Áreas grandes ahora aumenta realmente la altura mínima de los controles del lector. Avisos visuales ahora muestra también un aviso temporal en el modo ilustrado. Actualizar el menú sin cambiar contenido ya no reinicia la narración después de detenerla.
- Etiquetas accesibles de los sellos ilustrados recuperadas sin modificar la lógica de decisiones; el texto visual de estos sellos vive junto a ellos.
- Nueva auditoría automatizada de controles, combinaciones de perfiles, overrides, migración, reinicio independiente, foco y atajos simulados. Pruebas de regresión de accesibilidad, lector, arranque, menús/guardados, documentos y tutorial.
- Música y Ambiente siguen desactivados por ausencia de pistas. Narración española validada a nivel de proceso y órdenes; no equivale a escucha humana ni a teclado físico.
- Se conserva el despacho ilustrado v0.10 ya presente y todos los sistemas de campaña. No cambian escenas, imágenes, cartas ni formato de guardados.
- Informe y tabla: Documentation/El_Despacho_Auditoria_Controles_Accesibilidad_v0.9.5-prototipo.pdf y Documentation/Auditoria_Controles_v095.csv.


## v0.9.4-prototipo — Accesibilidad integrada visualmente en Ajustes (2026-11-16)

- Sustituida la presentación beige por el mismo fondo de lámpara y escritorio, panel oscuro translúcido, separadores dorados y botones ornamentados de los ajustes normales.
- Navegación lateral con Perfiles, Visión, Movimiento y controles, Comprensión, Audio y General. Contenido agrupado a la derecha y VOLVER coherente con Ajustes.
- Cuatro perfiles conservados con paneles oscuros; opciones y audio mantienen sus acciones, sliders, estados y explicaciones.
- Escalado de texto y controles con alturas adaptativas; desplazamiento independiente de navegación y contenido, incluido seguimiento del foco por teclado. Compatibilidad con alto contraste.
- Conservados preferencias, narración, interacción simplificada, lectura explícita y todos los sistemas de juego. Se preserva el despacho ilustrado v0.10 ya presente en esta copia local.
- Añadida AccessibilityStyleProbe (25 comprobaciones). Adaptada únicamente la comprobación de etiquetas de DeskValidationProbe al diseño actual, que coloca los textos junto a los sellos; no se cambia el juego.
- Unity Play Mode: pasan accesibilidad UX (57), accesibilidad general (588), persistencia entre procesos, lectura (141), arranque (22 + 15), tutorial (54), menús/guardado (58) y arte (21). Suite de documentos completada; menús sin errores ni avisos de ejecución.
- Revisión visual y ratón en Play Mode: Perfiles, Audio, Visión, volumen 80 a 50 %, alto contraste, texto al 125 % y retorno a Ajustes. Pruebas con preferencias temporales. Teclado validado mediante eventos automatizados; pendiente revisión humana de teclado y voz y build de distribución.
- Informe: Documentation/El_Despacho_Accesibilidad_Visual_Polish_v0.9.4-prototipo.pdf.

## v0.10-prototipo — Despacho ilustrado con el nuevo pixel art (2026-11-14)

- La pantalla principal se monta sobre `Art/Despacho/Fondo_Despacho` (320x180 a x6) siguiendo el doc de diseño de la pantalla principal (opción B). Nuevo archivo `DespachoGame.Office.cs`; si el fondo no está, `ShowOffice` usa el despacho anterior sin cambios.
- Pared: ventana con el estado del país en el alféizar (se oscurece y echa humo en crisis); corcho con Estabilidad/Seguridad/Prosperidad clavados (contenedor `HUD`), recorte con el último titular, informe CONFIDENCIAL con los países que esperan audiencia, hilos rojos y mapa del mundo clavado que abre el mapa (con «N visitas» y el mismo temblor de antes); reloj del mandato con 4 cuartos y botón `YearEndButton` debajo.
- Mesa: nota del asesor con scroll y placa «Presidencia de Aurelia» (izquierda); vade como zona de lectura con el expediente en el centro y los sellos `Sello_*` sobre sus tampones a los lados (izquierda no, derecha sí); La Gaceta (`UI_IconoGaceta`) con aviso rojo de noticias sin leer que abre el periódico con scroll; bandeja `Bandeja_Expedientes` con el número de expedientes pendientes.
- El expediente cerrado descansa en la bandeja y se arrastra al vade. Los sellos solo se pueden usar con el expediente abierto. La marca usa `Marca_Aprobado/Denegado` con giro aleatorio cuando la etiqueta es APROBADO/DENEGADO.
- `DeskDocument`: ajustes opcionales antes de `Initialize` (`pileAnchor`, `closedScale`, `pileAngle`, `openSize`, colores y textos de la zona). Valores por defecto = comportamiento anterior.
- Escenario fijo de 1920x1080 escalado entero (`StageFitter`): en pantallas que no son 16:9 salen bandas, sin deformar el pixel art.
- Se conservan los nombres que usan tutorial, accesibilidad y pruebas: HUD, CardArea, ReadingZone, DeskDocument, OpenedContent, Card, StampRack, StampYes, StampNo, YearEndButton.
- Probado en Unity Play Mode: carga de partida, sellado (sí y no), continuar, mesa despejada, La Gaceta, mapa, visita, cierre del año 1 y expediente nuevo arrastrado desde la bandeja. Consola sin errores. Las pruebas automáticas (Assets/Tests) no se han reejecutado.
- Rediseño del arte integrado el mismo día: sellos (24x34), tampones (28x9), marcas (40x15), bandeja y La Gaceta nuevos. Los tamaños ya no están fijos en el código: se leen del propio sprite (x6), así que el artista puede cambiar las medidas sin tocar código. Con los sellos más estrechos, el expediente abierto gana algo de ancho.
- Pendiente de arte: expediente abierto, tarjetas de indicadores, mapa clavado, recorte, informe, reloj, nota y placa siguen generados por código.
- Informe: Documentacion/El_Despacho_Informe_Despacho_Ilustrado_v0.10.pdf.

## v0.9.3-prototipo — Vista de lectura explícita durante la campaña (2026-11-13)

- Eliminada la activación automática del lector mediante Reflow. Texto ampliado, teclado, símbolos, ayudas y perfiles ya no sustituyen el juego ilustrado.
- Lectura alternativa disponible mediante F2 o Accesibilidad > Visión > Abrir ahora; salida con F2, ESC o Volver al modo visual. La preferencia guardada se conserva y se explica como apertura manual.
- Preservados recursos gráficos, lógica de campaña, documentos, diplomacia, tutorial y archivos personales de preferencias.
- Recorrido completo verificado en Unity Play Mode con cuatro configuraciones: 141 comprobaciones, incluidos nueva partida/confirmación, documentos, mapa, visita, regreso, lectura manual, salida, carga y eventos F2.
- Regresiones reejecutadas: accesibilidad 588 (563 previas más la nueva fila de salida en 25 combinaciones), UX 57, arranque/reinicio 37, tutorial 54, menús/guardados 58 y arte/importación 21; persistencia entre procesos correcta.
- Revisión visual mediante ratón: menú, campaña, expediente, mapa, diplomacia y regreso; apertura del lector desde ajustes y restauración del mismo expediente ilustrado.
- Pendientes: teclado físico y build de distribución. F2 pasó mediante Input System, pero no produjo cambios observables con la herramienta visual. La maquetación ampliada propia del lector continúa disponible al abrirlo manualmente.
- Informe: Documentation/El_Despacho_Reading_View_Gameplay_Fix_v0.9.3-prototipo.pdf.

## v0.9.2-prototipo — Arranque y preferencias de accesibilidad (2026-11-13)

- Corregida la vista de lectura opaca que ocultaba el menú principal cuando había preferencias de accesibilidad guardadas. Principal conserva fondo original, título y botones; F2 permite solicitar la lectura explícitamente.
- Conservados perfiles y preferencias personales sin reset ni migración destructiva. Las etiquetas nativas del menú respetan el tamaño de texto guardado.
- Limpieza de estado transitorio al navegar; carga de preferencias generales desde valores iniciales cuando falta el archivo. Rutas temporales del editor aisladas entre sesiones y pruebas visuales limitadas a lanzamiento explícito.
- Verificación real en Unity Play Mode del arranque normal con las preferencias personales intactas. Sus archivos permanecen idénticos byte a byte.
- Pruebas: 22 comprobaciones nuevas de arranque y 15 de reinicio; regresiones reejecutadas: accesibilidad 563, UX 57, tutorial 54, menús/guardados 58, arte/importación 21 y persistencia entre procesos. Sin errores de compilación ni excepciones de ejecución detectados en las suites.
- Pendientes: evaluación con teclado físico, tamaños extremos y ejecutable de distribución. No se atribuye intención histórica a preferencias guardadas sin evidencia.
- Informe: Documentation/El_Despacho_Startup_and_Accessibility_Fix_v0.9.2-prototipo.pdf.

## v0.9.1-prototipo — Centro de accesibilidad (2026-11-13)

- Rediseño de la portada con cuatro perfiles combinables y cinco categorías explicadas; tarjetas de pergamino, iconos, alto contraste y desplazamiento visible.
- Ajustes con descripciones y estados claros, seis barras de volumen, disponibilidad real de voz y controles de música/ambiente desactivados por falta de pistas.
- Vistas previas de texto/contraste, indicadores, documento de demostración y avisos visuales, aisladas de la campaña.
- Preferencias persistentes y estado recomendado/personalizado; confirmación de restauración. Sin cambios en narrativa, motores de juego, tutorial o esquema de partidas.
- Validación: 563 comprobaciones de regresión de accesibilidad reejecutadas, 57 nuevas de UX, reinicio en otro proceso, 54 del tutorial, 58 de menús y 21 de importación/diseño; regresión de documentos ejecutada desde la suite de menús.
- Revisión visual real en Play Mode de portada, categorías, desplazamiento y arrastre de volumen. Pendientes teclado físico, escucha humana y evaluación con usuarios. Voz detectada: Microsoft Helena Desktop (es-ES).
- Informe: Documentation/El_Despacho_Accesibilidad_UX_Redesign_v0.9.1-prototipo.pdf.

# El Despacho - Registro de cambios

## v0.9-prototipo - 2026-11-13 - Accesibilidad

- Sección Accesibilidad dentro de Ajustes, organizada en Visual, Motor, Cognitiva, Audio y General; preferencias independientes en accessibility.json, aplicación inmediata y restauración confirmada.
- Vista accesible de todo el juego con texto de tamaño fijo, filas ajustadas y scroll. Texto 75/100/125/150/175 %, escala 80/90/100/110/120 %, contraste, foco y áreas amplias. Varias ayudas activan esta presentación alternativa; no modifica los assets originales.
- Selección/colocación simplificada de expedientes y selección/confirmación de sellos, con cancelación y callbacks originales. Navegación propia mediante Input System, referencia de controles y repetir/detener configurables F6-F8.
- Indicadores con símbolos, valores y estado; etiquetas semánticas del mapa, glosario y versiones simplificadas revisadas de las 18 notas del tutorial. Se conservan las 12 lecciones y el progreso v0.8.
- Narración local Windows mediante System.Speech y proceso sin ventana: volumen, velocidad, repetir, detener y cancelación de voz anterior. Mono y filtro suave para fuentes Unity; avisos visuales opcionales. Música/ambiente preparados para fuentes futuras; no hay pistas actuales.
- Conservados formato de campaña, cartas, países, perfiles, reglas, equilibrio, efectos, cadenas y finales. Backup previo verificado de 193 archivos.
- Validación de accesibilidad: 563 aserciones Play Mode, incluidas las 25 combinaciones de tamaño/escala; pruebas de teclado por eventos y voz con confirmación de recepción. Persistencia probada en otro proceso Unity. Regresiones del tutorial y de menús/guardado/documentos descritas en el informe.
- Revisión visual de lectura, símbolos y selección/apertura de documento. Pendiente recorrido físico completo de teclado/ratón, evaluación auditiva española, participantes y ejecutable final. Este PC solo expuso Microsoft Zira en-US. La voz externa no pasa por mono/filtro Unity. Sin soporte certificado de lector de pantalla ni reasignación completa.
- Incidencia de prueba: aserción del InputManager al salir del fixture visual; añadida restauración de sus ajustes temporales, pendiente confirmar cierre manual. No declarar accesibilidad completa ni aceptación del torneo solo por las pruebas automatizadas.
- Documentación: informe PDF, checklist manual e instrucciones de integración v0.9 en Documentation.


## v0.8-prototipo - 2026-11-06 - Tutorial Presidencial

- Ampliación del tutorial existente a 12 lecciones / 18 notas de Julián Paz, con activación por pantalla, pestaña y acciones reales. Guía de expedientes, indicadores, diplomacia, cadenas, cierre anual y guardado.
- Estados por campaña: pendiente, en curso, completado y omitido. Saltar una lección no completa la guía. Desactivar/reactivar reconoce acciones ya realizadas.
- Progreso opcional en SaveData schema 1, compatible con datos anteriores y compartido por los tres slots manuales y el autoguardado.
- Índice de lecciones en Cómo jugar desde principal y pausa; consulta con Atrás/Siguiente sin modificar campaña, preguntas ni progreso.
- Notas de pergamino, resaltados no bloqueantes y transiciones compatibles con Reducir movimiento. Primera decisión diplomática pendiente muestra explicación: confirmar/omitir y volver a elegir antes de ejecutar consecuencias.
- Conservados assets originales del tutorial, arte v0.7.2, contenido, reglas, balance, perfiles y lógica de efectos. Backup previo verificado; detalles y archivos en el informe técnico PDF de Documentation y Tutorial/INTEGRATION.md.
- Validación Unity 6000.3.24f1: compilación; 54 comprobaciones Play Mode de tutorial, 58 de menús/guardado, 21 de skin y 32 de documentos, todas PASS. Sin errores ni advertencias de ejecución en esos recorridos.
- Revisión visual de bienvenida, indicadores, expediente, pausa e índice/consulta. Pendiente aceptación humana del arrastre con guía activa: la herramienta de escritorio no completó el arrastre, aunque eventos uGUI y raycast pasan. No se realizó una campaña manual completa ni build distribuible.


## v0.7.2-prototipo - 2026-11-06 - Pixel art del pack «GoodbyeGovernor_Arte»

### Implementado
- Iconos de mapa del pack (tu país, visitado, sin visitar, amenaza, aliado) en los países y en la leyenda, en lugar de los círculos con letra.
- Retrato de Korovia en su visita. El retrato «Valdoria» se asigna PROVISIONALMENTE a Selvana (Valdoria no existe en el juego): decidir con el equipo.
- Caja de diálogo del pack para la conversación de las visitas, con el nombre del líder en su pestaña.
- Botones de opción del pack para las preguntas (rojo con flecha al pasar el ratón; atenuado si ya se preguntó).
- Dossier del pack como portada del expediente cerrado en la pila.
- Si falta cualquier imagen, el juego vuelve al aspecto anterior generado por código.

### Archivos
- Nuevos: Assets/Scripts/ElDespacho/Art.cs; Assets/Editor/ElDespachoArtImporter.cs; Assets/Resources/Art/ (11 PNG + LEEME_Arte.md), con sus .meta.
- Modificado: Assets/Scripts/ElDespacho/DespachoGame.cs (portada del expediente, nodos y leyenda del mapa, retrato, caja de diálogo y botones de pregunta).
- Sin cambios en menús v0.7/v0.7.1, guardado, tutorial, balance, cartas ni países. Los nombres de objetos que usa el tutorial (DeskDocument, ClosedCover, CardArea...) se conservan.

### Pruebas
- Pendiente: compilar y revisar en Unity (el editor estaba cerrado al integrar). Comprobar mapa, una visita a Korovia y a Selvana, y el expediente en la pila.

## v0.7.1-prototipo - 2026-11-03 - Art pass / UI polish de menús

- Nuevos fondos del pack suministrado (1672 x 941), proporción conservada mediante EnvelopeParent y recorte del excedente. Se mantienen rutas y GUID de los fondos.
- Cuatro estados gráficos de botón con SpriteSwap y 9-slicing, texto oscuro y márgenes interiores. Vista de sprite en ejecución para compensar márgenes transparentes sin alterar los PNG. Estilo común para principal, ajustes, pausa, carga/guardado, confirmaciones, créditos y ayuda.
- DeskMenuSkin.cs e importador limitado a seis texturas; cambios de presentación en DeskMenus.cs. Sin cambios en lógica de juego, guardado, autoguardado, preferencias o contenido.
- Backup previo: C:/Users/User/Documents/Codex/work/menus-v071/GoodbyeGovernor_antes_v071_2026-11-03.zip.
- Validación final Unity 6000.3.24f1: compilación correcta; 21 PASS visual/importación + 58 PASS menús/guardado + 32 PASS documentos en Play Mode automatizado; sin errores ni advertencias de ejecución. Revisión visual real Full HD 16:9 de principal, ajustes Audio/Pantalla, nueva partida y pausa mediante botón.
- Pendiente: recorrido humano completo, ESC físico, otras proporciones, escalas de texto extremas y build Windows. Los estados del pack tienen siluetas distintas; monitorizar memoria de texturas sin compresión.
- Informe e instrucciones: Documentation/El_Despacho_Menu_Visual_Update_v0.7.1-prototipo.pdf. Evidencias en Documentation/Validacion_v0.7.1/.

## v0.7-prototipo - 2026-11-03 - Menús, pausa y guardado

### Implementado
- Menú principal con fondo del despacho aportado por el equipo: Continuar, Nueva partida, Cargar partida, Ajustes, Cómo jugar, Créditos y Salir; versión y resumen de guardado.
- Ajustes por categorías con fondo de lámpara verde. Volumen maestro, efectos y UI; pantalla completa/resolución (ejecutable), VSync, FPS y brillo; tutorial/autoguardado; restablecer; reducir movimiento, contraste y tamaño de texto de menús. Persistencia JSON.
- Pausa por ESC y botón por encima del tutorial. Bloquea interacción y animaciones de documentos/sellos, con cancelación segura de arrastres y restauración de la escala temporal.
- Tres slots manuales y autoguardado independiente. Escritura atómica, copia anterior .bak, versión/esquema y comprobación SHA-256; slots corruptos desactivados.
- Guardado de cartas, flags, cadenas, sellos, resultados, preguntas, relaciones, eventos pendientes, efectos por turnos, noticias y secuencia aleatoria. Sin duplicar consecuencias al cargar.
- Confirmación antes de cargar en partida, sustituir guardados, volver al menú o salir; opciones Guardar y volver/Guardar y salir (slot 1).

### Integración y preservación
- Nuevos: DeskMenus.cs, DeskPreferences.cs, DeskSaveStore.cs, DespachoGame.Persistence.cs; dos fondos en Assets/Resources/Menus; MenuValidationProbe.cs.
- Modificados: DespachoGame.cs (partial e integración), DeskDocument.cs, StampTool.cs, Kit.cs, TutorialManager.cs y arranque de DeskValidationProbe.cs. Incluir sus .meta.
- Conservados los sellos v0.4, tutorial v0.5 y balance v0.6. Cards, CountryDatabase, Model, Balance, escenas, Packages y ProjectSettings sin cambios de esta entrega (verificación SHA-256).
- Respaldo previo: C:/Users/User/Documents/Codex/work/menus-v07/GoodbyeGovernor_antes_v07_2026-11-03.zip. No se trabajó sobre un proyecto ZIP ni se restableció Git.

### Pruebas ejecutadas
- Compilación real en Unity 6000.3.24f1 sin errores de scripts.
- Play Mode automático: 58 PASS de menús/guardado y 32 PASS de regresión de documentos. Último ensayo sin errores ni advertencias de ejecución.
- Fondos importados; navegación, slots vacíos/Continuar, pausa en arrastre/apertura, ESC vía Input System, preferencias recargadas desde disco, carga de resultados/cadenas/visitas/efectos, corrupción, copia .bak, temporizador de autoguardado, secuencia aleatoria y estados preparados de final/derrota.
- Revisión visual del Editor y navegación por clics: fondos, principal, ajustes, introducción y tutorial. Las pruebas automáticas de eventos no se presentan como recorrido completo con ratón físico.

### Limitaciones y pendientes
- Las pantallas de consecuencias y cambio de año cargan en el despacho con efectos ya aplicados; el final/derrota sí se conserva. No se guarda la fase intermedia de una animación ni el paso del tutorial.
- Música ausente, idioma español y texto instantáneo: no se añaden controles falsos. Texto ampliado y alto contraste afectan solo a los nuevos menús. Sin remapeo ni confirmación adicional de sellos.
- Pendientes: ejecutable Windows (resolución/fullscreen/reversión), evaluación auditiva, recorrido físico completo, otras pantallas y cuatro años con todos los finales.
- Fondos de baja resolución y créditos individuales/atribución final por completar con el equipo.

### Documentación y reproducción
- Documentation/El_Despacho_Menu_Principal_y_Pausa_v0.7-prototipo.pdf.
- Documentation/Pruebas_Menus_v0.7.txt y Verificacion_Archivos_v0.7.json.
- Con el Editor cerrado: Unity -batchmode -nographics -projectPath RUTA -executeMethod MenuValidationRunner.Run -logFile LOG. Usa archivos temporales separados de las partidas y cierra el Editor al terminar.
- Futuras modificaciones: nueva entrada de CHANGELOG e informe técnico PDF versionado.

## v0.6-prototipo - 2026-11-03 - Más dificultad

### Motivo
En las pruebas casi nadie perdía y el golpe de Estado solo llegaba tras el año 4. Una simulación de 4.000 partidas por tipo de jugador lo confirmó: un jugador razonable perdía ~2 % de las partidas y uno perfecto nunca bajaba de ~47 en ningún indicador.

### Cambios (todos los números en Assets/Scripts/ElDespacho/Balance.cs)
- Valores iniciales: 50 / 45 / 45 (antes 60 / 55 / 50).
- Desgaste del cargo al empezar los años 2, 3 y 4: Estabilidad -3, Seguridad -4, Prosperidad -3. Se muestra en la pantalla de cambio de año y en la gaceta.
- Los efectos NEGATIVOS de las cartas se multiplican por año: x1 / x1,2 / x1,4 / x1,6. La pista al pasar el ratón por un sello usa ya el valor real.
- Diplomacia, eventos retrasados y finales sin cambios.

### Resultado simulado (4.000 partidas por perfil)
- Jugador «humano» (80 % de aciertos en diplomacia, 70 % en cartas): pierde 23 % (antes 2 %); derrotas en los años 3 y 4.
- Jugador al azar: pierde 98 % (antes 74 %); derrotas desde el año 2.
- Jugador perfecto: sigue ganando, con algún indicador por debajo de 30.
- La Seguridad casi nunca causa la derrota: las cartas apenas la tocan. Pendiente de ajustar contenido.

### Pruebas
- Compilación sin errores en Unity 6000.3.24f1. Falta jugar partidas completas para validar la sensación.

### Documentación
- Documentacion/El_Despacho_Informe_Avances_v0.6.pdf: tutorial (v0.5) y dificultad (v0.6), archivos, integración y pruebas.
- Documentacion/El_Despacho_Analisis_Arquitectura.pdf: análisis y hoja de ruta para cartas en CSV, tags de trama, menú de pausa y guardado (pendiente de implementar).

## v0.5-prototipo - 2026-11-03 - Tutorial paso a paso

### Implementado
- Tutorial de 6 pasos en la primera partida: ASUMIR EL CARGO, coger el expediente, llevarlo a la zona de lectura, sellar, CONTINUAR y despedida.
- Overlay en un Canvas propio (sortingOrder 200), generado por código: oscurece la pantalla con 4 paneles alrededor de un foco con marco dorado que late; fuera del foco no se puede hacer clic.
- El foco busca el objeto de la interfaz por nombre (targetName) y lo sigue aunque la pantalla se redibuje; spotlightAnchor queda como alternativa.
- Botón SALTAR TUTORIAL en todos los pasos. Al terminar se guarda en PlayerPrefs (tutorial_done_v1) y no vuelve a salir.
- Pasos editables como assets (TutorialStepData) en Assets/Resources/Tutorial/; si no hay assets, se usan los pasos por defecto del código.
- Menú Tools > El Despacho > Tutorial: crear los assets de los pasos y volver a mostrar el tutorial.

### Archivos
- Nuevos: Assets/Scripts/ElDespacho/Tutorial/TutorialStepData.cs, TutorialManager.cs, INTEGRATION.md (+ .meta).
- Nuevos: Assets/Resources/Tutorial/Step_00_Welcome ... Step_05_Done (.asset + .meta).
- Modificado: DespachoGame.cs: 4 llamadas a TutorialManager.Instance?.TryAdvance(...) y el botón ASUMIR EL CARGO se llama AssumeButton.
- Modificado: DeskDocument.cs: evento estático Grabbed al coger el expediente.

### Pruebas realizadas
- Compilación sin errores ni advertencias en Unity 6000.3.24f1.
- Recorrido completo con ratón físico de los 6 pasos; al terminar el juego queda libre y en la siguiente partida no vuelve a aparecer.

## v0.4-prototipo - 2026-11-02 - Sellos estilo Papers, Please

### Implementado
- Los botones SÍ / NO del expediente se sustituyen por dos sellos sobre un soporte: APROBADO (verde) y DENEGADO (rojo).
- El jugador arrastra un sello sobre el expediente abierto; al soltarlo encima queda la marca del sello (ligeramente girada) en el papel y se aplica la decisión.
- Soltar el sello fuera del papel lo devuelve a su sitio sin decidir nada.
- Al pasar el ratón o coger un sello se muestra a qué indicadores afecta (misma pista estilo Reigns de antes).
- Cartas con etiquetas propias (NEGARLO / ADMITIRLO) usan esas palabras en el sello.
- La marca del sello se mantiene en el resultado y al volver del mapa.
- Texto de la pantalla de título actualizado.
- El botón MAPA DEL MUNDO tiembla cada 2 segundos si ya sellaste todos los expedientes del año, quedan países por visitar y aún no has abierto el mapa ese año (componente Wiggle en Kit.cs).

### Archivos
- Nuevo: Assets/Scripts/ElDespacho/StampTool.cs (+ .meta): arrastre y soltado del sello; solo presentación.
- Modificado: Assets/Scripts/ElDespacho/DespachoGame.cs: soporte de sellos, StampCard, BuildStamp, BuildStampMark y estado del sello (stampPlaced, stampPos...). AnswerCard no cambia.
- Modificado: Assets/Tests/DeskValidationProbe.cs: la comprobación de etiquetas busca los sellos en vez de botones.
- Documentación: Documentacion/El_Despacho_Informe_Avances_v0.4.pdf (avances desde la v0.2, detalle de la v0.4, cómo integrarla y pruebas).

### Pruebas realizadas
- Compilación en Unity 6000.3.24f1 sin errores ni advertencias.
- Prueba manual con ratón físico: arrastre del expediente a la zona de lectura (primera vez comprobado con ratón real), sello APROBADO (Seguridad +4), sello DENEGADO (Estabilidad +1) y sello soltado fuera del papel que vuelve a su sitio.
- Tools > El Despacho > Validate document flow: 32 PASS, 0 FAIL.

## v0.3.1-prototipo - 2026-10-28 - Integración en el proyecto local

Proyecto: C:/Users/User/GoodbyeGovernor/GoodbyeGovernor.

### Antes de editar
- Verificada ausencia de la implementación v0.3 en los cinco scripts locales.
- Copia de seguridad de 80 archivos verificada con hashes e integridad ZIP en C:/Users/User/Documents/Codex/work/el-despacho-local/GoodbyeGovernor_antes_v0.3_local_2026-10-28.zip.
- Conservados los cambios previos en ShaderGraphSettings.asset y GoodbyeGovernor.slnx. No se ha creado un commit ni reiniciado Git.

### Implementado
- Expediente cerrado sobre pila de papeles, arrastre con botón izquierdo y zona de lectura resaltada.
- Retorno al soltar fuera; centrado y apertura al soltar dentro, según posición del cursor.
- Respuestas originales disponibles solo tras abrir. Se conservan texto, retrato, cargo, pistas y etiquetas especiales.
- Resultado narrativo/numérico original y archivo animado antes del siguiente asunto.
- Estado de lectura conservado al regresar del mapa y cancelación de animaciones al cerrar pantalla.
- Corrección del resaltado sobre la superficie visible del papel.
- Bloqueo de 0,30 s tras responder para que un doble clic no salte inmediatamente el resultado.
- Sonidos existentes Sfx.Paper, sin paquetes nuevos.

### Archivos
- Modificado: Assets/Scripts/ElDespacho/DespachoGame.cs.
- Nuevos: Assets/Scripts/ElDespacho/DeskDocument.cs y .meta.
- Prueba nueva: Assets/Tests/DeskValidationProbe.cs y metadatos; protegida por UNITY_EDITOR.
- Documentación: CHANGELOG.md, Documentacion/El_Despacho_Informe_Cartas_Diegeticas_Local_v0.3.1.pdf y Documentacion/El_Despacho_Pruebas_PlayMode_v0.3.1.txt.
- Unity regeneró Assembly-CSharp.csproj; no se editaron sus referencias manualmente.

### Conservado
Cards.cs, CountryDatabase.cs, Model.cs, Kit.cs, contenido, balance, flags, cadenas, diplomacia, mapa, años, finales, escenas y Packages. El cambio previo del usuario en ShaderGraphSettings.asset permanece intacto.

### Pruebas realizadas
- Compilación/importación real en Unity 6000.3.24f1 y entrada en Play Mode.
- 32 comprobaciones automáticas PASS y 0 FAIL en Play: recepción por GraphicRaycaster; seguimiento del cursor; retorno; resaltado; apertura; bloqueo de respuesta cerrada; tres efectos una sola vez; bloqueo de doble archivo y del salto inmediato del resultado; huelga -> huelga_general; varias decisiones; etiquetas personalizadas; acceso al mapa y persistencia de lectura.
- Consola inspeccionada después de la última prueba: 0 errores, 0 advertencias.
- Inspección visual de pila/zona/documento en Game Full HD con zoom reducido. Comprobada una respuesta personalizada por clic y Estabilidad 60 -> 50.

### Pendiente / límites
- Los eventos de arrastre se validaron mediante ExecuteEvents dentro de Unity. Los intentos de arrastre con la herramienta de control de ratón no produjeron PointerDown; no equivalen a una prueba completa con ratón físico. Ese ensayo sigue pendiente.
- Pendientes: 1280x720, pérdida de foco, arrastres rápidos, visitas completas, cuatro años, todos los finales y build de jugador.
- Se conservan los comportamientos previos de derrota diferida y penalizaciones diplomáticas. No se declara una versión estable.

### Repetir e integrar
Abrir SampleScene, entrar en Play y ejecutar Tools > El Despacho > Validate document flow (resets Play session). Reinicia la partida solo en memoria; no guarda esos estados. Ver Logs/DeskDocument-validation.txt. Debe producir 32 PASS. Salir de Play al terminar.

Para compañeros: fusionar DespachoGame.cs, añadir DeskDocument.cs con su .meta y conservar Packages/escenas/ajustes. La prueba es opcional para producción; incluirla permite repetir la validación en el editor. Leer el PDF antes de integrar. No copiar Library, Temp ni la copia de seguridad sobre trabajo posterior.
