﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;


namespace Cauldron.Vector
{
    public class VectorCharacterCardController : VectorBaseCharacterCardController
    {
        /*
         *
         * Setup:
         *
         * At the start of the game, put {Vector}'s villain character cards into play, "Asymptomatic Carrier" side up, with 40 HP.
         *
         * Gameplay:
         *
         * - Whenever {Vector} is dealt damage, play the top card of the villain deck.
         * - If {Vector} regains all his HP, he escapes. Game over.
         * - Supervirus is indestructible. Cards beneath it are indestructible and have no game text.
         * - Whenever Supervirus is in play and there are {H + 2} or more cards beneath it, flip {Vector}'s villain character cards.
         * - If {Vector} is dealt damage by an environment card, he becomes immune to damage dealt by environment cards until the end of the turn.
         *
         *
         * Advanced:
         *
         * - At the end of the villain turn, {Vector} regains 2 HP.
         *
         * Flipped Gameplay:
         *
         * - Whenever {Vector} flips to this side, remove Supervirus from the game. Put all virus cards that were beneath it into the villain trash.
         * - Reduce damage dealt to {Vector} by 1 for each villain target in play.
         * - At the end of the villain turn, play the top card of the villain deck.
         *
         * Flipped Advanced:
         *
         * - Increase damage dealt by {Vector} by 2.
         *
         */

        private const int AdvancedHpGain = 2;
        private const int AdvancedDamageIncrease = 2;
        private const string EndingMessage = "Vector regained all of his health!  The Heroes lose!";

        public VectorCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override void AddSideTriggers()
        {
            // Front side (Asymptomatic Carrier)
            if (!base.Card.IsFlipped)
            {
                // Whenever {Vector} is dealt damage, play the top card of the villain deck.
                base.SideTriggers.Add(
                    base.AddTrigger<DealDamageAction>(dda => dda.Target.Equals(this.Card) && dda.DamageSource != null &&
                    dda.Target.Equals(this.Card), DealtDamageResponse, new[] { TriggerType.PlayCard }, TriggerTiming.After));

                // If {Vector} regains all his HP, he escapes. Game over.
                base.SideTriggers.Add(base.AddTrigger<GainHPAction>(gha => gha.IsSuccessful && gha.HpGainer.Equals(this.Card), 
                    CheckHpResponse, TriggerType.GainHP, TriggerTiming.After));

                // If {Vector} is dealt damage by an environment card, he becomes
                // immune to damage dealt by environment cards until the end of the turn.
                base.SideTriggers.Add(base.AddTrigger<DealDamageAction>(dda => dda.Target.Equals(this.Card)
                    && dda.DamageSource != null && dda.DamageSource.IsEnvironmentCard,
                DealtDamageByEnvResponse, new[] {TriggerType.ImmuneToDamage}, TriggerTiming.After));
                
                if (this.IsGameAdvanced)
                {
                    // TODO:
                }
            }
            // Flipped side (Desperate Assassin)
            else
            {
                // At the end of the villain turn, play the top card of the villain deck.
                base.SideTriggers.Add(base.AddEndOfTurnTrigger(tt => tt == this.TurnTaker, 
                    FlippedEndOfTurnResponse, TriggerType.PlayCard));

                // Reduce damage dealt to {Vector} by 1 for each villain target in play.
                int AmountToReduce(DealDamageAction dmg) => base.FindCardsWhere(c => c.IsInPlayAndHasGameText && c.IsVillainTarget).Count();

                base.SideTriggers.Add(base.AddReduceDamageTrigger(dda => dda.DamageSource != null && dda.CardSource != null 
                                                                                                  && dda.Target == base.CharacterCard, AmountToReduce));

                if (this.IsGameAdvanced)
                {
                    // TODO:
                }
            }
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            // Remove Super Virus card from the game
            if (IsSuperVirusInPlay())
            {
                // Move any cards underneath Super Virus into villain trash
                IEnumerable<Card> cardsUnder = FindCardsWhere(c => c.Location == GetSuperVirusCard().UnderLocation);
                IEnumerator r1 = this.GameController.MoveCards(this.DecisionMaker, cardsUnder, this.TurnTaker.Trash,
                    cardSource: GetCardSource());

                // Remove Super Virus from game
                IEnumerator r2 = this.GameController.MoveCard(this.TurnTakerController, GetSuperVirusCard(),
                    new Location(this.Card, LocationName.OutOfGame));

                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(r1);
                    yield return base.GameController.StartCoroutine(r2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(r1);
                    base.GameController.ExhaustCoroutine(r2);
                }
            }
        }

        private IEnumerator DealtDamageResponse(DealDamageAction dda)
        {
            IEnumerator routine = this.GameController.PlayTopCard(this.DecisionMaker, this.TurnTakerController,
                false, 1);

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }
        }

        private IEnumerator DealtDamageByEnvResponse(DealDamageAction dda)
        {
            ImmuneToDamageStatusEffect itdse = new ImmuneToDamageStatusEffect
            {
                TargetCriteria = { IsSpecificCard = this.CharacterCard },
                SourceCriteria = { IsEnvironment = true }
            };
            itdse.UntilThisTurnIsOver(base.Game);

            IEnumerator routine = base.GameController.AddStatusEffect(itdse, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }
        }

        private IEnumerator CheckHpResponse(GainHPAction gha)
        {
            if (this.Card.HitPoints != this.Card.MaximumHitPoints)
            {
                yield break;
            }

            // Reached maximum HP, game over
            IEnumerator routine = base.GameController.GameOver(EndingResult.AlternateDefeat, EndingMessage, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }
        }

        private IEnumerator FlippedEndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator routine = this.GameController.PlayTopCard(this.DecisionMaker, this.TurnTakerController,
                false, 1);

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }
        }
    }
}