using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Threading;
using Wanderer.characters;


namespace Wanderer
{
    public class Drawer
    {
        public enum ImgType
        {
            SkeletonDown, SkeletonUp, SkeletonLeft, SkeletonRight,
            BossDown, BossUp, BossLeft, BossRight, 
            HeroDown, HeroUp, HeroLeft, HeroRight,
            Floor,
            Wall,
            FirstAid,
            Armour,
            Weapon,
            Potion
        }

        public Canvas Canvas;
        public int PicSize = 72;
        public Dictionary<string, Image> Images; // Dictionary is used to call different Image objects
        private Dictionary<string, Image> sidebarImages;
        public Dictionary<string, TextBlock> TextBlocks;
        ImgType[] sidebarTiles = new ImgType[] { ImgType.FirstAid, ImgType.Armour, ImgType.Weapon };
        public bool SidebarImagesLoaded;
        private int left;
        private int top;
        private Dictionary<ImgType, Bitmap> resources;
        private static string imagePath = @"../../../img/";

        // Variables used in RedScreen method
        private byte opacity;
        private bool redDown;
        public bool AvaloniaRedDownLock; // Not to display more images on Canvas
        Rectangle rectangle = new Rectangle()
        {
            Width = 600,
            Height = 600,
        };
        DispatcherTimer Timer = new DispatcherTimer();
        TextBlock PausedTextBlock;


        public Drawer(Canvas canvas, int picsize, int left, int top)
        {
            Canvas = canvas;
            PicSize = picsize;
            this.left = left;
            this.top = top;
            resources = new Dictionary<ImgType, Bitmap>();
            Images = new Dictionary<string, Image>();
            sidebarImages = new Dictionary<string, Image>();
            TextBlocks = new Dictionary<string, TextBlock>();
            Load();
            Timer.Tick += Timer_RedColor;
            Timer.Interval = TimeSpan.FromMilliseconds(1);
        }

        // Load all pictures into resources Dictionary
        private void Load()
        {
            resources.Add(ImgType.Floor,        new Bitmap(imagePath + "floor.png"));
            resources.Add(ImgType.Wall,         new Bitmap(imagePath + "wall.png"));
            resources.Add(ImgType.HeroDown,     new Bitmap(imagePath + "hero-down.png"));
            resources.Add(ImgType.HeroUp,       new Bitmap(imagePath + "hero-up.png"));
            resources.Add(ImgType.HeroLeft,     new Bitmap(imagePath + "hero-left.png"));
            resources.Add(ImgType.HeroRight,    new Bitmap(imagePath + "hero-right.png"));
            resources.Add(ImgType.SkeletonDown, new Bitmap(imagePath + "skeleton-down.png"));
            resources.Add(ImgType.SkeletonUp,   new Bitmap(imagePath + "skeleton-up.png"));
            resources.Add(ImgType.SkeletonLeft, new Bitmap(imagePath + "skeleton-left.png"));
            resources.Add(ImgType.SkeletonRight,new Bitmap(imagePath + "skeleton-right.png"));
            resources.Add(ImgType.BossDown,     new Bitmap(imagePath + "boss-down.png"));
            resources.Add(ImgType.BossUp,       new Bitmap(imagePath + "boss-up.png"));
            resources.Add(ImgType.BossLeft,     new Bitmap(imagePath + "boss-left.png"));
            resources.Add(ImgType.BossRight,    new Bitmap(imagePath + "boss-right.png"));
            resources.Add(ImgType.FirstAid,     new Bitmap(imagePath + "firstaid.png"));
            resources.Add(ImgType.Armour,       new Bitmap(imagePath + "armour.png"));
            resources.Add(ImgType.Weapon,       new Bitmap(imagePath + "weapon.png"));
            resources.Add(ImgType.Potion,       new Bitmap(imagePath + "potion.png"));
        }

        // Sidebar text blocks
        public void AddTextBlocks(string name, string text, int fontSize, int top, int left)
        {
            TextBlocks.Add(name, new TextBlock());
            TextBlocks[name].FontSize = fontSize;
            TextBlocks[name].Foreground = new SolidColorBrush(new Color(255, 200, 200, 200));
            TextBlocks[name].Text = text;
            Canvas.Children.Add(TextBlocks[name]);
            Canvas.SetTop(TextBlocks[name], top);
            Canvas.SetLeft(TextBlocks[name], left);
        }

        public void DrawImage(string imageName, ImgType type, Position pos) 
        {
            var image = new Image();
            if (imageName != null) Images.Add(imageName, image);
            image.Source = resources[type];
            Canvas.SetLeft(image, left + pos.X * PicSize);
            Canvas.SetTop(image, top + pos.Y * PicSize);
            Canvas.Children.Add(image);
        }

        public void DrawImage(Character ch, ImgType type)
        {
            DrawImage(ch.Id, type, ch.Position);
        }

        // The method is separate because we don't need to keep map tiles images
        public void DrawMapImage(ImgType type, Position pos)
        {
            DrawImage(null, type, pos);
        }

        public void MoveImage(Character ch, ImgType type)
        {
            if (Images.ContainsKey(ch.Id))
            {
                Image image = Images[ch.Id];
                image.Source = resources[type];
                Canvas.SetLeft(image, left + ch.Position.X * PicSize);
                Canvas.SetTop(image, top + ch.Position.Y * PicSize);
            }
        }

