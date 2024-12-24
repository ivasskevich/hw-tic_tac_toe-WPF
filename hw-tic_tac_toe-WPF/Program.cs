namespace hw_tic_tac_toe_WPF
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var model = new TicTacToeModel();
            var view = new GameForm();
            var presenter = new TicTacToePresenter(view, model);

            Application.Run(view);
        }
    }
}