using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Silk.NET.Input;
using Key = Jypeli.Key;
using MouseButton = Jypeli.MouseButton;

namespace ManniMies;

/// @author Valtteri Antikainen
/// @version 23.11.2023
/// <summary>
/// Korjattu muutama virhe
/// </summary>
public class ManniMies : PhysicsGame
{
    private PlatformCharacter pelaaja;
    private PlatformCharacter vihu;
    private PlatformCharacter boss;
    private PlatformWandererBrain bossinAivot;
    private bool bossiKuollut = false;
    
    private const double nopeus = 130;
    private const double hyppyNopeus = 750;
    private const int ruudunKoko = 40;
    
    private IntMeter pisteLaskuri;
    private DoubleMeter elamaLaskuri;
    private DoubleMeter bossinElamaLaskuri;
    private int kenttaNro = 1;
    
    private Ignorer sama = new ObjectIgnorer();
    
    private Image[] HahmoIdle = LoadImages("idle.png");
    private Image[] HahmonKavely = LoadImages("1", "2", "3", "4", "5", "6", "7", "8");
    private Image[] HahmoAmpuu = LoadImages("ammu1.png", "ammu2.png", "ammu3.png", "ammu4.png");
    
    private Image manniKuva = LoadImage("manni.png");
    private Image[] vihunKavely = LoadImages("vihu1.png","vihu2.png","vihu3.png","vihu4.png");
    private Image aseenKuva = LoadImage("ase.png");
    
    private Image[] bossIdle = LoadImages("bossIdle1.png", "bossIdle2.png", "bossIdle3.png", "bossIdle4.png",
        "bossIdle5.png", "bossIdle6.png");
    private Image[] bossKavely = LoadImages("bossKavely1.png", "bossKavely2.png", "bossKavely3.png", "bossKavely4.png",
        "bossKavely5.png", "bossKavely6.png", "bossKavely7.png", "bossKavely8.png", "bossKavely9.png", "bossKavely10.png", 
        "bossKavely11.png", "bossKavely12.png");
    
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
    
