using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Random = System.Random;

namespace snake;

public class Game1 : Game
{
    List<Point> snakePart = new List<Point>();   //skapar en lista för att kunna lagra alla delar av ormen, point är som en variabel med (x, y) koordinater


    private Texture2D pixel;   //skapar en textur som man kan rita med
    private SpriteFont font;    //skapar en font för att kunna rita text på skärmen
    private SpriteFont fontBig; //skapar en större font för att kunna rita "Game Over" texten

    Point direction = new Point(1, 0);   //en ny point som lägger till en X för att röra ormen
    Point food;   //äpplets position
    private Random random = new Random();
    private int score = 0;    //poäng
    private int highScore;   //variabel som håller reda på highscoret
    int foodRNG;   //en variabel som håller reda på om det ska spawna ett guldäpple eller inte
    private int cellSize = 20;
    private int maxX;    //deklareras så den kan användas överallt
    private int maxY;    //deklareras så den kan användas överallt
    private bool newHighScore = false;
    private bool gameOver = false;
    private bool showControls = false;   //en bool så man kan toggla controls texten i startmenyn
    private bool startText = true;   //en bool för att kunna visa starttexten innan spelet startar
    private bool gappleEaten = false;
    private float moveTimer = 0f;   //håller reda på tiden som gått
    private float moveDelay = 0.15f; //speeden för ormen normalt
    private float currentMoveDelay = 0.15f; //speeden som ormen har
    private float gappleMoveDelay = 0.05f; //speeden för guldäpplet

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private void SpawnFood()
    {
        //välj en random position i rutnätet, där varje ruta är 20x20 pixlar
        int maxX = _graphics.PreferredBackBufferWidth / 20;
        int maxY = _graphics.PreferredBackBufferHeight / 20;

        food = new Point(random.Next(0, maxX), random.Next(0, maxY));
    }

    private void SpawnGoldenFood()
    {
        //välj en random position i rutnätet, där varje ruta är 20x20 pixlar
        int maxX = _graphics.PreferredBackBufferWidth / 20;
        int maxY = _graphics.PreferredBackBufferHeight / 20;

        food = new Point(random.Next(0, maxX), random.Next(0, maxY));
    }

