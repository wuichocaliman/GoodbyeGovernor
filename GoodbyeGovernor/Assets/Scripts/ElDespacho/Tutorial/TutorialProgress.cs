using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElDespacho
{
    public enum TutorialStatus { Pendiente, EnCurso, Completado, Omitido }
    [Serializable] public sealed class TutorialProgress
    {
        public int revision = 1;
        public List<string> completed = new List<string>();
        public List<string> skipped = new List<string>();
        public List<string> facts = new List<string>();
        public string active = "";
        public void Normalize() { completed ??= new List<string>(); skipped ??= new List<string>(); facts ??= new List<string>(); active ??= ""; }
        public bool Resolved(string id) => completed.Contains(id) || skipped.Contains(id);
        public void Observe(string fact) { if (!facts.Contains(fact)) facts.Add(fact); }
        public void Complete(string id) { if (!Resolved(id)) completed.Add(id); if(active==id)active=""; }
        public TutorialProgress Copy() { var copy=JsonUtility.FromJson<TutorialProgress>(JsonUtility.ToJson(this)); copy.Normalize(); return copy; }
        public TutorialStatus Status(TutorialStepData[] steps)
        {
            if(steps.All(s=>skipped.Contains(s.persistentId)))return TutorialStatus.Omitido;
            if(steps.All(s=>Resolved(s.persistentId)))return skipped.Count>0?TutorialStatus.Omitido:TutorialStatus.Completado;
            return completed.Count+skipped.Count>0 || active!="" ? TutorialStatus.EnCurso : TutorialStatus.Pendiente;
        }
    }

    public partial class DespachoGame
    {
        internal TutorialProgress Tutorial { get; private set; } = new TutorialProgress();
        internal int TutorialQuestions => QuestionsPerVisit;
        internal int TutorialTab => docTab;
        internal bool TutorialOpen => documentOpened;
        internal bool TutorialAnswered => cardAnswered;
        internal bool TutorialReadyYear => CardsDoneThisYear && PendingThisYear==0;
        internal bool TutorialHasPaper => currentCard!=null;
        internal bool TutorialChain => currentCard!=null && (currentCard.followUpOnly || !string.IsNullOrEmpty(currentCard.requires));
        internal void TutorialFact(string fact) => Tutorial.Observe(fact);
        // Only public player actions/state enter the guide; never country truth or future effects.
        internal void ReconcileTutorial()
        {
            if(!HasSession)return;
            if(ScreenName!="Introducción" && ScreenName!="Título")Tutorial.Observe("assumed");
            if(documentOpened || cardAnswered) { Tutorial.Observe("grabbed");Tutorial.Observe("opened"); }
            if(cardAnswered)Tutorial.Observe("answered");
            if(cardsThisYear>1 || CardsDoneThisYear)Tutorial.Observe("archived");
            if(mapOpenedThisYear)Tutorial.Observe("map");
            if(countries.Any(c=>c.visited))Tutorial.Observe("visit");
            if(countries.Any(c=>c.questionsAsked>0))Tutorial.Observe("asked");
            if(countries.Any(c=>c.decision!=Decision.Ninguna))Tutorial.Observe("diplomacy");
            if(year>1 || mandateFinished)Tutorial.Observe("year");
        }
    }
}


