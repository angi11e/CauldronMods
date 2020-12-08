﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using Cauldron.TheStranger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Handelabra.Sentinels.Engine.Controller.ChronoRanger;

namespace CauldronTests
{
    [TestFixture()]
    public class TheStrangerVariantTests : BaseTest
    {
        #region TheStrangerHelperFunctions
        protected HeroTurnTakerController stranger { get { return FindHero("TheStranger"); } }
        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(stranger.CharacterCard, 1);
            DealDamage(villain, stranger, 2, DamageType.Melee);
        }


        #endregion

        [Test()]
        public void TestRunecarvedStrangerLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/RunecarvedTheStrangerCharacter", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(stranger);
            Assert.IsInstanceOf(typeof(RunecarvedTheStrangerCharacterCardController), stranger.CharacterCardController);

            Assert.AreEqual(28, stranger.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestPastStrangerLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/PastTheStrangerCharacter", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(stranger);
            Assert.IsInstanceOf(typeof(PastTheStrangerCharacterCardController), stranger.CharacterCardController);

            Assert.AreEqual(29, stranger.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestWastelandRoninStrangerLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/WastelandRoninTheStrangerCharacter", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(stranger);
            Assert.IsInstanceOf(typeof(WastelandRoninTheStrangerCharacterCardController), stranger.CharacterCardController);

            Assert.AreEqual(24, stranger.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestCornStrangerLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/CornTheStrangerCharacter", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(stranger);
            Assert.IsInstanceOf(typeof(CornTheStrangerCharacterCardController), stranger.CharacterCardController);

            Assert.AreEqual(27, stranger.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestRunecarvedStrangerInnatePower()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/RunecarvedTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            SetHitPoints(ra, 10);
            
            GoToUsePowerPhase(stranger);
            DecisionSelectCard = ra.CharacterCard;
            //1 hero target regains 2 HP.
            QuickHPStorage(ra);
            UsePower(stranger.CharacterCard);
            QuickHPCheck(2);
        }

        [Test()]
        public void TestRunecarvedStrangerIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/RunecarvedTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            SetupIncap(baron);
            AssertIncapacitated(stranger);

            Card mere = PutInHand("Mere");


            //One player may play a card now.
            GoToUseIncapacitatedAbilityPhase(stranger);
            DecisionSelectTarget = haka.CharacterCard;
            DecisionSelectCard = mere;
            QuickHandStorage(haka);
            UseIncapacitatedAbility(stranger, 0);
            //should have one fewer card in haka's hand
            QuickHandCheck(-1);
            AssertInPlayArea(haka, mere);

        }

        [Test()]
        public void TestRunecarvedStrangerIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/RunecarvedTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            SetupIncap(baron);
            AssertIncapacitated(stranger);

            Card dominion = PlayCard("Dominion");
            Card enduring = PlayCard("EnduringIntercession");
            Card backlash = PlayCard("BacklashField");
            Card field = PlayCard("LivingForceField");
            Card police = PlayCard("PoliceBackup");
            Card pileup = PlayCard("TrafficPileup");

            //Destroy 1 hero ongoing, 1 non-hero ongoing, and 1 environment card.
            GoToUseIncapacitatedAbilityPhase(stranger);
            DecisionSelectCards = new Card[] { enduring, field, pileup };
            UseIncapacitatedAbility(stranger, 1);
            AssertIsInPlay(dominion, backlash, police);
            AssertInTrash(enduring, field, pileup);

        }

        [Test()]
        public void TestRunecarvedStrangerIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/RunecarvedTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            SetHitPoints(baron, 20);
            SetHitPoints(haka, 20);

            SetupIncap(baron);
            AssertIncapacitated(stranger);

            //Villain targets may not regain HP until the start of your turn.
            GoToUseIncapacitatedAbilityPhase(stranger);
            UseIncapacitatedAbility(stranger, 2);

            //should prevent villain
            QuickHPStorage(baron);
            PlayCard("FleshRepairNanites");
            QuickHPCheckZero();

            //should not prevent hero
            QuickHPStorage(haka);
            PlayCard("VitalitySurge");
            QuickHPCheck(2);

            //should expire at start of next turn
            GoToStartOfTurn(stranger);
            QuickHPStorage(baron);
            PlayCard("FleshRepairNanites");
            QuickHPCheck(10);
        }

        [Test()]
        public void TestPastStrangerInnatePower()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/PastTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            GoToUsePowerPhase(stranger);
            //The next time {TheStranger} would deal himself damage, redirect it to another target.
            UsePower(stranger.CharacterCard);

            //check redirected
            SelectCardsForNextDecision(ra.CharacterCard);
            QuickHPStorage(stranger, ra);
            DealDamage(stranger, stranger, 1, DamageType.Toxic, isIrreducible: true);
            QuickHPCheck(0, -1);

            //only next damage
            SelectCardsForNextDecision(ra.CharacterCard);
            QuickHPStorage(stranger, ra);
            DealDamage(stranger, stranger, 1, DamageType.Toxic, isIrreducible: true);
            QuickHPCheck(-1, 0);

            UsePower(stranger.CharacterCard);

            //only self damage
            SelectCardsForNextDecision(ra.CharacterCard);
            QuickHPStorage(stranger, ra);
            DealDamage(ra, stranger, 1, DamageType.Fire, isIrreducible: true);
            QuickHPCheck(-1, 0);
        }

        [Test()]
        public void TestPastStrangerIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/PastTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            SetupIncap(baron);
            AssertIncapacitated(stranger);

            //One player may use a power now.
            GoToUseIncapacitatedAbilityPhase(stranger);
            DecisionSelectTurnTaker = ra.TurnTaker;
            DecisionSelectTarget = haka.CharacterCard;
            QuickHPStorage(haka);
            UseIncapacitatedAbility(stranger, 0);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestPastStrangerIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/PastTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            Card field = PlayCard("LivingForceField");
            Card backlash = PlayCard("BacklashField");

            SetupIncap(baron);
            AssertIncapacitated(stranger);

            //Destroy 1 ongoing card.
            GoToUseIncapacitatedAbilityPhase(stranger);
            DecisionSelectCard = backlash;
            UseIncapacitatedAbility(stranger, 1);
            AssertInPlayArea(baron, field);
            AssertInTrash(backlash);
        }

        [Test()]
        public void TestPastStrangerIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.TheStranger/PastTheStrangerCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();

            SetHitPoints(ra, 7);
            SetHitPoints(haka, 7);

            PlayCard("TaMoko");

            SetupIncap(baron);
            AssertIncapacitated(stranger);

            //The target with the lowest HP deals itself 1 irreducible toxic damage.
            GoToUseIncapacitatedAbilityPhase(stranger);
            SelectCardsForNextDecision(haka.CharacterCard);
            QuickHPStorage(ra, haka);
            UseIncapacitatedAbility(stranger, 2);
            //is irreducible so should go through Ta Moko
            QuickHPCheck(0, -1);

        }


    }
}
