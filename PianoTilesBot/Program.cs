using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PianoTilesBot
{
    class Program
    {
        private static ChromeDriver _driver;
        private static Tile[][] _ListOfRowsOfTiles = new Tile[4][];
        private static Bitmap _bmp;
        //start program
        public static void Main(string[] args)
        {
            //start chrome and navigate to piano tiles page

            Initialize_ChromeDriver();


            //lets get the game canvas dimensions
            var canvas = _driver.FindElementByTagName("canvas");

            var s = canvas.Size;
            var topleftpoint = canvas.Location;

            var tileheight = s.Height / 4;
            var tilewidth = s.Width / 4;

            //starting points in 2nd row

            string[] keys = new string[]
            {
                "A",
                "S",
                "D",
                "F"
            };

            Tile[] currentRow;
            Point currentPoint;

            for (int j = 0; j < 4; j++)
            {
                 currentRow= new Tile[4];              
                //top row
                for (int i = 0; i < 4; i++)
                {
                    currentPoint = new Point()
                    {
                        X = topleftpoint.X + i * tilewidth,
                        Y = topleftpoint.Y + s.Height - (j+1) * tileheight
                    };
                    currentRow[i] = new Tile(currentPoint, tilewidth, tileheight, keys[i]);
                }
                _ListOfRowsOfTiles[j] =  currentRow;
            }       

            //get into one specific game mode "arcade" is first class so we click that by default
            var arcadeTileElement = _driver.FindElementByClassName("tile");
            arcadeTileElement.Click();

            //sleep so element can load. Can probably do some sort of lambda to get the element first but idk
            Thread.Sleep(1000);

            var popup = _driver.FindElementById("help");
            popup.Click();

            //loop through and parse the image and find keys to hit. Will search all 4 rows and hit all 4 keys at once so
            // sounds a little funky hitting 3 or 4 keys at once.
           while (true)
           {
                try
                {
                    AnalyzeScreenShot();
                }catch(Exception ex)
                {
                        //close selenium
                        try { _driver.Close(); } catch (Exception exx) { }
                    //break out of loop and close program
                    break;
                }
           }
        }

        private static void Initialize_ChromeDriver()
        {
            _driver = new ChromeDriver(@"C:\Users\Anthony\source\repos\PianoTilesBot\PianoTilesBot\bin\Debug");
            _driver.Manage().Window.Maximize();
            _driver.Url = "http://tanksw.com/piano-tiles/";
            _driver.Navigate();
        }

        public static void AnalyzeScreenShot()
        {
            try
            {
                //grab a screen shot from chrome selenium
                using (var ms = new MemoryStream(_driver.GetScreenshot().AsByteArray))
                {
                    //create a bitmap image
                    _bmp = new Bitmap(ms);
                }
            }catch(Exception ex)
            {
                //bubble up the exception
                throw ex;
            }
            //loop through all rows of tiles
            foreach (var level in _ListOfRowsOfTiles)
            {
                //lop through all tiles in a row to see if it is black
                foreach (var tile in level)
                {
                    if (tile.IsTileBlack(_bmp, _driver))
                    {
                        break;
                    }
                }
            }
        }        
    }

    public class Tile
    {
        private int _width { get; }
        private int _height {  get; }
        public string _key { get; }
        private Point _topLeftCoordinate { get; }
        int _insideWall { get; }
        public Tile(Point topLeftCoordinate, int width, int height, string keyToType)
        {
            _key = keyToType;
            _width = width;
            _height = height;
            _topLeftCoordinate = topLeftCoordinate;
            _insideWall = _width / 7;
        }
        public bool IsTileBlack(Bitmap screenshotIn, ChromeDriver driver)
        {
            
            //check a line of pixels along the left wall, the entire height of the tile
            for(int i =3; i<_height-3; i+=3)
            {
                var test1 = screenshotIn.GetPixel(_topLeftCoordinate.X + _insideWall, _topLeftCoordinate.Y).Name;
                if (test1 != "ff111111")
                {
                    return false;
                }
            }
            //if tile is black, then send sned the key to the chromedriver
            new Actions(driver).SendKeys(_key).Build().Perform();
            return true;
        }
        internal void PressButton(ChromeDriver driver)
        {
            new Actions(driver).SendKeys(_key).Build().Perform();        
        }
    }
}
