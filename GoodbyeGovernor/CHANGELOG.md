# El Despacho - Registro de cambios

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