    /// <summary>
    /// Pelin alkuvalikko
    /// </summary>
    
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

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, AlkuTekstit, null);
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
    
    /// <summary>
    /// Vaihtaa punaiseksi valikonkohdan jonka päällä hiiri on
    /// </summary>
    /// <param name="kohta">valikonkohta</param>
    /// <param name="paalla">onko hiiri kohdan päällä vai ei</param>
    
    private void ValikossaLiikkuminen(Label kohta, bool paalla)
    {
        if (paalla)
        {
            kohta.TextColor = Color.Red;
            valikkoHover.Play();
        }
        else kohta.TextColor = Color.Black;
    }
    
    /// <summary>
    /// Pelin tarinaa
    /// </summary>
    
    private void AlkuTekstit()
    {
        ClearAll();
        MediaPlayer.Stop();
        Level.BackgroundColor = Color.Black;
        Timer.SingleShot(1,
            delegate { Teksti("Hirvittävä demoni on vohkinut kaikki maailman mannit!",1000,100,0,true, Color.White); });
        Timer.SingleShot(3.5,
            delegate { Teksti("Tehtäväsi on kerätä kaikki mannit",700,100,0,true, Color.White); });
        Timer.SingleShot(6,
            delegate { Teksti("sekä tuhota iljettävä demoni ja sen kätyrit",900,100,0,true, Color.White); });
        Timer.SingleShot(8.5,
            delegate { Teksti("Oletko valmis?",400,100,0,true, Color.White); });        
        Timer.SingleShot(10,SeuraavaKentta);
    }
    
    /// <summary>
    /// Vaihtaa seuraavaan kenttään
    /// </summary>
    
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
    
    /// <summary>
    /// Peli alkaa
    /// </summary>
    /// <param name="kenttaNRO">monesko kenttä on menossa</param>
    
    private void AloitaPeli(string kenttaNRO)
    {
        Gravity = new Vector(0, -1000);
        SetWindowSize(1920, 1080, false); 
        
        LuoKentta(kenttaNRO);
        LisaaNappaimet();
        LuoPisteLaskuri();
        LuoElamaLaskuri();
        if (kenttaNro == 3) LuoBossinElamalaskuri();
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
    
    /// <summary>
    /// Luo kentän
    /// </summary>
    /// <param name="kenttaNRO">mikä kenttä luodaan</param>

    private void LuoKentta(string kenttaNRO)
    {
        kentanVaihto.Play();
        
        TileMap kentta = TileMap.FromLevelAsset(kenttaNRO);
        kentta.SetTileMethod('#',LisaaTaso);
        kentta.SetTileMethod('N',LisaaPelaaja);
        kentta.SetTileMethod('*',LisaaManni);
        kentta.SetTileMethod('-',LisaaVihu);
        kentta.SetTileMethod('X',LisaaBossi);
        kentta.Execute(ruudunKoko,ruudunKoko);
        Level.CreateBorders();
        
        switch (kenttaNro)
        {
            case 1:
                Teksti("Level 1", 300, 80, 500, false, Color.Black);
                break;
            case 2:
                Teksti("Level 2", 300, 80, 500, false, Color.Black);
                break;
            case 3:
                Teksti("Level 3", 300, 80, 500, false, Color.Black);
                break;
        }
    }
    
    /// <summary>
    /// Lisöä kenttään tasot
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>

    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = platform;
        taso.Tag = "taso";
        Add(taso);
    }
    
    /// <summary>
    /// Lisää kenttään pelaajan
    /// </summary>
    /// <param name="paikka">pelaajan paikka kun kenttä alkaa</param>
    /// <param name="leveys">hahmon leveys</param>
    /// <param name="korkeus">hahmon korkeus</param>
    
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
        AddCollisionHandler(pelaaja,"bossi",TormaaBossiin);
        pelaaja.Weapon = new AssaultRifle(30, 10);
        pelaaja.Weapon.InfiniteAmmo = true;
        pelaaja.Weapon.ProjectileCollision = AmmusOsui;
        pelaaja.Weapon.FireRate = 1.5;
        pelaaja.Weapon.Image = aseenKuva;
        Add(pelaaja);
    }
    
    /// <summary>
    /// Lisää banaanit kenttään
    /// </summary>
    /// <param name="paikka">mihin banaanit lisätään</param>
    /// <param name="leveys">banaanin leveys</param>
    /// <param name="korkeus">banaanin korkeus</param>
    
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
    
    /// <summary>
    /// Lisää viholliset kenttään
    /// </summary>
    /// <param name="paikka">mihin viholliset lisätään</param>
    /// <param name="leveys">vihollisen leveys</param>
    /// <param name="korkeus">vihollisen korkeus</param>

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
    
    /// <summary>
    /// Lisää bossin kenttään
    /// </summary>
    /// <param name="paikka">mihin bossi lisätään</param>
    /// <param name="leveys">bossin leveys</param>
    /// <param name="korkeus">bossin korkeus</param>

    private void LisaaBossi(Vector paikka, double leveys, double korkeus)
    {
        boss = new PlatformCharacter(leveys*3, korkeus*3)
        {
            Position = paikka,
            Mass = 10,
            Tag = "bossi",
            AnimWalk = new Animation(bossKavely),
            AnimIdle = new Animation(bossIdle)
        };
        boss.AnimWalk.FPS = 8;
        boss.AnimIdle.FPS = 6;
        bossinAivot = new PlatformWandererBrain();
        bossinAivot.Speed = 90;
        bossinAivot.FallsOffPlatforms = true;
        bossinAivot.JumpSpeed = 600;
        bossinAivot.TriesToJump = true;
        boss.Brain = bossinAivot;
        bossinAivot.Active = false;
        Add(boss);
    }
    
    /// <summary>
    /// Lisää kontrollit peliin
    /// </summary>
    
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "pelaaja liikkuu oikealle", pelaaja, nopeus);
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "pelaaja liikkuu vasemmalle", pelaaja, -nopeus);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "pelaaja hyppää", pelaaja, hyppyNopeus);
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "pelaaja ampuu", pelaaja);
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "näytä ohjeet");
    }
    
    /// <summary>
    /// Liikuttaa pelaajaa
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="suunta">mihin suuntaan pelaaja liikkuu</param>

    private void Liikuta(PlatformCharacter hahmo, double suunta)
    {
        hahmo.Walk(suunta);
        if (pelaaja.Y >= 320) bossinAivot.Active = true;
    }
    
    /// <summary>
    /// Pelaaja hyppää
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="suunta">kuinka korkealle pelaaja hyppää</param>

    private void Hyppaa(PlatformCharacter hahmo, double suunta)
    {
        hahmo.Jump(suunta);
    }
    
    /// <summary>
    /// Kun pelaaja kerää banaanin
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="manni">banaani</param>
    
    private void TormaaManniin(PhysicsObject hahmo, PhysicsObject manni)
    {
        manninKerays.Play();
        manni.Destroy();
        pisteLaskuri.Value -= 1;
        Teksti("HERKKUA!!",250, 60, -330, true, Color.Black);
        
        if (pisteLaskuri.Value <= 0)
        {
            if (kenttaNro == 3 && bossiKuollut == false) return;
            levelCompleted.Play();
            kenttaNro++;
            if (kenttaNro > 3) Voitit();
            else LevelLapi();
        }
    }
    
    /// <summary>
    /// Kun pelaaja törmää viholliseen
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="vihollinen">vihollinen</param>

    private void TormaaVihuun(PhysicsObject hahmo, PhysicsObject vihollinen)
    {
        osuma.Play();
        Teksti("AIJAIJAI SATTUI!!", 250, 60, -330, true, Color.Black);
        vihollinen.Destroy();
        elamaLaskuri.Value -= 1;
    }

    /// <summary>
    /// Kun pelaaja törmää bossiin
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="bossi">bossi</param>
    
    private void TormaaBossiin(PhysicsObject hahmo, PhysicsObject bossi)
    {
        bossi.Throw(pelaaja,Angle.FromDegrees(30), 20000);
        osuma.Play();
        Teksti("AIJAIJAI SATTUI!!", 250, 60, -330, true, Color.Black);
        elamaLaskuri.Value -= 1;
    }
    
    /// <summary>
    /// Pelaaja ampuu aseella
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    
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
    
    /// <summary>
    /// Ammus osuu johonkin
    /// </summary>
    /// <param name="ammus">ammus</param>
    /// <param name="kohde">mihin ammus osuu</param>
    
    private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        if (ReferenceEquals(kohde.Tag, "taso"))
        {
            ammus.Destroy();
        }
        if (ReferenceEquals(kohde.Tag, "vihu"))
        {
            kohde.Destroy();
            ammus.Destroy();
        }
        if (ReferenceEquals(kohde.Tag, "pelaaja"))
        {
            elamaLaskuri.Value -= 1;
            ammus.Destroy();
        }
        if (ReferenceEquals(kohde.Tag, "bossi"))
        {
            if (pisteLaskuri.Value != 0)
            {
                ammus.Destroy();
                return;
            }
            bossinElamaLaskuri.Value -= 1;
            ammus.Destroy();
        }
    }
    
    /// <summary>
    /// Luo pistelaskurin oikeaan ylänurkkaan
    /// </summary>
    
    private void LuoPisteLaskuri()
    {
        pisteLaskuri = new IntMeter(10);

        Label pisteNaytto = new Label(300,80)
        {
            SizeMode = TextSizeMode.StretchText,
            X = Screen.Right - 200,
            Y = Screen.Top - 70,
            TextColor = Color.White
        };
        pisteNaytto.BindTo(pisteLaskuri);
        pisteNaytto.IntFormatString = "Manneja jäljellä: {0:D1}";
        Add(pisteNaytto);
    }
    
    /// <summary>
    /// Kun kenttä on läpäisty
    /// </summary>
    
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
    
    /// <summary>
    /// Kun voittaa pelin
    /// </summary>
    
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
    
    /// <summary>
    /// Luo elämäpalkin vasempaan ylänurkkaan
    /// </summary>
    
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
    
    /// <summary>
    /// Luo bossin elämäpalkin alareunaan 3. kentässä
    /// </summary>
    
    private void LuoBossinElamalaskuri()
    {
        bossinElamaLaskuri = new DoubleMeter(10);
        bossinElamaLaskuri.MaxValue = 10;
        bossinElamaLaskuri.LowerLimit += Voitit;

        ProgressBar elamaPalkki = new ProgressBar(500, 40)
        {
            
            Y = Screen.Bottom + 50,
            Color = Color.White,
            BarColor = Color.Red,
            BorderColor = Color.Black
        };
        elamaPalkki.BindTo(bossinElamaLaskuri);
        Add(elamaPalkki);
    }
    
    /// <summary>
    /// Kun pelaajan elämät loppuu
    /// </summary>
    
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
    
    /// <summary>
    /// Kaikki tekstit mitkä näkyy pelissä
    /// </summary>
    /// <param name="text">teksti</param>
    /// <param name="leveys">tekstin leveys</param>
    /// <param name="korkeus">tekstin korkeus</param>
    /// <param name="y">tekstin y-koordnaatti</param>
    /// <param name="onko">onko teksti pysyvä vai tuohoutuuko se 2 sekunnin kuluttua</param>
    /// <param name="vari">tekstin väri</param>
    
    private void Teksti(string text, double leveys, double korkeus, int y, bool onko, Color vari)
    {
        Label teksti = new Label(leveys, korkeus, text)
        {
            SizeMode = TextSizeMode.StretchText,
            Y = y,
            TextColor = vari
        };
        Add(teksti);
        if (onko)
        {
            Timer.SingleShot(2,
                delegate { teksti.Destroy(); }
            );
        }
    }
    
    /// <summary>
    /// Määrää mitä kussakin kentässä sataa
    /// </summary>
    
    private void Sade()
    {
        if (kenttaNro == 1) LisaaSade(35, 0.1, 5, Color.LightBlue, Shape.Circle);

        if (kenttaNro == 2) LisaaSade(25, 0.4, 7, Color.White, Shape.Circle);

        if (kenttaNro == 3) LisaaSade(25, 0.1, 7, Color.Red, Shape.Triangle);
    }
    
    /// <summary>
    /// Lisää sateen kenttiin
    /// </summary>
    /// <param name="paljonko">montako pisaraa luodaan kerralla</param>
    /// <param name="massa">pisaroiden massa</param>
    /// <param name="koko">pisaroiden koko</param>
    /// <param name="vari">pisaroiden väri</param>
    /// <param name="muoto">pisaroiden muoto</param>
    
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