    private void addPoint()
    {
        score++;
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1000;
        _graphics.PreferredBackBufferHeight = 1000;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        //spawnar första delarna av ormen
        snakePart.Add(new Point(5, 5));
        snakePart.Add(new Point(4, 5));
        snakePart.Add(new Point(3, 5));

        SpawnFood(); //spawnar ett äpple när spelet startar

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        fontBig = Content.Load<SpriteFont>("fontBig"); //laddar in fonten för poängen
        font = Content.Load<SpriteFont>("font"); //laddar in fonten för poängen

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {

        maxX = _graphics.PreferredBackBufferWidth / cellSize;
        maxY = _graphics.PreferredBackBufferHeight / cellSize;


        if (!startText)
        {

            //räknar hur mycket tid som gått sedan senaste uppdateringen
            //omvandlar tiden som har gått till ett float och adderar det till moveTimer som håller reda på hur mycket tid som gått totalt
            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //om det har gått tillräckligt mycket tid, flytta ormen
            if (moveTimer >= currentMoveDelay && !gameOver)
            {
                moveTimer = 0f; //nollställ timern

                //beräkna nytt huvud
                Point head = snakePart[0];
                Point newHead = head + direction;

                if (newHead.X < 0 || newHead.X >= maxX ||
                    newHead.Y < 0 || newHead.Y >= maxY)
                {
                    // Om ormens huvud är utanför spelområdet så dör man, delat på 20 för att det är 20 pixlar per ruta och -1 för att 
                    gameOver = true;
                }
                else
                {
                    //lägger till det nya huvudet först i listan
                    snakePart.Insert(0, newHead);

                    //tar bort sista delen av ormen så det blir att den rör på sig
                    snakePart.RemoveAt(snakePart.Count - 1);
                }
            }
        }

        if (foodRNG == 1 && snakePart[0] == food)
        {
            gappleEaten = true;
        }
        else if (foodRNG != 1 && snakePart[0] == food)
        {
            gappleEaten = false;
        }



        //när ormens huvud är på samma position som äpplet
        if (snakePart[0] == food)
            {
                //lägg till en ny del i slutet av ormen (genom att duplicera den sista delen)
                snakePart.Add(snakePart[snakePart.Count - 1]);

                addPoint();

                foodRNG = random.Next(1, 20); //slumpar ett nummer mellan 1 och 20 (5% chans)

                if (foodRNG == 1) //om numret är 1 så spawnar det ett guldäpple istället som ger mer poäng
                {
                    SpawnGoldenFood();
                }
                else
                {
                    SpawnFood();
                }
            }


        currentMoveDelay = moveDelay;

        if (gappleEaten)
        {
            currentMoveDelay = gappleMoveDelay; //sätter ormens speed till guldäpplets speed när det är ätet
        }

        if (score > highScore && gameOver)
        {
            highScore = score;
            newHighScore = true;
        }


        for (int i = 0; i < snakePart.Count - 1; i++)
        {
            if (snakePart[0] == snakePart[i + 1])
            {
                gameOver = true;
            }
        }



        //gör att man kan röra sig snabbare när man håller mellanslag
        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            if (!gappleEaten)
            {
                currentMoveDelay = moveDelay / 3;
            }
            else
            {
                currentMoveDelay = gappleMoveDelay / 3;
            }
        }


        if (gameOver && Keyboard.GetState().IsKeyDown(Keys.R))
        {
            //återställ spelet
            snakePart.Clear();   //clearar alla delar av ormen och lägger till de som ska finnas från början igen
            snakePart.Add(new Point(5, 5));
            snakePart.Add(new Point(4, 5));
            snakePart.Add(new Point(3, 5));
            direction = new Point(1, 0);   //återställer riktningen
            score = 0;
            newHighScore = false;
            gameOver = false;
            gappleEaten = false;
            SpawnFood();
        }
        else if (startText && !showControls && Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            startText = false;
        }




        //gör att man kan toggla controls menyn i startmenyn
        if (startText && !gameOver && Keyboard.GetState().IsKeyDown(Keys.C))
        {
            showControls = true;
        }
        else if (startText && !gameOver && Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            showControls = false;
        }


        //gör så man kan röra ormen men inte i motsatt riktning som den redan går
        if (Keyboard.GetState().IsKeyDown(Keys.Up) && direction.Y != 1) direction = new Point(0, -1);
        if (Keyboard.GetState().IsKeyDown(Keys.Down) && direction.Y != -1) direction = new Point(0, 1);
        if (Keyboard.GetState().IsKeyDown(Keys.Left) && direction.X != 1) direction = new Point(-1, 0);
        if (Keyboard.GetState().IsKeyDown(Keys.Right) && direction.X != -1) direction = new Point(1, 0);


        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.ForestGreen);

        _spriteBatch.Begin();

        //visar starttexten om det är första rundan och spelet inte är startat
        //annars rita allt annat normalt
        if (!gameOver && startText && !showControls)
        {
            string bigStartText = "Welcome to snake!";
            Vector2 textSizeBig = fontBig.MeasureString(bigStartText);
            Vector2 positionBig = new Vector2((_graphics.PreferredBackBufferWidth - textSizeBig.X) / 2, (_graphics.PreferredBackBufferHeight - textSizeBig.Y) / 2 - 300);
            _spriteBatch.DrawString(fontBig, bigStartText, positionBig, Color.White);

            string startText = "Press space to start";
            Vector2 textSize = font.MeasureString(startText);
            Vector2 position = new Vector2((_graphics.PreferredBackBufferWidth - textSize.X) / 2, (_graphics.PreferredBackBufferHeight - textSize.Y) / 2 - 200);
            _spriteBatch.DrawString(font, startText, position, Color.White);

            string startText2 = "Press C to view the controls";
            Vector2 textSize2 = font.MeasureString(startText2);
            Vector2 position2 = new Vector2((_graphics.PreferredBackBufferWidth - textSize2.X) / 2, (_graphics.PreferredBackBufferHeight - textSize2.Y) / 2 - 150);
            _spriteBatch.DrawString(font, startText2, position2, Color.White);

        }

