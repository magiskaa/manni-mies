using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace ManniMies;

/// @author Valtteri Antikainen
/// @version 3.11.2023
/// <summary>
/// Lisätty kenttään sade ja poistettu toistoa
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
    
    private SoundEffect valikkoHover = LoadSoundEffect("valikkoHover");
    private SoundEffect valikkoValinta = LoadSoundEffect("valikkovalinta");
    private SoundEffect manninKerays = LoadSoundEffect("manninKerays");
    private SoundEffect osuma = LoadSoundEffect("osuma");
    private SoundEffect levelCompleted = LoadSoundEffect("levelCompleted");
    private SoundEffect kentanVaihto = LoadSoundEffect("kentanVaihto");
    private SoundEffect wasted = LoadSoundEffect("wasted");
    
    public override void Begin()
    {
        SetWindowSize(1920, 1080, false); 
        ClearAll();
        Valikko();
    }
    
    private void Valikko()
    {
        Level.BackgroundColor = Color.LightBlue;
        
        MediaPlayer.Play("valikkoMusiikki");
        
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
            valikkoHover.Play();
        }
        else kohta.TextColor = Color.Black;
    }
    private void SeuraavaKentta()
    {
        ClearAll();
        valikkoValinta.Play();

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
        Timer.CreateAndStart(1, Sade);

        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 2;
        Camera.StayInLevel = true;
        Level.Background.Image = tausta;
        Level.Background.MovesWithCamera = true;
        Level.Background.FitToLevel();
        
        MediaPlayer.Play("taustamusiikki");
        MediaPlayer.IsRepeating = true;
    }

    private void LuoKentta(string kenttaNRO)
    {
        kentanVaihto.Play();
        
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
                Teksti("Level 1", 300, 80, 500, false);
                break;
            case 2:
                Teksti("Level 2", 300, 80, 500, false);
                break;
            case 3:
                Teksti("Level 3", 300, 80, 500, false);
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
        manninKerays.Play();
        manni.Destroy();
        pisteLaskuri.Value += 1;
        Teksti("HERKKUA!!",250, 60, -330, true);
        if (pisteLaskuri.Value >= 15)
        {
            levelCompleted.Play();
            kenttaNro++;
            if (kenttaNro > 3) Voitit();
            else LevelLapi();
        }
    }

    private void TormaaVihuun(PhysicsObject hahmo, PhysicsObject vihollinen)
    {
        osuma.Play();
        Teksti("AIJAIJAI SATTUI!!", 250, 60, -330, true);
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
        levelCompleted.Play();
        
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
        levelCompleted.Play();
        
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
        MediaPlayer.Stop();
        wasted.Play();
        
        ClearAll();
        Level.BackgroundColor = Color.Black;
        Label kuolit = new Label(900, 150, "WASTED")
        {
            SizeMode = TextSizeMode.StretchText,
            TextColor = Color.Red
        };
        Timer.SingleShot(2.5,delegate { Add(kuolit); });
        Timer.SingleShot(7.0, SeuraavaKentta);
    }
    
    private void Teksti(string text, double leveys, double korkeus, int y, bool onko)
    {
        Label teksti = new Label(leveys, korkeus, text)
        {
            SizeMode = TextSizeMode.StretchText,
            Y = y,
            TextColor = Color.Black
        };
        Add(teksti);
        if (onko)
        {
            Timer.SingleShot(1.5,
                delegate { teksti.Destroy(); }
            );
        }
    }
    
    private void Sade()
    {
        if (kenttaNro == 1) LisaaSade(35, 0.1, 5, Color.LightBlue, Shape.Circle);

        if (kenttaNro == 2) LisaaSade(25, 0.4, 7, Color.White, Shape.Circle);

        if (kenttaNro == 3) LisaaSade(25, 0.1, 7, Color.Red, Shape.Triangle);
    }
    
    private void LisaaSade(int paljonko, double massa, int koko, Color vari, Shape muoto)
    {
        for (int i = 0; i < paljonko; i++)
        {
            PhysicsObject pisara = new PhysicsObject(koko, koko, muoto);
            pisara.IgnoresGravity = true;
            pisara.CollisionIgnorer = sama;
            pisara.IgnoresCollisionResponse = true;
            pisara.Mass = massa;
            pisara.X = RandomGen.NextDouble(Level.Left, Level.Right);
            pisara.Y = RandomGen.NextDouble(Level.Top, Level.Top + 500);
            pisara.Color = vari;
            Add(pisara, 3);
            pisara.Hit(new Vector(0,-40));
        }
    }
}