﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra;

namespace Cauldron.Impact
{
    public class ImpactCharacterCardController : HeroCharacterCardController
    {
        public ImpactCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //"{Impact} deals 1 target 1 infernal damage. You may destroy 1 hero ongoing card to increase this damage by 2."
            int numTargets = GetPowerNumeral(0, 1);
            int numDamage = GetPowerNumeral(1, 1);
            int numToDestroy = GetPowerNumeral(2, 1);
            int numBoost = GetPowerNumeral(3, 2);

            var targetDecision = new SelectTargetsDecision(GameController,
                                            DecisionMaker,
                                            (Card c) => c.IsInPlayAndHasGameText && c.IsTarget && GameController.IsCardVisibleToCardSource(c, GetCardSource()),
                                            numTargets,
                                            false,
                                            numTargets,
                                            false,
                                            new DamageSource(GameController, this.Card),
                                            numDamage,
                                            DamageType.Infernal,
                                            selectTargetsEvenIfCannotPerformAction: true,
                                            cardSource: GetCardSource());
            IEnumerator coroutine = GameController.SelectCardsAndDoAction(targetDecision, _ => DoNothing());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            var selectedTargets = targetDecision.SelectCardDecisions.Select(scd => scd.SelectedCard).Where((Card c) => c != null);
            if(selectedTargets.Count() == 0)
            {
                yield break;
            }

            ITrigger boostTrigger = null;
            bool didDestroyCards = false;
            if (GameController.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.IsOngoing && c.IsHero && GameController.IsCardVisibleToCardSource(c, GetCardSource())).Count() >= numToDestroy)
            {
                var storedYesNo = new List<YesNoCardDecision> { };
                coroutine = GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.IncreaseDamage, this.Card, storedResults: storedYesNo, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if(DidPlayerAnswerYes(storedYesNo))
                {
                    coroutine = GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria(c => c.IsInPlayAndHasGameText && c.IsOngoing && c.IsHero && GameController.IsCardVisibleToCardSource(c, GetCardSource()), "hero ongoing"), numToDestroy, false, numToDestroy, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    boostTrigger = new IncreaseDamageTrigger(GameController, (DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.Card && dd.CardSource != null && dd.CardSource.Card == this.Card, dd => GameController.IncreaseDamage(dd, numBoost, false, GetCardSource()), null, TriggerPriority.Medium, false, GetCardSource());
                    AddToTemporaryTriggerList(AddTrigger(boostTrigger));
                    didDestroyCards = true;
                }
            }

            coroutine = GameController.DealDamage(DecisionMaker, this.Card, (Card c) => selectedTargets.Contains(c), numDamage, DamageType.Infernal, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (didDestroyCards)
            {
                RemoveTemporaryTrigger(boostTrigger);
            }
            yield break;
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        //"One hero may use a power now.",
                        yield break;
                    }
                case 1:
                    {
                        //"Select a hero target. That target deals 1 other target 1 projectile damage.",
                        break;
                    }
                case 2:
                    {
                        //"Damage dealt to environment cards is irreducible until the start of your turn."
                        break;
                    }
            }
            yield break;
        }

        private PhaseChangeAction FakeAction()
        {
            return new PhaseChangeAction(GetCardSource(), Game.ActiveTurnPhase, Game.ActiveTurnPhase, true);
        }

        private bool LogAndReturnTrue(Card c)
        {
            Log.Debug("Examining card " + c.Title);
            return true;
        }
    }
}