/*******************************************************************************
*
* @LampGuitar clone made for educational purposes
* using meta.ai for the parts that i dont understand, to
* try and learn more about the c# language and implementing
* separate ideas and gamedev concepts into my existing space invaders
* clone.
*
******************************************************************************/

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LampGuitar.SInvadersClone;
public enum GameState
    {
        Title,
        Game,
        GameOver
    }
public class MainRaylibGame
{
    public GameState currentState;
    public required BasicEnemy[] enemies;
    public required PlayerBullet[] playerBullets;
    const int maxBullets = 4;
    const float bulletRadius = 10;//for collision purposes
    public const int MaxFrameSpeed = 15;
    public const int MinFrameSpeed = 1;
    public const float playerMoveSpeed = 10f;
    const float playerRadius = 25;
    const int maxEnemies = 24;
    const float enemyRadius = 25;//for collision purposes
    const float enemyBulletRadius = 20;//for collision
    public static Color palette_color_1 = new Color(33, 118, 204, 255);//Blue
    public static Color palette_color_2 = new Color(255, 125, 110, 255);//Red
    public static Color palette_color_3 = new Color(252, 166, 172, 255);//Pink for shadows
    public static Color palette_color_4 = new Color(232, 231, 203, 255);//Cream-White
    public static Color palette_color_debug = new Color(204, 133, 33, 255);//Cream-White
    public static int Main()
    {
        // Initialization
        //---------------------------------------------------------------------
        const int screenWidth = 1280;
        const int screenHeight = 720;
        bool isDebug = false;

        //setting the current state
        GameState currentState = GameState.Title;
        InitWindow(screenWidth, screenHeight, "@lampGuitar - Space Invaders Clone");

        // NOTE: Textures MUST be loaded after Window initialization (OpenGL context is required)
        Texture2D scarfy = LoadTexture("resources/scarfy.png");
        Texture2D player_graphic = LoadTexture("resources/player_ship.png");
        Texture2D enemy_graphic = LoadTexture("resources/enemy_ship.png");
        Texture2D player_bullet_graphic = LoadTexture("resources/player_bullet.png");
        Texture2D enemy_bullet_graphic = LoadTexture("resources/enemy_bullet.png");
        Texture2D backgroundTexture;
        float backgroundScrollX;
        float backgroundScrollY;
        float backgroundScrollSpeed;

        // TTF font : Font data and atlas are generated directly from TTF
        // NOTE: We define a font base size of 32 pixels tall and up-to 250 characters
        Font fontTtf = LoadFontEx("resources/PeaberryBase.ttf", 32, null, 250);

        float scaleX = 1.0f;
        float scaleY = 1.0f;
        float targetScaleX = 1.0f;
        float targetScaleY = 1.0f;
        float scaleSpeed = 30.0f;
        Vector2 position = new(550.0f, 590.0f); //player position
        int score = 0; 
        
        Rectangle frameRec = new(0.0f, 0.0f, (float)scarfy.Width / 6, (float)scarfy.Height); 
        
        int currentFrame = 0;
        int framesCounter = 0;
        // Number of spritesheet frames shown by second
        int framesSpeed = 8;

        float shadow_offset = 5.0f; // for drawing the sprites shadow
        bool goinDown = true;
        
        //initialize enemy bullets
        const int maxEnemyBullets = 7;
        EnemyBullet[] enemyBullets = new EnemyBullet[maxEnemyBullets];
        resetEnemyBullets();

        //----- Initialize enemy array
        BasicEnemy[] enemies = new BasicEnemy[maxEnemies];
        initializeEnemies(enemies);
        
        //Initializing player bullets
        PlayerBullet[] current_PlayerBullets = new PlayerBullet[maxBullets];
        for (int i=0; i<maxBullets; i++){//setting all the bullets to not active
            current_PlayerBullets[i].isActive = false;
        }
        
        //init bg stuff
        backgroundTexture = LoadTexture("resources/scrolling_background.png");
        backgroundScrollX = 0.0f;
        backgroundScrollY = 0.0f;
        backgroundScrollSpeed = 33.0f;

        SetTargetFPS(60);
        //-----
        // Main game loop
        while (!WindowShouldClose())
        {
            //manage debug state, Backspace to toggle
            if(IsKeyPressed(KeyboardKey.Backspace)){
                isDebug = !isDebug;
            }

            //player graphics scaling
            // Interpolate scale
            scaleX = MathHelper.Lerp(scaleX, targetScaleX, scaleSpeed * GetFrameTime());
            scaleY = MathHelper.Lerp(scaleY, targetScaleY, scaleSpeed * GetFrameTime());

            // Reset target scale when shooting is done
            if (scaleX >= targetScaleX - 0.01f && targetScaleX > 1.0f)
            {
                targetScaleX = 1.0f;
                targetScaleY = 1.0f;
            }

            updateBackground(); //scrolling background
            manageGameState(); //State machine

            //------------------------------------------------------------
            // Draw
            //------------------------------------------------------------
            BeginDrawing(); //Raylibs method
            ClearBackground(palette_color_4);//bg color

            drawBackground();
            drawGameState();
            drawDebugStuff();
            
            DrawTextEx(fontTtf,
                        "@LampGuitar built with Raylib/C#",
                        new Vector2(screenHeight - 24, 24),
                        16, 2, palette_color_1);
            DrawTextEx(fontTtf,
                        "Score \n \n" + (score),
                        new Vector2(10, 10),
                        48, 4, palette_color_1);

            EndDrawing();
            //-----------------------------------------------------------
        }

        

        void resetEnemyBullets(){
            for (int i=0; i<maxEnemyBullets; i++){
                enemyBullets[i].isActive=false;
                enemyBullets[i].position.X = 0f;
                enemyBullets[i].position.Y = 0f;
            }
        }

        void resetPlayerBullets(PlayerBullet[] player_bullets){
            for (int i=0; i<maxBullets; i++){
                player_bullets[i].isActive = false;
                player_bullets[i].position = new Vector2(0,0);
            }
        }
        
        void drawGameState(){
            switch(currentState){
                case GameState.Title:
                    DrawTextEx(fontTtf,
                        "Title state \n \n press <ENTER> to begin.",
                        new Vector2(screenWidth/4, screenHeight/2),
                        32, 2, palette_color_1);
                    break;
                case GameState.Game:
                    
                    //DrawTextureRec(scarfy, frameRec, position, Color.White);
                    drawEnemies(enemies, enemy_graphic);
                    drawPlayerBullets(current_PlayerBullets, player_bullet_graphic);
                    drawEnemyBullets();

                    drawPlayer();
                    break;
                case GameState.GameOver:
                    DrawTextEx(fontTtf,
                        "Game Over state \n \n press <ENTER> to go to the Title Screen.",
                        new Vector2(screenWidth/4, screenHeight/2),
                        32, 2, palette_color_1);

                    break;
            }
        }

        void manageGameState()
        {
            switch(currentState)
            {
                case GameState.Title:
                    updateTitleState();
                    break;
                case GameState.Game:
                    updateGameState();
                    break;
                case GameState.GameOver:
                    updateGameOverState();
                    break;
            }
        }

        void updateTitleState(){
            //what all do we need to check on the title state
            if (IsKeyPressed(KeyboardKey.Enter))
            {
                // make the state the main game.
                currentState = GameState.Game;
            }
        }

        void updateGameState(){
            //what all goes on in the main game state?
            getPlayerInput();
            //inside updateEnemies is when it checks to see if all enemies are defeated.
            updateEnemies(enemies);
            updatePlayerBullets(current_PlayerBullets);
            updateEnemyBullets();

            updateShadowOffset();
        }

        void updateBackground(){
            //background update stuff
            backgroundScrollX += backgroundScrollSpeed * GetFrameTime();
            backgroundScrollY += backgroundScrollSpeed * GetFrameTime();

            if (backgroundScrollX>backgroundTexture.Width){
                backgroundScrollX = 0.0f;
            }

            if (backgroundScrollY>backgroundTexture.Height){
                backgroundScrollY = 0.0f;
            }
        }

        void updateEnemyBullets(){
            for (int i=0; i<maxEnemyBullets; i++)
            {
                if (enemyBullets[i].isActive)
                {
                    //update the bullets position
                    enemyBullets[i].position += enemyBullets[i].velocity * GetFrameTime();

                    //check collision with player==================================================
                    float distance = (float)Math.Sqrt(
                        Math.Pow(enemyBullets[i].position.X - position.X, 2)
                        + Math.Pow(enemyBullets[i].position.Y - position.Y, 2));
                    //float distance = Vector2Distance(enemyBullets[i].position, position);
                    if (distance < enemyBulletRadius + playerRadius) //adjust collision radius as needed here m'boi
                    {
                        //collision detected
                        currentState = GameState.GameOver;
                    }

                    //check if the enemy bullet has gone far enough to get destroyed.
                    if (enemyBullets[i].position.Y > screenHeight)
                    {
                        enemyBullets[i].isActive = false;
                    }
                }
            }
        }

        void updateGameOverState(){
            //we are just waiting for the enter key to go back to the title state
            if (IsKeyPressed(KeyboardKey.Enter))
            {
                restartGame();
            }
        }

        void updateShadowOffset(){
            //so we need to keep moving the shadow towards a certain value
            float normalize_rate = 0.25f;
            float goal_offset = 18.0f;

            if (goinDown) {shadow_offset += normalize_rate;}
            else if (goinDown==false){shadow_offset -= normalize_rate;}

            //and clamp it there if at the 'right' level
            if ( shadow_offset >= goal_offset && goinDown==true){ 
                goinDown=false; }
            if (shadow_offset <= (goal_offset/3) && goinDown==false){
                goinDown=true;
            }
            
        }

        bool CheckCollision(BasicEnemy enemy, PlayerBullet bullet)
        {
            float distanceX = Math.Abs(enemy.position.X - bullet.position.X);
            float distanceY = Math.Abs(enemy.position.Y - bullet.position.Y);

            float distance = (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            return distance < (enemyRadius + bulletRadius);
        }

        void initializeEnemies(BasicEnemy[] enemies)
        {
            int rows = 3;
            int collumns = 8;
            float x_spawn_offset = 120f;
            float y_spawn_offset = 100f;
            int sprite_width = enemy_graphic.Width;
            int sprite_height = enemy_graphic.Height;

            int enemyIndex = 0;

            for(int row=0; row<rows; row++)
            {
                for (int column=0; column<collumns; column++)
                {
                    enemies[enemyIndex].position = new Vector2(
                        x_spawn_offset + (column * sprite_width),
                        y_spawn_offset + (row * sprite_height));
                    enemies[enemyIndex].velocity = new Vector2(2, 0);
                    enemies[enemyIndex].speed = 80.0f;
                    enemies[enemyIndex].isAlive = true;
                    enemies[enemyIndex].facing_right = true;

                    enemyIndex ++;
                }
            }
            /*
            for (int i=0; i < enemies.Length; i++)
            {
                float x_spawn_offset = 120.0f; //spawn enemies starting from this x point.
                int sprite_width = enemy_graphic.Width; //for spacing purposes.

                enemies[i].position = new Vector2(x_spawn_offset + i*sprite_width, 100);
                enemies[i].velocity = new Vector2(2, 0);
                enemies[i].speed = 80.0f;
                enemies[i].isAlive = true;
                enemies[i].facing_right = true;
            }
            */
        }

        void shootEnemyBullet(Vector2 spawn_position){
            for (int i=0; i<maxEnemyBullets; i++)
            {
                if (!enemyBullets[i].isActive)
                {
                    enemyBullets[i].position = spawn_position;
                    enemyBullets[i].velocity = new Vector2(0, 300); //500 is speed
                    enemyBullets[i].isActive = true;
                    break;
                }
            }
        }

        void updateEnemies(BasicEnemy[] enemies)
        {
            bool allEnemiesDead = true;
            for (int i=0; i<enemies.Length; i++)//grab all the enemies
            {
                if (enemies[i].isAlive)//if the boy is alive
                {
                    //randomly shoot bullets
                    int chance_toShoot = GetRandomValue(0, 100);
                    if (chance_toShoot < 1){ //5% chance
                        shootEnemyBullet(enemies[i].position);
                    }

                    enemies[i].position += enemies[i].velocity * enemies[i].speed * GetFrameTime();

                    if (enemies[i].position.X > screenWidth && enemies[i].facing_right == true)//enemy has reached end of the screen and is facing to the right
                    {
                        //we need to flip the x velocity
                        enemies[i].velocity *= -1.0f;
                        //and then shift the enemy down a 'row'
                        enemies[i].position.Y += (enemy_graphic.Height/2);
                        enemies[i].facing_right = false; //we facing left now
                    }
                    else if (enemies[i].position.X < 0 && enemies[i].facing_right == false)//reached the left edge of the screen and we facing left
                    {
                        //we need to flip the x velocity
                        enemies[i].velocity *= -1.0f;
                        //and then shift the enemy down a 'row'
                        enemies[i].position.Y += (enemy_graphic.Height/2);
                        enemies[i].facing_right = true; //we facing right now
                    }

                    allEnemiesDead = false;
                }
            }
            
            if (allEnemiesDead)
            {
                restartGame();
            }
        }

        void restartGame(){
            position = new(550.0f, 590.0f); //player position
            score = 0;
            initializeEnemies(enemies);
            resetEnemyBullets();
            resetPlayerBullets(current_PlayerBullets);
            currentState = GameState.Title;
        }

        void updatePlayerBullets(PlayerBullet[] player_bullets){
            for (int i=0; i<maxBullets; i++)
            {
                if (player_bullets[i].isActive)
                {
                    player_bullets[i].position += player_bullets[i].velocity * player_bullets[i].speed * GetFrameTime();
                    //Check for collisions with enemies
                    for (int j=0; j<maxEnemies; j++)
                    {
                        if (enemies[j].isAlive)
                        {
                            if (CheckCollision(enemies[j], current_PlayerBullets[i]))
                            {
                                //Handle the collision, kill enemy and bullet
                                enemies[j].isAlive = false;
                                current_PlayerBullets[i].isActive = false;
                                score += 15;
                            }
                        }
                    }

                
                    //lets check if bullet reaches the end of screen
                    if (player_bullets[i].position.Y < 0){
                        //we then deactivate the bullet for reuse.
                        player_bullets[i].isActive = false;
                    }
                }
            }
        }

        void getPlayerInput(){
            if (IsKeyDown(KeyboardKey.D)) {
                position.X += playerMoveSpeed;
            }else if (IsKeyDown(KeyboardKey.A)) {
                position.X -= playerMoveSpeed;
            }

            if (IsKeyPressed(KeyboardKey.Space)){
                shootPlayerBullet();
            }
        }
        
        void shootPlayerBullet(){
            for (int i=0; i<maxBullets; i++)
            {
                if (!current_PlayerBullets[i].isActive){
                    current_PlayerBullets[i].position = position;
                    current_PlayerBullets[i].velocity = new Vector2(0, -1);
                    current_PlayerBullets[i].speed = 500.0f;
                    current_PlayerBullets[i].isActive = true;

                    targetScaleX = 1.5f; // Scale up to w/e
                    targetScaleY = 0.7f; // Scale up to w/e
                    break;
                }
            }
        }

        void drawBackground(){
            //drawing background stuff
            for (int x = -backgroundTexture.Width; x < screenWidth; x += backgroundTexture.Width){
                for (int y = -backgroundTexture.Height; y < screenHeight; y += backgroundTexture.Height){
                    DrawTextureV(backgroundTexture, new Vector2(x + backgroundScrollX, y + backgroundScrollY), new Color(33, 118, 204, 50));//blue color but half alpha'd
                }
            }
        }

        void drawPlayer(){
            // Draw part of the texture
            int positionX = (int)position.X;
            int positionY = (int)position.Y;

            //DrawTexture(player_graphic,
            //    positionX - (player_graphic.Width/2)+(int)shadow_offset,
            //    positionY - (player_graphic.Height/2)+(int)shadow_offset, palette_color_3); //pink
            
            Rectangle playerSourceRect = new Rectangle(0, 0, player_graphic.Width, player_graphic.Height);
            Rectangle playerDestRect = new Rectangle(
                position.X - (int)(player_graphic.Width * scaleX / 2), 
                position.Y - (int)(player_graphic.Height * scaleY / 2), 
                (int)(player_graphic.Width * scaleX), 
                (int)(player_graphic.Height * scaleY));
            Rectangle playerShadowDestRect = new Rectangle(
                (position.X + shadow_offset)- (int)(player_graphic.Width * scaleX / 2), 
                (position.Y + shadow_offset) - (int)(player_graphic.Height * scaleY / 2), 
                (int)(player_graphic.Width * scaleX), 
                (int)(player_graphic.Height * scaleY));

            //Shadow Graphic
            DrawTexturePro(player_graphic, playerSourceRect, playerShadowDestRect, new Vector2(0, 0), 0.0f, palette_color_3);

            //Main Graphic
            DrawTexturePro(player_graphic, playerSourceRect, playerDestRect, new Vector2(0, 0), 0.0f, palette_color_1);


            //Debug Collision circle for player
            if(isDebug) { DrawCircleV(position, playerRadius, palette_color_debug); }
        }

        void drawPlayerBullets(PlayerBullet[] current_PlayerBullets, Texture2D bullet_graphic){
            for (int i=0; i<current_PlayerBullets.Length; i++)
            {
                if (current_PlayerBullets[i].isActive)
                {
                    int positionX = (int)current_PlayerBullets[i].position.X;
                    int positionY = (int)current_PlayerBullets[i].position.Y;
                    //draw the shadow first
                    DrawTexture(bullet_graphic,
                        positionX-(bullet_graphic.Width/2)+(int)shadow_offset,
                        positionY-(bullet_graphic.Height/2)+(int)shadow_offset, palette_color_3);//pink
                    //then draw the bullet
                    DrawTexture(bullet_graphic,
                        positionX-(bullet_graphic.Width/2),
                        positionY-(bullet_graphic.Height/2), palette_color_1);//blue for player

                    if(isDebug) { DrawCircle(positionX, positionY, bulletRadius, palette_color_debug); }
                }
            }
        }

        void drawEnemyBullets(){
            for (int i=0; i<maxEnemyBullets; i++)
            {
                if (enemyBullets[i].isActive)
                {
                    int positionX = (int)enemyBullets[i].position.X;
                    int positionY = (int)enemyBullets[i].position.Y;
                    DrawTexture(enemy_bullet_graphic,
                        positionX-(enemy_bullet_graphic.Width/2)+(int)shadow_offset,
                        positionY-(enemy_bullet_graphic.Height/2)+(int)shadow_offset,
                        palette_color_3);//pink
                    //Actual bullet graphic
                    DrawTexture(enemy_bullet_graphic,
                        positionX-(enemy_bullet_graphic.Width/2),
                        positionY-(enemy_bullet_graphic.Height/2),
                        palette_color_2);//red

                    if(isDebug) { DrawCircleV(new Vector2(positionX, positionY), enemyBulletRadius, palette_color_debug); }
                }
            }
        }

        void drawEnemies(BasicEnemy[] enemies, Texture2D enemy_graphic)
        {
            for (int i=0; i<enemies.Length; i++)//grab all these bois
            {
                if (enemies[i].isAlive)
                {
                    int positionX = (int)enemies[i].position.X;
                    int positionY = (int)enemies[i].position.Y;
                    int shadowX = (int)enemies[i].position.X + (int)shadow_offset;
                    int shadowY = (int)enemies[i].position.Y + (int)shadow_offset;
                    //Drawing the shadow
                    //DrawTexture(enemy_graphic, positionX + shadow_offset, positionY + shadow_offset, Color.Black);
                    DrawTexture(enemy_graphic, 
                        shadowX - (enemy_graphic.Width/2), 
                        shadowY - (enemy_graphic.Height/2), palette_color_3);// pink

                    //Drawing the enemy graphic
                    DrawTexture(enemy_graphic, 
                        positionX-(enemy_graphic.Width/2),
                        positionY-(enemy_graphic.Height/2), palette_color_2);//red
                    
                    if(isDebug) { DrawCircle(positionX, positionY, enemyRadius, palette_color_debug); }
                }
            }
        }

        void drawDebugStuff(){
            int positionX = (int)position.X;
            int positionY = (int)position.Y;

            //then for some reason, im making the debug text a string, for easy formatting for me tbh.
            string debugText = "";
            debugText += "\n player x: " + position.X;
            debugText += "\n player y: " + position.Y;
            debugText += "\n shadow_offset: " + shadow_offset;
            debugText += "\n gamestate: " + currentState;
            if (isDebug){
                DrawTextEx(fontTtf,
                            debugText,
                            new Vector2(10, 100),
                            16, 5, palette_color_debug);
                //I wanna see the origin of the player
                DrawCircle(positionX, positionY, 3, palette_color_debug);
            }
        }
        
        void updateFrames(){
            //updating frames here
            framesCounter++;

            if (framesCounter >= (60 / framesSpeed))
            {
                framesCounter = 0;
                currentFrame++;

                if (currentFrame > 5)
                {
                    currentFrame = 0;
                }

                frameRec.X = (float)currentFrame * (float)scarfy.Width / 6;
            }

            framesSpeed = Math.Clamp(framesSpeed, MinFrameSpeed, MaxFrameSpeed);
        }
        
        // De-Initialization
        //-------------------------------------------------------------
        UnloadTexture(scarfy);
        UnloadTexture(enemy_graphic);
        UnloadTexture(player_graphic);
        UnloadTexture(player_bullet_graphic);
        UnloadTexture(enemy_bullet_graphic);
        UnloadTexture(backgroundTexture);
        UnloadFont(fontTtf);

        CloseWindow();
        //-------------------------------------------------------------

        return 0;
    }
}
