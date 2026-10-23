using System.Collections.Generic;
using UnityEngine;

namespace ElDespacho
{
    public enum Profile { Honesto, Mentiroso, Ambiguo, MalInformado }
    public enum Rumor { Ninguno, Amenaza, Aliado }
    public enum Decision { Ninguna, Alianza, Corte }
    public enum Correct { Alianza, Corte, Cualquiera }

    [System.Serializable]
    public struct Effect
    {
        public int estabilidad, seguridad, prosperidad;

        public Effect(int estabilidad, int seguridad, int prosperidad)
        {
            this.estabilidad = estabilidad;
            this.seguridad = seguridad;
            this.prosperidad = prosperidad;
        }

        public bool IsZero => estabilidad == 0 && seguridad == 0 && prosperidad == 0;

        public static Effect operator +(Effect a, Effect b)
        {
            return new Effect(a.estabilidad + b.estabilidad, a.seguridad + b.seguridad, a.prosperidad + b.prosperidad);
        }
    }

    public class Question
    {
        public string text;
        public string answer;
        public bool incomoda;
        public bool asked;
    }

    public class DelayedEvent
    {
        public int yearsLater;
        public string headline;
        public string body;
        public Effect effect;

        public DelayedEvent(int yearsLater, string headline, string body, Effect effect)
        {
            this.yearsLater = yearsLater;
            this.headline = headline;
            this.body = body;
            this.effect = effect;
        }
    }

    public class Country
    {
        // Identidad
        public string id, name, leader;
        public int year;
        public Vector2 mapPos;
        public Color color;

        // Verdad oculta
        public Profile profile;
        public Correct correct;
        public Rumor rumor;
        public bool attackable;

        // Dossier
        public string population, government, army, relation;

        // Diálogo
        public string speech;
        public List<Question> questions = new List<Question>();

        // Inteligencia
        public string intel, intelSource, intelDate, photo;

        // Comercio
        public string exports, imports, dependency;

        // Región
        public string region;
        public int stabilityBar, riskBar;

        // Consecuencias
        public Effect allyEffect, cutEffect;
        public string allyText, cutText;
        public DelayedEvent allyLater, cutLater;
        public Effect cutPerTurn;
        public int cutTurns;
        public string[] rivals = new string[0];

        // Estado en partida
        public bool visited;
        public Decision decision;
        public bool annoyed;
        public bool grateful;
        public bool attacked;
        public int questionsAsked;

        public bool WasCorrect => correct == Correct.Cualquiera ||
                                  (correct == Correct.Alianza && decision == Decision.Alianza) ||
                                  (correct == Correct.Corte && decision == Decision.Corte);
    }
}
