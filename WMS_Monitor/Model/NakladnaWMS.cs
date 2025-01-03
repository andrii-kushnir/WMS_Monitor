﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS_Monitor.Model
{
    public class NakladnaWMS
    {
        public int Coden { get; set; }
        public string GuidNakl { get; set; }
        public string PlaceWMS { get; set; }
        public string PlaceERP { get; set; }
        public string Text { get; set; }
        public NaklType Type { get; set; }
        public DateTime DatePrepare { get; set; }
        public DateTime DateOpen { get; set; }
        public DateTime DateClose { get; set; }
        public int Dostavka { get; set; }
        public string NameDoc { get; set; }
        public TimeSpan Timer { get; set; }
        public bool Sound5 { get; set; }
        public bool Sound15 { get; set; }
        public double Waiting { get; set; }
        public string Worker { get; set; }
        public Color Color { get; set; }
    }

    public enum NaklType
    {
        Empty = 0,
        PokCM = 1,
        TerCM = 2,
        Gosp = 3,
        Zbut = 4,
        Povern = 5,
        MP = 6,
        Filii = 7,
        VnPr = 8,
        Post = 9
    }
}
