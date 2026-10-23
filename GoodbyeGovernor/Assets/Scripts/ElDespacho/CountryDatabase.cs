using System.Collections.Generic;
using UnityEngine;

namespace ElDespacho
{
    /// <summary>
    /// Todo el contenido de la partida: los 15 países, sus perfiles ocultos,
    /// diálogos, documentos y consecuencias. Para añadir países, copia un bloque.
    /// </summary>
    public static class CountryDatabase
    {
        public const string PlayerCountry = "Aurelia";
        public static readonly Vector2 PlayerMapPos = new Vector2(0.40f, 0.48f);

        const string QRivales = "¿Qué opinas de nuestros rivales?";
        const string QRecursos = "¿Qué recursos tenéis?";
        const string QAlianza = "¿Buscáis una alianza?";
        const string QConflictos = "¿Hay conflictos en la región?";

        static List<Question> Q(string rivales, string recursos, string alianza, string conflictos, int incomoda)
        {
            var list = new List<Question>
            {
                new Question { text = QRivales, answer = rivales },
                new Question { text = QRecursos, answer = recursos },
                new Question { text = QAlianza, answer = alianza },
                new Question { text = QConflictos, answer = conflictos },
            };
            if (incomoda >= 0 && incomoda < list.Count) list[incomoda].incomoda = true;
            return list;
        }