        public void RemoveImage(Character ch)
        {
            if ((!(ch is Player) || !(ch is Boss)) && Images.ContainsKey(ch.Id))
            {
                Canvas.Children.Remove(Images[ch.Id]);
                Images.Remove(ch.Id);
            }
        }

        public void RemoveImage(Loot l)
        {
            if (Images.ContainsKey(l.Id))
            {
                Canvas.Children.Remove(Images[l.Id]);
                Images.Remove(l.Id);
            }
        }

        public void UpdateStatusText(string[] tb)
        {
            for (int i = 0; i < tb.Length; i++)
            {
                TextBlocks[i.ToString()].Text = tb[i];
            }
        }

        public void GameOver()
        {
            DrawCenterText("GAME OVER");
        }

        public void Loading()
        {
            DrawCenterText("LOADING...");
        }

        public void Pause(bool paused)
        {
            if (paused)
            {
                PausedTextBlock = CenterText("pause", SetColor(255, 0, 0), Darken(156));
                Canvas.Children.Add(PausedTextBlock);
            }
            else Canvas.Children.Remove(PausedTextBlock);
        }

        public void RedScreen()
        {
            Timer.Start();
            if(!AvaloniaRedDownLock)
            {
                Canvas.Children.Add(rectangle);
                AvaloniaRedDownLock = true;
            }
            Canvas.SetLeft(rectangle, 0);
            Canvas.SetTop(rectangle, 0);
            if (opacity < 5) return;
        }
        
        public void SideBar()
        {
            Rectangle sideBar = new Rectangle()
            {
                Width = 200,
                Height = 600,
            };
            // Grey sidebar background
            sideBar.Fill = new SolidColorBrush(new Color(255, 50, 50, 50));
            Canvas.SetLeft(sideBar, 600);
            Canvas.SetTop(sideBar, 0);
            Canvas.Children.Add(sideBar);
            if (!SidebarImagesLoaded)
            {
                PlaceSidebarImages("Player", 610, 50);
                SidebarImagesLoaded = true;
            }
        }

        private void PlaceSidebarImages(string name, int left, int top)
        {
            for (int i = 0; i < sidebarTiles.Length; i++)
            {
                LoadSidebarImages(name, sidebarTiles[i]);
                Canvas.SetLeft(sidebarImages[name + sidebarTiles[i].ToString()], left);
                Canvas.SetTop(sidebarImages[name + sidebarTiles[i].ToString()], top + i * 50);
            }
        }

        public void DisplayEnemySidebarImages(bool turnOn)
        {
            if (turnOn)
            {
                PlaceSidebarImages("Enemy", 610, 400);
            }
            if (!turnOn)
            {
                for (int i = 0; i < sidebarImages.Count; i++)
                {
                    for (int j = 0; j < sidebarTiles.Length; j++)
                    {
                        if (sidebarImages.ContainsKey("Enemy" + sidebarTiles[j].ToString()))
                        {
                            Canvas.Children.Remove(sidebarImages["Enemy" + sidebarTiles[j].ToString()]);
                            sidebarImages.Remove("Enemy" + sidebarTiles[j].ToString());
                        }
                    }
                }
            }
        }


        public void LoadSidebarImages(string type, ImgType name)
        {
            Image image = new Image();
            image.Source = resources[name];
            image.Width = 45;
            image.Height = 45;
            sidebarImages.Add(type + name.ToString(), image);
            Canvas.Children.Add(image);
        }


        private void Timer_RedColor(object sender, EventArgs e)
        {
            try
            {
                if (opacity < 120 && !redDown) opacity += 10;
                if (opacity >= 120) redDown = true;
                if (opacity > 0 && redDown)
                {
                    opacity -= 10;
                    if (opacity <= 10)
                    {
                        opacity = 0;
                        redDown = false;
                        rectangle.Fill = new SolidColorBrush(new Color(opacity, 255, 0, 0));
                        Timer.Stop();
                        return;
                    }
                }
                rectangle.Fill = new SolidColorBrush(new Color(opacity, 255, 0, 0));
            }
            catch (Exception m)
            {
                string str = m.Message;
            }
        }

        // The following methods are used to display text through all the screen
        private void DrawCenterText(string str)
        {
            TextBlock textblock = CenterText(str, SetColor(255, 0, 0), Darken(156));
            Canvas.Children.Add(textblock);
        }

        private TextBlock CenterText(string text, SolidColorBrush foreground, SolidColorBrush background)
        {
            var output = new TextBlock();
            output.Text = text.ToUpper();
            output.TextAlignment = TextAlignment.Center;
            output.Foreground = foreground;
            output.Background = background;
            output.FontWeight = FontWeight.Black;
            output.FontSize = 80;
            output.Width = 600;
            output.Height = 120;
            Canvas.SetTop(output, 240);
            return output;
        }

        private SolidColorBrush SetColor(byte r, byte g, byte b)
        {
            return new SolidColorBrush(new Color(255, r, g, b));
        }

        private SolidColorBrush Darken(byte a)
        {
            return new SolidColorBrush(new Color(a, 0, 0, 0));
        }

        public void PrepareDrawer()
        {
            AvaloniaRedDownLock = false;
            SidebarImagesLoaded = false;
            sidebarImages.Clear();
            TextBlocks.Clear();
            SideBar();
        }
    }
}
