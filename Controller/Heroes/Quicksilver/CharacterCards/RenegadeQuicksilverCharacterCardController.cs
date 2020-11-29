﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cauldron.Quicksilver
{
    public class RenegadeQuicksilverCharacterCardController : QuicksilverSubCharacterCardController
    {
        public RenegadeQuicksilverCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        int Incap2Count = 0;

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        //One player may draw a card now.
                        IEnumerator coroutine = base.GameController.SelectHeroToDrawCard(base.HeroTurnTakerController, cardSource: base.GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        break;
                    }
                case 1:
                    {
                        //The next time a hero is dealt damage, they may play a card.
                        OnDealDamageStatusEffect statusEffect = new OnDealDamageStatusEffect(base.Card, "PlayCardResponse", "The next time a hero is dealt damage, they may play a card.", new TriggerType[] { TriggerType.PlayCard }, base.TurnTaker, base.Card);
                        statusEffect.NumberOfUses = 1;
                        Incap2Count++;
                        IEnumerator coroutine2 = base.AddStatusEffect(statusEffect, true);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine2);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine2);
                        }
                        break;
                    }
                case 2:
                    {
                        //Increase all damage dealt by 1 until the start of your next turn.
                        IncreaseDamageStatusEffect increaseDamageStatusEffect = new IncreaseDamageStatusEffect(1);
                        increaseDamageStatusEffect.UntilStartOfNextTurn(base.TurnTaker);
                        IEnumerator coroutine3 = base.AddStatusEffect(increaseDamageStatusEffect, true);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine3);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine3);
                        }
                        break;
                    }
            }
            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Discard a card. 
            IEnumerator coroutine = base.SelectAndDiscardCards(base.HeroTurnTakerController, 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            //Search your deck for Iron Retort and put it into your hand. 
            coroutine = base.SearchForCards(base.HeroTurnTakerController, true, false, 1, 1, new LinqCardCriteria((Card c) => c.Identifier == "IronRetort"), false, true, false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            //Shuffle your deck.
            coroutine = base.ShuffleDeck(base.HeroTurnTakerController, base.TurnTaker.Deck);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        public IEnumerator PlayCardResponse(DealDamageAction action, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            //...they may play a card
            IEnumerator coroutine = base.GameController.SelectAndPlayCardsFromHand(base.GameController.FindHeroTurnTakerController(action.Target.Owner.ToHero()), Incap2Count, true, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }
    }
}