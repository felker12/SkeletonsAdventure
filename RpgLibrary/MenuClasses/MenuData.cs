using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RpgLibrary.MenuClasses
{
    public class MenuData
    {
        public string Title { get; set; } = "Menu";
        public bool Visible { get; set; } = false;
        public int Width { get; set; } = 200;
        public int Height { get; set; } = 200;
        public Vector2 Position { get; set; } = new();
        public Color TintColor { get; set; } = Color.White;
        public Color BackgroundColor { get; set; } = Color.White;

        public MenuData()
        {
        }


        public override string ToString()
        {
            string toString = string.Empty;
            toString += $"Title: {Title}, ";
            toString += $"Visible: {Visible}, ";
            toString += $"Width: {Width}, ";
            toString += $"Height: {Height}, ";
            toString += $"Position: {Position}, ";
            toString += $"TintColor: {TintColor}, ";
            toString += $"BackgroundColor: {BackgroundColor}, ";
            return toString;
        }
    }
}