        static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static List<Country> Create()
        {
            var l = new List<Country>();

            // ───────────────────────── AÑO 1 · Tutorial: casi todos honestos ─────────────────────────

            l.Add(new Country
            {
                id = "lindmark", name = "Lindmark", leader = "Primera Ministra Ingrid Halvorsen", year = 1,
                mapPos = new Vector2(0.28f, 0.72f), color = C("#6fa6c9"),
                profile = Profile.Honesto, correct = Correct.Alianza, rumor = Rumor.Aliado, attackable = false,
                population = "6,1 millones", government = "Democracia parlamentaria", army = "Pequeño", relation = "Cordial",
                speech = "Somos un país de pescadores y carpinteros. Queremos abrir nuestros puertos a Aurelia y vender madera y pescado a buen precio. No pedimos nada más que comercio justo.",
                questions = Q(
                    "Mirvald nos preocupa: sus barcos pescan en nuestras aguas sin permiso. Pero no buscamos pelea con nadie.",
                    "Madera, pescado y astilleros. Nada de petróleo, lo siento.",
                    "Una alianza comercial, sí. Militarmente no tenemos mucho que ofrecer, y preferimos ser sinceros.",
                    "Solo disputas de pesca con Mirvald. Nada que no se arregle hablando.",
                    -1),
                intel = "Gobierno estable, elecciones limpias. Ejército reducido y defensivo. Sin movimientos de tropas. Nuestros agentes no detectan intereses ocultos.",
                intelSource = "Agente «Gaviota», puerto de Halsund", intelDate = "Hace 2 semanas", photo = "",
                exports = "Madera, pescado, barcos", imports = "Maquinaria, vino", dependency = "Media: el 15 % de nuestra madera viene de Lindmark.",
                region = "Costa occidental. Vecino de Mirvald, con quien tiene disputas pesqueras.",
                stabilityBar = 85, riskBar = 10,
                allyEffect = new Effect(3, 0, 8), cutEffect = new Effect(-3, 0, -5),
                allyText = "Los barcos de Lindmark llegan a nuestros puertos. La gente celebra el pescado barato.",
                cutText = "Lindmark retira a su embajadora, desconcertada. Nadie entiende por qué rechazaste a un vecino pacífico.",
                rivals = new[] { "mirvald" },
            });

            l.Add(new Country
            {
                id = "pavonia", name = "Pavonia", leader = "Rey Aldric III", year = 1,
                mapPos = new Vector2(0.53f, 0.33f), color = C("#9b7bc4"),
                profile = Profile.Honesto, correct = Correct.Alianza, rumor = Rumor.Ninguno, attackable = false,
                population = "9,8 millones", government = "Monarquía constitucional", army = "Mediano", relation = "Neutral",
                speech = "Los piratas asaltan nuestras rutas en el mar del Este. Propongo patrullas conjuntas entre nuestras flotas. Os costará algo de dinero, pero vuestros barcos también estarán más seguros.",
                questions = Q(
                    "Draskovar me inquieta. Sus generales hablan demasiado de «fronteras históricas».",
                    "Trigo, lana y una flota respetable. No somos ricos, pero somos fiables.",
                    "Cooperación militar defensiva. No quiero arrastraros a ninguna guerra.",
                    "Los piratas, sobre todo. Y Draskovar, que mueve tropas cerca de nuestra frontera de vez en cuando.",
                    -1),
                intel = "La piratería en el mar del Este ha subido un 40 % este año. La flota pavonesa es disciplinada. El rey es respetado y el parlamento lo controla. Sin intenciones expansionistas.",
                intelSource = "Oficina naval de Aurelia", intelDate = "Hace 1 mes", photo = "Patrulla pavonesa escoltando un mercante",
                exports = "Trigo, lana", imports = "Herramientas, medicinas", dependency = "Baja.",
                region = "Mar del Este. Frontera tensa con Draskovar.",
                stabilityBar = 75, riskBar = 25,
                allyEffect = new Effect(0, 8, -2), cutEffect = new Effect(0, -4, 0),
                allyText = "Las flotas de Pavonia y Aurelia patrullan juntas. Los piratas se lo piensan dos veces.",
                cutText = "Pavonia patrullará sola. Nuestros mercantes navegan con más miedo.",
                rivals = new[] { "draskovar" },
            });

            l.Add(new Country
            {
                id = "tarvessa", name = "Tarvessa", leader = "General Osmar Kell", year = 1,
                mapPos = new Vector2(0.22f, 0.38f), color = C("#b58a52"),
                profile = Profile.Honesto, correct = Correct.Alianza, rumor = Rumor.Amenaza, attackable = true,
                population = "4,3 millones", government = "Junta militar de transición", army = "Pequeño", relation = "Fría",
                speech = "Sé lo que dicen de nosotros: uniformes, un general... Pero la junta convocará elecciones el año que viene. Ahora mismo solo queremos vender nuestro grano y que nos dejen en paz.",
                questions = Q(
                    "No tenemos rivales. Tenemos bastante con reconstruir el país.",
                    "Grano. Mucho grano. Sois uno de nuestros mejores clientes y queremos que siga así.",
                    "Comercio y neutralidad. No pedimos soldados a nadie.",
                    "Hubo disturbios tras la caída del antiguo régimen. Ya están controlados.",
                    -1),
                intel = "Pese a su fama, el ejército de Tarvessa es pequeño y se dedica sobre todo a ayudar tras inundaciones. Calendario electoral publicado. La «amenaza» viene de rumores de la época del antiguo régimen.",
                intelSource = "Agente «Espiga», capital de Tarvessa", intelDate = "Hace 1 semana", photo = "Soldados repartiendo sacos de grano",
                exports = "Grano, patatas", imports = "Fertilizantes, maquinaria agrícola", dependency = "ALTA: el 30 % de nuestro pan se hace con trigo de Tarvessa.",
                region = "Llanuras del noroeste. Sin conflictos activos con vecinos.",
                stabilityBar = 55, riskBar = 20,
                allyEffect = new Effect(-2, 0, 6), cutEffect = new Effect(-2, 2, -4),
                allyText = "El grano de Tarvessa sigue llegando. Algunos periódicos critican pactar con un general, pero el pan no sube.",
                cutText = "Cortas con Tarvessa. El grano deja de llegar y las panaderías suben precios.",
                cutPerTurn = new Effect(0, 0, -2), cutTurns = 3,
                cutLater = new DelayedEvent(1, "Tarvessa celebra elecciones limpias",
                    "Los observadores internacionales confirman que Tarvessa nunca fue una amenaza. La prensa pregunta por qué la trataste como a una.",
                    new Effect(-5, 0, 0)),
            });

            // ───────────────────────── AÑO 2 · Primeros mentirosos ─────────────────────────

            l.Add(new Country
            {
                id = "korovia", name = "Korovia", leader = "Presidente Dragan Volkov", year = 2,
                mapPos = new Vector2(0.67f, 0.62f), color = C("#c4574a"),
                profile = Profile.Mentiroso, correct = Correct.Corte, rumor = Rumor.Amenaza, attackable = true,
                population = "12,4 millones", government = "Autoritario", army = "Grande", relation = "Neutral",
                speech = "Nuestro pueblo busca estabilidad. Queremos acceso a tus mercados y cooperación militar. Creemos que juntos podemos hacer frente a las amenazas de la región.",
                questions = Q(
                    "...¿Habéis probado nuestro vino? Es el mejor del continente. Hablemos de cosas agradables.",
                    "Petróleo y hierro, en abundancia. Justo lo que Aurelia necesita, ¿verdad?",
                    "Por supuesto. Una alianza plena: comercio y ejércitos unidos.",
                    "Nuestros vecinos nos provocan, pero nosotros solo queremos paz.",
                    0),
                intel = "Aumentan su ejército. Tensiones con países vecinos (Selvana, Brennholt). Posible interés en expansión. Se han visto columnas de tanques hacia la frontera sur.",
                intelSource = "Satélite y agente «Cuervo»", intelDate = "Hace 3 días", photo = "Columna de tanques cerca de la frontera con Selvana",
                exports = "Petróleo, hierro", imports = "Tecnología, alimentos", dependency = "Podría cubrir el 40 % de nuestro petróleo.",
                region = "Centro-este. Rodeado de Selvana y Brennholt, ambos con miedo a Korovia.",
                stabilityBar = 45, riskBar = 85,
                allyEffect = new Effect(0, 2, 12), cutEffect = new Effect(0, 6, -8),
                allyText = "El petróleo de Korovia fluye hacia Aurelia. Las gasolineras bajan precios.",
                cutText = "Las bombas caen sobre los cuarteles de Korovia. Su ejército queda destrozado. Nadie celebra nada.",
                allyLater = new DelayedEvent(1, "Korovia invade Selvana y exige tu apoyo",
                    "Volkov cruza la frontera con sus tanques y recuerda en televisión que Aurelia es «su aliada». Los vecinos te ven como cómplice.",
                    new Effect(-12, -6, 0)),
                cutLater = new DelayedEvent(1, "Los vecinos de Korovia te lo agradecen",
                    "Selvana y Brennholt, libres de la amenaza, abren sus mercados a Aurelia.",
                    new Effect(2, 0, 8)),
                rivals = new[] { "selvana", "brennholt" },
            });

            l.Add(new Country
            {
                id = "selvana", name = "Selvana", leader = "Presidenta Lucía Ferro", year = 2,
                mapPos = new Vector2(0.79f, 0.74f), color = C("#5fae6e"),
                profile = Profile.Honesto, correct = Correct.Alianza, rumor = Rumor.Aliado, attackable = false,
                population = "3,2 millones", government = "Democracia presidencial", army = "Pequeño", relation = "Cordial",
                speech = "Somos pequeños y tenemos miedo. Korovia mueve tanques hacia nuestra frontera. Os pedimos amistad y, si llega el momento, que no miréis hacia otro lado.",
                questions = Q(
                    "Korovia. Volkov dice «paz» por la mañana y mueve tropas por la noche.",
                    "Café, cacao y unas minas de cobre modestas.",
                    "Sí. Os necesitamos más de lo que vosotros nos necesitáis, no voy a fingir lo contrario.",
                    "Korovia ha cerrado dos pasos fronterizos este mes. Tenemos refugiados en el sur.",
                    -1),
                intel = "Lo que cuenta la presidenta Ferro coincide con nuestros satélites: hay tropas de Korovia en su frontera. Selvana no tiene capacidad ofensiva.",
                intelSource = "Satélite", intelDate = "Hace 3 días", photo = "Campamento de refugiados en el sur de Selvana",
                exports = "Café, cacao, cobre", imports = "Medicinas, maquinaria", dependency = "Baja.",
                region = "Sureste. Frontera con Korovia, muy tensa.",
                stabilityBar = 60, riskBar = 15,
                allyEffect = new Effect(5, -2, 3), cutEffect = new Effect(-4, 0, 0),
                allyText = "Selvana entra en tu bloque. La presidenta Ferro te da las gracias en directo, emocionada.",
                cutText = "Rompes con Selvana. La pequeña república queda sola frente a Korovia.",
                rivals = new[] { "korovia" },
            });

            l.Add(new Country
            {
                id = "mirvald", name = "Mirvald", leader = "Canciller Otto Brask", year = 2,
                mapPos = new Vector2(0.12f, 0.58f), color = C("#7c8fa3"),
                profile = Profile.Mentiroso, correct = Correct.Corte, rumor = Rumor.Aliado, attackable = false,
                population = "8,7 millones", government = "República federal", army = "Mediano", relation = "Amistosa",
                speech = "¡Aurelia y Mirvald, viejos amigos! Os ofrezco un tratado de libre comercio sin aranceles. Nuestras empresas tecnológicas podrían modernizar todas vuestras oficinas públicas.",
                questions = Q(
                    "Lindmark exagera lo de la pesca. Son unos llorones.",
                    "Tecnología, telecomunicaciones, software. Lo mejor del continente.",
                    "Alianza comercial total. Instalaríamos nuestras redes en vuestros ministerios, gratis.",
                    "Ninguno. Mirvald es un remanso de paz.",
                    -1),
                intel = "Mirvald ha DUPLICADO sus aranceles a tres países con los que firmó «libre comercio». Sus empresas tecnológicas están ligadas a su servicio secreto. Dos países denunciaron espionaje en redes instaladas «gratis».",
                intelSource = "Contrainteligencia de Aurelia", intelDate = "Hace 2 meses", photo = "",
                exports = "Tecnología, software", imports = "Materias primas, energía", dependency = "Baja, pero subiría mucho si instalamos sus redes.",
                region = "Oeste. Disputas pesqueras con Lindmark.",
                stabilityBar = 80, riskBar = 45,
                allyEffect = new Effect(0, 0, 6), cutEffect = new Effect(0, 1, -3),
                allyText = "Técnicos de Mirvald instalan redes nuevas en los ministerios. Todo va más rápido.",
                cutText = "Rechazas el tratado. El canciller Brask se marcha ofendido.",
                allyLater = new DelayedEvent(1, "Escándalo: espionaje en los ministerios",
                    "Las redes de Mirvald enviaban documentos secretos a su servicio de inteligencia. Dimite medio gobierno.",
                    new Effect(-10, -6, 0)),
                cutLater = new DelayedEvent(1, "Mirvald, acusado de espionaje por sus socios",
                    "Varios países descubren que las redes de Mirvald les espiaban. Aurelia se libró.",
                    new Effect(2, 4, 0)),
                rivals = new[] { "lindmark" },
            });

            l.Add(new Country
            {
                id = "quessa", name = "Quessa", leader = "Gran Duquesa Amira Solé", year = 2,
                mapPos = new Vector2(0.50f, 0.14f), color = C("#d9b84a"),
                profile = Profile.MalInformado, correct = Correct.Alianza, rumor = Rumor.Amenaza, attackable = true,
                population = "5,5 millones", government = "Gran Ducado", army = "Mediano", relation = "Neutral",
                speech = "Me han dicho que en Aurelia creen que preparamos una guerra. Me hace gracia. Nuestro ejército desfila una vez al año y el resto del tiempo apaga incendios forestales.",
                questions = Q(
                    "¿Rivales? Nos llevamos bien con todo el mundo. Somos un país de turistas.",
                    "Turismo, aceite de oliva y sal marina.",
                    "Nos encantaría. Un acuerdo de turismo y comercio, y quizás ejercicios conjuntos contra incendios.",
                    "Ninguno que yo sepa. Mis ministros me dicen que todo está tranquilo.",
                    -1),
                intel = "MOVILIZACIÓN MILITAR: se han fotografiado miles de soldados y blindados en la capital de Quessa. Posible preparación ofensiva.",
                intelSource = "Fuente anónima (no verificada)", intelDate = "Hace 3 AÑOS", photo = "Desfile del Día Nacional de Quessa, con globos y banderitas",
                exports = "Aceite de oliva, sal, turismo", imports = "Electrónica, coches", dependency = "Media: muchos turistas de Aurelia veranean allí.",
                region = "Norte. Costa tranquila, sin disputas fronterizas conocidas.",
                stabilityBar = 80, riskBar = 70,
                allyEffect = new Effect(2, 3, 5), cutEffect = new Effect(0, 2, -3),
                allyText = "Quessa entra en tu bloque. Los vuelos de verano se llenan.",
                cutText = "Nuestros aviones atacan los cuarteles de Quessa. La Gran Duquesa habla en televisión, incrédula.",
                cutLater = new DelayedEvent(1, "Quessa era inocente",
                    "Periodistas descubren que el informe que justificó el ataque usaba fotos de un desfile de hace tres años. La indignación es enorme.",
                    new Effect(-12, 0, -4)),
            });

            // ───────────────────────── AÑO 3 · Bloques ─────────────────────────

            l.Add(new Country
            {
                id = "brennholt", name = "Brennholt", leader = "Mariscal retirada Hedda Brenn", year = 3,
                mapPos = new Vector2(0.61f, 0.85f), color = C("#8a6d4f"),
                profile = Profile.Honesto, correct = Correct.Alianza, rumor = Rumor.Ninguno, attackable = true,
                population = "7,0 millones", government = "República", army = "Grande (defensivo)", relation = "Neutral",
                speech = "Seré directa: tenemos el ejército más grande de la región porque vivimos al lado de Korovia. Os ofrezco una alianza defensiva. Si os atacan, iremos. Si atacáis vosotros, no contéis con nosotros.",
                questions = Q(
                    "Korovia. Y cualquiera que pacte con Korovia.",
                    "Acero, carbón y fábricas de vehículos.",
                    "Defensiva, sí. No firmaremos nada que nos meta en una guerra de agresión.",
                    "La frontera con Korovia es la más militarizada del continente.",
                    0),
                intel = "Ejército grande pero desplegado en posiciones defensivas. Doctrina militar puramente defensiva desde hace 30 años. La mariscal es conocida por su franqueza.",
                intelSource = "Agregado militar", intelDate = "Hace 1 semana", photo = "Búnkeres defensivos en la frontera con Korovia",
                exports = "Acero, carbón, vehículos", imports = "Alimentos, textiles", dependency = "Media.",
                region = "Sur. Frontera fortificada con Korovia. Forma bloque con Selvana.",
                stabilityBar = 70, riskBar = 40,
                allyEffect = new Effect(0, 7, 4), cutEffect = new Effect(-2, -3, -3),
                allyText = "Brennholt firma la alianza defensiva. Nuestros generales duermen más tranquilos.",
                cutText = "Rompes con Brennholt. Su ejército, el mayor de la región, ya no está de tu lado.",
                rivals = new[] { "korovia" },
            });

            l.Add(new Country
            {
                id = "zafira", name = "Zafira", leader = "Emir Kalil al-Rashan", year = 3,
                mapPos = new Vector2(0.87f, 0.42f), color = C("#c99a3a"),
                profile = Profile.Ambiguo, correct = Correct.Cualquiera, rumor = Rumor.Ninguno, attackable = false,
                population = "15,9 millones", government = "Monarquía absoluta", army = "Mediano", relation = "Neutral",
                speech = "Zafira es rica, Aurelia es próspera. Hay muchas formas de que dos países ricos se entiendan. Hablemos de oportunidades, no de política.",
                questions = Q(
                    "Todos son amigos de Zafira mientras paguen sus facturas.",
                    "Gas, petróleo, oro. Y paciencia, mucha paciencia.",
                    "Una alianza... es una palabra grande. Digamos una amistad con beneficios.",
                    "Siempre hay ruido en el mundo. Zafira prefiere no escucharlo.",
                    2),
                intel = "Datos incompletos. Dos fuentes dicen que reprime protestas internas; otra lo niega. Inversiones enormes en el extranjero. Sin movimientos militares relevantes.",
                intelSource = "Fuentes contradictorias", intelDate = "Hace 1 mes", photo = "",
                exports = "Gas, petróleo, oro", imports = "Todo lo demás", dependency = "Podría ser enorme, para bien o para mal.",
                region = "Este desértico. Tensiones con Ilvea por refugiados.",
                stabilityBar = 50, riskBar = 40,
                allyEffect = new Effect(-8, 0, 12), cutEffect = new Effect(2, 0, -6),
                allyText = "El dinero de Zafira inunda Aurelia. Las protestas por pactar con un monarca absoluto también.",
                cutText = "Rechazas al emir. Zafira lleva su dinero a otra parte. Los activistas de derechos humanos te aplauden.",
                rivals = new[] { "ilvea" },
            });

            l.Add(new Country
            {
                id = "draskovar", name = "Draskovar", leader = "Presidente vitalicio Radu Draskov", year = 3,
                mapPos = new Vector2(0.49f, 0.66f), color = C("#6b4a7a"),
                profile = Profile.Mentiroso, correct = Correct.Corte, rumor = Rumor.Ninguno, attackable = true,
                population = "10,2 millones", government = "Dictadura personalista", army = "Grande", relation = "Correcta",
                speech = "Draskovar y Aurelia compartimos frontera, historia y destino. Propongo un pacto de no agresión y desmilitarizar la frontera. Que nuestros hijos crezcan en paz.",
                questions = Q(
                    "Pavonia es una monarquía decadente. Pero no es asunto mío... por ahora.",
                    "Minerales, cemento. Mucho cemento.",
                    "Un pacto de amistad eterna. Retiraremos nuestras tropas si vosotros retiráis las vuestras primero.",
                    "Ninguno. La frontera está tranquila, os lo garantizo.",
                    0),
                intel = "Construcción masiva de búnkeres y pistas de aterrizaje a 20 km de NUESTRA frontera (el «mucho cemento»). Mapas escolares de Draskovar incluyen parte de Aurelia como «territorio histórico».",
                intelSource = "Satélite y agente «Topo»", intelDate = "Hace 5 días", photo = "Pista de aterrizaje militar en construcción junto a nuestra frontera",
                exports = "Minerales, cemento", imports = "Combustible, armas", dependency = "Baja.",
                region = "Frontera directa con Aurelia por el sur. Tensiones con Pavonia.",
                stabilityBar = 40, riskBar = 90,
                allyEffect = new Effect(0, 4, 2), cutEffect = new Effect(-3, 10, -4),
                allyText = "Firmas el pacto. Retiras tropas de la frontera sur como gesto de buena voluntad.",
                cutText = "Nuestros aviones destruyen las pistas y los búnkeres de Draskovar. La frontera sur queda en silencio.",
                allyLater = new DelayedEvent(1, "Draskovar rompe el pacto y cruza la frontera",
                    "Las tropas de Draskov avanzan por la frontera que tú mismo desmilitarizaste. Hay que movilizar al ejército a toda prisa.",
                    new Effect(-6, -16, -4)),
                rivals = new[] { "pavonia", "calidor" },
            });

            l.Add(new Country
            {
                id = "ilvea", name = "Ilvea", leader = "Primer Ministro Tomas Arenz", year = 3,
                mapPos = new Vector2(0.08f, 0.28f), color = C("#4fa3a0"),
                profile = Profile.Honesto, correct = Correct.Alianza, rumor = Rumor.Ninguno, attackable = false,
                population = "2,9 millones", government = "Democracia", army = "Muy pequeño", relation = "Neutral",
                speech = "No vengo a ofrecer negocios. Tenemos medio millón de refugiados de Zafira y nos quedamos sin recursos. Os pido ayuda. Ayudarnos os costará dinero; no os voy a engañar.",
                questions = Q(
                    "Zafira reprime a su gente y la gente huye hacia nosotros. Eso es todo lo que sé.",
                    "Pocos. Algo de pesca y lana. Lo que tenemos es gente trabajadora.",
                    "Sí, aunque sé que ahora mismo os pido más de lo que os doy.",
                    "La crisis de refugiados de Zafira. Cada semana llegan más.",
                    -1),
                intel = "La crisis de refugiados es real y está documentada por organizaciones internacionales. El gobierno de Ilvea es transparente. Cifras coincidentes con su discurso.",
                intelSource = "Organizaciones humanitarias", intelDate = "Hace 4 días", photo = "Tiendas de campaña en la frontera de Ilvea",
                exports = "Pesca, lana", imports = "Alimentos, medicinas", dependency = "Ninguna.",
                region = "Noroeste. Recibe refugiados de Zafira.",
                stabilityBar = 45, riskBar = 10,
                allyEffect = new Effect(6, 0, -5), cutEffect = new Effect(-6, 0, 2),
                allyText = "Aurelia envía ayuda a Ilvea. Cuesta dinero, pero las imágenes de los camiones llegando emocionan al país.",
                cutText = "Le das la espalda a Ilvea. Ahorras dinero; la prensa te llama «sin corazón».",
                rivals = new[] { "zafira" },
            });

            // ───────────────────────── AÑO 4 · Crisis global ─────────────────────────

            l.Add(new Country
            {
                id = "calidor", name = "Calidor", leader = "Presidente Iván Mestre", year = 4,
                mapPos = new Vector2(0.63f, 0.45f), color = C("#d07a3a"),
                profile = Profile.MalInformado, correct = Correct.Alianza, rumor = Rumor.Amenaza, attackable = true,
                population = "6,6 millones", government = "Democracia", army = "Mediano", relation = "Neutral",
                speech = "La crisis nos golpea a todos. Calidor ofrece comercio de alimentos a precio de coste. Y os pido que confiéis en Draskovar como confiamos nosotros: es un buen vecino.",
                questions = Q(
                    "¿Rivales? Draskovar es nuestro amigo. Mis asesores me dicen que es pacífico.",
                    "Frutas, verduras y conservas. En plena crisis, eso vale oro.",
                    "Sí. Comercio de alimentos y estabilidad para todos.",
                    "Ninguno. Draskovar me ha asegurado que sus obras en la frontera son carreteras.",
                    -1),
                intel = "Calidor es una democracia sana y sus ofertas comerciales son reales. PERO su presidente está mal informado: la inteligencia de Calidor depende de informes que le pasa... Draskovar.",
                intelSource = "Agente «Lince»", intelDate = "Hace 2 días", photo = "",
                exports = "Frutas, verduras, conservas", imports = "Maquinaria, fertilizantes", dependency = "Media.",
                region = "Centro. Relación estrecha (y engañosa) con Draskovar.",
                stabilityBar = 70, riskBar = 55,
                allyEffect = new Effect(0, 2, 6), cutEffect = new Effect(0, 1, -3),
                allyText = "Calidor llena nuestros mercados de comida barata en plena crisis.",
                cutText = "Atacas a Calidor por su amistad con Draskovar. Su presidente, desconcertado, pide explicaciones.",
                cutLater = new DelayedEvent(0, "Calidor no era una amenaza",
                    "Se demuestra que el presidente Mestre era sincero, solo estaba engañado. Atacar a una democracia por estar mal informada pasa factura.",
                    new Effect(-7, 0, 0)),
                rivals = new[] { "draskovar" },
            });

            l.Add(new Country
            {
                id = "nordhavn", name = "Nordhavn", leader = "Presidenta Sigrid Aal", year = 4,
                mapPos = new Vector2(0.30f, 0.91f), color = C("#5e86c2"),
                profile = Profile.Honesto, correct = Correct.Alianza, rumor = Rumor.Aliado, attackable = false,
                population = "5,0 millones", government = "Democracia", army = "Pequeño", relation = "Cordial",
                speech = "La crisis energética está hundiendo el continente. Nordhavn tiene energía hidroeléctrica de sobra. Os la vendemos a precio estable durante diez años. Sin letra pequeña.",
                questions = Q(
                    "Vaskarr. Compra las deudas de los países en crisis y luego les da órdenes.",
                    "Energía hidroeléctrica, aluminio, pescado.",
                    "Sí, comercial y energética.",
                    "Vaskarr está comprando la deuda de media región. Eso me preocupa más que cualquier ejército.",
                    -1),
                intel = "Producción eléctrica abundante y verificada. Cumple sus contratos desde hace décadas. Coincide con lo que dice.",
                intelSource = "Ministerio de Energía", intelDate = "Hace 1 semana", photo = "Presa hidroeléctrica de Nordhavn",
                exports = "Electricidad, aluminio", imports = "Alimentos, tecnología", dependency = "ALTA en plena crisis energética.",
                region = "Sur. Rival declarado de Vaskarr.",
                stabilityBar = 85, riskBar = 10,
                allyEffect = new Effect(2, 2, 9), cutEffect = new Effect(0, 0, -6),
                allyText = "La electricidad de Nordhavn llega a Aurelia. Las fábricas vuelven a funcionar.",
                cutText = "Rechazas la energía de Nordhavn en plena crisis. Hay apagones en varias ciudades.",
                cutPerTurn = new Effect(0, 0, -2), cutTurns = 3,
                rivals = new[] { "vaskarr" },
            });

            l.Add(new Country
            {
                id = "vaskarr", name = "Vaskarr", leader = "Gobernador Elias Varn", year = 4,
                mapPos = new Vector2(0.88f, 0.14f), color = C("#3f5a4a"),
                profile = Profile.Mentiroso, correct = Correct.Corte, rumor = Rumor.Aliado, attackable = false,
                population = "4,1 millones", government = "Oligarquía financiera", army = "Pequeño", relation = "Amistosa",
                speech = "En tiempos de crisis, los amigos se ayudan. Vaskarr os ofrece un fondo de rescate generoso, a bajo interés. Solo pedimos un asiento en vuestro consejo económico. Una formalidad.",
                questions = Q(
                    "Nordhavn tiene envidia de nuestro éxito.",
                    "Dinero. Mucho dinero. Y bancos dispuestos a prestarlo.",
                    "Una alianza financiera. Nosotros prestamos, vosotros crecéis.",
                    "La crisis es una oportunidad para quien sabe verla.",
                    -1),
                intel = "Tres países que aceptaron el «fondo de rescate» de Vaskarr han tenido que privatizar puertos y minas a favor de bancos de Vaskarr. El «asiento en el consejo» da derecho a veto sobre presupuestos.",
                intelSource = "Analistas del Banco Central", intelDate = "Hace 2 semanas", photo = "",
                exports = "Servicios financieros", imports = "Casi todo", dependency = "Ninguna... de momento.",
                region = "Islas del noreste. Rival de Nordhavn.",
                stabilityBar = 75, riskBar = 60,
                allyEffect = new Effect(0, 0, 10), cutEffect = new Effect(0, 0, -4),
                allyText = "El dinero de Vaskarr llega rápido. Un banquero de Vaskarr se sienta en tu consejo económico.",
                cutText = "Rechazas el fondo de rescate. Los mercados se ponen nerviosos un par de días.",
                allyLater = new DelayedEvent(0, "Vaskarr reclama sus garantías",
                    "Usando su asiento en el consejo, Vaskarr bloquea los presupuestos y exige la privatización del puerto principal.",
                    new Effect(-6, 0, -14)),
                rivals = new[] { "nordhavn" },
            });

            l.Add(new Country
            {
                id = "sarvia", name = "Sarvia", leader = "Coronel Mara Veyl (gobierno provisional)", year = 4,
                mapPos = new Vector2(0.73f, 0.25f), color = C("#a3a35a"),
                profile = Profile.Ambiguo, correct = Correct.Cualquiera, rumor = Rumor.Amenaza, attackable = true,
                population = "8,0 millones", government = "Gobierno provisional tras golpe", army = "Mediano", relation = "Incierta",
                speech = "Hace un mes había un presidente corrupto. Ahora estoy yo. Prometo elecciones... cuando las condiciones lo permitan. Necesito reconocimiento internacional. ¿Me lo daréis?",
                questions = Q(
                    "Depende de quién me reconozca primero.",
                    "Níquel, tierras raras. Y una posición estratégica en el estrecho.",
                    "Quiero socios. Lo demás ya se verá.",
                    "Hay facciones armadas en el norte. Las controlaremos.",
                    3),
                intel = "Información contradictoria: unos informes dicen que el golpe fue popular contra un gobierno corrupto; otros hablan de detenciones de periodistas. No hay datos fiables.",
                intelSource = "Fuentes contradictorias", intelDate = "Hace 1 semana", photo = "Multitud en la plaza central (¿celebrando o protestando?)",
                exports = "Níquel, tierras raras", imports = "Alimentos, combustible", dependency = "Baja.",
                region = "Estrecho del noreste. Situación interna volátil.",
                stabilityBar = 30, riskBar = 55,
                allyEffect = new Effect(-4, 5, 2), cutEffect = new Effect(2, -2, -2),
                allyText = "Reconoces al gobierno de la coronel Veyl. Ganas un socio en el estrecho; pierdes algo de imagen.",
                cutText = "Sancionas al gobierno golpista. La coronel Veyl busca apoyos en otra parte.",
                rivals = new string[0],
            });

            return l;
        }

        public static string YearHint(int year)
        {
            switch (year)
            {
                case 1: return "Año 1. Los líderes de este año suelen decir la verdad. Aprende a leer los papeles: dossier, inteligencia, comercio y riesgo. Recuerda que los iconos del mapa son SOSPECHAS, no certezas.";
                case 2: return "Año 2. Cuidado: algunos líderes mienten, y algunos informes también. Compara lo que dicen con lo que muestran los documentos. Fíjate en las fechas y en las fuentes.";
                case 3: return "Año 3. Los países se agrupan en bloques. Aliarte con uno puede enfriar la relación con sus rivales; enfrentarte a otro puede ganarte amigos.";
                default: return "Año 4. Crisis global. Solo DOS preguntas por visita y cada decisión desgasta un poco al país. Aguanta hasta el final del mandato.";
            }
        }
    }
}
