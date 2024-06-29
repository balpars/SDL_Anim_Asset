using System;
using SDL2;

namespace SpriteAnimation
{
    class Program
    {
        const int SPRITE_WIDTH = 93;  // Width of each sprite
        const int SPRITE_HEIGHT = 112; // Height of each sprite

        const int SPRITE_COUNT = 8;   // Number of sprites in the sheet
        const int SCREEN_WIDTH = 800;
        const int SCREEN_HEIGHT = 600;
        const int ANIMATION_SPEED = 100; // Time in milliseconds between frames
        const int MOVE_SPEED = 5; // Movement speed of the character

        static void Main(string[] args)
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("SDL could not initialize! SDL_Error: {0}", SDL.SDL_GetError());
                return;
            }

            IntPtr window = SDL.SDL_CreateWindow("Sprite Animation", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, SCREEN_WIDTH, SCREEN_HEIGHT, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (window == IntPtr.Zero)
            {
                Console.WriteLine("Window could not be created! SDL_Error: {0}", SDL.SDL_GetError());
                return;
            }

            IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine("Renderer could not be created! SDL_Error: {0}", SDL.SDL_GetError());
                return;
            }

            IntPtr image = SDL_image.IMG_LoadTexture(renderer, "RUN.png");
            if (image == IntPtr.Zero)
            {
                Console.WriteLine("Unable to load image! SDL_image Error: {0}", SDL.SDL_GetError());
                return;
            }

            SDL.SDL_Rect[] spriteClips = new SDL.SDL_Rect[SPRITE_COUNT];
            for (int i = 0; i < SPRITE_COUNT; ++i)
            {
                spriteClips[i].x = i * SPRITE_WIDTH;
                spriteClips[i].y = 0;
                spriteClips[i].w = SPRITE_WIDTH;
                spriteClips[i].h = SPRITE_HEIGHT;
            }

            int currentFrame = 0;
            bool quit = false;
            SDL.SDL_Event e;
            uint lastTime = SDL.SDL_GetTicks(), currentTime;

            int posX = (SCREEN_WIDTH - SPRITE_WIDTH) / 2;
            int posY = (SCREEN_HEIGHT - SPRITE_HEIGHT) / 2;

            while (!quit)
            {
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        quit = true;
                    }

                    if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                    {
                        switch (e.key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_UP:
                                posY -= MOVE_SPEED;
                                break;
                            case SDL.SDL_Keycode.SDLK_DOWN:
                                posY += MOVE_SPEED;
                                break;
                            case SDL.SDL_Keycode.SDLK_LEFT:
                                posX -= MOVE_SPEED;
                                break;
                            case SDL.SDL_Keycode.SDLK_RIGHT:
                                posX += MOVE_SPEED;
                                break;
                        }
                    }
                }

                currentTime = SDL.SDL_GetTicks();
                if (currentTime > lastTime + ANIMATION_SPEED)
                {
                    currentFrame = (currentFrame + 1) % SPRITE_COUNT;
                    lastTime = currentTime;
                }

                SDL.SDL_RenderClear(renderer);
                SDL.SDL_Rect renderQuad = new SDL.SDL_Rect { x = posX, y = posY, w = SPRITE_WIDTH, h = SPRITE_HEIGHT };
                SDL.SDL_RenderCopy(renderer, image, ref spriteClips[currentFrame], ref renderQuad);
                SDL.SDL_RenderPresent(renderer);
            }

            SDL.SDL_DestroyTexture(image);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
