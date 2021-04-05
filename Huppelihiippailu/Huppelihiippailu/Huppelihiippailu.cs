using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;


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

    /// <summary>
    /// Alkuvalikko, ohjeteksti ja aliohjelmakutsu pelin aloittamiseksi.
    /// </summary>
    public override void Begin()
    {
      //  SetWindowSize(1450, 900/*, true*/);
     

        //    IsMouseVisible = true;
        //    Level.Background.Image = 
        //        Image.FromGradient(1450, 900,
        //        new Color(0, 102, 51),
        //        new Color(0, 153, 76));
        //    MultiSelectWindow alkuvalikko = new MultiSelectWindow("Huppelihiippailun alkuvalikko", "Aloita peli", "Parhaat pisteet", "Lopeta");
        //    alkuvalikko.X = -160;
        //    alkuvalikko.Y = 0;
        //    Add(alkuvalikko);
        //    alkuvalikko.AddItemHandler(0, AloitaAlusta);
        //    alkuvalikko.AddItemHandler(1, ParhaatPisteet);
        //    alkuvalikko.AddItemHandler(2, Exit);
        //    alkuvalikko.DefaultCancel = 2;
        //    
        //    Label alkuinfo = new Label();
        //    alkuinfo.Position = new Vector(alkuvalikko.Right + 200, 0);
        //    alkuinfo.Color = Color.Transparent;
        //    alkuinfo.TextColor = new Color(184, 220, 202);
        //    alkuinfo.Text = 
        //        "Hupsis! Napsun pubi-ilta ystävien \n" +
        //        "kanssa venähti pikkutunneille, ja \n" +
        //        "nyt on aika suunnata kotiin. \n" +
        //        "Onneksi on leppeä kesäyö ja \n" +
        //        "kotimatkalla mieltä ilahduttavat \n" +
        //        "menomatkalla piilotetut herkut. \n" +
        //        "Vie Napsu kotiin ennen kuin \n" +
        //        "krapula iskee!";
        //    Add(alkuinfo);

        AloitaAlusta();
    }


    /// <summary>
    /// Tyhjentää pelin ja alustaa sen alkamaan alusta.
    /// </summary>
    void AloitaAlusta()
    {
        ClearAll();
        IsMouseVisible = true;
        LuoKentta();
        LuoKrapulamittari();
    }


    /// <summary>
    /// Luodaan pisteitä ja aikaa mittaava laskuri.
    /// </summary>
    void LuoKrapulamittari()
    {
        krapulamittari = new IntMeter(15); // mittarin lähtöarvo
        krapulamittari.MaxValue = 15;
        krapulamittari.LowerLimit += KrapulaVoitti;

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


    void PeliajanLaskenta()
    {
        // kokonaispeliaika, lopullisesti esim joku 2 min?
        Timer peliaika = new Timer();
        peliaika.Interval = 15;         // tähän se aika sekunteina, tsekkaa aikapistevähennys.start
        peliaika.Timeout += KrapulaVoitti;
        peliaika.Start(1);
    
        Timer aikapistevahennys = new Timer();
        aikapistevahennys.Interval = 3;      // 3 sekuntia aikaa
        aikapistevahennys.Timeout += delegate { krapulamittari.Value--; };
        aikapistevahennys.Start();  
    }
    
    /// <summary>
    /// TÄSTÄ PITÄIS SAADA TEHTYÄ SELLANEN GAME OVER -ALIOHJELMA.
    /// </summary>
    void KrapulaVoitti()
    {
         
        
        MessageDisplay.Add("O ou, taisi laskuhumala viedä voimat. \n Peli päättyi!");
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
        kentta.SetTileMethod('-', LuoNurmikko, "nurmi");
        kentta.SetTileMethod('x', LuoReunat);
        kentta.SetTileMethod('s', LuoSnack, "puteli");
        kentta.SetTileMethod('y', LuoSnack, "yopala");
        kentta.SetTileMethod('k', LuoEste, "kottari");
        kentta.SetTileMethod('p', LuoEste, "paali");
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
        
        // luodaan suojavyöhyke ukkelin ympärille; vihut luodaan girdlen ulkopuolelle
        double ylakulmaX = ukkeli.Position.X + 2 * RUUDUN_KOKO;
        double ylakulmaY = ukkeli.Position.Y + 2 * RUUDUN_KOKO;
        double alakulmaX = ukkeli.Position.X - 2 * RUUDUN_KOKO;
        double alakulmaY = ukkeli.Position.Y - 2 * RUUDUN_KOKO;

        double girdle = Varoetaisyys(ylakulmaX, ylakulmaY, alakulmaX, alakulmaY);
//        double maksimi = Maksimietaisyys(ukkeli.Position.X, ukkeli.Position.Y);
       
        Timer vihujenLisaaminen = new Timer();
        vihujenLisaaminen.Interval = RandomGen.NextDouble(20, 40);
        vihujenLisaaminen.Timeout += delegate { LisaaVihu(girdle/*, maksimi*/); };
        vihujenLisaaminen.Start();
    }


    /// <summary>
    /// Luodaan kentän taustalla näkyvä vihreä nurmialue.
    /// </summary>
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Leveys</param>
    /// <param name="korkeus">Korkeus</param>
    void LuoNurmikko(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject nurmikko = PhysicsObject.CreateStaticObject(leveys, korkeus);
        nurmikko.Position = paikka;
        nurmikko.Shape = Shape.Rectangle;
        nurmikko.Color = Color.FromHexCode("006600");
        //    nurmikko.Image = LoadImage(kuvanNimi);
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
    void LuoSnack(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject snack = new PhysicsObject(leveys * 0.5, korkeus);
        snack.Position = paikka;
        snack.Tag = "snack";
        snack.Image = LoadImage(kuvanNimi);
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
        PhysicsObject este = new PhysicsObject(leveys, korkeus * 0.8);
        este.Position = paikka;
        este.Tag = "este";
        este.MakeStatic();
        este.Image = LoadImage(kuvanNimi);
        Add(este);
    }

    static double Varoetaisyys(double ylakulmaX, double ylakulmaY, double alakulmaX, double alakulmaY)
    {
        Vector oikeaYlakulma = new Vector(ylakulmaX, ylakulmaY);
        Vector vasenAlakulma = new Vector(alakulmaX, alakulmaY);
        double varoetaisyys = Vector.Distance(oikeaYlakulma, vasenAlakulma);
        
        return varoetaisyys;
    }

    static double Maksimietaisyys(double ukkeliX, double ukkeliY)
    {
        Vector oikeaYlakulma = new Vector(ukkeliX + 8 * RUUDUN_KOKO, ukkeliY + 8 * RUUDUN_KOKO);
        Vector vasenAlakulma = new Vector(ukkeliX - 8 * RUUDUN_KOKO, ukkeliY - 8 * RUUDUN_KOKO);
        double maksimietaisyys = Vector.Distance(oikeaYlakulma, vasenAlakulma);

        return maksimietaisyys;
    }


    void LisaaVihu(double varoetaisyys/*, double enimmaisetaisyys*/)
    {
        Image[] vihut = { LoadImage("vihu1"), LoadImage("vihu2") };
        PhysicsObject vihu = new PhysicsObject(RUUDUN_KOKO, RUUDUN_KOKO, Shape.Circle);
        vihu.CanRotate = false;
        vihu.Image = RandomGen.SelectOne<Image>(vihut[0], vihut[1]);
        vihu.Tag = "vihu";
        Vector vihunSijainti;
        do
        {
            vihunSijainti = RandomGen.NextVector(Level.BoundingRect); //(ukkeli.Position.X, enimmaisetaisyys);
        }
        while (Vector.Distance(vihunSijainti, ukkeli.Position) > varoetaisyys);

        vihu.Position = vihunSijainti;

        //  RandomMoverBrain aivot = new RandomMoverBrain(100);  // liikenopeus 100
        //  aivot.ChangeMovementSeconds = 2;  // vaihtaa suuntaa 2 sek välein
        //  aivot.WanderRadius = 200;
        //  vihu.Brain = aivot;

        FollowerBrain aivot = new FollowerBrain(ukkeli);
        aivot.DistanceFar = 8 * RUUDUN_KOKO; // missä aletaan seurata (sama ku maksimi, poistetaanko maksimi?)
        aivot.DistanceClose = RUUDUN_KOKO;
        aivot.Speed = 50;
     //   aivot.TargetClose += NaapuriMorottaa;
        vihu.Brain = aivot;
    
        Add(vihu);

    }


    void NaapuriMorottaa()
    {
        TekstikenttaKeskelleRuutua("Huomenta! \n Heitä läpsy ja jatka matkaa.", 1.0);
    }








    /// <summary>
    /// Luodaan kentällä olevat paikallaan pysyvät rakennukset
    /// </summary>
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Leveys</param>
    /// <param name="korkeus">Korkeus</param>
    /// <param name="vari">Väri</param>
    void LuoTalo(Vector paikka, double leveys, double korkeus, string tag)
    {
        Image[] talot = { LoadImage("pubi"), LoadImage("koti"), LoadImage("naapurikanto"), LoadImage("naapurikivi"), LoadImage("naapurithatch"), LoadImage("naapuriruusut") };

        PhysicsObject talo = PhysicsObject.CreateStaticObject(leveys * 4, korkeus * 4);
        talo.Position = paikka;
        talo.Tag = tag;
        talo.Shape = Shape.Diamond;
        if (talo.Tag.ToString() == "lähtö") talo.Image = talot[0];
        if (talo.Tag.ToString() == "maali") talo.Image = talot[1];
        if (talo.Tag.ToString() == "naapuri") talo.Image = RandomGen.SelectOne<Image>(talot[2], talot[3], talot[4], talot[5]);
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
    /// Aliohjelma pelihahmon ja muiden kohteiden törmäyksen käsittelyyn.
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

            List<PhysicsObject> osututEsteet = new List<PhysicsObject>();
            osututEsteet.Add(kohde);
        }

        if (kohde.Tag.ToString() == "snack")
        {
            krapulamittari.Value++;
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
            TekstikenttaKeskelleRuutua("Hienosti! Voitit pelin!", 4.5);
            pojomestarit.EnterAndShow(krapulamittari.Value);
        }
    }

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
    /// Pelattavan hahmon ja yleiset ohjainkäskyt, pelistä poistuminen.
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
    /// <param name="suunta">???</param>
    void LiikutaJaKaanna(PlatformCharacter ukkeli, double suunta)
    {
       // ukkeli.Velocity = suunta;
        //Animation.ukkeliVasen = Animation.Mirror(ukkeli);
       // ukkeli.MirrorImage();
        ukkeli.Walk(suunta);
    }

}