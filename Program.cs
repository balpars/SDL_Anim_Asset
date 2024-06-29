using System;
using SDL2;
using static System.Net.Mime.MediaTypeNames;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace PongGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                Error("Init failed");
                return;
            }

            // Initialize SDL_ttf
            if (TTF_Init() < 0)
            {
                Error("SDL_ttf initialization failed");
                SDL_Quit();
                return;
            }

            var window = SDL_CreateWindow("Pong",
                SDL_WINDOWPOS_UNDEFINED,
                SDL_WINDOWPOS_UNDEFINED,
                800,
                600,
                SDL_WindowFlags.SDL_WINDOW_SHOWN);

            SDL_SetWindowFullscreen(window, (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);

            if (window == null)
            {
                Error("Window Creation Failed");
                return;
            }

            var renderer = SDL_CreateRenderer(window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            SDL_SetRenderDrawColor(renderer, 0, 0, 50, 255); // Arka plan rengini koyu mavi yapalım (RGB: 0, 0, 50)
            SDL_RenderClear(renderer);

            MainMenu menu = new MainMenu(renderer);
            menu.ShowMenu();

            SDL_Quit();
            return;
        }

        private static void Error(string v)
        {
            Console.WriteLine($"Error: {v} SDL_Error:{SDL_GetError()}");
        }
    }

    internal class MainMenu
    {
        private IntPtr renderer;

        public MainMenu(IntPtr renderer)
        {
            this.renderer = renderer;
        }

        public void ShowMenu()
        {
            // Ana menüyü oluştur
            SDL_Color textColor = new SDL_Color { r = 255, g = 255, b = 255 };
            var font = TTF_OpenFont("arial.ttf", 24);
            if (font == IntPtr.Zero)
            {
                //Program.Error("Failed to load font");
                return;
            }

            // Oyuna hoş geldiniz mesajı
            
            
            var welcomeSurface = TTF_RenderText_Solid(font, "PONG GAME", textColor);
            var welcomeTexture = SDL_CreateTextureFromSurface(renderer, welcomeSurface);
            var welcomeRect = new SDL_Rect { x = 400, y = 300, w = 400, h = 100 };
            SDL_RenderCopy(renderer, welcomeTexture, IntPtr.Zero, ref welcomeRect);
            SDL_RenderPresent(renderer);

            SDL_Delay(1000); // 2 saniye bekleyelim

            SDL_SetRenderDrawColor(renderer, 0, 0, 50, 255);
            SDL_RenderClear(renderer);

            var playWithFriendSurface = TTF_RenderText_Solid(font, "Play with a friend", textColor);
            var playWithComputerSurface = TTF_RenderText_Solid(font, "Play with computer", textColor);
            var settingsSurface = TTF_RenderText_Solid(font, "Settings", textColor);

            var playWithFriendTexture = SDL_CreateTextureFromSurface(renderer, playWithFriendSurface);
            var playWithComputerTexture = SDL_CreateTextureFromSurface(renderer, playWithComputerSurface);
            var settingsTexture = SDL_CreateTextureFromSurface(renderer, settingsSurface);

            var playWithFriendRect = new SDL_Rect { x = 200, y = 200, w = 700, h = 75 };
            var playWithComputerRect = new SDL_Rect { x = 200, y = 350, w = 700, h = 75 };
            var settingsRect = new SDL_Rect { x = 200, y = 500, w = 300, h = 75 };

            SDL_RenderCopy(renderer, playWithFriendTexture, IntPtr.Zero, ref playWithFriendRect);
            SDL_RenderCopy(renderer, playWithComputerTexture, IntPtr.Zero, ref playWithComputerRect);
            SDL_RenderCopy(renderer, settingsTexture, IntPtr.Zero, ref settingsRect);

            SDL_RenderPresent(renderer);

            bool quit = false;
            while (!quit)
            {
                SDL_Event e;
                while (SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL_EventType.SDL_QUIT)
                    {
                        quit = true;
                        break;
                    }
                    else if (e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
                    {
                        int x, y;
                        SDL_GetMouseState(out x, out y);
                        if (x >= playWithFriendRect.x && x < playWithFriendRect.x + playWithFriendRect.w &&
                            y >= playWithFriendRect.y && y < playWithFriendRect.y + playWithFriendRect.h)
                        {
                            StartGameWithFriend.Start(renderer);
                            break;
                        }
                        else if (x >= playWithComputerRect.x && x < playWithComputerRect.x + playWithComputerRect.w &&
                                 y >= playWithComputerRect.y && y < playWithComputerRect.y + playWithComputerRect.h)
                        {
                            Console.WriteLine("Starting game with computer!");
                            // "Play with computer" seçeneği seçildiğinde yapılacak işlemler
                        }
                        else if (x >= settingsRect.x && x < settingsRect.x + settingsRect.w &&
                                 y >= settingsRect.y && y < settingsRect.y + settingsRect.h)
                        {
                            Console.WriteLine("Opening settings!");
                            // "Settings" seçeneği seçildiğinde yapılacak işlemler
                        }
                    }
                }
            }

            // Texture'ları temizle
            SDL_DestroyTexture(playWithFriendTexture);
            SDL_DestroyTexture(playWithComputerTexture);
            SDL_DestroyTexture(settingsTexture);

            // Yüzeyleri temizle
            SDL_FreeSurface(playWithFriendSurface);
            SDL_FreeSurface(playWithComputerSurface);
            SDL_FreeSurface(settingsSurface);

            TTF_CloseFont(font);
        }
    }

    internal class StartGameWithFriend
    {
        public static void Start(IntPtr renderer)
        {
            // Top (ball) oluşturma
            var ballRect = new SDL_Rect { x = 400, y = 300, w = 20, h = 20 }; // Topun boyutunu ve konumunu belirleyelim
            float ballSpeedX = 2.0f; // Topun X eksenindeki hızı
            float ballSpeedY = 2.0f; // Topun Y eksenindeki hızı
            SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255); // Topun rengini beyaz yapalım
            SDL_RenderFillRect(renderer, ref ballRect); // Topu çizelim

            // Çubukların başlangıç konumları
            var leftPaddleRect = new SDL_Rect { x = 50, y = 250, w = 20, h = 100 };
            var rightPaddleRect = new SDL_Rect { x = 750, y = 250, w = 20, h = 100 };

            SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255); // Çubuk rengini beyaz yapalım
            SDL_RenderFillRect(renderer, ref leftPaddleRect); // Sol çubuğu çizelim
            SDL_RenderFillRect(renderer, ref rightPaddleRect); // Sağ çubuğu çizelim

            // Oyuncu skorları
            int player1Score = 0;
            int player2Score = 0;

            float speedIncrease = 0.2f;

            SDL_Color textColor2 = new SDL_Color { r = 255, g = 255, b = 255 };
            IntPtr font = TTF_OpenFont("arial.ttf", 24); // varsayılan bir TrueType font dosyası kullanıyoruz

            SDL_RenderPresent(renderer);

            bool quit = false;
            // Tuşları takip etmek için bir Dictionary oluştur
            Dictionary<SDL_Keycode, bool> keyStates = new Dictionary<SDL_Keycode, bool>();


            while (!quit)
            {
                SDL_Event e;
                while (SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL_EventType.SDL_QUIT || (e.type == SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE))
                    {
                        quit = true;
                        break;
                    }

                    // Tuş durumlarını güncelle
                    if (e.type == SDL_EventType.SDL_KEYDOWN)
                    {
                        keyStates[e.key.keysym.sym] = true;
                    }
                    else if (e.type == SDL_EventType.SDL_KEYUP)
                    {
                        keyStates[e.key.keysym.sym] = false;
                    }
                }
                SDL_Color textColor = new SDL_Color { r = 255, g = 255, b = 255 };
                // Check for winner
                if (player1Score >= 2 || player2Score >= 2)
                {
                    string winner = player1Score >= 10 ? "1" : "2";
                    var winSurface = TTF_RenderText_Solid(font, $"Player {winner} has won! Press enter to start again.", textColor);
                    var winTexture = SDL_CreateTextureFromSurface(renderer, winSurface);
                    var winRect = new SDL_Rect { x = 100, y = 150, w = 600, h = 50 };  // Adjust the position and size as necessary
                    SDL_RenderCopy(renderer, winTexture, IntPtr.Zero, ref winRect);
                    SDL_RenderPresent(renderer);

                    // Klavye tuşlarının durumunu sıfırla
                    keyStates.Clear();

                    // Diğer değişkenleri sıfırla veya yeniden başlat
                    player1Score = 0;
                    player2Score = 0;
                    ballRect.x = 400;
                    ballRect.y = 300;
                    leftPaddleRect.y = 250;
                    rightPaddleRect.y = 250;

                    // Wait for Enter to be pressed
                    bool restart = false;
                    while (!restart)
                    {
                        SDL_Event restartEvent;
                        while (SDL_PollEvent(out restartEvent) != 0)
                        {
                            if (restartEvent.type == SDL_EventType.SDL_KEYDOWN)
                            {
                                if (restartEvent.key.keysym.sym == SDL_Keycode.SDLK_RETURN)
                                {
                                    restart = true;
                                }
                            }
                            if (restartEvent.type == SDL_EventType.SDL_QUIT ||
                                (restartEvent.type == SDL_EventType.SDL_KEYDOWN && restartEvent.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE))
                            {
                                quit = true;
                                restart = true; // Exit both loops
                            }
                        }
                    }

                    // Restart the game
                    if (!quit)
                    {
                        player1Score = 0;
                        player2Score = 0;
                        ballRect.x = 400;
                        ballRect.y = 300;
                        leftPaddleRect.y = 250;
                        rightPaddleRect.y = 250;
                    }
                }


                // Sol çubuğu hareket ettirme (W ve S tuşları)
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_w) && keyStates[SDL_Keycode.SDLK_w])
                {
                    if (leftPaddleRect.y > 0)
                        leftPaddleRect.y -= 10;
                }
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_s) && keyStates[SDL_Keycode.SDLK_s])
                {
                    if (leftPaddleRect.y < 600 - leftPaddleRect.h)
                        leftPaddleRect.y += 10;
                }

                // Sağ çubuğu hareket ettirme (Yukarı ve Aşağı ok tuşları)
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_UP) && keyStates[SDL_Keycode.SDLK_UP])
                {
                    if (rightPaddleRect.y > 0)
                        rightPaddleRect.y -= 10;
                }
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_DOWN) && keyStates[SDL_Keycode.SDLK_DOWN])
                {
                    if (rightPaddleRect.y < 600 - rightPaddleRect.h)
                        rightPaddleRect.y += 10;
                }

                // Topun hareketini güncelleme
                ballRect.x += (int)ballSpeedX;
                ballRect.y += (int)ballSpeedY;

     

                // Topun çubuklara çarpma kontrolü
                if (ballRect.y <= 0 || ballRect.y >= 600 - ballRect.h)
                    ballSpeedY = -ballSpeedY; // Y ekseninde yansıma

                // Sol çubuğa çarpma kontrolü
                if (ballSpeedX < 0 && // Top sola doğru hareket ediyorsa
                    ballRect.x <= leftPaddleRect.x + leftPaddleRect.w && // Topun sağ kenarı, sol çubuğun sağ kenarından küçük veya eşitse
                    ballRect.x >= leftPaddleRect.x && // Topun sol kenarı, sol çubuğun sol kenarından büyük veya eşitse
                    ballRect.y + ballRect.h >= leftPaddleRect.y && // Topun alt kenarı, sol çubuğun üst kenarından büyük veya eşitse
                    ballRect.y <= leftPaddleRect.y + leftPaddleRect.h)
                { // Topun üst kenarı, sol çubuğun alt kenarından küçük veya eşitse
                    ballSpeedX -= speedIncrease;
                    ballSpeedX = -ballSpeedX; // Sol çubuğa çarpma               
                    ballRect.x -= (int)ballSpeedX;

                    if (ballSpeedY > 0)
                    {
                        ballSpeedY += speedIncrease;
                        ballRect.y += (int)ballSpeedY;
                    }
                    else
                    {
                        ballSpeedY -= speedIncrease;
                        ballRect.y -= (int)ballSpeedY;
                    }
                    Console.WriteLine("SOLballSpeedX: " + ballSpeedX + " SOLballSpeedY:" + +ballSpeedY);
                    Console.WriteLine("SOLspeedIncrease: " + speedIncrease);

                }

                // Sağ çubuğa çarpma kontrolü
                if (ballSpeedX > 0 && // Top sağa doğru hareket ediyorsa
                    ballRect.x + ballRect.w >= rightPaddleRect.x && // Topun sol kenarı, sağ çubuğun sol kenarından büyük veya eşitse
                    ballRect.x + ballRect.w <= rightPaddleRect.x + rightPaddleRect.w && // Topun sol kenarı, sağ çubuğun sağ kenarından küçük veya eşitse
                    ballRect.y + ballRect.h >= rightPaddleRect.y && // Topun alt kenarı, sağ çubuğun üst kenarından büyük veya eşitse
                    ballRect.y <= rightPaddleRect.y + rightPaddleRect.h)
                { // Topun üst kenarı, sağ çubuğun alt kenarından küçük veya eşitse
                    ballSpeedX += speedIncrease;
                    ballSpeedX = -ballSpeedX; // Sağ çubuğa çarpma
                    ballRect.x += (int)ballSpeedX;

                    if (ballSpeedY > 0)
                    {
                        ballSpeedY += speedIncrease;
                        ballRect.y += (int)ballSpeedY;
                    }
                    else
                    {
                        ballSpeedY -= speedIncrease;
                        ballRect.y -= (int)ballSpeedY;
                    }

                    Console.WriteLine("SAGballSpeedX: "+ballSpeedX + "SAGballSpeedY:" + +ballSpeedY);
                    Console.WriteLine("SAGspeedIncrease: " + speedIncrease);
                }
                // Topun ekran sınırlarına çarpma kontrolü
                if (ballRect.x <= 0)
                {
                    player2Score++; // Sağ oyuncu skorunu artır
                    ballRect.x = 400; // Topu başlangıç noktasına geri getir
                    ballRect.y = 300;
                }
                else if (ballRect.x >= 800 - ballRect.w)
                {
                    player1Score++; // Sol oyuncu skorunu artır
                    ballRect.x = 400; // Topu başlangıç noktasına geri getir
                    ballRect.y = 300;
                }

                SDL_SetRenderDrawColor(renderer, 0, 0, 50, 255);
                SDL_RenderClear(renderer);
                SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                SDL_RenderFillRect(renderer, ref leftPaddleRect);
                SDL_RenderFillRect(renderer, ref rightPaddleRect);
                SDL_RenderFillRect(renderer, ref ballRect); // Topu her döngüde çizelim
                

                // Skorları ekrana yazdırma
                var surface = TTF_RenderText_Solid(font, $"{player1Score} - {player2Score}", textColor); // Skorları birleştirip yazdır
                var scoreTexture = SDL_CreateTextureFromSurface(renderer, surface);
                var scoreRect = new SDL_Rect { x = 350, y = 50, w = 100, h = 100 }; // Ekranın ortasına yerleştir
                SDL_RenderCopy(renderer, scoreTexture, IntPtr.Zero, ref scoreRect);

                SDL_RenderPresent(renderer);
                SDL_Delay(10); // Topun hızını düşürmek için bir gecikme ekleyelim

            }

            SDL_Quit();
            return;
        }
    }
    
}
