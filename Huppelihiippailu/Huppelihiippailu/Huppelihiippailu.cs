using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using Jypeli.Effects;


/* TO DO
 * grafiikat (taustat, objektit)
 *      nurmikko ja polku vain väreinä
 * vihujen kääntyminen
 *      vihujen kuvat mirror? taulukon alkioiden mirrorointi?
 * "kyltit"  < perikato | koti >
 * pelin päättyminen?
 *      kotiinpääsy/maali
 *      G.O. uusi screen "wasted", KrapulaVoitti-aliohjelma
 *      voitto uusi screen + grafiikat, SelvisitMaaliin-aliohjelma
 * nice-to-have: pensasasiat piilopullojen eteen
 * nice-to-have: naapurit (vihu)
 *      mikä rooli? miten "tapellaan"? miten niistä pääsee eroon?
 *      ilmestyminen (esim kamera-alueen ylänurkasta eikä täysin randomisti)
 * nice-to-have: humalainen kävely
 * TARKISTA
 *      LuoReunat
 *      snack kuvat
 *      este kuvat
 *      naapuritalojen kuvat taulukkoon ja silmukkaan?
 */

/// @author saelmarj
/// @version 5.4.2021
/// <summary>
/// Huppelihiippailu-peli
/// </summary>
public class Huppelihiippailu : PhysicsGame
{
    PlatformCharacter ukkeli;

    const double LIIKKUMISNOPEUS = 300;
    const double RUUDUN_KOKO = 30;

    IntMeter krapulamittari;
    EasyHighScore pojomestarit = new EasyHighScore();
    double vahennetytSekuntit;

    List<PhysicsObject> yopalat = new List<PhysicsObject>();



    /// <summary>
    /// Alkuvalikko aliohjelmakutsu pelin aloittamiseksi.
    /// </summary>
    public override void Begin()
    {
        //        SetWindowSize(1450, 900/*, true*/);
        //        Level.Background.Image =
        //            Image.FromGradient(1450, 900,
        //            new Color(0, 102, 51),
        //            new Color(0, 153, 76));

        AloitaAlusta();
        //    IsPaused = true;
        //
        //    MultiSelectWindow alkuvalikko = new MultiSelectWindow("Huppelihiippailu", "Aloita uusi peli", "Näytä ohjeet", "Lopeta");
        //    alkuvalikko.Position = new Vector(0, 0);
        //    alkuvalikko.AddItemHandler(0, AloitaAlusta);
        //    alkuvalikko.AddItemHandler(1, NaytaOhjevalikko);
        //    alkuvalikko.AddItemHandler(2, ConfirmExit);
        //    alkuvalikko.DefaultCancel = 2;
        //    Add(alkuvalikko);
    }


    /// <summary>
    /// Tyhjentää pelin ja alustaa sen alkamaan alusta.
    /// Kentän ja pistelaskurin luominen.
    /// </summary>
    void AloitaAlusta()
    {
        IsMouseVisible = true;
        ClearAll();
        yopalat.Clear();
        LuoKentta();
        LuoKrapulamittari();
    }


    /// <summary>
    ///  Pelin ohjeet, johon pääsee alkuvalikosta tai keskeyttämällä pelin.
    ///  Paluu takaisin peliin, aloitus uudelleen, poistuminen pelistä.
    /// </summary>
    void NaytaOhjevalikko()
    {
        IsPaused = true;
        MultiSelectWindow ohjeet = new MultiSelectWindow
            ("Hupsis! Napsun pubi-ilta ystävien \n" +
               "kanssa venähti pikkutunneille, ja \n" +
               "nyt on aika suunnata kotiin. \n" +
               "Onneksi on leppeä kesäyö ja \n" +
               "kotimatkalla mieltä ilahduttavat \n" +
               "menomatkalla piilotetut herkut. \n" +
               "Kerää herkut, varo naapureita ja \n" +
               "vie Napsu kotiin ennen kuin aika \n" +
               "loppuu ja krapula iskee! Liikuta \n" +
               "Napsua nuolinäppäimillä, pidä \n" +
               "tauko painamalla välilyöntiä. \n",
               "Jatka peliä", "Uusi peli", "Lopeta");
        ohjeet.Position = new Vector(0, 0);
        ohjeet.AddItemHandler(0, delegate () { ohjeet.DefaultCancel = 0; });  // MIKÄ VIKA?? EI JATKA HETI PELIÄ, VAAN PITÄÄ PAINAA ESC UUDESTAAN PELISSÄ??
        ohjeet.AddItemHandler(1, AloitaAlusta);
        ohjeet.AddItemHandler(2, ConfirmExit);
        Add(ohjeet);
    }


