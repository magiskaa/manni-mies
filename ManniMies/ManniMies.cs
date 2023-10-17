using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace ManniMies;

/// @author Valtteri Antikainen
/// @version 17.10.2023
/// <summary>
/// Lisätty piste- sekä elämälaskuri
/// </summary>
public class ManniMies : PhysicsGame
{
    private PlatformCharacter pelaaja;
    private PlatformCharacter vihu;
    private const double nopeus = 130;
    private const double hyppyNopeus = 750;
    private const int ruudunKoko = 40;
    private IntMeter pisteLaskuri;
    private DoubleMeter elamaLaskuri;
    
    
    public override void Begin()
    {
        ClearAll();
        Gravity = new Vector(0, -1000);
        SetWindowSize(1920,1080,false);
        LuoKentta();
        LisaaNappaimet();
        LuoPisteLaskuri();
        LuoElamaLaskuri();
        Level.CreateBorders();
        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 2;
        Camera.StayInLevel = true;
    }

    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#',LisaaTaso);
        kentta.SetTileMethod('N',LisaaPelaaja);
        kentta.SetTileMethod('*',LisaaManni);
        kentta.SetTileMethod('-',LisaaVihu);
        kentta.Execute(ruudunKoko,ruudunKoko);
        Level.CreateBorders();
    }

    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.Gray;
        taso.Tag = "taso";
        Add(taso);
    }
    
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja = new PlatformCharacter(leveys,korkeus)
        {
            Position = paikka,
            Mass = 4.0,
            Tag = "pelaaja"
        };
        AddCollisionHandler(pelaaja, "manni", TormaaManniin);
        AddCollisionHandler(pelaaja, "vihu", TormaaVihuun);
        Add(pelaaja);
    }
    
    private void LisaaManni(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject manni = PhysicsObject.CreateStaticObject(leveys, korkeus);
        manni.IgnoresCollisionResponse = true;
        manni.Position = paikka;
        manni.Color = Color.Yellow;
        manni.Tag = "manni";
        Add(manni);
    }

    private void LisaaVihu(Vector paikka, double leveys, double korkeus)
    {
        vihu = new PlatformCharacter(leveys, korkeus)
        {
            Position = paikka,
            Mass = 4.0,
            Tag = "vihu",
            Color = Color.Black
        };
        PlatformWandererBrain vihunAivot = new PlatformWandererBrain();
        vihunAivot.Speed = 70;
        vihu.Brain = vihunAivot;
        Add(vihu);
    }
    
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "", pelaaja, nopeus);
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "", pelaaja, -nopeus);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "", pelaaja, hyppyNopeus);
    }

    private static void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    private static void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }
    
    private void TormaaManniin(PhysicsObject hahmo, PhysicsObject manni)
    {
        manni.Destroy();
        pisteLaskuri.Value += 1;
        Teksti("HERKKUA!!");
    }

    private void TormaaVihuun(PhysicsObject hahmo, PhysicsObject vihu)
    {
        Teksti("AIJAIJAI SATTUI!!");
        vihu.Destroy();
        elamaLaskuri.Value -= 1;
    }
    
    private void LuoPisteLaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label(200,80);
        pisteNaytto.SizeMode = TextSizeMode.StretchText;
        pisteNaytto.X = Screen.Right - 200;
        pisteNaytto.Y = Screen.Top - 70;
        pisteNaytto.TextColor = Color.White;
        pisteNaytto.BindTo(pisteLaskuri);
        pisteNaytto.IntFormatString = "Pisteitä: {0:D1}";
        Add(pisteNaytto);
    }
    
    private void LuoElamaLaskuri()
    {
        elamaLaskuri = new DoubleMeter(10);
        elamaLaskuri.MaxValue = 3;
        elamaLaskuri.LowerLimit += ElamaLoppui;

        ProgressBar elamaPalkki = new ProgressBar(300, 80);
        elamaPalkki.X = Screen.Left + 200;
        elamaPalkki.Y = Screen.Top - 100;
        elamaPalkki.Color = Color.White;
        elamaPalkki.BarColor = Color.Red;
        elamaPalkki.BorderColor = Color.Black;
        elamaPalkki.BindTo(elamaLaskuri);
        Add(elamaPalkki);
    }
    
    private void ElamaLoppui()
    {
        ClearAll();
        Level.BackgroundColor = Color.Black;
        Label kuolit = new Label(900, 150, "WASTED");
        kuolit.SizeMode = TextSizeMode.StretchText;
        kuolit.X = 0;   
        kuolit.Y = 0;
        kuolit.TextColor = Color.Red;
        Add(kuolit);
        Timer.SingleShot(2.0, 
            delegate { Begin(); } 
        );
    }
    
    private void Teksti(string text)
    {
        Label teksti = new Label(250, 60, text);
        teksti.SizeMode = TextSizeMode.StretchText;
        teksti.X = 0;
        teksti.Y = -330;
        teksti.TextColor = Color.Black;
        Add(teksti);
        Timer.SingleShot(1.5, 
            delegate { teksti.Destroy(); } 
        );
    }
}