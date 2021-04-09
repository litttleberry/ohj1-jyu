using Jypeli;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;


/// @author saelmarj
/// @version 9.4.2021
/// 
/// Grafiikat ja äänet PD, eivät omaa tuotantoa.
/// 
/// <summary>
/// Huppelihiippailu-peli.
/// </summary>
public class Huppelihiippailu : PhysicsGame
{
    /// <summary>
    /// Pelattava hahmo.
    /// </summary>
    PlatformCharacter ukkeli;


    /// <summary>
    /// Pelihahmon liikkumisnopeus.
    /// </summary>
    const double LIIKKUMISNOPEUS = 200;
    

    /// <summary>
    /// Pelikentän ruudun koko. Kenttä tekstitiedostosta.
    /// </summary>
    const double RUUDUN_KOKO = 30;
    
    
    /// <summary>
    /// Pelin kesto sekunteina, ja krapulamittarin maksimiarvo.
    /// </summary>
    const double PELIN_KESTO = 60;


    /// <summary>
    /// Yöpalojen määrä pelin alussa, eli
    /// 's'-merkkien määrä kentta.txt-tiedostossa.
    /// </summary>
    const double SNACKIT_ALUSSA = 20;

    
    /// <summary>
    /// Pistelaskuri.
    /// </summary>
    DoubleMeter krapulamittari;
    

    /// <summary>
    /// Yöllisten herkkupalojen lista.
    /// </summary>
    List<PhysicsObject> yopalat = new();


    /// <summary>
    /// Lista, joka kerää tietoa peliajan kulumisesta.
    /// Käytetään loppupisteiden laskennassa.
    /// </summary>
    List<double> loppupisteetAjasta = new();


    /// <summary>
    /// Alkuvalikko, jossa aliohjelmakutsu pelin aloittamiseksi.
    /// </summary>
    public override void Begin()
    {
        AloitaAlusta();
        IsPaused = true;

        MultiSelectWindow alkuvalikko = new
            ("Huppelihiippailu \n \n" +
            "Hupsis! Napsun pubi-ilta ystävien \n" +
               "kanssa venähti pikkutunneille, ja \n" +
               "nyt on aika suunnata kotiin. \n" +
               "Onneksi on leppeä kesäyö ja \n" +
               "kotimatkalla mieltä ilahduttavat \n" +
               "menomatkalla piilotetut herkut. \n" +
               "Kerää herkut, varo naapureita ja \n" +
               "tiellä lojuvia esteitä ja vie Napsu \n" +
               "kotiin ennen kuin aika loppuu ja \n" +
               "krapula iskee. Onnea peliin!",
               "Aloita uusi peli", "Näytä ohjeet", "Lopeta");
        alkuvalikko.Position = new Vector(0, 0);
        alkuvalikko.AddItemHandler(0, delegate () { 
            IsPaused = false;
            alkuvalikko.DefaultCancel = 0; });
        alkuvalikko.AddItemHandler(1, NaytaOhjevalikko);
        alkuvalikko.AddItemHandler(2, ConfirmExit);
        Add(alkuvalikko);
    }


    /// <summary>
    /// Tyhjentää pelin ja alustaa sen alkamaan alusta.
    /// Kentän ja pistelaskurin luominen, taustamusiikki.
    /// </summary>
    private void AloitaAlusta()
    {
        ClearAll();
        yopalat.Clear();
        LuoKentta();
        LuoKrapulamittari();
        MediaPlayer.Play("taustamusa");
        MediaPlayer.IsRepeating = true;
    }


    /// <summary>
    ///  Pelin ohjeet, joihin pääsee alkuvalikosta tai keskeyttämällä pelin.
    ///  Paluu takaisin peliin, aloitus uudelleen, poistuminen pelistä.
    /// </summary>
    private void NaytaOhjevalikko()
    {
        IsPaused = true;
        MultiSelectWindow ohjeet = new
            ("Liikuta Napsua nuolinäppäimillä, \n" +
            "pidä tauko painamalla välilyöntiä, \n" +
            "lopeta peli painamalla Esc. Näppäin- \n" +
            "komennot saat esiin pelin aikana \n" +
            "painamalla F1.",
               "Jatka peliä", "Uusi peli", "Lopeta");
        ohjeet.Position = new Vector(0, 0);
        ohjeet.AddItemHandler(0, delegate () { 
            IsPaused = false; 
            ohjeet.DefaultCancel = 0; });
        ohjeet.AddItemHandler(1, AloitaAlusta);
        ohjeet.AddItemHandler(2, ConfirmExit);
        Add(ohjeet);
    }


