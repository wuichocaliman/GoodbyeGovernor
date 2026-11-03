using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElDespacho
{
    public partial class DespachoGame
    {
        [Serializable] public class CountryState
        {
            public string id;
            public bool visited, annoyed, grateful, attacked;
            public Decision decision;
            public int questionsAsked;
            public bool[] asked;
        }
        [Serializable] public class PendingState
        {
            public int year; public string countryId, headline, body; public Effect effect;
        }
        [Serializable] public class DrainState { public Effect effect; public int turns; public string reason; }
        [Serializable] public class SaveData
        {
            public int schema = 1;
            public string version = DeskMenus.Version;
            public long utcTicks;
            public int year, estabilidad, seguridad, prosperidad, allies, destroyed, sanctioned;
            public int cardsThisYear, randomSeed, randomDraws, docTab;
            public string screen, currentCardId, countryId, cardResultText, stampText;
            public bool cardAnswered, documentOpened, stampPlaced, stampYes, mapOpened, mandateFinished;
            public Vector2 stampPos; public float stampAngle; public Effect cardResultEffect;
            public string[] usedCards, flags, queue, news, dialog;
            public CountryState[] countries;
            public PendingState[] pending;
            public DrainState[] drains;
        }
        internal string ScreenName { get; private set; } = "Título";
        internal bool HasSession { get; private set; }
        internal bool InIntroduction => ScreenName == "Introducción";
        internal string SessionSummary => $"Año {year} de 4  ·  {ScreenName}\nEstabilidad {estabilidad}   Seguridad {seguridad}   Prosperidad {prosperidad}";
        int randomSeed, randomDraws;
        bool mandateFinished;
        DeskMenus menus;

        internal void StartFromMenu()
        {
            if (TutorialManager.Instance != null) TutorialManager.Instance.PrepareNewSession();
            NewGame(); HasSession = true; ShowIntroduction();
        }
        internal void ReturnToTitle()
        {
            HasSession = false;
            ShowTitle();
        }
        void ShowTitle()
        {
            HasSession = false; ScreenName = "Título";
            Clear(); menus.ShowMain();
        }
        internal SaveData CaptureSave()
        {
            if (!HasSession) throw new InvalidOperationException("No hay una partida en curso");
            return new SaveData {
                utcTicks = DateTime.UtcNow.Ticks, year = year, estabilidad = estabilidad, seguridad = seguridad, prosperidad = prosperidad,
                allies = allies, destroyed = destroyed, sanctioned = sanctioned, cardsThisYear = cardsThisYear,
                randomSeed = randomSeed, randomDraws = randomDraws, docTab = docTab, screen = ScreenName,
                currentCardId = currentCard?.id, countryId = current?.id, cardResultText = cardResultText,
                cardAnswered = cardAnswered, documentOpened = documentOpened, stampPlaced = stampPlaced, stampYes = stampYes,
                stampText = stampText, stampPos = stampPos, stampAngle = stampAngle, cardResultEffect = cardResultEffect,
                mapOpened = mapOpenedThisYear, mandateFinished = mandateFinished,
                usedCards = usedCards.ToArray(), flags = flags.ToArray(), queue = cardQueue.ToArray(), news = news.ToArray(), dialog = dialogLog.ToArray(),
                countries = countries.Select(c => new CountryState {id=c.id, visited=c.visited, annoyed=c.annoyed,
                    grateful=c.grateful, attacked=c.attacked, decision=c.decision, questionsAsked=c.questionsAsked, asked=c.questions.Select(q=>q.asked).ToArray()}).ToArray(),
                pending = pending.Select(p=>new PendingState {year=p.year,countryId=p.country?.id,headline=p.ev.headline,body=p.ev.body,effect=p.ev.effect}).ToArray(),
                drains = drains.Select(d=>new DrainState {effect=d.perTurn,turns=d.turns,reason=d.reason}).ToArray()
            };
        }
        public static void ValidateSave(SaveData s)
        {
            void Require(bool valid) { if (!valid) throw new InvalidOperationException("Guardado incompatible o incompleto"); }
            Require(s != null && s.schema == 1);
            Require(s.year >= 1 && s.year <= 4 && s.utcTicks > 0 && s.utcTicks <= DateTime.MaxValue.Ticks);
            Require(s.estabilidad >= 0 && s.estabilidad <= 100 && s.seguridad >= 0 && s.seguridad <= 100 && s.prosperidad >= 0 && s.prosperidad <= 100);
            Require(s.randomDraws >= 0 && s.randomDraws <= 10000 && s.cardsThisYear >= 0);
            Require(s.usedCards != null && s.flags != null && s.queue != null && s.news != null && s.dialog != null && s.pending != null && s.drains != null && s.countries != null);
            var db = CountryDatabase.Create(); var deck = CardDatabase.Create();
            Require(s.countries.Length == db.Count && s.countries.Select(c=>c?.id).Distinct().Count() == db.Count);
            foreach (var c in s.countries) {
                var original = db.FirstOrDefault(x=>x.id==c?.id);
                Require(original != null && c.asked != null && c.asked.Length == original.questions.Count && Enum.IsDefined(typeof(Decision), c.decision));
                Require(c.questionsAsked >= 0 && c.questionsAsked <= original.questions.Count);
            }
            Require(s.usedCards.Concat(s.queue).All(id=>deck.Any(c=>c.id==id)));
            Require(string.IsNullOrEmpty(s.currentCardId) || deck.Any(c=>c.id==s.currentCardId));
            Require(!s.cardAnswered || !string.IsNullOrEmpty(s.currentCardId));
            Require(string.IsNullOrEmpty(s.countryId) || db.Any(c=>c.id==s.countryId));
            Require(s.screen != "Visita" || !string.IsNullOrEmpty(s.countryId));
            Require(s.pending.All(p=>p!=null && (string.IsNullOrEmpty(p.countryId)||db.Any(c=>c.id==p.countryId))));
            Require(s.drains.All(d=>d!=null && d.turns>=0));
        }
        internal void RestoreSave(SaveData s)
        {
            ValidateSave(s); // Complete validation before replacing the session.
            if (TutorialManager.Instance != null) TutorialManager.Instance.SuppressLoadedSession();
            NewGame(); HasSession = true;
            year=s.year; estabilidad=s.estabilidad; seguridad=s.seguridad; prosperidad=s.prosperidad;
            allies=s.allies; destroyed=s.destroyed; sanctioned=s.sanctioned; cardsThisYear=s.cardsThisYear;
            randomSeed=s.randomSeed; randomDraws=s.randomDraws; rng=new System.Random(randomSeed);
            for(int i=0;i<randomDraws;i++) rng.Next();
            currentCard=cards.FirstOrDefault(c=>c.id==s.currentCardId); current=Find(s.countryId); docTab=Mathf.Clamp(s.docTab,0,3);
            cardAnswered=s.cardAnswered; documentOpened=s.documentOpened; cardAnsweredAt=Time.unscaledTime;
            stampPlaced=s.stampPlaced; stampYes=s.stampYes; stampText=s.stampText; stampPos=s.stampPos; stampAngle=s.stampAngle;
            cardResultText=s.cardResultText; cardResultEffect=s.cardResultEffect; mapOpenedThisYear=s.mapOpened; mandateFinished=s.mandateFinished;
            usedCards.UnionWith(s.usedCards); flags.UnionWith(s.flags); cardQueue.AddRange(s.queue);
            news.Clear(); news.AddRange(s.news); dialogLog.Clear(); dialogLog.AddRange(s.dialog);
            foreach(var saved in s.countries) {
                var c=Find(saved.id); c.visited=saved.visited; c.annoyed=saved.annoyed; c.grateful=saved.grateful;
                c.attacked=saved.attacked; c.decision=saved.decision; c.questionsAsked=saved.questionsAsked;
                for(int i=0;i<c.questions.Count;i++) c.questions[i].asked=saved.asked[i];
            }
            foreach(var p in s.pending) pending.Add(new Pending {year=p.year,country=Find(p.countryId),ev=new DelayedEvent(0,p.headline,p.body,p.effect)});
            foreach(var d in s.drains) drains.Add(new Drain {perTurn=d.effect,turns=d.turns,reason=d.reason});
            // Consequences were already applied when saved; never execute them again.
            if(IsDefeated) ShowGameOver();
            else if(mandateFinished) ShowEnding();
            else if(s.screen=="Mapa") ShowMap();
            else if(s.screen=="Visita") ShowVisit();
            else if(s.screen=="Introducción") ShowIntroduction();
            else ShowOffice();
        }
    }
}
