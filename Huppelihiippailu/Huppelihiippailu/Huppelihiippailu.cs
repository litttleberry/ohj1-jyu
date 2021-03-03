using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;


/* TO DO
 * grafiikat (taustat, objektit)
 * esteiden paikat / liikkuminen? 
 * osuma reunaan/väärä suunta > siirrä B. alkuun
 * "kyltit"  < perikato | koti >
 * kotiinpääsy/maali
 * pelin päättyminen?
 *      G.O. uusi screen "wasted"
 *      voitto uusi screen + grafiikat
 * laskuri toimimaan -- aika ja ++/-- osumat
 * nice-to-have: pensasasiat piilopullojen eteen
 * nice-to-have: naapurit (vihu)
 * nice-to-have: humalainen kävely
 */


/// <summary>
/// Huppelihiippailu-peli
/// </summary>
public class Huppelihiippailu : PhysicsGame
{
    PhysicsObject ukkeli;

    const double LIIKKUMISNOPEUS = 300;
    const double RUUDUN_LEVEYS = 30;
    const double RUUDUN_KORKEUS = 30;

    DoubleMeter krapulamittari;
    
    public override void Begin()
    {
        LuoKentta();
        LuoKrapulamittari();

    }

    public void LuoKrapulamittari()
    {
        krapulamittari = new DoubleMeter(7); // lähtöarvo
        krapulamittari.MaxValue = 15;
        krapulamittari.LowerLimit += KrapulaVoitti;


        Label otsikko = new Label("Hilpeysmittari");
        otsikko.X = Screen.Left + 100;
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



    }

    
    public void KrapulaVoitti()
    {
         MessageDisplay.Add("O ou, taisi laskuhumala viedä voimat.");
    }

    public void LuoKentta()
    {
        SetWindowSize(1550, 1050);

        TileMap kentta = TileMap.FromLevelAsset("kentta");
        kentta.SetTileMethod('-', LuoNurmikko);
        kentta.SetTileMethod('o', LuoEste);
        kentta.SetTileMethod('s', LuoSnack);
        kentta.SetTileMethod('i', LuoUkkeli);
        kentta.SetTileMethod('P', LuoTalo, Color.DarkBrown); // pubi eli lähtö
        kentta.SetTileMethod('K', LuoTalo, Color.DarkRed); // koti eli maali
        kentta.SetTileMethod('N', LuoTalo, Color.Charcoal); // naapuri
        kentta.Execute(RUUDUN_LEVEYS, RUUDUN_KORKEUS);

        Level.CreateBorders();
        Level.BackgroundColor = Color.LightGray;

        Camera.ZoomToAllObjects();
        // Camera.StayInLevel = true;
        // Camera.Zoom(2);
        // Camera.Follow(ukkeli);

    }

    
    public void LuoSnack(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject snack = new PhysicsObject(30, 30);
        snack.Position = paikka;
        snack.Shape = Shape.Diamond;
        snack.Color = Color.Orange;
        snack.Tag = "snack";
        Add(snack);
    }


    public void LuoEste(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject este =  new PhysicsObject(30, 30);
        este.Position = paikka;
        este.Shape = Shape.Hexagon;
        este.Color = Color.Navy;
        este.Tag = "este";
        este.MakeStatic();
        Add(este);
    }
    

    public void LuoNurmikko(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject nurmikko = PhysicsObject.CreateStaticObject(leveys, korkeus);
        nurmikko.Position = paikka;
        nurmikko.Shape = Shape.Rectangle;
        nurmikko.Color = Color.DarkJungleGreen;
        Add(nurmikko);
    }


    /// <summary>
    /// Luodaan kentällä olevat staattiset rakennukset
    /// </summary>
    /// <param name="paikka">Paikka</param>
    /// <param name="leveys">Korkeus</param>
    /// <param name="korkeus">Leveys</param>
    /// <param name="vari">Väri</param>
    public void LuoTalo(Vector paikka, double leveys, double korkeus, Color vari)
    {
        PhysicsObject talo = PhysicsObject.CreateStaticObject(leveys, korkeus);
        talo.Position = paikka;
        talo.Shape = Shape.Rectangle;
        talo.Color = vari;
        talo.Tag = "rakennus";
        Add(talo);
    }
    

    /// <summary>
    /// Luodaan pelattava hahmo
    /// </summary>
    /// <param name="paikka">Lähtöpaikka</param>
    /// <param name="leveys">Hahmon leveys</param>
    /// <param name="korkeus">Hahmon korkeus</param>
    public void LuoUkkeli(Vector paikka, double leveys, double korkeus)
    {
        ukkeli = new PhysicsObject(30.0, 30.0);
        ukkeli.Position = paikka;
        ukkeli.Shape = Shape.Circle;
        ukkeli.Color = Color.BloodRed;
        ukkeli.Tag = "ukkeli";
        Add(ukkeli);

        AsetaOhjaimet();
        AddCollisionHandler(ukkeli, "este", PelaajaOsuuEsteeseen);
        AddCollisionHandler(ukkeli, "snack", PelaajaKeraaHerkun);
    }
    

    public void PelaajaOsuuEsteeseen(PhysicsObject ukkeli, PhysicsObject kohde)
    {
        MessageDisplay.Add("Osuit!");

        krapulamittari.Value --;
        
    }
    
    public void PelaajaKeraaHerkun(PhysicsObject ukkeli, PhysicsObject kohde)
    {
        krapulamittari.Value++;
        kohde.Destroy();
    }
    


    /// <summary>
    /// Pelattavan hahmon ja yleiset ohjainkäskyt, pelistä poistuminen.
    /// </summary>
    public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Up, ButtonState.Down, Liikuta, "Liikuta Brodoa ylöspäin", ukkeli, new Vector(0, LIIKKUMISNOPEUS));
        Keyboard.Listen(Key.Up, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);
        Keyboard.Listen(Key.Down, ButtonState.Down, Liikuta, "Liikuta Brodoa alaspäin", ukkeli, new Vector(0, -LIIKKUMISNOPEUS));
        Keyboard.Listen(Key.Down, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikuta Brodoa vasemmalle", ukkeli, new Vector(-LIIKKUMISNOPEUS, 0));
        Keyboard.Listen(Key.Left, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta Brodoa oikealle", ukkeli, new Vector(LIIKKUMISNOPEUS, 0));
        Keyboard.Listen(Key.Right, ButtonState.Released, Liikuta, null, ukkeli, Vector.Zero);

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
    }

    
    /// <summary>
    /// Pelattavan hahmon liikkuminen
    /// </summary>
    /// <param name="ukkeli">Pelihahmo</param>
    /// <param name="suunta">???</param>
    public void Liikuta(PhysicsObject ukkeli, Vector suunta)
    {
        ukkeli.Velocity = suunta;
    }


}