    /// <summary>
    /// Luodaan pisteitä ja aikaa mittaava laskuri joka näkyy pelikentän reunassa.
    /// </summary>
    private void LuoKrapulamittari()
    {
        krapulamittari = new(PELIN_KESTO);
        krapulamittari.MaxValue = PELIN_KESTO;
        krapulamittari.LowerLimit += delegate ()
        {
            KrapulaVoitti(4.5,
                "Voi rähmä! Liikaa kompurointia ja \n" +
                "liian vähän yöpaloja - Napsu ei \n" +
                "selvinnyt kotiin. Peli alkaa hetken \n" +
                "kuluttua alusta. Onnea matkaan!");
        };
        Label otsikko = new("Hilpeysmittari");
        otsikko.X = Screen.Left + 120;
        otsikko.Y = Screen.Top - 120;
        Add(otsikko);

        ProgressBar hilpeystaso = new(otsikko.Width, 20, krapulamittari);
        hilpeystaso.X = otsikko.X;
        hilpeystaso.Y = otsikko.Y - hilpeystaso.Height;
        hilpeystaso.BorderColor = Color.Black;
        hilpeystaso.BarColor = Color.Teal;
        hilpeystaso.Color = Color.Silver;
        hilpeystaso.BindTo(krapulamittari);
        Add(hilpeystaso);

        PeliajanLaskenta();
    }


    /// <summary>
    /// Luodaan aikalaskuri. Ajan kulumisen vaikutus pistelaskuriin on
    /// negatiivinen: kukin sekunti vähentää yhden pisteyksikön.
    /// </summary>
    private void PeliajanLaskenta()
    {
        Timer peliaika = new();
        peliaika.Interval = PELIN_KESTO;
        peliaika.Timeout += delegate ()
        {
            KrapulaVoitti(4.5,
                "Voi rähmä! Aika loppui ja lasku- \n" +
                "humala uuvutti Napsun. Peli alkaa \n" +
                "hetken kuluttua alusta. Onnea matkaan!");
        };
        peliaika.Start(1);

        Timer aikapistevahennys = new();
        aikapistevahennys.Interval = 1;
        aikapistevahennys.Timeout += delegate
        {
            krapulamittari.Value--;
            loppupisteetAjasta.Add(aikapistevahennys.Interval);
        };
        aikapistevahennys.Start();
    }


    /// <summary>
    /// Game over -aliohjelma. Tulostaa näytölle ilmoituksen jonka sisältö riippuu pelin
    /// päättymisen syystä (aika loppu tai kokonaispisteet nollassa) ja käynnistää pelin uudestaan.
    /// </summary>
    /// <param name="viiveAloitukseen">Ajastimen viive; aika jonka teksti näkyy ruudulla.</param>
    /// <param name="gameoverTeksti">Ilmoitus, joka näytetään pelaajalle.</param>
    public void KrapulaVoitti(double viiveAloitukseen, string gameoverTeksti)
    {
        TekstikenttaKeskelleRuutua(gameoverTeksti, viiveAloitukseen);
        Timer.SingleShot(viiveAloitukseen, AloitaAlusta);
    }


    /// <summary>
    /// Luodaan pelikenttä tekstitiedostosta ja viholliset aliohjelmasta,
    /// määritetään kamera ja kentän taustaväri.
    /// </summary>
    private void LuoKentta()
    {
        Level.Background.Color = Color.FromHexCode("86592d");

        TileMap kentta = TileMap.FromLevelAsset("kentta");
        kentta.SetTileMethod('-', LuoNurmikko);
        kentta.SetTileMethod('x', LuoReunat);
        kentta.SetTileMethod('s', LuoSnack, yopalat, "snack");
        kentta.SetTileMethod('e', LuoEste, "este");
        kentta.SetTileMethod('P', LuoTalo, "lähtö");
        kentta.SetTileMethod('K', LuoTalo, "maali");
        kentta.SetTileMethod('N', LuoTalo, "naapuri");
        kentta.SetTileMethod('i', LuoUkkeli);
        kentta.Optimize('-');
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);

        Timer vihutKentalle = new();
        vihutKentalle.Interval = RandomGen.NextDouble(3, 8);
        vihutKentalle.Timeout += delegate () { LuoVihu(); };
        vihutKentalle.Start();

