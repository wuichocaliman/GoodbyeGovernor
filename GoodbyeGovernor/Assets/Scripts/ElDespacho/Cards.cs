using System.Collections.Generic;
using UnityEngine;

namespace ElDespacho
{
    /// <summary>
    /// Una carta al estilo Reigns: alguien te plantea un problema y respondes SÍ o NO.
    /// </summary>
    public class Card
    {
        public string id;
        public string who;          // Quién te lo pide
        public string role;         // Cargo
        public Color color;         // Color del retrato
        public string text;         // El problema
        public string yesLabel = "SÍ", noLabel = "NO";
        public Effect yes, no;
        public string yesResult, noResult;

        public int minYear = 1, maxYear = 4;
        public string requires;      // Solo aparece si existe esta marca (flag)
        public bool followUpOnly;    // No sale al azar: solo cuando otra carta la encadena
        public string yesFlag, noFlag;   // Marcas que deja cada respuesta
        public string yesNext, noNext;   // Carta que aparece justo después
    }

    public static class CardDatabase
    {
        static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        // Personajes recurrentes
        static readonly Color Economia = C("#3d7a6a");
        static readonly Color Defensa = C("#5a6a3a");
        static readonly Color Pueblo = C("#a0603a");
        static readonly Color Espias = C("#3a3a4a");
        static readonly Color Prensa = C("#7a3a5a");
        static readonly Color Alcaldia = C("#4a6aa0");
        static readonly Color Asesor = C("#8a7040");
        static readonly Color Sanidad = C("#4aa0a0");
        static readonly Color Interior = C("#6a4a3a");
        static readonly Color Oposicion = C("#a03a3a");

