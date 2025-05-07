namespace Uno
{
    public class Player
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Personality { get; set; }
        public List<Card>? Cards { get; set; }
        public string? LeastColor { get; set; }
    }

    public class Card
    {
        public string Color { get; set; }
        public string Number { get; set; }
        public bool Used { get; set; }
    }

    public static class Program
    {
        static string SpecialCheck(string cardType, bool specialUsed)
        {
            bool isNumber = int.TryParse(cardType, out _);
            if (isNumber || specialUsed)
            {
                return string.Empty;
            }
            else
            {
                return cardType;
            }
        }

        static int HandleSpecial(string cardType, List<Player> players)
        {
            switch (cardType.ToLower())
            {
                case "skipturn":
                    return 1;
                case "reverse":
                    players.Reverse();
                    return 0;
                case "plussto":
                    return 3;
                case "plussfire":
                    return 4;
                default:
                    return 0;
            }
        }

        static string ChooseColor(List<Card> AI, bool playCheck)
        {
            int maxcount = 0;
            string mostFrequent = "Lilla";
            for (int i = 0; i < AI.Count; i++)
            {
                int count = 0;
                for (int j = 0; j < AI.Count; j++)
                {
                    if (AI[i] == AI[j])
                    {
                        count++;
                    }
                }
                if (count > maxcount)
                {
                    maxcount = count;
                    mostFrequent = AI[i].Color;
                }
            }
            //Console.WriteLine($"\n--------------AI sin mest forekommende farge er {mostFrequent}-----------------\n");
            if(mostFrequent.ToLower() == "wild" && !playCheck)
            {
                int chosenColor = new Random().Next(0, 4);
                switch (chosenColor)
                {
                    case 0:
                        mostFrequent = "Rød";
                        break;
                    case 1:
                        mostFrequent = "Blå";
                        break;
                    case 2:
                        mostFrequent = "Grønn";
                        break;
                    case 3:
                        mostFrequent = "Gul";
                        break;
                }
            }
            return mostFrequent;
        }

        static List<Card> CreateCards()
        {
            List<Card> cardBank = new();

            for (int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 13; j++)
                {
                    string colorTranslate = "Lilla";
                    switch (i){
                        case 0:
                        case 1:
                            colorTranslate = "Rød";
                            break;
                        case 2:
                        case 3:
                            colorTranslate = "Blå";
                            break;
                        case 4:
                        case 5:
                            colorTranslate = "Grønn";
                            break;
                        case 6:
                        case 7:
                            colorTranslate = "Gul";
                            break;
                    }

                    string numberTranslate;
                    bool used = false;
                    switch (j){
                        case 10:
                            numberTranslate = "SkipTurn";
                            used = false;
                            break;
                        case 11:
                            numberTranslate = "Reverse";
                            used = false;
                            break;
                        case 12:
                            numberTranslate = "PlussTo";
                            used = false;
                            break;
                        default:
                            numberTranslate = j.ToString();
                            break;
                    }
                    cardBank.Add(new Card() { Color = colorTranslate, Number = numberTranslate, Used = used });
                }
            }
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string wildCheck = "MinusFire";
                    bool used = false;
                    switch (i)
                    {
                        case 0:
                            wildCheck = "PlussFire";
                            used = false;
                            break;
                        case 1:
                            wildCheck = "Farge";
                            break;
                    }
                    cardBank.Add(new Card() { Color = "Wild", Number = wildCheck, Used = used });
                }
            }

            return cardBank;
        }

        static void SimpleRound(int AICount)
        {
            Console.WriteLine("Starter runde...");

            //Opprett spillerliste
            List<Player> players = new();

            //Generer kortstokk
            List<Card> cardBank = CreateCards();

            //Opprett bunke, og tildel kort (kommenter ut loopen under for å bare ha bots)
            List<Card> playerDeck = new();

            for (int i = 0; i < 7; i++)
            {
                Random startCardRandomizer = new();
                int startCardPicker = startCardRandomizer.Next(0, cardBank.Count);

                playerDeck.Add(cardBank[startCardPicker]);
                cardBank.RemoveAt(startCardPicker);
            }
            players.Add(new Player() { Name = "Player", Type = "Player", Personality = "None", Cards = playerDeck });

            List<List<Card>> AIDecks = new();

            //Opprett AIene og deres bunker
            for (int a = 0; a < AICount; a++)
            {
                List<Card> AIDeck = new();

                for (int i = 0; i < 7; i++)
                {
                    Random startCardRandomizer = new();
                    int startCardPicker = startCardRandomizer.Next(0, cardBank.Count);

                    AIDeck.Add(cardBank[startCardPicker]);
                    cardBank.RemoveAt(startCardPicker);

                    //Console.WriteLine($"AI {a}: Farge: " + AIDeck[i].Color + ", Nummer: " + AIDeck[i].Number + ". Total: " + cardBank.Count);
                }

                Random personRandomizer = new();
                int personPicker = personRandomizer.Next(0, 4);
                string personality = personPicker switch
                {
                    0 => "Standard",
                    1 => "Aggressiv",
                    2 => "Smart",
                    //3 => "Sabotør",
                    _ => "Standard",
                };
                AIDecks.Add(AIDeck);

                players.Add(new Player(){ Name = $"AI {a + 1}", Type = "AI", Personality = personality, Cards = AIDeck });
            }

            //Opprett generelle variabler
            Random startRoundRandomizer = new();
            int startRoundPicker = startRoundRandomizer.Next(0, cardBank.Count);

            string lastPlayedCardColor = cardBank[startRoundPicker].Color;
            string lastPlayedCardNumber = cardBank[startRoundPicker].Number;

            bool playerCardAvailability = false;

            string lastSpecial;
            int specialVar = 0;
            bool specialUsed = false;

            bool playerWin = false;
            dynamic? AIWin = null;

            int? defencePlay = null;
            int defenceStack = 0;

            //Start gameloop
            while (!playerWin || AIWin == null)
            {
                switch (specialVar)
                {
                    //Skip kort
                    case 1:
                        Thread.Sleep(1000);
                        switch (players[0].Type)
                        {
                            case "Player":
                                Console.WriteLine("\n------------------------------------------------------------------\n\nDin tur har blitt hoppet over.\n\n");
                                break;
                            case "AI":
                                Console.WriteLine($"\n------------------------------------------------------------------\n\n{players[0].Name} sin tur har blitt hoppet over.\n\n");
                                break;
                        }
                        Thread.Sleep(3000);
                        specialUsed = true;
                        break;

                    //Pluss to
                    case 3:
                        Thread.Sleep(1000);
                        //defenceStack += 2;
                        List<Card> defence = new();
                        foreach (Card card in players[0].Cards!)
                        {
                            if (card.Number.ToLower() == "plussto")
                            {
                                defence.Add(card);
                            }
                        }
                        if (defence.Count > 0)
                        {
                            switch (players[0].Type)
                            {
                                case "Player":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\n{players[^1].Name} har spilt et pluss-to kort. Vil du bruke ditt pluss-to kort for å forsvare deg selv?\n0 - Ikke spill\n1 - Spill\n\n");
                                    bool wLoopCheck = false;
                                    while (!wLoopCheck)
                                    {
                                        bool playChoice = int.TryParse(Console.ReadLine(), out int choice);
                                        if (playChoice && choice == 1)
                                        {
                                            defencePlay = players[0].Cards!.IndexOf(defence[0]);
                                            //lastPlayedCardNumber = players[0].Cards[defencePlay.GetValueOrDefault()];
                                            Console.WriteLine("Du har valgt å forsvare deg selv.");
                                            wLoopCheck = true;
                                        }
                                        else if (playChoice && choice == 0)
                                        {
                                            Console.WriteLine("Du har valgt å ikke forsvare deg selv, og må trekke kort.");
                                            Thread.Sleep(2000);
                                            for (int i = 0; i < defenceStack; i++)
                                            {
                                                Random drawCardRandomizer = new();
                                                int drawCardPicker = drawCardRandomizer.Next(0, cardBank.Count);

                                                Console.WriteLine("Farge: " + cardBank[drawCardPicker].Color + ", Nummer: " + cardBank[drawCardPicker].Number + ". Total: " + cardBank.Count);
                                                players[0].Cards!.Add(cardBank[drawCardPicker]);
                                                cardBank.RemoveAt(drawCardPicker);
                                                Thread.Sleep(1000);
                                            }
                                            Thread.Sleep(1000);
                                            specialUsed = true;
                                            players[0].LeastColor = string.Empty;
                                            defenceStack = 0;
                                            defencePlay = null;
                                            wLoopCheck = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Ugyldig verdi oppgitt, prøv igjen.");
                                        }
                                    }

                                    break;
                                case "AI":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\n{players[0].Name} spiller enda et pluss-to kort for å forsvare seg selv.\n\n");
                                    //defenceStack += 2;
                                    Thread.Sleep(1000);
                                    defencePlay = players[0].Cards!.IndexOf(defence[0]);
                                    break;
                            }
                        }
                        else
                        {
                            switch (players[0].Type)
                            {
                                case "Player":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\nDu må trekke {defenceStack} kort.\n\n");
                                    break;
                                case "AI":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\n{players[0].Name} må trekke {defenceStack} kort.\n\n");
                                    break;
                            }
                            Thread.Sleep(2000);
                            for (int i = 0; i < defenceStack; i++)
                            {
                                Random drawCardRandomizer = new();
                                int drawCardPicker = drawCardRandomizer.Next(0, cardBank.Count);

                                if(players[0].Type == "Player")
                                {
                                    Console.WriteLine("Farge: " + cardBank[drawCardPicker].Color + ", Nummer: " + cardBank[drawCardPicker].Number + ". Total: " + cardBank.Count);
                                }
                                else
                                {
                                    Console.WriteLine($"{players[0].Name} har dratt ett kort.");
                                }
                                players[0].Cards!.Add(cardBank[drawCardPicker]);
                                cardBank.RemoveAt(drawCardPicker);
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);
                            specialUsed = true;
                            players[0].LeastColor = string.Empty;
                            defenceStack = 0;
                            defencePlay = null;
                        }
                        break;

                    //Pluss fire
                    case 4:
                        Thread.Sleep(1000);
                        //defenceStack += 4;
                        List<Card> fDefence = new();
                        foreach(Card card in players[0].Cards!)
                        {
                            if(card.Number.ToLower() == "plussfire")
                            {
                                fDefence.Add(card);
                            }
                        }
                        if (fDefence.Count > 0)
                        {
                            switch (players[0].Type)
                            {
                                case "Player":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\n{players[^1].Name} har spilt et pluss-fire kort. Vil du bruke ditt pluss-fire kort for å forsvare deg selv?\n0 - Ikke spill\n1 - Spill\n\n");
                                    bool wLoopCheck = false;
                                    while (!wLoopCheck)
                                    {
                                        bool playChoice = int.TryParse(Console.ReadLine(), out int choice);
                                        if (playChoice && choice == 1)
                                        {
                                            defencePlay = players[0].Cards!.IndexOf(fDefence[0]);
                                            //lastPlayedCardNumber = players[0].Cards[defencePlay.GetValueOrDefault()];
                                            Console.WriteLine("Du har valgt å forsvare deg selv.");
                                            wLoopCheck = true;
                                        }
                                        else if (playChoice && choice == 0)
                                        {
                                            Thread.Sleep(2000);
                                            Console.WriteLine("Du har valgt å ikke forsvare deg selv, og må trekke kort.");
                                            for (int i = 0; i < defenceStack; i++)
                                            {
                                                Random drawCardRandomizer = new();
                                                int drawCardPicker = drawCardRandomizer.Next(0, cardBank.Count);

                                                Console.WriteLine("Farge: " + cardBank[drawCardPicker].Color + ", Nummer: " + cardBank[drawCardPicker].Number + ". Total: " + cardBank.Count);
                                                players[0].Cards!.Add(cardBank[drawCardPicker]);
                                                cardBank.RemoveAt(drawCardPicker);
                                                Thread.Sleep(1000);
                                            }
                                            Thread.Sleep(1000);
                                            specialUsed = true;
                                            players[0].LeastColor = string.Empty;
                                            defenceStack = 0;
                                            defencePlay = null;
                                            wLoopCheck = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Ugyldig verdi oppgitt, prøv igjen.");
                                        }
                                    }
                                    
                                    break;
                                case "AI":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\n{players[0].Name} spiller enda et pluss-fire kort for å forsvare seg selv.\n\n");
                                    defencePlay = players[0].Cards!.IndexOf(fDefence[0]);
                                    Thread.Sleep(1000);
                                    break;
                            }
                        }
                        else
                        {
                            switch (players[0].Type)
                            {
                                case "Player":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\nDu må trekke {defenceStack} kort.\n\n");
                                    break;
                                case "AI":
                                    Console.WriteLine($"\n------------------------------------------------------------------\n\n{players[0].Name} må trekke {defenceStack} kort.\n\n");
                                    break;
                            }
                            Thread.Sleep(2000);
                            for (int i = 0; i < defenceStack; i++)
                            {
                                Random drawCardRandomizer = new();
                                int drawCardPicker = drawCardRandomizer.Next(0, cardBank.Count);

                                if (players[0].Type == "Player")
                                {
                                    Console.WriteLine("Farge: " + cardBank[drawCardPicker].Color + ", Nummer: " + cardBank[drawCardPicker].Number + ". Total: " + cardBank.Count);
                                }
                                else
                                {
                                    Console.WriteLine($"{players[0].Name} har dratt ett kort.");
                                }
                                players[0].Cards!.Add(cardBank[drawCardPicker]);
                                cardBank.RemoveAt(drawCardPicker);
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);
                            specialUsed = true;
                            players[0].LeastColor = string.Empty;
                            defenceStack = 0;
                            defencePlay = null;
                        }
                        break;
                    default:
                        switch (players[0].Type)
                        {
                            case "Player":
                                Console.WriteLine($"\n-------------------------------------------------------------------\n\nDet er din tur. Kortet på bordet er en {lastPlayedCardColor} {lastPlayedCardNumber}. Dine kort:\n\n");
                                break;
                            case "AI":
                                Console.WriteLine($"\n-------------------------------------------------------------------\n\nDet er {players[0].Name} sin tur. Kortet på bordet er en {lastPlayedCardColor} {lastPlayedCardNumber}. Spillertypen er {players[0].Personality}\n");
                                Thread.Sleep(1000);
                                Console.WriteLine($"{players[0].Name} har {players[0].Cards!.Count} kort.\n\n");
                                break;
                        }
                        for (int i = 0; i < players[0].Cards!.Count; i++)
                        {
                            if (players[0].Type == "Player")
                            {
                                Console.WriteLine(i + " - " + players[0].Cards![i].Color + " " + players[0].Cards![i].Number);
                            }
                            if (players[0].Cards![i].Color.ToLower() == lastPlayedCardColor.ToLower() || players[0].Cards![i].Number.ToLower() == lastPlayedCardNumber.ToLower() || players[0].Cards![i].Color.ToLower() == "wild")
                            {
                                playerCardAvailability = true;
                            }
                        }
                        if (playerCardAvailability)
                        {
                            bool playCardLoopCheck = true;
                            while (playCardLoopCheck)
                            {
                                int chosenCard;
                                if (players[0].Type == "Player")
                                {
                                    Console.WriteLine("\nSkriv inn kortet du vil spille:\n");
                                    chosenCard = Convert.ToInt32(Console.ReadLine()); //Gjør om til TryParse slik at null-verdier blir håndtert riktig
                                }

                                //Kjør spesiell kode avhengig av AI sin personlighet

                                //Aggressiv personlighet
                                else if (players[0].Personality == "Aggressiv")
                                {
                                    List<Card> useableCards = new();
                                    List<Card> specCards = new();
                                    for (int i = 0; i < players[0].Cards!.Count; i++)
                                    {
                                        if (players[0].Cards![i].Color.ToLower() == lastPlayedCardColor.ToLower() || players[0].Cards![i].Number.ToLower() == lastPlayedCardNumber.ToLower() || players[0].Cards![i].Color.ToLower() == "wild")
                                        {
                                            useableCards.Add(players[0].Cards![i]);

                                            string specCheck = SpecialCheck(players[0].Cards![i].Number, false);
                                            if(players[0].Cards![i].Color == "Wild" || specCheck != string.Empty)
                                            {
                                                specCards.Add(players[0].Cards![i]);
                                            }
                                        }
                                    }
                                    Random AICardChooser = new();
                                    int selectedCard;
                                    if (specCards.Count == 0)
                                    {
                                        selectedCard = AICardChooser.Next(0, useableCards.Count);
                                        chosenCard = players[0].Cards!.IndexOf(useableCards[selectedCard]);
                                    }
                                    else
                                    {
                                        selectedCard = AICardChooser.Next(0, specCards.Count);
                                        chosenCard = players[0].Cards!.IndexOf(specCards[selectedCard]);
                                    }
                                }

                                //Smart personlighet
                                else if (players[0].Personality == "Smart")
                                {
                                    //Sorter kortene som kan spilles
                                    List<Card> useableCards = new();
                                    List<Card> specCards = new();
                                    List<Card> wildCards = new();
                                    for (int i = 0; i < players[0].Cards!.Count; i++)
                                    {
                                        if (players[0].Cards![i].Color.ToLower() == lastPlayedCardColor.ToLower() || players[0].Cards![i].Number.ToLower() == lastPlayedCardNumber.ToLower())
                                        {
                                            useableCards.Add(players[0].Cards![i]);

                                            string specCheck = SpecialCheck(players[0].Cards![i].Number, false);
                                            if (specCheck != string.Empty)
                                            {
                                                //Console.WriteLine($"AI har spesielle kort, {players[0].Cards![i].Number}");
                                                specCards.Add(players[0].Cards![i]);
                                            }
                                        }else if(players[0].Cards![i].Color.ToLower() == "wild")
                                        {
                                            //Console.WriteLine($"AI har wild-kort, {players[0].Cards![i].Number}");
                                            wildCards.Add(players[0].Cards![i]);
                                        }
                                    }
                                    //Finn ut hvem som har minst kort
                                    int lowestCards = 5;
                                    dynamic lowestCardsHolder = string.Empty;
                                    foreach(Player player in players)
                                    {
                                        if(player.Cards!.Count < lowestCards)
                                        {
                                            lowestCards = player.Cards!.Count;
                                            lowestCardsHolder = player.Name;
                                        }
                                    }
                                    //Kjør hvis spilleren etter deres tur er spilleren med minst kort
                                    if (lowestCardsHolder == players[1].Name)
                                    {
                                        int index = useableCards.FindIndex(color => color.Color == players[1].LeastColor);
                                        if(index >= 0)
                                        {
                                            //Console.WriteLine($"{players[0].Name} vet at {players[1].Name} ikke har {players[1].LeastColor}.");
                                            chosenCard = players[0].Cards!.IndexOf(useableCards[index]);
                                        }
                                        else if (index == -1 && specCards.Count != 0)
                                        {
                                            Random AICardChooser = new();
                                            int selectedCard = AICardChooser.Next(0, specCards.Count);
                                            chosenCard = players[0].Cards!.IndexOf(specCards[selectedCard]);
                                        }
                                        else
                                        {
                                            //Kjør vanlig kode
                                            if (useableCards.Count == 0 && wildCards.Count > 0)
                                            {
                                                Random AICardChooser = new();
                                                int rIndex = AICardChooser.Next(0, wildCards.Count);
                                                chosenCard = players[0].Cards!.IndexOf(wildCards[rIndex]);
                                            }
                                            else
                                            {
                                                string mostFrequentColor = ChooseColor(useableCards, true);
                                                List<Card> cards = new();
                                                foreach (Card card in useableCards)
                                                {
                                                    if (card.Color == mostFrequentColor)
                                                    {
                                                        cards.Add(card);
                                                        //Console.WriteLine(card.Color + ", " + card.Number);
                                                    }
                                                }
                                                int cIndex = useableCards.FindIndex(color => color.Color == mostFrequentColor);
                                                chosenCard = players[0].Cards!.IndexOf(useableCards[cIndex]);
                                            }

                                        }
                                    }
                                    //Vanlig kode
                                    else
                                    {
                                        if (useableCards.Count == 0 && wildCards.Count > 0)
                                        {
                                            Random AICardChooser = new();
                                            int index = AICardChooser.Next(0, wildCards.Count);
                                            chosenCard = players[0].Cards!.IndexOf(wildCards[index]);
                                        }
                                        else
                                        {
                                            string mostFrequentColor = ChooseColor(useableCards, true);
                                            List<Card> cards = new();
                                            foreach (Card card in useableCards)
                                            {
                                                if (card.Color == mostFrequentColor)
                                                {
                                                    cards.Add(card);
                                                    //Console.WriteLine(card.Color + ", " + card.Number);
                                                }
                                            }
                                            int cIndex = useableCards.FindIndex(color => color.Color == mostFrequentColor);
                                            chosenCard = players[0].Cards!.IndexOf(useableCards[cIndex]);
                                        }
                                    }
                                }
                                else
                                {
                                    Random AICardChooser = new();
                                    chosenCard = AICardChooser.Next(0, players[0].Cards!.Count);
                                }
                                
                                if (chosenCard != null && chosenCard < players[0].Cards!.Count)
                                {
                                    if (players[0].Cards![chosenCard].Color.ToLower() == lastPlayedCardColor.ToLower() || players[0].Cards![chosenCard].Number.ToLower() == lastPlayedCardNumber.ToLower() || players[0].Cards![chosenCard].Color.ToLower() == "wild")
                                    {
                                        if (players[0].Type == "Player" && players[0].Cards![chosenCard].Color.ToLower() != "wild")
                                        {
                                            Console.WriteLine($"\nDu har lagt på en {players[0].Cards![chosenCard].Color} {players[0].Cards![chosenCard].Number}.");
                                        }
                                        else if (players[0].Type == "AI" && players[0].Cards![chosenCard].Color.ToLower() != "wild")
                                        {
                                            Thread.Sleep(2000);
                                            Console.WriteLine($"\n{players[0].Name} har lagt på en {players[0].Cards![chosenCard].Color} {players[0].Cards![chosenCard].Number}.");
                                        }
                                        if (players[0].Cards![chosenCard].Color.ToLower() == "wild")
                                        {
                                            if (players[0].Type == "Player")
                                            {
                                                Console.WriteLine($"\nDu har lagt på et wild {players[0].Cards![chosenCard].Number} kort. Velg fargen du vil at kortet skal ha:\n0 - Rød\n1 - Blå\n2 - Grønn\n3 - Gul\n");
                                            }
                                            bool wildChooseColorLoop = true;
                                            while (wildChooseColorLoop)
                                            {
                                                dynamic chosenColor;
                                                if (players[0].Type == "Player")
                                                {
                                                    chosenColor = Convert.ToInt32(Console.ReadLine()); //Gjør om til TryParse slik at null-verdier blir håndtert riktig
                                                    if (chosenColor != null && chosenColor < 4)
                                                    {
                                                        switch (chosenColor)
                                                        {
                                                            case 0:
                                                                lastPlayedCardColor = "Rød";
                                                                break;
                                                            case 1:
                                                                lastPlayedCardColor = "Blå";
                                                                break;
                                                            case 2:
                                                                lastPlayedCardColor = "Grønn";
                                                                break;
                                                            case 3:
                                                                lastPlayedCardColor = "Gul";
                                                                break;
                                                        }
                                                        wildChooseColorLoop = false;
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("\nUgyldig verdi oppgitt, prøv igjen.\n");
                                                        Thread.Sleep(1000);
                                                    }
                                                }
                                                else
                                                {
                                                    chosenColor = ChooseColor(players[0].Cards!, false);
                                                    lastPlayedCardColor = chosenColor;

                                                    Thread.Sleep(2000);
                                                    Console.WriteLine($"{players[0].Name} har lagt på et Wild {players[0].Cards![chosenCard].Number} kort, og valgt fargen {lastPlayedCardColor}.");
                                                    Thread.Sleep(2000);
                                                    wildChooseColorLoop= false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            lastPlayedCardColor = players[0].Cards![chosenCard].Color;
                                        }
                                        lastPlayedCardNumber = players[0].Cards![chosenCard].Number;
                                        cardBank.Add(players[0].Cards![chosenCard]);
                                        players[0].Cards!.RemoveAt(chosenCard);
                                        
                                        playCardLoopCheck = false;
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        if (players[0].Type == "Player")
                                        {
                                            Console.WriteLine("\nDu kan ikke spille dette kortet.");
                                            Thread.Sleep(1000);
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Ugyldig verdi oppgitt, prøv igjen.");
                                    Thread.Sleep(1000);
                                }
                            }

                            specialUsed = false;
                            if (lastPlayedCardNumber.ToLower() == "plussto")
                            {
                                defenceStack += 2;
                            }
                            else if (lastPlayedCardNumber.ToLower() == "plussfire")
                            {
                                defenceStack += 4;
                            }
                        }
                        else
                        {
                            if (players[0].Type == "Player")
                            {
                                Console.WriteLine("\nDu har ingen kort som du kan spille. Trykk på enter for å trekke kort.");
                                Console.ReadLine();
                            }
                            else
                            {
                                Console.WriteLine($"\n{players[0].Name} har ingen kort som de kan spille, og må trekke kort.");
                            }
                            
                            bool drawLoopCheck = true;
                            while (drawLoopCheck)
                            {
                                Random drawCardRandomizer = new();
                                int drawCardPicker = drawCardRandomizer.Next(0, cardBank.Count);

                                if (players[0].Name == "Player")
                                {
                                    Console.WriteLine("Farge: " + cardBank[drawCardPicker].Color + ", Nummer: " + cardBank[drawCardPicker].Number + ". Total: " + cardBank.Count);
                                }
                                else
                                {
                                    Console.WriteLine($"{players[0].Name} har dratt ett kort.");
                                }
                                //Console.WriteLine("Farge: " + cardBank[drawCardPicker].Color + ", Nummer: " + cardBank[drawCardPicker].Number + ". Total: " + cardBank.Count);
                                players[0].Cards!.Add(cardBank[drawCardPicker]);

                                if (cardBank[drawCardPicker].Color.ToLower() == lastPlayedCardColor.ToLower() || cardBank[drawCardPicker].Number.ToLower() == lastPlayedCardNumber.ToLower() || cardBank[drawCardPicker].Color.ToLower() == "wild")
                                {
                                    players[0].Cards!.Remove(cardBank[drawCardPicker]);
                                    if (cardBank[drawCardPicker].Color.ToLower() == "wild")
                                    {
                                        if (players[0].Type == "Player")
                                        {
                                            Console.WriteLine($"\nDu har dratt et wild {cardBank[drawCardPicker].Number} kort, og legger det på. Velg fargen du vil at kortet skal ha:\n0 - Rød\n1 - Blå\n2 - Grønn\n3 - Gul\n");
                                            bool wildChooseColorLoop = true;
                                            while (wildChooseColorLoop)
                                            {
                                                int chosenColor = Convert.ToInt32(Console.ReadLine()); //Gjør om til TryParse slik at null-verdier blir håndtert riktig
                                                if (chosenColor != null && chosenColor < 4)
                                                {
                                                    switch (chosenColor)
                                                    {
                                                        case 0:
                                                            lastPlayedCardColor = "Rød";
                                                            break;
                                                        case 1:
                                                            lastPlayedCardColor = "Blå";
                                                            break;
                                                        case 2:
                                                            lastPlayedCardColor = "Grønn";
                                                            break;
                                                        case 3:
                                                            lastPlayedCardColor = "Gul";
                                                            break;
                                                    }
                                                    wildChooseColorLoop = false;
                                                }
                                                else
                                                {
                                                    Console.WriteLine("\nUgyldig verdi oppgitt, prøv igjen.\n");
                                                    Thread.Sleep(1000);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string chosenColor = ChooseColor(players[0].Cards!, false);
                                            lastPlayedCardColor = chosenColor;

                                            Console.WriteLine($"\n{players[0].Name} har trukket et wild {cardBank[drawCardPicker].Number} kort. De ender fargen til {lastPlayedCardColor} og legger det på.");
                                        }
                                    }
                                    else
                                    {
                                        Thread.Sleep(1000);
                                        if (players[0].Type == "Player")
                                        {
                                            Console.WriteLine($"\nDu har dratt en {cardBank[drawCardPicker].Color} {cardBank[drawCardPicker].Number}, og legger den på.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"\n{players[0].Name} har dratt en {cardBank[drawCardPicker].Color} {cardBank[drawCardPicker].Number}, og legger den på.");
                                        }

                                        players[0].LeastColor = lastPlayedCardColor;
                                        lastPlayedCardColor = cardBank[drawCardPicker].Color;
                                    }
                                    lastPlayedCardNumber = cardBank[drawCardPicker].Number;
                                    drawLoopCheck = false;
                                }
                                else
                                {
                                    cardBank.RemoveAt(drawCardPicker);
                                    Thread.Sleep(1000);
                                }
                            }
                            Thread.Sleep(2000);
                        specialUsed = false;
                        if (lastPlayedCardNumber.ToLower() == "plussto")
                        {
                            defenceStack += 2;
                        }
                        else if (lastPlayedCardNumber.ToLower() == "plussfire")
                        {
                            defenceStack += 4;
                        }
                    }
                    break;
                }

                

                if (defencePlay != null)
                {
                    //Console.WriteLine("Defenceplay: " + defencePlay.ToString());
                    if (players[0].Cards![defencePlay.GetValueOrDefault()].Number.ToLower() == "plussfire")
                    {
                        dynamic chosenColor;
                        lastPlayedCardNumber = players[0].Cards![defencePlay.GetValueOrDefault()].Number;
                        if (players[0].Type == "Player")
                        {
                            Console.WriteLine($"\nDu har lagt på et wild pluss-fire kort. Velg fargen du vil at kortet skal ha:\n0 - Rød\n1 - Blå\n2 - Grønn\n3 - Gul\n");
                            bool wildChooseColorLoop = true;
                            while (wildChooseColorLoop)
                            {
                                if (players[0].Type == "Player")
                                {
                                    chosenColor = Convert.ToInt32(Console.ReadLine()); //Gjør om til TryParse slik at null-verdier blir håndtert riktig
                                    if (chosenColor != null && chosenColor < 4)
                                    {
                                        switch (chosenColor)
                                        {
                                            case 0:
                                                lastPlayedCardColor = "Rød";
                                                break;
                                            case 1:
                                                lastPlayedCardColor = "Blå";
                                                break;
                                            case 2:
                                                lastPlayedCardColor = "Grønn";
                                                break;
                                            case 3:
                                                lastPlayedCardColor = "Gul";
                                                break;
                                        }
                                        cardBank.Add(players[0].Cards![defencePlay.GetValueOrDefault()]);
                                        players[0].Cards!.RemoveAt(defencePlay.GetValueOrDefault());
                                        wildChooseColorLoop = false;
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nUgyldig verdi oppgitt, prøv igjen.\n");
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                        else
                        {
                            cardBank.Add(players[0].Cards![defencePlay.GetValueOrDefault()]);
                            players[0].Cards!.RemoveAt(defencePlay.GetValueOrDefault());
                            chosenColor = ChooseColor(players[0].Cards!, false);
                            lastPlayedCardColor = chosenColor;

                            Thread.Sleep(2000);
                            Console.WriteLine($"{players[0].Name} har lagt på et Wild pluss-fire kort, og valgt fargen {lastPlayedCardColor}.");
                        }
                        defenceStack += 4;
                    }
                    else if (players[0].Cards![defencePlay.GetValueOrDefault()].Number.ToLower() == "plussto")
                    {
                        lastPlayedCardColor = players[0].Cards![defencePlay.GetValueOrDefault()].Color;
                        cardBank.Add(players[0].Cards![defencePlay.GetValueOrDefault()]);
                        players[0].Cards!.RemoveAt(defencePlay.GetValueOrDefault());
                        //Console.WriteLine("Pre stack: " + defenceStack);
                        defenceStack += 2;
                        //Console.WriteLine("Post stack: " + defenceStack);
                    }
                }

                if (players[0].Cards!.Count == 0)
                {
                    //playerWin = true;
                    Console.WriteLine($"\n{players[0].Name} har vunnet.");
                    break;
                }

                //Sjekk for og håndter spesialkort
                lastSpecial = SpecialCheck(lastPlayedCardNumber, specialUsed);
                specialVar = HandleSpecial(lastSpecial, players);
                        
                if (lastPlayedCardNumber != "Reverse")
                {
                    players.Add(players[0]);
                    players.Remove(players[0]);
                }
               
                playerCardAvailability = false;
            }
        }

        static void Main()
        {
            //For debugging, skip introsekvens
            //SimpleRound(3);
            Console.WriteLine("Velkommen til Uno. Skriv start for å starte, eller exit for å avslutte.\n");

            bool startWLoopCheck = false;
            while (!startWLoopCheck)
            {
                string startInput = Console.ReadLine()!.ToLower();

                if(startInput == "start")
                {
                    SimpleRound(3);
                    startWLoopCheck = true;
                }
                else if (startInput == "exit")
                {
                    startWLoopCheck = true;
                }
                else { Console.WriteLine("Ugyldig input, prøv igjen."); }
            }
        }
    }
}