        Camera.ZoomToLevel();
        Camera.StayInLevel = true;
        //  Camera.Zoom(2.7);
        //  Camera.Follow(ukkeli);
    }


    /// <summary>
    /// Luodaan kentän olioden perusrunko. Kutsutaan kentän luomisen aliohjelmista.
    /// </summary>
    /// <param name="paikka">Olion sijainti kentällä.</param>
    /// <param name="leveys">Olion leveys (ruudun koko).</param>
    /// <param name="korkeus">Olion korkeus (ruudun koko).</param>
    /// <param name="muoto">Olion muoto.</param>
    /// <returns>Fysiikkaolio.</returns>
    private static PhysicsObject LuoOlio(Vector paikka, double leveys, double korkeus, Shape muoto)
    {
        PhysicsObject olio = new(leveys, korkeus);
        olio.Position = paikka;
        olio.Shape = muoto;
        return olio;
    }


    /// <summary>
    /// Luodaan kentän taustalla näkyvä vihreä nurmialue.
    /// </summary>
    /// <param name="paikka">Sijainti kentällä.</param>
    /// <param name="leveys">Olion leveys.</param>
    /// <param name="korkeus">Olion korkeus.</param>
    private void LuoNurmikko(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject nurmikko = LuoOlio(paikka, leveys, korkeus, Shape.Rectangle);
        nurmikko.Color = Color.FromHexCode("006600");
        nurmikko.MakeStatic();
        nurmikko.CollisionIgnoreGroup = 1;
        Add(nurmikko);
    }


    /// <summary>
    /// Luodaan kentälle reunat, joihin voi törmätä.
    /// </summary>
    /// <param name="paikka">Sijainti kentällä.</param>
    /// <param name="leveys">Olion leveys.</param>
    /// <param name="korkeus">Olion korkeus.</param>
    private void LuoReunat(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject reunat = LuoOlio(paikka, leveys, korkeus, Shape.Rectangle);
        reunat.IsVisible = false;
        reunat.Tag = "reunat";
        reunat.MakeStatic();
        Add(reunat);
    }


    /// <summary>
    /// Luodaan yöpalat, joita pelaaja kerää matkallaan kotiin.
    /// Lisätään snackit yopalat-listaan.
    /// </summary>
    /// <param name="paikka">Sijainti kentällä.</param>
    /// <param name="leveys">Olion leveys.</param>
    /// <param name="korkeus">Olion korkeus.</param>
    /// <param name="yopalat">Lista, jolle yöpalat lisätään luomisen yhteydessä.</param>
    /// <param name="tag">Snackin tag-tunnus.</param>
    private void LuoSnack(Vector paikka, double leveys, double korkeus, List<PhysicsObject> yopalat, string tag)
    {
        PhysicsObject snack = LuoOlio(paikka, leveys, korkeus, Shape.Circle);
        snack.Tag = tag;
        snack.CanRotate = false;
        yopalat.Add(snack);
        Add(snack);

        foreach (PhysicsObject herkku in yopalat)
        {
            herkku.Image = LoadImage("snack" + RandomGen.NextInt(0, 8));
        }
    }


    /// <summary>
    /// Luodaan esteet, joihin pelihahmo voi törmätä matkallaan kotiin.
    /// </summary>
    /// <param name="paikka">Sijainti kentällä.</param>
    /// <param name="leveys">Olion leveys.</param>
    /// <param name="korkeus">Olion korkeus.</param>
    /// <param name="tag">Esteiden tag-tunnus.</param>
    /// 
    private void LuoEste(Vector paikka, double leveys, double korkeus, string tag)
    {
        Image[] esteet = { LoadImage("paali"), LoadImage("kottari") };

        PhysicsObject este = LuoOlio(paikka, leveys * 1.4, korkeus, Shape.Circle);
        este.MakeStatic();
        este.Tag = tag;
        este.Image = RandomGen.SelectOne<Image>(esteet[0], esteet[1]);
        Add(este);
    }


    /// <summary>
    /// Luodaan kentällä olevat paikallaan pysyvät rakennukset.
    /// </summary>
    /// <param name="paikka">Sijainti kentällä.</param>
    /// <param name="leveys">Olion leveys.</param>
    /// <param name="korkeus">Olion korkeus.</param>
    /// <param name="tag">Rakennustyypit toisistaan erottava tag.</param>
    private void LuoTalo(Vector paikka, double leveys, double korkeus, string tag)
    {
        Image[] talot = { LoadImage("pubi"), LoadImage("koti"), LoadImage("naapurikanto"), LoadImage("naapurikivi"), LoadImage("naapuriruusut"), LoadImage("naapurithatch") };

        PhysicsObject talo = LuoOlio(paikka, leveys * 4, korkeus * 4, Shape.Diamond);
        talo.Tag = tag;
        talo.MakeStatic();
        if (talo.Tag.ToString() == "lähtö") talo.Image = talot[0];
        if (talo.Tag.ToString() == "maali") talo.Image = talot[1];
        if (talo.Tag.ToString() == "naapuri") talo.Image = RandomGen.SelectOne<Image>(talot[2], talot[3], talot[4], talot[5]);
        talo.CollisionIgnoreGroup = 1;
        Add(talo, 1);
    }


    /// <summary>
    /// Luodaan pelattava hahmo. Kutsutaan törmäyskäsittelijää ja ohjainasetuksia.
    /// </summary>
    /// <param name="paikka">Pelihahmon lähtöpaikka.</param>
    /// <param name="leveys">Pelihahmon leveys</param>
    /// <param name="korkeus">Pelihahmon korkeus</param>
    private void LuoUkkeli(Vector paikka, double leveys, double korkeus)
    {
        ukkeli = new PlatformCharacter(leveys * 0.9, korkeus);
        ukkeli.Position = paikka;
        ukkeli.CanRotate = false;
        ukkeli.Tag = "ukkeli";
        ukkeli.Image = LoadImage("napsu");
        Add(ukkeli);

        AsetaOhjaimet();
        AddCollisionHandler<PlatformCharacter, PhysicsObject>(ukkeli, PelaajaTormasi);
    }


    /// <summary>
    /// Pelattavan hahmon ohjainkäskyt ja muut näppäinkomennot, pelistä poistuminen.
    /// </summary>
    private void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Up, ButtonState.Down, Liikuta, "Liikuta Napsua ylöspäin", ukkeli, new Vector(0, LIIKKUMISNOPEUS));
        Keyboard.Listen(Key.Up, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);
        Keyboard.Listen(Key.Down, ButtonState.Down, Liikuta, "Liikuta Napsua alaspäin", ukkeli, new Vector(0, -LIIKKUMISNOPEUS));
        Keyboard.Listen(Key.Down, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);

        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaJaKaanna, "Liikuta Napsua vasemmalle", ukkeli, -LIIKKUMISNOPEUS);
        Keyboard.Listen(Key.Left, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaJaKaanna, "Liikuta Napsua oikealle", ukkeli, LIIKKUMISNOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);

        Keyboard.Listen(Key.Space, ButtonState.Pressed, NaytaOhjevalikko, "Pidä tauko");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
    }


    /// <summary>
    /// Pelattavan hahmon liikkuminen ylös ja alas
    /// </summary>
    /// <param name="ukkeli">Pelihahmo</param>
    /// <param name="suunta">Liikkeen suunta (ylös, alas)</param>
    private void Liikuta(PlatformCharacter ukkeli, Vector suunta)
    {
        ukkeli.Velocity = suunta;
    }


    /// <summary>
    /// Pelattavan hahmon liikkuminen sivusuunnassa
    /// </summary>
    /// <param name="ukkeli">Pelihahmo</param>
    /// <param name="suunta">Liikkeen suunta. Kuva kääntyy nuolinäppäimen 
    ///  suuntaisesti vasemmalle tai oikealle.</param>
    private void LiikutaJaKaanna(PlatformCharacter ukkeli, double suunta)
    {
        ukkeli.Walk(suunta);
    }


    /// <summary>
    /// Aliohjelma pelihahmon ja muiden kohteiden välisen törmäyksen käsittelyyn.
    /// Ks. kommentit, joissa esitellään lyhyesti kunkin tilanteen käsittelyn päätapahtumat.
    /// </summary>
    /// <param name="ukkeli">Pelihahmo.</param>
    /// <param name="kohde">Kohde, johon törmättiin.</param>
    private void PelaajaTormasi(PlatformCharacter ukkeli, PhysicsObject kohde)
    {
        // Esteeseen törmäämisen käsittely.
        // Vaikutus krapulamittariin negatiivinen.
        if (kohde.Tag.ToString() == "este")
        {
            Label osuitEsteeseen = new Label("Hupsis, osuit!");
            osuitEsteeseen.Position = new Vector(Screen.Left + 120, Screen.Top - 180);
            osuitEsteeseen.Color = Color.Transparent;
            osuitEsteeseen.TextColor = Color.Black;
            osuitEsteeseen.LifetimeLeft = TimeSpan.FromSeconds(0.8);
            Add(osuitEsteeseen);

            krapulamittari.Value--;
        }

        // Herkkuun törmäämisen käsittely.
        // Herkku katoaa kentältä ja poistetaan yopalat-listasta.
        // Vaikutus krapulamittariin positiivinen.
        if (kohde.Tag.ToString() == "snack")
        {
            krapulamittari.Value++;
            yopalat.RemoveAt(0);
            kohde.Destroy();
        }

        // Naapuriin törmäämisen käsittely.
        // Naapuri katoaa kentältä. Vaikutus krapulamittariin negatiivinen.
        if (kohde.Tag.ToString() == "vihu")
        {
            krapulamittari.Value -= 3;
            kohde.Destroy();
        }

        // Kentän reunaan törmäämisen käsittely.
        // Tulostetaan teksti, ja alustetaan peli uudestaan.
        if (kohde.Tag.ToString() == "reunat")
        {
            TekstikenttaKeskelleRuutua("Hups! Taisit eksyä. Peli alkaa alusta tuokion kuluttua.", 4.0);
            Timer.SingleShot(4.0, AloitaAlusta);
        }

        // Pubiin törmäämisen käsittely.
        // Tulostetaan teksti.
        if (kohde.Tag.ToString() == "lähtö")
        {
            TekstikenttaKeskelleRuutua("Taverna on kiinni! Koti on toisessa suunnassa.", 2.0);
        }

        // Maaliin saapuminen.
        // Musiikki loppuu, loppufanfaari, pelaaja tuhoutuu.
        // Kutsutaan funktioita loppupisteiden laskemiseksi.
        // Tulostetaan kerättyjen herkkujen määrästä riippuva viesti.
        if (kohde.Tag.ToString() == "maali")
        {
            IsPaused = true;

            MediaPlayer.Stop();
            SoundEffect loppufanfaari = LoadSoundEffect("tultiinMaaliin");
            loppufanfaari.Play();
            ukkeli.Destroy();

            double pelaajanAikapisteet = PelaajanPeliaika(PELIN_KESTO, loppupisteetAjasta);
            double loppupojotskit = Loppupisteet(SNACKIT_ALUSSA, pelaajanAikapisteet);
            string maaliFiilikset = KerattiinkoKaikki(yopalat, SNACKIT_ALUSSA, loppupojotskit);
            TekstikenttaKeskelleRuutua(maaliFiilikset, 4.5);
        }
    }


    /// <summary>
    /// Funktio laskee, kuinka monta sekuntia eli yksikköä krapulamittarista
    /// vähenee pelin aikana. Käytetään lopullisten pisteiden määrittämisessä.
    /// </summary>
    /// <param name="kokoPelinKestoSek">Pelin kokonaiskesto sekunteina.</param>
    /// <param name="kaytetytSekuntit">Liukulukulista, johon on kerätty tieto 
    ///     siitä, kuinka monta sekuntia pelaaja on kuluttanut pelin aikana.</param>
    /// <returns>Jäljelle jäänyt peliaika, eli pelaajan ansaitsemat aikapisteet.</returns>
    private static double PelaajanPeliaika(double kokoPelinKestoSek, List<double> kaytetytSekuntit)
    {
        double pelaajanKayttamaAika = 0;

        for (int i = 0; i < kaytetytSekuntit.Count; i++)
        {
            pelaajanKayttamaAika += kaytetytSekuntit[i];
        }

        return kokoPelinKestoSek - pelaajanKayttamaAika;
    }


    /// <summary>
    /// Funktio tarkistaa, kerättiinkö pelin aikana kaikki herkut.
    /// </summary>
    /// <param name="yopalalista">Lista, jolta snackit poistetaan sitä mukaan kuin niitä kerätään.</param>
    /// <param name="snackitAlussa">Yöpalojen lukumäärä pelin alussa (kentta.txt -tiedoston 's' merkkien lkm).</param>
    /// <returns>Merkkijono, jonka sisältö riippuu kerättyjen yöpalojen lukumäärästä.</returns>
    private static string KerattiinkoKaikki(List<PhysicsObject> yopalalista, double snackitAlussa, double loppupisteet)
    {
        string maalissa;
        if (yopalalista.Count == 0)
        {
            maalissa = "Huippujuttu, löysit kaikki yöpalat! \n " +
                "Napsu sai vahvan alun päiväänsä ja \n" +
                "sinä sait " + snackitAlussa / 2 + " lisäpistettä! Yhteensä \n" +
                "kerrytit " + loppupisteet + " pistettä. Wau!";
        }
        else maalissa = "Kotiin selvitty, hyvä! Keräsit " + (snackitAlussa - yopalalista.Count) + " yöllistä herkkupalaa \n ja sait yhteensä " + loppupisteet + " pistettä. Huippua!";
        return maalissa;
    }


    /// <summary>
    /// Funktio laskee lopulliset pisteet pelaajalle. Parametrien lisäksi käytetään yopalat-listaa.
    /// </summary>
    /// <param name="snackitAlussa">Yöpalojen lukumäärä pelin alussa (kentta.txt -tiedoston 's' merkkien lkm).</param>
    /// <param name="aikapisteet">Jäljelle jäänyt peliaika, eli pelaajan ansaitsemat aikapisteet. Laskettu funktiossa PelaajanPeliaika.</param>
    /// <returns>Pelaajan lopullinen pistemäärä double-lukuna.</returns>
    private double Loppupisteet(double snackitAlussa, double aikapisteet)
    {
        double keratytSnackit = snackitAlussa - yopalat.Count;
        double peruspisteet = aikapisteet + keratytSnackit;
        if (yopalat.Count != 0) return peruspisteet;

        double huippupisteet = peruspisteet + snackitAlussa / 2;
        return huippupisteet;
    }


    /// <summary>
    /// Luodaan tekstikenttä keskelle kuvaruutua viestien näyttämiseksi lyhytaikaisesti.
    /// Kutsutaan törmäyksien yhteydessä ja pelin lopussa pisteiden näyttämiseksi.
    /// </summary>
    /// <param name="sisalto">Näytettävä teksti.</param>
    /// <param name="nakyvyysaika">Aika sekunteina, jonka tekstikenttä on näkyvissä.</param>
    private void TekstikenttaKeskelleRuutua(string sisalto, double nakyvyysaika)
    {
        Label infoteksti = new(RUUDUN_KOKO * 20, RUUDUN_KOKO * 5);
        infoteksti.Position = new Vector(0, 0);
        infoteksti.Color = Color.FromHexCode("006600");
        infoteksti.TextColor = Color.Black;
        infoteksti.BorderColor = Color.Silver;
        infoteksti.Text = sisalto;
        infoteksti.LifetimeLeft = TimeSpan.FromSeconds(nakyvyysaika);
        Add(infoteksti);
    }


    /// <summary>
    /// Luodaan "viholliset" eli kentällä kulkevat naapurit. Lähellä seuraaja-aivot, kaukana 
    /// satunnaisliikkuja-aivot. Sijainti luomisen yhteydessä on satunnainen.
    /// </summary>
    private void LuoVihu()
    {
        PhysicsObject vihu = new(RUUDUN_KOKO, RUUDUN_KOKO, Shape.Circle);
        vihu.CanRotate = false;
        vihu.Tag = "vihu";

        Vector vihunSijainti;
        do
        {
            vihunSijainti = RandomGen.NextVector(Level.BoundingRect);
        }
        while (Vector.Distance(vihunSijainti, ukkeli.Position) < RUUDUN_KOKO * 2);
        vihu.Position = vihunSijainti;

        Image[] vihuliinit = { LoadImage("vihu1"), LoadImage("vihu2") };
        vihu.Image = RandomGen.SelectOne<Image>(vihuliinit[0], vihuliinit[1]);

        RandomMoverBrain kaukoaivot = new RandomMoverBrain(RUUDUN_KOKO * 2);
        kaukoaivot.ChangeMovementSeconds = 3;

        FollowerBrain aivot = new FollowerBrain(ukkeli);
        aivot.DistanceFar = 8 * RUUDUN_KOKO;
        aivot.Speed = RUUDUN_KOKO * 2;
        aivot.FarBrain = kaukoaivot;
        vihu.Brain = aivot;

        Add(vihu);
    }
}