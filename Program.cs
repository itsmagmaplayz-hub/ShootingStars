using ShootingStars;
using ShootingStars.UI;
using System.Windows;

namespace ShootingStars
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            App app = new App();
            // explicitly create the window from the UI namespace
            UI.GameWindow window = new UI.GameWindow();
            app.Run(window);
        }
    }
}

