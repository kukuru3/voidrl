namespace Scanner.Impl.Responders {
    internal class MenuButton : ButtonResponder {
        protected override void OnButtonClicked() {
            GameController.LaunchMenu();
        }
    }
}
