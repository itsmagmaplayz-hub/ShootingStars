using ShootingStars;
using System.Windows;

namespace ShootingStars
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            App app = new App();
            GameWindow window = new GameWindow();
            app.Run(window);
        }
    }
}

