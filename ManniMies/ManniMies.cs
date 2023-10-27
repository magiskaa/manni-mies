using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace ManniMies;

/// @author Valtteri Antikainen
/// @version 27.10.2023
/// <summary>
/// Lisätty kaksi kenttää ja alkuvalikko
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
    private int kenttaNro = 1;
    
    private Ignorer sama = new ObjectIgnorer();
    
    private Image[] HahmoIdle = LoadImages("idle.png");
    private Image[] HahmonKavely = LoadImages("1", "2", "3", "4", "5", "6", "7", "8");
    private Image[] HahmoAmpuu = LoadImages("ammu1.png", "ammu2.png", "ammu3.png", "ammu4.png");
    
    private Image manniKuva = LoadImage("manni.png");
    private Image[] vihunKavely = LoadImages("vihu1.png","vihu2.png","vihu3.png","vihu4.png");
    private Image aseenKuva = LoadImage("ase.png");
    
    private Image tausta = LoadImage("BG.png");
    private Image platform = LoadImage("platform.png");
    
    List<Label> valikonKohdat;
    
    public override void Begin()
    {
        SetWindowSize(1920, 1080, false); 
        ClearAll();
        Valikko();
    }
    
    private void Valikko()
    {
        Level.BackgroundColor = Color.LightBlue;
        
        Label otsikko = new Label("ManniMies");
        otsikko.Y = 150;
        otsikko.Font = new Font(150, true);
        Add(otsikko);

        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Aloita Peli");
        kohta1.Position = new Vector(0, 0);
        kohta1.Font = new Font(100, false);
        valikonKohdat.Add(kohta1);

        Label kohta2 = new Label("Poistu");
        kohta2.Position = new Vector(0, -100);
        kohta2.Font = new Font(100, false);
        valikonKohdat.Add(kohta2);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, SeuraavaKentta, null);
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, Exit, null);
        Mouse.ListenOn(kohta1, HoverState.Enter, MouseButton.None, ButtonState.Irrelevant, ValikossaLiikkuminen, null,
            kohta1, true);
        Mouse.ListenOn(kohta1, HoverState.Exit, MouseButton.None, ButtonState.Irrelevant, ValikossaLiikkuminen, null,
            kohta1, false);
        Mouse.ListenOn(kohta2, HoverState.Enter, MouseButton.None, ButtonState.Irrelevant, ValikossaLiikkuminen, null,
            kohta2, true);
        Mouse.ListenOn(kohta2, HoverState.Exit, MouseButton.None, ButtonState.Irrelevant, ValikossaLiikkuminen, null,
            kohta2, false);
    }
    
    private void ValikossaLiikkuminen(Label kohta, bool paalla)
    {
        if (paalla)
        {
            kohta.TextColor = Color.Red;
        }
        else kohta.TextColor = Color.Black;
    }
    private void SeuraavaKentta()
    {
        ClearAll();

        switch (kenttaNro)
        {
            case 1:
                AloitaPeli("kentta1.txt");
                break;
            case 2:
                AloitaPeli("kentta2.txt");
                break;
            case 3:
                AloitaPeli("kentta3.txt");
                break;
            case > 3:
                kenttaNro = 1;
                Voitit();
                break;
        }
    }
    private void AloitaPeli(string kenttaNRO)
    {
        Gravity = new Vector(0, -1000);
        SetWindowSize(1920, 1080, false); 
        
        LuoKentta(kenttaNRO);
        LisaaNappaimet();
        LuoPisteLaskuri();
        LuoElamaLaskuri();

        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 2;
        Camera.StayInLevel = true;
        Level.Background.Image = tausta;
        Level.Background.MovesWithCamera = true;
        Level.Background.FitToLevel();
    }

    private void LuoKentta(string kenttaNRO)
    {
        TileMap kentta = TileMap.FromLevelAsset(kenttaNRO);
        kentta.SetTileMethod('#',LisaaTaso);
        kentta.SetTileMethod('N',LisaaPelaaja);
        kentta.SetTileMethod('*',LisaaManni);
        kentta.SetTileMethod('-',LisaaVihu);
        kentta.Execute(ruudunKoko,ruudunKoko);
        Level.CreateBorders();
        
        switch (kenttaNro)
        {
            case 1:
                LevelTeksti("Level 1");
                break;
            case 2:
                LevelTeksti("Level 2");
                break;
            case 3:
                LevelTeksti("Level 3");
                break;
        }
    }

    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = platform;
        taso.Tag = "taso";
        Add(taso);
    }
    
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja = new PlatformCharacter(leveys,korkeus)
        {
            Position = paikka,
            Mass = 4.0,
            Tag = "pelaaja",
            AnimWalk = new Animation(HahmonKavely),
            AnimIdle = new Animation(HahmoIdle),
            
        };
        pelaaja.AnimWalk.FPS = 12;
        AddCollisionHandler(pelaaja, "manni", TormaaManniin);
        AddCollisionHandler(pelaaja, "vihu", TormaaVihuun);
        pelaaja.Weapon = new AssaultRifle(30, 10);
        pelaaja.Weapon.InfiniteAmmo = true;
        pelaaja.Weapon.ProjectileCollision = AmmusOsui;
        pelaaja.Weapon.FireRate = 2;
        pelaaja.Weapon.Image = aseenKuva;
        Add(pelaaja);
    }
    
    private void LisaaManni(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject manni = PhysicsObject.CreateStaticObject(leveys, korkeus);
        manni.CollisionIgnorer = sama;
        manni.IgnoresCollisionResponse = true;
        manni.Position = paikka;
        manni.Image = manniKuva;
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
            AnimWalk = new Animation(vihunKavely)
        };
        vihu.AnimWalk.FPS = 6;
        PlatformWandererBrain vihunAivot = new PlatformWandererBrain();
        vihunAivot.Speed = 70;
        vihu.Brain = vihunAivot;
        Add(vihu);
    }
    
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "pelaaja liikkuu oikealle", pelaaja, nopeus);
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "pelaaja liikkuu vasemmalle", pelaaja, -nopeus);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "pelaaja hyppää", pelaaja, hyppyNopeus);
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "pelaaja ampuu", pelaaja);
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "näytä ohjeet");
    }

    private static void Liikuta(PlatformCharacter hahmo, double suunta)
    {
        hahmo.Walk(suunta);
    }

    private static void Hyppaa(PlatformCharacter hahmo, double suunta)
    {
        hahmo.Jump(suunta);
    }
    
    private void TormaaManniin(PhysicsObject hahmo, PhysicsObject manni)
    {
        manni.Destroy();
        pisteLaskuri.Value += 1;
        Teksti("HERKKUA!!");
        if (pisteLaskuri.Value >= 15)
        {
            kenttaNro++;
            if (kenttaNro > 3) Voitit();
            else LevelLapi();
        }
    }

    private void TormaaVihuun(PhysicsObject hahmo, PhysicsObject vihollinen)
    {
        Teksti("AIJAIJAI SATTUI!!");
        vihollinen.Destroy();
        elamaLaskuri.Value -= 1;
    }
    
    private void AmmuAseella(PlatformCharacter hahmo)
    {
        PhysicsObject ammus = hahmo.Weapon.Shoot();
        
        hahmo.Animation = new Animation(HahmoAmpuu);
        hahmo.Animation.FPS = 4;
        
        if (ammus != null)
        {
            ammus.CollisionIgnorer = sama;
            hahmo.Animation.Start(1);
            ammus.Size *= 1.1;
            ammus.MaximumLifetime = TimeSpan.FromSeconds(3.0);
        } 
    }
    
    private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        if (ReferenceEquals(kohde.Tag, "taso"))
        {
            ammus.Destroy();
            return;
        }
        if (ReferenceEquals(kohde.Tag, "vihu"))
        {
            kohde.Destroy();
            ammus.Destroy();
            pisteLaskuri.Value += 1;
            if (pisteLaskuri.Value >= 15)
            {
                kenttaNro++;
                if (kenttaNro > 3) Voitit();
                else LevelLapi();
            }
        }
        if (ReferenceEquals(kohde.Tag, "pelaaja"))
        {
            elamaLaskuri.Value -= 1;
            ammus.Destroy();
        }
    }
    
    private void LuoPisteLaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label(200,80)
        {
            SizeMode = TextSizeMode.StretchText,
            X = Screen.Right - 200,
            Y = Screen.Top - 70,
            TextColor = Color.White
        };
        pisteNaytto.BindTo(pisteLaskuri);
        pisteNaytto.IntFormatString = "Pisteitä: {0:D1}";
        Add(pisteNaytto);
    }
    
    private void LevelLapi()
    {
        if (kenttaNro >= 3) Voitit();
        
        MediaPlayer.Stop();
        
        ClearAll();
        Level.BackgroundColor = Color.Black;
        Label levelLapi = new Label(900, 150, "Taso Suoritettu")
        {
            SizeMode = TextSizeMode.StretchText,
            TextColor = Color.Yellow
        };
        Add(levelLapi);
        Timer.SingleShot(7.0, SeuraavaKentta);
    }
    
    private void Voitit()
    {
        MediaPlayer.Stop();
        
        ClearAll();
        Camera.ZoomFactor = 1.5;
        Camera.StayInLevel = true;
        Level.Background.Image = tausta;
        Level.Background.MovesWithCamera = true;
        Level.Background.Scale = 5.0;
        Label voitit = new Label(400, 150, "Voitit!")
        {
            SizeMode = TextSizeMode.StretchText,
            TextColor = Color.Green
        };
        Add(voitit);
        Timer.SingleShot(7.0, Begin);
    }
    
    private void LuoElamaLaskuri()
    {
        elamaLaskuri = new DoubleMeter(10);
        elamaLaskuri.MaxValue = 3;
        elamaLaskuri.LowerLimit += ElamaLoppui;

        ProgressBar elamaPalkki = new ProgressBar(300, 80)
        {
            X = Screen.Left + 200,
            Y = Screen.Top - 100,
            Color = Color.White,
            BarColor = Color.Red,
            BorderColor = Color.Black
        };
        elamaPalkki.BindTo(elamaLaskuri);
        Add(elamaPalkki);
    }
    
    private void ElamaLoppui()
    {
        ClearAll();
        Level.BackgroundColor = Color.Black;
        Label kuolit = new Label(900, 150, "WASTED")
        {
            SizeMode = TextSizeMode.StretchText,
            TextColor = Color.Red
        };
        Add(kuolit);
        Timer.SingleShot(2.0, Begin);
    }
    
    private void Teksti(string text)
    {
        Label teksti = new Label(250, 60, text)
        {
            SizeMode = TextSizeMode.StretchText,
            X = 0,
            Y = -330,
            TextColor = Color.Black
        };
        Add(teksti);
        Timer.SingleShot(1.5, 
            delegate { teksti.Destroy(); } 
        );
    }
    
    private void LevelTeksti(string kentta)
    {
        Label teksti = new Label(300, 80, kentta)
        {
            SizeMode = TextSizeMode.StretchText,
            X = 0,
            Y = 500,
            TextColor = Color.Black
        };
        Add(teksti);
    }
}