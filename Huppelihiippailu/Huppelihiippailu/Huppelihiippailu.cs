using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

// HUOM:
// Tämä projekti käyttää hyvin kokeellista FarseerPhysics -fysiikkamoottoria
// Varmista että projektin paketit ovat aina uusimmassa versiossa.
// Katso myös https://tim.jyu.fi/view/kurssit/jypeli/farseer
// kaikista tämän version eroavaisuuksista sekä tunnetuista ongelmista. Täydennä listaa tarvittaessa.
/// <summary>
/// Huppelihiippailu-peli
/// </summary>
public class Huppelihiippailu : PhysicsGame
{
    PhysicsObject ukkeli;

    const double LIIKKUMISNOPEUS = 300;
    const double RUUDUN_LEVEYS = 60;
    const double RUUDUN_KORKEUS = 60;


    public override void Begin()
    {
        LuoKentta();
    }


    public void LuoKentta()
    {
        SetWindowSize(850, 550);

        TileMap kentta = TileMap.FromLevelAsset("kentta");
        // kentta.SetTileMethod('X', LuoPolku);
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

        Camera.ZoomToLevel();

    }


    public void LuoSnack(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject snack = new PhysicsObject(10, 10);
        snack.Position = paikka;
        snack.Shape = Shape.Diamond;
        snack.Color = Color.Orange;
        Add(snack);
    }


    public void LuoEste(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject este =  new PhysicsObject(10, 10);
        este.Position = paikka;
        este.Shape = Shape.Hexagon;
        este.Color = Color.Navy;
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
        Add(ukkeli);

        AsetaOhjaimet();
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