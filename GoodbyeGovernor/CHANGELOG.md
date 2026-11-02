# El Despacho - Registro de cambios

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
