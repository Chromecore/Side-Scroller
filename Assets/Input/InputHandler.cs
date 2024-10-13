namespace Chromecore
{
	public class InputHandler
	{
		private static InputHandler instance;
		public static InputHandler Instance
		{
			get
			{
				// create instance
				if (instance != null) return instance;
				instance = new InputHandler();

				// initialize input
				instance.InitializeInput();
				instance.EnableControlls();
				return instance;
			}
			private set { instance = value; }
		}

		public InputMaster inputMaster { get; private set; }
		public InputMaster.PlayerActions playerActions;

		public void InitializeInput()
		{
			inputMaster = new InputMaster();
			playerActions = inputMaster.Player;
		}

		public void EnableControlls()
		{
			if (inputMaster == null) return;
			inputMaster.Enable();
		}

		public void DisableControlls()
		{
			if (inputMaster == null) return;
			inputMaster.Disable();
		}
	}
}