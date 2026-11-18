# El Despacho — Controles con mando y vibración (v0.10.5 · actualizado v0.10.6)

Esquema **híbrido**: navegación nativa con cruceta / stick en menús, mapa y visitas; gatillos analógicos en la mesa; LB / RB como respaldo para mandos con gatillos digitales. El ratón sigue funcionando igual y se puede alternar en cualquier momento.

Vibración: activada por defecto. Se cambia en **Ajustes › Controles** con el botón **VIBRACIÓN: SÍ / NO** (guardado en PlayerPrefs, clave `eldespacho_vibracion`) y la intensidad (100 % / 70 % / 40 %, en settings.json). El menú de Ajustes es el mismo desde el título y desde la pausa. Al ponerlo en NO, los motores se paran en el acto y no vuelve a vibrar nada.
Motores: **grave** = motor de baja frecuencia (izquierdo), **fino** = motor de alta frecuencia (derecho). Valores de 0 a 1.

## Tabla de controles

| Acción | Mando (Xbox / Steam Deck) | Contexto | Vibración |
| --- | --- | --- | --- |
| Moverse entre botones | Cruceta o stick izquierdo | Menús, título, mapa, visitas, mesa, La Gaceta, nota del tutorial | — |
| Pulsar el botón con foco | A | Cualquier pantalla | Según el sonido del botón: golpe (grave 0,9 + fino 0,5, 55 ms + eco) en acciones fuertes como ASUMIR o cerrar el año; papel (fino 0,12, 40 ms) al abrir el mapa |
| Volver / cerrar | B | Menús y ajustes (como ESC); cierra La Gaceta; cancela una selección en modo de clics simplificados | — |
| Pausa / volver | Start (Menú) | En partida abre la pausa; con un menú abierto, vuelve (igual que ESC) | Se para toda vibración mientras dura la pausa |
| Sacar el expediente de la bandeja | R2 (analógico) o mantener RB | Despacho, con un expediente en la bandeja | Papel al cogerlo (fino 0,12, 40 ms) + tensión progresiva mientras se aprieta (fino 0,04 → 0,26) |
| Abrir el expediente | R2 a fondo (≥ 95 %) o RB mantenido 0,35 s | Mientras se está sacando | Encaje: golpe corto (grave 0,55 + fino 0,25, 70 ms) |
| Devolver el expediente a la bandeja | Soltar R2 / RB antes del fondo | Mientras se está sacando | Toque suave (fino 0,08, 30 ms) |
| Bajar el sello DENEGADO | L2 (analógico) o mantener LB | Expediente abierto y sin decidir | Tensión progresiva (fino 0,04 → 0,26) |
| Bajar el sello APROBADO | R2 (analógico) o mantener RB | Expediente abierto y sin decidir | Tensión progresiva (fino 0,04 → 0,26) |
| Estampar el sello (decidir) | L2 / R2 a fondo (≥ 95 %) o LB / RB mantenido 0,35 s | Mientras el sello baja | Golpe seco (grave 0,9 + fino 0,5, 55 ms) y eco grave (0,2, 40 ms) |
| Cancelar el sello | Soltar el gatillo / botón antes del fondo | Mientras el sello baja | Toque suave (fino 0,08, 30 ms) |
| Archivar el expediente | A sobre CONTINUAR (recibe el foco solo) | Tras sellar | Papel (fino 0,12, 40 ms) |
| Desplazar texto largo | Stick derecho arriba / abajo | Nota del asesor, La Gaceta, diálogo de las visitas, páginas de ajustes | — |
| Abrir La Gaceta | A sobre el periódico de la mesa | Despacho | Papel (fino 0,12, 40 ms) |
| Siguiente nota del tutorial | A sobre SIGUIENTE (recibe el foco al aparecer una nota nueva) | Tutorial | — |
| Aliarse / cortar lazos | A sobre la decisión (nunca recibe el foco sola) | Visitas | Golpe (grave 0,9 + fino 0,5) o, si hay ataque, alerta grave |
| Alerta crítica | — (automático) | Consecuencias graves (ataque) | Pulso grave doble (grave 0,75, 180 ms · pausa 120 ms · 180 ms) |
| Activar / desactivar vibración | A sobre «VIBRACIÓN: SÍ / NO» | Ajustes › Controles (desde el título o la pausa) | Al activarla, un toque de papel (fino 0,12, 40 ms); al desactivarla, motores a cero en el acto |
| Modo teclado de Accesibilidad | Cruceta / stick arriba-abajo = Tab; A = Intro | Si «Navegación por teclado» está activada o hay una página de Accesibilidad abierta | — |

## Reglas de seguridad
- **Nada se decide de golpe.** Con un gatillo digital (0 → 1), el sello o el expediente tardan 0,3 s en llegar al fondo; soltar antes cancela.
- **Anti-repetición.** Tras abrir o sellar hay que soltar los dos gatillos y LB / RB por debajo del 15 % antes de la siguiente acción. El R2 que abre el expediente no puede sellar APROBADO sin querer.
- **Los dos gatillos a la vez** no eligen ningún sello.
- **Foco por defecto:** al cambiar de pantalla se elige CONTINUAR, VOLVER, ENTRAR, ASUMIR… o el último botón usado en esa pantalla; **nunca** una decisión irreversible (alianza, cortar lazos, atacar, salir, borrar, nueva partida, restablecer).
- **Motores siempre a cero** al cambiar de pantalla, abrir la pausa, perder el foco de la ventana, minimizar, mover el ratón o pulsar el teclado, desconectar el mando, desactivar el juego y salir. Además, cada pulso dura como máximo 1 s y la tensión se apaga sola si no se renueva cada frame.
- **Ratón:** en cuanto se mueve, el mando deja de mandar: se cancela su acción y desaparece el marco de foco, así que con ratón el juego se ve como antes.

## Pruebas
`Tools › El Despacho › Validate gamepad (virtual pad, temp saves)` (en Play mode): 27 comprobaciones con un mando virtual (incluido el interruptor de vibración y su guardado en PlayerPrefs), en carpetas temporales. Resultado en `Logs/Gamepad-validation.txt`. La prueba anterior del flujo con ratón (`Validate document flow`) sigue pasando (32 de 32).
No verificado: la sensación real de la vibración en un mando físico ni el perfil de Steam Input de la Steam Deck.
