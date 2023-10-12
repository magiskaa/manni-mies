using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace ManniMies;

/// @author Valtteri Antikainen
/// @version 29.09.2023
/// <summary>
/// 
/// </summary>
public class ManniMies : PhysicsGame
{
    private PlatformCharacter pelaaja;
    private const double nopeus = 100;
    private const double hyppyNopeus = 500;
    
    public override void Begin()
    {
        Gravity = new Vector(0, -500);
        SetWindowSize(1920,1080,false);
        LisaaPelaaja();
        LuoNappaimet();
        Level.CreateBorders();
        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 2;
        Camera.StayInLevel = true;
    }

    private void LisaaPelaaja()
    {
        pelaaja = new PlatformCharacter(100, 100, Shape.Circle);
        pelaaja.Mass = 4.0;
        Add(pelaaja);
    }

    private void LuoNappaimet()
    {
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "", pelaaja, nopeus);
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "", pelaaja, -nopeus);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "", pelaaja, hyppyNopeus);
    }

    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }
}

