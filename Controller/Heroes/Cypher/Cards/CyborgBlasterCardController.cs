﻿
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;


namespace Cauldron.Cypher
{
    public class CyborgBlasterCardController : CypherBaseCardController
    {
        //==============================================================
        // You may move 1 Augment in play next to a new hero.
        // One augmented hero deals 1 target 2 lightning damage.
        //==============================================================

        public static string Identifier = "CyborgBlaster";

        private const int DamageToDeal = 2;

        public CyborgBlasterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // You may move 1 Augment in play next to a new hero.
            SelectCardDecision scd = new SelectCardDecision(GameController, DecisionMaker,
                SelectionType.MoveCardNextToCard, GetAugmentsInPlay(), true, cardSource: GetCardSource());

            IEnumerator routine = base.GameController.SelectCardAndDoAction(scd, MoveAugment);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }

            // One augmented hero deals 1 target 2 lightning damage.
            List<SelectTurnTakerDecision> sttd = new List<SelectTurnTakerDecision>();
            routine = this.GameController.SelectHeroTurnTaker(this.DecisionMaker, SelectionType.CharacterCard, false, false, sttd,
                new LinqTurnTakerCriteria(tt => GetAugmentedHeroTurnTakers().Contains(tt)), cardSource: GetCardSource());

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }

            if (!sttd.Any())
            {
                yield break;
            }

            TurnTaker selectedTurnTaker = sttd.First().SelectedTurnTaker;

            this.GameController.SelectHeroToSelectTargetAndDealDamage()
            

        }

        private IEnumerator MoveAugment(SelectCardDecision scd)
        {
            if (scd.SelectedCard == null)
            {
                yield break;
            }



            yield break;
        }
    }
}