    /// <summary>
    /// Luodaan pisteitä ja aikaa mittaava laskuri.
    /// </summary>
    void LuoKrapulamittari()
    {
        krapulamittari = new IntMeter(15); // mittarin lähtöarvo
        krapulamittari.MaxValue = 15;
        krapulamittari.LowerLimit += delegate ()
        {
            KrapulaVoitti(4.5,
                "Voi rähmä! Liikaa kompurointia ja \n" +
                "liian vähän yöpaloja - Napsu ei \n" +
                "selvinnyt kotiin. Peli alkaa hetken \n" +
                "kuluttua alusta. Onnea matkaan!");
        };
        Label otsikko = new Label("Hilpeysmittari");
        otsikko.X = Screen.Left + 120;
        otsikko.Y = Screen.Top - 120;
        Add(otsikko);

        ProgressBar hilpeystaso = new ProgressBar(otsikko.Width, 20, krapulamittari);
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
    /// Luodaan aikalaskuri. Vaikutus pistelaskuriin negatiivinen.
    /// </summary>
    void PeliajanLaskenta()
    {
        // kokonaispeliaika, lopullisesti esim joku 2 min?
        Timer peliaika = new Timer();
        peliaika.Interval = 15;         // tähän se aika sekunteina, tsekkaa aikapistevähennys.start
        peliaika.Timeout += delegate ()
        {
            KrapulaVoitti(4.5,
                "Voi rähmä! Aika loppui ja lasku- \n" +
                "humala uuvutti Napsun. Peli alkaa \n" +
                "hetken kuluttua alusta. Onnea matkaan!");
        };
        peliaika.Start(1);

        Timer aikapistevahennys = new Timer();
        aikapistevahennys.Interval = 3;      // 3 sekuntia aikaa, eli kaiketi 3 sek välein putoaa 1 yksikkö
        aikapistevahennys.Timeout += delegate { krapulamittari.Value--; };
        aikapistevahennys.Start();

        vahennetytSekuntit = MontakoKertaaAikapistevahennysTimerKaynnistyy(peliaika.Interval, aikapistevahennys.Interval);
    }


    /// <summary>
    /// Funktio laskee, kuinka monta sekuntia eli yksikköä krapulamittarista
    /// vähenee pelin aikana. Käytetään lopullisten pisteiden määrittämisessä.
    /// </summary>
    /// <param name="kokoPelinKestoSek">Pelin kokonaiskesto sekunteina.</param>
    /// <param name="ajanVaikutusPistelaskuriin">Montako sekuntia kunkin pisteyksikkövähennyksen välillä kuluu.</param>
    /// <returns>Liukuluku, joka kertoo kuinka monta kertaa ajastin käynnistyy, eli
    ///     kuinka monta pisteyksikköä krapulamittarista vähenee.</returns>
    double MontakoKertaaAikapistevahennysTimerKaynnistyy(double kokoPelinKestoSek, double ajanVaikutusPistelaskuriin)
    {
        return kokoPelinKestoSek / ajanVaikutusPistelaskuriin;
    }


    /// <summary>
    /// Game over -aliohjelma. Tulostaa näytölle ilmoituksen jonka sisältö riippuu pelin
    /// päättymisen syystä (aika loppu tai kokonaispisteet nollassa) ja käynnistää pelin uudestaan.
    /// </summary>
    /// <param name="viiveAloitukseen">Ajastimen viive; aika jonka teksti näkyy ruudulla.</param>
    /// <param name="gameoverTeksti">Ilmoitus, joka näytetään pelaajalle.</param>
    void KrapulaVoitti(double viiveAloitukseen, string gameoverTeksti)
    {
        TekstikenttaKeskelleRuutua(gameoverTeksti, viiveAloitukseen);
        Timer.SingleShot(viiveAloitukseen, AloitaAlusta);
    }


    /// <summary>
    /// Luodaan pelikenttä tekstitiedostosta,
    /// määritetään peli-ikkunan koko ja kamera.
    /// </summary>
    void LuoKentta()
    {
        //        SetWindowSize(1450, 900/*, true*/);

        // Level.Height = 1500;
        // Level.Width = 3600;
        // Level.CreateBorders(0.0, true);

        Level.Background.Color = Color.FromHexCode("86592d");

        TileMap kentta = TileMap.FromLevelAsset("kentta");
        kentta.SetTileMethod('-', LuoNurmikko);
        kentta.SetTileMethod('x', LuoReunat);
        kentta.SetTileMethod('s', LuoSnack, "puteli", yopalat);
        kentta.SetTileMethod('e', LuoEste, "paali");
        kentta.SetTileMethod('P', LuoTalo, "lähtö"); // pubi eli lähtö
        kentta.SetTileMethod('K', LuoTalo, "maali"); // koti eli maali
        kentta.SetTileMethod('N', LuoTalo, "naapuri"); // naapuri
        kentta.SetTileMethod('i', LuoUkkeli);
        kentta.Optimize('-');
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);


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
    PhysicsObject LuoOlio(Vector paikka, double leveys, double korkeus, Shape muoto)
    {
        PhysicsObject olio = new PhysicsObject(leveys, korkeus);
        olio.Position = paikka;
        olio.Shape = muoto;
        return olio;
    }


    /// <summary>
    /// Luodaan kentän taustalla näkyvä vihreä nurmialue.
    /// </summary>
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Leveys</param>
    /// <param name="korkeus">Korkeus</param>
    void LuoNurmikko(Vector paikka, double leveys, double korkeus)
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
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Leveys</param>
    /// <param name="korkeus">Korkeus</param>
    void LuoReunat(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject reunat = new PhysicsObject(0.1, 0.1);
        reunat.Position = paikka;
        reunat.Tag = "reunat";
        Add(reunat);
    }


    /// <summary>
    /// Luodaan salaiset yöpalat, joita hahmo kerää matkallaan kotiin.
    /// </summary>
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Leveys</param>
    /// <param name="korkeus">Korkeus</param>
    /// 
    void LuoSnack(Vector paikka, double leveys, double korkeus, string kuvanNimi, List<PhysicsObject> yopalat)
    {
        PhysicsObject snack = LuoOlio(paikka, leveys * 0.5, korkeus, Shape.Circle);
        snack.Tag = "snack";
        snack.Image = LoadImage(kuvanNimi);
        yopalat.Add(snack);
        Add(snack);
    }


    /// <summary>
    /// Luodaan esteet, joihin pelihahmo voi törmätä matkallaan kotiin.
    /// </summary>
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Leveys</param>
    /// <param name="korkeus">Korkeus</param>
    void LuoEste(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject este = LuoOlio(paikka, leveys, korkeus * 0.8, Shape.Circle);
        este.MakeStatic();
        este.Tag = "este";
        este.Image = LoadImage(kuvanNimi);
        Add(este);
    }


    /// <summary>
    /// Luodaan kentällä olevat paikallaan pysyvät rakennukset
    /// </summary>
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Leveys</param>
    /// <param name="korkeus">Korkeus</param>
    /// <param name="tag">Rakennustyypit toisistaan erottava tag</param>
    void LuoTalo(Vector paikka, double leveys, double korkeus, string tag)
    {
        Image[] talot = { LoadImage("naapuri1"), LoadImage("naapuri2"), LoadImage("naapuri3"), LoadImage("naapuri4") };

        PhysicsObject talo = LuoOlio(paikka, leveys * 4, korkeus * 4, Shape.Diamond);
        talo.Tag = tag;
        talo.MakeStatic();
        if (talo.Tag.ToString() == "lähtö") talo.Image = LoadImage("pubi");
        if (talo.Tag.ToString() == "maali") talo.Image = LoadImage("koti");
        if (talo.Tag.ToString() == "naapuri") talo.Image = RandomGen.SelectOne<Image>(talot[2], talot[3], talot[1], talot[0]);
        talo.CollisionIgnoreGroup = 1;
        Add(talo, 1);
    }


    /// <summary>
    /// Luodaan pelattava hahmo
    /// </summary>
    /// <param name="paikka">Hahmon lähtöpaikka</param>
    /// <param name="leveys">Hahmon leveys</param>
    /// <param name="korkeus">Hahmon korkeus</param>
    void LuoUkkeli(Vector paikka, double leveys, double korkeus)
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
    void AsetaOhjaimet()
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
    /// Pelattavan hahmon liikkuminen
    /// </summary>
    /// <param name="ukkeli">Pelihahmo</param>
    /// <param name="suunta">Liikkeen suunta (ylös, alas, suoraan sivulle)</param>
    void Liikuta(PlatformCharacter ukkeli, Vector suunta)
    {
        ukkeli.Velocity = suunta;
    }


    /// <summary>
    /// Pelattavan hahmon liikkuminen
    /// </summary>
    /// <param name="ukkeli">Pelihahmo</param>
    /// <param name="suunta">Liikkeen suunta. Kuva kääntyy nuolinäppäimen 
    ///  suuntaisesti vasemmalle tai oikealle.</param>
    void LiikutaJaKaanna(PlatformCharacter ukkeli, double suunta)
    {
        ukkeli.Walk(suunta);
    }


    /// <summary>
    /// Aliohjelma pelihahmon ja muiden kohteiden välisen törmäyksen käsittelyyn.
    /// </summary>
    /// <param name="ukkeli"> Pelihahmo </param>
    /// <param name="kohde"> Kohde, johon törmättiin </param>
    void PelaajaTormasi(PlatformCharacter ukkeli, PhysicsObject kohde)
    {
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

        if (kohde.Tag.ToString() == "snack")
        {
            krapulamittari.Value++;
            yopalat.RemoveAt(0);
            MessageDisplay.Add("listalla nyt " + yopalat.Count + " lukua");
            kohde.Destroy();
        }

        if (kohde.Tag.ToString() == "vihu")
        {
            krapulamittari.Value--;
            kohde.Destroy();
        }

        if (kohde.Tag.ToString() == "reunat")
        {
            ukkeli.Stop(); // ei toimi??

            TekstikenttaKeskelleRuutua("Hups! Taisit eksyä. Peli alkaa alusta tuokion kuluttua.", 4.0);
            Timer.SingleShot(4.0, AloitaAlusta);
        }

        if (kohde.Tag.ToString() == "lähtö")
        {
            TekstikenttaKeskelleRuutua("Taverna on kiinni! Koti on toisessa suunnassa.", 2.0);
        }

        if (kohde.Tag.ToString() == "maali")
        {
            IsPaused = true;

            Level.AmbientLight = -1.0;
            Light valo = new Light();
            valo.Intensity = 1.0;
            valo.Distance = 150;
            valo.Position = ukkeli.Position;
            Add(valo);


            // ukkeli.Destroy();  // tähän tilalle joku jehu animaatio? tai fanfaariäänitiedosto?
            string maaliFiilikset = KerattiinkoKaikki(yopalat);
            TekstikenttaKeskelleRuutua(maaliFiilikset, 4.5);

            Loppupisteet(20);


            //    int keratytLkm = Pistetilastot1(snackLkm);
            //    double miinuspojot = Pistetilastot2(esteLkm, vihuLkm);
            //    TekstikenttaKeskelleRuutua("Hienosti! Voitit pelin! \n Keräsit " + keratytLkm + " yöllistä herkkupalaa ja \n voimasi hupenivat " + miinuspojot + " iskun johdosta.", 4.5);
            //    pojomestarit.EnterAndShow(krapulamittari.Value);
        }

        //   Pistetilastot(snackosumat, esteosumat, vihuosumat);
    }


    /// <summary>
    /// Funktio tarkistaa, kerättiinkö pelin aikana kaikki snackit.
    /// </summary>
    /// <param name="yopalalista">Lista, jolta snackit poistetaan sitä mukaan kuin niitä kerätään.</param>
    /// <returns>Merkkijono, jonka sisältö riippuu kerättyjen yöpalojen lukumäärästä.</returns>
    static string KerattiinkoKaikki(List<PhysicsObject> yopalalista)
    {
        string maalissa;
        if (yopalalista.Count == 0) maalissa = "Huippujuttu, löysit kaikki yöpalat! \n Napsu sai vahvan alun päiväänsä ja \n sinä sait XXX lisäpistettä!";
        else maalissa = "Kotiin selvitty, hyvä!";
        return maalissa;
    }


    /// <summary>
    /// Funktio laskee pisteet käyttäjälle. Parametrin lisäksi hyödynnetään 
    /// krapulamittarin lukemaa ja pistelaskurista vähennettyjä sekunteja.
    /// </summary>
    /// <param name="snackitAlussa">Yöpalojen lukumäärä pelin alussa (kentta.txt -tiedoston 's' merkkien lkm). </param>
    /// <returns>Pelaajan lopullinen pistemäärä double-lukuna.</returns>
    double Loppupisteet(double snackitAlussa)
    {
        double keratytSnackit = snackitAlussa - yopalat.Count;
        double peruspisteet = krapulamittari.Value + vahennetytSekuntit + keratytSnackit;
        if (yopalat.Count != 0) return peruspisteet;
        return peruspisteet + snackitAlussa / 2;
    }


    /// <summary>
    /// Luodaan tekstikenttä keskelle kuvaruutua viestien näyttämiseksi lyhytaikaisesti.
    /// </summary>
    /// <param name="sisalto">Näytettävä teksti </param>
    /// <param name="nakyvyysaika">Aika sekunteina, jonka tekstikenttä on näkyvissä </param>
    void TekstikenttaKeskelleRuutua(string sisalto, double nakyvyysaika)
    {
        Label infoteksti = new Label(RUUDUN_KOKO * 20, RUUDUN_KOKO * 5);
        infoteksti.Position = new Vector(0, 0);
        infoteksti.Color = Color.DarkJungleGreen;
        infoteksti.TextColor = Color.Black;
        infoteksti.BorderColor = Color.Silver;
        infoteksti.Text = sisalto;
        infoteksti.LifetimeLeft = TimeSpan.FromSeconds(nakyvyysaika);
        Add(infoteksti);
    }


    /// <summary>
    /// Lasketaan pelihahmon ympärille luotava suojavyöhyke, 
    /// jolle ei voida luoda satunnaisia vihollisia.
    /// </summary>
    /// <param name="ylakulmaX">Vyöhykkeen yläreunan X-koordinaatti.</param>
    /// <param name="ylakulmaY">Vyöhykkeen yläreunan Y-koordinaatti.</param>
    /// <param name="alakulmaX">Vyöhykkeen alareunan X-koordinaatti.</param>
    /// <param name="alakulmaY">Vyöhykkeen alareunan X-koordinaatti.</param>
    /// <returns>Pelihahmon ja vihollisen välisen varoetäisyyden.</returns>
    static double Varoetaisyys(double ylakulmaX, double ylakulmaY, double alakulmaX, double alakulmaY)
    {
        Vector oikeaYlakulma = new Vector(ylakulmaX, ylakulmaY);
        Vector vasenAlakulma = new Vector(alakulmaX, alakulmaY);
        double varoetaisyys = Vector.Distance(oikeaYlakulma, vasenAlakulma);
        return varoetaisyys;
    }
}