        //ritar controls texten om man är i startmenyn och klickar ner C, och ritar en svart bakgrund så det är lättare att läsa
        if (showControls && !gameOver && startText)
        {
            _spriteBatch.Draw(pixel, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.DarkGreen);

            string controlsTitle = "Controls: ";
            Vector2 textSizeTitle = fontBig.MeasureString(controlsTitle);
            Vector2 positionTitle = new Vector2((_graphics.PreferredBackBufferWidth - textSizeTitle.X) / 2, (_graphics.PreferredBackBufferHeight - textSizeTitle.Y) / 2 - 300);


            _spriteBatch.DrawString(fontBig, controlsTitle, positionTitle, Color.White);

            _spriteBatch.DrawString(font, "Use the arrow keys to control the direction", new Vector2(150, (_graphics.PreferredBackBufferHeight - textSizeTitle.Y) / 2 - 200), Color.White);
            _spriteBatch.DrawString(font, "Hold space to speed up", new Vector2(150, (_graphics.PreferredBackBufferHeight - textSizeTitle.Y) / 2 - 150), Color.White);
            _spriteBatch.DrawString(font, "Press ESC to go back to the main menu", new Vector2(150, (_graphics.PreferredBackBufferHeight - textSizeTitle.Y) / 2 - 100), Color.White);
        }

        else if (!startText)
        {
            //bakgrunden
            _spriteBatch.Draw(pixel, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.Green);

            // Rita vertikala linjer för rutnät
            for (int i = 0; i <= maxX; i++)
            {
                _spriteBatch.Draw(pixel, new Rectangle(i * cellSize, 0, 1, _graphics.PreferredBackBufferHeight), Color.DarkGreen);
            }

            // Rita horisontella linjer för rutnät
            for (int i = 0; i <= maxY; i++)
            {
                _spriteBatch.Draw(pixel, new Rectangle(0, i * cellSize, _graphics.PreferredBackBufferWidth, 1), Color.DarkGreen);
            }


            //ritar en fyrkant för varje del av ormen i listan, varje är 20x20 pixlar och positionen är baserad på dess x och y koordinater i listan
            foreach (Point part in snakePart)
            {
                if (part == snakePart[0])
                {
                    _spriteBatch.Draw(pixel, new Rectangle(part.X * 20, part.Y * 20, 20, 20), Color.SkyBlue); //ritar huvudet i en annan färg så man ser vart man åker lättare
                }
                else
                {
                    _spriteBatch.Draw(pixel, new Rectangle(part.X * 20, part.Y * 20, 20, 20), Color.Blue);
                }
            }

            //spawnar äpplet på de slumpade koordinaterna, också 20x20 pixlar
            if (foodRNG == 1)
            {
                _spriteBatch.Draw(pixel, new Rectangle(food.X * 20, food.Y * 20, 20, 20), Color.Gold);

    	        string gappleText = "Golden apple spawned! Eat it for a speed boost";
                Vector2 textSizeGapple = font.MeasureString(gappleText);
                Vector2 positionGapple = new Vector2((_graphics.PreferredBackBufferWidth - textSizeGapple.X) / 2, (_graphics.PreferredBackBufferHeight - textSizeGapple.Y) / 2 - 300);

                _spriteBatch.DrawString(font, gappleText, positionGapple, Color.Gold);
            }
            else
            {
                _spriteBatch.Draw(pixel, new Rectangle(food.X * 20, food.Y * 20, 20, 20), Color.Red);
            }

            _spriteBatch.DrawString(font, "Score: " + score, new Vector2(20, 20), Color.White);

        }

        //game over texten som ritas när gameOver är sann
        if (gameOver)
        {
            
            string gameOverText = "Game over!";
            Vector2 textSize = fontBig.MeasureString(gameOverText);
            Vector2 position = new Vector2((_graphics.PreferredBackBufferWidth - textSize.X) / 2, (_graphics.PreferredBackBufferHeight - textSize.Y) / 2 - 200);    
            _spriteBatch.DrawString(fontBig, gameOverText, position, Color.White); 

            if (newHighScore)
            {
                string highScoreNewText = "New high score: " + highScore;
                Vector2 highScoreNewSize = font.MeasureString(highScoreNewText);
                Vector2 highScoreNewPosition = new Vector2((_graphics.PreferredBackBufferWidth - highScoreNewSize.X) / 2, (_graphics.PreferredBackBufferHeight - highScoreNewSize.Y) / 2 - 130);
                _spriteBatch.DrawString(font, highScoreNewText, highScoreNewPosition, Color.White);
            }
            else if (!newHighScore && score != 0)
            {
                string highScoreText = "High score: " + highScore;
                Vector2 highScoreSize = font.MeasureString(highScoreText);
                Vector2 highScorePosition = new Vector2((_graphics.PreferredBackBufferWidth - highScoreSize.X) / 2, (_graphics.PreferredBackBufferHeight - highScoreSize.Y) / 2 - 130);
                _spriteBatch.DrawString(font, highScoreText, highScorePosition, Color.White);
            }
        }

        _spriteBatch.End();


        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
