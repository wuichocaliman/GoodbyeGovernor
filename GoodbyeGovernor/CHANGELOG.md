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
