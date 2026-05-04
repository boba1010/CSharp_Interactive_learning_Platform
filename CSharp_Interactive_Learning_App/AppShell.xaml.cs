using CSharp_Interactive_Learning_App.Views.UserViews;
using CSharp_Interactive_Learning_App.Views;

namespace CSharp_Interactive_Learning_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Login", typeof(LoginPage));
            Routing.RegisterRoute("Signup", typeof(SignupPage));
            Routing.RegisterRoute("Home", typeof(HomePage));
            Routing.RegisterRoute("Profile", typeof(ProfilePage));
            Routing.RegisterRoute("Leaderboard", typeof(LeaderboardPage));
            Routing.RegisterRoute("PlayGround", typeof(PlayGroundPage));
            Routing.RegisterRoute("Challenges", typeof(ChallengesPage));
            Routing.RegisterRoute("Battle", typeof(BattlePage));
        }
    }
}