        public static List<Card> Create()
        {
            return new List<Card>
            {
                // ───────── Economía ─────────
                new Card {
                    id = "impuestos", who = "Elena Cortés", role = "Ministra de Economía", color = Economia, maxYear = 2,
                    text = "Las arcas del Estado están casi vacías. ¿Subimos los impuestos a las grandes empresas?",
                    yes = new Effect(-3, 0, 6), yesResult = "Las empresas protestan, pero el Estado vuelve a respirar.",
                    no = new Effect(0, 0, -4), noResult = "Las cuentas siguen en rojo. Cortés suspira.",
                },
                new Card {
                    id = "fabrica", who = "Elena Cortés", role = "Ministra de Economía", color = Economia, maxYear = 3,
                    text = "Una multinacional quiere construir una fábrica enorme aquí. A cambio, pide no pagar impuestos durante diez años.",
                    yes = new Effect(-2, 0, 7), yesResult = "Miles de empleos nuevos. Algunos dicen que les has regalado el país.",
                    no = new Effect(1, 0, -2), noResult = "La fábrica se va al país vecino.",
                },
                new Card {
                    id = "nuclear", who = "Elena Cortés", role = "Ministra de Economía", color = Economia, minYear = 2,
                    text = "Podemos construir una central nuclear: energía barata durante décadas. Pero a mucha gente le da miedo.",
                    yes = new Effect(-5, 0, 7), yesResult = "Empiezan las obras entre protestas ecologistas.",
                    no = new Effect(1, 0, -3), noResult = "Seguimos importando energía cara.",
                },
                new Card {
                    id = "rescate_banco", who = "Elena Cortés", role = "Ministra de Economía", color = Economia, minYear = 4,
                    text = "La crisis global ha tumbado al Banco de Aurelia. Si quiebra, miles perderán sus ahorros. ¿Lo rescatamos con dinero público?",
                    yes = new Effect(-6, 0, 4), yesResult = "El banco se salva. La gente pregunta por qué a los banqueros sí y a ellos no.",
                    no = new Effect(-3, 0, -10), noResult = "El banco quiebra. Hay colas en los cajeros de todo el país.",
                },

                // ───────── Pueblo y sindicatos ─────────
                new Card {
                    id = "huelga", who = "Tomás Ruiz", role = "Líder del sindicato minero", color = Pueblo, maxYear = 3,
                    text = "Los mineros llevamos tres semanas en huelga. Pedimos un 10 % más de sueldo. ¿Nos lo das?",
                    yes = new Effect(6, 0, -5), yesResult = "Los mineros vuelven al trabajo cantando.",
                    no = new Effect(-5, 0, 0), noResult = "Ruiz sale del despacho dando un portazo.",
                    noNext = "huelga_general",
                },
                new Card {
                    id = "huelga_general", who = "General Ramos", role = "Jefe del Ejército", color = Defensa, followUpOnly = true,
                    text = "La huelga se ha extendido a todo el país. Propongo sacar al ejército a la calle para mantener el orden.",
                    yes = new Effect(-10, 3, 0), yesResult = "Las imágenes de soldados frente a mineros dan la vuelta al mundo.",
                    no = new Effect(-2, 0, -6), noResult = "Al final negocias, pero el país ha estado parado una semana.",
                },
                new Card {
                    id = "pan", who = "Rosa Medina", role = "Portavoz de los consumidores", color = Pueblo, minYear = 3,
                    text = "La sequía ha disparado el precio del pan. Las familias no llegan a fin de mes. ¿Subvencionáis el pan?",
                    yes = new Effect(5, 0, -6), yesResult = "El pan vuelve a un precio normal. El déficit, no.",
                    no = new Effect(-7, 0, 1), noResult = "Hay protestas frente a las panaderías.",
                },
                new Card {
                    id = "festival", who = "Nuria Prat", role = "Alcaldesa de la capital", color = Alcaldia,
                    text = "Se cumplen 200 años de Aurelia. ¿Organizamos un gran festival nacional?",
                    yes = new Effect(5, 0, -3), yesResult = "Fuegos artificiales, música y orgullo nacional.",
                    no = new Effect(-2, 0, 0), noResult = "El aniversario pasa sin pena ni gloria.",
                },
                new Card {
                    id = "inundacion", who = "Nuria Prat", role = "Alcaldesa de la capital", color = Alcaldia,
                    text = "¡El río se ha desbordado! Miles de familias se han quedado sin casa. ¿Movilizamos al ejército para ayudar?",
                    yes = new Effect(6, -2, -3), yesResult = "Los soldados reparten mantas y comida. La gente no lo olvidará.",
                    no = new Effect(-7, 0, 0), noResult = "Las familias se sienten abandonadas.",
                },
                new Card {
                    id = "autopista", who = "Nuria Prat", role = "Alcaldesa de la capital", color = Alcaldia, maxYear = 3,
                    text = "Queremos una autopista hasta la frontera: más comercio. Pero atraviesa un bosque protegido.",
                    yes = new Effect(-3, 0, 6), yesResult = "Las excavadoras entran en el bosque.",
                    no = new Effect(2, 0, -2), noResult = "El bosque se salva. Los camioneros siguen atascados.",
                },

                // ───────── Defensa ─────────
                new Card {
                    id = "tanques", who = "General Ramos", role = "Jefe del Ejército", color = Defensa,
                    text = "Nuestros tanques tienen cuarenta años. Necesito presupuesto para modernizar el ejército.",
                    yes = new Effect(0, 7, -5), yesResult = "Llegan los primeros tanques nuevos. Ramos sonríe por primera vez.",
                    no = new Effect(0, -4, 0), noResult = "«Luego no diga que no le avisé», gruñe Ramos.",
                },
                new Card {
                    id = "mili", who = "General Ramos", role = "Jefe del Ejército", color = Defensa, minYear = 3,
                    text = "Con la tensión en la región, propongo el servicio militar obligatorio para todos los jóvenes.",
                    yes = new Effect(-7, 8, 0), yesResult = "Los cuarteles se llenan. Los jóvenes, no tan contentos.",
                    no = new Effect(2, -3, 0), noResult = "Ramos se lleva el informe bajo el brazo, decepcionado.",
                },
                new Card {
                    id = "toque_queda", who = "General Ramos", role = "Jefe del Ejército", color = Defensa, minYear = 4,
                    text = "La crisis ha provocado disturbios en la capital. ¿Decretamos toque de queda?",
                    yes = new Effect(-6, 5, 0), yesResult = "Las calles se vacían por la noche. El miedo, no.",
                    no = new Effect(1, -5, 0), noResult = "Los disturbios continúan varias noches.",
                },

                // ───────── Espionaje ─────────
                new Card {
                    id = "pinchazos", who = "«Búho»", role = "Jefe de los servicios secretos", color = Espias,
                    text = "Podría pinchar los teléfonos de los líderes de la oposición. Solo por seguridad nacional, claro.",
                    yes = new Effect(0, 4, 0), yesResult = "Búho asiente. Nadie sabrá nada... de momento.", yesFlag = "espionaje",
                    no = new Effect(0, -2, 0), noResult = "Búho se encoge de hombros y desaparece entre las sombras.",
                },
                new Card {
                    id = "filtracion", who = "Clara Montes", role = "Periodista de investigación", color = Prensa, requires = "espionaje", minYear = 2,
                    text = "Tengo grabaciones que prueban que espiabas a la oposición. Mañana se publican. ¿Vas a negarlo?",
                    yesLabel = "NEGARLO", noLabel = "ADMITIRLO",
                    yes = new Effect(-10, 0, 0), yesResult = "Publican las grabaciones. Nadie cree tus excusas.",
                    no = new Effect(-5, -2, 0), noResult = "Pides perdón y cesas a Búho. Duele, pero menos.",
                },
                new Card {
                    id = "hackers", who = "«Búho»", role = "Jefe de los servicios secretos", color = Espias, minYear = 2,
                    text = "Hackers extranjeros han atacado nuestros bancos. Puedo devolver el golpe... pero si nos pillan, habrá problemas.",
                    yes = new Effect(-2, 5, 0), yesResult = "Los ataques cesan. Nadie sabe quién ha sido. O eso esperas.",
                    no = new Effect(0, -4, -3), noResult = "Los bancos pasan días sin funcionar bien.",
                },

                // ───────── Prensa y oposición ─────────
                new Card {
                    id = "corrupcion", who = "Clara Montes", role = "Periodista de investigación", color = Prensa, minYear = 2,
                    text = "Tengo pruebas de que tu ministro de Obras cobra comisiones. ¿Le cesas antes de que lo publique?",
                    yes = new Effect(6, 0, -2), yesResult = "Cesas al ministro en directo. La gente aplaude tu transparencia.",
                    no = new Effect(0, 0, 0), noResult = "Montes cierra su libreta. «Usted sabrá», dice.",
                    noNext = "corrupcion_2",
                },
                new Card {
                    id = "corrupcion_2", who = "Asesor Julián Paz", role = "Tu jefe de gabinete", color = Asesor, followUpOnly = true,
                    text = "El escándalo ha salido igualmente, y ahora dicen que tú lo tapabas. ¿Pides perdón públicamente?",
                    yes = new Effect(-4, 0, 0), yesResult = "Una disculpa incómoda, pero sincera.",
                    no = new Effect(-10, 0, 0), noResult = "Tu silencio se interpreta como culpa.",
                },
                new Card {
                    id = "debate", who = "Marco Bel", role = "Líder de la oposición", color = Oposicion, minYear = 2,
                    text = "Te reto a un debate televisado. Si no aceptas, le diré a todo el país que me tienes miedo.",
                    yes = new Effect(4, 0, 0), yesResult = "El debate es duro, pero sales bien parado.",
                    no = new Effect(-4, 0, 0), noResult = "Bel pasa la semana en la tele llamándote cobarde.",
                },
                new Card {
                    id = "estatua", who = "Asesor Julián Paz", role = "Tu jefe de gabinete", color = Asesor, maxYear = 2,
                    text = "Tus seguidores quieren levantar una estatua tuya en la plaza mayor. ¿Das el visto bueno?",
                    yes = new Effect(-4, 0, -2), yesResult = "La estatua queda... un poco de dictador, la verdad.",
                    no = new Effect(3, 0, 0), noResult = "La modestia gusta a la gente.",
                },
                new Card {
                    id = "vacaciones", who = "Asesor Julián Paz", role = "Tu jefe de gabinete", color = Asesor,
                    text = "Llevas meses sin dormir bien. ¿Te tomas una semana de vacaciones?",
                    yes = new Effect(-2, 0, 0), yesResult = "Titular: «El líder, en la playa mientras el país sufre».",
                    no = new Effect(1, 0, 0), noResult = "Sigues al pie del cañón. Tus ojeras salen en todas las fotos.",
                },

                // ───────── Sanidad e interior ─────────
                new Card {
                    id = "hospitales", who = "Irene Vidal", role = "Ministra de Sanidad", color = Sanidad, minYear = 2,
                    text = "Los hospitales están saturados. Necesito dinero para contratar médicos ya.",
                    yes = new Effect(6, 0, -6), yesResult = "Las listas de espera bajan por fin.",
                    no = new Effect(-5, 0, 0), noResult = "Los médicos convocan una huelga.",
                },
                new Card {
                    id = "refugiados", who = "Andrés Lobo", role = "Ministro del Interior", color = Interior, minYear = 2,
                    text = "Llegan barcos con refugiados a nuestras costas. ¿Abrimos los puertos?",
                    yes = new Effect(2, -1, -4), yesResult = "Unos te aplauden; otros protestan frente al Parlamento.",
                    no = new Effect(-4, 2, 0), noResult = "Las imágenes de los barcos rechazados duelen a mucha gente.",
                },

                // ───────── Cartas ligadas a tus decisiones diplomáticas ─────────
                new Card {
                    id = "korovia_tropas", who = "Dragan Volkov", role = "Presidente de Korovia (tu aliado)", color = C("#c4574a"),
                    requires = "ally_korovia", minYear = 3,
                    text = "Amigo mío, necesito que mis tropas crucen Aurelia de camino a Selvana. Es un simple trámite entre aliados.",
                    yes = new Effect(-8, -4, 0), yesResult = "Columnas de tanques extranjeros cruzan tus carreteras. Tu pueblo no lo entiende.",
                    no = new Effect(0, -3, -4), noResult = "Volkov cuelga el teléfono. El petróleo llega con «retrasos».",
                },
                new Card {
                    id = "zafira_censura", who = "Emir Kalil al-Rashan", role = "Emir de Zafira (tu aliado)", color = C("#c99a3a"),
                    requires = "ally_zafira",
                    text = "Un periodista de tu país me insulta cada semana. Entre amigos, ¿no podrías hacerle callar?",
                    yes = new Effect(-6, 0, 3), yesResult = "El periodista pierde su programa. La libertad de prensa, también un poco.",
                    no = new Effect(1, 0, -4), noResult = "El emir retira algunas inversiones, molesto.",
                },
                new Card {
                    id = "selvana_gracias", who = "Lucía Ferro", role = "Presidenta de Selvana", color = C("#5fae6e"),
                    requires = "cut_korovia",
                    text = "Gracias por plantar cara a Korovia. Queremos invitar a tus empresas a reconstruir nuestro país. ¿Aceptas?",
                    yes = new Effect(2, 0, 6), yesResult = "Empresas de Aurelia firman contratos en Selvana.",
                    no = new Effect(0, 0, 0), noResult = "Ferro no lo entiende, pero lo respeta.",
                },
                new Card {
                    id = "draskovar_refugiados", who = "Andrés Lobo", role = "Ministro del Interior", color = Interior,
                    requires = "cut_draskovar",
                    text = "Tras el ataque a Draskovar, miles de civiles huyen hacia nuestra frontera. ¿Les dejamos entrar?",
                    yes = new Effect(3, -2, -4), yesResult = "Montamos campamentos. Al menos ellos no tienen la culpa.",
                    no = new Effect(-5, 2, 0), noResult = "La frontera se cierra. Las imágenes son terribles.",
                },
            };
        }
    }
}
